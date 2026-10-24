import * as ResponseModel from '@/models/interfaces/commons/response';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import Handler from '@/utilities/api-handler.lib';
import * as Qs from 'qs';
import Cookies from 'js-cookie';
import { Locale } from '@/models/status/common';

export default {
	async getOrganizations(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/organizations`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getBetStatus(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/bet-record-status`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getGeneralTags(filterType : number| null = null): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/generalTags?type=${filterType}` ,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getTagTypes(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/tagTypes`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getEnableDisableStatus(): Promise<DropdownModel.IDropdownItems[]> {

		const config = {
			url: `/dropdown/enable-disable-status`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getVisibilityStatus(): Promise<DropdownModel.IDropdownItems[]> {

		const config = {
			url: `/dropdown/visibility-status`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getEventContentLayoutType(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/EventContent-LayoutType`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getCurrency(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/currency`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return dropdownItems;
	},


	async getMerchants(isIncludeCategory : boolean = false): Promise<DropdownModel.IDropdownItems[]> {

		const myFilter = {} as  DropdownModel.IMerchantDropdownRequest;
		myFilter.isIncludeCategory = isIncludeCategory;

		return this.getMerchantsByFilter(myFilter);
	},


	
	async getMerchantsByFilter(filter : DropdownModel.IMerchantDropdownRequest , isAssignSimplify : boolean = true): Promise<DropdownModel.IDropdownItems[]> {

		// 若有指定 會檢查全域設定
		if(isAssignSimplify) {
			const localSetting = localStorage.getItem('isEnableSimplifyDropdown');
			filter.isSimplifyItems = (localSetting && localSetting != null && localSetting === '1') ? true: false;
		}

		const config = {
			url: `/dropdown/merchants?${Qs.stringify(filter)}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getHierarchyMerchants(maxLayer : number | null = null): Promise<DropdownModel.IDropdownItems[]> {

		let query = '';

		if(maxLayer != null)
		{
			query = query + 'maxLayer='+ maxLayer;
		}

		const config = {
			url: `/dropdown/hierarchy-merchants?` + query,
			method: 'get',
		};

		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getHierarchyMerchantsByParam(payload: DropdownModel.ILazyCommonParameterReqeust): Promise<DropdownModel.IDropdownItems[]> {

		const config = {
			url: `/dropdown/hierarchy-merchants?${Qs.stringify(payload)}`,
			method: 'get',
		};
		
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getRootAgents(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/root-agents`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getMerchantSelectTemplatesByCategory(categoryID : number): Promise<DropdownModel.IDropdownItems[]> {
		const myFilter = {} as  DropdownModel.ILazyCommonParameterReqeust;
		myFilter.categoryID = categoryID;

		return this.getMerchantSelectTemplates(myFilter);
	},


	async getMerchantSelectTemplates(payload: DropdownModel.ILazyCommonParameterReqeust): Promise<DropdownModel.IDropdownItems[]> {

		const config = {
			url: `/dropdown/merchant-select-template?${Qs.stringify(payload)}`,
			method: 'get',
		};
		
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},


	async getEventCounterOrderBy(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/event-counter-order-by`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getMerchantsWithExtraInfo(isIncludeCategory : boolean = false): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/merchants?IsIncludeCategory=` + isIncludeCategory + '&IsIncludeExtraInfo=true',
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},


	async getMerchantsWithCurrency(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/merchants?isIncludeCurrency=true`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getMerchantTypes(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/merchant-types`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getMerchantSettingKeys(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/merchant-setting-keys`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},


	async getAgentAggregativeType(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/agentAggregativeType`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getAgentSpecialReportType(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/agent-special-report-type`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getDynamicQueryItems(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/dynamic-query-items`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertLang(dropdownItems);
	},

	async getNotifyQueueType(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/notifyQueueType`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getBOAnnouncementType(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/bo-announcement-types`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getBOAnnouncementHintType(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/bo-announcement-hintTypes`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	
	async getAdminSysAssignTypes(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/assign-types`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getSwLogsFilterTypes(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/sw-log-special-filter-type`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},


	async getApiRequestLogSpecialFilterTypes(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/special-filter-type/api-log`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getEventSpecialFilterTypes(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/special-filter-type/event`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},


	async getUsers(organizationType: number| null): Promise<DropdownModel.IDropdownItems[]> {
		let queryString = '?organizationType=';

		if(organizationType != null && organizationType > 0)
		{
			queryString = queryString + organizationType.toString();
		}

		const config = {
			url: `/dropdown/users` + queryString,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getRoles(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/roles`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertLang(dropdownItems);
	},

	async getTimezoneAreas(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/timezone-areas`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},
	
	async getLinkTypes(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/link-types`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getAnnouncementTypes(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/announcement-types`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getIMChannels(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/IMChannels`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},
	
	async getIPTemplates(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/ip-templates`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getMerchantSpecialFilterType(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/merchant-special-filter`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},
	
	async getUserSpecialFilterType(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/special-filter-type/users`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getIPTemplateSpecialFilterType(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/special-filter-type/ip-template`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getIMChannelSpecialFilterType(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/special-filter-type/imChannels`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},


	
	async getCopyMerchantExtaSettings(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/copy-merchant-extra-settings`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getGames(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/games`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getEnableGames(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/games?status=1`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getGamesOriginal(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/games`,
			method: 'get',
		};
		const result = await Handler.request(config);
		return (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
	},

	async getGamesStringFormat(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/games`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return dropdownItems;
	},


	async getGamesFilterGameVendor(gameVendor: number): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/games?gameVendorType=` + gameVendor,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getGameTypes(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/game-types`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getTransactionTypes(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/transactionTypes`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getGameVendorTransferTypes(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/game-vendor-transfer-type`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},


	async getOneTimesJobTypes(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/one-times-job-types`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getOneToManySettingTypes(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/one-to-many-setting-types`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getGameVendors(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/game-vendors-without-slot88`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getUserOperateTypes(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/user-operate-types`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},


	async getRoomLevels(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/room-levels`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertLang(dropdownItems);
	},


	async getServers(gameID: number | null = null): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/servers?${Qs.stringify({ gameID })}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertLang(dropdownItems);
	},

	async getServerAndVendorConfigs(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/server-and-vendorConfigs`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertLang(dropdownItems);
	},

	async getPerGameTags(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/perGame-Tags`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getPerGameTagsAllowManualAdd(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/perGame-Tags/allow-manual-add`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},	

	async GetAGGFishLogTypes(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/fish-log-types`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async GetAGGFreespinStatus(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/agg-freespin-status`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return dropdownItems;
	},

	async GetAGGPrizeTypes(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/agg-event-prize-types`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return dropdownItems;
	},

	
	async getPlayerTags(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/player-Tags`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getAllowManualAddPlayerTags(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/player-tags/allow-manual-add`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getSystemTags(dataCategory : number| null = null): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/system-Tags?dateCategory=` + dataCategory,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},


	async getThirdVendors(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/third-party-vendors`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getLanguages(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/BE-Languages`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return dropdownItems;
	},


	async getLanguageCodes(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/language-codes`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return dropdownItems;
	},


	async getInernalTestAccounts(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/internal-test-accounts`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getInernalTestMerchants(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/internal-merchants`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getMerchantsAllowCreateTestAccount(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/merchants-allow-create-test-account`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},


	async updateLanguage(lang: string): Promise<boolean> {
		const config = {
			url: '/users/language',
			method: 'put',
			data: { lang },
		};
		const result = await Handler.request(config);
		return <boolean>result.data;
	},

	async getEvents(): Promise<DropdownModel.IDropdownItems[]> {			
		const config = {
			url: `/dropdown/events?`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getEventSettingsForManualAdd(filter: DropdownModel.ILazyCommonParameterReqeust): Promise<DropdownModel.IDropdownItems[]> {	
		
		const config = {
			url: `/dropdown/event-settings-for-manual-add`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getEventContentColorType(): Promise<DropdownModel.IDropdownItems[]> {			
		const config = {
			url: `/dropdown/event-content-color-type`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getEventsByFilter(filter: DropdownModel.ILazyCommonParameterReqeust): Promise<DropdownModel.IDropdownItems[]> {	
		
		const config = {
			url: `/dropdown/events?${Qs.stringify(filter)}`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getEventTypes(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/event-types`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getEventTemplateTypes(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/event-template-types`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getExtraFilterType(categoryID : number = 0): Promise<DropdownModel.IDropdownItems[]> {
		const payload = {} as DropdownModel.ILazyCommonParameterReqeust;
		payload.categoryID = categoryID;

		const config = {
			url: `/dropdown/extra-filter-type?${Qs.stringify(payload)}`,
			method: 'get',
		};

		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getMerchantCreditChangeType(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/credit-change-type`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},


	async getEventPrizeTypes(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/event-prize-types`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getEventMultiplePrizeTypes(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/by-type-name/EventMultiplePrizeTypeEnum`,
			method: 'get',
		};

		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getNormalStatus(isExcludePermanentDelete :boolean = false): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/normal-active-status`,
			method: 'get',
		};
		const result = await Handler.request(config);
		let dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;

		// 若指定排除。會少一個選項
		if(isExcludePermanentDelete)
		{
			dropdownItems = dropdownItems.filter(x=> x.valueInt !== 999 );
		}
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getYesOrNoOrNotSelect(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/normal-yes-no-notselect-status`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},


	async getPrizeRecordStatus(): Promise<DropdownModel.IDropdownItems[]> {
		const config = {
			url: `/dropdown/prize-record-status`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getVendorConfigCurrencyDisplay(vendorID : number| null = null): Promise<DropdownModel.IDropdownItems[]> {

		const targetVendorID = vendorID == null ? '' : vendorID.toString();
		const config = {
			url: `/dropdown/vendor-config-currency-display?vendorID=` + targetVendorID,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getVendorConfig(vendorID : number| null = null): Promise<DropdownModel.IDropdownItems[]> {

		const targetVendorID = vendorID == null ? '' : vendorID.toString();
		const config = {
			url: `/dropdown/vendor-config?vendorID=` + targetVendorID,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getVendorConfigSupportAggFeature(): Promise<DropdownModel.IDropdownItems[]> {

		const config = {
			url: `/dropdown/vendor-config-support-aggAPI`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getGameListExtraFilterType(): Promise<DropdownModel.IDropdownItems[]> {

		const config = {
			url: `/dropdown/game-extra-filter-type`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getUserAssignMerchantMode(): Promise<DropdownModel.IDropdownItems[]> {

		const config = {
			url: `/dropdown/user-assign-merchant-mode`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},


	async getSortDirection(): Promise<DropdownModel.IDropdownItems[]> {

		const config = {
			url: `/dropdown/sort-direction`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getStatReportSearchColumns(reportType : string): Promise<DropdownModel.IDropdownItems[]> {

		// ※後端程式碼搜尋 GetStaReportSearchColumns
		// agent-total-report
		// merchant-summary-report
		const config = {
			url: `/dropdown/search-extra-columns/` + reportType,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},

	async getTGAnnouncementTemplates(): Promise<DropdownModel.IDropdownItems[]> {

		const config = {
			url: `/dropdown/tg-announcement-templates`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		
		return convertDropdownStringToNumberValue(dropdownItems);
	},


	async getLatestMonth(count: number = 12): Promise<DropdownModel.IDropdownItems[]> {

		const config = {
			url: `/dropdown/latest-month?count=` + count.toString(),
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return dropdownItems;
	},


	async getSortAmountColumns(): Promise<DropdownModel.IDropdownItems[]> {

		const config = {
			url: `/dropdown/sort-amount-columns`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return dropdownItems;
	},


	async getInvoiceExportCategory(): Promise<DropdownModel.IDropdownItems[]> {

		const config = {
			url: `/dropdown/InvoiceExportCategory`,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},


	async getVendorConfigSupportAccountingFetch(isTestEnvironment : boolean | null = null): Promise<DropdownModel.IDropdownItems[]> {
		const querystring = isTestEnvironment == null ? '' : 'IsTestEnvironment=' +isTestEnvironment;

		const config = {
			url: `/dropdown/vendor-config/support-fetch-report?` + querystring,
			method: 'get',
		};
		const result = await Handler.request(config);
		const dropdownItems = (<ResponseModel.IResponseData<DropdownModel.IDropdownItems[]>>result.data).data;
		return convertDropdownStringToNumberValue(dropdownItems);
	},


};



export const convertDropdownStringToNumberValue = (dropdownItems: DropdownModel.IDropdownItems[]): DropdownModel.IDropdownItems[] => {

	const res = dropdownItems.map((element: DropdownModel.IDropdownItems) => {
		return <DropdownModel.IDropdownItems>{
			extraProperty: element.extraProperty,
			text: element.text,
			text_CN: element.text_CN,
			text_EN: element.text_EN,
			fullTextForKeywordSearch : element.fullTextForKeywordSearch,
			value: Number(element.value),
			valueInt : element.valueInt,
			valueIntOrZero: element.valueInt == null ? 0 : element.valueInt,
		};
	});

	return res;
	
};


export const convertLang = (dropdownItems: DropdownModel.IDropdownItems[]): DropdownModel.IDropdownItems[] => {
	let res = dropdownItems.map((element: DropdownModel.IDropdownItems) => {
		return <DropdownModel.IDropdownItems>{
			extraProperty: element.extraProperty,
			text: element.text,
			text_CN: element.text_CN,
			text_EN: element.text_EN,
			value: element.value,
		};
	});
	const locale = Cookies.get('locale');
	switch (locale) {
		case Locale.ZHCN:
			res = dropdownItems.map((element: DropdownModel.IDropdownItems) => {
				return <DropdownModel.IDropdownItems>{
					extraProperty: element.extraProperty,
					text: element.text_CN,
					text_CN: element.text_CN,
					text_EN: element.text_EN,
					value: element.value,
				};
			});
			break;
		case Locale.ENUS:
			res = dropdownItems.map((element: DropdownModel.IDropdownItems) => {
				return <DropdownModel.IDropdownItems>{
					extraProperty: element.extraProperty,
					text: element.text_EN,
					text_CN: element.text_CN,
					text_EN: element.text_EN,
					value: element.value,
				};
			});
			break;
	}
	return <DropdownModel.IDropdownItems[]>res;
};
