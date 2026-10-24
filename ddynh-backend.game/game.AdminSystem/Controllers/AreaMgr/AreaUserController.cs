using FW.Core.Helper;
using game.Entity.Game;
using game.Entity.Money;
using game.Entity.Pay;
using game.Entity.Users;
using System;
using System.Web.Mvc;

namespace game.AdminSystem.Controllers.AreaMgr
{
    /// <summary>
    /// 渠道用户相关查询
    /// </summary>
    public class AreaUserController : BaseController
    {
        /// <summary>
        /// 查询用户信息
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public ActionResult SearchUserInfo(int? userID = null, string loginName = "", string nickName = "", DateTime? beginTime = null, DateTime? endTime = null)
        {
            if (userID == null && loginName.Length == 0 && nickName.Length == 0) return View();

            ViewBag.BeginTime = beginTime;
            ViewBag.EndTime = endTime;
            userID = UserService.GetUserID(userID, loginName, nickName);
            if (userID == 0) return View();

            var detail = SiteHelper.GetAPIResult<UserInfoDetail>(string.Format("/User/GetUserInfoDetail?userID={0}&beginTime={1}&endTime={2}", userID, beginTime, endTime));
            if (!SiteHelper.GetAdminAreaIDs().Contains(detail.AreaID)) return View();
            return View(detail);
        }

        /// <summary>
        /// 用户金币记录查询
        /// </summary>
        /// <returns></returns>
        public ActionResult UserMoneyLog(int? platformId = null, int? areaID = null, int? userID = null, string loginName = "", string nickName = "", int? sourceTypeGroup = null, int? sourceType = null, string ipAddress = "", DateTime? beginTime = null, DateTime? endTime = null, int p = 1)
        {
            userID = UserService.GetUserID(userID, loginName, nickName);
            if (userID == null && areaID == null)
            {
                return View(new PagedList<MoneyLog>());
            }

            if (areaID != null && !SiteHelper.GetAdminAreaIDs().Contains(areaID.Value))
            {
                return View(new PagedList<GameLog>());
            }

            if (userID != null)
            {
                var user = UserService.GetUserByID(userID.Value);
                if (user == null || !SiteHelper.GetAdminAreaIDs().Contains(user.AreaID))
                {
                    return View(new PagedList<MoneyLog>());
                }
            }

            if (beginTime == null) beginTime = DateTime.Now.AddDays(-1);

            var api = SiteHelper.GetAPIResult<MoenyLogAddSumFlowAmountReport>(string.Format("/Money/GetMoneyLog?userID={0}&sourceTypeGroup={1}&sourceType={2}&ipAddress={3}&beginTime={4}&endTime={5}&areaID={6}&platformId={7}&pageIndex={8}", userID, sourceTypeGroup, sourceType, ipAddress, beginTime, endTime, areaID, platformId, p));
            var pageList = api.PageListMoneyLog;

            return View(pageList);
        }

        /// <summary>
        /// 用户支付订单查询
        /// </summary>
        /// <returns></returns>
        public ActionResult UserOrderList(int? areaID = null, int? userID = null, string loginName = "", string nickName = "", int? orderID = null, string transData = "", string ipAddress = null, int? payTypeID = null, CDOrderStatus? status = null, DateTime? createBeginTime = null, DateTime? createEndTime = null, int p = 1)
        {
            userID = UserService.GetUserID(userID, loginName, nickName);
            if (userID == null && areaID == null)
            {
                return View(new PagedList<CDPayOrder>());
            }

            if (areaID != null && !SiteHelper.GetAdminAreaIDs().Contains(areaID.Value))
            {
                return View(new PagedList<GameLog>());
            }

            if (userID != null)
            {
                var user = UserService.GetUserByID(userID.Value);
                if (user == null || !SiteHelper.GetAdminAreaIDs().Contains(user.AreaID))
                {
                    return View(new PagedList<CDPayOrder>());
                }
            }

            if (createBeginTime == null) createBeginTime = Convert.ToDateTime(DateTime.Now.ToShortDateString());

            var adminId = SiteService.GetAdminUserID();
            var list = SiteHelper.GetAPIResult<PagedList<CDPayOrder>>(string.Format("/Pay/CDGetPayOrder?orderID={0}&userID={1}&transData={2}&ipAddress={3}&payTypeID={4}&status={5}&createBeginTime={6}&createEndTime={7}&areaID={8}&pageIndex={9}&adminId={10}", orderID, userID, transData.Trim(), ipAddress, payTypeID, status, createBeginTime, createEndTime, areaID, p, adminId));

            return View(list);
        }

        /// <summary>
        /// 用户游戏记录查询
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="loginName"></param>
        /// <param name="nickName"></param>
        /// <param name="serverID">服务器ID</param>
        /// <param name="gameID">游戏</param>
        /// <param name="ipAddress"></param>
        /// <param name="hDtype"></param>
        /// <param name="hDcode"></param>
        /// <param name="loginBeginTime"></param>
        /// <param name="loginEndTime"></param>
        /// <param name="logoutBeginTime"></param>
        /// <param name="logoutEndTime"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public ActionResult UserGameLog(int? areaID = null, int? userID = null, string loginName = "", string nickName = "",
            int? serverID = null, int? gameID = null, string ipAddress = null, game.Entity.Users.HDType? hDtype = null, string hDcode = null,
            DateTime? loginBeginTime = null, DateTime? loginEndTime = null, DateTime? logoutBeginTime = null, DateTime? logoutEndTime = null, int p = 1)
        {
            userID = UserService.GetUserID(userID, loginName, nickName);
            if (userID == null && areaID == null)
            {
                return View(new PagedList<GameLog>());
            }

            if (areaID != null && !SiteHelper.GetAdminAreaIDs().Contains(areaID.Value))
            {
                return View(new PagedList<GameLog>());
            }

            if (userID != null)
            {
                var user = UserService.GetUserByID(userID.Value);
                if (user == null || !SiteHelper.GetAdminAreaIDs().Contains(user.AreaID))
                {
                    return View(new PagedList<GameLog>());
                }
            }

            if (logoutBeginTime == null) logoutBeginTime = Convert.ToDateTime(DateTime.Now.ToShortDateString());

            var pagedList = SiteHelper.GetAPIResult<PagedList<GameLog>>(
                string.Format("/Game/GetGameLog?serverID={0}&gameID={1}&userID={2}&ipAddress={3}&hDtype={4}&hDcode={5}&loginBeginTime={6}&loginEndTime={7}&logoutBeginTime={8}&logoutEndTime={9}&areaID={10}&pageIndex={11}",
                serverID,
                gameID,
                userID,
                ipAddress,
                hDtype, hDcode,
                loginBeginTime, loginEndTime,
                logoutBeginTime, logoutEndTime,
                areaID, p));

            return View(pagedList);
        }
    }
}