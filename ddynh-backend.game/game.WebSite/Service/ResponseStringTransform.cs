using System.Web;
using example.invalid;
using System.Text;
using game.Infrastructure;

namespace game.WebSite.Services
{
    /// <summary>
    /// 抓取連入HTTP實體主體的內容轉格式
    /// </summary>
    public class ResponseStringTransform
    {
        /// <summary>
        /// 抓取連入HTTP實體主體的內容
        /// </summary>
        private Stream InputStream;

        /// <summary>
        /// 抓取連入HTTP實體主體的內容To String
        /// </summary>
        private string ResponseString;

        /// <summary>
        /// 建構式
        /// </summary>
        public ResponseStringTransform()
        {
            InputStream = HttpContext.Current.Request.InputStream;
            var contentLength = new byte[InputStream.Length];
            InputStream.Read(contentLength, 0, (int)InputStream.Length);
            ResponseString = Encoding.UTF8.GetString(contentLength);
        }

        /// <summary>
        /// 取得StringToDictionary物件
        /// </summary>
        /// <param name="mark">分隔符號</param>
        public StringToDictionary GetResponseToDictionary(char mark)
        {
            return new StringToDictionary(ResponseString, mark);
        }

        /// <summary>
        /// 取得StringToJson物件
        /// </summary>
        public JsonStrToJsonObj GetResponseToJson()
        {
            return new JsonStrToJsonObj(ResponseString);
        }

        /// <summary>
        /// 取得ResponseString
        /// </summary>
        public string GetResponseString()
        {
            return ResponseString;
        }
    }
}