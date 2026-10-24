export interface ISearchRequest {
	begin: string | null;
	end: string | null;
	merchantIDs: number[] | null;
}

export interface IGameListReponse {
	merchants: IMerchant[];
	isQuerySummaryByHierarchyMode: boolean;
}

export interface IMerchant {
	merchantID: number;
	merchantName: string;
	items: IItem[];
	totalSummary: IItem;
	categoryReportItems: IItem[];
}

export interface IItem {
	gameID: number;
	gameName: string;
	gameTypeText: string;
	totalRound: number;
	onlinePlayer: number;
	updateAmount: number;


	betAmount : number;
	validAmount : number;
	payoutAmount : number;
	rtp: number;


	winAmount: number;
	loseAmount: number;
	taxAmount: number;
	flowAmount: number;
	jackpotPoolAmount: number;
}




export interface IPlayerGameSummaryRequest {
	playerID : number | null;
	playerName : string | null;
	begin : string | null;
	end : string| null;
}



export interface IPlayerGameSummary {

	playerName : string;
	playerID : number;
	totalSummary : IPlayerGameSummaryItem;
	playerGameSummary: IPlayerGameSummaryItem[];
	eventBonusSummary: IPlayerEventSummaryItem[];
	totalEventSummary: IPlayerEventSummaryItem;
}



export interface IPlayerGameSummaryItem {

	merchantID : number;
	merchantName : string;
	playerName : string;
	gameID : number;
	gameName : string;
	totalRound : number;
	betAmount : number;
	validAmount : number;
	payoutAmount : number;
	winLossAmount : number;
	rtp : number;
	taxAmount : number;

}

export interface IPlayerEventSummaryItem {
	merchantID : number;
	merchantName : string;
	playerName : string;
	eventID : number;
	eventName : string;
	totalCount : number;
	totaAmount : number;
}