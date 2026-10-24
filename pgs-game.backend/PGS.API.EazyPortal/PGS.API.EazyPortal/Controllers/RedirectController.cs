using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using example.invalid;
using System;
using Microsoft.AspNetCore.Mvc;
using target.Utility.Utility.ThirdParty;

namespace target.API.EazyPortal.Controllers
{
    public class RedirectController : ControllerBase
    {
        private static Dictionary<string, int> Counter = new Dictionary<string, int>();

        private static HashSet<string> AlreadyNotifyKeys = new HashSet<string>();

        private readonly object myLock = new object();

        private const string _nullText = null;



        [Route("r-portal/{type}/{page}")]
        [HttpGet]
        public object RedirectToTargetPageForSOC(int type, string page)
        {
            AddCounter(type);
            return GenerateFakeResult(page);
        }


        [Route("r-portal/view-counter")]
        [HttpGet]
        public object GetCurrentCounter()
        {
            return new
            {
                MyCounter = Counter
            };
        }


        [Route("r-portal/reset-counter")]
        [HttpGet]
        public object ResetCounter()
        {
            Counter = new Dictionary<string, int>();
            return new
            {
                ok = true
            };
        }


        private string GenerateCategoryKey(int type, DateTime time)
        {
            return $"{type.ToString()}_{time.ToString("yyyyMMddHH")}";
        }


        private void NotifyToChannel(int type, string categoryKey)
        {
            if (AlreadyNotifyKeys.Contains(categoryKey))
                return;

            string nextHourCategoryKey = GenerateCategoryKey(type, DateTime.Now.AddHours(1));

            AlreadyNotifyKeys.Add(categoryKey);
            AlreadyNotifyKeys.Add(nextHourCategoryKey);

            var time = DateTime.UtcNow.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss");
            string msg = $"時間:{time} API次數異常";
            MyTelegramUtility.SendMessage(new TelegramMessageParameter
            {
                Message = msg,
                TelegramChannelEnum = TelegramChannelEnum.pgs_important_error,
            });
        }


        private void AddCounter(int type)
        {
            string categoryKey = $"{type.ToString()}_{DateTime.Now.ToString("yyyyMMddHH")}";

            if (Counter.ContainsKey(categoryKey))
            {
                Counter[categoryKey]++;

                if (Counter[categoryKey] > 1 && !AlreadyNotifyKeys.Contains(categoryKey))
                {
                    lock (myLock)
                    {


                    }
                }
            }
            else
            {
                try
                {
                    Counter.Add(categoryKey, 1);
                }
                catch
                {

                }
            }
        }


        private ActionResult GenerateFakeResult(string page)
        {
            //因為不想暴露中轉網址。還是用redirect的方式好了
            if (page == "400")
            {
                return Redirect("https://example.invalid/static-file/error400.json");
            }
            else
            {
                return Redirect("https://example.invalid/static-file/mylobby.json");
            }
        }

        //private static object FakeResponse_HomePage = new
        //{
        //    code = 0,
        //    message = _nullText,
        //    data = new
        //    {
        //        environmentName = "production",
        //        webSiteName = "target.API.Public.Production",
        //    }
        //};


        //private static object FakeResponse_401Error = new
        //{
        //    code = 401002,
        //    message = "Your login information has expired, please try to log in again. *",
        //    data = _nullText
        //};




    }
}
