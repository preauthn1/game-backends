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
    public class Shunchang
    {   
        public static PayState WithdrawCallback(string logGuid)
        {
            try
            {
                var requestType = HttpContext.Current.Request.RequestType;
                if (requestType == "GET")
                {
                    SiteHelper.Log(logGuid, string.Format("提现回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), Shunchang_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("提现回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), Shunchang_logFile);
                }


                var orderId = HttpContext.Current.Request["mch_order"];
                var orderAmt = HttpContext.Current.Request["amt"];
                var orderStatus = HttpContext.Current.Request["status"];
                var transId = string.Empty;
                var transData = string.Empty;
                var status = ConvertLogStatus.Queueing;

                if (!CheckSign(logGuid, ShunchangWithdraw_Key))
                {
                    SiteHelper.Log(logGuid, string.Format("提现异步回调非法签名"), Shunchang_logFile);
                    return PayState.SignError;
                }

                //1 打款中 10 打款成功 11 ⼿动成功 20 已撤销 21 ⼿动撤销 30 错误
                if (orderStatus == "1") status = ConvertLogStatus.Queueing;
                else if (orderStatus == "10" || orderStatus == "11") status = ConvertLogStatus.AlreadyPaid;
                else status = ConvertLogStatus.AutoPaymentFailed;

                var res = Service.WithdrawCallback(orderId, (int)status, transId, transData);
                if (res.Result)
                {
                    SiteHelper.Log(logGuid, $"提现回调成功 orderId={orderId}&orderState={status}&tranSeqId={transId}&orderAmt={orderAmt}", Shunchang_logFile);
                    return PayState.Success;
                }
                else
                {
                    SiteHelper.Log(logGuid, $"提现回调错误 {res.Message}", Shunchang_logFile);
                    return PayState.Error;
                }
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("提现异步回调异常错误 {0}", ex.ToString()), Shunchang_logFile);
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
            SiteHelper.Log(guid, string.Format("支付回调签名 cSign={0} sign={1} srt={2}", cSign, sign, str), Shunchang_logFile);

            return sign == cSign;
        }

        private static string Md5(string str, Encoding ec)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            return BitConverter.ToString(md5.ComputeHash(ec.GetBytes(str))).Replace("-", "");
        }
    }
}