using NuciAPI.Requests;

namespace NuciAPI.Client.UnitTests
{
    internal sealed class TestRequest : NuciApiRequest
    {
        public string Value { get; set; }

        public int Number { get; set; }
    }
}