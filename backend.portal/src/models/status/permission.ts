export enum AllPermissionToken {
	/**
	 * 首页公告
	 */
	Home = 'Home',

	MembersManagementList = 'MembersManagementList',
	/**
	 * 用户列表-查詢
	 */
	MembersManagementList_Search = 'MembersManagementList_Search',
	/**
	 * 用户列表-封號 */
	MembersManagementList_Lock = 'MembersManagementList_Lock',

	/**
	 * 用户查询-查詢 */
	MembersManagementDetail_Search = 'MembersManagementDetail_Search',
	/**
	 * 用户查询-封號 */
	MembersManagementDetail_Lock = 'MembersManagementDetail_Lock',

	/* 用户查询-顯示統計數據 */
	MembersManagementDetail_GameSummary = 'MembersManagementDetail_GameSummary',

	/* 用户查询-會員進階編輯 */
	MembersManagementDetail_AdvanceUpdate = 'MembersManagementDetail_AdvanceUpdate',
	

	MembersManagementDetail_AllowSimulateGameLaunch = 'MembersManagementDetail_AllowSimulateGameLaunch',


	/**
	 * 维护开关-查詢 */
	GameSettingSwitch_Search = 'GameSettingSwitch_Search',
	/**
	 * 维护开关-修改 */
	GameSettingSwitch_Update = 'GameSettingSwitch_Update',

	/**
	 * 帐号列表-查詢 */
	OperatorManagementAccounts_Search = 'OperatorManagementAccounts_Search',
	/**
	 * 帐号列表-新增帐号 */
	OperatorManagementAccounts_Create = 'OperatorManagementAccounts_Create',
	/**
	 * 帐号列表-编辑帐号 */
	OperatorManagementAccounts_Update = 'OperatorManagementAccounts_Update',
	/**
	 * 帐号列表-删除帐号 */
	OperatorManagementAccounts_Delete = 'OperatorManagementAccounts_Delete',

	/**
	 * 组织管理-查詢 */
	SystemSettingOrganizations_Search = 'SystemSettingOrganizations_Search',
	/**
	 * 组织管理-新增组织 */
	SystemSettingOrganizations_Create = 'SystemSettingOrganizations_Create',
	/**
	 * 组织管理-编辑组织 */
	SystemSettingOrganizations_Update = 'SystemSettingOrganizations_Update',
	/**
	 * 组织管理-删除组织 */
	SystemSettingOrganizations_Delete = 'SystemSettingOrganizations_Delete',

	/**
	 * 商户管理-查詢 */
	SystemSettingMerchant_Search = 'SystemSettingMerchant_Search',
	/**
	 * 商户管理-新增 */
	SystemSettingMerchant_Create = 'SystemSettingMerchant_Create',
	/**
	 * 商户管理-修改 */
	SystemSettingMerchant_Update = 'SystemSettingMerchant_Update',
	/**
	 * 商户管理-刪除 */
	SystemSettingMerchant_Delete = 'SystemSettingMerchant_Delete',


	System_AccountingInvoice_ExportInvoice = 'System_AccountingInvoice_ExportInvoice',




	SystemSetting_GameList = 'SystemSetting_GameList',
	/* 遊戲管理 新增 */
	SystemSetting_GameList_Create = 'SystemSetting_GameList_Create',

	SystemSetting_GameList_Update = 'SystemSetting_GameList_Update',

	/**
	 * 服务器管理-查詢 */
	SystemSettingServers_Search = 'SystemSettingServers_Search',
	/**
	 * 服务器管理-新增 */
	SystemSettingServers_Create = 'SystemSettingServers_Create',
	/**
	 * 服务器管理-修改 */
	SystemSettingServers_Update = 'SystemSettingServers_Update',
	/**
	 * 服务器管理-刪除 */
	SystemSettingServers_Delete = 'SystemSettingServers_Delete',
	/**
	 * 服务器管理-分配商戶 */
	SystemSettingServers_Dispatch = 'SystemSettingServers_Dispatch',

	/**
	机器人配置-查詢
	 */
	SystemSettingServerRobotConfig_Search = 'SystemSettingServerRobotConfig_Search',

	/**
	机器人配置-新增
	 */
	SystemSettingServerRobotConfig_Create = 'SystemSettingServerRobotConfig_Create',
	/**
	 * 机器人配置-修改 */
	SystemSettingServerRobotConfig_Update = 'SystemSettingServerRobotConfig_Update',
	/**
	 * 机器人配置-刪除 */
	SystemSettingServerRobotConfig_Delete = 'SystemSettingServerRobotConfig_Delete',

	/**
	 * 角色管理-查詢 */
	SystemSettingRoles_Search = 'SystemSettingRoles_Search',
	/**
	 * 角色管理-新增 */
	SystemSettingRoles_Create = 'SystemSettingRoles_Create',
	/**
	 * 角色管理-修改 */
	SystemSettingRoles_Update = 'SystemSettingRoles_Update',
	/**
	 * 角色管理-刪除 */
	SystemSettingRoles_Delete = 'SystemSettingRoles_Delete',
	/**
	 * 角色管理-名稱修改 */
	SystemSettingRoles_Rename = 'SystemSettingRoles_Rename',

	/** table link 用户查询 */
	MembersManagementDetail = 'MembersManagementDetail',

	/** table link 金幣紀錄 */
	MembersManagementMoneyRecord = 'MembersManagementMoneyRecord',


	MembersManagement_GameVendorTransfer_Search = 'MembersManagement_GameVendorTransfer_Search',
	MembersManagement_GameVendorTransfer_Advance = 'MembersManagement_GameVendorTransfer_Advance',



	SystemSetting_EventPrizeRecord = 'SystemSetting_EventPrizeRecord',



	SystemSetting_Event_ManualSendPrize = 'SystemSetting_Event_ManualSendPrize',


	/**  table link 游戏历程查询 */
	GamesManagementRecord = 'GamesManagementRecord',

	GamesManagement_BetRecord_ManualRefund = 'GamesManagement_BetRecord_ManualRefund',

	GamesManagement_BetRecordSearch = 'GamesManagement_BetRecordSearch',
	
	GamesManagement_BetRecordSearch_Export = 'GamesManagement_BetRecordSearch_Export',

	GamesManagement_BetRecord_ManualSettleBet = 'GamesManagement_BetRecord_ManualSettleBet',

	Statistics_EazyQuery_QueryInvoiceEachMerchantDetail = 'Statistics_EazyQuery_QueryInvoiceEachMerchantDetail',



	/**  遊戲歷程 管理員進階權限 */
	GamesManagementAdminPermission = 'GamesManagementAdminPermission',


	Agent_MerchantCredit_UpdateGameOpenFlag = 'Agent_MerchantCredit_UpdateGameOpenFlag',

	Agent_MerchantCredit_GameOpenFlag_EnableOnly = 'Agent_MerchantCredit_GameOpenFlag_EnableOnly',

	/**  遊戲歷程 管理員進階權限 */
	SystemSetting_SystemConfigSetting = 'SystemSetting_SystemConfigSetting',
	SystemSetting_SystemConfigSetting_Update = 'SystemSetting_SystemConfigSetting_Update',


	Agent_MerchantCredit_ExportApiKey = [REDACTED]',
	Agent_MerchantCredit_BatchExportApiKey = [REDACTED]',
	Agent_MerchantCredit_Update = 'Agent_MerchantCredit_Update',
	Agent_MerchantCredit_CreateChildAgent = 'Agent_MerchantCredit_CreateChildAgent',
	Agent_MerchantCredit_EditStatus = 'Agent_MerchantCredit_EditStatus',
	Agent_MerchantCredit_EditAdvanceSetting = 'Agent_MerchantCredit_EditAdvanceSetting',
	Agent_MerchantCredit_AllowSetWalletModeWhenCreate = 'Agent_MerchantCredit_AllowSetWalletModeWhenCreate',
	Agent_MerchantCredit_AllowSetIMChannelID = 'Agent_MerchantCredit_AllowSetIMChannelID',
	Agent_MerchantCredit_SyncSubAgent = 'Agent_MerchantCredit_SyncSubAgent',	
	Agent_MerchantCredit_TriggerSwValidateTest = 'Agent_MerchantCredit_TriggerSwValidateTest',
	Agent_MerchantCredit_CreateDevMerchant = 'Agent_MerchantCredit_CreateDevMerchant',

	Merchant_SelfUpdateSetting_Update = 'Merchant_SelfUpdateSetting_Update',

	Other_AccountManagement_Create = 'Other_AccountManagement_Create',
	Other_AccountManagement_Copy = 'Other_AccountManagement_Copy',
	Other_AccountManagement_EditStatus = 'Other_AccountManagement_EditStatus',
	Other_AccountManagement_ResetPWAndValidChannel = 'Other_AccountManagement_ResetPWAndValidChannel',

	Other_DownloadResource_ApiDocument = 'Other_DownloadResource_ApiDocument',

	Special_BetAmount_FlowAmount_Invisible = 'Special_BetAmount_FlowAmount_Invisible',
	Special_RTP_Invisible = 'Special_RTP_Invisible',
	Special_WinLose_Invisible = 'Special_WinLose_Invisible',

	Special_AllowToUseExperimentFeature = 'Special_AllowToUseExperimentFeature',
	Special_HideBeforeAfterAmount = 'Special_HideBeforeAfterAmount',

	/** 僅開放技術使用，可以使用技術人員特殊功能 **/
	Special_AllowToUseDevelopeFeature = 'Special_AllowToUseDevelopeFeature',
	Special_DefaultEnableSearchDetailCheckbox = 'Special_DefaultEnableSearchDetailCheckbox',
	Special_PGS_CustomerService= 'Special_PGS_CustomerService',
	Special_ExportMaxSizeRestrict = 'Special_ExportMaxSizeRestrict',
	Special_AM_Permission = 'Special_AM_Permission',

	SystemSetting_Merchant_IPAddressTemplate_AdvanceEdit = 'SystemSetting_Merchant_IPAddressTemplate_AdvanceEdit',
	SystemSetting_Merchant_IPAddressTemplate_AppendNewIP = 'SystemSetting_Merchant_IPAddressTemplate_AppendNewIP',
	SystemSetting_Merchant_IPAddressTemplate_Delete = 'SystemSetting_Merchant_IPAddressTemplate_Delete',

	Other_BOAnnouncement_CreateUpdate= 'Other_BOAnnouncement_CreateUpdate'

}
