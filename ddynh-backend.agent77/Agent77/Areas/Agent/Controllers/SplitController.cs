using Agent77.Filters;
using Agent77.Helper;
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
using Repository;

namespace Agent77.Areas.Agent.Controllers
{
    [ModeAuthorizeAttribute(ModeType.Mode3)]
    public class SplitController : BaseController
    {
        private SplitService _service;

        public SplitController()
        {
            _service = new SplitService(this.ModelState);
        }

        public ActionResult Index()
        {
            int? userid = null;
            if (!UserHelper.IsAdmin)
                userid = UserHelper.User.UserID;

            var model = new Split_GetUserSumData();

            var result = this._service.GetUserSumData(UserHelper.AdminID);
            if (result.Item1)
                model = result.Item3;

            return PartialView(model);
        }

        /// <summary>
        /// 用戶列表
        /// </summary>
        /// <returns></returns>
        public ActionResult UserInfoList()
        {
            return View();
        }

        /// <summary>
        /// 用戶列表
        /// </summary>
        /// <param name="draw"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <param name="targetID"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UserInfoList(int draw, int start, int length, int? targetID, DateTime? dtStartTime, DateTime? dtEndTime)
        { 
            var model = new List<Split_GetUserInfo>();

            var result = this._service.GetUserInfo(UserHelper.AdminID, targetID, dtStartTime, dtEndTime);

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
                             NickName = p.NickName,
                             PlatformID = p.PlatformID,
                             PlatformName = p.PlatformName,
                             StageLevel = p.StageLevel,
                             Status = p.Status,
                             TopUserID = p.TopUserID,
                             UpperUserID = p.UpperUserID,
                             CreateTime = p.CreateTime,
                             B_User = p.B_User,
                             C_User = p.C_User,
                             D_User = p.D_User,
                             TotalUser = p.TotalUser
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
        /// 提現紀錄
        /// </summary>
        /// <returns></returns>
        public ActionResult UserConvert()
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
        /// <param name="dtStartTime"></param>
        /// <param name="dtEndTime"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UserConvert(int draw, int start, int length, int? targetID, DateTime? dtStartTime, DateTime? dtEndTime)
        {
            var model = new List<Split_GetUserConvert>();

            var result = this._service.GetUserConvert(UserHelper.AdminID, targetID, dtStartTime, dtEndTime, start / length + 1, length);
           
            if (result.Item1)
                model = result.Item3;
            else
                return Json(new ResultJson());

            int recordsTotal = result.Item2;
            string col_index = Request.Form["order[0][column]"] ?? "0";
            string sortColName = Request.Form[$@"columns[{col_index}][name]"];
            string asc_desc = Request.Form["order[0][dir]"] ?? "desc";

            var query = (from p in model.OrderBy($@"{sortColName} {asc_desc}")
                         select new
                         {
                             RowNum = p.RowNum,
                             UserID = p.UserID,
                             LogDate = p.LogDate,
                             ConvertAmount = p.ConvertAmount.ToCash()
                         }).ToList();

            //if (length != -1)
            //    query = query.Skip(start).Take(length).ToList();

            return Json(new ResultJson
            {
                success = result.Item1,
                message = string.Empty,
                draw = draw,
                recordsTotal = recordsTotal,
                recordsFiltered = recordsTotal,
                data = query
            });
        }

        [CustomAuthorize(new RoleType[] { RoleType.Admin, RoleType.Biz, RoleType.CFO })]
        public ActionResult SystemRate()
        {
            return View();
        }

        [CustomAuthorize(new RoleType[] { RoleType.Admin, RoleType.Biz, RoleType.CFO })]
        [HttpPost]
        public ActionResult SystemRate(int draw, int start, int length)
        {
            var model = new List<PlatformInfo>();

            model = _service.GetPlatformInfo(UserHelper.User.UserID.ToString());

            int recordsTotal = model.Count();
            string col_index = Request.Form["order[0][column]"] ?? "0";
            string sortColName = Request.Form[$@"columns[{col_index}][name]"];
            string asc_desc = Request.Form["order[0][dir]"] ?? "desc";

            var query = model.OrderBy($@"{sortColName} {asc_desc}").ToList();

            if (length != -1)
                query = query.Skip(start).Take(length).ToList();

            return Json(new ResultJson
            {
                success = true,
                message = string.Empty,
                draw = draw,
                recordsTotal = recordsTotal,
                recordsFiltered = recordsTotal,
                data = query
            });
        }

        [CustomAuthorize(new RoleType[] { RoleType.Admin, RoleType.Biz, RoleType.CFO })]
        public ActionResult Edit(int id)
        {

            var result = _service.GetSystemRate(UserHelper.AdminID, id);
            var model = result.Item3.OrderBy(x=> x.Level).ToList();

            if(model.Count < 5)
            {
                for(int i= model.Count; i<=4;i++)
                    model.Add(new Split_GetSystemRate() { Level = i + 1 });
               

                ViewBag.PlatformID = id;
                ViewBag.TaxAmount = 0;
                ViewBag.WithdrawLimit = 0;
            }
            else
            {
                ViewBag.PlatformID = id;
                ViewBag.TaxAmount = result.Item2.TaxAmount;
                ViewBag.WithdrawLimit = result.Item2.WithdrawLimit;
            }            

            return View(model);
        }

        [CustomAuthorize(new RoleType[] { RoleType.Admin, RoleType.Biz, RoleType.CFO })]
        [HttpPost]
        public ActionResult Edit(List<Split_GetSystemRate> model, int platformID, long taxAmount, int withdrawLimit)
        {
            ViewBag.PlatformID = platformID;
            ViewBag.TaxAmount = taxAmount;
            ViewBag.WithdrawLimit = withdrawLimit;

            if (model.Any(x => x.MinRecruit1 >= x.MaxRecruit1) ||
                model.Any(x => x.MinRecruit2 >= x.MaxRecruit2) ||
                model.Any(x => x.MinRecruit3 >= x.MaxRecruit3))
            {
                ModelState.AddModelError("", "最低人数不能超过最高人数");
            }           

            var lv1 = model.First(z => z.Level == 1);
            var lv2 = model.First(z => z.Level == 2);

            for(int i = 1; i <= model.Count; i++)
            {

            }

            foreach(var i in model)
            {
                if (i.Level == 1)
                    continue;

                var lv = model.First(z => z.Level == i.Level-1);

                if (i.MinRecruit1 <= lv.MaxRecruit1 ||
                    i.MinRecruit2 <= lv.MaxRecruit2 ||
                    i.MinRecruit3 <= lv.MaxRecruit3)
                {
                    ModelState.AddModelError("", $"区间{i.Level}最低人数不能超过区间{lv.Level}最高人数");
                }
            }

            if(!ModelState.IsValid)
                return View(model);
            
            string chvLevelString = string.Join("|", model.OrderBy(x => x.Level).Select((x,index) => $"{index * 3 + 1},{x.MinRecruit1},{x.MaxRecruit1},{x.Stage1}|{index * 3 + 2},{x.MinRecruit2},{x.MaxRecruit2},{x.Stage2}|{index * 3 + 3},{x.MinRecruit3},{x.MaxRecruit3},{x.Stage3}"));

            var result = _service.UpdateSystemRate(UserHelper.AdminID, platformID, chvLevelString, taxAmount, withdrawLimit);                      

            if (result.Item1)
            {
                Success(result.Item2);
                return RedirectToAction("SystemRate");
            }
            else
            {
                Error(result.Item2);
                return View(model);
            }
        }

        /// <summary>
        /// 用户列表 取下層用戶資料
        /// </summary>
        /// <param name="draw"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetUserInfo_LevelUsers(int draw, int start, int length, int userID, int level)
        { 
            var model = new List<Split_GetUserInfo_LevelUsers>();

            var result = this._service.GetUserInfo_LevelUsers(userID, level);

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
                             CreateTime = p.CreateTime
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
    }
}