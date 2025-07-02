namespace VitoriaAirlinesWeb.Models.Dtos
{
    public class ChatRequestDto
    {
        public string Prompt { get; set; } = default!;
        public List<ChatMessageDto> History { get; set; } = new();
    }
}
