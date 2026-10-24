using game.Entity.Pay;
using game.Infrastructure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using static game.WebSite.PayConfig;

namespace game.WebSite.PaymentHelper
{
    public class chishrka
    {
        public static string post(CDPayOrder order, string guid, string tradeCode)
        {

            var mch_no = order.Id.ToString();
            var total_fee = (order.Amount * 100).ToString();
            var mch_id = Chishrka_mchid;
            var notify_url = Chishrka_NotifyUrl;
            var format = "HTML";
            var success_url = Chishrka_NotifyUrl;
            var attach = string.Empty;
            var key = Chishrka_Key;

            Dictionary<string, string> param = new Dictionary<string, string>
            {
                {"service",tradeCode},
                {"mch_id",mch_id},
                {"mch_no",mch_no},
                {"total_fee",total_fee},
                {"notify_url",notify_url },
                {"success_url",success_url},
                {"format",format },
                {"nonce_str",random()}
            };
            StringBuilder sb = new StringBuilder();
            ArrayList akeys = new ArrayList(param.Keys);
            akeys.Sort();

            var entries = param.Select(d =>
                string.Format("\"{0}\": [{1}]", d.Key, string.Join(",", d.Value)));

            SiteHelper.Log(guid, string.Format("充值请求: orderid={0}&data={1}", mch_no, "{" + string.Join(",", entries) + "}"), Chishrka_logFile);

            foreach (string k in akeys)
            {
                string v = (string)param[k];
                if (null != v && "".CompareTo(v) != 0
                    && "sign".CompareTo(k) != 0 && "key".CompareTo(k) != 0)
                {
                    sb.Append(k + "=" + v + "&");
                }

            }
            string p = sb.ToString();
            sb.Append("key=" + key);

            MD5CryptoServiceProvider MD5 = new MD5CryptoServiceProvider();
            string sign = BitConverter.ToString(MD5.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()))).Replace("-", "");

            p = p + "&sign=" + sign;
            string submiturl = PayConfig.Chishrka_PayURL + p;

            return submiturl;
        }


        public static PayState call(string guid)
        {

            try
            {
                string key = PayConfig.Chishrka_Key;
                string mch_id = HttpContext.Current.Request.QueryString["mch_id"];
                string mch_no = HttpContext.Current.Request.QueryString["mch_no"];
                string sys_no = HttpContext.Current.Request.QueryString["sys_no"];
                string total_fee = HttpContext.Current.Request.QueryString["total_fee"];
                string pay_state = HttpContext.Current.Request.QueryString["pay_state"];
                string attach = HttpContext.Current.Request.QueryString["attach"];
                string nonce_str = HttpContext.Current.Request.QueryString["nonce_str"];
                string time_end = HttpContext.Current.Request.QueryString["time_end"];


                Dictionary<string, string> param = new Dictionary<string, string>
                {
                    {"mch_id",mch_id},
                    {"mch_no",mch_no},
                    {"sys_no",sys_no},
                    {"total_fee",total_fee },
                    {"pay_state",pay_state},
                    {"attach",attach},
                    {"nonce_str",nonce_str },
                    {"time_end",time_end}
                };

                var entries = param.Select(d =>
                  string.Format("\"{0}\": [{1}]", d.Key, string.Join(",", d.Value)));

                //紀錄callback資料
                SiteHelper.Log(guid,
                     string.Format("支付回调 [FormData] {0}", "{" + string.Join(",", entries) + "}"),
                     Chishrka_logFile);


                //取得訂單資料
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(mch_no));

                //檢查訂單狀態
                if (order.Status == CDOrderStatus.Paied || order.Status == CDOrderStatus.fail)
                {
                    SiteHelper.Log(guid, string.Format("訂單狀態已經結束,無法進行金幣充值"), Chishrka_logFile);
                    return PayState.Success;
                }

                //檢查簽名是否正確
                if (checkSign(key, param))
                {
                    //簽名錯誤
                    SiteHelper.Log(guid, string.Format("支付回调非法签名"), Chishrka_logFile);
                    return PayState.SignError;
                }


                if (pay_state == "1")
                {
                    //進行金幣充值
                    string msg = "";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, sys_no, string.Join(",", entries), ref msg) == true)
                    {
                        SiteHelper.Log(guid, string.Format("進行金幣充值 orderid={0} amount={1}", order.Id, order.Amount), Chishrka_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(guid, string.Format("金幣充值错误 {0}", msg), Chishrka_logFile);
                        return PayState.Error;
                    }
                }
                else
                {
                    //更新訂單狀態
                    Service.CDFailOrder(order.Id, sys_no, "支付平台回覆失敗, " + "{" + string.Join(",", entries) + "}");
                    return PayState.Error;
                }
            }
            catch (Exception ex)
            {
                SiteHelper.Log(guid, string.Format("支付回调异常错误 {0}", ex.ToString()), Chishrka_logFile);
                return PayState.Error;
            }
        }


        private static bool checkSign(string key, Dictionary<string, string> param)
        {
            string sign = HttpContext.Current.Request.QueryString["sign"];

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

        private static string random()
        {
            char[] constant = {'0','1','2','3','4','5','6','7','8','9',
                               'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
                               'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z'};
            StringBuilder sb = new StringBuilder(32);
            Random rd = new Random();
            for (int i = 0; i < 32; i++)
            {
                sb.Append(constant[rd.Next(62)]);
            }
            return sb.ToString();
        }
    }
}