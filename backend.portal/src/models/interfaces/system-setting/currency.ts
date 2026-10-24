import * as CommonEnums from '@/models/status/common';
import * as MerchantsEnums from '@/models/status/merchant';



export interface ICurrencyAllListResult {	
	items: ICurrencyItem[];
}

export interface ICurrencyItem {		

	currency: string;
	exchangeRateToTWD: number;
	exchangeRateToUSDT: number;

	lastUpdateTime: string;
	lastUpdateTimeDiffText: string;	


	// 前端運算用
	exchangeRate_EditNewValue: number;
	displayWarningText: string;
	isRenderDiv: boolean;

}


export interface ICurrencyUpdateRequest{		

	currency: string;
	exchangeCurrency: string;
	exchangeRate: number;


}

