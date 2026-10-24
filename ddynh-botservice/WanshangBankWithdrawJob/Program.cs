using BankWithdrawJob;
using BankWithdrawJob.Entity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WanshangBankWithdrawJob
{
    class Program : BaseBankWithdrawJob
    {
        static string System_RSApubKey
        {
            get { return ConfigurationManager.AppSettings["System_RSApubKey"]; }
        }

        static string RSApubKey
        {
            get { return ConfigurationManager.AppSettings["RSApubKey"]; }
        }

        static string RSApriKey
        {
            get { return ConfigurationManager.AppSettings["RSApriKey"]; }
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
            string order_num = item.Id;
            string money = item.Amount.ToString();
            string accountNo = item.Account;
            string bankName = item.BankName;
            string accountName = item.RealName;
            string timestamp = Convert.ToInt64(DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds * 1000).ToString();

            Dictionary<string, string> paras = new Dictionary<string, string>();

            paras.Add("tranCode", "2101");                              //交易码          
            paras.Add("merId", MerId);                                  //商户号
            paras.Add("orderId", item.Id);                              //商户订单号
            paras.Add("tranDate", DateTime.Now.ToString("yyyyMMdd"));   //交易日期
            paras.Add("nonceStr", Guid.NewGuid().ToString("N"));        //随机字符串
            paras.Add("txnAmt", money);                                 //代付金额 单位分
            paras.Add("accountNo", accountNo);                          //账户 ,到账卡号            
            paras.Add("bankName", bankName);                            //银行名称 
            paras.Add("accountName", accountName);                      //账户名          
            paras.Add("cnaps", "308290003298");                         //联行号            
            paras.Add("accountType", "1");                              //0 对公 1 对私  
            paras.Add("notifyUrl", CallbackUrl);                        //回调地址

            //var sort = paras.OrderBy(k => k.Key);
            //paras = sort.ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);

            //string str = string.Join("&", paras.Select(m => string.Format("{0}={1}", m.Key, m.Value))) + $"&key={MD5KEY}";
            //string sign = Md5(str, Encoding.UTF8).ToUpper();
            //string rsa_key = RSAPublicKeyJava2DotNet(RSAPubKey);
            //string rsa_sign = RSAEncryptSignData(rsa_key, sign);

            //Dictionary<string, string> paras_sign = new Dictionary<string, string>();
            //paras_sign.Add("sign", rsa_sign);

            //var req = new REQ()
            //{
            //    REQ_HEAD = paras_sign,
            //    REQ_BODY = paras
            //};

            //var data = JsonConvert.SerializeObject(req);

            //var param = string.Join("&", paras.Select(m => string.Format("{0}={1}", m.Key, m.Value)));
            //WriteLine($"Id:{item.Id} param:{param}");          

            //string responseData = "";
            //int httpCode = HttpPost(WithdrawUrl, data, "application/json", ref responseData);
            //WriteLine($"Id:{item.Id}  return:{httpCode} response:{responseData}", httpCode == 200 ? ConsoleColor.Yellow : ConsoleColor.Red);


            //int intUpdStatus = 99; //預設自动化進行中
            //int intTeamOrderId = Convert.ToInt32(item.Id); //兑换订单号           
            //string returnCode = "null";
            //string returnMsg = "null";
            //string returnTranId = "0";

            //JObject json;
            //try
            //{
            //    json = JObject.Parse(responseData);
            //}
            //catch { json = null; }


            ////取得訪問結果與訊息
            //if (json != null && json["REP_BODY"]["rspcode"] != null) returnCode = json["REP_BODY"]["rspcode"].ToString();
            //if (json != null && json["REP_BODY"]["rspmsg"] != null) returnMsg = json["REP_BODY"]["rspmsg"].ToString();
            //if (json != null && json["REP_BODY"]["tranId"] != null) returnTranId = json["REP_BODY"]["tranId"].ToString();

            //if (!string.IsNullOrEmpty(returnMsg))
            //    returnMsg = HexToString(returnMsg);

            ////取得打款結果與訊息
            //if (json != null && returnCode.ToLower() != "000000")
            //{
            //    intUpdStatus = 100; //自动打款失败               
            //}

            //WriteLine($"Id:{item.Id} returnCode:{returnCode} returnMsg:{returnMsg}", intUpdStatus == 99 ? ConsoleColor.Green : ConsoleColor.Red);

            //string message = string.Empty;
            //bool returnValue = UpdateConvertLogStatus(intUpdStatus, intTeamOrderId, returnTranId, returnMsg, ref message);

            //WriteLine($"Id:{item.Id} msg:{message}", returnValue ? ConsoleColor.Green : ConsoleColor.Red);
        }
    }
}
