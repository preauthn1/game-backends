using game.Entity.Payment;
using static game.WebSite.PayConfig;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using game.WebSite.Services;
using game.Infrastructure;
using game.Entity.DTO.ThirdPartyPay;
using game.Infrastructure.Utility.ThirdParty;
using System.Text;
using System;
using System.Security.Cryptography;
using System.Web;

namespace game.WebSite.PaymentHelper
{
    /// <summary>
    /// 隍寶
    /// </summary>
    public class SjqpPay
    {
        public static string Post(CDPayOrder order, string guid)
        {
            string url = string.Empty;
            var payTypeObj = new SjqpPayType();
            CDMerchantUUID merchant = MerchantDataService.GetMerchantData(order);
            var InfoDataJson = JsonConvert.DeserializeObject<ThirdPartyPayMerchantInfo>(merchant.InfoData);
            var orderID = ThirdPartyOderIDService.GetThirdPartyOderID(order.Id, "SjqpPay", length: 20);

            string parasData = $"amount={order.Amount * 100}&callbackurl={Uri.EscapeDataString(SjqpPay_NotifyUrl)}&outtradeid={orderID}";
            parasData += $"&payto={InfoDataJson.MERCHANT_NO}&paytype={payTypeObj.GetPayType(merchant.BankCode)}";
            parasData += $"&remark={orderID}";
            parasData += $"&secret=[REDACTED]";
            var pay_md5sign = Md5(parasData, Encoding.UTF8);
           

            string request = $"amount={order.Amount * 100}&callbackurl={SjqpPay_NotifyUrl}&outtradeid={orderID}";
            request += $"&payto={InfoDataJson.MERCHANT_NO}&paytype={payTypeObj.GetPayType(merchant.BankCode)}";
            request += $"&remark={orderID}";
            request += $"&verification={pay_md5sign.ToLower()}";
            SiteHelper.Log(guid, $"{order.Id} 支付请求內容 [request] {request}", SjqpPay_logFile);

            //呼叫支付平台
            var responseString = SiteHelper.HttpPost((merchant.Url + "/" + SjqpPay_CreateOrderApiUrl), request);
            SiteHelper.Log(guid, string.Format("{0} 支付请求结果 [response] {1}", order.Id, responseString), SjqpPay_logFile);

            JObject json = JObject.Parse(responseString);
            var redirect_url = json["qr"] != null ? json["qr"].ToString() : string.Empty;
            var error_message = json["err"] != null ? json["err"].ToString() : string.Empty;
            SiteHelper.Log(guid, string.Format("{0} 支付请求 [data]({1}) {2}", order.Id, (merchant.Url + "/" + SjqpPay_CreateOrderApiUrl), url), SjqpPay_logFile);
            if (!redirect_url.Equals(string.Empty))
            {
                //將thrid party order no寫入Pay.OrderList.TransID
                Service.CDNewModifyTransID(order.Id, orderID, (int)merchant.GWID);
                return redirect_url;
            }
            return url;
        }

        public static PayState Call(string logGuid, SjqpPayResponseModel model)
        {
            int orderID = 0;
            try
            {
                //是否跳出function return
                bool isCallReturn;
                var payCallAction = new ThirdPartyPaymentCallAction(logGuid, SjqpPay_logFile);
                //非json使用此方法紀錄回傳值
                string responseString = JsonConvert.SerializeObject(model).ToString();
                payCallAction.LogResponseString(responseString);

                var payResponse = new SjqpPayResponse(model);

                if (model.status != 1)
                {
                    SiteHelper.Log(logGuid, $"{orderID} 支付回调状态錯誤", SjqpPay_logFile);
                    return PayState.Error;
                }
               
                //SYFPAY0000...
                string payOrderID = payResponse.GetThirdPartyOderID();
                //取得實際orderid
                orderID = payResponse.GetOrderID();
                //設定訂單資訊(紀錄LOG內容也需要)
                payCallAction.SetOrderData(orderID, payOrderID);

                //檢查訂單狀態
                var orderlist = Service.CDGetAEPayOrderByID(Convert.ToInt32(orderID));
                if (orderlist.Count == 0)
                {
                    SiteHelper.Log(logGuid, $"{orderID} 支付回调 查無訂單", SjqpPay_logFile);
                    return PayState.Error;
                }
                var order = orderlist[0];
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, $"{order.Id} 支付回调 訂單狀態已經完成,無須金幣充值", SjqpPay_logFile);
                    return PayState.Success;
                }
               

               
                //確認訂單資訊
                isCallReturn = payCallAction.CheckOrderData();
                //若訂單確認失敗則返回
                if (isCallReturn)
                {
                    if (payCallAction.IsPayStateSuccess)
                        return PayState.Success;
                    else
                        return PayState.Error;
                }
                var orderMerchant = payCallAction.GetOrder();
                CDMerchantUUID merchant = MerchantDataService.GetMerchantData(orderMerchant);
                var InfoDataJson = JsonConvert.DeserializeObject<ThirdPartyPayMerchantInfo>(merchant.InfoData);
                var pay_md5sign = Md5($"{model.id}{model.outtradeid}{model.payto}{model.amount}{model.paytime}{model.status}{model.remark}{InfoDataJson.MERCHANT_KEY}", Encoding.UTF8);
                if (pay_md5sign.ToLower() != model.verification.ToLower())
                {
                    SiteHelper.Log(logGuid, $"{orderID} 数据验证字段验证錯誤 pay_md5sign : {pay_md5sign}", SjqpPay_logFile);
                    return PayState.Error;
                }

                //進行金幣充值
                string msg = "";

                if (Convert.ToDouble(payResponse.GetPayAmount()) == order.Amount)
                {
                    var data = $"完成支付";

                    if (Service.CDFinishAEOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, order.TransID, data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 支付回调 金幣充值成功 merchantOrderId,:{payResponse.GetOrderID().ToString()}, totalAmount:{payResponse.GetPayAmount().ToString()}", SjqpPay_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 支付回调 金幣充值错误 {msg}", SjqpPay_logFile);
                        Service.CDFailOrder(order.Id, payResponse.GetOrderID().ToString(), $"{msg}");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, string.Format($"{order.Id}  支付回调 支付無效 merchantOrderId:{payResponse.GetOrderID().ToString()},totalAmount:{payResponse.GetPayAmount().ToString()}"), SjqpPay_logFile);
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
                SiteHelper.Log(logGuid, LogMessage, SjqpPay_logFile);
                return PayState.Error;
            }

        }
        private static string Md5(string str, Encoding ec)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            return BitConverter.ToString(md5.ComputeHash(ec.GetBytes(str))).Replace("-", "");
        }
    }
    public class SjqpPayType
    {
        /// <summary>
        /// 支付方式
        /// </summary>
        private Dictionary<string, string> payTypeDic;
        /// <summary>
        /// 建構式
        /// </summary>
        public SjqpPayType()

        {
            payTypeDic = new Dictionary<string, string>();
            // 支付宝扫码
            payTypeDic.Add("ZHIFUBAO_QR", "alipay");
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

            return "alipay";
        }
    }


    public class SjqpPayResponse
    {
        /// <summary>
        /// 四方支付訂單回傳參數格式
        /// </summary>
        private SjqpPayResponseModel PayResponseModel;
        /// <summary>
        /// MD5 SIGN前字串
        /// </summary>
        private string _payRetrunSignStringToMd5 = string.Empty;

        /// <summary>
        /// 建構式
        /// </summary>
        public SjqpPayResponse(SjqpPayResponseModel model)
        {
            PayResponseModel = model;
        }
        /// <summary>
        /// 取得訂單ID(資料庫OrderListID)
        /// </summary>
        public int GetOrderID()
        {
            return ThirdPartyOderIDService.GetOrderID(PayResponseModel.outtradeid, "SjqpPay");
        }
        /// <summary>
        /// 取得交易訂單ID(給第三方的訂單ID)
        /// </summary>
        public string GetThirdPartyOderID()
        {
            return PayResponseModel.outtradeid;
        }
        /// <summary>
        /// 取得金額
        /// </summary>
        public int GetPayAmount()
        {
            return PayResponseModel.amountMoney;
        }
    }
    public class SjqpPayResponseModel
    {
        public string id { get; set; }
        public string outtradeid { get; set; }
        public string payto { get; set; }

        public int amount { get; set; }

        public string realamount { get; set; }

        public int amountMoney
        {
            get
            {
                return amount / 100;
            }
        }

        public string paytime { get; set; }

        public int status { get; set; }

        public string remark { get; set; }

        public string verification { get; set; }

        public string verification2 { get; set; }
    }
}