<template>
	<v-app v-if="renderComponent" class="app">
		<v-expansion-panels v-model="panel" multiple>
			<v-expansion-panel>
				<v-expansion-panel-content>
					<v-flex>
						<v-row>
							<v-col cols="12" sm="3" md="3">
								<div class="">
									<v-autocomplete clearable :items="dropdownMerchants" chips v-model="searchFilter.merchantID" class="merchantDropdownItems" :label="$t('TableHeader.__merchant')">
									</v-autocomplete>

									<!-- <v-autocomplete
										clearable
										:items="dynamicMerchantDropdownWithKeyword"
										no-filter
										:no-data-text="'🔍🔍 ' + $t('Message.__pleaseInputKeywordToSearchDropdown')"
										:search-input.sync="globalKeywordSearchMerchantsDropdown"
										v-model="searchFilter.merchantID"
										:label="$t('TableHeader.__merchant')"
									>
										<template slot="selection" slot-scope="data">
											{{ data.item.text }}
										</template>
									</v-autocomplete> -->
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="">
									<v-autocomplete
										:items="searchExtraColumnDropdownItems"
										clearable
										deletable-chips
										chips
										:label="$t('Button.__showExtraInfo') + ' (' + $t('Message.__queryTimeWillLonger') + ')'"
										v-model="searchFilter.includeExtraSearchInfo"
										multiple
									>
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

							<v-col cols="12" sm="2" md="2">
								<div class="item mt-4">
									<v-btn color="yellow darken-1" text class="btn search" @click.stop="refresh">
										<v-icon left>search</v-icon>
										{{ $t('Button.__search') }} </v-btn
									>&#9;

									<v-btn color="pink lighten-3" text class="btn search" @click.stop="clear">
										<v-icon left>clear</v-icon>
										{{ $t('Button.__reset') }}
									</v-btn>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="minus-mt-4">
									<date-picker
										v-model="datetimeRangePicker.searchStartTimeRange"
										type="datetime"
										v-bind:clearable="false"
										class="force-w100"
										value-type="timestamp"
										range
										:placeholder="$t('TableHeader.__time')"
									>
									</date-picker>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3" class="quick-time">
								<div class="minus-mt-4">
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

							<v-col cols="12" sm="3" md="3">
								<div class="minus-mt-4">
									<v-checkbox
										class="ml-4"
										:label="$t('TableHeader.__excludeZeroAmount')"
										style="width: 250px; display: inline-block; margin-top: 0px"
										v-model="searchFilter.isExcludeZeroData"
									>
									</v-checkbox>
								</div>
							</v-col>
						</v-row>
					</v-flex>
				</v-expansion-panel-content>
			</v-expansion-panel>
		</v-expansion-panels>

		<div>
			<div style="float: left; padding-left: 15px">
				<div class="mt-2">
					<v-btn small color="yellow lighten-3" text class="btn search" v-if="isShowReturnRoot" @click.stop="refresh()">
						<i aria-hidden="true" style="font-size: 18px" class="v-icon notranslate mdi mdi-arrow-left theme--dark mr-1"></i>
						{{ $t('Button.__returnToTopLayer') }} </v-btn
					>&#9;

					<v-btn
						small
						color="yellow lighten-3"
						text
						class="btn search"
						v-if="searchResult && searchResult.parentMerchantID > 0"
						@click.stop="searchDetailAndChildsByMerchantIDAndName(searchResult.parentMerchantID, searchResult.parentMerchantName)"
					>
						<i aria-hidden="true" style="font-size: 18px" class="v-icon notranslate mdi mdi-arrow-left theme--dark mr-1"></i>
						{{ $t('Button.__viewParentAgent') }} ({{ searchResult.parentMerchantName }}) </v-btn
					>&#9;

					<v-btn small color="light-blue lighten-3" text class="btn search ml-2" v-if="searchResult.items && searchResult.items.length > 0" @click.stop="exportFiles">
						<v-icon left>cloud_download</v-icon>
						{{ $t('Button.__downloadFile') }} ({{ $t('TableHeader.__currentPageOnly') }}) </v-btn
					>&#9;

					<span class="ml-2 f14">
						{{ currentMerchantNameText }}
					</span>

					<v-chip small class="ml-1">{{ searchResult.targetMerchantCurrency }}</v-chip>

					<div v-if="searchResult.items && searchResult.items.length > 0" class="hint-row" style="display: inline-block">
						<v-icon class="ml-1" small color="">mdi-clock</v-icon>
						GMT+{{ userProfile.timeZone }}
					</div>

					<span class="f12 grey-text" style="display: inline-block"> {{ $t('TableHeader.__lastSearchTime') }}: {{ searchResult.serverTimeTW }} </span>
				</div>
			</div>

			<div class="hint-row" style="float: right">
				<v-icon> error </v-icon>
				{{ $t('Message.__dataDelay30Minutes') }}

				<v-icon class="ml-3"> visibility </v-icon>
				{{ $t('TableHeader.__bankViewPoint') }}
			</div>
		</div>

		<v-data-table id="table" :headers="headers" :items="searchResult.items" hide-default-footer v-if="isRenderTable" style="padding-bottom: 100px" :footer-props="footerProps" sort>
			<!-- <template v-slot:top>
				<v-toolbar>

				</v-toolbar>
			</template> -->

			<template v-slot:item.merchantName="{ item }">
				{{ item.merchantName }}
				<span v-if="item.isSelfCustomer">({{ $t('TableHeader.__directCustomerData') }}) </span>
			</template>
			<template v-slot:item.totalRound="{ item }">
				<div class="link-text-grey" v-if="hasSuperAdminPermission && hasPermission_IsWinLose_visible" @click="showAgentGameSummaryReport(item.merchantID, item.isSelfCustomer)">
					{{ item.totalRound | countDisplay }}
				</div>
				<div v-else>
					{{ item.totalRound | countDisplay }}
				</div>
			</template>

			<template v-slot:item.totalPlayerCount="{ item }">
				{{ item.totalPlayerCount | countDisplay }}
			</template>

			<template v-slot:item.betAmount="{ item }">
				{{ item.betAmount | amountDisplay }}
			</template>

			<template v-slot:item.validAmount="{ item }">
				{{ item.validAmount | amountDisplay }}
			</template>

			<template v-slot:item.invisibleColumn="{ item }"> - </template>

			<template v-slot:item.profitRate="{ item }">
				<span :class="item.profitRate < 0 ? 'minus-red' : ''">{{ item.profitRate | amountDisplay }}</span>
			</template>

			<template v-slot:item.payoutAmount="{ item }">{{ item.payoutAmount | amountDisplay }}</template>

			<template v-slot:item.merchantWinLossAmount="{ item }">
				<span :class="item.merchantWinLossAmount < 0 ? 'minus-red ' : ''">
					{{ item.merchantWinLossAmount | amountDisplay }}
				</span>
			</template>

			<template v-slot:item.eventCostAmount="{ item }">
				<span v-if="isFilterGames">-</span>
				<span v-else-if="item.isSelfCustomer">
					{{ item.eventCostAmount | amountDisplay }}
				</span>
				<span v-else>
					<span @click="showEventSummaryGroupByEventID(item.merchantID)" class="pointer link-text-grey">
						{{ item.eventCostAmount | amountDisplay }}
					</span>
				</span>
			</template>

			<template v-slot:item.merchantProfitWithCost="{ item }">
				<div v-if="isFilterGames">-</div>
				<div v-else>
					<span :class="item.merchantProfitWithCost < 0 ? 'minus-red ' : ''">
						{{ item.merchantProfitWithCost | amountDisplay }}
					</span>
				</div>
			</template>

			<template v-slot:item.rtp="{ item }">
				<span :class="item.rtp > 100 ? 'h-yellow  ' : ''">
					{{ item.rtp | amountDisplay }}
				</span>
			</template>

			<template v-slot:item.action="{ item }">
				<span class="link-text" v-if="!item.isSelfCustomer" @click="searchDetailAndChildsByMerchantID(item)">
					{{ $t('Button.__showDetail') }}
				</span>
			</template>

			<!-- <template slot="body.append" v-if="searchResult.totalSummary && dailyReports.length > 0">
				<tr>

					<td>
						{{ $t('TableHeader.__total') }}
						:
					</td>
					<td></td>
					<td class='text-r'>
						{{searchResult.totalSummary.totalRound | countDisplay}}
					</td>

					<td class='text-r'>
						{{searchResult.totalSummary.betAmount | amountDisplay}}
					</td>
					<td class='text-r'>
						{{searchResult.totalSummary.validAmount | amountDisplay}}
					</td>
					<td class='text-r'>
						{{searchResult.totalSummary.payoutAmount | amountDisplay}}
					</td>
					<td class='text-r'>
						{{searchResult.totalSummary.merchantWinLossAmount | amountDisplay}}
					</td>
					<td class='text-r'>
						{{searchResult.totalSummary.rtp | amountDisplay}}
					</td>

				</tr>
			</template> -->
		</v-data-table>

		<div class="mt-3" style="padding-bottom: 50px" v-if="searchResult.gameTypeSummaryItems && searchResult.gameTypeSummaryItems.length > 0">
			<h3 class="ml-2">{{ $t('TableHeader.__gameType') }}</h3>
			<v-data-table id="table2" :headers="headersOfGameSummary" style="width: 70%" :items="searchResult.gameTypeSummaryItems" hide-default-footer>
				<template v-slot:item.totalRound="{ item }">
					{{ item.totalRound | countDisplay }}
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

				<template v-slot:item.merchantWinLossAmount="{ item }">
					<span :class="item.merchantWinLossAmount < 0 ? 'minus-red ' : ''">
						{{ item.merchantWinLossAmount | amountDisplay }}
					</span>
				</template>
			</v-data-table>
		</div>

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
import MultipleGameSelectedDialog from '@/components/common-component/MultipleGameSelectedDialog.vue';

const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';
import { ChartDataTypeEnum } from '@/models/status/statistics';

// @ts-ignore
@Component({
	// @ts-ignore
	components: { NotificationWarning, DatePicker, ChartPopup, GameSummaryReportDataTableList, MultipleGameSelectedDialog, MerchantHierarchyTreeViewPartial },
	mixins: [commonDisplayFiltersMixin, datetimeMixin, commonMethodMixin],
})
export default class AgentTotalSummaryReport extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];

	CRUDActionEnums: typeof CRUDAction = CRUDAction;

	dialogVisible: DropdownModel.DialogVisible = new DropdownModel.DialogVisible();
	isShowReturnRoot: boolean = false;
	isRenderTable: boolean = true;
	isFilterGameManualFlag: boolean = false;

	currentMerchantNameText: string = '';

	popupExtraInfo: string = '';

	renderComponent: boolean = true;

	datetimeRangePicker: DropdownModel.DatetimeRangePicker = new DropdownModel.DatetimeRangePicker();

	searchFilter: Model.IQueryHierarchyAgentTotalReportReportApiRequestParameter = {} as Model.IQueryHierarchyAgentTotalReportReportApiRequestParameter;
	searchResult: Model.IQueryAgentSummaryReportByHierarchyChildMappingResult = {} as Model.IQueryAgentSummaryReportByHierarchyChildMappingResult;

	searchChildDetail: Model.IMerchantDailyReport = {} as Model.IMerchantDailyReport;

	footerProps: GameModel.IDataTablePageObject = {
		'items-per-page-options': [5000],
	} as GameModel.IDataTablePageObject;

	showNotificationWarning: boolean = false;
	panel: number[] = [0];

	dropdownGames: DropdownModel.IDropdownItems[] = [];
	dropdownMerchants: DropdownModel.IDropdownItems[] = [];
	dropdownAgentAggregativeType: DropdownModel.IDropdownItems[] = [];
	searchExtraColumnDropdownItems: DropdownModel.IDropdownItems[] = [];

	get hasSuperAdminPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSetting_SystemConfigSetting) >= 0;
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

	get headers() {
		const vm: any = this;
		const data = [
			{ text: '', value: 'action', align: 'center', sortable: false },
			{ text: this.$t('TableHeader.__merchantID'), value: 'merchantID', sortable: false },
			{ text: this.$t('TableHeader.__merchantName'), value: 'merchantName', sortable: false },
			{ text: this.$t('TableHeader.__totalRound'), value: 'totalRound', sortable: true, align: 'end' },
		];

		if (this.isArrayExistTargetID(this.searchFilter.includeExtraSearchInfo, 2501)) {
			data.push({ text: this.$t('TableHeader.__totalLoginPlayer'), value: 'totalPlayerCount', sortable: true, align: 'end' });
		}

		if (this.isArrayExistTargetID(this.searchFilter.includeExtraSearchInfo, 2502)) {
			data.push({ text: this.$t('TableHeader.__totalBetPlayer'), value: 'totalBettingPlayerCount', sortable: true, align: 'end' });
		}

		// 部分客戶希望不顯示特定欄位(莫名其妙的需求)
		data.push({ text: this.$t('TableHeader.__betAmount'), value: vm.visibleOrNot(this.hasPermission_IsBetAmount_visible, 'betAmount'), sortable: true, align: 'end' });
		data.push({ text: this.$t('TableHeader.__validAmount'), value: vm.visibleOrNot(this.hasPermission_IsBetAmount_visible, 'validAmount'), sortable: true, align: 'end' });

		data.push({ text: this.$t('TableHeader.__payoutAmount'), value: vm.visibleOrNot(this.hasPermission_IsWinLose_visible, 'payoutAmount'), sortable: true, align: 'end' });
		data.push({ text: this.$t('TableHeader.__gameWinLoss'), value: vm.visibleOrNot(this.hasPermission_IsWinLose_visible, 'merchantWinLossAmount'), sortable: true, align: 'end' });

		data.push({ text: this.$t('TableHeader.__promotionCost'), value: vm.visibleOrNot(this.hasPermission_IsWinLose_visible, 'eventCostAmount'), sortable: true, align: 'end' });
		data.push({ text: this.$t('TableHeader.__merchantProfit'), value: vm.visibleOrNot(this.hasPermission_IsWinLose_visible, 'merchantProfitWithCost'), sortable: true, align: 'end' });

		data.push({ text: this.$t('TableHeader.__profitRate'), value: vm.visibleOrNot(this.hasPermission_IsWinLose_visible, 'profitRate'), sortable: true, align: 'end' });

		return data;
	}

	get headersOfGameSummary() {
		const vm: any = this;
		const data = [
			{ text: this.$t('TableHeader.__gameType'), value: 'gameTypeText', sortable: false },
			{ text: this.$t('TableHeader.__totalRound'), value: 'totalRound', sortable: true, align: 'end' },
		];

		// 部分客戶希望不顯示特定欄位(莫名其妙的需求)
		data.push({ text: this.$t('TableHeader.__betAmount'), value: vm.visibleOrNot(this.hasPermission_IsBetAmount_visible, 'betAmount'), sortable: true, align: 'end' });
		data.push({ text: this.$t('TableHeader.__validAmount'), value: vm.visibleOrNot(this.hasPermission_IsBetAmount_visible, 'validAmount'), sortable: true, align: 'end' });
		data.push({ text: this.$t('TableHeader.__payoutAmount'), value: vm.visibleOrNot(this.hasPermission_IsWinLose_visible, 'payoutAmount'), sortable: true, align: 'end' });
		data.push({ text: this.$t('TableHeader.__gameWinLoss'), value: vm.visibleOrNot(this.hasPermission_IsWinLose_visible, 'merchantWinLossAmount'), sortable: true, align: 'end' });
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

	get dynamicMerchantDropdownWithKeyword() {
		const vm: any = this;

		console.log(vm.dropdownMerchants);
		console.log('----------------');

		const myItems = vm.filterDropdownItemsByKeywordSearch(vm.dropdownMerchants, vm.globalKeywordSearchMerchantsDropdown, vm.searchFilter.merchantID);

		console.log(myItems);
		return myItems;
	}

	get isFilterGames() {
		// 感覺vue監控 array怪怪的 需要手動處理
		return this.isFilterGameManualFlag;
	}

	whenSelectFilterGame() {
		const vm: any = this;

		if (vm.isEmptyCollection(this.searchFilter.gameIDs)) {
			this.isFilterGameManualFlag = false;
		} else {
			this.isFilterGameManualFlag = true;
		}
	}

	showRefMultipleGameSelectedDialog() {
		const targerRef: any = this.$refs.refMultipleGameSelectedDialog;
		targerRef.showDialog(this.dropdownGames);
	}

	confirmMultipleSelectedGames(gameIDs: number[]) {
		console.log('trigger parent');
		console.log(gameIDs);

		if (gameIDs != null && gameIDs.length > 0) {
			this.searchFilter.gameIDs = [];
			for (const id of gameIDs) {
				this.searchFilter.gameIDs.push(id);
			}

			this.reloadPage();
			this.whenSelectFilterGame();

			if (this.searchFilter.merchantID != null && this.searchFilter.merchantID > 0) {
				this.refresh();
			}
		}
	}

	isArrayExistTargetID(array: number[], targetID: number) {
		if (array == null) {
			return false;
		}
		if (array.length <= 0) {
			return false;
		}
		return array.includes(targetID);
	}

	showAgentGameSummaryReport(myMerchantID: number, isSelfCustomer: boolean) {
		const myFilter: any = {
			querySqlID: 900177,
			isQueryStraightCustomerOnly: isSelfCustomer,
			merchantID: myMerchantID,
			beginTime: this.searchFilter.begin,
			endTime: this.searchFilter.end,
			timeZone: this.searchFilter.timeZone,
		};

		const myConfig: any = {
			maxWidth: 1200,
		};

		EventBus.$emit(`globalShowCustomizedColumnsDisplayPopupDialog`, myFilter, myConfig);
	}

	showEventSummaryGroupByEventID(myMerchantID: number) {
		const myFilter: any = {
			querySqlID: 900179,
			merchantID: myMerchantID,
			beginTime: this.searchFilter.begin,
			endTime: this.searchFilter.end,
		};

		const myConfig: any = {
			maxWidth: 950,
		};

		EventBus.$emit(`globalShowCustomizedColumnsDisplayPopupDialog`, myFilter, myConfig);
	}

	geti18nTextAsString(key: string) {
		return this.$t(key).toString();
	}

	clear() {
		this.searchFilter = {} as Model.IQueryHierarchyAgentTotalReportReportApiRequestParameter;
		this.searchFilter.timeZone = this.userProfile.timeZone;
		this.searchFilter.isExcludeZeroData = true;
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
		this.dialogVisible.resetAllValueToFalse();
	}

	refresh() {
		EventBus.$emit('show_mask', null);

		this.searchFilter.begin = this.datetimeRangePicker.searchStartTimeRange[0] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[0]) : null;
		this.searchFilter.end = this.datetimeRangePicker.searchStartTimeRange[1] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[1]) : null;

		// 轉成當前時區的日期
		MerchantSummaryApi.getAgentHierartchyTotalSummaryReport(this.searchFilter)
			.then((res) => {
				this.searchResult = res.data;
				this.isShowReturnRoot = false;
				this.currentMerchantNameText = '';
				this.reloadTableRender();
			})
			.finally(() => {
				setTimeout(() => {
					EventBus.$emit('hide_mask', null);
				}, 500);
			});
	}

	searchDetailAndChildsByMerchantIDAndName(merchantID: number, name: string) {
		const model = {} as Model.IQueryDirectAgentSummaryReportByHierarchyChild;
		model.merchantID = merchantID;
		model.merchantName = name;

		this.searchDetailAndChildsByMerchantID(model);
	}

	searchDetailAndChildsByMerchantID(item: Model.IQueryDirectAgentSummaryReportByHierarchyChild) {
		EventBus.$emit('show_mask', null);

		this.searchFilter.begin = this.datetimeRangePicker.searchStartTimeRange[0] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[0]) : null;
		this.searchFilter.end = this.datetimeRangePicker.searchStartTimeRange[1] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[1]) : null;

		const myFilter = { ...this.searchFilter };
		myFilter.merchantID = item.merchantID;

		// 轉成當前時區的日期
		MerchantSummaryApi.getAgentHierartchyTotalSummaryReportDetail(myFilter)
			.then((res) => {
				this.searchResult = res.data;
				this.isShowReturnRoot = true;
				this.currentMerchantNameText = item.merchantName + ' (' + this.geti18nTextAsString('Button.__showDetail') + ')';
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	async exportFiles() {
		const excelHeaders: any = [
			this.geti18nTextAsString('TableHeader.__merchantID'),
			this.geti18nTextAsString('TableHeader.__merchantName'),
			this.geti18nTextAsString('TableHeader.__totalRound'),
			this.geti18nTextAsString('TableHeader.__totalLoginPlayer'),
			this.geti18nTextAsString('TableHeader.__betAmount'),
			this.geti18nTextAsString('TableHeader.__validAmount'),
			this.geti18nTextAsString('TableHeader.__payoutAmount'),
			this.geti18nTextAsString('TableHeader.__gameWinLoss'),
			this.geti18nTextAsString('TableHeader.__promotionCost'),
			this.geti18nTextAsString('TableHeader.__merchantProfit'),
			this.geti18nTextAsString('TableHeader.__profitRate'),
		];
		const excelData: any = [];
		const vm: any = this;

		this.searchResult.items.forEach((value) => {
			const eachRow: any = [];
			eachRow.push(value.merchantID);

			let text = value.merchantName;

			if (value.isSelfCustomer) {
				text = text + ' (' + this.geti18nTextAsString('TableHeader.__directCustomerData') + ')';
			}

			eachRow.push(text);
			eachRow.push(value.totalRound);
			eachRow.push(value.totalPlayerCount === 0 ? '-' : value.totalPlayerCount.toString());

			eachRow.push(vm.visibleOrDashValue(this.hasPermission_IsBetAmount_visible, value.betAmount));
			eachRow.push(vm.visibleOrDashValue(this.hasPermission_IsBetAmount_visible, value.validAmount));
			eachRow.push(vm.visibleOrDashValue(this.hasPermission_IsWinLose_visible, value.payoutAmount));
			eachRow.push(vm.visibleOrDashValue(this.hasPermission_IsWinLose_visible, value.merchantWinLossAmount));

			if (this.isFilterGames) {
				eachRow.push('-');
				eachRow.push('-');
			} else {
				eachRow.push(vm.visibleOrDashValue(this.hasPermission_IsWinLose_visible, value.eventCostAmount));
				eachRow.push(vm.visibleOrDashValue(this.hasPermission_IsWinLose_visible, value.merchantProfitWithCost));
			}

			eachRow.push(vm.visibleOrDashValue(this.hasPermission_IsWinLose_visible, value.profitRate));
			excelData.push(eachRow);
		});

		const config = {
			headers: excelHeaders,
			rowData: excelData,
			fileName: 'Export_AgentHierarchyTotalReport' + this.searchResult.exportFileSuffixText + '.xlsx',
		};

		// @ts-ignore
		this.downloadExcel(config);
	}

	mounted() {
		DropdownApi.getHierarchyMerchants().then((res) => {
			this.dropdownMerchants = res;

			if (this.dropdownMerchants.length === 1) {
				const selectValue: any = this.dropdownMerchants[0].valueInt;
				this.searchFilter.merchantID = selectValue;
			}
		});

		DropdownApi.getAgentAggregativeType().then((res) => {
			this.dropdownAgentAggregativeType = res;
		});

		DropdownApi.getStatReportSearchColumns('agent-total-report').then((res) => {
			this.searchExtraColumnDropdownItems = res;
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
</style>
