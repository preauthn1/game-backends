using game.Entity.Pay;
using game.Infrastructure;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using static game.WebSite.PayConfig;

namespace game.WebSite.PaymentHelper
{
    public class Wangwangbao
    {
        public static string post(CDPayOrder order, string guid, string tradeCode, ref string message)
        {
            string url = string.Empty;
            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            Dictionary<string, string> paras = new Dictionary<string, string>
            {
                { "ApiMethod", "OnLinePay"},
                { "Version", "V2.0"},
                { "MerID", Wangwangbao_MerchantNo },
                { "TradeNum", order.Id.ToString()},
                { "Amount", order.Amount.ToString("F2") },              
                { "NotifyUrl", Wangwangbao_NotifyUrl},             
                { "TransTime", DateTime.Now.ToString("yyyyMMddHHmmss")},
                { "PayType", tradeCode},
                { "SignType" , "MD5" }                      
            };
       
            var signdata = string.Join("&", paras.Where(x => !string.IsNullOrEmpty(x.Value)).OrderBy(x => x.Key).Select(m => string.Format("{0}={1}", m.Key, m.Value)));
            var sign = Md5($"{signdata}&key={Wangwangbao_Key}", Encoding.UTF8).ToLower();         
            paras.Add("Sign", sign);
            

            url = $"<html><body><form method='post' name='form' action='{Wangwangbao_Url}' enctype=\"application/x-www-form-urlencoded\">" +
                     string.Join("", paras.Select(m => string.Format("<input type='hidden' name='{0}' value='{1}' >", m.Key, m.Value))) +
                     "</form><script>window.onload = function(){document.forms['form'].submit();}</script></body></html>";

            SiteHelper.Log(guid, $"{order.Id} 支付请求 [data] {url}", Wangwangbao_logFile);

            var api = Service.CreateRedirectData((int)PayGatewayType.Wangwangbao, url);

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
                    SiteHelper.Log(logGuid, string.Format("支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), Wangwangbao_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), Wangwangbao_logFile);
                }
                Dictionary<string, string> parameters = null;

                if (!CheckSign(logGuid, out parameters))
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调非法签名"), Wangwangbao_logFile);
                    return PayState.SignError;
                }
               
                var returncode = parameters.ContainsKey("Status") ? parameters["Status"] : string.Empty;
                var orderid = parameters.ContainsKey("TradeNum") ? parameters["TradeNum"] : string.Empty;
                var amount = parameters.ContainsKey("Amount") ? parameters["Amount"] : string.Empty;
                var tradeno = parameters.ContainsKey("OrderNum") ? parameters["OrderNum"] : string.Empty;


                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(orderid));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 支付回调 訂單狀態已經完成,無須金幣充值", order.Id), Wangwangbao_logFile);
                    return PayState.Success;
                }

                //進行金幣充值
                string msg = "";

                if (returncode.Equals("01") && ((Convert.ToDecimal(amount)) == order.Amount))
                {
                    var data = $"支付回调 完成支付: 支付订单号: {orderid} , 渠道订单号: {tradeno}, 状态: {returncode}";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, tradeno, data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 支付回调 金幣充值成功 支付订单号:{orderid} 金额:{order.Amount} 渠道订单号:{tradeno}", Wangwangbao_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 支付回调 金幣充值错误 {msg}", Wangwangbao_logFile);

                        Service.CDFailOrder(order.Id, tradeno, $"支付订单号:{orderid} , 渠道订单号: {tradeno} , 支付金额: {amount}, 状态: {returncode}");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, $"{order.Id} 支付回调 支付無效 支付订单号:{orderid}, 渠道订单号:{"0"}, 支付金额:{amount}, 状态:{returncode}", Wangwangbao_logFile);
                Service.CDFailOrder(order.Id, tradeno, $"支付订单号:{orderid} , 渠道订单号: {tradeno} , 支付金额: {amount}, 状态: {returncode}");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("支付回调 异常错误 {0}", ex.ToString()), Wangwangbao_logFile);
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

            var sign = parameters["Sign"];
            parameters.Remove("Sign");
            parameters = parameters.OrderBy(o => o.Key).ToDictionary(o => o.Key, p => p.Value);

            var str = string.Join("&", parameters.Where(x => !string.IsNullOrEmpty(x.Value)).Select(m => string.Format("{0}={1}", m.Key, m.Value))) + "&key=" + Wangwangbao_Key;
            var cSign = Md5(str, Encoding.UTF8).ToLower();
            SiteHelper.Log(guid, string.Format("支付回调签名 cSign={0} sign={1} srt={2}", cSign, sign, str), Wangwangbao_logFile);

            return sign == cSign;
        }
    }
}