using BankWithdrawJob;
using BankWithdrawJob.Entity;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using example.invalid;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace MTPayBankWithdraw
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
                Thread.Sleep(5000);
            }
        }

        static void Process(ConvertLog item)
        {
            string orderNo = item.Id;
            string memberCode = item.UserID;
            string amount = (item.Amount / 100).ToString("F2");
            string accountName = item.RealName;
            string accountNo = item.Account;
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");

            //WithdrawInfoData
            JObject withdrawJson;
            try
            {
                withdrawJson = JObject.Parse(item.WithdrawInfoData);
            }
            catch { withdrawJson = null; }

            var merchantID = withdrawJson["MERCHANT_NO"] != null ? withdrawJson["MERCHANT_NO"].ToString() : string.Empty;  //商户号
            var md5Key = withdrawJson["MERCHANT_KEY"] != null ? withdrawJson["MERCHANT_KEY"].ToString() : string.Empty;  //商户号密钥

            Dictionary<string, string> requestData = new Dictionary<string, string>();

            requestData.Add("amount", amount);                  //金额，限额依据商户设定，小数点两位
            requestData.Add("bankCode", item.BankID);           //商户客户提款银行代码，
            //requestData.Add("bankBranch", );                  //分行名称; 非必傳
            requestData.Add("bankAccountNo", accountNo);        //商户客户提款银行账号  
            requestData.Add("bankAcoountName", accountName);    //商户客户提款银行户名 
            requestData.Add("memberCode", memberCode);          //商户客户提款银行户名       
            requestData.Add("orderNo", orderNo);                //商户订单号，不能重复            
            requestData.Add("orderTime", timestamp);            //商户系统发起订单时间，格式为yyyyMMddHHmmss
            requestData.Add("notifyUrl", CallbackUrl);          //通知商户系统订单完成网址

            //編成 Base64 字串
            string jsonString = "{" + string.Join(",", requestData.Select(d => $"\"{d.Key}\":\"{d.Value}\"")) + "}";
            WriteLine($"Id:{item.Id} base64encode before:{jsonString}");
            string request = Encode(jsonString, Encoding.GetEncoding("utf-8"));
            WriteLine($"Id:{item.Id} base64encode after:{request}");

            Dictionary<string, string> paras = new Dictionary<string, string>
                {
                    { "merchantNo", merchantID },
                    { "request", request },
                    { "version", "1.0" },
                    { "key", md5Key }
                };

            string parasData = string.Join("&", paras.Select((m => $"{m.Key}:{m.Value}")));
            WriteLine($"Id:{item.Id} md5 before:{parasData}");

            var pay_md5sign = Md5(parasData, Encoding.UTF8);
            WriteLine($"Id:{item.Id} md5 after:{pay_md5sign}");

            paras.Add("signMsg", pay_md5sign);
            paras.Remove("key");
            paras.Remove("request");
            paras.Add("request", HttpUtility.UrlEncode(request));

            var content = string.Join("&", paras.Select(m => $"{m.Key}={m.Value}"));
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
            string returnVersion = "1.0";
            string returnMerchantNo = string.Empty;
            string returnSign = string.Empty;
            string returnResponse = string.Empty;
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
            if (json != null && json["version"] != null) returnVersion = json["version"].ToString();//系统版本号，当前最新版本为 1.0
            if (json != null && json["merchantNo"] != null) returnMerchantNo = json["merchantNo"].ToString();//商户号，注册完毕后提供，GBP 开头共九码
            if (json != null && json["signMsg"] != null) returnSign = json["signMsg"].ToString();//MD5 验签，长度为 32，为防止传输过程中被篡改
            if (json != null && json["resCode"] != null) returnCode = json["resCode"].ToString();//系统回应三位数代码
            if (json != null && json["resMsg"] != null) returnMsg = json["resMsg"].ToString();//系统回应代码说明
            if (json != null && json["response"] != null) returnResponse = json["response"].ToString();//请求结果内容

            //取得打款結果與訊息
            if (json != null && returnCode != "000")
            {
                intUpdStatus = 100; //自动打款失败  
            }

            //返回商戶錯誤
            else if (returnMerchantNo != merchantID)
            {
                WriteLine($"Id:{item.Id} 商户号異常 MerId:{merchantID} returnMerID:{returnMerchantNo}");
                intUpdStatus = 100; //自动打款失败 
            }

            //驗簽
            Dictionary<string, string> res = new Dictionary<string, string>
                    {
                        { "merchantNo", returnMerchantNo },
                        { "resCode", returnCode },
                        { "resMsg", returnMsg },
                        { "response", returnResponse },
                        { "version", returnVersion },
                        { "key", md5Key }
                    };
            string resString = string.Join("&", res.Select((m => $"{m.Key}:{m.Value}")));
            WriteLine($"Id:{item.Id} 驗簽:{resString} returnMerID:{returnMerchantNo}");
            var sign = Md5(resString, Encoding.UTF8).ToUpper();
            if (returnSign != sign)
            {
                WriteLine($"Id:{item.Id} 代付回调非法签名: sign{sign} returnSign:{returnSign}");
                intUpdStatus = 100; //自动打款失败 
            }
            else
            {
                //Base64字串還原
                string decodeString = Encoding.GetEncoding("utf-8").GetString(Convert.FromBase64String(returnResponse));
                WriteLine($"Id:{item.Id} base64 decode after:{decodeString}");
                JObject resJson = JObject.Parse(decodeString);


                var resCode = resJson["code"] != null ? resJson["code"].ToString() : string.Empty;  //四位数 API 处理代码,请参考  5.2 订单检验 回应代码表 ,0000 代表成功,其他皆为失败
                returnMsg = resJson["msg"] != null ? resJson["msg"].ToString() : string.Empty;  //API 处理代码说明

                //取得打款結果與訊息
                if (resJson != null && resCode != "0000")
                {
                    intUpdStatus = 100; //自动打款失败  
                }
                else
                {
                    var resAmount = resJson["amount"] != null ? resJson["amount"].ToString() : string.Empty;  //此笔订单的交易金额
                    var resFee = resJson["fee"] != null ? resJson["fee"].ToString() : string.Empty; //手续费,根据商户设定 
                    var resOrderNo = resJson["orderNo"] != null ? resJson["orderNo"].ToString() : string.Empty; //商户提交的订单号
                    returnTranId = resJson["gbpOrderNo"] != null ? resJson["gbpOrderNo"].ToString() : string.Empty;  //此笔订单的我方系统订单号,不会重复
                    var resCreateTime = resJson["createTime"] != null ? resJson["createTime"].ToString() : string.Empty;  //我方系统建立订单时间
                }
            }

            WriteLine($"Id:{item.Id} returnCode:{returnCode} returnMsg:{returnMsg}", intUpdStatus == 99 ? ConsoleColor.Green : ConsoleColor.Red);

            //更新結果
            string message = string.Empty;
            bool returnValue = UpdateConvertLogStatus(intUpdStatus, intTeamOrderId, returnTranId, returnMsg, ref message);

            WriteLine($"Id:{item.Id} msg:{message}", returnValue ? ConsoleColor.Green : ConsoleColor.Red);
        }

        private static string md5(string str, Encoding ec)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            return BitConverter.ToString(md5.ComputeHash(ec.GetBytes(str))).Replace("-", "");
        }

        private static string Encode(string data, System.Text.Encoding encoding = null)
        {

            encoding = encoding ?? Encoding.UTF8;

            byte[] bytes = encoding.GetBytes(data);

            return Convert.ToBase64String(bytes);
        }
    }
}
