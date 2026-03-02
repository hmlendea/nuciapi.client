using System.Net.Http;
using System.Threading.Tasks;
using NuciAPI.Requests;
using NuciAPI.Responses;

namespace NuciAPI.Client
{
    /// <summary>
    /// Interface for a client that can send requests to an API.
    /// </summary>
    public interface INuciApiClient
    {
        /// <summary>
        /// The base URL of the API. This is used to construct the full URL for each request.
        /// </summary>
        public string BaseUrl { get; }

        /// <summary>
        /// Sends a request to the API and returns the response. The request is sent as JSON in the body of the HTTP request.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request to be sent to the API. This must be a subclass of <see cref="NuciApiRequest"/>.</typeparam>
        /// <typeparam name="TResponse">The type of the response expected from the API. This must be a subclass of <see cref="NuciApiResponse"/>.</typeparam>
        /// <param name="method">The HTTP method to use for the request.</param>
        /// <param name="request">The request object containing the data to be sent to the API.</param>
        /// <param name="endpoint">The endpoint of the API to which the request is sent.</param>
        /// <returns>The response from the API.</returns>
        public Task<NuciApiResponse> SendRequestAsync<TRequest, TResponse>(
            HttpMethod method,
            TRequest request,
            string endpoint)
                where TRequest : NuciApiRequest
                where TResponse : NuciApiResponse;

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
        public Task<NuciApiResponse> SendRequestAsync<TRequest, TResponse>(
            HttpMethod method,
            TRequest request,
            NuciApiRequestAuthorisationInfo authorisationInfo,
            string endpoint)
                where TRequest : NuciApiRequest
                where TResponse : NuciApiResponse;
    }
}
