using System;
using System.Collections.Generic;
using System.Configuration;
using example.invalid;
using System.Linq;
using example.invalid;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using DataTables.Mvc;
using FW.Core.Helper.Enum;
using game.Entity;
using game.Entity.Area;
using game.Entity.Game;
using game.Entity.Money;
using game.Entity.Users;
using game.Infrastructure;
using Newtonsoft.Json;

namespace game.AdminSystem.Controllers.Game
{
    /// <summary>
    /// 游戏管理-服务器管理 
    /// </summary>
    public class GameMasterController : BaseController
    {
        
        public ActionResult GameServerList(int? platformId)
        {
            var adminId = SiteService.GetAdminUserID();
            ViewBag.operatorID = adminId;
            var list = new List<PlatformGameServer>();
            if (platformId == null)
            {
                platformId = 0;
            }
           
            list = SiteHelper.GetAPIResult<List<PlatformGameServer>>(string.Format("/Game/GetSuperToolServerList?adminId={0}&platformId={1}"
                , adminId, platformId));
            
            return View(list);
        }
        public ActionResult VIPUser()
        {
            var ctrl = new GameCommonController();
            return ctrl.VIPUser(0);

        }
        #region 渠道配置管理
        [NoAuthorization]
        public ActionResult ModifyAreaConfig()
        {
            var adminId = SiteService.GetAdminUserID();
            //var isAdvanced = UserService.CheckToken("Advanced", "post");
            var isAdvanced = true; //可調整排序
            var areaList = SiteHelper.GetAPIResult<List<AreaAdminUser>>($"/Area/GetAreaByAdminID?adminId={adminId}");
            var gameList = SiteHelper.GetAPIResult<List<Entity.Game.Game>>("/Game/GetGame");
            var advertisements = SiteHelper.GetAPIResult<List<Advertisement>>("Area/GetAdvertisements");
            var gameAliasName = SiteHelper.GetAPIResult<List<GameAliasName>>("Area/GetGameAliasName");

            var platformList = DataService.GetAllPlatform();
            var companyList = DataService.GetAllCompany();
            var popular = Enum.GetValues(typeof(Popular)).Cast<Popular>().ToList();
            var behavior = Enum.GetValues(typeof(Behavior)).Cast<Behavior>().ToList().Where(x=> ((int)x) < 99).ToList(); //編號100以上的是 新制的編號
            var status = Enum.GetValues(typeof(Status)).Cast<Status>().ToList();

            var popularJson = from pro in popular
                              select new
                              {
                                  Name = pro.GetDisplayName(),
                                  Id = pro
                              };

            var behaviorJson = from pro in behavior
                               select new
                               {
                                   Name = pro.GetDisplayName(),
                                   Id = pro
                               };

            var statusJson = from pro in status
                             select new
                             {
                                 Name = pro.GetDisplayName(),
                                 Id = pro
                             };

            var platforms = from pro in platformList
                            select new
                            {
                                pro.Name,
                                pro.Id
                            };

            var gameJson = from pro in gameList
                           select new
                           {
                               Name = pro.GameName,
                               pro.Id
                           };

            string filepath = Server.MapPath("~/Content/systemConfig.json");
            string json = GetFileJson(filepath);
            var query = from pro in areaList
                        orderby pro.AreaID
                        select new
                        {
                            Id = pro.AreaID,
                            pro.AreaName
                        };

            ViewBag.AreaList = query;
            ViewBag.SystemConfig = json;
            ViewBag.Advertisements = advertisements;
            ViewBag.GameAliasName = gameAliasName;
            ViewBag.PlatformList = platforms;
            ViewBag.CompanyList = companyList;
            ViewBag.BehaviorJson = behaviorJson;
            ViewBag.PopularJson = popularJson;
            ViewBag.GameJson = gameJson;
            ViewBag.StatusJson = statusJson;
            ViewBag.isAdvanced = isAdvanced;
            ViewBag.InsertAdvertisementUrl = $"{ConfigurationManager.AppSettings["uploadFileBaseUrl"]}/Upload/InsertAdvertisement";
            ViewBag.UploadAdvertisementUrl = $"{ConfigurationManager.AppSettings["uploadFileBaseUrl"]}/Upload/UpdateAdvertisement";
            ViewBag.DelAdvertisementUrl = $"{ConfigurationManager.AppSettings["uploadFileBaseUrl"]}/Upload/DeleteAdvertisement";
            ViewBag.WebUrl = $"{ConfigurationManager.AppSettings["uploadFileBaseUrl"]}";
            return View();
        }

        [NoAuthorization]
        public ActionResult GetAreaById(int id)
        {
            var area = SiteHelper.GetAPIResult<Area>($"/Area/GetAreaById?areaID={id}");
            return Json(new
            {
                area.Id,
                PlatformName = area.Platform.Name,
                area.PlatformID,
                area.AreaName,
                area.ClientPlatform,
                area.Memo,
                ShowConvertLimit = area.ShowConvertLimit.ToCash(),
                ShowVIPAgent = area.ShowVIPAgent.ToCash(),
                ShowAllGameLimit = area.ShowAllGameLimit.ToCash(),
                ShowCS = area.ShowCS.ToCash(),
                area.PlaytimeCS,
                area.PlaytimeConvert,
                area.PlaytimeVIPAgent,
                FAQContent = Regex.Replace(area.FAQContent, @"\r\n?|\n", "<br>"),
                PayTips = Regex.Replace(area.PayTips, @"\r\n?|\n", "<br>"),
                area.UpGradeMsg,
                area.AnnMsg,
                area.AnnMsgShow,
                area.ShopNameID,
                area.AgentNameID,
                area.ShowConvertPopup,
                ConvertPopupText = string.IsNullOrEmpty(area.ConvertPopupText) ? string.Empty : Regex.Replace(area.ConvertPopupText, @"\r\n?|\n", "<br>"),
                area.ShowConvertBindPhone,
                area.ShowAllGameBindPhone,
                area.ShowVIPAgentBindPhone,
                area.ShowCSBindPhone,
                area.SetDefault,
                GiftMoney = area.GiftMoney.ToCash(),
                GiftMoneyFlowLimit = area.GiftMoneyFlowLimit.ToCash(),
                DefaultMoney = area.DefaultMoney.ToCash(),
                DefaultMoneyFlowLimit = area.DefaultMoneyFlowLimit.ToCash(),
                AreaConfig = area.AreaConfig == null ? JsonConvert.DeserializeObject<AreaConfig>(
                                new JavaScriptSerializer().Serialize(new AreaConfig
                                {
                                    systemConfig = new AreaConfig.System
                                    {
                                        payConfig = new AreaConfig.PayConfig(),
                                        Info = new example.invalid(),
                                        JiuLiao = new AreaConfig.JiuLiao()
                                    }
                                })) : JsonConvert.DeserializeObject<AreaConfig>(area.AreaConfig)
            }, JsonRequestBehavior.AllowGet);
        }


        [NoAuthorization]
        public ActionResult ReloadAreaConfig(Area model)
        {
            var adminId = SiteService.GetAdminUserID();
            var areaList = SiteHelper.GetAPIResult<List<AreaAdminUser>>($"/Area/GetAreaByAdminID?adminId={adminId}");
            var areaFullList = SiteHelper.GetAPIResult<List<Area>>("/Area/GetArea").OrderByDescending(m => m.SetDefault);
            var gameList = SiteHelper.GetAPIResult<List<Entity.Game.Game>>("/Game/GetGame");
            var advertisements = SiteHelper.GetAPIResult<List<Advertisement>>("Area/GetAdvertisements");
            var gameAliasName = SiteHelper.GetAPIResult<List<GameAliasName>>("Area/GetGameAliasName");

            var gameJson = from pro in gameList
                           select new
                           {
                               Name = pro.GameName,
                               pro.Id
                           };

            //var queryFullList = from pro in areaFullList
            //            orderby pro.Id
            //            select new
            //            {
            //                pro.Id,
            //                PlatformName = pro.Platform.Name,
            //                pro.PlatformID,
            //                pro.AreaName,
            //                pro.ClientPlatform,
            //                pro.Memo,
            //                ShowConvertLimit = pro.ShowConvertLimit.ToCash(),
            //                ShowVIPAgent = pro.ShowVIPAgent.ToCash(),
            //                ShowAllGameLimit = pro.ShowAllGameLimit.ToCash(),
            //                ShowCS = pro.ShowCS.ToCash(),
            //                pro.PlaytimeCS,
            //                pro.PlaytimeConvert,
            //                pro.PlaytimeVIPAgent,
            //                FAQContent = Regex.Replace(pro.FAQContent, @"\r\n?|\n", "<br>"),
            //                PayTips = Regex.Replace(pro.PayTips, @"\r\n?|\n", "<br>"),
            //                pro.UpGradeMsg,
            //                pro.AnnMsg,
            //                pro.AnnMsgShow,
            //                pro.ShopNameID,
            //                pro.AgentNameID,
            //                pro.SetDefault,
            //                GiftMoney = pro.GiftMoney.ToCash(),
            //                DefaultMoney = pro.DefaultMoney.ToCash(),
            //                AreaConfig = pro.AreaConfig == null ? JsonConvert.DeserializeObject<AreaConfig>(
            //                    new JavaScriptSerializer().Serialize(new AreaConfig
            //                    {
            //                        systemConfig = new AreaConfig.System
            //                        {
            //                            payConfig = new AreaConfig.PayConfig(),
            //                            Info = new example.invalid(),
            //                            JiuLiao = new AreaConfig.JiuLiao()
            //                        }
            //                    })) : JsonConvert.DeserializeObject<AreaConfig>(pro.AreaConfig)
            //            };
            var query = from pro in areaList
                        orderby pro.AreaID
                        select new
                        {
                            Id = pro.AreaID,
                            pro.AreaName
                        };
            return Json(new
            {
                //areaFullList = queryFullList,
                areaList = query,
                advertisements,
                gameAliasName,
                gameJson,
            }, JsonRequestBehavior.AllowGet);
        }

        [NoAuthorization]
        [ValidateInput(false)]
        public ActionResult CreateNewPlatform(int platformID, string platformName, int company, string areaNum)
        {
            var result = SiteHelper.GetAPIResult<APIResult>($"/Area/CreateNewPlatform?platformID={platformID}&platformName={platformName}&company={company}&areaNum={areaNum}");
            return Json(new
            {
                Message = result.Message,
                Result = result.Result
            });
        }

        [NoAuthorization]
        [ValidateInput(false)]
        public ActionResult CreateCmdWorker(int commandType, string param)
        {
            var p = Server.UrlEncode(param);
            var result = SiteHelper.GetAPIResult<APIResult>($"/Area/CreateCmdWorker?commandType={commandType}&param={p}");
            return Json(new
            {
                Message = result.Message,
                Result = result.Result
            }, JsonRequestBehavior.AllowGet);
        }

        [NoAuthorization]
        [ValidateInput(false)]
        public ActionResult SyncAreaConfig(int platformID, int areaID)
        {
            var result = SiteHelper.GetAPIResult<APIResult>($"/Area/SyncAreaConfig?platformID={platformID}&areaID={areaID}");
            return Json(new
            {
                Message = result.Message,
                Result = result.Result
            });
        }

        [NoAuthorization]
        [ValidateInput(false)]
        public ActionResult InsertAreaConfig(Area model, string GameSortList)
        {
            if (!string.IsNullOrEmpty(GameSortList))
            {
                var json_serializer = new JavaScriptSerializer();
                var gameSortList = json_serializer.Deserialize<List<GameSort>>(GameSortList);

                foreach (GameSort game in gameSortList)
                {
                    var result = SiteHelper.GetAPIResult<APIResult>($"/Area/UpdateGameSort?platformID={model.PlatformID}&areaID={model.Id}&gameID={game.GameID}&sort={game.Sort}&popular={game.Popular}&iconID={game.IconID}&status={game.Status}");
                }
            }
            var ret = SiteHelper.PostAPIResult<Area>("Area/AddArea", model);
            return Json(ret, JsonRequestBehavior.AllowGet);
        }

        [NoAuthorization]
        [ValidateInput(false)]
        public ActionResult UpdateAreaConfig(Area model, string GameSortList)
        {
            if (!string.IsNullOrEmpty(GameSortList))
            {
                var json_serializer = new JavaScriptSerializer();
                var gameSortList = json_serializer.Deserialize<List<GameSort>>(GameSortList);

                foreach (GameSort game in gameSortList)
                {
                    var result = SiteHelper.GetAPIResult<APIResult>($"/Area/UpdateGameSort?platformID={model.PlatformID}&areaID={model.Id}&gameID={game.GameID}&sort={game.Sort}&popular={game.Popular}&iconID={game.IconID}&status={game.Status}");
                }
            }

            var queryArea = SiteHelper.GetAPIResult<Area> ($"/Area/GetAreaById?areaID={model.Id}");

            //如果是站長這邊的渠道管理進行儲存。有部分欄位不會更新(再覆蓋回原本的值)
            if (queryArea != null)
            {
                model.IsGameCategoryFilterVisible = queryArea.IsGameCategoryFilterVisible;
            }

            var ret = SiteHelper.PostAPIResult<Area>("Area/UpdateAreaConfig", model);
            return Json(ret, JsonRequestBehavior.AllowGet);
        }

        [NoAuthorization]
        public ActionResult GetAreaGameSortById(int id)
        {
            var gamesSort = SiteHelper.GetAPIResult<List<GameSort>>($"/Area/GetPlatformGameSortById?id={id}");
            return Json(gamesSort, JsonRequestBehavior.AllowGet);
        }

        [NoAuthorization]
        public ActionResult InsertGameAliasName(GameAliasName model)
        {
            var result = SiteHelper.PostAPIResult<GameAliasName>("Area/InsertGameAliasName", model);
            return Json(result);
        }

        [NoAuthorization]
        public ActionResult UpdGameAliasName(GameAliasName model)
        {
            var result = SiteHelper.PostAPIResult<GameAliasName>("Area/UpdGameAliasName", model);
            return Json(result);
        }

        #region 系统配置管理

        public ActionResult ModifySystemConfig(int? platformId = null)
        {
            var adminID = SiteService.GetAdminUserID();
            ViewBag.operatorID = adminID;
            var systemConfig = SiteHelper.GetAPIResult<SystemConfig>($"Game/GetSystemConfig?platformId={platformId}");
            ViewBag.platformid = platformId;
            return View(systemConfig);
        }

        [HttpPost]
        public ActionResult ModifySystemConfig(SystemConfig model)
        {
            var ret = SiteHelper.PostAPIResult<SystemConfig>("Game/SaveSystemConfig", model);
            return Alert(ret.Message);
        }
        #endregion


        public string GetFileJson(string filepath)
        {
            string json = string.Empty;
            using (FileStream fs = new FileStream(filepath, FileMode.Open, example.invalid.FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader sr = new StreamReader(fs, Encoding.GetEncoding("gb2312")))
                {
                    json = sr.ReadToEnd().ToString();
                }
            }
            return json;
        }

    }
    #endregion
}