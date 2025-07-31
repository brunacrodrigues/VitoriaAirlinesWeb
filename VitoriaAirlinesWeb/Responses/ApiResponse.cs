namespace VitoriaAirlinesWeb.Responses
{
    /// <summary>
    /// Represents a standardized API response structure.
    /// </summary>
    public class ApiResponse
    {
        /// <summary>
        /// Gets or sets a value indicating whether the API call was successful.
        /// </summary>
        public bool IsSuccess { get; set; }


        /// <summary>
        /// Gets or sets a message related to the API response (e.g., success message, error details).
        /// </summary>
        public string Message { get; set; }


        /// <summary>
        /// Gets or sets the results data from the API call. This can be any object type.
        /// </summary>
        public object Results { get; set; }
    }
}