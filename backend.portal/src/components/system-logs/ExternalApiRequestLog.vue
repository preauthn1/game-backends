<template>
	<v-app v-if="renderComponent" class="app">
		<v-expansion-panels v-model="panel" multiple>
			<v-expansion-panel>
				<v-expansion-panel-content>
					<v-flex>
						<v-row class="search-form">
							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-text-field v-model="searchData.category" :label="$t('TableHeader.__category')" clearable> </v-text-field>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-text-field v-model="searchData.subCategory" :label="$t('TableHeader.__subCategory') + ' (GameVendorCode)'" clearable> </v-text-field>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete
										:items="dropdownOfKeywordColumns"
										clearable
										@change="whenChangeKeywordFilterType()"
										v-model="searchData.myInputKeywordFilterType"
										:label="'關鍵字Like查詢特定欄位(需搭配右欄)'"
									>
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
									<v-text-field v-model="searchData.merchantID" :label="'商戶ID (僅適用新版v2)'" clearable> </v-text-field>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-text-field v-model="searchData.responseStatusCode" :label="'StatusCode'" clearable> </v-text-field>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-text-field v-model="searchData.uniqueID" :label="'UniqueID (唯一32碼ID)'" clearable> </v-text-field>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete :label="'常用分類'" :items="dropdownCategory" @change="whenCategoryChange()" v-model="searchData.dropdownCategory" clearable> </v-autocomplete>
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
								<v-autocomplete :label="'特殊篩選'" :items="dropdownItemsSpecialFilterTypes" v-model="searchData.specialFilterType" clearable> </v-autocomplete>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="search-row-padding">
									<v-btn color="yellow darken-1" text class="btn search" @click.stop="search">
										<v-icon left>search</v-icon>
										{{ $t('Button.__search') }} (TOP 200筆) </v-btn
									>&#9;
									<v-btn color="pink lighten-3" text class="btn search" @click.stop="clear">
										<v-icon left>clear</v-icon>
										{{ $t('Button.__reset') }} </v-btn
									>&#9;
								</div>
							</v-col>
						</v-row>
					</v-flex>
				</v-expansion-panel-content>
			</v-expansion-panel>
		</v-expansion-panels>

		<v-data-table
			:headers="headers"
			id="table"
			:items="ListData"
			:page="DataTablePagingParameter.pageIndex"
			:items-per-page="DataTablePagingParameter.itemsPerPage"
			:server-items-length="DataTablePagingParameter.totalCount"
			:footer-props="DataTablePagingParameter.footerProps"
			@update:page="changePage"
			@update:items-per-page="changeItemPerPage"
			multi-sort
		>
			<template v-slot:item.action="{ item }">
				<v-icon small class="mr-2" style="color: #ccc; font-size: 20px" @click="showInfo(item)"> info </v-icon>
			</template>

			<template v-slot:item.uniqueID="{ item }">
				<span class="f14 link-text-white" @click="showSingleElasticsearchData(item.uniqueID)">
					{{ item.uniqueID }}
				</span>
			</template>

			<template v-slot:item.responseTime="{ item }">
				<span :class="item.responseTime > 5000 ? 'minus-red' : ''">
					{{ (item.responseTime / 1000).toFixed(1) }}
				</span>
			</template>

			<template v-slot:item.requestUrl="{ item }">
				<div style="display: inline-block; width: 450px; text-overflow: ellipsis; overflow: hidden">
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
import { pageData, time } from '@/utilities/api-handler.lib';
// import Validation from '@/utilities/validation';
import { datetimeMixin, startEndDate, daysAgo } from '@/utilities/datetime-format';
import DatePicker from 'vue2-datepicker';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import { commonMethodMixin } from '@/utilities/common-methods';
import { State, Action, Getter, namespace } from 'vuex-class';

const authModule = namespace('Auth');

import * as Permission from '@/models/status/permission';

// @ts-ignore
@Component({ components: { NotificationWarning, ExternalApiRequestLogDetailPopup, DatePicker, PopupCopyDatetime }, mixins: [commonDisplayFiltersMixin, commonMethodMixin, datetimeMixin] })
export default class ExternalApiRequestLog extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];
	renderComponent: boolean = true;
	isShowInfoDialog: boolean = false;
	panel: number[] = [0];

	datetimeRangePicker: DropdownModel.DatetimeRangePicker = new DropdownModel.DatetimeRangePicker();
	searchData: Model.IExternalApiRequestLogPagingRequest = {} as Model.IExternalApiRequestLogPagingRequest;
	ListData: Model.IExternalApiRequestLogResponse[] = [];
	Settings: SettingModel.ISystemConfigSetting = {} as SettingModel.ISystemConfigSetting;
	deleteID: number = 0;

	dropdownItemsSpecialFilterTypes: DropdownModel.IDropdownItems[] = [];
	DataTablePagingParameter: DropdownModel.DataTablePagingParameter = new DropdownModel.DataTablePagingParameter();
	editModeOpenGroups: number[] = [];

	get headers() {
		const data = [
			{ text: this.$t('TableHeader.__ID'), value: 'uniqueID', sortable: false, align: 'start' },
			{ text: this.$t('TableHeader.__createTime2'), value: 'createTime', sortable: false },
			{ text: this.$t('TableHeader.__category'), value: 'category', sortable: false },
			{ text: this.$t('TableHeader.__subCategory'), value: 'subCategory', sortable: false },
			{ text: '資訊1', value: 'debugInfo', sortable: false },
			{ text: '資訊2', value: 'debugInfo2', sortable: false },
			{ text: '請求網址', value: 'requestUrl', sortable: false },
			{ text: '回應時間', value: 'responseTime', sortable: false, align: 'end' },
			{ text: 'StatusCode', value: 'responseStatusCode', sortable: false, align: 'end' },
			{ text: this.$t('TableHeader.__memo'), value: 'memo', sortable: false },
			{ text: 'webSiteName', value: 'webSiteName', sortable: false },
		];

		if (true) {
			data.splice(0, 0, { text: this.$t('TableHeader.__action'), value: 'action', sortable: false, align: 'center' });
		}

		return data;
	}

	get dropdownCategory() {
		const dropdown = [
			{ text: 'GetGameLaunchUrl', value: 'GetGameLaunchUrl' },
			{ text: 'Freespin_Set', value: 'Freespin_Set' },
			{ text: 'GetReport', value: 'GetReport' },
		];

		return dropdown;
	}

	get dropdownOfKeywordColumns() {
		const dropdown = [
			{ text: 'HttpRquestBody (Like query)', value: 1 },
			{ text: 'HttpResponseText (Like query)', value: 2 },
			{ text: 'RequestUrl網址', value: 3 },
			{ text: '參考單號 (不是Unique PK)', value: 4 },
			{ text: 'PlayerID', value: 11 },
		];

		return dropdown;
	}

	clear() {
		this.searchData = {} as Model.IExternalApiRequestLogPagingRequest;
	}

	whenCategoryChange() {
		this.searchData.category = this.searchData.dropdownCategory ?? '';
	}

	search() {
		this.DataTablePagingParameter.pageIndex = 1;
		this.refresh();
	}

	refresh() {
		this.registerReloadComponent();

		const reqFilter = { ...this.searchData };
		const myFilterType = this.searchData.myInputKeywordFilterType;

		if (myFilterType === 1) {
			reqFilter.requestContentKeyword = this.searchData.myInputKeyword;
		} else if (myFilterType === 2) {
			reqFilter.responseTextKeyword = this.searchData.myInputKeyword;
		} else if (myFilterType === 3) {
			reqFilter.requestUrlKeyword = this.searchData.myInputKeyword;
		} else if (myFilterType === 4) {
			reqFilter.referOrderNo = this.searchData.myInputKeyword;
		} else if (myFilterType === 11) {
			reqFilter.playerID = this.searchData.myInputKeyword;
		}

		reqFilter.pageIndex = this.DataTablePagingParameter.pageIndex;
		reqFilter.pageSize = this.DataTablePagingParameter.itemsPerPage;
		reqFilter.createTimeBegin = this.datetimeRangePicker.searchStartTimeRange[0] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[0]) : null;
		reqFilter.createTimeEnd = this.datetimeRangePicker.searchStartTimeRange[1] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[1]) : null;

		EventBus.$emit('show_mask', null);

		LogApi.getLogs(reqFilter)
			.then((res) => {
				this.ListData = res.data.items;
				this.DataTablePagingParameter.totalCount = res.data.totalCount;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	whenChangeKeywordFilterType() {
		this.searchData.myInputKeyword = '';
	}

	registerReloadComponent() {
		EventBus.$on(`reload-${this.$router.currentRoute.path}`, (info: any) => {
			this.renderComponent = false;
			this.$nextTick(() => {
				this.renderComponent = true;
			});
		});
	}

	convertToDetailModel(item: Model.IExternalApiRequestLogResponse) {
		const model: Model.IExternalApiRequestLogDetailInfo = {} as Model.IExternalApiRequestLogDetailInfo;
		model.requestUrl = item.requestUrl;
		model.requestContent = item.requestContent;
		model.requestHeader = item.requestHeader;
		model.responseText = item.responseText;
		model.responseStatusCode = item.responseStatusCode;
		model.createTime = item.createTime;
		model.responseTime = item.responseTime;

		return model;
	}

	showInfo(item: Model.IExternalApiRequestLogResponse) {
		const model = this.convertToDetailModel(item);

		const isQueryByUniqueID = item.uniqueID != null && item.uniqueID.length > 5;

		if (!isQueryByUniqueID) {
			this.$nextTick(() => {
				const targetRef: any = this.$refs.logDetailPopup;
				targetRef.showInfoPopup(model);
			});

			return;
		}
		// 若有uniqueID
		EventBus.$emit('show_mask', null);

		LogApi.getThirdLogByUniqueID(item.uniqueID)
			.then((res) => {
				const model2 = this.convertToDetailModel(res.data);
				const targetRef: any = this.$refs.logDetailPopup;
				targetRef.showInfoPopup(model2);
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	showSingleElasticsearchData(uniqueID: string) {
		const myFilter: any = {
			querySqlID: 900858,
			TableName: 'ExternalApiRequestlog',
			UniqueID: uniqueID,
		};

		const myConfig: any = {
			maxWidth: 1200,
		};

		EventBus.$emit(`globalShowCustomizedColumnsDisplayPopupDialog`, myFilter, myConfig);
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

		DropdownApi.getExtraFilterType(8851).then((res) => {
			this.dropdownItemsSpecialFilterTypes = res;
		});
	}
}
</script>
