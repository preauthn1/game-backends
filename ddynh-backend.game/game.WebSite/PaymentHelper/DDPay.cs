using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using game.Entity.Payment;
using game.Infrastructure;
using game.WebSite.Services;
using static game.WebSite.PayConfig;
using game.Infrastructure.Extensions;
using System.Text;
using game.Infrastructure.Utility.ThirdParty;
using game.Entity.DTO.ThirdPartyPay;

namespace game.WebSite.PaymentHelper
{
    /// <summary>
    /// 叮叮寶
    /// </summary>
    public class DDPay
    {
        /// <summary>
        /// 创建订单(跳转网页链接)
        /// </summary>
        /// <param name="order">訂單</param>
        /// <param name="logGuid">LogGuID</param>
        /// <returns></returns>
        public static string Post(CDPayOrder order, string logGuid)
        {
            var postAction = new ThirdPartyPaymentPostAction(logGuid, DDPay_logFile);
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
            var PayBaseSetting = new DDPayBaseSetting(merchant.InfoData, merchant.Url);
            var PayOrder = new DDPayOrder(PayBaseSetting);
            PayOrder.SetOrderData(order);
            PayOrder.SetPayType(merchant.BankCode);
            PayOrder.Done();

            //紀錄交易編號
            string transNo = PayOrder.GetTransNo();
            postAction.LogThirdPartyOderID(transNo);

            //紀錄MD5資料
            string md5String = PayOrder.GetMd5String();
            postAction.LogPayOrderPostSignBeforeString(md5String);

            //紀錄postJson 和 action url 和 returnString
            string payOrderUrl = PayBaseSetting.GetBaseUrl();
            string payOrderJson = PayOrder.GetPostJsonString();
            string payOrderReturnString = SiteHelper.HttpPostJson(payOrderUrl, payOrderJson);
            postAction.LogPayOrderPostDataString(payOrderJson, payOrderUrl);
            postAction.LogPayReturnString(payOrderReturnString);

            //紀錄回傳值
            var payOrderReturn = new DDPayOrderReturn(payOrderReturnString);
            bool isPayOrderReturnSuccess = payOrderReturn.IsSuccess();
            string payOrderReturnMessage = payOrderReturn.GetReturnMessage();

            postAction.LogPayReturnMessage(isPayOrderReturnSuccess, payOrderReturnMessage);
            if (!isPayOrderReturnSuccess)
            {
                return postAction.GetReturnHtml();
            }

            string returnUrl = payOrderReturn.GetUrl();
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
                var payCallAction = new ThirdPartyPaymentCallAction(logGuid, DDPay_logFile);
                payCallAction.LogResponseString();
                string responseString = payCallAction.GetResponseString();

                var payResponse = new DDPayResponse(responseString);

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

                var payBaseSetting = new DDPayBaseSetting(merchant.InfoData, merchant.Url);
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
                SiteHelper.Log(logGuid, LogMessage, DDPay_logFile);
                return PayState.Error;
            }
        }
    }

    /// <summary>
    /// DDPay第三方支付基本支付通道設定(商户号、密钥...)
    /// </summary>
    public class DDPayBaseSetting : PaymentMerchantInfoBaseSetting
    {
        /// <summary>
        /// 建構式
        /// </summary>
        public DDPayBaseSetting(string InfoData, string Url)
        {
            var InfoDataJson = Newtonsoft.Json.JsonConvert.DeserializeObject<ThirdPartyPayMerchantInfo>(InfoData);
            _merchantNo = InfoDataJson.MERCHANT_NO;
            _key = InfoDataJson.MERCHANT_KEY;
            _baseUrl = Url + "/";
        }
    }

    /// <summary>
    /// 支付下单
    /// </summary>
    public class DDPayOrder
    {
        /// <summary>
        /// 支付所有需要欄位
        /// </summary>
        private DDPayOrderModel payData;
        /// <summary>
        /// md5排序
        /// </summary>
        private List<string> md5Sort;
        /// <summary>
        /// post排序
        /// </summary>
        private List<string> postSort;
        /// <summary>
        /// md5資料結構
        /// </summary>
        private Dictionary<string, object> md5Data;
        /// <summary>
        /// post資料結構
        /// </summary>
        private Dictionary<string, object> postData;
        /// <summary>
        /// 資料結構
        /// </summary>
        private Dictionary<string, object> defaultData;
        /// <summary>
        /// json post string
        /// </summary>
        private string JsonStr;
        /// <summary>
        /// payType(service)資料結構
        /// </summary>
        private Dictionary<string, string> payTypeData;

        /// <summary>
        /// 建構式
        /// </summary>
        public DDPayOrder(DDPayBaseSetting PayBaseSetting)
        {
            JsonStr = string.Empty;
            defaultData = new Dictionary<string, object>();
            md5Data = new Dictionary<string, object>();
            postData = new Dictionary<string, object>();
            
            md5Sort = new List<string> { "client_ip", "merchant_id", "nonce_str", "notify_url", "order_no", "return_url", "service", "total_fee", "key" };
            postSort = new List<string> { "service", "merchant_id", "sign", "nonce_str", "notify_url", "order_no", "total_fee", "client_ip" };
            
            SetDefaultPayData(PayBaseSetting);
            SetPayTypeData();
        }

        /// <summary>
        /// 設定基本DDPayOrderModle
        /// </summary>
        /// <param name="PayBaseSetting">商戶號等等基本設定</param>
        private void SetDefaultPayData(DDPayBaseSetting PayBaseSetting)
        {
            payData = new DDPayOrderModel();
            payData.merchant_id = PayBaseSetting.MerchantNo;
            payData.key = PayBaseSetting.Key;
            payData.notify_url = DDPay_NotifyUrl;
            payData.nonce_str = string.Empty;
            payData.order_no = string.Empty;
            payData.service = string.Empty;
            payData.sign = string.Empty;
            payData.total_fee = 0;
            payData.client_ip = "0.0.0.0";

            defaultData.Add("merchant_id", payData.merchant_id);
            defaultData.Add("key", payData.key);
            defaultData.Add("notify_url", payData.notify_url);
        }

        /// <summary>
        /// 設定支付類型(service)資料格式
        /// </summary>
        private void SetPayTypeData()
        {
            payTypeData = new Dictionary<string, string>();
            payTypeData.Add("EXPRESSPAY_H5", "Hz_Quick_Debit");
            payTypeData.Add("ZHIFUBAO_QR", "Hz_Alipay_QR");
            payTypeData.Add("WEIXIN_H5", "Hz_Weixin_Jsapi");
            payTypeData.Add("QUICKPASS_QR", "R_Union_QR");
        }

        /// <summary>
        /// 設定支付類型(service)
        /// </summary>
        /// <param name="payType">支付類型</param>
        public void SetPayType(string payType)
        {
            if (payTypeData.ContainsKey(payType))
                payData.service = payTypeData[payType];
            else
                payData.service = "R_Union_QR";

            defaultData.Add("service", payData.service);
        }

        /// <summary>
        /// 設定oder資料，商品ID使用orderID
        /// </summary>
        /// <param name="order"></param>
        public void SetOrderData(CDPayOrder order)
        {
            payData.order_no = ThirdPartyOderIDService.GetThirdPartyOderID(order.Id, "DDPay", 20);
            payData.total_fee = order.Amount * 100;
            payData.client_ip = order.IPAddress;
            payData.nonce_str = order.Id.ToString();

            defaultData.Add("order_no", payData.order_no);
            defaultData.Add("total_fee", payData.total_fee);
            defaultData.Add("client_ip", payData.client_ip);
            defaultData.Add("nonce_str", payData.nonce_str);
        }

        /// <summary>
        /// 完成
        /// </summary>
        public void Done()
        {
            //sort md5
            md5Data = StringObjectDirctionaryService.SortStrDictionary(defaultData, md5Sort);
            //md5 sign
            payData.sign = StringObjectDirctionaryService.GetMd5(md5Data,"&").ToUpper();
            defaultData.Add("sign", payData.sign);
            //sort post
            postData = StringObjectDirctionaryService.SortStrDictionary(defaultData, postSort);
            //to json(紀錄LOG使用)
            JsonStr = StringObjectDirctionaryService.ToJsonStr(postData);
        }

        /// <summary>
        /// 取得交易訂單編號
        /// </summary>
        public string GetTransNo()
        {
            return payData.order_no;
        }

        public string GetMd5String()
        {
            return StringObjectDirctionaryService.ConvertToUrlStringFormat(md5Data, "&");
        }

        /// <summary>
        /// 取得post Json to String(紀錄LOG使用)
        /// </summary>
        public string GetPostJsonString()
        {
            return JsonStr;
        }
    }

    /// <summary>
    /// DDPay訂單回傳
    /// </summary>
    public class DDPayOrderReturn
    {
        /// <summary>
        /// DDPay訂單回傳參數格式
        /// </summary>
        private DDPayOrderReturnModel payOrderReturnModel;
        /// <summary>
        /// DDPay訂單回傳參數格式
        /// </summary>
        private Dictionary<string,string> payOrderReturnMessegeData;

        /// <summary>
        /// 建構式
        /// </summary>
        public DDPayOrderReturn(string jsonString)
        {
            payOrderReturnModel = Newtonsoft.Json.JsonConvert.DeserializeObject<DDPayOrderReturnModel>(jsonString);
            payOrderReturnMessegeData = new Dictionary<string, string>();
            payOrderReturnMessegeData.Add("success", "成功");
            payOrderReturnMessegeData.Add("InvalidSign", "簽名錯誤");
            payOrderReturnMessegeData.Add("ParametersError", "參數錯誤");
            payOrderReturnMessegeData.Add("MerchantNotExist", "商戶不存在");
            payOrderReturnMessegeData.Add("WrongIP", "IP錯誤");
            payOrderReturnMessegeData.Add("AmountError", "金額錯誤 , 金額<:1元");
            payOrderReturnMessegeData.Add("PayMethodWrong", "支付方式錯誤");
            payOrderReturnMessegeData.Add("OrderAlreadyExist", "訂單號重複");
            payOrderReturnMessegeData.Add("AccountClose", "商戶未啟用");
            payOrderReturnMessegeData.Add("SystemIsBusyNow", "系統繁忙");
            payOrderReturnMessegeData.Add("BankIdError:", "銀行代碼錯誤");
            payOrderReturnMessegeData.Add("ClientIpError", "會員IP錯誤(付款人实际ip,并且必须是中国境内)");
            payOrderReturnMessegeData.Add("RParametersError", "R通道錯誤");
            payOrderReturnMessegeData.Add("RError", "無R通道");
            payOrderReturnMessegeData.Add("HzParametersError:", "Hz通道錯誤");
            payOrderReturnMessegeData.Add("HzError", "無Hz通道");
        }

        /// <summary>
        /// 是否成功得到回傳參數
        /// </summary>
        public bool IsSuccess()
        {
            return payOrderReturnModel.result == "success";
        }
        /// <summary>
        /// 取得回傳訊息
        /// </summary>
        public string GetReturnMessage()
        {
            if (payOrderReturnMessegeData.ContainsKey(payOrderReturnModel.message))
                return payOrderReturnModel.message + ":" + payOrderReturnMessegeData[payOrderReturnModel.message];

            return payOrderReturnModel.message;
        }
        /// <summary>
        /// 取得回傳網址
        /// </summary>
        public string GetUrl()
        {
            return payOrderReturnModel.url;
        }
    }

    /// <summary>
    /// DDPay代收交易非同步通知
    /// </summary>
    public class DDPayResponse
    {
        /// <summary>
        /// callback回傳值JsonModel
        /// </summary>
        private DDPayResponseModel payResponseModel;
        /// <summary>
        /// md5 sign string
        /// </summary>
        private string _payRetrunSignStringToMd5 = string.Empty;
        /// <summary>
        /// 建構式
        /// </summary>
        /// <param name="responseString">call back return string</param>
        public DDPayResponse(string responseString)
        {
            payResponseModel = Newtonsoft.Json.JsonConvert.DeserializeObject<DDPayResponseModel>(responseString);
        }
        /// <summary>
        /// 取得訂單ID(資料庫OrderListID)
        /// </summary>
        public int GetOrderID()
        {
            return ThirdPartyOderIDService.GetOrderID(payResponseModel.order_no, "DDPay");
        }
        /// <summary>
        /// 取得交易訂單ID(給第三方的訂單ID)
        /// </summary>
        public string GetThirdPartyOderID()
        {
            return payResponseModel.order_no;
        }
        /// <summary>
        /// 回調是否成功
        /// </summary>
        public bool IsSuccess()
        {
            return payResponseModel.is_paid;
        }
        /// <summary>
        /// 取得金額
        /// </summary>
        public long GetPayAmount()
        {
            return payResponseModel.total_fee / 100;
        }
        /// <summary>
        /// 回調簽名是否正確
        /// </summary>
        /// <param name="payBaseSetting">商戶號等基本設定</param>
        public bool IsSignTrue(DDPayBaseSetting payBaseSetting)
        {
            Dictionary<string, object> signChkData = new Dictionary<string, object>();
            signChkData.Add("is_paid", payResponseModel.is_paid);
            signChkData.Add("merchant_id", payBaseSetting.MerchantNo);
            signChkData.Add("nonce_str", payResponseModel.nonce_str);
            signChkData.Add("notify_time", payResponseModel.notify_time);
            signChkData.Add("order_no", payResponseModel.order_no);
            signChkData.Add("out_trade_no", payResponseModel.out_trade_no);
            signChkData.Add("service", payResponseModel.service);
            signChkData.Add("total_fee", payResponseModel.total_fee);
            signChkData.Add("key", payBaseSetting.Key);
            _payRetrunSignStringToMd5 = StringObjectDirctionaryService.ConvertToUrlStringFormat(signChkData, "&");
            string signDChk = MD5Service.Hash(_payRetrunSignStringToMd5, Encoding.UTF8).ToLower();
            return payResponseModel.sign == signDChk;
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