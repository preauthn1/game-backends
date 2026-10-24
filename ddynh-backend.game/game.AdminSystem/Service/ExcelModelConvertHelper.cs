using FW.Core.Helper.Enum;
using game.Entity.Stat;
using game.Infrastructure;
using System;
using System.Collections.Generic;
using example.invalid;
using System.Linq;
using System.Web;
using game.Infrastructure.Extensions;
using game.Entity.PerLog;
using game.Entity.Game;
using game.Entity.Money;

namespace game.AdminSystem
{
    public static class ExcelModelConvertHelper
    {

        public static string GenerateExcelFileName(string prefix, DateTime begin, DateTime end)
        {
            var beginText = begin.ToString("yyyyMMdd");
            var endText = end.ToString("yyyyMMdd");

            string fileName = $"{prefix}_{beginText}_{endText}.xlsx";
            return fileName;
        }

        /// <summary>
        /// 遊戲紀錄
        /// </summary>
        public static Stream ConvertToGameUserGameInfoExcelStream(List<UserGameInfoViewModel> list)
        {
            var excelSheets = new List<ExcelExportParameterModel>();

            excelSheets.Add(new ExcelExportParameterModel
            {
                SheetName = "游戏纪录",
                ListData = list.Select(item => new GameUserGameInfoExcelModel
                {
                    PlatformName = item.UserInfo.Area.Platform.Name,
                    UserID = item.UserID,
                    LoginName = item.UserInfo.LoginName,
                    GradeID = item.UserInfo.GradeID,
                    GameName = item.GameName,
                    WinCount = item.WinCount,
                    LossCount = item.LoseCount,
                    DrawCount = item.DrawCount,
                    TotalScore = item.Score,
                    GameTime = item.GameTime
                })
                .ToList()
            });

            Stream fileStream = SimpleExcelHelper.ConvertMultiModelToExcelFileStream(excelSheets);
            return fileStream;
        }

        /// <summary>
        /// 每局遊戲紀錄
        /// </summary>
        public static Stream ConvertToPerGameLogDetailLogExcelStream(List<DetailLog> list)
        {
            var excelSheets = new List<ExcelExportParameterModel>();

            excelSheets.Add(new ExcelExportParameterModel
            {
                SheetName = "每局游戏纪录",
                ListData = list.Select(item => new PerGameLogDetailLogExcelModel
                {
                    CreateTime = item.GameTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    DealNo = item.LogID,
                    ServerID = item.ServerID,
                    ServerName = item.ServerName,
                    GameName = item.GameName,
                    PlatformName = item.PlatformName,
                    UserID = item.UserID,
                    UserLoginName = item.LoginName,
                    GradeID = item.GradeID,
                    CurrentAmount = item.CurrentAmount,
                    UpdateAmount = item.UpdateAmount
                })
                .ToList()
            });

            Stream fileStream = SimpleExcelHelper.ConvertMultiModelToExcelFileStream(excelSheets);
            return fileStream;
        }


        /// <summary>
        /// 公司總報表统计
        /// </summary>
        public static Stream ConvertToCompanyReportExcelStream(List<CompanyReportTableRowViewModel> list, List<CompanyReportGroupByResultViewModel> groupByItems)
        {
            var excelSheets = new List<ExcelExportParameterModel>();

            excelSheets.Add(new ExcelExportParameterModel
            {
                SheetName = "每日统计",
                ListData = list.Select(z => 
                {
                    //呼叫这个 请确保栏位命名一致
                    var convert = z.ReturnInjecteObject<CompanyReportExcelModel>();
                    return convert;
                })
                .ToList()
            });

            excelSheets.Add(new ExcelExportParameterModel
            {
                SheetName = "渠道总和",
                ListData = groupByItems.Select(x => 
                {
                    //呼叫这个 请确保栏位命名一致
                    var excelModel = x.ReturnInjecteObject<CompanyReportGroupByAreaExcelModel>();
                    return excelModel;
                })
            });

            Stream fileStream = SimpleExcelHelper.ConvertMultiModelToExcelFileStream(excelSheets);
            return fileStream;
        }

        /// <summary>
        /// 金币记录
        /// </summary>
        public static Stream ConvertToMoneyLogExcelStream(List<MoneyLogUserInfoPage> list)
        {
            var excelSheets = new List<ExcelExportParameterModel>();

            excelSheets.Add(new ExcelExportParameterModel
            {
                IsAutoGenerateTotalSumRow = true,
                SheetName = "金币记录",
                ListData = list.Select(z =>
                {
                    //呼叫这个 请确保栏位命名一致
                    var convert = z.ReturnInjecteObject<MoneyLogExcelModel>();
                    convert.FlowAmount = (long)(decimal.Parse(z.FlowAmount.Replace(",", "")) * 100); //這欄希望自動加總 所以要轉long

                    return convert;
                })
                .ToList()
            });


            Stream fileStream = SimpleExcelHelper.ConvertMultiModelToExcelFileStream(excelSheets);
            return fileStream;
        }


        /// <summary>
        /// 游戏税收统计
        /// </summary>
        public static Stream ConvertToGameTaxLogExcelStream(List<GameTaxLog> list, List<GameTaxLogGroupInfoViewModel> groupByGames)
        {
            var excelSheets = new List<ExcelExportParameterModel>();

            excelSheets.Add(new ExcelExportParameterModel
            {
                SheetName = "每日统计",
                ListData = list.Select(z => new GameTaxLogDateSummaryExcelModel
                {
                    Date = z.StatTime.ToString("yyyy-MM-dd"),
                    GameDisplayName = z.Game.GameName,
                    Player_Balance_Amount = z.Amount,
                    Player_Balance_Tax = z.TaxAmount,
                    Player_Balance_WinLoss = z.Amount + z.TaxAmount,
                    Player_Balance_Flow = z.FlowAmount,
                    Robot_Balance_Amount = z.RobotAmount,
                    Robot_Balance_Tax = z.RobotTaxAmount,
                    Robot_Balance_WinLoss = z.RobotAmount + z.RobotTaxAmount,
                    PoolAmount = z.PoolAmount,
                })
                .ToList()
            });

            excelSheets.Add(new ExcelExportParameterModel
            {
                SheetName = "各游戏总计",
                ListData = groupByGames.Select(x =>
                {
                    //呼叫这个 请确保栏位命名一致
                    var excelModel = x.ReturnInjecteObject<GameTaxLogGroupInfoExcelModel>();
                    return excelModel;
                })
            });

            Stream fileStream = SimpleExcelHelper.ConvertMultiModelToExcelFileStream(excelSheets);
            return fileStream;
        }

        /// <summary>
        /// 游戏房间税收统计
        /// </summary>
        public static Stream ConvertToGameServerTaxLogExcelStream(List<GameServerTaxLog> list, List<GameServerTaxLogGroupRoomInfoViewModel> groupByGameRooms)
        {
            var excelSheets = new List<ExcelExportParameterModel>();

            excelSheets.Add(new ExcelExportParameterModel
            {
                //IsAutoGenerateTotalSumRow = true, //透過此參數標記 是否要加總欄
                SheetName = "每日统计",
                ListData = list.Select(z => new GameServerTaxLogRowDataExcelModel
                {
                    Date = z.StatTime.ToString("yyyy-MM-dd"),
                    GroupDisplayName = z.ServerName,
                    Player_Balance_Amount = z.Amount,
                    Player_Balance_Tax = z.TaxAmount,
                    Player_Balance_WinLoss = z.Amount + z.TaxAmount,
                    Player_Balance_Flow = z.FlowAmount,
                    Robot_Balance_Amount = z.RobotAmount,
                    Robot_Balance_Tax = z.RobotTaxAmount,
                    Robot_Balance_WinLoss = z.RobotAmount + z.RobotTaxAmount,
                })
                .ToList()
            });

            excelSheets.Add(new ExcelExportParameterModel
            {
                SheetName = "各房间总计",
                ListData = groupByGameRooms.Select(x =>
                {
                    //呼叫这个 请确保栏位命名一致
                    var excelModel = x.ReturnInjecteObject<GameServerTaxLogGroupRoomInfoExcelModel>();
                    return excelModel;

                })
            });

            Stream fileStream = SimpleExcelHelper.ConvertMultiModelToExcelFileStream(excelSheets);
            return fileStream;
        }

        /// <summary>
        /// 金币流向每日统计匯出Excel
        /// </summary>
        public static Stream ConvertToMoneyStatLogExcelStream(List<MoneySourceLog> list, MoneySourceStatLogSummaryViewModel summary)
        {
            var excelSheets = new List<ExcelExportParameterModel>();

            excelSheets.Add(new ExcelExportParameterModel
            {
                SheetName = "金币流向每日统计",
                ListData = list.Select(x => new MoneySourceStatLogExcelRowModel
                {
                    Date = x.StatTime.ToString("yyyy-MM-dd"),
                    SourceID = x.SourceID,
                    SourceName = x.SourceType.SourceName,
                    ChangeType = x.SourceType.ChangeType.GetAttributeFromEnum<EnumDescriptionAttribute>().DisplayName,
                    Amount = x.Amount.ToCash()
                })
                .ToList()
            });

            excelSheets.Add(new ExcelExportParameterModel
            {
                SheetName = "各项总和",
                ListData = summary.GroupSummary.Select(x => new SourceTypeGroupSummaryExcelViewModel
                {
                    TypeName = x.TypeName,
                    Amount = x.Amount.ToCash(),
                })
            });

            Stream fileStream = SimpleExcelHelper.ConvertMultiModelToExcelFileStream(excelSheets);
            return fileStream;
        }

        /// <summary>
        /// 日總計匯出Excel
        /// </summary>
        /// <param name="list">日總計LOG資料</param>
        public static Stream ConvertToDailyMoneyLogExcelStream(List<DailyMoneyLog> list)
        {
            Stream fileStream = SimpleExcelHelper.ConvertModelToExcelFileStream(list);
            return fileStream;
        }

        /// <summary>
        /// 系統總報 日報表匯出Excel
        /// </summary>
        public static Stream ConvertToAllAreaDataDayExcelStream(List<AreaDataDay> list, IEnumerable<AreaDataDay> summary, string beginTime, string endTime)
        {
            var excelSheets = new List<ExcelExportParameterModel>();

            excelSheets.Add(new ExcelExportParameterModel
            {
                SheetName = "日报表",
                ListData = list.Select(x => new AllAreaDataDayExcelRowModel
                {
                    PlatformName = x.PlatformName,
                    AreaName = x.AreaName,
                    Date = x.StatTime.ToString("yyyy-MM-dd"),
                    TotalCharg = (x.OldCharge + x.NewCharge).ToCash(),
                    TotalNewCharge = x.TotalNewCharge.ToCash(),
                    TotalRegCount = x.TotalRegCount,
                    NewOrderUser = x.NewOrderUser,
                    TotalOrderUser = x.Total_OrderUser,
                    TotalAgentChargeAmount = x.TotalAgentChargeAmount.ToCash(),
                    TotalAgentChargeUser = x.TotalAgentChargeUser,
                    TotalConvert = (x.OldConvert + x.NewConvert).ToCash(),
                    TotalConvertUser = x.TotalConvertUser,
                    DAU = x.DAU,
                    TotalLoginIP = x.TotalLoginIP,
                    TotalBindUser = x.TotalBindUser,
                    OneDayKeep = x.OneDayKeep,
                    OneThirdDayKeep = x.OneThirdDayKeep
                })
                .ToList()
            });

            excelSheets.Add(new ExcelExportParameterModel
            {
                SheetName = "日报表总计",
                ListData = summary.Select(x => new AllAreaDataDayTotalExcelRowModel
                {
                    PlatformName = x.PlatformName,
                    AreaName = x.AreaName,
                    Date = beginTime + " ~ " + endTime,
                    TotalCharg = (x.OldCharge + x.NewCharge).ToCash(),
                    TotalNewCharge = x.TotalNewCharge.ToCash(),
                    TotalRegCount = x.TotalRegCount,
                    NewOrderUser = x.NewOrderUser,
                    TotalOrderUser = x.Total_OrderUser,
                    TotalAgentChargeAmount = x.TotalAgentChargeAmount.ToCash(),
                    TotalAgentChargeUser = x.TotalAgentChargeUser,
                    TotalConvert = (x.OldConvert + x.NewConvert).ToCash(),
                    TotalConvertUser = x.TotalConvertUser,
                    DAU = x.DAU,
                    TotalLoginIP = x.TotalLoginIP,
                    TotalBindUser = x.TotalBindUser
                })
            });

            Stream fileStream = SimpleExcelHelper.ConvertMultiModelToExcelFileStream(excelSheets);
            return fileStream;
        }

        /// <summary>
        /// 系統總報 月報表匯出Excel
        /// </summary>
        public static Stream ConvertToAllAreaDataMonthExcelStream(List<AreaDataMonth> list, SumAreaDataMonthView summary, string beginTime, string endTime)
        {
            var excelSheets = new List<ExcelExportParameterModel>();

            excelSheets.Add(new ExcelExportParameterModel
            {
                SheetName = "月报表",
                ListData = list.Select(row => new AreaDataMonthExcel
                {
                    PlatformName = row.PlatformName,
                    AreaName = row.AreaName,
                    StatTime = row.StatTime.ToString("yyyy-MM"),
                    TotalCharge = (row.OldCharge + row.NewCharge).ToCash(),
                    TotalConvert = (row.OldConvert + row.NewConvert).ToCash(),
                    TotalConvertTax = (row.OldConvertTax + row.NewConvertTax).ToCash(),
                    TotalLogin = row.OldLogin + row.NewLogin,
                    LoginMac = row.LoginMac,
                    LoginIP = row.LoginIP,
                    GameTax = row.GameTax.ToCash(),
                    ARPU = row.ARPU.ToCash(),
                    TotalRegCount = row.TotalRegCount,
                    NewOrderUser = row.NewOrderUser,
                    Total_OrderUser = row.Total_OrderUser
                })
            });

            excelSheets.Add(new ExcelExportParameterModel
            {
                SheetName = "依照渠道计算",
                ListData = summary.GroupSummary.Select(row => new AreaDataMonthExcel
                {
                    PlatformName = row.PlatformName,
                    AreaName = row.AreaName,
                    StatTime = beginTime + " - " + endTime,
                    TotalCharge = row.TotalCharge.ToCash(),
                    TotalConvert = row.TotalConvert.ToCash(),
                    TotalConvertTax = row.TotalConvertTax.ToCash(),
                    TotalLogin = row.TotalLogin,
                    LoginMac = row.LoginMac,
                    LoginIP = row.LoginIP,
                    GameTax = row.GameTax.ToCash(),
                    ARPU = row.ARPU.ToCash(),
                    TotalRegCount = row.TotalRegCount,
                    NewOrderUser = row.NewOrderUser,
                    Total_OrderUser = row.Total_OrderUser
                })
            });

            Stream fileStream = SimpleExcelHelper.ConvertMultiModelToExcelFileStream(excelSheets);
            return fileStream;
        }

        /// <summary>
        /// 系統總報 渠道日报匯出Excel
        /// </summary>
        public static Stream ConvertToAllAreaDataDaySummaryExcelStream(List<AreaDataDaySummary> list, IEnumerable<AreaDataDaySummary> summary, string beginTime, string endTime)
        {
            var excelSheets = new List<ExcelExportParameterModel>();

            excelSheets.Add(new ExcelExportParameterModel
            {
                SheetName = "每日统计",
                ListData = list.Select(x => new AllAreaDataDaySummaryExcelRowModel
                {
                    Date = x.StatTime.ToString("yyyy-MM-dd"),
                    AreaName = x.AreaName,
                    TotalRegCount = x.TotalRegCount,
                    LoginUser = (x.OldLogin + x.NewLogin),
					BindUser = (x.BindIOSUser + x.BindAndroidUser),
					BetCount = x.BetCount,
                    TotalOrderUser = x.Total_OrderUser,
                    NewOrderUser = x.NewOrderUser,
                    OldOrderUser = (x.Total_OrderUser - x.NewOrderUser),
                    NewCharge = (x.NewCharge).ToCash(),
                    OldCharge = x.OldCharge.ToCash(),
                    TotalCharge = x.TotalCharge.ToCash(),
                    TotalConvert = Math.Abs(x.TotalConvert).ToCash(),
                    GameTax = x.GameTax.ToCash(),
                    Income = x.Income.ToCash()
                })
                .ToList()
            });

            excelSheets.Add(new ExcelExportParameterModel
            {
                SheetName = "渠道总和",
                ListData = summary.Select(x => new AllAreaDataDaySummaryExcelRowModel
                {
                    Date = beginTime + " - " + endTime,
                    AreaName = x.AreaName,
                    TotalRegCount = x.TotalRegCount,
                    LoginUser = (x.OldLogin + x.NewLogin),
					BindUser = (x.BindIOSUser + x.BindAndroidUser),
					BetCount = x.BetCount,
                    TotalOrderUser = x.Total_OrderUser,
                    NewOrderUser = x.NewOrderUser,
                    OldOrderUser = (x.Total_OrderUser - x.NewOrderUser),
                    NewCharge = (x.NewCharge).ToCash(),
                    OldCharge = x.OldCharge.ToCash(),
                    TotalCharge = x.TotalCharge.ToCash(),
                    TotalConvert = Math.Abs(x.TotalConvert).ToCash(),
                    GameTax = x.GameTax.ToCash(),
                    Income = x.Income.ToCash()
                })
            });

            Stream fileStream = SimpleExcelHelper.ConvertMultiModelToExcelFileStream(excelSheets);
            return fileStream;
        }


        public static Stream ConvertToBankSourceStatLogExcelStream(List<BankSourceLog> list, List<BankSourceLog> summary)
        {
            var excelSheets = new List<ExcelExportParameterModel>();

            excelSheets.Add(new ExcelExportParameterModel
            {
                SheetName = "保險箱流向统计",
                ListData = list.Select(x => new BankSourceStatLogExcelModel
                {
                    Date = x.StatTime.ToString("yyyy-MM-dd"),
                    SourceID = x.SourceID,
                    SourceName = x.SourceType.SourceName,
                    ChangeType = x.SourceType.ChangeType.GetAttributeFromEnum<EnumDescriptionAttribute>().DisplayName,
                    Amount = x.Amount
                })
                .ToList()
            });

            excelSheets.Add(new ExcelExportParameterModel
            {
                SheetName = "各流向加总",
                ListData = summary.Select(x => new BankSourceStatLogGroupBySourceExcelModel
                {
                    SourceID = x.SourceID,
                    SourceName = x.SourceType.SourceName,
                    ChangeType = x.SourceType.ChangeType.GetAttributeFromEnum<EnumDescriptionAttribute>().DisplayName,
                    Amount = x.Amount
                })
                .ToList()
            });

            Stream fileStream = SimpleExcelHelper.ConvertMultiModelToExcelFileStream(excelSheets);
            return fileStream;
        }




    }




}