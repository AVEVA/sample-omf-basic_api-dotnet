﻿using System;

namespace OMFAPI
{
    public class Endpoint
    {
        /// <summary>
        /// Tells the application if the endpoint should be sent to
        /// </summary>
        public bool Selected { get; set; }

        /// <summary>
        /// The endpoint type. This will be OCS, EDS, or PI
        /// </summary>
        public string EndpointType { get; set; }

        /// <summary>
        /// The base endpoint. E.g. https://dat-b.osisoft.com for OCS
        /// </summary>
        public string Resource { get; set; }

        /// <summary>
        /// The ID of the Namespace in OCS that is being sent to
        /// </summary>
        public string NamespaceId { get; set; }

        /// <summary>
        /// The ID of the Tenant in OCS that is being sent to
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// The client ID that is being used for authenticating to OCS
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// The client secret that is being used for authenticating to OCS
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// The API version of the endpoint
        /// </summary>
        public string ApiVersion { get; set; }

        /// <summary>
        /// A feature flag for verifying SSL when connecting to the endpoint
        /// </summary>
        public object VerifySSL { get; set; }

        /// <summary>
        /// A feature flag for enabling compression on messages send to endpoint
        /// </summary>
        public bool UseCompression { get; set; }

        /// <summary>
        /// An optional timeout setting for web requests
        /// </summary>
        public int WebRequestTimeoutSeconds { get; set; }

        /// <summary>
        /// The name of the PI Data Archive that is being sent to
        /// </summary>
        public string DataArchiveName { get; set; }

        /// <summary>
        /// The username that is being used for authenticating to the PI Web API
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The password that is being used for authenticating to the PI Web API
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// The token used to authenticate to an OCS endpoint
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// returns the base endpoint URL of endpoint
        /// </summary>
        public string BaseEndpoint
        {
            get
            {
                string baseEndpoint = string.Empty;

                if (string.Equals(EndpointType, "OCS", StringComparison.OrdinalIgnoreCase))
                {
                    baseEndpoint = $"{Resource}/api/{ApiVersion}/tenants/{TenantId}/namespaces/{NamespaceId}";
                }
                else if (string.Equals(EndpointType, "EDS", StringComparison.OrdinalIgnoreCase))
                {
                    baseEndpoint = $"{Resource}/api/{ApiVersion}/tenants/default/namespaces/default";
                }
                else if (string.Equals(EndpointType, "PI", StringComparison.OrdinalIgnoreCase))
                {
                    baseEndpoint = Resource;
                }

                return baseEndpoint;
            }
        }

        /// <summary>
        /// returns the omf endpoint URL of endpoint
        /// </summary>
        public string OmfEndpoint
        {
            get { return $"{BaseEndpoint}/omf"; }
        }
    }
}
