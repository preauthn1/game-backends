using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Agent77.Filters;
using Agent77.Helper;
using Repository.Models;
using Repository.ViewModel;
using Service;
using Service.Helper;

namespace Agent77.Areas.Agent.Controllers
{
    [ModeAuthorizeAttribute(ModeType.Mode1)]
    public class CouponController : BaseController
    {

        // GET: Agent/Coupon/DailyReport
        public ActionResult DailyReport()
        {

            var service = new CouponService();
            var result = service.Exec_PrWeb_Coupon_Stat_CouponData_Get(UserHelper.AdminID, null, null);
            var model = result.Item2;
            if (!result.Item1)
            {
                Error("ERROR");
                return RedirectToAction("SubUserInfo");
            }
            return View(model);
        }

        // GET: Agent/Coupon/SearchUser
        public ActionResult SearchUser()
        {
            return View();
        }
        /// <summary>
        /// 我的代理
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SearchUser(int draw, int start, int length, int? user_id)
        {
            if (user_id == null)
            {
                return Json(new ResultJson());
            }
            var model = new List<Coupon_SearchUser>();
            var service = new CouponService();
            var result = service.Exec_PrWeb_Coupon_CouponDataByUserID_Get(UserHelper.AdminID, user_id);
            model = result.Item2;
            var recordsTotal = result.Item2.Count();
            var query = (from p in model
                         select new
                         {
                             UserID = p.UserID,
                             CouponCount = p.CouponCount,
                             CouponAmount = p.CouponAmount.ToCash(),
                             UsedCouponAmount = p.UsedCouponAmount.ToCash(),
                             LastLogon = p.LastLogon
                         }).ToList();
            return Json(new ResultJson
            {
                success = result.Item1,
                message = string.Empty,
                draw = draw,
                recordsTotal = recordsTotal,
                recordsFiltered = recordsTotal,
                data = new { data = query, total = recordsTotal }
            });
        }

        // GET: Agent/Coupon/CouponDetail
        public ActionResult CouponDetail()
        {
            return View();
        }
        /// <summary>
        /// 我的代理
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CouponDetail(int draw, int start, int length, string coupon_code)
        {
            if (coupon_code == null)
            {
                return Json(new ResultJson());
            }
            var model = new List<Coupon_CouponDetail>();
            var service = new CouponService();
            var result = service.Exec_PrWeb_Coupon_CouponDataByCode_Get(UserHelper.AdminID, coupon_code);
            model = result.Item2;
            var recordsTotal = result.Item2.Count();
            var query = (from p in model
                         select new
                         {
                             CouponCode = p.CouponCode,
                             UserID = p.UserID,
                             Amount = p.Amount.ToCash(),
                             UseDate = p.UseDate
                         }).ToList();
            return Json(new ResultJson
            {
                success = result.Item1,
                message = string.Empty,
                draw = draw,
                recordsTotal = recordsTotal,
                recordsFiltered = recordsTotal,
                data = new { data = query, total = recordsTotal }
            });
        }

        // GET: Agent/Coupon/CouponGenerator
        public ActionResult CouponGenerator()
        {
            var service = new CouponService();
            ViewBag.PlatformID = service.GetPlatformSelectList(null);
            return View();
        }
        /// <summary>
        /// 我的代理
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CouponGenerator(int draw, int start, int length, long? amount, int? counts, int? platform_id)
        {
            if (platform_id == null)
            {
                platform_id = 0;
            }
            if (amount == null || counts == null)
            {
                return Json(new ResultJson());
            }
            amount = amount * 100; //轉換成分
            var service = new CouponService();
            var result = service.Exec_PrWeb_Coupon_CreateCouponQuantity(UserHelper.AdminID, platform_id, amount, counts);
            var model = result.Item2;
            var recordsTotal = result.Item2.Count();
            var query = (from p in model
                         select new
                         {
                             CouponCode = p.CouponCode,
                             Amount = p.Amount.ToCash()
                         }).ToList();
            return Json(new ResultJson
            {
                success = result.Item1,
                message = string.Empty,
                draw = draw,
                recordsTotal = recordsTotal,
                recordsFiltered = recordsTotal,
                data = new { data = query, total = recordsTotal }
            });
        }
    }
}