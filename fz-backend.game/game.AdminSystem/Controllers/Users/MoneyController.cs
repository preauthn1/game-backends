using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FW.Core.Helper;
using FW.Core.Helper.Enum;
using game.Entity;
using game.Entity.Money;
using game.Infrastructure;
using System.Configuration;
using game.Entity.AdminSys;

namespace game.AdminSystem.Controllers.Users
{
    /// <summary>
    /// 用户管理-金币查询
    /// </summary>
    public class MoneyController : BaseController
    {
        /// <summary>
        /// 3-金币查询
        /// </summary>
        /// <returns></returns>
        public ActionResult Search(int? userID = null, string loginName = "", string nickName = "")
        {
            if (userID == null && loginName.Length == 0 && nickName.Length == 0) return View();

            var user = UserService.GetUser(userID, loginName, nickName);
            if (user == null) return View();

            var operatorID = CurrentUser.UserID;
            var isPermission = SiteHelper.GetAPIResult<APIResult>($"/User/GetOperatorPermission?operatorID={operatorID}&userID={user.Id}");
            if (!isPermission.Result) return View();

            ViewBag.User = user;
            ViewBag.UserCash = null;
            ViewBag.UserChargeSum = null;

            ViewBag.UserCash = SiteHelper.GetAPIResult<Cash>(string.Format("/Money/GetUserCash?userID={0}", user.Id));
            ViewBag.UserBank = SiteHelper.GetAPIResult<Bank>(string.Format("/Money/GetUserBank?userID={0}", user.Id));
            return View();
        }


        public ActionResult MoneyLog()
        {
            return View();
        }

        /// <summary>
        /// 3-现金记录查询
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetMoneyLog(int? draw, int? start, int length, int? userID = null, string loginName = "", string nickName = "", int? sourceTypeGroup = null, int? sourceType = null, string ipAddress = "", DateTime? beginTime = null, DateTime? endTime = null)
        {
            userID = UserService.GetUserID(userID, loginName, nickName);

            if (userID == null && sourceTypeGroup == null && sourceType == null && string.IsNullOrEmpty(ipAddress) && beginTime == null && endTime == null)
            {
                return Json(new
                {
                    draw = draw,
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    success = true,
                    data = new { }
                });
            }

            if (beginTime == null) beginTime = DateTime.Now.AddDays(-1);

            var pageIndex = Math.Ceiling((decimal)start.Value / length) + 1;
            var api = SiteHelper.GetAPIResult<PagedList<MoneyLog>>(string.Format("/Money/GetMoneyLog?userID={0}&sourceType={1}&ipAddress={2}&beginTime={3}&endTime={4}&pageIndex={5}&pageSize={6}&sourceTypeGroup={7}",
    userID, sourceType, ipAddress, beginTime, endTime, pageIndex, length, sourceTypeGroup));


            var query = from pro in api.PageData
                        select new
                        {
                            CreateTime = pro.CreateTime.ToString("yyyy/MM/dd HH:mm"),
                            PlatformName = pro.User.Area.Platform.Name,
                            UserID = pro.User.Id,
                            LoginName = pro.User.LoginName,
                            GradeID = pro.User.GradeID,
                            SourceName = pro.SourceType.SourceName,
                            CurrentAmount = pro.CurrentAmount.ToCash(),
                            UpdateAmount = pro.UpdateAmount.ToCash(),
                            TotalAmount = (pro.CurrentAmount + pro.UpdateAmount).ToCash(),
                            IPAddress = pro.IPAddress,
                            Memo = pro.Memo,
                            TradeID = pro.TradeID
                        };

            return Json(new
            {
                draw = draw,
                recordsTotal = api.TotalCount,
                recordsFiltered = api.TotalCount,
                success = true,
                data = query
            });
        }

        [HttpPost]
        public ActionResult GetPerGameLogByTradeID(string tradeID)
        {
            var list = SiteHelper.GetAPIResult<List<string>>(string.Format("/PerGameLog/GetPerGameLogByTradeID?tradeID={0}", tradeID));

            return Json(new
            {
                success = true,
                result = list
            });
        }

        /// <summary>
        /// 银行记录查询
        /// </summary>
        /// <returns></returns>
        public ActionResult BankLog()
        {
            return View();
        }


        /// <summary>
        /// 银行记录查询
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetBankLog(int? draw, int? start, int length, int? userID = null, string loginName = "", string nickName = "", int? sourceType = null, string ipAddress = "", DateTime? beginTime = null, DateTime? endTime = null)
        {
            var pageIndex = Math.Ceiling((decimal)start.Value / length) + 1;

            userID = UserService.GetUserID(userID, loginName, nickName);
            if (userID == null && sourceType == null && string.IsNullOrEmpty(ipAddress) && beginTime == null && endTime == null)
            {
                return Json(new
                {
                    draw = draw,
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    success = true,
                    data = new { }
                });
            }

            if (beginTime == null) beginTime = DateTime.Now.AddDays(-1);

            var operatorID = CurrentUser.UserID;
            var api = SiteHelper.GetAPIResult<PagedList<BankLog>>($"/Money/GetBankLog?operatorID={operatorID}&userID={userID}&sourceType={sourceType}&ipAddress={ipAddress}&beginTime={beginTime}"
                + $"&endTime={endTime}&pageSize={length}&pageIndex={pageIndex}");

            var query = from pro in api.PageData
                        select new
                        {
                            Platform = pro.User.Area.Platform.Name,
                            UserID = pro.UserID,
                            LoginName = pro.User.LoginName,
                            GradeID = pro.User.GradeID,
                            CreateTime = pro.CreateTime.ToString("yyyy/MM/dd hh:mm"),
                            SourceName = pro.SourceType.SourceName,
                            CurrentAmount = pro.CurrentAmount.ToCash(),
                            UpdateAmount = pro.UpdateAmount.ToCash(),
                            TotalAmount = (pro.CurrentAmount + pro.UpdateAmount).ToCash(),
                            IPAddress = pro.IPAddress,
                            Memo = pro.Memo
                        };

            return Json(new
            {
                draw = draw,
                recordsTotal = api.TotalCount,
                recordsFiltered = api.TotalCount,
                success = true,
                data = query
            });
        }

        /// <summary>
        /// 3-修改现金
        /// </summary>
        /// <returns></returns>
        public ActionResult UpdateUserCash(int? userID = null, string loginName = "", string nickName = "")
        {
            if (userID == null && loginName.Length == 0 && nickName.Length == 0) return View();

            var user = UserService.GetUser(userID, loginName, nickName);
            if (user == null) return View();

            var operatorID = CurrentUser.UserID;
            var isPermission = SiteHelper.GetAPIResult<APIResult>($"/User/GetOperatorPermission?operatorID={operatorID}&userID={user.Id}");
            if (!isPermission.Result) return View();

            ViewBag.Model = user;
            return View(SiteHelper.GetAPIResult<Cash>(string.Format("/Money/GetUserCash?userID={0}", user.Id)));
        }

        /// <summary>
        /// 4-管理员增加/减少现金
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="amount"></param>
        /// <param name="memo"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UpdateUserCash(int userID, [ModelBinder(typeof(MoneyModeBinder))]Int64 amount, string memo, string txtScretKey)
        {
            #region 驗證金鑰
            
            var roles = SiteHelper.GetAPIResult<List<UserRole>>($"/AdminSys/GetUserRole?userID={CurrentUser.UserID}").Select(x => x.RoleID.ToString()).ToList();
            var ignoreRoles = ConfigurationManager.AppSettings["IgnoreRolesVerification"].ToString().Split(',').ToList();
            var _verifySecretKeyResult = SiteService.VerifyAdminUserSecretKey(txtScretKey);

            var isVerify = roles.Any(x => ignoreRoles.Exists(i => i == x));
            if (!_verifySecretKeyResult && !isVerify)
            {
                return Alert("金钥不正确");
            }

            #endregion

            var api = SiteHelper.GetAPIResult<APIResult>(string.Format("/Money/UpdateUserCash?userID={0}&amount={1}&sourceType={2}&ipAddress={3}&memo={4}",
                userID,
                amount,
                amount > 0 ? SourceType_Cash.AdminIn : SourceType_Cash.AdminOut,
                Server.UrlEncode(game.Infrastructure.SystemHelper.UserIPAddress),
                Server.UrlEncode(memo)));

            return Alert(api.Message, "/Money/UpdateUserCash?userID=" + userID);

        }


        /// <summary>
        /// 3-修改银行
        /// </summary>
        /// <returns></returns>        
        public ActionResult UpdateUserBank(int? userID = null, string loginName = "", string nickName = "")
        {
            if (userID == null && loginName.Length == 0 && nickName.Length == 0) return View();

            var user = UserService.GetUser(userID, loginName, nickName);
            if (user == null) return View();

            var operatorID = CurrentUser.UserID;
            var isPermission = SiteHelper.GetAPIResult<APIResult>($"/User/GetOperatorPermission?operatorID={operatorID}&userID={user.Id}");
            if (!isPermission.Result) return View();

            ViewBag.Model = user;
            return View(SiteHelper.GetAPIResult<Bank>(string.Format("/Money/GetUserBank?userID={0}", user.Id)));
        }

        /// <summary>
        /// 4-管理员增加/减少银行
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="amount"></param>
        /// <param name="memo"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UpdateUserBank(int userID, [ModelBinder(typeof(MoneyModeBinder))]Int64 amount, string memo, string txtScretKey)
        {

            #region 驗證金鑰
            var _verifySecretKeyResult = SiteService.VerifyAdminUserSecretKey(txtScretKey);
            if (!_verifySecretKeyResult)
            {
                return Alert("金钥不正确");
            }

            #endregion

            var operatorID = CurrentUser.UserID;

            var api = SiteHelper.GetAPIResult<APIResult>(string.Format("/Money/UpdateUserBank?adminID={0}&userID={1}&amount={2}&sourceType={3}&ipAddress={4}&memo={5}",
                operatorID,
                userID,
                amount,
                amount > 0 ? SourceType_Bank.AdminIn : SourceType_Bank.AdminOut,
                Server.UrlEncode(game.Infrastructure.SystemHelper.UserIPAddress),
                Server.UrlEncode(memo)));

            return Alert(api.Message, "/Money/UpdateUserBank?userID=" + userID);
        }

        #region 兑换记录

        public ActionResult GetConvertLogCount(ConvertLogStatus status = ConvertLogStatus.Exchange)
        {
            if (SiteHelper.GetPlatformIDs().Count > 0)
            {
                var waitAuditingCount = SiteHelper.GetAPIResult<int>($"/Money/GetConvertLogCountByStatus?status={status}&platformIDs={string.Join(",", SiteHelper.GetPlatformIDs())}");
                return Json(new AjaxResult() { success = true, data = new { WaitAuditingCount = waitAuditingCount } }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return HttpNotFound();
            }
        }


        public ActionResult ConvertLog(ConvertLogStatus? status = null)
        {
            ViewBag.Status = status.ToString().ToLower();
            return View();
        }

        [HttpPost]
        public ActionResult GetConvertLog(int? draw, int? start, int length, int? platformIds = null, int? userID = null, string loginName = "", string nickName = "", ConvertLogStatus? status = null, int? convertType = null, string realName = null, string ipAddress = null, DateTime? beginTime = null, DateTime? endTime = null, string operatorName = null, int? gwid = null, long? minAmount = null, long? maxAmount = null)
        {

            var pageIndex = Math.Ceiling((decimal)start.Value / length) + 1;

            if (beginTime == null) beginTime = DateTime.Now.AddHours(-24);

            userID = UserService.GetUserID(userID, loginName, nickName);
            var adminId = SiteService.GetAdminUserID();

            var api = SiteHelper.GetAPIResult<PagedList<ConvertLog>>(string.Format("/Money/GetConvertLog?userID={0}&status={1}&convertType={2}&realName={3}&ipAddress={4}&beginTime={5}&endTime={6}&pageIndex={7}&pageSize={8}&platformId={9}&adminId={10}&operatorName={11}&gwid={12}&minAmount={13}&maxAmount={14}",
                    userID, status, convertType, realName, ipAddress, beginTime, endTime, pageIndex, length, platformIds, adminId, operatorName, gwid, minAmount, maxAmount));


            var PageData = api.PageData.Where(x => !x.Id.Equals(0));
            var total = api.PageData.Where(x => x.Id.Equals(0)).ToList();

            var query = from pro in PageData
                        select new
                        {
                            ID = pro.Id,
                            UserID = pro.User.Id,
                            Amount = pro.Amount / 100,
                            CashAmount = pro.CashAmount.ToCash(),
                            TaxAmount = Math.Floor(double.Parse(pro.TaxAmount.ToCash())),
                            BankAmount = Math.Floor(double.Parse(pro.BankAmount.ToCash())),
                            RealName = pro.RealName,
                            Account = pro.Account,
                            BankName = pro.BankName,
                            TotalConvert = Math.Floor(double.Parse(pro.TotalConvert.ToCash())),
                            TotalConvertIOS = Math.Floor(double.Parse(pro.TotalConvertIOS.ToCash())),
                            TotalConvertAndroid = Math.Floor(double.Parse(pro.TotalConvertAndroid.ToCash())),
                            TotalCharge = Math.Floor(double.Parse(pro.TotalCharge.ToCash())),
                            ChargeConvertRate = Math.Floor(double.Parse(pro.ChargeConvertRate.ToCash())),
                            CreateTime = pro.CreateTime.ToString("yyyy/MM/dd HH:mm"),
                            LastUpdateTime = pro.LastUpdateTime.ToString("yyyy/MM/dd HH:mm"),
                            Status = pro.Status,
                            StatusName = pro.Status.GetDisplayName(),
                            Operator = pro.Operator,
                            pro.ClubAgent,
                            pro.Suspicious,
                            pro.Memo,
                            pro.GWID,
                            pro.GatewayName,
                            pro.ConvertType
                        };

            return Json(new
            {
                draw = draw,
                recordsTotal = api.TotalCount,
                recordsFiltered = api.TotalCount,
                success = true,
                data = query,
                TransData = total[0].TransData
            });
        }

        /// <summary>
        /// 審核通過
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult ModifyConvertLogStatusPassed(string memo, int? gwid, params int[] id)
        {
            string message = "";
            var loginName = CurrentUser.LoginName;

            Array.ForEach(id, s =>
            {
                var ret = SiteHelper.GetAPIResult<APIResult>(string.Format("/Money/ModifyConvertLogStatusPassed?id={0}&status={1}&updateUser={2}&memo={3}&gwid={4}", s, ConvertLogStatus.ConvertSuccessed, loginName, memo, gwid));
                if (!ret.Result)
                    message += string.Format("id {0}: {1} \\r\\n", s, ret.Message);
            });

            var data = new
            {
                status = true,
                msg = string.IsNullOrEmpty(message) ? "操作成功" : message
            };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 審核拒絕
        /// </summary>
        /// <param name="status">拒絕理由</param>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult ModifyConvertLogStatusRefused(string status, string memo, params int[] id)
        {
            string message = "";
            var loginName = this.CurrentUser.LoginName;

            Array.ForEach(id, s =>
            {
                var ret = SiteHelper.GetAPIResult<APIResult>(string.Format("/Money/ModifyConvertLogStatusRefused?id={0}&status={1}&updateUser={2}&memo={3}", s, status, loginName, memo));
                if (!ret.Result)
                    message += string.Format("id {0}: {1} \\r\\n", s, ret.Message);
            });

            var data = new
            {
                status = true,
                msg = string.IsNullOrEmpty(message) ? "操作成功" : message
            };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 修改兑换备注
        /// </summary>        
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult ModifyConvertLogStatusMemo(string memo, params int[] id)
        {
            string message = "";
            var loginName = this.CurrentUser.LoginName;

            Array.ForEach(id, s =>
            {
                var ret = SiteHelper.GetAPIResult<APIResult>(string.Format("/Money/ModifyConvertLogStatusMemo?id={0}&updateUser={1}&memo={2}", s, loginName, memo));
                if (!ret.Result)
                    message += string.Format("id {0}: {1} \\r\\n", s, ret.Message);
            });

            var data = new
            {
                status = true,
                msg = string.IsNullOrEmpty(message) ? "操作成功" : message
            };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult FinishedConvertLogSuccessed(int id)
        {
            var loginName = this.CurrentUser.LoginName;
            var result = SiteHelper.GetAPIResult<APIResult2>(string.Format("/Money/FinishedConvertLog?id={0}&status={1}&realTaxAmount={2}&transId={3}&transData={4}&updateUser={5}", id, (byte)ConvertLogStatus.AlreadyPaid, 0, 0, "打款完成", loginName));

            return Json(result);
        }

        #endregion

        /// <summary>
        /// 可疑帳戶
        /// </summary>
        /// <returns></returns>
        public ActionResult SuspiciousAccount()
        {
            return View();
        }

        /// <summary>
        /// 取得可疑帳戶列表
        /// </summary>
        /// <returns></returns>
        [NoAuthorization]
        public ActionResult GetSuspicious(string name, string accountID)
        {
            var _list = SiteHelper.GetAPIResult<List<ConvertSuspicious>>(string.Format("/Money/GetSuspicious?name={0}&accountID={1}", name, accountID));
            return Json(_list, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 新增可疑帳戶
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddSuspicious(ConvertSuspicious model)
        {
            var _result = SiteHelper.PostAPIResult<ConvertSuspicious>("/Money/AddSuspicious", model);
            return Alert(_result.Message);
        }

        /// <summary>
        /// 修改可疑帳戶
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EditSuspicious(ConvertSuspicious model)
        {
            var _result = SiteHelper.GetAPIResult<APIResult>(string.Format("/Money/EditSuspicious?id={0}&name={1}&accountID={2}&action={3}", model.Id, model.Name, model.AccountID, "upd"));
            return Alert(_result.Message);
        }

        /// <summary>
        /// 刪除可疑帳戶
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteSuspicious(int Id)
        {
            var _result = SiteHelper.GetAPIResult<APIResult>(string.Format("/Money/EditSuspicious?id={0}&action={1}", Id, "del"));
            return Json(new { status = _result.Result, msg = _result.Message });
        }

        private void SetSession(string key, object value, int? timeout)
        {
            int time = timeout ?? Session.Timeout;
            DateTime endTime = DateTime.Now + new TimeSpan(0, time, 0);
            var tuple = Tuple.Create(key, value, endTime);
            Session[key] = tuple;
        }


        private object GetSingleSession(string key)
        {
            if (Session[key] == null) { return null; }
            var tuple = Session[key] as Tuple<string, object, DateTime>;
            var diff = DateTime.Compare(tuple.Item3, DateTime.Now);
            object result = null;
            if (diff > 0) result = tuple.Item2;//有效時間截止之前正常返回值
            else Session.Remove(key);//超過有效時間清空Session
            return result;
        }
    }
}