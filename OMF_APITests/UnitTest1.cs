using Newtonsoft.Json;
using OMF_API;
using System;
using System.Collections.Generic;
using System.Net;
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
            Assert.True(OMF_API.Program.runMain(true));
            // Step 8 - Check Creations
            Assert.True(checkCreations());
            // Step 9 - Cleanup
            Assert.True(cleanup());
        }

        private bool checkCreations()
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
                            if (!response.IsSuccessStatusCode || responseString == "")
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

        public static async Task<HttpResponseMessage> sendGetRequestToEndpoint(Endpoint endpoint, string uri)
        {
            // create a request
            HttpRequestMessage request = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(uri)
            }; 

            // add headers to request
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
