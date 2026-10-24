using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using game.Entity.Pay;
using game.Infrastructure;
using static game.WebSite.PayConfig;
using Newtonsoft.Json;
using System.Threading.Tasks;
using example.invalid.Http.Headers;
using example.invalid.Http;
using Newtonsoft.Json.Linq;

namespace game.WebSite.PaymentHelper
{
    public class NiuNiuPay
    {
        public static string post(CDPayOrder order, string guid, string tradeCode)
        {
            string url = string.Empty;
            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            Dictionary<string, string> paras = new Dictionary<string, string>
            {
                { "login_id", niuniu_loginid },
                { "create_time", unixTimestamp.ToString() },
                { "create_ip", order.IPAddress },
                { "nonce", "8502" },
                { "sign_type", "MD5" },
                { "pay_type", tradeCode },
                { "order_type", "1" },
                { "order_sn", order.Id.ToString() },
                { "amount", (order.Amount).ToString() },
                { "send_currency", "cny" },
                { "recv_currency", "cny" },
                { "notify_url", niuniu_shopNotifyUrl },

            };

            var pay_md5sign = Md5($"amount={(order.Amount).ToString()}&create_ip={order.IPAddress}&create_time={unixTimestamp.ToString()}&login_id={niuniu_loginid}&nonce=8502&notify_url={niuniu_shopNotifyUrl}&order_sn={ order.Id.ToString()}&order_type=1&pay_type={tradeCode}&recv_currency=cny&send_currency=cny&sign_type=MD5&api_secret=[REDACTED]", Encoding.UTF8).ToLower();
            paras.Add("sign", pay_md5sign);
            var data = string.Join("&", paras.Select(m => string.Format("{0}={1}", m.Key, m.Value)));

          
            SiteHelper.Log(guid, string.Format("{0} 支付请求 [data]({1}) {2}", order.Id, niuniu_PayURL, data), niuniu_logFile);

            //呼叫支付平台
            var responseString = SiteHelper.HttpPost(niuniu_PayURL, data);
            SiteHelper.Log(guid, string.Format("{0} 支付请求结果 [response] {1}", order.Id, responseString), niuniu_logFile);

            JObject json = JObject.Parse(responseString);
            var retCode = json["code"] != null ? json["code"].ToString() : string.Empty;

            if (!retCode.Equals("0"))
            {
                var respmsg = json["msg"] != null ? json["msg"].ToString() : string.Empty;
                //更新狀態
                Service.CDFailOrder(order.Id, string.Empty, respmsg);                
            }
            else
            {
                var rtnurl = json["http_url"] != null ? json["http_url"].ToString() : string.Empty;
                url = rtnurl;
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
                    SiteHelper.Log(logGuid, string.Format("支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), niuniu_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), niuniu_logFile);
                }

                Dictionary<string, string> parameters = null;

                if (!CheckSign(logGuid, out parameters))
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调非法签名"), niuniu_logFile);
                    return PayState.SignError;
                }

                var payOrderId = parameters.ContainsKey("order_sn") ? parameters["order_sn"] : string.Empty;
                var amount = parameters.ContainsKey("order_amount") ? parameters["order_amount"] : string.Empty;
                var status = parameters.ContainsKey("pay_state") ? parameters["pay_state"] : string.Empty;
                var channelOrderNo = parameters.ContainsKey("order_id") ? parameters["order_id"] : string.Empty;
               

                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(payOrderId));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 支付回调 訂單狀態已經完成,無須金幣充值", order.Id), niuniu_logFile);
                    return PayState.Success;
                }

              

                //進行金幣充值
                string msg = "";

                if ((status =="1") && (Convert.ToDecimal(amount) == order.Amount ))
                {
                    var data = $"支付回调 完成支付: 支付订单号: {payOrderId} , 渠道订单号: {channelOrderNo}, 状态: {status}";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, payOrderId, data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, string.Format("{0} 支付回调 金幣充值成功 支付订单号:{1} 金额:amount={2} 渠道订单号:{3}", order.Id, payOrderId, order.Amount, channelOrderNo), niuniu_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, string.Format("{0} 支付回调 金幣充值错误 {1}", order.Id, msg), niuniu_logFile);
                        Service.CDFailOrder(order.Id, payOrderId, $"支付订单号:{payOrderId} , 渠道订单号: {channelOrderNo} , 支付金额(分): {amount}, 状态: {status}");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, string.Format("{0}  支付回调 支付無效 支付订单号:{1}, 渠道订单号:{2}, 支付金额(分):{3}, 状态:{4}", order.Id, payOrderId, channelOrderNo, amount, status), niuniu_logFile);
                Service.CDFailOrder(order.Id, payOrderId, $"支付订单号:{payOrderId} , 渠道订单号: {channelOrderNo} , 支付金额(分): {amount}, 状态: {status}");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("支付回调 异常错误 {0}", ex.ToString()), niuniu_logFile);
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

            if (parameters.ContainsKey("params"))
            {
                parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(parameters["params"]);
            }
           

            var sign = parameters["sign"];
            parameters.Remove("sign");
            parameters = parameters.OrderBy(o => o.Key).ToDictionary(o => o.Key, p => p.Value);

            var str = string.Join("&", parameters.Select(m => string.Format("{0}={1}", m.Key, m.Value))) + "&api_secret=" + niuniu_Key;
            var cSign = Md5(str, Encoding.UTF8).ToLower();                                                   
            SiteHelper.Log(guid, string.Format("支付回调签名 cSign={0} sign={1} srt={2}", cSign, sign, str), niuniu_logFile);

            return sign == cSign;
        }

    }
}