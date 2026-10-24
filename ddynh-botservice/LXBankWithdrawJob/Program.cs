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


namespace LXBankWithdrawJob
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
                Thread.Sleep(5000);
            }
        }

        public new static void Test(ConvertLog item)
        {
            item.WithdrawInfoData = "{\"MERCHANT_NO\":\"bot3398\",\"MERCHANT_KEY\":\"6efUePSOcwXw1gLkvFZt70BW1gKtjJVN0lf\"}";
            CallbackUrl = "http://0.0.0.0/Convert/MayiWithdrawCallback";
            Process(item);
        }

        static void Process(ConvertLog item)
        {
            WriteLine("-------------------------------------------------------------");
            string money = item.Amount.ToString();
            string timestamp = Convert.ToInt64(DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds * 1000).ToString();

            //訂單編號處理
            var trade_id = "000000000000" + item.Id.ToString();   //店家自行设定，不得小于 6 个字符，不得重复
            trade_id = "DD" + trade_id.Substring((trade_id.Length - 10));

            //WithdrawInfoData
            JObject withdrawJson;
            try
            {
                withdrawJson = JObject.Parse(item.WithdrawInfoData);
            }
            catch { withdrawJson = null; }

            var merchantID = withdrawJson["MERCHANT_NO"] != null ? withdrawJson["MERCHANT_NO"].ToString() : string.Empty;  //商户号
            var md5Key = withdrawJson["MERCHANT_KEY"] != null ? withdrawJson["MERCHANT_KEY"].ToString() : string.Empty;  //商户号密钥

            Dictionary<string, string> paras = new Dictionary<string, string>();
            paras.Add("accname", item.RealName);           //收款人姓名
            paras.Add("accno", item.Account);              //收款人帐号
            paras.Add("acctype", "0");             //帐户类型; 0: 银行卡
            paras.Add("amount", money);             //金额(以分为单位)100000(金额 1000 元)
            paras.Add("bankcode", item.BankID);              //银行代码
            paras.Add("currency", "CNY");     //币别   
            paras.Add("mhtorderno", trade_id);     //商户代付订单号
            paras.Add("notifyurl", CallbackUrl);   //回调网址
            paras.Add("opmhtid", merchantID);         //持卡人姓名
            paras.Add("random", timestamp);            //随机字段(每次呼叫带入不同随机字段)
            paras.Add("signkey", md5Key);            //随机字段(每次呼叫带入不同随机字段)

            var str = string.Join("&", paras.Select(m => string.Format("{0}={1}", m.Key, m.Value)));
            WriteLine($"str:{str}");
            string md5sign = Md5(str, Encoding.UTF8);
            paras.Add("sign", md5sign);
            WriteLine($"pay_md5sign:{md5sign}");              //签章值

            var content = string.Join("&", paras.Select(m => string.Format("{0}={1}", m.Key, m.Value)));
            WriteLine($"Id:{item.Id} url:{WithdrawUrl}\nparam:{content}");

            string responseData = string.Empty;
            var headers = new Dictionary<string, string>()
            {
                { "Content-Type", "application/x-www-form-urlencoded" }
            };
            int httpCode = HttpPost(WithdrawUrl, content, headers, ref responseData);
            WriteLine($"Id:{item.Id} http:{httpCode}\nresponse:{responseData}", httpCode == 200 ? ConsoleColor.Yellow : ConsoleColor.Red);

            int intUpdStatus = 99; //預設自动化進行中
            int intTeamOrderId = Convert.ToInt32(item.Id); //兑换订单号           
            string returnCode = "null";
            string returnMsg = "null";
            string returnTranId = "0";
            string returnAfterBalance = "0";

            /**
             * {
             *  "errorCode": 0,                         //错误代码; 0: 成功
             *  "errorMsg": "",                         //错误讯息; 成功则为空
             *  "result":{                              //失败为 null
             *      "pforderno":"HYRTJE181222IJRGE",    //平台订单号
             *      "afterbalance":5000000              //商户剩余可代付金额(以分为单位); 50000000 (金额 500000 元)
             *  }
             */
            JObject json;
            try
            {
                json = JObject.Parse(responseData);
            }
            catch { json = null; }

            //取得訪問結果與訊息
            if (json != null)
            {
                if (json["errorCode"] != null) returnCode = json["errorCode"].ToString();
                if (json["errorMsg"] != null) returnMsg = json["errorMsg"].ToString();
                if (returnCode != "0")
                {
                    intUpdStatus = 100;     //自动打款失败
                }
                else
                {
                    returnTranId = json["result"]["pforderno"].ToString();
                    returnAfterBalance = json["result"]["afterbalance"].ToString();
                }
            }


            WriteLine($"Id:{item.Id} returnCode:{returnCode} returnMsg:{returnMsg}", intUpdStatus == 99 ? ConsoleColor.Green : ConsoleColor.Red);

            //更新結果
            string message = string.Empty;
            bool returnValue = UpdateConvertLogStatus(intUpdStatus, intTeamOrderId, returnTranId, returnMsg, ref message);

            WriteLine($"Id:{item.Id} intUpdStatus:{intUpdStatus} msg:{message}", returnValue ? ConsoleColor.Green : ConsoleColor.Red);
            WriteLine(" ");
        }
    }
}
