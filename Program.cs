using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DIDStatus
{
    internal class Program
    {
        public static async Task Main()
        {
            string namehist = "";
            string json = File.ReadAllText("config.json");
            string asciiArt = @"
 ____   __ ____       __  ______  ___  ______ __ __  __    
 || \\  || || \\     (( \ | || | // \\ | || | || || (( \   
 ||  )) || ||  ))     \\    ||   ||=||   ||   || ||  \\    
 ||_//  || ||_//     \_))   ||   || ||   ||   \\_// \_))   
                                                           


                         
";

            Console.WriteLine(asciiArt);
            // Parse the JSON content into a JsonDocument
            JsonDocument jsonDoc = JsonDocument.Parse(json);
            string dctoken = jsonDoc.RootElement.GetProperty("DiscordToken").GetString();
            string sptoken = jsonDoc.RootElement.GetProperty("SimplyPluralToken").GetString();
            HttpClient client = new HttpClient();
            HttpClient client2 = new HttpClient();
            HttpClient client3 = new HttpClient();
            client.DefaultRequestHeaders.TryAddWithoutValidation("authorization", sptoken);
            client2.DefaultRequestHeaders.TryAddWithoutValidation("authorization", sptoken);
            client3.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", dctoken);
            client3.DefaultRequestHeaders.TryAddWithoutValidation("content-type", "application/json");


            while (true)
            {

                HttpResponseMessage response = await client.GetAsync("https://api.apparyllis.com:8443/v1/fronters/");

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    JArray jsonArray = JArray.Parse(responseBody);
                    JObject jsonObject = (JObject)jsonArray[0];
                    string uid = jsonObject["content"]["uid"].ToString();
                    string member = jsonObject["content"]["member"].ToString();


                    HttpResponseMessage response2 = await client2.GetAsync("https://api.apparyllis.com:8443/v1/member/" + uid + "/" + member);
                    if (response2.IsSuccessStatusCode)
                    {
                        string responseBody2 = await response2.Content.ReadAsStringAsync();
                        dynamic jsonObject2 = JsonConvert.DeserializeObject(responseBody2);
                        string name = jsonObject2.content.name;
                        string pronouns = jsonObject2.content.pronouns;

                        if (namehist != name)
                        {
                            namehist = name;
                            Console.WriteLine("Name changed");

                            
                            string status = "{\"custom_status\":{\"text\": \"" + name + " | " + pronouns + "\"}}";
                            StringContent content = new StringContent(status, Encoding.UTF8, "application/json");
                            HttpRequestMessage request3 = new HttpRequestMessage(new HttpMethod("PATCH"), "https://discord.com/api/v9/users/@me/settings");
                            request3.Content = content;
                            HttpResponseMessage response3 = await client3.SendAsync(request3);

                            string responseBody3 = await response3.Content.ReadAsStringAsync();




                        }
                    }
                    else
                    {
                        Console.WriteLine($"Request failed with status code: {response2.StatusCode}");
                    }
                }
                else
                {
                    Console.WriteLine($"Request failed with status code: {response.StatusCode}");
                }
            System.Threading.Thread.Sleep(5000);
            }
            client.Dispose();
            System.Threading.Thread.Sleep(50000);
            

        }
    }
}
