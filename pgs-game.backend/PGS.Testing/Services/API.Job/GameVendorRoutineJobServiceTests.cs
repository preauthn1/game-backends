using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.DA.Repository;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace target.Testing
{
    [TestClass]
    public class GameVendorRoutineJobServiceTests : BaseTest
    {

        public IGameVendorRoutineJobService ServiceInstance { get; set; }

        public BetRecordTagMappingRepository BetRecordTagMappingRepository { get; set; }

        public BetRecordAGGRepository BetRecordAGGRepository { get; set; }

        public BetRecordRawWagerTagMappingRepository BetRecordRawWagerTagMappingRepository { get; set; }


        [TestMethod]
        public void CheckPendingWagersAndCancelNotFoundTag_Test()
        {
            var query = BetRecordAGGRepository.GetPagingByFilter(new QueryBetRecordGeneralRequestViewModel
            {
                PageSize = 5,
                CreateTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-7),
                OrderByColumn = "CreateTime",
                IsASC = false
            });

            if (query.Items.IsEmptyOrNull())
                return;

            var matchFirst = query.Items.OrderByRandom().FirstOrDefault();

            //先清除資料
            BetRecordTagMappingRepository.DeleteByModel(new { BetRecordNoPGS = matchFirst.BetRecordNoPGS });
            BetRecordRawWagerTagMappingRepository.DeleteByModel(new { BetRecordNo = matchFirst.BetRecordNo });

            matchFirst.Status = BetRecordStatusEnum.Pending.ToInteger();
            BetRecordAGGRepository.Update(matchFirst);

            //增加tag
            BetRecordRawWagerTagMappingRepository.Create(new Models.Table.BetRecordRawWagerTagMappingEntity
            {
                BetRecordNo = matchFirst.BetRecordNo,
                GameVendorID = GameVendorTypeEnum.AGG.ToInteger(),
                CreateTime = DateTimeHelper.GetCurrentTime(),
                TagID = SystemTagEnum.BetRecord_Cancel_ButNotFound.ToInteger(),
            });

            ServiceInstance.CheckPendingWagersAndCancelNotFoundTag();

            Thread.Sleep(1000);

            var queryInsertData = BetRecordTagMappingRepository.GetByFilter(new QueryBetRecordTagMappingRequestViewModel
            {
                BetRecordNoPGSList = matchFirst.BetRecordNoPGS.ToListCollection()
            });

            Assert.IsTrue(queryInsertData.Any(z => z.TagID == SystemTagEnum.BetRecord_Cancel_ButNotFound.ToInteger()));
            return;
        }


    }
}

