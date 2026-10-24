using game.Entity.Pay;
using game.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using example.invalid;
using System.Linq;
using example.invalid;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using static game.WebSite.PayConfig;

namespace game.WebSite.PaymentHelper
{
    public class Pay365
    {
        /// <summary>
        /// 创建订单(跳转网页链接)
        /// 
        public static string post(CDPayOrder order, string guid, string tradeCode, ref string message)
        {
            string url = string.Empty;

            Dictionary<string, string> paras = new Dictionary<string, string>
            {
                { "version", "V1.0" },
                { "partner_id", Pay365_PartnerID},
                { "pay_key", GetPayKey(tradeCode)},
                { "pay_type", tradeCode},
                { "order_no", order.Id.ToString()},
                { "amount", order.Amount.ToString()},
                { "notify_url", Pay365_NotifyUrl},
                { "payer_id", order.UserID.ToString()}
            };

            //支付请求的签名方式
            //sign = md5((amount=额度&key=商户秘钥&notify_url=回调地址&order_no=订单号&partner_id=商户号&pay_key=WEIXIN&pay_type=0002&payer_id=付款人账号&version = V1.0).小写).小写
            var sign = Md5(($"amount={order.Amount.ToString()}&key={Pay365_Key}&notify_url={Pay365_NotifyUrl}&order_no={order.Id.ToString()}&partner_id={Pay365_PartnerID}&pay_key={GetPayKey(tradeCode)}&pay_type={tradeCode}&payer_id={order.UserID.ToString()}&version=V1.0").ToLower(), Encoding.UTF8).ToLower();
            paras.Add("sign", sign);

            //api的请求，都是post + json的方式。
            var data = JsonConvert.SerializeObject(paras);
            SiteHelper.Log(guid, $"{order.Id} 支付请求 [data]({Pay365_Url}) {data}", Pay365_logFile);

            //呼叫支付平台
            string responseString = SiteHelper.HttpPostJson(Pay365_Url, data);
            SiteHelper.Log(guid, $"{order.Id} 支付请求 [data] {responseString}", Pay365_logFile);


            //微信扫码、微信H5、支付宝扫码，都是返回跳转地址，(只有支付宝H5返回的是html<--暫時還沒處理)
            if (CheckURLValid(responseString))
            {
                url = responseString;
            }
            else
            {
                JObject json = JObject.Parse(responseString);
                var code = json["code"] != null ? json["code"].ToString() : string.Empty;
                var msg = json["msg"] != null ? json["msg"].ToString() : string.Empty;

                message = msg;

                //更新狀態
                Service.CDFailOrder(order.Id, string.Empty, message);
                SiteHelper.Log(guid, string.Format("{0} 支付请求失敗 {1} {2}", order.Id, code, msg), Pay365_logFile);
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
                SiteHelper.Log(logGuid, string.Format("支付回调 [FormData] {0}", responseString), Pay365_logFile);

                JObject json = JObject.Parse(responseString);

                var order_no = json["order_no"] != null ? json["order_no"].ToString() : string.Empty;               //商户订单号
                var littlepay_id = json["littlepay_id"] != null ? json["littlepay_id"].ToString() : string.Empty;   //365平台订单号
                var amount = json["amount"] != null ? json["amount"].ToString() : string.Empty;                     //订单金额
                var status = json["status"] != null ? json["status"].ToString() : string.Empty;                     //支付状态
                var sign = json["sign"] != null ? json["sign"].ToString() : string.Empty;                           //MD5签名

                if (!Pay365_CheckSign(logGuid, amount, littlepay_id, order_no, status, sign))
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调非法签名"), Pay365_logFile);
                    return PayState.SignError;
                }

                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(order_no));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 支付回调 訂單狀態已經完成,無須金幣充值", order.Id), Pay365_logFile);
                    return PayState.Success;
                }

                //進行金幣充值
                string msg = "";

                if (status.Equals("1") && ((Convert.ToDecimal(amount)) == order.Amount))
                {
                    var data = $"365支付回调 完成支付: 支付订单号: {order_no} , 渠道订单号: {littlepay_id}, 状态: {status}";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, littlepay_id, data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 365支付回调 金幣充值成功 支付订单号:{order_no} 金额:amount={order.Amount} 渠道订单号:{littlepay_id}", Pay365_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 365支付回调 金幣充值错误 {msg}", Pay365_logFile);

                        Service.CDFailOrder(order.Id, littlepay_id, $"365支付订单号:{order_no} , 渠道订单号: {littlepay_id} , 支付金额(分): {(Convert.ToInt64(amount) * 100).ToString()}, 状态: {status}");
                        return PayState.Error;
                    }
                }
                else if (status.Equals("2"))
                {
                    SiteHelper.Log(logGuid, $"{order.Id}  365支付回调 支付無效 支付订单号:{order_no}, 渠道订单号:{"0"}, 支付金额(分):{(Convert.ToInt64(amount) * 100).ToString()}, 状态:{status}", Pay365_logFile);
                    return PayState.Error;
                }

                SiteHelper.Log(logGuid, $"{order.Id}  365支付回调 支付無效 支付订单号:{order_no}, 渠道订单号:{"0"}, 支付金额(分):{(Convert.ToInt64(amount) * 100).ToString()}, 状态:{status}", Pay365_logFile);
                Service.CDFailOrder(order.Id, "0", $"365支付订单号:{order_no} , 渠道订单号: {littlepay_id} , 支付金额(分): {(Convert.ToDecimal(amount) * 100).ToString()}, 状态: {status}");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("365支付回调 异常错误 {0}", ex.ToString()), Pay365_logFile);
                return PayState.Error;
            }
        }

        private static string GetPayKey(string pay_type)
        {
            switch (pay_type)
            {
                case "0002":
                case "0004":
                    return "WEIXIN";
                case "0005":
                case "0006":
                    return "ALIPAY";
                default:
                    return "";
            }
        }

        private static string Md5(string str, Encoding ec)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            return BitConverter.ToString(md5.ComputeHash(ec.GetBytes(str))).Replace("-", "");
        }

        //檢查是否為有效網址
        private static bool CheckURLValid(string source)
        {
            Uri uriResult;
            return Uri.TryCreate(source, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        private static bool Pay365_CheckSign(string guid, string amount, string littlepay_id, string order_no, string status, string sign )
        {
            var str = $"amount={amount}&key={Pay365_Key}&littlepay_id={littlepay_id}&order_no={order_no}&status={status}";
            var cSign = Md5(str.ToLower(), Encoding.UTF8).ToLower();

            SiteHelper.Log(guid, string.Format("支付回调签名 cSign={0} sign={1} srt={2}", cSign, sign, str), Pay365_logFile);

            return sign == cSign;
        }
    }
}