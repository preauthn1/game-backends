import * as Model from '@/models/interfaces/system-setting/vendorMaintenance';
import * as ResponseModel from '@/models/interfaces/commons/response';
import Handler, { pageData } from '@/utilities/api-handler.lib';
import * as Qs from 'qs';

export default {

	async getVendorMaintenance(filter : Model.IVendorMaintenancePagingRequest): Promise<ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IVendorMaintenance>>> {
		
		const config = {
			url: `/vendor-maintenance?${Qs.stringify(filter)}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IVendorMaintenance>>>result.data;
	},

	async createVendorMaintenance(payload: Model.ICreateVendorMaintenance): Promise<boolean> {
		const config = {
			url: `/vendor-maintenance`,
			method: 'post',
			data: payload,
		};
		const result = await Handler.request(config);
		return <boolean>result.data;
	},


	async deleteVendorMaintenance(targetID : number): Promise<boolean> {
		const config = {
			url: `/vendor-maintenance/${targetID}`,
			method: 'delete',
		};
		const result = await Handler.request(config);
		return <boolean>result.data;
	},


	async deleteAllVendorMaintenance(): Promise<boolean> {
		const config = {
			url: `/vendor-maintenance/removeAll`,
			method: 'delete',
		};
		const result = await Handler.request(config);
		return <boolean>result.data;
	},

	async updateExcludeTemplateID(targetID : number , myTempalteID : number): Promise<boolean> {
		
		const payload =
		{
			templateID : myTempalteID
		};

		const config = {
			url: `/vendor-maintenance/${targetID}/exclude-merchant-template`,
			method: 'put',
			data: payload
		};

		const result = await Handler.request(config);
		return <boolean>result.data;
	},

	
	async updateIncludeTemplateID(targetID : number , myTempalteID : number): Promise<boolean> {
		
		const payload =
		{
			templateID : myTempalteID
		};

		const config = {
			url: `/vendor-maintenance/${targetID}/include-merchant-template`,
			method: 'put',
			data: payload
		};

		const result = await Handler.request(config);
		return <boolean>result.data;
	}

};
