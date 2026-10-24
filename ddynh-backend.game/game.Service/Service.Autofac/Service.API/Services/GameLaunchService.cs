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
        public AreaRepository AreaRepository { get; set; }

        public UserInfoRepository UserInfoRepository { get; set; }

        public CashRepository CashRepository { get; set; }

        public CertificationRepository CertificationRepository { get; set; }

        public GameListRepository GameListRepository { get; set; }

        public GameUserListRepository GameUserListRepository { get; set; }

        public GameServerListRepository GameServerListRepository { get; set; }

        public UserGameLaunchSummaryRepository UserGameLaunchSummaryRepository { get; set; }

        public EGamingBetRecordRepository EGamingBetRecordRepository { get; set; }

        public AreaGameListRepository AreaGameListRepository { get; set; }

        public GameMerchantRepository GameMerchantRepository { get; set; }

        public IGameMerchantIntegrationCommonService GameMerchantIntegrationCommonService { get; set; }



        public GameLaunchService()
        {
            //注意事項 此service是透過AutoFac注入
            //以往的舊版寫法 IDatabaseFactory _dbFactory; 這種無法使用，

            //若要進行資料庫存取 
            // 1. 請呼叫 XXXXXXXRepository 的寫法
            // 2. 透過httpRequest 存取GameAPI (不建議此寫法了)
        }


        public GameLaunchViewModel GameLaunch(GameLaunchRequestViewModel model)
        {
            model.RuleFor(x => x.UserID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.GameID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.Certificate).Rule_NotEmptyText();
            model.RuleFor(x => x.IPAddress).Rule_NotEmptyText();

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

            var queryGame = GameListRepository.GetById(model.GameID);

            if (queryGame == null)
                throw new TerproException(ApiErrorCode.NotFoundGame);

            if(queryGame.GameMerchantID <= 0)
                throw new TerproException(ApiErrorCode.BusinessLogicDeny);

            var queryWallet = CashRepository.GetById(model.UserID);

            var queryLaunchSummary = UserGameLaunchSummaryRepository.GetByUserAndGameID(model.UserID , model.GameID);

            if (queryLaunchSummary == null)
            {
                long minAmount = GetMinRoomInMoneyAmount(model.GameID);

                //如果是首次啟動遊戲，且金幣太少 會限制最低入場金幣
                if (queryWallet == null || queryWallet.Amount < minAmount)
                {
                    var displayAmount = minAmount / 100;
                    string msgText = ApiErrorCode.Forbidden_WalletAmountNotEnough_Dollars.GetDescription() + $"({displayAmount}元)";
                    throw new TerproException(ApiErrorCode.Forbidden_WalletAmountNotEnough_Dollars , msgText);
                }
            }                

            GameMerchantEnum gameMercahnt = (GameMerchantEnum)queryGame.GameMerchantID;

            ThirdPartyGameApiLaunchResultModel api_result = null;

            int platformID = AreaRepository.GetSingleByCache(queryUser.AreaID).PlatformID;

            var para = new ThirdPartyGameApiLaunchRequestModel
            {
                IsTryPlayMode = model.IsTryPlayMode,
                PlatformID = platformID,
                IPAddress = model.IPAddress,
                UserID = model.UserID,
                GameCode = GetGameCodeByGameID(queryGame.GameID)
            };

            //檢查重複進房
            var queryDuplicateGameRoom = GameUserListRepository.GetById(model.UserID);

            if (queryDuplicateGameRoom != null && queryDuplicateGameRoom.ServerID <= 10000)
            {
                throw new TerproException(ApiErrorCode.Forbidden_DuplicateEnterGameServer);
            }


            var queryGameEnableSetting = AreaGameListRepository.GetByFilter(new QueryAreaGameListRequestViewModel
            {
                GameIDs = model.GameID.ToListCollection(),
                AreaIDs = queryUser.AreaID.ToListCollection(),
                Status = NormalActiveStatusEnum.Enable.ToInteger()
            });

            if(!model.IsSkipCheck)
            {
                //檢查渠道是否有開通此遊戲
                if (queryGameEnableSetting.IsEmptyOrNull())
                {
                    throw new TerproException(ApiErrorCode.NotFoundAreaGameList);
                }
            }

            //根據遊戲廠商不同 instance不同service
            IBaseThirdPartyGameMerchantApiService serviceInstance = InstanceHelper.InstanceServiceByGameMerchant(gameMercahnt);

            //檢查帳號是否存在(不存在則新建一筆)
            CheckAccountExistOrCreateNew(serviceInstance, model, platformID);

            //有成功幫他建帳號就可以建這個資料了
            UserGameLaunchSummaryRepository.CreateOrUpdate(model.UserID, model.GameID);

            //先launch、再轉帳照理說比較合適...但如果有些遊戲商限制入場金幣...
            api_result = serviceInstance.GetGameLaunchResult(para);


            //如果有拿到網址 嘗試 自動轉帳過去
            bool isSuccessGetUrl = !string.IsNullOrWhiteSpace(api_result.ResultUrl);

            long amount = (queryWallet.Amount / 100);

            if (isSuccessGetUrl && !model.IsTryPlayMode)
            {
                //限制一下不要轉太誇張的數字 (2萬)  (對方API沒有測試環境)
                if(SettingHelper.IsDev)
                {
                    amount = amount >= 20000 ? 20000 : amount;
                }
                
                var queryGameMerchant = GameMerchantRepository.GetById((int)gameMercahnt);

                //如果遊戲商有限制最大轉帳額度
                if (queryGameMerchant.MAXTrasnferAmount.HasValue)
                {
                    long maxValue = queryGameMerchant.MAXTrasnferAmount.Value / 100;
                    amount = amount >= maxValue ? maxValue : amount;
                }

                if (amount > 0)
                {
                    GameMerchantIntegrationCommonService.DoTransferToGameWalletProcess(gameMercahnt.ToInteger(), new ThirdPartyGameMerchantApiWalletTransferRequestModel
                    {
                        Amount = amount,
                        AreaID = queryUser.AreaID,
                        IPAddress = model.IPAddress,
                        PlatformID = platformID,
                        BeforeAmount = queryWallet.Amount,
                        UserID = model.UserID,
                        GameMerchantWalletType = queryGame.GameMerchantWalletID,
                    });
                }
            }
            

            return new GameLaunchViewModel
            {
                RedirectUrl = api_result.ResultUrl,
                TransferAmount = amount
            };
        }



        private long GetMinRoomInMoneyAmount(int gameID)
        {
            var queryServers = GameServerListRepository.GetByFilter(new QueryGameServerListRequestViewModel
            {
                GameIDs = gameID.ToListCollection()
            });
            
            //若有查到伺服器，已伺服器設定為主
            if(queryServers.IsNotEmptyNull())
            {
                var queryFisrt = queryServers.FirstOrDefault();

                //防呆 最低金額限制
                queryFisrt.Money = queryFisrt.Money <= 100 ? 100: queryFisrt.Money;
                return queryFisrt.Money;
            }
                       
            //若無伺服器，以config設置為準
            return SettingHelper.ThirdPartyGamePlayMinMoney * 100;
        }



        private string GetGameCodeByGameID(int gameID)
        {
            GameListEnum gameIDEnum = (GameListEnum)gameID;

            if (!gameIDEnum.IsDefinedValue())
                throw new TerproException(ApiErrorCode.NotFoundDefineEnumValue);

            var setting = gameIDEnum.GetAttributeFromEnum<EnumSettingValue>();

            if (setting == null)
                return string.Empty;

            return setting.Value;
        }




        private void CheckAccountExistOrCreateNew(IBaseThirdPartyGameMerchantApiService serviceInstance ,GameLaunchRequestViewModel model, int platformID)
        {
            bool isAccountExist = serviceInstance.IsAccountExist(new ThirdPartyGameApiCommonParameterModel
            {
                PlatformID = platformID,
                UserID = model.UserID

            }).IsExist;

            if (!isAccountExist)
            {
                serviceInstance.CreateUserGameAccount(new ThirdPartyGameApiAccountCreateResultModel
                {
                    PlatformID = platformID,
                    UserID = model.UserID
                });
            }
        }    

    }

}
