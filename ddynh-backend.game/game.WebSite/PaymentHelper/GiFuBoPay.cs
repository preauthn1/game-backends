using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using game.Entity.Payment;
using game.Infrastructure;
using game.Infrastructure.Utility.ThirdParty;
using game.WebSite.Services;
using Newtonsoft.Json;
using static game.WebSite.PayConfig;

namespace game.WebSite.PaymentHelper
{
    /// <summary>
    /// 巨富()
    /// </summary>
    public class GiFuBoPay
    {
        /// <summary>
        /// 创建订单(跳转网页链接)
        /// </summary>
        /// <param name="order">訂單</param>
        /// <param name="guid">LogGuID</param>
        /// <returns></returns>
        public static string Post(CDPayOrder order, string guid)
        {
            string returnMsg;

            var postAction = new ThirdPartyPaymentPostAction(guid, GiFuBoPay_logFile);
            bool isSetOrderData = postAction.SetOrderData(order);
            if (!isSetOrderData)
            {
                return postAction.GetReturnHtml();
            }

            CDMerchantUUID merchant = MerchantDataService.GetMerchantData(order);
            bool isSetMerChant = postAction.SetMerChant(merchant);
            if (!isSetMerChant)
            {
                return postAction.GetReturnHtml();
            }

            //把GWID寫入訂單資料
            Service.CDNewModifyTransID(order.Id, "0", merchant.GWID);

            //支付下单地址
            var PayBaseSetting = new GiFuBoPayBaseSetting(merchant.InfoData, merchant.Url);
            var PayOrder = new GiFuBoPayOrder(PayBaseSetting);
            PayOrder.SetOrderData(order);
            string SendUrl = PayOrder.GetSendUrl(merchant.BankCode);
            PayOrder.Done();
            //紀錄交易編號
            string transNo = PayOrder.GetTransNo();
            postAction.LogThirdPartyOderID(transNo);

            string payOrderUrl = PayBaseSetting.GetBaseUrl(SendUrl);
            string payOrderJson = PayOrder.GetPostJsonString();

            postAction.LogPayOrderPostDataString(payOrderJson, payOrderUrl);
            var payOderPost = PayOrder.GetPostData();
            returnMsg = postAction.GetFormAutoSubmitHtml(payOderPost, payOrderUrl);
            postAction.SetReturnHtml(returnMsg);

            return returnMsg;
        }

        /// <summary>
        /// 代收交易非同步通知
        /// </summary>
        /// <param name="logGuid">LogGuID</param>
        public static PayState Call(string logGuid, GiFuBoPayCallbackModel payReturnData)
        {
            int orderID = 0;
            try
            {
                bool isReturn;
                var payCallAction = new ThirdPartyPaymentCallAction(logGuid, GiFuBoPay_logFile);
                //非json使用此方法紀錄回傳值
                string responseString = JsonConvert.SerializeObject(payReturnData).ToString();
                payCallAction.LogResponseString(responseString);

                var payResponse = new GiFuBoPayReturn(payReturnData);

                //SYFPAY0000...
                string payOrderID = payResponse.GetPayOderID();
                //取得實際orderid
                orderID = payResponse.GetOrderID();

                //設定訂單資訊(紀錄LOG內容也需要)
                payCallAction.SetOrderData(orderID, payOrderID);
                //確認回調
                isReturn = payCallAction.CheckResponseIsSuccess(payResponse.IsSuccess());
                if (isReturn)
                {
                    if (payCallAction.IsPayStateSuccess)
                        return PayState.Success;
                    else
                        return PayState.Error;
                }

                //確認訂單資訊
                isReturn = payCallAction.CheckOrderData();
                //若訂單確認失敗則返回
                if (isReturn)
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

                var payBaseSetting = new GiFuBoPayBaseSetting(merchant.InfoData, merchant.Url);
                bool isSignTrue = payResponse.IsSignTrue(payBaseSetting);
                string returnstringToMd5 = payResponse.GetReturnstringToMd5();
                isReturn = payCallAction.CheckResponseSign(isSignTrue, returnstringToMd5);
                if (isReturn)
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
                SiteHelper.Log(logGuid, LogMessage, GiFuBoPay_logFile);
                return PayState.Error;
            }
        }

    }

    public class GiFuBoPayMerchantInfo
    {
        public string MERCHANT_NO { get; set; }
        public string MERCHANT_KEY { get; set; }
        public string ORG_ID { get; set; }
        public string API_URL { get; set; }
    }

    /// <summary>
    /// 第三方支付基本支付通道設定(商户号、密钥...)
    /// </summary>
    public class GiFuBoPayBaseSetting
    {
        private string _merchantNo;
        private string _baseUrl;
        private string _key;
        public GiFuBoPayBaseSetting(string InfoData, string Url)
        {
            var InfoDataJson = Newtonsoft.Json.JsonConvert.DeserializeObject<GiFuBoPayMerchantInfo>(InfoData);
            _merchantNo = InfoDataJson.MERCHANT_NO;
            _key = InfoDataJson.MERCHANT_KEY;
            _baseUrl = Url + "/";
        }

        public string MerchantNo
        {
            get
            {
                return _merchantNo;
            }
        }

        public string Key
        {
            get
            {
                return _key;
            }
        }

        public string GetBaseUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return _baseUrl;
            }
            return _baseUrl + url;
        }
    }

    public class GiFuBoPayOrderData
    {
        /// <summary>
        /// 商户号ID
        /// </summary>
        public string MerID { get; set; }
        /// <summary>
        /// 商户key
        /// </summary>
        public string SignKey { get; set; }
        /// <summary>
        /// 店家交易编号
        /// </summary>
        public string MerTradeID { get; set; }
        /// <summary>
        /// 店家商品代号
        /// </summary>
        public string MerProductID { get; set; }
        /// <summary>
        /// 店家消费者ID
        /// </summary>
        public string MerUserID { get; set; }
        /// <summary>
        /// 交易金额
        /// </summary>
        public string Amount { get; set; }
        /// <summary>
        /// 交易描述
        /// </summary>
        public string TradeDesc { get; set; }
        /// <summary>
        /// 商品名称
        /// </summary>
        public string ItemName { get; set; }
        /// <summary>
        /// MD5签名串
        /// </summary>
        public string Sign { get; set; }
        /// <summary>
        /// 交易异步回调网址
        /// </summary>
        public string NotifyUrl { get; set; }
        /// <summary>
        /// 是否只回传支付链结地址，由贵司自行生成支付页面
        /// </summary>
        public string ReturnCodeURL { get; set; }

    }

    /// <summary>
    /// 支付下单地址
    /// </summary>
    public class GiFuBoPayOrder
    {
        /// <summary>
        /// 请求地址
        /// </summary>
        private Dictionary<string, string> Url;
        /// <summary>
        /// 支付所有需要欄位
        /// </summary>
        private GiFuBoPayOrderData payData;
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


        public GiFuBoPayOrder(GiFuBoPayBaseSetting PayBaseSetting)
        {
            payData = new GiFuBoPayOrderData();
            payData.MerID = PayBaseSetting.MerchantNo;
            payData.SignKey = PayBaseSetting.Key;
            payData.NotifyUrl = GiFuBoPay_NotifyUrl;
            payData.ReturnCodeURL = "0";
            payData.Amount = "0.00";

            defaultData = new Dictionary<string, object>();
            md5Data = new Dictionary<string, object>();
            postData = new Dictionary<string, object>();

            Url = new Dictionary<string, string>();
            SetSendUrl();

            payData.MerTradeID = string.Empty;
            payData.MerProductID = string.Empty;
            payData.MerUserID = string.Empty;
            payData.TradeDesc = string.Empty;
            payData.ItemName = string.Empty;
            payData.Sign = string.Empty;
            JsonStr = string.Empty;

            md5Sort = new List<string> { "MerID", "MerTradeID", "MerProductID", "MerUserID", "Amount", "SignKey" };
            postSort = new List<string> { "MerID", "MerTradeID", "MerProductID", "MerUserID", "Amount", "TradeDesc", "ItemName", "NotifyUrl", "Sign" };

            defaultData.Add("MerID", payData.MerID);
            defaultData.Add("SignKey", payData.SignKey);
            defaultData.Add("NotifyUrl", payData.NotifyUrl);
            defaultData.Add("ReturnCodeURL", payData.ReturnCodeURL);
        }

        /// <summary>
        /// 設定支付下单地址
        /// </summary>
        private void SetSendUrl()
        {
            //支付宝扫码支付
            Url.Add("ZHIFUBAO_QR", "AliQRCodePayment.php");
            //支付宝H5支付
            Url.Add("ZHIFUBAO_H5", "AliH5Payment.php");
            //微信扫码支付
            Url.Add("WEIXIN_QR", "WechatQRCodePayment.php");
            //云闪付扫码支付
            Url.Add("QUICKPASS_QR", "UnionYSFQRCodePayment.php");
        }

        /// <summary>
        /// 交易描述和商品名稱寫入支付類型
        /// </summary>
        /// <param name="payType">支付類型</param>
        private void SetPayType(string payType)
        {
            payData.TradeDesc = payType;
            payData.ItemName = payType;

            defaultData.Add("TradeDesc", payData.TradeDesc);
            defaultData.Add("ItemName", payData.ItemName);
        }

        /// <summary>
        /// 取得支付下单地址
        /// </summary>
        /// <param name="payType">支付類型</param>
        public string GetSendUrl(string payType)
        {
            if (string.IsNullOrEmpty(payType))
                return string.Empty;

            SetPayType(payType);
            return Url.ContainsKey(payType) ? Url[payType] : string.Empty;
        }

        /// <summary>
        /// 設定oder資料，商品ID使用orderID
        /// </summary>
        /// <param name="order"></param>
        public void SetOrderData(CDPayOrder order)
        {
            payData.Amount = order.Amount.ToString("F2");
            payData.MerTradeID = ThirdPartyOderIDService.GetThirdPartyOderID(order.Id, "GiFuBoPay", 20);
            payData.MerProductID = order.Id.ToString();
            payData.MerUserID = order.UserID.ToString();

            defaultData.Add("Amount", payData.Amount);
            defaultData.Add("MerTradeID", payData.MerTradeID);
            defaultData.Add("MerProductID", payData.MerProductID);
            defaultData.Add("MerUserID", payData.MerUserID);
        }

        /// <summary>
        /// 完成
        /// </summary>
        public void Done()
        {
            //sort md5
            md5Data = StringObjectDirctionaryService.SortStrDictionary(defaultData, md5Sort);
            //md5 sign
            payData.Sign = StringObjectDirctionaryService.GetMd5Content(md5Data).ToLower();
            defaultData.Add("Sign", payData.Sign);
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
            return payData.MerTradeID;
        }

        /// <summary>
        /// 取得post Dictionary
        /// </summary>
        public Dictionary<string, object> GetPostData()
        {
            return postData;
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
    /// callback Json 結構
    /// </summary>
    public class GiFuBoPayCallbackModel
    {
        /// <summary>
        /// 回传代码，1:成功,其他为失败
        /// </summary>
        public string RtnCode { get; set; }

        /// <summary>
        /// 回传讯息
        /// </summary>
        public string RtnMessage { get; set; }

        /// <summary>
        /// 店家交易编号
        /// </summary>
        public string MerTradeID { get; set; }

        /// <summary>
        /// 店家商品代号
        /// </summary>
        public string MerProductID { get; set; }

        /// <summary>
        /// 店家消费者ID
        /// </summary>
        public int MerUserID { get; set; }

        /// <summary>
        /// 交易金额
        /// </summary>
        public string Amount { get; set; }

        /// <summary>
        /// 付款时间
        /// </summary>
        public DateTime PaymentDate { get; set; }

        /// <summary>
        /// MD5签名串
        /// </summary>
        public string Sign { get; set; }
    }

    /// <summary>
    /// 异步通知
    /// </summary>
    public class GiFuBoPayReturn
    {
        /// <summary>
        /// callback回傳值JsonModel
        /// </summary>
        private GiFuBoPayCallbackModel _payReturnData;
        /// <summary>
        /// md5 sing string
        /// </summary>
        private string _payRetrunSignStringToMd5 = string.Empty;
        /// <summary>
        /// 建構式
        /// </summary>
        /// <param name="payReturnData">callback Json 結構</param>
        public GiFuBoPayReturn(GiFuBoPayCallbackModel payReturnData)
        {
            _payReturnData = payReturnData;
        }
        /// <summary>
        /// 取得訂單ID
        /// </summary>
        public int GetOrderID()
        {
            return Convert.ToInt32(_payReturnData.MerProductID);
        }
        /// <summary>
        /// 取得交易訂單ID
        /// </summary>
        public string GetPayOderID()
        {
            return _payReturnData.MerTradeID;
        }
        /// <summary>
        /// 回調是否成功
        /// </summary>
        public bool IsSuccess()
        {
            return _payReturnData.RtnCode == "1";
        }
        /// <summary>
        /// 取得金額
        /// </summary>
        public long GetPayAmount()
        {
            return (long)Decimal.Parse(_payReturnData.Amount);
        }
        /// <summary>
        /// 回調簽名是否正確
        /// </summary>
        /// <param name="payBaseSetting">商戶號等基本設定</param>
        public bool IsSignTrue(GiFuBoPayBaseSetting payBaseSetting)
        {
            Dictionary<string, object> signChkData = new Dictionary<string, object>();
            signChkData.Add("MerID", payBaseSetting.MerchantNo);
            signChkData.Add("RtnCode", _payReturnData.RtnCode);
            signChkData.Add("MerTradeID", _payReturnData.MerTradeID);
            signChkData.Add("MerUserID", _payReturnData.MerUserID);
            signChkData.Add("Amount", _payReturnData.Amount);
            signChkData.Add("SignKey", payBaseSetting.Key);
            _payRetrunSignStringToMd5 = StringObjectDirctionaryService.ConvertToUrlStringFormat(signChkData, "&");
            string signDChk = StringObjectDirctionaryService.GetMd5(signChkData, "&").ToLower();
            return _payReturnData.Sign == signDChk;
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