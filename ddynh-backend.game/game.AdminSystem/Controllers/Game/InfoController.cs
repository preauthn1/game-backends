using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using DataTables.Mvc;
using FW.Core.Helper;
using game.Entity;
using game.Entity.Area;
using game.Entity.Game;
using game.Infrastructure;

namespace game.AdminSystem.Controllers.Game
{
    public class InfoController : BaseController
    {
        public ActionResult GameIndex()
        {
            return View(SiteHelper.GetAPIResult<List<Entity.Game.Game>>("/Game/GetGameListVersion"));
        }

        [HttpPost]
        public ActionResult GameIndex(string games, string ip)
        {
            var gamesObj = Newtonsoft.Json.JsonConvert.DeserializeObject<List<gameModel>>(games);

            List<game.Entity.Game.Game> gameList = new List<Entity.Game.Game>();
            var gameserverList = SiteHelper.GetAPIResult<List<GameServer>>("/Game/GetGameServer")
                .Where(p => p.LastUpdateTime >= DateTime.Now.AddMinutes(-5));

            string alertdata = "";
            foreach (var item in gamesObj)
            {
                for (int i = 1; i <= 3; i++)
                {
                    game.Entity.Game.Game game = new Entity.Game.Game()
                    {
                        Id = Convert.ToInt32(item.id),
                        AndroidVersion = item.andver,
                        IOSVersion = item.iosver
                    };
                    gameList.Add(game);
                }
            }
            if (String.IsNullOrWhiteSpace(ip))
            {
                SiteHelper.PostAPIResult<List<game.Entity.Game.Game>>(string.Format("/Game/SaveGameListVersion"), gameList);
            }

            return Alert("操作成功！" + ip + "," + alertdata, "/Info/GameIndex");
        }


        // GET: Info
        public ActionResult Index()
        {
            var versionList = SiteHelper.GetAPIResult<List<HallVersion>>("/Game/GetNewHallVersion");
            var hallSkinList = DataService.GetAllHallSkin();
            var platformList = DataService.GetAllPlatform();

            List<HallVersionInfo> versionInfoList = new List<HallVersionInfo>();

            foreach (var platform in platformList)
            {
                foreach (var skin in hallSkinList)
                {
                    var versionInfo = new HallVersionInfo();
                    versionInfo.PlatformID = platform.Id;
                    versionInfo.PlatformName = platform.Name;
                    versionInfo.HallSkinID = skin.HallSkinID;
                    versionInfo.HallSkinName = skin.Name;
                    versionInfo.AndroidVersion = string.Empty;
                    versionInfo.IOSVersion = string.Empty;
                    versionInfo.AndroidVersion2 = string.Empty;
                    versionInfo.IOSVersion2 = string.Empty;

                    foreach (var version in versionList)
                    {
                        if (platform.Id == version.PlatformID && skin.HallSkinID == version.HallSkinID)
                        {
                            versionInfo.AndroidVersion = string.IsNullOrEmpty(version.AndroidVersion) ? string.Empty : version.AndroidVersion;
                            versionInfo.IOSVersion = string.IsNullOrEmpty(version.IOSVersion) ? string.Empty : version.IOSVersion;
                            versionInfo.AndroidVersion2 = string.IsNullOrEmpty(version.AndroidVersion2) ? string.Empty : version.AndroidVersion2;
                            versionInfo.IOSVersion2 = string.IsNullOrEmpty(version.IOSVersion2) ? string.Empty : version.IOSVersion2;
                        }
                    }
                    versionInfoList.Add(versionInfo);
                }
            }

            //記錄現有版本數, 供view rowspan使用
            ViewBag.Rowspan = hallSkinList.Count();
            return View(versionInfoList);
        }

        /// <summary>
        /// 渠道包資訊
        /// </summary>
        /// <returns></returns>
        public ActionResult DownloadList()
        {
            return View();
        }

        /// <summary>
        /// 渠道包資訊列表
        /// </summary>
        /// <returns></returns>
        [NoAuthorization]
        public ActionResult GetDownloadList(int? platformID)
        {
            var adminId = SiteService.GetAdminUserID();
            var _result = SiteHelper.GetAPIResult<List<PlatformDownload>>($"/Game/GetDownloadList?adminID={adminId}&platformID={platformID}");
            var query = from pro in _result
                        select new
                        {
                            pro.Name,
                            pro.Android,
                            pro.Correct,
                            pro.IOS,
                            pro.IOSMemo,
                            pro.IOSTerminate,
                            pro.Official,
                            pro.PlatformID,
                            pro.Memo,
                            UpdateTime = pro.UpdateTime.ToString("yyyy-MM-dd HH:mm")
                        };

            return Json(new
            {
                recordsTotal = _result.Count(),
                recordsFiltered = _result.Count(),
                success = true,
                data = query
            }, JsonRequestBehavior.AllowGet);
        }

        [NoAuthorization]
        public ActionResult UpdatePlatformDownload(PlatformDownload model)
        {
            var adminId = SiteService.GetAdminUserID();
            var _api = SiteHelper.PostAPIResult<PlatformDownload>($"/Game/UpdatePlatformDownload?adminID={adminId}", model);
            return Json(new { result = _api.Result, msg = _api.Message }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Index(HallVersionList hall)
        {
            var cnt = hall.PlatformID.Count();
            for (var i = 0; i < cnt; i++)
            {
                var url = string.Format("/Game/SaveHallVersions?platformID={0}&skin={1}&anVersion={2}&iosVersion={3}&anVersion2={4}&iosVersion2={5}",
                        hall.PlatformID[i],
                        hall.Skin[i],
                        hall.AndroidVersion[i],
                        hall.IOSVersion[i],
                        hall.AndroidVersion2[i],
                        hall.IOSVersion2[i]
                        );
                Console.WriteLine(url);
                SiteHelper.GetAPIResult<APIResult>(url);
            }
            return Alert("操作成功！", "/Info/Index");
        }

        public ActionResult ProtectNode()
        {
            return View();
        }

        [NoAuthorization]
        public ActionResult GetProtectNode([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, int? userID, string ip)
        {
            var draw = requestModel.Draw;
            var start = requestModel.Start;
            var length = requestModel.Length;
            var pageIndex = Math.Ceiling((decimal)start / length) + 1;

            var _list = SiteHelper.GetAPIResult<PagedList<ProtectNodeLog>>(string.Format("/Game/GetProtectNode?userID={0}&ip={1}&pageIndex={2}&pageSize={3}", userID, ip.Trim(), pageIndex, length));

            return Json(new
            {
                draw = draw,
                recordsTotal = _list.TotalCount,
                recordsFiltered = _list.TotalCount,
                success = true,
                data = _list.PageData
            });
        }
    }

    class hallModel
    {
        public string andver { get; set; }
        public string iosver { get; set; }
        public string andlink { get; set; }
        public string ioslink { get; set; }
    }

    class gameModel
    {
        public int id { get; set; }
        public string andver { get; set; }
        public string iosver { get; set; }
    }
}