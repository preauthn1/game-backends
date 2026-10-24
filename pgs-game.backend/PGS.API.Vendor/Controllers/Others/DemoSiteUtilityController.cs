using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using example.invalid;
using System;
using game.Entity.Game;
using target.BL.Interface;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.Models;

namespace target.API.Vendor.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("v{version:int}")]
    public class DemoSiteUtilityController : BaseController
    {
        public IInternalUtilityService InternalUtilityService { get; set; }


        /// <summary>
        /// 取得指定商戶的所有 開通遊戲、服務器
        /// 這隻API 基本上是for demo站使用
        /// </summary>
        [Route("internal-api/merchant/{merchantID}/servers")]
        [HttpGet]
        public List<DemoGameCategoryViewModels> GetServers(int merchantID)
        {
            // 2026/08  怎好像找不到有誰在使用? 嘗試拋exception看看
            // 2026/08  怎好像找不到有誰在使用? 嘗試拋exception看看
            // 2026/08  怎好像找不到有誰在使用? 嘗試拋exception看看
            throw new TerproException(ApiErrorCode.NotImplement, new ErrorDetailModel
            {
                IsAlwaysNotify = true
            });

            var query = InternalUtilityService.GetMerchantServers(merchantID);
            return query;
        }

    }
}
