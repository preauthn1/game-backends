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
    public class ExtensionOfCollectionTests
    {

        [TestMethod]
        public void SplitListByTargetLength_Test()
        {
            List<int> list = new List<int>
            {
                3,3,3,
                4,4,4,
                5,2,5,
                9,9,9,
                1,1
            };

            var splitResult = list.SplitListByTargetLength(3);
            Assert.IsTrue(splitResult.Count() == 5);
            return;
        }



        [TestMethod]
        public void SplitListByTargetLength_Test_2()
        {
            List<int> list = new List<int>
            {
                3,3,
            };

            var splitResult = list.SplitListByTargetLength(3);
            Assert.IsTrue(splitResult.Count() == 1);
            return;
        }


    }




}
