using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using game.Entity.DTO.ThirdPartyPay;
using game.Entity.Payment;
using game.WebSite;
using game.WebSite.PaymentHelper;
using game.WebSite.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Web;
using game.Entity.Enums.ThirdPartyPay;
using game.Infrastructure.Extensions;
using System.Linq;

namespace game.WebSiteTests.Service
{
    public static class ThirdPartyPayment_TestDataGenerator
    {
        public static CDPayOrder GetDefaultOrder(int amount = 100)
        {
            CDPayOrder order = new CDPayOrder();
            order.Id = 1;
            order.UserID = 1;
            order.IPAddress = "未知IP";
            order.Amount = amount;
            return order;
        }
        public static CDMerchantUUID GetDefaultMerchant(string bankCode = "ZHIFUBAO_QR")
        {
            CDMerchantUUID merchant = new CDMerchantUUID();
            merchant.BankCode = bankCode;
            merchant.GWID = 1;
            return merchant;
        }

        public static string GetDefaultInfoData(string merchantNo, string merchantKey)
        {
            var result = new
            {
                MERCHANT_NO = merchantNo,
                MERCHANT_KEY = merchantKey
            };
            string infoData = JsonConvert.SerializeObject(result).ToString();
            return infoData;
        }

        public static string GetPayConfigUrl(string thirdPartyPayUrlKey)
        {
            string PayCenterDoamin = ConfigurationManager.AppSettings["PayCenter_Doamin"].ToString();
            string url = ConfigurationManager.AppSettings[thirdPartyPayUrlKey].ToString();
            return PayCenterDoamin + url;
        }
    }

    [TestClass]
    public class GiFuBoPayBaseSettingTest
    {
        [TestMethod]
        public void GiFuBoPayBaseSettingConstructorsTest()
        {
            var result = new { MERCHANT_NO = "123456", MERCHANT_KEY = "ABCDEF" };
            var dataJsonString = JsonConvert.SerializeObject(result).ToString();
            string Url = "http://0.0.0.0";

            var obj = new GiFuBoPayBaseSetting(dataJsonString, Url);
            string testUrl = obj.GetBaseUrl("test");
            string chkTestUrl = "http://0.0.0.0/test";

            Assert.AreEqual(obj.MerchantNo, "123456");
            Assert.AreEqual(obj.Key, "ABCDEF");
            Assert.AreEqual(testUrl, chkTestUrl);
        }
    }

    [TestClass]
    public class GiFuBoPayOrderTest
    {
        private GiFuBoPayOrder GetNewObj()
        {
            var result = new { MERCHANT_NO = "123456", MERCHANT_KEY = "ABCDEF" };
            var dataJsonString = JsonConvert.SerializeObject(result).ToString();
            string Url = "http://0.0.0.0";
            var payBaseSetting = new GiFuBoPayBaseSetting(dataJsonString, Url);

            var payOrder = new GiFuBoPayOrder(payBaseSetting);

            return payOrder;
        }

        private CDPayOrder GetDefaultOrder()
        {
            CDPayOrder order = new CDPayOrder();
            order.Id = 1;
            order.UserID = 1;
            order.Amount = 10;
            return order;
        }

        private CDMerchantUUID GetDefaultMerchant()
        {
            CDMerchantUUID merchant = new CDMerchantUUID();
            merchant.BankCode = "ZHIFUBAO_QR";
            merchant.GWID = 1;
            return merchant;
        }

        [TestMethod]
        public void GetSendUrlTest()
        {
            /*
            //支付宝扫码支付
            Url.Add("ZHIFUBAO_QR", "AliQRCodePayment.php");
            //支付宝H5支付
            Url.Add("ZHIFUBAO_H5", "AliH5Payment.php");
            //微信扫码支付
            Url.Add("WEIXIN_QR", "WechatQRCodePayment.php");
            //云闪付扫码支付
            Url.Add("QUICKPASS_QR", "UnionYSFQRCodePayment.php");
             */

            string payType = null;
            string sendUrl;
            string sendUrlChk = string.Empty;
            var order = GetDefaultOrder();

            var payOrder1 = GetNewObj();
            payOrder1.SetOrderData(order);
            sendUrl = payOrder1.GetSendUrl(payType);
            Assert.AreEqual(sendUrl, sendUrlChk);

            var payOrder2 = GetNewObj();
            payOrder2.SetOrderData(order);
            sendUrl = payOrder2.GetSendUrl("ZHIFUBAO_QR");
            sendUrlChk = "AliQRCodePayment.php";
            Assert.AreEqual(sendUrl, sendUrlChk);

            var payOrder3 = GetNewObj();
            payOrder3.SetOrderData(order);
            sendUrl = payOrder3.GetSendUrl("ZHIFUBAO_H5");
            sendUrlChk = "AliH5Payment.php";
            Assert.AreEqual(sendUrl, sendUrlChk);

            var payOrder4 = GetNewObj();
            payOrder4.SetOrderData(order);
            sendUrl = payOrder4.GetSendUrl("WEIXIN_QR");
            sendUrlChk = "WechatQRCodePayment.php";
            Assert.AreEqual(sendUrl, sendUrlChk);

            var payOrder5 = GetNewObj();
            payOrder5.SetOrderData(order);
            sendUrl = payOrder5.GetSendUrl("QUICKPASS_QR");
            sendUrlChk = "UnionYSFQRCodePayment.php";
            Assert.AreEqual(sendUrl, sendUrlChk);
        }

        [TestMethod]
        public void GetTransNoTest()
        {
            string transNo;
            string transNoChk;
            CDPayOrder order = GetDefaultOrder();
            var payOrder1 = GetNewObj();
            payOrder1.SetOrderData(order);
            transNo = payOrder1.GetTransNo();
            transNoChk = "GiFuBoPay00000000001";
            Assert.AreEqual(transNo, transNoChk);

            //測試更改order
            order = new CDPayOrder();
            order.Id = 1234567890;
            order.UserID = 1;
            order.Amount = 10;

            var payOrder2 = GetNewObj();
            payOrder2.SetOrderData(order);
            transNo = payOrder2.GetTransNo();
            transNoChk = "GiFuBoPay01234567890";
            Assert.AreEqual(transNo, transNoChk);
        }

        [TestMethod]
        public void GetPostJsonStringTest()
        {
            string MerID = "123456";
            string SignKey = "ABCDEF";
            string NotifyUrl = ThirdPartyPayment_TestDataGenerator.GetPayConfigUrl("pay_GiFuBoPay_notifyurl");
            CDPayOrder order = GetDefaultOrder();
            CDMerchantUUID merchant = GetDefaultMerchant();
            string Sign;
            string jsonString;
            string jsonStringChk;

            var payOrder1 = GetNewObj();
            payOrder1.SetOrderData(order);
            payOrder1.GetSendUrl(merchant.BankCode);
            payOrder1.Done();

            string ToMd5String = MerID + "GiFuBoPay00000000001" + order.Id.ToString() + order.UserID.ToString() + order.Amount.ToString("F2") + SignKey;
            Sign = MD5Service.Hash(ToMd5String, Encoding.UTF8).ToLower();

            var result = new
            {
                MerID = MerID,
                MerTradeID = "GiFuBoPay00000000001",
                MerProductID = order.Id.ToString(),
                MerUserID = order.UserID.ToString(),
                Amount = order.Amount.ToString("F2"),
                TradeDesc = merchant.BankCode,
                ItemName = merchant.BankCode,
                NotifyUrl = NotifyUrl,
                Sign = Sign
            };
            jsonStringChk = JsonConvert.SerializeObject(result).ToString();

            jsonString = payOrder1.GetPostJsonString();
            Assert.AreEqual(jsonString, jsonStringChk);

        }
    }

    [TestClass]
    public class THPayTest
    {
        private THPayBaseSetting GetDefaultTHPayBaseSetting()
        {
            string infoData = ThirdPartyPayment_TestDataGenerator.GetDefaultInfoData("1938348jK2", "2c59f8e397eb437ea43315b7acc86689");
            string url = "http://example.invalid:8848/qrpay/pay";

            var PayBaseSetting = new THPayBaseSetting(infoData, url);
            return PayBaseSetting;
        }

        [TestMethod]
        public void THPayOrderTest()
        {
            var order = ThirdPartyPayment_TestDataGenerator.GetDefaultOrder(100);
            var merchant = ThirdPartyPayment_TestDataGenerator.GetDefaultMerchant("ZHIFUBAO_QR");
            var payBaseSetting = GetDefaultTHPayBaseSetting();
            string notify_url = ThirdPartyPayment_TestDataGenerator.GetPayConfigUrl("pay_THPay_notifyurl");
            string return_url = ThirdPartyPayment_TestDataGenerator.GetPayConfigUrl("pay_finishUrl");

            var payOrder = new THPayOrder(payBaseSetting);
            payOrder.SetPayType(merchant.BankCode);
            payOrder.SetOrderData(order);
            payOrder.Done();

            //測試交易單號
            string transNo = payOrder.GetThirdPartyOderID();
            string transNoChk = "THPay000000000000001";
            Assert.AreEqual(transNo, transNoChk);

            //測試字串md5之後將存成sign
            string md5String = payOrder.GetMd5String();
            string md5StringChk = "1938348jK2100.0011" + notify_url + return_url + "THPay0000000000000012c59f8e397eb437ea43315b7acc86689";
            Assert.AreEqual(md5String, md5StringChk);

            //測試post json string
            var postObj = new {
                uid = "1938348jK2",
                price = "100.00",
                paytype = 11,
                notify_url = notify_url,
                return_url = return_url,
                user_order_no = transNo,
                note = order.Id.ToString(),
                cuid = order.UserID.ToString(),
                tm = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                sign = MD5Service.Hash(md5String, Encoding.UTF8).ToLower()
            };
            string postJsonChk = JsonConvert.SerializeObject(postObj).ToString();
            string postJson = payOrder.GetPostJsonString();
            Assert.AreEqual(postJson, postJsonChk);
        }

        [TestMethod]
        public void THPayResponseTest()
        {
            var order = ThirdPartyPayment_TestDataGenerator.GetDefaultOrder(100);
            var merchant = ThirdPartyPayment_TestDataGenerator.GetDefaultMerchant("ZHIFUBAO_QR");
            var payBaseSetting = GetDefaultTHPayBaseSetting();

            var payOrder = new THPayOrder(payBaseSetting);
            payOrder.SetPayType(merchant.BankCode);
            payOrder.SetOrderData(order);
            payOrder.Done();
            var payOrderModel = payOrder.GetPayOrderModel();

            string md5String1 = payOrderModel.user_order_no + "123456" + "456789" + payOrderModel.price + payOrderModel.price + payBaseSetting.Key;
            string md5String2 = payOrderModel.user_order_no + "123456" + "456789" + payOrderModel.price + payOrderModel.price + "3" + payBaseSetting.Key;

            var result = new
            {
                user_order_no = payOrderModel.user_order_no,
                orderno = "123456",
                tradeno = "456789",
                price = payOrderModel.price,
                realprice = payOrderModel.price,
                cuid = payOrderModel.cuid,
                note = payOrderModel.note,
                status = 3,
                sign = MD5Service.Hash(md5String1, Encoding.UTF8).ToLower(),
                sign2 = MD5Service.Hash(md5String2, Encoding.UTF8).ToLower()
            };

            string returnJson = JsonConvert.SerializeObject(result).ToString();

            var payResponse = new THPayResponse(returnJson);

            //測試確認回調是否成功
            Assert.AreEqual(payResponse.IsSuccess(), true);

            //測試資料庫訂單ID是否正確
            int orderID = payResponse.GetOrderID();
            int orderIDChk = order.Id;
            Assert.AreEqual(orderID, orderIDChk);

            //測試取得交易訂單ID(給第三方的訂單ID)
            string thirdPartyOderID = payResponse.GetThirdPartyOderID();
            string thirdPartyOderIDChk = payOrderModel.user_order_no;
            Assert.AreEqual(thirdPartyOderID, thirdPartyOderIDChk);

            //測試金額
            long amount = payResponse.GetPayAmount();
            long amountChk = (long)Decimal.Parse(payOrderModel.price);
            Assert.AreEqual(amount, amountChk);

            //測試sign是否正確
            Assert.AreEqual(payResponse.IsSignTrue(payBaseSetting), true);

            //取得驗證簽名MD5使用字串
            string md5String = payResponse.GetReturnstringToMd5();
            Assert.AreEqual(md5String, md5String2);
        }
    }

    [TestClass]
    public class IronPayTest
    {
        private IronPayBaseSetting GetDefaultIronPayBaseSetting()
        {
            var result = new
            {
                MERCHANT_NO = "847500002507403",
                MERCHANT_KEY = "4d9c4618d3ac0737ed9ea69ab0b4ec23",
                MERCHANT_USERID = "8800451",
                MERCHANT_USERNAME = "飞皇娱乐2"
            };
            string infoData = JsonConvert.SerializeObject(result).ToString();
            string url = "http://a.ironpay.tech";

            var PayBaseSetting = new IronPayBaseSetting(infoData, url);
            return PayBaseSetting;
        }

        private IronPayOrder GetDefaultIronPayOrder()
        {
            var order = ThirdPartyPayment_TestDataGenerator.GetDefaultOrder(10);
            var merchant = ThirdPartyPayment_TestDataGenerator.GetDefaultMerchant("ZHIFUBAO_H5");
            var payBaseSetting = GetDefaultIronPayBaseSetting();
            
            var payOrder = new IronPayOrder(payBaseSetting);
            payOrder.SetPayType(merchant.BankCode);
            payOrder.SetOrderData(order);
            payOrder.Done();

            return payOrder;
        }

        private string GetDefaultIronPayRetrunJsonString()
        {
            var result = new
            {
                sign = "5FA376C450F7EC44F4FB23F46059F0FA",
                message = "请求成功",
                status = "SUCCESS",
                payUrl = "http://example.invalid/#/?sysid=C20190924043125405",
                merchantNo = "847500002507403"
            };
            string payReturnJsonString = JsonConvert.SerializeObject(result).ToString();
            return payReturnJsonString;
        }

        [TestMethod]
        public void IronPayOrderTest()
        {
            string notify_url = ThirdPartyPayment_TestDataGenerator.GetPayConfigUrl("pay_IronPay_notifyurl");
            string return_url = ThirdPartyPayment_TestDataGenerator.GetPayConfigUrl("pay_finishUrl");
            var payOrder = GetDefaultIronPayOrder();

            //測試交易單號
            string thirdPartyOderID = payOrder.GetThirdPartyOderID();
            string thirdPartyOderIDChk = "IronPay0000000000001";
            Assert.AreEqual(thirdPartyOderID, thirdPartyOderIDChk);

            //測試字串md5之後將存成sign
            string md5String = payOrder.GetMd5String();
            string md5StringChk = "body=1&channel=Android&merchantNo=847500002507403&notifyUrl=" + notify_url + "&orderMoney=1000&orderNo=IronPay0000000000001&returnUrl=" + return_url + "&source=4&subject=1&userId=8800451&userName="+ MD5Service.Hash("飞皇娱乐2", Encoding.UTF8).ToUpper() + "&key=4d9c4618d3ac0737ed9ea69ab0b4ec23";
            Assert.AreEqual(md5String, md5StringChk);

            //測試post json string
            var postObj = new
            {
                merchantNo = "847500002507403",
                source = "4",
                subject = "1",
                body = "1",
                channel = "Android",
                orderNo = "IronPay0000000000001",
                orderMoney = "1000",
                returnUrl = return_url,
                notifyUrl = notify_url,
                userId = "8800451",
                userName = MD5Service.Hash("飞皇娱乐2", Encoding.UTF8).ToUpper(),
                sign = MD5Service.Hash(md5String, Encoding.UTF8).ToUpper()
            };
            string postJsonChk = JsonConvert.SerializeObject(postObj).ToString();
            string postJson = payOrder.GetPostJsonString();
            Assert.AreEqual(postJson, postJsonChk);
        }
        
        [TestMethod]
        public void PayReturnTest()
        {
            var payBaseSetting = GetDefaultIronPayBaseSetting();
            string payReturnJsonString = GetDefaultIronPayRetrunJsonString();
            var payReturn = new IronPayReturn(payReturnJsonString);

            //測試是否成功
            Assert.AreEqual(payReturn.IsSuccess(), true);

            //測試回傳網址
            Assert.AreEqual(payReturn.GetReturnUrl(), "http://example.invalid/#/?sysid=C20190924043125405");

            //測試檢查sign
            Assert.AreEqual(payReturn.IsSignTrue(payBaseSetting), true);
        }

        [TestMethod]
        public void PayResponseTest()
        {
            var payBaseSetting = GetDefaultIronPayBaseSetting();
            var payOrder = GetDefaultIronPayOrder();
            var payOrderModel = payOrder.GetPayOrderModel();

            string md5StringChk = "merchantNo="+ payBaseSetting.MerchantNo + "&message=请求成功&orderMoney="+ payOrderModel.orderMoney + "&orderNo="+ payOrderModel.orderNo + "&status=SUCCESS&key=" + payBaseSetting.Key;
            string sign = MD5Service.Hash(md5StringChk, Encoding.UTF8).ToUpper();
            var result = new
            {
                sign = sign,
                message = "请求成功",
                status = "SUCCESS",
                merchantNo = "847500002507403",
                orderNo = payOrderModel.orderNo,
                orderMoney = payOrderModel.orderMoney
            };
            string payReturnJsonString = JsonConvert.SerializeObject(result).ToString();

            var payResponse = new IronPayResponse(payReturnJsonString);

            //測試確認回調是否成功
            Assert.AreEqual(payResponse.IsSuccess(), true);

            //測試資料庫訂單ID是否正確
            int orderID = payResponse.GetOrderID();
            int orderIDChk = 1;
            Assert.AreEqual(orderID, orderIDChk);

            //測試取得交易訂單ID(給第三方的訂單ID)
            string thirdPartyOderID = payResponse.GetThirdPartyOderID();
            string thirdPartyOderIDChk = payOrderModel.orderNo;
            Assert.AreEqual(thirdPartyOderID, thirdPartyOderIDChk);

            //測試金額
            long amount = payResponse.GetPayAmount();
            long amountChk = Convert.ToInt64(payOrderModel.orderMoney)/100;
            Assert.AreEqual(amount, amountChk);

            //測試sign是否正確
            Assert.AreEqual(payResponse.IsSignTrue(payBaseSetting), true);

            //取得驗證簽名MD5使用字串
            string md5String = payResponse.GetReturnstringToMd5();
            Assert.AreEqual(md5String, md5StringChk);
        }
    }

    /// <summary>
    /// 四方支付(新王朝)
    /// </summary>
    [TestClass]
    public class HFZCPayTest
    {
        #region 測試用假資料
        private static string merchantNo = "API2027980026185171";
        private static string merchantKey = "81298d7f86a74518970ff8c51da3f8bc";
        private static int orderAmount = 10;

        private string dateTimeFormat = "yyyyMMddHHmmss";
        private string notifyUrl = ThirdPartyPayment_TestDataGenerator.GetPayConfigUrl("pay_HFZCPay_notifyurl");
        private string returnUrl = ThirdPartyPayment_TestDataGenerator.GetPayConfigUrl("pay_finishUrl");

        private CDPayOrder order = ThirdPartyPayment_TestDataGenerator.GetDefaultOrder(orderAmount);
        private CDMerchantUUID merchant = ThirdPartyPayment_TestDataGenerator.GetDefaultMerchant();

        private HFZCPayBaseSetting GetHFZCPayBaseSetting()
        {
            string url = "http://example.invalid:8084";
            string infoData = ThirdPartyPayment_TestDataGenerator.GetDefaultInfoData(merchantNo, merchantKey);
            HFZCPayBaseSetting baseSetting = new HFZCPayBaseSetting(infoData, url);
            return baseSetting;
        }
        #endregion

        [TestMethod]
        public void HFZCPayOrderTest()
        {
            var payBaseSetting = GetHFZCPayBaseSetting();
            var payOrder = new HFZCPayOrder(payBaseSetting);
            payOrder.SetPayType(merchant.BankCode);
            payOrder.SetOrderData(order);
            payOrder.Done();

            //測試第三方訂單號碼
            string thirdPartyOrderID = payOrder.GetThirdPartyOderID();
            Assert.AreEqual(thirdPartyOrderID, "HFZCPay0000000000001");

            //測試取得MD5前字串
            string md5String = payOrder.GetMd5String();
            string md5StringChk = $"bizAmt={orderAmount}&channleType=1&customNo={order.UserID}&date={DateTime.Now.ToString(dateTimeFormat)}&merchantNo={merchantNo}&noticeUrl={notifyUrl}&orderNo={thirdPartyOrderID}&returnUrl={returnUrl}&signType=MD5&version=V2{merchantKey}";
            Assert.AreEqual(md5String, md5StringChk);

            //測試InputJson
            var postObj = new
            {
                version = "V2",
                signType = "MD5",
                merchantNo = merchantNo,
                date = DateTime.Now.ToString(dateTimeFormat),
                channleType = 1,
                sign = MD5Service.Hash(md5String, Encoding.UTF8).ToLower(),
                orderNo = thirdPartyOrderID,
                bizAmt = orderAmount,
                returnUrl = returnUrl,
                noticeUrl = notifyUrl,
                customNo = order.UserID
            };
            string postJsonChk = JsonConvert.SerializeObject(postObj).ToString();
            string postJson = payOrder.GetPostJsonString();
            Assert.AreEqual(postJson, postJsonChk);
        }

        [TestMethod]
        public void HFZCPayReturnTest()
        {
            //測試回傳值為URL格式
            var jsonObj = new
            {
                code = "0",
                msg = "OK",
                detail = new
                {
                    PayHtml = "",
                    PayURL = "https://payonline.asia/api/pay/2F3E9E9852682061BCF33A4BB0881D87"
                }
            };
            string payReturnJsonString = JsonConvert.SerializeObject(jsonObj).ToString();

            var payReturn = new HFZCPayReturn(payReturnJsonString);

            //測試是否成功
            Assert.AreEqual(payReturn.IsSuccess(), true);

            //測試回傳模式
            Assert.AreEqual(payReturn.GetReturnMehtod(), "URL");

            //測試回傳HTML
            Assert.AreEqual(payReturn.GetReturnHtml(), jsonObj.detail.PayURL);

            //測試回傳值為Form格式
            jsonObj = new
            {
                code = "0",
                msg = "OK",
                detail = new
                {
                    PayHtml = "<html><body></body></html>",
                    PayURL = ""
                }
            };
            payReturnJsonString = JsonConvert.SerializeObject(jsonObj).ToString();

            payReturn = new HFZCPayReturn(payReturnJsonString);

            //測試是否成功
            Assert.AreEqual(payReturn.IsSuccess(), true);

            //測試回傳模式
            Assert.AreEqual(payReturn.GetReturnMehtod(), "FORM");

            //測試回傳HTML
            Assert.AreEqual(payReturn.GetReturnHtml(), jsonObj.detail.PayHtml);

            //測試某種失敗訊息
            jsonObj = new
            {
                code = "-1",
                msg = "当前通道繁忙,请联系客服",
                detail = new
                {
                    PayHtml = "",
                    PayURL = ""
                }
            };
            payReturnJsonString = JsonConvert.SerializeObject(jsonObj).ToString();

            payReturn = new HFZCPayReturn(payReturnJsonString);

            //測試是否成功
            Assert.AreEqual(payReturn.IsSuccess(), false);

            //測試回傳模式
            Assert.AreEqual(payReturn.GetReturnMehtod(), "FORM");

            //測試回傳HTML
            Assert.AreEqual(payReturn.GetReturnHtml(), jsonObj.detail.PayHtml);
        }

        [TestMethod]
        public void PayResponseTest()
        {
            var payBaseSetting = GetHFZCPayBaseSetting();
            var payOrder = new HFZCPayOrder(payBaseSetting);
            payOrder.SetPayType(merchant.BankCode);
            payOrder.SetOrderData(order);
            payOrder.Done();
            var payOrderModel = payOrder.GetPayOrderModel();

            string md5StringChk = $"bizAmt={payOrderModel.bizAmt}&date={payOrderModel.date}&orderNo={payOrderModel.orderNo}&remark=请求成功&status=1&version=V2{merchantKey}";
            string sign = MD5Service.Hash(md5StringChk, Encoding.UTF8).ToLower();
            var payReturnJson = new
            {
                orderNo = payOrderModel.orderNo,
                bizAmt = payOrderModel.bizAmt,
                status = 1,
                remark = "请求成功",
                version = "V2",
                date = payOrderModel.date,
                sign = sign
            };
            string payReturnJsonString = JsonConvert.SerializeObject(payReturnJson).ToString();

            var payResponse = new HFZCPayResponse(payReturnJsonString);

            //測試確認回調是否成功
            Assert.AreEqual(payResponse.IsSuccess(), true);

            //測試資料庫訂單ID是否正確
            int orderID = payResponse.GetOrderID();
            int orderIDChk = 1;
            Assert.AreEqual(orderID, orderIDChk);

            //測試取得交易訂單ID(給第三方的訂單ID)
            string thirdPartyOderID = payResponse.GetThirdPartyOderID();
            string thirdPartyOderIDChk = payOrderModel.orderNo;
            Assert.AreEqual(thirdPartyOderID, thirdPartyOderIDChk);

            //測試金額
            long amount = payResponse.GetPayAmount();
            long amountChk = Convert.ToInt64(payOrderModel.bizAmt);
            Assert.AreEqual(amount, amountChk);

            //測試sign是否正確
            Assert.AreEqual(payResponse.IsSignTrue(payBaseSetting), true);

            //取得驗證簽名MD5使用字串
            string md5String = payResponse.GetReturnstringToMd5();
            Assert.AreEqual(md5String, md5StringChk);
        }
    }

    [TestClass]
    public class GoldSoarPayTest
    {
        #region 測試用假資料
        private static string merchantNo = "2303";
        private static string merchantKey = "5e70043022154e02a1c8905dd79e565d";
        private static string secretKey = "2d016c33b1d94afbaa9729607c9147cd";
        private static int orderAmount = 10;
        private int thirdPartyPayType = 20000203;

        private static string dateTimeFormat = "yyyyMMddHHmmss";
        private string dateTimeNow = DateTime.Now.ToString(dateTimeFormat);
        private string notifyUrl = ThirdPartyPayment_TestDataGenerator.GetPayConfigUrl("pay_GoldSoarPay_notifyurl");
        private string returnUrl = ThirdPartyPayment_TestDataGenerator.GetPayConfigUrl("pay_finishUrl");

        private CDPayOrder order = ThirdPartyPayment_TestDataGenerator.GetDefaultOrder(orderAmount);
        private CDMerchantUUID merchant = ThirdPartyPayment_TestDataGenerator.GetDefaultMerchant("ZHIFUBAO_H5");

        private GoldSoarPayBaseSetting GetGoldSoarPayBaseSetting()
        {
            var result = new
            {
                MERCHANT_NO = merchantNo,
                MERCHANT_KEY = merchantKey,
                SECRET_KEY = [REDACTED]
            };
            string infoData = JsonConvert.SerializeObject(result).ToString();

            string url = "http://example.invalid";
            GoldSoarPayBaseSetting baseSetting = new GoldSoarPayBaseSetting(infoData, url);
            return baseSetting;
        }
        #endregion

        [TestMethod]
        public void PayOrderTest()
        {
            var payBaseSetting = GetGoldSoarPayBaseSetting();
            var payOrder = new GoldSoarPayOrder(payBaseSetting);
            payOrder.SetPayType(merchant.BankCode);
            payOrder.SetOrderData(order);
            payOrder.Done();

            //測試第三方訂單號碼
            string thirdPartyOrderID = payOrder.GetThirdPartyOderID();
            Assert.AreEqual(thirdPartyOrderID, "GoldSoarPay000000001");

            //測試取得MD5前字串
            
            string md5String = payOrder.GetMd5String();
            string md5StringChk = $"notifyUrl={notifyUrl}&orderNo={thirdPartyOrderID}&orderPrice={orderAmount}&orderTime={dateTimeNow}&payKey={merchantKey}&productName={order.Id.ToString()}&productType={thirdPartyPayType}&returnUrl={returnUrl}&paySecret=[REDACTED]";
            Assert.AreEqual(md5String, md5StringChk);

            //測試Input字串
            string sign = MD5Service.Hash(md5String, Encoding.UTF8).ToUpper();
            string postInputChk = $"payKey={merchantKey}&orderNo={thirdPartyOrderID}&productType={thirdPartyPayType}&orderPrice={orderAmount}&orderTime={dateTimeNow}&productName={order.Id.ToString()}&returnUrl={returnUrl}&notifyUrl={notifyUrl}&sign={sign}";
            string postInput = payOrder.GetPostInputString();
            Assert.AreEqual(postInput, postInputChk);

            //測試InputJson
            var postObj = new
            {
                payKey = merchantKey,
                orderNo = thirdPartyOrderID,
                productType = thirdPartyPayType,
                orderPrice = orderAmount,
                orderTime = dateTimeNow,
                productName = order.Id.ToString(),
                returnUrl = returnUrl,
                notifyUrl = notifyUrl,
                sign = sign
            };
            string postJsonChk = JsonConvert.SerializeObject(postObj).ToString();
            string postJson = payOrder.GetPostJsonString();
            Assert.AreEqual(postJson, postJsonChk);
        }

        [TestMethod]
        public void PayReturnTest()
        {
            var payBaseSetting = GetGoldSoarPayBaseSetting();
            //測試回傳值為URL格式
            var jsonObj = new
            {
                respCode = "0000",
                respMsg = "success",
                payUrl = "http://0.0.0.0:9010/topay/index.jhtml?orderid=3d7098c5d759150c04496c58c2b4d937a150683f56575967",
                sign = "16FCF0CDCD4916A7AEDC9A9F87789507"
            };
            string payReturnJsonString = JsonConvert.SerializeObject(jsonObj).ToString();

            var payReturn = new GoldSoarPayReturn(payReturnJsonString);

            //測試是否成功
            Assert.AreEqual(payReturn.IsSuccess(), true);

            //測試回傳URL
            Assert.AreEqual(payReturn.GetReturnUrl(), jsonObj.payUrl);

            //測試sign是否正確
            Assert.AreEqual(payReturn.IsSignTrue(payBaseSetting), true);

            //測試MD5字串是否正確
            string md5StringChk = $"payUrl={jsonObj.payUrl}&respCode={jsonObj.respCode}&respMsg={jsonObj.respMsg}&paySecret=[REDACTED]";
            Assert.AreEqual(payReturn.GetMd5String(), md5StringChk);
        }

        [TestMethod]
        public void PayResponseTest()
        {
            var payBaseSetting = GetGoldSoarPayBaseSetting();
            var payOrder = new GoldSoarPayOrder(payBaseSetting);
            payOrder.SetPayType(merchant.BankCode);
            payOrder.SetOrderData(order);
            payOrder.Done();
            var payOrderModel = payOrder.GetPayOrderModel();

            string md5StringChk = $"orderPrice={payOrderModel.orderPrice}&orderTime={dateTimeNow}&outTradeNo={payOrderModel.orderNo}&payKey={payOrderModel.payKey}&productName={payOrderModel.productName}&productType={payOrderModel.productType}&successTime={dateTimeNow}&tradeStatus=SUCCESS&trxNo=123456&paySecret=[REDACTED]";
            string sign = MD5Service.Hash(md5StringChk, Encoding.UTF8).ToUpper();

            var payResponseModel = new GoldSoarPayResponseModel
            {
                payKey = payOrderModel.payKey,
                productName = payOrderModel.productName,
                orderPrice = payOrderModel.orderPrice.ToString(),
                outTradeNo = payOrderModel.orderNo,
                productType = payOrderModel.productType,
                orderTime = dateTimeNow,
                tradeStatus = "SUCCESS",
                successTime = dateTimeNow,
                trxNo = "123456",
                remark = string.Empty,
                sign = sign
            };

            var payResponse = new GoldSoarPayResponse(payResponseModel);

            //測試確認回調是否成功
            Assert.AreEqual(payResponse.IsSuccess(), true);

            //測試資料庫訂單ID是否正確
            int orderID = payResponse.GetOrderID();
            int orderIDChk = 1;
            Assert.AreEqual(orderID, orderIDChk);

            //測試取得交易訂單ID(給第三方的訂單ID)
            string thirdPartyOderID = payResponse.GetThirdPartyOderID();
            string thirdPartyOderIDChk = payOrderModel.orderNo;
            Assert.AreEqual(thirdPartyOderID, thirdPartyOderIDChk);

            //測試金額
            long amount = payResponse.GetPayAmount();
            long amountChk = Convert.ToInt64(payOrderModel.orderPrice);
            Assert.AreEqual(amount, amountChk);

            //測試sign是否正確
            Assert.AreEqual(payResponse.IsSignTrue(payBaseSetting), true);

            //取得驗證簽名MD5使用字串
            string md5String = payResponse.GetReturnstringToMd5();
            Assert.AreEqual(md5String, md5StringChk);
        }
    }

    [TestClass]
    public class JBPayTest
    {
        #region 測試用假資料
        private static string merchantNo = "tct109109";
        private static string merchantKey = "uqwzs8urZp0ce8qO";
        private static int orderAmount = 300;
        private string thirdPartyPayType = "ALI_QR";

        private string notifyUrl = ThirdPartyPayment_TestDataGenerator.GetPayConfigUrl("pay_JBPay_notifyurl");
        private string returnUrl = ThirdPartyPayment_TestDataGenerator.GetPayConfigUrl("pay_finishUrl");

        private CDPayOrder order = ThirdPartyPayment_TestDataGenerator.GetDefaultOrder(orderAmount);
        private CDMerchantUUID merchant = ThirdPartyPayment_TestDataGenerator.GetDefaultMerchant();

        private JBPayBaseSetting GetJBPayBaseSetting()
        {
            string infoData = ThirdPartyPayment_TestDataGenerator.GetDefaultInfoData(merchantNo, merchantKey);

            string url = "https://example.invalid";
            JBPayBaseSetting baseSetting = new JBPayBaseSetting(infoData, url);
            return baseSetting;
        }
        #endregion

        [TestMethod]
        public void PayOrderTest()
        {
            var payBaseSetting = GetJBPayBaseSetting();
            var payOrder = new JBPayOrder(payBaseSetting);
            payOrder.SetPayType(merchant.BankCode);
            payOrder.SetOrderData(order);
            payOrder.Done();

            //測試第三方訂單號碼
            string thirdPartyOrderID = payOrder.GetThirdPartyOderID();
            Assert.AreEqual(thirdPartyOrderID, "JBPay000000000000001");

            //測試取得MD5前字串

            string md5String = payOrder.GetMd5String();
            string md5StringChk = $"amount={orderAmount.ToString("F2")}&client_id={merchantNo}&key={merchantKey}&notify_url={notifyUrl}&order_id={thirdPartyOrderID}&return_url={returnUrl}&slug={thirdPartyPayType}";
            Assert.AreEqual(md5String, md5StringChk);

            //測試Input字串
            string sign = MD5Service.Hash(md5String, Encoding.UTF8).ToLower();
            string postInputChk = $"client_id={merchantNo}&slug={thirdPartyPayType}&order_id={thirdPartyOrderID}&amount={orderAmount.ToString("F2")}&notify_url={notifyUrl}&return_url={returnUrl}&sign={sign}";
            string postInput = payOrder.GetPostInputString();
            Assert.AreEqual(postInput, postInputChk);

            //測試InputJson
            var postObj = new
            {
                client_id = merchantNo,
                slug = thirdPartyPayType,
                order_id = thirdPartyOrderID,
                amount = orderAmount.ToString("F2"),
                notify_url = notifyUrl,
                return_url = returnUrl,
                sign = sign
            };
            string postJsonChk = JsonConvert.SerializeObject(postObj).ToString();
            string postJson = payOrder.GetPostJsonString();
            Assert.AreEqual(postJson, postJsonChk);
        }

        [TestMethod]
        public void PayReturnTest()
        {
            //-----------測試正確的回傳值-------------------------------------
            var jsonObj = new
            {
                code = 0,
                data = new
                {
                    order_id = "5d9495ba24473",
                    read_url = "https://example.invalid/pay_url/2207c098b945",
                    message = "交易遞延中"
                }
            };
            string payReturnJsonString = JsonConvert.SerializeObject(jsonObj).ToString();

            var payReturn = new JBPayReturn(payReturnJsonString);

            //測試是否成功
            Assert.AreEqual(payReturn.IsSuccess(), true);

            //測試回傳URL
            Assert.AreEqual(payReturn.GetReturnUrl(), jsonObj.data.read_url);

            //測試回傳訊息
            Assert.AreEqual(payReturn.GetMessage(), jsonObj.data.message);

            //------測試錯誤的回傳值----------------------------------------------
            var jsonObjError = new
            {
                code = "2X003",
                message = "\u5ba2\u6236\u4ea4\u6613\u65b9\u5f0f\u95dc\u9589"
            };

            payReturnJsonString = JsonConvert.SerializeObject(jsonObjError).ToString();

            payReturn = new JBPayReturn(payReturnJsonString);

            //測試是否成功
            Assert.AreEqual(payReturn.IsSuccess(), false);

            //測試回傳URL
            Assert.AreEqual(payReturn.GetReturnUrl(), string.Empty);

            //測試回傳訊息
            Assert.AreEqual(payReturn.GetMessage(), jsonObjError.message);
        }

        [TestMethod]
        public void PayResponseTest()
        {
            string thirdPartyPayOrderID = "JBPay0000000000000001";
            string amountString = orderAmount.ToString("F2");
            var payBaseSetting = GetJBPayBaseSetting();

            string md5StringChk = $"amount={amountString}&client_id={merchantNo}&key={merchantKey}&merchant_order={thirdPartyPayOrderID}";
            string sign = MD5Service.Hash(md5StringChk, Encoding.UTF8).ToLower();

            var payResponseModel = new JBPayResponseModel
            {
                client_id = merchantNo,
                merchant_order = thirdPartyPayOrderID,
                amount = amountString,
                attach = string.Empty,
                sign = sign
            };

            var payResponse = new JBPayResponse(payResponseModel);

            //測試資料庫訂單ID是否正確
            int orderID = payResponse.GetOrderID();
            int orderIDChk = 1;
            Assert.AreEqual(orderID, orderIDChk);

            //測試取得交易訂單ID(給第三方的訂單ID)
            string thirdPartyOderID = payResponse.GetThirdPartyOderID();
            Assert.AreEqual(thirdPartyOderID, thirdPartyPayOrderID);

            //測試金額
            long amount = payResponse.GetPayAmount();
            Assert.AreEqual(amount, orderAmount);

            //測試sign是否正確
            Assert.AreEqual(payResponse.IsSignTrue(payBaseSetting), true);

            //取得驗證簽名MD5使用字串
            string md5String = payResponse.GetReturnstringToMd5();
            Assert.AreEqual(md5String, md5StringChk);
        }
    }

    [TestClass]
    public class LeHuiPayTest
    {
        #region 測試用假資料
        private static string merchantNo = "LH001020400ZZ00";
        private static string merchantKey = "095F3A2C4AC2E6B936D093DFD8D4EE5B";
        private static int orderAmount = 20;
        private int thirdPartyPayType = 30004;

        private string notifyUrl = ThirdPartyPayment_TestDataGenerator.GetPayConfigUrl("pay_LeHuiPay_notifyurl"); 
        private string returnUrl = ThirdPartyPayment_TestDataGenerator.GetPayConfigUrl("pay_finishUrl");

        private CDPayOrder order = ThirdPartyPayment_TestDataGenerator.GetDefaultOrder(orderAmount);
        private CDMerchantUUID merchant = ThirdPartyPayment_TestDataGenerator.GetDefaultMerchant();

        private LeHuiPayBaseSetting GetLeHuiPayBaseSetting()
        {
            string infoData = ThirdPartyPayment_TestDataGenerator.GetDefaultInfoData(merchantNo, merchantKey);

            string url = "http://example.invalid";
            LeHuiPayBaseSetting baseSetting = new LeHuiPayBaseSetting(infoData, url);
            return baseSetting;
        }
        #endregion

        [TestMethod]
        public void PayOrderTest()
        {
            long orderAmountToLeHui = orderAmount * 100;
            var payBaseSetting = GetLeHuiPayBaseSetting();
            var payOrder = new LeHuiPayOrder(payBaseSetting);
            payOrder.SetPayType(merchant.BankCode, false);
            payOrder.SetOrderData(order);
            payOrder.Done();

            //測試第三方訂單號碼
            string thirdPartyOrderID = payOrder.GetThirdPartyOderID();
            Assert.AreEqual(thirdPartyOrderID, "LeHuiPay000000000001");

            //測試取得MD5前字串

            string md5String = payOrder.GetMd5String();
            string md5StringUrl = $"{merchantNo}&{notifyUrl}&{thirdPartyOrderID}&{orderAmountToLeHui}&{thirdPartyPayType}&{merchantKey}";

            string md5StringChk = MD5Service.UpperCaseUrlEncode(md5StringUrl);
            Assert.AreEqual(md5String, md5StringChk);

            //測試Input字串
            string sign = MD5Service.Hash(md5String, Encoding.UTF8).ToUpper();
            string postInputChk = $"merchantID={merchantNo}&notifyUrl={notifyUrl}&outTradeNo={thirdPartyOrderID}&payMoney={orderAmountToLeHui}&goodsName={order.Id.ToString()}&remark={order.Id.ToString()}&channel={thirdPartyPayType}&sign={sign}";
            string postInput = payOrder.GetPostInputString();
            Assert.AreEqual(postInput, postInputChk);

            //測試InputJson
            var postObj = new
            {
                merchantID = merchantNo,
                notifyUrl = notifyUrl,
                outTradeNo = thirdPartyOrderID,
                payMoney = orderAmountToLeHui,
                goodsName = order.Id.ToString(),
                remark = order.Id.ToString(),
                channel = thirdPartyPayType,
                sign = sign
            };
            string postJsonChk = JsonConvert.SerializeObject(postObj).ToString();
            string postJson = payOrder.GetPostJsonString();
            Assert.AreEqual(postJson, postJsonChk);
        }

        [TestMethod]
        public void PayResponseTest()
        {
            string thirdPartyPayOrderID = "LeHuiPay0000000000000001";
            string amountString = (orderAmount * 100).ToString();
            string payTime = DateTime.Now.ToString("yyyyMMddHHmmss");

            var payBaseSetting = GetLeHuiPayBaseSetting();

            string md5StringUrl = $"{merchantNo}&D{thirdPartyPayOrderID}&{thirdPartyPayOrderID}&{amountString}&{payTime}&000&{merchantKey}";
            string md5StringChk = MD5Service.UpperCaseUrlEncode(md5StringUrl);
            string sign = MD5Service.Hash(md5StringChk, Encoding.UTF8).ToLower();

            var payResponseModel = new LeHuiPayResponseModel
            {
                resultCode = "00",
                message = "success",
                merchantID = merchantNo,
                tradeNo = "D" + thirdPartyPayOrderID,
                outTradeNo = thirdPartyPayOrderID,
                payMoney = Convert.ToInt64(amountString),
                payTime = payTime,
                status = "000",
                remark = "1",
                sign = sign
            };

            var payResponse = new LeHuiPayResponse(payResponseModel);

            //測試回傳是否成功
            Assert.AreEqual(payResponse.IsSuccess(), true);

            //測試交易是否成功
            Assert.AreEqual(payResponse.IsTransactionSuccess(), true);

            //測試資料庫訂單ID是否正確
            int orderID = payResponse.GetOrderID();
            int orderIDChk = 1;
            Assert.AreEqual(orderID, orderIDChk);

            //測試取得交易訂單ID(給第三方的訂單ID)
            string thirdPartyOderID = payResponse.GetThirdPartyOderID();
            Assert.AreEqual(thirdPartyOderID, thirdPartyPayOrderID);

            //測試金額
            int amount = payResponse.GetPayAmount();
            Assert.AreEqual(amount, orderAmount);

            //測試sign是否正確
            Assert.AreEqual(payResponse.IsSignTrue(payBaseSetting), true);

            //取得驗證簽名MD5使用字串
            string md5String = payResponse.GetReturnstringToMd5();
            Assert.AreEqual(md5String, md5StringChk);
        }
    }

    [TestClass]
    public class D1FPayTest
    {
        #region 測試用假資料
        private static string merchantNo = "D1F069";
        private static string merchantKey = "J0JkShHyAcjePKmpAcBCNBxqxy0ZsNU2";
        private static int orderAmount = 20;
        private string thirdPartyPayType = "alipay_118";

        private string notifyUrl = ThirdPartyPayment_TestDataGenerator.GetPayConfigUrl("pay_D1FPay_notifyurl");
        private string returnUrl = ThirdPartyPayment_TestDataGenerator.GetPayConfigUrl("pay_finishUrl");
        private string createTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        private CDPayOrder order = ThirdPartyPayment_TestDataGenerator.GetDefaultOrder(orderAmount);
        private CDMerchantUUID merchant = ThirdPartyPayment_TestDataGenerator.GetDefaultMerchant();

        private D1FPayBaseSetting GetD1FPayBaseSetting()
        {
            string infoData = ThirdPartyPayment_TestDataGenerator.GetDefaultInfoData(merchantNo, merchantKey);

            string url = "https://example.invalid";
            D1FPayBaseSetting baseSetting = new D1FPayBaseSetting(infoData, url);
            return baseSetting;
        }
        #endregion

        [TestMethod]
        public void PayOrderTest()
        {
            order.IPAddress = "0.0.0.0";

            string userID = order.UserID.ToString();
            string orderID = order.Id.ToString();
            string orderMoney = orderAmount.ToString("F2");

            var payBaseSetting = GetD1FPayBaseSetting();
            var payOrder = new D1FPayOrder(payBaseSetting);
            payOrder.SetPayType(merchant.BankCode);
            payOrder.SetOrderData(order);
            payOrder.Done();

            //測試第三方訂單號碼
            string thirdPartyOrderID = payOrder.GetThirdPartyOderID();
            Assert.AreEqual(thirdPartyOrderID, "D1FPay00000000000001");

            //測試取得MD5前字串

            string md5String = payOrder.GetMd5String();
            string md5StringChk = $"channeltype={thirdPartyPayType}&customerbillno={thirdPartyOrderID}&customerbilltime={createTime}&customerextinfo={orderID}&customerno={merchantNo}&customeruser={userID}&devicetype=wap&ip={order.IPAddress}&notifyurl={notifyUrl}&orderamount={orderMoney}&returnurl={returnUrl}&key={merchantKey}";
            Assert.AreEqual(md5String, md5StringChk);

            //測試Input字串
            string sign = MD5Service.Hash(md5String, Encoding.UTF8).ToLower();
            string postInputChk = $"customerno={merchantNo}&channeltype={thirdPartyPayType}&customerbillno={thirdPartyOrderID}&orderamount={orderMoney}&customerbilltime={createTime}&notifyurl={notifyUrl}&returnurl={returnUrl}&ip={order.IPAddress}&sign={sign}&devicetype=wap&customeruser={userID}&customerextinfo={orderID}";
            string postInput = payOrder.GetPostInputString();
            Assert.AreEqual(postInput, postInputChk);

            //測試InputJson
            var postObj = new
            {
                customerno = merchantNo,
                channeltype = thirdPartyPayType,
                customerbillno = thirdPartyOrderID,
                orderamount = orderMoney,
                customerbilltime = createTime,
                notifyurl = notifyUrl,
                returnurl = returnUrl,
                ip = order.IPAddress,
                sign = sign,
                devicetype = "wap",
                customeruser = userID,
                customerextinfo = orderID
             };
            string postJsonChk = JsonConvert.SerializeObject(postObj).ToString();
            string postJson = payOrder.GetPostJsonString();
            Assert.AreEqual(postJson, postJsonChk);
        }

        [TestMethod]
        public void PayResponseTest()
        {
            string thirdPartyOrderIDChk = "D1FPay00000000000001";
            string orderIDString = order.Id.ToString();
            string orderMoney = orderAmount.ToString("F2");

            var payBaseSetting = GetD1FPayBaseSetting();

            string md5StringChk = $"customerbillno={thirdPartyOrderIDChk}&customerbilltime={createTime}&customerextinfo={orderIDString}&customerno={merchantNo}&orderamount={orderMoney}&orderno={thirdPartyOrderIDChk}&paystatus=SUCCESS&preorderamount={orderMoney}&key={merchantKey}";
            string sign = MD5Service.Hash(md5StringChk, Encoding.UTF8).ToLower();

            var payResponseModel = new D1FPayResponseModel
            {
                orderno = thirdPartyOrderIDChk,
                customerno = merchantNo,
                customerbillno = thirdPartyOrderIDChk,
                customerbilltime = createTime,
                preorderamount = orderMoney,
                orderamount = orderMoney,
                paystatus = "SUCCESS",
                customerextinfo = orderIDString,
                sign = sign
            };

            var payResponse = new D1FPayResponse(payResponseModel);

            //測試回傳是否成功
            Assert.AreEqual(payResponse.IsSuccess(), true);

            //測試資料庫訂單ID是否正確
            int orderID = payResponse.GetOrderID();
            int orderIDChk = 1;
            Assert.AreEqual(orderID, orderIDChk);

            //測試取得交易訂單ID(給第三方的訂單ID)
            string thirdPartyOrderID = payResponse.GetThirdPartyOderID();
            Assert.AreEqual(thirdPartyOrderID, thirdPartyOrderIDChk);

            //測試金額
            int amount = payResponse.GetPayAmount();
            Assert.AreEqual(amount, orderAmount);

            //測試sign是否正確
            Assert.AreEqual(payResponse.IsSignTrue(payBaseSetting), true);

            //取得驗證簽名MD5使用字串
            string md5String = payResponse.GetReturnstringToMd5();
            Assert.AreEqual(md5String, md5StringChk);
        }
    }

    [TestClass]
    public class LiMaFuPayTest
    {
        #region 測試用假資料
        private static string merchantNo = "95695";
        private static string merchantKey = "6164707607918515076542869183459647867982079131530707694312307675";
        private static int orderAmount = 200;
        private string thirdPartyPayType = "ZFB";

        private string notifyUrl = ThirdPartyPayment_TestDataGenerator.GetPayConfigUrl("pay_LiMaFuPay_notifyurl");
        private string returnUrl = ThirdPartyPayment_TestDataGenerator.GetPayConfigUrl("pay_finishUrl");

        private CDPayOrder order = ThirdPartyPayment_TestDataGenerator.GetDefaultOrder(orderAmount);
        private CDMerchantUUID merchant = ThirdPartyPayment_TestDataGenerator.GetDefaultMerchant();

        private LiMaFuPayBaseSetting GetLiMaFuPayBaseSetting()
        {
            string infoData = ThirdPartyPayment_TestDataGenerator.GetDefaultInfoData(merchantNo, merchantKey);

            string url = "http://example.invalid/";
            LiMaFuPayBaseSetting baseSetting = new LiMaFuPayBaseSetting(infoData, url);
            return baseSetting;
        }
        #endregion

        [TestMethod]
        public void PayOrderTest()
        {
            order.IPAddress = "0.0.0.0";
            string orderMoney = orderAmount.ToString("F2");

            var payBaseSetting = GetLiMaFuPayBaseSetting();
            var payOrder = new LiMaFuPayOrder(payBaseSetting);
            payOrder.SetPayType(merchant.BankCode);
            payOrder.SetOrderData(order);
            payOrder.Done();

            //測試第三方訂單號碼
            string thirdPartyOrderID = payOrder.GetThirdPartyOderID();
            Assert.AreEqual(thirdPartyOrderID, "LiMaFuPay00000000001");

            //測試取得MD5前字串

            string md5String = payOrder.GetMd5String();
            string md5StringChk = $"{merchantNo}{thirdPartyOrderID}{thirdPartyPayType}{notifyUrl}{returnUrl}{orderMoney}{merchantKey}";
            Assert.AreEqual(md5String, md5StringChk);

            //測試Input字串
            string sign = MD5Service.Hash(md5String, Encoding.UTF8).ToLower();
            string postInputChk = $"merchant_id={merchantNo}&orderid={thirdPartyOrderID}&paytype={thirdPartyPayType}&notifyurl={notifyUrl}&callbackurl={returnUrl}&userip={order.IPAddress}&money={orderMoney}&sign={sign}";
            string postInput = payOrder.GetPostInputString();
            Assert.AreEqual(postInput, postInputChk);

            //測試InputJson
            var postObj = new
            {
                merchant_id = merchantNo,
                orderid = thirdPartyOrderID,
                paytype = thirdPartyPayType,
                notifyurl = notifyUrl,
                callbackurl = returnUrl,
                userip = order.IPAddress,
                money = orderMoney,
                sign = sign,
            };
            string postJsonChk = JsonConvert.SerializeObject(postObj).ToString();
            string postJson = payOrder.GetPostJsonString();
            Assert.AreEqual(postJson, postJsonChk);
        }

        [TestMethod]
        public void PayReturnTest()
        {
            var jsonObj = new
            {
                status = 1,
                data = new
                {
                    url = "http://example.invalid/pay/mark/gt/pay.aspx?id=5583279",
                    qrcode = ""
                },
                data2 = "",
                message = "",
                page = ""
            };
            string payReturnJsonString = JsonConvert.SerializeObject(jsonObj).ToString();

            var payReturn = new LiMaFuPayReturn(payReturnJsonString);

            //測試是否成功
            Assert.AreEqual(payReturn.IsSuccess(), true);

            //測試回傳URL
            Assert.AreEqual(payReturn.GetReturnUrl(), jsonObj.data.url);

            //測試回傳訊息
            Assert.AreEqual(payReturn.GetMessage(), jsonObj.message);
        }

        [TestMethod]
        public void PayResponseTest()
        {
            string thirdPartyOrderIDChk = "LiMaFuPay00000000001";
            string orderMoney = orderAmount.ToString("F2");

            var payBaseSetting = GetLiMaFuPayBaseSetting();

            string md5StringChk = $"{merchantNo}{thirdPartyOrderIDChk}{orderMoney}{merchantKey}";
            string sign = MD5Service.Hash(md5StringChk, Encoding.UTF8).ToLower();

            var payResponseModel = new LiMaFuPayResponseModel
            {
                merchant_id = merchantNo,
                porder = thirdPartyOrderIDChk,
                orderid = thirdPartyOrderIDChk,
                money = orderMoney,
                status = 1,
                sign = sign
            };

            var payResponse = new LiMaFuPayResponse(payResponseModel);

            //測試回傳是否成功
            Assert.AreEqual(payResponse.IsSuccess(), true);

            //測試資料庫訂單ID是否正確
            int orderID = payResponse.GetOrderID();
            int orderIDChk = 1;
            Assert.AreEqual(orderID, orderIDChk);

            //測試取得交易訂單ID(給第三方的訂單ID)
            string thirdPartyOrderID = payResponse.GetThirdPartyOderID();
            Assert.AreEqual(thirdPartyOrderID, thirdPartyOrderIDChk);

            //測試金額
            int amount = payResponse.GetPayAmount();
            Assert.AreEqual(amount, orderAmount);

            //測試sign是否正確
            Assert.AreEqual(payResponse.IsSignTrue(payBaseSetting), true);

            //取得驗證簽名MD5使用字串
            string md5String = payResponse.GetReturnstringToMd5();
            Assert.AreEqual(md5String, md5StringChk);
        }
    }

    [TestClass]
    public class SpeedPayTest
    {
        #region 測試用假資料
        private static string merchantNo = "aweatenane";
        private static string merchantKey = "ff5iesi5ab6crpt31xnph0ali3sn1gp";
        private static int orderAmount = 100;
        private string thirdPartyPayType = "alipay";
        private long timeStamp = TimeZoneInfo.ConvertTimeToUtc(DateTime.Now).CovertToUnixTimestamp();

        private string returnUrl = ThirdPartyPayment_TestDataGenerator.GetPayConfigUrl("pay_finishUrl");

        private CDPayOrder order = ThirdPartyPayment_TestDataGenerator.GetDefaultOrder(orderAmount);
        private CDMerchantUUID merchant = ThirdPartyPayment_TestDataGenerator.GetDefaultMerchant();

        private SpeedPayBaseSetting GetSpeedPayBaseSetting()
        {
            string infoData = ThirdPartyPayment_TestDataGenerator.GetDefaultInfoData(merchantNo, merchantKey);

            string url = "https://example.invalid";
            SpeedPayBaseSetting baseSetting = new SpeedPayBaseSetting(infoData, url);
            return baseSetting;
        }
        #endregion

        [TestMethod]
        public void PayOrderTest()
        {
            var payBaseSetting = GetSpeedPayBaseSetting();
            var payOrder = new SpeedPayOrder(payBaseSetting);
            payOrder.SetPayType(merchant.BankCode);
            payOrder.SetOrderData(order);
            payOrder.Done();

            //測試第三方訂單號碼
            string thirdPartyOrderID = payOrder.GetThirdPartyOderID();
            Assert.AreEqual(thirdPartyOrderID, "SpeedPay000000000001");

            //測試取得MD5前字串

            string md5String = payOrder.GetMd5String();
            string md5StringChk = $"{merchantKey}currency_money{orderAmount}currency_typeCNYfortestfalsemchno{merchantNo}order_no{thirdPartyOrderID}order_type1pay_methods{thirdPartyPayType}remark{order.Id}timestamp{timeStamp}username{order.UserID}vcoin_codeFCNY{merchantKey}";
            Assert.AreEqual(md5String, md5StringChk);

            //測試Input字串
            string sign = MD5Service.Hash(md5String, Encoding.UTF8).ToLower();
            string postInputChk = $"mchno={merchantNo}&timestamp={timeStamp}&signature={sign}&username={order.UserID}&order_no={thirdPartyOrderID}&order_type=1&vcoin_code=FCNY&currency_type=CNY&currency_money={orderAmount}&pay_methods={thirdPartyPayType}&remark={order.Id}&fortest=false";
            string postInput = payOrder.GetPostInputString();
            Assert.AreEqual(postInput, postInputChk);

            //測試InputJson
            var postObj = new
            {
                mchno = merchantNo,
                timestamp = timeStamp,
                signature = sign,
                username = order.UserID.ToString(),
                order_no = thirdPartyOrderID,
                order_type = 1,
                vcoin_code = "FCNY",
                currency_type = "CNY",
                currency_money = orderAmount,
                pay_methods = thirdPartyPayType,
                remark = order.Id.ToString(),
                fortest = "false",
            };
            string postJsonChk = JsonConvert.SerializeObject(postObj).ToString();
            string postJson = payOrder.GetPostJsonString();
            Assert.AreEqual(postJson, postJsonChk);
        }

        [TestMethod]
        public void PayReturnTest()
        {
            //測試正確資料
            var jsonObj = new SpeedPayReturnModel
            {
                errcode = 0,
                errmsg = "",
                data = "https://example.invalid/v1/order/login?_token=[REDACTED]",
                total = null,
            };
            string payReturnJsonString = JsonConvert.SerializeObject(jsonObj).ToString();

            var payReturn = new SpeedPayReturn(payReturnJsonString);

            //測試是否成功
            Assert.AreEqual(payReturn.IsSuccess(), true);

            //測試回傳URL
            Assert.AreEqual(payReturn.GetReturnUrl(), jsonObj.data);

            //測試回傳訊息
            Assert.AreEqual(payReturn.GetMessage(), jsonObj.errmsg);

            //測試錯誤資料
            jsonObj = new SpeedPayReturnModel
            {
                errcode = 10002,
                errmsg = "收款账号不能为空",
                data = null,
                total = null,
            };

            payReturnJsonString = JsonConvert.SerializeObject(jsonObj).ToString();

            payReturn = new SpeedPayReturn(payReturnJsonString);

            //測試是否成功
            Assert.AreEqual(payReturn.IsSuccess(), false);

            //測試回傳URL
            Assert.AreEqual(payReturn.GetReturnUrl(), string.Empty);

            //測試回傳訊息
            Assert.AreEqual(payReturn.GetMessage(), jsonObj.errmsg);
        }

        [TestMethod]
        public void PayResponseTest()
        {
            string thirdPartyOrderIDChk = "SpeedPay000000000001";
            string orderIDString = order.Id.ToString();
            string userIDString = order.UserID.ToString();
            string orderAmountString = orderAmount.ToString("F4");
            string orderAmountStringF10 = orderAmount.ToString("F10");
            byte status;

            var payBaseSetting = GetSpeedPayBaseSetting();

            //測試第一次回調
            status = (byte)SpeedPayResponseStatus.CreateOrder;
            string md5StringChk = $"{merchantKey}create_time2019-10-15 12:00:00currency_money{orderAmountString}currency_typeCNYfinish_time2019-10-15 15:00:00mchno{merchantNo}order_no{thirdPartyOrderIDChk}order_typebuypay_amount{orderAmountString}pay_order_no{thirdPartyOrderIDChk}price{orderAmountString}remark{orderIDString}status{status}timestamp{timeStamp}username{userIDString}vcoin_codeFCNYvcoin_count{orderAmountStringF10}{merchantKey}";
            string sign = MD5Service.Hash(md5StringChk, Encoding.UTF8).ToLower();

            var payResponseModel = new SpeedPayResponseModel
            {
                mchno = merchantNo,
                timestamp = timeStamp,
                signature = sign,
                username = userIDString,
                order_no = thirdPartyOrderIDChk,
                pay_order_no = thirdPartyOrderIDChk,
                order_type = "buy",
                vcoin_code = "FCNY",
                vcoin_count = orderAmountStringF10,
                currency_type = "CNY",
                currency_money = orderAmountString,
                pay_amount = orderAmountString,
                price = orderAmountString,
                status = (int)SpeedPayResponseStatus.CreateOrder,
                create_time = "2019-10-15 12:00:00",
                finish_time = "2019-10-15 15:00:00",
                remark = orderIDString,
            };
            var payResponse = new SpeedPayResponse(payResponseModel);

            //測試資料庫訂單ID是否正確
            int orderID = payResponse.GetOrderID();
            int orderIDChk = 1;
            Assert.AreEqual(orderID, orderIDChk);

            //測試sign是否正確
            Assert.AreEqual(payResponse.IsSignTrue(payBaseSetting), true);

            //取得驗證簽名MD5使用字串
            string md5String = payResponse.GetReturnstringToMd5();
            Assert.AreEqual(md5String, md5StringChk);

            //測試第二次回調
            status = (byte)SpeedPayResponseStatus.Payed;
            md5StringChk = $"{merchantKey}create_time2019-10-15 12:00:00currency_money{orderAmountString}currency_typeCNYfinish_time2019-10-15 15:00:00mchno{merchantNo}order_no{thirdPartyOrderIDChk}order_typebuypay_amount{orderAmountString}pay_order_no{thirdPartyOrderIDChk}price{orderAmountString}remark{orderIDString}status{status}timestamp{timeStamp}username{userIDString}vcoin_codeFCNYvcoin_count{orderAmountStringF10}{merchantKey}";
            sign = MD5Service.Hash(md5StringChk, Encoding.UTF8).ToLower();

            payResponseModel = new SpeedPayResponseModel
            {
                mchno = merchantNo,
                timestamp = timeStamp,
                signature = sign,
                username = userIDString,
                order_no = thirdPartyOrderIDChk,
                pay_order_no = thirdPartyOrderIDChk,
                order_type = "buy",
                vcoin_code = "FCNY",
                vcoin_count = orderAmountStringF10,
                currency_type = "CNY",
                currency_money = orderAmountString,
                pay_amount = orderAmountString,
                price = orderAmountString,
                status = (int)SpeedPayResponseStatus.Payed,
                create_time = "2019-10-15 12:00:00",
                finish_time = "2019-10-15 15:00:00",
                remark = orderIDString,
            };
            payResponse = new SpeedPayResponse(payResponseModel);

            orderID = payResponse.GetOrderID();
            orderIDChk = 1;
            Assert.AreEqual(orderID, orderIDChk);

            //測試sign是否正確
            Assert.AreEqual(payResponse.IsSignTrue(payBaseSetting), true);

            //取得驗證簽名MD5使用字串
            md5String = payResponse.GetReturnstringToMd5();
            Assert.AreEqual(md5String, md5StringChk);

            //測試第三次回調
            status = (byte)SpeedPayResponseStatus.Finish;
            md5StringChk = $"{merchantKey}create_time2019-10-15 12:00:00currency_money{orderAmountString}currency_typeCNYfinish_time2019-10-15 15:00:00mchno{merchantNo}order_no{thirdPartyOrderIDChk}order_typebuypay_amount{orderAmountString}pay_order_no{thirdPartyOrderIDChk}price{orderAmountString}remark{orderIDString}status{status}timestamp{timeStamp}username{userIDString}vcoin_codeFCNYvcoin_count{orderAmountStringF10}{merchantKey}";
            sign = MD5Service.Hash(md5StringChk, Encoding.UTF8).ToLower();

            payResponseModel = new SpeedPayResponseModel
            {
                mchno = merchantNo,
                timestamp = timeStamp,
                signature = sign,
                username = userIDString,
                order_no = thirdPartyOrderIDChk,
                pay_order_no = thirdPartyOrderIDChk,
                order_type = "buy",
                vcoin_code = "FCNY",
                vcoin_count = orderAmountStringF10,
                currency_type = "CNY",
                currency_money = orderAmountString,
                pay_amount = orderAmountString,
                price = orderAmountString,
                status = (int)SpeedPayResponseStatus.Finish,
                create_time = "2019-10-15 12:00:00",
                finish_time = "2019-10-15 15:00:00",
                remark = orderIDString,
            };
            payResponse = new SpeedPayResponse(payResponseModel);

            //測試資料庫訂單ID是否正確
            orderID = payResponse.GetOrderID();
            orderIDChk = 1;
            Assert.AreEqual(orderID, orderIDChk);

            //測試取得交易訂單ID(給第三方的訂單ID)
            string thirdPartyOrderID = payResponse.GetThirdPartyOderID();
            Assert.AreEqual(thirdPartyOrderID, thirdPartyOrderIDChk);

            //測試金額
            int amount = payResponse.GetPayAmount();
            Assert.AreEqual(amount, orderAmount);

            //測試sign是否正確
            Assert.AreEqual(payResponse.IsSignTrue(payBaseSetting), true);

            //取得驗證簽名MD5使用字串
            md5String = payResponse.GetReturnstringToMd5();
            Assert.AreEqual(md5String, md5StringChk);

            //測試第三次回調，實際假資料
            status = (byte)SpeedPayResponseStatus.Finish;
            string priceReal = "1.0000";
            string pay_amountReal = "100.0000";
            string currency_moneyReal = "100.0000";
            string vcoin_countReal = "100.0000000000";
            long timeStampReal = 1571199384;
            string pay_order_noReal = "20191016121519652716";
            string order_noReal = "SpeedPay000000022100";
            string usernameReal = "64549493";
            md5StringChk = $"{merchantKey}create_timecurrency_money{currency_moneyReal}currency_typeCNYfinish_timemchno{merchantNo}order_no{order_noReal}order_typebuypay_amount{pay_amountReal}pay_order_no{pay_order_noReal}price{priceReal}remarkstatus{status}timestamp{timeStampReal}username{usernameReal}vcoin_codeFCNYvcoin_count{vcoin_countReal}{merchantKey}";
            sign = "656582fcfc44c7b6a7af8f6d1d312964";

            payResponseModel = new SpeedPayResponseModel
            {
                mchno = merchantNo,
                timestamp = timeStampReal,
                signature = sign,
                username = usernameReal,
                order_no = order_noReal,
                pay_order_no = pay_order_noReal,
                order_type = "buy",
                vcoin_code = "FCNY",
                vcoin_count = vcoin_countReal,
                currency_type = "CNY",
                currency_money = currency_moneyReal,
                pay_amount = pay_amountReal,
                price = priceReal,
                status = (int)SpeedPayResponseStatus.Finish,
                create_time = "",
                finish_time = "",
                remark = "",
            };
            payResponse = new SpeedPayResponse(payResponseModel);

            //測試資料庫訂單ID是否正確
            orderID = payResponse.GetOrderID();
            orderIDChk = 22100;
            Assert.AreEqual(orderID, orderIDChk);

            //測試取得交易訂單ID(給第三方的訂單ID)
            thirdPartyOrderID = payResponse.GetThirdPartyOderID();
            Assert.AreEqual(thirdPartyOrderID, order_noReal);

            //測試金額
            amount = payResponse.GetPayAmount();
            Assert.AreEqual(amount, orderAmount);

            //測試sign是否正確
            payResponse.IsSignTrue(payBaseSetting);
            Assert.AreEqual(payResponse.IsSignTrue(payBaseSetting), true);

            //取得驗證簽名MD5使用字串
            md5String = payResponse.GetReturnstringToMd5();
            Assert.AreEqual(md5String, md5StringChk);

            string signChk = MD5Service.Hash(md5String, Encoding.UTF8).ToLower();
            Assert.AreEqual(signChk, sign);
        }
    }

    [TestClass]
    public class SFPayTest
    {
        #region 測試用假資料
        private static string merchantNo = "20094";
        private static string merchantKey = "7f862055b94d85a8e54603ac20c53a43";
        private static int orderAmount = 100;
        private static string orderAmountString = orderAmount.ToString("F2");
        private string thirdPartyPayType = "ALIPAY";
        private byte thirdPartyPayTypeCode = 3;

        private string notifyUrl = ThirdPartyPayment_TestDataGenerator.GetPayConfigUrl("pay_SFPay_notifyurl");
        private string returnUrl = ThirdPartyPayment_TestDataGenerator.GetPayConfigUrl("pay_finishUrl");

        private CDPayOrder order = ThirdPartyPayment_TestDataGenerator.GetDefaultOrder(orderAmount);
        private CDMerchantUUID merchant = ThirdPartyPayment_TestDataGenerator.GetDefaultMerchant();

        private SFPayBaseSetting GetSFPayBaseSetting()
        {
            string infoData = ThirdPartyPayment_TestDataGenerator.GetDefaultInfoData(merchantNo, merchantKey);

            string url = "https://example.invalid";
            SFPayBaseSetting baseSetting = new SFPayBaseSetting(infoData, url);
            return baseSetting;
        }
        #endregion

        [TestMethod]
        public void PayOrderTest()
        {
            var payBaseSetting = GetSFPayBaseSetting();
            var payOrder = new SFPayOrder(payBaseSetting);
            payOrder.SetPayType(merchant.BankCode);
            payOrder.SetOrderData(order);
            payOrder.Done();

            //測試第三方訂單號碼
            string thirdPartyOrderID = payOrder.GetThirdPartyOderID();
            Assert.AreEqual(thirdPartyOrderID, "SFPay000000000000001");

            //測試取得MD5前字串

            string md5String = payOrder.GetMd5String();
            string md5StringChk = $"money={orderAmountString}&notify_url={notifyUrl}&out_trade_no={thirdPartyOrderID}&partner_id={merchantNo}&pay_code={thirdPartyPayType}&pay_type={thirdPartyPayTypeCode}&return_url={returnUrl}{merchantKey}";
            Assert.AreEqual(md5String, md5StringChk);

            //測試InputJson
            var postObj = new
            {
                partner_id = merchantNo,
                pay_type = thirdPartyPayTypeCode,
                pay_code = thirdPartyPayType,
                out_trade_no = thirdPartyOrderID,
                notify_url = notifyUrl,
                return_url = returnUrl,
                money = orderAmountString,
                sign = MD5Service.Hash(md5String, Encoding.UTF8).ToLower(),
                sign_type = "md5"
            };
            string postJsonChk = JsonConvert.SerializeObject(postObj).ToString();
            string postJson = payOrder.GetPostJsonString();
            Assert.AreEqual(postJson, postJsonChk);
        }

        [TestMethod]
        public void PayResponseTest()
        {
            string thirdPartyOrderIDChk = "SFPay000000000000001";

            var payBaseSetting = GetSFPayBaseSetting();
            string md5StringChk = $"money={orderAmountString}&money_true={orderAmountString}&out_trade_no={thirdPartyOrderIDChk}&partner_id={merchantNo}&pay_code={thirdPartyPayType}&pay_type={thirdPartyPayTypeCode}&trade_no={thirdPartyOrderIDChk}{merchantKey}";
            string sign = MD5Service.Hash(md5StringChk, Encoding.UTF8).ToLower();

            var payResponseModel = new SFPayResponseModel
            {
                partner_id = merchantNo,
                pay_type = thirdPartyPayTypeCode,
                pay_code = thirdPartyPayType,
                trade_no = thirdPartyOrderIDChk,
                out_trade_no = thirdPartyOrderIDChk,
                money = orderAmountString,
                money_true = orderAmountString,
                sign = sign,
                sign_type = "md5"
            };
            string payResponseModelJsonString = JsonConvert.SerializeObject(payResponseModel).ToString(); ;
            var payResponse = new SFPayResponse(payResponseModelJsonString);

            //測試資料庫訂單ID是否正確
            int orderID = payResponse.GetOrderID();
            int orderIDChk = 1;
            Assert.AreEqual(orderID, orderIDChk);

            //測試取得交易訂單ID(給第三方的訂單ID)
            string thirdPartyOrderID = payResponse.GetThirdPartyOderID();
            Assert.AreEqual(thirdPartyOrderID, thirdPartyOrderIDChk);

            //測試金額
            int amount = payResponse.GetPayAmount();
            Assert.AreEqual(amount, orderAmount);

            //測試sign是否正確
            Assert.AreEqual(payResponse.IsSignTrue(payBaseSetting), true);

            //取得驗證簽名MD5使用字串
            string md5String = payResponse.GetReturnstringToMd5();
            Assert.AreEqual(md5String, md5StringChk);

            
            //第三方提供示範
            payResponseModelJsonString = "{\"partner_id\":\"20086\",\"pay_type\":\"3\",\"pay_code\":\"ALIPAY\",\"trade_no\":\"SF1910181833373357\",\"out_trade_no\":\"TEST1910181833376242\",\"money\":\"100.35\",\"money_true\":\"100.35\",\"sign\":\"e94c9834820a0688876be1896dcb06fc\",\"sign_type\":\"md5\"}";
            payResponseModel = JsonConvert.DeserializeObject<SFPayResponseModel>(payResponseModelJsonString);

            payResponse = new SFPayResponse(payResponseModelJsonString);

            //測試取得交易訂單ID(給第三方的訂單ID)
            thirdPartyOrderID = payResponse.GetThirdPartyOderID();
            Assert.AreEqual(thirdPartyOrderID, payResponseModel.out_trade_no);

            //測試sign是否正確
            string infoData = ThirdPartyPayment_TestDataGenerator.GetDefaultInfoData("20086", "0529713e5a67b798c900bf4a9d578547");
            string url = "https://example.invalid";
            payBaseSetting = new SFPayBaseSetting(infoData, url);
            Assert.AreEqual(payResponse.IsSignTrue(payBaseSetting), true);
        }
    }

    [TestClass]
    public class WonderfulskyPayTest
    {
        #region 測試用假資料
        private static string merchantNo = "6474";
        private static string merchantKey = "72454F48164F686B7095FA319535FEDB";
        private string privateKeyContent = "[PRIVATE_KEY_BLOCK_REDACTED]\n";
        private string publicKeyContent = "-----BEGIN PUBLIC KEY-----\nMIICIjANBgkqhkiG9w0BAQEFAAOCAg8AMIICCgKCAgEAy9Oftcu/MAPWfLwZ/VJO\n0S+2gRKnlhqcb7KE1phQTcukTT//ObAkWAiZrsXn3TfsiX0py4iiwAFDmI4bylTI\nmdZDgp8/9oYnNINfQR9vRqj+NO5R+E1qCTrTlRN5jF2TM6XoN9zqTKABSV5l085Z\nRwgG4SAF5Ak0D0g/T9T4I+YWbXo8knDsL6zstHjcZ8cP0dkR8jvyAho5VkD0rF0Y\nPUEWCloQOYchwyuJgG798FcwWf5pk0ALd7jWztEd+FB5DktWe5+wkrE+xrCsHX2/\nw3RpSc46NU/hd+DjWrLjQDIJnMe9qUop1F+Uc/2z0bUnawMQSt9xAQ+JtySwzbQo\nWlOhI5rSqvtDo+7PHexgfR+sda2nuyJZocYL6IVheRXVzEcGpj89E4kimAdssscB\nqhmZEHdR+ConCLy+yBLj255fqw0uBoxVwWzRxB08YeCeNf9SwAd4gw5B8MJ73jGF\noQbOWMyxG3OG5m46tPxtGlQOtkU5H/jOJ1BVysRPdAS5feIyb+z9xqsP1aist12O\nKnUmElgQ8iouxCS9iUsD4XI+cXGUldP4xeK1bDXCsl4JSvM4lE0dqYvj9qSEl95R\nZRMyJQH6Xoxjg1qL2UWxa9PFqKghgYWbmDlEfmnDvJx8B8EmXhxqvent5JSSffNA\nSlD7Jgzi/7DXGWwnMUnB+GUCAwEAAQ==\n-----END PUBLIC KEY-----\n";
        private static int orderAmount = 100;
        private static string orderAmountString = orderAmount.ToString("F2");
        private string thirdPartyPayType = "alipay";

        private string notifyUrl = ThirdPartyPayment_TestDataGenerator.GetPayConfigUrl("pay_WonderfulskyPay_notifyurl");
        private string returnUrl = ThirdPartyPayment_TestDataGenerator.GetPayConfigUrl("pay_finishUrl");

        private CDPayOrder order = ThirdPartyPayment_TestDataGenerator.GetDefaultOrder(orderAmount);
        private CDMerchantUUID merchant = ThirdPartyPayment_TestDataGenerator.GetDefaultMerchant();

        private WonderfulskyPayBaseSetting GetWonderfulskyPayBaseSetting()
        {
            var result = new
            {
                MERCHANT_NO = merchantNo,
                MERCHANT_KEY = merchantKey,
                PRIVATE_KEY_CONTENT = privateKeyContent,
                PUBLIC_KEY_CONTENT = publicKeyContent,
            };
            string infoData = JsonConvert.SerializeObject(result).ToString();

            string url = "https://example.invalid/service";
            WonderfulskyPayBaseSetting baseSetting = new WonderfulskyPayBaseSetting(infoData, url);
            return baseSetting;
        }
        #endregion

        [TestMethod]
        public void PayOrderTest()
        {
            var payBaseSetting = GetWonderfulskyPayBaseSetting();
            var payOrder = new WonderfulskyPayOrder(payBaseSetting);
            payOrder.SetPayType(merchant.BankCode);
            payOrder.SetOrderData(order);
            payOrder.Done();

            //測試第三方訂單號碼
            string thirdPartyOrderID = payOrder.GetThirdPartyOderID();
            Assert.AreEqual(thirdPartyOrderID, "WonderfulskyPay0000000001");

            //測試取得MD5前字串

            string md5String = payOrder.GetSignBeforeString();
            string md5StringChk = $"amount={orderAmount}&channel={thirdPartyPayType}&currency=CNY&merchantid={thirdPartyOrderID}&mid={merchantNo}&notifyurl={notifyUrl}&returnurl={returnUrl}&service=Payment&{merchantKey}";
            Assert.AreEqual(md5String, md5StringChk);

            //測試Input字串，因為RSA加密每次sign都不同，所以不檢查(因若去除sign則內容與MD5前字串相同)
        }

        [TestMethod]
        public void PayReturnTest()
        {
            //測試正確資料
            var jsonObj = new WonderfulskyPayReturnModel
            {
                merchantid = "WonderfulskyPay0000000002",
                mid = "6474",
                qrcodeurl = "http://example.invalid/user/Api/getPaymentCode/type/1/url/aHR0cDovL3FyaW1nLmNkbnBwcGF5LmNvbS8yMDE5MTAyOC8wMTI5NDY2MGU0NzMzNGVhMTlhNmFmMjc0YzQyNDE1Ni5qcGc=/cashremake/959194/money/100/qrid/78454",
                result = 1,
                sign = "U+bqhC+Cf/HX61eI1tLCgtLxdiySi4CUtleHVbIf03VocdDUug9zs4cJQmF6PNCL0mTKKePwbtbqkFSSIP34NdP77ora+GqJXMz34DNb+HaqxnPg1fq9XZKNcWeVjKnUN0YvX1e/8G2/pCoD0xSBMBxIJj8L6bk5B/IGgSVxjJVNO/KOY8nBrFWXTOqsZH++7Sqd5r66N4W8sycIchFF5KBN3U45KRWARqzoeXItL/sBtRq6Kk8qLsszHvGSkCJfX1ZaMFUIJl4j+DNzTEkqpAjdu/+rXKpudF7lTE9coqNRludym3Fq+n629/uKxCmAiJ0PUEStJ6gi78Nd83LLS377dfNa36XpzLKJ0MFay74Ubf5gGfGKedpAwuzJaNjCXfknk1XhX0tW3KWeRnRwpQCiKT8uX585PhC9JO96qxVazKHoudO4d6V0Pp/+eHq1j7M3K+12nrsIdT01ZIhvVc3rsvadSM5UHuxUfH2JYTRqdsisHZhKclOfI6fRaXU90MGRgKo2U0SaUgOl58CtpiRGo9lIoldURc2cfH1nqYKou9x+WIM3g4vg+ilLkdlwOMwL7gRQmyNrSp9+qTz9rGTMdqvUSvZ/UxgwvouAW+TCmwJg6GQGhfaYUCVNmM9bH6oSHzvCP75kGudZXOuLLr3yiz9ZSOzb+D488Y3oOBg=",
            };
            string payReturnJsonString = JsonConvert.SerializeObject(jsonObj).ToString();

            var payReturn = new WonderfulskyPayReturn(payReturnJsonString);

            //測試是否成功
            Assert.AreEqual(payReturn.IsSuccess(), true);

            //測試回傳URL
            Assert.AreEqual(payReturn.GetReturnUrl(), jsonObj.qrcodeurl);

            //測試sign是否正確
            var payBaseSetting = GetWonderfulskyPayBaseSetting();
            Assert.AreEqual(payReturn.IsSignTrue(payBaseSetting), true);

            //取得驗證簽名RSA使用字串
            string rsaStringChk = $"merchantid={jsonObj.merchantid}&mid={jsonObj.mid}&qrcodeurl={jsonObj.qrcodeurl}&result={jsonObj.result}&{merchantKey}";
            string rsaString = payReturn.GetSignBeforeString();
            Assert.AreEqual(rsaString, rsaStringChk);

            //測試錯誤資料
            jsonObj = new WonderfulskyPayReturnModel
            {
                result = 1,
                code = "0006",
                message = "Sign Error!",
            };

            payReturnJsonString = JsonConvert.SerializeObject(jsonObj).ToString();

            payReturn = new WonderfulskyPayReturn(payReturnJsonString);

            //測試是否成功
            Assert.AreEqual(payReturn.IsSuccess(), false);

            //測試回傳URL
            Assert.AreEqual(payReturn.GetReturnUrl(), string.Empty);

            //測試回傳訊息
            Assert.AreEqual(payReturn.GetMessage(), jsonObj.message);
        }

        [TestMethod]
        public void PayResponseTest()
        {
            string thirdPartyOrderIDChk = "WonderfulskyPay0000000001";
            string dateTimeNow = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");

            var payBaseSetting = GetWonderfulskyPayBaseSetting();
            string signBeforeStringChk = $"amount={orderAmountString}&merchantid={thirdPartyOrderIDChk}&mid={merchantNo}&status=1&systemid={thirdPartyOrderIDChk}&time={dateTimeNow}&{merchantKey}";
            string sign = RSAService.Sign(payBaseSetting.PrivateKeyContent, signBeforeStringChk);

            var payResponseModel = new WonderfulskyPayResponseModel
            {
                mid = merchantNo,
                merchantid = thirdPartyOrderIDChk,
                systemid = thirdPartyOrderIDChk,
                amount = orderAmountString,
                status = 1.ToString(),
                time = dateTimeNow,
                remark = null,
                sign = sign,
            };

            var payResponse = new WonderfulskyPayResponse(payResponseModel);

            //測試訂單是否成功
            Assert.AreEqual(payResponse.IsSuccess(), true);

            //測試資料庫訂單ID是否正確
            int orderID = payResponse.GetOrderID();
            Assert.AreEqual(orderID, order.Id);

            //測試取得交易訂單ID(給第三方的訂單ID)
            string thirdPartyOrderID = payResponse.GetThirdPartyOderID();
            Assert.AreEqual(thirdPartyOrderID, thirdPartyOrderIDChk);

            //測試金額
            int amount = payResponse.GetPayAmount();
            Assert.AreEqual(amount, orderAmount);

            //測試sign是否正確
            payResponse.IsSignTrue(payBaseSetting);
            //Assert.AreEqual(payResponse.IsSignTrue(payBaseSetting), true);

            //取得驗證簽名使用字串
            string signBeforeString = payResponse.GetSignBeforeString();
            Assert.AreEqual(signBeforeString, signBeforeStringChk);

        }
    }

    [TestClass]
    public class GoPayTest
    {
        #region 測試用假資料
        private static string merchantNo = "15578144836";
        private static string merchantKey = "48dc3907ecd0c1fb16acf9fc4f14cf3d4240a7ba";
        private static int orderAmount = 100;
        private static string orderAmountString = orderAmount.ToString("F2");
        private byte thirdPartyPayType = 1;

        private string notifyUrl = ThirdPartyPayment_TestDataGenerator.GetPayConfigUrl("pay_GoPay_notifyurl");

        private static DateTime dtUtNow = DateTime.UtcNow;
        private static long timeStamp = dtUtNow.CovertToUnixTimestamp();

        private CDPayOrder order = ThirdPartyPayment_TestDataGenerator.GetDefaultOrder(orderAmount);
        private CDMerchantUUID merchant = ThirdPartyPayment_TestDataGenerator.GetDefaultMerchant();

        private GoPayBaseSetting GetGoPayBaseSetting()
        {
            string infoData = ThirdPartyPayment_TestDataGenerator.GetDefaultInfoData(merchantNo, merchantKey);

            string url = "https://example.invalid:888";
            GoPayBaseSetting baseSetting = new GoPayBaseSetting(infoData, url);
            return baseSetting;
        }
        #endregion

        [TestMethod]
        public void PayOrderTest()
        {
            order.IPAddress = string.Empty;
            var payBaseSetting = GetGoPayBaseSetting();
            var payOrder = new GoPayOrder(payBaseSetting);
            payOrder.SetPayType(merchant.BankCode);
            payOrder.SetOrderData(order);
            payOrder.Done();

            //測試第三方訂單號碼
            string thirdPartyOrderID = payOrder.GetThirdPartyOderID();
            Assert.AreEqual(thirdPartyOrderID, "GoPay000000000000001");

            //測試取得MD5前字串
            string md5String = payOrder.GetMd5String();
            string md5StringChk = $"client_ip=&format=page&goods_desc=&mch_id={merchantNo}&money={orderAmountString}&notify_url={notifyUrl}&order_sn={thirdPartyOrderID}&ptype={thirdPartyPayType}&time={timeStamp}&username={order.UserID}&key={merchantKey}";
            Assert.AreEqual(md5String, md5StringChk);

            //測試InputString
            string sign = MD5Service.Hash(md5String).ToLower();
            string postJsonChk = $"client_ip=&format=page&goods_desc=&mch_id={merchantNo}&money={orderAmountString}&notify_url={notifyUrl.UrlEncode()}&order_sn={thirdPartyOrderID}&ptype={thirdPartyPayType}&time={timeStamp}&username={order.UserID}&sign={sign}";
            string postJson = payOrder.GetPostInputString();
            Assert.AreEqual(postJson, postJsonChk);
        }

        [TestMethod]
        public void PayReturnTest()
        {
            string thirdPartyPayOrderID = "GoPay000000000000001";

            var jsonObj = new
            {
                code = "1",
                msg = "下单成功",
                data = new
                {
                    order_sn = thirdPartyPayOrderID,
                    ptype = 1,
                    ptype_name = "支付宝扫码",
                    realname = "1",
                    account = merchantNo,
                    money = "100.00",
                    bank = "",
                    qrcode = "",
                    url = "http://localhsot"
                }
            };
            string payReturnJsonString = JsonConvert.SerializeObject(jsonObj);
            var payReturn = new GoPayReturn(payReturnJsonString);

            //測試是否成功
            Assert.AreEqual(payReturn.IsSuccess(), true);

            //測試訂單號碼
            Assert.AreEqual(payReturn.GetThirdPartyOderID(), jsonObj.data.order_sn);

            //測試回傳訊息
            Assert.AreEqual(payReturn.GetMessage(), jsonObj.msg);

            //測試回傳網址
            Assert.AreEqual(payReturn.GetReturnUrl(), jsonObj.data.url);

            //實際回應測試
            jsonObj = new
            {
                code = "1",
                msg = "下单成功",
                data = new
                {
                    order_sn = "MS2019110113034091543",
                    ptype = 1,
                    ptype_name = "支付宝扫码",
                    realname = "李春",
                    account = "思语",
                    money = "100.00",
                    bank = "",
                    qrcode = "https://example.invalid:888/uploads/home/201910/78f0d7f507bc9e12.jpeg",
                    url = "https://example.invalid:888/?c=Pay&a=info&osn=MS2019110113034091543"
                }
            };
            payReturnJsonString = JsonConvert.SerializeObject(jsonObj);
            payReturn = new GoPayReturn(payReturnJsonString);

            //測試是否成功
            Assert.AreEqual(payReturn.IsSuccess(), true);

            //測試訂單號碼
            Assert.AreEqual(payReturn.GetThirdPartyOderID(), jsonObj.data.order_sn);

            //測試回傳訊息
            Assert.AreEqual(payReturn.GetMessage(), jsonObj.msg);

            //測試回傳網址
            Assert.AreEqual(payReturn.GetReturnUrl(), jsonObj.data.url);
        }

        [TestMethod]
        public void PayResponseTest()
        {
            string thirdPartyOrderIDChk = "GoPay000000000000001";

            var payBaseSetting = GetGoPayBaseSetting();
            string md5StringChk = $"money={orderAmountString}&pt_order={thirdPartyOrderIDChk}&sh_order={thirdPartyOrderIDChk}&time={timeStamp}&key={merchantKey}";
            string sign = MD5Service.Hash(md5StringChk).ToLower();

            var payResponseModel = new GoPayResponseModel
            {
                money = orderAmountString,
                pt_order = thirdPartyOrderIDChk,
                sh_order = thirdPartyOrderIDChk,
                time = timeStamp,
                sign = sign,
            };

            var payResponse = new GoPayResponse(payResponseModel);

            //測試資料庫訂單ID是否正確
            int orderID = payResponse.GetOrderID();
            int orderIDChk = order.Id;
            Assert.AreEqual(orderID, orderIDChk);

            //測試取得交易訂單ID(給第三方的訂單ID)
            string thirdPartyOrderID = payResponse.GetThirdPartyOderID();
            Assert.AreEqual(thirdPartyOrderID, thirdPartyOrderIDChk);

            //測試金額
            int amount = payResponse.GetPayAmount();
            Assert.AreEqual(amount, orderAmount);

            //測試sign是否正確
            Assert.AreEqual(payResponse.IsSignTrue(payBaseSetting), true);

            //取得驗證簽名MD5使用字串
            string md5String = payResponse.GetSignBeforeString();
            Assert.AreEqual(md5String, md5StringChk);
        }
    }

    [TestClass]
    public class SFTPayTest
    {
        #region 測試用假資料
        private static string merchantNo = "1910312050137750001";
        private static string merchantKey = "spqla01s";
        private static int orderAmount = 100;
        private static string orderAmountString = orderAmount.ToString("F2");
        private byte thirdPartyPayChannel = 2;
        private byte thirdPartyPayType = 2;
        private byte thirdPartyPayWay = 1;

        private string notifyUrl = ThirdPartyPayment_TestDataGenerator.GetPayConfigUrl("pay_SFTPay_notifyurl");
        private string returnUrl = ThirdPartyPayment_TestDataGenerator.GetPayConfigUrl("pay_finishUrl");

        private CDPayOrder order = ThirdPartyPayment_TestDataGenerator.GetDefaultOrder(orderAmount);
        private CDMerchantUUID merchant = ThirdPartyPayment_TestDataGenerator.GetDefaultMerchant();

        private ThirdPartyPayMerchantInfoSetting GetSFTPayBaseSetting()
        {
            string infoData = ThirdPartyPayment_TestDataGenerator.GetDefaultInfoData(merchantNo, merchantKey);

            string url = "https://example.invalid";
            ThirdPartyPayMerchantInfoSetting baseSetting = new ThirdPartyPayMerchantInfoSetting(infoData, url);
            return baseSetting;
        }
        #endregion

        [TestMethod]
        public void SFTPaySignServiceTest()
        {
            string out_trade_no = "MERPay00000000000001";
            var obj = new SFTPaySignService(merchantKey);
            var temp = obj.MerchantKeyASCIIByte;
            var tempIntArray = new int[] { 231, 228, 87, 198, 13, 52, 196, 17, 2, 15, 60, 247, 27, 161, 224, 138, 134, 92, 150, 94, 97, 65, 101, 233, 84, 131, 133, 100, 77, 214, 37, 183, 246, 59, 116, 222, 227, 127, 158, 235, 91, 244, 143, 61, 242, 89, 49, 83, 141, 70, 178, 126, 212, 106, 50, 123, 39, 30, 122, 105, 191, 255, 45, 174, 22, 176, 99, 223, 31, 8, 33, 34, 32, 68, 254, 181, 159, 137, 155, 71, 142, 179, 121, 66, 113, 7, 130, 29, 252, 108, 248, 232, 41, 0, 57, 4, 102, 175, 104, 200, 129, 114, 180, 135, 188, 199, 88, 5, 107, 110, 167, 153, 35, 78, 95, 80, 236, 93, 238, 216, 195, 172, 111, 25, 215, 132, 112, 10, 162, 165, 120, 220, 189, 46, 148, 160, 16, 203, 38, 6, 72, 145, 62, 136, 166, 146, 147, 21, 229, 54, 152, 168, 67, 140, 14, 36, 117, 125, 11, 245, 47, 43, 208, 40, 1, 185, 230, 170, 169, 193, 217, 144, 186, 184, 76, 28, 24, 98, 206, 187, 173, 149, 204, 74, 26, 218, 202, 20, 171, 243, 128, 190, 213, 118, 51, 53, 151, 251, 234, 124, 3, 241, 55, 182, 221, 225, 42, 73, 103, 64, 119, 75, 9, 226, 210, 82, 58, 156, 81, 163, 18, 211, 253, 250, 19, 240, 177, 86, 219, 237, 48, 115, 44, 90, 239, 192, 63, 85, 139, 207, 201, 96, 69, 209, 157, 249, 79, 194, 154, 109, 164, 197, 12, 56, 23, 205 };
            var tempChk = tempIntArray.Select(x => Convert.ToByte(x)).ToArray();

            for (var i = 0; i < temp.Length; i++)
            {
                Assert.AreEqual(tempChk[i], temp[i]);
            }

            var sign = obj.Sign(orderAmountString, out_trade_no);

            temp = obj.MerchantKeyASCIIByte;
            tempIntArray = new int[] { 231, 198, 105, 219, 224, 99, 196, 133, 131, 91, 129, 232, 238, 17, 227, 76, 212, 166, 158, 189, 237, 150, 6, 242, 162, 120, 233, 5, 26, 62, 187, 88, 102, 59, 116, 222, 13, 127, 65, 235, 15, 244, 143, 61, 161, 89, 49, 83, 141, 70, 178, 126, 134, 106, 50, 123, 39, 30, 122, 87, 191, 255, 45, 174, 22, 176, 52, 223, 31, 8, 33, 34, 32, 68, 254, 181, 159, 137, 155, 71, 142, 179, 121, 66, 113, 7, 130, 29, 252, 108, 248, 247, 41, 0, 57, 4, 246, 175, 104, 200, 60, 114, 180, 135, 188, 199, 183, 100, 107, 110, 167, 153, 35, 78, 95, 80, 236, 93, 27, 216, 195, 172, 111, 25, 215, 132, 112, 10, 84, 165, 2, 220, 94, 46, 148, 160, 16, 203, 38, 101, 72, 145, 214, 136, 92, 146, 147, 21, 229, 54, 152, 168, 67, 140, 14, 36, 117, 125, 11, 245, 47, 43, 208, 40, 1, 185, 230, 170, 169, 193, 217, 144, 186, 184, 138, 28, 24, 98, 206, 37, 173, 149, 204, 74, 77, 218, 202, 20, 171, 243, 128, 190, 213, 118, 51, 53, 151, 251, 234, 124, 3, 241, 55, 182, 221, 225, 42, 73, 103, 64, 119, 75, 9, 226, 210, 82, 58, 156, 81, 163, 18, 211, 253, 250, 19, 240, 177, 86, 228, 97, 48, 115, 44, 90, 239, 192, 63, 85, 139, 207, 201, 96, 69, 209, 157, 249, 79, 194, 154, 109, 164, 197, 12, 56, 23, 205 };
            tempChk = tempIntArray.Select(x => Convert.ToByte(x)).ToArray();

            for (var i = 0; i < temp.Length; i++)
            {
                Assert.AreEqual(tempChk[i], temp[i]);
            }

            temp = obj.Md5ASCIIByte;
            tempIntArray = new int[] { 221, 228, 29, 52, 205, 115, 151, 76, 62, 145, 153, 104, 252, 252, 230, 154, 95, 183, 7, 174, 55, 100, 34, 119, 91, 144, 254, 182, 218, 37, 124, 31 };
            tempChk = tempIntArray.Select(x => Convert.ToByte(x)).ToArray();

            for (var i = 0; i < temp.Length; i++)
            {
                Assert.AreEqual(tempChk[i], temp[i]);
            }

            var signChk = "b834398a3ec0c81fad7a3cd33f66ad5e";
            Assert.AreEqual(signChk, sign);

            obj = new SFTPaySignService(merchantKey);
            bool IsSignTrue = obj.IsSignTrue(orderAmountString, out_trade_no, sign);
            Assert.IsTrue(IsSignTrue);
        }

        [TestMethod]
        public void PayOrderTest()
        {
            var payBaseSetting = GetSFTPayBaseSetting();
            var payOrder = new SFTPayOrder(payBaseSetting);
            payOrder.SetPayType(merchant.BankCode);
            payOrder.SetOrderData(order);
            payOrder.Done();

            //測試第三方訂單號碼
            string thirdPartyOrderID = payOrder.GetThirdPartyOderID();
            Assert.AreEqual(thirdPartyOrderID, "SFTPay00000000000001");

            //測試取得簽名前資料(此支付方式非字串加密)
            string signBeforeData = payOrder.GetSignBeforeData();
            string signBeforeDataChk = $"[amount] {orderAmountString}; [outTradeNo] {thirdPartyOrderID}; [merchantKey] {merchantKey}";
            Assert.AreEqual(signBeforeDataChk, signBeforeData);

            //測試InputString
            string sign = "3c10859a8929251eb08646fd3cd9254d";
            string postInputChk = $"accountId={merchantNo}&channel={thirdPartyPayChannel}&payType={thirdPartyPayType}&payWay={thirdPartyPayWay}&amount={orderAmountString}&outTradeNo={thirdPartyOrderID}&contentType=json&notifyUrl={notifyUrl}&successUrl={returnUrl}&sign={sign}";
            string postInput = payOrder.GetPostInputString();
            Assert.AreEqual(postInputChk, postInput);
        }

        [TestMethod]
        public void PayReturnTest()
        {
            //實際回應測試
            var jsonObj = new SFTPayReturnModel
            {
                code = 200,
                msg = "success",
                userMsg = null,
                status = true,
                time = 1572849560797,
                data = "https://example.invalid/order/pay/gateway.do?id=1911041439207160001",
            };
            var payReturnJsonString = JsonConvert.SerializeObject(jsonObj);
            var payReturn = new SFTPayReturn(payReturnJsonString);

            //測試是否成功
            Assert.IsTrue(payReturn.IsSuccess());

            //測試回傳訊息
            Assert.AreEqual(payReturn.GetMessage(), jsonObj.msg);

            //測試回傳網址
            Assert.AreEqual(payReturn.GetReturnUrl(), jsonObj.data);

            //實際錯誤回應測試
            jsonObj = new SFTPayReturnModel
            {
                code = 400,
                msg = "fail",
                userMsg = "签名不正确，请检查!",
                status = false,
                time = 1572943262199,
                data = null,
            };
            payReturnJsonString = JsonConvert.SerializeObject(jsonObj);
            payReturn = new SFTPayReturn(payReturnJsonString);

            //測試是否成功
            Assert.IsFalse(payReturn.IsSuccess());

            //測試回傳訊息
            Assert.AreEqual(payReturn.GetMessage(), jsonObj.userMsg);

            //測試回傳網址
            Assert.AreEqual(payReturn.GetReturnUrl(), string.Empty);
        }

        [TestMethod]
        public void PayResponseTest()
        {
            string thirdPartyOrderIDChk = "SFTPay00000000000001";

            var payBaseSetting = GetSFTPayBaseSetting();
            var signChk = "3c10859a8929251eb08646fd3cd9254d";

            var payResponseModel = new SFTPayResponseModel
            {
                tradeId = thirdPartyOrderIDChk,
                outTradeNo = thirdPartyOrderIDChk,
                amount = orderAmountString,
                amountFee = "1.00",
                remark = "",
                sign = signChk,
            };
            var payResponse = new SFTPayResponse(payResponseModel);

            //測試資料庫訂單ID是否正確
            int orderID = payResponse.GetOrderID();
            int orderIDChk = 1;
            Assert.AreEqual(orderID, orderIDChk);

            //測試取得交易訂單ID(給第三方的訂單ID)
            string thirdPartyOrderID = payResponse.GetThirdPartyOderID();
            Assert.AreEqual(thirdPartyOrderID, thirdPartyOrderIDChk);

            //測試金額
            int amount = payResponse.GetPayAmount();
            Assert.AreEqual(amount, orderAmount);

            //測試sign是否正確
            Assert.IsTrue(payResponse.IsSignTrue(payBaseSetting));

            //取得驗證簽名MD5使用字串
            string signBeforeDataChk = $"[amount] {orderAmountString}; [outTradeNo] {thirdPartyOrderID}; [merchantKey] {merchantKey}";
            string signBeforeData = payResponse.GetSingBeforeData();
            Assert.AreEqual(signBeforeData, signBeforeDataChk);
        }
    }

    [TestClass]
    public class DSDFPayTest
    {
        #region 測試用假資料
        private static string merchantNo = "2545";
        private static string merchantKey = "1atfPweQEE5C2lTyIeWcia9nJshd5xSeewmNATaTmi4ZmjNmUIYQau281U2phzkF";
        private static int orderAmount = 100;
        private static long orderAmountLong = orderAmount * 100;
        
        private string thirdPartyPayType = "qrcode";
        private string thirdPartyPayTflag = "ALIPAY";

        private static DateTime dtUtNow = DateTime.UtcNow;
        private static long timeStamp = dtUtNow.CovertToUnixTimestamp();

        private CDPayOrder order = ThirdPartyPayment_TestDataGenerator.GetDefaultOrder(orderAmount);
        private CDMerchantUUID merchant = ThirdPartyPayment_TestDataGenerator.GetDefaultMerchant();

        private ThirdPartyPayMerchantInfoSetting GetDSDFPayBaseSetting()
        {
            string infoData = ThirdPartyPayment_TestDataGenerator.GetDefaultInfoData(merchantNo, merchantKey);

            string url = "https://example.invalid";
            ThirdPartyPayMerchantInfoSetting baseSetting = new ThirdPartyPayMerchantInfoSetting(infoData, url);
            return baseSetting;
        }
        #endregion

        [TestMethod]
        public void DSDFPaySignServiceTest()
        {
            string thirdPartyOrderID = "DSDFPay0000000000001";
            string signBeforeString = $"cid={merchantNo}&uid={order.UserID}&time={timeStamp}&amount={orderAmount}&order_id={thirdPartyOrderID}&ip={order.IPAddress}&{merchantKey}";

            var obj = new DSDFPaySignService(merchantKey);
            string sign = obj.Sign(signBeforeString);

            obj = new DSDFPaySignService(merchantKey);
            bool isSignTrue = obj.IsSignTrue(signBeforeString, sign);
            Assert.IsTrue(isSignTrue);
        }

        [TestMethod]
        public void DSDFPayTypeBankCodeTest()
        {
            string type;
            string tflag;
            string bankcode;
            //支付寶QR
            bankcode = "ZHIFUBAO_QR";
            var obj = new DSDFPayTypeBankCode(bankcode);
            type = obj.GetPayType();
            Assert.AreEqual(type, "qrcode");

            tflag = obj.GetPayTflag();
            Assert.AreEqual(tflag, "ALIPAY");

            //支付寶H5
            bankcode = "ZHIFUBAO_H5";
            obj = new DSDFPayTypeBankCode(bankcode);
            type = obj.GetPayType();
            Assert.AreEqual(type, "remit");

            tflag = obj.GetPayTflag();
            Assert.AreEqual(tflag, "ALIPAY");

            //微信QR
            bankcode = "WEIXIN_QR";
            obj = new DSDFPayTypeBankCode(bankcode);
            type = obj.GetPayType();
            Assert.AreEqual(type, "qrcode");

            tflag = obj.GetPayTflag();
            Assert.AreEqual(tflag, "WebMM");

            //微信H5
            bankcode = "WEIXIN_H5";
            obj = new DSDFPayTypeBankCode(bankcode);
            type = obj.GetPayType();
            Assert.AreEqual(type, "remit");

            tflag = obj.GetPayTflag();
            Assert.AreEqual(tflag, "WebMM");

            //QQQR
            bankcode = "QQ_QR";
            obj = new DSDFPayTypeBankCode(bankcode);
            type = obj.GetPayType();
            Assert.AreEqual(type, "qrcode");

            tflag = obj.GetPayTflag();
            Assert.AreEqual(tflag, "QQPAY");

            //QQQR
            bankcode = "QQ_H5";
            obj = new DSDFPayTypeBankCode(bankcode);
            type = obj.GetPayType();
            Assert.AreEqual(type, "remit");

            tflag = obj.GetPayTflag();
            Assert.AreEqual(tflag, "QQPAY");

            //雲閃付QR
            bankcode = "QUICKPASS_QR";
            obj = new DSDFPayTypeBankCode(bankcode);
            type = obj.GetPayType();
            Assert.AreEqual(type, "qrcode");

            tflag = obj.GetPayTflag();
            Assert.AreEqual(tflag, "UNIPAY");

            //網銀
            bankcode = "UNIONPAY_ONLINE_BANK";
            obj = new DSDFPayTypeBankCode(bankcode);
            type = obj.GetPayType();
            Assert.AreEqual(type, "online");

            tflag = obj.GetPayTflag();
            Assert.AreEqual(tflag, "ALIPAY");

            //快捷
            bankcode = "EXPRESSPAY_H5";
            obj = new DSDFPayTypeBankCode(bankcode);
            type = obj.GetPayType();
            Assert.AreEqual(type, "quick");

            tflag = obj.GetPayTflag();
            Assert.AreEqual(tflag, "ALIPAY");
        }

        [TestMethod]
        public void PayOrderTest()
        {
            var payBaseSetting = GetDSDFPayBaseSetting();
            var payOrder = new DSDFPayOrder(payBaseSetting);
            payOrder.SetPayType(merchant.BankCode);
            payOrder.SetOrderData(order);
            payOrder.Done();

            //測試第三方訂單號碼
            string thirdPartyOrderID = payOrder.GetThirdPartyOderID();
            Assert.AreEqual(thirdPartyOrderID, "DSDFPay0000000000001");

            //測試取得簽名前資料(此支付方式非字串加密)
            string signBeforeString = payOrder.GetSignBeforeString();
            string signBeforeStringChk = $"cid={merchantNo}&uid={order.UserID}&time={timeStamp}&amount={orderAmount}&order_id={thirdPartyOrderID}&ip={order.IPAddress}";
            Assert.AreEqual(signBeforeStringChk, signBeforeString);

            //測試InputString
            var obj = new DSDFPaySignService(merchantKey);
            string sign = obj.Sign(signBeforeString);

            string postInputChk = $"cid={merchantNo}&uid={order.UserID}&time={timeStamp}&amount={orderAmount}&order_id={thirdPartyOrderID}&ip={order.IPAddress}&sign={sign}&type={thirdPartyPayType}&tflag={thirdPartyPayTflag}";
            string postInput = payOrder.GetPostInputString();
            Assert.AreEqual(postInputChk, postInput);
        }

        [TestMethod]
        public void PayResponseTest()
        {
            var payBaseSetting = GetDSDFPayBaseSetting();

            string thirdPartyOrderIDChk = "DSDFPay00000000000001";
            string signBeforeStringChk = $"order_id={thirdPartyOrderIDChk}&amount={orderAmountLong}&verified_time={timeStamp}";

            var signObj = new DSDFPaySignService(payBaseSetting.MerchantInfo.MERCHANT_KEY);
            string signChk =  signObj.Sign(signBeforeStringChk);

            var payResponseModel = new DSDFPayResponseModel
            {
                order_id = thirdPartyOrderIDChk,
                direction = "in",
                amount = orderAmountLong,
                orig_amount = orderAmountLong,
                customer_name = order.UserID.ToString(),
                verified_time = timeStamp,
                created_time = timeStamp,
                status = "verified",
                type = "qrcode",
                customer_bankflag = "ALIPAY",
                to_bankflag = "",
                to_cardnumber = "",
                to_username = "",
                is_mobile = true,
                is_wap = true,
                qsign = signChk,
            };
            string payResponseModelJsonString = JsonConvert.SerializeObject(payResponseModel).ToString(); ;
            var payResponse = new DSDFPayResponse(payResponseModelJsonString);

            //測試資料庫訂單ID是否正確
            int orderID = payResponse.GetOrderID();
            int orderIDChk = 1;
            Assert.AreEqual(orderID, orderIDChk);

            //測試取得交易訂單ID(給第三方的訂單ID)
            string thirdPartyOrderID = payResponse.GetThirdPartyOderID();
            Assert.AreEqual(thirdPartyOrderID, thirdPartyOrderIDChk);

            //測試金額
            int amount = payResponse.GetPayAmount();
            Assert.AreEqual(amount, orderAmount);

            //確認是否為充值
            Assert.IsTrue(payResponse.IsPayment());

            //確認是否支付成功
            Assert.IsTrue(payResponse.IsPaySuccess());

            //測試sign是否正確
            Assert.IsTrue(payResponse.IsSignTrue(payBaseSetting));

            //取得驗證簽名MD5使用字串
            string signBeforeString = payResponse.GetSingBeforeString();
            Assert.AreEqual(signBeforeString, signBeforeStringChk);
        }
    }
}
