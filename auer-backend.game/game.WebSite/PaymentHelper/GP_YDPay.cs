using game.Entity.Pay;
using game.Infrastructure;
using Newtonsoft.Json;
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
    public class GP_YDPay
    {
        public class gpydResult
        {
            public string amount { get; set; }
            public string nonce_str { get; set; }
            public string qrCode { get; set; }
            public string sign { get; set; }
            public string orderNum { get; set; }
            public string orderStatus { get; set; }
            public string resp_Code { get; set; }
        }
        public class gpydErrorResult
        {         
            public string resp_ErrMsg { get; set; }
            public string resp_ErrCode { get; set; }
            public string resp_Code { get; set; }
        }

        /// <summary>
        /// 创建订单(跳转网页链接)
        /// </summary>
        /// <param name="order"></param>
        /// <param name="guid"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string post(CDPayOrder order, string guid, string tradeCode)
        {
      
            Dictionary<string, string> paras = new Dictionary<string, string>
            {
                { "merchantNum", gpyd_merchantNum },               
                { "orderNum", (gpyd_merMark + order.Id.ToString())},
                { "merMark", gpyd_merMark},
                { "payType", tradeCode},
                { "amount", (order.Amount*100).ToString()  },
                { "version", "V1.0"},
                { "nonce_str", "1234"},
                { "client_ip",order.IPAddress },
                { "orderTime", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")},
                { "signType", "MD5"},
                { "body", "Recharge"},
                { "notify_url", gpyd_shopNotifyUrl},
                { "return_url", gpyd_shopCallbackUrl},
            };
           

            var pay_md5sign = Md5($"version=V1.0&merchantNum={gpyd_merchantNum}&nonce_str=1234&merMark={gpyd_merMark}&client_ip={order.IPAddress}&payType={tradeCode}&orderNum={(gpyd_merMark + order.Id.ToString())}&amount={(order.Amount * 100).ToString()}&body=Recharge&key={gpyd__Key}", Encoding.UTF8);
            paras.Add("sign", pay_md5sign);

            var data = string.Join("&", paras.Select(m => string.Format("{0}={1}", m.Key, m.Value)));
            SiteHelper.Log(guid, $"{order.Id} 支付请求 [data]({gpyd_PayURL}) {data}", gpyd_logFile);
            var responseString = SiteHelper.HttpPost(gpyd_PayURL, data);
            SiteHelper.Log(guid, $"{order.Id} 支付请求结果 [response] {responseString}", gpyd_logFile);
           
            JObject res= JObject.Parse(responseString);
            if (res["resp_ErrCode"] == null)
            {
                var  result = JsonConvert.DeserializeObject<gpydResult>(responseString);
                if (!result.resp_Code.Equals("S") || !result.orderStatus.Equals("SUCCESS"))
                {
                    var message = $"支付请求失敗({result.orderNum}) {result.orderStatus}";
                    //更新狀態
                    Service.CDFailOrder(order.Id, string.Empty, message);
                    SiteHelper.Log(guid, message, gpyd_logFile);
                    return String.Empty;
                }
                else
                    return result.qrCode;

            }
            else
            {
                var result = JsonConvert.DeserializeObject<gpydErrorResult>(responseString);
                var message = $"支付请求失敗({result.resp_ErrCode}) {result.resp_ErrMsg}";
                //更新狀態
                Service.CDFailOrder(order.Id, string.Empty, message);
                SiteHelper.Log(guid, message, gpyd_logFile);
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
                    SiteHelper.Log(logGuid, string.Format("亿点支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), gpyd_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("亿点支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), gpyd_logFile);
                }
                Dictionary<string, string> parameters = null;

                if (!CheckSign(logGuid, out parameters))
                {
                    SiteHelper.Log(logGuid, string.Format("亿点支付回调非法签名"), gpyd_logFile);
                    return PayState.SignError;
                }

                //支付状态，0表示支付成功;
                var status = parameters.ContainsKey("orderStatus") ? parameters["orderStatus"] : string.Empty;

                //商家订单号 
                var orderNum = parameters.ContainsKey("orderNum") ? parameters["orderNum"] : string.Empty;
                
                var order_no = orderNum.Replace(gpyd_merMark, "");
             
                //订单金额
                var amount = parameters.ContainsKey("amount") ? parameters["amount"] : string.Empty;

                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(order_no));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 亿点支付回调 訂單狀態已經完成,無須金幣充值", order.Id), gpyd_logFile);
                    return PayState.Success;
                }

                //進行金幣充值
                string msg = "";

                if (status.Equals("SUCCESS") && ((Convert.ToInt64(amount) / 100) == order.Amount))
                {
                    var data = $"亿点支付回调 完成支付: 支付订单号: {order_no}  状态: {status}";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, "0", data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 亿点支付回调 金幣充值成功 支付订单号:{order_no} 金额:amount={order.Amount} 渠道订单号:0", gpyd_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 亿点支付回调 金幣充值错误 {msg}", gpyd_logFile);

                        Service.CDFailOrder(order.Id, "0", $"亿点支付订单号:{order_no} , 渠道订单号: {"0"} , 支付金额(分): {amount}, 状态: {status}");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, $"{order.Id}  亿点支付回调 支付無效 支付订单号:{order_no}, 渠道订单号:{"0"}, 支付金额(分):{amount}, 状态:{status}", gpyd_logFile);
                Service.CDFailOrder(order.Id, "0", $"亿点支付订单号:{order_no} , 渠道订单号: {"0"} , 支付金额(分): {amount}, 状态: {status}");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("亿点支付回调 异常错误 {0}", ex.ToString()), gpyd_logFile);
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

            var sign = parameters.ContainsKey("sign") ? parameters["sign"] : string.Empty;
            var amount = parameters.ContainsKey("amount") ? parameters["amount"] : string.Empty;
            var orderNum = parameters.ContainsKey("orderNum") ? parameters["orderNum"] : string.Empty; //商户订单号
            var nonce_str = parameters.ContainsKey("nonce_str") ? parameters["nonce_str"] : string.Empty; //随机码
            var orderStatus = parameters.ContainsKey("orderStatus") ? parameters["orderStatus"] : string.Empty; //随机码

            var cSign = Md5($"merchantNum={gpyd_merchantNum}&orderNum={orderNum}&amount={ amount.ToString()}&nonce_str={nonce_str.ToString()}&orderStatus={orderStatus}&key={gpyd__Key}", Encoding.UTF8);
            SiteHelper.Log(guid, $"亿点支付回调签名 cSign={cSign} sign={sign}", gpyd_logFile);
            return sign == cSign;
        }
    }
}