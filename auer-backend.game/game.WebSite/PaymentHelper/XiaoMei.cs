using game.Entity.Pay;
using game.Infrastructure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using example.invalid;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using static game.WebSite.PayConfig;

namespace game.WebSite.PaymentHelper
{
    public class XiaoMei
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
                { "inputCharset", "utf-8"  },
                { "notifyUrl", xiaomei_shopNotifyUrl  },
                { "payType", tradeCode  },
                { "merchantId", xiaomei_memberid },
                { "orderId", order.Id.ToString()},
                { "transAmt", order.Amount.ToString()},
                { "orderTime", d},
            };
            var sign = Md5($"inputCharset=utf-8&merchantId={xiaomei_memberid}&notifyUrl={xiaomei_shopNotifyUrl}&orderId={order.Id.ToString()}&orderTime={d}&payType={tradeCode}&transAmt={order.Amount.ToString()}&key={xiaomei_Key}", Encoding.UTF8).ToLower();
            paras.Add("sign", sign);          

            var url = $"<html><body><form method='post' name='form' action='{xiaomei_PayURL}' enctype=\"application/x-www-form-urlencoded\">" +
                    string.Join("", paras.Select(m => string.Format("<input type='hidden' name='{0}' value='{1}' >", m.Key, m.Value))) +
                    "</form><script>window.onload = function(){document.forms['form'].submit();}</script></body></html>";

            SiteHelper.Log(guid, $"{order.Id} 支付请求 [data]({url}) ", xiaomei_logFile);

            var api = Service.CreateRedirectData((int)PayGatewayType.XiaoMei, url);

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
                    SiteHelper.Log(logGuid, string.Format("支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), xiaomei_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), xiaomei_logFile);
                }

                 Dictionary<string, string> parameters = null;

                if (!CheckSign(logGuid, out parameters))
                {
                    SiteHelper.Log(logGuid, string.Format("小美支付回调非法签名"), xiaomei_logFile);
                    return PayState.SignError;
                }


                //支付状态，00表示支付成功;
                var returncode = parameters.ContainsKey("status") ? parameters["status"] : string.Empty;

                //商家订单号 
                var orderid = parameters.ContainsKey("transId") ? parameters["transId"] : string.Empty;

                //订单金额
                var amount = parameters.ContainsKey("payAmount") ? parameters["payAmount"] : string.Empty;

                var trxNo = parameters.ContainsKey("orderId") ? parameters["orderId"] : string.Empty; ;

                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(orderid));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 小美支付回调 訂單狀態已經完成,無須金幣充值", order.Id), xiaomei_logFile);
                    return PayState.Success;
                }

                //進行金幣充值
                string msg = "";

                if (returncode.Equals("1") && ((Convert.ToDecimal(amount)) == order.Amount))
                {
                    var data = $"小美支付回调 完成支付: 支付订单号: {orderid} , 渠道订单号: {trxNo}, 状态: {returncode}";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, trxNo, data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 小美支付回调 金幣充值成功 支付订单号:{orderid} 金额:{order.Amount} 渠道订单号:{trxNo}", xiaomei_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 小美支付回调 金幣充值错误 {msg}", xiaomei_logFile);

                        Service.CDFailOrder(order.Id, trxNo, $"小美支付订单号:{orderid} , 渠道订单号: {trxNo} , 支付金额: {amount}, 状态: {returncode}");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, $"{order.Id}  小美支付回调 支付無效 支付订单号:{orderid}, 渠道订单号:{trxNo}, 支付金额:{amount}, 状态:{returncode}", xiaomei_logFile);
                Service.CDFailOrder(order.Id, trxNo, $"小美支付订单号:{orderid} , 渠道订单号: {trxNo} , 支付金额: {amount}, 状态: {returncode}");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("小美支付回调 异常错误 {0}", ex.ToString()), xiaomei_logFile);
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
                parameters = HttpContext.Current.Request.QueryString.AllKeys.Where(x => !string.IsNullOrEmpty(x)).Cast<string>()
                    .ToDictionary(k => k, v => HttpContext.Current.Request[v]).Where(x => x.Key != null).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            }
            else
            {
                parameters = HttpContext.Current.Request.Form.AllKeys.Cast<string>()
                    .ToDictionary(k => k, v => HttpContext.Current.Request[v]).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            }

            var sign = parameters.ContainsKey("sign") ? parameters["sign"] : string.Empty;
            parameters.Remove("sign");

            var csigndata = string.Join("&", parameters.Where(x => !string.IsNullOrEmpty(x.Value) && x.Value != "null").OrderBy(x=>x.Key).Select((m => $"{m.Key}={m.Value}")));
            SiteHelper.Log(guid, $"小美支付签名LOG DATA={csigndata}", xiaomei_logFile);
            var cSign = Md5(csigndata + $"&key={xiaomei_Key}", Encoding.UTF8).ToLower();

            SiteHelper.Log(guid, $"小美支付回调签名 cSign={cSign} sign={sign}", xiaomei_logFile);
            return sign == cSign;
        }
    }
}