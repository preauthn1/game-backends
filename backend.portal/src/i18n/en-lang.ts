export const Menu = {
	undefined: '',
	Home: 'Home',

	MembersManagement: 'Player Manage',
	MembersManagementList: 'Player List',
	MembersManagementList_Search: 'Entry & Query',
	MembersManagementList_Lock: 'Block',

	MembersManagementDetail: 'Player Detail',
	MembersManagementDetail_Search: 'Entry & Query',
	MembersManagementDetail_Lock: 'Block',
	MembersManagementDetail_GameSummary: 'View Game Summary',
	MembersManagementDetail_AdvanceUpdate : 'Member Advance Edit',

	MembersManagementMoneyRecord: 'Money Record',
	MembersManagementBetRecord: 'Player Room Summary Record',
	MembersManagementLock: 'Block Record',
	MembersManagementTransfer: 'Transfer Record',


	MembersManagement_GameVendorTransfer_Management: 'Transfer Record (Special)',
	MembersManagement_GameVendorTransfer_Search: 'Transfer Record (Fishing) search',
	MembersManagement_GameVendorTransfer_Advance: 'Transfer Record (Fishing) advance',
	

	MembersManagementDetail_AllowSimulateGameLaunch  : 'Allow simulate player game launch',
	MembersManagementMoneyRecord_Search   : 'Money Record basic search',
	MembersManagement_MoneyRecord_ForceFilterPlayer : 'Money Record force filter player',


	MembersManagementThirdPartyGameLoginLogList: 'Third-party game launch history',
	
	Other_AccountManagement : 'Account Management',
	Other_AccountManagement_Search : 'Account search',
	Other_AccountManagement_Create : 'Account create',
	Other_AccountManagement_Copy : 'Account Copy',
	Other_AccountManagement_EditStatus: 'Account-Edit status',
	Other_AccountManagement_ResetPWAndValidChannel: 'Reset password and channel verification (for CS)',
	

	Other_BOAnnouncement_Management: 'BO Announcement',
	Other_BOAnnouncement_List: 'BO Announcement List',
	Other_BOAnnouncement_CreateUpdate: 'BO Announcement Update',

	Other_DownloadResource_Page : 'Resource download',
	Other_DownloadResource_ApiDocument : 'API Document download',

	GameManagement: 'Game Data',
	GamesManagementRecord: 'Game Histories',
	GamesManagementServersHealthSummary: 'Servers Status',
	GamesManagement_BetRecordSearch : 'Search BetRecord ',
	GamesManagement_BetRecordSearch_SearchList : 'Search BetRecord List',
	GamesManagement_BetRecord_ManualRefund : 'Manual Refund BetRecord',
	GamesManagement_BetRecord_ManualSettleBet : 'Manual Settlement',
	GamesManagement_BetRecordSearch_UnionBetRecord : 'Game Vendor BetRecord',
	GamesManagement_BetRecordSearch_Export : 'Export BetRecord (ThirdParty)',
	GamesManagement_DynamicQuery: 'Eazy Query',
	GamesManagement_NotifyQueueList : 'Seamless Notify Queue List',

	GameManagement_AGGJpAmount : 'Game jackpot search',
	System_CustomerServiceTool : 'Customer Service Tool',

	GameSetting: 'Game Confign',
	GameSettingSwitch: 'Maintance Switch',
	GameSettingSwitch_Search: 'Entry & Query',
	GameSettingSwitch_Update: 'Modify Switch',



	EventManagement : 'Event Management',
	SystemSetting_EventManagement: 'Event List',
	SystemSetting_EventManagement_Search: 'Event List Search',
	SystemSetting_Event_ManualSendPrize: 'Event Manual Send Prize',
	SystemSetting_EventDetail: 'Event Detail',
	SystemSetting_Event_DrawPrize : 'Event Draw Prize Management',

	SystemSetting_EventPrizeRecord: 'EventPrize Record',
	SystemSetting_GameVendorRTPManagement: 'RTP Management',
	SystemSetting_Announcement: 'Announcement Management',


	SystemSetting_GeneralTag: 'Tag management', 
	SystemSetting_GeneralTag_ListSearch: 'Tag management(search)', 	
	

	SystemSetting_OneTimeJob: 'One times job',


	SystemSetting_Merchant_IPAddressTemplate: 'IPddress whitelist',
	SystemSetting_Merchant_IPAddressTemplate_Search: 'IPddress whitelist search',
	SystemSetting_Merchant_IPAddressTemplate_AdvanceEdit : 'IPddress whitelist Advance Edit',
	SystemSetting_Merchant_IPAddressTemplate_AppendNewIP: 'IPddress whitelist Append new ip',	
	SystemSetting_Merchant_IPAddressTemplate_Delete: 'IPddress whitelist Delete',

	Statistics: 'Statistics',
	StatisticsMerchantSummary: 'Data Report',
	StatisticsGame: 'Game Report',
	Statistics_MerchantDailySummary: 'Daily Report',
	Statistics_PlayerGameSummary: 'Player Game Report',
	Statistics_RetentionRate: 'Retention Report',
	Statistics_MerchantAllPlayerWinLoss: 'Merchant player winloss report',
	Statistics_GameVendorFinanceCompareReport : 'Game Vendor Compare Report',
	Statistics_AgentHierarchyDailySummary : 'Agent Daily Report',
	Statistics_AgentHierarchy_TotalReport: 'Agent Total Report',
	Statistics_MerchantWinLossRankingReport: 'Player WinLoss Ranking',
	Statistics_AgentHierarchyDailyGameReport : 'Agent Daily Game Report',
	Statistics_AgentSpecialReport : 'Agent Special Report',
	Statistics_FreespinReport: 'Event Summary Report',
	Statistics_FreespinStatusList: 'Freespin Status List',

	Statistics_EazyQuery_ForPartner: 'Additional query (for partners)',
	Statistics_EazyQuery_QueryInvoiceEachMerchantDetail: 'Allow querying monthly invoice and each merchant details',

	OperatorManagement: 'Account Manage',
	OperatorManagementAccounts: 'Account List',
	OperatorManagementAccounts_Search: 'Entry & Query',
	OperatorManagementAccounts_Create: 'Create',
	OperatorManagementAccounts_Update: 'Edit',
	OperatorManagementAccounts_Delete: 'Delete',
	OperatorManagementAccounts_AllowViewAllOrgAccounts: 'Allow view all userAccount with same Org',

	Agent_Management : 'Agent Management',
	Agent_MerchantCredit : 'Agent list',
	Agent_MerchantCredit_Search : 'Agent Credit Search',
	Agent_MerchantCredit_Update : 'Agent Credit Update',
	Agent_MerchantCredit_CreateChildAgent : 'Create sub Agent',
	Agent_MerchantCredit_EditStatus : 'Edit Merchant Status',
	Agent_MerchantCredit_EditAdvanceSetting: 'Edit Advance Setting',
	Agent_MerchantCredit_ExportApiKey: [REDACTED] Api Key',
	Agent_MerchantCredit_BatchExportApiKey: [REDACTED] Api Key(Batch)',
	Agent_MerchantCredit_AllowSetWalletModeWhenCreate: 'Allow set walletMode when create sub agent',
	Agent_MerchantCredit_AllowSetIMChannelID: 'Allow set IMChannel when create sub agent',
	Agent_MerchantCredit_DisableCreateLayer2Agent: 'Disable create layer2 agent',
	Agent_MerchantCredit_SyncSubAgent: 'Sync sub agent game-flag',
	Agent_MerchantCredit_TriggerSwValidateTest: 'Trigger Seamless Validate Test',
	Agent_MerchantCredit_CreateDevMerchant: 'Create Dev Merchant',

	Agent_MerchantCreditChangeLog: 'Credit Change Logs',
	Agent_MerchantHierarchyTreeView : 'Agent tree view',

	Agent_MerchantCredit_UpdateGameOpenFlag: '(Choose one)Edit Agent Game Setting',
	Agent_MerchantCredit_GameOpenFlag_EnableOnly: '(Choose one)Edit Agent Game Setting (enable only)',


	System_AccountingInvoice_Basic : 'AccountingInvoice Basic',
	System_AccountingInvoice_ExportInvoice : 'AccountingInvoice ExportInvoice & search',

	SystemSetting: 'System Settings',
	SystemSettingOrganizations: 'Org Manage',
	SystemSettingOrganizations_Search: 'Entry & Query',
	SystemSettingOrganizations_Create: 'Create',
	SystemSettingOrganizations_Update: 'Edit',
	SystemSettingOrganizations_Delete: 'Delete',

	SystemSettingMerchant: 'Merchant Manage',
	SystemSettingMerchant_Search: 'Entry & Query',
	SystemSettingMerchant_Create: 'Create',
	SystemSettingMerchant_Update: 'Edit',
	SystemSettingMerchant_Delete: 'Delete',

	System_Merchant_CustomizedSettingList: 'Merchant Advance Setting List',

	SystemSettingServers: 'Server Manage',
	SystemSettingServers_Search: 'Entry & Query',
	SystemSettingServers_Create: 'Create',
	SystemSettingServers_Update: 'Edit',
	SystemSettingServers_Delete: 'Delete',
	SystemSettingServers_Dispatch: 'Dispatch',
	SystemSettingServers_DispatchBatch: 'Dispatch',


	SystemSetting_LanguageText_Management : 'Language Text Management',
	SystemSetting_LanguageText_List : 'Language Text List',


	SystemSetting_GameList: 'GameList manage',
	SystemSetting_GameList_Search: 'GameList search',
	SystemSetting_GameList_Create: 'GameList create',
	SystemSetting_GameList_Update: 'GameList update',

	SystemSetting_VendorMaintenaceManage: 'Vendor Maintenance management',
	SystemSetting_VendorMaintenace: 'Vendor Maintenance List',
	SystemSetting_VendorMaintenaceCreate: 'Create Vendor Maintenance List',


	SystemSetting_VendorConfig: 'VendorConfig Management',
	SystemSetting_VendorConfigList: 'VendorConfig List',
	

	SystemSetting_SystemConfigSetting_Search : 'Global System Config ',
	SystemSetting_SystemConfigSetting_Update : 'Global System Config Edit',
	
	SystemSetting_IMChannelList: 'IMChannel List',

	System_AccountingInvoice : 'Inovice Management',

	GamesManagementViewList : 'Bet Record View List',
	GamesManagementAdminPermission : 'Admin Permission',


	SystemSetting_PayTableImageManagmenet : 'PayTable image management',
	SystemSetting_PayTableImageList: 'PayTable image list',

	Merchant_SelfUpdateSetting: 'Other',
	Merchant_SelfUpdateSetting_GameSwitch : '(Discard)Merchant lobby visible games',
	Merchant_SelfUpdateSetting_Search: 'Merchant game settings search',
	Merchant_SelfUpdateSetting_Update: 'Merchant game settings update',


	SystemSettingServerRobotConfig: 'Robot Config',
	SystemSettingServerRobotConfig_Search: 'Entry & Query',
	SystemSettingServerRobotConfig_Create: 'Create',
	SystemSettingServerRobotConfig_Update: 'Edit',
	SystemSettingServerRobotConfig_Delete: 'Delete',

	SystemSettingRoles: 'Role Setting',
	SystemSettingRoles_Search: 'Entry & Query',
	SystemSettingRoles_Create: 'Create',
	SystemSettingRoles_Update: 'Edit',
	SystemSettingRoles_Delete: 'Delete',
	SystemSettingRoles_Rename: 'Rename',

	SystemSetting_SystemConfigSetting: 'System Config',
	SystemSetting_OneToManySettings: 'One to Many Settings',


	SystemLogs_SingleWalletPlatformReqeustLog: 'SingleWallet Platform Reqeust Manage',
	SystemLogs_SingleWalletPlatformReqeustLog_List: 'SingleWallet Platform ReqeustLogs',

	SystemLogs_ApiReqeustLogManagement: 'System ApiRequestLog',
	SystemSetting_TGAnnouncement : 'TG batch announcement',

		 
	SystemLogsExternalApiRequestLogManagement: 'Third Party Api Request Log',
	SystemLogsExternalApiRequestLogList: 'Third Party Api Request Log',

	
	Records_UserOperateRecord : 'Operate records',
	Records_UserOperateRecord_Search: 'Operate records Search',
	Records_UserOperateRecord_SearchAllOrg: 'Operate records view org all',

	Special_PermissionSetting: 'Special Settings',
	Special_BetAmount_FlowAmount_Invisible: 'BetAmount/FlowAmount invisible',
	Special_RTP_Invisible: 'RTP invisible',
	Special_WinLose_Invisible: 'WinLose invisible',

	Special_AlertNotifyWhenLogin : 'Alert notify when login',
	Special_ExportMaxSizeRestrict: 'Export will restrict maxSize',	

	Special_OnlyVisible_IDG_Games: 'Show IDG visiable games only',
	Special_AllowToUseExperimentFeature : 'Allow use Experiment Feature',
	Special_AllowAssignHierarchyMerchantForUser : 'Allow assign hierarchyMerchant for user',
	Special_AllowToUseDevelopeFeature: 'Allow use feature for develope only. (Warning*)',
	Special_DefaultEnableSearchDetailCheckbox: 'Default enable search detail (ReportPage)',
	Special_SuperAdminOrg_ButRestrictFeature: 'Restrict permission for this superAccount',
	Special_PGS_CustomerService: 'target Customer Service',
	Special_CanSelectedSelfRoles: 'Allow set self role when create BO account',
	Special_Account_SearchAccountByMultiMerchantAssignMode : '允許透過多種判斷條件_列出示可管理帳號(Super代理)',
	Special_AM_Permission : 'AM Level Special Permissions',
	Special_Dropdown_ShowRichmondDropdowns: 'Dropdown_Show all richmond channels',

	Special_SuperAdminOrg_ButNeedValidateWhenSubmit: 'Super Org ,but validate restrcit more when submit',

	SystemSetting_GameVendorRTPManagement_ListSearch : 'RTP Management-Search',
	SystemSetting_GameVendorRTPManagement_Update : 'RTP Management-Update',

	Special_HideBeforeAfterAmount: 'PerGameDetailLog_HideBeforeAmount',
	
};




export const TableHeader = {
	__prompt: 'Prompt',
	__lock: 'Block',
	__ID: 'ID',
	__playerID: 'Player ID',
	__playerAccount: 'Player account',
	__player: 'Player',
	__displayName: 'Display name',
	__newName: 'New name',
	__status: 'Status',
	__playerType: 'Type',
	__walletMode: 'Wallet Mode',
	__merchant: 'Merchant',
	__merchantID: 'Merchant ID',
	__merchantName: 'Merchant Name',
	__organizationName: 'Org Name',
	__organizationID: 'Org ID',
	__organization: 'Organization',
	__role: 'Role',
	__roleAndPermission: 'Role (Affect function and permission)',
	__roleID: 'Role ID',
	__roleName: 'Role name',
	__walletAmount: 'Cash amount',
	__createBy: 'Create By',
	__settleTime: 'Settlement time',
	__createTime: 'Create time',
	__createTime2: 'Create time',
	__receiveTime: 'Receive time',
	__createIPAddress: 'Register IP',
	__defaultBalanace: 'Default Balance',
	__convertRate: 'Convert Rate',
	__defaultPassword: [REDACTED] password',
	__create: 'Create time',
	__lastLogin: 'Last Login',
	__lastLoginTime: 'Last Login Time',
	__loginCount: 'Login Count',
	__lastIPAddress: 'Last login IP',
	__updateTime: 'Update time',
	__lastUpdateTime: 'Last update time',
	__lastSearchTime: 'Last search time',

	__gameInfo: 'Game Information',
	__gameName: 'Game name',	
	__code: 'Code',
	__winCount: 'Num of wins',
	__loseCount: 'Num of lost',
	__drawCount: 'Num of draws',
	__score: 'Coins variation',
	__bonusAmount: 'Bonus',
	__gameTimeSeconds: 'Game duration (hours: minutes: seconds)',
	__gameTime: 'Game duration',
	__total: 'Total',
	__currentPageOnly: 'Current page',

	__loginRecord: 'Login Record',
	__time: 'Time',
	__times: 'Times',
	__hdType: 'Login device',
	__hdBrowser: 'Browser',

	__iPAddress: 'IP address',
	__game: 'Game',
	__gameID: 'Game ID',
	__gameCode: 'Game Code',
	__gameType: 'Game Type',
	__gameVendorType: 'GameVendor',
	__server: 'Server',
	__serverID: 'Server ID',
	__serverName: 'Server name',
	__loginTime: 'Login time',
	__logoutTime: 'Logout time',

	__NotSetValue: 'Default',
	__loginAmount: 'Login coins',
	__currentAmount: 'Current coins',
	__updateAmount: 'Update Coin',	
	__winLossAmount: 'Win/Loss Coin',
	__taxAmount: 'Taxes',

	__othersMemo: 'Other memo',
	__others: 'Other',
	__memo: 'Description',
	__reason: 'Reason',
	__searchField: 'Search field',
	__searchText: 'Search text',

	__isDefault: 'Is Default',
	__isTestEnv: 'Is Test Env',
	__lockTime: 'Block time',
	__operateType: 'Block type',
	__category: 'Category',
	__subCategory: 'Sub Category',
	__updateBy: 'Operator',

	__type: 'Type',
	__transferType: 'Transfer type',
	__transactionType: 'Transaction type',
	__orderNo: 'System order number',
	__betRecordNoPGS: 'TransactionNo',
	__betRecordNo: 'Bet Order No',
	__transactionNo: 'TransactionNo',
	__merchantOrderNo: 'Merchant order number',
	__amount: 'Amount',
	__beforeAmount: 'Amount before update',
	__afterAmount: 'Amount after update',

	__perGameLogID: 'GameLog ID',
	__gameData: 'Game data',
	__enableGamesCount: 'Enable games',
	__enableGamesFilter: 'Enable this game',

	__serverStatus: 'Service Status',
	__onlinePlayer: 'OnlinePlayers',
	__profitAmount: `Today's Profit`,
	__flowAmount: 'Flowing water today',
	__zeroSkipInsert: 'Skip insert when value is zero',

	__action: 'action',

	__account: 'Account',
	__BOAccount: 'BO Account',
	__userName: 'User name',
	__displayName2: 'User display name',
	__password: [REDACTED]',
	__password2: 'Confirm password',
	__timeZoneArea: 'Time Zone',

	__organizationCode: 'Prefix',
	__merchantTotalCount: 'Num of merchants',
	__usersTotalCount: 'Num of users',
	__lang: 'Language',
	__organizationType: 'Org Type',
	__currency: 'Currency',
	__apiValidatePlayerSessionUrl: 'Validate routing (AVPSU)',
	__apiSingleWalletGetAmountUrl: 'Single wallet routing',

	__flagLevel: 'FlagID',
	__roomFlagLevel: 'RoomLevel',
	__clientAddr: 'Client connect address',
	__notifyAddr: 'Notify address',
	__httpAddr: 'Http address',
	__assignMerchantTotalCount: 'Assigned merchant',
	__minAvailableMoney: 'Admission coin',

	__minLevel: 'Min IQ',
	__maxLevel: 'Max IQ',
	__beginTime: 'Start time',
	__endTime: 'End time',
	__minRobotCount: 'Minimum number',
	__maxRobotCount: 'Maximum number',

	__finalScore: 'Settlement Amount',
	__betAmount: 'Bet Amount',
	__validAmount: 'Valid turnover',
	__payoutAmount: 'Payout',
	__merchantProfit: 'Revenue',
	__gameWinLoss: 'Total W/L',
	__playerWinLoss: 'Player winloss',

	__permission: 'Permission',
	__gameResult : 'Bet game result',

	__today: 'Today',
	__yesterday: 'Yesterday',
	__thisWeek: 'This week',
	__lastWeek: 'Last week',
	__thisMonth: 'This month',
	__lastMonth: 'Last month',
	__totalMinutes: 'Total Minutes',

	__basicInfo: 'Basic Information',
	__totalLoginPlayer: 'Num of logins (not repeated)',
	__transferAmount: 'Wallet variation',
	__transferInAmount: 'Transfer In',
	__transferOutAmount: 'Transfer Out',

	__loginInfo: 'Login Information',
	__totalLoginCount: 'Total logins',
	__newLoginPlayer: 'New logins',
	__maxLoginPlayer: 'Highest Online',
	__avgLoginPlayer: 'Average Online',

	__rate: 'Rate',
	__totalPeopleCount: 'Total count',
	__totalCount: 'Total count',
	__totalRound: 'Total round',
	__totalAmount: 'Total amount',
	__maxExportSize: 'Max export size',
	__totalBetPlayer: 'Num of players (not repeated)',
	__loseAmount: 'Player loses',
	__winAmount: 'Player wins',
	__moneyChangeAmount: 'Coins variation',

	__xTitle: 'Time',
	__loseAmount2: 'Lose Coins',
	__winAmount2: 'Win Coins',
	__jackpotPoolAmount: 'Pool',
	__tag : 'Tag',
	__eventID : 'Event ID',
	__amountOrQuantity : 'Amount/Quantity/Rate',
	__vendorConfig : 'VendorConfig',
	__jackpotTag : 'Jackpot',
	
	__referenceInfo : 'Reference Info',
	__referenceNo : 'Reference No',
	__eventName : 'Event Name',

	__bankViewPoint: 'Banker view',
	__playerViewPoint: 'Player view',
 
	__restrictCurrency: 'Restrict Currency',
	__supportCurrency: 'Support Currency',
	__supportGames: 'Support Games',
	__supportMerchants: 'Support Merchant',
	
	__extraInfo : 'Extra info',
	__imChannelName: 'IM Channel Name',
	__extraFilterType : 'Extra filter',
	
	__title : 'Title',
	__description : 'Description',
	__memoOrInternalDisplayName : 'memo／internal display name',
	__internalDisplayName : 'Internal display name',
	__playerLanguageDistribute : 'Player language distribution',
	__playerDeviceDistribute : 'Player device distribution',
	
	__merchantAndAllChilds: 'Agent & childs',
	__imageUrl: 'Image url',
	__barChart : 'bar chart graph',
	__pieChart: 'Pie chart',
	__device : 'device',
	__rtp : 'RTP',
	__winRate: 'Win Rate',	
	__profitRate: 'Profit Rate',
	__futureTime: 'Future',
	__recentDailySummary : 'Recent daily summary',
	__keyword: 'keyword',
	__keywordSearch: 'keyword search',
	__searchType: 'search type',	
	__parent: 'Parent',
	__parentLine: 'Parent／upline',
	__creditAmount: 'Credit amount',
	__adjustCreditAmount: 'Adjust credit amount',
	__yourRemainingCreditAmount: 'your credit amount',
	__agentRemainingCreditAmount: 'agent credit amount',
	__agent: 'agent',
	__isUnlimit: 'is unlimit credt',
	__unlimitCredit: '無限額度',
	__operationType: 'operation type',
	__adjustValue: 'adjust value', 

	__beforeValue : 'before',
	__afterValue : 'after',
	__merchantOrAgent : 'Merchant(Agent)',
	__directCustomerData: 'Direct customer data',
	__promotionBonus: 'Promotion',
	__promotionCost: 'Promotion Cost',
	__promotionBonusReceive: 'Promotion receive',
	__merchantProfitWithPromotionCost: 'Revenue (Include Promotion Cost)',
	__detailOfSingleDate : 'Detail of single date',
	__detailOfFishGame : 'Detail of fish game',
	__FishingGame : 'Fishing',
	__currentGameRoom : 'current game room',	
	__excludeZeroAmount : 'hide useless data (amount=0)',

	__sendTarget: 'Send target',
	__FourDecimalPlaces: '4 decimal places',
	__layoutOrientation: 'Orientation',
	
	__IMChannel: 'IM Channel',
	__AssignIPAddressCount: 'Assign IPAddress count',
	__searchResult: 'Search Result',

	__flagGameEnable: 'Game Enable',
	__flagSubAgentDefaultEnableGame: 'Enable game by default(Create Sub Agent)',
	__lobbyVisible: 'Game visible in lobby',

	__remainingRound: 'Remaining round',
	__freespinID: 'freespin ID',

	__roundID: 'Round ID',
	__sequence: 'Sequence',
	__sort: 'Sort',
	__sortDirection: 'Sort direction',

	__slots: 'Slots',
	__boardGame: 'Board games',
	__liveCasino: 'Live casino',
	__specialCategory: 'Special Category',
	__specialCategory2: 'Special Category 2',

	__environment: 'Environment',
	__wager: 'Wager',
};

export const Popup = {
	__lock: 'Block',
	__unlock: 'Unblock',
	__batchLock: 'Batch Lock',
	__reason: 'Please leave some comment.',

	__create: 'Add',
	__update: 'Edit',


	__updatePassword: [REDACTED] password',
	__updateTimezone: 'Change timezone',


	__warningFullLoginName: 'Notice! Please use a full account when logging in',
};

export const Status = {
	Enable: 'Enable',
	Disable: 'Disabled',
	PermanentDelete: 'Hide permanently',
};

export const PlayerType = {
	undefined: '---',
	Normal: 'General account',
	Rebot: 'Robot',
};

export const MerchantsStatus = {
	undefined: '---',
	TransferMode: 'Transfer Mode',
	SingleWalletMode: 'Seamless Mode',
};

export const UserOperateTypeEnum = {
	Player_LockStatus: 'Block',
	Player_UnlockStatus: 'Unblock',
	User_UserLogin: 'Login',
};

export const WalletTransferTypeEnum = {
	Transfer_In: 'Wallet Transfer In',
	Transfer_Out: 'Wallet Transfer Out',
};

export const TransactionTypeEnum = {
	TransferWallet_To_GameWallet: 'Wallet Transfer In',
	TransferWallet_To_PlatformWallet: 'Wallet Transfer Out',
	BetGame_Settlement: 'Settled',
};

export const ServerStatusEnum = {
	Enable: 'Server On',
	Disable: 'Server Off',
	ServerDown: 'Temporarily Closed',
};

export const Message = {
	__error: 'Error',
	__thisMayTakeMuchTimeWatting: 'This process may take much time.',
	__yourCreditAmountNotEnough: 'Agent credit is not enough, please contact your upline for topup credit.',
	__productionMercahntWillCalculateBilling: 'This key is for "Production environment" only. All bet data will calculate to billing, please note.',	
	__hintConvertRate: 'Your merchant has set an automatic exchange ratio (transfer-mode transfer-in and transfer-out API)',	
	__devEnvDoNotNeedSetWhiteList: 'Dev/Staging environment do not need setup whitelist ip.',
	__ifYouCanNotSeeDataPleaseContactUs: 'If you can not see data here. please contact us.',
	__OnlySearchSingleMerchantNotAllAgent: 'Can only query single merchant report and does not include the entire agency tree data.',
		
	__thisPageHasLimitIPAddress: 'This page has IP region restrictions',
	__youCanClickThisCopyText: 'You can click below to copy the text',
	__OneIpPerLine: 'Enter one set of IP Addresses per line',

	__thisAgentIsTreeModePleaseUseAgentReport: 'This merchant is agent-tree mode. Please use the 《Agent Report》 to query. or search the sub-agent in this page.',

	__pleaseInputAgentWhiteListIP : 'Fill in the whitelist IP, one set per rows',

	__accountingIgnore: 'Accounting exclusions (accounting special calculations)',
	__searchResultWillIncludeSubAgents: 'Search results will include sub-agents',




	__swValidateTest_Hint: 'Before the test, please input a player account and ensure that it meets the following conditions.',
	__swValidateTest_Rule1: 'Player need at least betting one times in recent 24 hours.',
	__swValidateTest_Rule2: 'Ensure player\'s last sessionToken is alive.',
	__swValidateTest_Rule3: 'You are only allowed 5 tests per day (please contact our technical team if you exceed this limit).',
	__swValidateTest_Hint2: 'We suggest you contact your technical team if You don\'t understand certain terms.',


	__mustSelectLeftFilterWhichColumn : 'Left side specifies field',

	__noLongerSupport: 'Discard , No longer support',
	__pleaseSelectMerchant: 'Please select merchant.',
	__successfully: 'System : Successfully modified!',
	__success: 'Success',
	__PleaseCheckDownloadFile: 'Please check the downloaded file',
	__NoAbnormality: 'No abnormality',
	__copySuccessful: 'System : Successfully Copy!',
	__warning: 'Warning',
	__notSetValueHint: 'Not set',
	__delaySettle: 'Settlement delay',
	__forCSOnly: 'for cs operate',
	__permissionNotEnough: 'Permission not enough',
	__isConfirm: 'Are you sure',
	__isConfirmDelete: 'Are you sure to delete',
	__isConfirmOperate: 'Are you sure',
	__pleaseInput : 'please fill in..',
	__pleaseInputKeywordToSearch: 'Please input keyword to search',
	__pleaseInputKeywordToSearchDropdown: 'Input keyword to show more items.',
	__pleaseFillFormContent : 'Please complete the required fields.',
	__dataDelay30Minutes: 'Data is delayed by 30 minutes.',
	__mustToExternalLinkWatchInfo: 'This information needs to be linked to an external link.',	
	__singleWalletRemarkInfo: 'Seamless mode does not display before-amount and after-amount.',	
	__theDifferenceBetweenWalletMode: 'Memo: the difference between wallet mode.',	
	
	__notAllGameVendorProvideDetailInfo: 'Not all game vendor provide detail info. ',
	__theNotes: '*Notes',
	__dataKeep2MonthOnly: '*Only keep data within 2 months',
	__dataKeepXMonthOnly: '*Only keep data within {month} months',
	__singleWalletNotSupport: 'SingleWallet Mode do not support.',
	__pleaseAddIPToWhiteList: 'Please add those ip to whitelist',
	__someOfPlayerWillMultipleDevice: 'Some players will use multiple devices.',

	__disableBOAccountHint: 'Set BO account status only affects BO login. To close the entire merchant (agent) and forbidden players entering the game. Please go to agent management to adjust the merchant status.',

	__pleaseClickTheBelowLink : 'Click the below link to view history.',
	__fewOfMobileWillCopyFail : 'Some mobile devices may not support one-click copying',


	__notIncludePrefix : 'not include prefix',
	__atLeast_3text: 'at least 3 chararcter',
	__toDisableAccountHint: 'To disable the BO account or change password. please go to ',

	__queryTimeWillLonger: 'query time will be longer',
	__limitSubAgentMaxCount1000: 'Limit agent had maximum of 1,000 sub-agents (Level2+Level3)',

	__accountingBaseOnColumn: 'Accounting statistics are based on this time:',

	__enableGameByAgentDefault: 'Enable game by default(Suggest)',

	__confirmYourCanOnlyEnableGame : 'Please note: Your permission can only enable the game, but not disable it. If you enable the game and then want to disable it. You must contact the customer service  or technical team for assist. Are you sure you want to do this?',

	_mustFilterPlayerID : 'Must filter playerID/account',

	__syncStatusToChilds: 'Sync child-agent\'s status（Disabled）',
	__areYouSureOpenAllGameByType: 'Enable all game of type',
	__confirmResetPasswordToDefault: 'Reset password to default?',
	__playerId_NumberFormat: ' (SystemID number format)',
	__searchAllChildsOfAgent: 'search direct sub-agent',
	__searchHierarchyOfTargetAgent: 'Search hierarchy by agent',
	__searchChildsByParentAgent: 'Search sub-agent by parent',
	__is: ' want ',
	__open: 'open ',
	__close: 'close ',
	__fuzzyQuery: 'fuzzy query',
	__game: 'Game ',
	__defaultValue: 'Default',
	__notSupport: 'Not support',
	__notSupport2: 'Not support',
	__rtp: 'RTP',	
	__theBelowInfoOnlyCalculateStraightPlayer : 'The below information only list each agent\'s straight player. (not include child-agent\'s amount)',
	__all : 'All',
	__adminSysLoginHint1 : 'Use this account to login: ',
	__adminSysLoginHint2 : '(account prefix required)',
	__allFieldRequired : 'All field required',
	__contentContainsImage: 'There are images in content.',
	__BOAnnouncement: 'Announcement',
	__agentStatusDisable_Hint1 : 'When set agent\'s status to disable. Player launch game will be forbidden.',
	__agentStatusDisable_Hint2 : 'To disable backoffice account login. You need to operate in account management page',

	__canNotEditCreditThisAccount : 'Not allow edit credit',
	__pleaseContactTechTeam : 'Please contact our tech team',
	__useCategorySelectGames: 'Select games by category',
	__afterUpdateYouNeedSubmit: 'Please remember save/submit after you update',
};

export const Validation = {
	__requiredRules: 'Required fields',
	__accountRules: 'Account must be 4-30 characters',
	__passwordRules: 'Password must be a combination of 6-30 digits and letters',
	__numberRules: 'Must be a positive number.',
};

export const Lang = {
	'zh-cn': 'CN',
	'zh-tw': 'TC',
	'en-us': 'EN',
};

export const GameType = {
	None: 'None',
	Slot: 'Slot',
};

export const GameVendorType = {
	None: 'None',
	Slot88: 'Slot88',
};

export const Currency = {
	CNY: 'RMB',
	TWD: 'Taiwan Dollar',
	USD: 'US dollar',
	MYR: 'MYR',
	AUD: 'AUD',
	VND: 'VND',
	SGD: 'SGD',
	INR: 'INR',
	IDR: 'IDR',
	CAD: 'Canadian Dollar',
	CZK: 'Czech Koruna',
	DKK: 'Danish Krone',
	EUR: 'Euro Dollar',
	GBP: 'British Pound',
	HKD: 'Hong Kong Dollar',
	JPY: 'Japanese Yen',
	KRW: 'South Korean Won',
	MXN: 'Mexican Pesos',
	MMK: 'Mynamar Kyat',
	NOK: 'Norwegian Krone',
	NZD: 'New Zealand Dollar',
	PHP: 'Philippine Peso',
	PLZ: 'Polish Zloty',
	SEK: 'Swedish Krona',
	THB: 'Thai Baht',
	BETA: 'BETA Coin',
	MSC_POINT: 'MSC-Point',
};

export const OrganizationType = {
	SuperAdmin: 'Super Admin',
	Normal: 'Normal',
};

export const ServerStatus = {
	Enable: 'Server On',
	Disable: 'Server Off',
	ServerDown: 'Temporarily Closed',
};

export const HDTypeEnum = {
	PC: 'PC',
	Android: 'Android',
	IOS: 'Apple',
};



export const $vuetify = {
	noDataText: 'No data found.',
	dataTable: {
		itemsPerPageText: 'Items per page:',
		sortBy : 'Sort by'
	},
	dataIterator: {
		noResultsText: 'No data found'
	},
	dataFooter: {
		itemsPerPageText: 'Items per page:',
		pageText: '{0}-{1} of {2}',
		itemsPerPageAll: 'ALL',
	},
};

export const Home = {
	__totalOnlineUsers: 'OnlinePlayers',
	__totalMoneyCash: 'Todays Profit',
	__totalMoneyTransferIn: 'Transfer In today',
	__totalMoneyTransferOut: 'Transfer Out today',
	__totalMoney: 'TotalMoney',
};

export const Button = {

	__enableEditMode: 'Enable edit mode',
	__showExtraInfo : 'show extra info',
	__create: 'Create',
	__edit: 'Edit',
	__delete: 'Delete',
	__view: 'View',
	__search: 'Search',
	__reset: 'Reset',
	__lock: 'Block',
	__save: 'Save',
	__submit: 'Submit',
	__cancel: 'Cancel',
	__close: 'Close',
	__copy: 'Copy',
	__copyNew: 'Copy and create new',
	__downloadFile: 'Export',
	__openLink: 'Open Link',
	__openExternalLink: 'Open external/replay link',
	__copyExternalLink: 'Copy external/replay link',
	__showDetail: 'Show Detail',
	__disabledEdit: 'Edit Disabled',
	__removeRoomLock: 'Kick from Room',
	__advanceSetting: 'Advance Setting',
	__copyToClipboard: 'Copy To Clipboard',
	__ManualSettlement: 'Manual Settlement',
	__ManualDepositWallet: 'Deposit Wallet',
	__newTabRedirect: 'open new tab',
	__addTag: 'Add Tag',
	__showResultImage : 'Show image history',
	__addCredit : 'increase credit',
	__reduceCredit : 'reduce credit',
	__logout: 'Logout',
	__closeAllTabs: 'Close All Tabs',
	__copyUrl: 'copy url',
	__copyInfo: 'Copy Info',
	__top10 : 'top 10 ',
	__showMore : 'more',
	__copyTime: 'Copy time',
	__JPOnly: 'JP Only',
	__exportApiKey: [REDACTED] Api Key',
	__yes : 'Yes',
	__no : 'No',
	__returnToTopLayer: 'Return to top layer',
	__showEachAgentCustomerDetail: 'Straight customer detail (Each agent)',
	__resetPasswordToDefault: 'Password reset to default',
	__upload : 'Upload',
	__allGames: 'All games',
	__groupGamesAllInOne: 'All games totalsum',
	__groupAndListEachGames: 'List each games',
	__appendNewIP: 'Append new ipAddress',
	__viewParentAgent: 'view parent',
	__selectAll: 'Select all',
	__enableAll: 'Enable all',

};

export const Filter = {
	__filter: 'Filter',
	__sort: 'Sort',
	__sortDirection: 'Sort direction',
	__increasing: 'Increasing',
};