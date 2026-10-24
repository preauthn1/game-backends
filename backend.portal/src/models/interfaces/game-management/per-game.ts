export interface IListRequest {
	playerID: number;
	perGameLogID: number;
	gameID: number | string;
	serverID: number | string;
	merchantID: number;
	playerName: string;
	gameStartTimeBegin: string | null;
	gameStartTimeEnd: string | null;
	transactionNo: string | null;
	isJackpotOnly: boolean | null;
	tagID: number | null;
	payoutAmountLargeThan: number | null;

	pageIndex: number;
	pageSize: number;
	orderByColumn: string | null;
	isASC: boolean | null;
}

export interface IListResponse {
	perGameDetailLogID : number;
	perGameLogID: number;
	serverID: number;
	serverName: string;
	gameID: number;
	gameName: string;
	thirdPartyGameCode: string;
	playerID: number;
	playerName: string;
	merchantID: number;
	merchantName: string;
	currentAmount: number;
	updateAmount: number;
	gameTime: string;
	afterAmount: number;
	betAmount: number;
	jackpotAmount: number;
	payoutAmount: number;
	validAmount: number;
	taxAmount: number;
	isExternalVendorGame: boolean;
	vendorName: string;
	thirdPartyBetRecordNo : string;
	transactionNo : string;
	isHideAfterAmountInfo : boolean;
	isAnyPrizeRecordReference: boolean;
}

export interface IUnionBetRecordSearchRequest {
	playerID: number;
	gameID: number | string;
	merchantID: number;
	playerName: string;
	createTimeBegin: string | null;
	createTimeEnd: string | null;
	betRecordNo: string | null;
	transactionNoPGS: string | null;
	roundID: string | null;
	gameVendorID: number | null;
	pageIndex: number;
	pageSize: number;
	status: number;
	orderByColumn: string | null;
	isASC: boolean | null;
}


export interface IUnionBetRecordResponse {
	betRecordNo : string;
	betRecordNoPGS : string;
	merchantID : number;
	merchantName : string;
	currency: string;
	gameVendor : number;
	gameVendorName : string;
	gameID : number;
	gameName : string;
	playerID : number;
	playerName : string;
	betAmount : number;
	winLossAmount : number;
	statusText : string;
	createTime : string;
	settlementTime : string;
	merchantNotifyQueueID: number;


	typeName : string;
	 payoutAmount: number;
}

export interface IBetRecordCopyInfoResponse {
	message: string;
}


export interface IBetRecordNotifyQueue {
	
	id: number | null;
	type: number;
	betRecordNoPGS: string | null;
	retryTimes: number;
	isSuccess: boolean;
	lastRequestUUID: string;
	typeName: string;
	logs : ISingleWalletApiRequestLog[];
}


export interface IBetRecordNotifyQueuePageQuery {
	
	id: number | null;
	type: number;
	betRecordNoPGS: string | null;
	retryTimes: number;
	isSuccess: boolean;
	lastRequestUUID: string;
	typeName: string;
}

export interface IBetRecordNotifyQueueRequestParameter {
	
	betRecordNoPGS: string | null;
	pageIndex: number;
	pageSize: number;
	createTimeBegin: string | null;
	createTimeEnd: string | null;
	orderByColumn: string | null;
	isASC: boolean | null;

}



export interface ISingleWalletApiRequestLog {
	
	id: number;
	category: string;
	subCategory: string;
	requestID: string;
	requestUrl: string;
	requestContent: string;
	requestHeader: string;
	responseText: string;
	responseTime: string;
	responseStatusCode: string;
	webSiteName: string;
	memo: string;
	createTime: string;

}




export interface IBetRecordInfo {

	gameVendorID: number;
	gameVendorName: string;
	isSupportExternalLink: boolean;
	isSingleWalletMode : boolean;

	copyInfoForGameVendor: string;


	generalInfo: IBetRecordGeneralInfo;
	perGameLogInfo: IBetRecordPerGameInfo;
	vendorConfigAssignMerchantID: string;
	currency: string;
}


export interface IManualSettleRequest {
	transactionNo: string | null;
	payoutAmount: string| null;
	validAmount: string| null;
	memo: string| null;
}


export interface IManualChangeSettleInfoRequest {
	transactionNo: string | null;
	payoutAmount: string| null;
	validAmount: string| null;
	memo: string| null;
	isTriggerCallback: boolean;
}


export interface IManualWritePerGameRequest {
	
	transactionNoPGS: string| null;

	isReplaceGameTimeToNowTime: boolean;
	isWriteSingleWalletNotifyQueue: boolean;
	isUpdatePayoutAmount: boolean;

	memo: string| null;

}


export interface IBetRecordGeneralInfo {

	betRecordNo: string;
	betRecordNoPGS: string;
	thirdPartyGameCode: string;
	merchantID: number;
	gameID: number;
	gameName: string;
	playerID: number;
	playerName: string;
	playerIPAddress: string;
	betAmount: number;
	payoutAmount: number;
	validAmount: number;
	winLossAmount: number;
	status: number;
	createTime: string;
	updateTime: string;
	settlementTime: string;
	statusText: string;
	isManualSettlement: boolean ;
	manualSettlementUserName: string;

}


export interface IBetRecordPerGameInfo {

	perGameDetailLogID: number;
	perGameLogID: number;

}


export interface IPerGameDetailExternalInfoPopup {

	gameName: string;
	vendorName : string;
	thirdPartyBetRecordNo : string;
	targetPerGameDetailLogID: number;
	isASC: boolean | null;
}


export interface IBetRecordManualRefundRequestModel {	

	transactionNo : string;
	memo : string;
}


export interface IExternalUrlResult {	
	url: string;
	infoType : number;
	rawJson : string;
}

export interface IDetailResponse {
	playerDetails: IPlayerDetail[];
	perGameLogID: number;
	serverID: number;
	serverName: string;
	gameID: number;
	gameName: string;
	gameData: string;
	beginTime: string;
	endTime: string;
}

export interface IPlayerDetail {
	perGameLogID: number;
	serverID: number;
	serverName: string;
	gameID: number;
	gameName: string;
	playerID: number;
	playerName: string;
	merchantID: number;
	merchantName: string;
	currentAmount: number;
	updateAmount: number;
	gameTime: string;
}




export interface ISlotHistoryDetailLogResponse {

	perGameDetailLogID : number;
	slotGameHistoryLogID: number;
	perGameLogID : number;
	gameID: number;
	gameName: string;
	playerID: number;
	betAmount: number;
	updateAmount: number;
	gameTime: string;
	isMultipleSlotsResult: boolean;
	beforeAmount : number;
	afterAmount : number;

	lineResult: ISlotHistoryResultIcon[][];
	distinctIcons: ISlotHistoryResultIcon[];

}




export interface ISlotHistoryResultIcon {

	code : string;
	imageUrl: string;

}

	
export interface IAGGJackpotAmount {
	items : IAGGJackpotAmountItem[];
}
	
export interface IAGGJackpotAmountItem {
	groupID : number;
	groupName : string;
	jPAmountName : string;
	jPAmount : number;
	isDefined : boolean;
}
	
	
export interface IPerGameAddTagRequest {

	perGameDetailLogID : number;
	tagID : number;

}
	
export interface IFishGameDetailRecordQueryRequest {
	transactionNo : string;
}

export interface IFishGameDetailInfoRecord {
	items : IFishGameDetailRecordItem[];
}

export interface IFishGameDetailRecordItem {
	playerName : string;
	gameID : number;
	roundID : string;
	wagerID : string;
	fishType : number;
	fishTypeText : string;
	betTime : string | null;
	betAmount : number;
	payoutAmount : number;
	validAmount : number;
	winLossAmount : number;
}


export interface IFindNotSettleRecordRequest {	
	gameVendorID: number;
	beginTime: string;
	endTime: string;
}

export interface IFindNotSettleRecordResult {		

	transactionNoList: string[];
	transactionNoLsitLineBreak: string;
}


