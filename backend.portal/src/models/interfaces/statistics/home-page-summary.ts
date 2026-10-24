export interface IReponse {
	merchantID: number;
	merchantName: string;
	totalOnlineUsers: number;
	totalMoneyCash: number;
	totalMoneyTransferIn: number;
	totalMoneyTransferOut: number;
	totalMoney: number;
}


export interface IFinanceOverviewReport {
	merchantID: number;
	merchantName: string;
	totalOnlineUsers: number;
	totalMoneyCash: number;
	totalMoneyTransferIn: number;
	totalMoneyTransferOut: number;
	totalMoney: number;
}


export interface IHomePageTodayInfo {
	currentOnlinePlayers: number;
	totalPlayers_Today: number;
	totalPlayers_Yesterday: number;	
	totalRound_KValue_Today: number;
	totalRound_KValue_Yesterday: number;
}
