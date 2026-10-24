using Service.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using game.Infrastructure.Extensions;


namespace Agent77.Areas.Agent.Controllers
{
    public class BaseController : Controller
    {      

        public BaseController()
        {
          
        }


        protected bool CheckAgentHasPlatformPermission(int platformID)
        {
            if (UserHelper.PlatformArea.IsEmptyOrNull())
                return false;

            return UserHelper.PlatformArea.Any(z => z.PlatformID == platformID);
        }


        protected bool CheckAgentHasAreaPermission(int areaId)
        {
            if (UserHelper.PlatformArea.IsEmptyOrNull())
                return false;

            return UserHelper.PlatformArea.Any(z => z.AreaID == areaId);
        }






        public void Attention(string message)
        {
            TempData.Add(Alerts.ATTENTION, message);
        }

        public void Success(string message)
        {
            TempData.Add(Alerts.SUCCESS, message);
        }

        public void Information(string message)
        {
            TempData.Add(Alerts.INFORMATION, message);
        }

        public void Error(string message)
        {
            TempData.Add(Alerts.ERROR, message);
        }
    }


    public static class Alerts
    {
        public const string SUCCESS = "success";
        public const string ATTENTION = "attention";
        public const string ERROR = "error";
        public const string INFORMATION = "info";

        public static string[] ALL
        {
            get
            {
                return new[]
              {
                SUCCESS,
                ATTENTION,
                INFORMATION,
                ERROR
            };
            }
        }
    }
}