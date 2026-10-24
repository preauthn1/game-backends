using FW.Core.Helper;
using game.Entity.Game;
using game.Entity.Money;
using game.Entity.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using game.Entity.Area;
using game.Infrastructure;

namespace game.Service
{
    /// <summary>
    /// 游戏业务接口
    /// </summary>
    public interface IGameService
    {
        /// <summary>
        /// 获取所有游戏
        /// </summary>
        /// <returns></returns>
        List<Game> GetGame();

        /// <summary>
        /// 根据ID获取游戏
        /// </summary>
        /// <param name="gameID"></param>
        /// <returns></returns>
        Game GetGameByID(int gameID);

        /// <summary>
        /// 设置游戏状态
        /// </summary>
        /// <param name="gameID"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        bool SetGameStatus(int gameID, byte status, ref string message);

        /// <summary>
        /// 获取游戏服务器
        /// </summary>
        /// <param name="gameID"></param>
        /// <param name="serverName"></param>
        /// <param name="ipAddress"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        List<GameServer> GetGameServer(int? gameID, int? serverID, string ipAddress);

        /// <summary>
        /// 根据ID获取游戏服务器
        /// </summary>
        /// <param name="serverID"></param>
        /// <returns></returns>
        GameServer GetGameServerByID(int serverID);

        /// <summary>
        /// 添加游戏服务器
        /// </summary>
        /// <param name="server"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool AddGameServer(GameServer server, ref string message);


        /// <summary>
        /// 修改游戏服务器
        /// </summary>
        /// <param name="server"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool ModifyGameServer(GameServer server, ref string message);

        /// <summary>
        /// 删除游戏服务器
        /// </summary>
        /// <param name="id"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool DelGameServer(int id, ref string message);

        /// <summary>
        /// 获取广场服务器
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        List<PlazaServer> GetPlazaServer(string ipAddress);
        /// <summary>
        /// 获取所有广场服务器
        /// </summary>
        /// <returns></returns>
        List<PlazaServer> GetAllPlazaServer();

        /// <summary>
        /// 根据ID获取广场服务器
        /// </summary>
        /// <param name="serverID"></param>
        /// <returns></returns>
        PlazaServer GetPlazaServerByID(int serverID);


        /// <summary>
        /// 获取广场在线用户
        /// </summary>
        /// <param name="serverID"></param>
        /// <param name="hdType"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        List<HallUserOnLine> GetHallOnLine(int? serverID, HDType? hdType, int? userID);

        /// <summary>
        /// 获取游戏服务器在线用户
        /// </summary>
        /// <param name="serverID"></param
        /// <param name="hdType"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        List<GameUserOnLine> GetGameOnLine(int? serverID, HDType? hdType, int? userID);

        /// <summary>
        /// 获取用户游戏信息
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="gameID"></param>
        /// <returns></returns>
        List<UserGameInfo> GetUserGameInfo(int userID, int? gameID);



        /// <summary>
        /// 把用戶踢出房間
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="serverID"></param>
        /// <returns></returns>
        bool KickoutGameUser(int? userID, int? serverID);


        /// <summary>
        /// 获取游戏记录
        /// </summary>
        /// <param name="serverID"></param>
        /// <param name="gameID"></param>
        /// <param name="userID"></param>
        /// <param name="ipAddress"></param>
        /// <param name="loginBeginTime"></param>
        /// <param name="loginEndTime"></param>
        /// <param name="logoutBeginTime"></param>
        /// <param name="logoutEndTime"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        PagedList<GameLog> GetGameLog(int? serverID, int? gameID, int? userID, string ipAddress, game.Entity.Users.HDType? hDtype, string hDcode,
            DateTime? loginBeginTime, DateTime? loginEndTime, DateTime? logoutBeginTime, DateTime? logoutEndTime, int? areaID, int pageIndex, int pageSize);

        /// <summary>
        /// 查询指定范围内用户的游戏统计信息
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <param name="beginTime">开始时间范围</param>
        /// <param name="endTime">结束时间范围</param>
        /// <returns></returns>
        List<game.Entity.Game.UserGameInfo> GetStatUserGameInfo(int userID, DateTime? beginTime, DateTime? endTime);



        #region 封锁IP
        /// <summary>
        /// 查询封锁IP
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="createBeginTime"></param>
        /// <param name="createEndTime"></param>
        /// <returns></returns>
        PagedList<ForbidIPAddress> GetForbidIPAddress(string ipAddress, DateTime? createBeginTime, DateTime? createEndTime, int pageIndex, int pageSize);
        /// <summary>
        /// 添加封锁IP
        /// </summary>
        /// <param name="forbidIPAddress"></param>
        /// <returns></returns>
        bool AddForbidIPAddress(ForbidIPAddress forbidIPAddress, ref string message);
        /// <summary>
        /// 删除封锁IP
        /// </summary>
        /// <param name="forbidIPAddress"></param>
        /// <returns></returns>
        bool DelForbidIPAddress(int id, ref string message);

        #endregion

        #region 游戏版本信息
        /// <summary>
        /// 大厅版本号
        /// </summary>
        /// <returns></returns>
        HallVersion GetHallVersion();
        /// <summary>
        /// 保存版本号
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool SaveHallVersion(string anVersion, string iosVersion, string anVersion2, string iosVersion2);
        /// <summary>
        /// 获取游戏服务器版本
        /// </summary>
        /// <returns></returns>
        List<game.Entity.Game.Game> GetGameListVersion();
        /// <summary>
        /// 修改服务器版本
        /// </summary>
        /// <param name="gameList"></param>
        /// <returns></returns>
        bool SaveGameListVersion(List<game.Entity.Game.Game> gameList);

        #endregion

        #region 系统配置

        SystemConfig GetSystemConfig();

        bool SaveSystemConfig(SystemConfig model, ref string message);

        string GetProtectNodeBackupIP();
        bool SaveProtectNodeBackupIP(string iplist, ref string message);

        List<PlatformDownload> GetDownloadList(int adminID, int? platformID);

        bool UpdatePlatformDownload(int adminID, PlatformDownload model, ref string msg);
        #endregion

        #region 游戏赢了通知配置

        /// <summary>
        /// 获取游戏赢了通知配置
        /// </summary>
        /// <returns></returns>
        List<WinAnnConfig> GetWinAnnConfig();

        /// <summary>
        /// 根据ID获取游戏赢了通知配置
        /// </summary>
        /// <param name="gameID"></param>
        /// <returns></returns>
        WinAnnConfig GetWinAnnConfigByID(int gameID);

        /// <summary>
        /// 添加游戏赢了通知配置
        /// </summary>
        /// <param name="config"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool AddWinAnnConfig(WinAnnConfig config, ref string message);

        /// <summary>
        /// 修改游戏赢了通知配置
        /// </summary>
        /// <param name="server"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool ModifyWinAnnConfig(WinAnnConfig config, ref string message);

        /// <summary>
        /// 删除游戏通赢了知配置
        /// </summary>
        /// <param name="id"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool DelWinAnnConfig(int id, ref string message);

        #endregion

        #region 机器人服务器配置

        /// <summary>
        /// 修改配置 (新删修)
        /// </summary>
        /// <param name="area"></param>
        /// <param name="message">返回消息</param>
        /// <returns></returns>
        bool UpdateRobotServer(int gameID, int serverID, string beginTime, string endTime, int minCount, int maxCount, int minLevel, int maxLevel, int status, ref string message);

        /// <summary>
        /// 获取配置
        /// </summary>
        /// <param name="gameID"></param>
        /// <param name="serverID"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        PagedList<RobotServerConfig> GetRobotServerConfig(int? gameID, int? serverID, int pageIndex, int pageSize);

        #endregion

        List<MonitorUser> GetTableBlackUser(string showName, int gameID, int? active);

        void UpdateTableBlackUser(MonitorUser model, ref string message);

        PagedList<WinPrizeLog> GetWinPrizeLog(int serverID, int gameID, int tableNo, DateTime? beginTime, DateTime? endTime, int pageIndex, int pageSize);

        /// <summary>
        /// 取得輸贏排行榜資訊
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        List<WinLostLeaderboard> GetWinLostLeaderboard(int operatorID, int? userID, DateTime? startTime, DateTime? endTime, int sortName, int sortLifting);

        /// <summary>
        /// 节点登入列表
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="ip"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        PagedList<ProtectNodeLog> GetProtectNode(int? userID, string ip, int pageIndex, int pageSize);

        List<MonitorUser> GetTableWhiteUser(string showName, int gameID, int? active);

        void UpdateTableWhiteUser(MonitorUser model, ref string message);

        /// <summary>
        /// 取得超端變更紀錄
        /// </summary>
        /// <param name="gameID"></param>
        /// <param name="type">1: 編輯獎池金額 2:增加放分金額 </param>
        /// <returns></returns>
        List<SuperToolChangeLog> GetChangeLog(int gameID, int type);

        /// <summary>
        /// 取得超端今日人數
        /// </summary>
        /// <param name="gameID"></param>
        /// <returns></returns>
        List<SuperToolTodayUser> GetTodayUserList(int gameID);

        /// <summary>
        /// 移除監控
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        bool DelVIPUser(int userID, ref string msg);

        /// <summary>
        /// 加入監控
        /// </summary>
        /// <param name="model"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        bool AddVIPUser(SuperToolVIPUser model, ref string msg);

        /// <summary>
        /// 取得今日輸贏排行榜
        /// </summary>
        /// <param name="gameID"></param>
        /// <param name="online"></param>
        /// <returns></returns>
        List<SuperToolTodayRanking> GetTodayRanking(int gameID, int? online);

        /// <summary>
        /// 取得監控名單
        /// </summary>
        /// <param name="gameID"></param>
        /// <returns></returns>
        List<SuperToolVIPUser> GetVIPUser(int gameID, int? userID);

        /// <summary>
        /// 取得基本資訊
        /// </summary>
        /// <param name="gameID"></param>
        /// <returns></returns>
        SuperToolDetailInfo GetInfo(int gameID);

        /// <summary>
        /// 建立指定局開獎資料
        /// </summary>
        /// <param name="tableNo"></param>
        /// <param name="gameID"></param>
        /// <param name="flag"></param>
        /// <param name="setting"></param>
        /// <param name="operatorUser"></param>
        /// <returns></returns>
        bool SetAssignReward(int tableNo, int gameID, int flag, string setting, string operatorUser, ref string msg);

        /// <summary>
        /// 取得開獎資料
        /// </summary>
        /// <param name="gameID"></param>
        /// <returns></returns>
        List<SuperToolAssignReward> GetAssignReward(int gameID);

        /// <summary>
        /// 取得開獎明細
        /// </summary>
        /// <param name="gameID"></param>
        /// <returns></returns>
        List<SuperToolAssignRewardDetail> GatAssignRewardDetail(int gameID);

        /// <summary>
        /// 取消指定局開獎
        /// </summary>
        /// <param name="gameID"></param>
        /// <returns></returns>
        bool CancelAssignReward(int gameID, ref string msg);

        /// <summary>
        /// 捕魚超端 取得在線用戶資訊
        /// </summary>
        /// <param name="serverID"></param>
        /// <returns></returns>
        List<SuperToolTodayUser> GetFishOnlineUser(string showName);

        /// <summary>
        /// 捕魚超端 取得个人水池开关
        /// </summary>
        /// <param name="serverID"></param>
        /// <returns></returns>
        List<FishPlayerPool> GetFishPlayerPool();

        /// <summary>
        /// 捕魚超端 更新个人水池开关
        /// </summary>
        /// <param name="serverID"></param>
        /// <returns></returns>
        bool FishUpdPlayerPool(int flagID, int status, ref string message);

        /// <summary>
        /// 取得捕魚輸贏榜
        /// </summary>
        /// <returns></returns>
        FishRanking GetFishRanking();

        /// <summary>
        /// 取得捕魚系統放分
        /// </summary>
        /// <param name="serverID"></param>
        /// <returns></returns>
        List<SuperToolFishSystemReward> GetFishSystemReward(int? flagID, int? status);

        /// <summary>
        /// 更新捕魚系統放分
        /// </summary>
        /// <param name="model"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        bool UpdFishSystemReward(SuperToolFishSystemReward model, ref string msg);

        /// <summary>
        /// 更新捕魚系統放分狀態
        /// </summary>
        /// <param name="model"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        bool StartFishSystemReward(SuperToolFishSystemReward model, ref string msg);

        /// <summary>
        /// 取得炸金花監控名單
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        List<SuperToolVIPUser_ZJH> GetZJHVIPUser(int? userID);

        /// <summary>
        /// 新增炸金花監控
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        bool AddVIPUserZJH(SuperToolVIPUser_ZJH model, ref string msg);

        /// <summary>
        /// 更新炸金花監控
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        bool UpdVIPUserZJH(SuperToolVIPUser_ZJH model, ref string msg);

        /// <summary>
        /// 刪除炸金花名單
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        bool DelVIPUserZJH(int userID, ref string msg);


        /// <summary>
        /// 取得捕鱼数据
        /// </summary>
        /// <param name="serverID"></param>
        /// <returns></returns>
        List<SuperToolFishSystemRecord> GetFishSystemRecord(int? flagID);


        List<ServerRelations> GetServerRelations(int? areaID, int? gameID);


        bool AddServerRelations(ServerRelations model, ref string message);

    }
}
