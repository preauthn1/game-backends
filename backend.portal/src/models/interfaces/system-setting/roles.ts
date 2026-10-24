export interface IRole {
	roleID: number;
	roleName: string;
	status: number;
	createBy: string;
	createTime: string;
	updateBy: string;
	updateTime: string;
	sort: number;
	memo: string;
	permissions: number[];
}

export interface ICreateRoleRequest {
	roleName: string;
	sort: number;
	memo: string;
}

export interface IUpdateRoleRequest {
	roleID: number;
	roleName: string;
	sort: number;
	memo: string;
}

export interface IUpdateRolePermissionsRequest {
	roleID: number;
	permissions: number[];
}

export interface IMenuRootObject {
	id: number;
	name: string;
	children: IChild[];
}

export interface IChild {
	id: number;
	name: string;
}
