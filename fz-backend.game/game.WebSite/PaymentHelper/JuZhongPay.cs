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
    public class JuZhongPay
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
                { "businessOrdid", order.Id.ToString()},
                { "orderName", "Recharge"},
                { "merId", juzhongpay_merId},
                { "terId", juzhongpay_terId},
                { "tradeMoney", (order.Amount*100).ToString()  },
                { "payType",tradeCode },
                { "appSence","1002" }, 
                { "syncURL", juzhongpay_syncURL},
                { "asynURL", juzhongpay_asynURL},
            };

            var sign = Md5($"appSence=1002&asynURL={juzhongpay_asynURL}&businessOrdid={order.Id.ToString()}&merId={juzhongpay_merId}&orderName=Recharge&payType={tradeCode}&syncURL={juzhongpay_syncURL}&terId={juzhongpay_terId}&tradeMoney={(order.Amount * 100).ToString()}&key={juzhongpay_Key}", Encoding.UTF8).ToLower();
            paras.Add("sign", sign);
            var url = $"<html><body><form method='post' name='form' action='{juzhongpay_PayURL}'>" +
                      string.Join("", paras.Select(m => string.Format("<input type='hidden' name='{0}' value='{1}' >", m.Key, m.Value))) +
                      "</form><script>window.onload = function(){document.forms['form'].submit();}</script></body></html>";
            SiteHelper.Log(guid, $"{order.Id} 支付请求 [data]({juzhongpay_PayURL}) {url}", juzhongpay_logFile);
            var api = Service.CreateRedirectData((int)PayGatewayType.JuZhongPay, url);

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
                    SiteHelper.Log(logGuid, string.Format("聚众支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), juzhongpay_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("聚众支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), juzhongpay_logFile);
                }
                Dictionary<string, string> parameters = null;

                if (!CheckSign(logGuid, out parameters))
                {
                    SiteHelper.Log(logGuid, string.Format("聚众支付回调非法签名"), juzhongpay_logFile);
                    return PayState.SignError;
                }

                //支付状态，1003表示支付成功;
                var orderstatus = parameters.ContainsKey("order_state") ? parameters["order_state"] : string.Empty;

                //商家订单号 
                var orderid = parameters.ContainsKey("orderId") ? parameters["orderId"] : string.Empty;
                //第三方订单号 
                var tradeid = parameters.ContainsKey("payOrderId") ? parameters["payOrderId"] : string.Empty;

                //订单金额
                var amount = parameters.ContainsKey("money") ? parameters["money"] : string.Empty;

                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(orderid));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 聚众支付回调 訂單狀態已經完成,無須金幣充值", order.Id), juzhongpay_logFile);
                    return PayState.Success;
                }

                //進行金幣充值
                string msg = "";

                if (orderstatus.Equals("1003") && ((Convert.ToDecimal(amount)/100) == order.Amount))
                {
                    var data = $"聚众支付回调 完成支付: 支付订单号: {orderid} , 渠道订单号: {tradeid}, 状态: {orderstatus}";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, tradeid, data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 聚众支付回调 金幣充值成功 支付订单号:{orderid} 金额:amount={order.Amount} 渠道订单号:{tradeid}", juzhongpay_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 聚众支付回调 金幣充值错误 {msg}", juzhongpay_logFile);

                        Service.CDFailOrder(order.Id, tradeid, $"聚众支付订单号:{orderid} , 渠道订单号: {tradeid} , 支付金额(分): {(Convert.ToInt64(amount) * 100).ToString()}, 状态: {orderstatus}");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, $"{order.Id}  聚众支付回调 支付無效 支付订单号:{orderid}, 渠道订单号:{tradeid}, 支付金额(分):{(Convert.ToInt64(amount) * 100).ToString()}, 状态:{orderstatus}", juzhongpay_logFile);
                Service.CDFailOrder(order.Id, "0", $"聚众支付订单号:{orderid} , 渠道订单号: {tradeid} , 支付金额(分): {(Convert.ToInt64(amount) * 100).ToString()}, 状态: {orderstatus}");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("聚众支付回调 异常错误 {0}", ex.ToString()), juzhongpay_logFile);
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
            var balanceCurrName = parameters.ContainsKey("balanceCurrName") ? parameters["balanceCurrName"] : string.Empty;
            var merId = parameters.ContainsKey("merId") ? parameters["merId"] : string.Empty;
            var orderId = parameters.ContainsKey("orderId") ? parameters["orderId"] : string.Empty;
            var payOrderId = parameters.ContainsKey("payOrderId") ? parameters["payOrderId"] : string.Empty;
            var order_state = parameters.ContainsKey("order_state") ? parameters["order_state"] : string.Empty;
            var money = parameters.ContainsKey("money") ? parameters["money"] : string.Empty;
            var payReturnTime = parameters.ContainsKey("payReturnTime") ? parameters["payReturnTime"] : string.Empty;
            var payCurrName = parameters.ContainsKey("payCurrName") ? parameters["payCurrName"] : string.Empty;
            var payType = parameters.ContainsKey("payType") ? parameters["payType"] : string.Empty;
            var notifyType = parameters.ContainsKey("notifyType") ? parameters["notifyType"] : string.Empty;           
            var sign = parameters.ContainsKey("sign") ? parameters["sign"] : string.Empty;
            var cSign = Md5($"balanceCurrName={balanceCurrName}&merId={merId}&money={money}&notifyType={notifyType}&orderId={orderId}&order_state={order_state}&payCurrName={payCurrName}&payOrderId={payOrderId}&payReturnTime={payReturnTime}&payType={payType}&key={juzhongpay_Key}", Encoding.UTF8).ToLower();
            SiteHelper.Log(guid, $"聚众支付回调签名 cSign={cSign} sign={sign}", juzhongpay_logFile);
            return sign == cSign;
        }

    }
}