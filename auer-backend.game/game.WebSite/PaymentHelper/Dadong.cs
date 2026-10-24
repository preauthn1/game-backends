using game.Entity.Money;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using example.invalid;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using static game.WebSite.PayConfig;

namespace game.WebSite.PaymentHelper
{
    public class Dadong
    {
        public static PayState WithdrawCallback(string logGuid, string key)
        {
            try
            {
                var requestType = HttpContext.Current.Request.RequestType;

                Stream stream = HttpContext.Current.Request.InputStream;
                var bytes = new byte[stream.Length];
                stream.Read(bytes, 0, (int)stream.Length);
                var responseString = Encoding.UTF8.GetString(bytes);

                SiteHelper.Log(logGuid, $"代付回调 [FormData {requestType}] {responseString}", Dadong_logFile);


                JObject json = JObject.Parse(responseString);

                //SUCCESS:成功
                var code = json["code"] != null ? json["code"].ToString() : string.Empty; 
                var sign = json["sign"] != null ? json["sign"].ToString() : string.Empty; 
                var response = json["biz"] != null ? json["biz"].ToString() : string.Empty;

                Dictionary<string, string> parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(response);
                parameters.Add("sign", sign);

                if (!CheckSign(logGuid, key, parameters))
                {
                    SiteHelper.Log(logGuid, string.Format("提现异步回调非法签名"), Dadong_logFile);
                    return PayState.SignError;
                }

                var orderId = parameters.ContainsKey("merchantOrderNo") ? parameters["merchantOrderNo"] : string.Empty;
                var orderAmt = parameters.ContainsKey("orderAmount") ? parameters["orderAmount"] : string.Empty;
                var orderStatus = parameters.ContainsKey("orderStatus") ? parameters["orderStatus"] : string.Empty;
                var transId = parameters.ContainsKey("platformOrderNo") ? parameters["platformOrderNo"] : string.Empty;
                var transData = json["msg"] != null ? json["msg"].ToString() : string.Empty;
                var status = ConvertLogStatus.Queueing;

                /* 
                 * WaitTransfer:等待划款
                 * Transfered:已划款
                 * Success:划款成功
                 * Fail:划款失败
                 */
                if (code == "SUCCESS" && orderStatus == "Success") status = ConvertLogStatus.AlreadyPaid;
                else if (code == "SUCCESS" && (orderStatus == "WaitTransfer" || orderStatus == "Transfered")) status = ConvertLogStatus.Queueing;
                else status = ConvertLogStatus.AutoPaymentFailed;

                var res = Service.WithdrawCallback(orderId, (int)status, transId, transData);
                if (res.Result)
                {
                    SiteHelper.Log(logGuid, $"提现回调成功 orderId={orderId}&orderState={status}&tranSeqId={transId}&orderAmt={orderAmt}", Dadong_logFile);
                    return PayState.Success;
                }
                else
                {
                    SiteHelper.Log(logGuid, $"提现回调错误 {res.Message}", Dadong_logFile);
                    return PayState.Error;
                }
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("提现异步回调异常错误 {0}", ex.ToString()), Dadong_logFile);
                return PayState.Error;
            }
        }

        private static bool CheckSign(string guid, string key, Dictionary<string, string> parameters)
        {
            var sign = parameters["sign"];
            parameters.Remove("sign");
            parameters = parameters.OrderBy(o => o.Key).ToDictionary(o => o.Key, p => p.Value);

            var str = string.Join("&", parameters.Where(x=>!string.IsNullOrEmpty(x.Value)).Select(m => string.Format("{0}={1}", m.Key, m.Value))) + key;
            var cSign = Md5(str, Encoding.UTF8).ToLower();       

            SiteHelper.Log(guid, $"代付回调签名 cSign={cSign} sign={sign}", Dadong_logFile);
            return sign == cSign;
        }

        private static string Md5(string str, Encoding ec)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            return BitConverter.ToString(md5.ComputeHash(ec.GetBytes(str))).Replace("-", "");
        }
    }
}