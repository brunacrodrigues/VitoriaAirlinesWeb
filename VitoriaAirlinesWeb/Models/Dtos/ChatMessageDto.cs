namespace VitoriaAirlinesWeb.Models.Dtos
{
    /// <summary>
    /// Represents a single message within a chat conversation, including the role of the sender and the content of the message.
    /// </summary>
    public class ChatMessageDto
    {
        /// <summary>
        /// Gets or sets the role of the sender of the message (e.g., "user", "model", "admin").
        /// </summary>
        public string Role { get; set; } = default!;


        /// <summary>
        /// Gets or sets the textual content of the chat message.
        /// </summary>
        public string Content { get; set; } = default!;
    }
}