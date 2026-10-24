import * as Model from '@/models/interfaces/statistics/merchant-summary';
import * as GameReportModel from '@/models/interfaces/statistics/game';
import * as ResponseModel from '@/models/interfaces/commons/response';
import Handler, { pageData } from '@/utilities/api-handler.lib';
import axios, { AxiosResponse, AxiosRequestConfig } from 'axios';
import * as Qs from 'qs';

export default {

	async getMerchantSummaryList(payload: Model.IMerchantSummarySearchRequest): Promise<ResponseModel.IResponseData<Model.IReponse>> {
		const config = {
			url: `/report/stat/merchant-summary?${Qs.stringify(payload)}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IReponse>>result.data;
	},

	async getMerchantDailyReport(payload: Model.IMerchantDailySearchRequest): Promise<ResponseModel.IResponseData<Model.IMerchantDailyReport>> {
		const config = {
			url: `/report/stat/merchant-daily-summary`,
			method: 'post',	
			data: payload
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IMerchantDailyReport>>result.data;
	},

	async getAgentHierarchyDailyReport(payload: Model.IAgentHierarchyDailyReportRequest): Promise<ResponseModel.IResponseData<Model.IMerchantDailyReport>> {
		const config = {
			url: `/report/stat/hierarchy-daily-summary?${Qs.stringify(payload)}`,
			method: 'get',		
		};
		
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IMerchantDailyReport>>result.data;
	},
	
	async getAgentHierarchyDailyGameReport(payload: Model.IMerchantDailyGameReportRequest): Promise<ResponseModel.IResponseData<Model.IMerchantDailyGameReportResult>> {
		
		const config = {
			url: `/report/stat/agent-daily-game-summary`,
			method: 'post',		
			data: payload
		};
		
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IMerchantDailyGameReportResult>>result.data;
	},
	
	async getAgentHierartchyTotalSummaryReport(payload: Model.IQueryHierarchyAgentTotalReportReportApiRequestParameter): Promise<ResponseModel.IResponseData<Model.IQueryAgentSummaryReportByHierarchyChildMappingResult>> {
		const config = {
			url: `/report/stat/agent-hierarchy-total-summary/totalsum`,
			method: 'post',
			data: payload
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IQueryAgentSummaryReportByHierarchyChildMappingResult>>result.data;
	},
	
	async getAgentHierartchyTotalSummaryReportDetail(payload: Model.IQueryHierarchyAgentTotalReportReportApiRequestParameter): Promise<ResponseModel.IResponseData<Model.IQueryAgentSummaryReportByHierarchyChildMappingResult>> {
		
		const config = {
			url: `/report/stat/agent-hierarchy-total-summary/detail-info`,
			method: 'post',
			data: payload
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IQueryAgentSummaryReportByHierarchyChildMappingResult>>result.data;
	},


	async getAgentSpecialReport(payload: any): Promise<ResponseModel.IResponseData<Model.IAgentSpecialReport>> {
		const config = {
			url: `/report/stat/agent-special-report?${Qs.stringify(payload)}`,
			method: 'get',		
		};
		
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IAgentSpecialReport>>result.data;
	},



	async getChildAgentSingleDateSummaryReport(payload: Model.IChildAgentSingleDateSummaryRequest): Promise<ResponseModel.IResponseData<Model.IMerchantDailyReport>> {
	
		const config = {
			url: `/report/stat/child-agent-single-date-summary?${Qs.stringify(payload)}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IMerchantDailyReport>>result.data;
	},

	async getMerchantAllPlayerWinLossReport(payload: Model.IMerchantAllPlayerWinLossReportRequest): Promise<ResponseModel.IResponseData<Model.IMerchantAllPlayerWinLossReportResponse>> {
		const config = {
			url: `/report/stat/merchant-all-player-winLoss?${Qs.stringify(payload , { skipNulls: true })}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IMerchantAllPlayerWinLossReportResponse>>result.data;
	},
	
	async getFreespinReport(payload: Model.IFreespinReportRequestModel): Promise<ResponseModel.IResponseData<Model.IFreespinReportResponseModel>> {
		const config = {
			url: `/report/stat/freespin-report?${Qs.stringify(payload)}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IFreespinReportResponseModel>>result.data;
	},

	async getFreespinStatusList(payload: any): Promise<ResponseModel.IResponseData<ResponseModel.IPageListReponse<any>>> {
		const config = {
			url: `/report/stat/freespin-status-list?${Qs.stringify(payload)}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.IPageListReponse<any>>>result.data;
	},

	async getPlayerWinLossRankingReport(payload: Model.IPlayerWinLossRankingReportRequest): Promise<ResponseModel.IResponseData<Model.IPlayerWinLossRankingSearchResult>> {
		const config = {
			url: `/report/stat/player-winLoss-ranking-report?${Qs.stringify(payload)}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IPlayerWinLossRankingSearchResult>>result.data;
	},

	async downloadFile(payload: Model.IMerchantDailySearchRequest): Promise<void> {
		const config = {
			url: `/export/excel/dailyReport?${Qs.stringify(payload)}`,
			method: 'get',
		};

		await Handler.downloadExcel(config.url, 'PGS_DailyReport');
	},


	async getRetentionReport(payload: Model.IRetentionReportRequest): Promise<ResponseModel.IResponseData<Model.IRetentionReportResponse>> {
		const config = {
			url: `/report/stat/retention-rate?${Qs.stringify(payload)}`,
			method: 'get',
		};		
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IRetentionReportResponse>>result.data;
	},


	async getGameReportList(payload: Model.IGameReportRequest): Promise<ResponseModel.IResponseData<GameReportModel.IGameListReponse>> {
		const config = {
			url: `/report/stat/game?${Qs.stringify(payload)}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<GameReportModel.IGameListReponse>>result.data;
	},


	async getPlayerGameSummaryReportList(payload: GameReportModel.IPlayerGameSummaryRequest): Promise<ResponseModel.IResponseData<GameReportModel.IPlayerGameSummary>> {
		const config = {
			url: `/report/stat/player-game-summary?${Qs.stringify(payload)}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<GameReportModel.IPlayerGameSummary>>result.data;
	},


	async getFinanceCompareDateMemo(payload: Model.IQueryVendorConfigFinanceCompareRequest): Promise<ResponseModel.IResponseData<Model.IExportFinanceReportDateMemo>> {
		const config = {
			url: `/report/stat/export-all-finance-compare-memo?${Qs.stringify(payload)}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IExportFinanceReportDateMemo>>result.data;
	},



	async getVendorConfigFinanceCompareStatReport(filter:Model.IQueryVendorConfigFinanceCompareRequest):
	Promise	<ResponseModel.IResponseData<Model.IVendorConfigFinanceCompareResponse>> {
		const config = {
		url: `report/stat/finance-compare-report?${Qs.stringify(filter)}`,
		method: 'get',
		};
		const result = await Handler.request(config);
		return (<ResponseModel.IResponseData<Model.IVendorConfigFinanceCompareResponse>>result.data);			
	},

	async triggerFetchThirdAccounting(myPayload :any):	Promise	<ResponseModel.IResponseData<any>> {
		const config = {
			url: `report/fetch-third-accounting-again`,
			method: 'post',
			data: myPayload
		};
		const result = await Handler.request(config);
		return (<ResponseModel.IResponseData<any>>result.data);			
	},

};
