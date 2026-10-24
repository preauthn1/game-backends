using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using example.invalid;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using game.Infrastructure;
using game.Entity.Payment;
using static game.WebSite.PayConfig;
using game.Infrastructure.Utility.ThirdParty;

namespace game.WebSite.PaymentHelper
{
    /// <summary>
    /// 商易付pay
    /// </summary>
    public class SYFPay
    {
        /// <summary>
        /// LogGuID
        /// </summary>
        private static string Guid;
        public static string Post(CDPayOrder order, string guid)
        {
            Guid = guid;
            string return_url = string.Empty;
            string message;

            //platformID
            var platform_id = SiteHelper.GetAPIResult<int>($"/Payment/GetPlatformId?UserID={order.UserID.ToString()}");
            var pay_list = SiteHelper.GetAPIResult<List<CDMerchantUUID>>($"/Payment/GetMerchantUuid?payTypeID={order.PayTypeID}&platformID={platform_id}");

            if (pay_list.Count() == 0)
            {
                string noListLog = $"{order.Id} 支付请求失敗 [pay list]{pay_list.Count()}";
                SetLog(noListLog);
                message = "支付通道不足";
                Service.CDFailOrder(order.Id, string.Empty, $" {message}");
                return return_url;
            }

            var r = (pay_list.Count() > 1) ? new Random().Next(0, pay_list.Count() - 1) : 0;
            var gwid = pay_list[r].GWID;
            var bankCode = pay_list[r].BankCode;
            var baseUrl = pay_list[r].Url;
            var infoDAtaJson = new JsonStrToJsonObj(pay_list[r].InfoData);

            //將thrid party order no寫入Pay.OrderList.TransID
            Service.CDNewModifyTransID(order.Id, "0", gwid);

            var SYFPayBaseSetting = new SYFPayBaseSetting(infoDAtaJson.GetJsonValueInt("MERCHANT_NO"), baseUrl, infoDAtaJson.GetJsonValueStr("MERCHANT_KEY"));

            var SYFPaySend = new SYFPaySend(SYFPayBaseSetting);
            SYFPaySend.SetPayOrderData(order.Id, order.Amount);
            SYFPaySend.SetPayType(bankCode);
            SYFPaySend.Done();

            string data = SYFPaySend.GetPostDataToUrl();
            string mainUrl = SYFPaySend.GetBaseUrl();

            var responseString = SiteHelper.HttpPost(mainUrl, data);

            var Json = JsonConvert.DeserializeObject<SYFPayResponseJson>(responseString);

            string msg;
            if (Json.status == "success")
            {
                Service.CDModifyTransID(order.Id, SYFPaySend.GetPayOrderId());
                msg = Json.msg;
                message = $"{order.Id} 支付请求成功 [msg]{Json.msg} [data]{Json.data}";
            }
            else
            {
                msg = "目前支付人员太多, 请稍后再试";
                message = $"{order.Id} 支付请求失敗 [msg]{Json.msg} [data]{Json.data}";
            }

            //紀錄log
            //md5所需資料的string
            string SingDataStr = SYFPaySend.GetSingDataStr();
            string md5BeforeLog = $"{order.Id} md5 [before] {SingDataStr}";
            SetLog(md5BeforeLog);

            //md5處理結果
            string SignStr = SYFPaySend.GetMd5Sing();
            string md5AfterLog = $"{order.Id} md5 [after] {SignStr}";
            SetLog(md5AfterLog);

            //請求支付結果
            string responseStringLog = $"{order.Id} 支付请求结果 [response] {responseString}";
            SetLog(responseStringLog);

            //更新狀態
            SetLog(message);

            return msg;
        }

        private static void SetLog(string LogMsg)
        {
            SiteHelper.Log(Guid, LogMsg, SYFPay_logFile);
        }

        public static PayState Call(string logGuid)
        {
            Guid = logGuid;
            int orderID = 0;
            try
            {
                Stream stream = HttpContext.Current.Request.InputStream;
                var b = new byte[stream.Length];
                stream.Read(b, 0, (int)stream.Length);
                var responseString = Encoding.UTF8.GetString(b);

                //LOG
                string responseStringLog = string.Format("支付回调 [FormData] {0}", responseString);
                SetLog(responseStringLog);

                var SYFPayRespose = new SYFPayRespose(responseString);

                //SYFPAY0000...
                string payOrderID = SYFPayRespose.GetPayOderID();
                //取得實際orderID
                orderID = SYFPayRespose.GetOrderID();

                //檢查訂單狀態
                var orderlist = Service.CDGetAEPayOrderByID(Convert.ToInt32(orderID));
                if (orderlist.Count == 0)
                {
                    string LogStr = $"{orderID} 支付回调 查無訂單";
                    SetLog(LogStr);
                    return PayState.Error;
                }

                var order = orderlist[0];
                if (order.Status == CDOrderStatus.Paied)
                {
                    string LogStr = $"{order.Id} 支付回调 訂單狀態已經完成,無須金幣充值";
                    SetLog(LogStr);
                    return PayState.Success;
                }

                if (!SYFPayRespose.IsSuccess())
                {
                    string LogStr = $"{order.Id} 支付回调 订单失敗";
                    SetLog(LogStr);
                    Service.CDFailOrder(order.Id, payOrderID, $"{SYFPayRespose.IsSuccess().ToString()}");
                    return PayState.Error;
                }

                //進行金幣充值
                if(SYFPayRespose.GetPayAmount() != order.Amount)
                {
                    string LogStr = string.Format($"{order.Id}  支付回调 支付無效 merchantOrderId:{payOrderID},totalAmount:{SYFPayRespose.GetPayAmountStr()}");
                    SetLog(LogStr);
                    Service.CDFailOrder(order.Id, payOrderID, $"responseCode:{SYFPayRespose.IsSuccess().ToString()}");
                    return PayState.Error;
                }

                string msg = "";
                var data = $"完成支付";

                if (Service.CDFinishAEOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, payOrderID, data, ref msg))
                {
                    string LogStr = $"{order.Id} 支付回调 金幣充值成功 merchantOrderId,:{payOrderID}, totalAmount:{SYFPayRespose.GetPayAmountStr()}";
                    SetLog(LogStr);
                    return PayState.Success;
                }
                else
                {
                    string LogStr = $"{order.Id} 支付回调 金幣充值错误 {msg}";
                    SetLog(LogStr);
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

                string LogStr = string.Format("支付回调 异常错误 {0}", ex.ToString());
                SetLog(LogStr);
                return PayState.Error;
            }
        }
    }

    public class SYFPayResponseJson
    {
        public string status { get; set; }
        public string msg { get; set; }
        public string[] data { get; set; }

    }

    public class SYFPayBaseSetting
    {
        /*
         * 商户号：10551
         * 网关地址：http://example.invalid/Pay_Index.html
         * 商户APIKEY：aww5166h7g0hlbwnll6q7kgznbx6exz4
         * 
         */

        private int SYFPayID;    //商户号:平台分配商户号
        private string base_url;     //网关地址
        private string key;          //商户APIKEY

        public SYFPayBaseSetting(int payID, string payUrl, string payKey)
        {
            SYFPayID = payID;
            base_url = payUrl + "/";
            key = payKey;
        }

        public int GetID()
        {
            return SYFPayID;
        }

        public string GetBaseUrl(string url)
        {
            if(url.Length == 0)
            {
                return base_url;
            }
            return base_url + url + "/";
        }

        public string GetKey()
        {
            return key;
        }
    }

    public class SYFPaySend
    {
        //以下為必填參數
        private int pay_memberid;    //商户号:平台分配商户号
        private string pay_orderid;     //订单号:上送订单号唯一, 字符长度20
        private DateTime pay_applydate; //提交时间:时间格式：2016-12-26 18:18:18
        private int pay_bankcode;    //银行编码
        private string pay_notifyurl;   //服务端通知:服务端返回地址.（POST返回数据）
        private string pay_callbackurl; //页面跳转通知:页面跳转返回地址（POST返回数据）
        private double pay_amount;         //订单金额
        private string pay_md5sign;     //MD5签名
        private string pay_productname; //商品名称
        private string base_url;        //网关地址
        private string key;          //商户APIKEY
        private Dictionary<string, string> postDate;
        private Dictionary<string, string> md5signDate;
        private string md5signDateStr;
        private string[] sortPostData;
        
        //以下非必填參數
        private string pay_attach;      //附加字段
        private int pay_productnum;     //商户品数量
        private string pay_productdesc; //商品描述
        private string pay_producturl;  //商户链接地址

        public SYFPaySend(SYFPayBaseSetting SYFPayBaseSetting)
        {
            //var SYFPayBaseSetting = new SYFPayBaseSetting(0,"","");
            pay_memberid = SYFPayBaseSetting.GetID();
            base_url = SYFPayBaseSetting.GetBaseUrl("Pay_Index.html"); 
            key = SYFPayBaseSetting.GetKey();

            postDate = new Dictionary<string, string> { };
            md5signDate = new Dictionary<string, string> { };
            pay_productname = "";
            pay_orderid = "";
            pay_applydate = DateTime.Now;
            pay_bankcode = 927;
            pay_notifyurl = SYFPay_NotifyUrl;
            pay_callbackurl = FinishUrl;
            pay_amount = 0.00;
            pay_md5sign = "";
            sortPostData = new string[] { "pay_amount", "pay_applydate", "pay_bankcode", "pay_callbackurl", "pay_memberid", "pay_notifyurl", "pay_orderid", "key" };

            postDate.Add("pay_memberid", pay_memberid.ToString());
            postDate.Add("key", key);
            postDate.Add("pay_applydate", pay_applydate.ToString("yyyy-MM-dd HH:mm:ss"));
            postDate.Add("pay_notifyurl", pay_notifyurl);
            postDate.Add("pay_callbackurl", pay_callbackurl);
        }

        public void SetPayOrderData(int orderID, double orderAmount)
        {
            pay_orderid = SetTradeNo(orderID);
            pay_amount = Math.Round(orderAmount, 2);

            postDate.Add("pay_orderid", pay_orderid);
            postDate.Add("pay_amount", pay_amount.ToString("#0.00"));
        }

        public void SetPayType(string payType)
        {
            pay_productname = payType;
            if (payType == "ZHIFUBAO_QR")
                pay_bankcode = 927;

            if(payType == "ZHIFUBAO_H5")
                pay_bankcode = 926;

            postDate.Add("pay_productname", pay_productname);
            postDate.Add("pay_bankcode", pay_bankcode.ToString());
        }

        private string SetTradeNo(int orderID)
        {
            //訂單編號處理
            var trade_id = "00000000000000000000" + orderID.ToString();   //上送订单号唯一, 字符长度20
            return "SYFPAY" + trade_id.Substring((trade_id.Length - 14));
        }

        private void SortMd5SignDate()
        {
            foreach (string item in sortPostData)
            {
                if (postDate.ContainsKey(item))
                {
                    md5signDate.Add(item, postDate[item]);
                }
            }
            postDate.Remove("key");
        }

        private void SetSign()
        {
            //stringSignTemp="pay_amount=pay_amount&pay_applydate=pay_applydate&pay_bankcode=pay_bankcode&pay_callbackurl=pay_callbackurl&pay_memberid=pay_memberid&pay_notifyurl=pay_notifyurl&pay_orderid=pay_orderid&key=key"  
            //sign =MD5(stringSignTemp).toUpperCase()

            md5signDateStr = SetFieldValueAddUrl(md5signDate);
            pay_md5sign = MD5Service.Hash(md5signDateStr, Encoding.UTF8).ToUpper();
            postDate.Add("pay_md5sign", pay_md5sign);
        }

        private string SetFieldValueAddUrl(Dictionary<string, string> dictionary)
        {
            return string.Join("&", dictionary.Select(m => string.Format("{0}={1}", m.Key, m.Value)));
        }

        public void Done()
        {
            SortMd5SignDate();
            SetSign();
        }

        public Dictionary<string, string> GetSignData()
        {
            return md5signDate;
        }

        public string GetSingDataStr()
        {
            return md5signDateStr;
        }

        public Dictionary<string, string> GetPostData()
        {
            return postDate;
        }

        public string GetPostDataJson()
        {
            return JsonConvert.SerializeObject(postDate);
        }

        public string GetPostDataToUrl()
        {
            return SetFieldValueAddUrl(postDate);
        }

        public string GetBaseUrl()
        {
            return base_url;
        }

        public string GetMd5Sing()
        {
            return pay_md5sign;
        }

        public string GetPayOrderId()
        {
            return pay_orderid;
        }
    }

    public class SYFPayRespose
    {
        private int memberid;
        private string orderid;
        private double amount;
        private string transaction_id;
        private DateTime datetime;
        private string returncode;
        private string attach;
        private string sign;

        public SYFPayRespose(string responseString)
        {
            var GetJsonDAta = new StringToDictionary(responseString, '&');

            returncode = GetJsonDAta.GetValueStr("returncode");
            orderid = GetJsonDAta.GetValueStr("orderid");
            transaction_id = GetJsonDAta.GetValueStr("transaction_id");
            attach = GetJsonDAta.GetValueStr("attach");
            sign = GetJsonDAta.GetValueStr("sign");
            memberid = GetJsonDAta.GetValueInt("memberid");
            amount = GetJsonDAta.GetValueDouble("amount");
            datetime = GetJsonDAta.GetValueDateTime("datetime", "yyyyMMddHHmmss");
        }

        public int GetMemberID()
        {
            return memberid;
        }

        public string GetPayOderID()
        {
            return orderid;
        }

        public double GetPayAmount()
        {
            return amount;
        }

        public string GetTransactionID()
        {
            return transaction_id;
        }

        public DateTime GetDateTime()
        {
            return datetime;
        }

        public string GetReturnCode()
        {
            return returncode;
        }

        public string GetAtrach()
        {
            return attach;
        }

        public string GetSign()
        {
            return sign;
        }

        public bool IsSuccess()
        {
            if (returncode == "00")
                return true;

            return false;
        }

        public int GetOrderID()
        {
            return Convert.ToInt32(orderid.Substring(6));
        }

        public string GetPayAmountStr()
        {
            return Math.Round(amount, 2).ToString();
        }
    }
}