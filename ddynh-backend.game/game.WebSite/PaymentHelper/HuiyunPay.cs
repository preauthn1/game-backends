using System.Security.Cryptography;
using System.Text;
using System.Web;
using game.Entity.Pay;
using game.Infrastructure;
using static game.WebSite.PayConfig;
using Newtonsoft.Json;
using System.Threading.Tasks;
using example.invalid.Http.Headers;
using example.invalid.Http;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System;

namespace game.WebSite.PaymentHelper
{
    public class HuiyunPay
    {
        public static string post(CDPayOrder order, string guid, string tradeCode)
        {
            string url = string.Empty;
           
            Dictionary<string, string> paras = new Dictionary<string, string>
            {
                { "version", "1.0" },
                { "customerid", huiyun_customerid },
                { "sdorderno", order.Id.ToString() },
                { "total_fee", (order.Amount).ToString("f2") },
                { "notifyurl", huiyun_shopNotifyUrl },
                { "returnurl", huiyun_shopCallbackUrl },
                { "paytype", tradeCode },
                { "get_code", "1" },

            };

            var pay_md5sign = Md5($"version=1.0&customerid={huiyun_customerid}&total_fee={order.Amount.ToString("f2")}&sdorderno={order.Id.ToString()}&notifyurl={huiyun_shopNotifyUrl }&returnurl={huiyun_shopCallbackUrl}&{huiyun_Key}", Encoding.UTF8).ToLower();
            paras.Add("sign", pay_md5sign);
            var data = string.Join("&", paras.Select(m => string.Format("{0}={1}", m.Key, m.Value)));


            SiteHelper.Log(guid, string.Format("{0} 支付请求 [data]({1}) {2}", order.Id, huiyun_PayURL, data), huiyun_logFile);

            //呼叫支付平台
            var responseString = SiteHelper.HttpPost(huiyun_PayURL, data);
           

            return responseString;
        }

        public static PayState call(string logGuid)
        {
            try
            {
                var requestType = HttpContext.Current.Request.RequestType;
                if (requestType == "GET")
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), huiyun_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), huiyun_logFile);
                }

                Dictionary<string, string> parameters = null;

                if (!CheckSign(logGuid, out parameters))
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调非法签名"), huiyun_logFile);
                    return PayState.SignError;
                }

                var payOrderId = parameters.ContainsKey("sdorderno") ? parameters["sdorderno"] : string.Empty;
                var amount = parameters.ContainsKey("total_fee") ? parameters["total_fee"] : string.Empty;
                var status = parameters.ContainsKey("status") ? parameters["status"] : string.Empty;
                var channelOrderNo = parameters.ContainsKey("sdpayno") ? parameters["sdpayno"] : string.Empty;


                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(payOrderId));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 支付回调 訂單狀態已經完成,無須金幣充值", order.Id), huiyun_logFile);
                    return PayState.Success;
                }



                //進行金幣充值
                string msg = "";

                if ((status == "1") && (Convert.ToDecimal(amount) == order.Amount))
                {
                    var data = $"支付回调 完成支付: 支付订单号: {payOrderId} , 渠道订单号: {channelOrderNo}, 状态: {status}";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, payOrderId, data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, string.Format("{0} 支付回调 金幣充值成功 支付订单号:{1} 金额:amount={2} 渠道订单号:{3}", order.Id, payOrderId, order.Amount, channelOrderNo), huiyun_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, string.Format("{0} 支付回调 金幣充值错误 {1}", order.Id, msg), huiyun_logFile);
                        Service.CDFailOrder(order.Id, payOrderId, $"支付订单号:{payOrderId} , 渠道订单号: {channelOrderNo} , 支付金额(分): {amount}, 状态: {status}");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, string.Format("{0}  支付回调 支付無效 支付订单号:{1}, 渠道订单号:{2}, 支付金额(分):{3}, 状态:{4}", order.Id, payOrderId, channelOrderNo, amount, status), huiyun_logFile);
                Service.CDFailOrder(order.Id, payOrderId, $"支付订单号:{payOrderId} , 渠道订单号: {channelOrderNo} , 支付金额(分): {amount}, 状态: {status}");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("支付回调 异常错误 {0}", ex.ToString()), huiyun_logFile);
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

         


            var customerid = parameters["customerid"];
            var sdorderno = parameters["sdorderno"];
            var total_fee = parameters["total_fee"];
            var sdpayno = parameters["sdpayno"];
            var status = parameters["status"];
            var paytype = parameters["paytype"];
            var sign = parameters["sign"];


            var cSign = Md5($"customerid={customerid}&status={status}&sdpayno={sdpayno}&sdorderno={sdorderno}&total_fee={total_fee}&paytype={paytype}&{huiyun_Key}", Encoding.UTF8).ToLower();
            SiteHelper.Log(guid, string.Format("支付回调签名 cSign={0} sign={1}", cSign, sign), huiyun_logFile);

            return sign == cSign;
        }
    }
}