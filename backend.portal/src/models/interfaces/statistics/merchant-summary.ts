import * as ResponseModel from '@/models/interfaces/commons/response';

export interface IMerchantSummarySearchRequest {
	begin: string | null;
	end: string | null;
	isQueryExtraReportInfo : boolean | null;
	merchantIDs: number[] | null;
	includeExtraSearchInfo: number[] | null;
}

export interface IGameReportRequest {
	begin: string | null;
	end: string | null;
	isMergeChildAgentData: boolean;
	merchantIDs: number[] | null;
}

export interface IChildAgentSingleDateSummaryRequest {
	begin: string | null;
	end: string | null;

	parentMerchantID : number;
	timeZone : number;
	aggregativeType : number;
}

export interface IMerchantDailySearchRequest {
	begin: string | null;
	end: string | null;
	timeZone: number;
	merchantID: number | null;
	gameIDs: number[];
	isIncludeExtraInfo : boolean;
}

export interface IMerchantAllPlayerWinLossReportRequest {
	begin: string | null;
	end: string | null;
	merchantID: number | null;
}


export interface IPlayerWinLossRankingReportRequest {
	begin: string | null;
	end: string | null;
	merchantID: number;
	vendorConfigID: number | null;
	sortByDirection: number | null;
	pageIndex: number;
	pageSize: number;
	gameID: number | null;
	orderByColumn: string;	
	isASC: boolean;
	isGroupAllGameOne: boolean;
	reportType: number;

}

export interface IPlayerWinLossRankingDetailItem {
	playerID: number;
	playerName : string;
	merchantID: number;
	merchantName: string;
	gameID: number;
	gameName: string;
	betAmount: number;
	payoutAmount: number;
	winLossAmount: number;
	rtp: number;
	winRate: number;
	totalRound: number;
}


export interface IPlayerWinLossRankingSearchResult {
	playerID: number;
	playerName : string;
	pagingItems: ResponseModel.IPageListReponse<IPlayerWinLossRankingDetailItem>;
}


export interface IFreespinReportRequestModel {

	begin: string | null;
	end: string | null;
	merchantID: number;
	vendorConfigID: number;
	gameID: number | null;
	
}

export interface IFreespinReportResponseModel {
	begin: string | null;
	items : any[];
	totalSummary: any;
}


export interface IFreespinStatusListRequestModel {

	begin: string | null;
	end: string | null;
	merchantID: number;
	vendorConfigID: number;
	gameID: number | null;


	pageSize: number;
	pageIndex: number;	
}


export interface IReponse {
	merchants: IMerchant[];
}


export interface IMerchantDailyReport {

	merchantID : number;
	merchantName : string;
	merchantCurrency : string;
	items: IMerchantDailyReportItem[];
	isSearchFishGameExtraInfo : boolean;
	totalSummary : IMerchantDailyReportItem;
	hierarchyExtraInfo : any;
}

export interface IAgentSpecialReport {

	merchantDataItems: any[];
	totalSummary : any;
}

export interface IMerchantDailyGameReportRequest {

	merchantID : number;
	merchantName : string;

	begin : string | null;
	end : string | null;

	timeZone: number;

	pageSize: number;
	pageIndex: number;
}

export interface IMerchantDailyGameReportResult {

	serverTimeTW: string;
	pagedList: ResponseModel.IPageListReponse<any>;

}


export interface IMerchantAllPlayerWinLossReportResponse {
	items: IMerchantAllPlayerWinLossItem[];
	totalSummary : IMerchantAllPlayerWinLossItem;
	isShowGoToAgentReportHint: boolean;
}



export interface IMerchantAllPlayerWinLossItem {

	playerID : number;
	playerName : string;
	totalRound : number;
	taxAmount: number;
	betAmount: number;
	payoutAmount: number;
	validAmount: number;
	winLossAmount: number;
	rtp: number;
}



export interface IAgentHierarchyDailyReportRequest {
	
	begin: string | null;
	end: string | null;
	timeZone: number;
	aggregativeType : number;
	merchantID: number | null;
}


export interface IAgentSepcialReportRequest {
	
	begin: string | null;
	end: string | null;
	agentReportType : number;
	sortDirection: number;
	parentMerchantID: number | null;
}


export interface IMerchantDailyReportItem {
	date: string;
	begin : string;
	end: string;
	merchantID: number;
	merchantName: string;
	totalLoginCount: number;
	totalRound : number;
	transferInAmount: number;
	transferOutAmount: number;
	totalLoginPlayer: number;
	taxAmount: number;
	betAmount: number;
	payoutAmount: number;
	validAmount: number;
	winLossAmount: number;
	merchantWinLossAmount: number;
	rtp: number;
	promotionCostAmount: number;
	merchantWinLossAmountWithPromotionCost: number;
}

export interface IMerchant {
	merchantID: number;
	merchantName: string;
	basicSummary: IBasicSummary;
	loginSummary: ILoginSummary;
	gameSummary: IGameSummary;
	gameVendorSummary: IGameVendorSummary;
	languageDistributeReport: ICountReportSummary[];
	deviceDistributeReport: ICountReportSummary[];
	playerTWDBetAmountDistribute : IBetAmountDistribute;
}

export interface IBetAmountDistribute {	
	currency : string;
	exchangeRate : number;
	items : IBetAmountDistributeItem[];
}


export interface IBetAmountDistributeItem {
	
	maxBetAmount : number;
	minBetAmount : number;
	totalCount : number;
	rate : number;
	rate_100X : number;
	playerIDs : number[];
}



export interface IGameVendorSummary {
	
	items : IGameVendorSummaryItem[];
	totalSummary : IGameVendorSummaryItem[];

}

export interface ICountReportSummary {
	
	memo: string;
	displayName: string;
	totalCount: number;
	rate: number;
}


export interface IGameVendorSummaryItem {
	
	betAmount: number;
	betPlayer: number;
	gameVendorType: number;
	gameVendorTypeText: string;
	payoutAmount: number;
	rtp: number;
	totalRound: number;
	validAmount: number;
	winLossAmount: number;

}


export interface IGameSummary {
	merchantName: string;
	totalRound: number;
	totalBetPlayer: number;
	winAmount: number;
	loseAmount: number;
	moneyChangeAmount: number;
	betAmount: number;
	validAmount: number;
	payoutAmount: number;
	totalPromotionCost: number;
	rtp: number;
}

export interface ILoginSummary {
	merchantName: string;
	totalLoginCount: number;
	newLoginPlayer: number;
	maxLoginPlayer: number;
	avgLoginPlayer: number;
}

export interface IBasicSummary {
	merchantName: string;
	merchantID: number;
	totalLoginPlayer: number;
	transferAmount: number;
	transferInAmount: number;
	transferOutAmount: number;
	walletAmount: number;
	currency : string;
}



export interface IRetentionReportResponse {

	items : IRetentionReportResponseItem[];
}


export interface IRetentionReportResponseItem {
	merchantID: number;


	dateText : string;
	newPlayerCount : number;
	day1: any;
	day3: any;
	day7: any;
	day14: any;
	day30: any;
}



export interface IRetentionReportRequest {
	merchantID: number;
	begin: string | null;
	end: string| null;
	timezone: number;	
}


export interface IQueryVendorConfigFinanceCompareRequest {
	vendorConfigID : number;
	beginTime : string | null;
	endTime : string| null;
	isDisableFakeData : boolean;
	isDescendingOrder: boolean;
}


export interface IVendorConfigFinanceCompareResponse {
	exportFileNamePrefix: string;
	debugSQL: string;
	isHideExportButton: boolean;
	merchants: any[];
	merchantsDataExist: any[];
	assignVendorID: string;
	vendorConfigID: number;
	gameVendorID: number;
	currency: string;
	items : IVendorConfigFinanceCompare[];


	totalRTP: number;
	totalDiff: number;
	totalGameVendorSideWinloss: number;
	

	totalSummary : ISettleDateDiff;
	totalSummaryAfterAdjustment : ISettleDateDiff;
	totalSummaryThirdParty: ISettleDateDiff;
	vendorConfigName: string;
	eachMerchantTotalSumCustomizedDisplayFormat: any;
}


export interface ISettleDateDiff {
	perGameDetailLogIDs : number[];
	totalBetAmount : number;
	totalWinLossAmount : number;
	totalRound : number;
}

export interface IVendorConfigFinanceCompare {
	pgsSettleThisDate_But_ThirdSettleOtherDate : ISettleDateDiff;
	thirdSettleThisDate_But_PgsSettleOtherDate : ISettleDateDiff;
	pgsAfterAdjustment: ISettleDateDiff;
	fetchThirdPartyStatisticReport : ISettleDateDiff;

	diff_AggregateByThirdSettleDate_vs_FetchThirdPartyStatisticReport: any;

	isFetchDataAllZero: boolean;
	isCompareCorrect_ByGameVendorSettleDate: boolean;
	isCompareCorrect_AfterAdjustment: boolean;
	isCompareCorrect : boolean;
	isRoundNotEqualOnly_ByGameVendorSettleDate: boolean;
	isAnyAdjustmentByDiffDate : boolean;
	pgsStatDB: any;
	extraMemoInfo : string;
	totalAdjustRecordCount : number;
	statDate : string;
	statDateText : string;	
	beginUTC: string;
	endUTC: string;	
}


export interface IQueryHierarchyAgentTotalReportReportApiRequestParameter {
	merchantID : number;
	isQueryDirectAgentOnly : boolean;
	timeZone : any;
	begin : string | null;
	end : string | null;
	isExcludeZeroData : boolean;
	includeExtraSearchInfo: number[];
	gameIDs: number[] | null;
}
	
export interface IQueryAgentSummaryReportByHierarchyChildMappingResult {
	exportFileSuffixText: string;
	targetMerchantID : number;
	targetMerchantName : string;
	items : IQueryDirectAgentSummaryReportByHierarchyChild[];
	gameTypeSummaryItems: any[];
	serverTimeTW: string;
}

	
export interface IQueryDirectAgentSummaryReportByHierarchyChild {
	merchantID : number;
	merchantName : string;
	isSelfCustomer : boolean;
	totalRound : number;
	totalPlayerCount: number;
	betAmount : number;
	validAmount : number;
	payoutAmount : number;
	winLossAmount : number;
	merchantWinLossAmount: number;
	eventCostAmount: number;
	profitRate: number;
	merchantProfitWithCost: number;
}
	



export interface IExportFinanceReportDateMemo {

	beginText : string;
	endText : string;
	items: IExportFinanceReportDateMemoItem[];
}



export interface IExportFinanceReportDateMemoItem {
	vendorConfigID : string;
	currency : number;
	vendorConfigName : number;
	date : number;
	nemo : number;
}

