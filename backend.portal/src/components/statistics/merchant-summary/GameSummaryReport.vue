<template>
	<v-app v-if="renderComponent" class="app">
		<v-expansion-panels v-model="panel" multiple>
			<v-expansion-panel>
				<v-expansion-panel-content>
					<v-flex>
						<v-row>
							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete clearable :items="merchantsDropdownItems" chips v-model="selectMerchants" class="merchantDropdownItems" :label="$t('TableHeader.__merchant')">
										<template v-slot:item="data">
											<div :class="data.item.valueInt == null ? 'merchantCategoryText' : 'merchantValue'">
												<span>{{ data.item.text }}</span>
											</div>
										</template>
									</v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="">
									<v-autocomplete
										:items="searchExtraColumnDropdownItems"
										clearable
										chips
										deletable-chips
										:label="$t('Button.__showExtraInfo') + ' (' + $t('Message.__queryTimeWillLonger') + ')'"
										v-model="searchFilter.includeExtraSearchInfo"
										multiple
									>
									</v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="6" md="6">
								<!-- <div class="item right"> -->
								<div class="item">
									<v-btn color="yellow darken-1" text class="btn search" @click.stop="search">
										<v-icon left>search</v-icon>
										{{ $t('Button.__search') }} </v-btn
									>&#9;
									<v-btn color="pink lighten-3" text class="btn search" @click.stop="clear">
										<v-icon left>clear</v-icon>
										{{ $t('Button.__reset') }}
									</v-btn>

									<v-btn color="light-blue lighten-3" text class="btn search" @click.stop="exportFiles">
										<v-icon left>cloud_download</v-icon>
										{{ $t('Button.__downloadFile') }} </v-btn
									>&#9;
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="" style="margin-top: -20px">
									<date-picker
										v-model="datetimeRangePicker.searchStartTimeRange"
										type="datetime"
										v-bind:clearable="false"
										value-type="timestamp"
										range
										:placeholder="$t('TableHeader.__time')"
									>
									</date-picker>
								</div>
							</v-col>

							<v-col cols="12" sm="6" md="6" class="quick-time">
								<!-- <div class="item"> -->
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
								<!-- </div> -->
							</v-col>
						</v-row>
					</v-flex>
				</v-expansion-panel-content>
			</v-expansion-panel>
		</v-expansion-panels>

		<div class="hint-row" style="width: 100%">
			<v-icon> error </v-icon>
			{{ $t('Message.__dataDelay30Minutes') }}

			<v-icon class="ml-3"> visibility </v-icon>
			{{ $t('TableHeader.__playerViewPoint') }}

			<v-icon class="ml-3"> error </v-icon>
			{{ $t('Message.__OnlySearchSingleMerchantNotAllAgent') }}

			<v-icon class="ml-2" small>mdi-clock</v-icon>
			GMT+{{ userProfile.timeZone }}

			<div style="float: right" v-if="searchResultFullModel != null && searchResultFullModel.merchants != null && searchResultFullModel.merchants.length > 0">
				<v-btn @click="showPieChart()" color="blue lighten-4" small text class="btn mr-3">
					<v-icon color="blue lighten-4" left>list</v-icon>
					{{ $t('TableHeader.__pieChart') }} ({{ $t('TableHeader.__betAmount') }})
				</v-btn>
			</div>
		</div>

		<GameSummaryReportDataTableList ref="refGameSummaryReportDataTableList" @setSearchResultToParentComponent="setSearchResult"> </GameSummaryReportDataTableList>

		<v-dialog v-model="dialogVisible.isShowChart" max-width="1200px">
			<ChartPopup ref="refChartPopup" :chartData="searchChartData" :datasets="searchChartData.datasets" :xTitle="'xxxxx'" :yTitle="'yyyyy'" @close="closeAllModal" />
		</v-dialog>

		<v-dialog v-model="dialog" v-if="dialog">
			<LineChartPopup v-if="isMounted" :title="title" :datasets="datasets" :xTitle="xTitle" :yTitle="yTitle" @close="dialog = false" />
			<v-overlay :absolute="true" :value="true" v-show="!isMounted">
				<!-- <v-progress-circular class="loading" :size="70" :width="7" indeterminate></v-progress-circular> -->
				<div class="loader"></div>
			</v-overlay>
		</v-dialog>
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import { State, Action, Getter, namespace } from 'vuex-class';
import EventBus from '@/utilities/event-bus';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { commonMethodMixin } from '@/utilities/common-methods';
import DropdownApi from '@/api/commons/dropdown';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import { datetimeMixin, startEndDate, timeWithTimezone, daysAgo, weeksAgo, monthsAgo } from '@/utilities/datetime-format';
import * as Model from '@/models/interfaces/statistics/game';
import * as ReportModel from '@/models/interfaces/statistics/merchant-summary';
import * as ChartModel from '@/models/interfaces/statistics/chart';
import ReportApi from '@/api/statistics/merchant-summary';
import ChartApi from '@/api/statistics/chart';
import DatePicker from 'vue2-datepicker';
import LineChartPopup from '@/components/statistics/LineChartPopup.vue';
import ChartPopup from '@/components/statistics/PieChartPopup.vue';
import GameSummaryReportDataTableList from '@/components/statistics/merchant-summary/GameSummaryReportDataTableList.vue';
import { ChartDataTypeEnum } from '@/models/status/statistics';
import * as PermissionModel from '@/models/interfaces/commons/permission';
const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';

// @ts-ignore
@Component({
	// @ts-ignore
	mixins: [datetimeMixin, commonDisplayFiltersMixin, commonMethodMixin],
	components: { DatePicker, LineChartPopup, ChartPopup, GameSummaryReportDataTableList },
})
export default class SearchList extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];
	@authModule.State('defaultLangTimes') defaultLangTimes!: number;

	panel: number[] = [0];
	selectMerchants: number[] = [];
	renderComponent: boolean = true;

	dialogVisible: DropdownModel.DialogVisible = new DropdownModel.DialogVisible();

	merchantsDropdownItems: DropdownModel.IDropdownItems[] = [];
	datetimeRangePicker: DropdownModel.DatetimeRangePicker = new DropdownModel.DatetimeRangePicker();
	result: Model.IMerchant[] = [];
	searchChartData: ChartModel.ICommonChartReponse = {} as ChartModel.ICommonChartReponse;
	searchResultFullModel: Model.IGameListReponse = {} as Model.IGameListReponse;
	searchFilter: ReportModel.IGameReportRequest = {} as ReportModel.IGameReportRequest;

	searchExtraColumnDropdownItems: DropdownModel.IDropdownItems[] = [];

	get hasPermissionGameDetail() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.GamesManagementRecord) >= 0;
	}

	get hasPermission_IsRTP_visible() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.Special_RTP_Invisible) >= 0 ? false : true;
	}

	get hasPermission_IsWinLose_visible() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.Special_WinLose_Invisible) >= 0 ? false : true;
	}

	dialog: boolean = false;
	isMounted: boolean = false;
	title: string = '';
	datasets: any[] = [];
	xTitle: string = '';
	yTitle: string = '';

	@Action('Players/setGameID') private setGameID!: any;
	@Action('Players/setGamePlayerID') private setGamePlayerID!: any;
	registerReloadComponent() {
		EventBus.$on(`reload-${this.$router.currentRoute.path}`, (info: any) => {
			this.renderComponent = false;
			this.$nextTick(() => {
				this.clear();

				this.renderComponent = true;
			});
		});
	}
	@Watch('defaultLangTimes')
	onDefaultLangChanged() {
		if (this.result.length > 0) {
			this.search();
		}
	}

	clear() {
		this.selectMerchants = [];
	}

	closeAllModal() {
		this.dialogVisible.resetAllValueToFalse();
	}

	showPieChart() {
		const payload: ChartModel.IChartConvertRequest = {} as ChartModel.IChartConvertRequest;

		if (!this.searchResultFullModel || this.searchResultFullModel.merchants.length <= 0) {
			return;
		}

		const matchMerchant = this.searchResultFullModel.merchants[0];

		payload.chartDisplayName = '';
		payload.targetName = matchMerchant.merchantName;
		payload.items = [];
		payload.isDescOrderByValue = true;

		matchMerchant.items.forEach((item) => {
			const myItem: ChartModel.IChartConvertItem = {} as ChartModel.IChartConvertItem;
			myItem.x_Label = item.gameName;
			myItem.y_Value = item.betAmount;
			payload.items.push(myItem);
		});

		EventBus.$emit('show_mask', null);

		ChartApi.convertToPieChartObject(payload)
			.then((res) => {
				this.dialogVisible.isShowChart = true;
				this.searchChartData = res.data;

				this.$nextTick(() => {
					const targetRef: any = this.$refs.refChartPopup;
					targetRef.initialCanvasData();
				});
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	openGameDetail(gameID: number) {
		this.setGameID(gameID);
		this.setGamePlayerID('');
		this.$router.push({ path: '/games-management/record' });
	}

	search() {
		const selectedMerchantCount = this.selectMerchants.length;

		if (selectedMerchantCount === 0) {
			EventBus.$emit('event-error', this.geti18nTextAsString('Message.__pleaseSelectMerchant'));
			return;
		}

		this.searchFilter.merchantIDs = this.selectMerchants === null || this.selectMerchants.length === 0 ? [] : this.selectMerchants;
		this.searchFilter.begin = this.datetimeRangePicker.searchStartTimeRange[0] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[0]) : null;
		this.searchFilter.end = this.datetimeRangePicker.searchStartTimeRange[1] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[1]) : null;

		this.$nextTick(() => {
			const myRef: any = this.$refs.refGameSummaryReportDataTableList;
			myRef.searchByFilter(this.searchFilter);
		});

		EventBus.$emit('show_mask', null);
	}

	setSearchResult(result: any) {
		this.searchResultFullModel = result;
		console.log(this.searchResultFullModel);
	}

	async exportFiles() {
		const selectedMerchantCount = this.selectMerchants.length;

		if (selectedMerchantCount === 0) {
			alert(this.geti18nTextAsString('Message.__pleaseSelectMerchant'));
			return;
		}

		if (selectedMerchantCount > 1) {
			alert('匯出報表請選擇單一商戶，不支援多選');
			return;
		}

		const excelHeaders: any = [
			this.geti18nTextAsString('TableHeader.__gameID'),
			this.geti18nTextAsString('TableHeader.__gameName'),
			this.geti18nTextAsString('TableHeader.__type'),
			this.geti18nTextAsString('TableHeader.__totalBetPlayer'),
			'总游玩局数',
			this.geti18nTextAsString('TableHeader.__betAmount'),
			this.geti18nTextAsString('TableHeader.__validAmount'),
			this.geti18nTextAsString('TableHeader.__payoutAmount'),
			this.geti18nTextAsString('TableHeader.__playerWinLoss'),
			'rtp',
		];

		const vm: any = this;
		const excelData: any = [];

		this.searchFilter.merchantIDs = this.selectMerchants === null || this.selectMerchants.length === 0 ? [] : this.selectMerchants;
		this.searchFilter.begin = this.datetimeRangePicker.searchStartTimeRange[0] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[0]) : null;
		this.searchFilter.end = this.datetimeRangePicker.searchStartTimeRange[1] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[1]) : null;

		EventBus.$emit('show_mask', null);

		const result = await ReportApi.getGameReportList(this.searchFilter).finally(() => {
			EventBus.$emit('hide_mask', null);
		});

		result.data.merchants[0].items.forEach((value) => {
			const eachRow: any = [];
			eachRow.push(value.gameID);
			eachRow.push(value.gameName);
			eachRow.push(value.gameTypeText);
			eachRow.push(value.onlinePlayer);
			eachRow.push(value.totalRound);
			eachRow.push(value.betAmount);
			eachRow.push(value.validAmount);
			eachRow.push(vm.visibleOrDashValue(this.hasPermission_IsWinLose_visible, value.payoutAmount));
			eachRow.push(vm.visibleOrDashValue(this.hasPermission_IsWinLose_visible, value.updateAmount));
			eachRow.push(vm.visibleOrDashValue(this.hasPermission_IsRTP_visible, value.rtp));

			excelData.push(eachRow);
		});

		const config = {
			headers: excelHeaders,
			rowData: excelData,
			fileName: 'Export_GameReport.xlsx',
		};

		// @ts-ignore
		this.downloadExcel(config);
	}

	showTotalRoundChart(merchantID: number) {
		this.isMounted = false;
		this.dialog = true;

		this.xTitle = this.$t('TableHeader.__xTitle').toString();
		this.yTitle = this.$t('TableHeader.__game').toString();

		ChartApi.getChart(ChartDataTypeEnum.Game_TotalGame, <ChartModel.IChartRequest>{
			begin: this.datetimeRangePicker.searchStartTimeRange[0] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[0]) : null,
			end: this.datetimeRangePicker.searchStartTimeRange[1] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[1]) : null,
			merchantIDs: [merchantID],
		}).then((res) => {
			this.isMounted = true;
			this.title = this.$t('TableHeader.__totalRound').toString(); // res.data.chartName
			this.datasets = [];
			res.data.chartDatas.forEach((item) => {
				this.datasets.push({
					group: item.gameName,
					data: item.data.map((s) => {
						return {
							x: new Date(timeWithTimezone(s.statTime, this.userProfile.timeZone)),
							y: s.dataCount,
						};
					}),
				});
			});
		});
	}

	showOnlinePlayerChart(merchantID: number) {
		this.isMounted = false;
		this.dialog = true;

		this.xTitle = this.$t('TableHeader.__xTitle').toString();
		this.yTitle = this.$t('TableHeader.__game').toString();

		ChartApi.getChart(ChartDataTypeEnum.Game_OnlineUser, <ChartModel.IChartRequest>{
			begin: this.datetimeRangePicker.searchStartTimeRange[0] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[0]) : null,
			end: this.datetimeRangePicker.searchStartTimeRange[1] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[1]) : null,
			merchantIDs: [merchantID],
		}).then((res) => {
			this.isMounted = true;
			this.title = this.$t('TableHeader.__onlinePlayer').toString(); // res.data.chartName
			this.datasets = [];
			res.data.chartDatas.forEach((item) => {
				this.datasets.push({
					group: item.gameName,
					data: item.data.map((s) => {
						return {
							x: new Date(timeWithTimezone(s.statTime, this.userProfile.timeZone)),
							y: s.dataCount,
						};
					}),
				});
			});
		});
	}

	geti18nTextAsString(key: string) {
		return this.$t(key).toString();
	}

	init() {
		DropdownApi.getMerchants(true).then((res) => {
			this.merchantsDropdownItems = res;
		});

		DropdownApi.getStatReportSearchColumns('game-report').then((res) => {
			this.searchExtraColumnDropdownItems = res;
		});

		this.datetimeRangePicker.searchStartTimeRange = [daysAgo(-7), daysAgo(1)];
	}

	mounted() {
		this.registerReloadComponent();
		this.init();
	}
}
</script>
<style lang="scss" scoped>
.v-expansion-panel--active > .v-expansion-panel-header {
	min-height: 1% !important;
	max-height: 1% !important;
}

.item {
	max-width: 350px;
	// width: 100%;
	// display: block;
	max-height: 20px !important;
	margin-top: 2px !important;
	margin-bottom: 3px !important;
	&.right {
		text-align: right;
	}
}

.checkbox-item {
	margin-top: -15px !important;
}
.search {
	max-width: 80px;
	margin-top: 15px !important;
}
.footer {
	white-space: nowrap;
}

.mx-datepicker {
	margin-top: 15px !important;
	width: 100% !important;
	width: 100% !important;
}

.v-input--dense {
	margin-top: 15px !important;
	width: 100% !important;
	width: 100% !important;
}

.text-r {
	text-align: right;
}
</style>
