using BankWithdrawJob;
using BankWithdrawJob.Entity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LunyunBankWithdrawJob
{
    class Program : BaseBankWithdrawJob
    {
        static void Main(string[] args)
        {
            BankCode = GetConvertBankCode();

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
            var bank = BankCode.FirstOrDefault(x => x.Code == item.BankCode);

            string orderNo = item.Id;
            string tradeAmt = (item.Amount / 100).ToString("F2");
         
            string accountName = item.RealName;
            string accountNo = item.Account;
            string bankAddress = string.Empty;
            string bankCity = string.Empty;
            string bankID = item.BankID;
            string bankNo = bank == null ? "0" : bank.TradeCode;
            string bankProvince = string.Empty;
            string accountMobile = item.LoginName;          

            var model = new RequestModel();
            model.module = "order";
            model.method = "api_add_order";
            model.company_name = MerId;
            model.payload.card_number = accountNo;
            model.payload.amount = tradeAmt;
            model.payload.trans_mode = "out_trans";
            model.payload.order_number = orderNo;
            model.payload.real_name = accountName;

            string param = JsonConvert.SerializeObject(model);     
           
            var sign = CreateSign(param, MD5_Key);
            var headers = new Dictionary<string, string>();
            headers.Add("TLYHMAC", sign);

            WriteLine($"Id:{item.Id} url:{WithdrawUrl}\nparam:{param}");

            string responseData = string.Empty;
            int httpCode = HttpPostJson(WithdrawUrl, param, headers, ref responseData);

            WriteLine($"Id:{item.Id} http:{httpCode} response:{responseData}", httpCode == 200 ? ConsoleColor.Yellow : ConsoleColor.Red);


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
            if (json != null && json["success"] != null) returnCode = json["success"].ToString();
            if (json != null && json["message"] != null) returnMsg = json["message"].ToString();            


            //取得打款結果與訊息
            if (json != null && returnCode.ToLower() != "true")
                intUpdStatus = 100; //自动打款失败    
            else
                if (json != null && json["data"]["order_id"] != null) returnTranId = json["data"]["order_id"].ToString();

            WriteLine($"Id:{item.Id} returnCode:{returnCode} returnMsg:{returnMsg}", intUpdStatus == 99 ? ConsoleColor.Green : ConsoleColor.Red);

            //更新結果
            string message = string.Empty;
            bool returnValue = UpdateConvertLogStatus(intUpdStatus, intTeamOrderId, returnTranId, returnMsg, ref message);

            WriteLine($"Id:{item.Id} msg:{message}", returnValue ? ConsoleColor.Green : ConsoleColor.Red);
        }

        static string CreateSign(string message, string key)
        {
            var encoding = new System.Text.UTF8Encoding();
            byte[] keyByte = encoding.GetBytes(key);
            byte[] messageBytes = encoding.GetBytes(message);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                return Convert.ToBase64String(hashmessage);
            }
        }

    }
}
