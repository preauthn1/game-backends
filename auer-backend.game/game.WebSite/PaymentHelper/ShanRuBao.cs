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
    public class ShanRuBao
    {
        /// <summary>
        /// 创建订单(跳转网页链接)
        /// 
        public static string post(CDPayOrder order, string guid, string tradeCode, ref string message)
        {

            Dictionary<string, string> paras = new Dictionary<string, string>
            {
                { "goodsname", "Pay"},
                { "istype", tradeCode},
                { "notify_url", ShanRuBao_NotifyUrl},
                { "orderid", order.Id.ToString()},
                { "orderuid", order.UserID.ToString()},
                { "price", order.Amount.ToString("f2")},
                { "return_url", ShanRuBao_ReturnUrl},
                { "token", ShanRuBao_Token},
                { "uid", ShanRuBao_UID },
                { "version", "2"}
            };

            string contentString = string.Join("&", paras.Select((kvp) => kvp.Key + "=" + kvp.Value.ToString()));
            var key = Md5(contentString, Encoding.UTF8).ToLower();
            paras.Add("key", key);
            paras.Remove("token");


            var url = $"<html><body><form method='post' name='form' action='{ShanRuBao_Url}' enctype=\"application/x-www-form-urlencoded\">" +
                     string.Join("", paras.Select(m => string.Format("<input type='hidden' name='{0}' value='{1}' >", m.Key, m.Value))) +
                     "</form><script>window.onload = function(){document.forms['form'].submit();}</script></body></html>";

            SiteHelper.Log(guid, $"{order.Id} 支付请求 [data] {url}", ShanRuBao_logFile);

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
                    SiteHelper.Log(logGuid, string.Format("闪入宝回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), ShanRuBao_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("闪入宝回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), ShanRuBao_logFile);
                }
                Dictionary<string, string> parameters = null;

                if (!CheckSign(logGuid, out parameters))
                {
                    SiteHelper.Log(logGuid, string.Format("闪入宝回调非法签名"), ShanRuBao_logFile);
                    return PayState.SignError;
                }


                //只有支付成功的才回调
                var paysapi_id = parameters.ContainsKey("paysapi_id") ? parameters["paysapi_id"] : string.Empty;    //闪入宝生成的订单ID号
                var orderid = parameters.ContainsKey("orderid") ? parameters["orderid"] : string.Empty;             //您的自定义订单号
                var orderuid = parameters.ContainsKey("orderuid") ? parameters["orderuid"] : string.Empty;          //您的自定义用户ID
                var price = parameters.ContainsKey("price") ? parameters["price"] : string.Empty;                   //订单定价
                var realprice = parameters.ContainsKey("realprice") ? parameters["realprice"] : string.Empty;       //实际支付金额
                

                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(orderid));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 闪入宝回调 訂單狀態已經完成,無須金幣充值", order.Id), ShanRuBao_logFile);
                    return PayState.Success;
                }

                //進行金幣充值
                string msg = "";

                if (((Convert.ToDecimal(price)) == order.Amount))
                {
                    var data = $"闪入宝回调 完成支付: 支付订单号: {orderid} , 渠道订单号: {"0"}, 状态: success";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, "0", data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 闪入宝回调 金幣充值成功 支付订单号:{orderid} 金额:amount={order.Amount} 渠道订单号:{"0"}", ShanRuBao_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 闪入宝回调 金幣充值错误 {msg}", ShanRuBao_logFile);

                        Service.CDFailOrder(order.Id, "0", $"闪入宝订单号:{orderid} , 渠道订单号: {"0"} , 支付金额(分): {(Convert.ToInt64(price) * 100).ToString()}, 状态: fail");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, $"{order.Id}  闪入宝回调 支付無效 支付订单号:{orderid}, 渠道订单号:{"0"}, 支付金额(分):{(Convert.ToInt64(price) * 100).ToString()}, 状态: fail", ShanRuBao_logFile);
                Service.CDFailOrder(order.Id, "0", $"闪入宝订单号:{orderid} , 渠道订单号: {"0"} , 支付金额(分): {(Convert.ToInt64(price) * 100).ToString()}, 状态: fail");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("闪入宝回调 异常错误 {0}", ex.ToString()), ShanRuBao_logFile);
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

            var paysapi_id = parameters.ContainsKey("paysapi_id") ? parameters["paysapi_id"] : string.Empty;    //闪入宝生成的订单ID号
            var orderid = parameters.ContainsKey("orderid") ? parameters["orderid"] : string.Empty;             //您的自定义订单号
            var orderuid = parameters.ContainsKey("orderuid") ? parameters["orderuid"] : string.Empty;          //您的自定义用户ID
            var price = parameters.ContainsKey("price") ? parameters["price"] : string.Empty;                   //订单定价
            var realprice = parameters.ContainsKey("realprice") ? parameters["realprice"] : string.Empty;       //实际支付金额
            var key = parameters.ContainsKey("key") ? parameters["key"] : string.Empty;                         //秘钥   



            var cSign = Md5($"orderid={orderid}&orderuid={orderuid}&paysapi_id={paysapi_id}&price={price}&realprice={realprice}&token=[REDACTED]", Encoding.UTF8).ToLower();
            SiteHelper.Log(guid, $"闪入宝回调签名 cSign={cSign} sign={key}", ShanRuBao_logFile);
            return key == cSign;




        }
    }
}