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
using example.invalid;

namespace game.WebSite.PaymentHelper
{
    public class Putao
    {
        public static string post(CDPayOrder order, string guid, string tradeCode, ref string message)
        {
            string url = string.Empty;

            Dictionary<string, string> paras = new Dictionary<string, string>
            {
                { "version", "1.0" },
                { "totalAmount", order.Amount.ToString() },
                { "merId", Putao_merId },
                { "orderNo", order.Id.ToString() },
                { "currency", "RMB" },
                { "payType", tradeCode },
                { "notifyUrl", Putao_NotifyUrl },
                { "returnUrl", Putao_CallbackUrl },
                { "subject", "charge" }
            };

            string parasData = string.Join("&", paras.OrderBy(x => x.Key).Select((m => $"{m.Key}={m.Value}"))) + "&key=" + Putao_Key;
            var pay_md5sign = Md5(parasData, Encoding.UTF8).ToUpper();
            paras.Add("signType", "MD5");
            paras.Add("signData", pay_md5sign);

            var data = string.Join("&", paras.OrderBy(x => x.Key).Select((m => $"{m.Key}={m.Value}")));
            SiteHelper.Log(guid, string.Format("{0} 支付请求 [data]({1}) {2}", order.Id, Putao_Url, data), Putao_logFile);

            //呼叫支付平台
            var responseString = SiteHelper.HttpPost(Putao_Url, data);
            SiteHelper.Log(guid, string.Format("{0} 支付请求结果 [response] {1}", order.Id, responseString), Putao_logFile);

            JObject json = JObject.Parse(responseString);
            var retCode = json["code"] != null ? json["code"].ToString() : string.Empty;

            if (!retCode.Equals("1"))
            {
                var respmsg = json["message"] != null ? json["message"].ToString() : string.Empty;
                message = string.Format("{0} 支付请求失敗 {1}", order.Id, respmsg);

                //更新狀態
                Service.CDFailOrder(order.Id, string.Empty, respmsg);
                SiteHelper.Log(guid, message, Putao_logFile);
            }
            else
            {             
                url = json["htmlUrl"] != null ? json["htmlUrl"].ToString() : string.Empty;
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
                    SiteHelper.Log(logGuid, string.Format("支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), Putao_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), Putao_logFile);
                }

                Dictionary<string, string> parameters = null;

                if (!CheckSign(logGuid, out parameters))
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调非法签名"), Putao_logFile);
                    return PayState.SignError;
                }

                var amount = parameters.ContainsKey("totalAmount") ? parameters["totalAmount"] : string.Empty;
                var transNo = parameters.ContainsKey("tradeNo") ? parameters["tradeNo"] : string.Empty;
                var status = parameters.ContainsKey("status") ? parameters["status"] : string.Empty;
                var orderNo = parameters.ContainsKey("merOrderNo") ? parameters["merOrderNo"] : string.Empty;        

                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(orderNo));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 支付回调 訂單狀態已經完成,無須金幣充值", order.Id), Putao_logFile);
                    return PayState.Success;
                }

                if (status != "TRADE_SUCCESS")
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 支付回调 订单生成: {1}", order.Id, transNo), Putao_logFile);
                    return PayState.Error;
                }

                //進行金幣充值
                string msg = "";

                if (status == "TRADE_SUCCESS" && (Convert.ToInt64(amount) == order.Amount))
                {
                    var data = $"支付回调 完成支付: 状态: {status}";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, transNo, data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, string.Format("{0} 支付回调 金幣充值成功 支付订单号:{1} 金额:{2}", order.Id, transNo, order.Amount), Putao_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, string.Format("{0} 支付回调 金幣充值错误 {1}", order.Id, msg), Putao_logFile);
                        Service.CDFailOrder(order.Id, transNo, $"支付金额: {amount}, 状态: {status}");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, string.Format("{0}  支付回调 支付無效 支付订单号:{1}, 支付金额:{2}, 状态:{3}", order.Id, transNo, amount, status), Putao_logFile);
                Service.CDFailOrder(order.Id, transNo, $"支付订单号:{transNo}, 支付金额: {amount}, 状态: {status}");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("支付回调 异常错误 {0}", ex.ToString()), Putao_logFile);
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

            var sign = parameters["signData"];
            parameters.Remove("signData");
            parameters.Remove("signType");
            parameters = parameters.OrderBy(o => o.Key).ToDictionary(o => o.Key, p => p.Value);

            var str = string.Join("&", parameters.Where(x => !string.IsNullOrEmpty(x.Value)).Select(m => string.Format("{0}={1}", m.Key, m.Value))) + "&key=" + Putao_Key;
            var cSign = Md5(str, Encoding.UTF8).ToUpper();
            SiteHelper.Log(guid, string.Format("支付回调签名 cSign={0} sign={1} srt={2}", cSign, sign, str), Putao_logFile);

            return sign == cSign;
        }
    }
}