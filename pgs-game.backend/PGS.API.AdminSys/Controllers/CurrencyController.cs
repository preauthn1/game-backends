using Microsoft.AspNetCore.Mvc;
using target.BL.Interface;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.ViewModels;
using target.Web.Modules.Filter;

namespace target.API.AdminSys.Controllers
{
    [AdminTokenrAuthorizeFilter] 
    public class CurrencyController : BaseController
    {
        public ICurrencyService CurrencyService { get; set; }


        [Route("currency/all-list")]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.System_AccountingInvoice_Basic)]
        [HttpGet]
        public CurrencyManagementViewModel GetAllCurrencyList()
        {
            int userID = GetCurrentUserID();
            var query = CurrencyService.GetAllCurrencyList(userID);
            return query;
        }


        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.System_AccountingInvoice_Basic)]
        [Route("currency")]
        [HttpPut]
        public CRUDResultModel UpdateCurrency([FromBody] CurrencyUdpateRequestModel model)
        {
            int userID = GetCurrentUserID();
            var result = CurrencyService.Update(userID , model);
            return result;
        }


        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.System_AccountingInvoice_Basic)]
        [Route("currency/update-rate-by-api")]
        [HttpPost]
        public void UpdateCurrencyRateByAPI()
        {
            int userID = GetCurrentUserID();
            CurrencyService.UpdateCurrencyRateByAPI(userID);
            return;
        }


    }
}
