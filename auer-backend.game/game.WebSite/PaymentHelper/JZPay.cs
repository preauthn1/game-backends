using game.Entity.Pay;
using game.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using example.invalid;
using System.Linq;
using example.invalid;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using static game.WebSite.PayConfig;

namespace game.WebSite.PaymentHelper
{
    public class JZPay
    {
        /// <summary>
        /// 创建订单(跳转网页链接)
        /// </summary>
        /// <param name="order"></param>
        /// <param name="guid"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string post(CDPayOrder order, string guid, string tradeCode, ref string message)
        {
            long unixTimestamp = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;                       

            Dictionary<string, string> body = new Dictionary<string, string>
            {
                { "jUserId", order.UserID.ToString()  },
                { "jUserIp", order.IPAddress},
                { "jOrderId", order.Id.ToString()},
                { "orderType", "1"},
                { "payWay", tradeCode},
                { "amount", order.Amount.ToString("F2")},
                { "currency","CNY"},             
                { "notifyUrl",JZPay_NotifyUrl},
            };
            var data = string.Join("&", body.Select(m => string.Format("{0}={1}", m.Key, m.Value)));

            Dictionary<string, string> paras = new Dictionary<string, string>
            {
                { "merchantId", JZPay_MemberID  },              
                { "signatureMethod", "HmacSHA256"},
                { "signatureVersion", "1"},
                { "timestamp", unixTimestamp.ToString()},              
            };

            foreach (var b in body)
                paras.Add(b.Key, b.Value);

            var signdata = string.Join("&", paras.OrderBy(x=>x.Key).Select(m => string.Format("{0}={1}", m.Key, m.Value)));
            SiteHelper.Log(guid, $" 支付请求加密参数 [data]({signdata}) ", JZPay_logFile);

            var sign = HmacSHA256(signdata, JZPay_Key).ToUpper();
            paras.Add("signature", sign);           
            var getdata = string.Join("&", paras.Select(m => string.Format("{0}={1}", m.Key, m.Value)));

            string PayUrl = $"{JZPay_Url}?merchantId={JZPay_MemberID}&signatureMethod=HmacSHA256&signatureVersion=1&timestamp={unixTimestamp}&signature={sign}";
            SiteHelper.Log(guid, $"{order.Id} 支付请求 [data]({PayUrl}) {data}", JZPay_logFile);

            //呼叫支付平台
            string responseString = SiteHelper.HttpPost(PayUrl, data);
            SiteHelper.Log(guid, $"{order.Id} 支付请求 [data] {responseString}", JZPay_logFile);

            JObject json = JObject.Parse(responseString);

            string url;
            var result = json["code"] != null ? json["code"].ToString() : string.Empty;

            if (!result.Equals("0"))
            {
                var respmsg = json["message"] != null ? json["message"].ToString() : string.Empty;
                message = string.Format("{0} 支付请求失敗 {1}", order.Id, respmsg);

                //更新狀態               
                SiteHelper.Log(guid, message, JZPay_logFile);
                url = string.Empty;
            }
            else
            {
                var transID = json["data"]["orderId"] != null ? json["data"]["orderId"].ToString() : string.Empty;
                url = json["data"]["paymentUrl"] != null ? json["data"]["paymentUrl"].ToString() : string.Empty;              

                Service.CDModifyTransID(order.Id, transID);
            }

            return url;
        }


        /// <summary>
        /// 支付成功回调
        /// </summary>
        /// <param name="logGuid"></param>
        /// <returns></returns>
        public static PayState call(string logGuid)
        {
            try
            {
                var requestType = HttpContext.Current.Request.RequestType;
                if (requestType == "GET")
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), JZPay_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), JZPay_logFile);
                }
                Dictionary<string, string> parameters = null;

                if (!CheckSign(logGuid, out parameters))
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调非法签名"), JZPay_logFile);
                    return PayState.SignError;
                }
             
                var status = parameters.ContainsKey("status") ? parameters["status"] : string.Empty;              
                var orderid = parameters.ContainsKey("jOrderId") ? parameters["jOrderId"] : string.Empty;
                var amount = parameters.ContainsKey("amount") ? parameters["amount"] : string.Empty;
                var tradeId = parameters.ContainsKey("orderId") ? parameters["orderId"] : string.Empty;

                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(orderid));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 支付回调 訂單狀態已經完成,無須金幣充值", order.Id), JZPay_logFile);
                    return PayState.Success;
                }

                //進行金幣充值
                string msg = "";

                if ((status.Equals("2") || status.Equals("3")) && ((Convert.ToDecimal(amount)) == order.Amount))
                {
                    var data = $"支付回调 完成支付: 支付订单号: {orderid} , 渠道订单号: {tradeId}, 状态: {status}";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, tradeId, data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 支付回调 金幣充值成功 支付订单号:{orderid} 金额:{order.Amount} 渠道订单号:{tradeId}", JZPay_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 支付回调 金幣充值错误 {msg}", JZPay_logFile);

                        Service.CDFailOrder(order.Id, "0", $"支付订单号:{orderid} , 渠道订单号: {tradeId} , 支付金额: {amount}, 状态: {status}");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, $"{order.Id}  支付回调 支付無效 支付订单号:{orderid}, 渠道订单号:{tradeId}, 支付金额:{amount}, 状态:{status}", JZPay_logFile);
                Service.CDFailOrder(order.Id, "0", $"支付订单号:{orderid} , 渠道订单号: {tradeId} , 支付金额: {amount}, 状态: {status}");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("支付回调 异常错误 {0}", ex.ToString()), JZPay_logFile);
                return PayState.Error;
            }
        }

        private static string HmacSHA256(string message, string secret)
        {
            secret = [REDACTED] ?? "";
            var encoding = new System.Text.UTF8Encoding();
            byte[] keyByte = encoding.GetBytes(secret);
            byte[] messageBytes = encoding.GetBytes(message);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                return BitConverter.ToString(hashmessage).Replace("-", "").ToLower();                
            }
        }

        private static bool CheckSign(string guid, out Dictionary<string, string> parameters)
        {
            Dictionary<string, string> requestTypeByGet;           
            requestTypeByGet = parameters = HttpContext.Current.Request.QueryString.AllKeys.Where(x => !string.IsNullOrEmpty(x)).Cast<string>()
                .ToDictionary(k => k, v => HttpContext.Current.Request[v]).Where(x => x.Key != null).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
           
            parameters = HttpContext.Current.Request.Form.AllKeys.Cast<string>()
                .ToDictionary(k => k, v => HttpContext.Current.Request[v]).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
         
            foreach (var p in requestTypeByGet)
                parameters.Add(p.Key, p.Value);

            var sign = parameters["signature"];
            parameters.Remove("signature");
            var parasData = string.Join("&", parameters.Where(x => !string.IsNullOrEmpty(x.Value)).OrderBy(x => x.Key).Select(m => string.Format("{0}={1}", m.Key, m.Value)));
            var cSign = HmacSHA256(parasData, JZPay_Key).ToUpper();
          
            SiteHelper.Log(guid, $"支付回调签名 cSign={cSign} sign={sign}", JZPay_logFile);
            return sign == cSign;
        }
    }
}