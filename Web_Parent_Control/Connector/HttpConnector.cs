using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System;
using Web_Parent_Control.Database;
using Web_Parent_Control.Models;

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
                var data = JsonSerializer.Deserialize<T>(result);                           
                return data;
            }
        }
    }
}
