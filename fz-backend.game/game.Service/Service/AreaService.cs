using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity.Area;
using game.Entity.Game;
using game.Infrastructure;

namespace game.Service.Service
{
    public class AreaService : IAreaService
    {
        private IRepositoryFactory _repoFactory;
        private IDatabaseFactory _dbFactory;
        public AreaService(IRepositoryFactory repoFactory, IDatabaseFactory dbFactory)
        {
            _repoFactory = repoFactory;
            _dbFactory = dbFactory;
        }
        public bool AddArea(Area area, ref string message)
        {
            var chvErrMsg = new SqlParameter
            {
                ParameterName = "@chvErrMsg",
                Value = message,
                Direction = ParameterDirection.Output,
                Size = 32
            };

            SqlParameter[] paras = {
                                    new SqlParameter("@intAreaID",area.Id),
                                    new SqlParameter("@chvAreaName",area.AreaName),
                                    new SqlParameter("@chvClientPlatform",area.ClientPlatform),
                                    new SqlParameter("@intPlatformID",area.PlatformID),
                                    new SqlParameter("@chvMemo",area.Memo),
                                    new SqlParameter("@intAdminID",area.AdminID),
                                    new SqlParameter("@intSort",area.Sort),
                                    new SqlParameter("@chvShareURL",area.ShareURL),
                                    new SqlParameter("@intShowConvertLimit",area.ShowConvertLimit),
                                    new SqlParameter("@intShowAllGameLimit",area.ShowAllGameLimit),
                                    new SqlParameter("@intShowCS",area.ShowCS),
                                    new SqlParameter("@intShowVIPAgent",area.ShowVIPAgent),

                                    new SqlParameter("@intPlaytimeCS",area.PlaytimeCS),
                                    new SqlParameter("@intPlaytimeConvert",area.PlaytimeConvert),
                                    new SqlParameter("@intPlaytimeVIPAgent",area.PlaytimeVIPAgent),

                                    new SqlParameter("@chvFAQContent",area.FAQContent),
                                    new SqlParameter("@chvPayTips",area.PayTips),
                                    new SqlParameter("@chvUpgradeMsg",area.UpGradeMsg),
                                    new SqlParameter("@chvAnnMsg",area.AnnMsg),
                                    new SqlParameter("@tnyAnnMsgShow",area.AnnMsgShow),
                                    new SqlParameter("@tnyShowCampaignAtLogin",area.ShowCampaignAtLogin),
                                    new SqlParameter("@tnyShowFirstDepositAtLogin", area.ShowFirstDepositAtLogin),
                                    new SqlParameter("@tnySetDefault",area.SetDefault),
                                    new SqlParameter("@lngDefaultMoney",area.DefaultMoney),
                                    new SqlParameter("@lngGiftMoney",area.GiftMoney),
                                    new SqlParameter("@chvAreaConfig",area.AreaConfig),
                                    chvErrMsg
                                   };

            int returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_Area_insertArea", paras);
            message = chvErrMsg.Value.ToString();
            if (returnValue == 1) message = "操作成功";

            return returnValue == 1;
        }
        public bool ModifyArea(int areaID, int oldAreaID, string AreaName, string clientPlatform, int platformID, string memo, int adminID, int sort, string shareURL, long showConvertLimit, long showAllGameLimit, ref string message)
        {

            var chvErrMsg = new SqlParameter
            {
                ParameterName = "@chvErrMsg",
                Value = message,
                Direction = ParameterDirection.Output,
                Size = 32
            };

            SqlParameter[] paras = {
                                    new SqlParameter("@intAreaID", areaID),
                                    new SqlParameter("@intOldAreaID", oldAreaID),
                                    new SqlParameter("@chvAreaName",AreaName),
                                    new SqlParameter("@chvClientPlatform",clientPlatform),
                                    new SqlParameter("@intPlatformID",platformID),
                                    new SqlParameter("@chvMemo",memo),
                                    new SqlParameter("@intAdminID",adminID),
                                    new SqlParameter("@intSort",sort),
                                    new SqlParameter("@chvShareURL",shareURL),
                                    new SqlParameter("@intShowConvertLimit",showConvertLimit),
                                    new SqlParameter("@intShowAllGameLimit",showAllGameLimit),
                                    chvErrMsg
                                   };

            int returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_Area_updateArea", paras);
            message = chvErrMsg.Value.ToString();

            if (returnValue == 1) message = "操作成功";

            return returnValue == 1;
        }
        public bool DeleteArea(int areaID, ref string message)
        {
            var area = _repoFactory.CreateRepository<Area>().GetById(areaID);
            if (area == null)
            {
                message = "操作失败:区域不存在";
                return false;
            }
            _repoFactory.CreateRepository<Area>().Delete(area);
            message = "操作成功";
            return true;
        }

        public Area GetAreaById(int areaID)
        {
            return _repoFactory.CreateRepository<Area>().Entities.FirstOrDefault(x => x.Id == areaID);
        }

        public List<Area> GetArea(int? platformId, int? areaID)
        {
            return _repoFactory.CreateRepository<Area>().Entities.Where(m => platformId == null || m.PlatformID == platformId).Where(x => areaID == null || x.Id == areaID).ToList();
        }

        public List<Area> GetAreaByEnable(int? platformId, int? areaID)
        {
            return _repoFactory.CreateRepository<Area>().Entities.Where(e => e.AreaStatus == true).Where(m => platformId == null || m.PlatformID == platformId).Where(x => areaID == null || x.Id == areaID).ToList();
        }



        public List<AreaGameList> GetAreaGameList(int areaID)
        {
            return _repoFactory.CreateRepository<AreaGameList>().Entities.Where(a => a.AreaID == areaID).OrderByDescending(a => a.Sort).ToList();
        }

        public bool AddGameArea(Entity.Area.AreaGameList m, ref string message)
        {
            if (_repoFactory.CreateRepository<AreaGameList>().Entities.FirstOrDefault(ag => ag.GameID == m.GameID && ag.AreaID == m.AreaID) != null)
            {
                message = "操作失败:该区域已经存在此游戏";
                return false;
            }

            _repoFactory.CreateRepository<AreaGameList>().Insert(m);
            return true;
        }

        public AreaGameList GetAreaGameById(int id)
        {
            return _repoFactory.CreateRepository<AreaGameList>().Entities.FirstOrDefault(x => x.Id == id);
        }
        public bool ModifyGameAreaSort(int id, int sort, ref string message)
        {
            var ag = _repoFactory.CreateRepository<AreaGameList>().GetById(id);
            if (ag == null)
            {
                message = "操作失败;记录不存在";
                return true;
            }
            ag.Sort = sort;
            _repoFactory.CreateRepository<AreaGameList>().Update(ag);
            message = "操作成功";
            return true;
        }

        public bool DeleteGameArea(int id, ref string message)
        {
            var ag = _repoFactory.CreateRepository<AreaGameList>().GetById(id);
            if (ag != null)
            {
                _repoFactory.CreateRepository<AreaGameList>().Delete(ag);
            }
            message = "操作成功";
            return true;
        }


        public List<AreaAdminUser> GetAreaByAdminID(int adminID)
        {
            SqlParameter[] paras = {
                                     new SqlParameter("@intAdminID",adminID)
                                   };
            var list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<AreaAdminUser>("dbo.[PrWeb_GetAreaAdminUser]", paras).ToList();
            return list;
        }


        public List<Platform> GetPlatform()
        {
            return _repoFactory.CreateRepository<Platform>().Entities.ToList();
        }


        public Platform GetPlatformById(int id)
        {
            return _repoFactory.CreateRepository<Platform>().GetById(id);
        }

        public Platform GetPlatformByShanFuID(string id)
        {
            return _repoFactory.CreateRepository<Platform>().Entities.FirstOrDefault(x=>x.ShanFuAccountID == id);
        }

        public List<Company> GetCompany()
        {
            return _repoFactory.CreateRepository<Company>().Entities.ToList();
        }

        public Company GetCompanyById(int id)
        {
            return _repoFactory.CreateRepository<Company>().GetById(id);
        }

        public List<GameSort> GetPlatformGameSortById(int areaID)
        {
            SqlParameter[] paras = {
                                     new SqlParameter("@intAreaID",areaID)
                                   };

            return _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<GameSort>("dbo.[PrWeb_Game_GetGameSort]", paras).ToList();
        }

        public List<Platform> GetPlatformByAdminID(int adminID)
        {
            return _repoFactory.CreateRepository<PlatformAdminUser>().Entities.Where(m => m.AdminID == adminID).Select(m => m.Platform).ToList();
        }

        public bool ModifyPlatform(Platform m, ref string message)
        {
            var p = _repoFactory.CreateRepository<Platform>().GetById(m.Id);
            if (_repoFactory.CreateRepository<Platform>().Entities.FirstOrDefault(a => a.Name == m.Name && a.Id != m.Id) != null)
            {
                message = "操作失败:平台名已经存在";
                return false;
            }
            _repoFactory.CreateRepository<Platform>().Update(m);
            if (p.PayAmountConfig != m.PayAmountConfig)
            {
                //用户存储过程
                SqlParameter[] paras = {
                                   new SqlParameter("@intPlatformID",m.Id)
                                   };
                int returnVal = _dbFactory.GetDatabase(game.Infrastructure.DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_Noitfy_PayAmountConfig", paras);
            }
            message = "操作成功";
            return true;
        }

        /// <summary>
        /// 建立平台
        /// </summary>
        /// <param name="platformID">指定平台號</param>
        /// <param name="platformName">平台名稱</param>
        /// <param name="unionPlatform">是否是聯運平台 1=Yes 0=No</param>
        /// <param name="areaNum">預設建立渠道数量</param>
        /// <param name="message">返回消息</param>
        /// <returns></returns>
        public bool CreateNewPlatform(int platformID, string platformName, int company, string areaNum, ref string message)
        {
            var chvErrMsg = new SqlParameter
            {
                ParameterName = "@chvErrMsg",
                Value = message,
                Direction = ParameterDirection.Output,
                Size = 32
            };

            SqlParameter[] paras = {
                                    new SqlParameter("@intPlatformID", platformID),
                                    new SqlParameter("@chvPlatformName", platformName),
                                    new SqlParameter("@intCompany",company),
                                    new SqlParameter("@intAreaNum",areaNum),
                                    chvErrMsg
                                   };

            int returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_CreateNewPlatform", paras);
            message = chvErrMsg.Value.ToString();
            if (returnValue == 1) message = "操作成功";
            return returnValue == 1;
        }

        /// <summary>
        /// 同步平台
        /// </summary>
        /// <param name="platformID">平台號</param>
        /// <param name="areaID">同步參考的渠道號</param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool SyncAreaConfig(int platformID, int areaID, ref string message)
        {
            var chvErrMsg = new SqlParameter
            {
                ParameterName = "@chvErrMsg",
                Value = message,
                Direction = ParameterDirection.Output,
                Size = 32
            };

            SqlParameter[] paras = {
                                    new SqlParameter("@intPlatformID", platformID),
                                    new SqlParameter("@intAreaID", areaID),
                                    chvErrMsg
                                   };

            int returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_SyncAreaConfig", paras);
            message = chvErrMsg.Value.ToString();
            if (returnValue == 1) message = "操作成功";
            return returnValue == 1;
        }

        public bool UpdateAreaConfig(Area model, ref string message)
        {
            var p = _repoFactory.CreateRepository<Area>().GetById(model.Id);
            p.PlatformID = model.PlatformID;
            p.AreaName = model.AreaName;
            p.ClientPlatform = model.ClientPlatform;
            p.Memo = model.Memo;
            p.ShowConvertLimit = model.ShowConvertLimit;
            p.ShowVIPAgent = model.ShowVIPAgent;
            p.ShowCS = model.ShowCS;
            p.ShowAllGameLimit = model.ShowAllGameLimit;
            p.PlaytimeCS = model.PlaytimeCS;
            p.PlaytimeConvert = model.PlaytimeConvert;
            p.PlaytimeVIPAgent = model.PlaytimeVIPAgent;
            p.FAQContent = model.FAQContent;
            p.PayTips = model.PayTips;
            p.UpGradeMsg = model.UpGradeMsg;
            p.AnnMsg = model.AnnMsg;
            p.AnnMsgShow = model.AnnMsgShow;
            p.ShowCampaignAtLogin = model.ShowCampaignAtLogin;
            p.ShowFirstDepositAtLogin = model.ShowFirstDepositAtLogin;
            p.SetDefault = model.SetDefault;
            p.AreaConfig = model.AreaConfig;
            p.GiftMoney = model.GiftMoney;
            p.DefaultMoney = model.DefaultMoney;
            p.AgentNameID = model.AgentNameID;
            p.ShopNameID = model.ShopNameID;
            p.ShowConvertPopup = model.ShowConvertPopup;
            p.ConvertPopupText = model.ConvertPopupText;
            p.ShowConvertBindPhone = model.ShowConvertBindPhone;
            p.ShowAllGameBindPhone = model.ShowAllGameBindPhone;
            p.ShowVIPAgentBindPhone = model.ShowVIPAgentBindPhone;
            p.ShowCSBindPhone = model.ShowCSBindPhone;
            _repoFactory.CreateRepository<Area>().Update(p);
            message = "操作成功";
            return true;
        }

        public bool UpdateGameSort(int platformID, int areaID, int gameID, int sort, int popular, int iconID, int status)
        {
            SqlParameter[] paras = {
                                   new SqlParameter("@intPlatformID",platformID),
                                   new SqlParameter("@intAreaID",areaID),
                                   new SqlParameter("@intGameID",gameID),
                                   new SqlParameter("@intSort",sort),
                                   new SqlParameter("@intPopular",popular),
                                   new SqlParameter("@intIconID",iconID),
                                   new SqlParameter("@intStatus",status)
                                   };
            int returnVal = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_Game_UpdateGameSort", paras);
            return true;
        }

        public bool AddAdminPlatform(int adminID, int platformID, int? areaID, ref string message)
        {

            SqlParameter[] paras = {
                                   new SqlParameter("@intAdminID",adminID),
                                   new SqlParameter("@intPlatformID",platformID),
                                   new SqlParameter("@intAreaID",(object)areaID??DBNull.Value)

                                   };
            int returnVal = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_AddPlatformAdminUser", paras);
            message = "操作成功";
            return true;
        }

        public bool RemoveAdminPlatform(int adminID, ref string message)
        {
            SqlParameter[] paras = {
                                   new SqlParameter("@intAdminID",adminID)
                                   };
            int returnVal = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_RemovePlatformAdminUser", paras);
            message = "操作成功";
            return true;
        }

        public PagedList<AreaGame> GetAreaGame(int? platformID, int? areaID, int pageIndex, int pageSize)
        {
            int pageCount = 0;

            SqlParameter[] paras = {
                                     new SqlParameter("@intPlatformID",platformID),
                                     new SqlParameter("@intAreaID  ",areaID),
                                     new SqlParameter("@intPageIndex",pageIndex),
                                     new SqlParameter("@intPageSize",pageSize),
                                     new SqlParameter("@intPageCount",pageCount),
                                   };

            paras.LastOrDefault().Direction = ParameterDirection.Output;
            var list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<AreaGame>("dbo.[PrWeb_GetGetAreaGame]", paras).ToList();
            pageCount = Convert.ToInt32(paras.LastOrDefault().Value);

            return new PagedList<AreaGame>(list, pageIndex, pageSize, pageCount); ;

        }


        public bool SetAreaGameStatus(int id, bool status, ref string message)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intID",id),
                                    new SqlParameter("@tnyStatus",status)
                                   };

            int returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_UpdateAreaGameStatus", paras);
            message = Convert.ToString(paras.LastOrDefault().Value);

            if (returnValue == 1) message = "修改成功";
            return returnValue == 1;

        }


        public bool AddAreaGame(int platformID, int areaID, int gameID, int sort, bool status, int popular, ref string message)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intPlatformID",platformID),
                                    new SqlParameter("@intAreaID",areaID),
                                    new SqlParameter("@intGameID",gameID),
                                    new SqlParameter("@intSort",sort),
                                    new SqlParameter("@tintStatus",status),
                                    new SqlParameter("@tintPopular",popular),
                                    DBHelper.GetProcedureErrorMsg()
                                   };

            int returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_AddAreaGame", paras);
            message = Convert.ToString(paras.LastOrDefault().Value);

            return returnValue == 1;

        }

        public bool UpdateAreaGame(int ID, int sort, bool status, int popular, ref string message)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intID",ID),
                                    new SqlParameter("@intSort",sort),
                                    new SqlParameter("@tintStatus",status),
                                    new SqlParameter("@tintPopular",popular),
                                    DBHelper.GetProcedureErrorMsg()
                                   };

            int returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_UpdateAreaGame", paras);
            message = Convert.ToString(paras.LastOrDefault().Value);

            return returnValue == 1;

        }

        public bool DeleteAreaGame(int ID, ref string message)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intID",ID),
                                    DBHelper.GetProcedureErrorMsg()
                                   };

            int returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_DeleteAreaGame", paras);
            message = Convert.ToString(paras.LastOrDefault().Value);

            return returnValue == 1;

        }

        public List<Advertisement> GetAdvertisements(int? platformID, int? areaID)
        {
            SqlParameter[] paras = {
                                     new SqlParameter("@intPlatformID",(object)platformID??DBNull.Value),
                                     new SqlParameter("@intAreaID",(object)areaID??DBNull.Value)
                                   };

            return _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<Advertisement>("dbo.[PrWeb_AD_GetList]", paras).ToList();
        }

        public bool AddAdvertisement(Advertisement mode, ref string message)
        {
            SqlParameter[] paras = {
                                     new SqlParameter("@intPlatformID",mode.PlatformID),
                                     new SqlParameter("@intAreaID",mode.AreaID),
                                     new SqlParameter("@chvPicture",mode.Picture),
                                     new SqlParameter("@chvLink",mode.Link),
                                     new SqlParameter("@tnyBehavior",(int)mode.Behavior),
                                     new SqlParameter("@tnyStatus",(int)mode.Status),
                                   };

            int returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.[PrWeb_AD_AddData]", paras);

            message = "操作成功";
            return true;
        }

        public bool UpdateAdvertisement(Advertisement mode, ref string message)
        {
            SqlParameter[] paras = {
                                     new SqlParameter("@intID",mode.Id),
                                     new SqlParameter("@chvPicture",mode.Picture),
                                     new SqlParameter("@chvLink",mode.Link),
                                     new SqlParameter("@tnyBehavior",(int)mode.Behavior),
                                     new SqlParameter("@tnyStatus",(int)mode.Status),
                                   };

            int returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.[PrWeb_AD_UpdateData]", paras);

            message = "操作成功";
            return true;
        }

        public bool DeleteAdvertisement(int id, ref string message)
        {
            SqlParameter[] paras = {
                                     new SqlParameter("@intID",id)
                                   };

            int returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.[PrWeb_AD_DeleteData]", paras);

            message = "操作成功";
            return true;
        }

        public List<Campaign> GetCampaigns(int? platformID, int? areaID)
        {
            SqlParameter[] paras = {
                                     new SqlParameter("@intPlatformID",(object)platformID??DBNull.Value),
                                     new SqlParameter("@intAreaID",(object)areaID??DBNull.Value)
                                   };

            return _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<Campaign>("dbo.[PrWeb_Campaign_GetList]", paras).ToList();
        }

        public bool AddCampaign(Campaign mode, ref string message)
        {
            SqlParameter[] paras = {
                                     new SqlParameter("@intPlatformID",mode.PlatformID),
                                     new SqlParameter("@intAreaID",mode.AreaID),
                                     new SqlParameter("@chvPicture",mode.Picture),
                                     new SqlParameter("@chvTitle",mode.Title),
                                     new SqlParameter("@intSeqID", mode.SeqID),
                                     new SqlParameter("@tnyTag",(int)mode.CampaignTag),
                                     new SqlParameter("@tnyBehavior",(int)mode.CampaignBehavior),
                                     new SqlParameter("@chvLink",mode.Link),
                                     new SqlParameter("@chvStartTime",mode.StrStartTime),
                                     new SqlParameter("@chvEndTime",mode.StrEndTime),
                                     new SqlParameter("@tnyStatus",(int)mode.CampaignStatus)
                                   };

            int returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.[PrWeb_Campaign_AddData]", paras);

            message = "操作成功";
            return true;
        }

        public bool UpdateCampaign(Campaign mode, ref string message)
        {
            SqlParameter[] paras = {
                                     new SqlParameter("@intID",mode.Id),
                                     new SqlParameter("@chvPicture",mode.Picture),
                                     new SqlParameter("@chvTitle",mode.Title),
                                     new SqlParameter("@intSeqID", mode.SeqID),
                                     new SqlParameter("@tnyTag",(int)mode.CampaignTag),
                                     new SqlParameter("@tnyBehavior",(int)mode.CampaignBehavior),
                                     new SqlParameter("@chvLink",mode.Link),
                                     new SqlParameter("@chvStartTime",mode.StrStartTime),
                                     new SqlParameter("@chvEndTime",mode.StrEndTime),
                                     new SqlParameter("@tnyStatus",(int)mode.CampaignStatus)
                                   };

            int returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.[PrWeb_Campaign_UpdateData]", paras);

            message = "操作成功";
            return true;
        }

        public bool DeleteCampaign(int id, ref string message)
        {
            SqlParameter[] paras = {
                                     new SqlParameter("@intID",id)
                                   };

            int returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.[PrWeb_Campaign_DeleteData]", paras);

            message = "操作成功";
            return true;
        }








        public List<GameAliasName> GetGameAliasName()
        {
            return _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<GameAliasName>("dbo.[PrWeb_Area_GetGameAliasName]").ToList();
        }

        public bool InsertGameAliasName(GameAliasName mode, ref string message)
        {
            var chvErrMsg = new SqlParameter
            {
                ParameterName = "@chvErrMsg",
                Value = message,
                Direction = ParameterDirection.Output,
                Size = 32
            };
            SqlParameter[] paras = {
                                     new SqlParameter("@intGameID", mode.GameID),
                                     new SqlParameter("@chvIconTitle", mode.IconTitle),
                                     new SqlParameter("@chvIconName", mode.IconName),
                                     new SqlParameter("@chvAliasName", mode.AliasName),
                                     new SqlParameter("@chvSkin", mode.Skin),
                                     chvErrMsg
                                   };

            int returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.[PrWeb_Area_AddGameAliasName]", paras);

            message = chvErrMsg.Value.ToString();
            if (returnValue == 1) message = "操作成功";
            return true;
        }

        public bool UpdGameAliasName(GameAliasName mode, ref string message)
        {
            SqlParameter[] paras = {
                                     new SqlParameter("@intGameID", mode.GameID),
                                     new SqlParameter("@intIconID", mode.IconID),
                                     new SqlParameter("@chvIconTitle", mode.IconTitle),
                                     new SqlParameter("@chvIconName", mode.IconName),
                                     new SqlParameter("@chvAliasName", mode.AliasName),
                                     new SqlParameter("@chvSkin", mode.Skin)
                                   };

            int returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.[PrWeb_Area_UpdGameAliasName]", paras);
            message = "操作成功";
            return true;
        }

        public bool AddPlatform(int platformID, string platformName, ref string message)
        {
            SqlParameter[] paras = {
                                     new SqlParameter("@intPlatformID", platformID),
                                     new SqlParameter("@chvPlatformName", platformName),
                                     DBHelper.GetProcedureErrorMsg()
                                   };

            int returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.[PrWeb_CreateNewPlatform]", paras);
            message = Convert.ToString(paras.LastOrDefault().Value);

            return true;
        }
    }
}
