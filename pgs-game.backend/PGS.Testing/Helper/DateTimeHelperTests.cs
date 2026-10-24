using Lck.Utility.Extensions.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.Models.Enums;
using target.Models.Models;
using target.Utility.Extensions;
using target.Utility.Helper;
using System;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class DateTimeHelperTests : BaseTest
    {

        [TestMethod]
        public void Convert_GMT8_DateStringToUtcDateTime_Test()
        {
            //假定這些時間都強制認定為+8
            var time1 = DateTimeHelper.Convert_GMTDateString_ToUtcDateTime("2024-01-01 08:30:00" , 8);
            var time2 = DateTimeHelper.Convert_GMTDateString_ToUtcDateTime("2024-01-01T08:00:00Z", 8);

            Assert.IsTrue(time1.Hour == 0);
            Assert.IsTrue(time2.Hour == 0);

            return;
        }


        [TestMethod]
        public void ReplaceMillisecondsToZero_Test()
        {
            var time1 = DateTime.Parse("2025-06-12T15:59:59.999");
            var time2 = DateTime.Parse("2025-06-12T15:59:59.777");

            Assert.IsTrue(time1.Millisecond == 999);
            Assert.IsTrue(time2.Millisecond == 777);

            var result1 = DateTimeHelper.ReplaceMillisecondsToZero(time1);
            var result2 = DateTimeHelper.ReplaceMillisecondsToZero(time2);

            Assert.IsTrue(result1.Hour == time1.Hour);
            Assert.IsTrue(result1.Minute == time1.Minute);
            Assert.IsTrue(result1.Month == time1.Month);
            Assert.IsTrue(result1.Day == time1.Day);
            Assert.IsTrue(result1.Year == time1.Year);
            Assert.IsTrue(result1.Millisecond == 0);
            Assert.IsTrue(result2.Millisecond == 0);

            return;
        }


    }
}
