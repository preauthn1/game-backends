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
    public class OnlineUserSummaryServiceTest : BaseTest
    {

        public IOnlineUserSummaryService ServiceInstance { get; set; }


        [TestMethod]
        public void GetEachGameOnlineUsers_Test()
        {
            var query = ServiceInstance.GetEachGameOnlineUsers(_testConfig.Default.UserID);
            this.AssertEmptyCollection(query);
            return;
        }

    }
}
