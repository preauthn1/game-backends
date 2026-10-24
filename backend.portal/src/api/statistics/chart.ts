import * as Model from '@/models/interfaces/statistics/chart';
import * as ResponseModel from '@/models/interfaces/commons/response';
import Handler, { pageData } from '@/utilities/api-handler.lib';
import * as Qs from 'qs';
import { ChartDataTypeEnum } from '@/models/status/statistics';

export default {
	async getChart(chartType: ChartDataTypeEnum, payload: Model.IChartRequest): Promise<ResponseModel.IResponseData<Model.IChartReponse>> {
		const config = {
			url: `/report/stat/chartdata/${chartType}?${Qs.stringify(payload)}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IChartReponse>>result.data;
	},

	async getChartV2(chartType: ChartDataTypeEnum, payload: Model.IChartRequest): Promise<ResponseModel.IResponseData<Model.ICommonChartReponse>> {
		const config = {
			url: `/report/stat/chartdata/v2/${chartType}?${Qs.stringify(payload)}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.ICommonChartReponse>>result.data;
	},

	async convertToBarChartObject(convertRequest: Model.IChartConvertRequest): Promise<ResponseModel.IResponseData<Model.ICommonChartReponse>> {
		const config = {
			url: `/report/chart-convert/bar`,
			method: 'post',
			data : convertRequest
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.ICommonChartReponse>>result.data;
	},

	async convertToPieChartObject(convertRequest: Model.IChartConvertRequest): Promise<ResponseModel.IResponseData<Model.ICommonChartReponse>> {
		const config = {
			url: `/report/chart-convert/pie`,
			method: 'post',
			data : convertRequest
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.ICommonChartReponse>>result.data;
	},

	async getChartTestttt(chartType: ChartDataTypeEnum, payload: Model.IChartRequest): Promise<ResponseModel.IResponseData<Model.ICommonChartReponse>> {
		const config = {
			url: `/report/stat/chartdata-testttt`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.ICommonChartReponse>>result.data;
	},


};
