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
    public class GameManageController : BaseController
    {

        /// <summary>
        /// 3-游戏管理
        /// </summary>
        /// <returns></returns>
        public ActionResult GameList()
        {
            return View(SiteHelper.GetAPIResult<List<game.Entity.Game.Game>>("/Game/GetGame"));
        }

        public ActionResult SetGameStatus(int gameID, byte status)
        {
            var ret = SiteHelper.GetAPIResult<APIResult>(string.Format("Game/SetGameStatus?gameID={0}&status={1}", gameID, status));
            return Alert(ret.Message);
        }

        /// <summary>
        /// 3-房间管理 
        /// </summary>
        /// <returns></returns>
        public ActionResult GameServerList(int? gameID, string clientAddr, int? serverID)
        {
            return View(SiteHelper.GetAPIResult<List<GameServer>>(string.Format("/Game/GetGameServer?gameID={0}&serverID={1}&clientAddr={2}"
                , gameID, serverID, clientAddr)));
        }

        [NoAuthorization]
        public ActionResult GameSelectServerList()
        {
            var _gameServers = SiteHelper.GetAPIResult<List<GameServer>>("/Game/GetGameServer");
            return Json(_gameServers, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 3-新增房间 
        /// </summary>
        public ActionResult AddGameServer()
        {
            return View();
        }

        /// <summary>
        /// 3-新增房间  4-保存 
        /// </summary>
        [HttpPost]
        public ActionResult AddGameServer(GameServer server)
        {
            server.LastUpdateTime = DateTime.Now;

            APIResult api = SiteHelper.PostAPIResult<GameServer>("/Game/AddGameServer", server);

            if (api.Result)
            {
                return Confirm("/GameManage/AddGameServer", "/GameManage/GameServerList", "是否继续新增房间");
            }
            else
            {
                return Alert(api.Message);
            }
        }

        /// <summary>
        /// 3-房间管理 4-修改房间 
        /// </summary>
        public ActionResult EditGameServer(int id)
        {
            GameServer server = SiteHelper.GetAPIResult<GameServer>("Game/GetGameServerByID?serverID=" + id);
            if (server == null)
            {
                return Alert("房间不存在", Request.UrlReferrer.ToString());
            }
            else
            {
                return View(server);
            }
        }

        /// <summary>
        /// 3-房间管理 4-修改房间  5-保存
        /// </summary>
        [HttpPost]
        public ActionResult EditGameServer(GameServer server)
        {
            var entity = SiteHelper.GetAPIResult<GameServer>("Game/GetGameServerByID?serverID=" + server.Id);

            entity.GameID = server.GameID;
            entity.Game = null;
            entity.ServerName = server.ServerName;
            entity.ShowName = server.ShowName;
            entity.ClientAddr = server.ClientAddr;
            entity.NotifyAddr = server.NotifyAddr;
            entity.HttpAddr = server.HttpAddr;
            entity.Money = server.Money;
            entity.LastUpdateTime = DateTime.Now;

            var api = SiteHelper.PostAPIResult<GameServer>("Game/ModifyGameServer", entity);
            if (api.Result)
            {
                return ConfirmReturnList("/GameManage/GameServerList");
            }
            else
            {
                return Alert(api.Message);
            }
        }

        /// <summary>
        /// 3-房间管理 4-删除房间
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Del(int id)
        {
            var api = SiteHelper.GetAPIResult<APIResult>("Game/DelGameServer?id=" + id);
            if (api.Result)
            {
                return Alert("操作成功", Request.UrlReferrer.ToString());
            }
            else
            {
                return Alert(api.Message);
            }
        }

        public ActionResult GetUserInfoById(int userID)
        {
            var userInfo = SiteHelper.GetAPIResult<UserInfo>(string.Format("/User/GetUserByID?userID={0}", userID));
            var userCash = SiteHelper.GetAPIResult<Cash>(string.Format("/Money/GetUserCash?userID={0}", userID));
            var userBank = SiteHelper.GetAPIResult<Bank>(string.Format("/Money/GetUserBank?userID={0}", userID));
            return Json(new
            {
                userInfo.Id,
                userInfo.FaceID,
                userInfo.LoginName,
                userInfo.GradeID,
                CreateTime = userInfo.CreateTime.ToString("yyyy-MM-dd HH:mm"),
                LastLogonTime = userInfo.LastLogonTime.ToString("yyyy-MM-dd HH:mm"),
                userInfo.SumLoginTimes,
                RealName = string.IsNullOrWhiteSpace(userInfo.RealName) ? "未绑定" : userInfo.RealName,
                AlipayAccount = string.IsNullOrWhiteSpace(userInfo.AlipayAccount) ? "未绑定" : userInfo.AlipayAccount,
                AreaName = userInfo.Area.AreaName,
                PlatformName = userInfo.Area.Platform.Name,
                AllCoinAmount = (userCash == null ? 0 : userCash.Amount) + (userBank == null ? 0 : userBank.Amount),
                userInfo.SumCharge,
                userInfo.SumConvert,
                userInfo.SumTax
            }, JsonRequestBehavior.AllowGet);
        }

        #region 平台配置管理

        public ActionResult ModifyPlatformConfig(int id = 0)
        {
            var list = SiteHelper.GetAPIResult<List<Platform>>("/Area/GetPlatform");
            if (id == 0)
            {
                if (list.Count > 0)
                    return Redirect("/GameManage/ModifyPlatformConfig/" + list[0].Id);
                return Content("暂无平台数据");
            }

            var platform = SiteHelper.GetAPIResult<Platform>("/Area/GetPlatformById?id=" + id);
            var gamesSort = SiteHelper.GetAPIResult<List<GameSort>>($"/Area/GetPlatformGameSortById?id={id}");

            ViewBag.PlatformList = list;
            ViewBag.GamesSort = gamesSort;
            ViewBag.UploadAdvertisementUrl = $"{ConfigurationManager.AppSettings["uploadFileBaseUrl"]}/Upload/NewOrEditAdvertisement";
            ViewBag.DelAdvertisementUrl = $"{ConfigurationManager.AppSettings["uploadFileBaseUrl"]}/Upload/DeleteAdvertisement";
            ViewBag.WebUrl = $"{ConfigurationManager.AppSettings["uploadFileBaseUrl"]}";

            return View(platform);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult ModifyPlatformConfig(Platform model, string GameSortList)
        {
            if (!string.IsNullOrEmpty(GameSortList))
            {
                var json_serializer = new JavaScriptSerializer();
                var gameSortList = json_serializer.Deserialize<List<GameSort>>(GameSortList);

                foreach (GameSort game in gameSortList)
                {
                    var result = SiteHelper.GetAPIResult<APIResult>($"/Area/UpdateGameSort?platformID={model.Id}&gameID={game.GameID}&sort={game.Sort}");
                }
            }
            var ret = SiteHelper.PostAPIResult<Platform>("Area/ModifyPlatform", model);
            return Alert(ret.Message);
        }

        public ActionResult GetAdvertisements([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            var draw = requestModel.Draw;
            var start = requestModel.Start;
            var length = requestModel.Length;
            var pageIndex = Math.Ceiling((decimal)start / length) + 1;
            var advertisements = SiteHelper.GetAPIResult<List<Advertisement>>("Area/GetAdvertisements");

            return Json(new
            {
                draw = draw,
                recordsTotal = advertisements.Count,
                recordsFiltered = advertisements.Count,
                success = true,
                data = advertisements
            });
        }

        public ActionResult GetCampaigns([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            var draw = requestModel.Draw;
            var start = requestModel.Start;
            var length = requestModel.Length;
            var pageIndex = Math.Ceiling((decimal)start / length) + 1;
            var campaigns = SiteHelper.GetAPIResult<List<Campaign>>("Area/GetCampaigns");

            return Json(new
            {
                draw = draw,
                recordsTotal = campaigns.Count,
                recordsFiltered = campaigns.Count,
                success = true,
                data = campaigns
            });
        }

        [HttpPost]
        public ActionResult AddPlatform(int Id, int platformID, string platformName)
        {
            var result = SiteHelper.GetAPIResult<APIResult>(string.Format("Area/AddPlatform?platformID={0}&platformName={1}", platformID, platformName));

            return Alert(result.Message);
        }
        #endregion




        #region 渠道配置管理
        public ActionResult ModifyAreaConfig()
        {
            var isAdvanced = UserService.CheckToken("Advanced", "post");
            var areaList = SiteHelper.GetAPIResult<List<Area>>("/Area/GetAreaByEnable").Where(w => w.AreaStatus==true).OrderByDescending(m => m.SetDefault);
            var gameList = SiteHelper.GetAPIResult<List<Entity.Game.Game>>("/Game/GetGame");
            var advertisements = SiteHelper.GetAPIResult<List<Advertisement>>("Area/GetAdvertisements");
            var gameAliasName = SiteHelper.GetAPIResult<List<GameAliasName>>("Area/GetGameAliasName");
            

            var platformList = DataService.GetAllPlatform();
            var companyList = DataService.GetAllCompany();
            var popular = Enum.GetValues(typeof(Popular)).Cast<Popular>().ToList();
            var behavior = Enum.GetValues(typeof(Behavior)).Cast<Behavior>().ToList();
            var status = Enum.GetValues(typeof(Status)).Cast<Status>().ToList();

            var campaigns = SiteHelper.GetAPIResult<List<Campaign>>("Area/GetCampaigns");
            var campaignTag = Enum.GetValues(typeof(CampaignTag)).Cast<CampaignTag>().ToList();
            var campaignBehavior = Enum.GetValues(typeof(CampaignBehavior)).Cast<CampaignBehavior>().ToList();
            var campaignStatus = Enum.GetValues(typeof(CampaignStatus)).Cast<CampaignStatus>().ToList();

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

            var campaignTagJson = from pro in campaignTag
                               select new
                               {
                                   Name = pro.GetDisplayName(),
                                   Id = pro
                               };

            var campaignBehaviorJson = from pro in campaignBehavior
                                       select new
                               {
                                   Name = pro.GetDisplayName(),
                                   Id = pro
                               };

            var campaignStatusJson = from pro in campaignStatus
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
                        select new
                        {
                            pro.Id,
                            pro.AreaName,
                            pro.SetDefault
                        };

            ViewBag.AreaList = query;
            ViewBag.SystemConfig = json;
            ViewBag.Advertisements = advertisements;
            ViewBag.GameAliasName = gameAliasName;
            ViewBag.PlatformList = platforms;
            ViewBag.CompanyList = companyList;
            ViewBag.BehaviorJson = behaviorJson;
            ViewBag.StatusJson = statusJson;
            ViewBag.CampaignTagJson = campaignTagJson;
            ViewBag.CampaignBehaviorJson = campaignBehaviorJson;
            ViewBag.CampaignStatusJson = campaignStatusJson;
            ViewBag.PopularJson = popularJson;
            ViewBag.GameJson = gameJson;
            ViewBag.isAdvanced = isAdvanced;
            ViewBag.InsertAdvertisementUrl = $"{ConfigurationManager.AppSettings["uploadFileBaseUrl"]}/Upload/InsertAdvertisement";
            ViewBag.UploadAdvertisementUrl = $"{ConfigurationManager.AppSettings["uploadFileBaseUrl"]}/Upload/UpdateAdvertisement";
            ViewBag.DelAdvertisementUrl = $"{ConfigurationManager.AppSettings["uploadFileBaseUrl"]}/Upload/DeleteAdvertisement";

            ViewBag.Campaigns = campaigns;
            ViewBag.InsertCampaignUrl = $"{ConfigurationManager.AppSettings["uploadFileBaseUrl"]}/Upload/InsertCampaign";
            ViewBag.UpdateCampaignUrl = $"{ConfigurationManager.AppSettings["uploadFileBaseUrl"]}/Upload/UpdateCampaign";
            ViewBag.DelCampaignUrl = $"{ConfigurationManager.AppSettings["uploadFileBaseUrl"]}/Upload/DeleteCampaign";

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
                area.ShowCampaignAtLogin,
                area.ShowFirstDepositAtLogin,
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
                DefaultMoney = area.DefaultMoney.ToCash(),
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
            var areaList = SiteHelper.GetAPIResult<List<Area>>("/Area/GetArea").OrderByDescending(m => m.SetDefault);
            var gameList = SiteHelper.GetAPIResult<List<Entity.Game.Game>>("/Game/GetGame");
            var advertisements = SiteHelper.GetAPIResult<List<Advertisement>>("Area/GetAdvertisements");
            var campaigns = SiteHelper.GetAPIResult<List<Campaign>>("Area/GetCampaigns");
            var gameAliasName = SiteHelper.GetAPIResult<List<GameAliasName>>("Area/GetGameAliasName");
            

            var gameJson = from pro in gameList
                           select new
                           {
                               Name = pro.GameName,
                               pro.Id
                           };

            var query = from pro in areaList
                        select new
                        {
                            pro.Id,
                            PlatformName = pro.Platform.Name,
                            pro.PlatformID,
                            pro.AreaName,
                            pro.ClientPlatform,
                            pro.Memo,
                            ShowConvertLimit = pro.ShowConvertLimit.ToCash(),
                            ShowVIPAgent = pro.ShowVIPAgent.ToCash(),
                            ShowAllGameLimit = pro.ShowAllGameLimit.ToCash(),
                            ShowCS = pro.ShowCS.ToCash(),
                            pro.PlaytimeCS,
                            pro.PlaytimeConvert,
                            pro.PlaytimeVIPAgent,
                            FAQContent = Regex.Replace(pro.FAQContent, @"\r\n?|\n", "<br>"),
                            PayTips = Regex.Replace(pro.PayTips, @"\r\n?|\n", "<br>"),
                            pro.UpGradeMsg,
                            pro.AnnMsg,
                            pro.AnnMsgShow,
                            pro.ShowCampaignAtLogin,
                            pro.ShowFirstDepositAtLogin,
                            pro.ShopNameID,
                            pro.AgentNameID,
                            pro.SetDefault,
                            GiftMoney = pro.GiftMoney.ToCash(),
                            DefaultMoney = pro.DefaultMoney.ToCash(),
                            AreaConfig = pro.AreaConfig == null ? JsonConvert.DeserializeObject<AreaConfig>(
                                new JavaScriptSerializer().Serialize(new AreaConfig
                                {
                                    systemConfig = new AreaConfig.System
                                    {
                                        payConfig = new AreaConfig.PayConfig(),
                                        Info = new example.invalid(),
                                        JiuLiao = new AreaConfig.JiuLiao()
                                    }
                                })) : JsonConvert.DeserializeObject<AreaConfig>(pro.AreaConfig)
                        };
            return Json(new
            {
                areaList = query,
                advertisements,
                campaigns,
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

        #endregion

        #region 系统配置管理

        public ActionResult ModifySystemConfig()
        {
            var systemConfig = SiteHelper.GetAPIResult<SystemConfig>("Game/GetSystemConfig");
            return View(systemConfig);
        }

        [HttpPost]
        public ActionResult ModifySystemConfig(SystemConfig model)
        {
            var ret = SiteHelper.PostAPIResult<SystemConfig>("Game/SaveSystemConfig", model);
            return Alert(ret.Message);
        }


        public ActionResult ModifyProtectNodeConfig()
        {
            var iplist = SiteHelper.GetAPIResult<string>("Game/GetProtectNodeBackupIP");
            ViewBag.iplist = iplist;
            return View();
        }

        [HttpPost]
        public ActionResult ModifyProtectNodeConfig(string ProtectNodeBackupIP)
        {
            ProtectNodeBackupIP = ProtectNodeBackupIP.Trim();
            if (ProtectNodeBackupIP.Length != 0)
            {
                string[] iplist = ProtectNodeBackupIP.Split(',');
                for (int i = 0; i < iplist.Length; i++)
                {
                    if (!CheckIP(iplist[i]))
                    {
                        return Alert("IP格式错误");
                    }
                }
            }

            var ret = SiteHelper.GetAPIResult<APIResult>(string.Format("Game/SaveProtectNodeBackupIP?iplist={0}", ProtectNodeBackupIP));
            return Alert(ret.Message);
        }

        /// <summary>
        /// 檢查 IP 是否合法
        /// </summary>
        private bool CheckIP(string strPattern)
        {
            if (strPattern == "::1" || strPattern == "0.0.0.0")
                return true;

            // regular: ^\d{1,3}[\.]\d{1,3}[\.]\d{1,3}[\.]\d{1,3}$
            Regex regex = new Regex("^\\d{1,3}[\\.]\\d{1,3}[\\.]\\d{1,3}[\\.]\\d{1,3}$");
            Match m = regex.Match(strPattern);
            return m.Success;
        }

        #endregion

        /// <summary>
        /// Post请求，返回响应文本
        /// </summary>
        /// <param name="url"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string PostData(string url, string param)
        {
            try
            {
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
                Encoding encoding = Encoding.GetEncoding("GB2312");
                req.Timeout = 1000 * 10;

                byte[] bs = encoding.GetBytes(param);
                string responseData = String.Empty;
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded";
                req.ContentLength = bs.Length;
                using (Stream reqStream = req.GetRequestStream())
                {
                    reqStream.Write(bs, 0, bs.Length);
                    reqStream.Close();
                }

                using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream(), encoding))
                    {
                        responseData = reader.ReadToEnd().ToString();
                    }
                    return responseData;
                }
            }
            catch
            {
                return string.Empty;
            }
        }

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

        public ActionResult ServerRelations()
        {
            return View();
        }

        [NoAuthorization]
        [HttpPost]
        public ActionResult GetServerRelations(int? areaID, int? gameID)
        {
            var res = SiteHelper.GetAPIResult<List<ServerRelations>>($"/Game/GetServerRelations?areaID={areaID}&gameID={gameID}");
            return Json(new
            {
                data = res
            });
        }


        [NoAuthorization]
        [HttpPost]
        public ActionResult AddServerRelations(List<ServerRelations> data)
        {
            foreach (var item in data)
            {
                var result = SiteHelper.PostAPIResult<ServerRelations>("Game/AddServerRelations", item);
            }
            return Json(new { });
        }
    }
}