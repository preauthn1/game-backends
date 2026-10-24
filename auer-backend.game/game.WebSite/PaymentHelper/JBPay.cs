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
    public class JBPay
    {
        /// <summary>
        /// 创建订单(跳转网页链接)
        /// </summary>
        /// <param name="order"></param>
        /// <param name="guid"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string post(CDPayOrder order, string guid, string tradeCode)
        {
            string d = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Dictionary<string, string> paras = new Dictionary<string, string>
            {
                { "customerno", jbpay_MerchantNo },
                { "channeltype", tradeCode},
                { "customerbillno", order.Id.ToString()},
                { "orderamount", (order.Amount.ToString("f2"))  },
                { "customerbilltime",d},               
                { "notifyurl", jbpay_NotifyUrl},
                { "ip", order.IPAddress},
                { "devicetype", "web"},
                { "customeruser", order.UserID.ToString()},
            };
            var signdata = string.Join("&", paras.OrderBy(x => x.Key).Select(m => string.Format("{0}={1}", m.Key, m.Value))) + "&key=" + jbpay_Key;
            SiteHelper.Log(guid, $"加密资料 [data] {signdata}", jbpay_logFile);
            var sign = Md5(signdata, Encoding.UTF8).ToLower();
            paras.Add("sign", sign);

            var url = $"<html><body><form method='post' name='form' action='{jbpay_Url}' enctype='application/x-www-form-urlencoded'>" +
                      string.Join("", paras.Select(m => string.Format("<input type='hidden' name='{0}' value='{1}' >", m.Key, m.Value))) +
                      "</form><script>window.onload = function(){document.forms['form'].submit();}</script></body></html>";

            SiteHelper.Log(guid, $"{order.Id} 支付请求 [url]({jbpay_Url})[data] {url}", jbpay_logFile);

            var api = Service.CreateRedirectData((int)PayGatewayType.JBPay, url);

            if (api.Result)
                return api.Extend.ToString();
            else
                return string.Empty;
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
                    SiteHelper.Log(logGuid, string.Format("聚宝支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), jbpay_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("聚宝支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), jbpay_logFile);
                }
                Dictionary<string, string> parameters = null;

                if (!CheckSign(logGuid, out parameters))
                {
                    SiteHelper.Log(logGuid, string.Format("聚宝支付回调非法签名"), jbpay_logFile);
                    return PayState.SignError;
                }

                //支付状态，00表示支付成功;
                var returncode = parameters.ContainsKey("paystatus") ? parameters["paystatus"] : string.Empty;

                //商家订单号 
                var orderid = parameters.ContainsKey("customerbillno") ? parameters["customerbillno"] : string.Empty;

                //订单金额
                var amount = parameters.ContainsKey("preorderamount") ? parameters["preorderamount"] : string.Empty;

                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(orderid));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 聚宝支付回调 訂單狀態已經完成,無須金幣充值", order.Id), jbpay_logFile);
                    return PayState.Success;
                }

                //進行金幣充值
                string msg = "";

                if (returncode.Equals("SUCCESS") && ((Convert.ToDecimal(amount)) == order.Amount))
                {
                    var data = $"聚宝支付回调 完成支付: 支付订单号: {orderid} , 渠道订单号: {"0"}, 状态: {returncode}";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, "0", data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 聚宝支付回调 金幣充值成功 支付订单号:{orderid} 金额:amount={order.Amount} 渠道订单号:{"0"}", jbpay_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 聚宝支付回调 金幣充值错误 {msg}", jbpay_logFile);

                        Service.CDFailOrder(order.Id, "0", $"聚宝支付订单号:{orderid} , 渠道订单号: {"0"} , 支付金额(分): {(Convert.ToDecimal(amount) * 100).ToString()}, 状态: {returncode}");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, $"{order.Id}  聚宝支付回调 支付無效 支付订单号:{orderid}, 渠道订单号:{"0"}, 支付金额(分):{(Convert.ToDecimal(amount) * 100).ToString()}, 状态:{returncode}", jbpay_logFile);
                Service.CDFailOrder(order.Id, "0", $"聚宝支付订单号:{orderid} , 渠道订单号: {"0"} , 支付金额(分): {(Convert.ToDecimal(amount) * 100).ToString()}, 状态: {returncode}");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("聚宝支付回调 异常错误 {0}", ex.ToString()), jbpay_logFile);
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
                parameters = HttpContext.Current.Request.QueryString.AllKeys.Where(x => !string.IsNullOrEmpty(x)).Cast<string>()
                    .ToDictionary(k => k, v => HttpContext.Current.Request[v]).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            }
            else
            {
                parameters = HttpContext.Current.Request.Form.AllKeys.Where(x => !string.IsNullOrEmpty(x)).Cast<string>()
                    .ToDictionary(k => k, v => HttpContext.Current.Request[v]).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            }


            var sign = parameters.ContainsKey("sign") ? parameters["sign"] : string.Empty;
            parameters.Remove("sign");

            var signdata = string.Join("&", parameters.Where(x=> !string.IsNullOrEmpty(x.Value)).Select(m => string.Format("{0}={1}", m.Key, m.Value))) + "&key=" + jbpay_Key;

            SiteHelper.Log(guid, $"回传加密资料 [data] {signdata}", jbpay_logFile);           


            var cSign = Md5(signdata, Encoding.UTF8).ToLower();
            SiteHelper.Log(guid, $"聚宝支付回调签名 cSign={cSign} sign={sign}", jbpay_logFile);
            return sign == cSign;
        }
    }
}