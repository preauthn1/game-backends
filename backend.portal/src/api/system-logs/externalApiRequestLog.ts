import * as Model from '@/models/interfaces/system-logs/externalApiRequestLog';
import * as RecordModel from '@/models/interfaces/members-management/records';
import * as ResponseModel from '@/models/interfaces/commons/response';
import Handler, { pageData } from '@/utilities/api-handler.lib';
import * as Qs from 'qs';

export default {

	async getLogs(filter : Model.IExternalApiRequestLogPagingRequest): Promise<ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IExternalApiRequestLogResponse>>> {
		
		const config = {
			url: `/records/ex-api-logs?${Qs.stringify(filter)}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IExternalApiRequestLogResponse>>>result.data;
	},

	async getThirdLogByUniqueID(uniqueID: string): Promise<ResponseModel.IResponseData<Model.IExternalApiRequestLogResponse>> {
		
		const config = {
			url: `/records/ex-api-logs/uniqueID/` + uniqueID,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IExternalApiRequestLogResponse>>result.data;
	},

	async sendLog(model: Model.ISendLogRequest): Promise<ResponseModel.IResponseData<ResponseModel.ICRUDResult>> {
		const config = {
			url: `/tg-announcement/send-log`,
			data: model,
			method: 'post',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.ICRUDResult>>result.data;
	},


	async getSystemLogApiRequestLogs(filter : Model.IExternalApiRequestLogPagingRequest): Promise<ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IExternalApiRequestLogResponse>>> {
		
		const config = {
			url: `/records/sysyem-logs-api-request-logs?${Qs.stringify(filter)}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IExternalApiRequestLogResponse>>>result.data;
	},

	async getSystemLogApiRequestLogsByUniqueID(uniqueID: string , tableName: string| null): Promise<ResponseModel.IResponseData<Model.IExternalApiRequestLogResponse>> {
		
		const queryOjb = 
		{
			UniqueID: uniqueID,
			TableName: tableName,
		};

		const config = {
			url: `/records/sysyem-logs-api-request-logs/unique?${Qs.stringify(queryOjb)}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IExternalApiRequestLogResponse>>result.data;
	},



	async getSingleWalletPlatformRequestLogs(filter : Model.IExternalApiRequestLogPagingRequest): Promise<ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IExternalApiRequestLogResponse>>> {
		
		const config = {
			url: `/records/sw-platform-request-logs?${Qs.stringify(filter)}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IExternalApiRequestLogResponse>>>result.data;
	},

	async getUniqueSingleWalletPlatformRequestLogs(uniqueID: string , createTimeText: string): Promise<ResponseModel.IResponseData<Model.IExternalApiRequestLogResponse>> {
		
		const config = {
			url: `/records/sw-platform-request-logs/unique?uniqueID=` + uniqueID + '&createTimeBegin=' + createTimeText,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IExternalApiRequestLogResponse>>result.data;
	},

	async getUserOperateRecords(filter : RecordModel.IOperateRecordRequest): Promise<ResponseModel.IResponseData<ResponseModel.IPageListReponse<RecordModel.IOperateRecordReponse>>> {
		const config = {
			url: `/records/operate-records?${Qs.stringify(filter)}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.IPageListReponse<RecordModel.IOperateRecordReponse>>>result.data;
	}

};
