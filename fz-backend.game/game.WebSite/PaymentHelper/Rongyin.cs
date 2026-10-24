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
    public class Rongyin
    {
        public static string post(CDPayOrder order, string guid, string tradeCode, ref string message)
        {
            string url = string.Empty;

            Dictionary<string, string> paras = new Dictionary<string, string>
            {
                { "code", Rongyin_code },
                { "tradeAmount", order.Amount.ToString() },
                { "outOrderNo", order.Id.ToString() },
                { "goodsClauses", "充值" },
                { "payCode", tradeCode },
                { "notifyUrl", Rongyin_CallbackUrl }
            };

            string parasData = string.Join("&", paras.OrderBy(x => x.Key).Select((m => $"{m.Key}={m.Value}"))) + "&key=" + Rongyin_Key;
            var pay_md5sign = Md5(parasData, Encoding.UTF8).ToLower();
            paras.Add("sign", pay_md5sign);

            var data = string.Join("&", paras.Select((m => $"{m.Key}={m.Value}")));
            SiteHelper.Log(guid, string.Format("{0} 支付请求 [data]({1}) {2}", order.Id, Rongyin_PayURL, data), Rongyin_logFile);

            //呼叫支付平台
            var responseString = SiteHelper.HttpPost(Rongyin_PayURL, data);
            SiteHelper.Log(guid, string.Format("{0} 支付请求结果 [response] {1}", order.Id, responseString), Rongyin_logFile);

            JObject json = JObject.Parse(responseString);
            var payState = json["payState"] != null ? json["payState"].ToString() : string.Empty;

            if (!payState.Equals("success"))
            {
                var respmsg = json["message"] != null ? json["message"].ToString() : string.Empty;
                message = string.Format("{0} 支付请求失敗 {1}", order.Id, respmsg);

                //更新狀態
                Service.CDFailOrder(order.Id, string.Empty, respmsg);
                SiteHelper.Log(guid, message, Rongyin_logFile);
            }
            else
            {
                var shopCode = json["shopCode"] != null ? json["shopCode"].ToString() : string.Empty;

                Service.CDModifyTransID(order.Id, shopCode);
                if (json["qrcode"] != null)
                    url = json["qrcode"] != null ? json["qrcode"].ToString() : string.Empty;
                else
                    url = json["url"] != null ? json["url"].ToString() : string.Empty;
            }

            return url;
        }


        public static PayState call(string logGuid)
        {
            try
            {      
                Stream stream = HttpContext.Current.Request.InputStream;
                var b = new byte[stream.Length];
                stream.Read(b, 0, (int)stream.Length);
                var responseString = Encoding.UTF8.GetString(b);
                SiteHelper.Log(logGuid, string.Format("支付回调 [JSON] {0}", responseString), Rongyin_logFile);

                JObject json = JObject.Parse(responseString);

                if (!CheckSign(logGuid, json))
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调非法签名"), Rongyin_logFile);
                    return PayState.SignError;
                }

                var outOrderNo = json["outOrderNo"] != null ? json["outOrderNo"].ToString() : string.Empty;
                var status = json["code"] != null ? json["code"].ToString() : string.Empty;
                var amount = json["tradeAmount"] != null ? json["tradeAmount"].ToString() : string.Empty;
                var shopCode = json["shopCode"] != null ? json["shopCode"].ToString() : string.Empty;
                var message = json["msg"] != null ? json["msg"].ToString() : string.Empty;

                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(outOrderNo));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 支付回调 訂單狀態已經完成,無須金幣充值", order.Id), Rongyin_logFile);
                    return PayState.Success;
                }


                //進行金幣充值
                string msg = "";

                if ((message == "SUCCESS") && (Convert.ToDouble(amount) == order.Amount))
                {
                    var data = $"支付回调 完成支付: 支付订单号: {shopCode}, 状态: {status}";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, shopCode, data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, string.Format("{0} 支付回调 金幣充值成功 支付订单号:{1} 金额:amount={2}", order.Id, shopCode, order.Amount), Rongyin_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, string.Format("{0} 支付回调 金幣充值错误 {1}", order.Id, msg), Rongyin_logFile);
                        Service.CDFailOrder(order.Id, shopCode, $"支付订单号:{shopCode} , 支付金额(分): {amount}, 状态: {status}");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, string.Format("{0}  支付回调 支付無效 支付订单号:{1}, 支付金额:{2}, 状态:{3}", order.Id, shopCode, amount, status), Rongyin_logFile);
                Service.CDFailOrder(order.Id, shopCode, $"支付订单号:{shopCode} , 支付金额(分): {amount}, 状态: {status}");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("支付回调 异常错误 {0}", ex.ToString()), Rongyin_logFile);
                return PayState.Error;
            }
        }

        private static string Md5(string str, Encoding ec)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            return BitConverter.ToString(md5.ComputeHash(ec.GetBytes(str))).Replace("-", "");
        }

        private static bool CheckSign(string guid, JObject json)
        {
            Dictionary<string, string> parameters = json.ToObject<Dictionary<string, string>>();

            var sign = parameters["sign"];
            parameters.Remove("sign");
            parameters = parameters.OrderBy(o => o.Key).ToDictionary(o => o.Key, p => p.Value);

            var str = string.Join("&", parameters.Select(m => string.Format("{0}={1}", m.Key, m.Value))) + "&key=" + Rongyin_Key;
            var cSign = Md5(str, Encoding.UTF8).ToLower();
            SiteHelper.Log(guid, string.Format("支付回调签名 cSign={0} sign={1} srt={2}", cSign, sign, str), Rongyin_logFile);

            return sign == cSign;
        }
    }
}