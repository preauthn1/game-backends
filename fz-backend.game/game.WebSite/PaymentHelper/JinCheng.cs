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
    public class JinCheng
    {
        /// <summary>
        /// 创建订单(跳转网页链接)
        /// 
        public static string post(CDPayOrder order, string guid, string tradeCode, ref string message)
        {

            Dictionary<string, string> paras = new Dictionary<string, string>
            {
                { "amount", order.Amount.ToString("f2")},
                { "create_time", order.CreateTime.ToString("yyyy-MM-dd HH:mm:ss")},
                { "extras", order.UserID.ToString()},
                { "merchant_code", JinCheng_MerchantCode },
                { "notify_url", JinCheng_NotifyUrl},
                { "order_no", order.Id.ToString()},
                { "return_url", JinCheng_ReturnUrl},
                { "source_ip", order.IPAddress},
                { "trade_type", tradeCode},
                { "key", JinCheng_Key}
            };

            string contentString = string.Join("&", paras.Select((kvp) => kvp.Key + "=" + kvp.Value.ToString()));
            var sign = Md5(contentString, Encoding.UTF8).ToLower();
            paras.Add("sign", sign);
            paras.Remove("key");


            var url = $"<html><body><form method='post' name='form' action='{JinCheng_Url}' enctype=\"application/x-www-form-urlencoded\">" +
                     string.Join("", paras.Select(m => string.Format("<input type='hidden' name='{0}' value='{1}' >", m.Key, m.Value))) +
                     "</form><script>window.onload = function(){document.forms['form'].submit();}</script></body></html>";

            SiteHelper.Log(guid, $"{order.Id} 支付请求 [data] {url}", JinCheng_logFile);

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
                    SiteHelper.Log(logGuid, string.Format("金城支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), JinCheng_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("金城支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), JinCheng_logFile);
                }
                Dictionary<string, string> parameters = null;

                if (!CheckSign(logGuid, out parameters))
                {
                    SiteHelper.Log(logGuid, string.Format("金城支付回调非法签名"), JinCheng_logFile);
                    return PayState.SignError;
                }


                var merchant_code = parameters.ContainsKey("merchant_code") ? parameters["merchant_code"] : string.Empty;   //商户号
                var order_no = parameters.ContainsKey("order_no") ? parameters["order_no"] : string.Empty;                  //商户唯一订单
                var order_amount = parameters.ContainsKey("order_amount") ? parameters["order_amount"] : string.Empty;      //商户订单总金额
                var order_time = parameters.ContainsKey("order_time") ? parameters["order_time"] : string.Empty;            //商户订单时间
                var extras = parameters.ContainsKey("extras") ? parameters["extras"] : string.Empty;                        //回传参数   
                var trade_no = parameters.ContainsKey("trade_no") ? parameters["trade_no"] : string.Empty;                  //支付平台订单号   
                var trade_time = parameters.ContainsKey("trade_time") ? parameters["trade_time"] : string.Empty;            //支付平台订单时间   
                var trade_status = parameters.ContainsKey("trade_status") ? parameters["trade_status"] : string.Empty;      //交易状态（success 交易成功 failed 交易失败 paying 交易中）


                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(order_no));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 金城支付回调 訂單狀態已經完成,無須金幣充值", order.Id), JinCheng_logFile);
                    return PayState.Success;
                }

                //進行金幣充值
                string msg = "";

                if (trade_status.Equals("success") && ((Convert.ToDecimal(order_amount)) == order.Amount))
                {
                    var data = $"金城支付回调 完成支付: 支付订单号: {order_no} , 渠道订单号: {"0"}, 状态: {trade_status}";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, "0", data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 金城支付回调 金幣充值成功 支付订单号:{order_no} 金额:amount={order.Amount} 渠道订单号:{"0"}", JinCheng_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 金城支付回调 金幣充值错误 {msg}", JinCheng_logFile);

                        Service.CDFailOrder(order.Id, "0", $"金城支付订单号:{order_no} , 渠道订单号: {"0"} , 支付金额(分): {(Convert.ToInt64(order_amount) * 100).ToString()}, 状态: {trade_status}");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, $"{order.Id}  金城支付回调 支付無效 支付订单号:{order_no}, 渠道订单号:{"0"}, 支付金额(分):{(Convert.ToInt64(order_amount) * 100).ToString()}, 状态:{trade_status}", JinCheng_logFile);
                Service.CDFailOrder(order.Id, "0", $"金城支付订单号:{order_no} , 渠道订单号: {"0"} , 支付金额(分): {(Convert.ToInt64(order_amount) * 100).ToString()}, 状态: {trade_status}");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("金城支付回调 异常错误 {0}", ex.ToString()), JinCheng_logFile);
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

            var merchant_code = parameters.ContainsKey("merchant_code") ? parameters["merchant_code"] : string.Empty;   //商户号
            var sign = parameters.ContainsKey("sign") ? parameters["sign"] : string.Empty;                              //签名
            var order_no = parameters.ContainsKey("order_no") ? parameters["order_no"] : string.Empty;                  //商户唯一订单
            var order_amount = parameters.ContainsKey("order_amount") ? parameters["order_amount"] : string.Empty;      //商户订单总金额
            var order_time = parameters.ContainsKey("order_time") ? parameters["order_time"] : string.Empty;            //商户订单时间
            var extras = parameters.ContainsKey("extras") ? parameters["extras"] : string.Empty;                        //回传参数   
            var trade_no = parameters.ContainsKey("trade_no") ? parameters["trade_no"] : string.Empty;                  //支付平台订单号   
            var trade_time = parameters.ContainsKey("trade_time") ? parameters["trade_time"] : string.Empty;            //支付平台订单时间   
            var trade_status = parameters.ContainsKey("trade_status") ? parameters["trade_status"] : string.Empty;      //交易状态   



            var cSign = Md5($"extras={extras}&merchant_code={merchant_code}&order_amount={order_amount}&order_no={order_no}&order_time={order_time}&trade_no={trade_no}&trade_status={trade_status}&trade_time={trade_time}&key={JinCheng_Key}", Encoding.UTF8).ToLower();
            SiteHelper.Log(guid, $"金城支付回调签名 cSign={cSign} sign={sign}", JinCheng_logFile);
            return sign == cSign;




        }
    }
}