using Agent77.Filters;
using Newtonsoft.Json;
using Repository.ViewModel;
using Service;
using Service.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using example.invalid;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using static Repository.ViewModel.UserLoginInfo;
using System.Linq.Dynamic;
using Repository.Models;
using game.Service.DAO;
using example.invalid;
using game.Infrastructure;

namespace Agent77.Areas.Agent.Controllers
{
    public class TestCaseController : BaseController
    {



        /// <summary>
        /// excel範例
        /// </summary>
        public ActionResult ExportExcel()
        {

            List<ExcelSampleClass> list = new List<ExcelSampleClass>();

            list.Add(new ExcelSampleClass
            {
                UserID = 1,
                UserName = "account001",
                MobilePhone = "0951222333"
            });

            list.Add(new ExcelSampleClass
            {
                UserID = 2,
                UserName = "admin222",
                MobilePhone = "09514411533"
            });

            list.Add(new ExcelSampleClass
            {
                UserID = 3,
                UserName = "rory333",
                MobilePhone = "0953984152"
            });


            string fileName = "ExportRanger.xlsx";
            var fileStream = SimpleExcelHelper.ConvertModelToExcelFileStream(list);            
            return File(fileStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

    }


    public class ExcelSampleClass
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string MobilePhone { get; set; }
    }


}