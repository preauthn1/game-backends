using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.DA.Repository;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.ViewModels;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class BetRecordExtraInfoRepositoryTests : BaseTest
    {

        public BetRecordExtraInfoRepository BetRecordExtraInfoRepository { get; set; }


        [TestMethod]
        public void Create_Test()
        {
            var recordNo = RandomHelper.GetPgsUniqueDateHourSequenceOrderNo();
            BetRecordExtraInfoRepository.CreateOrUpdate(recordNo, "MSTest");

            var queryAgain = BetRecordExtraInfoRepository.GetByBetRecordNoPGS(recordNo);

            Assert.IsNotNull(queryAgain.BetRecordNoPGS);
            Assert.IsTrue(queryAgain.BetRecordNoPGS == recordNo);
        }

    }
}

