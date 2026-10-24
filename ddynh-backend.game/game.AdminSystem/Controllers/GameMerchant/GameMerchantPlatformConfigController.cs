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
using Newtonsoft.Json;

namespace game.AdminSystem.Controllers.GameMerchant
{
    public class GameMerchantPlatformConfigController : BaseController
    {

        public IGameMerchantPlatformConfigService GameMerchantPlatformConfigService { get; set; }

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

            var updateViewModel = new GameMerchantPlatformConfigAdminUpdateViewModel();

            if (isUpdate)
            {
                var query = GameMerchantPlatformConfigService.GetByID(targetID);

                if (query == null)
                    return Json("查无资料", JsonRequestBehavior.AllowGet);

                updateViewModel = query.ReturnInjecteObjectNullable<GameMerchantPlatformConfigAdminUpdateViewModel>();

                updateViewModel.ExtraSetting = InstanceExtraSettings(query.ConfigJsonValues);
            }

            updateViewModel.IsUpdatePage = isUpdate;
            return View(updateViewModel);
        }


        [HttpPost]
        public ActionResult Update(GameMerchantPlatformConfigAdminUpdateViewModel model)
        {
            bool isUpdate = model.ID != 0;

            DateTime now = DateTimeHelper.GetCurrentTime();

            //是否有updateBy 更新人欄位要填
            model.UpdateBy = GetCurrentUserName();

            var postDataModel = model.ReturnInjecteObjectNullable<GameMerchantPlatformConfigRawEntity>();
            postDataModel.ConfigJsonValues = model.ExtraSetting?.ConvertToJsonString();

            if (isUpdate)
            {
                var result = GameMerchantPlatformConfigService.Edit(postDataModel);
                return AlertOrRedirectConfirm(result, "List");
            }
            else
            {
                var result = GameMerchantPlatformConfigService.Create(postDataModel);
                return AlertOrRedirectConfirm(result, "List");
            }
        }


        [NoAuthorization]
        public ActionResult GetList(QueryGameMerchantPlatformConfigJQueryRequestModel filter)
        {
            string myPlatformIDsText = DataService.GetMyPlatformIDsText();

            var queryFilter = SiteHelper.GenerateQueryFilterModel<QueryGameMerchantPlatformConfigRequestViewModel>(filter);
            queryFilter.GameMerchantIDs = filter.GameMerchantID.ToListOrEmptyList();
            queryFilter.PlatformIDs = filter.PlatformID.ToListOrEmptyList();

            //只能看到自家的轉帳單
            if (queryFilter.PlatformIDs.IsEmptyOrNull())
            {
                queryFilter.PlatformIDs = DataService.GetMyPlatformIDs();
            }

            var query = GameMerchantPlatformConfigService.GetPagingByFilter(queryFilter);

            PagedList<GameMerchantPlatformConfigDisplayViewModel> pagingViewModel = ConvertToViewModels(query);
            return ConvertToDataTableJsonResult(filter.Draw, pagingViewModel);
        }
                

        [NoAuthorization]
        public ActionResult CopyPlatformSettingFromTemplate(int templatePlatformID , int targetPlatformID , int gameMerchantID)
        {
            var user = GetCurrentUserName();

            var result = GameMerchantPlatformConfigService.CopySettingFromPlatform(gameMerchantID, templatePlatformID, targetPlatformID, user);

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        public PagedList<GameMerchantPlatformConfigDisplayViewModel> ConvertToViewModels(PagedList<GameMerchantPlatformConfigRawEntity> queryApis)
        {
            var queryGameMerchants = GameMerchantService.GetByFilter(new QueryGameMerchantRequestViewModel());

            var convert = queryApis.ReturnInjecteObject<PagedList<GameMerchantPlatformConfigDisplayViewModel>>();

            convert.PageData = queryApis.PageData.Select(z =>
            {
                var convertModel = z.ReturnInjecteObject<GameMerchantPlatformConfigDisplayViewModel>();
                convertModel.CreateTimeText = z.CreateTime.ToString("yyyy-MM-dd HH:mm:ss");
                convertModel.UpdateTimeText = z.UpdateTime.ToString("yyyy-MM-dd HH:mm:ss");
                convertModel.PlatformName = DataService.GetPlatformNameByCache(z.PlatformID);
                convertModel.GameMerchantName = queryGameMerchants.FirstOrDefault(x => x.GameMerchantID == z.GameMerchantID)?.GameMerchantName;

                return convertModel;
            })
            .ToList();

            return convert;
        }


        private Dictionary<string, string> InstanceExtraSettings(string json)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(json))
                return dic;

            var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            return values;
        }


    }
}