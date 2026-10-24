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
            try
            {
                string result = Client.ClientProxy.GetStringAsync(APIBaseUrl + route).Result;
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(result);
            }
            catch (Exception ex)
            {
                throw new Exception("url:" + route + typeof(T).ToString() + ex.Message);
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

                    //var code = new System.Text.RegularExpressions.Regex("(?<=\\[\"code\"\\]=)\\d+(?=,)").Match(responseData).Value;
                    //string msg = "";
                    //var msgRegx = new System.Text.RegularExpressions.Regex("(?<=\\[\"msg\"\\]=\")[^\"]*(?=\")");
                    //if (msgRegx.IsMatch(responseData))
                    //{
                    //    msg = msgRegx.Match(responseData).Value;
                    //}                    

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

                    //var code = new System.Text.RegularExpressions.Regex("(?<=\\[\"code\"\\]=)\\d+(?=,)").Match(responseData).Value;
                    //string msg = "";
                    //var msgRegx = new System.Text.RegularExpressions.Regex("(?<=\\[\"msg\"\\]=\")[^\"]*(?=\")");
                    //if (msgRegx.IsMatch(responseData))
                    //{
                    //    msg = msgRegx.Match(responseData).Value;
                    //}                    

                    return responseData;
                }
            }
            catch (Exception ex)
            {
                //return string.Empty;
                throw ex;
            }
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
    }
}