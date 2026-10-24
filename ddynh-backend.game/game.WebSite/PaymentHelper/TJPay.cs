using System;
using example.invalid;
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
using System.Security.Cryptography.X509Certificates;
using game.Infrastructure.Utility.ThirdParty;

namespace game.WebSite.PaymentHelper
{
    public class TJPay
    {
        public static string Post(CDPayOrder order, string guid, ref string outMessage)
        {
            string merchant_url = string.Empty;  //API 网址
            string return_url = string.Empty;
            string channel = string.Empty;    //店家商品代号

            //platformID
            var platform_id = SiteHelper.GetAPIResult<int>($"/Payment/GetPlatformId?UserID={order.UserID.ToString()}");
            var pay_list = GetMerchantInfo(guid, platform_id, order.PayTypeID);

            //訂單編號處理
            var trade_id = "000000000000" + order.Id.ToString();   //店家自行设定，不得小于 6 个字符，不得重复
            trade_id = "TJPAY" + trade_id.Substring((trade_id.Length - 10));

            if (string.IsNullOrEmpty(pay_list.InfoData))
            {
                SiteHelper.Log(guid, $"{order.Id} 支付请求失敗", TJPay_LogFile);
                outMessage = "支付通道不足";
                Service.CDFailOrder(order.Id, string.Empty, $" {outMessage}");
            }
            else
            {
                var gwid = pay_list.GWID;
                var mainUrl = pay_list.Url;
                var bankCode = pay_list.BankCode;
                var infoData = JObject.Parse(pay_list.InfoData);

                //將thrid party order no寫入Pay.OrderList.TransID
                Service.CDNewModifyTransID(order.Id, "0", gwid);

                var merchant_no = infoData["MERCHANT_NO"] != null ? infoData["MERCHANT_NO"].ToString() : string.Empty;   //商户號
                var merchant_key = infoData["MERCHANT_KEY"] != null ? infoData["MERCHANT_KEY"].ToString() : string.Empty;      //商戶密鑰
                switch (bankCode)
                {
                    //支付宝 H5
                    case "ZHIFUBAO_H5":
                        channel = "3";
                        break;
                    //支付宝掃碼
                    case "ZHIFUBAO_QR":
                        channel = "2";
                        break;
                    //微信掃碼
                    case "WEIXIN_QR":
                        channel = "1";
                        break;
                }

                Dictionary<string, string> paras = new Dictionary<string, string>
                {
                    { "Amount", (order.Amount * 100).ToString() },  //订单金额
                    { "AppKey", merchant_no },  //商户号
                    { "CallBackUrl",  FinishUrl},    //跳转地址
                    { "Channel",  channel},    //通道编号
                    { "MerchantReferenceNumber", trade_id }, //平台订单号
                    { "NotifyUrl", TJPay_NotifyUrl },  //回调地址
                    { "Key", merchant_key}  //密钥
                };

                //md5(“Amount = 订单金额 & AppKey = 商户号 & CallBackUrl = 跳转地址& Channel = 通道编号 & MerchantReferenceNumber = 平台订单号 & NotifyUrl = 回调地址 & Key = 密钥”)
                string parasData = string.Join("&", paras.Select((m => $"{m.Key}={m.Value}")));
                SiteHelper.Log(guid, $"{order.Id} md5 [before] {parasData}", TJPay_LogFile);
                var md5sign = MD5Service.Hash(parasData, Encoding.UTF8).ToLower();
                SiteHelper.Log(guid, $"{order.Id} md5 [after] {md5sign} ", TJPay_LogFile);

                paras.Remove("Key");
                paras.Add("Sign", md5sign);

                //轉換josn
                var data = JsonConvert.SerializeObject(paras);
                SiteHelper.Log(guid, $"{order.Id} 支付请求內容 [request] {data}", TJPay_LogFile);

                //呼叫支付平台
                var responseString = SiteHelper.HttpPost(mainUrl, data, "application/json");


                return_url = $"<html><body><form method='post' name='form' action='{mainUrl}' enctype=\"application/x-www-form-urlencoded\">" +
                      string.Join("", paras.Select(m => $"<input type='hidden' name='{m.Key}' value='{m.Value}' >")) +
                      "</form><script>window.onload = function(){document.forms['form'].submit();}</script></body></html>";
            }
            return return_url;
        }

        public static PayState Call(string logGuid)
        {
            int orderId = 0;
            try
            {    
                Dictionary<string, string> parameters = null;

                if (CheckSign(logGuid, out parameters))
                {
                    SiteHelper.Log(logGuid, $"body:{string.Join("&", parameters.Select(m => $"{m.Key}={m.Value}"))}", TJPay_LogFile);
                    //檢查訂單狀態
                    orderId = Convert.ToInt32(parameters["MerchantReferenceNumber"].ToString().Replace("TJPAY", ""));
                    var orderlist = Service.CDGetAEPayOrderByID(Convert.ToInt32(orderId));

                    if (orderlist.Count == 0)
                    {
                        SiteHelper.Log(logGuid, $"{orderId} 支付回调 查無訂單", TJPay_LogFile);
                        return PayState.Error;
                    }
                    var order = orderlist[0];
                    if (order.Status == CDOrderStatus.Paied)
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 支付回调 訂單狀態已經完成,無須金幣充值", TJPay_LogFile);
                        return PayState.Success;
                    }

                    //進行金幣充值
                    var amount = Convert.ToInt32(parameters["OriginAmount"].ToString()) / 100;
                    var merchantOrderId = parameters["MerchantReferenceNumber"].ToString();
                    string msg = "";

                    if (Convert.ToDouble(amount) == order.Amount)
                    {
                        var data = $"完成支付";

                        if (Service.CDFinishAEOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, merchantOrderId, data, ref msg) == true)
                        {
                            SiteHelper.Log(logGuid, $"{order.Id} 支付回调 金幣充值成功 merchantOrderId,:{merchantOrderId}, totalAmount:{amount}", TJPay_LogFile);
                            return PayState.Success;
                        }
                        else
                        {
                            SiteHelper.Log(logGuid, $"{order.Id} 支付回调 金幣充值错误 {msg}", TJPay_LogFile);
                            Service.CDFailOrder(order.Id, merchantOrderId, $"{msg}");
                            return PayState.Error;
                        }
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, string.Format($"{order.Id}  支付回调無效 merchantOrderId:{merchantOrderId},totalAmount:{amount}"), TJPay_LogFile);
                        Service.CDFailOrder(order.Id, merchantOrderId, $"金额錯誤 amount:{amount}");
                        return PayState.Error;
                    }
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调非法签名"), TJPay_LogFile);
                    return PayState.SignError;
                }
            }
            catch (Exception ex)
            {
                string msg = $"[orderID]{orderId}[LogID]{logGuid}[message]{ex.ToString()}";

                MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
                {
                    Channel = SlackChannelEnum.n_api_payment,
                    Message = msg
                });

                SiteHelper.Log(logGuid, string.Format("支付回调 异常错误 {0}", ex.ToString()), TJPay_LogFile);
                return PayState.Error;
            }
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
            SiteHelper.Log(guid, $"method:{requestType} body:{string.Join("&", parameters.Select(m => $"{m.Key}={m.Value}"))}", TJPay_LogFile);

            var sign = parameters["Sign"].ToString();
            var orderNo = parameters["MerchantReferenceNumber"].ToString();

            //去除MSZPAY前綴後取得實際orderid
            int orderID = Convert.ToInt32(orderNo.Replace("TJPAY", ""));
            //取得訂單號
            var order = Service.CDGetAEPayOrderByID(orderID)[0];
            var platformId = SiteHelper.GetAPIResult<int>($"/Payment/GetPlatformId?UserID={order.UserID.ToString()}");
            var payList = GetMerchantInfo(guid, platformId, order.PayTypeID);
            var infoData = JObject.Parse(payList.InfoData);
            var merchantNo = infoData["MERCHANT_NO"] != null ? infoData["MERCHANT_NO"].ToString() : string.Empty;
            var merchantKey = infoData["MERCHANT_KEY"] != null ? infoData["MERCHANT_KEY"].ToString() : string.Empty; 

            Dictionary<string, string> paras = new Dictionary<string, string>();
            paras.Add("Amount", parameters["Amount"].ToString());
            paras.Add("AppKey", parameters["AppKey"].ToString());
            paras.Add("Channel", parameters["Channel"].ToString());
            paras.Add("MerchantReferenceNumber", orderNo);
            paras.Add("OriginAmount", parameters["OriginAmount"].ToString());
            paras.Add("Key", merchantKey);

            string str = string.Join("&", paras.Select(m => $"{m.Key}={m.Value}"));
            string cSign = MD5Service.Hash(str, Encoding.UTF8).ToLower();

            SiteHelper.Log(guid, $"支付回调签名 cSign={cSign} sign={sign} srt={str}", TJPay_LogFile);

            return sign == cSign;
        }

        private static CDMerchantUUID GetMerchantInfo(string guid, int platformId, int payTypeID)
        {
            var pay_list = SiteHelper.GetAPIResult<List<CDMerchantUUID>>($"/Payment/GetMerchantUuid?payTypeID={payTypeID}&platformID={platformId}");
            if (pay_list.Count() > 1)
            {
                var r = new Random().Next(0, pay_list.Count() - 1);
                return pay_list[r];
            }
            else if (pay_list.Count() == 1)
            {
                return pay_list[0];
            }
            else {
                return new CDMerchantUUID();
            }
        }
    }
}