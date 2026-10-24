import * as Model from '@/models/interfaces/system-setting/accountingInvoice';
import * as SystemSettingModel from '@/models/interfaces/system-setting/systemConfigSetting';
import * as ResponseModel from '@/models/interfaces/commons/response';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import Handler, { pageData } from '@/utilities/api-handler.lib';
import * as Qs from 'qs';

export default {

	async getExportInvoiceList(filter : any): Promise<ResponseModel.IResponseData<Model.IAccountingInvoiceList>> {
			
		const config = {
			url: `/accounting-export/merchant-invoice-list?${Qs.stringify(filter)}`,
			method: 'get',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IAccountingInvoiceList>>result.data;
	},

	async exportMonthlyAccountingTotalExcelTextForCopy(filter : any): Promise<string> {
			
		const config = {
			url: `/accounting-export/export-excel-raw-text?${Qs.stringify(filter)}`,
			method: 'get',
		};

		const result = await Handler.request(config);
		return <string>result.data;
	},
	
	
	async getAdjustmentRulesAndAssignGroup(): Promise<ResponseModel.IResponseData<any>> {
			
		const config = {
			url: `/accounting/rules-and-assigner`,
			method: 'get',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<any>>result.data;
	},

	async copyAndCreateNewRule(payload : DropdownModel.ILazyCommonParameterReqeust): Promise<ResponseModel.IResponseData<void>> {
			
		const config = {
			url: `/accounting/rules/copy-and-create`,
			method: 'post',
			data: payload
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<void>>result.data;
	},

	async getVworldInvoiceEachMerchantPromotionDetailWithAccordionFormat(filter : any): Promise<ResponseModel.IResponseData<any>> {
			
		const config = {
			url: `/accounting-export/export-excel-vwolrd-version-promotion-detail/accordion-format?${Qs.stringify(filter)}`,
			method: 'get',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<any>>result.data;
	},

};
