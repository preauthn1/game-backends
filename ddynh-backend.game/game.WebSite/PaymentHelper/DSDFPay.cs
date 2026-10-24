using game.Entity.DTO.ThirdPartyPay;
using game.Entity.Payment;
using game.Infrastructure;
using game.Infrastructure.Extensions;
using game.Infrastructure.Utility.ThirdParty;
using game.WebSite.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using example.invalid;
using System.Reflection;
using System.Text;
using System.Web;
using static game.WebSite.PayConfig;

namespace game.WebSite.PaymentHelper
{
    /// <summary>
    /// 风云聚合
    /// </summary>
    public class DSDFPay
    {
        /// <summary>
        /// 創建訂單
        /// </summary>
        /// <param name="order">資料庫訂單模型</param>
        /// <param name="logGuid">LOG File ID</param>
        /// <returns>回傳網址或訂單View</returns>
        public static string Post(CDPayOrder order, string logGuid)
        {
            var postAction = new ThirdPartyPaymentPostAction(logGuid, DSDFPay_logFile);
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
            var PayOrder = new DSDFPayOrder(PayBaseSetting);
            PayOrder.SetOrderData(order);
            PayOrder.SetPayType(merchant.BankCode);
            PayOrder.Done();

            //紀錄交易編號
            string ThirdPartyOderID = PayOrder.GetThirdPartyOderID();
            postAction.LogThirdPartyOderID(ThirdPartyOderID);

            //紀錄MD5資料
            string signBeforeData = PayOrder.GetSignBeforeString();
            postAction.LogPayOrderPostSignBeforeString(signBeforeData);

            //紀錄postJson 、 action url 和 ReturnString
            string payOrderUrl = PayBaseSetting.GetBaseUrl(DSDFPay_CreateOrderApiUrl);
            string payOrderInput = PayOrder.GetPostInputString();
            postAction.LogPayOrderPostDataString(payOrderInput, payOrderUrl);

            var payOrderPostData = PayOrder.GetPostData();
            string returnHtml = postAction.GetFormAutoSubmitHtml(payOrderPostData, payOrderUrl);
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
            string payTypeName = "风云聚合";
            try
            {
                //是否跳出function return
                bool isCallReturn;
                var payCallAction = new ThirdPartyPaymentCallAction(logGuid, DSDFPay_logFile);
                payCallAction.LogResponseString();
                string responseString = payCallAction.GetResponseString();

                var payResponse = new DSDFPayResponse(responseString);

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

                //確認是否為充值
                bool isPayment = payResponse.IsPayment();
                isCallReturn = payCallAction.CheckTransactionIsPayment(isPayment);
                //若訂單確認失敗則返回
                if (isCallReturn)
                {
                    return payCallAction.PayState;
                }

                //確認是否支付成功
                bool isPaySuccess = payResponse.IsPaySuccess();
                isCallReturn = payCallAction.CheckTransactionIsSuccess(isPaySuccess);
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
                string signBeforeString = payResponse.GetSingBeforeString();
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
                SiteHelper.Log(logGuid, LogMessage, DSDFPay_logFile);
                return PayState.Error;
            }
        }

        /// <summary>
        /// 代付回調
        /// </summary>
        /// <param name="logGuid"></param>
        /// <returns></returns>
        public static PayState WithdrawCallback(string logGuid)
        {
            try
            {
                Dictionary<string, string> parameters = null;

                if (CheckSign(logGuid, out parameters))
                {
                    var msg = parameters["status"].ToLower();
                    var id = parameters["order_id"].ToString();
                    var order_num = Convert.ToInt32(parameters["order_id"]);

                    var state = 1;  //(1:已提交,2:已处理,3:已退回,4.已出账)
                    switch (parameters["status"].ToLower())
                    {
                        case "verified":  //成功支付
                            state = 4;  //已提交
                            break;
                        case "revoked":  //撤销的
                            state = 3;
                            break;
                        case "created":  //等待支付
                            state = 2;  //已处理
                            break;
                        case "timeout":  //超时的
                            state = 3;  //已处理
                            break;
                    }

                    var res = Service.WithdrawCallbackByCK(order_num, state, id, msg);
                    if (res.Result)
                    {
                        SiteHelper.Log(logGuid, $"提现回调成功 order_num={order_num}&state={state}&id={id}", DSDFPayWithdraw_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"提现回调错误 {res.Message}", DSDFPayWithdraw_logFile);
                        return PayState.Error;
                    }
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("提现异步回调非法签名"), DSDFPayWithdraw_logFile);
                    return PayState.SignError;
                }
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("提现异步回调异常错误 {0}", ex.ToString()), DSDFPayWithdraw_logFile);
                return PayState.Error;
            }
        }

        private static bool CheckSign(string guid, out Dictionary<string, string> parameters)
        {
            Stream stream = HttpContext.Current.Request.InputStream;
            var b = new byte[stream.Length];
            stream.Read(b, 0, (int)stream.Length);
            var responseString = Encoding.UTF8.GetString(b);
            SiteHelper.Log(guid, string.Format("支付回调 [FormData] {0}", responseString), DSDFPayWithdraw_logFile);

            JObject json = JObject.Parse(responseString);
            var orderID = json["order_id"] != null ? json["order_id"].ToString() : string.Empty;  //接入商网站提交的订单号
            var amount = json["amount"] != null ? json["amount"].ToString() : string.Empty;  //金额，单位 分，没有小数点
            var verifiedTime = json["verified_time"] != null ? json["verified_time"].ToString() : string.Empty;  //订单完成时间，UNIX时间戳秒值

            parameters = new Dictionary<string, string>();
            parameters.Add("order_id", orderID);
            parameters.Add("direction", json["direction"] != null ? json["direction"].ToString() : string.Empty);  //out
            parameters.Add("amount", amount);
            parameters.Add("customer_name", json["customer_name"] != null ? json["customer_name"].ToString() : string.Empty);  //接入商网站的会员ID(提单API传递过来的)
            parameters.Add("verified_time", verifiedTime);
            parameters.Add("created_time", json["created_time"] != null ? json["created_time"].ToString() : string.Empty);  //订单创建时间，UNIX时间戳秒值
            parameters.Add("status", json["status"] != null ? json["status"].ToString() : string.Empty);  //此参数判断订单结果：verified = 成功支付
            parameters.Add("out_cardnumber", json["out_cardnumber"] != null ? json["out_cardnumber"].ToString() : string.Empty);  //会员提现，系统使用的出款卡
            parameters.Add("trans_fee", json["trans_fee"] != null ? json["trans_fee"].ToString() : string.Empty);  //会员提现，银行收取的手续费 单位 分


            string str = $"order_id={orderID}&amount={amount}&verified_time={verifiedTime}";

            var encoding = new System.Text.UTF8Encoding();
            byte[] keyByte = encoding.GetBytes("g4F3MTzeYQ0dXgA81zCmtrpadoAOwmbxDSd7ES9r3qII2XsmOJJni4jBIlFmFwtl");
            byte[] inputByte = encoding.GetBytes(str);
            var hmacsha = new System.Security.Cryptography.HMACSHA1(keyByte);
            byte[] hashmessage = hmacsha.ComputeHash(inputByte);
            string cSign = Convert.ToBase64String(hashmessage);
            var sign = json["qsign"] != null ? json["qsign"].ToString() : string.Empty;  // 简易签名，签名内容：order_id=xxx&amount=xxx&verified_time=xxx

            SiteHelper.Log(guid, $"代付回调签名 cSign={cSign} sign={sign} srt={str}", DSDFPayWithdraw_logFile);
            return sign == cSign;
        }
    }

    public class DSDFPaySignService
    {
        private string _merchantKey;
        public DSDFPaySignService(string merchantKey)
        {
            _merchantKey = merchantKey;
        }
        public string Sign(string inputUrlFormat)
        {
            var encoding = new System.Text.UTF8Encoding();
            byte[] keyByte = encoding.GetBytes(_merchantKey);
            byte[] inputByte = encoding.GetBytes(inputUrlFormat);

            var hmacsha = new System.Security.Cryptography.HMACSHA1(keyByte);
            byte[] hashmessage = hmacsha.ComputeHash(inputByte);
            return Convert.ToBase64String(hashmessage);
        }

        public bool IsSignTrue(string inputUrlFormat, string sign)
        {
            string signChk = Sign(inputUrlFormat);
            return sign == signChk;
        }
    }

    public class DSDFPayTypeBankCode
    {
        private Dictionary<string, string> payTypeDic;
        private string bankcode;
        private string type;
        private string tflag;

        public DSDFPayTypeBankCode(string payType)
        {
            payTypeDic = new Dictionary<string, string>();
            payTypeDic.Add("ZHIFUBAO", "ALIPAY");
            payTypeDic.Add("WEIXIN", "WebMM");
            payTypeDic.Add("QQ", "QQPAY");
            payTypeDic.Add("QUICKPASS", "UNIPAY");

            bankcode = payType;
            string[] temp = payType.Split('_');
            type = temp[1];
            tflag = temp[0];
        }

        public string GetPayType()
        {
            if (bankcode == "UNIONPAY_ONLINE_BANK")
                return "online";

            if (bankcode == "EXPRESSPAY_H5")
                return "quick";

            if (type == "QR")
                return "qrcode";

            if (type == "H5")
                return "remit";

            return "qrcode";
        }

        public string GetPayTflag()
        {
            if (payTypeDic.ContainsKey(tflag))
                return payTypeDic[tflag];

            return payTypeDic["ZHIFUBAO"];
        }
    }

    /// <summary>
    /// 創建訂單
    /// </summary>
    public class DSDFPayOrder
    {
        /// <summary>
        /// 支付所有需要欄位
        /// </summary>
        private DSDFPayOrderModel PayOrderModel;
        /// <summary>
        /// 簽名排序
        /// </summary>
        private List<string> SignSortList;
        /// <summary>
        /// 表單排序
        /// </summary>
        private List<string> PostSortList;
        /// <summary>
        /// 表單資料(產生FROM使用)
        /// </summary>
        private Dictionary<string, object> PostData;
        /// <summary>
        /// 此字串md5之後將存成sign
        /// </summary>
        private string SignBeforeString;
        /// <summary>
        /// post string
        /// </summary>
        private string InputString;

        /// <summary>
        /// 建構式
        /// </summary>
        public DSDFPayOrder(ThirdPartyPayMerchantInfoSetting payBaseSetting)
        {
            SignBeforeString = string.Empty;
            InputString = string.Empty;
            PostData = new Dictionary<string, object>();
            PostSortList = new List<string> { "cid", "uid", "time", "amount", "order_id", "ip", "sign", "type", "tflag" };
            SignSortList = new List<string>(PostSortList);
            SignSortList.Remove("sign");
            SignSortList.Remove("type");
            SignSortList.Remove("tflag");
            SetDefaultPayData(payBaseSetting);
        }

        /// <summary>
        /// 設定支付類型(service)
        /// </summary>
        /// <param name="payType">支付類型</param>
        public void SetPayType(string payType)
        {
            var payTypeObj = new DSDFPayTypeBankCode(payType);

            PayOrderModel.type = payTypeObj.GetPayType();
            PayOrderModel.tflag = payTypeObj.GetPayTflag();
            //若是快捷或網銀則不傳此欄位(到收銀台畫面後選擇)
            if (payType == "EXPRESSPAY_H5" || payType == "UNIONPAY_ONLINE_BANK")
            {
                PostSortList.Remove("tflag");
            }
        }

        /// <summary>
        /// 設定oder資料，商品ID使用orderID
        /// </summary>
        /// <param name="order"></param>
        public void SetOrderData(CDPayOrder order)
        {
            PayOrderModel.amount = order.Amount;
            PayOrderModel.order_id = ThirdPartyOderIDService.GetThirdPartyOderID(order.Id, "DSDFPay", length: 20);
            PayOrderModel.uid = order.UserID.ToString();
            PayOrderModel.ip = order.IPAddress;
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
            return PayOrderModel.order_id;
        }

        public string GetSignBeforeString()
        {
            return SignBeforeString;
        }

        /// <summary>
        /// 取得post Json to String(紀錄LOG使用)
        /// </summary>
        public string GetPostInputString()
        {
            return InputString;
        }

        public Dictionary<string, object> GetPostData()
        {
            return PostData;
        }

        /// <summary>
        /// 設定基本DSDFPayOrderModel
        /// </summary>
        /// <param name="payBaseSetting">商戶號等等基本設定</param>
        private void SetDefaultPayData(ThirdPartyPayMerchantInfoSetting payBaseSetting)
        {
            PayOrderModel = new DSDFPayOrderModel();
            PayOrderModel.cid = payBaseSetting.MerchantInfo.MERCHANT_NO;
            PayOrderModel.key = payBaseSetting.MerchantInfo.MERCHANT_KEY;

            PayOrderModel.time = DateTime.UtcNow.CovertToUnixTimestamp();
        }

        /// <summary>
        /// 依照排序設定Dictionary
        /// </summary>
        private void SetDictionary(List<string> stringArr, Dictionary<string, object> dictionary)
        {
            foreach (var key in stringArr)
            {
                var list = PayOrderModel.GetType().GetProperties();
                foreach (PropertyInfo prop in PayOrderModel.GetType().GetProperties())
                {
                    if (prop.Name == key)
                    {
                        var value = prop.GetValue(PayOrderModel, null);
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
            Dictionary<string, object> data = new Dictionary<string, object>();
            SetDictionary(SignSortList, data);
            SignBeforeString = StringObjectDirctionaryService.ConvertToUrlStringFormat(data, "&");
            //sign
            var signObj = new DSDFPaySignService(PayOrderModel.key);
            PayOrderModel.sign = signObj.Sign(SignBeforeString);
        }

        /// <summary>
        /// 設定post資料(JSON 或者INPUT FORM)
        /// </summary>
        private void SetPost()
        {
            SetDictionary(PostSortList, PostData);
            //to UrlFormat(紀錄LOG使用)
            InputString = StringObjectDirctionaryService.ConvertToUrlStringFormat(PostData, "&");
        }
    }

    /// <summary>
    /// 支付第三方異步通知
    /// </summary>
    public class DSDFPayResponse
    {
        /// <summary>
        /// 四方支付訂單回傳參數格式
        /// </summary>
        private DSDFPayResponseModel PayResponseModel;
        /// <summary>
        /// MD5 SIGN前字串
        /// </summary>
        private string signBeforeString = string.Empty;

        /// <summary>
        /// 建構式
        /// </summary>
        public DSDFPayResponse(string responseString)
        {
            PayResponseModel = JsonConvert.DeserializeObject<DSDFPayResponseModel>(responseString);
        }

        /// <summary>
        /// 取得訂單ID(資料庫OrderListID)
        /// </summary>
        public int GetOrderID()
        {
            return ThirdPartyOderIDService.GetOrderID(PayResponseModel.order_id, "DSDFPay");
        }
        /// <summary>
        /// 取得交易訂單ID(給第三方的訂單ID)
        /// </summary>
        public string GetThirdPartyOderID()
        {
            return PayResponseModel.order_id;
        }
        /// <summary>
        /// 取得金額
        /// </summary>
        public int GetPayAmount()
        {
            return Convert.ToInt32(PayResponseModel.orig_amount / 100);
        }
        /// <summary>
        /// 確認是否為充值(direction: in/out,  充值 or 提现 )
        /// </summary>
        public bool IsPayment()
        {
            return PayResponseModel.direction == "in";
        }
        /// <summary>
        /// 確認訂單是否支付成功(status: "verified / timeout / revoked"  ​此参数判断订单结果：verified = 成功支付 )
        /// </summary>
        public bool IsPaySuccess()
        {
            return PayResponseModel.status == "verified";
        }
        /// <summary>
        /// 回調簽名是否正確
        /// </summary>
        /// <param name="payBaseSetting">商戶號等基本設定</param>
        public bool IsSignTrue(ThirdPartyPayMerchantInfoSetting payBaseSetting)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add("order_id", PayResponseModel.order_id);
            data.Add("amount", PayResponseModel.amount);
            data.Add("verified_time", PayResponseModel.verified_time);
            signBeforeString = StringObjectDirctionaryService.ConvertToUrlStringFormat(data, "&");

            var signObj = new DSDFPaySignService(payBaseSetting.MerchantInfo.MERCHANT_KEY);
            return signObj.IsSignTrue(signBeforeString, PayResponseModel.qsign);
        }
        /// <summary>
        /// 取得驗證簽名MD5使用字串
        /// </summary>
        public string GetSingBeforeString()
        {
            return signBeforeString;
        }
    }
}