using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Models.StoreProcedures;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.ViewModels.API.AdminSys;

namespace target.BL.Interface
{

    public interface IMonthlyAccountingExportService
    {
        AccountingStandardInvoiceMerchantListViewModel GetStandardFormatMonthlyAccountingList(int userID , AccountingSettingGroupRequestModel model);

        AccountingSettingGroupViewV2Model GetAllMonthlyAccountingReport(AccountingSettingGroupRequestModel model);

        void GetMonthlyAccountingReport(AccountingSettingGroupRequestModel model);

        AccountingDetailItemVworldVersionViewModel GeVworldMonthlyAccountingReportForInvoceExport(AccountingSettingGroupRequestModel model);

        AccountingDetailVworldVersionEachMerchantPromotionDetailViewModel GetVworldExportEachMerchantPromotionDetailDebugInfo(AccountingSettingGroupDetailDebugRequestModel filter);

        AccountingAGGGameVendorVersionInvoiceViewModel GeAGGMonthlyAccountingReportForInvoceExport(int userID, AccountingSettingGroupRequestModel model);

        AccountingInvoiceViewModel GetInvoiceInformation(AccountingSettingGroupRequestModel model);

        VendorConfigFinanceCompareInvoiceExportModel GeGameVendorMonthlyInvoceCompareReport(int userID, AccountingSettingGroupRequestModel model);

    }
}
