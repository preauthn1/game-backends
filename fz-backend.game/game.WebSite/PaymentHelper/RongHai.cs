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
    public class RongHai
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
                { "pay_memberid", ronghai_memberid },
                { "pay_orderid", order.Id.ToString()},
                { "pay_applydate",d},
                { "pay_bankcode", tradeCode},
                { "pay_notifyurl", ronghai_shopNotifyUrl},
                { "pay_callbackurl", ronghai_shopCallBackUrl},
                { "pay_amount", (order.Amount).ToString()  },
                { "pay_productname", "Recharge"},
            };

            var pay_md5sign = Md5($"pay_amount={(order.Amount).ToString()}&pay_applydate={d}&pay_bankcode={tradeCode}&pay_callbackurl={ronghai_shopCallBackUrl}&pay_memberid={ronghai_memberid}&pay_notifyurl={ronghai_shopNotifyUrl}&pay_orderid={ order.Id.ToString()}&key={ronghai_Key}", Encoding.UTF8).ToUpper();
            paras.Add("pay_md5sign", pay_md5sign);

            var url = $"<html><body><form method='post' name='form' action='{ronghai_PayURL}' enctype=\"application/x-www-form-urlencoded\">" +
                      string.Join("", paras.Select(m => string.Format("<input type='hidden' name='{0}' value='{1}' >", m.Key, m.Value))) +
                      "</form><script>window.onload = function(){document.forms['form'].submit();}</script></body></html>";

            SiteHelper.Log(guid, $"{order.Id} 支付请求 [url]({ronghai_PayURL})[data] {url}", ronghai_logFile);

            var api = Service.CreateRedirectData((int)PayGatewayType.RongHai, url);

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
                    SiteHelper.Log(logGuid, string.Format("融海支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), ronghai_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("融海支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), ronghai_logFile);
                }
                Dictionary<string, string> parameters = null;

                if (!CheckSign(logGuid, out parameters))
                {
                    SiteHelper.Log(logGuid, string.Format("融海支付回调非法签名"), ronghai_logFile);
                    return PayState.SignError;
                }

                //支付状态，00表示支付成功;
                var returncode = parameters.ContainsKey("returncode") ? parameters["returncode"] : string.Empty;

                //商家订单号 
                var orderid = parameters.ContainsKey("orderid") ? parameters["orderid"] : string.Empty;

                //订单金额
                var amount = parameters.ContainsKey("amount") ? parameters["amount"] : string.Empty;

                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(orderid));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 融海支付回调 訂單狀態已經完成,無須金幣充值", order.Id), ronghai_logFile);
                    return PayState.Success;
                }

                //進行金幣充值
                string msg = "";

                if (returncode.Equals("00") && ((Convert.ToDecimal(amount)) == order.Amount))
                {
                    var data = $"融海支付回调 完成支付: 支付订单号: {orderid} , 渠道订单号: {"0"}, 状态: {returncode}";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, "0", data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 融海支付回调 金幣充值成功 支付订单号:{orderid} 金额:amount={order.Amount} 渠道订单号:{"0"}", ronghai_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 融海支付回调 金幣充值错误 {msg}", ronghai_logFile);

                        Service.CDFailOrder(order.Id, "0", $"融海支付订单号:{orderid} , 渠道订单号: {"0"} , 支付金额(分): {(Convert.ToInt64(amount) * 100).ToString()}, 状态: {returncode}");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, $"{order.Id}  融海支付回调 支付無效 支付订单号:{orderid}, 渠道订单号:{"0"}, 支付金额(分):{(Convert.ToInt64(amount) * 100).ToString()}, 状态:{returncode}", ronghai_logFile);
                Service.CDFailOrder(order.Id, "0", $"融海支付订单号:{orderid} , 渠道订单号: {"0"} , 支付金额(分): {(Convert.ToInt64(amount) * 100).ToString()}, 状态: {returncode}");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("融海支付回调 异常错误 {0}", ex.ToString()), ronghai_logFile);
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
            var memberid = parameters.ContainsKey("memberid") ? parameters["memberid"] : string.Empty;
            var amount = parameters.ContainsKey("amount") ? parameters["amount"] : string.Empty;
            var orderid = parameters.ContainsKey("orderid") ? parameters["orderid"] : string.Empty; //商户订单号
            var transaction_id = parameters.ContainsKey("transaction_id") ? parameters["transaction_id"] : string.Empty; //平台交易时间         
            var returncode = parameters.ContainsKey("returncode") ? parameters["returncode"] : string.Empty; //平台交易结果 
            var datetime = parameters.ContainsKey("datetime") ? parameters["datetime"] : string.Empty; //平台交易时间         
            var attach = parameters.ContainsKey("attach") ? parameters["attach"] : string.Empty; //商户附加数据返回       



            var cSign = Md5($"amount={amount}&datetime={datetime}&memberid={memberid}&orderid={orderid}&returncode={returncode}&transaction_id={transaction_id}&key={ronghai_Key}", Encoding.UTF8).ToUpper();
            SiteHelper.Log(guid, $"融海支付回调签名 cSign={cSign} sign={sign}", ronghai_logFile);
            return sign == cSign;
        }
    }
}