using game.Entity.DTO.ThirdPartyPay;
using game.Entity.Enums.ThirdPartyPay;
using game.Entity.Payment;
using game.Infrastructure;
using game.Infrastructure.Utility.ThirdParty;
using game.WebSite.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using static game.WebSite.PayConfig;

namespace game.WebSite.PaymentHelper
{
    public class LiMaFuPay
    {
        /// <summary>
        /// 創建訂單
        /// </summary>
        /// <param name="order">資料庫訂單模型</param>
        /// <param name="logGuid">LOG File ID</param>
        /// <returns>回傳網址或訂單View</returns>
        public static string Post(CDPayOrder order, string logGuid)
        {
            var postAction = new ThirdPartyPaymentPostAction(logGuid, LiMaFuPay_logFile);
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
            var PayBaseSetting = new LiMaFuPayBaseSetting(merchant.InfoData, merchant.Url);
            var PayOrder = new LiMaFuPayOrder(PayBaseSetting);
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
            string payOrderUrl = PayBaseSetting.GetBaseUrl();
            string payOrderInput = PayOrder.GetPostInputString();
            string payOrderReturnString = SiteHelper.HttpPost(payOrderUrl, payOrderInput);
            postAction.LogPayOrderPostDataString(payOrderInput, payOrderUrl);
            postAction.LogPayReturnString(payOrderReturnString);

            //處理回傳json
            var payReturn = new LiMaFuPayReturn(payOrderReturnString);

            //確認回傳值是否正確
            bool isPayReturnSuccess = payReturn.IsSuccess();
            postAction.LogPayReturnMessage(isPayReturnSuccess, payReturn.GetMessage());
            if (!isPayReturnSuccess)
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
        /// <param name="model">異步回調資料模型</param>
        /// <returns>是否回調正確並支付成功</returns>
        public static PayState Call(string logGuid, LiMaFuPayResponseModel model)
        {
            int orderID = 0;
            int payTypeID = 0;
            string payTypeName = "立馬付支付";
            try
            {
                //是否跳出function return
                bool isCallReturn;
                var payCallAction = new ThirdPartyPaymentCallAction(logGuid, LiMaFuPay_logFile);
                //非json使用此方法紀錄回傳值
                string responseString = JsonConvert.SerializeObject(model).ToString();
                payCallAction.LogResponseString(responseString);

                var payResponse = new LiMaFuPayResponse(model);

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
                payTypeID = order.PayTypeID;
                CDMerchantUUID merchant = MerchantDataService.GetMerchantData(order);
                if (merchant == null)
                {
                    payCallAction.CheckResponseSign(false, string.Empty);
                    return PayState.Error;
                }

                var payBaseSetting = new LiMaFuPayBaseSetting(merchant.InfoData, merchant.Url);
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
                SiteHelper.Log(logGuid, LogMessage, LiMaFuPay_logFile);
                return PayState.Error;
            }
        }
    }

    /// <summary>
    /// 立馬付支付方式對照
    /// </summary>
    public class LiMaFuPayTypeBankCode
    {
        /// <summary>
        /// 支付方式
        /// </summary>
        private Dictionary<string, string> payTypeDic;
        /// <summary>
        /// 建構式
        /// </summary>
        public LiMaFuPayTypeBankCode()
        {
            payTypeDic = new Dictionary<string, string>();
            payTypeDic.Add("ZHIFUBAO_QR", "ZFB");
            payTypeDic.Add("ZHIFUBAO_H5", "ZFBH5");
            payTypeDic.Add("WEIXIN_QR", "WX");
            payTypeDic.Add("WEIXIN_H5", "WXH5");
            payTypeDic.Add("QUICKPASS_QR", "YSF");
            payTypeDic.Add("UNIONPAY_QR", "YLNET");
            payTypeDic.Add("UNIONPAY_ONLINE_BANK", "YLNETH5");
            payTypeDic.Add("QQ_QR", "QQ");
            payTypeDic.Add("QQ_H5", "QQH5");
            payTypeDic.Add("JDPAY_QR", "JD");
            payTypeDic.Add("JDPAY_H5", "JDH5");
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

            return "ZFB";
        }
    }

    /// <summary>
    /// 第三方支付基本支付通道設定(商户号、密钥...)
    /// </summary>
    public class LiMaFuPayBaseSetting : PaymentMerchantInfoBaseSetting
    {
        /// <summary>
        /// 建構式
        /// </summary>
        /// <param name="infoData">MerchantInfo</param>
        /// <param name="url">接口網址</param>
        public LiMaFuPayBaseSetting(string infoData, string url)
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
    public class LiMaFuPayOrder
    {
        /// <summary>
        /// 支付所有需要欄位
        /// </summary>
        private LiMaFuPayOrderModel PayOrderModle;
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
        public LiMaFuPayOrder(LiMaFuPayBaseSetting payBaseSetting)
        {
            JsonStr = string.Empty;
            Md5Data = new Dictionary<string, object>();
            PostData = new Dictionary<string, object>();

            PostSort = new List<string> { "merchant_id", "orderid", "paytype", "notifyurl", "callbackurl","userip", "money", "sign"};
            Md5Sort = new List<string>(PostSort);
            Md5Sort.Remove("userip");
            Md5Sort.Remove("sign");

            SetDefaultPayData(payBaseSetting);
        }

        /// <summary>
        /// 設定支付類型(service)
        /// </summary>
        /// <param name="payType">支付類型</param>
        public void SetPayType(string payType)
        {
            var payTypeObj = new LiMaFuPayTypeBankCode();
            PayOrderModle.paytype = payTypeObj.GetPayType(payType);
        }

        /// <summary>
        /// 設定oder資料，商品ID使用orderID
        /// </summary>
        /// <param name="order"></param>
        public void SetOrderData(CDPayOrder order)
        {
            PayOrderModle.money = order.Amount.ToString("F2");
            PayOrderModle.orderid = ThirdPartyOderIDService.GetThirdPartyOderID(order.Id, "LiMaFuPay", length: 20);
            PayOrderModle.userip = order.IPAddress;
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
            return PayOrderModle.orderid;
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

        public Dictionary<string, object> GetPostData()
        {
            return PostData;
        }

        /// <summary>
        /// 設定基本LiMaFuPayOrderModle
        /// </summary>
        /// <param name="payBaseSetting">商戶號等等基本設定</param>
        private void SetDefaultPayData(LiMaFuPayBaseSetting payBaseSetting)
        {
            PayOrderModle = new LiMaFuPayOrderModel();
            PayOrderModle.merchant_id = payBaseSetting.MerchantNo;
            PayOrderModle.key = payBaseSetting.Key;
            PayOrderModle.notifyurl = LiMaFuPay_NotifyUrl;
            PayOrderModle.callbackurl = FinishUrl;
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
            Md5Sort.Add("key");
            //排序MD5字串
            SetDictionary(Md5Sort, Md5Data);
            //UrlEncode
            Md5String = StringObjectDirctionaryService.ConvertToValueAddSeparatorFormat(Md5Data);
            //md5 sign
            PayOrderModle.sign = MD5Service.Hash(Md5String, Encoding.UTF8).ToLower();
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
    /// 第三方訂單return
    /// </summary>
    public class LiMaFuPayReturn
    {
        /// <summary>
        /// LiMaFuPay訂單回傳參數格式
        /// </summary>
        private LiMaFuPayReturnModel PayReturnModel;

        /// <summary>
        /// 建構式
        /// </summary>
        public LiMaFuPayReturn(string jsonString)
        {
            PayReturnModel = JsonConvert.DeserializeObject<LiMaFuPayReturnModel>(jsonString);
        }

        /// <summary>
        /// 檢查回傳json是否成功
        /// </summary>
        public bool IsSuccess()
        {
            return PayReturnModel.status == (byte)ThirdPartyPayReturnStatusEnum.SUCCESS;
        }

        /// <summary>
        /// 取得回傳訊息
        /// </summary>
        /// <returns></returns>
        public string GetMessage()
        {
            return PayReturnModel.message;
        }

        /// <summary>
        /// 取得回傳網址
        /// </summary>
        public string GetReturnUrl()
        {
            if (!IsSuccess())
                return string.Empty;

            return PayReturnModel.data.url;
        }
    }

    /// <summary>
    /// 支付第三方異步通知
    /// </summary>
    public class LiMaFuPayResponse
    {
        /// <summary>
        /// 四方支付訂單回傳參數格式
        /// </summary>
        private LiMaFuPayResponseModel PayResponseModel;
        /// <summary>
        /// MD5 SIGN前字串
        /// </summary>
        private string _payRetrunSignStringToMd5 = string.Empty;

        /// <summary>
        /// 建構式
        /// </summary>
        public LiMaFuPayResponse(LiMaFuPayResponseModel model)
        {
            PayResponseModel = model;
        }

        /// <summary>
        /// 異步回調通知是否成功
        /// </summary>
        public bool IsSuccess()
        {
            return PayResponseModel.status == (byte)ThirdPartyPayReturnStatusEnum.SUCCESS;
        }

        /// <summary>
        /// 取得訂單ID(資料庫OrderListID)
        /// </summary>
        public int GetOrderID()
        {
            return ThirdPartyOderIDService.GetOrderID(PayResponseModel.orderid, "LiMaFuPay");
        }
        /// <summary>
        /// 取得交易訂單ID(給第三方的訂單ID)
        /// </summary>
        public string GetThirdPartyOderID()
        {
            return PayResponseModel.orderid;
        }
        /// <summary>
        /// 取得金額
        /// </summary>
        public int GetPayAmount()
        {
            return PayResponseModel.moneyInt;
        }
        /// <summary>
        /// 回調簽名是否正確
        /// </summary>
        /// <param name="payBaseSetting">商戶號等基本設定</param>
        public bool IsSignTrue(LiMaFuPayBaseSetting payBaseSetting)
        {
            Dictionary<string, object> signChkData = new Dictionary<string, object>();
            signChkData.Add("merchant_id", PayResponseModel.merchant_id);
            signChkData.Add("orderid", PayResponseModel.orderid);
            signChkData.Add("money", PayResponseModel.money);
            signChkData.Add("key", payBaseSetting.Key);

            _payRetrunSignStringToMd5 = StringObjectDirctionaryService.ConvertToValueAddSeparatorFormat(signChkData);

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