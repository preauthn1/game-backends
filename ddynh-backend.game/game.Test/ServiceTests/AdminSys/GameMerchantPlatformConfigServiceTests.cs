using System;
using System.Collections.Generic;
using game.Entity;
using game.Service.DAO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using game.Service;
using game.Entity.ViewModels.Query;
using game.Infrastructure.Extensions;

namespace game.Test
{
    [TestClass]
    public class GameMerchantPlatformConfigServiceTest : BaseTest
    {

        public IGameMerchantPlatformConfigService ServiceInstance { get; set; }


        [TestMethod]
        public void GetPagingByFilter_Test()
        {
            var query = ServiceInstance.GetPagingByFilter(new QueryGameMerchantPlatformConfigRequestViewModel
            {
                PageSize = 100
            });

            this.AssertEmptyCollection(query.PageData);
            Assert.IsTrue(true);

            var querySinle = ServiceInstance.GetByID(query.PageData.FirstOrDefault().ID);
            Assert.IsNotNull(querySinle);

            return;
        }

    }
}
