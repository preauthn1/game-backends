import * as CommonEnums from '@/models/status/common';
import * as MerchantsEnums from '@/models/status/merchant';



export interface IQueryAccountingInvoiceListParameter {
	
	// status : number;
	// pageIndex : number;
	// pageSize : number;
	// orderByColumn : string;
	// isASC : boolean;

	accountingGroupID: number;
	targetMonth: string;
	isShowSkipItemForDebug: boolean;
	invoiceExportCategory: number;
	extraCateogyID : number;

}

export interface IAccountingInvoiceList {	
	
	items: IAccountingInvoiceListItems[];
}



export interface IAccountingInvoiceListItems {	
	
	// public int MerchantID { get; set; }
	// public string MerchantName { get; set; }
	// public string Currency { get; set; }
	// public decimal ExchangeRateToUSDT { get; set; }
	// public string TagMemo { get; set; }
	// public int AccountingGroupID { get; set; }
	// public string InvoiceGroupCategoryName { get; set; }
	// public decimal ChargeFeeAmount { get; set; }
	// public decimal ChargeFeeAmountAftetExchange { get; set; }

	merchantID: number;
	accountingGroupID: number;
	f2e_isCheckbox: boolean; // 前端專用-非API欄位
}

