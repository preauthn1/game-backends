using game.Entity.Game;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using target.BL.Interface;
using target.BL.Services;
using target.DA.Repository;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.ViewModels;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Utility.IOC;
using target.Utility.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class SingleWalletPlatformApiRequestRefServiceTests : BaseTest
    {
        public SingleWalletMerchantSessionTokenRepository SingleWalletMerchantSessionTokenRepository { get; set; }

        public ISingleWalletPlatformApiRequestRefService ServiceInstance { get; set; }


        public class MockNotFoundRecordPlatformApiRequestRefService : SingleWalletPlatformApiRequestRefService            
        {
            protected override ApiRequestResultModel<T> RequestApiResult<T>(RequestParameters para, object body = null)
            {
                dynamic res = new ApiRequestResultModel<BaseSingleWalletModeResponseModel<SingleWalletModeCancelApiResponseModel>>
                {
                    ResponseText = "{ 'code' : '404016'  ,  'message' : '查無注單' }",
                    ResultObject = new BaseSingleWalletModeResponseModel<SingleWalletModeCancelApiResponseModel>
                    {
                        Code = ApiErrorCode.NotFoundBetRecord.ToInteger(),
                        Data = null
                    }
                };

                return res;
            }
        }

        public class MockBalanceNotEnoughPlatformApiRequestRefService : SingleWalletPlatformApiRequestRefService
        {
            protected override ApiRequestResultModel<T> RequestApiResult<T>(RequestParameters para, object body = null)
            {
                dynamic res = new ApiRequestResultModel<BaseSingleWalletModeResponseModel<SingleWalletModePlaceBetApiResponseModel>>
                {
                    ResponseText = "{ 'code' : '403652' ,  'message' : '餘額不足' }",
                    ResultObject = new BaseSingleWalletModeResponseModel<SingleWalletModePlaceBetApiResponseModel>
                    {
                        Code = ApiErrorCode.BetFail_WalletAmountNotEnough.ToInteger(),
                        Data = null
                    }
                };

                return res;
            }
        }


        public class MockTimeoutErrorPlatformApiRequestRefService : SingleWalletPlatformApiRequestRefService
        {
            protected override ApiRequestResultModel<T> RequestApiResult<T>(RequestParameters para, object body = null)
            {
                dynamic res = new ApiRequestResultModel<BaseSingleWalletModeResponseModel<SingleWalletModeCancelApiResponseModel>>
                {
                    Exception = new Exception("timeout"),
                    IsTimeoutException = true,
                    ResponseText = "Error!",
                };

                return res;
            }
        }


        [TestMethod]
        public void SingleWallet_PlaceBet_NotEnoughBalance_Test()
        {
            var mockService = new MockBalanceNotEnoughPlatformApiRequestRefService();

            var defaultData = new Models.Table.SingleWalletMerchantSessionTokenEntity
            {
                SessionTokenPGS = _TEST_DATA.SingleWallet.SessionTokenPGS,
                SessionTokenRaw = _TEST_DATA.SingleWallet.SessionTokenRaw,
                CreateTime = DateTimeHelper.GetCurrentTime(),
                UpdateTime = DateTimeHelper.GetCurrentTime(),
                GameID = _TEST_DATA.Default.GameID,
                MerchantID = _TEST_DATA.SingleWallet.MerchantID,
                PlayerID = _TEST_DATA.Default.PlayerID,
                PlayerDisplayName = _TEST_DATA.SingleWallet.PlayerName,
            };

            var queryPGS = SingleWalletMerchantSessionTokenRepository.GetByFilter(new QuerySingleWalletMerchantSessionTokenRequestViewModel
            {
                SessionTokenPGS = defaultData.SessionTokenPGS
            });

            //若沒資料 自動產生資料
            if (queryPGS.IsEmptyOrNull())
            {
                SingleWalletMerchantSessionTokenRepository.Create(defaultData);
            }

            try
            {
                var result = mockService.PlaceBet(new SingleWalletModePlaceBetParameter
                {
                    SessionTokenPGS = defaultData.SessionTokenPGS,
                    BetRecordNoPGS = _TEST_DATA.SingleWallet.BetRecordNo,
                    BetAmount = 10,
                    GameID = defaultData.GameID,
                    PlayerID = defaultData.PlayerID,
                });
            }
            catch(Exception ex)
            {
                AssertExpectException(ex, new List<ApiErrorCode>
                {
                    ApiErrorCode.BetFail_WalletAmountNotEnough
                });
            }            
           
            return;
        }


        [TestMethod]
        public void SingleWallet_Cancel_NotFoundBetRecord_Test()
        {
            var mockService = new MockNotFoundRecordPlatformApiRequestRefService();

            var result = mockService.Cancel(new SingleWalletModeCancelApiParameter
            {
                GameID = _TEST_DATA.Default.GameID,
                MerchantID = _TEST_DATA.SingleWallet.MerchantID,
                PlayerID = _TEST_DATA.SingleWallet.PlayerID,
                TransactionNo = RandomHelper.GetOrderNo("UNI")
            });

            Assert.IsNotNull(result);

            return;
        }


        [TestMethod]
        public void SingleWallet_Cancel_Timeout_Test()
        {
            var mockService = new MockTimeoutErrorPlatformApiRequestRefService();

            try
            {
                var result = mockService.Cancel(new SingleWalletModeCancelApiParameter
                {
                    GameID = _TEST_DATA.Default.GameID,
                    MerchantID = _TEST_DATA.SingleWallet.MerchantID,
                    PlayerID = _TEST_DATA.SingleWallet.PlayerID,
                    TransactionNo = RandomHelper.GetOrderNo("UNI")
                });

            }
            catch(Exception ex)
            {
                AssertExpectException(ex, new List<ApiErrorCode>
                {
                    ApiErrorCode.ThirdPartyApiFail_Timeout
                });
            }

            return;
        }




        [TestMethod]
        public void CheckIsErrorResponse_Test()
        {
            List<int> ignoreErrorCode = new List<int>
            {
                ApiErrorCode.BusinessLogicDeny_DuplicateDataExist.ToInteger(),
            };

            var myService = new SingleWalletPlatformApiRequestRefService();

            var platformFakeResponse1 = new BaseSingleWalletModeResponseModel<string>
            {
                Code = ApiErrorCode.BusinessLogicDeny_DuplicateDataExist.ToInteger(),
                Data = "MSTest"
            };

            var platformFakeResponse2 = new BaseSingleWalletModeResponseModel<string>
            {
                Code = ApiErrorCode.BetFail_WalletAmountNotEnough.ToInteger(),
                Data = "MSTest"
            };            

            var apiFakeResponse1 = new ApiRequestResultModel<BaseSingleWalletModeResponseModel<string>>
            {
                ResultObject = platformFakeResponse1,
                ResponseText = platformFakeResponse1.ConvertToJsonString(),
            };

            var apiFakeResponse2 = new ApiRequestResultModel<BaseSingleWalletModeResponseModel<string>>
            {
                ResultObject = platformFakeResponse2,
                ResponseText = platformFakeResponse2.ConvertToJsonString(),
            };
                      
            var apiFakeResponse4 = new ApiRequestResultModel<BaseSingleWalletModeResponseModel<string>>
            {
                ResultObject = null,
                ResponseText = "SystemError",
            };

            var result1 = myService.CheckIsErrorResponse(apiFakeResponse1, "XXX", false, ignoreErrorCode);
            var result2 = myService.CheckIsErrorResponse(apiFakeResponse2, "XXX", false, ignoreErrorCode);
            var result4 = myService.CheckIsErrorResponse(apiFakeResponse4, "XXX", false, ignoreErrorCode);


            Assert.IsFalse(result1.isException);
            Assert.IsTrue(result2.isException);
            Assert.IsTrue(result4.isException);

            Assert.IsTrue(((int)result1.errorCode) == apiFakeResponse1.ResultObject.Code);
            Assert.IsTrue(((int)result2.errorCode) == apiFakeResponse2.ResultObject.Code);

            //預期會拋錯誤
            AssertExpectExceptionProcess(() =>
            {
                var result222 = myService.CheckIsErrorResponse(apiFakeResponse2, "XXX", true, ignoreErrorCode);

            }, (ApiErrorCode)apiFakeResponse2.ResultObject.Code);

            return;
        }



        [TestMethod]
        public void CheckIsErrorResponse_StatusCode400_Test()
        {
            List<int> ignoreErrorCode = new List<int>
            {
                ApiErrorCode.BusinessLogicDeny_DuplicateDataExist.ToInteger(),
            };

            var myService = new SingleWalletPlatformApiRequestRefService();


            var platformFakeResponseStatus400 = new BaseSingleWalletModeResponseModel<string>
            {
                Code = ApiErrorCode.BusinessLogicDeny_DuplicateDataExist.ToInteger(),
                Data = "MSTest"
            };

            var apiFakeResponse_Status400 = new ApiRequestResultModel<BaseSingleWalletModeResponseModel<string>>
            {
                Exception = new Exception("MStest"),
                ResponseText = platformFakeResponseStatus400.ConvertToJsonString(),
            };

            var apiFakeResponse_Status400_UnknowResponseText = new ApiRequestResultModel<BaseSingleWalletModeResponseModel<string>>
            {
                Exception = new Exception("MStest"),
                ResponseText = "SystemError",
            };

            var result_001 = myService.CheckIsErrorResponse(apiFakeResponse_Status400, "XXX", false, ignoreErrorCode);
            var result_002 = myService.CheckIsErrorResponse(apiFakeResponse_Status400_UnknowResponseText, "XXX", false, ignoreErrorCode);


            Assert.IsFalse(result_001.isException);
            Assert.IsTrue(result_002.isException);

            Assert.IsTrue(((int)result_001.errorCode) == platformFakeResponseStatus400.Code);
            Assert.IsTrue(((int)result_002.errorCode) == (int)ApiErrorCode.ThirdPartyApiFail_ResponseError || ((int)result_002.errorCode) == (int)ApiErrorCode.ThirdPartyApiFail_SeamlessAPIResponseError);

            return;
        }


    }
}

