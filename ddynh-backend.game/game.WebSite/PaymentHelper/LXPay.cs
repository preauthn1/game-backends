using game.Entity.DTO.ThirdPartyPay;
using game.Entity.Payment;
using game.Infrastructure;
using game.Infrastructure.Extensions;
using game.Infrastructure.Utility.ThirdParty;
using game.WebSite.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using example.invalid;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using static game.WebSite.PayConfig;

namespace game.WebSite.PaymentHelper
{
    /// <summary>
    /// 利鑫
    /// </summary>
    public class LXPay
    {
        /// <summary>
        /// 代付回調
        /// </summary>
        /// <param name="logGuid"></param>
        /// <returns></returns>
        public static PayState WithdrawCallback(string logGuid)
        {
            try
            {
                Dictionary<string, string> parameters = null;

                string msg = string.Empty;
                if (CheckSign(logGuid, out parameters, ref msg))
                {
                    var id = Convert.ToInt32(parameters["mhtorderno"].Replace(LXPay_prefix, ""));
                    var state = (parameters["resultcode"] == "0") ? 4 : 3; //第三方状态(1:已提交, 2:已处理, 3:已退回, 4.已出账)
                    var order_num = parameters["pforderno"];

                    var res = Service.WithdrawCallbackByCK(id, state, order_num, $"note={msg}");
                    if (res.Result)
                    {
                        SiteHelper.Log(logGuid, $"提现回调成功 order_num={order_num}&state={state}&id={id}", LXPayWithdraw_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"提现回调错误 {res.Message}", LXPayWithdraw_logFile);
                        return PayState.Error;
                    }
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("提现异步回调非法签名"), LXPayWithdraw_logFile);
                    return PayState.SignError;
                }
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("提现异步回调异常错误 {0}", ex.ToString()), LXPayWithdraw_logFile);
                return PayState.Error;
            }
        }

        private static bool CheckSign(string guid, out Dictionary<string, string> parameters, ref string msg)
        {
            var requestType = HttpContext.Current.Request.RequestType;

            SiteHelper.Log(guid, $"支付回调 [Type]{requestType} [FormData] {string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))}", LXPayWithdraw_logFile);

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

            //若代付失败则为空值)
            if (HttpContext.Current.Request["resultcode"] == "1")
            {
                parameters.Remove("payouttime");
            }
            if (string.IsNullOrEmpty(HttpContext.Current.Request["note"]))
            {
                parameters.Remove("note");
                msg = "";
            }
            else
            {
                msg = HttpContext.Current.Request["note"];
            }
            
            parameters.Remove("sign");
            parameters.Add("signkey", LXPay_key);

            var sign = HttpContext.Current.Request["sign"];
            var str = string.Join("&", parameters.Select(m => $"{m.Key}={m.Value}"));
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            string cSign = BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(str))).Replace("-", "");
            SiteHelper.Log(guid, $"代付回调签名 check={sign == cSign} cSign={cSign} sign={sign} srt={str}", LXPayWithdraw_logFile);

            return sign == cSign;
        }
    }
}