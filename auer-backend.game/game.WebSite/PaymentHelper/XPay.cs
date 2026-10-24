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

namespace game.WebSite.PaymentHelper
{
    public class XPay
    {
        public static string post(CDPayOrder order, string guid, string tradeCode, ref string message)
        {
            string url = string.Empty;

            Dictionary<string, string> paras = new Dictionary<string, string>
            {
                { "appId", XPay_appId },
                { "mchId", XPay_mchId },
                { "mchOrderNo", order.Id.ToString() },
                { "currency", "cny" },
                { "productId", tradeCode },
                { "amount", (order.Amount * 100).ToString() },
                { "body", "充值" },
                { "subject", "XPAY支付" },
                { "notifyUrl", XPay_CallbackUrl }
            };
            
            string parasData = string.Join("&", paras.OrderBy(x => x.Key).Select((m => $"{m.Key}={m.Value}")));
            var pay_md5sign = Md5(parasData + "&key=" + XPay_key, Encoding.UTF8).ToUpper();           
            paras.Add("sign", pay_md5sign); 
                
            var data = "params=" + JsonConvert.SerializeObject(paras);
            SiteHelper.Log(guid, string.Format("{0} 支付请求 [data]({1}) {2}", order.Id, XPay_PayURL, data), XPay_logFile);

            //呼叫支付平台
            var responseString = SiteHelper.HttpPost(XPay_PayURL, data);
            SiteHelper.Log(guid, string.Format("{0} 支付请求结果 [response] {1}", order.Id, responseString), XPay_logFile);

            JObject json = JObject.Parse(responseString);
            var retCode = json["retCode"] != null ? json["retCode"].ToString() : string.Empty;    

            if (!retCode.Equals("SUCCESS"))
            {
                var respmsg = json["retMsg"] != null ? json["retMsg"].ToString() : string.Empty;               
                message = string.Format("{0} 支付请求失敗 {1}", order.Id, respmsg);

                //更新狀態
                Service.CDFailOrder(order.Id, string.Empty, respmsg);
                SiteHelper.Log(guid, message, XPay_logFile);
            }
            else
            {      
                var payOrderId = json["payOrderId"] != null ? json["payOrderId"].ToString() : string.Empty;

                Service.CDModifyTransID(order.Id, payOrderId);
                if(json["payParams"]["payJumpUrl"] != null)
                    url = json["payParams"]["payJumpUrl"] != null ? json["payParams"]["payJumpUrl"].ToString() : string.Empty;
                else
                    url = json["payParams"]["payUrl"] != null ? json["payParams"]["payUrl"].ToString() : string.Empty;
            }

            return url;
        }

        public static PayState call(string logGuid)
        {
            try
            {
                var requestType = HttpContext.Current.Request.RequestType;
                if (requestType == "GET")
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), XPay_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), XPay_logFile);
                }

                Dictionary<string, string> parameters = null;

                if (!CheckSign(logGuid, out parameters))
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调非法签名"), XPay_logFile);
                    return PayState.SignError;
                }
                
                var payOrderId = parameters.ContainsKey("payOrderId") ? parameters["payOrderId"] : string.Empty;
                var mchId = parameters.ContainsKey("mchId") ? parameters["mchId"] : string.Empty;
                var appId = parameters.ContainsKey("appId") ? parameters["appId"] : string.Empty;
                var productId = parameters.ContainsKey("productId") ? parameters["productId"] : string.Empty;
                var mchOrderNo = parameters.ContainsKey("mchOrderNo") ? parameters["mchOrderNo"] : string.Empty;
                var amount = parameters.ContainsKey("amount") ? parameters["amount"] : string.Empty;
                var status = parameters.ContainsKey("status") ? parameters["status"] : string.Empty;
                var paySuccTime = parameters.ContainsKey("paySuccTime") ? parameters["paySuccTime"] : string.Empty;
                var backType = parameters.ContainsKey("backType") ? parameters["backType"] : string.Empty;
                //var sign = parameters.ContainsKey("sign") ? parameters["sign"] : string.Empty;

                var channelOrderNo = parameters.ContainsKey("channelOrderNo") ? parameters["channelOrderNo"] : string.Empty;
                var channelAttach = parameters.ContainsKey("channelAttach") ? parameters["channelAttach"] : string.Empty;


                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(mchOrderNo));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 支付回调 訂單狀態已經完成,無須金幣充值", order.Id), XPay_logFile);
                    return PayState.Success;
                }

                if (status == "1")
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 支付回调 订单生成: {1}", order.Id, payOrderId), XPay_logFile);
                    return PayState.Error;
                }

                //進行金幣充值
                string msg = "";
                
                if ((status == "2" || status == "3") && (Convert.ToInt64(amount) == order.Amount * 100))
                {
                    var data = $"支付回调 完成支付: 支付订单号: {payOrderId} , 渠道订单号: {channelOrderNo}, 状态: {status}";
                    
                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, payOrderId, data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, string.Format("{0} 支付回调 金幣充值成功 支付订单号:{1} 金额:amount={2} 渠道订单号:{3}", order.Id, payOrderId, order.Amount, channelOrderNo), XPay_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, string.Format("{0} 支付回调 金幣充值错误 {1}", order.Id, msg), XPay_logFile);
                        Service.CDFailOrder(order.Id, payOrderId, $"支付订单号:{payOrderId} , 渠道订单号: {channelOrderNo} , 支付金额(分): {amount}, 状态: {status}");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, string.Format("{0}  支付回调 支付無效 支付订单号:{1}, 渠道订单号:{2}, 支付金额(分):{3}, 状态:{4}", order.Id, payOrderId, channelOrderNo, amount, status), XPay_logFile);
                Service.CDFailOrder(order.Id, payOrderId, $"支付订单号:{payOrderId} , 渠道订单号: {channelOrderNo} , 支付金额(分): {amount}, 状态: {status}");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("支付回调 异常错误 {0}", ex.ToString()), XPay_logFile);
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

            if (parameters.ContainsKey("params"))
            {
                parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(parameters["params"]);
            }
           
            var sign = parameters["sign"];
            parameters.Remove("sign");
            parameters = parameters.Where(o=> !string.IsNullOrEmpty(o.Value)).OrderBy(o => o.Key).ToDictionary(o => o.Key, p => p.Value);            
            
            var str = string.Join("&", parameters.Select(m => string.Format("{0}={1}", m.Key, m.Value))) + "&key=" + XPay_key;
            var cSign = Md5(str, Encoding.UTF8).ToUpper();
            SiteHelper.Log(guid, string.Format("支付回调签名 cSign={0} sign={1} srt={2}", cSign, sign, str), XPay_logFile);

            return sign == cSign;
        }
    }

}
