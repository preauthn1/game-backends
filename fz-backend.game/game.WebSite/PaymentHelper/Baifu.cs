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

namespace game.WebSite.PaymentHelper
{
    public class Baifu
    {
        public static string post(CDPayOrder order, string guid, string tradeCode, ref string message)
        {
            string url = string.Empty;

            Dictionary<string, string> paras = new Dictionary<string, string>
            {
                { "merchantNo", Baifu_MerchantNo },
                { "netwayCode", tradeCode },
                { "randomNum", new Random().Next(1,1000).ToString("N4") },
                { "orderNum", order.Id.ToString() },
                { "payAmount", (order.Amount * 100).ToString() },
                { "goodsName", "充值" },
                { "callBackUrl", Baifu_NotifyUrl },
                { "frontBackUrl", Baifu_CallbackUrl },
                { "requestIP", order.IPAddress },                
            };

            var sort = paras.OrderBy(k => k.Key);
            paras = sort.ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);
            string parasData = JsonConvert.SerializeObject(paras);
            var pay_md5sign = Md5(parasData + Baifu_Key, Encoding.UTF8).ToUpper();
            paras.Add("sign", pay_md5sign);

            var data = "paramData=" + JsonConvert.SerializeObject(paras); 
            SiteHelper.Log(guid, string.Format("{0} 支付请求 [data]({1}) {2}", order.Id, Baifu_Url, data), Baifu_logFile);

            //呼叫支付平台
            var responseString = SiteHelper.HttpPost(Baifu_Url, data);
            SiteHelper.Log(guid, string.Format("{0} 支付请求结果 [response] {1}", order.Id, responseString), Baifu_logFile);

            JObject json = JObject.Parse(responseString);
            var retCode = json["resultCode"] != null ? json["resultCode"].ToString() : string.Empty;

            if (!retCode.Equals("00"))
            {
                var respmsg = json["resultMsg"] != null ? json["resultMsg"].ToString() : string.Empty;
                message = string.Format("{0} 支付请求失敗 {1}", order.Id, respmsg);

                //更新狀態
                //Service.CDFailOrder(order.Id, string.Empty, respmsg);
                SiteHelper.Log(guid, message, Baifu_logFile);
            }
            else
            {
                url = json["CodeUrl"] != null ? json["CodeUrl"].ToString() : string.Empty;
            }

            return url;
        }

        public static PayState call(string logGuid)
        {
            try
            {
                var requestType = HttpContext.Current.Request.RequestType;
                if (requestType == "GET")
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), Baifu_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), Baifu_logFile);
                }

                Dictionary<string, string> parameters = null;

                if (!CheckSign(logGuid, out parameters))
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调非法签名"), Baifu_logFile);
                    return PayState.SignError;
                }

                var orderNum = parameters.ContainsKey("orderNum") ? parameters["orderNum"] : string.Empty;
                var resultCode = parameters.ContainsKey("resultCode") ? parameters["resultCode"] : string.Empty;
                var amount = parameters.ContainsKey("payAmount") ? parameters["payAmount"] : string.Empty;         


                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(orderNum));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 支付回调 訂單狀態已經完成,無須金幣充值", order.Id), Baifu_logFile);
                    return PayState.Success;
                }

                if (resultCode == "99")
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 支付回调 订单生成: {1}", order.Id, resultCode), Baifu_logFile);
                    return PayState.Error;
                }

                //進行金幣充值
                string msg = "";

                if (resultCode == "00" && (Convert.ToInt64(amount) == order.Amount * 100))
                {
                    var data = $"支付回调 完成支付: 状态: {resultCode}";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, "0", data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, string.Format("{0} 支付回调 金幣充值成功 金额:{1}, 状态:{2}", order.Id, order.Amount, resultCode), Baifu_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, string.Format("{0} 支付回调 金幣充值错误 {1}", order.Id, msg), Baifu_logFile);
                        Service.CDFailOrder(order.Id, "0", $"支付金额: {amount}, 状态: {resultCode}");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, string.Format("{0}  支付回调 支付無效 支付金额(分):{1}, 状态:{2}", order.Id, amount, resultCode), Baifu_logFile);
                Service.CDFailOrder(order.Id, "0", $"支付金额(分): {amount}, 状态: {resultCode}");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("支付回调 异常错误 {0}", ex.ToString()), Baifu_logFile);
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
                parameters = HttpContext.Current.Request.QueryString.AllKeys.Cast<string>()
                    .ToDictionary(k => k, v => HttpContext.Current.Request[v]).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            }
            else
            {
                parameters = HttpContext.Current.Request.Form.AllKeys.Cast<string>()
                    .ToDictionary(k => k, v => HttpContext.Current.Request[v]).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            }

            var paramData = parameters["paramData"];
            parameters = new Dictionary<string, string>();
            JObject json = JObject.Parse(paramData);

            foreach(var jo in json)
            {
                parameters.Add(jo.Key, jo.Value.ToString());
            }

            var sign = parameters["sign"];
            parameters.Remove("sign");

            var sort = parameters.OrderBy(k => k.Key);
            parameters = sort.ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);
            string str = JsonConvert.SerializeObject(parameters) + Baifu_Key;
            var cSign = Md5(str, Encoding.UTF8).ToUpper();
            
            SiteHelper.Log(guid, string.Format("支付回调签名 cSign={0} sign={1} srt={2}", cSign, sign, str), Baifu_logFile);

            return sign == cSign;
        }
    }
}