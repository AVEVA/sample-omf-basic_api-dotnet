using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OMFAPI;
using Xunit;

namespace OMFAPITests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            // Steps 1 to 7 - Run the main program
            Dictionary<string, dynamic> sentData = new ();
            Assert.True(Program.RunMain(true, sentData));

            // Step 8 - Check Creations
            Assert.True(CheckCreations(sentData));

            // Step 9 - Cleanup
            Assert.True(Cleanup());
        }

        private static bool CompareData(dynamic response, dynamic sentData)
        {
            bool success = true;

            foreach (JProperty property in sentData["values"][0])
            {
                if (property.Name != "Timestamp" && ((string)property.Value != (string)response.Property(property.Name).Value))
                    success = false;
            }

            return success;
        }

        private static bool CompareData(string itemName, dynamic response, dynamic sentData)
        {
            bool success = true;

            string[] split = itemName.Split(".");
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
                foreach (JProperty property in sentData["values"][0])
                {
                    if (property.Name != "Timestamp" && ((string)property.Value != (string)response))
                        success = false;
                }
            }

            return success;
        }

        private static async Task<HttpResponseMessage> SendGetRequestToEndpoint(Endpoint endpoint, string uri)
        {
            // create a request
            using HttpRequestMessage request = new ()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(uri),
            };

            // add headers to request
            request.Headers.Add("Accept-Verbosity", "verbose");
            if (string.Equals(endpoint.EndpointType, "CDS", StringComparison.OrdinalIgnoreCase))
            {
                request.Headers.Add("Authorization", "Bearer " + Program.GetToken(endpoint));
            }
            else if (string.Equals(endpoint.EndpointType, "PI", StringComparison.OrdinalIgnoreCase))
            {
                request.Headers.Add("x-requested-with", "XMLHTTPRequest");
                request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(string.Format(CultureInfo.InvariantCulture, "{0}:{1}", endpoint.Username, endpoint.Password))));
            }

            HttpResponseMessage response = await endpoint.Client.SendAsync(request).ConfigureAwait(false);
            return response;
        }

        private static bool Cleanup()
        {
            AppSettings settings = Program.GetAppSettings();
            IList<Endpoint> endpoints = settings.Endpoints;
            dynamic omfTypes = Program.GetJsonFile("OMF-Types.json");
            dynamic omfContainers = Program.GetJsonFile("OMF-Containers.json");

            bool success = true;

            foreach (Endpoint endpoint in endpoints)
            {
                if ((endpoint.VerifySSL is bool boolean) && boolean == false)
                {
                    endpoint.Handler = new ();
                    endpoint.Handler.ServerCertificateCustomValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
                    endpoint.Client = new (endpoint.Handler);
                }
                else
                {
                    endpoint.Client = new ();
                }

                try
                {
                    // delete containers
                    foreach (dynamic omfContainer in omfContainers)
                    {
                        string omfContainerString = $"[{JsonConvert.SerializeObject(omfContainer)}]";
                        Program.SendMessageToOmfEndpoint(endpoint, "container", omfContainerString, "delete");
                    }

                    // delete types
                    foreach (dynamic omfType in omfTypes)
                    {
                        string omfTypeString = $"[{JsonConvert.SerializeObject(omfType)}]";
                        Program.SendMessageToOmfEndpoint(endpoint, "type", omfTypeString, "delete");
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

        private bool CheckCreations(Dictionary<string, dynamic> sentData)
        {
            AppSettings settings = Program.GetAppSettings();
            IList<Endpoint> endpoints = settings.Endpoints;
            dynamic omfTypes = Program.GetJsonFile("OMF-Types.json");
            dynamic omfContainers = Program.GetJsonFile("OMF-Containers.json");
            dynamic omfData = Program.GetJsonFile("OMF-Data.json");

            bool success = true;

            foreach (Endpoint endpoint in endpoints)
            {
                if ((endpoint.VerifySSL is bool boolean) && boolean == false)
                {
                    endpoint.Handler = new ();
                    endpoint.Handler.ServerCertificateCustomValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
                    endpoint.Client = new (endpoint.Handler);
                }
                else
                {
                    endpoint.Client = new ();
                }

                try
                {
                    if (string.Equals(endpoint.EndpointType, "PI", StringComparison.OrdinalIgnoreCase))
                    {
                        // get point URLs
                        HttpResponseMessage response = SendGetRequestToEndpoint(endpoint, $"{endpoint.BaseEndpoint}/dataservers?name={endpoint.DataArchiveName}").Result;
                        string content = response.Content.ReadAsStringAsync().Result;
                        dynamic dynamicJson = JsonConvert.DeserializeObject(content);
                        string pointsURL = dynamicJson.Links.Points;

                        // get point data and check response
                        foreach (dynamic omfContainer in omfContainers)
                        {
                            response = SendGetRequestToEndpoint(endpoint, $"{pointsURL}?nameFilter={omfContainer.id}*").Result;
                            content = response.Content.ReadAsStringAsync().Result;
                            dynamicJson = JsonConvert.DeserializeObject(content);

                            // get end value URLs
                            foreach (dynamic item in dynamicJson.Items)
                            {
                                string endValueURL = item.Links.Value;

                                // retrieve data
                                response = SendGetRequestToEndpoint(endpoint, $"{endValueURL}").Result;
                                content = response.Content.ReadAsStringAsync().Result;
                                dynamicJson = JsonConvert.DeserializeObject(content);
                                dynamic endValue = dynamicJson.Value;

                                // check that the response was good and that data was written to the point
                                JToken name = endValue.SelectToken("Name");
                                if (!response.IsSuccessStatusCode)
                                {
                                    success = false;
                                    Console.WriteLine($"Unable to find name {name}");
                                }
                                else if (name != null && string.Equals(endValue.Name, "Pt Created", StringComparison.OrdinalIgnoreCase))
                                {
                                    success = false;
                                    Console.WriteLine($"{name} has no recorded data");
                                }

                                // compare the returned data to what was sent
                                if (!CompareData((string)item.Name, endValue, sentData[(string)omfContainer.id]))
                                {
                                    success = false;
                                    Console.WriteLine($"{name}'s data does not match what was sent");
                                }
                            }
                        }
                    }
                    else
                    {
                        // retrieve types and check response
                        foreach (dynamic omfType in omfTypes)
                        {
                            HttpResponseMessage response = SendGetRequestToEndpoint(endpoint, $"{endpoint.BaseEndpoint}/Types/{omfType.id}").Result;
                            if (!response.IsSuccessStatusCode)
                            {
                                Console.WriteLine($"Unable to find type {omfType.id}");
                                success = false;
                            }
                        }

                        // retrieve containers and check response
                        foreach (dynamic omfContainer in omfContainers)
                        {
                            HttpResponseMessage response = SendGetRequestToEndpoint(endpoint, $"{endpoint.BaseEndpoint}/Streams/{omfContainer.id}").Result;
                            if (!response.IsSuccessStatusCode)
                            {
                                success = false;
                                Console.WriteLine($"Unable to find container {omfContainer.id}");
                            }
                        }

                        // retrieve most recent data and check response
                        foreach (dynamic omfDatum in omfData)
                        {
                            HttpResponseMessage response = SendGetRequestToEndpoint(endpoint, $"{endpoint.BaseEndpoint}/Streams/{omfDatum.containerid}/Data/last").Result;
                            string responseString = response.Content.ReadAsStringAsync().Result;
                            string content = response.Content.ReadAsStringAsync().Result;
                            if (!response.IsSuccessStatusCode || string.IsNullOrEmpty(responseString))
                            {
                                success = false;
                                Console.WriteLine($"{omfDatum.id} has no recorded data");
                            }
                            else if (!CompareData(JsonConvert.DeserializeObject(content), sentData[(string)omfDatum.containerid]))
                            {
                                success = false;
                                Console.WriteLine($"Data in {omfDatum.id} does not match what was sent");
                            }
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
    }
}
