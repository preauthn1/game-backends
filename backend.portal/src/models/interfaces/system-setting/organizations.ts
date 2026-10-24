import * as Model from '@/models/interfaces/commons/response';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import * as CommonEnums from '@/models/status/common';
import * as OrganizationsEnums from '@/models/status/organizations';

export interface ILoginRequest {
	account: string;
	password: [REDACTED];
}

export interface ILoginResponse {
	access_Token: [REDACTED];
}

export interface IProfileResponse {
	userID: number;
	userAccount: string;
	operatorID: number;
	operatorName: string;
	defaultLang: string;
}

export interface IOrganizationsData {
	organizationID: number;
	organizationName: string;
	organizationCode: string;
	memo: string;
	status: CommonEnums.Status;
	lang: CommonEnums.Locale;
	merchantTotalCount: number;
	usersTotalCount: number;
	organizationType: number;
	createBy: string;
	createTime: string;
	updateBy: string;
	updateTime: string;
	roles: number[];
	merchants: number[];
}

export interface IOrganizationsRequest {
	organizationID: number;
	organizationName: string;
	organizationCode: string;
	memo: string;
	lang: CommonEnums.Locale;
	status: CommonEnums.Status;
	organizationType: OrganizationsEnums.OrganizationTypeEnum;
	roles: any;
	rolesDropdown: DropdownModel.IDropdownItems[];
	merchants: any;
	merchantsDropdown: DropdownModel.IDropdownItems[];
}

export interface IOrganizationCustomizedSetting {
	organizationID: number;
	settingKey: number;
	settingValue: string;
}

export type IOrganizationCustomizedSettingUpdateRequest = IOrganizationCustomizedSetting;
