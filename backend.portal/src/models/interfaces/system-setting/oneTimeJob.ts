import * as CommonEnums from '@/models/status/common';
import * as MerchantsEnums from '@/models/status/merchant';


export interface IQueryOneTimeJobRequest {
	
	status : number;
	pageIndex : number;
	pageSize : number;
	orderByColumn : string;
	isASC : boolean;
}


export interface IOneTimeJobViewModel {

	memo: string;
	
}


export interface IOneTimeJobCreateRequest {
	
	memo: string;
	nextTime: string;
	type: number;
	nextTimeNumber: number | null;	
	settingJson : string;
}
