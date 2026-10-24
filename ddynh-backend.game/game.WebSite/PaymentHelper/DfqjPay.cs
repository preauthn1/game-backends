using game.Entity.DTO.ThirdPartyPay;
using game.Entity.Payment;
using game.Infrastructure;
using game.Infrastructure.Utility.ThirdParty;
using game.WebSite.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using static game.WebSite.PayConfig;

namespace game.WebSite.PaymentHelper
{
    /// <summary>
    /// DFQJ支付
    /// </summary>
    public class DfqjPay
    {
        /// <summary>
        /// 創建訂單
        /// </summary>
        /// <param name="order">資料庫訂單模型</param>
        /// <param name="logGuid">LOG File ID</param>
        /// <returns>回傳網址或訂單View</returns>
        public static string Post(CDPayOrder order, string logGuid)
        {
            var postAction = new ThirdPartyPaymentPostAction(logGuid, DfqjPay_logFile);
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
            var PayBaseSetting = new DfqjPayBaseSetting(merchant.InfoData, merchant.Url);
            var PayOrder = new DfqjPayOrder(PayBaseSetting);
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
            string payOrderUrl = PayBaseSetting.GetBaseUrl(DfqjPay_CreateOrderApiUrl);
            string payOrderInput = PayOrder.GetPostInputString();
            string payOrderReturnString = SiteHelper.HttpPost(payOrderUrl, payOrderInput);
            postAction.LogPayOrderPostDataString(payOrderInput, payOrderUrl);
            postAction.LogPayReturnString(payOrderReturnString);

            //處理回傳json
            var payReturn = new DfqjPayReturn(payOrderReturnString);

            //確認回傳值是否正確
            bool isPayReturnSuccess = payReturn.IsSuccess();
            postAction.LogPayReturnMessage(isPayReturnSuccess, payReturn.GetMessage());
            if (!isPayReturnSuccess)
            {
                return postAction.GetReturnHtml();
            }

            //LOG 回傳網址
            postAction.SetReturnHtml(payReturn.GetReturnUrl());
            return payReturn.GetReturnUrl();
        }

        /// <summary>
        /// 異步回調
        /// </summary>
        /// <param name="logGuid">LOG File ID</param>
        /// <param name="model">異步回調資料模型</param>
        /// <returns>是否回調正確並支付成功</returns>
        public static PayState Call(string logGuid, DfqjPayResponseModel model)
        {
            int orderID = 0;
            try
            {
                //是否跳出function return
                bool isCallReturn;
                var payCallAction = new ThirdPartyPaymentCallAction(logGuid, DfqjPay_logFile);
                //非json使用此方法紀錄回傳值
                string responseString = JsonConvert.SerializeObject(model).ToString();
                payCallAction.LogResponseString(responseString);

                var payResponse = new DfqjPayResponse(model);

                //SYFPAY0000...
                string payOrderID = payResponse.GetThirdPartyOderID();
                //取得實際orderid
                orderID = payResponse.GetOrderID();

                //設定訂單資訊(紀錄LOG內容也需要)
                payCallAction.SetOrderData(orderID, payOrderID);

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

                var payBaseSetting = new DfqjPayBaseSetting(merchant.InfoData, merchant.Url);
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
                SiteHelper.Log(logGuid, LogMessage, DfqjPay_logFile);
                return PayState.Error;
            }
        }
    }

    public class DfqjPayType
    {
        /// <summary>
        /// 支付方式
        /// </summary>
        private Dictionary<string, string> payTypeDic;
        /// <summary>
        /// 建構式
        /// </summary>
        public DfqjPayType()
        {
            payTypeDic = new Dictionary<string, string>();

            // 东方微信扫码
            // 单笔1000-10000
            payTypeDic.Add("WEIXIN_QR", "pay_wechat");

            //东方支付宝小额
            payTypeDic.Add("ZHIFUBAO_H5", "pay_alipayphone");

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

            return "w_alipay";
        }
    }

    /// <summary>
    /// 微第三方支付基本支付通道設定(商户号、密钥...)
    /// </summary>
    public class DfqjPayBaseSetting : PaymentMerchantInfoBaseSetting
    {
        /// <summary>
        /// 建構式
        /// </summary>
        /// <param name="infoData">MerchantInfo</param>
        /// <param name="url">接口網址</param>
        public DfqjPayBaseSetting(string infoData, string url)
        {
            var InfoDataJson = JsonConvert.DeserializeObject<ThirdPartyPayMerchantInfo>(infoData);
            _merchantNo = InfoDataJson.MERCHANT_NO;
            _key = InfoDataJson.MERCHANT_KEY;
            _baseUrl = url + "/";
        }
    }

    /// <summary>
    /// 創建訂單
    /// </summary>
    public class DfqjPayOrder
    {
        /// <summary>
        /// 支付所有需要欄位
        /// </summary>
        private DfqjPayOrderModel PayOrderModle;
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
        public DfqjPayOrder(DfqjPayBaseSetting payBaseSetting)
        {
            JsonStr = string.Empty;
            Md5Data = new Dictionary<string, object>();
            PostData = new Dictionary<string, object>();

            PostSort = new List<string> { "fxid", "fxddh", "fxdesc", "fxfee", "fxnotifyurl", "fxbackurl", "fxpay", "fxnotifystyle", "fxip", "fxsign", "sign", };
            Md5Sort = new List<string>(PostSort);
            Md5Sort.Remove("sign");
            Md5Sort.Add("fxsign");

            SetDefaultPayData(payBaseSetting);
        }

        /// <summary>
        /// 設定基本DfqjPayOrderModle
        /// </summary>
        /// <param name="payBaseSetting">商戶號等等基本設定</param>
        private void SetDefaultPayData(DfqjPayBaseSetting payBaseSetting)
        {
            PayOrderModle = new DfqjPayOrderModel();
            PayOrderModle.fxid = payBaseSetting.MerchantNo;
            PayOrderModle.key = payBaseSetting.Key;
            PayOrderModle.fxnotifyurl = DfqjPay_NotifyUrl;
            PayOrderModle.fxbackurl = FinishUrl;
        }

        /// <summary>
        /// 設定支付類型(service)
        /// </summary>
        /// <param name="payType">支付類型</param>
        public void SetPayType(string payType)
        {
            var payTypeObj = new DfqjPayType();
            PayOrderModle.fxpay = payTypeObj.GetPayType(payType);
        }

        /// <summary>
        /// 設定oder資料，商品ID使用orderID
        /// </summary>
        /// <param name="order"></param>
        public void SetOrderData(CDPayOrder order)
        {
            PayOrderModle.fxfee = order.Amount.ToString("F2");
            PayOrderModle.fxddh = ThirdPartyOderIDService.GetThirdPartyOderID(order.Id, "DfqjPay", length: 20);
            PayOrderModle.fxdesc = PayOrderModle.fxddh;
            PayOrderModle.fxnotifystyle = "2";
            PayOrderModle.fxip = !string.IsNullOrEmpty(order.IPAddress) && order.IPAddress != "未知IP" ? order.IPAddress : string.Empty;
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
            //md5 sign
            //Md5String = StringObjectDirctionaryService.ConvertToUrlStringFormat(Md5Data, "&");

            Md5String = PayOrderModle.fxid + PayOrderModle.fxddh + PayOrderModle.fxfee + PayOrderModle.fxnotifyurl + PayOrderModle.key;
            PayOrderModle.fxsign = MD5Service.Hash(Md5String, Encoding.UTF8).ToLower();
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
            return PayOrderModle.fxddh;
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
        public DfqjPayOrderModel GetPayOrderModel()
        {
            return PayOrderModle;
        }
    }

    /// <summary>
    /// 第三方訂單return
    /// </summary>
    public class DfqjPayReturn
    {
        /// <summary>
        /// DfqjPay訂單回傳參數格式
        /// </summary>
        private DfqjPayReturnModel PayReturnModel;

        /// <summary>
        /// 建構式
        /// </summary>
        public DfqjPayReturn(string jsonString)
        {
            PayReturnModel = JsonConvert.DeserializeObject<DfqjPayReturnModel>(jsonString);
        }

        /// <summary>
        /// 檢查回傳json是否成功
        /// </summary>
        public bool IsSuccess()
        {
            return PayReturnModel.status == "1";
        }

        /// <summary>
        /// 取得回傳訊息
        /// </summary>
        /// <returns></returns>
        public string GetMessage()
        {
            if (!IsSuccess())
                return PayReturnModel.status;

            return PayReturnModel.payurl;
        }

        /// <summary>
        /// 取得回傳網址
        /// </summary>
        public string GetReturnUrl()
        {
            if (!IsSuccess())
                return string.Empty;

            return PayReturnModel.payurl;
        }
    }

    /// <summary>
    /// 支付第三方異步通知
    /// </summary>
    public class DfqjPayResponse
    {
        /// <summary>
        /// 四方支付訂單回傳參數格式
        /// </summary>
        private DfqjPayResponseModel PayResponseModel;
        /// <summary>
        /// MD5 SIGN前字串
        /// </summary>
        private string _payRetrunSignStringToMd5 = string.Empty;

        /// <summary>
        /// 建構式
        /// </summary>
        public DfqjPayResponse(DfqjPayResponseModel model)
        {
            PayResponseModel = model;
        }
        /// <summary>
        /// 取得訂單ID(資料庫OrderListID)
        /// </summary>
        public int GetOrderID()
        {
            return ThirdPartyOderIDService.GetOrderID(PayResponseModel.fxddh, "DfqjPay");
        }
        /// <summary>
        /// 取得交易訂單ID(給第三方的訂單ID)
        /// </summary>
        public string GetThirdPartyOderID()
        {
            return PayResponseModel.fxddh;
        }
        /// <summary>
        /// 取得金額
        /// </summary>
        public int GetPayAmount()
        {
            return PayResponseModel.amountMoney;
        }
        /// <summary>
        /// 回調簽名是否正確
        /// </summary>
        /// <param name="payBaseSetting">商戶號等基本設定</param>
        public bool IsSignTrue(DfqjPayBaseSetting payBaseSetting)
        {
            //Dictionary<string, object> signChkData = new Dictionary<string, object>();
            //signChkData.Add("amount", PayResponseModel.amount);
            //signChkData.Add("amount_ex", PayResponseModel.amount_ex);
            ////確認自定義欄位是否為空
            //if (!string.IsNullOrEmpty(PayResponseModel.attach))
            //{
            //    signChkData.Add("attach", PayResponseModel.attach);
            //}

            //signChkData.Add("client_id", PayResponseModel.client_id);
            //signChkData.Add("key", payBaseSetting.Key);
            //signChkData.Add("merchant_order", PayResponseModel.merchant_order);

            var Md5String = PayResponseModel.fxstatus + PayResponseModel.fxid + PayResponseModel.fxddh + PayResponseModel.fxfee + payBaseSetting.Key;
            //_payRetrunSignStringToMd5 = StringObjectDirctionaryService.ConvertToUrlStringFormat(signChkData, "&");
            string signChk = MD5Service.Hash(Md5String, Encoding.UTF8).ToLower();
            return PayResponseModel.fxsign == signChk;
        }
        /// <summary>
        /// 取得驗證簽名MD5使用字串
        /// </summary>
        public string GetReturnstringToMd5()
        {
            return _payRetrunSignStringToMd5;
        }
    }

    /// <summary>
    /// 創建訂單資料格式
    /// </summary>
    public class DfqjPayOrderModel
    {

        public string fxid { set; get; }

        public string fxddh { set; get; }

        public string fxdesc { set; get; }

        public string fxfee { set; get; }

        public string fxnotifyurl { set; get; }

        public string fxbackurl { set; get; }

        public string fxpay { set; get; }

        public string fxnotifystyle { set; get; }

        public string fxsign { set; get; }

        public string fxip { set; get; }

        public string key { set; get; }
    }

    /// <summary>
    /// 創建訂單返回参数data內容
    /// </summary>
    public class DfqjPayReturnDataModel
    {
        /// <summary>
        /// 我方建立的訂單編號
        /// </summary>
        public string order_id { set; get; }
        /// <summary>
        /// 充值付款網址
        /// </summary>
        public string read_url { set; get; }
        /// <summary>
        /// 充值訊息
        /// </summary>
        public string message { set; get; }
    }

    /// <summary>
    /// 創建訂單返回参数
    /// </summary>
    public class DfqjPayReturnModel
    {
        /// <summary>
        /// 交易狀態 1:交易成功, 非 1 均為交易失敗
        /// </summary>
        public string status { set; get; }
        /// <summary>
        /// 錯誤充值訊息
        /// </summary>
        public string payurl { set; get; }
        /// <summary>
        /// 充值資料
        /// </summary>
        public string error { set; get; }
    }

    /// <summary>
    /// 異步回調模型
    /// </summary>
    public class DfqjPayResponseModel
    {
        /// <summary>
        /// 商戶帳號
        /// </summary>
        public string fxid { set; get; }
        /// <summary>
        /// 商戶單號
        /// </summary>
        public string fxddh { set; get; }
        public string fxorder { set; get; }
        public string fxdesc { set; get; }
        /// <summary>
        /// 交易金額
        /// </summary>
        public string fxfee { set; get; }
        /// <summary>
        /// 金额 轉數字型態
        /// </summary>
        public int amountMoney
        {
            get
            {
                return (int)Decimal.Parse(fxfee);
            }
        }
        /// <summary>
        /// 商戶自定義資料欄位
        /// </summary>
        public string fxattch { set; get; }
        public string fxstatus { set; get; }
        public string fxtime { set; get; }
        public string fxsign { set; get; }
        /// <summary>
        /// 簽名:sign 將 key 加入參數組、依字母升冪排序、清除空白參數，再用 MD 5 加密後轉成小寫
        /// </summary>
        //public string fxsign { set; get; }
        //public string key { set; get; }
    }
}