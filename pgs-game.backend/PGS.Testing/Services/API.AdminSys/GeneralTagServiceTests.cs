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

namespace target.Testing
{
    [TestClass]
    public class GeneralTagServiceTests : BaseTest
    {

        public IGeneralTagService ServiceInstance { get; set; }


        private static DateTime SkipTestTempDate = new DateTime(2023, 12, 8);


        [TestMethod]
        public void GetByPagingFilter_Test()
        {
            if(IsSkipTest_WhenSmallerThanTargetTime(SkipTestTempDate))
            {
                return;
            }

            var query = ServiceInstance.GetByPagingFilter(_TEST_DATA.Default.UserID , new QueryGeneralTagApiRequestParameterModel
            {

            });

            Assert_NotEmptyCollection(query.Items);
            Assert.IsNotNull(new CRUDResultModel());
            return;
        }


        [TestMethod]
        public void UpdateAssigner_Test()
        {
            if (IsSkipTest_WhenSmallerThanTargetTime(SkipTestTempDate))
            {
                return;
            }

            int userID = _TEST_DATA.Default.UserID;

            var query = ServiceInstance.GetByPagingFilter(userID, new QueryGeneralTagApiRequestParameterModel
            {

            });

            var matchFirst = query.Items
                   .Where(x => x.Type == TagTypeEnum.GameTag.ToInteger())
                   .OrderBy(x=> x.TagID == 10001 ? 0 : 999)
                   .FirstOrDefault();

            if (matchFirst != null)
            {
                ServiceInstance.UpdateAssigner(userID, new GeneralTagUpdateRequestModel
                {
                    TagID = matchFirst.TagID,
                    AssignTargetIDs = new List<int>
                    {
                        _TEST_DATA.Default.GameID,
                        _TEST_DATA.Default.GameID_AGG
                    }
                });

                var queryDetail = ServiceInstance.GetByID(matchFirst.TagID);

                Assert.IsNotNull(queryDetail);
                Assert.IsTrue(queryDetail.AssignTargetIDs.IsNotEmptyNull());

                ServiceInstance.AppendteTagsToAssigner(userID, new GeneralTagUpdateByAssignerRequestModel
                {
                    AssignTargetID = _TEST_DATA.Default.GameID,
                    Type = TagTypeEnum.GameTag.ToInteger(),
                    TagIDs = new List<int>
                    {
                        matchFirst.TagID
                    }
                });
            }

            return;
        }



    }
}

