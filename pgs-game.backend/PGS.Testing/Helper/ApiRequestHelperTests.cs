using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.Models;
using target.Models.Enums;
using target.Models.Models;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Utility.Utility;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class APIRequestHelperTests : BaseTest
    {

        [TestMethod]
        public void ApiRequestHelper_Timeout_Test()
        {
            var beginTime = DateTimeHelper.GetCurrentTime();

            int timeout = 1000 * 3;

            var apiResult = APIRequestHelper.RequestApiResult<string>(new RequestParameters
            {
                BaseUrl = "https://example.invalid",
                ApiUrl = "testcase/testTimeout",
                Method = HttpMethodType.HttpGet,
                TimeoutMillisecond = timeout,
                AdvancedSetting = new AdvancedSetting
                {

                }
            });

            var endTime = DateTimeHelper.GetCurrentTime();

            var totalMilliseconds = (endTime - beginTime).TotalMilliseconds;

            Assert.IsTrue(apiResult.IsTimeoutException);
            Assert.IsTrue(totalMilliseconds >= (timeout - 500) && totalMilliseconds <= (timeout + 500));

            return;

        }



        [TestMethod]
        public void ApiRequestHelper_RequestApiResult_Test()
        {
            var apiResult = APIRequestHelper.RequestApiResult<string>(new RequestParameters
            {
                BaseUrl = SystemConstant.DocumentUrl_VendorAPIUrl,
                ApiUrl = APIRequestHelper.No_Url_Keyword,
                Method = HttpMethodType.HttpGet
            });

            if(apiResult.Exception != null)
            {
                Assert.Fail("Http請求壞掉了" + apiResult.Exception.Message);
            }

            return;

        }
		
        
    }
}
