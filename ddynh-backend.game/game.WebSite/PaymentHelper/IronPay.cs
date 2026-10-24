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
using game.Infrastructure;
using game.Infrastructure.Utility.ThirdParty;

namespace game.WebSite.PaymentHelper
{
    /// <summary>
    /// 鐵付
    /// </summary>
    public class IronPay
    {
        /// <summary>
        /// 创建订单(跳转网页链接)
        /// </summary>
        /// <param name="order">訂單</param>
        /// <param name="logGuid">LogGuID</param>
        /// <returns>第三方產生訂單網址</returns>
        public static string Post(CDPayOrder order, string logGuid)
        {
            var postAction = new ThirdPartyPaymentPostAction(logGuid, IronPay_logFile);
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
            var payBaseSetting = new IronPayBaseSetting(merchant.InfoData, merchant.Url);
            var payOrder = new IronPayOrder(payBaseSetting);
            payOrder.SetOrderData(order);
            payOrder.SetPayType(merchant.BankCode);
            payOrder.Done();

            //紀錄交易編號
            string thirdPartyOderID = payOrder.GetThirdPartyOderID();
            postAction.LogThirdPartyOderID(thirdPartyOderID);

            //紀錄MD5資料
            string md5String = payOrder.GetMd5String();
            postAction.LogPayOrderPostSignBeforeString(md5String);

            //紀錄postJson 、 action url 和 ReturnString
            string payOrderUrl = payBaseSetting.GetBaseUrl("v5/pay.html");
            string payOrderJson = payOrder.GetPostJsonString();
            string payOrderReturnString = SiteHelper.HttpPostJson(payOrderUrl, payOrderJson);
            postAction.LogPayOrderPostDataString(payOrderJson, payOrderUrl);
            postAction.LogPayReturnString(payOrderReturnString);

            //處理回傳json
            var payReturn = new IronPayReturn(payOrderReturnString);

            //確認json是否正確
            if (!payReturn.IsSuccess())
            {
                return postAction.GetReturnHtml();
            }

            //確認SIGN是否正確
            if(!payReturn.IsSignTrue(payBaseSetting))
            {
                return postAction.GetReturnHtml();
            }

            //紀錄returnUrl
            string returnUrl = payReturn.GetReturnUrl();
            postAction.SetReturnHtml(returnUrl);
            return postAction.GetReturnHtml();
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
                var payCallAction = new ThirdPartyPaymentCallAction(logGuid, IronPay_logFile);
                payCallAction.LogResponseString();
                string responseString = payCallAction.GetResponseString();

                var payResponse = new IronPayResponse(responseString);

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

                var payBaseSetting = new IronPayBaseSetting(merchant.InfoData, merchant.Url);
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

                string userIP = SystemHelper.UserIPAddress;
                int responseAmount = Convert.ToInt32(payResponse.GetPayAmount());

                //進行金幣充值
                payCallAction.FinishOrderAndCheckMonery(responseAmount, userIP);

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
                SiteHelper.Log(logGuid, LogMessage, IronPay_logFile);
                return PayState.Error;
            }
        }
    }

    /// <summary>
    /// 鐵付第三方支付基本支付通道設定(商户号、密钥...)
    /// </summary>
    public class IronPayBaseSetting : PaymentMerchantInfoBaseSetting
    {
        /// <summary>
        /// 商戶編號
        /// </summary>
        private int _userID;
        /// <summary>
        /// 商戶名稱
        /// </summary>
        private string _userName;
        /// <summary>
        /// 建構式
        /// </summary>
        public IronPayBaseSetting(string InfoData, string Url)
        {
            var InfoDataJson = JsonConvert.DeserializeObject<IronPayMerchantInfo>(InfoData);
            _merchantNo = InfoDataJson.MERCHANT_NO;
            _key = InfoDataJson.MERCHANT_KEY;
            _userID = InfoDataJson.MERCHANT_USERID;
            _userName = InfoDataJson.MERCHANT_USERNAME;
            _baseUrl = Url + "/";
        }
        /// <summary>
        /// 商戶編號
        /// </summary>
        public int UserID
        {
            get { return _userID; }
        }
        /// <summary>
        /// 商戶名稱
        /// </summary>
        public string UserName
        {
            get { return MD5Service.Hash(_userName, Encoding.UTF8).ToUpper(); }
        }
    }

    /// <summary>
    /// 創造第三方訂單
    /// </summary>
    public class IronPayOrder
    {
        /// <summary>
        /// 支付所有需要欄位
        /// </summary>
        private IronPayOrderModel PayOrderModle;
        /// <summary>
        /// 支付方式資料結構
        /// </summary>
        private Dictionary<string, string> PayType;
        /// <summary>
        /// md5排序
        /// </summary>
        private List<string> Md5Sort;
        /// <summary>
        /// post排序
        /// </summary>
        private List<string> PostSort;
        /// <summary>設定基本IronPayOrderModle
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
        public IronPayOrder(IronPayBaseSetting PayBaseSetting)
        {
            JsonStr = string.Empty;
            Md5Data = new Dictionary<string, object>();
            PostData = new Dictionary<string, object>();

            Md5Sort = new List<string> { "body", "channel", "merchantNo", "notifyUrl", "orderMoney", "orderNo", "returnUrl", "source", "subject", "userId", "userName", "key" };
            PostSort = new List<string> { "merchantNo", "source", "subject", "body", "channel", "orderNo", "orderMoney", "returnUrl", "notifyUrl", "userId", "userName", "sign" };

            SetPayTypeData();
            SetDefaultPayData(PayBaseSetting);
        }
        /// <summary>
        /// 設定基本IronPayOrderModle
        /// </summary>
        /// <param name="PayBaseSetting">商戶號等等基本設定</param>
        private void SetDefaultPayData(IronPayBaseSetting PayBaseSetting)
        {
            PayOrderModle = new IronPayOrderModel();
            PayOrderModle.merchantNo = PayBaseSetting.MerchantNo;
            PayOrderModle.key = PayBaseSetting.Key;
            PayOrderModle.userId = PayBaseSetting.UserID.ToString();
            PayOrderModle.userName = PayBaseSetting.UserName;
            PayOrderModle.notifyUrl = IronPay_NotifyUrl;
            PayOrderModle.returnUrl = FinishUrl;
            PayOrderModle.channel = "Android";
            PayOrderModle.source = "0";
            PayOrderModle.subject = string.Empty;
            PayOrderModle.body = string.Empty;
            PayOrderModle.orderNo = string.Empty;
            PayOrderModle.orderMoney = "0";
            
            PayOrderModle.sign = string.Empty;
        }
        /// <summary>
        /// 設定鐵付支付方式模型
        /// </summary>
        private void SetPayTypeData()
        {
            PayType = new Dictionary<string, string>();
            PayType.Add("ZHIFUBAO_QR", "1");
            PayType.Add("ZHIFUBAO_H5", "4");
            PayType.Add("WEIXIN_QR", "0");
            PayType.Add("WEIXIN_H5", "3");
            PayType.Add("QQ_QR", "2");
            PayType.Add("JDPAY_QR", "J");
            PayType.Add("UNIONPAY_QR", "6");
        }
        /// <summary>
        /// 設定支付類型(service)
        /// </summary>
        /// <param name="payType">支付類型</param>
        public void SetPayType(string payType)
        {
            if (PayType.ContainsKey(payType))
                PayOrderModle.source = PayType[payType];
        }
        /// <summary>
        /// 設定oder資料，商品ID使用orderID
        /// </summary>
        /// <param name="order"></param>
        public void SetOrderData(CDPayOrder order)
        {
            PayOrderModle.subject = order.Id.ToString();
            PayOrderModle.body = order.Id.ToString();
            PayOrderModle.orderNo = ThirdPartyOderIDService.GetThirdPartyOderID(order.Id, "IronPay", 20);
            PayOrderModle.orderMoney = (order.Amount * 100).ToString();
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
        /// Set Sign
        /// </summary>
        private void SetSign()
        {
            //set md5 dictionary
            SetDictionary(Md5Sort, Md5Data);
            //md5 sign
            Md5String = StringObjectDirctionaryService.ConvertToUrlStringFormat(Md5Data, "&");
            PayOrderModle.sign = MD5Service.Hash(Md5String, Encoding.UTF8).ToUpper();
        }
        /// <summary>
        /// 完成
        /// </summary>
        public void Done()
        {
            SetSign();
            //set post dictionary
            SetDictionary(PostSort, PostData);
            //to json(紀錄LOG使用)
            JsonStr = StringObjectDirctionaryService.ToJsonStr(PostData);
        }
        /// <summary>
        /// 取得交易訂單編號
        /// </summary>
        public string GetThirdPartyOderID()
        {
            return PayOrderModle.orderNo;
        }
        /// <summary>
        /// 取得此字串md5之後將存成sign
        /// </summary>
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
        public IronPayOrderModel GetPayOrderModel()
        {
            return PayOrderModle;
        }
    }
    /// <summary>
    /// 第三方訂單return
    /// </summary>
    public class IronPayReturn
    {
        /// <summary>
        /// IronPay訂單回傳參數格式
        /// </summary>
        private IronPayReturnModel PayReturnModel;
        /// <summary>
        /// 建構式
        /// </summary>
        public IronPayReturn(string jsonString)
        {
            PayReturnModel = JsonConvert.DeserializeObject<IronPayReturnModel>(jsonString);
        }
        /// <summary>
        /// 檢查回傳json是否成功
        /// </summary>
        public bool IsSuccess()
        {
            return PayReturnModel.status == "SUCCESS";
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
        public bool IsSignTrue(IronPayBaseSetting payBaseSetting)
        {
            Dictionary<string, object> md5Data = new Dictionary<string, object>();
            md5Data.Add("merchantNo", PayReturnModel.merchantNo);
            md5Data.Add("message", PayReturnModel.message);
            md5Data.Add("payUrl", PayReturnModel.payUrl);
            md5Data.Add("status", PayReturnModel.status);
            md5Data.Add("key", payBaseSetting.Key);
            string signChk = StringObjectDirctionaryService.GetMd5(md5Data, "&");
            return PayReturnModel.sign == signChk;
        }
    }
    /// <summary>
    /// 第三方異步通知
    /// </summary>
    public class IronPayResponse
    {
        /// <summary>
        /// IronPay訂單回傳參數格式
        /// </summary>
        private IronPayResponseModel PayResponseModel;
        /// <summary>
        /// md5 sign string
        /// </summary>
        private string _payRetrunSignStringToMd5 = string.Empty;
        /// <summary>
        /// 建構式
        /// </summary>
        public IronPayResponse(string jsonString)
        {
            PayResponseModel = JsonConvert.DeserializeObject<IronPayResponseModel>(jsonString);
        }
        /// <summary>
        /// 取得訂單ID(資料庫OrderListID)
        /// </summary>
        public int GetOrderID()
        {
            return ThirdPartyOderIDService.GetOrderID(PayResponseModel.orderNo, "IronPay");
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
            return PayResponseModel.status == "SUCCESS";
        }
        /// <summary>
        /// 取得金額
        /// </summary>
        public long GetPayAmount()
        {
            return Convert.ToInt64(PayResponseModel.orderMoney) / 100;
        }
        /// <summary>
        /// 回調簽名是否正確
        /// </summary>
        /// <param name="payBaseSetting">商戶號等基本設定</param>
        public bool IsSignTrue(IronPayBaseSetting payBaseSetting)
        {
            Dictionary<string, object> md5Data = new Dictionary<string, object>();
            md5Data.Add("merchantNo", PayResponseModel.merchantNo);
            md5Data.Add("message", PayResponseModel.message);
            md5Data.Add("orderMoney", PayResponseModel.orderMoney);
            md5Data.Add("orderNo", PayResponseModel.orderNo);
            md5Data.Add("status", PayResponseModel.status);
            md5Data.Add("key", payBaseSetting.Key);
            _payRetrunSignStringToMd5 = StringObjectDirctionaryService.ConvertToUrlStringFormat(md5Data, "&");
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