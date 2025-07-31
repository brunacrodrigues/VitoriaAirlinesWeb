namespace VitoriaAirlinesWeb.Models.Dtos
{
    /// <summary>
    /// Represents a request made to a chat bot, containing the user's prompt and the conversation history.
    /// </summary>
    public class ChatRequestDto
    {
        /// <summary>
        /// Gets or sets the current prompt or query from the user.
        /// </summary>
        public string Prompt { get; set; } = default!;


        /// <summary>
        /// Gets or sets the history of the conversation, represented as a list of chat messages.
        /// </summary>
        public List<ChatMessageDto> History { get; set; } = new();
    }
}