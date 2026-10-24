import * as Model from '@/models/interfaces/commons/permission';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import * as TagModel from '@/models/interfaces/game-management/generalTag';
import * as ResponseModel from '@/models/interfaces/commons/response';
import Handler from '@/utilities/api-handler.lib';
import * as Qs from 'qs';

export default {

	async getPagingList(filter : TagModel.IGeneralTagRequest): Promise<ResponseModel.IResponseData<ResponseModel.IPageListReponse<TagModel.IGeneralTagResponse>>> {
				
		const config = {
			url: `/tags?${Qs.stringify(filter)}`,
			method: 'get',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.IPageListReponse<TagModel.IGeneralTagResponse>>>result.data;
	},	

	async getByID(tagID : number): Promise<ResponseModel.IResponseData<TagModel.IGeneralTagResponse>> {
				
		const config = {
			url: `/tags/`+ tagID.toString(),
			method: 'get',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<TagModel.IGeneralTagResponse>>result.data;
	},	
	

	async createTag(payload : TagModel.IGeneralTagCreateRequest): Promise<ResponseModel.IResponseData<void>> {
				
		const config = {
			url: `/tags`,
			method: 'post',
			data: payload
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<void>>result.data;
	},
	
	async updateAssigner(payload : TagModel.IGeneralTagAssignerUpdateRequest): Promise<ResponseModel.IResponseData<void>> {
				
		const config = {
			url: `/tags/assigner`,
			method: 'put',
			data: payload
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<void>>result.data;
	},	

	async appendTagToAssigner(payload : any): Promise<ResponseModel.IResponseData<void>> {
				
		const config = {
			url: `/tags/append-tags-to-assigner`,
			method: 'post',
			data: payload
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<void>>result.data;
	}	


};
