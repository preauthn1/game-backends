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

namespace game.Test
{
    [TestClass]
    public class OtherTests
    {

        [TestMethod]
        public void OtherTest_Test()
        {
            long qqqqqq = 1555;
            var qqqq1 = qqqqqq.To_Cash();





            string qqq1 = "http://example.invalid/gpks/webdb/";
            string qqq2 = "http://example.invalid/gpks/webdb";
            string result1 = qqq1.ReplaceLastWordIfMatch('/');
            string result2 = qqq2.ReplaceLastWordIfMatch('/');


            string text = "200";
            var result = (long)Decimal.Parse(text);
            return;
        }
        
    }
}
