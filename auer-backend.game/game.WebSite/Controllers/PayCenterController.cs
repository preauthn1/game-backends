using System;
using System.Drawing;
using example.invalid;
using System.Web.Mvc;
using game.Entity.Pay;
using game.WebSite.PaymentHelper;
using ZXing;
using ZXing.QrCode;
using static game.WebSite.PayConfig;
using static game.WebSite.PaymentHelper.Gash;

namespace game.WebSite.Controllers
{
    public class PayCenterController : Controller
    {
        string logGuid = Guid.NewGuid().ToString().Substring(0, 5);


        public ActionResult Index(int orderid, int? gwid, string appid = "")
        {
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
            /////////////////////////

            var logMessage = string.Empty;
            var outMessage = string.Empty;
            var returnUrl = string.Empty;
            var tradeCode = string.Empty;

            SiteHelper.Log(logGuid, string.Format("充值请求 orderid={0}", orderid), defaultlog);
            try
            {
                //取得訂單號
                var order = Service.CDGetPayOrderByID(orderid);
                if (!(order != null && order.Id > 0 && order.Status == CDOrderStatus.Waiting))
                {
                    logMessage = $"订单号无效，订单号 {orderid}";
                    SiteHelper.Log(logGuid, logMessage, defaultlog);
                    //return RedirectToAction("Error", new { errMessage = logMessage });
                    return Content(ErrorPage(logMessage));
                }

                //取得第三方支付
                var GWID = SiteHelper.GetGWID(order, gwid, ref tradeCode);
                if (GWID == 0)
                {
                    logMessage = $"支付渠道无效，订单号 {orderid}";
                    SiteHelper.Log(logGuid, logMessage, defaultlog);
                    //return RedirectToAction("Error", new { errMessage = logMessage });
                    return Content(ErrorPage(logMessage));
                }

                if (order.GWID == (int)PayGatewayType.Aimisen)
                {
                    var bankName = Request["bankName"];
                    if (order.PayTypeID == (int)PayType.Bank && bankName == null) return View();

                    returnUrl = aimisen.post(order, logGuid, tradeCode, ref outMessage);
                }
                else if (order.GWID == (int)PayGatewayType.GashPay)
                {
                    returnUrl = Gash.post(order, Gash_Currency, tradeCode);
                    return Content(returnUrl);
                }
                else if (order.GWID == (int)PayGatewayType.Wfu)
                {
                    if (order.PayTypeID == (int)PayType.Bank)
                    {
                        ViewBag.Paras = wfu.Wfu_Bank(order, logGuid);
                        return View("Wfu");
                    }

                    returnUrl = wfu.post(order, logGuid, tradeCode, ref outMessage);
                }
                else if (order.GWID == (int)PayGatewayType.Card70)
                {
                    returnUrl = chishrka.post(order, logGuid, tradeCode);
                }
                else if (order.GWID == (int)PayGatewayType.Tungle)
                {
                    returnUrl = tungle.post(order, logGuid, tradeCode, ref outMessage);
                }
                else if (order.GWID == (int)PayGatewayType.Jiuhe)
                {
                    returnUrl = jiuhe.post(order, logGuid, tradeCode);
                }
                else if (order.GWID == (int)PayGatewayType.Tungyi || 
                         order.GWID == (int)PayGatewayType.TungyiHonda ||
                         order.GWID == (int)PayGatewayType.TungyiDF)
                {
                    var content = tungyi.post(order, logGuid, tradeCode, appid, ref outMessage);

                    if (content.Length == 0)
                    {
                        SiteHelper.Log(logGuid, outMessage, defaultlog);
                        return Content(ErrorPage(logMessage));
                    }
                    return Content(content);
                }
                else if (order.GWID == (int)PayGatewayType.MCPay)
                {
                    returnUrl = MCPay.post(order, logGuid, tradeCode);
                }
                else if (order.GWID == (int)PayGatewayType.UNPay)
                {
                    returnUrl = UNPay.post(order, logGuid, tradeCode);
                }
                else if (order.GWID == (int)PayGatewayType.XPay)
                {
                    returnUrl = XPay.post(order, logGuid, tradeCode, ref outMessage);
                }
                else if (order.GWID == (int)PayGatewayType.XunPay)
                {
                    returnUrl = Xunpay.post(order, logGuid, tradeCode, ref outMessage);
                }
                else if (order.GWID == (int)PayGatewayType.CfanPay)
                {
                    returnUrl = CfanPay.post(order, logGuid, tradeCode, ref outMessage);
                }

                else if (order.GWID == (int)PayGatewayType.KuaiLian)
                {
                    var isQrcodeBuild = false;
                    returnUrl = kuailian.post(order, logGuid, tradeCode, ref isQrcodeBuild, ref outMessage);
                    if (isQrcodeBuild)
                    {
                        //支付方式 (名字带有【扫码】【二维码】的方式，返回的payUrl鏈接要自已主动生成二维码！)
                        if (!string.IsNullOrEmpty(returnUrl))
                        {
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
                            return File(buff, "image/jpeg");
                        }
                        return Content(ErrorPage(outMessage));
                    }
                }
                else if (order.GWID == (int)PayGatewayType.DianYuanTong)
                {
                    returnUrl = dianyuantong.post(order, logGuid);
                }
                else if (order.GWID == (int)PayGatewayType.Baohepay)
                {
                    returnUrl = Baohepay.post(order, logGuid, tradeCode, ref outMessage);

                    if (returnUrl.Length > 0)
                        return Content("<html><body>" + returnUrl + "</body></html>");
                }
                else if (order.GWID == (int)PayGatewayType.Tiechuipay)
                {
                    returnUrl = Tiechuipay.post(order, logGuid, tradeCode, ref outMessage);

                }
                else if (order.GWID == (int)PayGatewayType.Xiamipay)
                {
                    returnUrl = Xiamipay.post(order, logGuid, tradeCode, ref outMessage);

                }
                else if (order.GWID == (int)PayGatewayType.DFXiamipay)
                {
                    returnUrl = DFXiamipay.post(order, logGuid, tradeCode, ref outMessage);

                }
                else if (order.GWID == (int)PayGatewayType.XF188pay)
                {
                    returnUrl = XF188Pay.post(order, logGuid, tradeCode);
                    if (returnUrl.Length > 0)
                        return Content(returnUrl);
                }
                else if (order.GWID == (int)PayGatewayType.GP_YDpay)
                {
                    returnUrl = GP_YDPay.post(order, logGuid, tradeCode);

                }
                else if (order.GWID == (int)PayGatewayType.Qingting || order.GWID == (int)PayGatewayType.QingtingQe)
                {
                    returnUrl = Qingting.post(order, logGuid, tradeCode, ref outMessage);

                    if (returnUrl.Length == 0)
                        return Content(ErrorPage(outMessage));
                }
                else if (order.GWID == (int)PayGatewayType.KFTpay)
                {
                    returnUrl = KFTPay.post(order, logGuid, tradeCode);                   
                }
                else if (order.GWID == (int)PayGatewayType.Huitong)
                {
                    returnUrl = Huitong.post(order, logGuid, tradeCode, ref outMessage);
                }
                else if (order.GWID == (int)PayGatewayType.HYTpay)
                {
                    returnUrl = HYTPay.post(order, logGuid, tradeCode);
                    return Content(returnUrl);
                }
                else if (order.GWID == (int)PayGatewayType.HX2Zpay)
                {
                    returnUrl = HX2ZPay.post(order, logGuid, tradeCode);
                    return Content(returnUrl);
                }
                else if (order.GWID == (int)PayGatewayType.GPMCpay)
                {
                    returnUrl = GPMCPay.post(order, logGuid, tradeCode);
                    return Content(returnUrl);
                }
                else if (order.GWID == (int)PayGatewayType.xingrubao)
                {
                    returnUrl = xingrubao.post(order, logGuid, tradeCode);
                    return Content(returnUrl);
                }

                else if (order.GWID == (int)PayGatewayType.YAPay)
                {
                    var isQrcodeBuild = false;
                    returnUrl = YAPay.post(order, logGuid, tradeCode, ref isQrcodeBuild, ref logMessage);
                    if (isQrcodeBuild)
                    {
                        //支付方式 (名字带有【扫码】【二维码】的方式，返回的payUrl鏈接要自已主动生成二维码！)
                        if (!string.IsNullOrEmpty(returnUrl))
                        {
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
                            return File(buff, "image/jpeg");
                        }
                        return Content(ErrorPage(outMessage));
                    }
                }
                else if (order.GWID == (int)PayGatewayType.Rongyin)
                {
                    returnUrl = Rongyin.post(order, logGuid, tradeCode, ref logMessage);

                }
                else if (order.GWID == (int)PayGatewayType.RongPay)
                {
                    returnUrl = RongPay.post(order, logGuid, tradeCode, ref logMessage);

                }
                else if (order.GWID == (int)PayGatewayType.Uxin)
                {
                    returnUrl = Uxin.post(order, logGuid, tradeCode, ref logMessage);

                    if (returnUrl.Length > 0)
                        return Content("<html><body>" + returnUrl + "</body></html>");
                }
                else if (order.GWID == (int)PayGatewayType.Yongwang)
                {
                    returnUrl = Yongwang.post(order, logGuid, tradeCode, ref logMessage);

                }
                else if (order.GWID == (int)PayGatewayType.WHBPay)
                {
                    
                    returnUrl = WHBPay.post(order, logGuid, tradeCode);
                    return Content(returnUrl);
                }
                else if (order.GWID == (int)PayGatewayType.EightPay)
                {

                    returnUrl = EightPay.post(order, logGuid, tradeCode);
                    return Content(returnUrl);
                }
                else if (order.GWID == (int)PayGatewayType.HiPay)
                {
                    returnUrl = HiPay.post(order, logGuid, tradeCode, ref logMessage);                  
                }
                else if (order.GWID == (int)PayGatewayType.JuZhongPay)
                {
                    returnUrl = JuZhongPay.post(order, logGuid, tradeCode, ref logMessage);
                   
                }
                else if (order.GWID == (int)PayGatewayType.Anxanpay)
                {
                    returnUrl = Anxanpay.post(order, logGuid, tradeCode, ref logMessage);
                    return Content(returnUrl);
                }
                else if (order.GWID == (int)PayGatewayType.WangyuaPay)
                {
                    returnUrl = WangyuaPay.post(order, logGuid, tradeCode, ref logMessage);
                   
                }
                else if (order.GWID == (int)PayGatewayType.Tianbianpay)
                {
                    returnUrl = TianbianPay.post(order, logGuid, tradeCode, ref logMessage);

                    if (!string.IsNullOrEmpty(returnUrl))
                    {
                        var writer = new BarcodeWriter
                        {
                            Format = BarcodeFormat.QR_CODE,
                            Options = new QrCodeEncodingOptions
                            {
                                Height = 200,
                                Width = 200,
                                Margin = 1
                            }
                        };
                        Bitmap m_Bitmap = writer.Write(returnUrl);
                        MemoryStream ms = new MemoryStream();
                        m_Bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                        byte[] buff = ms.ToArray();
                        
                        return Content(string.Format("<html><body><img src='data:image/jpeg;base64,{0}'style='margin: 0 auto; display: block;' /></body></html>", Convert.ToBase64String(buff)));
                    }

                }
                else if (order.GWID == (int)PayGatewayType.PPZFPay)
                {
                    returnUrl = PPZFPay.post(order, logGuid, tradeCode, ref logMessage);
                   
                }
                else if (order.GWID == (int)PayGatewayType.KaiXuanPay)
                {
                    returnUrl = KaiXuanPay.post(order, logGuid, tradeCode, ref logMessage);
                    return Content(returnUrl);

                }
                else if (order.GWID == (int)PayGatewayType.Pay365)
                {
                    returnUrl = Pay365.post(order, logGuid, tradeCode, ref logMessage);
                }
                else if (order.GWID == (int)PayGatewayType.YINPay)
                {
                    returnUrl = YINPay.post(order, logGuid, tradeCode);
                    return Content(returnUrl);
                }
                else if (order.GWID == (int)PayGatewayType.ShanRuBao)
                {
                    returnUrl = ShanRuBao.post(order, logGuid, tradeCode, ref logMessage);
                    if (returnUrl.Length > 0)
                        return Content(returnUrl);
                }
                else if (order.GWID == (int)PayGatewayType.JinCheng)
                {
                    returnUrl = JinCheng.post(order, logGuid, tradeCode, ref logMessage);
                    if (returnUrl.Length > 0)
                        return Content(returnUrl);
                }
                else if (order.GWID == (int)PayGatewayType.KuaiFu)
                {
                    returnUrl = KuaiFu.post(order, logGuid, tradeCode);
                    if (returnUrl.Length > 0)
                        return Content(returnUrl);
                }
                else if (order.GWID == (int)PayGatewayType.NiuNiuPay)
                {
                    returnUrl = NiuNiuPay.post(order, logGuid, tradeCode);
                 
                }
                else if (order.GWID == (int)PayGatewayType.HeFuPay)
                {
                    returnUrl = HeFuPay.post(order, logGuid, tradeCode);
                    return Content(returnUrl);
                }
                else if (order.GWID == (int)PayGatewayType.Huiyun)
                {
                    returnUrl = HuiyunPay.post(order, logGuid, tradeCode);
                 
                }
                else if (order.GWID == (int)PayGatewayType.Djfzf)
                {
                    returnUrl = DjfzfPay.post(order, logGuid, tradeCode);
                    
                }
                else if (order.GWID == (int)PayGatewayType.EightZpay)
                {
                    returnUrl = EightZpay.post(order, logGuid, tradeCode);
                    return Content(returnUrl);

                }
                else if (order.GWID == (int)PayGatewayType.JinxinPay)
                {
                    returnUrl = JinxinPay.post(order, logGuid, tradeCode);
                    
                }
                else if (order.GWID == (int)PayGatewayType.HuiChengPay)
                {
                    returnUrl = HuiChengPay.post(order, logGuid, tradeCode);
                  
                }
                else if (order.GWID == (int)PayGatewayType.XKPay)
                {
                    returnUrl = XKPay.post(order, logGuid, tradeCode);
                }
                else if (order.GWID == (int)PayGatewayType.Baifu)
                {
                    returnUrl = Baifu.post(order, logGuid, tradeCode, ref logMessage);
                }
                else if (order.GWID == (int)PayGatewayType.ShengShi)
                {
                    returnUrl = ShengShi.post(order, logGuid, tradeCode);
                    return Content(returnUrl);
                }
                else if (order.GWID == (int)PayGatewayType.Zumire)
                {
                    returnUrl = Zumire.post(order, logGuid, tradeCode, ref logMessage);
                }
                else if (order.GWID == (int)PayGatewayType.BitexPay)
                {
                    returnUrl = BitexPay.post(order, logGuid, tradeCode);
                   
                }
                else if (order.GWID == (int)PayGatewayType.Xiangyou)
                {
                    returnUrl = Xiangyou.post(order, logGuid, tradeCode, ref logMessage);                 
                }
                else if (order.GWID == (int)PayGatewayType.QuickPay)
                {
                    returnUrl = QuickPay.post(order, logGuid, tradeCode);

                }
                else if (order.GWID == (int)PayGatewayType.RongHai)
                {
                    returnUrl = RongHai.post(order, logGuid, tradeCode);                   
                }

                else if (order.GWID == (int)PayGatewayType.JFPay)
                {
                    returnUrl = JFPay.post(order, logGuid, tradeCode, ref logMessage);                   
                }
                else if (order.GWID == (int)PayGatewayType.Zhongbang)
                {
                    returnUrl = Zhongbang.post(order, logGuid, tradeCode, ref logMessage);
                }
                else if (order.GWID == (int)PayGatewayType.XinQuanFu)
                {
                    returnUrl = XinQuanFu.post(order, logGuid, tradeCode);
                } 
                else if (order.GWID == (int)PayGatewayType.Putao)
                {
                    returnUrl = Putao.post(order, logGuid, tradeCode, ref logMessage);
                }
                else if (order.GWID == (int)PayGatewayType.LiuBa)
                {
                    returnUrl = LiuBa.post(order, logGuid, tradeCode);
                }
                else if (order.GWID == (int)PayGatewayType.ChengYiJiu)
                {                    
                    returnUrl = ChengYiJiu.post(order, logGuid, tradeCode);
                }
                else if (order.GWID == (int)PayGatewayType.PerfectPay)
                {                    
                   returnUrl = PerfectPay.post(order, logGuid, tradeCode);
                }
                else if (order.GWID == (int)PayGatewayType.TMAllPay)
                {
                    returnUrl = TMAllPay.post(order, logGuid, tradeCode);
                }
                else if (order.GWID == (int)PayGatewayType.XingKunRH)
                {
                    returnUrl = XingKunRH.post(order, logGuid, tradeCode);
                    if (returnUrl.Length > 0)
                        return Content(returnUrl);
                }
                else if (order.GWID == (int)PayGatewayType.GaoSheng)
                {
                    var isQrcodeBuild = false;
                    returnUrl = GaoSheng.post(order, logGuid, tradeCode, ref isQrcodeBuild, ref outMessage);
                    if (isQrcodeBuild)
                    {
                        //支付方式 (名字带有【扫码】【二维码】的方式，返回的payUrl鏈接要自已主动生成二维码！)
                        if (!string.IsNullOrEmpty(returnUrl))
                        {
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
                        }
                        return Content(ErrorPage(outMessage));
                    }
                }
                else if (order.GWID == (int)PayGatewayType.XingKunYH)
                {
                    returnUrl = XingKunYH.post(order, logGuid, tradeCode, ref outMessage);
                    if (!string.IsNullOrEmpty(returnUrl))
                    {
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
                        return File(buff, "image/jpeg");
                    }
                    return Content(ErrorPage(outMessage));
                    /*
                    returnUrl = XingKunYH.post(order, logGuid, tradeCode, ref outMessage);
                    if (returnUrl.Length > 0)
                        return Content(returnUrl);
                    */
                }
                else if (order.GWID == (int)PayGatewayType.XingKunXK)
                {
                    returnUrl = XingKunXK.post(order, logGuid, tradeCode, ref outMessage);
                }
                else if (order.GWID == (int)PayGatewayType.QiShengPay)
                {
                    returnUrl = QiShengPay.post(order, logGuid, tradeCode);
                }
                else if (order.GWID == (int)PayGatewayType.XiaoMei)
                {
                    returnUrl = XiaoMei.post(order, logGuid, tradeCode);
                }
                else if (order.GWID == (int)PayGatewayType.SumiPay)
                {
                    returnUrl = SumiPay.post(order, logGuid, tradeCode);
                }
                else if (order.GWID == (int)PayGatewayType.FytxPay)
                {
                    var isQrcodeBuild = false;
                    returnUrl = FytxPay.post(order, logGuid, tradeCode, ref isQrcodeBuild, ref logMessage);
                    if (isQrcodeBuild)
                    {
                        //支付方式 (名字带有【扫码】【二维码】的方式，返回的payUrl鏈接要自已主动生成二维码！)
                        if (!string.IsNullOrEmpty(returnUrl))
                        {
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
                            return File(buff, "image/jpeg");
                        }
                        return Content(ErrorPage(outMessage));
                    }
                }
                else if (order.GWID == (int)PayGatewayType.ZhaoCaiBao)
                {
                    returnUrl = ZhaoCaiBao.post(order, logGuid, tradeCode, ref outMessage);                    

                }
                else if (order.GWID == (int)PayGatewayType.JBPay)
                {
                    returnUrl = JBPay.post(order, logGuid, tradeCode);
                }
                else if (order.GWID == (int)PayGatewayType.StarPay)
                {
                    returnUrl = StarPay.post(order, logGuid, tradeCode);
                  
                }
                else if (order.GWID == (int)PayGatewayType.JuGuangPay)
                {
                    returnUrl = JuGuangPay.post(order, logGuid, tradeCode);

                }
                else if (order.GWID == (int)PayGatewayType.ZhongChengPay)
                {
                    returnUrl = ZhongChengPay.post(order, logGuid, tradeCode);
                }
                else if (order.GWID == (int)PayGatewayType.ZhongChengPay)
                {
                    returnUrl = ZhongChengPay.post(order, logGuid, tradeCode);
                }
                else if (order.GWID == (int)PayGatewayType.OKPay)
                {
                    returnUrl = OKPay.post(order, logGuid, tradeCode);
                }
                else if (order.GWID == (int)PayGatewayType.BDPay)
                {
                    returnUrl = BDPay.post(order, logGuid, tradeCode);
                }
                else if (order.GWID == (int)PayGatewayType.DongFangPay)
                {
                    var isQrcodeBuild = false;
                    returnUrl = DongFangPay.post(order, logGuid, tradeCode, ref isQrcodeBuild);
                    if (isQrcodeBuild)
                    {
                        //支付方式 (名字带有【扫码】【二维码】的方式，返回的payUrl鏈接要自已主动生成二维码！)
                        if (!string.IsNullOrEmpty(returnUrl))
                        {
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
                            return File(buff, "image/jpeg");
                        }
                        return Content(ErrorPage(outMessage));
                    }
                }
                else if (order.GWID == (int)PayGatewayType.ShouMiPay)
                {
                    returnUrl = ShouMiPay.post(order, logGuid, tradeCode);
                }
                else if (order.GWID == (int)PayGatewayType.DuoBaoPay)
                {
                    returnUrl = DuoBaoPay.post(order, logGuid, tradeCode);
                }
                else if (order.GWID == (int)PayGatewayType.Xingyao)
                {
                    returnUrl = Xingyao.post(order, logGuid, tradeCode, ref logMessage);                  
                }
                else if (order.GWID == (int)PayGatewayType.Chunqiu)
                {
                    returnUrl = Chunqiu.post(order, logGuid, tradeCode, ref logMessage);
                }
                else if (order.GWID == (int)PayGatewayType.BeifuPay)
                {                  
                    returnUrl = BeifuPay.post(order, logGuid, tradeCode, ref outMessage);              
                }
                else if (order.GWID == (int)PayGatewayType.LivePay)
                {
                    returnUrl = LivePay.post(order, logGuid, tradeCode, ref outMessage);
                }
                else if (order.GWID == (int)PayGatewayType.WeiChuang)
                {
                    returnUrl = WeiChuang.post(order, logGuid, tradeCode, ref outMessage);
                }
                else if (order.GWID == (int)PayGatewayType.JZPay)
                {
                    returnUrl = JZPay.post(order, logGuid, tradeCode, ref logMessage);
                }
                else if (order.GWID == (int)PayGatewayType.Wangwangbao)
                {
                    returnUrl = Wangwangbao.post(order, logGuid, tradeCode, ref logMessage);
                }
                else if (order.GWID == (int)PayGatewayType.Easypay)
                {
                    returnUrl = Easypay.post(order, logGuid, tradeCode, ref logMessage);
                }
                else if (order.GWID == (int)PayGatewayType.Yilian)
                {
                    returnUrl = Yilian.post(order, logGuid, tradeCode);
                }
                else if (order.GWID == (int)PayGatewayType.Diandian)
                {
                    returnUrl = Diandian.post(order, logGuid, tradeCode, ref logMessage);
                }
                else if (order.GWID == (int)PayGatewayType.OPay)
                {
                    returnUrl = OPay.post(order, logGuid, tradeCode, ref logMessage);
                }
                else if (order.GWID == (int)PayGatewayType.MonsterPay)
                {
                    returnUrl = MonsterPay.post(order, logGuid, tradeCode);
                }



                    //顯示錯誤訊息
                if (returnUrl.Length == 0)
                {   
                    SiteHelper.Log(logGuid, outMessage, defaultlog);
                    return Content(ErrorPage(logMessage));
                }
                //跳轉支付平台
                return Content("<html><body><script language='javascript'>location.replace('" + returnUrl + "');</script></body></html>");

            }
            catch (Exception e)
            {
                logMessage = "系統錯誤";
                SiteHelper.Log(logGuid, $"{logMessage}({e.ToString()})", defaultlog);

                return Content(ErrorPage(logMessage));
            }
        }

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
        /// 申请支付金额接口
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult getamountlist()
        {
            if (SiteHelper.PaymentEnable == "0") return Content("");
            return Content(tungyi.getamountlist(logGuid));
        }

        [HttpGet]
        public ActionResult getamountlistByHonda()
        {
            if (SiteHelper.PaymentEnable == "0") return Content("");
            return Content(tungyi.getamountlistByHonda(logGuid));
        }

        public ActionResult getamountlistByDF()
        {
            if (SiteHelper.PaymentEnable == "0") return Content("");
            return Content(tungyi.getamountlistByDF(logGuid));
        }

        /// <summary>
        /// 申请支付金额接口
        /// </summary>
        /// <returns></returns>   
        [OutputCache(Duration = 15, VaryByParam = "id")]
        public ActionResult amountlist(int id)
        {
            if (SiteHelper.PaymentEnable == "0") return Content("");
            return Content(tungyi.amountlist(logGuid, id));
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
        /// 轰达冬瓜支付
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
        /// 巅峰冬瓜支付
        /// </summary>
        /// <returns></returns>
        public ActionResult DFXiamipayCall()
        {
            var state = DFXiamipay.call(logGuid);
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
        public ActionResult FytxPayCall()
        {
            var state = FytxPay.call(logGuid);
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
        /// 惠诚支付
        /// </summary>
        /// <returns></returns>
        public ActionResult HuiChengPayCall()
        {
            var state = HuiChengPay.call(logGuid);
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
        /// 新金鑫支付
        /// </summary>
        /// <returns></returns>
        public ActionResult JinxinPayCall()
        {
            var state = JinxinPay.call(logGuid);

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
        /// 怪兽支付
        /// </summary>
        /// <returns></returns>
        public ActionResult MonsterPayCall()
        {
            var state = MonsterPay.call(logGuid);
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
        /// 多聚发支付
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
        /// 久付支付
        /// </summary>
        /// <returns></returns>
        public ActionResult EightzPayCall()
        {
            var state = EightZpay.call(logGuid);
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
        /// 訊科支付
        /// </summary>
        /// <returns></returns>
        public ActionResult XKPayCall()
        {
            var state = XKPay.call(logGuid);
            if (state == PayState.Success)
            {
                return Content("opstate=0");
            }
            else
            {
                return Content("error");
            }
        }

        /// <summary>
        /// 盛世支付
        /// </summary>
        /// <returns></returns>
        public ActionResult ShengShiCall()
        {
            var state = ShengShi.call(logGuid);

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
        /// 融海支付
        /// </summary>
        /// <returns></returns>
        public ActionResult RongHaiCall()
        {
            var state = RongHai.call(logGuid);

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
        /// 完美支付
        /// </summary>
        /// <returns></returns>
        public ActionResult PerfectPayCall()
        {
            var state = PerfectPay.call(logGuid);

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
        /// 诚易玖支付
        /// </summary>
        /// <returns></returns>
        public ActionResult ChengYiJiuCall()
        {
            var state = ChengYiJiu.call(logGuid);

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
        /// 68支付
        /// </summary>
        /// <returns></returns>
        public ActionResult LiuBaCall()
        {
            var state = LiuBa.call(logGuid);

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
        /// 澄支付
        /// </summary>
        /// <returns></returns>
        public ActionResult SumiPayCall()
        {
            var state = SumiPay.call(logGuid);

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
        /// 星耀支付
        /// </summary>
        /// <returns></returns>
        public ActionResult XingyaoPayCall()
        {
            var state = Xingyao.call(logGuid);

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
        /// 聚光支付
        /// </summary>
        /// <returns></returns>
        public ActionResult JuGuangPayCall()
        {
            var state = JuGuangPay.call(logGuid);

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
        /// 众诚支付
        /// </summary>
        /// <returns></returns>
        public ActionResult ZhongChengPayCall()
        {
            var state = ZhongChengPay.call(logGuid);

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
        /// 星支付
        /// </summary>
        /// <returns></returns>
        public ActionResult StarPayCall()
        {
            var state = StarPay.call(logGuid);

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
        /// 火星支付
        /// </summary>
        /// <returns></returns>
        public ActionResult BitexPayCall()
        {
            var state = BitexPay.call(logGuid);

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
        /// 聚宝支付
        /// </summary>
        /// <returns></returns>
        public ActionResult JBPayCall()
        {
            var state = JBPay.call(logGuid);

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
        /// 东方闪付支付
        /// </summary>
        /// <returns></returns>
        public ActionResult QuickPayCall()
        {
            var state = QuickPay.call(logGuid);
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
        /// 多宝支付
        /// </summary>
        /// <returns></returns>
        public ActionResult DuoBaoPayCall()
        {
            var state = DuoBaoPay.call(logGuid);
           
            if (state == PayState.Success)
            {
                return Content("opstate=0");
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
        /// 金发回调
        /// </summary>
        /// <returns></returns>
        public ActionResult JFPayCall()
        {
            var state = JFPay.call(logGuid);

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
        /// 天猫支付
        /// </summary>
        /// <returns></returns>
        public ActionResult TMAllPayCall()
        {
            var state = TMAllPay.call(logGuid);
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
        /// 东方支付
        /// </summary>
        /// <returns></returns>
        public ActionResult DongFangPayCall()
        {
            var state = DongFangPay.call(logGuid);
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
        /// 贝富支付
        /// </summary>
        /// <returns></returns>
        public ActionResult BeifuPayCall()
        {
            var state = BeifuPay.call(logGuid);
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
        /// 实时支付
        /// </summary>
        /// <returns></returns>
        public ActionResult LivePayCall()
        {
            var state = LivePay.call(logGuid);
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
        /// 唯创支付
        /// </summary>
        /// <returns></returns>
        public ActionResult WeiChuangCall()
        {
            var state = WeiChuang.call(logGuid);
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
        /// 小美支付
        /// </summary>
        /// <returns></returns>
        public ActionResult XiaoMeiCall()
        {
            var state = XiaoMei.call(logGuid);
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
        /// 金钻支付
        /// </summary>
        /// <returns></returns>
        public ActionResult JZPayCall()
        {
            var state = JZPay.call(logGuid);
            if (state == PayState.Success)
            {
                return Json(new
                {
                    code = 0,
                    message = "ok",
                    data = string.Empty
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new
                {
                    code = 0,
                    message = "error",
                    data = string.Empty
                }, JsonRequestBehavior.AllowGet);
            }
        }
        
    /// <summary>
    /// 星坤支付RH
    /// </summary>
    /// <returns></returns>
    public ActionResult XingKunRHCall()
        {
            var state = XingKunRH.call(logGuid);
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
        /// 星坤支付YH
        /// </summary>
        /// <returns></returns>
        public ActionResult XingKunYHCall()
        {
            var state = XingKunYH.call(logGuid);
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
        /// 星坤支付RH
        /// </summary>
        /// <returns></returns>
        public ActionResult XingKunXKCall()
        {
            var state = XingKunXK.call(logGuid);
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
        /// 高盛支付
        /// </summary>
        /// <returns></returns>
        public ActionResult GaoShengCall()
        {
            var state = GaoSheng.call(logGuid);
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
        /// 招财宝
        /// </summary>
        /// <returns></returns>
        public ActionResult ZhaoCaiBaoCall()
        {
            var state = ZhaoCaiBao.call(logGuid);
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
        /// 收米支付
        /// </summary>
        /// <returns></returns>
        public ActionResult ShouMiPayCall()
        {
            var state = ShouMiPay.call(logGuid);
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
        /// BDPay支付
        /// </summary>
        /// <returns></returns>
        public ActionResult BDPayCall()
        {
            var state = BDPay.call(logGuid);
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
        /// OKPay支付
        /// </summary>
        /// <returns></returns>
        public ActionResult OKPayCall()
        {
            var state = OKPay.call(logGuid);
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

        /// <summary>
        /// 永旺支付
        /// </summary>
        /// <returns></returns>
        public ActionResult YongwangCall()
        {
            var state = Yongwang.call(logGuid);
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
        /// 佰富支付
        /// </summary>
        /// <returns></returns>
        public ActionResult BaifuCall()
        {
            var state = Baifu.call(logGuid);
            if (state == PayState.Success)
            {
                return Content("000000");
            }
            else
            {
                return Content("error");
            }
        }
        /// <summary>
        /// 佰富支付
        /// </summary>
        /// <returns></returns>
        public ActionResult QiShengPayCall()
        {
            var state = QiShengPay.call(logGuid);
            if (state == PayState.Success)
            {
                return Content("Success");
            }
            else
            {
                return Content("error");
            }
        }

        /// <summary>
        /// 轰达統一
        /// </summary>
        /// <returns></returns>
        public ActionResult HondaCall()
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
        /// 畅支付
        /// </summary>
        /// <returns></returns>
        public ActionResult ZumireCall()
        {
            var state = Zumire.call(logGuid);
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
        /// 翔优支付
        /// </summary>
        /// <returns></returns>
        public ActionResult XiangyouCall()
        {
            var state = Xiangyou.call(logGuid);
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
        /// 中邦支付
        /// </summary>
        /// <returns></returns>
        public ActionResult ZhongbangCall()
        {
            var state = Zhongbang.call(logGuid);
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
        /// 新全付支付
        /// </summary>
        /// <returns></returns>
        public ActionResult XinQuanFuCall()
        {
            var state = XinQuanFu.call(logGuid);
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
        /// 葡萄支付
        /// </summary>
        /// <returns></returns>
        public ActionResult PutaoCall()
        {
            var state = Putao.call(logGuid);
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
        /// 春秋支付
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ChunqiuCall()
        {

            var state = Chunqiu.call(logGuid);

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
        /// 旺旺宝支付
        /// </summary>
        /// <returns></returns>
        public ActionResult WangwangbaoCall()
        {
            var state = Wangwangbao.call(logGuid);
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
        /// 易收支付
        /// </summary>
        /// <returns></returns>
        public ActionResult EasypayCall()
        {
            var state = Easypay.call(logGuid);
            if (state == PayState.Success)
            {
                return Json("success");
            }
            else
            {
                return Json("error");
            }
        }

        /// <summary>
        /// 易联支付
        /// </summary>
        /// <returns></returns>
        public ActionResult YilianPayCall()
        {
            var state = Yilian.call(logGuid);
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
        /// 点点支付
        /// </summary>
        /// <returns></returns>
        public ActionResult DiandianCall()
        {
            var state = Diandian.call(logGuid);
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
        /// opay支付
        /// </summary>
        /// <returns></returns>
        public ActionResult OPayCall()
        {
            var state = OPay.call(logGuid);
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
        /// Gash回調
        /// </summary>
        /// <returns></returns>
        public ActionResult GashCall()
        {
            var state = Gash.call(logGuid);
            if (state == PayState.Success)
            {
                Response.Redirect("~/Content/success.html");
            }
            else
            {
                return View("Error");
            }
            return Content("");
        }

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

    }
}