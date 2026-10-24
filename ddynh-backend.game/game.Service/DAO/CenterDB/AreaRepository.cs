using Dapper;
using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity;
using game.Entity.AdminSys;
using game.Entity.Area;
using game.Entity.Game;
using game.Entity.Payment;
using game.Entity.SystemLog;
using game.Infrastructure;
using game.Infrastructure.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using game.Infrastructure.Extensions;
using game.Entity.ViewModels;
using game.Infrastructure.Extensions.Helper;

namespace game.Service.DAO
{
    public class AreaRepository : IDapperRepository<AreaEntity>
    {
        public override string ConnectionString
        {
            get {return GetConnectionStringByDbName(DBNameConfig.DBName_CenterDB); }
        }

        public override string TableName
        {
            get { return "[Area].[Area]"; }
        }

        public override string PrimaryKeyColumnName
        {
            get { return "AreaID"; }
        }
        
        public AreaBriefModel GetSingleByCache(int areaID)
        {
            string cacheKey = $"Area_PlatformID_{areaID}";

            var cacheValue = CacheHelper.GetCacheOrQuery(cacheKey, () =>
               {
                   var query = GetAreasBriefByID(areaID);
                   return query;
               } , expiredMinute: 10);

            return cacheValue;
        }

        /// <summary>
        /// 取得 很簡易的Area資訊欄位 (查詢效率較輕量)
        /// </summary>
        public AreaBriefModel GetAreasBriefByID(int areaID)
        {
            var query = GetAreasBriefByFilter(new QueryAreaRequestViewModel { AreaIDs = areaID.ToListCollection() });
            return query.FirstOrDefault();
        }

        
        /// <summary>
        /// 取得 很簡易的Area資訊欄位 (查詢效率較輕量)
        /// </summary>
        public List<AreaBriefModel> GetAreasBriefByFilter(QueryAreaRequestViewModel filter)
        {
            string sql = $@"SELECT a.AreaID , a.PlatformID , a.AreaName , p.Name as PlatformName
                            FROM [Area].[Area] as a WITH (NOLOCK)
                            LEFT JOIN [Area].[Platform] as p WITH (NOLOCK) on a.PlatformID = p.PlatformID
                            WHERE 1=1 ";

            filter.AreaIDs = filter.AreaIDsText.SeperateToListIntOrDefaultValue(filter.AreaIDs);
            filter.PlatformIDs = filter.PlatformIDsText.SeperateToListIntOrDefaultValue(filter.PlatformIDs);

            if (filter.AreaIDs.IsNotEmptyNull())
                sql += " AND a.AreaID in @AreaIDs ";

            if (filter.PlatformIDs.IsNotEmptyNull())
                sql += " AND a.PlatformID in @PlatformIDs ";

            var query = QueryByDapper<AreaBriefModel>(sql , filter);
            return query;
        }


 		public CRUDResultModel SyncAreaConfigToTargetAreaIDs(int templateAreaID, List<int> targetAreaIDs)
        {
            var p = new DynamicParameters();
            p.Add("intAreaID", templateAreaID);
            p.Add("SelectedAreaIDs", CustomSqlHelper.ConvertListIntToDatableValueType(targetAreaIDs).AsTableValuedParameter());
            p.Add("chvErrMsg", dbType: DbType.String, direction: ParameterDirection.Output, size: 50);

            using (var conn = new SqlConnection(ConnectionString))
            {
                var result = conn.Execute("PrWeb_SyncAreaConfigNew", p, commandType: CommandType.StoredProcedure);

                var msg = p.Get<string>("chvErrMsg");

                return new CRUDResultModel
                {
                    IsSuccess = msg.Contains("渠道設定成功"),
                    Message = msg
                };
            }
        }


        /// <summary>
        /// 這個function專門處理更新 AreaConfig 這個欄位 (json)
        /// </summary>
        public CRUDResultModel SyncAreaConfigJsonValueToTargetAreaIDs(int templateAreaID, List<int> targetAreaIDs , AreaConfigSyncOptionalSettingModel syncOptionalSetting)
        {
            var queryTemplateArea = GetById(templateAreaID);

            if (queryTemplateArea == null)
                return new CRUDResultModel("查无资料");
            
            //**這寫法最擔心的是 json的欄位結構有改變。model這邊又沒有同步到...
            var myAreaConfig = queryTemplateArea.AreaConfig.TryJsonConvertToModel<AreaConfigJsonModel>();

            if (myAreaConfig == null)
                return new CRUDResultModel("AreaConfig解析失败");

            var queryOthers = GetByIds(targetAreaIDs);

            if (queryOthers.IsEmptyOrNull())
                return new CRUDResultModel("无资料可更新");


            Dictionary<int, string> updateValueDic = new Dictionary<int, string>();


            foreach(var item in queryOthers)
            {
                AreaConfigJsonModel originalSetting = item.AreaConfig.TryJsonConvertToModel<AreaConfigJsonModel>();

                if (originalSetting == null)
                    return new CRUDResultModel("AreaConfig解析失敗");

                AreaConfigJsonModel updateValue = myAreaConfig.ReturnInjecteObject<AreaConfigJsonModel>();
                example.invalid.SiteID = example.invalid.SiteID;

                //如果有指定不覆蓋特定欄位的值 還原成原本的設定
                if (!syncOptionalSetting.IsSyncDownloadUrl)
                {
                    updateValue.systemConfig.JiuLiao.DownloadUrl = originalSetting.systemConfig.JiuLiao.DownloadUrl;
                }

                if(!syncOptionalSetting.IsSyncAgentSetting)
                {
                    updateValue.systemConfig.payConfig.UIClub2AgentMode1 = originalSetting.systemConfig.payConfig.UIClub2AgentMode1;
                    updateValue.systemConfig.payConfig.UIClub2AgentMode2 = originalSetting.systemConfig.payConfig.UIClub2AgentMode2;
                    updateValue.systemConfig.payConfig.UIClub2AgentMode3 = originalSetting.systemConfig.payConfig.UIClub2AgentMode3;
                    updateValue.systemConfig.payConfig.UIClub2AgentMode4 = originalSetting.systemConfig.payConfig.UIClub2AgentMode4;
                    updateValue.systemConfig.payConfig.UIClub2AgentMode5 = originalSetting.systemConfig.payConfig.UIClub2AgentMode5;
                    updateValue.systemConfig.payConfig.UIClub2AgentMode6 = originalSetting.systemConfig.payConfig.UIClub2AgentMode6;
                }

                if(!syncOptionalSetting.IsSyncHallStyle)
                {
                    example.invalid.Template = example.invalid.Template;
                }

                string toJsonValue = updateValue.ConvertToJsonString();
                updateValueDic.Add(item.AreaID, toJsonValue);
            }


            string updateSQL = $" UPDATE {TableName} SET AreaConfig = @AreaConfig WHERE AreaID = @AreaID ";
            using (var conn = new SqlConnection(ConnectionString))
            {
                //迴圈呼叫update有點鳥，但這邊筆數應該不多 還可接受
                foreach(var item in updateValueDic)
                {
                    var updatePara = new { AreaID = item.Key, AreaConfig = item.Value };
                    conn.Execute(updateSQL, updatePara);
                }
            }

            return new CRUDResultModel
            {
                IsSuccess = true,
                Message = "更新成功"
            };

        }


    }


}
