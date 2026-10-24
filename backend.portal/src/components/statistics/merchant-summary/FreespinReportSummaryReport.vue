<template>
	<v-app v-if="renderComponent" class="app">
		<v-expansion-panels v-model="panel" multiple>
			<v-expansion-panel>
				<v-expansion-panel-content>
					<v-flex v-if="isRenderForm">
						<v-row>
							<v-col cols="12" sm="3" md="3">
								<div class="" style="margin-top: 15px">
									<v-autocomplete
										clearable
										:items="dropdownVendorConfig"
										v-model="searchFilter.vendorConfigID"
										class="dropdownVendorConfig"
										:label="$t('TableHeader.__currency') + ''"
									>
									</v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="" style="margin-top: 15px">
									<v-autocomplete clearable :items="dropdownMerchants" v-model="searchFilter.merchantID" class="merchantDropdownItems" :label="$t('TableHeader.__merchant') + ''">
									</v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="">
									<v-autocomplete :items="dropdownGames" clearable chips :label="$t('TableHeader.__game')" v-model="searchFilter.gameID"> </v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="">
									<v-autocomplete :items="dropdownPrizeTypes" clearable chips :label="$t('TableHeader.__type')" v-model="searchFilter.eventPrizeType"> </v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="minus-mt-4 w95">
									<date-picker v-model="datetimeRangePicker.searchStartTimeRange" type="datetime" value-type="timestamp" range :placeholder="$t('TableHeader.__time')"> </date-picker>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3" class="quick-time">
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

							<v-col cols="12" sm="3" md="3"> </v-col>

							<v-col cols="12" sm="3" md="3" style="padding: 0px">
								<div class="">
									<v-btn color="yellow darken-1" text class="btn search" @click.stop="search">
										<v-icon left>search</v-icon>
										{{ $t('Button.__search') }} </v-btn
									>&#9;
									<v-btn color="pink lighten-3" text class="btn search" @click.stop="clear">
										<v-icon left>clear</v-icon>
										{{ $t('Button.__reset') }}
									</v-btn>

									<v-btn color="blue lighten-3" text class="btn search" @click.stop="exportExcel">
										<v-icon left>file_download</v-icon>
										{{ $t('Button.__downloadFile') }}
									</v-btn>
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

			<span class="f12 grey-text ml-3" style="display: inline-block" v-if="searchResult.serverTimeTW"> {{ $t('TableHeader.__lastSearchTime') }}: {{ searchResult.serverTimeTW }} </span>
		</div>

		<div v-if="searchResult && searchResult.items && searchResult.items.length > 0">
			<v-data-table id="table" :headers="headers" :items="searchResult.items" :footer-props="footerProps" hide-default-footer sort>
				<template v-slot:item.gameID="{ item }">
					<span v-if="item.gameID > 0">
						{{ item.gameID }}
					</span>
					<span v-else> - </span>
				</template>

				<template v-slot:item.totalValidAmount="{ item }">
					<span v-if="item.totalValidAmount > 0">
						{{ item.totalValidAmount | amountDisplay }}
					</span>
					<span v-else> - </span>
				</template>

				<template v-slot:item.totalRound="{ item }">
					{{ item.totalRound | countDisplay }}
				</template>

				<template v-slot:item.totalLoginCount="{ item }">
					{{ item.totalLoginCount | countDisplay }}
				</template>

				<template v-slot:item.totalAmount="{ item }">
					{{ item.totalAmount | amountDisplay }}
				</template>

				<template v-slot:item.winLossAmount="{ item }">
					{{ item.winLossAmount | amountDisplay }}
				</template>

				<template v-slot:item.payoutAmount="{ item }">
					{{ item.payoutAmount | amountDisplay }}
				</template>

				<template v-slot:item.promotionCostAmount="{ item }">
					<span v-if="isFilterGames">-</span>
					<span v-else>{{ item.promotionCostAmount | amountDisplay }}</span>
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
				<!-- 
				<template slot="body.append" v-if="totalSummaryRow">
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

						<td class="text-r" v-if="hasPermission_IsBetAmount_visible">
							{{ dailyReportsTotalSummary.betAmount | amountDisplay }}
						</td>
						<td class="text-r" v-if="hasPermission_IsBetAmount_visible">
							{{ dailyReportsTotalSummary.validAmount | amountDisplay }}
						</td>
						<td class="text-r" v-if="hasPermission_IsBetAmount_visible">
							{{ visibleOrDashValue(hasPermission_IsWinLose_visible, dailyReportsTotalSummary.payoutAmount) | amountDisplay }}
						</td>
					</tr>
				</template>
 -->
			</v-data-table>
		</div>

		<v-dialog v-model="dialog" v-if="dialog"> </v-dialog>
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import { State, Action, Getter, namespace } from 'vuex-class';
import EventBus from '@/utilities/event-bus';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { commonMethodMixin } from '@/utilities/common-methods';
import DropdownApi from '@/api/commons/dropdown';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import { datetimeMixin, startEndDate, timeWithTimezone, daysAgo, weeksAgo, monthsAgo } from '@/utilities/datetime-format';
import TextareaParseToListIDPopup from '@/components/system-setting/TextareaParseToListIDPopup.vue';
import * as Model from '@/models/interfaces/statistics/merchant-summary';
import * as GameModel from '@/models/interfaces/game-setting/switch';
import MerchantSummaryApi from '@/api/statistics/merchant-summary';

import DatePicker from 'vue2-datepicker';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import * as Permission from '@/models/status/permission';

const authModule = namespace('Auth');

// @ts-ignore
@Component({ mixins: [datetimeMixin, commonDisplayFiltersMixin, commonMethodMixin], components: { TextareaParseToListIDPopup, DatePicker } })
export default class FreespinReport extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];

	panel: number[] = [0];
	dialogVisible: DropdownModel.DialogVisible = new DropdownModel.DialogVisible();
	renderComponent: boolean = true;
	isRenderForm: boolean = true;

	searchFilter: Model.IFreespinReportRequestModel = {} as Model.IFreespinReportRequestModel;
	searchResult: Model.IFreespinReportResponseModel = {} as Model.IFreespinReportResponseModel;
	totalSummaryRow: any = {};
	datetimeRangePicker: DropdownModel.DatetimeRangePicker = new DropdownModel.DatetimeRangePicker();

	searchExtraColumnDropdownItems: DropdownModel.IDropdownItems[] = [];
	dropdownMerchantsWithKeyword: DropdownModel.IDropdownItems[] = [];
	dropdownMerchants: DropdownModel.IDropdownItems[] = [];
	dropdownGames: DropdownModel.IDropdownItems[] = [];
	dropdownStatus: DropdownModel.IDropdownItems[] = [];
	dropdownVendorConfig: DropdownModel.IDropdownItems[] = [];
	dropdownPrizeTypes: DropdownModel.IDropdownItems[] = [];

	merchantsSearchKeyword: string = '';
	dialog: boolean = false;
	isMounted: boolean = false;
	title: string = '';
	datasets: any[] = [];

	footerProps: GameModel.IDataTablePageObject = {
		'items-per-page-options': [10000],
	} as GameModel.IDataTablePageObject;

	get headers() {
		return [
			{ text: this.$t('TableHeader.__action'), value: 'action', sortable: false, width: '8%' },
			{ text: this.$t('TableHeader.__merchantID'), value: 'merchantID', width: '6%' },
			{ text: this.$t('TableHeader.__merchant'), value: 'merchantName', sortable: false, width: '8%' },
			{ text: this.$t('TableHeader.__currency'), value: 'currency', sortable: false, width: '5%' },
			{ text: this.$t('TableHeader.__type'), value: 'eventPrizeTypeName', width: '8%' },
			{ text: this.$t('TableHeader.__gameID'), value: 'gameID', width: '6%' },
			{ text: this.$t('TableHeader.__game'), value: 'gameName', sortable: false, width: '8%' },

			{ text: this.$t('TableHeader.__totalCount'), value: 'totalCount' },
			{ text: this.$t('TableHeader.__totalPeopleCount'), value: 'totalPlayers' },
			{ text: this.$t('TableHeader.__validAmount'), value: 'totalValidAmount', align: 'end' },
			{ text: this.$t('TableHeader.__playerWinLoss'), value: 'winLossAmount', align: 'end' },
		];
	}

	get hasPermissionSuperAdmin() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSetting_SystemConfigSetting) >= 0;
	}

	get isAllowMultipleSelected() {
		return this.hasPermissionSuperAdmin;
	}

	showParseTextToListIntDialog() {
		const targetRef: any = this.$refs.refTextareaParseToListIDPopup;
		targetRef.showParseTextareaPraseToIDs();
	}

	copyPlayerIDs(item: Model.IBetAmountDistributeItem) {
		const vm: any = this;
		const playerIDs = item.playerIDs.join();

		if (item.playerIDs.length <= 0) {
			return;
		}

		vm.copyTextToClipboard2(playerIDs);
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

	isSimplifyDropdownItems() {
		const vm: any = this;
		return vm.isEnableSimplifyDropdown();
	}

	registerReloadComponent() {
		EventBus.$on(`reload-${this.$router.currentRoute.path}`, (info: any) => {
			this.renderComponent = false;
			this.$nextTick(() => {
				this.clear();

				this.renderComponent = true;
			});
		});
	}

	search() {
		this.searchFilter.begin = this.datetimeRangePicker.searchStartTimeRange[0] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[0]) : null;
		this.searchFilter.end = this.datetimeRangePicker.searchStartTimeRange[1] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[1]) : null;

		EventBus.$emit('show_mask', null);

		MerchantSummaryApi.getFreespinReport(this.searchFilter)
			.then((res) => {
				this.searchResult = res.data;
				this.totalSummaryRow = res.data.totalSummary;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	async exportExcel() {
		const excelHeaders: any[] = this.headers
			.filter((x) => x.value !== 'action')
			.map((header) => {
				return header.text;
			});

		const excelData: any = [];

		this.searchFilter.begin = this.datetimeRangePicker.searchStartTimeRange[0] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[0]) : null;
		this.searchFilter.end = this.datetimeRangePicker.searchStartTimeRange[1] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[1]) : null;

		EventBus.$emit('show_mask', null);

		const queryResult: any = await MerchantSummaryApi.getFreespinReport(this.searchFilter).finally(() => {
			EventBus.$emit('hide_mask', null);
		});

		console.log(queryResult);

		queryResult.data.items.forEach((value: any) => {
			const eachRow: any = [];

			eachRow.push(value.merchantID);
			eachRow.push(value.merchantName);
			eachRow.push(value.currency);
			eachRow.push(value.eventPrizeTypeName);

			eachRow.push(value.gameID > 0 ? value.gameID : '-');
			eachRow.push(value.gameName);
			eachRow.push(value.totalCount);
			eachRow.push(value.totalPlayers);
			eachRow.push(value.totalValidAmount > 0 ? value.totalValidAmount : '-');
			eachRow.push(value.winLossAmount);

			excelData.push(eachRow);
		});

		const config = {
			headers: excelHeaders,
			rowData: excelData,
			fileName: 'Export_FreespinSummary_' + this.searchFilter.vendorConfigID + '.xlsx',
		};

		// @ts-ignore
		this.downloadExcel(config);
	}

	clear() {
		this.searchFilter = {} as Model.IFreespinReportRequestModel;
	}

	mounted() {
		this.registerReloadComponent();
		this.init();
	}

	init() {
		this.datetimeRangePicker.searchStartTimeRange = [daysAgo(0), daysAgo(1)];

		DropdownApi.getMerchants(false).then((res) => {
			this.dropdownMerchants = res;

			if (this.dropdownMerchants.length === 1) {
				const selectValue: any = this.dropdownMerchants[0].valueInt;
			}
		});

		// 管理員才會用這種模式
		DropdownApi.getGames().then((res) => {
			this.dropdownGames = res;
		});

		DropdownApi.GetAGGPrizeTypes().then((res) => {
			this.dropdownPrizeTypes = res;
		});

		DropdownApi.getVendorConfigCurrencyDisplay(290000).then((res) => {
			this.dropdownVendorConfig = res;
			if (this.dropdownVendorConfig.length === 1) {
				const selectValue: any = this.dropdownVendorConfig[0].valueInt;
				this.searchFilter.vendorConfigID = selectValue;
			}
		});

		DropdownApi.getStatReportSearchColumns('merchant-summary-report').then((res) => {
			this.searchExtraColumnDropdownItems = res;
		});
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

td.text-end > span {
	//padding-right: 16px !important;
}
</style>
