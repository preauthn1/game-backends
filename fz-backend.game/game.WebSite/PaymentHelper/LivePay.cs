using game.Entity.Pay;
using game.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using static game.WebSite.PayConfig;
using Newtonsoft.Json;

namespace game.WebSite.PaymentHelper
{
    public class LivePay
    {
        
        public class LivePayResultData
        {
            [JsonProperty("qrcodeUrl")]
            public string qrcodeUrl { get; set; }
            
        }

        public class LivePayResult
        {
            [JsonProperty("status")]
            public string status { get; set; }

            [JsonProperty("msg")]
            public string msg { get; set; }                        

            [JsonProperty("data")]
            public LivePayResultData data { get; set; }
        }
        


        /// <summary>
        /// 创建订单(跳转网页链接)
        /// </summary>
        /// <param name="order"></param>
        /// <param name="guid"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string post(CDPayOrder order, string guid, string tradeCode, ref string message)
        {
            var orderTimeStamp = (Int32)(order.CreateTime.Subtract(new DateTime(1970, 1, 1, 8, 0, 0))).TotalSeconds;  //使用北京時區

            Dictionary<string, string> paras = new Dictionary<string, string>
            {
                { "amount", order.Amount.ToString("f2") },
                { "applydate", orderTimeStamp.ToString()},
                { "bankcode", tradeCode },
                { "callbackurl", LivePay_CallbackUrl},
                { "memberid", LivePay_MemberID },
                { "notifyurl", LivePay_NotifyUrl },
                { "orderid", order.Id.ToString() }
            };

            string signString = string.Join("&", paras.Select((kvp) => kvp.Key + "=" + kvp.Value.ToString())) + "&key=" + LivePay_Key;
            var sign = Md5(signString, Encoding.UTF8).ToUpper();

            paras.Add("productname", "在线支付");
            paras.Add("ddlx", "0");         //ddlx = 0 商户在线支付订单
            paras.Add("md5sign", sign);
            paras.Add("serverReturnType", "1");

            var url = $"<html><body><form method='post' name='form' action='{LivePay_Url}' enctype=\"application/x-www-form-urlencoded\">" +
                    string.Join("", paras.Select(m => string.Format("<input type='hidden' name='{0}' value='{1}' >", m.Key, m.Value))) +
                    "</form><script>window.onload = function(){document.forms['form'].submit();}</script></body></html>";

            SiteHelper.Log(guid, $"{order.Id} 支付请求 [data]({url}) ", LivePay_logFile);

            var api = Service.CreateRedirectData((int)PayGatewayType.LivePay, url);

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
                    SiteHelper.Log(logGuid, string.Format("实时支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), LivePay_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("实时支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), LivePay_logFile);
                }
                Dictionary<string, string> parameters = null;

                if (!CheckSign(logGuid, out parameters))
                {
                    SiteHelper.Log(logGuid, string.Format("实时支付回调非法签名"), LivePay_logFile);
                    return PayState.SignError;
                }

                var memberid = parameters.ContainsKey("memberid") ? parameters["memberid"] : string.Empty;                  //商户分配的ID
                var outtradeid = parameters.ContainsKey("outtradeid") ? parameters["outtradeid"] : string.Empty;            //商户订单号
                var payorderid = parameters.ContainsKey("payorderid") ? parameters["payorderid"] : string.Empty;            //四方平台的支付流水号
                var amount = parameters.ContainsKey("amount") ? parameters["amount"] : string.Empty;                        //交易金额
                var datetime = parameters.ContainsKey("datetime") ? parameters["datetime"] : string.Empty;                  //交易时间
                var returncode = parameters.ContainsKey("returncode") ? parameters["returncode"] : string.Empty;            //交易状态(“00”表示支付成功)
                var sign = parameters.ContainsKey("sign") ? parameters["sign"] : string.Empty;                              //四方平台的签名

                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(outtradeid));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 实时支付回调 訂單狀態已經完成,無須金幣充值", order.Id), LivePay_logFile);
                    return PayState.Success;
                }

                //進行金幣充值
                string msg = "";

                if (returncode.Equals("00") && (Convert.ToDecimal(amount) == order.Amount))
                {
                    var data = $"实时支付回调 完成支付: 支付订单号: {outtradeid} , 渠道订单号: {payorderid}, 状态: success";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, payorderid, data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 实时支付回调 金幣充值成功 支付订单号:{outtradeid} 金额:{order.Amount} 渠道订单号:{payorderid}", LivePay_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 实时支付回调 金幣充值错误 {msg}", LivePay_logFile);

                        Service.CDFailOrder(order.Id, payorderid, $"实时支付订单号:{outtradeid} , 渠道订单号: {payorderid} , 支付金额: {amount}, 状态: fail");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, $"{order.Id}  实时支付回调 支付無效 支付订单号:{outtradeid}, 渠道订单号:{payorderid}, 支付金额:{amount}, 状态: fail", LivePay_logFile);
                Service.CDFailOrder(order.Id, "0", $"实时支付订单号:{outtradeid} , 渠道订单号: {payorderid} , 支付金额: {amount}, 状态: fail");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("实时支付回调 异常错误 {0}", ex.ToString()), LivePay_logFile);
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

            var memberid = parameters.ContainsKey("memberid") ? parameters["memberid"] : string.Empty;                  //商户分配的ID
            var outtradeid = parameters.ContainsKey("outtradeid") ? parameters["outtradeid"] : string.Empty;            //商户订单号
            var payorderid = parameters.ContainsKey("payorderid") ? parameters["payorderid"] : string.Empty;            //四方平台的支付流水号
            var amount = parameters.ContainsKey("amount") ? parameters["amount"] : string.Empty;                        //交易金额
            var datetime = parameters.ContainsKey("datetime") ? parameters["datetime"] : string.Empty;                  //交易时间
            var returncode = parameters.ContainsKey("returncode") ? parameters["returncode"] : string.Empty;            //交易状态(“00”表示支付成功)
            var sign = parameters.ContainsKey("sign") ? parameters["sign"] : string.Empty;                              //四方平台的签名

            var data = $"amount={amount}" +
                $"&datetime={datetime}" +
                $"&memberid={memberid}" +
                $"&outtradeid={outtradeid}" +
                $"&payorderid={payorderid}" +
                $"&returncode={returncode}" +
                $"&key={LivePay_Key}";
            var cSign = Md5(data, Encoding.UTF8).ToUpper();
            SiteHelper.Log(guid, $"实时支付回调签名 data={data} cSign={cSign} sign={sign}", LivePay_logFile);
            return sign == cSign;
        }
    }
}
