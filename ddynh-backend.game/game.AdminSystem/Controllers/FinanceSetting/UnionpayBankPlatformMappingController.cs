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

namespace game.AdminSystem.Controllers.FinanceSetting
{
    public class UnionpayBankPlatformMappingController : BaseController
    {
        /// <summary>
        /// 新增/編輯
        /// </summary>
        public ActionResult Update(int platformID = 0)
        {
            DisableUniformCheckbox();

            if (platformID != 0)
            {
                CheckPlatformPermissionExistOrRedirect(platformID);
            }

            var queryMyPlatforms = DataService.GetMyPlatforms();

            //如果只有一笔资料
            if (queryMyPlatforms.IsNotEmptyNull() && queryMyPlatforms.Count() == 1)
                platformID = queryMyPlatforms.FirstOrDefault().Id;

            var queryAllBanks = SiteHelper.GetAPIResult<List<UnionpayBankCodeEntity>>($"UnionpayBankCode/GetByFilter");

            var queryMySettings = SiteHelper.GetAPIResult<List<UnionpayBankPlatformMappingEntity>>($"UnionpayBankPlatformMapping/GetByFilter?PlatformIDsText={platformID}");

            var convertToSelected = queryAllBanks.Select(z => new UnionpayBankPlatformMappingSelectedAdminUpdateViewModel
            {
                UnionpayBankCodeID = z.Id,
                UnionpayBankName = z.BankName,
                UnionpayBankCodeDOMElementID = $"check_{z.Id}",
                IsSelected = queryMySettings.Any(x => x.UnionpayBankCodeID == z.Id)
            })
            .OrderBy(x=> x.UnionpayBankName)
            .ToList();

            var updateModel = new UnionpayBankPlatformMappingAdminUpdateViewModel
            {
                IsMultiplePlatforms = queryMyPlatforms.Count() > 1 ,
                MyPlatforms = queryMyPlatforms,
                PlatformID = platformID,
                PlatformName = DataService.GetPlatformNameByCache(platformID),
                SettingItems = convertToSelected
            };
          
            return View(updateModel);
        }

        
        [NoAuthorization]
        [HttpPost]
        public ActionResult Update(UnionpayBankPlatformMappingAdminUpdateViewModel model)
        {
            var updateModel = new UnionpayBankPlatformMappingEntity();


            //是否有updateBy 更新人欄位要填
            var postDataModel = new UnionpayBankPlatformUpdateViewModel
            {
                PlatformID = model.PlatformID,
                Items = model.SettingItems.Where(x => x.IsSelected).Select(x => new UnionpayBankPlatformMappingEntity
                {
                    PlatformID = model.PlatformID,
                    UnionpayBankCodeID = x.UnionpayBankCodeID
                })
                .ToList()
            };

            var result = SiteHelper.PostGameApi<CRUDResultModel>($"UnionpayBankPlatformMapping/Edit", postDataModel);
            return Json(result.ResultObject , JsonRequestBehavior.AllowGet);
           
        }



    }
}