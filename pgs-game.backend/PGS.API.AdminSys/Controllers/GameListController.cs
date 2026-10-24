using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using example.invalid;
using System;
using game.Entity.Game;
using target.BL.Interface;
using target.Models.Table;
using target.Models.ViewModels;
using target.Web.Modules.Filter;
using target.Models.Paging;
using target.Models.Enums;

namespace target.API.AdminSys.Controllers
{
    [AdminTokenrAuthorizeFilter]
    public class GameListController : BaseController
    {
        public IGameListService GameListService { get; set; }


        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_GameList_Search)]
        [Route("games")]
        [HttpGet]
        public PagedList<GameListViewModel> GetGameListPagingByFilter([FromQuery] QueryGameListApiRequestParameter filter)
        {
            var query = GameListService.GetByPagingFilter(filter);
            return query;
        }


        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_GameList_Search)]
        [Route("games/{gameID}")]
        [HttpGet]
        public GameListViewModel GetByID(int gameID)
        {
            var query = GameListService.GetByID(gameID);
            return query;
        }



        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_GameList_Search)]
        [Route("games/{gameID}/merchants-info")]
        [HttpGet]
        public GameMerchantsMappingInfoViewModel GetGameMerchantsInfo(int gameID)
        {
            var query = GameListService.GetGameMerchantsInfo(gameID);
            return query;
        }


        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_GameList_Create)]
        [Route("games")]
        [HttpPost]
        public void Create([FromBody]GameListCreateRequestModel model)
        {
            GameListService.Create(model);
        }


        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_GameList_Update)]
        [Route("games")]
        [HttpPut]
        public void Update([FromBody]GameListUpdateRequestModel model)
        {
            GameListService.Update(model);
        }


        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_GameList_Update)]
        [Route("games/{targetID}")]
        [HttpDelete]
        public void Delete(int targetID)
        {
            GameListService.Delete(targetID);
        }


        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_GameList_Update)]
        [Route("games/{targetID}/customized-settings")]
        [HttpGet]
        public GameListCustomizedSettingBatchEditModel GetGameListCustomizedSetting(int targetID)
        {
            int userID = GetCurrentUserID();
            var result = GameListService.GetGameListCutomziedSettings(userID , targetID);
            return result;
        }


        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_GameList_Update)]
        [Route("games/customized-settings")]
        [HttpPost]
        public void CreateOrUpdateGameListSetting([FromBody] GameListCustomizedSettingItemModel model)
        {
            int userID = GetCurrentUserID();
            GameListService.CreateOrUpdateGameListSetting(userID, model);
            return;
        }


        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_GameList_Update)]
        [Route("games/customized-settings")]
        [HttpDelete]
        public void DeleteGameListSetting([FromBody] GameListCustomizedSettingItemModel model)
        {
            int userID = GetCurrentUserID();
            GameListService.DeleteGameListSetting(userID, model);
            return;
        }

    }
}
