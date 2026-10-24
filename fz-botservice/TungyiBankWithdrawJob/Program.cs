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

namespace TungyiBankWithdrawJob
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
        static string mch_id
        {
            get { return ConfigurationManager.AppSettings["mchid"]; }
        }
        static string mch_key
        {
            get { return ConfigurationManager.AppSettings["mchkey"]; }
        }
        static string withdrawUrl
        {
            get { return ConfigurationManager.AppSettings["withdrawUrl"]; }
        }
        static string callbackUrl
        {
            get { return ConfigurationManager.AppSettings["callbackUrl"]; }
        }
        static bool ConvertDebug
        {
            get { return Convert.ToBoolean(ConfigurationManager.AppSettings["convert_Debug"]); }
        }
        static string ConvertGWID
        {
            get { return ConfigurationManager.AppSettings["convert_gwid"]; }
        }
        static string ConvertType
        {
            get { return ConfigurationManager.AppSettings["convert_type"]; }
        }

        static void Main(string[] args)
        {
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
                        if (ConvertDebug)
                            test(item);
                        else
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
            string mchid = mch_id;
            string key = mch_key;
            string notifyUrl = callbackUrl;
            string url = withdrawUrl;

            string teamOrderId = item.Id;
            string amount = item.Amount;
            string identity = "";
            string mobile = item.LoginName;
            string bankCode = item.BankID;
            string bankName = item.BankName;
            string cardType = "debit";
            string accountType = "B2C";
            string accountName = item.RealName;
            string accountNo = item.Account;

            Dictionary<string, string> paras = new Dictionary<string, string>();

            paras.Add("accountId", mchid);
            paras.Add("teamOrderId", teamOrderId);
            paras.Add("amount", amount);
            paras.Add("identity", identity);
            paras.Add("mobile", mobile);
            paras.Add("bankCode", bankCode);
            paras.Add("bankName", bankName);
            paras.Add("cardType", cardType);
            paras.Add("accountType", accountType);
            paras.Add("accountName", accountName);
            paras.Add("accountNo", accountNo);
            paras.Add("notifyUrl", notifyUrl);

            var sort = paras.OrderBy(k => k.Key);
            paras = sort.ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);
            paras.Add("key", key);

            string pay_md5sign = Md5(string.Join("&", paras.Select(m => string.Format("{0}={1}", m.Key, m.Value))), Encoding.UTF8);

            paras.Remove("key");
            paras.Add("sign", pay_md5sign);

            var param = string.Join("&", paras.Select(m => string.Format("{0}={1}", m.Key, m.Value)));

            Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")} param:{param}");
            FileLog($"ConvertId:{item.Id} param:{param}", fileLogName);

            string responseData = "";
            int httpCode = HttpPost(url, param, ref responseData);

            Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")} return:{httpCode} response:{responseData}");
            FileLog($"ConvertId:{item.Id} http:{httpCode} response:{responseData}", fileLogName);


            int intUpdStatus = 99; //預設自动化進行中
            int intTeamOrderId = Convert.ToInt32(item.Id); //兑换订单号
            string returnStatus = ""; //支付中心回调的状态
            string returnOrderId = ""; //支付中心订单号
            string returnCode = "null";
            string returnMsg = "null";

            JObject json;
            try
            {
                json = JObject.Parse(responseData);
            }
            catch { json = null; }


            //可能網路問題或其他因素沒有回傳內容, 但中心可能已經在處理中, 一律視為自动化進行中
            if (json == null || json["data"] == null)
            {
                intUpdStatus = 99; //自动化進行中
                returnStatus = "";
                returnOrderId = "";
            }

            //取得訪問結果與訊息
            if (json != null && json["code"] != null) returnCode = json["code"].ToString();
            if (json != null && json["msg"] != null) returnMsg = json["msg"].ToString();

            //取得打款結果與訊息
            if (json != null && json["data"] != null && json["data"]["status"] != null)
            {
                returnStatus = json["data"]["status"].ToString();
                if (returnStatus.ToLower() == "success") intUpdStatus = 3; //打款完成
                else if (returnStatus.ToLower() == "pending") intUpdStatus = 99; //自动打款程序进行中
                else if (returnStatus.ToLower() == "fail") intUpdStatus = 100; //自动打款失败

                returnOrderId = json["data"]["orderId"].ToString();
            }

            Console.ForegroundColor = intUpdStatus == 3 || intUpdStatus == 99 ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")} ConvertId:{item.Id} intUpdStatus:{intUpdStatus} returnMsg:{returnMsg}");
            Console.ResetColor();

            FileLog($"ConvertId:{item.Id} code:{returnCode} msg:{returnMsg}", fileLogName);

            //更新結果
            UpdateConvertLogStatus(intUpdStatus, intTeamOrderId, returnOrderId, returnStatus);

        }

        static void test(ConvertLog item)
        {
            int intUpdStatus = 99; //預設自动化進行中
            int intTeamOrderId = Convert.ToInt32(item.Id); //兑换订单号            
            string returnMsg = "success";
            string returnTranId = "0";
            string returnCode = "000000";            

            Console.ForegroundColor = intUpdStatus == 3 || intUpdStatus == 99 ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")} ConvertId:{item.Id} intUpdStatus:{intUpdStatus} returnMsg:{returnMsg}");
            Console.ResetColor();

            FileLog($"ConvertId:{item.Id} code:{returnCode} msg:{returnMsg}", fileLogName);

            //更新結果
            UpdateConvertLogStatus(intUpdStatus, intTeamOrderId, returnTranId, returnMsg);
        }       

        //取得銀行類型 & 審核通過的清單
        static List<ConvertLog> GetConvertBankLog()
        {
            List<ConvertLog> lstConvertLog = new List<ConvertLog>();
            using (SqlConnection conn = new SqlConnection(dbConnection))
            {
                conn.Open();
                String sql = "dbo.PrWeb_Money_WithdrawGet";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    try
                    {
                        SqlDataAdapter dataAdapter = new SqlDataAdapter();
                        dataAdapter.SelectCommand = cmd;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@intGWID", ConvertGWID);
                        cmd.Parameters.AddWithValue("@intConvertType", ConvertType);

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
    }
}