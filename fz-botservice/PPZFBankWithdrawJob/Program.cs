using BankWithdrawJob;
using BankWithdrawJob.Entity;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PPZFBankWithdrawJob
{
    class Program : BaseBankWithdrawJob
    {
        static string GroupID
        {
            get { return ConfigurationManager.AppSettings["groupID"] ?? string.Empty; }
        }

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
            string tradeAmt = (item.Amount / 100).ToString();
            string merchantID = MerId;
            string accountName = item.RealName;
            string accountNo = item.Account;
            string bankAddress = string.Empty;
            string bankCity = string.Empty;
            string bankID = item.BankID;
            string bankNo = bank == null ? "0":bank.TradeCode;
            string bankProvince = string.Empty;
            string accountMobile = item.LoginName;

            Dictionary<string, string> paras = new Dictionary<string, string>();
            paras.Add("service", "withdraw");               //网关服务
            paras.Add("merchantID", merchantID);            //商户号       
            paras.Add("accountName", accountName);          //*收款人姓名          
            paras.Add("accountNo", accountNo);              //*支付宝账号   
            //paras.Add("bankAddress", bankAddress);          //*收款开户行地址
            //paras.Add("bankCity", bankCity);                //*收款开户行市
            //paras.Add("bankID", bankID);                  //收款银行编号     
            paras.Add("bankNo", bankNo);                    //*收款银行编号      
            //paras.Add("bankProvince", bankProvince);        //*收款开户行省    
            paras.Add("accountMobile", accountMobile);      //*手机号            
            paras.Add("groupID", GroupID);                  //渠道群组编
            paras.Add("orderNo", orderNo);                  //商户订单号
            paras.Add("tradeAmt", tradeAmt);                //代付金额 单位元            


            string str = $"service=withdraw&" +
                         $"merchantID={merchantID}&" +
                         $"accountMobile={accountMobile}&" +
                         $"accountName={accountName}&" +
                         $"accountNo={accountNo}&" +
                         //$"bankAddress={bankAddress}&" +
                         //$"bankCity={bankCity}&" +
                         $"bankNo={bankNo}&" +
                         //$"bankProvince={bankProvince}&" +
                         $"groupID={GroupID}&" +
                         $"orderNo={orderNo}&" +
                         $"tradeAmt={tradeAmt}{MD5_Key}";

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
            if (json != null && json["errcode"] != null) returnCode = json["errcode"].ToString();
            if (json != null && json["errmsg"] != null) returnMsg = json["errmsg"].ToString();
            if (json != null && json["tradeNo"] != null) returnTranId = json["tradeNo"].ToString();


            //取得打款結果與訊息
            if (json != null && returnCode != "0")
                intUpdStatus = 100; //自动打款失败    

            WriteLine($"Id:{item.Id} returnCode:{returnCode} returnMsg:{returnMsg}", intUpdStatus == 99 ? ConsoleColor.Green : ConsoleColor.Red);

            //更新結果
            string message = string.Empty;
            bool returnValue = UpdateConvertLogStatus(intUpdStatus, intTeamOrderId, returnTranId, returnMsg, ref message);

            WriteLine($"Id:{item.Id} msg:{message}", returnValue ? ConsoleColor.Green : ConsoleColor.Red);
        }
    }
}
