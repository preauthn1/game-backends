using BankWithdrawJob;
using BankWithdrawJob.Entity;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using example.invalid;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DadongBankWithdrawJob
{
    class Program : BaseBankWithdrawJob
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.GetEncoding(936);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

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
            string orderNo = item.Id;
            string tradeAmt = (item.Amount / 100).ToString("F2");           
            string accountName = item.RealName;
            string accountNo = item.Account;         
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");

            Dictionary<string, string> paras = new Dictionary<string, string>();
            paras.Add("merchantNo", MerId);                 //商户号       
            paras.Add("merchantOrderNo", orderNo);          //商户订单号
            paras.Add("merchantReqTime", timestamp);        //请求时间(yyyyMMddHHmmss)
            paras.Add("orderAmount", tradeAmt);             //代付金额 单位元   
            paras.Add("tradeSummary", "1");                 //交易摘要         
            paras.Add("bankAccountNo", accountNo);          //收款人卡号            
            paras.Add("bankAccountName", accountName);      //收款人姓名                      
            paras.Add("province", "1");                     //开户行所属省          
            paras.Add("city", "1");                         //开户行所属市          
            paras.Add("bankName", "1");                     //开户行名称          
            paras.Add("bankCode", "ALIPAY");                //银行代码
            paras.Add("orderReason", "1");                  //代付原因/用途          
            paras.Add("requestIp", item.CreateIP);          //请求 IP          
            paras.Add("backNoticeUrl", CallbackUrl);        //异步通知地址         
            paras.Add("merchantParam", "1");                //商户回传参数

            var sort = paras.OrderBy(k => k.Key);
            paras = sort.ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);

            string str = string.Join("&", paras.Select(m => string.Format("{0}={1}", m.Key, m.Value))) + $"{MD5_Key}";
            string sign = Md5(str).ToLower();
            paras.Add("sign", sign);


            var param = string.Join("&", paras.Select(m => string.Format("{0}={1}", m.Key, m.Value)));
            WriteLine($"Id:{item.Id} url:{WithdrawUrl}\nparam:{param}");

            string responseData = string.Empty;
            int httpCode = HttpPost(WithdrawUrl, param, ref responseData);

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
            if (json != null && json["code"] != null) returnCode = json["code"].ToString();
            if (json != null && json["msg"] != null) returnMsg = json["msg"].ToString();
            if (json != null && json["biz"] != null) returnTranId = json["biz"]["platformOrderNo"].ToString();


            //取得打款結果與訊息
            if (json != null && returnCode != "SUCCESS")
                intUpdStatus = 100; //自动打款失败    

            WriteLine($"Id:{item.Id} returnCode:{returnCode} returnMsg:{returnMsg}", intUpdStatus == 99 ? ConsoleColor.Green : ConsoleColor.Red);

            //更新結果
            string message = string.Empty;
            bool returnValue = UpdateConvertLogStatus(intUpdStatus, intTeamOrderId, returnTranId, returnMsg, ref message);

            WriteLine($"Id:{item.Id} msg:{message}", returnValue ? ConsoleColor.Green : ConsoleColor.Red);
        }
    }
}
