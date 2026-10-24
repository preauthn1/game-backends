using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using example.invalid;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using example.invalid;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using BankWithdrawJob;
using BankWithdrawJob.Entity;

namespace HammerQueryJob
{
    class Program : BaseBankWithdrawJob
    {      
        static string agentCode
        {
            get { return ConfigurationManager.AppSettings["agentCode"]; }
        }      

        public static string Tiechuipay_Key
        {
            get { return ConfigurationManager.AppSettings["pay_tiechuipay_key"].ToString(); }
        }

        public static string Tiechuipay_RSA_PrivateKey
        {
            get { return ConfigurationManager.AppSettings["pay_tiechuipay_RSA_PrivateKey"].ToString(); }
        }

        public static string Tiechuipay_RSA_PublicKey
        {
            get { return ConfigurationManager.AppSettings["pay_tiechuipay_RSA_PublicKey"].ToString(); }
        }


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
                    string err = $"job exception:{e}";
                    Console.WriteLine(err);
                    FileLog(err, fileLogName);
                }
                Thread.Sleep(1000);
            }
        }

        new static void Test(ConvertLog item)
        {
            int intUpdStatus = 3; //預設自动化進行中
            int intTeamOrderId = Convert.ToInt32(item.Id); //兑换订单号            
            string returnMsg = "null";
            string returnTranId = "0";


            string message = string.Empty;
            if (UpdateConvertLogStatus(intUpdStatus, intTeamOrderId, returnTranId, returnMsg, ref message))
                WriteLine($"Id:{intTeamOrderId} msg:{message}", ConsoleColor.Green);
        }

        static void Process(ConvertLog item)
        {
            string agentCode = Program.agentCode;
            string merchantCode = Program.MerId;
            string url = WithdrawUrl;

            string order_num = item.Id;
            string money = item.Amount.ToString();
            string accountNo = item.Account;
            string bankName = item.BankName;
            string accountName = item.RealName;
            string timestamp = Convert.ToInt64(DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds * 1000).ToString();

            var req = new REQ()
            {
                downOrderNum = order_num,
               
            };
            var secKey = Guid.NewGuid().ToString("N").Substring(0, 16);
           
            REQMain d = new REQMain();
            d.agentCode = agentCode;
            d.merchantCode = merchantCode;
            d.transactionLog = req;           


            var dataJson = JsonConvert.SerializeObject(d);
            var encryptData = AESEncrypt(dataJson, secKey);
            var public_key = RSAPublicKeyJava2DotNet(Tiechuipay_Key);

            var encrtptKey = RSAEncrypt(public_key, secKey);
            var signData = RSAEncrypt(public_key, dataJson);

            WriteLine($"Id:{item.Id} Data:{dataJson}");

            Dictionary<string, string> paras = new Dictionary<string, string>
            {
                { "merchantCode", merchantCode },//商户号
                { "agentCode", agentCode },      //代理商编号
                { "encryptData", encryptData },             //加密后的请求报文
                { "encrtptKey", encrtptKey },               //加密后的AES
                { "signData", signData },                   //签名
                { "version", "3.0.0" }                      //版本
            };

            var data = JsonConvert.SerializeObject(paras);

            string responseData = "";
            int httpCode = HttpPostJson(url, data, ref responseData);
           
            

            JObject Retjson = JObject.Parse(responseData);
            var result_signData = Retjson["signData"] != null ? Retjson["signData"].ToString() : string.Empty;
            var result_encrtptKey = Retjson["encrtptKey"] != null ? Retjson["encrtptKey"].ToString() : string.Empty;
            var result_encryptData = Retjson["encryptData"] != null ? Retjson["encryptData"].ToString() : string.Empty;


            var decrypt_aesKey = RSADecrypt(Tiechuipay_RSA_PrivateKey, result_encrtptKey);
            var decrypt_json = AESDecrypt(result_encryptData, decrypt_aesKey);
            var decrypt_sign = RSAEncrypt(RSAPublicKeyJava2DotNet(Tiechuipay_RSA_PublicKey), decrypt_json);
          
            WriteLine($"Id:{item.Id} return:{httpCode} response:{decrypt_json}", httpCode == 200 ? ConsoleColor.Yellow : ConsoleColor.Red);

            int intUpdStatus = 99; //預設自动化進行中
            int intTeamOrderId = Convert.ToInt32(item.Id); //兑换订单号           
            string returnCode = "null";
            string returnMsg = "null";          

            JObject json;
            try
            {
                json = JObject.Parse(decrypt_json);
            }
            catch { json = null; }


            //取得訪問結果與訊息
            if (json != null && json["data"]["responseCode"] != null) returnCode = json["data"]["responseCode"].ToString();
            if (json != null && json["msg"] != null) returnMsg = json["msg"].ToString();


            string message = string.Empty;

            //取得打款結果與訊息
            if (json != null && returnCode.ToLower() == "60000")
            {
                intUpdStatus = 100; //自动打款失败              
                WriteLine($"Id:{item.Id} returnCode:{returnCode} returnMsg:{returnMsg}", ConsoleColor.Red);

                //更新結果   
                bool returnValue = UpdateConvertLogStatus(intUpdStatus, intTeamOrderId, "0", returnMsg, ref message);
                WriteLine($"Id:{item.Id} msg:{message}", returnValue ? ConsoleColor.Green : ConsoleColor.Red);
            }
            else if (json != null && returnCode.ToLower() == "20000")
            {
                intUpdStatus = 3; //自动打款成功
                WriteLine($"Id:{item.Id} returnCode:{returnCode} returnMsg:{returnMsg}", ConsoleColor.Green);

                //更新結果   
                bool returnValue = UpdateConvertLogStatus(intUpdStatus, intTeamOrderId, "0", returnMsg, ref message);
                WriteLine($"Id:{item.Id} msg:{message}", returnValue ? ConsoleColor.Green : ConsoleColor.Red);
            }
        }
    }
   
    [Serializable]
    class REQ
    {
        public string downOrderNum { get; set; }      
    }

    [Serializable]
    class REQMain
    {
        public string merchantCode { get; set; }
        public string agentCode { get; set; }  
             
        public REQ transactionLog { get; set; }

    }
}
