using FW.Core.Helper;
using game.Entity.Area;
using game.Entity.Game;
using System.Collections.Generic;

namespace game.Service
{
    public interface IAreaService
    {
        /// <summary>
        /// 添加区域
        /// </summary>
        /// <param name="area"></param>
        /// <param name="message">返回消息</param>
        /// <returns></returns>
        bool AddArea(Area area, ref string message);
        /// <summary>
        /// 修改区域
        /// </summary>
        /// <param name="area"></param>
        /// <param name="message">返回消息</param>
        /// <returns></returns>
        bool ModifyArea(int areaID, int oldAreaID, string AreaName, string clientPlatform, int platformID, string memo, int adminID, int sort, string shareURL, long showConvertLimit, long showAllGameLimit, ref string message);

        bool DeleteArea(int areaID, ref string message);

        /// <summary>
        /// 返回所有区域信息
        /// </summary>
        /// <returns></returns>
        List<Area> GetArea(int? platformId, int? areaID);
        List<Area> GetAreaByEnable(int? platformId, int? areaID);

        Area GetAreaById(int areaID);

        /// <summary>
        /// 根据管理员能管理的渠道
        /// </summary>
        /// <param name="adminID"></param>
        /// <returns></returns>
        List<AreaAdminUser> GetAreaByAdminID(int adminID);

        /// <summary>
        /// 返回区域下面的游戏列表
        /// </summary>
        /// <param name="areaID"></param>
        /// <returns></returns>
        List<AreaGameList> GetAreaGameList(int areaID);

        /// <summary>
        /// 添加区域下的游戏
        /// </summary>
        /// <param name="m"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool AddGameArea(AreaGameList m, ref string message);

        /// <summary>
        /// 修改区域下游戏的排序
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        bool ModifyGameAreaSort(int id, int sort, ref string message);

        AreaGameList GetAreaGameById(int id);

        /// <summary>
        /// 删除区域下的游戏
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool DeleteGameArea(int id, ref string message);

        /// <summary>
        /// 查询运营平台
        /// </summary>
        /// <returns></returns>
        List<Platform> GetPlatform();

        Platform GetPlatformById(int id);

        Platform GetPlatformByShanFuID(string id);

        List<Company> GetCompany();

        Company GetCompanyById(int id);

        List<GameSort> GetPlatformGameSortById(int id);


        /// <summary>
        /// 查询管理员ID能管理的平台信息
        /// </summary>
        /// <param name="adminID"></param>
        /// <returns></returns>
        List<Platform> GetPlatformByAdminID(int adminID);
        /// <summary>
        /// 修改平台信息
        /// </summary>
        /// <param name="m"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool ModifyPlatform(Platform m, ref string message);

        /// <summary>
        /// 建立平台
        /// </summary>
        /// <param name="platformID">指定平台號</param>
        /// <param name="platformName">平台名稱</param>
        /// <param name="company">营运平台</param>
        /// <param name="areaNum">預設建立渠道数量</param>
        /// <param name="message">返回消息</param>
        /// <returns></returns>
        bool CreateNewPlatform(int platformID, string platformName, int company, string areaNum, ref string message);

        /// <summary>
        /// 同步平台
        /// </summary>
        /// <param name="platformID">平台號</param>
        /// <param name="areaID">同步參考的渠道號</param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool SyncAreaConfig(int platformID, int areaID, ref string message);

        bool UpdateAreaConfig(Area model, ref string message);

        bool UpdateGameSort(int platformID, int areaID, int gameID, int sort, int popular, int iconID, int status);
        bool AddAdminPlatform(int adminID, int platformID, int? areaID, ref string message);

        bool RemoveAdminPlatform(int adminID, ref string message);

        /// <summary>
        /// 查询渠道游戏
        /// </summary>
        PagedList<AreaGame> GetAreaGame(int? platformID, int? areaID, int pageIndex, int pageSize);


        /// <summary>
        /// 修改渠道游戏是否显示
        /// </summary>
        /// <returns></returns>
        bool SetAreaGameStatus(int id, bool status, ref string message);

        /// <summary>
        /// 新增渠道游戏
        /// </summary>
        /// <returns></returns>
        bool AddAreaGame(int platformID, int areaID, int gameID, int sort, bool status, int popular, ref string message);

        /// <summary>
        /// 更新渠道游戏
        /// </summary>
        /// <returns></returns>
        bool UpdateAreaGame(int ID, int sort, bool status, int popular, ref string message);

        /// <summary>
        /// 删除渠道游戏
        /// </summary>
        /// <returns></returns>
        bool DeleteAreaGame(int ID, ref string message);

        /// <summary>
        /// 取得平台廣告列表
        /// </summary>
        /// <param name="platformID"></param>
        /// <param name="areaID"></param>
        /// <returns></returns>
        List<Advertisement> GetAdvertisements(int? platformID, int? areaID);


        /// <summary>
        /// 新增平台廣告
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        bool AddAdvertisement(Advertisement model, ref string message);

        /// <summary>
        /// 修改平台廣告
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        bool UpdateAdvertisement(Advertisement model, ref string message);

        /// <summary>
        /// 刪除平台廣告
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        bool DeleteAdvertisement(int Id, ref string message);


        /// <summary>
        /// 取得平台活動列表
        /// </summary>
        /// <param name="platformID"></param>
        /// <param name="areaID"></param>
        /// <returns></returns>
        List<Campaign> GetCampaigns(int? platformID, int? areaID);


        /// <summary>
        /// 新增平台活動
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        bool AddCampaign(Campaign model, ref string message);

        /// <summary>
        /// 修改平台活動
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        bool UpdateCampaign(Campaign model, ref string message);

        /// <summary>
        /// 刪除平台活動
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        bool DeleteCampaign(int Id, ref string message);


        List<GameAliasName> GetGameAliasName();


        bool InsertGameAliasName(GameAliasName model, ref string message);
        bool UpdGameAliasName(GameAliasName model, ref string message);


        /// <summary>
        /// 增加平台
        /// </summary>
        /// <param name="platformID"></param>
        /// <param name="platformName"></param>
        /// <returns></returns>
        bool AddPlatform(int platformID, string platformName, ref string message);

    }
}
