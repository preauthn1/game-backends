using BankWithdrawJob;
using BankWithdrawJob.Entity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LunyunQueryJob
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
                Thread.Sleep(1000);
            }
        }

        static void Process(ConvertLog item)
        {
            Dictionary<string, string> paras = new Dictionary<string, string>();
            paras.Add("apikey", MD5_Key);
            paras.Add("id", item.Id.ToString());
            paras.Add("active", "true");
            paras.Add("verified", "true");

            var param = JsonConvert.SerializeObject(paras);
            WriteLine($"Id:{item.Id} url:{WithdrawUrl}\nparam:{param}");

            string responseData = string.Empty;
            int httpCode = HttpPostJson(WithdrawUrl, param, ref responseData);

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
            //if (json != null && json["id"] != null) returnTranId = json["id"].ToString();


            //取得打款結果與訊息
            if (returnCode.ToLower() == "true")   
            {
                string returnData = string.Empty;
                if (json != null && json["data"] != null) returnData = json["data"].ToString();

                foreach (var p in JObject.Parse(returnData).Properties())
                {


                    //intUpdStatus = 3;

                    //WriteLine($"Id:{item.Id} returnCode:{returnCode} returnMsg:{returnMsg}", intUpdStatus == 99 ? ConsoleColor.Green : ConsoleColor.Red);

                    ////更新結果
                    //string message = string.Empty;
                    //bool returnValue = UpdateConvertLogStatus(intUpdStatus, intTeamOrderId, returnTranId, returnMsg, ref message);

                    
                    //WriteLine($"Id:{item.Id} msg:{message}", returnValue ? ConsoleColor.Green : ConsoleColor.Red);
                }
            }
            
        }
    }
}
