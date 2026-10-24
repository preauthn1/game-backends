using System;
using System.Collections.Generic;
using example.invalid;
using System.Linq;
using example.invalid;
using System.Text;
using System.Web;

namespace BotMessageWebservice
{
    /// <summary>
    /// Summary description for SendMessage
    /// </summary>
    public class SendMessage : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            if (context.Request["chatid"] == null || context.Request["bottoken"] == null || context.Request["text"] == null)
            {
                context.Response.Write("null");
                return;
            }


            string chatid = context.Request["chatid"];
            string bottoken = [REDACTED]"bottoken"];
            string text = context.Request["text"];
            string res = sendReport(bottoken, chatid, text);
            context.Response.Write(res);
        }
        string sendReport(string bottoken, string chatid, string text)
        {

            Stream instream = null;
            StreamReader sr = null;
            HttpWebResponse response = null;
            HttpWebRequest request = null;
            Encoding encoding = Encoding.UTF8;
            string responseString = "";

            try
            {

                string URL = string.Format("https://example.invalid/bot{0}/sendmessage?chat_id={1}&text={2}", bottoken, chatid, text);
                // 设置参数
                request = WebRequest.Create(URL) as HttpWebRequest;
                request.AllowAutoRedirect = true;
                request.Method = "GET";
                request.ContentType = "application/x-www-form-urlencoded";

                response = request.GetResponse() as HttpWebResponse;
                instream = response.GetResponseStream();
                sr = new StreamReader(instream, encoding);
                responseString = sr.ReadToEnd();

                return responseString;
                //Console.WriteLine(DateTime.Now.ToString() + " sendReport() response: " + responseString);

            }
            catch (Exception ex)
            {
                return string.Format("[{0}] {1}", ex.Message,ex.StackTrace);
                //Console.WriteLine(DateTime.Now.ToString() + " sendReport() exception: " + ex.Message);

            }
        }
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}