using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using game.Entity.Pay;
using game.Infrastructure;
using static game.WebSite.PayConfig;
using Newtonsoft.Json.Linq;

namespace game.WebSite.PaymentHelper
{
    public class Qingting
    {
        public static string post(CDPayOrder order, string guid, string tradeCode, ref string message)
        {
            string url = string.Empty;
            string mchid = order.GWID == (int)PayGatewayType.Qingting ? Qingting_mchid : QingtingQe_mchid;
            string key = order.GWID == (int)PayGatewayType.Qingting ? Qingting_Key : QingtingQe_Key;

            Dictionary<string, string> paras = new Dictionary<string, string>
            {
                { "agent_code", mchid },
                { "order_no", order.Id.ToString() },
                { "pay_type", tradeCode.Substring(0,1)},
                { "pay_money", order.Amount.ToString() },
                { "notify_url", $"{Qingting_CallbackUrl}/{order.GWID}" },
                { "back_url", string.Empty },
                { "client_ip", order.IPAddress },
                { "api_h5", tradeCode.Substring(1,1) },
                { "timestamp", Convert.ToInt64(DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds * 1000).ToString() },
                { "game_account", order.UserID.ToString() },
                { "key", key }
            };

            string parasData = string.Join("&", paras.Select((m => $"{m.Key}={m.Value}")));
            var pay_md5sign = Md5(parasData, Encoding.UTF8).ToLower();

            paras.Remove("key");           
            paras.Add("sign", pay_md5sign);

            var data = string.Join("&", paras.Select((m => $"{m.Key}={m.Value}")));
            SiteHelper.Log(guid, string.Format("{0} 支付请求 [data]({1}) {2}", order.Id, Qingting_URL, data), Qingting_logFile);


            //呼叫支付平台
            var responseString = SiteHelper.HttpPost(Qingting_URL, data);
            SiteHelper.Log(guid, string.Format("{0} 支付请求结果 [response] {1}", order.Id, responseString), Qingting_logFile);

            
            JObject json = JObject.Parse(responseString);
            var code = json["code"] != null ? json["code"].ToString() : string.Empty;
            var msg = json["msg"] != null ? json["msg"].ToString() : string.Empty;

            if (code != "00")
            {
                message = msg;

                //更新狀態
                Service.CDFailOrder(order.Id, string.Empty, message);
                SiteHelper.Log(guid, string.Format("{0} 支付请求失敗 {1} {2}", order.Id, code, msg), Qingting_logFile);
            }
            else
            {               
                var pay_order_sn = json["data"]["pay_order_sn"] != null ? json["data"]["pay_order_sn"].ToString() : string.Empty;

                Service.CDModifyTransID(order.Id, pay_order_sn);
                url = json["data"]["qr_code"] != null ? json["data"]["qr_code"].ToString() : string.Empty;
            }

            return url;
        }

        public static PayState call(string logGuid, int id)
        {
            try
            {
                var requestType = HttpContext.Current.Request.RequestType;
                if (requestType == "GET")
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), Qingting_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), Qingting_logFile);
                }
                
                string qingting_Key = id == (int)PayGatewayType.Qingting ? Qingting_Key : QingtingQe_Key;

                Dictionary<string, string> parameters = null;

                if (!CheckSign(logGuid, qingting_Key, out parameters))
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调非法签名"), Qingting_logFile);
                    return PayState.SignError;
                }

                var agent_code = parameters.ContainsKey("agent_code") ? parameters["agent_code"] : string.Empty;
                var pay_order_sn = parameters.ContainsKey("pay_order_sn") ? parameters["pay_order_sn"] : string.Empty;
                var orderid = parameters.ContainsKey("order_no") ? parameters["order_no"] : string.Empty;              
                var amount = parameters.ContainsKey("qr_code_money") ? parameters["qr_code_money"] : string.Empty;
                var status = parameters.ContainsKey("status") ? parameters["status"] : string.Empty;

                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(orderid));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 支付回调 訂單狀態已經完成,無須金幣充值", order.Id), Qingting_logFile);
                    return PayState.Success;
                }
                
                if (status != "1" || order.GWID != id)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 支付回调 订单失敗 status:{1},gwid={2}", order.Id, status, id), Qingting_logFile);
                    Service.CDFailOrder(order.Id, pay_order_sn, $"status:{status},gwid={id}");
                    return PayState.Error;
                }

                //進行金幣充值
                string msg = "";

                if (Convert.ToDouble(amount) == order.Amount)
                {
                    var data = $"完成支付";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, pay_order_sn, data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 支付回调 金幣充值成功 status:{status},pay_order_sn:{pay_order_sn},amount:{amount}", Qingting_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 支付回调 金幣充值错误 {msg}", Qingting_logFile);
                        Service.CDFailOrder(order.Id, pay_order_sn, $"{msg}");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, $"{order.Id}  支付回调 支付無效 status:{status},pay_order_sn:{pay_order_sn},amount:{amount}", Qingting_logFile);
                Service.CDFailOrder(order.Id, pay_order_sn, $"status:{status},pay_order_sn:{pay_order_sn},amount:{amount}");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("支付回调 异常错误 {0}", ex.ToString()), Qingting_logFile);
                return PayState.Error;
            }
        }

        private static string Md5(string str, Encoding ec)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            return BitConverter.ToString(md5.ComputeHash(ec.GetBytes(str))).Replace("-", "");
        }

        private static bool CheckSign(string guid, string key, out Dictionary<string, string> parameters)
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

            var sign = parameters["sign"].ToUpper();
            parameters.Remove("sign");        

            var str = $"agent_code={parameters["agent_code"]}" +
                      $"&pay_order_sn={parameters["pay_order_sn"]}" +
                      $"&order_no={parameters["order_no"]}" +
                      $"&status={parameters["status"]}" +
                      $"&qr_code_money={parameters["qr_code_money"]}" +
                      $"&pay_type={parameters["pay_type"]}" +
                      $"&timestamp={parameters["timestamp"]}" +
                      $"&game_account={parameters["game_account"]}" +
                      $"&key={key}";

            var cSign = Md5(str, Encoding.UTF8);
            SiteHelper.Log(guid, string.Format("支付回调签名 cSign={0} sign={1} srt={2}", cSign, sign, str), Qingting_logFile);

            return sign == cSign;
        }
    }
}