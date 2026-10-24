using System.Collections.Generic;
using System.Web.Http;
using FW.Core.Helper;
using game.Entity;
using game.Entity.Area;
using game.Entity.Game;
using game.Infrastructure;
using game.Infrastructure.Helper;
using game.Service;

namespace game.GameAPI.Controllers
{
    public class AreaController : BaseAPIController
    {
        private IAreaService _iAreaService;


        public AreaController(IAreaService iAreaService)
        {
            _iAreaService = iAreaService;
        }
        /// <summary>
        /// 返回所有区域信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public List<Area> GetArea(int? platformId, int? areaID , bool isBriefOnly = false)
        {
            var query = _iAreaService.GetArea(platformId, areaID);

            //如果有指定要簡化回傳資料(html欄位不需要)
            if(isBriefOnly)
            {
                query.ForEach(x => DataSimplifyHelper.SimplifyAreaContentAndTips(x));
            }

            return query;
        }

        /// <summary>
        /// 返回 輕量簡要版 Area資訊
        /// </summary>
        [HttpGet]
        public List<AreaBriefModel> GetAreaBrief([FromUri]QueryAreaRequestViewModel filter)
        {
            var query = _iAreaService.GetAreaBrief(filter);
            return query;
        }
        

        [HttpGet]
        public List<Area> GetAreaByEnable(int? platformId, int? areaID)
        {
            return _iAreaService.GetAreaByEnable(platformId, areaID);
        }


        public Area GetAreaById(int areaID)
        {
            return _iAreaService.GetAreaById(areaID);
        }

        [HttpGet]
        public List<AreaAdminUser> GetAreaByAdminID(int adminID)
        {
            var query = _iAreaService.GetAreaByAdminID(adminID);
            return query;
        }

        [HttpGet]
        public APIResult DeleteArea(int areaID)
        {
            string message = "";
            bool result = _iAreaService.DeleteArea(areaID, ref message);
            return new APIResult(result, message);
        }

        [HttpPost]
        public APIResult AddArea(Area model)
        {
            string message = "";
            bool result = _iAreaService.AddArea(model, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public APIResult ModifyArea(int areaID, int oldAreaID, string AreaName, string clientPlatform, int platformID, string memo, int adminID, int sort, string shareURL, long showConvertLimit, long showAllGameLimit)
        {
            string message = "";
            bool result = _iAreaService.ModifyArea(areaID, oldAreaID, AreaName, clientPlatform, platformID, memo, adminID, sort, shareURL, showConvertLimit, showAllGameLimit, ref message);
            return new APIResult(result, message);
        }

        public List<AreaGameList> GetAreaGameList(int areaID)
        {
            return _iAreaService.GetAreaGameList(areaID);
        }
        [HttpPost]
        public APIResult AddGameArea(AreaGameList model)
        {
            string message = "";
            bool result = _iAreaService.AddGameArea(model, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public APIResult ModifyGameAreaSort(int id, int sort)
        {
            string message = "";
            bool result = _iAreaService.ModifyGameAreaSort(id, sort, ref message);
            return new APIResult(result, message);
        }
        public AreaGameList GetAreaGameById(int id)
        {
            return _iAreaService.GetAreaGameById(id);
        }
        [HttpGet]
        public APIResult DelGameAreaSort(int id)
        {
            string message = "";
            bool result = _iAreaService.DeleteGameArea(id, ref message);
            return new APIResult(result, message);
        }


        public List<Platform> GetPlatform(bool isBriefOnly = false)
        {
            var query = _iAreaService.GetPlatform();

            if(isBriefOnly)
            {
                DataSimplifyHelper.SimplifyPlatformContent(query);
            }

            return query;
        }

        public Platform GetPlatformById(int id)
        {
            return _iAreaService.GetPlatformById(id);
        }

        public List<Company> GetCompany()
        {
            return _iAreaService.GetCompany();
        }

        public Company GetCompanyById(int id)
        {
            return _iAreaService.GetCompanyById(id);
        }

        public List<GameSort> GetPlatformGameSortById(int id)
        {
            return _iAreaService.GetPlatformGameSortById(id);
        }

        public List<Platform> GetPlatformByAdminID(int adminID)
        {
            return _iAreaService.GetPlatformByAdminID(adminID);
        }

        [HttpPost]
        public APIResult ModifyPlatform(Platform m)
        {
            string msg = "";
            bool result = _iAreaService.ModifyPlatform(m, ref msg);
            return new APIResult(result, msg);
        }
        [HttpGet]
        public APIResult CreateCmdWorker(int commandType, string param)
        {
            string msg = "";
            bool result = _iAreaService.CreateCmdWorker(commandType, param, ref msg);
            return new APIResult(result, msg);
        }
        [HttpGet]
        public APIResult CreateNewPlatform(int platformID, string platformName, int company, string areaNum)
        {
            string msg = "";
            bool result = _iAreaService.CreateNewPlatform(platformID, platformName, company, areaNum, ref msg);
            return new APIResult(result, msg);
        }

        [HttpGet]
        public APIResult SyncAreaConfig(int platformID, int areaID)
        {
            string msg = "";
            bool result = _iAreaService.SyncAreaConfig(platformID, areaID, ref msg);
            return new APIResult(result, msg);
        }

        [HttpPost]
        public CRUDResultModel SyncAreaConfigToTargetAreaIDs([FromBody]SyncAreaRequestModel model)
        {
            model = model ?? new SyncAreaRequestModel();
            var result = _iAreaService.SyncAreaConfigToTargetAreaIDs(model);
            return result;
        }

        [HttpPost]
        public APIResult UpdateAreaConfig(Area model)
        {
            string msg = "";
            bool result = _iAreaService.UpdateAreaConfig(model, ref msg);
            return new APIResult(result, msg);
        }

        [HttpGet]
        public APIResult UpdateGameSort(int platformID, int areaID, int gameID, int sort, int popular, int iconID, int status)
        {
            string msg = "";
            bool result = _iAreaService.UpdateGameSort(platformID, areaID, gameID, sort, popular, iconID, status);
            return new APIResult(result, msg);
        }

        [HttpGet]
        public APIResult AddAdminPlatform(int adminID, int platformID, int? areaID)
        {
            string message = "";
            bool result = _iAreaService.AddAdminPlatform(adminID, platformID, areaID, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public APIResult RemoveAdminPlatform(int adminID)
        {
            string message = "";
            bool result = _iAreaService.RemoveAdminPlatform(adminID, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public PagedList<AreaGame> GetAreaGame(int? platformID, int? areaID, int pageIndex, int pageSize)
        {
            return _iAreaService.GetAreaGame(platformID, areaID, pageIndex, pageSize);
        }

        [HttpGet]
        public APIResult SetAreaGameStatus(int id, bool status)
        {
            string message = "";
            bool result = _iAreaService.SetAreaGameStatus(id, status, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public APIResult AddAreaGame(int platformID, int areaID, int gameID, int sort, bool status, int popular)
        {
            string message = "";
            bool result = _iAreaService.AddAreaGame(platformID, areaID, gameID, sort, status, popular, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public APIResult UpdateAreaGame(int ID, int sort, bool status, int popular)
        {
            string message = "";
            bool result = _iAreaService.UpdateAreaGame(ID, sort, status, popular, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public APIResult DeleteAreaGame(int ID)
        {
            string message = "";
            bool result = _iAreaService.DeleteAreaGame(ID, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public List<Advertisement> GetAdvertisements(int? platformID, int? areaID)
        {
            return _iAreaService.GetAdvertisements(platformID, areaID);
        }


        [HttpPost]
        public APIResult AddAdvertisement(Advertisement model)
        {
            string msg = "";
            bool result = _iAreaService.AddAdvertisement(model, ref msg);
            return new APIResult(result, msg);
        }

        [HttpPost]
        public APIResult UpdateAdvertisement(Advertisement model)
        {
            string msg = "";
            bool result = _iAreaService.UpdateAdvertisement(model, ref msg);
            return new APIResult(result, msg);
        }

        [HttpGet]
        public APIResult DeleteAdvertisement(int id)
        {
            string msg = "";
            bool result = _iAreaService.DeleteAdvertisement(id, ref msg);
            return new APIResult(result, msg);
        }

        [HttpGet]
        public List<GameAliasName> GetGameAliasName()
        {
            return _iAreaService.GetGameAliasName();
        }

        [HttpPost]
        public APIResult InsertGameAliasName(GameAliasName model)
        {
            string msg = "";
            bool result = _iAreaService.InsertGameAliasName(model, ref msg);
            return new APIResult(result, msg);
        }
        [HttpPost]
        public APIResult UpdGameAliasName(GameAliasName model)
        {
            string msg = "";
            bool result = _iAreaService.UpdGameAliasName(model, ref msg);
            return new APIResult(result, msg);
        }
        [HttpGet]
        public APIResult AddPlatform(int platformID, string platformName)
        {
            string msg = "";
            bool result = _iAreaService.AddPlatform(platformID, platformName, ref msg);
            return new APIResult(result, msg);
        }

        [HttpGet]
        public APIResult AutoAddArea(int platformID, int areaID, string areaName, int areaNum)
        {
            string message = "";
            bool result = _iAreaService.AutoAddArea(platformID, areaID, areaName, areaNum, ref message);
            return new APIResult(result, message);
        }
    }
}