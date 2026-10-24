using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.Models.Models;
using target.Utility.Extensions;
using target.Utility.Helper;
using System.Linq;
using target.Models.CoreLibs;
using target.Models.ViewModels;
using target.DA.Repository;
using target.Models.Enums;
using System.Collections.Generic;
using System;
using game.Entity.Game;
using target.BL.Services;

namespace target.Testing
{
    [TestClass]
    public class PlayerInfoServiceTests : BaseTest
    {

        public IPlayerService ServiceInstance { get; set; }


        public IViewPermissionCheckService ViewPermissionCheckService { get; set; }


        [TestMethod]
        public void PlayerInfo_GetPlayerIdByPlayerNameOrID_Test()
        {
            var query1 = ServiceInstance.GetPlayerIdByPlayerNameOrID(_TEST_DATA.Default.UserID, new PlayerDetailQueryRequest(_TEST_DATA.Default.PlayerID.ToString()));
            var query2 = ServiceInstance.GetPlayerIdByPlayerNameOrID(_TEST_DATA.Default.UserID, new PlayerDetailQueryRequest(_TEST_DATA.Default.PlayerName));
            var query3 = ServiceInstance.GetPlayerIdByPlayerNameOrID(_TEST_DATA.Default.UserID, new PlayerDetailQueryRequest
            {
                PlayerNameOrID = _TEST_DATA.Default.PlayerID.ToString(),
                MerchantID = _TEST_DATA.Default.MerchantID
            });

            Assert.IsTrue(query1 > 0);
            Assert.IsTrue(query2 > 0);
            Assert.IsTrue(query3 > 0);

            try
            {
                //確保不是出現int32 overflow的錯誤
                var query999 = ServiceInstance.GetPlayerIdByPlayerNameOrID(_TEST_DATA.Default.UserID, new PlayerDetailQueryRequest((9992147483647).ToString()));
            }
            catch(Exception ex)
            {
                AssertExpectException(ex, ApiErrorCode.NotFoundPlayer);
            }

            return;
        }



        [TestMethod]
        public void PlayerInfo_GetByPagingFilter_Test()
        {
            var query = ServiceInstance.GetByPagingFilter(_TEST_DATA.Default.UserID , new QueryPlayerApiRequestParameterModel { PageSize = 10});
            Assert_NotEmptyCollection(query.Items);
            return;
        }


        [TestMethod]
        public void Player_GetByPagingFilter_FilterAccount_Test()
        {
            var query = ServiceInstance.GetByPagingFilter(_TEST_DATA.Default.UserID, new QueryPlayerApiRequestParameterModel 
            {
                PlayerName = _TEST_DATA.Default.PlayerName,
                PageSize = 10 
            });

            Assert_NotEmptyCollection(query.Items);
            return;
        }


        [TestMethod]
        public void PlayerInfo_GetByPagingFilter_AndCheckViewPermission_Test()
        {
            GetPlayersAndCheckViewPermission(_TEST_DATA.Default.UserID);
            GetPlayersAndCheckViewPermission(_TEST_DATA.CreditAgent.UserID);
            GetPlayersAndCheckViewPermission(_TEST_DATA.NotSuperMerchant.UserID);
            return;
        }


        private void GetPlayersAndCheckViewPermission(int userID)
        {
            var query = ServiceInstance.GetByPagingFilter(userID, new QueryPlayerApiRequestParameterModel 
            { 
                PageSize = 20 
            });

            if(query.Items.IsNotEmptyNull())
            {
                var merchantIDs = query.Items.Select(x => x.MerchantID).Distinct().ToList();

                foreach(var mID in merchantIDs)
                {
                    ViewPermissionCheckService.CheckViewMerchantPermission(userID, mID);
                }
            }            
        }


        [TestMethod]
        public void PlayerInfo_GetPlayerSummaryInfo_Test()
        {
            var query = ServiceInstance.GetPlayerSummaryInfo(_TEST_DATA.Default.UserID , _TEST_DATA.Default.PlayerID);
            Assert.IsNotNull(query);
            return;
        }


        [TestMethod]
        public void PlayerInfo_CheckPlayerAnyW88TransferFail_Test()
        {
            var query1 = ServiceInstance.CheckPlayerAnyW88TransferFail(_TEST_DATA.Default.UserID, new LazyCommonRequestModel
            {
                PlayerID = _TEST_DATA.Default.PlayerID,
            });

            var query2 = ServiceInstance.CheckPlayerAnyW88TransferFail(_TEST_DATA.Default.UserID, new LazyCommonRequestModel
            {
                PlayerID = _TEST_DATA.NotSuperMerchant.PlayerID,
            });

            Assert.IsNotNull(query1);
            Assert.IsNotNull(query2);
            return;
        }



        [TestMethod]
        public void UpdatePlayerStatus_Test()
        {
            ServiceInstance.UpdatePlayerStatus(_TEST_DATA.Default.UserID , new PlayerUpdateStatusRequestModel
            {
                Memo = "unitTest",
                Status = PlayerStatusEnum.Enable.ToInteger(),
                PlayerID = _TEST_DATA.Default.PlayerID
            });

            return;
        }


        [TestMethod]
        public void ManualDepositPlayerWallet_Test()
        {
            var result = ServiceInstance.ManualDepositPlayerWallet(_TEST_DATA.Default.UserID,  new PlayerManualDepositRequestModel
            {
                Amount = 10,
                PlayerID = _TEST_DATA.Default.PlayerID,
                PlayerName = _TEST_DATA.Default.PlayerName,
                Memo = "MSTest"
            });

            Assert.IsTrue(result.BeforeAmount != 0 || result.AfterAmount != 0);
            return;
        }


        [TestMethod]
        public void SearchByPlayerID_ButNotFound_Test()
        {
            var result11 = ServiceInstance.GetByPagingFilter(_TEST_DATA.Default.UserID, new QueryPlayerApiRequestParameterModel
            {
                PlayerID = _TEST_DATA.SingleWallet.PlayerID,
            });

            AssertExpectExceptionProcess(() =>
            {
                var result2 = ServiceInstance.GetByPagingFilter(_TEST_DATA.NotSuperMerchant.UserID, new QueryPlayerApiRequestParameterModel
                {
                    PlayerID = _TEST_DATA.SingleWallet.PlayerID,
                });

            }, ApiErrorCode.NotFoundPlayer_IsThisAccountOrPlayerID);
            return;
        }

        [TestMethod]
        public void InternalTestAccountWalletDepositAmount_Test()
        {
            var result = ServiceInstance.InternalTestAccountWalletDepositAmount(_TEST_DATA.Default.UserID, new InternalTestAccountPlusWalletRequestModel
            {
                Amount = 10,
                ConfirmMerchantID = _TEST_DATA.Default.MerchantID,
                ConfirmPlayerName = _TEST_DATA.Default.PlayerName,
                PlayerID = _TEST_DATA.Default.PlayerID
            });

            Assert.IsTrue(result.BeforeAmount != 0 || result.AfterAmount != 0);
            return;
        }


        [TestMethod]
        public void InternalTestAccountWalletDepositAmount_Error_Test()
        {
            try
            {
                var result = ServiceInstance.InternalTestAccountWalletDepositAmount(_TEST_DATA.Default.UserID, new InternalTestAccountPlusWalletRequestModel
                {
                    Amount = 10,
                    ConfirmMerchantID = _TEST_DATA.Default.MerchantID,
                    ConfirmPlayerName = "XXXXXXXXXXXXXXXX",
                    PlayerID = _TEST_DATA.Default.PlayerID
                });

                Assert.Fail("不合乎預期");
            }
            catch (Exception ex)
            {
                AssertExpectException(ex, ApiErrorCode.InvalidParameter_ConfirmCodeNotMatch);
            }

            return;
        }


    }
}

