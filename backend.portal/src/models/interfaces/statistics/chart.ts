export interface IChartRequest {
	begin: string | null;
	end: string | null;
	timezone: number | null;
	merchantIDs: number[] | null;
	merchantID: number;
	currency: string;
}

export interface IChartReponse {
	chartName: string;
	xAxis: string;
	yAxis: string;
	times: any[];
	chartDatas: IChartData[];
}

export interface IChartData {
	merchantID: number;
	merchantName: string;
	gameID: number;
	gameName: string;
	data: IData[];
}

export interface IData {
	statTime: string;
	dataCount: number;
}


export interface ICommonChartReponse {
	chartName: string;
	xAxis: string;
	yAxis: string;
	times: any[];
	groups : string[];
	labels : string[];
	datasets: IColorGroupChartData[];	
}


export interface IChartConvertRequest {
	isDescOrderByValue: boolean;
	chartDisplayName : string;
	targetName : string;
	items: IChartConvertItem[];
}


export interface IChartConvertItem {
	x_Label: string;
	y_Value: number;
}




export interface IColorGroupChartData {
	groupName: string;
	data: IChartLabelValue[];
}

export interface IChartLabelValue {
	labelName: string;
	value: number;
}

export interface IData {
	statTime: string;
	dataCount: number;
}


export interface IChartDisplayViewModel {
	title : string;
	datasets : IChartDisplayDatasetViewModel[];	
}


export interface IChartDisplayDatasetViewModel {
	group : string;
	data : IChartXYItemDisplayViewModel[];	
}


export interface IChartXYItemDisplayViewModel {
	x : Date;
	y : number;	
}





