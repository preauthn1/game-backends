using Microsoft.AspNetCore.Mvc;
using target.BL.Interface;
using target.Models.ViewModels;
using target.Web.Modules.Filter;
using target.Models.Paging;
using target.Models.Enums;

namespace target.API.AdminSys.Controllers
{
    /// <summary>
    /// 帳變紀錄
    /// </summary>
    public class TransactionController : BaseController
    {
        /// <summary>
        /// 帳變紀錄
        /// </summary>
        public ITransactionRecordService TransactionRecordService { get; set; }


        /// <summary>
        /// 後台-金幣紀錄(帳變紀錄)
        /// </summary>
        [Route("transaction")]
        [HttpGet]
        [AdminTokenrAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.MembersManagementMoneyRecord)]
        public PagedList<TransactionRecordViewModel> GetPagingByFilter([FromQuery] QueryTransactionRecordRequestParameterModel filter)
        {
            int userID = GetCurrentUserID();
            var query = TransactionRecordService.GetPagingByFilter(userID, filter);
            return query;
        }
    }
}
