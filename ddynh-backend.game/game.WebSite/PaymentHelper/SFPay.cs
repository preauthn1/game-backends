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
    public class SFPay
    {
        /// <summary>
        /// 創建訂單
        /// </summary>
        /// <param name="order">資料庫訂單模型</param>
        /// <param name="logGuid">LOG File ID</param>
        /// <returns>回傳網址或訂單View</returns>
        public static string Post(CDPayOrder order, string logGuid)
        {
            var postAction = new ThirdPartyPaymentPostAction(logGuid, SFPay_logFile);
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
            var PayBaseSetting = new SFPayBaseSetting(merchant.InfoData, merchant.Url);
            var PayOrder = new SFPayOrder(PayBaseSetting);
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
            string payOrderUrl = PayBaseSetting.GetBaseUrl(SFPay_CreateOrderApiUrl);
            string payOrderJson = PayOrder.GetPostJsonString();
            postAction.LogPayOrderPostDataString(payOrderJson, payOrderUrl);

            var payOrderPost = PayOrder.GetPostData();
            string returnHtml = postAction.GetFormAutoSubmitHtml(payOrderPost, payOrderUrl);

            //LOG returnHtml
            postAction.SetReturnHtml(returnHtml);
            return postAction.GetReturnHtml();
        }

        /// <summary>
        /// 異步回調
        /// </summary>
        /// <param name="logGuid">LOG File ID</param>
        /// <param name="model">異步回調資料模型</param>
        /// <returns>是否回調正確並支付成功</returns>
        public static PayState Call(string logGuid)
        {
            int orderID = 0;
            int payTypeID = 0;
            string payTypeName = "善付支付";
            try
            {
                //是否跳出function return
                bool isCallReturn;
                var payCallAction = new ThirdPartyPaymentCallAction(logGuid, SFPay_logFile);
                payCallAction.LogResponseString();
                string responseString = payCallAction.GetResponseString();
                var payResponse = new SFPayResponse(responseString);

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

                var payBaseSetting = new SFPayBaseSetting(merchant.InfoData, merchant.Url);
                bool isSignTrue = payResponse.IsSignTrue(payBaseSetting);
                string returnstringToMd5 = payResponse.GetReturnstringToMd5();
                isCallReturn = payCallAction.CheckResponseSign(isSignTrue, returnstringToMd5);
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
                SiteHelper.Log(logGuid, LogMessage, SFPay_logFile);
                return PayState.Error;
            }
        }
    }

    /// <summary>
    /// 支付方式對照
    /// </summary>
    public class SFPayTypeBankCode
    {
        /// <summary>
        /// 支付方式
        /// </summary>
        private Dictionary<string, string> payTypeDic;
        /// <summary>
        /// 建構式
        /// </summary>
        public SFPayTypeBankCode()
        {
            payTypeDic = new Dictionary<string, string>();
            payTypeDic.Add("ZHIFUBAO_QR", "ALIPAY");
            payTypeDic.Add("ZHIFUBAO_H5", "ALIPAY_H5_PDD");
            payTypeDic.Add("WEIXIN_QR", "WECHAT");
            payTypeDic.Add("WEIXIN_H5", "WECHAT_H5_PDD");
            payTypeDic.Add("QQ_QR", "QQ");
            payTypeDic.Add("QQ_H5", "QQ_H5");
            payTypeDic.Add("JDPAY_QR", "JD");
            payTypeDic.Add("JDPAY_H5", "JD_H5");
            payTypeDic.Add("EXPRESSPAY_H5", "QUICK");
            //银联|云闪付扫码
            payTypeDic.Add("UNIONPAY_QR", "UNION");
            payTypeDic.Add("QUICKPASS_QR", "UNION");
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
    public class SFPayBaseSetting : PaymentMerchantInfoBaseSetting
    {
        /// <summary>
        /// 建構式
        /// </summary>
        /// <param name="infoData">MerchantInfo</param>
        /// <param name="url">接口網址</param>
        public SFPayBaseSetting(string infoData, string url)
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
    public class SFPayOrder
    {
        /// <summary>
        /// 支付所有需要欄位
        /// </summary>
        private SFPayOrderModel PayOrderModle;
        /// <summary>
        /// md5排序
        /// </summary>
        private List<string> Md5Sort;
        /// <summary>
        /// post排序
        /// </summary>
        private List<string> PostSort;
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
        public SFPayOrder(SFPayBaseSetting payBaseSetting)
        {
            JsonStr = string.Empty;
            PostData = new Dictionary<string, object>();

            PostSort = new List<string> { "partner_id", "pay_type", "pay_code", "out_trade_no", "notify_url", "return_url", "money", "sign", "sign_type" };
            Md5Sort = new List<string>(PostSort);
            Md5Sort.Remove("sign");
            Md5Sort.Remove("sign_type");

            SetDefaultPayData(payBaseSetting);
        }

        /// <summary>
        /// 設定支付類型(service)
        /// </summary>
        /// <param name="payType">支付類型</param>
        public void SetPayType(string payType)
        {
            var payTypeObj = new SFPayTypeBankCode();
            PayOrderModle.pay_code = payTypeObj.GetPayType(payType);
            PayOrderModle.pay_type = (int)ExtensionOfEnum.GetEnumByTextValue<SFPayTypeEnum>(payType);
        }

        /// <summary>
        /// 設定oder資料，商品ID使用orderID
        /// </summary>
        /// <param name="order"></param>
        public void SetOrderData(CDPayOrder order)
        {
            PayOrderModle.money = order.Amount.ToString("F2");
            PayOrderModle.out_trade_no = ThirdPartyOderIDService.GetThirdPartyOderID(order.Id, "SFPay", length: 20);
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
            return PayOrderModle.out_trade_no;
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

        public Dictionary<string, object> GetPostData()
        {
            return PostData;
        }

        /// <summary>
        /// 設定基本SFPayOrderModle
        /// </summary>
        /// <param name="payBaseSetting">商戶號等等基本設定</param>
        private void SetDefaultPayData(SFPayBaseSetting payBaseSetting)
        {
            PayOrderModle = new SFPayOrderModel();
            PayOrderModle.partner_id = payBaseSetting.MerchantNo;
            PayOrderModle.key = payBaseSetting.Key;
            PayOrderModle.notify_url = SFPay_NotifyUrl;
            PayOrderModle.return_url = FinishUrl;
            PayOrderModle.sign_type = "md5";
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
            Md5String = StringObjectDirctionaryService.ConvertToUrlStringFormat(Md5Data,"&") + PayOrderModle.key;
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
            //to json(紀錄LOG使用)
            JsonStr = StringObjectDirctionaryService.ToJsonStr(PostData);
        }
    }

    /// <summary>
    /// 支付第三方異步通知
    /// </summary>
    public class SFPayResponse
    {
        /// <summary>
        /// 四方支付訂單回傳參數格式
        /// </summary>
        private SFPayResponseModel PayResponseModel;
        /// <summary>
        /// MD5 SIGN前字串
        /// </summary>
        private string _payRetrunSignStringToMd5 = string.Empty;

        /// <summary>
        /// 建構式
        /// </summary>
        public SFPayResponse(string responseString)
        {
            PayResponseModel = JsonConvert.DeserializeObject<SFPayResponseModel>(responseString);
        }

        /// <summary>
        /// 取得訂單ID(資料庫OrderListID)
        /// </summary>
        public int GetOrderID()
        {
            return ThirdPartyOderIDService.GetOrderID(PayResponseModel.out_trade_no, "SFPay");
        }
        /// <summary>
        /// 取得交易訂單ID(給第三方的訂單ID)
        /// </summary>
        public string GetThirdPartyOderID()
        {
            return PayResponseModel.out_trade_no;
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
        public bool IsSignTrue(SFPayBaseSetting payBaseSetting)
        {
            var sortList = new List<string> { "partner_id", "pay_type", "pay_code", "trade_no", "out_trade_no", "money", "money_true"};
            sortList.Sort();
            Dictionary<string, object> signChkData = new Dictionary<string, object>();
            SetDictionary(sortList, signChkData);

            _payRetrunSignStringToMd5 = StringObjectDirctionaryService.ConvertToUrlStringFormat(signChkData, "&") + payBaseSetting.Key;

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