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
    public class PPZFPay
    {
        public class PPResult
        {
            public string errcode { get; set; }
            public string errmsg { get; set; }
            public string orderno { get; set; }
            public string tradeamt { get; set; }
            public string qrcode { get; set; }
            public string imgurl { get; set; }
            public string formtext { get; set; }
           
        }

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
                { "merchantID", ppzfpay_mchid},
                { "bankCode", tradeCode},
                { "tradeAmt", order.Amount.ToString("f2")  },
                { "orderNo", order.Id.ToString()  },
                { "notifyUrl", ppzfpay_shopNotifyUrl  },
                { "returnUrl", ppzfpay_shopCallBackUrl},
                { "payerID", order.UserID.ToString()},
                { "payerIP", order.IPAddress},
            };

            var sign = Md5($"merchantID={ppzfpay_mchid}&bankCode={tradeCode}&tradeAmt={order.Amount.ToString("f2")}&orderNo={order.Id.ToString()}&notifyUrl={ppzfpay_shopNotifyUrl}&returnUrl={ppzfpay_shopCallBackUrl}&payerIP={order.IPAddress}{ppzfpay_Key}", Encoding.UTF8).ToLower();
            paras.Add("sign", sign);
            var data = string.Join("&", paras.Select(m => string.Format("{0}={1}", m.Key, m.Value)));
            SiteHelper.Log(guid, $"{order.Id} 支付请求 [data]({ppzfpay_PayURL}) {data}", ppzfpay_logFile);
            string responseString = SiteHelper.HttpPost(ppzfpay_PayURL, data);
            SiteHelper.Log(guid, $"{order.Id} 支付请求 [data] {responseString}", ppzfpay_logFile);
            JObject res = JObject.Parse(responseString);
            if (res["errmsg"] != null)
            {
                var result = JsonConvert.DeserializeObject<PPResult>(responseString);
                if (!result.errcode.Equals("0"))
                {
                    outMessage = $"支付请求失敗({result.orderno}) {result.errmsg}";
                    //更新狀態
                    Service.CDFailOrder(order.Id, string.Empty, outMessage);
                    SiteHelper.Log(guid, outMessage, ppzfpay_logFile);
                    return String.Empty;
                }
                else
                {
                   
                    
                    return result.qrcode;
                }
            }
            else
            {              
                //更新狀態
                Service.CDFailOrder(order.Id, string.Empty, outMessage);
                SiteHelper.Log(guid, outMessage, ppzfpay_logFile);
                return String.Empty;
            }
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
                    SiteHelper.Log(logGuid, string.Format("派派支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), ppzfpay_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("派派支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), ppzfpay_logFile);
                }
                Dictionary<string, string> parameters = null;

                if (!CheckSign(logGuid, out parameters))
                {
                    SiteHelper.Log(logGuid, string.Format("派派支付回调非法签名"), ppzfpay_logFile);
                    return PayState.SignError;
                }

                //支付状态，pay_success表示支付成功;
                var orderstatus = parameters.ContainsKey("tradeState") ? parameters["tradeState"] : string.Empty;

                //商家订单号 
                var orderid = parameters.ContainsKey("orderNo") ? parameters["orderNo"] : string.Empty;

                //订单金额
                var amount = parameters.ContainsKey("tradeAmt") ? parameters["tradeAmt"] : string.Empty;

                var tradeNo = parameters.ContainsKey("tradeNo") ? parameters["tradeNo"] : string.Empty;

                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(orderid));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 派派支付回调 訂單狀態已經完成,無須金幣充值", order.Id), ppzfpay_logFile);
                    return PayState.Success;
                }

                //進行金幣充值
                string msg = "";

                if (orderstatus.Equals("0") && ((Convert.ToDecimal(amount)) == order.Amount))
                {
                    var data = $"派派支付回调 完成支付: 支付订单号: {orderid} , 渠道订单号: {"0"}, 状态: {orderstatus}";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, tradeNo, data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 派派支付回调 金幣充值成功 支付订单号:{orderid} 金额:amount={order.Amount} 渠道订单号:{tradeNo}", ppzfpay_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 派派支付回调 金幣充值错误 {msg}", ppzfpay_logFile);

                        Service.CDFailOrder(order.Id, tradeNo, $"派派支付订单号:{orderid} , 渠道订单号: {tradeNo} , 支付金额(分): {(Convert.ToInt64(amount) * 100).ToString()}, 状态: {orderstatus}");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, $"{order.Id}  派派支付回调 支付無效 支付订单号:{orderid}, 渠道订单号:{"0"}, 支付金额(分):{(Convert.ToInt64(amount) * 100).ToString()}, 状态:{orderstatus}", ppzfpay_logFile);
                Service.CDFailOrder(order.Id, "0", $"派派支付订单号:{orderid} , 渠道订单号: {tradeNo} , 支付金额(分): {(Convert.ToDecimal(amount) * 100).ToString()}, 状态: {orderstatus}");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("派派支付回调 异常错误 {0}", ex.ToString()), ppzfpay_logFile);
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

            var merchantID = parameters.ContainsKey("merchantID") ? parameters["merchantID"] : string.Empty;
            var orderNo = parameters.ContainsKey("orderNo") ? parameters["orderNo"] : string.Empty;
            var tradeAmt = parameters.ContainsKey("tradeAmt") ? parameters["tradeAmt"] : string.Empty;
            var tradeState = parameters.ContainsKey("tradeState") ? parameters["tradeState"] : string.Empty;
            var tradeNo = parameters.ContainsKey("tradeNo") ? parameters["tradeNo"] : string.Empty;
           
            var sign = parameters.ContainsKey("sign") ? parameters["sign"] : string.Empty;

            var cSign = Md5($"merchantID={merchantID}&orderNo={orderNo}&tradeAmt={tradeAmt}&tradeState={tradeState}&tradeNo={tradeNo}{ppzfpay_Key}", Encoding.UTF8).ToLower();
            SiteHelper.Log(guid, $"派派支付回调签名 cSign={cSign} sign={sign}", ppzfpay_logFile);
            return sign == cSign;
        }
    }
}