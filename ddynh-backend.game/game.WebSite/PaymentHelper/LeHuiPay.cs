using game.Entity.DTO.ThirdPartyPay;
using game.Entity.Enums.ThirdPartyPay;
using game.Entity.Payment;
using game.Infrastructure;
using game.Infrastructure.Extensions;
using game.Infrastructure.Utility.ThirdParty;
using game.WebSite.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using static game.WebSite.PayConfig;

namespace game.WebSite.PaymentHelper
{
    public class LeHuiPay
    {
        /// <summary>
        /// 創建訂單
        /// </summary>
        /// <param name="order">資料庫訂單模型</param>
        /// <param name="logGuid">LOG File ID</param>
        /// <returns>回傳網址或訂單View</returns>
        public static string Post(CDPayOrder order, string logGuid)
        {
            var postAction = new ThirdPartyPaymentPostAction(logGuid, LeHuiPay_logFile);
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

            var obj = new PayTypeService();
            PayType payType = obj.GetPayTypeByOrder(order);
            bool isLogPayTypeSuccess = postAction.LogPayType(payType);
            if (!isLogPayTypeSuccess)
            {
                return postAction.GetReturnHtml();
            }

            //支付下单地址
            var PayBaseSetting = new LeHuiPayBaseSetting(merchant.InfoData, merchant.Url);
            var PayOrder = new LeHuiPayOrder(PayBaseSetting);
            PayOrder.SetOrderData(order);
            PayOrder.SetPayType(merchant.BankCode, payType.AmountFixed);
            PayOrder.Done();

            //紀錄交易編號
            string ThirdPartyOderID = PayOrder.GetThirdPartyOderID();
            postAction.LogThirdPartyOderID(ThirdPartyOderID);

            //紀錄MD5資料
            string md5String = PayOrder.GetMd5String();
            postAction.LogPayOrderPostSignBeforeString(md5String);

            //紀錄postJson 、 action url 和 ReturnString
            string payOrderUrl = PayBaseSetting.GetBaseUrl(LeHuiPay_CreateOrderApiUrl);
            var payOrderPost = PayOrder.GetPostData();
            string payOrderForm = postAction.GetFormAutoSubmitHtml(payOrderPost, payOrderUrl);
            postAction.LogPayOrderPostDataString(payOrderForm, payOrderUrl);

            ////LOG 回傳網址
            postAction.SetReturnHtml(payOrderForm);

            return postAction.GetReturnHtml();
        }

        /// <summary>
        /// 異步回調
        /// </summary>
        /// <param name="logGuid">LOG File ID</param>
        /// <param name="model">異步回調資料模型</param>
        /// <returns>是否回調正確並支付成功</returns>
        public static PayState Call(string logGuid, LeHuiPayResponseModel model)
        {
            int orderID = 0;
            int payTypeID = 0;
            string payTypeName = "樂匯支付";
            try
            {
                //是否跳出function return
                bool isCallReturn;
                var payCallAction = new ThirdPartyPaymentCallAction(logGuid, LeHuiPay_logFile);
                //非json使用此方法紀錄回傳值
                string responseString = JsonConvert.SerializeObject(model).ToString();
                payCallAction.LogResponseString(responseString);

                var payResponse = new LeHuiPayResponse(model);

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

                //確認交易是否成功
                isCallReturn = payCallAction.CheckTransactionIsSuccess(payResponse.IsTransactionSuccess());
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
                payTypeID = order.PayTypeID;
                CDMerchantUUID merchant = MerchantDataService.GetMerchantData(order);
                if (merchant == null)
                {
                    payCallAction.CheckResponseSign(false, string.Empty);
                    return PayState.Error;
                }

                var payBaseSetting = new LeHuiPayBaseSetting(merchant.InfoData, merchant.Url);
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

                //進行金幣充值
                string sourceIP = SystemHelper.UserIPAddress;
                int responseAmount = Convert.ToInt32(payResponse.GetPayAmount());
                
                payCallAction.FinishOrderAndCheckMonery(responseAmount, sourceIP);

                if (payCallAction.IsPayStateSuccess)
                    return PayState.Success;
                else
                    return PayState.Error;
            }
            catch (Exception ex)
            {
                string msg = $"[orderID]{orderID}[LogID]{logGuid}[payTypeID]{payTypeID}[payTypeName]{payTypeName}[message]{ex.ToString()}";

                MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
                {
                    Channel = SlackChannelEnum.n_api_payment,
                    Message = msg
                });

                string LogMessage = string.Format("支付回调 异常错误 {0}", ex.ToString());
                SiteHelper.Log(logGuid, LogMessage, LeHuiPay_logFile);
                return PayState.Error;
            }
        }
    }

    /// <summary>
    /// 吉寶第三方支付基本支付通道設定(商户号、密钥...)
    /// </summary>
    public class LeHuiPayBaseSetting : PaymentMerchantInfoBaseSetting
    {
        /// <summary>
        /// 建構式
        /// </summary>
        /// <param name="infoData">MerchantInfo</param>
        /// <param name="url">接口網址</param>
        public LeHuiPayBaseSetting(string infoData, string url)
        {
            var infoDataJson = JsonConvert.DeserializeObject<ThirdPartyPayMerchantInfo>(infoData);
            _merchantNo = infoDataJson.MERCHANT_NO;
            _key = infoDataJson.MERCHANT_KEY;
            _baseUrl = url + "/";
        }
    }

    /// <summary>
    /// 創建訂單
    /// </summary>
    public class LeHuiPayOrder
    {
        /// <summary>
        /// 支付所有需要欄位
        /// </summary>
        private LeHuiPayOrderModel PayOrderModle;
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
        public LeHuiPayOrder(LeHuiPayBaseSetting payBaseSetting)
        {
            JsonStr = string.Empty;
            Md5Data = new Dictionary<string, object>();
            PostData = new Dictionary<string, object>();

            Md5Sort = new List<string> { "merchantID", "notifyUrl", "outTradeNo", "payMoney", "channel", "key" };
            PostSort = new List<string> { "merchantID", "notifyUrl", "outTradeNo", "payMoney", "goodsName", "remark", "channel", "sign" };

            SetDefaultPayData(payBaseSetting);
        }

        /// <summary>
        /// 設定支付類型(service)
        /// </summary>
        /// <param name="payType">支付類型</param>
        public void SetPayType(string payType, bool isFixed)
        {
            if(isFixed)
            {
                payType = payType + "_FIX";
            }

            try
            {
                PayOrderModle.channel = (int)ExtensionOfEnum.GetEnumByTextValue<LeHuiPayTypeEnum>(payType);
            }
            catch
            {
                PayOrderModle.channel = (int)LeHuiPayTypeEnum.ZHIFUBAO_QR;
            }
        }

        /// <summary>
        /// 設定oder資料，商品ID使用orderID
        /// </summary>
        /// <param name="order"></param>
        public void SetOrderData(CDPayOrder order)
        {
            PayOrderModle.payMoney = order.Amount * 100;
            PayOrderModle.outTradeNo = ThirdPartyOderIDService.GetThirdPartyOderID(order.Id, "LeHuiPay", length: 20);
            PayOrderModle.goodsName = order.Id.ToString();
            PayOrderModle.remark = order.Id.ToString();
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
            return PayOrderModle.outTradeNo;
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
        public LeHuiPayOrderModel GetPayOrderModel()
        {
            return PayOrderModle;
        }

        public Dictionary<string,object> GetPostData()
        {
            return PostData;
        }

        /// <summary>
        /// 設定基本LeHuiPayOrderModle
        /// </summary>
        /// <param name="payBaseSetting">商戶號等等基本設定</param>
        private void SetDefaultPayData(LeHuiPayBaseSetting payBaseSetting)
        {
            PayOrderModle = new LeHuiPayOrderModel();
            PayOrderModle.merchantID = payBaseSetting.MerchantNo;
            PayOrderModle.key = payBaseSetting.Key;
            PayOrderModle.notifyUrl = LeHuiPay_NotifyUrl;
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
            //UrlEncode
            string md5StringUrl = StringObjectDirctionaryService.ConvertToValueAddSeparatorFormat(Md5Data, "&");
            Md5String = MD5Service.UpperCaseUrlEncode(md5StringUrl);
            //md5 sign
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
    }

    /// <summary>
    /// 支付第三方異步通知
    /// </summary>
    public class LeHuiPayResponse
    {
        /// <summary>
        /// 四方支付訂單回傳參數格式
        /// </summary>
        private LeHuiPayResponseModel PayResponseModel;
        /// <summary>
        /// MD5 SIGN前字串
        /// </summary>
        private string _payRetrunSignStringToMd5 = string.Empty;

        /// <summary>
        /// 建構式
        /// </summary>
        public LeHuiPayResponse(LeHuiPayResponseModel model)
        {
            PayResponseModel = model;
        }

        /// <summary>
        /// 異步回調通知是否成功
        /// </summary>
        public bool IsSuccess()
        {
            return PayResponseModel.resultCode == "00";
        }

        /// <summary>
        /// 交易是否成功
        /// </summary>
        public bool IsTransactionSuccess()
        {
            return PayResponseModel.status == "000";
        }

        /// <summary>
        /// 取得訂單ID(資料庫OrderListID)
        /// </summary>
        public int GetOrderID()
        {
            return ThirdPartyOderIDService.GetOrderID(PayResponseModel.outTradeNo, "LeHuiPay");
        }
        /// <summary>
        /// 取得交易訂單ID(給第三方的訂單ID)
        /// </summary>
        public string GetThirdPartyOderID()
        {
            return PayResponseModel.outTradeNo;
        }
        /// <summary>
        /// 取得金額
        /// </summary>
        public int GetPayAmount()
        {
            return Convert.ToInt32(PayResponseModel.payMoney / 100);
        }
        /// <summary>
        /// 回調簽名是否正確
        /// </summary>
        /// <param name="payBaseSetting">商戶號等基本設定</param>
        public bool IsSignTrue(LeHuiPayBaseSetting payBaseSetting)
        {
            Dictionary<string, object> signChkData = new Dictionary<string, object>();
            signChkData.Add("merchantID", PayResponseModel.merchantID);
            signChkData.Add("tradeNo", PayResponseModel.tradeNo);
            signChkData.Add("outTradeNo", PayResponseModel.outTradeNo);
            signChkData.Add("payMoney", PayResponseModel.payMoney);
            signChkData.Add("payTime", PayResponseModel.payTime);
            signChkData.Add("status", PayResponseModel.status);
            signChkData.Add("key", payBaseSetting.Key);

            string md5StringUrl = StringObjectDirctionaryService.ConvertToValueAddSeparatorFormat(signChkData, "&");
            _payRetrunSignStringToMd5 = MD5Service.UpperCaseUrlEncode(md5StringUrl);

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