import * as Model from '@/models/interfaces/operator-management/users';
import * as ResponseModel from '@/models/interfaces/commons/response';
import Handler, { pageData } from '@/utilities/api-handler.lib';
import * as Qs from 'qs';

export default {


	async getUsers(filter : Model.IUserFilter): Promise<ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IUser>>> {		
		const config = {
			url: `/users?${Qs.stringify(filter)}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IUser>>>result.data;
	},

	
	async getUsersWithPageSize(filter : Model.IUserFilter): Promise<ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IUser>>> {
		
		const config = {
			url: `/users?${Qs.stringify(filter)}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IUser>>>result.data;
	},


	async getUser(userID: number): Promise<ResponseModel.IResponseData<Model.IUser>> {
		const config = {
			url: `/users/${userID}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IUser>>result.data;
	},

	async requestUserDefaultPassword(userID: number): Promise<ResponseModel.IResponseData<Model.IRequestUserDefaultPasswordResponse>> {
		const myPayload =  
		{
			userID,
			ts : Date.now() / 1000
		};

		const config = {
			url: `/users/requestDefaultPassword`,
			data: myPayload,
			method: 'post',
			isHash: true,
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IRequestUserDefaultPasswordResponse>>result.data;
	},


	async createUsers(payload: Model.IUserRequest): Promise<boolean> {
		const config = {
			url: `/users`,
			method: 'post',
			data: payload,
			isHash: true,
		};
		const result = await Handler.request(config);
		return <boolean>result.data;
	},


	async copyUser(payload: Model.IUserCopyRequest): Promise<boolean> {
		const config = {
			url: `/users/copy-by-userID`,
			method: 'post',
			data: payload,		
			isHash: true,
		};
		const result = await Handler.request(config);
		return <boolean>result.data;
	},
	
	async updateUsers(payload: Model.IUserRequest): Promise<boolean> {
		const config = {
			url: `/users`,
			method: 'put',
			data: payload,			
			isHash: true,
		};
		const result = await Handler.request(config);
		return <boolean>result.data;
	},

	async resetReferenceCreditMerchant(targetUserID : number): Promise<boolean> {
		const config = {
			url: `/users/${targetUserID}/reset-referenceCreditMerchant`,
			method: 'post',
		};
		const result = await Handler.request(config);
		return <boolean>result.data;
	},

	async updateUserBriefInfo(payload: Model.IUpdateUserBriefInfoRequest): Promise<boolean> {
		const config = {
			url: `/users/brief-update`,
			method: 'put',
			data: payload,
		};
		const result = await Handler.request(config);
		return <boolean>result.data;
	},

	async deleteUsers(userID: number): Promise<boolean> {
		const config = {
			url: `/users/${userID}`,
			method: 'delete',
		};
		const result = await Handler.request(config);
		return <boolean>result.data;
	},

	async updateTargetUserPassword(payload: Model.IUpdatePasswordRequest): Promise<ResponseModel.IResponseData<any>> {
		const config = {
			url: `/users/${payload.targetID}/password`,
			method: 'put',
			data: payload,
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<any>>result.data;
	},

	async forceLogoutUser(targetUserID: number): Promise<boolean> {
		const config = {
			url: `/users/${targetUserID}/force-logout`,
			method: 'post',
		};
		const result = await Handler.request(config);
		return <boolean>result.data;
	}

};
