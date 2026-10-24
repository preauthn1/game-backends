import * as Model from '@/models/interfaces/operator-management/BOAnnouncement';
import * as ResponseModel from '@/models/interfaces/commons/response';
import Handler, { pageData } from '@/utilities/api-handler.lib';
import * as Qs from 'qs';

export default {


	async getList(filter : Model.IBOAnnouncementFilter): Promise<ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IBOAnnouncement>>> {		
		const config = {
			url: `/adminSys/announcement?${Qs.stringify(filter)}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IBOAnnouncement>>>result.data;
	},


	async getById(id: number): Promise<ResponseModel.IResponseData<Model.IBOAnnouncement>> {
		const config = {
			url: `/adminSys/announcement/${id}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IBOAnnouncement>>result.data;
	},

	async create(payload: Model.IBOAnnouncementRequest): Promise<boolean> {
		const config = {
			url: `/adminSys/announcement`,
			method: 'post',
			data: payload,
		};
		const result = await Handler.request(config);
		return <boolean>result.data;
	},
	async update(payload: Model.IBOAnnouncementRequest): Promise<boolean> {
		const config = {
			url: `/adminSys/announcement`,
			method: 'put',
			data: payload,
		};
		const result = await Handler.request(config);
		return <boolean>result.data;
	},
	
	
	async delete(id: number): Promise<boolean> {
		const config = {
			url: `/adminSys/announcement/${id}`,
			method: 'delete',
		};
		const result = await Handler.request(config);
		return <boolean>result.data;
	},


	async getVisibleAnnouncement(): Promise<ResponseModel.IResponseData<Model.IHomePageAnnouncement>> {		
		const config = {
			url: `/adminSys/visible-announcement?`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IHomePageAnnouncement>>result.data;
	},


	async SetAnnouncementRead(id: number): Promise<ResponseModel.IResponseData<void>> {		
		const config = {
			url: `/adminSys/bo-announcement/` + id.toString() + `/read`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<void>>result.data;
	},

};
