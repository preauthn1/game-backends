import * as Model from '@/models/interfaces/commons/permission';
import * as ResponseModel from '@/models/interfaces/commons/response';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import Handler from '@/utilities/api-handler.lib';

export default {
	async login(payload: Model.ILoginRequest): Promise<ResponseModel.IResponseData<Model.ILoginResponse>> {
		const config = {
			url: `/users/login`,
			method: 'post',
			data: payload,
		};
		const result = await Handler.request(config, true);
		return <ResponseModel.IResponseData<Model.ILoginResponse>>result.data;
	},

	async bind2FA(payload: any , tempAccessToken: [REDACTED] = ''): Promise<ResponseModel.IResponseData<Model.ILoginResponse>> {
		const config = {
			url: `/users/bind-2FA`,
			method: 'post',
			data: payload,
		};
		const result = await Handler.request(config, false , tempAccessToken);
		return <ResponseModel.IResponseData<Model.ILoginResponse>>result.data;
	},


	async getProfile(): Promise<ResponseModel.IResponseData<Model.IProfileResponse>> {
		const config = {
			url: `/users/profile`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IProfileResponse>>result.data;
	},

	async updatePassword(payload : Model.IUpdateCurrentUserPassword): Promise<ResponseModel.IResponseData<void>> {
		const config = {
			url: `/users/password`,
			method: 'put',
			data : payload
		};		
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<void>>result.data;
	},

	async updateTimezone(payload : Model.IUpdateCurrentUserTimezone): Promise<ResponseModel.IResponseData<void>> {
		const config = {
			url: `/users/timeZone`,
			method: 'put',
			data : payload
		};		
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<void>>result.data;
	},



	async getUsers(): Promise<ResponseModel.IResponseData<Model.IProfileResponse>> {
		const config = {
			url: `/users`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IProfileResponse>>result.data;
	},
	async getRoles(): Promise<ResponseModel.IResponseData<Model.IProfileResponse>> {
		const config = {
			url: `/roles`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IProfileResponse>>result.data;
	},	
	async getPermissionsNested(): Promise<ResponseModel.IResponseData<Model.IPermissions>> {
		const config = {
			url: `/permissions/nested`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IPermissions>>result.data;
	},

	async getMyPermissions(): Promise<ResponseModel.IResponseData<Model.IPermissions>> {
		const config = {
			url: `/users/permissions`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IPermissions>>result.data;
	},

	async getComparePermissionsResult(payload : DropdownModel.ILazyCommonParameterReqeust): Promise<ResponseModel.IResponseData<Model.IRolePermissionCompare>> {
		const config = {
			url: `/permissions/compare`,
			method: 'post',
			data : payload
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IRolePermissionCompare>>result.data;
	},




};
