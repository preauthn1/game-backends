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
using System.Web;
using static game.WebSite.PayConfig;

namespace game.WebSite.PaymentHelper
{
    public class GoPay
    {
        /// <summary>
        /// 創建訂單
        /// </summary>
        /// <param name="order">資料庫訂單模型</param>
        /// <param name="logGuid">LOG File ID</param>
        /// <returns>回傳網址或訂單View</returns>
        public static string Post(CDPayOrder order, string logGuid)
        {
            var postAction = new ThirdPartyPaymentPostAction(logGuid, GoPay_logFile);
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
            var PayBaseSetting = new GoPayBaseSetting(merchant.InfoData, merchant.Url);
            var PayOrder = new GoPayOrder(PayBaseSetting);
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
            string payOrderUrl = PayBaseSetting.GetBaseUrl(GoPay_CreateOrderApiUrl);
            string payOrderInput = PayOrder.GetPostInputString();
            string payOrderReturnString = SiteHelper.HttpGet(payOrderUrl+ "&" +payOrderInput);
            postAction.LogPayOrderPostDataString(payOrderInput, payOrderUrl);
            postAction.LogPayReturnString(payOrderReturnString);

            //處理回傳json
            var payReturn = new GoPayReturn(payOrderReturnString);

            //確認回傳值是否正確
            bool isPayReturnSuccess = payReturn.IsSuccess();
            postAction.LogPayReturnMessage(isPayReturnSuccess, payReturn.GetMessage());
            if (!isPayReturnSuccess)
            {
                return postAction.GetReturnHtml();
            }

            //LOG 回傳網址
            string returnHtml = postAction.GenerateRedirectHtmlOpenBrowser(payReturn.GetReturnUrl());
            postAction.SetReturnHtml(returnHtml);
            return postAction.GetReturnHtml();
        }

        /// <summary>
        /// 異步回調
        /// </summary>
        /// <param name="logGuid">LOG File ID</param>
        /// <param name="model">異步回調資料模型</param>
        /// <returns>是否回調正確並支付成功</returns>
        public static PayState Call(string logGuid, GoPayResponseModel model)
        {
            int orderID = 0;
            int payTypeID = 0;
            string payTypeName = "佰坊支付";
            try
            {
                //是否跳出function return
                bool isCallReturn;
                var payCallAction = new ThirdPartyPaymentCallAction(logGuid, GoPay_logFile);
                string responseString = JsonConvert.SerializeObject(model).ToString();
                payCallAction.LogResponseString(responseString);

                var payResponse = new GoPayResponse(model);

                //SYFPAY0000...
                string payOrderID = payResponse.GetThirdPartyOderID();
                //取得實際orderid
                orderID = payResponse.GetOrderID();

                //設定訂單資訊(紀錄LOG內容也需要)
                payCallAction.SetOrderData(orderID, payOrderID);

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

                var payBaseSetting = new GoPayBaseSetting(merchant.InfoData, merchant.Url);
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
                SiteHelper.Log(logGuid, LogMessage, GoPay_logFile);
                return PayState.Error;
            }
        }
    }

    /// <summary>
    /// 第三方支付基本支付通道設定(商户号、密钥...)
    /// </summary>
    public class GoPayBaseSetting : PaymentMerchantInfoBaseSetting
    {
        /// <summary>
        /// 建構式
        /// </summary>
        /// <param name="infoData">MerchantInfo</param>
        /// <param name="url">接口網址</param>
        public GoPayBaseSetting(string infoData, string url)
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
    public class GoPayOrder
    {
        /// <summary>
        /// 支付所有需要欄位
        /// </summary>
        private GoPayOrderModel PayOrderModle;
        /// <summary>
        /// 簽名排序
        /// </summary>
        private List<string> SignSortList;
        /// <summary>
        /// 紀錄簽名與表單資訊
        /// </summary>
        private Dictionary<string, object> Data;
        /// <summary>
        /// 此字串md5之後將存成sign
        /// </summary>
        private string Md5String;
        /// <summary>
        /// post string
        /// </summary>
        private string InputString;

        /// <summary>
        /// 建構式
        /// </summary>
        public GoPayOrder(GoPayBaseSetting payBaseSetting)
        {
            Md5String = string.Empty;
            InputString = string.Empty;
            Data = new Dictionary<string, object>();
            SignSortList = new List<string> { "mch_id", "ptype", "order_sn", "money", "goods_desc",  "username", "client_ip", "format", "notify_url", "time" };
            SetDefaultPayData(payBaseSetting);
        }

        /// <summary>
        /// 設定支付類型(service)
        /// </summary>
        /// <param name="payType">支付類型</param>
        public void SetPayType(string payType)
        {
            PayOrderModle.ptype = (byte)ExtensionOfEnum.GetEnumByTextValue<GoPayTypeEnum>(payType);
        }

        /// <summary>
        /// 設定oder資料，商品ID使用orderID
        /// </summary>
        /// <param name="order"></param>
        public void SetOrderData(CDPayOrder order)
        {
            PayOrderModle.moneyInt = order.Amount;
            PayOrderModle.order_sn = ThirdPartyOderIDService.GetThirdPartyOderID(order.Id, "GoPay", length: 20);
            PayOrderModle.username = order.UserID.ToString();

            string ipAddress = order.IPAddress;
            if (order.IPAddress == "未知IP")
                ipAddress = string.Empty;

            PayOrderModle.client_ip = ipAddress;
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
            return PayOrderModle.order_sn;
        }

        public string GetMd5String()
        {
            return Md5String;
        }

        /// <summary>
        /// 取得post Json to String(紀錄LOG使用)
        /// </summary>
        public string GetPostInputString()
        {
            return InputString;
        }

        /// <summary>
        /// 設定基本GoPayOrderModle
        /// </summary>
        /// <param name="payBaseSetting">商戶號等等基本設定</param>
        private void SetDefaultPayData(GoPayBaseSetting payBaseSetting)
        {
            PayOrderModle = new GoPayOrderModel();
            PayOrderModle.mch_id = payBaseSetting.MerchantNo;
            PayOrderModle.key = payBaseSetting.Key;
            PayOrderModle.notify_url = GoPay_NotifyUrl;
            PayOrderModle.goods_desc = string.Empty;
            PayOrderModle.format = "page";
            PayOrderModle.time = DateTime.UtcNow.CovertToUnixTimestamp();
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
                        dictionary.Add(key, value);
                    }
                }
            }
        }

        /// <summary>
        /// 設定簽名
        /// </summary>
        private void SetSign()
        {
            SignSortList.Sort();
            //排序MD5字串
            SetDictionary(SignSortList, Data);
            Data.Add("key", PayOrderModle.key);
            //UrlEncode
            Md5String = StringObjectDirctionaryService.ConvertToUrlStringFormat(Data, "&");
            //md5 sign
            PayOrderModle.sign = MD5Service.Hash(Md5String).ToLower();
        }

        /// <summary>
        /// 設定post資料(JSON 或者INPUT FORM)
        /// </summary>
        private void SetPost()
        {
            Data.Remove("key");
            Data.Add("sign", PayOrderModle.sign);
            //to UrlFormat(紀錄LOG使用)
            InputString = StringObjectDirctionaryService.ConvertToUrlStringFormat(Data, "&", true);
        }
    }

    /// <summary>
    /// 第三方訂單return
    /// </summary>
    public class GoPayReturn
    {
        /// <summary>
        /// GoPay訂單回傳參數格式
        /// </summary>
        private GoPayReturnModel PayReturnModel;

        /// <summary>
        /// 建構式
        /// </summary>
        public GoPayReturn(string payReturnJsonString)
        {
            PayReturnModel = JsonConvert.DeserializeObject<GoPayReturnModel>(payReturnJsonString);
        }

        /// <summary>
        /// 檢查回傳json是否成功
        /// </summary>
        public bool IsSuccess()
        {
            return PayReturnModel.code == "1";
        }

        public string GetThirdPartyOderID()
        {
            if (!IsSuccess())
                return string.Empty;

            return PayReturnModel.data.order_sn;
        }

        /// <summary>
        /// 取得回傳訊息
        /// </summary>
        /// <returns></returns>
        public string GetMessage()
        {
            return PayReturnModel.msg;
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
    public class GoPayResponse
    {
        /// <summary>
        /// 四方支付訂單回傳參數格式
        /// </summary>
        private GoPayResponseModel PayResponseModel;
        /// <summary>
        /// MD5 SIGN前字串
        /// </summary>
        private string _payRetrunSignStringToMd5 = string.Empty;

        /// <summary>
        /// 建構式
        /// </summary>
        public GoPayResponse(GoPayResponseModel model)
        {
            PayResponseModel = model;
        }

        /// <summary>
        /// 取得訂單ID(資料庫OrderListID)
        /// </summary>
        public int GetOrderID()
        {
            return ThirdPartyOderIDService.GetOrderID(PayResponseModel.sh_order, "GoPay");
        }
        /// <summary>
        /// 取得交易訂單ID(給第三方的訂單ID)
        /// </summary>
        public string GetThirdPartyOderID()
        {
            return PayResponseModel.sh_order;
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
        public bool IsSignTrue(GoPayBaseSetting payBaseSetting)
        {
            var sortList = new List<string> { "sh_order", "pt_order", "money", "time" };
            sortList.Sort();
            Dictionary<string, object> signChkData = new Dictionary<string, object>();
            SetDictionary(sortList, signChkData);
            signChkData.Add("key", payBaseSetting.Key);

            _payRetrunSignStringToMd5 = StringObjectDirctionaryService.ConvertToUrlStringFormat(signChkData, "&");

            string signChk = MD5Service.Hash(_payRetrunSignStringToMd5).ToLower();
            return PayResponseModel.sign == signChk;
        }
        /// <summary>
        /// 取得驗證簽名MD5使用字串
        /// </summary>
        public string GetSignBeforeString()
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