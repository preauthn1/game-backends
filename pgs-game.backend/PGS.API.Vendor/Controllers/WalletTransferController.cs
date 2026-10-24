using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using example.invalid;
using System;
using game.Entity.Game;
using target.BL.Interface;
using target.Models.Table;
using target.Models.ViewModels;
using target.API.Vendor.Controllers;
using target.API.AdminSys;
using target.Web.Modules.Filter;

namespace target.API.Vendor.Controllers
{
    [Route("v{version:int}")]
    public class WalletTransferController : BaseController
    {
        public IWalletCashService WalletCashService { get; set; }

        public ITransferRecordVendorService TransferRecordVendorService { get; set; }

        private const string accountRequiredHintMessageEN = "The player's account should `exist` when access this API (call API 2.2.2)";
        private const string accountRequiredHintMessageCN = "呼叫此API前，须先`建立玩家账号` (至少调用一次游戏启动、或调用建立账号API)";


        private const string transferDenyHintMessageEN = "<BR> Transfer is `not allowed` while the player is playing in the game-room";
        private const string transferDenyHintMessageCN = "<BR> 玩家进行游戏过程中，`不允许钱包转帐`";


        private const string queryRecordHintMessageEN = "<BR> If the api response timeout or response error.please use `API 2.4.4` to query the transfer-record exist or not.";
        private const string queryRecordHintMessageCN = "<BR> 若此API逾时或未得到正确回应，建议使用 `2.4.4 API` 查询转帐纪录是否成功(或使用后台查询)，以利运营商进行退还金币等动作";



        /// <summary>
        /// 2.4.1 Get player wallet (取得玩家钱包余额)
        /// </summary>
        [SwaggerApiDescriptionEN("Get player wallet balance. <BR>" + accountRequiredHintMessageEN)]
        [SwaggerApiDescriptionCN("取得玩家錢包金额。<BR> " + accountRequiredHintMessageCN )]
        [Route("wallet/amount")]
        [ApiExplorerSettings(GroupName = "tfMode")]
        [HttpGet]
        public WalletCashPlayerAmountViewModel GetPlayerWalletCash([FromQuery]WalletCashPlayerAmountRequestModel model)
        {
            var query = WalletCashService.GetPlayerWalletCash(model);
            return query;
        }


        /// <summary>
        /// 2.4.2 Wallet transfer from platform to gameWallet (玩家钱包转账)
        /// </summary>
        [SwaggerApiDescriptionEN(accountRequiredHintMessageEN + transferDenyHintMessageEN + queryRecordHintMessageEN)]
        [SwaggerApiDescriptionCN(accountRequiredHintMessageCN + transferDenyHintMessageCN + queryRecordHintMessageCN)]
        [Route("wallet/transfer-in")]
        [ApiExplorerSettings(GroupName = "tfMode")]
        [HttpPost]
        public WalletTransferResultViewModel TransferCashToGameWallet(WalletTransferRequestModel model)
        {
            var query = WalletCashService.TransferCashToGameWallet(model);
            return query;
        }


        /// <summary>
        /// 2.4.3 Wallet transfer from gameWallet to platform (玩家钱包转账)
        /// </summary>
        [SwaggerApiDescriptionEN(accountRequiredHintMessageEN + transferDenyHintMessageEN  + queryRecordHintMessageEN)]
        [SwaggerApiDescriptionCN(accountRequiredHintMessageCN + transferDenyHintMessageCN + queryRecordHintMessageCN)]
        [Route("wallet/transfer-out")]
        [ApiExplorerSettings(GroupName = "tfMode")]
        [HttpPost]
        public WalletTransferResultViewModel TransferCashToPlatformWallet(WalletTransferRequestModel model)
        {
            var query = WalletCashService.TransferCashToPlatformWallet(model);
            return query;
        }


        /// <summary>
        /// 2.4.4 Query transfer record (查询转账记录)
        /// </summary>
        [SwaggerApiDescriptionEN("Get transfer record by transactionID. (if data not exist. api will response `null`). Database may be busy with data writes and synchronization delays. There may be 30-second delay in query result.")]
        [SwaggerApiDescriptionCN("根据转账单号取得轉帳紀錄。若查无资料返回data: null，数据库可能因忙碌发生数据写入、同步延迟。查询结果可能会有30秒延迟。。 ")]
        [Route("transfer")]
        [ApiExplorerSettings(GroupName = "tfMode")]
        [HttpGet]
        public TransferRecordVendorViewModel GetTransferByMerchant([FromQuery] QueryTransferRecordRequestModel filter)
        {
            var query = TransferRecordVendorService.GetTransferByMerchant(filter);
            return query;
        }



        /// <summary>
        /// 4.7.9 Get fishing transfer record (查询特殊捕鱼游戏的转账记录-僅無縫錢包模式支援此)
        /// </summary>
        [SwaggerApiDescriptionEN("Get transfer record by transactionID. (if data not exist. api will response `null`). Database may be busy with data writes and synchronization delays. There may be 30 seconds delay in query result.")]
        [SwaggerApiDescriptionCN("根据转账单号取得轉帳紀錄。若查无资料返回data: null，数据库可能因忙碌发生数据写入、同步延迟。查询结果可能会有30秒延迟。。 ")]
        [Route("fishing-transfer-record")]
        [ApiExplorerSettings(GroupName = "swMode" , IgnoreApi = true)]
        [HttpGet]
        public FishingTransferRecordVendorViewModel GetFishingTransferByMerchant([FromQuery] QueryFishingTransferRecordRequestModel filter)
        {
            var query = TransferRecordVendorService.GetFishingTransferByMerchant(filter);
            return query;
        }


    }
}
