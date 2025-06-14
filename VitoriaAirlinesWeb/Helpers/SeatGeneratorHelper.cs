using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Data.Enums;

namespace VitoriaAirlinesWeb.Helpers
{
    public class SeatGeneratorHelper : ISeatGeneratorHelper
    {
        public List<Seat> GenerateSeats(int airplaneId, int totalExecutiveSeats, int totalEconomySeats)
        {
            var seats = new List<Seat>();
            int seatsPerRow = 6;
            char[] letters = { 'A', 'B', 'C', 'D', 'E', 'F' };
            int currentRow = 1;

            // Executive seats
            for (int i = 0; i < totalExecutiveSeats; i++)
            {
                seats.Add(new Seat
                {
                    AirplaneId = airplaneId,
                    Row = currentRow,
                    Letter = letters[i % seatsPerRow].ToString(),
                    Class = SeatClass.Executive
                });

                if ((i + 1) % seatsPerRow == 0)
                    currentRow++;
            }

            if (totalExecutiveSeats > 0 && totalExecutiveSeats % seatsPerRow != 0)
                currentRow++;

            // Economy seats
            for (int i = 0; i < totalEconomySeats; i++)
            {
                seats.Add(new Seat
                {
                    AirplaneId = airplaneId,
                    Row = currentRow,
                    Letter = letters[i % seatsPerRow].ToString(),
                    Class = SeatClass.Economy
                });

                if ((i + 1) % seatsPerRow == 0)
                    currentRow++;
            }

            return seats;
        }

    }
}
