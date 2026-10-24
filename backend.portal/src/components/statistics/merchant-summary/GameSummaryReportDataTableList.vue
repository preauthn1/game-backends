<template>
	<div>
		<div v-if="apiResultObject.isShowGoToAgentReportHint" style="padding: 15px; font-size: 15px">
			<div class="hint-go-to-agent-report">
				<v-icon color="red ligthen-1" small>error</v-icon>
				{{ $t('Message.__thisAgentIsTreeModePleaseUseAgentReport') }}
			</div>
		</div>

		<v-row v-if="!apiResultObject.isShowGoToAgentReportHint">
			<v-col v-for="(table, index) in result" :key="index" cols="12" sm="12" md="12" lg="12" class="padding-top-0">
				<v-data-table id="table.merchantName" :headers="headers" :items="table.items" sort :items-per-page="1000" hide-default-footer>
					<!-- 					
					<template v-slot:top>
						<v-toolbar>
							<v-toolbar-title>
								<h4>
									{{ table.merchantName }}

									<span class="f14 ml-2" style="color:#AAA;">
										{{extraMemo}}
									</span>

								</h4>
							</v-toolbar-title>
							<v-divider class="mx-4" inset vertical></v-divider>
							<v-spacer></v-spacer>
						</v-toolbar>
					</template> -->

					<template v-slot:header.totalRound>
						<span>
							{{ $t('TableHeader.__totalRound') }}
						</span>
						<!-- <span @click="showTotalRoundChart(table.merchantID)">
							<v-tooltip top color="teal darken-1">
								<template v-slot:activator="{ on }">
									<a v-on="on">
										<v-icon color="primary" small left>bar_chart</v-icon>
									</a>
								</template>
								<span>{{ $t('Button.__showDetail') }}</span>
							</v-tooltip>
						</span> -->
					</template>

					<template v-slot:header.onlinePlayer>
						<span>
							{{ $t('TableHeader.__totalBetPlayer') }}
						</span>
					</template>

					<template v-slot:item.gameName="{ item }">
						<div>
							{{ item.gameName }}
						</div>

						<div class="sub-title-text">
							{{ item.gameNameEN }}
						</div>

						<!-- <v-tooltip top color="teal darken-1">
							<template v-slot:activator="{ on }">
								<a v-on="on" v-show="hasPermissionGameDetail" @click="openGameDetail(item.gameID)">
									<v-icon color="primary" small left>mdi-link</v-icon>
									{{ item.gameName }}
								</a>
							</template>
							<span>{{ $t('Button.__openLink') }}</span>
						</v-tooltip>
						<div v-show="!hasPermissionGameDetail">{{ item.gameName }}</div>
						 -->
					</template>
					<template v-slot:item.totalRound="{ item }">{{ item.totalRound | countDisplay }}</template>
					<template v-slot:item.onlinePlayer="{ item }">
						<span v-if="hasPermissionSuperAdmin && !isSearchMergeHierarchyAgent" @click="showSpecificGameReportPlayers(item.gameID)" class="link-text-grey">
							{{ item.onlinePlayer | countDisplay }}
						</span>
						<span v-else>
							{{ item.onlinePlayer | countDisplay }}
						</span>
					</template>
					<template v-slot:item.updateAmount="{ item }">
						<span :class="item.updateAmount < 0 ? 'minus-red ' : ''">
							{{ item.updateAmount | amountDisplay }}
						</span>
					</template>

					<template v-slot:item.merchantWinLoss="{ item }">
						<span :class="item.merchantWinLoss < 0 ? 'minus-red ' : ''">
							{{ item.merchantWinLoss | amountDisplay }}
						</span>
					</template>

					<template v-slot:item.betAmount="{ item }">{{ item.betAmount | amountDisplay }}</template>
					<template v-slot:item.validAmount="{ item }">{{ item.validAmount | amountDisplay }}</template>
					<template v-slot:item.payoutAmount="{ item }">{{ item.payoutAmount | amountDisplay }}</template>

					<template v-slot:item.rtp="{ item }">
						<span :class="item.rtp > 100 ? 'h-yellow  ' : ''">
							{{ item.rtp | amountDisplay }}
						</span>
					</template>

					<template v-slot:item.invisibleColumn="{ item }"> - </template>

					<template v-slot:item.taxAmount="{ item }">
						{{ item.taxAmount | amountDisplay }}
					</template>
					<template v-slot:item.jackpotPoolAmount="{ item }">{{ item.jackpotPoolAmount | amountDisplay }} </template>

					<template slot="body.append" v-if="table.totalSummary">
						<tr>
							<td>
								{{ $t('TableHeader.__total') }}
								:
							</td>
							<td></td>

							<td class="text-r"></td>
							<td class="text-r" v-if="!isHideTotalPlayersColumn"></td>

							<td class="text-r">
								{{ table.totalSummary.totalRound | countDisplay }}
							</td>

							<td class="text-r">
								{{ table.totalSummary.betAmount | amountDisplay }}
							</td>

							<td class="text-r">
								{{ table.totalSummary.validAmount | amountDisplay }}
							</td>

							<td class="text-r">
								{{ visibleOrDashValue(hasPermission_IsWinLose_visible, table.totalSummary.payoutAmount) | amountDisplay }}
							</td>

							<td class="text-r" :class="table.totalSummary.updateAmount < 0 ? 'minus-red ' : ''">
								{{ visibleOrDashValue(hasPermission_IsWinLose_visible, table.totalSummary.updateAmount) | amountDisplay }}
							</td>

							<td class="text-r" :class="table.totalSummary.merchantWinLoss < 0 ? 'minus-red ' : ''">
								{{ visibleOrDashValue(hasPermission_IsRTP_visible, table.totalSummary.merchantWinLoss) | amountDisplay }}
							</td>

							<td class="text-r" :class="table.totalSummary.rtp > 100 ? 'h-yellow   ' : ''">
								{{ visibleOrDashValue(hasPermission_IsRTP_visible, table.totalSummary.rtp) | amountDisplay }}
							</td>

							<!-- 
							<td class='text-r'>
								{{table.totalSummary.taxAmount | amountDisplay}}
							</td> -->
						</tr>
					</template>
				</v-data-table>

				<v-data-table
					id="table2"
					class="mt-4"
					:headers="headersOfCategorySummary"
					v-if="table.categoryReportItems && table.categoryReportItems.length > 0"
					:items="table.categoryReportItems"
					:items-per-page="1000"
					hide-default-footer
				>
					<template v-slot:top>
						<v-toolbar>
							<v-toolbar-title>
								<h4>
									{{ $t('Menu.Statistics') }} ({{ $t('TableHeader.__gameType') }})

									<span class="f14 ml-2" style="color: #aaa">
										{{ extraMemo }}
									</span>
								</h4>
							</v-toolbar-title>
							<v-divider class="mx-4" inset vertical></v-divider>
							<v-spacer></v-spacer>
						</v-toolbar>
					</template>

					<template v-slot:item.totalRound="{ item }">
						{{ item.totalRound | countDisplay }}
					</template>

					<template v-slot:item.updateAmount="{ item }">
						<span :class="item.updateAmount < 0 ? 'minus-red ' : ''">
							{{ item.updateAmount | amountDisplay }}
						</span>
					</template>

					<template v-slot:item.merchantWinLoss="{ item }">
						<span :class="item.merchantWinLoss < 0 ? 'minus-red ' : ''">
							{{ item.merchantWinLoss | amountDisplay }}
						</span>
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

					<template v-slot:item.rtp="{ item }">
						<span :class="item.rtp > 100 ? 'h-yellow  ' : ''">
							{{ item.rtp | amountDisplay }}
						</span>
					</template>
				</v-data-table>
			</v-col>
		</v-row>
	</div>
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
import { ChartDataTypeEnum } from '@/models/status/statistics';
import * as PermissionModel from '@/models/interfaces/commons/permission';

const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';

// @ts-ignore
@Component({ mixins: [datetimeMixin, commonDisplayFiltersMixin, commonMethodMixin], components: { DatePicker, LineChartPopup } })
export default class SearchList extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];
	@authModule.State('defaultLangTimes') defaultLangTimes!: number;

	panel: number[] = [0];
	selectMerchants: number[] = [];
	renderComponent: boolean = true;

	mySearchFilter: ReportModel.IGameReportRequest = {} as ReportModel.IGameReportRequest;
	extraMemo: string = '';
	targetMerchantID: number = 0;
	merchantsDropdownItems: DropdownModel.IDropdownItems[] = [];
	datetimeRangePicker: DropdownModel.DatetimeRangePicker = new DropdownModel.DatetimeRangePicker();
	result: Model.IMerchant[] = [];
	isHideTotalPlayersColumn: boolean = false;
	isSearchMergeHierarchyAgent: boolean = false;

	apiResultObject: any = {
		isShowGoToAgentReportHint: false,
	};

	dialog: boolean = false;
	isMounted: boolean = false;
	title: string = '';
	datasets: any[] = [];
	xTitle: string = '';
	yTitle: string = '';

	get hasPermissionGameDetail() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.GamesManagementRecord) >= 0;
	}

	get hasPermission_IsRTP_visible() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.Special_RTP_Invisible) >= 0 ? false : true;
	}

	get hasPermission_IsWinLose_visible() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.Special_WinLose_Invisible) >= 0 ? false : true;
	}

	get hasPermissionSuperAdmin() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSetting_SystemConfigSetting) >= 0;
	}

	get headers() {
		const vm: any = this;
		const headers: any[] = [];

		headers.push({ text: this.$t('TableHeader.__gameID'), value: 'gameID', sortable: true });
		headers.push({ text: this.$t('TableHeader.__gameName'), value: 'gameName', sortable: false });
		headers.push({ text: this.$t('TableHeader.__type'), value: 'gameTypeText', sortable: false });

		if (!this.isHideTotalPlayersColumn) {
			headers.push({ text: this.$t('TableHeader.__totalBetPlayer'), value: 'onlinePlayer', align: 'end', sortable: true });
		}

		headers.push({ text: this.$t('TableHeader.__totalRound'), value: 'totalRound', align: 'end', sortable: true });
		headers.push({ text: this.$t('TableHeader.__betAmount'), value: 'betAmount', align: 'end', sortable: true });
		headers.push({ text: this.$t('TableHeader.__validAmount'), value: 'validAmount', align: 'end', sortable: true });
		headers.push({ text: this.$t('TableHeader.__payoutAmount'), value: vm.visibleOrNot(this.hasPermission_IsWinLose_visible, 'payoutAmount'), align: 'end', sortable: true });
		headers.push({ text: this.$t('TableHeader.__playerWinLoss'), value: vm.visibleOrNot(this.hasPermission_IsWinLose_visible, 'updateAmount'), align: 'end', sortable: true });
		headers.push({ text: this.$t('TableHeader.__merchantProfit'), value: vm.visibleOrNot(this.hasPermission_IsWinLose_visible, 'merchantWinLoss'), align: 'end', sortable: true });
		headers.push({ text: 'RTP', value: vm.visibleOrNot(this.hasPermission_IsRTP_visible, 'rtp'), align: 'end', sortable: true });

		return headers;
	}

	get headersOfCategorySummary() {
		const vm: any = this;
		return [
			{ text: this.$t('TableHeader.__type'), value: 'gameTypeText', sortable: false },
			{ text: this.$t('TableHeader.__totalRound'), value: 'totalRound', align: 'end', sortable: false },
			{ text: this.$t('TableHeader.__betAmount'), value: 'betAmount', align: 'end', sortable: false },
			{ text: this.$t('TableHeader.__validAmount'), value: 'validAmount', align: 'end', sortable: false },
			{ text: this.$t('TableHeader.__payoutAmount'), value: vm.visibleOrNot(this.hasPermission_IsWinLose_visible, 'payoutAmount'), align: 'end', sortable: false },
			{ text: this.$t('TableHeader.__playerWinLoss'), value: vm.visibleOrNot(this.hasPermission_IsWinLose_visible, 'updateAmount'), align: 'end', sortable: false },
			{ text: this.$t('TableHeader.__merchantProfit'), value: vm.visibleOrNot(this.hasPermission_IsWinLose_visible, 'merchantWinLoss'), align: 'end', sortable: false },
			{ text: 'RTP', value: vm.visibleOrNot(this.hasPermission_IsRTP_visible, 'rtp'), align: 'end', sortable: false },
		];
	}

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

	clear() {
		this.selectMerchants = [];
	}

	showSpecificGameReportPlayers(targetGameID: number) {
		const myFilter: any = {
			querySqlID: 900081,
			merchantID: this.targetMerchantID,
			gameID: targetGameID,
			beginTime: this.mySearchFilter.begin,
			endTime: this.mySearchFilter.end,
		};

		const myConfig: any = {
			maxWidth: 1000,
			isFixedHeader: true,
		};

		EventBus.$emit(`globalShowCustomizedColumnsDisplayPopupDialog`, myFilter, myConfig);
	}

	openGameDetail(gameID: number) {
		this.setGameID(gameID);
		this.setGamePlayerID('');
		this.$router.push({ path: '/games-management/record' });
	}

	setExtraMemo(text: string) {
		this.extraMemo = text;
	}

	searchByFilter(filter: ReportModel.IGameReportRequest) {
		this.mySearchFilter = filter;
		this.result = [];

		EventBus.$emit('show_mask', null);

		ReportApi.getGameReportList(filter)
			.then((res) => {
				this.apiResultObject = res.data;
				this.result = res.data.merchants;
				if (res.data.merchants != null && res.data.merchants.length > 0) {
					this.targetMerchantID = res.data.merchants[0].merchantID;
				}

				this.isHideTotalPlayersColumn = false;
				this.isSearchMergeHierarchyAgent = res.data.isQuerySummaryByHierarchyMode;

				// 搜尋結果傳給parent
				this.$emit('setSearchResultToParentComponent', res.data);
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	geti18nTextAsString(key: string) {
		return this.$t(key).toString();
	}

	init() {
		DropdownApi.getMerchants(true).then((res) => {
			this.merchantsDropdownItems = res;
		});
		this.datetimeRangePicker.searchStartTimeRange = [daysAgo(-7), daysAgo(1)];
	}

	mounted() {
		this.registerReloadComponent();
		this.init();
	}
}
</script>

<style lang="scss" scoped></style>
