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
   
    public class transparm
    {
        public string outTradeNo { get; set; }
        public long money { get; set; }
        public string type { get; set; }
        public string body { get; set; }
        public string detail { get; set; }
        public string notifyUrl { get; set; }
        public string productId { get; set; }
        public string successUrl { get; set; }
      
    }

    public class RequestParm
    {
        public string accessToken { get; set; }
        public transparm param { get; set; }
    }
    public class ShouMiPay
    {
        public static string post(CDPayOrder order, string guid, string tradeCode)
        {
        
        string url = string.Empty;
            string accessToken = [REDACTED];
            string PayUrl = string.Empty;
            string d = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            string nonce = "1234";
            Dictionary<string, string> paras = new Dictionary<string, string>
            {
                { "merchantNo", shoumipay_partner },
                { "key", shoumipay_Key},
                { "token",null},
                { "nonce", nonce},
                { "timestamp", unixTimestamp.ToString()},
            };

            var sign = Md5($"merchantNo={shoumipay_partner}&nonce={nonce}&timestamp={unixTimestamp.ToString()}&key={shoumipay_Key}", Encoding.UTF8).ToUpper();
            paras.Add("sign", sign);
            
            //   var data = string.Join("&", paras.Select(m => string.Format("{0}={1}", m.Key, m.Value)));
            var data = JsonConvert.SerializeObject(paras);
            SiteHelper.Log(guid, string.Format("{0} 支付请求 [data]({1}) {2}", order.Id, shoumitoken_url, data), shoumipay_logFile);

            //呼叫支付平台
            string responseString = SiteHelper.HttpPostJson(shoumitoken_url, data);
            SiteHelper.Log(guid, string.Format("{0} 支付请求回覆 [data]({1}) {2}", order.Id, shoumitoken_url, responseString), shoumipay_logFile);
            JObject json = JObject.Parse(responseString);
            var success = json["success"] != null ? json["success"].ToString() : string.Empty;

            if (success.Equals("True"))
            {
                var value = json["value"] != null ? json["value"].ToString() : string.Empty;
                JObject valuejson = JObject.Parse(value);
                accessToken = [REDACTED]"accessToken"] != null ? valuejson["accessToken"].ToString() : string.Empty;

                if (order.PayTypeID == 1)
                    PayUrl = shoumialipaywap_url;
                else if (order.PayTypeID == 11)
                    PayUrl = shoumialipayscan_url;
                else if (order.PayTypeID == 2)
                    PayUrl = shoumiwechatwap_url;
                else if (order.PayTypeID == 21)
                    PayUrl = shoumiwechatscan_url;
                transparm tp = new transparm();
                tp.outTradeNo = order.Id.ToString();
                tp.money = order.Amount * 100;
                tp.type="T1";
                tp.body = "body";
                tp.detail = "detail";
                tp.notifyUrl = shoumipay_notifyUrl;
                tp.productId = "1";
                tp.successUrl = shoumipay_callbackurl;
                RequestParm rp = new RequestParm();
                rp.accessToken = [REDACTED];
                rp.param =tp;
               

                data = JsonConvert.SerializeObject(rp);
                SiteHelper.Log(guid, string.Format("{0} 支付请求2 [data]({1}) {2}", order.Id, PayUrl, data), shoumipay_logFile);
                //呼叫支付平台
                responseString = SiteHelper.HttpPostJson(PayUrl, data);
                SiteHelper.Log(guid, string.Format("{0} 支付请求回覆2 [data]({1}) {2}", order.Id, PayUrl, responseString), shoumipay_logFile);

                json = JObject.Parse(responseString);
                success = json["success"] != null ? json["success"].ToString() : string.Empty;

                if (success.Equals("True"))
                {
                 var valueurl = json["value"] != null ? json["value"].ToString() : string.Empty;
                    url = valueurl;
                }
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
                    SiteHelper.Log(logGuid, string.Format("收米支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), shoumipay_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("收米支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), shoumipay_logFile);
                }
                Dictionary<string, string> parameters = null;

                if (!CheckSign(logGuid, out parameters))
                {
                    SiteHelper.Log(logGuid, string.Format("收米支付回调非法签名"), shoumipay_logFile);
                    return PayState.SignError;
                }

                //支付状态，00表示支付成功;
                var returncode = parameters.ContainsKey("success") ? parameters["success"] : string.Empty;

                //商家订单号 
                var orderid = parameters.ContainsKey("outTradeNo") ? parameters["outTradeNo"] : string.Empty;

                //订单金额
                var amount = parameters.ContainsKey("money") ? parameters["money"] : string.Empty;

                //平台订单号 
                var tradeno = parameters.ContainsKey("no") ? parameters["no"] : string.Empty;


                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(orderid));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 收米支付回调 訂單狀態已經完成,無須金幣充值", order.Id), shoumipay_logFile);
                    return PayState.Success;
                }

                //進行金幣充值
                string msg = "";

                if (returncode.Equals("True") && ((Convert.ToDecimal(amount)/100) == order.Amount))
                {
                    var data = $"收米支付回调 完成支付: 支付订单号: {orderid} , 渠道订单号: {tradeno}, 状态: {returncode}";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, tradeno, data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 收米支付回调 金幣充值成功 支付订单号:{orderid} 金额:amount={order.Amount} 渠道订单号:{tradeno}", shoumipay_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 收米支付回调 金幣充值错误 {msg}", shoumipay_logFile);

                        Service.CDFailOrder(order.Id, tradeno, $"收米支付订单号:{orderid} , 渠道订单号: {tradeno} , 支付金额(分): {(Convert.ToDecimal(amount) ).ToString()}, 状态: {returncode}");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, $"{order.Id}  收米支付回调 支付無效 支付订单号:{orderid}, 渠道订单号:{"0"}, 支付金额(分):{(Convert.ToDecimal(amount) ).ToString()}, 状态:{returncode}", shoumipay_logFile);
                Service.CDFailOrder(order.Id, tradeno, $"收米支付订单号:{orderid} , 渠道订单号: {tradeno} , 支付金额(分): {(Convert.ToDecimal(amount) ).ToString()}, 状态: {returncode}");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("收米支付回调 异常错误 {0}", ex.ToString()), shoumipay_logFile);
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
            if (parameters.Count == 0)
            {
                Stream stream = HttpContext.Current.Request.InputStream;
                var b = new byte[stream.Length];
                stream.Read(b, 0, (int)stream.Length);
                var responseString = Encoding.UTF8.GetString(b);
                SiteHelper.Log(guid, $"收米支付回调串流 DATA={responseString}", shoumipay_logFile);

                JObject jsonstr = JObject.Parse(responseString);
                Dictionary<string, object> paras = jsonstr.ToObject<Dictionary<string, object>>();
                parameters = paras.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value.ToString());
            }

            var sign = parameters.ContainsKey("sign") ? parameters["sign"] : string.Empty;
            var merchantNo = parameters.ContainsKey("merchantNo") ? parameters["merchantNo"] : string.Empty;
            var no = parameters.ContainsKey("no") ? parameters["no"] : string.Empty;
            var nonce = parameters.ContainsKey("nonce") ? parameters["nonce"] : string.Empty;
            var timestamp= parameters.ContainsKey("timestamp") ? parameters["timestamp"] : string.Empty;


            var signdata = $"merchantNo={merchantNo}&no={no}&nonce={nonce}&timestamp={timestamp}&key={shoumipay_Key}";

            SiteHelper.Log(guid, $"回传加密资料 [data] {signdata}", shoumipay_logFile);


            var cSign = Md5(signdata, Encoding.UTF8).ToUpper();
            SiteHelper.Log(guid, $"收米支付回调签名 cSign={cSign} sign={sign}", shoumipay_logFile);
            return sign == cSign;
        }



    }
}