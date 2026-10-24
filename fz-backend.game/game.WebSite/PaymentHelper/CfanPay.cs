using System;
using System.Collections.Generic;
using System.Linq;
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

namespace game.WebSite.PaymentHelper
{
    public class CfanPay
    {
        public static string post(CDPayOrder order, string guid, string tradeCode, ref string message)
        {
            string url = string.Empty;

            Dictionary<string, string> paras = new Dictionary<string, string>
            {
                { "merchantId", CfanPay_mchId },
                { "orderNo", order.Id.ToString() },               
                { "amount", (order.Amount * 100).ToString() },
                { "clientIp", order.IPAddress },
                { "tradeDate", DateTime.Now.ToString("yyyyMMdd") },
                { "tradeTime", DateTime.Now.ToString("HHmmss") },
                { "service", tradeCode },
                { "version", "1.0" },
                { "key", CfanPay_key },
                { "notifyUrl", CfanPay_CallbackUrl }
            };

            string parasData = string.Join("&", paras.OrderBy(x => x.Key).Select((m => $"{m.Key}={m.Value}")));
            var pay_md5sign = Md5(parasData, Encoding.UTF8);
            paras.Add("sign", pay_md5sign);
            paras.Remove("key");

            var data = string.Join("&", paras.OrderBy(x => x.Key).Select((m => $"{m.Key}={m.Value}")));
            SiteHelper.Log(guid, string.Format("{0} 支付请求 [data]({1}) {2}", order.Id, CfanPay_PayURL, data), CfanPay_logFile);


            //呼叫支付平台
            var responseString = SiteHelper.HttpPost(CfanPay_PayURL, data);
            SiteHelper.Log(guid, string.Format("{0} 支付请求结果 [response] {1}", order.Id, responseString), CfanPay_logFile);

            var resultDic = new Dictionary<string, string>();
            foreach (var i in responseString.Split('&'))
            {
                var array = i.Split('=');
                resultDic.Add(array[0], array[1]);
                
            }
                        
            var result = resultDic.ContainsKey("repCode") ? resultDic["repCode"].ToString() : string.Empty;

            if (!result.Equals("0001"))
            {
                var repCode = resultDic.ContainsKey("repCode") ? resultDic["repCode"].ToString() : string.Empty;
                var repMsg = resultDic.ContainsKey("repMsg") ? resultDic["repMsg"].ToString() : string.Empty;
                message = string.Format("{0} 支付请求失敗 {1} {2}", order.Id, repCode, repMsg);

                //更新狀態
                Service.CDFailOrder(order.Id, string.Empty, repCode + repMsg);
                SiteHelper.Log(guid, message, CfanPay_logFile);
            }
            else
            {
                Service.CDModifyTransID(order.Id, "0");
                url = resultDic.ContainsKey("resultUrl") ? resultDic["resultUrl"].ToString() : string.Empty;
            }

            return url;
        }       

        public static PayState call(string logGuid)
        {
            try
            {
                var requestType = HttpContext.Current.Request.RequestType;
                if (requestType == "GET")
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), CfanPay_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), CfanPay_logFile);
                }

                Dictionary<string, string> parameters = null;

                if (!CheckSign(logGuid, out parameters))
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调非法签名"), CfanPay_logFile);
                    return PayState.SignError;
                }

                var merchantId = parameters.ContainsKey("merchantId") ? parameters["merchantId"] : string.Empty;
                var orderNo = parameters.ContainsKey("orderNo") ? parameters["orderNo"] : string.Empty;
                var tradeDate = parameters.ContainsKey("tradeDate") ? parameters["tradeDate"] : string.Empty;
                var tradeTime = parameters.ContainsKey("tradeTime") ? parameters["tradeTime"] : string.Empty;
                var amount = parameters.ContainsKey("amount") ? parameters["amount"] : string.Empty;
                var resultCode = parameters.ContainsKey("resultCode") ? parameters["resultCode"] : string.Empty;
                //var repMsg = parameters.ContainsKey("version") ? parameters["version"] : string.Empty;


                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(orderNo));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 支付回调 訂單狀態已經完成,無須金幣充值", order.Id), CfanPay_logFile);
                    return PayState.Success;
                }

                if (resultCode != "0")
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 支付回调 订单失敗 resultCode:{1}", order.Id, resultCode), CfanPay_logFile);
                    Service.CDFailOrder(order.Id, null, $"resultCode:{resultCode}");
                    return PayState.Error;
                }

                //進行金幣充值
                string msg = "";

                if (Convert.ToInt64(amount) == order.Amount * 100)
                {
                    var data = $"完成支付 resultCode:{resultCode},tradeDate:{tradeDate},tradeTime:{tradeTime},amount:{amount}";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, "0", data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 支付回调 金幣充值成功 resultCode:{resultCode},tradeDate:{tradeDate},tradeTime:{tradeTime},amount:{amount}", CfanPay_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 支付回调 金幣充值错误 {msg}", CfanPay_logFile);
                        Service.CDFailOrder(order.Id, null, $"{msg}");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, string.Format($"{order.Id}  支付回调 支付無效 resultCode:{resultCode},tradeDate:{tradeDate},tradeTime:{tradeTime},amount:{amount}"), CfanPay_logFile);
                Service.CDFailOrder(order.Id, null, $"resultCode:{resultCode},tradeDate:{tradeDate},tradeTime:{tradeTime},amount:{amount}");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("支付回调 异常错误 {0}", ex.ToString()), CfanPay_logFile);
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

            var sign = parameters["sign"];
            parameters.Remove("sign");
            parameters.Add("key", CfanPay_key);
            parameters = parameters.OrderBy(o => o.Key).ToDictionary(o => o.Key, p => p.Value);

            var str = string.Join("&", parameters.Select(m => string.Format("{0}={1}", m.Key, m.Value)));
            var cSign = Md5(str, Encoding.UTF8).ToLower();
            SiteHelper.Log(guid, string.Format("支付回调签名 cSign={0} sign={1} srt={2}", cSign, sign, str), Xunpay_logFile);

            return sign == cSign;
        }
    }
}