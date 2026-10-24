using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using example.invalid;
using System;
using game.Entity.Game;
using target.BL.Interface;
using target.Models.Table;
using target.Models.ViewModels;
using Microsoft.AspNetCore.Cors;
using target.Utility.Extensions;
using example.invalid;
using example.invalid.Http;
using Microsoft.AspNetCore.Mvc;
using target.Web.Modules.Filter;
using target.Utility.Helper;
using target.Models;
using target.Models.Enums;

namespace target.API.Public.Controllers
{
    [Route("v{version:int}")]
    public class W88TransferController : BaseController
    {

        public IMerchantTransferService MerchantTransferService { get; set; }


        /// <summary>
        /// 取得錢包餘額 (平台+pgs錢包)
        /// </summary>
        [Route("platform-transfer/wallet")]
        [SwaggerDocumentAccessTokenRequired]
        [LobbyTokenAuthorizeFilter]
        [HttpGet]
        public MerchantWalletInfoViewModel GetWalletInfo()
        {
            int playerID = GetLobbyTokenPlayerID();
            var query = MerchantTransferService.GetWalletInfo(playerID);
            return query;
        }


        /// <summary>
        /// 轉帳 (平台 > 遊戲錢包)
        /// 基本上就是針對 w88的客製機制
        /// </summary>
        [Route("platform-transfer/platform-to-gamwWallet")]
        [SwaggerDocumentAccessTokenRequired]
        [LobbyTokenAuthorizeFilter]
        [HttpPost]
        public MerchantWalletTransferResultViewModel WalletTransfer_1([FromBody] MerchantWalletTransferRequestModel model)
        {
            int playerID = GetLobbyTokenPlayerID();
            var query = MerchantTransferService.PgsWalletTransferByMerchantApi(playerID , new MerchantWalletTransferRequestModel
            {
                Amount = model.Amount,
                Type = WalletTransferTypeEnum.Transfer_In
            });

            return query;
        }

        /// <summary>
        /// 轉帳 (遊戲錢包 > 平台)
        /// 基本上就是針對 w88的客製機制
        /// </summary>
        [Route("platform-transfer/gamwWallet-to-platform")]
        [SwaggerDocumentAccessTokenRequired]
        [LobbyTokenAuthorizeFilter]
        [HttpPost]
        public MerchantWalletTransferResultViewModel WalletTransfer_2([FromBody] MerchantWalletTransferRequestModel model)
        {
            int playerID = GetLobbyTokenPlayerID();
            var query = MerchantTransferService.PgsWalletTransferByMerchantApi(playerID, new MerchantWalletTransferRequestModel
            {
                Amount = model.Amount,
                Type = WalletTransferTypeEnum.Transfer_Out
            });

            return query;
        }

    }
}
