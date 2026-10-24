using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity;
using game.Entity.Area;
using game.Entity.Game;
using game.Entity.Users;
using game.Infrastructure;
using game.Service.DAO;
using game.Infrastructure.Extensions;
using game.Infrastructure.Helper;

namespace game.Service
{
    public class GameService : IGameService
    {
        private IRepositoryFactory _repoFactory;
        private IDatabaseFactory _dbFactory;
        private UserGameInfoRepository _userGameInfoRepository;
        private ServerPlatformRelationsRepository _serverPlatformRelationsRepository;

        public GameService(IRepositoryFactory repoFactory, IDatabaseFactory dbFactory)
        {
            _repoFactory = repoFactory;
            _dbFactory = dbFactory;
            _userGameInfoRepository = new UserGameInfoRepository();
            _serverPlatformRelationsRepository = new ServerPlatformRelationsRepository();
        }

        public List<Entity.Game.Game> GetGame()
        {
            return _repoFactory.CreateRepository<Game>().Entities.ToList();
        }

        public Entity.Game.Game GetGameByID(int gameID)
        {
            return _repoFactory.CreateRepository<Game>().GetById(gameID);
        }

        public bool SetGameStatus(int gameID, byte status, ref string message)
        {
            var game = _repoFactory.CreateRepository<Game>().GetById(gameID);
            if (game == null)
            {
                message = "操作失败：游戏不存在";
                return false;
            }

            game.Status = status;
            _repoFactory.CreateRepository<Game>().Update(game);

            message = "操作成功：设置成功";
            return true;
        }

        public List<Entity.Game.GameServer> GetGameServer(int? gameID, int? serverID, string ipAddress ,int? adminID)
        {
            IQueryable<GameServer> iq = _repoFactory
                    .CreateRepository<GameServer>()
                    .Entities
                    .Where(m => (gameID == null || gameID == m.GameID) &&
                            (serverID == null || serverID == m.Id) &&
                            (ipAddress == null || ipAddress == m.ClientAddr));

            //如果有指定adminID 額外從某張table過濾
            if(adminID.HasValue)
            {
                List<int> queryAvailableServers = _serverPlatformRelationsRepository.GetAvailableServerIDsByAdminUserID(adminID.Value);

                iq = iq.Where(x => queryAvailableServers.Contains(x.Id));
            }

            var result = iq.OrderBy(m => m.ServerName)
                         .ThenBy(m => m.Id)
                         .ToList();

            return result;
        }


        public List<Entity.Game.PlatformGameServer> GetSuperToolServerList(int adminId, int platformId)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intAdminID",adminId),
                                    new SqlParameter("@intPlatformID",platformId)
                                   };
            return _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<Entity.Game.PlatformGameServer>("[dbo].[PrWeb_Game_GetSuperToolServerList]", paras).ToList();
        }


        public Entity.Game.GameServer GetGameServerByID(int serverID)
        {
            return _repoFactory.CreateRepository<GameServer>().GetById(serverID);
        }

        public List<Entity.Game.PlazaServer> GetPlazaServer(string ipAddress)
        {
            return _repoFactory.CreateRepository<PlazaServer>()
                .Entities
                .Where(m => ipAddress == m.ClientAddr)
                .OrderBy(m => m.Id)
                .ToList();
        }

        public Entity.Game.PlazaServer GetPlazaServerByID(int serverID)
        {
            return _repoFactory.CreateRepository<PlazaServer>().GetById(serverID);
        }

        public List<Entity.Game.HallUserOnLine> GetHallOnLine(int? serverID, int? userID)
        {
            return _repoFactory.CreateRepository<HallUserOnLine>()
                .Entities
                .Where(m => (serverID == null || m.ServerID == serverID) &&
                        (userID == null || m.Id == userID)
                )
                .ToList();
        }

        public List<Entity.Game.GameUserOnLine> GetGameOnLine(int? serverID, int? userID)
        {
            return _repoFactory.CreateRepository<GameUserOnLine>()
                .Entities
                .Where(m => (serverID == null || m.ServerID == serverID) &&
                        (userID == null || m.Id == userID)
                )
                .ToList();
        }


        public bool KickoutGameUser(int? userID, int? serverID)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intUserID",userID),
                                    new SqlParameter("@intServerID",serverID)
                                   };
            int returnVal = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("[dbo].[PrWeb_Game_KickoutUser]", paras);
            return returnVal == 1 ? true : false;
        }

        public List<Entity.Game.UserGameInfo> GetUserGameInfo(int userID, int? gameID)
        {
            return _repoFactory.CreateRepository<UserGameInfo>()
                .Entities
                .Where(m => userID == m.UserID && (gameID == null || gameID == m.GameID))
                .OrderBy(m => m.GameID)
                .ToList();
        }

        public PagedList<UserGameInfoViewModel> GetUserGameInfoPaging(QueryUserGameInfoRequestModel filter)
        {
            var queryPaging = _userGameInfoRepository.GetPagingByFilter(filter);

            if (queryPaging.PageData.IsEmptyOrNull())
                return queryPaging;

            List<int> queryUserIds = queryPaging.PageData.Select(x => x.UserID).Distinct().ToList();

            var queryUsers = _repoFactory.CreateRepository<UserInfo>()
                .Entities
                .Where(x => queryUserIds.Contains(x.Id))
                .ToList();

            foreach(var item in queryPaging.PageData)
            {
                item.UserInfo = queryUsers.FirstOrDefault(x => x.Id == item.UserID);

                //嚴重耗費 傳輸資源的string欄位 在此API不使用
                if(item.UserInfo != null)
                {
                    DataSimplifyHelper.SimplifyAreaContent(item.UserInfo.Area);
                }
            }

            return queryPaging;
        }

        

        public FW.Core.Helper.PagedList<Entity.Game.GameLog> GetGameLog(int? serverID, int? gameID, int? userID, string ipAddress, game.Entity.Users.HDType? hDtype, string hDcode, DateTime? loginBeginTime, DateTime? loginEndTime, DateTime? logoutBeginTime, DateTime? logoutEndTime, int? areaID, int pageIndex, int pageSize, int? AdminID = null)
        {
            int pageCount = 0;
            SqlParameter[] paras = {
                                     new SqlParameter("@intAdminID",AdminID),
                                     new SqlParameter("@intServerID",serverID),
                                     new SqlParameter("@intGameID",gameID),
                                     new SqlParameter("@intUserID",userID),
                                     new SqlParameter("@chvIpAddress",ipAddress),
                                     new SqlParameter("@intHDtype",hDtype==null?null:Convert.ToInt32(hDtype).ToString()),
                                     new SqlParameter("@chvHDcode",hDcode),
                                     new SqlParameter("@logInBeginTime",loginBeginTime),
                                     new SqlParameter("@logInEndTime",loginEndTime),
                                     new SqlParameter("@logOutBeginTime",logoutBeginTime),
                                     new SqlParameter("@logOutEndTime",logoutEndTime),
                                     new SqlParameter("@intAreaID",areaID),
                                     new SqlParameter("@intPageIndex",pageIndex),
                                     new SqlParameter("@intPageSize",pageSize),
                                     new SqlParameter("@intPageCount",pageCount),
                                   };
            paras.LastOrDefault().Direction = System.Data.ParameterDirection.Output;
            var list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<GameLog>("dbo.[PrWeb_Game_GetGameLog]", paras);
            pageCount = Convert.ToInt32(paras.LastOrDefault().Value);
            var pageList = new FW.Core.Helper.PagedList<GameLog>();
            pageList.PageData = list.ToList();
            pageList.PageIndex = pageIndex;
            pageList.PageSize = pageSize;
            pageList.TotalCount = Convert.ToInt32(pageCount);
            pageList.TotalPage = game.Infrastructure.Common.GetTotalPage(pageSize, Convert.ToInt32(pageCount));

            return pageList;
        }

        public List<HallUserOnLine> GetHallOnLine(int? serverID, Entity.Users.HDType? hdType, int? userID)
        {
            return _repoFactory.CreateRepository<HallUserOnLine>().Entities
                .Where(m => (hdType == null || m.HDType == hdType) &&
                            (userID == null || m.Id == userID) &&
                            (serverID == null || m.ServerID == serverID)
                )
                .ToList();
        }

        public List<GameUserOnLine> GetGameOnLine(int? serverID, Entity.Users.HDType? hdType, int? userID)
        {
            return _repoFactory.CreateRepository<GameUserOnLine>()
                .Entities
                .Where(m => (serverID == null || m.ServerID == serverID) &&
                        (hdType == null || m.HDType == hdType) &&
                        (userID == null || m.Id == userID)
                )
                .ToList();
        }

        public bool AddGameServer(GameServer server, ref string message)
        {
            var rtnCode = string.Empty;
            var chvErrMsg = new SqlParameter
            {
                ParameterName = "@chvErrMsg",
                Value = message,
                Direction = ParameterDirection.Output,
                Size = 32
            };
            var chvRtnCode = new SqlParameter
            {
                ParameterName = "@intRtnCode",
                Value = rtnCode,
                Direction = ParameterDirection.Output,
                Size = 32
            };
            SqlParameter[] paras = {
                            new SqlParameter("@intServerID", (object)server.Id),
                            new SqlParameter("@intGameID", (object)server.GameID),
                            new SqlParameter("@chvServerName", server.ServerName),
                            new SqlParameter("@chvShowName", server.ShowName),
                            new SqlParameter("@chvClientAddr", server.ClientAddr),
                            new SqlParameter("@chvNotifyAddr", server.NotifyAddr),
                            new SqlParameter("@chvHttpAddr", server.HttpAddr),
                            new SqlParameter("@intMoney", server.Money),
                            chvErrMsg,
                            chvRtnCode
                                    };

            _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("[dbo].[PrWeb_AddGameServer]", paras);

            message = chvErrMsg.Value.ToString();

            if (Convert.ToInt32(chvRtnCode.Value) == 1)
                return true;
            else
                return false;
        }


        public bool ModifyGameServer(GameServer server, ref string message)
        {
            if (_repoFactory.CreateRepository<GameServer>().Entities.Where(s => s.GameID == server.GameID && s.ServerName == server.ServerName && s.Id != server.Id).FirstOrDefault() != null)
            {
                message = "操作失败:房间名已经存在";
                return false;
            }

            _repoFactory.CreateRepository<GameServer>().Update(server);
            message = "操作成功:修改房间信息成功";
            return true;

        }

        public bool DelGameServer(int id, ref string message)
        {
            var server = _repoFactory.CreateRepository<GameServer>().Entities.Where(s => s.Id == id).FirstOrDefault();
            if (server != null)
            {
                _repoFactory.CreateRepository<GameServer>().Delete(server);
                message = "操作成功:删除房间成功";
                return true;
            }

            message = "操作失败:房间信息不存在";
            return false;
        }

        /// <summary>
        /// 金币回滚
        /// </summary>
        /// <param name="serverID"></param>
        public void Restore(int serverID)
        {
            SqlParameter[] paras = { new SqlParameter("@intServerID", serverID) };
            _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("[dbo].[PrPsWeb_MoneyRestore]", paras);
        }

        public List<PlazaServer> GetAllPlazaServer()
        {
            return _repoFactory.CreateRepository<PlazaServer>()
                 .Entities
                 .OrderBy(m => m.Id)
                 .ToList();
        }

        public FW.Core.Helper.PagedList<ForbidIPAddress> GetForbidIPAddress(string ipAddress, DateTime? createBeginTime, DateTime? createEndTime, int pageIndex, int pageSize)
        {
            return new PagedList<ForbidIPAddress>(_repoFactory.CreateRepository<ForbidIPAddress>()
                .Entities
                .Where(m => (ipAddress == null || m.IPAddress.Contains(ipAddress)) &&
                (createBeginTime == null || m.Createtime > createBeginTime) &&
                (createEndTime == null || m.Createtime < createEndTime))
                .OrderByDescending(m => m.Createtime), pageIndex, pageSize);
        }

        public bool AddForbidIPAddress(ForbidIPAddress forbidIPAddress, ref string message)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@chvIPAddress", forbidIPAddress.IPAddress),
                                    new SqlParameter("@intSec", (object)0),
                                    new SqlParameter("@chvMemo", forbidIPAddress.Memo),
                                    new SqlParameter("@dtexpiredate", forbidIPAddress.ExpireTime),
                                    new SqlParameter("@chvIPAddress2", forbidIPAddress.IPAddress2)
                                    };

            _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("[dbo].[PrSys_BlockIP]", paras);
            message = "操作成功！";
            return true;
        }

        public bool DelForbidIPAddress(int id, ref string message)
        {
            var forbididaddress = _repoFactory.CreateRepository<ForbidIPAddress>().Entities.Where(m => m.Id == id).FirstOrDefault();
            if (forbididaddress == null)
            {
                message = "操作失败！无该条数据！";
                return false;
            }
            _repoFactory.CreateRepository<ForbidIPAddress>().Delete(forbididaddress);
            message = "操作成功！";
            return true;
        }

        public HallVersion GetHallVersion()
        {
            return _dbFactory.GetDatabase(game.Infrastructure.DBNameConfig.DBName_CenterDB).ExecuteSqlQuery<HallVersion>("SELECT TOP 1 * FROM [CenterDB].[ServerInfo].[HallVersion]").FirstOrDefault();
        }

        public List<HallVersion> GetNewHallVersion()
        {
            var sql = "SELECT PlatformID, HallSkinID, AndroidVersion = Max(AnVersion), AndroidVersion2 = Max(AnVersion1), IOSVersion = Max(IOSVersion), IOSVersion2 = Max(IOSVersion1) FROM (" +
                        "SELECT PlatformID, HallSkinID, AnVersion = CASE WHEN Type = 1 THEN AndroidVersion ELSE '' END, AnVersion1 = CASE WHEN Type = 2 THEN AndroidVersion ELSE '' END, IOSVersion = CASE WHEN Type = 1 THEN IOSVersion ELSE '' END, IOSVersion1 = CASE WHEN Type = 2 THEN IOSVersion ELSE '' END " +
                        "FROM ServerInfo.HallVersions) AS A GROUP BY PlatformID, HallSkinID ORDER BY PlatformID, HallSkinID";
            return _dbFactory.GetDatabase(game.Infrastructure.DBNameConfig.DBName_CenterDB).ExecuteSqlQuery<HallVersion>(sql).ToList();
        }

        public List<HallSkin> GetHallSkin()
        {
            var sql = "SELECT HallSkinID, Code, Name FROM ServerInfo.HallSkin WHERE Status = 1";
            return _dbFactory.GetDatabase(game.Infrastructure.DBNameConfig.DBName_CenterDB).ExecuteSqlQuery<HallSkin>(sql).ToList();
        }

        public bool SaveHallVersion(string anVersion, string iosVersion, string anVersion2, string iosVersion2)
        {
            _dbFactory.GetDatabase(game.Infrastructure.DBNameConfig.DBName_CenterDB).ExecuteCommand("UPDATE [CenterDB].[ServerInfo].[HallVersion] SET [AndroidVersion]=@AnVersion, [IOSVersion]=@IOSVersion, [AndroidVersion2]=@AnVersion2, [IOSVersion2]=@IOSVersion2",
             new System.Data.Common.DbParameter[] {
               new SqlParameter("@AnVersion", anVersion),
               new SqlParameter("@IOSVersion", iosVersion),
               new SqlParameter("@AnVersion2", anVersion2),
               new SqlParameter("@IOSVersion2", iosVersion2)
            });
            return true;
        }

        public bool SaveHallVersions(int platformID, int skin, string anVersion, string iosVersion, string anVersion2, string iosVersion2)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intPlatformID", platformID),
                                    new SqlParameter("@intSkin", skin),
                                    new SqlParameter("@chvAnVersion", anVersion),
                                    new SqlParameter("@chvAnVersion2", anVersion2),
                                    new SqlParameter("@chvIOSVersion", iosVersion),
                                    new SqlParameter("@chvIOSVersion2", iosVersion2)
                                    };

            _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("[dbo].[PrWeb_Hall_AddVersion]", paras);
            return true;
        }

        public List<Entity.Game.Game> GetGameListVersion()
        {
            return _repoFactory.CreateRepository<game.Entity.Game.Game>().Entities.ToList();
        }

        public bool SaveGameListVersion(List<Entity.Game.Game> gameList)
        {
            foreach (var item in gameList)
            {
                var model = _repoFactory.CreateRepository<game.Entity.Game.Game>().Entities.Where(m => m.Id == item.Id).FirstOrDefault();
                model.AndroidVersion = item.AndroidVersion;
                model.IOSVersion = item.IOSVersion;
                _repoFactory.CreateRepository<game.Entity.Game.Game>().Update(model);
            }
            return true;
        }

        #region 兑换配置

        public SystemConfig GetSystemConfig(int? platformId)
        {
            return _repoFactory.CreateRepository<SystemConfig>().Entities.Where(m => m.PlatformID == platformId).FirstOrDefault();
        }

        public bool SaveSystemConfig(SystemConfig model, ref string message)
        {
            _repoFactory.CreateRepository<SystemConfig>().Update(model);

            message = "操作成功";
            return true;
        }

        public string GetProtectNodeBackupIP()
        {
            string sql = @"Select top 1 IP from ServerInfo.ProtectNodeBackupIP";
            string iplist = _dbFactory.GetDatabase(Infrastructure.DBNameConfig.DBName_CenterDB).ExecuteSqlQuery<string>(sql).FirstOrDefault();
            return iplist;
        }

        public bool SaveProtectNodeBackupIP(string iplist, ref string message)
        {

            if (iplist == null)
                iplist = "";
            _dbFactory.GetDatabase(game.Infrastructure.DBNameConfig.DBName_CenterDB).ExecuteCommand("UPDATE [CenterDB].[ServerInfo].[ProtectNodeBackupIP] SET [IP] = @iplist",
               new System.Data.Common.DbParameter[] {
               new SqlParameter("@iplist", iplist)
            });

            message = "操作成功";
            return true;
        }

        public List<PlatformDownload> GetDownloadList(int adminID, int? platformID)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intAdminID ",adminID),
                                    new SqlParameter("@intPlatformID",platformID)
                                   };
            var list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<PlatformDownload>("PrWeb_GetPlatformDownload", paras).ToList();
            return list;
        }


        public bool UpdatePlatformDownload(int adminID, PlatformDownload model, ref string msg)
        {
            msg = "操作成功";
            SqlParameter[] paras = {
                    new SqlParameter("@intAdminID ",adminID),
                    new SqlParameter("@intPlatformID", model.PlatformID),
                    new SqlParameter("@chvIOS", model.IOS),
                    new SqlParameter("@chvAndroid", model.Android),
                    new SqlParameter("@chvOfficial", model.Official),
                    new SqlParameter("@chvIOSMemo", model.IOSMemo),
                    new SqlParameter("@intIOSTerminate", model.IOSTerminate),
                    new SqlParameter("@intCorrect", model.Correct),
                    new SqlParameter("@chvMemo", model.Memo)
                    };
            int _returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("PrWeb_UpdatePlatformDownload", paras);

            if (_returnValue != 1) { msg = "操作失败"; }
            return _returnValue == 1;
        }

        #endregion


        public List<UserGameInfo> GetStatUserGameInfo(int userID, DateTime? beginTime, DateTime? endTime)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intUserID",userID),
                                    new SqlParameter("@dtBeginTime",beginTime),
                                    new SqlParameter("@dtEndTime",endTime)
                                   };
            var list = _dbFactory.GetDatabase(game.Infrastructure.DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<UserGameInfo>("PrWeb_Stat_GetUserGameInfo", paras).ToList();
            return list;
        }

        public List<ServerRelations> GetServerRelations(int? areaID, int? gameID)
        {
            SqlParameter[] paras = {
                                     new SqlParameter("@intAreaID",(object)areaID?? DBNull.Value),
                                     new SqlParameter("@intGameID",(object)gameID?? DBNull.Value)
                                   };
            var list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<ServerRelations>("dbo.[PrWeb_Area_GetServerRelations]", paras).ToList();
            return list;
        }


        public bool AddServerRelations(ServerRelations model, ref string message)
        {
            var _intRtnCode = 0;
            var intRtnCode = new SqlParameter
            {
                ParameterName = "@intRtnCode",
                Value = _intRtnCode,
                Direction = ParameterDirection.Output
            };


            SqlParameter[] paras = {
                                    new SqlParameter("@intAreaID",(object)model.AreaID?? DBNull.Value),
                                    new SqlParameter("@intGameID",(object)model.GameID?? DBNull.Value),
                                    new SqlParameter("@intServerID",(object)model.ServerID?? DBNull.Value),
                                    new SqlParameter("@intFlagID",(object)model.FlagID?? DBNull.Value),
                                    new SqlParameter("@tnyStatus",(object)model.Status?? DBNull.Value),
                                    intRtnCode,
                                    DBHelper.GetProcedureErrorMsg()
                                   };
            _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("[dbo].[PrWeb_Area_AddServerRelations]", paras);

            message = Convert.ToString(paras.LastOrDefault().Value);
            return intRtnCode.Value.ToString() == "1" ? true : false;
        }


        #region 游戏赢了通知配置

        public List<WinAnnConfig> GetWinAnnConfig()
        {
            return _repoFactory.CreateRepository<WinAnnConfig>().Entities.ToList();
        }

        public WinAnnConfig GetWinAnnConfigByID(int gameID)
        {
            return _repoFactory.CreateRepository<WinAnnConfig>().GetById(gameID);
        }

        public bool AddWinAnnConfig(WinAnnConfig config, ref string message)
        {
            if (GetWinAnnConfigByID(config.Id) != null)
            {
                message = "操作失败：该游戏的通知配置已经存在";
                return false;
            }

            _repoFactory.CreateRepository<WinAnnConfig>().Insert(config);

            message = "操作成功：添加游戏通知配置成功";
            return true;
        }

        public bool ModifyWinAnnConfig(WinAnnConfig config, ref string message)
        {
            if (GetWinAnnConfigByID(config.Id) == null)
            {
                message = "操作失败：游戏通知配置不存在";
                return false;
            }
            _repoFactory.CreateRepository<WinAnnConfig>().Update(config);
            message = "操作成功：修改游戏通知配置成功";
            return true;

        }

        public bool DelWinAnnConfig(int id, ref string message)
        {
            var server = GetWinAnnConfigByID(id);
            if (server != null)
            {
                _repoFactory.CreateRepository<WinAnnConfig>().Delete(server);
                message = "操作成功：删除游戏通知配置成功";
                return true;
            }

            message = "操作失败：游戏通知配置不存在";
            return false;
        }

        #endregion

        #region 机器人服务器配置

        public bool UpdateRobotServer(int gameID, int serverID, string beginTime, string endTime, int minCount, int maxCount, int minLevel, int maxLevel, int status, ref string message)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intServerID",serverID),
                                    new SqlParameter("@intGameID",gameID),
                                    new SqlParameter("@chvBeginTime",beginTime),
                                    new SqlParameter("@chvEndTime",endTime),
                                    new SqlParameter("@intMinCount",minCount),
                                    new SqlParameter("@intMaxCount",maxCount),
                                    new SqlParameter("@intMinLevel",minLevel),
                                    new SqlParameter("@intMaxLevel",maxLevel),
                                    new SqlParameter("@tnyStatus",status),
                                    DBHelper.GetProcedureErrorMsg()
                                   };

            int returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_RobotSettingModify", paras);
            message = Convert.ToString(paras.LastOrDefault().Value);

            return returnValue == 1;
        }

        public PagedList<RobotServerConfig> GetRobotServerConfig(int? gameID, int? serverID, int pageIndex, int pageSize)
        {
            int pageCount = 0;

            SqlParameter[] paras = {
                                     new SqlParameter("@intGameID",gameID),
                                     new SqlParameter("@intServerID",serverID),
                                     new SqlParameter("@intPageIndex",pageIndex),
                                     new SqlParameter("@intPageSize",pageSize),
                                     new SqlParameter("@intPageCount",pageCount),
                                   };

            paras.LastOrDefault().Direction = ParameterDirection.Output;
            var list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<RobotServerConfig>("dbo.[PrWeb_GetRobotSetting]", paras).ToList();
            pageCount = Convert.ToInt32(paras.LastOrDefault().Value);

            return new PagedList<RobotServerConfig>(list, pageIndex, pageSize, pageCount); ;
        }

        #endregion

        #region 白名單&黑名單

        public List<MonitorUser> GetTableBlackUser(string showName, int gameID, int? active)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intGameID", gameID),
                                    new SqlParameter("@chvShowNameID", showName),
                                    new SqlParameter("@tnyActive", active)
                                   };
            var _result = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<MonitorUser>("dbo.[PrPsWeb_Game_GetTableBlackUser]", paras).ToList();
            return _result;
        }

        public void UpdateTableBlackUser(MonitorUser model, ref string message)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intGameID", model.GameID),
                                    new SqlParameter("@chvShowNameID", model.ShowName),
                                    new SqlParameter("@tnyActive", model.Active),
                                    new SqlParameter("@chvUserIDList", model.UserIDList)
                                    };

            _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("[dbo].[PrPsWeb_Game_UpdateTableBlackUser]", paras);
            message = "操作成功！";
        }

        public List<MonitorUser> GetTableWhiteUser(string showName, int gameID, int? active)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intGameID", gameID),
                                    new SqlParameter("@chvShowNameID", showName),
                                    new SqlParameter("@tnyActive", active)
                                   };
            var _result = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<MonitorUser>("dbo.[PrPsWeb_Game_GetTableWhiteUser]", paras).ToList();
            return _result;
        }

        public void UpdateTableWhiteUser(MonitorUser model, ref string message)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intGameID", model.GameID),
                                    new SqlParameter("@chvShowNameID", model.ShowName),
                                    new SqlParameter("@tnyActive", model.Active),
                                    new SqlParameter("@chvUserIDList", model.UserIDList)
                                    };

            _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("[dbo].[PrPsWeb_Game_UpdateTableWhiteUser]", paras);
            message = "操作成功！";
        }

        #endregion

        #region 超端

        public PagedList<WinPrizeLog> GetWinPrizeLog(int serverID, int gameID, int tableNo, DateTime? beginTime, DateTime? endTime, int pageIndex, int pageSize)
        {
            int pageCount = 0;
            SqlParameter[] paras = {
                                    new SqlParameter("@intGameID",gameID),
                                    new SqlParameter("@intServerID",serverID),
                                    new SqlParameter("@intTableNo",tableNo),
                                    new SqlParameter("@dtBeginTime",beginTime),
                                    new SqlParameter("@intPageIndex",pageIndex),
                                    new SqlParameter("@intPageSize",pageSize),
                                    new SqlParameter("@intPageCount",pageCount)
                                   };
            paras.LastOrDefault().Direction = ParameterDirection.Output;
            var list = _dbFactory.GetDatabase(DBNameConfig.DBName_PerGameLog).ExecuteStoreProcedureForDataList<WinPrizeLog>("dbo.[PrWeb_GetWinPrizeLog]", paras).ToList();
            pageCount = Convert.ToInt32(paras.LastOrDefault().Value);

            var pageList = new PagedList<WinPrizeLog>();
            pageList.PageData = list;
            pageList.PageIndex = pageIndex;
            pageList.PageSize = pageSize;
            pageList.TotalCount = Convert.ToInt32(pageCount);
            pageList.TotalPage = Common.GetTotalPage(pageSize, Convert.ToInt32(pageCount));

            return pageList;
        }

        /// <summary>
        /// 取得輸贏排行榜
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public List<WinLostLeaderboard> GetWinLostLeaderboard(int operatorID, int? userID, DateTime? startTime, DateTime? endTime, int sortName, int sortLifting)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intAdminID ",operatorID),
                                    new SqlParameter("@intUserID", userID),
                                    new SqlParameter("@dtStartTime", startTime),
                                    new SqlParameter("@dtEndTime", endTime),
                                    new SqlParameter("@tnyColumns",sortName),
                                    new SqlParameter("@tnySort",sortLifting)
                                   };

            var _list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<WinLostLeaderboard>("dbo.PrWeb_Game_GetUserWinLostLeaderboard", paras).ToList();
            return _list;
        }

        public PagedList<ProtectNodeLog> GetProtectNode(int? userID, string ip, int pageIndex, int pageSize)
        {
            int pageCount = 0;
            SqlParameter[] paras = {
                    new SqlParameter("@intUserID", userID),
                    new SqlParameter("@chvNodeIPAddress",ip),
                    new SqlParameter("@dtBeginTime", null),
                    new SqlParameter("@dtEndTime", null),
                    new SqlParameter("@intPageIndex",pageIndex),
                    new SqlParameter("@intPageSize",pageSize),
                    new SqlParameter("@intPageCount",pageCount)
                    };

            paras.LastOrDefault().Direction = ParameterDirection.Output;
            var _list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<ProtectNodeLog>("dbo.[PrWeb_GetProtectNodeLog]", paras).ToList();

            pageCount = Convert.ToInt32(paras.LastOrDefault().Value);
            if (pageCount > int.MaxValue) pageCount = int.MaxValue;

            var pageList = new PagedList<ProtectNodeLog>()
            {
                PageData = _list,
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalCount = Convert.ToInt32(pageCount),
                TotalPage = Common.GetTotalPage(pageSize, Convert.ToInt32(pageCount))
            };

            return pageList;
        }

        /// <summary>
        /// 取得超端變更紀錄
        /// </summary>
        /// <param name="gameID"></param>
        /// <param name="type">1: 編輯獎池金額 2:增加放分金額 </param>
        /// <returns></returns>
        public List<SuperToolChangeLog> GetChangeLog(int gameID, int type)
        {
            SqlParameter[] paras = {
                    new SqlParameter("@GameID", gameID),
                    new SqlParameter("@intType",type)
                    };
            var _list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<SuperToolChangeLog>("dbo.[PrWeb_SuperTool_GetChangeLog]", paras).ToList();
            return _list;
        }

        /// <summary>
        /// 取得超端今日人數
        /// </summary>
        /// <param name="gameID"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public List<SuperToolTodayUser> GetTodayUserList(int gameID)
        {
            SqlParameter[] paras = {
                    new SqlParameter("@intGameID", gameID)
                    };
            var _list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<SuperToolTodayUser>("dbo.[PrWeb_SuperTool_GetTodayUser]", paras).ToList();
            return _list;
        }

        /// <summary>
        /// 加入監控
        /// </summary>
        /// <param name="model"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool AddVIPUser(SuperToolVIPUser model, ref string msg)
        {
            msg = "添加成功";
            var chvErrMsg = new SqlParameter
            {
                ParameterName = "@chvErrMsg",
                Value = msg,
                Direction = ParameterDirection.Output,
                Size = 32
            };
            
            SqlParameter[] paras = {
                    new SqlParameter("@intUserID", model.UserID),
                    new SqlParameter("@intAssignAmount", model.AssignAmount),
                    new SqlParameter("@intAssignRate", model.AssignRate),
                    new SqlParameter("@intStatus", model.Status),
                    new SqlParameter("@chvOperator", model.Operator),
                    chvErrMsg
                    };
            int _returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_SuperTool_AddVIPUser", paras);

            msg = chvErrMsg.Value.ToString();
            return _returnValue == 1;
        }

        /// <summary>
        /// 移除監控
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool DelVIPUser(int userID, ref string msg)
        {
            msg = "移除成功";
            SqlParameter[] paras = {
                    new SqlParameter("@intUserID", userID)
                    };
            int _returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_SuperTool_DelVIPUser", paras);

            if (_returnValue != 1) { msg = "移除失败"; }
            return _returnValue == 1;
        }

        /// <summary>
        /// 取得今日輸贏
        /// </summary>
        /// <param name="gameID"></param>
        /// <param name="online"></param>
        /// <returns></returns>
        public List<SuperToolTodayRanking> GetTodayRanking(int gameID, int? online)
        {
            SqlParameter[] paras = {
                    new SqlParameter("@intGameID", gameID),
                    new SqlParameter("@intOnline", online)
                    };
            var _list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<SuperToolTodayRanking>("dbo.[PrWeb_SuperTool_GetTodayRanking]", paras).ToList();
            return _list;
        }

        /// <summary>
        /// 取得監控名單
        /// </summary>
        /// <param name="gameID"></param>
        /// <returns></returns>
        public List<SuperToolVIPUser> GetVIPUser(int adminId, int gameID, int? userID)
        {
            SqlParameter[] paras = {
                    new SqlParameter("@intGameID", gameID),
                    new SqlParameter("@intUserID", userID),
                    new SqlParameter("@intAdminID", adminId)
                    };
            var _list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<SuperToolVIPUser>("dbo.[PrWeb_SuperTool_GetVIPUser]", paras).ToList();
            return _list;
        }

        /// <summary>
        /// 取得基本資料
        /// </summary>
        /// <param name="gameID"></param>
        /// <returns></returns>
        public SuperToolDetailInfo GetInfo(int gameID)
        {
            SqlParameter[] paras = {
                    new SqlParameter("@intGameID", gameID)
                    };
            var _result = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<SuperToolDetailInfo>("dbo.[PrWeb_SuperTool_GetInfo]", paras).FirstOrDefault();
            return _result;
        }

        /// <summary>
        /// 建立指定多局開獎
        /// </summary>
        /// <param name="tableNo"></param>
        /// <param name="gameID"></param>
        /// <param name="flag"></param>
        /// <param name="setting"></param>
        /// <param name="operatorUser"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool SetAssignReward(int tableNo, int gameID, int flag, string setting, string operatorUser, ref string msg)
        {
            msg = "添加成功";
            SqlParameter[] paras = {
                    new SqlParameter("@intGameID",gameID),
                    new SqlParameter("@intFlag", flag),
                    new SqlParameter("@intTableNo", tableNo),
                    new SqlParameter("@chvSetting", setting),
                    new SqlParameter("@chvOperator",operatorUser)
                    };
            int _returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_SuperTool_AddAssignReward", paras);

            if (_returnValue != 1) { msg = "添加失败"; }
            return _returnValue == 1;
        }

        /// <summary>
        /// 取消指定多局開獎
        /// </summary>
        /// <param name="tableNo"></param>
        /// <param name="gameID"></param>
        /// <param name="flag"></param>
        /// <param name="setting"></param>
        /// <param name="operatorUser"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool CancelAssignReward(int gameID, ref string msg)
        {
            msg = "取消成功";
            SqlParameter[] paras = {
                    new SqlParameter("@intGameID",gameID)
                    };
            int _returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_SuperTool_CancelAssignReward", paras);

            if (_returnValue != 1) { msg = "取消失败"; }
            return _returnValue == 1;
        }

        /// <summary>
        /// 取得開獎紀錄
        /// </summary>
        /// <param name="gameID"></param>
        /// <returns></returns>
        public List<SuperToolAssignReward> GetAssignReward(int gameID)
        {
            SqlParameter[] paras = {
                    new SqlParameter("@intGameID", gameID)
                    };
            var _result = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<SuperToolAssignReward>("dbo.[PrWeb_SuperTool_GetAssignReward]", paras).ToList();
            return _result;
        }

        /// <summary>
        /// 取得開獎明細
        /// </summary>
        /// <param name="gameID"></param>
        /// <returns></returns>
        public List<SuperToolAssignRewardDetail> GatAssignRewardDetail(int gameID)
        {
            SqlParameter[] paras = {
                    new SqlParameter("@intGameID", gameID)
                    };
            var _result = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<SuperToolAssignRewardDetail>("dbo.[PrWeb_SuperTool_GetAssignRewardDetail]", paras).ToList();
            return _result;
        }

        #endregion

        #region 捕魚超端

        /// <summary>
        /// 捕魚超端 取得在線用戶資訊
        /// </summary>
        /// <param name="serverID"></param>
        /// <returns></returns>
        public List<SuperToolTodayUser> GetFishOnlineUser(string showName)
        {
            SqlParameter[] paras = {
                    new SqlParameter("@chvShowName", (object)showName??DBNull.Value)
                    };
            var _list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<SuperToolTodayUser>("dbo.[PrWeb_SuperTool_Fish_GetOnlineUser]", paras).ToList();
            return _list;
        }

        /// <summary>
        /// 捕魚超端 取得个人水池开关
        /// </summary>
        /// <param name="serverID"></param>
        /// <returns></returns>
        public List<FishPlayerPool> GetFishPlayerPool()
        {
            var _list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<FishPlayerPool>("dbo.[PrWeb_SuperTool_Fish_GetPlayerPool]").ToList();
            return _list;
        }

        /// <summary>
        /// 捕魚超端 更新个人水池开关
        /// </summary>
        /// <param name="serverID"></param>
        /// <returns></returns>
        public bool FishUpdPlayerPool(int flagID, int status, ref string message)
        {
            SqlParameter[] paras = {
                    new SqlParameter("@intFlagID", (object)flagID??DBNull.Value),
                    new SqlParameter("@intStatus", (object)status??DBNull.Value)
                    };
            var _list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<FishPlayerPool>("dbo.[PrWeb_SuperTool_Fish_UpdPlayerPool]", paras).ToList();
            message = "操作成功！";
            return true;
        }

        public FishRanking GetFishRanking()
        {
            var _totalRanking = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<SuperToolFishRanking>("dbo.[PrWeb_SuperTool_Fish_GetTotalRanking]").ToList();
            var _todayRanking = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<SuperToolFishRanking>("dbo.[PrWeb_SuperTool_Fish_GetTodayRanking]").ToList();
            var _yestodayRanking = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<SuperToolFishRanking>("dbo.[PrWeb_SuperTool_Fish_GetYestodayRanking]").ToList();
            return new FishRanking()
            {
                TodayRanking = _todayRanking,
                TotalRanking = _totalRanking,
                YestodayRanking = _yestodayRanking
            };
        }

        public List<SuperToolFishSystemReward> GetFishSystemReward(int? flagID, int? status)
        {
            SqlParameter[] paras = {
                    new SqlParameter("@intFlagID", (object)flagID??DBNull.Value),
                    new SqlParameter("@intStatus", status)
                    };
            var _list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<SuperToolFishSystemReward>("dbo.[PrGsWeb_SuperTool_Fish_GetSystemReward]", paras).ToList();
            return _list;
        }

        public bool UpdFishSystemReward(SuperToolFishSystemReward model, ref string msg)
        {
            msg = "添加成功";
            SqlParameter[] paras = {
                    new SqlParameter("@intFlagID", model.FlagID),
                    new SqlParameter("@intAmount", model.Amount),
                    new SqlParameter("@intRate", model.Rate),
                    new SqlParameter("@intFishID", model.FishID),
                    new SqlParameter("@chvOperator", model.Operator)
                    };
            int _returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_SuperTool_Fish_UpdSystemReward", paras);

            if (_returnValue != 1) { msg = "添加失败"; }
            return _returnValue == 1;
        }

        public bool StartFishSystemReward(SuperToolFishSystemReward model, ref string msg)
        {
            msg = "添加成功";
            SqlParameter[] paras = {
                    new SqlParameter("@intFlagID", model.FlagID),
                    new SqlParameter("@intFishID", model.FishID),
                    new SqlParameter("@intStatus", model.Status),
                    new SqlParameter("@chvOperator", model.Operator)
                    };
            int _returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_SuperTool_Fish_StartSystemReward", paras);

            if (_returnValue != 1) { msg = "添加失败"; }
            return _returnValue == 1;
        }


        public List<SuperToolFishSystemRecord> GetFishSystemRecord(int? flagID)
        {
            SqlParameter[] paras = {
                    new SqlParameter("@intFlagID", (object)flagID??DBNull.Value)
                    };
            var _list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<SuperToolFishSystemRecord>("dbo.[PrWeb_SuperTool_Fish_GetData]", paras).ToList();
            return _list;
        }

        #endregion

        #region 炸金花超端

        /// <summary>
        /// 取得炸金花監控名單
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public List<SuperToolVIPUser_ZJH> GetZJHVIPUser(int? userID)
        {
            SqlParameter[] paras = {
                    new SqlParameter("@intUserID", userID)
                    };
            var _list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<SuperToolVIPUser_ZJH>("dbo.[PrWeb_SuperTool_ZJH_GetVIPUser]", paras).ToList();
            return _list;
        }

        public bool AddVIPUserZJH(SuperToolVIPUser_ZJH model, ref string msg)
        {
            msg = "添加成功";
            SqlParameter[] paras = {
                    new SqlParameter("@intUserID", model.UserID),
                    new SqlParameter("@intNextType", model.NextType),
                    new SqlParameter("@intNextWinLose", model.NextWinLose),
                    new SqlParameter("@intNextWinLoseNum", model.NextWinLoseNum),
                    new SqlParameter("@intNextTable", model.NextTable),
                    new SqlParameter("@intStatus", model.Status),
                    new SqlParameter("@chvOperator", model.Operator)
                    };
            int _returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_SuperTool_ZJH_AddVIPUser", paras);

            if (_returnValue != 1) { msg = "添加失败"; }
            return _returnValue == 1;
        }

        public bool UpdVIPUserZJH(SuperToolVIPUser_ZJH model, ref string msg)
        {
            msg = "添加成功";
            SqlParameter[] paras = {
                    new SqlParameter("@intUserID", model.UserID),
                    new SqlParameter("@intNextType", model.NextType),
                    new SqlParameter("@intNextWinLose", model.NextWinLose),
                    new SqlParameter("@intNextWinLoseNum", model.NextWinLoseNum),
                    new SqlParameter("@intNextTable", model.NextTable),
                    new SqlParameter("@intStatus", model.Status),
                    new SqlParameter("@chvOperator", model.Operator)
                    };
            int _returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_SuperTool_ZJH_UpdVIPUser", paras);

            if (_returnValue != 1) { msg = "添加失败"; }
            return _returnValue == 1;
        }

        public bool DelVIPUserZJH(int userID, ref string msg)
        {
            msg = "移除成功";
            SqlParameter[] paras = {
                    new SqlParameter("@intUserID", userID)
                    };
            int _returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_SuperTool_ZJH_DelVIPUser", paras);

            if (_returnValue != 1) { msg = "移除失败"; }
            return _returnValue == 1;
        }
        #endregion
    }
}
