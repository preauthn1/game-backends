using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.BL.Services;
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
    public class OneToManyCustomizedSettingServiceTests : BaseTest
    {

        public IOneToManyCustomizedSettingService ServiceInstance { get; set; }


        [TestMethod]
        public void GetByPagingFilter_Test()
        {
            var query = ServiceInstance.GetByPagingFilter(_TEST_DATA.Default.UserID , new QueryOneToManyCustomizedSettingApiRequestParameterModel
            {

            });

            Assert_NotEmptyCollection(query.Items);

            var matchFirst = query.Items.FirstOrDefault();

            var querySettingItems = ServiceInstance.GetSettingItems(_TEST_DATA.Default.UserID, matchFirst.OneToManyCustomizedSettingID);

            Assert_NotEmptyCollection(querySettingItems.Groups);
            return;
        }



        [TestMethod]
        public void InsertNewEmptyItem_Test()
        {
            int userID = _TEST_DATA.Default.UserID;

            var query = ServiceInstance.GetByPagingFilter(userID, new QueryOneToManyCustomizedSettingApiRequestParameterModel
            {
                Type = OneToManyCustomizedSettingTypeEnum.GameUrlDoamin_BackupUrls.ToInteger()
            });

            if(query.Items.IsNotEmptyNull())
            {
                var matchFirst = query.Items.FirstOrDefault();

                var settings = ServiceInstance.GetSettingItems(userID, matchFirst.OneToManyCustomizedSettingID);

                ServiceInstance.UpdateSettingItems(userID, new SystemConfigSettingUpdateRequestModel
                {
                    OwnerID = matchFirst.OneToManyCustomizedSettingID,
                    Items = settings.Groups.SelectMany(z=> z.Items)
                    .Select(x=> new SystemConfigSettingUpdateRequestItemModel
                    {
                        Value = x.Value,
                        SettingKey = x.SettingKey,
                        SettingKeyText = x.SettingKeyText
                    })
                    .ToList()
                });

                ServiceInstance.InsertNewEmptyItem(userID, new OneToManySettingEazyRequestModel
                {
                    OneToManyCustomizedSettingID = matchFirst.OneToManyCustomizedSettingID
                });
            }

            return;
        }


    }
}

