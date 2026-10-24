using System;
using System.Collections.Generic;
using example.invalid;
using System.Linq;
using System.Web.Mvc;
using DataTables.Mvc;
using FW.Core.Helper;
using FW.Core.Helper.Enum;
using game.Entity;
using game.Entity.Game;
using game.Entity.Money;
using game.Entity.Pay;
using game.Entity.PerLog;
using game.Entity.Users;
using game.Infrastructure;
using Newtonsoft.Json;
using NPOI.XSSF.UserModel;

namespace game.AdminSystem.Controllers.Users
{
    /// <summary>
    /// 用户管理-基本资料查询
    /// </summary>
    public class UserInfoController : BaseController
    {
        /// <summary>
        /// 3-用户查询
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public ActionResult Search(int? userID = null, string loginName = "", string nickName = "", DateTime? beginTime = null, DateTime? endTime = null, string openTag = null)
        {
            ViewBag.BeginTime = beginTime;
            ViewBag.EndTime = endTime;
            ViewBag.OpenTag = openTag;




            if (userID != null)
            {
                Session["userID"] = userID;
            }

            if (Session["userID"] != null)
            {
                ViewBag.userID = int.Parse(Session["userID"].ToString());
            }

            if (userID == null && loginName.Length == 0 && nickName.Length == 0 && Session["userID"] == null)
            {
                return View();
            }
            else
            {
                if (userID == null && loginName.Length == 0 && nickName.Length == 0 && Session["userID"] != null)
                {
                    userID = int.Parse(Session["userID"].ToString());
                }
            }

            userID = UserService.GetUserID(userID, loginName, nickName);
            if (userID == null) return View();

            var operatorID = CurrentUser.UserID;
            var isPermission = SiteHelper.GetAPIResult<APIResult>($"/User/GetOperatorPermission?operatorID={operatorID}&userID={userID}");
            if (!isPermission.Result) return View();

            var res = SiteHelper.GetAPIResult<string>($"/User/GetContestUserTicketSummary?userID={userID}");
            var tickets = JsonConvert.DeserializeObject<List<UserTicketSummary>>(res);
            ViewBag.Tickets = tickets;

            return View(SiteHelper.GetAPIResult<UserInfoDetail>(string.Format("/User/GetUserInfoDetail?userID={0}&beginTime={1}&endTime={2}", userID, beginTime, endTime)));
        }

        /// <summary>
        /// 4-重置密码
        /// </summary>
        public ActionResult ResetPassword(int userID)
        {
            var user = UserService.GetUserByID(userID);
            APIResult result = SiteHelper.GetAPIResult<APIResult>(string.Format("/User/ModifyUserPassword?userID={0}&newpwd=[REDACTED]", user.Id, "111111", Server.UrlEncode(game.Infrastructure.SystemHelper.UserIPAddress)));

            if (result.Result)
            {
                return Alert("密码重置为:111111");
            }
            else
            {
                return Alert("操作失败");
            }
        }

        /// <summary>
        /// 重置保险箱密码
        /// </summary>
        public ActionResult ResetBankPwd(int userID)
        {
            var user = UserService.GetUserByID(userID);
            APIResult result = SiteHelper.GetAPIResult<APIResult>(string.Format("/User/ModifyUserBankPassword?userID={0}&newpwd=[REDACTED]", user.Id, "888888", Server.UrlEncode(game.Infrastructure.SystemHelper.UserIPAddress)));

            if (result.Result)
            {
                return Alert("密码重置为:888888");
            }
            else
            {
                return Alert("操作失败");
            }
        }

        /// <summary>
        /// 用户资料修改记录
        /// </summary>
        public ActionResult UserInfoChangeLog()
        {
            return View();
        }

        /// <summary>
        /// 用户资料修改记录
        /// </summary>
        public ActionResult GetUserInfoChangeLog([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, int? userID = null, string loginName = "", string nickName = "", UserInfoChangeType? type = null, string ipAddress = null, DateTime? beginTime = null, DateTime? endTime = null)
        {
            var draw = requestModel.Draw;
            var start = requestModel.Start;
            var length = requestModel.Length;
            var pageIndex = Math.Ceiling((decimal)start / length) + 1;

            userID = UserService.GetUserID(userID, loginName, nickName);
            if (beginTime == null) beginTime = Convert.ToDateTime(DateTime.Now.ToShortDateString());

            var operatorID = CurrentUser.UserID;
            var api = SiteHelper.GetAPIResult<PagedList<UserInfoChangeLog>>($"/User/GetUserInfoChangeLog?operatorID={operatorID}&userID={userID}&type={type}&ipAddress={ipAddress}&createTimeBegin={beginTime}"
                + $"&createTimeEnd={endTime}&pageSize={length}&pageIndex={pageIndex}");

            var query = from pro in api.PageData
                        select new
                        {
                            WriteTime = pro.WriteTime.ToString("yyyy-MM-dd HH:mm"),
                            Platform = pro.User.Area.Platform.Name,
                            UserID = pro.UserID,
                            LoginName = pro.User.LoginName,
                            GradeID = pro.User.GradeID,
                            ChangeType = pro.ChangeType.GetDisplayName(),
                            OldData = pro.OldData,
                            NewData = pro.NewData,
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
        /// 用户免费券纪录
        /// </summary>
        [NoAuthorization]
        public ActionResult GetUserTicket([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, int gameID, int userID, DateTime? beginTime = null, DateTime? endTime = null)
        {
            var draw = requestModel.Draw;
            var start = requestModel.Start;
            var length = requestModel.Length;
            var pageIndex = Math.Ceiling((decimal)start / length) + 1;

            if (beginTime == null) beginTime = Convert.ToDateTime(DateTime.Now.ToShortDateString());

            var api = SiteHelper.GetAPIResult<PagedList<UserTicket>>($"/User/GetUserTicket?gameID={gameID}&userID={userID}&beginTime={beginTime}&endTime={endTime}&pageSize={length}&pageIndex={pageIndex}");

            var query = from pro in api.PageData
                        select new
                        {
                            pro.Name,
                            CreateTime = pro.CreateTime.ToString("yyyy-MM-dd HH:mm"),
                            UpdateTime = pro.UpdateTime == null ? string.Empty : pro.UpdateTime.Value.ToString("yyyy-MM-dd HH:mm"),
                            Type = pro.Type.GetDisplayName()
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
        /// 比赛排名纪录
        /// </summary>
        [NoAuthorization]
        public ActionResult GetUserInfoRankLog([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, int? gameID, int userID, DateTime? beginTime = null, DateTime? endTime = null)
        {
            var draw = requestModel.Draw;
            var start = requestModel.Start;
            var length = requestModel.Length;
            var pageIndex = Math.Ceiling((decimal)start / length) + 1;

            if (beginTime == null) beginTime = Convert.ToDateTime(DateTime.Now.ToShortDateString());

            var api = SiteHelper.GetAPIResult<PagedList<UserRank>>($"/User/GetUserInfoRankLog?gameID={gameID}&userID={userID}&beginTime={beginTime}&endTime={endTime}&pageSize={length}&pageIndex={pageIndex}");

            var query = from pro in api.PageData
                        select new
                        {
                            pro.Name,
                            Type = pro.Type.GetDisplayName(),
                            CreateTime = pro.CreateTime.ToString("yyyy-MM-dd HH:mm"),
                            LastCalcRank = pro.LastCalcRank.GetDisplayName(),
                            pro.Rank,
                            pro.Score,
                            pro.Reward
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

        public ActionResult ResetAlipayAccount(int userID)
        {
            SiteHelper.GetAPIResult("/User/ResetAlipay?userid=" + userID + "&ip=" + IPHelper.GetIP());
            return Alert("支付宝账号已经重置,可重新绑定", Request.UrlReferrer.ToString());
        }
        public ActionResult ResetUnionpayAccount(int userID)
        {
            SiteHelper.GetAPIResult("/User/ResetUnionpay?userid=" + userID + "&ip=" + IPHelper.GetIP());
            return Alert("銀聯账号已经重置,可重新绑定", Request.UrlReferrer.ToString());
        }

        /// <summary>
        /// 解除綁定
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public ActionResult ResetPhoneNumber(int userID)
        {
            SiteHelper.GetAPIResult(string.Format("/User/ResetPhoneNumber?userid={0}&ip={1}", userID, IPHelper.GetIP()));
            return Alert("注册手机号已经重置,可重新绑定", Request.UrlReferrer.ToString());
        }

        [NoAuthorization]
        public ActionResult ForbidUser(int userID)
        {
            var _memo = string.Format("{0} 封锁帐号，原因：{1}", this.CurrentUser.LoginName, "用户管理快速封锁");
            var api = SiteHelper.GetAPIResult<APIResult>(string.Format("/ForbidUser/ForbidUser?userID={0}&memo={1}&ipAddress={2}", userID, Server.UrlEncode(_memo), Server.UrlEncode(SystemHelper.UserIPAddress)));
            if (api.Result == true) return Alert("操作成功", Request.UrlReferrer.ToString());
            return Alert(api.Message, Request.UrlReferrer.ToString());
        }

        public ActionResult KickoutGameAccount(int userID)
        {
            var result = SiteHelper.GetAPIResult<APIResult>("/Game/KickoutGameUser?userid=" + userID);

            var data = new
            {
                status = result.Result,
                msg = result.Result ? "已經踢出房間" : "踢出房間失敗"
            };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// VIP用户列表
        /// </summary>
        /// <returns></returns>
        public ActionResult VIPList()
        {
            var list = SiteHelper.GetAPIResult<List<VIPUserInfo>>("/User/GetFocusUser");
            return View(list);
        }

        /// <summary>
        /// 删除VIP用户
        /// </summary>
        public ActionResult DeleteVIP(int userID)
        {
            var api = SiteHelper.GetAPIResult<APIResult>(string.Format("/User/DeleteFocusUser?userID={0}", userID));
            if (api.Result)
                return Back();
            else
                return Alert(api.Message);
        }

        /// <summary>
        /// 添加VIP用户
        /// </summary>
        /// <returns></returns>
        public ActionResult AddVIP()
        {
            return View();
        }

        /// <summary>
        /// 添加VIP用户
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddVIP(int userID)
        {
            var focusUser = new FocusUser()
            {
                Id = userID
            };
            var id = UserService.GetUserID(Convert.ToInt32(userID), null, null);
            if (id == null || id == 0)
                return Alert("操作失败：用户不存在！");
            var api = SiteHelper.PostAPIResult<FocusUser>("/User/AddFocusUser", focusUser);
            if (api.Result)
                return ConfirmReturnList("VIPList");
            else
                return Alert(api.Message);
        }


        /// <summary>
        /// 使用者现金记录查询
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetMoneyLog(int? draw, int? start, int length, int? userID = null, DateTime? beginTime = null, DateTime? endTime = null)
        {
            if (beginTime == null) beginTime = DateTime.Now.AddDays(-1);

            var pageIndex = Math.Ceiling((decimal)start.Value / length) + 1;

            var api = SiteHelper.GetAPIResult<PagedList<MoneyLogReport>>(string.Format("/User/GetMoneyLog?userID={0}&beginTime={1}&endTime={2}&pageIndex={3}&pageSize={4}", userID, beginTime, endTime, pageIndex, length));

            var query = from pro in api.PageData
                        select new
                        {
                            CreateTime = pro.CreateTime.ToString("yyyy/MM/dd HH:mm:ss"),
                            SourceName = pro.SourceType.SourceName,
                            CurrentAmount = pro.CurrentAmount.ToCash(),
                            UpdateAmount = pro.UpdateAmount.ToCash(),
                            TotalAmount = (pro.CurrentAmount + pro.UpdateAmount).ToCash(),
                            FlowAmount = pro.FlowAmount.ToCash(),
                            IPAddress = pro.IPAddress,
                            Memo = pro.Memo,
                            pro.UserID,
                            pro.TradeID
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
        /// 使用者银行记录查询
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetBankLog(int? draw, int? start, int length, int? userID = null, DateTime? beginTime = null, DateTime? endTime = null)
        {
            if (beginTime == null) beginTime = DateTime.Now.AddDays(-1);
            var pageIndex = Math.Ceiling((decimal)start.Value / length) + 1;
            var operatorID = CurrentUser.UserID;
            var api = SiteHelper.GetAPIResult<PagedList<MoneyLog>>($"/User/GetBankLog?operatorID={operatorID}&userID={userID}&beginTime={beginTime}&endTime={endTime}&pageIndex={pageIndex}&pageSize={length}");

            var query = from pro in api.PageData
                        select new
                        {
                            CreateTime = pro.CreateTime.ToString("yyyy/MM/dd HH:mm:ss"),
                            SourceName = pro.SourceType.SourceName,
                            CurrentAmount = pro.CurrentAmount.ToCash(),
                            UpdateAmount = pro.UpdateAmount.ToCash(),
                            TotalAmount = (pro.CurrentAmount + pro.UpdateAmount).ToCash(),
                            IPAddress = pro.IPAddress,
                            Memo = pro.Memo,
                            pro.UserID
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
        /// 使用者税收记录查询
        /// </summary>
        /// <returns></returns>

        [HttpPost]
        public ActionResult GetGameLog(int? draw, int? start, int length, int? userID = null, DateTime? beginTime = null, DateTime? endTime = null)
        {

            if (beginTime == null) beginTime = Convert.ToDateTime(DateTime.Now.ToShortDateString());

            var pageIndex = Math.Ceiling((decimal)start.Value / length) + 1;

            var api = SiteHelper.GetAPIResult<PagedList<GameLog>>(string.Format("/User/GetGameLog?userID={0}&logoutBeginTime={1}&logoutEndTime={2}&pageIndex={3}&pageSize={4}", userID, beginTime, endTime, pageIndex, length));

            var query = from pro in api.PageData
                        select new
                        {
                            ServerName = pro.GameServer.ServerName,
                            LoginMoney = pro.LoginMoney.ToCash(),
                            Amount = pro.Amount.ToCash(),
                            TaxAmount = pro.TaxAmount.ToCash(),
                            WinCount = pro.WinCount,
                            LoseCount = pro.LoseCount,
                            DrawCount = pro.DrawCount,
                            GameTime = pro.GameTime,
                            IPAddress = pro.IPAddress,
                            HDType = EnumExtensions.GetDisplayName(pro.HDType),
                            HDCode = pro.HDCode,
                            LoginTime = pro.LoginTime.ToString("yyyy/MM/dd HH:mm:ss"),
                            LogoutTime = pro.LogoutTime.ToString("yyyy/MM/dd HH:mm:ss"),
                            pro.UserID
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
        /// 使用者充值记录查询
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetOrderList(int? draw, int? start, int? length, string submitType, int? userID = null, DateTime? beginTime = null, DateTime? endTime = null)
        {

            if (beginTime == null) beginTime = Convert.ToDateTime(DateTime.Now.ToShortDateString());
            var adminId = SiteService.GetAdminUserID();

            switch (submitType)
            {
                case "Export":
                    return this.exportOrderList(userID, beginTime, endTime, adminId);
                default:
                    var pageIndex = Math.Ceiling((decimal)start.Value / length.Value) + 1;

                    var api = SiteHelper.GetAPIResult<PagedList<OrderListDTO>>(string.Format("/User/GetPayOrder?userID={0}&createBeginTime={1}&createEndTime={2}&pageIndex={3}&pageSize={4}&adminId={5}", userID, beginTime, endTime, pageIndex, length, adminId));

                    var query = from pro in api.PageData
                                select new
                                {
                                    CreateTime = pro.CreateTime.ToString("yyyy/MM/dd HH:mm:ss"),
                                    LastUpdateTime = pro.LastUpdateTime != null ? pro.LastUpdateTime.Value.ToString("yyyy/MM/dd HH:mm:ss") : string.Empty,
                                    OrderID = pro.OrderID,
                                    PayTypeID = pro.PayTypeID.GetDisplayName(),
                                    Amount = pro.Amount,
                                    PointAmount = pro.PointAmount,
                                    Status = pro.Status.GetDisplayName(),
                                    IPAddress = pro.IPAddress,
                                    TransID = pro.TransID,
                                    TransData = pro.TransData,
                                    pro.UserID
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
        }

        /// <summary>
        /// 使用者兑换记录查询
        /// </summary>
        /// <returns></returns>

        [HttpPost]
        public ActionResult GetConvertLog(int? draw, int? start, int length, int? userID = null, DateTime? beginTime = null, DateTime? endTime = null)
        {
            if (beginTime == null) beginTime = DateTime.Now.AddHours(-24);

            var pageIndex = Math.Ceiling((decimal)start.Value / length) + 1;
            var adminId = SiteService.GetAdminUserID();

            var api = SiteHelper.GetAPIResult<PagedList<ConvertLog>>(string.Format("/User/GetConvertLog?userID={0}&beginTime={1}&endTime={2}&pageIndex={3}&pageSize={4}&adminId={5}", userID, beginTime, endTime, pageIndex, length, adminId));
            var PageData = api.PageData.Where(x => !x.Id.Equals(0));
            var query = from pro in PageData
                        select new
                        {
                            Id = pro.Id,
                            CreateTime = pro.CreateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                            Status = pro.Status.GetDisplayName(),
                            Amount = pro.Amount.ToCash(),
                            ConvertType = pro.ConvertType == 1 ? "支付宝" : "银行卡",
                            Account = pro.Account,
                            RealName = pro.RealName,
                            BankName = pro.BankName,
                            TransID = pro.TransID,
                            TransData = pro.TransData,
                            CreateIP = pro.CreateIP,
                            LastUpdateTime = pro.LastUpdateTime.ToString("yyyy-MM-dd HH:mm")
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
        /// 每局游戏记录查询
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetDetailLog(int? draw, int? start, int length, int? userID = null, DateTime? beginTime = null, DateTime? endTime = null)
        {
            var pageIndex = Math.Ceiling((decimal)start.Value / length) + 1;

            var api = SiteHelper.GetAPIResult<PagedList<DetailLog>>(string.Format("/PerGameLog/GetDetailLog?userID={0}&beginTime={1}&endTime={2}&pageIndex={3}&pageSize={4}"
                , userID, beginTime, endTime, pageIndex, length));

            var query = from pro in api.PageData
                        select new
                        {
                            GameTime = pro.GameTime.ToString("yyyy-MM-dd HH:mm:ss"),
                            LogID = pro.LogID,
                            ServerID = pro.ServerID,
                            ServerName = pro.ServerName,
                            GameName = pro.GameName,
                            CurrentAmount = pro.CurrentAmount.ToCash(),
                            UpdateAmount = pro.UpdateAmount.ToCash(),
                            CurrentCouponAmount = pro.CurrentCouponAmount.ToCash(),
                            CouponAmount = pro.CouponAmount.ToCash()
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
        public ActionResult GetMoneyDetailLog(long? tradeID, int? userID)
        {

            if (!tradeID.HasValue || !userID.HasValue) { return View(new List<DetailLog>()); }

            var api = SiteHelper.GetAPIResult<List<DetailLog>>(string.Format("/PerGameLog/GetGameLogByTradeID?tradeID={0}&userID={1}"
               , tradeID.Value, userID.Value));
            #region 空值判斷
            if (api == null)
            {
                return Json(new
                {
                    success = true,
                    data = string.Empty
                });
            }
            #endregion

            var _newApi = api.Select(pro => new
            {
                GameTime = pro.GameTime.ToString("yyyy-MM-dd HH:mm:ss"),
                LogID = pro.LogID,
                ServerID = pro.ServerID,
                ServerName = pro.ServerName,
                GameName = pro.GameName,
                CurrentAmount = pro.CurrentAmount.ToCash(),
                UpdateAmount = pro.UpdateAmount.ToCash()
            });

            return Json(new
            {
                recordsTotal = _newApi.Count(),
                recordsFiltered = _newApi.Count(),
                success = true,
                data = _newApi
            });
        }

        /// <summary>
        /// 在线玩家总汇
        /// </summary>
        public ActionResult Online(int? column, string sortDes)
        {
            ViewBag.column = column;
            ViewBag.sortDes = sortDes;
            return View();
        }


        /// <summary>
        /// 在线玩家总汇
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetOnlineUser([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, int? gameID, int? serverID)
        {
            var draw = requestModel.Draw;
            var start = requestModel.Start;
            var length = requestModel.Length;
            var sortColumn = requestModel.Columns.GetSortedColumns().FirstOrDefault();
            var sortName = sortColumn.Name;
            var sortLifting = (int)sortColumn.SortDirection;
            var pageIndex = Math.Ceiling((decimal)start / length) + 1;
            var operatorID = CurrentUser.UserID;

            var api = SiteHelper.GetAPIResult<PagedList<OnlineUser>>($"/PerGameLog/GetOnlineUser?operatorID={operatorID}&gameID={gameID}&pageIndex={pageIndex}&pageSize={length}"
                + $"&sortName={sortName}&sortLifting={sortLifting}&serverID={serverID}");

            var query = from pro in api.PageData
                        select new
                        {
                            pro.Id,
                            pro.ServerName,
                            pro.UserID,
                            SumCharge = pro.SumCharge.ToCash(),
                            SumConvert = pro.SumConvert.ToCash(),
                            pro.TotalWinLose,
                            Amount = pro.Amount.ToCash(),
                            BankAmount = pro.BankAmount.ToCash(),
                            TotalGameWinLose = pro.TotalGameWinLose.ToCash(),
                            pro.AreaName,
                            HDType = pro.HDType == 2 ? "Android" : "iOS",
                            pro.IP,
                            pro.Location,
                            CreateTime = pro.CreateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                            pro.ServerID,
                            pro.WinCount,
                            pro.LoseCount,
                            LastLogonTime = pro.LastLogonTime.ToString("yyyy-MM-dd HH:mm:ss"),
                            pro.TableURL
                        };

            return Json(new
            {
                draw = draw,
                recordsTotal = api.TotalCount,
                recordsFiltered = api.TotalCount,
                success = true,
                data = query,
                output = api.OutputData
            });
        }

        /// <summary>
        /// 輸贏排行榜
        /// </summary>
        /// <returns></returns>
        public ActionResult WinRecord()
        {
            return View();
        }

        /// <summary>
        /// 取得輸贏列表
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        [NoAuthorization]
        public ActionResult GetWinLostLeaderboard(int? userID, DateTime? startTime, DateTime? endTime, [ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            var _sortColumn = requestModel.Columns.GetSortedColumns().FirstOrDefault();
            var _sortName = _sortColumn.Name;
            var _sortLifting = (int)_sortColumn.SortDirection;
            var _operatorID = CurrentUser.UserID;
            var _result = SiteHelper.GetAPIResult<List<WinLostLeaderboard>>(string.Format("User/GetWinLostLeaderboard?operatorID={0}&userID={1}&startTime={2}&endTime={3}&sortName={4}&sortLifting={5}", _operatorID, userID, startTime, endTime, _sortName, _sortLifting));

            return Json(new
            {
                recordsTotal = _result.Count(),
                recordsFiltered = _result.Count(),
                success = true,
                data = _result
            }, JsonRequestBehavior.AllowGet);
        }

        [NoAuthorization]
        public ActionResult GameServerList()
        {
            var _gameServers = SiteHelper.GetAPIResult<List<GameServer>>("/Game/GetGameServer");
            return Json(_gameServers, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult OnlineVipUserCount()
        {

            var adminId = SiteService.GetAdminUserID();

            var _onlineVipUserCount = SiteHelper.GetAPIResult<int>($"/User/GetOnlineVipUserCount?adminId={adminId}");
            return Json(_onlineVipUserCount, JsonRequestBehavior.AllowGet);
        }

        [NoAuthorization]
        public ActionResult GetPlatformDownloadIncorrectCount()
        {
            var adminId = SiteService.GetAdminUserID();
            var _onlineVipUserCount = SiteHelper.GetAPIResult<int>($"/User/GetPlatformDownloadIncorrectCount?adminId={adminId}");
            return Json(_onlineVipUserCount, JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult Download(string fileGuid, string fileName)
        {
            if (TempData[fileGuid] != null)
            {
                byte[] data = TempData[fileGuid] as byte[];
                return File(data, "application/vnd.ms-excel", fileName);
            }
            else
            {
                return new EmptyResult();
            }
        }

        private ActionResult exportOrderList(int? userID, DateTime? beginTime, DateTime? endTime, int adminId)
        {
            var _datas = SiteHelper.GetAPIResult<PagedList<OrderListDTO>>(string.Format("/User/GetPayOrder?userID={0}&createBeginTime={1}&createEndTime={2}&pageIndex={3}&pageSize={4}&adminId={5}", userID, beginTime, endTime, 1, 10000, adminId));

            var _path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "ExportTemplat", "UserInfo_OrderList.xlsx");

            using (FileStream file = new FileStream(_path, FileMode.Open, FileAccess.Read))
            {
                #region 畫Excel

                var _workbook = new XSSFWorkbook(file);
                var _sheet = _workbook.GetSheetAt(0);
                file.Close();

                _sheet.GetRow(1).GetCell(7).SetCellValue(string.Format("{0}~{1}", beginTime.Value.ToString("yyyy-MM-dd HH:mm:ss"), endTime.HasValue ? endTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : ""));

                if (_datas.TotalCount != 0)
                {
                    for (var i = 0; i < _datas.PageData.Count; i++)
                    {
                        OrderListDTO _order = _datas.PageData[i];
                        var _row = _sheet.CreateRow(3 + i);
                        _row.CreateCell(0).SetCellValue(_order.UserID);
                        _row.CreateCell(1).SetCellValue(_order.CreateTime.ToString("yyyy/MM/dd HH:mm:ss"));
                        _row.CreateCell(2).SetCellValue(_order.OrderID);
                        _row.CreateCell(3).SetCellValue(_order.PayTypeID.GetDisplayName());
                        _row.CreateCell(4).SetCellValue(_order.Amount);
                        _row.CreateCell(5).SetCellValue(_order.Status.GetDisplayName());
                        _row.CreateCell(6).SetCellValue(_order.IPAddress);
                        _row.CreateCell(7).SetCellValue(_order.TransID);
                        _row.CreateCell(8).SetCellValue(_order.TransData);

                    }
                }

                #endregion

                using (var exportData = new MemoryStream())
                {
                    _workbook.Write(exportData);
                    var _saveAsFileName = string.Format("充值记录_{0}_{1:d}.xlsx", userID, DateTime.Now).Replace("/", "-");
                    var _handle = Guid.NewGuid().ToString();

                    exportData.Position = 0;
                    TempData[_handle] = exportData.ToArray();
                    return new JsonResult() { Data = new { FileGuid = _handle, FileName = _saveAsFileName } };
                }
            }
        }
    }
}