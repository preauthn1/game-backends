using target.Models.Enums;
using target.Models.Models;
using target.Models.Models.StoreProcedures;
using target.Models.Table;
using System;
using System.Collections.Generic;
using example.invalid;

namespace target.BL.Interface
{
    public interface IExcelFileParseService
    {
        string GenerateVipLevelDescriptionHtml(EventContentBrandingType branding, MerchantWalletModeEnum walletMode);

        Stream CreateNewExcelAndAppendTextV2(NewExcelSheetV2Parameter param);

        Stream DownalodVworldVersionExcelAndGenerateInvoiceExcel(AccountingDetailItemVworldVersionViewModel model);

        Stream GenerateGameVendorMonthlyCompareInvoiceExcel(VendorConfigFinanceCompareInvoiceExportModel model);

        Stream ConvertCommonListToExcel(List<List<string>> eachRowAndCell);

        Stream ConvertCommonListToExcel<T>(ExcelEazyConvertClassValueModel<T> eachRowAndCell) where T : class;

    }


}
