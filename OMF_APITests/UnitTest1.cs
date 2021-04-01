using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OMF_API;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace OMF_APITests
{
    public class UnitTest1
    {
        private static readonly HttpClient client = new HttpClient();

        [Fact]
        public void Test1()
        {
            // Steps 1 to 7 - Run the main program
            Dictionary<string, dynamic> sentData = new Dictionary<string, dynamic>();
            Assert.True(OMF_API.Program.runMain(true, sentData));
            // Step 8 - Check Creations
            Assert.True(checkCreations(sentData));
            // Step 9 - Cleanup
            Assert.True(cleanup());
        }

        private bool checkCreations(Dictionary<string, dynamic> sentData)
        {
            AppSettings settings = OMF_API.Program.getAppSettings();
            IList<Endpoint> endpoints = settings.endpoints;
            dynamic omfTypes = OMF_API.Program.getJsonFile("OMF-Types.json");
            dynamic omfContainers = OMF_API.Program.getJsonFile("OMF-Containers.json");
            dynamic omfData = OMF_API.Program.getJsonFile("OMF-Data.json");

            bool success = true;

            foreach (var endpoint in endpoints)
            {
                try
                {
                    if (endpoint.endpoint_type == "PI")
                    {
                        // get point URLs
                        HttpResponseMessage response = sendGetRequestToEndpoint(endpoint, $"{endpoint.getBaseEndpoint()}/dataservers?name={endpoint.data_server_name}").Result;
                        string content = response.Content.ReadAsStringAsync().Result;
                        dynamic dynamicJson = JsonConvert.DeserializeObject(content);
                        string pointsURL = dynamicJson.Links.Points;

                        // get point data and check response
                        foreach (var omfContainer in omfContainers)
                        {
                            response = sendGetRequestToEndpoint(endpoint, $"{pointsURL}?nameFilter={omfContainer.id}*").Result;
                            content = response.Content.ReadAsStringAsync().Result;
                            dynamicJson = JsonConvert.DeserializeObject(content);

                            // get end value URLs
                            foreach (var item in dynamicJson.Items)
                            {
                                string endValueURL = item.Links.Value;
                                // retrieve data
                                response = sendGetRequestToEndpoint(endpoint, $"{endValueURL}").Result;
                                content = response.Content.ReadAsStringAsync().Result;
                                dynamicJson = JsonConvert.DeserializeObject(content);
                                dynamic endValue = dynamicJson.Value;
                                //check that the response was good and that data was written to the point
                                JToken name = endValue.SelectToken("Name");
                                if (!response.IsSuccessStatusCode)
                                    success = false;
                                else if (name != null && endValue.Name == "Pt Created")
                                    success = false;
                                // compare the returned data to what was sent
                                if (!compareData((string)item.Name, endValue, sentData[(string)omfContainer.id]))
                                    success = false;
                            }
                        }
                    }
                    else
                    {
                        // retrieve types and check response
                        foreach (var omfType in omfTypes)
                        {
                            HttpResponseMessage response = sendGetRequestToEndpoint(endpoint, $"{endpoint.getBaseEndpoint()}/Types/{omfType.id}").Result;
                            if (!response.IsSuccessStatusCode)
                                success = false;
                        }

                        // retrieve containers and check response
                        foreach (var omfContainer in omfContainers)
                        {
                            HttpResponseMessage response = sendGetRequestToEndpoint(endpoint, $"{endpoint.getBaseEndpoint()}/Streams/{omfContainer.id}").Result;
                            if (!response.IsSuccessStatusCode)
                                success = false;
                        }

                        // retrieve most recent data and check response
                        foreach (var omfDatum in omfData)
                        {
                            HttpResponseMessage response = sendGetRequestToEndpoint(endpoint, $"{endpoint.getBaseEndpoint()}/Streams/{omfDatum.containerid}/Data/last").Result;
                            string responseString = response.Content.ReadAsStringAsync().Result;
                            string content = response.Content.ReadAsStringAsync().Result;
                            if (!response.IsSuccessStatusCode || responseString == "")
                                success = false;
                            else if (!compareData(JsonConvert.DeserializeObject(content), sentData[(string)omfDatum.containerid]))
                                success = false;
                        }

                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Encountered Error: {e}");
                    success = false;
                    throw;
                }
            }

            return success;
        }

        private bool cleanup()
        {
            AppSettings settings = OMF_API.Program.getAppSettings();
            IList<Endpoint> endpoints = settings.endpoints;
            dynamic omfTypes = OMF_API.Program.getJsonFile("OMF-Types.json");
            dynamic omfContainers = OMF_API.Program.getJsonFile("OMF-Containers.json");
            dynamic omfData = OMF_API.Program.getJsonFile("OMF-Data.json");

            bool success = true;

            foreach (var endpoint in endpoints)
            {
                try
                {
                    // delete containers
                    foreach (var omfContainer in omfContainers)
                    {
                        string omfContainerString = $"[{JsonConvert.SerializeObject(omfContainer)}]";
                        OMF_API.Program.sendMessageToOmfEndpoint(endpoint, "container", omfContainerString, "delete");
                    }

                    // delete types
                    foreach (var omfType in omfTypes)
                    {
                        string omfTypeString = $"[{JsonConvert.SerializeObject(omfType)}]";
                        OMF_API.Program.sendMessageToOmfEndpoint(endpoint, "type", omfTypeString, "delete");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Encountered Error: {e}");
                    success = false;
                    throw;
                }
            }

            return success;
        }

        private bool compareData(dynamic response, dynamic sentData)
        {
            bool success = true;

            foreach (JProperty property in sentData["values"][0])
            {
                if (property.Name != "timestamp" && ((string)property.Value != (string)response.Property(property.Name).Value))
                    success = false;
            }

            return success;
        }

        private bool compareData(string itemName, dynamic response, dynamic sentData)
        {
            bool success = true;

            var split = itemName.Split(".");
            if (split.Length == 2)
            {
                string key = split[1];
                foreach (JProperty property in sentData["values"][0])
                {
                    if (key == property.Name && ((string)property.Value != (string)response))
                        success = false;
                }
            }
            else
            {
                string key = split[0];
                foreach (JProperty property in sentData["values"][0])
                {
                    if (property.Name != "timestamp" && ((string)property.Value != (string)response))
                        success = false;
                }
            }

            return success;
        }

        private static async Task<HttpResponseMessage> sendGetRequestToEndpoint(Endpoint endpoint, string uri)
        {
            // create a request
            HttpRequestMessage request = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(uri)
            };

            // add headers to request
            request.Headers.Add("Accept-Verbosity", "verbose");
            if (endpoint.endpoint_type == "OCS")
            {
                request.Headers.Add("Authorization", "Bearer " + OMF_API.Program.getToken(endpoint));
            }
            else if (endpoint.endpoint_type == "PI")
            {
                request.Headers.Add("x-requested-with", "XMLHTTPRequest");
                request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", endpoint.username, endpoint.password))));
            }

            //return JsonConvert.DeserializeObject(OMF_API.Program.Send(request).Result);
            var response = await client.SendAsync(request);
            return response;
            //return OMF_API.Program.Send(request);
        }
    }
}
