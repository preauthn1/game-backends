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
    public class VendorMaintenanceRepositoryTests : BaseTest
    {
        public MerchantSelectTemplateRepository MerchantSelectTemplateRepository { get; set; }

        public VendorMaintenanceRepository VendorMaintenanceRepository { get; set; }



        [TestMethod]
        public void GetVendorMaintenance_OnProgressingOrReadyToStart_Test()
        {
            var q1 = VendorMaintenanceRepository.GetVendorMaintenance_OnProgressingOrReadyToStart(GameVendorTypeEnum.AGG.ToInteger());
            var q2 = VendorMaintenanceRepository.GetVendorMaintenance_OnProgressingOrReadyToStart(GameVendorTypeEnum.target.ToInteger());
            return;
        }


        [TestMethod]
        public void IsPlayerCanSkipMaintainByExcludeMerchantSetting_Test()
        {
            int playerID = _TEST_DATA.Default.PlayerID;
            int merchantID = _TEST_DATA.Default.MerchantID;

            var queryTempaltes = MerchantSelectTemplateRepository.GetPagingByFilter(new QueryMerchantSelectTemplateRequestViewModel
            {
                AssignIncludeMerchantIDs = merchantID.ToListCollection(),
                IsIncludeDeleteData = true,
                PageSize = 5
            });

            if (queryTempaltes.Items.IsNotEmptyNull())
            {
                var match = queryTempaltes.Items.FirstOrDefault();
                bool isPlayerCanSkip = VendorMaintenanceRepository.IsPlayerCanSkipMaintainByExcludeMerchantSetting(match.TemplateID, playerID);
                Assert.IsTrue(isPlayerCanSkip);
            }

            return;
        }


        [TestMethod]
        public void GetVendorMaintenanceByCache_Test()
        {
            var q1 = VendorMaintenanceRepository.GetVendorMaintenanceByCache(GameVendorTypeEnum.AGG.ToInteger());
            var q2 = VendorMaintenanceRepository.GetVendorMaintenanceByCache(GameVendorTypeEnum.AGGLive.ToInteger());
            var q3 = VendorMaintenanceRepository.GetVendorMaintenanceByCache(GameVendorTypeEnum.target.ToInteger());

            var q21 = VendorMaintenanceRepository.GetVendorMaintenanceByCache(GameVendorTypeEnum.AGG.ToInteger());
            var q22 = VendorMaintenanceRepository.GetVendorMaintenanceByCache(GameVendorTypeEnum.AGGLive.ToInteger());


            return;
        }


    }
}

