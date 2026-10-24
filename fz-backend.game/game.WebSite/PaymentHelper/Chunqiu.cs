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
    public class Chunqiu
    {
        public static string post(CDPayOrder order, string guid, string tradeCode, ref string message)
        {
            string url = string.Empty;

            Dictionary<string, string> paras = new Dictionary<string, string>
            {
                { "amount", (order.Amount*100).ToString() },
                { "appid", Chunqiu_appId },
                { "callbackurl", Chunqiu_CallbackUrl },
                { "notifyurl", Chunqiu_NotifyUrl },
                { "orderno", order.Id.ToString() },
                { "type", tradeCode },
            };

            string parasData = string.Join("&", paras.Select((m => $"{m.Key}={m.Value}"))) + Chunqiu_key;
            var pay_md5sign = Md5(parasData, Encoding.UTF8).ToUpper();          
            paras.Add("sign", pay_md5sign);          

            url = $"<html><body><form method='post' name='form' action='{Chunqiu_PayURL}' enctype=\"application/x-www-form-urlencoded\">" +
                    string.Join("", paras.Select(m => string.Format("<input type='hidden' name='{0}' value='{1}' >", m.Key, m.Value))) +
                    "</form><script>window.onload = function(){document.forms['form'].submit();}</script></body></html>";

            SiteHelper.Log(guid, $"{order.Id} 支付请求 [data]({url}) ", Chunqiu_logFile);
       
            var api = Service.CreateRedirectData((int)PayGatewayType.Chunqiu, url);

            if (api.Result)
                return api.Extend.ToString();
            else
                return string.Empty;
        }

        public static PayState call(string logGuid)
        {
            try
            {
                SiteHelper.Log(logGuid, string.Format("支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), Chunqiu_logFile);


                Dictionary<string, string> parameters = null;

                if (!CheckSign(logGuid, out parameters))
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调非法签名"), Chunqiu_logFile);
                    return PayState.SignError;
                }

                var order_no = parameters.ContainsKey("merchant_order_sn") ? parameters["merchant_order_sn"] : string.Empty;
                var transID = parameters.ContainsKey("trade_no") ? parameters["trade_no"] : string.Empty;
                var trade_money = parameters.ContainsKey("total_fee") ? parameters["total_fee"] : string.Empty;
                var status = parameters.ContainsKey("result_code") ? parameters["result_code"] : string.Empty;
                var transData = parameters.ContainsKey("result_message") ? parameters["result_message"] : string.Empty;

                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(order_no));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 支付回调 訂單狀態已經完成,無須金幣充值", order.Id), Chunqiu_logFile);
                    return PayState.Success;
                }

                //支付失败
                if (status != "200")
                {
                    SiteHelper.Log(logGuid, string.Format("支付失败"), Chunqiu_logFile);
                    Service.CDFailOrder(order.Id, transID, $"支付失败 : {status} {transData}");
                    return PayState.Error;
                }

                //進行金幣充值
                string msg = "";

                if (status == "200" && (Convert.ToDouble(trade_money) == order.Amount))
                {
                    var data = $"支付回调 完成支付: {transData} ";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, transID, data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, string.Format("{0} 支付回调 金幣充值成功 支付订单号:{1} 金额:{2} ", order.Id, transID, order.Amount), Chunqiu_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, string.Format("{0} 支付回调 金幣充值错误 {1}", order.Id, msg), Chunqiu_logFile);
                        Service.CDFailOrder(order.Id, transID, $"支付订单号:{transID} , 金额: {trade_money}");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, string.Format("{0}  支付回调 支付無效 支付订单号:{1}, 支付金额(:{2}", order.Id, transID, trade_money), Chunqiu_logFile);
                Service.CDFailOrder(order.Id, transID, $"支付订单号:{transID} , 金额: {trade_money}");

                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("支付回调 异常错误 {0}", ex.ToString()), Chunqiu_logFile);
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
            parameters.Remove("sign");

            var str = string.Join("&", parameters.OrderBy(x=>x.Key).Select((m => $"{m.Key}={m.Value}"))) + Chunqiu_key;
            var cSign = Md5(str, Encoding.UTF8).ToUpper();
            SiteHelper.Log(guid, string.Format("支付回调签名 cSign={0} sign={1} srt={2}", cSign, sign, str), Chunqiu_logFile);

            return sign == cSign;
        }
    }
}