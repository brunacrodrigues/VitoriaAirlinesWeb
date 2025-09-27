namespace VitoriaAirlinesAPI.Dtos
{
    public class LoginResponse
    {
        public string Token { get; set; } = null!;
        public DateTime Expiration { get; set; }

        public string? Role { get; set; }

    }
}
