using game.Entity.Pay;
using game.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using static game.WebSite.PayConfig;
using Newtonsoft.Json;


namespace game.WebSite.PaymentHelper
{
    public class GaoSheng
    {
        public class GaoShengResult
        {
            public string appId { get; set; }           //商戶號
            public string outTradeNo { get; set; }      //商戶訂單號
            public string qrCode { get; set; }          //二維碼地址 or 跳轉地址
            public string resultCode { get; set; }      //回應碼(00:成功, 99:失敗)
            public string resultMsg { get; set; }       //回應描述
            public string sign { get; set; }            //簽名
            public string totalAmount { get; set; }     //交易金額
        }

        public class GaoShengCallBackResult
        {
            public string appId { get; set; }           //商戶號
            public string payType { get; set; }         //支付网关
            public string outTradeNo { get; set; }      //商戶訂單號
            public string totalAmount { get; set; }     //交易金額
            public string goodsInfo { get; set; }       //商品名称
            public string resultCode { get; set; }      //回應碼(00:成功, 99:失敗)
            public string payDate { get; set; }       //支付时间
            public string sign { get; set; }            //簽名
        }

        /// <summary>
        /// 创建订单(跳转网页链接)
        /// </summary>
        /// <param name="order"></param>
        /// <param name="guid"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string post(CDPayOrder order, string guid, string tradeCode, ref bool isQrcodeBuild, ref string message)
        {

            Random ran = new Random();

            Dictionary<string, string> paras = new Dictionary<string, string>
            {
                { "appId", Gaosheng_AppID },
                { "goodsInfo", "充值" },
                { "nonceStr",  ran.Next(100000,99999999).ToString()},
                { "notifyUrl", GaoSheng_NotifyUrl },
                { "outTradeNo", order.Id.ToString() },
                { "payType", tradeCode },
                { "requestIp", order.IPAddress },
                { "returnUrl", GaoSheng_ReturnUrl },
                { "totalAmount", (order.Amount*100).ToString() }
            };

            var data = JsonConvert.SerializeObject(paras);
            var sign = Md5(data + GaoSheng_Key, Encoding.UTF8).ToUpper();
            paras.Add("sign", sign);

            var reqData = JsonConvert.SerializeObject(paras);

            Dictionary<string, string> post_paras = new Dictionary<string, string>
            {
                { "reqData", reqData }
            };

            var post_data = string.Join("&", post_paras.Select(m => string.Format("{0}={1}", m.Key, m.Value)));
            SiteHelper.Log(guid, $"{order.Id} 高盛支付请求 [data]({GaoSheng_Url}) {post_data}", GaoSheng_logFile);
            var responseString = SiteHelper.HttpPost(GaoSheng_Url, post_data);
            SiteHelper.Log(guid, $"{order.Id} 高盛支付请求结果 [response] {responseString}", GaoSheng_logFile);

            var result = JsonConvert.DeserializeObject<GaoShengResult>(responseString);

            if (!result.resultCode.Equals("00"))
            {
                message = $"高盛支付请求失敗({result.outTradeNo}) {result.resultMsg}";

                //更新狀態
                Service.CDFailOrder(order.Id, string.Empty, message);
                SiteHelper.Log(guid, message, GaoSheng_logFile);

                return String.Empty;
            }

            //判斷是要直接顯示二為碼還是轉跳
            if (tradeCode.Equals("ZFB") || tradeCode.Equals("WX") || tradeCode.Equals("QQ") || tradeCode.Equals("JDQB") || tradeCode.Equals("YL") || tradeCode.Equals("KJ"))
            {
                isQrcodeBuild = true;
            }
            return result.qrCode;
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
                Dictionary<string, string> parameters = null;

                var requestType = HttpContext.Current.Request.RequestType;
                if (requestType == "GET")
                {
                    SiteHelper.Log(logGuid, string.Format("高盛支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), GaoSheng_logFile);
                    parameters = HttpContext.Current.Request.QueryString.AllKeys.Where(x => !string.IsNullOrEmpty(x)).Cast<string>()
                    .ToDictionary(k => k, v => HttpContext.Current.Request[v]).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("高盛支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), GaoSheng_logFile);
                    parameters = HttpContext.Current.Request.Form.AllKeys.Where(x => !string.IsNullOrEmpty(x)).Cast<string>()
                   .ToDictionary(k => k, v => HttpContext.Current.Request[v]).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
                }
                
                var reqData = parameters.ContainsKey("reqData") ? parameters["reqData"] : string.Empty;
                var callbackResult = JsonConvert.DeserializeObject<GaoShengCallBackResult>(reqData);


                Dictionary<string, string> paras = new Dictionary<string, string>();
                paras.Add("appId", callbackResult.appId);
                paras.Add("goodsInfo", callbackResult.goodsInfo);
                paras.Add("outTradeNo", callbackResult.outTradeNo);
                paras.Add("payDate", callbackResult.payDate);
                paras.Add("payType", callbackResult.payType);
                paras.Add("resultCode", callbackResult.resultCode);
                paras.Add("totalAmount", callbackResult.totalAmount);

                //md5加密 & 驗簽
                var dataSign = JsonConvert.SerializeObject(paras);
                var cSign = Md5(dataSign + GaoSheng_Key, Encoding.UTF8).ToUpper();
                SiteHelper.Log(logGuid, string.Format("支付回调签名 cSign={0} sign={1} srt={2}", cSign, callbackResult.sign, dataSign), GaoSheng_logFile);
                
                if (callbackResult.sign != cSign)
                {
                    SiteHelper.Log(logGuid, string.Format("高盛支付回调非法签名"), GaoSheng_logFile);
                    return PayState.SignError;
                }
                
                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(callbackResult.outTradeNo));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 高盛支付回调 訂單狀態已經完成,無須金幣充值", order.Id), GaoSheng_logFile);
                    return PayState.Success;
                }

                //進行金幣充值
                string msg = "";

                if (callbackResult.resultCode.Equals("00") && (Convert.ToInt64(callbackResult.totalAmount) == order.Amount * 100))
                {
                    var data = $"高盛支付回调 完成支付: 支付订单号: {callbackResult.outTradeNo} , 渠道订单号: {"0"}, 状态: {callbackResult.resultCode}";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, "0", data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 高盛支付回调 金幣充值成功 支付订单号:{callbackResult.outTradeNo} 金额:amount={order.Amount} 渠道订单号:{"0"}", GaoSheng_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 高盛支付回调 金幣充值错误 {msg}", GaoSheng_logFile);

                        Service.CDFailOrder(order.Id, callbackResult.outTradeNo, $"高盛支付订单号:{callbackResult.outTradeNo} , 渠道订单号: {"0"} , 支付金额(分): {callbackResult.totalAmount}, 状态: {callbackResult.resultCode}");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, $"{order.Id}  高盛支付回调 支付無效 支付订单号:{callbackResult.outTradeNo}, 渠道订单号:{"0"}, 支付金额(分):{callbackResult.totalAmount}, 状态:{callbackResult.resultCode}", GaoSheng_logFile);
                Service.CDFailOrder(order.Id, callbackResult.outTradeNo, $"高盛支付订单号:{callbackResult.outTradeNo} , 渠道订单号: {"0"} , 支付金额(分): {callbackResult.totalAmount}, 状态: {callbackResult.resultCode}");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("高盛支付回调 异常错误 {0}", ex.ToString()), GaoSheng_logFile);
                return PayState.Error;
            }
        }


        private static string Md5(string str, Encoding ec)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            return BitConverter.ToString(md5.ComputeHash(ec.GetBytes(str))).Replace("-", "");
        }
        
    }
}
