import * as Model from '@/models/interfaces/system-setting/merchants';
import * as IMChannel from '@/models/interfaces/system-setting/imChannel';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import * as ResponseModel from '@/models/interfaces/commons/response';
import Handler, { pageData } from '@/utilities/api-handler.lib';
import * as Qs from 'qs';

export default {

	async getList(filter : IMChannel.IMChannelRequestFilter): Promise<ResponseModel.IResponseData<ResponseModel.IPageListReponse<IMChannel.IIMChannelModel>>> {
		
		filter.pageSize = 10000; // 不要再往上調了 不然就改成標準型分頁
		const config = {
			url: `/IMChannel-list?${Qs.stringify(filter, { skipNulls: true })}`,
			method: 'get',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.IPageListReponse<IMChannel.IIMChannelModel>>>result.data;

	},


	async create(model : IMChannel.IMChannelUpdate): Promise<ResponseModel.IResponseData<ResponseModel.ICRUDResult>> {
		
		const config = {
			url: `/IMChannel`,
			data: model,
			method: 'post',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.ICRUDResult>>result.data;
	},


	async update(model : IMChannel.IMChannelUpdate): Promise<ResponseModel.IResponseData<ResponseModel.ICRUDResult>> {
		
		const config = {
			url: `/IMChannel`,
			data: model,
			method: 'put',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.ICRUDResult>>result.data;
	},


	async getIMChannelCustomizedSettings(targetID: number): Promise<ResponseModel.IResponseData<ResponseModel.ICommonCustomizedSettingBatchUpdate >> {
		
		const config = {
			url: `/IMChannel/${targetID}/customized-settings`,
			method: 'get',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.ICommonCustomizedSettingBatchUpdate >>result.data;
	},


	async updateIMChannelCustomizedSettings(payload: ResponseModel.ICommonCustomizedSettingItemEditUpdate): Promise<ResponseModel.IResponseData<void>> {
		
		const config = {
			url: `/IMChannel/customized-settings`,
			data: payload,
			method: 'post',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<void>>result.data;
	},




	async getTGTemplate(filter :  DropdownModel.ILazyCommonParameterReqeust): Promise<ResponseModel.IResponseData<any>> {
		
		const config = {
			url: `/tg-announcement/template-content?${Qs.stringify(filter)}`,
			method: 'get',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<any>>result.data;
	},

	async sendTGNotify(model : any): Promise<ResponseModel.IResponseData<ResponseModel.ICRUDResult>> {
		
		const config = {
			url: `/tg-announcement/template-notify-send`,
			data: model,
			method: 'post',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.ICRUDResult>>result.data;
	},

	async getAnnouncementHistory(model : any): Promise<ResponseModel.IResponseData<any[]>> {
		
		const config = {
			url: `/tg-announcement/send-history?${Qs.stringify(model)}`,
			method: 'get',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<any[]>>result.data;
	},


	async syncTGChatID(): Promise<ResponseModel.IResponseData<any>> {
		
		const config = {
			url: `/tg-announcement/sync-chat-ID`,
			method: 'post',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<any>>result.data;
	},

	async removeSingleMessages(myUUID: string): Promise<ResponseModel.IResponseData<any>> {		
		const payload = 
		{
			uuid: myUUID
		};

		const config = {
			url: `/tg-announcement/remove`,
			data: payload,
			method: 'delete',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<any>>result.data;
	},


	async removeLatestMessages(payload: any): Promise<ResponseModel.IResponseData<any>> {
		
		const config = {
			url: `/tg-announcement/latest-message-remove`,
			data: payload,
			method: 'post',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<any>>result.data;
	}




};
