using Dapper;
using game.Entity;
using System.Linq;
using target.Core.Repository;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using System;
using System.Collections.Generic;
using target.Utility.Extensions;
using target.Models.Enums;
using Lck.Utility.Extensions.Helper;
using target.Utility.Helper;
using target.Models.Models;

namespace target.DA.Repository
{

    public class MerchantRepository : FormatIntegerPrimaryKeyRepository<MerchantEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[Merchant]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "MerchantID"; }
        }


        public override int Create(MerchantEntity model)
        {
            model.RuleFor(x => x.Currency).Rule_NotEmptyText();
            model.RuleFor(x => x.ChargeCurrency).Rule_NotEmptyText();

            return base.Create(model);
        }


        public void CheckStatusEnableOrThrowException<T>(T query) where T: IMerchantStatusInfo
        {
            if (query == null)
                throw new TerproException(ApiErrorCode.NotFoundMerchant);

            if (query.Status != NormalActiveStatus.Enable.ToInteger())
                throw new TerproException(ApiErrorCode.InvalidStatus);
        }

        public MerchantBriefEntity GetByTokenAndSecret(int merchantID, string secretKey)
        {
            var filter = new QueryMerchantRequestViewModel
            {
                ApiSecretKey = secretKey,
                MerchantIDs = merchantID.ToListCollection()
            };

            var query = GetBriefByFilter(filter);
            return query.FirstOrDefault();
        }

        public MerchantEntity GetSingleByCache(int merchantID)
        {
            string cacheKey = CacheKey.AdminSys_MerchantInfo(merchantID);

            var queryCachae = CacheHelper.GetCacheOrQuery(cacheKey, () =>
               {
                   var querySingle = GetById(merchantID);
                   return querySingle;

               }, 60);

            return queryCachae;
        }

        public MerchantBriefEntity GetByPlayerID(int playerID)
        {
            var query = GetBriefByFilter(new QueryMerchantRequestViewModel
            {
                PlayerID = playerID
            });

            return query.FirstOrDefault();
        }

        public MerchantBriefEntity GetBriefById(int merchantID)
        {
            var query = GetBriefByFilter(new QueryMerchantRequestViewModel
            {
                MerchantIDs = merchantID.ToListCollection()
            });

            return query.FirstOrDefault();
        }

        public Dictionary<int , MerchantBriefEntity> GetBriefDicByIDs(List<int> merchantIDs)
        {
            var query = GetBriefByFilter(new QueryMerchantRequestViewModel
            {
                MerchantIDs = merchantIDs
            });

            return merchantIDs.ToDictionary(x => x, x=> query.FirstOrDefault(z => z.MerchantID == x) ?? new MerchantBriefEntity());
        }


        public List<MerchantBriefEntity> GetBriefByFilter(QueryMerchantRequestViewModel filter)
        {
            string selectColumns = " MerchantID , Name , Currency , Status , DefaultLanguage , ApiSecretKey , WalletMode , ShortName ";

            var query = GetByFilter(filter, selectColumns);

            return query
                .Select(x => x.ReturnInjecteObject<MerchantBriefEntity>())
                .ToList();
        }

      
        public List<MerchantEntity> GetByFilter(QueryMerchantRequestViewModel filter, string selectColumns = "*")
        {
            var sqlGen = GenerateWhereSQL(filter, selectColumns);

            var config = filter.DbQueryExtraConfig ?? new DbQueryConfig();
            config.TempIDsAppendSQL = sqlGen.TempIDsAppendSQL;

            var query = QueryByDapper<MerchantEntity>(sqlGen.WhereSQL, filter , config);
            return query;
        }


        public PagedList<MerchantEntity> GetPagingByFilter(QueryMerchantRequestViewModel filter)
        {
            var sqlGen = GenerateWhereSQL(filter);
            var query = GetPagingEntitiesBySQLScript<MerchantEntity>(sqlGen.WhereSQL, filter, filter);
            return query;
        }

        private GenerateWhereSQLResult GenerateWhereSQL(QueryMerchantRequestViewModel filter, string columns = "*")
        {
            var sqlGen = new GenerateWhereSQLResult();

            string sql = $@"SELECT {columns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            //*這個的順序要在merchantIDs之前
            if (filter.HierarchyOfTargetMerchant.HasValue)
            {
                filter.MerchantIDs = AppHelper.SetListToEmptyWhenLargeFilter(filter.MerchantIDs);
                sql += AppHelper.GenerateMerchantHierarchyWhereSQL(filter.HierarchyOfTargetMerchant.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.ApiSecretKey))
                sql += " AND ApiSecretKey = @ApiSecretKey ";

            if (!string.IsNullOrWhiteSpace(filter.Currency))
                sql += " AND (Currency = @Currency OR ChargeCurrency = @Currency) ";

            if (filter.CurrencyList.IsNotEmptyNull())
                sql += " AND (Currency in @CurrencyList ) ";

            if (!string.IsNullOrWhiteSpace(filter.MerchantGuid))
            {
                if(!filter.MerchantGuid.IsGuid())
                {
                    throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel("Guid"));
                }

                sql += " AND MerchantGuid = @MerchantGuid ";
            }

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                filter.Keyword = filter.Keyword.Trim();
                sql += $" AND (" +
                    $"{GenerateLikeQuery("Name", "@Keyword")} OR " +
                    $"{GenerateLikeQuery("Memo", "@Keyword")} OR " +                    
                    $"{GenerateLikeQuery("cast(MerchantGuid as nvarchar(50))", "@Keyword")} OR " +
                    $"{GenerateLikeQuery("cast(MerchantID as nvarchar(10))", "@Keyword")} OR " +
                    $"{GenerateLikeQuery("Currency", "@Keyword")} " +
                    ")";
            }

            if (filter.MerchantIDs.IsNotEmptyNull())
            {
                var whereInColumns = GenerateInQuery_Or_LargeIDsQuery("@MerchantIDs", filter.MerchantIDs);

                sql += $" AND MerchantID in {whereInColumns.WhereSQL} ";

                sqlGen.TempIDsAppendSQL = whereInColumns.TempIDsAppendSQL;
            }

            if (filter.FilterMerchantIDsByMultipleHierarchyMerchant.IsNotEmptyNull())
            {
                sql += $" {AppHelper.GenerateMultipleHierarchyMerchantFilterWhereSQL_V2026("MerchantID")}  ";
            }               

            if (!filter.IsIncludeDeleteData)
                sql += $" AND Status != {NormalActiveStatus.PermanentDelete.ToInteger()} ";

            if(!string.IsNullOrWhiteSpace(filter.NameMatch))
                sql += $" AND Name = @NameMatch ";

            if (filter.Status.HasValue)
                sql += $" AND Status = @Status ";

            if (filter.WalletMode.HasValue)
                sql += $" AND WalletMode = @WalletMode ";

            if (filter.IsExcludeLayer3Agents)
                sql += $" AND MerchantID in ( SELECT MerchantID from [dbo].[MerchantHierarchy] WITH (NOLOCK) where LEN(Hierarchy) <= 15 ) ";

            if (filter.IsSearchLayer2AgentOnly)
                sql += $" AND MerchantID in ( SELECT MerchantID from [dbo].[MerchantHierarchy] WITH (NOLOCK) where LEN(Hierarchy) >= 8 AND LEN(Hierarchy) <= 15 ) ";
           
            if (filter.IsSearchLayer1AgentOnly)
                sql += $" AND MerchantID in ( SELECT MerchantID from [dbo].[MerchantHierarchy] WITH (NOLOCK) where ParentMerchantID = 0 ) ";

            if (filter.IsRootAgentOnly)
                sql += $" AND MerchantID in ( SELECT RootMerchantID from [dbo].[MerchantHierarchy] WITH (NOLOCK)  ) ";

            if (filter.IsNotAgent)
                sql += $" AND MerchantID not in ( SELECT MerchantID from [dbo].[MerchantHierarchy] WITH (NOLOCK)  ) ";

            if (filter.EnableSpecificGameID.HasValue)
            {
                sql += @$" AND MerchantID in ( 
                                SELECT MerchantID from [dbo].[MerchantGameSetting] WITH (NOLOCK)
                                WHERE gameID = @EnableSpecificGameID AND status = 1
                          ) ";
            }

            if (filter.EnableSpecificGameIDs.IsNotEmptyNull())
            {
                sql += @$" AND MerchantID in ( 
                                SELECT MerchantID from [dbo].[MerchantGameSetting] WITH (NOLOCK)
                                WHERE gameID in @EnableSpecificGameIDs AND status = 1
                          ) ";
            }

            if (filter.IsFilterHierarchyMerchantOnly)
            {
                sql += @$" AND MerchantID in ( 
                            SELECT MerchantID FROM [dbo].[MerchantHierarchy] WITH (NOLOCK)
                          ) ";
            }

            if (filter.ParentMerchantID.HasValue)
            {
                sql += @$" AND MerchantID in ( 
                            SELECT MerchantID FROM [dbo].[MerchantHierarchy] WITH (NOLOCK)
                            WHERE [ParentMerchantID] = @ParentMerchantID  
                          ) ";
            }

            if (filter.RootMerchantID.HasValue)
            {
                sql += @$" AND MerchantID in ( 
                            SELECT MerchantID FROM [dbo].[MerchantHierarchy] WITH (NOLOCK)
                            WHERE [RootMerchantID] = @RootMerchantID  
                          ) ";
            }

            if (filter.MatchGeneralTagAndIMChannel.HasValue)
            {
                int settingKey = MerchantCustomizedSettingKeyEnum.Common_IMChannelID.ToInteger();
                sql += @$" AND MerchantID in 
                          (          
                            SELECT MerchantID FROM MerchantCustomizedSetting 
                            WHERE CustomizedSettingKey = {settingKey} AND SettingValue in
                            (
	                            select TargetID from [dbo].[GeneralTagMapping]
	                            where TagId = {GeneralTagEnum.DoNotEnableJackpotGame.ToInteger()}
                            )
                          ) ";
            }


            if (filter.PlayerID.HasValue)
                sql += $" AND MerchantID in (SELECT MerchantID FROM [dbo].[Player] WITH (NOLOCK) WHERE playerID = @playerID) ";

            if (filter.Type.HasValue)
                sql += $" AND Type = @Type ";

            if (filter.OrganizationIDs.IsNotEmptyNull())
                sql += $" AND MerchantID in (SELECT MerchantID FROM [PGS_Operation].[dbo].[OrganizationMerchantMapping] WHERE organizationID in @OrganizationIDs) ";

            if (filter.MerchantCustomizedSetting_SettingKey.HasValue)
            {
                sql += @$" AND MerchantID in (  
                            SELECT MerchantID from [dbo].[MerchantCustomizedSetting] with (NOLOCK)
                            WHERE CustomizedSettingKey = @MerchantCustomizedSetting_SettingKey AND SettingValue = @MerchantCustomizedSetting_SettingValue   
                        ) ";
            }

            if(filter.CreateTimeBegin.HasValue)
            {
                sql += $" AND CreateTime >= @CreateTimeBegin ";
            }

            if (filter.CreateTimeEnd.HasValue)
            {
                sql += $" AND CreateTime < @CreateTimeEnd ";
            }

            if(filter.FilterGeneralTagID.HasValue)
            {
                sql += @" AND MerchantID in 
                          ( 
                             SELECT TargetID 
                             FROM [dbo].[GeneralTagMapping] WITH (NOLOCK)
                             WHERE TagID = @FilterGeneralTagID 
                          )";
            }

            sqlGen.WhereSQL = sql;
            return sqlGen;
        }

    }


}
