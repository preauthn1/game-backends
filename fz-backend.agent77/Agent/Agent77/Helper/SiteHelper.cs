using FW.Core.Web.WebAPI;
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

namespace Agent77.Helper
{
    public static class SiteHelper
    {  
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

            //发送成功后接收返回的XML信息
            HttpWebResponse response = (HttpWebResponse)myHttpWebRequest.GetResponse();
            string lcHtml = string.Empty;
            Encoding enc = Encoding.GetEncoding(name);
            Stream stream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(stream, enc);
            lcHtml = streamReader.ReadToEnd();
            return lcHtml;
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

            //发送成功后接收返回的XML信息
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string responseString = string.Empty;
            Encoding enc = Encoding.GetEncoding(name);
            Stream stream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(stream, enc);
            responseString = streamReader.ReadToEnd();
            return responseString;
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

        /// <summary>
        /// 显示金币格式
        /// </summary>
        /// <param name="_this"></param>
        /// <returns></returns>
        public static string ToCash(this long _this)
        {
            if (CoinShowRate > 1)
            {
                return (_this / CoinShowRate * 1m).ToString("N2");
            }
            else
            {
                return (_this * CoinShowRate).ToString();
            }
        }

        public static string ToShortDate(this DateTime? _this)
        {
            if (_this.HasValue)
            {
                return _this.Value.ToString("yyyy-MM-dd");
            }
            else
            {
                return string.Empty;
            }
        }

        public static string ToLongDateString(this DateTime? _this)
        {
            if (_this.HasValue)
            {
                return _this.Value.ToString("yyyy-MM-dd HH:mm");
            }
            else
            {
                return string.Empty;
            }
        }
        
        public static Decimal CoinShowRate
        {
            get
            {
                return Convert.ToDecimal(System.Configuration.ConfigurationManager.AppSettings["CoinShowRate"]);
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
    }
}