import * as Model from '@/models/interfaces/system-setting/announcement';
import * as SystemSettingModel from '@/models/interfaces/system-setting/systemConfigSetting';

import * as ResponseModel from '@/models/interfaces/commons/response';
import Handler, { pageData } from '@/utilities/api-handler.lib';
import * as Qs from 'qs';

export default {

	async getList(filter : Model.IQueryAnnouncementApiParameter): Promise<ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IAnnouncementListResponse>>> {
			
		const config = {
			url: `/announcement?${Qs.stringify(filter)}`,
			method: 'get',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IAnnouncementListResponse>>>result.data;
	},

	async getDetail(announcementID : number): Promise<ResponseModel.IResponseData<Model.IAnnouncementDetail>> {
			
		const config = {
			url: `/announcement/${announcementID}`,
			method: 'get',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IAnnouncementDetail>>result.data;
	},

	async updateDetail(payload : Model.IAnnouncementDetail): Promise<ResponseModel.IResponseData<Model.IAnnouncementDetail>> {
			
		const config = {
			url: `/announcement`,
			method: 'put',
			data : payload
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IAnnouncementDetail>>result.data;
	},


	async quickCreate(payload : Model.IAnnouncementCreateRequest): Promise<ResponseModel.IResponseData<void>> {
			
		const config = {
			url: `/announcement/quickCreate`,
			method: 'post',
			data : payload
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<void>>result.data;
	},

	async clearAllRead(announcementID : number): Promise<ResponseModel.IResponseData<void>> {
			
		const config = {
			url: `/announcement/${announcementID}/clear-all-read` ,
			method: 'delete',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<void>>result.data;
	},

	async delete(announcementID : number): Promise<ResponseModel.IResponseData<void>> {
			
		const config = {
			url: `/announcement/` + announcementID,
			method: 'delete',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<void>>result.data;
	}

};
