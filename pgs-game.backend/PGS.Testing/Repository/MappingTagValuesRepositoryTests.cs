using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
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
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class MappingTagValuesRepositoryTests : BaseTest
    {

        public MappingTagValuesRepository MappingTagValuesRepository { get; set; }


        [TestMethod]
        public void CreateMany_BundleInsert_Test()
        {
            var orderResult = RandomHelper.GetPgsUniqueDateHourSequenceOrderNoAndTime();

            List<MappingTagValuesEntity> insert1 = new List<MappingTagValuesEntity>();
            List<MappingTagValuesEntity> insert2 = new List<MappingTagValuesEntity>();

            insert1.Add(new MappingTagValuesEntity
            {
                SettingKey = ApiReqeustLogTagValueMappingTypeEnum.Memo.ToInteger(),
                SettingValue = Guid.NewGuid().ToString(),
                PartitionTime = orderResult.TimeWithHour,
                UniqueID = orderResult.TransactionNo
            });

            insert2.Add(new MappingTagValuesEntity
            {
                SettingKey = ApiReqeustLogTagValueMappingTypeEnum.Headers.ToInteger(),
                SettingValue = "Headers_MSTest_" + Guid.NewGuid().ToString(),
                PartitionTime = orderResult.TimeWithHour,
                UniqueID = orderResult.TransactionNo
            });

            MappingTagValuesRepository.CreateMany_BundleInsert(insert1 , insert2);
            return;
        }

    }
}

