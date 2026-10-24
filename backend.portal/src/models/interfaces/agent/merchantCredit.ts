import * as CommonEnums from '@/models/status/common';
import * as MerchantsEnums from '@/models/status/merchant';


export interface IQueryMerchantCreditChangeLogApiRequestParameter {


	createTimeBegin : string | null;
	createTimeEnd : string| null;


	iDs : number[];
	pageIndex : number;
	pageSize : number;
	orderByColumn : string;
	isASC : boolean;
}
	
	
export interface IMerchantCreditChangeLog {
	id : number;
	type : number;
	merchantID : number;
	amount : number;
	beforeAmount : number;
	afterAmount : number;
	memo : string;
	iPAddress : string;
	createTime : string | null;
	createBy : string;
}
	


export interface IQueryMerchantCreditApiRequestParameter {

	keyword: string;
	hierarchyOfTargetMerchant: number | null;	
	merchantID : number | null;
	organizationID : number;
	merchantIDs : number[];
	organizationIDs : number[];
	apiSecretKey : string;
	isIncludeDeleteData : boolean;
	status : number;
	merchantGuid : string;
	currency : string;
	playerID : number;
	walletMode : number;
	type : number;
	code : string;
	IMChannelMappingID: number | null;

	createTimeBegin: string | null;
	createTimeEnd: string| null;

	pageIndex : number;
	pageSize : number;
	orderByColumn : string;
	isASC : boolean;
}
	
	
export interface IMerchantCredit {
	merchantID : number;
	name : string;

	status : number;
	statusText : string;
	currency : string;
	currencyText : string;
	chargeCurrency : string;
	chargeCurrencyText : string;
	walletMode : number;
	walletModeText : string;
	createTime: string;
	creditAmount : number;
	parentMerchantName: string;

	enableGamesCount: number;
	memo2 : string;

	isSingleWallet: boolean;
	isCreditModeEnable : boolean;
	isUnlimit : boolean;

	extraTagsBlackColor: string[];
}
	

export interface IMerchantCreditExportColumns {
	merchantID : number;
	name : string;
	currency : string;
	currencyText : string;
	chargeCurrency : string;
	chargeCurrencyText : string;
	walletMode : number;
	walletModeText : string;
	parentMerchantName: string;

	isProductionEnvType: boolean;
	isSingleWalletDoaminNotSetup: boolean;
	isHintTransferConvertRate: boolean;

	whiteListServerIPs: string[];
	transferConvertRate: number;
	creditAmount : number;
	isCreditModeEnable : boolean;
	isUnlimit : boolean;

	userAccount : string;
	userPassword : [REDACTED];
	backOfficeUrl : string;

	merchantType: number;
	merchantTypeText: string;

	apiUrl : string;
	apiSecretKey : string;
	vendorCode: string;
	singleWalletPgsIdentity: string;
	memo: string;
	memo_ThisIsDevKey: string;
	memo_DevBETACoin: string;
	memo_WhereToDownloadDocument: string;

	isEndpointNotSetup: boolean;
	isSingleWallet: boolean;
}

	
export interface IMerchantCopyByAgentRequest {

	parentMerchantID : number | null;
	name : string;
	account : string;
	password : [REDACTED];
	confirmPassword: [REDACTED];
	roleID : number;
	organizationID : number | null;
	gameIDs: number[];
	walletMode: number | null;
	imChannelID: number | null;

}
	

export interface IMerchantMyCreditResponse {
	isSuperAdmin : boolean;
	isUnlimit : boolean;
	creditAmount : number;
}



export interface IMerchantCreditUpdatePreview {
	myCredit : IMerchantCreditPreviewItem;
	targetMerchantCredit : IMerchantCreditPreviewItem;
}
	
export interface IMerchantCreditPreviewItem {
	merchantID : number;
	merchantName : string;
	isUnlimit : boolean;
	isSuperAdmin : boolean;
	isMultipleHierarchy: boolean;
	creditAmount : number;
}
	

export interface IMerchantCreditUpdateFormRequest {
	
	merchantID : number;
	plusType : number;
	updateAmount : number;
	memo : string;
}

export interface ICreateDevMerchantRequest {
	displayName : string;
	type : number | null;
	IMChannelID : number | null;
}


export interface IMerchantHierarchyTreeviewRequest {
	rootMerchantID : number | null;
	searchHierarchyMemberMerchantID : number| null;
	isShowSpecificTreeOnly : boolean;
}

export interface IMerchantHierarchyTreeview {

	hierarchyMaxLayerLimit : number;
	root : IMerchantHierarchyTreeviewItem;
}

	
export interface IMerchantHierarchyTreeviewItem {
	merchantID : number;
	parentMerchantID : number;
	name : string;
	layer : number;
	childs : IMerchantHierarchyTreeviewItem[];
}
	

export interface IMerchantHierarchyCreateRequest {
	targetMerchantID : number;
	parentMerchantID : number;
}

export interface IMerchantHierarchyMigrateRequest {
	targetMerchantID : number;
	newParentMerchantID : number;
}

export interface IMerchantHierarchyDeleteRequest {
	targetMerchantID : number;

}
