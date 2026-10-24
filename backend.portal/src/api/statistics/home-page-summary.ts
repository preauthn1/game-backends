import * as Model from '@/models/interfaces/statistics/home-page-summary';
import * as ResponseModel from '@/models/interfaces/commons/response';
import Handler, { pageData } from '@/utilities/api-handler.lib';
import * as Qs from 'qs';
export default {
	async getHomePageSummary(payload: any): Promise<ResponseModel.IResponseData<Model.IReponse[]>> {
		const config = {
			url: `/report/home-page-summary?${Qs.stringify(payload)}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IReponse[]>>result.data;
	},
	
	
	async getHomePageTodayInfo(payload: any): Promise<ResponseModel.IResponseData<Model.IHomePageTodayInfo>> {
		const config = {
			url: `/report/home-page-summary/v2/today-info?${Qs.stringify(payload)}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IHomePageTodayInfo>>result.data;
	},

	async getHomePageFinanceOverview(payload: any): Promise<ResponseModel.IResponseData<Model.IFinanceOverviewReport[]>> {
		const config = {
			url: `/report/each-currency-report?${Qs.stringify(payload)}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IFinanceOverviewReport[]>>result.data;
	},
};
