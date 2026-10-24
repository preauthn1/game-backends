import * as Model from '@/models/interfaces/game-management/per-game';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import * as ResponseModel from '@/models/interfaces/commons/response';
import Handler, { pageData } from '@/utilities/api-handler.lib';
import * as Qs from 'qs';

export default {
	async getList(payload: Model.IListRequest): Promise<ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IListResponse>>> {
		const config = {
			url: `/perGame/detail-logs?${Qs.stringify(payload)}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IListResponse>>>result.data;
	},

	async getUnionBetRecords(payload: Model.IUnionBetRecordSearchRequest): Promise<ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IUnionBetRecordResponse>>> {
		const config = {
			url: `/bet-records/union/list?${Qs.stringify(payload)}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IUnionBetRecordResponse>>>result.data;
	},

	async getBetRecordCopyInfo(betRecordNoPGS: string): Promise<ResponseModel.IResponseData<Model.IBetRecordCopyInfoResponse>> {
		const config = {
			url: `/bet-record/copy-info?${Qs.stringify({ betRecordNoPGS })}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IBetRecordCopyInfoResponse>>result.data;
	},

	async getNotifyQueueList(payload: Model.IBetRecordNotifyQueueRequestParameter): Promise<ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IBetRecordNotifyQueuePageQuery>>> {
		const config = {
			url: `/bet-records/notify-queue-list?${Qs.stringify(payload)}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IBetRecordNotifyQueuePageQuery>>>result.data;
	},

	async ResetNotifyQueue(queueID: number): Promise<ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IBetRecordNotifyQueuePageQuery>>> {
		const config = {
			url: `/bet-records/notify-queue-reset`,
			method: 'post',
			data: {
				targetID: queueID
			}
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IBetRecordNotifyQueuePageQuery>>>result.data;
	},

	async triggerNotifyQueue(notifyQueueID: number): Promise<ResponseModel.IResponseData<void>> {
		const config = {
			url: `/bet-records/notify-queue/trigger?notifyQueueID=${notifyQueueID}`,
			method: 'post',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<void>>result.data;
	},

	async getNotifyQueueInfo(queueID: number): Promise<ResponseModel.IResponseData<Model.IBetRecordNotifyQueue>> {
		const config = {
			url: `/bet-records/notify-queue?notifyQueueID=${queueID}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IBetRecordNotifyQueue>>result.data;
	},

	async getBetRecordByTransactionNoForPerGamePage(transactionNo : string): Promise<ResponseModel.IResponseData<Model.IBetRecordInfo>> {
		const config = {
			url: `/perGame/third-detail/${transactionNo}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IBetRecordInfo>>result.data;
	},

	async getBetRecordByTransactionNo(transactionNo : string): Promise<ResponseModel.IResponseData<Model.IBetRecordInfo>> {
		const config = {
			url: `/bet-records/find?TransactionNo=${transactionNo}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IBetRecordInfo>>result.data;
	},

	async exportLargeCsvByVendorConfigID(payload: DropdownModel.ILazyCommonParameterReqeust , fileNamePrefix: string): Promise<void> {
		const config = {
			url: `/perGame/large-csv-export-by-vendorConfigID?${Qs.stringify(payload)}`,
			method: 'get',
		};

		let dateText : any = payload.beginDateTW;
		dateText = dateText.replaceAll('-' , '');
		const result = await Handler.httpGetDownloadCsv(config.url , fileNamePrefix + 'export_' + dateText);
		return result;
	},

	async getSlotHistory(perGameDetailLogID : number): Promise<ResponseModel.IResponseData<Model.ISlotHistoryDetailLogResponse>> {
		const config = {
			url: `/perGame/slot-result/${perGameDetailLogID}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.ISlotHistoryDetailLogResponse>>result.data;
	},

	async getSlotHistoryByTransactionNo(transactionNo : string): Promise<ResponseModel.IResponseData<Model.ISlotHistoryDetailLogResponse>> {
		const config = {
			url: `/perGame/slot-result/transactionNo/${transactionNo}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.ISlotHistoryDetailLogResponse>>result.data;
	},

	// 這支貌似已經沒有code使用的...? 程式碼也查無
	async getSlotHistoryByToken(transactionNo : string , token : [REDACTED] Promise<ResponseModel.IResponseData<Model.ISlotHistoryDetailLogResponse>> {
		const config = {
			url: `/perGame/slot-result/transactionNo?transactionNo=${transactionNo}&token=[REDACTED],
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.ISlotHistoryDetailLogResponse>>result.data;
	},


	async getFullInfo(perGameLogID: number): Promise<ResponseModel.IResponseData<Model.IDetailResponse>> {
		const config = {
			url: `/perGame/${perGameLogID}/full-info`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IDetailResponse>>result.data;
	},
	
	async downloadFile(payload: Model.IListRequest): Promise<void> {
		const config = {
			url: `/export/excel/perGame?${Qs.stringify(payload)}`,
			method: 'get',
		};
		const result = await Handler.downloadExcel<any>(config.url, 'PGS_PerGameLog');
	},
	
	async getExternalBetRecordDetailUrlByTransactionNo(transactionNo: string): Promise<ResponseModel.IResponseData<Model.IExternalUrlResult>> {
		
		const config = {
			url: `/game-vendor/betRecord/by-transaction-no/${transactionNo}/detailInfo`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IExternalUrlResult>>result.data;
	},
	

	async manualRefund(data: Model.IBetRecordManualRefundRequestModel): Promise<ResponseModel.IResponseData<Model.IExternalUrlResult>> {
		
		const config = {
			url: `/bet-records/manual/refund`,
			method: 'post',
			data
		};
				
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IExternalUrlResult>>result.data;
	},

	async manualSettle(data: Model.IManualSettleRequest): Promise<ResponseModel.IResponseData<Model.IExternalUrlResult>> {
		
		const config = {
			url: `/bet-records/manual/settle`,
			method: 'post',
			data
		};
				
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IExternalUrlResult>>result.data;
	},

	async manualChangeSettleInfo(data: Model.IManualChangeSettleInfoRequest): Promise<ResponseModel.IResponseData<Model.IExternalUrlResult>> {
		
		const config = {
			url: `/bet-records/manual/change-settle-amount`,
			method: 'post',
			isHash: true,
			data
		};
				
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IExternalUrlResult>>result.data;
	},

	async manualWritePerGame(postModel: Model.IManualWritePerGameRequest): Promise<ResponseModel.IResponseData<Model.IExternalUrlResult>> {
		
		const config = {
			url: `/bet-records/manual/write-PerGameLog`,
			method: 'post',
			isHash: true,
			data: postModel
		};
				
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IExternalUrlResult>>result.data;
	},


	async recoverNotifyQueueData(betRecordNoPGS: string): Promise<ResponseModel.IResponseData<void>> {
		
		const config = {
			url: `/bet-records/notify-queue-recover/` + betRecordNoPGS,
			method: 'post',
		};
				
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<void>>result.data;
	},


	async getAGGJackpotCurrentAmount(vendorConfigID : number): Promise<ResponseModel.IResponseData<Model.IAGGJackpotAmount>> {
		
		const config = {
			url: `/agg/management/jp?VendorConfigID=` + vendorConfigID,
			method: 'get',
		};
				
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IAGGJackpotAmount>>result.data;
	},


	async addPerGameTag(payload : Model.IPerGameAddTagRequest): Promise<ResponseModel.IResponseData<void>> {
		
		const config = {
			url: `/perGame/tag`,
			method: 'post',
			data: payload
		};
				
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<void>>result.data;
	},	

	async getFishGameDetailRecords(filter:Model.IFishGameDetailRecordQueryRequest):Promise<ResponseModel.IResponseData<Model.IFishGameDetailInfoRecord>> {
		const config = {
			url: `bet-records/fish/detail-records?${Qs.stringify(filter)}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return (<ResponseModel.IResponseData<Model.IFishGameDetailInfoRecord>>result.data);			
	},

	async getBetRecordRawResponseByApi(betRecordNoPGS : string):Promise<ResponseModel.IResponseData<ResponseModel.ISelfDefineColumnDisplay>> {
		const config = {
			url: `bet-records/raw-response/betRecord-detail?TransactionNo=` + betRecordNoPGS,
			method: 'get',
		};
		const result = await Handler.request(config);
		return (<ResponseModel.IResponseData<ResponseModel.ISelfDefineColumnDisplay>>result.data);			
	},

	async findNotSettleRecords(form : Model.IFindNotSettleRecordRequest):Promise<ResponseModel.IResponseData<Model.IFindNotSettleRecordResult>> {
		const config = {
			url: `bet-records/find-notSettle-records?${Qs.stringify(form)}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return (<ResponseModel.IResponseData<Model.IFindNotSettleRecordResult>>result.data);			
	},

	async getDebugSQLAndLog(transactionNo: string):Promise<ResponseModel.IResponseData<any>> {
		const config = {
			url: `records/bet/debug-sql-info?transactionNo=${transactionNo}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return (<ResponseModel.IResponseData<any>>result.data);			
	},

	async batchWritePerGame(form : any):Promise<ResponseModel.IResponseData<any>> {
		const config = {
			url: `bet-records/manual/batch-write-PerGameLog`,
			method: 'post',
			data: form
		};
		const result = await Handler.request(config);
		return (<ResponseModel.IResponseData<any>>result.data);			
	},

	async batchRefund(form : any):Promise<ResponseModel.IResponseData<any>> {
		const config = {
			url: `bet-records/manual/batch-refund`,
			method: 'post',
			data: form
		};
		const result = await Handler.request(config);
		return (<ResponseModel.IResponseData<any>>result.data);			
	},


};
