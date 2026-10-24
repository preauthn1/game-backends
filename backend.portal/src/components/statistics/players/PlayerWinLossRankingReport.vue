<template>
	<v-app v-if="renderComponent" class="app">
		<v-expansion-panels v-model="panel" multiple>
			<v-expansion-panel>

				<v-expansion-panel-content v-if="isRenderSearchFilter">
					<v-flex>
						<v-row>

							<v-col cols="12" sm="3" md="3">

								<div class="item merchantDropdownItemsWrapper">
									<v-autocomplete clearable :items="dropdownVendorConfigs" class="required" chips
										v-model="searchFilter.vendorConfigID" :label="$t('TableHeader.__vendorConfig')">

									</v-autocomplete>
								</div>

							</v-col>

							<v-col cols="12" sm="3" md="3">

								<div class="item">
									<v-autocomplete clearable :items="dropdownReportType" class="required" chips
										@change="whenReportTypeChange()" v-model="searchFilter.reportType"
										:label="'報表類型'">
									</v-autocomplete>
								</div>

							</v-col>

							<v-col cols="12" sm="3" md="3">

								<div class="item merchantDropdownItemsWrapper">
									<v-autocomplete clearable :items="dropdownMerchants" class="merchantDropdownItems"
										chips v-model="searchFilter.merchantID" :label="$t('TableHeader.__merchant')">

										<template v-slot:item="data">
											<div
												:class="data.item.valueInt == null  ? 'merchantCategoryText': 'merchantValue'">
												<span>{{data.item.text}}</span>
											</div>
										</template>

									</v-autocomplete>
								</div>

							</v-col>

							<v-col cols="12" sm="3" md="3">
								<!-- <div class="item right"> -->
								<div class="item">
									<v-btn color="yellow darken-1" text class="btn search" @click.stop="search">
										<v-icon left>search</v-icon>
										{{ $t('Button.__search') }}
									</v-btn>&#9;
									<v-btn color="pink lighten-3" text class="btn search" @click.stop="clear">
										<v-icon left>clear</v-icon>
										{{ $t('Button.__reset') }}
									</v-btn>

									<v-btn color="light-blue lighten-3" text class="btn search"
										@click.stop="exportFiles">
										<v-icon left>cloud_download</v-icon>
										{{ $t('Button.__downloadFile') }}
									</v-btn>&#9;

								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">

									<v-text-field v-model="searchFilter.playerID" clearable
										:label="$t('TableHeader.__playerID')">

									</v-text-field>

								</div>

							</v-col>
							<v-col cols="12" sm="3" md="3">
								<div class="item">

									<v-text-field v-model="searchFilter.playerName" clearable
										:label="$t('TableHeader.__playerAccount')">

									</v-text-field>

								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">

									<v-autocomplete clearable :items="sortColumns" class=""
										v-model="searchFilter.orderByColumn" :label="$t('Filter.__sort')">

									</v-autocomplete>

								</div>

							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">

									<v-autocomplete clearable :items="dropdownSortDirection" class=""
										v-model="searchFilter.sortByDirection" :label="$t('Filter.__sortDirection')">

									</v-autocomplete>

								</div>
							</v-col>

						</v-row>

						<v-row style="padding-bottom: 20px;">

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<date-picker v-model="datetimeRangePicker.searchStartTimeRange" type="datetime"
										v-bind:clearable="false" value-type="timestamp" range
										:placeholder="$t('TableHeader.__time')">
									</date-picker>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3" class="quick-time">
								<div class="item">
									<v-chip class="mr-2"
										:class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'today' }"
										@click="globalQuicklyPickTimeRange('today')">
										<v-icon left>mdi-alarm-check</v-icon>
										{{ $t(`TableHeader.__today`) }}
									</v-chip>
									<v-chip class="mr-2"
										:class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'yesterday' }"
										@click="globalQuicklyPickTimeRange('yesterday')">
										<v-icon left>mdi-alarm-check</v-icon>
										{{ $t(`TableHeader.__yesterday`) }}
									</v-chip>
									<v-chip class="mr-2"
										:class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'thisWeek' }"
										@click="globalQuicklyPickTimeRange('thisWeek')">
										<v-icon left>mdi-alarm-check</v-icon>
										{{ $t(`TableHeader.__thisWeek`) }}
									</v-chip>

									<v-chip class="mr-2"
										:class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'lastWeek' }"
										@click="globalQuicklyPickTimeRange('lastWeek')">
										<v-icon left>mdi-alarm-check</v-icon>
										{{ $t(`TableHeader.__lastWeek`) }}
									</v-chip>
									<v-chip class="mr-2"
										:class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'thisMonth' }"
										@click="globalQuicklyPickTimeRange('thisMonth')">
										<v-icon left>mdi-alarm-check</v-icon>
										{{ $t(`TableHeader.__thisMonth`) }}
									</v-chip>
									<v-chip class="mr-2"
										:class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'lastMonth' }"
										@click="globalQuicklyPickTimeRange('lastMonth')">
										<v-icon left>mdi-alarm-check</v-icon>
										{{ $t(`TableHeader.__lastMonth`) }}
									</v-chip>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">

								<div class="item merchantDropdownItemsWrapper">
									<v-autocomplete clearable :items="dropdownGames" class="" chips
										:disabled="isSearchAllInOneReportType()" v-model="searchFilter.gameID"
										:label="$t('TableHeader.__game')">

									</v-autocomplete>
								</div>

							</v-col>
						</v-row>
					</v-flex>
				</v-expansion-panel-content>
			</v-expansion-panel>
		</v-expansion-panels>

		<div class="hint-row">

			<v-icon> error </v-icon>
			{{ $t('Message.__dataDelay30Minutes') }}

			<v-icon class="ml-3"> visibility </v-icon>
			{{ $t('TableHeader.__playerViewPoint') }}

			<span class="f12 grey-text ml-3" style="display: inline-block;"
				v-if="searchResult.serverTimeTW">
				{{ $t('TableHeader.__lastSearchTime') }}: {{ searchResult.serverTimeTW }}
			</span>

		</div>

		<v-row>
			<v-col cols="12">

				<v-data-table :items="searchItems" :headers="headers" id="table" :disable-sort="true"
					:page="DataTablePagingParameter.pageIndex"
					:items-per-page="DataTablePagingParameter.itemsPerPage"
					:server-items-length="DataTablePagingParameter.totalCount"
					:footer-props="DataTablePagingParameter.footerProps" @update:page="changePage"
					@update:items-per-page="getItemPerPage">

					<template v-slot:item.merchantName="{ item }">
						<div>{{item.merchantName}}</div>
						<div class="sub-title-text">{{item.merchantID}}</div>
					</template>

					<template v-slot:item.gameName="{ item }">

						<div v-if="item.gameID == 0">
							{{$t("Button.__allGames")}}
						</div>
						<div v-else>

							<div>{{item.gameName}}</div>
							<div class="sub-title-text">{{item.gameID}}</div>
						</div>

					</template>

					<template v-slot:item.playerName="{ item }">
						<div class="link-text" @click="openPlayerDetail(item.playerID)">
							{{item.playerName}}
						</div>
						<div class="sub-title-text">{{item.playerID}}</div>
					</template>

					<template v-slot:item.winRate="{ item }">
						{{ item.winRate | amountDisplay }}
					</template>

					<template v-slot:item.betAmount="{ item }">
						{{ item.betAmount | amountDisplay }}
					</template>

					<template v-slot:item.payoutAmount="{ item }">
						{{ item.payoutAmount | amountDisplay }}
					</template>

					<template v-slot:item.winLossAmount="{ item }">
						<span :class="item.winLossAmount < 0 ? 'minus-red' : ''">
							{{ item.winLossAmount | amountDisplay }}
						</span>
					</template>

					<template v-slot:item.rtp="{ item }">
						{{ item.rtp | amountDisplay }}
					</template>

					<template v-slot:item.totalRound="{ item }">
						{{ item.totalRound | countDisplay }}
					</template>

				</v-data-table>

			</v-col>
		</v-row>

		<!-- 彈出視窗: 用戶詳情 -->
		<SinglePlayerGameReportPopup ref="refSinglePlayerGameReportPopup"></SinglePlayerGameReportPopup>

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
import * as GameReportModel from '@/models/interfaces/statistics/game';
import { datetimeMixin, startEndDate, timeWithTimezone, daysAgo, weeksAgo, monthsAgo, toDateStringYYYYMMDDByDateText } from '@/utilities/datetime-format';
import * as ChartModel from '@/models/interfaces/statistics/chart';
import ChartApi from '@/api/statistics/chart';
import DatePicker from 'vue2-datepicker';
import LineChartPopup from '@/components/statistics/LineChartPopup.vue';
import SinglePlayerGameReportPopup from '@/components/statistics/players/SinglePlayerGameReportPopup.vue';
import { ChartDataTypeEnum } from '@/models/status/statistics';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import ReportApi from '@/api/statistics/merchant-summary';
import * as Model from '@/models/interfaces/statistics/merchant-summary';

const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';

// @ts-ignore
@Component({ mixins: [datetimeMixin, commonDisplayFiltersMixin, commonMethodMixin], components: { DatePicker, LineChartPopup, SinglePlayerGameReportPopup } })
export default class PlayerWinRateReport extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];
	@authModule.State('defaultLangTimes') defaultLangTimes!: number;

	panel: number[] = [0];
	renderComponent: boolean = true;
	isRenderSearchFilter: boolean = true;

	DataTablePagingParameter: DropdownModel.DataTablePagingParameter = new DropdownModel.DataTablePagingParameter();
	datetimeRangePicker: DropdownModel.DatetimeRangePicker = new DropdownModel.DatetimeRangePicker();
	searchFilter: Model.IPlayerWinLossRankingReportRequest = {} as Model.IPlayerWinLossRankingReportRequest;
	searchResult: Model.IPlayerWinLossRankingSearchResult = {} as Model.IPlayerWinLossRankingSearchResult;
	searchItems: Model.IPlayerWinLossRankingDetailItem[] = [];

	dropdownMerchants: DropdownModel.IDropdownItems[] = [];
	dropdownVendorConfigs: DropdownModel.IDropdownItems[] = [];
	dropdownGames: DropdownModel.IDropdownItems[] = [];
	dropdownSortDirection: DropdownModel.IDropdownItems[] = [];

	get headers() {
		return [
			{ text: this.$t('TableHeader.__action'), value: 'action', align: 'center', sortable: false },
			{ text: this.$t('TableHeader.__merchantName'), value: 'merchantName', align: 'start', sortable: false },
			{ text: this.$t('TableHeader.__playerAccount'), value: 'playerName', align: 'start', sortable: false },
			{ text: this.$t('TableHeader.__gameName'), value: 'gameName', align: 'start', sortable: false },
			{ text: this.$t('TableHeader.__totalRound'), value: 'totalRound', align: 'end', sortable: true },
			{ text: this.$t('TableHeader.__winRate') + ' (%)', value: 'winRate', align: 'end', sortable: true },
			{ text: this.$t('TableHeader.__betAmount'), value: 'betAmount', align: 'end', sortable: true },
			{ text: this.$t('TableHeader.__payoutAmount'), value: 'payoutAmount', align: 'end', sortable: true },
			{ text: this.$t('TableHeader.__playerWinLoss'), value: 'winLossAmount', align: 'end', sortable: true },
			{ text: this.$t('TableHeader.__rtp'), value: 'rtp', align: 'end', sortable: true },
		];
	}

	get sortColumns() {
		const sortColumns: any = [];
		sortColumns.push({ text: this.geti18nTextAsString('TableHeader.__gameWinLoss'), value: 'winLossAmount' });
		sortColumns.push({ text: this.geti18nTextAsString('TableHeader.__rtp'), value: 'rtp' });
		sortColumns.push({ text: this.geti18nTextAsString('TableHeader.__winRate'), value: 'winRate' });
		return sortColumns;
	}

	get dropdownReportType() {
		const sortColumns: any = [];
		sortColumns.push({ text: this.geti18nTextAsString('Button.__groupGamesAllInOne'), value: 0 });
		sortColumns.push({ text: this.geti18nTextAsString('Button.__groupAndListEachGames'), value: 1 });
		return sortColumns;
	}

	@Action('Players/setPlayerID') private setPlayerID!: any;

	registerReloadComponent() {
		EventBus.$on(`reload-${this.$router.currentRoute.path}`, (info: any) => {
			this.clear();
			this.reloadComponent();
		});
	}

	isSearchAllInOneReportType() {
		return this.searchFilter.reportType === 0;
	}

	reloadComponent() {
		setTimeout(() => {
			this.renderComponent = false;
			this.$nextTick(() => {
				this.renderComponent = true;
			});
		}, 250);
	}

	changePage(index: number = 1) {
		this.DataTablePagingParameter.pageIndex = index;
		if (this.searchItems.length > 0) {
			this.search();
		}
	}

	getItemPerPage(val: number = 10) {
		this.DataTablePagingParameter.itemsPerPage = val;
		if (this.searchItems.length > 0) {
			this.search();
		}
	}

	whenReportTypeChange() {
		this.$nextTick(() => {
			this.resetGameID_WhenReportTypeIsAllInOne();
		});
	}

	resetGameID_WhenReportTypeIsAllInOne() {
		if (this.isSearchAllInOneReportType()) {
			this.searchFilter.gameID = null;
		}

		this.isRenderSearchFilter = false;
		this.isRenderSearchFilter = true;
	}

	search() {
		EventBus.$emit('show_mask', null);

		this.resetGameID_WhenReportTypeIsAllInOne();
		this.setSearchFilter();

		ReportApi.getPlayerWinLossRankingReport(this.searchFilter)
			.then((res) => {
				this.searchResult = res.data;
				this.searchItems = res.data.pagingItems.items;
				this.DataTablePagingParameter.totalCount = res.data.pagingItems.totalCount;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	setSearchFilter() {
		this.resetGameID_WhenReportTypeIsAllInOne();
		this.searchFilter.isGroupAllGameOne = this.isSearchAllInOneReportType();
		this.searchFilter.pageIndex = this.DataTablePagingParameter.pageIndex;
		this.searchFilter.pageSize = this.DataTablePagingParameter.itemsPerPage;
		this.searchFilter.isASC = this.searchFilter.sortByDirection === 0;
		this.searchFilter.begin = this.datetimeRangePicker.searchStartTimeRange[0] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[0]) : null;
		this.searchFilter.end = this.datetimeRangePicker.searchStartTimeRange[1] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[1]) : null;
	}

	openPlayerDetail(playerID: number) {
		this.setPlayerID(playerID);
		this.$router.push({ path: '/members-management/detail' });
	}

	showSinglePlayerDetail(playerID: number) {
		const begin = this.datetimeRangePicker.searchStartTimeRange[0] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[0]) : null;
		const end = this.datetimeRangePicker.searchStartTimeRange[1] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[1]) : null;

		const targetRef: any = this.$refs.refSinglePlayerGameReportPopup;
		targetRef.showSinglePlayerDetail(playerID, begin, end);
	}

	clear() {
		const tempValue = this.searchFilter.vendorConfigID;
		this.searchFilter = {} as Model.IPlayerWinLossRankingReportRequest;
		this.searchFilter.vendorConfigID = tempValue;
	}

	geti18nTextAsString(key: string) {
		return this.$t(key).toString();
	}

	mounted() {
		this.reloadComponent();
		this.registerReloadComponent();
		this.init();

		DropdownApi.getMerchants(true).then((res) => {
			this.dropdownMerchants = res;
		});

		DropdownApi.getGames().then((res) => {
			this.dropdownGames = res;
		});

		DropdownApi.getVendorConfig().then((res) => {
			this.dropdownVendorConfigs = res;
			this.searchFilter.vendorConfigID = res[0].valueInt;
		});

		DropdownApi.getSortDirection().then((res) => {
			this.dropdownSortDirection = res;
			this.searchFilter.sortByDirection = res[1].valueInt;
		});
	}

	init() {
		this.datetimeRangePicker.searchStartTimeRange = [daysAgo(-3), daysAgo(1)];
		this.searchFilter.reportType = 0;
		this.searchFilter.orderByColumn = this.sortColumns[0].value;

		const vm: any = this;

		this.$nextTick(() => {
			setTimeout(() => {
				vm.isRenderSearchFilter = false;
				vm.isRenderSearchFilter = true;
			}, 250);
		});
	}

	async exportFiles() {
		const excelHeaders: any = [
			this.geti18nTextAsString('TableHeader.__merchantName'),
			this.geti18nTextAsString('TableHeader.__playerAccount'),
			this.geti18nTextAsString('TableHeader.__gameName'),
			this.geti18nTextAsString('TableHeader.__totalRound'),
			this.geti18nTextAsString('TableHeader.__winRate'),
			this.geti18nTextAsString('TableHeader.__betAmount'),
			this.geti18nTextAsString('TableHeader.__payoutAmount'),
			this.geti18nTextAsString('TableHeader.__playerWinLoss'),
			this.geti18nTextAsString('TableHeader.__rtp'),
		];

		const excelData: any = [];

		this.setSearchFilter();
		this.searchFilter.pageIndex = 1;
		this.searchFilter.pageSize = 5000;

		EventBus.$emit('show_mask', null);
		const result = await ReportApi.getPlayerWinLossRankingReport(this.searchFilter).finally(() => {
			EventBus.$emit('hide_mask', null);
		});

		result.data.pagingItems.items.forEach((value) => {
			const eachRow: any = [];
			eachRow.push(value.merchantName);
			eachRow.push(value.playerName);
			eachRow.push(value.gameName);
			eachRow.push(value.totalRound);
			eachRow.push(value.winRate);
			eachRow.push(value.betAmount);
			eachRow.push(value.payoutAmount);
			eachRow.push(value.winLossAmount);
			eachRow.push(value.rtp);
			excelData.push(eachRow);
		});

		const config = {
			headers: excelHeaders,
			rowData: excelData,
			fileName: 'Export_PlayerWinLossRankingReport.xlsx',
		};

		// @ts-ignore
		this.downloadExcel(config);
	}
}
</script>
<style lang="scss" scoped>
</style>
