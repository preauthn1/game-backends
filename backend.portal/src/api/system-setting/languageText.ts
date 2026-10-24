import * as Model from '@/models/interfaces/system-setting/merchants';
import * as ResponseModel from '@/models/interfaces/commons/response';
import Handler, { pageData } from '@/utilities/api-handler.lib';
import * as Qs from 'qs';

export default {

	async getLanguageCodes(filter: Model.ILanguageTextPagingRequest): Promise<ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.ILanguageText>>> {
	
		const config = {
			url: `/language-text?${Qs.stringify(filter)}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.ILanguageText>>>result.data;
	},
	

	async getAllTextByLanguageCode(languageCode: string): Promise<ResponseModel.IResponseData<Model.ILanguageTextSingleInfoModel>> {
		
		const config = {
			url: `/language-text/${languageCode}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.ILanguageTextSingleInfoModel>>result.data;
	},
	

	async batchUpdate(payload: Model.ILanguageBatchUpdateModel): Promise<ResponseModel.IResponseData<Model.ILanguageTextItemModel[]>> {
		
		const config = {
			url: `/language-text/batch-update`,
			method: 'put',
			data: payload,
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.ILanguageTextItemModel[]>>result.data;
	},
	
	async createNotExistGameNameLanguageText(): Promise<ResponseModel.IResponseData<string>> {
		
		const config = {
			url: `/language-text/initialize/game-text`,
			method: 'post',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<string>>result.data;
	},
	

};
