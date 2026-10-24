export interface ILoginRequest {
	account: string;
	password: [REDACTED];
}

export interface ILoginResponse {
	access_Token: [REDACTED];
	auth_2FA_QRCode: string;
	isResetPasswordWarning: boolean;
	isShowSkipResetButton: boolean;
	isSuperAdminOrg: boolean;
}

export interface IProfileResponse {
	userID: number;
	userAccount: string;
	operatorID: number;
	operatorName: string;
	defaultLang: string;
	timeZone: number;
	timeZoneAreaID: number;
}

export interface IUpdateCurrentUserPassword {
	
	originalPassword: [REDACTED];
	newPassword: [REDACTED];
	confirmPassword: [REDACTED];
}


export interface IUpdateCurrentUserTimezone {
	
	timeZoneID: number;
}



export interface IPermissions {
	nestedPermissions: INestedPermission[];
	allPermissions: IAllPermission[];
	allPermissionsIncludeUserPermission: IAllPermission[];
}

export interface IAllPermission {
	permissionID: number;
	parentPermissionID : number;
	token: [REDACTED];
	pageUrl: string;
}

export interface INestedPermission {
	permissionID: number;
	rootPermissionID: number;
	parentPermissionID: number;
	displayName: string;
	pageUrl: string;
	sort: number;
	permissionType: number;
	token: [REDACTED];
	childPermissions: [];
}




export interface IPermissionCompareRoleItem {
	roleID: number;
	roleName: string;
}

export interface IPermissionComparePermissionItem {
	permissionID: number;
	permissionToken: [REDACTED];
	permissionNevigatePath: string[];
	roleEnableFlag: string[];
	isUnAssignToRoles: boolean;
}

export interface IRolePermissionCompare {
	roles: IPermissionCompareRoleItem[];
	permissions: IPermissionComparePermissionItem[];
}