using FW.Core.Helper.Enum;
using game.Entity.Pay;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using example.invalid;
using System.Linq;
using example.invalid;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;
using static game.WebSite.PayConfig;
using static game.WebSite.PaymentHelper.Payment;
using static game.WebSite.PaymentHelper.tungyi;

namespace game.WebSite.PaymentHelper
{
    public class Payment
    {
        public static PayState call(string guid)
        {
            try
            {
                var requestType = HttpContext.Current.Request.RequestType;                
                {
                    SiteHelper.Log(guid, string.Format("支付中心主动回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), Payment_logFile);
                }

                if (TungyiEnable == "1")
                {
                    SiteHelper.SetCache("amountlist", string.Empty, DateTime.Now.AddSeconds(-1));
                    HttpResponse.RemoveOutputCacheItem("/PayCenter/amountlist");


                    Dictionary<string, string> parameters = parameters = HttpContext.Current.Request.Form.AllKeys.Cast<string>()
                        .ToDictionary(k => k, v => HttpContext.Current.Request[v]).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);


                    var listConfig = new List<Config>();

                    foreach (var p in parameters)
                    {
                        MatchCollection matches = Regex.Matches(p.Key, @"(?<=\[)[^[]+(?=\])", RegexOptions.IgnoreCase);

                        if (matches.Count != 2)
                            continue;

                        string index = matches[0].Value;
                        string name = matches[1].Value;
                        bool isAdd = false;
                        var config = listConfig.FirstOrDefault(x => x.Index == index);


                        if (config == null)
                        {
                            config = new Config();
                            config.Index = index;
                            isAdd = true;
                        }

                        switch (name)
                        {
                            case "payType":
                                config.PayType = (TugyiPayType)Convert.ToInt32(p.Value);
                                break;
                            case "amounts":
                                config.Amounts = p.Value;
                                break;
                            case "minAmount":
                                config.MinAmount = p.Value;
                                break;
                            case "maxAmount":
                                config.MaxAmount = p.Value;
                                break;
                        }

                        if(isAdd)
                            listConfig.Add(config);
                    }

                    var content = string.Join(Environment.NewLine, listConfig.Select(x => $"支付类型:{x.PayType.GetDisplayName()}{Environment.NewLine}" + 
                                                                                          $"最低面額:{x.MinAmount}{Environment.NewLine}" + 
                                                                                          $"最高面額:{x.MaxAmount}{Environment.NewLine}" + 
                                                                                          $"支付面額:{x.Amounts}{Environment.NewLine}" + 
                                                                                          $"{new string('=', 30)}"));

                    for (int cid = 0; cid < Chatid.Length; cid++)
                    {
                        var responseString = SiteHelper.HttpPost(SendAlertUrl, $"bottoken=[REDACTED]");
                    }
                }

                return PayState.Success;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(guid, string.Format("支付中心主动回调 异常错误 {0}", ex.ToString()), Payment_logFile);
                return PayState.Error;
            }
        }        
    }

    public class Config
    {
        public string Index { get; set; }
        public TugyiPayType PayType { get; set; }
        public string Amounts { get; set; }
        public string MinAmount { get; set; }
        public string MaxAmount { get; set; }
    }

    public enum TugyiPayType : int
    {
        [EnumDescription("久聊支付")]
        Naychat = 0,
        [EnumDescription("支付宝")]
        Alipay = 1,
        [EnumDescription("微信")]
        Weixin = 2,
        [EnumDescription("QQ")]
        QQ = 3,
        [EnumDescription("銀聯")]
        Bank = 4,
        [EnumDescription("京東支付")]
        JD = 5,
        [EnumDescription("支付宝扫码")]
        AliQRpay = 6,
        [EnumDescription("微信扫码")]
        WeixinQR = 7,
        [EnumDescription("支付宝固额")]
        AliFixedpay = 8,
        [EnumDescription("微信固额")]
        WeixinFixed = 9,
        [EnumDescription("云闪付")]
        CloudPay = 11,
        [EnumDescription("支付宝红包")]
        AlipayEnvelope = 12,
        [EnumDescription("支付宝转帐")]
        AlipayTransfer = 13
    }
}