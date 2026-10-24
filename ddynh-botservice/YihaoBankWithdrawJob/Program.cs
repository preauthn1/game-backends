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

namespace YihaoBankWithdrawJob
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

        static void Process(ConvertLog item)
        {
            string merId = MerId;
            string notifyUrl = CallbackUrl;
            string url = WithdrawUrl;

            string order_num = item.Id;
            string money = item.Amount.ToString();
            string accountNo = item.Account;
            string bankName = item.BankName;
            string accountName = item.RealName;
            string timestamp = Convert.ToInt64(DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds * 1000).ToString();

            Dictionary<string, string> paras = new Dictionary<string, string>();
            paras.Add("mer_id", merId);                             //商户号
            //paras.Add("biz_id", "0");                             //通道id
            paras.Add("out_trade_no", item.Id);                     //商户订单号
            paras.Add("pay_type", "016");                           //支付类型
            paras.Add("total_fee", money);                          //代付金额 单位分
            paras.Add("acct_name", accountName);                    //收款人姓名          
            paras.Add("acct_no", accountNo);                        //银行账号         
            paras.Add("bank_name", bankName);                       //银行名称 
            paras.Add("cnaps_name", "");                            //开户行支行
            paras.Add("nonce_str", Guid.NewGuid().ToString("N"));   //随机字符串
            paras.Add("notify_url", notifyUrl);                     //回调地址

            var sort = paras.OrderBy(k => k.Key);
            paras = sort.ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);

            string str = string.Join("&", paras.Where(x => !string.IsNullOrEmpty(x.Value)).Select(m => string.Format("{0}={1}", m.Key, m.Value))) + $"&key={MD5_Key}";
            string sign = Md5(str).ToLower();
            paras.Add("sign", sign);


            var param = string.Join("&", paras.Select(m => string.Format("{0}={1}", m.Key, m.Value)));
            WriteLine($"Id:{item.Id} param:{url}?{param}");          

            string responseData = string.Empty;
            int httpCode = HttpGet($"{url}?{param}" , "", ref responseData);

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
            if (json != null && json["resp_code"] != null) returnCode = json["resp_code"].ToString();
            if (json != null && json["resp_msg"] != null) returnMsg = json["resp_msg"].ToString();
            if (json != null && json["pay_trade_no"] != null) returnTranId = json["pay_trade_no"].ToString();


            //取得打款結果與訊息
            if (json != null && returnCode != "0000")
                intUpdStatus = 100; //自动打款失败    

            WriteLine($"Id:{item.Id} returnCode:{returnCode} returnMsg:{returnMsg}", intUpdStatus == 99 ? ConsoleColor.Green : ConsoleColor.Red);

            //更新結果
            string message = string.Empty;
            bool returnValue = UpdateConvertLogStatus(intUpdStatus, intTeamOrderId, returnTranId, returnMsg, ref message);

            WriteLine($"Id:{item.Id} msg:{message}", returnValue ? ConsoleColor.Green :ConsoleColor.Red);
        }
    }
    
}
