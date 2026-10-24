using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.Models.CoreLibs;
using target.Models.Models;
using target.Utility.Extensions;
using target.Utility.Helper;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class SystemLogApiRequestLogServiceTests : BaseTest
    {

        public ISystemLogApiRequestLogService SystemLogApiRequestLogService { get; set; }


        [TestMethod]
        public void GetTopSizeList_Test()
        {
            if(IsNotDeveloperComputerName())
            {
                return;
            }
                        
            string targetTableName = "[dbo].[ApiRequestLogExternalVendorAPIErrorBrief]";

            var query = SystemLogApiRequestLogService.GetTopSizeList(_TEST_DATA.Default.UserID, new Models.ViewModels.QueryApiRequestLogRequestParameterModel
            {
                PageSize = 10,
                TargetTableName = targetTableName,
                CreateTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-10),
                CreateTimeEnd = DateTimeHelper.GetCurrentTime(),
            });

            if(query.Items.IsNotEmptyNull())
            {
                var matchFirst = query.Items.Where(x=>x.UniqueID.IsNotEmptyTextNotNull()).FirstOrDefault();

                var detail = SystemLogApiRequestLogService.GetByUniqueID(new Models.ViewModels.LazyCommonRequestModel
                {
                    UniqueID = matchFirst.UniqueID,
                    TableName = targetTableName
                });

                Assert.IsTrue(detail != null);
            }

            return;
        }

    }
}

