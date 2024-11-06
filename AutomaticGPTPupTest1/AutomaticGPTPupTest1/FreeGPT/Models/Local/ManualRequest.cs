namespace AutomaticGPTPupTest1.FreeGPT.Models.Local
{
    public class ManualRequest
    {
        public string Method { get; set; }
        public string Url { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public string? PostData { get; set; }

        public ManualRequest(string method, string url, Dictionary<string, string> headerDict, string? postData = null)
        {
            Method = method;
            Url = url;
            Headers = headerDict;
            PostData = postData;
        }
    }
}
