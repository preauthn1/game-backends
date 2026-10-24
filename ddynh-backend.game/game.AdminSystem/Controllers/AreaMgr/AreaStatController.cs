using System;
using example.invalid;
using System.Linq;
using System.Collections.Generic;
using System.Web.Mvc;
using FW.Core.Helper;
using game.Entity;
using game.Entity.AdminSys;
using game.Entity.Stat;
using game.Infrastructure.Extensions;
using game.Infrastructure;

namespace game.AdminSystem.Controllers.AreaMgr
{
    public class AreaStatController : BaseController
    {
        public ActionResult StatDataDay(int platformID = 0, int areaID = 0, DateTime? beginTime = null, DateTime? endTime = null, int p = 1)
        {
            if (beginTime == null)
                beginTime = DateTime.Now.AddDays(-7);

            return View(SiteHelper.GetAPIResult<PagedList<AreaDataDay>>(string.Format("/AreaStat/GetAreaDataDay?platformID={0}&areaID={1}&beginTime={2}&endTime={3}&pageIndex={4}&sortName={5}&sortLifting={6}", platformID, areaID, beginTime, endTime, p, SortName.ChargeAmount, SortLifting.DESC)));
        }

        //public ActionResult StatDataMonth(int platformID = 0, int areaID = 0, DateTime? beginTime = null, DateTime? endTime = null, int p = 1)
        //{
        //    if (beginTime == null)
        //        beginTime = DateTime.Now.AddMonths(-6);

        //    return View(SiteHelper.GetAPIResult<PagedList<AreaDataMonth>>(string.Format("/AreaStat/GetAreaDataMonth?platformID={0}&areaID={1}&beginTime={2}&endTime={3}&pageIndex={4}&sortName={5}&sortLifting={6}", platformID, areaID, beginTime, endTime, p, SortName.ChargeAmount, SortLifting.DESC)));
        //}

        public ActionResult StatAllAreaDataDay(string btnSearch, int platformID = 0, int areaID = 0, DateTime? beginTime = null, DateTime? endTime = null, int p = 1, SortName sortName = SortName.Area, SortLifting sortLifting = SortLifting.ASC)
        {
            if (btnSearch == "today")
            {
                beginTime = DateTime.Now;
                endTime = DateTime.Now;
            }
            else if (btnSearch == "yesterday")
            {
                beginTime = DateTime.Now.AddDays(-1);
                endTime = DateTime.Now.AddDays(-1);
            }
            else
            {
                if (beginTime == null) beginTime = Convert.ToDateTime(DateTime.Now);
            }
            var operatorID = CurrentUser.UserID;
            ViewBag.CurrentSort = sortName;
            ViewBag.SortLifting = Convert.ToBoolean((int)sortLifting);

            beginTime = (beginTime.HasValue) ? beginTime : DateTime.Now;
            ViewBag.BeginTimeStr = beginTime.Value.ToString("yyyy-MM-dd");

            endTime = (endTime.HasValue) ? endTime: DateTime.Now;
            ViewBag.EndTimeStr = endTime.Value.ToString("yyyy-MM-dd");
            
            var url = $"/AreaStat/GetAreaDataDay?operatorID={operatorID}&platformID={platformID}&areaID={areaID}&beginTime={beginTime}&endTime={endTime}&sortName={sortName}&sortLifting={sortLifting}";
            var pageList = SiteHelper.GetAPIResult<PagedList<AreaDataDay>>(url + $"&pageIndex={p}");


            int takeSize = pageList.PageData.Count() - 1; //因為包含了total總加總那一欄 所以要減1
            var pageTotal = pageList.PageData.Take(takeSize).CalculateTotalSumMergeToOneObject<AreaDataDay>();
            pageTotal.PlatformName = "当页加总";
            pageTotal.AreaName = "当页加总";


            int pageItemCount = pageList.PageData.Count() - 1;

            //因為寫法的問題 當頁加總要塞進倒數第二列
            pageList.PageData.Insert(pageItemCount, pageTotal);

            //Summary
            var summaryList = SiteHelper.GetAPIResult<PagedList<AreaDataDay>>(url + $"&pageIndex={1}&pageSize={10000}").PageData;
            var summary = from pro in summaryList
                          group pro by new { pro.PlatformID, pro.AreaName, pro.PlatformName } into log
                          select new AreaDataDay
                          {
                              PlatformName = log.Key.PlatformName,      //所属平台 
                              AreaName = log.Key.AreaName,              //所属渠道
                              OldCharge = log.Sum(pro => pro.OldCharge),
                              NewCharge = log.Sum(pro => pro.NewCharge),
                              TotalNewCharge = log.Sum(pro => pro.TotalNewCharge),                      //当日新增充值金额
                              TotalRegCount = log.Sum(pro => pro.TotalRegCount),                        //註冊用戶人数
                              NewOrderUser = log.Sum(pro => pro.NewOrderUser),                          //註冊用戶充值人数
                              Total_OrderUser = log.Sum(pro => pro.Total_OrderUser),                    //每日充值人数
                              TotalAgentChargeAmount = log.Sum(pro => pro.TotalAgentChargeAmount),      //币商充值
                              TotalAgentChargeUser = log.Sum(pro => pro.TotalAgentChargeUser),           //币商充值人数
                              // = (log.Key.OldConvert + log.Key.NewConvert).ToCash(),                  //兑换金额
                              OldConvert = log.Sum(pro => pro.OldConvert),
                              NewConvert = log.Sum(pro => pro.NewConvert),
                              TotalConvertUser = log.Sum(pro => pro.TotalConvertUser),                  //兑换人数
                              DAU = log.Sum(pro => pro.DAU),                                            //活跃人数
                              TotalLoginIP = log.Sum(pro => pro.TotalLoginIP),                          //注册IP数
                              TotalBindUser = log.Sum(pro => pro.TotalBindUser)                        //绑定手机数
                          };
            ViewBag.Summary = summary;

            //若選擇匯出Excel
            if (btnSearch == "excel")
            {
                var exportList = SiteHelper.GetAPIResult<PagedList<AreaDataDay>>(url + $"&pageIndex={1}&pageSize={10000}");
                string fileName = ExcelModelConvertHelper.GenerateExcelFileName("日报表", beginTime.Value, endTime.Value);
                Stream stream = ExcelModelConvertHelper.ConvertToAllAreaDataDayExcelStream(exportList.PageData, summary, beginTime.Value.ToString("yyyy-MM-dd"), endTime.Value.ToString("yyyy-MM-dd"));
                return File(stream, SystemConst.Excel_Format_ContentType, fileName);
            }

            return View(pageList);
        }

        [NoAuthorization]
        public ActionResult StatAllAreaDataDaySummary(int? platformID = null, int? areaID = null, DateTime? beginTime = null, DateTime? endTime = null, string btnSearch = null)
        {
            var operatorID = CurrentUser.UserID;
            ViewBag.operatorID = Convert.ToInt32(operatorID);

            string url = $"/AreaStat/GetAreaDataDaySummary?operatorID={operatorID}&platformID={platformID}&areaID={areaID}&beginTime={beginTime}&endTime={endTime}";

            var list = SiteHelper.GetAPIResult<List<AreaDataDaySummary>>(url);

            beginTime = (!beginTime.HasValue) ? DateTime.Now : beginTime;
            endTime = (!endTime.HasValue) ? DateTime.Now : endTime;
            //計算加總
            ViewBag.BeginTimeStr = beginTime.Value.ToString("yyyy-MM-dd");
            ViewBag.EndTimeStr = endTime.Value.ToString("yyyy-MM-dd");

            var summary = from pro in list
                          group pro by new { pro.AreaID, pro.AreaName } into log
                          select new AreaDataDaySummary
                          {
                              AreaID = log.Key.AreaID,
                              AreaName = log.Key.AreaName,
                              //營收
                              Income = log.Sum(pro => pro.Income),              //營收
                              TotalMoney = log.Sum(pro => pro.TotalMoney),      //用戶金币
                              TotalBankMoney = log.Sum(pro => pro.TotalBankMoney),        //用戶保險箱金币
                              //充值
                              Total_OrderUser = log.Sum(pro => pro.Total_OrderUser),    //不重複充值人數
                              TotalOrderCounts = log.Sum(pro => pro.TotalOrderCounts),    //充值成功筆數
                              NewOrderUser = log.Sum(pro => pro.NewOrderUser),    //新用户不重複充值人數
                              TotalCharge = log.Sum(pro => pro.TotalCharge),    //總充值金額
                              OldCharge = log.Sum(pro => pro.OldCharge),    //舊用戶充值金額(已經包含代理充值數據)
                              NewCharge = log.Sum(pro => pro.NewCharge),    //新用戶充值金額(已經包含代理充值數據)
                              OldAgentCharge = log.Sum(pro => pro.OldAgentCharge),    //舊用戶代理充值金額
                              NewAgentCharge = log.Sum(pro => pro.NewAgentCharge),    //新用戶代理充值金額
                              //兌換
                              TotalConvertUser = log.Sum(pro => pro.TotalConvertUser),    //兌換去重用戶數
                              NewConvertUser = log.Sum(pro => pro.NewConvertUser),    //新用戶兌換去重用戶數
                              TotalConvert = log.Sum(pro => pro.TotalConvert),    //總兌換金額
                              OldConvert = log.Sum(pro => pro.OldConvert),    //舊用戶兌換金額
                              NewConvert = log.Sum(pro => pro.NewConvert),    //新用戶兌換金額
                              OldConvertTax = log.Sum(pro => pro.OldConvertTax),    //舊用戶兌換手續費金額
                              NewConvertTax = log.Sum(pro => pro.NewConvertTax),    //新用戶兌換手續費金額
                              //註冊用戶數
                              TotalRegCount = log.Sum(pro => pro.TotalRegCount),    //當日註冊用戶總數
                              IOSRegCount = log.Sum(pro => pro.IOSRegCount),    //當日註冊IOS用戶數
                              AndroidRegCount = log.Sum(pro => pro.AndroidRegCount),    //當日註冊安卓用戶數
                              TotalBindUser = log.Sum(pro => pro.TotalBindUser),    //當日綁定用戶數
                              BindIOSUser = log.Sum(pro => pro.BindIOSUser),    //當日綁定IOS用戶數
                              BindAndroidUser = log.Sum(pro => pro.BindAndroidUser),    //當日綁定安卓用戶數
                              //登錄用戶數
                              OldLogin = log.Sum(pro => pro.OldLogin),    //舊用戶数登入游戏
                              NewLogin = log.Sum(pro => pro.NewLogin),    //新用户数登入游戏
                              IOSCount = log.Sum(pro => pro.IOSCount),    //蘋果註冊數且登入遊戲
                              NewiOSCount = log.Sum(pro => pro.NewiOSCount),    //新用戶蘋果註冊數且登入遊戲
                              AndroidCount = log.Sum(pro => pro.AndroidCount),    //安卓註冊數且登入遊戲
                              NewAndroidCount = log.Sum(pro => pro.NewAndroidCount),    //新用戶安卓註冊數且登入遊戲
                              Keep1 = log.Sum(pro => pro.Keep1),    //1日留存
                              Keep3 = log.Sum(pro => pro.Keep3),    //3日留存
                              Keep5 = log.Sum(pro => pro.Keep5),    //5日留存
                              Keep7 = log.Sum(pro => pro.Keep7),    //7日留存
							  //有投注人數
							  BetCount = log.Sum(pro => pro.BetCount),    //投注人數
                              //稅收
                              GameTax = log.Sum(pro => pro.GameTax),    //系统税收, 用戶(不含機器人)游戏税收
                              UserGameAmount = log.Sum(pro => pro.UserGameAmount),    //用戶輸贏, 用戶(不含機器人)遊戲輸贏金額
                              RobotGameAmount = log.Sum(pro => pro.RobotGameAmount),    //系統輸贏, 機器人遊戲輸贏金額
                              AmountTotal = log.Sum(pro => pro.AmountTotal)    //遊戲輸贏, 用戶+机器人輸贏總金額
                          };
            ViewBag.Summary = summary;

            //若選擇匯出Excel
            if (btnSearch == "excel")
            {
                string fileName = ExcelModelConvertHelper.GenerateExcelFileName("渠道日报", beginTime.Value, endTime.Value);
                Stream stream = ExcelModelConvertHelper.ConvertToAllAreaDataDaySummaryExcelStream(list, summary, beginTime.Value.ToString("yyyy-MM-dd"), endTime.Value.ToString("yyyy-MM-dd"));
                return File(stream, SystemConst.Excel_Format_ContentType, fileName);
            }

            return View(list);
        }


        private SumAreaDataMonthView SumStatAllAreaDataMonth(List<AreaDataMonth> list)
        {
            var summary = new SumAreaDataMonthView();

            summary.GroupSummary = (from row in list
                             group row by new {row.AreaID, row.AreaName } into log
                             select new SumAreaDataMonthLog
                             {
                                 PlatformID = log.FirstOrDefault().PlatformID,
                                 PlatformName = log.FirstOrDefault().PlatformName,
                                 AreaID = log.Key.AreaID,
                                 AreaName = log.Key.AreaName,
                                 OldCharge = log.Sum(row => row.OldCharge),
                                 NewCharge = log.Sum(row => row.NewCharge),
                                 OldConvert = log.Sum(row => row.OldConvert),
                                 NewConvert = log.Sum(row => row.NewConvert),
                                 OldConvertTax = log.Sum(row => row.OldConvertTax),
                                 NewConvertTax = log.Sum(row => row.NewConvertTax),
                                 OldLogin = log.Sum(row => row.OldLogin),
                                 NewLogin = log.Sum(row => row.NewLogin),
                                 LoginMac = log.Sum(row => row.LoginMac),
                                 LoginIP = log.Sum(row => row.LoginIP),
                                 GameTax = log.Sum(row => row.GameTax),
                                 ARPU = log.Sum(row => row.ARPU),
                                 TotalRegCount = log.Sum(row => row.TotalRegCount),
                                 NewOrderUser = log.Sum(row => row.NewOrderUser),
                                 Total_OrderUser = log.Sum(row => row.Total_OrderUser)
                                 
                             }).ToList();
            return summary;
        }

        public ActionResult StatAllAreaDataMonth(int platformID = 0, int areaID = 0, string btnSearch = "search",DateTime? beginTime = null, DateTime? endTime = null, int p = 1, SortName sortName = SortName.ChargeAmount, SortLifting sortLifting = SortLifting.DESC)
        {
            var operatorID = CurrentUser.UserID;
            if (beginTime == null)
                beginTime = DateTime.Now.AddMonths(-6);

            if (endTime == null)
                endTime = DateTime.Now;

            ViewBag.CurrentSort = sortName;
            ViewBag.SortLifting = Convert.ToBoolean((int)sortLifting);

            var url = $"/AreaStat/GetAreaDataMonth?operatorID={operatorID}&platformID={platformID}&areaID={areaID}&beginTime={beginTime}&endTime={endTime}&sortName={sortName}&sortLifting={sortLifting}";
            var pagedList = SiteHelper.GetAPIResult<PagedList<AreaDataMonth>>(url + $"&pageIndex={p}");
            var list = SiteHelper.GetAPIResult<PagedList<AreaDataMonth>> (url + $"&pageIndex={1}&pageSize={10000}");

            SumAreaDataMonthView summary = SumStatAllAreaDataMonth(list.PageData);
            summary.BeginTime = beginTime.Value.ToString("yyyy-MM-dd");
            summary.EndTime = endTime.Value.ToString("yyyy-MM-dd");

            ViewBag.Summary = summary;

            if (btnSearch == "excel")
            {
                string fileName = ExcelModelConvertHelper.GenerateExcelFileName("月报表", beginTime.Value, endTime.Value);
                Stream stream = ExcelModelConvertHelper.ConvertToAllAreaDataMonthExcelStream(list.PageData, summary, beginTime.Value.ToString("yyyy-MM-dd"), endTime.Value.ToString("yyyy-MM-dd"));
                return File(stream, SystemConst.Excel_Format_ContentType, fileName);
            }

            return View(pagedList);
        }


        public ActionResult StatAllAreaData(int? platformids, int? areaID, string btnSearch, DateTime? beginTime = null, DateTime? endTime = null)
        {
            if (btnSearch == "today")
            {
                beginTime = DateTime.Now;
                endTime = DateTime.Now;
            }
            else if (btnSearch == "yesterday")
            {
                beginTime = DateTime.Now.AddDays(-1);
                endTime = DateTime.Now.AddDays(-1);
            }
            else
            {
                if (beginTime == null) beginTime = DateTime.Now.AddDays(-3);
                if (endTime == null) endTime = DateTime.Now;
            }
            var operatorID = CurrentUser.UserID;
            var list = SiteHelper.GetAPIResult<List<DailyMoneyLog>>($"/AreaStat/GetDailyMoneyLog?operatorID={operatorID}&platformID={platformids}&areaID={areaID}&beginTime={beginTime.Value.ToString("yyyy-MM-dd")}&endTime={endTime.Value.ToString("yyyy-MM-dd")}");

            if (btnSearch == "excel")
            {
                string fileName = ExcelModelConvertHelper.GenerateExcelFileName("日总计", beginTime.Value, endTime.Value);
                Stream stream = ExcelModelConvertHelper.ConvertToDailyMoneyLogExcelStream(list);
                return File(stream, SystemConst.Excel_Format_ContentType, fileName);
            }

            return View(list);
        }

        public ActionResult TotalGameOnLineLog(int? areaID = null, DateTime? beginTime = null, DateTime? endTime = null)
        {
            if (areaID == null || !areaID.HasValue)
                return View();

            if (!SiteHelper.GetAdminAreaIDs().Contains(areaID.Value))
                return Alert("非法访问");

            if (beginTime == null) beginTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00"));

            return View(SiteHelper.GetAPIResult<List<TotalGameOnLineLogArea>>(string.Format("/OnLineStat/GetTotalGameOnLineLogArea?areaID={0}&beginTime={1}&endTime={2}", areaID, beginTime, endTime)));
        }        

        public ActionResult CompanyReport(QueryCompanyReportAdminSysRequestModel filter = null)
        {
            filter = filter ?? new QueryCompanyReportAdminSysRequestModel();

            //預設日期
            if (!filter.BeginTime.HasValue)
                filter.BeginTime = DateTime.Now.Date;

            if (!filter.EndTime.HasValue)
                filter.EndTime = DateTime.Now.Date.AddDays(1);

            if( (filter.EndTime.Value - filter.BeginTime.Value).TotalDays >= 180)
            {
                return Alert("时间区间范围不得超过180天。");
            }

            int operatorID = GetCurrentUserID();

            string dateBegin = filter.BeginTime.ToDateTimeStringOrEmpty();
            string dateEnd = filter.EndTime.ToDateTimeStringOrEmpty();

            string url = $"/StatReport/GetCompanayReport?AdminID={operatorID}&AreaID={filter.AreaID}&PlatformID={filter.PlatformID}&beginTime={dateBegin}&endTime={dateEnd}";
            var queryApi = SiteHelper.GetAPIResult<List<CompanyReportSPResultModel>>(url);


            //將api查出的結果轉成 admin畫面上的格式
            var viewModels = queryApi
                .OrderBy(z=> z.Date)
                .Select(z =>
                {
                    var convert = z.ReturnInjecteObject<CompanyReportTableRowViewModel>();
                    convert.Date = z.Date.ToString("yyyy-MM-dd");

                    return convert;
                })
                .ToList();

            var groupResults = viewModels
                .GroupBy(z => z.AreaName)
                .Select(x =>
                {
                    var totalSumModel = x.ToList().CalculateTotalSumMergeToOneObject<CompanyReportGroupByResultViewModel>();
                    totalSumModel.AreaName = x.FirstOrDefault().AreaName;

                    return totalSumModel;
                })
                .ToList();


            if (filter.IsExport)
            {
                string fileName = ExcelModelConvertHelper.GenerateExcelFileName("公司总计报表", filter.BeginTime.Value, filter.EndTime.Value);
                Stream stream = ExcelModelConvertHelper.ConvertToCompanyReportExcelStream(viewModels, groupResults);
                return File(stream, SystemConst.Excel_Format_ContentType, fileName);
            }

            var viewModel = new CompanyReportViewModel
            {
                QueryBeginTime = filter.BeginTime.Value.ToString("yyyy-MM-dd"),
                QueryEndTime = filter.EndTime.Value.ToString("yyyy-MM-dd"),
                Items = viewModels,
                GroupByResult = groupResults
            };

            return View(viewModel);
        }




    }
}