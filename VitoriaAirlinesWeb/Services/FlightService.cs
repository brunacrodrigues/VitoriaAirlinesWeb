using Microsoft.EntityFrameworkCore;
using VitoriaAirlinesWeb.Data;
using VitoriaAirlinesWeb.Data.Enums;

namespace VitoriaAirlinesWeb.Services
{
    /// <summary>
    /// Provides services for managing flight statuses, such as updating them based on departure/arrival times
    /// and notifying relevant parties.
    /// </summary>
    public class FlightService : IFlightService
    {
        private readonly DataContext _context;
        private readonly INotificationService _notificationService;


        /// <summary>
        /// Initializes a new instance of the FlightService class.
        /// </summary>
        /// <param name="context">The data context for database operations.</param>
        /// <param name="notificationService">Service for sending various notifications.</param>
        public FlightService(
            DataContext context,
            INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }


        /// <summary>
        /// Asynchronously updates the status of flights based on their scheduled times and the current UTC time.
        /// Flights change from Scheduled to Departed, and from Departed to Completed.
        /// Relevant administrators and customers are notified of status changes.
        /// </summary>
        /// <returns>Task: A Task representing the asynchronous operation.</returns>
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


                    await _notificationService.NotifyFlightStatusChangedAsync(flight.Id, "Completed");
                }
            }

            if (hasChanges)
            {
                await _context.SaveChangesAsync();
            }
        }
    }
}