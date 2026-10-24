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
    public class StarPay
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
            string url = string.Empty;
            string d = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Dictionary<string, string> paras = new Dictionary<string, string>
            {
                { "appId", starpay_partnerid },
                { "tradeOrderId", order.Id.ToString("D20")},
                { "amount", (order.Amount*100).ToString()  },
                { "channelId", tradeCode},
                { "notifyUrl", starpay_shopNotifyUrl},               
            };
            var signdata = string.Join("&", paras.OrderBy(x => x.Key).Select(m => string.Format("{0}={1}", m.Key, m.Value))) + "&key=" + starpay_Key;
            SiteHelper.Log(guid, $"加密资料 [data] {signdata}", starpay_logFile);
            var sign = Md5(signdata, Encoding.UTF8).ToLower();
            paras.Add("sign", sign);
            var data = string.Join("&", paras.Select(m => string.Format("{0}={1}", m.Key, m.Value)));
            SiteHelper.Log(guid, $"{order.Id} 支付请求 [data]({starpay_PayURL}) {data}", starpay_logFile);
            var responseString = SiteHelper.HttpPost(starpay_PayURL, data);
            SiteHelper.Log(guid, $"{order.Id} 支付请求回覆 [data] {responseString}", starpay_logFile);

            JObject json = JObject.Parse(responseString);
            var retCode = json["code"] != null ? json["code"].ToString() : string.Empty;

            if (!retCode.Equals("200"))
            {
                var respmsg = json["msg"] != null ? json["msg"].ToString() : string.Empty;
                var message = string.Format("{0} 支付请求失敗 {1}", order.Id, respmsg);

                SiteHelper.Log(guid, message, starpay_logFile);
            }
            else
            {              
                url = json["data"] != null ? json["data"].ToString() : string.Empty;
            }

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
                    SiteHelper.Log(logGuid, string.Format("星支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), starpay_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("星支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), starpay_logFile);
                }
                Dictionary<string, string> parameters = null;

                if (!CheckSign(logGuid, out parameters))
                {
                    SiteHelper.Log(logGuid, string.Format("星支付回调非法签名"), starpay_logFile);
                    return PayState.SignError;
                }

                //支付状态，00表示支付成功;
                var returncode = parameters.ContainsKey("success") ? parameters["success"].ToLower() : string.Empty;

                //商家订单号 
                var orderid = parameters.ContainsKey("tradeOrderId") ? parameters["tradeOrderId"] : string.Empty;

                //订单金额
                var amount = parameters.ContainsKey("amount") ? parameters["amount"] : string.Empty;

                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(orderid));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 星支付回调 訂單狀態已經完成,無須金幣充值", order.Id), starpay_logFile);
                    return PayState.Success;
                }

                //進行金幣充值
                string msg = "";

                if (returncode.Equals("true") && ((Convert.ToDecimal(amount)/100) == order.Amount))
                {
                    var data = $"星支付回调 完成支付: 支付订单号: {orderid} , 渠道订单号: {"0"}, 状态: {returncode}";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, "0", data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 星支付回调 金幣充值成功 支付订单号:{orderid} 金额:amount={order.Amount} 渠道订单号:{"0"}", starpay_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 星支付回调 金幣充值错误 {msg}", starpay_logFile);
                        Service.CDFailOrder(order.Id, "0", $"星支付订单号:{orderid} , 渠道订单号: {"0"} , 支付金额(分): {(Convert.ToDecimal(amount)).ToString()}, 状态: {returncode}");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, $"{order.Id}  星支付回调 支付無效 支付订单号:{orderid}, 渠道订单号:{"0"}, 支付金额(分):{(Convert.ToDecimal(amount)).ToString()}, 状态:{returncode}", starpay_logFile);
                Service.CDFailOrder(order.Id, "0", $"星支付订单号:{orderid} , 渠道订单号: {"0"} , 支付金额(分): {(Convert.ToDecimal(amount)).ToString()}, 状态: {returncode}");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("星支付回调 异常错误 {0}", ex.ToString()), starpay_logFile);
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
                    .ToDictionary(k => k, v => HttpContext.Current.Request[v]).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            }
            else
            {
                parameters = HttpContext.Current.Request.Form.AllKeys.Where(x => !string.IsNullOrEmpty(x)).Cast<string>()
                    .ToDictionary(k => k, v => HttpContext.Current.Request[v]).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            }


            var sign = parameters.ContainsKey("sign") ? parameters["sign"] : string.Empty;
            parameters.Remove("sign");

            var signdata = string.Join("&", parameters.Select(m => string.Format("{0}={1}", m.Key, m.Value))) + "&key=" + starpay_Key;

            SiteHelper.Log(guid, $"回传加密资料 [data] {signdata}", starpay_logFile);


            var cSign = Md5(signdata, Encoding.UTF8).ToLower();
            SiteHelper.Log(guid, $"星支付回调签名 cSign={cSign} sign={sign}", starpay_logFile);
            return sign == cSign;
        }
    }
}