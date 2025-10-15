using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace ShortUrl.Common.Utility.ToolsHelpers
{
    public class HttpTools
    {
        public async Task<string> GetAsync(string uri, string token = null)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, uri);

            if (token != null)
                requestMessage.Headers.Add("Authorization", $"bearer {token}");

            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            var client = new HttpClient() { Timeout = new TimeSpan(0, 2, 0) };

            var response = await client.SendAsync(requestMessage);

            return response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync() : null;
        }

        public async Task<string> PostAsync(string uri, object data, string token = null, int TimeoutByMinutes = 2)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json")
            };

            if (token != null)
                requestMessage.Headers.Add("Authorization", $"bearer {token}");

            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            var client = new HttpClient() { Timeout = new TimeSpan(0, TimeoutByMinutes, 0) };

            var response = await client.SendAsync(requestMessage);

            return response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync() : null;
        }
    }
}
