using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.Models.Enums;
using target.Models.Models;
using target.Models.ViewModels;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Utility.Utility;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class TypeScriptConvertTests : BaseTest
    {

        [TestMethod]
        public void ApiRequestHelper_RequestApiResult_Test()
        {
            var apiResult = TypeScriptConvert.ConvertClassToTypeScriptModel(typeof(MerchantAllPlayerWinLossStatReportInfoViewModel));
            return;
        }
		
        
    }
}
