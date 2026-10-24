import * as PlayersEnums from '@/models/status/players';

export interface ITransferRecordRequest {
	playerID: number;
	merchantID: number;
	playerName: string;
	orderNo: string | null;
	merchantOrderNo: string | null;
	transferType: PlayersEnums.WalletTransferTypeEnum | null;
	createTimeBegin: string | null;
	createTimeEnd: string | null;
	pageIndex: number;
	pageSize: number;
	orderByColumn: string | null;
	isASC: boolean | null;
}

export interface IGameVendorTransferRecordRequest {
	playerID: number;
	merchantID: number;
	playerName: string;
	orderNo: string | null;
	merchantOrderNo: string | null;
	transferType: PlayersEnums.WalletTransferTypeEnum | null;
	createTimeBegin: string | null;
	createTimeEnd: string | null;
	pageIndex: number;
	pageSize: number;
	orderByColumn: string | null;
	isASC: boolean | null;
}

export interface ITransferRecordReponse {
	orderNo: string;
	merchantOrderNo: string;
	transferType: PlayersEnums.WalletTransferTypeEnum;
	transferTypeText : string;
	convertRate: number;
	playerID: number;
	playerName: string;
	merchantID: number;
	merchantName: string;
	amount: number;
	beforeAmount: number;
	afterAmount: number;
	memo: string;
	createTime: string;
}


export interface IGameVendorTransferRecordReponse {
	orderNo: string;
	merchantOrderNo: string;
	transferType: PlayersEnums.WalletTransferTypeEnum;
	transferTypeText : string;
	playerID: number;
	playerName: string;
	merchantID: number;
	merchantName: string;
	amount: number;
	beforeAmount: number;
	afterAmount: number;
	memo: string;
	createTime: string;
	extraOrderNo: string;
}

export interface ITransferValidCheckReponse {
	isValid: string;
	message: string;
	thirdOrderNo: string;
	platformOrderNo: string;
	transactionNo: string;
	apiCode: number;
}
