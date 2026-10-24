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
using game.Service.DAO;
using game.Infrastructure.Helper;
using game.Service.Service;
using game.Service;
using game.Entity.ViewModels.Query;

namespace game.AdminSystem.Controllers.GameMerchant
{
    public class GameMerchantController : BaseController
    {

        public IGameMerchantService GameMerchantService { get; set; }


        public ActionResult List()
        {
            return View();
        }

        /// <summary>
        /// 新增/編輯
        /// </summary>
        public ActionResult Update(int targetID = 0)
        {
            bool isUpdate = targetID != 0;

            var updateViewModel = new GameMerchantAdminUpdateViewModel();

            if (isUpdate)
            {
                var query = GameMerchantService.GetByID(targetID);

                if (query == null)
                    return Json("查无资料", JsonRequestBehavior.AllowGet);

                updateViewModel = query.ReturnInjecteObjectNullable<GameMerchantAdminUpdateViewModel>();
            }

            updateViewModel.IsUpdatePage = isUpdate;
            return View(updateViewModel);
        }



        [HttpPost]
        public ActionResult Update(GameMerchantAdminUpdateViewModel model)
        {
            bool isUpdate = model.IsUpdatePage;

            DateTime now = DateTimeHelper.GetCurrentTime();
            model.UpdateBy = GetCurrentUserName();

            var postDataModel = model.ReturnInjecteObjectNullable<GameMerchantRawEntity>();

            if (isUpdate)
            {
                //是否有updateBy 更新人欄位要填
                var result = GameMerchantService.Edit(postDataModel);
                return AlertOrRedirectConfirm(result, "List");
            }
            else
            {
                var result = GameMerchantService.Create(postDataModel);
                return AlertOrRedirectConfirm(result, "List");
            }
        }



        [NoAuthorization]
        public ActionResult GetList(QueryGameMerchantJQueryRequestModel filter)
        {
            string myPlatformIDsText = DataService.GetMyPlatformIDsText();
            
            var queryFilter = SiteHelper.GenerateQueryFilterModel<QueryGameMerchantRequestViewModel>(filter);
            queryFilter.GameMerchantIDs = filter.GameMerchantID.ToListOrEmptyList();

            var query = GameMerchantService.GetPagingByFilter(queryFilter);

            PagedList<GameMerchantDisplayViewModel> pagingViewModel = ConvertToViewModels(query);
            return ConvertToDataTableJsonResult(filter.Draw, pagingViewModel);
        }



        public PagedList<GameMerchantDisplayViewModel> ConvertToViewModels(PagedList<GameMerchantRawEntity> queryApis)
        {
            var convert = queryApis.ReturnInjecteObject<PagedList<GameMerchantDisplayViewModel>>();

            convert.PageData = queryApis.PageData.Select(z =>
            {
                var convertModel = z.ReturnInjecteObject<GameMerchantDisplayViewModel>();

                convertModel.StatusText = ((GameMerchantStatusEnum)z.Status).GetEnumDescription();
                convertModel.CreateTimeText = z.CreateTime.ToString("yyyy-MM-dd HH:mm:ss");
                convertModel.UpdateTimeText = z.UpdateTime.ToString("yyyy-MM-dd HH:mm:ss");

                return convertModel;
            })
            .ToList();

            return convert;
        }


    }
}