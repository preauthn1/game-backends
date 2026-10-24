using Microsoft.AspNetCore.Mvc;
using target.Models.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using target.Utility.Extensions;
using target.Utility.Helper.Dependency;
using target.Models.Table;
using target.Utility;
using target.Utility.IOC;
using target.BL.Interface;
using target.Models.Models;
using target.Models;
using target.Utility.Helper;
using target.DA.Repository;
using target.Models.Enums;

namespace target.Web.Modules.Filter
{
    public class AdminSysAuthorizeHelper
    {

        public static AdminUserTokenJwtModel GetValidUserSessionByJwt()
        {
            var service = IOCHelper.Resolve<IUserInfoService>();

            string jwtToken = [REDACTED];

            var parseResult = service.ParseAndCheckIsValidJwtToken(jwtToken);

            return parseResult;
        }

    }
}
