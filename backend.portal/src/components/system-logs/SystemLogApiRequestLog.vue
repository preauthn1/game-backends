<template>
	<v-app v-if="renderComponent" class="app">
		<v-expansion-panels v-model="panel" multiple>
			<v-expansion-panel>
				<v-expansion-panel-content>
					<v-flex>
						<v-row class="search-form">
							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-select :items="tables" v-model="searchData.targetTableName" chips @change="renewSearchDataObject()" :label="'日誌資料表'"> </v-select>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-text-field v-model="searchData.relativeUrl" :label="'網址分類'" clearable> </v-text-field>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete :items="dropdownOfKeywordColumns" clearable v-model="searchData.myInputKeywordFilterType" :label="'關鍵字Like查詢特定欄位(需搭配右欄)'">
									</v-autocomplete>
								</div>
							</v-col>
							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-text-field v-model="searchData.myInputKeyword" :label="'查詢關鍵字'" clearable> </v-text-field>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-text-field v-model="searchData.identityKey" :label="displayLabelIdentity" clearable> </v-text-field>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-text-field v-model="searchData.searchID" :label="'Id(系統ID)／UniqueID'" clearable> </v-text-field>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete clearable :items="dropdownCategory" @change="whenDropdownCategoryChanged()" v-model="searchData.tempValue" :label="'常用網址分類'">
									</v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-text-field v-model="searchData.responseStatusCode" :label="'StatusCode'" clearable> </v-text-field>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<date-picker v-model="datetimeRangePicker.searchStartTimeRange" type="datetime" value-type="timestamp" range :placeholder="$t('TableHeader.__createTime2')">
									</date-picker>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="" style="margin-top: 25px">
									<v-chip class="mr-1" :class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'today' }" @click="globalQuicklyPickTimeRange('today')">
										<v-icon left small>mdi-alarm-check</v-icon>
										{{ $t(`TableHeader.__today`) }}
									</v-chip>
									<v-chip class="mr-1" :class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'yesterday' }" @click="globalQuicklyPickTimeRange('yesterday')">
										<v-icon left small>mdi-alarm-check</v-icon>
										{{ $t(`TableHeader.__yesterday`) }}
									</v-chip>
									<v-chip class="mr-1" :class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'thisWeek' }" @click="globalQuicklyPickTimeRange('thisWeek')">
										<v-icon left small>mdi-alarm-check</v-icon>
										{{ $t(`TableHeader.__thisWeek`) }}
									</v-chip>

									<v-chip class="mr-1" :class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'lastWeek' }" @click="globalQuicklyPickTimeRange('lastWeek')">
										<v-icon left small>mdi-alarm-check</v-icon>
										{{ $t(`TableHeader.__lastWeek`) }}
									</v-chip>

									<v-chip class="mr-1 mt-1" :class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === '1hour' }" @click="globalQuicklyPickTimeRange('1hour')">
										<v-icon left small>mdi-alarm-check</v-icon>
										1 hour
									</v-chip>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete v-model="searchData.specialFilterType" :items="dropdownItemsSpecialTypes" :label="'特殊篩選'"> </v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item search-row-padding">
									<v-btn color="yellow darken-1" text class="btn search" @click.stop="search">
										<v-icon left>search</v-icon>
										{{ $t('Button.__search') }} (top 500筆) </v-btn
									>&#9;

									<v-btn color="pink lighten-3" text class="btn search" @click.stop="clear">
										<v-icon left>clear</v-icon>
										{{ $t('Button.__reset') }} </v-btn
									>&#9;

									<v-btn color="blue lighten-3" text class="btn search" @click.stop="exportFiles">
										<v-icon left>cloud_download</v-icon>
										{{ $t('Button.__downloadFile') }} </v-btn
									>&#9;
								</div>
							</v-col>
						</v-row>
					</v-flex>
				</v-expansion-panel-content>
			</v-expansion-panel>
		</v-expansion-panels>

		<div class="mt-2 mb-2">
			<div v-if="debugSQL != null">
				<v-btn color="grey lighten-1" small text class="btn search" @click.stop="copyTextToClipboard2(debugSQL)">
					<v-icon left>file_copy</v-icon>
					常用SQL複製
				</v-btn>
			</div>
		</div>

		<v-data-table :headers="headers" id="table" :items="ListData" :footer-props="myFooterProps" multi-sort>
			<template v-slot:item.action="{ item }">
				<v-icon small class="mr-2" style="color: #ccc; font-size: 20px" @click="showInfo(item)"> info </v-icon>
			</template>

			<template v-slot:item.id="{ item }">
				<span class="f14 link-text-white" @click="showSingleElasticsearchData(item.elasticIndexName, item.id)">
					{{ item.id }}
				</span>
			</template>

			<template v-slot:item.category="{ item }">
				<span class="copyTextTo" @click="copyTextToClipboard2(item.category)">
					{{ item.category }}
				</span>
			</template>

			<template v-slot:item.responseTime="{ item }">
				<span :class="item.responseTime > 5000 ? 'minus-red' : ''">
					{{ (item.responseTime / 1000).toFixed(1) }}
				</span>
			</template>

			<template v-slot:item.requestUrl="{ item }">
				<div style="display: inline-block; width: 450px; text-overflow: ellipsis; overflow: hidden" :title="item.requestUrl">
					{{ item.requestUrl }}
				</div>
			</template>

			<template v-slot:item.responseStatusCode="{ item }">
				<span :class="item.responseStatusCode != 200 ? 'minus-red' : ''">
					{{ item.responseStatusCode }}
				</span>
			</template>

			<template v-slot:item.createTime="{ item }">
				<span class="copyTextTo" @click="globalPopupCopyDateTime(item.createTime)">
					{{ item.createTime | timeWithAddTimezone(userProfile.timeZone) }}
				</span>
			</template>
		</v-data-table>

		<ExternalApiRequestLogDetailPopup ref="logDetailPopup"> </ExternalApiRequestLogDetailPopup>

		<PopupCopyDatetime ref="refCopyDateTime"></PopupCopyDatetime>
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import ExternalApiRequestLogDetailPopup from '@/components/system-logs/ExternalApiRequestLogDetailPopup.vue';
import PopupCopyDatetime from '@/components/PopupCopyDatetime.vue';
import EventBus from '@/utilities/event-bus';
import * as SettingModel from '@/models/interfaces/system-setting/systemConfigSetting';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import * as Model from '@/models/interfaces/system-logs/externalApiRequestLog';
import LogApi from '@/api/system-logs/externalApiRequestLog';
import DropdownApi from '@/api/commons/dropdown';
import { Locale, Currency, Status, GameType, CRUDAction, GameVendorType } from '@/models/status/common';
import { WalletMode } from '@/models/status/merchant';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { ALL_CURRENCY } from '@/utilities/enums_helper';
import NotificationWarning from '@/components/notification/NotificationWarning.vue';
import internalTestApi from '@/api/commons/internalTest';
import { pageData, time } from '@/utilities/api-handler.lib';
// import Validation from '@/utilities/validation';
import { datetimeMixin, startEndDate, daysAgo, dateStringToTimestamp, timeWithTimezone, toDateStringMMDDByDateText } from '@/utilities/datetime-format';
import DatePicker from 'vue2-datepicker';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import { commonMethodMixin } from '@/utilities/common-methods';
import { State, Action, Getter, namespace } from 'vuex-class';

const authModule = namespace('Auth');

import * as Permission from '@/models/status/permission';

// @ts-ignore
@Component({ components: { NotificationWarning, ExternalApiRequestLogDetailPopup, DatePicker, PopupCopyDatetime }, mixins: [commonDisplayFiltersMixin, commonMethodMixin, datetimeMixin] })
export default class SystemLogApiRequestLog extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];
	renderComponent: boolean = true;
	isShowInfoDialog: boolean = false;
	panel: number[] = [0];

	mySelectTableNameV2: string = '';
	datetimeRangePicker: DropdownModel.DatetimeRangePicker = new DropdownModel.DatetimeRangePicker();
	searchData: Model.IExternalApiRequestLogPagingRequest = {} as Model.IExternalApiRequestLogPagingRequest;

	debugSQL: string = '';
	ListData: Model.IExternalApiRequestLogResponse[] = [];
	Settings: SettingModel.ISystemConfigSetting = {} as SettingModel.ISystemConfigSetting;
	deleteID: number = 0;

	myFooterProps: any = { 'items-per-page-options': [10, 30, 50, 100] };
	DataTablePagingParameter: DropdownModel.DataTablePagingParameter = new DropdownModel.DataTablePagingParameter();
	editModeOpenGroups: number[] = [];

	dropdownItemsSpecialTypes: DropdownModel.IDropdownItems[] = [];

	get tables() {
		const dropdown = [
			{ text: 'AdminSysAPI', value: '[ApiRequestLogAdminSysAPI]' },
			{ text: 'AdminSysAPIError', value: '[ApiRequestLogAdminSysAPIError]' },
			{ text: 'ExternalVendorAPI', value: '[ApiRequestLogExternalVendorAPI]' },
			// { text: 'ExternalVendorAPIOther', value: '[ApiRequestLogExternalVendorAPIOther]' },
			{ text: 'ExternalVendorAPIError', value: '[ApiRequestLogExternalVendorAPIError]' },
			{ text: 'GameAPI', value: '[ApiRequestLogGameAPI]' },
			{ text: 'GameAPIError', value: '[ApiRequestLogGameAPIError]' },
			{ text: 'JobAPI', value: '[ApiRequestLogJobAPI]' },
			{ text: 'JobAPIError', value: '[ApiRequestLogJobAPIError]' },
			{ text: 'PublicAPI', value: '[ApiRequestLogPublicAPI]' },
			{ text: 'PublicAPIError', value: '[ApiRequestLogPublicAPIError]' },
			{ text: 'VendorAPI', value: '[ApiRequestLogVendorAPI]' },
			{ text: 'VendorAPIError', value: '[ApiRequestLogVendorAPIError]' },
			// { text: 'VendorAPIError (Brief)', value: '[ApiRequestLogVendorAPIErrorBrief]' },
			// { text: 'VendorAPIErrorOther', value: '[ApiRequestLogVendorAPIOther]' },
		];

		return dropdown;
	}

	get headers() {
		const data = [
			{ text: this.$t('TableHeader.__action'), value: 'action', sortable: false, align: 'center' },
			{ text: this.$t('TableHeader.__ID') + '／UniqueID', value: 'id', sortable: false, align: 'start' },
			{ text: this.$t('TableHeader.__createTime2'), value: 'createTime', sortable: false },
			{ text: '網址分類', value: 'category', sortable: false },
			{ text: 'Method', value: 'subCategory', sortable: false },
			{ text: '資訊1', value: 'identityText1', sortable: false },
			{ text: '資訊2', value: 'identityText2', sortable: false },
			{ text: '資訊3', value: 'debugInfo3', sortable: false },
			{ text: 'IPAddress', value: 'ipAddress', sortable: false },
			{ text: 'StatusCode', value: 'responseStatusCode', sortable: false, align: 'end' },
			{ text: '回應時間(秒)', value: 'executionTimeSeconds', sortable: false, align: 'end' },
			{ text: 'webSiteName', value: 'webSiteName', sortable: false },
		];
		return data;
	}

	get dropdownOfKeywordColumns() {
		const dropdown = [
			{ text: '*搜尋所有欄位 (匹配關鍵字) ', value: 99 },
			{ text: '*搜尋所有欄位 (排除指定關鍵字)', value: 98 },
			{ text: 'HttpRquestBody (Like query)', value: 1 },
			{ text: 'HttpResponseText (Like query)', value: 2 },
			{ text: 'RequestUrl (Like query)', value: 3 },
			{ text: 'RelativeUrl (Like query)', value: 4 },
			{ text: 'ReferOrderNo (參考單號) ', value: 21 },
			{ text: 'MerchantID (部分情境才會記錄到) ', value: 25 },
			{ text: 'PlayerID (部分情境才會記錄到) ', value: 27 },
			{ text: 'IPAddress', value: 31 },
			{ text: 'DebugInfo', value: 81 },
		];

		return dropdown;
	}

	get dropdownCategory() {
		const dropdown = [
			{ text: '/agg/supplier/v2/transaction/win', value: '/agg/supplier/v2/transaction/win', table: '[ApiRequestLogExternalVendorAPI]', table2: '[ApiRequestLogExternalVendorAPIError]' },
			{ text: '/agg/supplier/v2/transaction/bet', value: '/agg/supplier/v2/transaction/bet', table: '[ApiRequestLogExternalVendorAPI]', table2: '[ApiRequestLogExternalVendorAPIError]' },
			{ text: '/agg/supplier/v2/wager/cancel', value: '/agg/supplier/v2/wager/cancel', table: '[ApiRequestLogExternalVendorAPI]', table2: '[ApiRequestLogExternalVendorAPIError]' },
			{ text: '/agg/supplier/v2/bonus/win', value: '/agg/supplier/v2/bonus/win', table: '[ApiRequestLogExternalVendorAPI]', table2: '[ApiRequestLogExternalVendorAPIError]' },

			{ text: '/agg/supplier/v2/user/authorize', value: '/agg/supplier/v2/user/authorize', table: '[ApiRequestLogExternalVendorAPI]', table2: '[ApiRequestLogExternalVendorAPIError]' },
			{ text: '/agg/supplier/v2/user/balance', value: '/agg/supplier/v2/user/balance', table: '[ApiRequestLogExternalVendorAPI]', table2: '[ApiRequestLogExternalVendorAPIError]' },

			// { text: 'XXXXXXXXXXX', value: 'XXXXXXXXXXX', table: '[ApiRequestLogExternalVendorAPI]', table2: '[ApiRequestLogExternalVendorAPIError]' },

			{ text: '/game/link', value: '/v1/game/link', table: '[ApiRequestLogVendorAPI]', table2: '[ApiRequestLogVendorAPIError]' },
			{ text: '/game/launch-link', value: '/v1/game/launch-link', table: '[ApiRequestLogVendorAPI]', table2: '[ApiRequestLogVendorAPIError]' },
		];

		return dropdown;
	}

	get displayLabelIdentity() {
		if (this.IsSelectVendorAPI) {
			return 'Identity識別 (ES不適用)';
		}

		return 'Identity識別 (ES不適用)';
	}

	get IsSelectVendorAPI() {
		if (this.searchData.targetTableName === '[ApiRequestLogVendorAPI]' || this.searchData.targetTableName === '[ApiRequestLogVendorAPIError]') {
			return true;
		}

		return false;
	}

	renewSearchDataObject() {
		// 強制重建物件 讓change事件被刷新
		this.searchData = { ...this.searchData };
	}

	whenDropdownCategoryChanged() {
		const mySearchFilter: any = this.searchData;
		mySearchFilter.relativeUrl = mySearchFilter.tempValue;

		const match = this.dropdownCategory.filter((x) => x.value === mySearchFilter.tempValue);

		if (match && match.length > 0) {
			if (mySearchFilter.targetTableName !== match[0].table && mySearchFilter.targetTableName !== match[0].table2) {
				mySearchFilter.targetTableName = match[0].table;
			}
		}
	}

	clear() {
		const temp1: any = this.searchData.targetTableName;

		this.searchData = {} as Model.IExternalApiRequestLogPagingRequest;
		this.searchData.targetTableName = temp1;
	}

	search() {
		this.DataTablePagingParameter.pageIndex = 1;
		this.refresh();
	}

	refresh() {
		this.registerReloadComponent();

		const searchFilter = { ...this.searchData };

		if (this.searchData.myInputKeywordFilterType === 1) {
			searchFilter.requestBodyKeyword = this.searchData.myInputKeyword;
		} else if (this.searchData.myInputKeywordFilterType === 2) {
			searchFilter.responseTextKeyword = this.searchData.myInputKeyword;
		} else if (this.searchData.myInputKeywordFilterType === 3) {
			searchFilter.requestUrlKeyword = this.searchData.myInputKeyword;
		} else if (this.searchData.myInputKeywordFilterType === 4) {
			searchFilter.relativeUrlKeyword = this.searchData.myInputKeyword;
		} else if (this.searchData.myInputKeywordFilterType === 21) {
			searchFilter.referOrderNo = this.searchData.myInputKeyword;
		} else if (this.searchData.myInputKeywordFilterType === 25) {
			searchFilter.merchantID = this.searchData.myInputKeyword;
		} else if (this.searchData.myInputKeywordFilterType === 27) {
			searchFilter.playerID = this.searchData.myInputKeyword;
		} else if (this.searchData.myInputKeywordFilterType === 31) {
			searchFilter.IPAddress = this.searchData.myInputKeyword;
		} else if (this.searchData.myInputKeywordFilterType === 81) {
			searchFilter.debugInfo = this.searchData.myInputKeyword;
		} else if (this.searchData.myInputKeywordFilterType === 98) {
			searchFilter.SearchAllColumnsNotLikeKeyword = this.searchData.myInputKeyword;
		} else if (this.searchData.myInputKeywordFilterType === 99) {
			searchFilter.searchAllColumnsKeyword = this.searchData.myInputKeyword;
		}

		searchFilter.pageIndex = this.DataTablePagingParameter.pageIndex;
		searchFilter.pageSize = 500;
		searchFilter.createTimeBegin = this.datetimeRangePicker.searchStartTimeRange[0] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[0]) : null;
		searchFilter.createTimeEnd = this.datetimeRangePicker.searchStartTimeRange[1] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[1]) : null;

		EventBus.$emit('show_mask', null);

		LogApi.getSystemLogApiRequestLogs(searchFilter)
			.then((res) => {
				this.ListData = res.data.items;
				this.DataTablePagingParameter.totalCount = res.data.totalCount;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	geti18nTextAsString(key: string) {
		return this.$t(key).toString();
	}

	exportFiles() {
		if (this.ListData == null || this.ListData.length <= 0) {
			alert('請先進行搜尋');
			return;
		}

		const excelHeaders: any = [
			{ text: this.geti18nTextAsString('TableHeader.__time') + '(GMT+' + this.userProfile.timeZone.toString() + ')', width: 20 },
			{ text: 'ID', width: 35 },
			{ text: 'ReqeustUrl', width: 35 },
			{ text: 'RequestMethod', width: 18 },
			{ text: 'StatusCode', width: 15 },
			{ text: 'Memo1', width: 18 },
			{ text: 'Memo2', width: 18 },
		];

		const excelData: any = [];
		EventBus.$emit('show_mask', null);

		this.ListData.forEach((value) => {
			const eachRow: any = [];

			eachRow.push(timeWithTimezone(value.createTime, this.userProfile.timeZone));
			eachRow.push(value.id);
			eachRow.push(value.category);
			eachRow.push(value.subCategory);

			// eachRow.push(value.requestContent);
			// eachRow.push(value.responseText);

			eachRow.push(value.responseStatusCode);
			eachRow.push(value.debugInfo);
			eachRow.push(value.debugInfo2);

			excelData.push(eachRow);
		});

		const beginText = this.datetimeRangePicker.searchStartTimeRange[0] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[0]) : null;
		const endText = this.datetimeRangePicker.searchStartTimeRange[1] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[1]) : null;

		const datePrefix = '_' + toDateStringMMDDByDateText(beginText, this.userProfile.timeZone) + '_' + toDateStringMMDDByDateText(endText, this.userProfile.timeZone);

		const config = {
			headersWithConfig: excelHeaders,
			rowData: excelData,
			fileName: 'Export_ApiReqeustLogs' + datePrefix + '.xlsx',
		};

		console.log(config);

		setTimeout(() => {
			EventBus.$emit('hide_mask', null);
		}, 1000);

		// @ts-ignore
		this.downloadExcel(config);
	}

	registerReloadComponent() {
		EventBus.$on(`reload-${this.$router.currentRoute.path}`, (info: any) => {
			this.renderComponent = false;
			this.$nextTick(() => {
				this.renderComponent = true;
			});
		});
	}

	showInfo(item: Model.IExternalApiRequestLogResponse) {
		if (item.isSearchDetailByAPI) {
			EventBus.$emit('show_mask', null);
			LogApi.getSystemLogApiRequestLogsByUniqueID(item.uniqueID, this.searchData.targetTableName)
				.then((res) => {
					const resultItem = res.data;
					this.openDetailPopupDialog(resultItem);
				})
				.finally(() => {
					EventBus.$emit('hide_mask', null);
				});
		} else {
			this.openDetailPopupDialog(item);
		}
	}

	showSingleElasticsearchData(indexName: string, uniqueID: string) {
		const myFilter: any = {
			querySqlID: 900858,
			TableName: indexName,
			UniqueID: uniqueID,
		};

		const myConfig: any = {
			maxWidth: 1200,
		};

		EventBus.$emit(`globalShowCustomizedColumnsDisplayPopupDialog`, myFilter, myConfig);
	}

	openDetailPopupDialog(item: Model.IExternalApiRequestLogResponse) {
		const model: Model.IExternalApiRequestLogDetailInfo = {} as Model.IExternalApiRequestLogDetailInfo;
		model.requestUrl = item.requestUrl;
		model.requestContent = item.requestContent;
		model.requestHeader = item.requestHeader;
		model.responseText = item.responseText;
		model.responseStatusCode = item.responseStatusCode;
		model.createTime = item.createTime;

		this.$nextTick(() => {
			const targetRef: any = this.$refs.logDetailPopup;
			targetRef.showInfoPopup(model);
		});
	}

	changePage(index: number = 1) {
		this.DataTablePagingParameter.pageIndex = index;
		this.refresh();
	}

	changeItemPerPage(val: number = 10) {
		this.DataTablePagingParameter.itemsPerPage = val;
		this.refresh();
	}

	resetFormValidate() {
		const form: any = this.$refs.loginForm;
		form.resetValidation();
	}

	close() {
		this.isShowInfoDialog = false;
	}

	save() {}

	mounted() {
		const defaultBegin = Date.now() + -1 * 60 * 240 * 1000;
		this.datetimeRangePicker.searchStartTimeRange = [defaultBegin, daysAgo(1)];

		const referenceTransactionNo = this.$route.query.referenceTransactionNo;
		const begin = this.$route.query.begin;

		if (referenceTransactionNo && referenceTransactionNo != null) {
			this.searchData.referenceTransactionNo = String(referenceTransactionNo);
		}

		if (begin && begin != null) {
			const beginTS = dateStringToTimestamp(String(begin), 0);
			this.datetimeRangePicker.searchStartTimeRange[0] = beginTS - 1800 * 1000;
			this.datetimeRangePicker.searchStartTimeRange[1] = beginTS + 1800 * 1000;
		}

		this.searchData.targetTableName = '[ApiRequestLogExternalVendorAPI]';
		this.DataTablePagingParameter.itemsPerPage = 10;

		DropdownApi.getApiRequestLogSpecialFilterTypes().then((res) => {
			this.dropdownItemsSpecialTypes = res;
		});

		internalTestApi.getAdminSysFavoritSetting().then((res) => {
			this.debugSQL = res.data.debugSQL_1;
		});
	}
}
</script>
