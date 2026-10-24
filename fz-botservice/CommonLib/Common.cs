using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using example.invalid;
using System.Linq;
using example.invalid;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommonLib
{
    public class Common
    {
        static string sendReport(string bottoken, string chatid, string text)
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
                return string.Format("[{0}] {1}", ex.Message, ex.StackTrace);
                //Console.WriteLine(DateTime.Now.ToString() + " sendReport() exception: " + ex.Message);

            }
        }

        public static void pushMessage(string botToken, string[] chatid, string data)
        {
            try
            {

                for (int cid = 0; cid < chatid.Length; cid++)
                {
                    var responseString = sendReport(botToken, chatid[cid], data);
                    

                    Console.WriteLine(DateTime.Now.ToString() + " sendAlert() response: " + responseString);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now.ToString() + " sendAlert() exception: " + ex.Message);
            }
        }

        public static int HttpPost(string httpUrl, string param, ref string responseData)
        {
            //設定連線數
            example.invalid.ServicePointManager.DefaultConnectionLimit = 200;

            Encoding encoding = Encoding.UTF8;
            HttpWebRequest request = null;
            int returnCode = 0;

            try
            {
                request = HttpWebRequest.Create(httpUrl) as HttpWebRequest;
                request.Method = "POST";   // 方法
                request.ContentType = "application/x-www-form-urlencoded";

                byte[] bs = Encoding.UTF8.GetBytes(param);

                using (Stream reqStream = request.GetRequestStream())
                {
                    reqStream.Write(bs, 0, bs.Length);
                    reqStream.Flush();
                }

                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    StreamReader sr = new StreamReader(response.GetResponseStream(), encoding);
                    responseData = sr.ReadToEnd();
                    sr.Close();
                    returnCode = (int)response.StatusCode;
                }
            }
            catch (Exception e)
            {
                responseData = e.Message;
                returnCode = 500;
            }
            finally
            {
                if (request != null)
                    request.Abort();

                request = null;
            }
            return returnCode;
        }

        public static int HttpGet(string httpUrl, ref string responseData)
        {
            //設定連線數
            example.invalid.ServicePointManager.DefaultConnectionLimit = 200;

            HttpWebRequest request = null;
            int returnCode = 0;

            try
            {
                //設定連線數
                example.invalid.ServicePointManager.DefaultConnectionLimit = 200;
                request = WebRequest.Create(httpUrl) as HttpWebRequest;
                request.AllowAutoRedirect = true;
                request.Method = "GET";
                //request.Timeout = 5 * 1000; //5sec

                // 取得回應資料
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    using (StreamReader sr = new StreamReader(response.GetResponseStream(),Encoding.UTF8))
                    {
                        responseData = sr.ReadToEnd();
                        returnCode = (int)response.StatusCode;
                    }
                }

            }
            catch (Exception e)
            {
                responseData = e.Message;
                returnCode = 500;
            }
            finally
            {
                if (request != null)
                    request.Abort();

                request = null;
            }
            return returnCode;
        }

        private static object logSyncObject = new object();
        public static void FileLog(string message, string filelog)
        {

            lock (logSyncObject)
            {
                var dirpath = string.Format("/logcustom/{0}/", DateTime.Now.ToString("yyyyMMdd"));
                string dir = System.Environment.CurrentDirectory + dirpath;
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                var path = System.Environment.CurrentDirectory + dirpath + filelog;

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
                example.invalid.File.AppendAllText(path, message + Environment.NewLine);
            }

        }
        static async Task<int> FileLogAsync(string filePath, string filename, string text)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException("filePath");

            if (string.IsNullOrEmpty(text))
                throw new ArgumentNullException("text");


            if (!Directory.Exists(filePath))
            {
                lock (logSyncObject)
                {
                    Directory.CreateDirectory(filePath);
                }
            }

            var path = filePath + filename;
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            Int32 offset = 0;
            Int32 sizeOfBuffer = 4096;

            //Console.WriteLine(path);

            using (FileStream fileStream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite, bufferSize: sizeOfBuffer, useAsync: true))
            {
                await fileStream.WriteAsync(buffer, offset, buffer.Length);
            }

            return 1;

        }
        public static void PrintAndLog(string printstr, string fileLogName)
        {
            Console.WriteLine(printstr);
            Common.FileLog(printstr, fileLogName);
        }
        public static async Task<int> PrintAndLogAsync(string printstr, string fileLogName)
        {
            Console.WriteLine(printstr);
            var path = System.Environment.CurrentDirectory + $@"\logcustom\{DateTime.Now.ToString("yyyyMMdd")}\";
            return await Common.FileLogAsync(path, fileLogName, printstr);
        }
    }

    public class AsyncHttp
    {
        private static ManualResetEvent allDone = new ManualResetEvent(false);
        public static void GetRequestStreamCallback(IAsyncResult asynchronousResult)
        {

            HttpRequestState requestState = null;
            try
            {

                // State of request is asynchronous.
                requestState = (HttpRequestState)asynchronousResult.AsyncState;
                HttpWebRequest request = requestState.Request;

                // End the operation
                Stream postStream = request.EndGetRequestStream(asynchronousResult);
                string postData = requestState.RequestParams;

                // Convert the string into a byte array.
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);

                // Write to the request stream.
                postStream.Write(byteArray, 0, postData.Length);
                postStream.Close();

                // Start the asynchronous operation to get the response
                request.BeginGetResponse(new AsyncCallback(GetResponseCallback), requestState);
            }
            catch (WebException e)
            {
                requestState.ResponseData = String.Format("GetRequestStreamCallback WebException. Message:{0} Status:{1}", e.Message, e.Status); ;
                requestState.DisplayText = requestState.ErrorText;
                requestState.TimeTaken = (int)(DateTime.Now - requestState.RequestTime).TotalMilliseconds;

                if (e.Response == null)
                {
                    requestState.HttpStatusCode = (int)e.Status;
                    requestState.HttpStatusName = e.Status.ToString();
                }
                else
                {
                    requestState.HttpStatusCode = (int)((HttpWebResponse)e.Response).StatusCode;
                    requestState.HttpStatusName = ((HttpWebResponse)e.Response).StatusCode.ToString();
                }

                //print data
                Common.PrintAndLog(requestState.PrintData, requestState.FileLogName);
                Common.pushMessage(requestState.BotToken, requestState.ChatID, requestState.PrintData);

            }
            catch (Exception e)
            {
                requestState.ResponseData = String.Format("GetRequestStreamCallback Exception. Message:{0}", e.Message);
                requestState.HttpStatusCode = 500;
                requestState.DisplayText = requestState.ErrorText;
                requestState.TimeTaken = (int)(DateTime.Now - requestState.RequestTime).TotalMilliseconds;

  
                //print data
                Common.PrintAndLog(requestState.PrintData, requestState.FileLogName);
            }

        }

        public static void GetResponseCallback(IAsyncResult asynchronousResult)
        {
            HttpRequestState requestState = null;
            HttpWebResponse response = null;

            try
            {
                string responseString = "";

                // State of request is asynchronous.
                requestState = (HttpRequestState)asynchronousResult.AsyncState;
                HttpWebRequest request = requestState.Request;

                // End the operation
                response = (HttpWebResponse)request.EndGetResponse(asynchronousResult);
                bool GB2312 = response.Headers["Content-Type"] == null ? false : 
                    (response.Headers["Content-Type"].Contains("gb2312") ? true : false);

                using (Stream streamResponse = response.GetResponseStream())
                {
                    Encoding ec = GB2312 ? Encoding.GetEncoding("gb2312") : Encoding.UTF8;
                    using (StreamReader streamRead = new StreamReader(streamResponse, ec))
                    {
                        responseString = streamRead.ReadToEnd();
                    }
                }
                requestState.ResponseData = responseString;
                requestState.HttpStatusName = response.StatusCode.ToString();
                requestState.HttpStatusCode = (int)response.StatusCode;
                requestState.DisplayText = requestState.NormalText;

            }
            catch (WebException e)
            {
                requestState.ResponseData = String.Format("GetResponseCallback WebException. Message:{0}", e.Message);

                if (e.Status == WebExceptionStatus.RequestCanceled)
                {
                    requestState.HttpStatusCode = 408;
                    requestState.HttpStatusName = "Timeout. " + e.Status;
                }
                else if (e.Response == null)
                {
                    requestState.HttpStatusCode = (int)e.Status;
                    requestState.HttpStatusName = e.Status.ToString();
                }

                else
                {
                    requestState.HttpStatusCode = (int)((HttpWebResponse)e.Response).StatusCode;
                    requestState.HttpStatusName = ((HttpWebResponse)e.Response).StatusCode.ToString();
                }
                requestState.DisplayText = requestState.ErrorText;

            }
            catch (Exception e)
            {
                requestState.ResponseData = String.Format("GetResponseCallback Exception. Message:{0}", e.Message); ;
                requestState.HttpStatusCode = 500;
                requestState.HttpStatusName = "Exception";
                requestState.DisplayText = requestState.ErrorText;

            }
            finally
            {
                if (response != null) {
                    response.Close();
                    response = null;

                    //temp log
                    //Common.PrintAndLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + requestState.SessionToken + " close " + requestState.Url, requestState.FileLogName);

                }
            }

            //calc request time
            requestState.TimeTaken = (int)(DateTime.Now - requestState.RequestTime).TotalMilliseconds;

            allDone.Set();
        }



    }
    

    public class HttpRequestState
    {

        public HttpRequestState()
        {
            RequestParams = "";
            Request = null;
            ResponseData = "";
            HttpStatusCode = 0;
            DefaultTimeoutSec = 5;
            RequestTime = DateTime.Now;
        }
        public HttpWebRequest Request; // This class stores the State of the request.
        public string Url { get; set; }

        public string ResponseData { get; set; } //访问后回传内容
        public int HttpStatusCode { get; set; } //http 回传状态码
        public string HttpStatusName { get; set; } //http 回传状态名称
        public string RequestParams { get; set; } //post参数
        public string DisplayText { get; set; } //项目名称
        public string ErrorText { get; set; } //错误显示字眼
        public string NormalText { get; set; } //正常显示字眼
        public string FileLogName { get; set; }
        public string BotToken { get; set; }
        public string[] ChatID { get; set; }
        public int TimeTaken { get; set; } //访问结束花费时间
        public DateTime RequestTime { get; set; } //开始访问时间
        public string PrintData //log 显示格式
        {
            get
            {
                return string.Format("{0} {7} {1} {2} status:{3} {4} response:{5} time:{6}ms",
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                DisplayText,
                Request.RequestUri.AbsoluteUri,
                HttpStatusCode,
                HttpStatusName,
                ResponseData,
                TimeTaken, 
                SessionToken);
            }
        }
        public int DefaultTimeoutSec { get; set; } //预设访问timeout时间
        public int Quality { get; set; }
        public string QualityArrow { get; set; }
        public string ItemName { get; set; } //url定义的名称
        public bool SessionContinue { get; set; }
        public string SessionToken { get; set; }
    }

}
