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
using System.Threading.Tasks;
using game.Entity.Gamegame.Entity.Game;
using game.Infrastructure.Utility.ThirdParty;

namespace game.Service.Service
{
    public class WalletTransferService : BaseService, IWalletTransferService
    {
        public UserInfoRepository UserInfoRepository { get; set; }

        public CertificationRepository CertificationRepository { get; set; }

        public UserGameLaunchSummaryRepository UserGameLaunchSummaryRepository { get; set; }

        public GameListRepository GameListRepository { get; set; }

        public GameMerchantRepository GameMerchantRepository { get; set; }

        public AreaRepository AreaRepository { get; set; }

        public TransferRecordRepository TransferRecordRepository { get; set; }



        public IAPLGameMerchantApiService APLGameMerchantApiService { get; set; }

        public IEGamingGameMerchantApiService EGamingGameMerchantApiService { get; set; }

        public IGameMerchantIntegrationCommonService GameMerchantIntegrationCommonService { get; set; }

        public IServerSideTriggerhNotifyService ServerSideTriggerhNotifyService { get; set; }


        public WalletTransferService()
        {
            //注意事項 此service是透過AutoFac注入
            //以往的舊版寫法 IDatabaseFactory _dbFactory; 這種無法使用，
            //若要進行資料庫存取 
            // 1. 請呼叫 XXXXXXXRepository 的寫法
            // 2. 透過httpRequest 存取GameAPI (不建議此寫法了)
        }


        public WalletTransferViewModel AutoWalletTransfer(WalletTransferRequestViewModel model)
        {
            model.RuleFor(x => x.UserID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.Certificate).Rule_NotEmptyText();

            var queryUser = UserInfoRepository.GetById(model.UserID);

            if (queryUser == null)
                throw new TerproException(ApiErrorCode.NotFoundUser);

            //正式環境才檢查 Certificate
            if (SettingHelper.IsProduction && model.Certificate != SystemConst.MAGIC_CODE)
            {
                var queryCert = CertificationRepository.GetById(model.UserID);

                if (queryCert == null)
                    throw new TerproException(ApiErrorCode.NotFoundUser);

                if (queryCert.CerCode != model.Certificate)
                    throw new TerproException(ApiErrorCode.Authorize_AccessToken_Expired);
            }

            //先查出是否曾經有啟動過遊戲
            var queryGameLaunchRecords = UserGameLaunchSummaryRepository.GetByFilter(new QueryUserGameLaunchSummaryRequestViewModel
            {
                UserIDs = model.UserID.ToListCollection()
            });

            //超過一個月沒玩了，也排除掉
            DateTime previousDays = DateTimeHelper.GetCurrentTime().AddDays(-1 * 30);
            queryGameLaunchRecords = queryGameLaunchRecords.Where(x => x.LastLaunchTime >= previousDays).ToList();


            // Case 1. 如果從來沒啟動過三方遊戲 直接結束
            if (queryGameLaunchRecords.IsEmptyOrNull())
            {
                return new WalletTransferViewModel();
            }

            int platformID = AreaRepository.GetSingleByCache(queryUser.AreaID).PlatformID;

            List<int> gameIDs = queryGameLaunchRecords.Select(x => x.GameID).ToList();

            var queryGames = GameListRepository.GetByIds(gameIDs);

            var groupByWalletType = queryGames.GroupBy(x => x.GameMerchantWalletID)
                .Select(x => new
                {
                    GameMerchantWalletID = x.Key,
                    GameMerchantID = x.FirstOrDefault().GameMerchantID
                })
                .ToList();


            List<int> gameMerchantIDs = queryGames.Select(x => x.GameMerchantID).ToList();

            var queryMerchants = GameMerchantRepository.GetByIds(gameMerchantIDs);

            List<bool> eachTransferResult = new List<bool>();
            List<Task> allAsyncThread = new List<Task>();

            foreach (var eachGameWallet in groupByWalletType)
            {
                var merchant = (GameMerchantEnum)eachGameWallet.GameMerchantID;
                var walletType = eachGameWallet.GameMerchantWalletID;

                var matchGameMerchant = queryMerchants.FirstOrDefault(x => x.GameMerchantID == eachGameWallet.GameMerchantID);

                //如果遊戲商再維護中或已關閉
                if (matchGameMerchant.Status != GameMerchantStatusEnum.Active.ToInteger())
                    continue;

                //用非同步的方式處理
                allAsyncThread.Add(Task.Factory.StartNew(() =>
                {
                    try
                    {
                        //根據廠商不同 instance不同service
                        IBaseThirdPartyGameMerchantApiService serviceInstance = InstanceHelper.InstanceServiceByGameMerchant(merchant);

                        //呼叫轉帳API
                        bool isTriggerTransfer = TransferWalletBalanceOrNot(model.UserID, queryUser.AreaID, platformID, eachGameWallet.GameMerchantID , eachGameWallet.GameMerchantWalletID , model.IPAddress , serviceInstance);
                        eachTransferResult.Add(isTriggerTransfer);

                    }
                    catch (Exception ex)
                    {
                        string msg = ex.GetDebugMessage();
                        MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
                        {
                            Channel = SlackChannelEnum.n_dd_api_game_merchant,
                            Message = $"AutoWalletTransfer , {merchant} , {walletType} , {ex.Message}"
                        });
                    }
                }));
            }


            //全部執行完才接著後續動作
            Task.WaitAll(allAsyncThread.ToArray() , millisecondsTimeout: 30000);


            //全部結束後，若有任一款遊戲觸發轉帳，提示刷新錢包金額
            bool isTransferAnyGameWallet = eachTransferResult.Any(x => x == true);
            if (isTransferAnyGameWallet)
            {
                ServerSideTriggerhNotifyService.TriggerRefreshWalletAmountEvent(model.UserID);
            }

            return new WalletTransferViewModel
            {
                
            };

        }
    

        private bool TransferWalletBalanceOrNot(int userID, int areaID, int platformID , int gameMerchantID , int gameMerchantWalletID , string ipAddress, IBaseThirdPartyGameMerchantApiService serviceInstance)
        {
            var walletParam = new ThirdPartyGameMerchantApiWalletAmountRequestModel
            {
                PlatformID = platformID,
                UserID = userID,
                GameMerchantWalletType = gameMerchantWalletID
            };

            var wallet = serviceInstance.GetWalletAmount(walletParam);

            if (wallet.BalanceAmount <= 0)
                return false;

            //如果錢包內有金幣，轉回來
            GameMerchantIntegrationCommonService.DoTransferToCenterWalletProcess(gameMerchantID, new ThirdPartyGameMerchantApiWalletTransferRequestModel
            {
                GameMerchantWalletType = gameMerchantWalletID,
                Amount = wallet.BalanceAmount,
                UserID = userID,
                AreaID = areaID,
                PlatformID = platformID,
                IPAddress = ipAddress
            });

            return true;
        }
    }

}
