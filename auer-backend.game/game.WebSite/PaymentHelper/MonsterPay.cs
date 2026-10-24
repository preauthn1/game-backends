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
    public class MonsterPay
    {
        /// <summary>
        /// 创建订单(跳转网页链接)
        /// </summary>
        /// <param name="order"></param>
        /// <param name="guid"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string post(CDPayOrder order, string guid, string tradeCode)
        {

            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;


            Dictionary<string, string> paras = new Dictionary<string, string>
            {
                { "merchant_code", MonsterPay_memberid  },
                { "method", tradeCode},
                { "money", order.Amount.ToString()},
                { "order_sn", order.Id.ToString("D9")},
                { "notify_url", MonsterPay_NotifyUrl},
                { "return_url", MonsterPay_CallBackUrl},     
            };
            var signdata = string.Join("&", paras.OrderBy(x=>x.Key).Select(m => string.Format("{0}={1}", m.Key, m.Value)));
            SiteHelper.Log(guid, $"怪兽支付请求加密参数 [data]({signdata}) ", MonsterPay_logFile);
            var merchant_private_key = PayHelper.RSAPrivateKeyJava2DotNet(MonsterPay_PrivateKey);
            var sign = PayHelper.RSASign(signdata, merchant_private_key); ;
            paras.Add("sign", sign);


            var url = $"<html><body><form method='post' name='form' action='{MonsterPay_PayURL}' enctype=\"application/x-www-form-urlencoded\">" +
                    string.Join("", paras.Select(m => string.Format("<input type='hidden' name='{0}' value='{1}' >", m.Key, m.Value))) +
                    "</form><script>window.onload = function(){document.forms['form'].submit();}</script></body></html>";

            SiteHelper.Log(guid, $"{order.Id} 支付请求 [data]({url}) ", MonsterPay_logFile);

            var api = Service.CreateRedirectData((int)PayGatewayType.MonsterPay, url);

            if (api.Result)
                return api.Extend.ToString();
            else
                return string.Empty;

        }


        /// <summary>
        /// 支付成功回调
        /// </summary>
        /// <param name="logGuid"></param>
        /// <returns></returns>
        public static PayState call(string logGuid)
        {
            try
            {
                var requestType = HttpContext.Current.Request.RequestType;
                if (requestType == "GET")
                {
                    SiteHelper.Log(logGuid, string.Format("怪兽支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), MonsterPay_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("怪兽支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), MonsterPay_logFile);
                }
                Dictionary<string, string> parameters = null;

                if (!CheckSign(logGuid, out parameters))
                {
                    SiteHelper.Log(logGuid, string.Format("怪兽支付回调非法签名"), MonsterPay_logFile);
                    return PayState.SignError;
                }

                //支付状态，1表示支付成功;
                var orderstatus = parameters.ContainsKey("status") ? parameters["status"] : string.Empty;

                //商家订单号 
                var orderid = parameters.ContainsKey("order_sn") ? parameters["order_sn"] : string.Empty;

                //订单金额
                var amount = parameters.ContainsKey("money") ? parameters["money"] : string.Empty;

                //第三方订单号
                var TradeId = parameters.ContainsKey("trade_sn") ? parameters["trade_sn"] : string.Empty;

                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(orderid));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 怪兽支付回调 訂單狀態已經完成,無須金幣充值", order.Id), MonsterPay_logFile);
                    return PayState.Success;
                }

                //進行金幣充值
                string msg = "";

                if (orderstatus.Equals("1") && ((Convert.ToDecimal(amount)) == order.Amount))
                {
                    var data = $"怪兽支付回调 完成支付: 支付订单号: {orderid} , 渠道订单号: {TradeId}, 状态: {orderstatus}";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, TradeId, data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 怪兽支付回调 金幣充值成功 支付订单号:{orderid} 金额:amount={order.Amount} 渠道订单号:{TradeId}", MonsterPay_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 怪兽支付回调 金幣充值错误 {msg}", MonsterPay_logFile);

                        Service.CDFailOrder(order.Id, "0", $"怪兽支付订单号:{orderid} , 渠道订单号: {TradeId} , 支付金额(分): {(Convert.ToDecimal(amount) * 100).ToString()}, 状态: {orderstatus}");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, $"{order.Id}  怪兽支付回调 支付無效 支付订单号:{orderid}, 渠道订单号:{TradeId}, 支付金额(分):{(Convert.ToDecimal(amount) * 100).ToString()}, 状态:{orderstatus}", MonsterPay_logFile);
                Service.CDFailOrder(order.Id, "0", $"怪兽支付订单号:{orderid} , 渠道订单号: {TradeId} , 支付金额(分): {(Convert.ToDecimal(amount) * 100).ToString()}, 状态: {orderstatus}");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("怪兽支付回调 异常错误 {0}", ex.ToString()), MonsterPay_logFile);
                return PayState.Error;
            }
        }
  

        private static bool CheckSign(string guid, out Dictionary<string, string> parameters)
        {
            parameters = PayHelper.GetParamters(MonsterPay_logFile, guid);

            var signStr = string.Join("&", parameters.OrderBy(x => x.Key).Select(m => string.Format("{0}={1}", m.Key, m.Value)));
            SiteHelper.Log(guid, $"怪兽支付回调加密参数 [data]({signStr}) ", MonsterPay_logFile);
            /**
					1)syceepay_public_key，Syceepay公钥，每个商家对应一个固定的Syceepay公钥（不是使用工具生成的密钥merchant_public_key，不要混淆），
					即为Syceepay商家后台"接口管理-接口信息"里的"SyceePay公钥"
					2)demo提供的syceepay_public_key是测试商户号691239的Syceepay公钥，请自行复制对应商户号的Syceepay公钥进行调整和替换。
			*/

            var sign = parameters.ContainsKey("sign") ? parameters["sign"] : string.Empty;
            string syceepay_public_key = MonsterPay_PublicKey;
            syceepay_public_key = PayHelper.RSAPublicKeyJava2DotNet(syceepay_public_key);
            bool result = PayHelper.ValidateRsaSign(signStr, syceepay_public_key, sign);

            
            return result;
        }
    }
}