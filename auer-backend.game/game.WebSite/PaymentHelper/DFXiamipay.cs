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
using example.invalid;
using Newtonsoft.Json;

namespace game.WebSite.PaymentHelper
{
    public class DFXiamipay
    {
        public static string post(CDPayOrder order, string guid, string tradeCode, ref string message)
        {
            string title = order.Id.ToString();
            string amount = (order.Amount * 100).ToString();
            string clientip = order.IPAddress;
            string timestamp = Convert.ToInt64(DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds * 1000).ToString();
            string callback =DFXiamipay_CallbackUrl;
            string sign = Md5($"{DFXiamipay_appKey}{title}{amount}{timestamp}{DFXiamipay_appSecret}", Encoding.UTF8).ToLower();

            string url =DFXiamipay_URL + $"/api/{DFXiamipay_appKey}/{tradeCode}?title={title}&amount={amount}&clientip={clientip}&timestamp={timestamp}&sign={sign}&callback={callback}";
            SiteHelper.Log(guid, $"充值请求: {url}",DFXiamipay_logFile);

            var responseString = SiteHelper.GetPage(url);
            SiteHelper.Log(guid, string.Format("{0} 支付请求结果 [response] {1}", order.Id, responseString),DFXiamipay_logFile);

            JObject json = JObject.Parse(responseString);
            var result = json["ret"] != null ? json["ret"].ToString() : string.Empty;

            if (!result.Equals("True"))
            {
                var respmsg = json["msg"] != null ? json["msg"].ToString() : string.Empty;
                message = string.Format("{0} 支付请求失敗 {1}", order.Id, respmsg);

                //更新狀態
                Service.CDFailOrder(order.Id, string.Empty, respmsg);
                SiteHelper.Log(guid, message,DFXiamipay_logFile);
                url = string.Empty;
            }
            else
            {
                var result_title = json["data"]["title"] != null ? json["data"]["title"].ToString() : string.Empty;
                var result_amount = json["data"]["amount"] != null ? json["data"]["amount"].ToString() : string.Empty;

                Service.CDModifyTransID(order.Id, null);
                url = json["data"]["payUrl"] != null ? json["data"]["payUrl"].ToString() : string.Empty;
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
                SiteHelper.Log(logGuid, string.Format("支付回调 [FormData] {0}", responseString),DFXiamipay_logFile);

                if (!CheckSign(logGuid, responseString))
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调非法签名"),DFXiamipay_logFile);
                    return PayState.SignError;
                }

                JObject json_data = JObject.Parse(responseString);
                JObject json = JObject.Parse(json_data["data"].ToString());

                var amount = json["amount"] != null ? json["amount"].ToString() : string.Empty;
                var orderNo = json["title"] != null ? json["title"].ToString() : string.Empty;
                var title = json["title"] != null ? json["title"].ToString() : string.Empty;
                var type = json["type"] != null ? json["type"].ToString() : string.Empty;
                var transId = json["transId"] != null ? json["transId"].ToString() : string.Empty;
                var transTime = json["transTime"] != null ? json["transTime"].ToString() : string.Empty;
                var createTime = json["createTime"] != null ? json["createTime"].ToString() : string.Empty;
                var appKey = json["appKey"] != null ? json["appKey"].ToString() : string.Empty;



                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(orderNo));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 支付回调 訂單狀態已經完成,無須金幣充值", order.Id),DFXiamipay_logFile);
                    return PayState.Success;
                }

                //進行金幣充值
                string msg = "";

                if (Convert.ToInt64(amount) == order.Amount * 100)
                {
                    var data = $"完成支付 transTime:{transTime}";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, transId, data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 支付回调 金幣充值成功 transId,:{transId},transTime:{transTime},createTime:{createTime},amount:{amount}",DFXiamipay_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 支付回调 金幣充值错误 {msg}",DFXiamipay_logFile);
                        Service.CDFailOrder(order.Id, transId, $"{msg}");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, string.Format($"{order.Id}  支付回调 支付無效 transId:{transId},transTime:{transTime},amount:{amount}"),DFXiamipay_logFile);
                Service.CDFailOrder(order.Id, transId, $"transTime:{transTime},createTime:{createTime},amount:{amount}");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("支付回调 异常错误 {0}", ex.ToString()),DFXiamipay_logFile);
                return PayState.Error;
            }
        }

        private static string Md5(string str, Encoding ec)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            return BitConverter.ToString(md5.ComputeHash(ec.GetBytes(str))).Replace("-", "");
        }

        private static bool CheckSign(string guid, string responseString)
        {
            JObject json = JObject.Parse(responseString);
            var data = json["data"] != null ? json["data"].ToString() : string.Empty;
            var sign = json["sign"] != null ? json["sign"].ToString() : string.Empty;

            var str = data +DFXiamipay_appSecret;
            var cSign = Md5(str, Encoding.UTF8).ToLower();
            SiteHelper.Log(guid, string.Format("支付回调签名 cSign={0} sign={1} srt={2}", cSign, sign, str),DFXiamipay_logFile);

            return sign == cSign;
        }
    }
}