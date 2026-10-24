using System;
using example.invalid;
using example.invalid;
using System.Text;
using System.Web;

namespace game.Infrastructure
{
    public static class SystemHelper
    {
        /// <summary>
        /// 用户IP地址
        /// </summary>
        public static string UserIPAddress
        {
            get
            {
                var r = HttpContext.Current.Request;
                var REMOTE_ADDR = r.ServerVariables["REMOTE_ADDR"];
                var HTTP_X_FORWARDED_FOR = r.ServerVariables["HTTP_X_FORWARDED_FOR"];
                var realIp = r.Headers.Get("RemoteIP");

                if (!string.IsNullOrWhiteSpace(HTTP_X_FORWARDED_FOR))
                {
                    string x_ip = "0.0.0.0";
                    //將取得的 IP 字串存入陣列
                    string[] ipRange = HTTP_X_FORWARDED_FOR.Split(',');

                    //比對陣列中的每個 IP
                    for (int i = 0; i < ipRange.Length; i++)
                    {
                        //剔除內部 IP 及不合法的 IP 後，取出第一個合法 IP
                        if (ipRange[i].Trim().Substring(0, 3) != "10." &&
                            ipRange[i].Trim().Substring(0, 5) != "127.0" &&
                            ipRange[i].Trim().Substring(0, 7) != "192.168" &&
                            ipRange[i].Trim().Substring(0, 7) != "172.16.")
                        {
                            x_ip = ipRange[i].Trim();
                            break;
                        }
                    }

                    return x_ip;
                }

                if (!string.IsNullOrWhiteSpace(REMOTE_ADDR))
                {
                    return REMOTE_ADDR;
                }

                if (!string.IsNullOrWhiteSpace(realIp))
                {
                    return realIp;
                }
                else
                {
                    return r.UserHostAddress;
                }
            }
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


        public static bool IsLocalhost(int? port = null)
        {
            string keyword = string.Format("http://localhost:{0}", (port.HasValue ? port.Value.ToString() : string.Empty));

            var request = System.Web.HttpContext.Current.Request;

            if (request == null)
                return false;

            return (request.Url.AbsoluteUri.Contains(keyword));
        }




    }
}
