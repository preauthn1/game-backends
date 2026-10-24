using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using game.Entity.Pay;
using game.Infrastructure;
using static game.WebSite.PayConfig;

namespace game.WebSite.PaymentHelper
{
    public class UNPay
    {
        private class ComparerString : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                return string.CompareOrdinal(x, y);
            }
        }

        public static string post(CDPayOrder order, string guid, string tradeCode)
        {
            //參數簽名順序已經排好
            var data = $"parter={UNPay_Parter}&type={tradeCode}&orderid={order.Id.ToString()}&callbackurl={UNPay_CallbackUrl}";
            //簽名結果
            var paysign = MD532(data + UNPay_Key);

            //跳轉url
            var payUrl = UNPay_PayURL.Contains("?") ? UNPay_PayURL : UNPay_PayURL + "?";
            var url = $"{payUrl}{data}&value={order.Amount.ToString()}&sign={paysign.ToLower()}";

            SiteHelper.Log(guid, $"充值请求: {url}", UNPay_logFile);

            return url;
        }


        public static PayState call(string logGuid)
        {
            try
            {
                var requestType = HttpContext.Current.Request.RequestType;
                if (requestType == "GET")
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), UNPay_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), UNPay_logFile);
                }

                if (CheckSign(logGuid))
                {
                    var orderId = HttpContext.Current.Request["orderid"];
                    var status = HttpContext.Current.Request["restate"];
                    var orderAmount = HttpContext.Current.Request["ovalue"];
                    var sign = HttpContext.Current.Request["sign"];



                    //檢查訂單狀態
                    var order = Service.CDGetPayOrderByID(Convert.ToInt32(orderId));
                    if (order.Status == CDOrderStatus.Paied)
                    {
                        SiteHelper.Log(logGuid, string.Format("訂單狀態已經完成,無須金幣充值"), UNPay_logFile);
                        return PayState.Success;
                    }

                    //0成功, 其他失敗
                    if (status != "0")
                    {
                        SiteHelper.Log(logGuid, string.Format("支付失败"), UNPay_logFile);
                        Service.CDFailOrder(order.Id, orderId, $"支付失败 UNPay訂單: {orderId} , orderAmount: {orderAmount}, status: {status}");
                        return PayState.Error;
                    }

                    //進行金幣充值
                    string msg = "";

                    if (status == "0" && order.Amount == Convert.ToInt64(double.Parse(orderAmount)))
                    {
                        var data = $"完成支付: UNPay訂單: {orderId}, orderAmount: {orderAmount}, status: {status}";

                        if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, "0", data, ref msg) == true)
                        {
                            SiteHelper.Log(logGuid, $"金幣充值成功 orderid={order.Id} amount={order.Amount}", UNPay_logFile);
                            return PayState.Success;
                        }
                        else
                        {
                            SiteHelper.Log(logGuid, string.Format("金幣充值错误 {0}", msg), UNPay_logFile);
                            return PayState.Error;
                        }
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, string.Format("支付無效"), UNPay_logFile);
                        Service.CDFailOrder(order.Id, orderId, $"支付無效 UNPay訂單: {orderId} , orderAmount: {orderAmount}, status: {status}");
                        return PayState.Error;
                    }
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调非法签名"), UNPay_logFile);
                    return PayState.SignError;
                }
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("支付回调异常错误 {0}", ex.ToString()), UNPay_logFile);
                return PayState.Error;
            }
        }


        private static bool CheckSign(string guid)
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

            var orderid = parameters["orderid"];
            var restate = parameters["restate"];
            var ovalue = parameters["ovalue"];
            var sign = parameters["sign"];

            //參數已經按順序排序
            var str = $"orderid={orderid}&restate={restate}&ovalue={ovalue}{UNPay_Key}";
            var cSign = MD532(str);
            SiteHelper.Log(guid, string.Format("支付回调签名 cSign={0} sign={1} srt={2}", cSign, sign, str), UNPay_logFile);

            return sign.ToLower() == cSign.ToLower();
        }


        /// <summary>
        /// 与java的MD5加密保持一致
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static string MD532(string str)
        {
            var md5Csp = new MD5CryptoServiceProvider();
            byte[] md5Source = Encoding.UTF8.GetBytes(str);
            byte[] md5Out = md5Csp.ComputeHash(md5Source);
            string pwd = "";
            for (int i = 0; i < md5Out.Length; i++)
            {
                pwd += md5Out[i].ToString("x2");
            }
            return pwd;
        }
    }
}