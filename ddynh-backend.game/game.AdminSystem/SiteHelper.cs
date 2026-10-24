using System;
using System.Collections.Generic;
using System.Configuration;
using example.invalid;
using System.Linq;
using example.invalid;
using example.invalid.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using FW.Core.Web.WebAPI;
using game.Infrastructure;
using game.Entity.HttpClient;
using game.Infrastructure.Utility;
using FW.Core.Helper;
using game.AdminSystem.Models;
using game.Infrastructure.Extensions;

namespace game.AdminSystem
{
    public static class SiteHelper
    {
        public static WebAPIClient Client;
        public static string APIBaseUrl = ConfigurationManager.AppSettings["GameAPIUrl"];
        public static string APIID = ConfigurationManager.AppSettings["GameAPIID"];
        public static string APIKey = [REDACTED]"GameAPIKey"];

        static SiteHelper()
        {
            Client = new WebAPIClient(APIID, APIKey, false, APIBaseUrl);
        }

        public static T GetAPIResult<T>(string route)
        {
            string responseText = string.Empty;

            try
            {
                HttpClient apiInstance = InstanceHttpClient();
                apiInstance.MaxResponseContentBufferSize = 999000000;
                responseText = apiInstance.GetStringAsync(APIBaseUrl + route).Result;
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(responseText);
            }
            catch (Exception ex)
            {
                string msg = "Requesst Exception url:" + route + typeof(T).ToString() + ex.Message + ex.InnerException?.Message;
                throw new Exception(msg);
            }
        }


        public static APIResult PostAPIResult<T>(string route, T value)
        {
            try
            {
                HttpClient apiInstance = InstanceHttpClient();
                APIResult result = apiInstance.PostAsJsonAsync(APIBaseUrl + route, value).Result.Content.ReadAsAsync<APIResult>().Result;
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void GetAPIResult(string route)
        {
            HttpClient apiInstance = InstanceHttpClient();
            var result = apiInstance.GetAsync(APIBaseUrl + route).Result;
        }

        /// <summary>
        /// Get请求，返回响应文本
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetData(string url)
        {
            var wc = new WebClient();
            return wc.DownloadString(url);
        }

        /// <summary>
        /// Post请求，返回响应文本
        /// </summary>
        /// <param name="url"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static string PostData(string url, string param)
        {
            try
            {
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
                Encoding encoding = Encoding.GetEncoding("GB2312");
                req.Timeout = 1000 * 10;

                byte[] bs = encoding.GetBytes(param);
                string responseData = String.Empty;
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded";
                req.ContentLength = bs.Length;
                using (Stream reqStream = req.GetRequestStream())
                {
                    reqStream.Write(bs, 0, bs.Length);
                    reqStream.Close();
                }

                using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream(), encoding))
                    {
                        responseData = reader.ReadToEnd().ToString();
                    }        

                    return responseData;
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Post请求，返回响应文本
        /// </summary>
        /// <param name="url"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static string PostDataUTF8(string url, string param)
        {
            try
            {
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
                Encoding encoding = Encoding.UTF8;
                req.Timeout = 1000 * 10;

                byte[] bs = encoding.GetBytes(param);
                string responseData = String.Empty;
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded";
                req.ContentLength = bs.Length;
                using (Stream reqStream = req.GetRequestStream())
                {
                    reqStream.Write(bs, 0, bs.Length);
                    reqStream.Close();
                }

                using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream(), encoding))
                    {
                        responseData = reader.ReadToEnd().ToString();
                    } 

                    return responseData;
                }
            }
            catch (Exception ex)
            {
                //return string.Empty;
                throw ex;
            }
        }

        /// <summary>
        /// Post请求，返回响应文本
        /// </summary>
        /// <param name="url"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static Response HttpClient(string url, string method, string bodyData = "", string auth = "")
        {
            Response result = new Response();
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
            HttpWebResponse res = null;
            req.Method = method;
            req.Timeout = 1000 * 10;
            UTF8Encoding encoding = new UTF8Encoding();
            byte[] bs = encoding.GetBytes(bodyData);
            string responseData = String.Empty;

            req.ContentType = "application/json";
            req.ContentLength = bs.Length;
            req.Headers["Authorization"] = auth;
            if (method != "GET") { 
                using (var streamWriter = new StreamWriter(req.GetRequestStream()))
                {

                    streamWriter.Write(bodyData);
                }
            }

            try
            {
                //若網站回應錯誤，就會在此引發WebException
                res = (HttpWebResponse)req.GetResponse();
            }
            catch (WebException ex)
            {
                res = (HttpWebResponse)ex.Response;
            }

            if (res != null)
            {
                Stream stream = res.GetResponseStream();
                StreamReader sr = new StreamReader(stream);
                responseData = sr.ReadToEnd().ToString();

                switch (res.StatusCode)
                {
                    case HttpStatusCode.OK:
                        //200:正常
                        result.StatusCode = 200;
                        break;
                    case HttpStatusCode.NoContent:
                        //204: PUT SUCCESS
                        result.StatusCode = 204;
                        break;
                    case HttpStatusCode.BadRequest:
                        //400:錯誤
                        result.StatusCode = 400;
                        break;
                    case HttpStatusCode.InternalServerError:
                        result.StatusCode = 500;
                        break;
                    default:
                        //其他狀態
                        result.StatusCode = 999;
                        break;
                }
                result.Data = responseData;
                res.Close();
            }
            return result;
            //return responseData;
        }

        public static string ProtoBufSign(SortedDictionary<string, string> dicParam)
        {
            return FW.Core.Helper.Encrypt.EncryptHelper.MD5(string.Join("", dicParam.Select(m => m.Key + m.Value).ToArray()) + ConfigData.ProtoBufMD5Key);
        }

        /// <summary>
        /// 显示金币格式
        /// </summary>
        /// <param name="_this"></param>
        /// <returns></returns>
        public static string ToCash(this long _this)
        {
            if (ConfigData.CoinShowRate > 1)
            {
                return (_this / ConfigData.CoinShowRate * 1m).ToString("N2");
            }
            else
            {
                return (_this * ConfigData.CoinShowRate).ToString();
            }
        }

        /// <summary>
        /// 获取时间字符串
        /// </summary>
        /// <param name="seconds">秒数</param>
        /// <returns></returns>
        public static string GetTimeStr(long seconds)
        {
            string str = "";
            long spare = seconds % (60 * 60);
            long hours = seconds / (60 * 60);
            long minute = spare / 60;
            if (hours > 0)
            {
                str = minute > 0 ? string.Format("{0}小时{1}分钟", hours, minute) : string.Format("{0}小时", hours);
            }
            else
            {
                str = string.Format("{0}分钟", minute);
            }
            return str;
        }

        public static List<int> GetAdminAreaIDs()
        {
            var session = System.Web.HttpContext.Current.Session;
            if (session["adminAreaIDs"] == null)
            {
                return new List<int>();
            }
            else
            {
                return session["adminAreaIDs"].ToString().Split(',').Select(m => Convert.ToInt32(m)).ToList();
            }
        }
        /// <summary>
        /// 获取后台用户能管理的平台IDs
        /// </summary>
        /// <returns></returns>
        public static List<int> GetPlatformIDs()
        {
            var session = System.Web.HttpContext.Current.Session;
            if (session["adminPlatformIDs"] == null || string.IsNullOrWhiteSpace(session["adminPlatformIDs"].ToString()))
            {
                return new List<int>();
            }
            else
            {
                return session["adminPlatformIDs"].ToString().Split(',').Select(m => Convert.ToInt32(m)).ToList();
            }

        }



        /// <summary>
        /// 取得使用者IP
        /// </summary>
        /// <returns></returns>
        public static string GetIP()
        {
            string ip;
            string trueIP = string.Empty;

            //先取得是否有經過代理伺服器
            ip = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ip))
            {
                //將取得的 IP 字串存入陣列
                string[] ipRange = ip.Split(',');

                //比對陣列中的每個 IP
                for (int i = 0; i < ipRange.Length; i++)
                {
                    //剔除內部 IP 及不合法的 IP 後，取出第一個合法 IP
                    if (ipRange[i].Trim().Substring(0, 3) != "10." &&
                        ipRange[i].Trim().Substring(0, 5) != "127.0" &&
                        ipRange[i].Trim().Substring(0, 7) != "192.168" &&
                        ipRange[i].Trim().Substring(0, 7) != "172.16." &&
                        CheckIP(ipRange[i].Trim()))
                    {
                        trueIP = ipRange[i].Trim();
                        break;
                    }
                }

            }
            else
            {
                //沒經過代理伺服器，直接使用 ServerVariables["REMOTE_ADDR"]
                //並經過 CheckIP( ) 的驗證
                trueIP = CheckIP(HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"]) ?
                    HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"] : "";
            }

            return trueIP;
        }



        public static ApiRequestResultModel<T> PostGameApi<T>(string suffixRouteUrl , object model)
        {
            var para = new RequestParameters
            {
                BaseUrl = APIBaseUrl,
                ApiUrl = suffixRouteUrl,
                Method = HttpMethodType.HttpPost
            };

            SetCommonHeader(para);

            var result = APIRequestHelper.RequestApiResult<T>(para, model);
            return result;
        }

        public static ApiRequestResultModel<T> PutGameApi<T>(string suffixRouteUrl, object model)
        {
            var para = new RequestParameters
            {
                BaseUrl = APIBaseUrl,
                ApiUrl = suffixRouteUrl,
                Method = HttpMethodType.HttpPost
            };

            SetCommonHeader(para);

            var result = APIRequestHelper.RequestApiResult<T>(para, model);
            return result;
        }

        public static ApiRequestResultModel<T> HttpGetGameApi<T>(string suffixRouteUrl)
        {
            var para = new RequestParameters
            {
                BaseUrl = APIBaseUrl,
                ApiUrl = suffixRouteUrl,
                Method = HttpMethodType.HttpGet
            };

            SetCommonHeader(para);

            var result = APIRequestHelper.RequestApiResult<T>(para);
            return result;
        }




        /// <summary>
        /// 檢查 IP 是否合法
        /// </summary>
        public static bool CheckIP(string strPattern)
        {

            // regular: ^\d{1,3}[\.]\d{1,3}[\.]\d{1,3}[\.]\d{1,3}$
            Regex regex = new Regex("^\\d{1,3}[\\.]\\d{1,3}[\\.]\\d{1,3}[\\.]\\d{1,3}$");
            Match m = regex.Match(strPattern);
            return m.Success;
        }



        public static string GeneratePageQueryString(RazorPagingRequestModel paging)
        {
            if (paging == null)
                paging = new RazorPagingRequestModel();

            if (paging.P <= 0)
                paging.P = 1;

            if (paging.PageSize <= 0)
                paging.PageSize = 20;

            return $"pageSize={paging.PageSize}&pageIndex={paging.P}";
        }


        public static string GeneratePageQueryString(JQueryDataTablePagingModel paging)
        {
            if (paging == null)
                paging = new JQueryDataTablePagingModel();

            //此套件數值從0開始
            if (paging.Start <= 0)
                paging.Start = 0;

            if (paging.Length <= 0)
                paging.Length = 20;

            int pageIndex = 1 + (int)(paging.Start / paging.Length);

            if (paging.Start % paging.Length != 0)
                pageIndex = pageIndex + 1;

            return $"pageSize={paging.Length}&pageIndex={pageIndex}";
        }


        public static T GenerateQueryFilterModel<T>(JQueryDataTablePagingModel paging) where T: PaginationParameter , new()
        {
            if (paging == null)
                paging = new JQueryDataTablePagingModel();

            //此套件數值從0開始
            if (paging.Start <= 0)
                paging.Start = 0;

            if (paging.Length <= 0)
                paging.Length = 20;

            int pageIndex = 1 + (int)(paging.Start / paging.Length);

            if (paging.Start % paging.Length != 0)
                pageIndex = pageIndex + 1;

            var instance = paging.ReturnInjecteObject<T>();
            instance.PageSize = paging.Length;
            instance.PageIndex = pageIndex;
            return instance;
        }


        public static string GetCurrentUserAdminID()
        {
            try
            {
                var user = FW.Core.Engine.EngineContext.Current.Resolve<FW.Core.Auth.AuthManager>().CurrentUser;

                if (user == null)
                    return string.Empty;

                return user.UserID;

            }
            catch (Exception ex)
            {

            }

            return string.Empty;
        }
        

        public static string GeneratePageQueryString(PaginationParameter paging)
        {
            if (paging == null)
                paging = new PaginationParameter();

            if (paging.PageIndex <= 0)
                paging.PageIndex = 1;

            if (paging.PageSize <= 0)
                paging.PageSize = SystemConst.DefaultPageSize;

            return $"pageSize={ paging.PageSize}&pageIndex={paging.PageIndex}";
        }



        public static string GenerateQueryStringParameter<T>(T model) where T : class
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();


            if (model == null)
                return string.Empty;

            foreach (var item in model.GetType().GetProperties())
            {
                object myValue = item.GetValue(model);

                if (myValue == null)
                    continue;

                //如果是分頁參數 不透過此function轉成queryString
                if (item.DeclaringType == typeof(PaginationParameter))
                    continue;

                if (item.PropertyType == typeof(DateTime) || item.PropertyType == typeof(DateTime?))
                {
                    myValue = ((DateTime)myValue).ToString("yyyy-MM-dd HH:mm:ss").UrlEncode();
                }

                dic.Add(item.Name, myValue.ToString());
            }

            return string.Join("&", dic.Select(x => $"{x.Key}={x.Value}"));
        }


        public static string GetCurrentUserName()
        {
            try
            {
                var user = FW.Core.Engine.EngineContext.Current.Resolve<FW.Core.Auth.AuthManager>().CurrentUser;

                if (user == null)
                    return string.Empty;

                return user.LoginName;

            }
            catch(Exception ex)
            {

            }

            return string.Empty;
        }

        public static string GetRequestUrl()
        {
            try
            {
                var req = System.Web.HttpContext.Current.Request;

                if (req == null)
                    return string.Empty;

                return req.Url.AbsolutePath;

            }
            catch (Exception ex)
            {

            }

            return string.Empty;
        }


        private static void SetCommonHeader(RequestParameters para)
        {
            para.AddHeaderData(SystemConst.AdminSys_AlwaysHeaderUserName, GetCurrentUserName());
            para.AddHeaderData(SystemConst.AdminSys_AlwaysHeaderMemo, GetRequestUrl());
        }



        private static HttpClient InstanceHttpClient()
        {
            HttpClient apiInstance = Client.ClientProxy;

            apiInstance.DefaultRequestHeaders.Add(SystemConst.AdminSys_AlwaysHeaderMemo, GetRequestUrl());
            apiInstance.DefaultRequestHeaders.Add(SystemConst.AdminSys_AlwaysHeaderUniqueRequestID, GetUniqueRequestIDHeader());
            apiInstance.DefaultRequestHeaders.Add(SystemConst.AdminSys_AlwaysHeaderUserName, GetCurrentUserName());
            apiInstance.DefaultRequestHeaders.Add(SystemConst.AdminSys_AlwaysHeaderAdminID, GetCurrentUserAdminID());

            return apiInstance;
        }


        private static string GetUniqueRequestIDHeader()
        {
            var skipSetting = (System.Web.HttpContext.Current.Items["UniqueRequestID"]);

            if (skipSetting == null)
                return string.Empty;

            return skipSetting.ToString();
        }





    }
}