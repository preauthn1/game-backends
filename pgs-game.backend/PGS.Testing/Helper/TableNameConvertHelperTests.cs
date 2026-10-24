using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.Models.Enums;
using target.Models.Models;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Utility.Utility;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class TableNameConvertHelperTests : BaseTest
    {
        [TestMethod]
        public void GetElasticsearchIndexNameByTableName_Test()
        {
            Assert.IsTrue(TableNameConvertHelper.GetElasticsearchIndexNameByTableName("ExterNALApiRequestLog") == GetESIndexName(ElasticLogTableTypeEnum.ExternalApiRequestlog));
            Assert.IsTrue(TableNameConvertHelper.GetElasticsearchIndexNameByTableName("[dbo].[ExternalApiRequestlog]") == GetESIndexName(ElasticLogTableTypeEnum.ExternalApiRequestlog));
            Assert.IsTrue(TableNameConvertHelper.GetElasticsearchIndexNameByTableName("SinglewalletMerchantApiRequestLog") == GetESIndexName(ElasticLogTableTypeEnum.SinglewalletMerchantApiRequestLog));
            Assert.IsTrue(TableNameConvertHelper.GetElasticsearchIndexNameByTableName("[ApiRequestLogVendorAPIError]") == GetESIndexName(ElasticLogTableTypeEnum.ApiRequestLogVendorAPIError));
            Assert.IsTrue(TableNameConvertHelper.GetElasticsearchIndexNameByTableName("[ApiRequestLogVendorAPIErrorBrief]") == GetESIndexName(ElasticLogTableTypeEnum.ApiRequestLogVendorAPIError));
            Assert.IsTrue(TableNameConvertHelper.GetElasticsearchIndexNameByTableName("[dbo].[ApiRequestLogVendorAPIErrorBrief]") == GetESIndexName(ElasticLogTableTypeEnum.ApiRequestLogVendorAPIError));

            return;

        }

        [TestMethod]
        public void GetElasticsearchIndexName_Test()
        {
            Assert.IsTrue("externalapirequestlog" == GetESIndexName(ElasticLogTableTypeEnum.ExternalApiRequestlog));
            Assert.IsTrue("singlewalletmerchantapirequestlog" == GetESIndexName(ElasticLogTableTypeEnum.SinglewalletMerchantApiRequestLog));           
            Assert.IsTrue("apirequestlogexternalvendorapi" == GetESIndexName(ElasticLogTableTypeEnum.ApiRequestLogExternalVendorApi));
            Assert.IsTrue("apirequestlogvendorapierror" == GetESIndexName(ElasticLogTableTypeEnum.ApiRequestLogVendorAPIError));
            return;

        }

        private string GetESIndexName(ElasticLogTableTypeEnum type)
        {
            string result = TableNameConvertHelper.GetElasticsearchIndexName(type);
            return result;
        }

    }
}
