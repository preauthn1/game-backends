using game.Entity.Pay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static game.WebSite.PayConfig;
using Newtonsoft.Json;
using System.Text;
using System.Security.Cryptography;
using game.Infrastructure;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using example.invalid;

namespace game.WebSite.PaymentHelper
{
    public class Zhongbang
    {
        public static string post(CDPayOrder order, string guid, string tradeCode, ref string message)
        {
            string url = string.Empty;
            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            PayapiRequestModel<ApiPaySumitInput> model = new PayapiRequestModel<ApiPaySumitInput>();
            model.mchid = Convert.ToInt32(Zhongbang_mchId);
            model.nonce = new Random().Next(1, 100000).ToString("D6");
            model.timestamp = unixTimestamp.ToString();
            model.data = new ApiPaySumitInput();
            model.data.goodsname = "charge";
            model.data.notify_url = Zhongbang_NotifyUrl;
            model.data.return_url = Zhongbang_CallbackUrl;
            model.data.out_trade_no = order.Id.ToString();
            model.data.paytype = Convert.ToInt32(tradeCode);
            model.data.total_fee = order.Amount.ToString("F2");
            model.data.remark = "";
            model.data.requestip = order.IPAddress;

            Dictionary<string, string> paras = new Dictionary<string, string>();

            paras = JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(model.data));
            paras.Add("timestamp", model.timestamp);
            paras.Add("mchid", model.mchid.ToString());
            paras.Add("nonce", model.nonce);

            var sort = paras.OrderBy(k => k.Key);
            paras = sort.Where(x=>!string.IsNullOrEmpty(x.Value)).ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);
            var md5_data = string.Join("&", paras.Select((m => $"{m.Key}={m.Value}"))) + $"&key={Zhongbang_Key}";
            model.sign = Md5(md5_data, Encoding.UTF8).ToLower();         

            var data = JsonConvert.SerializeObject(model);
            SiteHelper.Log(guid, string.Format("{0} 支付请求 [data]({1}) {2}", order.Id, Zhongbang_Url, data), Zhongbang_logFile);

            //呼叫支付平台
            var responseString = SiteHelper.HttpPost(Zhongbang_Url, data, "application/json");
            SiteHelper.Log(guid, string.Format("{0} 支付请求结果 [response] {1}", order.Id, responseString), Zhongbang_logFile);


            JObject json = JObject.Parse(responseString);
            var retCode = json["error"] != null ? json["error"].ToString() : string.Empty;

            if (!retCode.Equals("0"))
            {
                var respmsg = json["msg"] != null ? json["msg"].ToString() : string.Empty;
                message = string.Format("{0} 支付请求失敗 {1}", order.Id, respmsg);

                //更新狀態
                Service.CDFailOrder(order.Id, string.Empty, respmsg);
                SiteHelper.Log(guid, message, Zhongbang_logFile);
            }
            else
            {
                var trade_no = json["data"]["trade_no"] != null ? json["data"]["trade_no"].ToString() : string.Empty;
                url = json["data"]["payurl"] != null ? json["data"]["payurl"].ToString() : string.Empty;

                Service.CDModifyTransID(order.Id, trade_no);
            }

            return url;
        }

        public static PayState call(string logGuid)
        {
            try
            {
                Stream stream = HttpContext.Current.Request.InputStream;
                var bytes = new byte[stream.Length];
                stream.Read(bytes, 0, (int)stream.Length);
                var responseString = Encoding.UTF8.GetString(bytes);
                var requestType = HttpContext.Current.Request.RequestType;

                SiteHelper.Log(logGuid, $"支付回调 [FormData {requestType}] {responseString}", Zhongbang_logFile);


                Dictionary<string, string> parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);

                if (CheckSign(logGuid, parameters))
                {
                    var orderId = parameters["out_trade_no"];
                    var orderAmount = parameters["total_fee"];
                    var tradeNo = parameters["trade_no"];
                    var paysucessdate = parameters["paysucessdate"];

                    //檢查訂單狀態
                    var order = Service.CDGetPayOrderByID(Convert.ToInt32(orderId));
                    if (order.Status == CDOrderStatus.Paied)
                    {
                        SiteHelper.Log(logGuid, string.Format("訂單狀態已經完成,無須金幣充值"), Zhongbang_logFile);
                        return PayState.Success;
                    }

                    //支付失败
                    if (string.IsNullOrEmpty(paysucessdate))
                    {
                        SiteHelper.Log(logGuid, string.Format("支付失败"), Zhongbang_logFile);
                        Service.CDFailOrder(order.Id, tradeNo, $"支付失败 amt: {orderAmount}");
                        return PayState.Error;
                    }

                    //進行金幣充值
                    string msg = "";

                    if (!string.IsNullOrEmpty(paysucessdate) && order.Amount == Convert.ToInt64(double.Parse(orderAmount)))
                    {
                        var data = $"完成支付: 訂單: {orderId}, amt: {orderAmount}";

                        if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, tradeNo, data, ref msg) == true)
                        {
                            SiteHelper.Log(logGuid, $"金幣充值成功 orderid={order.Id} amount={order.Amount}", Zhongbang_logFile);
                            return PayState.Success;
                        }
                        else
                        {
                            SiteHelper.Log(logGuid, string.Format("金幣充值错误 {0}", msg), Zhongbang_logFile);
                            return PayState.Error;
                        }
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, string.Format("支付無效"), Zhongbang_logFile);
                        Service.CDFailOrder(order.Id, tradeNo, $"支付無效 訂單: {orderId}, 金額: {orderAmount}");
                        return PayState.Error;
                    }
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调非法签名"), Zhongbang_logFile);
                    return PayState.SignError;
                }
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("支付回调异常错误 {0}", ex.ToString()), Zhongbang_logFile);
                return PayState.Error;
            }
        }


        private static bool CheckSign(string guid, Dictionary<string, string> parameters)
        {
            var sign = parameters["sign"];
            parameters.Remove("sign");

            var sort = parameters.Where(x => !string.IsNullOrEmpty(x.Value)).OrderBy(k => k.Key);
            parameters = sort.ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);


            var str = string.Join("&", parameters.Select(m => string.Format("{0}={1}", m.Key, m.Value))) + $"&key={Zhongbang_Key}"; ;
            var cSign = Md5(str, Encoding.UTF8).ToLower();
            SiteHelper.Log(guid, string.Format("支付回调签名 cSign={0} sign={1} srt={2}", cSign, sign, str), Zhongbang_logFile);

            return sign == cSign;
        }

        private static string Md5(string str, Encoding ec)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            return BitConverter.ToString(md5.ComputeHash(ec.GetBytes(str))).Replace("-", "");
        }
    }

    /// <summary>
    /// 支付请求公用参数
    /// </summary>
    [Serializable]
    [DataContract]
    public class PayapiRequestModel
    {
        /// <summary>
        /// 商户ID
        /// </summary>
        [Display(Name = "商户ID")]
        [DataMember]
        public int mchid
        {
            get;
            set;
        }
        /// <summary>
        /// 时间戳(10位秒)
        /// </summary>
        [Display(Name = "时间戳(10位秒)")]
        [DataMember]
        public string timestamp
        {
            get;
            set;
        }
        /// <summary>
        /// 随机码
        /// </summary>
        [DataMember]
        [Display(Name = "随机码")]
        public string nonce
        {
            get;
            set;
        }


        /// <summary>
        /// 签名
        /// </summary>
        [DataMember]
        [Display(Name = "签名")]
        public string sign
        {
            get;
            set;
        }
    }

    /// <summary>
    /// 请求参数
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    [DataContract]
    public class PayapiRequestModel<T> : PayapiRequestModel
    {
        /// <summary>
        /// 数据
        /// </summary>
        [DataMember]
        [Display(Name = "数据")]
        public virtual T data { get; set; }
    }

    /// <summary>
    ///  请求参数
    /// </summary>
    [Serializable]
    [DataContract]
    public class PayapiRequestObjModel : PayapiRequestModel
    {
        /// <summary>
        /// 数据
        /// </summary>
        [DataMember]
        [Display(Name = "数据")]
        public Object data
        {
            get;
            set;
        }
    }

    /// <summary>
	///支付接口提交参数
	/// </summary>
    [Serializable]
    [DataContract]
    public class ApiPaySumitInput
    {
        /////<summary>
        /////商户id
        /////</summary>
        //[DataMember]
        //public int mchid
        //{
        //    get;
        //    set;
        //}
        ///<summary>
        ///支付方式
        ///</summary>
        [DataMember]
        public int paytype
        {
            get;
            set;
        }
        ///<summary>
        ///商户订单号
        ///</summary>
        [DataMember]
        public string out_trade_no
        {
            get;
            set;
        }

        ///<summary>
        ///异步通知地址
        ///</summary>
        [DataMember]
        public string notify_url
        {
            get;
            set;
        }
        ///<summary>
        ///同步通知地址	(可空)
        ///</summary>
        [DataMember]
        public string return_url
        {
            get;
            set;
        }
        ///<summary>
        ///商品名称
        ///</summary>
        [DataMember]
        public string goodsname
        {
            get;
            set;
        }

        ///<summary>
        ///商品金额0.01
        ///</summary>
        [DataMember]
        public string total_fee
        {
            get;
            set;
        }
        ///<summary>
        ///订单说明(可空)
        ///</summary>
        [DataMember]
        public string remark
        {
            get;
            set;
        }
        ///<summary>
        ///终端用户发起请求IP
        ///</summary>
        [DataMember]
        public string requestip
        {
            get;
            set;
        }
    }
}