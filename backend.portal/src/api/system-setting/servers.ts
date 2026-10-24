import * as Model from '@/models/interfaces/system-setting/servers';
import * as ResponseModel from '@/models/interfaces/commons/response';
import Handler, { pageData } from '@/utilities/api-handler.lib';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import * as Qs from 'qs';

export default {
	async getServers(payload: Model.IServerListRequest): Promise<ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IServer>>> {
		const config = {
			url: `/servers?${Qs.stringify(payload)}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IServer>>>result.data;
	},

	async createServer(payload: Model.IServer): Promise<boolean> {
		const config = {
			url: `/servers`,
			method: 'post',
			data: payload,
		};
		const result = await Handler.request(config);
		return <boolean>result.data;
	},
	async addServer(payload: Model.IServer): Promise<boolean> {
		const config = {
			url: `/servers`,
			method: 'post',
			data: payload,
		};
		const result = await Handler.request(config);
		return <boolean>result.data;
	},
	async updateServer(payload: Model.IServer): Promise<boolean> {
		const config = {
			url: `/servers`,
			method: 'put',
			data: payload,
		};
		const result = await Handler.request(config);
		return <boolean>result.data;
	},
	async deleteServer(serverID: number): Promise<boolean> {
		const config = {
			url: `/servers/${serverID}`,
			method: 'delete',
		};
		const result = await Handler.request(config);
		return <boolean>result.data;
	},
	async getAvailableMerchants(serverID: number): Promise<ResponseModel.IResponseData<Model.IAvailableSettingList>> {
		const config = {
			url: `/servers/${serverID}/availableMerchants`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IAvailableSettingList>>result.data;
	},

	async getAllServerMerchantMappings(payload: DropdownModel.ILazyCommonParameterReqeust): Promise<ResponseModel.IResponseData<Model.IAllServerMerchantMappings>> {
		const config = {
			url: `servers/all-merchant-mapping`,
			method: 'post',
			data : payload
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IAllServerMerchantMappings>>result.data;
	},

	async reverServerMerchantFlag(payload: Model.IReverFlag): Promise<boolean> {
		const config = {
			url: `/servers/merchant-mapping/revers`,
			method: 'put',
			data: payload,
		};
		const result = await Handler.request(config);

		const res: any = result.data;
		return <boolean>res.data.afterValue;
	},

	async updateAvailableMerchants(payload: Model.IAvailableSetting): Promise<boolean> {
		const config = {
			url: `/servers/${payload.serverID}/availableMerchants`,
			method: 'put',
			data: payload,
		};
		const result = await Handler.request(config);
		return <boolean>result.data;
	},

	async getServersHealthSummary(merchantID: number | null): Promise<ResponseModel.IResponseData<Model.IServersHealthSummaryResponse>> {
		const config = {
			url: `/servers/health/summary?${Qs.stringify({ merchantID })}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IServersHealthSummaryResponse>>result.data;
	},
	async getServersRobotConfig(payload: Model.IServerRobotConfigRequest): Promise<ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IServerRobotConfigReponse>>> {
		const config = {
			url: `/server/robot-config?${Qs.stringify(payload)}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IServerRobotConfigReponse>>>result.data;
	},
	async addServersRobotConfig(payload: Model.IUpdateServerRobotConfigRequest): Promise<boolean> {
		const config = {
			url: `/server/robot-config`,
			method: 'post',
			data: payload,
		};
		const result = await Handler.request(config);
		return <boolean>result.data;
	},
	async getServersRobotConfigDetail(serverID: number): Promise<ResponseModel.IResponseData<Model.IServerRobotConfigReponse>> {
		const config = {
			url: `/server/robot-config/${serverID}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IServerRobotConfigReponse>>result.data;
	},

	async updateServersRobotConfig(payload: Model.IUpdateServerRobotConfigRequest): Promise<boolean> {
		const config = {
			url: `/server/robot-config`,
			method: 'put',
			data: payload,
		};
		const result = await Handler.request(config);
		return <boolean>result.data;
	},
	async deleteServersRobotConfig(targetID: number): Promise<boolean> {
		const config = {
			url: `/server/robot-config/${targetID}`,
			method: 'delete',
		};
		const result = await Handler.request(config);
		return <boolean>result.data;
	},
};
