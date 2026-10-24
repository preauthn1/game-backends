using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static game.WebSite.PayConfig;
using game.Entity.Payment;
using game.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using example.invalid;
using example.invalid;
using System.Security.Cryptography;
using System.Text;

namespace game.WebSite.PaymentHelper
{
    public class AEPay
    {
        public static string Post(CDPayOrder order, string guid, string tradeCode, ref string message, ref int showType)
        {
            //platformID
            var platformID = SiteHelper.GetAPIResult<int>($"/Payment/GetPlatformId?UserID={order.UserID.ToString()}");
            var payList = SiteHelper.GetAPIResult<List<CDMerchantUUID>>($"/Payment/GetMerchantUuid?payTypeID={order.PayTypeID}&platformID={platformID}");
            var gwid = 0;
            var uuid = "";
            var bankCode = "";
            var currency = "CNY";
            var returnHtml = string.Empty;
            var postUrl = AEPay_Url + "/payins";
            var postMethod = "POST";

            SiteHelper.Log(guid, $"{order.Id} 支付请求 [url] {postUrl} [method] {postMethod} [auth] {AEPay_AUTH}", AEPay_logFile);

            if (payList.Count() == 0)
            {
                showType = 0;
                returnHtml = "支付通道不足";
                Service.CDFailOrder(order.Id, string.Empty, $" {returnHtml}");
            }
            else
            {
                var r = (payList.Count() > 1) ? new Random().Next(0, payList.Count() - 1) : 0;
                gwid = payList[r].GWID;
                uuid = payList[r].UUID;
                bankCode = payList[r].BankCode;
                currency = payList[r].Currencies;

                //將thrid party order no寫入Pay.OrderList.TransID
                Service.CDNewModifyTransID(order.Id, "0", gwid);

                var dictionary = new Dictionary<string, object>();
                dictionary.Add("PLAYER_ID", order.UserID.ToString());  //用戶編號
                dictionary.Add("RETURN_URL", AEPay_CallbackUrl);  //轉址至第三方金流頁面後需轉址回的URL
                dictionary.Add("REMARK", order.Id.ToString());     //備註
                dictionary.Add("ITEM_NAME", "test");      //商品名稱
                dictionary.Add("BANK_NAME", "");      //銀行名稱
                dictionary.Add("PLAYER_IP", order.IPAddress);      //用戶IP

                var dictionaryMain = new Dictionary<string, object>();
                dictionaryMain.Add("thirdPartyPaymentId", uuid.Replace("\"", ""));  //第三方金流商務號編號
                dictionaryMain.Add("bankCode", bankCode);                           //銀行代碼
                dictionaryMain.Add("currency", currency);                           //幣別
                dictionaryMain.Add("amount", order.Amount.ToString("f2"));          //代收金額；1元即為1
                dictionaryMain.Add("requestOrderNo", order.Id.ToString());          //請求方交易編號
                dictionaryMain.Add("requestNotifyUrl", AEPay_NotifyUrl);            //請求方非同步通知URL
                dictionaryMain.Add("payinInfo", dictionary);

                var json = JsonConvert.SerializeObject(dictionaryMain);

                SiteHelper.Log(guid, $"{order.Id} 支付请求 [url] {postUrl} [method] {postMethod} [auth] {AEPay_AUTH}", AEPay_logFile);
                SiteHelper.Log(guid, $"{order.Id} 支付请求 [data] {json}", AEPay_logFile);

                var str = SiteHelper.HttpClient(postUrl, postMethod, json, AEPay_AUTH);
                SiteHelper.Log(guid, $"{order.Id} 支付请求回覆 [data] {str}", AEPay_logFile);

                PayinData payin = JsonConvert.DeserializeObject<PayinData>(str); //反序列化

                Service.CDModifyTransID(order.Id, payin.orderNo);

                //判斷是否為Error Message
                if (!String.IsNullOrEmpty(payin.errorCode))
                {
                    showType = 0;
                    SiteHelper.Log(guid, $"{order.Id} 支付请求錯誤 [data] errorCode:{payin.errorCode}, msg:{payin.msg}", AEPay_logFile);

                    //更新狀態
                    Service.CDFailOrder(order.Id, string.Empty, $" errorCode:{payin.errorCode} msg:{payin.msg}");
                    returnHtml = "目前支付人员太多, 请稍后再试";
                }
                else
                {
                    //比對金流系統交易編號
                    switch (payin.gatewayType)
                    {
                        case "QRCODE":  //回傳交易網址，介接方需要將此網址轉為QR code
                            showType = 1;
                            returnHtml = payin.gatewayUrl;
                            break;

                        case "REDIRECTION":  //回傳帶有參數資訊(gatewayParam)讓介接方跳轉
                            showType = 2;
                            returnHtml = $"<form method='" + payin.gatewayMethod + "' name='form' action='" + payin.gatewayUrl + "' enctype='application/x-www-form-urlencoded'>";
                            foreach (KeyValuePair<string, string> param in payin.gatewayParam)
                            {
                                returnHtml += "<input type='hidden' id='" + param.Key + "' name='" + param.Key + "' value='" + param.Value + "'>";
                            }
                            returnHtml += "</form>";
                            break;

                        case "QRCODE_IMAGE":  //回傳圖片網址進行掃碼交易
                            showType = 3;
                            returnHtml = payin.gatewayUrl;
                            break;

                        case "HTML":  //回傳網頁內容，介接方直接顯示該內容
                            showType = 4;
                            returnHtml = payin.gatewayUrl;
                            break;
                    }
                    SiteHelper.Log(guid, $"{order.Id} 支付请求顯示 [data] {returnHtml}", AEPay_logFile);
                }
            }
            return returnHtml;
        }

        //8.代收交易非同步通知
        public static PayState Call(string logGuid)
        {
            try
            {
                Stream stream = HttpContext.Current.Request.InputStream;
                var contentLength = new byte[stream.Length];
                stream.Read(contentLength, 0, (int)stream.Length);
                var responseString = Encoding.UTF8.GetString(contentLength);

                SiteHelper.Log(logGuid, string.Format("支付回调 [FormData] {0}", responseString), AEPay_logFile);

                JObject json = JObject.Parse(responseString);
                var third_party_order_no = json["orderNo"] != null ? json["orderNo"].ToString() : string.Empty;                             //金流系統交易編號
                var third_party_payment_id = json["thirdPartyPaymentId"] != null ? json["thirdPartyPaymentId"].ToString() : string.Empty;   //第三方金流商務號編號
                var bank_code = json["bankCode"] != null ? json["bankCode"].ToString() : string.Empty;                                      //銀行代碼
                var currency = json["currency"] != null ? json["currency"].ToString() : string.Empty;                                       //幣別
                var order_amount = json["amount"] != null ? json["amount"].ToString() : string.Empty;                                       //代收金額
                var order_no = json["requestOrderNo"] != null ? json["requestOrderNo"].ToString() : string.Empty;                           //請求方交易編號  
                var trade_status = json["status"] != null ? json["status"].ToString() : string.Empty;                                       //狀態；PAYING, SUCCESS, FAILED
                var trade_time = json["createTime"] != null ? json["createTime"].ToString() : string.Empty;                                 //建立時間

                //檢查訂單狀態
                var order = Service.CDGetAEPayOrderByID(Convert.ToInt32(order_no))[0];
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 支付回调 訂單狀態已經完成,無須金幣充值", order.Id), AEPay_logFile);
                    return PayState.Success;
                }

                //進行金幣充值
                string msg = "";
                if (trade_status.Equals("SUCCESS") && ((Convert.ToDecimal(order_amount)) == order.Amount))
                {
                    var data = $"支付回调 完成支付: 支付订单号: {order_no} , 渠道订单号: {third_party_order_no}, 状态: {trade_status}";

                    if (Service.CDFinishAEOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, third_party_order_no, data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 支付回调 金幣充值成功 支付订单号:{order_no} 金额:amount={order.Amount} 渠道订单号:{third_party_order_no}", AEPay_logFile);
                        return PayState.Success;
                    }
                    SiteHelper.Log(logGuid, $"{order.Id} 支付回调 金幣充值错误 {msg}", AEPay_logFile);
                }
                SiteHelper.Log(logGuid, $"{order.Id}  支付回调 支付無效 支付订单号:{order_no}, 渠道订单号:{third_party_order_no}, 支付金额(分):{Convert.ToInt64(order_amount).ToString()}, 状态:{trade_status}", AEPay_logFile);

                Service.CDFailOrder(order.Id, "0", $"支付订单号:{order_no} , 渠道订单号: {"0"} , 支付金额(分): {Convert.ToInt64(order_amount).ToString()}, 状态: {trade_status}");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("支付回调 异常错误 {0}", ex.ToString()), AEPay_logFile);
                return PayState.Error;
            }
        }
    }
}