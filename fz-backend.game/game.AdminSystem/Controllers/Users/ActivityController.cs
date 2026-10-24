using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DataTables.Mvc;
using FW.Core.Helper;
using game.Entity;
using game.Entity.Users;
using game.Infrastructure;

namespace game.AdminSystem.Controllers.Users
{
    public class ActivityController : BaseController
    {

        public ActionResult List()
        {
            return View();
        }

        public ActionResult GetEventList(DateTime? beginTime, DateTime? endTime, int? status, string eventName)
        {
            var _list = SiteHelper.GetAPIResult<List<EventSetting>>(string.Format("/Activity/GetEventList?beginTime={0}&endTime={1}&status={2}&eventName={3}", beginTime, endTime, status, eventName));
            return Json(_list, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ModifyEvent(EventSetting model)
        {
            model.Updater = SiteService.GetAdminUserName();
            var _result = SiteHelper.PostAPIResult<EventSetting>("/Activity/ModifyEvent", model);
            if (model.Status == Entity.Game.Status.Delete) { return Json(new { status = _result.Result, msg = _result.Message }); }
            return Alert(_result.Message);
        }

        [HttpPost]
        public ActionResult DeleteEvent(EventSetting model)
        {
            model.Updater = SiteService.GetAdminUserName();
            var _result = SiteHelper.PostAPIResult<EventSetting>("/Activity/ModifyEvent", model);

            return Json(new { status = _result.Result, msg = _result.Message });
        }

        public ActionResult UserEventAccessList()
        {
            return View();
        }

        public ActionResult GetUserEventAccessList(DateTime? beginTime, DateTime? endTime, int? userID, string loginName = null)
        {
            var _list = SiteHelper.GetAPIResult<List<EventLog>>(string.Format("/Activity/GetUserEventAccessList?beginTime={0}&endTime={1}&userID={2}&loginName={3}", beginTime, endTime, userID, loginName));
            return Json(_list, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetUserEventAccessDetail(DateTime? beginTime, DateTime? endTime, int? userID, [ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            var draw = requestModel.Draw;
            var start = requestModel.Start;
            var length = requestModel.Length;
            var sortColumn = requestModel.Columns.GetSortedColumns().FirstOrDefault();
            var sortName = sortColumn.Name;
            var sortLifting = (int)sortColumn.SortDirection;
            var pageIndex = Math.Ceiling((decimal)start / length) + 1;

            var _list = SiteHelper.GetAPIResult<PagedList<EventDetailLog>>(string.Format("/Activity/GetUserEventAccessDetail?beginTime={0}&endTime={1}&userID={2}&pageIndex={3}&pageSize={4}", beginTime, endTime, userID, pageIndex, length));

            return Json(new
            {
                draw = draw,
                recordsTotal = _list.TotalCount,
                recordsFiltered = _list.TotalCount,
                success = true,
                data = _list.PageData
            });
        }

        [HttpPost]
        public ActionResult ResetGrantTime(int eventID, int userID)
        {
            var _restTime = 0;
            var _updater = SiteService.GetAdminUserName();
            var _userID = userID;
            var _result = SiteHelper.GetAPIResult<APIResult>(string.Format("Activity/ResetGrantTime?eventID={0}&userID={1}&updater={2}&resetTime={3}", eventID, _userID, _updater, _restTime));
            return Json(new { status = _result.Result, msg = _result.Message });
        }


        public ActionResult GamesEventMGMT()
        {
            var gameList = SiteHelper.GetAPIResult<List<Entity.Game.Game>>("/Game/GetGame");
            var gameJson = from pro in gameList
                           select new
                           {
                               text = pro.GameName,
                               value = pro.Id
                           };
            ViewBag.GameJson = gameJson;
            return View();
        }

        [NoAuthorization]
        public ActionResult GetGamesEventMGMT(DateTime? beginTime, DateTime? endTime)
        {
            var list = SiteHelper.GetAPIResult<List<EventContest>>($"/Activity/GetGamesEventMGMT?beginTime={beginTime}&endTime={endTime}");
            var query = from pro in list
                        select new
                        {
                            Charge = pro.Charge.ToCash(),
                            Coin = pro.Coin.ToCash(),
                            CreateTime = pro.CreateTime.ToString("yyyy-MM-dd HH:mm"),
                            StartTime = pro.StartTime.ToString("yyyy-MM-dd HH:mm"),
                            EndTime = pro.EndTime.ToString("yyyy-MM-dd HH:mm"),
                            Deadline = pro.Deadline.ToString("hh':'mm"),
                            Entertime = pro.Entertime.ToString("hh':'mm"),
                            PeriodEntertime = pro.PeriodEntertime.ToString("hh':'mm"),
                            PeriodDeadline = pro.PeriodDeadline.ToString("hh':'mm"),
                            pro.GameID,
                            pro.GameName,
                            pro.ID,
                            pro.Name,
                            pro.PlayTime,
                            pro.RewardAfterMin,
                            pro.Show,
                            pro.Status,
                            pro.RankInterval
                        };
            return Json(query, JsonRequestBehavior.AllowGet);
        }

        [NoAuthorization]
        public ActionResult GetContestRank(int? contestID, DateTime? rankDate, int? rankType)
        {
            var rank_list = SiteHelper.GetAPIResult<List<EventRank>>($"/Activity/GetContestRank?contestID={contestID}&rankDate={rankDate}&rankType={rankType}");

            var rankQuery = from pro in rank_list
                            select new
                            {
                                items = new
                                {
                                    pro.Rank,
                                    pro.UserID,
                                    Score = pro.Score.ToCash(),
                                    pro.Type
                                }
                            };
            return Json(new { rank = rankQuery }, JsonRequestBehavior.AllowGet);
        }

        [NoAuthorization]
        public ActionResult GetBonusAndReward(int? contestID)
        {
            var bonus_list = SiteHelper.GetAPIResult<List<EventBonus>>($"/Activity/GetEventBonus?contestID={contestID}");
            var reward_list = SiteHelper.GetAPIResult<List<EventReward>>($"/Activity/GetEventReward?contestID={contestID}");

            var bonusQuery = from pro in bonus_list
                             select new
                             {
                                 pro.ID,
                                 pro.ContestID,
                                 items = new
                                 {
                                     OrderReach = pro.OrderReach.ToCash(),
                                     pro.FreeTicket,
                                 }
                             };

            var rewardQuery_day = from pro in reward_list.Where(x => x.Type == 1)
                                  select new
                                  {
                                      pro.ID,
                                      pro.ContestID,
                                      items = new
                                      {
                                          pro.Rank,
                                          Money = pro.Money.ToCash(),
                                          Type = pro.Type
                                      }
                                  };

            var rewardQuery_interval = from pro in reward_list.Where(x => x.Type == 2)
                                       select new
                                       {
                                           pro.ID,
                                           pro.ContestID,
                                           items = new
                                           {
                                               pro.Rank,
                                               Money = pro.Money.ToCash(),
                                               Type = pro.Type
                                           }
                                       };

            return Json(new { bonus = bonusQuery, reward_day = rewardQuery_day, reward_interval = rewardQuery_interval }, JsonRequestBehavior.AllowGet);
        }

        [NoAuthorization]
        public ActionResult UpdateEventContest(EventContest model)
        {
            var result = SiteHelper.PostAPIResult<EventContest>("Activity/UpdateEventContest", model);
            return Json(result);
        }


        [NoAuthorization]
        public ActionResult UpdateEventBonus(EventBonus model)
        {
            var result = SiteHelper.PostAPIResult<EventBonus>("Activity/UpdateEventBonus", model);
            return Json(result);
        }

        [NoAuthorization]
        public ActionResult UpdateEventReward(EventReward model)
        {
            var result = SiteHelper.PostAPIResult<EventReward>("Activity/UpdateEventReward", model);
            return Json(result);
        }


        [NoAuthorization]
        public ActionResult DeleteEventBonus(int id)
        {
            var result = SiteHelper.GetAPIResult<APIResult>($"Activity/DeleteEventBonus?id={id}");
            return Json(result);
        }

        [NoAuthorization]
        public ActionResult DeleteEventReward(int id)
        {
            var result = SiteHelper.GetAPIResult<APIResult>($"Activity/DeleteEventReward?id={id}");
            return Json(result);
        }


        public ActionResult SevenDaysLoginBonus()
        {
            return View();
        }

        public ActionResult GetSevenDaysLoginBonus(int? platformID, int? areaID)
        {
            var _list = SiteHelper.GetAPIResult<List<SevenDaysLoginBonus>>(string.Format("/Activity/GetSevenDaysLoginBonus?platformID={0}&areaID={1}", platformID, areaID));
            return Json(_list, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult AddSevenDayLoginBonus(SevenDaysLoginBonus model)
        {
            model.Updater = SiteService.GetAdminUserName();
            var _result = SiteHelper.PostAPIResult<SevenDaysLoginBonus>("/Activity/AddSevenDaysLoginBonus", model);
            if (model.Status == Entity.Game.Status.Delete) { return Json(new { status = _result.Result, msg = _result.Message }); }
            return Alert(_result.Message);
        }

        [HttpPost]
        public ActionResult DelSevenDaysLoginBonus(SevenDaysLoginBonus model)
        {
            //model.Updater = SiteService.GetAdminUserName();
            var _result = SiteHelper.PostAPIResult<SevenDaysLoginBonus>("/Activity/DelSevenDaysLoginBonus", model);

            return Json(new { status = _result.Result, msg = _result.Message });

            //if (model.Status == Entity.Game.Status.Delete) { return Json(new { status = _result.Result, msg = _result.Message }); }
            //return Alert(_result.Message);
            
        }

        /*
        [HttpPost]
        public ActionResult DelSevenDaysLoginBonus(int? platformID, int? areaID)
        {
            var model = new SevenDaysLoginBonus();
            model.PlatformID = platformID;
            model.AreaID = areaID;
            //model.Updater = SiteService.GetAdminUserName();
            var _result = SiteHelper.PostAPIResult<SevenDaysLoginBonus>("/Activity/DelSevenDaysLoginBonus", model);

            return Json(new { status = _result.Result, msg = _result.Message });

            //if (model.Status == Entity.Game.Status.Delete) { return Json(new { status = _result.Result, msg = _result.Message }); }
            //return Alert(_result.Message);

        }
        */
    }
}