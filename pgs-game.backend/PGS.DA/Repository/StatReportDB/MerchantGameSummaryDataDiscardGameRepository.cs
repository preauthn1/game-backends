using target.Core.Repository;
using target.Models.Enums;
using target.Models.Table;

namespace target.DA.Repository
{
    public class MerchantGameSummaryDataDiscardGameRepository : FormatIntegerPrimaryKeyRepository<MerchantGameSummaryDataDiscardGameEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.StatReportDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[MerchantGameSummaryData_DiscardGame]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "StatTime"; }
        }
    }
}
