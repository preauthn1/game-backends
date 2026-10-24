using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using target.DA.Repository;
using target.Models;
using target.Models.Enums;
using target.Models.Models;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Utility.Helper.HttpContext;
using target.Utility.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using example.invalid;

namespace target.Testing
{
    [DoNotParallelize]
    [TestClass]
    public class IPHelperTests : BaseTest
    {

        /// <summary>
        /// 透過API檢查cloudflare 是否有新的IPs
        /// </summary>
        [TestMethod]
        public void IPHelper_CheckCloudflare_IP_RangeUpdate_Test()
        {
            var cf_api_result = IPHelper.FetchCloudflareIPRange();

            string hintMsg = "Cloudflare 有新IPs，需要加進去IPHelper中 ";

            foreach (var ip in cf_api_result.result.ipv4_cidrs)
            {
                if(!IPHelper.CloudflareCidrs.Any(x=> x == ip))
                {
                    throw new TerproException(ApiErrorCode.Forbidden_IPAddressNotAllow, new ErrorDetailModel(hintMsg));
                }
            }

            foreach (var ip in cf_api_result.result.ipv6_cidrs)
            {
                if (!IPHelper.CloudflareCidrs.Any(x => x == ip))
                {
                    throw new TerproException(ApiErrorCode.Forbidden_IPAddressNotAllow, new ErrorDetailModel(hintMsg));
                }
            }

            return;
        }


        [TestMethod]
        public void IsCloudflareNode_Test()
        {
            Assert.IsTrue(IPHelper.IsCloudflareNode(IPAddress.Parse("0.0.0.0")));
            Assert.IsTrue(IPHelper.IsCloudflareNode(IPAddress.Parse("0.0.0.0")));
            Assert.IsTrue(IPHelper.IsCloudflareNode(IPAddress.Parse("0.0.0.0")));
            Assert.IsTrue(IPHelper.IsCloudflareNode(IPAddress.Parse("0.0.0.0")));
            Assert.IsTrue(IPHelper.IsCloudflareNode(IPAddress.Parse("0.0.0.0")));

            Assert.IsFalse(IPHelper.IsCloudflareNode(IPAddress.Parse("0.0.0.0")));
            Assert.IsFalse(IPHelper.IsCloudflareNode(IPAddress.Parse("0.0.0.0")));
            return;
        }



        [TestMethod]
        public void IsCompanyIP_Test()
        {
            Assert.IsTrue(IPHelper.IsCompanyIP("0.0.0.0"));
            Assert.IsFalse(IPHelper.IsCompanyIP("0.0.0.0"));
            Assert.IsFalse(IPHelper.IsCompanyIP("0.0.0.0"));

            return;
        }



        [TestMethod]
        public void GetIP_Test()
        {
            string myTestIP1 = "0.0.0.0";
            string myTestIP2 = "0.0.0.0";

            //若沒有帶入 HeaderKey_PGS_Nginx_AlwaysHeader 會判斷是偽造
            var fake_nginx_mockHeaders = new HeaderDictionary(new Dictionary<String, StringValues>
                    {
                        { SystemConstant.HeaderKey_Lang ,LanguageEnum.zh_cn.ToString()},
                        { SystemConstant.HeaderKey_HDCode , SystemConstant.DefaultDeviceHDCode},
                        { "X-Real-IP" , myTestIP2},

                    }) as IHeaderDictionary;

            SetMockHttpHeaders(fake_nginx_mockHeaders);

            var get_my_ip_001 = IPHelper.GetIP();
            Assert.IsTrue(get_my_ip_001 == SystemConstant.CompanyIPAddress);

            var mockHeaders2 = new HeaderDictionary(new Dictionary<String, StringValues>
                    {
                        { SystemConstant.HeaderKey_Lang ,LanguageEnum.zh_cn.ToString()},
                        { SystemConstant.HeaderKey_HDCode , SystemConstant.DefaultDeviceHDCode},
                        { SystemConstant.HeaderKey_PGS_Nginx_AlwaysHeader , "true"},
                        { "X-Real-IP" , myTestIP2},
                        { "X-Original-Forwarded-For" , myTestIP1},

                    }) as IHeaderDictionary;

            SetMockHttpHeaders(mockHeaders2);

            var get_my_ip_002 = IPHelper.GetIP();

            // Header 即使宣稱來自 Nginx，實際連線來源不是可信代理時仍不可採信。
            Assert.IsTrue(get_my_ip_002 == SystemConstant.CompanyIPAddress);

            SetMockHttpHeaders(mockHeaders2, IPAddress.Parse("0.0.0.0"));

            var get_my_ip_003 = IPHelper.GetIP();

            Assert.IsTrue(get_my_ip_003 == myTestIP2);

            return;
        }

        [TestMethod]
        public void GetIP_TrustedProxyChain_ReturnsFirstUntrustedIPAddress_Test()
        {
            var headers = new HeaderDictionary(new Dictionary<String, StringValues>
            {
                { SystemConstant.HeaderKey_PGS_Nginx_AlwaysHeader, "true" },
                { "X-Real-IP", "0.0.0.0" },
                { "X-Forwarded-For", "0.0.0.0, 0.0.0.0,0.0.0.0, 0.0.0.0" },
                { "X-Original-Forwarded-For", "0.0.0.0, 0.0.0.0,0.0.0.0" }
            }) as IHeaderDictionary;

            SetMockHttpHeaders(headers, IPAddress.Parse("0.0.0.0"));

            Assert.AreEqual("0.0.0.0", IPHelper.GetIP());
        }

        [TestMethod]
        public void GetIP_UntrustedRemoteAddress_IgnoresSpoofedForwardingHeaders_Test()
        {
            var headers = new HeaderDictionary(new Dictionary<String, StringValues>
            {
                { SystemConstant.HeaderKey_PGS_Nginx_AlwaysHeader, "true" },
                { "X-Real-IP", "0.0.0.0" },
                { "X-Forwarded-For", "0.0.0.0" }
            }) as IHeaderDictionary;

            SetMockHttpHeaders(headers, IPAddress.Parse("0.0.0.0"));

            Assert.AreEqual("0.0.0.0", IPHelper.GetIP());
        }



        [TestMethod]
        public void MockerHeaders_ByJson_GetIP_Test()
        {
            string jsonText = @"{
   ""Content-Type"":""application/json"",
   ""Accept"":""application/json, text/plain, */*"",
   ""Cookie"":""locale=zh-TW"",
   ""Referer"":""https://example.invalid/"",
   ""User-Agent"":""Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/0.0.0.0 Safari/537.36 Edg/0.0.0.0"",
   ""Origin"":""https://example.invalid"",
   ""X-Real-IP"":""0.0.0.0"",
   ""X-Forwarded-For"":""0.0.0.0"",
   ""X-Forwarded-Scheme"":""https"",
   ""X-Original-Forwarded-For"":""0.0.0.0,0.0.0.0,0.0.0.0"",
   ""X-Request-Start"":""t=1739262373.011"",
   ""cf-ray"":""9102f3e73a6fb2fc-TPE"",
   ""CF-IPCountry"":""TW"",
   ""CF-Visitor"":""{\""scheme\"":\""https\""}"",
   ""locale"":""zh-TW"",
   ""x-target-device-width"":""1912"",
   ""hdcode"":""643cf610-e84c-11ef-a62a-83a086254f35"",
   ""sec-ch-ua-mobile"":""?0"",
   ""x-target-device-height"":""920"",
   ""sec-ch-ua"":""\""Not A(Brand\"";v=\""8\"", \""Chromium\"";v=\""132\"", \""Microsoft Edge\"";v=\""132\"""",
   ""sec-ch-ua-platform"":""\""Windows\"""",
   ""sec-fetch-site"":""same-origin"",
   ""sec-fetch-mode"":""cors"",
   ""cdn-loop"":""cloudflare; loops=2"",
   ""cf-connecting-ip"":""0.0.0.0"",
   ""sec-fetch-dest"":""empty"",
   ""priority"":""u=1, i"",
   ""RequestBeginTime"":""2025-02-11 08:26:13"",
   ""DebugInfo2"":""【marshall】"",
   ""CustomizedPayloadForLog"":""{\""Account\"":\""marshall\"",\""Password\"":\""******\"",\""Device\"":\""643cf610-e84c-11ef-a62a-83a086254f35\"",\""Sign\"":\""6c4db28f43220db1efd3f772530d9511\"",\""ServerHash\"":\""51eaef693b3bcfb0ed571e5139ef0077\"",\""Auth2FACode\"":\""\""}""
}";

            var convertToDic = jsonText.JsonConvertToModel<Dictionary<string, string>>();

            var mockHeaders = new HeaderDictionary(new Dictionary<String, StringValues>());

            foreach(var item in convertToDic)
            {
                mockHeaders.Add(new KeyValuePair<string, StringValues>(item.Key, item.Value));
            }

            SetMockHttpHeaders(mockHeaders);

            var get_my_ip_001 = IPHelper.GetIP();

            return;
        }



        [TestMethod]
        public void IsIPv6_Test()
        {
            Assert.IsFalse(IPHelper.IsIPv6("0.0.0.0"));
            Assert.IsFalse(IPHelper.IsIPv6("0.0.0.0"));
            Assert.IsFalse(IPHelper.IsIPv6("0.0.0.0"));
            Assert.IsFalse(IPHelper.IsIPv6("XXXXXXXXX"));
            Assert.IsFalse(IPHelper.IsIPv6("AAAAAAAA"));

            Assert.IsTrue(IPHelper.IsIPv6("2400:8901::f03c:92ff:fe86:8f6f"));
            Assert.IsTrue(IPHelper.IsIPv6("2400:8901::f03c:92ff:fec2:d651"));

            return;
        }



        [TestMethod]
        public void AnalyzeIPsAndCombineOne_Test()
        {
            string companyIP = SystemConstant.CompanyIPAddress;
            string myIPv6 = "2001:db8:3333:4444:5555:6666:7777:8888";

            List<string> myIPs = new List<string>
            {
                //有多組/24 (案例1)
                "0.0.0.0",
                "0.0.0.0",
                "0.0.0.0",
                "0.0.0.0",
                "0.0.0.0",

                //有多組/24 (案例1)
                "0.0.0.0",
                "0.0.0.0",

                //其他
                "0.0.0.0",
                "0.0.0.0",
                "0.0.0.0",
                "0.0.0.0",
                "0.0.0.0",
                companyIP,
                myIPv6,
            };

            var result = IPHelper.AnalyzeIPsAndCombineOne(myIPs);

            Assert.IsTrue(result.Any(x => x == "192.168.120.*"));
            Assert.IsTrue(result.Any(x => x == "200.11.199.*"));
            Assert.IsTrue(result.Any(x => x == companyIP));
            Assert.IsTrue(result.Any(x => x == myIPv6));

            Assert.IsFalse(result.Any(x => x == "0.0.0.0"));
            Assert.IsFalse(result.Any(x => x == "0.0.0.0"));
            return;
        }




        private void SetMockHttpHeaders(IHeaderDictionary mockHeaders, IPAddress remoteIPAddress = null)
        {
            var myHttpContext = Substitute.For<IHttpContextAccessor>();

            //設定假的實體的Add方法當傳入3,6 回傳 9
            myHttpContext.HttpContext.Request.Method = "GET";
            myHttpContext.HttpContext.Request.Headers.Returns(mockHeaders);
            myHttpContext.HttpContext.Connection.RemoteIpAddress.Returns(remoteIPAddress ?? new MockIPAddress(0));

            MyHttpContext.Configure(myHttpContext);
        }






        [TestMethod]
        public void CheckIsMarshallHomeIPAddress_Test()
        {
            if (IsSkipTest_WhenSmallerThanTargetTime(new DateTime(2024, 7, 14)))
                return;

            Assert.IsFalse(IPHelper.CheckIsMarshallHomeIPAddress("0.0.0.0"));
            Assert.IsFalse(IPHelper.CheckIsMarshallHomeIPAddress("0.0.0.0"));
            Assert.IsTrue(IPHelper.CheckIsMarshallHomeIPAddress("0.0.0.0"));
            return;
        }


    }
}
