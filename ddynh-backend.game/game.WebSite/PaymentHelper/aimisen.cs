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
    public class aimisen
    {

        public static string post(CDPayOrder order, string logGuid, string tradeCode, ref string message)
        {

            string total_fee = order.Amount.ToString() + "00";
            string out_trade_no = order.Id.ToString();
            string time_start = order.CreateTime.ToString("yyyyMMddHHmmss");
            string finish_url = PayConfig.Aimisen_Finish_Url;

            Dictionary<string, string> paras = new Dictionary<string, string>();

            if (order.PayTypeID == (int)PayType.Bank)
            {
                var bankName = HttpContext.Current.Request["bankName"];
                paras.Add("extend", "{\"bankName\":\"" + bankName + "\",\"cardType\":\"借记卡\"}");
            }

            paras.Add("finish_url", PayConfig.Aimisen_Finish_Url);
            paras.Add("goods_name", productName);
            paras.Add("mchid", PayConfig.Aimisen_mchid);
            paras.Add("out_trade_no", out_trade_no);
            paras.Add("src_code", PayConfig.Aimisen_SrcCode);
            paras.Add("time_start", time_start);
            paras.Add("total_fee", total_fee);
            paras.Add("trade_type", tradeCode);
            paras.Add("key", PayConfig.Aimisen_Key);

            string pay_md5sign = Md5(string.Join("&", paras.Select(m => string.Format("{0}={1}", m.Key, m.Value))), System.Text.Encoding.UTF8);

            paras.Remove("key");
            paras.Add("sign", pay_md5sign);

            var data = string.Join("&", paras.Select(m => string.Format("{0}={1}", m.Key, m.Value)));
            SiteHelper.Log(logGuid, string.Format("支付请求 [data] {0}", data), Aimisen_logFile);

            //呼叫支付平台
            var responseString = SiteHelper.HttpPost(PayConfig.Aimisen_PayURL, data);
            SiteHelper.Log(logGuid, string.Format("支付请求结果 [response] {0}", responseString), Aimisen_logFile);

            JObject json = JObject.Parse(responseString);
            var pay_params = json["data"]["pay_params"] != null ? json["data"]["pay_params"].ToString() : string.Empty;
            var respcd = json["respcd"] != null ? json["respcd"].ToString() : string.Empty;


            if (!respcd.Equals("0000"))
            {
                var respmsg = json["respmsg"] != null ? json["respmsg"].ToString() : string.Empty;
                var trade_no = json["data"]["trade_no"] != null ? json["data"]["trade_no"].ToString() : "0";
                message = string.Format("支付请求失敗({0}) {1}", respcd ,respmsg);

                //更新狀態
                Service.CDFailOrder(order.Id, trade_no, message);
                SiteHelper.Log(logGuid, message, Aimisen_logFile);

                return String.Empty;
            }

            return pay_params;
        }


        public static PayState callback(string logGuid)
        {
            try
            {
                //紀錄充值回調參數
                SiteHelper.Log(logGuid,
                    string.Format("支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))),
                    Aimisen_logFile);

                if (CheckSign_Aimisen(logGuid))
                {
                    var orig_trade_no = HttpContext.Current.Request["orig_trade_no"];
                    var order_status = HttpContext.Current.Request["order_status"]; //1:下单中； 2:等待支付； 3:支付成功； 4:支付失败； 6:用户未支付
                    var trade_no = HttpContext.Current.Request["trade_no"];

                    //檢查訂單狀態
                    var order = Service.CDGetPayOrderByID(Convert.ToInt32(orig_trade_no));
                    if (order.Status == CDOrderStatus.Paied || order.Status == CDOrderStatus.Waiting)
                    {
                        SiteHelper.Log(logGuid, string.Format("訂單狀態已經完成,無須金幣充值"), Aimisen_logFile);
                        return PayState.Success;
                    }


                    //支付進行中
                    if (order_status == "1" || order_status == "2")
                    {
                        SiteHelper.Log(logGuid, string.Format("支付進行中"), Aimisen_logFile);
                        return PayState.Success;
                    }

                    //用户未支付
                    if (order_status == "6")
                    {
                        SiteHelper.Log(logGuid, string.Format("用户未支付"), Aimisen_logFile);
                        Service.CDFailOrder(order.Id, trade_no, string.Format("用户未支付 status={0}", order_status));
                        return PayState.Success;
                    }

                    //支付失败
                    if (order_status == "4")
                    {
                        SiteHelper.Log(logGuid, string.Format("支付失败"), Aimisen_logFile);
                        Service.CDFailOrder(order.Id, trade_no, string.Format("支付失败 status={0}", order_status));
                        return PayState.Success;
                    }

                    //進行金幣充值
                    string msg = "";
                    if (order_status == "3")
                    {
                        if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, trade_no, order_status, ref msg) == true)
                        {
                            SiteHelper.Log(logGuid, string.Format("金幣充值成功 orderid={0} amount={1}", order.Id, order.Amount), Aimisen_logFile);
                            return PayState.Success;
                        }
                        else
                        {
                            SiteHelper.Log(logGuid, string.Format("金幣充值错误 {0}", msg), Aimisen_logFile);
                            return PayState.Error;
                        }
                    }

                    SiteHelper.Log(logGuid, string.Format("支付無效狀態 orderstatus={0}", order_status), Aimisen_logFile);
                    return PayState.Error;
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调非法签名"), Aimisen_logFile);
                    return PayState.Error;
                }
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("支付回调异常错误 {0}", ex.ToString()), Aimisen_logFile);
                return PayState.Error;
            }

        }

        private static bool CheckSign_Aimisen(string guid)
        {
            Dictionary<string, string> parameters = HttpContext.Current.Request.Form.AllKeys.Cast<string>()
                .ToDictionary(k => k, v => HttpContext.Current.Request[v]);

            parameters.Remove("sign");
            parameters.Add("key", PayConfig.Aimisen_Key);

            string sign = HttpContext.Current.Request["sign"];
            string str = string.Join("&", parameters.Select(m => string.Format("{0}={1}", m.Key, m.Value)));

            string cSign = Md5(str, Encoding.UTF8);

            SiteHelper.Log(guid, string.Format("支付回调签名 cSign={0} sign={1}", cSign, sign), Aimisen_logFile);

            return sign == cSign;
        }

        private static string Md5(string str, Encoding ec)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            return BitConverter.ToString(md5.ComputeHash(ec.GetBytes(str))).Replace("-", "");
        }
    }
}