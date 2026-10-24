import * as CommonEnums from '@/models/status/common';
import * as MerchantsEnums from '@/models/status/merchant';



export interface IQueryAnnouncementApiParameter {
	
	status : number;
	pageIndex : number;
	pageSize : number;
	orderByColumn : string;
	isASC : boolean;
}


export interface IAnnouncementListResponse {
	announcementID : number;
	announcementGuid : any;
	type : number;
	memo : string;
	status : number;
	statusText : string;
	beginTime : string | null;
	endTime : string | null;
	createTime : string | null;
	updateTime : string | null;
}


export interface IAnnouncementDetail {
	announcementID : number;
	announcementGuid : any;
	type : number;
	memo : string;
	status : number;
	beginTime : string | null;
	endTime : string | null;

	beginTimeTS_1000: number;
	endTimeTS_1000: number;

	assignMerchants : number[];
	merchantSelectTemplateID : number;
	contents : IAnnouncementContentBlockItem[];
}
	
export interface IAnnouncementContentBlockItem {
	contentBlockID : number;
	languageCode : string;
	ownerGuid : string;
	memo : string;
	title : string;
	description : string;
	imageUrlTop : string;
	imageUrlMiddle : string;
	imageUrlBottom : string;
	linkType : number;
	linkReference : string;
	isDefault : boolean;
}


export interface IAnnouncementCreateRequest {
	memo : string;
	type : number | null;
}
	