import * as Model from '@/models/interfaces/system-setting/gameList';
import * as ResponseModel from '@/models/interfaces/commons/response';
import Handler, { pageData } from '@/utilities/api-handler.lib';
import * as Qs from 'qs';

export default {
	async getGames(filter: Model.IGameListPagingRequest): Promise<ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IGameList>>> {
		
		filter.pageIndex = 1;
		filter.pageSize = 2000;

		const config = {
			url: `/games?${Qs.stringify(filter)}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IGameList>>>result.data;
	},


	async getGameMerchantInfo(gameID : number): Promise<ResponseModel.IResponseData<Model.IGameMerchantInfo>> {
		
		const config = {
			url: `/games/${gameID}/merchants-info`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IGameMerchantInfo>>result.data;
	},

	
	async createGames(payload: Model.IGameListRequest): Promise<boolean> {
		const config = {
			url: `/games`,
			method: 'post',
			data: payload,
		};
		const result = await Handler.request(config);
		return <boolean>result.data;
	},
	async editGames(payload: Model.IGameListRequest): Promise<boolean> {
		const config = {
			url: `/games`,
			method: 'PUT',
			data: payload,
		};
		const result = await Handler.request(config);
		return <boolean>result.data;
	},

	async getByID(gameID: number): Promise<ResponseModel.IResponseData<Model.IGameList>> {
		const config = {
			url: `/games/${gameID}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IGameList>>result.data;
	},


	async getPayTableImages(filter: Model.IGamePayTableImageRequest): Promise<ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IGamePayTableImageResponse>>> {
		
		filter.pageIndex = 1;
		filter.pageSize = 1000;

		const config = {
			url: `/game/pay-table-images?${Qs.stringify(filter)}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IGamePayTableImageResponse>>>result.data;
	},


	async createOrUpdatePayTableImage(payload: Model.IGamePayTableImageEditRequest): Promise<boolean> {
		
		const config = {
			url: `/game/pay-table-images`,
			method: 'post',			
			data: payload,
		};

		const result = await Handler.request(config);
		return <boolean>result.data;

	},

	async getBatchEditCustomizedSetting(gameID: number): Promise<ResponseModel.IResponseData<Model.IGameBatchEditSettingModel>> {
		
		const config = {
			url: `/games/${gameID}/customized-settings`,
			method: 'get',		
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IGameBatchEditSettingModel>>result.data;
	},


	async updateGameListCustomizedSetting(payload: Model.IGameCustomizedSettingEditItemModel): Promise<ResponseModel.IResponseData<void>> {
		
		const config = {
			url: `/games/customized-settings`,
			method: 'post',	
			data: payload,
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<void>>result.data;
	},

	async deleteGameListCustomizedSetting(payload: Model.IGameCustomizedSettingEditItemModel): Promise<ResponseModel.IResponseData<void>> {
		
		const config = {
			url: `/games/customized-settings`,
			method: 'delete',	
			data: payload,
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<void>>result.data;
	}


};
