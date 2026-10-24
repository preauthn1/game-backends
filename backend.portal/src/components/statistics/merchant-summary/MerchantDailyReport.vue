<template>
	<v-app v-if="renderComponent" class="app">
		<v-expansion-panels v-model="panel" multiple>
			<v-expansion-panel>
				<v-expansion-panel-content>
					<v-flex>
						<v-row>
							<v-col cols="12" sm="3" md="3">
								<!-- 若有勾選簡化下拉選單 會採用serverRender -->
								<div class="item" v-if="!isSimplifyDropdownItems">
									<v-autocomplete clearable :items="dropdownMerchants" v-model="searchFilter.merchantID" class="merchantDropdownItems" :label="$t('TableHeader.__merchant')">
										<template v-slot:item="data">
											<div :class="data.item.valueInt == null ? 'merchantCategoryText' : 'merchantValue'">
												<span>{{ data.item.text }}</span>
											</div>
										</template>
									</v-autocomplete>
								</div>
								<div class="item" v-else>
									<v-autocomplete
										clearable
										:items="dropdownMerchantsWithKeyword"
										chips
										:search-input.sync="merchantsSearchKeyword"
										v-model="searchFilter.merchantID"
										class="merchantDropdownItems"
										:label="$t('TableHeader.__merchant')"
									>
									</v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="2" md="2">
								<div class="item">
									<v-autocomplete clearable :items="timeZones" v-model="searchFilter.timeZone" chips menu-props="auto" class="" :label="$t('TableHeader.__timeZoneArea')">
									</v-autocomplete>
								</div>
							</v-col>
							<v-col cols="12" sm="4" md="4" class="smallGamesDropdown">
								<div class="mt-4">
									<v-autocomplete
										v-model="searchFilter.gameIDs"
										:items="dropdownGames"
										clearable
										deletable-chips
										multiple
										@change="whenSelectFilterGame()"
										small-chips
										:label="$t('TableHeader.__game')"
									>
										<template v-slot:prepend-item>
											<v-list-item ripple @click="showRefMultipleGameSelectedDialog()">
												<v-list-item-action>
													<v-icon>check</v-icon>
												</v-list-item-action>

												<v-list-item-content>
													<v-list-item-title>
														{{ $t('Message.__useCategorySelectGames') }}
													</v-list-item-title>
												</v-list-item-content>
											</v-list-item>

											<v-divider />
										</template>
									</v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="btn-row" style="margin-top: 25px !important">
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
										{{ $t('Button.__downloadFile') }} </v-btn
									>&#9;
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="minus-mt-1">
									<date-picker
										v-model="datetimeRangePicker.searchStartTimeRange"
										type="date"
										v-bind:clearable="false"
										class="force-w100"
										value-type="timestamp"
										range
										:placeholder="$t('TableHeader.__time')"
									>
									</date-picker>
								</div>
							</v-col>

							<v-col cols="12" sm="6" md="6" class="quick-time">
								<div class="minus-mt-1">
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

		<div v-if="dailyReportsRawResponse.isShowGoToAgentReportHint" style="padding: 15px; font-size: 15px">
			<div class="hint-go-to-agent-report">
				<v-icon color="red ligthen-1" small>error</v-icon>
				{{ $t('Message.__thisAgentIsTreeModePleaseUseAgentReport') }}
			</div>
		</div>

		<v-data-table
			id="table"
			:headers="headers"
			:items="dailyReports"
			:footer-props="footerProps"
			v-if="isRenderTable && !dailyReportsRawResponse.isShowGoToAgentReportHint"
			hide-default-footer
			sort
		>
			<template v-slot:top>
				<v-toolbar>
					<div class="hint-row" style="width: 100%">
						<v-icon> error </v-icon>
						{{ $t('Message.__dataDelay30Minutes') }}

						<v-icon class="ml-3"> visibility </v-icon>
						{{ $t('TableHeader.__bankViewPoint') }}

						<v-icon class="ml-3">mdi-clock</v-icon>
						GMT+{{ searchFilter.timeZone }}

						<v-chip class="ml-4" title="currency" v-if="dailyReportsRawResponse && !isEmptyString(dailyReportsRawResponse.merchantCurrency)">
							{{ dailyReportsRawResponse.merchantCurrency }}
						</v-chip>

						<span class="f12 grey-text ml-3" style="display: inline-block" v-if="dailyReportsRawResponse.serverTimeTW">
							{{ $t('TableHeader.__lastSearchTime') }}: {{ dailyReportsRawResponse.serverTimeTW }}
						</span>

						<div class="hide-in-phone" style="float: right" v-if="dailyReports && dailyReports.length > 0">
							<v-btn @click="showChartByArray(1)" color="blue lighten-4" small text class="btn search mr-3">
								<v-icon color="blue lighten-4" left>list</v-icon>
								{{ $t('TableHeader.__barChart') }} ({{ $t('TableHeader.__betAmount') }})
							</v-btn>

							<v-btn @click="showChartByArray(2)" color="blue lighten-4" small text class="btn search">
								<v-icon color="blue lighten-4" left>list</v-icon>
								{{ $t('TableHeader.__barChart') }} ({{ $t('TableHeader.__totalRound') }})
							</v-btn>
						</div>
					</div>
				</v-toolbar>
			</template>

			<template v-slot:item.totalRound="{ item }">
				{{ item.totalRound | countDisplay }}
			</template>

			<template v-slot:item.totalLoginCount="{ item }">
				{{ item.totalLoginCount | countDisplay }}
			</template>

			<template v-slot:item.transferInAmount="{ item }">
				<span v-if="isShowTransferAmount">
					{{ item.transferInAmount | amountDisplay }}
				</span>
				<span v-if="!isShowTransferAmount"> - </span>
			</template>
			<template v-slot:item.transferOutAmount="{ item }">
				<span v-if="isShowTransferAmount">
					{{ item.transferOutAmount | amountDisplay }}
				</span>
				<span v-if="!isShowTransferAmount"> - </span>
			</template>
			<template v-slot:item.invisibleColumn="{ item }"> - </template>

			<template v-slot:item.fishGameRemainingBalance="{ item }">
				{{ item.fishGameRemainingBalance | amountDisplay }}
			</template>

			<template v-slot:item.betAmount="{ item }">
				{{ item.betAmount | amountDisplay }}
			</template>

			<template v-slot:item.validAmount="{ item }">
				{{ item.validAmount | amountDisplay }}
			</template>

			<template v-slot:item.payoutAmount="{ item }">
				{{ item.payoutAmount | amountDisplay }}
			</template>

			<template v-slot:item.promotionCostAmount="{ item }">
				<div v-if="isFilterGames">-</div>
				<div v-else>
					<span class="link-text-grey" @click="showEventSummaryGroupByEventID(item.merchantID, item.begin, item.end)">
						{{ item.promotionCostAmount | amountDisplay }}
					</span>
				</div>
			</template>

			<template v-slot:item.merchantWinLossAmountWithPromotionCost="{ item }">
				<span v-if="isFilterGames">-</span>
				<span v-else :class="item.merchantWinLossAmountWithPromotionCost < 0 ? 'minus-red ' : ''">
					{{ item.merchantWinLossAmountWithPromotionCost | amountDisplay }}
				</span>
			</template>

			<template v-slot:item.merchantWinLossAmount="{ item }">
				<span :class="item.merchantWinLossAmount < 0 ? 'minus-red ' : ''">
					{{ item.merchantWinLossAmount | amountDisplay }}
				</span>
			</template>

			<template v-slot:item.rtp="{ item }">
				<span :class="item.rtp > 100 ? 'h-yellow  ' : ''">
					{{ item.rtp | amountDisplay }}
				</span>
			</template>

			<template v-slot:item.action="{ item }">
				<v-menu offset-y>
					<template v-slot:activator="{ on, attrs }">
						<div class="dropdown-menu" v-bind="attrs" v-on="on">
							<v-icon dark outline> expand_more </v-icon>
						</div>
					</template>

					<v-list class="dropdonwMenuPanel">
						<v-list-item class="dropdownMenu">
							<v-list-item @click="showChart(item, 5101)">
								<v-icon> visibility </v-icon>
								{{ $t('TableHeader.__barChart') }} ({{ $t('TableHeader.__totalRound') }})
							</v-list-item>

							<v-list-item @click="showChart(item, 5103)" v-if="hasPermission_IsBetAmount_visible">
								<v-icon> visibility </v-icon>
								{{ $t('TableHeader.__barChart') }} ({{ $t('TableHeader.__betAmount') }})
							</v-list-item>

							<v-list-item @click="showTargetDateGameSummary(item)" v-if="hasPermission_IsBetAmount_visible">
								<v-icon> view_list </v-icon>
								{{ $t('Menu.StatisticsGame') }}
							</v-list-item>
						</v-list-item>
					</v-list>
				</v-menu>
			</template>

			<template slot="body.append" v-if="dailyReportsTotalSummary && dailyReports && dailyReports.length > 0">
				<tr>
					<td>
						{{ $t('TableHeader.__total') }}
						:
					</td>
					<td></td>
					<td></td>
					<td class="text-r">
						{{ dailyReportsTotalSummary.totalRound | countDisplay }}
					</td>
					<!-- 					
					<td class='text-r'>
						{{dailyReportsTotalSummary.transferInAmount | amountDisplay}}
					</td>
					<td class='text-r'>
						{{dailyReportsTotalSummary.transferOutAmount | amountDisplay}}
					</td> -->

					<td class="text-r" v-if="hasPermission_IsBetAmount_visible">
						{{ dailyReportsTotalSummary.betAmount | amountDisplay }}
					</td>
					<td class="text-r" v-if="hasPermission_IsBetAmount_visible">
						{{ dailyReportsTotalSummary.validAmount | amountDisplay }}
					</td>
					<td class="text-r" v-if="hasPermission_IsBetAmount_visible">
						{{ visibleOrDashValue(hasPermission_IsWinLose_visible, dailyReportsTotalSummary.payoutAmount) | amountDisplay }}
					</td>
					<td class="text-r">
						<span :class="dailyReportsTotalSummary.merchantWinLossAmount < 0 ? 'minus-red' : ''">
							{{ visibleOrDashValue(hasPermission_IsWinLose_visible, dailyReportsTotalSummary.merchantWinLossAmount) | amountDisplay }}
						</span>
					</td>

					<td class="text-r" v-if="hasPermission_IsRTP_visible">
						{{ visibleOrDashValue(hasPermission_IsRTP_visible, dailyReportsTotalSummary.rtp) | amountDisplay }}
					</td>

					<td class="text-r" v-if="isShowFisgGameExtraInfo">-</td>

					<td class="text-r">
						<span
							v-if="hasPermission_IsWinLose_visible && !isFilterGames"
							class="link-text-grey"
							@click="showEventSummaryGroupByEventID(searchFilter.merchantID, dailyReportsRawResponse.queryBeginUTC, dailyReportsRawResponse.queryEndUTC)"
						>
							{{ dailyReportsTotalSummary.promotionCostAmount | amountDisplay }}
						</span>
						<span v-else> - </span>
					</td>

					<td class="text-r">
						<span :class="dailyReportsTotalSummary.merchantWinLossAmountWithPromotionCost < 0 ? 'minus-red' : ''">
							{{ visibleOrDashValue(hasPermission_IsWinLose_visible && !isFilterGames, dailyReportsTotalSummary.merchantWinLossAmountWithPromotionCost) | amountDisplay }}
						</span>
					</td>
				</tr>
			</template>
		</v-data-table>

		<v-dialog v-model="isShowGameSummary" max-width="1300px" scrollable>
			<v-card class="">
				<v-card-title class="grey darken-2"> {{ $t('Button.__showDetail') }} ({{ $t('TableHeader.__game') }}) </v-card-title>

				<v-card-text>
					<GameSummaryReportDataTableList ref="refGameSummaryReportDataTableList"> </GameSummaryReportDataTableList>
				</v-card-text>
			</v-card>
		</v-dialog>

		<v-dialog v-model="isShowChartDialog" max-width="1200px">
			<ChartPopup ref="refChartPopup" :chartData="searchChartData" :datasets="searchChartData.datasets" :xTitle="'xxxxx'" :yTitle="'yyyyy'" @close="closeAllModal" />
		</v-dialog>

		<MultipleGameSelectedDialog ref="refMultipleGameSelectedDialog" @parentConfirmMultipleSelectedGames="confirmMultipleSelectedGames"> </MultipleGameSelectedDialog>
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import * as Model from '@/models/interfaces/statistics/merchant-summary';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import DropdownApi from '@/api/commons/dropdown';
import { datetimeMixin, startEndDate, timeWithTimezone, daysAgo, weeksAgo, monthsAgo, toLocalDateString, toDateStringMMDDByDateText } from '@/utilities/datetime-format';
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
import MultipleGameSelectedDialog from '@/components/common-component/MultipleGameSelectedDialog.vue';

const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';
import { ChartDataTypeEnum } from '@/models/status/statistics';

// @ts-ignore
@Component({
	// @ts-ignore
	components: { NotificationWarning, DatePicker, ChartPopup, GameSummaryReportDataTableList, MultipleGameSelectedDialog },
	mixins: [commonDisplayFiltersMixin, datetimeMixin, commonMethodMixin],
})
export default class GameList extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];

	CRUDActionEnums: typeof CRUDAction = CRUDAction;

	isRenderTable: boolean = true;
	isShowChartDialog: boolean = false;
	isShowTransferAmount: boolean = false;
	isShowGameSummary: boolean = false;
	isShowFisgGameExtraInfo: boolean = false;
	isShowDetailSearchCheckbox: boolean = true;
	isFilterGameManualFlag: boolean = false;
	isLockSearchKeywordAPI: boolean = false;

	renderComponent: boolean = true;
	pendingTimeoutTask: number = 0;

	merchantsSearchKeyword: string = '';

	chartTypeOfBetAmount: number = 1;
	chartTypeOfRound: number = 2;

	datetimeRangePicker: DropdownModel.DatetimeRangePicker = new DropdownModel.DatetimeRangePicker();

	dailyReportsRawResponse: Model.IMerchantDailyReport = {} as Model.IMerchantDailyReport;
	dailyReports: Model.IMerchantDailyReportItem[] = [];
	dailyReportsTotalSummary: Model.IMerchantDailyReportItem = {} as Model.IMerchantDailyReportItem;

	searchFilter = {} as Model.IMerchantDailySearchRequest;
	searchChartData: ChartModel.ICommonChartReponse = {} as ChartModel.ICommonChartReponse;

	footerProps: GameModel.IDataTablePageObject = {
		'items-per-page-options': [100],
	} as GameModel.IDataTablePageObject;

	showNotificationWarning: boolean = false;
	panel: number[] = [0];

	dropdownMerchantsWithKeyword: DropdownModel.IDropdownItems[] = [];
	dropdownMerchants: DropdownModel.IDropdownItems[] = [];
	dropdownGames: DropdownModel.IDropdownItems[] = [];

	get hasPermissionDefaultEnableSearchDetail() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.Special_DefaultEnableSearchDetailCheckbox) >= 0;
	}
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
	get hasPermission_IsWinLose_visible() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.Special_WinLose_Invisible) >= 0 ? false : true;
	}
	get hasPermission_AgentTotalSumReport() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.Special_RTP_Invisible) >= 0 ? false : true;
	}

	get headers() {
		const vm: any = this;
		const titleText = this.$t('TableHeader.__merchantProfit');
		const data = [
			{ text: '', value: 'action', sortable: true, align: 'center' },
			{ text: this.$t('TableHeader.__xTitle'), value: 'date', sortable: false },
			{ text: this.$t('TableHeader.__totalBetPlayer'), value: 'totalLoginCount', sortable: true, align: 'end' },
			{ text: this.$t('TableHeader.__totalRound'), value: 'totalRound', sortable: true, align: 'end' },
		];

		// 部分客戶希望不顯示特定欄位(莫名其妙)
		if (this.hasPermission_IsBetAmount_visible) {
			data.push({ text: this.$t('TableHeader.__betAmount'), value: 'betAmount', sortable: true, align: 'end' });
			data.push({ text: this.$t('TableHeader.__validAmount'), value: 'validAmount', sortable: true, align: 'end' });
		}

		data.push({ text: this.$t('TableHeader.__payoutAmount'), value: vm.visibleOrNot(this.hasPermission_IsWinLose_visible, 'payoutAmount'), sortable: true, align: 'end' });
		data.push({ text: this.$t('TableHeader.__gameWinLoss'), value: vm.visibleOrNot(this.hasPermission_IsWinLose_visible, 'merchantWinLossAmount'), sortable: true, align: 'end' });

		if (this.hasPermission_IsRTP_visible) {
			data.push({ text: 'RTP', value: 'rtp', sortable: true, align: 'end' });
		}

		if (this.isShowFisgGameExtraInfo) {
			data.push({ text: '平台剩余金币', value: 'fishGameRemainingBalance', sortable: true, align: 'end' });
		}

		data.push({ text: this.$t('TableHeader.__promotionCost'), value: vm.visibleOrNot(this.hasPermission_IsWinLose_visible, 'promotionCostAmount'), sortable: true, align: 'end' });
		data.push({ text: titleText, value: vm.visibleOrNot(this.hasPermission_IsWinLose_visible, 'merchantWinLossAmountWithPromotionCost'), sortable: true, align: 'end' });

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

	get isFilterGames() {
		return this.isFilterGameManualFlag;
	}

	@Watch('merchantsSearchKeyword')
	whenMerchantKeywordChange() {
		clearTimeout(this.pendingTimeoutTask);
		this.pendingTimeoutTask = setTimeout(() => {
			this.searchDropdownByKeywordAPI();
		}, 600);
	}

	searchDropdownByKeywordAPI() {
		const myFilter = {} as DropdownModel.IMerchantDropdownRequest;
		myFilter.keyword = this.merchantsSearchKeyword;
		myFilter.isSimplifyItems = false;

		DropdownApi.getMerchantsByFilter(myFilter, false)
			.then((res) => {
				this.dropdownMerchantsWithKeyword = res;
			})
			.finally(() => {});

		return;
	}

	whenSelectFilterGame() {
		const vm: any = this;
		this.isFilterGameManualFlag = !vm.isEmptyCollection(this.searchFilter.gameIDs);
	}

	showRefMultipleGameSelectedDialog() {
		const targerRef: any = this.$refs.refMultipleGameSelectedDialog;
		targerRef.showDialog(this.dropdownGames);
	}

	confirmMultipleSelectedGames(gameIDs: number[]) {
		if (gameIDs != null && gameIDs.length > 0) {
			this.searchFilter.gameIDs = [];
			for (const id of gameIDs) {
				this.searchFilter.gameIDs.push(id);
			}
			this.reloadPage();

			if (this.searchFilter.merchantID != null && this.searchFilter.merchantID > 0) {
				this.refresh();
			}
		}
	}

	showEventSummaryGroupByEventID(myMerchantID: number, rowBegin: string, rowEnd: string) {
		const myFilter: any = {
			querySqlID: 900179,
			merchantID: myMerchantID,
			beginTime: rowBegin.replaceAll('Z', ''),
			endTime: rowEnd.replaceAll('Z', ''),
		};

		const myConfig: any = {
			maxWidth: 950,
		};

		EventBus.$emit(`globalShowCustomizedColumnsDisplayPopupDialog`, myFilter, myConfig);
	}

	showChart(item: Model.IMerchantDailyReportItem, type: ChartDataTypeEnum) {
		this.isShowChartDialog = true;

		const filter: ChartModel.IChartRequest = {} as ChartModel.IChartRequest;
		filter.merchantID = item.merchantID;
		filter.begin = item.begin;
		filter.end = item.end;
		filter.timezone = this.searchFilter.timeZone;

		EventBus.$emit('show_mask', null);

		ChartApi.getChartV2(type, filter)
			.then((res) => {
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

	showChartByArray(chartType: number) {
		const payload: ChartModel.IChartConvertRequest = {} as ChartModel.IChartConvertRequest;

		payload.targetName = this.dailyReportsRawResponse.merchantName;
		payload.items = [];

		this.dailyReports.forEach((item) => {
			const myItem: ChartModel.IChartConvertItem = {} as ChartModel.IChartConvertItem;

			const dateText: any = item.date;
			myItem.x_Label = dateText.replaceAll('2022-', '').replaceAll('2023-', '').replaceAll('2024-', '').replaceAll('2025-', '').replaceAll('2026-', '');

			if (chartType === this.chartTypeOfBetAmount) {
				payload.chartDisplayName = this.geti18nTextAsString('TableHeader.__betAmount');
				myItem.y_Value = item.betAmount;
			} else if (chartType === this.chartTypeOfRound) {
				payload.chartDisplayName = this.geti18nTextAsString('TableHeader.__totalRound');
				myItem.y_Value = item.totalRound;
			}

			payload.items.push(myItem);
		});

		EventBus.$emit('show_mask', null);
		ChartApi.convertToBarChartObject(payload)
			.then((res) => {
				this.searchChartData = res.data;
				this.isShowChartDialog = true;

				this.$nextTick(() => {
					const targetRef: any = this.$refs.refChartPopup;
					targetRef.initialCanvasData();
				});
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	setExcelHeaders() {
		const key = 'RTP';
	}

	isSimplifyDropdownItems() {
		const vm: any = this;
		return vm.isEnableSimplifyDropdown();
	}

	showTargetDateGameSummary(item: Model.IMerchantDailyReportItem) {
		this.isShowGameSummary = true;

		const myFilter = {} as Model.IGameReportRequest;

		myFilter.begin = item.begin;
		myFilter.end = item.end;
		myFilter.merchantIDs = [item.merchantID];

		this.$nextTick(() => {
			const myRef: any = this.$refs.refGameSummaryReportDataTableList;
			myRef.searchByFilter(myFilter);
			myRef.setExtraMemo(' (' + item.date + ')');
		});
	}

	geti18nTextAsString(key: string) {
		return this.$t(key).toString();
	}

	clear() {
		this.searchFilter.merchantID = null;
		this.searchFilter.gameIDs = [];
		this.reloadPage();
	}

	reloadPage() {
		this.renderComponent = false;
		this.$nextTick(() => {
			this.renderComponent = true;
		});
	}

	reloadTableRender() {
		this.isRenderTable = false;
		this.$nextTick(() => {
			this.isRenderTable = true;
		});
	}

	closeAllModal() {
		this.isShowChartDialog = false;
	}

	changeSelectExtraInfoValue() {
		if (!this.searchFilter.merchantID || this.searchFilter.merchantID == null) {
			return;
		}
		this.isShowDetailSearchCheckbox = this.searchFilter.isIncludeExtraInfo;
		this.refresh();
	}

	selectedAllGamesType(gameType: number) {
		this.searchFilter.gameIDs = [];

		for (const item of this.dropdownGames) {
			const myGameID: any = item.valueInt;

			if (gameType === 1000) {
				// @ts-ignore
				if (myGameID >= 1000 && myGameID <= 1999) {
					// @ts-ignore
					this.searchFilter.gameIDs.push(myGameID);
				}
				if (myGameID >= 290000 && myGameID <= 290999) {
					// @ts-ignore
					this.searchFilter.gameIDs.push(myGameID);
				}
			}
			if (gameType === 2500) {
				// @ts-ignore
				if (myGameID >= 330000 && myGameID <= 339999) {
					// @ts-ignore
					this.searchFilter.gameIDs.push(myGameID);
				}
				if (myGameID >= 530000 && myGameID <= 539999) {
					// @ts-ignore
					this.searchFilter.gameIDs.push(myGameID);
				}
			}
		}

		this.renderComponent = false;

		this.$nextTick(() => {
			this.renderComponent = true;
		});
	}

	refresh() {
		// if (!this.searchFilter.merchantID || this.searchFilter.merchantID == null) {
		// 	EventBus.$emit('auto-hidden-error', this.$t('Message.__pleaseSelectMerchant'));
		// 	return;
		// }

		EventBus.$emit('show_mask', null);

		this.whenSelectFilterGame();
		this.isRenderTable = false;

		this.searchFilter.begin = this.datetimeRangePicker.searchStartTimeRange[0] ? toLocalDateString(this.datetimeRangePicker.searchStartTimeRange[0]) : null;
		this.searchFilter.end = this.datetimeRangePicker.searchStartTimeRange[1] ? toLocalDateString(this.datetimeRangePicker.searchStartTimeRange[1]) : null;

		// 轉成當前時區的日期
		MerchantSummaryApi.getMerchantDailyReport(this.searchFilter)
			.then((res) => {
				this.dailyReportsRawResponse = res.data;
				this.dailyReports = res.data.items;
				this.dailyReportsTotalSummary = res.data.totalSummary;
				this.isShowFisgGameExtraInfo = res.data.isSearchFishGameExtraInfo;
				this.isRenderTable = true;
			})
			.finally(() => {
				setTimeout(() => {
					EventBus.$emit('hide_mask', null);
				}, 500);
			});
	}

	async exportFiles() {
		const excelHeaders: any = [
			this.geti18nTextAsString('TableHeader.__merchant'),
			this.geti18nTextAsString('TableHeader.__xTitle'),
			this.geti18nTextAsString('TableHeader.__totalBetPlayer'),
			this.geti18nTextAsString('TableHeader.__totalRound'),
			this.geti18nTextAsString('TableHeader.__betAmount'),
			this.geti18nTextAsString('TableHeader.__validAmount'),
			this.geti18nTextAsString('TableHeader.__payoutAmount'),
			this.geti18nTextAsString('TableHeader.__gameWinLoss'),
			'rtp',
		];

		excelHeaders.push(this.geti18nTextAsString('TableHeader.__promotionCost'));
		excelHeaders.push(this.geti18nTextAsString('TableHeader.__merchantProfit'));

		const vm: any = this;
		const excelData: any = [];

		this.searchFilter.begin = this.datetimeRangePicker.searchStartTimeRange[0] ? toLocalDateString(this.datetimeRangePicker.searchStartTimeRange[0]) : null;
		this.searchFilter.end = this.datetimeRangePicker.searchStartTimeRange[1] ? toLocalDateString(this.datetimeRangePicker.searchStartTimeRange[1]) : null;

		EventBus.$emit('show_mask', null);
		const result = await MerchantSummaryApi.getMerchantDailyReport(this.searchFilter).finally(() => {
			EventBus.$emit('hide_mask', null);
		});

		result.data.items.forEach((value) => {
			const eachRow: any = [];
			eachRow.push(value.merchantName);
			eachRow.push(value.date);
			eachRow.push(value.totalLoginCount);
			eachRow.push(value.totalRound);
			eachRow.push(value.betAmount);
			eachRow.push(value.validAmount);
			eachRow.push(vm.visibleOrDashValue(this.hasPermission_IsWinLose_visible, value.payoutAmount));
			eachRow.push(vm.visibleOrDashValue(this.hasPermission_IsWinLose_visible, value.merchantWinLossAmount));
			eachRow.push(vm.visibleOrDashValue(this.hasPermission_IsRTP_visible, value.rtp));
			eachRow.push(vm.visibleOrDashValue(this.hasPermission_IsWinLose_visible, value.promotionCostAmount));
			eachRow.push(vm.visibleOrDashValue(this.hasPermission_IsWinLose_visible, value.merchantWinLossAmountWithPromotionCost));

			excelData.push(eachRow);
		});

		const totalSum: any = [];
		totalSum.push('Total');
		totalSum.push('');
		totalSum.push('');
		totalSum.push(result.data.totalSummary.totalRound);
		totalSum.push(vm.visibleOrDashValue(this.hasPermission_IsBetAmount_visible, result.data.totalSummary.betAmount));
		totalSum.push(vm.visibleOrDashValue(this.hasPermission_IsBetAmount_visible, result.data.totalSummary.validAmount));
		totalSum.push(vm.visibleOrDashValue(this.hasPermission_IsWinLose_visible, result.data.totalSummary.payoutAmount));
		totalSum.push(vm.visibleOrDashValue(this.hasPermission_IsWinLose_visible, result.data.totalSummary.merchantWinLossAmount));
		totalSum.push(vm.visibleOrDashValue(this.hasPermission_IsRTP_visible, result.data.totalSummary.rtp));
		totalSum.push(vm.visibleOrDashValue(this.hasPermission_IsWinLose_visible, result.data.totalSummary.promotionCostAmount));
		totalSum.push(vm.visibleOrDashValue(this.hasPermission_IsWinLose_visible, result.data.totalSummary.merchantWinLossAmountWithPromotionCost));
		excelData.push(totalSum);

		const datePrefix = '_' + toDateStringMMDDByDateText(this.searchFilter.begin, this.userProfile.timeZone) + '_' + toDateStringMMDDByDateText(this.searchFilter.end, this.userProfile.timeZone);

		const config = {
			headers: excelHeaders,
			rowData: excelData,
			fileName: 'Export_DailyReport' + datePrefix + '.xlsx',
		};

		// @ts-ignore
		this.downloadExcel(config);
	}

	mounted() {
		DropdownApi.getMerchants(true).then((res) => {
			this.dropdownMerchants = res;
			this.dropdownMerchantsWithKeyword = this.dropdownMerchants;
			if (this.dropdownMerchants.length === 1) {
				this.searchFilter.merchantID = this.dropdownMerchants[0].valueInt;
			}
		});

		DropdownApi.getGames().then((res) => {
			this.dropdownGames = res;
		});

		const vm: any = this;
		setTimeout(() => {
			this.searchFilter.isIncludeExtraInfo = true;
			vm.datetimeRangePicker.searchStartTimeRange = [daysAgo(-7), daysAgo(1)];
			vm.globalQuicklyPickTimeRange('thisMonth');
		}, 250);

		this.searchFilter.timeZone = this.userProfile.timeZone;
	}
}
</script>

<style scoped>
.btn-row {
	margin-top: 15px !important;
	width: 100% !important;
	width: 100% !important;
}
</style>
