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

namespace DSDFWithdrawJob
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
            //MD5_Key = "283a86f5e5f88fca42ba06b514424aef";
            //MerId = "2fb2f4bfdd0dfb44";
            //CallbackUrl = "http://0.0.0.0/Convert/MayiWithdrawCallback";
            //Process(item);
        }

        static void Process(ConvertLog item)
        {
            string money = (item.Amount / 100).ToString();
            string timestamp = Convert.ToInt64(DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds * 1000).ToString();
            string notifyUrl = HttpUtility.UrlEncode(CallbackUrl);
            string realName = HttpUtility.UrlEncode(item.RealName);

            Dictionary<string, string> paras = new Dictionary<string, string>();
            paras.Add("cid", MerId);           //创建接入商公司时生成的数字，登录风云聚合收付平台获取
            paras.Add("uid", item.UserID);              //此订单的会员ID，运营网站系统的会员唯一标识
            paras.Add("time", timestamp);             //此次操作的时间，格式: UNIX时间戳 秒值，超过３分钟则为非法订单 (即此参数值和风云系统收到请求的时间差不能超过３分钟)
            paras.Add("amount", money);             //提现的金额，单位 元 ，最多可带两位小数点，如：1.23, 2.6, 60
            paras.Add("order_id", item.Id);              //接入商网站生成的订单号，保证唯一性
            paras.Add("to_bank_flag", item.BankCode);     //银行类型: ABC,CMB...  
            paras.Add("to_cardnumber", item.Account);   //银行卡号
            paras.Add("to_username", realName);         //持卡人姓名
            paras.Add("location", "北京");            //银行卡省份: 定义见文档最后面，可直接填中文
            paras.Add("city", "北京市");               //银行卡城市: 定义见文档最后面，可直接填中文

            var content = JsonConvert.SerializeObject(paras);

            string responseData = string.Empty;

            var headers = new Dictionary<string, string>()
            {
                { "Content-Hmac", MD5_Key},
                { "Content-Type", "application/json" }
            };

            WriteLine($"Id:{item.Id} WithdrawUrl:{WithdrawUrl}\nContent-Hmac:{MD5_Key}\ncontent:{content}");//mars add
            int httpCode = HttpPost(WithdrawUrl, content, headers, ref responseData);
            WriteLine($"Id:{item.Id} http:{httpCode}\nresponse:{responseData}", httpCode == 200 ? ConsoleColor.Yellow : ConsoleColor.Red);


            int intUpdStatus = 99; //預設自动化進行中
            int intTeamOrderId = Convert.ToInt32(item.Id); //兑换订单号           
            string returnCode = "null";
            string returnMsg = "null";
            string returnTranId = "0";

            //{"success":true}
            //{"success":false,"msg":"invalid params"}
            JObject json;
            try
            {
                json = JObject.Parse(responseData);
            }
            catch { json = null; }

            //取得訪問結果與訊息
            if (json != null && json["success"] != null) returnCode = json["success"].ToString();
            if (json != null && json["msg"] != null) returnMsg = json["msg"].ToString();

            //取得打款結果與訊息
            if (json != null && returnCode.ToLower() != "true")
                intUpdStatus = 100;     //自动打款失败

            WriteLine($"Id:{item.Id} returnCode:{returnCode} returnMsg:{returnMsg}", intUpdStatus == 99 ? ConsoleColor.Green : ConsoleColor.Red);

            //更新結果
            string message = string.Empty;
            bool returnValue = UpdateConvertLogStatus(intUpdStatus, intTeamOrderId, returnTranId, returnMsg, ref message);

            WriteLine($"Id:{item.Id} intUpdStatus:{intUpdStatus} msg:{message}", returnValue ? ConsoleColor.Green : ConsoleColor.Red);
        }
    }
}
