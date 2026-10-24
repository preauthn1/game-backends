import * as CommonEnums from '@/models/status/common';
import * as MerchantsEnums from '@/models/status/merchant';
import * as Dropdown from '@/models/interfaces/commons/dropdown';

export interface ISystemConfigSetting {

	ownerID : number;
	groups: ISystemConfigSettingGroup[];
}

export interface ISystemConfigSettingGroup {
	groupID: number;
	groupName: string;
	isAllDisableEdit: boolean;
	items: ISystemConfigSettingItem[];
}

export interface ISystemConfigSettingItem {
	settingKey: number;
	settingKeyDisplayName: string;
	settingKeyText: string;
	dataType: string;
	isDisableEdit: boolean;
	isDropdownDataType: boolean;
	dataDropdowns: Dropdown.IDropdownItems[];
	value: string;
	debug1: string;
	memo: string;
}

export interface ISystemConfigSettingRequest {
	merchantID : number;
	ownerID : number;
	items: ISystemConfigSettingUpdateRequestItem[];
}

export interface ISystemConfigSettingUpdateRequestItem {
	settingKeyText : string;
	settingKey: number;
	value: string;
}


export interface IQueryOneToManyCustomizedSettingApiRequestParameter {	
	pageIndex : number;
	pageSize : number;
	orderByColumn : string;
	isASC : boolean;
}

	
export interface IOneToManyCustomizedSetting {
	oneToManyCustomizedSettingID : number;
	type : number;
	typeName : number;
	ownerID : number;
	ownerIDMemo : string;
	displayName : string;
	memo : string;
	createTime : string | null;
}


export interface IOneToManyCustomizedSettingItem {

	ownerID : number;
	groups : ISystemConfigSettingGroup[];

}


export interface IQueryOneToManyCustomizedSettingCreateRequestParameter {	

	type : number;
	displayName : string;
	memo : string;
	defaultSubCategory: string;
	ownerID: string;
	ownerIDMemo: string;
}

export interface IEditDateMemoRequest {
	dateText : string;
	memo : string;
	vendorConfigID : number;
}
