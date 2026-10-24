using System;
using System.Configuration;
using example.invalid;
using example.invalid;
using example.invalid.Http;
using System.Text;
using FW.Core.Web.WebAPI;
using game.Infrastructure;

namespace game.OfficialWebsite
{
    public class SiteHelper
    {

        public static WebAPIClient Client;
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

    }
}