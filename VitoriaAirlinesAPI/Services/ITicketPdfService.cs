using VitoriaAirlinesWeb.Data.Entities;

namespace VitoriaAirlinesAPI.Services
{
    public interface ITicketPdfService
    {
        Task<byte[]> GenerateTicketPdfAsync(Ticket ticket);
    }
}
