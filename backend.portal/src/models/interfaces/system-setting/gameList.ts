import * as CommonEnums from '@/models/status/common';
import * as MerchantsEnums from '@/models/status/merchant';

export interface IGameList {
	gameID: number;
	gameName: string;
	gameName_CN: string;
	gameName_EN: string;
	gameCode: string;
	gameType: number;
	gameTypeText: string;
	gameVendorType: number;
	gameVendorTypeText: string;
	status: CommonEnums.Status;	
	supportCurrency: string;
	createTime: string;
	thirdPartyGameCode: string;
	gameImageUrl : string;
	memo: string;
	
	supportedOrientationCN: string;
	supportedOrientationEN: string;

	isThirdPartyGame : boolean;
	isSkipInsertZeroBet : boolean;	
	isMultiLanguageDefined : boolean;	
	isGameListEnumDefined : boolean;
}




export interface IGameListPagingRequest {

	gameVendorType: number| null;		
	gameVendorTypes: number[];	
	status: number| null;	
	assignMerchantID: number| null;	
	excludeGameID: number | null;

	pageIndex: number;	
	pageSize: number;	
}




export interface IGameLaunchPreview {	
	gameID : number;
	gameName : string;
	vendorName : string;
	selectedPlayerID : number;
	backUrl: string;
}


export interface IGameListRequest {
	gameID: number;
	gameName: string;
	gameCode: string;
	gameType: number;
	gameVendorType: number;
	status: number;
	memo: string;
}


export interface IGamePayTableImageRequest {
	gameID: number| null;	
	pageIndex: number;	
	pageSize: number;	
}


export interface IGamePayTableImageEditRequest {
	
	id: number;	
	gameID: number;	
	code: string;	
	imageUrl: string;	
	memo: string;	
}


export interface IGamePayTableImageResponse {

	id : number;
	gameID: number;
	gameName: string;
	code: string;
	imageUrl: string;
	memo: string;

}


export interface IGameMerchantInfo {
	gameID: number;
	gameName: string;
	gameVendorType: number;
	merchants : any[];
}


export interface IGameBatchEditSettingModel {

	gameID : number;
	gameName: string;
	items : IGameCustomizedSettingEditItemModel[];
}

export interface IGameCustomizedSettingEditItemModel {

	gameID : number;
	settingKey: string;
	settingKeyText: string;
	value: string;
}
