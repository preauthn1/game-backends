import * as Model from '@/models/interfaces/members-management/transfer';
import * as ResponseModel from '@/models/interfaces/commons/response';
import Handler, { pageData } from '@/utilities/api-handler.lib';
import * as Qs from 'qs';

export default {
	async getTransferRecord(payload: Model.ITransferRecordRequest): Promise<ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.ITransferRecordReponse>>> {
		const config = {
			url: `/transfer?${Qs.stringify(payload)}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.ITransferRecordReponse>>>result.data;
	},

	async getGameVendorTransferRecord(payload: Model.IGameVendorTransferRecordRequest): Promise<ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IGameVendorTransferRecordReponse>>> {
		const config = {
			url: `/gameVendor/transfer?${Qs.stringify(payload)}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IGameVendorTransferRecordReponse>>>result.data;
	},

	async checkTransferIsValidByThirdApi(transactionNo: string): Promise<ResponseModel.IResponseData<Model.ITransferValidCheckReponse>> {
		const config = {
			url: `/gameVendor/transfer/third-api-check?transactionNo=` + transactionNo,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.ITransferValidCheckReponse>>result.data;
	},

	async downloadFile(payload: Model.ITransferRecordRequest): Promise<void> {
		const config = {
			url: `/export/excel/transfer?${Qs.stringify(payload)}`,
			method: 'get',
		};
		const result = await Handler.downloadExcel<any>(config.url, 'PGS_Transfer');
	},
};
