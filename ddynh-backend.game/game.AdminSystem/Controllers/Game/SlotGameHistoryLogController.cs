using System;
using System.Collections.Generic;
using System.Web.Mvc;
using game.Entity.AdminSys;
using game.Entity.Money;
using game.Infrastructure.Extensions;
using game.Entity.Stat;
using game.Entity.Area;
using System.Linq;
using game.Entity;
using FW.Core.Helper;
using game.Infrastructure;
using game.AdminSystem.Models.ViewModels;
using game.Entity.Event;
using System.Web;
using example.invalid;
using game.Infrastructure.Utility.ThirdParty;
using game.Entity.Game;
using game.Service;
using game.Infrastructure.Helper;
using game.Entity.ViewModels.Query;
using game.Entity.RawEntity;
using game.Service.DAO;

namespace game.AdminSystem.Controllers.Game
{
    public class SlotGameHistoryLogController : BaseController
    {

        public SlotGameHistoryLogRepository SlotGameHistoryLogRepository { get; set; }

        public GameServerListRepository GameServerListRepository { get; set; }



        public ActionResult List()
        {
            return View();
        }


        [NoAuthorization]
        public ActionResult GetList(QuerySlotGameHistoryLogJQueryRequestModel filter)
        {
            var queryFilter = SiteHelper.GenerateQueryFilterModel<QuerySlotGameHistoryLogRequestViewModel>(filter);
            queryFilter.GameIDs = filter.GameID.ToListOrEmptyList();
            queryFilter.PlatformIDs = filter.PlatformID.ToListOrEmptyList();
            queryFilter.UserIDs = filter.UserID.ToListOrEmptyList();
            queryFilter.GameTypes = filter.GameType.ToListOrEmptyList();

            if (queryFilter.PlatformIDs.IsEmptyOrNull())
                queryFilter.PlatformIDs = DataService.GetMyPlatformIDs();

            var query = SlotGameHistoryLogRepository.GetPagingByFilter(queryFilter);

            PagedList<SlotGameHistoryLogDisplayViewModel> pagingViewModel = ConvertToViewModels(query);
            return ConvertToDataTableJsonResult(filter.Draw, pagingViewModel);
        }


        public PagedList<SlotGameHistoryLogDisplayViewModel> ConvertToViewModels(PagedList<SlotGameHistoryLogRawEntity> queryApis)
        {
            var convert = queryApis.ReturnInjecteObject<PagedList<SlotGameHistoryLogDisplayViewModel>>();

            List<int> serverIDs = queryApis.PageData.Select(z => z.ServerID).ToList();

            var queryServers = GameServerListRepository.GetByIds(serverIDs);


            convert.PageData = queryApis.PageData.Select(z =>
            {
                var convertModel = z.ReturnInjecteObject<SlotGameHistoryLogDisplayViewModel>();
                convertModel.CreateTimeText = z.CreateTime.ToString("yyyy-MM-dd HH:mm:ss");
                convertModel.PlatformName = DataService.GetPlatformNameByCache(z.PlatformID);
                convertModel.GameName = ((GameListEnum)z.GameID).GetEnumDescription();
                convertModel.ServerName = queryServers.FirstOrDefault(x => x.ServerID == z.ServerID)?.ServerName;
                convertModel.GameDataConvert = FormatterGameData(z.GameData);
                convertModel.GameTypeText = ((SlotGameHistoryGameTypeEnum)z.GameType).GetEnumDescription();
                convertModel.LogTypeText = ((SlotGameHistoryLogTypeEnum)z.LogType).GetEnumDescription();

                return convertModel;
            })
            .ToList();

            return convert;
        }


        private string FormatterGameData(string gameData)
        {
            try
            {
                var split = gameData.Split('|').ToList();

                string finalHtml = string.Empty;

                foreach (var eachRow in split)
                {
                    var eachText = eachRow.Split(',');

                    string allResultText = string.Empty;

                    foreach (var eachResult in eachText)
                    {
                        allResultText += string.Format("<div class='resultItem'>{0}</div>", eachResult);
                    }

                    finalHtml += string.Format("<div class='eachResultRow'>{0}</div>", allResultText);
                }

                return finalHtml;

            }
            catch
            {
                return gameData;
            }

        }


    }
}