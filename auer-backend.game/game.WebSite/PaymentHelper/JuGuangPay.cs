using game.Entity.Pay;
using game.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    public class JuGuangPay
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
            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        
            Dictionary<string, string> paras = new Dictionary<string, string>
            {
                { "channelType", "2" },
                { "payType", tradeCode},
                { "merchantNum", juguangpay_code},
                { "merchantOrderNum", order.Id.ToString()},
                { "notifyUrl", juguangpay_shopNotifyUrl},
                { "returnUrl", juguangpay_shopCallBackUrl},                
                { "amount", order.Amount.ToString("f2")},               
                { "time",unixTimestamp.ToString() }
            };

            var signdata = string.Join("&", paras.OrderBy(x => x.Key).Select(m => string.Format("{0}={1}", m.Key, m.Value))) + "&" + juguangpay_key;
            //支付请求的签名方式
            var sign = Md5($"{signdata}", Encoding.UTF8).ToLower();
            paras.Add("signature", sign);
            SiteHelper.Log(guid, $"{order.Id} 支付请求签名 [data]{sign} {signdata}", juguangpay_logFile);
            //api的请求，都是post + json的方式。
            var data = JsonConvert.SerializeObject(paras);
            SiteHelper.Log(guid, $"{order.Id} 支付请求 [data]({juguangpay_PayURL}) {data}", juguangpay_logFile);

            //呼叫支付平台
            string responseString = SiteHelper.HttpPostJson(juguangpay_PayURL, data);
            SiteHelper.Log(guid, $"{order.Id} 支付请求 [data] {responseString}", juguangpay_logFile);


            JObject json = JObject.Parse(responseString);
            var success = json["success"] != null ? json["success"].ToString() : string.Empty;
            

            if (success.Equals("True"))
            {
                var orderjson = json["data"] != null ? json["data"].ToString() : string.Empty;
                JObject orderdata = JObject.Parse(orderjson);
                var payurl = orderdata["payUrl"] != null ? orderdata["payUrl"].ToString() : string.Empty;
             
                return payurl;               
            }
            else
            {
                SiteHelper.Log(guid, json["message"].ToString(), juguangpay_logFile);
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
                    SiteHelper.Log(logGuid, string.Format("聚光支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), juguangpay_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("聚光支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), juguangpay_logFile);
                }
                Dictionary<string, string> parameters = null;

                if (!CheckSign(logGuid, out parameters))
                {
                    SiteHelper.Log(logGuid, string.Format("聚光支付回调非法签名"), juguangpay_logFile);
                    return PayState.SignError;
                }

                //支付状态，00表示支付成功;
                var returncode = parameters.ContainsKey("payStatus") ? parameters["payStatus"].ToLower() : string.Empty;

                //商家订单号 
                var orderid = parameters.ContainsKey("merchantOrderNum") ? parameters["merchantOrderNum"] : string.Empty;

                //订单金额
                var amount = parameters.ContainsKey("payAmount") ? parameters["payAmount"] : string.Empty;

                //平台订单号 
                var tradeNo = parameters.ContainsKey("payOrderNum") ? parameters["payOrderNum"] : string.Empty;

                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(orderid));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 聚光支付回调 訂單狀態已經完成,無須金幣充值", order.Id), juguangpay_logFile);
                    return PayState.Success;
                }

                //進行金幣充值
                string msg = "";

                if (returncode.Equals("1") && ((Convert.ToDecimal(amount) ) == order.Amount))
                {
                    var data = $"聚光支付回调 完成支付: 支付订单号: {orderid} , 渠道订单号: {tradeNo}, 状态: {returncode}";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, tradeNo, data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 聚光支付回调 金幣充值成功 支付订单号:{orderid} 金额:amount={order.Amount} 渠道订单号:{tradeNo}", juguangpay_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 聚光支付回调 金幣充值错误 {msg}", juguangpay_logFile);
                        Service.CDFailOrder(order.Id, "0", $"聚光支付订单号:{orderid} , 渠道订单号: {tradeNo} , 支付金额(分): {(Convert.ToDecimal(amount)*100).ToString()}, 状态: {returncode}");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, $"{order.Id}  聚光支付回调 支付無效 支付订单号:{orderid}, 渠道订单号:{tradeNo}, 支付金额(分):{(Convert.ToDecimal(amount)*100).ToString()}, 状态:{returncode}", juguangpay_logFile);
                Service.CDFailOrder(order.Id, tradeNo, $"聚光支付订单号:{orderid} , 渠道订单号: {tradeNo} , 支付金额(分): {(Convert.ToDecimal(amount)*100).ToString()}, 状态: {returncode}");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("聚光支付回调 异常错误 {0}", ex.ToString()), juguangpay_logFile);
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
            if (parameters.Count == 0)
            {
                Stream stream = HttpContext.Current.Request.InputStream;
                var b = new byte[stream.Length];
                stream.Read(b, 0, (int)stream.Length);
                var responseString = Encoding.UTF8.GetString(b);
                SiteHelper.Log(guid, $"聚光支付回调串流 DATA={responseString}", xinquanfu_logFile);

                JObject jsonstr = JObject.Parse(responseString);
                Dictionary<string, object> paras = jsonstr.ToObject<Dictionary<string, object>>();
                
                parameters = paras.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value.ToString());
            }

            var sign = parameters.ContainsKey("signature") ? parameters["signature"] : string.Empty;
            parameters.Remove("signature");

            var signdata = string.Join("&", parameters.Select(m => string.Format("{0}={1}", m.Key, m.Value))) + "&" + juguangpay_key;

            SiteHelper.Log(guid, $"回传加密资料 [data] {signdata}", juguangpay_logFile);


            var cSign = Md5(signdata, Encoding.UTF8).ToLower();
            SiteHelper.Log(guid, $"聚光支付回调签名 cSign={cSign} sign={sign}", juguangpay_logFile);
            return sign == cSign;
        }
    }
}