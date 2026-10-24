using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static game.WebSite.PayConfig;
using game.Entity.Payment;
using game.WebSite.Services;
using Newtonsoft.Json;
using game.Entity.DTO.ThirdPartyPay;
using game.Infrastructure.Extensions;
using game.Entity.Enums.ThirdPartyPay;
using System.Reflection;
using System.Text;
using game.Infrastructure.Utility.ThirdParty;
using game.Infrastructure;

namespace game.WebSite.PaymentHelper
{
    /// <summary>
    /// 啟飛支付
    /// </summary>
    public static class GoldSoarPay
    {
        /// <summary>
        /// 創建訂單
        /// </summary>
        /// <param name="order">資料庫訂單模型</param>
        /// <param name="logGuid">LOG File ID</param>
        /// <returns>回傳網址或訂單View</returns>
        public static string Post(CDPayOrder order,  string logGuid)
        {
            var postAction = new ThirdPartyPaymentPostAction(logGuid, GoldSoarPay_logFile);
            bool isSetOrderDataSuccess = postAction.SetOrderData(order);
            if (!isSetOrderDataSuccess)
            {
                return postAction.GetReturnHtml();
            }

            CDMerchantUUID merchant = MerchantDataService.GetMerchantData(order);
            bool isSetMerchantSuccess = postAction.SetMerChant(merchant);
            if (!isSetMerchantSuccess)
            {
                return postAction.GetReturnHtml();
            }

            //支付下单地址
            var PayBaseSetting = new GoldSoarPayBaseSetting(merchant.InfoData, merchant.Url);
            var PayOrder = new GoldSoarPayOrder(PayBaseSetting);
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
            string payOrderUrl = PayBaseSetting.GetBaseUrl("cnppay/order");
            string payOrderInput = PayOrder.GetPostInputString();
            string payOrderReturnString = SiteHelper.HttpPost(payOrderUrl, payOrderInput);
            postAction.LogPayOrderPostDataString(payOrderInput, payOrderUrl);
            postAction.LogPayReturnString(payOrderReturnString);

            //處理回傳json
            var payReturn = new GoldSoarPayReturn(payOrderReturnString);

            //確認回傳值是否正確
            bool isPayReturnSuccess = payReturn.IsSuccess();
            postAction.LogPayReturnMessage(isPayReturnSuccess, payReturn.GetMessage());
            if (!isPayReturnSuccess)
            {
                return postAction.GetReturnHtml();
            }

            //確認回傳值簽名是否正確
            bool isPayReturnSignTrue = payReturn.IsSignTrue(PayBaseSetting);
            postAction.LogPayReturnSign(isPayReturnSignTrue, payReturn.GetMd5String());
            if (!isPayReturnSignTrue)
            {
                return postAction.GetReturnHtml();
            }

            //LOG 回傳網址
            postAction.SetReturnHtml(payReturn.GetReturnUrl());

            return postAction.GetReturnHtml();
        }
        /// <summary>
        /// 異步回調
        /// </summary>
        /// <param name="logGuid">LOG File ID</param>
        /// <returns>是否回調正確並支付成功</returns>
        public static PayState Call(string logGuid, GoldSoarPayResponseModel model)
        {
            int orderID = 0;
            try
            {
                //是否跳出function return
                bool isCallReturn;
                var payCallAction = new ThirdPartyPaymentCallAction(logGuid, GoldSoarPay_logFile);
                //非json使用此方法紀錄回傳值
                string responseString = JsonConvert.SerializeObject(model).ToString();
                payCallAction.LogResponseString(responseString);

                var payResponse = new GoldSoarPayResponse(model);

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

                var payBaseSetting = new GoldSoarPayBaseSetting(merchant.InfoData, merchant.Url);
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
                SiteHelper.Log(logGuid, LogMessage, GoldSoarPay_logFile);
                return PayState.Error;
            }
        }
    }

    /// <summary>
    /// 啟飛第三方支付基本支付通道設定(商户号、密钥...)
    /// </summary>
    public class GoldSoarPayBaseSetting : PaymentMerchantInfoBaseSetting
    {
        /// <summary>
        /// 加密密鑰
        /// </summary>
        private string _secretKey;
        /// <summary>
        /// 建構式
        /// </summary>
        /// <param name="infoData">MerchantInfo</param>
        /// <param name="url">接口網址</param>
        public GoldSoarPayBaseSetting(string infoData, string url)
        {
            var InfoDataJson = JsonConvert.DeserializeObject<GoldSoarPayMerchantInfo>(infoData);
            _merchantNo = InfoDataJson.MERCHANT_NO;
            _key = InfoDataJson.MERCHANT_KEY;
            _secretKey = InfoDataJson.SECRET_KEY;
            _baseUrl = url + "/";
        }
        /// <summary>
        /// 加密密鑰
        /// </summary>
        public string SecretKey
        {
            get { return _secretKey; }
        }
    }

    /// <summary>
    /// 創建訂單
    /// </summary>
    public class GoldSoarPayOrder
    {
        /// <summary>
        /// 支付所有需要欄位
        /// </summary>
        private GoldSoarPayOrderModel PayOrderModle;
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
        /// postData表單格式字串
        /// </summary>
        private string InputStr;

        /// <summary>
        /// 建構式
        /// </summary>
        public GoldSoarPayOrder(GoldSoarPayBaseSetting payBaseSetting)
        {
            JsonStr = string.Empty;
            Md5Data = new Dictionary<string, object>();
            PostData = new Dictionary<string, object>();

            Md5Sort = new List<string> { "notifyUrl", "orderNo", "orderPrice", "orderTime", "payKey", "productName", "productType", "returnUrl", "paySecret" };
            PostSort = new List<string> { "payKey", "orderNo", "productType", "orderPrice", "orderTime", "productName", "returnUrl", "notifyUrl", "sign" };

            SetDefaultPayData(payBaseSetting);
        }

        /// <summary>
        /// 設定基本GoldSoarPayOrderModle
        /// </summary>
        /// <param name="payBaseSetting">商戶號等等基本設定</param>
        private void SetDefaultPayData(GoldSoarPayBaseSetting payBaseSetting)
        {
            PayOrderModle = new GoldSoarPayOrderModel();
            PayOrderModle.payKey = payBaseSetting.Key;
            PayOrderModle.paySecret = [REDACTED];
            PayOrderModle.notifyUrl = GoldSoarPay_NotifyUrl;
            PayOrderModle.returnUrl = FinishUrl;
            PayOrderModle.orderTimeStamp = DateTime.Now;
        }

        /// <summary>
        /// 設定支付類型(service)
        /// </summary>
        /// <param name="payType">支付類型</param>
        public void SetPayType(string payType)
        {
            try
            {
                PayOrderModle.productType = (int)ExtensionOfEnum.GetEnumByTextValue<GoldSoarPayTypeEnum>(payType);
            }
            catch
            {
                PayOrderModle.productType = (int)GoldSoarPayTypeEnum.ZHIFUBAO_H5;
            }
        }

        /// <summary>
        /// 設定oder資料，商品ID使用orderID
        /// </summary>
        /// <param name="order"></param>
        public void SetOrderData(CDPayOrder order)
        {
            PayOrderModle.orderPrice = order.Amount;
            PayOrderModle.orderNo = ThirdPartyOderIDService.GetThirdPartyOderID(order.Id, "GoldSoarPay", length: 20);
            PayOrderModle.productName = order.UserID.ToString();
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
            PayOrderModle.sign = MD5Service.Hash(Md5String, Encoding.UTF8).ToUpper();
        }

        /// <summary>
        /// 設定post資料(JSON 或者INPUT FORM)
        /// </summary>
        private void SetPost()
        {
            //set post dictionary
            SetDictionary(PostSort, PostData);
            //postData to field1=value1&field2=value2...字串
            InputStr = StringObjectDirctionaryService.ConvertToUrlStringFormat(PostData, "&");
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
        /// 取得post input to String(紀錄LOG使用)
        /// </summary>
        public string GetPostInputString()
        {
            return InputStr;
        }

        /// <summary>
        /// 取得PayOrderModle(測試使用)
        /// </summary>
        public GoldSoarPayOrderModel GetPayOrderModel()
        {
            return PayOrderModle;
        }
    }

    /// <summary>
    /// 第三方訂單return
    /// </summary>
    public class GoldSoarPayReturn
    {
        /// <summary>
        /// GoldSoarPay訂單回傳參數格式
        /// </summary>
        private GoldSoarPayReturnModel PayReturnModel;
        /// <summary>
        /// 此字串md5之後將存成sign
        /// </summary>
        private string Md5String;

        /// <summary>
        /// 建構式
        /// </summary>
        public GoldSoarPayReturn(string jsonString)
        {
            PayReturnModel = JsonConvert.DeserializeObject<GoldSoarPayReturnModel>(jsonString);
        }
        /// <summary>
        /// 檢查回傳json是否成功
        /// </summary>
        public bool IsSuccess()
        {
            return PayReturnModel.respCode == "0000";
        }
        /// <summary>
        /// 取得回傳訊息
        /// </summary>
        /// <returns></returns>
        public string GetMessage()
        {
            return PayReturnModel.respMsg;
        }
        /// <summary>
        /// 取得回傳網址
        /// </summary>
        public string GetReturnUrl()
        {
            return PayReturnModel.payUrl;
        }
        /// <summary>
        /// 檢查sign
        /// </summary>
        public bool IsSignTrue(GoldSoarPayBaseSetting payBaseSetting)
        {
            Dictionary<string, object> md5Data = new Dictionary<string, object>();
            md5Data.Add("payUrl", PayReturnModel.payUrl);
            md5Data.Add("respCode", PayReturnModel.respCode);
            md5Data.Add("respMsg", PayReturnModel.respMsg);
            md5Data.Add("paySecret", payBaseSetting.SecretKey);
            Md5String = StringObjectDirctionaryService.ConvertToUrlStringFormat(md5Data, "&");
            return PayReturnModel.sign == MD5Service.Hash(Md5String, Encoding.UTF8);
        }
        /// <summary>
        /// 取得MD5字串
        /// </summary>
        public string GetMd5String()
        {
            return Md5String;
        }
    }

    /// <summary>
    /// 支付第三方異步通知
    /// </summary>
    public class GoldSoarPayResponse
    {
        /// <summary>
        /// 四方支付訂單回傳參數格式
        /// </summary>
        private GoldSoarPayResponseModel PayResponseModel;
        /// <summary>
        /// MD5 SIGN前字串
        /// </summary>
        private string _payRetrunSignStringToMd5 = string.Empty;

        /// <summary>
        /// 建構式
        /// </summary>
        public GoldSoarPayResponse(GoldSoarPayResponseModel model)
        {
            PayResponseModel = model;
        }
        /// <summary>
        /// 取得訂單ID(資料庫OrderListID)
        /// </summary>
        public int GetOrderID()
        {
            return ThirdPartyOderIDService.GetOrderID(PayResponseModel.outTradeNo, "GoldSoarPay");
        }
        /// <summary>
        /// 取得交易訂單ID(給第三方的訂單ID)
        /// </summary>
        public string GetThirdPartyOderID()
        {
            return PayResponseModel.outTradeNo;
        }
        /// <summary>
        /// 回調是否成功
        /// </summary>
        public bool IsSuccess()
        {
            return PayResponseModel.tradeStatus == "SUCCESS";
        }
        /// <summary>
        /// 取得金額
        /// </summary>
        public long GetPayAmount()
        {
            return PayResponseModel.orderPriceMoney;
        }
        /// <summary>
        /// 回調簽名是否正確
        /// </summary>
        /// <param name="payBaseSetting">商戶號等基本設定</param>
        public bool IsSignTrue(GoldSoarPayBaseSetting payBaseSetting)
        {
            Dictionary<string, object> signChkData = new Dictionary<string, object>();
            signChkData.Add("orderPrice", PayResponseModel.orderPrice);
            signChkData.Add("orderTime", PayResponseModel.orderTime);
            signChkData.Add("outTradeNo", PayResponseModel.outTradeNo);
            signChkData.Add("payKey", PayResponseModel.payKey);
            signChkData.Add("productName", PayResponseModel.productName);
            signChkData.Add("productType", PayResponseModel.productType);
            signChkData.Add("successTime", PayResponseModel.successTime);
            signChkData.Add("tradeStatus", PayResponseModel.tradeStatus);
            signChkData.Add("trxNo", PayResponseModel.trxNo);
            signChkData.Add("paySecret", payBaseSetting.SecretKey);
            _payRetrunSignStringToMd5 = StringObjectDirctionaryService.ConvertToUrlStringFormat(signChkData, "&");
            string signChk = MD5Service.Hash(_payRetrunSignStringToMd5, Encoding.UTF8).ToUpper();
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