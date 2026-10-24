import * as Model from '@/models/interfaces/system-setting/merchants';
import * as CreditModel from '@/models/interfaces/agent/merchantCredit';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import * as SystemSettingModel from '@/models/interfaces/system-setting/systemConfigSetting';

import * as ResponseModel from '@/models/interfaces/commons/response';
import Handler, { pageData } from '@/utilities/api-handler.lib';
import * as Qs from 'qs';

export default {
	async getMerchants(filter : Model.IMerchantFilterRequest): Promise<ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IMerchant>>> {
		
		filter.pageSize = 50000; // 不要再往上調了 不然就改成標準型分頁
		const config = {
			url: `/merchants?${Qs.stringify(filter, { skipNulls: true })}`,
			method: 'get',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IMerchant>>>result.data;
	},

	async createMerchant(payload: Model.IMerchantRequest): Promise<boolean> {
		const config = {
			url: `/merchants`,
			method: 'post',
			data: payload,
		};
		const result = await Handler.request(config);
		return <boolean>result.data;
	},

	async getMerchant(merchantID: number): Promise<ResponseModel.IResponseData<Model.IMerchant>> {
		const config = {
			url: `/merchants/${merchantID}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IMerchant>>result.data;
	},

	async copyAndCreateNewMerchant(payload: Model.IMerchantCopyRequest): Promise<ResponseModel.IResponseData<ResponseModel.ICRUDResult>> {

		payload.merchantNameList =  [payload.name ];
		const config = {
			url: `/merchants/batch-copy`,
			method: 'post',
			data: payload,
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.ICRUDResult>>result.data;
	},

	async copyCreateChildAgent(payload: CreditModel.IMerchantCopyByAgentRequest): Promise<boolean> {

		const config = {
			url: `/merchants/copy-merchant-by-agent`,
			method: 'post',
			data: payload,
			isHash: true,
		};
		const result = await Handler.request(config);
		return <boolean>result.data;
	},

	async getMerchantCopyPreviewInfo(merchantID: number): Promise<ResponseModel.IResponseData<Model.IMerchantCopyPreviewInfo>> {
		const config = {
			url: `/merchants/copy-info/${merchantID}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IMerchantCopyPreviewInfo>>result.data;
	},


	async updateMerchant(payload: Model.IMerchantRequest): Promise<boolean> {
		const config = {
			url: `/merchants`,
			method: 'put',
			data: payload,
		};
		const result = await Handler.request(config);
		return <boolean>result.data;
	},

	async deleteMerchant(merchantID: number , myMemo : string = ''): Promise<any> {
		const payload = {
			memo : myMemo
		};		
		const config = {
			url: `/merchants/${merchantID}`,
			method: 'delete',
			data : payload
		};
		const result = await Handler.request(config);
		return <any>result.data;
	},


	async updateMerchantStatusByAgent(payload: DropdownModel.ILazyCommonParameterReqeust): Promise<boolean> {
		const config = {
			url: `/merchants/edit-merchant-status-by-agent`,
			method: 'put',
			data: payload,
		};
		const result = await Handler.request(config);
		return <boolean>result.data;
	},


	async getCreateAvailableSettings(): Promise<ResponseModel.IResponseData<SystemSettingModel.ISystemConfigSettingItem[]>> {

		const config = {
			url: `merchants/create-available-settings`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<SystemSettingModel.ISystemConfigSettingItem[]>>result.data;
	},

	async getMerchantAdvanceSetting(merchantID: number , specificKey : number | null = null): Promise<ResponseModel.IResponseData<SystemSettingModel.ISystemConfigSetting>> {

		let specificKeyParam = '';

		if(specificKey != null)
		{
			specificKeyParam = "?SpecificKey=" + specificKey.toString();
		}

		const config = {
			url: `v2/merchants/${merchantID}/advance-setting` + specificKeyParam,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<SystemSettingModel.ISystemConfigSetting>>result.data;
	},



	async getMerchantAdvanceSettingBySettingKeys(merchantID: number , specificKeys : number[]): Promise<ResponseModel.IResponseData<SystemSettingModel.ISystemConfigSetting>> {

		const params = {
			settingKeys : specificKeys
		};				
		const config = {
			url: `v2/merchants/${merchantID}/advance-setting` + `?${Qs.stringify(params)}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<SystemSettingModel.ISystemConfigSetting>>result.data;
	},


	async getStatusAndHierarchyPreviewInfo(merchantID: number): Promise<ResponseModel.IResponseData<Model.IMerchantStatusHierarchyPreview>> {		

		const config = {
			url: `/merchants/${merchantID}/merchant-status-and-hierarchy-preview`,
			method: 'get',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IMerchantStatusHierarchyPreview>>result.data;
	},

	async updateMerchantAdvanceSetting(request: SystemSettingModel.ISystemConfigSettingRequest): Promise<boolean> {
		const config = {
			url: `v2/merchants/advance-setting`,
			method: 'post',
			data: request,
		};
		const result = await Handler.request(config);
		return <boolean>result.data;
	},

	async getMerchantCompareInfo(merchantID: number , merchantID2 : number): Promise<ResponseModel.IResponseData<Model.IMerchantCompareResult>> {

		const payload = 
		{
			LeftMerchantID: merchantID,
			RightMerchantID: merchantID2,
		};
	
		const config = {
			url: `merchants/compare`,
			method: 'post',
			data: payload,
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IMerchantCompareResult>>result.data;
	},

	async parseMerchantIDsByRawText(myRawText: string ): Promise<ResponseModel.IResponseData<DropdownModel.ILazyCommonResponseViewModel>> {

		const payload = 
		{
			rawText: myRawText,
		};
	
		const config = {
			url: `merchants/parse-merchantIDs-by-rawText`,
			method: 'post',
			data: payload,
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<DropdownModel.ILazyCommonResponseViewModel>>result.data;
	},


	async getMerchantCustomizedSettingList(payload: Model.IMerchantAdvanceCustomizedSettingListRequest): Promise<ResponseModel.IResponseData<Model.IMerchantAdvanceCustomizedSettingListResult>> {
	
		const tempArray = payload.merchantSettingKeys;
		let filterKeys = '';

		payload.merchantSettingKeys.forEach((item)=>
		{
			filterKeys = filterKeys + "merchantSettingKeys=" +  item.toString() + "&";
		});

		payload.merchantSettingKeys = [];

		let q = `${Qs.stringify(payload)}`;
		
		if(q === '') {
			q = '?' + filterKeys;
		}
		else {
			q =  '?' + q + '&' + filterKeys;
		}		

		payload.merchantSettingKeys = tempArray;

		const config = {
			url: `merchants/list-all-settings`+  q,
			method: 'get',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IMerchantAdvanceCustomizedSettingListResult>>result.data;
	},
		

	async getMerchantSelectTemplateByID(templateID: number): Promise<ResponseModel.IResponseData<Model.IMerchantSelectTemplateResponse>> {

		const config = {
			url: `merchant-select-template/` + templateID.toString(),
			method: 'get',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IMerchantSelectTemplateResponse>>result.data;
	},


	async createMerchantSelectTemplate(payload: Model.ICreateMerchantSelectTemplate): Promise<ResponseModel.IResponseData<ResponseModel.ICRUDResult>> {

		const config = {
			url: `merchant-select-template`,
			method: 'post',
			data: payload,
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.ICRUDResult>>result.data;
	},


	async updateMerchantSelectTemplate(payload: Model.ICreateMerchantSelectTemplate): Promise<ResponseModel.IResponseData<ResponseModel.ICRUDResult>> {

		const config = {
			url: `merchant-select-template`,
			method: 'put',
			data: payload,
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.ICRUDResult>>result.data;
	},



	async deleteMerchantSelectTemplate(templateID : number): Promise<ResponseModel.IResponseData<ResponseModel.ICRUDResult>> {

		const config = {
			url: `merchant-select-template/` + templateID.toString(),
			method: 'delete',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.ICRUDResult>>result.data;
	},


	async getIPAddressTemplates(filter : Model.IIPAddressTemplateQueryRequest): Promise<ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IIPAddressTemplateResponse>>> {
		
		const config = {
			url: `/ip-templates?${Qs.stringify(filter)}`,
			method: 'get',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IIPAddressTemplateResponse>>>result.data;
	},



	async getIPAddressTemplatesById(id : number): Promise<ResponseModel.IResponseData<Model.IIPAddressTemplateResponse>> {
		
		const config = {
			url: `/ip-templates/${id}`,
			method: 'get',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IIPAddressTemplateResponse>>result.data;
	},

	async getEnableGamesForSubAgent(merchantID : number): Promise<ResponseModel.IResponseData<any>> {
		
		const config = {
			url: `/merchant/${merchantID}/enable-games-for-subAgent`,
			method: 'get',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<any>>result.data;
	},

	async getGameEnableSettingsForAgentSelfEdit(merchantID : number): Promise<ResponseModel.IResponseData<any>> {
		
		const config = {
			url: `/merchant/${merchantID}/game-setting-edit-by-agent`,
			method: 'get',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<any>>result.data;
	},

	async saveEnableGamesByAgentSelfEdit(payload : any): Promise<ResponseModel.IResponseData<void>> {
		
		const config = {
			url: `/merchant/game-setting-edit-by-agent/batch-update`,
			method: 'post',
			data: payload
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<void>>result.data;
	},


	async createIPAddressTemplates(payload : Model.IIPAddressTemplateCreateRequest): Promise<ResponseModel.IResponseData<void>> {
		
		const config = {
			url: `/ip-templates`,
			method: 'post',
			data: payload,
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<void>>result.data;
	},

	async updateIPAddressTemplates(payload : Model.IIPAddressTemplateCreateRequest): Promise<ResponseModel.IResponseData<void>> {
		
		const config = {
			url: `/ip-templates`,
			method: 'put',
			data: payload,
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<void>>result.data;
	},

	async appendSingleIPAddressToSetting(payload : Model.IIPAddressTemplateSingleAppendRequest): Promise<ResponseModel.IResponseData<void>> {
		
		const config = {
			url: `/ip-templates/append-single-ip`,
			method: 'post',
			data: payload,
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<void>>result.data;
	},

	async deleteIPTemplate(targetID: number): Promise<ResponseModel.IResponseData<void>> {
		
		const config = {
			url: `/ip-templates/` + targetID,
			method: 'delete',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<void>>result.data;
	},


	async updateIPAddressTemplatesAssignMerchant(payload : Model.IIPAddressTemplateUpdateAssignMerchantRequest): Promise<ResponseModel.IResponseData<void>> {
		
		const config = {
			url: `/ip-templates/assign-merchant`,
			method: 'post',
			data: payload,
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<void>>result.data;
	},


	async doExtraOperateForMerchant(myMerchantID : number , myType: number): Promise<ResponseModel.IResponseData<void>> {
		
		const payload = {
			merchantID: myMerchantID,
			type :myType
		};
		const config = {
			url: `/merchants/extra-operate-by-type`,
			method: 'post',
			data: payload,
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<void>>result.data;
	},

	async batchUpdateCustomizedSetting(payload:  Model.IBatchUpdateMerchantSettingRequest): Promise<ResponseModel.IResponseData<any>> {
		
		const config = {
			url: `/v2/merchants/advance-setting/batch-update`,
			method: 'post',
			data: payload,
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<any>>result.data;
	}


};
