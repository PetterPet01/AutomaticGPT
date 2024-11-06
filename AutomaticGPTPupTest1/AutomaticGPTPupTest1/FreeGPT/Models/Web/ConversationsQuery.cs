namespace AutomaticGPTPupTest1.FreeGPT.Models.Web
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Item
    {
        public string id { get; set; }
        public string title { get; set; }
        public DateTime create_time { get; set; }
        public DateTime update_time { get; set; }
        public object mapping { get; set; }
        public object current_node { get; set; }
    }

    public class ConversationsQuery
    {
        public List<Item> items { get; set; }
        public int total { get; set; }
        public int limit { get; set; }
        public int offset { get; set; }
        public bool has_missing_conversations { get; set; }
    }
}
