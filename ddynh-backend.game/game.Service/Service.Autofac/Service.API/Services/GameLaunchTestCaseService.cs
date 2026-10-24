using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity.Game;
using game.Service.DAO;
using game.Infrastructure;
using game.Entity.ViewModels;
using game.Infrastructure.Extensions;
using game.Infrastructure.Helper;
using game.Entity;
using game.Service.DAO;
using game.Entity.ViewModels.Query;
using game.Entity.Money;

namespace game.Service.Service
{
    public partial class GameLaunchService : BaseService, IGameLaunchService
    {
        public IAPLGameMerchantApiService APLGameMerchantApiService { get; set; }

        public IEGamingGameMerchantApiService EGamingGameMerchantApiService { get; set; }

        public IBGGameMerchantApiService BGGameMerchantApiService { get; set; }

        
        public object TestCaseTestCase(GameLaunchRequestViewModel model)
        {
            model.RuleFor(x => x.UserID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.GameID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.Certificate).Rule_NotEmptyText();
            model.RuleFor(x => x.IPAddress).Rule_NotEmptyText();



            var queryUser = UserInfoRepository.GetById(model.UserID);

            if (queryUser == null)
                throw new TerproException(ApiErrorCode.NotFoundUser);

            int platformID = AreaRepository.GetSingleByCache(queryUser.AreaID).PlatformID;

            string orderNo = RandomHelper.GetOrderNo("TRI");


            var qqq = APLGameMerchantApiService.GetFishBetRecords(new APLGameMerchantApiQueryBetRecordRequestModel
            {
                PlatformID = platformID,
                BeginTimeUTC = new DateTime(2020 , 4 , 24),
                EndTimeUTC = new DateTime(2020, 4, 25),
            });


            return APLGameMerchantApiService.GetGameLaunchResult(new ThirdPartyGameApiLaunchRequestModel
            {
                PlatformID = platformID,
                UserID = model.UserID,
                GameCode = "FH",
                IPAddress = model.IPAddress
            });


            //var qqq = BGGameMerchantApiService.GetEGameLaunchUrl(new ThirdPartyGameApiLaunchRequestModel
            //{
            //    PlatformID = platformID,
            //    UserID = model.UserID
            //});



            //產生代理代理代理代理代理代理代理代理代理

            //BGGameMerchantApiService.CreateAgentAccount(new ThirdPartyGameApiAccountCreateResultModel
            //{
            //    PlatformID = platformID,
            //    UserID = model.UserID
            //});




            //var qeweewre = BGGameMerchantApiService.GetGameLaunchResult(new ThirdPartyGameApiLaunchRequestModel
            //{
            //    IPAddress = model.IPAddress,
            //    PlatformID = platformID,
            //    UserID = model.UserID,                
            //});


            //BGGameMerchantApiService.CreateUserGameAccount(new ThirdPartyGameApiAccountCreateResultModel
            //{
            //    PlatformID = platformID,
            //    UserID = model.UserID
            //});

            //var isExist = BGGameMerchantApiService.IsAccountExist(new ThirdPartyGameApiCommonParameterModel
            //{
            //    PlatformID = platformID,
            //    UserID = model.UserID,
            //});

            //var isExist2 = BGGameMerchantApiService.IsAccountExist(new ThirdPartyGameApiCommonParameterModel
            //{
            //    PlatformID = platformID,
            //    UserID = 3333,
            //});


            //GetFishLaunchUrl



            //var qqqq222344444 = BGGameMerchantApiService.GetFishLaunchUrl(new ThirdPartyGameApiLaunchRequestModel
            //{
            //    PlatformID = platformID,
            //    UserID = model.UserID,
            //    IPAddress = "0.0.0.0"
            //});


            var queryEGames = BGGameMerchantApiService.GetEGameBetRecords(new BGGameMerchantApiQueryBetRecordRequestModel
            {
                PageIndex = 1,
                PageSize = 20,
                PlatformID = platformID,
                BeginTimeUTC = new DateTime(2020, 4, 21),
                EndTimeUTC = new DateTime(2020, 4, 22),
            });



            var qqqq222344444 = BGGameMerchantApiService.GetFishBetRecords(new BGGameMerchantApiQueryBetRecordRequestModel
            {
                PageIndex = 1,
                PageSize = 20,
                PlatformID = platformID,
                BeginTimeUTC = new DateTime(2020, 4, 21),
                EndTimeUTC = new DateTime(2020, 4, 22),
            });




            //var qq232332qqq = BGGameMerchantApiService.GetReportSummary(new ThirdPartyGameMerchantApiQueryDailyReportSummaryParameterModel
            //{
            //    PlatformID = platformID,
            //    BeginDateUTC = new DateTime(2020, 4, 20),
            //    EndDateUTC = new DateTime(2020, 4, 21),
            //});





            //var qq2327777q = BGGameMerchantApiService.TransferMoneyToGameWallet(new ThirdPartyGameMerchantApiWalletTransferRequestModel
            //{
            //    Amount = 66,
            //    AreaID = queryUser.AreaID,
            //    PlatformID = platformID,
            //    BeforeAmount = 0,
            //    IPAddress = "0.0.0.0",
            //    OrderNo = DateTimeHelper.GetCurrentTime().ToString("yyyyMMddHHmmssfff"),
            //    UserID = model.UserID,
            //});


            //var qq232332qqq = BGGameMerchantApiService.GetBetRecords(new BGGameMerchantApiQueryBetRecordRequestModel
            //{
            //    PageIndex = 1,
            //    PageSize = 500,
            //    PlatformID = platformID,
            //    BeginTimeUTC = DateTimeHelper.GetCurrentTime().AddDays(-1),
            //    EndTimeUTC = DateTimeHelper.GetCurrentTime(),
            //});


            //var qqqqq = BGGameMerchantApiService.GetTransferRecord( platformID, model.UserID , "123456");

            //var wallet = BGGameMerchantApiService.TransferMoneyToCenterWallet(new ThirdPartyGameMerchantApiWalletTransferRequestModel
            //{
            //    Amount = 77777,
            //    AreaID = queryUser.AreaID,
            //    PlatformID = platformID,
            //    BeforeAmount = 100,
            //    IPAddress = "0.0.0.0",
            //    OrderNo = DateTimeHelper.GetCurrentTime().ToString("yyyyMMddHHmmssfff"),
            //    UserID = model.UserID,
            //});

            //var qqqqq2 = BGGameMerchantApiService.GetWalletAmount(new ThirdPartyGameMerchantApiWalletAmountRequestModel
            //{
            //    PlatformID = platformID,
            //    UserID = model.UserID
            //});







            //APLGameMerchantApiService.TransferMoneyToCenterWallet(new ThirdPartyGameMerchantApiWalletTransferRequestModel
            //{
            //    Amount = 9999999,
            //    BeforeAmount = 0,
            //    UserID = 15266896,
            //    PlatformID = 2000,
            //    OrderNo = orderNo,
            //    GameMerchantWalletType = 15001001,
            //    AreaID = queryUser.AreaID,
            //});



            //APLGameMerchantApiService.GetTransferRecord(2000 , 15266896 , orderNo);


            //APLGameMerchantApiService.GetTransferRecord(2000 , 15266896, "TRO200316150231l4e437777");


            //string test = "";



            //APLGameMerchantApiService.GetReportSummary(new ThirdPartyGameMerchantApiQueryDailyReportSummaryParameterModel
            //{
            //    PlatformID = platformID,
            //    BeginTime = new DateTime(2020 , 3 , 16) ,
            //    EndTime = new DateTime(2020, 3, 17),
            //});





            //var result11 = EGamingGameMerchantApiService.TransferMoneyToCenterWallet(new ThirdPartyGameMerchantApiWalletTransferRequestModel
            //{
            //    PlatformID = platformID,
            //    UserID = model.UserID,
            //    Amount = 99999999999,
            //    OrderNo = orderNo,
            //    IPAddress = model.IPAddress
            //});

            //var result11 = EGamingGameMerchantApiService.TransferMoneyToGameWallet(new ThirdPartyGameMerchantApiWalletTransferRequestModel
            //{
            //    PlatformID = platformID,
            //    UserID = model.UserID,
            //    Amount = 125,
            //    OrderNo = orderNo,
            //    IPAddress = model.IPAddress
            //});

            //var result222 = EGamingGameMerchantApiService.GetTransferRecord(platformID , orderNo);




            //subChannelId



            //EGamingGameMerchantApiService.CreateUserGameAccount(new EGamingGameMerchantApiRequestModel
            //{
            //    PlatformID = platformID,
            //    UserID = queryUser.UserID
            //});


            //DateTime utcNow = DateTime.UtcNow;
            //return EGamingGameMerchantApiService.GetBetRecords(new EGamingGameMerchantApiQueryBetRecordFilterParameterModel
            //{
            //    PlatformID = platformID,
            //    BeginTimeUTC = utcNow.AddDays(-1),
            //    EndTimeUTC = utcNow,
            //    PageIndex = 1,
            //    PageSize = 100
            //});


            //var result_report = EGamingGameMerchantApiService.GetReportSummary(new EGamingGameMerchantApiQueryBetRecordFilterParameterModel
            //{
            //    PlatformID = platformID,
            //    BeginTimeUTC = utcNow.AddDays(-1),
            //    EndTimeUTC = utcNow,
            //    PageIndex = 1,
            //    PageSize = 100
            //});



            //var qqqq = EGamingGameMerchantApiService.GetWalletAmount(new ThirdPartyGameApiCommonParameterModel
            //{
            //    PlatformID = platformID,
            //    UserID = model.UserID,
            //});


            //var result11 = EGamingGameMerchantApiService.TransferMoneyToGameWallet(new ThirdPartyGameMerchantApiWalletTransferRequestModel
            //{
            //    PlatformID = platformID,
            //    UserID = model.UserID,
            //    Amount = 125,
            //    OrderNo = RandomHelper.GetOrderNo("DD"),
            //});



            //var result222 = EGamingGameMerchantApiService.GetGameLaunchResult(new ThirdPartyGameApiLaunchRequestModel
            //{
            //    PlatformID = platformID,
            //    UserID = model.UserID,
            //    IPAddress = "0.0.0.0",
            //    GameCode = "wwww"
            //});

            //var result = EGamingGameMerchantApiService.TransferMoneyToGameWallet(new ThirdPartyGameApiCommonParameterModel
            //{
            //    PlatformID = platformID,
            //    UserID = model.UserID,
            //    Amount = 125,
            //    OrderNo = RandomHelper.GetOrderNo("DD"),
            //});

            //var qqqq = EGamingGameMerchantApiService.GetWalletAmount(new EGamingGameMerchantApiRequestModel
            //{
            //    PlatformID = platformID,
            //    UserID = model.UserID
            //});



            //subChannelId

            return null;
        }









    }

}
