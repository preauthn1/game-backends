import * as Model from '@/models/interfaces/system-setting/roles';
import * as ResponseModel from '@/models/interfaces/commons/response';
import Handler, { pageData } from '@/utilities/api-handler.lib';
import * as Qs from 'qs';

export default {
	async getRoles(index: number = 1): Promise<ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IRole>>> {
		if (index) {
			pageData.pageIndex = index;
		}
		const config = {
			url: `/roles?${Qs.stringify(pageData)}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IRole>>>result.data;
	},
	async getRolePermissions(roleID: number): Promise<ResponseModel.IResponseData<Model.IRole>> {
		const config = {
			url: `/roles/${roleID}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IRole>>result.data;
	},
	async createRole(payload: Model.ICreateRoleRequest): Promise<boolean> {
		const config = {
			url: `/roles`,
			method: 'post',
			data: payload,
			isHash : true,
		};
		const result = await Handler.request(config);
		return <boolean>result.data;
	},
	async updateRole(payload: Model.IUpdateRoleRequest): Promise<boolean> {
		const config = {
			url: `/roles`,
			method: 'put',
			data: payload,
			isHash : true,
		};
		const result = await Handler.request(config);
		return <boolean>result.data;
	},
	async deleteRole(roleID: number): Promise<boolean> {
		const config = {
			url: `/roles/${roleID}`,
			method: 'delete',
		};
		const result = await Handler.request(config);
		return <boolean>result.data;
	},
	async updateRolePermissions(payload: Model.IUpdateRolePermissionsRequest): Promise<boolean> {
		const config = {
			url: `/roles/${payload.roleID}/permissions`,
			method: 'put',
			data: payload,
			isHash : true,
		};
		const result = await Handler.request(config);
		return <boolean>result.data;
	},
};
