using game.Entity.Game;
using target.Models.Enums;
using target.Utility;
using target.Utility.IOC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace target.Testing
{

    public class TestDataConfig
    {
        public TestSampleDataModel Default => new TestSampleDataModel();

        public TestSampleData_SingleWallet SingleWallet => new TestSampleData_SingleWallet();

        public TestSampleData_ForbiddenCreateAccountMerchant ForbiddenCreateAccountMerchant => new TestSampleData_ForbiddenCreateAccountMerchant();

        public TestSampleData_W88 W88 => new TestSampleData_W88();

        public TransferReate100MerchanteDataModel TransferRate100 => new TransferReate100MerchanteDataModel();

        public TestSampleData_Not_SuperUser_Model NotSuperMerchant => new TestSampleData_Not_SuperUser_Model();

        public TestSampleData_Not_LobbyTransfer_Model LobbyTransferMerchant => new TestSampleData_Not_LobbyTransfer_Model();

        public TestSampleData_SeldomUseForTest Seldom_Use => new TestSampleData_SeldomUseForTest();

        public TestSampleData_CreditAgent CreditAgent => new TestSampleData_CreditAgent();


        public TestSampleData_GameVendor ExternalGameVendor => new TestSampleData_GameVendor();
    }



    public class TestSampleDataModel
    {

        public int UserID = 137;

        public string UserName = "unit_test_001";

        public string UserPassword = [REDACTED]";

        public int PlayerID = 1000035;

        public string PlayerName = "unit_test_001";

        public int ServerID = 999999;

        public int ServerID_2 = 102300410;

        //請務必是PGS自製遊戲
        public int GameID = (int)GameListEnum.DFDC_多福多财;

        public int GameID_AGG = 290030;

        public int GameID_Funky = 250014;

        public int RoleID = 1; //超級管理員


        public List<int> SupportRaiseProcessGameIDs = new List<int>
        {
            390083,
            390036,
            390038,
            390042,
            390062
        };

        public int ThirdPartyGameID = 250001;

        public long TradeID = 20200100000031136;

        public int MerchantID = 175001;

        public string MerchantSecret = [REDACTED]";

        public string MerchantGuid = "032071cb-df86-4aff-b988-92d6b87f42cf";

        public int MerchantSelectTemplateID = 350287;

        public string Language_Text_Test = "GameList_fun_10291";

        public int OrganizationID = 11;

        public string OrganizationCode = "target";

        public int VendorConfigID = 50004;

        public string IPAddress = "0.0.0.0";


        public int EventID_Delete = 250013;

        public int EventDrawPrizeID = 82502;

        public int IMChannelTagID = 50001;

        public int IMChannelID = 10001;


        /// <summary>
        /// 下方這幾個資料建議不要刪除
        /// 這筆單所對應的slotHistory資料也別刪
        /// </summary>
        public int SlotGamePerGameDetailLogID = 4514475;

        /// <summary>
        /// 必須pgs遊戲、並且是 175001 的注單、
        /// </summary>
        public string TransactionNo_PgsBet = "32I1d797edb8b698418b8045a60ed1b6";

        public string TransactionNo_ForRefund = "1B127cfd86a843594b1ca0ba89139d9e";

        public string TransactionNo_ForManualSettle = "1B1295f19f397408442883ef14a6f86b";

        /// <summary>
        /// 單元測試刪測試資料 可能會判斷比對此欄位相同 刪除資料
        /// </summary>
        public string DisplayName__MustDelete = "unitTest建立";

        /// <summary>
        /// 單元測試刪測試資料 可能會判斷比對此欄位相同 刪除資料
        /// </summary>
        public long TradeID_MustDelete = 20190100000071179;

        /// <summary>
        /// 單元測試刪測試資料 可能會判斷比對此欄位相同 刪除資料
        /// </summary>
        public decimal BetAmount_MustDelete = 3377;



        /// <summary>
        /// 如果有其他要略過的日期指定。建議定一個新的變數。或是刪除掉之前有使用function使用的地方
        /// </summary>
        public DateTime Date_NewGameVendor_Intergrate = new DateTime(2022, 12, 15);  //jili或未來其他新的三方

    }


    public class TestSampleData_Not_SuperUser_Model
    {
        public int OrganizationID = 12;

        public int UserID = 144;

        public string UserName = "AE-MSTest088";

        public string UserPassword = [REDACTED]";

        public int EventID_RedEnvelope = 250059;

        public int EventID_LuckyWheel = 255440;

        public int EventID_RedEnvelope_PrizeCountRestrict = 254803;

        public int RoleID = 746; //營運平台站長

        public int EventID = 250010;

        public int EventGameID = 290089;

        /// <summary>
        /// 對應商戶: unit_test專用_請勿修改
        /// </summary>
        public int MerchantID = 175010;

        public string MerchantSecret = [REDACTED]";

        public int PlayerID = 1000217;

        public string PlayerName = "unit_test_003";
    }

    public class TestSampleData_ForbiddenCreateAccountMerchant
    {
        /// <summary>
        /// 對應商戶: MSTest_禁止創帳號的商戶
        /// </summary>
        public int MerchantID = 177603;

        public string MerchantSecret = [REDACTED]";

    }


    public class TestSampleData_Not_LobbyTransfer_Model
    {

        /// <summary>
        /// 對應商戶: unit_test專用_請勿修改
        /// </summary>
        public int MerchantID = 177098;

        public string MerchantSecret = [REDACTED]";

        public int PlayerID = 1019779;

        public string PlayerName = "unit_test_0056";
    }



    public class TestSampleData_CreditAgent
    {
        public int UserID = 147;

        public string UserName = "ag1-admin";


        public int UserID_Layer2 = 150;

        public string UserName_Layer2 = "ag1-A-layer2";


        /// <summary>
        /// 信用模式-總代
        /// </summary>
        public int MerchantID = 175565;

        /// <summary>
        /// 二級代理: 
        /// </summary>
        public int Layer2_MerchantID = 175566;

        /// <summary>
        /// 三級代理: 
        /// </summary>
        public int Layer3_MerchantID = 175574;



        public string MerchantSecret = [REDACTED]";
    }



    public class TestSampleData_W88
    {
        public int MerchantID = 175888;

        public int PlayerID = 1017588;
    }



    /// <summary>
    /// 較少使用的特殊案例 特別隔離開 (避免跟其他案例混再一起互相影響)
    /// </summary>
    public class TestSampleData_SeldomUseForTest
    {

        public int PlayerID = 1000218;

        public string PlayerName = "unit_test_gs";

    }


    public class TestSampleData_SingleWallet
    {
        public int MerchantID = 175022;

        public string MerchantSecret = [REDACTED]";

        public int PlayerID = 1015906;

        public decimal BalanceAmount = 99999;

        public string PlayerName = "UnitTest_SW001";

        /// <summary>
        /// 這種token沒有過期的問題，純粹只是要能確認是對方的token即可
        /// </summary>
        public string SessionTokenRaw = "ab184bbf-99bf-4ff6-a09d-280e1695eafe";
        public string SessionTokenPGS = "f33f5ad8-d8fd-4856-83bf-baad0fe002a3";

        public string BetRecordNo = "HHF21013114014694149gp3";

    }


    public class TestSampleData_GameVendor
    {
        public int GameID = 290030; //星月魔女

        public int GameID_AGGGame = 290089; //強制要求這個變數需要是AGG遊戲
    }


    public class TransferReate100MerchanteDataModel
    {

        public int MerchantID = 178518;

        public string MerchantSecret = [REDACTED]";

        public decimal TransferConvertRate = 100;

    }


}