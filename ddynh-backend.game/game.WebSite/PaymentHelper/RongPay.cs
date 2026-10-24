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
    public class RongPay
    {
     

        /// <summary>
        /// 创建订单(跳转网页链接)
        /// </summary>
        /// <param name="order"></param>
        /// <param name="guid"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string post(CDPayOrder order, string guid, string tradeCode, ref string outMessage)
        {

            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;


            Dictionary<string, string> paras = new Dictionary<string, string>
            {
                { "money", order.Amount.ToString()  },
                { "clientOrderId", order.Id.ToString()},
                { "channelNO", rong_channelno},               
                { "callback", rong_shopCallBackUrl},
            };

            var code = Md5($"{order.Amount.ToString()}{order.Id.ToString()}{rong_key}", Encoding.UTF8).ToLower();
            paras.Add("code", code);
            var data = string.Join("&", paras.Select(m => string.Format("{0}={1}", m.Key, m.Value)));

            SiteHelper.Log(guid, $"{order.Id} 支付请求 [data]({rong_PayURL}) {data}", rong_logFile);
            return (rong_PayURL + "?" + data);
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
                    SiteHelper.Log(logGuid, string.Format("融支付支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), rong_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("融支付支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), rong_logFile);
                }
                Dictionary<string, string> parameters = null;

                if (!CheckSign(logGuid, out parameters))
                {
                    SiteHelper.Log(logGuid, string.Format("融支付支付回调非法签名"), rong_logFile);
                    return PayState.SignError;
                }

                //支付状态，pay_success表示支付成功;
                var orderstatus = parameters.ContainsKey("action") ? parameters["action"] : string.Empty;

                //商家订单号 
                var orderid = parameters.ContainsKey("clientOrderId") ? parameters["clientOrderId"] : string.Empty;

                //订单金额
                var amount = parameters.ContainsKey("money") ? parameters["money"] : string.Empty;

                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(orderid));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 融支付支付回调 訂單狀態已經完成,無須金幣充值", order.Id), rong_logFile);
                    return PayState.Success;
                }

                //進行金幣充值
                string msg = "";

                if (orderstatus.Equals("pay_success") && ((Convert.ToDecimal(amount)) == order.Amount))
                {
                    var data = $"融支付支付回调 完成支付: 支付订单号: {orderid} , 渠道订单号: {"0"}, 状态: {orderstatus}";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, "0", data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 融支付支付回调 金幣充值成功 支付订单号:{orderid} 金额:amount={order.Amount} 渠道订单号:{"0"}", rong_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 融支付支付回调 金幣充值错误 {msg}", rong_logFile);

                        Service.CDFailOrder(order.Id, "0", $"融支付支付订单号:{orderid} , 渠道订单号: {"0"} , 支付金额(分): {(Convert.ToInt64(amount) * 100).ToString()}, 状态: {orderstatus}");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, $"{order.Id}  融支付支付回调 支付無效 支付订单号:{orderid}, 渠道订单号:{"0"}, 支付金额(分):{(Convert.ToInt64(amount) * 100).ToString()}, 状态:{orderstatus}", rong_logFile);
                Service.CDFailOrder(order.Id, "0", $"融支付支付订单号:{orderid} , 渠道订单号: {"0"} , 支付金额(分): {(Convert.ToInt64(amount) * 100).ToString()}, 状态: {orderstatus}");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("融支付支付回调 异常错误 {0}", ex.ToString()), rong_logFile);
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

            var clientOrderId = parameters.ContainsKey("clientOrderId") ? parameters["clientOrderId"] : string.Empty;
            var channelNO = parameters.ContainsKey("channelNO") ? parameters["channelNO"] : string.Empty;
            var money = parameters.ContainsKey("money") ? parameters["money"] : string.Empty;
            var depositableMoney = parameters.ContainsKey("depositableMoney") ? parameters["depositableMoney"] : string.Empty; 
            var action = parameters.ContainsKey("action") ? parameters["action"] : string.Empty;
            var payTime = parameters.ContainsKey("payTime") ? parameters["payTime"] : string.Empty;
            var code = parameters.ContainsKey("code") ? parameters["code"] : string.Empty;

            var cSign = Md5($"{money}{clientOrderId}{channelNO}{rong_key}", Encoding.UTF8).ToLower();
            SiteHelper.Log(guid, $"融支付支付回调签名 cSign={cSign} sign={code}", rong_logFile);
            return code == cSign;
        }

    }
}