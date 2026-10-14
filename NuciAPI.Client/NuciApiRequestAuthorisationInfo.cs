namespace NuciAPI.Client
{
    public class NuciApiRequestAuthorisationInfo
    {
        public string ClientId { get; set; }

        public string BearerToken { get; set; }

        public string HmacSharedSecretKey { get; set; }
    }
}
