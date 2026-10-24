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


namespace game.AdminSystem.Controllers.GameMerchant
{
    public class TransferRecordController : BaseController
    {

        public ITransferRecordService TransferRecordService { get; set; }


        //--------------------------------
        // 記得之後要把 NoAuthorization 標記拿掉
        // 記得之後要把 NoAuthorization 標記拿掉
        // 記得之後要把 NoAuthorization 標記拿掉
        //--------------------------------

        [NoAuthorization]
        public ActionResult List()
        {
            return View();
        }


        [NoAuthorization]
        public ActionResult GetList(QueryTransferRecordJQueryRequestModel filter)
        {
            string myPlatformIDsText = DataService.GetMyPlatformIDsText();

            var queryFilter = SiteHelper.GenerateQueryFilterModel<QueryTransferRecordRequestViewModel>(filter);
            queryFilter.PlatformIDs = filter.PlatformID.ToListOrEmptyList();
            queryFilter.GameMerchantIDs = filter.GameMerchantID.ToListOrEmptyList();
            queryFilter.UserIDs = filter.UserID.ToListOrEmptyList();

            //只能看到自家的轉帳單
            if (queryFilter.PlatformIDs.IsEmptyOrNull())
            {
                queryFilter.PlatformIDs = DataService.GetMyPlatformIDs();
            }

            var query = TransferRecordService.GetPagingByFilter(queryFilter);

            PagedList<TransferRecordDisplayViewModel> pagingViewModel = ConvertToViewModels(query);
            return ConvertToDataTableJsonResult(filter.Draw, pagingViewModel);
        }



        public PagedList<TransferRecordDisplayViewModel> ConvertToViewModels(PagedList<TransferRecordRawEntity> queryApis)
        {
            var convert = queryApis.ReturnInjecteObject<PagedList<TransferRecordDisplayViewModel>>();

            convert.PageData = queryApis.PageData.Select(z =>
            {
                var convertModel = z.ReturnInjecteObject<TransferRecordDisplayViewModel>();
                convertModel.CreateTimeText = z.CreateTime.ToString("yyyy-MM-dd HH:mm:ss");
                convertModel.PlatformName = DataService.GetPlatformNameByCache(z.PlatformID);
                convertModel.GameMerchantText = ((GameMerchantEnum)z.GameMerchantID).GetEnumDescription();
                convertModel.TransferTypeText = ((TransferTypeEnum)z.TransferType).GetEnumDescription();
                convertModel.StatusText = ((ThirdPartyTransferRecordStatusEnum)z.Status).GetEnumDescription();
                convertModel.AmountText = z.Amount.ToCash();

                return convertModel;
            })
            .ToList();

            return convert;
        }




    }
}