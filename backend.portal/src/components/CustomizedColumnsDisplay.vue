<template>

	<v-app v-if="isShowInfo" class="">

		<!-- 簡易查詢 -->
		<v-expansion-panels v-model="panel" multiple>
			<v-expansion-panel>

				<v-expansion-panel-content>

					<v-flex>

						<v-row v-if="!isSearchTypeSelected()" class="mt-2">
							<v-icon color="yellow lighten-2" small class="mr-1">error</v-icon> <span
								class="yellow-text">請先選擇要進行哪一項查詢</span>
						</v-row>

						<v-row v-if="isSearchTypeSelected()">

							<v-col cols="12" sm="8" md="8">
								<div class="item">
									<date-picker v-model="datetimeRangePicker.searchStartTimeRange"
										:disabled="formFilterDisabled('beginTime')" type="datetime"
										value-type="timestamp" range :placeholder="$t('TableHeader.__time')">
									</date-picker>

									<v-chip class="mr-1"
										:class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'today' }"
										@click="globalQuicklyPickTimeRange('today')">
										<v-icon left>mdi-alarm-check</v-icon>
										{{ $t(`TableHeader.__today`) }}
									</v-chip>
									<v-chip class="mr-1"
										:class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'yesterday' }"
										@click="globalQuicklyPickTimeRange('yesterday')">
										<v-icon left>mdi-alarm-check</v-icon>
										{{ $t(`TableHeader.__yesterday`) }}
									</v-chip>
									<v-chip class="mr-1"
										:class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'thisWeek' }"
										@click="globalQuicklyPickTimeRange('thisWeek')">
										<v-icon left>mdi-alarm-check</v-icon>
										{{ $t(`TableHeader.__thisWeek`) }}
									</v-chip>

									<v-chip class="mr-1"
										:class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'lastWeek' }"
										@click="globalQuicklyPickTimeRange('lastWeek')">
										<v-icon left>mdi-alarm-check</v-icon>
										{{ $t(`TableHeader.__lastWeek`) }}
									</v-chip>
									<v-chip class="mr-1"
										:class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'thisMonth' }"
										@click="globalQuicklyPickTimeRange('thisMonth')">
										<v-icon left>mdi-alarm-check</v-icon>
										{{ $t(`TableHeader.__thisMonth`) }}
									</v-chip>
									<v-chip class="mr-1"
										:class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'lastMonth' }"
										@click="globalQuicklyPickTimeRange('lastMonth')">
										<v-icon left>mdi-alarm-check</v-icon>
										{{ $t(`TableHeader.__lastMonth`) }}
									</v-chip>
								</div>

							</v-col>

							<v-col cols="12" sm="4" md="4">

								<div class="item buttonRow" style="text-align: right;">

									<v-btn color="yellow darken-1" text class="btn search"
										@click.stop="pageOneSearch">
										<v-icon left>search</v-icon>
										{{ $t('Button.__search') }}
									</v-btn>&#9;

									<v-btn color="pink lighten-3" text class="btn search" @click.stop="clear">
										<v-icon left>clear</v-icon>
										{{ $t('Button.__reset') }}
									</v-btn>&#9;

									<v-btn text color="teal lighten-2" v-clipboard:copy="copyText"
										v-clipboard:success="copySuccessMsg" class="btn">
										<v-icon left>file_copy</v-icon>
										複製文字(Excel排版)
									</v-btn>

									<v-btn text color="blue lighten-2" @click="exportExcel()" class="btn ml-1">
										<v-icon left>cloud_download</v-icon>
										匯出Excel檔案
									</v-btn>

								</div>
							</v-col>

						</v-row>

					</v-flex>

					<!-- 各個篩選欄位 -->
					<v-flex>
						<v-row>
							<v-col cols="12" sm="3" md="3" v-if="!formFilterDisabled('playerID')">
								<div class="item">
									<v-text-field v-model="searchFilter.playerID"
										:label="$t('TableHeader.__playerID')" append-icon="perm_identity" clearable>
									</v-text-field>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3" v-if="!formFilterDisabled('playerName')">
								<div class="item">
									<v-text-field v-model="searchFilter.playerName"
										:label="$t('TableHeader.__playerAccount')" append-icon="person" clearable>
									</v-text-field>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3" v-if="!formFilterDisabled('merchantID')">
								<div class="item">
									<v-autocomplete clearable :items="merchantDropdownItems" class=""
										v-model="searchFilter.merchantID" :label="$t('TableHeader.__merchant')">
									</v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3" v-if="!formFilterDisabled('hierarchyMerchantID')">
								<div class="item">
									<v-autocomplete clearable :items="hierarchyMerchantDropdownItems" class=""
										v-model="searchFilter.merchantID"
										:label="$t('TableHeader.__merchant') + '/' + $t('TableHeader.__agent')">
									</v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3" v-if="!formFilterDisabled('gameID')">
								<div class="item">
									<v-autocomplete clearable :items="gamesDropdownItems"
										v-model="searchFilter.gameID" :label="$t('TableHeader.__gameName')">
									</v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3" v-if="!formFilterDisabled('gameVendor')">
								<div class="item">
									<v-autocomplete clearable :items="gameVendorDropdownItems"
										v-model="searchFilter.gameVendor" :label="$t('TableHeader.__gameVendorType')">
									</v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3" v-if="!formFilterDisabled('vendorConfig')">
								<div class="item">
									<v-autocomplete clearable :items="vendorConfigDropdownItems"
										v-model="searchFilter.vendorConfigID" :label="$t('TableHeader.__vendorConfig')">
									</v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3" v-if="!formFilterDisabled('imChannelID')">
								<div class="item">
									<v-autocomplete clearable :items="imChannelDropdownItems"
										v-model="searchFilter.imChannelID" :label="$t('TableHeader.__imChannelName')">
									</v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3" v-if="!formFilterDisabled('currency')">
								<div class="item">
									<v-autocomplete clearable :items="currencyDropdownItems"
										v-model="searchFilter.currency" :label="$t('TableHeader.__currency')">

										<template v-slot:item="data">
											<div class="letter-size-equal">
												<span>{{data.item.text}}</span>
											</div>
										</template>

									</v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3" v-if="!formFilterDisabled('eventID')">
								<div class="item">
									<v-autocomplete clearable :items="eventsDropdownItems"
										v-model="searchFilter.eventID" :label="$t('TableHeader.__eventID')">
									</v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3" v-if="!formFilterDisabled('sortByColumn')">
								<div class="item">
									<v-autocomplete :items="sortAmountColumnsDropdownItems"
										v-model="searchFilter.sortByColumn" :label="'常用排序欄位'">
									</v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3" v-if="!formFilterDisabled('sortDirection')">
								<div class="item">
									<v-autocomplete :items="sortDirectionDropdownItems"
										v-model="searchFilter.sortDirection" :label="'排序方向'">
									</v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3" v-if="!formFilterDisabled('text')">
								<div class="item">
									<v-text-field v-model="searchFilter.text" :label="'文字栏位比对(e.g. 关键字、单号)'"
										append-icon="person" clearable>
									</v-text-field>
								</div>
							</v-col>

						</v-row>
					</v-flex>

					<v-flex style="margin-top:20px;">

						<v-row v-for='(item , index) in queryDynamicQueryCategoryDropdown.eachCategory'
							:key="index">

							<v-col cols="12" sm="1" md="1" class="no-margin-padding">

								<v-chip>
									{{item.category}}
								</v-chip>

							</v-col>

							<v-col cols="12" sm="11" md="11" class="no-margin-padding">

								<span class='link-sm link-text-grey mt-1'
									v-for="(dropdownItem , myIndex) in item.items"
									@click="changeSearchType(dropdownItem.id)" :key="myIndex">

									{{dropdownItem.displayName}}

								</span>

								<!-- <div style="text-align: right;">
									<div class='link-sm link-text-grey' style="margin-top: 8px;"
										v-for='(item , index) in dynamicQueryDropdownItems' :key="index"
										@click="changeSearchType(item.value)">
										{{item.text}}
									</div>
								</div> -->

							</v-col>

						</v-row>

						<!-- 
						<v-row >
							<v-col cols="12" sm="3" md="3">
							</v-col>

							<v-col cols="12" sm="9" md="9">

								<div style="text-align: right;">
									<div class='link-sm link-text-grey' style="margin-top: 8px;"
										v-for='(item , index) in dynamicQueryDropdownItems' :key="index"
										@click="changeSearchType(item.value)">
										{{item.text}}
									</div>
								</div>

							</v-col>

						</v-row> 
-->

					</v-flex>
				</v-expansion-panel-content>

			</v-expansion-panel>
		</v-expansion-panels>

		<div class="hintRow">

			<v-chip color="blue darken-3" class="mr-2" style="vertical-align: bottom;"
				:title=" this.searchDynamicQuerySetting.id"
				v-if="this.searchDynamicQuerySetting.displayName && this.searchDynamicQuerySetting.displayName.length > 0">
				查询类型：{{ this.searchDynamicQuerySetting.displayName }}
			</v-chip>

			<div>
				<v-icon>
					error
				</v-icon>
				时间区间不要拉太长
			</div>

			<div>
				<v-icon>
					error
				</v-icon>
				部分筛选栏位要求必填(效能考量)
			</div>
			<div>
				<v-icon>
					error
				</v-icon>
				部分查詢會有查詢筆數限制
			</div>

		</div>

		<v-data-table v-if="isShowTable" :headers="headers" id="table" :items="searchItems"
			:items-per-page="pagingItemsPerPage" :footer-props="pagingFooterProps">

			<template v-slot:item.GameTime="{ item }">

				<span @click="copyDuration_5Minutes(item.GameTime)" class='copyTextTo' title='copy'>
					{{item.GameTime}}
				</span>
			</template>

			<template v-slot:item.MerchantProfit="{ item }">

				<span :class="item.MerchantProfit < 0 ? 'minus-red': ''">
					{{item.MerchantProfit | amountDisplay}}
				</span>

			</template>

			<template v-slot:item.totalRound="{ item }">
				{{item.totalRound | countDisplay}}
			</template>

			<template v-slot:item.TotalRound="{ item }">
				{{item.TotalRound | countDisplay}}
			</template>

			<template v-slot:item.WinLossAmount="{ item }">

				<span :class="isMinusValue(item.WinLossAmount) ? 'minus-red': ''">
					{{item.WinLossAmount}}
				</span>

			</template>

			<template v-slot:item.WinLoss="{ item }">

				<span :class="isMinusValue(item.WinLoss) ? 'minus-red': ''">
					{{item.WinLoss}}
				</span>

			</template>

			<template v-slot:item.RTP="{ item }">

				<span :class="item.RTP >= 100 ? 'h-yellow': ''">
					{{item.RTP}}
				</span>

			</template>

			<template v-slot:item.UpdateAmount="{ item }">

				<span :class="isMinusValue(item.UpdateAmount) ? 'minus-red': ''">
					{{item.UpdateAmount}}
				</span>

			</template>

		</v-data-table>

	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import EventBus from '@/utilities/event-bus';
import { Component, Prop } from 'vue-property-decorator';
import { State, Action, Getter, namespace } from 'vuex-class';
import { Locale } from '@/models/status/common';
import DropdownApi from '@/api/commons/dropdown';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import DatePicker from 'vue2-datepicker';
import * as ResponseModel from '@/models/interfaces/commons/response';
import { datetimeMixin, startEndDate, timeWithTimezone, daysAgo, weeksAgo, monthsAgo, formatMixin, addMinutes } from '@/utilities/datetime-format';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { commonMethodMixin } from '@/utilities/common-methods';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import internalTestApi from '@/api/commons/internalTest';
import * as Permission from '@/models/status/permission';

import Cookies from 'js-cookie';
const authModule = namespace('Auth');

// @ts-ignore
@Component({
	// @ts-ignore
	mixins: [datetimeMixin, commonDisplayFiltersMixin, formatMixin, commonMethodMixin],
	components: { DatePicker },
})
export default class CustomizedColumnsDisplay extends Vue {
	@Prop(String) readonly defaultLang!: string;

	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;

	isShowInfo: boolean = true;
	isShowTable: boolean = false;
	headers: any = null;
	copyText: string = '';

	pagingFooterProps: any = { 'items-per-page-options': [10, 30, 50, 100] };
	pagingItemsPerPage: number = 10;

	availableForms: string[] = [];
	datetimeRangePicker: DropdownModel.DatetimeRangePicker = new DropdownModel.DatetimeRangePicker();
	dataTablePagingParameter: DropdownModel.DataTablePagingParameter = new DropdownModel.DataTablePagingParameter();
	searchDynamicQuerySetting: ResponseModel.IDynamicQueryCommandSettingResult = {} as ResponseModel.IDynamicQueryCommandSettingResult;
	queryDynamicQueryCategoryDropdown: ResponseModel.IDynamicQueryCategoryDropdowns = {} as ResponseModel.IDynamicQueryCategoryDropdowns;

	searchFilter: ResponseModel.ICustomizedColumnRequest = {} as ResponseModel.ICustomizedColumnRequest;
	searchItems: any = null; // 格式是動態的
	searchExtraInfo: any = null;
	panel: number[] = [0];

	dynamicQueryDropdownItems: DropdownModel.IDropdownItems[] = [];
	gamesDropdownItems: DropdownModel.IDropdownItems[] = [];
	merchantDropdownItems: DropdownModel.IDropdownItems[] = [];
	gameVendorDropdownItems: DropdownModel.IDropdownItems[] = [];
	currencyDropdownItems: DropdownModel.IDropdownItems[] = [];
	vendorConfigDropdownItems: DropdownModel.IDropdownItems[] = [];
	imChannelDropdownItems: DropdownModel.IDropdownItems[] = [];
	eventsDropdownItems: DropdownModel.IDropdownItems[] = [];
	hierarchyMerchantDropdownItems: DropdownModel.IDropdownItems[] = [];

	sortDirectionDropdownItems: DropdownModel.IDropdownItems[] = [];
	sortAmountColumnsDropdownItems: DropdownModel.IDropdownItems[] = [];

	get hasSuperAdminPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSetting_SystemConfigSetting) >= 0;
	}

	created() {
		this.searchFilter = {} as ResponseModel.ICustomizedColumnRequest;
	}

	copyDuration_5Minutes(time: string) {
		const vm: any = this;

		const beforeMinutes5 = addMinutes(time, -5, this.userProfile.timeZone);
		const afterMinutes5 = addMinutes(time, 5, this.userProfile.timeZone);

		vm.copyTextToClipboard(beforeMinutes5 + ' ~ ' + afterMinutes5);
	}

	pageOneSearch() {
		this.refresh();
	}

	isMinusValue(amountDisplayText: string) {
		try {
			const amount = amountDisplayText.replace(',', '');
			return parseInt(amount, 10) < 0;
		} catch (error) {}

		return false;
	}

	isSearchTypeSelected() {
		return this.searchFilter.querySqlID && this.searchFilter.querySqlID != null && this.searchFilter.querySqlID !== 0;
	}

	changeSearchType(type: number) {
		this.searchFilter.querySqlID = type;
		this.getDynamicQuerySetting();

		this.isShowTable = false;
		this.searchItems = [];
		this.copyText = '';

		// this.refresh();
	}

	clear() {
		const queryID = this.searchFilter.querySqlID;
		this.searchFilter = {} as ResponseModel.ICustomizedColumnRequest;
		this.searchFilter.querySqlID = queryID;

		const vm: any = this;
		vm.globalClear();

		// 順序不要亂改
		this.init();
	}

	refresh() {
		EventBus.$emit('show_mask', null);

		const timePicker = this.datetimeRangePicker;
		this.searchFilter.beginTime = timePicker.searchStartTimeRange[0] ? startEndDate(timePicker.searchStartTimeRange[0]) : null;
		this.searchFilter.endTime = timePicker.searchStartTimeRange[1] ? startEndDate(timePicker.searchStartTimeRange[1]) : null;

		if (this.searchDynamicQuerySetting.extraSetting && this.searchDynamicQuerySetting.extraSetting.isCsvFormat) {
			internalTestApi
				.downloadCsvByDynamicQuery(this.searchFilter)
				.then((res) => {})
				.finally(() => {
					setTimeout(() => {
						EventBus.$emit('hide_mask', null);
					}, 500);
				});
		} else {
			internalTestApi
				.getCustomizedColumns(this.searchFilter)
				.then((res) => {
					this.isShowTable = true;
					this.searchItems = res.data.items;
					this.searchExtraInfo = res.data.extraObject;
					this.copyText = res.data.extraObject.copyExcel;
					this.headers = res.data.extraObject.columns.map((item: any) => {
						const alignValue = this.getAlignByColumn(item.columnName);
						return { text: item.columnName, value: item.columnName, sortable: false, align: alignValue };
					});
				})
				.finally(() => {
					EventBus.$emit('hide_mask', null);
				});
		}
	}

	getAlignByColumn(columnName: string) {
		if (columnName.toLowerCase().includes('amount') || columnName.toLowerCase().includes('rtp') || columnName.toLowerCase().includes('profit')) {
			return 'end';
		}

		return 'start';
	}

	formFilterDisabled(formName: string) {
		const isDisable = true;

		if (!this.searchDynamicQuerySetting) {
			return isDisable;
		}

		const availableForms: string[] = this.searchDynamicQuerySetting.availableFilterFormItems;

		if (!availableForms) {
			return isDisable;
		}

		const isAvailable = availableForms.includes(formName);
		return !isAvailable;
	}

	getDynamicQuerySetting() {
		internalTestApi.getDynamicQuerySetting(this.searchFilter.querySqlID).then((res) => {
			this.searchDynamicQuerySetting = res.data;
			this.availableForms = res.data.availableFilterFormItems;
			this.pagingItemsPerPage = res.data.defaultPageSize;
		});
	}

	init() {
		this.datetimeRangePicker.searchStartTimeRange = [daysAgo(-1), daysAgo(1)];
	}

	exportExcel() {
		if (this.searchItems == null || this.searchItems.length <= 0) {
			alert('請先查詢至少一筆結果');
			return;
		}

		const excelHeaders: any = [];

		EventBus.$emit('show_mask', null);

		for (const item of this.headers) {
			excelHeaders.push(item.text);
		}

		const excelData: any = [];

		for (const item of this.searchItems) {
			const eachRow: any = [];

			for (const eachColumn of this.headers) {
				eachRow.push(item[eachColumn.text]);
			}

			excelData.push(eachRow);
		}

		const config = {
			headers: excelHeaders,
			rowData: excelData,
			fileName: 'Export_EazySearchExport_' + this.searchDynamicQuerySetting.displayName + '.xlsx',
		};

		// @ts-ignore
		this.downloadExcel(config);

		setTimeout(() => {
			EventBus.$emit('hide_mask', null);
		}, 500);
	}

	mounted() {
		const urlParams = new URLSearchParams(window.location.search);
		const myParam = urlParams.get('querySqlID');

		if (!myParam || myParam == null || myParam === '') {
			// myParam = '9011'; // 預設值
			// this.isShowInfo = false;
		} else {
			this.searchFilter.querySqlID = Number(myParam);
			this.getDynamicQuerySetting();
		}

		DropdownApi.getMerchants().then((res) => {
			this.merchantDropdownItems = res;
		});

		DropdownApi.getDynamicQueryItems().then((res) => {
			this.dynamicQueryDropdownItems = res;
		});

		internalTestApi.getDynamicQuerySelectDropdowns().then((res) => {
			this.queryDynamicQueryCategoryDropdown = res.data;
		});

		DropdownApi.getGamesOriginal().then((res) => {
			this.gamesDropdownItems = res;
		});

		DropdownApi.getGameVendors().then((res) => {
			this.gameVendorDropdownItems = res;
		});

		DropdownApi.getCurrency().then((res) => {
			this.currencyDropdownItems = res;
		});

		DropdownApi.getEvents().then((res) => {
			this.eventsDropdownItems = res;
		});

		DropdownApi.getVendorConfig().then((res) => {
			this.vendorConfigDropdownItems = res;
		});

		DropdownApi.getSortDirection().then((res) => {
			this.sortDirectionDropdownItems = res;
			this.searchFilter.sortDirection = res[0].valueInt;
		});

		DropdownApi.getHierarchyMerchants().then((res) => {
			this.hierarchyMerchantDropdownItems = res;
		});

		DropdownApi.getSortAmountColumns().then((res) => {
			this.sortAmountColumnsDropdownItems = res;
			this.searchFilter.sortByColumn = res[0].value.toString();
		});

		DropdownApi.getIMChannels().then((res) => {
			this.imChannelDropdownItems = res;
		});

		this.init();
	}
}
</script>
<style lang="scss" scope>
span.white--text.headline {
	font-family: 微軟正黑體, arial !important;
}
.country {
	background-color: transparent !important;
	box-shadow: none !important;
}
.lang {
	white-space: nowrap !important;
}

table th {
	user-select: initial !important;
}

.col-sm-3 .buttonRow,
.col-md-3 .buttonRow {
	margin-top: 15px;
}

.hintRow,
.hintRow .v-icon {
	padding: 3px 0px;
	font-size: 14px;
	color: #aaa;
}

.hintRow > div {
	display: inline-block;
	margin-right: 15px;
}

.hintRow .v-icon {
	color: #aaa;
	font-size: 20px;
}

.link-sm {
	font-size: 16px;
	margin-right: 20px;
	display: inline-block;
}

.no-margin-padding {
	padding: 3px;
	margin: 0px;
}
</style>
