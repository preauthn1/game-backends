using System;
using System.Collections.Generic;
using System.Web.Http;
using game.Service;
using game.Entity.Game;
using game.Infrastructure;
using game.Entity.Users;
using FW.Core.Helper;
using game.Entity.Area;
using game.Entity;

namespace game.GameAPI.Controllers
{
    public class GameController : BaseAPIController
    {

        private IGameService _gameServie;
        public GameController(IGameService gameService)
        {
            _gameServie = gameService;
        }

        [HttpGet]
        public List<Game> GetGame()
        {
            return _gameServie.GetGame();
        }

        [HttpGet]
        public Game GetGameByID(int gameID)
        {
            return _gameServie.GetGameByID(gameID);
        }

        [HttpGet]
        public APIResult SetGameStatus(int gameID, byte status)
        {
            string message = "";
            bool result = _gameServie.SetGameStatus(gameID, status, ref message);
            return new APIResult(result, message);
        }


        [HttpGet]
        public APIResult KickoutGameUser(int? userID = null, int? serverID = null)
        {
            string message = "";
            bool result = _gameServie.KickoutGameUser(userID, serverID);
            return new APIResult(result, message);
        }

        [HttpGet]
        public List<GameServer> GetGameServer(int? gameID, int? serverID, string clientAddr , int? adminID = null)
        {
            return _gameServie.GetGameServer(gameID, serverID, clientAddr ,adminID);
        }

        [HttpGet]
        public List<PlatformGameServer> GetSuperToolServerList(int adminId, int platformId)
        {
            return _gameServie.GetSuperToolServerList(adminId, platformId);
        }

        [HttpGet]
        public GameServer GetGameServerByID(int serverID)
        {
            return _gameServie.GetGameServerByID(serverID);
        }

        [HttpPost]
        public APIResult AddGameServer(GameServer server)
        {
            string message = "";
            bool result = _gameServie.AddGameServer(server, ref message);
            return new APIResult(result, message, server.Id);
        }

        [HttpPost]
        public APIResult ModifyGameServer(GameServer server)
        {
            string message = "";
            bool result = _gameServie.ModifyGameServer(server, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public APIResult DelGameServer(int id)
        {
            string message = "";
            bool result = _gameServie.DelGameServer(id, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public List<PlazaServer> GetAllPlazaServer()
        {
            return _gameServie.GetAllPlazaServer();
        }

        [HttpGet]
        public List<PlazaServer> GetPlazaServer(string ipAddress)
        {
            return _gameServie.GetPlazaServer(ipAddress);
        }

        [HttpGet]
        public PlazaServer GetPlazaServerByID(int serverID)
        {
            return _gameServie.GetPlazaServerByID(serverID);
        }

        [HttpGet]
        public List<HallUserOnLine> GetHallOnLine(int? serverID, HDType? hdType = null, int? userID = null)
        {
            return _gameServie.GetHallOnLine(serverID, hdType, userID);
        }

        [HttpGet]
        public List<GameUserOnLine> GetGameOnLine(int? serverID = null, HDType? hdType = null, int? userID = null)
        {
            return _gameServie.GetGameOnLine(serverID, hdType, userID);
        }

        [HttpGet]
        public List<UserGameInfo> GameUserGameInfo(int userID, int? gameID = null)
        {
            return _gameServie.GetUserGameInfo(userID, gameID);
        }

        [HttpGet]
        public PagedList<UserGameInfoViewModel> GetUserGameInfoPaging([FromUri]QueryUserGameInfoRequestModel filter)
        {
            var query = _gameServie.GetUserGameInfoPaging(filter);
            return query;
        }
                
        [HttpGet]
        public PagedList<GameLog> GetGameLog(int? serverID = null, int? gameID = null, int? userID = null, string ipAddress = null, game.Entity.Users.HDType? hDtype = null, string hDcode = null,
            DateTime? loginBeginTime = null, DateTime? loginEndTime = null, DateTime? logoutBeginTime = null, DateTime? logoutEndTime = null, int? areaID = null, int pageIndex = 1, int pageSize = 20, int? AdminID = null)
        {
            return _gameServie.GetGameLog(serverID, gameID, userID, ipAddress, hDtype, hDcode, loginBeginTime, loginEndTime, logoutBeginTime, logoutEndTime, areaID, pageIndex, pageSize, AdminID);
        }

        #region 封锁IP
        [HttpGet]
        public PagedList<ForbidIPAddress> GetForbidIPAddress(string ipAddress, DateTime? createBeginTime, DateTime? createEndTime, int pageIndex = 1, int pageSize = 20)
        {
            return _gameServie.GetForbidIPAddress(ipAddress, createBeginTime, createEndTime, pageIndex, pageSize);
        }
        [HttpPost]
        public APIResult AddForbidIPAddress(ForbidIPAddress forbidIPAddress)
        {
            string message = string.Empty;
            var result = _gameServie.AddForbidIPAddress(forbidIPAddress, ref message);
            return new APIResult(result, message);
        }
        [HttpGet]
        public APIResult DelForbidIPAddress(int id)
        {
            string message = string.Empty;
            var result = _gameServie.DelForbidIPAddress(id, ref message);
            return new APIResult(result, message);
        }
        #endregion

        #region 游戏版本
        /// <summary>
        /// 获取广场服务器版本
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HallVersion GetHallVersion()
        {
            return _gameServie.GetHallVersion();
        }

        [HttpGet]
        public List<HallVersion> GetNewHallVersion()
        {
            return _gameServie.GetNewHallVersion();
        }

        [HttpGet]
        public List<HallSkin> GetHallSkin()
        {
            return _gameServie.GetHallSkin();
        }

        [HttpGet]
        public APIResult SaveHallVersion(string anVersion, string iosVersion, string anVersion2, string iosVersion2)
        {
            return new APIResult(_gameServie.SaveHallVersion(anVersion, iosVersion, anVersion2, iosVersion2));
        }
        [HttpGet]
        public APIResult SaveHallVersions(int platformID, int skin, string anVersion, string iosVersion, string anVersion2, string iosVersion2)
        {
            return new APIResult(_gameServie.SaveHallVersions(platformID, skin, anVersion, iosVersion, anVersion2, iosVersion2));
        }

        [HttpGet]
        public List<game.Entity.Game.Game> GetGameListVersion()
        {
            return _gameServie.GetGameListVersion();
        }

        [HttpGet]
        public List<game.Entity.Game.Game> GetGameListVersions()
        {
            return _gameServie.GetGameListVersion();
        }

        [HttpPost]
        public APIResult SaveGameListVersion(List<game.Entity.Game.Game> gameList)
        {
            return new APIResult(_gameServie.SaveGameListVersion(gameList));
        }

        #endregion

        #region 系統配置

        [HttpGet]
        public SystemConfig GetSystemConfig(int? platformId = null)
        {
            return _gameServie.GetSystemConfig(platformId);
        }

        [HttpPost]
        public APIResult SaveSystemConfig(SystemConfig model)
        {
            string message = string.Empty;
            var result = _gameServie.SaveSystemConfig(model, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public string GetProtectNodeBackupIP()
        {
            return _gameServie.GetProtectNodeBackupIP();
        }

        [HttpGet]
        public APIResult SaveProtectNodeBackupIP(string iplist)
        {
            string message = string.Empty;
            var result = _gameServie.SaveProtectNodeBackupIP(iplist, ref message);
            return new APIResult(result, message);
        }


        [HttpGet]
        public List<PlatformDownload> GetDownloadList(int adminID, int? platformID)
        {
            var result = _gameServie.GetDownloadList(adminID, platformID);
            return result;
        }

        [HttpPost]
        public APIResult UpdatePlatformDownload(int adminID, PlatformDownload model)
        {
            string message = string.Empty;
            var _result = _gameServie.UpdatePlatformDownload(adminID, model, ref message);
            return new APIResult(_result, message);
        }
        #endregion

        #region 游戏赢了通知配置

        [HttpGet]
        public List<WinAnnConfig> GetWinAnnConfig()
        {
            return _gameServie.GetWinAnnConfig();
        }

        [HttpGet]
        public WinAnnConfig GetWinAnnConfigByID(int gameID)
        {
            return _gameServie.GetWinAnnConfigByID(gameID);
        }

        [HttpPost]
        public APIResult AddWinAnnConfig(WinAnnConfig model)
        {
            string message = string.Empty;
            var result = _gameServie.AddWinAnnConfig(model, ref message);
            return new APIResult(result, message);
        }

        [HttpPost]
        public APIResult ModifyWinAnnConfig(WinAnnConfig model)
        {
            string message = string.Empty;
            var result = _gameServie.ModifyWinAnnConfig(model, ref message);
            return new APIResult(result, message);

        }

        [HttpGet]
        public APIResult DelWinAnnConfig(int id)
        {
            string message = string.Empty;
            var result = _gameServie.DelWinAnnConfig(id, ref message);
            return new APIResult(result, message);
        }

        #endregion

        #region 机器人服务器配置

        [HttpGet]
        public APIResult UpdateRobotServer(int gameID, int serverID, string beginTime, string endTime, int minCount, int maxCount, int minLevel, int maxLevel, int status)
        {
            string message = string.Empty;
            var result = _gameServie.UpdateRobotServer(gameID, serverID, beginTime, endTime, minCount, maxCount, minLevel, maxLevel, status, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public PagedList<RobotServerConfig> GetRobotServerConfig(int? gameID, int? serverID, int pageIndex, int pageSize)
        {
            return _gameServie.GetRobotServerConfig(gameID, serverID, pageIndex, pageSize);
        }

        #endregion

        #region 黑名單&白名單


        [HttpGet]
        public List<MonitorUser> GetTableBlackUser(string showName, int gameID, int? active)
        {
            return _gameServie.GetTableBlackUser(showName, gameID, active);
        }

        [HttpPost]
        public APIResult UpdateTableBlackUser(MonitorUser model)
        {
            string message = string.Empty;
            _gameServie.UpdateTableBlackUser(model, ref message);
            return new APIResult(true, message);
        }

        [HttpPost]
        public APIResult DeleteAndAddTableBlackUser(List<MonitorUser> modelList)
        {
            string message = string.Empty;
            modelList.ForEach(model =>
            {
                _gameServie.UpdateTableBlackUser(model, ref message);
            });

            return new APIResult(true, message);
        }

        [HttpGet]
        public List<MonitorUser> GetTableWhiteUser(string showName, int gameID, int? active)
        {
            return _gameServie.GetTableWhiteUser(showName, gameID, active);
        }

        [HttpPost]
        public APIResult UpdateTableWhiteUser(MonitorUser model)
        {
            string message = string.Empty;
            _gameServie.UpdateTableWhiteUser(model, ref message);
            return new APIResult(true, message);
        }

        [HttpPost]
        public APIResult DeleteAndAddTableWhiteUser(List<MonitorUser> modelList)
        {
            string message = string.Empty;
            modelList.ForEach(model =>
            {
                _gameServie.UpdateTableWhiteUser(model, ref message);
            });

            return new APIResult(true, message);
        }

        #endregion

        [HttpGet]
        public PagedList<WinPrizeLog> GetWinPrizeLog(int serverID, int gameID, int tableNo, DateTime? beginTime, DateTime? endTime, int pageIndex = 1, int pageSize = 20)
        {
            return _gameServie.GetWinPrizeLog(serverID, gameID, tableNo, beginTime, endTime, pageIndex, pageSize);
        }

        /// <summary>
        /// 节点登入列表
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="ip"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        public PagedList<ProtectNodeLog> GetProtectNode(int? userID, string ip, int pageIndex, int pageSize)
        {
            return _gameServie.GetProtectNode(userID, ip, pageIndex, pageSize);
        }

        [HttpGet]
        public List<SuperToolChangeLog> GetChangeLog(int gameID, int type)
        {
            return _gameServie.GetChangeLog(gameID, type);
        }

        [HttpGet]
        public List<SuperToolTodayUser> GetTodayUserList(int gameID)
        {
            return _gameServie.GetTodayUserList(gameID);
        }

        [HttpPost]
        public APIResult AddVIPUser(SuperToolVIPUser model)
        {
            string message = string.Empty;
            var _result = _gameServie.AddVIPUser(model, ref message);
            return new APIResult(_result, message);
        }

        [HttpGet]
        public APIResult DelVIPUser(int userID)
        {
            string message = string.Empty;
            var _result = _gameServie.DelVIPUser(userID, ref message);
            return new APIResult(_result, message);
        }

        [HttpGet]
        public List<SuperToolTodayRanking> GetTodayRanking(int gameID, int? online)
        {
            return _gameServie.GetTodayRanking(gameID, online);
        }

        [HttpGet]
        public List<SuperToolVIPUser> GetVIPUser(int adminID, int gameID, int? userID)
        {
            return _gameServie.GetVIPUser(adminID, gameID, userID);
        }

        [HttpGet]
        public SuperToolDetailInfo GetInfo(int gameID)
        {
            return _gameServie.GetInfo(gameID);
        }

        [HttpGet]
        public APIResult SetAssignReward(int tableNo, int gameID, int flag, string setting, string operatorUser)
        {
            string message = string.Empty;
            var _result = _gameServie.SetAssignReward(tableNo, gameID, flag, setting, operatorUser, ref message);
            return new APIResult(_result, message);
        }

        [HttpGet]
        public List<SuperToolAssignReward> GetAssignReward(int gameID)
        {
            return _gameServie.GetAssignReward(gameID);
        }

        [HttpGet]
        public List<SuperToolAssignRewardDetail> GatAssignRewardDetail(int gameID)
        {
            return _gameServie.GatAssignRewardDetail(gameID);
        }

        [HttpGet]
        public APIResult CancelAssignReward(int gameID)
        {
            string message = string.Empty;
            var _result = _gameServie.CancelAssignReward(gameID, ref message);
            return new APIResult(_result, message);
        }

        [HttpGet]
        public List<SuperToolTodayUser> GetFishOnlineUser(string showName)
        {
            return _gameServie.GetFishOnlineUser(showName);
        }

        [HttpGet]
        public List<FishPlayerPool> GetFishPlayerPool()
        {
            return _gameServie.GetFishPlayerPool();
        }

        [HttpGet]
        public APIResult FishUpdPlayerPool(int flagID, int status)
        {
            string message = string.Empty;
            var _result = _gameServie.FishUpdPlayerPool(flagID, status, ref message);
            return new APIResult(_result, message);
        }

        [HttpGet]
        public FishRanking GetFishRanking()
        {
            return _gameServie.GetFishRanking();
        }

        [HttpGet]
        public List<SuperToolFishSystemReward> GetFishSystemReward(int? flagID, int? status)
        {
            return _gameServie.GetFishSystemReward(flagID, status);
        }

        [HttpPost]
        public APIResult UpdFishSystemReward(SuperToolFishSystemReward model)
        {
            string message = string.Empty;
            var _result = _gameServie.UpdFishSystemReward(model, ref message);
            return new APIResult(_result, message);
        }

        [HttpPost]
        public APIResult StartFishSystemReward(SuperToolFishSystemReward model)
        {
            string message = string.Empty;
            var _result = _gameServie.StartFishSystemReward(model, ref message);
            return new APIResult(_result, message);
        }

        [HttpGet]
        public List<SuperToolVIPUser_ZJH> GetZJHVIPUser(int? userID)
        {
            return _gameServie.GetZJHVIPUser(userID);
        }

        [HttpPost]
        public APIResult AddVIPUserZJH(SuperToolVIPUser_ZJH model)
        {
            string message = string.Empty;
            var _result = _gameServie.AddVIPUserZJH(model, ref message);
            return new APIResult(_result, message);
        }

        [HttpPost]
        public APIResult UpdVIPUserZJH(SuperToolVIPUser_ZJH model)
        {
            string message = string.Empty;
            var _result = _gameServie.UpdVIPUserZJH(model, ref message);
            return new APIResult(_result, message);
        }

        [HttpGet]
        public APIResult DelVIPUserZJH(int userID)
        {
            string message = string.Empty;
            var _result = _gameServie.DelVIPUserZJH(userID, ref message);
            return new APIResult(_result, message);
        }

        [HttpGet]
        public List<SuperToolFishSystemRecord> GetFishSystemRecord(int? flagID)
        {
            return _gameServie.GetFishSystemRecord(flagID);
        }

        [HttpGet]
        public List<ServerRelations> GetServerRelations(int? areaID, int? gameID)
        {
            return _gameServie.GetServerRelations(areaID, gameID);
        }

        [HttpPost]
        public APIResult AddServerRelations(ServerRelations model)
        {
            string message = "";
            bool result = _gameServie.AddServerRelations(model, ref message);
            return new APIResult(result, message);
        }
    }
}