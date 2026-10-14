using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Moq;
using Moq.Protected;

using NUnit.Framework;

using NuciAPI.Client;
using NuciAPI.Requests;
using NuciAPI.Responses;
using NuciExtensions;

namespace NuciAPI.Client.UnitTests
{
    [TestFixture]
    public class NuciApiClientTests
    {
        private Mock<HttpMessageHandler> httpMessageHandlerMock;
        private HttpClient httpClient;
        private NuciApiClient client;
        private const string BaseUrl = "https://api.example.com";

        [SetUp]
        public void SetUp()
        {
            httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            httpClient = new HttpClient(httpMessageHandlerMock.Object);
            client = new NuciApiClient(BaseUrl, httpClient);
        }

        [Test]
        public void GivenBaseUrl_WhenConstructing_ThenBaseUrlIsSet()
            => Assert.That(client.BaseUrl, Is.EqualTo(BaseUrl));

        [Test]
        public void GivenEmptyBaseUrl_WhenConstructing_ThenBaseUrlIsEmpty()
        {
            NuciApiClient newClient = new(string.Empty);
            Assert.That(newClient.BaseUrl, Is.EqualTo(string.Empty));
        }

        [Test]
        public async Task GivenValidRequest_WhenSendRequestAsync_ThenReturnsResponse()
        {
            TestRequest request = new() { Value = "test" };
            TestResponse expectedResponse = new() { Result = "success" };
            string responseJson = expectedResponse.ToJson();

            httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseJson)
                });

            NuciApiResponse result = await client.SendRequestAsync<TestRequest, TestResponse>(
                HttpMethod.Post,
                request,
                "/test");

            Assert.That(result, Is.TypeOf<TestResponse>());
            Assert.That(((TestResponse)result).Result, Is.EqualTo("success"));
        }

        [Test]
        public async Task GivenErrorResponse_WhenSendRequestAsync_ThenReturnsErrorResponse()
        {
            TestRequest request = new() { Value = "test" };
            NuciApiErrorResponse errorResponse = new(
                "Not found",
                NuciApiResponseCodes.ErrorCodes.NotFound);

            httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Content = new StringContent(errorResponse.ToJson())
                });

            NuciApiResponse result = await client.SendRequestAsync<TestRequest, TestResponse>(
                HttpMethod.Get,
                request,
                "/test");

            Assert.That(result, Is.TypeOf<NuciApiErrorResponse>());
            Assert.That(((NuciApiErrorResponse)result).Code, Is.EqualTo(NuciApiResponseCodes.ErrorCodes.NotFound));
        }

        [Test]
        public async Task GivenNullResponseBody_WhenErrorResponse_ThenReturnsDefaultErrorResponse()
        {
            TestRequest request = new() { Value = "test" };

            httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent(string.Empty)
                });

            NuciApiResponse result = await client.SendRequestAsync<TestRequest, TestResponse>(
                HttpMethod.Get,
                request,
                "/test");

            Assert.That(result, Is.TypeOf<NuciApiErrorResponse>());
            Assert.That(((NuciApiErrorResponse)result).Code, Is.EqualTo(NuciApiResponseCodes.ErrorCodes.Default));
        }

        [Test]
        public async Task GivenAuthorisationInfo_WhenSendRequestAsync_ThenHeadersAreAttached()
        {
            TestRequest request = new() { Value = "test" };
            NuciApiRequestAuthorisationInfo authorisationInfo = new()
            {
                ClientId = "test-client",
                BearerToken = "test-token",
                HmacSharedSecretKey = "test-secret"
            };

            HttpRequestMessage capturedRequest = null;

            httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(new TestResponse { Result = "success" }.ToJson())
                });

            await client.SendRequestAsync<TestRequest, TestResponse>(
                HttpMethod.Post,
                request,
                authorisationInfo,
                "/test");

            Assert.That(capturedRequest, Is.Not.Null);
            Assert.That(capturedRequest.Headers.GetValues("X-Client-ID"), Has.Exactly(1).EqualTo("test-client"));
            Assert.That(capturedRequest.Headers.GetValues("X-Request-ID"), Has.Exactly(1).Matches<string>(s => Guid.TryParse(s, out _)));
            Assert.That(capturedRequest.Headers.GetValues("X-Timestamp"), Has.Exactly(1).Matches<string>(s => DateTimeOffset.TryParse(Uri.UnescapeDataString(s), out _)));
            Assert.That(capturedRequest.Headers.Authorization, Is.Not.Null);
            Assert.That(capturedRequest.Headers.Authorization.Scheme, Is.EqualTo("Bearer"));
            Assert.That(capturedRequest.Headers.Authorization.Parameter, Is.EqualTo("test-token"));
            Assert.That(capturedRequest.Headers.GetValues("X-HMAC"), Has.Exactly(1).Matches<string>(s => !string.IsNullOrEmpty(s)));
        }

        [Test]
        public async Task GivenNoAuthorisationInfo_WhenSendRequestAsync_ThenUsesMachineNameAsClientId()
        {
            TestRequest request = new() { Value = "test" };

            HttpRequestMessage capturedRequest = null;

            httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(new TestResponse { Result = "success" }.ToJson())
                });

            await client.SendRequestAsync<TestRequest, TestResponse>(
                HttpMethod.Post,
                request,
                "/test");

            Assert.That(capturedRequest, Is.Not.Null);
            Assert.That(capturedRequest.Headers.GetValues("X-Client-ID"), Has.Exactly(1).EqualTo(Environment.MachineName));
        }

        [Test]
        public async Task GivenGetRequest_WhenSendRequestAsync_ThenQueryStringIsBuilt()
        {
            TestRequest request = new() { Value = "test", Number = 42 };
            TestResponse expectedResponse = new() { Result = "success" };

            HttpRequestMessage capturedRequest = null;

            httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(expectedResponse.ToJson())
                });

            await client.SendRequestAsync<TestRequest, TestResponse>(
                HttpMethod.Get,
                request,
                "/test");

            Assert.That(capturedRequest, Is.Not.Null);
            Assert.That(capturedRequest.RequestUri.Query, Does.Contain("value=test"));
            Assert.That(capturedRequest.RequestUri.Query, Does.Contain("number=42"));
        }

        [Test]
        public async Task GivenPostRequest_WhenSendRequestAsync_ThenBodyIsSent()
        {
            TestRequest request = new() { Value = "test", Number = 42 };
            TestResponse expectedResponse = new() { Result = "success" };

            HttpRequestMessage capturedRequest = null;

            httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(expectedResponse.ToJson())
                });

            await client.SendRequestAsync<TestRequest, TestResponse>(
                HttpMethod.Post,
                request,
                "/test");

            Assert.That(capturedRequest, Is.Not.Null);
            Assert.That(capturedRequest.Content, Is.Not.Null);
            string content = await capturedRequest.Content.ReadAsStringAsync();
            Assert.That(content, Does.Contain("test"));
            Assert.That(content, Does.Contain("42"));
        }

        [Test]
        public async Task GivenHttpException_WhenSendRequestAsync_ThenThrows()
        {
            TestRequest request = new() { Value = "test" };

            httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            Assert.ThrowsAsync<HttpRequestException>(async () =>
                await client.SendRequestAsync<TestRequest, TestResponse>(
                    HttpMethod.Get,
                    request,
                    "/test"));
        }

        private sealed class TestRequest : NuciApiRequest
        {
            public string Value { get; set; }
            public int Number { get; set; }
        }

        private sealed class TestResponse : NuciApiSuccessResponse
        {
            public string Result { get; set; }
        }
    }
}