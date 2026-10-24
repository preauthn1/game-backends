using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using game.Entity.Payment;
using game.Infrastructure;
using static game.WebSite.PayConfig;
using game.WebSite.Services;
using game.Infrastructure.Extensions;
using game.Infrastructure.Utility.ThirdParty;

namespace game.WebSite.PaymentHelper
{
    /// <summary>
    /// 威付pay
    /// </summary>
    public class NewWeiFuPay
    {
        /// <summary>
        /// 创建订单(跳转网页链接)
        /// </summary>
        /// <param name="order">訂單</param>
        /// <param name="guid">LogGuID</param>
        /// <returns></returns>
        public static string Post(CDPayOrder order, string logGuid)
        {
            string message;
            string logMessage;
            string returnMsg = "目前支付人员太多, 请稍后再试";

            CDMerchantUUID merchant = MerchantDataService.GetMerchantData(order);
            //(pay_list.Count() == 0 return ""
            if (merchant == null)
            {
                logMessage = $"{order.Id} 支付请求失敗 [pay list] 0";
                SetLog(logGuid,logMessage);
                message = "支付通道不足";
                Service.CDFailOrder(order.Id, string.Empty, $" {message}");
                return returnMsg;
            }

            logMessage = $"訂單資訊 [ID]{order.Id}、[Amount]{order.Amount.ToString()}、[payType]{merchant.BankCode}";
            SetLog(logGuid,logMessage);

            //把GWID寫入訂單資料
            Service.CDNewModifyTransID(order.Id, "0", merchant.GWID);

            //获取accessToken設定
            var PayBaseSetting = new NewWeiFuPayBaseSetting(merchant.InfoData, merchant.Url);
            var GetNewWeiFuPayToken = [REDACTED] GetNewWeiFuPayToken(PayBaseSetting);
            string getTokenJson = GetNewWeiFuPayToken.GetTokenSendJson();
            string getTokenUrl = PayBaseSetting.GetBaseUrl("getAccessToken/merchant");
            string accessTokenReturn = SiteHelper.HttpPostJson(getTokenUrl, getTokenJson);

            var accessTokenReturnJson = new JsonStrToJsonObj(accessTokenReturn);
            bool chk = accessTokenReturnJson.GetJsonValueBoolen("success");

            //获取accessToken設定失敗
            if (!chk)
            {
                logMessage = $"获取accessToken設定失敗 [accessToken success]{chk}";
                SetLog(logGuid,logMessage);
                return returnMsg;
            }

            JObject accessTokenJson = accessTokenReturnJson.GetJsonObj();
            string accessToken = [REDACTED]"value").Value<string>("accessToken");

            logMessage = $"获取accessToken [md5SignData]{GetNewWeiFuPayToken.GetMd5Data()}、[sign]{GetNewWeiFuPayToken.GetSign()}";
            SetLog(logGuid,logMessage);
            logMessage = $"获取accessToken [Url]{getTokenUrl} 、[postJson]{getTokenJson}、[accessToken]{accessToken}";
            SetLog(logGuid,logMessage);

            //支付下单地址
            var NewWeiFuPayOrder = new NewWeiFuPayOrder(accessToken);
            NewWeiFuPayOrder.SetOrderData(order);
            string SendUrl = NewWeiFuPayOrder.GetSendUrl(merchant.BankCode);
            NewWeiFuPayOrder.Done();
            string payOrderJson = NewWeiFuPayOrder.GetPostJson();
            string payOrderUrl = PayBaseSetting.GetBaseUrl(SendUrl);
            string PayOrderReturn = SiteHelper.HttpPostJson(payOrderUrl, payOrderJson);

            logMessage = $"支付下单地址 [Url]{payOrderUrl} 、[postJson]{payOrderJson} 、[return]{PayOrderReturn}";
            SetLog(logGuid,logMessage);

            //取得回傳值產生HTML
            var PayOrderReturnJson = new JsonStrToJsonObj(PayOrderReturn);
            bool payOrderReturnCHK = PayOrderReturnJson.GetJsonValueBoolen("success");
            string payOrderReturnMsg = PayOrderReturnJson.GetJsonValueStr("message");
            string payOrderReturnData = PayOrderReturnJson.GetJsonValueStr("value");
            int payOrderReturnError = PayOrderReturnJson.GetJsonValueInt("errorCode");

            if (!payOrderReturnCHK)
            {
                returnMsg = "目前支付人员太多, 请稍后再试";
                logMessage = $"{order.Id} 支付请求失敗 [message]{payOrderReturnMsg} [data]{payOrderReturnData} [errorCode]{payOrderReturnError}";
                SetLog(logGuid,logMessage);
                message = $"[message]{payOrderReturnMsg} [data]{payOrderReturnData} [errorCode]{payOrderReturnError}";
                Service.CDFailOrder(order.Id, string.Empty, $" {message}");
            }
            else
            {
                returnMsg = "<script>window.location.href='" + payOrderReturnData + "';</script>";
                logMessage = $"{order.Id} 支付请求成功 [message]{payOrderReturnMsg} [data]{payOrderReturnData} [errorCode]{payOrderReturnError}";
                SetLog(logGuid,logMessage);
                Service.CDModifyTransID(order.Id, NewWeiFuPayOrder.GetOutTradeNo());
            }

            return returnMsg;
        }

        /// <summary>
        /// 代收交易非同步通知
        /// </summary>
        /// <param name="logGuid">LogGuID</param>
        public static PayState Call(string logGuid)
        {
            string LogMessage;
            int orderID = 0;
            try
            {
                var obj = new ResponseStringTransform();
                string responseString = obj.GetResponseString();

                //LOG
                LogMessage = string.Format("支付回调 [FormData] {0}", responseString);
                SetLog(logGuid,LogMessage);

                var payResponse = new NewWeiFuPayReturn(responseString);

                //SYFPAY0000...
                string payOrderID = payResponse.GetPayOderID();
                //取得實際orderid
                orderID = payResponse.GetOrderID();

                //檢查訂單狀態
                var orderlist = Service.CDGetAEPayOrderByID(orderID);
                if (orderlist.IsEmptyOrNull())
                {
                    LogMessage = $"{orderID} 支付回调 查無訂單";
                    SetLog(logGuid,LogMessage);
                    return PayState.Error;
                }

                var order = orderlist[0];
                if (order.Status == CDOrderStatus.Paied)
                {
                    LogMessage = $"{order.Id} 支付回调 訂單狀態已經完成,無須金幣充值";
                    SetLog(logGuid,LogMessage);
                    return PayState.Success;
                }

                if (!payResponse.IsSuccess())
                {
                    LogMessage = $"{order.Id} 支付回调 订单失敗";
                    SetLog(logGuid,LogMessage);
                    Service.CDFailOrder(order.Id, payOrderID, $"{payResponse.IsSuccess().ToString()}");
                    return PayState.Error;
                }

                //進行金幣充值
                if (payResponse.GetPayAmount() != order.Amount)
                {
                    LogMessage = string.Format($"{order.Id}  支付回调 支付無效 merchantOrderId:{payOrderID},totalAmount:{payResponse.GetPayAmountStr()}");
                    SetLog(logGuid,LogMessage);
                    Service.CDFailOrder(order.Id, payOrderID, $"responseCode:{payResponse.IsSuccess().ToString()}");
                    return PayState.Error;
                }

                string msg = "";
                var data = $"完成支付";

                if (Service.CDFinishAEOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, payOrderID, data, ref msg))
                {
                    LogMessage = $"{order.Id} 支付回调 金幣充值成功 merchantOrderId,:{payOrderID}, totalAmount:{payResponse.GetPayAmountStr()}";
                    SetLog(logGuid,LogMessage);
                    return PayState.Success;
                }
                else
                {
                    LogMessage = $"{order.Id} 支付回调 金幣充值错误 {msg}";
                    SetLog(logGuid,LogMessage);
                    Service.CDFailOrder(order.Id, payOrderID, $"{msg}");
                    return PayState.Error;
                }
            }
            catch (Exception ex)
            {
                string msg = $"[orderID]{orderID}[LogID]{logGuid}[message]{ex.ToString()}";

                MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
                {
                    Channel = SlackChannelEnum.n_api_payment,
                    Message = msg
                });

                LogMessage = string.Format("支付回调 异常错误 {0}", ex.ToString());
                SetLog(logGuid,LogMessage);
                return PayState.Error;
            }

        }

        /// <summary>
        /// Log紀錄
        /// </summary>
        /// <param name="LogMsg">Log紀錄內容</param>
        private static void SetLog(string LogGuID, string LogMsg)
        {
            SiteHelper.Log(LogGuID, LogMsg, NewWeiFuPay_logFile);
        }

    }

    /// <summary>
    /// 第三方支付基本支付通道設定(商户号、密钥...)
    /// </summary>
    public class NewWeiFuPayBaseSetting
    {
        private string _merchantNo;
        private string _baseUrl;
        private string _key;
        public NewWeiFuPayBaseSetting(string InfoData, string Url)
        {
            var infoDAtaJson = new JsonStrToJsonObj(InfoData);
            _merchantNo = infoDAtaJson.GetJsonValueStr("MERCHANT_NO");
            _key = infoDAtaJson.GetJsonValueStr("MERCHANT_KEY");
            _baseUrl = Url + "/";
        }

        public string merchantNo
        {
            get
            {
                return _merchantNo;
            }
        }

        public string key
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

    /// <summary>
    /// 获取accessToken設定
    /// </summary>
    public class GetNewWeiFuPayToken
    {
        /// <summary>
        /// 商户号
        /// </summary>
        private string merchantNo;
        /// <summary>
        /// 商户密钥
        /// </summary>
        private string key;
        /// <summary>
        /// 随机字符
        /// </summary>
        private string nonce;
        /// <summary>
        /// 时间戳
        /// </summary>
        private string timestamp;
        /// <summary>
        /// 签名
        /// </summary>
        private string sign;
        /// <summary>
        /// 令牌(可為空)
        /// </summary>
        private string token;
        /// <summary>
        /// md5欄位集合
        /// </summary>
        private Dictionary<string, object> md5Data;
        /// <summary>
        /// post欄位集合
        /// </summary>
        private Dictionary<string, object> postData;
        /// <summary>
        /// 欄位集合
        /// </summary>
        private Dictionary<string, object> defaultData;
        /// <summary>
        /// Md5欄位排序
        /// </summary>
        private List<string> md5Sort;
        /// <summary>
        /// post欄位排序
        /// </summary>
        private List<string> postSort;

        /// <summary>
        /// 建構式
        /// </summary>
        /// <param name="NewWeiFuPayBaseSetting">第三方支付基本支付通道設定</param>
        public GetNewWeiFuPayToken(NewWeiFuPayBaseSetting NewWeiFuPayBaseSetting)
        {
            merchantNo = NewWeiFuPayBaseSetting.merchantNo;
            key = NewWeiFuPayBaseSetting.key;
            nonce = "123456";
            token = [REDACTED];
            timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            md5Sort = new List<string> { "merchantNo", "nonce", "timestamp", "token", "key" };
            postSort = new List<string> { "merchantNo", "key", "token", "nonce", "timestamp", "sign" };

            defaultData = new Dictionary<string, object> { };
            md5Data = new Dictionary<string, object> { };
            postData = new Dictionary<string, object> { };

            defaultData.Add("merchantNo", merchantNo);
            defaultData.Add("key", key);
            defaultData.Add("nonce", nonce);
            defaultData.Add("token", token);
            defaultData.Add("timestamp", timestamp);
        }

        /// <summary>
        /// 取得获取accessToken的PostJson
        /// </summary>
        public string GetTokenSendJson()
        {
            //md5 sign
            if(string.IsNullOrEmpty(token))
            {
                md5Sort = md5Sort.Where(val => val != "token").ToList();
            }
            md5Data = StringObjectDirctionaryService.SortStrDictionary(defaultData, md5Sort);
            sign = StringObjectDirctionaryService.GetMd5(md5Data, "&");
            defaultData.Add("sign", sign);

            //sort post
            postData = StringObjectDirctionaryService.SortStrDictionary(defaultData, postSort);

            //to json
            string JsonStr = StringObjectDirctionaryService.ToJsonStr(postData);

            return JsonStr;
        }

        /// <summary>
        /// 取得获取accessToken的md5Date
        /// </summary>
        public string GetMd5Data()
        {
            return StringObjectDirctionaryService.ConvertToUrlStringFormat(md5Data, "&");
        }

        /// <summary>
        /// 取得获取accessToken的sign
        /// </summary>
        public string GetSign()
        {
            return sign;
        }
    }

    /// <summary>
    /// 支付下单地址
    /// </summary>
    public class NewWeiFuPayOrder
    {
        /// <summary>
        /// 请求地址
        /// </summary>
        private Dictionary<string, string> Url;
        /// <summary>
        /// 商户accessToken
        /// </summary>
        private string accessToken;
        /// <summary>
        /// 商户订单号
        /// </summary>
        private string outTradeNo;
        /// <summary>
        /// 金额(分)
        /// </summary>
        private long money;
        /// <summary>
        /// 付款类型 T0/T1
        /// </summary>
        private string type;
        /// <summary>
        /// 商品描述
        /// </summary>
        private string body;
        /// <summary>
        /// 商品详情
        /// </summary>
        private string detail;
        /// <summary>
        /// 后台通知地址
        /// </summary>
        private string notifyUrl;
        /// <summary>
        /// 商品ID
        /// </summary>
        private string productId;
        /// <summary>
        /// 前台通知地址（只是一个跳转地址，无返回）可空
        /// </summary>
        private string successUrl;
        /// <summary>
        /// callbackSuccessUrl 可空
        /// </summary>
        private string callbackSuccessUrl;
        /// <summary>
        /// post排序
        /// </summary>
        private List<string> postSort;
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
        /// 建構式
        /// </summary>
        /// <param name="AccessToken">Token</param>
        public NewWeiFuPayOrder(string AccessToken)
        {
            accessToken = [REDACTED];
            type = "T1";

            money = 0;
            outTradeNo = "NewWeiFu";
            productId = string.Empty;

            body = string.Empty;
            detail = string.Empty;

            notifyUrl = NewWeiFuPay_NotifyUrl;
            successUrl = string.Empty;
            callbackSuccessUrl = FinishUrl;

            postData = new Dictionary<string, object> { };
            defaultData = new Dictionary<string, object> { };

            Url = new Dictionary<string, string> { };
            SetSendUrl();

            postSort = new List<string> { "outTradeNo", "money", "type", "body", "detail", "notifyUrl", "successUrl", "callbackSuccessUrl", "productId" };
            JsonStr = string.Empty;

            defaultData.Add("accessToken", accessToken);
            defaultData.Add("type", type);
            defaultData.Add("notifyUrl", notifyUrl);
            defaultData.Add("successUrl", successUrl);
            defaultData.Add("callbackSuccessUrl", callbackSuccessUrl);
        }

        /// <summary>
        /// 設定支付下单地址
        /// </summary>
        private void SetSendUrl()
        {
            //支付宝扫码
            Url.Add("ZHIFUBAO_QR", "order/alipayScan");
            //支付宝wap
            Url.Add("ZHIFUBAO_H5", "order/alipayWapPay");
            //微信扫码
            Url.Add("WEIXIN_QR", "order/wechatScan");
            //微信wap
            Url.Add("WEIXIN_H5", "order/wechatWapPay");
            //QQ扫码
            Url.Add("QQ_QR", "order/qqScan");
            //京东扫码
            Url.Add("JDPAY_QR", "order/jdPay");
            //银联扫码
            Url.Add("UNIONPAY_QR", "order/unionpayScan");
            //网关
            Url.Add("WANGGUAN_ONLINE_BANK", "order/bankPay");
        }

        /// <summary>
        /// 取得支付下单地址
        /// </summary>
        /// <param name="payType">支付類型</param>
        public string GetSendUrl(string payType)
        {
            SetPayType(payType);
            return Url.ContainsKey(payType) ? Url[payType] : string.Empty;
        }

        /// <summary>
        /// 設定oder資料，商品ID使用orderID
        /// </summary>
        /// <param name="order"></param>
        public void SetOrderData(CDPayOrder order)
        {
            money = order.Amount * 100;
            outTradeNo = ThirdPartyOderIDService.GetThirdPartyOderID(order.Id, "NewWeiFu", 20);
            productId = order.Id.ToString();

            defaultData.Add("money", money);
            defaultData.Add("outTradeNo", outTradeNo);
            defaultData.Add("productId", productId);
        }

        /// <summary>
        /// 商品和商品詳細紀錄支付類型
        /// </summary>
        /// <param name="payType">支付類型</param>
        private void SetPayType(string payType)
        {
            body = payType;
            detail = payType;

            defaultData.Add("body", body);
            defaultData.Add("detail", detail);
        }

        /// <summary>
        /// 完成
        /// </summary>
        public void Done()
        {
            //sort post 不包括accessToken
            postData = StringObjectDirctionaryService.SortStrDictionary(defaultData, postSort);
            //to json
            string postDataJsonStr = StringObjectDirctionaryService.ToJsonStr(postData);
            JsonStr = "{\"accessToken\": \""+ accessToken + "\",\"param\":" + postDataJsonStr + "}";
        }

        /// <summary>
        /// 取得post json string
        /// </summary>
        public string GetPostJson()
        {
            return JsonStr;
        }

        /// <summary>
        /// 取得交易訂單號NewWeiFuPay0000XXX
        /// </summary>
        public string GetOutTradeNo()
        {
            return outTradeNo;
        }
    }

    /// <summary>
    /// 异步通知return to json model
    /// </summary>
    public class NewWeiFuPayReturnJsonModel
    {
        /// <summary>
        /// 系统订单号
        /// </summary>
        public string no;
        /// <summary>
        /// 商户订单号
        /// </summary>
        public string outTradeNo;
        /// <summary>
        /// 商户号
        /// </summary>
        public string merchantNo;
        /// <summary>
        /// 商品ID
        /// </summary>
        public string productId;
        /// <summary>
        /// 支付金额(分)
        /// </summary>
        public long money
        {
            set { }
            get { return money / 100; }
        }
        /// <summary>
        /// 订单内容
        /// </summary>
        public string body;
        /// <summary>
        /// 订单详情
        /// </summary>
        public string detail;
        /// <summary>
        /// 结算类型(如T0、T1)
        /// </summary>
        public string tradeType;
        /// <summary>
        /// 支付时间
        /// </summary>
        public DateTime Date;
        /// <summary>
        /// 随机字符
        /// </summary>
        public string nonce;
        /// <summary>
        /// 时间戳
        /// </summary>
        public long timestamp;
        /// <summary>
        /// 签名
        /// </summary>
        public string sign;
        /// <summary>
        /// 支付装态（付款成功：true,付款失败：false,未付款:null）
        /// </summary>
        public bool success;
    }

    /// <summary>
    /// 异步通知
    /// </summary>
    public class NewWeiFuPayReturn
    {
        /// <summary>
        /// callback回傳值JsonModel
        /// </summary>
        private NewWeiFuPayReturnJsonModel payReturnJson;
        /// <summary>
        /// 建構式
        /// </summary>
        /// <param name="responseString">call back return string</param>
        public NewWeiFuPayReturn(string responseString)
        {
            payReturnJson = Newtonsoft.Json.JsonConvert.DeserializeObject<NewWeiFuPayReturnJsonModel>(responseString);
        }

        public int GetOrderID()
        {
            return Convert.ToInt32(payReturnJson.productId);
        }

        public string GetPayOderID()
        {
            return payReturnJson.outTradeNo;
        }

        public bool IsSuccess()
        {
            return payReturnJson.success;
        }

        public long GetPayAmount()
        {
            return payReturnJson.money;
        }

        public string GetPayAmountStr()
        {
            return GetPayAmount().ToString();
        }
    }
}