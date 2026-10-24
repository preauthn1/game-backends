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
    public class WangyuaPay
    {
       // err_code 、 err_msg 、 data
       // err_code 为0 是表示成功；
       //data 含有： qrcode 、 timeExpire 两个参数
        public class WGResult
        {
            public string err_msg { get; set; }
            public string err_code { get; set; }
            public WGDataResult data { get; set; }
        }
        public class WGDataResult
        {
            public string timeExpire { get; set; }
            public string qrcode { get; set; }

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

            Dictionary<string, string> paras = new Dictionary<string, string>
            {
                { "order_no", order.Id.ToString()},
                { "amount", order.Amount.ToString()  },
                { "subject", "3C产品"},
                { "paytype","1"},
                { "mch_id", wangyuapay_mchid},               
                { "appSence","1002" },
                { "return_url", wangyuapay_shopCallBackUrl},
                { "order_time", DateTime.Now.ToString("yyyyMMddHHmmss")},
                
            };

            var sign = Md5($"{order.Id.ToString()}{order.Amount.ToString()}{wangyuapay_Key}{wangyuapay_mchid}", Encoding.UTF8).ToUpper();
            paras.Add("sign", sign);

            var data = string.Join("&", paras.Select(m => string.Format("{0}={1}", m.Key, m.Value)));
            SiteHelper.Log(guid, $"{order.Id} 支付请求 [data]({wangyuapay_PayURL}) {data}", wangyuapay_logFile);
            string responseString = SiteHelper.HttpPost(wangyuapay_PayURL, data);
            SiteHelper.Log(guid, $"{order.Id} 支付请求 [data] {responseString}", wangyuapay_logFile);
            var result = JsonConvert.DeserializeObject<WGResult>(responseString);

            if (result.err_code == "0")
            {
                return result.data.qrcode;
            }
            else
            {
                return string.Empty;
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
                    SiteHelper.Log(logGuid, string.Format("万伽支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), wangyuapay_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("万伽支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), wangyuapay_logFile);
                }
                Dictionary<string, string> parameters = null;

                if (!CheckSign(logGuid, out parameters))
                {
                    SiteHelper.Log(logGuid, string.Format("万伽支付回调非法签名"), wangyuapay_logFile);
                    return PayState.SignError;
                }

                //支付状态;
                var orderstatus = "0";

                //商家订单号 
                var orderid = parameters.ContainsKey("order_no") ? parameters["order_no"] : string.Empty;
                //第三方订单号 
                var tradeid = parameters.ContainsKey("tran_id") ? parameters["tran_id"] : string.Empty;
                //订单金额
                var amount = parameters.ContainsKey("amount") ? parameters["amount"] : string.Empty;

                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(orderid));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 万伽支付回调 訂單狀態已經完成,無須金幣充值", order.Id), wangyuapay_logFile);
                    return PayState.Success;
                }

                //進行金幣充值
                string msg = "";

                if (orderstatus.Equals("0") && ((Convert.ToDecimal(amount) ) == order.Amount))
                {
                    var data = $"万伽支付回调 完成支付: 支付订单号: {orderid} , 渠道订单号: {tradeid}, 状态: {orderstatus}";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, tradeid, data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 万伽支付回调 金幣充值成功 支付订单号:{orderid} 金额:amount={order.Amount} 渠道订单号:{tradeid}", wangyuapay_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 万伽支付回调 金幣充值错误 {msg}", wangyuapay_logFile);

                        Service.CDFailOrder(order.Id, tradeid, $"万伽支付订单号:{orderid} , 渠道订单号: {tradeid} , 支付金额(分): {(Convert.ToInt64(amount) * 100).ToString()}, 状态: {orderstatus}");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, $"{order.Id}  万伽支付回调 支付無效 支付订单号:{orderid}, 渠道订单号:{tradeid}, 支付金额(分):{(Convert.ToInt64(amount) * 100).ToString()}, 状态:{orderstatus}", wangyuapay_logFile);
                Service.CDFailOrder(order.Id, "0", $"万伽支付订单号:{orderid} , 渠道订单号: {tradeid} , 支付金额(分): {(Convert.ToInt64(amount) * 100).ToString()}, 状态: {orderstatus}");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("万伽支付回调 异常错误 {0}", ex.ToString()), wangyuapay_logFile);
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

            var order_no = parameters.ContainsKey("order_no") ? parameters["order_no"] : string.Empty;
            var amount = parameters.ContainsKey("amount") ? parameters["amount"] : string.Empty;
            var tran_id = parameters.ContainsKey("tran_id") ? parameters["tran_id"] : string.Empty;
            var sign = parameters.ContainsKey("sign") ? parameters["sign"] : string.Empty;

            var cSign = Md5($"{order_no}{amount}{tran_id}{wangyuapay_Key}", Encoding.UTF8).ToLower();
            SiteHelper.Log(guid, $"万伽支付回调签名 cSign={cSign} sign={sign}", wangyuapay_logFile);
            return sign == cSign;
        }
    }
}