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
    public class MerchantCreditController : BaseController
    {
        public IMerchantCreditService MerchantCreditService { get; set; }


        /// <summary>
        /// 這個功能、如果非代理樹狀模式會顯示空陣列
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.Agent_MerchantCredit_Search)]
        [Route("merchant-credit")]
        [HttpGet]
        public PagedList<MerchantCreditViewModel> GetMerchantCreditPagingByFilter([FromQuery] QueryMerchantCreditApiRequestParameterModel filter)
        {
            int userID = GetCurrentUserID();
            var query = MerchantCreditService.GetByPagingFilter(userID , filter);
            return query;
        }


        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.Agent_MerchantCredit_Search)]
        [Route("merchant-credit/export-columns")]
        [HttpGet]
        public List<MerchantCreditExportViewModel> GetMerchantCreditExportColumns([FromQuery] QueryMerchantCreditApiRequestParameterModel filter)
        {
            int userID = GetCurrentUserID();
            var query = MerchantCreditService.GetMerchantCreditExportColumns(userID, filter);
            return query;
        }


        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.Agent_MerchantCredit_Search)]
        [Route("merchant-credit/myself")]
        [HttpGet]
        public MerchantCreditEazyCheckModel GetMyCredit()
        {
            int userID = GetCurrentUserID();
            var query = MerchantCreditService.GetMyCredit(userID);
            return query;
        }


        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.Agent_MerchantCredit_Search)]
        [Route("merchant-credit/{merchantID}/preview")]
        [HttpGet]
        public MerchantCreditUpdatePreviewViewModel PreviewCreditUpdate(int merchantID)
        {
            int userID = GetCurrentUserID();
            var query = MerchantCreditService.PreviewCreditUpdate(userID , merchantID);
            return query;
        }


        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.Agent_MerchantCredit_Update)]
        [Route("merchant-credit")]
        [HttpPut]
        public void UpdateCreditAmount([FromBody] MerchantCreditUpdateRequestModel model)
        {
            int userID = GetCurrentUserID();
            MerchantCreditService.UpdateCreditAmount(userID, model);
            return;
        }

    }
}
