import * as CommonEnums from '@/models/status/common';

export interface IUser {
	userID: number;
	userName: string;
	displayName: string;
	organizationID: number;
	organizationName: string;
	roleID: any;
	roleName_CN: string;
	roleName_EN: string;
	imChannelName: string;
	memo : string;
	merchants: number[];
	status: CommonEnums.Status;
	createTime: string;
	lastLogonTime: string;
	lastLogonIPAddress?: string;
	timeZoneAreaID: number;
	referenceCreditMerchant: number | null;
	referenceCreditMerchantName: string;
	isDefaultPassword: [REDACTED];
	assignMerchantMode: number;
}

export interface IRequestUserDefaultPasswordResponse {
	userPassword: [REDACTED];
}

export interface IUserFilter {

	organizationID: number;
	referenceCreditMerchantID: number | null;
	pageIndex: number;
	pageSize: number;

}


export interface IUserRequest {
	userID: number;
	userName: string;
	displayName: string;
	memo: string;
	organizationID: number | null;
	roleID: any;
	password: [REDACTED];
	confirmPassword: [REDACTED];
	merchants: any[];
	status: CommonEnums.Status;
	timeZoneAreaID: number;
	referenceCreditMerchant: number | null;
	isOperateFromAgentPage: boolean;
	assignMerchantMode: number;
}


export interface IUserCopyRequest {
	sourceUserID: number;
	userName: string;
	organizationCode: string; // 非request參數純粹前端使用
}


export interface IUpdatePasswordRequest {

	targetUserName : string; // 這是前端暫存使用的(後端無此欄位)
	targetID : number; // 這是前端暫存使用的(後端無此欄位)
	imChannelName: string;// 這是前端暫存使用的(後端無此欄位)

	originalPassword : [REDACTED];
	newPassword : [REDACTED];
	confirmPassword : [REDACTED];	
	isResetDefaultPasswordPolicy: boolean;
	isValidateIMChannel: boolean;
}


export interface IUpdateUserBriefInfoRequest {

	targetUserName : string; // 這是前端暫存使用的(後端無此欄位)

	userID : number;
	memo : string;
	status : number;

}
