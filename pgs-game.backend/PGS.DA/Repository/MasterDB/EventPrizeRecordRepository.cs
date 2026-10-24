using Dapper;
using DocumentFormat.OpenXml.Office2010.Excel;
using game.Entity;
using Lck.Utility.Extensions.Helper;
using target.Core.Helper;
using target.Core.Repository;
using target.Models;
using target.Models.Attributes;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.Utility.Extensions.Web;
using target.Utility.Helper;
using target.Utility.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.DA.Repository
{
    public class EventPrizeRecordRepository : FormatLongPrimaryKeyRepository<EventPrizeRecordEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[EventPrizeRecord]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }


        public long Create(EventPrizeRecordEntity model , EventPrizeRecordDetailEntity detailModel)
        {
            long resultID = 0;

            using (var scope = AppHelper.InstanceTransactionScopeByLevel(System.Transactions.IsolationLevel.ReadCommitted))
            {
                //Step 1. 主表
                resultID = Create(model);

                //Step 2. 特定活動會寫detail表
                if(detailModel != null)
                {
                    detailModel.EventPrizeRecordId = resultID;

                    string insertSQL = SQLHelper.GetInsertCommand<EventPrizeRecordDetailEntity>("EventPrizeRecordDetail");

                    using (var conn = InstanceSQLConn())
                    {
                        var resultId = conn.Query<long?>(insertSQL, detailModel);
                    }
                }

                scope.Complete();
            }

            return resultID;
        }



        public SQLUpdateRowCountResult UpdateStatusToReceiveSuccess(long id, bool isThrowException = true)
        {
            //為了確保資源競爭的狀況 where條件更嚴格
            string sql = $@" UPDATE {TableName} SET Status = @newStatus , ReceiveTime = getutcdate()
                             WHERE Id = @Id AND Status = @mustMatchStatus ";

            var para = new
            {
                mustMatchStatus = EventPrizeRecordStatusEnum.AllocateSuccessNotReceive.ToInteger(),
                newStatus = EventPrizeRecordStatusEnum.AllocateSuccessReceiveSuccess.ToInteger(),
                Id = id
            };

            var updateResult = UpdateAndSelectRowCount(sql, para);

            if (updateResult.UpdateRowCount <= 0 && isThrowException)
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_DatabaseBusyAndUpdateFail);

            return updateResult;
        }


        public SQLUpdateRowCountResult UpdateStatusToUsage(long id , bool isThrowException = true)
        {
            //為了確保資源競爭的狀況 where條件更嚴格
            string sql = $@" UPDATE {TableName} SET Status = @newStatus 
                             WHERE Id = @Id AND Status = @mustMatchStatus ";

            var para = new
            {
                mustMatchStatus = EventPrizeRecordStatusEnum.AllocateSuccess.ToInteger(),
                newStatus = EventPrizeRecordStatusEnum.Usage.ToInteger(),
                Id = id
            };

            var updateResult = UpdateAndSelectRowCount(sql, para);

            if (updateResult.UpdateRowCount <= 0 && isThrowException)
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_DatabaseBusyAndUpdateFail);

            return updateResult;
        }


        /// <summary>
        /// 單元測試使用
        /// </summary>
        public void UpdateWithSqlCommandForMSTest(string setValueSQL , string whereSQL)
        {
            string sql = @$"UPDATE {TableName} {setValueSQL}
                            {whereSQL} ";

            ExecuteSqlByDapper(sql, null);
        }

        public bool IsEventFinish(int playerID, int eventID)
        {
            string sql = @$"SELECT count(1) from {TableName} WITH (NOLOCK)
                            WHERE  PlayerID = @playerID AND eventID = @eventID AND EventPrizeType = @eventPrizeType";

            var para = new
            {
                playerID = playerID,
                eventID = eventID,
                eventPrizeType = EventPrizeTypeEnum.EventFinish.ToInteger()
            };

            bool exists = QueryByDapper<bool>(sql, para).FirstOrDefault();
            return exists;
        }

        public List<EachMerchantAGGEventPrizeTotalSummaryModel> GetEachMerchantEventPrizeTotalSumByDetailTable(EachMerchantEventPrizeTotalSummaryQueryParameter model)
        {
            model.RuleFor(x => x.EventPrizeTypes).Rule_IsNotEmptyCollection();

            string extraWhere = "  ";

            if(model.MerchantID.HasValue)
            {
                extraWhere = " AND prize.MerchantID = @MerchantID ";
            }

            if(model.FilterMerchantIDsByMultipleHierarchyMerchant.IsNotEmptyNull())
            {
                extraWhere = AppHelper.GenerateMultipleHierarchyMerchantFilterWhereSQL_V2026("prize.MerchantID");
            }

            if (model.GameID.HasValue)
            {
                extraWhere = " AND prize.GameID = @GameID ";
            }

            string selectColumn = " SELECT prize.EventPrizeType, MerchantID , GameID, count(*) as TotalCount, sum(ISNULL(d.ReferValidAmount , 0)) as TotalValidAmount , count(distinct prize.PlayerID) as TotalPlayers , sum(prize.UpdateAmount) as winLossAmount ";

            string sql1 = @"
FROM [PGS_Master].[dbo].[EventPrizeRecord] as prize WITH (NOLOCK)
LEFT JOIN [PGS_Master].[dbo].[EventPrizeRecordDetail] as d on d.EventPrizeRecordId = prize.Id";

//            string sql2 = @"
//FROM [PGS_Master].[dbo].[EventPrizeRecordDetail] as d WITH (NOLOCK)
//INNER JOIN [dbo].[EventPrizeRecord] as prize on d.EventPrizeRecordId = prize.Id";

            string preSQL = selectColumn + sql1;

            //if(model.EventPrizeTypes.Count() == 1 &&   == (int)EventPrizeTypeEnum.AGG_Freespin)
            //{
            //    preSQL = selectColumn + sql2;
            //}

            string sql = preSQL + $@"
            WHERE prize.EventPrizeType in @EventPrizeTypes AND prize.ReceiveTime >= @BeginUTC AND prize.ReceiveTime <= @EndUTC {extraWhere} AND MerchantID in 
            (
                SELECT MerchantID FROM [ThirdParty].[VendorConfigMerchantMapping] WITH (NOLOCK)
                WHERE VendorConfigID = @VendorConfigID
            )
            GROUP BY prize.EventPrizeType, MerchantID , GameID 
            ORDER BY  MerchantID , prize.EventPrizeType , GameID ";

            var query = QueryByDapper<EachMerchantAGGEventPrizeTotalSummaryModel>(sql, model).ToList();
            return query;
        }


        /// <summary>
        /// 這個method只提供給帳單系統使用 會有個特殊邏輯
        /// 超過5個月的話 會改查詢另一張表
        /// </summary>
        public List<EachMerchantEventPrizeTotalSummaryModel> GetEachMerchantEventPrizeTotalSum(List<int> merchantIDs , DateTime beginUTC , DateTime endUTC)
        {
            string whereMerchantID = " AND MerchantID in @merchantIDs ";

            List<int> paramMerchantIDs = merchantIDs ?? new List<int>();

            if (paramMerchantIDs.Count() > 1000)
            {
                paramMerchantIDs = new List<int>();
            }

            if(paramMerchantIDs.IsEmptyOrNull())
            {
                whereMerchantID = string.Empty;
            }

            string sql = $@"SELECT * , e.EventType , ISNULL(e.DisplayNameExternal , e.DisplayName) as EventName FROM
                            (	
	                            SELECT MerchantID , EventID , GameID , EventPrizeType , sum(UpdateAmount) as UpdateAmount
	                            FROM [dbo].[EventPrizeRecord] WITH (NOLOCK)
	                            WHERE ReceiveTime >= @beginUTC AND ReceiveTime < @endUTC {whereMerchantID}
	                            GROUP BY MerchantID , EventID , GameID , EventPrizeType
                            ) as main
                            LEFT JOIN  [dbo].[Event] as e WITH (NOLOCK) on main.EventID = e.EventID ";

            if(beginUTC <= DateTimeHelper.GetCurrentTime().AddMonths(-1 * 5))
            {
                sql = $@"SELECT * , e.EventType , ISNULL(e.DisplayNameExternal , e.DisplayName) as EventName FROM
                            (	
	                             SELECT MerchantID , EventID , 0 as GameID, EventPrizeType , sum(TotalAmount) as UpdateAmount
                                 FROM [PGS_StatReport].[dbo].[MerchantEventSummaryData] WITH (NOLOCK)
                                 WHERE StatTime >= @beginUTC AND StatTime < @endUTC {whereMerchantID}
                                 GROUP BY MerchantID , EventID,  EventPrizeType
                            ) as main
                            LEFT JOIN  [dbo].[Event] as e WITH (NOLOCK) on main.EventID = e.EventID ";
            }

            var para = new
            {
                merchantIDs = paramMerchantIDs,
                beginUTC = beginUTC,
                endUTC = endUTC,
            };

            var query = QueryByDapper<EachMerchantEventPrizeTotalSummaryModel>(sql, para).ToList();

            //程式碼篩選 會比較快
            if(paramMerchantIDs.IsEmptyOrNull() && merchantIDs.IsNotEmptyNull())
            {
                query = query
                    .Where(x => merchantIDs.Contains(x.MerchantID))
                    .ToList();
            }

            //補上gameVendorID資訊
            foreach(var item in query)
            {
                int gameVendor = 0;

                if (item.GameID > 0)
                {
                    gameVendor = AppHelper.GetGameVendorByGameID(item.GameID);
                }
                else
                {
                    var myEnum = (EventPrizeTypeEnum)item.EventPrizeType;

                    var attr = myEnum.GetAttributeFromEnum<ExtraIntValue>();

                    if (attr == null)
                    {
                        gameVendor = AppHelper.GetGameVendorByGameID(item.GameID);
                    }
                    else
                    {
                        gameVendor = attr.Value;
                    }
                }

                item.ReferGameVendorID = gameVendor;
            }

            return query;
        }

        public bool IsPrizeRecordsExistByCache(int playerID, int eventPrizeBundleID, int gameID)
        {
            string cacheKey = CacheKey.Events_PrizeRecord_GameID_ExistCheck(playerID , eventPrizeBundleID , gameID);

            CacheQueryConfig config = new CacheQueryConfig
            {
                ExpiredTime = new TimeSpan(0, 5, 0),
                SetCacheOnlyWhenValueEqual = "true",
            };

            //只有true的情況才會儲存cache
            var queryCache = CacheHelper.GetCacheOrQuery(cacheKey, () =>
               {
                   bool queryExist = IsPrizeIdRecordsExist(playerID, eventPrizeBundleID, gameID);
                   return queryExist ? "true" : "false";

               }, config);

            return queryCache == "true";
        }


        public bool IsPrizeRecordsExistByCache(int playerID, int eventPrizeBundleID, int gameID , DateTime targetDateOfTW)
        {
            var dateText = targetDateOfTW.ToString("yyyyMMdd");

            string cacheKey = CacheKey.Events_PrizeRecord_GameID_ExistCheck(playerID, eventPrizeBundleID, gameID , dateText);

            DateTime begin = targetDateOfTW.AddHours(-1 * SystemConstant.DailyEventBasicTimezone);
            DateTime end = begin.AddDays(1);

            CacheQueryConfig config = new CacheQueryConfig
            {
                ExpiredTime = new TimeSpan(0, 5, 0),
                SetCacheOnlyWhenValueEqual = "true",
            };

            //只有true的情況才會儲存cache
            var queryCache = CacheHelper.GetCacheOrQuery(cacheKey, () =>
            {
                bool queryExist = IsPrizeIdRecordsExist(playerID, eventPrizeBundleID, gameID , begin , end);
                return queryExist ? "true" : "false";

            }, config);

            return queryCache == "true";
        }


        public bool IsPrizeIdRecordsExist(int playerID, int eventPrizeBundleID , int? gameID , DateTime? begin = null , DateTime? end = null)
        {
            //若gmaeID=0 不篩選where、此邏輯請勿修改
            //若gmaeID=0 不篩選where、此邏輯請勿修改
            //若gmaeID=0 不篩選where、此邏輯請勿修改
            string extraWhere = gameID.HasValue && gameID > 0 ? " AND GameID = @gameID " : "";

            string extraWhereTimeFilter = begin.HasValue ? " AND ( ReceiveTime >= @begin AND ReceiveTime < @end) " : "";

            string sql = @$"SELECT count(1) from {TableName} WITH (NOLOCK)
                            WHERE  PlayerID = @playerID AND EventPrizeBundleID = @eventPrizeBundleID {extraWhere} {extraWhereTimeFilter} ";

            var para = new
            {
                playerID = playerID,
                eventPrizeBundleID = eventPrizeBundleID,
                gameID = gameID,
                begin = begin,
                end = end,
            };

            bool exists = QueryByDapper<bool>(sql, para).FirstOrDefault();
            return exists;
        }

        public int SelectCountByFilter(QueryEventPrizeRecordRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter, " count(*) ");
            var query = QueryByDapper<int>(sql, filter);
            return query.FirstOrDefault();
        }

        public List<QueryPlayerEventSummaryModel> GetPlayerEventSummary(int playerID , DateTime begin , DateTime end)
        {
            string sql = @"select ISNULL(e.EventID , 0) as EventID , e.DisplayName as EventName, sub.totalCount , sub.totalAmount from
                            (
                                select EventID, count(*) as totalCount, sum(UpdateAmount) as totalAmount
                                from [PGS_Master].[dbo].[EventPrizeRecord] with(NOLOCK)
                                where PlayerID = @playerID AND ReceiveTime >= @begin AND ReceiveTime < @end
                                group by EventID
                            ) as sub
                            left join [PGS_Master].[dbo].[Event] as e on sub.EventID = e.EventID";

            var param = new
            {
                playerID = playerID,
                begin = begin,
                end = end,
            };

            var query = QueryByDapper<QueryPlayerEventSummaryModel>(sql, param);
            return query;
        }

        public EventPrizeRecordDetailQuery GetJoinDetailInfoByID(long myEventPrizeID)
        {
            string sql = @"
SELECT TOP 1 prize.* , detail.ReferValidAmount , detail.ReferThirdPartyEventID , e.DisplayName as EventDisplayName
FROM [dbo].[EventPrizeRecord] as prize WITH (NOLOCK)
LEFT JOIN [dbo].[EventPrizeRecordDetail] as detail on prize.Id = detail.EventPrizeRecordId
LEFT JOIN [dbo].[Event] as e on prize.EventID = e.EventID
WHERE Id = @eventPrizeID";

            var param = new
            {
                eventPrizeID = myEventPrizeID
            };

            var query = QueryByDapper<EventPrizeRecordDetailQuery>(sql, param);
            return query.FirstOrDefault();
        }


        public List<EventPrizeRecordEntity> GetByFilter(QueryEventPrizeRecordRequestViewModel filter , string selecetColumns = "prize.*")
        {
            string sql = GenerateWhereSQL(filter , selecetColumns , filter.PageSize);
            var query = QueryByDapper<EventPrizeRecordEntity>(sql, filter);
            return query;
        }


        public PagedList<EventPrizeRecordDetailQuery> GetPagingByFilter(QueryEventPrizeRecordRequestViewModel filter, string selecetColumns = "prize.*")
        {
            string sql = GenerateWhereSQL(filter , selecetColumns);
            var query = GetPagingEntitiesBySQLScript<EventPrizeRecordDetailQuery>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryEventPrizeRecordRequestViewModel filter , string selecetColumns = "prize.*", int? topQuery = null)
        {
            string topQuerySQL = GenerateTopSizeQuerySQL(topQuery);
            string noLock = filter.IsLockQuery ? "" : " WITH (NOLOCK) ";

            string tableSQL = $@"SELECT {topQuerySQL} {selecetColumns} FROM {TableName} as prize {noLock} ";

            if(filter.IsJoinDetailData)
            {
                tableSQL = $@" SELECT {topQuerySQL} {selecetColumns} , detail.ReferValidAmount , detail.ReferThirdPartyEventID , e.DisplayName as EventDisplayName
                               FROM {TableName} as prize {noLock} 
                               LEFT JOIN [dbo].[EventPrizeRecordDetail] as detail on prize.Id = detail.EventPrizeRecordId
                               LEFT JOIN [dbo].[Event] as e on prize.EventID = e.EventID";
            }

            string sql = tableSQL + " WHERE 1=1 ";

            if (filter.EventIDs.IsNotEmptyNull())
                sql += " AND prize.EventID in @EventIDs ";

            if (filter.EventPrizeBundleIDs.IsNotEmptyNull())
                sql += " AND prize.EventPrizeBundleID in @EventPrizeBundleIDs ";

            if (filter.PlayerIDs.IsNotEmptyNull())
                sql += " AND prize.PlayerID in @PlayerIDs ";

            if (filter.PrizeTypes.IsNotEmptyNull())
                sql += " AND prize.EventPrizeType in @PrizeTypes ";

            if (filter.FilterMerchantIDsByMultipleHierarchyMerchant.IsNotEmptyNull())
            {
                filter.MerchantIDs = AppHelper.SetListToEmptyWhenLargeFilter(filter.MerchantIDs);
                sql += AppHelper.GenerateMultipleHierarchyMerchantFilterWhereSQL("prize.MerchantID");
            }

            if (filter.MerchantIDs.IsNotEmptyNull())
                sql += " AND prize.MerchantID in @MerchantIDs ";

            if (filter.GameIDs.IsNotEmptyNull())
                sql += " AND prize.GameID in @GameIDs ";

            if (filter.VendorConfigID.HasValue)
            {
                sql += @$" AND prize.MerchantID in 
                    (
                        SELECT MerchantID FROM [ThirdParty].[VendorConfigMerchantMapping] WITH (NOLOCK)
                        WHERE VendorConfigID = @VendorConfigID
                    )
                ";
            }                

            if (filter.CreateTimeBegin.HasValue)
                sql += " AND prize.CreateTime >= @CreateTimeBegin ";

            if (filter.CreateTimeEnd.HasValue)
                sql += " AND prize.CreateTime < @CreateTimeEnd ";

            if (filter.ReceiveTimeBegin.HasValue)
                sql += " AND prize.ReceiveTime >= @ReceiveTimeBegin ";

            if (filter.ReceiveTimeEnd.HasValue)
                sql += " AND prize.ReceiveTime < @ReceiveTimeEnd ";

            if (filter.Status.HasValue)
                sql += " AND prize.Status = @Status ";

            if (filter.ThirdOrderNo.IsNotEmptyTextNotNull())
                sql += " AND prize.ThirdOrderNo = @ThirdOrderNo ";

            if (filter.ReferenceNo.IsNotEmptyTextNotNull())
            {
                sql += " AND ( prize.ReferenceNo = @ReferenceNo ) ";
            }

            if (filter.FreespinID.IsNotEmptyTextNotNull())
            {
                sql += " AND prize.Id in ( SELECT [EventPrizeRecordId] FROM [dbo].[EventPrizeRecordDetail] WITH (NOLOCK) WHERE [ReferThirdPartyEventID] = @FreespinID ) ";
            }

            if (filter.MultipleOrderColumnSearch.IsNotEmptyTextNotNull())
            {
                //AGG單號 有時候會有其他前綴
                if (filter.MultipleOrderColumnSearch.ContainsWihtoutCase("OceanGame") || filter.MultipleOrderColumnSearch.ContainsWihtoutCase("GameItem"))
                {
                    sql += $@" AND ( 
                            {GenerateLikeQuery("prize.ThirdOrderNo", "@MultipleOrderColumnSearch")} 
                        ) ";
                }
                else
                {
                    sql += @" AND ( 
                            prize.ReferenceNo = @MultipleOrderColumnSearch OR  
                            prize.PgsTransactionNo = @MultipleOrderColumnSearch  OR  
                            prize.ThirdOrderNo = @MultipleOrderColumnSearch 
                        ) ";

                }
            }

            return sql;
        }

    }


}
