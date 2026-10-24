using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using target.Models.ViewModels.Base;
using target.Utility.Extensions;
using target.Models;
using target.Models.Models;
using target.Utility.Helper;
using System;
using target.Utility;
using target.Utility.IOC;
using target.BL.Interface;
using target.Models.Enums;
using System.Collections.Generic;
using target.Utility.Extensions.Web;

namespace target.Web.Modules.Filter
{

    public class GameVendorIPAddressAuthorizeFilter : Attribute, IAuthorizationFilter
    {

        public GameVendorTypeEnum GameVendorType { get; set; }



        public GameVendorIPAddressAuthorizeFilter(GameVendorTypeEnum gameVendor)
        {
            this.GameVendorType = gameVendor;
        }


        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (SettingHelper.IsProduction ||
                SettingHelper.IsLocalhostWebSiteName)
            {
                var myIP = IPHelper.GetIP();

                var service = IOCHelper.Resolve<IExternalApiGameVendorCommonService>();
                service.CheckIsValidGameVendorIPAddress(myIP, this.GameVendorType);
            }
        }
    }




}
