import * as Model from '@/models/interfaces/system-setting/vendor';
import * as SettingModel from '@/models/interfaces/system-setting/systemConfigSetting';
import * as ResponseModel from '@/models/interfaces/commons/response';
import Handler, { pageData } from '@/utilities/api-handler.lib';
import * as Qs from 'qs';

export default {

	async getVendorConfig(filter : Model.IVendorConfigPagingRequest): Promise<ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IVendorConfig>>> {
		
		const config = {
			url: `/vendor-config?${Qs.stringify(filter)}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IVendorConfig>>>result.data;
	},

	async getSingleDetail(vendorConfigID : number): Promise<ResponseModel.IResponseData<Model.IVendorConfig>> {		
		const config = {
			url: `/vendor-config/detail/${vendorConfigID}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IVendorConfig>>result.data;
	},


	async getVendorConfigSettings(vendorConfigID : number): Promise<ResponseModel.IResponseData<SettingModel.ISystemConfigSetting>> 
	{		
		const config = {
			url: `/vendor-config/${vendorConfigID}/settings`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<SettingModel.ISystemConfigSetting>>result.data;
	},


	async syncUnassignMerchants(vendorConfigID : number): Promise<ResponseModel.IResponseData<SettingModel.ISystemConfigSetting>> 
	{		
		const config = {
			url: `/vendor-config/${vendorConfigID}/sync-unassign-merchant`,
			method: 'get',
		};
		
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<SettingModel.ISystemConfigSetting>>result.data;
	},


	async updateVendorConfigSettings(payload : SettingModel.ISystemConfigSettingRequest): Promise<ResponseModel.IResponseData<void>> 
	{		
		const config = {
			url: `/vendor-config/settings`,
			method: 'put',
			data : payload
		};
		
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<void>>result.data;
	},


	async deleteMerchantMapping(targetVendorConfigID : number , targetMerchantID : number): Promise<ResponseModel.IResponseData<void>> 
	{		
		const payload =  
		{
			vendorConfigID : targetVendorConfigID,
			merchantID : targetMerchantID
		};

		const config = {
			url: `/vendor-config/merchants`,
			method: 'delete',
			data : payload
		};
		
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<void>>result.data;
	},



	async createEmptyVendorConfig(payload :Model.IVendorConfigCreateRequest): Promise<ResponseModel.IResponseData<void>> 
	{		
		const config = {
			url: `/vendor-config/empty-create`,
			method: 'post',
			data : payload
		};
		
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<void>>result.data;
	},

	async updateVendorConfigBasicInfo(payload :Model.IVendorConfigBasicInfoUpdateRequest): Promise<ResponseModel.IResponseData<void>> 
	{		
		const config = {
			url: `/vendor-config/basic-info`,
			method: 'put',
			data : payload
		};
		
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<void>>result.data;
	},



	async copyAndCreateNewVendorConfig(payload :Model.IVendorConfigCopyRequest): Promise<ResponseModel.IResponseData<void>> 
	{		
		const config = {
			url: `/vendor-config/copy-create`,
			method: 'post',
			data : payload
		};
		
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<void>>result.data;
	},


	async getMerchantVendorConfigMappings(merchantID : number): Promise<ResponseModel.IResponseData<Model.IMerchantVendorConfigSettings>> 
	{		
		const config = {
			url: `/vendor-config/merchants/${merchantID}/settings`,
			method: 'get',
		};
		
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IMerchantVendorConfigSettings>>result.data;
	},

	async updateMerchantVendorConfigMappings(payload : Model.IMerchantVendorConfigSettingUpdate): Promise<ResponseModel.IResponseData<void>> 
	{		
		const config = {
			url: `/vendor-config/merchants/settings`,
			method: 'put',
			data: payload
		};
		
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<void>>result.data;
	},



};
