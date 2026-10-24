using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using example.invalid;
using System.Linq;
using example.invalid;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CKBankWithdrawJob
{
    class Program
    {
        /// <summary>
        /// 資料庫連線
        /// </summary>
        static string dbConnection { get { return System.Configuration.ConfigurationManager.ConnectionStrings["dbConnection"].ConnectionString; } }

        /// <summary>
        /// 執行記錄檔
        /// </summary>
        static string fileLogName { get { return ConfigurationManager.AppSettings["fileLog"]; } }
        static string app_id
        {
            get { return ConfigurationManager.AppSettings["app_id"]; }
        }
        static string app_secret
        {
            get { return ConfigurationManager.AppSettings["app_secret"]; }
        }
        static string withdrawUrl
        {
            get { return ConfigurationManager.AppSettings["withdrawUrl"]; }
        }
        static string callbackUrl
        {
            get { return ConfigurationManager.AppSettings["callbackUrl"]; }
        }

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Thread t = new Thread(job);
            t.Start();
        }

        static void job()
        {

            while (true)
            {
                try
                {
                    List<ConvertLog> data = GetConvertBankLog();
                    foreach (var item in data)
                    {
                        process(item);
                    }
                }
                catch (Exception e)
                {
                    string err = $"job exception:{e}";
                    Console.WriteLine(err);
                    FileLog(err, fileLogName);
                }
                Thread.Sleep(500);
            }
        }

        static void process(ConvertLog item)
        {
            string app_id = Program.app_id;
            string app_secret = [REDACTED];
            string notifyUrl = callbackUrl;
            string url = withdrawUrl;

            string order_num = item.Id;
            string money = item.Amount;           
            string bank_name = item.RealName;
            string bank_card = item.Account;
            string timestamp = Convert.ToInt64(DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds * 1000).ToString();  

            Dictionary<string, string> paras = new Dictionary<string, string>();

            paras.Add("app_id", app_id);
            paras.Add("app_secret", app_secret);
            paras.Add("bank_card", bank_card);    //收款方银行卡号
            paras.Add("bank_name", bank_name);    //收款方卡号账户名
            paras.Add("money", money);            //代付金额(元)
            paras.Add("order_num", order_num);    //交易业务单号
            paras.Add("notice_url", notifyUrl);   //回调通知地址
            paras.Add("timestamp", timestamp);    //时间戳

            var sort = paras.OrderBy(k => k.Key);
            paras = sort.ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);

            string str = string.Join("&", paras.Select(m => string.Format("{0}={1}", m.Key, m.Value)));          
            string pay_sha1 = SHA1(str, Encoding.UTF8).ToLower();
            string pay_md5sign = Md5(pay_sha1, Encoding.UTF8).ToUpper();

            paras.Remove("app_secret");
            paras.Add("signature", pay_md5sign);

            var param = string.Join("&", paras.Select(m => string.Format("{0}={1}", m.Key, m.Value)));

            Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")} param:{param}");
            FileLog($"ConvertId:{item.Id} param:{param}", fileLogName);

            string responseData = "";
            int httpCode = HttpPost(url, param, ref responseData);

            Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")} return:{httpCode} response:{responseData}");
            FileLog($"ConvertId:{item.Id} http:{httpCode} response:{responseData}", fileLogName);


            int intUpdStatus = 99; //預設自动化進行中
            int intTeamOrderId = Convert.ToInt32(item.Id); //兑换订单号           
            string returnCode = "null";
            string returnMsg = "null";

            JObject json;
            try
            {
                json = JObject.Parse(responseData);
            }
            catch { json = null; }
           

            //取得訪問結果與訊息
            if (json != null && json["code"] != null) returnCode = json["code"].ToString();
            if (json != null && json["msg"] != null) returnMsg = json["msg"].ToString();

            //取得打款結果與訊息
            if (json != null && returnCode.ToLower() == "1")
            {            
                intUpdStatus = 100; //自动打款失败
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")} returnMsg:{returnMsg}");
                Console.ResetColor();
            }

            FileLog($"ConvertId:{item.Id} code:{returnCode} msg:{returnMsg}", fileLogName);

            //更新結果
            UpdateConvertLogStatus(intUpdStatus, intTeamOrderId, "0", returnMsg);
        }

        //取得銀行類型 & 審核通過的清單
        static List<ConvertLog> GetConvertBankLog()
        {
            List<ConvertLog> lstConvertLog = new List<ConvertLog>();
            using (SqlConnection conn = new SqlConnection(dbConnection))
            {
                conn.Open();
                String sql = "exec dbo.PrWeb_Money_CKWithdrawGet";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    try
                    {
                        SqlDataAdapter dataAdapter = new SqlDataAdapter();
                        dataAdapter.SelectCommand = cmd;
                        DataSet ds = new DataSet();
                        dataAdapter.Fill(ds, "log");

                        DataTable table = ds.Tables[0];

                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            for (int r = 0; r < table.Rows.Count; r++)
                            {
                                ConvertLog item = new ConvertLog();
                                item.Id = table.Rows[r]["ID"].ToString();
                                item.Amount = Math.Abs(Convert.ToInt64(table.Rows[r]["Amount"].ToString())).ToString();
                                item.BankName = table.Rows[r]["BankName"].ToString();
                                item.RealName = table.Rows[r]["RealName"].ToString();
                                item.Account = table.Rows[r]["Account"].ToString();
                                item.LoginName = table.Rows[r]["LoginName"].ToString();
                                item.BankID = table.Rows[r]["BankID"].ToString();
                                lstConvertLog.Add(item);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")} GetConvertBankLog exception:{e}");
                    }

                }
            }

            return lstConvertLog;
        }

        //更新post後的返回結果
        static void UpdateConvertLogStatus(int status, int TeamOrderId, string TransId, string TransData)
        {
            using (SqlConnection conn = new SqlConnection(dbConnection))
            {
                conn.Open();
                String sql = $"exec [LogDB].[dbo].[PrWeb_Money_TungyiWithdrawCreate] @intStatus={status}, @chvTransId='{TransId}', @chvTransData='{TransData}', @intTeamOrderId={TeamOrderId}";

                FileLog(sql, fileLogName);

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        string err = $"UpdateConvertLogStatus exception:{e}";
                        FileLog(err, fileLogName);
                        Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")} {err}");
                    }

                }
            }
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
                example.invalid.File.AppendAllText(path, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " " + message + Environment.NewLine);
            }

        }
        static int HttpPost(string httpUrl, string param, ref string responseData)
        {
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

        static string Md5(string str, Encoding ec)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            return BitConverter.ToString(md5.ComputeHash(ec.GetBytes(str))).Replace("-", "");
        }

        static string SHA1(string str, Encoding ec)
        {
            SHA1 sha1 = new SHA1CryptoServiceProvider();
            byte[] source = ec.GetBytes(str);
            byte[] crypto = sha1.ComputeHash(source);
            
            return BitConverter.ToString(crypto).Replace("-", "");
        }
    }

    class ConvertLog
    {
        /// <summary>
        /// 订单
        /// </summary>
        public string Id;
        /// <summary>
        /// 金额
        /// </summary>
        public string Amount;
        /// <summary>
        /// 银行名称
        /// </summary>
        public string BankName;
        /// <summary>
        /// 帐户名
        /// </summary>
        public string RealName;
        /// <summary>
        /// 银行卡号
        /// </summary>
        public string Account;
        /// <summary>
        /// 用户正式帐号（电话）
        /// </summary>
        public string LoginName;
        /// <summary>
        /// 银行编码
        /// </summary>
        public string BankID;
    }
}
