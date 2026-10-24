using Microsoft.AspNetCore.Mvc;
using target.BL.Interface;
using target.BL.Services;
using target.Models;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.ViewModels;
using target.Utility;
using target.Utility.Extensions;
using target.Web.Modules.Filter;
using System;

namespace target.API.AdminSys.Controllers
{
    [AdminTokenrAuthorizeFilter]
    public class AccountingExportController : BaseController
    {
        public IMonthlyAccountingCommonService IMonthlyAccountingCommonService { get; set; }

        public IMonthlyAccountingExportService MonthlyAccountingExportServiceV2 { get; set; }

        public IExcelFileParseService ExcelFileParseService { get; set; }


        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName_IncludeOtherTeam)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.System_AccountingInvoice)]
        [Route("accounting-export/merchant-invoice-list")]
        [HttpGet]
        public AccountingStandardInvoiceMerchantListViewModel GetStandardFormatMonthlyAccountingList([FromQuery] AccountingSettingGroupRequestModel filter)
        {
            int userID = GetCurrentUserID();
            var query = MonthlyAccountingExportServiceV2.GetStandardFormatMonthlyAccountingList(userID, filter);
            return query;
        }


        [ApiConfig(IsLogApiEnable = false)]
        [Route("invoice-generate")]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.System_AccountingInvoice)]
        [HttpGet]
        public IActionResult GenerateInvoiceAndDownloadExcel([FromQuery] AccountingSettingGroupRequestModel model)
        {
            if (SettingHelper.IsDev && model == null || model.AccountingGroupID <= 0)
            {
                model = new AccountingSettingGroupRequestModel
                {
                    TargetMonth = new DateTime(2023, 9, 1),
                    AccountingGroupID = 315004,
                    TargetMerchantID = 175080,
                    IgnoreZero = false
                };
            }

            var myModel = MonthlyAccountingExportServiceV2.GetInvoiceInformation(model);

            var service2 = new ExcelFileParseService();
            var myStream = service2.DownalodExcelAndGenerateInvoiceExcel(myModel);

            string fileName = string.Format("{0}.xlsx", myModel.ExportFileName);

            HttpContext.Response.Headers.Add(SystemConstant.PgsFileHeaderBase64FileName, fileName.Base64Encode()); //不曉得是哪邊給我強制轉成camelCase ..
            return File(myStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }


        /// <summary>
        /// 2026/06 移除之。總表用。但目前已經沒有使用了
        /// </summary>
        //[AdminPermissionAuthorizeFilter(AdminSysPermissionKey.System_AccountingInvoice)]
        //[Route("accounting-export/export-excel-raw-text")]
        //[HttpGet]
        //public IActionResult GetAllMonthlyAccountingReportExcelRawText([FromQuery] AccountingSettingGroupRequestModel filter)
        //{
        //    int userID = GetCurrentUserID();
        //    var query = MonthlyAccountingExportServiceV2.GetAllMonthlyAccountingReport(filter);
        //    return Content(query?.ExportExcelText, "text/plain");
        //}



        /// <summary>
        /// 類型1: Vworld帳單 (Excel檔案下載)
        /// 類型2: 遊戲商的帳單 (AGG以外的)
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.System_AccountingInvoice)]
        [Route("accounting-export/export-excel-raw-text-vwolrd-version")]
        [HttpPost]
        public IActionResult GetVworldExportExcelItems([FromQuery] AccountingSettingGroupRequestModel filter)
        {
            filter.RuleFor(x => x.InvoiceExportCategory).Rule_IsNotNullNotZeroNotMinus();

            int userID = GetCurrentUserID();

            // ※【AGG帳單不走這裡】請參考 export-excel-agg-version
            if (filter.InvoiceExportCategory >= 250000)
            {
                //遊戲商的帳單 (e.g. 跟Funky、Jili對帳用)
                var query = MonthlyAccountingExportServiceV2.GeGameVendorMonthlyInvoceCompareReport(userID, filter);

                var service2 = new ExcelFileParseService();
                var myStream = service2.GenerateGameVendorMonthlyCompareInvoiceExcel(query);

                string defaultName = ((GameVendorTypeEnum)filter.InvoiceExportCategory).ToString();
                string file_name_brand = ((GameVendorTypeEnum)filter.InvoiceExportCategory).GetDescriptionOrDefault(defaultName);

                string fileName = string.Format("{0}.xlsx", $"INVOICE_{file_name_brand}_{filter.TargetMonth.ToString("yyyyMM")}");
                HttpContext.Response.Headers.Add(SystemConstant.PgsFileHeaderBase64FileName, fileName.Base64Encode());
                return File(myStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            else
            {
                var query = MonthlyAccountingExportServiceV2.GeVworldMonthlyAccountingReportForInvoceExport(filter);

                var service2 = new ExcelFileParseService();
                var myStream = service2.DownalodVworldVersionExcelAndGenerateInvoiceExcel(query);

                string file_name_brand = "Vworld";

                if (filter.InvoiceExportCategory.HasValue)
                {
                    file_name_brand = ((AccountingInvoiceExportCategoryEnum)filter.InvoiceExportCategory.Value).ToString();
                }

                string fileName = string.Format("{0}.xlsx", $"INVOICE_{file_name_brand}_{filter.TargetMonth.ToString("yyyyMM")}");
                HttpContext.Response.Headers.Add(SystemConstant.PgsFileHeaderBase64FileName, fileName.Base64Encode());
                return File(myStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

        /// <summary>
        /// AGG 遊戲商的帳單
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.System_AccountingInvoice)]
        [Route("accounting-export/export-excel-agg-version")]
        [HttpPost]
        public object GetAGGGameVendorExportExcelItems([FromQuery] AccountingSettingGroupRequestModel filter)
        {
            filter.RuleFor(x => x.InvoiceExportCategory).Rule_IsNotNullNotZeroNotMinus();

            int userID = GetCurrentUserID();

            var query = MonthlyAccountingExportServiceV2.GeAGGMonthlyAccountingReportForInvoceExport(userID, filter);

            var service2 = new ExcelFileParseService();
            var myStream = service2.DownalodAggGameVendorVersionExcelAndGenerateInvoiceExcel(query);

            string fileName = string.Format("{0}.xlsx", $"INVOICE_AGG_{filter.TargetMonth.ToString("yyyyMM")}");
            HttpContext.Response.Headers.Add(SystemConstant.PgsFileHeaderBase64FileName, fileName.Base64Encode());
            return File(myStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }


        /// <summary>
        /// 折扣明細條目 (有提供給Esther使用)
        /// 這是細項API (不是匯出帳單API)
        /// </summary>
        [Route("accounting-export/export-excel-vwolrd-version-promotion-detail")]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.System_AccountingInvoice, AdminSysPermissionKey.Statistics_EazyQuery_QueryInvoiceEachMerchantDetail)]
        [HttpPost]
        public IActionResult GetVworldExportEachMerchantPromotionDetail([FromQuery] AccountingSettingGroupDetailDebugRequestModel filter)
        {
            var queryExcelParseInfo = MonthlyAccountingExportServiceV2.GetVworldExportEachMerchantPromotionDetailDebugInfo(filter);

            var myStream = ExcelFileParseService.ConvertCommonListToExcel(queryExcelParseInfo.ExcelParseRequiredInfo);

            string fileName = string.Format("EachMerchantDiscountDetail_{0}.xlsx", filter.TargetMonth.ToString("yyyyMM"));

            HttpContext.Response.Headers.Add(SystemConstant.PgsFileHeaderBase64FileName, fileName.Base64Encode()); //不曉得是哪邊給我強制轉成camelCase ..
            return File(myStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }


        /// <summary>
        /// 折扣明細條目 (手風琴格式) 合作方使用
        /// </summary>
        [Route("accounting-export/export-excel-vwolrd-version-promotion-detail/accordion-format")]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.System_AccountingInvoice, AdminSysPermissionKey.Statistics_EazyQuery_QueryInvoiceEachMerchantDetail)]
        [HttpGet]
        public AccountingDetailVworldVersionEachMerchantPromotionDetailViewModel GetVworldExportEachMerchantPromotionDetailAccordionFormat([FromQuery] AccountingSettingGroupDetailDebugRequestModel filter)
        {
            filter = filter ?? new AccountingSettingGroupDetailDebugRequestModel();
            filter.IsQueryAccordionViewFormat = true;

            var queryExcelParseInfo = MonthlyAccountingExportServiceV2.GetVworldExportEachMerchantPromotionDetailDebugInfo(filter);
            return queryExcelParseInfo;
        }


        /// <summary>
        /// 帳單管理 > 右下角顯示的折扣項目
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.System_AccountingInvoice, AdminSysPermissionKey.Statistics_EazyQuery_QueryInvoiceEachMerchantDetail)]
        [Route("accounting/rules-and-assigner")]
        [HttpGet]
        public AccountingAdjustmentRuleViewModel GetAccountingAdjustmentRules()
        {
            int userID = GetCurrentUserID();
            var query = IMonthlyAccountingCommonService.GetAccountingAdjustmentRules(userID);
            return query;
        }



        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.System_AccountingInvoice)]
        [Route("accounting/rules/copy-and-create")]
        [HttpPost]
        public CRUDResultModel CopyAndCreateNewAdjustmentRules([FromBody] LazyCommonRequestModel model)
        {
            int userID = GetCurrentUserID();
            var query = IMonthlyAccountingCommonService.CopyAndCreateNewAdjustmentRule(userID, model);
            return query;
        }



    }
}
