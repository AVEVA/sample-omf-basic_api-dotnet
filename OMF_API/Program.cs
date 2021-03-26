using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OMF_API
{
    public class Program
    {
        private static readonly HttpClient client = new HttpClient();

        // The version of the OMFmessages
        static string omfVersion = "1.1";

        // Holders for the data message values
        static Random rnd = new Random();
        static bool dynamicBoolHolder = true;
        static int dynamicIntHolder = 0;

        static void Main(string[] args)
        {
            runMain();
        }

        /// <summary>
        /// Main function to allow for easy testing.
        /// </summary>
        /// <param name="test">Whether this is a test or not</param>
        /// <returns></returns>
        public static bool runMain(bool test = false)
        {
            bool success = true;

            // Step 1 - Read endpoint configurations from config.json
            AppSettings settings = getAppSettings();
            IList<Endpoint> endpoints = settings.endpoints;

            // Step 2 - Get OMF Types
            dynamic omfTypes = getJsonFile("OMF-Types.json");

            // Step 3 - Get OMF Containers
            dynamic omfContainers = getJsonFile("OMF-Containers.json");

            // Step 4 - Get OMF Data
            dynamic omfData = getJsonFile("OMF-Data.json");

            // Send messages and check for each endpoint in config.json

            try
            {
                // Send out the messages that only need to be sent once
                foreach (var endpoint in endpoints)
                {
                    if ((endpoint.verify_ssl is bool) && (bool)endpoint.verify_ssl == false)
                        Console.WriteLine("You are not verifying the certificate of the end point.  This is not advised for any system as there are security issues with doing this.");


                    getToken(endpoint);

                    // Step 5 - Send OMF Types
                    foreach (var omfType in omfTypes)
                    {
                        string omfTypeString = $"[{JsonConvert.SerializeObject(omfType)}]";
                        sendMessageToOmfEndpoint(endpoint, "type", omfTypeString);
                    }

                    // Step 6 - Send OMF Containers
                    foreach (var omfContainer in omfContainers)
                    {
                        string omfContainerString = $"[{JsonConvert.SerializeObject(omfContainer)}]";
                        sendMessageToOmfEndpoint(endpoint, "container", omfContainerString);
                    }
                }

                // Step 7 - Send OMF Data
                int count = 0;
                //send data to all endpoints forever if this is not a test
                while (!test || count < 2)
                {
                    /*
                    * This is where custom loop logic should go.
                    * The getData call should also be customized to populate omfData with relevant data.
                    * */

                    foreach (var omfDatum in omfData)
                    {
                        foreach (var endpoint in endpoints)
                        {
                            // send data
                            getData(omfDatum);
                            string omfDatumString = $"[{JsonConvert.SerializeObject(omfDatum)}]";
                            sendMessageToOmfEndpoint(endpoint, "data", omfDatumString);
                        }
                    }

                    count++;
                    Thread.Sleep(1000);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine($"Encountered Error: {e}");
                success = false;
                if (test)
                    throw;
            }

            Console.WriteLine("Done");
            return success;
        }

        /// <summary>
        /// Gets a json file in the current directory of name filename.
        /// </summary>
        /// <param string="filename">The file to retrieve</param>
        /// <returns></returns>
        private static dynamic getJsonFile(string filename)
        {
            dynamic dynamicJson = JsonConvert.DeserializeObject(File.ReadAllText($"{Directory.GetCurrentDirectory()}\\{filename}"));

            return dynamicJson;
        }

        /// <summary>
        /// Gets the application settings
        /// </summary>
        /// <returns></returns>
        private static AppSettings getAppSettings()
        {
            AppSettings settings = JsonConvert.DeserializeObject<AppSettings>(File.ReadAllText(Directory.GetCurrentDirectory() + "\\appsettings.json"));

            // check for optional/nullable parameters
            foreach (var endpoint in settings.endpoints)
            {
                if (endpoint.verify_ssl == null)
                    endpoint.verify_ssl = true;
            }

            return settings;
        }

        /// <summary>
        /// Gets the current time
        /// </summary>
        /// <returns></returns>
        private static string getCurrentTime()
        {
            return DateTime.UtcNow.ToString("o");
        }

        /// <summary>
        /// Populates data with relevant data depending on the container type
        /// </summary>
        /// <param name="data">The dynamic json data object to populate</param>
        /// <returns></returns>
        private static void getData(dynamic data)
        {
            if (data.containerid == "Container1" || data.containerid == "Container2")
            {
                data.values[0].timestamp = getCurrentTime();
                data.values[0].IntegerProperty = (int)(rnd.NextDouble() * 100);
            }
            else if (data.containerid == "Container3")
            {
                dynamicBoolHolder = !dynamicBoolHolder;
                data.values[0].timestamp = getCurrentTime();
                data.values[0].NumberProperty1 = rnd.NextDouble() * 100;
                data.values[0].NumberProperty2 = rnd.NextDouble() * 100;
                data.values[0].StringEnum = dynamicBoolHolder.ToString();
            }
            else if (data.containerid == "Container4")
            {
                dynamicIntHolder = (dynamicIntHolder + 1) % 2;
                data.values[0].timestamp = getCurrentTime();
                data.values[0].IntegerEnum = dynamicIntHolder;
            }
            else
                Console.WriteLine($"Container {data.containerid} not recognized");
        }

        /// <summary>
        /// Gets the token for auth for connecting
        /// </summary>
        /// <param name="endpoint">The endpoint to retieve a token for</param>
        public static string getToken(Endpoint endpoint)
        {
            // PI and EDS currently require no auth
            if (endpoint.endpoint_type != "OCS")
                return null;

            //use cached version
            if (!String.IsNullOrWhiteSpace(endpoint.token))
                return endpoint.token;

            HttpRequestMessage request = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(endpoint.resource + "/identity/.well-known/openid-configuration")
            };
            request.Headers.Add("Accept", "application/json");

            string res = Send(request).Result;
            var objectContainingURLForAuth = JsonConvert.DeserializeObject<JObject>(res);

            var data = new Dictionary<string, string>
            {
               { "client_id", endpoint.client_id },
               { "client_secret", endpoint.client_secret },
               { "grant_type", "client_credentials" }
            };

            HttpRequestMessage request2 = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(objectContainingURLForAuth["token_endpoint"].ToString()),
                Content = new FormUrlEncodedContent(data)
            };
            request2.Headers.Add("Accept", "application/json");


            string res2 = Send(request2).Result;

            var tokenObject = JsonConvert.DeserializeObject<JObject>(res2);
            endpoint.token = tokenObject["access_token"].ToString();
            return endpoint.token;
        }

        /// <summary>
        /// Send message using HttpRequestMessage
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The result of the async task of the responding value from the endpoint</returns>
        private static async Task<string> Send(HttpRequestMessage request)
        {
            var response = await client.SendAsync(request);

            var responseString = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Error sending OMF response code:{response.StatusCode}.  Response {responseString}");
            return responseString;
        }


        /// <summary>
        /// Actual async call to send message to omf endpoint
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static string Send(WebRequest request)
        {
            // ServicePointManager.SecurityProtocol = SecurityProtocolType.;s
            using (var resp = request.GetResponse())
            {
                using (HttpWebResponse response = (HttpWebResponse)resp)
                {

                    var stream = resp.GetResponseStream();
                    var code = (int)response.StatusCode;

                    using (StreamReader reader = new StreamReader(stream))
                    {
                        // Read the content.  
                        string responseString = reader.ReadToEnd();
                        // Display the content.  

                        return responseString;
                    }
                }
            }
        }

        /// <summary>
        /// Sends message to the preconfigured omf endpoint
        /// </summary>
        /// <param name="endpoint">The endpoint to send an OMF message to</param>
        /// <param name="messageType">The OMF message type</param>
        /// <param name="dataJson">The message payload in a string format</param>
        /// <param name="action">The action for the OMF endpoint to conduct</param>
        private static void sendMessageToOmfEndpoint(Endpoint endpoint, string messageType, string dataJson, string action = "create")
        {
            // create a request
            WebRequest request = WebRequest.Create(new Uri(endpoint.getOmfEndpoint()));
            request.Method = "post";

            // add headers to request
            request.Headers.Add("messagetype", messageType);
            request.Headers.Add("action", action);
            request.Headers.Add("messageformat", "JSON");
            request.Headers.Add("omfversion", omfVersion);
            if (endpoint.endpoint_type == "OCS")
            {
                request.Headers.Add("Authorization", "Bearer " + getToken(endpoint));
            }
            else if (endpoint.endpoint_type == "PI")
            {
                request.Headers.Add("x-requested-with", "XMLHTTPRequest");
                request.Credentials = new NetworkCredential(endpoint.username, endpoint.password);
            }

            // compress dataJson if configured for compression
            byte[] byteArray;


            if (!endpoint.use_compression)
            {
                request.ContentType = "application/json";
                byteArray = Encoding.UTF8.GetBytes(dataJson);
            }
            else
            {
                request.ContentType = "application/x-www-form-urlencoded";
                using (var msi = new MemoryStream(Encoding.UTF8.GetBytes(dataJson)))
                using (var mso = new MemoryStream())
                {
                    using (var gs = new GZipStream(mso, CompressionMode.Compress))
                    {
                        // copy bytes from msi to gs
                        byte[] bytes = new byte[4096];

                        int cnt;

                        while ((cnt = msi.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            gs.Write(bytes, 0, cnt);
                        }

                    }

                    byteArray = mso.ToArray();
                }
                request.Headers.Add("compression", "gzip");
            }
            request.ContentLength = byteArray.Length;

            Stream dataStream = request.GetRequestStream();
            // Write the data to the request stream.  
            dataStream.Write(byteArray, 0, byteArray.Length);
            // Close the Stream object.  
            dataStream.Close();

            Send(request);
        }

    }

}
