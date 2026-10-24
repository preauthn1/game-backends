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
    public class TianbianPay
    {
        public static string post(CDPayOrder order, string guid, string tradeCode, ref string message)
        {
           
            string XML = @"
             <xml>                
             <notify_url><![CDATA[{0}]]></notify_url> 
             <sign><![CDATA[{1}]]></sign> 
             <pay_type><![CDATA[{5}]]></pay_type> 
             <total_fee><![CDATA[{2}]]></total_fee> 
             <mch_id><![CDATA[{3}]]></mch_id> 
             <out_trade_no><![CDATA[{4}]]></out_trade_no>
             </xml>";
            
            var sign = Md5($"mch_id={tianbianpay_mchid}&notify_url={tianbianpay_shopCallBackUrl}&out_trade_no={order.Id.ToString()}&pay_type={tradeCode}&total_fee={order.Amount.ToString("f2")}&key={tianbianpay_Key}", Encoding.UTF8).ToUpper();
            var _xml = string.Format(XML, tianbianpay_shopCallBackUrl, sign, order.Amount.ToString("f2"), tianbianpay_mchid, order.Id.ToString(), tradeCode);
            SiteHelper.Log(guid, string.Format("支付发送 [xml] {0}", _xml), tianbianpay_logFile);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(tianbianpay_PayURL);
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
                SiteHelper.Log(guid, string.Format("支付发送接收 [xml] {0}", responseStr), tianbianpay_logFile);
                string regex = @"<!\[CDATA\[([^]]+)\]\]>";

                //将同步返回的xml中的参数提取出来
                var el = XElement.Load(new StringReader(responseStr));
                var returncode = el.XPathSelectElement("/return_code");

                if (returncode != null)
                {
                    var msg = el.XPathSelectElement("/return_code");
                    var temp = Regex.Match(msg.ToString(), regex);
                    var rtncode = temp.Groups[1].Value;

                    if (rtncode == "success")
                    {
                        var payurl = el.XPathSelectElement("/pay_url");
                        var temppayurl = Regex.Match(payurl.ToString(), regex);
                        var ricurl = temppayurl.Groups[1].Value;
                        SiteHelper.Log(guid, string.Format("支付发送接收 [url] {0}", ricurl), tianbianpay_logFile);
                        return ricurl;
                    }
                }               
            }
                return string.Empty;
        }
        private static string Md5(string str, Encoding ec)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            return BitConverter.ToString(md5.ComputeHash(ec.GetBytes(str))).Replace("-", "");
        }

        public static PayState call(string logGuid)
        {
            try
            {
                Stream stream = HttpContext.Current.Request.InputStream;
                var b = new byte[stream.Length];
                stream.Read(b, 0, (int)stream.Length);
                var responseString = Encoding.UTF8.GetString(b);
                SiteHelper.Log(logGuid, string.Format("支付回调 [xml] {0}", responseString), tianbianpay_logFile);
                string regex = @"<!\[CDATA\[([^]]+)\]\]>";
                var el = XElement.Load(new StringReader(responseString));
                var returncode = el.XPathSelectElement("/return_code");
                var order_id = string.Empty;
                var orderAmt = string.Empty;
                var outTradeNo = string.Empty;
                var rtncode = string.Empty;
                if (returncode != null)
                {
                    var xmlrtncode = el.XPathSelectElement("/return_code");
                    var temprencode = Regex.Match(xmlrtncode.ToString(), regex);
                     rtncode = temprencode.Groups[1].Value;

                    if (rtncode == "success")
                    {
                        order_id = Regex.Match(el.XPathSelectElement("/out_trade_no").ToString(), regex).Groups[1].Value;
                        orderAmt = Regex.Match(el.XPathSelectElement("/total_fee").ToString(), regex).Groups[1].Value;
                        outTradeNo = Regex.Match(el.XPathSelectElement("/transaction_id").ToString(), regex).Groups[1].Value;
                        
                    }
                   
                }

                if (!checkSign(responseString))
                {
                    SiteHelper.Log(logGuid, string.Format("天边支付回调非法签名"), tianbianpay_logFile);
                    return PayState.SignError;
                }



                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(order_id));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 天边支付回调 訂單狀態已經完成,無須金幣充值", order.Id), tianbianpay_logFile);
                    return PayState.Success;
                }

                //進行金幣充值
                string msg = "";

                if (rtncode.Equals("success") && (Convert.ToDecimal(orderAmt) == order.Amount))
                {
                    var data = $"支付回调 完成支付: 支付订单号: {order_id}  状态: {rtncode}";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, "0", data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 天边支付回调 金幣充值成功 支付订单号:{order_id} 金额:amount={order.Amount} 渠道订单号:{outTradeNo}", tianbianpay_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 天边支付回调 金幣充值错误 {msg}", tianbianpay_logFile);

                        Service.CDFailOrder(order.Id, "0", $"天边支付订单号:{order_id} , 渠道订单号: {outTradeNo} , 支付金额(分): {orderAmt}, 状态: {rtncode}");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, $"{order.Id}  天边支付回调 支付無效 支付订单号:{order_id}, 渠道订单号:{outTradeNo}, 支付金额(分):{orderAmt}, 状态:{rtncode}", tianbianpay_logFile);
                Service.CDFailOrder(order.Id, "0", $"天边支付订单号:{order_id} , 渠道订单号: {outTradeNo} , 支付金额(分): {orderAmt}, 状态: {rtncode}");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("天边支付回调 异常错误 {0}", ex.ToString()), tianbianpay_logFile);
                return PayState.Error;
            }
        }



        private static bool checkSign(string res)
        {
            string regex = @"<!\[CDATA\[([^]]+)\]\]>";
            var el = XElement.Load(new StringReader(res));
            var out_trade_no = Regex.Match(el.XPathSelectElement("/out_trade_no").ToString(), regex).Groups[1].Value;
            var total_fee = Regex.Match(el.XPathSelectElement("/total_fee").ToString(), regex).Groups[1].Value;
            var transaction_id = Regex.Match(el.XPathSelectElement("/transaction_id").ToString(), regex).Groups[1].Value;
            var pay_type = Regex.Match(el.XPathSelectElement("/pay_type").ToString(), regex).Groups[1].Value;
            var mch_id = Regex.Match(el.XPathSelectElement("/mch_id").ToString(), regex).Groups[1].Value;
            var return_msg = Regex.Match(el.XPathSelectElement("/return_msg").ToString(), regex).Groups[1].Value;
            var return_code = Regex.Match(el.XPathSelectElement("/return_code").ToString(), regex).Groups[1].Value;
            var sign = Regex.Match(el.XPathSelectElement("/sign").ToString(), regex).Groups[1].Value;

            var cSign = Md5($"mch_id={mch_id}&out_trade_no={out_trade_no}&pay_type={pay_type}&return_code={return_code}&return_msg={return_msg}&total_fee={total_fee}&transaction_id={transaction_id}&key={tianbianpay_Key}", Encoding.UTF8).ToUpper();
            SiteHelper.Log( $"天边支付回调签名 cSign={cSign} sign={sign}", tianbianpay_logFile);
          
            return cSign == sign;
        }

    }
}