using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using NuciAPI.Requests;
using NuciAPI.Responses;
using NuciExtensions;
using NuciSecurity.HMAC;

namespace NuciAPI.Client
{
    /// <summary>
    /// A client that can send requests to an API.
    /// </summary>
    /// <param name="baseUrl">The base URL of the API. This is used to construct the full URL for each request.</param>
    public class NuciApiClient(string baseUrl) : INuciApiClient
    {
        /// <summary>
        /// The base URL of the API. This is used to construct the full URL for each request.
        /// </summary>
        public string BaseUrl { get; } = baseUrl;

        HttpClient HttpClient { get; } = new HttpClient();

        /// <summary>
        /// Sends a request to the API and returns the response. The request is sent as JSON in the body of the HTTP request.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request to be sent to the API. This must be a subclass of <see cref="NuciApiRequest"/>.</typeparam>
        /// <typeparam name="TResponse">The type of the response expected from the API. This must be a subclass of <see cref="NuciApiResponse"/>.</typeparam>
        /// <param name="method">The HTTP method to use for the request.</param>
        /// <param name="request">The request object containing the data to be sent to the API.</param>
        /// <param name="endpoint">The endpoint of the API to which the request is sent.</param>
        /// <returns>The response from the API.</returns>
        public async Task<NuciApiResponse> SendRequestAsync<TRequest, TResponse>(
            HttpMethod method,
            TRequest request,
            string endpoint)
                where TRequest : NuciApiRequest
                where TResponse : NuciApiResponse
            => await SendRequestAsync<TRequest, TResponse>(method, request, null, endpoint);

        /// <summary>
        /// Sends a request to the API and returns the response. The request is sent as JSON in the body of the HTTP request.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request to be sent to the API. This must be a subclass of <see cref="NuciApiRequest"/>.</typeparam>
        /// <typeparam name="TResponse">The type of the response expected from the API. This must be a subclass of <see cref="NuciApiResponse"/>.</typeparam>
        /// <param name="method">The HTTP method to use for the request.</param>
        /// <param name="request">The request object containing the data to be sent to the API.</param>
        /// <param name="authorisationInfo">The authorisation information to be included in the request.</param>
        /// <param name="endpoint">The endpoint of the API to which the request is sent.</param>
        /// <returns>The response from the API.</returns>
        public async Task<NuciApiResponse> SendRequestAsync<TRequest, TResponse>(
            HttpMethod method,
            TRequest request,
            NuciApiRequestAuthorisationInfo authorisationInfo,
            string endpoint)
                where TRequest : NuciApiRequest
                where TResponse : NuciApiResponse
        {
            HttpResponseMessage httpResponse =
                await HttpClient.SendAsync(GenerateHttpRequestMessage(
                    method,
                    request,
                    authorisationInfo,
                    endpoint));

            if (!httpResponse.IsSuccessStatusCode)
            {
                return await DeserialiseErrorResponse(httpResponse);
            }

            httpResponse.EnsureSuccessStatusCode();

            return (await httpResponse.Content.ReadAsStringAsync()).FromJson<TResponse>();
        }

        HttpRequestMessage GenerateHttpRequestMessage<TRequest>(
            HttpMethod method,
            TRequest request,
            NuciApiRequestAuthorisationInfo authorisationInfo,
            string endpoint) where TRequest : NuciApiRequest
        {
            HttpRequestMessage httpRequest = new(method, GetRequestUrl(endpoint))
            {
                Content = new StringContent(
                    request.ToJson(),
                    Encoding.UTF8,
                    "application/json"
                )
            };

            if (authorisationInfo is not null)
            {
                if (!string.IsNullOrEmpty(authorisationInfo.BearerToken))
                {
                    httpRequest.Headers.Authorization =
                        new AuthenticationHeaderValue(
                            "Bearer",
                            authorisationInfo.BearerToken);
                }

                if (!string.IsNullOrEmpty(authorisationInfo.HmacSharedSecretKey))
                {
                    httpRequest.Headers.Add(
                        "X-HMAC",
                        Uri.EscapeDataString(HmacEncoder.GenerateToken(
                            request,
                            authorisationInfo.HmacSharedSecretKey)
                        ));
                }
            }

            return httpRequest;
        }

        static async Task<NuciApiErrorResponse> DeserialiseErrorResponse(
            HttpResponseMessage httpResponse)
        {
            string responseString = await httpResponse.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(responseString))
            {
                return new NuciApiErrorResponse(
                    $"Request failed with status code" +
                    $" {(int)httpResponse.StatusCode}" +
                    $" ({httpResponse.StatusCode})");
            }

            return responseString.FromJson<NuciApiErrorResponse>();
        }

        string GetRequestUrl(string endpoint)
        {
            if (string.IsNullOrEmpty(BaseUrl))
            {
                return endpoint;
            }

            if (string.IsNullOrEmpty(endpoint))
            {
                return BaseUrl;
            }

            string url = BaseUrl;

            if (!endpoint.StartsWith("/") &&
                !url.EndsWith("/"))
            {
                url += "/";
            }

            return url + endpoint;
        }
    }
}
