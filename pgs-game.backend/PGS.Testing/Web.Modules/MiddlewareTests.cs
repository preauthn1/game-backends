using Autofac;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using target.BL.Interface;
using target.Models.Table;
using target.Utility.Helper.HttpContext;
using target.Utility.IOC;
using target.Web.Modules.Filter;
using System;
using example.invalid;
using System.Text;
using System.Threading.Tasks;

namespace target.Testing
{
    
    [TestClass]
    [DoNotParallelize]
    public class MiddlewareTests : BaseTest
    {
        [TestMethod]
        public async Task Test_001()
        {
           
        }

    }
}
