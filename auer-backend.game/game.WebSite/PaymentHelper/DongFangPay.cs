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
    public class DongFangPay
    {
        /// <summary>
        /// 创建订单(跳转网页链接)
        /// </summary>
        /// <param name="order"></param>
        /// <param name="guid"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string post(CDPayOrder order, string guid, string tradeCode,ref bool isQrcodeBuild)
        {
            string url = string.Empty;
            string d = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Dictionary<string, string> paras = new Dictionary<string, string>
            {
                { "user_id", dongfangpay_partner },
                { "out_trade_no", order.Id.ToString()},
                { "product_id", tradeCode},
                { "notify_url", dongfangpay_shopNotifyUrl},
                { "return_url", dongfangpay_shopCallBackUrl},
                { "subject", "诺亚吸尘器2合1"},
                { "pay_amount", (order.Amount*100).ToString()},
                { "applydate",d }
            };

            var signdata = string.Join("&", paras.OrderBy(x => x.Key).Select(m => string.Format("{0}={1}", m.Key, m.Value))) + "&apikey=" + dongfangpay_key;
            //支付请求的签名方式
            var sign = Md5($"{signdata}", Encoding.UTF8).ToUpper();
            paras.Add("sign", sign);
            SiteHelper.Log(guid, $"{order.Id} 支付请求签名 [data]{sign} {signdata}", dongfangpay_logFile);
            //api的请求，都是post + json的方式。
            var data = JsonConvert.SerializeObject(paras);
            SiteHelper.Log(guid, $"{order.Id} 支付请求 [data]({dongfangpay_PayURL}) {data}", dongfangpay_logFile);

            //呼叫支付平台
            string responseString = SiteHelper.HttpPostJson(dongfangpay_PayURL, data);
            SiteHelper.Log(guid, $"{order.Id} 支付请求 [data] {responseString}", dongfangpay_logFile);


            JObject json = JObject.Parse(responseString);
            var code = json["code"] != null ? json["code"].ToString() : string.Empty;


            if (code.Equals("1"))
            {
                var orderjson = json["data"] != null ? json["data"].ToString() : string.Empty;
                JObject orderdata = JObject.Parse(orderjson);
                var pay_extendsjson = orderdata["pay_extends"] != null ? orderdata["pay_extends"].ToString() : string.Empty;
                JObject pay_extendsdata = JObject.Parse(pay_extendsjson);

                var pay_url = pay_extendsdata["pay_url"] != null ? pay_extendsdata["pay_url"].ToString() : string.Empty;
                var pay_action = pay_extendsdata["pay_action"] != null ? pay_extendsdata["pay_action"].ToString() : string.Empty;
                isQrcodeBuild = pay_action.Equals("qrcode");
                
                return pay_url;
            }
            else
            {
                SiteHelper.Log(guid, json["msg"].ToString(), dongfangpay_logFile);
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
                    SiteHelper.Log(logGuid, string.Format("东方支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), dongfangpay_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("东方支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), dongfangpay_logFile);
                }
                Dictionary<string, string> parameters = null;

                if (!CheckSign(logGuid, out parameters))
                {
                    SiteHelper.Log(logGuid, string.Format("东方支付回调非法签名"), dongfangpay_logFile);
                    return PayState.SignError;
                }

                //支付状态，00表示支付成功;
                var returncode = parameters.ContainsKey("returncode") ? parameters["returncode"].ToLower() : string.Empty;

                //商家订单号 
                var orderid = parameters.ContainsKey("out_trade_no") ? parameters["out_trade_no"] : string.Empty;

                //订单金额
                var amount = parameters.ContainsKey("transaction_money") ? parameters["transaction_money"] : string.Empty;

                //平台订单号 
                var tradeNo = parameters.ContainsKey("trade_no") ? parameters["trade_no"] : string.Empty;

                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(orderid));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 东方支付回调 訂單狀態已經完成,無須金幣充值", order.Id), dongfangpay_logFile);
                    return PayState.Success;
                }

                //進行金幣充值
                string msg = "";

                if (returncode.Equals("00") && ((Convert.ToDecimal(amount)/100) == order.Amount))
                {
                    var data = $"东方支付回调 完成支付: 支付订单号: {orderid} , 渠道订单号: {tradeNo}, 状态: {returncode}";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, tradeNo, data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 东方支付回调 金幣充值成功 支付订单号:{orderid} 金额:amount={order.Amount} 渠道订单号:{tradeNo}", dongfangpay_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 东方支付回调 金幣充值错误 {msg}", dongfangpay_logFile);
                        Service.CDFailOrder(order.Id, "0", $"东方支付订单号:{orderid} , 渠道订单号: {tradeNo} , 支付金额(分): {(Convert.ToDecimal(amount) ).ToString()}, 状态: {returncode}");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, $"{order.Id}  东方支付回调 支付無效 支付订单号:{orderid}, 渠道订单号:{tradeNo}, 支付金额(分):{(Convert.ToDecimal(amount) ).ToString()}, 状态:{returncode}", dongfangpay_logFile);
                Service.CDFailOrder(order.Id, tradeNo, $"东方支付订单号:{orderid} , 渠道订单号: {tradeNo} , 支付金额(分): {(Convert.ToDecimal(amount)).ToString()}, 状态: {returncode}");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("东方支付回调 异常错误 {0}", ex.ToString()), dongfangpay_logFile);
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
                SiteHelper.Log(guid, $"东方支付回调串流 DATA={responseString}", xinquanfu_logFile);

                JObject jsonstr = JObject.Parse(responseString);
                Dictionary<string, object> paras = jsonstr.ToObject<Dictionary<string, object>>();

                parameters = paras.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value.ToString());
            }

            var sign = parameters.ContainsKey("sign") ? parameters["sign"] : string.Empty;
            parameters.Remove("sign");

            var signdata = string.Join("&", parameters.Where(x=>!string.IsNullOrEmpty(x.Value)).Select(m => string.Format("{0}={1}", m.Key, m.Value))) + "&apikey=" + dongfangpay_key;

            SiteHelper.Log(guid, $"回传加密资料 [data] {signdata}", dongfangpay_logFile);


            var cSign = Md5(signdata, Encoding.UTF8).ToUpper();
            SiteHelper.Log(guid, $"东方支付回调签名 cSign={cSign} sign={sign}", dongfangpay_logFile);
            return sign == cSign;
        }
    }
}