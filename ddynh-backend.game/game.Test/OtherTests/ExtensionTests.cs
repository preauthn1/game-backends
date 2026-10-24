using System;
using System.Collections.Generic;
using game.Entity;
using game.Service.DAO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using FW.Core.Repository;
using game.Entity.ViewModels;
using game.Infrastructure.Extensions;
using System.Reflection;
using game.Infrastructure.Extensions;

namespace game.Test
{
    [TestClass]
    public class ExtensionTests
    {

        [TestMethod]
        public void CalculateTotalSumMergeToOneObject_Test()
        {
            List<TestSumModel> list = new List<TestSumModel>();

            list.Add(new TestSumModel
            {
                ID = 1,
                Amount = 500,
                PayStatus = 1,
                Tax = 16,
                TotalUserCount = 25,
            });

            list.Add(new TestSumModel
            {
                ID = 2,
                Amount = 750,
                PayStatus = 1,
                Tax = 3,
                TotalUserCount = 9,
            });

            list.Add(new TestSumModel
            {
                ID = 3,
                Amount = 1500,
                PayStatus = 1,
                Tax = 77,
                TotalUserCount = 6,
            });


            var mergeResult = list.CalculateTotalSumMergeToOneObject();


            Assert.AreEqual(mergeResult.Amount, list.Sum(z => z.Amount));
            Assert.AreEqual(mergeResult.TotalUserCount, list.Sum(z => z.TotalUserCount));            
            return;
        }
        
    }


    public class TestSumModel
    {
        public int ID { get; set; }

        public int PayStatus { get; set; }

        public int Amount { get; set; }

        public int Tax { get; set; }

        public int TotalUserCount { get; set; }

    }




}
