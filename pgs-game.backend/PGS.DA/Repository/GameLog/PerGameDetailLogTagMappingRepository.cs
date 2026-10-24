using Dapper;
using game.Entity;
using Lck.Utility.Extensions.Helper;
using target.Core.Repository;
using target.Models.Enums;
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
    public class PerGameDetailLogTagMappingRepository : FormatMultiplePrimaryKeyRepository<PerGameDetailLogTagMappingEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.GameLogDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[PerGameDetailLogTagMapping]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "PerGameDetailLogID"; }
        }

        protected override bool IsConvertToDbValueType
        {
            get
            {
                return true;
            }
        }

        public PerGameDetailLogTagMappingEntity InstanceInsertEntity(PerGameDetailLogEntity perGame, SystemTagEnum tag)
        {
            return InstanceInsertEntity(perGame, tag.ToInteger());
        }

        public PerGameDetailLogTagMappingEntity InstanceInsertEntity(PerGameDetailLogEntity perGame , int tagID)
        {
            return new PerGameDetailLogTagMappingEntity
            {
                GameTime = perGame.GameTime,
                PerGameDetailLogID = perGame.Id,
                TransactionNo = perGame.TransactionNo,
                TagID = tagID,
                VendorConfigID = perGame.ServerID,
            };
        }


        public override void Create(PerGameDetailLogTagMappingEntity model)
        {
            model.RuleFor(x => x.GameTime).Rule_NotNullDateTime();

            base.Create(model);
        }


        public List<PerGameDetailLogTagMappingEntity> GetByFilter(QueryPerGameDetailLogTagMappingRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<PerGameDetailLogTagMappingEntity>(sql, filter);
            return query;
        }


        public PagedList<PerGameDetailLogTagMappingEntity> GetPagingByFilter(QueryPerGameDetailLogTagMappingRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<PerGameDetailLogTagMappingEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryPerGameDetailLogTagMappingRequestViewModel filter)
        {
            string noNock = filter.IsLockQuery ? "" : " WITH (NOLOCK) ";
            string sql = $@"SELECT * FROM {TableName} {noNock}
                           WHERE 1=1 ";

            if (filter.PerGameDetailLogIDs.IsNotEmptyNull())
                sql += " AND PerGameDetailLogID in @PerGameDetailLogIDs ";

            if (filter.TagIDs.IsNotEmptyNull())
                sql += " AND TagID in @TagIDs ";

            if (filter.ListOfTransactionNo.IsNotEmptyNull())
                sql += " AND TransactionNo in @ListOfTransactionNo ";

            if (filter.GameTimeBegin.HasValue)
                sql += " AND GameTime >= @GameTimeBegin ";

            if (filter.GameTimeEnd.HasValue)
                sql += " AND GameTime < @GameTimeEnd ";

            return sql;
        }

    }


}
