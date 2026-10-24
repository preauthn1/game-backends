using game.Entity.DTO.ThirdPartyPay;
using game.Entity.Payment;
using game.Infrastructure;
using game.Infrastructure.Extensions;
using game.Infrastructure.Utility.ThirdParty;
using game.Infrastructure.Utility;
using game.WebSite.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using example.invalid;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Text;
using static game.WebSite.PayConfig;

namespace game.WebSite.PaymentHelper
{
    /// <summary>
    /// 聚宝盆
    /// </summary>
    public class MTPay
    {
        public static string Post(CDPayOrder order, string guid, ref string outMessage)
        {
            string merchant_url = string.Empty;  //API 网址
            string return_url = string.Empty;
            string product_id = string.Empty;    //店家商品代号

            //platformID
            var platform_id = SiteHelper.GetAPIResult<int>($"/Payment/GetPlatformId?UserID={order.UserID.ToString()}");
            var url = $"/Payment/GetMerchantUuid?payTypeID={order.PayTypeID}&platformID={platform_id}";
            var pay_list = SiteHelper.GetAPIResult<List<CDMerchantUUID>>(url);

            //訂單編號處理
            var trade_id = "000000000000" + order.Id.ToString();   //店家自行设定，不得小于 6 个字符，不得重复
            trade_id = MTPay_perfix + trade_id.Substring((trade_id.Length - 10));

            if (pay_list.Count() == 0)
            {
                SiteHelper.Log(guid, $"{order.Id} 支付请求失敗 [pay list]{pay_list.Count()}", MTPay_logFile);
                outMessage = "支付通道不足";
                Service.CDFailOrder(order.Id, string.Empty, $" {outMessage}");
            }
            else
            {
                var r = (pay_list.Count() > 1) ? new Random().Next(0, pay_list.Count() - 1) : 0;
                var gwid = pay_list[r].GWID;
                var mainUrl = pay_list[r].Url;
                var bankCode = pay_list[r].BankCode;
                var infoData = JObject.Parse(pay_list[r].InfoData);

                //將thrid party order no寫入Pay.OrderList.TransID
                Service.CDNewModifyTransID(order.Id, "0", gwid);

                var merchant_no = infoData["MERCHANT_NO"] != null ? infoData["MERCHANT_NO"].ToString() : string.Empty;      //商户号
                var merchant_key = infoData["MERCHANT_KEY"] != null ? infoData["MERCHANT_KEY"].ToString() : string.Empty;   //商户MD5密钥
                var display_name = infoData["DISPLAY_NAME"] != null ? infoData["DISPLAY_NAME"].ToString() : string.Empty;   //訂單顯示商戶名稱

                var path = "/qrh5";
                switch (bankCode)
                {
                    //宝转卡H5
                    case "ZHIFUBAO_H5":
                        product_id = "ALIPAY2CARDH5";
                        break;
                    //卡对卡网银
                    case "UNIONPAY_QR":
                        product_id = "WEBBANK";
                        path = "/per-bank";
                        break;
                    //宝转卡扫码
                    default:  //ZHIFUBAO_QR
                        product_id = "ALIPAY2CARDQR";
                        break;
                }
                mainUrl += path;
                var a1 = order.Amount.ToString("F2");

                Dictionary<string, string> requestData = new Dictionary<string, string>();
                requestData.Add("amount", order.Amount.ToString("F2"));
                if (bankCode == "UNIONPAY_QR")
                {
                    requestData.Add("type", product_id);
                }
                else
                {
                    requestData.Add("paymentCode", product_id);
                }
                requestData.Add("orderNo", trade_id);
                requestData.Add("orderTime", DateTime.Now.ToString("yyyyMMddHHmmss"));
                if (bankCode != "UNIONPAY_QR")
                {
                    requestData.Add("returnUrl", FinishUrl);
                }
                requestData.Add("notifyUrl", MTPay_notifyurl);
                requestData.Add("memberCode", order.UserID.ToString());
                if (bankCode != "UNIONPAY_QR")
                {
                    requestData.Add("merchantName", display_name);
                }

                //編成 Base64 字串
                string jsonString = "{" + string.Join(",", requestData.Select(d => string.Format("\"{0}\": \"{1}\"", d.Key, string.Join(",", d.Value)))) + "}";
                SiteHelper.Log(guid, $"{order.Id} base64 encode [before] {jsonString} ", MTPay_logFile);
                string request = Convert.ToBase64String(Encoding.GetEncoding("utf-8").GetBytes(jsonString));
                SiteHelper.Log(guid, $"{order.Id} base64 encode [after] {request} ", MTPay_logFile);

                Dictionary<string, string> paras = new Dictionary<string, string>
                {
                    { "merchantNo", merchant_no },
                    { "request", request },
                    { "version", "1.0" },
                    { "key", merchant_key }
                };
                string parasData = string.Join("&", paras.Select((m => $"{m.Key}:{m.Value}")));
                SiteHelper.Log(guid, $"{order.Id} md5 [before] {parasData} ", MTPay_logFile);
                var pay_md5sign = MD5Service.Hash(parasData, Encoding.UTF8).ToUpper();
                SiteHelper.Log(guid, $"{order.Id} md5 [after] {pay_md5sign} ", MTPay_logFile);

                paras.Add("signMsg", pay_md5sign);
                paras.Remove("key");
                paras.Remove("request");
                paras.Add("request", HttpUtility.UrlEncode(request));

                //轉換josn
                var data = string.Join("&", paras.Select((m => $"{m.Key}={m.Value}")));
                SiteHelper.Log(guid, $"{order.Id} Request {data} ", MTPay_logFile);

                //呼叫支付平台
                var responseString = APIRequestHelper.RequestApiResult<string>(new RequestParameters
                {
                    BaseUrl = mainUrl,
                    ApiUrl = APIRequestHelper.No_Url_Keyword,
                    Method = HttpMethodType.HttpPost,
                    EncodingType = EncodingType.None,
                    ContentType = "application/x-www-form-urlencoded"
                } , data);

                SiteHelper.Log(guid, string.Format("{0} 支付请求结果 [response] {1}", order.Id, responseString.ResponseText), MTPay_logFile);
                JObject json = JObject.Parse(responseString.ResponseText);

                var version = json["version"] != null ? json["version"].ToString().ToUpper() : string.Empty;    //系统版本号，当前最新版本为 1.0
                var merchantNo = json["merchantNo"] != null ? json["merchantNo"].ToString() : string.Empty;     //商户号，注册完毕后提供，GBP 开头共九码
                var signMsg = json["signMsg"] != null ? json["signMsg"].ToString().ToUpper() : string.Empty;    //MD5 验签，长度为 32，为防止传输过程中被篡改
                var code = json["resCode"] != null ? json["resCode"].ToString() : string.Empty;              //系统回应三位数代码
                var msg = json["resMsg"] != null ? json["resMsg"].ToString() : string.Empty;       //系统回应代码说明，
                var response = json["response"] != null ? json["response"].ToString() : string.Empty;           //请求结果内容

                if (!code.Equals("000"))
                {
                    switch (code)
                    {
                        case "101":
                            outMessage = "API不存在";
                            break;
                        case "202":
                            outMessage = "查无商户号";
                            break;
                        case "203":
                            outMessage = "商户停用中";
                            break;
                        case "204":
                            outMessage = "商户锁定中";
                            break;
                        case "205":
                            outMessage = "商户冻结中";
                            break;
                        case "206":
                            outMessage = "商户配置错误";
                            break;
                        case "301":
                            outMessage = "version 参数错误";
                            break;
                        case "302":
                            outMessage = " merchantNo 参数错误";
                            break;
                        case "303":
                            outMessage = "request 参数错误";
                            break;
                        case "304":
                            outMessage = "signMsg 错误";
                            break;
                        case "401":
                            outMessage = "验签失败";
                            break;
                        case "402":
                            outMessage = "加解密失败";
                            break;
                        case "403":
                            outMessage = "密文格式错误";
                            break;
                        case "404":
                            outMessage = "密文格式拼接错误";
                            break;
                        case "999":
                            outMessage = "系统内部错误";
                            break;
                    }

                    //更新狀態
                    Service.CDFailOrder(order.Id, string.Empty, $"{code} {msg}");
                    SiteHelper.Log(guid, $"{order.Id} 支付请求失敗 [code]{code} [errorMsg]{msg} [descript]{outMessage}", MTPay_logFile);
                }
                else
                {
                    //返回商戶錯誤
                    if (merchantNo != merchant_no)
                    {
                        outMessage = "商户号異常";
                        Service.CDFailOrder(order.Id, string.Empty, $"{code} {msg}");
                        SiteHelper.Log(guid, $"{order.Id} 支付请求回调商户号異常 [code]{code} [outMessage]{outMessage} [errorMsg]{msg} [merchantNo]{merchantNo}", MTPay_logFile);
                    }

                    //驗簽
                    Dictionary<string, string> res = new Dictionary<string, string>
                    {
                        { "merchantNo", merchantNo },
                        { "resCode", code },
                        { "resMsg", msg },
                        { "response", response },
                        { "version", version },
                        { "key", merchant_key }
                    };
                    string resString = string.Join("&", res.Select((m => $"{m.Key}:{m.Value}")));
                    SiteHelper.Log(guid, $"[驗簽] {resString}", MTPay_logFile);
                    var sign = MD5Service.Hash(resString, Encoding.UTF8).ToUpper();
                    if (signMsg != sign)
                    {
                        outMessage = "支付回调非法签名";
                        SiteHelper.Log(guid, $"{order.Id} 支付请求回调非法签名 [code]{code} [outMessage]{outMessage} [errorMsg]{msg} [merchantNo]{merchantNo} [signMsg]{signMsg} [sign]{sign} [key]{merchant_key}", MTPay_logFile);
                        //更新狀態
                        Service.CDFailOrder(order.Id, string.Empty, $"{code} {msg}");
                    }

                    //Base64字串還原
                    string decodeString = Encoding.GetEncoding("utf-8").GetString(Convert.FromBase64String(response));
                    SiteHelper.Log(guid, $"{order.Id} base64 decode [after] {decodeString} ", MTPay_logFile);
                    JObject resJson = JObject.Parse(decodeString);

                    //此笔订单的交易金额
                    var resAmount = resJson["amount"] != null ? resJson["amount"].ToString() : string.Empty;
                    //商户提交的订单号
                    var resOrderNo = resJson["orderNo"] != null ? resJson["orderNo"].ToString() : string.Empty;
                    //此笔订单的我方系统订单号，不会重复
                    var resGbpOrderNo = resJson["gbpOrderNo"] != null ? resJson["gbpOrderNo"].ToString() : string.Empty;
                    //我方系统建立订单时间
                    var resCreateTime = resJson["createTime"] != null ? resJson["createTime"].ToString() : string.Empty;
                    //支付网址，请商户提供给客户进行支付
                    var resResultUrl = resJson["resultUrl"] != null ? resJson["resultUrl"].ToString() : string.Empty;
                    var resResultQRImage = string.Empty;
                    if (bankCode != "UNIONPAY_QR")
                    {
                        //产品类型后有加 QR 或 H5 的类型会回传图片网址
                        resResultQRImage = resJson["resultQRImage"] != null ? resJson["resultQRImage"].ToString() : string.Empty;
                    }
                    //四位数 API 处理代码，请参考 5.2 订单检验回应代码表，0000 代表成功，其他皆为失败
                    var resCode = resJson["code"] != null ? resJson["code"].ToString() : string.Empty;
                    //API 处理代码说明
                    var resMsg = resJson["msg"] != null ? resJson["msg"].ToString() : string.Empty;

                    if (resCode != "0000")
                    {
                        switch (resCode.Substring(0, 2))
                        {
                            case "01":
                                outMessage = "订单建立错误";
                                break;
                            case "20":
                                outMessage = "公对私错误";
                                break;
                            case "21":
                                outMessage = "快捷支付错误";
                                break;
                            case "22":
                                outMessage = "扫码错误";
                                break;
                        }

                        //更新狀態
                        Service.CDFailOrder(order.Id, string.Empty, $"{resCode} {resMsg}");
                        SiteHelper.Log(guid, $"{order.Id} 支付请求订单检验失敗 [Code]{resCode} [errorMsg]{resMsg} [descript]{outMessage}", MTPay_logFile);
                    }
                    else
                    {
                        return_url = string.IsNullOrEmpty(resResultQRImage) ? resResultUrl : resResultQRImage;
                    }
                }
            }
            return return_url;
        }

        /// <summary>
        /// 商户接到通知后回应 OK 将不再通知，如果商户没有回传 OK，将会持续通知 10 次，通知的时间间隔为下列规则
        /// 1    2    3    4     5       6       7     8       9        10
        /// 实时 30秒 30秒 1分钟 1分30秒 2分30秒 4分钟 6分30秒 10分30秒 16分30秒
        /// 
        /// 总历时 44 分钟，超过后放弃通知，请到商户后台手动同步订单，或通知商务协助处理
        /// </summary>
        /// <param name="logGuid"></param>
        /// <returns></returns>
        /// 
        public static PayState Call(string logGuid)
        {
            int orderID = 0;
            try
            {
                Stream stream = HttpContext.Current.Request.InputStream;
                var b = new byte[stream.Length];
                stream.Read(b, 0, (int)stream.Length);
                var responseString = Encoding.UTF8.GetString(b);
                SiteHelper.Log(logGuid, string.Format("支付回调 [FormData] {0}", responseString), MTPay_logFile);

                //签名结果
                var signMsg = HttpContext.Current.Request["signMsg"];

                //版本号
                var version = HttpContext.Current.Request["version"];

                //通知时间
                var notifyTime = HttpContext.Current.Request["notifyTime"];

                //回调內容
                var content = HttpContext.Current.Request["content"];

                //Base64字串還原
                string decodeString = Encoding.GetEncoding("utf-8").GetString(Convert.FromBase64String(content));
                SiteHelper.Log(logGuid, $" base64 decode [after] {decodeString} ", MTPay_logFile);
                JObject data = JObject.Parse(decodeString);

                //服务名称
                var service = data["service"] != null ? data["service"].ToString() : string.Empty;
                //SERVICE 为 QRH5不为空
                var paymentCode = data["paymentCode"] != null ? data["paymentCode"].ToString() : string.Empty;
                //SERVICE 为 QRH5为空
                var bankCode = data["bankCode"] != null ? data["bankCode"].ToString() : string.Empty;
                //SUCCESS: 成功 FAIL: 失败
                var status = data["status"] != null ? data["status"].ToString() : string.Empty;
                //此笔订单的交易金额
                var amount = data["amount"] != null ? data["amount"].ToString() : string.Empty;
                //商户提交的订单号
                var orderNo = data["orderNo"] != null ? data["orderNo"].ToString() : string.Empty;
                //此笔订单的我方系统订单号，不会重复
                var gbpOrderNo = data["gbpOrderNo"] != null ? data["gbpOrderNo"].ToString() : string.Empty;
                //我方系统建立订单时间，時區為 GMT+8
                var createTime = data["createTime"] != null ? data["createTime"].ToString() : string.Empty;

                //透過order id取得UserID, PayTypeID
                int order_id = Convert.ToInt32(orderNo.Replace(MTPay_perfix, ""));
                var orderInfo = Service.CDGetAEPayOrderByID(order_id);

                if (orderInfo.Count == 0)
                {
                    SiteHelper.Log(logGuid, $"{order_id} 支付回调 查無訂單", MTPay_logFile);
                    return PayState.Error;
                }

                var order = orderInfo[0];
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, $"{order.Id} 支付回调 訂單狀態已經完成,無須金幣充值", MTPay_logFile);
                    return PayState.Success;
                }

                //取得Merchant Key
                var platform_id = SiteHelper.GetAPIResult<int>($"/Payment/GetPlatformId?UserID={order.UserID}");
                var url = $"/Payment/GetMerchantUuid?payTypeID={order.PayTypeID}&platformID={platform_id}";
                var pay_list = SiteHelper.GetAPIResult<List<CDMerchantUUID>>(url);
                var infoData = JObject.Parse(pay_list[0].InfoData);
                var merchantKey = infoData["MERCHANT_KEY"] != null ? infoData["MERCHANT_KEY"].ToString() : string.Empty;   //商户MD5密钥

                //驗簽
                Dictionary<string, string> res = new Dictionary<string, string>
                {
                    { "content", content },
                    { "notifyTime", notifyTime },
                    { "version", version },
                    { "key", merchantKey }
                };

                string resString = string.Join("&", res.Select((m => $"{m.Key}:{m.Value}")));
                string sign = MD5Service.Hash(resString, Encoding.UTF8).ToUpper();
                if (signMsg != sign)
                {
                    SiteHelper.Log(logGuid, $"{orderID} 支付回调 非法签名 [merchant_key]{merchantKey} [sign]{sign} [signMsg]{signMsg} [resString]{resString}", MTPay_logFile);
                    return PayState.Error;
                }
                else if (status.ToUpper() != "SUCCESS")
                {
                    SiteHelper.Log(logGuid, $"{order.Id} 支付回调 订单状态失敗 status:{status}", MTPay_logFile);
                    Service.CDFailOrder(order.Id, gbpOrderNo, $"订单状态 {status} ");
                    return PayState.Success;
                }
                else
                {
                    //進行金幣充值
                    string returnMsg = "";
                    if (Convert.ToDouble(amount) == order.Amount)
                    {
                        if (Service.CDFinishAEOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, gbpOrderNo, $"完成支付", ref returnMsg) == true)
                        {
                            SiteHelper.Log(logGuid, $"{order.Id} 支付回调 金币充值成功 merchantOrderId,:{gbpOrderNo}, totalAmount:{amount}", MTPay_logFile);
                            return PayState.Success;
                        }
                        else
                        {
                            SiteHelper.Log(logGuid, $"{order.Id} 支付回调 金币充值错误 {returnMsg}", MSZPay_logFile);
                            Service.CDFailOrder(order.Id, gbpOrderNo, $"{returnMsg}");
                            return PayState.Error;
                        }
                    }

                    SiteHelper.Log(logGuid, string.Format($"{order.Id}  支付回调 支付無效 merchantOrderId:{gbpOrderNo},totalAmount:{amount}"), MTPay_logFile);
                    Service.CDFailOrder(order.Id, gbpOrderNo, $"responseCode:{status}, responseMsg:{status}");
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

                SiteHelper.Log(logGuid, string.Format("支付回调 异常错误 {0}", ex.ToString()), MTPay_logFile);
                return PayState.Error;
            }
        }


        #region Withdraw
        /// <summary>
        /// 代付回調
        /// </summary>
        /// <param name="logGuid"></param>
        /// <returns></returns>
        public static PayState WithdrawCallback(string logGuid)
        {
            try
            {
                Stream stream = HttpContext.Current.Request.InputStream;
                var b = new byte[stream.Length];
                stream.Read(b, 0, (int)stream.Length);
                var responseString = Encoding.UTF8.GetString(b);
                SiteHelper.Log(logGuid, string.Format("代付回调 [FormData] {0}", responseString), MTPayWithdraw_logFile);

                //签名结果
                var signMsg = HttpContext.Current.Request["signMsg"];

                //版本号
                var version = HttpContext.Current.Request["version"];

                //通知时间
                var notifyTime = HttpContext.Current.Request["notifyTime"];

                //回调內容
                var content = HttpContext.Current.Request["content"];

                //Base64字串還原
                string decodeString = Encoding.GetEncoding("utf-8").GetString(Convert.FromBase64String(content));
                SiteHelper.Log(logGuid, $" base64 decode [after] {decodeString} ", MTPayWithdraw_logFile);
                JObject data = JObject.Parse(decodeString);

                //服务名称
                var service = data["service"] != null ? data["service"].ToString() : string.Empty;
                //SERVICE 为 QRH5不为空
                var paymentCode = data["paymentCode"] != null ? data["paymentCode"].ToString() : string.Empty;
                //SERVICE 为 QRH5为空
                var bankCode = data["bankCode"] != null ? data["bankCode"].ToString() : string.Empty;
                //SUCCESS: 成功 FAIL: 失败
                var status = data["status"] != null ? data["status"].ToString() : string.Empty;
                //此笔订单的交易金额
                var amount = data["amount"] != null ? data["amount"].ToString() : string.Empty;
                //商户提交的订单号
                var orderNo = data["orderNo"] != null ? data["orderNo"].ToString() : string.Empty;
                //此笔订单的我方系统订单号，不会重复
                var gbpOrderNo = data["gbpOrderNo"] != null ? data["gbpOrderNo"].ToString() : string.Empty;
                //我方系统建立订单时间，時區為 GMT+8
                var createTime = data["createTime"] != null ? data["createTime"].ToString() : string.Empty;

                //驗簽
                Dictionary<string, string> response = new Dictionary<string, string>
                {
                    { "content", content },
                    { "notifyTime", notifyTime },
                    { "version", version },
                    { "key", MTPay_withdraw_key }
                };

                string resString = string.Join("&", response.Select((m => $"{m.Key}:{m.Value}")));
                string sign = MD5Service.Hash(resString, Encoding.UTF8).ToUpper();
                if (signMsg != sign)
                {
                    SiteHelper.Log(logGuid, $"{orderNo} 提现异步回调非法签名 [merchant_key]{MTPay_withdraw_key} [sign]{sign} [signMsg]{signMsg} [resString]{resString}", MTPayWithdraw_logFile);
                    return PayState.SignError;
                }
                var id = Convert.ToInt32(orderNo);
                var state = (status.ToUpper() == "SUCCESS") ? 4 : 3; //第三方状态(1:已提交, 2:已处理, 3:已退回, 4.已出账)

                var res = Service.WithdrawCallbackByCK(id, state, gbpOrderNo, $"status={status}");
                if (res.Result)
                {
                    SiteHelper.Log(logGuid, $"提现回调成功 order_num={gbpOrderNo}&state={state}&id={id}", MTPayWithdraw_logFile);
                    return PayState.Success;
                }
                else
                {
                    SiteHelper.Log(logGuid, $"提现回调错误 {res.Message}", MTPayWithdraw_logFile);
                    return PayState.Error;
                }
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("提现异步回调异常错误 {0}", ex.ToString()), MTPayWithdraw_logFile);
                return PayState.Error;
            }
        }
        #endregion
    }
}