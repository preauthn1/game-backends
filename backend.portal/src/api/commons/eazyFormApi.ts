import * as ResponseModel from '@/models/interfaces/commons/response';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import Handler from '@/utilities/api-handler.lib';
import * as Qs from 'qs';
import Cookies from 'js-cookie';
import { Locale } from '@/models/status/common';

export default {

	async getHashKeyByServerTime(): Promise<ResponseModel.IResponseData<any>> {
		
		const config = {
			url: `/common/server-time`,
			method: 'post',
		};

		const result = await Handler.request(config);
		const myObject = <ResponseModel.IResponseData<any>>result.data;
		return myObject;
	},

	async getFormColumns(myFilter: DropdownModel.ILazyCommonParameterReqeust): Promise<any> {
		
		const config = {
			url: `/eazy-form/getFrom?${Qs.stringify(myFilter)}`,
			method: 'get',
		};

		const result = await Handler.request(config);
		const myObject = <ResponseModel.IResponseData<any>>result.data;
		return myObject;
	},

	async saveForm(payload: any): Promise<any> {
		
		const config = {
			url: `/eazy-form/save`,
			method: 'post',
			data: payload
		};

		const result = await Handler.request(config);
		const myObject = <ResponseModel.IResponseData<any>>result.data;
		return myObject;
	}
	
};
