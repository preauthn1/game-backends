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
								<div class="mt-4">
									<v-text-field clearable chips :label="$t('TableHeader.__freespinID')" v-model="searchFilter.freespinID"> </v-text-field>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="mt-4">
									<v-text-field clearable chips :label="$t('TableHeader.__playerAccount')" v-model="searchFilter.playerName"> </v-text-field>
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
							<v-col cols="12" sm="3" md="3">
								<div class="minus-mt-4">
									<v-autocomplete :items="dropdownFreespinStatus" clearable chips :label="$t('TableHeader.__status')" v-model="searchFilter.freeSpinUseStatus"> </v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3" style="padding: 0px">
								<div class="ml-4">
									<v-btn color="yellow darken-1" text class="btn search" @click.stop="pageOneSearch">
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

		<div class="hint-row"></div>

		<div v-if="searchItems && searchItems && searchItems.length > 0">
			<v-data-table
				:items="searchItems"
				:headers="headers"
				id="table"
				:disable-sort="true"
				:page="DataTablePagingParameter.pageIndex"
				:items-per-page="DataTablePagingParameter.itemsPerPage"
				:server-items-length="DataTablePagingParameter.totalCount"
				:footer-props="DataTablePagingParameter.footerProps"
				@update:page="changePage"
				@update:items-per-page="getItemPerPage"
			>
				<template v-slot:item.action="{ item }">
					<div @click="showDetailLogs(item.freespinID, item.playerID)" class="link-text-grey">
						{{ $t('Button.__showDetail') }}
					</div>
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

				<template v-slot:item.totalValidAmount="{ item }">
					{{ item.totalValidAmount | amountDisplay }}
				</template>

				<template v-slot:item.winLossAmount="{ item }">
					{{ item.winLossAmount | amountDisplay }}
				</template>

				<template v-slot:item.payoutAmount="{ item }">
					{{ item.payoutAmount | amountDisplay }}
				</template>

				<template v-slot:item.createTime="{ item }">
					{{ item.createTime | timeWithAddTimezone(userProfile.timeZone) }}
				</template>

				<template v-slot:item.rtp="{ item }">
					<span :class="item.rtp > 100 ? 'h-yellow  ' : ''">
						{{ item.rtp | amountDisplay }}
					</span>
				</template>
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
	selectMerchant: number = 0;
	selectMerchantIDs: number[] = [];
	renderComponent: boolean = true;
	isRenderForm: boolean = true;

	DataTablePagingParameter: DropdownModel.DataTablePagingParameter = new DropdownModel.DataTablePagingParameter();

	searchFilter: Model.IFreespinStatusListRequestModel = {} as Model.IFreespinStatusListRequestModel;
	searchItems: any[] = [];
	totalSummaryRow: any = {};
	datetimeRangePicker: DropdownModel.DatetimeRangePicker = new DropdownModel.DatetimeRangePicker();

	searchExtraColumnDropdownItems: DropdownModel.IDropdownItems[] = [];
	dropdownMerchantsWithKeyword: DropdownModel.IDropdownItems[] = [];
	dropdownMerchants: DropdownModel.IDropdownItems[] = [];
	dropdownGames: DropdownModel.IDropdownItems[] = [];
	dropdownStatus: DropdownModel.IDropdownItems[] = [];
	dropdownVendorConfig: DropdownModel.IDropdownItems[] = [];
	dropdownFreespinStatus: DropdownModel.IDropdownItems[] = [];
	dropdownPrizeTypes: DropdownModel.IDropdownItems[] = [];

	merchantsSearchKeyword: string = '';
	dialog: boolean = false;
	isMounted: boolean = false;
	title: string = '';
	datasets: any[] = [];

	footerProps: GameModel.IDataTablePageObject = {
		'items-per-page-options': [50],
	} as GameModel.IDataTablePageObject;

	get headers() {
		return [
			{ text: '詳細', value: 'action', align: 'end', width: '8%' },
			{ text: this.$t('TableHeader.__createTime'), value: 'createTime', width: '8%' },
			{ text: this.$t('TableHeader.__freespinID'), value: 'freespinID', align: 'center', width: '8%' },
			{ text: this.$t('TableHeader.__merchantID'), value: 'merchantID', sortable: false, width: '8%' },
			{ text: this.$t('TableHeader.__merchant'), value: 'merchantName', sortable: false, width: '8%' },
			{ text: this.$t('TableHeader.__playerID'), value: 'playerID', sortable: false, width: '8%' },
			{ text: this.$t('TableHeader.__playerAccount'), value: 'playerName', sortable: false, width: '8%' },
			{ text: this.$t('TableHeader.__status'), value: 'freeSpinUseStatus', sortable: false, width: '8%' },

			{ text: this.$t('TableHeader.__remainingRound'), value: 'lastRounds', width: '10%' },
			{ text: this.$t('TableHeader.__totalRound'), value: 'totalRounds', width: '10%' },
			{ text: this.$t('TableHeader.__payoutAmount'), value: 'totalWin', align: 'end', width: '10%' },
			{ text: ' ', value: 'xxxxxxx', align: 'end' },
		];
	}

	get hasPermissionSuperAdmin() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSetting_SystemConfigSetting) >= 0;
	}

	get isAllowMultipleSelected() {
		return this.hasPermissionSuperAdmin;
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

	showParseTextToListIntDialog() {
		const targetRef: any = this.$refs.refTextareaParseToListIDPopup;
		targetRef.showParseTextareaPraseToIDs();
	}

	setResultMerchantIDs(IDs: number[]) {
		this.selectMerchantIDs = [];
		for (const id of IDs) {
			this.selectMerchantIDs.push(id);
		}
		this.renderComponent = false;
		this.$nextTick(() => {
			this.renderComponent = true;
		});
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

	pageOneSearch() {
		this.DataTablePagingParameter.pageIndex = 1;
		this.search();
	}

	search() {
		this.searchFilter.begin = this.datetimeRangePicker.searchStartTimeRange[0] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[0]) : null;
		this.searchFilter.end = this.datetimeRangePicker.searchStartTimeRange[1] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[1]) : null;

		EventBus.$emit('show_mask', null);

		this.searchFilter.pageIndex = this.DataTablePagingParameter.pageIndex;
		this.searchFilter.pageSize = this.DataTablePagingParameter.itemsPerPage;

		MerchantSummaryApi.getFreespinStatusList(this.searchFilter)
			.then((res) => {
				this.searchItems = res.data.items;
				this.DataTablePagingParameter.totalCount = res.data.totalCount;
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

		this.searchFilter.pageIndex = 1;
		this.searchFilter.pageSize = 5000;

		EventBus.$emit('show_mask', null);

		const queryResult: any = await MerchantSummaryApi.getFreespinStatusList(this.searchFilter).finally(() => {
			EventBus.$emit('hide_mask', null);
		});

		queryResult.data.items.forEach((value: any) => {
			const eachRow: any = [];

			eachRow.push(timeWithTimezone(value.createTime, this.userProfile.timeZone));
			eachRow.push(value.freespinID);
			eachRow.push(value.merchantID);
			eachRow.push(value.merchantName);
			eachRow.push(value.playerID);
			eachRow.push(value.playerName);
			eachRow.push(value.freeSpinUseStatus);
			eachRow.push(value.lastRounds);
			eachRow.push(value.totalRounds);
			eachRow.push(value.totalWin);

			excelData.push(eachRow);
		});

		const config = {
			headers: excelHeaders,
			rowData: excelData,
			fileName: 'Export_FreespinStatusList_' + this.searchFilter.vendorConfigID + '.xlsx',
		};

		// @ts-ignore
		this.downloadExcel(config);
	}

	showDetailLogs(myFreespinID: string, myPlayerID: number) {
		const myFilter: any = {
			querySqlID: 900171,
			freespinID: myFreespinID,
			playerID: myPlayerID,
		};

		const config: any = {
			maxWidth: 1100,
		};

		EventBus.$emit(`globalShowCustomizedColumnsDisplayPopupDialog`, myFilter, config);
	}

	clear() {
		const tempVendorConfig = this.searchFilter.vendorConfigID;
		this.searchFilter = {} as Model.IFreespinStatusListRequestModel;
		this.searchFilter.vendorConfigID = tempVendorConfig;
		this.selectMerchant = 0;
		this.selectMerchantIDs = [];
	}

	mounted() {
		this.registerReloadComponent();
		this.init();
	}

	init() {
		this.datetimeRangePicker.searchStartTimeRange = [daysAgo(0), daysAgo(1)];

		DropdownApi.getMerchants(false).then((res) => {
			this.dropdownMerchants = res;

			console.log('XXXXXXXXXXXXXXX');
			console.log(this.dropdownMerchants);

			if (this.dropdownMerchants.length === 1) {
				const selectValue: any = this.dropdownMerchants[0].valueInt;
				this.selectMerchant = selectValue;
			}
		});

		// 管理員才會用這種模式
		DropdownApi.getGames().then((res) => {
			this.dropdownGames = res;
		});

		DropdownApi.GetAGGFreespinStatus().then((res) => {
			this.dropdownFreespinStatus = res;
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
