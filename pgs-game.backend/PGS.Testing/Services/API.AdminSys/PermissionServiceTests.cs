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
    public class PermissionServiceTests : BaseTest
    {

        public IPermissionService PermissionService { get; set; }

        public UserPermissionMappingRepository UserPermissionMappingRepository { get; set; }

        public PermissionRepository PermissionRepository { get; set; }


        [TestMethod]
        public void GetAvailablePermissions_Test()
        {
            var query = PermissionService.GetAvailablePermissionsForMenu(_TEST_DATA.Default.UserID);
            Assert_NotEmptyCollection(query.AllPermissions);
            return;
        }


        [TestMethod]
        public void GetPermissions_Test()
        {
            var query = PermissionService.GetPermissions(_TEST_DATA.Default.UserID);
            Assert_NotEmptyCollection(query);
            return;
        }


        [TestMethod]
        public void GetPermissionsNestedFormat_Test()
        {
            var query = PermissionService.GetPermissionsNestedFormat(_TEST_DATA.Default.UserID);
            Assert_NotEmptyCollection(query.AllPermissions);
            return;
        }


        [TestMethod]
        public void IsPermissionExist_Test()
        {
            AdminSysPermissionKey key1 = AdminSysPermissionKey.Home;
            AdminSysPermissionKey key2 = (AdminSysPermissionKey) (-1);

            int defaultUserID = _TEST_DATA.Default.UserID;

            Assert.IsTrue(PermissionService.IsPermissionExist(defaultUserID, key1.ToListCollection()));
            Assert.IsFalse(PermissionService.IsPermissionExist(defaultUserID, key2.ToListCollection()));

            var query = UserPermissionMappingRepository.GetPagingByFilter(new QueryUserPermissionMappingRequestViewModel
                {
                    PageSize = 50
                })
                .Items
                .Where(x => x.UserID != defaultUserID)
                .ToList();

            if(query.IsNotEmptyNull())
            {
                foreach(var matchFisrt in query.OrderByRandom().Take(5))
                {
                    var matchPermission = PermissionRepository.GetById(matchFisrt.PermissionID);

                    var matchEnumKey = ExtensionOfEnum.GetEnumByTextValue<AdminSysPermissionKey>(matchPermission.PermissionToken, key2);

                    //有可能部分enums沒定義
                    if (matchEnumKey == key2)
                        continue;

                    Assert.IsTrue(PermissionService.IsPermissionExist(matchFisrt.UserID, matchEnumKey.ToListCollection()));
                }
            }

            return;
        }





    }
}

