using game.Entity.Pay;
using game.Infrastructure;
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
    public class BitexPay
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
            string d = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Dictionary<string, string> paras = new Dictionary<string, string>
            {
                { "orderPrice", (order.Amount).ToString()  },
                { "payWayCode", tradeCode  },
                { "rechargeOrWithdraw", "1"  },
                { "userId", order.UserID.ToString()  },
                { "orderNo", order.Id.ToString()},
                { "productName", "湾仔火锅组"  },
                { "returnUrl", ""  },
                { "notifyUrl", bitexpay_shopNotifyUrl  },
                { "payKey", bitexpay_Key },               
            };
            string parasData = string.Join("&", paras.OrderBy(x => x.Key).Where(x=>!string.IsNullOrEmpty(x.Value)).Select((m => $"{m.Key}={m.Value}")));
            var sign = Md5(parasData + "&paySecret=" + bitexpay_Secret, Encoding.UTF8).ToUpper();
            paras.Add("sign", sign);


            var url = $"<html><body><form method='post' name='form' action='{bitexpay_PayURL}'>" +
                    string.Join("", paras.Select(m => string.Format("<input type='hidden' name='{0}' value='{1}' >", m.Key, m.Value))) +
                    "</form><script>window.onload = function(){document.forms['form'].submit();}</script></body></html>";
            SiteHelper.Log(guid, $"{order.Id} 支付请求 [data] {url}", bitexpay_logFile);

            var api = Service.CreateRedirectData((int)PayGatewayType.BitexPay, url);

            if (api.Result)
                return api.Extend.ToString();
            else
                return string.Empty;
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
                    SiteHelper.Log(logGuid, string.Format("火星支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), bitexpay_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("火星支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), bitexpay_logFile);
                }
                Dictionary<string, string> parameters = null;

                if (!CheckSign(logGuid, out parameters))
                {
                    SiteHelper.Log(logGuid, string.Format("火星支付回调非法签名"), bitexpay_logFile);
                    return PayState.SignError;
                }


              
                //支付状态，00表示支付成功;
                var returncode = parameters.ContainsKey("statusCode") ? parameters["statusCode"] : string.Empty;

                //商家订单号 
                var orderid = parameters.ContainsKey("orderNo") ? parameters["orderNo"] : string.Empty;

                //订单金额
                var amount = parameters.ContainsKey("orderPrice") ? parameters["orderPrice"] : string.Empty;

                var trxNo = parameters.ContainsKey("trxNo") ? parameters["trxNo"] : string.Empty;

                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(orderid));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 火星支付回调 訂單狀態已經完成,無須金幣充值", order.Id), bitexpay_logFile);
                    return PayState.Success;
                }

                //進行金幣充值
                string msg = "";

                if (returncode.Equals("00") && ((Convert.ToDecimal(amount)) == order.Amount))
                {
                    var data = $"火星支付回调 完成支付: 支付订单号: {orderid} , 渠道订单号: {trxNo}, 状态: {returncode}";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, trxNo, data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 火星支付回调 金幣充值成功 支付订单号:{orderid} 金额:amount={order.Amount} 渠道订单号:{trxNo}", bitexpay_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 火星支付回调 金幣充值错误 {msg}", bitexpay_logFile);

                        Service.CDFailOrder(order.Id, trxNo, $"火星支付订单号:{orderid} , 渠道订单号: {trxNo} , 支付金额(分): {(Convert.ToInt64(amount) * 100).ToString()}, 状态: {returncode}");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, $"{order.Id}  火星支付回调 支付無效 支付订单号:{orderid}, 渠道订单号:{trxNo}, 支付金额(分):{(Convert.ToInt64(amount) * 100).ToString()}, 状态:{returncode}", bitexpay_logFile);
                Service.CDFailOrder(order.Id, trxNo, $"火星支付订单号:{orderid} , 渠道订单号: {trxNo} , 支付金额(分): {(Convert.ToInt64(amount) * 100).ToString()}, 状态: {returncode}");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("火星支付回调 异常错误 {0}", ex.ToString()), bitexpay_logFile);
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
                parameters = HttpContext.Current.Request.QueryString.AllKeys.Where(x=>!string.IsNullOrEmpty(x)).Cast<string>()
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
                SiteHelper.Log(guid, $"火星支付回调串流 DATA={responseString}", bitexpay_logFile);
                JObject json = JObject.Parse(responseString);
            }
            var sign = parameters.ContainsKey("sign") ? parameters["sign"] : string.Empty;
        
        
            parameters.Remove("sign");


            var cSign = Md5(string.Join("&", parameters.Where(x=>!string.IsNullOrEmpty( x.Value)).Select((m => $"{m.Key}={m.Value}"))) + "&paySecret=" + bitexpay_Secret, Encoding.UTF8).ToUpper();
            SiteHelper.Log(guid, $"火星支付回调签名 cSign={cSign} sign={sign}", bitexpay_logFile);
            return sign == cSign;




        }
    }
}