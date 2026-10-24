export const Menu = {
	undefined: '',
	Home: '首页公告',

	MembersManagement: '用户管理',
	MembersManagementList: '用户列表',
	MembersManagementList_Search: '进入并查询',
	MembersManagementList_Lock: '封號',

	MembersManagementDetail: '用户查询',
	MembersManagementDetail_Search: '进入并查询',
	MembersManagementDetail_Lock: '封號',
	MembersManagementDetail_GameSummary: '查看游戏统计',	
	MembersManagementDetail_AdvanceUpdate : '会员高级编辑',
	
	MembersManagementMoneyRecord: '金币纪录',
	MembersManagementBetRecord: '玩家进出房纪录',
	MembersManagementLock: '封号纪录',
	MembersManagementTransfer: '转账纪录',

	MembersManagement_GameVendorTransfer_Management: '转帐纪录(特殊)',
	MembersManagement_GameVendorTransfer_Search: '转帐纪录_特殊游戏_搜寻',
	MembersManagement_GameVendorTransfer_Advance: '转帐纪录_特殊游戏_进阶操作',

	
	MembersManagementDetail_AllowSimulateGameLaunch  : '允许模拟玩家进入游戏',
	MembersManagementMoneyRecord_Search   : '金币纪录_基本搜寻',
	MembersManagement_MoneyRecord_ForceFilterPlayer : '金币纪录_强制要求筛选玩家',


	GameManagement: '游戏纪录',
	GamesManagementRecord: '游戏历程查询',
	GamesManagementServersHealthSummary: '服务状态',
	GamesManagement_BetRecordSearch : '注单号查询',
	GamesManagement_BetRecordSearch_SearchList : '游戏商注单查询 ',
	GamesManagement_BetRecord_ManualRefund : '手动退还注单金币',
	GamesManagement_BetRecord_ManualSettleBet : '手动结算注单',
	GamesManagement_BetRecordSearch_UnionBetRecord : '游戏商注单查询',	
	GamesManagement_BetRecordSearch_Export : '汇出三方游戏注单',
	GamesManagement_DynamicQuery: '简易结果查询 ',
	GamesManagement_NotifyQueueList : '无缝钱包回调通知管理',

	MembersManagementThirdPartyGameLoginLogList: '三方游戏启动纪录',
	GameManagement_AGGJpAmount : '游戏彩池查询',

	Other_AccountManagement : '帐号管理',
	Other_AccountManagement_Search : '帐号管理-搜寻',
	Other_AccountManagement_Create : '帐号管理-新增',
	Other_AccountManagement_Copy : '帐号管理-複製',
	Other_AccountManagement_EditStatus: '帐号管理-修改状态',
	Other_AccountManagement_ResetPWAndValidChannel: '帐号管理-重设密码且需验证频道(客服层级用)',

	Other_BOAnnouncement_Management: '后台公告管理',
	Other_BOAnnouncement_List: '后台公告管理-搜寻',
	Other_BOAnnouncement_CreateUpdate: '后台公告管理-新增',
	
	Other_DownloadResource_Page : '资源下载',
	Other_DownloadResource_ApiDocument : 'API文檔下載',

	System_CustomerServiceTool : '客服工具',

	GameSetting: '游戏配置',
	GameSettingSwitch: '商户游戏开关',
	GameSettingSwitch_Search: '进入并查询',
	GameSettingSwitch_Update: '修改开关',



	EventManagement : '活动管理',
	SystemSetting_EventManagement: '活动列表',
	SystemSetting_EventManagement_Search: '活动列表-查询',
	SystemSetting_Event_ManualSendPrize: '活动管理_手动派发奖励',
	SystemSetting_EventDetail: '活动详细',
	SystemSetting_Event_DrawPrize : '多重奖项管理 ',

	SystemSetting_EventPrizeRecord: '活动派奖记录',
	SystemSetting_GameVendorRTPManagement: '游戏RTP管理',
	SystemSetting_Announcement: '公告/广告管理',

	SystemSetting_GeneralTag: '标签管理',
	SystemSetting_GeneralTag_ListSearch: '标签管理(搜寻)', 

	SystemSetting_OneTimeJob: '一次性排程',

	SystemSetting_Merchant_IPAddressTemplate: 'IP白名单管理',
	SystemSetting_Merchant_IPAddressTemplate_Search: 'IP白名单管理_搜寻',	
	SystemSetting_Merchant_IPAddressTemplate_AdvanceEdit : 'IP白名单管理_ 进阶编辑与新增',
	SystemSetting_Merchant_IPAddressTemplate_AppendNewIP: 'IP白名单管理_新增一組新IP',	
	SystemSetting_Merchant_IPAddressTemplate_Delete: 'IP白名单管理_删除资料',


	Statistics: '统计资料',
	StatisticsMerchantSummary: '数据报表',
	StatisticsGame: '游戏报表',
	Statistics_MerchantDailySummary: '营运日报表',
	Statistics_PlayerGameSummary: '玩家游戏报表',
	Statistics_RetentionRate: '留存率报表',
	Statistics_MerchantAllPlayerWinLoss: '玩家输赢报表',
	Statistics_GameVendorFinanceCompareReport : '三方数据比对报表 ',
	Statistics_AgentHierarchyDailySummary : '代理日报表(內部用)',
	Statistics_AgentHierarchy_TotalReport: '代理总报表',
	Statistics_MerchantWinLossRankingReport: '玩家输赢排名',
	Statistics_AgentHierarchyDailyGameReport : '代理遊戲日報表',
	Statistics_AgentSpecialReport : '代理特殊報表',
	Statistics_FreespinReport: '活动汇总报表',
	Statistics_FreespinStatusList: '免费旋转状态查询',


	Statistics_EazyQuery_ForPartner: '额外查询(合作方使用)',
	Statistics_EazyQuery_QueryInvoiceEachMerchantDetail: '(請勿亂開) 允许查询每月帐务_商户明细',

	OperatorManagement: '帐号管理',
	OperatorManagementAccounts: '帐号管理',
	OperatorManagementAccounts_Search: '进入并查询',
	OperatorManagementAccounts_Create: '新增帐号',
	OperatorManagementAccounts_Update: '编辑帐号',
	OperatorManagementAccounts_Delete: '删除帐号 ',
	OperatorManagementAccounts_AllowViewAllOrgAccounts: '允许浏览同组织所有帐号(针对非超级管理员)',

	Agent_Management : '代理管理',
	Agent_MerchantCredit : '代理管理',
	Agent_MerchantCredit_Search : '代理信用额度查詢',
	Agent_MerchantCredit_Update : '代理信用额度编辑',
	Agent_MerchantCredit_CreateChildAgent : '新增下级代理',
	Agent_MerchantCredit_EditStatus : '修改商户状态',
	Agent_MerchantCredit_EditAdvanceSetting: '代理進階編輯',
	Agent_MerchantCredit_ExportApiKey: [REDACTED]',
	Agent_MerchantCredit_BatchExportApiKey: [REDACTED]',
	Agent_MerchantCredit_AllowSetWalletModeWhenCreate: '创建子代_可以改变钱包模式',
	Agent_MerchantCredit_AllowSetIMChannelID: '创建子代_可以选择IM频道',
	Agent_MerchantCredit_DisableCreateLayer2Agent: '禁止創建二級代理',
	Agent_MerchantCredit_SyncSubAgent: '(进阶)同步子代理游戏开关',
	Agent_MerchantCredit_TriggerSwValidateTest: '触发单钱包商户 Validate Test',
	Agent_MerchantCredit_CreateDevMerchant: '创建Dev商户',

	Agent_MerchantCredit_UpdateGameOpenFlag: '(二选一)代理管理_编辑游戏开关',
	Agent_MerchantCredit_GameOpenFlag_EnableOnly: '(二选一)代理管理_编辑游戏开关_仅能开启',



	Agent_MerchantCreditChangeLog: '额度异动纪录',
	Agent_MerchantHierarchyTreeView : '代理树状结构总览',



	System_AccountingInvoice_Basic : '帳單管理_基本功能',
	System_AccountingInvoice_ExportInvoice : '帳單管理_帳單匯出與查看',

	SystemSetting: '管理员设置',
	SystemSettingOrganizations: '组织管理',
	SystemSettingOrganizations_Search: '进入并查询',
	SystemSettingOrganizations_Create: '新增组织',
	SystemSettingOrganizations_Update: '编辑组织',
	SystemSettingOrganizations_Delete: '删除组织',

	SystemSettingMerchant: '商户管理',
	SystemSettingMerchant_Search: '进入并查询',
	SystemSettingMerchant_Create: '新增',
	SystemSettingMerchant_Update: '修改',
	SystemSettingMerchant_Delete: '刪除',

	System_Merchant_CustomizedSettingList: '商户高級配置总览',

	SystemSettingServers: '服务器管理',
	SystemSettingServers_Search: ' 进入并查询',
	SystemSettingServers_Create: '新增',
	SystemSettingServers_Update: '修改',
	SystemSettingServers_Delete: '刪除',
	SystemSettingServers_Dispatch: '分配商戶',
	SystemSettingServers_DispatchBatch: '分配商戶',

	SystemSetting_GameList: '游戏管理',
	SystemSetting_GameList_Search: '游戏查询',
	SystemSetting_GameList_Create: '游戏新增',
	SystemSetting_GameList_Update: '游戏更新',

	SystemSetting_VendorMaintenaceManage: '游戏商维护管理',
	SystemSetting_VendorMaintenace: '游戏商维护',
	SystemSetting_VendorMaintenaceCreate: '添加游戏商维护',


	SystemSetting_VendorConfig: '游戏商设置',
	SystemSetting_VendorConfigList: '游戏商设置列表',
	

	GamesManagementViewList : '游戏历程-查看纪录',
	GamesManagementAdminPermission : '游戏历程-管理员进阶管理',
	SystemSetting_PayTableImageManagmenet : '投注结果图片管理',
	SystemSetting_PayTableImageList: '投注结果图片列表',

	Merchant_SelfUpdateSetting: '其他设置',
	Merchant_SelfUpdateSetting_GameSwitch : '(功能已廢棄)大厅游戏显示开关',
	Merchant_SelfUpdateSetting_Search: '商户自订游戏开关查询',
	Merchant_SelfUpdateSetting_Update: '商户自订游戏开关编辑',


	SystemSettingServerRobotConfig: '机器人配置',
	SystemSettingServerRobotConfig_Search: '进入并查询',
	SystemSettingServerRobotConfig_Create: '新增',
	SystemSettingServerRobotConfig_Update: '修改',
	SystemSettingServerRobotConfig_Delete: '刪除',


	SystemLogs_SingleWalletPlatformReqeustLog: '日志-单钱包商户请求记录管理',
	SystemLogs_SingleWalletPlatformReqeustLog_List: '单钱包户商请求记录_查询',

		 
	SystemLogs_ApiReqeustLogManagement: '日志-系统日志ApiRequestLog',
	SystemSetting_TGAnnouncement : 'TG公告批次發送',

	SystemSetting_LanguageText_Management : '多语系文字管理',
	SystemSetting_LanguageText_List : '多语系文字列表',


	SystemSettingRoles: '角色管理',
	SystemSettingRoles_Search: '进入并查询',
	SystemSettingRoles_Create: '新增',
	SystemSettingRoles_Update: '修改',
	SystemSettingRoles_Delete: '刪除',
	SystemSettingRoles_Rename: '名稱修改',

	SystemSetting_SystemConfigSetting: '系统配置',
	SystemSetting_SystemConfigSetting_Search : '系統全域設置管理',
	SystemSetting_SystemConfigSetting_Update : '系統全域設置修改',

	SystemSetting_IMChannelList: 'TG/Skype频道管理',

	System_AccountingInvoice : '帐单管理',

	SystemSetting_OneToManySettings: '多键值系统配置管理',

	SystemLogsExternalApiRequestLogManagement: '日志-三方API请求记录管理',
	SystemLogsExternalApiRequestLogList: 'API請求紀錄',

	Records_UserOperateRecord : '操作纪录',
	Records_UserOperateRecord_Search: '操作纪录_搜尋',
	Records_UserOperateRecord_SearchAllOrg: '操作纪录_查看全组织',
	
	Special_PermissionSetting: '特殊权限配置',
	Special_BetAmount_FlowAmount_Invisible: '部分页面不显示投注额栏位',
	Special_RTP_Invisible: '部分页面不显示RTP栏位',
	Special_WinLose_Invisible: '部分页面不显示WinLose栏位',

	Special_AlertNotifyWhenLogin : '登入成功时会警报',
	Special_ExportMaxSizeRestrict: '汇出功能笔数受限',	

	Special_OnlyVisible_IDG_Games: '仅显示部分IDG可见游戏',
	Special_AllowToUseExperimentFeature : '允许使用实验性功能',
	Special_AllowAssignHierarchyMerchantForUser : '商戶分配特殊權限(請勿亂勾此)',
	Special_AllowToUseDevelopeFeature: '开放使用技术专用特殊功能(请勿乱勾此)',
	Special_DefaultEnableSearchDetailCheckbox: '报表页预设勾选查询详细',
	Special_SuperAdminOrg_ButRestrictFeature: '超级管理组织_但是帐号功能受限',
	Special_PGS_CustomerService: 'PGS客服人员标记',
	Special_CanSelectedSelfRoles: '创建后台帐号_允许选自己当前角色(无视组织配置)',
	Special_Account_SearchAccountByMultiMerchantAssignMode : '允许透过多种判断条件_列出示可管理帐号(Super代理)',
	Special_AM_Permission : 'AM层级_特殊权限',
	Special_Dropdown_ShowRichmondDropdowns: '下拉選單_顯示所有Richmond客戶頻道',

	Special_SuperAdminOrg_ButNeedValidateWhenSubmit: '超级管理组织_但功能防呆更严格',


	SystemSetting_GameVendorRTPManagement_ListSearch : '游戏商RTP管理-搜寻',
	SystemSetting_GameVendorRTPManagement_Update : '游戏商RTP管理-更新RTP',

	Special_HideBeforeAfterAmount: '游戏历程_隐藏栏位_更新前后金额',
};


export const TableHeader = {
	__prompt: '提示',
	__lock: '封号',
	__ID: 'ID',
	__playerID: '玩家ID',
	__playerAccount: '玩家帐号',
	__player: '玩家',
	__displayName: '显示名称',
	__newName: '重新输入名称 ',
	__status: '状态',
	__playerType: '帐号身分',
	__walletMode: '钱包模式',
	__merchant: '商户',
	__merchantID: '商户 ID',
	__merchantName: '商户名称',
	__organizationName: '所属组织',
	__organizationID: '组织ID',
	__organization: '组织',
	__role: '角色',
	__roleAndPermission: '角色（可操作功能权限不同）',
	__roleID: '角色 ID',
	__roleName: '角色名称',
	__walletAmount: '现金数',
	__createBy: '建立人',
	__settleTime: '结算时间',
	__createTime: '创建时间',
	__createTime2: '创建时间',
	__receiveTime: '领取时间',
	__createIPAddress: '注册IP',
	__defaultBalanace: '初始金币',
	__convertRate: '转换比例',
	__defaultPassword: [REDACTED]',
	__create: '創建时间',
	__lastLogin: '最后登入',
	__lastLoginTime: '最后登入时间',
	__loginCount: '累计登入次数',
	__lastIPAddress: '最后登入IP',
	__updateTime: '更新时间',
	__lastUpdateTime: '最后更新时间',
	__lastSearchTime: '最后查询时间',
	__NotSetValue: '未设置',
	__gameInfo: '游戏讯息',
	__gameName: '游戏名称',	
	__code: '代码',
	__winCount: '赢的局数',
	__loseCount: '输的局数',
	__drawCount: '平的局数',
	__score: '输赢金币数',
	__bonusAmount: '彩金',
	__gameTimeSeconds: '游戏时长(時:分:秒)',
	__gameTime: '游戏时长',
	__total: '汇总',
	__currentPageOnly: '本頁',

	__loginRecord: '登入纪录',
	__time: '时间',
	__times: '次數',
	__hdType: '登入设备',
	__hdBrowser: '登入浏览器',

	__iPAddress: 'IP地址',
	__game: '游戏',
	__gameID: '游戏 ID',
	__gameCode: '游戏代码',
	__gameType: '游戏分类',
	__gameVendorType: '游戏厂商',
	__server: '服务器',
	__serverID: '服务器 ID',
	__serverName: '服务器名称',
	__loginTime: '登入时间',
	__logoutTime: '登出时间',

	__loginAmount: '进游戏金币',
	__currentAmount: '当前金币',
	__updateAmount: '更新金币',
	__winLossAmount: '输赢金币',
	__taxAmount: '税收',

	__othersMemo: '其他说明',
	__others: '其他',
	__memo: '備註说明',
	__reason: '原因',

	__searchField: '搜寻栏位',
	__searchText : '搜寻文字',

	__isDefault: '是否为预设值',
	__isTestEnv: '是否为测试环境',
	__lockTime: '封锁时间',
	__operateType: '封号类型',	
	__category: '分类',
	__subCategory: '次分类',
	__updateBy: '操作人',

	__type: '类型',
	__transferType: '流通类型',
	__transactionType: '交易类型',
	__orderNo: '系統单号',	
	__betRecordNoPGS: '天馬投注单号 (TransactionNo)',
	__betRecordNo: '投注单号',
	__transactionNo: '交易单号',
	__merchantOrderNo: '商户转账单号',
	__amount: '交易金额',
	__beforeAmount: '更新前金币',
	__afterAmount: '更新后金币',

	__perGameLogID: '牌局编号',
	__gameData: '游戏数据',
	__enableGamesCount: '开启游戏数',
	__enableGamesFilter: '有开启此游戏',

	__serverStatus: '服务状态',
	__onlinePlayer: '在线人数',
	__profitAmount: '今日损益',
	__flowAmount: '今日流水',
	__zeroSkipInsert: '资料为0不写入',

	__action: '操作',

	__account: '帐号',
	__BOAccount: '后台帐号',
	__userName: '用戶名',
	__displayName2: '用户显示名称',
	__password: '密码',
	__password2: '确认密码',
	__timeZoneArea: '时区',
	__totalMinutes: '总时间(分)',

	__organizationCode: '前缀',
	__merchantTotalCount: '商户数',
	__usersTotalCount: '帐号数',
	__lang: '语系',
	__organizationType: '组织类别',
	__currency: '币别',
	__apiValidatePlayerSessionUrl: '验证路由(AVPSU)',
	__apiSingleWalletGetAmountUrl: '单钱包路由',

	__flagLevel: 'FlagID',
	__roomFlagLevel: '厅房分类',
	__clientAddr: '客户端连接地址',
	__notifyAddr: '通知地址',
	__httpAddr: 'http地址',
	__assignMerchantTotalCount: '已分配商户',
	__minAvailableMoney: '入场金币',

	__minLevel: '最小智商',
	__maxLevel: '最大智商',
	__beginTime: '开始时间',
	__endTime: '结束时间',
	__minRobotCount: '最小数量',
	__maxRobotCount: '最大数量',

	__finalScore: '结算金额',
	__betAmount: '投注金额',
	__validAmount: '有效流水',
	__payoutAmount: '中奖金额',
	__merchantProfit: '商户盈利',
	__gameWinLoss: '游戏输赢',
	__playerWinLoss: '玩家输赢',

	__permission: '权限',
	__gameResult : '投注结果',

	__today: '今日',
	__yesterday: '昨日',
	__thisWeek: '本周',
	__lastWeek: '上周',
	__thisMonth: '本月',
	__lastMonth: '上月',

	__basicInfo: '基本讯息',
	__totalLoginPlayer: '总登录人数 （不重复）',
	__transferAmount: '钱包增減',
	__transferInAmount: '汇入金币',
	__transferOutAmount: '汇出金币',

	__loginInfo: '登录讯息',
	__totalLoginCount: '总登录人次',
	__newLoginPlayer: '新登录人数',
	__maxLoginPlayer: '最高在线',
	__avgLoginPlayer: '平均在线',

	__rate: '占比',
	__totalPeopleCount: '总人数',
	__totalCount: '总笔数',
	__totalRound: '总局数',
	__totalAmount: '总金额',
	__maxExportSize: '汇出笔数上限',
	__totalBetPlayer: '总游玩人数（不重复）',
	__loseAmount: '用户输',
	__winAmount: '用户赢',
	__moneyChangeAmount: '金币增减',

	__xTitle: '时间',
	__loseAmount2: '输金币',
	__winAmount2: '赢金币',
	__jackpotPoolAmount: '彩池',
	__tag : '標籤',
	__eventID : '活動ID',
	__amountOrQuantity : '数量/金额/倍率',
	__vendorConfig : '游戏商配置',
	__jackpotTag : 'JP彩池中奖',
	__referenceInfo : '参考资讯',
	__referenceNo : '参考单号',
	__eventName : '活动名称',

	__bankViewPoint: '庄家视角',
	__playerViewPoint: '玩家视角',
 
	__restrictCurrency: '限制币种',
	__supportCurrency: '支援币种',
	__supportGames: '支援游戏',
	__supportMerchants: '支援商户',



	__specialTag: '特有標記',
	__basicRule : '固定條件',
	__eventTarget: '活動完成目標',

	__extraInfo : '額外資訊',
	__imChannelName: 'IM频道名称',


	__extraFilterType : '额外筛选条件',
	__memoOrInternalDisplayName : '備註／内部显示名称',
	__internalDisplayName : '内部显示名称',
	__title : '标题',
	__description : '內容描述',


	__recentDailySummary : '近期投注日报表',
	__futureTime: '未来日期 ',
	__playerLanguageDistribute : '玩家使用语系分布',
	__playerDeviceDistribute : '玩家登入装置分布',

	__merchantAndAllChilds: '代理以及所有下级代理',
	__imageUrl: '图片网址',
	__barChart : '长条数据图',
	__pieChart: '圆饼数据图',
	__device : '裝置',
	__rtp: 'RTP',	
	__winRate: '胜率',
	__profitRate: '盈利率',
	__promotionBonus: '活动奖金',
	__promotionCost: '活动奖励',
	__promotionBonusReceive: '金币异动 (奖励领取)',
	__merchantProfitWithPromotionCost: '商户营利 (扣除活动成本)',

	__keyword: '關鍵字',
	__keywordSearch: '關鍵字查詢',
	__searchType: '查詢方式',
	
	__parent: '上级',
	__parentLine: '上级',

	__creditAmount: '信用额度',
	__adjustCreditAmount: '调整信用额度',
	__yourRemainingCreditAmount: '您的剩余额度',
	__agentRemainingCreditAmount: '代理剩余额度',
	__agent: '代理',
	__isUnlimit: '是否无限额度',
	__unlimitCredit: '無限額度',
	__operationType: '操作类型',
	__adjustValue: '调整数值', 
	 
	__beforeValue : '更新前',
	__afterValue : '更新后',
	__merchantOrAgent : '商户(代理)',
	__directCustomerData: '直客玩家数据',
	__detailOfSingleDate : '单日细项',
	__detailOfFishGame : '捕鱼详细记录',
	__FishingGame : '捕鱼',
	__currentGameRoom : '当前游戏房',
	__excludeZeroAmount : '排除数据为0的资料',
	
	__sendTarget: '發送對象',
	__FourDecimalPlaces: '小数点四位',
	__layoutOrientation: '版型',
	
	__IMChannel: 'IM群组',
	__AssignIPAddressCount: '已设置IP数',
	__searchResult: '查询结果',

	__flagGameEnable: '游戏开关',
	__flagSubAgentDefaultEnableGame: '创建子代预设勾选此游戏',
	__lobbyVisible: 'pgs大厅显示此游戏',

	__remainingRound: '剩余局数',
	__freespinID: '免费旋转ID',


	__roundID: '局号',
	__sequence: '序号',
	__sort: '排序',
	__sortDirection: '排序方向',

	__slots: '老虎机',
	__boardGame: '棋牌',
	__liveCasino: '真人视讯',
	__specialCategory: '特殊分类',
	__specialCategory2: '特殊分类2',

	__environment: '环境',
	__wager: '注单',

};

export const Popup = {
	__lock: '封号',
	__unlock: '解封',
	__batchLock: '批次封号',
	__reason: '请填写原因',

	__create: '新增',
	__update: '编辑',

	__updatePassword: [REDACTED]',
	__updateTimezone: '修改时区',

	__warningFullLoginName: '注意!登入时请使用完整帐号',
};

export const Status = {
	Enable: '启用',
	Disable: '关闭中',
	PermanentDelete: '永久隐藏',
};

export const PlayerType = {
	undefined: '---',
	Normal: '一般帐号',
	Rebot: '机器人',
};

export const MerchantsStatus = {
	undefined: '---',
	TransferMode: '转账模式',
	SingleWalletMode: '单一钱包模式',
};

export const UserOperateTypeEnum = {
	Player_LockStatus: '封号',
	Player_UnlockStatus: '解封',
	User_UserLogin: '登入',
};

export const WalletTransferTypeEnum = {
	Transfer_In: '转入',
	Transfer_Out: '转出',
};

export const TransactionTypeEnum = {
	TransferWallet_To_GameWallet: '游戏钱包转入',
	TransferWallet_To_PlatformWallet: '游戏钱包转出',
	BetGame_Settlement: '游戏结算',
};

export const ServerStatusEnum = {
	Enable: '启动中',
	Disable: '关闭禁用',
	ServerDown: '服务器端设为临时关闭',
};

export const Message = {
	__error: '错误',
	__thisMayTakeMuchTimeWatting: '此行为可能需花费较长时间等待',
	__yourCreditAmountNotEnough: '信用额度不足。请联系您的上级协助加值(topup)',
	__productionMercahntWillCalculateBilling: '此设置为「正式环境」对接使用。正式环境进行的投注数据会《计入帐务》。测试时请注意',
	__hintConvertRate: '您的商户已设定自动兑换比例(转帐模式 转入转出API)',
	__devEnvDoNotNeedSetWhiteList: '测试(Dev/Staging)环境不需设置白名单',
	__ifYouCanNotSeeDataPleaseContactUs: '若您的帐号在此页查无资料。 请联系我们技术',
	__OnlySearchSingleMerchantNotAllAgent: '此查询仅能查询单一商户，不含整个代理结构',

	__thisPageHasLimitIPAddress: '此页面有IP地区限制',
	__youCanClickThisCopyText: '您可以点击下方复制文字',

	__OneIpPerLine: '每行填一组ipAddress',

	__thisAgentIsTreeModePleaseUseAgentReport: '此商户属于代理树状商户。 请改用《代理总报表》查询、或您只能操作本功能搜寻底下子代理 (搜寻单一子代理)',

	__pleaseInputAgentWhiteListIP : '填写白名单IP，一列填写一组',


	__swValidateTest_Hint: '执行测试前。请指定一名玩家帐号，并确保符合以下条件',
	__swValidateTest_Rule1: '玩家需于近期24小时内至少投注过一次',
	__swValidateTest_Rule2: '需确保玩家最后一笔sessionToken存活有效',
	__swValidateTest_Rule3: '每天只允许测试5次 (若超过次数请联系我方技术团队)',
	__swValidateTest_Hint2: '若对以上不清楚。可与您的技术团队询问',


	__mustSelectLeftFilterWhichColumn : '左方指定查询栏位',



	__accountingIgnore: '帐务排除(帐务特殊计算)',
	__searchResultWillIncludeSubAgents: '搜尋結果將包含子代理',

	__noLongerSupport: '已废弃、已不再支援此',	
	__pleaseSelectMerchant: '请选择商户',
	__successfully: '系统提示：修改成功',
	__success: '成功',
	__PleaseCheckDownloadFile: '请查看下载档案',

	__NoAbnormality: '无异常',
	__copySuccessful: '系统提示：复制成功',
	__warning: '警告',
	__notSetValueHint: '未设置',
	__delaySettle: '延迟结算',
	__forCSOnly: '仅供客服使用',
	__permissionNotEnough: '权限不足',
	__isConfirm: '是否确认',
	__pleaseInput : '请输入..',
	__pleaseInputKeywordToSearch: '请输入关键字查询',
	__pleaseInputKeywordToSearchDropdown: '输入关键字以查询更多选项',
	__pleaseFillFormContent : '请完整输入表单必填项目',
	__isConfirmDelete: '是否确认删除',
	__isConfirmOperate: '确认进行此操作',
	__dataDelay30Minutes: '统计数据有30分钟延迟',
	__mustToExternalLinkWatchInfo: '部分投注资讯，需连结至外部连结查看',
	__singleWalletRemarkInfo: '单钱包模式不显示异动前后金额 ',
	__theDifferenceBetweenWalletMode: '*显示差异注意事项',
	__notAllGameVendorProvideDetailInfo: '部分游戏商无提供详细资讯 ',
	__someOfPlayerWillMultipleDevice: '部分玩家会使用多种装置登入',
	__disableBOAccountHint: '后台账号状态关闭仅影响后台登入。若要关闭整个商户(代理)、禁止玩家进入游戏。请至代理管理调整状态。',
	__theNotes: '*注意事项',
	__dataKeep2MonthOnly: '*仅保留2个月内资料',
	__dataKeepXMonthOnly: '*仅保留{month}个月内数据',
	__singleWalletNotSupport: '单钱包模式不提供此纪录',
	__pleaseAddIPToWhiteList: '请将这几组IP加入白名单中',
	_mustFilterPlayerID : '需筛选玩家ID或账号',

	__syncStatusToChilds: '同时关闭所有子代理',
	__areYouSureOpenAllGameByType: '是否开启所有此类游戏',



	__confirmResetPasswordToDefault: '是否将此帐户的密码修改为预设密码?',

	__pleaseClickTheBelowLink : '请透过下方连结查看开奖纪录',
	__fewOfMobileWillCopyFail : '部分裝置不支援一鍵複製、需自行複製此連結',

	__searchAllChildsOfAgent: '查出该代理直属下线',
	__searchHierarchyOfTargetAgent: '透过代理搜寻树状阶层',
	__searchChildsByParentAgent: '透过上级代理搜寻直属下级',
	__notIncludePrefix : '不含前缀',
	__atLeast_3text: '至少3個字',
	__toDisableAccountHint: '若要关闭后台帐号或修改密码。请至',

	__queryTimeWillLonger: '查询速度将会受影响',
	__limitSubAgentMaxCount1000: '限制每位总代底下最多1000位代理 (二级+三级)',

	__accountingBaseOnColumn: '帐务统计是以此时间为基准:',

	__enableGameByAgentDefault: '自动勾选游戏(建议设置)',
	__confirmYourCanOnlyEnableGame : '请注意: 您的权限只能开启游戏、不能关闭游戏。若您开启游戏后又想关闭游戏。您必须联系客服窗口或技术窗口协助关闭。是否确定执行此操作?',

	__playerId_NumberFormat: ' (系统编号)',
	__is: '要',
	__open: '开启',
	__close: '关闭',
	__fuzzyQuery: '模糊查詢',
	__game: '游戏',
	__defaultValue: '默认',
	__notSupport: '不支援此項目',
	__notSupport2: '不支援',
	__theBelowInfoOnlyCalculateStraightPlayer : '下方每位代理的数字仅包含直客玩家(未包含子代理的成绩)',

	__all : '全部',
	__adminSysLoginHint1 : '登入后台需使用',
	__adminSysLoginHint2 : '进行登入(需包含前缀)',
	__allFieldRequired : '全部栏位都是必填',

	__BOAnnouncement: '公告通知',
	__contentContainsImage: '内文包含图片',

	__agentStatusDisable_Hint1 : '状态关闭将会禁止所有玩家进入游戏。请谨慎操作',
	__agentStatusDisable_Hint2 : '若要禁止后台帐号登入，则需前往 [帐号管理] 修改',
	__canNotEditCreditThisAccount : '禁止编辑信用额度',
	__pleaseContactTechTeam : '请与技术单位确认',


	__useCategorySelectGames: '根据分类批次勾选游戏',
	__afterUpdateYouNeedSubmit: '调整更新后。记得要按储存',
};

export const Validation = {
	__requiredRules: '必填项目',
	__accountRules: '帐号必须为4-30位的字符',
	__passwordRules: '密码必须为6-30位的数字和字母的组合',
	__numberRules: '必须为正值数字.',
};


export const Lang = {
	'zh-cn': '简中',
	'zh-tw': '繁中',
	'en-us': '英文',
};

export const GameType = {
	None: 'None',
	Slot: 'Slot老虎机',
};

export const GameVendorType = {
	None: '无',
	Slot88: 'Slot88',
};

export const Currency = {
	CNY: '人民币',
	TWD: '台币',
	USD: '美元',
	MYR: '馬來幣',
	AUD: '澳幣',
	VND: '越南盾',
	SGD: '新加坡幣',
	INR: '盧比',
	IDR: '印尼盾',
	CAD: '加拿大币',
	CZK: '捷克币',
	DKK: '丹麦克朗',
	EUR: '欧元',
	GBP: '英鎊',
	HKD: '港币',
	JPY: '日元',
	KRW: '韩币',
	MXN: '墨西哥币',
	MMK: '缅甸元',
	NOK: '挪威幣',
	NZD: '纽西兰元',
	PHP: '菲律宾披索',
	PLZ: '波兰币',
	SEK: '瑞典克朗',
	THB: '泰銖',
	BETA: 'BETA幣',
	MSC_POINT: 'MSC点数',
};

export const OrganizationType = {
	SuperAdmin: '超级管理',
	Normal: '一般维运',
};

export const ServerStatus = {
	Enable: '启动中',
	Disable: '关闭禁用',
	ServerDown: '服务器端设为临时关闭',
};

export const HDTypeEnum = {
	PC: 'PC',
	Android: '安卓',
	IOS: '苹果',
};



export const $vuetify = {
	noDataText: '请输入查询条件、或您当前的条件查无资料',
	dataTable: {
		itemsPerPageText: '每页笔数',
		sortBy : '排序方式'
	},
	dataIterator: {
		noResultsText: '请输入查询条件、或您当前的条件查无资料'
	},
	dataFooter: {
		itemsPerPageText: '每页笔数',
		pageText: '{0}-{1} of {2}',
		itemsPerPageAll: 'ALL',
	},
};
export const Home = {
	__totalOnlineUsers: '线上人数',
	__totalMoneyCash: '今日金币增减',
	__totalMoneyTransferIn: '今日汇入金币',
	__totalMoneyTransferOut: '今日汇出金币',
	__totalMoney: '金币存量',
};

export const Button = {
	
	__enableEditMode: '开启编辑模式',
	__showExtraInfo : '显示额外信息',
	__create: '新增',
	__edit: '编辑',
	__delete: '删除',
	__view: '查看',

	__search: '搜索',
	__reset: '重置',
	__lock: '封号',
	__save: '储存送出',
	__submit: '确认',
	__cancel: '取消',
	__close: '关闭',
	__copy: '复制',	
	__copyNew: '复制并建立一笔新资料',
	__downloadFile: '汇出',
	__openLink: '开启连结',
	__openExternalLink: '开启快照/回播链接',
	__copyExternalLink: '复制快照/回播链接', 
	__showDetail: '查看详细',
	__disabledEdit: '未开放编辑',
	__removeRoomLock: '解除卡房/踢除玩家',
	__advanceSetting: '高级设置',
	__copyToClipboard: '复制信息至剪贴簿',
	__ManualSettlement: '手动结算',
	__ManualDepositWallet: '手动加值钱包',
	__newTabRedirect: '另开分页前往',
	__addTag: '标记标签',
	__showResultImage : '查看盘面纪录',
	__addCredit : '增加额度',
	__reduceCredit : '減少額度',
	__logout: '登出',
	__closeAllTabs: '关闭所有分页',
	__copyUrl: '复制网址',
	__copyInfo: '复制信息',
	__top10 : '最新10笔 ',
	__showMore : '查看更多 ',
	__copyTime: '复制时间',
	__JPOnly: '仅显示JP',
	__exportApiKey: [REDACTED]',
	__yes : '是',	
	__no : '否',
	__returnToTopLayer: '返回搜寻结果最上层',		
	__showEachAgentCustomerDetail: '直客数据 (每位代理)',
	__resetPasswordToDefault: '重设为预设密码',
	__upload : '上传',
	__allGames: '全游戏',
	__groupGamesAllInOne: '不分游戏(统计全游戏加总)',
	__groupAndListEachGames: '列出每款游戏',
	__appendNewIP: '增加一组新IP',
	__viewParentAgent: '返回上级',
	__selectAll: '勾选全部',
	__enableAll: '启用全部',

};

export const Filter = {
	__filter: '筛选',
	__sort: '排序',
	__sortDirection: '排序方向',
	__increasing: '递增',
};
