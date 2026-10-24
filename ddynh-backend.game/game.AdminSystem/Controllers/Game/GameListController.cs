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
using game.Entity.Gamegame.Entity.Game;
using game.Service.DAO;

namespace game.AdminSystem.Controllers.Game
{
    public class GameListController : BaseController
    {

        public IGameListService GameListService { get; set; }

        public GameServerListRepository GameServerListRepository { get; set; }

        public GameAliasNameRepository GameAliasNameRepository { get; set; }



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

            var updateViewModel = new GameListAdminUpdateViewModel();

            if (isUpdate)
            {
                var query = GameListService.GetByID(targetID);

                if (query == null)
                    return Json("查无资料", JsonRequestBehavior.AllowGet);

                updateViewModel = query.ReturnInjecteObjectNullable<GameListAdminUpdateViewModel>();
            }
            else
            {
                updateViewModel.Status = NormalActiveStatusEnum.Enable.ToByte();
            }

            updateViewModel.IsUpdatePage = isUpdate;
            return View(updateViewModel);
        }



        [HttpPost]
        public ActionResult Update(GameListAdminUpdateViewModel model)
        {
            bool isUpdate = model.IsUpdatePage;

            DateTime now = DateTimeHelper.GetCurrentTime();

            var postDataModel = model.ReturnInjecteObjectNullable<GameListRawEntity>();

            if (isUpdate)
            {
                var result = GameListService.Edit(postDataModel);
                return AlertOrRedirectConfirm(result, "List");
            }
            else
            {
                var result = GameListService.Create(postDataModel);
                return AlertOrRedirectConfirm(result, "List");
            }
        }


        [NoAuthorization]
        public ActionResult GetList(QueryGameListJQueryRequestModel filter)
        {
            string myPlatformIDsText = DataService.GetMyPlatformIDsText();

            var queryFilter = SiteHelper.GenerateQueryFilterModel<QueryGameListRequestViewModel>(filter);
            queryFilter.GameMerchantIDs = filter.GameMerchantID.ToListOrEmptyList();
            queryFilter.IsASC = true;

            var query = GameListService.GetPagingByFilter(queryFilter);

            PagedList<GameListDisplayViewModel> pagingViewModel = ConvertToViewModels(query);
            return ConvertToDataTableJsonResult(filter.Draw, pagingViewModel);
        }



        public PagedList<GameListDisplayViewModel> ConvertToViewModels(PagedList<GameListRawEntity> queryApis)
        {
            var queryServers = GameServerListRepository.GetBriefByFilter(new QueryGameServerListRequestViewModel
            {
                GameIDs = queryApis.PageData.Select(x => x.GameID).ToList()
            });

            var convert = queryApis.ReturnInjecteObject<PagedList<GameListDisplayViewModel>>();

            var queryAllAliNames = GameAliasNameRepository.GetAll();

            convert.PageData = queryApis.PageData.Select(z =>
            {
                var convertModel = z.ReturnInjecteObject<GameListDisplayViewModel>();
                convertModel.GameMerchantText = ((GameMerchantEnum)z.GameMerchantID).GetEnumDescription();
                convertModel.GameMerchantWalletText = ((GameMerchantWalletTypeEnum)z.GameMerchantWalletID).GetEnumDescription();
                convertModel.StatusText = ((NormalActiveStatusEnum)z.Status).GetEnumDescription();
                convertModel.GameServerCount = queryServers.Where(x => x.GameID == z.GameID).Count();
                convertModel.AliasNameCount = queryAllAliNames.Count(x => x.GameID == z.GameID);

                return convertModel;
            })
            .ToList();

            return convert;
        }




    }
}