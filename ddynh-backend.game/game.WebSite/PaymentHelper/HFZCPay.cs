using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using static game.WebSite.PayConfig;
using game.Entity.DTO.ThirdPartyPay;
using game.WebSite.Services;
using game.Entity.Payment;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using game.Infrastructure.Extensions;
using game.Entity.Enums.ThirdPartyPay;
using game.Infrastructure;
using game.Infrastructure.Utility.ThirdParty;
using game.WebSite;

namespace game.WebSite.PaymentHelper
{
    /// <summary>
    /// 四方(新王朝)
    /// </summary>
    public static class HFZCPay
    {
        /// <summary>
        /// 创建订单(跳转网页链接)
        /// </summary>
        /// <param name="order">訂單</param>
        /// <param name="logGuid">LogGuID</param>
        /// <returns>第三方產生訂單網址</returns>
        public static HFZCPayReturnToPayCenterModel Post(CDPayOrder order, string logGuid)
        {
            var returnModel = new HFZCPayReturnToPayCenterModel();
            returnModel.Method = "URL";

            var postAction = new ThirdPartyPaymentPostAction(logGuid, HFZCPay_logFile);
            bool isSetOrderDataSuccess = postAction.SetOrderData(order);
            if (!isSetOrderDataSuccess)
            {
                returnModel.Html = postAction.GetReturnHtml();
                return returnModel;
            }

            CDMerchantUUID merchant = MerchantDataService.GetMerchantData(order);
            bool isSetMerchantSuccess = postAction.SetMerChant(merchant);
            if (!isSetMerchantSuccess)
            {
                returnModel.Html = postAction.GetReturnHtml();
                return returnModel;
            }

            //支付下单地址
            var PayBaseSetting = new HFZCPayBaseSetting(merchant.InfoData, merchant.Url);
            var PayOrder = new HFZCPayOrder(PayBaseSetting);
            PayOrder.SetOrderData(order);
            PayOrder.SetPayType(merchant.BankCode);
            PayOrder.Done();

            //紀錄交易編號
            string ThirdPartyOderID = PayOrder.GetThirdPartyOderID();
            postAction.LogThirdPartyOderID(ThirdPartyOderID);

            //紀錄MD5資料
            string md5String = PayOrder.GetMd5String();
            postAction.LogPayOrderPostSignBeforeString(md5String);

            //紀錄postJson 、 action url 和 ReturnString
            string payOrderUrl = PayBaseSetting.GetBaseUrl("api/pay/V2");
            string payOrderJson = PayOrder.GetPostJsonString();
            string payOrderReturnString = SiteHelper.HttpPostJson(payOrderUrl, payOrderJson);
            postAction.LogPayOrderPostDataString(payOrderJson, payOrderUrl);
            postAction.LogPayReturnString(payOrderReturnString);

            //處理回傳json
            var payReturn = new HFZCPayReturn(payOrderReturnString);

            //確認json是否正確
            if (!payReturn.IsSuccess())
            {
                returnModel.Html = postAction.GetReturnHtml();
                return returnModel;
            }

            //確認回傳格式
            returnModel.Method = payReturn.GetReturnMehtod();
            returnModel.Html = payReturn.GetReturnHtml();
            postAction.SetReturnHtml(returnModel.Html);

            return returnModel;
        }

        /// <summary>
        /// 代收交易非同步通知
        /// </summary>
        /// <param name="logGuid">LogGuID</param>
        public static PayState Call(string logGuid)
        {
            int orderID = 0;
            try
            {
                //是否跳出function return
                bool isCallReturn;
                var payCallAction = new ThirdPartyPaymentCallAction(logGuid, HFZCPay_logFile);
                payCallAction.LogResponseString();
                string responseString = payCallAction.GetResponseString();

                var payResponse = new HFZCPayResponse(responseString);

                //SYFPAY0000...
                string payOrderID = payResponse.GetThirdPartyOderID();
                //取得實際orderid
                orderID = payResponse.GetOrderID();

                //設定訂單資訊(紀錄LOG內容也需要)
                payCallAction.SetOrderData(orderID, payOrderID);
                //確認回調
                isCallReturn = payCallAction.CheckResponseIsSuccess(payResponse.IsSuccess());
                if (isCallReturn)
                {
                    if (payCallAction.IsPayStateSuccess)
                        return PayState.Success;
                    else
                        return PayState.Error;
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

                //確認回調sign
                var order = payCallAction.GetOrder();
                CDMerchantUUID merchant = MerchantDataService.GetMerchantData(order);
                if (merchant == null)
                {
                    payCallAction.CheckResponseSign(false, string.Empty);
                    return PayState.Error;
                }

                var payBaseSetting = new HFZCPayBaseSetting(merchant.InfoData, merchant.Url);
                bool isSignTrue = payResponse.IsSignTrue(payBaseSetting);
                string returnstringToMd5 = payResponse.GetReturnstringToMd5();
                isCallReturn = payCallAction.CheckResponseSign(isSignTrue, returnstringToMd5);
                if (isCallReturn)
                {
                    if (payCallAction.IsPayStateSuccess)
                        return PayState.Success;
                    else
                        return PayState.SignError;
                }

                string sourceIP = SystemHelper.UserIPAddress;
                int responseAmount = Convert.ToInt32(payResponse.GetPayAmount());

                //進行金幣充值
                payCallAction.FinishOrderAndCheckMonery(responseAmount, sourceIP);

                if (payCallAction.IsPayStateSuccess)
                    return PayState.Success;
                else
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
                SiteHelper.Log(logGuid, LogMessage, HFZCPay_logFile);
                return PayState.Error;
            }

        }
    }

    /// <summary>
    /// 四方支付基本通道設定(商户号、密钥...)
    /// </summary>
    public class HFZCPayBaseSetting : PaymentMerchantInfoBaseSetting
    {
        /// <summary>
        /// 建構式
        /// </summary>
        public HFZCPayBaseSetting(string InfoData, string Url)
        {
            var InfoDataJson = Newtonsoft.Json.JsonConvert.DeserializeObject<ThirdPartyPayMerchantInfo>(InfoData);
            _merchantNo = InfoDataJson.MERCHANT_NO;
            _key = InfoDataJson.MERCHANT_KEY;
            _baseUrl = Url + "/";
        }
    }

    /// <summary>
    /// 創建訂單
    /// </summary>
    public class HFZCPayOrder
    {
        /// <summary>
        /// 支付所有需要欄位
        /// </summary>
        private HFZCPayOrderModel PayOrderModle;
        /// <summary>
        /// md5排序
        /// </summary>
        private List<string> Md5Sort;
        /// <summary>
        /// post排序
        /// </summary>
        private List<string> PostSort;
        /// <summary>
        /// md5資料結構
        /// </summary>
        private Dictionary<string, object> Md5Data;
        /// <summary>
        /// post資料結構
        /// </summary>
        private Dictionary<string, object> PostData;
        /// <summary>
        /// 此字串md5之後將存成sign
        /// </summary>
        private string Md5String;
        /// <summary>
        /// json post string
        /// </summary>
        private string JsonStr;

        /// <summary>
        /// 建構式
        /// </summary>
        public HFZCPayOrder(HFZCPayBaseSetting PayBaseSetting)
        {
            JsonStr = string.Empty;
            Md5Data = new Dictionary<string, object>();
            PostData = new Dictionary<string, object>();

            Md5Sort = new List<string> { "bizAmt", "channleType", "customNo", "date", "merchantNo", "noticeUrl", "orderNo", "returnUrl", "signType", "version" };
            PostSort = new List<string> { "version", "signType", "merchantNo", "date", "channleType", "sign", "orderNo", "bizAmt", "returnUrl", "noticeUrl", "customNo" };

            SetDefaultPayData(PayBaseSetting);
        }

        /// <summary>
        /// 設定基本HFZCPayOrderModle
        /// </summary>
        /// <param name="PayBaseSetting">商戶號等等基本設定</param>
        private void SetDefaultPayData(HFZCPayBaseSetting PayBaseSetting)
        {
            PayOrderModle = new HFZCPayOrderModel();
            PayOrderModle.merchantNo = PayBaseSetting.MerchantNo;
            PayOrderModle.token = [REDACTED];
            PayOrderModle.noticeUrl = HFZCPay_NotifyUrl;
            PayOrderModle.returnUrl = FinishUrl;
        }

        /// <summary>
        /// 設定支付類型(service)
        /// </summary>
        /// <param name="payType">支付類型</param>
        public void SetPayType(string payType)
        {
            if(payType == "WANGGUAN_ONLINE_BANK")
            {
                Md5Sort.Add("bankCode");
                Md5Sort.Sort();
            }
            try
            {
                PayOrderModle.channleType = (int)ExtensionOfEnum.GetEnumByTextValue<HFZCPayTypeEnum>(payType);
            }
            catch
            {
                PayOrderModle.channleType = (int)HFZCPayTypeEnum.ZHIFUBAO_QR;
            }
        }

        /// <summary>
        /// 設定oder資料，商品ID使用orderID
        /// </summary>
        /// <param name="order"></param>
        public void SetOrderData(CDPayOrder order)
        {
            PayOrderModle.bizAmt = order.Amount;
            PayOrderModle.orderNo = ThirdPartyOderIDService.GetThirdPartyOderID(order.Id, "HFZCPay", length: 20);
            PayOrderModle.customNo = order.UserID;
        }

        /// <summary>
        /// 依照排序設定Dictionary
        /// </summary>
        private void SetDictionary(List<string> stringArr, Dictionary<string, object> dictionary)
        {
            foreach (var key in stringArr)
            {
                foreach (PropertyInfo prop in PayOrderModle.GetType().GetProperties())
                {
                    if (prop.Name == key)
                    {
                        var value = prop.GetValue(PayOrderModle, null);
                        if (value != null)
                        {
                            dictionary.Add(key, value);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 設定簽名
        /// </summary>
        private void SetSign()
        {
            //排序MD5字串
            SetDictionary(Md5Sort, Md5Data);
            //md5 sign
            Md5String = StringObjectDirctionaryService.ConvertToUrlStringFormat(Md5Data, "&");
            Md5String = Md5String + PayOrderModle.token;
            PayOrderModle.sign = MD5Service.Hash(Md5String, Encoding.UTF8).ToLower();
        }

        /// <summary>
        /// 設定post資料(JSON 或者INPUT FORM)
        /// </summary>
        private void SetPost()
        {
            //set post dictionary
            SetDictionary(PostSort, PostData);
            //to json(紀錄LOG使用)
            JsonStr = StringObjectDirctionaryService.ToJsonStr(PostData);
        }

        /// <summary>
        /// 完成
        /// </summary>
        public void Done()
        {
            SetSign();
            SetPost();
        }

        /// <summary>
        /// 取得交易訂單編號
        /// </summary>
        public string GetThirdPartyOderID()
        {
            return PayOrderModle.orderNo;
        }

        public string GetMd5String()
        {
            return Md5String;
        }

        /// <summary>
        /// 取得post Json to String(紀錄LOG使用)
        /// </summary>
        public string GetPostJsonString()
        {
            return JsonStr;
        }

        /// <summary>
        /// 取得PayOrderModle(測試使用)
        /// </summary>
        public HFZCPayOrderModel GetPayOrderModel()
        {
            return PayOrderModle;
        }
    }

    public class HFZCPayReturn
    {
        /// <summary>
        /// 四方支付訂單回傳參數格式
        /// </summary>
        private HFZCPayReturnModel PayReturnModel;

        /// <summary>
        /// 建構式
        /// </summary>
        public HFZCPayReturn(string jsonString)
        {
            PayReturnModel = JsonConvert.DeserializeObject<HFZCPayReturnModel>(jsonString);
        }
        /// <summary>
        /// 檢查回傳json是否成功
        /// </summary>
        public bool IsSuccess()
        {
            return PayReturnModel.code == (byte)HFZCPayRetrunCodeEnum.Success;
        }
        /// <summary>
        /// 取得回傳取訂單模式
        /// </summary>
        /// <returns></returns>
        public string GetReturnMehtod()
        {
            if (string.IsNullOrEmpty(PayReturnModel.detail.PayURL))
                return "FORM";

            return "URL";
        }
        /// <summary>
        /// 取得回傳頁面
        /// </summary>
        public string GetReturnHtml()
        {
            if (!string.IsNullOrEmpty(PayReturnModel.detail.PayURL))
                return PayReturnModel.detail.PayURL;

            return PayReturnModel.detail.PayHtml;
        }
    }

    /// <summary>
    /// 四方支付第三方異步通知
    /// </summary>
    public class HFZCPayResponse
    {
        /// <summary>
        /// 四方支付訂單回傳參數格式
        /// </summary>
        private HFZCPayResponseModel PayResponseModel;
        /// <summary>
        /// MD5 SIGN前字串
        /// </summary>
        private string _payRetrunSignStringToMd5 = string.Empty;

        /// <summary>
        /// 建構式
        /// </summary>
        public HFZCPayResponse(string jsonString)
        {
            PayResponseModel = JsonConvert.DeserializeObject<HFZCPayResponseModel>(jsonString);
        }
        /// <summary>
        /// 取得訂單ID(資料庫OrderListID)
        /// </summary>
        public int GetOrderID()
        {
            return ThirdPartyOderIDService.GetOrderID(PayResponseModel.orderNo, "HFZCPay");
        }
        /// <summary>
        /// 取得交易訂單ID(給第三方的訂單ID)
        /// </summary>
        public string GetThirdPartyOderID()
        {
            return PayResponseModel.orderNo;
        }
        /// <summary>
        /// 回調是否成功
        /// </summary>
        public bool IsSuccess()
        {
            return PayResponseModel.status == (int)HFZCPayRetrunStatusEnum.Success;
        }
        /// <summary>
        /// 取得金額
        /// </summary>
        public long GetPayAmount()
        {
            return PayResponseModel.bizAmtMoney;
        }
        /// <summary>
        /// 回調簽名是否正確
        /// </summary>
        /// <param name="payBaseSetting">商戶號等基本設定</param>
        public bool IsSignTrue(HFZCPayBaseSetting payBaseSetting)
        {
            Dictionary<string, object> signChkData = new Dictionary<string, object>();
            signChkData.Add("bizAmt", PayResponseModel.bizAmt);
            signChkData.Add("date", PayResponseModel.date);
            signChkData.Add("orderNo", PayResponseModel.orderNo);
            signChkData.Add("remark", PayResponseModel.remark);
            signChkData.Add("status", PayResponseModel.status);
            signChkData.Add("version", PayResponseModel.version);
            _payRetrunSignStringToMd5 = StringObjectDirctionaryService.ConvertToUrlStringFormat(signChkData, "&");
            _payRetrunSignStringToMd5 = _payRetrunSignStringToMd5 + payBaseSetting.Key;
            string signChk = MD5Service.Hash(_payRetrunSignStringToMd5, Encoding.UTF8).ToLower();
            return PayResponseModel.sign == signChk;
        }
        /// <summary>
        /// 取得驗證簽名MD5使用字串
        /// </summary>
        public string GetReturnstringToMd5()
        {
            return _payRetrunSignStringToMd5;
        }
    }
}