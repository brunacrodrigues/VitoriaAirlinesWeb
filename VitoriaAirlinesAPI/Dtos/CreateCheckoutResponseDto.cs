namespace VitoriaAirlinesAPI.Dtos
{
    public class CreateCheckoutResponseDto
    {
        public string CheckoutUrl { get; set; }
        public string StripeSessionId { get; set; }
    }
}
