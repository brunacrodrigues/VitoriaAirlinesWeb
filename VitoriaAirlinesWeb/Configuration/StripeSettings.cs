namespace VitoriaAirlinesWeb.Configuration
{
    /// <summary>
    /// Represents the configuration settings required to integrate with the Stripe payment gateway.
    /// Contains the public and secret API keys used for processing payments.
    /// </summary>
    public class StripeSettings
    {
        /// <summary>
        /// Gets or sets the public API key used on the client-side to initialize Stripe.
        /// </summary>
        public string PublishableKey { get; set; } = null!;


        /// <summary>
        /// Gets or sets the secret API key used on the server-side to authenticate Stripe requests.
        /// </summary>
        public string SecretKey { get; set; } = null!;
    }
}

