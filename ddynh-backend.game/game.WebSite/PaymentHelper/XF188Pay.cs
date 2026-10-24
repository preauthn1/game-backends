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
    public class XF188Pay
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
                { "app_id", xf188_mchid },
                { "order_no", order.Id.ToString()},
                { "pkid", xf188_Pkid},
                { "channel", tradeCode},
                { "data_type", "text"},
                { "amount", (order.Amount*100).ToString()  },
                { "version", "1.0"},
                { "notify_url", xf188_shopNotifyUrl},
                { "return_url", xf188_shopCallbackUrl},
            };

            var pay_md5sign = Md5($"version=1.0&app_id={xf188_mchid}&amount={ (order.Amount * 100).ToString()}&order_no={order.Id.ToString()}&notify_url={xf188_shopNotifyUrl}&return_url={xf188_shopCallbackUrl}&{xf188_Key}", Encoding.UTF8).ToLower();
            paras.Add("sign", pay_md5sign);
            
            var url = $"<html><body><form method='post' name='form' action='{xf188_PayURL}'>" +
                      string.Join("", paras.Select(m => string.Format("<input type='hidden' name='{0}' value='{1}' >", m.Key, m.Value))) +
                      "</form><script>window.onload = function(){document.forms['form'].submit();}</script></body></html>";
            SiteHelper.Log(guid, $"{order.Id} 支付请求 [data] {url}", xf188_logFile);

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
                    SiteHelper.Log(logGuid, string.Format("财付支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), xf188_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("财付支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), xf188_logFile);
                }
                Dictionary<string, string> parameters = null;

                if (!CheckSign(logGuid, out parameters))
                {
                    SiteHelper.Log(logGuid, string.Format("财付支付回调非法签名"), xf188_logFile);
                    return PayState.SignError;
                }

                //支付状态，0表示支付成功;
                var status = parameters.ContainsKey("status") ? parameters["status"] : string.Empty;

                //商家订单号 
                var out_trade_no = parameters.ContainsKey("out_trade_no") ? parameters["out_trade_no"] : string.Empty;

                //支付平台流水号，同trade_no 一样
                var trade_no = parameters.ContainsKey("trade_no") ? parameters["trade_no"] : string.Empty;

                //订单金额
                var amount = parameters.ContainsKey("amount") ? parameters["amount"] : string.Empty;

                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(out_trade_no));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 财付支付回调 訂單狀態已經完成,無須金幣充值", order.Id), xf188_logFile);
                    return PayState.Success;
                }

                //進行金幣充值
                string msg = "";

                if (status.Equals("success") && ((Convert.ToInt64(amount)/100) == order.Amount))
                {
                    var data = $"财付支付回调 完成支付: 支付订单号: {out_trade_no} , 渠道订单号: {trade_no}, 状态: {status}";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, trade_no, data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 财付支付回调 金幣充值成功 支付订单号:{out_trade_no} 金额:amount={order.Amount} 渠道订单号:{trade_no}", xf188_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 财付支付回调 金幣充值错误 {msg}", xf188_logFile);

                        Service.CDFailOrder(order.Id, trade_no, $"财付支付订单号:{out_trade_no} , 渠道订单号: {trade_no} , 支付金额(分): {amount}, 状态: {status}");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, $"{order.Id}  财付支付回调 支付無效 支付订单号:{out_trade_no}, 渠道订单号:{trade_no}, 支付金额(分):{amount}, 状态:{status}", xf188_logFile);
                Service.CDFailOrder(order.Id, trade_no, $"财付支付订单号:{out_trade_no} , 渠道订单号: {trade_no} , 支付金额(分): {amount}, 状态: {status}");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("财付支付回调 异常错误 {0}", ex.ToString()), xf188_logFile);
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
            var amount = parameters.ContainsKey("amount") ? parameters["amount"] : string.Empty;
            var out_trade_no = parameters.ContainsKey("out_trade_no") ? parameters["out_trade_no"] : string.Empty; //商户订单号
            var trade_no = parameters.ContainsKey("trade_no") ? parameters["trade_no"] : string.Empty; //平台交易订单号            
            var cSign = Md5($"version=1.0&app_id={xf188_mchid}&amount={ amount.ToString()}&order_no={out_trade_no.ToString()}&{xf188_Key}", Encoding.UTF8).ToLower();

            SiteHelper.Log(guid, $"财付支付回调签名 cSign={cSign} sign={sign}", xf188_logFile);
            return sign == cSign;
        }
    }
}