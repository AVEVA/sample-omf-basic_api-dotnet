namespace OMF_API
{
    public class AppSettings
    {
        public string NamespaceId { get; set; }
        public string TenantId { get; set; }
        public string Resource { get; set; }
        public string ClientId { get; set; }
        public string ClientKey { get; set; }
        public string ApiVersion { get; set; }
        public string dataservername { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public bool VERIFY_SSL { get; set; }
        public string ProducerToken { get; set; }
    }
}
