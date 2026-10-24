using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.Models.Models;
using target.Utility.Extensions;
using target.Utility.Helper;
using System.Linq;
using target.Models.CoreLibs;
using target.Models.ViewModels;
using target.DA.Repository;
using target.Models.Enums;

namespace target.Testing
{
    [TestClass]
    public class OrganizationServiceTests : BaseTest
    {

        public IOrganizationService ServiceInstance { get; set; }


        [TestMethod]
        public void Organization_GetPagingByFilter_Test()
        {
            var query = ServiceInstance.GetPagingByFilter(new QueryOrganizationApiRequestParameterModel { PageSize = 10 } );
            Assert_NotEmptyCollection(query.Items);

            var matchFirst = query.Items.FirstOrDefault();
            var querySingle = ServiceInstance.GetByID(_TEST_DATA.Default.UserID, matchFirst.OrganizationID);

            Assert.IsNotNull(querySingle);
            return;
        }


        [TestMethod]
        public void GetCustomizedSettings_Tests()
        {
            int userID = _TEST_DATA.Default.UserID;
            int organizationID = _TEST_DATA.Default.OrganizationID;

            var query = ServiceInstance.GetCustomizedSettings(userID, new OrganizationCustomizedSettingQueryModel
            {
                OrganizationID = organizationID
            });

            if (query.IsEmptyOrNull())
                return;

            Assert.IsTrue(query.All(x => x.OrganizationID == organizationID));
            return;
        }


        [TestMethod]
        public void CreateOrUpdateCustomizedSetting_Tests()
        {
            int organizationID = _TEST_DATA.NotSuperMerchant.OrganizationID;
            int userID = _TEST_DATA.Default.UserID;
            int targetSettingKey = OrganizationCustomizedSettingKey.LoginRestrictIPs.ToInteger();

            var myModel = new OrganizationCustomizedSettingViewModel
            {
                OrganizationID = organizationID,
                SettingKey = targetSettingKey,
                SettingValue = $"",
            };

            ServiceInstance.CreateOrUpdateCustomizedSetting(userID, myModel);

            var querySettings = ServiceInstance.GetCustomizedSettings(userID , new OrganizationCustomizedSettingQueryModel
            {
                OrganizationID = organizationID
            });

            var updatedSetting = querySettings.FirstOrDefault(x => x.SettingKey == targetSettingKey);

            Assert.IsNotNull(updatedSetting);
            Assert.AreEqual(myModel.OrganizationID, updatedSetting.OrganizationID);
            Assert.AreEqual(myModel.SettingKey, updatedSetting.SettingKey);
            Assert.AreEqual(myModel.SettingValue, updatedSetting.SettingValue);
            return;
        }

    }
}

