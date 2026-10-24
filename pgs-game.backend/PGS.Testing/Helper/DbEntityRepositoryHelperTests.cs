using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.Core.Repository;
using target.DA.Repository;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.Utility.Extensions.Web;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class DbEntityRepositoryHelperTests : BaseTest
    {

        [TestMethod]
        public void GetPrimaryKeyColumnValueForDebug_Test()
        {
            string uuid = Guid.NewGuid().ToString();
            int tagID = 5566;

            var myEntity = new BetRecordTagMappingEntity
            {
                BetRecordNoPGS = uuid,
                TagID = tagID
            };

            var debugInfo = DbEntityRepositoryHelper.GetPrimaryKeyColumnValueForDebug(myEntity);

            Assert.IsTrue(debugInfo.Contains(uuid) && debugInfo.Contains(tagID.ToString()));
            return;

        }

    }
}

