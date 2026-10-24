using game.Entity.Pay;
using game.Infrastructure;
using Newtonsoft.Json;
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
    public class KFTPay
    {

        public class KFTResult
        {
            public string sign { get; set; }
            public string platOrderId { get; set; }
            public string tradeState { get; set; }
            public string retCode { get; set; }
            public string codeUrl { get; set; }
            public string funCode { get; set; }
            public string merId { get; set; }
            public string outTradeNo { get; set; }
        }
        public class KFTErrorResult
        {
            public string retCode { get; set; }
            public string retMsg { get; set; }
        
        }
        public class transparm
        {
          public    string amt {get;set;}
            public string body { get; set; }
            public string funCode { get; set; }
            public string funName { get; set; }
            public string notifyUrl { get; set; }
            public string orderTime { get; set; }
            public string payMethod { get; set; }
            public string platMerId { get; set; }
            public string platOrderId { get; set; }
            public string subject { get; set; }
            public string tradeTime { get; set; }
            public string sign { get; set; }
        }

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
            string[] code = tradeCode.Split(',');
            transparm tp = new transparm();
            tp.amt = (order.Amount * 100).ToString();
            tp.body = "Recharge";
            tp.funCode = code[1].ToString();
            tp.funName = "prepay";
            tp.notifyUrl = kft_shopNotifyUrl;
            tp.orderTime = "10";
            tp.payMethod = code[0].ToString();
            tp.platMerId = kft_platMerId;
            tp.platOrderId = order.Id.ToString();
            tp.subject = "Recharge";
            tp.tradeTime = unixTimestamp.ToString();
            var pay_md5sign = Md5($"amt={tp.amt}&body={tp.body}&funCode={tp.funCode}&funName={tp.funName}&notifyUrl={tp.notifyUrl}&orderTime={tp.orderTime}&payMethod={tp.payMethod}&platMerId={tp.platMerId}&platOrderId={tp.platOrderId}&subject={tp.subject}&tradeTime={tp.tradeTime}&key={kft_Key}", Encoding.UTF8);
            tp.sign = pay_md5sign;

            Dictionary<string, string> paras = new Dictionary<string, string>
            {
                { "reqJson", JsonConvert.SerializeObject(tp) }
            };


           

            var data = string.Join("&", paras.Select(m => string.Format("{0}={1}", m.Key, m.Value)));
            SiteHelper.Log(guid, $"{order.Id} 支付请求 [data]({kft_PayURL}) {data}", kft_logFile);
            var responseString = SiteHelper.HttpPost(kft_PayURL, data);
            SiteHelper.Log(guid, $"{order.Id} 支付请求结果 [response] {responseString}", kft_logFile);

            JObject res = JObject.Parse(responseString);
            if (res["retMsg"] == null)
            {
                var result = JsonConvert.DeserializeObject<KFTResult>(responseString);
                if (!result.retCode.Equals("0000"))
                {
                    var message = $"支付请求失敗({result.platOrderId}) {result.tradeState}";
                    //更新狀態
                    Service.CDFailOrder(order.Id, string.Empty, message);
                    SiteHelper.Log(guid, message, kft_logFile);
                    return String.Empty;
                }
                else
                {                    
                    Service.CDModifyTransID(order.Id, result.outTradeNo.Replace(kft_platMerId, ""));
                    return result.codeUrl;
                }

            }
            else
            {
                var result = JsonConvert.DeserializeObject<KFTErrorResult>(responseString);
                var message = $"支付请求失敗({result.retCode}) {result.retMsg}";
                //更新狀態
                Service.CDFailOrder(order.Id, string.Empty, message);
                SiteHelper.Log(guid, message, kft_logFile);
                return String.Empty;
            }

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
                Stream stream = HttpContext.Current.Request.InputStream;
                var b = new byte[stream.Length];
                stream.Read(b, 0, (int)stream.Length);
                var responseString = Encoding.UTF8.GetString(b);

                SiteHelper.Log(logGuid, string.Format("支付回调 [FormData] {0}", responseString), kft_logFile);
                JObject json = JObject.Parse(responseString);
                var platOrderId = json["platOrderId"] != null ? json["platOrderId"].ToString() : string.Empty;
                var platMerId = json["platMerId"] != null ? json["platMerId"].ToString() : string.Empty;
                var outTradeNo = json["outTradeNo"] != null ? json["outTradeNo"].ToString() : string.Empty;
                var tradeState = json["tradeState"] != null ? json["tradeState"].ToString() : string.Empty;
                var orderAmt = json["orderAmt"] != null ? json["orderAmt"].ToString() : string.Empty;
                var retCode = json["retCode"] != null ? json["retCode"].ToString() : string.Empty;
                var retMsg = json["retMsg"] != null ? json["retMsg"].ToString() : string.Empty;
                var sign = json["sign"] != null ? json["sign"].ToString() : string.Empty;
                var funCode = json["funCode"] != null ? json["funCode"].ToString() : string.Empty;


                outTradeNo = outTradeNo.Replace(kft_platMerId, "");

                //if (!CheckSign(logGuid, out parameters))
                //{
                //    SiteHelper.Log(logGuid, string.Format("客付通支付回调非法签名"), kft_logFile);
                //    return PayState.SignError;
                //}


                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(platOrderId));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 客付通支付回调 訂單狀態已經完成,無須金幣充值", order.Id), kft_logFile);
                    return PayState.Success;
                }

                //進行金幣充值
                string msg = "";

                if (tradeState.Equals("TRADE_SUCCESS") && ((Convert.ToInt64(orderAmt) / 100) == order.Amount))
                {
                    var data = $"支付回调 完成支付: 支付订单号: {platOrderId}  状态: {tradeState}";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, "0", data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 客付通支付回调 金幣充值成功 支付订单号:{platOrderId} 金额:amount={order.Amount} 渠道订单号:{outTradeNo}", kft_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 客付通支付回调 金幣充值错误 {msg}", kft_logFile);

                        Service.CDFailOrder(order.Id, "0", $"客付通支付订单号:{platOrderId} , 渠道订单号: {outTradeNo} , 支付金额(分): {orderAmt}, 状态: {tradeState}");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, $"{order.Id}  客付通支付回调 支付無效 支付订单号:{platOrderId}, 渠道订单号:{outTradeNo}, 支付金额(分):{orderAmt}, 状态:{tradeState}", kft_logFile);
                Service.CDFailOrder(order.Id, "0", $"客付通支付订单号:{platOrderId} , 渠道订单号: {outTradeNo} , 支付金额(分): {orderAmt}, 状态: {tradeState}");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("客付通支付回调 异常错误 {0}", ex.ToString()), kft_logFile);
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

            return true;

            //var sign = parameters.ContainsKey("sign") ? parameters["sign"] : string.Empty;
            //var amount = parameters.ContainsKey("amount") ? parameters["amount"] : string.Empty;
            //var orderNum = parameters.ContainsKey("orderNum") ? parameters["orderNum"] : string.Empty; //商户订单号
            //var nonce_str = parameters.ContainsKey("nonce_str") ? parameters["nonce_str"] : string.Empty; //随机码
            //var orderStatus = parameters.ContainsKey("orderStatus") ? parameters["orderStatus"] : string.Empty; //随机码

            //var cSign = Md5($"merchantNum={gpyd_merchantNum}&orderNum={orderNum}&amount={ amount.ToString()}&nonce_str={nonce_str.ToString()}&orderStatus={orderStatus}&key={gpyd__Key}", Encoding.UTF8);
            //SiteHelper.Log(guid, $"客付通支付回调签名 cSign={cSign} sign={sign}", gpyd_logFile);
            //return sign == cSign;
        }
    }
}