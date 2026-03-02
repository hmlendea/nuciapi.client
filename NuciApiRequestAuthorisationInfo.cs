namespace NuciAPI.Client
{
    public class NuciApiRequestAuthorisationInfo
    {
        public string BearerToken { get; set; }

        public string HmacSharedSecretKey { get; set; }
    }
}
