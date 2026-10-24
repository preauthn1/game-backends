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
    public class KuaiFu
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
                { "partner", kuaifu_partner },
                { "user_seller", kuaifu_seller },
                { "out_order_no", order.Id.ToString()},
                { "subject","Recharge"},
                { "total_fee", (order.Amount).ToString()  },
                { "pay_type", tradeCode},
                { "notify_url", kuaifu_shopNotifyUrl},
                { "return_url", kuaifu_shopCallBackUrl},
                { "http_referer", "example.invalid"},
            };

            var pay_md5sign = Md5($"notify_url={kuaifu_shopNotifyUrl}&out_order_no={order.Id.ToString()}&partner={kuaifu_partner}&return_url={kuaifu_shopCallBackUrl}&subject=Recharge&total_fee={(order.Amount).ToString()}&user_seller={kuaifu_seller}{kuaifu_Key}", Encoding.UTF8).ToLower();
            paras.Add("sign", pay_md5sign);

            var url = $"<html><body><form method='post' name='form' action='{kuaifu_PayURL}' enctype=\"application/x-www-form-urlencoded\">" +
                      string.Join("", paras.Select(m => string.Format("<input type='hidden' name='{0}' value='{1}' >", m.Key, m.Value))) +
                      "</form><script>window.onload = function(){document.forms['form'].submit();}</script></body></html>";

            SiteHelper.Log(guid, $"{order.Id} 支付请求 [url]({kuaifu_PayURL})[data] {url}", kuaifu_logFile);

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
                    SiteHelper.Log(logGuid, string.Format("快付支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), kuaifu_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("快付支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), kuaifu_logFile);
                }
                Dictionary<string, string> parameters = null;

                if (!CheckSign(logGuid, out parameters))
                {
                    SiteHelper.Log(logGuid, string.Format("快付支付回调非法签名"), kuaifu_logFile);
                    return PayState.SignError;
                }

              

                //支付状态，00表示支付成功;
                var returncode = parameters.ContainsKey("trade_status") ? parameters["trade_status"] : string.Empty;

                //商家订单号 
                var orderid = parameters.ContainsKey("out_order_no") ? parameters["out_order_no"] : string.Empty;

                //订单金额
                var amount = parameters.ContainsKey("total_fee") ? parameters["total_fee"] : string.Empty;

                //第三方交易ID
                var trade_no = parameters.ContainsKey("trade_no") ? parameters["trade_no"] : string.Empty;

                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(orderid));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 快付支付回调 訂單狀態已經完成,無須金幣充值", order.Id), kuaifu_logFile);
                    return PayState.Success;
                }

                //進行金幣充值
                string msg = "";

                if (returncode.Equals("TRADE_SUCCESS") && ((Convert.ToDecimal(amount)) == order.Amount))
                {
                    var data = $"快付支付回调 完成支付: 支付订单号: {orderid} , 渠道订单号: {trade_no}, 状态: {returncode}";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, trade_no, data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 快付支付回调 金幣充值成功 支付订单号:{orderid} 金额:amount={order.Amount} 渠道订单号:{trade_no}", kuaifu_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 快付支付回调 金幣充值错误 {msg}", kuaifu_logFile);

                        Service.CDFailOrder(order.Id, trade_no, $"快付支付订单号:{orderid} , 渠道订单号: {"0"} , 支付金额(分): {(Convert.ToInt64(amount) * 100).ToString()}, 状态: {returncode}");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, $"{order.Id}  快付支付回调 支付無效 支付订单号:{orderid}, 渠道订单号:{trade_no}, 支付金额(分):{(Convert.ToInt64(amount) * 100).ToString()}, 状态:{returncode}", kuaifu_logFile);
                Service.CDFailOrder(order.Id, trade_no, $"快付支付订单号:{orderid} , 渠道订单号: {"0"} , 支付金额(分): {(Convert.ToInt64(amount) * 100).ToString()}, 状态: {returncode}");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("快付支付回调 异常错误 {0}", ex.ToString()), kuaifu_logFile);
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

            var total_fee = parameters.ContainsKey("total_fee") ? parameters["total_fee"] : string.Empty;
            var out_order_no = parameters.ContainsKey("out_order_no") ? parameters["out_order_no"] : string.Empty;
            var sign = parameters.ContainsKey("sign") ? parameters["sign"] : string.Empty;
            var trade_no = parameters.ContainsKey("trade_no") ? parameters["trade_no"] : string.Empty;
            var trade_status = parameters.ContainsKey("trade_status") ? parameters["trade_status"] : string.Empty;      
            

            var cSign = Md5($"{out_order_no}{total_fee}{trade_status}{kuaifu_partner}{kuaifu_Key}", Encoding.UTF8).ToLower();
            SiteHelper.Log(guid, $"快付支付回调签名 cSign={cSign} sign={sign}", kuaifu_logFile);
            return sign == cSign;
           



        }
    }
}