using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Data.Enums;

namespace VitoriaAirlinesWeb.Helpers
{
    /// <summary>
    /// Provides helper methods for generating seat configurations for airplanes.
    /// </summary>
    public class SeatGeneratorHelper : ISeatGeneratorHelper
    {
        /// <summary>
        /// Generates a list of Seat entities for a given airplane, populating executive and economy seats.
        /// Seats are generated sequentially by row and letter (A-F). Executive seats come before economy seats.
        /// </summary>
        /// <param name="airplaneId">The ID of the airplane for which to generate seats.</param>
        /// <param name="totalExecutiveSeats">The total number of executive class seats to generate.</param>
        /// <param name="totalEconomySeats">The total number of economy class seats to generate.</param>
        /// <returns>A List of Seat entities representing the generated seats.</returns>
        public List<Seat> GenerateSeats(int airplaneId, int totalExecutiveSeats, int totalEconomySeats)
        {
            var seats = new List<Seat>();
            int seatsPerRow = 6; // Standard layout (A, B, C, D, E, F)
            char[] letters = { 'A', 'B', 'C', 'D', 'E', 'F' };
            int currentRow = 1; // Start from row 1

            // Generate Executive Class Seats
            for (int i = 0; i < totalExecutiveSeats; i++)
            {
                seats.Add(new Seat
                {
                    AirplaneId = airplaneId,
                    Row = currentRow,
                    Letter = letters[i % seatsPerRow].ToString(), // Cycle through letters A-F
                    Class = SeatClass.Executive
                });

                // Move to the next row if the current row is full (6 seats per row)
                if ((i + 1) % seatsPerRow == 0)
                    currentRow++;
            }

            // If executive seats ended mid-row, start economy seats on the next full row.
            // This prevents executive and economy seats from mixing in the same row unless intentionally designed.
            if (totalExecutiveSeats > 0 && totalExecutiveSeats % seatsPerRow != 0)
                currentRow++;

            // Generate Economy Class Seats
            for (int i = 0; i < totalEconomySeats; i++)
            {
                seats.Add(new Seat
                {
                    AirplaneId = airplaneId,
                    Row = currentRow,
                    Letter = letters[i % seatsPerRow].ToString(), // Cycle through letters A-F
                    Class = SeatClass.Economy
                });

                // Move to the next row if the current row is full (6 seats per row)
                if ((i + 1) % seatsPerRow == 0)
                    currentRow++;
            }

            return seats;
        }
    }
}