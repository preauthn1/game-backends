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
    /// 專門清除MSTest用
    /// 如果有CreateTime 建議刪除48小時前以上的即可
    /// </summary>
    public class MSTestDataDeleteRepository : FormatIntegerPrimaryKeyRepository<AccountingAdjustmentRuleGroupAssignEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[XXXXXXXXXXXXXXXXX]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }

        public void DeleteIPTemplate()
        {
            string sql = $@"DELETE FROM IPAddressListTemplate
                            WHERE {GenerateLikeQuery("DisplayName", "@keyword")} AND CreateTime <= DATEADD(hour , -48 , getutcdate())";

            var param = new
            {
                keyword = "Hook_Sykpe_New_202"
            };

            ExecuteSqlByDapper(sql, param);
        }

    }


}
