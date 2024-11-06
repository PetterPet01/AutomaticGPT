namespace AutomaticGPTPupTest1.FreeGPT.Models.Web
{
    public class MessageStatusResponse
    {
        public class ModerationResponse
        {
            public bool flagged { get; set; }
            public bool blocked { get; set; }
            public string moderation_id { get; set; }
        }
        public string conversation_id { get; set; }
        public string message_id { get; set; }
        public bool is_completion { get; set; }
        public ModerationResponse moderation_response { get; set; }
    }
}
