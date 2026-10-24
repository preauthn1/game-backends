using System;
using System.Collections.Generic;
using game.Entity;
using game.Service.DAO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using game.Infrastructure.Extensions;
using System.Reflection;
using FW.Core.Engine;

namespace game.Test
{
    [TestClass]
    public partial class BaseTest
    {

        public TestDataConfig _testConfig = new TestDataConfig();



        public BaseTest()
        {
            InitailHelper.InitializeIOC();

            InstanceAllRepositoryImplement();
        }
        

        protected void AssertEmptyCollection<T>(IEnumerable<T> models)
        {
            Assert.IsTrue(models.IsNotEmptyNull());
        }


        protected void InstanceAllRepositoryImplement()
        {           
            var allService = this.GetType()
                .GetProperties(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
                .Where(x => x.Name.ToLower().Contains("service"))
                .ToList();


            foreach (PropertyInfo item in allService)
            {
                if (item.PropertyType.IsInterface)
                {
                    var instanceValue = IOCHelper.Resolve(item.PropertyType);
                    item.SetValue(this, instanceValue);
                }
            }

        }

    }
}
