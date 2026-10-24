using System;
using System.Collections.Generic;
using game.Entity;
using game.Service.DAO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using game.Service;
using game.Entity.ViewModels.Query;
using game.Infrastructure.Extensions;
using game.Entity.ViewModels;
using game.Infrastructure;

namespace game.Test
{
    [TestClass]
    public class PromotionEventApiServiceTest : BaseTest
    {

        public IPromotionEventApiService ServiceInstance { get; set; }


        [TestMethod]
        public void GetEvents_Test()
        {
            var query = ServiceInstance.GetEvents(new PromotionEventRequestViewModel
            {
                UserID = _testConfig.Default.UserID,
                Certificate =  SystemConst.MAGIC_CODE
            });

            Assert.IsTrue(query != null);
            return;
        }



        [TestMethod]
        public void GetEventDetail_Test()
        {
            var query = ServiceInstance.GetEvents(new PromotionEventRequestViewModel
            {
                UserID = _testConfig.Default.UserID,
                Certificate = _testConfig.Default.Certificate
            });


            if(query.Events.IsNotEmptyNull())
            {
                var matchFirst = query.Events.FirstOrDefault();

                var queryDetail = ServiceInstance.GetEventDetail(new PromotionEventDetailRequestViewModel
                {
                    UserID = _testConfig.Default.UserID,
                    Certificate = _testConfig.Default.Certificate,
                    EventID = matchFirst.EventID
                });

                Assert.IsNotNull(queryDetail);
            }

            return;
        }



    }
}
