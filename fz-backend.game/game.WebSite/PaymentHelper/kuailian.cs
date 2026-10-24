using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using game.Entity.Pay;
using game.Infrastructure;
using Newtonsoft.Json;
using static game.WebSite.PayConfig;

namespace game.WebSite.PaymentHelper
{
    public class kuailian
    {

        public class kuailianResult
        {
            public string merchantNo { get; set; }
            public string orderAmount { get; set; }
            public string orderNo { get; set; }
            public string payUrl { get; set; }
            public string msg { get; set; }
            public string sign { get; set; }
            public string code { get; set; }
        }


        public static string post(CDPayOrder order, string guid, string tradeCode, ref bool isQrcodeBuild, ref string message)
        {
            Dictionary<string, string> paras = new Dictionary<string, string>
            {
                { "merchantNo", kuailian_mchid },
                { "orderNo", order.Id.ToString() },
                { "orderAmount", (order.Amount * 100).ToString()  },
                { "payType", tradeCode },
                { "notifyUrl", kuailian_notifyUrl},
                { "callbackUrl", kuailian_notifyUrl}
            };

            var parasData = string.Join("&", paras.OrderBy(x => x.Key).Select((m => $"{m.Key}={m.Value}")));
            var pay_md5sign = Md5(parasData + kuailian_Key, Encoding.UTF8).ToLower();
            paras.Add("sign", pay_md5sign);

            var data = string.Join("&", paras.Select(m => string.Format("{0}={1}", m.Key, m.Value)));

            SiteHelper.Log(guid, $"{order.Id} 快联支付请求 [data]({kuailian_PayURL}) {data}", kuailian_logFile);
            var responseString = SiteHelper.HttpPost(kuailian_PayURL, data);

            SiteHelper.Log(guid, $"{order.Id} 快联支付请求结果 [response] {responseString}", kuailian_logFile);

            var result = JsonConvert.DeserializeObject<kuailianResult>(responseString);

            if (!result.code.Equals("0000"))
            {
                message = $"快联支付请求失敗({result.orderNo}) {result.msg}";

                //更新狀態
                Service.CDFailOrder(order.Id, string.Empty, message);
                SiteHelper.Log(guid, message, kuailian_logFile);

                return String.Empty;
            }


            //支付方式 (名字带有【扫码】【二维码】的方式，返回的payUrl鏈接要自已主动生成二维码！)
            //參考 https://example.invalid/page/1251064867691339
            if (tradeCode.Equals("2") || tradeCode.Equals("4") || tradeCode.Equals(8) || tradeCode.Equals("9") || tradeCode.Equals("14") || tradeCode.Equals("15"))
            {
                isQrcodeBuild = true;
            }
            return result.payUrl;
        }

        public static PayState call(string logGuid)
        {
            try
            {
                var requestType = HttpContext.Current.Request.RequestType;
                if (requestType == "GET")
                {
                    SiteHelper.Log(logGuid, string.Format("快联支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), kuailian_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("快联支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), kuailian_logFile);
                }
                Dictionary<string, string> parameters = null;

                if (!CheckSign(logGuid, out parameters))
                {
                    SiteHelper.Log(logGuid, string.Format("快联支付回调非法签名"), kuailian_logFile);
                    return PayState.SignError;
                }

                var merchantNo = parameters.ContainsKey("merchantNo") ? parameters["merchantNo"] : string.Empty;
                var orderAmount = parameters.ContainsKey("orderAmount") ? parameters["orderAmount"] : string.Empty;
                var orderNo = parameters.ContainsKey("orderNo") ? parameters["orderNo"] : string.Empty;
                var wtfOrderNo = parameters.ContainsKey("wtfOrderNo") ? parameters["wtfOrderNo"] : string.Empty;
                var orderStatus = parameters.ContainsKey("orderStatus") ? parameters["orderStatus"] : string.Empty;
                var payTime = parameters.ContainsKey("payTime") ? parameters["payTime"] : string.Empty;

                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(orderNo));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 快联支付回调 訂單狀態已經完成,無須金幣充值", order.Id), kuailian_logFile);
                    return PayState.Success;
                }

                if (orderStatus.Equals("INIT") || orderStatus.Equals("NOTPAY") || orderStatus.Equals("CANCEL") || orderStatus.Equals("REFUND"))
                {
                    //以上狀態皆當成成功
                    SiteHelper.Log(logGuid, string.Format("{0} 快联支付回调 订单生成: {1}", order.Id, wtfOrderNo), kuailian_logFile);
                    return PayState.Success;
                }

                //進行金幣充值
                string msg = "";

                if (orderStatus.Equals("SUCCESS") && (Convert.ToInt64(orderAmount) == order.Amount * 100))
                {
                    var data = $"快联支付回调 完成支付: 支付订单号: {orderNo} , 渠道订单号: {wtfOrderNo}, 状态: {orderStatus}";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, wtfOrderNo, data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 快联支付回调 金幣充值成功 支付订单号:{orderNo} 金额:amount={order.Amount} 渠道订单号:{wtfOrderNo}", kuailian_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 快联支付回调 金幣充值错误 {msg}", kuailian_logFile);

                        Service.CDFailOrder(order.Id, orderNo, $"快联支付订单号:{orderNo} , 渠道订单号: {wtfOrderNo} , 支付金额(分): {orderAmount}, 状态: {orderStatus}");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, $"{order.Id}  快联支付回调 支付無效 支付订单号:{orderNo}, 渠道订单号:{wtfOrderNo}, 支付金额(分):{orderAmount}, 状态:{orderStatus}", kuailian_logFile);
                Service.CDFailOrder(order.Id, orderNo, $"快联支付订单号:{orderNo} , 渠道订单号: {wtfOrderNo} , 支付金额(分): {orderAmount}, 状态: {orderStatus}");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("快联支付回调 异常错误 {0}", ex.ToString()), kuailian_logFile);
                return PayState.Error;
            }

        }
        private static string Md5(string str, Encoding ec)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            return BitConverter.ToString(md5.ComputeHash(ec.GetBytes(str))).Replace("-", "");
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
            parameters = parameters.OrderBy(o => o.Key).ToDictionary(o => o.Key, p => p.Value);

            var str = string.Join("&", parameters.Select(m => string.Format("{0}={1}", m.Key, m.Value))) + kuailian_Key;
            var cSign = Md5(str, Encoding.UTF8).ToLower();

            SiteHelper.Log(guid, string.Format("支付回调签名 cSign={0} sign={1} srt={2}", cSign, sign, str), kuailian_logFile);
            return sign == cSign;
        }
    }
}