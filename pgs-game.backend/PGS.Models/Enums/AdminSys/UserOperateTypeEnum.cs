using target.Models.CultureText;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace target.Models.Enums
{

    /// <summary>
    /// UserOperateRecord 這張表在用的
    /// </summary>
    public enum UserOperateTypeEnum
    {       
        User_UserLogin = 7777,

        Player_LockStatus = 1001,

        Player_UnlockStatus = 1002,

        Player_RoomLockRemove = 1011,

        Player_LaunchGameFromAdminSys = 1029,

        Player_RemoveTag = 1333,

        Player_ManualDepositWalletAmount = 1444,


        /// <summary>
        /// 手動關閉注單 並退還金幣
        /// </summary>
        SingleWallet_CancelRefundWalletManual = 2051,

        /// <summary>
        /// 手動結算注單
        /// </summary>
        SingleWallet_ManualSettleBetRecord = 2052,

        /// <summary>
        /// 手動補救perGameLog
        /// </summary>
        BetRecordManualWritePerGameLog = 2053,

        BetRecordManualChangeSettleAmount = 2059,

        BetRecordRecoverNotifyQueue = 2071,






        UpdateIPAddressTemplate = 3322,

        UpdateGameRTP = 3501,

        UpdatePlayerRTP = 3502,

        ResetLargePlayersRTP = 3507,

        ResePlayersRTPByEvent = 3509,

        UpdateCurrencyRate = 3518,

        UpdateMerchantAdvanceSetting = 3538,

        UpdateMerchantStatus = 3541,

        Merchant_UpdateMerchant = 3542,

        DeleteMerchant = 3545,

        ManualTriggerSendRankPrize = 4501,

        TriggerBackendButton = 4833,

        TriggerSingleWalletValidateTest = 4857,

        BatchUpdateMerchantCustomized = 5101,

        Agent_CreateNewMerchantByAgent = 5561,

        Agent_AgentMigrate = 5565,

        Agent_AgentHierarchyUpdate = 5566,


        Agent_ExportMerchantCredential = 5588,

        SearchPlayerSuspicoiusCheck = 5888,

        UpdateMerchantCreditAmount = 6101,

        UpdateRolePermission = 6122,

        UpdateGameSettingByAgentSelf = 6135,

        UpdateGameSettingByGameSwitchPage = 6136,

        UpdateGameSettingMultipleEdit = 6137,

        UpdateIMChannelBasic = 6291,

        UpdateIMChannelSettings = 6301,

        UpdateTagAssigner = 6501,

        UpdateUserPassword = [REDACTED],

        CopyUser = 7105,

        User_Create = 7113,

        User_Update = 7114,
        
        User_UpdateUserStatus = 7119,

        User_AutoLockUserStatus = 7339,

        SendTgAnnouncement = 8101,

        RemoveTgAnnouncement = 8105,
    }
}
