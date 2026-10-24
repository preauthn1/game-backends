export interface IGames {
	numberOfChildMerchant: number;
	gameSettingItems: IGameSettingItem[];
}

export interface IGameSettingItem {
	merchantID: number;
	gameID: number;
	gameName: string;
	gameNameCN: string;
	gameNameEN: string;
	gameTypeText: string;
	imageUrl : string;
	status: number | boolean;
}



export interface IGameSettingCopyRequest {

	sourceMerchantID: number;
	copyToMerchantID: number;
	isDeleteOriginalGames: boolean;

}



export interface ITable {
	merchantID: number;
	merchantName: string;
	search: string;
	numberOfChildMerchant: number;
	gameSettingItems: IGameSettingItem[];
}

export interface IMultipleMerchantAdjustGameSetting {
	type: number;
	rawText: string;
	gameIDs: number[];
	parentMerchantID: number;
}


export interface IDataTablePageObject {
	rowsPerPage: number;
	'items-per-page-options': number[];
}


export interface IMerchantSelfUpdateSetting {
	merchantID : number;
	lobbyVisibleGames : IMerchantGameFlagSelfSetting[];
}
	
export interface IMerchantGameFlagSelfSetting {
	gameID : number;
	gameName : string;
	isEnable : boolean;
}
	
	
	


