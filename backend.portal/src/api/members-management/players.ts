import * as Model from '@/models/interfaces/members-management/players';
import * as ResponseModel from '@/models/interfaces/commons/response';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import Handler, { pageData } from '@/utilities/api-handler.lib';
import * as Qs from 'qs';

export default {
	async getPlayers(payload: Model.IPlayersRequest): Promise<ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IPlayer>>> {
		const config = {
			url: `/players?${Qs.stringify(payload)}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IPlayer>>>result.data;
	},
	async getPlayer(playerID: number): Promise<ResponseModel.IResponseData<Model.IPlayerDetail>> {
		const config = {
			url: `/players/${playerID}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IPlayerDetail>>result.data;
	},

	async getPlayerDetailByFilter(filter: Model.IPlayerDetailSearchRequest): Promise<ResponseModel.IResponseData<Model.IPlayerDetail>> {
		const config = {
			url: `/players/detail?${Qs.stringify(filter)}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IPlayerDetail>>result.data;
	},

	async playerLock(data: Model.ILockRequest): Promise<boolean> {
		const config = {
			url: `/players/${data.playerID}/status`,
			method: 'put',
			data,
		};
		const result = await Handler.request(config);
		return <boolean>result.data;
	},

	async removeRoomLock(playerID: number): Promise<boolean> {
		const config = {
			url: `/players/${playerID}/room-lock/remove`,
			method: 'delete',
		};
		const result = await Handler.request(config);
		return <boolean>result.data;
	},

	async createInernalTestAccount(model: Model.IInternalTestAccountCreateRequest): Promise<boolean> {

		const config = {
			url: `/players/internal-account/create`,
			method: 'post',
			data: model,
		};
		const result = await Handler.request(config);
		return <boolean>result.data;
	},


	async internalTestAccountDeposit(payload : Model.IInternalTestAccountPlusWalletRequest): Promise<boolean> {

		const config = {
			url: `/players/internal-account/manual-deposit`,
			method: 'post',
			data: payload,
			isHash: true,
		};
		const result = await Handler.request(config);
		return <boolean>result.data;
	},

	async fetchFishWallet(playerID: number):  Promise<ResponseModel.IResponseData<Model.IFetchAGGBalance>> {

		const config = {
			url: `/players/${playerID}/agg-fish-balance`,
			method: 'get',
		};
		
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IFetchAGGBalance>> result.data;
	},

	async playerBatchLock(payload: Model.IBatchLockRequest): Promise<boolean> {
		const config = {
			url: `/players/batch-lock`,
			method: 'put',
			data: payload,
		};
		const result = await Handler.request(config);
		return <boolean>result.data;
	},
	async getPlayerLoginDetail(payload: Model.ILoginDetailRequest): Promise<ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.ILoginDetailResponse>>> {
		const config = {
			url: `/records/player-login?${Qs.stringify(payload)}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.ILoginDetailResponse>>>result.data;
	},
	
	async getLockRecord(payload: Model.ILockRecordRequest): Promise<ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.ILockRecordReponse>>> {
		const config = {
			url: `/records/modify-player-status?${Qs.stringify(payload)}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.ILockRecordReponse>>>result.data;
	},

	async getBetecord(payload: Model.IBetRecordRequest): Promise<ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IBetRecordReponse>>> {
		const config = {
			url: `/records/game-play?${Qs.stringify(payload)}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IBetRecordReponse>>>result.data;
	},
	async getMoneyecord(payload: Model.IMoneyRecordRequest): Promise<ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IMoneyRecordReponse>>> {
		const config = {
			url: `/transaction?${Qs.stringify(payload)}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IMoneyRecordReponse>>>result.data;
	},



	async downloadFile(payload: Model.IMoneyRecordRequest): Promise<void> {
		const config = {
			url: `/export/excel/transaction?${Qs.stringify(payload)}`,
			method: 'get',
		};
		const result = await Handler.downloadExcel<any>(config.url, 'PGS_Transaction');
	},



	
	async addTag(payload: Model.IPlayerTagUpdate): Promise<void> {
		const config = {
			url: `/players/tag`,
			method: 'post',
			data : payload
		};
		const result = await Handler.request<any>(config);
	},

	
	async deleteTag(payload: Model.IPlayerTagUpdate): Promise<void> {

		const config = {
			url: `/players/tag`,
			method: 'delete',
			data : payload
		};
		const result = await Handler.request<any>(config);
	},

	
	async playerSuspiciousCheck(payload: DropdownModel.ILazyCommonParameterReqeust): Promise<ResponseModel.IResponseData<any>> {

		const config = {
			url: `/players/suspicious-check?${Qs.stringify(payload , { skipNulls: true })}`,
			method: 'get',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<any>>result.data;
	},

	async playerLiveCasinoSuspiciousCheck(payload: DropdownModel.ILazyCommonParameterReqeust): Promise<ResponseModel.IResponseData<any>> {

		const config = {
			url: `/players/suspicious-check-live-casino?${Qs.stringify(payload , { skipNulls: true })}`,
			method: 'get',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<any>>result.data;
	},


	async playerW88TransferFailCheck(playerID : number): Promise<ResponseModel.IResponseData<any>> {
		const config = {
			url: `/players/w88-transfer-fail-check?playerID=` + playerID.toString(),
			method: 'get',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<any>>result.data;
	},

};
