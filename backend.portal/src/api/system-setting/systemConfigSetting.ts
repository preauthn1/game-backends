import * as Model from '@/models/interfaces/system-setting/systemConfigSetting';
import * as ResponseModel from '@/models/interfaces/commons/response';
import Handler, { pageData } from '@/utilities/api-handler.lib';
import * as Qs from 'qs';

export default {
	async getSystemConfigSettings(): Promise<ResponseModel.IResponseData<Model.ISystemConfigSetting>> {
		const config = {
			url: `/system-config-setting`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.ISystemConfigSetting>>result.data;
	},

	async updateSystemConfigSetting(request: Model.ISystemConfigSettingRequest): Promise<boolean> {
		const config = {
			url: `/system-config-setting`,
			method: 'post',
			data: request,
			isHash: true,
		};
		const result = await Handler.request(config);
		return <boolean>result.data;
	},


	
	async removeAllCache(): Promise<boolean> {
		const config = {
			url: `/internal/cache/removeAll`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <boolean>result.data;
	},
};
