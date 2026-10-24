using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using example.invalid;
using System;
using System.Linq;
using game.Entity.Game;
using target.BL.Interface;
using target.Models.Table;
using target.Models.ViewModels;
using target.Web.Modules.Filter;
using target.Models.Paging;
using target.Models.Enums;
using System.Text;
using target.Utility.Helper;

namespace target.API.AdminSys.Controllers
{
    [AdminTokenrAuthorizeFilter]
    public class CustomizedColumnQueryController : BaseController
    {
        public ICustomizedColumnDisplayService CustomizedColumnDisplayService { get; set; }


        /// <summary>
        /// 列出全部
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.GamesManagement_DynamicQuery)]
        [Route("customized-columns/dynamicQuery/selectItems")]
        [HttpGet]
        public DynamicQueryCommandDropdownItemViewModel GetDynamicDropdownItems()
        {
            int userID = GetCurrentUserID();
            var query = CustomizedColumnDisplayService.GetDynamicDropdownItems(userID);
            return query;
        }

        /// <summary>
        /// 取得單一筆資料
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.GamesManagement_DynamicQuery)]
        [Route("customized-columns/dynamicQuery/setting/{querySqlID}")]
        [HttpGet]
        public DynamicQueryCommandViewModel GetSingleDynamicQueryCommandSetting(int querySqlID)
        {
            int userID = GetCurrentUserID();
            var query = CustomizedColumnDisplayService.GetDynamicQueryCommandSetting(querySqlID);
            return query;
        }


        /// <summary>
        /// 執行 SQL查詢
        /// </summary>
        [Route("customized-columns")]
        [Route("customized-columns/result")]
        [HttpGet]
        public CustomizedColumnDynamicQueryResultViewModel QueryDynamicResult([FromQuery] CustomizedColumnDisplayRequestModel filter)
        {
            int userID = GetCurrentUserID();
            var query = CustomizedColumnDisplayService.QueryDynamicResult(userID, filter);
            return query;
        }


        /// <summary>
        /// 執行查詢 SQL 並轉csv原始raw text
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.GamesManagement_DynamicQuery)]
        [Route("customized-columns/csv/result")]
        [HttpGet]
        public IActionResult DownloadCsvRawTextByDynamicResult([FromQuery] CustomizedColumnDisplayRequestModel filter)
        {
            int userID = GetCurrentUserID();
            string symbol = "\"";

            var query = CustomizedColumnDisplayService.QueryDynamicResult(userID, filter);
            StringBuilder sb = new StringBuilder();

            sb.Append(string.Join(",", query.ExtraObject.Columns.Select(x=> x.ColumnName)));
            sb.Append(Environment.NewLine);

            foreach (var item in query.Items)
            {
                sb.Append(string.Join(",", item.Select(x =>  $"{symbol}{x.Value}{symbol}")));
                sb.Append(Environment.NewLine);
            }

            return Content(sb.ToString());
        }



    }
}
