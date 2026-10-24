using game.Entity.Pay;
using game.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using static game.WebSite.PayConfig;
using Newtonsoft.Json;

namespace game.WebSite.PaymentHelper
{
    public class XingKunYH
    {
        public class XingKunYHResult
        {
            public string return_code { get; set; }
            public string message { get; set; }
            public string out_trade_no { get; set; }
            public string pay_url { get; set; }
        }

        /// <summary>
        /// 创建订单(跳转网页链接)
        /// </summary>
        /// <param name="order"></param>
        /// <param name="guid"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string post(CDPayOrder order, string guid, string tradeCode, ref string message)
        {

            Dictionary<string, string> paras = new Dictionary<string, string>
            {
                { "merchant_no", XingKun_Merchant_No },
                { "pay_num", order.Id.ToString() },
                { "total_fee", order.Amount.ToString("f2") },
                { "pay_method", tradeCode },
                { "merchant_userid", order.UserID.ToString() },
                { "return_url", XingKun_ReturnUrl  },
                { "notify_url", XingKun_YHPay_NotifyUrl  },
                { "return_type", "1"  }        //0:使用表单提交，直接跳转支付页面，1：返回json数据格式
            };

            //sign = MD5(merchant_no+ pay_num + total_fee + today +key)； 其中today = yyyyMMdd，md5 后转大写
            var today = DateTime.Now.ToString("yyyyMMdd");
            var toBeSign = XingKun_Merchant_No + order.Id.ToString() + order.Amount.ToString("f2") + today + XingKun_Key;            
            var sign = Md5(toBeSign, Encoding.UTF8).ToUpper();
            paras.Add("sign", sign);

            var data = string.Join("&", paras.Select(m => string.Format("{0}={1}", m.Key, m.Value)));
            SiteHelper.Log(guid, $"{order.Id} 星坤YH支付请求 [data]({XingKun_XKPay_Url}) {data}", XingKun_YHPay_logFile);
            var responseString = SiteHelper.HttpPost(XingKun_XKPay_Url, data);
            SiteHelper.Log(guid, $"{order.Id} 星坤YH支付请求结果 [response] {responseString}", XingKun_YHPay_logFile);

            var result = JsonConvert.DeserializeObject<XingKunYHResult>(responseString);

            if (!result.return_code.Equals("1"))
            {
                message = $"星坤YH支付请求失敗({result.return_code}) {result.message}";

                //更新狀態
                Service.CDFailOrder(order.Id, string.Empty, message);
                SiteHelper.Log(guid, message, XingKun_YHPay_logFile);

                return String.Empty;
            }


            return result.pay_url;
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
                //成功才返回，不成功不返回
                var requestType = HttpContext.Current.Request.RequestType;
                if (requestType == "GET")
                {
                    SiteHelper.Log(logGuid, string.Format("星坤YH回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), XingKun_YHPay_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("星坤YH回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), XingKun_YHPay_logFile);
                }
                Dictionary<string, string> parameters = null;

                if (!CheckSign(logGuid, out parameters))
                {
                    SiteHelper.Log(logGuid, string.Format("星坤YH回调非法签名"), XingKun_YHPay_logFile);
                    return PayState.SignError;
                }


                var merchant_no = parameters.ContainsKey("merchant_no") ? parameters["merchant_no"] : string.Empty; //商户号
                var pay_num = parameters.ContainsKey("pay_num") ? parameters["pay_num"] : string.Empty;     //上游订单号
                var order_num = parameters.ContainsKey("order_num") ? parameters["order_num"] : string.Empty;  //商户订单号 
                var pay_amount = parameters.ContainsKey("pay_amount") ? parameters["pay_amount"] : string.Empty;    //订单实际付款金额
                var pay_date = parameters.ContainsKey("pay_date") ? parameters["pay_date"] : string.Empty;      //支付时间
                var pay_result = parameters.ContainsKey("pay_result") ? parameters["pay_result"] : string.Empty; //支付结果
                var sign = parameters.ContainsKey("sign") ? parameters["sign"] : string.Empty;          //验证签名

                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(pay_num));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 星坤YH回调 訂單狀態已經完成,無須金幣充值", order.Id), XingKun_YHPay_logFile);
                    return PayState.Success;
                }

                //進行金幣充值
                string msg = "";

                if (((Convert.ToDecimal(pay_amount)) == order.Amount))
                {
                    var data = $"星坤YH回调 完成支付: 支付订单号: {pay_num} , 渠道订单号: {order_num}, 状态: success";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, order_num, data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 星坤YH回调 金幣充值成功 支付订单号:{pay_num} 金额:amount={order.Amount} 渠道订单号:{order_num}", XingKun_YHPay_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 星坤YH回调 金幣充值错误 {msg}", XingKun_YHPay_logFile);

                        Service.CDFailOrder(order.Id, "0", $"星坤YH订单号:{pay_num} , 渠道订单号: {order_num} , 支付金额(分): {(Convert.ToInt64(pay_amount) * 100).ToString()}, 状态: fail");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, $"{order.Id}  星坤YH回调 支付無效 支付订单号:{pay_amount}, 渠道订单号:{order_num}, 支付金额(分):{(Convert.ToInt64(pay_amount) * 100).ToString()}, 状态: fail", XingKun_YHPay_logFile);
                Service.CDFailOrder(order.Id, "0", $"星坤YH订单号:{pay_num} , 渠道订单号: {order_num} , 支付金额(分): {(Convert.ToInt64(pay_amount) * 100).ToString()}, 状态: fail");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("星坤YH回调 异常错误 {0}", ex.ToString()), XingKun_YHPay_logFile);
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

            var merchant_no = parameters.ContainsKey("merchant_no") ? parameters["merchant_no"] : string.Empty;     
            var pay_num = parameters.ContainsKey("pay_num") ? parameters["pay_num"] : string.Empty;
            var order_num = parameters.ContainsKey("order_num") ? parameters["order_num"] : string.Empty;
            var pay_amount = parameters.ContainsKey("pay_amount") ? parameters["pay_amount"] : string.Empty;
            var pay_date = parameters.ContainsKey("pay_date") ? parameters["pay_date"] : string.Empty;
            var pay_result = parameters.ContainsKey("pay_result") ? parameters["pay_result"] : string.Empty;
            var sign = parameters.ContainsKey("sign") ? parameters["sign"] : string.Empty;



            //sign = MD5(merchant_no + pay_num + pay_amount +today + key)；其中today = yyyyMMdd，md5 后转大写.

            var today = DateTime.Now.ToString("yyyyMMdd");

            var data = merchant_no + pay_num + pay_amount + today + XingKun_Key;
            var cSign = Md5(data, Encoding.UTF8).ToUpper();
            SiteHelper.Log(guid, $"星坤YH支付回调签名 data={data} cSign={cSign} sign={sign}", XingKun_YHPay_logFile);
            return sign == cSign;




        }
    }
}
 