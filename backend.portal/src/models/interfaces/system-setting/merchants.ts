import * as MerchantsEnums from '@/models/status/merchant';
import * as CommonEnums from '@/models/status/common';

export interface IMerchant {
	merchantID: number;
	name: string;
	memo: string;
	code: string;
	type: number;
	currency: CommonEnums.Currency;
	chargeCurrency : CommonEnums.Currency;
	defaultLanguage: CommonEnums.Locale;
	defaultBalance: number;
	status: CommonEnums.Status;
	apiSecretKey: string;
	apiValidatePlayerSessionUrl: string;
	apiSingleWalletGetAmountUrl: string;
	externalAPI_TestAccountLink_Dev: string;
	externalAPI_TestAccountLink_Pro: string;
	searchExtraSettingValue: string;
	imChannelName: string;
	walletMode: MerchantsEnums.WalletMode | null;
	walletModeText : string;
	createBy: string;
	createTime: string;
	updateBy: string;
	updateTime: string;
	numberOfGames: number;
	numberOfServers: number;
	merchantGuid: string;
}

export interface IMerchantCopyPreviewInfo {

	parentMerchantID: number;
	enableGameVendorGames : any[];
	info : IMerchant;
	isEnableGameEmpty: boolean;
	isEnablePGSGameOnlyButServerEmpty: boolean;
	isCodeNotSetup : boolean;
	isCreditMode : boolean;
	enablePgsGames : number;
	enableThirdPartyGames: number;
	enablePgsGameServers: number;
	pgsConvertRate: number;
	isProvideComplete: boolean;
	adminsSysAccountPassword : [REDACTED];
	singleWalletIdentityToken: [REDACTED];
	singleWalletCallbackEndpointUrl : string;
	gamesMaterialUrl: string;
	imChannelName: string;
	imChannelID: number;
	memo_CN : string[];
	memo_EN : string[];

	credentialMessage_CN: string;
	credentialMessage_EN: string;
}



export interface IMerchantExportColumns {


	isExportMerchantID: boolean;
	isExportMerchantName: boolean;
	isExportWalletMode: boolean;
	isExportApiKey: [REDACTED];
	isExportTestLinkDev: boolean;
	isExportTestLinkPro: boolean;
	isExportGuid: boolean;
	isExportCurrency: boolean;
	isExportIMChannel: boolean;
	isExportExtraSettingValue: boolean;
	isIncludeOtherPage: boolean;

}




export interface IMerchantFilterRequest {

	currency : string;
	walletMode : number;
	type : number;
	pageIndex: number;
	pageSize: number;
	orderByColumn : string;
	searchExtraSettingKey: number;
	dataTableFetchMaxSize: number;

	isSpecialOrder: boolean;
	isASC : boolean;
}

export interface IMerchantRequest {
	merchantID: number;
	name: string;
	memo: string;
	code : string;
	type : number;
	currency: CommonEnums.Currency;
	defaultLanguage: CommonEnums.Locale;
	status: CommonEnums.Status;
	apiSecretKey: string;
	apiValidatePlayerSessionUrl: string;
	apiSingleWalletGetAmountUrl: string;
	walletMode: MerchantsEnums.WalletMode | null;
	extraSettings : any;
	merchantGuid: string;
}


export interface IMerchantCopyRequest {

	sourceMerchantID: number;
	parentMerchantID : number;
	merchantNameList : string[];
	memo: string;
	code: string;
	currency: string;
	isCreateUserAccount : boolean;
	createAccountInfo : IMerchantCopyCreateUserAccountRequest;	
	imChannelID: number;

	// 還在考慮要不要做...
	isEnableAGGLive: boolean;
	isEnableAGG : boolean;
	isLockEditIMChannel: boolean;



	// 前端才有的欄位
	name: string;

	

}


export interface IMerchantCopyCreateUserAccountRequest {

	organizationID: number;
	account: string;
	password: [REDACTED];
	roleID: number;
	isHierarchyMode: boolean;
}



export interface IMerchantAdvanceSetting {
	merchantID: number;
	items: IMerchantAdvanceSettingItems[];
}

export interface IMerchantAdvanceSettingItems {
	customizedSettingKey: number;
	customizedSettingDisplayName: string;
	dataType: string;
	isDisableEdit: boolean;
	isDropdownDataType: boolean;
	dataDropdowns: IMerchantAdvanceSettingDataDropdowns[];
	value: string;
}

export interface IMerchantUpdateAdvanceSettingRequest {
	merchantID: number;
	customizedSettingKey: number;
	value: string;
}

export interface IMerchantAdvanceSettingDataDropdowns {
	text: string;
	value: string;
}

export interface IMerchantEditDefaultBalance {
	merchantID: number;
	name: string;
	defaultBalance: number;
}


export interface ILanguageText {

	languageCode: string;
	totalCount: number;
	operator: string;

}



export interface ILanguageTextPagingRequest {

	languageCode : string;
	languageCodeLikeQuery: string;

	pageIndex: number;
	pageSize: number;
	orderByColumn: string | null;
	isASC: boolean | null;

}


export interface ILanguageTextSingleInfoModel {

	items: ILanguageTextItemModel[];
	insertSQL: string;

}



export interface ILanguageTextItemModel {

	languageCode: string;
	language: string;
	text: string;

}


export interface ILanguageBatchUpdateModel {

	languageCode: string;	
	items: ILanguageTextItemModel[];
}





export interface IMerchantCompareResult {
	
	leftMerchant : IMerchantCompareBasicInfo;
	rightMerchant : IMerchantCompareBasicInfo;
	compareColumns : IMerchantCompareDetailItem[];
	advanceSettings : IMerchantCompareDetailItem[];

}




export interface IMerchantCompareBasicInfo {
	
	merchantID : number;
	name : string;

}


export interface IMerchantCompareDetailItem {

	displayName : string;
	isEqual : boolean;
	isAllowCopy : boolean;
	leftValue : any;
	rightValue : any;

}




export interface IMerchantAdvanceCustomizedSettingListRequest {
	
	merchantIDsText: string;
	merchantType : number | null;
	merchantSettingKeys : number[];
}


export interface IMerchantAdvanceCustomizedSettingListResult {
	
	extraHeaders : IExtraHeader[];
	items : any[];
}


export interface IExtraHeader {
	
	headerDisplayName : string;
	headerPropertyName : string;

}


export interface ICreateMerchantSelectTemplate {	
	templateID : number;
	categoryID : number;
	valueType : number;
	displayName : string;
	includeMerchantIDs : number[];
	parentMerchantID : number;
	excludeMerchantIDs : number[];
	alwaysAddMerchantIDs: number[];
}

export interface IMerchantSelectTemplateResponse {	
	templateID : number;
	categoryID : number;
	valueType : number;
	displayName : string;
	includeMerchantIDs : number[];
	parentMerchantID : number;
	excludeMerchantIDs : number[];
	alwaysAddMerchantIDs: number[];
}

export interface IMerchantStatusHierarchyPreview {	
	
	merchantID : number;
	status : number;
	numberOfChilds : number;
	merchantName : string;

}


export interface IIPAddressTemplateResponse {	
	id: number;
	displayName: string;
	memo: string;
	ipAddressList: string;
	ipAddressWithLinebreak: string;
	ipAddressForDisplay: string;
	assignMerchantCount: number;
	ipAddressCount: number;
	updateTime: string;	
	assignMerchantIDs: number[];
	merchantNameMapping: any;
}


export interface IIPAddressTemplateQueryRequest {		
	keyword : string;
	pageSize: number;
	pageIndex: number;
	isASC: boolean;
}


export interface IIPAddressTemplateCreateRequest {		
	id: number;
	displayName: string;
	memo: string;
	ipAddressList: string;
	ipAddressWithLinebreak: string;
	assignMerchantCount: number;
	ipAddressCount: number;
	updateTime: string;	
	assignMerchantIDs: number[];
}



export interface IIPAddressTemplateSingleAppendRequest {	
	id: number;
	ipAddress: string;

	// 前端顯示用 (非API使用)
	displayName: string;
	ipAddressList: string;
	ipAddressWithLinebreak: string;

}


export interface IIPAddressTemplateUpdateAssignMerchantRequest {		
	
	// 前端顯示用
	displayName: string;
	assignMerchantIDs: number[];
	merchantNameMapping: any;
	ipAddressWithLinebreak: string;
	ipAddressForDisplay: string;

	// 實際上post是下面這幾個欄位
	id: number;
	merchantID: any;
	merchantIDs: number[];
	isDelete: boolean;
}


export interface IBatchUpdateMerchantSettingRequest {		
	
	merchantIDsRawText: string;
	settingKey: number;
	settingValue: any;
}
