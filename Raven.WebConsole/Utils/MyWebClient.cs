using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Raven.WebConsole.Utils
{
    public interface IWebClient
    {
        dynamic GetDynamicJson(string url);
        T GetJson<T>(string url);
        dynamic PostJson(string url, object json);
    }

    public class MyWebClient : IWebClient
    {
        public string GetString(string url)
        {
            using (var webClient = new WebClient())
                return webClient.DownloadString(url);
        }

        public T GetJson<T>(string url)
        {
            return JsonConvert.DeserializeObject<T>(GetString(url));
        }

        public dynamic GetDynamicJson(string url)
        {
            return JObject.Parse(GetString(url));
        }

        public dynamic PostJson(string url, object json)
        {
            using (var webClient = new WebClient())
                return JObject.Parse(webClient.UploadString(url, JsonConvert.SerializeObject(json)));
        }
    }
}