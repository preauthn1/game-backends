
export interface IExternalApiReqeustLogResponse {

	id : number;
	category : string;
	subCategory: string;
	requestID : string;
	requestUrl : string;
	requestContent : string;
	requestHeader : string;
	requestBody : string;
	responseText: string;
	responseTime: number;
	responseStatusCode: number;
	webSiteName : string; 
	memo : string;
	createTime : string;

}



export interface IOperateRecordReponse {

	Id : number;
	playerID: number;
	playerName: string;
	operateType: number;
	operateTypeText: string;
	merchantID: number;
	vendorID: number| null;
	merchantName: string;
	memo: string;
	updateBy: string;
	createTime: string;
	createIPAddress: string;
}

export interface IOperateRecordRequest {

	operateType : number;
	merchantID: number;
	playerID: number;
	playerName: string;
	createTimeBegin: string | null;
	createTimeEnd: string | null;
	memo: string | null;
	pageIndex: number;
	pageSize: number;
	orderByColumn: string | null;
	isASC: boolean | null;
}


