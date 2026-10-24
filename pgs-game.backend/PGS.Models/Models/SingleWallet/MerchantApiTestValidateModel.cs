using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using target.Models.Table;

namespace target.Models.Models
{
    public class SingleWalletMerchantApiTestValidateTestData
    {
        public string Setting_ApiEndpoint { get; set; }

        public string Setting_PGS_ValidateFixedApiToken { get; set; }

        public int MerchantID { get; set; }

        public int PlayerID { get; set; }

        public string PlayerUniqueName { get; set; }

        public int GameID { get; set; }

        public string BetRecordNoPGS_AlreadySettleRequest { get; set; }

        public string BetRecordNoPGS_PendingWager { get; set; }

        public string Lastest_SessionToken { get; set; }

        public string Lastest_SessionTokenRaw { get; set; }

        public bool IsDisableNotify { get; set; }


        public PerGameDetailLogEntity ExistData_SettleAlready { get; set; }

        public SingleWalletMerchantFixedTestData FixedTestData { get; set; }

    }

    /// <summary>
    /// Service請參考 SingleWalletMerchantApiTestValidateService
    /// </summary>
    public class SwValidateTestInitialConfigParameter
    {
        public int MerchantID { get; set; }

        public string BetRecordNoPGS_AlreadySettle { get; set; }

        public bool IsDisableNotify { get; set; }

    }


    public class SingleWalletMerchantFixedTestData
    {
        public string Unknow_BetRecordNoPGS { get; set; }

        public string Unknow_PlayerUniqueID { get; set; }

        public string Unknow_SessionToken { get; set; }

        public int Default_GameID { get; set; }

        public string DebugProcessUUID { get; set; }
    }

}
