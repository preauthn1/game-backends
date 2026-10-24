<template>
	<v-app v-if="renderComponent" class="app">
		<v-expansion-panels v-model="panel" multiple>
			<v-expansion-panel>
				<v-expansion-panel-content>
					<v-flex>
						<v-row>
							<v-col cols="12" sm="3" md="3">
								<div class="">
									<v-autocomplete
										chips
										clearable
										:items="dropdownMerchants"
										v-model="searchFilter.merchantID"
										class="merchantDropdownItems"
										:label="$t('TableHeader.__merchant') + ' (' + $t('TableHeader.__agent') + ')'"
									>
									</v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="">
									<v-autocomplete chips clearable multiple :items="dropdownGames" v-model="searchFilter.gameIDs" menu-props="auto" class="" :label="$t('TableHeader.__game')">
									</v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="">
									<v-autocomplete chips clearable :items="timeZones" v-model="searchFilter.timeZone" menu-props="auto" class="" :label="$t('TableHeader.__timeZoneArea')">
									</v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<!-- <div class="item right"> -->
								<div class="btn-row">
									<v-btn color="yellow darken-1" text class="btn search" @click.stop="refresh">
										<v-icon left>search</v-icon>
										{{ $t('Button.__search') }} </v-btn
									>&#9;
									<v-btn color="pink lighten-3" text class="btn search" @click.stop="clear">
										<v-icon left>clear</v-icon>
										{{ $t('Button.__reset') }}
									</v-btn>
									<v-btn color="light-blue lighten-3" text class="btn search" @click.stop="exportFiles">
										<v-icon left>cloud_download</v-icon>
										{{ $t('Button.__downloadFile') }} ({{ $t('TableHeader.__currentPageOnly') }}) </v-btn
									>&#9;
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="noMargin">
									<date-picker
										v-model="datetimeRangePicker.searchStartTimeRange"
										type="datetime"
										class="force-w100"
										value-type="timestamp"
										range
										:placeholder="$t('TableHeader.__time')"
									>
									</date-picker>
								</div>
							</v-col>

							<v-col cols="12" sm="6" md="6" class="quick-time">
								<div class="">
									<v-chip class="mr-2" :class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'today' }" @click="globalQuicklyPickTimeRange('today')">
										<v-icon left small>mdi-alarm-check</v-icon>
										{{ $t(`TableHeader.__today`) }}
									</v-chip>
									<v-chip class="mr-2" :class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'yesterday' }" @click="globalQuicklyPickTimeRange('yesterday')">
										<v-icon left small>mdi-alarm-check</v-icon>
										{{ $t(`TableHeader.__yesterday`) }}
									</v-chip>
									<v-chip class="mr-2" :class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'thisWeek' }" @click="globalQuicklyPickTimeRange('thisWeek')">
										<v-icon left small>mdi-alarm-check</v-icon>
										{{ $t(`TableHeader.__thisWeek`) }}
									</v-chip>

									<v-chip class="mr-2" :class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'lastWeek' }" @click="globalQuicklyPickTimeRange('lastWeek')">
										<v-icon left small>mdi-alarm-check</v-icon>
										{{ $t(`TableHeader.__lastWeek`) }}
									</v-chip>
									<v-chip class="mr-2" :class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'thisMonth' }" @click="globalQuicklyPickTimeRange('thisMonth')">
										<v-icon left small>mdi-alarm-check</v-icon>
										{{ $t(`TableHeader.__thisMonth`) }}
									</v-chip>
									<v-chip class="mr-2" :class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'lastMonth' }" @click="globalQuicklyPickTimeRange('lastMonth')">
										<v-icon left small>mdi-alarm-check</v-icon>
										{{ $t(`TableHeader.__lastMonth`) }}
									</v-chip>
								</div>
							</v-col>
						</v-row>
					</v-flex>
				</v-expansion-panel-content>
			</v-expansion-panel>
		</v-expansion-panels>

		<div>
			<div class="hint-row">
				<v-icon> error </v-icon>
				{{ $t('Message.__dataDelay30Minutes') }}

				<v-icon class="ml-3"> visibility </v-icon>
				{{ $t('TableHeader.__bankViewPoint') }}

				<v-icon class="ml-3"> visibility </v-icon>
				{{ $t('Message.__searchResultWillIncludeSubAgents') }}
			</div>
		</div>

		<v-data-table
			:items="pageItems"
			:headers="headers"
			id="table"
			:disable-sort="true"
			:page="DataTablePagingParameter.pageIndex"
			:items-per-page="DataTablePagingParameter.itemsPerPage"
			:server-items-length="DataTablePagingParameter.totalCount"
			:footer-props="footerProps"
			@update:page="changePage"
			@update:items-per-page="getItemPerPage"
		>
			<template v-slot:item.totalRound="{ item }">
				{{ item.totalRound | countDisplay }}
			</template>

			<template v-slot:item.totalPlayers="{ item }">
				{{ item.totalPlayers | countDisplay }}
			</template>

			<template v-slot:item.betAmount="{ item }">{{ item.betAmount | amountDisplay }}</template>
			<template v-slot:item.validAmount="{ item }">{{ item.validAmount | amountDisplay }}</template>
			<template v-slot:item.payoutAmount="{ item }">{{ item.payoutAmount | amountDisplay }}</template>

			<template v-slot:item.merchantWinLossAmount="{ item }">
				<span :class="item.merchantWinLossAmount < 0 ? 'minus-red ' : ''">{{ item.merchantWinLossAmount | amountDisplay }}</span>
			</template>

			<template v-slot:item.gameID="{ item }">
				<div>{{ item.gameName }}</div>
				<div class="sub-title-text">{{ item.gameID }}</div>
			</template>

			<template v-slot:item.merchantID="{ item }">
				<div>{{ item.merchantName }}</div>
				<div class="sub-title-text">{{ item.merchantID }}</div>
			</template>

			<template v-slot:item.rtp="{ item }">
				<span :class="item.rtp > 100 ? 'h-yellow  ' : ''">
					{{ item.rtp | amountDisplay }}
				</span>
			</template>

			<template v-slot:item.profitRate="{ item }">
				<span :class="item.profitRate < 0 ? 'minus-red ' : ''">
					{{ item.profitRate | amountDisplay }}
				</span>
			</template>

			<template v-slot:item.action="{ item }">
				<span class="link-text" @click="showChildAgentSingleDateSummary(item)">
					{{ $t('Button.__showEachAgentCustomerDetail') }}
				</span>
			</template>

			<!-- <template slot="body.append" v-if="searchResult.totalSummary && dailyReports.length > 0">
				<tr>
					<td>
						{{ $t('TableHeader.__total') }}
						:
					</td>
					<td></td>
					<td class="text-r">
						{{ searchResult.totalSummary.totalRound | countDisplay }}
					</td>

					<td class="text-r" v-if="hasPermission_IsBetAmount_visible">
						{{ searchResult.totalSummary.betAmount | amountDisplay }}
					</td>
					<td class="text-r" v-if="hasPermission_IsBetAmount_visible">
						{{ searchResult.totalSummary.validAmount | amountDisplay }}
					</td>
					<td class="text-r" v-if="hasPermission_IsBetAmount_visible">
						{{ searchResult.totalSummary.payoutAmount | amountDisplay }}
					</td>
					<td class="text-r">
						{{ searchResult.totalSummary.merchantWinLossAmount | amountDisplay }}
					</td>
					<td class="text-r" v-if="hasPermission_IsRTP_visible">
						{{ searchResult.totalSummary.rtp | amountDisplay }}
					</td>
				</tr>
			</template> -->
		</v-data-table>

		<v-dialog v-model="dialogVisible.isShowTreeInfo" max-width="800px">
			<v-card>
				<v-card-title class="grey darken-2">
					{{ $t('Menu.Agent_MerchantHierarchyTreeView') }}

					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>

				<v-card-text>
					<div style="height: 650px; overflow-x: hidden; overflow-y: auto">
						<MerchantHierarchyTreeViewPartial ref="refMerchantHierarchyTreeViewPartial"> </MerchantHierarchyTreeViewPartial>
					</div>
				</v-card-text>
			</v-card>
		</v-dialog>
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import * as Model from '@/models/interfaces/statistics/merchant-summary';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import DropdownApi from '@/api/commons/dropdown';
import { datetimeMixin, startEndDate, timeWithTimezone, daysAgo, weeksAgo, monthsAgo, toLocalDateString } from '@/utilities/datetime-format';
import { Locale, Currency, Status, GameType, CRUDAction, GameVendorType } from '@/models/status/common';
import { commonMethodMixin } from '@/utilities/common-methods';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import NotificationWarning from '@/components/notification/NotificationWarning.vue';
import GameSummaryReportDataTableList from '@/components/statistics/merchant-summary/GameSummaryReportDataTableList.vue';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import { State, Action, Getter, namespace } from 'vuex-class';
import MerchantSummaryApi from '@/api/statistics/merchant-summary';
import DatePicker from 'vue2-datepicker';
import * as GameModel from '@/models/interfaces/game-setting/switch';
import ChartApi from '@/api/statistics/chart';
import * as ChartModel from '@/models/interfaces/statistics/chart';
import ChartPopup from '@/components/statistics/BarChartPopup.vue';
import * as MerchantCreditModel from '@/models/interfaces/agent/merchantCredit';
import MerchantHierarchyTreeViewPartial from '@/components/agent/MerchantHierarchyTreeViewPartial.vue';

const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';
import { ChartDataTypeEnum } from '@/models/status/statistics';

// @ts-ignore
@Component({
	// @ts-ignore
	components: { NotificationWarning, DatePicker, ChartPopup, GameSummaryReportDataTableList, MerchantHierarchyTreeViewPartial },
	mixins: [commonDisplayFiltersMixin, datetimeMixin, commonMethodMixin],
})
export default class AgentHierarchyDailyGameReport extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];

	CRUDActionEnums: typeof CRUDAction = CRUDAction;

	dialogVisible: DropdownModel.DialogVisible = new DropdownModel.DialogVisible();
	popupExtraInfo: string = '';

	renderComponent: boolean = true;

	DataTablePagingParameter: DropdownModel.DataTablePagingParameter = new DropdownModel.DataTablePagingParameter();
	datetimeRangePicker: DropdownModel.DatetimeRangePicker = new DropdownModel.DatetimeRangePicker();

	searchFilter: Model.IMerchantDailyGameReportRequest = {} as Model.IMerchantDailyGameReportRequest;
	searchResult: Model.IMerchantDailyGameReportResult = {} as Model.IMerchantDailyGameReportResult;
	pageItems: any[] = [];

	searchChildDetail: Model.IMerchantDailyReport = {} as Model.IMerchantDailyReport;

	footerProps: GameModel.IDataTablePageObject = {
		'items-per-page-options': [10, 50, 100],
	} as GameModel.IDataTablePageObject;

	showNotificationWarning: boolean = false;
	panel: number[] = [0];

	dropdownGames: DropdownModel.IDropdownItems[] = [];
	dropdownMerchants: DropdownModel.IDropdownItems[] = [];
	dropdownAgentAggregativeType: DropdownModel.IDropdownItems[] = [];

	get hasPermissionCreate() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSetting_GameList_Create) >= 0;
	}
	get hasPermissionUpdate() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSetting_GameList_Update) >= 0;
	}

	get hasPermission_IsBetAmount_visible() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.Special_BetAmount_FlowAmount_Invisible) >= 0 ? false : true;
	}
	get hasPermission_IsRTP_visible() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.Special_RTP_Invisible) >= 0 ? false : true;
	}

	get headers() {
		const data = [
			{ text: this.$t('TableHeader.__time'), value: 'displayDate', sortable: true, align: 'start', width: '7%' },
			{ text: this.$t('TableHeader.__merchant'), value: 'merchantID', sortable: true, align: 'start', width: '10%' },
			{ text: this.$t('TableHeader.__game'), value: 'gameID', sortable: true, align: 'start', width: '10%' },
			{ text: this.$t('TableHeader.__gameType'), value: 'gameTypeText', sortable: true, align: 'start', width: '10%' },
			{ text: this.$t('TableHeader.__totalPeopleCount'), value: 'totalPlayers', sortable: true, align: 'end' },
			{ text: this.$t('TableHeader.__totalRound'), value: 'totalRound', sortable: true, align: 'end' },
			{ text: this.$t('TableHeader.__betAmount'), value: 'betAmount', sortable: true, align: 'end' },
			{ text: this.$t('TableHeader.__payoutAmount'), value: 'payoutAmount', sortable: true, align: 'end' },
			{ text: this.$t('TableHeader.__merchantProfit'), value: 'merchantWinLossAmount', sortable: true, align: 'end' },
			{ text: this.$t('TableHeader.__profitRate'), value: 'profitRate', sortable: true, align: 'end' },
		];

		return data;
	}

	get headersOfMerchantDetail() {
		const data = [
			{ text: this.$t('TableHeader.__time'), value: 'date', sortable: true, align: 'start' },
			{ text: this.$t('TableHeader.__merchant'), value: 'merchantID', sortable: true, align: 'start' },
			{ text: this.$t('TableHeader.__game'), value: 'gameID', sortable: true, align: 'end' },
			{ text: this.$t('TableHeader.__totalRound'), value: 'totalRound', sortable: true, align: 'end' },
			{ text: this.$t('TableHeader.__betAmount'), value: 'betAmount', sortable: true, align: 'end' },
			{ text: this.$t('TableHeader.__payoutAmount'), value: 'payoutAmount', sortable: true, align: 'end' },
			{ text: this.$t('TableHeader.__winLossAmount'), value: 'winLossAmount', sortable: true, align: 'end' },
			{ text: this.$t('TableHeader.__totalRound'), value: 'totalRound', sortable: true, align: 'end' },
		];

		return data;
	}

	get timeZones() {
		const data = [
			{ text: 'UTC-12:00', value: -12 },
			{ text: 'UTC-11:00', value: -11 },
			{ text: 'UTC-10:00', value: -10 },
			{ text: 'UTC-9:00', value: -9 },
			{ text: 'UTC-8:00', value: -8 },
			{ text: 'UTC-7:00', value: -7 },
			{ text: 'UTC-6:00', value: -6 },
			{ text: 'UTC-5:00', value: -5 },
			{ text: 'UTC-4:00', value: -4 },
			{ text: 'UTC-3:30', value: -3.5 },
			{ text: 'UTC-3:00', value: -3 },
			{ text: 'UTC-2:00', value: -2 },
			{ text: 'UTC-1:00', value: -1 },
			{ text: 'UTC+0:00', value: 0 },
			{ text: 'UTC+1:00', value: 1 },
			{ text: 'UTC+2:00', value: 2 },
			{ text: 'UTC+3:00', value: 3 },
			{ text: 'UTC+4:00', value: 4 },
			{ text: 'UTC+5:00', value: 5 },
			{ text: 'UTC+5:30', value: 5.5 },
			{ text: 'UTC+6:00', value: 6 },
			{ text: 'UTC+6:30', value: 6.5 },
			{ text: 'UTC+7:00', value: 7 },
			{ text: 'UTC+8:00', value: 8 },
			{ text: 'UTC+9:00', value: 9 },
			{ text: 'UTC+9:30', value: 9.5 },
			{ text: 'UTC+10:00', value: 10 },
			{ text: 'UTC+10:30', value: 10.5 },
			{ text: 'UTC+11:00', value: 11 },
			{ text: 'UTC+12:00', value: 12 },
		];

		return data;
	}

	geti18nTextAsString(key: string) {
		return this.$t(key).toString();
	}

	clear() {
		this.searchFilter = {} as Model.IMerchantDailyGameReportRequest;
		this.searchFilter.timeZone = this.userProfile.timeZone;
	}

	closeAllModal() {
		this.dialogVisible.resetAllValueToFalse();
	}

	showChildAgentSingleDateSummary(searchItem: Model.IMerchantDailyReportItem) {
		// this.dialogVisible.isShowDetailInfo = true;
		// this.popupExtraInfo = searchItem.date;
		// this.searchChildDetail.items = [];
		// const myFilter = {} as Model.IChildAgentSingleDateSummaryRequest;
		// myFilter.begin = searchItem.begin;
		// myFilter.end = searchItem.end;
		// myFilter.timeZone = this.searchFilter.timeZone;
		// EventBus.$emit('show_mask', null);
		// const result = MerchantSummaryApi.getChildAgentSingleDateSummaryReport(myFilter)
		// 	.then((res) => {
		// 		this.searchChildDetail = res.data;
		// 	})
		// 	.finally(() => {
		// 		EventBus.$emit('hide_mask', null);
		// 	});
	}

	refresh() {
		EventBus.$emit('show_mask', null);

		this.searchFilter.pageIndex = this.DataTablePagingParameter.pageIndex;
		this.searchFilter.pageSize = this.DataTablePagingParameter.itemsPerPage;
		this.searchFilter.begin = this.datetimeRangePicker.searchStartTimeRange[0] ? toLocalDateString(this.datetimeRangePicker.searchStartTimeRange[0]) : null;
		this.searchFilter.end = this.datetimeRangePicker.searchStartTimeRange[1] ? toLocalDateString(this.datetimeRangePicker.searchStartTimeRange[1]) : null;

		// 轉成當前時區的日期
		MerchantSummaryApi.getAgentHierarchyDailyGameReport(this.searchFilter)
			.then((res) => {
				this.searchResult = res.data;
				this.pageItems = res.data.pagedList.items;
				this.DataTablePagingParameter.totalCount = res.data.pagedList.totalCount;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	changePage(index: number = 1) {
		this.DataTablePagingParameter.pageIndex = index;
		if (this.pageItems.length > 0) {
			this.refresh();
		}
	}

	getItemPerPage(val: number = 10) {
		this.DataTablePagingParameter.itemsPerPage = val;
		if (this.pageItems.length > 0) {
			this.refresh();
		}
	}

	async exportFiles() {
		const excelHeaders: any = [
			this.geti18nTextAsString('TableHeader.__time'),
			this.geti18nTextAsString('TableHeader.__merchant'),
			this.geti18nTextAsString('TableHeader.__game'),
			this.geti18nTextAsString('TableHeader.__gameType'),
			this.geti18nTextAsString('TableHeader.__totalPeopleCount'),
			this.geti18nTextAsString('TableHeader.__totalRound'),
			this.geti18nTextAsString('TableHeader.__betAmount'),
			this.geti18nTextAsString('TableHeader.__payoutAmount'),
			this.geti18nTextAsString('TableHeader.__winLossAmount'),
			this.geti18nTextAsString('TableHeader.__merchantProfit'),
		];
		const excelData: any = [];

		if (this.pageItems.length <= 0) {
			alert('Please search data first before exporting.');
			return;
		}

		this.pageItems.forEach((value) => {
			const eachRow: any = [];
			eachRow.push(value.displayDate);
			eachRow.push(value.merchantName);
			eachRow.push(value.gameName);
			eachRow.push(value.gameTypeText);
			eachRow.push(value.totalPlayers);
			eachRow.push(value.totalRound);
			eachRow.push(value.betAmount);
			eachRow.push(value.payoutAmount);
			eachRow.push(value.merchantWinLossAmount);
			eachRow.push(value.profitRate);

			excelData.push(eachRow);
		});

		const config = {
			headers: excelHeaders,
			rowData: excelData,
			fileName: 'Export_AgentHierarchyDailyGameReport.xlsx',
		};

		// @ts-ignore
		this.downloadExcel(config);
	}

	mounted() {
		DropdownApi.getHierarchyMerchants().then((res) => {
			this.dropdownMerchants = res;
		});

		DropdownApi.getAgentAggregativeType().then((res) => {
			this.dropdownAgentAggregativeType = res;
		});

		DropdownApi.getGames().then((res) => {
			this.dropdownGames = res;
		});

		this.datetimeRangePicker.searchStartTimeRange = [daysAgo(-7), daysAgo(1)];
		this.clear();
	}
}
</script>

<style scoped>
.btn-row {
	margin-top: 15px !important;
	width: 100% !important;
	width: 100% !important;
}

.border-r-style {
	border-right: solid 1px #eee;
}
</style>
