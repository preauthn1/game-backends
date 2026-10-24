using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.Models.Enums;
using target.Models.Models;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Utility.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class RandomHelperTests : BaseTest
    {

        [TestMethod]
        public void RollDiceByRate_Test()
        {
            // 3 = 3%機率
            decimal rate = 3;
            bool isSuccess = RollDice(rate);

            if(!isSuccess)
            {
                //如果失敗嘗試roll第二次
                isSuccess = RollDice(rate); ;
                Assert.IsTrue(isSuccess);
            }

            // 0.05 = 0.05% = 2000分之一 機率
            decimal rate2 = 0.05M;
            bool isSuccess2 = RollDice(rate2);

            if (!isSuccess2)
            {
                //如果失敗嘗試roll第二次
                isSuccess2 = RollDice(rate2); ;
                Assert.IsTrue(isSuccess2);
            }

            return;
        }


        private static bool RollDice(decimal successRate)
        {
            int successTimes = 0;

            int maxTimes = 500000;

            for (int i = 0; i < maxTimes; i++)
            {
                var result = RandomHelper.RollDiceByRate(successRate);

                if (result.isSuccess)
                {
                    successTimes++;
                }
            }

            decimal realSuccessRate = ((decimal)successTimes / (decimal)maxTimes);

            decimal realSuccessRate_100X = realSuccessRate * 100;

            //允許在 預期機率的5%範圍內   
            bool isExpectedSuccessRate = successRate * 0.85M < realSuccessRate_100X && realSuccessRate_100X < successRate * 1.15M;
            return isExpectedSuccessRate;
        }


        [TestMethod]
        public void GetRandIntegerNumbers_Test()
        {
            List<int> values = new List<int>();

            for (int i = 0; i < 10000; i++)
            {
                var value = RandomHelper.GetRandIntegerNumbers(1, 10);
                values.Add(value);
            }

            var qqq = values.Distinct().ToList();
            return;
        }



        [TestMethod]
        public void IsPgsUniqueOrderNoPasswordFormat_Test()
        {
            Assert.IsFalse(RandomHelper.IsPgsUniqueOrderNoPasswordFormat("AAAAAAAAA"));
            Assert.IsFalse(RandomHelper.IsPgsUniqueOrderNoPasswordFormat("30bc125d-1346-4025-8611-b665a134d501"));
            Assert.IsFalse(RandomHelper.IsPgsUniqueOrderNoPasswordFormat("30bc125d134640258611b665a134d501"));
            Assert.IsFalse(RandomHelper.IsPgsUniqueOrderNoPasswordFormat("691F40cfbfa0f40d4da585dbc7c03f51"));

            string currentTime_tranNo = RandomHelper.GetPgsUniqueDateHourSequenceOrderNo();
            string correctForamt_tranNo = currentTime_tranNo.Truncate(26) +  "e04su3";

            Assert.IsFalse(RandomHelper.IsPgsUniqueOrderNoPasswordFormat(currentTime_tranNo));
            Assert.IsTrue(RandomHelper.IsPgsUniqueOrderNoPasswordFormat(correctForamt_tranNo));
            return;
        }


        [TestMethod]
        public void To36FormatDateSequenceOrderNo_Test()
        {
            List<int> values = new List<int>();

            int currentYear = DateTimeHelper.GetCurrentTime().Year;
            int nextYear = currentYear + 1;

            var q1 = RandomHelper.To36FormatDateSequenceOrderNo(new DateTime(2055, 12, 31, 23, 59, 59));
            var q2 = RandomHelper.To36FormatDateSequenceOrderNo(new DateTime(2021, 12, 31, 17, 0, 59));
            var q3 = RandomHelper.To36FormatDateSequenceOrderNo(new DateTime(2024, 12, 31, 4, 59, 59));
            var q4 = RandomHelper.To36FormatDateSequenceOrderNo(new DateTime(2021, 5, 12, 5, 0, 59));
            var q5 = RandomHelper.To36FormatDateSequenceOrderNo(new DateTime(2024, 6, 2, 6, 0, 59));
            var q6 = RandomHelper.To36FormatDateSequenceOrderNo(new DateTime(2024, 2, 29, 23, 5, 59));
            var q7 = RandomHelper.To36FormatDateSequenceOrderNo(new DateTime(2023, 2, 15, 14, 0, 59));
            var q8 = RandomHelper.To36FormatDateSequenceOrderNo(new DateTime(2023, 1, 1, 16, 3, 59));

            var q91 = RandomHelper.To36FormatDateSequenceOrderNo(new DateTime(currentYear, 1, 1, 18, 3, 59));
            var q92 = RandomHelper.To36FormatDateSequenceOrderNo(new DateTime(nextYear, 1, 1, 18, 3, 59));
            var q93 = RandomHelper.To36FormatDateSequenceOrderNo(new DateTime(nextYear, 1, 1, 0, 0, 0));

            var value_2021 = RandomHelper.To36FormatDateSequenceOrderNo(new DateTime(2021, 10, 31, 17, 0, 59));
            var value_2030 = RandomHelper.To36FormatDateSequenceOrderNo(new DateTime(2030, 10, 31, 17, 0, 59));

            Assert.IsTrue(value_2021.Length == 32);
            Assert.IsTrue(value_2021.StartsWith('1'));
            Assert.IsTrue(value_2030.StartsWith('A'));

            Assert.IsTrue(RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(q1).IsPgsOrderNo);
            Assert.IsTrue(RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(q2).IsPgsOrderNo);
            Assert.IsTrue(RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(q3).IsPgsOrderNo);
            Assert.IsTrue(RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(q4).IsPgsOrderNo);
            Assert.IsTrue(RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(q5).IsPgsOrderNo);
            Assert.IsTrue(RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(q6).IsPgsOrderNo);
            Assert.IsTrue(RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(q7).IsPgsOrderNo);
            Assert.IsTrue(RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(q8).IsPgsOrderNo);
            Assert.IsTrue(RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(q91).IsPgsOrderNo);
            Assert.IsTrue(RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(q92).IsPgsOrderNo);
            Assert.IsTrue(RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(q93).IsPgsOrderNo);

            Assert.IsFalse(RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat("1SP6af1d48e1e1aa4c8db0f1a45ce998").IsPgsOrderNo);
            Assert.IsFalse(RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat("18Z6af1d48e1e1aa4c8db0f1a45ce998").IsPgsOrderNo);
            Assert.IsFalse(RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat("1Z1Zaf1d48e1e1aa4c8db0f1a45ce998").IsPgsOrderNo);
            Assert.IsFalse(RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat("18ZZaf1d48e1e1aa4c8db0f1a45ce998").IsPgsOrderNo);

            //確保1~31都不會出問題
            for(int day = 1; day < 31; day++)
            {
                RandomHelper.To36FormatDateSequenceOrderNo(new DateTime(2023, 8, day));
            }

            return;
        }


    }
}
