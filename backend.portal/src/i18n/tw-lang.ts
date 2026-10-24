export const Menu = {
	undefined: '',
	Home: '首頁公告',

	MembersManagement: '用戶管理',
	MembersManagementList: '用戶列表',
	MembersManagementList_Search: '進入並查詢',
	MembersManagementList_Lock: '封號',

	MembersManagementDetail: '用戶查詢',
	MembersManagementDetail_Search: '進入並查詢',
	MembersManagementDetail_Lock: '封號',
	MembersManagementDetail_GameSummary: '查看遊戲統計',
	MembersManagementDetail_AdvanceUpdate: '會員進階編輯',

	MembersManagementMoneyRecord: '金幣紀錄',
	MembersManagementBetRecord: '玩家進出房紀錄',
	MembersManagementLock: '封號紀錄',
	MembersManagementTransfer: '轉帳紀錄',


	MembersManagement_GameVendorTransfer_Management: '轉帳紀錄(特殊)',
	MembersManagement_GameVendorTransfer_Search: '轉帳紀錄_特殊遊戲_搜尋',
	MembersManagement_GameVendorTransfer_Advance: '轉帳紀錄_特殊遊戲_進階操作',
	
	MembersManagementDetail_AllowSimulateGameLaunch  : '允許模擬玩家進入遊戲',
	MembersManagementMoneyRecord_Search   : '金幣紀錄_基本搜尋',
	MembersManagement_MoneyRecord_ForceFilterPlayer : '金幣紀錄_強制要求篩選玩家',

	SystemLogsExternalApiRequestLogManagement: '日誌-三方API請求記錄管理',
	SystemLogsExternalApiRequestLogList: '三方API請求紀錄',

	GamesManagementViewList: '遊戲歷程-查看紀錄',
	GamesManagementAdminPermission: '遊戲歷程-管理員進階管理',
	SystemSetting_PayTableImageManagmenet: '投注结果圖片管理',
	SystemSetting_PayTableImageList: '投注结果圖片列表',


	Merchant_SelfUpdateSetting: '其他設置',
	Merchant_SelfUpdateSetting_GameSwitch: '(功能已廢棄)大廳顯示遊戲開關',
	Merchant_SelfUpdateSetting_Search: '商戶自訂遊戲開關查詢',
	Merchant_SelfUpdateSetting_Update: '商戶自訂遊戲開關編輯',


	GameManagement: '遊戲紀錄',
	GamesManagementRecord: '遊戲歷程查詢',
	GamesManagementServersHealthSummary: '服務狀態',
	GamesManagement_BetRecordSearch: '注單號查詢',
	GamesManagement_BetRecordSearch_SearchList: '遊戲商注單查詢',
	GamesManagement_BetRecord_ManualRefund: '手動退還注單金幣',
	GamesManagement_BetRecord_ManualSettleBet: '手動結算注單',
	GamesManagement_BetRecordSearch_UnionBetRecord: '遊戲商注單查詢',
	GamesManagement_BetRecordSearch_Export : '匯出三方遊戲注單',	
	GamesManagement_DynamicQuery: '簡易結果查詢',
	GamesManagement_NotifyQueueList : '無縫錢包回調通知管理',

	GameManagement_AGGJpAmount: '遊戲彩池查詢',
	System_CustomerServiceTool: '客服工具',

	GameSetting: '遊戲配置',
	GameSettingSwitch: '商戶遊戲開關',
	GameSettingSwitch_Search: '進入並查詢',
	GameSettingSwitch_Update: '修改開關',

	Other_AccountManagement: '帳號管理',
	Other_AccountManagement_Search : '帳號管理-搜尋',
	Other_AccountManagement_Create : '帳號管理-新增',
	Other_AccountManagement_Copy : '帳號管理-複製帳號',
	Other_AccountManagement_EditStatus: '帳號管理-修改狀態',
	Other_AccountManagement_ResetPWAndValidChannel: '帳號管理-重設密碼且需驗證頻道(客服層級用)',


	Other_BOAnnouncement_Management: '後台公告管理',
	Other_BOAnnouncement_List: '後台公告管理-搜寻',
	Other_BOAnnouncement_CreateUpdate: '後台公告管理-新增',

	MembersManagementThirdPartyGameLoginLogList: '三方遊戲啟動紀錄',

	Other_DownloadResource_Page : '資源下載',
	Other_DownloadResource_ApiDocument : 'API文檔下載',
	

	EventManagement: '活動管理',
	SystemSetting_EventManagement: '活動列表',
	SystemSetting_EventManagement_Search: '活動列表-查詢',
	SystemSetting_Event_ManualSendPrize: '活動管理_手動派發獎勵',
	SystemSetting_EventDetail: '活動詳細',
	SystemSetting_Event_DrawPrize: '多重獎項管理',

	SystemSetting_EventPrizeRecord: '活動派獎記錄',
	SystemSetting_GameVendorRTPManagement: '遊戲RTP管理',
	SystemSetting_Announcement: '公告/廣告管理',

	SystemSetting_GeneralTag: '標籤管理',
	SystemSetting_GeneralTag_ListSearch: '標籤管理(搜尋)', 

	SystemSetting_OneTimeJob: '一次性排程',

	SystemSetting_Merchant_IPAddressTemplate: 'IP白名單管理',
	SystemSetting_Merchant_IPAddressTemplate_Search: 'IP白名單管理_搜尋',
	SystemSetting_Merchant_IPAddressTemplate_AdvanceEdit : 'IP白名单管理_ 進階編輯與新增',
	SystemSetting_Merchant_IPAddressTemplate_AppendNewIP: 'IP白名单管理_新增一組新IP',	
	SystemSetting_Merchant_IPAddressTemplate_Delete: 'IP白名單管理_刪除資料',

	
	Statistics: '統計資料',
	StatisticsMerchantSummary: '數據報表',
	StatisticsGame: '遊戲報表',
	Statistics_MerchantDailySummary: '營運日報表',
	Statistics_PlayerGameSummary: '玩家遊戲報表',
	Statistics_RetentionRate: '留存率報表',
	Statistics_MerchantAllPlayerWinLoss: '玩家輸贏報表',
	Statistics_GameVendorFinanceCompareReport: '三方數據比對報表',
	Statistics_AgentHierarchyDailySummary: '代理日報表(內部用)',
	Statistics_AgentHierarchy_TotalReport: '代理總報表',
	Statistics_MerchantWinLossRankingReport: '玩家輸贏排名',
	Statistics_AgentHierarchyDailyGameReport : '代理遊戲日報表',
	Statistics_AgentSpecialReport : '代理特殊报表',
	Statistics_FreespinReport: '活動彙總報表',
	Statistics_FreespinStatusList: '免費旋轉狀態查詢',

	Statistics_EazyQuery_ForPartner: '額外查詢(合作方使用)',
	Statistics_EazyQuery_QueryInvoiceEachMerchantDetail: '(請勿亂開) 允許查詢每月帳務_商戶明細',


	OperatorManagement: '帳號管理',
	OperatorManagementAccounts: '帳號管理',
	OperatorManagementAccounts_Search: '進入並查詢',
	OperatorManagementAccounts_Create: '新增帳號',
	OperatorManagementAccounts_Update: '編輯帳號',
	OperatorManagementAccounts_Delete: '刪除帳號 ',
	OperatorManagementAccounts_AllowViewAllOrgAccounts: '允許瀏覽同組織所有帳號(針對非超級管理員)',


	Agent_Management: '代理管理',
	Agent_MerchantCredit: '代理管理',
	Agent_MerchantCredit_Search: '代理信用額度查詢',
	Agent_MerchantCredit_Update: '代理信用額度編輯',
	Agent_MerchantCredit_CreateChildAgent: '新增下級代理',
	Agent_MerchantCredit_EditStatus : '修改商戶狀態',
	Agent_MerchantCredit_EditAdvanceSetting: '代理進階編輯',
	Agent_MerchantCredit_ExportApiKey: [REDACTED]',
	Agent_MerchantCredit_BatchExportApiKey: [REDACTED]',
	Agent_MerchantCredit_AllowSetWalletModeWhenCreate: '創建子代_可以改變錢包模式',
	Agent_MerchantCredit_AllowSetIMChannelID: '創建子代_可以選擇IM頻道',
	Agent_MerchantCredit_DisableCreateLayer2Agent: '禁止創建二級代理',
	Agent_MerchantCredit_SyncSubAgent: '(進階)同步子代理遊戲開關',
	Agent_MerchantCredit_TriggerSwValidateTest: '觸發單錢包商戶 Validate Test',
	Agent_MerchantCredit_CreateDevMerchant: '創建Dev商戶',


	Agent_MerchantCredit_UpdateGameOpenFlag: '(二選一)代理管理_編輯遊戲開關',
	Agent_MerchantCredit_GameOpenFlag_EnableOnly: '(二選一)代理管理_僅能開啟遊戲',

	Agent_MerchantCreditChangeLog: '額度異動紀錄',
	Agent_MerchantHierarchyTreeView: '代理樹狀結構總覽',

	System_AccountingInvoice_Basic : '帳單管理_基本功能',
	System_AccountingInvoice_ExportInvoice : '帳單管理_帳單匯出與查看',


	SystemSetting: '管理員設置',
	SystemSettingOrganizations: '組織管理',
	SystemSettingOrganizations_Search: '進入並查詢',
	SystemSettingOrganizations_Create: '新增組織',
	SystemSettingOrganizations_Update: '編輯組織',
	SystemSettingOrganizations_Delete: '刪除組織',

	SystemSettingMerchant: '商戶管理',
	SystemSettingMerchant_Search: '進入並查詢',
	SystemSettingMerchant_Create: '新增',
	SystemSettingMerchant_Update: '修改',
	SystemSettingMerchant_Delete: '刪除',

	System_Merchant_CustomizedSettingList: '商戶進階配置總覽',

	SystemSettingServers: '伺服器管理',
	SystemSettingServers_Search: ' 進入並查詢',
	SystemSettingServers_Create: '新增',
	SystemSettingServers_Update: '修改',
	SystemSettingServers_Delete: '刪除',
	SystemSettingServers_Dispatch: '分配商戶',
	SystemSettingServers_DispatchBatch: '分配商戶',

	SystemSetting_GameList: '遊戲管理',
	SystemSetting_GameList_Search: '遊戲查詢',
	SystemSetting_GameList_Create: '遊戲新增',
	SystemSetting_GameList_Update: '遊戲更新',

	SystemSetting_VendorMaintenaceManage: '遊戲商維護管理',
	SystemSetting_VendorMaintenace: '遊戲商維護',
	SystemSetting_VendorMaintenaceCreate: '新增遊戲商維護',

	SystemSetting_VendorConfig: '遊戲商設置',
	SystemSetting_VendorConfigList: '遊戲商設置列表',


	SystemSettingServerRobotConfig: '機器人配置',
	SystemSettingServerRobotConfig_Search: '進入並查詢',
	SystemSettingServerRobotConfig_Create: '新增',
	SystemSettingServerRobotConfig_Update: '修改',
	SystemSettingServerRobotConfig_Delete: '刪除',

	SystemSetting_LanguageText_Management: '多語系文字管理',
	SystemSetting_LanguageText_List: '多語系文字列表',


	SystemSettingRoles: '角色管理',
	SystemSettingRoles_Search: '進入並查詢',
	SystemSettingRoles_Create: '新增',
	SystemSettingRoles_Update: '修改',
	SystemSettingRoles_Delete: '刪除',
	SystemSettingRoles_Rename: '名稱修改',


	SystemSetting_OneToManySettings: '多鍵值系統配置管理',
	SystemLogs_SingleWalletPlatformReqeustLog: '日誌-單錢包商戶請求紀錄管理',
	SystemLogs_SingleWalletPlatformReqeustLog_List: '單錢包商戶請求紀錄_查詢',
	
	SystemLogs_ApiReqeustLogManagement: '日誌-系統日誌ApiRequestLog',
	SystemSetting_TGAnnouncement : 'TG公告發送',

	SystemSetting_SystemConfigSetting: '系統配置',
	SystemSetting_SystemConfigSetting_Search: '系統全域設置管理',
	SystemSetting_SystemConfigSetting_Update: '系統全域設置修改',

	SystemSetting_IMChannelList: 'TG/Skype频道管理',

	System_AccountingInvoice : '帳單管理',


	Records_UserOperateRecord: '操作纪录',
	Records_UserOperateRecord_Search: '操作纪录_搜尋',
	Records_UserOperateRecord_SearchAllOrg: '操作纪录_查看全组织',
	

	Special_PermissionSetting: '特殊權限配置',
	Special_BetAmount_FlowAmount_Invisible: '部分頁面不顯示投注額欄位',
	Special_RTP_Invisible: '部分頁面不顯示RTP欄位',
	Special_WinLose_Invisible: '部分頁面不顯示WinLose欄位',	

	Special_AlertNotifyWhenLogin : '登入成功會發送警報',
	Special_ExportMaxSizeRestrict: '匯出功能會限制筆數',	

	Special_OnlyVisible_IDG_Games: '僅顯示部分IDG可見遊戲',
	Special_AllowToUseExperimentFeature : '允许使用实验性功能',
	Special_AllowAssignHierarchyMerchantForUser : '商戶分配特殊權限(請勿亂勾此)',
	Special_AllowToUseDevelopeFeature: '開放使用技術專用特殊功能(請勿亂勾此)',
	Special_DefaultEnableSearchDetailCheckbox: '报表页预设勾选查询详细',
	Special_SuperAdminOrg_ButRestrictFeature: '超級管理組織_但是帳號功能受限',
	Special_PGS_CustomerService: 'PGS客服人員標記',
	Special_CanSelectedSelfRoles: '創建後台帳號_允許選自己當前角色(無視組織配置)',
	Special_Account_SearchAccountByMultiMerchantAssignMode : '允許透過多種判斷條件_列出示可管理帳號(Super代理)',
	Special_AM_Permission : 'AM層級_特殊權限',
	Special_Dropdown_ShowRichmondDropdowns: '下拉選單_顯示所有Richmond客戶頻道',

	Special_SuperAdminOrg_ButNeedValidateWhenSubmit: '超級管理組織_但功能防呆更嚴格',

	SystemSetting_GameVendorRTPManagement_ListSearch : '游戏商RTP管理-搜寻',
	SystemSetting_GameVendorRTPManagement_Update : '游戏商RTP管理-更新RTP',

	Special_HideBeforeAfterAmount: '遊戲歷程_隱藏欄位_更新前後金額',

};


export const TableHeader = {

	__prompt: '提示',
	__lock: '封號',
	__ID: 'ID',
	__playerID: '玩家ID',
	__playerAccount: '玩家帳號',
	__player: '玩家',
	__displayName: '顯示名稱',
	__newName: '重新輸入名稱',
	__status: '狀態',
	__playerType: '帳號身分',
	__walletMode: '錢包模式',
	__merchant: '商戶',
	__merchantID: '商戶 ID',
	__merchantName: '商戶名稱',
	__organizationName: '所屬組織',
	__organizationID: '組織ID',
	__organization: '組織',
	__role: '角色',
	__roleAndPermission: '角色（可操作功能權限不同）',
	__roleID: '角色 ID',
	__roleName: '角色名稱',
	__walletAmount: '現金數',
	__createBy: '建立人',
	__settleTime: '結算時間',
	__createTime: '創建時間',
	__createTime2: '創建時間',
	__receiveTime: '領取時間',
	__createIPAddress: '註冊IP',
	__defaultBalanace: '初始金幣',
	__convertRate: '轉換比例',
	__defaultPassword: [REDACTED]',

	__create: '創建時間',
	__lastLogin: '最後登入',
	__lastLoginTime: '最後登入時間',
	__loginCount: '累計登入次數',
	__lastIPAddress: '最後登入IP',
	__updateTime: '更新時間',
	__lastUpdateTime: '最後更新時間',
	__lastSearchTime: '最後查詢時間',

	__gameInfo: '遊戲訊息',
	__gameName: '遊戲名稱',
	__winCount: '贏的局數',
	__loseCount: '輸的局數',
	__drawCount: '平的局數',
	__score: '輸贏金幣數',
	__bonusAmount: '彩金',
	__gameTimeSeconds: '遊戲時長(時:分:秒)',
	__gameTime: '遊戲時長',
	__code: '代碼',
	__total: '匯總',
	__currentPageOnly: '本頁',

	__loginRecord: '登入紀錄',
	__time: '時間',
	__times: '次數',
	__hdType: '登入設備',
	__hdBrowser: '登入瀏覽器',
	__NotSetValue: '未設置',
	__iPAddress: 'IP地址',
	__game: '遊戲',
	__gameID: '遊戲ID',
	__gameCode: '遊戲代碼',
	__gameType: '遊戲分類',
	__gameVendorType: '遊戲廠商',
	__server: '伺服器',
	__serverID: '伺服器 ID',
	__serverName: '伺服器名稱',
	__loginTime: '登入時間',
	__logoutTime: '登出時間',
	__totalMinutes: '總時間(分)',

	__loginAmount: '進遊戲金幣',
	__currentAmount: '當前金幣',
	__updateAmount: '更新金幣',
	__winLossAmount: '輸贏金幣',
	__taxAmount: '稅收',

	__othersMemo: '其他說明',
	__others: '其他',
	__memo: '備註說明',
	__reason: '原因',


	__searchField: '搜尋欄位',
	__searchText: '搜尋文字',

	__isDefault: '是否為預設值',
	__isTestEnv: '是否為測試環境',
	__lockTime: '封鎖時間',
	__operateType: '封號類型',
	__category: '分類',
	__subCategory: '次分類',

	__updateBy: '操作人',

	__type: '類型',
	__transferType: '流通類型',
	__transactionType: '交易類型',
	__orderNo: '系統單號',
	__betRecordNoPGS: '天馬投注單號 (TransactionNo)',
	__transactionNo: '交易單號',
	__betRecordNo: '投注單號',
	__merchantOrderNo: '商戶轉帳單號',
	__amount: '交易金額',
	__beforeAmount: '更新前金幣',
	__afterAmount: '更新後金幣',

	__perGameLogID: '牌局編號',
	__gameData: '遊戲數據',
	__enableGamesCount: '開啟遊戲數',
	__enableGamesFilter: '有開啟此遊戲',

	__serverStatus: '服務狀態',
	__onlinePlayer: '在線人數',
	__profitAmount: '今日損益',
	__flowAmount: '今日流水',
	__zeroSkipInsert: '資料為0不寫入',

	__action: '操作',

	__account: '帳號',
	__BOAccount: '後台帳號',
	__userName: '用戶名',
	__displayName2: '用戶顯示名稱',
	__password: '密碼',
	__password2: '確認密碼',
	__timeZoneArea: '時區',

	__organizationCode: '前綴',
	__merchantTotalCount: '商戶數',
	__usersTotalCount: '帳號數',
	__lang: '語系',
	__organizationType: '組織類別',
	__currency: '幣別',
	__apiValidatePlayerSessionUrl: '驗證路由(AVPSU)',
	__apiSingleWalletGetAmountUrl: '單錢包路由',

	__flagLevel: 'FlagID',
	__roomFlagLevel: '厅房分類',
	__clientAddr: '客戶端連接地址',
	__notifyAddr: '通知地址',
	__httpAddr: 'http地址',
	__assignMerchantTotalCount: '已分配商戶',
	__minAvailableMoney: '入場金幣',

	__minLevel: '最小智商',
	__maxLevel: '最大智商',
	__beginTime: '開始時間',
	__endTime: '結束時間',
	__minRobotCount: '最小數量',
	__maxRobotCount: '最大數量',

	__finalScore: '結算金額',
	__betAmount: '投注金額',
	__validAmount: '有效流水',
	__payoutAmount: '中獎金額',
	__merchantProfit: '商戶盈利',
	__gameWinLoss: '遊戲輸贏',
	__playerWinLoss: '玩家輸贏',

	__permission: '權限',
	__gameResult: '投注結果',


	__today: '今日',
	__yesterday: '昨日',
	__thisWeek: '本週',
	__lastWeek: '上週',
	__thisMonth: '本月',
	__lastMonth: '上月',

	__basicInfo: '基本訊息',
	__totalLoginPlayer: '總登錄人數 （不重複）',
	__transferAmount: '錢包增減',
	__transferInAmount: '匯入金幣',
	__transferOutAmount: '匯出金幣',

	__loginInfo: '登錄訊息',
	__totalLoginCount: '總登錄人次',
	__newLoginPlayer: '新登錄人數',
	__maxLoginPlayer: '最高在線',
	__avgLoginPlayer: '平均在線',

	__rate: '占比',
	__totalPeopleCount: '總人數',
	__totalCount: '總筆數',
	__totalRound: '總局數',
	__totalAmount: '總金額',
	__maxExportSize: '匯出筆數上限',
	__totalBetPlayer: '總遊玩人數（不重複）',
	__loseAmount: '用戶輸',
	__winAmount: '用戶贏',
	__moneyChangeAmount: '金幣增減',
	__xTitle: '時間',
	__loseAmount2: '輸金幣',
	__winAmount2: '贏金幣',
	__jackpotPoolAmount: '彩池',
	__tag: '標籤',
	__eventID: '活動ID',
	__amountOrQuantity: '數量/金額/倍率',
	__vendorConfig: '遊戲商配置',

	__jackpotTag: 'JP彩池中奖',
	__referenceInfo: '參考資訊',
	__referenceNo: '參考單號',
	__eventName: '活動名稱',

	__bankViewPoint: '莊家視角',
	__playerViewPoint: '玩家視角',

	__restrictCurrency: '限制幣別',
	__supportCurrency: '支援幣別',
	__supportGames: '支援遊戲',
	__supportMerchants: '支援商戶',
	__extraInfo: '額外資訊',	
	__imChannelName: 'IM頻道名稱',

	

	__extraFilterType : '額外篩選條件',	

	__futureTime: '未來日期',
	__memoOrInternalDisplayName: '備註／內部顯示名稱',
	__internalDisplayName: '內部顯示名稱',
	__title: '標題',
	__description: '内容描述',
	__playerLanguageDistribute: '玩家使用語系分布',
	__playerDeviceDistribute: '玩家登入裝置分布',



	__merchantAndAllChilds: '代理以及所有下級代理',

	__imageUrl: '圖片網址',
	__barChart: '長條數據圖',
	__pieChart: '圓餅數據圖',
	__recentDailySummary: '近期投注日報表',
	__device: '裝置',
	__rtp: 'RTP',
	__winRate: '勝率',
	__profitRate: '盈利率',
	__promotionBonus: '活動獎金',
	__promotionCost: '活動獎勵',
	__promotionBonusReceive: '金幣異動 (獎勵領取)',
	__merchantProfitWithPromotionCost: '商户營利 (扣除活動成本)',

	__parent: '上級',
	__parentLine: '上級',

	__creditAmount: '信用額度',
	__adjustCreditAmount: '調整信用額度',
	__yourRemainingCreditAmount: '您的剩餘額度',
	__agentRemainingCreditAmount: '代理剩餘額度',
	__agent: '代理',
	__isUnlimit: '是否無限額度',
	__unlimitCredit: '無限額度',
	__operationType: '操作類型',
	__adjustValue: '調整數值',
	__beforeValue: '更新前',
	__afterValue: '更新後',
	__keyword: '關鍵字',
	__keywordSearch: '關鍵字查詢',
	__searchType: '查詢方式',
	__merchantOrAgent: '商戶(代理)',
	__directCustomerData: '直客玩家數據',
	__detailOfSingleDate : '單日細項',
	__detailOfFishGame : '捕魚詳細記錄',
	__FishingGame : '捕魚',
	__currentGameRoom : '當前遊戲房',
	__excludeZeroAmount : '排除數據为0的资料',
	__sendTarget: '发送对象',
	__FourDecimalPlaces: '小數點四位',
	__layoutOrientation: '版型',

	
	__IMChannel: 'IM群組',
	__AssignIPAddressCount: '已設置IP數',
	__searchResult: '查詢結果',

	__flagGameEnable: '遊戲開關',
	__flagSubAgentDefaultEnableGame: '創建子代預設勾選此遊戲',
	__lobbyVisible: 'pgs大廳顯示此遊戲',

	__remainingRound: '剩餘局數',
	__freespinID: '免費旋轉ID',

	__roundID: '局號',
	__sequence: '序號',
	__sort: '排序',
	__sortDirection: '排序方向',

	__slots: '老虎機',
	__boardGame: '棋牌',
	__liveCasino: '真人視訊',
	__specialCategory: '特殊分類',
	__specialCategory2: '特殊分類2',

	__environment: '環境',
	__wager: '注單',


};

export const Popup = {
	__lock: '封號',
	__unlock: '解封',
	__batchLock: '批次封號',
	__reason: '請填寫原因',

	__create: '新增',
	__update: '編輯',

	__updatePassword: [REDACTED]',
	__updateTimezone: '修改時區',

	__warningFullLoginName: '注意!登入時請使用完整帳號',
};

export const Status = {
	Enable: '啟用',
	Disable: '關閉中',
	PermanentDelete: '永久隱藏',
};

export const PlayerType = {
	undefined: '---',
	Normal: '一般帳號',
	Rebot: '機器人',
};

export const MerchantsStatus = {
	undefined: '---',
	TransferMode: '轉帳模式',
	SingleWalletMode: '單一錢包模式',
};

export const UserOperateTypeEnum = {
	Player_LockStatus: '封號',
	Player_UnlockStatus: '解封',
	User_UserLogin: '登入',
};

export const WalletTransferTypeEnum = {
	Transfer_In: '轉入',
	Transfer_Out: '轉出',
};

export const TransactionTypeEnum = {
	TransferWallet_To_GameWallet: '遊戲錢包轉入',
	TransferWallet_To_PlatformWallet: '遊戲錢包轉出',
	BetGame_Settlement: '遊戲結算',
};

export const ServerStatusEnum = {
	Enable: '啟動中',
	Disable: '關閉禁用',
	ServerDown: '伺服器端設為臨時關閉',
};

export const Message = {
	__error: '錯誤',
	__thisMayTakeMuchTimeWatting: '此行為可能需花費較長時間等待',
	__pleaseSelectTheTitlePlayer: '請選擇封號玩家',
	__yourCreditAmountNotEnough: '信用額度不足。請聯繫您的上級協助加值(topup)',
	__productionMercahntWillCalculateBilling: '此設定為「正式環境」對接使用。 正式環境進行的投注資料會《計入帳務》。 測試時請注意',
	
	__hintConvertRate: '您的商戶已設定自動兌換比例(轉帳模式 轉入轉出API)',	
	__devEnvDoNotNeedSetWhiteList: '測試(Dev/Staging)環境不需設置白名單',
	__ifYouCanNotSeeDataPleaseContactUs: '若您的帳號在此頁查無資料。請聯繫我們技術',
	__OnlySearchSingleMerchantNotAllAgent: '此查询仅能查询单一商户，不含整个代理结构',
	

	__swValidateTest_Hint: '执行测试前。请指定一名玩家帐号，并确保符合以下条件',
	__swValidateTest_Rule1: '玩家需于近期24小时内至少投注过一次',
	__swValidateTest_Rule2: '需确保玩家最后一笔sessionToken存活有效',
	__swValidateTest_Rule3: '每天只允许测试5次 (若超过次数请联系我方技术团队)',
	__swValidateTest_Hint2: '若对以上不清楚。可与您的技术团队询问',

	__mustSelectLeftFilterWhichColumn : '左方指定查詢欄位',

	__thisPageHasLimitIPAddress: '此頁面有IP地區限制',
	__youCanClickThisCopyText: '您可以點擊下方複製文字',
	__OneIpPerLine: '每行填一組ipAddress',

	__thisAgentIsTreeModePleaseUseAgentReport: '此商户属于代理树状商户。 请改用《代理总报表》查询、或您只能操作本功能搜尋底下子代理 (搜尋單一子代理)',

	__pleaseInputAgentWhiteListIP : '填寫白名單IP，一列填寫一組',

	__accountingIgnore: '帐务排除(帐务特殊计算)',
	__searchResultWillIncludeSubAgents: '搜尋結果將包含子代理',

	__noLongerSupport: '已廢棄，已不再支援此',
	__pleaseSelectMerchant: '請選擇商戶',
	__successfully: '系統提示：修改成功',
	__success: '成功',
	__PleaseCheckDownloadFile: '請查看下載檔案',
	__NoAbnormality: '無異常',	
	__copySuccessful: '系统提示：複製成功',
	__warning: '警告',
	__isConfirm: '是否確認',
	__notSetValueHint: '未設置',
	__delaySettle: '延遲結算',
	__forCSOnly: '僅供客服使用',
	__permissionNotEnough: '權限不足',
	__pleaseInput: '請輸入..',
	__pleaseInputKeywordToSearch: '請輸入關鍵字查詢',
	__pleaseInputKeywordToSearchDropdown: '輸入關鍵字以查詢更多選項',
	__pleaseFillFormContent: '請完整輸入表單必填項目',
	__isConfirmDelete: '是否確認刪除',
	__isConfirmOperate: '確認進行此操作',
	__dataDelay30Minutes: '統計數據有30分鐘延遲',
	__mustToExternalLinkWatchInfo: '部分投注資訊，需連結至外部連結查看',
	__singleWalletRemarkInfo: '單錢包模式不顯示異動前後金額',
	__theDifferenceBetweenWalletMode: '*顯示差異注意事項',
	__notAllGameVendorProvideDetailInfo: '部分遊戲商無提供詳細資訊',
	__theNotes: '*注意事項',
	__dataKeep2MonthOnly: '*僅保留2個月內資料',
	__dataKeepXMonthOnly: '*僅保留{month}個月內資料',
	__singleWalletNotSupport: '單錢包模式不提供此紀錄',

	__pleaseAddIPToWhiteList: '請將這幾組IP加入白名單中',

	__someOfPlayerWillMultipleDevice: '部分玩家會使用多種裝置登入',

	__disableBOAccountHint: '后台账号状态关闭仅影响后台登入。若要关闭整个商户(代理)、禁止玩家进入游戏。请至代理管理调整状态。',

	__searchAllChildsOfAgent: '搜尋該代理直屬下級',
	__searchHierarchyOfTargetAgent: '透過代理搜尋樹狀階層',
	__searchChildsByParentAgent: '透过上级代理搜寻直属下级',
	_mustFilterPlayerID: '需篩選玩家ID或帳號',

	__syncStatusToChilds: '同時關閉所有子代理',
	__areYouSureOpenAllGameByType: '是否開啟所有此類遊戲',
	__confirmResetPasswordToDefault: '是否將此帳戶的密碼修改為預設密碼?',
	

	__pleaseClickTheBelowLink: '請透過下方連結查看開獎紀錄',
	__fewOfMobileWillCopyFail: '部分裝置不支援一鍵複製、需自行複製此連結',

	__playerId_NumberFormat: ' (系統編號)',
	__notIncludePrefix: '不含前綴',
	__atLeast_3text: '至少3個字',
	__toDisableAccountHint: '若要關閉後台帳號或修改密碼。請至',

	__queryTimeWillLonger: '查詢速度將會受影響',
	__limitSubAgentMaxCount1000: '限制每位总代底下最多1000位代理 (二级+三级)',

	__accountingBaseOnColumn: '帳務統計是以此時間為基準:',

	__enableGameByAgentDefault: '自動勾選遊戲(建議設定)',
	__confirmYourCanOnlyEnableGame : '請注意: 您的權限只能開啟遊戲、不能關閉遊戲。若您開啟遊戲後又想關閉遊戲。您必須聯繫客服窗口或技術窗口協助關閉。是否確定執行此操作?',

	__is: '要',
	__open: '開啟',
	__close: '關閉',
	__game: '遊戲',
	__defaultValue: '默認',
	__fuzzyQuery: '模糊查詢',
	__notSupport: '不支援此項目',
	__notSupport2: '不支援',
	__theBelowInfoOnlyCalculateStraightPlayer : '下方每位代理的數字僅包含直客玩家(未包含子代理的成績)',
	__all : '全部',
	__adminSysLoginHint1 : '登入后台需使用',
	__adminSysLoginHint2 : '进行登入(需包含前缀)',
	__allFieldRequired : '全部欄位都是必填',

	__contentContainsImage: '內文包含圖片',
	__BOAnnouncement: '公告通知',

	__agentStatusDisable_Hint1 : '状态关闭将会禁止所有玩家进入游戏。请谨慎操作',
	__agentStatusDisable_Hint2 : '若要禁止后台帐号登入，则需前往 [帐号管理] 修改',
	
	__canNotEditCreditThisAccount : '禁止編輯信用額度',
	__pleaseContactTechTeam : '请与技术单位确认',


	__useCategorySelectGames: '根據分類批次勾選遊戲',


	__afterUpdateYouNeedSubmit: '調整更新後。記得要按儲存',
};

export const Validation = {
	__requiredRules: '必填項目',
	__accountRules: '帳號必須為4-30位的字元',
	__passwordRules: '密碼必須為6-30位的數字和字母的組合',
	__numberRules: '必須為正值數字.',
};

export const GameType = {
	None: '無',
	Slot: 'Slot老虎機',
};

export const GameVendorType = {
	None: '無',
	Slot88: 'Slot88',
};


export const Lang = {
	'zh-cn': '簡中',
	'zh-tw': '繁中',
	'en-us': '英文',
};

export const Currency = {
	CNY: '人民幣',
	TWD: '台幣',
	USD: '美元',
	MYR: '馬來幣',
	AUD: '澳幣',
	VND: '越南盾',
	SGD: '新加坡幣',
	INR: '盧比',
	IDR: '印尼盾',
	CAD: '加拿大幣',
	CZK: '捷克幣',
	DKK: '丹麥克朗',
	EUR: '歐元',
	GBP: '英鎊',
	HKD: '港幣',
	JPY: '日元',
	KRW: '韓幣',
	MXN: '墨西哥幣',
	MMK: '緬甸元',
	NOK: '挪威幣',
	NZD: '紐西蘭元',
	PHP: '菲律賓披索',
	PLZ: '波蘭幣',
	SEK: '瑞典克朗',
	THB: '泰銖',
	BETA: 'BETA幣',
	MSC_POINT: 'MSC點數',
};

export const OrganizationType = {
	SuperAdmin: '超級管理',
	Normal: '一般維運',
};

export const ServerStatus = {
	Enable: '啟動中',
	Disable: '關閉禁用',
	ServerDown: '伺服器端設為臨時關閉',
};

export const HDTypeEnum = {
	PC: 'PC',
	Android: '安卓',
	IOS: '蘋果',
};




export const $vuetify = {
	noDataText: '請輸入查詢條件、或您當前的條件查無資料.',
	dataTable: {
		itemsPerPageText: '每頁筆數',
		sortBy : '排序方式'
	},
	dataIterator:
	{
		noResultsText: '請輸入查詢條件、或您當前的條件查無資料'
	},
	dataFooter: {
		itemsPerPageText: '每頁筆數',
		pageText: '{0}-{1} of {2}',
		itemsPerPageAll: 'ALL',
	},
};

export const Home = {
	__totalOnlineUsers: '線上人數',
	__totalMoneyCash: '今日金幣增減',
	__totalMoneyTransferIn: '今日匯入金幣',
	__totalMoneyTransferOut: '今日匯出金幣',
	__totalMoney: '金幣存量',
};


export const Button = {

	__enableEditMode: '開啟編輯模式',
	__showExtraInfo: '顯示額外資訊',
	__create: '新增',
	__edit: '編輯',
	__delete: '刪除',
	__view: '查看',
	__reset: '重置',
	__search: '搜索',
	__lock: '封號',
	__submit: '確認',
	__save: '儲存送出',
	__cancel: '取消',
	__close: '關閉',
	__copy: '複製',
	__copyNew: '複製並建立一筆新資料',
	__downloadFile: '匯出',
	__openLink: '開啟連結',
	__openExternalLink: '開啟快照/回播連結',
	__copyExternalLink: '複製快照/回播連結',
	__showDetail: '查看詳細',
	__disabledEdit: '未開放編輯',
	__removeRoomLock: '解除卡房/踢除玩家',
	__advanceSetting: '進階設置',
	__copyToClipboard: '複製資訊至剪貼簿',
	__ManualSettlement: '手動結算',
	__ManualDepositWallet: '手動加值錢包',
	__newTabRedirect: '另開分頁前往',
	__copyUrl: '複製網址',
	__copyInfo: '複製資訊',
	__top10: '最新10筆',
	__showMore: '查看更多',
	__showResultImage: '查看盤面紀錄',
	__JPOnly: '僅顯示JP',
	__copyTime: '複製時間',
	__logout: '登出',
	__closeAllTabs: '關閉所有分頁',
	__addTag: '標記標籤',
	__addCredit: '增加額度',
	__reduceCredit: '減少額度',
	__exportApiKey: [REDACTED]',
	__yes: '是',
	__no: '否',
	__returnToTopLayer: '返回搜寻结果最上层',	
	__showEachAgentCustomerDetail: '直客數據 (每位代理)',
	__resetPasswordToDefault: '重設成預設密碼',
	__upload : '上傳',
	__allGames: '全遊戲',
	__groupGamesAllInOne: '不分遊戲(統計全遊戲加總)',
	__groupAndListEachGames: '列出每款遊戲',
	__appendNewIP: '增加一組新IP',
	__viewParentAgent: '返回上級',
	__selectAll: '勾選全部',
	__enableAll: '啟用全部',

};




export const Filter = {
	__filter: '篩選',
	__sort: '排序',
	__sortDirection: '排序方向',
	__increasing: '遞增',
};



