import * as Model from '@/models/interfaces/members-management/records';
import * as ResponseModel from '@/models/interfaces/commons/response';
import Handler, { pageData } from '@/utilities/api-handler.lib';
import * as Qs from 'qs';

export default {

	async getExternalApiRequestLog(requestUUID : string): Promise<ResponseModel.IResponseData<Model.IExternalApiReqeustLogResponse>> {
		const config = {
			url: `/records/ex-api-logs/RequestID/${requestUUID}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IExternalApiReqeustLogResponse>>result.data;
	},

	async getExternalApiRequestLogByID(id : number): Promise<ResponseModel.IResponseData<Model.IExternalApiReqeustLogResponse>> {
		const config = {
			url: `/records/ex-api-logs/id/${id}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IExternalApiReqeustLogResponse>>result.data;
	},

};
