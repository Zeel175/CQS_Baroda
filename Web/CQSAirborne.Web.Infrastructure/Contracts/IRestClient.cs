using CQSAirborne.Web.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CQSAirborne.Web.Infrastructure.Contracts
{
    public interface IRestClient
    {
        string BaseUrl { get; }
        Task<RestReponse> ExecuteAsync(RestRequest restRequest);
        Task<RestReponse<TResponse>> ExecuteGetAsync<TResponse>(RestRequest restRequest) where TResponse : class;
        Task<RestReponse> ExecutePostAsync<TRequest>(RestRequest<TRequest> restRequest) where TRequest : class;
        Task<RestReponse<TResponse>> ExecutePostAsync<TRequest, TResponse>(RestRequest<TRequest> restRequest)
            where TRequest : class
            where TResponse : class;

        Task<RestReponse<TResponse>> ExecutePostFileAsync<TResponse>(RestRequest<IFormFile> restRequest)
                where TResponse : class;

        Task<RestFileReponse> ExecuteFileRequestAsync(RestRequest restRequest);
    }
}
