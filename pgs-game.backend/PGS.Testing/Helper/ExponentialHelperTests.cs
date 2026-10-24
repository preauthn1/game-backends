using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.DA.Repository;
using target.Models.Enums;
using target.Models.Models;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class ExponentialHelperTests : BaseTest
    {

        [TestMethod]
        public void AppHelper_Local_Test()
        {
            for (int x = 0; x <= 50; x += 2)
            {
                decimal result = ExponentialHelper.DetermineCasinoGame_SuspiciousRTP_ByRound(x);
                Debug.WriteLine($"x = {x}, y = {result:F2}");
            }

            for (int x = 30; x <= 50; x += 2)
            {
                decimal result = ExponentialHelper.DetermineCasinoGame_SuspiciousRTP_ByRound(x);
                Assert.IsTrue(result > 97.5M && result < 99);
            }

            return;
        }

    }
}
