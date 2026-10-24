using game.Entity.Pay;
using game.Infrastructure;
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
    public class Yongwang
    {
        public static string post(CDPayOrder order, string guid, string tradeCode, ref string message)
        {
            string url = string.Empty;
            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            Dictionary<string, string> paras = new Dictionary<string, string>
            {
                { "UserID", Yongwang_ID },
                { "OrderID", order.Id.ToString() },
                { "FaceValue", order.Amount.ToString("N2") },
                { "ChannelID", tradeCode },
                { "TimeStamp", unixTimestamp.ToString() },
                { "Version", "V2.0" },
                { "IP", order.IPAddress },
                { "ResultType", "1" },
                { "NotifyUrl", Yongwang_notifyUrl }
            };

            var sort = paras.OrderBy(k => k.Key);
            paras = sort.ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);

            string parasData = string.Join("&", paras.Select((m => $"{m.Key}={m.Value}"))) + "&key=" + Yongwang_Key1;
            var pay_md5sign = Md5(parasData, Encoding.UTF8).ToLower();
            paras.Add("sign", pay_md5sign);

            var data = string.Join("&", paras.Select((m => $"{m.Key}={m.Value}")));
            SiteHelper.Log(guid, string.Format("{0} 支付请求 [data]({1}) {2}", order.Id, Yongwang_Url, data), Yongwang_logFile);

            //呼叫支付平台
            var responseString = SiteHelper.HttpPost(Yongwang_Url, data);
            SiteHelper.Log(guid, string.Format("{0} 支付请求结果 [response] {1}", order.Id, responseString), Yongwang_logFile);

            JObject json = JObject.Parse(responseString);
            var retCode = json["Resultcode"] != null ? json["Resultcode"].ToString() : string.Empty;

            if (!retCode.Equals("0"))
            {
                var respmsg = json["Resultdesc"] != null ? json["Resultdesc"].ToString() : string.Empty;
                message = string.Format("{0} 支付请求失敗 {1}", order.Id, respmsg);

                //更新狀態
                Service.CDFailOrder(order.Id, string.Empty, respmsg);
                SiteHelper.Log(guid, message, Yongwang_logFile);
            }
            else
            {
                var orderIDP = json["OrderIDP"] != null ? json["OrderIDP"].ToString() : string.Empty;

                Service.CDModifyTransID(order.Id, orderIDP);
                url = json["PayUrl"] != null ? json["PayUrl"].ToString() : string.Empty;
            }

            return url;
        }

        public static PayState call(string logGuid)
        {
            try
            {
                var requestType = HttpContext.Current.Request.RequestType;
                if (requestType == "GET")
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), Yongwang_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), Yongwang_logFile);
                }  

                Dictionary<string, string> parameters = null;                

                if (!CheckSign(logGuid, out parameters))
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调非法签名"), Yongwang_logFile);
                    return PayState.SignError;
                }
               
                var orderID = parameters.ContainsKey("OrderID") ? parameters["OrderID"] : string.Empty;
                var orderIDP = parameters.ContainsKey("OrderIDP") ? parameters["OrderIDP"] : string.Empty;
                var faceValue = parameters.ContainsKey("FaceValue") ? parameters["FaceValue"] : string.Empty;
                var amount = parameters.ContainsKey("Amount") ? parameters["Amount"] : string.Empty;
                var payTime = parameters.ContainsKey("PayTime") ? parameters["PayTime"] : string.Empty;
                var payState = parameters.ContainsKey("PayState") ? parameters["PayState"] : string.Empty;


                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(orderID));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 支付回调 訂單狀態已經完成,無須金幣充值", order.Id), Yongwang_logFile);
                    return PayState.Success;
                }

                if (payState != "1")
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 支付回调 订单生成: {1}", order.Id, orderIDP), Yongwang_logFile);
                    return PayState.Error;
                }

                //進行金幣充值
                string msg = "";

                if (payState == "1" && (Convert.ToDouble(faceValue) == order.Amount))
                {
                    var data = $"支付回调 完成支付: 支付时间: {payTime}";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, orderIDP, data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, string.Format("{0} 支付回调 金幣充值成功 支付订单号:{1} 金额:{2}", order.Id, orderIDP, faceValue), Yongwang_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, string.Format("{0} 支付回调 金幣充值错误 {1}", order.Id, msg), Yongwang_logFile);
                        Service.CDFailOrder(order.Id, orderIDP, $"支付订单号:{orderIDP} , 支付金额: {faceValue}, 状态: {payState}");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, string.Format("{0}  支付回调 支付無效 支付订单号:{1}, 支付金额:{2}, 状态:{3}", order.Id, orderIDP, faceValue, payState), Yongwang_logFile);
                Service.CDFailOrder(order.Id, orderIDP, $"支付订单号:{orderIDP} , 支付金额: {faceValue}, 状态: {payState}");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("支付回调 异常错误 {0}", ex.ToString()), Yongwang_logFile);
                return PayState.Error;
            }
        }

        private static string Md5(string str, Encoding ec)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            return BitConverter.ToString(md5.ComputeHash(ec.GetBytes(str))).Replace("-", "");
        }

        private static bool CheckSign(string guid, out Dictionary<string, string> parameters)
        {
            var requestType = HttpContext.Current.Request.RequestType;

            if (requestType == "GET")
            {
                parameters = HttpContext.Current.Request.QueryString.AllKeys.Cast<string>()
                    .ToDictionary(k => k, v => HttpContext.Current.Request[v]).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            }
            else
            {
                parameters = HttpContext.Current.Request.Form.AllKeys.Cast<string>()
                    .ToDictionary(k => k, v => HttpContext.Current.Request[v]).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            }

            var sign = parameters["sign"];
            parameters.Remove("sign");
            parameters = parameters.OrderBy(o => o.Key).ToDictionary(o => o.Key, p => p.Value);

            var str = string.Join("&", parameters.Where(x => !string.IsNullOrEmpty(x.Value)).Select(m => string.Format("{0}={1}", m.Key, m.Value))) + "&key=" + Yongwang_Key2;
            var cSign = Md5(str, Encoding.UTF8).ToLower();
            SiteHelper.Log(guid, string.Format("支付回调签名 cSign={0} sign={1} srt={2}", cSign, sign, str), Yongwang_logFile);

            return sign == cSign;
        }
    }
}