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
    public class Xunpay
    {
        public static string post(CDPayOrder order, string guid, string tradeCode, ref string message)
        {
            string url = string.Empty;

            Dictionary<string, string> paras = new Dictionary<string, string>
            {
                { "appId", Xunpay_appId },               
                { "orderNo", order.Id.ToString() },  
                { "timestamp", Convert.ToInt64(DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds * 1000).ToString() },
                { "money", order.Amount.ToString() },               
                { "noticeUrl", Xunpay_CallbackUrl }
            };
            
            string parasData = $"{paras["appId"]}^{paras["orderNo"]}^{paras["money"]}^{paras["timestamp"]}^{Xunpay_key}";
            var pay_md5sign = Md5(parasData, Encoding.UTF8).ToLower();
            paras.Add("sign", pay_md5sign);

            var data = string.Join("&", paras.OrderBy(x => x.Key).Select((m => $"{m.Key}={m.Value}")));
            SiteHelper.Log(guid, string.Format("{0} 支付请求 [data]({1}) {2}", order.Id, Xunpay_PayURL, data), Xunpay_logFile);          

            //呼叫支付平台
            var responseString = SiteHelper.HttpPost(Xunpay_PayURL, data);
            SiteHelper.Log(guid, string.Format("{0} 支付请求结果 [response] {1}", order.Id, responseString), Xunpay_logFile);

            JObject json = JObject.Parse(responseString);
            var result = json["result"] != null ? json["result"].ToString() : string.Empty;

            if (!result.Equals("success"))
            {
                var respmsg = json["msg"] != null ? json["msg"].ToString() : string.Empty;
                message = string.Format("{0} 支付请求失敗 {1}", order.Id, respmsg);

                //更新狀態
                Service.CDFailOrder(order.Id, string.Empty, respmsg);
                SiteHelper.Log(guid, message, Xunpay_logFile);
            }
            else
            {
                var orderNo = json["data"]["orderNo"] != null ? json["data"]["orderNo"].ToString() : string.Empty;

                Service.CDModifyTransID(order.Id, orderNo);
                url = json["data"]["qrCode"] != null ? json["data"]["qrCode"].ToString() : string.Empty;
            }

            return url;
        }

        public static PayState call(string logGuid)
        {
            try
            {             
                SiteHelper.Log(logGuid, string.Format("支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), Xunpay_logFile);
               
               
                Dictionary<string, string> parameters = null;

                if (!CheckSign(logGuid, out parameters))
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调非法签名"), Xunpay_logFile);
                    return PayState.SignError;
                }

                var appId = parameters.ContainsKey("appId") ? parameters["appId"] : string.Empty;
                var order_no = parameters.ContainsKey("order_no") ? parameters["order_no"] : string.Empty;
                var out_order_no = parameters.ContainsKey("out_order_no") ? parameters["out_order_no"] : string.Empty;
                var trade_money = parameters.ContainsKey("trade_money") ? parameters["trade_money"] : string.Empty;
                var trade_time = parameters.ContainsKey("trade_time") ? parameters["trade_time"] : string.Empty;
                var rebate = parameters.ContainsKey("rebate") ? parameters["rebate"] : string.Empty;
                var rebate_money = parameters.ContainsKey("rebate_money") ? parameters["rebate_money"] : string.Empty;
                var sign = parameters.ContainsKey("sign") ? parameters["sign"] : string.Empty;


                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(out_order_no));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 支付回调 訂單狀態已經完成,無須金幣充值", order.Id), Xunpay_logFile);
                    return PayState.Success;
                }                

                //進行金幣充值
                string msg = "";

                if ((Convert.ToDouble(trade_money) == order.Amount))
                {
                    var data = $"支付回调 完成支付: 支付订单号: {order_no} ";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, order_no, data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, string.Format("{0} 支付回调 金幣充值成功 支付订单号:{1} 金额:amount={2} ", order.Id, order_no, order.Amount), Xunpay_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, string.Format("{0} 支付回调 金幣充值错误 {1}", order.Id, msg), Xunpay_logFile);
                        Service.CDFailOrder(order.Id, order_no, $"支付订单号:{order_no} , 金额: {trade_money}");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, string.Format("{0}  支付回调 支付無效 支付订单号:{1}, 支付金额(:{2}", order.Id, order_no, trade_money), Xunpay_logFile);
                Service.CDFailOrder(order.Id, order_no, $"支付订单号:{order_no} , 金额: {trade_money}");

                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("支付回调 异常错误 {0}", ex.ToString()), Xunpay_logFile);
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
          
            var str = $"{parameters["appId"]}^{parameters["order_no"]}^{parameters["out_order_no"]}^{parameters["trade_time"]}^{Xunpay_key}";
            var cSign = Md5(str, Encoding.UTF8).ToLower();
            SiteHelper.Log(guid, string.Format("支付回调签名 cSign={0} sign={1} srt={2}", cSign, sign, str), Xunpay_logFile);

            return sign == cSign;
        }
    }
}