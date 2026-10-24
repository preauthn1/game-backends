using Microsoft.AspNetCore.Mvc;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using System;
using System.Collections.Generic;

namespace target.BL.Interface
{

    public interface IAGGFreespinService
    {
        AggGameInfoResponseViewModel FreespinSpin_GetBetSetting(AggGameInfoReqeustParameter model);

        PagedList<AggFreespinListDetailItemModel> FreespinSpin_List(VendorAPIFreespinListReqeustParameter model);

        AggFreespinSetPromotionQuotaResponseViewModel FreespinSpin_Create_ByFreeQuota(VendorAPIFreespinSetPromotionQuotaReqeustParameter model);

        AggFreespinSetResponseViewModel FreespinSpin_Set(VendorAPIFreespinSetReqeustParameter model);

        AggFreespinSetResponseViewModel FreespinSpin_Disable(VendorAPIFreespinDisableReqeustParameter model);

        PagedList<AggFreespinStatusItemViewModel> FreespinSpin_Status(VendorAPIFreespinStatusReqeustParameter model);

        AggFreespinRecordResultViewModel FreespinSpin_Record(VendorAPIFreespinRecordReqeustParameter model);

        AggFreespinSummaryResultViewModel FreespinSpin_Summary(VendorAPIFreespinSummaryReqeustParameter model);

        VendorAPIFreespinBudgetSetResultViewModel FreespinSpin_BudgetCreate_ByAdmin([FromBody] VendorAPIFreespinBudgetAdminCreateReqeustParameter model);

        VendorAPIFreespinBudgetListResultViewModel FreespinSpin_Budget_List([FromBody] VendorAPIFreespinBudgetListReqeustParameter model);

        VendorAPIFreespinBudgetSetResultViewModel FreespinSpin_Budget_Set([FromBody] VendorAPIFreespinBudgetSetReqeustParameter model);

        VendorAPIFreespinBudgetWalletUpdateResultViewModel FreespinSpin_Budget_Deposit([FromBody] VendorAPIFreespinBudgetDepositReqeustParameter model);

        VendorAPIFreespinBudgetWalletUpdateResultViewModel FreespinSpin_Budget_Withdrawal([FromBody] VendorAPIFreespinBudgetWithdrawalReqeustParameter model);

    }
}
