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
    public class YAPay
    {
        public class YAResult
        {
            public string price { get; set; }
            public string real_price { get; set; }
            public string is_type { get; set; }
            public string order_id { get; set; }
            public string mark { get; set; }
            public string paysapi_id { get; set; }
            public string payurl { get; set; }
            public YAMsgResult messages { get; set; }
        }
        public class YAMsgResult
        {
            public string returncode { get; set; }
            public string returnmsg { get; set; }

        }

        /// <summary>
        /// 创建订单(跳转网页链接)
        /// </summary>
        /// <param name="order"></param>
        /// <param name="guid"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string post(CDPayOrder order, string guid, string tradeCode,ref bool isQrcodeBuild, ref string outMessage)
        {

            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            Dictionary<string, string> paras = new Dictionary<string, string>
            {              
                { "return_type", "json" },
                { "order_id", order.Id.ToString()},
                { "api_code", ya_code},
                { "is_type", tradeCode},
                { "mark", "Recharge"},
                { "time", unixTimestamp.ToString()},
                { "price", order.Amount.ToString()  },
                { "notify_url", ya_shopNotifyUrl},
                { "return_url", ya_shopCallBackUrl},
            };
           
            var pay_md5sign = Md5($"api_code={ya_code}&is_type={tradeCode}&mark=Recharge&notify_url={ya_shopNotifyUrl}&order_id={order.Id.ToString()}&price={order.Amount.ToString()}&return_type=json&return_url={ya_shopCallBackUrl}&time={unixTimestamp.ToString()}&key={ya_key}", Encoding.UTF8).ToUpper();
            paras.Add("sign", pay_md5sign);
            var data = string.Join("&", paras.Select(m => string.Format("{0}={1}", m.Key, m.Value)));

            SiteHelper.Log(guid, $"{order.Id} 支付请求 [data]({ya_PayURL}) {data}", ya_logFile);
            //     var responseString = SiteHelper.HttpPost(kft_PayURL, data);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ya_PayURL);
            request.UserAgent = "PostmanRuntime/7.13.0";
            request.Method = "POST";
            UTF8Encoding encoding = new UTF8Encoding();
            byte[] byte1 = encoding.GetBytes(data);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byte1.Length;
            Stream newStream = request.GetRequestStream();
            newStream.Write(byte1, 0, byte1.Length);
            newStream.Close();

            //发送成功后接收返回的XML信息
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string responseString = string.Empty;
            Encoding enc = Encoding.GetEncoding("UTF-8");
            Stream stream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(stream, enc);
            responseString = streamReader.ReadToEnd();

            SiteHelper.Log(guid, $"{order.Id} 支付请求结果 [response] {responseString}", ya_logFile);

            JObject res = JObject.Parse(responseString);
            if (res["payurl"] != null)
            {
                var result = JsonConvert.DeserializeObject<YAResult>(responseString);
             

                if (!result.messages.returncode.Equals("SUCCESS"))
                {
                     outMessage = $"支付请求失敗({result.order_id}) {result.messages.returncode}";
                    //更新狀態
                    Service.CDFailOrder(order.Id, string.Empty, outMessage);
                    SiteHelper.Log(guid, outMessage, ya_logFile);
                    return String.Empty;
                }
                else
                {
                    if (tradeCode == "wechatangel")
                        isQrcodeBuild = true;

                    Service.CDModifyTransID(order.Id, result.paysapi_id);
                    return result.payurl;
                }

            }
            else
            {
                var result = JsonConvert.DeserializeObject<YAMsgResult>(responseString);
                outMessage = $"支付请求失敗({result.returncode}) {result.returnmsg}";
                //更新狀態
                Service.CDFailOrder(order.Id, string.Empty, outMessage);
                SiteHelper.Log(guid, outMessage, ya_logFile);
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
                Stream stream = HttpContext.Current.Request.InputStream;
                var b = new byte[stream.Length];
                stream.Read(b, 0, (int)stream.Length);
                var responseString = Encoding.UTF8.GetString(b);
                SiteHelper.Log(logGuid, string.Format("支付回调 [FormData] {0}", responseString), ya_logFile);

                JObject json = JObject.Parse(responseString);
                //var requestType = HttpContext.Current.Request.RequestType;
                //if (requestType == "GET")
                //{
                //    SiteHelper.Log(logGuid, string.Format("永安支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), ya_logFile);
                //}
                //else
                //{
                //    SiteHelper.Log(logGuid, string.Format("永安支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), ya_logFile);
                //}
               
                if (!CheckSign(logGuid, json))
                {
                    SiteHelper.Log(logGuid, string.Format("永安支付回调非法签名"), ya_logFile);
                    return PayState.SignError;
                }
                
                //支付状态，0表示支付成功;
                var status = json["code"] != null ? json["code"].ToString() : string.Empty;

                //商家订单号 
                var out_trade_no = json["order_id"] != null ? json["order_id"].ToString() : string.Empty;

                //支付平台流水号，同trade_no 一样
                var trade_no = json["paysapi_id"] != null ? json["paysapi_id"].ToString() : string.Empty;

                //订单金额
                var amount = json["price"] != null ? json["price"].ToString() : string.Empty;

                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(out_trade_no));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 永安支付回调 訂單狀態已經完成,無須金幣充值", order.Id), ya_logFile);
                    return PayState.Success;
                }

                //進行金幣充值
                string msg = "";

                if (status.Equals("1") && ((Convert.ToDecimal(amount)) == order.Amount))
                {
                    var data = $"永安支付回调 完成支付: 支付订单号: {out_trade_no} , 渠道订单号: {trade_no}, 状态: {status}";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, trade_no, data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 永安支付回调 金幣充值成功 支付订单号:{out_trade_no} 金额:amount={order.Amount} 渠道订单号:{trade_no}", ya_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 永安支付回调 金幣充值错误 {msg}", ya_logFile);

                        Service.CDFailOrder(order.Id, trade_no, $"永安支付订单号:{out_trade_no} , 渠道订单号: {trade_no} , 支付金额(分): {amount}, 状态: {status}");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, $"{order.Id}  永安支付回调 支付無效 支付订单号:{out_trade_no}, 渠道订单号:{trade_no}, 支付金额(分):{amount}, 状态:{status}", ya_logFile);
                Service.CDFailOrder(order.Id, trade_no, $"永安支付订单号:{out_trade_no} , 渠道订单号: {trade_no} , 支付金额(分): {amount}, 状态: {status}");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("永安支付回调 异常错误 {0}", ex.ToString()), ya_logFile);
                return PayState.Error;
            }
        }


        private static string Md5(string str, Encoding ec)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            return BitConverter.ToString(md5.ComputeHash(ec.GetBytes(str))).Replace("-", "");
        }

        private static bool CheckSign(string guid, JObject j)
        {

            
            var sign = j["sign"] != null ? j["sign"].ToString() : string.Empty;
            
            var order_id = j["order_id"] != null ? j["order_id"].ToString() : string.Empty; //商户订单号
            var paysapi_id = j["paysapi_id"] != null ? j["paysapi_id"].ToString() : string.Empty;//平台交易订单号 
            var is_type = j["is_type"] != null ? j["is_type"].ToString() : string.Empty;
            var price = j["price"] != null ? j["price"].ToString() : string.Empty;
            var real_price = j["real_price"] != null ? j["real_price"].ToString() : string.Empty;
            var mark = j["mark"] != null ? j["mark"].ToString() : string.Empty;
            var code = j["code"] != null ? j["code"].ToString() : string.Empty;
            var cSign = Md5($"api_code={ya_code}&code={code}&is_type={is_type}&mark={mark}&order_id={order_id}&paysapi_id={paysapi_id}&price={price}&real_price={real_price}&key={ya_key}", Encoding.UTF8).ToUpper();

            SiteHelper.Log(guid, $"永安支付回调签名 cSign={cSign} sign={sign}", ya_logFile);
            return sign == cSign;
        }
    }
}