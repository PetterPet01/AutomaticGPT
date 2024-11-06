namespace AutomaticGPTPupTest1.FreeGPT.Models.Web
{
    internal class ConversationFullQuery
    {
        public class Author
        {
            public string role { get; set; }
            public Metadata metadata { get; set; }
        }

        public class Content
        {
            public string content_type { get; set; }
            public List<string> parts { get; set; }
        }

        public class MessageData
        {
            public string id { get; set; }
            public Message message { get; set; }
            public string? parent { get; set; }
            public List<string> children { get; set; }
        }

        public class FinishDetails
        {
            public string type { get; set; }
            public string stop { get; set; }
        }

        public class Message
        {
            public string id { get; set; }
            public Author author { get; set; }
            public double create_time { get; set; }
            public Content content { get; set; }
            public string status { get; set; }
            public bool end_turn { get; set; }
            public double weight { get; set; }
            public Metadata metadata { get; set; }
            public string recipient { get; set; }
        }

        public class Metadata
        {
            public string timestamp_ { get; set; }
            public FinishDetails finish_details { get; set; }
            public bool is_complete { get; set; }
            public string model_slug { get; set; }
        }

        public string title { get; set; }
        public double create_time { get; set; }
        public double update_time { get; set; }
        public Dictionary<string, MessageData> mapping { get; set; }
        public List<object> moderation_results { get; set; }
        public string current_node { get; set; }
    }
}
