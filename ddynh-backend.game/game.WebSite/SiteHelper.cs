using FW.Core.Web.WebAPI;
using game.Entity.Pay;
using game.Infrastructure;
using Newtonsoft.Json;
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
using static game.WebSite.PaymentHelper.tungyi;

namespace game.WebSite
{
    public static class SiteHelper
    {
        public static WebAPIClient Client;
        public static string PaymentEnable = ConfigurationManager.AppSettings["paymentEnable"];
        public static string APIBaseUrl = ConfigurationManager.AppSettings["GameAPIUrl"];
        public static string GameAPIId = ConfigurationManager.AppSettings["GameAPIId"];
        public static string GameAPIKey = [REDACTED]"GameAPIKey"];

        static SiteHelper()
        {
            Client = new WebAPIClient(GameAPIId, GameAPIKey, false, APIBaseUrl);
        }

        public static T GetAPIResult<T>(string route)
        {
            try
            {
                string result = Client.ClientProxy.GetStringAsync(APIBaseUrl + route).Result;
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(result);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    String.Format("url={0} \r\ntype={1} \r\nexception={2}", APIBaseUrl + route, typeof(T).ToString(), ex.Message));
            }
        }

        public static APIResult PostAPIResult<T>(string route, T value)
        {
            try
            {
                APIResult result = Client.ClientProxy.PostAsJsonAsync(APIBaseUrl + route, value).Result.Content.ReadAsAsync<APIResult>().Result;
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public static void GetAPIResult(string route)
        {
            var result = Client.ClientProxy.GetAsync(APIBaseUrl + route).Result;
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

        public static string HttpClient(string url, string method, string bodyData = "", string auth = "")
        {
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

            using (var streamWriter = new StreamWriter(req.GetRequestStream()))
            {
                streamWriter.Write(bodyData);
            }

            try
            {
                //若網站回應錯誤，就會在此引發WebException
                res = (HttpWebResponse)req.GetResponse();
            }
            catch (WebException ex)
            {
                res = (HttpWebResponse) ex.Response;
            }

            if (res != null)
            {
                Stream stream = res.GetResponseStream();
                StreamReader sr = new StreamReader(stream);
                responseData = sr.ReadToEnd().ToString();
                /*
                switch (res.StatusCode)
                {
                    case HttpStatusCode.OK:
                        //200:正常
                        //(略)
                        break;
                    case HttpStatusCode.BadRequest:
                        //400:錯誤
                        //(略)
                        break;
                    default:
                        //其他狀態
                        //(略)
                        break;
                }
                */
                res.Close();
            }
            return responseData;
        }

        /// <summary>
        /// 判断订单渠道
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>

        public static int GetGWID(CDPayOrder order, ref string tradeCode)
        {
            var amount = order.Amount;
            var payTypeID = order.PayTypeID;
            var areaID = order.UserInfo.AreaID;

            var list = GetAPIResult<List<PayGateway>>($"/Pay/GetPayGatewayByAreaID?payTypeID={payTypeID}&areaID={areaID}");
              

            //找不到任何渠道
            if (list.Count == 0) return 0;

            if (PayConfig.TungyiEnable == "1")
            {
                var listAmount = JsonConvert.DeserializeObject<Amount>(getamountlist(Guid.NewGuid().ToString().Substring(0, 5)));

                foreach(var m in list.Where(x => x.GWID == 6))
                {
                    if (!listAmount.data.payAmountInfo.Any(x => x.payType.ToString() == m.TradeCode))
                        m.Status = false;
                }

                list = list.Where(x => x.Status).ToList();
            }

            Random ran = new Random(Guid.NewGuid().GetHashCode());
            int i = ran.Next(0, list.Count());
            var GWID = list[i].GWID;
            order.GWID = GWID;

            var payGateway = list.Where(w => w.GWID.Equals(GWID) && w.TypeID.Equals(order.PayTypeID)).FirstOrDefault();
            tradeCode = payGateway.TradeCode;
            Service.CDUpdateGWID(order);
            return GWID;
        }

        /// <summary>
        /// get请求
        /// </summary>
        /// <param name="posturl"></param>
        /// <returns></returns>
        public static string GetPage(string posturl)
        {
            Stream instream = null;
            StreamReader sr = null;
            HttpWebResponse response = null;
            HttpWebRequest request = null;
            Encoding encoding = Encoding.UTF8;
            // 准备请求...
            try
            {
                // 设置参数
                request = WebRequest.Create(posturl) as HttpWebRequest;
                CookieContainer cookieContainer = new CookieContainer();
                request.CookieContainer = cookieContainer;
                request.AllowAutoRedirect = true;
                request.Method = "GET";
                request.ContentType = "application/x-www-form-urlencoded";
                //发送请求并获取相应回应数据
                response = request.GetResponse() as HttpWebResponse;
                //直到request.GetResponse()程序才开始向目标网页发送Post请求
                instream = response.GetResponseStream();
                sr = new StreamReader(instream, encoding);
                //返回结果网页（html）代码
                string content = sr.ReadToEnd();
                string err = string.Empty;
                return content;
            }
            catch (Exception ex)
            {
                string err = ex.Message;
                return string.Empty;
            }
        }

        private static object logSyncObject = new object();

        public static void Log(string log, string fileName = "log.log")
        {
            lock (logSyncObject)
            {
                var dirpath = string.Format("/logcustom/{0}/", DateTime.Now.ToString("yyyyMMdd"));
                var dir = HttpContext.Current.Server.MapPath(dirpath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                var path = HttpContext.Current.Server.MapPath(dirpath + fileName);

                StreamReader sr;
                if (!File.Exists(path))
                {
                    sr = new StreamReader(File.Create(path));
                }
                else sr = new StreamReader(path);
                using (sr)
                {
                    sr.Dispose();
                }
                example.invalid.File.AppendAllText(path, log + Environment.NewLine);
            }
        }

        /// <summary>
        /// Paylog
        /// </summary>
        /// <param name="guid">log tracecode</param>
        /// <param name="message">log message</param>
        /// <param name="filelog">log filename</param>
        public static void Log(string guid, string message, string filelog)
        {
            Log(string.Format("{0} {1} {2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), guid, message), filelog);
        }

        /// <summary>
        /// post请求到指定地址并获取返回的信息内容
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="postData">请求参数</param>
        /// <returns>返回响应内容</returns>
        public static string HttpPost(string POSTURL, string PostData)
        {           
            return HttpPost(POSTURL, PostData, "application/x-www-form-urlencoded");
        }

        /// <summary>
        /// post请求到指定地址并获取返回的信息内容(使用json格式)
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="postData">请求参数</param>
        /// <returns>返回响应内容</returns>
        public static string HttpPostJson(string POSTURL, string PostData)
        {
            return HttpPost(POSTURL, PostData, "application/json");
        }

        /// <summary>
        /// post请求到指定地址并获取返回的信息内容
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="postData">请求参数</param>
        /// <param name="ContentType"></param>
        /// <returns></returns>
        public static string HttpPost(string POSTURL, string PostData, string ContentType, string name = "UTF-8")
        {
            //发送请求的数据
            WebRequest myHttpWebRequest = WebRequest.Create(POSTURL);
            myHttpWebRequest.Method = "POST";
            UTF8Encoding encoding = new UTF8Encoding();
            byte[] byte1 = encoding.GetBytes(PostData);
            myHttpWebRequest.ContentType = ContentType;
            myHttpWebRequest.ContentLength = byte1.Length;
            Stream newStream = myHttpWebRequest.GetRequestStream();
            newStream.Write(byte1, 0, byte1.Length);
            newStream.Close();

            return HttpFormSendAndGetResponseString(myHttpWebRequest, name);
        }

        private static string HttpFormSendAndGetResponseString(WebRequest request, string name = "UTF-8")
        {
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string responseString = string.Empty;
            Encoding enc = Encoding.GetEncoding(name);
            Stream stream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(stream, enc);
            responseString = streamReader.ReadToEnd();
            return responseString;
        }


        public static string HttpWebPost(string POSTURL, string PostData, string name = "UTF-8")
        {
            //发送请求的数据
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(POSTURL);
            request.UserAgent = "PostmanRuntime/7.13.0";
            request.Method = "POST";
            UTF8Encoding encoding = new UTF8Encoding();
            byte[] byte1 = encoding.GetBytes(PostData);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byte1.Length;
            Stream newStream = request.GetRequestStream();
            newStream.Write(byte1, 0, byte1.Length);
            newStream.Close();

            return HttpFormSendAndGetResponseString(request, name);
        }

        public static string HttpGet(string sendUrl, string name = "UTF-8")
        {
            WebRequest myHttpWebRequest = WebRequest.Create(sendUrl);
            myHttpWebRequest.Method = "GET";

            //发送並接收返回的XML信息
            return HttpFormSendAndGetResponseString(myHttpWebRequest, name);
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


        public static void SetCache(string CacheKey, object objObject, DateTime Timeout)
        {
            System.Web.Caching.Cache objCache = HttpRuntime.Cache;
            objCache.Insert(CacheKey, objObject, null, Timeout, System.Web.Caching.Cache.NoSlidingExpiration);
        }

        public static void SetCache(string CacheKey, object objObject, System.TimeSpan Timeout)
        {
            System.Web.Caching.Cache objCache = HttpRuntime.Cache;
            objCache.Insert(CacheKey, objObject, null, System.Web.Caching.Cache.NoAbsoluteExpiration, Timeout);
        }

        public static object GetCache(string CacheKey)
        {
            System.Web.Caching.Cache objCache = HttpRuntime.Cache;
            return objCache[CacheKey];
        }     
    }
}