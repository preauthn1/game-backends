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

namespace game.AdminSystem.Controllers.Game
{
    public class GameCategoryController : BaseController
    {

        /// <summary>
        /// 新增/編輯
        /// </summary>
        public ActionResult Update()
        {
            DisableUniformCheckbox();

            var queryAllCategory = SiteHelper.GetAPIResult<List<GameCategoryEntity>>($"GameCategory/GetGameCategoriesByFilter");
            var queryAllCategorySettings = SiteHelper.GetAPIResult<List<GameCategorySettingEntity>>($"GameCategory/GetGameCategorySettingsByFilter");
            var queryGames = SiteHelper.GetAPIResult<List<game.Entity.Game.Game>>("/Game/GetGame");

            var viewModel = new GameCategoryAdminUpdateViewModel
            {
                EachCategory = queryAllCategory.Select(x=> new GameCategoryEditItemViewModel
                {
                    IsActive = x.Status == NormalActiveStatusEnum.Enable.ToInteger(),
                    Id = x.Id,
                    Name = x.Name,
                    EacgGames = queryGames.Select(z=> new GameCategoryGameSelectedViewModel
                    {
                        ParentCategoryID = x.Id,
                        GameID = z.Id,
                        GameName = z.GameName,
                        IsSelected = queryAllCategorySettings.Any(t=> t.GameID == z.Id && t.GameCategoryID == x.Id)
                    })
                    .ToList()
                })
                .ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Create(GameCategoryAdminCreateViewModel model)
        {
            var createModel = new GameCategoryEntity
            {
                Name = model.Name
            };

            var result = SiteHelper.PostGameApi<CRUDResultModel>($"GameCategory/Create", createModel);
            return Json(result.ResultObject, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult Update(GameCategoryAdminUpdateViewModel model)
        {
            var updateModel = new GameCategoryEntity();

            var categories = model.EachCategory.Select(x => new GameCategoryEntity
            {
                Status = (int)(x.IsActive ? NormalActiveStatusEnum.Enable : NormalActiveStatusEnum.Disable),
                Id = x.Id,
                Name = x.Name
            })
            .ToList();

            var updateSettings = model.EachCategory.SelectMany(x=> x.EacgGames)
                .Where(x=> x.IsSelected)
                .Select(x => new GameCategorySettingEntity
                {
                    GameID = x.GameID,
                    GameCategoryID = x.ParentCategoryID
                })
               .ToList();


            //Step 1. 更新名稱
            var result1 = SiteHelper.PostGameApi<CRUDResultModel>($"GameCategory/EditCategories", categories);

            if (!result1.ResultObject.IsSuccess)
                return Json(result1.ResultObject, JsonRequestBehavior.AllowGet);

            //Step 2. 更新分類勾選
            var result2 = SiteHelper.PostGameApi<CRUDResultModel>($"GameCategory/EditGameCategorySettings", updateSettings);

            return Json(result2.ResultObject, JsonRequestBehavior.AllowGet);
        }


    }
}