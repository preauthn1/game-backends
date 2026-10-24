using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using game.Entity.Pay;
using game.Infrastructure;
using static game.WebSite.PayConfig;
using Newtonsoft.Json.Linq;

namespace game.WebSite.PaymentHelper
{
    public class Uxin
    {
        public static string post(CDPayOrder order, string guid, string tradeCode, ref string message)
        {
            string url = string.Empty;

            Dictionary<string, string> paras = new Dictionary<string, string>
            {
                { "pay_amount", order.Amount.ToString() },
                { "pay_applydate", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") },
                { "pay_bankcode", tradeCode },
                { "pay_callbackurl", Uxin_CallbackUrl },
                { "pay_memberid", Uxin_mchid },
                { "pay_notifyurl", Uxin_NotifyUrl },
                { "pay_orderid", order.Id.ToString() },
                { "key", Uxin_Key }
            };

            string parasData = string.Join("&", paras.Select((m => $"{m.Key}={m.Value}")));
            var pay_md5sign = Md5(parasData, Encoding.UTF8);

            paras.Remove("key");
            //paras.Add("pay_productname", "充值");
            paras.Add("pay_md5sign", pay_md5sign.ToUpper());

            var data = string.Join("&", paras.Select((m => $"{m.Key}={m.Value}")));
            SiteHelper.Log(guid, string.Format("{0} 支付请求 [data]({1}) {2}", order.Id, Uxin_URL, data), Uxin_logFile);


            //呼叫支付平台
            var responseString = SiteHelper.HttpPost(Uxin_URL, data);
            SiteHelper.Log(guid, string.Format("{0} 支付请求结果 [response] {1}", order.Id, responseString), Uxin_logFile);


            if ((responseString.StartsWith("{") && responseString.EndsWith("}")) ||
                (responseString.StartsWith("[") && responseString.EndsWith("]")))
            {
                JObject json = JObject.Parse(responseString);
                var status = json["status"] != null ? json["status"].ToString() : string.Empty;
                var msg = json["msg"] != null ? json["msg"].ToString() : string.Empty;

                message = msg;

                //更新狀態
                Service.CDFailOrder(order.Id, string.Empty, msg);
                SiteHelper.Log(guid, string.Format("{0} 支付请求失敗 {1} {2}", order.Id, status, msg), Uxin_logFile);
            }
            else
            {
                url = responseString;
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
                    SiteHelper.Log(logGuid, string.Format("支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), Uxin_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), Uxin_logFile);
                }

                Dictionary<string, string> parameters = null;

                if (!CheckSign(logGuid, out parameters))
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调非法签名"), Uxin_logFile);
                    return PayState.SignError;
                }

                var memberid = parameters.ContainsKey("memberid") ? parameters["memberid"] : string.Empty;
                var orderid = parameters.ContainsKey("orderid") ? parameters["orderid"] : string.Empty;
                var transaction_id = parameters.ContainsKey("transaction_id") ? parameters["transaction_id"] : string.Empty;
                var datetime = parameters.ContainsKey("datetime") ? parameters["datetime"] : string.Empty;
                var amount = parameters.ContainsKey("amount") ? parameters["amount"] : string.Empty;
                var returncode = parameters.ContainsKey("returncode") ? parameters["returncode"] : string.Empty;

                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(orderid));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 支付回调 訂單狀態已經完成,無須金幣充值", order.Id), Uxin_logFile);
                    return PayState.Success;
                }

                if (returncode != "00")
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 支付回调 订单失敗 returncode:{1}", order.Id, returncode), Uxin_logFile);
                    Service.CDFailOrder(order.Id, transaction_id, $"returncode:{returncode}");
                    return PayState.Error;
                }

                //進行金幣充值
                string msg = "";

                if (Convert.ToDouble(amount) == order.Amount)
                {
                    var data = $"完成支付";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, transaction_id, data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 支付回调 金幣充值成功 returncode:{returncode},transaction_id:{transaction_id},amount:{amount}", Uxin_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 支付回调 金幣充值错误 {msg}", Uxin_logFile);
                        Service.CDFailOrder(order.Id, transaction_id, $"{msg}");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, $"{order.Id}  支付回调 支付無效 returncode:{returncode},transaction_id:{transaction_id},amount:{amount}", Uxin_logFile);
                Service.CDFailOrder(order.Id, transaction_id, $"returncode:{returncode},transaction_id:{transaction_id},amount:{amount}");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("支付回调 异常错误 {0}", ex.ToString()), Uxin_logFile);
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
            parameters = parameters.Where(x => !string.IsNullOrEmpty(x.Value)).OrderBy(o => o.Key).ToDictionary(o => o.Key, p => p.Value);

            var str = string.Join("&", parameters.Select(m => string.Format("{0}={1}", m.Key, m.Value))) + "&key=" + Uxin_Key;
            var cSign = Md5(str, Encoding.UTF8).ToUpper();
            SiteHelper.Log(guid, string.Format("支付回调签名 cSign={0} sign={1} srt={2}", cSign, sign, str), Uxin_logFile);

            return sign == cSign;
        }
    }
}