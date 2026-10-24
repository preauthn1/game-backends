import * as Model from '@/models/interfaces/game-setting/switch';
import * as ResponseModel from '@/models/interfaces/commons/response';
import Handler, { pageData } from '@/utilities/api-handler.lib';
import * as Qs from 'qs';

export default {
	async getGames(merchantID: number): Promise<ResponseModel.IResponseData<Model.IGames>> {
		const config = {
			url: `/merchant/${merchantID}/game-setting`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IGames>>result.data;
	},
	async updateGames(payload: Model.IGameSettingItem): Promise<boolean> {
		const config = {
			url: `/merchant/${payload.merchantID}/game-setting`,
			method: 'put',
			data: payload,
		};
		const result = await Handler.request(config);
		return <boolean>result.data;
	},

	async CopySetting(payload: Model.IGameSettingCopyRequest): Promise<ResponseModel.IResponseData<void>> {
		const config = {
			url: `/merchant/game-setting-copy`,
			method: 'post',
			data: payload,
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<void>>result.data;
	},

	async syncSettingToChilds(merchantID : number): Promise<ResponseModel.IResponseData<void>> {

		const config = {
			url: `/merchant/${merchantID}/game-setting-sync-to-childs`,
			method: 'post',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<void>>result.data;
	},

	async saveMultipleMerchantAdjustGameSetting(payload : Model.IMultipleMerchantAdjustGameSetting): Promise<ResponseModel.IResponseData<ResponseModel.ICRUDResult>> {

		const config = {
			url: `/merchant/multiple-merchant/adjust-game-settings`,
			method: 'post',
			data: payload
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.ICRUDResult>>result.data;
	},


	async getMerchantSelfUpdateEnableGames(merchantID: number): Promise<ResponseModel.IResponseData<Model.IMerchantSelfUpdateSetting>> {
		const config = {
			url: `/merchants/${merchantID}/self-update-settings`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IMerchantSelfUpdateSetting>>result.data;
	},


	async UpdateMerchantSelfUpdateEnableGames(payload: Model.IMerchantSelfUpdateSetting): Promise<ResponseModel.IResponseData<void>> {
		const config = {
			url: `/merchants/self-update-settings/lobby-visible-games`,
			method: 'put',
			data : payload
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<void>>result.data;
	},


};
