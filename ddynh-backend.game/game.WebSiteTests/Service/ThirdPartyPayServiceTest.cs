using Microsoft.VisualStudio.TestTools.UnitTesting;
using game.WebSite.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using game.Entity.Payment;
using game.Infrastructure.Extensions;
using System.Security.Cryptography;

namespace game.WebSite.Services.Tests
{
    [TestClass()]
    public class ThirdPartyOderIDServiceTests
    {
        [TestMethod()]
        public void GetThirdPartyOderIDTest()
        {
            string TradeNo;
            string chkTradeNo;

            //測試丟非法數值
            int orderID1 = 0;
            string paymentName1 = string.Empty;
            int length1 = 0;

            TradeNo = ThirdPartyOderIDService.GetThirdPartyOderID(orderID1, paymentName1, length1);
            chkTradeNo = string.Empty;
            Assert.AreEqual(chkTradeNo, TradeNo);

            int orderID2 = 0;
            string paymentName2 = "123";

            TradeNo = ThirdPartyOderIDService.GetThirdPartyOderID(orderID2, paymentName2);
            chkTradeNo = string.Empty;
            Assert.AreEqual(chkTradeNo, TradeNo);

            int orderID3 = 1;
            string paymentName3 = string.Empty;

            TradeNo = ThirdPartyOderIDService.GetThirdPartyOderID(orderID3, paymentName3);
            chkTradeNo = string.Empty;
            Assert.AreEqual(chkTradeNo, TradeNo);

            //測試合法數值
            int orderID4 = 1234567890;
            string paymentName4 = "test";
            int length4 = 5;

            TradeNo = ThirdPartyOderIDService.GetThirdPartyOderID(orderID4, paymentName4, length4);
            chkTradeNo = "67890";
            Assert.AreEqual(chkTradeNo, TradeNo);

            int orderID5 = 1;
            string paymentName5 = "test";

            TradeNo = ThirdPartyOderIDService.GetThirdPartyOderID(orderID5, paymentName5);
            chkTradeNo = "test000001";
            Assert.AreEqual(chkTradeNo, TradeNo);

            int orderID6 = 1;
            string paymentName6 = "test";
            int length6 = 15;

            TradeNo = ThirdPartyOderIDService.GetThirdPartyOderID(orderID6, paymentName6, length6);
            chkTradeNo = "test00000000001";
            Assert.AreEqual(chkTradeNo, TradeNo);

            int orderID7 = 1234567890;
            string paymentName7 = "GiFuBoPay";
            int length7 = 20;

            TradeNo = ThirdPartyOderIDService.GetThirdPartyOderID(orderID7, paymentName7, length7);
            chkTradeNo = "GiFuBoPay01234567890";
            Assert.AreEqual(chkTradeNo, TradeNo);
        }
        [TestMethod()]
        public void GetOrderIDTest()
        {
            string thirdPartyOderID;
            string paymentName;
            int orderID;
            int orderIDChk;

            paymentName = "GiFuBoPay";
            thirdPartyOderID = paymentName + "01234567890";
            orderID = ThirdPartyOderIDService.GetOrderID(thirdPartyOderID, "GiFuBoPay");
            orderIDChk = 1234567890;
            Assert.AreEqual(orderID, orderIDChk);

            paymentName = "test";
            thirdPartyOderID = paymentName + "000000000000001234567890";
            orderID = ThirdPartyOderIDService.GetOrderID(thirdPartyOderID, "test");
            orderIDChk = 1234567890;
            Assert.AreEqual(orderID, orderIDChk);
        }
    }

    [TestClass()]
    public class StringObjectDirctionaryServiceTests
    {
        [TestMethod()]
        public void ConvertToUrlStringFormatTest()
        {
            string UrlString;
            string ChkUrlString;
            //測試空值
            var dic1 = new Dictionary<string, object>();
            UrlString = StringObjectDirctionaryService.ConvertToUrlStringFormat(dic1);
            ChkUrlString = string.Empty;
            Assert.AreEqual(ChkUrlString, UrlString);

            //測試dic空值，separator非空值
            var dic2 = new Dictionary<string, object>();
            UrlString = StringObjectDirctionaryService.ConvertToUrlStringFormat(dic2, "&");
            ChkUrlString = string.Empty;
            Assert.AreEqual(ChkUrlString, UrlString);

            //測試dic非空值，separator空值
            var dic3 = new Dictionary<string, object>();
            dic3.Add("field1", "value1");
            dic3.Add("field2", "value2");
            UrlString = StringObjectDirctionaryService.ConvertToUrlStringFormat(dic3);
            ChkUrlString = "field1=value1field2=value2";
            Assert.AreEqual(ChkUrlString, UrlString);

            //測試dic非空值，separator非空值
            var dic4 = new Dictionary<string, object>();
            string separator = "&";
            dic4.Add("field1", "value1");
            dic4.Add("field2", "value2");
            dic4.Add("field3", "value3");
            UrlString = StringObjectDirctionaryService.ConvertToUrlStringFormat(dic4, separator);
            ChkUrlString = $"field1=value1{separator}field2=value2{separator}field3=value3";
            Assert.AreEqual(ChkUrlString, UrlString);
        }

        [TestMethod()]
        public void SortStrDictionaryTest()
        {
            //測試dic空值，sortArr空值
            Dictionary<string, object> dic = new Dictionary<string, object>();
            Dictionary<string, object> dicChk = new Dictionary<string, object>();
            List<string> sortArr = new List<string>();


            var SortStrDictionary = StringObjectDirctionaryService.SortStrDictionary(dic, sortArr);
            Assert.IsTrue(SortStrDictionary.IsEmptyOrNull());

            //測試dic空值，sortArr非空值
            dic.Clear();
            sortArr.Clear();
            sortArr = new List<string> { "test1","test2"};
            SortStrDictionary = StringObjectDirctionaryService.SortStrDictionary(dic, sortArr);
            Assert.IsTrue(SortStrDictionary.IsEmptyOrNull());

            //測試dic非空值，sortArr空值
            dic.Clear();
            sortArr.Clear();
            dic.Add("field1", "value1");
            dic.Add("field2", "value2");
            SortStrDictionary = StringObjectDirctionaryService.SortStrDictionary(dic, sortArr);
            Assert.AreEqual(SortStrDictionary, dic);

            //測試dic非空值，sortArr非空值
            dic.Clear();
            dicChk.Clear();
            sortArr.Clear();
            dic.Add("field1", "value1");
            dic.Add("field2", "value2");
            dic.Add("field3", "value3");
            sortArr = new List<string> { "field3", "field1", "field2" };
            SortStrDictionary = StringObjectDirctionaryService.SortStrDictionary(dic, sortArr);
            dicChk.Add("field3", "value3");
            dicChk.Add("field1", "value1");
            dicChk.Add("field2", "value2");
            Assert.AreEqual(SortStrDictionary.First(), dicChk.First());
            Assert.AreEqual(SortStrDictionary.Last(), dicChk.Last());
        }

        [TestMethod()]
        public void GetMd5Test()
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            string UrlString;
            string Md5String;
            string ChkMd5String;

            //測試空值
            Md5String = StringObjectDirctionaryService.GetMd5(dic);
            ChkMd5String = MD5Service.Hash(string.Empty, Encoding.UTF8);
            Assert.AreEqual(Md5String, ChkMd5String);

            //測試dic空值，separator非空值
            dic.Clear();
            Md5String = StringObjectDirctionaryService.GetMd5(dic);
            ChkMd5String = MD5Service.Hash(string.Empty, Encoding.UTF8);
            Assert.AreEqual(Md5String, ChkMd5String);

            //測試dic非空值，separator空值
            dic.Clear();
            dic.Add("field1", "value1");
            dic.Add("field2", "value2");
            UrlString = "field1=value1field2=value2";
            Md5String = StringObjectDirctionaryService.GetMd5(dic);
            ChkMd5String = MD5Service.Hash(UrlString, Encoding.UTF8);
            Assert.AreEqual(Md5String, ChkMd5String);

            //測試dic非空值，separator非空值
            dic.Clear();
            string separator = "&";
            dic.Add("field1", "value1");
            dic.Add("field2", "value2");
            dic.Add("field3", "value3");
            UrlString = $"field1=value1{separator}field2=value2{separator}field3=value3";
            Md5String = StringObjectDirctionaryService.GetMd5(dic, separator);
            ChkMd5String = MD5Service.Hash(UrlString, Encoding.UTF8);
            Assert.AreEqual(Md5String, ChkMd5String);
        }

        [TestMethod()]
        public void ConvertToValueAddSeparatorFormatTest()
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            string Md5String;
            string ChkMd5String;

            //測試空值
            Md5String = StringObjectDirctionaryService.ConvertToValueAddSeparatorFormat(dic);
            Assert.AreEqual(Md5String, string.Empty);

            //測試dic空值，separator非空值
            dic.Clear();
            Md5String = StringObjectDirctionaryService.ConvertToValueAddSeparatorFormat(dic);
            Assert.AreEqual(Md5String, string.Empty);

            //測試dic非空值，separator空值
            dic.Clear();
            dic.Add("field1", "value1");
            dic.Add("field2", "value2");
            ChkMd5String = "value1value2";
            Md5String = StringObjectDirctionaryService.ConvertToValueAddSeparatorFormat(dic);
            Assert.AreEqual(Md5String, ChkMd5String);

            //測試dic非空值，separator非空值
            dic.Clear();
            string separator = "&";
            dic.Add("field1", "value1");
            dic.Add("field2", "value2");
            dic.Add("field3", "value3");
            ChkMd5String = $"value1{separator}value2{separator}value3";
            Md5String = StringObjectDirctionaryService.ConvertToValueAddSeparatorFormat(dic, separator);
            Assert.AreEqual(Md5String, ChkMd5String);
        }

        [TestMethod()]
        public void GetMd5ContentTest()
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            string UrlString;
            string Md5String;
            string ChkMd5String;

            //測試空值
            Md5String = StringObjectDirctionaryService.GetMd5Content(dic);
            ChkMd5String = MD5Service.Hash(string.Empty, Encoding.UTF8);
            Assert.AreEqual(Md5String, ChkMd5String);

            //測試dic空值，separator非空值
            dic.Clear();
            Md5String = StringObjectDirctionaryService.GetMd5Content(dic);
            ChkMd5String = MD5Service.Hash(string.Empty, Encoding.UTF8);
            Assert.AreEqual(Md5String, ChkMd5String);

            //測試dic非空值，separator空值
            dic.Clear();
            dic.Add("field1", "value1");
            dic.Add("field2", "value2");
            UrlString = "value1value2";
            Md5String = StringObjectDirctionaryService.GetMd5Content(dic);
            ChkMd5String = MD5Service.Hash(UrlString, Encoding.UTF8);
            Assert.AreEqual(Md5String, ChkMd5String);

            //測試dic非空值，separator非空值
            dic.Clear();
            string separator = "&";
            dic.Add("field1", "value1");
            dic.Add("field2", "value2");
            dic.Add("field3", "value3");
            UrlString = $"value1{separator}value2{separator}value3";
            Md5String = StringObjectDirctionaryService.GetMd5Content(dic, separator);
            ChkMd5String = MD5Service.Hash(UrlString, Encoding.UTF8);
            Assert.AreEqual(Md5String, ChkMd5String);
        }

        [TestMethod()]
        public void ConvertToFieldValueFormatTest()
        {
            string UrlString;
            string ChkUrlString;
            //測試空值
            var dic1 = new Dictionary<string, object>();
            UrlString = StringObjectDirctionaryService.ConvertToFieldValueFormat(dic1);
            ChkUrlString = string.Empty;
            Assert.AreEqual(ChkUrlString, UrlString);

            //測試dic空值，separator非空值
            var dic2 = new Dictionary<string, object>();
            UrlString = StringObjectDirctionaryService.ConvertToFieldValueFormat(dic2, "&");
            ChkUrlString = string.Empty;
            Assert.AreEqual(ChkUrlString, UrlString);

            //測試dic非空值，separator空值
            var dic3 = new Dictionary<string, object>();
            dic3.Add("field1", "value1");
            dic3.Add("field2", "value2");
            UrlString = StringObjectDirctionaryService.ConvertToFieldValueFormat(dic3);
            ChkUrlString = "field1value1field2value2";
            Assert.AreEqual(ChkUrlString, UrlString);

            //測試dic非空值，separator非空值
            var dic4 = new Dictionary<string, object>();
            string separator = "&";
            dic4.Add("field1", "value1");
            dic4.Add("field2", "value2");
            dic4.Add("field3", "value3");
            UrlString = StringObjectDirctionaryService.ConvertToFieldValueFormat(dic4, separator);
            ChkUrlString = $"field1value1{separator}field2value2{separator}field3value3";
            Assert.AreEqual(ChkUrlString, UrlString);
        }
    }

    [TestClass()]
    public class ThirdPartyPaymentPostActionTests
    {
        string logGuid = "12345";
        string logFile = "TestDemo.log";
        //不測試是否正常寫入LOG
        [TestMethod()]
        public void SetOrderDataTest()
        {
            bool chk;

            var obj = new ThirdPartyPaymentPostAction(logGuid, logFile);
            obj.SetIsTesting(true);

            chk = obj.SetOrderData(null);
            Assert.IsFalse(chk);

            var order = new CDPayOrder();
            order.GWID = 1;
            chk = obj.SetOrderData(order);
            Assert.IsTrue(chk);
        }

        [TestMethod()]
        public void SetMerChantTest()
        {
            bool chk;
            var obj = new ThirdPartyPaymentPostAction(logGuid, logFile);
            obj.SetIsTesting(true);
            chk = obj.SetMerChant(null);
            Assert.IsFalse(chk);

            //測試沒有先設定order
            var merchant = new CDMerchantUUID();
            merchant.GWID = 1;
            var obj2 = new ThirdPartyPaymentPostAction(logGuid, logFile);
            obj2.SetIsTesting(true);
            chk = obj2.SetMerChant(merchant);
            Assert.IsFalse(chk);

            var order = new CDPayOrder();
            order.GWID = 1;
            var obj3 = new ThirdPartyPaymentPostAction(logGuid, logFile);
            obj3.SetIsTesting(true);
            obj3.SetOrderData(order);
            chk = obj3.SetMerChant(merchant);
            Assert.IsTrue(chk);
        }

        [TestMethod()]
        public void GetFormAutoSubmitHtml()
        {
            string url = string.Empty;
            string method;
            string returnHtml;
            string returnHtmlChk = string.Empty;
            var dictionary = new Dictionary<string, object>();

            var obj = new ThirdPartyPaymentPostAction(logGuid, logFile);
            obj.SetIsTesting(true);

            //測試全部空值
            returnHtml = obj.GetFormAutoSubmitHtml(dictionary, url);
            Assert.AreEqual(returnHtml, returnHtmlChk);

            //測試部分空值
            dictionary.Clear();
            method = "GET";
            returnHtml = obj.GetFormAutoSubmitHtml(dictionary, url, method);
            Assert.AreEqual(returnHtml, returnHtmlChk);

            dictionary.Clear();
            url = "http://0.0.0.0/";
            returnHtml = obj.GetFormAutoSubmitHtml(dictionary, url);
            Assert.AreEqual(returnHtml, returnHtmlChk);

            //測試全部非空值(method可不給值)
            url = "http://0.0.0.0/";
            dictionary.Add("field1", "value1");
            dictionary.Add("field2", "value2");
            dictionary.Add("field3", "value3");
            returnHtml = obj.GetFormAutoSubmitHtml(dictionary, url);

            returnHtmlChk = "<form method=\"" + "POST" + "\" name=\"form\" action=\"" + url + "\" enctype=\"application/x-www-form-urlencoded\">";
            returnHtmlChk += "<input type=\"hidden\" id=\"" + "field1" + "\" name=\"" + "field1" + "\" value=\"" + "value1" + "\">";
            returnHtmlChk += "<input type=\"hidden\" id=\"" + "field2" + "\" name=\"" + "field2" + "\" value=\"" + "value2" + "\">";
            returnHtmlChk += "<input type=\"hidden\" id=\"" + "field3" + "\" name=\"" + "field3" + "\" value=\"" + "value3" + "\">";
            returnHtmlChk += "</form>";
            returnHtmlChk += "<script>window.onload = function(){document.forms[\"form\"].submit();}</script>";
            Assert.AreEqual(returnHtml, returnHtmlChk);

            dictionary.Clear();
            method = "GET";
            url = "http://0.0.0.0/";
            dictionary.Add("field1", "value1");
            dictionary.Add("field2", "value2");
            dictionary.Add("field3", "value3");
            returnHtml = obj.GetFormAutoSubmitHtml(dictionary, url, method);

            returnHtmlChk = "<form method=\"" + "GET" + "\" name=\"form\" action=\"" + url + "\" enctype=\"application/x-www-form-urlencoded\">";
            returnHtmlChk += "<input type=\"hidden\" id=\"" + "field1" + "\" name=\"" + "field1" + "\" value=\"" + "value1" + "\">";
            returnHtmlChk += "<input type=\"hidden\" id=\"" + "field2" + "\" name=\"" + "field2" + "\" value=\"" + "value2" + "\">";
            returnHtmlChk += "<input type=\"hidden\" id=\"" + "field3" + "\" name=\"" + "field3" + "\" value=\"" + "value3" + "\">";
            returnHtmlChk += "</form>";
            returnHtmlChk += "<script>window.onload = function(){document.forms[\"form\"].submit();}</script>";
            Assert.AreEqual(returnHtml, returnHtmlChk);
        }
    }

    [TestClass()]
    public class RSAServiceTest
    {
        private string privateKeyContent = "[PRIVATE_KEY_BLOCK_REDACTED]\n";
        //此為AE的公鑰與上方的私鑰為一組，但json存的公鑰為三方的公鑰，故不相同。
        private string publicKeyContent = "-----BEGIN PUBLIC KEY-----\nMIICIjANBgkqhkiG9w0BAQEFAAOCAg8AMIICCgKCAgEA3Y3UIUusQXrLj+LE6KaM\nZ8b/6WcEro250hdpvtKgaEoYTAvOubzPbhClkz8cjPSz/qwyxxXnuwyWDgLC2onG\nY8My8HbKGnYBKsDpq/6QIt8N/Z7mu0BawRRSDIlMvDcW2gK9pU6yPx93JL8caYxa\nXhqugSUuW4VUwQUFsJOlI3rNb9L7Qs21pyAMT8T///+7KpqXT7k8YfYouoKKq+t1\nfEcfwW3hYImeaZxnIhKr9DjbxjvXq9aBZllsOw4Ifr6vTNEefNNNzhcWdGIZ2dD3\nmLvQkanxj79eI8C2rY6f3U88HKb8SUg/ae8PfrGQqIhTaQHNmaaUnGaWvYYa1Iqc\nnzCdiWirp2BhmbJ1hNgp3sPThq4tiPCLxEOig/3KJxATjWTkAwPpha2M45/s8GQ5\nXr4SjTaPmkI6Dhu+cV4AXjGDrl7GcxdLzqdr9FW1XM++3vJhdcRSGdxoDCUFwtfR\nMJ+01kIGE+Xj3nJj71yDsyKBMdm1WZN9TC7UXHMrygesBKe9er0rKZl1OlVCqK0S\n2ppS5Lt6PlcO+ygVfBspfeVapdYL//pBzt4K/ogUmPYPJyrIBfOECFRop49EGIro\nPEU72+xnkATg5CEgeEFb8F3CFfdhgPpy8JbPPaqZMXVlGspgfOFFOmFVM/4CvAYq\nbqrfACW9LBoisgRq3frQT3sCAwEAAQ==\n-----END PUBLIC KEY-----\n";

        [TestMethod()]
        public void RSAEncryptAndRSADecryptTest()
        {
            string content = "test123";
            
            string rsaTempString = RSAService.Sign(privateKeyContent, content);
            bool IsSuccess = RSAService.SignChk(publicKeyContent, content, rsaTempString);
            Assert.AreEqual(IsSuccess, true);
        }

    }
}