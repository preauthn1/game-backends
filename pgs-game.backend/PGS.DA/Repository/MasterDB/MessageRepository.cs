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
    public class MessageRepository : FormatLongPrimaryKeyRepository<MessageEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[Message]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "MessageID"; }
        }

        public int GetPlayerUnreadMessageCount(PlayerEntity queryPlayer)
        {
            string sql = GeneratePlayerMessageQuerySql(queryPlayer, new QueryUnreadMessageParameter
            {
                IsQueryTotalCount = true,
                IsQueryUnreadOnly = true
            });

            var param = new
            {
                PlayerID = queryPlayer.PlayerID,
                MerchantID = queryPlayer.MerchantID,
                RegisterTime = queryPlayer.CreateTime
            };

            var query = QueryByDapper<int>(sql, param);
            return query.FirstOrDefault();
        }


        public List<PlayerMessageExtraInfoModel> GetPlayerMessages(PlayerEntity queryPlayer, int size)
        {
            string sql = GeneratePlayerMessageQuerySql(queryPlayer , new QueryUnreadMessageParameter
            {
                PageSize = size,
                IsQueryUnreadOnly = false
            });

            var param = new
            {
                PlayerID = queryPlayer.PlayerID,
                MerchantID = queryPlayer.MerchantID,
                RegisterTime = queryPlayer.CreateTime
            };

            var query = QueryByDapper<PlayerMessageExtraInfoModel>(sql, param);
            return query;
        }

        public string GeneratePlayerMessageQuerySql(PlayerEntity queryPlayer, QueryUnreadMessageParameter filter)
        {
            filter.PageSize = filter.PageSize <= 0 ? 20 : filter.PageSize;
            string queryColumns = filter.IsQueryTotalCount ? " count(*) " : $" TOP {filter.PageSize} m.* , CASE WHEN mr.MessageID is NULL THEN 0 ELSE 1 END as IsRead ";
            string extraWhere = filter.IsQueryUnreadOnly ? " AND mr.MessageID is NULL " : "";

            string sql = $@"SELECT {queryColumns} FROM [dbo].[Message] AS m WITH (NOLOCK)
                            LEFT JOIN [dbo].[MessageRead] AS mr WITH (NOLOCK) on m.MessageID = mr.MessageID AND mr.PlayerID = @PlayerID
                            WHERE m.Status = 1 AND
                            (
                                m.MessageID in
                                (
	                                SELECT MessageID FROM [dbo].[MessageAssign] WITH (NOLOCK)
	                                WHERE AssignType = {MessageAssignTypeEnum.SpecificPlayerOnly.ToInteger()} AND PlayerID = @PlayerID
                                )
                                OR
                                m.MessageID in
                                (
	                                SELECT MessageID FROM [dbo].[MessageAssign] WITH (NOLOCK)
	                                WHERE AssignType = {MessageAssignTypeEnum.MerchantAll.ToInteger()} AND MerchantID = @MerchantID
                                )
                                OR
                                (
	                                m.CreateTime <= @RegisterTime AND m.MessageID in
	                                (
		                                SELECT MessageID FROM [dbo].[MessageAssign] WITH (NOLOCK)
		                                WHERE AssignType = {MessageAssignTypeEnum.MerchantAllExcludeNewPlayer.ToInteger()} AND MerchantID = @MerchantID
	                                )
                                )
                            ) {extraWhere}
                            order by m.MessageID desc";

            return sql;
        }


        public List<MessageEntity> GetByFilter(QueryMessageRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<MessageEntity>(sql, filter);
            return query;
        }


        public PagedList<MessageEntity> GetPagingByFilter(QueryMessageRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<MessageEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryMessageRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.IDs.IsNotEmptyNull())
                sql += " AND XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX in @IDs ";

            return sql;
        }

    }


}
