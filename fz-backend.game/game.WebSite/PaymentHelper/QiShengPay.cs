using game.Entity.Pay;
using game.Infrastructure;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using static game.WebSite.PayConfig;

namespace game.WebSite.PaymentHelper
{
    public class QiShengPay
    {
        /// <summary>
        /// 创建订单(跳转网页链接)
        /// </summary>
        /// <param name="order"></param>
        /// <param name="guid"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string post(CDPayOrder order, string guid, string tradeCode)
        {
            string d = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Dictionary<string, string> paras = new Dictionary<string, string>
            {
                { "channel_id", QiSheng_Channelid  },
                { "amount", (order.Amount).ToString()  },
                { "out_bill_num", order.Id.ToString()},
                { "pay_trench", tradeCode  },
                { "timestamp", d  },
               

            };
            string parasData = string.Join("&", paras.OrderBy(x => x.Key).Where(x => !string.IsNullOrEmpty(x.Value)).Select((m => $"{m.Key}={m.Value}")));
            var sign = Md5(parasData + QiSheng_Key, Encoding.UTF8).ToLower();
            SiteHelper.Log(guid, string.Format(" 签名参数 [data]({0}) sign={1}", (parasData  + QiSheng_Key), sign), QiSheng_logFile);
            paras.Add("sign", sign);
            var data = string.Join("&", paras.Select(m => string.Format("{0}={1}", m.Key, m.Value)));
            SiteHelper.Log(guid, string.Format("{0} 支付请求 [data]({1}) {2}", order.Id, QiSheng_Url, data), QiSheng_logFile);

            //呼叫支付平台
            var responseString = SiteHelper.HttpPost(QiSheng_Url, data);
            SiteHelper.Log(guid, string.Format("{0} 支付请求结果 [response] {1}", order.Id, responseString), QiSheng_logFile);


            JObject json = JObject.Parse(responseString);
            var result = json["response_code"] != null ? json["response_code"].ToString() : string.Empty;
            var url = string.Empty;
            if (result.Equals("1001"))
            {
                var orderNo = json["response_data"]["bill_num"] != null ? json["response_data"]["bill_num"].ToString() : string.Empty;

                Service.CDModifyTransID(order.Id, orderNo);
                url = json["response_data"]["pay_url"] != null ? json["response_data"]["pay_url"].ToString() : string.Empty;

            }
            

            return url;
        }


        /// <summary>
        /// 支付成功回调
        /// </summary>
        /// <param name="logGuid"></param>
        /// <returns></returns>
        public static PayState call(string logGuid)
        {
            try
            {
                var requestType = HttpContext.Current.Request.RequestType;
                if (requestType == "GET")
                {
                    SiteHelper.Log(logGuid, string.Format("旗胜支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), QiSheng_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("旗胜支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), QiSheng_logFile);
                }
                Dictionary<string, string> parameters = null;

                if (!CheckSign(logGuid, out parameters))
                {
                    SiteHelper.Log(logGuid, string.Format("旗胜支付回调非法签名"), QiSheng_logFile);
                    return PayState.SignError;
                }



                //支付状态，00表示支付成功;
                var returncode = parameters.ContainsKey("pay_status") ? parameters["pay_status"] : string.Empty;

                //商家订单号 
                var orderid = parameters.ContainsKey("out_bill_num") ? parameters["out_bill_num"] : string.Empty;

                //订单金额
                var amount = parameters.ContainsKey("pay_money") ? parameters["pay_money"] : string.Empty;

                var trxNo = parameters.ContainsKey("bill_num") ? parameters["bill_num"] : string.Empty;

                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(orderid));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 旗胜支付回调 訂單狀態已經完成,無須金幣充值", order.Id), QiSheng_logFile);
                    return PayState.Success;
                }

                //進行金幣充值
                string msg = "";

                if (returncode.Equals("1") && ((Convert.ToDecimal(amount)) == order.Amount))
                {
                    var data = $"旗胜支付回调 完成支付: 支付订单号: {orderid} , 渠道订单号: {trxNo}, 状态: {returncode}";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, trxNo, data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 旗胜支付回调 金幣充值成功 支付订单号:{orderid} 金额:amount={order.Amount} 渠道订单号:{trxNo}", QiSheng_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 旗胜支付回调 金幣充值错误 {msg}", QiSheng_logFile);

                        Service.CDFailOrder(order.Id, trxNo, $"旗胜支付订单号:{orderid} , 渠道订单号: {trxNo} , 支付金额(分): {(Convert.ToDecimal(amount)).ToString()}, 状态: {returncode}");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, $"{order.Id}  旗胜支付回调 支付無效 支付订单号:{orderid}, 渠道订单号:{trxNo}, 支付金额(分):{(Convert.ToDecimal(amount)*100).ToString()}, 状态:{returncode}", QiSheng_logFile);
                Service.CDFailOrder(order.Id, trxNo, $"旗胜支付订单号:{orderid} , 渠道订单号: {trxNo} , 支付金额(分): {(Convert.ToDecimal(amount) * 100).ToString()}, 状态: {returncode}");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("旗胜支付回调 异常错误 {0}", ex.ToString()), QiSheng_logFile);
                return PayState.Error;
            }
        }


        private static string Md5(string str, Encoding ec)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            return BitConverter.ToString(md5.ComputeHash(ec.GetBytes(str))).Replace("-", "");
        }

        private static bool CheckSign(string guid, out Dictionary<string, string> parameters)
        {
            var requestType = HttpContext.Current.Request.RequestType;
            if (requestType == "GET")
            {
                parameters = HttpContext.Current.Request.QueryString.AllKeys.Where(x => !string.IsNullOrEmpty(x)).Cast<string>()
                    .ToDictionary(k => k, v => HttpContext.Current.Request[v]).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            }
            else
            {
                parameters = HttpContext.Current.Request.Form.AllKeys.Where(x => !string.IsNullOrEmpty(x)).Cast<string>()
                    .ToDictionary(k => k, v => HttpContext.Current.Request[v]).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            }

            var sign = parameters.ContainsKey("sign") ? parameters["sign"] : string.Empty;
            parameters.Remove("sign");

            var csigndata = string.Join("&", parameters.Where(x => !string.IsNullOrEmpty(x.Value)).Select((m => $"{m.Key}={m.Value}"))) +  QiSheng_Key;
            SiteHelper.Log(guid, $"旗胜支付签名LOG DATA={csigndata}", QiSheng_logFile);
            var cSign = Md5(csigndata, Encoding.UTF8).ToLower();
            SiteHelper.Log(guid, $"旗胜支付回调签名 cSign={cSign} sign={sign}", QiSheng_logFile);
            return sign == cSign;




        }
    }
}