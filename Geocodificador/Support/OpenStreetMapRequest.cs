using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Geocodificador.Support
{
    public class OpenStreetMapRequest
    {
        public Coords getCoordsFromAdress(string url) {

            Coords coords = new Coords();
            using var client = new HttpClient();
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            requestMessage.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.150 Safari/537.36");
            var content = client.SendAsync(requestMessage).Result;
            var stringResponse = content.Content.ReadAsStringAsync().Result;

            if (content.IsSuccessStatusCode)
            {
                var arrayObject = JArray.Parse(stringResponse);
                coords.lat = (double)arrayObject[0]["lat"];
                coords.lon = (double)arrayObject[0]["lon"];
                Console.WriteLine("Datos geograficos obtenidos: {0} , {1}", arrayObject[0]["lat"], arrayObject[0]["lon"]);
            }
            else
                Console.WriteLine(content.StatusCode);

            return coords;
        }

        public class Coords
        {
            public double lat;
            public double lon;
        }

    }
}
