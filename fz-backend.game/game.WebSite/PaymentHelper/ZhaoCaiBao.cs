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
    public class ZhaoCaiBao
    {
        public class ZhaoCaiBaoResultData
        {
            [JsonProperty("result")]
            public string result { get; set; }

            [JsonProperty("businessnumber")]
            public string businessnumber { get; set; }

            [JsonProperty("status")]
            public string status { get; set; }

            [JsonProperty("transactiondate")]
            public string transactiondate { get; set; }

            [JsonProperty("amount")]
            public string amount { get; set; }

            [JsonProperty("real_amount")]
            public string real_amount { get; set; }

            [JsonProperty("transactiontype")]
            public string transactiontype { get; set; }

            [JsonProperty("inputdate")]
            public string inputdate { get; set; }

            [JsonProperty("trade_qrcode")]
            public string trade_qrcode { get; set; }

            [JsonProperty("remark")]
            public string remark { get; set; }
        }

        public class ZhaoCaiBaoResult
        {
            [JsonProperty("result")]
            public string result { get; set; }

            [JsonProperty("code")]
            public string code { get; set; }

            [JsonProperty("msg")]
            public string msg { get; set; }

            [JsonProperty("data")]
            public ZhaoCaiBaoResultData data { get; set; }
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
                { "ServerUrl", ZhaoCaiBao_NotifyUrl },
                { "amount", (order.Amount*100).ToString() },
                { "backurl", ZhaoCaiBao_ReturnUrl},
                { "businessnumber", order.Id.ToString("0000000000") },
                { "failUrl", ZhaoCaiBao_ReturnUrl },
                { "goodsName", "Deposit" },
                { "mer_id", ZhaoCaiBao_MerID},
                { "terminal", tradeCode  },
                { "timestamp", order.CreateTime.ToString("yyyy-MM-dd HH:mm:ss") },
                { "version", "01" }
            };            
            
            string signString = string.Join("&", paras.Select((kvp) => kvp.Key + "=" + kvp.Value.ToString())) + "&" + ZhaoCaiBao_Key;
            var sign = Md5(signString, Encoding.UTF8).ToUpper();

            paras.Add("sign", sign);
            paras.Add("sign_type", "md5");

            var data = string.Join("&", paras.Select(m => string.Format("{0}={1}", m.Key, m.Value)));
            SiteHelper.Log(guid, $"{order.Id} 招财宝支付请求 [data]({ZhaoCaiBao_Url}) {data}", ZhaoCaiBao_logFile);
            var responseString = SiteHelper.HttpPost(ZhaoCaiBao_Url, data);
            SiteHelper.Log(guid, $"{order.Id} 招财宝支付请求结果 [response] {responseString}", ZhaoCaiBao_logFile);

            var result = JsonConvert.DeserializeObject<ZhaoCaiBaoResult>(responseString);

            if (!result.result.Equals("success"))
            {
                message = $"招财宝支付请求失敗({result.code}) {result.msg}";

                //更新狀態
                Service.CDFailOrder(order.Id, string.Empty, message);
                SiteHelper.Log(guid, message, ZhaoCaiBao_logFile);

                return String.Empty;
            }

            return result.data.trade_qrcode;




            /*
            var url = $"<html><body><form method='post' name='form' action='{ZhaoCaiBao_Url}' enctype=\"application/x-www-form-urlencoded\">" +
                     string.Join("", paras.Select(m => string.Format("<input type='hidden' name='{0}' value='{1}' >", m.Key, m.Value))) +
                     "</form><script>window.onload = function(){document.forms['form'].submit();}</script></body></html>";
 
            //SiteHelper.Log(guid, $"{order.Id} 支付请求 [data]({url}) ", ZhaoCaiBao_logFile);
            return url;
            */
            
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
                    SiteHelper.Log(logGuid, string.Format("招财宝回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), ZhaoCaiBao_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("招财宝回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), ZhaoCaiBao_logFile);
                }
                Dictionary<string, string> parameters = null;

                if (!CheckSign(logGuid, out parameters))
                {
                    SiteHelper.Log(logGuid, string.Format("招财宝回调非法签名"), ZhaoCaiBao_logFile);
                    return PayState.SignError;
                }

                var result = parameters.ContainsKey("result") ? parameters["result"] : string.Empty;                              //返回码
                var code = parameters.ContainsKey("code") ? parameters["code"] : string.Empty;                                    //返回码描述
                var return_msg1 = parameters.ContainsKey("msg") ? parameters["msg"] : string.Empty;                                       //错误描述 
                var mer_id = parameters.ContainsKey("mer_id") ? parameters["mer_id"] : string.Empty;                              //商戶號
                var businessnumber = parameters.ContainsKey("businessnumber") ? parameters["businessnumber"] : string.Empty;      //业务订单号
                var status = parameters.ContainsKey("status") ? parameters["status"] : string.Empty;                              //交易状态
                var transactiondate = parameters.ContainsKey("transactiondate") ? parameters["transactiondate"] : string.Empty;   //交易时间
                var amount = parameters.ContainsKey("amount") ? parameters["amount"] : string.Empty;                              //交易金额
                var real_amount = parameters.ContainsKey("real_amount") ? parameters["real_amount"] : string.Empty;               //實際交易金额
                var transactiontype = parameters.ContainsKey("transactiontype") ? parameters["transactiontype"] : string.Empty;   //交易类型
                var inputdate = parameters.ContainsKey("inputdate") ? parameters["inputdate"] : string.Empty;                     //交易创建时间
                var remark = parameters.ContainsKey("remark") ? parameters["remark"] : string.Empty;                              //结果说明
                var sign = parameters.ContainsKey("sign") ? parameters["sign"] : string.Empty;                                    //签名
                var sign_type = parameters.ContainsKey("sign_type") ? parameters["sign_type"] : string.Empty;                     //签名算法类型

                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(businessnumber));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 招财宝回调 訂單狀態已經完成,無須金幣充值", order.Id), ZhaoCaiBao_logFile);
                    return PayState.Success;
                }

                //進行金幣充值
                string msg = "";

                if (status.ToLower().Equals("成功") && (Convert.ToInt64(amount) == order.Amount * 100))
                {
                    var data = $"招财宝回调 完成支付: 支付订单号: {businessnumber} , 渠道订单号: {"0"}, 状态: success";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, businessnumber, data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 招财宝回调 金幣充值成功 支付订单号:{businessnumber} 金额:amount={order.Amount} 渠道订单号:{"0"}", ZhaoCaiBao_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 招财宝回调 金幣充值错误 {msg}", ZhaoCaiBao_logFile);

                        Service.CDFailOrder(order.Id, "0", $"招财宝订单号:{businessnumber} , 渠道订单号: {"0"} , 支付金额(分): {(Convert.ToInt64(amount) * 100).ToString()}, 状态: fail");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, $"{order.Id}  招财宝回调 支付無效 支付订单号:{businessnumber}, 渠道订单号:{"0"}, 支付金额(分):{(Convert.ToInt64(amount) * 100).ToString()}, 状态: fail", ZhaoCaiBao_logFile);
                Service.CDFailOrder(order.Id, "0", $"招财宝订单号:{businessnumber} , 渠道订单号: {"0"} , 支付金额(分): {(Convert.ToInt64(amount) * 100).ToString()}, 状态: fail");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("招财宝回调 异常错误 {0}", ex.ToString()), ZhaoCaiBao_logFile);
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

            var amount = parameters.ContainsKey("amount") ? parameters["amount"] : string.Empty;                              //交易金额
            var businessnumber = parameters.ContainsKey("businessnumber") ? parameters["businessnumber"] : string.Empty;      //业务订单号
            var code = parameters.ContainsKey("code") ? parameters["code"] : string.Empty;                                    //返回码描述
            var inputdate = parameters.ContainsKey("inputdate") ? parameters["inputdate"] : string.Empty;                     //交易创建时间
            var mer_id = parameters.ContainsKey("mer_id") ? parameters["mer_id"] : string.Empty;                              //商戶號
            var msg = parameters.ContainsKey("msg") ? parameters["msg"] : string.Empty;                                       //错误描述 
            var real_amount = parameters.ContainsKey("real_amount") ? parameters["real_amount"] : string.Empty;               //實際交易金额
            var remark = parameters.ContainsKey("remark") ? parameters["remark"] : string.Empty;                              //结果说明
            var result = parameters.ContainsKey("result") ? parameters["result"] : string.Empty;                              //返回码
            var status = parameters.ContainsKey("status") ? parameters["status"] : string.Empty;                              //交易状态
            var transactiondate = parameters.ContainsKey("transactiondate") ? parameters["transactiondate"] : string.Empty;   //交易时间
            var transactiontype = parameters.ContainsKey("transactiontype") ? parameters["transactiontype"] : string.Empty;   //交易类型
            
            var sign = parameters.ContainsKey("sign") ? parameters["sign"] : string.Empty;                                    //签名
            var sign_type = parameters.ContainsKey("sign_type") ? parameters["sign_type"] : string.Empty;                     //签名算法类型

            var data = $"amount={amount}" +
                $"&businessnumber={businessnumber}" +
                $"&code={code}" +
                $"&inputdate={inputdate}" +
                $"&mer_id={mer_id}" +
                $"&msg={msg}" +
                $"&real_amount={real_amount}" +
                $"&remark={remark}" +
                $"&result={result}" +
                $"&status={status}" +
                $"&transactiondate={transactiondate}" +
                $"&transactiontype={transactiontype}" + "&" + ZhaoCaiBao_Key;
            var cSign = Md5(data, Encoding.UTF8).ToUpper();
            SiteHelper.Log(guid, $"招财宝支付回调签名 data={data} cSign={cSign} sign={sign}", ZhaoCaiBao_logFile);
            return sign == cSign;
        }
    }
}
 