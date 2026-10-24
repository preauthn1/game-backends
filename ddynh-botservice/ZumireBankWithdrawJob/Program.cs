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

namespace ZumireBankWithdrawJob
{
    class Program : BaseBankWithdrawJob
    {
        public static string MchUser
        {
            get { return ConfigurationManager.AppSettings["mch_user"] ?? string.Empty; }
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
            string money = (item.Amount / 100).ToString();
            string bnkID = item.BankID == "0" ? "NOFOUND" : item.BankID;

            string timestamp = Convert.ToInt64(DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds * 1000).ToString();

            Dictionary<string, string> paras = new Dictionary<string, string>();
            paras.Add("mch_id", MerId);                 //商户号
            //paras.Add("mch_user", MchUser);           //⽤户名
            paras.Add("mch_order", item.Id);            //商户订单号
            paras.Add("client_ip", item.CreateIP);      //客户端IP
            paras.Add("amt", money);                    //代付金额 单位元
            paras.Add("notify_url", CallbackUrl);       //回调地址
            paras.Add("bank_id", item.BankID);          //银⾏名code
            paras.Add("bank_card", item.Account);       //银行账号         
            paras.Add("real_name", item.RealName);      //收款人姓名 
            paras.Add("sign_type", "md5");              //签名类型
            paras.Add("created_at", timestamp);         //下单时间
            paras.Add("mch_key", MD5_Key);

            var sort = paras.OrderBy(k => k.Key);
            paras = sort.ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);

            string str = string.Join("&", paras.Where(x => !string.IsNullOrEmpty(x.Value)).Select(m => string.Format("{0}={1}", m.Key, m.Value)));
            string sign = Md5(str).ToLower();
            paras.Add("sign", sign);
            paras.Remove("mch_key");

            var param = string.Join("&", paras.Select(m => string.Format("{0}={1}", m.Key, m.Value)));
            WriteLine($"Id:{item.Id} param:{WithdrawUrl}?{param}");

            string responseData = string.Empty;
            int httpCode = HttpPost(WithdrawUrl, param, ref responseData);

            WriteLine($"Id:{item.Id} http:{httpCode} response:{responseData}", httpCode == 200 ? ConsoleColor.Yellow : ConsoleColor.Red);


            int intUpdStatus = 99; //預設自动化進行中
            int intTeamOrderId = Convert.ToInt32(item.Id); //兑换订单号           
            string returnCode = "null";
            string returnMsg = "null";
            string returntSatus = "null";
            string returnTranId = "0";

            JObject json;
            try
            {
                json = JObject.Parse(responseData);
            }
            catch { json = null; }


            //取得訪問結果與訊息
            if (json != null && json["code"] != null) returnCode = json["code"].ToString();
            if (json != null && json["msg"] != null) returnMsg = json["msg"].ToString(); 
            if (json != null && returnCode == "1" && json["data"]["status"] != null) returntSatus = json["data"]["status"].ToString();


            //取得打款結果與訊息
            if (json != null && returntSatus == "1")
                intUpdStatus = 99;      //自动化進行中 
            else if (returntSatus == "10" || returntSatus == "11")
                intUpdStatus = 3;       //打款成功
            else
                intUpdStatus = 100;     //自动打款失败

            WriteLine($"Id:{item.Id} returntSatus:{returntSatus} returnMsg:{returnMsg}", intUpdStatus == 99 ? ConsoleColor.Green : ConsoleColor.Red);

            //更新結果
            string message = string.Empty;
            bool returnValue = UpdateConvertLogStatus(intUpdStatus, intTeamOrderId, returnTranId, returnMsg, ref message);

            WriteLine($"Id:{item.Id} msg:{message}", returnValue ? ConsoleColor.Green : ConsoleColor.Red);
        }
    }
}
