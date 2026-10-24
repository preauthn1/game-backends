import * as Model from '@/models/interfaces/system-setting/currency';
import * as SystemSettingModel from '@/models/interfaces/system-setting/systemConfigSetting';
import * as ResponseModel from '@/models/interfaces/commons/response';
import Handler, { pageData } from '@/utilities/api-handler.lib';
import * as Qs from 'qs';

export default {

	async getAllCurrencyList(): Promise<ResponseModel.IResponseData<Model.ICurrencyAllListResult>> {
			
		const config = {
			url: `/currency/all-list`,
			method: 'get',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.ICurrencyAllListResult>>result.data;
	},

	async updateSingleCurrency(payload: Model.ICurrencyUpdateRequest): Promise<ResponseModel.IResponseData<ResponseModel.ICRUDResult>> {
			
		const config = {
			url: `/currency`,
			method: 'put',
			data: payload
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.ICRUDResult>>result.data;
	},

	async updateCurrencyRateByThirdAPI(): Promise<ResponseModel.IResponseData<ResponseModel.ICRUDResult>> {
			
		const config = {
			url: `/currency/update-rate-by-api`,
			method: 'post',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.ICRUDResult>>result.data;
	},

};
