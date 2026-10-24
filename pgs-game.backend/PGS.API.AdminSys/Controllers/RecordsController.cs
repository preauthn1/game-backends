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
    public class RecordsController : BaseController
    {
        public IPlayerLoginLogService PlayerLoginLogService { get; set; }

        public IUserOperateRecordService UserOperateRecordService { get; set; }

        public IPlayerGameRecordService PlayerGameRecordService { get; set; }

        public IRTPChangeLogService RTPChangeLogService { get; set; }

        public IMerchantCreditChangeLogService MerchantCreditChangeLogService { get; set; }


        /// <summary>
        /// 後台-進出房紀錄
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.MembersManagementBetRecord)]
        [Route("records/game-play")]
        [HttpGet]
        public PagedList<PlayerGameRecordViewModel> GetPlayerGameRecordPagingByFilter([FromQuery]QueryPlayerGameRecordApiRequestParameterModel filter)
        {
            int userID = GetCurrentUserID();
            var query = PlayerGameRecordService.GetByPagingFilter(userID, filter);
            return query;
        }


        /// <summary>
        /// 後台-玩家遊戲啟動紀錄、三方遊戲啟動紀錄
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.MembersManagementDetail)]
        [Route("records/player-login")]
        [HttpGet]
        public PagedList<PlayerLoginLogViewModel> GetPlayerLoginLogPagingByFilter([FromQuery]QueryPlayerLoginLogApiRequestParameterModel filter)
        {
            int userID = GetCurrentUserID();
            var query = PlayerLoginLogService.GetByPagingFilter(userID , filter);
            return query;
        }


        /// <summary>
        /// 後台-玩家封號紀錄
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.MembersManagementLock)]
        [Route("records/modify-player-status")]
        [HttpGet]
        public PagedList<UserOperateLockPlayerRecordViewModel> GetPlayerStatusChangeRecords([FromQuery]QueryUserOperateRecordApiRequestParameterModel filter)
        {
            int userID = GetCurrentUserID();
            var query = UserOperateRecordService.GetPlayerLockRecordByPagingFilter(userID, filter);
            return query;
        }


        /// <summary>
        /// 後台-各種操作紀錄
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.Records_UserOperateRecord)]
        [Route("records/operate-records")]
        [HttpGet]
        public PagedList<UserOperateRecordViewModel> GetUserOperateRecords([FromQuery] QueryUserOperateRecordApiRequestParameterModel filter)
        {
            int userID = GetCurrentUserID();
            var query = UserOperateRecordService.GetByPagingFilter(userID, filter);
            return query;
        }


        [Route("records/rtp-change-logs")]
        [HttpGet]
        public PagedList<RTPChangeLogViewModel> GetRTPChangeLogs([FromQuery] QueryGameVendorRTPChangeRecordRequestParameterModel filter)
        {
            int userID = GetCurrentUserID();
            var query = RTPChangeLogService.GetByPagingFilter(userID, filter);
            return query;
        }


        [Route("records/merchant-credit-change-logs")]
        [HttpGet]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.Agent_MerchantCreditChangeLog)]
        public PagedList<MerchantCreditChangeLogViewModel> GetCreditChangeLogByPagingFilter([FromQuery] QueryMerchantCreditChangeLogApiRequestParameterModel filter)
        {
            int userID = GetCurrentUserID();
            var query = MerchantCreditChangeLogService.GetByPagingFilter(userID , filter);
            return query;
        }


    }
}
