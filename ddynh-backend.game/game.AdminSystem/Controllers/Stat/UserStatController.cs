using System;
using System.Collections.Generic;
using example.invalid;
using System.Linq;
using System.Web.Mvc;
using FW.Core.Helper;
using FW.Core.Helper.Enum;
using game.Entity;
using game.Entity.AdminSys;
using game.Entity.Stat;
using game.Entity.Users;
using game.Infrastructure;
using NPOI.XSSF.UserModel;

namespace game.AdminSystem.Controllers.Stat
{
    /// <summary>
    /// 统计分析-用户统计
    /// </summary>
    public class UserStatController : BaseController
    {
        /// <summary>
        /// 3-登录记录查询
        /// </summary>
        public ActionResult UserLogonLog(int? userID = null, string loginName = null, string nickName = null, DateTime? beginTime = null, DateTime? endTime = null, int p = 1)
        {
            if (beginTime == null) beginTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00"));
            userID = UserService.GetUserID(userID, loginName, nickName);
            var list = SiteHelper.GetAPIResult<PagedList<LogonLog>>(string.Format("/UserStat/GetUserLogonLog?userID={0}&beginTime={1}&endTime={2}&pageIndex={3}"
                , userID, beginTime, endTime, p));
            return View(list);
        }

        /// <summary>
        /// 3-注册账号统计
        /// </summary>
        public ActionResult GetRegUserLog( DateTime? beginTime, DateTime? endTime, string btnSearch, int platformids = 0)
        {
            if (btnSearch == "today")
            {
                beginTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));
            }
            else if (btnSearch == "yesterday")
            {
                beginTime = Convert.ToDateTime(DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd"));
                endTime = Convert.ToDateTime(DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd"));
            }
            else
            {
                if (endTime == null || endTime > DateTime.Now) endTime = Convert.ToDateTime(DateTime.Now.ToShortDateString()).AddSeconds(1);
                if (beginTime == null) beginTime = Convert.ToDateTime(DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd"));
            }

            var operatorID = CurrentUser.UserID;

            var list = SiteHelper.GetAPIResult<List<RegUserLog>>($"/UserStat/GetRegUserLog?operatorID={operatorID}&beginTime={beginTime}&endTime={endTime}&platformID={platformids}");
            return View(list);
        }

        /// <summary>
        /// 3-登录账号统计
        /// </summary>
        public ActionResult GetLogonUserLog( DateTime? beginTime, DateTime? endTime, string btnSearch, int platformids = 0)
        {
            if (btnSearch == "today")
            {
                beginTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));
            }
            else if (btnSearch == "yesterday")
            {
                beginTime = Convert.ToDateTime(DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd"));
                endTime = Convert.ToDateTime(DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd"));
            }
            else
            {
                if (endTime == null || endTime > DateTime.Now) endTime = Convert.ToDateTime(DateTime.Now.ToShortDateString()).AddSeconds(1);
                if (beginTime == null) beginTime = Convert.ToDateTime(DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd"));
            }
            var operatorID = CurrentUser.UserID;

            var list = SiteHelper.GetAPIResult<List<LogonUserLog>>($"/UserStat/GetLogonUserLog?operatorID={operatorID}&beginTime={beginTime}&endTime={endTime}&platformID={platformids}");
            return View(list);
        }

        /// <summary>
        /// 用户综合查询
        /// </summary>
        public ActionResult AllUser(QueryAllUsersSPRequestViewModel filter, int p = 1 ,
            [ModelBinder(typeof(MoneyModeBinder))]long? chargeAmountBegin = null, 
            [ModelBinder(typeof(MoneyModeBinder))]long? chargeAmountEnd = null, 
            [ModelBinder(typeof(MoneyModeBinder))]long? convertAmountBegin = null, 
            [ModelBinder(typeof(MoneyModeBinder))]long? convertAmountEnd = null)
        {
            filter = filter ?? new QueryAllUsersSPRequestViewModel();
            filter.PageIndex = p;
            filter.chargeAmountBegin = chargeAmountBegin;
            filter.chargeAmountEnd = chargeAmountEnd;
            filter.convertAmountBegin = convertAmountBegin;
            filter.convertAmountEnd = convertAmountEnd;
            
            var gradeSettingList = SiteHelper.GetAPIResult<List<GradeSetting>>("/UserStat/GetGradeSetting");
            ViewBag.GradeSettingList = gradeSettingList;

            filter.AdminID = Convert.ToInt32(CurrentUser.UserID);

            var query = GetUser2(filter);
            return View(query);
        }

        public ActionResult BlockUser(string msg, params int[] Id)
        {
            var adminLoginName = FW.Core.Engine.EngineContext.Current.Resolve<FW.Core.Auth.AuthManager>().CurrentUser.LoginName;
            string memo = string.Format("{0} 封锁帐号，原因：{1}", this.CurrentUser.LoginName, msg);

            Array.ForEach(Id, s =>
            {
                var api = SiteHelper.GetAPIResult<APIResult>(string.Format("/ForbidUser/ForbidUser?userID={0}&memo={1}&ipAddress={2}", s, Server.UrlEncode(memo), Server.UrlEncode(SystemHelper.UserIPAddress)));
            });
            return Json(new { status = true, msg = "操作成功" }, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 3-登录用户查询
        /// </summary>
        public ActionResult LoginUser(int? userID = null, string loginName = null, string nickName = null, DateTime? lastlogonBeginTime = null, DateTime? lastlogonEndTime = null, HDType? hdType = null, string lastlogonIPAddress = null, int p = 1)
        {
            if (lastlogonBeginTime == null) lastlogonBeginTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00"));
            //return View(GetUser(userID, loginName, nickName, null, null, lastlogonBeginTime, lastlogonEndTime, null, null, hdType, null, lastlogonIPAddress, null, null, null, null, null, null, null, null, null, null, null, p));
            return View();
        }

        /// <summary>
        /// 3-注册用户查询
        /// </summary>
        public ActionResult RegUser(int? platformids, string submitType, int? userID = null, int? areaID = null, string loginName = null, string nickName = null, DateTime? regBeginTime = null, DateTime? regEndTime = null, HDType? hdType = null, string regIpAddress = null, int p = 1, SortName sortName = SortName.StatTime, SortLifting sortLifting = SortLifting.DESC)
        {

            if (regBeginTime == null) regBeginTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00"));

            var adminId = SiteService.GetAdminUserID();

            switch (submitType)
            {
                case "Export":
                    return exportRegUser(platformids, areaID, adminId, regBeginTime.Value, userID, regEndTime, regIpAddress, hdType, p, sortName, sortLifting);
                default:
                    userID = UserService.GetUserID(userID, loginName, nickName);
                    var regChargeInfo = SiteHelper.GetAPIResult<RegCountLog>(string.Format("/User/GetUserRegChargeInfo?regBeginTime={0}&regEndTime={1}&areaID={2}&hdType={3}&platformId={4}&adminId={5}", regBeginTime, regEndTime, areaID, hdType, platformids, adminId));

                    ViewBag.ChargeCount = regChargeInfo.ChargeCount;
                    ViewBag.RegCount = regChargeInfo.RegCount;
                    ViewBag.CurrentSort = sortName;
                    ViewBag.SortLifting = Convert.ToBoolean((int)sortLifting);

                    return View(GetRegUser(platformids, areaID, adminId, userID, regBeginTime, regEndTime, regIpAddress, hdType, p, sortName, sortLifting));
            }
        }
        private PagedList<RegUserInfo> GetRegUser(int? platformid, int? areaID, int? adminId, int? userID = null, DateTime? regBeginTime = null, DateTime? regEndTime = null,
          string regIpAddress = null, HDType? hdType = null, int pageIndex = 1, SortName sortName = SortName.StatTime, SortLifting sortLifting = SortLifting.DESC)
        {
            return SiteHelper.GetAPIResult<PagedList<RegUserInfo>>(
                string.Format("/User/GetRegUser?platformid={0}&areaID={1}&adminId={2}&userID={3}&regBeginTime={4}&regEndTime={5}&regIpAddress={6}&hdType={7}&pageIndex={8}&sortName={9}&sortLifting={10}"
                , platformid, areaID, adminId, userID, regBeginTime, regEndTime, regIpAddress, hdType, pageIndex, sortName, sortLifting));
        }

        private PagedList<UserInfo> GetUser(int? userID = null, string loginName = null, string nickName = null, DateTime? regBeginTime = null, DateTime? regEndTime = null, DateTime? lastlogonBeginTime = null, DateTime? lastlogonEndTime = null,
            string regIpAddress = null, UserType? userType = null, HDType? hdType = null, string hdCode = null, string lastlogonIPAddress = null, Sex? sex = null, decimal? convertChargeRateBegin = null, decimal? convertChargeRateEnd = null,
            long? chargeAmountBegin = null, long? chargeAmountEnd = null, long? convertAmountBegin = null, long? convertAmountEnd = null, int? gradeID = null, string alipayAccount = null, string realName = null, int? areaID = null, int pageIndex = 1)
        {
            return SiteHelper.GetAPIResult<PagedList<UserInfo>>(
                string.Format("/User/GetUser?userID={0}&loginName={1}&nickName={2}&regBeginTime={3}&regEndTime={4}&lastlogonBeginTime={5}&lastlogonEndTime={6}&regIpAddress={7}&userType={8}&hdType={9}&hdCode={10}&lastlogonIPAddress={11}&sex={12}&convertChargeRateBegin={13}&convertChargeRateEnd={14}&chargeAmountBegin={15}&chargeAmountEnd={16}&convertAmountBegin={17}&convertAmountEnd={18}&gradeID={19}&alipayAccount={20}&realName={21}&areaID={22}&pageIndex={23}"
                , userID, loginName, nickName, regBeginTime, regEndTime, lastlogonBeginTime, lastlogonEndTime, regIpAddress, userType, hdType, hdCode, lastlogonIPAddress, sex, convertChargeRateBegin, convertChargeRateEnd, chargeAmountBegin, chargeAmountEnd, convertAmountBegin, convertAmountEnd, gradeID, alipayAccount, realName, areaID, pageIndex));
        }

        private PagedList<UserInfo2> GetUser2(QueryAllUsersSPRequestViewModel filter)
        {
            string apiUrl = $"/User/GetUser2?{SiteHelper.GeneratePageQueryString(filter)}&{SiteHelper.GenerateQueryStringParameter(filter)}";

            var query = SiteHelper.GetAPIResult<PagedList<UserInfo2>>(apiUrl);

            return query;
        }

        /// <summary>
        /// 留存率统计
        /// </summary>
        public ActionResult GetKeepLog(DateTime? beginTime, DateTime? endTime, int? platformID = 0)
        {
            var adminId = SiteService.GetAdminUserID();
            if (endTime == null || endTime > DateTime.Now) endTime = Convert.ToDateTime(DateTime.Now.ToShortDateString()).AddSeconds(1);
            if (beginTime == null) beginTime = Convert.ToDateTime(DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd"));
            var list = SiteHelper.GetAPIResult<List<KeepLog>>(string.Format("/UserStat/GetKeepLog?adminId={0}&beginTime={1}&endTime={2}&platformID={3}", adminId, beginTime, endTime, platformID));
            return View("KeepLog", list);
        }

        /// <summary>
        /// 充直留存率统计
        /// </summary>
        public ActionResult GetChargeKeepLog(string hdType = null, int? platformID = null)
        {
            string value = "";
            if (!String.IsNullOrEmpty(hdType))
            {
                if (hdType.ToLower() == "android") value = "2";
                if (hdType.ToLower() == "ios") value = "3";
            }
            int operatorID = Convert.ToInt32(CurrentUser.UserID);
            ViewBag.operatorID = operatorID;
            var list = SiteHelper.GetAPIResult<List<ChargeKeepLog>>(string.Format("/UserStat/GetChargeKeepLog?hdType={0}&platformID={1}&adminID={2}", value, platformID, operatorID));
            return View("ChargeKeepLog", list);
        }

        /// <summary>
        /// 有效會員留存率统计
        /// </summary>
        public ActionResult GetActiveUserKeepLog(int? platformID)
        {
            int operatorID = Convert.ToInt32(CurrentUser.UserID);
            ViewBag.operatorID = operatorID;
            var list = SiteHelper.GetAPIResult<List<ActiveUserKeepLog>>(string.Format($"/UserStat/GetActiveUserKeepLog?platformID={platformID}&adminID={operatorID}"));
            return View("ActiveUserKeepLog", list);
        }


        public ActionResult SearchUserStatInfo(byte searchType = 1, int count = 5)
        {
            int AdminID = Convert.ToInt32(CurrentUser.UserID);
            ViewBag.SearchType = searchType;
            if (count < 5) { count = 5; }
            var list = SiteHelper.GetAPIResult<Dictionary<string, string>>(string.Format("/UserStat/SearchUserStatInfo?searchType={0}&count={1}&AdminID={2}", searchType, count, AdminID));
            return View(list);
        }

        /// <summary>
        /// 用户等级统计
        /// </summary>
        /// <returns></returns>
        public ActionResult GetUserGradeCount()
        {
            int AdminID = Convert.ToInt32(CurrentUser.UserID);
            string Url = string.Format("/UserStat/GetUserGradeCount?AdminID={0}", AdminID);
            var list = SiteHelper.GetAPIResult<List<GradeSetting>>(Url);
            return View(list);
        }

        /// <summary>
        /// 取得用戶資訊
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetUserInfo(int? areaID, int? platformID, DateTime? startTime, long? totalCharge, int? totalLogin, int draw, int start = 0, int length = 20)
        {
            #region 參數錯誤
            if (!areaID.HasValue || !platformID.HasValue || !startTime.HasValue || (!totalCharge.HasValue && !totalLogin.HasValue))
            {
                return Json(new { success = false });
            }
            #endregion

            long? _totalCharge = null;
            int? _totalLogin = null;
            var _startTime = startTime.Value;
            var _pageIndex = Math.Ceiling((decimal)start / length) + 1;

            if (totalLogin.HasValue) { _totalLogin = totalLogin.Value; }
            if (totalCharge.HasValue) { _totalCharge = totalCharge.Value; }

            var _api = SiteHelper.GetAPIResult<PagedList<UserInfoForReport>>(string.Format("/User/GetUserInfo?areaID={0}&platformID={1}&startTime={2}&totalCharge={3}&totalLogin={4}&pageIndex={5}&pageSize={6}",
                 areaID.Value, platformID.Value, _startTime.ToString("yyyy-MM-dd"), _totalCharge, _totalLogin, _pageIndex, length));


            var _newApi = _api.PageData.Select(pro => new
            {
                CreateTime = pro.CreateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                pro.UserID,
                SumCharge = pro.SumCharge.ToCash(),
                SumConvert = pro.SumConvert.ToCash(),
                pro.SumChargeByQuery,
                Sex = pro.Sex.GetDisplayName(),
                HDType = pro.HDType.GetDisplayName(),
                pro.HDCode,
                pro.CreateIPAddress,
                pro.AreaID,
                pro.Area,
                pro.LastLogonIPAddress,
                LastLogonTime = pro.LastLogonTime.ToString("yyyy-MM-dd HH:mm:ss"),
                UserType = pro.UserType.GetDisplayName(),
                Amount = pro.Amount.ToCash()

            });

            return Json(new
            {
                data = _newApi,
                recordsTotal = _api.TotalCount,
                recordsFiltered = _api.TotalCount,
                success = true,
                draw = draw,

            });
        }

        /// <summary>
        /// 匯出註冊用戶查詢Excel
        /// </summary>
        /// <param name="platformid"></param>
        /// <param name="areaID"></param>
        /// <param name="adminId"></param>
        /// <param name="regBeginTime"></param>
        /// <param name="userID"></param>
        /// <param name="regEndTime"></param>
        /// <param name="regIpAddress"></param>
        /// <param name="hdType"></param>
        /// <param name="pageIndex"></param>
        /// <param name="sortName"></param>
        /// <param name="sortLifting"></param>
        /// <returns></returns>
        private ActionResult exportRegUser(int? platformid, int? areaID, int? adminId, DateTime regBeginTime, int? userID = null, DateTime? regEndTime = null,
          string regIpAddress = null, HDType? hdType = null, int pageIndex = 1, SortName sortName = SortName.StatTime, SortLifting sortLifting = SortLifting.DESC)
        {
            var _datas = SiteHelper.GetAPIResult<PagedList<RegUserInfo>>(string.Format("/User/GetRegUser?platformid={0}&areaID={1}&adminId={2}&userID={3}&regBeginTime={4}&regEndTime={5}&regIpAddress={6}&hdType={7}&pageIndex={8}&pageSize={9}&sortName={10}&sortLifting={11}"
                , platformid, areaID, adminId, userID, regBeginTime, regEndTime, regIpAddress, hdType, 1, 10000, sortName, sortLifting));

            var _path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "ExportTemplat", "Userstat_Reguser.xlsx");

            using (FileStream file = new FileStream(_path, FileMode.Open, FileAccess.Read))
            {
                #region 畫Excel

                var _workbook = new XSSFWorkbook(file);
                var _sheet = _workbook.GetSheetAt(0);
                file.Close();

                _sheet.GetRow(1).GetCell(11).SetCellValue(string.Format("{0}~{1}", regBeginTime.ToString("yyyy-MM-dd HH:mm:ss"), regEndTime));

                if (_datas.TotalCount != 0)
                {
                    for (var i = 0; i < _datas.PageData.Count; i++)
                    {
                        RegUserInfo _regUser = _datas.PageData[i];
                        var _row = _sheet.CreateRow(3 + i);
                        _row.CreateCell(0).SetCellValue(_regUser.Area.AreaName);
                        _row.CreateCell(1).SetCellValue(_regUser.Id);
                        _row.CreateCell(2).SetCellValue(_regUser.LoginName);
                        _row.CreateCell(3).SetCellValue(_regUser.GradeID);
                        _row.CreateCell(4).SetCellValue(_regUser.SumCharge.ToCash());
                        _row.CreateCell(5).SetCellValue(_regUser.cashamount.ToCash());
                        _row.CreateCell(6).SetCellValue(_regUser.Sex.GetDisplayName());
                        _row.CreateCell(7).SetCellValue(_regUser.HDType.GetDisplayName());
                        _row.CreateCell(8).SetCellValue(_regUser.HDCode);
                        _row.CreateCell(9).SetCellValue(_regUser.UserType.GetDisplayName());
                        _row.CreateCell(10).SetCellValue(_regUser.CreateTime.ToString("yyyy-MM-dd HH:mm:ss"));
                        _row.CreateCell(11).SetCellValue(_regUser.CreateIPAddress);
                        _row.CreateCell(12).SetCellValue(_regUser.Location);
                        _row.CreateCell(13).SetCellValue(_regUser.LastLogonTime.ToString("yyyy-MM-dd HH:mm:ss"));
                        _row.CreateCell(14).SetCellValue(_regUser.LastLogonIPAddress);

                    }
                }

                #endregion

                using (var exportData = new MemoryStream())
                {
                    _workbook.Write(exportData);
                    string saveAsFileName = string.Format("注册用户查询_{0:d}.xlsx", DateTime.Now).Replace("/", "-");

                    byte[] bytes = exportData.ToArray();
                    return File(bytes, "application/vnd.ms-excel", saveAsFileName);
                }
            }
        }
    }
}