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
    public class EightZpay
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
            Dictionary<string, string> paras = new Dictionary<string, string>
            {
                { "merchant_id", eightzpay_merchantid  },
                { "payment_way", tradeCode},
                { "order_amount", order.Amount.ToString()},
                { "source_order_id", order.Id.ToString()},
                { "goods_name", "3C"},
                { "bank_code", "ICBC"},
                { "client_ip", order.IPAddress},
                { "notify_url", eightzpay_shopNotifyUrl},
                { "return_url", eightzpay_shopCallbackUrl},

            };

            var sign = Md5($"bank_code=ICBC&client_ip={order.IPAddress}&goods_name=3C&merchant_id={eightzpay_merchantid}&notify_url={eightzpay_shopNotifyUrl}&order_amount={order.Amount.ToString()}&payment_way={tradeCode}&return_url={eightzpay_shopCallbackUrl}&source_order_id={order.Id.ToString()}&token=[REDACTED]", Encoding.UTF8).ToLower();
            paras.Add("sign", sign);
            var data = string.Join("&", paras.Select(m => string.Format("{0}={1}", m.Key, m.Value)));


            SiteHelper.Log(guid, string.Format("{0} 支付请求 [data]({1}) {2}", order.Id, eightzpay_PayURL, data), eightzpay_logFile);

            //呼叫支付平台
            var responseString = SiteHelper.HttpPost(eightzpay_PayURL, data);
            SiteHelper.Log(guid, string.Format("{0} 支付请求返回 [data]({1})", order.Id,  responseString), eightzpay_logFile);

            return responseString;
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
                    SiteHelper.Log(logGuid, string.Format("久付支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), eightzpay_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("久付支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), eightzpay_logFile);
                }
                Dictionary<string, string> parameters = null;

                if (!CheckSign(logGuid, out parameters))
                {
                    SiteHelper.Log(logGuid, string.Format("久付支付回调非法签名"), eightzpay_logFile);
                    return PayState.SignError;
                }

                //支付状态，pay_success表示支付成功;
                var orderstatus = parameters.ContainsKey("status") ? parameters["status"] : string.Empty;

                //商家订单号 
                var orderid = parameters.ContainsKey("source_order_id") ? parameters["source_order_id"] : string.Empty;

                //订单金额
                var amount = parameters.ContainsKey("order_amount") ? parameters["order_amount"] : string.Empty;

                //第三方订单号
                var TradeId = parameters.ContainsKey("order_code") ? parameters["order_code"] : string.Empty;

                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(orderid));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 久付支付回调 訂單狀態已經完成,無須金幣充值", order.Id), eightzpay_logFile);
                    return PayState.Success;
                }

                //進行金幣充值
                string msg = "";

                if (orderstatus.Equals("1") && ((Convert.ToDecimal(amount)) == order.Amount))
                {
                    var data = $"久付支付回调 完成支付: 支付订单号: {orderid} , 渠道订单号: {TradeId}, 状态: {orderstatus}";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, TradeId, data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 久付支付回调 金幣充值成功 支付订单号:{orderid} 金额:amount={order.Amount} 渠道订单号:{TradeId}", eightzpay_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 久付支付回调 金幣充值错误 {msg}", eightzpay_logFile);

                        Service.CDFailOrder(order.Id, "0", $"久付支付订单号:{orderid} , 渠道订单号: {TradeId} , 支付金额(分): {(Convert.ToInt64(amount) * 100).ToString()}, 状态: {orderstatus}");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, $"{order.Id}  久付支付回调 支付無效 支付订单号:{orderid}, 渠道订单号:{TradeId}, 支付金额(分):{(Convert.ToInt64(amount) * 100).ToString()}, 状态:{orderstatus}", eightzpay_logFile);
                Service.CDFailOrder(order.Id, "0", $"久付支付订单号:{orderid} , 渠道订单号: {TradeId} , 支付金额(分): {(Convert.ToInt64(amount) * 100).ToString()}, 状态: {orderstatus}");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("久付支付回调 异常错误 {0}", ex.ToString()), eightzpay_logFile);
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

            var merchant_id = parameters.ContainsKey("merchant_id") ? parameters["merchant_id"] : string.Empty;
            var source_order_id = parameters.ContainsKey("source_order_id") ? parameters["source_order_id"] : string.Empty;
            var order_amount = parameters.ContainsKey("order_amount") ? parameters["order_amount"] : string.Empty;
            var order_code = parameters.ContainsKey("order_code") ? parameters["order_code"] : string.Empty;
            var goods_name = parameters.ContainsKey("goods_name") ? parameters["goods_name"] : string.Empty;
            var payTime = parameters.ContainsKey("payTime") ? parameters["payTime"] : string.Empty;
            var status = parameters.ContainsKey("status") ? parameters["status"] : string.Empty;
            var sign = parameters.ContainsKey("sign") ? parameters["sign"] : string.Empty;

            var cSign = Md5($"goods_name={goods_name}&merchant_id={merchant_id}&order_amount={order_amount}&order_code={order_code}&payTime={payTime}&source_order_id={source_order_id}&status={status}&token=[REDACTED]", Encoding.UTF8).ToLower();
            SiteHelper.Log(guid, $"久付支付回调签名 cSign={cSign} sign={sign}", eightzpay_logFile);
            return sign == cSign;
        }
    }
}