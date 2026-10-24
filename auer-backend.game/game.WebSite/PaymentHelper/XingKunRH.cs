using game.Entity.Pay;
using game.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using static game.WebSite.PayConfig;

namespace game.WebSite.PaymentHelper
{
    public class XingKunRH
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
            Dictionary<string, string> paras = new Dictionary<string, string>
            {
                { "merchant_no", XingKun_Merchant_No },
                { "pay_num", order.Id.ToString() },
                { "total_fee", order.Amount.ToString("f2") },
                { "pay_method", tradeCode },
                { "merchant_userid", order.UserID.ToString() },
                { "return_url", XingKun_ReturnUrl  },
                { "notify_url", XingKun_RHPay_NotifyUrl  },
                { "return_type", "1"  }        //0:使用表单提交，直接跳转支付页面，1：返回json数据格式
            };

            //sign = MD5(merchant_no+ pay_num + total_fee + today +key)； 其中today = yyyyMMdd，md5 后转大写
            var today = DateTime.Now.ToString("yyyyMMdd");
            var toBeSign = XingKun_Merchant_No + order.Id.ToString() + order.Amount.ToString("f2") + today + XingKun_Key;            
            var sign = Md5(toBeSign, Encoding.UTF8).ToUpper();
            paras.Add("sign", sign);
            
            
            var url = $"<html><body><form method='post' name='form' action='{XingKun_RHPay_Url}' enctype=\"application/x-www-form-urlencoded\">" +
                     string.Join("", paras.Select(m => string.Format("<input type='hidden' name='{0}' value='{1}' >", m.Key, m.Value))) +
                     "</form><script>window.onload = function(){document.forms['form'].submit();}</script></body></html>";
 
            SiteHelper.Log(guid, $"{order.Id} 支付请求 [data]({url}) ", XingKun_RHPay_logFile);
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
                //成功才返回，不成功不返回
                var requestType = HttpContext.Current.Request.RequestType;
                if (requestType == "GET")
                {
                    SiteHelper.Log(logGuid, string.Format("星坤RH回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), XingKun_RHPay_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("星坤RH回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), XingKun_RHPay_logFile);
                }
                Dictionary<string, string> parameters = null;

                if (!CheckSign(logGuid, out parameters))
                {
                    SiteHelper.Log(logGuid, string.Format("星坤RH回调非法签名"), XingKun_RHPay_logFile);
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
                    SiteHelper.Log(logGuid, string.Format("{0} 星坤RH回调 訂單狀態已經完成,無須金幣充值", order.Id), XingKun_RHPay_logFile);
                    return PayState.Success;
                }

                //進行金幣充值
                string msg = "";

                if (((Convert.ToDecimal(pay_amount)) == order.Amount))
                {
                    var data = $"星坤RH回调 完成支付: 支付订单号: {pay_num} , 渠道订单号: {order_num}, 状态: success";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, order_num, data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 星坤RH回调 金幣充值成功 支付订单号:{pay_num} 金额:amount={order.Amount} 渠道订单号:{order_num}", XingKun_RHPay_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 星坤RH回调 金幣充值错误 {msg}", XingKun_RHPay_logFile);

                        Service.CDFailOrder(order.Id, "0", $"星坤RH订单号:{pay_num} , 渠道订单号: {order_num} , 支付金额(分): {(Convert.ToInt64(pay_amount) * 100).ToString()}, 状态: fail");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, $"{order.Id}  星坤RH回调 支付無效 支付订单号:{pay_amount}, 渠道订单号:{order_num}, 支付金额(分):{(Convert.ToInt64(pay_amount) * 100).ToString()}, 状态: fail", XingKun_RHPay_logFile);
                Service.CDFailOrder(order.Id, "0", $"星坤RH订单号:{pay_num} , 渠道订单号: {order_num} , 支付金额(分): {(Convert.ToInt64(pay_amount) * 100).ToString()}, 状态: fail");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("星坤RH回调 异常错误 {0}", ex.ToString()), XingKun_RHPay_logFile);
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
            SiteHelper.Log(guid, $"星坤RH支付回调签名 data={data} cSign={cSign} sign={sign}", XingKun_RHPay_logFile);
            return sign == cSign;




        }
    }
}
 