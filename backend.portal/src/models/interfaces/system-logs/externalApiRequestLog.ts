
export interface IExternalApiRequestLogResponse {	

	id: number;
	category: string;
	subCategory: string;
	requestID: string;
	requestUrl : string;
	requestContent: string;
	requestHeader : string;
	responseText : string;
	responseTime : number;
	responseStatusCode: number;
	webSiteName : string;
	memo : string;
	createTime : string;	
	requestStartTime: string;

	uniqueID: string;


	debugInfo: string;
	debugInfo2: string;
	debugInfo3: string;
	merchantID: string; // 部分情況才有此欄位
	isSearchDetailByAPI: boolean; // 部分情況才有此欄位
	
}

export interface IExternalApiRequestLogDetailInfo {	

	isShowCustomerMessage : boolean;

	merchantID: number;
	playerID : number;
	playerName : string;
	requestUrl : string;
	requestContent: string;
	requestHeader : string;
	responseStatusCode: number;
	responseText : string;
	createTime: string;
	responseTime: number;

}

export interface ISendLogRequest {
	notifyMerchantID: number;
	notifyContent: string;
}



export interface IExternalApiRequestLogPagingRequest {

	id: number;
	requestID: string;
	category: string;
	subCategory: string;
	createTimeBegin: string | null;
	createTimeEnd: string| null;
	dropdownCategory : string | null;
	referenceTransactionNo: string | null;
	targetTableName: string | null;
	topSize : number | null;

	debugInfo : string;
	debugInfoNotKeyword : string;
	debugInfo2: string;
	playerID: string;
	playerName: string;
	merchantID: string;
	IPAddress: string;



	requestContentKeyword: string | null;
	responseTextKeyword: string| null;
	relativeUrlKeyword: string| null;
	requestBodyKeyword: string| null;
	requestUrlKeyword: string| null;
	referOrderNo: string| null;
	searchAllColumnsKeyword: string | null; 
	SearchAllColumnsNotLikeKeyword: string | null;


	// 純粹前端使用
	myInputKeywordFilterType: number;
	myInputKeyword: string;


	pageIndex: number;
	pageSize: number;
	orderByColumn: string | null;
	isASC: boolean | null;
}
