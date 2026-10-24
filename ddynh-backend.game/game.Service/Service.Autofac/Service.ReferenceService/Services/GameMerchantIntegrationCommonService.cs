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
using game.Infrastructure.Utility;
using game.Infrastructure.Extensions;
using System.Web;
using System.Text;
using System.Security.Cryptography;
using game.Entity.Money;
using game.Entity.Gamegame.Entity.Game;
using game.Entity.Enums;

namespace game.Service.Service
{
    public class GameMerchantIntegrationCommonService : BaseThirdPartyGameService, IGameMerchantIntegrationCommonService
    {
        public override int GameMerchantID
        {
            get
            {
                return GetGameMerchantID();
            }
        }


        /// <summary>
        /// 鳥鳥的寫法...
        /// </summary>
        private int _overrideGameMerchantID { get; set; }

        public TransferRecordRepository TransferRecordRepository { get; set; }

        public CashRepository CashRepository { get; set; }

        public UserInfoRepository UserInfoRepository { get; set; }



        public void DoTransferToGameWalletProcess(int gameMerchantID, ThirdPartyGameMerchantApiWalletTransferRequestModel model)
        {
            model.RuleFor(x => x.UserID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.Amount).Rule_NotZeroNotMinus();


            _overrideGameMerchantID = gameMerchantID;
            
            //檢查商戶狀態
            CheckGameMerchantIsValid(gameMerchantID, model.PlatformID);

            //為了避免call by reference的問題 另存變數
            long amount_x_100 = model.Amount_X_100;

            model.OrderNo = GenerateOrderNo("TRI" , gameMerchantID, model.UserID);

            //先寫一筆轉帳紀錄
            TransferRecordRepository.Create(new TransferRecordRawEntity
            {
                GameMerchantID = GameMerchantID,
                Amount = amount_x_100,
                Status = ThirdPartyTransferRecordStatusEnum.Waiting.ToInteger(),
                BeforeAmount = 0,
                CreateTime = DateTimeHelper.GetCurrentTime(),
                CreateIPAddress = model.IPAddress,
                TransferType = TransferTypeEnum.Transfer_In.ToInteger(),
                OrderNo = model.OrderNo,
                GameMerchantOrderNo = "",
                UserID = model.UserID,
                PlatformID = model.PlatformID,
                GameMerchantWalletType = model.GameMerchantWalletType,
                Memo = "",
            });


            //預先扣錢 寫入一筆鎖定資金 (?)
            int updateCashResultCode = CashRepository.UpdateWalletBalance(new UpdateUserBalanceParameterModel
            {
                Amount = model.Amount_X_100 * -1,
                AreaID = model.AreaID,
                PlatformID = model.PlatformID,
                UserID = model.UserID,
                IPAddress = model.IPAddress,
                CurrentAmount = model.BeforeAmount,
                Memo = SourceTypeEnum.TransferWallet_ToGameWallet.GetEnumDescription(),
                SourceType = SourceTypeEnum.TransferWallet_ToGameWallet.ToInteger(),
            });
            
            if (updateCashResultCode < 0)
            {
                //更新轉帳狀態(失敗)
                TransferRecordRepository.UpdateTransferStatus(model.OrderNo, ThirdPartyTransferRecordStatusEnum.Fail, "呼叫SP UpdateWalletBalance失敗");
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_UpdateWalletBalanceFail);
            }

            GameMerchantEnum gameMerchant = (GameMerchantEnum)gameMerchantID;

            ThirdPartyGameApiWalletTransferResultModel transferResult = null;

            //根據廠商不同 呼叫不同service實作
            IBaseThirdPartyGameMerchantApiService instanceType = InstanceHelper.InstanceServiceByGameMerchant(gameMerchant);
            transferResult = instanceType.TransferMoneyToGameWallet(model);
            

            //請務必要實作 DebugObject 返回值
            if (transferResult.DebugObject == null)
            {
                throw new TerproException(ApiErrorCode.NotImplement);
            }

            ThirdPartyGameTransferResultCheckModel transferCheckResult = transferResult.DebugObject;

            if (transferCheckResult.IsFirstRequestResponseSuccess)
            {
                //更新轉帳狀態(成功)
                TransferRecordRepository.UpdateTransferStatus(model.OrderNo, ThirdPartyTransferRecordStatusEnum.Success, transferCheckResult.DebugMsg);
            }
            else
            {
                if (transferCheckResult.IsRecheckApiSuccess)
                {
                    //更新轉帳狀態(成功)
                    TransferRecordRepository.UpdateTransferStatus(model.OrderNo, ThirdPartyTransferRecordStatusEnum.Success, transferCheckResult.DebugMsg);
                }
                else if (transferCheckResult.IsUnknowError)
                {
                    //更新轉帳狀態(需人工確認....假如對方沒有API可以確認)
                    TransferRecordRepository.UpdateTransferStatus(model.OrderNo, ThirdPartyTransferRecordStatusEnum.ConfirmRequired, transferCheckResult.DebugMsg);
                }
                else
                {
                    //更新轉帳狀態(失敗)
                    TransferRecordRepository.UpdateTransferStatus(model.OrderNo, ThirdPartyTransferRecordStatusEnum.Fail, transferCheckResult.DebugMsg);

                    //串接新遊戲 請務必要測試到這段 (退還金幣)
                    CashRepository.UpdateWalletBalance(new UpdateUserBalanceParameterModel
                    {
                        Amount = amount_x_100,
                        AreaID = model.AreaID,
                        PlatformID = model.PlatformID,
                        UserID = model.UserID,
                        IPAddress = model.IPAddress,
                        Memo = SourceTypeEnum.TransferWallet_ReleaseBack.GetEnumDescription(),
                        SourceType = SourceTypeEnum.TransferWallet_ReleaseBack.ToInteger(),
                    });
                }
            }
        }


        public void DoTransferToCenterWalletProcess(int gameMerchantID, ThirdPartyGameMerchantApiWalletTransferRequestModel model)
        {
            model.RuleFor(x => x.UserID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.Amount).Rule_NotZeroNotMinus();

            _overrideGameMerchantID = gameMerchantID;

            //檢查商戶狀態
            CheckGameMerchantIsValid(gameMerchantID, model.PlatformID);

            GameMerchantEnum gameMerchant = (GameMerchantEnum)gameMerchantID;

            model.OrderNo = GenerateOrderNo("TRO" , gameMerchantID , model.UserID);

            //為了避免call by reference的問題 另存變數
            long amount_x_100 = model.Amount_X_100;

            //先寫一筆轉帳紀錄
            TransferRecordRepository.Create(new TransferRecordRawEntity
            {
                GameMerchantID = GameMerchantID,
                Amount = model.Amount_X_100,
                Status = ThirdPartyTransferRecordStatusEnum.Waiting.ToInteger(),
                BeforeAmount = 0,
                CreateTime = DateTimeHelper.GetCurrentTime(),
                CreateIPAddress = model.IPAddress,
                TransferType = TransferTypeEnum.Transfer_Out.ToInteger(),
                OrderNo = model.OrderNo,
                GameMerchantOrderNo = "",
                UserID = model.UserID,
                PlatformID = model.PlatformID,
                GameMerchantWalletType = model.GameMerchantWalletType,
                Memo = "",
            });



            //根據廠商不同 呼叫不同service實作
            IBaseThirdPartyGameMerchantApiService instanceType = InstanceHelper.InstanceServiceByGameMerchant(gameMerchant);

            ThirdPartyGameApiWalletTransferResultModel transferResult = instanceType.TransferMoneyToCenterWallet(model);

            //請勿必要實作 DebugObject 返回值
            if (transferResult.DebugObject == null)
            {
                throw new TerproException(ApiErrorCode.NotImplement);
            }


            ThirdPartyGameTransferResultCheckModel transferCheckResult = transferResult.DebugObject;

            if (transferCheckResult.IsFirstRequestResponseSuccess)
            {
                //更新轉帳狀態(成功)
                TransferRecordRepository.UpdateTransferStatus(model.OrderNo, ThirdPartyTransferRecordStatusEnum.Success, transferCheckResult.DebugMsg);

                //把中心錢包的錢加回去
                AddWalletCashWhenGameWalletReturn(amount_x_100 , model);
            }
            else
            {
                if (transferCheckResult.IsRecheckApiSuccess)
                {
                    //更新轉帳狀態(成功)
                    TransferRecordRepository.UpdateTransferStatus(model.OrderNo, ThirdPartyTransferRecordStatusEnum.Success, transferCheckResult.DebugMsg);                    

                    //把中心錢包的錢加回去
                    AddWalletCashWhenGameWalletReturn(amount_x_100 , model);

                }
                else if (transferCheckResult.IsUnknowError)
                {
                    //更新轉帳狀態(需人工確認....假如對方沒有API可以確認)
                    TransferRecordRepository.UpdateTransferStatus(model.OrderNo, ThirdPartyTransferRecordStatusEnum.ConfirmRequired, transferCheckResult.DebugMsg);
                }
                else
                {
                    //更新轉帳狀態(失敗)
                    TransferRecordRepository.UpdateTransferStatus(model.OrderNo, ThirdPartyTransferRecordStatusEnum.Fail, transferCheckResult.DebugMsg);
                }
            }
        }

        private void AddWalletCashWhenGameWalletReturn(long amount_x_100, ThirdPartyGameMerchantApiWalletTransferRequestModel model)
        {
            CashRepository.UpdateWalletBalance(new UpdateUserBalanceParameterModel
            {
                Amount = amount_x_100,
                AreaID = model.AreaID,
                PlatformID = model.PlatformID,
                UserID = model.UserID,
                IPAddress = model.IPAddress ?? "",
                Memo = SourceTypeEnum.TransferWallet_ToCenterWallet.GetEnumDescription(),
                SourceType = SourceTypeEnum.TransferWallet_ToCenterWallet.ToInteger(),
            });
        }


        public void CheckGameMerchantIsValid(int gameMerchantID, int platformID)
        {
            _overrideGameMerchantID = gameMerchantID;

            //檢查遊戲商狀態
            CheckGameMerchantExistAndStatusIsOK();

            //取得遊戲商密鑰設置
            GetGameMerchantConfigDataAndCheckStatus(platformID);
        }

        
        private string GenerateOrderNo(string prefix, int gameMerchantID , int userID)
        {
            //有特定幾家的訂單號 僅支援數字格式
            if (gameMerchantID == (int)GameMerchantEnum.BG)
            {
                string dateText = DateTimeHelper.GetCurrentTimeTimestamp().ToString();
                return dateText + userID.ToString();
            }

            return RandomHelper.GetOrderNo(prefix);
        }
        

        public int GetGameMerchantID()
        {
            return _overrideGameMerchantID;
        }

    }

}
