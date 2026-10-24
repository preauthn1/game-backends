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
using target.Models.Models;
using target.Models;
using target.Utility.Helper;
using System.Reflection;
using target.Web.Modules.Filter;
using target.Utility.Utility.ThirdParty;
using target.Models.Enums;

namespace target.Utility.Extensions.Web
{

    public class GameVendorExtraLogicHelper
    {
        public static bool IsExecuteBonusSettleProcess(int vendorID, decimal betAmount)
        {
            return IsExecuteBonusSettleProcess((GameVendorTypeEnum)vendorID, betAmount);
        }


        public static bool IsExecuteBonusSettleProcess(GameVendorTypeEnum vendorID, decimal betAmount)
        {
            if (betAmount > 0)
                return false;

            var vendorDef = GameVendorDefinitionHelper.GetMatchGameVendorDefineSetting(vendorID);

            if (betAmount == 0 && !vendorDef.IsSupportZeroBetAmountAndStillCallbackPlaceBet)
            {
                return true;
            }

            return false;
        }

    }

}