using Dapper;
using game.Entity;
using Lck.Utility.Extensions.Helper;
using target.Core.Repository;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Utility.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.DA.Repository
{
    /// <summary>
    /// 此Table曾經歷經一次table調整轉型 (所以會有一些看似奇怪的型別轉換寫法)
    /// 未來可能會調整成獨立table (目前是偷懶用 OneToManySetting 這張表)
    /// </summary>
    public class IMChannelRepository : FormatIntegerPrimaryKeyRepository<IMChannelEntity> 
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[IMChannel]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "IMChannelID" +
                    ""; }
        }

        public override bool Update(IMChannelEntity model)
        {
            model.UpdateTime = DateTimeHelper.GetCurrentTime();

            return base.Update(model);
        }


        /// <summary>
        /// 有 單一/多組IDs 兩種提供呼叫
        /// </summary>
        public MerchantIMChannelQueryModel GetMerchantIMChannel(int merchantID)
        {
            var query = GetMerchantIMChannel(merchantID.ToListCollection());
            return query.FirstOrDefault();
        }


        /// <summary>
        /// 有 單一/多組IDs 兩種提供呼叫
        /// </summary>
        public List<MerchantIMChannelQueryModel> GetMerchantIMChannel(List<int> merchantIDs, bool isReturnEmptyWhenPassEmptyIDs = true)
        {
            if (merchantIDs.IsEmptyOrNull())
            {
                if (isReturnEmptyWhenPassEmptyIDs)
                {
                    return new List<MerchantIMChannelQueryModel>();
                }
            }

            bool isLargeListQuery = merchantIDs.CountOrZero() >= 1000;

            string filterMerchantIDWhere = " AND m.MerchantID in @merchantIDs ";

            if (merchantIDs.IsEmptyOrNull() || isLargeListQuery)
            {
                filterMerchantIDWhere = "";
            }

            int key1 = MerchantCustomizedSettingKeyEnum.Common_IMChannelID.ToInteger();

            string sql = $@"SELECT m.MerchantID , merchant.Name as MerchantName, merchant.Type as MerchantType, merchant.WalletMode as MerchantWalletMode , imChannel.IMChannelID , imChannel.DisplayName as IMChannelName
                            FROM [PGS_Master].[dbo].[MerchantCustomizedSetting] as m WITH (NOLOCK)
                            LEFT JOIN [dbo].[Merchant] as merchant WITH (NOLOCK) on m.MerchantID = merchant.MerchantID
                            INNER JOIN
                            (
	                            SELECT IMChannelID , DisplayName 
	                            FROM [dbo].[IMChannel] WITH (NOLOCK)	                            
                            ) as imChannel on m.SettingValue = cast(imChannel.IMChannelID  as nvarchar(10))
                            WHERE m.CustomizedSettingKey = {key1} {filterMerchantIDWhere} ";

            var param = new
            {
                merchantIDs = merchantIDs,
            };

            var query = QueryByDapper<MerchantIMChannelQueryModel>(sql, param).ToList();

            foreach (var item in query)
            {
                if (item.IMChannelID.IsNumeric())
                {
                    item.IMChannelID_Integer = Int32.Parse(item.IMChannelID);
                }
            }

            if(isLargeListQuery)
            {
                query = query.Where(x => merchantIDs.Contains(x.MerchantID)).ToList();
            }

            return query;
        }

        public IMChannelEntity GetByMerchantID(int merchantID)
        {
            var query = GetByFilter(new QueryIMChannelRequestViewModel
            {
                AssignMerchantIDs = merchantID.ToListCollection(),
            });

            return query.FirstOrDefault();
        }

        public IMChannelEntity GetByIMChannelID(int imChannelID)
        {
            var query = GetByFilter(new QueryIMChannelRequestViewModel
            {
                IMChannelIDs = imChannelID.ToListCollection()
            });

            return query.FirstOrDefault();
        }

        public List<IMChannelEntity> GetByIMChannelIDs(List<int> imChannelIDs)
        {
            if (imChannelIDs.IsEmptyOrNull())
                return new List<IMChannelEntity>();

            var query = GetByFilter(new QueryIMChannelRequestViewModel
                {
                    IMChannelIDs = imChannelIDs
                })
                .ToList();

            return query;
        }


        public IMChannelEntity GetByChatID(string chatID, IMChannelTypeEnum type)
        {
            var query = GetByFilter(new QueryIMChannelRequestViewModel
            {
                ThirdPartyChatID = chatID,
                IMChannelTypes = type.ToInteger().ToListCollection(),
            });
           
            return query.FirstOrDefault();
        }

        public List<IMChannelEntity> GetIMChannels()
        {
            var query = GetByFilter(new QueryIMChannelRequestViewModel
            {

            });

            return query;
        }

        public List<IMChannelEntity> GetByFilter(QueryIMChannelRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<IMChannelEntity>(sql, filter);
            return query;
        }



        public PagedList<IMChannelEntity> GetPagingByFilter(QueryIMChannelRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<IMChannelEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryIMChannelRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.IMChannelIDs.IsNotEmptyNull())
                sql += " AND IMChannelID in @IMChannelIDs ";

            if (filter.IMChannelTypes.IsNotEmptyNull())
                sql += " AND Type in @IMChannelTypes ";

            if (filter.Keyword.IsNotEmptyTextNotNull())
            {
                sql += $" AND (" +
                             $"{GenerateLikeQuery("DisplayName", "@Keyword")} OR " +
                             $"{GenerateLikeQuery("Memo", "@Keyword")} OR " +
                             $"{GenerateLikeQuery("ThirdPartyChatID", "@Keyword")} OR " +
                             $"{GenerateLikeQuery("IMChannelID", "@Keyword")} " +
                             ")";
            }

        
            if (filter.ThirdPartyChatID.IsNotEmptyTextNotNull())
                sql += $@" AND ThirdPartyChatID = @ThirdPartyChatID ";

            if (filter.TagID.HasValue)
            {
                sql += $@" AND IMChannelID in 
                (
                    SELECT TargetID FROM [dbo].[GeneralTagMapping] WITH (NOLOCK)
                    WHERE TagID = @TagID
                ) ";
            }

            if (filter.IsAssignProductionMerchantOnly)
            {
                sql += $@" AND IMChannelID in 
                (
                    SELECT SettingValue FROM [dbo].[MerchantCustomizedSetting] WITH (NOLOCK)
                    WHERE LEN(SettingValue) > 3 AND CustomizedSettingKey = 3088 AND MerchantID in
                    (
	                    SELECT MerchantID FROM [dbo].[Merchant] WITH (NOLOCK)
	                    WHERE Type = 20 AND Status = 1
                    )
                ) ";
            }

            if (filter.NotAssignTagID.HasValue)
            {
                sql += $@" AND IMChannelID not in 
                (
                    SELECT TargetID FROM [dbo].[GeneralTagMapping] WITH (NOLOCK)
                    WHERE TagID = @NotAssignTagID
                ) ";
            }

            if (filter.FilterMerchantIDsByMultipleHierarchyMerchant.IsNotEmptyNull())
            {
                string subQuery = AppHelper.GenerateMultipleHierarchyMerchantFilterWhereSQL();

                sql += $@" AND IMChannelID in 
                (
                    SELECT SettingValue FROM [dbo].[MerchantCustomizedSetting] WITH (NOLOCK)
                    WHERE LEN(SettingValue) > 3 AND CustomizedSettingKey = 3088 {subQuery}
                ) ";
            }

            if (filter.AssignMerchantIDs.IsNotEmptyNull())
            {
                sql += $@" AND IMChannelID in 
                (
                    SELECT SettingValue FROM [dbo].[MerchantCustomizedSetting] WITH (NOLOCK)
                    WHERE LEN(SettingValue) > 3 AND CustomizedSettingKey = 3088 AND MerchantID in @AssignMerchantIDs
                ) ";
            }

            if (!filter.IsIncludeDeleteData)
            {
                sql += $@" AND Status != {NormalActiveStatus.PermanentDelete.ToInteger()} ";
            }

            return sql;
        }

    }


}
