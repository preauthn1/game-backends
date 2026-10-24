using game.Entity.Pay;
using game.Infrastructure;
using Newtonsoft.Json;
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
    public class Xiangyou
    {
        public static string post(CDPayOrder order, string guid, string tradeCode, ref string message)
        {
            string url = string.Empty;
          
            Dictionary<string, string> paras = new Dictionary<string, string>
            {
                { "notify_url", Xiangyou_NotifyUrl },
                { "return_url", Xiangyou_CallbackUrl },
                { "method", tradeCode },
                { "app_id", Xiangyou_Appid },
                { "out_trade_no", order.Id.ToString() },
                { "total_amount", (order.Amount * 100).ToString("F2") },
                { "subject", "充值" }
            };

            var sort = paras.OrderBy(k => k.Key);
            paras = sort.ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);

            string parasData = string.Join("&", paras.Select((m => $"{m.Key}={m.Value}"))) + $"&key={Xiangyou_Key}";
            var pay_md5sign = Md5(parasData, Encoding.UTF8).ToUpper();
            paras.Add("sign", pay_md5sign);
          

            var data = string.Join("&", paras.Select((m => $"{m.Key}={m.Value}")));
            SiteHelper.Log(guid, string.Format("{0} 支付请求 [data]({1}) {2}", order.Id, Xiangyou_Url, data), Xiangyou_logFile);

            //呼叫支付平台
            var responseString = SiteHelper.HttpPost(Xiangyou_Url, data);
            SiteHelper.Log(guid, string.Format("{0} 支付请求结果 [response] {1}", order.Id, responseString), Xiangyou_logFile);

            if (responseString.Contains("form"))
            {
                var api = Service.CreateRedirectData((int)PayGatewayType.LiuBa, $"<html><body>" + responseString + "</body></html>");

                if (api.Result)
                    url = api.Extend.ToString();
            }
            else
                message = responseString;

            return url;
        }

        public static PayState call(string logGuid)
        {
            try
            {
                Stream stream = HttpContext.Current.Request.InputStream;
                var bytes = new byte[stream.Length];
                stream.Read(bytes, 0, (int)stream.Length);
                var responseString = Encoding.UTF8.GetString(bytes);
                var requestType = HttpContext.Current.Request.RequestType;

                SiteHelper.Log(logGuid, $"支付回调 [FormData {requestType}] {responseString}", Xiangyou_logFile);

             
                Dictionary<string, string> parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);
            
                if (CheckSign(logGuid, parameters))
                {
                    var orderId = parameters["out_trade_no"];
                    var orderAmount = parameters["total_amount"];
                    var tradeNo = parameters["trade_no"];
                    var status = parameters["status"];

                    //檢查訂單狀態
                    var order = Service.CDGetPayOrderByID(Convert.ToInt32(orderId));
                    if (order.Status == CDOrderStatus.Paied)
                    {
                        SiteHelper.Log(logGuid, string.Format("訂單狀態已經完成,無須金幣充值"), Xiangyou_logFile);
                        return PayState.Success;
                    }

                    //支付失败
                    if (status != "1")
                    {
                        SiteHelper.Log(logGuid, string.Format("支付失败"), Xiangyou_logFile);
                        Service.CDFailOrder(order.Id, tradeNo, $"支付失败 amt: {orderAmount},status: {status}");
                        return PayState.Error;
                    }

                    //進行金幣充值
                    string msg = "";

                    if (status == "1" && order.Amount * 100 == Convert.ToInt64(double.Parse(orderAmount)))
                    {
                        var data = $"完成支付: 訂單: {orderId}, amt: {orderAmount}, status: {status}";

                        if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, tradeNo, data, ref msg) == true)
                        {
                            SiteHelper.Log(logGuid, $"金幣充值成功 orderid={order.Id} amount={order.Amount}", Xiangyou_logFile);
                            return PayState.Success;
                        }
                        else
                        {
                            SiteHelper.Log(logGuid, string.Format("金幣充值错误 {0}", msg), Xiangyou_logFile);
                            return PayState.Error;
                        }
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, string.Format("支付無效"), Xiangyou_logFile);
                        Service.CDFailOrder(order.Id, tradeNo, $"支付無效 訂單: {orderId} , amt: {orderAmount},status: {status}");
                        return PayState.Error;
                    }
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调非法签名"), Xiangyou_logFile);
                    return PayState.SignError;
                }
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("支付回调异常错误 {0}", ex.ToString()), Xiangyou_logFile);
                return PayState.Error;
            }
        }


        private static bool CheckSign(string guid, Dictionary<string, string> parameters)
        {
            var sign = parameters["sign"];
            parameters.Remove("sign");

            var sort = parameters.OrderBy(k => k.Key);
            parameters = sort.ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);          

            
            var str = string.Join("&", parameters.Select(m => string.Format("{0}={1}", m.Key, m.Value))) + $"&key={Xiangyou_Key}"; ;
            var cSign = Md5(str, Encoding.UTF8).ToUpper();
            SiteHelper.Log(guid, string.Format("支付回调签名 cSign={0} sign={1} srt={2}", cSign, sign, str), Xiangyou_logFile);

            return sign == cSign;
        }

        private static string Md5(string str, Encoding ec)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            return BitConverter.ToString(md5.ComputeHash(ec.GetBytes(str))).Replace("-", "");
        }
    }
}