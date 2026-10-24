using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Services;
using target.Models.Enums;
using target.Models.Models;
using target.Utility.Extensions;
using target.Utility.Helper;
using System;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class RegexRuleHelperRuleForTests : BaseTest
    {


        [TestMethod]
        public void RuleFor_IsValidMerchantID()
        {
            (new { MerchantID = 175001 }).RuleFor(x => x.MerchantID).Rule_IsValidMerchantID();
            (new { MerchantID = 185001 }).RuleFor(x => x.MerchantID).Rule_IsValidMerchantID();
            (new { MerchantID = 195099 }).RuleFor(x => x.MerchantID).Rule_IsValidMerchantID();
            (new { MerchantID = 375001 }).RuleFor(x => x.MerchantID).Rule_IsValidMerchantID();


            AssertExpectExceptionProcess(() =>
            {
                (new { MerchantID = 1000 }).RuleFor(x => x.MerchantID).Rule_IsValidMerchantID();

            }, ApiErrorCode.InvalidParameter_IsNotValidMerchantIDFormat);

            AssertExpectExceptionProcess(() =>
            {
                (new { MerchantID = -1 }).RuleFor(x => x.MerchantID).Rule_IsValidMerchantID();

            }, ApiErrorCode.InvalidParameter_IsNotValidMerchantIDFormat);

            AssertExpectExceptionProcess(() =>
            {
                (new { MerchantID = 3750019 }).RuleFor(x => x.MerchantID).Rule_IsValidMerchantID();

            }, ApiErrorCode.InvalidParameter_IsNotValidMerchantIDFormat);


            return;

        }


        [TestMethod]
        public void RuleFor_DecimalTwoPlaces()
        {
            RuleIsDecimalPointPlacesTwo(1);
            RuleIsDecimalPointPlacesTwo(5000);
            RuleIsDecimalPointPlacesTwo(5000.22M);
            RuleIsDecimalPointPlacesTwo(1.2M);
            RuleIsDecimalPointPlacesTwo(1.22M);

            AssertExceptionOccur(1.233M);
            AssertExceptionOccur(1.234567M);
            AssertExceptionOccur(5000.123M);
            return;

        }



        [TestMethod]
        public void RuleFor_IsMatchSqlInjectionKeyword()
        {
            Assert.IsTrue(RegexRuleHelper.IsMatchRequestBodySqlInjectionKeyword("where Id = 100"));
            Assert.IsFalse(RegexRuleHelper.IsMatchRequestBodySqlInjectionKeyword("where Id = 100" , isHighLevelCheck: false ));

            Assert.IsFalse(RegexRuleHelper.IsMatchRequestBodySqlInjectionKeyword("test001"));
            Assert.IsFalse(RegexRuleHelper.IsMatchRequestBodySqlInjectionKeyword("安安你好"));
            Assert.IsFalse(RegexRuleHelper.IsMatchRequestBodySqlInjectionKeyword("this is pen"));
            Assert.IsFalse(RegexRuleHelper.IsMatchRequestBodySqlInjectionKeyword("https://example.invalid"));
            Assert.IsFalse(RegexRuleHelper.IsMatchRequestBodySqlInjectionKeyword("{ 'memberID': 'qqqq' , 'name': 'PGS5566'}"));

            Assert.IsTrue(RegexRuleHelper.IsMatchRequestBodySqlInjectionKeyword("lastLoginTimeEnd='/**/and(select'1'from/**/pg_sleep(2))::text>'0&pageIndex=1&pageSize=10&playerName=123&playerNameFuzzyQuery=123"));
            Assert.IsTrue(RegexRuleHelper.IsMatchRequestBodySqlInjectionKeyword("VIP005' AND (SELECT 2291 FROM (SELECT(SLEEP(5)))AKOr) AND 'uxGD'='uxGD"));
            Assert.IsTrue(RegexRuleHelper.IsMatchRequestBodySqlInjectionKeyword("VIP005;SELECT DBMS_PIPE.RECEIVE_MESSAGE(CHR(73)||CHR(108)||CHR(109)||CHR(74),5) FROM DUAL--"));
            return;
        }



        [TestMethod]
        public void RuleFor_IsUrlFormat()
        {
            Assert.IsTrue(RegexRuleHelper.IsUrlFormat("https%3A%2F%example.invalid%2F"));
            Assert.IsTrue(RegexRuleHelper.IsUrlFormat("https://example.invalid/"));

            Assert.IsFalse(RegexRuleHelper.IsUrlFormat("https-123-123"));
            Assert.IsFalse(RegexRuleHelper.IsUrlFormat("example.invalid"));
            return;
        }



        private void RuleIsDecimalPointPlacesTwo(decimal value)
        {
            var model = new
            {
                Amount = value
            };

            model.RuleFor(z => z.Amount).Rule_IsDecimalPointPlacesTwo();
        }

        private void AssertExceptionOccur(decimal value)
        {
            try
            {
                var model = new
                {
                    Amount = value
                };

                model.RuleFor(z => z.Amount).Rule_IsDecimalPointPlacesTwo();
            }
            catch (TerproException ex)
            {
                Assert.IsTrue(ex.ErrroCode == ApiErrorCode.InvalidParameter_NumberValueMustPlacesTwo);
                return;
            }

            Assert.Fail("unexpect !!");
        }

    }
}
