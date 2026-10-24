using BankWithdrawJob;
using BankWithdrawJob.Entity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using example.invalid;
using System.Linq;
using example.invalid;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HammerBankWithdrawJob
{
    class Program : BaseBankWithdrawJob
    {
        #region 参数         

        static string Tiechuipay_merchantCode
        {
            get { return ConfigurationManager.AppSettings["pay_tiechuipay_merchantCode"].ToString(); }
        }

        static string Tiechuipay_agentCode
        {
            get { return ConfigurationManager.AppSettings["pay_tiechuipay_agentCode"].ToString(); }
        }

        static string Tiechuipay_channelCode
        {
            get { return ConfigurationManager.AppSettings["pay_tiechuipay_channelCode"].ToString(); }
        }

        static string Tiechuipay_Key
        {
            get { return ConfigurationManager.AppSettings["pay_tiechuipay_key"].ToString(); }
        }

        static string Tiechuipay_RSA_PrivateKey
        {
            get { return ConfigurationManager.AppSettings["pay_tiechuipay_RSA_PrivateKey"].ToString(); }
        }

        static string Tiechuipay_RSA_PublicKey
        {
            get { return ConfigurationManager.AppSettings["pay_tiechuipay_RSA_PublicKey"].ToString(); }
        }

        #endregion

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

        static void Process(ConvertLog item)
        {
            var secKey = Guid.NewGuid().ToString("N").Substring(0, 16);

            var sData = new PayapiRequestModel();
            sData.merchantCode = Tiechuipay_merchantCode;
            sData.agentCode = Tiechuipay_agentCode;
            sData.channelCode = Tiechuipay_channelCode;

            sData.transactionLog = new TransactionLog();
            sData.transactionLog.tradeAmount = (item.Amount / 100).ToString();
            sData.transactionLog.downOrderNum = item.Id;
            sData.transactionLog.subjectCard = item.Account;
            sData.transactionLog.subjectName = item.RealName;
            sData.transactionLog.subjectPhone = string.Empty;
            sData.transactionLog.certType = "1";
            sData.transactionLog.certNo = string.Empty;
            sData.transactionLog.toPrivate = "0";
            sData.transactionLog.bankCode = string.Empty;
            sData.transactionLog.bankGeneralName = item.BankName;
                        

            var dataJson = JsonConvert.SerializeObject(sData);
            var encryptData = AESEncrypt(dataJson, secKey);
            var public_key = RSAPublicKeyJava2DotNet(Tiechuipay_Key);

            var encrtptKey = RSAEncrypt(public_key, secKey);
            var signData = RSAEncrypt(public_key, dataJson);

            WriteLine($"Id:{item.Id} param:{dataJson}");

            Dictionary<string, string> paras = new Dictionary<string, string>
            {
                { "agentCode", Tiechuipay_agentCode },      //代理商编号
                { "merchantCode", Tiechuipay_merchantCode },//商户号
                { "encryptData", encryptData },             //加密后的请求报文
                { "encrtptKey", encrtptKey },               //加密后的AES
                { "signData", signData },                   //签名
                { "version", "3.0.0" }                      //版本
            };
           
            var data = JsonConvert.SerializeObject(paras);

            string responseData = "";
            int httpCode = HttpPostJson(WithdrawUrl, data, ref responseData);
            FileLog($"Id:{item.Id} http:{httpCode} response:{responseData}", fileLogName);


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

            var result_signData = json["signData"] != null ? json["signData"].ToString() : string.Empty;
            var result_encrtptKey = json["encrtptKey"] != null ? json["encrtptKey"].ToString() : string.Empty;
            var result_encryptData = json["encryptData"] != null ? json["encryptData"].ToString() : string.Empty;

            var decrypt_aesKey = RSADecrypt(Tiechuipay_RSA_PrivateKey, result_encrtptKey);
            var decrypt_json = AESDecrypt(result_encryptData, decrypt_aesKey);
            var decrypt_sign = RSAEncrypt(RSAPublicKeyJava2DotNet(Tiechuipay_RSA_PublicKey), decrypt_json);
       
            WriteLine($"Id:{item.Id} return:{httpCode} response:{decrypt_json}", httpCode == 200 ? ConsoleColor.Yellow : ConsoleColor.Red);           


            json = JObject.Parse(decrypt_json);
            //取得訪問結果與訊息
            if (json != null && json["code"] != null) returnCode = json["code"].ToString();
            if (json != null && json["msg"] != null) returnMsg = json["msg"].ToString();        

            //取得打款結果與訊息
            if (json != null && returnCode == "20000")
            {                
                //更新結果               
                string message = string.Empty;
                bool returnValue = UpdateConvertLogStatus(intUpdStatus, intTeamOrderId, returnTranId, returnMsg, ref message);

                WriteLine($"Id:{item.Id} msg:{message}", returnValue ? ConsoleColor.Green : ConsoleColor.Red);
            }
        }
    } 
}
