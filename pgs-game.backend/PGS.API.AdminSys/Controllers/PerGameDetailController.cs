using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using example.invalid;
using System;
using game.Entity.Game;
using target.BL.Interface;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.Paging;
using target.Web.Modules.Filter;
using target.Models.Enums;
using target.Models.CoreLibs;

namespace target.API.AdminSys.Controllers
{
    public class PerGameDetailController : BaseController
    {
        public IPerGameDetailService PerGameDetailService { get; set; }

        public IBetRecordService BetRecordService { get; set; }

        /// <summary>
        /// 遊戲歷程紀錄 PageList
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.GamesManagementRecord)]
        [Route("perGame/player-details")]
        [Route("perGame/detail-logs")]
        [ApiConfig(IsHeaderLogEnable = false , IsResponseLogEnable = false)]
        [HttpGet]
        public PagedList<PerGameDetailViewModel> GetByPagingFilter([FromQuery]QueryPerGameDetailLogApiRequestParameterModel filter)
        {
            int userID = GetCurrentUserID();
            var query = PerGameDetailService.GetByPagingFilter(userID , filter);
            return query;
        }

        /// <summary>
        /// PerGame詳細資訊 by transactionNo
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.GamesManagementRecord)]
        [Route("perGame/third-detail/{transactionNo}")]
        [HttpGet]
        public BetRecordGeneralInfoViewModel GetByTransactionNoForPerGamePage(string transactionNo)
        {
            int userID = GetCurrentUserID();
            var query = BetRecordService.GetByTransactionNo_ForPerGamePage(userID, new BetRecordQueryRequestModel
            {
                TransactionNo = transactionNo
            });
            return query;
        }


        //三方遊戲商，產生外部連結、回播連結  是在 BetRecordsController.cs 這個檔案
        //三方遊戲商，產生外部連結、回播連結  是在 BetRecordsController.cs 這個檔案
        //三方遊戲商，產生外部連結、回播連結  是在 BetRecordsController.cs 這個檔案
        //game-vendor/betRecord/by-transaction-no/{transactionNo}/detailInfo


        /// <summary>
        /// 游戏历程查询-详情 by perGameLogID
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.GamesManagementRecord)]
        [Route("perGame/{perGameLogID}/full-info")]
        [HttpGet]
        public PerGameLogFullInformationViewModel GetFullInformationInPerGame(long perGameLogID)
        {
            int userID = GetCurrentUserID();
            var query = PerGameDetailService.GetFullInformationInPerGame(userID, perGameLogID);
            return query;
        }


        /// <summary>
        /// 游戏历程查询-盤看老虎機遊戲盤面圖片(by transactionNo)
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.GamesManagementRecord)]
        [Route("perGame/slot-result/transactionNo/{myTransactionNo}")]
        [HttpGet]
        public PerGameSlotHistoryDetailInfoViewModel GetSlotHistoryImageByTransactionNo(string myTransactionNo)
        {
            int userID = GetCurrentUserID();
            var query = PerGameDetailService.GetSlotHistoryByTransactionNo(userID, myTransactionNo);
            return query;
        }



        /// <summary>
        /// 大筆數資料匯出 (csv格式)
        /// </summary>
        [AdminTrustIPAddressAuthorizeFilter(Level = RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.Statistics_GameVendorFinanceCompareReport)]
        [ApiConfig(IsResponseLogEnable = false)]
        [Route("perGame/large-csv-export-by-vendorConfigID")]
        [HttpGet]
        public IActionResult ExportCsvRawTextByVendorConfigID([FromQuery]LazyCommonRequestModel filter)
        {
            int userID = GetCurrentUserID();
            var csvText = PerGameDetailService.ExportLargeCsvTextByVendorConfigID(userID , filter);
            return Content(csvText);
        }


        /// <summary>
        /// 遊戲歷程標記Tag 
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.GamesManagementRecord)]
        [Route("perGame/tag")]
        [HttpPost]
        public void AddTag([FromBody] PerGameTagAddRequestModel model)
        {
            int userID = GetCurrentUserID();
            PerGameDetailService.AddTag(userID, model);
            return;
        }

        /// <summary>
        /// 遊戲歷程標記Tag (指定多筆單號)
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.GamesManagementRecord)]
        [Route("perGame/batch-add-tags")]
        [HttpPost]
        public CRUDResultModel AddTagByTransactionNoList([FromBody] PerGameTagBatchAddRequestModel model)
        {
            int userID = GetCurrentUserID();
            var result = PerGameDetailService.AddTagByTransactionNoList(userID, model);
            return result;
        }


        /// <summary>
        /// 游戏历程查询-盤看老虎機遊戲盤面圖片
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.GamesManagementRecord)]
        [Route("perGame/slot-result/{perGameDetailLogID}")]
        [HttpGet]
        public PerGameSlotHistoryDetailInfoViewModel GetSlotHistoryFromPerGameLog(long perGameDetailLogID)
        {
            int userID = GetCurrentUserID();
            var query = PerGameDetailService.GetSlotHistoryFromPerGameLog(userID, perGameDetailLogID);
            return query;
        }


    }
}
