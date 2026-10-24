import * as Model from '@/models/interfaces/system-setting/oneTimeJob';
import * as ResponseModel from '@/models/interfaces/commons/response';
import Handler, { pageData } from '@/utilities/api-handler.lib';
import * as Qs from 'qs';

export default {

	async getList(filter : Model.IQueryOneTimeJobRequest): Promise<ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IOneTimeJobViewModel>>> {
			
		const config = {
			url: `/one-time-job?${Qs.stringify(filter)}`,
			method: 'get',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IOneTimeJobViewModel>>>result.data;
	},

	async create(payload : Model.IOneTimeJobCreateRequest): Promise<ResponseModel.IResponseData<void>> {
			
		const config = {
			url: `/one-time-job`,
			data: payload,
			method: 'post',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<void>>result.data;
	},

};
