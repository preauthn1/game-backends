using game.Entity.Money;
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
    public class Zumire
    {
        public static string post(CDPayOrder order, string guid, string tradeCode, ref string message)
        {
            string url = string.Empty;
            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            Dictionary<string, string> paras = new Dictionary<string, string>
            {
                { "sign_type", "md5" },
                { "mch_id", Zumire_mchID },
                { "mch_order", order.Id.ToString() },
                { "amt", (order.Amount * 1000).ToString() },
                { "remark", "充值" },
                { "created_at", unixTimestamp.ToString() },
                { "client_ip", order.IPAddress },
                { "notify_url", Zumire_NotifyUrl },
                { "mch_key", Zumire_Key }
            };

            var sort = paras.OrderBy(k => k.Key);
            paras = sort.ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);

            string parasData = string.Join("&", paras.Select((m => $"{m.Key}={m.Value}")));
            var pay_md5sign = Md5(parasData, Encoding.UTF8).ToLower();
            paras.Add("sign", pay_md5sign);
            paras.Remove("mch_key");

            var data = string.Join("&", paras.Select((m => $"{m.Key}={m.Value}")));
            SiteHelper.Log(guid, string.Format("{0} 支付请求 [data]({1}) {2}", order.Id, Zumire_Url + tradeCode, data), Zumire_logFile);

            //呼叫支付平台
            var responseString = SiteHelper.HttpPost(Zumire_Url + tradeCode, data);
            SiteHelper.Log(guid, string.Format("{0} 支付请求结果 [response] {1}", order.Id, responseString), Zumire_logFile);

            JObject json = JObject.Parse(responseString);
            var retCode = json["code"] != null ? json["code"].ToString() : string.Empty;

            if (!retCode.Equals("1"))
            {
                var respmsg = json["msg"] != null ? json["msg"].ToString() : string.Empty;
                message = string.Format("{0} 支付请求失敗 {1}", order.Id, respmsg);

                //更新狀態
                Service.CDFailOrder(order.Id, string.Empty, respmsg);
                SiteHelper.Log(guid, message, Zumire_logFile);
            }
            else
            {                
                //Service.CDModifyTransID(order.Id, orderIDP);
                url = json["data"]["jump_url"] != null ? json["data"]["jump_url"].ToString() : string.Empty;
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
                    SiteHelper.Log(logGuid, string.Format("支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), Zumire_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), Zumire_logFile);
                }

                if (CheckSign(logGuid, Zumire_Key))
                {
                    var orderId = HttpContext.Current.Request["mch_order"];
                    var orderAmount = HttpContext.Current.Request["amt"];
                    var status = HttpContext.Current.Request["status"];                  

                    //檢查訂單狀態
                    var order = Service.CDGetPayOrderByID(Convert.ToInt32(orderId));
                    if (order.Status == CDOrderStatus.Paied)
                    {
                        SiteHelper.Log(logGuid, string.Format("訂單狀態已經完成,無須金幣充值"), Zumire_logFile);
                        return PayState.Success;
                    }

                    //支付失败
                    if (status != "2")
                    {
                        SiteHelper.Log(logGuid, string.Format("支付失败"), Zumire_logFile);
                        Service.CDFailOrder(order.Id, orderId, $"支付失败 amt: {orderAmount},status: {status}");
                        return PayState.Error;
                    }

                    //進行金幣充值
                    string msg = "";

                    if (status == "2" && order.Amount * 1000 == Convert.ToInt64(double.Parse(orderAmount)))
                    {
                        var data = $"完成支付: 訂單: {orderId}, amt: {orderAmount}, status: {status}";

                        if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, "0", data, ref msg) == true)
                        {
                            SiteHelper.Log(logGuid, $"金幣充值成功 orderid={order.Id} amount={order.Amount}", Zumire_logFile);
                            return PayState.Success;
                        }
                        else
                        {
                            SiteHelper.Log(logGuid, string.Format("金幣充值错误 {0}", msg), Zumire_logFile);
                            return PayState.Error;
                        }
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, string.Format("支付無效"), Zumire_logFile);
                        Service.CDFailOrder(order.Id, orderId, $"支付無效 訂單: {orderId} , amt: {orderAmount},status: {status}");
                        return PayState.Error;
                    }
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调非法签名"), Zumire_logFile);
                    return PayState.SignError;
                }
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("支付回调异常错误 {0}", ex.ToString()), Zumire_logFile);
                return PayState.Error;
            }
        }

        public static PayState WithdrawCallback(string logGuid)
        {
            try
            {
                var requestType = HttpContext.Current.Request.RequestType;
                if (requestType == "GET")
                {
                    SiteHelper.Log(logGuid, string.Format("提现回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), Zumire_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("提现回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), Zumire_logFile);
                }


                var orderId = HttpContext.Current.Request["mch_order"];
                var orderAmt = HttpContext.Current.Request["amt"];
                var orderStatus = HttpContext.Current.Request["status"];
                var transId = string.Empty;
                var transData = string.Empty;
                var status = ConvertLogStatus.Queueing;

                if (!CheckSign(logGuid, ZumireWithdraw_Key))
                {
                    SiteHelper.Log(logGuid, string.Format("提现异步回调非法签名"), Zumire_logFile);
                    return PayState.SignError;
                }

                //1 打款中 10 打款成功 11 ⼿动成功 20 已撤销 21 ⼿动撤销 30 错误
                if (orderStatus == "1") status = ConvertLogStatus.Queueing;
                else if (orderStatus == "10" || orderStatus == "11") status = ConvertLogStatus.AlreadyPaid;
                else status = ConvertLogStatus.AutoPaymentFailed;

                var res = Service.WithdrawCallback(orderId, (int)status, transId, transData);
                if (res.Result)
                {
                    SiteHelper.Log(logGuid, $"提现回调成功 orderId={orderId}&orderState={status}&tranSeqId={transId}&orderAmt={orderAmt}", Zumire_logFile);
                    return PayState.Success;
                }
                else
                {
                    SiteHelper.Log(logGuid, $"提现回调错误 {res.Message}", Zumire_logFile);
                    return PayState.Error;
                }
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("提现异步回调异常错误 {0}", ex.ToString()), Zumire_logFile);
                return PayState.Error;
            }
        }

        private static bool CheckSign(string guid, string key)
        {

            var requestType = HttpContext.Current.Request.RequestType;
            Dictionary<string, string> parameters = null;

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

            parameters.Remove("sign");
            parameters.Add("mch_key", key);
            var sort = parameters.Where(x => !string.IsNullOrEmpty(x.Value)).OrderBy(k => k.Key);
            parameters = sort.ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);

            
            //parameters = parameters.Where(x=>!string.IsNullOrEmpty(x.Value)).OrderBy(o => o.Key).ToDictionary(o => o.Key, p => p.Value);

            var sign = HttpContext.Current.Request["sign"];
            var str = string.Join("&", parameters.Select(m => string.Format("{0}={1}", m.Key, m.Value)));
            var cSign = Md5(str, Encoding.UTF8).ToLower();
            SiteHelper.Log(guid, string.Format("支付回调签名 cSign={0} sign={1} srt={2}", cSign, sign, str), Zumire_logFile);

            return sign == cSign;
        }

        private static string Md5(string str, Encoding ec)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            return BitConverter.ToString(md5.ComputeHash(ec.GetBytes(str))).Replace("-", "");
        }
    }
}