using game.Entity.Pay;
using game.Infrastructure;
using System;
using System.Collections.Generic;
using example.invalid;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;
using System.Xml.XPath;
using static game.WebSite.PayConfig;

namespace game.WebSite.PaymentHelper
{
    public class wfu
    {
        
        public static string post(CDPayOrder order, string logGuid, string tradeCode, ref string message)
        {
            string interface_version = tradeCode == "direct_pay" ? "V3.0" : "V3.1";
            string service_type = tradeCode;
            string sign_type = PayConfig.Wfu_sign_type;
            string merchant_code = PayConfig.Wfu_merchant_code;
            string order_no = order.Id.ToString();
            string order_time = order.CreateTime.ToString("yyyy-MM-dd HH:mm:ss");
            string order_amount = order.Amount.ToString();
            string product_name = productName;
            string extra_return_param = "pay";
            string notify_url = PayConfig.Wfu_NotifyUrl;

            ////////判斷測試用////////
            string client_ip = order.IPAddress.ToUpper().Contains("192.168.") != true ? order.IPAddress : "0.0.0.0";

            ////////////////组装签名参数//////////////////
            string signStr = "";
            if (client_ip != "")
            {
                signStr = signStr + "client_ip=" + client_ip + "&";
            }
            if (extra_return_param != "")
            {
                signStr = signStr + "extra_return_param=" + extra_return_param + "&";
            }

            if (tradeCode == "direct_pay")
            {
                signStr = signStr + "input_charset=UTF-8&";
            }
            if (interface_version != "")
            {
                signStr = signStr + "interface_version=" + interface_version + "&";
            }
            if (merchant_code != "")
            {
                signStr = signStr + "merchant_code=" + merchant_code + "&";
            }
            if (notify_url != "")
            {
                signStr = signStr + "notify_url=" + notify_url + "&";
            }
            if (order_amount != "")
            {
                signStr = signStr + "order_amount=" + order_amount + "&";
            }
            if (order_no != "")
            {
                signStr = signStr + "order_no=" + order_no + "&";
            }
            if (order_time != "")
            {
                signStr = signStr + "order_time=" + order_time + "&";
            }

            if (product_name != "")
            {
                signStr = signStr + "product_name=" + product_name + "&";
            }
            if (service_type != "")
            {
                signStr = signStr + "service_type=" + service_type;
            }

            //商家私钥
            string merchant_private_key = PayConfig.Wfu_merchant_private_key;

            //私钥转换成C#专用私钥
            merchant_private_key = PayConfig.Wfu_RSAPrivateKeyJava2DotNet(merchant_private_key);
            //签名
            string signData = PayConfig.Wfu_RSASign(signStr, merchant_private_key);
            //将signData进行UrlEncode编码
            signData = HttpUtility.UrlEncode(signData);
            //组装字符串
            string param = signStr + "&sign_type=" + sign_type + "&sign=" + signData;

            //紀錄充值資訊
            SiteHelper.Log(logGuid, string.Format(
                "支付请求 [signStr] {0}{1}" +
                "[signData] {2}", signStr, System.Environment.NewLine, signData), Wfu_logFile);

            string _xml = SiteHelper.HttpPost(PayConfig.Wfu_PayURL, param);

            //紀錄充值結果
            SiteHelper.Log(logGuid, string.Format("支付请求结果 [_xml] {0}", _xml), Wfu_logFile);


            //将同步返回的xml中的参数提取出来
            var el = XElement.Load(new StringReader(_xml));
            var payURL = el.XPathSelectElement("/response/payURL");
            var result_code = el.XPathSelectElement("/response/result_code"); //0:success 1:error 
            if (payURL == null || result_code.Value == "1")
            {
                //取得錯誤說明
                string result_desc = el.XPathSelectElement("/response/result_desc") == null ? _xml : el.XPathSelectElement("/response/result_desc").Value;
                //取得錯誤代碼
                string error_code = el.XPathSelectElement("/response/error_code") == null ? "-" : el.XPathSelectElement("/response/error_code").Value;
                //取得交易編號
                string trade_no = el.XPathSelectElement("/response/trade_no") == null ? "-" : el.XPathSelectElement("/response/trade_no").Value;
                //更新狀態
                Service.CDFailOrder(order.Id, trade_no, error_code);
                message = "支付请求失敗:" + result_desc;
                return String.Empty;
            }
            //去掉首尾的标签并转换成string
            string qrcode = Regex.Match(payURL.ToString(), "(?<=>).*?(?=<)").Value;   //二维码链接
            string returnUrl = HttpContext.Current.Server.UrlDecode(qrcode);
            return returnUrl;
        }
        public static Dictionary<string, string> Wfu_Bank(CDPayOrder order, string logGuid)
        {

            string interface_version = "V3.0";
            string service_type = "direct_pay";
            string pay_type = "b2c";
            string order_no = order.Id.ToString();
            string order_time = order.CreateTime.ToString("yyyy-MM-dd HH:mm:ss");
            string order_amount = order.Amount.ToString();
            string extra_return_param = "pay";
            string client_ip = order.IPAddress;

            Dictionary<string, string> paras = new Dictionary<string, string>();
            paras.Add("interface_version", interface_version);
            paras.Add("service_type", service_type);
            paras.Add("sign_type", PayConfig.Wfu_sign_type);
            paras.Add("merchant_code", PayConfig.Wfu_merchant_code);
            paras.Add("order_no", order_no);
            paras.Add("order_time", order_time);
            paras.Add("order_amount", order_amount);
            paras.Add("product_name", productName);
            paras.Add("extra_return_param", extra_return_param);
            paras.Add("notify_url", PayConfig.Wfu_NotifyUrl);
            paras.Add("client_ip", client_ip);
            paras.Add("input_charset", "UTF-8");
            paras.Add("pay_type", pay_type);

            ////////////////组装签名参数//////////////////
            string signStr = "";
            if (client_ip != "")
            {
                signStr = signStr + "client_ip=" + client_ip + "&";
            }
            if (extra_return_param != "")
            {
                signStr = signStr + "extra_return_param=" + extra_return_param + "&";
            }

            signStr = signStr + "input_charset=UTF-8&";

            if (interface_version != "")
            {
                signStr = signStr + "interface_version=" + interface_version + "&";
            }
            if (PayConfig.Wfu_merchant_code != "")
            {
                signStr = signStr + "merchant_code=" + PayConfig.Wfu_merchant_code + "&";
            }
            if (PayConfig.Wfu_NotifyUrl != "")
            {
                signStr = signStr + "notify_url=" + PayConfig.Wfu_NotifyUrl + "&";
            }
            if (order_amount != "")
            {
                signStr = signStr + "order_amount=" + order_amount + "&";
            }
            if (order_no != "")
            {
                signStr = signStr + "order_no=" + order_no + "&";
            }
            if (order_time != "")
            {
                signStr = signStr + "order_time=" + order_time + "&";
            }
            if (pay_type != "")
            {
                signStr = signStr + "pay_type=" + pay_type + "&";
            }

            if (productName != "")
            {
                signStr = signStr + "product_name=" + productName + "&";
            }
            if (service_type != "")
            {
                signStr = signStr + "service_type=" + service_type;
            }


            //商家私钥
            string merchant_private_key = PayConfig.Wfu_merchant_private_key;

            //私钥转换成C#专用私钥
            merchant_private_key = PayConfig.Wfu_RSAPrivateKeyJava2DotNet(merchant_private_key);
            //签名
            string signData = PayConfig.Wfu_RSASign(signStr, merchant_private_key);

            //紀錄充值資訊
            SiteHelper.Log(logGuid, string.Format(
                "支付資訊 [signStr] {0}{1}" +
                "[signData] {2}", signStr, System.Environment.NewLine, signData), Wfu_logFile);

            paras.Add("sign", signData);

            return paras;
        }

        public static PayState Callback(string logGuid)
        {
            try
            {

                //紀錄充值回調參數
                SiteHelper.Log(logGuid,
                    string.Format("支付回调資料 {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))),
                    Wfu_logFile);


                if (CheckSign_Wfu(logGuid))
                {
                    var order_no = HttpContext.Current.Request["order_no"];
                    var trade_status = HttpContext.Current.Request["trade_status"];
                    var trade_no = HttpContext.Current.Request["trade_no"];

                    //檢查訂單狀態
                    var order = Service.CDGetPayOrderByID(Convert.ToInt32(order_no));
                    if (order.Status == CDOrderStatus.Paied || order.Status == CDOrderStatus.fail)
                    {
                        SiteHelper.Log(logGuid, string.Format("訂單狀態已經完成,無須金幣充值"), Wfu_logFile);
                        return PayState.Success;
                    }

                    
                    if (trade_status == "SUCCESS") //支付成功
                    {
                        string outMsg = string.Empty;
                        if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, trade_no, trade_status, ref outMsg) == true)
                        {
                            SiteHelper.Log(logGuid, string.Format("金幣充值成功 orderid={0} amount={1}", order.Id, order.Amount), Wfu_logFile);
                            return PayState.Success;
                        }
                        else
                        {
                            SiteHelper.Log(logGuid, string.Format("金幣充值错误 {0}", outMsg), Wfu_logFile);
                            return PayState.Error;
                        }
                    }
                    else //支付失敗
                    {

                        SiteHelper.Log(logGuid, string.Format("支付失败"), Wfu_logFile);
                        Service.CDFailOrder(order.Id, trade_no, string.Format("支付失败 trade_status={0}", trade_status));
                        return PayState.Success;
                    }

                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调非法签名"), Wfu_logFile);
                    return PayState.Error;
                }

            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("支付回调异常错误 {0}", ex.ToString()), Wfu_logFile);
                return PayState.Error;
            }

        }

        private static bool CheckSign_Wfu(string logGuid)
        {
            //获取w付反馈信息
            string merchant_code = HttpContext.Current.Request["merchant_code"];
            string notify_type = HttpContext.Current.Request["notify_type"];
            string notify_id = HttpContext.Current.Request["notify_id"];
            string interface_version = HttpContext.Current.Request["interface_version"];
            string sign_type = HttpContext.Current.Request["sign_type"];
            string zhfsign = HttpContext.Current.Request["sign"];
            string order_no = HttpContext.Current.Request["order_no"];
            string order_time = HttpContext.Current.Request["order_time"];
            string order_amount = HttpContext.Current.Request["order_amount"];
            string extra_return_param = HttpContext.Current.Request["extra_return_param"];
            string trade_no = HttpContext.Current.Request["trade_no"];
            string trade_time = HttpContext.Current.Request["trade_time"];
            string trade_status = HttpContext.Current.Request["trade_status"];
            string bank_seq_no = HttpContext.Current.Request["bank_seq_no"];

            /**
             *签名顺序按照参数名a到z的顺序排序，若遇到相同首字母，则看第二个字母，以此类推，
            *参数名1=参数值1&参数名2=参数值2&……&参数名n=参数值n
            **/
            //组织订单信息
            string signStr = "";

            if (null != bank_seq_no && bank_seq_no != "")
            {
                signStr = signStr + "bank_seq_no=" + bank_seq_no.ToString().Trim() + "&";
            }

            if (null != extra_return_param && extra_return_param != "")
            {
                signStr = signStr + "extra_return_param=" + extra_return_param + "&";
            }
            signStr = signStr + "interface_version=V3.0" + "&";
            signStr = signStr + "merchant_code=" + merchant_code + "&";


            if (null != notify_id && notify_id != "")
            {
                signStr = signStr + "notify_id=" + notify_id + "&notify_type=" + notify_type + "&";
            }

            signStr = signStr + "order_amount=" + order_amount + "&";
            signStr = signStr + "order_no=" + order_no + "&";
            signStr = signStr + "order_time=" + order_time + "&";
            signStr = signStr + "trade_no=" + trade_no + "&";
            signStr = signStr + "trade_status=" + trade_status + "&";

            if (null != trade_time && trade_time != "")
            {
                signStr = signStr + "trade_time=" + trade_time;
            }


            SiteHelper.Log(logGuid, string.Format("支付回调签名 zhfsign={0} signStr={1}", zhfsign, signStr), Wfu_logFile);

            /**
            1)zhf_public_key，w付公钥，每个商家对应一个固定的w付公钥（不是使用工具生成的密钥merchant_public_key，不要混淆），
            即为w付商家后台"公钥管理"->"w付公钥"里的绿色字符串内容
            2)demo提供的zhf_public_key是测试商户号123123123000的w付公钥，请自行复制对应商户号的w付公钥进行调整和替换。
            */

            string zhf_public_key = PayConfig.Wfu_public_key;
            //将W付公钥转换成C#专用格式
            zhf_public_key = PayConfig.Wfu_RSAPublicKeyJava2DotNet(zhf_public_key);
            //验签
            bool result = PayConfig.Wfu_ValidateRsaSign(signStr, zhf_public_key, zhfsign);
            return result;
        }

    }
}