using game.Entity.Pay;
using game.Infrastructure;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace game.WebSite.Controllers
{
    public class IAPController : Controller
    {

        string filelog = "applepay.log";
        int paySourceID = (int)PaySourceType.IOS;
        // GET: IAP
        //public ActionResult Index()
        //{
        //    return View();
        //}


        //public ContentResult Applepay()
        //{
        //    var receipt = Request.Form.AllKeys.Contains("receipt") ? Request.Form["receipt"] : Request.QueryString["receipt"];
        //    var info = Request.Form.AllKeys.Contains("info") ? Request.Form["info"] : Request.QueryString["info"];
        //    var item = Request.Form.AllKeys.Contains("item") ? Request.Form["item"] : Request.QueryString["item"];

        //    SiteHelper.Log(Environment.NewLine + string.Format("receipt:{0},info:{1},item:{2}", receipt, info, item), "ios.log");

        //    int userID = 0;
        //    try
        //    {
        //        userID = Convert.ToInt32(info.Split('A')[0]);
        //    }
        //    catch
        //    {
        //        return Content(Lua.JsonToLua(Newtonsoft.Json.JsonConvert.SerializeObject(new
        //        {
        //            data = item
        //        })));
        //    }

        //    //https://example.invalid/verifyReceipt   	测试请求苹果地址
        //    //https://example.invalid/verifyReceipt   		正式请求苹果地址
        //    if (verifyReceipt("https://example.invalid/verifyReceipt", receipt, userID) == false)
        //    {
        //        verifyReceipt("https://example.invalid/verifyReceipt", receipt, userID);
        //    }

        //    return Content(Lua.JsonToLua(Newtonsoft.Json.JsonConvert.SerializeObject(new
        //    {
        //        data = item
        //    })));
        //}


        public ContentResult Pay(int userID, int amount, string memo)
        {

            bool IAPEnable = true;
            try
            {
                string setting = ConfigurationManager.AppSettings["IAPEnable"].ToString();

                if (setting == "0")
                    IAPEnable = false;
                else
                    IAPEnable = true;

            }
            catch
            {
                IAPEnable = true;
            }

            if (!IAPEnable) return Content("IAP_Disable");


            var api = SiteHelper.GetAPIResult<APIResult>(string.Format("/Money/UpdateUserCash?userID={0}&amount={1}&sourceType={2}&ipAddress={3}&memo={4}",
            userID,
            amount * 100,
            paySourceID,
            Server.UrlEncode(game.Infrastructure.SystemHelper.UserIPAddress),
            Server.UrlEncode(memo)));

            SiteHelper.Log(Environment.NewLine +
                String.Format("{0} ios充值 result={1}, message={2}, userid={3}, amount={4}, memo={5}",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                    api.Result.ToString(),
                    api.Message,
                    userID, amount, memo), filelog);

            return Content("finish");
        }
    }
}

