using CQSAirborne.Model.Core;
using CQSAirborne.Web.Infrastructure.Contracts;
using CQSAirborne.Web.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CQSAirborne.Web.Infrastructure.Implementation
{
    public class CQSRestClient : IRestClient
    {
        public string BaseUrl { get; }
        private readonly HttpClient _httpClient;

        public CQSRestClient(string baseUrl)
        {
            BaseUrl = baseUrl;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl)
            };

        }

        /// <summary>
        /// Executes the HTTP request based on the settings
        /// </summary>
        /// <param name="restRequest"></param>
        /// <returns></returns>
        public Task<RestReponse> ExecuteAsync(RestRequest restRequest)
        {
            if(!string.IsNullOrEmpty(restRequest.Token))
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", restRequest.Token);

            switch (restRequest.Method)
            {
                case RestMethodType.Post:
                    return ProcessPostRequestAsync(restRequest.ResourceUrl, restRequest.Parameters, restRequest.QueryStrings, null);
                case RestMethodType.Get:
                    return ProcessGetRequestAsync(restRequest.ResourceUrl, restRequest.Parameters, restRequest.QueryStrings);
            }
            return Task.Run(() => new RestReponse { IsSuccess = false, Status = System.Net.HttpStatusCode.NotAcceptable });
        }

        public async Task<RestReponse<TResponse>> ExecuteGetAsync<TResponse>(RestRequest restRequest)
            where TResponse : class
        {
            if (!string.IsNullOrEmpty(restRequest.Token))
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", restRequest.Token);

            var data = await ProcessGetRequestAsync(restRequest.ResourceUrl, restRequest.Parameters, restRequest.QueryStrings);
            return new RestReponse<TResponse>
            {
                IsSuccess = data.IsSuccess,
                Status = data.Status,
                Data = data.IsSuccess && data.Data != null ? JsonConvert.DeserializeObject<TResponse>(data.Data) : null
            };
        }

        /// <summary>
        /// Execute the HTTP POST request to pass the Body part
        /// </summary>
        /// <typeparam name="TReqeust"></typeparam>
        /// <param name="restRequest"></param>
        /// <returns></returns>
        public Task<RestReponse> ExecutePostAsync<TReqeust>(RestRequest<TReqeust> restRequest) where TReqeust : class
        {
            if (!string.IsNullOrEmpty(restRequest.Token))
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", restRequest.Token);

            return ProcessPostRequestAsync(restRequest.ResourceUrl, restRequest.Parameters, restRequest.QueryStrings, restRequest.Body);
        }

        /// <summary>
        /// Execute the HTTP POST request to pass the Body part
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="restRequest"></param>
        /// <returns></returns>
        public async Task<RestReponse<TResponse>> ExecutePostAsync<TRequest, TResponse>(RestRequest<TRequest> restRequest)
            where TRequest : class
            where TResponse : class
        {
            if (!string.IsNullOrEmpty(restRequest.Token))
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", restRequest.Token);


            var data = await ProcessPostRequestAsync(restRequest.ResourceUrl, restRequest.Parameters, restRequest.QueryStrings, restRequest.Body);
            return new RestReponse<TResponse>
            {
                IsSuccess = data.IsSuccess,
                Status = data.Status,
                Data = data.IsSuccess && data.Data != null ? JsonConvert.DeserializeObject<TResponse>(data.Data) : null,
                Error = data.Errors != null && data.Errors.Count() > 0 ? data.Errors[0]?.Errors[0]?.ToString() : ""
            };
        }

        public async Task<RestReponse<TResponse>> ExecutePostFileAsync<TResponse>(RestRequest<IFormFile> restRequest)
            where TResponse : class
        {
            if (!string.IsNullOrEmpty(restRequest.Token))
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", restRequest.Token);

            var data = await ProcessPostRequestWithFileAsync(restRequest.ResourceUrl, restRequest.Parameters, restRequest.QueryStrings, restRequest.Body);
            return new RestReponse<TResponse>
            {
                IsSuccess = data.IsSuccess,
                Status = data.Status,
                Data = data.IsSuccess && data.Data != null ? JsonConvert.DeserializeObject<TResponse>(data.Data) : null
            };
        }

        private async Task<RestReponse> ProcessGetRequestAsync(string resourceUrl, Dictionary<string, string> parameters, Dictionary<string, string> queryStrings)
        {
            resourceUrl = GenerateResourceUrl(resourceUrl, parameters);

            var response = await _httpClient.GetAsync(resourceUrl);
            if (response.IsSuccessStatusCode)
            {
                return new RestReponse
                {
                    IsSuccess = true,
                    Status = response.StatusCode,
                    Data = await response.Content.ReadAsStringAsync()
                };
            }
            return new RestReponse
            {
                IsSuccess = false,
                Status = response.StatusCode,
            };
        }



        private async Task<RestReponse> ProcessPostRequestAsync(string resourceUrl, Dictionary<string, string> parameters, Dictionary<string, string> queryStrings, object body)
        {
            try
            {
                resourceUrl = GenerateResourceUrl(resourceUrl, parameters);
                _httpClient.Timeout = TimeSpan.FromMinutes(10);
                var response = await _httpClient.PostAsync(resourceUrl, new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json"));
                if (response.IsSuccessStatusCode)
                {
                    return new RestReponse
                    {
                        IsSuccess = true,
                        Status = response.StatusCode,
                        Data = await response.Content.ReadAsStringAsync()
                    };
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    string errors = await response.Content.ReadAsStringAsync();
                    var errorResponse = new RestReponse
                    {
                        IsSuccess = false,
                        Status = response.StatusCode,
                    };
                    if (!string.IsNullOrEmpty(errors))
                    {
                        errorResponse.Errors = JsonConvert.DeserializeObject<List<ValidationResultModel>>(errors);
                    }


                    return errorResponse;
                }

                return new RestReponse
                {
                    IsSuccess = false,
                    Status = response.StatusCode,
                };
            }
            catch(Exception ex)
            {
                return new RestReponse
                {
                    IsSuccess = false,
                    Status = System.Net.HttpStatusCode.Accepted,
                };
            }
        }

        private async Task<RestReponse> ProcessPostRequestWithFileAsync(string resourceUrl, Dictionary<string, string> parameters, Dictionary<string, string> queryStrings, IFormFile file)
        {
            resourceUrl = GenerateResourceUrl(resourceUrl, parameters);

            byte[] data;
            using (var br = new BinaryReader(file.OpenReadStream()))
                data = br.ReadBytes((int)file.OpenReadStream().Length);


            ByteArrayContent bytes = new ByteArrayContent(data);
            bytes.Headers.ContentType = MediaTypeHeaderValue.Parse(file.ContentType);

            MultipartFormDataContent multiContent = new MultipartFormDataContent
            {
                { bytes, "file", file.FileName }
            };


            var response = await _httpClient.PostAsync(resourceUrl, multiContent);
            if (response.IsSuccessStatusCode)
            {
                return new RestReponse
                {
                    IsSuccess = true,
                    Status = response.StatusCode,
                    Data = await response.Content.ReadAsStringAsync()
                };
            }
            return new RestReponse
            {
                IsSuccess = false,
                Status = response.StatusCode,
            };
        }

        private static string GenerateResourceUrl(string resourceUrl, Dictionary<string, string> parameters)
        {
            foreach (var parameter in parameters)
            {
                resourceUrl = $"{resourceUrl}/{parameter.Value}";
            }

            bool isFirstQuery = true;
            foreach (var parameter in parameters)
            {
                if (isFirstQuery)
                {
                    isFirstQuery = false;
                    resourceUrl = $"{resourceUrl}?{parameter.Key}={parameter.Value}";
                    continue;
                }
                resourceUrl = $"{resourceUrl}&{parameter.Key}={parameter.Value}";
            }

            return resourceUrl;
        }

        public async Task<RestFileReponse> ExecuteFileRequestAsync(RestRequest restRequest)
        {
            if (!string.IsNullOrEmpty(restRequest.Token))
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", restRequest.Token);

            string resourceUrl = GenerateResourceUrl(restRequest.ResourceUrl, restRequest.Parameters);
            var response = await _httpClient.GetAsync(resourceUrl);
            if (response.IsSuccessStatusCode)
            {
                string contentFileName = response.Content.Headers.ContentDisposition?.FileName;
                if (!string.IsNullOrEmpty(contentFileName))
                {
                    char firstCharacter = contentFileName[0];
                    char lastCharacter = contentFileName[contentFileName.Length - 1];

                    if (firstCharacter == '"' && lastCharacter == '"')
                    {
                        contentFileName = contentFileName.Substring(1, contentFileName.Length - 2);
                    }
                }


                return new RestFileReponse
                {
                    IsSuccess = true,
                    Status = response.StatusCode,
                    File = await response.Content.ReadAsByteArrayAsync(),
                    FileName = contentFileName,
                    ContentType = response.Content.Headers.ContentType.ToString()
                };
            }
            return new RestFileReponse
            {
                IsSuccess = false,
                Status = response.StatusCode,
            };
        }
    }
}