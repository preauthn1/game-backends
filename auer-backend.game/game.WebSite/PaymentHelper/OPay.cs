using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using game.Entity.Pay;
using game.Infrastructure;
using static game.WebSite.PayConfig;

namespace game.WebSite.PaymentHelper
{
    public class OPay
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
            string url = string.Empty;
            string d = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Dictionary<string, string> paras = new Dictionary<string, string>
            {
                { "pay_memberid", OPay_memberid },
                { "pay_orderid", order.Id.ToString()},
                { "pay_applydate",d},
                { "pay_bankcode", tradeCode},
                { "pay_notifyurl", OPay_NotifyUrl},
                { "pay_callbackurl", OPay_CallBackUrl},
                { "pay_amount", (order.Amount).ToString()  },
            };

            var sort = paras.OrderBy(k => k.Key);
            paras = sort.ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);
            string parasData = string.Join("&", paras.Select((m => $"{m.Key}={m.Value}"))) + $"&key={OPay_Key}";

            var pay_md5sign = Md5(parasData, Encoding.UTF8).ToUpper();
            paras.Add("pay_md5sign", pay_md5sign);

            url = $"<html><body><form method='post' name='form' action='{OPay_PayURL}' enctype=\"application/x-www-form-urlencoded\">" +
                    string.Join("", paras.Select(m => string.Format("<input type='hidden' name='{0}' value='{1}' >", m.Key, m.Value))) +
                    "</form><script>window.onload = function(){document.forms['form'].submit();}</script></body></html>";

            SiteHelper.Log(guid, $"{order.Id} 支付请求 [data]({url}) ", OPay_logFile);

            var api = Service.CreateRedirectData((int)PayGatewayType.OPay, url);

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
                    SiteHelper.Log(logGuid, string.Format("支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), OPay_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), OPay_logFile);
                }
                Dictionary<string, string> parameters = null;

                if (!CheckSign(logGuid, out parameters))
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调非法签名"), OPay_logFile);
                    return PayState.SignError;
                }

                //支付状态，00表示支付成功;
                var returncode = parameters.ContainsKey("returncode") ? parameters["returncode"] : string.Empty;
                var orderid = parameters.ContainsKey("orderid") ? parameters["orderid"] : string.Empty;
                var amount = parameters.ContainsKey("amount") ? parameters["amount"] : string.Empty;
                var transID = parameters.ContainsKey("transaction_id") ? parameters["transaction_id"] : string.Empty;

                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(orderid));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 支付回调 訂單狀態已經完成,無須金幣充值", order.Id), OPay_logFile);
                    return PayState.Success;
                }

                //進行金幣充值
                string msg = "";

                if (returncode.Equals("00") && (Math.Round(Convert.ToDecimal(amount), 0) == order.Amount))
                {
                    
                    var data = $"支付回调 完成支付: 渠道订单号:{transID}, 金额: {amount}, 状态: {returncode}";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, transID, data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 支付回调 金幣充值成功 渠道订单号:{transID} 金额:{order.Amount}", OPay_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 支付回调 金幣充值错误 {msg}", OPay_logFile);

                        Service.CDFailOrder(order.Id, transID, $"渠道订单号:{transID}, 金额: {amount}, 状态: {returncode}");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, $"{order.Id}  支付回调 支付無效 支付订单号:{orderid}, 渠道订单号:{transID}, 支付金额:{amount}, 状态:{returncode}", OPay_logFile);
                Service.CDFailOrder(order.Id, transID, $"支付订单号:{orderid}, 支付金额: {amount}, 状态: {returncode}");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("支付回调 异常错误 {0}", ex.ToString()), OPay_logFile);
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

            var sign = parameters.ContainsKey("sign") ? parameters["sign"] : string.Empty;
            parameters.Remove("sign");

            var sort = parameters.OrderBy(k => k.Key);
            parameters = sort.ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);
            string parasData = string.Join("&", parameters.Where(x => !string.IsNullOrEmpty(x.Value)).Select((m => $"{m.Key}={m.Value}")));

            var cSign = Md5(parasData + $"&key={OPay_Key}", Encoding.UTF8).ToUpper();

            SiteHelper.Log(guid, $"支付回调签名 cSign={cSign} sign={sign}", OPay_logFile);
            return sign == cSign;
        }
    }
}