using Agent77.Filters;
using Repository.Models;
using Repository.ViewModel;
using Service;
using Service.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Linq.Dynamic;
using Agent77.Helper;

namespace Agent77.Areas.Agent.Controllers
{
    [ModeAuthorizeAttribute(ModeType.Mode5)]
    public class UnlimitController : BaseController
    {
        private UnlimitService _service;

        public UnlimitController()
        {
            _service = new UnlimitService(this.ModelState);
        }

        public ActionResult Index()
        {
            int? userid = null;
            if (!UserHelper.IsAdmin)
                userid = UserHelper.User.UserID;

            var model = new Unlimit_GetUserSumData();

            var result = this._service.GetUserSumData(UserHelper.AdminID, userid);
            if (result.Item1)
                model = result.Item2;

            return PartialView(model);
        }

        /// <summary>
        /// 团队列表
        /// </summary>
        /// <returns></returns>
        public ActionResult UserInfoList()
        {
            return View();
        }

        /// <summary>
        /// 团队列表
        /// </summary>
        /// <param name="draw"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <param name="targetID"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UserInfoList(int draw, int start, int length, int? targetID)
        {
            int? userid = null;
            if (!UserHelper.IsAdmin)
                userid = UserHelper.User.UserID;

            var model = new List<Unlimit_GetUserInfo>();

            var result = this._service.GetUserInfoList(UserHelper.AdminID, userid, targetID, start / length + 1, length);
                     
            if (result.Item1)
                model = result.Item3;
            else
                return Json(new ResultJson() {                 
                    message = result.Item2
                });

            int recordsTotal = result.Item4;
            string col_index = Request.Form["order[0][column]"] ?? "0";
            string sortColName = Request.Form[$@"columns[{col_index}][name]"];
            string asc_desc = Request.Form["order[0][dir]"] ?? "desc";

            var query = (from p in model
                         select new
                         {
                             UserID = p.UserID,
                             NickName = p.NickName,
                             PlatformID = p.PlatformID,
                             PlatformName = p.PlatformName,
                             StageLevel = p.StageLevel,
                             Status = p.Status,
                             TopUserID = p.TopUserID,
                             SubDirectUser = string.Format("{0:N0}", p.SubDirectUser),
                             SubUser = string.Format("{0:N0}", p.SubUser),
                             TotalUser = string.Format("{0:N0}", p.TotalUser),
                             TotalPerformance = p.TotalPerformance.ToCash(),
                             PoolAmount = p.PoolAmount.ToCash(),
                             WithDrawAmount = p.WithDrawAmount.ToCash(),
                             CreateTime = p.CreateTime,
                             Memo = p.Memo,
                             UpperUserID = p.UpperUserID
                         }).ToList();

            //if (length != -1)
            //    query = query.Skip(start).Take(length).ToList();
            
            return Json(new ResultJson
            {             
                success = result.Item1,
                message = result.Item2,
                draw = draw,
                recordsTotal = recordsTotal,
                recordsFiltered = recordsTotal,
                data = query
            });
        }

        /// <summary>
        /// 用户资讯
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult UserInfo(int id)
        {
            int? userid = null;
            if (!UserHelper.IsAdmin)
                userid = UserHelper.User.UserID;

            var model = new UserInfoModel();

            var result = this._service.GetUserInfo(UserHelper.AdminID, userid, id);

            if (result.Item1)
            {
                model.UserInfo = result.Item3;
                model.UserGameInfo = result.Item4;
            }
            else
            {
                Error(result.Item2);
            }

            return PartialView("_UnlimitUserInfo", model);
        }       

        [CustomAuthorize(new RoleType[] { RoleType.Admin, RoleType.Biz })]
        [HttpPost]
        public ActionResult ChangeUserStatus(UserInfoModel model)
        {
            var status = model.UserInfo.UserStatus == 0 ? 1 : 0;
            var result = this._service.ChangeUserStatus(UserHelper.AdminID, 4, model.UserInfo.UserID, status);

            if (!result.Item1)
                Error(result.Item2);
            else
                Success(result.Item2);

            return RedirectToAction("UserInfo", new { id = model.UserInfo.UserID });
        }

        /// <summary>
        /// 提現紀錄
        /// </summary>
        /// <returns></returns>       
        [CustomAuthorize(new RoleType[] { RoleType.Agent })]
        public ActionResult UserConvertInfo()
        {
            return View();
        }

        /// <summary>
        /// 提現紀錄
        /// </summary>
        /// <param name="draw"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <param name="targetID"></param>
        /// <returns></returns>
        [CustomAuthorize(new RoleType[] { RoleType.Agent })]
        [HttpPost]
        public ActionResult UserConvertInfo(int draw, int start, int length, int? targetID, int? orderID, DateTime? dtStartTime, DateTime? dtEndTime)
        {
            int? userid = null;            
            if (!UserHelper.IsAdmin)
                userid = UserHelper.User.UserID;           
           
            var model = new List<Unlimit_GetUserConvertInfo>();

            var result = this._service.GetUserConvertInfo(UserHelper.AdminID, userid, targetID, orderID, dtStartTime, dtEndTime);

            if (result.Item1)
                model = result.Item3;
            else
                return Json(new ResultJson()
                {
                    message = result.Item2
                });

            int recordsTotal = model.Count();
            string col_index = Request.Form["order[0][column]"] ?? "0";
            string sortColName = Request.Form[$@"columns[{col_index}][name]"];
            string asc_desc = Request.Form["order[0][dir]"] ?? "desc";

            var query = (from p in model.OrderBy($@"{sortColName} {asc_desc}")
                         select new
                         {
                             OrderID = p.OrderID,
                             UserID = p.UserID,
                             NickName = p.NickName,
                             PlatformID = p.PlatformID,
                             PlatformName = p.PlatformName,
                             ConvertAmount = p.ConvertAmount.ToCash(),
                             PoolAmount = p.PoolAmount.ToCash(),
                             Status = p.Status,                            
                             CreateTime = p.CreateTime,
                             Updatetime = p.Updatetime,
                             Updater = p.Updater
                         }).ToList();

            if (length != -1)
                query = query.Skip(start).Take(length).ToList();

            return Json(new ResultJson
            {
                success = result.Item1,
                message = result.Item2,
                draw = draw,
                recordsTotal = recordsTotal,
                recordsFiltered = recordsTotal,
                data = query
            });
        }

        /// <summary>
        /// 下线提现纪录
        /// </summary>
        /// <returns></returns>
        public ActionResult SubUserConvertInfo()
        {
            return View();
        }

        /// <summary>
        /// 下线提现纪录
        /// </summary>
        /// <param name="draw"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <param name="targetID"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SubUserConvertInfo(int draw, int start, int length, int? targetID, int? orderID, DateTime? dtStartTime, DateTime? dtEndTime)
        {
            int? userid = null;
            if (!UserHelper.IsAdmin)
                userid = UserHelper.User.UserID;

            var model = new List<Unlimit_GetSubUserConvertInfo>();

            var result = this._service.GetSubUserConvertInfo(UserHelper.AdminID, userid, targetID, orderID, dtStartTime, dtEndTime);

            if (result.Item1)
                model = result.Item3;
            else
                return Json(new ResultJson()
                {
                    message = result.Item2
                });

            int recordsTotal = model.Count();
            string col_index = Request.Form["order[0][column]"] ?? "0";
            string sortColName = Request.Form[$@"columns[{col_index}][name]"];
            string asc_desc = Request.Form["order[0][dir]"] ?? "desc";

            var query = (from p in model.OrderBy($@"{sortColName} {asc_desc}")
                         select new
                         {
                             OrderID = p.OrderID,
                             UserID = p.UserID,                          
                             ConvertAmount = p.ConvertAmount.ToCash(),
                             PoolAmount = p.PoolAmount.ToCash(),
                             Status = p.Status,
                             CreateTime = p.CreateTime,
                             Updatetime = p.Updatetime,
                             Updater = p.Updater
                         }).ToList();

            if (length != -1)
                query = query.Skip(start).Take(length).ToList();

            return Json(new ResultJson
            {
                success = result.Item1,
                message = result.Item2,
                draw = draw,
                recordsTotal = recordsTotal,
                recordsFiltered = recordsTotal,
                data = query
            });
        }

        /// <summary>
        /// 預提紀錄
        /// </summary>
        /// <returns></returns>       
        [CustomAuthorize(new RoleType[] { RoleType.Agent })]
        public ActionResult UserPreConvertInfo()
        {
            return View();
        }

        /// <summary>
        /// 預提紀錄
        /// </summary>
        /// <param name="draw"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <param name="targetID"></param>
        /// <returns></returns>
        [CustomAuthorize(new RoleType[] { RoleType.Agent })]
        [HttpPost]
        public ActionResult UserPreConvertInfo(int draw, int start, int length, int? targetID, int? orderID, DateTime? dtStartTime, DateTime? dtEndTime)
        {
            int? userid = null;
            if (!UserHelper.IsAdmin)
                userid = UserHelper.User.UserID;

            var model = new List<Unlimit_GetUserPreConvertInfo>();

            var result = this._service.GetUserPreConvertInfo(UserHelper.AdminID, userid, targetID, orderID, dtStartTime, dtEndTime);

            if (result.Item1)
                model = result.Item3;
            else
                return Json(new ResultJson()
                {
                    message = result.Item2
                });

            int recordsTotal = model.Count();
            string col_index = Request.Form["order[0][column]"] ?? "0";
            string sortColName = Request.Form[$@"columns[{col_index}][name]"];
            string asc_desc = Request.Form["order[0][dir]"] ?? "desc";

            var query = (from p in model.OrderBy($@"{sortColName} {asc_desc}")
                         select new
                         {
                             OrderID = p.OrderID,
                             UserID = p.UserID,
                             NickName = p.NickName,
                             PlatformID = p.PlatformID,
                             PlatformName = p.PlatformName,
                             PreConvertAmount = p.PreConvertAmount.ToCash(),
                             LastDayBonusAmount = p.LastDayBonusAmount.ToCash(),
                             Status = p.Status,
                             CreateTime = p.CreateTime,
                             Updatetime = p.Updatetime,
                             Updater = p.Updater
                         }).ToList();

            if (length != -1)
                query = query.Skip(start).Take(length).ToList();

            return Json(new ResultJson
            {
                success = result.Item1,
                message = result.Item2,
                draw = draw,
                recordsTotal = recordsTotal,
                recordsFiltered = recordsTotal,
                data = query
            });
        }

        /// <summary>
        /// 下線預提紀錄
        /// </summary>
        /// <returns></returns>
        public ActionResult SubUserPreConvertInfo()
        {
            return View();
        }

        /// <summary>
        /// 下線預提紀錄
        /// </summary>
        /// <param name="draw"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <param name="targetID"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SubUserPreConvertInfo(int draw, int start, int length, int? targetID, int? orderID, DateTime? dtStartTime, DateTime? dtEndTime)
        {
            int? userid = null;
            if (!UserHelper.IsAdmin)
                userid = UserHelper.User.UserID;

            var model = new List<Unlimit_GetSubUserPreConvertInfo>();

            var result = this._service.GetSubUserPreConvertInfo(UserHelper.AdminID, userid, targetID, orderID, dtStartTime, dtEndTime);

            if (result.Item1)
                model = result.Item3;
            else
                return Json(new ResultJson()
                {
                    message = result.Item2
                });

            int recordsTotal = model.Count();
            string col_index = Request.Form["order[0][column]"] ?? "0";
            string sortColName = Request.Form[$@"columns[{col_index}][name]"];
            string asc_desc = Request.Form["order[0][dir]"] ?? "desc";

            var query = (from p in model.OrderBy($@"{sortColName} {asc_desc}")
                         select new
                         {
                             OrderID = p.OrderID,
                             UserID = p.UserID,
                             NickName = p.NickName,
                             PlatformID = p.PlatformID,
                             PlatformName = p.PlatformName,
                             PoolAmount = p.PoolAmount.ToCash(),
                             BonusAmount = p.BonusAmount.ToCash(),
                             Status = p.Status,
                             CreateTime = p.CreateTime,
                             Updatetime = p.Updatetime,
                             Updater = p.Updater
                         }).ToList();

            if (length != -1)
                query = query.Skip(start).Take(length).ToList();

            return Json(new ResultJson
            {
                success = result.Item1,
                message = result.Item2,
                draw = draw,
                recordsTotal = recordsTotal,
                recordsFiltered = recordsTotal,
                data = query
            });
        }

        public ActionResult ConvertWithdrawOrder(int orderID, int status)
        {
            var result = this._service.WithdrawOrder(UserHelper.AdminID, orderID, status);

            if (result.Item1)
                Success(result.Item2);
            else
                Error(result.Item2);

            return RedirectToAction("SubUserConvertInfo");
        }

        public ActionResult PreConvertWithdrawOrder(int orderID, int status)
        {
            var result = this._service.WithdrawOrder(UserHelper.AdminID, orderID, status);

            if (result.Item1)
                Success(result.Item2);
            else
                Error(result.Item2);

            return RedirectToAction("SubUserPreConvertInfo");
        }

        /// <summary>
        /// 修改用户备注
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="memo"></param>
        /// <returns></returns>
        [CustomAuthorize(new RoleType[] { RoleType.Admin, RoleType.Biz, RoleType.CFO })]
        [HttpPost]
        public ActionResult UserInfoMemoUpdate(int userID, string memo)
        {
            var result = this._service.UserInfoMemoUpdate(UserHelper.AdminID.Value, userID, memo);

            if (result.Item1)
                Success(result.Item2);
            else
                Error(result.Item2);

            return RedirectToAction("UserInfoList");
        }

        /// <summary>
        /// 推广人数信息
        /// </summary>
        /// <param name="draw"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetUserInfoDetail(int draw, int start, int length, int? userID)
        {
            int? userid = null;
            if (!UserHelper.IsAdmin)
                userid = UserHelper.User.UserID;

            var model = new List<Unlimit_GetUserInfoDetail>();

            var result = this._service.GetUserInfoDetail(userID);

            if (result.Item1)
                model = result.Item3;
            else
                return Json(new ResultJson()
                {
                    message = result.Item2
                });

            int recordsTotal = model.Count();
            string col_index = Request.Form["order[0][column]"] ?? "0";
            string sortColName = Request.Form[$@"columns[{col_index}][name]"];
            string asc_desc = Request.Form["order[0][dir]"] ?? "desc";

            var query = (from p in model.OrderBy($@"{sortColName} {asc_desc}")
                         select new
                         {
                             UserID = p.UserID,
                             StorageAmount = p.StorageAmount.ToCash(),
                             SumCharge = p.SumCharge.ToCash(),
                             SumConvert = p.SumConvert.ToCash(),
                             TodayCharge = p.TodayCharge.ToCash(),
                             TodayConvert = p.TodayConvert.ToCash(),
                             TopUserID = p.TopUserID,
                             Memo = p.Memo
                         }).ToList();

            if (length != -1)
                query = query.Skip(start).Take(length).ToList();

            return Json(new ResultJson
            {
                success = result.Item1,
                message = result.Item2,
                draw = draw,
                recordsTotal = recordsTotal,
                recordsFiltered = recordsTotal,
                data = query
            });
        }

        /// <summary>
        /// 绑定上级
        /// </summary>
        /// <returns></returns>
        [CustomAuthorize(new RoleType[] { RoleType.Admin, RoleType.Biz })]
        public ActionResult AddUserRelation()
        {
            return View();
        }

        /// <summary>
        /// 绑定上级
        /// </summary>
        /// <param name="topuserid"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        [CustomAuthorize(new RoleType[] { RoleType.Admin, RoleType.Biz })]
        [HttpPost]
        public ActionResult AddUserRelation(int? topuserid, int? userid)
        {
            if (!topuserid.HasValue)
                ModelState.AddModelError("topuserid", "上级ID不能为空");

            if (!userid.HasValue)
                ModelState.AddModelError("userid", "下线ID不能为空");

            if (ModelState.IsValid)
            {
                var result = this._service.AddUserRelation(topuserid, userid);

                if (result.Item1)
                    Success(result.Item2);
                else
                    Error(result.Item2);
            }

            return View();
        }
    }    
}