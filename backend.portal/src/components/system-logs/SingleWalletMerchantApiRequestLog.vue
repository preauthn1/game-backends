<template>
	<v-app v-if="renderComponent" class="app">
		<v-expansion-panels v-model="panel" multiple>
			<v-expansion-panel>
				<v-expansion-panel-content>
					<v-flex>
						<v-row class="search-form">
							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-text-field v-model="searchFilter.category" :label="$t('TableHeader.__category')" clearable> </v-text-field>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-text-field v-model="searchFilter.subCategory" :label="$t('TableHeader.__subCategory')" clearable> </v-text-field>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete
										:items="dropdownOfKeywordColumns"
										clearable
										@change="whenChangeKeywordFilterType()"
										v-model="searchFilter.myInputKeywordFilterType"
										:label="'關鍵字Like查詢特定欄位(需搭配右欄)'"
									>
									</v-autocomplete>
								</div>
							</v-col>
							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-text-field v-model="searchFilter.myInputKeyword" :label="'查詢關鍵字'" clearable> </v-text-field>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-text-field v-model="searchFilter.uniqueID" :label="'UniqueID (LogID 非注單號)'" clearable> </v-text-field>
								</div>
							</v-col>
							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete :items="dropdownMerchants" v-model="searchFilter.merchantID" :label="'商戶列表'" clearable> </v-autocomplete>
									<!--test -->
									<!--<v-text-field v-model="searchFilter.merchantID" :label="'商戶ID'" clearable> </v-text-field> -->
								</div>
							</v-col>
							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete clearable :items="dropdownCategory" v-model="searchFilter.subCategory" :label="'常用分類'"> </v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-text-field v-model="searchFilter.responseStatusCode" :label="'StatusCode'" clearable> </v-text-field>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<date-picker v-model="datetimeRangePicker.searchStartTimeRange" type="datetime" value-type="timestamp" range :placeholder="$t('TableHeader.__createTime2')">
									</date-picker>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="mt-4">
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
									<v-autocomplete :items="dropdownItemsSpecialFilterTypes" clearable v-model="searchFilter.specialFilterType" :label="'特殊篩選'"> </v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item search-row-padding">
									<v-btn color="yellow darken-1" text class="btn search" @click.stop="search">
										<v-icon left>search</v-icon>
										{{ $t('Button.__search') }}(Top 200筆) </v-btn
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

		<v-data-table :headers="headers" id="table" :items="ListData" multi-sort>
			<template v-slot:item.action="{ item }">
				<v-icon small class="mr-2" style="color: #ccc; font-size: 20px" @click="showInfo(item)"> info </v-icon>
			</template>

			<template v-slot:item.id="{ item }">
				<span v-if="item.id > 0" class="f14 link-text-white" @click="showSingleElasticsearchData(item.uniqueID)">
					{{ item.id }}
				</span>
				<span v-else class="f14 link-text-white" @click="showSingleElasticsearchData(item.uniqueID)">
					{{ item.uniqueID }}
				</span>
			</template>

			<template v-slot:item.responseTime="{ item }">
				<div v-if="item.responseTime == 0">-</div>
				<div v-else>
					<span :class="item.responseTime > 5000 ? 'minus-red' : ''">
						{{ (item.responseTime / 1000).toFixed(1) }}
					</span>
				</div>
			</template>

			<template v-slot:item.requestUrl="{ item }">
				<div style="display: inline-block; width: 300px; text-overflow: ellipsis; overflow: hidden">
					{{ item.requestUrl }}
				</div>
			</template>

			<template v-slot:item.responseStatusCode="{ item }">
				<v-icon v-if="item.isNotSuccessCode" color="red lighten-1"> error </v-icon>
				<span :class="item.responseStatusCode != 200 || item.isNotSuccessCode ? 'minus-red' : ''">
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
export default class SingleWalletMerchantApiRequestLogService extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];
	renderComponent: boolean = true;
	isShowInfoDialog: boolean = false;
	panel: number[] = [0];

	datetimeRangePicker: DropdownModel.DatetimeRangePicker = new DropdownModel.DatetimeRangePicker();
	searchFilter: Model.IExternalApiRequestLogPagingRequest = {} as Model.IExternalApiRequestLogPagingRequest;
	uniqueDetailInfo: Model.IExternalApiRequestLogResponse = {} as Model.IExternalApiRequestLogResponse;
	ListData: Model.IExternalApiRequestLogResponse[] = [];
	Settings: SettingModel.ISystemConfigSetting = {} as SettingModel.ISystemConfigSetting;
	deleteID: number = 0;

	DataTablePagingParameter: DropdownModel.DataTablePagingParameter = new DropdownModel.DataTablePagingParameter();
	editModeOpenGroups: number[] = [];

	dropdownItemsSpecialFilterTypes: DropdownModel.IDropdownItems[] = [];
	dropdownMerchants: DropdownModel.IDropdownItems[] = [];

	get headers() {
		const data = [
			{ text: this.$t('TableHeader.__ID'), value: 'id', sortable: false, align: 'start' },
			{ text: this.$t('TableHeader.__createTime2'), value: 'createTime', sortable: false },
			{ text: this.$t('TableHeader.__merchant'), value: 'merchantID', sortable: false },
			{ text: this.$t('TableHeader.__subCategory'), value: 'subCategory', sortable: false },
			{ text: '請求網址', value: 'requestUrl', sortable: false },
			{ text: 'StatusCode', value: 'responseStatusCode', sortable: false, align: 'end' },
			{ text: '回應時間', value: 'responseTime', sortable: false, align: 'end' },
			{ text: this.$t('TableHeader.__memo'), value: 'debugInfo2', sortable: false },
			{ text: this.$t('TableHeader.__memo'), value: 'debugInfo3', sortable: false },
			{ text: 'webSiteName', value: 'webSiteName', sortable: false },
		];

		if (true) {
			data.splice(0, 0, { text: this.$t('TableHeader.__action'), value: 'action', sortable: false, align: 'center' });
		}

		return data;
	}

	get dropdownOfKeywordColumns() {
		const dropdown = [
			{ text: 'HttpRquestBody (Like query)', value: 1 },
			{ text: 'HttpResponseText (Like query)', value: 2 },
			{ text: 'RequestUrl', value: 3 },

			{ text: '參考單號 (BetRecordNoPGS、不是UniqueID)', value: 7 },
			{ text: 'PlayerID', value: 11 },
			{ text: 'PlayerName', value: 12 },
		];

		return dropdown;
	}

	get dropdownOfStatusCodeFilter() {
		const dropdown = [
			{ text: 'Status 200', value: 200 },
			{ text: 'Status 200 以外', value: -200 },
		];

		return dropdown;
	}

	get dropdownCategory() {
		const dropdown = [
			{ text: '錢包餘額(wallet)', value: 'target/players/wallet' },
			{ text: '投注(PlaceBet)', value: 'target/bet/placeBet' },
			{ text: '結算(Settle)', value: 'target/bet/settle' },
			{ text: '取消(Cancel)', value: 'target/bet/cancel' },
			{ text: '獎勵派發(EventBonus)', value: 'target/transaction/event-bonus' },
			{ text: '轉入遊戲錢包(deposit)', value: 'target/transaction/deposit' },
			{ text: '轉出遊戲錢包(withdrawal)', value: 'target/transaction/withdrawal' },
		];

		return dropdown;
	}

	clear() {
		this.searchFilter = {} as Model.IExternalApiRequestLogPagingRequest;
	}

	search() {
		this.DataTablePagingParameter.pageIndex = 1;
		this.refresh();
	}

	whenChangeKeywordFilterType() {
		this.searchFilter.myInputKeyword = '';
	}

	refresh() {
		this.reloadComponent();

		const reqFilter = { ...this.searchFilter };
		const myFilterType = this.searchFilter.myInputKeywordFilterType;

		if (myFilterType === 1) {
			reqFilter.requestContentKeyword = this.searchFilter.myInputKeyword;
		} else if (myFilterType === 2) {
			reqFilter.responseTextKeyword = this.searchFilter.myInputKeyword;
		} else if (myFilterType === 3) {
			reqFilter.requestUrlKeyword = this.searchFilter.myInputKeyword;
		} else if (myFilterType === 7) {
			reqFilter.referenceTransactionNo = this.searchFilter.myInputKeyword;
		} else if (myFilterType === 4) {
			reqFilter.debugInfo2 = this.searchFilter.myInputKeyword;
		} else if (myFilterType === 11) {
			reqFilter.playerID = this.searchFilter.myInputKeyword;
		} else if (myFilterType === 12) {
			reqFilter.playerName = this.searchFilter.myInputKeyword;
		}

		reqFilter.createTimeBegin = this.datetimeRangePicker.searchStartTimeRange[0] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[0]) : null;
		reqFilter.createTimeEnd = this.datetimeRangePicker.searchStartTimeRange[1] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[1]) : null;
		reqFilter.topSize = 200;
		EventBus.$emit('show_mask', null);

		// 這邊的查詢不使用標準分頁查詢法
		LogApi.getSingleWalletPlatformRequestLogs(reqFilter)
			.then((res) => {
				this.ListData = res.data.items;
				this.DataTablePagingParameter.totalCount = res.data.totalCount;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	exportFiles() {
		if (this.ListData == null || this.ListData.length <= 0) {
			alert('請先進行搜尋');
			return;
		}

		const excelHeaders: any = ['CreateTime' + '(GMT+' + this.userProfile.timeZone.toString() + ')', 'MerchantID', 'ReqeustUrl', 'ReqeustBody', 'Response', 'StatusCode', 'Memo1', 'Memo2'];

		const excelData: any = [];
		EventBus.$emit('show_mask', null);

		this.ListData.forEach((value) => {
			const eachRow: any = [];

			eachRow.push(timeWithTimezone(value.createTime, this.userProfile.timeZone));
			eachRow.push(value.merchantID);
			eachRow.push(value.requestUrl);
			eachRow.push(value.requestContent);
			eachRow.push(value.responseText);
			eachRow.push(value.responseStatusCode);
			eachRow.push(value.debugInfo2);
			eachRow.push(value.debugInfo3);

			excelData.push(eachRow);
		});

		const datePrefix =
			'_' +
			toDateStringMMDDByDateText(this.searchFilter.createTimeBegin, this.userProfile.timeZone) +
			'_' +
			toDateStringMMDDByDateText(this.searchFilter.createTimeEnd, this.userProfile.timeZone);

		const config = {
			headers: excelHeaders,
			rowData: excelData,
			fileName: 'Export_SingleWalletCallbackLogs' + datePrefix + '.xlsx',
		};

		setTimeout(() => {
			EventBus.$emit('hide_mask', null);
		}, 1000);

		// @ts-ignore
		this.downloadExcel(config);
	}

	registerReloadComponentEvent() {
		const eventName = `reload-${this.$router.currentRoute.path}`;

		EventBus.$on(eventName, (info: any) => {
			// 需要稍微延遲下、不然會異常。
			// 重新設置查詢欄位
			setTimeout(() => {
				this.initial(true);
			}, 250);
		});
	}

	reloadComponent() {
		this.renderComponent = false;
		this.$nextTick(() => {
			this.renderComponent = true;
		});
	}

	async showInfo(item: Model.IExternalApiRequestLogResponse) {
		this.getSingleByUniqueKey(item);

		// const model: Model.IExternalApiRequestLogDetailInfo = {} as Model.IExternalApiRequestLogDetailInfo;
		// model.requestUrl = item.requestUrl;
		// model.requestContent = item.requestContent;
		// model.requestHeader = item.requestHeader;
		// model.responseText = item.responseText;
		// model.responseStatusCode = item.responseStatusCode;
		// model.createTime = item.requestStartTime;
		// model.responseTime = item.responseTime;
	}

	showSingleElasticsearchData(uniqueID: string) {
		const myFilter: any = {
			querySqlID: 900858,
			TableName: 'SinglewalletMerchantApiRequestLog',
			UniqueID: uniqueID,
		};

		const myConfig: any = {
			maxWidth: 1200,
		};

		EventBus.$emit(`globalShowCustomizedColumnsDisplayPopupDialog`, myFilter, myConfig);
	}

	showInfoPopup(myInfo: Model.IExternalApiRequestLogResponse) {
		this.$nextTick(() => {
			const targetRef: any = this.$refs.logDetailPopup;
			targetRef.showInfoPopup(myInfo);
		});
	}

	async getSingleByUniqueKey(item: Model.IExternalApiRequestLogResponse) {
		EventBus.$emit('show_mask', null);

		// 這邊的查詢不使用標準分頁查詢法
		LogApi.getUniqueSingleWalletPlatformRequestLogs(item.uniqueID, this.replaceToNoneIOS8601(item.createTime))
			.then((res) => {
				this.uniqueDetailInfo = res.data;
				this.showInfoPopup(this.uniqueDetailInfo);
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
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

	replaceToNoneIOS8601(iso8601: string) {
		return iso8601.replace('T', ' ').replace('Z', '');
	}

	close() {
		this.isShowInfoDialog = false;
	}

	save() {}

	setRequestIDByQueryString(queryStringValue: any) {
		if (queryStringValue && queryStringValue != null && queryStringValue.length > 0) {
			this.searchFilter.requestID = String(queryStringValue);
		}
	}

	setBeginTimeByQueryString(queryStringValue: any) {
		if (queryStringValue && queryStringValue != null && queryStringValue.length > 0) {
			const beginTS = dateStringToTimestamp(String(queryStringValue), 0);
			this.datetimeRangePicker.searchStartTimeRange[0] = beginTS - 1800 * 1000;
			this.datetimeRangePicker.searchStartTimeRange[1] = beginTS + 1800 * 1000;
		}
	}

	setReferenceTransactionNoByQueryString(queryStringValue: any) {
		if (queryStringValue && queryStringValue != null && queryStringValue.length > 0) {
			this.searchFilter.referenceTransactionNo = String(queryStringValue);
			this.searchFilter.myInputKeyword = String(queryStringValue);
			this.searchFilter.myInputKeywordFilterType = 7;
		}
	}

	setPlayerIDByQueryString(queryStringValue: any) {
		if (queryStringValue && queryStringValue != null && queryStringValue.length > 0) {
			this.searchFilter.myInputKeyword = String(queryStringValue);
			this.searchFilter.myInputKeywordFilterType = 11;
		}
	}

	setMerchantIDByQueryString(queryStringValue: any) {
		if (queryStringValue && queryStringValue != null && queryStringValue.length > 0) {
			this.searchFilter.merchantID = String(queryStringValue);
		}
	}

	initial(isTriggerSearch: boolean = false) {
		console.log('initial');
		console.log(this.$route.query);
		console.log(this.$route.query.requestID);

		this.setRequestIDByQueryString(this.$route.query.requestID);
		this.setBeginTimeByQueryString(this.$route.query.begin);
		this.setReferenceTransactionNoByQueryString(this.$route.query.referenceTransactionNo);
		this.setPlayerIDByQueryString(this.$route.query.playerID);
		this.setMerchantIDByQueryString(this.$route.query.merchantID);

		const vm: any = this;

		if (isTriggerSearch || !vm.isEmptyString(this.searchFilter.referenceTransactionNo)) {
			setTimeout(() => {
				this.search();
			}, 500);
		}
	}

	mounted() {
		const defaultBegin = Date.now() + -1 * 60 * 240 * 1000;
		this.datetimeRangePicker.searchStartTimeRange = [defaultBegin, daysAgo(1)];

		this.registerReloadComponentEvent();
		this.initial(false);

		DropdownApi.getSwLogsFilterTypes().then((res) => {
			this.dropdownItemsSpecialFilterTypes = res;
		});

		DropdownApi.getMerchants().then((res) => {
			this.dropdownMerchants = res;
		});
	}
}
</script>
