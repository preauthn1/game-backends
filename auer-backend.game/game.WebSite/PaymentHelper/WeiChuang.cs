using game.Entity.Pay;
using game.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using static game.WebSite.PayConfig;
using Newtonsoft.Json;

namespace game.WebSite.PaymentHelper
{
    public class WeiChuang
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

            Dictionary<string, string> paras = new Dictionary<string, string>
            {
                { "version", "1.0" },
                { "app_id", WeiChuang_AppID},
                { "amount", (order.Amount*100).ToString() },                //支付金额 单位为分
                { "order_no", order.Id.ToString()},
                { "notify_url", WeiChuang_NotifyUrl },
                { "return_url", WeiChuang_CallbackUrl }
            };

            string signString = string.Join("&", paras.Select((kvp) => kvp.Key + "=" + kvp.Value.ToString())) + "&" + WeiChuang_Key;
            var sign = Md5(signString, Encoding.UTF8).ToLower();

            paras.Add("data_type", "text");          //请求获取的网页类型，json 返回json数据，text直接跳转html界面支付，如没有特殊需要，建议默认text即可
            paras.Add("channel", tradeCode);         //支付通道：支付宝：alipay、微信：wechat
            paras.Add("sign", sign);
            paras.Add("pkid", WeiChuang_PKID);

            var url = $"<html><body><form method='post' name='form' action='{WeiChuang_Url}' enctype=\"application/x-www-form-urlencoded\">" +
                    string.Join("", paras.Select(m => string.Format("<input type='hidden' name='{0}' value='{1}' >", m.Key, m.Value))) +
                    "</form><script>window.onload = function(){document.forms['form'].submit();}</script></body></html>";

            SiteHelper.Log(guid, $"{order.Id} 唯创支付请求 [data]({url}) ", WeiChuang_logFile);

            var api = Service.CreateRedirectData((int)PayGatewayType.WeiChuang, url);

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
                    SiteHelper.Log(logGuid, string.Format("唯创支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), WeiChuang_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("唯创支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), WeiChuang_logFile);
                }
                Dictionary<string, string> parameters = null;

                if (!CheckSign(logGuid, out parameters))
                {
                    SiteHelper.Log(logGuid, string.Format("唯创支付回调非法签名"), WeiChuang_logFile);
                    return PayState.SignError;
                }

                var app_id = parameters.ContainsKey("app_id") ? parameters["app_id"] : string.Empty;                  //app_id
                var pay_time = parameters.ContainsKey("pay_time") ? parameters["pay_time"] : string.Empty;            //支付时间
                var status = parameters.ContainsKey("status") ? parameters["status"] : string.Empty;            //支付状态(success为成功,其它为失败)
                var amount = parameters.ContainsKey("amount") ? parameters["amount"] : string.Empty;                        //订单金额
                var order_no = parameters.ContainsKey("order_no") ? parameters["order_no"] : string.Empty;                  //商户订单号
                var trade_no = parameters.ContainsKey("trade_no") ? parameters["trade_no"] : string.Empty;            //平台订单号
                var sign = parameters.ContainsKey("sign") ? parameters["sign"] : string.Empty;                              //签名

                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(order_no));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 唯创支付回调 訂單狀態已經完成,無須金幣充值", order.Id), WeiChuang_logFile);
                    return PayState.Success;
                }

                //進行金幣充值
                string msg = "";

                if (status.ToLower().Equals("success") && (Convert.ToInt64(amount) == order.Amount * 100))
                {
                    var data = $"唯创支付回调 完成支付: 支付订单号: {order_no} , 渠道订单号: {trade_no}, 状态: success";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, trade_no, data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 唯创支付回调 金幣充值成功 支付订单号:{order_no} 金额(分):{order.Amount} 渠道订单号:{trade_no}", WeiChuang_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 唯创支付回调 金幣充值错误 {msg}", WeiChuang_logFile);

                        Service.CDFailOrder(order.Id, trade_no, $"唯创支付订单号:{order_no} , 渠道订单号: {trade_no} , 支付金额(分): {amount}, 状态: fail");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, $"{order.Id}  唯创支付回调 支付無效 支付订单号:{order_no}, 渠道订单号:{trade_no}, 支付金额(分):{amount}, 状态: fail", WeiChuang_logFile);
                Service.CDFailOrder(order.Id, "0", $"唯创支付订单号:{order_no} , 渠道订单号: {trade_no} , 支付金额(分): {amount}, 状态: fail");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("唯创支付回调 异常错误 {0}", ex.ToString()), WeiChuang_logFile);
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

            var app_id = parameters.ContainsKey("app_id") ? parameters["app_id"] : string.Empty;                  //app_id
            var amount = parameters.ContainsKey("amount") ? parameters["amount"] : string.Empty;                        //订单金额
            var order_no = parameters.ContainsKey("order_no") ? parameters["order_no"] : string.Empty;                  //商户订单号
            var sign = parameters.ContainsKey("sign") ? parameters["sign"] : string.Empty;                              //签名

            var data = $"version=1.0" +
                $"&app_id={app_id}" +
                $"&amount={amount}" +
                $"&order_no={order_no}" +
                $"&" + WeiChuang_Key;
            var cSign = Md5(data, Encoding.UTF8).ToLower();
            SiteHelper.Log(guid, $"唯创支付回调签名 data={data} cSign={cSign} sign={sign}", WeiChuang_logFile);
            return sign == cSign;
        }
    }
}
