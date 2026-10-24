using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using static game.WebSite.PayConfig;

namespace game.WebSite.PaymentHelper
{
    public class shanfu
    {
        public static string shanfuCallBack(string logGuid, int accountId, int playerId, string orderId, string teamOrderId, string chaseOrderId, int payType, int amount, int presentAmount, string status, int time, string sign, out string _orderId, out string _message)
        {
            try
            {
                Dictionary<string, string> paras = new Dictionary<string, string>();
                paras.Add("accountId", accountId.ToString());
                paras.Add("playerId", playerId.ToString());
                paras.Add("orderId", orderId);

                if (!string.IsNullOrEmpty(teamOrderId)) paras.Add("teamOrderId", teamOrderId);
                if (!string.IsNullOrEmpty(chaseOrderId)) paras.Add("chaseOrderId", chaseOrderId);

                paras.Add("payType", payType.ToString());
                paras.Add("amount", amount.ToString());
                paras.Add("presentAmount", presentAmount.ToString());
                paras.Add("status", status);
                paras.Add("time", time.ToString());
                paras = paras.OrderBy(o => o.Key).ToDictionary(o => o.Key, p => p.Value);
                paras.Add("key", Shanfu_Key);

                string pay_md5sign = Md5(string.Join("&", paras.Select(m => string.Format("{0}={1}", m.Key, m.Value))), Encoding.UTF8);
                var entries = paras.Select(d => string.Format("\"{0}\": [{1}]", d.Key, string.Join(",", d.Value)));

                //紀錄callback資料
                SiteHelper.Log(logGuid,
                     string.Format("闪付支付回調 [FormData] {0}", "{" + string.Join(",", entries) + "}"),
                     Shanfu_logFile);

                if (pay_md5sign.Equals(sign))
                {
                    var res = Service.FinishedOrderShanfu(playerId, orderId, teamOrderId, chaseOrderId, payType, amount, presentAmount, status, time);
                    _message = res.Message;
                    _orderId = res.Extend.ToString();
                    SiteHelper.Log(logGuid, $"闪付回調处理结果 Result:{res.Result}&_orderId={_orderId}&_message={_message}", Shanfu_logFile);

                    if (res.Result)
                        return "SUCCESS";
                    else if (_message == "FAILAMOUNT")
                        return "FAILAMOUNT";
                    else if (_message == "ERRORUSER")
                        return "ERRORUSER";
                    else
                        return "ERROR";
                }
                else
                {
                    _message = $"签名字符错误";
                    _orderId = "";
                    SiteHelper.Log(logGuid, $"闪付回調签名字符错误: {sign}", Shanfu_logFile);
                    return "ERROR";
                }
            }
            catch (Exception ex)
            {
                _message = ex.ToString();
                _orderId = "";
                SiteHelper.Log(logGuid, string.Format("闪付支付回調常错误 {0}", ex.ToString()), Shanfu_logFile);
                return "ERROR";
            }
        }

        private static string Md5(string str, Encoding ec)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            return BitConverter.ToString(md5.ComputeHash(ec.GetBytes(str))).Replace("-", "");
        }
    }
}