using game.Entity.Pay;
using game.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using static game.WebSite.PayConfig;

namespace game.WebSite.PaymentHelper
{
    public class TMAllPay
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
        
            Dictionary<string, string> paras = new Dictionary<string, string>
            {
                { "app_id", tmallpay_memberid  },
                { "amount", (order.Amount*100).ToString()  },
                { "order_no", order.Id.ToString("D6")},
                { "device", tradeCode  },
                { "notify_url", tmallpay_shopNotifyUrl  },
                { "return_url", tmallpay_shopCallBackUrl  },

            };
            var sign = Md5($"app_id={tmallpay_memberid}&amount={(order.Amount * 100).ToString()}&order_no={order.Id.ToString("D6")}&device={tradeCode}&app_secret=[REDACTED]", Encoding.UTF8).ToLower();
            paras.Add("sign", sign);
            var data = string.Join("&", paras.Select(m => string.Format("{0}={1}", m.Key, m.Value)));
            var url = $"{tmallpay_PayURL}?{data}";
            SiteHelper.Log(guid, $"{order.Id} 支付请求 [data]({url}) ", tmallpay_logFile);
           
            
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
                    SiteHelper.Log(logGuid, string.Format("天猫支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), tmallpay_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("天猫支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), tmallpay_logFile);
                }
                Dictionary<string, string> parameters = null;

                if (!CheckSign(logGuid, out parameters))
                {
                    SiteHelper.Log(logGuid, string.Format("天猫支付回调非法签名"), tmallpay_logFile);
                    return PayState.SignError;
                }



                //支付状态，00表示支付成功;
                var returncode = parameters.ContainsKey("order_status") ? parameters["order_status"] : string.Empty;

                //商家订单号 
                var orderid = parameters.ContainsKey("order_no") ? parameters["order_no"] : string.Empty;

                //订单金额
                var amount = parameters.ContainsKey("amount") ? parameters["amount"] : string.Empty;

                var trxNo = "0";

                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(orderid));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 天猫支付回调 訂單狀態已經完成,無須金幣充值", order.Id), tmallpay_logFile);
                    return PayState.Success;
                }

                //進行金幣充值
                string msg = "";

                if (returncode.Equals("success") && ((Convert.ToDecimal(amount)/100) == order.Amount))
                {
                    var data = $"天猫支付回调 完成支付: 支付订单号: {orderid} , 渠道订单号: {trxNo}, 状态: {returncode}";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, trxNo, data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 天猫支付回调 金幣充值成功 支付订单号:{orderid} 金额:amount={order.Amount} 渠道订单号:{trxNo}", tmallpay_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 天猫支付回调 金幣充值错误 {msg}", tmallpay_logFile);

                        Service.CDFailOrder(order.Id, trxNo, $"天猫支付订单号:{orderid} , 渠道订单号: {trxNo} , 支付金额(分): {(Convert.ToDecimal(amount) ).ToString()}, 状态: {returncode}");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, $"{order.Id}  天猫支付回调 支付無效 支付订单号:{orderid}, 渠道订单号:{trxNo}, 支付金额(分):{(Convert.ToDecimal(amount)).ToString()}, 状态:{returncode}", tmallpay_logFile);
                Service.CDFailOrder(order.Id, trxNo, $"天猫支付订单号:{orderid} , 渠道订单号: {trxNo} , 支付金额(分): {(Convert.ToDecimal(amount)).ToString()}, 状态: {returncode}");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("天猫支付回调 异常错误 {0}", ex.ToString()), tmallpay_logFile);
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

            var status = parameters.ContainsKey("status") ? parameters["status"] : string.Empty;
            var amount = parameters.ContainsKey("amount") ? parameters["amount"] : string.Empty;
            var order_no = parameters.ContainsKey("order_no") ? parameters["order_no"] : string.Empty;
            var order_status = parameters.ContainsKey("order_status") ? parameters["order_status"] : string.Empty;
            var pay_time = parameters.ContainsKey("pay_time") ? parameters["pay_time"] : string.Empty;
            var sign = parameters.ContainsKey("sign") ? parameters["sign"] : string.Empty;




            var data = $"status={status}&amount={amount}&order_no={order_no}&order_status={order_status}&pay_time={pay_time}&app_secret=[REDACTED]";
            var cSign = Md5($"status={status}&amount={amount}&order_no={order_no}&order_status={order_status}&pay_time={pay_time}&app_secret=[REDACTED]", Encoding.UTF8).ToLower();
            SiteHelper.Log(guid, $"天猫支付回调签名 data={data} cSign={cSign} sign={sign}", tmallpay_logFile);
            return sign == cSign;




        }
    }
}