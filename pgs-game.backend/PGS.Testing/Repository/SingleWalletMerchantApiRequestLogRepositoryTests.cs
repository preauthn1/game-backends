using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.DA.Repository;
using target.Models.CoreLibs;
using target.Models.Models;
using target.Utility.Extensions;
using target.Utility.Helper;
using System;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class SingleWalletMerchantApiRequestLogRepositoryTests : BaseTest
    {

        public SingleWalletMerchantApiRequestLogRepository SingleWalletMerchantApiRequestLogRepository { get; set; }


        [TestMethod]
        public void GetTopSizeByES_Test()
        {
            if(IsNotDeveloperComputerName())
            {
                return;
            }

            DateTime begin = DateTimeHelper.GetCurrentTime().AddDays(-1 * 14);
            DateTime end = DateTimeHelper.GetCurrentTime();

            var query = SingleWalletMerchantApiRequestLogRepository.GetTopSizeByES(begin, end, 0);

            if(query.CountOrZero() > 0)
            {
                var matchFirst = query.OrderByRandom().FirstOrDefault();

                var querySingle = SingleWalletMerchantApiRequestLogRepository.GetFullInformationByUniqueID(matchFirst.UniqueID);

                Assert.IsTrue(querySingle != null);
                Assert.IsTrue(querySingle.UniqueID == matchFirst.UniqueID);
            }

            return;
        }

    }
}

