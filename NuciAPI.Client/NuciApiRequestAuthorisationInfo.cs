namespace NuciAPI.Client
{
    public sealed class NuciApiRequestAuthorisationInfo
    {
        public string ClientId { get; set; }

        public string BearerToken { get; set; }

        public string HmacSharedSecretKey { get; set; }
    }
}
