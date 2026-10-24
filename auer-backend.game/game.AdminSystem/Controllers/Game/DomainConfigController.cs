using System.Web.Mvc;
using FW.Core.AdminSys.Provider;
using game.Entity;
using System.Web;

namespace game.AdminSystem.Controllers.Game
{
    public class DomainConfigController : BaseController
    {
        // GET: DomainConfig
        public ActionResult Setting()
        {
            var domainConfig = SiteHelper.GetAPIResult<Entity.AdminSys.DomainConfig>("/AdminSys/GetDomainConfig"); 
            var companyList = DataService.GetAllCompany();
            ViewBag.CompanyList = companyList;

            return View();
        }

        [HttpPost]
        public ActionResult Setting(string payment, string update, string club, string customer, int company)
        {
            payment = HttpUtility.UrlEncode(payment);
            update = HttpUtility.UrlEncode(update);
            club = HttpUtility.UrlEncode(club);
            customer = HttpUtility.UrlEncode(customer);
            var result = SiteHelper.GetAPIResult<APIResult>($"/AdminSys/UpdateDomainConfig?payment={payment}&update={update}&club={club}&customer={customer}&company={company}");

            if (result.Result)
                return ConfirmReturnList("Setting");
            else
                return Alert(result.Message);
        }

        [NoAuthorization]
        public ActionResult GetSettingById(int id)
        {
            var domainConfig = SiteHelper.GetAPIResult<Entity.AdminSys.DomainConfig>($"/AdminSys/GetDomainConfigByID?id={id}");
            return Json(new
            {
                Club = domainConfig.Club,
                Customer = domainConfig.Customer,
                Update = domainConfig.DLUpdate,
                Payment = domainConfig.Payment
            }, JsonRequestBehavior.AllowGet);
        }
    }
}