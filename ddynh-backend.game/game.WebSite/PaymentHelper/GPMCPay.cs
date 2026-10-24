using game.Entity.Pay;
using game.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using static game.WebSite.PayConfig;

namespace game.WebSite.PaymentHelper
{
    public class GPMCPay
    {
        // <summary>
        /// 创建订单(跳转网页链接)
        /// </summary>
        /// <param name="order"></param>
        /// <param name="guid"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string post(CDPayOrder order, string guid, string tradeCode)
        {
            var a = HttpUtility.UrlEncode(gpmc_appid);
            var sign = base64($"banktype={tradeCode}&callbackurl={gpmc_shopCallBackUrl}&paymoney={(order.Amount).ToString()}&usernumber={order.Id.ToString()}");
            var p = HttpUtility.UrlEncode(sign);
            var s = Md5(sign, Encoding.UTF8).ToLower();

            string Payurl = gpmc_PayURL + $"?a={a}&p={p}&s={s}";

            var url = $"<html><body><form method='post' name='form' action='{Payurl}' enctype=\"application/x-www-form-urlencoded\">" +
                       "</form><script>window.onload = function(){document.forms['form'].submit();}</script></body></html>";

            SiteHelper.Log(guid, $"{order.Id} 支付请求 [url]({Payurl})[data] {url}", gpmc_logFile);

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
                    SiteHelper.Log(logGuid, string.Format("金牌MC支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), gpmc_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("金牌MC支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), gpmc_logFile);
                }
                Dictionary<string, string> parameters = null;

                if (!CheckSign(logGuid, out parameters))
                {
                    SiteHelper.Log(logGuid, string.Format("金牌MC支付回调非法签名"), gpmc_logFile);
                    return PayState.SignError;
                }

                //支付状态，1表示支付成功;
                var orderstatus = parameters.ContainsKey("orderstatus") ? parameters["orderstatus"] : string.Empty;

                //商家订单号 
                var orderid = parameters.ContainsKey("usernumber") ? parameters["usernumber"] : string.Empty;

                //订单金额
                var amount = parameters.ContainsKey("paymoney") ? parameters["paymoney"] : string.Empty;

                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(orderid));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 金牌MC支付回调 訂單狀態已經完成,無須金幣充值", order.Id), gpmc_logFile);
                    return PayState.Success;
                }

                //進行金幣充值
                string msg = "";

                if (orderstatus.Equals("1") && ((Convert.ToDecimal(amount)) == order.Amount))
                {
                    var data = $"金牌MC支付回调 完成支付: 支付订单号: {orderid} , 渠道订单号: {"0"}, 状态: {orderstatus}";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, "0", data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 金牌MC支付回调 金幣充值成功 支付订单号:{orderid} 金额:amount={order.Amount} 渠道订单号:{"0"}", gpmc_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 金牌MC支付回调 金幣充值错误 {msg}", gpmc_logFile);

                        Service.CDFailOrder(order.Id, "0", $"金牌MC支付订单号:{orderid} , 渠道订单号: {"0"} , 支付金额(分): {(Convert.ToInt64(amount) * 100).ToString()}, 状态: {orderstatus}");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, $"{order.Id}  金牌MC支付回调 支付無效 支付订单号:{orderid}, 渠道订单号:{"0"}, 支付金额(分):{(Convert.ToInt64(amount) * 100).ToString()}, 状态:{orderstatus}", gpmc_logFile);
                Service.CDFailOrder(order.Id, "0", $"金牌MC支付订单号:{orderid} , 渠道订单号: {"0"} , 支付金额(分): {(Convert.ToInt64(amount) * 100).ToString()}, 状态: {orderstatus}");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("金牌MC支付回调 异常错误 {0}", ex.ToString()), gpmc_logFile);
                return PayState.Error;
            }
        }


        private static string Md5(string str, Encoding ec)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            return BitConverter.ToString(md5.ComputeHash(ec.GetBytes(str))).Replace("-", "");
        }
        public static string base64(string code)
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(code));
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

            var sign = parameters.ContainsKey("sign") ? parameters["sign"] : string.Empty;
            var id = parameters.ContainsKey("id") ? parameters["id"] : string.Empty;
            var paymoney = parameters.ContainsKey("paymoney") ? parameters["paymoney"] : string.Empty;
            var usernumber = parameters.ContainsKey("usernumber") ? parameters["usernumber"] : string.Empty; //商户订单号
            var orderstatus = parameters.ContainsKey("orderstatus") ? parameters["orderstatus"] : string.Empty;
            var banktype = parameters.ContainsKey("banktype") ? parameters["banktype"] : string.Empty;
            var s = parameters.ContainsKey("s") ? parameters["s"] : string.Empty;

            var cSign = Md5($"banktype={banktype}&id={id}&orderstatus={orderstatus}&paymoney={paymoney}&sign={sign}&usernumber={usernumber}", Encoding.UTF8).ToLower();
            SiteHelper.Log(guid, $"金牌MC支付回调签名 cSign={cSign} sign={s}", gpmc_logFile);
            return s == cSign;
        }

    }
}