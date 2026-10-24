using Dapper;
using game.Entity;
using Lck.Utility.Extensions.Helper;
using MongoDB.Driver.Core.Configuration;
using target.Core.Repository;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Extensions.Web;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;

namespace target.DA.Repository
{
    public class BetRecordRepository : FormatStringPrimaryKeyRepository<BetRecordEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.GameLogDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[BetRecord]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "BetRecordNoPGS"; }
        }

        protected override int? PK_Text_Length
        {
            get
            {
                return 32;
            }
        }


        public ServerBetSummaryResultModel GetBetSummaryByServer(int serverID, DateTime begin, DateTime end)
        {
            string sql = @"SELECT sum(BetAmount) as TotalBetAmount , sum(ValidAmount) as TotalValidAmount , sum(WinLossAmount) as TotalWinLoss
                           FROM  [dbo].[BetRecord] WITH (NOLOCK)
                           WHERE Status = 1 AND CreateTime >= @begin AND CreateTime < @end AND ServerID = @serverID ";

            var param = new
            {
                serverID = serverID,
                begin = begin,
                end = end
            };

            var query = QueryByDapper<ServerBetSummaryResultModel>(sql, param);
            return query.FirstOrDefault();
        }


        /// <summary>
        /// 取得狀態pending 且 被標記特定tag的注單
        /// </summary>
        public List<BetRecordSimpleQueryModel> GetPendingAndExprectCancelRecords(GameVendorTypeEnum vendor, DateTime begin, DateTime end)
        {
            var tableName = GameVendorDefinitionHelper.GetBetRecordTableName(vendor);
            var dbCategory = GetDbCategoryByGameVendor(vendor);

            string timeColumnName = GameVendorDefinitionHelper.IsSupportPartitionTable(vendor) ? "PartitionTime" : "CreateTime";

            string sql = $@"SELECT BetRecordNoPGS from {dbCategory}.{tableName} as record with (NOLOCK) 
                            inner join [dbo].[BetRecordRawWagerTagMapping] as tag with (NOLOCK) on record.BetRecordNo = tag.BetRecordNo and tag.TagID = @tagID
                            WHERE status = 0 AND record.{timeColumnName} >= @begin AND record.{timeColumnName} < @end ";

            var param = new
            {
                begin = begin,
                end = end,
                tagID = SystemTagEnum.BetRecord_Cancel_ButNotFound.ToInteger()
            };

            var query = QueryByDapper<BetRecordSimpleQueryModel>(sql, param);
            return query.ToList();
        }


        public BetRecordEntity GetRecordByGameVendorAndPgsRecordNo(int gameVendor, string betRecordNoPGS)
        {
            string tableName = GameVendorDefinitionHelper.GetBetRecordTableName(gameVendor);

            var param = new QueryByTransactionNoAndPartitionTime
            {
                BetRecordNoPGS = betRecordNoPGS
            };

            string extraWhere = CheckExtraWhereAndSetPartitionTime(gameVendor, param);
            string sql = $" SELECT * FROM {tableName} WITH (NOLOCK) where BetRecordNoPGS = @BetRecordNoPGS {extraWhere} ";
            var query = QueryByDapper<BetRecordEntity>(sql, param);
            return query.FirstOrDefault();
        }


        public void ChangeSettleAmount(BetRecordChangeSettleInfoParameter model)
        {
            model.RuleFor(z => z.BetRecordNoPGS).Rule_NotEmptyText();

            string tableName = GameVendorDefinitionHelper.GetBetRecordTableName(model.GameVendorType);
            var hasTax = GameVendorDefinitionHelper.HasTaxAmount(model.GameVendorType);

            string taxAmountSql = hasTax ? " TaxAmount = @TaxAmount , " : "";
            string wherePartitionTime = CheckExtraWhereAndSetPartitionTime(model.GameVendorType, model);

            var sql = @$" UPDATE [PGS_GameLogRaw].{tableName} SET 
                                {taxAmountSql}
                                PayoutAmount = @PayoutAmount ,
                                ValidAmount = @ValidAmount ,
                                WinLossAmount = @WinLossAmount 
                          WHERE BetRecordNoPGS = @BetRecordNoPGS {wherePartitionTime} ";

            ExecuteSqlByDapper(sql, model);
        }

        public List<string> GetPendingBetRecords(int gameVendorID, DateTime begin, DateTime end)
        {
            string tableName = GameVendorDefinitionHelper.GetBetRecordTableName(gameVendorID);
            var dbCategory = GetDbCategoryByGameVendor((GameVendorTypeEnum)gameVendorID);

            string whereTime = " AND (CreateTime >= @Begin AND CreateTime < @End) ";

            if (GameVendorDefinitionHelper.IsSupportPartitionTable(gameVendorID))
            {
                whereTime = " AND (PartitionTime >= @Begin AND PartitionTime < @End) ";
            }

            string final_sql = @$" SELECT BetRecordNoPGS FROM {dbCategory}.{tableName} WITH (NOLOCK)  
                                   WHERE Status = @Status {whereTime} ";

            var param = new
            {
                Status = BetRecordStatusEnum.Pending.ToInteger(),
                Begin = begin,
                End = end
            };

            var query = QueryByDapper<UnionBetRecordQueryModel>(final_sql, param);
            return query.ToList().Select(x => x.BetRecordNoPGS).ToList();
        }


        public PagedList<UnionBetRecordQueryModel> GetUnionBetRecordsByPagingFilter(QueryUnionBetRecordRequestViewModel filter)
        {
            filter.OrderByColumn = " CreateTime ";

            if (filter.GameVendorID.HasValue &&
               GameVendorDefinitionHelper.IsSupportPartitionTable(filter.GameVendorID.Value))
            {
                filter.OrderByColumn = " PartitionTime desc , CreateTime desc";
            }

            if (filter.RoundID.IsNotEmptyTextNotNull() && filter.GameVendorID.HasValue)
            {
                CheckRoundIDColumnExist(filter.GameVendorID.Value, filter.RoundID);
            }

            //根據 gameVendor篩選條件 產生union查詢語法
            string unionSQL = GenerateAllUnionSQLs(filter);

            string final_sql = $" SELECT * FROM ({unionSQL}) as u WHERE 1=1 ";

            if (filter.TransactionNoPGS.IsNotEmptyTextNotNull())
            {
                final_sql += " AND BetRecordNoPGS = @TransactionNoPGS ";
            }

            if (filter.ListOfTransactionNoPGS.IsNotEmptyNull())
                final_sql += " AND BetRecordNoPGS in @ListOfTransactionNoPGS ";

            if (filter.BetRecordNo.IsNotEmptyTextNotNull())
                final_sql += " AND BetRecordNo = @BetRecordNo ";

            if (filter.Status.HasValue)
                final_sql += " AND Status = @Status ";

            if (filter.PlayerIDs.IsNotEmptyNull())
                final_sql += " AND PlayerID in @PlayerIDs ";

            if (filter.MerchantIDs.IsNotEmptyNull())
                final_sql += " AND MerchantID in @MerchantIDs ";

            if (filter.GameID.HasValue)
                final_sql += " AND GameID = @GameID ";

            if (filter.PartitionTimeMatch.HasValue)
                final_sql += " AND PartitionTime = @PartitionTimeMatch ";

            if (filter.PartitionTimeBegin.HasValue)
                final_sql += " AND PartitionTime >= @PartitionTimeBegin ";

            if (filter.PartitionTimeEnd.HasValue)
                final_sql += " AND PartitionTime < @PartitionTimeEnd ";

            if (filter.CreateTimeBegin.HasValue)
                final_sql += " AND CreateTime >= @CreateTimeBegin ";

            if (filter.CreateTimeEnd.HasValue)
                final_sql += " AND CreateTime < @CreateTimeEnd ";

            if (filter.RoundID.IsNotEmptyTextNotNull())
                final_sql += " AND RoundID = @RoundID ";

            if (filter.TagID.HasValue)
            {
                string extraWhereOfTime = "";

                if (filter.PartitionTimeBegin.HasValue)
                {
                    extraWhereOfTime = " AND CreateTime >= @PartitionTimeBegin ";
                }
                else if (filter.CreateTimeBegin.HasValue)
                {
                    extraWhereOfTime = " AND CreateTime >= @CreateTimeBegin ";
                }

                final_sql += $@" AND [BetRecordNoPGS] in ( 
                                    SELECT [BetRecordNoPGS] from [dbo].[BetRecordTagMapping] with (NOLOCK) 
                                    WHERE TagID = @TagID {extraWhereOfTime}
                                ) ";
            }

            var query = GetPagingEntitiesBySQLScript<UnionBetRecordQueryModel>(final_sql, filter, filter);
            return query;
        }

        private string GetDbCategoryByGameVendor(GameVendorTypeEnum gameVendor)
        {
            var dbCategory = "[PGS_GameLogRaw]";

            if (gameVendor == GameVendorTypeEnum.target ||
                gameVendor == GameVendorTypeEnum.AGGLive)
            {
                dbCategory = "[PGS_GameLog]";
            }

            return dbCategory;
        }

        private string GenerateAllUnionSQLs(QueryUnionBetRecordRequestViewModel filter)
        {
            string commonExtraSQL = "  0 as type , ";

            if (filter.RoundID.IsNotEmptyTextNotNull() || filter.GameVendorID == (int)GameVendorTypeEnum.Galaxy)
            {
                commonExtraSQL += " RoundID ,";
            }

            List<string> allSQLs = new List<string>();

            //效能考量 強制要求需篩選遊戲商
            if (!filter.GameVendorID.HasValue)
            {
                throw new TerproException(ApiErrorCode.BusinessLogicDeny, new ErrorDetailModel("GameVendorID"));
            }

            if (filter.GameVendorID.Value == (int)GameVendorTypeEnum.target)
            {
                //只有PGS的select語法比較特殊
                string pgsSelectSQL = GenerateUnionQuerySQL(GameVendorTypeEnum.target, " Type as type , ");
                allSQLs = pgsSelectSQL.ToListCollection();
            }
            else
            {
                if (GameVendorDefinitionHelper.IsSupportPartitionTable(filter.GameVendorID.Value))
                {
                    allSQLs = GenerateUnionQuerySQL((GameVendorTypeEnum)filter.GameVendorID.Value, commonExtraSQL + " PartitionTime , ").ToListCollection();
                }
                else
                {
                    allSQLs = GenerateUnionQuerySQL((GameVendorTypeEnum)filter.GameVendorID.Value, commonExtraSQL).ToListCollection();
                }
            }

            //不再使用union查詢的方法了
            //string unionSQL = string.Join(" UNION ", allSQLs); 

            return allSQLs.FirstOrDefault();
        }


        private string GenerateUnionQuerySQL(GameVendorTypeEnum gameVendor, string extraColumns)
        {
            string dbName = GameVendorDefinitionHelper.GetDbConnType(gameVendor) == "GameLogDB" ? "[PGS_GameLog]" : "[PGS_GameLogRaw]";
            string tableName = GameVendorDefinitionHelper.GetBetRecordTableName(gameVendor);

            string extraColumn = GameVendorDefinitionHelper.HasTaxAmount(gameVendor) ? " , TaxAmount " : "";

            string thirdPartyExtra = " , VendorConfigID ";

            if (gameVendor == GameVendorTypeEnum.target || gameVendor == GameVendorTypeEnum.Slot88)
            {
                thirdPartyExtra = string.Empty;
            }

            string common_column = $@"
                        BetRecordNo , BetRecordNoPGS , MerchantID, GameID, PlayerID, PlayerName, BetAmount, 
                        WinLossAmount , PayoutAmount , Status, CreateTime, SettlementTime {thirdPartyExtra}  {extraColumn} ";

            string resultSQL = @$" SELECT {(int)gameVendor} as GameVendor , {extraColumns}  {common_column} FROM {dbName}.{tableName} WITH (NOLOCK) ";
            return resultSQL;
        }


        public void UpdateStatus(string betRecordNoPGS, BetRecordStatusEnum status)
        {
            string sql = $@"UPDATE {TableName} SET 
                                Status = @status , 
                                UpdateTime = @updateTime
                            WHERE BetRecordNoPGS = @betRecordNoPGS";

            var para = new
            {
                status = status.ToInteger(),
                updateTime = DateTimeHelper.GetCurrentTime(),
                betRecordNoPGS = betRecordNoPGS,
            };

            ExecuteSqlByDapper(sql, para);
        }


        public BetRecordEntity GetByTransactionNo(string betRecordNoPGS)
        {
            var query = GetByFilter(new QueryBetRecordRequestViewModel
            {
                BetRecordNoPGS = betRecordNoPGS
            });

            return query.FirstOrDefault();
        }


        /// <summary>
        /// 邏輯大致上搜尋超過30~60分鐘前的注單。並檢查是否缺少perGame
        /// 後續要補救
        /// </summary>
        /// <returns></returns>
        public List<BetRecordEntity> CheckAvatarMissPerGame()
        {
            string sql = @"
    SELECT BetRecordNoPGS 
	FROM [PGS_GameLog].[dbo].[BetRecord] as record WITH(NOLOCK)
	left join 
	(
		SELECT * FROM [PGS_GameLog].[dbo].[PerGameDetailLog] AS per with (NOLOCK)
		WHERE per.GameTime >= DATEADD( hour , -8 , @begin) AND per.GameTime <  DATEADD( hour , 8 , @end) AND MerchantID = @merchantID  AND GameID = 2233 
	) as per on record.BetRecordNoPGS = per.TransactionNo 
	WHERE record.Status = 1 AND record.CreateTime >= @begin AND record.CreateTime <  @end AND per.Id is null AND record.BetAmount > 0 AND record.MerchantID = @merchantID  AND record.GameID = 2233  
";

            var begin = DateTimeHelper.GetCurrentTime().AddMinutes(-1 * 90);
            //var begin = new DateTime(2026, 6, 9, 21, 37, 0).AddHours(-8);

            var param = new
            {
                begin = begin,
                end = begin.AddMinutes(35),
                merchantID = 176130
            };

            var query = QueryByDapper<BetRecordEntity>(sql, param);
            return query;
        }


        public List<BetRecordEntity> GetByFilter(QueryBetRecordRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<BetRecordEntity>(sql, filter);
            return query;
        }


        public PagedList<BetRecordEntity> GetPagingByFilter(QueryBetRecordRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<BetRecordEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryBetRecordRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} WITH (NOLOCK) 
                           WHERE 1=1 ";

            if (filter.BetRecordNoPGS.IsNotEmptyTextNotNull())
                sql += " AND BetRecordNoPGS = @BetRecordNoPGS ";

            return sql;
        }


        private string CheckExtraWhereAndSetPartitionTime<T>(int gameVendor, T param) where T : IQueryByTransactionNoAndPartitionTime
        {
            string extraWhere = "";

            //如果有支援partitionTime的話
            if (GameVendorDefinitionHelper.IsSupportPartitionTable(gameVendor))
            {
                var recover = RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(param.BetRecordNoPGS);
                extraWhere = " AND PartitionTime = @PartitionTime ";
                param.PartitionTime = recover.Time;
            }

            return extraWhere;
        }

        private void CheckRoundIDColumnExist(int gameVendorID, string roundID)
        {
            string tableName = GameVendorDefinitionHelper.GetBetRecordTableName(gameVendorID);

            using (var connection = InstanceSQLConn())
            {
                connection.Open();

                // SQL 查詢 COL_LENGTH
                string sql = $"SELECT COL_LENGTH('{tableName}', 'RoundID')";

                // 執行查詢並取得結果
                int? colLength = connection.QuerySingleOrDefault<int?>(sql);

                if (!colLength.HasValue || colLength <= 0)
                {
                    throw new TerproException(ApiErrorCode.Forbidden_NotSupportThis, new ErrorDetailModel($"{tableName} 資料表沒有 RoundID"));
                }
            }
        }

    }


}
