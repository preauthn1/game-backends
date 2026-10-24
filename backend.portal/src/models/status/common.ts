/**
 * @description 語系
 */
export enum Locale {
	ZHTW = 'zh-tw',
	ZHCN = 'zh-cn',
	ENUS = 'en-us',
}

export enum CRUDAction {
	Create = 1,
	Update = 2,
	Delete = 3,
}

export enum BackendButton {	

	RemoveRedisCache = 1222,

}


export enum Currency {
	CNY = 'CNY',
	TWD = 'TWD',
	USD = 'USD',
	MYR = 'MYR',
	AUD = 'AUD',
	VND = 'VND',
	SGD = 'SGD',
	INR = 'INR',
	IDR = 'IDR',
	CAD = 'CAD',
	CZK = 'CZK',
	DKK = 'DKK',
	EUR = 'EUR',
	GBP = 'GBP',
	HKD = 'HKD',
	JPY = 'JPY',
	KRW = 'KRW',
	MXN = 'MXN',
	MMK = 'MMK',
	NOK = 'NOK',
	NZD = 'NZD',
	PHP = 'PHP',
	PLZ = 'PLZ',
	SEK = 'SEK',
	THB = 'THB',

	BETA = 'BETA',
	MSC_POINT = 'MSC_POINT',
}

export enum GameType {
	None = 0,
	Slot = 1000,
}

export enum GameVendorType {
	None = 0,
	Slot88 = 2001,
	AGG = 290000,
}

export enum EventType {
	
	RTPEvent = 51,
}

export enum Status {
	/**关闭中*/
	Disable = 0,
	/**启用*/
	Enable = 1,
	/**永久隐藏*/
	PermanentDelete = 999,
}

// 建議參考後端的DataCategoryEnum
export enum ExtraFilterTypeDataCategory{
	
	Player = 8800,
}


export enum MenuIcon {
	Home = 'mdi-garage',
	MembersManagement = 'mdi-account-box-multiple',
	GameManagement = 'mdi-controller-classic',
	GameSetting = 'mdi-dice-multiple-outline',
	Statistics = 'mdi-database-settings',
	EventManagement = 'mdi-database-settings',
	Agent_Management = 'mdi-cash-100',
	OperatorManagement = 'mdi-account-card-details',
	SystemSetting = 'mdi-settings-box',
	Merchant_SelfUpdateSetting = 'mdi-settings',
}


