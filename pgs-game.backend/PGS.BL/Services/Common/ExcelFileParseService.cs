using Lck.Utility.Extensions.Helper;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using target.BL.Interface;
using target.DA.Repository;
using target.Models;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Models.StoreProcedures;
using target.Models.Table;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Utility.Utility.ThirdParty;
using System;
using System.Collections.Generic;
using example.invalid;
using System.Linq;
using example.invalid;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace target.BL.Services
{
    public class ExcelFileParseService : BaseService, IExcelFileParseService
    {
        private const int InvoiceExcelDetailItemslRowNumberPlus = 12; // +1 等於excel實際看到的row編號
        private const int InvoiceExcelSubTotalRowNumberPlus = 15;     // +1 等於excel實際看到的row編號
        private const int InvoiceExcelOtherCommentRowNumberPlus = 21; // +1 等於excel實際看到的row編號
        private const int InvoiceExcelLastRowNumberPlus = 25;         // +1 等於excel實際看到的row編號

        private int shiftRowNumberOfOtherComment = 0;



        public string GenerateVipLevelDescriptionHtml(EventContentBrandingType branding, MerchantWalletModeEnum walletMode)
        {
            var myItems = DonwloadExcelAndParseToVipLevelModel();

            string myImg1 = GetPgsLogoImageHtml(branding);

            string innerHtml = ""
                + "<h2 class='bold'>How to set bet limit? (如何设置投注限红？) </h2>"
                + "<div style='color:#555'>"
                + "<div class='max-w-600'>"
                   + "针对真人视讯游戏(LiveCasino)，若要设定玩家投注限额(bet limit) 您可以在呼叫 game-launch API 时使用 <span class='spanKeyword'>liveCasinoLevelID</span> 参数指定限红组别 (API 2.1.1 / 2.1.2)"
                   + "各个币别的限红设置。您可以参考下方表格"
                + "</div>"
                + "<div class='max-w-600 mt-2'>"
                   + "For live casino games, If you need set player bet limits, you can specify the <span class='spanKeyword'>liveCasinoLevelID</span> parameter when call game-launch API. (API 2.1.1 / 2.1.2)."
                   + "For each currency settiings. You can refer the following vip-level table list."
                + "</div>"
                + "</div>";


            string apiDocumentLink = "/api-document/transfer/EN";

            if (walletMode == MerchantWalletModeEnum.SingleWalletMode)
            {
                apiDocumentLink = "/api-document/singleWallet/EN";
            }

            innerHtml += "<div class='mt-4'>" + $"<a href='{apiDocumentLink}' target='_blank'>API_Document_Link(文件链结)</a>" + "</div>";


            var level_vip000 = myItems.CurrencyItems.FirstOrDefault(x => x.Currency == "MYR" && x.VipLevel == "VIP000");
            var level_vip001 = myItems.CurrencyItems.FirstOrDefault(x => x.Currency == "MYR" && x.VipLevel == "VIP001");

            innerHtml += "<h2 class='mt-4 bold'>" + $"Example" + "</h2>";
            innerHtml += "<div>" + $"Player001 , Currency: MYR. the default LevelID is <span class='spanKeyword'>VIP000</span>. " + "</div>";
            innerHtml += "<div>" + $"When player enter to game room. the min-max is {level_vip000?.Min}-{level_vip000?.Max} " + "</div>";
            innerHtml += "<div>" + $"If you set <span class='spanKeyword'>VIP001</span> for this player." + "</div>";
            innerHtml += "<div>" + $"When player enter to game room. the min-max is {level_vip001?.Min}-{level_vip001?.Max} " + "</div>";

            StringBuilder sb1 = new StringBuilder();

            sb1.AppendLine("<table class='vipLevelTable mt-4'>");
            sb1.AppendLine("<thead> <tr>" + TdHtml("Currency") + TdHtml("LevelID") + TdHtml("Min Bet", "text-r") + TdHtml("Max Bet", "text-r") + "</tr> </thead>");

            int loopIndex = 0;
            string lastCurrency = "";

            foreach (var item in myItems.CurrencyItems)
            {
                string extraBorderStyle = "";

                if (lastCurrency != item.Currency)
                {
                    lastCurrency = item.Currency;
                    loopIndex++;
                    extraBorderStyle = " border-top-style";
                }

                string extraColorStyle = $"currency_color_{loopIndex % 2} {extraBorderStyle} ";
                string textRight = $" text-r {extraBorderStyle} ";

                sb1.AppendLine("<tr>" + TdHtml(item.Currency, extraColorStyle) + TdHtml(item.VipLevel, extraBorderStyle) + TdHtml(ToMoneyFormat(item.Min), textRight) + TdHtml(ToMoneyFormat(item.Max), textRight) + "</tr>");
            }

            sb1.AppendLine("</table>");

            innerHtml += "<h2 class='bold mt-4'>Each Currency LevelID (各币种LevelID對照表)</h2>";
            innerHtml += sb1.ToString();

            string title = "Live Casino Level ID (限紅配置)";

            string finalHtml = $@"<!DOCTYPE html>
                                <html lang='en'>

                                <head>
                                    <meta name='viewport' content='width=device-width,initial-scale=1'>
                                    <meta charset='utf-8' />
                                    <title>{title}) Enable games</title>
                                    <link rel='stylesheet' href='/css/swagger_customized.css' />
                                    <link rel='stylesheet' href='https://example.invalid/bootstrap/3.4.1/css/bootstrap.min.css' >
                                </head>
                                <body style='background: #CCC;'>
                                   
                                    <div class='container no-padding' style='background: #FFF;'>
                                        <div class='headerBarTitle'>API Q&A {myImg1}</div>
                                        <div class='documentPanel f14 vipLevelPage'>
                                        {innerHtml}
                                        </div>                                         
                                    </div>                                   
                                </body>
                                </html>";

            return finalHtml;
        }


        /// <summary>
        /// 標準版帳單 / 橫版帳單
        /// </summary>
        public Stream DownalodExcelAndGenerateInvoiceExcel(AccountingInvoiceViewModel invoice)
        {
            Stream stream = null;

            InvoiceExportTemplateFileTypeEnum myTemplateType = invoice.InvoiceTemplateFileType;

            string targetFileName = "invoice_template_v1.xlsx";

            //這種版型會多一欄
            bool isLandscapeLayout = myTemplateType == InvoiceExportTemplateFileTypeEnum.LandscapeLayoutWithExchangeRate;

            if (isLandscapeLayout)
            {
                targetFileName = "invoice_template_landscape_v1.xlsx";
            }

            using (var client = new WebClient())
            {
                stream = client.OpenRead(AppHelper.StaticResourceDownloadDomain() + $"/backend_template/{targetFileName}?v=" + DateTimeHelper.GetCurrentTimeTimestamp());
            }

            var wb = new XSSFWorkbook(stream); // excel2007

            // 只取第一個sheet。
            ISheet sheet = wb.GetSheetAt(0);

            int totalRowNumbers = InvoiceExcelLastRowNumberPlus + 2;

            //某些檔案版型 會多一欄 所以部分欄位會往右偏移一個
            int shiftByFileType = 0;

            if (isLandscapeLayout)
            {
                shiftByFileType = 1;
            }

            for (int rowNumber = 0; rowNumber < totalRowNumbers; rowNumber++)
            {
                int rowNumberPlus = rowNumber + 1; //+1 等同於excel實際看到的row編號
                var myRow = sheet.GetRow(rowNumber);

                switch (rowNumberPlus)
                {
                    case 1:
                        myRow.GetCell(0).SetCellValue(invoice.TitleSection.MerchantShortName);
                        break;

                    case 2:
                        myRow.GetCell(4 + shiftByFileType).SetCellValue(invoice.TitleSection.InvoiceMonth.ToString("MM/yyyy"));
                        break;

                    case 3:
                        myRow.GetCell(4 + shiftByFileType).SetCellValue(invoice.TitleSection.CurrentDateText);
                        break;

                    case 4:
                        myRow.GetCell(4 + shiftByFileType).SetCellValue(invoice.TitleSection.InvoiceNumber);
                        break;

                    case 5:
                        myRow.GetCell(4 + shiftByFileType).SetCellValue(invoice.TitleSection.MerchantShortName);
                        break;

                    case 9:
                        myRow.GetCell(0).SetCellValue(invoice.TitleSection.MerchantFullName);
                        break;

                    case 11:
                        if (!isLandscapeLayout)
                        {
                            myRow.GetCell(2).SetCellValue($"GGR in {invoice.TitleSection.Currency}");
                            myRow.GetCell(3).SetCellValue($"Amount in {invoice.TitleSection.Currency}");
                        }
                        break;

                    case InvoiceExcelSubTotalRowNumberPlus:
                        SetCellValueAdnMinusRedStyle(myRow.GetCell(4 + shiftByFileType), invoice.SubTotal, wb);
                        break;

                    case InvoiceExcelSubTotalRowNumberPlus + 3:
                        SetCellValueAdnMinusRedStyle(myRow.GetCell(4 + shiftByFileType), invoice.Total, wb);
                        break;

                    default:
                        break;
                }
            }

            if (invoice.ExcelRowDetailItems.IsNotEmptyNull())
            {
                int itemsCount = invoice.ExcelRowDetailItems.Count();

                // step 1. 調整後半段rows 順序往後推
                ShiftRowSequence(sheet, InvoiceExcelSubTotalRowNumberPlus, 15, itemsCount);

                // step 2. append new rows
                int appendStartRow = InvoiceExcelDetailItemslRowNumberPlus + 1; //樣板文件預留了兩row。所以額外再+1
                InsertRows(sheet, appendStartRow - 1, itemsCount, isMergeCell: false, wb);

                // step 3. 開始寫入detailItems數值
                for (int i = 0; i < itemsCount; i++)
                {
                    int targetRowNumber = InvoiceExcelDetailItemslRowNumberPlus + i;

                    var myRow = sheet.GetRow(targetRowNumber - 1);

                    var targetDetailItem = invoice.ExcelRowDetailItems[i];

                    myRow.GetCell(0).SetCellValue(targetDetailItem.Description);
                    myRow.GetCell(1).SetCellValue(targetDetailItem.RatePercentage.ToString() + "%");

                    if (targetDetailItem.Description.ContainsWihtoutCase("discount"))
                    {
                        targetDetailItem.WinLossOriginal = targetDetailItem.WinLossOriginal.ToAbs();
                    }

                    SetCellValueAdnMinusRedStyle(myRow.GetCell(2), targetDetailItem.WinLossOriginal, wb);
                    SetCellValueAdnMinusRedStyle(myRow.GetCell(3), targetDetailItem.AmountOrginal, wb);

                    if (isLandscapeLayout)
                    {
                        SetCellValueAdnMinusRedStyle(myRow.GetCell(4), targetDetailItem.ExchangeRate, wb, "#,##0.00000000", 8, isCreateNewStyle: true);
                        SetCellValueAdnMinusRedStyle(myRow.GetCell(5), targetDetailItem.AmountCurrencyExchange, wb);
                    }
                    else
                    {
                        SetCellValueAdnMinusRedStyle(myRow.GetCell(4 + shiftByFileType), targetDetailItem.AmountCurrencyExchange, wb);
                    }
                }

                // step 4. append rows後。後面欄位編號順序改變了
                shiftRowNumberOfOtherComment += itemsCount;
            }

            //因為上面已經有顯示匯率了 下面就不顯示
            if (isLandscapeLayout && invoice.CurrencyExchangeList.IsNotEmptyNull())
            {
                var matchFirst = invoice.CurrencyExchangeList.FirstOrDefault();
                invoice.CurrencyExchangeList = new List<AccountingInvoiceCurrenctExchangeDetailViewModel>
                {
                    new AccountingInvoiceCurrenctExchangeDetailViewModel
                    {
                        Currency = "",
                        Description = matchFirst.Description,
                        CurrencExchangeText = "",
                        LastUpdateDateText = matchFirst.LastUpdateDateText,
                        Rate = -1
                    }
                };
            }

            if (invoice.CurrencyExchangeList.IsNotEmptyNull())
            {
                int itemsCount = invoice.CurrencyExchangeList.Count();

                // step 1. 調整後半段rows 順序往後推
                ShiftRowSequence(sheet, InvoiceExcelLastRowNumberPlus + shiftRowNumberOfOtherComment, 10, itemsCount);

                // step 2. append new rows
                InsertRows(sheet, InvoiceExcelOtherCommentRowNumberPlus + shiftRowNumberOfOtherComment - 1, itemsCount, isMergeCell: true, wb);

                // step 3. 開始寫入detailItems數值
                for (int i = 0; i < itemsCount; i++)
                {
                    int targetRowNumber = InvoiceExcelOtherCommentRowNumberPlus + shiftRowNumberOfOtherComment + i;

                    var myRow = sheet.GetRow(targetRowNumber - 1);

                    var targetItem = invoice.CurrencyExchangeList[i];

                    myRow.GetCell(0).SetCellValue(targetItem.Description + "(" + targetItem.LastUpdateDateText + ")");
                    myRow.GetCell(3).SetCellValue(targetItem.CurrencExchangeText);

                    if (targetItem.Rate != -1)
                    {
                        SetCellValueAdnMinusRedStyle(myRow.GetCell(4), targetItem.Rate, wb, "#,##0.00000000", 8, isCreateNewStyle: true);
                    }
                }
            }

            NpoiMemoryStream myStream = new NpoiMemoryStream();
            myStream.AllowClose = false;

            //FileStream file = new FileStream($@"C:\temp\QQQQQ_{DateTimeHelper.GetCurrentTimeTimestamp()}.xlsx", FileMode.Create);//產生檔案
            wb.Write(myStream);
            //myStream.Position = 0;

            myStream.Flush();
            myStream.Seek(0, SeekOrigin.Begin);
            myStream.AllowClose = true;

            return myStream;
        }


        /// <summary>
        /// 透過簡易model array格式 產生一份Excel 
        /// </summary>
        public Stream CreateNewExcelAndAppendTextV2(NewExcelSheetV2Parameter param)
        {
            var wb = new XSSFWorkbook();

            List<List<string>> eachRowText = param.EachRowText;

            // 創建新的工作表
            ISheet sheet = wb.CreateSheet("Sheet1");

            ICellStyle boldStyle = wb.CreateCellStyle();
            IFont boldFont = wb.CreateFont();
            boldFont.IsBold = true; // 設定粗體
            boldFont.Color = IndexedColors.Brown.Index;
            boldStyle.FillForegroundColor = IndexedColors.LightYellow.Index; // 設定前景色
            boldStyle.FillPattern = FillPattern.SolidForeground;            // 實心填充
            boldStyle.SetFont(boldFont);

            ICellStyle wrapStyle = wb.CreateCellStyle();
            wrapStyle.WrapText = true;
            wrapStyle.VerticalAlignment = example.invalid;

            param.ColumnConfigs = param.ColumnConfigs ?? new List<ExcelCellConfigV2>();

            if (param.ColumnConfigs.IsNotEmptyNull())
            {
                int myColumnNumber = 0;

                foreach (var setting in param.ColumnConfigs)
                {
                    sheet.SetColumnWidth(myColumnNumber, setting.Width * 256);
                    myColumnNumber++;
                }
            }

            int rowNumber = 0;

            foreach (var eachRow in eachRowText)
            {
                // 創建第一行
                IRow row = sheet.CreateRow(rowNumber);

                if (param.RowHeight != 0 && rowNumber > 0)
                {
                    row.Height = param.RowHeight;
                }

                int columnNumber = 0;

                foreach (string each_column_text in eachRow)
                {
                    ICell cell = row.CreateCell(columnNumber);
                    cell.SetCellValue(string.IsNullOrWhiteSpace(each_column_text) ? " " : each_column_text);

                    if (param.IsHeaderRowHighlightStyle && rowNumber == 0)
                    {
                        cell.CellStyle = boldStyle;
                    }

                    if (rowNumber > 0)
                    {
                        var matchSetting = param.ColumnConfigs.GetIndexMatchOrNull(columnNumber);

                        if (matchSetting != null)
                        {
                            if (matchSetting.IsWrapText)
                            {
                                cell.CellStyle = wrapStyle;
                            }
                        }
                    }

                    columnNumber++;
                }

                rowNumber++;
            }

            sheet.CreateFreezePane(0, 1, 0, 1);

            NpoiMemoryStream myStream = new NpoiMemoryStream();
            myStream.AllowClose = false;

            wb.Write(myStream);
            myStream.Flush();
            myStream.Seek(0, SeekOrigin.Begin);
            myStream.AllowClose = true;

            return myStream;
        }



        /// <summary>
        /// 這是vworld帳單的部分 (不要改錯地方了)
        /// </summary>
        public Stream DownalodVworldVersionExcelAndGenerateInvoiceExcel(AccountingDetailItemVworldVersionViewModel model)
        {
            Stream stream = null;

            using (var client = new WebClient())
            {
                stream = client.OpenRead(AppHelper.StaticResourceDownloadDomain() + "/backend_template/invoice_template_vworldVersion.xlsx?v=" + DateTimeHelper.GetCurrentTimeTimestamp());
            }

            var wb = new XSSFWorkbook(stream); // excel2007

            // 只取第一個sheet。
            ISheet sheet = wb.GetSheetAt(0);


            int totalColumns = 8;
            int shiftNumber = 0;
            int detailItemRowPlus = 2;
            int commentDetailRowNumberPlus = 6;

            if (model.Items.IsNotEmptyNull())
            {
                int itemsCount = model.Items.Count();

                // step 1. 調整後半段rows 順序往後推
                ShiftRowSequence(sheet, commentDetailRowNumberPlus - 2, commentDetailRowNumberPlus + 2, itemsCount);

                // step 2. append new rows
                int appendStartRow = detailItemRowPlus;
                InsertRows(sheet, appendStartRow - 1, itemsCount, isMergeCell: false, wb);

                // step 3. 開始寫入detailItems數值
                for (int i = 0; i < itemsCount; i++)
                {
                    int targetRowNumber = detailItemRowPlus + i;

                    var myRow = sheet.GetRow(targetRowNumber - 1);
                    InitializeCellObject(myRow, 0, totalColumns, wb);

                    var targetDetailItem = model.Items[i];

                    myRow.GetCell(0).SetCellValue(targetDetailItem.Description);
                    myRow.GetCell(1).SetCellValue(targetDetailItem.ReferMerchantFullName);
                    myRow.GetCell(2).SetCellValue(targetDetailItem.Currency);
                    SetCellValueAdnMinusRedStyle(myRow.GetCell(3), targetDetailItem.SourceAmount, wb);
                    myRow.GetCell(4).SetCellValue(targetDetailItem.RatePercentage.ToString() + "%");
                    SetCellValueAdnMinusRedStyle(myRow.GetCell(5), targetDetailItem.GGRAmountOrginal, wb);
                    SetFormulaValueToCell(myRow.GetCell(6), $"F{targetRowNumber}*{targetDetailItem.CurrencyExchangeRate}", wb);
                    myRow.GetCell(7).SetCellValue(targetDetailItem.Memo1 ?? "");


                    if (targetDetailItem.GGRAmountOrginal < 0)
                    {
                        SetTargetCellToMinusRedStyle(myRow.GetCell(6), wb);
                    }

                    if (targetDetailItem.RatePercentage < 0)
                    {
                        SetTargetCellToMinusRedStyle(myRow.GetCell(4), wb);
                    }
                }

                // step 4. append rows後。後面欄位編號順序改變了
                shiftNumber += itemsCount;
            }


            if (model.CurrencyExchangeList.IsNotEmptyNull())
            {
                int itemsCount = model.CurrencyExchangeList.Count();

                int startRowNumberPlus = commentDetailRowNumberPlus + shiftNumber;

                // step 2. append new rows
                InsertRows(sheet, startRowNumberPlus - 1, itemsCount, isMergeCell: false, wb);

                // step 3. 開始寫入detailItems數值
                for (int i = 0; i < itemsCount; i++)
                {
                    int targetRowNumber = startRowNumberPlus + i;

                    var myRow = sheet.GetRow(targetRowNumber - 1);

                    var targetItem = model.CurrencyExchangeList[i];

                    myRow.GetCell(0).SetCellValue(targetItem.Description + "(" + targetItem.LastUpdateDateText + ")");

                    SetCellValueOrCreateNewCell(myRow, 1, targetItem.CurrencExchangeText);
                    SetCellValueOrCreateNewCell(myRow, 3, targetItem.Rate.ToString());
                }
            }
            HSSFFormulaEvaluator.EvaluateAllFormulaCells(wb);

            NpoiMemoryStream myStream = new NpoiMemoryStream();
            myStream.AllowClose = false;
            wb.Write(myStream);

            myStream.Flush();
            myStream.Seek(0, SeekOrigin.Begin);
            myStream.AllowClose = true;

            return myStream;
        }


        /// <summary>
        /// 這是 遊戲商比對用(compare)的Excel 方便娟娟跟原廠帳單比對
        /// </summary>
        public Stream GenerateGameVendorMonthlyCompareInvoiceExcel(VendorConfigFinanceCompareInvoiceExportModel model)
        {
            Stream stream = null;

            using (var client = new WebClient())
            {
                stream = client.OpenRead(AppHelper.StaticResourceDownloadDomain() + "/backend_template/invoice_gameVendors.xlsx?v=" + DateTimeHelper.GetCurrentTimeTimestamp());
            }

            var wb = new XSSFWorkbook(stream); // excel2007

            // 只取第一個sheet。
            ISheet sheet = wb.GetSheetAt(0);

            // 插入第二個sheet分頁 (填入一些摘要資訊)
            Set_Sheet2_Summary(model, wb);


            int totalColumns = 8;
            int shiftNumber = 0;
            int detailItemRowPlus = 2;

            if (model.Items.IsNotEmptyNull())
            {
                int itemsCount = model.Items.Count();

                // step 2. append new rows
                int appendStartRow = detailItemRowPlus;
                InsertRows(sheet, appendStartRow - 1, itemsCount, isMergeCell: false, wb);

                // step 3. 開始寫入detailItems數值
                for (int i = 0; i < itemsCount; i++)
                {
                    int targetRowNumber = detailItemRowPlus + i;

                    var myRow = sheet.GetRow(targetRowNumber - 1);
                    InitializeCellObject(myRow, 0, totalColumns, wb);

                    var targetDetailItem = model.Items[i];
                    targetDetailItem.ExtraExcelFormatConfig = targetDetailItem.ExtraExcelFormatConfig ?? new ExtraExcelFormatConfig();


                    string displayID = targetDetailItem.VendorConfigID == 0 ? "" : $"{targetDetailItem.VendorConfigID}｜";
                    string serverName = $"{displayID}{targetDetailItem.VendorConfigName}";
                    string rootAgentID = targetDetailItem.AssignRootAgentID;

                    decimal merchantProfit = targetDetailItem.TotalWinLoss * -1;
                    decimal profitUSD = targetDetailItem.WinLossAmountUSD * -1;

                    myRow.GetCell(0).SetCellValue(rootAgentID.IsNotEmptyTextNotNull() ? rootAgentID : "-");
                    myRow.GetCell(1).SetCellValue(serverName);
                    myRow.GetCell(2).SetCellValue(targetDetailItem.AssignVendorID);
                    myRow.GetCell(3).SetCellValue(targetDetailItem.Currency);
                    SetCellValueAdnMinusRedStyle(myRow.GetCell(4), merchantProfit, wb, targetDetailItem.ExtraExcelFormatConfig.AmountValueFormat);
                    myRow.GetCell(5).SetCellValue(targetDetailItem.MemoInfo);

                    int cellNumber_USD = 6;

                    if (model.IsShowExtraDebugColumn)
                    {
                        SetCellValueAdnMinusRedStyle(myRow.GetCell(cellNumber_USD), profitUSD, wb, targetDetailItem.ExtraExcelFormatConfig.AmountValueFormat);
                    }
                    else
                    {
                        myRow.GetCell(cellNumber_USD).SetCellValue("-");
                    }

                    if (merchantProfit < 0)
                    {
                        SetTargetCellToMinusRedStyle(myRow.GetCell(4), wb);
                        SetTargetCellToMinusRedStyle(myRow.GetCell(cellNumber_USD), wb);
                    }

                    var highlightColorConfig = targetDetailItem.ExtraExcelFormatConfig.HighlightColor;

                    if (highlightColorConfig != ExcelTextColorEnum.Default)
                    {
                        SetTargetCellTextColor(myRow.GetCell(3), wb, highlightColorConfig);
                        SetTargetCellTextColor(myRow.GetCell(5), wb, highlightColorConfig);
                    }
                }

                // step 4. append rows後。後面欄位編號順序改變了
                shiftNumber += itemsCount;
            }

            HSSFFormulaEvaluator.EvaluateAllFormulaCells(wb);

            NpoiMemoryStream myStream = new NpoiMemoryStream();
            myStream.AllowClose = false;
            wb.Write(myStream);

            myStream.Flush();
            myStream.Seek(0, SeekOrigin.Begin);
            myStream.AllowClose = true;

            return myStream;
        }

        private static void Set_Sheet2_Summary(VendorConfigFinanceCompareInvoiceExportModel model, XSSFWorkbook wb)
        {
            string mySummaryText = @"";

            foreach (var item in model.Items)
            {
                mySummaryText += $@"
服務器代號: {item.AssignVendorID}
貨幣代號: {item.Currency}
輸贏金額: {Math.Round(item.TotalWinLoss * -1 , 2)}" + Environment.NewLine;
            }

            mySummaryText += Environment.NewLine + Environment.NewLine +
                $"您好，本月我方數字請參考上方。{Environment.NewLine}若無太大差異。本月數字請以我方為主、並確保有同步給貴司財務單位知道。謝謝";

            ISheet summarySheet = wb.CreateSheet("Summary");
            summarySheet.SetColumnWidth(0, 100 * 256);
            IRow summaryFirstRow = summarySheet.CreateRow(0);
            summaryFirstRow.HeightInPoints = 200;
            summaryFirstRow.CreateCell(0).SetCellValue(mySummaryText);

            wb.SetSheetOrder(summarySheet.SheetName, 1);
            return;
        }

        public Stream ConvertCommonListToExcel(List<List<string>> eachRowAndCell)
        {
            var wb = new XSSFWorkbook(); // excel2007
            var ws = wb.CreateSheet("Sheet1");

            int totalCells = eachRowAndCell.FirstOrDefault().Count();

            int rowNumber = 0;

            foreach (List<string> cells in eachRowAndCell)
            {
                ws.CreateRow(rowNumber);
                int cellNumber = 0;

                var myRow = ws.GetRow(rowNumber);

                foreach (string cellText in cells)
                {
                    var myCell = myRow.CreateCell(cellNumber);

                    if (cellText.IsNumeric() && !RegexRuleHelper.IsMerchantID(cellText))
                    {
                        myCell.SetCellValue(double.Parse(cellText));

                        var newStyle = wb.CreateCellStyle();
                        myCell.CellStyle = newStyle;

                        string myFormat = "#,##0.00";

                        var format = wb.CreateDataFormat();
                        myCell.CellStyle.DataFormat = format.GetFormat(myFormat);

                        SetTargetCellToMinusRedStyle(myCell, wb);
                    }
                    else
                    {
                        myCell.SetCellValue(cellText);
                    }

                    cellNumber++;
                }

                rowNumber++;
            }

            for (int i = 0; i < totalCells; i++)
            {
                ws.AutoSizeColumn(i);
            }


            //寫入本地檔案
            string fileName = string.Format("Export_{0}.xlsx", DateTimeHelper.GetCurrentTimeTimestamp());

            FileStream file = new FileStream(@"C:/temp/" + fileName, FileMode.Create);//產生檔案
            wb.Write(file);
            file.Close();

            //var fileStream = new FileStream("C:/temp/qqqq.xlsx", FileMode.Create, FileAccess.Write);
            //stream.CopyTo(fileStream);
            //fileStream.Dispose();

            //NpoiMemoryStream myStream = new NpoiMemoryStream();
            //myStream.AllowClose = false;
            //wb.Write(myStream);

            //myStream.Flush();
            //myStream.Seek(0, SeekOrigin.Begin);
            //myStream.AllowClose = true;
            return null;
        }

        public Stream ConvertCommonListToExcel<T>(ExcelEazyConvertClassValueModel<T> eachRowAndCell) where T : class
        {
            var wb = new XSSFWorkbook(); // excel2007
            var ws = wb.CreateSheet("Sheet1");

            int rowNumber = 0;

            ws.CreateRow(rowNumber);

            int headerCellNumber = 0;

            foreach (string cellText in eachRowAndCell.HeaderCells.Select(x => x.HeaderText))
            {
                var myCell = ws.GetRow(rowNumber).CreateCell(headerCellNumber);
                myCell.SetCellValue(cellText);

                SetTargetCellToYellowBGStyle(myCell, wb);

                headerCellNumber++;
            }

            ws.CreateFreezePane(0, 1);
            rowNumber++;

            int totalCells = eachRowAndCell.HeaderCells.Count();

            foreach (var cellItem in eachRowAndCell.RowCells)
            {
                ws.CreateRow(rowNumber);
                int cellNumber = 0;

                var myRow = ws.GetRow(rowNumber);

                foreach (var prop in cellItem.GetType().GetProperties())
                {
                    if (prop.HasAttribute<IsNotExcelColumn>())
                        continue;

                    var cellValue = prop.GetValue(cellItem);

                    string cellText = cellValue?.ToString();

                    var myCell = myRow.CreateCell(cellNumber);

                    var numberFormat = "#,##0.00";

                    string custDefine = prop.GetTargetAttribute<ExcelDataFormatAttribute>()?.MyValue;

                    if (custDefine.IsNotEmptyTextNotNull())
                    {
                        numberFormat = custDefine;
                    }

                    if (cellText.IsNumeric() && !RegexRuleHelper.IsMerchantID(cellText))
                    {
                        double myNumberValue = double.Parse(cellText);
                        myCell.SetCellValue(myNumberValue);

                        var newStyle = wb.CreateCellStyle();
                        myCell.CellStyle = newStyle;

                        var format = wb.CreateDataFormat();
                        myCell.CellStyle.DataFormat = format.GetFormat(numberFormat);

                        if (prop.HasAttribute<IsMinusRedStyleAttribute>() && myNumberValue < 0)
                        {
                            SetTargetCellToMinusRedStyle(myCell, wb);
                        }
                    }
                    else
                    {
                        myCell.SetCellValue(cellText);
                    }

                    cellNumber++;
                }

                rowNumber++;
            }

            for (int i = 0; i < totalCells; i++)
            {
                ws.SetColumnWidth(i, eachRowAndCell.HeaderCells[i].ColumnWidth * 256);

                //try
                //{
                //    //linux機器會拿到錯誤 No fonts found installed on the machine.，
                //    ws.AutoSizeColumn(i);
                //}
                //catch
                //{

                //}
            }

            //寫入本地檔案 (debug用)
            if (eachRowAndCell.IsDownloadToLocalForDebug)
            {
                string fileName = string.Format("Export_{0}.xlsx", DateTimeHelper.GetCurrentTimeTimestamp());
                FileStream file = new FileStream(@"C:/temp/" + fileName, FileMode.Create);//產生檔案
                wb.Write(file);
                file.Close();
            }

            NpoiMemoryStream myStream = new NpoiMemoryStream();
            myStream.AllowClose = false;
            wb.Write(myStream);

            myStream.Flush();
            myStream.Seek(0, SeekOrigin.Begin);
            myStream.AllowClose = true;
            return myStream;

            //return null;
        }


        /// <summary>
        /// AGG (遊戲商帳號)
        /// </summary>
        public Stream DownalodAggGameVendorVersionExcelAndGenerateInvoiceExcel(AccountingAGGGameVendorVersionInvoiceViewModel model)
        {
            Stream stream = null;

            using (var client = new WebClient())
            {
                stream = client.OpenRead(AppHelper.StaticResourceDownloadDomain() + "/backend_template/invoice_template_aggVersion_v3.xlsx?v=" + DateTimeHelper.GetCurrentTimeTimestamp());
            }

            var wb = new XSSFWorkbook(stream); // excel2007

            HashSet<string> mergeHistory = new HashSet<string>();

            // 只取第一個sheet。
            ISheet sheet = wb.GetSheetAt(0);

            if (model.DetailItems.IsNotEmptyNull())
            {
                int itemsCount = model.DetailItems.Count();

                // step 3. 開始寫入detailItems數值
                for (int i = 0; i < itemsCount; i++)
                {
                    int targetRowNumber = 1 + i;
                    int targetRowNumberPlus = targetRowNumber + 1;

                    var myRow = sheet.GetRow(targetRowNumber);

                    if (myRow == null)
                    {
                        throw new TerproException(ApiErrorCode.InvalidParameter_ModelIsNull, new ErrorDetailModel($"row:{targetRowNumber}"));
                    }

                    var targetItem = model.DetailItems[i];

                    int totalColumnsCount = 11; //不含最後的加總欄

                    //初始化每個欄位
                    InitializeCellObject(myRow, 0, (totalColumnsCount - 1), wb);                   

                    myRow.GetCell(0).SetCellValue(targetItem.Name);
                    myRow.GetCell(1).SetCellValue(targetItem.SubCategory ?? "");
                    SetCellValueAdnMinusRedStyle(myRow.GetCell(2), targetItem.ExchangeRateForAGG, wb, "", 8);
                    myRow.GetCell(3).SetCellValue(targetItem.Currency);
                    SetCellAlignment(myRow.GetCell(3), HorizontalAlignment.Right);

                    SetCellValueAdnMinusRedStyle(myRow.GetCell(5), targetItem.ReferenceSourceAmount, wb);
                    SetCellValueToPercentageFormat(myRow.GetCell(6), targetItem.ReferenceRate, wb);

                    bool isMinusRed = targetItem.ReferenceRate * targetItem.ReferenceSourceAmount < 0;

                    SetFormulaValueToCell(myRow.GetCell(7), $"F{targetRowNumberPlus}*G{targetRowNumberPlus}", wb);
                    SetFormulaValueToCell(myRow.GetCell(8), $"H{targetRowNumberPlus}*C{targetRowNumberPlus}", wb);
                    SetCellValueToPercentageFormat(myRow.GetCell(9), 0.8M, wb);
                    SetFormulaValueToCell(myRow.GetCell(10), $"I{targetRowNumberPlus}*J{targetRowNumberPlus}", wb);

                    if (targetItem.ReferenceRate < 0)
                    {
                        SetTargetCellToMinusRedStyle(myRow.GetCell(6), wb);
                    }

                    if (isMinusRed)
                    {
                        SetTargetCellToMinusRedStyle(myRow.GetCell(7), wb);
                        SetTargetCellToMinusRedStyle(myRow.GetCell(8), wb);
                        SetTargetCellToMinusRedStyle(myRow.GetCell(10), wb);
                    }

                    if (!string.IsNullOrWhiteSpace(targetItem.MergeCellCategory))
                    {
                        if (mergeHistory.Contains(targetItem.MergeCellCategory))
                        {
                            //myRow.GetCell(0).SetCellValue(targetItem.Name);
                        }
                        else
                        {
                            int sameCount = model.DetailItems.Where(x => x.MergeCellCategory == targetItem.MergeCellCategory).Count();

                            decimal sumValue = model.DetailItems
                                .Where(x => x.MergeCellCategory == targetItem.MergeCellCategory)
                                .Select(x => x.ReferenceSourceAmount)
                                .Sum();

                            if (sameCount >= 2)
                            {
                                int firstRow = targetRowNumber;
                                int lastRow = targetRowNumber + sameCount - 1;

                                CellRangeAddress region = new CellRangeAddress(firstRow, lastRow, 4, 4);
                                sheet.AddMergedRegion(region);

                                SetCellValueAdnMinusRedStyle(myRow.GetCell(4), sumValue, wb);
                                mergeHistory.Add(targetItem.MergeCellCategory);
                            }
                            else
                            {
                                SetCellValueAdnMinusRedStyle(myRow.GetCell(4), sumValue, wb);
                            }
                        }
                    }

                }
            }

            HSSFFormulaEvaluator.EvaluateAllFormulaCells(wb);

            NpoiMemoryStream myStream = new NpoiMemoryStream();
            myStream.AllowClose = false;
            wb.Write(myStream);

            myStream.Flush();
            myStream.Seek(0, SeekOrigin.Begin);
            myStream.AllowClose = true;

            return myStream;
        }


        private void SetTargetCellToMinusRedStyle(ICell cell, XSSFWorkbook wb)
        {
            SetTargetCellTextColor(cell, wb, ExcelTextColorEnum.Red);
        }


        private void SetTargetCellTextColor(ICell cell, XSSFWorkbook wb, ExcelTextColorEnum color = ExcelTextColorEnum.Red)
        {
            try
            {
                var currentFont = cell.CellStyle.GetFont(wb);

                XSSFFont defaultFont = (XSSFFont)wb.CreateFont();

                if (currentFont != null)
                {
                    defaultFont.CloneStyleFrom(currentFont);
                }

                switch (color)
                {
                    case ExcelTextColorEnum.Red:
                        defaultFont.Color = IndexedColors.Red.Index;
                        break;

                    case ExcelTextColorEnum.Pink:
                        defaultFont.Color = IndexedColors.Pink.Index;
                        break;

                    case ExcelTextColorEnum.Brown:
                        defaultFont.Color = IndexedColors.Brown.Index;
                        break;

                    case ExcelTextColorEnum.DarkYellow:
                        defaultFont.Color = IndexedColors.DarkYellow.Index;
                        break;

                    case ExcelTextColorEnum.DarkBlue:
                        defaultFont.Color = IndexedColors.DarkBlue.Index;
                        break;

                    case ExcelTextColorEnum.RoyalBlue:
                        defaultFont.Color = IndexedColors.RoyalBlue.Index;
                        break;

                    default:
                        break;
                }

                cell.CellStyle.SetFont(defaultFont);
            }
            catch
            {

            }
        }

        private void SetCellAlignment(ICell cell, HorizontalAlignment? h = null , VerticalAlignment? v = null)
        {
            try
            {
                if(h.HasValue)
                {
                    cell.CellStyle.Alignment = h.Value;
                }

                if (h.HasValue)
                {
                    cell.CellStyle.VerticalAlignment = v.Value;
                }
            }
            catch
            {

            }
        }


        private void SetTargetCellToYellowBGStyle(ICell cell, XSSFWorkbook wb)
        {
            try
            {
                XSSFCellStyle cellStyle = (XSSFCellStyle)wb.CreateCellStyle();
                cellStyle.FillPattern = FillPattern.SolidForeground;
                cellStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.LightYellow.Index;

                XSSFFont defaultFont = (XSSFFont)wb.CreateFont();
                defaultFont.Color = IndexedColors.Black.Index;
                defaultFont.IsBold = true;

                cellStyle.SetFont(defaultFont);

                cell.CellStyle = cellStyle;
            }
            catch
            {

            }

        }


        private void SetCellValueOrCreateNewCell(IRow row, int targetIndex, string targetValue)
        {
            var myCell = row.GetCell(targetIndex);

            if (myCell == null)
            {
                row.CreateCell(targetIndex);

                myCell = row.GetCell(targetIndex);
                myCell.SetCellValue(targetValue);
            }
            else
            {
                myCell.SetCellValue(targetValue);
            }
        }


        private void InitializeCellObject(IRow row, int startIndex, int endIndex, XSSFWorkbook wb)
        {
            for (int i = startIndex; i <= endIndex; i++)
            {
                var myCell = row.GetCell(i);

                if (myCell == null)
                {
                    row.CreateCell(i);

                    myCell = row.GetCell(i);

                    var HSSFCellStyle = wb.CreateCellStyle();

                    XSSFFont defaultFont = (XSSFFont)wb.CreateFont();
                    defaultFont.Color = IndexedColors.Black.Index;

                    HSSFCellStyle.SetFont(defaultFont);
                    HSSFCellStyle.VerticalAlignment = example.invalid;

                    myCell.CellStyle = HSSFCellStyle;
                }
            }
        }

        private void SetCellValueAdnMinusRedStyle(ICell cell, decimal amountValue, XSSFWorkbook wb, string myFormat = "#,##0.00", int maxDecimalPlace = 2, bool isCreateNewStyle = false)
        {
            if (amountValue < 0)
            {
                var currentFont = cell.CellStyle.GetFont(wb);

                XSSFFont defaultFont = (XSSFFont)wb.CreateFont();
                defaultFont.CloneStyleFrom(currentFont);
                defaultFont.Color = IndexedColors.Red.Index;

                cell.CellStyle.SetFont(defaultFont);
            }

            if (isCreateNewStyle)
            {
                var newStyle = wb.CreateCellStyle();
                newStyle.CloneStyleFrom(cell.CellStyle);

                cell.CellStyle = newStyle;

                if (!string.IsNullOrWhiteSpace(myFormat))
                {
                    var format = wb.CreateDataFormat();
                    cell.CellStyle.DataFormat = format.GetFormat(myFormat);
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(myFormat))
                {
                    var format = wb.CreateDataFormat();
                    cell.CellStyle.DataFormat = format.GetFormat(myFormat);
                }
            }

            //必須轉成double格式才會生效 (00.00格式才會生效)
            cell.SetCellValue((double)Math.Round(amountValue, maxDecimalPlace));
        }


        private void SetCellValueToPercentageFormat(ICell cell, decimal amountValue, XSSFWorkbook wb)
        {
            var format = wb.CreateDataFormat();

            cell.CellStyle.DataFormat = format.GetFormat("0.00%");

            //必須轉成double格式才會生效
            cell.SetCellValue((double)Math.Round(amountValue, 4));
        }


        private void SetFormulaValueToCell(ICell cell, string FormulaText, XSSFWorkbook wb)
        {
            cell.SetCellType(CellType.Formula);
            cell.SetCellFormula(FormulaText);

            var format = wb.CreateDataFormat();
            cell.CellStyle.DataFormat = format.GetFormat("#,##0.00");

            //ps. 字體紅色是後面又呼叫 SetTargetCellToMinusRedStyle
            //最後還需要呼叫 EvaluateAllFormulaCells
        }


        private void ShiftRowSequence(ISheet targetSheet, int targetRowNumberPlus, int includeHowManyRows, int shiftNumber)
        {
            //把後半段的rows順序往後推
            int totalSumAndDescriptionRowNumber = targetRowNumberPlus - 1;
            targetSheet.ShiftRows(totalSumAndDescriptionRowNumber, totalSumAndDescriptionRowNumber + includeHowManyRows, shiftNumber);
        }



        private void InsertRows(ISheet targetSheet, int fromRowIndex, int insertTotalRowCount, bool isMergeCell, XSSFWorkbook wb)
        {
            var rowSource = targetSheet.GetRow(fromRowIndex);

            for (int rowIndex = fromRowIndex; rowIndex < fromRowIndex + insertTotalRowCount; rowIndex++)
            {
                int targerRowIndex = rowIndex + 1;
                var rowInsert = targetSheet.CreateRow(targerRowIndex);
                for (int colIndex = 0; colIndex < (rowSource.LastCellNum + 1); colIndex++)
                {
                    //如果template Excel一直沒有複製 第2列的style 代表你要把那格Excel cell預設塞一個空字串 否則會一直拿到null
                    var cellSource = rowSource.GetCell(colIndex);
                    var cellInsert = rowInsert.CreateCell(colIndex);
                    if (cellSource != null)
                    {
                        var newStyle = wb.CreateCellStyle();
                        newStyle.CloneStyleFrom(cellSource.CellStyle);

                        cellInsert.CellStyle = newStyle;
                        cellInsert.SetCellValue(cellSource.StringCellValue);
                    }
                }

                if (isMergeCell)
                {
                    var cra1 = new NPOI.SS.Util.CellRangeAddress(targerRowIndex, targerRowIndex, 0, 1);
                    targetSheet.AddMergedRegion(cra1);
                }

            }
        }





        private string TdHtml(string text, string extraClass = "")
        {
            return string.Format("<td class='{1}'>{0}</td>", text, extraClass);
        }

        private string ToMoneyFormat(decimal number)
        {
            return number.ToMoneyFormat();
        }

        public ExcelParseResultModel DonwloadExcelAndParseToVipLevelModel()
        {
            Stream stream = null;

            using (var client = new WebClient())
            {
                stream = client.OpenRead(AppHelper.StaticResourceDownloadDomain() + "/cs-download/LiveCasino_VIPLevel.xlsx?v=" + DateTimeHelper.GetCurrentTimeTimestamp());
            }

            if (stream == null)
                throw new TerproException(ApiErrorCode.InternalServerError);

            var wb = new XSSFWorkbook(stream); // excel2007

            List<CurrencyLimitBetRowItemModel> items = new List<CurrencyLimitBetRowItemModel>();

            // Get worksheet using its index
            ISheet worksheet = wb.GetSheetAt(0);

            // Get number of rows and columns
            int rows = worksheet.LastRowNum;
            int cols = 5;

            // 從row1 開始
            for (int rowNumber = 1; rowNumber < rows; rowNumber++)
            {
                var myModels = new CurrencyLimitBetRowItemModel();

                var row = worksheet.GetRow(rowNumber);

                // Loop through each column in selected row
                for (int j = 0; j < cols; j++)
                {
                    var columnCell = row.GetCell(j);

                    string columnValueText = "";

                    if (columnCell != null)
                    {
                        columnValueText = TryGetCellStringValue(columnCell, wb);
                    }

                    if (j != 1 && string.IsNullOrEmpty(columnValueText))
                    {
                        break;
                    }

                    if (j == 0)
                    {
                        myModels.Currency = columnValueText;
                    }
                    else if (j == 2)
                    {
                        myModels.VipLevel = columnValueText;
                    }
                    else if (j == 3)
                    {
                        myModels.Min = TryParseDecimal(columnValueText);
                    }
                    else if (j == 4)
                    {
                        myModels.Max = TryParseDecimal(columnValueText);
                    }
                }

                if (myModels.Min <= 0 && myModels.Max <= 0)
                {
                    continue;
                }

                items.Add(myModels);
            }

            return new ExcelParseResultModel
            {
                CurrencyItems = items
            };
        }


        private decimal TryParseDecimal(string cellValueText)
        {
            if (string.IsNullOrWhiteSpace(cellValueText))
                return 0;

            return decimal.Parse(cellValueText);
        }


        private string TryGetCellStringValue(ICell cell, XSSFWorkbook wb)
        {
            try
            {
                if (cell == null)
                    return null;

                if (cell.CellType == CellType.Numeric)
                    return cell.NumericCellValue.ToString();

                if (cell.CellType == CellType.Formula)
                {
                    var myCellFormula = cell.CellFormula;

                    if (myCellFormula.Contains("IFERROR"))
                    {
                        var mySplit = myCellFormula.Split(',')
                            .Select(x => x.Replace("(", "").Replace(")", ""))
                            .Where(x => x.IsNumeric())
                            .LastOrDefault();

                        return mySplit;
                    }
                    else
                    {
                        var formulaEvaluator = wb.GetCreationHelper().CreateFormulaEvaluator();
                        var cellValue = formulaEvaluator.Evaluate(cell);
                        return cellValue.StringValue;
                    }
                }

                return cell.StringCellValue;

            }
            catch
            {

            }

            return string.Empty;
        }


        private string GetPgsLogoImageHtml(EventContentBrandingType branding = EventContentBrandingType.target)
        {
            string imageUrl = AppHelper.StaticResourceDownloadDomain() + "/game-icon/pgs_logo_512x512.png";

            if (branding == EventContentBrandingType.IONSlot)
            {
                imageUrl = AppHelper.StaticResourceDownloadDomain() + "/Artist/IONSLOT2-.png";
            }

            return $"<img src='{imageUrl}'></img>";
        }

    }

}
