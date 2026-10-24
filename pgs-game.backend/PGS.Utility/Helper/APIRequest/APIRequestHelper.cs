using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using target.Models.Enums;
using target.Utility.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using example.invalid;
using example.invalid.Compression;
using System.Linq;
using example.invalid;
using example.invalid.Http;
using System.Reflection;
using System.Security.Authentication;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace target.Utility.Utility
{
    public partial class APIRequestHelper
    {
        public static string No_Url_Keyword = "NO_URL";


        public static ApiRequestResultModel<T> RequestApiResult<T>(RequestParameters parameter, object postBodyModel = null)
        {
            //檢查與防呆
            VerifyParameters(parameter);

            ApiRequestLogTemporaryInfoModel requestParamsTemp = SetRequestTempInfo(parameter);

            var request_config = InstanceHttpWebRequest(parameter, postBodyModel, requestParamsTemp);

            try
            {
                ApiRequestResultModel<T> resultModel = GetResponse<T>(request_config, requestParamsTemp);
                return resultModel;
            }
            catch (System.Exception ex)
            {
                return CheckErrorMessage<T>(ex, requestParamsTemp);
            }
        }


        private static ApiRequestConfigV2026Model InstanceHttpWebRequest(RequestParameters parameter, object postBodyModel, ApiRequestLogTemporaryInfoModel tempInfo)
        {
            parameter.AdvancedSetting = parameter.AdvancedSetting ?? new AdvancedSetting();

            string url = GetValidFormatUrl(parameter);

            var httpClient = InstanceHttpClient(parameter);

            HttpMethod methodType = ConvertHttpMethodType(parameter);

            var requestMessage = new HttpRequestMessage(methodType, url);

            TryAddDefaultHeader(requestMessage, "Accept", parameter.Accept);
            TryAddDefaultHeader(requestMessage, "Host", parameter.Host);
            TryAddDefaultHeader(requestMessage, "Referer", parameter.Referer);
            TryAddDefaultHeader(requestMessage, "User-Agent", parameter.UserAgent);

            if (postBodyModel != null)
            {
                string jsonString = string.Empty;

                if (parameter.AdvancedSetting.IsCamelCase)
                {
                    jsonString = JsonConvert.SerializeObject(postBodyModel, new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    });
                }
                else
                {
                    if (postBodyModel != null)
                    {
                        if (typeof(string) == postBodyModel.GetType())
                        {
                            jsonString = (string)postBodyModel;
                        }
                        else
                        {
                            jsonString = JsonConvert.SerializeObject(postBodyModel);
                        }
                    }
                    else
                    {
                        jsonString = "";
                    }
                }

                tempInfo.RequestContent = jsonString;
                requestMessage.Content = new StringContent(jsonString, Encoding.UTF8, parameter.ContentType);
            }

            //加入header
            AddHeaders(requestMessage, parameter.Headers);

            //加入cookie
            if (parameter.Cookies != null)
            {

                var cookieDomain = new Uri(parameter.BaseUrl);

                //改成V2版本後 不能指定cookieDomain了 (因為DI就決定 handler了) 不想處理了
                //var cookieContainer = new CookieContainer();
                //foreach (var item in parameter.Cookies)
                //{
                //    cookieContainer.Add(cookieDomain, new Cookie(item.Key, item.Value));
                //}

                string concateCookieValue = string.Join("; ", parameter.Cookies.Select(x => $"{x.Key}={x.Value}"));

                TryAddDefaultHeader(requestMessage, "Cookie", concateCookieValue);
            }

            return new ApiRequestConfigV2026Model
            {
                HttpClient = httpClient,
                RequestMessage = requestMessage
            };
        }

        private static HttpMethod ConvertHttpMethodType(RequestParameters parameter)
        {
            HttpMethod methodType = HttpMethod.Get;

            switch (parameter.Method)
            {
                case HttpMethodType.HttpGet:
                    methodType = HttpMethod.Get;
                    break;

                case HttpMethodType.HttpPost:
                    methodType = HttpMethod.Post;
                    break;

                case HttpMethodType.HttpPut:
                    methodType = HttpMethod.Put;
                    break;

                case HttpMethodType.HttpDelete:
                    methodType = HttpMethod.Delete;
                    break;

                default:
                    break;
            }

            return methodType;
        }

        private static ApiRequestLogTemporaryInfoModel SetRequestTempInfo(RequestParameters parameter)
        {
            var requestParameterTemp = new ApiRequestLogTemporaryInfoModel();
            requestParameterTemp.RequestParameters = parameter;
            requestParameterTemp.RequestContent = string.Empty;
            requestParameterTemp.RequestStartTime = DateTime.UtcNow;

            return requestParameterTemp;
        }


        private static void AddHeaders(HttpRequestMessage httpWebRequest, IList<KeyValuePair<string, string>> headers)
        {
            if (headers == null || headers.Count() == 0)
                return;

            foreach (var header in headers)
            {
                httpWebRequest.Headers.Remove(header.Key);
                httpWebRequest.Headers.Add(header.Key, header.Value);
            }
        }


        private static T ParseResponseToModel<T>(ApiRequestLogTemporaryInfoModel parameter, string responseText)
        {
            T converResult = default(T);

            if (typeof(T) == typeof(string))
            {
                converResult = (T)(object)responseText;
            }
            else
            {
                var setting = parameter.RequestParameters.AdvancedSetting ?? new AdvancedSetting();

                if (setting != null && setting.IsIgnoreJsonParsingColumnError)
                {
                    converResult = JsonConvert.DeserializeObject<T>(responseText, new JsonSerializerSettings
                    {
                        Error = (se, ev) =>
                        {
                            ev.ErrorContext.Handled = true;
                        }
                    });
                }
                else if (setting != null && setting.IsReturnNullWhenParsingFail)
                {
                    try
                    {
                        converResult = JsonConvert.DeserializeObject<T>(responseText);
                    }
                    catch
                    {
                        converResult = default(T);
                    }
                }
                else
                {
                    converResult = JsonConvert.DeserializeObject<T>(responseText);
                }
            }

            return converResult;
        }


        private static ApiRequestResultModel<T> GetResponse<T>(ApiRequestConfigV2026Model requestConfig, ApiRequestLogTemporaryInfoModel parameter)
        {
            if (parameter.RequestParameters.ApiUrl.Contains("player/openGame"))
            {
                string debug666 = "";
            }

            bool isParsingError = false;

            //通用cancel token自訂逾時
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromMilliseconds(parameter.RequestParameters.TimeoutMillisecond));

            HttpResponseMessage httpResponse = requestConfig.HttpClient.SendAsync(requestConfig.RequestMessage, cts.Token).Result;

            string responseText = httpResponse.Content.ReadAsStringAsync().Result;

            //如果有指定Log行為
            SetResponseInfoAndWriteLog(parameter, responseText, httpResponse);

            int statusCode = (int)httpResponse.StatusCode;

            bool isRedirectResult = statusCode == 303 || statusCode == 307;

            if(!isRedirectResult)
            {
                httpResponse.EnsureSuccessStatusCode();
            }

            T converResult = ParseResponseToModel<T>(parameter, responseText);

            var debugInfo = new DebugInfo
            {
                IsJsonParsingException = isParsingError,
                ExecutedTime = parameter.Response.ResponseTime,
                RequestParameters = parameter.RequestParameters
            };

            if (isRedirectResult && httpResponse.Headers?.Location != null)
            {
                debugInfo.RedirectLocationUrl = httpResponse.Headers.Location.ToString();
            }

            return new ApiRequestResultModel<T>
            {
                ResponseStatusCode = (int)httpResponse.StatusCode,
                ResultObject = converResult,
                RelativeUrl = GetRelativeUrl(parameter.RequestParameters),
                ResponseText = responseText,
                DebugInfo = debugInfo
            };
        }


        private static void SetResponseInfoAndWriteLog(ApiRequestLogTemporaryInfoModel parameter, string responseText, HttpResponseMessage httpResponse)
        {
            parameter.Response.ResponseStatusCode = (int)httpResponse.StatusCode;
            parameter.Response.ResponseContent = responseText;

            if (parameter.Response.ResponseStatusCode >= 400)
            {
                parameter.Response.ResponseContent = responseText;
            }

            if (parameter.RequestParameters.LogAction == null)
                return;

            //執行log行為
            ExecuteLogAction(parameter);
        }


        private static void ExecuteLogAction(ApiRequestLogTemporaryInfoModel parameter)
        {
            parameter.Response.ResponseTime = (int)(DateTime.UtcNow - parameter.RequestStartTime).TotalMilliseconds;

            if (parameter.RequestParameters.LogAction == null)
                return;

            parameter.RequestParameters.LogAction(parameter);
        }


        private static ApiRequestResultModel<T> CheckErrorMessage<T>(Exception ex, ApiRequestLogTemporaryInfoModel requestParamsTemp)
        {
            var resultModel = new ApiRequestResultModel<T>
            {
                DebugInfo = new DebugInfo
                {
                    RequestParameters = requestParamsTemp.RequestParameters
                },
                Exception = ex,
                IsTimeoutException = ex is TimeoutException || ex.InnerException is TimeoutException || (ex.InnerException?.Message ?? "").ContainsWihtoutCase("task was canceled"),
                RelativeUrl = GetRelativeUrl(requestParamsTemp.RequestParameters)
            };

            if (requestParamsTemp.Response.ResponseStatusCode == 0)
            {
                requestParamsTemp.Response.ResponseStatusCode = 500;
            }

            if (string.IsNullOrWhiteSpace(requestParamsTemp.Response.ResponseContent))
            {
                requestParamsTemp.Response.ResponseContent = "(*Exception) " + ex.Message;
            }

            ExecuteLogAction(requestParamsTemp);

            try
            {
                if (requestParamsTemp.RequestParameters.AdvancedSetting != null &&
                   requestParamsTemp.RequestParameters.AdvancedSetting.IsIgnoreJsonParsingColumnError)
                {
                    resultModel.ResultObject = requestParamsTemp.Response.ResponseContent.JsonConvertToModel<T>();
                }
            }
            catch
            {
                resultModel.ResultObject = default(T);
            }

            //若指定型別為string 特別處理
            if (typeof(T) == typeof(string))
            {
                resultModel.ResultObject = (T)(object)requestParamsTemp.Response.ResponseContent;
            }

            resultModel.ResponseText = requestParamsTemp?.Response?.ResponseContent;
            return resultModel;
        }

        /// <summary>
        /// 檢查與防呆
        /// </summary>
        private static void VerifyParameters(RequestParameters parameter)
        {
            if (string.IsNullOrWhiteSpace(parameter.BaseUrl))
                throw new System.Exception("BaseUrl Format Not Allow #001");

            if (parameter.BaseUrl.Contains("http") == false && parameter.BaseUrl.Contains("https") == false)
                throw new System.Exception("BaseUrl Format Not Allow #002");

            if (string.IsNullOrWhiteSpace(parameter.ApiUrl))
                throw new System.Exception("ApiUrl Format Not Allow #002");

            if (parameter.TimeoutMillisecond <= 0)
                parameter.TimeoutMillisecond = 120 * 1000;
        }


        private static HttpClient InstanceHttpClient(RequestParameters param)
        {
            var httpClient = RuntimeHelper.ServiceProvider.GetRequiredService<IMyHttpClientProvider>();

            var clientType = HttpClientTypeEnum.Default;

            if (param.AdvancedSetting.IsIgnoreSSLCertificateError)
            {
                clientType = HttpClientTypeEnum.IgnoreSSLError;
            }

            return httpClient.CreateClient(clientType);
        }

        private static void TryAddDefaultHeader(HttpRequestMessage http, string key, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            http.Headers.Remove(key);
            http.Headers.Add(key, value);
        }


        private static string GetRelativeUrl(RequestParameters parameters)
        {
            if (parameters == null)
                return null;

            if (parameters.ApiUrl == No_Url_Keyword)
                return parameters.BaseUrl;

            return parameters.ApiUrl;
        }


        private static string GetValidFormatUrl(RequestParameters parameters)
        {
            string url = string.Format("{0}/{1}", parameters.BaseUrl.Trim(), parameters.ApiUrl);

            if (parameters.ApiUrl == No_Url_Keyword)
                url = parameters.BaseUrl;

            url = ReplaceDuplicateUrlSymbol(url);

            if (parameters.Method != HttpMethodType.HttpGet)
                return url;

            if (parameters.Parameters.Count == 0)
                return url;

            //如果是HttpGet 把參數接在網址後面
            url = url + "?";
            int count = 0;
            foreach (var item in parameters.Parameters)
            {
                if (count > 0)
                    url += "&";

                url += string.Format("{0}={1}", item.Key, item.Value);
                count++;
            }

            return url;
        }

        /// <summary>
        /// 取代多餘的斜線符號
        /// </summary>
        public static string ReplaceDuplicateUrlSymbol(string source)
        {
            return source.ReplaceDuplicateUrlChar();
        }


    }
}