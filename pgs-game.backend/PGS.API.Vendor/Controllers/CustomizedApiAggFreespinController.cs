using game.Entity.Game;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using target.API.AdminSys;
using target.BL.Interface;
using target.Models;
using target.Models.Models;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.Web.Modules.Filter;
using System;
using System.Collections.Generic;
using example.invalid;

namespace target.API.Vendor.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("v{version:int}")]
    public class CustomizedApiAggFreespinController : BaseController
    {
        public IAGGFreespinService IAGGFreespinService { get; set; }

      
        [SwaggerApiDescriptionEN("Get the Freespin data list. <BR>")]
        [SwaggerApiDescriptionCN("取得已创建的Freespin资料<BR> ")]
        [ApiExplorerSettings(GroupName = SystemConstant.SwaggerGroupName_Freespin)]
        [Route("event/FreespinSpin/List")]
        [ApiConfig(IsHeaderLogEnable = false, IsResponseLogEnable = true)]
        [HttpPost]
        public PagedList<AggFreespinListDetailItemModel> FreespinSpin_List([FromBody] VendorAPIFreespinListReqeustParameter payload)
        {
            var result = IAGGFreespinService.FreespinSpin_List(payload);
            return result;
        }

        [SwaggerApiDescriptionEN("(Promotion*) Use the free quota (promotion) to create FreeSpin. <BR>")]
        [SwaggerApiDescriptionCN("(活动用*) 使用活动免费额度创建 freespin <BR> ")]
        [ApiExplorerSettings(GroupName = SystemConstant.SwaggerGroupName_Freespin)]
        [Route("event/FreespinSpin/Promotion-Quota-Create")]
        [ApiConfig(IsHeaderLogEnable = false, IsResponseLogEnable = true)]
        [HttpPost]
        public AggFreespinSetPromotionQuotaResponseViewModel FreespinSpin_PromotionQoataCreate([FromBody] VendorAPIFreespinSetPromotionQuotaReqeustParameter payload)
        {
            var result = IAGGFreespinService.FreespinSpin_Create_ByFreeQuota(payload);
            return result;
        }


        [SwaggerApiDescriptionEN("Disable FreeSpin settings. <BR>")]
        [SwaggerApiDescriptionCN("停用 freespin 设置 <BR> ")]
        [ApiExplorerSettings(GroupName = SystemConstant.SwaggerGroupName_Freespin)]
        [Route("event/FreespinSpin/Disable")]
        [ApiConfig(IsHeaderLogEnable = false, IsResponseLogEnable = true)]
        [HttpPost]
        public void FreespinSpin_Disable([FromBody] VendorAPIFreespinDisableReqeustParameter payload)
        {
            var result = IAGGFreespinService.FreespinSpin_Disable(payload);
            return;
        }

        [SwaggerApiDescriptionEN("Get FreeSpin status information. <BR>")]
        [SwaggerApiDescriptionCN("取得freespin狀態資訊 <BR> ")]
        [ApiExplorerSettings(GroupName = SystemConstant.SwaggerGroupName_Freespin)]
        [Route("event/FreespinSpin/Status")]
        [ApiConfig(IsHeaderLogEnable = false, IsResponseLogEnable = true)]
        [HttpPost]
        public PagedList<AggFreespinStatusItemViewModel> FreespinSpin_Status([FromBody] VendorAPIFreespinStatusReqeustParameter payload)
        {
            var result = IAGGFreespinService.FreespinSpin_Status(payload);
            return result;
        }


        ///// <summary>
        ///// (⚠️Advance API) for Non-free quota
        ///// </summary>
        //[SwaggerApiDescriptionEN("Get the betting amount that can be set for freespin games. <BR>")]
        //[SwaggerApiDescriptionCN("取得可设置freespin的游戏 & 可配置投注额<BR> ")]
        //[ApiExplorerSettings(GroupName = SystemConstant.SwaggerGroupName_Freespin)]
        //[Route("event/FreespinSpin/betting-info")]
        //[ApiConfig(IsHeaderLogEnable = false, IsResponseLogEnable = true)]
        //[HttpPost]
        //public AggGameInfoResponseViewModel FreespinSpin_GetBetSetting([FromBody] AggGameInfoReqeustParameter payload)
        //{
        //    var result = IAGGFreespinService.FreespinSpin_GetBetSetting(payload);
        //    return result;
        //}


        ///// <summary>
        ///// (⚠️Advance API) for Non-free quota
        ///// </summary>
        //[SwaggerApiDescriptionEN("Create  FreeSpin settings. <BR>")]
        //[SwaggerApiDescriptionCN("创建 freespin 设置 <BR> ")]
        //[ApiExplorerSettings(GroupName = SystemConstant.SwaggerGroupName_Freespin)]
        //[Route("event/FreespinSpin/Create")]
        //[ApiConfig(IsHeaderLogEnable = false, IsResponseLogEnable = true)]
        //[HttpPost]
        //public AggFreespinSetResponseViewModel FreespinSpin_Set([FromBody] VendorAPIFreespinSetReqeustParameter payload)
        //{
        //    var result = IAGGFreespinService.FreespinSpin_Set(payload);
        //    return result;
        //}



        //[SwaggerApiDescriptionEN("Get FreeSpin budget list")]
        //[SwaggerApiDescriptionCN("取得以创建的 freespin预算配置 清单")]
        //[ApiExplorerSettings(GroupName = SystemConstant.SwaggerGroupName_Freespin)]
        //[Route("event/FreespinSpin-budget/List")]
        //[ApiConfig(IsHeaderLogEnable = false, IsResponseLogEnable = true)]
        //[HttpPost]
        //public VendorAPIFreespinBudgetListResultViewModel FreespinSpin_Budget_List([FromBody] VendorAPIFreespinBudgetListReqeustParameter payload)
        //{
        //    var result = IAGGFreespinService.FreespinSpin_Budget_List(payload);
        //    return result;
        //}

        //[SwaggerApiDescriptionEN("Create FreeSpin budget.")]
        //[SwaggerApiDescriptionCN("创建 或 更新freespin 预算配置")]
        //[ApiExplorerSettings(GroupName = SystemConstant.SwaggerGroupName_Freespin)]
        //[Route("event/FreespinSpin-budget/Set")]
        //[ApiConfig(IsHeaderLogEnable = false, IsResponseLogEnable = true)]
        //[HttpPost]
        //public VendorAPIFreespinBudgetSetResultViewModel FreespinSpin_Budget_Set([FromBody] VendorAPIFreespinBudgetSetReqeustParameter payload)
        //{
        //    var result = IAGGFreespinService.FreespinSpin_Budget_Set(payload);
        //    return result;
        //}

        //[SwaggerApiDescriptionEN("FreeSpin budget to deposit.")]
        //[SwaggerApiDescriptionCN("针对预算配置充值额度")]
        //[ApiExplorerSettings(GroupName = SystemConstant.SwaggerGroupName_Freespin)]
        //[Route("event/FreespinSpin-budget/Desposit")]
        //[ApiConfig(IsHeaderLogEnable = false, IsResponseLogEnable = true)]
        //[HttpPost]
        //public VendorAPIFreespinBudgetWalletUpdateResultViewModel FreespinSpin_Budget_Deposit([FromBody] VendorAPIFreespinBudgetDepositReqeustParameter payload)
        //{
        //    var result = IAGGFreespinService.FreespinSpin_Budget_Deposit(payload);
        //    return result;
        //}

        //[SwaggerApiDescriptionEN("FreeSpin budget to withdraw.")]
        //[SwaggerApiDescriptionCN("针对预算配置扣除额度")]
        //[ApiExplorerSettings(GroupName = SystemConstant.SwaggerGroupName_Freespin)]
        //[Route("event/FreespinSpin-budget/Withdrawal")]
        //[ApiConfig(IsHeaderLogEnable = false, IsResponseLogEnable = true)]
        //[HttpPost]
        //public VendorAPIFreespinBudgetWalletUpdateResultViewModel FreespinSpin_Budget_Withdrawal([FromBody] VendorAPIFreespinBudgetWithdrawalReqeustParameter payload)
        //{
        //    var result = IAGGFreespinService.FreespinSpin_Budget_Withdrawal(payload);
        //    return result;
        //}




        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("event/FreespinSpin/BudgeCreate_ByAdmin")]
        [HttpPost]
        public VendorAPIFreespinBudgetSetResultViewModel FreespinSpin_BudgeCreate_ByAdmin([FromBody] VendorAPIFreespinBudgetAdminCreateReqeustParameter payload)
        {
            var result = IAGGFreespinService.FreespinSpin_BudgetCreate_ByAdmin(payload);
            return result;
        }

        // 下面原本預期就不開放了
        // 下面原本預期就不開放了
        // 下面原本預期就不開放了
        // 下面原本預期就不開放了
        // 下面原本預期就不開放了
        // 下面原本預期就不開放了
        // 下面原本預期就不開放了
        // 下面原本預期就不開放了
        // 下面原本預期就不開放了
        // 下面原本預期就不開放了
        // 下面原本預期就不開放了
        // 下面原本預期就不開放了
        // 下面原本預期就不開放了
        // 下面原本預期就不開放了


        //[SwaggerApiDescriptionEN("Get FreeSpin record. <BR>")]
        //[SwaggerApiDescriptionCN("取得freespin紀錄 <BR> ")]
        //[ApiExplorerSettings(GroupName = SystemConstant.SwaggerGroupName_Freespin)]
        //[Route("event/FreespinSpin/Record")]
        //[ApiConfig(IsHeaderLogEnable = false, IsResponseLogEnable = true)]
        //[HttpPost]
        //public AggFreespinRecordResultViewModel FreespinSpin_Record([FromBody] VendorAPIFreespinRecordReqeustParameter payload)
        //{
        //    var result = IAGGFreespinService.FreespinSpin_Record(payload);
        //    return result;
        //}


        //[SwaggerApiDescriptionEN("Get FreeSpin statistical summary <BR>")]
        //[SwaggerApiDescriptionCN("取得freespin统计数据 <BR> ")]
        //[ApiExplorerSettings(GroupName = SystemConstant.SwaggerGroupName_Freespin)]
        //[Route("event/FreespinSpin/Summary")]
        //[ApiConfig(IsHeaderLogEnable = false, IsResponseLogEnable = true)]
        //[HttpPost]
        //public AggFreespinSummaryResultViewModel FreespinSpin_Summary([FromBody] VendorAPIFreespinSummaryReqeustParameter payload)
        //{
        //    var result = IAGGFreespinService.FreespinSpin_Summary(payload);
        //    return result;
        //}




    }
}
