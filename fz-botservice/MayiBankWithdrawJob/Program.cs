using BankWithdrawJob;
using BankWithdrawJob.Entity;
using Newtonsoft.Json;
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
using System.Web;

namespace MayiBankWithdrawJob
{
    class Program : BaseBankWithdrawJob
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.GetEncoding(936);
            Thread t = new Thread(Job);
            t.Start();
        }

        static void Job()
        {
            while (true)
            {
                try
                {
                    List<ConvertLog> data = GetConvertBankLog();
                    foreach (var item in data)
                    {
                        if (IsDebug)
                            Test(item);
                        else
                            Process(item);
                    }
                }
                catch (Exception e)
                {
                    WriteLine($"job exception:{e}", ConsoleColor.Red);
                }
                Thread.Sleep(500);
            }
        }

        public new static void Test(ConvertLog item)
        {
            MD5_Key = "283a86f5e5f88fca42ba06b514424aef";
            MerId = "2fb2f4bfdd0dfb44";
            CallbackUrl = "http://0.0.0.0/Convert/MayiWithdrawCallback";
            Process(item);
        }

        static void Process(ConvertLog item)
        {
            string money = (item.Amount / 100).ToString(); 
            string timestamp = Convert.ToInt64(DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds * 1000).ToString();
            string notifyUrl = HttpUtility.UrlEncode(CallbackUrl);
            string realName = HttpUtility.UrlEncode(item.RealName);

            Dictionary<string, string> paras = new Dictionary<string, string>();
            paras.Add("business", "Transfer");      //业务固定值
            paras.Add("business_type", "10101");    //业务编码
            paras.Add("api_sn", item.Id);           //商户订单号
            paras.Add("notify_url", notifyUrl);     //回调地址
            paras.Add("money", money);              //代付金额 单位元
            paras.Add("bene_no", item.Account);     //银行账号         
            paras.Add("bank_id", "");               //银⾏名code
            paras.Add("payee", realName);           //收款人姓名 
            paras.Add("timestamp", timestamp);      //下单时间

            var sort = paras.OrderBy(k => k.Key);
            paras = sort.ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);

            string str = string.Join("&", paras.Where(x => !string.IsNullOrEmpty(x.Value)).Select(m => string.Format("{0}={1}", m.Key, m.Value))) + $"&key={MD5_Key}";
            string sign = Md5(str).ToUpper();
            paras.Add("sign", sign);

            var content = JsonConvert.SerializeObject(paras);
            string key = MD5_Key.Substring(0, 24);
            string iv = MD5_Key.Substring(0, 8);

            string data1 = Convert.ToBase64String(Des3EncodeCBC(Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(iv), Encoding.UTF8.GetBytes(content)));

            var param = $"mcode={MerId}&params={Convert.ToBase64String(Encoding.UTF8.GetBytes(data1))}";
            WriteLine($"Id:{item.Id} url:{WithdrawUrl}\nparam:{content}");

            string responseData = string.Empty;
            int httpCode = HttpPost(WithdrawUrl, param, ref responseData);

            WriteLine($"Id:{item.Id} http:{httpCode}\nresponse:{responseData}", httpCode == 200 ? ConsoleColor.Yellow : ConsoleColor.Red);


            int intUpdStatus = 99; //預設自动化進行中
            int intTeamOrderId = Convert.ToInt32(item.Id); //兑换订单号           
            string returnCode = "null";
            string returnMsg = "null";          
            string returnTranId = "0";

            JObject json;
            try
            {
                json = JObject.Parse(responseData);
            }
            catch { json = null; }
          
           
            //取得訪問結果與訊息
            if (json != null && json["status"] != null) returnCode = json["status"].ToString();
            if (json != null && json["msg"] != null) returnMsg = json["msg"].ToString();
            if (json != null && returnCode.ToLower() == "true" && json["data"]["order_sn"] != null) returnTranId = json["data"]["order_sn"].ToString();


            //取得打款結果與訊息
            if (json != null && returnCode.ToLower() != "true")
                intUpdStatus = 100;     //自动打款失败

            WriteLine($"Id:{item.Id} returnCode:{returnCode} returnMsg:{returnMsg}", intUpdStatus == 99 ? ConsoleColor.Green : ConsoleColor.Red);

            //更新結果
            string message = string.Empty;
            bool returnValue = UpdateConvertLogStatus(intUpdStatus, intTeamOrderId, returnTranId, returnMsg, ref message);

            WriteLine($"Id:{item.Id} intUpdStatus:{intUpdStatus} msg:{message}", returnValue ? ConsoleColor.Green : ConsoleColor.Red);
        }

        #region CBC模式
        /// <summary>  
        /// DES3 CBC模式加密  
        /// </summary>  
        /// <param name="key">密钥</param>  
        /// <param name="iv">IV</param>  
        /// <param name="data">明文的byte数组</param>  
        /// <returns>密文的byte数组</returns>  
        public static byte[] Des3EncodeCBC(byte[] key, byte[] iv, byte[] data)
        {
            //复制于MSDN  
            try
            {
                // Create a MemoryStream.  
                MemoryStream mStream = new MemoryStream();
                TripleDESCryptoServiceProvider tdsp = new TripleDESCryptoServiceProvider();
                tdsp.Mode = CipherMode.CBC;             //默认值  
                tdsp.Padding = PaddingMode.PKCS7;       //默认值  
                                                 
                CryptoStream cStream = new CryptoStream(mStream,
                    tdsp.CreateEncryptor(key, iv),
                    CryptoStreamMode.Write);

                cStream.Write(data, 0, data.Length);
                cStream.FlushFinalBlock();
                              
                byte[] ret = mStream.ToArray();
                cStream.Close();
                mStream.Close();
                return ret;
            }
            catch (CryptographicException e)
            {
                Console.WriteLine("A Cryptographic error occurred: {0}", e.Message);
                return null;
            }
        }

        /// <summary>  
        /// DES3 CBC模式解密  
        /// </summary>  
        /// <param name="key">密钥</param>  
        /// <param name="iv">IV</param>  
        /// <param name="data">密文的byte数组</param>  
        /// <returns>明文的byte数组</returns>  
        public static byte[] Des3DecodeCBC(byte[] key, byte[] iv, byte[] data)
        {
            try
            {               
                MemoryStream msDecrypt = new MemoryStream(data);
                TripleDESCryptoServiceProvider tdsp = new TripleDESCryptoServiceProvider();
                tdsp.Mode = CipherMode.CBC;
                tdsp.Padding = PaddingMode.PKCS7;             

                CryptoStream csDecrypt = new CryptoStream(msDecrypt,
                    tdsp.CreateDecryptor(key, iv),
                    CryptoStreamMode.Read);

                byte[] fromEncrypt = new byte[data.Length];
                csDecrypt.Read(fromEncrypt, 0, fromEncrypt.Length);

                return fromEncrypt;
            }
            catch (CryptographicException e)
            {
                Console.WriteLine("A Cryptographic error occurred: {0}", e.Message);
                return null;
            }
        }
        #endregion
    }
}
