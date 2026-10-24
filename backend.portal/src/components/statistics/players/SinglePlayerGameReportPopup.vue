<template>

	<!-- 彈出視窗: 用戶詳情 -->
	<v-dialog v-model="isShowSinglePlayerDetail" max-width="1200px">

		<v-card style="border: solid 2px #555;">

			<v-card-title class="grey darken-2">
				{{$t("Button.__showDetail")}}

				<span class="ml-2" style="font-size:14px;color:#999">
					({{singlePlayerSearchResult.playerName}})
				</span>

			</v-card-title>

			<v-card-text style="min-height: 300px;">
				<v-row>
					<v-col cols="12">

						<v-data-table :headers="playerGameHeaders"
							:items="singlePlayerSearchResult.playerGameSummary" sort :items-per-page="1000"
							hide-default-footer>

							<template v-slot:item.gameName="{ item }">
								<v-tooltip top color="teal darken-1">
									<template v-slot:activator="{ on }">
										<a v-on="on" v-show="hasPermissionGameDetail"
											@click="openGameDetail(item.gameID)">
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

							<template slot="body.append" v-if="singlePlayerSearchResult.totalSummary">
								<tr>

									<td>
										{{ $t('TableHeader.__total') }}
										:
									</td>
									<td></td>

									<td class='text-r'>
										{{singlePlayerSearchResult.totalSummary.totalRound | countDisplay}}
									</td>
									<td class='text-r'>
										{{singlePlayerSearchResult.totalSummary.betAmount | amountDisplay}}
									</td>
									<td class='text-r'>
										{{singlePlayerSearchResult.totalSummary.validAmount | amountDisplay}}
									</td>
									<td class='text-r'>
										{{singlePlayerSearchResult.totalSummary.payoutAmount | amountDisplay}}
									</td>
									<td class='text-r'>

										<span
											:class="singlePlayerSearchResult.totalSummary.winLossAmount  < 0 ? 'minus-red ' : ''">
											{{ singlePlayerSearchResult.totalSummary.winLossAmount  | amountDisplay }}
										</span>

									</td>
									<td class='text-r'
										:class="singlePlayerSearchResult.totalSummary.rtp > 100 ? 'h-yellow   ' : ''">
										{{singlePlayerSearchResult.totalSummary.rtp | amountDisplay}}
									</td>

								</tr>
							</template>

						</v-data-table>

					</v-col>
				</v-row>

			</v-card-text>

		</v-card>

	</v-dialog>

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
import { datetimeMixin, startEndDate, timeWithTimezone, daysAgo, weeksAgo, monthsAgo } from '@/utilities/datetime-format';
import * as ChartModel from '@/models/interfaces/statistics/chart';
import ChartApi from '@/api/statistics/chart';
import DatePicker from 'vue2-datepicker';
import LineChartPopup from '@/components/statistics/LineChartPopup.vue';
import { ChartDataTypeEnum } from '@/models/status/statistics';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import ReportApi from '@/api/statistics/merchant-summary';
import * as Model from '@/models/interfaces/statistics/merchant-summary';

const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';

// @ts-ignore
@Component({ mixins: [datetimeMixin, commonDisplayFiltersMixin, commonMethodMixin], components: { DatePicker, LineChartPopup } })
export default class SinglePlayerGameReportPopup extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];
	@authModule.State('defaultLangTimes') defaultLangTimes!: number;

	panel: number[] = [0];
	renderComponent: boolean = true;
	isShowSinglePlayerDetail: boolean = false;
	singlePlayerSearchResult: GameReportModel.IPlayerGameSummary = {} as GameReportModel.IPlayerGameSummary;

	dropdownMerchants: DropdownModel.IDropdownItems[] = [];

	get playerGameHeaders() {
		return [
			{ text: this.$t('TableHeader.__gameID'), value: 'gameID', sortable: true },
			{ text: this.$t('TableHeader.__gameName'), value: 'gameName', sortable: false },
			{ text: this.$t('TableHeader.__totalRound'), value: 'totalRound', align: 'end', sortable: false },
			{ text: this.$t('TableHeader.__betAmount'), value: 'betAmount', align: 'end', sortable: true },
			{ text: this.$t('TableHeader.__validAmount'), value: 'validAmount', align: 'end', sortable: true },
			{ text: this.$t('TableHeader.__payoutAmount'), value: 'payoutAmount', align: 'end', sortable: true },
			{ text: this.$t('TableHeader.__playerWinLoss'), value: 'winLossAmount', align: 'end', sortable: true },
			{ text: 'RTP', value: 'rtp', align: 'end', sortable: true },
			// { text: this.$t('TableHeader.__taxAmount'), value: 'taxAmount', align: 'end', sortable: true },
		];
	}

	reloadComponent() {
		setTimeout(() => {
			this.renderComponent = false;
			this.$nextTick(() => {
				this.renderComponent = true;
			});
		}, 250);
	}

	showSinglePlayerDetail(playerID: number, begin: string, end: string) {
		this.isShowSinglePlayerDetail = true;

		const filter = {} as GameReportModel.IPlayerGameSummaryRequest;
		filter.begin = begin;
		filter.end = end;
		filter.playerID = playerID;

		EventBus.$emit('show_mask', null);
		ReportApi.getPlayerGameSummaryReportList(filter)
			.then((res) => {
				this.singlePlayerSearchResult = res.data;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	geti18nTextAsString(key: string) {
		return this.$t(key).toString();
	}

	mounted() {
		this.reloadComponent();
		this.init();

		DropdownApi.getMerchants(true).then((res) => {
			this.dropdownMerchants = res;
		});
	}

	init() {}
}
</script>
<style lang="scss" scoped>
</style>
