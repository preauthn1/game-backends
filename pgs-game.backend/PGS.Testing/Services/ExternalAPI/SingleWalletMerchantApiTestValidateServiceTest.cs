using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Services;
using target.DA.Repository;
using target.Models.Models;
using target.Models.ViewModels;
using target.Utility.Helper;
using System.Linq;
using target.Utility.Extensions;

namespace target.Testing
{
    [TestClass]
    public class SingleWalletMerchantApiTestValidateServiceTest: BaseTest
    {
        public PerGameDetailLogRepository PerGameDetailLogRepository { get; set; }


        [TestMethod]
        public void Construct_Test()
        {
            int targetMerchantID = _TEST_DATA.SingleWallet.MerchantID;

            var queryPerGames = PerGameDetailLogRepository.GetByFilter(new QueryDetailLogRequestViewModel
            {
                MerchantID = targetMerchantID,
                GameStartTimeBegin = DateTimeHelper.GetCurrentTime().AddHours(-1 * 36),                
            });

            if (queryPerGames.IsEmptyOrNull())
                return;

            var matchFirst = queryPerGames.FirstOrDefault();

            var myService = new SingleWalletMerchantApiTestValidateService(new SwValidateTestInitialConfigParameter
            {
                MerchantID = targetMerchantID,
                IsDisableNotify = true,
                BetRecordNoPGS_AlreadySettle = matchFirst.TransactionNo,
            });

            return;
        }
    }
}
