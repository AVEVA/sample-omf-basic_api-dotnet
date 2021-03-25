using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace OMF_API
{
    public class AppSettings
    {
        /// <summary>
        /// The list of endpoints to connect and send to
        /// </summary>
        public IList<Endpoint> endpoints { get; set; }
    }

    public class Endpoint
    {
        /// <summary>
        /// The endpoint type. This will be OCS, EDS, or PI
        /// </summary>
        public string endpoint_type { get; set; }

        /// <summary>
        /// The base endpoint. E.g. https://dat-b.osisoft.com for OCS
        /// </summary>
        public string resource { get; set; }

        /// <summary>
        /// The name of the Namespace in OCS that is being sent to
        /// </summary>
        public string namespace_name { get; set; }

        /// <summary>
        /// The name of the Tenant ID of the Tenant in OCS that is being sent to
        /// </summary>
        public string tenant { get; set; }

        /// <summary>
        /// The client ID that is being used for authenticating to OCS
        /// </summary>
        public string client_id { get; set; }

        /// <summary>
        /// The client secret that is being used for authenticating to OCS
        /// </summary>
        public string client_secret { get; set; }

        /// <summary>
        /// The API version of the endpoint
        /// </summary>
        public string api_version { get; set; }

        /// <summary>
        /// A feature flag for verifying SSL when connecting to the endpoint
        /// </summary>
        public object verify_ssl { get; set; }

        /// <summary>
        /// A feature flag for enabling compression on messages send to endpoint
        /// </summary>
        public bool use_compression { get; set; }

        /// <summary>
        /// An optional timeout setting for web requests
        /// </summary>
        public int web_request_timeout_seconds { get; set; }

        /// <summary>
        /// The name of the PI Data Archive that is being sent to
        /// </summary>
        public string data_server_name { get; set; }

        /// <summary>
        /// The username that is being used for authenticating to the PI Web API
        /// </summary>
        public string username { get; set; }

        /// <summary>
        /// The password that is being used for authenticating to the PI Web API
        /// </summary>
        public string password { get; set; }

        /// <summary>
        /// The token used to authenticate to an OCS endpoint
        /// </summary>
        public string token { get; set; }

        /// <summary>
        /// returns the base endpoint URL of endpoint
        /// </summary>
        public string getBaseEndpoint()
        {
            string base_endpoint = "";

            if(this.endpoint_type == "OCS")
            {
                base_endpoint = $"{this.resource}/api/{this.api_version}/tenants/{this.tenant}/namespaces/{this.namespace_name}";
            }
            else if (this.endpoint_type == "EDS")
            {
                base_endpoint = $"{this.resource}/api/{this.api_version}/tenants/default/namespaces/default";
            }
            else if (this.endpoint_type == "PI")
            {
                base_endpoint = this.resource;
            }

            return base_endpoint;
        }

        /// <summary>
        /// returns the omf endpoint URL of endpoint
        /// </summary>
        public string getOmfEndpoint()
        {
            return $"{this.getBaseEndpoint()}/omf";
        }
    }
}
