namespace VitoriaAirlinesWeb.Helpers
{
    public interface IFlightHelper
    {
        Task<string> GenerateUniqueFlightNumberAsync();
    }
}
