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
    public class RecordsSystemLogController : BaseController
    {
     
        public IExternalApiRequestLogService ExternalApiRequestLogService { get; set; }
    
        public ISingleWalletMerchantApiRequestLogService SingleWalletMerchantApiRequestLogService { get; set; }

        public ISystemLogApiRequestLogService SystemLogApiRequestLogService { get; set; }


        /// <summary>
        /// 各API站請求紀錄 (API request log)
        /// </summary>
        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [Route("records/sysyem-logs-api-request-logs")]
        [HttpGet]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemLogs_ApiReqeustLogManagement)]
        public PagedList<ExternalApiRequestLogViewModel> GeySystemApiRequestLogs([FromQuery] QueryApiRequestLogRequestParameterModel filter)
        {
            int userID = GetCurrentUserID();
            var query = SystemLogApiRequestLogService.GetTopSizeList(userID, filter);
            return query;
        }


        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [Route("records/sysyem-logs-api-request-logs/unique")]
        [HttpGet]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemLogs_ApiReqeustLogManagement)]
        public ExternalApiRequestLogViewModel GeySystemApiRequestLogsByUniqueKey([FromQuery] LazyCommonRequestModel filter)
        {
            int userID = GetCurrentUserID();
            var query = SystemLogApiRequestLogService.GetByUniqueID(filter);
            return query;
        }


        /// <summary>
        /// SW logs (單錢包callback 紀錄)
        /// </summary>
        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemLogs_SingleWalletPlatformReqeustLog)]
        [Route("records/sw-callback-logs")]
        [Route("records/sw-platform-request-logs")]
        [HttpGet]
        public PagedList<ExternalApiRequestLogViewModel> GetSingleWalletPlatformReqeustLog([FromQuery] QuerySingleWalletMerchantApiRequestLogApiRequestParameterModel filter)
        {
            int userID = GetCurrentUserID();
            var query = SingleWalletMerchantApiRequestLogService.GetByPagingFilter(userID, filter);
            return query;
        }


        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemLogs_SingleWalletPlatformReqeustLog)]
        [Route("records/sw-platform-request-logs/unique")]
        [HttpGet]
        public ExternalApiRequestLogViewModel GetSingleWalletByUniqueID([FromQuery] QuerySingleWalletMerchantApiRequestLogApiRequestParameterModel filter)
        {
            int userID = GetCurrentUserID();
            var query = SingleWalletMerchantApiRequestLogService.GetSingleByUniqueID(userID, filter);
            return query;
        }


        /// <summary>
        /// 三方API請求紀錄 (external API)
        /// </summary>
        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemLogsExternalApiRequestLogList, AdminSysPermissionKey.SystemLogsExternalApiRequestLogManagement)]
        [Route("records/ex-api-logs")]
        [HttpGet]
        public PagedList<ExternalApiRequestLogViewModel> GetExternalLogsByRequestID([FromQuery] QueryExternalApiRequestLogApiRequestParameterModel filter)
        {
            int userID = GetCurrentUserID();
            var query = ExternalApiRequestLogService.GetByPagingFilter(userID, filter);
            return query;
        }


        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemLogsExternalApiRequestLogList, AdminSysPermissionKey.SystemLogsExternalApiRequestLogManagement)]
        [Route("records/ex-api-logs/uniqueID/{uniqueID}")]
        [HttpGet]
        public ExternalApiRequestLogViewModel GetExternalLogByUniqueID(string uniqueID)
        {
            int userID = GetCurrentUserID();
            var query = ExternalApiRequestLogService.GetByUniqueID(uniqueID);
            return query;
        }



        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemLogsExternalApiRequestLogList, AdminSysPermissionKey.SystemLogsExternalApiRequestLogManagement)]
        [Route("records/ex-api-logs/id/{id}")]
        [HttpGet]
        public ExternalApiRequestLogViewModel GetExternalLogByID(long id)
        {
            int userID = GetCurrentUserID();
            var query = ExternalApiRequestLogService.GetByID(id);
            return query;
        }

        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemLogsExternalApiRequestLogList, AdminSysPermissionKey.SystemLogsExternalApiRequestLogManagement)]
        [Route("records/ex-api-logs/RequestID/{requestID}")]
        [HttpGet]
        public ExternalApiRequestLogViewModel GetByRequestID(string requestID)
        {
            int userID = GetCurrentUserID();
            var query = ExternalApiRequestLogService.GetByRequestID(requestID);
            return query;
        }


    }
}
