using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using game.Entity.Payment;
using static game.WebSite.PayConfig;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using game.WebSite.Services;
using game.Infrastructure;
using game.Entity.DTO.ThirdPartyPay;
using game.Infrastructure.Utility.ThirdParty;

namespace game.WebSite.PaymentHelper
{
    /// <summary>
    /// 永信支付
    /// </summary>
    public class YongXinPay
    {
        public static string Post(CDPayOrder order, string guid)
        {
            string url = string.Empty;
            var payTypeObj = new YongXinPayType();
            CDMerchantUUID merchant = MerchantDataService.GetMerchantData(order);
            var InfoDataJson = JsonConvert.DeserializeObject<ThirdPartyPayMerchantInfo>(merchant.InfoData);
            var orderID = ThirdPartyOderIDService.GetThirdPartyOderID(order.Id, "YongXinPay", length: 20);
            Dictionary<string, string> paras = new Dictionary<string, string>
            {
                { "amount", order.Amount.ToString("F2") }, // 300.00 ~ 10000.00
                { "channel",    payTypeObj.GetPayType(merchant.BankCode)},
                { "merchant_number", InfoDataJson.MERCHANT_NO },
                { "notify_url",  YongXinPay_NotifyUrl },
                { "order_number", orderID },
                { "secret_key", InfoDataJson.MERCHANT_KEY },
            };

            string parasData = string.Join("&", paras.Select((m => $"{m.Key}={ Uri.EscapeDataString(m.Value)}")));
            var pay_md5sign = Md5(parasData, Encoding.UTF8);

            paras.Remove("secret_key");
            paras.Add("sign", pay_md5sign.ToLower());

            //轉換josn
            var data = JsonConvert.SerializeObject(paras);
            SiteHelper.Log(guid, $"{order.Id} 支付请求內容 [request] {data}", YongXinPay_logFile);

            //呼叫支付平台
            var responseString = SiteHelper.HttpPostJson((merchant.Url + "/" + YongXinPay_CreateOrderApiUrl), data);
            SiteHelper.Log(guid, string.Format("{0} 支付请求结果 [response] {1}", order.Id, responseString), YongXinPay_logFile);

            JObject json = JObject.Parse(responseString);
            var redirect_url = json["redirect_url"] != null ? json["redirect_url"].ToString() : string.Empty;
            var error_message = json["message"] != null ? json["message"].ToString() : string.Empty;
            SiteHelper.Log(guid, string.Format("{0} 支付请求 [data]({1}) {2}", order.Id, (merchant.Url + "/" + YongXinPay_CreateOrderApiUrl), url), YongXinPay_logFile);
            if (!redirect_url.Equals(string.Empty))
            {
                //將thrid party order no寫入Pay.OrderList.TransID
                Service.CDNewModifyTransID(order.Id, orderID, (int)merchant.GWID);
                return redirect_url;
            }
            return url;
        }

        public static PayState Call(string logGuid, YongXinPayResponseModel model)
        {
            int orderID = 0;
            try
            {
                var payCallAction = new ThirdPartyPaymentCallAction(logGuid, YongXinPay_logFile);
                //非json使用此方法紀錄回傳值
                string responseString = JsonConvert.SerializeObject(model).ToString();
                payCallAction.LogResponseString(responseString);

                var payResponse = new YongXinPayResponse(model);

                //SYFPAY0000...
                string payOrderID = payResponse.GetThirdPartyOderID();
                //取得實際orderid
                orderID = payResponse.GetOrderID();

                //檢查訂單狀態
                var orderlist = Service.CDGetAEPayOrderByID(Convert.ToInt32(orderID));
                if (orderlist.Count == 0)
                {
                    SiteHelper.Log(logGuid, $"{orderID} 支付回调 查無訂單", YongXinPay_logFile);
                    return PayState.Error;
                }
                var order = orderlist[0];
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, $"{order.Id} 支付回调 訂單狀態已經完成,無須金幣充值", YongXinPay_logFile);
                    return PayState.Success;
                }

                //進行金幣充值
                string msg = "";

                if (Convert.ToDouble(payResponse.GetPayAmount()) == order.Amount)
                {
                    var data = $"完成支付";

                    if (Service.CDFinishAEOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, order.TransID, data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 支付回调 金幣充值成功 merchantOrderId,:{payResponse.GetOrderID().ToString()}, totalAmount:{payResponse.GetPayAmount().ToString()}", YongXinPay_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 支付回调 金幣充值错误 {msg}", YongXinPay_logFile);
                        Service.CDFailOrder(order.Id, payResponse.GetOrderID().ToString(), $"{msg}");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, string.Format($"{order.Id}  支付回调 支付無效 merchantOrderId:{payResponse.GetOrderID().ToString()},totalAmount:{payResponse.GetPayAmount().ToString()}"), YongXinPay_logFile);
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

                string LogMessage = string.Format("支付回调 异常错误 {0}", ex.ToString());
                SiteHelper.Log(logGuid, LogMessage, YongXinPay_logFile);
                return PayState.Error;
            }

        }
        private static string Md5(string str, Encoding ec)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            return BitConverter.ToString(md5.ComputeHash(ec.GetBytes(str))).Replace("-", "");
        }
    }
    public class YongXinPayType
    {
        /// <summary>
        /// 支付方式
        /// </summary>
        private Dictionary<string, string> payTypeDic;
        /// <summary>
        /// 建構式
        /// </summary>
        public YongXinPayType()

        {
            payTypeDic = new Dictionary<string, string>();
            // 1 入金 - 卡对卡
            payTypeDic.Add("UNIONPAY_H5", "BANK_CARD");
            // 2 入金 - 银联扫码
            payTypeDic.Add("UNIONPAY_QR", "QR_UNION");
            // 3 入金 - 微信扫码
            payTypeDic.Add("WEIXIN_QR", "QR_WECHAT");
            // 4 入金 - 网银
            //payTypeDic.Add("", "");
            // 5 入金 - 支付宝扫码
            payTypeDic.Add("ZHIFUBAO_QR", "QR_ALIPAY");
            // 6 入金 - 支付宝 WAP
            payTypeDic.Add("ZHIFUBAO_H5", "WAP_ALIPAY");
        }
        /// <summary>
        /// 取得支付方式
        /// </summary>
        /// <param name="payType">資料庫支付方式</param>
        /// <returns>第三方支付方式</returns>
        public string GetPayType(string payType)
        {
            if (payTypeDic.ContainsKey(payType))
                return payTypeDic[payType];

            return "QR_ALIPAY";
        }
    }


    public class YongXinPayResponse
    {
        /// <summary>
        /// 四方支付訂單回傳參數格式
        /// </summary>
        private YongXinPayResponseModel PayResponseModel;
        /// <summary>
        /// MD5 SIGN前字串
        /// </summary>
        private string _payRetrunSignStringToMd5 = string.Empty;

        /// <summary>
        /// 建構式
        /// </summary>
        public YongXinPayResponse(YongXinPayResponseModel model)
        {
            PayResponseModel = model;
        }
        /// <summary>
        /// 取得訂單ID(資料庫OrderListID)
        /// </summary>
        public int GetOrderID()
        {
            return ThirdPartyOderIDService.GetOrderID(PayResponseModel.order_number, "YongXinPay");
        }
        /// <summary>
        /// 取得交易訂單ID(給第三方的訂單ID)
        /// </summary>
        public string GetThirdPartyOderID()
        {
            return PayResponseModel.order_number;
        }
        /// <summary>
        /// 取得金額
        /// </summary>
        public int GetPayAmount()
        {
            return PayResponseModel.amountMoney;
        }
    }
    public class YongXinPayResponseModel
    {
        public string merchant_number { get; set; }
        public string order_number { get; set; }
        public string order_amount { get; set; }

        public int amountMoney
        {
            get
            {
                return (int)Decimal.Parse(order_amount);
            }
        }
    }
}