using game.Entity.Money;
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
    public class Yihao
    {
        public static PayState WithdrawCallback(string logGuid)
        {
            try
            {
                var requestType = HttpContext.Current.Request.RequestType;
                if (requestType == "GET")
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), Yihao_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), Yihao_logFile);
                }
                Dictionary<string, string> parameters = null;

                if (!CheckSign(logGuid, out parameters))
                {
                    SiteHelper.Log(logGuid, string.Format("回调非法签名"), Yihao_logFile);
                    return PayState.SignError;
                }

                var orderId = parameters.ContainsKey("out_trade_no") ? parameters["out_trade_no"] : string.Empty;
                var orderStatus = parameters.ContainsKey("result") ? parameters["result"] : string.Empty;
                var transId = parameters.ContainsKey("pay_trade_no") ? parameters["pay_trade_no"] : string.Empty;
                var transData = string.Empty;
                var orderAmt = parameters.ContainsKey("real_fee") ? parameters["real_fee"] : string.Empty;

                var status = ConvertLogStatus.Queueing;
                if (orderStatus == "0") status = ConvertLogStatus.AlreadyPaid;              
                else if (orderStatus == "2") status = ConvertLogStatus.AutoPaymentFailed;

                var res = Service.WithdrawCallback(orderId, (int)status, transId, transData);
                if (res.Result)
                {
                    SiteHelper.Log(logGuid, $"提现回调成功 orderId={orderId}&orderState={status}&tranSeqId={transId}&orderAmt={orderAmt}", Yihao_logFile);
                    return PayState.Success;
                }
                else
                {
                    SiteHelper.Log(logGuid, $"提现回调错误 {res.Message}", Yihao_logFile);
                    return PayState.Error;
                }
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("提现异步回调异常错误 {0}", ex.ToString()), Yihao_logFile);
                return PayState.Error;
            }
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

            var sort = parameters.OrderBy(k => k.Key);
            parameters = sort.ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);

            var str = string.Join("&", parameters.Where(x=> !string.IsNullOrEmpty(x.Value)).Select(m => string.Format("{0}={1}", m.Key, m.Value))) + "&key=" + Yihao_MD5KEY;
            var cSign = Md5(str, Encoding.UTF8).ToLower();
            SiteHelper.Log(guid, string.Format("支付回调签名 cSign={0} sign={1} srt={2}", cSign, sign, str), Yihao_logFile);

            return sign == cSign;
        }

        static string Md5(string str, Encoding ec)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            return BitConverter.ToString(md5.ComputeHash(ec.GetBytes(str))).Replace("-", "");
        }
    }
}