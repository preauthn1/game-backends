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
using System.Threading.Tasks;

namespace target.API.AdminSys.Controllers
{
    [AdminTokenrAuthorizeFilter]
    public class MerchantHierarchyController : BaseController
    {
        public IMerchantHierarchyService MerchantHierarchyService { get; set; }

        public ICommonRoutineJobService CommonRoutineJobService { get; set; }


        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.Agent_MerchantHierarchyTreeView , AdminSysPermissionKey.Statistics_AgentHierarchyDailySummary)]
        [Route("merchant-hierarchy/tree-view")]
        [HttpGet]
        public MerchantHierarchyTreeviewViewModel GetHierarchyTreeView([FromQuery] MerchantHierarchyTreeviewRequestModel filter)
        {
            int userID = GetCurrentUserID();
            var query = MerchantHierarchyService.GetHierarchyTreeView(userID , filter);
            return query;
        }

        /// <summary>
        /// 新增總代
        /// </summary>
        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.Agent_MerchantHierarchyTreeView)]
        [Route("root-merchant-hierarchy/create")]
        [HttpPost]
        public void CreateAsRootMerchantHierarchy([FromBody] LazyCommonRequestModel model)
        {
            int userID = GetCurrentUserID();
            MerchantHierarchyService.CreateAsRootMerchantHierarchy(userID, model.MerchantID);
            return;
        }


        /// <summary>
        /// 新增代理樹狀 (新增子代)
        /// </summary>
        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.Agent_MerchantHierarchyTreeView)]
        [Route("merchant-hierarchy")]
        [HttpPost]
        public void CreateMerchantHierarchy([FromBody] MerchantHierarchyCreateRequestModel model)
        {
            int userID = GetCurrentUserID();
            MerchantHierarchyService.CreateMerchantHierarchy(userID, model);
            return;
        }


        /// <summary>
        /// 移除代理樹狀
        /// </summary>
        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.Agent_MerchantHierarchyTreeView)]
        [Route("merchant-hierarchy")]
        [HttpDelete]
        public void DeleteMerchantHierarchy([FromBody] MerchantHierarchyDeleteRequestModel model)
        {
            int userID = GetCurrentUserID();
            MerchantHierarchyService.DeleteMerchantHierarchy(userID, model);
            return;
        }


        /// <summary>
        /// 代理移轉
        /// </summary>
        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.Agent_MerchantHierarchyTreeView)]
        [Route("merchant-hierarchy/migrate")]
        [HttpPost]
        public void MerchantHierarchyMigrate([FromBody] MerchantHierarchyMigrateRequestModel model)
        {
            int userID = GetCurrentUserID();
            MerchantHierarchyService.MerchantHierarchyMigrate(userID, model);
            return;
        }


        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.Agent_MerchantHierarchyTreeView)]
        [Route("merchant-hierarchy/tree-error-check")]
        [HttpPost]
        public IActionResult MerchantHierarchyTreeErrorCheck()
        {
            //這種寫法代表完全不等待 立刻執行下一段程式碼 
            _ = CommonRoutineJobService.CheckMerchantHierarchyTreeWeiredAsync();
            return Ok();
        }

        /// <summary>
        /// 手動觸發用 (重新產生某一條樹的child mpaiing資料)
        /// </summary>
        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.Agent_MerchantHierarchyTreeView)]
        [Route("merchant-hierarchy/hierarchyChildTree-gen")]
        [HttpPost]
        public void GenerateHierarchyChildTreeData([FromBody] LazyCommonRequestModel model)
        {
            int userID = GetCurrentUserID();
            MerchantHierarchyService.GenerateHierarchyChildTreeData(model.MerchantID);
            return;
        }


    }
}
