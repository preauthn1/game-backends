using System;
using System.Drawing;
using example.invalid;
using System.Text;
using System.Web.Mvc;
using game.Infrastructure;
using game.WebSite.PaymentHelper;
using ZXing;
using ZXing.QrCode;
using static game.WebSite.PayConfig;

using System.Collections.Generic;
using System.Linq;
using System.Web;
using game.Entity.Payment;
using game.Infrastructure.Utility.ThirdParty;
using game.Entity.DTO.ThirdPartyPay;
using Newtonsoft.Json;

namespace game.WebSite.Controllers
{
    public class PayCenterController : Controller
    {
        string logGuid = Guid.NewGuid().ToString().Substring(0, 5);

        /// <summary>
        /// 開啟瀏覽器跳轉網頁
        /// </summary>
        /// <param name="targetUrl">跳轉網址</param>
        private string GenerateRedirectHtmlOpenBrowser(string targetUrl)
        {
            //目前有兩種狀況會開瀏覽器1.XX.href、2.XX.replace
            //<script>後方的 } 不可刪除，跳轉會失效
            if (targetUrl == "目前支付人员太多, 请稍后再试")
                return targetUrl;

            return "<script>location.href='" + targetUrl + "';}</script>";
        }

        /// <summary>
        /// 開啟瀏覽頁送出表單
        /// </summary>
        /// <param name="formHtml">表單HTML</param>
        private string FormSubmitToRedirectHtmlOpenBrowser(string formHtml)
        {
            string formString = "<html><body>" + formHtml + "</body></html>";
            string AESEncryptString = MyAESEncryption.ByteToHex(MyAESEncryption.AESEncrypt(formString, AESSecretKey)).Replace("-", "");
            string url = PayCenterDoamin + "paycenter/open/?s=" + AESEncryptString;
            return GenerateRedirectHtmlOpenBrowser(url);
        }

        public ActionResult Index(int orderid, string appid = "")
        {
            SetGuidOnRequest(orderid);

            SiteHelper.Log(logGuid, string.Format("充值请求 orderid={0}", orderid), defaultlog);
            if (SiteHelper.PaymentEnable == "0") return Content("");

            //如果cache有資料, 檢查是否是無效查詢
            var cache = SiteHelper.GetCache(orderid.ToString());
            int reqTimes = 0;
            if (cache != null)
            {
                reqTimes = Convert.ToInt32(cache);
                if (reqTimes >= 5)
                {
                    SiteHelper.Log(logGuid, string.Format("充值请求過量 orderid={0}", orderid), defaultlog);
                    return Content("");
                }
                reqTimes++;
            }
            //如果10秒後沒有訪問就過期
            SiteHelper.SetCache(orderid.ToString(), reqTimes, TimeSpan.FromSeconds(10));

            var logMessage = string.Empty;
            var outMessage = string.Empty;
            var showType = 0;
            var returnUrl = string.Empty;
            var tradeCode = string.Empty;

            SiteHelper.Log(logGuid, string.Format("充值请求 orderid={0}", orderid), defaultlog);
            try
            {
                //取得訂單號
                var orderList = Service.CDGetAEPayOrderByID(orderid);
                if (orderList.Count == 0)
                {
                    logMessage = $"订单号无效，订单号 {orderid}";
                    SiteHelper.Log(logGuid, logMessage, defaultlog);
                    return Content(ErrorPage(logMessage));
                }

                var order = orderList[0];
                
                if (!(order != null && order.Id > 0 && order.Status == game.Entity.Payment.CDOrderStatus.Waiting))
                {
                    logMessage = $"订单号无效，订单号 {orderid}";
                    SiteHelper.Log(logGuid, logMessage, defaultlog);
                    return Content(ErrorPage(logMessage));
                }

                //取得第三方支付System
                var payGateway = Service.GetPayGateway(Convert.ToInt32(order.PayTypeID));
                SiteHelper.Log(logGuid, $"充值请求 system={payGateway.Systems} Url={payGateway.Url}", defaultlog);
                switch (payGateway.Systems)
                {
                    case 2: // 第三方支付
                        
                        if (payGateway.GatewayCode == "DYNASPAY")
                        {
                            returnUrl = DynasPay.Post(order, logGuid, tradeCode, ref outMessage);
                        }
                        else if (payGateway.GatewayCode == "MSZPAY")
                        {
                            string returnString = MSZPay.Post(order, logGuid, tradeCode, ref outMessage);
                            if (string.IsNullOrEmpty(outMessage))
                            {
                                returnUrl = "<script>location.href = \"" + returnString + "\";</script>";
                                SiteHelper.Log(logGuid, $"[returnUrl]{returnUrl}", MSZPay_logFile);
                            }
                            else
                            {
                                //支付異常
                                logMessage = "目前支付人员太多, 请稍后再试";
                                return Content(PayGatewayNotFound(logMessage));
                            }
                        }
                        else if(payGateway.GatewayCode == "SYFPAY")
                        {
                            string returnString = SYFPay.Post(order, logGuid);
                            returnUrl = GenerateRedirectHtmlOpenBrowser(returnString);
                            SiteHelper.Log(logGuid, $"[returnUrl]{returnUrl}", SYFPay_logFile);
                        }
                        else if (payGateway.GatewayCode == "NewWeiFuPay")
                        {
                            returnUrl = NewWeiFuPay.Post(order, logGuid);
                        }
                        else if (payGateway.GatewayCode == "GiFuBoPay")
                        {
                            returnUrl = GiFuBoPay.Post(order, logGuid);
                        }
                        else if (payGateway.GatewayCode == "DDPay")
                        {
                            string returnString = DDPay.Post(order, logGuid);
                            returnUrl = GenerateRedirectHtmlOpenBrowser(returnString);
                            SiteHelper.Log(logGuid, $"[returnUrl]{returnUrl}", DDPay_logFile);
                        }
                        else if (payGateway.GatewayCode == "THPay")
                        {
                            returnUrl = THPay.Post(order, logGuid);
                        }
                        else if (payGateway.GatewayCode == "IronPay")
                        {
                            string returnString = IronPay.Post(order, logGuid);
                            returnUrl = GenerateRedirectHtmlOpenBrowser(returnString);
                            SiteHelper.Log(logGuid, $"[returnUrl]{returnUrl}", IronPay_logFile);
                        }
                        else if (payGateway.GatewayCode == "HFZCPay")
                        {
                            var returnModel = HFZCPay.Post(order, logGuid);
                            if (returnModel.Method == "URL")
                            {
                                returnUrl = returnModel.Html;
                            }
                            else
                            {
                                returnUrl = returnModel.Html.Replace("\"", "'");
                            }
                        }
                        else if (payGateway.GatewayCode == "GoldSoarPay")
                        {
                            string returnString = GoldSoarPay.Post(order, logGuid);
                            returnUrl = GenerateRedirectHtmlOpenBrowser(returnString);
                            SiteHelper.Log(logGuid, $"[returnUrl]{returnUrl}", GoldSoarPay_logFile);
                        }
                        else if (payGateway.GatewayCode == "JBPay")
                        {
                            string returnString = JBPay.Post(order, logGuid);
                            returnUrl = GenerateRedirectHtmlOpenBrowser(returnString);
                            SiteHelper.Log(logGuid, $"[returnUrl]{returnUrl}", JBPay_logFile);
                        }
                        else if (payGateway.GatewayCode == "LeHuiPay")
                        {
                            string returnString = LeHuiPay.Post(order, logGuid);
                            returnUrl = FormSubmitToRedirectHtmlOpenBrowser(returnString);
                            SiteHelper.Log(logGuid, $"[returnUrl]{returnUrl}", LeHuiPay_logFile);
                        }
                        else if (payGateway.GatewayCode == "D1FPay")
                        {
                            returnUrl = D1FPay.Post(order, logGuid);
                        }
                        else if (payGateway.GatewayCode == "LiMaFuPay")
                        {
                            string returnString = LiMaFuPay.Post(order, logGuid);
                            returnUrl = GenerateRedirectHtmlOpenBrowser(returnString);
                            SiteHelper.Log(logGuid, $"[returnUrl]{returnUrl}", LiMaFuPay_logFile);
                        }
                        else if (payGateway.GatewayCode == "YungShengPay")
                        {
                            string returnString = YungShengPay.Post(order, logGuid);
                            returnUrl = GenerateRedirectHtmlOpenBrowser(returnString);
                            SiteHelper.Log(logGuid, $"[returnUrl]{returnUrl}", YungShengPay_logFile);
                        }
                        else if (payGateway.GatewayCode == "YongXinPay")
                        {
                            string returnString = YongXinPay.Post(order, logGuid);
                            returnUrl = GenerateRedirectHtmlOpenBrowser(returnString);
                            SiteHelper.Log(logGuid, $"[returnUrl]{returnUrl}", YongXinPay_logFile);
                        }
                        else if (payGateway.GatewayCode == "WpPay")
                        {
                            string returnString = WpPay.Post(order, logGuid, false);
                            returnUrl = GenerateRedirectHtmlOpenBrowser(returnString);
                            SiteHelper.Log(logGuid, $"[returnUrl]{returnUrl}", WpPay_logFile);
                        }
                        else if (payGateway.GatewayCode == "WpPayFixed")
                        {
                            string returnString = WpPay.Post(order, logGuid, true);
                            returnUrl = GenerateRedirectHtmlOpenBrowser(returnString);
                            SiteHelper.Log(logGuid, $"[returnUrl]{returnUrl}", WpPay_logFile);
                        }
                        else if (payGateway.GatewayCode == "DfqjPay")
                        {
                            string returnString = DfqjPay.Post(order, logGuid);
                            returnUrl = GenerateRedirectHtmlOpenBrowser(returnString);
                            SiteHelper.Log(logGuid, $"[returnUrl]{returnUrl}", DfqjPay_logFile);
                        }
                        else if (payGateway.GatewayCode == "SjqpPay")
                        {
                            string returnString = SjqpPay.Post(order, logGuid);
                            returnUrl = GenerateRedirectHtmlOpenBrowser(returnString);
                            SiteHelper.Log(logGuid, $"[returnUrl]{returnUrl}", DfqjPay_logFile);
                        }
                        else if (payGateway.GatewayCode == "SpeedPay")
                        {
                            string returnString = SpeedPay.Post(order, logGuid);
                            returnUrl = GenerateRedirectHtmlOpenBrowser(returnString);
                            SiteHelper.Log(logGuid, $"[returnUrl]{returnUrl}", SpeedPay_logFile);
                        }
                        else if (payGateway.GatewayCode == "SFPay")
                        {
                            returnUrl = SFPay.Post(order, logGuid);
                        }
                        else if (payGateway.GatewayCode == "WonderfulskyPay")
                        {
                            returnUrl = WonderfulskyPay.Post(order, logGuid);
                        }
                        else if (payGateway.GatewayCode == "GoPay")
                        {
                            returnUrl = GoPay.Post(order, logGuid);
                        }
                        else if (payGateway.GatewayCode == "SFTPay")
                        {
                            returnUrl = SFTPay.Post(order, logGuid);
                        }
                        else if (payGateway.GatewayCode == "DSDFPay" || payGateway.GatewayCode == "DSDFPay2")
                        {
                            string returnString = DSDFPay.Post(order, logGuid);
                            SiteHelper.Log(logGuid, $"[returnString]{returnString}", DSDFPay_logFile);
                            
                            returnUrl = FormSubmitToRedirectHtmlOpenBrowser(returnString);
                            SiteHelper.Log(logGuid, $"[returnUrl]{returnUrl}", DSDFPay_logFile);
                        }
                        else if (payGateway.GatewayCode == "MTPay")
                        {
                            string returnString = MTPay.Post(order, logGuid, ref outMessage);
                            if (string.IsNullOrEmpty(outMessage))
                            {
                                returnUrl = "<script>location.href = \"" + returnString + "\";</script>";
                                SiteHelper.Log(logGuid, $"[returnUrl]{returnUrl}", MTPay_logFile);
                            }
                            else
                            {
                                //支付異常
                                logMessage = "目前支付人员太多, 请稍后再试";
                                return Content(PayGatewayNotFound(logMessage));
                            }
                        }
                        else if (payGateway.GatewayCode == "TJPay")
                        {
                            string returnString = TJPay.Post(order, logGuid, ref outMessage);
                            if (string.IsNullOrEmpty(outMessage))
                            {
                                SiteHelper.Log(logGuid, $"[returnString]{returnString}", TJPay_LogFile);
                                returnUrl = FormSubmitToRedirectHtmlOpenBrowser(returnString);
                                SiteHelper.Log(logGuid, $"[returnUrl]{returnUrl}", TJPay_LogFile);
                            }
                            else
                            {
                                //支付異常
                                logMessage = "目前支付人员太多, 请稍后再试";
                                return Content(PayGatewayNotFound(logMessage));
                            }
                        }
                        else if (payGateway.GatewayCode == "LongJuPay")
                        {
                            string returnString = LongJuPay.Post(order, logGuid, tradeCode, ref outMessage);
                            if (!string.IsNullOrEmpty(outMessage))
                            {
                                //支付異常
                                logMessage = "目前支付人员太多, 请稍后再试";
                                return Content(PayGatewayNotFound(logMessage));
                            }
                            returnUrl = FormSubmitToRedirectHtmlOpenBrowser("<script>location.href = \"" + returnString + "\";</script>");
                        }
                        else
                        {
                            returnUrl = "";
                        }

                        string contentHtml = "<!doctype html><html lang='zh-CN'><head><meta charset='UTF-8'></head><body>" + returnUrl + "</body></html>";
                        SiteHelper.Log(logGuid, $"充值请求 returnHtml = {contentHtml}", defaultlog);
                        return Content(contentHtml);

                    case 1: // AE Pay Center
                    default:
                        returnUrl = AEPay.Post(order, logGuid, tradeCode, ref outMessage, ref showType);

                        //顯示錯誤訊息
                        if (returnUrl.Length == 0)
                        {
                            logMessage = "支付通道不足";
                            SiteHelper.Log(logGuid, outMessage, defaultlog);
                            return Content(PayGatewayNotFound(logMessage));
                        }

                        //跳轉支付平台
                        switch (showType)
                        {
                            case 1: //回傳交易網址，介接方需要將此網址轉為QR code
                                //支付方式 (名字带有【扫码】【二维码】的方式，返回的payUrl鏈接要自已主动生成二维码！)
                                var writer = new BarcodeWriter
                                {
                                    Format = BarcodeFormat.QR_CODE,
                                    Options = new QrCodeEncodingOptions
                                    {
                                        Height = 300,
                                        Width = 300,
                                        Margin = 1
                                    }
                                };
                                Bitmap m_Bitmap = writer.Write(returnUrl);
                                MemoryStream ms = new MemoryStream();
                                m_Bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                                byte[] buff = ms.ToArray();
                                return Content(string.Format("<html><body><img src='data:image/jpeg;base64,{0}'style='margin: 0 auto; display: block;' /></body></html>", Convert.ToBase64String(buff)));

                            case 2:  //回傳帶有參數資訊(gatewayParam)讓介接方跳轉
                                if (payGateway.Id == 13) //悍付需要開瀏覽器跳轉
                                {
                                    var str = "<html><body>" + returnUrl + "<script>window.onload = function(){document.forms['form'].submit();}</script></body></html>";
                                    string s = MyAESEncryption.ByteToHex(MyAESEncryption.AESEncrypt(str, "V1M5WE7X8WE2TWEK")).Replace("-", "");
                                    return Content("<!doctype html><html lang='zh-TW'><head><meta charset='UTF-8'></head><body><script>location.href='http://example.invalid/paycenter/open/?s=" + s + "';}</script></body></html>");
                                }
                                return Content("<html><body>" + returnUrl + "<script>window.onload = function(){document.forms['form'].submit();}</script></body></html>");

                            case 3:  //回傳圖片網址進行掃碼交易
                                return Content("<html><body><div style='text-align:center;'><img src='" + returnUrl + "' style='max-height:250px;' /></div></body></html>");

                            case 4:  //回傳網頁內容，介接方直接顯示該內容
                                return Content(returnUrl);

                            case 5:  //回傳帶有參數資訊(gatewayParam)讓介接方跳轉 GET
                                return Content("<html><body><script>location.href='" + returnUrl + "';}</script></body></html>");

                            default:
                                logMessage = "目前支付人员太多, 请稍后再试";
                                return Content(PayGatewayNotFound(returnUrl));
                        }
                }
            }
            catch (Exception ex)
            {
                string msg = $"[orderID]{orderid}[LogID]{logGuid}[message]{ex.ToString()}";

                MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
                {
                    Channel = SlackChannelEnum.n_api_payment,
                    Message = msg
                });

                string LogMessage = string.Format("系統錯誤 {0}", ex.ToString());
                SiteHelper.Log(logGuid, LogMessage, defaultlog);

                return Content(ErrorPage(logMessage));
            }
        }
        
        [HttpGet]
        public ActionResult open(string s)
        {
            if (SiteHelper.PaymentEnable == "0") return Content("");
            var html = Encoding.UTF8.GetString(MyAESEncryption.AESDecrypt(MyAESEncryption.HexToByte(s), AESSecretKey)).Replace("\0", "");
            SiteHelper.Log(logGuid, $"[html]{html}", defaultlog);
            return Content(html);
        }
        /// <summary>
        /// 申请支付金额接口
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult getamountlist()
        {
            if (SiteHelper.PaymentEnable == "0") return Content("");
            return Content(tungyi.getamountlist(logGuid));
        }

        /// <summary>
        /// 申请支付金额接口
        /// </summary>
        /// <returns></returns>   
        [OutputCache(Duration = 15, VaryByParam = "id")]
        public ActionResult amountlist(int id)
        {
            if (SiteHelper.PaymentEnable == "0") return Content("");
            return Content(Service.MergeAmountList(logGuid, id));
            //return Content(tungyi.amountlist(logGuid, id));
        }
        public ActionResult oldamountlist(int id)
        {
            if (SiteHelper.PaymentEnable == "0") return Content("");
            return Content(Service.NewAmountList(logGuid, id));
        }

        public ActionResult randomamountlist(int id)
        {
            if (SiteHelper.PaymentEnable == "0") return Content("");
            return Content(Service.RandomAmountList(logGuid, id));
        }

        public ActionResult mergeamountlist(int id)
        {
            if (SiteHelper.PaymentEnable == "0") return Content("");
            return Content(Service.MergeAmountList(logGuid, id));
        }

        #region Other Pay

        /// <summary>
        /// W付回調
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult WfuCall()
        {
            var state = wfu.Callback(logGuid);
            if (state == PayState.Success)
            {
                return Content("SUCCESS");
            }
            else
            {
                return Content("Error");
            }
        }

        /// <summary>
        /// 艾米森回調
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AimisenCall()
        {
            var state = aimisen.callback(logGuid);
            if (state == PayState.Success)
            {
                return Content("SUCCESS");
            }
            else
            {
                return Content("Error");
            }
        }

        /// <summary>
        /// 70卡回調
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ChishrkaCall()
        {

            var state = chishrka.call(logGuid);

            if (state == PayState.SignError)
            {
                return Content("sign error");
            }

            if (state == PayState.Success)
            {
                return Content("ok");
            }
            else
            {
                return Content("error");
            }

        }

        /// <summary>
        /// 同乐支付
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult TungleCall()
        {

            var state = tungle.call(logGuid);

            if (state == PayState.SignError)
            {
                return Content("sign error");
            }

            if (state == PayState.Success)
            {
                return Content("SUCCESS");
            }
            else
            {
                return Content("error");
            }
        }

        /// <summary>
        /// 聚合支付
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult JiuheCall()
        {

            var state = jiuhe.call(logGuid);

            if (state == PayState.SignError)
            {
                return Content("sign error");
            }

            if (state == PayState.Success)
            {
                return Content("SUCCESS");
            }
            else
            {
                return Content("error");
            }

        }

        /// <summary>
        /// 统一支付
        /// </summary>
        /// <returns></returns>
        public ActionResult TungyiCall()
        {
            var state = tungyi.call(logGuid);

            if (state == PayState.SignError)
            {
                return Content("sign error");
            }

            if (state == PayState.Success)
            {
                return Content("SUCCESS");
            }
            else
            {
                return Content("error");
            }
        }

        /// <summary>
        /// 统一支付代理回調(久聊)
        /// </summary>
        /// <returns></returns>
        public ActionResult TungyiAgentCall()
        {
            var state = tungyi.AgentCall(logGuid);

            if (state == PayState.SignError)
            {
                return Content("sign error");
            }

            if (state == PayState.Success)
            {
                return Content("SUCCESS");
            }
            else
            {
                return Content("error");
            }
        }

        /// <summary>
        /// MCPay回調
        /// </summary>
        /// <returns></returns>
        public ActionResult MCPayCall()
        {
            var state = MCPay.call(logGuid);
            if (state == PayState.Success)
            {
                return Content("OK");
            }
            else
            {
                return Content("Error");
            }
        }

        /// <summary>
        /// UNPay回調
        /// </summary>
        /// <returns></returns>
        public ActionResult UNPayCall()
        {
            var state = UNPay.call(logGuid);
            if (state == PayState.Success)
            {
                return Content("ok");
            }
            else
            {
                return Content("error");
            }
        }

        /// <summary>
        /// XPayCall
        /// </summary>
        /// <returns></returns>
        public ActionResult XPayCall()
        {
            var state = XPay.call(logGuid);
            if (state == PayState.Success)
            {
                return Content("success");
            }
            else
            {
                return Content("error");
            }
        }

        /// <summary>
        /// 云支付
        /// </summary>
        /// <returns></returns>
        public ActionResult XunPayCall()
        {
            var state = Xunpay.call(logGuid);
            if (state == PayState.Success)
            {
                return Content("success");
            }
            else
            {
                return Content("error");
            }
        }

        /// <summary>
        /// 超凡支付
        /// </summary>
        /// <returns></returns>
        public ActionResult CfanPayCall()
        {
            var state = CfanPay.call(logGuid);
            if (state == PayState.Success)
            {
                return Content("success");
            }
            else
            {
                return Content("error");
            }
        }

        /// <summary>
        /// 铁锤支付
        /// </summary>
        /// <returns></returns>
        public ActionResult TiechuipayCall()
        {
            var state = Tiechuipay.call(logGuid);
            if (state == PayState.Success)
            {
                return Content("OK");
            }
            else
            {
                return Content("error");
            }
        }

        /// <summary>
        /// 宝盒支付
        /// </summary>
        /// <returns></returns>
        public ActionResult BaohepayCall()
        {
            var state = Baohepay.call(logGuid);
            if (state == PayState.Success)
            {
                return Content("OK");
            }
            else
            {
                return Content("error");
            }
        }

        /// <summary>
        /// 冬瓜支付
        /// </summary>
        /// <returns></returns>
        public ActionResult XiamipayCall()
        {
            var state = Xiamipay.call(logGuid);
            if (state == PayState.Success)
            {
                return Content(@"{""ret"":true}");
            }
            else
            {
                return Content("error");
            }
        }

        /// <summary>
        /// 财付支付
        /// </summary>
        /// <returns></returns>
        public ActionResult XF188Call()
        {
            var state = XF188Pay.call(logGuid);
            if (state == PayState.Success)
            {
                return Content(@"OK");
            }
            else
            {
                return Content("error");
            }
        }

        /// <summary>
        /// 亿点支付
        /// </summary>
        /// <returns></returns>
        public ActionResult GP_YDCall()
        {
            var state = GP_YDPay.call(logGuid);
            if (state == PayState.Success)
            {
                return Content(@"OK");
            }
            else
            {
                return Content("error");
            }
        }

        /// <summary>
        /// 客付通支付
        /// </summary>
        /// <returns></returns>
        public ActionResult KFTCall()
        {
            var state = KFTPay.call(logGuid);
            if (state == PayState.Success)
            {
                return Content(@"OK");
            }
            else
            {
                return Content("error");
            }
        }

        /// <summary>
        /// 汇源通支付
        /// </summary>
        /// <returns></returns>
        public ActionResult HYTCall()
        {
            var state = HYTPay.call(logGuid);
            if (state == PayState.Success)
            {
                return Content(@"OK");
            }
            else
            {
                return Content("error");
            }
        }

        /// <summary>
        /// 環信2站
        /// </summary>
        /// <returns></returns>
        public ActionResult HX2ZCall()
        {
            var state = HX2ZPay.call(logGuid);
            if (state == PayState.Success)
            {
                return Content(@"OK");
            }
            else
            {
                return Content("error");
            }
        }

        /// <summary>
        /// 金牌MC
        /// </summary>
        /// <returns></returns>
        public ActionResult GPMCCall()
        {
            var state = GPMCPay.call(logGuid);
            if (state == PayState.Success)
            {
                return Content(@"OK");
            }
            else
            {
                return Content("error");
            }
        }

        /// <summary>
        /// 星入宝
        /// </summary>
        /// <returns></returns>
        public ActionResult xingrubaoCall()
        {
            var state = xingrubao.call(logGuid);
            if (state == PayState.Success)
            {
                return Content(@"OK");
            }
            else
            {
                return Content("error");
            }
        }

        /// <summary>
        /// 永安
        /// </summary>
        /// <returns></returns>
        public ActionResult YACall()
        {
            var state = YAPay.call(logGuid);
            if (state == PayState.Success)
            {
                return Content(@"SUCCESS");
            }
            else
            {
                return Content("error");
            }
        }

        /// <summary>
        /// 蓉银
        /// </summary>
        /// <returns></returns>
        public ActionResult RongyinCall()
        {
            var state = Rongyin.call(logGuid);
            if (state == PayState.Success)
            {
                return Content("success");
            }
            else
            {
                return Content("error");
            }
        }

        /// <summary>
        /// 融支付
        /// </summary>
        /// <returns></returns>
        public ActionResult RongCall()
        {
            var state = RongPay.call(logGuid);
            if (state == PayState.Success)
            {
                return Content("success");
            }
            else
            {
                return Content("error");
            }
        }

        /// <summary>
        /// 维护宝支付
        /// </summary>
        /// <returns></returns>
        public ActionResult WHBCall()
        {
            var state = WHBPay.call(logGuid);
            if (state == PayState.Success)
            {
                return Content("OK");
            }
            else
            {
                return Content("error");
            }
        }

        /// <summary>
        /// 8pay支付
        /// </summary>
        /// <returns></returns>
        public ActionResult EightPayCall()
        {
            var state = EightPay.call(logGuid);
            if (state == PayState.Success)
            {
                return Content("ok");
            }
            else
            {
                return Content("error");
            }
        }

        /// <summary>
        /// 聚众支付
        /// </summary>
        /// <returns></returns>
        public ActionResult JuZhongPayCall()
        {
            var state = JuZhongPay.call(logGuid);
            if (state == PayState.Success)
            {
                return Content("SUCCESS");
            }
            else
            {
                return Content("error");
            }
        }

        /// <summary>
        /// 万伽支付
        /// </summary>
        /// <returns></returns>
        public ActionResult WangyuaPayCall()
        {
            var state = WangyuaPay.call(logGuid);
            if (state == PayState.Success)
            {
                return Content("success");
            }
            else
            {
                return Content("error");
            }
        }

        /// <summary>
        /// 天边支付
        /// </summary>
        /// <returns></returns>
        public ActionResult TBCall()
        {
            var state = TianbianPay.call(logGuid);
            if (state == PayState.Success)
            {
                return Content("success");
            }
            else
            {
                return Content("error");
            }
        }

        /// <summary>
        /// 派派支付
        /// </summary>
        /// <returns></returns>
        public ActionResult PPZFCall()
        {
            var state = PPZFPay.call(logGuid);
            if (state == PayState.Success)
            {
                return Content("success");
            }
            else
            {
                return Content("error");
            }
        }

        /// <summary>
        /// 凯旋支付
        /// </summary>
        /// <returns></returns>
        public ActionResult KaiXuanCall()
        {
            var state = KaiXuanPay.call(logGuid);
            if (state == PayState.Success)
            {
                return Content("success");
            }
            else
            {
                return Content("error");
            }
        }

        /// <summary>
        /// 365支付
        /// </summary>
        /// <returns></returns>
        public ActionResult Pay365Call()
        {
            var state = Pay365.call(logGuid);
            if (state == PayState.Success)
            {
                return Content("SUCCESS");
            }
            else
            {
                return Content("FAIL");
            }
        }

        /// <summary>
        /// 盈支付
        /// </summary>
        /// <returns></returns>
        public ActionResult YINPayCall()
        {
            var state = YINPay.call(logGuid);
            if (state == PayState.Success)
            {
                return Content("OK");
            }
            else
            {
                return Content("error");
            }
        }

        /// <summary>
        /// 闪入宝
        /// </summary>
        /// <returns></returns>
        public ActionResult ShanRuBaoCall()
        {
            var state = ShanRuBao.call(logGuid);
            if (state == PayState.Success)
            {
                return Content("success");
            }
            else
            {
                return Content("error");
            }
        }

        /// <summary>
        /// 金城支付
        /// </summary>
        /// <returns></returns>
        public ActionResult JinChengCall()
        {
            var state = JinCheng.call(logGuid);
            if (state == PayState.Success)
            {
                return Content("success");
            }
            else
            {
                return Content("error");
            }
        }

        /// <summary>
        /// 快付支付
        /// </summary>
        /// <returns></returns>
        public ActionResult KuaiFuCall()
        {
            var state = KuaiFu.call(logGuid);
            if (state == PayState.Success)
            {
                return Content("success");
            }
            else
            {
                return Content("error");
            }
        }

        /// <summary>
        /// 牛牛支付
        /// </summary>
        /// <returns></returns>
        public ActionResult NiuNiuCall()
        {
            var state = NiuNiuPay.call(logGuid);
            if (state == PayState.Success)
            {
                return Content("OK");
            }
            else
            {
                return Content("error");
            }
        }

        /// <summary>
        /// 盒付支付
        /// </summary>
        /// <returns></returns>
        public ActionResult HeFuPayCall()
        {
            var state = HeFuPay.call(logGuid);
            if (state == PayState.Success)
            {
                return Content("OK");
            }
            else
            {
                return Content("error");
            }
        }

        /// <summary>
        /// 慧云支付
        /// </summary>
        /// <returns></returns>
        public ActionResult HuiyunCall()
        {
            var state = HuiyunPay.call(logGuid);
            if (state == PayState.Success)
            {
                return Content("ok");
            }
            else
            {
                return Content("error");
            }
        }

        /// <summary>
        /// 慧云支付
        /// </summary>
        /// <returns></returns>
        public ActionResult DjfzfCall()
        {
            var state = DjfzfPay.call(logGuid);
            if (state == PayState.Success)
            {
                return Content("ok");
            }
            else
            {
                return Content("error");
            }
        }

        /// <summary>
        /// 闪付支付回調
        /// </summary>
        /// <returns></returns>
        public ActionResult shanfuCallBack(int accountId, int playerId, string orderId, string teamOrderId, string chaseOrderId, int payType, int amount, int presentAmount, string status, int time, string sign)
        {
            var _orderId = string.Empty;
            var _message = string.Empty;
            string result = shanfu.shanfuCallBack(logGuid, accountId, playerId, orderId, teamOrderId, chaseOrderId, payType, amount, presentAmount, status, time, sign, out _orderId, out _message);
            return Json(new
            {
                status = result,
                teamOrderId = _orderId
            });
        }

        public ActionResult kuailianCallBack()
        {
            var state = kuailian.call(logGuid);

            if (state == PayState.Success)
            {
                return Content("0000");
            }
            else
            {
                return Content("Error");
            }
        }

        public ActionResult dianyuantongCallBack()
        {
            var state = dianyuantong.call(logGuid);

            if (state == PayState.Success)
            {
                return Content("success");
            }
            else
            {
                return Content("fail");
            }
        }

        /// <summary>
        /// 蜻蜓回调
        /// </summary>
        /// <returns></returns>
        public ActionResult QingtingCall(int id)
        {
            var state = Qingting.call(logGuid, id);

            if (state == PayState.Success)
            {
                return Content("success");
            }
            else
            {
                return Content("fail");
            }
        }

        /// <summary>
        /// 汇通
        /// </summary>
        /// <returns></returns>
        public ActionResult HuitongCall()
        {
            var state = Huitong.call(logGuid);
            if (state == PayState.Success)
            {
                return Content("success");
            }
            else
            {
                return Content("error");
            }
        }

        /// <summary>
        /// U鑫支付
        /// </summary>
        /// <returns></returns>
        public ActionResult UxinCall()
        {
            var state = Uxin.call(logGuid);
            if (state == PayState.Success)
            {
                return Content("OK");
            }
            else
            {
                return Content("error");
            }
        }

        /// <summary>
        /// HiPay支付
        /// </summary>
        /// <returns></returns>
        public ActionResult HiPayCall()
        {
            var state = HiPay.call(logGuid);
            if (state == PayState.Success)
            {
                return Content("success");
            }
            else
            {
                return Content("error");
            }
        }

        /// <summary>
        /// 安迅通支付
        /// </summary>
        /// <returns></returns>
        public ActionResult AnxanpayCall()
        {
            var state = Anxanpay.call(logGuid);
            if (state == PayState.Success)
            {
                return Content("OK");
            }
            else
            {
                return Content("error");
            }
        }

        #endregion

        /// <summary>
        /// AE支付
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AEPayCall()
        {
            var state = AEPay.Call(logGuid);
            if (state == PayState.Success)
            {
                return Content("SUCCESS");
            }
            else
            {
                return Content("Error");
            }
        }


        #region 3rd pay

        /// <summary>
        /// 金王朝
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DynasPayCall()
        {
            var state = DynasPay.Call(logGuid);
            if (state == PayState.Success)
            {
                return Content("SUCCESS");
            }
            else
            {
                return Content("Error");
            }
        }

        /// <summary>
        /// 碼上賺
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult MSZPayCall()
        {
            var state = MSZPay.Call(logGuid);
            if (state == PayState.Success)
            {
                return Content("SUCCESS");
            }
            else
            {
                return Content("SUCCESS");
            }
        }

        /// <summary>
        /// 商易付
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SYFPayCall()
        {
            var state = SYFPay.Call(logGuid);
            if (state == PayState.Success)
            {
                return Content("OK");
            }
            else
            {
                //對商易付只要有收到回應就傳OK，所以訂單失敗不額外處理
                return Content("OK");
            }
        }

        /// <summary>
        /// 新威付
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult NewWeiFuPayCall()
        {
            var state = NewWeiFuPay.Call(logGuid);
            if (state == PayState.Success)
            {
                return Content("SUCCESS");
            }
            else
            {
                //失败回送任意值
                return Content("ERROR");
            }
        }

        /// <summary>
        /// 巨富
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GiFuBoPayCall(GiFuBoPayCallbackModel model)
        {
            var state = GiFuBoPay.Call(logGuid, model);
            //收到我方的回调通知后，回复 success 字符串给我方。
            return Content("success");
        }

        /// <summary>
        /// 叮叮寶
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DDPayCall()
        {
            var state = DDPay.Call(logGuid);
            //收到我方的回调通知后，回复 success 字符串给我方。
            return Content("success");
        }

        /// <summary>
        /// 通匯
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult THPayCall()
        {
            var stage = THPay.Call(logGuid);
            if(stage == PayState.SignError)
            {
                return Content("error");
            }

            return Content("success");
        }

        /// <summary>
        /// 鐵付
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult IronPayCall()
        {
            var stage = IronPay.Call(logGuid);
            if (stage == PayState.SignError)
            {
                return Content("fail");
            }

            return Content("success");
        }

        /// <summary>
        /// 四方(新王朝)
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult HFZCPayCall()
        {
            var stage = HFZCPay.Call(logGuid);
            if (stage == PayState.SignError)
            {
                return Content("ERROR");
            }

            return Content("SUCCESS");
        }

        /// <summary>
        /// 啟飛
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GoldSoarPayCall(GoldSoarPayResponseModel model)
        {
            var stage = GoldSoarPay.Call(logGuid, model);
            if (stage == PayState.SignError)
            {
                return Content("ERROR");
            }

            return Content("SUCCESS");
        }

        /// <summary>
        /// 吉寶
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult JBPayCall(JBPayResponseModel model)
        {
            var stage = JBPay.Call(logGuid, model);
            if (stage == PayState.SignError)
            {
                return Content("fail");
            }

            return Content("success");
        }

        /// <summary>
        /// 樂匯第三方
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult LeHuiPayCall(LeHuiPayResponseModel model)
        {
            var stage = LeHuiPay.Call(logGuid, model);
            if (stage == PayState.SignError)
            {
                return Content("ERROR");
            }

            return Content("SUCCESS");
        }

        /// <summary>
        /// 第一付
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult D1FPayCall(D1FPayResponseModel model)
        {
            var stage = D1FPay.Call(logGuid, model);
            if (stage == PayState.SignError)
            {
                return Content("ERROR");
            }

            return Content("OK");
        }

        /// <summary>
        /// 立馬付
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult LiMaFuPayCall(LiMaFuPayResponseModel model)
        {
            var stage = LiMaFuPay.Call(logGuid, model);
            if (stage == PayState.SignError)
            {
                return Content("ERROR");
            }

            return Content("SUCCESS");
        }

        /// <summary>
        /// 永勝支付
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult YungShengPayCall(YungShengPayResponseModel model)
        {
            var stage = YungShengPay.Call(logGuid, model);
            if (stage == PayState.SignError)
            {
                return Content("error");
            }

            return Content("success");
        }

        /// <summary>
        /// 永信支付
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult YongXinPayCall(YongXinPayResponseModel model)
        {
            var stage = YongXinPay.Call(logGuid, model);
            if (stage == PayState.SignError)
            {
                return Content("error");
            }

            return Content("success");
        }

        /// <summary>
        /// 微支付
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult WpPayCall(WpPayResponseModel model)
        {
            var stage = WpPay.Call(logGuid, model);
            if (stage == PayState.SignError)
            {
                return Content("error");
            }

            return Content("success");
        }

        /// <summary>
        /// DFQJ支付
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DfqjPayCall(DfqjPayResponseModel model)
        {
            var stage = DfqjPay.Call(logGuid, model);
            if (stage == PayState.SignError)
            {
                return Content("error");
            }

            return Content("success");
        }

        /// <summary>
        /// SJQP支付
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SjqpPayCall(SjqpPayResponseModel model)
        {
            var stage = SjqpPay.Call(logGuid, model);
            if (stage == PayState.SignError)
            {
                return Content("error");
            }

            return Content("ok");
        }

        /// <summary>
        /// 光速付
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SpeedPayCall(SpeedPayResponseModel model)
        {
            SpeedPay.Call(logGuid, model);
            var returnObj = new SpeedPayResponseReturnModel();

            return Json(returnObj);
        }

        /// <summary>
        /// 善付
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SFPayCall()
        {
            var stage = SFPay.Call(logGuid);
            if (stage == PayState.SignError)
            {
                return Content("fail");
            }

            return Content("success");
        }

        /// <summary>
        /// 天空付
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult WonderfulskyPayCall(WonderfulskyPayResponseGetModel model)
        {
            WonderfulskyPay.Call(logGuid, model);
            //貴司須在接到通知後返回”SUCCESS”以確認接收。
            return Content("SUCCESS");
        }

        /// <summary>
        /// 天空付
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult WonderfulskyPayCall(WonderfulskyPayResponseModel model)
        {
            WonderfulskyPay.Call(logGuid, model);
            //貴司須在接到通知後返回”SUCCESS”以確認接收。
            return Content("SUCCESS");
        }

        /// <summary>
        /// 佰坊支付
        /// </summary>
        [HttpPost]
        public ActionResult GoPayCall(GoPayResponseModel model)
        {
            var stage = GoPay.Call(logGuid, model);
            if (stage == PayState.SignError)
            {
                return Content("fail");
            }

            return Content("success");
        }

        /// <summary>
        /// 上分通
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SFTPayCall(SFTPayResponseModel model)
        {
            var stage = SFTPay.Call(logGuid, model);
            if (stage == PayState.SignError)
            {
                return Content("fail");
            }

            return Content("success");
        }

        /// <summary>
        /// 风云聚合
        /// </summary>
        [HttpPost]
        public ActionResult DSDFPayCall()
        {
            var stage = DSDFPay.Call(logGuid);
            if (stage == PayState.SignError)
            {
                return Content("fail");
            }

            return Content("true");
        }

        /// <summary>
        /// 聚宝盆
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult MTPayCall()
        {
            var state = MTPay.Call(logGuid);
            if (state == PayState.Success)
            {
                return Content("OK");
            }
            else
            {
                //失败回送任意值
                return Content("OK");
            }
        }

        /// <summary>
        /// TJPay
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult TJPayCall()
        {
            var state = TJPay.Call(logGuid);
            if (state == PayState.Success)
            {
                return Content("OK");
            }
            else
            {
                //失败回送任意值
                return Content("OK");
            }
        }

        /// <summary>
        /// 龍聚支付
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult LongJuPayCall()
        {
            var state = LongJuPay.Call(logGuid);
            if (state == PayState.Success)
            {
                return Content("success ");
            }
            else
            {
                //失败回送任意值
                return Content("success ");
            }
        }


        
        #endregion

        /// <summary>
        /// 支付完成頁面
        /// </summary>
        /// <returns></returns>
        public ActionResult Finish()
        {
            return View("finish");
        }
        public ActionResult Error(string errMessage = "")
        {
            ViewBag.ErrorMessage = errMessage;
            return View("Error");
        }

        private string ErrorPage(string errMessage = "")
        {

            string page = @"

            <!DOCTYPE html>
            <html>
            <head> 
            <meta charset='UTF-8' /> 
            <meta name='viewport' content='width=device-width' />
            <title>支付结果</title> 
            </head> 
            <body>
            <style type='text/css'>
            body {
            margin: 0;
            padding: 0;
            background-color: brown;
            -moz-background-size: cover;
            background-size: cover;
            }
            </style>
            <table width='100%' height='100%'>
            <tr>
            <td align='center'>
            <font color='white'>
            <H1>支付发生错误，请关闭页面重新尝试！</H1>
            <H2>" + errMessage + @"</H2>
            </font>
            </td>
            </tr>
            </table>
            </body></html>
            ";
            return page;
        }
        private string PayGatewayNotFound(string errMessage = "")
        {
            string page = @"
            <!DOCTYPE html>
                <html>
                    <head> 
                        <meta charset='UTF-8' /> 
                        <meta name='viewport' content='width=device-width' />
                        <title>支付结果</title> 
                    </head> 
                    <body>
                    <style type='text/css'>
                        body {
                            margin: 0;
                            padding: 0;
                            -moz-background-size: cover;
                            background-size: cover;
                        }
                    </style>
                    <table width='100%' height='100%'>
                        <tr>
                            <td align='center'>
                                <div>
                                    <H2>&nbsp;</H2>
                                    <H3>" + errMessage + @"</H3>
                                </div>
                            </td>
                        </tr>
                    </table>
                </body>
            </html>";
            return page;
        }



        private void SetGuidOnRequest(int orderId)
        {
            try
            {
                System.Web.HttpContext.Current.Items.Add("Memo", "orderId: " + orderId.ToString());
                System.Web.HttpContext.Current.Items.Add("UUID", logGuid);
            }
            catch
            {

            }
        }

    }
}
 