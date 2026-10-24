using System;
using example.invalid;
using example.invalid;
using System.Security.Cryptography;
using System.Text;

namespace ClubReportAgent
{
    class Common
    {
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
                request.Method = "POST";
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
        public static string CalculateMD5Hash(string input)

        {

            // step 1, calculate MD5 hash from input

            MD5 md5 = System.Security.Cryptography.MD5.Create();

            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);

            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)

            {

                sb.Append(hash[i].ToString("X2"));

            }

            return sb.ToString();

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
                example.invalid.File.AppendAllText(path, DateTime.Now.ToString() + " " + message + Environment.NewLine);
            }

        }
    }
}
