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
using System.Web;
using static game.WebSite.PayConfig;

namespace game.WebSite.PaymentHelper
{
    public class SpeedPay
    {
        /// <summary>
        /// 創建訂單
        /// </summary>
        /// <param name="order">資料庫訂單模型</param>
        /// <param name="logGuid">LOG File ID</param>
        /// <returns>回傳網址或訂單View</returns>
        public static string Post(CDPayOrder order, string logGuid)
        {
            var postAction = new ThirdPartyPaymentPostAction(logGuid, SpeedPay_logFile);
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
            var PayBaseSetting = new SpeedPayBaseSetting(merchant.InfoData, merchant.Url);
            var PayOrder = new SpeedPayOrder(PayBaseSetting);
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
            string payOrderUrl = PayBaseSetting.GetBaseUrl(SpeedPay_CreateOrderApiUrl);
            string payOrderInput = PayOrder.GetPostInputString();
            string payOrderReturnString = SiteHelper.HttpPost(payOrderUrl, payOrderInput);
            postAction.LogPayOrderPostDataString(payOrderInput, payOrderUrl);
            postAction.LogPayReturnString(payOrderReturnString);

            //處理回傳json
            var payReturn = new SpeedPayReturn(payOrderReturnString);

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
        public static PayState Call(string logGuid, SpeedPayResponseModel model)
        {
            int orderID = 0;
            int payTypeID = 0;
            string payTypeName = "光速付支付";
            try
            {
                //是否跳出function return
                bool isCallReturn;
                var payCallAction = new ThirdPartyPaymentCallAction(logGuid, SpeedPay_logFile);
                string responseString = JsonConvert.SerializeObject(model).ToString();
                payCallAction.LogResponseString(responseString);

                var payResponse = new SpeedPayResponse(model);

                //SYFPAY0000...
                string payOrderID = payResponse.GetThirdPartyOderID();
                //取得實際orderid
                orderID = payResponse.GetOrderID();

                //設定訂單資訊(紀錄LOG內容也需要)
                payCallAction.SetOrderData(orderID, payOrderID);

                //確認第三方訂單狀態
                string status = "第三方交易失敗";
                switch (model.status)
                {
                    case (int)SpeedPayResponseStatus.CreateOrder:
                        status = "第三方建立訂單成功";
                        payCallAction.LogThirdPartyPayOrderStatus(status);
                        return PayState.Watting;

                    case (int)SpeedPayResponseStatus.Payed:
                        status = "玩家付款成功，第三方尚未收到款項";
                        payCallAction.LogThirdPartyPayOrderStatus(status);
                        return PayState.Watting;

                    case (int)SpeedPayResponseStatus.Finish:
                        //確認訂單資訊
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

                        var payBaseSetting = new SpeedPayBaseSetting(merchant.InfoData, merchant.Url);
                        bool isSignTrue = payResponse.IsSignTrue(payBaseSetting);
                        string returnstringToMd5 = payResponse.GetReturnstringToMd5();
                        isCallReturn = payCallAction.CheckResponseSign(isSignTrue, returnstringToMd5);
                        if (isCallReturn)
                        {
                            return payCallAction.PayState;
                        }

                        //進行金幣充值
                        string sourceIP = SystemHelper.UserIPAddress;
                        int responseAmount = Convert.ToInt32(payResponse.GetPayAmount());

                        payCallAction.FinishOrderAndCheckMonery(responseAmount, sourceIP);
                        return payCallAction.PayState;

                    default:
                        payCallAction.LogThirdPartyPayOrderStatus(status);
                        return PayState.Error;
                }
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
                SiteHelper.Log(logGuid, LogMessage, SpeedPay_logFile);
                return PayState.Error;
            }
        }
    }

    /// <summary>
    /// 支付方式對照
    /// </summary>
    public class SpeedPayTypeBankCode
    {
        /// <summary>
        /// 支付方式
        /// </summary>
        private Dictionary<string, string> payTypeDic;
        /// <summary>
        /// 建構式
        /// </summary>
        public SpeedPayTypeBankCode()
        {
            payTypeDic = new Dictionary<string, string>();
            payTypeDic.Add("ZHIFUBAO_QR", "alipay");
            payTypeDic.Add("WEIXIN_QR", "wxpay");
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
    public class SpeedPayBaseSetting : PaymentMerchantInfoBaseSetting
    {
        /// <summary>
        /// 建構式
        /// </summary>
        /// <param name="infoData">MerchantInfo</param>
        /// <param name="url">接口網址</param>
        public SpeedPayBaseSetting(string infoData, string url)
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
    public class SpeedPayOrder
    {
        /// <summary>
        /// 支付所有需要欄位
        /// </summary>
        private SpeedPayOrderModel PayOrderModle;
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
        public SpeedPayOrder(SpeedPayBaseSetting payBaseSetting)
        {
            JsonStr = string.Empty;
            Md5Data = new Dictionary<string, object>();
            PostData = new Dictionary<string, object>();

            PostSort = new List<string> { "mchno", "timestamp", "signature", "username", "order_no", "order_type", "vcoin_code", "currency_type", "currency_money", "pay_methods", "remark", "fortest" };
            Md5Sort = new List<string>(PostSort);
            Md5Sort.Remove("signature");

            SetDefaultPayData(payBaseSetting);
        }

        /// <summary>
        /// 設定支付類型(service)
        /// </summary>
        /// <param name="payType">支付類型</param>
        public void SetPayType(string payType)
        {
            var payTypeObj = new SpeedPayTypeBankCode();
            PayOrderModle.pay_methods = payTypeObj.GetPayType(payType);
        }

        /// <summary>
        /// 設定oder資料，商品ID使用orderID
        /// </summary>
        /// <param name="order"></param>
        public void SetOrderData(CDPayOrder order)
        {
            PayOrderModle.currency_money = order.Amount;
            PayOrderModle.order_no = ThirdPartyOderIDService.GetThirdPartyOderID(order.Id, "SpeedPay", length: 20);
            PayOrderModle.username = order.UserID.ToString();
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
            return PayOrderModle.order_no;
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
        /// 設定基本SpeedPayOrderModle
        /// </summary>
        /// <param name="payBaseSetting">商戶號等等基本設定</param>
        private void SetDefaultPayData(SpeedPayBaseSetting payBaseSetting)
        {
            PayOrderModle = new SpeedPayOrderModel();
            PayOrderModle.mchno = payBaseSetting.MerchantNo;
            PayOrderModle.key = payBaseSetting.Key;
            PayOrderModle.timestamp = TimeZoneInfo.ConvertTimeToUtc(DateTime.Now).CovertToUnixTimestamp();
            PayOrderModle.order_type = (byte)SpeedPayOrderType.Buy;
            PayOrderModle.vcoin_code = "FCNY";
            PayOrderModle.currency_type = "CNY";
            PayOrderModle.fortest = "false";
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
            Md5Sort.Sort();
            //排序MD5字串
            SetDictionary(Md5Sort, Md5Data);
            //UrlEncode
            Md5String = PayOrderModle.key + StringObjectDirctionaryService.ConvertToFieldValueFormat(Md5Data) + PayOrderModle.key;
            //md5 sign
            PayOrderModle.signature = MD5Service.Hash(Md5String, Encoding.UTF8).ToLower();
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
    public class SpeedPayReturn
    {
        /// <summary>
        /// SpeedPay訂單回傳參數格式
        /// </summary>
        private SpeedPayReturnModel PayReturnModel;

        /// <summary>
        /// 建構式
        /// </summary>
        public SpeedPayReturn(string jsonString)
        {
            PayReturnModel = JsonConvert.DeserializeObject<SpeedPayReturnModel>(jsonString);
        }

        /// <summary>
        /// 檢查回傳json是否成功
        /// </summary>
        public bool IsSuccess()
        {
            return PayReturnModel.errcode == (int)SpeedPayReturnCode.Succese;
        }

        /// <summary>
        /// 取得回傳訊息
        /// </summary>
        /// <returns></returns>
        public string GetMessage()
        {
            return PayReturnModel.errmsg;
        }

        /// <summary>
        /// 取得回傳網址
        /// </summary>
        public string GetReturnUrl()
        {
            if (!IsSuccess())
                return string.Empty;

            return PayReturnModel.data;
        }
    }

    /// <summary>
    /// 支付第三方異步通知
    /// </summary>
    public class SpeedPayResponse
    {
        /// <summary>
        /// 四方支付訂單回傳參數格式
        /// </summary>
        private SpeedPayResponseModel PayResponseModel;
        /// <summary>
        /// MD5 SIGN前字串
        /// </summary>
        private string _payRetrunSignStringToMd5 = string.Empty;

        /// <summary>
        /// 建構式
        /// </summary>
        public SpeedPayResponse(SpeedPayResponseModel model)
        {
            PayResponseModel = model;
        }

        /// <summary>
        /// 取得訂單ID(資料庫OrderListID)
        /// </summary>
        public int GetOrderID()
        {
            return ThirdPartyOderIDService.GetOrderID(PayResponseModel.order_no, "SpeedPay");
        }
        /// <summary>
        /// 取得交易訂單ID(給第三方的訂單ID)
        /// </summary>
        public string GetThirdPartyOderID()
        {
            return PayResponseModel.order_no;
        }
        /// <summary>
        /// 取得金額
        /// </summary>
        public int GetPayAmount()
        {
            return PayResponseModel.currency_moneyInt;
        }
        /// <summary>
        /// 回調簽名是否正確
        /// </summary>
        /// <param name="payBaseSetting">商戶號等基本設定</param>
        public bool IsSignTrue(SpeedPayBaseSetting payBaseSetting)
        {
            var sortList = new List<string> { "create_time", "currency_money", "currency_type", "finish_time", "mchno", "order_no", "order_type", "pay_amount", "pay_order_no", "price", "remark", "status", "timestamp", "username", "vcoin_code", "vcoin_count" };
            Dictionary<string, object> signChkData = new Dictionary<string, object>();
            SetDictionary(sortList, signChkData);

            _payRetrunSignStringToMd5 = payBaseSetting.Key + StringObjectDirctionaryService.ConvertToFieldValueFormat(signChkData) + payBaseSetting.Key;

            string signChk = MD5Service.Hash(_payRetrunSignStringToMd5, Encoding.UTF8).ToLower();
            return PayResponseModel.signature == signChk;
        }
        /// <summary>
        /// 取得驗證簽名MD5使用字串
        /// </summary>
        public string GetReturnstringToMd5()
        {
            return _payRetrunSignStringToMd5;
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
                        dictionary.Add(key, value);
                    }
                }
            }
        }
    }
}