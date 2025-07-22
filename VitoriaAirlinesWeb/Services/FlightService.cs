using Microsoft.EntityFrameworkCore;
using VitoriaAirlinesWeb.Data;
using VitoriaAirlinesWeb.Data.Enums;

namespace VitoriaAirlinesWeb.Services
{
    public class FlightService : IFlightService
    {
        private readonly DataContext _context;
        private readonly INotificationService _notificationService;

        public FlightService(
            DataContext context,
            INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task UpdateFlightStatusAsync()
        {
            var now = DateTime.UtcNow;

            var flights = await _context.Flights
                .Where(f => f.Status != FlightStatus.Canceled)
                .ToListAsync();

            bool hasChanges = false;

            foreach (var flight in flights)
            {
                if (flight.Status == FlightStatus.Scheduled && flight.DepartureUtc <= now && now < flight.ArrivalUtc)
                {
                    flight.Status = FlightStatus.Departed;
                    hasChanges = true;


                    await _notificationService.NotifyAdminsAsync($"Flight {flight.FlightNumber} has departed.");
                    await _notificationService.NotifyFlightCustomersAsync(flight, $"Your flight {flight.FlightNumber} has just departed.");

                    await _notificationService.NotifyFlightStatusChangedAsync(flight.Id, "Departed");

                }
                else if (now >= flight.ArrivalUtc && flight.Status != FlightStatus.Completed)
                {
                    flight.Status = FlightStatus.Completed;
                    hasChanges = true;

                    await _notificationService.NotifyAdminsAsync($"Flight {flight.FlightNumber} has arrived.");
                    await _notificationService.NotifyFlightCustomersAsync(flight, $"Your flight {flight.FlightNumber} has arrived. Thank you for flying with us!");


                    await _notificationService.NotifyFlightStatusChangedAsync(flight.Id, "Completed"); ;
                }
            }

            if (hasChanges)
            {
                await _context.SaveChangesAsync();
            }
        }

    }
}