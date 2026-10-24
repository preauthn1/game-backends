using game.Entity.Game;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using target.Core.Repository;
using target.Models;
using target.Models.Enums;
using target.Models.Models;
using target.Models.ViewModels;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Utility.Helper.HttpContext;
using target.Utility.IOC;
using System;
using System.Collections.Generic;
using System.Linq;
using example.invalid;
using System.Reflection;
using System.Web;

namespace target.Testing
{

    public class BaseTest
    {

        //禁止平行測試 可以標記 [DoNotParallelize]

        public static TestDataConfig _TEST_DATA => new TestDataConfig();


        public BaseTest()
        {
            //如果沒呼叫這段 讀config會出問題
            RuntimeHelper.Initialize("Dev");

            var services = new ServiceCollection();
            RuntimeHelper.RegisterHttpClient(services);
            RuntimeHelper.SetGlobalServiceProvider(services.BuildServiceProvider());          


            InitializeIOC();
            InstanceAllRepositoryImplement();
            MockHttpRequest();

            if(SettingHelper.IsProductionDbConnection)
            {
                throw new TerproException(ApiErrorCode.BusinessLogicDeny , "嚴禁使用production DB運行");
            }
        }


        private void ToDoList()
        {
            //備忘錄一些需要補上的測試
            //備忘錄一些需要補上的測試
            //備忘錄一些需要補上的測試


            //後台報表API 跟 VendorAPi做比對
            //SendEventPrizeCallbackMerchantApi 可能需要簡單測試

        }



        protected string GeneratePlayerJwtToken(int playerID , int merchantID , string playerName , string sessionToken = "")
        {
            if(string.IsNullOrWhiteSpace(sessionToken))
            {
                sessionToken = [REDACTED];
            }

            var param = new GameLaunchRequestModel
            {
                MerchantID = merchantID,
                PlayerName = playerName,
                Lang = SystemConstant.DefaultLang,
                SessionToken = [REDACTED],
                BackUrl = ""
            };

            var result = MyJwtHelper.GeneratePlayerJwt(
                    playerID,
                    sessionToken,
                    param
                );

            return result.JwtToken;
        }


        private void MockHttpRequest()
        {
            var fakeJwt = MyJwtHelper.GenerateAdminUserJwt(_TEST_DATA.Default.UserID, _TEST_DATA.Default.UserName, _TEST_DATA.Default.OrganizationID);

            MockHttpHeaderJwtToken(fakeJwt.JwtToken);
        }


        /// <summary>
        /// 若判斷為true。請直接return跳過
        /// </summary>
        protected bool IsSkipTest_WhenSmallerThanTargetTime(DateTime targetTime)
        {
            //因為develop環境尚未合併部分程式碼。導致錯誤。透過指定日期暫時略過測試
            var now = DateTimeHelper.GetCurrentTime(8);

            //當前日期需大於指定日期後。才會執行原先應該運行的測試
            if (now < targetTime)
                return true;

            return false;
        }

        protected bool IsSkipTest_WhenLargeThanTargetDate(DateTime targetTime)
        {
            //因為develop環境尚未合併部分程式碼。導致錯誤。透過指定日期暫時略過測試
            var now = DateTimeHelper.GetCurrentTime(8);

            //當前日期需大於指定日期後。才會執行原先應該運行的測試
            if (now >= targetTime)
                return true;

            return false;
        }


        protected void MockHttpHeaderJwtToken(string jwtToken , string httpMethod = "GET")
        {
            var mockHeaders = new HeaderDictionary(new Dictionary<String, StringValues>
                    {
                        { SystemConstant.HeaderKey_Lang ,LanguageEnum.zh_cn.ToString()},
                        { SystemConstant.HeaderKey_AccessToken ,jwtToken},
                        { SystemConstant.HeaderKey_HDCode , SystemConstant.DefaultDeviceHDCode}

                    }) as IHeaderDictionary;


            MockHttpHeaders(mockHeaders, httpMethod);
        }


        protected void MockCommonHttpHeader(string httpMethod = "GET")
        {
            var mockHeaders = new HeaderDictionary(new Dictionary<String, StringValues>
                    {
                        { SystemConstant.HeaderKey_Lang ,LanguageEnum.zh_cn.ToString()},
                        { SystemConstant.HeaderKey_HDCode , SystemConstant.DefaultDeviceHDCode}

                    }) as IHeaderDictionary;


            MockHttpHeaders(mockHeaders, httpMethod);
        }

        protected void ResetMockRegister()
        {
            //清空其他測試注冊的mock
            IOCHelper.MockInstanceMapping = new Dictionary<string, object>();
        }



        private void MockHttpHeaders(IHeaderDictionary headers , string httpMethod = "GET")
        {
            var myHttpContext = Substitute.For<IHttpContextAccessor>();

            myHttpContext.HttpContext.Request.Method = httpMethod;
            myHttpContext.HttpContext.Request.Headers.Returns(headers);
            myHttpContext.HttpContext.Connection.RemoteIpAddress.Returns(new MockIPAddress(0));

            MyHttpContext.Configure(myHttpContext);
        }


        /// <summary>
        /// 如果某些 test 只有本機開發才執行的話，可加入此判斷
        /// </summary>
        protected bool IsNotDeveloperComputerName()
        {
            string currentComputerName = System.Environment.MachineName ?? "unknow";

            if (SystemConstant.RD_ComputerNames.Contains(currentComputerName) ||
                currentComputerName == "MSI")
            {
                return false;
            }

            return true;
        }


        protected T InstanceRepository<T>()
        {
            return (T)Activator.CreateInstance(typeof(T));
        }

        protected void AssertFail(string msg = "失敗! 與預期結果不符合")
        {
            Assert.Fail(msg);
        }
       
        protected void AssertExpectExceptionProcess(Action method , params ApiErrorCode[] expectedCode)
        {
            if(expectedCode.IsEmptyOrNull())
            {
                Assert.Fail("expectedCode 不允許傳空陣列");
            }

            try
            {
                method();
                AssertFail("預期應該拋錯誤。但是卻成功執行");
            }
            catch(Exception ex)
            {
                AssertExpectException(ex, expectedCode.ToList());
            }
        }

        protected void AssertExpectException(Exception ex, ApiErrorCode code)
        {
            AssertExpectException(ex , new List<ApiErrorCode> { code });
        }

        protected void AssertExpectException(Exception ex , List<ApiErrorCode> codes)
        {
            var terproEx = ex as TerproException;

            if(terproEx == null)
            {
                Assert.Fail($"非 TerproException ? {ex.Message} {ex.ToString()}");
            }

            bool isDefine = codes.Any(z => z == terproEx.ErrroCode);

            if(!isDefine)
            {
                Assert.Fail($"非預期的errorCodes , got code：{((int)terproEx.ErrroCode)}");
            }
        }

        protected void Assert_NotEmptyCollection<T>(IEnumerable<T> models)
        {
            if(models.IsEmptyOrNull())
            {
                Assert.Fail("models 為空集合，不合預期");
            }
        }


        private void InitializeIOC()
        {
            List<string> projectList = new List<string>
            {
                "target.BL"
            };

            IOCInitializeHelper.InitializeIOC(projectList);
            IOCHelper.SetMSTestMode();
        }


        protected void InstanceAllRepositoryImplement()
        {
            //如果service class內的全域變數
            //1. 定義為 Property (非Field)
            //2. 且命名包含 repository
            //3. 有繼承 IDapperRepository

            var allService = this.GetType()
                .GetProperties(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
                .Where(x => x.Name.ToLower().Contains("service"))
                .ToList();


            foreach (PropertyInfo item in allService)
            {
                if (item.PropertyType.IsInterface)
                {
                    var instanceValue = IOCHelper.Resolve(item.PropertyType);
                    item.SetValue(this, instanceValue);
                }
            }

            var allRep = this.GetType()
             .GetProperties(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
             .Where(x => x.Name.ToLower().Contains("repository") || x.Name.ToLower().Contains("rep"))
             .ToList();


            foreach (PropertyInfo item in allRep)
            {
                if (item.PropertyType.BaseType != null)
                {
                    if (item.PropertyType.BaseType.IsGenericType &&
                        (
                        item.PropertyType.BaseType.GetGenericTypeDefinition() == typeof(FormatIntegerPrimaryKeyRepository<>) ||
                        item.PropertyType.BaseType.GetGenericTypeDefinition() == typeof(FormatStringPrimaryKeyRepository<>) ||
                        item.PropertyType.BaseType.GetGenericTypeDefinition() == typeof(FormatMultiplePrimaryKeyRepository<>) ||
                        item.PropertyType.BaseType.GetGenericTypeDefinition() == typeof(FormatBetRecordRepository<>) ||
                        item.PropertyType.BaseType.GetGenericTypeDefinition() == typeof(FormatLongPrimaryKeyRepository<>)
                       ))
                    {
                        var instanceValue = Activator.CreateInstance(item.PropertyType);
                        item.SetValue(this, instanceValue);
                    }
                }
            }

        }

    }



    public class MockIPAddress : IPAddress
    {
        public MockIPAddress(long newAddress):base(newAddress)
        {

        }

        public override string ToString()
        {
            return  SystemConstant.CompanyIPAddress;
        }
    }

}