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
using game.Service.DAO;
using game.Entity.RawEntity;

namespace game.AdminSystem.Controllers.SystemManagement
{
    public class GameMerchantJobSettingController : BaseController
    {

        public GameMerchantJobSettingRepository GameMerchantJobSettingRepository { get; set; }
        


        public ActionResult List()
        {
            return View();
        }



        [NoAuthorization]
        public ActionResult GetList(QueryGameMerchantJobSettingJQueryRequestModel filter)
        {
            string myPlatformIDsText = DataService.GetMyPlatformIDsText();
            var queryFilter = SiteHelper.GenerateQueryFilterModel<QueryPlatformGameMerchantJobSettingRequestViewModel>(filter);
            queryFilter.GameMerchantIDs = filter.GameMerchantID.ToListOrEmptyList();
            queryFilter.IsEnableOnly = filter.Status.HasValue ? true : false;
            queryFilter.OrderByColumn = "IsEnable desc , JobNextStarttTime";
            queryFilter.IsASC = true;

            var query = GameMerchantJobSettingRepository.GetPagingByFilter(queryFilter);

            PagedList<GameMerchantJobSettingDisplayViewModel> pagingViewModel = ConvertToViewModels(query);
            return ConvertToDataTableJsonResult(filter.Draw, pagingViewModel);
        }



        public PagedList<GameMerchantJobSettingDisplayViewModel> ConvertToViewModels(PagedList<GameMerchantJobSettingRawEntity> queryApis)
        {
            DateTime now = DateTimeHelper.GetCurrentTime();

            var convert = queryApis.ReturnInjecteObject<PagedList<GameMerchantJobSettingDisplayViewModel>>();

            convert.PageData = queryApis.PageData.Select(z =>
            {
                var convertModel = z.ReturnInjecteObject<GameMerchantJobSettingDisplayViewModel>();

                if(z.PlatformID == 0)
                    convertModel.PlatformText = "-";
                else
                    convertModel.PlatformText = DataService.GetPlatformNameByCache(z.PlatformID);
                
                convertModel.GameMerchantName = ((GameMerchantEnum)z.GameMerchantID).GetEnumDescription();
                convertModel.IsEnableText = z.IsEnable ? "v" : "";
                convertModel.JobTypeText = ((GameMerchantJobTypeEnum)z.JobType).GetEnumDescription();

                if(!z.IsEnable)
                {
                    convertModel.JobNextStarttTimeText = "-";
                }
                else
                {
                    convertModel.JobNextStarttTimeText = convertModel.JobNextStarttTime.ConvertToDateTimeGapChineseText(now);
                }

                convertModel.LastExecutedTimeText = z.LastExecutedTime.HasValue ? z.LastExecutedTime.Value.ConvertToDateTimeGapChineseText(now , "前") : "";
                return convertModel;
            })
            .ToList();

            return convert;
        }


    }
}