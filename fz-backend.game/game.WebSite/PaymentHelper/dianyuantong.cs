using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using game.Entity.Pay;
using game.Infrastructure;
using static game.WebSite.PayConfig;
using example.invalid;
using example.invalid;

namespace game.WebSite.PaymentHelper
{
    public class dianyuantong
    {

        /// <summary>
        /// 创建订单(跳转网页链接)
        /// </summary>
        /// <param name="order"></param>
        /// <param name="guid"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string post(CDPayOrder order, string guid)
        {
            Dictionary<string, string> paras = new Dictionary<string, string>
            {
                { "shopAccountId", dianyuantong_mchid },
                { "shopUserId", order.UserID.ToString() },
                { "amountInString", order.Amount.ToString()  },
                { "payChannel", "alipay" },
                { "shopNo", order.Id.ToString()},
                { "shopCallbackUrl", dianyuantong_shopCallbackUrl},
                { "returnUrl", dianyuantong_shopCallbackUrl},
                { "target", "1"},
            };

            var pay_md5sign = Md5($"{dianyuantong_mchid}&{order.UserID.ToString()}&{order.Amount.ToString()}&{order.Id.ToString()}&alipay&{dianyuantong_Key}", Encoding.UTF8).ToLower();
            paras.Add("sign", pay_md5sign);

            var data = string.Join("&", paras.Select(m => string.Format("{0}={1}", m.Key, m.Value)));
            SiteHelper.Log(guid, $"{order.Id} 支付请求 [data]({dianyuantong_PayURL}) {data}", dianyuantong_logFile);
         

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(dianyuantong_PayURL);
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
            string lcHtml = string.Empty;
            Encoding enc = Encoding.GetEncoding("UTF-8");
            Stream stream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(stream, enc);
            lcHtml = streamReader.ReadToEnd();

            if (dianyuantong_PayURL == response.ResponseUri.ToString())
            {
                Service.CDFailOrder(order.Id, null, lcHtml);
                SiteHelper.Log(guid, string.Format("{0} 支付请求结果 [response] {1}", order.Id, lcHtml), dianyuantong_logFile);

                return string.Empty; ;
            }

            SiteHelper.Log(guid, string.Format("{0} 支付请求结果 [response] {1}", order.Id, response.ResponseUri.ToString()), dianyuantong_logFile);
            return response.ResponseUri.ToString();
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
                    SiteHelper.Log(logGuid, string.Format("店員通支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), dianyuantong_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("店員通支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), dianyuantong_logFile);
                }
                Dictionary<string, string> parameters = null;

                if (!CheckSign(logGuid, out parameters))
                {
                    SiteHelper.Log(logGuid, string.Format("店員通支付回调非法签名"), dianyuantong_logFile);
                    return PayState.SignError;
                }

                //支付状态，0表示支付成功;
                var status = parameters.ContainsKey("status") ? parameters["status"] : string.Empty;

                //商家订单号 
                var shop_no = parameters.ContainsKey("shop_no") ? parameters["shop_no"] : string.Empty;

                //支付平台流水号，同trade_no 一样
                var order_no = parameters.ContainsKey("order_no") ? parameters["order_no"] : string.Empty;
                var trade_no = parameters.ContainsKey("trade_no") ? parameters["trade_no"] : string.Empty;

                //订单金额
                var money = parameters.ContainsKey("money") ? parameters["money"] : string.Empty;

                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(shop_no));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 店员通支付回调 訂單狀態已經完成,無須金幣充值", order.Id), dianyuantong_logFile);
                    return PayState.Success;
                }

                //進行金幣充值
                string msg = "";

                if (status.Equals("0") && (Convert.ToInt64(money) == order.Amount))
                {
                    var data = $"店员通支付回调 完成支付: 支付订单号: {shop_no} , 渠道订单号: {trade_no}, 状态: {status}";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, trade_no, data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 店员通支付回调 金幣充值成功 支付订单号:{shop_no} 金额:amount={order.Amount} 渠道订单号:{trade_no}", dianyuantong_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 店员通支付回调 金幣充值错误 {msg}", dianyuantong_logFile);

                        Service.CDFailOrder(order.Id, shop_no, $"店员通支付订单号:{shop_no} , 渠道订单号: {trade_no} , 支付金额(分): {money}, 状态: {status}");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, $"{order.Id}  店员通支付回调 支付無效 支付订单号:{shop_no}, 渠道订单号:{trade_no}, 支付金额(分):{money}, 状态:{status}", dianyuantong_logFile);
                Service.CDFailOrder(order.Id, shop_no, $"店员通支付订单号:{shop_no} , 渠道订单号: {trade_no} , 支付金额(分): {money}, 状态: {status}");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("店员通支付回调 异常错误 {0}", ex.ToString()), dianyuantong_logFile);
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
            var shopAccountId = parameters.ContainsKey("shopAccountId") ? parameters["shopAccountId"] : string.Empty;
            var user_id = parameters.ContainsKey("user_id") ? parameters["user_id"] : string.Empty;
            var trade_no = parameters.ContainsKey("trade_no") ? parameters["trade_no"] : string.Empty;
            var money = parameters.ContainsKey("money") ? parameters["money"] : string.Empty;
            var type = parameters.ContainsKey("type") ? parameters["type"] : string.Empty;
            var cSign = Md5($"{shopAccountId}&{user_id}&{trade_no}&{dianyuantong_Key}&{money}&{type}", Encoding.UTF8).ToLower();

            SiteHelper.Log(guid, $"店員通支付回调签名 cSign={cSign} sign={sign}", dianyuantong_logFile);
            return sign == cSign;
        }
    }
}