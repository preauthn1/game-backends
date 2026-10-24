using Repository;
using Repository.Models;
using Repository.ViewModel;
using Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Agent77.Helper;
using Service.Helper;
using static Repository.ViewModel.UserLoginInfo;
using Agent77.Filters;
using System.Linq.Dynamic;
using game.Infrastructure;

namespace Agent77.Areas.Agent.Controllers
{
    [ModeAuthorizeAttribute(ModeType.Mode1)]
    public class CustomerController : BaseController
    {
        private CustomerService _service;

        public CustomerController()
        {
            _service = new CustomerService(this.ModelState);
        }

        /// <summary>
        /// 非Admin的UserID
        /// </summary>
        private int? GetLoginUserID()
        {
            int? userid = null;

            if (!UserHelper.IsAdmin)
                userid = UserHelper.User.UserID;

            return userid;
        }

        public ActionResult Index()
        {
            int? userid = GetLoginUserID();

            var model = new UserAgent_GetAgentExecInfo();

            var result = this._service.GetAgentExecInfo(UserHelper.AdminID, userid);

            if (result.IsSuccess)                
                model = result.ResultData;

            return PartialView(model);
        }

        /// <summary>
        /// 我的代理
        /// </summary>
        /// <returns></returns>
        public ActionResult SubUserInfo()
        {
            return View();
        }

        /// <summary>
        /// 我的代理
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SubUserInfo(int draw, int start, int length, int? subUserLeve, int? subUserID, DateTime? dtStartTime, DateTime? dtEndTime)
        {
            int? userid = GetLoginUserID();

            var model = new List<UserAgent_SubUserInfo>();

            var result = this._service.GetSubUserInfo(UserHelper.AdminID, userid, 0, subUserID, dtStartTime, dtEndTime);

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
                             UserLevel = ((UserLevel)p.UserLevel).ToDescription(),
                             SumTaxTotal = p.SumTaxTotal.ToCash(),
                             SumTaxBonus = p.SumTaxBonus.ToCash(),
                             SumTaxConvert = p.SumTaxConvert.ToCash(),
                             SumTaxPerformed = p.SumTaxPerformed.ToCash(),
                             UnWithDrawTotal = p.UnWithDrawTotal.ToCash(),
                             CreateTime = p.CreateTime,
                             Status = ((Status)p.Status).ToDescription(),
                             TopUserID = p.TopUserID,
                             NickName = p.NickName
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
        /// 我的代理Excel
        /// </summary>
        /// <param name="subUserID">查詢玩家ID</param>
        /// <param name="dtStartTime">查詢起始時間</param>
        /// <param name="dtEndTime">查詢結束時間</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult SubUserInfoExcel(int? subUserID, DateTime? dtStartTime, DateTime? dtEndTime)
        {
            int? userid = GetLoginUserID();

            var model = new List<UserAgent_SubUserInfo>();

            var result = this._service.GetSubUserInfo(UserHelper.AdminID, userid, 0, subUserID, dtStartTime, dtEndTime);

            if (result.Item1)
                model = result.Item3;

            string fileName = "SubUserInfo.xlsx";
            var fileStream = SimpleExcelHelper.ConvertModelToExcelFileStream(model);
            return File(fileStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        /// <summary>
        /// 分成比例设定
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Edit(int id)
        {      
            var result = this._service.GetUserRate(id);

            if (!result.Item1)
            {
                Error(result.Item2);
                return RedirectToAction("SubUserInfo");
            }

            return View(result.Item3);
        }

        /// <summary>
        /// 分成比例设定
        /// </summary>
        /// <param name="model"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Edit(List<UserAgent_UserRate> model, int id)
        {
            string chvLevelString = string.Join("|", model.OrderBy(x => x.UserLevel).Select(x => $"{x.UserLevel},{ x.Stage1 },{ x.Stage2 },{ x.Stage3 }"));

            var result = this._service.UpdateUserRate(id, chvLevelString);

            if (result.Item1)
            {
                Success(result.Item2);
                return RedirectToAction("SubUserInfo"); 
            }
            else
            {
                Error(result.Item2);
                return View(model);
            }   
        }

        /// <summary>
        /// 按日税收记录
        /// </summary>
        /// <returns></returns>
        public ActionResult ProfitlogDay()
        {
            return View();
        }

        /// <summary>
        /// 按日税收记录
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ProfitlogDay(int draw, int start, int length, int? subUserID, DateTime? dtStartTime, DateTime? dtEndTime)
        {
            int? userid = GetLoginUserID();

            var model = new List<UserAgent_GetAgentTaxLog>();

            var result = this._service.GetProfitlogDay(UserHelper.AdminID, userid, subUserID, dtStartTime, dtEndTime);

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
                             LogDate = p.LogDate,
                             UserID = p.UserID,
                             Amount = p.Amount.ToCash()
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
        /// 按日税收记录EXCEL
        /// </summary>
        /// <param name="subUserID">查詢玩家ID</param>
        /// <param name="dtStartTime">查詢起始時間</param>
        /// <param name="dtEndTime">查詢結束時間</param>
        [HttpGet]
        public ActionResult ProfitlogDayExportExcel(int? subUserID, DateTime? dtStartTime, DateTime? dtEndTime)
        {
            int? userid = GetLoginUserID();

            var model = new List<UserAgent_GetAgentTaxLog>();

            var result = this._service.GetProfitlogDay(UserHelper.AdminID, userid, subUserID, dtStartTime, dtEndTime);

            if (result.Item1)
                model = result.Item3;

            string fileName = "ProfitlogDay.xlsx";
            var fileStream = SimpleExcelHelper.ConvertModelToExcelFileStream(model);
            return File(fileStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        /// <summary>
        /// 按日收益记录
        /// </summary>
        /// <returns></returns>
        public ActionResult TaxBonusLog()
        {
            return View();
        }

        /// <summary>
        /// 按日收益记录
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult TaxBonusLog(int draw, int start, int length, int? subUserID, DateTime? dtStartTime, DateTime? dtEndTime)
        {
            int? userid = GetLoginUserID();

            var model = new List<UserAgent_GetAgentTaxBonusLog>();

            var result = this._service.GetTaxBonusLog(UserHelper.AdminID, userid, subUserID, dtStartTime, dtEndTime);

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
                             LogDate = p.LogDate,
                             UserID = p.UserID,
                             Amount = p.Amount.ToCash()
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
        /// 按日收益记录EXCEL
        /// </summary>
        /// <param name="subUserID">查詢玩家ID</param>
        /// <param name="dtStartTime">查詢起始時間</param>
        /// <param name="dtEndTime">查詢結束時間</param>
        [HttpGet]
        public ActionResult TaxBonusLogExcel(int? subUserID, DateTime? dtStartTime, DateTime? dtEndTime)
        {
            int? userid = GetLoginUserID();

            var model = new List<UserAgent_GetAgentTaxBonusLog>();

            var result = this._service.GetTaxBonusLog(UserHelper.AdminID, userid, subUserID, dtStartTime, dtEndTime);

            if (result.Item1)
                model = result.Item3;

            string fileName = "TaxBonusLog.xlsx";
            var fileStream = SimpleExcelHelper.ConvertModelToExcelFileStream(model);
            return File(fileStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        /// <summary>
        /// 提取收益记录
        /// </summary>
        /// <returns></returns>
        public ActionResult WithdrawList()
        {
            return View();
        }

        /// <summary>
        /// 提取收益记录
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult WithdrawList(int draw, int start, int length, int? subUserID, DateTime? dtStartTime, DateTime? dtEndTime)
        {
            int? userid = GetLoginUserID();

            var model = new List<UserAgent_GetWithdrawList>();
            var modelTotal = new UserAgent_GetWithdrawListTotal();

            var result = this._service.GetWithdrawList(UserHelper.AdminID, userid, subUserID, dtStartTime, dtEndTime);

            if (result.Item1)
            {
                model = result.Item3;
                modelTotal = result.Item2;
            }
            else
                return Json(new ResultJson());

            int recordsTotal = model.Count();
            string col_index = Request.Form["order[0][column]"] ?? "0";
            string sortColName = Request.Form[$@"columns[{col_index}][name]"];
            string asc_desc = Request.Form["order[0][dir]"] ?? "desc";

            var query = (from p in model.OrderBy($@"{sortColName} {asc_desc}")
                         select new
                         {
                             LogDate = p.LogDate,
                             WithdrawAmount = p.WithdrawAmount.ToCash(),
                             Status = p.Status
                         }).ToList();

            if (length != -1)
                query = query.Skip(start).Take(length).ToList();

            return Json(new ResultJson
            {
                success = result.Item1,
                message = string.Empty,
                draw = draw,
                recordsTotal = recordsTotal,
                recordsFiltered = recordsTotal,
                data = new {
                    data = query,
                    total = modelTotal
                }
            });           
        }

        /// <summary>
        /// 提取收益记录Excel
        /// </summary>
        /// <param name="subUserID">查詢玩家ID</param>
        /// <param name="dtStartTime">查詢起始時間</param>
        /// <param name="dtEndTime">查詢結束時間</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult WithdrawListExcel(int? subUserID, DateTime? dtStartTime, DateTime? dtEndTime)
        {
            int? userid = GetLoginUserID();

            var model = new List<UserAgent_GetWithdrawList>();

            var result = this._service.GetWithdrawList(UserHelper.AdminID, userid, subUserID, dtStartTime, dtEndTime);

            if (result.Item1)
            {
                model = result.Item3;
            }

            string fileName = "WithdrawList.xlsx";
            var fileStream = SimpleExcelHelper.ConvertModelToExcelFileStream(model);
            return File(fileStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}