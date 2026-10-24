
export interface IVendorConfig {
	vendorConfigID : number;
	vendorID : number;
	vendorName : string;
	name : string;
	memo : string;
	assignVendorID: string;
	isDefault : boolean;
	createTime : string;
	createBy : string;
	updateTime : string;
	updateBy : string;
	numberOfMerchants: number;
	mappingMerchants: IVendorConfigMappingMerchantItem[];
	supportCurrency: string;
	isTestEnvironment: boolean;
	isSkipFetchSummary: boolean;
}


export interface IVendorConfigMappingMerchantItem
{
	merchantID: number;
	merchantName: string;
}



export interface IVendorConfigPagingRequest 
{
	pageIndex: number;
	pageSize: number;
	orderByColumn: string | null;
	isASC: boolean | null;
}


export interface IVendorConfigBasicInfoUpdateRequest 
{
	vendorConfigID: number;
	name: string;
	supportCurrency: string;
	isTestEnvironment: boolean;
	isSkipFetchSummary: boolean;
}


export interface IVendorConfigCreateRequest 
{
	name: string;
	gameVendorID: number;
	isTestEnvironment: boolean;
	
}


export interface IVendorConfigCopyRequest 
{
	name: string;
	copyFromVendorConfigID: number;
	assignVendorID: string;
	
}


export interface IMerchantVendorConfigSettings {
	merchantID : number;
	merchantType : number;
	merchantTypeText : string;
	merchantName : string;
	mappings : IMerchantVendorConfigSettingItem[];
}



export interface IMerchantVendorConfigSettingItem {
gameVendor : number;
gameVendorName : string;
vendorConfigID : number;
vendorConfigName : string;
dropdownItems : any[];
}



export interface IMerchantVendorConfigSettingUpdate {
	
	merchantID : number;
	gameVendor : number;
	vendorConfigID : number;
}
	