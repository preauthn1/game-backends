using game.WebSite.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using static game.WebSite.PayConfig;
using game.Infrastructure.Extensions;
using game.Entity.Payment;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using game.Infrastructure.Utility.ThirdParty;
using game.Infrastructure;
using game.Entity.Enums.ThirdPartyPay;
using game.Entity.DTO.ThirdPartyPay;

namespace game.WebSite.PaymentHelper
{
    /// <summary>
    /// 通匯
    /// </summary>
    public static class THPay
    {
        /// <summary>
        /// 创建订单(跳转网页链接)
        /// </summary>
        /// <param name="order">訂單</param>
        /// <param name="logGuid">LogGuID</param>
        /// <returns>包括QRCode與訂單ID和金額的HTML頁面</returns>
        public static string Post(CDPayOrder order, string logGuid)
        {
            var postAction = new ThirdPartyPaymentPostAction(logGuid, THPay_logFile);
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
            var PayBaseSetting = new THPayBaseSetting(merchant.InfoData, merchant.Url);
            var PayOrder = new THPayOrder(PayBaseSetting);
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
            string payOrderJson = PayOrder.GetPostJsonString();
            string payOrderReturnString = SiteHelper.HttpPostJson(payOrderUrl, payOrderJson);
            postAction.LogPayOrderPostDataString(payOrderJson, payOrderUrl);
            postAction.LogPayReturnString(payOrderReturnString);

            //處理回傳json
            var payReturn = new THPayReturn(payOrderReturnString);

            //確認json是否正確
            if(!payReturn.IsSuccess())
            {
                return postAction.GetReturnHtml();
            }

            //此支付不產生頁面，下方開始產生HTML
            string returnUrl = payReturn.GetReturnHtml(ThirdPartyOderID);
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
                var payCallAction = new ThirdPartyPaymentCallAction(logGuid, THPay_logFile);
                payCallAction.LogResponseString();
                string responseString = payCallAction.GetResponseString();
                
                var payResponse = new THPayResponse(responseString);

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

                var payBaseSetting = new THPayBaseSetting(merchant.InfoData, merchant.Url);
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
                SiteHelper.Log(logGuid, LogMessage, THPay_logFile);
                return PayState.Error;
            }

        }
    }

    /// <summary>
    /// THPay第三方支付基本支付通道設定(商户号、密钥...)
    /// </summary>
    public class THPayBaseSetting : PaymentMerchantInfoBaseSetting
    {
        /// <summary>
        /// 建構式
        /// </summary>
        public THPayBaseSetting(string InfoData, string Url)
        {
            var InfoDataJson = Newtonsoft.Json.JsonConvert.DeserializeObject<ThirdPartyPayMerchantInfo>(InfoData);
            _merchantNo = InfoDataJson.MERCHANT_NO;
            _key = InfoDataJson.MERCHANT_KEY;
            _baseUrl = Url;
        }
    }
    /// <summary>
    /// 創造第三方訂單
    /// </summary>
    public class THPayOrder
    {
        /// <summary>
        /// 支付所有需要欄位
        /// </summary>
        private THPayOrderModel PayOrderModle;
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
        public THPayOrder(THPayBaseSetting PayBaseSetting)
        {
            JsonStr = string.Empty;
            Md5Data = new Dictionary<string, object>();
            PostData = new Dictionary<string, object>();

            Md5Sort = new List<string> { "uid", "price", "paytype", "notify_url", "return_url", "user_order_no", "token" };
            PostSort = new List<string> { "uid", "price", "paytype", "notify_url", "return_url", "user_order_no", "note", "cuid", "tm", "sign" };

            SetDefaultPayData(PayBaseSetting);
        }
        /// <summary>
        /// 設定基本THPayOrderModle
        /// </summary>
        /// <param name="PayBaseSetting">商戶號等等基本設定</param>
        private void SetDefaultPayData(THPayBaseSetting PayBaseSetting)
        {
            PayOrderModle = new THPayOrderModel();
            PayOrderModle.uid = PayBaseSetting.MerchantNo;
            PayOrderModle.token = [REDACTED];
            PayOrderModle.price = "0.00";
            PayOrderModle.paytype = (int)THPayTypeEnum.ZHIFUBAO_QR;
            PayOrderModle.notify_url = THPay_NotifyUrl;
            PayOrderModle.return_url = FinishUrl;
            PayOrderModle.user_order_no = string.Empty;
            PayOrderModle.note = string.Empty;
            PayOrderModle.cuid = string.Empty;
            PayOrderModle.tm = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            PayOrderModle.sign = string.Empty;
        }
        /// <summary>
        /// 設定支付類型(service)
        /// </summary>
        /// <param name="payType">支付類型</param>
        public void SetPayType(string payType)
        {
            try
            {
                PayOrderModle.paytype = (int)ExtensionOfEnum.GetEnumByTextValue<THPayTypeEnum>(payType);
            }
            catch
            {
                PayOrderModle.paytype = (int)THPayTypeEnum.ZHIFUBAO_QR;
            }
        }
        /// <summary>
        /// 設定oder資料，商品ID使用orderID
        /// </summary>
        /// <param name="order"></param>
        public void SetOrderData(CDPayOrder order)
        {
            PayOrderModle.user_order_no = ThirdPartyOderIDService.GetThirdPartyOderID(order.Id, "THPay", 20);
            PayOrderModle.price = order.Amount.ToString("F2");
            PayOrderModle.note = order.Id.ToString();
            PayOrderModle.cuid = order.UserID.ToString();
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
            Md5String = StringObjectDirctionaryService.ConvertToValueAddSeparatorFormat(Md5Data);
            PayOrderModle.sign = MD5Service.Hash(Md5String, Encoding.UTF8).ToLower();
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
            return PayOrderModle.user_order_no;
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
        public THPayOrderModel GetPayOrderModel()
        {
            return PayOrderModle;
        }
    }

    /// <summary>
    /// 第三方訂單return
    /// </summary>
    public class THPayReturn
    {
        /// <summary>
        /// THPay訂單回傳參數格式
        /// </summary>
        private THPayReturnModel PayReturnModel;
        /// <summary>
        /// 建構式
        /// </summary>
        public THPayReturn(string jsonString)
        {
            PayReturnModel = JsonConvert.DeserializeObject<THPayReturnModel>(jsonString);
        }
        /// <summary>
        /// 檢查回傳json是否成功
        /// </summary>
        public bool IsSuccess()
        {
            return PayReturnModel.Code == (byte)THPayPayReturnSuccessEnum.Success;
        }
        /// <summary>
        /// 取得回傳頁面
        /// </summary>
        public string GetReturnHtml(string thirdPartyOderID)
        {
            string Html = "<div>";
            Html += "<div>" + "<img src='" + PayReturnModel.QRCodeLink + "' style = 'max-height:250px;' />" + "</div>";
            Html += "<div>訂單編號：" + thirdPartyOderID + "</div>";
            Html += "<div>支付寶訂單編號：" + PayReturnModel.OrderNo + "</div>";
            Html += "<div>實際支付金額：" + PayReturnModel.RealPrice + "</div>";
            return Html;
        }
    }

    /// <summary>
    /// 第三方異步通知
    /// </summary>
    public class THPayResponse
    {
        /// <summary>
        /// THPay訂單回傳參數格式
        /// </summary>
        private THPayResponseModel PayResponseModel;
        /// <summary>
        /// md5 sign string
        /// </summary>
        private string _payRetrunSignStringToMd5 = string.Empty;
        /// <summary>
        /// 建構式
        /// </summary>
        public THPayResponse(string jsonString)
        {
            PayResponseModel = JsonConvert.DeserializeObject<THPayResponseModel>(jsonString);
        }
        /// <summary>
        /// 取得訂單ID(資料庫OrderListID)
        /// </summary>
        public int GetOrderID()
        {
            return Convert.ToInt32(PayResponseModel.note);
        }
        /// <summary>
        /// 取得交易訂單ID(給第三方的訂單ID)
        /// </summary>
        public string GetThirdPartyOderID()
        {
            return PayResponseModel.user_order_no;
        }
        /// <summary>
        /// 回調是否成功
        /// </summary>
        public bool IsSuccess()
        {
            return PayResponseModel.status == (int)THPayResponseStatusEnum.PaySuccess;
        }
        /// <summary>
        /// 取得金額
        /// </summary>
        public long GetPayAmount()
        {
            return (long)Decimal.Parse(PayResponseModel.price);
        }
        /// <summary>
        /// 回調簽名是否正確
        /// </summary>
        /// <param name="payBaseSetting">商戶號等基本設定</param>
        public bool IsSignTrue(THPayBaseSetting payBaseSetting)
        {
            Dictionary<string, object> signChkData = new Dictionary<string, object>();
            signChkData.Add("user_order_no", PayResponseModel.user_order_no);
            signChkData.Add("orderno", PayResponseModel.orderno);
            signChkData.Add("tradeno", PayResponseModel.tradeno);
            signChkData.Add("price", PayResponseModel.price);
            signChkData.Add("realprice", PayResponseModel.realprice);
            signChkData.Add("status ", PayResponseModel.status);
            signChkData.Add("token", payBaseSetting.Key);
            _payRetrunSignStringToMd5 = StringObjectDirctionaryService.ConvertToValueAddSeparatorFormat(signChkData);
            string signChk = MD5Service.Hash(_payRetrunSignStringToMd5, Encoding.UTF8).ToLower();
            return PayResponseModel.sign2 == signChk;
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