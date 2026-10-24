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
    public class BDPay
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
            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            Dictionary<string, string> paras = new Dictionary<string, string>
            {
                { "fxid", BDPay_MerchantNo },
                { "fxorderid", order.Id.ToString("D12")},
                { "fxamount", (order.Amount).ToString()  },
                { "fxnotifyurl", BDPay_NotifyUrl},
                { "fxpaytype", tradeCode},                
               
            };
           
            var signdata = $"{BDPay_MerchantNo}{order.Id.ToString("D12")}{(order.Amount).ToString()}{BDPay_NotifyUrl}{BDPay_Key}";
            //支付请求的签名方式
            var sign = Md5($"{signdata}", Encoding.UTF8).ToLower();
            SiteHelper.Log(guid, $"{order.Id} 支付请求签名 [data]{sign} {signdata}", BDPay_logFile);       
            paras.Add("fxsign", sign);
            var url = $"<html><body><form method='post' name='form' action='{BDPay_Url}' enctype='application/x-www-form-urlencoded'>" +
                     string.Join("", paras.Select(m => string.Format("<input type='hidden' name='{0}' value='{1}' >", m.Key, m.Value))) +
                     "</form><script>window.onload = function(){document.forms['form'].submit();}</script></body></html>";

            SiteHelper.Log(guid, $"{order.Id} 支付请求 [url]({BDPay_Url})[data] {url}", BDPay_logFile);

            var api = Service.CreateRedirectData((int)PayGatewayType.BDPay, url);

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
                    SiteHelper.Log(logGuid, string.Format("BD支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), BDPay_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("BD支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), BDPay_logFile);
                }
                Dictionary<string, string> parameters = null;

                if (!CheckSign(logGuid, out parameters))
                {
                    SiteHelper.Log(logGuid, string.Format("BD支付回调非法签名"), BDPay_logFile);
                    return PayState.SignError;
                }

                //支付状态，00表示支付成功;
                var returncode = parameters.ContainsKey("fxstatus") ? parameters["fxstatus"] : string.Empty;

                //商家订单号 
                var orderid = parameters.ContainsKey("fxorderid") ? parameters["fxorderid"] : string.Empty;

                //订单金额
                var amount = parameters.ContainsKey("fxamount") ? parameters["fxamount"] : string.Empty;

                //平台订单号 
                var tradeno = parameters.ContainsKey("fxtranid") ? parameters["fxtranid"] : string.Empty;


                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(orderid));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} BD支付回调 訂單狀態已經完成,無須金幣充值", order.Id), BDPay_logFile);
                    return PayState.Success;
                }

                //進行金幣充值
                string msg = "";

                if (returncode.Equals("succ") && ((Convert.ToDecimal(amount)) == order.Amount))
                {
                    var data = $"BD支付回调 完成支付: 支付订单号: {orderid} , 渠道订单号: {tradeno}, 状态: {returncode}";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, tradeno, data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} BD支付回调 金幣充值成功 支付订单号:{orderid} 金额:amount={order.Amount} 渠道订单号:{tradeno}", BDPay_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} BD支付回调 金幣充值错误 {msg}", BDPay_logFile);

                        Service.CDFailOrder(order.Id, tradeno, $"BD支付订单号:{orderid} , 渠道订单号: {tradeno} , 支付金额(分): {(Convert.ToDecimal(amount) * 100).ToString()}, 状态: {returncode}");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, $"{order.Id}  BD支付回调 支付無效 支付订单号:{orderid}, 渠道订单号:{"0"}, 支付金额(分):{(Convert.ToDecimal(amount) * 100).ToString()}, 状态:{returncode}", BDPay_logFile);
                Service.CDFailOrder(order.Id, tradeno, $"BD支付订单号:{orderid} , 渠道订单号: {tradeno} , 支付金额(分): {(Convert.ToDecimal(amount) * 100).ToString()}, 状态: {returncode}");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("BD支付回调 异常错误 {0}", ex.ToString()), BDPay_logFile);
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

            var sign = parameters.ContainsKey("fxsign") ? parameters["fxsign"] : string.Empty;
            var fxid = parameters.ContainsKey("fxid") ? parameters["fxid"] : string.Empty;
            var fxorderid = parameters.ContainsKey("fxorderid") ? parameters["fxorderid"] : string.Empty;
            var fxtranid = parameters.ContainsKey("fxtranid") ? parameters["fxtranid"] : string.Empty;
            var fxamount = parameters.ContainsKey("fxamount") ? parameters["fxamount"] : string.Empty;
            var fxamount_succ = parameters.ContainsKey("fxamount_succ") ? parameters["fxamount_succ"] : string.Empty;
            var fxstatus = parameters.ContainsKey("fxstatus") ? parameters["fxstatus"] : string.Empty;


            var signdata = $"{fxstatus}{BDPay_MerchantNo}{fxamount_succ}{fxorderid}{fxamount}{BDPay_Key}";
            //支付请求的签名方式
            var cSign = Md5($"{signdata}", Encoding.UTF8).ToLower();
            SiteHelper.Log(guid, $"BD支付回调签名 cSign={cSign} sign={sign}", BDPay_logFile);
            return sign == cSign;
        }

    }
}