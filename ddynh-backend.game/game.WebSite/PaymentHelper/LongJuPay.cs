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
using game.WebSite.Services;

namespace game.WebSite.PaymentHelper
{
    public class LongJuPay
    {
        public static string Post(CDPayOrder order, string guid, string tradeCode, ref string outMessage)
        {
            string merchantUrl = string.Empty;  //API 网址
            string returnUrl = string.Empty;
            string productID = string.Empty;    //店家商品代号

            var payInfoList = MerchantDataService.GetMerchantData(order);

            //訂單編號處理
            var tradeID = ThirdPartyOderIDService.GetThirdPartyOderID(order.Id, LongJuPay_Prefix);

            if (string.IsNullOrEmpty(payInfoList.InfoData))
            {
                SiteHelper.Log(guid, $"{order.Id} 支付请求失敗", LongJuPay_LogFile);
                outMessage = "支付通道不足";
                Service.CDFailOrder(order.Id, string.Empty, $" {outMessage}");
            }
            else
            {
                var gwid = payInfoList.GWID;
                var mainUrl = payInfoList.Url;
                var bankCode = payInfoList.BankCode;
                var infoData = JObject.Parse(payInfoList.InfoData);

                //將thrid party order no寫入Pay.OrderList.TransID
                Service.CDNewModifyTransID(order.Id, "0", gwid);

                var merchantNo = infoData["MERCHANT_NO"] != null ? infoData["MERCHANT_NO"].ToString() : string.Empty;      //商户代码
                var merchantKey = infoData["MERCHANT_KEY"] != null ? infoData["MERCHANT_KEY"].ToString() : string.Empty;               //加密密钥
                mainUrl += "/customer.pay";
                switch (bankCode)
                {
                    case "ZHIFUBAO_QR":
                        productID = "WG_CARD";        //支付宝 H5
                        break;
                    case "QUICKPASS_H5":
                        productID = "H5";      //银联云闪付H5
                        break;
                }
                var amount = order.Amount * SystemConst.CashAmountRate;
                Dictionary<string, string> paras = new Dictionary<string, string>
                {
                    { "ServerUrl", LongJuPay_NotifyUrl },       //异步返回的url
                    { "amount", amount.ToString() },      //金额; 代付金额(单位 分)
                    { "backurl", LongJuPay_NotifyUrl },         //同步返回的url
                    { "businessnumber", tradeID },         //商品订单号; 商品订单号（['A~Z,a~z,0~9']组成的10到64位字符串）
                    { "failUrl", LongJuPay_NotifyUrl },         //同步返回的url; 支付失败的同步返回的url
                    { "goodsName", bankCode },                  //商品名称（描述）,建议使用英文
                    { "mer_id", merchantNo },                //商户号
                    { "terminal", productID },                 //支付类型
                    { "timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },       //请求时间; 时间戳,格式yyyy-MM-dd HH:mm:ss
                    { "version", "01" }                         //版本号
                };

                string parasData = string.Join("&", paras.Select((m => $"{m.Key}={m.Value}"))) + "&" + merchantKey;

                SiteHelper.Log(guid, $"{order.Id} md5 [before] {parasData} [secret_key]{merchantKey}", LongJuPay_LogFile);
                var pay_md5sign = MD5Service.Hash(parasData, Encoding.UTF8).ToUpper();
                SiteHelper.Log(guid, $"{order.Id} md5 [after] {pay_md5sign} ", LongJuPay_LogFile);

                paras.Add("sign", pay_md5sign);
                paras.Add("sign_type", "md5");

                //网关与快捷跳转支付页面,其他支付方式返回json数据
                if (bankCode == "WANGGUAN_ONLINE_BANK" || bankCode == "EXPRESSPAY_QR" || bankCode == "EXPRESSPAY_H5")
                {
                    returnUrl = $"<html><body><form method='post' name='form' action='{mainUrl}' enctype=\"application/x-www-form-urlencoded\">" +
                          string.Join("", paras.Select(m => $"<input type='hidden' name='{m.Key}' value='{m.Value}' >")) +
                          "</form><script>window.onload = function(){document.forms['form'].submit();}</script></body></html>";
                }

                //轉換josn
                var data = JsonConvert.SerializeObject(paras);
                SiteHelper.Log(guid, $"{order.Id} 支付请求內容 [request] {data}", LongJuPay_LogFile);

                //呼叫支付平台
                var responseString = SiteHelper.HttpPost(mainUrl, data, "application/json");
                SiteHelper.Log(guid, string.Format("{0} 支付请求结果 [response] {1}", order.Id, responseString), LongJuPay_LogFile);

                JObject json = JObject.Parse(responseString);

                var result = json["result"] != null ? json["result"].ToString() : string.Empty;   //返回码; 请求结果标志, success：请求成功, error：请求失败
                var code = json["code"] != null ? json["code"].ToString() : string.Empty;  //返回码描述; MSG_OK 请求成功
                var message = json["msg"] != null ? json["msg"].ToString() : string.Empty;
                var responseData = json["data"] != null ? json["data"].ToString() : string.Empty;

                if (!result.ToLower().Equals("success"))
                {
                    outMessage = message;
                    
                    //更新狀態
                    Service.CDFailOrder(order.Id, string.Empty, $"{result} {code} {outMessage}");
                    SiteHelper.Log(guid, $"{order.Id} 支付请求失敗 [code]{code} [outMessage]{outMessage}", LongJuPay_LogFile);
                }
                else
                {
                    JObject resData;
                    try
                    {
                        resData = JObject.Parse(responseData);
                    }
                    catch
                    {
                        resData = null;
                    }

                    var resMerId = resData["mer_id"] != null ? resData["mer_id"].ToString() : string.Empty;  //平台分给商户的编号
                    var resBusinessnumber = resData["businessnumber"] != null ? resData["businessnumber"].ToString() : string.Empty;  //商户该笔订单号，30位字符串
                    var resStatus = json["status"] != null ? resData["status"].ToString() : string.Empty;  //成功，失败
                    var resDescript = resData["transactiondate"] != null ? resData["transactiondate"].ToString() : string.Empty;  //订单发生的时间
                    var resAmount = resData["amount"] != null ? resData["amount"].ToString() : string.Empty;  //订单发生的金额
                    var resRealAmount = resData["real_amount"] != null ? resData["real_amount"].ToString() : string.Empty;  //商户实际到账金额，即为扣除手续费以后实际到账的金额
                    var resTransactiontype = resData["transactiontype"] != null ? resData["transactiontype"].ToString() : string.Empty;  //订单发生的类型，代收，代付
                    var resInputdate = resData["inputdate"] != null ? resData["inputdate"].ToString() : string.Empty;  //订单创建时间
                    var resTradeQrcode = resData["trade_qrcode"] != null ? resData["trade_qrcode"].ToString() : string.Empty;  //交易地址
                    var resRemark = resData["remark"] != null ? resData["remark"].ToString() : string.Empty;

                    returnUrl = resTradeQrcode.Replace("\\", "");
                }
            }
            return returnUrl;
        }

        public static PayState Call(string logGuid)
        {
            int orderID = 0;
            try
            {
                var requestType = HttpContext.Current.Request.RequestType;
                Dictionary<string, string> parameters = null;
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

                SiteHelper.Log(logGuid, $"支付回调 [FormData] {string.Join("&", parameters.Select(m => $"{m.Key}={m.Value}"))}", LongJuPay_LogFile);

                //去除前綴後取得實際orderid
                var merchantOrderId = parameters.ContainsKey("businessnumber") ? parameters["businessnumber"] : string.Empty;
                orderID = Convert.ToInt32(merchantOrderId.ToString().Replace(LongJuPay_Prefix, ""));

                //取得merchantNo & merchantKey
                var orderList = Service.CDGetAEPayOrderByID(orderID);
                if (orderList.Count == 0)
                {
                    SiteHelper.Log(logGuid, $"{orderID} 支付回调 查無訂單", LongJuPay_LogFile);
                    return PayState.Error;
                }
                var order = orderList[0];
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, $"{order.Id} 支付回调 訂單狀態已經完成,無須金幣充值", LongJuPay_LogFile);
                    return PayState.Success;
                }
                var payInfoList = MerchantDataService.GetMerchantData(order);

                JObject infoData;
                try
                {
                    infoData = JObject.Parse(payInfoList.InfoData);
                }
                catch { infoData = null; }
                var merchantNo = infoData["MERCHANT_NO"] != null ? infoData["MERCHANT_NO"].ToString() : string.Empty;      //商户代码
                var merchantKey = infoData["MERCHANT_KEY"] != null ? infoData["MERCHANT_KEY"].ToString() : string.Empty;               //加密密钥

                //加密
                var sign = parameters.ContainsKey("sign") ? parameters["sign"] : string.Empty;
                parameters.Remove("sign");
                parameters.Remove("sign_type");

                var md5String = string.Join("&", parameters.Select(m => $"{m.Key}={m.Value}")) + "&" + merchantKey;

                if (!CheckSign(logGuid, sign, md5String))
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调非法签名"), LongJuPay_LogFile);
                    return PayState.SignError;
                }
                else
                {
                    var result = parameters.ContainsKey("result") ? parameters["result"] : string.Empty;
                    var code = parameters.ContainsKey("code") ? parameters["code"] : string.Empty;
                    var msg = parameters.ContainsKey("msg") ? parameters["msg"] : string.Empty;
                    var mer_id = parameters.ContainsKey("mer_id") ? parameters["mer_id"] : string.Empty;
                    var status = parameters.ContainsKey("status") ? parameters["status"] : string.Empty;
                    var transactiondate = parameters.ContainsKey("transactiondate") ? parameters["transactiondate"] : string.Empty;
                    var amount = parameters.ContainsKey("amount") ? parameters["amount"] : string.Empty;
                    var real_amount = parameters.ContainsKey("real_amount") ? parameters["real_amount"] : string.Empty;
                    var transactiontype = parameters.ContainsKey("transactiontype") ? parameters["transactiontype"] : string.Empty;
                    var inputdate = parameters.ContainsKey("inputdate") ? parameters["inputdate"] : string.Empty;
                    var remark = parameters.ContainsKey("remark") ? parameters["remark"] : string.Empty;

                    if (!result.ToLower().Equals("success"))
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 支付回调 订单失敗 code{code} msg{msg} responseMsg:{remark}", LongJuPay_LogFile);
                        Service.CDFailOrder(order.Id, merchantOrderId, $"{status} {remark}");
                        return PayState.Error;
                    }
                    else if ((Convert.ToDouble(amount) / SystemConst.CashAmountRate) != order.Amount)
                    {
                        SiteHelper.Log(logGuid, string.Format($"{order.Id}  支付回调 支付無效 merchantOrderId:{merchantOrderId},totalAmount:{amount}"), LongJuPay_LogFile);
                        Service.CDFailOrder(order.Id, merchantOrderId, $"responseCode:{status}, responseMsg:{remark}");
                        return PayState.Error;
                    }
                    else
                    {
                        var data = $"完成支付";

                        if (Service.CDFinishAEOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, merchantOrderId, data, ref msg) == true)
                        {
                            SiteHelper.Log(logGuid, $"{order.Id} 支付回调 金幣充值成功 merchantOrderId,:{merchantOrderId}, totalAmount:{amount}", LongJuPay_LogFile);
                            return PayState.Success;
                        }
                        else
                        {
                            SiteHelper.Log(logGuid, $"{order.Id} 支付回调 金幣充值错误 {msg}", LongJuPay_LogFile);
                            Service.CDFailOrder(order.Id, merchantOrderId, $"{msg}");
                            return PayState.Error;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = $"[orderID]{orderID}[LogID]{logGuid}[message]{ex.ToString()}";

                MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
                {
                    Channel = SlackChannelEnum.n_api_payment,
                    Message = msg
                });

                SiteHelper.Log(logGuid, string.Format("支付回调 异常错误 {0}", ex.ToString()), LongJuPay_LogFile);
                return PayState.Error;
            }
        }

        private static bool CheckSign(string guid, string sign, string md5String)
        {
            var cSign = MD5Service.Hash(md5String, Encoding.UTF8).ToUpper();
            SiteHelper.Log(guid, $"回调签名 cSign={cSign} sign={sign} srt={md5String}", LongJuPay_LogFile);
            return sign == cSign;
        }
    }
}