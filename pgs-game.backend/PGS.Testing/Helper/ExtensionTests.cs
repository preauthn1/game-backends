using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.Models.Models;
using target.Models.ViewModels.API.AdminSys;
using target.Utility.Extensions;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class ExtensionTests : BaseTest
    {



        [TestMethod]
        public void IsNumericTest()
        {
            Assert.IsTrue("20231107106".IsNumeric());
            Assert.IsTrue("2147483648".IsNumeric());
            Assert.IsTrue("2247483648".IsNumeric());
            Assert.IsTrue("1.23456".IsNumeric());
            Assert.IsTrue("1000".IsNumbericOfPositiveInt32());
            Assert.IsTrue("176053".IsNumbericOfPositiveInt32());
            Assert.IsTrue("1047483648".IsNumbericOfPositiveInt32());

            Assert.IsFalse("AAAAA".IsNumeric());
            Assert.IsFalse("2247483648".IsNumbericOfPositiveInt32());
            Assert.IsFalse("2247483648".IsNumbericOfPositiveInt32());
            Assert.IsFalse("1.22222".IsNumbericOfPositiveInt32());
            Assert.IsFalse("1.00".IsNumbericOfPositiveInt32());
            Assert.IsFalse("-10".IsNumbericOfPositiveInt32());

            return;
        }


        [TestMethod]
        public void FloorTwoPlaceDecimal()
        {
            Assert.IsTrue((3.26M).FloorTwoPlaceDecimal() == 3.26M);
            Assert.IsTrue((6.01M).FloorTwoPlaceDecimal() == 6.01M);
            Assert.IsTrue((6.0133M).FloorTwoPlaceDecimal() == 6.01M);
            Assert.IsTrue((6.0199M).FloorTwoPlaceDecimal() == 6.01M);
            Assert.IsTrue((6.9999M).FloorTwoPlaceDecimal() == 6.99M);
            Assert.IsTrue((6.9M).FloorTwoPlaceDecimal() == 6.9M);
            Assert.IsTrue((6.37M).FloorTwoPlaceDecimal() == 6.37M);
            Assert.IsTrue((1000M).FloorTwoPlaceDecimal() == 1000M);
            return;
        }


        [TestMethod]
        public void UnixTimeStampToDateTime_Test()
        {
            var qqq1 = ExtensionOfDatetime.UnixTimeStampToDateTime(1697685573504);
            var qqq2 = ExtensionOfDatetime.UnixTimeStampToDateTime(1697685573);

            Assert.IsTrue(qqq1.Date == new DateTime(2023, 10, 19));
            Assert.IsTrue(qqq2.Date == new DateTime(2023, 10, 19));
            return;
        }


        [TestMethod]
        public void MaskPrefixText_Test()
        {
            string text = "abcd1234";

            var qqqq01 = text.DisplayPrefixAndMaskSuffixText(4);
            var qqqq02 = text.DisplayPrefixAndMaskSuffixText(6);

            var qqqq11 = text.MaskPrefixAndDisplaySuffixText(4);
            var qqqq12 = text.MaskPrefixAndDisplaySuffixText(6);

            var qqqq31 = "176252".MaskPrefixAndDisplaySuffixText(3);

            Assert.IsTrue(qqqq01.Replace("*", "") == "abcd");
            Assert.IsTrue(qqqq02.Replace("*", "") == "abcd12");

            Assert.IsTrue(qqqq11.Replace("*", "") == "1234");
            Assert.IsTrue(qqqq12.Replace("*", "") == "cd1234");

            Assert.IsTrue(qqqq31.Replace("*", "") == "252");
            return;
        }



        [TestMethod]
        public void SplitListByArraySize_Test()
        {
            List<int> myList = new List<int>
            {
                1,2,3,4,5,6,7,8,9,10
            };

            var qqq1 = myList.SplitListByArraySize(2);
            var qqq2 = myList.SplitListByArraySize(3);
            var qqq3 = myList.SplitListByArraySize(5);

            Assert.IsTrue(qqq1.Count() == 5);
            Assert.IsTrue(qqq2.Count() == 4);
            Assert.IsTrue(qqq3.Count() == 2);

            Assert.IsTrue(qqq1.SelectMany(z=> z).Count() == 10);
            Assert.IsTrue(qqq2.SelectMany(z => z).Count() == 10);
            Assert.IsTrue(qqq3.SelectMany(z => z).Count() == 10);
            return;
        }


        [TestMethod]
        public void GetErrorOccurLineFromStackTrace_Test()
        {
            var parseValue = ExtensionOfException.GetErrorOccurLineFromStackTrace("at System.Nullable`1.get_Value()  at target.BL.Services.AGGIntegrationService.GetExternalBetDetailAndScreenshot(ExternalVendorBetRecordDetailInfoRequestModel model) in /builds/backend/target/game.backend/target.BL/Services/GameVendorAPI/AGGIntegrationService.cs:line 628    at target.BL.Services.ThirdPartyGameVendorService.GetReplayInfoByGameVendorApi(Int32 userID, Int32 gameVendorType, String betRecordNoPGS) in /builds/backend/target/game.backend/target.BL/Services/API.AdminSys/ThirdPartyGameVendorService.cs:line 196    at target.BL.Services.ThirdPartyGameVendorService.GetDetailUrlByTransactionNo(Int32 userID, String pgsTransactionNo) in /builds/backend/target/game.backend/target.BL/Services/API.AdminSys/ThirdPartyGameVendorService.cs:line 86    at target.API.Vendor.Controllers.BetRecordsController.GenerateScreenshotDetailInfoLink(BetRecordDetailInfoLinkGenerateRequestModel model) in /builds/backend/target/game.backend/target.API.Vendor/Controllers/BetRecordsController.cs:line 91    at lambda_method(Closure , Object , Object[] )    at Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor.SyncObjectResultExecutor.Execute(IActionResultTypeMapper mapper, ObjectMethodExecutor executor, Object controller, Object[] arguments)    at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.InvokeActionMethodAsync()    at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)    at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.InvokeNextActionFilterAsync() --- End of stack trace from previous location where exception was thrown ---    at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Rethrow(ActionExecutedContextSealed context)    at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)    at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.InvokeInnerFilterAsync() --- End of stack trace from previous location where exception was thrown ---    at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeNextResourceFilter>g__Awaited|24_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)    at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Rethrow(ResourceExecutedContextSealed context)    at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)    at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.InvokeFilterPipelineAsync() --- End of stack trace from previous location where exception was thrown ---    at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)    at Microsoft.AspNetCore.Routing.EndpointMiddleware.<Invoke>g__AwaitRequestTask|6_0(Endpoint endpoint, Task requestTask, ILogger logger)    at Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddleware.<Invoke>g__Awaited|6_0(ExceptionHandlerMiddleware middleware, HttpContext context, Task task)");
            return;
        }



        [TestMethod]
        public void ReplaceNoneAccountFormatText_Tests()
        {
            string nullText = null;

            Assert.AreEqual("", nullText.ReplaceNoneAccountFormatText());
            Assert.AreEqual("", "   ".ReplaceNoneAccountFormatText());
            Assert.AreEqual("Player001", "  Player001  ".ReplaceNoneAccountFormatText());
            Assert.AreEqual("Player001", "  游戏账号：Player001  ".ReplaceNoneAccountFormatText());
            return;
        }


        [TestMethod]
        public void ReplaceLastWordToNewText_Test()
        {
            string text = "123456789";

            var result_1 = text.ReplaceLastWordToNewText(5, "*****");
            var result_2 = text.ReplaceLastWordToNewText(100, "*****");

            Assert.IsTrue(result_1 == "1234*****");
            Assert.IsTrue(result_2 == text); //字數超過則返回原字
            return;
        }


        [TestMethod]
        public void SplitListBySeperateSize_Test()
        {
            List<int> myList = new List<int>
            {
                1,2,3,4,5,6,7,8,9,10
            };

            var qqq1 = myList.SplitListBySeperateSize(2);
            var qqq2 = myList.SplitListBySeperateSize(3);
            var qqq3 = myList.SplitListBySeperateSize(5);

            Assert.IsTrue(qqq1.Count() == 2);
            Assert.IsTrue(qqq2.Count() == 3);
            Assert.IsTrue(qqq3.Count() == 5);

            Assert.IsTrue(qqq1.SelectMany(z => z).Count() == 10);
            Assert.IsTrue(qqq2.SelectMany(z => z).Count() == 10);
            Assert.IsTrue(qqq3.SelectMany(z => z).Count() == 10);
            return;
        }



        [TestMethod]
        public void IsDictionary_Test()
        {

            Dictionary<string, object> dic1 = new Dictionary<string, object>();
            Dictionary<string, string> dic2 = new Dictionary<string, string>();
            Dictionary<string, int> dic3 = new Dictionary<string, int>();

            Assert.IsTrue(dic1.IsDictionary());
            Assert.IsTrue(dic2.IsDictionary());
            Assert.IsTrue(dic3.IsDictionary());


            Assert.IsFalse((new { qqq = "" }).IsDictionary());
            Assert.IsFalse((new DropdownValueViewModel()).IsDictionary());

            return;
        }


        [TestMethod]
        public void ReplaceDuplicateUrlChar_Test()
        {
            string correctUrl = "https://example.invalid/lobby/?page=speedTest";

            string url1 = "https://example.invalid/lobby/?page=speedTest".ReplaceDuplicateUrlChar();
            string url2 = "https://example.invalid/lobby//?page=speedTest".ReplaceDuplicateUrlChar();
            string url3 = "https://example.invalid/lobby///?page=speedTest".ReplaceDuplicateUrlChar();
            string url4 = "http://example.invalid/lobby///?page=speedTest".ReplaceDuplicateUrlChar();
            string url5 = "http://example.invalid/lobby/?page=speedTest".ReplaceDuplicateUrlChar();

            Assert.IsTrue(url1 == correctUrl);
            Assert.IsTrue(url2 == correctUrl);
            Assert.IsTrue(url3 == correctUrl);
            Assert.IsTrue(url4 == correctUrl.Replace("https", "http"));
            Assert.IsTrue(url5 == correctUrl.Replace("https", "http"));

            //string url_201 = "https://example.invalid/lobby/?page=speedTest".ReplaceDuplicateUrlSymbolV2();
            //string url_202 = "https://example.invalid/lobby//?page=speedTest".ReplaceDuplicateUrlSymbolV2();
            //string url_203 = "https://example.invalid/lobby///?page=speedTest".ReplaceDuplicateUrlSymbolV2();
            //string url_204 = "http://example.invalid/lobby///?page=speedTest".ReplaceDuplicateUrlChar();
            //string url_205 = "http://example.invalid/lobby/?page=speedTest".ReplaceDuplicateUrlChar();

            //Assert.IsTrue(url_201 == correctUrl);
            //Assert.IsTrue(url_202 == correctUrl);
            //Assert.IsTrue(url_203 == correctUrl);
            //Assert.IsTrue(url_204 == correctUrl.Replace("https", "http"));
            //Assert.IsTrue(url_205 == correctUrl.Replace("https", "http"));

            return;
        }



        [TestMethod]
        public void SplitToDateListByHourRange_Test()
        {
            DateTime beginTime = new DateTime(2022, 1, 1);

            int hoursRange = 24;
            var result1 = beginTime.SplitToDateListByHourRange(new DateTime(2022, 1, 5), hoursRange);
            var result2 = beginTime.SplitToDateListByHourRange(new DateTime(2022, 1, 2), 4);
            var result3 = beginTime.SplitToDateListByHourRange(new DateTime(2022, 1, 2), 3);

            Assert.IsTrue(result1.Count() == 4);
            Assert.IsTrue((result1[1] - result1[0]).TotalHours == hoursRange);
            Assert.IsTrue(result2.Count() == 6);
            Assert.IsTrue(result3.Count() == 8);
            return;
        }



        [TestMethod]
        public void CutLastCharIfMatch_Test()
        {
            string test1 = "abcd_/";
            string test2 = "abcd_@";
            string test3 = null;

            var rrrr1 = test1.CutLastCharIfMatch('/');
            var rrrr2 = test2.CutLastCharIfMatch('/');
            var rrrr3 = test3.CutLastCharIfMatch('/');


            Assert.IsTrue((test1.Length - rrrr1.Length) == 1);
            Assert.IsTrue(test2 == rrrr2);
            Assert.IsTrue(test3 == rrrr3);

            return;
        }


        [TestMethod]
        public void MD5Hash_Test()
        {
            string beforeEncrypt = "qwe123!@#5566[]{ }";

            string default8Encode = beforeEncrypt.MD5Hash();
            string uff8Encode = beforeEncrypt.MD5Hash();
            string asciiEncode = beforeEncrypt.MD5Hash(System.Text.Encoding.ASCII);

            //改過code保險起見測試一下
            Assert.IsTrue(uff8Encode == asciiEncode && uff8Encode == default8Encode);
            return;
        }



        [TestMethod]
        public void GetInnerTextBetweenTwoWord_Test()
        {
            string text = "hello (this is my)";

            var rrrr1 = text.GetInnerTextBetweenTwoWord("(", ")");
            var rrrr2 = text.GetInnerTextBetweenTwoWord("[", "]");

            return;
        }


        [TestMethod]
        public void DateTimeTest()
        {
            var date = DateTimeHelper.GetCurrentTime();

            var r1 = date.ToISO8601();
            return;
        }

        [TestMethod]
        public void MerchantID_Rule_NotZeroNotMinus_Test()
        {
            var model1 = new { MerchantID = 0 };
            var model2 = new { merchantid = 0 };
            var model3 = new { UserID = 0 };

            var errorCodes = new List<ApiErrorCode>
            {
                ApiErrorCode.InvalidParameter_PleaseSelectMerchantID,
                ApiErrorCode.InvalidParameter_NumberEmptyZero,
            };

            //要符合預期的前提是需要Mock出 httpGET的環境
            AssertExpectExceptionProcess(() =>
            {
                model1.RuleFor(x => x.MerchantID).Rule_NotZeroNotMinus();

            }, errorCodes.ToArray());

            AssertExpectExceptionProcess(() =>
            {
                model2.RuleFor(x => x.merchantid).Rule_NotZeroNotMinus();

            }, errorCodes.ToArray());

            AssertExpectExceptionProcess(() =>
            {
                model3.RuleFor(x => x.UserID).Rule_NotZeroNotMinus();

            }, ApiErrorCode.InvalidParameter_NumberEmptyZero);

            return;
        }


        [TestMethod]
        public void CheckBeginEndTimeConflict_Test()
        {
            var soureTime = new
            {
                Begin = new DateTime(2021, 2, 1),
                End = new DateTime(2021, 3, 1)
            };

            DateTime time_0101 = new DateTime(2021, 1, 1);
            DateTime time_0130 = new DateTime(2021, 1, 30);
            DateTime time_0210 = new DateTime(2021, 2, 10);
            DateTime time_0215 = new DateTime(2021, 2, 15);
            DateTime time_0315 = new DateTime(2021, 3, 15);
            DateTime time_0330 = new DateTime(2021, 3, 30);

            Assert.IsTrue(ExtensionOfDatetime.CheckBeginEndTimeConflict(soureTime.Begin, soureTime.End, time_0101, time_0215));
            Assert.IsTrue(ExtensionOfDatetime.CheckBeginEndTimeConflict(soureTime.Begin, soureTime.End, time_0215, time_0315));
            Assert.IsTrue(ExtensionOfDatetime.CheckBeginEndTimeConflict(soureTime.Begin, soureTime.End, time_0101, time_0315));
            Assert.IsTrue(ExtensionOfDatetime.CheckBeginEndTimeConflict(soureTime.Begin, soureTime.End, time_0210, time_0215));


            Assert.IsFalse(ExtensionOfDatetime.CheckBeginEndTimeConflict(soureTime.Begin, soureTime.End, time_0101, time_0130));
            Assert.IsFalse(ExtensionOfDatetime.CheckBeginEndTimeConflict(soureTime.Begin, soureTime.End, time_0315, time_0330));

            return;
        }



        [TestMethod]
        public void ObjectToQueryString_Test()
        {
            var body = new 
            { 
                gameId = "AA",
                playerId = "BB",
                sessionId = "CC",
            };

            var result = ExtensionOfHttpRequestBase.GenerateQueryString(body);

            Assert.IsTrue(result.Contains("gameId"));
            Assert.IsTrue(result.Contains("playerId"));
            Assert.IsTrue(result.Contains("&"));
            Assert.IsTrue(!result.Contains("?")); //請寫成不要自動包含?
            return;
        }






        [TestMethod]
        public void Exception_Test()
        {
            var ex1 = new TerproException(ApiErrorCode.NotFoundData);
            var ex2 = new TerproException(ApiErrorCode.NotFoundEvent, new ErrorDetailModel("XXXXX"));
            var ex3 = new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataNotCompleted);

            var test1 = ex1.Message;
            var test2 = ex1.ToString();
            var test3 = ex1.GetMessageAndErrorLine();
            return;
        }


    }
}
