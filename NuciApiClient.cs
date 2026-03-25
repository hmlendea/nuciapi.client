using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
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
            HttpRequestMessage httpRequest = new(method, GetRequestUrl(endpoint));

            if (method.Equals(HttpMethod.Post) ||
                method.Equals(HttpMethod.Put) ||
                method.Equals(HttpMethod.Patch))
            {
                httpRequest.Content = new StringContent(
                    request.ToJson(),
                    Encoding.UTF8,
                    "application/json");
            }
            else
            {
                Dictionary<string, string> queryParams =
                    QueryStringBuilder.Build(request);

                httpRequest.RequestUri =
                    new Uri(QueryHelpers.AddQueryString(
                        httpRequest.RequestUri.ToString(),
                        queryParams));
            }

            AttachRequestHeaders(httpRequest, authorisationInfo);

            return httpRequest;
        }

        void AttachRequestHeaders(
            HttpRequestMessage httpRequest,
            NuciApiRequestAuthorisationInfo authorisationInfo)
        {
            string clientId = authorisationInfo?.ClientId;

            if (string.IsNullOrEmpty(clientId))
            {
                clientId = "UnknownClient";
            }

            httpRequest.Headers.Add(
                "X-Client-ID",
                clientId);

            httpRequest.Headers.Add(
                "X-Request-ID",
                Guid.NewGuid().ToString().ToUpper());

            httpRequest.Headers.Add(
                "X-Timestamp",
                Uri.EscapeDataString(DateTimeOffset.Now.ToString("o")));

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
                            httpRequest,
                            authorisationInfo.HmacSharedSecretKey)
                        ));
                }
            }
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
                    $" ({httpResponse.StatusCode})",
                    NuciApiResponseCodes.ErrorCodes.Default);
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
