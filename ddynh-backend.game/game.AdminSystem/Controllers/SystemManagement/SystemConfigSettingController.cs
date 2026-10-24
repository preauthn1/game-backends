using System;
using System.Collections.Generic;
using System.Web.Mvc;
using game.Entity.AdminSys;
using game.Entity.Money;
using game.Infrastructure.Extensions;
using game.Entity.Stat;
using game.Entity.Area;
using System.Linq;
using game.Entity;
using FW.Core.Helper;
using game.Infrastructure;
using game.AdminSystem.Models.ViewModels;
using game.Entity.Event;
using System.Web;
using example.invalid;
using game.Infrastructure.Utility.ThirdParty;
using game.Entity.Game;
using game.Service;
using game.Infrastructure.Helper;
using game.Service.DAO;
using game.Entity.SQL;

namespace game.AdminSystem.Controllers.SystemManagement
{
    public class SystemConfigSettingController : BaseController
    {

        public MSSQLSystemRepository MSSQLSystemRepository { get; set; }

        
        public ActionResult List()
        {
            List<QuerySystemJobResultModel> queryAll = MSSQLSystemRepository.GetAllJobs();
            queryAll = queryAll.Where(x => x.Name.Contains("自動充值成功")).ToList();

            ViewBag.QueryJobs = queryAll;
            return View();
        }


        [HttpPost]
        public ActionResult UpdateJobStatus(string target_job_id)
        {
            List<QuerySystemJobResultModel> queryAll = MSSQLSystemRepository.GetAllJobs();

            var targetJob = queryAll.FirstOrDefault(x => x.Job_ID.ToString() == target_job_id);

            bool afterValue = !targetJob.IsEnable;

            MSSQLSystemRepository.ChangeJobEnableFlag(targetJob.Name, afterValue);

            return Json(new CRUDResultModel
            {
                IsSuccess = true,
                Message = "更新成功",

            }, JsonRequestBehavior.AllowGet);
        }

    }
}