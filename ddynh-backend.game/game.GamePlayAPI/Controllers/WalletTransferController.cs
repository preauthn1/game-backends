using FW.Core.Helper;
using game.Entity;
using game.Entity.Event;
using game.Entity.Game;
using game.Entity.Users;
using game.Infrastructure;
using game.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using game.Infrastructure.Extensions;
using game.Entity.ViewModels;
using FW.Core.Engine;

namespace game.GamePlayAPI.Controllers
{
    public class WalletTransferController : BaseController
    {
        public IWalletTransferService WalletTransferService { get; set; }


        [Route("wallet/auto-transfer")]
        [HttpPost]
        public WalletTransferViewModel AutoWalletTransfer([FromBody]WalletTransferRequestViewModel model)
        {
            var result = WalletTransferService.AutoWalletTransfer(model);
            return result;
        }

    }
}