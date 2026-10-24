using System;
using System.Collections.Generic;
using System.Text;

namespace target.Models
{
    public class SystemConstant
    {
        public const int JwtToken_Expired_Seconds = 86400;

        public const string JwtToken_SaltKey = "y7dw2t6tvc2ujel618";

        public const string Password_SaltKey = "8f3c0a484f9fc1";

        public const string ThirdPartyMD5_SaltKey = "4942335a69c2";

        public const string PublicAPI_Singkey_SaltKey = "2b3c7342aadf";

        public const string PublicAPI_Lobby_Singkey_SaltKey = "z85pc7w42a6dc";

        /// <summary>
        /// public API，萬用加密Key
        /// </summary>
        public const string PublicAPI_SignKey_MagicPassword = [REDACTED]";

        public const string CheckCode_MagicPassword = [REDACTED]";

        public const string ExternalAPI_SignKey_MagicHashKey = "f28d291b-07f3-486b-8799-602819f0df86";

        public const string DevEnvOnly_MagicKey = "DEV_TEST_API_KEY";

        public const string Default_EazyPassword = [REDACTED]";

        public const int MaintainAllVendors = 9999999;

        public const string HeaderKey_AccessToken = [REDACTED]";

        public const string QueueGroup_Common = "Common";
        public const string QueueGroup_CreateRound = "CreateRound";
        public const string QueueGroup_SettleRound = "SettleRound";


        /// <summary>
        /// 大部分報表的結算基準
        /// </summary>
        public const double PgsDefaultSettlementTimezone = 8.0;

        /// <summary>
        /// 站台關閉的delay時間
        /// </summary>
        public const int ApplicationShutdownDelaySeconds = 60;

        public const string ApiUrlRemoveAllCache = "internal/cache/removeAll";

        public const string DefaultCurrency = "CNY";

        public const string DefaultLang = "zh-cn";

        public const int JwtTokenVersionAdminSys = 2001;

        public const int JwtTokenVersionPlayer = 1001;

        public const string CompanyIPAddress = "0.0.0.0";

        public const int DefaultMerchantID = 175001;

        public const int SkipValidUserID = -5566;

        public const int DefaultRoundSettleSeconds = 30;


        /// <summary>
        /// 玩家帳號字串長度
        /// </summary>
        public const int PlayerAccountMinLength = 3;

        public const int PlayerAccountMaxLength = 50;


        /// <summary>
        /// 玩家密碼字串長度
        /// </summary>
        public const int PlayerPasswordMinLength = 6;

        public const int PlayerPasswordMaxLength = 30;



        //各環境名稱定義 (判斷的時候請轉小寫)
        public const string Env_Name_Dev = "dev"; //local主要是用這個
        public const string Env_Name_Development = "development";
        public const string Env_Name_QA = "qa";
        public const string Env_Name_Production = "production";


        public const int CrazyFishingDefaultGameID = 310001;

        public const string CrazyFishingDefaultGameCode = "happyfishing";


        public const decimal UnlimitCreditAmount = 9999999;


        public readonly static List<string> RD_ComputerNames = new List<string>
        {
            "DESKTOP-EMCG6HU"
        };

        public const string EmptyAddressHash = "0x0000000000000000000000000000000000000000";
        public const string Hex_Value0 = "0x0";
        public const string Hex_Value1 = "0x1"; //sucess

    }
}
