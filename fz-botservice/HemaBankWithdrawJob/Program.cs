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
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HemaBankWithdrawJob
{
    class Program : BaseBankWithdrawJob
    {        
        static string AgtId
        {
            get { return ConfigurationManager.AppSettings["agtId"]; }
        }            

        static string PriKey
        {
            get { return ConfigurationManager.AppSettings["priKey"]; }
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
                    WriteLine($"job exception:{e}", ConsoleColor.Red);
                }
                Thread.Sleep(500);
            }
        }

        static void Process(ConvertLog item)
        {
            string agtId = Program.AgtId;
            string merId = Program.MerId;
            string MD5KEY = Program.MD5_Key;            
            string priKey = Program.PriKey;
            string notifyUrl = CallbackUrl;
            string url = WithdrawUrl;

            string order_num = item.Id;
            string money = item.Amount.ToString();
            string accountNo = item.Account;
            string bankName = item.BankName;
            string accountName = item.RealName;
            string timestamp = Convert.ToInt64(DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds * 1000).ToString();

            Dictionary<string, string> paras = new Dictionary<string, string>();

            paras.Add("tranCode", "2101");                              //交易码
            paras.Add("agtId", agtId);                                  //机构号
            paras.Add("merId", merId);                                  //商户号
            paras.Add("orderId", item.Id);                              //商户订单号
            paras.Add("tranDate", DateTime.Now.ToString("yyyyMMdd"));   //交易日期
            paras.Add("nonceStr", Guid.NewGuid().ToString("N"));        //随机字符串
            paras.Add("txnAmt", money);                                 //代付金额 单位分
            paras.Add("accountNo", accountNo);                          //账户 ,到账卡号            
            paras.Add("bankName", BToHex(Encoding.UTF8.GetBytes(bankName)).ToLower());        //银行名称 
            paras.Add("accountName", BToHex(Encoding.UTF8.GetBytes(accountName)).ToLower());  //账户名          
            paras.Add("cnaps", "308290003298");                         //联行号            
            paras.Add("accountType", "1");                              //0 对公 1 对私  
            paras.Add("notifyUrl", notifyUrl);                          //回调地址

            var sort = paras.OrderBy(k => k.Key);
            paras = sort.ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);

            string str = string.Join("&", paras.Select(m => string.Format("{0}={1}", m.Key, m.Value))) + $"&key={MD5KEY}";
            string sign = Md5(str, Encoding.UTF8).ToUpper();
            string rsa_key = RSAPrivateKeyJava2DotNet(PriKey);
            string rsa_sign = RSAEncryptSignData(rsa_key, sign);         

            Dictionary<string, string> paras_sign = new Dictionary<string, string>();
            paras_sign.Add("sign", rsa_sign);

            var req = new REQ()
            {
                REQ_HEAD = paras_sign,
                REQ_BODY = paras
            };

            var data = JsonConvert.SerializeObject(req);

            var param = string.Join("&", paras.Select(m => string.Format("{0}={1}", m.Key, m.Value)));
            WriteLine($"Id:{item.Id} param:{param}");


            string responseData = "";
            int httpCode = HttpPostJson(url, data, ref responseData);           
            WriteLine($"Id:{item.Id}  return:{httpCode} response:{responseData}", httpCode == 200 ? ConsoleColor.Yellow : ConsoleColor.Red);


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
            if (json != null && json["REP_BODY"]["rspcode"] != null) returnCode = json["REP_BODY"]["rspcode"].ToString();
            if (json != null && json["REP_BODY"]["rspmsg"] != null) returnMsg = json["REP_BODY"]["rspmsg"].ToString();
            if (json != null && json["REP_BODY"]["tranId"] != null) returnTranId = json["REP_BODY"]["tranId"].ToString();

            if (!string.IsNullOrEmpty(returnMsg))
                returnMsg = HexToString(returnMsg);

            //取得打款結果與訊息
            if (json != null && returnCode.ToLower() != "000000")
            {
                intUpdStatus = 100; //自动打款失败               
            }
           
            WriteLine($"Id:{item.Id} returnCode:{returnCode} returnMsg:{returnMsg}", intUpdStatus == 99 ? ConsoleColor.Green : ConsoleColor.Red);       

            string message = string.Empty;
            bool returnValue = UpdateConvertLogStatus(intUpdStatus, intTeamOrderId, returnTranId, returnMsg, ref message);

            WriteLine($"Id:{item.Id} msg:{message}", returnValue ? ConsoleColor.Green : ConsoleColor.Red);
        }       

        static string BToHex(byte[] Bdata)
        {
            return BitConverter.ToString(Bdata).Replace("-", "");
        }

        static string HexToString(string hexString)
        {
            var bytes = new byte[hexString.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                string currentHex = hexString.Substring(i * 2, 2);
                bytes[i] = Convert.ToByte(currentHex, 16);
            }

            return Encoding.GetEncoding("UTF-8").GetString(bytes);
        }
    }  
}
