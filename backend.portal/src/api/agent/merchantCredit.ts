import * as Model from '@/models/interfaces/agent/merchantCredit';
import * as SystemSettingModel from '@/models/interfaces/system-setting/systemConfigSetting';
import * as ResponseModel from '@/models/interfaces/commons/response';
import Handler, { pageData } from '@/utilities/api-handler.lib';
import * as Qs from 'qs';

export default {

	async getMerchantCredit(filter : Model.IQueryMerchantCreditApiRequestParameter): Promise<ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IMerchantCredit>>> {
			
		const config = {
			url: `/merchant-credit?${Qs.stringify(filter)}`,
			method: 'get',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IMerchantCredit>>>result.data;
	},


	async getMyCredit(): Promise<ResponseModel.IResponseData<Model.IMerchantMyCreditResponse>> {
					
		const config = {
			url: `/merchant-credit/myself`,
			method: 'get',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IMerchantMyCreditResponse>>result.data;
	},


	async getMerchantCreditPreview(merchantID : number): Promise<ResponseModel.IResponseData<Model.IMerchantCreditUpdatePreview>> {
			
		const config = {
			url: `/merchant-credit/${merchantID}/preview`,
			method: 'get',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IMerchantCreditUpdatePreview>>result.data;
	},


	async getMerchantCreditExportColumns(filter : Model.IQueryMerchantCreditApiRequestParameter): Promise<ResponseModel.IResponseData<Model.IMerchantCreditExportColumns[]>> {
			
		const config = {
			url: `/merchant-credit/export-columns?${Qs.stringify(filter)}`,
			method: 'get',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IMerchantCreditExportColumns[]>>result.data;
	},


	async updateMerchantCredit(payload : Model.IMerchantCreditUpdateFormRequest): Promise<ResponseModel.IResponseData<void>> {
			
		const config = {
			url: `/merchant-credit`,
			method: 'put',
			data : payload
		};		
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<void>>result.data;
	},

	async createDevMerchant(payload : Model.ICreateDevMerchantRequest): Promise<ResponseModel.IResponseData<void>> {
		const config = {
			url: `/merchants/dev-merchant-create`,
			method: 'post',
			data : payload
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<void>>result.data;
	},

	async getMerchantHierarchy(filter : Model.IMerchantHierarchyTreeviewRequest): Promise<ResponseModel.IResponseData<Model.IMerchantHierarchyTreeview>> {
			
		const config = {
			url: `/merchant-hierarchy/tree-view?${Qs.stringify(filter)}`,
			method: 'get',
		};		
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IMerchantHierarchyTreeview>>result.data;
	},


	async createMerchantHierarchy(payload : Model.IMerchantHierarchyCreateRequest): Promise<ResponseModel.IResponseData<void>> {
			
		const config = {
			url: `/merchant-hierarchy`,
			method: 'post',
			data : payload
		};		
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<void>>result.data;
	},


	async deleteMerchantHierarchy(payload : Model.IMerchantHierarchyDeleteRequest): Promise<ResponseModel.IResponseData<void>> {
			
		const config = {
			url: `/merchant-hierarchy`,
			method: 'delete',
			data : payload
		};		
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<void>>result.data;
	},

	async addRootAgent(merchantID : number): Promise<ResponseModel.IResponseData<void>> {
			
		const payload = 
		{
			MerchantID : merchantID
		};

		const config = {
			url: `/root-merchant-hierarchy/create`,
			method: 'post',
			data : payload
		};		

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<void>>result.data;
	},

	async migrateHierarchy(payload : Model.IMerchantHierarchyMigrateRequest): Promise<ResponseModel.IResponseData<void>> {
			
		const config = {
			url: `/merchant-hierarchy/migrate`,
			method: 'post',
			data : payload
		};		

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<void>>result.data;
	},

	async checkMerchantTreeError(): Promise<ResponseModel.IResponseData<void>> {
			
		const config = {
			url: `/merchant-hierarchy/tree-error-check`,
			method: 'post',
		};		

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<void>>result.data;
	},


	async getChangeLogs(filter : Model.IQueryMerchantCreditChangeLogApiRequestParameter): Promise<ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IMerchantCreditChangeLog>>> {
			
		const config = {
			url: `/records/merchant-credit-change-logs?${Qs.stringify(filter)}`,
			method: 'get',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IMerchantCreditChangeLog>>>result.data;
	},
	
	
};
