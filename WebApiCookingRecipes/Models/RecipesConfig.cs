using System.ComponentModel;

namespace WebApiTestApp.Models
{
    public class RecipesApiConfig
    {
        public string ApiKeyName { get; set; }
        public string ApiKeyValue { get; set; }
        public string HostName { get; set; }
        public string HostValue { get; set; }
        public string Url { get; set; }
    }
}
