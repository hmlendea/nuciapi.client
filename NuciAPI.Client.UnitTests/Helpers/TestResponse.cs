using NuciAPI.Responses;

namespace NuciAPI.Client.UnitTests
{
    internal sealed class TestResponse : NuciApiSuccessResponse
    {
        public string Result { get; set; }
    }
}