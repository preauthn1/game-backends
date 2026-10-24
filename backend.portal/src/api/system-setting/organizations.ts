import * as Model from '@/models/interfaces/system-setting/organizations';
import * as ResponseModel from '@/models/interfaces/commons/response';
import Handler, { pageData } from '@/utilities/api-handler.lib';
import * as Qs from 'qs';

export default {
	async getOrganizations(index: number = 1): Promise<ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IOrganizationsData>>> {
		if (index) {
			pageData.pageIndex = index;
		}
		const config = {
			url: `/organizations?${Qs.stringify(pageData)}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IOrganizationsData>>>result.data;
	},
	async getOrganization(orgID: number): Promise<ResponseModel.IResponseData<Model.IOrganizationsData>> {
		const config = {
			url: `/organizations/${orgID}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IOrganizationsData>>result.data;
	},
	async createOrganizations(payload: Model.IOrganizationsRequest): Promise<boolean> {
		const config = {
			url: `/organizations`,
			method: 'post',
			data: payload,
		};
		const result = await Handler.request(config);
		return <boolean>result.data;
	},
	async updateOrganizations(payload: Model.IOrganizationsRequest): Promise<boolean> {
		const config = {
			url: `/organizations`,
			method: 'put',
			data: payload,
		};
		const result = await Handler.request(config);
		return <boolean>result.data;
	},
	async getCustomizedSettings(organizationID: number): Promise<ResponseModel.IResponseData<Model.IOrganizationCustomizedSetting[]>> {
		const config = {
			url: `/organizations/customized-settings?${Qs.stringify({ organizationID })}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IOrganizationCustomizedSetting[]>>result.data;
	},
	async updateCustomizedSetting(payload: Model.IOrganizationCustomizedSettingUpdateRequest): Promise<ResponseModel.IResponseData<void>> {
		const config = {
			url: `/organizations/customized-settings`,
			method: 'post',
			data: payload,
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<void>>result.data;
	},
};
