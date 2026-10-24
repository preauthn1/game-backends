export interface IResponseData<T> {
	data: T;
	code: number;
	message: string;
}

export interface ICustomizedColumnDisplayResult<T> {
	columnTranslateMapping: any;
	extraDisplayName: string;
	extraDisplayType: number;
	extraInfo: any;
	items: T[];
	extraObject?: any;
}

export interface IPageListReponse<T> {
	items: T[];
	pageIndex: number;
	pageSize: number;
	totalCount: number;
	totalPage: number;
	extraObject?: any;
}

export interface ICRUDResult {	
	isSuccess: boolean;
	message:string;
	debugMessage: string;

	resultExtraString: string;
	resultTime: string;
	resultId : number;
	resultListText : string[];
	resultListIDs : number[];
	resultListObject : any[];
}



export interface IInternalTestRequest {	

	playerID: number;
	gameID: number;
	lang: string;
	doNotIframe: boolean;
	backUrl: string;
}

export interface ICustomerServiceUtilityInfo {	

	refreshTimeTW: string;
	warningMessages: any[];
}


export interface IInternalPlayerExtraInfo {	
	isVisible : boolean;
	isIframeEnable : boolean;
	isLobbyBackUrl : boolean;
	isAlwaysTrialMode : boolean;
}


export interface IInternalTestResult {	
	url: string;
}

export interface ICustomizedColumnRequest {		
	pageIndex: number;
	pageSize: number;
	querySqlID: number | null;
	playerID: number | null;
	playerName: string;
	merchantID: number | null;
	gameID : number | null;
	beginTime: string| null;
	endTime: string| null;	
	sortDirection: number | null;
	sortByColumn: string;
}

export interface ILazyCommonResponseV2 {	
	responseText: string;
}

export interface ICommonCustomizedSettingBatchUpdate {
	
	ownerID: number;
	ownerName: number;
	items: ICommonCustomizedSettingItemEditUpdate[];

}

export interface  ICommonCustomizedSettingItemEditUpdate {	
	ownerID: number;
	settingKey: number;
	settingKeyText: number;
	value: number;
}


export interface ICustomizedColumnResult {		
	xxxx: string;
	data: string;
}

export interface IDynamicQueryCommandSettingResult {		
	displayName: string;
	memo: string;
	filterFormItems: string;
	id: string;
	availableFilterFormItems: string[];
	defaultPageSize: number;
	extraSetting : IDynamicQueryExtraSetting;
}


export interface IDynamicQueryExtraSetting {		
	isCsvFormat : boolean;
}



export interface IDynamicQueryCategoryDropdowns {		
	eachCategory : IDynamicQueryCategoryItem[];
}


export interface IDynamicQueryCategoryItem {		
	category : string;
	items : IDynamicQueryDropdownItem[];
}

export interface IDynamicQueryDropdownItem {	
	id : number;
	displayName : string;
}



export interface IAdminSysFavoritSetting {			
	copyMerchantFavoritMerchants: any[];        
	isEnableRDAdminMode : boolean;
	debugSQL_1: string;
}



export interface IBackendButtonResponse {		
	items: IBackendButtonItem[];
}

export interface IBackendButtonItem {		

	backendButtonActionID: number;
	displayName: string ;
	requiredInput: string[] ;
	

}
export interface IBackendButtonActionRequest {		

	backendButtonActionID : number;
	extraParameter : IBackendButtonActionExtraParameterRequest;
}

export interface IBackendButtonActionExtraParameterRequest {		
	merchantID : number;
	begin: string | null;
	end: string | null;
	date: string | null;
	dateTS: number; // timestamp
}

export interface ISelfDefineColumnDisplay {		

	title: string;
	items: ISelfDefineColumnDisplayItem[];
}

export interface ISelfDefineColumnDisplayItem {		
	
	displayName: string;
	value: string;
	format: string;
}

