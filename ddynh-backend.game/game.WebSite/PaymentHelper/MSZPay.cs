using System;
using example.invalid;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using game.Entity.Payment;
using game.Infrastructure;
using static game.WebSite.PayConfig;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Security.Cryptography.X509Certificates;
using game.Infrastructure.Utility.ThirdParty;

namespace game.WebSite.PaymentHelper
{
    public class MSZPay
    {
        public static string Post(CDPayOrder order, string guid, string tradeCode, ref string outMessage)
        {
            string merchant_url = string.Empty;  //API 网址
            string return_url = string.Empty;

            //platformID
            var platform_id = SiteHelper.GetAPIResult<int>($"/Payment/GetPlatformId?UserID={order.UserID.ToString()}");
            var pay_list = GetMerchantInfo(guid, platform_id, order.PayTypeID);

            //訂單編號處理
            var trade_id = "000000000000" + order.Id.ToString();   //店家自行设定，不得小于 6 个字符，不得重复
            trade_id = MSZPay_Prefix + trade_id.Substring((trade_id.Length - 10));

            if (string.IsNullOrEmpty(pay_list.InfoData))
            {
                SiteHelper.Log(guid, $"{order.Id} 支付请求失敗", MSZPay_logFile);
                outMessage = "支付通道不足";
                Service.CDFailOrder(order.Id, string.Empty, $" {outMessage}");
            }
            else
            {
                var gwid = pay_list.GWID;
                var mainUrl = pay_list.Url;
                var infoData = JObject.Parse(pay_list.InfoData);

                //將thrid party order no寫入Pay.OrderList.TransID
                Service.CDNewModifyTransID(order.Id, trade_id, gwid);

                var merchant_alias = infoData["MERCHANT_ALIAS"] != null ? infoData["MERCHANT_ALIAS"].ToString() : string.Empty;   //商户别名
                var merchant_code = infoData["MERCHANT_CODE"] != null ? infoData["MERCHANT_CODE"].ToString() : string.Empty;      //商户代码
                var secret_key = [REDACTED]"SECRET_KEY"] != null ? infoData["SECRET_KEY"].ToString() : string.Empty;               //加密密钥
                mainUrl += "/" + merchant_alias + "/orders/v3/scan";
                
                Dictionary<string, string> paras = new Dictionary<string, string>
                {
                    { "amount", order.Amount.ToString() },
                    { "merchantCode", merchant_code },
                    { "merchantMemberId", order.UserID.ToString() },    //商户会员编号; 商户网站唯一会员编号
                    { "merchantMemberIp", order.IPAddress },            //商户会员 IP 
                    { "merchantOrderId", trade_id },
                    { "paymentTypeCode", pay_list.TradeCode },
                    { "successUrl", MSZPay_NotifyUrl }
                };

                string parasData = string.Join("|", paras.Select((m => $"{m.Key}={m.Value}"))) + secret_key;

                SiteHelper.Log(guid, $"{order.Id} md5 [before] {parasData} [secret_key]{secret_key}", MSZPay_logFile);
                var pay_md5sign = MD5Service.Hash(parasData, Encoding.UTF8).ToLower();
                SiteHelper.Log(guid, $"{order.Id} md5 [after] {pay_md5sign} ", MSZPay_logFile);

                paras.Add("currencyCode", "CNY");
                paras.Add("mp", $"{platform_id},{order.PayTypeID}");
                paras.Add("sign", pay_md5sign);

                //轉換josn
                var data = JsonConvert.SerializeObject(paras);

                SiteHelper.Log(guid, $"{order.Id} 支付请求內容 [request] {data}", MSZPay_logFile);

                //呼叫支付平台
                var responseString = SiteHelper.HttpPost(mainUrl, data, "application/json");
                SiteHelper.Log(guid, string.Format("{0} 支付请求结果 [response] {1}", order.Id, responseString), MSZPay_logFile);

                JObject json = JObject.Parse(responseString);

                var status = json["result"] != null ? json["result"].ToString() : string.Empty;
                var error_message = json["errorMsg"] != null ? json["errorMsg"].ToString() : string.Empty;

                if (!status.ToUpper().Equals("TRUE"))
                {
                    json = JObject.Parse(error_message);
                    var code = json["code"] != null ? json["code"].ToString() : string.Empty;
                    var errorMsg = json["errorMsg"] != null ? json["errorMsg"].ToString() : string.Empty;
                    var descript = json["descript"] != null ? json["descript"].ToString() : string.Empty;

                    switch (code.ToString())
                    {
                        case "101":
                            outMessage = "数据验证错误";
                            break;
                        case "102":
                            outMessage = "支付通道已关闭";
                            break;
                        case "111":
                            outMessage = "订单号不存在";
                            break;
                        case "112":
                            outMessage = "游戏订单建立失败";
                            break;
                        case "113":
                            outMessage = "游戏订单更新失败";
                            break;
                        case "117":
                            outMessage = "通知二维码异常失败";
                            break;
                        case "131":
                            outMessage = "商户不存在";
                            break;
                        case "202":
                            outMessage = "取得支付通道失败";
                            break;
                        case "301":
                            outMessage = "请求参数验证失败";
                            break;
                        case "302":
                            outMessage = "交易签名无效";
                            break;
                        default:
                            outMessage = "系统错误";
                            break;
                    }

                    //更新狀態
                    Service.CDFailOrder(order.Id, trade_id, $"{status} {outMessage}");
                    SiteHelper.Log(guid, $"{order.Id} 支付请求失敗 [code]{code} [outMessage]{outMessage} [errorMsg]{errorMsg} [descript]{descript}", MSZPay_logFile);
                }
                else
                {
                    var result_data = json["data"] != null ? json["data"].ToString() : string.Empty;
                    json = JObject.Parse(result_data);

                    var gamerOrderId = json["gamerOrderId"] != null ? json["gamerOrderId"].ToString() : string.Empty;
                    var httpUrl = json["httpUrl"] != null ? json["httpUrl"].ToString() : string.Empty;
                    var httpsUrl = json["httpsUrl"] != null ? json["httpsUrl"].ToString() : string.Empty;
                    //var qrCode = json["qrCode"] != null ? json["qrCode"].ToString() : string.Empty;
                    //var remark = json["remark"] != null ? json["remark"].ToString() : string.Empty;
                    var sign = json["sign"] != null ? json["sign"].ToString() : string.Empty;

                    //驗證數據(md5 check)
                    Dictionary<string, string> responseParas = new Dictionary<string, string>
                    {
                        { "gamerOrderId", gamerOrderId },
                        { "httpUrl", httpUrl },
                        { "httpsUrl", httpsUrl }
                    };

                    var md5String = string.Join("|", responseParas.Select(m => $"{m.Key}={m.Value}")) + secret_key;
                    if (!CheckSign(guid, sign, md5String))
                    {
                        outMessage = "支付回调非法签名";
                        SiteHelper.Log(guid, $"{outMessage}", MSZPay_logFile);
                    }

                    return_url = string.IsNullOrEmpty(httpUrl) ? httpsUrl : httpUrl;
                }
            }
            return return_url;
        }

        public static PayState Call(string logGuid)
        {
            int orderID = 0;
            try
            {
                Stream stream = HttpContext.Current.Request.InputStream;
                var b = new byte[stream.Length];
                stream.Read(b, 0, (int)stream.Length);
                var responseString = Encoding.UTF8.GetString(b);
                SiteHelper.Log(logGuid, string.Format("支付回调 [FormData] {0}", responseString), MSZPay_logFile);

                JObject json = JObject.Parse(responseString);

                var gamerOrderId = json["gamerOrderId"] != null ? json["gamerOrderId"].ToString() : string.Empty;
                var merchantOrderId = json["merchantOrderId"] != null ? json["merchantOrderId"].ToString() : string.Empty;
                var currencyCode = json["currencyCode"] != null ? json["currencyCode"].ToString() : string.Empty;
                var paymentTypeCode = json["paymentTypeCode"] != null ? json["paymentTypeCode"].ToString() : string.Empty;
                var amount = json["amount"] != null ? json["amount"].ToString() : string.Empty;
                var remark = json["remark"] != null ? json["remark"].ToString() : string.Empty;
                var status = json["status"] != null ? json["status"].ToString() : string.Empty;
                var mp = json["mp"] != null ? json["mp"].ToString() : string.Empty;
                var sign = json["sign"] != null ? json["sign"].ToString() : string.Empty;

                var platformID = Convert.ToInt32(mp.Split(',')[0]);
                var payTypeID = Convert.ToInt32(mp.Split(',')[1]);
                var payList = GetMerchantInfo(logGuid, platformID, payTypeID);
                var infoData = JObject.Parse(payList.InfoData);
                var secret_key = [REDACTED]"SECRET_KEY"] != null ? infoData["SECRET_KEY"].ToString() : string.Empty;    

                /**
                 * gamerOrderId     游戏平台订单编号  string  游戏平台产生之唯一订单编号
                 * merchantOrderId  商户订单编号 string  商户网站唯一订单编号
                 * currencyCode     币种        string
                 * paymentTypeCode  支付类型别名 string  支付类型编码,详见「支付类型对照表」
                 * amount           交易金额    string  单位;元,精确到分,如;100.25
                 * remark           错误讯息    string  附言,可查询之交易代码
                 * status           错误讯息    string  订单支付状态; Pending;订单已建立  ReadyToPay;等待支付 Confirmed;订单已支付 Success;成功 Failed;失败
                 * mp               扩展讯息    string  返回建立订单时之扩展讯息
                 * sign             错误讯息    string  验证数据,验证方式详见「验证方式」
                 */

                Dictionary<string, string> parameters = new Dictionary<string, string>()
                {
                    { "amount", amount },  //交易金额
                    { "currencyCode", currencyCode },  //币种
                    { "gamerOrderId", gamerOrderId },  //游戏平台订单编号
                    { "merchantOrderId", merchantOrderId },  //商户订单编号
                    { "mp", mp},
                    { "paymentTypeCode", paymentTypeCode },  //支付类型别
                    { "remark", remark },  //附言
                    { "status", status },  //订单状态
                };

                //md5 check

                var md5String = string.Join("|", parameters.Select((m => $"{m.Key}={m.Value}"))) + secret_key;
                if (!CheckSign(logGuid, sign, md5String))
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调非法签名"), MSZPay_logFile);
                    return PayState.SignError;
                }

                //去除MSZPAY前綴後取得實際orderid
                orderID = Convert.ToInt32(merchantOrderId.Replace(MSZPay_Prefix, ""));

                //檢查訂單狀態
                var orderlist = Service.CDGetAEPayOrderByID(Convert.ToInt32(orderID));
                if (orderlist.Count == 0)
                {
                    SiteHelper.Log(logGuid, $"{orderID} 支付回调 查無訂單", MSZPay_logFile);
                    return PayState.Error;
                }
                var order = orderlist[0];
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, $"{order.Id} 支付回调 訂單狀態已經完成,無須金幣充值", MSZPay_logFile);
                    return PayState.Success;
                }

                if (status != "Success")
                {
                    SiteHelper.Log(logGuid, $"{order.Id} 支付回调 订单失敗 responseMsg:{remark}", MSZPay_logFile);
                    Service.CDFailOrder(order.Id, merchantOrderId, $"{status} {remark}");
                    return PayState.Error;
                }

                //進行金幣充值
                string msg = "";

                if (Convert.ToDouble(amount) == order.Amount)
                {
                    var data = $"完成支付";

                    if (Service.CDFinishAEOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, merchantOrderId, data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 支付回调 金幣充值成功 merchantOrderId,:{merchantOrderId}, totalAmount:{amount}", MSZPay_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 支付回调 金幣充值错误 {msg}", MSZPay_logFile);
                        Service.CDFailOrder(order.Id, merchantOrderId, $"{msg}");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, string.Format($"{order.Id}  支付回调 支付無效 merchantOrderId:{merchantOrderId},totalAmount:{amount}"), MSZPay_logFile);
                Service.CDFailOrder(order.Id, merchantOrderId, $"responseCode:{status}, responseMsg:{remark}");
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

                SiteHelper.Log(logGuid, string.Format("支付回调 异常错误 {0}", ex.ToString()), MSZPay_logFile);
                return PayState.Error;
            }
        }

        private static bool CheckSign(string guid, string sign, string md5String)
        {
            var cSign = MD5Service.Hash(md5String, Encoding.UTF8).ToLower();
            SiteHelper.Log(guid, $"支付回调签名 cSign={cSign} sign={sign} srt={md5String}", MSZPay_logFile);

            return sign == cSign;
        }

        private static CDMerchantUUID GetMerchantInfo(string guid, int platformId, int payTypeID)
        {
            var pay_list = SiteHelper.GetAPIResult<List<CDMerchantUUID>>($"/Payment/GetMerchantUuid?payTypeID={payTypeID}&platformID={platformId}");
            if (pay_list.Count() > 1)
            {
                var r = new Random().Next(0, pay_list.Count() - 1);
                return pay_list[r];
            }
            else if (pay_list.Count() == 1)
            {
                return pay_list[0];
            }
            else
            {
                return new CDMerchantUUID();
            }
        }

        #region Withdraw/// <summary>
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
                SiteHelper.Log(logGuid, string.Format("代付回调 [FormData] {0}", responseString), MSZBankWithdraw_logFile);

                JObject json = JObject.Parse(responseString);

                var gamerOrderId = json["gamerOrderId"] != null ? json["gamerOrderId"].ToString() : string.Empty;
                var merchantOrderId = json["merchantOrderId"] != null ? json["merchantOrderId"].ToString() : string.Empty;
                var currencyCode = json["currencyCode"] != null ? json["currencyCode"].ToString() : string.Empty;
                var paymentTypeCode = json["paymentTypeCode"] != null ? json["paymentTypeCode"].ToString() : string.Empty;
                var amount = json["amount"] != null ? json["amount"].ToString() : string.Empty;
                var remark = json["remark"] != null ? json["remark"].ToString() : string.Empty;
                var status = json["status"] != null ? json["status"].ToString() : string.Empty;
                //var mp = (json["mp"] != null ) ? json["mp"].ToString() : "null";
                var sign = json["sign"] != null ? json["sign"].ToString() : string.Empty;

                Dictionary<string, string> parameters = new Dictionary<string, string>()
                {
                    { "amount", amount },  //交易金额
                    { "currencyCode", currencyCode },  //币种
                    { "gamerOrderId", gamerOrderId },  //游戏平台订单编号
                    { "merchantOrderId", merchantOrderId },  //商户订单编号
                    { "mp", "null"},
                    { "paymentTypeCode", paymentTypeCode },  //支付类型别
                    { "remark", remark },  //附言
                    { "status", status },  //订单状态
                };

                //md5 check
                var md5String = string.Join("|", parameters.Select((m => $"{m.Key}={m.Value}"))) + MSZBankWithdraw_Secure_Key;
                if (!CheckWithdrawSign(logGuid, sign, md5String))
                {
                    SiteHelper.Log(logGuid, $"{merchantOrderId} 支付回调非法签名", MSZBankWithdraw_logFile);
                    return PayState.SignError;
                }
                //去除MSZPAY前綴後取得實際orderid
                var id = Convert.ToInt32(merchantOrderId.Replace(MSZBankWithdraw_Prefix, ""));
                var state = (status.ToUpper() == "SUCCESS") ? 4 : 3; //第三方状态(1:已提交, 2:已处理, 3:已退回, 4.已出账)

                var res = Service.WithdrawCallbackByCK(id, state, merchantOrderId, $"status={status}");
                if (res.Result)
                {
                    SiteHelper.Log(logGuid, $"提现回调成功 order_num={merchantOrderId}&state={state}&id={id}", MSZBankWithdraw_logFile);
                    return PayState.Success;
                }
                else
                {
                    SiteHelper.Log(logGuid, $"提现回调错误 {res.Message}", MSZBankWithdraw_logFile);
                    return PayState.Error;
                }
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("提现异步回调异常错误 {0}", ex.ToString()), MTPayWithdraw_logFile);
                return PayState.Error;
            }
        }

        private static bool CheckWithdrawSign(string guid, string sign, string md5String)
        {
            var cSign = MD5Service.Hash(md5String, Encoding.UTF8).ToLower();
            SiteHelper.Log(guid, $"提现回调签名 cSign={cSign} sign={sign} srt={md5String}", MSZBankWithdraw_logFile);
            return sign == cSign;
        }
        #endregion
    }
}