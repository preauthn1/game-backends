using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.Core.Repository;
using target.DA.Repository;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class PerGameDetailLogTagMappingRepositoryTests : BaseTest
    {
        public PerGameDetailLogTagMappingRepository PerGameDetailLogTagMappingRepository { get; set; }


        [TestMethod]
        public void PerGameDetailLogTagMappingRepository_Create_Test()
        {
            long targetID = 1;

            PerGameDetailLogTagMappingRepository.DeleteByModel(new { PerGameDetailLogID = targetID });

            PerGameDetailLogTagMappingRepository.Create(new PerGameDetailLogTagMappingEntity
            {
                PerGameDetailLogID = targetID,
                TagID = SystemTagEnum.PerGameDetailLog_Jackpot.ToInteger(),
                GameTime = DateTimeHelper.GetCurrentTime(),
                TransactionNo = RandomHelper.GetPgsUniqueDateHourSequenceOrderNo()
            });

            return;
        }

    }
}

