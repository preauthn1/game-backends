<template>
	<v-app v-if="renderComponent" class="app">
		<v-expansion-panels v-model="panel" multiple>
			<v-expansion-panel>

				<v-expansion-panel-content>
					<v-flex>
						<v-row>

							<v-col cols="12" sm="3" md="3">

								<div class=" merchantDropdownItemsWrapper">
									<v-autocomplete clearable :items="dropdownMerchants" class="merchantDropdownItems"
										v-model="searchFilter.merchantID" :label="$t('TableHeader.__merchant')">

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
								<div class="">

									<v-text-field v-model="searchFilter.playerID" clearable
										:label="$t('TableHeader.__playerID')">

									</v-text-field>

								</div>

							</v-col>
							<v-col cols="12" sm="3" md="3">

								<div class="">

									<v-text-field v-model="searchFilter.playerName" clearable
										:label="$t('TableHeader.__playerAccount')">

									</v-text-field>
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
								<div class="">
									<date-picker v-model="datetimeRangePicker.searchStartTimeRange" type="datetime"
										v-bind:clearable="false" class="force-w100" value-type="timestamp" range
										:placeholder="$t('TableHeader.__time')">
									</date-picker>
								</div>
							</v-col>

							<v-col cols="12" sm="6" md="6" class="quick-time">
								<!-- <div class="item"> -->
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
								<!-- </div> -->
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

			<v-icon class="ml-3"> error </v-icon>
			{{ $t('Message.__OnlySearchSingleMerchantNotAllAgent') }}

			<span class="f12 grey-text ml-3" style="display: inline-block;"
				v-if="searchResult.serverTimeTW">
				{{ $t('TableHeader.__lastSearchTime') }}: {{ searchResult.serverTimeTW }}
			</span>

		</div>

		<v-row>
			<v-col cols="12">

				<div v-if="searchResult.isShowGoToAgentReportHint"
					style="padding-left:15px; font-size: 15px;">

					<div class="hint-go-to-agent-report">
						<v-icon color='red ligthen-1' small>error</v-icon>
						{{ $t('Message.__thisAgentIsTreeModePleaseUseAgentReport') }}
					</div>

				</div>

				<v-data-table v-if="!searchResult.isShowGoToAgentReportHint" :headers="headers"
					:items="searchResult.items" sort :items-per-page="10">

					<template v-slot:item.gameName="{ item }">
						<v-tooltip top color="teal darken-1">
							<template v-slot:activator="{ on }">
								<a v-on="on" v-show="hasPermissionGameDetail" @click="openGameDetail(item.gameID)">
									<v-icon color="primary" small left>mdi-link</v-icon>
									{{ item.gameName }}
								</a>
							</template>
							<span>{{ $t('Button.__openLink') }}</span>
						</v-tooltip>
						<div v-show="!hasPermissionGameDetail">{{ item.gameName }}</div>
					</template>

					<template v-slot:item.totalRound="{ item }">
						{{ item.totalRound | countDisplay }}
					</template>

					<template v-slot:item.winLossAmount="{ item }">
						<span :class="item.winLossAmount < 0 ? 'minus-red ' : ''">
							{{ item.winLossAmount | amountDisplay }}
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

					<template v-slot:item.taxAmount="{ item }">
						{{ item.taxAmount | amountDisplay }}
					</template>

					<template v-slot:item.action="{ item }">

						<span class="link-text-grey" @click="showSinglePlayerDetail(item.playerID)">
							{{$t("Button.__showDetail")}}
						</span>

					</template>

					<template slot="body.append" v-if="searchResult.totalSummary">
						<tr>

							<td>
								{{ $t('TableHeader.__total') }}
								:
							</td>
							<td></td>
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

								<span :class="searchResult.totalSummary.winLossAmount  < 0 ? 'minus-red ' : ''">
									{{ searchResult.totalSummary.winLossAmount  | amountDisplay }}
								</span>

							</td>
							<td class='text-r' :class="searchResult.totalSummary.rtp > 100 ? 'h-yellow   ' : ''">
								{{searchResult.totalSummary.rtp | amountDisplay}}
							</td>

						</tr>
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
import { datetimeMixin, startEndDate, timeWithTimezone, daysAgo, weeksAgo, monthsAgo, toDateStringYYYYMMDDByDateText, toDateStringMMDDByDateText } from '@/utilities/datetime-format';
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
export default class SearchList extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];
	@authModule.State('defaultLangTimes') defaultLangTimes!: number;

	panel: number[] = [0];
	renderComponent: boolean = true;

	datetimeRangePicker: DropdownModel.DatetimeRangePicker = new DropdownModel.DatetimeRangePicker();
	searchFilter: Model.IMerchantAllPlayerWinLossReportRequest = {} as Model.IMerchantAllPlayerWinLossReportRequest;
	searchResult: Model.IMerchantAllPlayerWinLossReportResponse = {} as Model.IMerchantAllPlayerWinLossReportResponse;

	dropdownMerchants: DropdownModel.IDropdownItems[] = [];

	get headers() {
		return [
			{ text: this.$t('TableHeader.__merchantID'), value: 'merchantID', align: 'start', sortable: false },
			{ text: this.$t('TableHeader.__playerID'), value: 'playerID', align: 'start', sortable: false },
			{ text: this.$t('TableHeader.__playerAccount'), value: 'playerName', align: 'start', sortable: false },
			{ text: this.$t('TableHeader.__totalRound'), value: 'totalRound', align: 'end', sortable: true },
			{ text: this.$t('TableHeader.__betAmount'), value: 'betAmount', align: 'end', sortable: true },
			{ text: this.$t('TableHeader.__validAmount'), value: 'validAmount', align: 'end', sortable: true },
			{ text: this.$t('TableHeader.__payoutAmount'), value: 'payoutAmount', align: 'end', sortable: true },
			{ text: this.$t('TableHeader.__playerWinLoss'), value: 'winLossAmount', align: 'end', sortable: true },
			{ text: this.$t('TableHeader.__rtp'), value: 'rtp', align: 'end', sortable: true },
			{ text: this.$t('TableHeader.__action'), value: 'action', align: 'end', sortable: false },
		];
	}

	@Action('Players/setGameID') private setGameID!: any;
	@Action('Players/setGamePlayerID') private setGamePlayerID!: any;
	registerReloadComponent() {
		EventBus.$on(`reload-${this.$router.currentRoute.path}`, (info: any) => {
			this.clear();
			this.reloadComponent();
		});
	}

	reloadComponent() {
		setTimeout(() => {
			this.renderComponent = false;
			this.$nextTick(() => {
				this.renderComponent = true;
			});
		}, 250);
	}

	search() {
		EventBus.$emit('show_mask', null);
		this.searchFilter.begin = this.datetimeRangePicker.searchStartTimeRange[0] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[0]) : null;
		this.searchFilter.end = this.datetimeRangePicker.searchStartTimeRange[1] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[1]) : null;

		ReportApi.getMerchantAllPlayerWinLossReport(this.searchFilter)
			.then((res) => {
				this.searchResult = res.data;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	showSinglePlayerDetail(playerID: number) {
		const begin = this.datetimeRangePicker.searchStartTimeRange[0] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[0]) : null;
		const end = this.datetimeRangePicker.searchStartTimeRange[1] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[1]) : null;

		const targetRef: any = this.$refs.refSinglePlayerGameReportPopup;
		targetRef.showSinglePlayerDetail(playerID, begin, end);
	}

	clear() {
		this.searchFilter = {} as Model.IMerchantAllPlayerWinLossReportRequest;
	}

	geti18nTextAsString(key: string) {
		return this.$t(key).toString();
	}

	mounted() {
		// this.searchFilter.playerID = 1017184;

		this.reloadComponent();
		this.registerReloadComponent();
		this.init();

		DropdownApi.getMerchants(true).then((res) => {
			this.dropdownMerchants = res;
		});
	}

	init() {
		this.datetimeRangePicker.searchStartTimeRange = [daysAgo(-2), daysAgo(1)];
	}

	async exportFiles() {
		const excelHeaders: any = [
			this.geti18nTextAsString('TableHeader.__playerID'),
			this.geti18nTextAsString('TableHeader.__playerAccount'),
			this.geti18nTextAsString('TableHeader.__totalRound'),
			this.geti18nTextAsString('TableHeader.__betAmount'),
			this.geti18nTextAsString('TableHeader.__validAmount'),
			this.geti18nTextAsString('TableHeader.__payoutAmount'),
			this.geti18nTextAsString('TableHeader.__playerWinLoss'),
			this.geti18nTextAsString('TableHeader.__rtp'),
		];

		const excelData: any = [];

		this.searchFilter.begin = this.datetimeRangePicker.searchStartTimeRange[0] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[0]) : null;
		this.searchFilter.end = this.datetimeRangePicker.searchStartTimeRange[1] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[1]) : null;

		EventBus.$emit('show_mask', null);
		const result = await ReportApi.getMerchantAllPlayerWinLossReport(this.searchFilter).finally(() => {
			EventBus.$emit('hide_mask', null);
		});

		result.data.items.forEach((value) => {
			const eachRow: any = [];
			eachRow.push(value.playerID);
			eachRow.push(value.playerName);
			eachRow.push(value.totalRound);
			eachRow.push(value.betAmount);
			eachRow.push(value.validAmount);
			eachRow.push(value.payoutAmount);
			eachRow.push(value.winLossAmount);
			eachRow.push(value.rtp);
			excelData.push(eachRow);
		});

		const datePrefix = '_' + toDateStringMMDDByDateText(this.searchFilter.begin, this.userProfile.timeZone) + '_' + toDateStringMMDDByDateText(this.searchFilter.end, this.userProfile.timeZone);

		const config = {
			headers: excelHeaders,
			rowData: excelData,
			fileName: 'Export_AllPlayerWinLossReport' + datePrefix + '.xlsx',
		};
		// @ts-ignore
		this.downloadExcel(config);
	}
}
</script>
<style lang="scss" scoped>
</style>
