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
    public class TransferRecordServiceTest : BaseTest
    {

        public ITransferRecordService ServiceInstance { get; set; }


        [TestMethod]
        public void GetPagingByFilter_Test()
        {
            var query = ServiceInstance.GetPagingByFilter(new QueryTransferRecordRequestViewModel
            {
                PageSize = 10
            });

            this.AssertEmptyCollection(query.PageData);
            Assert.IsTrue(true);

            return;
        }



        [TestMethod]
        public void GetPagingByFilter_FilterDate_Test()
        {
            var query = ServiceInstance.GetPagingByFilter(new QueryTransferRecordRequestViewModel
            {
                PageSize = 10,
                BeginDate = new DateTime(2020 , 2 , 1)
            });

            this.AssertEmptyCollection(query.PageData);
            Assert.IsTrue(true);

            return;
        }


    }
}
