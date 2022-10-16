using System.Net.Http;
using System.Text.Json;
using System;


namespace Web_Parent_Control.Connector
{
    public class HttpConnector
    {
        public T GetData<T>(string url)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(url)
            };
            using (var response = client.Send(request))
            {
                response.EnsureSuccessStatusCode();
                var result = response.Content.ReadAsStringAsync().Result;
                var gzip = new Gzip();
                var decompressed = gzip.Decompress(result);
                var data = JsonSerializer.Deserialize<T>(decompressed);                           
                return data;
            }
        }
    }
}
