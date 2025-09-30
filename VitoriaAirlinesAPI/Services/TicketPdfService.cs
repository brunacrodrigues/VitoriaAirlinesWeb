using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Drawing;
using System.Drawing.Imaging;
using VitoriaAirlinesWeb.Data.Entities;
using ZXing;
using ZXing.Common;

namespace VitoriaAirlinesAPI.Services
{
    public class TicketPdfService : ITicketPdfService
    {
        public async Task<byte[]> GenerateTicketPdfAsync(Ticket ticket)
        {
            return await Task.Run(() =>
            {
                var document = new TicketDocument(ticket);
                return document.GeneratePdf();
            });
        }
    }

    public class TicketDocument : IDocument
    {
        private readonly Ticket _ticket;

        public TicketDocument(Ticket ticket)
        {
            _ticket = ticket;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Size(PageSizes.A6.Landscape());
                    page.Margin(10, Unit.Millimetre);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(ComposeContent);
                    page.Footer().Element(ComposeFooter);
                });
        }

        void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Text("Vitoria Airlines Boarding Pass").Bold().FontSize(16);
                row.ConstantItem(100).Text($"TICKET NO: TKT{_ticket.Id:D8}");
            });
        }

        void ComposeContent(IContainer container)
        {
            container.PaddingVertical(1, Unit.Centimetre).Column(column =>
            {
                column.Spacing(10);

                // Passenger and Flight
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("PASSENGER").SemiBold().FontSize(8);
                        col.Item().Text(_ticket.User.FullName).FontSize(14).FontColor(Colors.Blue.Medium);
                    });
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("FLIGHT").SemiBold().FontSize(8);
                        col.Item().Text(_ticket.Flight.FlightNumber).FontSize(14).FontColor(Colors.Red.Medium);
                    });
                });

                // From and To
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("FROM / ORIGIN").SemiBold().FontSize(8);
                        col.Item().Text(_ticket.Flight.OriginAirport.IATA).Bold().FontSize(24);
                        col.Item().Text(_ticket.Flight.OriginAirport.FullName).FontSize(10);
                    });

                    row.ConstantItem(30).AlignMiddle().Text("→").FontSize(24).AlignCenter();

                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("TO / DESTINATION").SemiBold().FontSize(8);
                        col.Item().Text(_ticket.Flight.DestinationAirport.IATA).Bold().FontSize(24);
                        col.Item().Text(_ticket.Flight.DestinationAirport.FullName).FontSize(10);
                    });
                });

                // Details
                column.Item().Grid(grid =>
                {
                    grid.Columns(4);
                    grid.Item().Text("DEPARTURE").SemiBold().FontSize(8);
                    grid.Item().Text("SEAT").SemiBold().FontSize(8);
                    grid.Item().Text("CLASS").SemiBold().FontSize(8);
                    grid.Item().Text("GATE").SemiBold().FontSize(8);

                    grid.Item().Text($"{_ticket.Flight.DepartureUtc:dd MMM yyyy, HH:mm} UTC").FontSize(10);
                    grid.Item().Text($"{_ticket.Seat.Row}{_ticket.Seat.Letter}").FontSize(14).Bold().FontColor(Colors.Green.Medium);
                    grid.Item().Text(_ticket.Seat.Class.ToString()).FontSize(10);
                    grid.Item().Text("N17").FontSize(10);
                });
            });
        }

        void ComposeFooter(IContainer container)
        {
            var barcodeValue = $"TKT{_ticket.Id:D8}FLT{_ticket.FlightId:D5}";

            // Gerar o código de barras usando ZXing
            var writer = new BarcodeWriterPixelData
            {
                Format = BarcodeFormat.CODE_128,
                Options = new EncodingOptions
                {
                    Height = 50,
                    Width = 150,
                    Margin = 1
                }
            };

            var pixelData = writer.Write(barcodeValue);

            using var bitmap = new Bitmap(pixelData.Width, pixelData.Height,
                PixelFormat.Format32bppRgb);

            var bitmapData = bitmap.LockBits(
                new Rectangle(0, 0, pixelData.Width, pixelData.Height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format32bppRgb);

            try
            {
                System.Runtime.InteropServices.Marshal.Copy(pixelData.Pixels, 0, bitmapData.Scan0,
                    pixelData.Pixels.Length);
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }

            using var ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Position = 0;

            // Renderizar no PDF
            container.PaddingTop(10).Row(row =>
            {
                row.RelativeItem().Text($"Price Paid: €{_ticket.PricePaid:F2}").FontSize(8);
                row.ConstantItem(150).Image(ms.ToArray());
            });
        }

    }
}