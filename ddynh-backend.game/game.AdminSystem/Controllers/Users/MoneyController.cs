using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FW.Core.AdminSys.WebUI.Themes.Bootstrap;
using FW.Core.Helper;
using FW.Core.Helper.Enum;
using game.Entity;
using game.Entity.Money;
using game.Entity.Payment;
using game.Infrastructure;
using Newtonsoft.Json.Linq;
using game.Infrastructure.Extensions;
using game.Entity.Customer;

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


        public ActionResult MoneyLog(int? SourceType = null, DateTime? beginDate = null , DateTime? endDate = null)
        {
            DateTime? defaultDateInput = DateTime.Now.AddDays(-1);
            ViewBag.defaultSourceType = SourceType;
            ViewBag.defaultBeginTime = (beginDate ?? defaultDateInput).ToDateTimeMinuteStringOrEmpty();
            ViewBag.defaultEndTime = endDate.ToDateTimeMinuteStringOrEmpty();
            return View();
        }

        /// <summary>
        /// 3-现金记录查询
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetMoneyLog(int? draw, int? start, int length, int? userID = null, string loginName = "", string nickName = "", int? sourceTypeGroup = null, int? sourceType = null, string ipAddress = "", DateTime? beginTime = null, DateTime? endTime = null)
        {
            var nullJson = Json(new
            {
                draw = draw,
                recordsTotal = 0,
                recordsFiltered = 0,
                success = true,
                data = new { }
            });

            var currentAdminID = GetCurrentUserID();

            //若篩選條件 有填寫userID 或 userName
            if (userID.HasValue || !string.IsNullOrWhiteSpace(loginName) || !string.IsNullOrWhiteSpace(nickName))
            {
                var queryUser = UserService.GetUser(userID, loginName, nickName);

                //若查無資料 直接retunr
                if (queryUser == null)
                {
                    return nullJson;
                }

                userID = queryUser.Id;
            }


            if (beginTime == null)
                beginTime = DateTime.Now.AddDays(-1);

            var pageIndex = Math.Ceiling((decimal)start.Value / length) + 1;

            string apiUrl = string.Format("/Money/GetMoneyLog?userID={0}&sourceType={1}&ipAddress={2}&beginTime={3}&endTime={4}&pageIndex={5}&pageSize={6}&sourceTypeGroup={7}&AdminID={8}",
                 userID, sourceType, ipAddress, beginTime, endTime, pageIndex, length, sourceTypeGroup , currentAdminID);

            var api = SiteHelper.GetAPIResult<MoenyLogAddSumFlowAmountReport>(apiUrl).PageListMoneyLog;

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
        public ActionResult BankLog(int? SourceType = null, DateTime? beginDate = null, DateTime? endDate = null)
        {
            DateTime? defaultDateInput = DateTime.Now.AddDays(-1);
            ViewBag.defaultSourceType = SourceType;
            ViewBag.defaultBeginTime = (beginDate ?? defaultDateInput).ToDateTimeMinuteStringOrEmpty();
            ViewBag.defaultEndTime = endDate.ToDateTimeMinuteStringOrEmpty();

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
        public ActionResult UpdateUserCash(int userID, [ModelBinder(typeof(MoneyModeBinder))]Int64 amount, float auditAmount, int auditType, string memo, string txtScretKey)
        {
            var operatorID = CurrentUser.UserID;
            var ipAddress = Server.UrlEncode(game.Infrastructure.SystemHelper.UserIPAddress);

            #region 驗證金鑰
            /*
            var _verifySecretKeyResult = SiteService.VerifyAdminUserSecretKey(txtScretKey);
            if (!_verifySecretKeyResult)
            {
                return Alert("金钥不正确");
            }
            */
            #endregion
            var Url = string.Empty;

            //增加線下支付
            if (auditType == (int)UpdateCashAuditTypeEnum.ManualCashIn_IncludeCharge)
            {
                //生成訂單
                var amount1 = amount / 100;
                var result = SiteHelper.GetAPIResult<APIResult>($"/Pay/GetCreateOrder?userID={userID}&amount={amount1}&memo={Server.UrlEncode(memo)}&ipAddress={ipAddress}");
                var orderid = result.Extend;
                //加上GWID
                SiteHelper.GetAPIResult<APIResult>($"/Pay/CDNewModifyTransID?orderID={orderid}&transID={orderid}&gwid={28}");
                //手動上分
                Url = $"/Pay/CDFinishAEOrder?orderID={orderid}&amount={amount1}&ipAddress={ipAddress}&transID={orderid}&transData=''";
            }
            else
            {
                Url = string.Format("/Money/UpdateUserCash?userID={0}&amount={1}&sourceType={2}&ipAddress={3}&memo={4}",
                    userID,
                    amount,
                    amount > 0 ? (auditType == (int)UpdateCashAuditTypeEnum.EventBonus ? SourceType_Cash.EventIn : SourceType_Cash.AdminIn) : (auditType == 3 ? SourceType_Cash.EventOut : SourceType_Cash.AdminOut),  //增加當選擇活動時, 改SourceTypeID為 活动收费扣金币/活动赠送奖励金币
                    ipAddress,
                    Server.UrlEncode(memo));
            }
            var api = SiteHelper.GetAPIResult<APIResult>(Url);

            if (api.Result == true)
            {
                /*計算流水, 並寫入資料表 CenterDB.Event.FlowAmountLimit*/
                //if (auditAmount > 0)
                //{
                var url = string.Format("/Money/CreateFlowAmountLimitLog?adminID={0}&userID={1}&auditType={2}&auditAmount={3}&sourceType={4}&amount={5}",
                operatorID,
                userID,
                auditType,
                auditAmount * 100,
                amount > 0 ? (auditType == 3 ? SourceType_Cash.EventIn : SourceType_Cash.AdminIn) : (auditType == 3 ? SourceType_Cash.EventOut : SourceType_Cash.AdminOut),  //增加當選擇活動時, 改SourceTypeID為 活动收费扣金币/活动赠送奖励金币
                amount);
                var flowLimitResult = SiteHelper.GetAPIResult<APIResult>(url);
                //}
            }

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
        public ActionResult UpdateUserBank(int userID, [ModelBinder(typeof(MoneyModeBinder))]long amount, string memo, string txtScretKey , int auditType , long auditAmount)
        {
            #region 驗證金鑰
			/*
            var _verifySecretKeyResult = SiteService.VerifyAdminUserSecretKey(txtScretKey);
            if (!_verifySecretKeyResult)
            {
                return Alert("金钥不正确");
            }
			*/
            #endregion

            var operatorID = CurrentUser.UserID;            

            string ipAddress = Server.UrlEncode(game.Infrastructure.SystemHelper.UserIPAddress);

            //增加線下支付
            int sourceType = amount > 0 ? SourceType_Bank.AdminIn : SourceType_Bank.AdminOut;

            if(auditType == (int)UpdateCashAuditTypeEnum.EventBonus)
                sourceType = amount > 0 ? SourceType_Cash.EventIn : SourceType_Cash.EventOut;

            var api = SiteHelper.GetAPIResult<APIResult>(string.Format("/Money/UpdateUserBank?adminID={0}&userID={1}&amount={2}&sourceType={3}&ipAddress={4}&memo={5}",
                operatorID,
                userID,
                amount,
                sourceType,
                Server.UrlEncode(game.Infrastructure.SystemHelper.UserIPAddress),
                Server.UrlEncode(memo)));


            if (api.Result == true)
            {               
                string url = string.Format("/Money/CreateFlowAmountLimitLog?adminID={0}&userID={1}&auditType={2}&auditAmount={3}&sourceType={4}&amount={5}",
                    operatorID,
                    userID,
                    auditType,
                    auditAmount * 100,
                    sourceType, 
                    amount
                );

                //產生一筆稽核流水
                var flowLimitResult = SiteHelper.GetAPIResult<APIResult>(url);                
            }

            return Alert(api.Message, "/Money/UpdateUserBank?userID=" + userID);
        }

        #region 兑换记录

        public ActionResult GetConvertLogCount(ConvertLogStatus status = ConvertLogStatus.Exchange)
        {
            if (SiteHelper.GetPlatformIDs().Count > 0)
            {
                string url = $"/Money/GetConvertLogCountByStatus?status={status}&platformIDs={string.Join(",", SiteHelper.GetPlatformIDs())}";
                var waitAuditingCount = SiteHelper.GetAPIResult<int>(url);

                var resultData = new
                {
                    WaitAuditingCount = waitAuditingCount,
                    IsSoundNotify = CustomHtmlHelper.CheckFunctionPermissionExist("Money" , "ConvertNotifySound")
                };

                return Json(new AjaxResult() { success = true, data = resultData }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return HttpNotFound();
            }
        }


        public ActionResult ConvertLog(ConvertLogStatus? status = null)
        {
            var operatorID = Convert.ToInt32(CurrentUser.UserID);
            ViewBag.operatorID = operatorID;
            ViewBag.Status = status.ToString().ToLower();
            return View();
        }

        [HttpPost]
        public ActionResult GetConvertLog(int? draw, int? start, int length, int? platformIds = null, int? userID = null, string loginName = "", string nickName = "", ConvertLogStatus? status = null, int? convertType = null, string realName = null, string ipAddress = null, DateTime? beginTime = null, DateTime? endTime = null, string operatorName = null, int? gwid = null, decimal? minAmount = null, decimal? maxAmount = null)
        {

            var pageIndex = Math.Ceiling((decimal)start.Value / length) + 1;

            if (beginTime == null) beginTime = DateTime.Now.AddHours(-24);

            userID = UserService.GetUserID(userID, loginName, nickName);
            var adminId = SiteService.GetAdminUserID();

            var url = $"/Money/GetConvertLog?userID={userID}&status={status}&convertType={convertType}&realName={realName}&ipAddress={ipAddress}&beginTime={beginTime}&endTime={endTime}&pageIndex={pageIndex}" +
                        $"&pageSize={length}&platformId={platformIds}&adminId={adminId}&operatorName={operatorName}&gwid={gwid}&minAmount={minAmount}&maxAmount={maxAmount}";
            var api = SiteHelper.GetAPIResult<PagedList<ConvertLog>>(url);

            var PageData = api.PageData.Where(x => !x.Id.Equals(0));
            var total = api.PageData.Where(x => x.Id.Equals(0)).ToList();

            var query = from pro in PageData
                        select new
                        {
                            ID = pro.Id,
                            UserID = pro.User.Id,
                            Amount = pro.Amount / 100,
                            CashAmount = (pro.CashAmount + pro.Amount - pro.TaxAmount).ToCash(),
                            TaxAmount = Math.Floor(double.Parse(pro.TaxAmount.ToCash())),
                            BankAmount = Math.Floor(double.Parse(pro.BankAmount.ToCash())),
                            RealName = pro.RealName,
                            Account = pro.Account,
                            BankCodeID = pro.BankCodeID,
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

        [NoAuthorization]
        public ActionResult GetConvertGateway(int convertType, int userId, int? bankCodeID = null)
        {
            var loginName = CurrentUser.LoginName;

            var url = $"/Payment/GetConvertGateway?operatorID={SiteService.GetAdminUserID()}&convertType={convertType}&userId={userId}&bankCodeID={bankCodeID}";
            var ret = SiteHelper.GetAPIResult<List<ConvertGateway>>(url);

            return Json(ret, JsonRequestBehavior.AllowGet);
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
                var ret = SiteHelper.GetAPIResult<APIResult>($"/Money/ModifyConvertLogStatusPassed?id={s}&status={ConvertLogStatus.ConvertSuccessed}&updateUser={loginName}&memo={memo}&gwid={gwid}");
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
                var ret = SiteHelper.GetAPIResult<APIResult>($"/Money/ModifyConvertLogStatusRefused?id={s}&status={status}&updateUser={loginName}&memo={memo}");
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
                var ret = SiteHelper.GetAPIResult<APIResult>($"/Money/ModifyConvertLogStatusMemo?id={loginName}&updateUser={s}&memo={memo}");
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