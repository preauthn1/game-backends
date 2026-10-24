using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.Core.Repository;
using target.DA.Repository;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class SystemSPRepositoryTests : BaseTest
    {

        [TestMethod]
        public void GetMasterDbDiskRemaningSize_Test()
        {
            var rep = new SystemSPRepository();
            var query = rep.GetMasterDbDiskRemaningSize();

            //drive, 
            //MB可用空間

            return;
        }

    }
}

