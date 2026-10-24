import * as CommonEnums from '@/models/status/common';
import * as MerchantsEnums from '@/models/status/merchant';


export interface IEventList {

	eventID: number;
	
	eventType: number;
	eventTypeText: string;
	displayName: string;
	displayNameExternal: string;
	status: number;
	statusText: string;
	beginTime: string;
	endTime: string;
	createTime: number;
	createBy: string;
	updateTime: number;
	updateBy: string;
	numberOfMerchants: number;
	numberOfGames: number;
	
}



export interface IEventListRequest {
	
	currency : string;
	walletMode : number;
	type : number;


	pageIndex: number;
	pageSize: number;
	orderByColumn : string;
	isASC : boolean;

}




export interface IEventQuickCreateRequest {

	displayName : string;
	eventTemplateType: number;
	merchantIDs : number[];
	gameIDs : number[];
	
	extraEventParameter : IEventQuickCreateExtraParameterRequest;

}



export interface IEventAppendSettingRequest {

	eventID : number;
	settingKey : number;
	settingValue : string;

	// 純粹前端顯示用
	displayName: string;

}



export interface IEventQuickCreateExtraParameterRequest {

	vendorConfigID: number;
	isAccountCreateTimeCheck: boolean;
}



export interface IEventDetail {

	eventID : number;
	eventType : number;
	eventTypeText : string;
	displayName : string;
	status : number;
	beginTime : string;
	endTime : string;

	beginTimeTS_1000 : number;
	endTimeTS_1000 : number;

	supportGameIDs : number[];
	availableMerchantIDs : number[];
	merchantSelectTemplateID: number| null;
	alwaysRuleBundle : any;
	prizeBundles : any[];
	uiControl: IUIControlModel;

}

export interface IUIControlModel
{
	restrictGameIDs : number[];
	restrictMerchantIDs : number[];
}



export interface IGameVendorRTP {

	vendorConfigID: number;
	vendorConfigName : number;
	gameID: number;
	gameName: string;
	availableRTP : string;
	rtp : number;
	winLimit: number;
	createTime: string;
}



export interface IGameVendorRTPUpdateRequest {

	vendorConfigID : number;
	gameID : number;
	RTP : number;
	winLimit : number;
	memo : number;

}



export interface IPlayerRTPUpdateRequest {

	playerID : number;
	gameID : number;
	gameIDs : number[];
	RTP : number;
	memo : number;
	isReset : boolean;

}




export interface IGameVendorRTPRequest {
	

	pageIndex: number;
	pageSize: number;
	orderByColumn : string;
	isASC : boolean;

}


export interface IManualSendEventPrizeRequest {

	playerID : number;
	eventID : number;
	
}



export interface IPrizeRecordList {

	createTime : string;
	receiveTime: string;
	merchantName : string;
	playerID : string;
	playerName : string;
	eventName : string;
	eventPrizeTypeName : string;
	amountOrQuantity : string;
	updateAmount : string;
	statusText : string;
	gameID: number;
	gameName : string;
	memo : string;
	referenceNo : string;
	pgsTransactionNo: string;
	displayOrderNo2: string;
	
}


export interface IPrizeRecordRequest {

	timeColumn: number; // 純粹前端使用

	eventID : number;
	playerID : string | null;
	playerName : string | null;
	multipleOrderColumnSearch : string | null;



	merchantID : number ;
	status: number;

	createTimeBegin : string | null;
	createTimeEnd : string| null;


	receiveTimeBegin: string| null;
	receiveTimeEnd: string| null;

	pageIndex: number;
	pageSize: number;
	orderByColumn : string;
	isASC : boolean;

}








export interface IEventDrawPrizeList {

	eventDrawPrizeID : number;
	eventID : number;
	eventName : string;
	type : number;
	typeText : string;
	status : number;
	statusText : string;
	displayName : string;
	memo : string;
	updateTime : string;
	
}



export interface IEventDrawPrizeRequest {
	
	displayNameLikeQuery: string | null;
	pageIndex: number;
	pageSize: number;
	orderByColumn : string;
	isASC : boolean;
}


export interface IEventDrawPrizeDetailInfo {
	
	eventDrawPrizeID : number;
	type : number;
	displayName : string;
	items : any[];
	pageControl : IEventMultiplePrizePageControl;

}


export interface IEventMultiplePrizePageControl {
	
	amountColumnDisplayText: string;
	extraDescription: string | null;
	isMaxReferenceValueVisible: boolean;
	isProbabilityWeightsDisable: boolean;
}


export interface IEventDrawPrizeCloneRequest {

	eventDrawPrizeID: number;
	displayName : string;

}




export interface IEventRankPreviewRequest {
	eventID : number;
	pageSize : number;
	orderByType : number;
}

export interface IEventRankPreview {
	isPrizeEmpty : boolean;
	isJobExist: boolean;
	defaultCompetiteType: number;
	players : IEventRankPreviewViewItem[];
}

export interface IEventRankPreviewViewItem {
	playerID : number;
	playerName : string;
	totalRound : number;
	totalBetAmount : number;
	totalPayoutAmount : number;
	totalWinLoss : number;
}


export interface IEventCopyRequest {
	sourceEventID : number;
	name : string;
	displayNameExternal: string;

	// 純粹前端顯示用
	sourceEventName: string;
}


export interface IEventContentBlockEditModelResponse {
	eventID : number;
	isNotInitial : boolean;
	eventContentBlocks : IEventContentBlockParent[];
}

export interface IEventContentBlockEditModelResponse {
	eventID : number;
	isNotInitial : boolean;
	eventContentBlocks : IEventContentBlockParent[];
}

	
export interface IAdminSysContentBlockItem {
	languageCode : string;
	ownerGuid : any;
	title : string;
	description : string;
	imageUrlTop : string;
	imageUrlMiddle : string;
	imageUrlBottom : string;
}
	

export interface IEventContentBlockParent {
	eventContentBlockID : number;
	eventID : number;
	eventContentBlockGuid : any;
	memo : string;
	layoutType : string;
	styleType : string;
	sequence : string;
	status : string;
	contents : IAdminSysContentBlockItem[];
}
	


export interface IEventContentBlockEditModelResponse {
	eventID : number;
	isNotInitial : boolean;
	eventContentBlocks : IEventContentBlockParent[];
}


export interface ISelectMerchantByParentAgentRequest {
	eventID : number;
	merchantSetting : ISelectMerchantByParentAgentDetailMerchantSelect;
}


export interface ISelectMerchantByParentAgentDetailMerchantSelect {
	merchantID : number;
	excludeMerchantIDs : number[];
	isResetToInitial : boolean;
}



export interface IExportPrizeValidateRequest
{
	merchantID : number;
	begin : string | null;
	end : string| null;

	totalCount: number;
	totalAmount: number;
	timeZone: number;
}

export interface IExportPrizeValidateResult
{
	totalCount_Export: number;
	totalCount_StatDB: number;
	totalAmount_Export: number;
	totalAmount_StatDB: number;
}
