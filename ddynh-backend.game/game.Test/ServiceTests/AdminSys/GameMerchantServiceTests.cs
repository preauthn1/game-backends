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
    public class GameMerchantServiceTest : BaseTest
    {

        public IGameMerchantService ServiceInstance { get; set; }


        [TestMethod]
        public void GetPagingByFilter_Test()
        {
            var query = ServiceInstance.GetPagingByFilter(new QueryGameMerchantRequestViewModel
            {
                PageSize = 10,
            });

            this.AssertEmptyCollection(query.PageData);
            Assert.IsTrue(query.PageData.Count() <= 10);


            var querySinle = ServiceInstance.GetByID(query.PageData.FirstOrDefault().GameMerchantID);
            Assert.IsNotNull(querySinle);

            return;
        }



    }
}
