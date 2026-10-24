using Dapper;
using game.Entity;
using target.Core.Repository;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using System;
using System.Collections.Generic;
using target.Utility.Extensions;
using System.Linq;
using target.Models.Enums;
using target.Models.Models;
using target.Utility;
using target.Utility.Extensions.Web;
using target.Utility.Helper;
using target.Utility.Utility.ThirdParty;
using target.Core.Helper;

namespace target.DA.Repository
{

    public class PerGameDetailLogRepository : FormatLongPrimaryKeyRepository<PerGameDetailLogEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.GameLogDB); }
        }

        protected override string ConnectionStringRepli
        {
            get { return GetConnectionStringByDbName(DBConnType.GameLogDB, isReplicationDB: true); }
        }

        protected override string TableName
        {
            get { return "[dbo].[PerGameDetailLog]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }

        protected override string DefaultSortColumn
        {
            get
            {
                return "GameTime";  //用partition key排序會比PK Id快
            }
        }


        public long CreateTransactionScope(PerGameDetailLogEntity entity)
        {
            var result = CreateByTransactionScope(new List<PerGameDetailLogEntity> { entity });
            return result.FirstOrDefault().Id;
        }


        public List<PerGameDetailLogEntity> CreateTransactionScope(List<PerGameDetailLogEntity> listOfEntity)
        {
            var listResult = CreateByTransactionScope(listOfEntity);
            return listResult;
        }



        public class PerGameBatchInsertModel
        {
            public PerGameDetailLogEntity PerGameDetailLogEntity { get; set; }

            public PerGameDetailLogMerchantJoinEntity PerGameDetailLogMerchantJoinEntity { get; set; }

            public DynamicParameters InsertDynamicParameters { get; set; }
        }



        private List<PerGameDetailLogEntity> CreateByTransactionScope(List<PerGameDetailLogEntity> perGameEntityItems)
        {
            string insertSQL1 = SQLHelper.GetInsertCommand<PerGameDetailLogEntity>(TableName);
            string insertSQL2 = SQLHelper.GetInsertCommand<PerGameDetailLogMerchantJoinEntity>("PerGameDetailLogMerchantJoin");
            insertSQL1 += " SELECT CAST(SCOPE_IDENTITY() as bigint) ";

            List<PerGameBatchInsertModel> listOfInsert = new List<PerGameBatchInsertModel>();

            foreach (var myEntity in perGameEntityItems)
            {
                listOfInsert.Add(new PerGameBatchInsertModel
                {
                    PerGameDetailLogEntity = myEntity,
                    InsertDynamicParameters = ConvertClassToDapperDynamicParameters(myEntity),
                    PerGameDetailLogMerchantJoinEntity = new PerGameDetailLogMerchantJoinEntity
                    {
                        MerchantID = myEntity.MerchantID,
                        GameTime = myEntity.GameTime,
                    }
                });
            }

            using (var scope = AppHelper.InstanceTransactionScopeByLevel(System.Transactions.IsolationLevel.ReadCommitted))
            {
                foreach (var eachItem in listOfInsert)
                {
                    using (var conn = InstanceSQLConn())
                    {
                        // Step 1. insert主表
                        var resultId = conn.Query<long?>(insertSQL1, eachItem.InsertDynamicParameters);

                        if (resultId.IsEmptyOrNull())
                        {
                            throw new TerproException(ApiErrorCode.BusinessLogicDeny_DatabaseBusyAndUpdateFail);
                        }

                        var myValue = resultId.FirstOrDefault();

                        if (myValue == null)
                        {
                            throw new TerproException(ApiErrorCode.BusinessLogicDeny_DatabaseBusyAndUpdateFail);
                        }

                        eachItem.PerGameDetailLogEntity.Id = myValue.Value;
                        eachItem.PerGameDetailLogMerchantJoinEntity.PerGameDetailLogID = eachItem.PerGameDetailLogEntity.Id;

                        // Step 2. insert JOIN用子表
                        conn.Execute(insertSQL2, eachItem.PerGameDetailLogMerchantJoinEntity);
                    }
                }

                scope.Complete();
            }

            return listOfInsert.Select(x => x.PerGameDetailLogEntity).ToList();
        }


        /// <summary>
        /// 取得遺失perGameLog資料的投注紀錄
        /// </summary>
        public List<BetRecordEntity> GetMissPerGameLogRecords(GameVendorTypeEnum gameVendor, DateTime begin, DateTime end, QueryMissPerGameExtraParameter filter)
        {
            var targetConn = GetConnectionStringByDbName(GameVendorDefinitionHelper.GetDbConnType(gameVendor));
            var tableName = GameVendorDefinitionHelper.GetBetRecordTableName(gameVendor);
            string timeColumns = "third.CreateTime";
            string extraWhere = "";

            if (GameVendorDefinitionHelper.IsSupportPartitionTable(gameVendor))
            {
                timeColumns = "third.PartitionTime";
            }

            //捕魚的0元注單不會寫成perGame資料
            if (gameVendor == GameVendorTypeEnum.AGGFish)
            {
                extraWhere = " AND third.BetAmount > 0 ";
            }

            List<int> excludePlayerIDs = filter.ExcludePlayerIDs;

            if (excludePlayerIDs.IsNotEmptyNull())
            {
                if (excludePlayerIDs.Count() > 200)
                {
                    //list筆數太多的話 可能直接用sql查會更快。
                    excludePlayerIDs = new List<int>();
                    extraWhere = $@" AND third.PlayerID not in 
                                    (
                                        SELECT PlayerID from[PGS_Master].[dbo].[PlayerTagMapping] WITH(NOLOCK)
                                        WHERE TagID = {SystemTagEnum.Player_GameVendorAcoountingIgnoreThisPlayer.ToInteger()}
                                    ) ";
                }
                else
                {
                    extraWhere = " AND third.PlayerID not in @excludePlayerIDs ";
                }
            }

            if (filter.IsExcludeFinanceIgnore)
            {
                int tagID = SystemTagEnum.BetRecord_AccountingIgnore.ToInteger();
                extraWhere += $@" AND third.BetRecordNoPGS not in 
                ( 
                   SELECT  BetRecordNoPGS 
                   FROM [PGS_GameLog].[dbo].[BetRecordTagMapping] WITH (NOLOCK)
                   WHERE TagID = {tagID}   
                ) ";
            }

            string sql = $@"SELECT TOP 50 third.BetRecordNoPGS , third.PlayerID FROM {tableName} as third WITH(NOLOCK)
                            LEFT JOIN 
                            (
	                            SELECT Id , GameTime , TransactionNo 
                                FROM [PGS_GameLog].[dbo].[PerGameDetailLog] AS per with (NOLOCK)
	                            WHERE per.GameTime >= @begin_perGame AND per.GameTime < @end_perGame
                            ) as per
                            on third.BetRecordNoPGS = per.TransactionNo AND third.SettlementTime = per.GameTime
                            WHERE third.Status = 1 AND {timeColumns} >= @begin AND {timeColumns} < @end AND per.Id is null {extraWhere} ";

            var para = new
            {
                begin_perGame = begin.AddDays(-2),
                end_perGame = end.AddDays(1),
                begin = begin,
                end = end,
                excludePlayerIDs = excludePlayerIDs
            };

            using (var conn = InstanceSQLConn(targetConn))
            {
                var query = conn.Query<BetRecordEntity>(sql, para, commandTimeout: 300);
                return query.ToList();
            }
        }



        /// <summary>
        /// 如果判定是pgs單號，會自動篩選時間 (因為有partition 理應這樣查會較快)
        /// </summary>
        public PerGameDetailLogEntity GetByTransactionNo_WithGameTimeRange(string transactionNo)
        {
            var isOrderNo = RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(transactionNo);

            if (isOrderNo.IsPgsOrderNo)
            {
                DateTime targetGameTime = isOrderNo.Time.Value;

                //因為有partition 預期這樣查應該會比較快
                var myFilter = new QueryDetailLogRequestViewModel
                {
                    TransactionNo = transactionNo,
                    GameStartTimeBegin = targetGameTime.AddMinutes(-5),
                    GameStartTimeEnd = targetGameTime.AddHours(2).AddMinutes(5),
                };

                var queryPerGame = GetByFilter(myFilter);

                if (queryPerGame.IsEmptyOrNull())
                {
                    myFilter.GameStartTimeEnd = null;

                    //如果查不到 則去掉endTime再查一次 (可能有延遲結算的問題..但根據這種條件查詢的機率不高)
                    queryPerGame = GetByFilter(myFilter);
                }

                return queryPerGame.FirstOrDefault();
            }
            else
            {
                var queryPerGame = GetByFilter(new QueryDetailLogRequestViewModel
                {
                    TransactionNo = transactionNo,
                });

                return queryPerGame.FirstOrDefault();
            }
        }


        public PerGameDetailLogEntity GetByIdAndGameTime(long id, DateTime settlementTime_gameTime)
        {
            //因為有partition 預期這樣查應該會比較快
            var myFilter = new QueryDetailLogRequestViewModel
            {
                PerGameDetailLogID = id,
                GameTimeMatch = settlementTime_gameTime,
            };

            var queryPerGame = GetByFilter(myFilter);
            return queryPerGame.FirstOrDefault();
        }


        public PerGameDetailLogEntity GetByTransactionNoAndGameTime(string transactionNo, DateTime settlementTime_gameTime)
        {
            //因為有partition 預期這樣查應該會比較快
            var myFilter = new QueryDetailLogRequestViewModel
            {
                TransactionNo = transactionNo,
                GameStartTimeBegin = settlementTime_gameTime.AddMinutes(-1),
                GameStartTimeEnd = settlementTime_gameTime.AddMinutes(1),
            };

            var queryPerGame = GetByFilter(myFilter);
            return queryPerGame.FirstOrDefault();
        }


        public List<PerGameDetailLogEntity> GetRecordsByTagID(SystemTagEnum tagID, int merchantID, DateTime beginTime, DateTime endTime)
        {
            string sql = @" SELECT per.* FROM [dbo].[PerGameDetailLogTagMapping] as tag WITH (NOLOCK)
                            LEFT JOIN [dbo].[PerGameDetailLog] as per on tag.PerGameDetailLogID = per.Id AND tag.GameTime = per.GameTime
                            WHERE tagID = @tagID AND tag.GameTime > @beginTime AND per.MerchantID = @merchantID ";

            var param = new
            {
                tagID = tagID.ToInteger(),
                merchantID = merchantID,
                beginTime = beginTime,
                endTime = endTime,
            };

            var query = QueryByDapper<PerGameDetailLogEntity>(sql, param);
            return query.ToList();
        }



        /// <summary>
        /// 臨時更改 perGame的派彩金額
        /// </summary>
        public void ChangePerGameSettleAmount(long perGameDetailID, PerGameChangeSettleInfoParameter model)
        {
            Update(perGameDetailID, model);

            SendNotify(perGameDetailID);
        }


        public void UpdateAndReplaceMerchantIDPlayerID(PerGameDeleteProcess_UpdateMerchantPlayerParameter param)
        {
            param.RuleFor(x => x.Id).Rule_NotZeroNotMinus();
            param.RuleFor(x => x.GameTime).Rule_NotEmptyDateTime();
            param.RuleFor(x => x.NewPlayerID).Rule_NotZeroNotMinus();
            param.RuleFor(x => x.NewMerchantID).Rule_NotZeroNotMinus();

            string sql = $@" UPDATE {TableName} SET 
                                MerchantID = @NewMerchantID ,
                                PlayerID = @NewPlayerID ,
                                PlayerName = @NewPlayerName 
                             WHERE Id = @Id AND GameTime = @GameTime ";

            ExecuteSqlByDapper(sql, param);

            SendNotify(param.Id);
        }



        private void SendNotify(long id)
        {
            string msg = $"perGameDetail: {id} 有觸發update行為 是否會影響sync資料";
            MyTelegramUtility.SendMessageToChannelAsync(new TelegramMessageParameter
            {
                Message = msg,
                TelegramChannelEnum = TelegramChannelEnum.pgs_externalAPI,
                TelegramExtraSetting = new TelegramExtraSetting
                {
                    IsSkipWhenMSTest = true,
                }
            });
        }

        //public void UpdateAmountAfterSettle(long perGameDetailID, DateTime gameTime, decimal amount)
        //{
        //    string sql = $@" UPDATE {TableName} SET AmountAfterSettle = @AmountAfterSettle 
        //                     WHERE Id = @Id AND GameTime = @GameTime ";

        //    var param = new
        //    {
        //        AmountAfterSettle = amount,
        //        Id = perGameDetailID,
        //        GameTime = gameTime,  //因為partition的因素 如果沒傳GameTime 效能速度會被嚴重拖累
        //    };

        //    ExecuteSqlByDapper(sql, param);
        //}

        public int GetPlayersCount(DateTime begin, DateTime end)
        {
            string sql = $@"SELECT count(distinct PlayerID) from {TableName} with (NOLOCK)
                           WHERE GameTime >= @begin AND GameTime < @end ";

            var param = new
            {
                begin = begin,
                end = end,
            };

            var query = QueryByDapper<int>(sql, param);
            return query.FirstOrDefault();
        }

        public List<PerGameEachMerchantOnlinePlayerCountQueryModel> GetEachMerchantPlayersCount(DateTime begin, DateTime end, bool isProductionMerchantOnly = true)
        {
            //禁止時間拉太長
            if ((end - begin).TotalMinutes > 10)
            {
                throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel("時間不得拉太長、速度會很慢"));
            }

            string extraWhere = isProductionMerchantOnly ? $" AND m.Type = {MerchantTypeEnum.Plaform_ProductionEnvironment.ToInteger()} " : "";
            string sql = $@"SELECT sub.MerchantID , m.Name , m.Currency , sub.PlayerCount FROM
                            (	
	                            SELECT MerchantID , count(distinct PlayerID) as PlayerCount 
	                            FROM [dbo].[PerGameDetailLog] with (NOLOCK)
	                            WHERE GameTime >= @begin AND GameTime < @end 
	                            GROUP BY MerchantID
                            ) as sub
                            LEFT JOIN [PGS_Master].[dbo].[Merchant] as m WITH (NOLOCK) on sub.MerchantID = m.MerchantID
                            WHERE 1=1 {extraWhere}
                            ORDER BY sub.PlayerCount DESC ";

            var param = new
            {
                begin = begin,
                end = end,
            };

            var query = QueryByDapper<PerGameEachMerchantOnlinePlayerCountQueryModel>(sql, param);
            return query.ToList();
        }


        /// <summary>
        /// 常常會有不明原因timeout
        /// 可能...
        /// </summary>
        /// <returns></returns>
        public List<PerGameDetailLogEntity> GetPerGameBySystemTag_V2(SystemTagEnum tag, DateTime begin, DateTime end, int vendorConfigID)
        {

            string sqlTag = @" SELECT PerGameDetailLogID , GameTime from [PGS_GameLog].[dbo].[PerGameDetailLogTagMapping] as main with (NOLOCK) 
                               WHERE main.TagID = @tagID AND main.GameTime >= @begin AND main.GameTime < @end  AND VendorConfigID = @vendorConfigID ";

            var param_tag = new
            {
                tagID = tag.ToInteger(),
                begin = begin,
                end = end,
                vendorConfigID = vendorConfigID
            };

            var queryTags = QueryByDapper<PerGameDetailLogTagMappingEntity>(sqlTag, param_tag);

            if (queryTags.IsEmptyOrNull())
            {
                return new List<PerGameDetailLogEntity>();
            }

            int unexpectCount = SettingHelper.IsBWG ? 20000 : 10000;

            //筆數異常多。視為資料錯誤 (跳過)
            if (queryTags.CountOrZero() > unexpectCount)
            {
                return new List<PerGameDetailLogEntity>();
            }

            queryTags = queryTags.OrderBy(x => x.GameTime).ToList();

            List<PerGameDetailLogEntity> listResult = new List<PerGameDetailLogEntity>();

            List<List<PerGameDetailLogTagMappingEntity>> splitToSmallSize = new List<List<PerGameDetailLogTagMappingEntity>>
            {
                queryTags
            };

            if(queryTags.CountOrZero() >= 500)
            {
                //先拆成三等份是因為避免 gameTime range太大。查詢會超慢
                splitToSmallSize = queryTags.SplitListBySeperateSize(3);
            }

            foreach (var each_array in splitToSmallSize)
            {
                if (each_array.IsEmptyOrNull())
                    continue;

                //避免超過2100筆
                var sizeSplit_max2000 = each_array.SplitListByArraySize(2000);

                foreach(var each_2000_array in sizeSplit_max2000)
                {
                    //前面的select 只搜尋特定欄位 (請注意*) 效能考量
                    DateTime minTime = each_2000_array.Select(x => x.GameTime).Min().Value.AddMinutes(-1);
                    DateTime maxTime = each_2000_array.Select(x => x.GameTime).Max().Value.AddMinutes(1);

                    List<long> Ids = each_2000_array.Select(x => x.PerGameDetailLogID).ToList();

                    var queryPerGames = GetByFilter(new QueryDetailLogRequestViewModel
                    {
                        GameStartTimeBegin = minTime,
                        GameStartTimeEnd = maxTime,
                        PerGameDetailLogIDs = Ids,
                    })
                    .Where(x => x.ServerID == vendorConfigID)
                    .ToList();

                    listResult.AddRange(queryPerGames);
                }

            }

            return listResult;        
        }

        public List<PerGameLargeExportQueryModel> ExportLargePerGameByVendorConfigID(int vendorConfigID, DateTime begin, DateTime end)
        {
            string sql = @"SELECT ThirdPartyBetRecordNo, BetAmount, UpdateAmount , ThirdSettleDate  
                           FROM [dbo].[PerGameDetailLog] with(NOLOCK)
                           WHERE GameTime >= @begin AND GameTime < @end AND ServerID = @vendorConfigID";

            var param = new
            {
                vendorConfigID = vendorConfigID,
                begin = begin,
                end = end
            };

            int timeoutSeconds = 180;
            var query = QueryByDapper<PerGameLargeExportQueryModel>(sql, param, timeoutSeconds);
            return query;
        }


        public override PerGameDetailLogEntity GetById(long id, bool isLockQuery = false)
        {
            var myFilter = new QueryDetailLogRequestViewModel
            {
                //絕大多數的情境應該都是近日的單。這樣查詢速度會快一點點
                GameStartTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-7),
                GameStartTimeEnd = DateTimeHelper.GetCurrentTime(),
                PerGameDetailLogID = id,
                IsLockQuery = isLockQuery
            };

            var query = GetByFilter(myFilter).FirstOrDefault();

            if (query != null)
                return query;

            myFilter.GameStartTimeBegin = null;
            myFilter.GameStartTimeEnd = null;

            var queryAgain = GetByFilter(myFilter).FirstOrDefault();
            return queryAgain;
        }


        public List<PerGameDetailLogEntity> GetByFilter(QueryDetailLogRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<PerGameDetailLogEntity>(sql, filter, filter.DbQueryExtraConfig);
            return query;
        }


        /// <summary>
        /// 2024/05 改用特製table + 語法查詢。
        /// 貌似執行計畫有差10倍成本 (!?)
        /// </summary>
        public PagedList<PerGameDetailLogEntity> GetPagingByJoinBriefTable(QueryDetailLogRequestViewModel filter, string selectColumns = "*")
        {
            string wherePlayerID = filter.PlayerIDs.IsNotEmptyNull() ? " AND PlayerID in @PlayerIDs " : "";

            string countSQL = $@"SELECT count(*)		                            
		                         FROM [dbo].[PerGameDetailLogMerchantJoin] WITH (NOLOCK)      
		                         WHERE MerchantID in @MerchantIDs 
		                              AND GameTime >= @GameStartTimeBegin
		                              AND GameTime <  @GameStartTimeEnd {wherePlayerID}";


            //按照執行計劃來看 成本會差10倍 希望是真的
            #region 分頁語法(有稍微客製調整)

            filter.PageIndex = filter.PageIndex <= 1 ? 1 : filter.PageIndex;
            filter.PageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;

            int startRowNum = (filter.PageIndex <= 1) ? 1 : 1 + (filter.PageIndex - 1) * filter.PageSize;
            int endRowNum = (startRowNum - 1) + filter.PageSize;

            string pagingSQL = $@"
SELECT join_perGame.* FROM 
(
	SELECT * FROM 
	  (
		SELECT 
		  source_table.*, 
		  (ROW_NUMBER() OVER ( ORDER BY source_table.GameTime desc )) as RowNum 
		FROM [dbo].[PerGameDetailLogMerchantJoin] as source_table WITH (NOLOCK)      
		WHERE 
		  source_table.MerchantID in @MerchantIDs 
		  AND source_table.GameTime >=  @GameStartTimeBegin  
		  AND source_table.GameTime <  @GameStartTimeEnd {wherePlayerID}
	  ) as pageData 
	WHERE RowNum >= @startRowNum AND RowNum <= @endRowNum
) as source_table
LEFT JOIN 
(         
     SELECT * 
     FROM [dbo].[PerGameDetailLog] WITH (NOLOCK) 
     WHERE GameTime >= @GameStartTimeBegin AND GameTime < @GameStartTimeEnd  
) as join_perGame on source_table.PerGameDetailLogID = join_perGame.id AND source_table.GameTime = join_perGame.GameTime";

            #endregion

            var myParam = new
            {
                startRowNum = startRowNum,
                endRowNum = endRowNum,
                MerchantIDs = filter.MerchantIDs,
                GameStartTimeBegin = filter.GameStartTimeBegin,
                GameStartTimeEnd = filter.GameStartTimeEnd,
                PlayerIDs = filter.PlayerIDs,
            };

            var totalCount = GetScalarBySQLScript<int>(countSQL, myParam, filter.DbQueryExtraConfig);

            var myResult = new PagedList<PerGameDetailLogEntity>
            {
                PageIndex = filter.PageIndex,
                PageSize = filter.PageSize,
                TotalCount = totalCount,
                TotalPage = CalculateTotalPage(totalCount, filter.PageSize),
                Items = new List<PerGameDetailLogEntity>(),
            };

            if (totalCount <= 0)
                return myResult;

            myResult.Items = GetEntitiesBySQLScript<PerGameDetailLogEntity>(pagingSQL, myParam, filter.DbQueryExtraConfig);
            return myResult;
        }


        public PagedList<PerGameDetailLogEntity> GetPagingByFilter(QueryDetailLogRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<PerGameDetailLogEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryDetailLogRequestViewModel filter, string selectColumns = "*")
        {
            filter.DbQueryExtraConfig = filter.DbQueryExtraConfig ?? new DbQueryConfig();

            string debugInfo = GenerateDebugSourceInfo(filter.DebugSourceMethod);

            string noLock = filter.IsLockQuery ? "" : " WITH (NOLOCK) ";

            string sql = $@" {debugInfo} 
                           SELECT {selectColumns} FROM {TableName} {noLock}
                           WHERE 1=1 ";


            var splitTime = new DateTime(2024, 4, 30, 16, 0, 0);

            if (filter.GameStartTimeBegin <= splitTime || filter.GameTimeMatch <= splitTime)
            {
                if (SettingHelper.IsProduction || SettingHelper.IsLocalTestProductionDb)
                {
                    filter.DbQueryExtraConfig.IsQueryByOldDataStorageDB = true;
                }
            }

            //前面可能沒處理到...後面這邊還是自動處理比較好
            if (filter.MerchantIDs.IsNotEmptyNull() && filter.MerchantIDs.Count() == 1 && filter.MerchantID == null)
            {
                filter.MerchantID = filter.MerchantIDs.FirstOrDefault();
            }

            //單筆搜尋效能最好
            if (filter.MerchantID.NotNullNotZeroNotMinus())
            {
                if (filter.MerchantID.HasValue)
                {
                    sql += " AND MerchantID = @MerchantID ";
                }
            }
            else
            {
                //筆數少的話 還是允許使用 MerchantIDs in 的查詢
                if (filter.MerchantIDs.IsNotEmptyNull() && filter.MerchantIDs.Count() <= 5)
                {
                    sql += " AND MerchantID in @MerchantIDs ";
                }
                else
                {
                    //有指定hierarchy 優先用這種方式查 (100+筆 IDs in query 速度會很慢)
                    if (filter.FilterMerchantIDByHierarchyMerchant.HasValue)
                    {
                        filter.MerchantIDs = AppHelper.SetListToEmptyWhenLargeFilter(filter.MerchantIDs);
                        sql += AppHelper.GenerateMerchantHierarchyWhereSQL(filter.FilterMerchantIDByHierarchyMerchant.Value);
                    }
                    else if (filter.FilterMerchantIDsByMultipleHierarchyMerchant.IsNotEmptyNull())
                    {
                        filter.MerchantIDs = new List<int>();

                        string sql1 = $" {AppHelper.GenerateMultipleHierarchyMerchantFilterWhereSQL("MerchantID", "")} ";
                        string sql2 = " MerchantID in @FilterMerchantIDsByMultipleHierarchyMerchant ";
                        sql += $" AND ( {sql1} OR {sql2} )";
                    }
                    else if (filter.MerchantIDs.IsNotEmptyNull())
                    {
                        sql += " AND MerchantID in @MerchantIDs ";
                    }
                }
            }

            if (filter.PerGameDetailLogID.HasValue)
                sql += " AND Id = @PerGameDetailLogID ";

            if (filter.PerGameDetailLogIDs.IsNotEmptyNull())
                sql += " AND Id in @PerGameDetailLogIDs ";

            if (filter.PlayerIDs.IsNotEmptyNull())
                sql += " AND PlayerID in @PlayerIDs ";

            if (filter.GameIDs.IsNotEmptyNull())
                sql += " AND GameID in @GameIDs ";

            if (filter.GameVendorID.HasValue)
                sql += " AND GameID in ( SELECT GameID FROM [PGS_Master].[dbo].[GameList] WITH (NOLOCK) WHERE GameVendorType = @GameVendorID ) ";

            if (filter.ServerIDs.IsNotEmptyNull())
                sql += " AND ServerID in @ServerIDs ";

            if (filter.GameStartTimeBegin.HasValue)
                sql += " AND GameTime >= @GameStartTimeBegin ";

            if (filter.GameStartTimeEnd.HasValue)
                sql += " AND GameTime <= @GameStartTimeEnd ";

            if (filter.GameTimeMatch.HasValue)
                sql += " AND GameTime = @GameTimeMatch ";

            if (filter.TransactionNo.IsNotEmptyTextNotNull())
                sql += " AND TransactionNo = @TransactionNo ";

            if (filter.ListOfTransactionNo.IsNotEmptyNull())
                sql += " AND TransactionNo in @ListOfTransactionNo ";

            if (filter.ThirdPartyBetRecordNo.IsNotEmptyTextNotNull())
                sql += " AND ThirdPartyBetRecordNo = @ThirdPartyBetRecordNo ";

            if (filter.TagID.HasValue)
            {
                if (filter.GameStartTimeBegin.HasValue && filter.GameStartTimeEnd.HasValue)
                {
                    sql += @" AND Id in 
                    ( 
                        SELECT PerGameDetailLogID 
                        FROM [dbo].[PerGameDetailLogTagMapping] WITH (NOLOCK) 
                        WHERE TagID = @TagID AND GameTime >= @GameStartTimeBegin AND GameTime < @GameStartTimeEnd
                    ) ";
                }
                else
                {
                    sql += " AND Id in ( SELECT PerGameDetailLogID FROM [dbo].[PerGameDetailLogTagMapping] WITH (NOLOCK) WHERE TagID = @TagID) ";
                }
            }

            if (filter.PayoutAmountLargeThan.HasValue)
                sql += " AND PayoutAmount >= @PayoutAmountLargeThan ";

            if (filter.IsJackpotOnly)
            {
                int tagID = SystemTagEnum.PerGameDetailLog_Jackpot.ToInteger();
                sql += $" AND Id in ( SELECT PerGameDetailLogID FROM [dbo].[PerGameDetailLogTagMapping] WHERE TagID = {tagID} ) ";
            }

            return sql;
        }

    }


}
