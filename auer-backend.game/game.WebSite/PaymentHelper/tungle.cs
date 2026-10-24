using game.Entity.Pay;
using game.Infrastructure;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using example.invalid;
using System.Linq;
using example.invalid;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;
using System.Xml.XPath;
using static game.WebSite.PayConfig;

namespace game.WebSite.PaymentHelper
{
    public class tungle
    {
        public static string post(CDPayOrder order, string guid, string tradeCode, ref string message)
        {
            var data = new
            {
                mch_app_id = "dayingjia",
                device_info = "AND_WAP",
                ua = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36",
                mch_app_name = "大赢家"
            };

            var user_id = order.UserID.ToString();
            var order_no = order.Id.ToString();
            var total_fee = order.Amount.ToString();
            var user_ip = order.IPAddress;
            var biz_content = HttpContext.Current.Server.UrlEncode(JsonConvert.SerializeObject(data));
            var content = Convert.ToBase64String(Encoding.UTF8.GetBytes(biz_content));

            var nonce_str = Guid.NewGuid().ToString("N");

            Dictionary<string, string> param = new Dictionary<string, string>
            {
                {"version", "1.0" },
                {"charset", "utf-8" },
                {"merchant_id", Tungle_mchid },
                {"out_trade_no", order_no },
                {"trade_type", tradeCode },
                {"user_ip", user_ip },
                {"subject", productName },
                {"body", productName },
                {"user_id", user_id },
                {"total_fee", total_fee },
                {"notify_url", Tungle_NotifyUrl },
                {"nonce_str", nonce_str },
                {"biz_content", content },
            };

            StringBuilder sb = new StringBuilder();
            ArrayList akeys = new ArrayList(param.Keys);
            akeys.Sort();

            var entries = param.Select(d =>
                string.Format("\"{0}\": [{1}]", d.Key, string.Join(",", d.Value)));

            foreach (string k in akeys)
            {
                string v = (string)param[k];
                if (null != v && "".CompareTo(v) != 0
                    && "sign".CompareTo(k) != 0 && "key".CompareTo(k) != 0)
                {
                    sb.Append(k + "=" + v + "&");
                }

            }

            SiteHelper.Log(guid, string.Format("支付请求: orderid={0}&data={1}", order_no, "{" + string.Join(",", entries) + "}"), Tungle_logFile);

            string p = sb.ToString();
            sb.Append("key=" + PayConfig.Tungle_Key);

            MD5CryptoServiceProvider MD5 = new MD5CryptoServiceProvider();
            string sign = BitConverter.ToString(MD5.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()))).Replace("-", "").ToUpper();

            string XML = @"
             <xml>
                <trade_type><![CDATA[{0}]]></trade_type>
                <merchant_id><![CDATA[{1}]]></merchant_id>
                <out_trade_no><![CDATA[{2}]]></out_trade_no>
                <charset><![CDATA[utf-8]]></charset>
                <version><![CDATA[1.0]]></version>
                <nonce_str><![CDATA[{3}]]></nonce_str>
                <user_id><![CDATA[{4}]]></user_id>
                <user_ip><![CDATA[{5}]]></user_ip>
                <total_fee><![CDATA[{6}]]></total_fee>
                <notify_url><![CDATA[{7}]]></notify_url>
                <subject><![CDATA[{8}]]></subject>
                <body><![CDATA[{9}]]></body>
                <biz_content><![CDATA[{10}]]> </biz_content>
                <sign><![CDATA[{11}]]></sign>
            </xml>";


            var _xml = string.Format(XML, tradeCode, Tungle_mchid, order_no, nonce_str, user_id, user_ip, total_fee, Tungle_NotifyUrl, productName, productName, content, sign);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Tungle_PayURL);
            byte[] bytes;
            bytes = Encoding.UTF8.GetBytes(_xml);

            request.ContentType = "text/xml; encoding='utf-8'";
            request.ContentLength = bytes.Length;
            request.Method = "POST";
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(bytes, 0, bytes.Length);
            requestStream.Close();
            HttpWebResponse response;
            response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream responseStream = response.GetResponseStream();
                string responseStr = new StreamReader(responseStream).ReadToEnd();
                string regex = @"<!\[CDATA\[([^]]+)\]\]>";

                //将同步返回的xml中的参数提取出来
                var el = XElement.Load(new StringReader(responseStr));
                var pay_info = el.XPathSelectElement("/pay_info");

                if (pay_info == null)
                {
                    var msg = el.XPathSelectElement("/message");

                    var temp = Regex.Match(msg.ToString(), regex);
                    message = temp.Groups[1].Value;

                    return string.Empty;
                }
                else
                {
                    var temp = Regex.Match(pay_info.ToString(), regex);

                    return temp.Groups[1].Value;
                }
            }
            else
            {
                return string.Empty;
            }
        }

        public static PayState call(string guid)
        {
            try
            {
                string merchant_id = HttpContext.Current.Request["merchant_id"];
                string out_trade_no = HttpContext.Current.Request["out_trade_no"];
                string subject = HttpContext.Current.Request["subject"];
                string body = HttpContext.Current.Request["body"];
                string trade_no = HttpContext.Current.Request["trade_no"];
                string total_fee = HttpContext.Current.Request["total_fee"];
                string trade_time = HttpContext.Current.Request["trade_time"];
                string trade_status = HttpContext.Current.Request["trade_status"];
                string trade_result = HttpContext.Current.Request["trade_result"];

                Dictionary<string, string> param = new Dictionary<string, string>
            {
                {"merchant_id",merchant_id},
                {"out_trade_no",out_trade_no},
                {"subject",subject},
                {"body",body },
                {"trade_no",trade_no},
                {"total_fee",total_fee},
                {"trade_time",trade_time },
                {"trade_result",trade_result},
                {"trade_status",trade_status}
            };

                var entries = param.Select(d =>
                  string.Format("\"{0}\": [{1}]", d.Key, string.Join(",", d.Value)));

                //紀錄callback資料

                SiteHelper.Log(guid,
                     string.Format("支付回调 [FormData] {0}", "{" + string.Join(",", entries) + "}"),
                     Tungle_logFile);

                if (checkSign(Tungle_Key, param))
                {
                    //簽名錯誤
                    SiteHelper.Log(guid, string.Format("支付回调非法签名"), Tungle_logFile);
                    return PayState.SignError;
                }

                //取得訂單資料
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(out_trade_no));

                //檢查訂單狀態
                if (order.Status == CDOrderStatus.Paied || order.Status == CDOrderStatus.fail)
                {
                    SiteHelper.Log(guid, string.Format("訂單狀態已經結束,無法進行金幣充值"), Tungle_logFile);
                    return PayState.Success;
                }

                if (trade_status == "1")
                {
                    //進行金幣充值
                    string msg = "";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, trade_no, string.Join(",", entries), ref msg) == true)
                    {
                        SiteHelper.Log(guid, string.Format("進行金幣充值 orderid={0} amount={1}", order.Id, order.Amount), Tungle_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(guid, string.Format("金幣充值错误 {0}", msg), Tungle_logFile);
                        return PayState.Error;
                    }
                }
                else
                {
                    //更新訂單狀態
                    Service.CDFailOrder(order.Id, trade_no, "支付平台回覆失敗, " + "{" + string.Join(",", entries) + "}");
                    return PayState.Error;
                }
            }
            catch (Exception ex)
            {
                SiteHelper.Log(guid, string.Format("支付回调异常错误 {0}", ex.ToString()), Tungle_logFile);
                return PayState.Error;
            }
        }


        private static bool checkSign(string key, Dictionary<string, string> param)
        {
            string sign = HttpContext.Current.Request["sign"];

            StringBuilder sb = new StringBuilder();
            ArrayList akeys = new ArrayList(param.Keys);
            akeys.Sort();

            foreach (string k in akeys)
            {
                string v = (string)param[k];
                if (null != v && "".CompareTo(v) != 0
                    && "sign".CompareTo(k) != 0 && "key".CompareTo(k) != 0)
                {
                    sb.Append(k + "=" + v + "&");
                }
            }
            sb.Append("key=" + key);

            MD5CryptoServiceProvider MD5 = new MD5CryptoServiceProvider();
            string localsign = BitConverter.ToString(MD5.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()))).Replace("-", "");

            return localsign != sign;
        }

    }
}