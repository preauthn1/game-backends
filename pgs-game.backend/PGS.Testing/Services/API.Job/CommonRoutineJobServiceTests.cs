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
using System.Threading.Tasks;

namespace target.Testing
{
    [TestClass]
    public class CommonRoutineJobServiceTests : BaseTest
    {

        public ICommonRoutineJobService CommonRoutineJobService { get; set; }

        //





        [TestMethod]
        public void CommonRoutineJobService_Local_Test()
        {
            if(IsNotDeveloperComputerName())
            {
                return;
            }

            var myService = new CommonRoutineJobService();
            myService.CheckElasticSearchErrorLogAlert();

            return;
        }





        [TestMethod]
        public void RoutineJobs_Every30Minutes_Test()
        {
            CommonRoutineJobService.RoutineJobs_Every30Minutes();
            return;
        }


        [TestMethod]
        public async Task CheckMerchantHierarchyTreeWeiredAsync_Test()
        {
            await CommonRoutineJobService.CheckMerchantHierarchyTreeWeiredAsync();
            return;
        }



        [TestMethod]
        public void ExecuteStoreProcedureList_Test()
        {
            var myService = new CommonRoutineJobService();
            myService.ExecuteStoreProcedureList(OneToManyCustomizedSettingTypeEnum.RoutineJob_StoreProcedureList_24Hours);
            return;
        }


        [TestMethod]
        public void ExecuteStoreProcedureList_8Hours_Test()
        {
            var myService = new CommonRoutineJobService();
            myService.ExecuteStoreProcedureList(OneToManyCustomizedSettingTypeEnum.RoutineJob_StoreProcedureList_8Hours);
            return;
        }



        [TestMethod]
        public void CheckMissPerGameAndSendNotify_Test()
        {
            List<GameVendorTypeEnum> gameVendors = new List<GameVendorTypeEnum>
            {
                GameVendorTypeEnum.AGG,
                GameVendorTypeEnum.AGGLive,
                GameVendorTypeEnum.AGGFish,
            };

            var myService = new CommonRoutineJobService();

            var begin = DateTimeHelper.GetCurrentTime().AddDays(-1);
            var end = DateTimeHelper.GetCurrentTime();

            foreach (GameVendorTypeEnum gameVendor in gameVendors)
            {
                myService.CheckMissPerGameAndSendNotify(gameVendor, begin, end , null);
            }

            return;
        }



        [TestMethod]
        public void CheckLockSQLs_Test()
        {           
            var myService = new CommonRoutineJobService();
            myService.CheckLockSQLs();
            return;
        }



    }
}

