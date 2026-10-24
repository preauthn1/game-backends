import * as Model from '@/models/interfaces/system-setting/events';
import * as SystemSettingModel from '@/models/interfaces/system-setting/systemConfigSetting';
import * as ResponseModel from '@/models/interfaces/commons/response';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import Handler, { pageData } from '@/utilities/api-handler.lib';
import * as Qs from 'qs';

export default {

	async getEvents(filter : Model.IEventListRequest): Promise<ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IEventList>>> {
			
		const config = {
			url: `/events?${Qs.stringify(filter)}`,
			method: 'get',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IEventList>>>result.data;
	},
	
	async quickCreateEvent(payload : Model.IEventQuickCreateRequest): Promise<ResponseModel.IResponseData<void>> {
			
		const config = {
			url: `/event/quick-create`,
			method: 'post',
			data: payload,
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<void>>result.data;
	},


	async deleteEvent(myEventID : number): Promise<ResponseModel.IResponseData<ResponseModel.ICRUDResult>> {
		
		const payload = 
		{
			eventID : myEventID
		};

		const config = {
			url: `/event`,
			method: 'delete',
			data : payload
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.ICRUDResult>>result.data;
	},



	async updatePlayerRTP(payload : Model.IPlayerRTPUpdateRequest): Promise<ResponseModel.IResponseData<ResponseModel.ICRUDResult>> {
		
		const config = {
			url: `/player-rtp`,
			method: 'put',
			data : payload
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.ICRUDResult>>result.data;
	},

	async batchUpdatePlayerRTP(payload : Model.IPlayerRTPUpdateRequest): Promise<ResponseModel.IResponseData<ResponseModel.ICRUDResult>> {
		
		const config = {
			url: `/player-rtp/batch-update`,
			method: 'put',
			data : payload
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.ICRUDResult>>result.data;
	},

	async appendEventSetting(payload : Model.IEventAppendSettingRequest): Promise<ResponseModel.IResponseData<ResponseModel.ICRUDResult>> {
		const config = {
			url: `/event/append-setting`,
			method: 'post',
			data : payload
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.ICRUDResult>>result.data;
	},


	async resetEventPlayerRTP(myEventID : number): Promise<ResponseModel.IResponseData<ResponseModel.ICRUDResult>> {
		
		const payload = 
		{
			eventID : myEventID
		};

		const config = {
			url: `/player-rtp/reset-by-event`,
			method: 'put',
			data : payload
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.ICRUDResult>>result.data;
	},

	async getEventDetail(eventID : number): Promise<ResponseModel.IResponseData<Model.IEventDetail>> {
		
		const config = {
			url: `/event/detail/${eventID}`,
			method: 'get',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IEventDetail>>result.data;
	},

	async updateEvent(payload : Model.IEventDetail): Promise<ResponseModel.IResponseData<void>> {
		
		const config = {
			url: `/event/detail`,
			method: 'put',	
			data: payload,
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<void>>result.data;
	},

	
	async getGameVendorRTP(filter : Model.IGameVendorRTPRequest): Promise<ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IGameVendorRTP>>> {
			
		const config = {
			url: `/game-rtp?${Qs.stringify(filter)}`,
			method: 'get',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IGameVendorRTP>>>result.data;
	},

	async syncAGGAllRTP(model: DropdownModel.ILazyCommonParameterReqeust): Promise<ResponseModel.IResponseData<void>> {
			
		const config = {
			url: `/game-rtp/sync-agg-all`,
			method: 'post',
			data: model
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<void>>result.data;
	},


	async updateGameVendorRTP(payload : Model.IGameVendorRTPUpdateRequest): Promise<ResponseModel.IResponseData<void>> {
			
		const config = {
			url: `/game-rtp`,
			method: 'put',
			data:payload
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<void>>result.data;
	},


	async updateGameWinLimit(payload : Model.IGameVendorRTPUpdateRequest): Promise<ResponseModel.IResponseData<void>> {
			
		const config = {
			url: `/game-option/win-limit`,
			method: 'put',
			data:payload
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<void>>result.data;
	},


	async getPrizeRecords(filter : Model.IPrizeRecordRequest): Promise<ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IPrizeRecordList>>> {
			
		const config = {
			url: `/event/prize-records?${Qs.stringify(filter)}`,
			method: 'get',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IPrizeRecordList>>>result.data;
	},

	async submitManualSendEventPrize(payload : Model.IManualSendEventPrizeRequest): Promise<ResponseModel.IResponseData<void>> {
			
		const config = {
			url: `/event/manual-send-bonus`,
			method: 'post',
			data: payload
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<void>>result.data;
	},

	async fetchGameOptionRawResponse(vendorConfigID : number , gameID: number): Promise<ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IPrizeRecordList>>> {
			
		const config = {
			url: `/game-option/query-raw-response?vendorConfigID=${vendorConfigID}&gameID=${gameID}`,
			method: 'get',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IPrizeRecordList>>>result.data;
	},

	

	async getEventsDrawPrize(filter : Model.IEventDrawPrizeRequest): Promise<ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IEventDrawPrizeList>>> {
			
		const config = {
			url: `/events/multiple-prize?${Qs.stringify(filter)}`,
			method: 'get',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<ResponseModel.IPageListReponse<Model.IEventDrawPrizeList>>>result.data;
	},



	async getEventsDrawPrizeDetail(drawPrizeID : number): Promise<ResponseModel.IResponseData<Model.IEventDrawPrizeDetailInfo>> {
			
		const config = {
			url: `/events/multiple-prize/${drawPrizeID}`,
			method: 'get',
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<Model.IEventDrawPrizeDetailInfo>>result.data;
	},


	async updateEventDrawPrizeDetail(payload : Model.IEventDrawPrizeDetailInfo): Promise<ResponseModel.IResponseData<void>> {
			
		const config = {
			url: `/events/multiple-prize`,
			method: 'put',
			data : payload
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<void>>result.data;
	},

	async cloneEventDrawPrize(payload : Model.IEventDrawPrizeCloneRequest): Promise<ResponseModel.IResponseData<void>> {
			
		const config = {
			url: `/events/multiple-prize/clone`,
			method: 'post',
			data : payload
		};

		const result = await Handler.request(config);
		return <ResponseModel.IResponseData<void>>result.data;
	},


	async getRankEventPreview(filter:Model.IEventRankPreviewRequest):Promise<ResponseModel.IResponseData<Model.IEventRankPreview>> {

		const config = {
			url: `event/rank-preview?${Qs.stringify(filter)}`,
			method: 'get',
		};

		const result = await Handler.request(config);
		return (<ResponseModel.IResponseData<Model.IEventRankPreview>>result.data);
	},


	async rankEventSendPrize(myEventID : number , orderByColumn : number):Promise<ResponseModel.IResponseData<void>> {

		const payload = {
			eventID : myEventID,
			orderBy : orderByColumn
		};

		const config = {
			url: `event/manual-rank-prize`,
			method: 'post',
			data : payload
		};

		const result = await Handler.request(config);
		return (<ResponseModel.IResponseData<void>>result.data);
	},


	async getEventContentBlock(eventID : number):Promise<ResponseModel.IResponseData<Model.IEventContentBlockEditModelResponse>> {

		const config = {
			url: `event/content-block?eventID=` + eventID,
			method: 'get',
		};

		const result = await Handler.request(config);
		return (<ResponseModel.IResponseData<Model.IEventContentBlockEditModelResponse>>result.data);
	},


	async updateEventContentBlock(payload : Model.IEventContentBlockEditModelResponse):Promise<ResponseModel.IResponseData<void>> {

		const config = {
			url: `event/content-block`,
			method: 'put',
			data : payload
		};

		const result = await Handler.request(config);
		return (<ResponseModel.IResponseData<void>>result.data);
	},


	async copyEvent(payload : Model.IEventCopyRequest):Promise<ResponseModel.IResponseData<void>> {

		const config = {
			url: `event/copy`,
			method: 'post',
			data : payload
		};

		const result = await Handler.request(config);
		return (<ResponseModel.IResponseData<void>>result.data);
	},

	async initialContentBlock(eventID : number):Promise<ResponseModel.IResponseData<void>> {

		const payload = 
		{
			EventID : eventID
		};

		const config = {
			url: `event/initial-content-block`,
			method: 'post',
			data : payload
		};

		const result = await Handler.request(config);
		return (<ResponseModel.IResponseData<void>>result.data);
	},


	async getSelectMerchantByParentAgentSetting(eventID :number):Promise<ResponseModel.IResponseData<Model.ISelectMerchantByParentAgentRequest>> {

		const config = {
			url: `event/select-merchant-by-parent/info?eventID=` + eventID,
			method: 'get',
		};

		const result = await Handler.request(config);
		return (<ResponseModel.IResponseData<Model.ISelectMerchantByParentAgentRequest>>result.data);
	},


	async updateSelectMerchantByParentAgent(payload : Model.ISelectMerchantByParentAgentRequest):Promise<ResponseModel.IResponseData<void>> {

		const config = {
			url: `event/select-merchant-by-parent`,
			method: 'post',
			data : payload
		};

		const result = await Handler.request(config);
		return (<ResponseModel.IResponseData<void>>result.data);
	},

	async checkExportPrizeValid(payload : Model.IExportPrizeValidateRequest):Promise<ResponseModel.IResponseData<Model.IExportPrizeValidateResult>> {

		const config = {
			url: `event/check-export-prize-records-valid`,
			method: 'post',
			data : payload
		};

		const result = await Handler.request(config);
		return (<ResponseModel.IResponseData<Model.IExportPrizeValidateResult>>result.data);
	},
};
