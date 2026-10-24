using Microsoft.Extensions.DependencyInjection;
using target.Models.Enums;
using System;
using System.Collections.Concurrent;
using example.invalid.Http;

namespace target.Utility.Utility
{
    public interface IMyHttpClientProvider
    {
        HttpClient CreateClient(HttpClientTypeEnum type);
    }

    public class MyHttpClientProvider : IMyHttpClientProvider
    {
        private IHttpClientFactory _factory;

        public MyHttpClientProvider(IHttpClientFactory factory)
        {
            _factory = factory;
        }

        public HttpClient CreateClient(HttpClientTypeEnum type)
        {
            //如果這邊出錯誤 代表startup 沒有註冊相關 DI。請參考 RuntimeHelper.RegisterHttpClient
            return _factory.CreateClient(type.ToString());
        }

    }

}