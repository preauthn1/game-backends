using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using example.invalid;
using System;
using game.Entity.Game;
using target.BL.Interface;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.Paging;
using target.Web.Modules.Filter;
using target.Models.Enums;

namespace target.API.AdminSys.Controllers
{
    [AdminTokenrAuthorizeFilter]
    public class PlayerController : BaseController
    {
        public IPlayerService PlayerInfoService { get; set; }

        public IPlayerSuspiciousCheckService PlayerSuspiciousCheckService { get; set; }



        /// <summary>
        /// 用户列表
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.MembersManagementList_Search)]
        [Route("players")]
        [HttpGet]
        public PagedList<PlayerInfoViewModel> GetByPagingFilter([FromQuery]QueryPlayerApiRequestParameterModel model)
        {
            int userID = GetCurrentUserID();
            var query = PlayerInfoService.GetByPagingFilter(userID , model);
            return query;
        }


        /// <summary>
        /// 用户詳細 (詳細、統計資訊)
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.MembersManagementList_Search, AdminSysPermissionKey.MembersManagementDetail_Search)]
        [Route("players/{playerID}")]
        [HttpGet]
        public PlayerInfoSummaryViewModel GetPlayerDetailAndSummaryInfo(int playerID)
        {
            int userID = GetCurrentUserID();
            var query = PlayerInfoService.GetPlayerSummaryInfo(userID, playerID);
            return query;
        }

        /// <summary>
        /// 用户查询 (傳id或account)
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.MembersManagementList_Search, AdminSysPermissionKey.MembersManagementDetail_Search)]
        [Route("players/detail")]
        [HttpGet]
        public PlayerInfoSummaryViewModel GetPlayerSummaryInfoByNameOrID([FromQuery] PlayerDetailQueryRequest filter)
        {
            int userID = GetCurrentUserID();

            int playerID = PlayerInfoService.GetPlayerIdByPlayerNameOrID(userID, filter);
            var query = PlayerInfoService.GetPlayerSummaryInfo(userID, playerID);
            return query;
        }

        /// <summary>
        /// 檢查玩家帳務是否有異常
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.MembersManagementList_Search, AdminSysPermissionKey.MembersManagementDetail_Search)]
        [Route("players/suspicious-check")]
        [HttpGet]
        public PlayerSuspiciousCheckResultViewModel CheckPlayerDataIsSuspicious([FromQuery] LazyCommonRequestModel filter)
        {
            int userID = GetCurrentUserID();
            var query = PlayerSuspiciousCheckService.CheckPlayerDataIsSuspicious(userID, filter);
            return query;
        }


        /// <summary>
        /// 檢查玩家是否有異常 (視訊遊戲簡易對賭查詢)
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.MembersManagementList_Search, AdminSysPermissionKey.MembersManagementDetail_Search)]
        [Route("players/suspicious-check-live-casino")]
        [HttpGet]
        public PlayerLiveCasinoSuspiciousCheckResultViewModel CheckPlayerLiveCasinoGameIsSuspicious([FromQuery] LazyCommonRequestModel filter)
        {
            int userID = GetCurrentUserID();
            var query = PlayerSuspiciousCheckService.CheckPlayerLiveCasinoGameSuspicious(userID, filter);
            return query;
        }


        /// <summary>
        /// 檢查玩家是否有w88掉單
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.MembersManagementList_Search, AdminSysPermissionKey.MembersManagementDetail_Search)]
        [Route("players/w88-transfer-fail-check")]
        [HttpGet]
        public PlayerW88TransferFailCheckResultViewModel CheckPlayerAnyW88TransferFail([FromQuery] LazyCommonRequestModel filter)
        {
            int userID = GetCurrentUserID();
            var query = PlayerInfoService.CheckPlayerAnyW88TransferFail(userID, filter);
            return query;
        }


        /// <summary>
        /// 用戶補錢 
        /// </summary>
        //[AdminTrustIPAddressAuthorizeFilter]
        //[AdminPermissionAuthorizeFilter(AdminSysPermissionKey.MembersManagementDetail_AdvanceUpdate)]
        //[Route("players/manual-deposit")]
        //[HttpPost]
        //public PlayerManualDepositResultViewModel ManualDepositPlayerWallet([FromBody] PlayerManualDepositRequestModel model)
        //{
        //    int userID = GetCurrentUserID();
        //    var result = PlayerInfoService.ManualDepositPlayerWallet(userID, model);
        //    return result;
        //}


        /// <summary>
        /// 用戶補錢 (後台加值介面是打這支API)
        /// </summary>
        [PostHashHeaderValidateFilter]
        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.MembersManagementDetail_AdvanceUpdate , AdminSysPermissionKey.SystemSetting_SystemConfigSetting_Update)]
        [Route("players/internal-account/manual-deposit")]
        [HttpPost]
        public PlayerManualDepositResultViewModel InternalTestAccountWalletDepositAmount([FromBody] InternalTestAccountPlusWalletRequestModel model)
        {
            int userID = GetCurrentUserID();
            var result = PlayerInfoService.InternalTestAccountWalletDepositAmount(userID, model);
            return result;
        }


        /// <summary>
        /// 產生測試用帳號
        /// </summary>
        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.MembersManagementList_Search)]
        [Route("players/internal-account/create")]
        [HttpPost]
        public void CreateInernalTestAccount([FromBody] CreateInternalTestAccountRequestModel model)
        {
            int userID = GetCurrentUserID();
            PlayerInfoService.CreateInernalTestAccount(userID, model);
            return;
        }


        /// <summary>
        /// 透過暫存table 大量批次幫玩家補錢 
        /// </summary>
        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.MembersManagementDetail_AdvanceUpdate)]
        [Route("players/manual-deposit/batch-udpate-by-queue-table")]
        [HttpPost]
        public void BatchManualDepositPlayerWalletByQueueTable([FromBody] BatchPlayerManualDepositRequestModel model)
        {
            int userID = GetCurrentUserID();
            PlayerInfoService.BatchManualDepositPlayerWalletByQueueTable(userID, model);
            return;
        }



        /// <summary>
        /// 解除卡房
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.MembersManagementDetail_Lock)]
        [Route("players/{playerID}/room-lock/remove")]
        [HttpDelete]
        public void RemovePlayerRoomLock(int playerID)
        {
            int userID = GetCurrentUserID();
            PlayerInfoService.RemovePlayerRoomLock(userID , playerID);
            return;
        }



        /// <summary>
        /// 用户 锁定/解锁
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.MembersManagementDetail_Lock)]
        [Route("players/{playerID}/status")]
        [HttpPut]
        public void UpdatePlayerStatus(int playerID , [FromBody]PlayerUpdateStatusRequestModel model)
        {
            int userID = GetCurrentUserID();
            model.PlayerID = playerID;
            PlayerInfoService.UpdatePlayerStatus(userID , model);
        }

        /// <summary>
        /// 用户批量锁定
        /// </summary>
        [Route("players/batch-lock")]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.MembersManagementDetail_Lock)]
        [HttpPut]
        public void UpdateMultiplePlayerStatus([FromBody]PlayerMultipleUpdateStatusRequestModel model)
        {
            int userID = GetCurrentUserID();
            model.Status = (int)PlayerStatusEnum.Disable;
            PlayerInfoService.UpdateMultiplePlayerStatus(userID, model);
        }


        [Route("players/brief-info/list")]
        [HttpPost]
        public List<CommonBriefInfoListViewModel> GetPlayerBriefByIDs([FromBody] LazyCommonRequestModel model)
        {
            int userID = GetCurrentUserID();
            var query = PlayerInfoService.GetPlayerBriefInfoByIDs(userID, model);
            return query;
        }


        [Route("players/{playerID}/agg-fish-balance")]
        [HttpGet]
        public PlayerAGGWalletBalanceViewModel GetAGGFishWalletBalance(int playerID)
        {
            int userID = GetCurrentUserID();
            var query = PlayerInfoService.GetAGGFishWalletBalance(userID, playerID);
            return query;
        }


    }
}
