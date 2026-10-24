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
using System.Text.RegularExpressions;
using System.Web;
using static game.WebSite.PayConfig;

namespace game.WebSite.PaymentHelper
{
    public class JFPay
    {
        /// <summary>
        /// 创建订单(跳转网页链接)
        /// </summary>
        /// <param name="order"></param>
        /// <param name="guid"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string post(CDPayOrder order, string guid, string tradeCode, ref string message)
        {
            string url = string.Empty;
            string d = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Dictionary<string, string> paras = new Dictionary<string, string>
            {
                { "version", "1.0"  },
                { "mer_id", jfpay_partner  },
                { "order_id", (order.Id).ToString()  },
                { "price", (order.Amount*100).ToString()  },
                { "notify_url", jfpay_shopNotifyUrl  },
                { "return_url", jfpay_shopCallbackUrl  },
                { "pay_type", tradeCode  },
                { "randomid", "1234"  },
               

            };
            var sign = Md5($"version=1.0&mer_id={jfpay_partner}&order_id={(order.Id).ToString()}&price={(order.Amount * 100).ToString()}&notify_url={jfpay_shopNotifyUrl}&pay_type={tradeCode}&randomid=1234&key={jfpay_Key}", Encoding.UTF8).ToLower();
            paras.Add("sign", sign);
            var data = string.Join("&", paras.Select(m => string.Format("{0}={1}", m.Key, m.Value)));
            string PayUrl = string.Empty;
            if (order.PayTypeID == 1 || order.PayTypeID == 11)
                PayUrl = jfpay_alipayPayURL;
            else if (order.PayTypeID == 2 || order.PayTypeID == 21)
                PayUrl = jfpay_wechatPayURL;
            else
                PayUrl = jfpay_bankPayURL;

            SiteHelper.Log(guid, $"{order.Id} 支付请求 [data]({PayUrl}) {data}", jfpay_logFile);
            var responseString = SiteHelper.HttpPost(PayUrl, data);           
            SiteHelper.Log(guid, $"{order.Id} 支付请求回覆 [data] {responseString}", jfpay_logFile);

            JObject json = JObject.Parse(responseString);
            var code = json["code"] != null ? json["code"].ToString() : string.Empty;
            var msg = json["message"] != null ? json["message"].ToString() : string.Empty;
            if (!code.Equals("00"))
            {              
                message = string.Format("{0} 支付请求失敗 {1}", order.Id, Regex.Unescape(msg) );

                //更新狀態
                Service.CDFailOrder(order.Id, string.Empty, msg);
                SiteHelper.Log(guid, message, Baifu_logFile);
            }
            else
            {
                 JObject jsondata = JObject.Parse(json["data"].ToString());
                
                url = jsondata["qrcode_url"] != null ? jsondata["qrcode_url"].ToString() : string.Empty;
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
                    SiteHelper.Log(logGuid, string.Format("金发支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), jfpay_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("金发支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), jfpay_logFile);
                }
                Dictionary<string, string> parameters = null;

                if (!CheckSign(logGuid, out parameters))
                {
                    SiteHelper.Log(logGuid, string.Format("金发支付回调非法签名"), jfpay_logFile);
                    return PayState.SignError;
                }



                //支付状态，00表示支付成功;
                var returncode = parameters.ContainsKey("code") ? parameters["code"] : string.Empty;

                //商家订单号 
                var orderid = parameters.ContainsKey("order_id") ? parameters["order_id"] : string.Empty;

                //订单金额
                var amount = parameters.ContainsKey("price") ? parameters["price"] : string.Empty;

                var trxNo = parameters.ContainsKey("pay_order") ? parameters["pay_order"] : string.Empty;

                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(orderid));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 金发支付回调 訂單狀態已經完成,無須金幣充值", order.Id), jfpay_logFile);
                    return PayState.Success;
                }

                //進行金幣充值
                string msg = "";

                if (returncode.Equals("00") && ((Convert.ToDecimal(amount)/100) == order.Amount))
                {
                    var data = $"金发支付回调 完成支付: 支付订单号: {orderid} , 渠道订单号: {trxNo}, 状态: {returncode}";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, trxNo, data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 金发支付回调 金幣充值成功 支付订单号:{orderid} 金额:amount={order.Amount} 渠道订单号:{trxNo}", jfpay_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 金发支付回调 金幣充值错误 {msg}", jfpay_logFile);

                        Service.CDFailOrder(order.Id, trxNo, $"金发支付订单号:{orderid} , 渠道订单号: {trxNo} , 支付金额(分): {(Convert.ToDecimal(amount)).ToString()}, 状态: {returncode}");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, $"{order.Id}  金发支付回调 支付無效 支付订单号:{orderid}, 渠道订单号:{trxNo}, 支付金额(分):{(Convert.ToDecimal(amount) ).ToString()}, 状态:{returncode}", jfpay_logFile);
                Service.CDFailOrder(order.Id, trxNo, $"金发支付订单号:{orderid} , 渠道订单号: {trxNo} , 支付金额(分): {(Convert.ToInt64(amount) * 100).ToString()}, 状态: {returncode}");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("金发支付回调 异常错误 {0}", ex.ToString()), jfpay_logFile);
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

            var pay_order = parameters.ContainsKey("pay_order") ? parameters["pay_order"] : string.Empty;
            var order_id = parameters.ContainsKey("order_id") ? parameters["order_id"] : string.Empty;
            var price = parameters.ContainsKey("price") ? parameters["price"] : string.Empty;
            var pay_type = parameters.ContainsKey("pay_type") ? parameters["pay_type"] : string.Empty;
            var code = parameters.ContainsKey("code") ? parameters["code"] : string.Empty;
            var timestamp = parameters.ContainsKey("timestamp") ? parameters["timestamp"] : string.Empty;
            var sign = parameters.ContainsKey("sign") ? parameters["sign"] : string.Empty;

            var cSign = Md5($"pay_order={pay_order}&order_id={order_id}&price={price}&pay_type={pay_type}&code={code}&timestamp={timestamp}&key={jfpay_Key}", Encoding.UTF8).ToLower();
            SiteHelper.Log(guid, $"金发支付回调签名 cSign={cSign} sign={sign}", jfpay_logFile);
            return sign == cSign;

        }
    }
}