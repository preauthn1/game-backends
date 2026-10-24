using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DataTables.Mvc;
using FW.Core.Helper;
using game.AdminSystem.Models.ViewModels;
using game.Entity;
using game.Entity.Event;
using game.Entity.Money;
using game.Entity.Users;
using game.Infrastructure;
using game.Infrastructure.Extensions;
using game.Infrastructure.Utility;
using game.Service.DAO;

namespace game.AdminSystem.Controllers.Event
{
    public class ActivityController : BaseController
    {

        public EventSettingRepository EventSettingRepository { get; set; }


        public ActionResult List()
        {
            return View();
        }
     

        public ActionResult GetEventList(QueryEventSettingRequestModel filter = null)
        {
            filter = filter ?? new QueryEventSettingRequestModel();

            var dbFilter = new QueryEventSettingRequestViewModel
            {
                PageIndex = 1,
                PageSize = 9999
            };

            dbFilter.PlatformIDs = filter.PlatformID.ToListOrEmptyList();
            dbFilter.Status = filter.Status;

            //篩選當前登入者可看到平台
            if (dbFilter.PlatformIDs.IsEmptyOrNull())
                dbFilter.PlatformIDs = DataService.GetMyPlatformIDs();

            var query = EventSettingRepository.GetPagingByFilter(dbFilter);

            var viewModels = ConvertToViewModels(query);

            return Json(viewModels, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult ModifyEvent(EventSettingEntity model)
        {
            model.Updater = SiteService.GetAdminUserName();

            ApiRequestResultModel<CRUDResultModel> result = null;

            //新增或編輯
            if(model.ID <= 0)
            {
                result = SiteHelper.PostGameApi<CRUDResultModel>("/Activity/CreateEventSetting", model);
            }
            else
            {
                result = SiteHelper.PostGameApi<CRUDResultModel>("/Activity/ModifyEvent", model);
            }

            return Alert(result.ResultObject.Message);
        }

        [HttpPost]
        public ActionResult DeleteEvent(EventSettingEntity model)
        {
            var updateModel = new CommonUpdateStatusRequestModel
            {
                TargetId = model.ID,
                TargetStatus = (int)NormalActiveStatusEnum.PermanentInvisible,
                UpdateBy = GetCurrentUserName(),
            };

            var _result = SiteHelper.PostGameApi<CRUDResultModel>("/Activity/UpdateStatus", updateModel);

            return Json(new { status = _result.ResultObject.IsSuccess, msg = _result.ResultObject.Message });
        }

        public ActionResult UserEventAccessList()
        {
            return View();
        }

        public ActionResult GetUserEventAccessList(DateTime? beginTime, DateTime? endTime, int? userID, string loginName = null)
        {
            string url = string.Format("/Activity/GetUserEventAccessList?beginTime={0}&endTime={1}&userID={2}&loginName={3}", beginTime, endTime, userID, loginName);
            var queryApi = SiteHelper.GetAPIResult<List<EventLog>>(url);
            var convertViewModels = queryApi
                .Select(x => x.ReturnInjecteObject<EventLogViewModels>())
                .ToList();

            return Json(convertViewModels, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetUserEventAccessDetail(int eventID, DateTime? beginTime, DateTime? endTime, int? userID, [ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            var draw = requestModel.Draw;
            var start = requestModel.Start;
            var length = requestModel.Length;
            var sortColumn = requestModel.Columns.GetSortedColumns().FirstOrDefault();
            var sortName = sortColumn.Name;
            var sortLifting = (int)sortColumn.SortDirection;
            var pageIndex = Math.Ceiling((decimal)start / length) + 1;


            string url = string.Format("/Activity/GetUserEventAccessDetail?beginTime={0}&endTime={1}&userID={2}&pageIndex={3}&pageSize={4}", beginTime, endTime, userID, pageIndex, length);
            var queryApi = SiteHelper.GetAPIResult<PagedList<EventDetailLog>>(url);

            var convertViewModels = queryApi.PageData
                .Where(x => x.EventSettingID == eventID)
                 .Select(x => x.ReturnInjecteObject<EventDetailLogViewModels>())
                 .ToList();

            return Json(new
            {
                draw = draw,
                recordsTotal = queryApi.TotalCount,
                recordsFiltered = queryApi.TotalCount,
                success = true,
                data = convertViewModels
            });
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

        private static List<EventSettingViewModel> ConvertToViewModels(PagedList<EventSettingEntity> queryApi)
        {
            DateTime now = DateTime.Now;
            var converts = queryApi.PageData
                .Select(x =>
                {
                    var convert = x.ReturnInjecteObject<EventSettingViewModel>();
                    convert.StartTimeText = x.StartTime.ToString("yyyy-MM-dd HH:mm:ss");
                    convert.EndTimeText = x.EndTime.ToString("yyyy-MM-dd HH:mm:ss");
                    convert.UpdateTimeText = x.UpdateTime.ToString("yyyy-MM-dd HH:mm:ss");
                    convert.StatusName = ((NormalActiveStatusEnum)x.Status).GetEnumDescription();
                    convert.EventTypeText = ((ApplyEventTypeEnum)x.EventType).GetEnumDescription();
                    convert.PlatformName = DataService.GetPlatformNameByCache(x.PlatformID);
                    convert.IsExpired = !(x.StartTime <= now && now <= x.EndTime);
                    return convert;
                })
                .ToList();

            return converts;
        }

    }
}