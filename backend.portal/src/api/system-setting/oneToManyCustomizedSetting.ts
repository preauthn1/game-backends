import * as Model from '@/models/interfaces/system-setting/systemConfigSetting';
import * as ResponseModel from '@/models/interfaces/commons/response';
import Handler, { pageData } from '@/utilities/api-handler.lib';
import * as Qs from 'qs';

export default {
	
	async getByPagingFilter(filter:Model.IQueryOneToManyCustomizedSettingApiRequestParameter):	Promise <ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IOneToManyCustomizedSetting>>> {
		const config = 
		{
			url: `oneToManySettings?${Qs.stringify(filter)}`,
			method: 'get',
		};

		const result = await Handler.request(config);
		return (<ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IOneToManyCustomizedSetting>>>result.data);			
	},

	async create(payload :Model.IQueryOneToManyCustomizedSettingCreateRequestParameter):	Promise <ResponseModel.IResponseData<void>> {
		const config = 
		{
			url: `oneToManySettings/empty-create`,
			method: 'post',
			data: payload
		};

		const result = await Handler.request(config);
		return (<ResponseModel.IResponseData<void>>result.data);			
	},


	async updateDateMemo(payload :Model.IEditDateMemoRequest):	Promise <ResponseModel.IResponseData<void>> {
		const config = 
		{
			url: `oneToManySettings/report-date-memo/edit`,
			method: 'post',
			data: payload
		};

		const result = await Handler.request(config);
		return (<ResponseModel.IResponseData<void>>result.data);			
	},

	async getSettingItems(oneToManyCustomizedSettingID : number):Promise<ResponseModel.IResponseData<Model.IOneToManyCustomizedSettingItem>> {

		const config = 
		{
			url: `oneToManySettings/${oneToManyCustomizedSettingID}/items`,
			method: 'get',
		};

		const result = await Handler.request(config);
		return (<ResponseModel.IResponseData<Model.IOneToManyCustomizedSettingItem>>result.data);			
	},
	

	async updateSettings(payload : Model.ISystemConfigSettingRequest):Promise<void> {

		const config = 
		{
			url: `oneToManySettings/items`,
			method: 'post',
			data : payload,
		};

		const result = await Handler.request(config);
		return (<void>result.data);			
	},
	

	async insertEmptyItem(payload: any):Promise<void> {
		const config = 
		{
			url: `oneToManySettings/insert-new-empty-item`,
			method: 'post',
			data : payload,
		};

		const result = await Handler.request(config);
		return (<void>result.data);			
	},
	

	async createNewIMChannel(payload : any):Promise<void> {

		const config = 
		{
			url: `oneToManySettings/IMChannel`,
			method: 'post',
			data : payload,
		};

		const result = await Handler.request(config);
		return (<void>result.data);			
	},
	
	
};
