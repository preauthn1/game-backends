using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.Models.Enums;
using target.Models.Models;
using target.Utility.Extensions;
using target.Utility.Helper;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class MultiLanguageTextHelperTests : BaseTest
    {

        [TestMethod]
        public void GetCultureTextByEnum_Tests()
        {
            var textValues1 = MultiLanguageTextHelper.GetEnumsCultureText(WalletTransferTypeEnum.Transfer_In);
            var textValues2 = MultiLanguageTextHelper.GetEnumsCultureText(GameListEnum.ALD_阿拉丁);
            var textValues11 = MultiLanguageTextHelper.GetEnumsCultureText(LanguageEnum.zh_cn);
            var textValues12 = MultiLanguageTextHelper.GetEnumsCultureText(TransactionTypeEnum.TransferWallet_To_GameWallet);
            var textValues13 = MultiLanguageTextHelper.GetEnumsCultureText(PlayerTypeEnum.Robot);
            var textValues14 = MultiLanguageTextHelper.GetEnumsCultureText(PlayerStatusEnum.Enable);
            var textValues15 = MultiLanguageTextHelper.GetEnumsCultureText(WalletTypeEnum.GameWallet);
            var textValues16 = MultiLanguageTextHelper.GetEnumsCultureText(MerchantWalletModeEnum.SingleWalletMode);
            var textValues17 = MultiLanguageTextHelper.GetEnumsCultureTextByLanguage((GameListEnum)654121 , LanguageEnum.en_us);


            var textValues_21 = MultiLanguageTextHelper.GetEnumsCultureText(PlayerTypeEnum.Normal);
            var textValues_22 = MultiLanguageTextHelper.GetEnumsCultureText(PlayerTypeEnum.Robot);
            var textValues_28_EN = MultiLanguageTextHelper.GetEnumsCultureText(NormalActiveStatus.Enable , LanguageEnum.en_us);
            var textValues_28_CN = MultiLanguageTextHelper.GetEnumsCultureText(NormalActiveStatus.Disable , LanguageEnum.zh_cn);


            Assert.IsTrue(!string.IsNullOrWhiteSpace(textValues1));
            Assert.IsTrue(!string.IsNullOrWhiteSpace(textValues2));
            Assert.IsTrue(!string.IsNullOrWhiteSpace(textValues11));
            Assert.IsTrue(!string.IsNullOrWhiteSpace(textValues12));
            Assert.IsTrue(!string.IsNullOrWhiteSpace(textValues13));
            Assert.IsTrue(!string.IsNullOrWhiteSpace(textValues14));
            Assert.IsTrue(!string.IsNullOrWhiteSpace(textValues15));
            Assert.IsTrue(!string.IsNullOrWhiteSpace(textValues16));


            Assert.IsTrue(!string.IsNullOrWhiteSpace(textValues_21));
            Assert.IsTrue(!string.IsNullOrWhiteSpace(textValues_22));
            Assert.IsTrue(!textValues_28_EN.IsChinese());
            Assert.IsTrue(textValues_28_CN.IsChinese());
            return;
        }


        [TestMethod]
        public void GetCultureText_Other_Tests()
        {
            var textValues3 = MultiLanguageTextHelper.GameServerReconnectMsg("dfdz高級廳");
            var textValues4 = MultiLanguageTextHelper.DefaultErrorMsg("65536");
            var textValues5 = MultiLanguageTextHelper.EnterGameRoom_AmountNotEnough(100, 50);
            var textValues6 = MultiLanguageTextHelper.GetTimeZoneDisplayName(TimeZoneAreaEnum.Arabian_0400.ToInteger());

            var textValues_11 = MultiLanguageTextHelper.GetTimeZoneDisplayName(775100);

            var textValues_91 = MultiLanguageTextHelper.GetStringEnumsCultureText<CurrencyEnum>("MYR");
            var textValues_92 = MultiLanguageTextHelper.GetStringEnumsCultureText<CurrencyEnum>("USD");

            Assert.IsTrue(!string.IsNullOrWhiteSpace(textValues3));
            Assert.IsTrue(!string.IsNullOrWhiteSpace(textValues4));
            Assert.IsTrue(!string.IsNullOrWhiteSpace(textValues5));
            Assert.IsTrue(!string.IsNullOrWhiteSpace(textValues6));

            Assert.IsTrue(!string.IsNullOrWhiteSpace(textValues_11));

            Assert.IsTrue(!string.IsNullOrWhiteSpace(textValues_91));
            Assert.IsTrue(!string.IsNullOrWhiteSpace(textValues_92));

            return;
        }



        [TestMethod]
        public void GetErrorCodeMsg_Tests()
        {
            string replaceMsg = "被我覆蓋了";

            var textValues21 = MultiLanguageTextHelper.GetErrorCodeMsg(ApiErrorCode.Authorize_AccessToken_Expired, null);
            var textValues22 = MultiLanguageTextHelper.GetErrorCodeMsg(ApiErrorCode.BusinessLogicDeny, null);
            var textValues23 = MultiLanguageTextHelper.GetErrorCodeMsg(ApiErrorCode.InvalidParameter_ModelIsNull, new ErrorDetailModel { ReplaceMessage = replaceMsg });

            var text_7_1 = MultiLanguageTextHelper.GetErrorCodeMsg(ApiErrorCode.InvalidParameter, new ErrorDetailModel("mErChAntID"));
            var text_7_2 = MultiLanguageTextHelper.GetErrorCodeMsg(ApiErrorCode.InvalidParameter, new ErrorDetailModel("plaYeRid"));
            var text_7_3 = MultiLanguageTextHelper.GetErrorCodeMsg(ApiErrorCode.InvalidParameter, new ErrorDetailModel("merchantID"));
            var text_7_4 = MultiLanguageTextHelper.GetErrorCodeMsg(ApiErrorCode.InvalidParameter, new ErrorDetailModel("XXXXXQQQQYYY"));


            Assert.IsTrue(!string.IsNullOrWhiteSpace(textValues21));
            Assert.IsTrue(!string.IsNullOrWhiteSpace(textValues22));
            Assert.IsTrue(!string.IsNullOrWhiteSpace(textValues23));


            Assert.IsTrue(text_7_1.ContainsWihtoutCase("mErChAntID"));
            Assert.IsTrue(!string.IsNullOrWhiteSpace(text_7_2));
            Assert.IsTrue(!string.IsNullOrWhiteSpace(text_7_3));
            Assert.IsTrue(!string.IsNullOrWhiteSpace(text_7_4));
            Assert.IsTrue(text_7_4.ContainsWihtoutCase("XXXXXQQQQYYY"));

            Assert.IsTrue(replaceMsg == textValues23);
            return;
        }



        [TestMethod]
        public void GetGameVendorDisplayName_Tests()
        {
            var text_1 = MultiLanguageTextHelper.GetGameVendorDisplayName(GameVendorTypeEnum.AGG.ToInteger());
            var text_2 = MultiLanguageTextHelper.GetGameVendorDisplayName(GameVendorTypeEnum.Funky.ToInteger());
            var text_3 = MultiLanguageTextHelper.GetGameVendorDisplayName(GameVendorTypeEnum.Jili.ToInteger());

            Assert.IsTrue(!string.IsNullOrWhiteSpace(text_1));
            Assert.IsTrue(!string.IsNullOrWhiteSpace(text_2));
            Assert.IsTrue(!string.IsNullOrWhiteSpace(text_3));

            return;
        }




    }
}
