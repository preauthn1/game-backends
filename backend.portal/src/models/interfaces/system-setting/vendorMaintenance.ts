import * as MerchantsEnums from '@/models/status/merchant';
import * as CommonEnums from '@/models/status/common';

export interface IVendorMaintenance {
	id : number;
	type : number;
	vendorID : number;
	status : number;
	StartTime : string;
	EndTime : string;
	createTime : string;
	createBy : string;
	updateTime : string;
	updateBy : string;
	isExpired : boolean;
	maintainMessage : string;
	excludeMerchantTemplateName : string;
	excludeMerchantTemplateID : string;

	includeMerchantTemplateName: string;
	includeMerchantTemplateID: string;
}



export interface IVendorMaintenancePagingRequest {
	pageIndex: number;
	pageSize: number;
	orderByColumn: string | null;
	isASC: boolean | null;
}



export interface ICreateVendorMaintenance 
{
	vendorID : number;
	startTime : string;
	endTime : string;
}
