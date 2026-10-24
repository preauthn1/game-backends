using game.Entity.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static game.WebSite.PayConfig;
using game.Infrastructure.Extensions;
using System.Security.Cryptography;
using game.WebSite.Services;
using System.Reflection;
using game.Entity.DTO.ThirdPartyPay;
using game.Entity.Enums.ThirdPartyPay;
using Newtonsoft.Json;
using game.Infrastructure.Utility.ThirdParty;
using game.Infrastructure;

namespace game.WebSite.PaymentHelper
{
    public class SFTPay
    {
        /// <summary>
        /// 創建訂單
        /// </summary>
        /// <param name="order">資料庫訂單模型</param>
        /// <param name="logGuid">LOG File ID</param>
        /// <returns>回傳網址或訂單View</returns>
        public static string Post(CDPayOrder order, string logGuid)
        {
            var postAction = new ThirdPartyPaymentPostAction(logGuid, SFTPay_logFile);
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
            var PayBaseSetting = new ThirdPartyPayMerchantInfoSetting(merchant.InfoData, merchant.Url);
            var PayOrder = new SFTPayOrder(PayBaseSetting);
            PayOrder.SetOrderData(order);
            PayOrder.SetPayType(merchant.BankCode);
            PayOrder.Done();

            //紀錄交易編號
            string ThirdPartyOderID = PayOrder.GetThirdPartyOderID();
            postAction.LogThirdPartyOderID(ThirdPartyOderID);

            //紀錄MD5資料
            string signBeforeData = PayOrder.GetSignBeforeData();
            postAction.LogPayOrderPostSignBeforeString(signBeforeData);

            //紀錄postJson 、 action url 和 ReturnString
            string payOrderUrl = PayBaseSetting.GetBaseUrl(SFTPay_CreateOrderApiUrl);
            string payOrderInput = PayOrder.GetPostInputString();
            postAction.LogPayOrderPostDataString(payOrderInput, payOrderUrl);

            string payOrderReturnString = SiteHelper.HttpPost(payOrderUrl, payOrderInput);
            postAction.LogPayReturnString(payOrderReturnString);

            //處理回傳json
            var payReturn = new SFTPayReturn(payOrderReturnString);

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
        /// <returns>是否回調正確並支付成功</returns>
        public static PayState Call(string logGuid, SFTPayResponseModel model)
        {
            int orderID = 0;
            int payTypeID = 0;
            string payTypeName = "上分通支付";
            try
            {
                //是否跳出function return
                bool isCallReturn;
                var payCallAction = new ThirdPartyPaymentCallAction(logGuid, SFTPay_logFile);

                string responseString = JsonConvert.SerializeObject(model).ToString();
                payCallAction.LogResponseString(responseString);

                var payResponse = new SFTPayResponse(model);

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

                var payBaseSetting = new ThirdPartyPayMerchantInfoSetting(merchant.InfoData, merchant.Url);
                bool isSignTrue = payResponse.IsSignTrue(payBaseSetting);
                string signBeforeString = payResponse.GetSingBeforeData();
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
                SiteHelper.Log(logGuid, LogMessage, SFTPay_logFile);
                return PayState.Error;
            }
        }
    }

    public class SFTPaySignService
    {
        private string _merchantKey;
        private byte[] _merchantKeyASCIIByte = new byte[256];
        public byte[] MerchantKeyASCIIByte { get { return _merchantKeyASCIIByte; } }
        private byte[] _md5ASCIIByte = new byte[32];
        public byte[] Md5ASCIIByte { get { return _md5ASCIIByte; } }
        public SFTPaySignService(string merchantKey)
        {
            _merchantKey = merchantKey;
            _merchantKeyASCIIByte = InitKey(_merchantKey);
        }


        private byte[] InitKey(string keyString)
        {
            byte[] keyByte = System.Text.Encoding.ASCII.GetBytes(keyString);
            byte[] state = Enumerable.Range(0, 256).Select(c => Convert.ToByte(c)).ToArray();

            if (keyByte.IsEmptyOrNull())
                return null;

            byte index1 = 0;
            byte index2 = 0;
            for (var i = 0; i <= 255; i++)
            {
                index2 = (byte)((keyByte[index1] & 0xff) + (state[i] & 0xff) + index2 & 0xff);
                index1 = (byte)((index1 + 1) % keyByte.Length);

                byte temp = state[i];
                state[i] = state[index2];
                state[index2] = temp;
            }
            return state;
        }

        public string Sign(string amountString, string thirdPartyPayOrderID)
        {
            string md5String = MD5Service.Hash(amountString + thirdPartyPayOrderID).ToLower();
            byte[] md5StringByte = System.Text.Encoding.ASCII.GetBytes(md5String);
            _md5ASCIIByte = EncryRC4Byte(md5StringByte);

            string output = string.Join("", MD5.Create().ComputeHash(_md5ASCIIByte).Select(s => s.ToString("x2")));
            return output;
        }

        private byte[] EncryRC4Byte(byte[] input)
        {
            if (input.IsEmptyOrNull())
                return null;

            int x = 0;
            int y = 0;
            int xorIndex;
            byte[] result = new byte[input.Length];

            for (var i = 0; i < input.Length; i++)
            {
                x = (x + 1) & 0xff;
                y = ((_merchantKeyASCIIByte[x] & 0xff) + y) & 0xff;
                byte tmp = _merchantKeyASCIIByte[x];
                _merchantKeyASCIIByte[x] = _merchantKeyASCIIByte[y];
                _merchantKeyASCIIByte[y] = tmp;
                xorIndex = ((_merchantKeyASCIIByte[x] & 0xff) + (_merchantKeyASCIIByte[y] & 0xff)) & 0xff;
                result[i] = (byte)(input[i] ^ _merchantKeyASCIIByte[xorIndex]);
            }
            return result;

        }

        public bool IsSignTrue(string amountString, string thirdPartyPayOrderID, string signChk)
        {
            string sign = Sign(amountString, thirdPartyPayOrderID);
            return sign == signChk;
        }
    }

    /// <summary>
    /// 創建訂單
    /// </summary>
    public class SFTPayOrder
    {
        /// <summary>
        /// 支付所有需要欄位
        /// </summary>
        private SFTPayOrderModel PayOrderModle;
        /// <summary>
        /// 表單input排序
        /// </summary>
        private List<string> FormSortList;
        /// <summary>
        /// 紀錄簽名與表單資訊
        /// </summary>
        private Dictionary<string, object> Data;
        /// <summary>
        /// 此字串md5之後將存成sign
        /// </summary>
        private string SignBeforeData;
        /// <summary>
        /// post string
        /// </summary>
        private string InputString;

        /// <summary>
        /// 建構式
        /// </summary>
        public SFTPayOrder(ThirdPartyPayMerchantInfoSetting payBaseSetting)
        {
            SignBeforeData = string.Empty;
            InputString = string.Empty;
            Data = new Dictionary<string, object>();
            FormSortList = new List<string> { "accountId", "channel", "payType", "payWay", "amount", "outTradeNo", "contentType", "notifyUrl", "successUrl","sign" };
            SetDefaultPayData(payBaseSetting);
        }

        /// <summary>
        /// 設定支付類型(service)
        /// </summary>
        /// <param name="payType">支付類型</param>
        public void SetPayType(string payType)
        {
            string[] temp = payType.Split('_');
            string channel = temp[0];
            string type = temp[1];
            PayOrderModle.channel = (byte)ExtensionOfEnum.GetEnumByTextValue<SFTPayChannelEnum>(channel);
            PayOrderModle.payType = (byte)ExtensionOfEnum.GetEnumByTextValue<SFTPayTypeEnum>(type);
            PayOrderModle.payWay = (byte)ExtensionOfEnum.GetEnumByTextValue<SFTPayWayEnum>("QRCode");
        }

        /// <summary>
        /// 設定oder資料，商品ID使用orderID
        /// </summary>
        /// <param name="order"></param>
        public void SetOrderData(CDPayOrder order)
        {
            PayOrderModle.amountInt = order.Amount;
            PayOrderModle.outTradeNo = ThirdPartyOderIDService.GetThirdPartyOderID(order.Id, "SFTPay", length: 20);
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
            return PayOrderModle.outTradeNo;
        }

        public string GetSignBeforeData()
        {
            return SignBeforeData;
        }

        /// <summary>
        /// 取得post Json to String(紀錄LOG使用)
        /// </summary>
        public string GetPostInputString()
        {
            return InputString;
        }

        /// <summary>
        /// 設定基本SFTPayOrderModle
        /// </summary>
        /// <param name="payBaseSetting">商戶號等等基本設定</param>
        private void SetDefaultPayData(ThirdPartyPayMerchantInfoSetting payBaseSetting)
        {
            PayOrderModle = new SFTPayOrderModel();
            PayOrderModle.accountId = payBaseSetting.MerchantInfo.MERCHANT_NO;
            PayOrderModle.key = payBaseSetting.MerchantInfo.MERCHANT_KEY;
            PayOrderModle.notifyUrl = SFTPay_NotifyUrl;
            PayOrderModle.successUrl = FinishUrl;
            PayOrderModle.contentType = "json";
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
            var signObj = new SFTPaySignService(PayOrderModle.key);
            //md5 sign
            PayOrderModle.sign = signObj.Sign(PayOrderModle.amount, PayOrderModle.outTradeNo);
            SignBeforeData = $"[amount] {PayOrderModle.amount}; [outTradeNo] {PayOrderModle.outTradeNo}; [merchantKey] {PayOrderModle.key}";
        }

        /// <summary>
        /// 設定post資料(JSON 或者INPUT FORM)
        /// </summary>
        private void SetPost()
        {
            SetDictionary(FormSortList, Data);
            //to UrlFormat(紀錄LOG使用)
            InputString = StringObjectDirctionaryService.ConvertToUrlStringFormat(Data, "&");
        }
    }

    /// <summary>
    /// 第三方訂單return
    /// </summary>
    public class SFTPayReturn
    {
        /// <summary>
        /// SFTPay訂單回傳參數格式
        /// </summary>
        private SFTPayReturnModel PayReturnModel;

        /// <summary>
        /// 建構式
        /// </summary>
        public SFTPayReturn(string payReturnJsonString)
        {
            PayReturnModel = JsonConvert.DeserializeObject<SFTPayReturnModel>(payReturnJsonString);
        }

        /// <summary>
        /// 檢查回傳json是否成功
        /// </summary>
        public bool IsSuccess()
        {
            return PayReturnModel.code == (short)WebStatusEnum.SUCCESS;
        }

        /// <summary>
        /// 取得回傳訊息
        /// </summary>
        /// <returns></returns>
        public string GetMessage()
        {
            if (IsSuccess())
                return PayReturnModel.msg;
            else
                return PayReturnModel.userMsg;
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
    public class SFTPayResponse
    {
        /// <summary>
        /// 四方支付訂單回傳參數格式
        /// </summary>
        private SFTPayResponseModel PayResponseModel;
        /// <summary>
        /// MD5 SIGN前字串
        /// </summary>
        private string _signBeforeData = string.Empty;

        /// <summary>
        /// 建構式
        /// </summary>
        public SFTPayResponse(SFTPayResponseModel model)
        {
            PayResponseModel = model;
        }

        /// <summary>
        /// 取得訂單ID(資料庫OrderListID)
        /// </summary>
        public int GetOrderID()
        {
            return ThirdPartyOderIDService.GetOrderID(PayResponseModel.outTradeNo, "SFTPay");
        }
        /// <summary>
        /// 取得交易訂單ID(給第三方的訂單ID)
        /// </summary>
        public string GetThirdPartyOderID()
        {
            return PayResponseModel.outTradeNo;
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
        public bool IsSignTrue(ThirdPartyPayMerchantInfoSetting payBaseSetting)
        {
            _signBeforeData = $"[amount] {PayResponseModel.amount}; [outTradeNo] {PayResponseModel.outTradeNo}; [merchantKey] {payBaseSetting.MerchantInfo.MERCHANT_KEY}";
            var signObj = new SFTPaySignService(payBaseSetting.MerchantInfo.MERCHANT_KEY);
            return signObj.IsSignTrue(PayResponseModel.amount, PayResponseModel.outTradeNo,PayResponseModel.sign);
        }
        /// <summary>
        /// 取得驗證簽名MD5使用字串
        /// </summary>
        public string GetSingBeforeData()
        {
            return _signBeforeData;
        }
    }
}