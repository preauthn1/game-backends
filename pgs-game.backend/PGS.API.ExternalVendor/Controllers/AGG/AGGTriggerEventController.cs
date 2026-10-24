using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using example.invalid;
using System;
using game.Entity.Game;
using target.BL.Interface;
using target.Models.Table;
using target.Models.ViewModels;
using Microsoft.AspNetCore.Cors;
using target.Web.Modules.Filter;
using target.BL.Services;
using target.Utility.Extensions;
using target.Models.Enums;
using target.Models.ViewModels.GameVendor;
using System.Linq;
using target.Models.Models;
using target.Utility.Helper;
using target.Utility.IOC;

namespace target.API.ExternalVendor.Controllers
{
    public partial class AGGBetController : BaseGameVendorController
    {

        [GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        [ApiConfig(ApiCategory = ApiConfigCategoryEnum.SingleWalletApi_AGGApi , IsWriteToOtherLogTable = true)]
        [Route("agg/event/gameSession")]
        [HttpPost]
        public string RoomEnterEvent([FromBody] AGGGameSessionRequestModel model)
        {
            if (model.EventType == "Game_Slot_Join")
            {

            }
            else if (model.EventType == "Game_Slot_Leave")
            {

                var specialCaseService = IOCHelper.Resolve<IGameVendorSpecialCaseService>();

                int playerID = Int32.Parse(model.UserID);
                specialCaseService.ForcePendingRecordTriggerSettle(playerID);
            }

            return "OK";
        }

    }
}
