using Newtonsoft.Json;

namespace AutomaticGPTPupTest1.FreeGPT.Models.Web
{
    public class MessageRequest
    {
        public class Author
        {
            public class Metadata
            {
                public string timestamp_ { get; set; }
            }
            public string role { get; set; }
            //Added for response regeneration
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public Metadata? metadata { get; set; }
        }

        public class Content
        {
            public string content_type { get; set; }
            public List<string> parts { get; set; }
        }

        public class Message
        {
            public class Metadata
            {
                public string? message_type { get; set; }
                public string? model_slug { get; set; }
            }
            public string id { get; set; }
            public Author author { get; set; }
            public Content content { get; set; }
            //Added for response regeneration
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public double? create_time { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public int? weight { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public Metadata? metadata { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string? recipient { get; set; }
        }

        public string action { get; set; }
        public List<Message> messages { get; set; }
        public string? conversation_id { get; set; }
        public string parent_message_id { get; set; }
        public string model { get; set; }
        public int timezone_offset_min { get; set; }
        public string? variant_purpose { get; set; }
    }
}
