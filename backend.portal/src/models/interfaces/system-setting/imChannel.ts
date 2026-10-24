import * as MerchantsEnums from '@/models/status/merchant';
import * as CommonEnums from '@/models/status/common';

export interface IIMChannelModel {
	merchantID: number;
	name: string;
	memo: string;
	memo2: string;
	code: string;	
	imChannelID: number;
	type: string;
	channelName: string;
	internalMemo: string;
	referIPTemplateID: number;
}

export interface IMChannelRequestFilter {
	
	pageSize: number;
	pageIndex: number;
}

export interface IMChannelUpdate {

	channelName: string;
	memo: string;


	// create API 獨有
	type: number;

	// update API 獨有
	targetChatID: string;
	imChannelID: number;
	referIPAddressTemplateID: number;
}
