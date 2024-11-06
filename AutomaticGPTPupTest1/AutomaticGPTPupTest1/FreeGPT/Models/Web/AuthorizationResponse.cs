namespace AutomaticGPTPupTest1.FreeGPT.Models.Web
{
    public class AuthorizationResponse
    {
        public User user { get; set; }
        public DateTime expires { get; set; }
        public string accessToken { get; set; }
        public string authProvider { get; set; }
    }

    public class User
    {
        public string id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string image { get; set; }
        public string picture { get; set; }
        public bool mfa { get; set; }
        public List<object> groups { get; set; }
        public string intercom_hash { get; set; }
    }
}
