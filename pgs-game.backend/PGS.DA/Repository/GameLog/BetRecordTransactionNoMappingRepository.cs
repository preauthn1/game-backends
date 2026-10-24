using Dapper;
using game.Entity;
using Lck.Utility.Extensions.Helper;
using target.Core.Helper;
using target.Core.Repository;
using target.Models.Enums;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility;
using target.Utility.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace target.DA.Repository
{
    public class BetRecordTransactionNoMappingRepository : FormatStringPrimaryKeyRepository<BetRecordTransactionNoMappingEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.GameLogDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[BetRecordTransactionNoMapping]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "BetRecordNoPGS"; }
        }

        protected override DbType PK_Type
        {
            get
            {
                return DbType.AnsiString;
            }
        }

        protected override int? PK_Text_Length
        {
            get
            {
                return 32;
            }
        }

        public void Create(string betRecordNoPGS , string betRecordNo, int gameVendorID)
        {
            //2025/12 調整。不再紀錄betRecordNo
            //2025/12 調整。不再紀錄betRecordNo
            //2025/12 調整。不再紀錄betRecordNo

            var parseResult = RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(betRecordNoPGS);

            var param = new DynamicParameters();

            //擔心型別轉換不正確。會導致效能降低。因此自己處理
            param.Add("@PartitionTime", parseResult.Time.Value, dbType: DbType.DateTime);
            param.Add("@BetRecordNoPGS", betRecordNoPGS, dbType: DbType.AnsiString, size: PK_Text_Length.Value);
            param.Add("@GameVendorID", gameVendorID, dbType: DbType.Int32);

            string insertSQL = SQLHelper.GetInsertCommand<BetRecordTransactionNoMappingEntity>(TableName);

            using (var conn = InstanceSQLConn())
            {
                var resultId = conn.Execute(insertSQL, param);
            }
        }

        public BetRecordTransactionNoMappingBriefModel GetByTransactionNoPGS(string transactionNoPGS)
        {
            string sql = $@"SELECT * FROM {TableName} WITH (NOLOCK)
                            WHERE BetRecordNoPGS = @BetRecordNoPGS AND PartitionTime = @PartitionTime";

            var parseResult = RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(transactionNoPGS);

            var transactionNoParam = ToVarchar(transactionNoPGS, PK_Text_Length.Value);

            var para = new
            {
                BetRecordNoPGS = transactionNoParam,   
                PartitionTime = parseResult.Time
            };

            var query = QueryByDapper<BetRecordTransactionNoMappingBriefModel>(sql, para);
            return query.FirstOrDefault();
        }       

    }


}
