using Dapper;
using game.Entity;
using Lck.Utility.Extensions.Helper;
using target.Core.Repository;
using target.Models;
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
using System.Transactions;

namespace target.DA.Repository
{
    public class MerchantHierarchyRepository : FormatIntegerPrimaryKeyRepository<MerchantHierarchyEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[MerchantHierarchy]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "MerchantID"; }
        }


        public void MigrateHierarchy(MerchantHierarchyMigrateParameter param)
        {
            string replace = $" REPLACE(Hierarchy, '{param.KeywordReplaceFrom}','{param.KeywordReplaceTo}') ";

            string whereLikeQuery = GenerateHierarchyPathLikeQuery(param.TargetMerchantID , prefix: "");

            string sql = @$" UPDATE {TableName} 
                                    SET Hierarchy = {replace} ,
                                        RootMerchantID = @NewRootID
                             WHERE {whereLikeQuery} ";

            string sql_2 = @$" UPDATE {TableName} 
                                    SET ParentMerchantID = @NewParentMerchantID
                              WHERE MerchantID = @TargetMerchantID ";

            using (var scope = new TransactionScope())
            {
                ExecuteSqlByDapper(sql, param);
                ExecuteSqlByDapper(sql_2, param);

                scope.Complete();
            }

            return;

        }
      
        public MerchantHierarchyEntity GetSingleByCacheOrEmptyModel(int merchantID)
        {
            string cacheKey = CacheKey.DbEntity_MerchantHierarchy(merchantID);

            var queryCache = CacheHelper.GetCacheOrQuery(cacheKey, () =>
            {
                var q = GetById(merchantID , isLockQuery: false);

                //如果返回null會 導致沒有使用到cache
                if(q == null)
                {
                    return new MerchantHierarchyEntity
                    {
                        MerchantID = 0,
                        RootMerchantID = 0,
                        CreateTime = SystemConstant.EmptyTime1970,
                    };
                }

                return q;

            }, expiredMinute: 15);

            return queryCache;
        }


        public List<int> GetMerchantIDsByFilter(QueryMerchantHierarchyRequestViewModel filter)
        {
            var query = GetByFilter(filter, " MerchantID ");
            return query.Select(z=> z.MerchantID).ToList();
        }


        public List<MerchantHierarchyBriefModel> GetJoinInfoByMerchantIDs(List<int> merchantIDs)
        {
            string extraWhere = " WHERE h.merchantID in @MerchantIDs ";

            bool isLargeAmount = merchantIDs.CountOrZero() > 2000;

            if (merchantIDs.IsEmptyOrNull() || isLargeAmount)
                extraWhere = "";

            string sql = $@"SELECT h.MerchantID , h.ParentMerchantID , h.Hierarchy, m_parent.Name as ParentMerchantName , m_self.Currency , m_self.Name as MerchantName
                             FROM [dbo].[MerchantHierarchy] as h WITH(NOLOCK)
                             LEFT JOIN [dbo].[Merchant] as m_parent WITH(NOLOCK) on h.ParentMerchantID = m_parent.MerchantID
                             LEFT JOIN [dbo].[Merchant] as m_self WITH(NOLOCK) on h.MerchantID = m_self.MerchantID
                           {extraWhere} ";

            var filter = new { MerchantIDs = merchantIDs };
            var query = QueryByDapper<MerchantHierarchyBriefModel>(sql, filter);

            if(isLargeAmount)
            {
                query = query
                    .Where(x => merchantIDs.Contains(x.MerchantID))
                    .ToList();
            }

            return query;
        }

        public int GetCountByFilter(QueryMerchantHierarchyRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter, " count(*) ");
            var query = QueryByDapper<int>(sql, filter);
            return query.FirstOrDefault();
        }


        /// <summary>
        /// 確認某個parentMerchantID 是 可以瀏覽 某個子代理的
        /// </summary>
        public bool IsParentMerchantCanViewMerchant(int parentMerchantID, int targetChildMerchantID)
        {
            if (parentMerchantID == targetChildMerchantID)
                return true;

            var queryChild = GetByFilter(new QueryMerchantHierarchyRequestViewModel
                {
                    MerchantIDs = targetChildMerchantID.ToListCollection(),
                })
                .FirstOrDefault();

            if (queryChild == null)
                return false;

            string parentMerchantIDText = parentMerchantID.ToString();

            if (queryChild.Hierarchy.Contains(parentMerchantIDText))
                return true;

            return false;
        }


        public List<MerchantHierarchyEntity> GetByFilter(QueryMerchantHierarchyRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter , selectColumns);
            var query = QueryByDapper<MerchantHierarchyEntity>(sql, filter);
            return query;
        }


        public PagedList<MerchantHierarchyEntity> GetPagingByFilter(QueryMerchantHierarchyRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);

            var query = GetPagingEntitiesBySQLScript<MerchantHierarchyEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryMerchantHierarchyRequestViewModel filter , string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.MerchantIDs.IsNotEmptyNull())
                sql += " AND MerchantID in @MerchantIDs ";

            if (filter.ParentMerchantID.HasValue)
                sql += " AND ParentMerchantID = @ParentMerchantID ";

            if (filter.RootMerchantID.HasValue)
                sql += " AND RootMerchantID = @RootMerchantID ";

            if (filter.ExcludeMerchantID.HasValue)
                sql += " AND MerchantID != @ExcludeMerchantID ";

            if (filter.MinLayer.HasValue)
            {
                int targetLength = filter.MinLayer.Value * 7; //商戶ID是六位數字 + 斜線符號。共七個字
                sql += $" AND LEN(Hierarchy) >= {targetLength} ";
            }

            if (filter.MaxLayer.HasValue)
            {
                int targetLength = filter.MaxLayer.Value * 7; //商戶ID是六位數字 + 斜線符號。共七個字
                sql += $" AND LEN(Hierarchy) <= {targetLength} ";
            }

            if (filter.ExcludeDeleteMerchants)
            {
                int status = NormalActiveStatus.PermanentDelete.ToInteger();
                sql += $" AND MerchantID not in ( SELECT MerchantID from [dbo].[Merchant] WITH(NOLOCK) WHERE Status = {status}) ";
            }

            if (filter.HierarchyOfTargerMerchant.HasValue)
            {
                sql += GenerateHierarchyPathLikeQuery(filter.HierarchyOfTargerMerchant.Value);
            }

            if (filter.FilterMerchantIDsByMultipleHierarchyMerchant.IsNotEmptyNull())
            {
                sql += AppHelper.GenerateMultipleHierarchyMerchantFilterWhereSQL();
            }

            return sql;
        }

        private string GenerateHierarchyPathLikeQuery(int merchantID , string prefix = " AND ")
        {
            string keyword = GenerateHierarchyPathByMerchantID(merchantID);
            return $" {prefix} Hierarchy LIKE CONCAT('%', '{keyword}' ,'%')  ";
        }

        private string GenerateHierarchyPathByMerchantID(int merchantID)
        {
            return $"{merchantID.ToString()}/"; 
        }

    }


}
