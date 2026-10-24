using NSubstitute;
using target.BL.Services;
using target.Utility;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.DA.Repository;
using target.Models;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.Utility.Extensions.Web;
using target.Utility.Helper;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    [DoNotParallelize]
    public class VendorCustomizedRequirementServiceTests : BaseTest
    {

        public IVendorCustomizedRequirementService VendorCustomizedRequirementService { get; set; }


        [TestMethod]
        public void GetGameReplayScreenshotThub_Tests()
        {
            var headers = target.Utility.Helper.HttpContext.MyHttpContext.Current.Request.Headers;
            bool hadProviderCode = headers.ContainsKey("ProviderCode");
            var originalProviderCode = headers["ProviderCode"];

            try
            {
                headers["ProviderCode"] = "PGSS";
                var repository = new MerchantCustomizedSettingRepository();
                var merchants = repository.GetByFilter(new QueryMerchantCustomizedSettingRequestViewModel
                {
                    SettingValueMatch = "PGSS",
                    MatchMerchantByLogicType = SingleWalletPlatformApiLogicTypeEnum.THub.ToInteger(),
                    CustomizedSettingKeyIDs = new List<int>
                    {
                        MerchantCustomizedSettingKeyEnum.Integration_SingleWallet_ReverseReferCode.ToInteger()
                    }
                });
                Assert.IsTrue(merchants.Any(), "測試環境必須有 PGSS 的 THub 商戶設定。");

                const string gameRoundId = "THUB-REPLAY-TEST";
                const string expectedUrl = "https://example.invalid/replay/THUB-REPLAY-TEST";
                var thirdPartyService = Substitute.For<IThirdPartyGameVendorService>();
                thirdPartyService.GetDetailUrlByTransactionNo(SystemConstant.SkipValidateUserID, gameRoundId)
                    .Returns(new ThirdPartyGameVendorBetRecordDetailUrlViewModel
                    {
                        MerchantID = merchants.First().MerchantID,
                        Url = expectedUrl
                    });
                var service = new VendorCustomizedRequirementService
                {
                    MerchantCustomizedSettingRepository = repository,
                    ThirdPartyGameVendorService = thirdPartyService
                };
                var result = service.GetGameReplayScreenshotThub(new THubPlatformGetGameReplayScreenshotRequestParameter
                {
                    GameRoundId = gameRoundId
                });

                Assert.AreEqual(expectedUrl, result.Value.Url);
                thirdPartyService.Received(1).GetDetailUrlByTransactionNo(SystemConstant.SkipValidateUserID, gameRoundId);

                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Value);
                Assert.IsFalse(string.IsNullOrWhiteSpace(result.Value.Url));
                Assert.IsTrue(System.Uri.TryCreate(result.Value.Url, System.UriKind.Absolute, out var replayUri));
                Assert.IsTrue(replayUri.Scheme == System.Uri.UriSchemeHttp || replayUri.Scheme == System.Uri.UriSchemeHttps);
            }
            finally
            {
                if (hadProviderCode)
                    headers["ProviderCode"] = originalProviderCode;
                else
                    headers.Remove("ProviderCode");
            }
        }


        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("   ")]
        public void GetGameReplayScreenshotThub_EmptyGameRoundId_Tests(string gameRoundId)
        {
            var exception = Assert.ThrowsException<TerproException>(() => VendorCustomizedRequirementService.GetGameReplayScreenshotThub(
                new THubPlatformGetGameReplayScreenshotRequestParameter { GameRoundId = gameRoundId }));

            Assert.AreEqual(ApiErrorCode.InvalidParameter_TextEmpty, exception.ErrroCode);
            Assert.AreEqual("GameRoundId", exception.ErrorDetail.ColumnName);
        }


        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("   ")]
        public void GetGameReplayScreenshotThub_EmptyProviderCode_Tests(string providerCode)
        {
            var headers = target.Utility.Helper.HttpContext.MyHttpContext.Current.Request.Headers;
            bool hadProviderCode = headers.ContainsKey("ProviderCode");
            var originalProviderCode = headers["ProviderCode"];

            try
            {
                if (providerCode == null)
                    headers.Remove("ProviderCode");
                else
                    headers["ProviderCode"] = providerCode;

                var exception = Assert.ThrowsException<TerproException>(() => VendorCustomizedRequirementService.GetGameReplayScreenshotThub(
                    new THubPlatformGetGameReplayScreenshotRequestParameter { GameRoundId = "THUB-REPLAY-TEST" }));

                Assert.AreEqual(ApiErrorCode.InvalidParameter, exception.ErrroCode);
                Assert.AreEqual("ProviderCode", exception.ErrorDetail.ColumnName);
            }
            finally
            {
                if (hadProviderCode)
                    headers["ProviderCode"] = originalProviderCode;
                else
                    headers.Remove("ProviderCode");
            }
        }


        [TestMethod]
        public void GetGameList_THub_Test()
        {
            var queryItems = VendorCustomizedRequirementService.GetGameListTHub(new THubPlatformGetGameListRequestParameter
            {
                PageNo = 1,
                PageSize = 50,
                ProviderCode = "PGSS"
            });

            Assert.IsNotNull(queryItems.Value);
            Assert.IsTrue(queryItems.Value.Total > 0);
            Assert_NotEmptyCollection(queryItems.Value.Games);
            return;
        }


        [TestMethod]
        public void GetGameList_Notix_Test()
        {
            var queryItems = VendorCustomizedRequirementService.GetGameListNotix(new NotixGameListRequestParameter
            {
                ProviderCode = "target"
            });

            Assert_NotEmptyCollection(queryItems.games);
            return;
        }


    }
}

