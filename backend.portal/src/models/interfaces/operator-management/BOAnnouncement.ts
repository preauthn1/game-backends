import * as CommonEnums from '@/models/status/common';

export interface IBOAnnouncement {
	boAnnouncementID: number;
	title: string;
	content: string;
	imageUrl: string;
	beginTime: string;
	endTime: string;
	type: number;
	hintType: number;
	status: number;
	isRead: boolean;
	assignParticipate: IBOAnnouncementAssignTypeRequest;
}

export interface IBOAnnouncementFilter {

	organizationID: number;
	referenceCreditMerchantID: number | null;
	pageIndex: number;
	pageSize: number;

}


export interface IBOAnnouncementRequest {
	boAnnouncementID: number;
	title: string;
	content: string;
	imageUrl: string;
	beginTime: string | null;
	endTime: string | null;
	assignTemplateID: string;
	type: number;
	status: number;
	hintType: number;
	replaceCreateBy: string;
	assignParticipate: IBOAnnouncementAssignTypeRequest;
}


export interface IBOAnnouncementAssignTypeRequest
{
	assignType: number;
	participaterIDs: number[];
}


export interface IHomePageAnnouncement {
	items: IBOAnnouncement[];
	popupAnns : IBOAnnouncement[];
}




