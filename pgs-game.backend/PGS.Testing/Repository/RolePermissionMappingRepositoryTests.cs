using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.DA.Repository;
using target.Models;
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
    public class RolePermissionMappingRepositoryTests : BaseTest
    {

        public RolePermissionMappingRepository RolePermissionMappingRepository { get; set; }


        [TestMethod]
        public void IsDataExistByFilter_Test()
        {
            var isExist1 = RolePermissionMappingRepository.IsDataExistByFilter(new QueryRolePermissionMappingRequestViewModel
            {
                RoleIDs = _TEST_DATA.Default.RoleID.ToListCollection(),
                PermissionToken = [REDACTED]",
            });

            var isExist2 = RolePermissionMappingRepository.IsDataExistByFilter(new QueryRolePermissionMappingRequestViewModel
            {
                RoleIDs = _TEST_DATA.NotSuperMerchant.RoleID.ToListCollection(),
                PermissionToken = [REDACTED]",
            });

            Assert.IsTrue(isExist1);
            Assert.IsFalse(isExist2);
            return;
        }


        [TestMethod]
        public void GetByFilter_NotSuperRole_Test()
        {
            var myFilter = new QueryRolePermissionMappingRequestViewModel
            {
                RoleIDs = _TEST_DATA.NotSuperMerchant.RoleID.ToListCollection(),
                MatchAnyPermissionTokens = SystemConstant.AdminSys_HighRiskPermissions,
            };

            var query1 = RolePermissionMappingRepository.GetByFilter(myFilter);
            var query2 = RolePermissionMappingRepository.IsDataExistByFilter(myFilter);

            Assert.IsTrue(query1.IsEmptyOrNull());
            Assert.IsFalse(query2);
            return;
        }


        [TestMethod]
        public void IsDataExistByFilter_HighRiskFilter_Test()
        {
            //找出有標記高風險權限的roles
            var query = RolePermissionMappingRepository.GetByFilter(new QueryRolePermissionMappingRequestViewModel
            {
                IsMatchHighRiskPermission = true,
            });

            Assert_NotEmptyCollection(query);

            int permisionCount = SystemConstant.AdminSys_HighRiskPermissions.Count();

            foreach (var item in query)
            {
                var queryHighPermissions = RolePermissionMappingRepository.GetByFilter(new QueryRolePermissionMappingRequestViewModel
                {
                    RoleIDs = item.RoleID.ToListCollection(),
                    MatchAnyPermissionTokens = SystemConstant.AdminSys_HighRiskPermissions,
                });

                Assert.IsTrue(queryHighPermissions.CountOrZero() >= 1 && queryHighPermissions.CountOrZero() <= permisionCount);
            }

            return;
        }


    }
}

