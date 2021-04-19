using System.Collections.Generic;

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
        public string endpointType { get; set; }

        /// <summary>
        /// The base endpoint. E.g. https://dat-b.osisoft.com for OCS
        /// </summary>
        public string resource { get; set; }

        /// <summary>
        /// The name of the Namespace in OCS that is being sent to
        /// </summary>
        public string namespaceName { get; set; }

        /// <summary>
        /// The name of the Tenant ID of the Tenant in OCS that is being sent to
        /// </summary>
        public string tenant { get; set; }

        /// <summary>
        /// The client ID that is being used for authenticating to OCS
        /// </summary>
        public string clientId { get; set; }

        /// <summary>
        /// The client secret that is being used for authenticating to OCS
        /// </summary>
        public string clientSecret { get; set; }

        /// <summary>
        /// The API version of the endpoint
        /// </summary>
        public string apiVersion { get; set; }

        /// <summary>
        /// A feature flag for verifying SSL when connecting to the endpoint
        /// </summary>
        public object verifySSL { get; set; }

        /// <summary>
        /// A feature flag for enabling compression on messages send to endpoint
        /// </summary>
        public bool useCompression { get; set; }

        /// <summary>
        /// An optional timeout setting for web requests
        /// </summary>
        public int webRequestTimeoutSeconds { get; set; }

        /// <summary>
        /// The name of the PI Data Archive that is being sent to
        /// </summary>
        public string dataServerName { get; set; }

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
        public string baseEndpoint
        {
            get
            {
                string base_endpoint = "";

                if (string.Equals(this.endpointType, "OCS"))
                {
                    base_endpoint = $"{this.resource}/api/{this.apiVersion}/tenants/{this.tenant}/namespaces/{this.namespaceName}";
                }
                else if (string.Equals(this.endpointType, "EDS"))
                {
                    base_endpoint = $"{this.resource}/api/{this.apiVersion}/tenants/default/namespaces/default";
                }
                else if (string.Equals(this.endpointType, "PI"))
                {
                    base_endpoint = this.resource;
                }

                return base_endpoint;
            }
        }

        /// <summary>
        /// returns the omf endpoint URL of endpoint
        /// </summary>
        public string omf_endpoint
        {
            get { return $"{this.baseEndpoint}/omf"; }
        }
    }
}
