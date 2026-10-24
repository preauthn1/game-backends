using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using game.Entity.Payment;
using game.Infrastructure;
using static game.WebSite.PayConfig;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using game.Infrastructure.Utility.ThirdParty;

namespace game.WebSite.PaymentHelper
{
    public class DynasPay
    {
        public static string Post(CDPayOrder order, string guid, string tradeCode, ref string message)
        {
            string postUrl = DynasPay_Url;  //API 网址
            string returnUrl = string.Empty;
            string productID = string.Empty;    //店家商品代号
            var url = string.Empty;

            var merTradeID = "000000000000" + order.Id.ToString();   //店家自行设定，不得小于 6 个字符，不得重复
            merTradeID = "DYNAS" + merTradeID.Substring((merTradeID.Length - 10));

            //platformID
            var platformID = SiteHelper.GetAPIResult<int>($"/Payment/GetPlatformId?UserID={order.UserID.ToString()}");
            var payList = SiteHelper.GetAPIResult<List<CDMerchantUUID>>($"/Payment/GetMerchantUuid?payTypeID={order.PayTypeID}&platformID={platformID}");

            if (payList.Count() == 0)
            {
                SiteHelper.Log(guid, $"{order.Id} 支付请求 [url] {postUrl} ", DynasPay_logFile);
                message = "支付通道不足";
                Service.CDFailOrder(order.Id, string.Empty, $" {message}");
            }
            else
            {
                var r = (payList.Count() > 1) ? new Random().Next(0, payList.Count() - 1) : 0;
                var gwid = payList[r].GWID;
                var mainUrl = payList[r].Url;
                var bankCode = payList[r].BankCode;
                var InfoData = JObject.Parse(payList[r].InfoData);

                //將thrid party order no寫入Pay.OrderList.TransID
                Service.CDNewModifyTransID(order.Id, merTradeID, gwid);

                var hashKey = InfoData["HASHKEY"] != null ? InfoData["HASHKEY"].ToString() : string.Empty;           //金流系統交易編號
                var hashIv = InfoData["HASHIV"] != null ? InfoData["HASHIV"].ToString() : string.Empty;              //金流系統交易編號
                var VerifyCode = InfoData["VERIFYCODE"] != null ? InfoData["VERIFYCODE"].ToString() : string.Empty;  //金流系統交易編號

                switch (bankCode)
                {
                    //支付寶支付介接
                    case "ZHIFUBAO_QR":
                        postUrl += "/AliPayment.php";
                        productID = "ZHIFUBAO_QR";
                        break;
                    //微信支付介接
                    case "WEIXIN_QR":
                    case "WEIXIN_H5":
                        postUrl += "/WecahtPayment.php";
                        productID = "WEIXIN_H5";
                        break;
                    //云闪付支付介接
                    case "QUICKPASS_QR":
                        postUrl += "/YsfPayment.php";
                        productID = "QUICKPASS_QR";
                        break;
                    ///支付宝转卡支付介接
                    case "ZHIFUBAO_H5":
                        postUrl += "/UnionPayment.php";
                        productID = "ZHIFUBAO_H5";
                        break;
                }

                Dictionary<string, string> paras = new Dictionary<string, string>
                {
                    { "MerTradeID",  merTradeID},
                    { "MerProductID", productID },
                    { "MerUserID", order.UserID.ToString() },
                    { "Amount", order.Amount.ToString() },
                    { "ValidateKey", VerifyCode}  //DynasPay_VerifyCode
                };

                string parasData = string.Join("&", paras.Select((m => $"{m.Key}={m.Value}")));

                SiteHelper.Log(guid, $"{order.Id} md5 [before] {parasData} ", DynasPay_logFile);
                var pay_md5sign = MD5Service.Hash(parasData, Encoding.UTF8);
                SiteHelper.Log(guid, $"{order.Id} md5 [after] {pay_md5sign} ", DynasPay_logFile);

                paras.Add("HashKey", hashKey);     //DynasPay_HashKey
                paras.Add("HashIV", hashIv);      //DynasPay_HashIv
                paras.Remove("ValidateKey");
                paras.Add("VerifyCode", pay_md5sign.ToLower());

                var data = string.Join("&", paras.Select((m => $"{m.Key}={m.Value}")));
                SiteHelper.Log(guid, string.Format("{0} 支付请求 [data]({1}) {2}", order.Id, postUrl, data), DynasPay_logFile);

                url = $"<html><body><form method='post' name='form' action='{postUrl}' enctype=\"application/x-www-form-urlencoded\">" +
                          string.Join("", paras.Select(m => string.Format("<input type='hidden' name='{0}' value='{1}' >", m.Key, m.Value))) +
                          "</form><script>window.onload = function(){document.forms['form'].submit();}</script></body></html>";
                SiteHelper.Log(guid, $"{order.Id} 支付请求 [data]{url}", DynasPay_logFile);

                /** //other
                var data = JsonConvert.SerializeObject(paras);
                SiteHelper.Log(guid, string.Format("{0} 支付请求 [data]({1}) {2}", order.Id, postUrl, data), DynasPay_logFile);
                var responseString = SiteHelper.HttpClient(postUrl, "POST", data);
                SiteHelper.Log(guid, $"{order.Id} 支付请求回覆 [url]({postUrl}) [data]{url}", DynasPay_logFile);

                //<center>錯誤：商户离线,请联系客服(商户离线, 请联系客服) </center>
                if (responseString.StartsWith("<center>") && responseString.EndsWith("</center>"))
                {
                    //將thrid party order no寫入Pay.OrderList.TransID
                    Service.CDNewModifyTransID(order.Id, "", gwid);
                    //更新狀態
                    Service.CDFailOrder(order.Id, string.Empty, $" msg:{responseString.Replace("<center>", "").Replace("</center>", "")}");
                }
                else
                {
                    //將thrid party order no寫入Pay.OrderList.TransID
                }
                url = $"<html><body>{responseString}</body></html>";
                */
            }
            return url;
        }

        public static PayState Call(string logGuid)
        {
            int orderID = 0;
            try
            {
                var requestType = HttpContext.Current.Request.RequestType;

                SiteHelper.Log(logGuid, string.Format("支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), DynasPay_logFile);

                Dictionary<string, string> parameters = null;

                if (!CheckSign(logGuid, out parameters))
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调非法签名"), DynasPay_logFile);
                    return PayState.SignError;
                }

                var returncode = parameters.ContainsKey("RtnCode") ? parameters["RtnCode"] : string.Empty;
                var return_message = parameters.ContainsKey("RtnMessage") ? parameters["RtnMessage"] : string.Empty;
                var transaction_id = parameters.ContainsKey("MerTradeID") ? parameters["MerTradeID"] : string.Empty;
                var product_id = parameters.ContainsKey("MerProductID") ? parameters["MerProductID"] : string.Empty;
                var user_id = parameters.ContainsKey("MerUserID") ? parameters["MerUserID"] : string.Empty;
                var amount = parameters.ContainsKey("Amount") ? parameters["Amount"] : string.Empty;
                var payment_data = parameters.ContainsKey("PaymentDate") ? parameters["PaymentDate"] : string.Empty;
                var validate = parameters.ContainsKey("Validate") ? parameters["Validate"] : string.Empty;

                orderID = Convert.ToInt32(transaction_id.Substring(5));  //去除AE前綴後取得實際orderid

                //檢查訂單狀態
                var orderlist = Service.CDGetAEPayOrderByID(Convert.ToInt32(orderID));
                if (orderlist.Count == 0)
                {
                    SiteHelper.Log(logGuid, $"{orderID} 支付回调 查無訂單", DynasPay_logFile);
                    return PayState.Error;
                }
                var order = orderlist[0];
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 支付回调 訂單狀態已經完成,無須金幣充值", order.Id), DynasPay_logFile);
                    return PayState.Success;
                }

                if (returncode != "1")
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 支付回调 订单失敗 returncode:{1}", order.Id, returncode), DynasPay_logFile);
                    Service.CDFailOrder(order.Id, transaction_id, $"returncode:{returncode}");
                    return PayState.Error;
                }
                //進行金幣充值
                string msg = "";

                if (Convert.ToInt32(amount) == order.Amount)
                {
                    var data = $"完成支付";

                    if (Service.CDFinishAEOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, transaction_id, data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 支付回调 金幣充值成功 returncode:{returncode},transaction_id:{transaction_id},amount:{amount}", DynasPay_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 支付回调 金幣充值错误 {msg}", DynasPay_logFile);
                        Service.CDFailOrder(order.Id, transaction_id, $"{msg}");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, $"{order.Id}  支付回调 支付無效 returncode:{returncode},transaction_id:{transaction_id},amount:{amount}", DynasPay_logFile);
                Service.CDFailOrder(order.Id, transaction_id, $"returncode:{returncode},transaction_id:{transaction_id},amount:{amount}");
                
                return PayState.Error;
            }
            catch (Exception ex)
            {
                string msg = $"[orderID]{orderID}[LogID]{logGuid}[message]{ex.ToString()}";

                MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
                {
                    Channel = SlackChannelEnum.n_api_payment,
                    Message = msg
                });

                SiteHelper.Log(logGuid, string.Format("支付回调 异常错误 {0}", ex.ToString()), DynasPay_logFile);
                return PayState.Error;
            }
        }

        private static bool CheckSign(string guid, out Dictionary<string, string> parameters)
        {
            var requestType = HttpContext.Current.Request.RequestType;

            parameters = HttpContext.Current.Request.Form.AllKeys.Cast<string>()
                    .ToDictionary(k => k, v => HttpContext.Current.Request[v]).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);

            var sign = parameters["Validate"];

            Dictionary<string, string> paras = new Dictionary<string, string>
            {
                { "ValidateKey", DynasPay_VerifyCode},
                { "HashKey", DynasPay_HashKey},
                { "RtnCode", parameters["RtnCode"]},
                { "TradeID", parameters["MerTradeID"]},
                { "UserID", parameters["MerUserID"]},
                { "Money", parameters["Amount"]}
            };

            var str = string.Join("&", paras.Select((m => $"{m.Key}={m.Value}")));
            var cSign = MD5Service.Hash(str, Encoding.UTF8).ToLower();
            SiteHelper.Log(guid, $"支付回调签名 cSign={cSign} sign={sign} srt={str}", DynasPay_logFile);

            return sign == cSign;
        }
    }
}
 