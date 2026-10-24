export interface IServer {
	serverID: number | null;
	gameID: number | string;
	serverName: string;
	gameName: string;
	currency: string;
	flagLevel: number | null;
	roomFlagLevel: number | string;
	status: number;
	clientAddr: string;
	notifyAddr: string;
	httpAddr: string;
	createTime: string;
	updateTime: string;
	updateBy: string;
	minAvailableMoney: number | null;
	assignMerchantTotalCount: number;
}

export interface IServerListRequest {
	currency: string | null;
	gameID: number | null;
	pageIndex: number;
	pageSize: number;
	orderByColumn: string | null;
	isASC: boolean | null;
}

export interface IAvailableSettingList {
	availableSettings: IAvailableSetting[];
}

export interface IAllServerMerchantMappings {

	availableCurrency : string[];
	currencyGroups: IAllServerMerchantMappingsCurrencyGroup[];
}

export interface IAllServerMerchantMappingsCurrencyGroup {
	currency: string;
	merchants: IBriefMerchantItemModel[];
	merchantsDropdown: any[];
	servers: IEditServerItem[];
}


export interface IBriefMerchantItemModel {
	
	merchantID: number;
	merchantName: string;
	currency: string;
}

export interface IEditMerchantItem {
	uiCollapseFlag: boolean;
	merchantID: number;
	merchantName: string;
	currency: string;
}

export interface IEditGameItem {
	gameID: number;
	gameName: string;
	servers: IEditServerItem[];
}

export interface IEditServerItem {
	gameID: number;
	gameName: string;
	serverID: number;
	serverName: string;
	currency: string;
	merchantEnableFlags: IEditMappingFlagItem[];
}

export interface IEditMappingFlagItem {
	merchantID: number;
	isEnable: boolean;
	merchantName: string;
	merchantCurrency: string;
}

export interface IReverFlag {
	serverID: number;
	merchantID: number;
}

export interface IAvailableSetting {
	serverID: number;
	merchantID: number;
	merchantName: string;
	isEnable: boolean;
}

export interface IServersHealthSummaryResponse {
	items: IServersHealthSummaryItem[];
	rowTotal: IServersHealthSummaryItem;
}

export interface IServersHealthSummaryItem {
	merchantID: number;
	merchantName: string;
	serverID: number;
	serverName: string;
	gameID: number;
	gameName: string;
	status: number;
	onlinePlayer: number;
	profitAmount: number;
	flowAmount: number;
}

export interface IServerRobotConfigRequest {
	gameID: number;
	pageIndex: number;
	pageSize: number;
	orderByColumn: string | null;
	isASC: boolean | null;
}

export interface IServerRobotConfigReponse {
	serverID: number;
	gameID: number;
	serverName: string;
	minRobotCount: number;
	maxRobotCount: number;
	minLevel: number;
	maxLevel: number;
	isRobotEnable: boolean;
	beginTime: string;
	endTime: string;
	createTime: string;
	createBy: string;
	updateTime: string;
	updateBy: string;
}

export interface IUpdateServerRobotConfigRequest {
	serverID: number | string;
	gameID: number | string;
	minRobotCount: number;
	maxRobotCount: number;
	minLevel: number;
	maxLevel: number;
	isRobotEnable: boolean;
	beginTime: string;
	endTime: string;
}
