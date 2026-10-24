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
    public class Yilian
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
            string dt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Dictionary<string, string> paras = new Dictionary<string, string>
            {
                { "pay_memberid", Yilian_memberid },
                { "pay_orderid", order.Id.ToString()},
                { "pay_applydate",dt},
                { "pay_bankcode", tradeCode},
                { "pay_notifyurl", Yilian_NotifyUrl},
                { "pay_callbackurl", Yilian_CallBackUrl},
                { "pay_amount", (order.Amount).ToString()  }
            };

            var sort = paras.OrderBy(k => k.Key);
            paras = sort.ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);
            string parasData = string.Join("&", paras.Select((m => $"{m.Key}={m.Value}"))) + $"&key={Yilian_Key}";

            var pay_md5sign = Md5(parasData, Encoding.UTF8).ToUpper();
            paras.Add("pay_md5sign", pay_md5sign);

            var url = $"<html><body><form method='post' name='form' action='{Yilian_PayURL}' enctype=\"application/x-www-form-urlencoded\">" +
                      string.Join("", paras.Select(m => string.Format("<input type='hidden' name='{0}' value='{1}' >", m.Key, m.Value))) +
                      "</form><script>window.onload = function(){document.forms['form'].submit();}</script></body></html>";

            SiteHelper.Log(guid, $"{order.Id} 支付请求 [url]({Yilian_PayURL})[data] {url}", Yilian_logFile);

            var api = Service.CreateRedirectData((int)PayGatewayType.Yilian, url);

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
                    SiteHelper.Log(logGuid, string.Format("支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), Yilian_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), Yilian_logFile);
                }
                Dictionary<string, string> parameters = null;

                if (!CheckSign(logGuid, out parameters))
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调非法签名"), Yilian_logFile);
                    return PayState.SignError;
                }
           
                var returncode = parameters.ContainsKey("returncode") ? parameters["returncode"] : string.Empty;            
                var orderid = parameters.ContainsKey("orderid") ? parameters["orderid"] : string.Empty;             
                var amount = parameters.ContainsKey("amount") ? parameters["amount"] : string.Empty;
                var transID = parameters.ContainsKey("transaction_id") ? parameters["transaction_id"] : string.Empty;

                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(orderid));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 支付回调 訂單狀態已經完成,無須金幣充值", order.Id), Yilian_logFile);
                    return PayState.Success;
                }

                //進行金幣充值
                string msg = "";

                if (returncode.Equals("00") && ((Convert.ToDecimal(amount)) == order.Amount))
                {
                    var data = $"支付回调 完成支付: 渠道订单号: {transID}, 状态: {returncode}";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, transID, data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 支付回调 金幣充值成功 金额: {amount} 渠道订单号: {transID}", Yilian_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 支付回调 金幣充值错误 {msg}", Yilian_logFile);

                        Service.CDFailOrder(order.Id, transID, $"渠道订单号: {transID}, 支付金额: {amount}, 状态: {returncode}");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, $"{order.Id}  支付回调 支付無效 支付订单号:{orderid}, 渠道订单号:{transID}, 支付金额:{amount}, 状态:{returncode}", Yilian_logFile);
                Service.CDFailOrder(order.Id, transID, $"渠道订单号: {transID}, 支付金额: {amount}, 状态: {returncode}");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("支付回调 异常错误 {0}", ex.ToString()), Yilian_logFile);
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
            string parasData = string.Join("&", parameters.Where(x=>!string.IsNullOrEmpty(x.Value)).Select((m => $"{m.Key}={m.Value}")));

            var cSign = Md5(parasData + $"&key={Yilian_Key}", Encoding.UTF8).ToUpper();

            SiteHelper.Log(guid, $"支付回调签名 cSign={cSign} sign={sign}", Yilian_logFile);
            return sign == cSign;
        }
    }
}