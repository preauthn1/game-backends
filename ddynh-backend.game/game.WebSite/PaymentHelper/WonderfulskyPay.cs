using game.Entity.DTO.ThirdPartyPay;
using game.Entity.Enums.ThirdPartyPay;
using game.Entity.Payment;
using game.Infrastructure;
using game.Infrastructure.Utility.ThirdParty;
using game.WebSite.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using static game.WebSite.PayConfig;

namespace game.WebSite.PaymentHelper
{
    public static class WonderfulskyPay
    {
        /// <summary>
        /// 創建訂單
        /// </summary>
        /// <param name="order">資料庫訂單模型</param>
        /// <param name="logGuid">LOG File ID</param>
        /// <returns>回傳網址或訂單View</returns>
        public static string Post(CDPayOrder order, string logGuid)
        {
            var postAction = new ThirdPartyPaymentPostAction(logGuid, WonderfulskyPay_logFile);
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
            var PayBaseSetting = new WonderfulskyPayBaseSetting(merchant.InfoData, merchant.Url);
            var PayOrder = new WonderfulskyPayOrder(PayBaseSetting);
            PayOrder.SetOrderData(order);
            PayOrder.SetPayType(merchant.BankCode);
            PayOrder.Done();

            //紀錄交易編號
            string ThirdPartyOderID = PayOrder.GetThirdPartyOderID();
            postAction.LogThirdPartyOderID(ThirdPartyOderID);

            //紀錄MD5資料
            string md5String = PayOrder.GetSignBeforeString();
            postAction.LogPayOrderPostSignBeforeString(md5String);

            //紀錄postJson 、 action url 和 ReturnString
            string payOrderUrl = PayBaseSetting.GetBaseUrl(WonderfulskyPay_CreateOrderApiUrl);
            string payOrderInput = PayOrder.GetPostInputString();
            postAction.LogPayOrderPostDataString(payOrderInput, payOrderUrl);

            string payOrderReturnString = SiteHelper.HttpPost(payOrderUrl, payOrderInput); 
            postAction.LogPayReturnString(payOrderReturnString);

            //若BankCode = 支付寶QR或是屬於H5支付方式，且回傳值有<script>，則直接回傳
            if(IsReturnRedirectHtml(merchant.BankCode, payOrderReturnString))
            {
                return payOrderReturnString;
            }

            //處理回傳json
            var payReturn = new WonderfulskyPayReturn(payOrderReturnString);

            //確認回傳值是否正確
            bool isPayReturnSuccess = payReturn.IsSuccess();
            postAction.LogPayReturnMessage(isPayReturnSuccess, payReturn.GetMessage());
            if (!isPayReturnSuccess)
            {
                return postAction.GetReturnHtml();
            }

            //確認回傳值簽名是否正確
            bool isPayReturnSignTrue = payReturn.IsSignTrue(PayBaseSetting);
            postAction.LogPayReturnSign(isPayReturnSignTrue, payReturn.GetSignBeforeString());
            if (!isPayReturnSignTrue)
            {
                return postAction.GetReturnHtml();
            }

            //LOG 回傳網址
            string returnHtml = postAction.GenerateRedirectHtmlOpenBrowser(payReturn.GetReturnUrl());
            postAction.SetReturnHtml(returnHtml);
            return postAction.GetReturnHtml();
        }

        private static bool IsReturnRedirectHtml(string bankcode, string returnString)
        {
            bool IsReturnScript = returnString.Contains("<script>");
            bool IsH5PayType = bankcode.Contains("H5");
            if ((IsH5PayType || bankcode == "ZHIFUBAO_QR" || bankcode == "UNIONPAY_QR") && IsReturnScript)
                return true;

            return false;
        }


        /// <summary>
        /// 異步回調
        /// </summary>
        /// <param name="logGuid">LOG File ID</param>
        /// <param name="model">異步回調資料模型</param>
        /// <returns>是否回調正確並支付成功</returns>
        public static PayState Call(string logGuid, WonderfulskyPayResponseModel model)
        {
            int orderID = 0;
            int payTypeID = 0;
            string payTypeName = "天空付支付";
            try
            {
                //是否跳出function return
                bool isCallReturn;
                var payCallAction = new ThirdPartyPaymentCallAction(logGuid, WonderfulskyPay_logFile);
                string responseString = JsonConvert.SerializeObject(model).ToString();
                payCallAction.LogResponseString(responseString);

                var payResponse = new WonderfulskyPayResponse(model);

                //SYFPAY0000...
                string payOrderID = payResponse.GetThirdPartyOderID();
                //取得實際orderid
                orderID = payResponse.GetOrderID();

                //設定訂單資訊(紀錄LOG內容也需要)
                payCallAction.SetOrderData(orderID, payOrderID);

                //確認回調是否成功
                isCallReturn = payCallAction.CheckResponseIsSuccess(payResponse.IsSuccess());
                if (isCallReturn)
                {
                    if (payCallAction.IsPayStateSuccess)
                        return PayState.Success;
                    else
                        return PayState.Error;
                }

                isCallReturn = payCallAction.CheckOrderData();
                //若訂單確認失敗則返回
                if (isCallReturn)
                {
                    return payCallAction.PayState;
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

                var payBaseSetting = new WonderfulskyPayBaseSetting(merchant.InfoData, merchant.Url);
                bool isSignTrue = payResponse.IsSignTrue(payBaseSetting);
                string signBeforeString = payResponse.GetSignBeforeString();
                isCallReturn = payCallAction.CheckResponseSign(isSignTrue, signBeforeString);
                if (isCallReturn)
                {
                    return payCallAction.PayState;
                }

                //進行金幣充值
                string sourceIP = SystemHelper.UserIPAddress;
                int responseAmount = payResponse.GetPayAmount();

                payCallAction.FinishOrderAndCheckMonery(responseAmount, sourceIP);
                return payCallAction.PayState;
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
                SiteHelper.Log(logGuid, LogMessage, WonderfulskyPay_logFile);
                return PayState.Error;
            }
        }

    }

    /// <summary>
    /// 支付方式對照
    /// </summary>
    public class WonderfulskyPayTypeBankCode
    {
        /// <summary>
        /// 支付方式
        /// </summary>
        private Dictionary<string, string> payTypeDic;
        /// <summary>
        /// 建構式
        /// </summary>
        public WonderfulskyPayTypeBankCode()
        {
            payTypeDic = new Dictionary<string, string>();
            payTypeDic.Add("ZHIFUBAO_QR", "alipay");
            payTypeDic.Add("ZHIFUBAO_H5", "alipayh5");
            payTypeDic.Add("WEIXIN_QR", "wechat");
            payTypeDic.Add("WEIXIN_H5", "wechath5");
            payTypeDic.Add("QQ_QR", "qqpay");
            payTypeDic.Add("QQ_H5", "qqpayh5");
            payTypeDic.Add("JDPAY_QR", "jdpay");
            payTypeDic.Add("EXPRESSPAY_H5", "quickpay");
            payTypeDic.Add("UNIONPAY_QR", "unionpay");
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

            return payTypeDic["ZHIFUBAO_QR"];
        }
    }

    /// <summary>
    /// 第三方支付基本支付通道設定(商户号、密钥...)
    /// </summary>
    public class WonderfulskyPayBaseSetting : PaymentMerchantInfoBaseSetting
    {
        private string _publicKeyContent;
        private string _privateKeyContent;
        public string PublicKeyContent { get { return _publicKeyContent; } }
        public string PrivateKeyContent { get { return _privateKeyContent; } }

        /// <summary>
        /// 建構式
        /// </summary>
        /// <param name="infoData">MerchantInfo</param>
        /// <param name="url">接口網址</param>
        public WonderfulskyPayBaseSetting(string infoData, string url)
        {
            var infoDataJson = JsonConvert.DeserializeObject<ThirdPartyPayMerchantInfo>(infoData);
            _merchantNo = infoDataJson.MERCHANT_NO;
            _key = infoDataJson.MERCHANT_KEY;
            _publicKeyContent = infoDataJson.PUBLIC_KEY_CONTENT;
            _privateKeyContent = infoDataJson.PRIVATE_KEY_CONTENT;
            _baseUrl = url + "/";
        }
    }

    /// <summary>
    /// 創建訂單
    /// </summary>
    public class WonderfulskyPayOrder
    {
        /// <summary>
        /// 支付所有需要欄位
        /// </summary>
        private WonderfulskyPayOrderModel PayOrderModle;
        /// <summary>
        /// md5排序
        /// </summary>
        private List<string> Md5Sort;
        /// <summary>
        /// 此字串md5之後將存成sign
        /// </summary>
        private string Md5String;
        /// <summary>
        /// 傳給第三方的post
        /// </summary>
        private string PostInputString;

        /// <summary>
        /// 建構式
        /// </summary>
        public WonderfulskyPayOrder(WonderfulskyPayBaseSetting payBaseSetting)
        {
            Md5Sort = new List<string> { "amount", "channel", "currency", "merchantid", "mid", "notifyurl", "returnurl", "service"};

            SetDefaultPayData(payBaseSetting);
        }

        /// <summary>
        /// 設定支付類型(service)
        /// </summary>
        /// <param name="payType">支付類型</param>
        public void SetPayType(string payType)
        {
            var payTypeObj = new WonderfulskyPayTypeBankCode();
            PayOrderModle.channel = payTypeObj.GetPayType(payType);
        }

        /// <summary>
        /// 設定oder資料，商品ID使用orderID
        /// </summary>
        /// <param name="order"></param>
        public void SetOrderData(CDPayOrder order)
        {
            PayOrderModle.amount = order.Amount;
            PayOrderModle.merchantid = ThirdPartyOderIDService.GetThirdPartyOderID(order.Id, "WonderfulskyPay", length: 25);
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
            return PayOrderModle.merchantid;
        }

        public string GetSignBeforeString()
        {
            return Md5String;
        }

        public string GetPostInputString()
        {
            return PostInputString;
        }

        /// <summary>
        /// 設定基本WonderfulskyPayOrderModle
        /// </summary>
        /// <param name="payBaseSetting">商戶號等等基本設定</param>
        private void SetDefaultPayData(WonderfulskyPayBaseSetting payBaseSetting)
        {
            PayOrderModle = new WonderfulskyPayOrderModel();
            PayOrderModle.mid = payBaseSetting.MerchantNo;
            PayOrderModle.key = payBaseSetting.Key;
            PayOrderModle.rsaPublicKey = payBaseSetting.PublicKeyContent;
            PayOrderModle.rsaPrivateKey = payBaseSetting.PrivateKeyContent;
            PayOrderModle.notifyurl = WonderfulskyPay_NotifyUrl;
            PayOrderModle.returnurl = FinishUrl;
            PayOrderModle.service = "Payment";
            PayOrderModle.currency = "CNY";
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
            var Md5Data = new Dictionary<string, object>();
            Md5Sort.Sort();
            //排序MD5字串
            SetDictionary(Md5Sort, Md5Data);
            //UrlEncode
            Md5String = StringObjectDirctionaryService.ConvertToUrlStringFormat(Md5Data, "&") + "&" + PayOrderModle.key;
            //md5 sign
            PayOrderModle.sign = RSAService.Sign(PayOrderModle.rsaPrivateKey, Md5String);
        }

        /// <summary>
        /// 設定post資料(JSON 或者INPUT FORM)
        /// </summary>
        private void SetPost()
        {
            var PostData = new Dictionary<string, object>();
            Md5Sort.Add("sign");
            SetDictionary(Md5Sort, PostData);
            PostInputString = StringObjectDirctionaryService.ConvertToUrlStringFormat(PostData, "&" , true);
        }
    }

    /// <summary>
    /// 第三方訂單return
    /// </summary>
    public class WonderfulskyPayReturn
    {
        /// <summary>
        /// WonderfulskyPay訂單回傳參數格式
        /// </summary>
        private WonderfulskyPayReturnModel PayReturnModel;
        private string _SignBeforeString;

        /// <summary>
        /// 建構式
        /// </summary>
        public WonderfulskyPayReturn(string jsonString)
        {
            _SignBeforeString = string.Empty;
            PayReturnModel = JsonConvert.DeserializeObject<WonderfulskyPayReturnModel>(jsonString);
        }

        /// <summary>
        /// 檢查回傳json是否成功
        /// </summary>
        public bool IsSuccess()
        {
            return PayReturnModel.result == (int)ThirdPartyPayReturnStatusEnum.SUCCESS && PayReturnModel.code == null;
        }

        /// <summary>
        /// 取得回傳網址
        /// </summary>
        public string GetReturnUrl()
        {
            if (!IsSuccess())
                return string.Empty;

            return PayReturnModel.qrcodeurl;
        }

        /// <summary>
        /// 檢查sign
        /// </summary>
        public bool IsSignTrue(WonderfulskyPayBaseSetting payBaseSetting)
        {
            Dictionary<string, object> signData = new Dictionary<string, object>();
            signData.Add("merchantid", PayReturnModel.merchantid);
            signData.Add("mid", PayReturnModel.mid);
            signData.Add("qrcodeurl", PayReturnModel.qrcodeurl);
            signData.Add("result", PayReturnModel.result);
            _SignBeforeString = StringObjectDirctionaryService.ConvertToUrlStringFormat(signData, "&") + "&" + payBaseSetting.Key;
            return RSAService.SignChk(payBaseSetting.PublicKeyContent, _SignBeforeString, PayReturnModel.sign);
        }

        public string GetSignBeforeString()
        {
            return _SignBeforeString;
        }

        public string GetMessage()
        {
            //此支付只有錯誤訊息
            if (IsSuccess())
                return string.Empty;

            return PayReturnModel.message;
        }
    }

    /// <summary>
    /// 支付第三方異步通知
    /// </summary>
    public class WonderfulskyPayResponse
    {
        /// <summary>
        /// 四方支付訂單回傳參數格式
        /// </summary>
        private WonderfulskyPayResponseModel PayResponseModel;
        /// <summary>
        /// MD5 SIGN前字串
        /// </summary>
        private string _SignBeforeString = string.Empty;

        /// <summary>
        /// 建構式
        /// </summary>
        public WonderfulskyPayResponse(WonderfulskyPayResponseModel model)
        {
            PayResponseModel = model;
        }

        public bool IsSuccess()
        {
            return PayResponseModel.status == "1";
        }

        /// <summary>
        /// 取得訂單ID(資料庫OrderListID)
        /// </summary>
        public int GetOrderID()
        {
            return ThirdPartyOderIDService.GetOrderID(PayResponseModel.merchantid, "WonderfulskyPay");
        }
        /// <summary>
        /// 取得交易訂單ID(給第三方的訂單ID)
        /// </summary>
        public string GetThirdPartyOderID()
        {
            return PayResponseModel.merchantid;
        }
        /// <summary>
        /// 取得金額
        /// </summary>
        public int GetPayAmount()
        {
            return PayResponseModel.amountInt;
        }
        /// <summary>
        /// 回調簽名是否正確
        /// </summary>
        /// <param name="payBaseSetting">商戶號等基本設定</param>
        public bool IsSignTrue(WonderfulskyPayBaseSetting payBaseSetting)
        {
            var sortList = new List<string> { "mid", "merchantid", "systemid", "amount", "status", "time" };
            sortList.Sort();
            Dictionary<string, object> signChkData = new Dictionary<string, object>();
            SetDictionary(sortList, signChkData);

            _SignBeforeString = StringObjectDirctionaryService.ConvertToUrlStringFormat(signChkData, "&") + "&" + payBaseSetting.Key;
            return RSAService.SignChk(payBaseSetting.PublicKeyContent, _SignBeforeString, PayResponseModel.sign);
        }
        /// <summary>
        /// 取得驗證簽名使用字串
        /// </summary>
        public string GetSignBeforeString()
        {
            return _SignBeforeString;
        }

        private void SetDictionary(List<string> stringArr, Dictionary<string, object> dictionary)
        {
            foreach (var key in stringArr)
            {
                foreach (PropertyInfo prop in PayResponseModel.GetType().GetProperties())
                {
                    if (prop.Name == key)
                    {
                        var value = prop.GetValue(PayResponseModel, null);
                        if(value != null)
                            dictionary.Add(key, value);
                    }
                }
            }
        }
    }

}