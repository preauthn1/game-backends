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
    public class RegexRuleHelperTests : BaseTest
    {

        [TestMethod]
        public void CheckIsAccountForamt()
        {
            //已確認要pass的案例
            //2024/06 有部分客戶有@符號的帳號
            Assert.IsTrue(IsAccountFormat("12345"));
            Assert.IsTrue(IsAccountFormat("abcd"));
            Assert.IsTrue(IsAccountFormat("AAAAAAAAABBBBBBBBB"));
            Assert.IsTrue(IsAccountFormat("user@example.invalid"));
            Assert.IsTrue(IsAccountFormat("user@example.invalid"));
            Assert.IsTrue(IsAccountFormat("user@example.invalid"));
            Assert.IsTrue(IsAccountFormat("user@example.invalid"));
            Assert.IsTrue(IsAccountFormat("jack#3365"));


            //已確認要fail的案例
            Assert.IsFalse(IsAccountFormat("aaaaaaa!!!"));
            Assert.IsFalse(IsAccountFormat("hello hi"));
            Assert.IsFalse(IsAccountFormat("qqqqq*"));
            Assert.IsFalse(IsAccountFormat("qqqqq\t"));
            Assert.IsFalse(IsAccountFormat("qqqqq/t"));
            Assert.IsFalse(IsAccountFormat("user@example.invalid"));
            Assert.IsFalse(IsAccountFormat("1"));
            Assert.IsFalse(IsAccountFormat("にっぽんんごんごご"));
            Assert.IsFalse(IsAccountFormat("吃吃飯吃飯飯"));
            Assert.IsFalse(IsAccountFormat("~~~~~~!!!"));
            Assert.IsFalse(IsAccountFormat("abc999 "));

            var match13 = IsAccountFormat("123abc?&*@@");
            var match63 = IsAccountFormat("hello-world");
            var match68 = IsAccountFormat("???????$#########");
            var match79 = IsAccountFormat("");
            return;
        }




        [TestMethod]
        public void Check_IsOrderNoFormat()
        {
            //已確認要pass的案例
            Assert.IsTrue(IsOrderNumberFormat("12345"));
            Assert.IsTrue(IsOrderNumberFormat("abcdQQQQ"));
            Assert.IsTrue(IsOrderNumberFormat("AAAAAAAAABBBBBBBBB"));
            Assert.IsTrue(IsOrderNumberFormat("AAAAAAAA88888888"));
            Assert.IsTrue(IsOrderNumberFormat("AAA_BBB_CCC"));
            Assert.IsTrue(IsOrderNumberFormat("AAA-BBB-CCC"));
            Assert.IsTrue(IsOrderNumberFormat("GD_63801365719_pgs"));
            Assert.IsTrue(IsOrderNumberFormat("Wxg0536711-AMNR4"));
            Assert.IsTrue(IsOrderNumberFormat("107378b8-932c-4b96-bd84-fa502c24e37d"));
            Assert.IsTrue(IsOrderNumberFormat(Guid.NewGuid().ToString()));

            //已確認要fail的案例
            Assert.IsFalse(IsOrderNumberFormat("YYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY"));
            Assert.IsFalse(IsOrderNumberFormat("1"));
            Assert.IsFalse(IsOrderNumberFormat("user@example.invalid"));
            Assert.IsFalse(IsOrderNumberFormat("AAAAAAAAA**"));
            Assert.IsFalse(IsOrderNumberFormat("AAAA+BBBBB"));
            Assert.IsFalse(IsOrderNumberFormat("2022/01/01"));
            Assert.IsFalse(IsOrderNumberFormat("123.123.123"));
            Assert.IsFalse(IsOrderNumberFormat("ABC_132_/*Q"));
            Assert.IsFalse(IsOrderNumberFormat("AAAAAA BBBBBB")); //包含空白
            Assert.IsFalse(IsOrderNumberFormat("AAAAAABBBBBB ")); //包含空白
            Assert.IsFalse(IsOrderNumberFormat(" AAAAAABBBBBB")); //包含空白
            Assert.IsFalse(IsOrderNumberFormat("ABC_132_2022:10:01"));
            Assert.IsFalse(IsOrderNumberFormat("ABC_########55555AAAA"));
            Assert.IsFalse(IsOrderNumberFormat("にっぽんんごんごご"));
            Assert.IsFalse(IsOrderNumberFormat("吃吃飯吃飯飯"));
            Assert.IsFalse(IsOrderNumberFormat("~~~~~~!!!"));

            return;
        }



        [TestMethod]
        public void Check_IsPasswordFormat()
        {
            Assert.IsTrue(RegexRuleHelper.IsPasswordFormat("abc123"));
            Assert.IsTrue(RegexRuleHelper.IsPasswordFormat("QQQQQBBBBBccc"));
            Assert.IsFalse(RegexRuleHelper.IsPasswordFormat("QQQQQBBBBBccc "));
            Assert.IsFalse(RegexRuleHelper.IsPasswordFormat("A "));
            return;
        }


        [TestMethod]
        public void IsDecimalPointOverThanPlacesTwo()
        {
            Assert.IsFalse(RegexRuleHelper.IsOverThanDecimalPlaces(0.1M));
            Assert.IsFalse(RegexRuleHelper.IsOverThanDecimalPlaces(0.1M, 4));
            Assert.IsFalse(RegexRuleHelper.IsOverThanDecimalPlaces(1.200M));
            Assert.IsFalse(RegexRuleHelper.IsOverThanDecimalPlaces(1.250M));
            Assert.IsFalse(RegexRuleHelper.IsOverThanDecimalPlaces(1.1235M, 4));
            Assert.IsFalse(RegexRuleHelper.IsOverThanDecimalPlaces(1.123500M, 4));
            Assert.IsFalse(RegexRuleHelper.IsOverThanDecimalPlaces(1.875M, 4));
            Assert.IsFalse(RegexRuleHelper.IsOverThanDecimalPlaces(0.001M, 4));


            Assert.IsTrue(RegexRuleHelper.IsOverThanDecimalPlaces(1.875M));
            Assert.IsTrue(RegexRuleHelper.IsOverThanDecimalPlaces(0.001M));
            Assert.IsTrue(RegexRuleHelper.IsOverThanDecimalPlaces(0.000000000001M, 4));
            Assert.IsTrue(RegexRuleHelper.IsOverThanDecimalPlaces(0.00000001M, 4));
            Assert.IsTrue(RegexRuleHelper.IsOverThanDecimalPlaces(0.00123456M, 4));
            return;
        }


        [TestMethod]
        public void Check_IsIPAddressFormat()
        {
            Assert.IsTrue(RegexRuleHelper.IsIPAddressFormat("0.0.0.0"));
            Assert.IsTrue(RegexRuleHelper.IsIPAddressFormat("0.0.0.0"));
            Assert.IsTrue(RegexRuleHelper.IsIPAddressFormat("0.0.0.0"));
            Assert.IsTrue(RegexRuleHelper.IsIPAddressFormat("0.0.0.0"));
            Assert.IsTrue(RegexRuleHelper.IsIPAddressFormat("2001:db8:3333:4444:CCCC:DDDD:EEEE:FFFF"));
            Assert.IsTrue(RegexRuleHelper.IsIPAddressFormat("2406:da19:8e9:f31f:43c5:ef98:850b:e83a"));

            Assert.IsFalse(RegexRuleHelper.IsIPAddressFormat("AAAAAAAAAA"));
            Assert.IsFalse(RegexRuleHelper.IsIPAddressFormat("0.0.0.0"));
            Assert.IsFalse(RegexRuleHelper.IsIPAddressFormat("192.168"));
            Assert.IsFalse(RegexRuleHelper.IsIPAddressFormat("你好"));
            Assert.IsFalse(RegexRuleHelper.IsIPAddressFormat(" "));
            return;
        }


        [TestMethod]
        public void Check_IsMultipleIPAddressFormat()
        {
            string new_line = Environment.NewLine;

            Assert.IsTrue(RegexRuleHelper.IsMultipleIPAddressFormat("0.0.0.0 0.0.0.0   0.0.0.0 0.0.0.0"));
            Assert.IsTrue(RegexRuleHelper.IsMultipleIPAddressFormat("0.0.0.0,0.0.0.0,0.0.0.0,0.0.0.0"));
            Assert.IsTrue(RegexRuleHelper.IsMultipleIPAddressFormat($"0.0.0.0{new_line}0.0.0.0{new_line}0.0.0.0{new_line}0.0.0.0"));
            Assert.IsTrue(RegexRuleHelper.IsMultipleIPAddressFormat("0.0.0.0"));

            //全形符號
            Assert.IsTrue(RegexRuleHelper.IsMultipleIPAddressFormat("0.0.0.0　0.0.0.0　0.0.0.0　0.0.0.0"));

            Assert.IsFalse(RegexRuleHelper.IsMultipleIPAddressFormat("192.168"));
            Assert.IsFalse(RegexRuleHelper.IsMultipleIPAddressFormat("0.0.0.0  123"));
            Assert.IsFalse(RegexRuleHelper.IsMultipleIPAddressFormat("0.0.0.0.123"));
            Assert.IsFalse(RegexRuleHelper.IsMultipleIPAddressFormat("0.0.0.0.250.3   4,0.0.0.0  0.0.0.0"));

            return;
        }




        [TestMethod]
        public void CheckIsIntegerValue()
        {
            //已確認要pass的案例
            Assert.IsTrue((1.0M).IsIntegerValue());
            Assert.IsTrue((3500.0M).IsIntegerValue());
            Assert.IsTrue((173511.0M).IsIntegerValue());

            Assert.IsFalse((7.00008M).IsIntegerValue());
            Assert.IsFalse((3.60600M).IsIntegerValue());
            return;
        }


        private bool IsAccountFormat(string myValue)
        {
            return RegexRuleHelper.IsAccountFormat(myValue);
        }


        private bool IsOrderNumberFormat(string myValue)
        {
            return RegexRuleHelper.IsOrderNumberFormat(myValue);
        }

    }
}
