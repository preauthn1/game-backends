using game.Entity.Pay;
using game.Infrastructure;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using example.invalid;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using static game.WebSite.PayConfig;


namespace game.WebSite.PaymentHelper
{
    public class XinQuanFu
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
            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            Dictionary<string, string> paras = new Dictionary<string, string>
            {
                { "api_code", xinquanfu_partner  },
                { "is_type", tradeCode  },
                { "price", (order.Amount.ToString("f2"))  },
                { "order_id", order.Id.ToString()},
                { "time", unixTimestamp.ToString()},
                { "mark", "备注"  },
                { "return_type", "data"  },              
                { "return_url", xinquanfu_shopCallbackUrl  },
                { "notify_url", xinquanfu_shopNotifyUrl  },
               
            };
            string parasData = string.Join("&", paras.OrderBy(x => x.Key).Where(x => !string.IsNullOrEmpty(x.Value)).Select((m => $"{m.Key}={m.Value}")));
            var sign = Md5(parasData + "&key=" + xinquanfu_Key, Encoding.UTF8).ToUpper();
            paras.Add("sign", sign);
            var data = string.Join("&", paras.Select(m => string.Format("{0}={1}", m.Key, m.Value)));

            SiteHelper.Log(guid, $"{order.Id} 支付请求 [data]({xinquanfu_PayURL}) {data}", xinquanfu_logFile);
            var responseString = SiteHelper.HttpPost(xinquanfu_PayURL, data);
            SiteHelper.Log(guid, $"{order.Id} 支付请求回覆 [data] {responseString}", xinquanfu_logFile);
            var url = $"<html><body>" +responseString +"</body></html>";

            var api = Service.CreateRedirectData((int)PayGatewayType.XinQuanFu, url);

            if (api.Result)
                return api.Extend.ToString();
            else
                return string.Empty;

            
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
                    SiteHelper.Log(logGuid, string.Format("新全付支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), xinquanfu_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("新全付支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), xinquanfu_logFile);
                }
                Dictionary<string, string> parameters = null;

                if (!CheckSign(logGuid, out parameters))
                {
                    SiteHelper.Log(logGuid, string.Format("新全付支付回调非法签名"), xinquanfu_logFile);
                    return PayState.SignError;
                }



                //支付状态，00表示支付成功;
                var returncode = parameters.ContainsKey("status") ? parameters["status"] : string.Empty;

                //商家订单号 
                var orderid = parameters.ContainsKey("order_id") ? parameters["order_id"] : string.Empty;

                //订单金额
                var amount = parameters.ContainsKey("price") ? parameters["price"] : string.Empty;

                var trxNo = parameters.ContainsKey("paysapi_id") ? parameters["paysapi_id"] : string.Empty;

                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(orderid));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 新全付支付回调 訂單狀態已經完成,無須金幣充值", order.Id), xinquanfu_logFile);
                    return PayState.Success;
                }

                //進行金幣充值
                string msg = "";

                if (returncode.Equals("1") && ((Convert.ToDecimal(amount)) == order.Amount))
                {
                    var data = $"新全付支付回调 完成支付: 支付订单号: {orderid} , 渠道订单号: {trxNo}, 状态: {returncode}";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, trxNo, data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 新全付支付回调 金幣充值成功 支付订单号:{orderid} 金额:amount={order.Amount} 渠道订单号:{trxNo}", xinquanfu_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 新全付支付回调 金幣充值错误 {msg}", xinquanfu_logFile);

                        Service.CDFailOrder(order.Id, trxNo, $"新全付支付订单号:{orderid} , 渠道订单号: {trxNo} , 支付金额(分): {(Convert.ToInt64(amount) * 100).ToString()}, 状态: {returncode}");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, $"{order.Id}  新全付支付回调 支付無效 支付订单号:{orderid}, 渠道订单号:{trxNo}, 支付金额(分):{(Convert.ToInt64(amount) * 100).ToString()}, 状态:{returncode}", xinquanfu_logFile);
                Service.CDFailOrder(order.Id, trxNo, $"新全付支付订单号:{orderid} , 渠道订单号: {trxNo} , 支付金额(分): {(Convert.ToInt64(amount) * 100).ToString()}, 状态: {returncode}");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("新全付支付回调 异常错误 {0}", ex.ToString()), xinquanfu_logFile);
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
            if (parameters.Count == 0)
            {
                Stream stream = HttpContext.Current.Request.InputStream;
                var b = new byte[stream.Length];
                stream.Read(b, 0, (int)stream.Length);
                var responseString = Encoding.UTF8.GetString(b);
                SiteHelper.Log(guid, $"新全付支付回调串流 DATA={responseString}", xinquanfu_logFile);

                JObject json = JObject.Parse(responseString);

                Dictionary<string, string> paras = new Dictionary<string, string>
                {
                    { "api_code",   json["api_code"] != null ? json["api_code"].ToString() : string.Empty  },
                    { "paysapi_id", json["paysapi_id"] != null ? json["paysapi_id"].ToString() : string.Empty  },
                    { "order_id",   json["order_id"] != null ? json["order_id"].ToString() : string.Empty  },
                    { "is_type",    json["is_type"] != null ? json["is_type"].ToString() : string.Empty },
                    { "price",      json["price"] != null ? json["price"].ToString() : string.Empty },
                    { "real_price", json["real_price"] != null ? json["real_price"].ToString() : string.Empty   },
                    { "mark",       json["mark"] != null ? json["mark"].ToString() : string.Empty   },
                    { "status",     json["status"] != null ? json["status"].ToString() : string.Empty   },
                    { "sign",     json["sign"] != null ? json["sign"].ToString() : string.Empty   },
                };
                parameters = paras.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            }
            var sign = parameters.ContainsKey("sign") ? parameters["sign"] : string.Empty;          
            parameters.Remove("sign");

            SiteHelper.Log(guid, $"新全付支付签名LOG DATA={string.Join("&", parameters.Where(x => !string.IsNullOrEmpty(x.Value)).Select((m => $"{m.Key}={m.Value}"))) + "&paySecret=" + xinquanfu_Key}", xinquanfu_logFile);

            var cSign = Md5(string.Join("&", parameters.Where(x => !string.IsNullOrEmpty(x.Value)).Select((m => $"{m.Key}={m.Value}"))) + "&key=" + xinquanfu_Key, Encoding.UTF8).ToUpper();
            SiteHelper.Log(guid, $"新全付支付回调签名 cSign={cSign} sign={sign}", xinquanfu_logFile);
            return sign == cSign;




        }
    }
}