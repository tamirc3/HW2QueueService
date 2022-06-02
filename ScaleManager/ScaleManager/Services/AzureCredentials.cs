namespace ScaleManager.Services
{
    public class AzureCredentials
    {
        public string TenantId{ get; }
        public string ClientId { get; }
        public string SecretId { get; }

        public AzureCredentials(string secretId, string clientId, string tenantId)
        {
            SecretId = secretId;
            ClientId = clientId;
            TenantId = tenantId;
        }
    }
}
