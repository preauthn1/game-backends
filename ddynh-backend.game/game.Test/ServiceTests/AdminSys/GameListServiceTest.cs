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
    public class GameListServiceTest : BaseTest
    {

        public IGameListService Service { get; set; }
        

        [TestMethod]
        public void GetByFilter_Test()
        {
            var query = Service.GetByFilter(new QueryGameListRequestViewModel());
            this.AssertEmptyCollection(query);
            return;
        }
        

        [TestMethod]
        public void GetPagingByFilter_Test()
        {
            var query = Service.GetPagingByFilter(new QueryGameListRequestViewModel
            {
                PageSize = 10
            });
            this.AssertEmptyCollection(query.PageData);
            return;
        }

    }
}
