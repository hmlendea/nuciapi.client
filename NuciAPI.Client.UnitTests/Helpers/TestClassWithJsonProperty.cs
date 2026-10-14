using System.Text.Json.Serialization;

namespace NuciAPI.Client.UnitTests.Helpers
{
    internal sealed class TestClassWithJsonProperty
    {
        [JsonPropertyName("custom_name")]
        public string CustomName { get; set; }
    }
}