import * as CommonEnums from '@/models/status/common';
import * as MerchantsEnums from '@/models/status/merchant';
import * as PlayersEnums from '@/models/status/players';
export interface IPlayersRequest {
	playerID: number;
	status: CommonEnums.Status | null;
	merchantID: number;
	createTimeBegin: string | null;
	createTimeEnd: string | null;
	lastLoginTimeBegin: string | null;
	lastLoginTimeEnd: string | null;
	playerName: string;
	playerNameFuzzyQuery : string | null;
	lastIPAddress: string;
	createIPAddress: string;
	pageIndex: number;
	pageSize: number;
	orderByColumn: string | null;
	isASC: boolean | null;
}

export interface IPlayer {
	// isCheck: boolean | null;
	playerID: number;
	status: CommonEnums.Status;
	playerName: string;
	merchantID: number;
	merchantName: string;
	walletAmount: number;
	walletMode: MerchantsEnums.WalletMode | null;
	walletModeText: string;
	createTime: string;
	createIPAddress: string;
	lastLoginTime: string;
	lastIPAddress: string;
}

export interface IPlayerDetail {
	playerID: number;
	playerName: string;

	isFishWalletVisible: boolean;
	isTestAccountOrMerchant : boolean;
	playerType : number;
	playerTypeText : string;
	displayName: string;
	status: number;
	merchantID: number;
	merchantName: string;
	walletAmount: number;
	walletMode : number;
	riskScore: number;
	createTime: string;
	createIPAddress: string;
	copyTextForCS: string;
	lastLoginTime: string;
	totalSummary: ITotalSummary;
	playerGameSummaries: IPlayerGameSummary[];
	playerRecentlyBetSummary: IPlayerDailyGameSummary[];
	playerGameSummaryRowTotal: IPlayerGameSummary;
	lastBetDateTimestamp : number | null;
	memo: string;
	playerTags: any[];
}

export interface IPlayerDetailSearchRequest {

	playerNameOrID: string | null;
	merchantID : number | null;
}

export interface IInternalTestAccountCreateRequest {

	playerName: string;
	merchantID: number;
	isAddToInternalTestAccount: boolean;

}


export interface IPlayerGameSummary {
	gameName: string;
	playerID: number;
	gameID: number;
	winCount: number;
	loseCount: number;
	drawCount: number;
	score: number;
	lastWinLose: number;
	gameTimeSeconds: number;
	createTime: string;
	updateTime: string;
	betAmount: number;
	validAmount: number;
	payoutAmount: number;
	gameVendorType : number;
	gameVendorTypeText : string;
	gameVendorConfigID : number;
	gameVendorConfigName : string;
	isSupportRTPConfig : boolean;
	rtpConfigValue : number;
	lastBetTime : string;
	lastBetTimeTS : number;
}



export interface IPlayerDailyGameSummary {
	
	date : string;
	dateText : string;
	TotalRound: number;
	betAmount: number;
	validAmount: number;
	taxAmount: number;
	payoutAmount: number;
	winLossAmount: number;
	rtp: number;
}





export interface ITotalSummary {
	totalLoginTimes: number;
}

export interface ILockRequest {
	playerID: number;
	memo: string;
	status: number;
}

export interface IBatchLockRequest {
	playerIDs: number[];
	memo: string;
	status: number;
}

export interface ILoginDetailResponse {
	playerID: number;
	playerName: string;
	merchantID: number;
	merchantName: string;
	requestUUID: string;
	gameID : number;
	gameName: string;
	statusText: string;
	vendorName: string;
	vendorID: number;	
	hdType: number;
	hdBrowser: string;
	createTime: string;
	createIPAddress: string;
	memo: string;
	isLaunchRecordExist: boolean;
}

export interface ILoginDetailRequest {
	playerID: number;
	playerName: string;
	vendorID : number| null;
	createTimeBegin: string | null;
	createTimeEnd: string | null;
	pageIndex: number;
	pageSize: number;
	orderByColumn: string | null;
	isASC: boolean | null;
	isThirdPartyGame : boolean | null;
}

export interface ILockRecordReponse {
	playerID: number;
	playerName: string;
	operateType: number;
	merchantID: number;
	vendorID: number| null;
	merchantName: string;
	memo: string;
	updateBy: string;
	createTime: string;
	createIPAddress: string;
}

export interface ILockRecordRequest {
	merchantID: number;
	playerID: number;
	playerName: string;
	createTimeBegin: string | null;
	createTimeEnd: string | null;
	memo: string | null;
	pageIndex: number;
	pageSize: number;
	orderByColumn: string | null;
	isASC: boolean | null;
}

export interface IBetRecordReponse {
	playerID: number;
	playerName: string;
	merchantID: number;
	merchantName: string;
	walletMode: MerchantsEnums.WalletMode | null;
	gameID: number;
	gameName: string;
	serverID: number;
	serverName: string;
	tradeID: number;
	loginAmount: number;
	updateAmount: number;
	taxAmount: number;
	winCount: number;
	loseCount: number;
	drawCount: number;
	ipAddress: string;
	gameTimeSecond: number;
	loginTime: string;
	logoutTime: string;
}

export interface IBetRecordRequest {
	gameID: number;
	serverID: number;
	playerID: number;
	playerName: string;
	iPAddress: string;
	tradeID: number;
	loginTimeBegin: string | null;
	loginTimeEnd: string | null;
	logoutTimeBegin: string | null;
	logoutTimeEnd: string | null;
	pageIndex: number;
	pageSize: number;
	orderByColumn: string | null;
	isASC: boolean | null;
}

export interface IMoneyRecordReponse {
	playerID: number;
	playerName: string;
	merchantID: number;
	transactionType: PlayersEnums.TransactionTypeEnum;
	transactionTypeText : string;
	afterAmount: number;
	createTime: string;
	gameID: number;
	ipAddress: string;
	// serverID: number;
	walletMode: MerchantsEnums.WalletMode | null;
	merchantName: string;
	currentAmount: number;
	updateAmount: number;
	gameName: string;
	referenceNo: string;
	// serverName: string;
}

export interface IMoneyRecordRequest {
	merchantID: number;
	playerID: number;
	playerName: string;
	gameID: number;
	transactionType: PlayersEnums.TransactionTypeEnum;
	referenceOrderNo : string;
	iPAddress: string;
	createTimeBegin: string | null;
	createTimeEnd: string | null;
	pageIndex: number;
	pageSize: number;
	orderByColumn: string | null;
	isASC: boolean | null;
}


export interface IPlayerTagUpdate {

	playerID: number;
	tagID: number;
}

export interface IInternalTestAccountPlusWalletRequest {
	playerID : number; 
	confirmMerchantID : number; 
	confirmPlayerName : string; 
	amount : number; 
	memo : string;
}

export interface IFetchAGGBalance {
	playerID: number;
	balance: number;
}