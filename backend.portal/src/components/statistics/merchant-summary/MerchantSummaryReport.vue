<template>
	<v-app v-if="renderComponent" class="app">
		<v-expansion-panels v-model="panel" multiple>
			<v-expansion-panel>

				<v-expansion-panel-content>
					<v-flex v-if="isRenderForm">
						<v-row>

							<v-col cols="12" sm="3" md="3">

								<!-- 單選(一般用戶) -->
								<div class="minus-mt-0" v-if="!isAllowMultipleSelected">

									<v-autocomplete clearable v-model="selectMerchant" class="merchantDropdownItems"
										deletable-chips chips :items="merchantsDropdownItems"
										:label="$t('TableHeader.__merchant')">

										<template v-slot:item="data">
											<div
												:class="data.item.valueInt == null  ? 'merchantCategoryText': 'merchantValue'">
												<span>{{data.item.text}}</span>
											</div>
										</template>

									</v-autocomplete>

								</div>

								<div class="minus-mt-3 text-r" v-else>

									<!-- 多選(管理員用戶) -->
									<v-autocomplete clearable v-model="selectMerchantIDs" full-width
										style="width:100%" small-chips class="merchantDropdownItems" multiple
										:items="merchantsWithoutCategoryDropdownItems"
										:label="$t('TableHeader.__merchant')">

									</v-autocomplete>

									<div v-if="hasPermissionSuperAdmin" @click="showParseTextToListIntDialog()"
										class="link-text-grey f12 minus-mt-2 z-999">
										貼上多筆商戶IDs
									</div>

								</div>

							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="">

									<v-autocomplete :items="searchExtraColumnDropdownItems" clearable chips
										:label="$t('Button.__showExtraInfo') + ' ('  +   $t('Message.__queryTimeWillLonger')  +')'"
										v-model="searchFilter.includeExtraSearchInfo" multiple>
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
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">

								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="minus-mt-4 w95">
									<date-picker v-model="datetimeRangePicker.searchStartTimeRange" type="datetime"
										value-type="timestamp" range :placeholder="$t('TableHeader.__time')">
									</date-picker>
								</div>
							</v-col>

							<v-col cols="12" sm="6" md="6" class="quick-time">

								<div class="minus-mt-1">
									<v-chip class="mr-2"
										:class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'today' }"
										@click="globalQuicklyPickTimeRange('today')">
										<v-icon left small>mdi-alarm-check</v-icon>
										{{ $t(`TableHeader.__today`) }}
									</v-chip>
									<v-chip class="mr-2"
										:class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'yesterday' }"
										@click="globalQuicklyPickTimeRange('yesterday')">
										<v-icon left small>mdi-alarm-check</v-icon>
										{{ $t(`TableHeader.__yesterday`) }}
									</v-chip>
									<v-chip class="mr-2"
										:class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'thisWeek' }"
										@click="globalQuicklyPickTimeRange('thisWeek')">
										<v-icon left small>mdi-alarm-check</v-icon>
										{{ $t(`TableHeader.__thisWeek`) }}
									</v-chip>

									<v-chip class="mr-2"
										:class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'lastWeek' }"
										@click="globalQuicklyPickTimeRange('lastWeek')">
										<v-icon left small>mdi-alarm-check</v-icon>
										{{ $t(`TableHeader.__lastWeek`) }}
									</v-chip>
									<v-chip class="mr-2"
										:class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'thisMonth' }"
										@click="globalQuicklyPickTimeRange('thisMonth')">
										<v-icon left small>mdi-alarm-check</v-icon>
										{{ $t(`TableHeader.__thisMonth`) }}
									</v-chip>
									<v-chip class="mr-2"
										:class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'lastMonth' }"
										@click="globalQuicklyPickTimeRange('lastMonth')">
										<v-icon left small>mdi-alarm-check</v-icon>
										{{ $t(`TableHeader.__lastMonth`) }}
									</v-chip>
								</div>

							</v-col>

							<v-col cols="12" sm="3" md="3">

							</v-col>

						</v-row>

					</v-flex>
				</v-expansion-panel-content>
			</v-expansion-panel>
		</v-expansion-panels>

		<div class="hint-row">
			<v-icon> error </v-icon>
			{{ $t('Message.__dataDelay30Minutes') }}

			<span class="f12 grey-text ml-3" style="display: inline-block;"
				v-if="searchResult.serverTimeTW">
				{{ $t('TableHeader.__lastSearchTime') }}: {{ searchResult.serverTimeTW }}
			</span>

		</div>

		<v-row style="padding: 0px 20px;">
			<v-col cols="12">
				<v-row>
					<v-row>
						<v-col cols="12" sm="10" md="10" lg="10"
							v-if="basicSummary.length > 0 && isFieldVisible(1011)">

							<!-- 基本資訊區塊 -->
							<v-data-table id="basicSummary" :headers="basicSummaryHeaders" :items="basicSummary"
								disable-sort hide-default-footer>
								<template v-slot:top>
									<v-toolbar>
										<v-toolbar-title>
											<h4>{{ $t('TableHeader.__basicInfo') }}</h4>
										</v-toolbar-title>
										<v-divider class="mx-4" inset vertical></v-divider>

										<span class="f12 grey-text hide-in-PC ">
											{{searchFirstMerchantID}}
										</span>

										<v-spacer></v-spacer>

									</v-toolbar>
								</template>

								<template
									v-slot:item.totalLoginPlayer="{ item }">{{ item.totalLoginPlayer | countDisplay }}
								</template>

								<template
									v-slot:item.transferAmount="{ item }">{{ item.transferAmount | amountDisplay }}</template>
								<template
									v-slot:item.transferInAmount="{ item }">{{ item.transferInAmount | amountDisplay }}</template>
								<template
									v-slot:item.transferOutAmount="{ item }">{{ item.transferOutAmount | amountDisplay }}</template>
								<template
									v-slot:item.walletAmount="{ item }">{{ item.walletAmount | amountDisplay }}</template>
							</v-data-table>
						</v-col>

						<!-- 遊戲訊息區塊 -->
						<v-col cols="12" sm="10" md="10" lg="10" v-if="gameSummary.length > 0">
							<v-data-table id="gameSummary" :headers="gameSummaryHeaders" :items="gameSummary"
								:footer-props="{'items-per-page-options': [200] }" disable-sort hide-default-footer>
								<template v-slot:top>
									<v-toolbar>
										<v-toolbar-title>
											<h4>{{ $t('TableHeader.__gameInfo') }}</h4>
										</v-toolbar-title>
										<v-divider class="mx-4" inset vertical></v-divider>

										<span class="f12 grey-text hide-in-PC ">
											{{searchFirstMerchantID}}
										</span>

										<v-spacer></v-spacer>
									</v-toolbar>
								</template>

								<template v-slot:item.merchantID="{ item }">

									<div class="text-l">
										<div class="">{{ item.merchantName  }} </div>
										<div class="sub-title-text">{{ item.merchantID }}</div>
									</div>
								</template>

								<template v-slot:item.totalRound="{ item }">
									{{ item.totalRound | countDisplay }}
								</template>
								<template
									v-slot:item.totalBetPlayer="{ item }">{{ item.totalBetPlayer | countDisplay }}</template>
								<template
									v-slot:item.loseAmount="{ item }">{{ item.loseAmount | amountDisplay }}</template>
								<template v-slot:item.winAmount="{ item }">{{ item.winAmount | amountDisplay }}
								</template>

								<template v-slot:item.betAmount="{ item }">{{ item.betAmount | amountDisplay }}
								</template>
								<template v-slot:item.validAmount="{ item }">
									{{ item.validAmount | amountDisplay }}
								</template>

								<template v-slot:item.totalPromotionCost="{ item }">
									{{ item.totalPromotionCost | amountDisplay }}
								</template>
								<template v-slot:item.payoutAmount="{ item }">
									{{ item.payoutAmount | amountDisplay }}
								</template>

								<template v-slot:item.rtp="{ item }">

									<span :class="item.rtp >= 100 ? 'h-yellow' : ''">
										{{ item.rtp | amountDisplay }}
									</span>

								</template>

								<template v-slot:item.moneyChangeAmount="{ item }">
									<span :class="item.moneyChangeAmount < 0 ? 'minus-red ' : ''">
										{{ item.moneyChangeAmount | amountDisplay }}</span>
								</template>
							</v-data-table>
						</v-col>

						<!-- 游戏厂商區塊 -->
						<v-col cols="12" sm="10" md="10" lg="10"
							v-if="selectMerchantIDs.length <= 1 && gameVendorSummary.length > 0 && hasPermissionSuperAdmin">
							<v-data-table id="gameVendorSummary" :headers="gameVendorSummaryHeaders"
								:items="gameVendorSummary[0].items" hide-default-footer>
								<template v-slot:top>
									<v-toolbar>
										<v-toolbar-title>
											<h4>

												{{ $t('TableHeader.__gameVendorType') }}
												<span class="f16 orange-text">(僅內部人員看到此區塊)</span>

											</h4>
										</v-toolbar-title>
										<v-divider class="mx-4" inset vertical></v-divider>
										<v-spacer></v-spacer>
									</v-toolbar>
								</template>

								<!-- <template v-slot:body.prepend>
									<tr>
										<td>
											<span>{{ $t('TableHeader.__gameVendorType') }}</span>
										</td>
										<td class="text-r">
											<span>
												{{ $t('TableHeader.__totalRound') }}
											</span>
										</td>

										<td class="text-r">
											<span>{{ $t('TableHeader.__totalBetPlayer') }}</span>
										</td>

										<td class="text-r">
											<span>{{ $t('TableHeader.__betAmount') }}</span>
										</td>

										<td class="text-r">
											<span>{{ $t('TableHeader.__validAmount') }}</span>
										</td>

										<td class="text-r">
											<span>{{ $t('TableHeader.__payoutAmount') }}</span>
										</td>

										<td class="text-r">
											<span>{{ $t('TableHeader.__moneyChangeAmount') }}</span>
										</td>

										<td class="text-r">
											<span>
												RTP
											</span>
										</td>
									</tr>
								</template> -->

								<template v-slot:item.totalRound="{ item }">
									{{ item.totalRound | countDisplay }}
								</template>

								<template v-slot:item.betPlayer="{ item }">
									{{ item.betPlayer | countDisplay }}
								</template>

								<template v-slot:item.loseAmount="{ item }">
									{{ item.loseAmount | amountDisplay }}
								</template>

								<template v-slot:item.winAmount="{ item }">
									{{ item.winAmount | amountDisplay }}
								</template>

								<template
									v-slot:item.betAmount="{ item }">{{ item.betAmount | amountDisplay }}</template>
								<template
									v-slot:item.validAmount="{ item }">{{ item.validAmount | amountDisplay }}</template>
								<template
									v-slot:item.payoutAmount="{ item }">{{ item.payoutAmount | amountDisplay }}
								</template>

								<template v-slot:item.rtp="{ item }">

									<span :class="item.rtp >= 100 ? 'h-yellow' : ''">
										{{ item.rtp | amountDisplay }}
									</span>

								</template>

								<template v-slot:item.winLossAmount="{ item }">
									<span :class="item.winLossAmount < 0 ? 'minus-red ' : ''">
										{{ item.winLossAmount | amountDisplay }}</span>
								</template>

								<template slot="body.append" v-if="getGameVendorTotalSummaryRow()">
									<tr>

										<td class="grey-text">
											{{ $t('TableHeader.__total') }}
											:
										</td>

										<td class='text-r'>
											{{getGameVendorTotalSummaryRow().totalRound | countDisplay}}
										</td>

										<td class='text-r'>
											{{getGameVendorTotalSummaryRow().betPlayer | countDisplay}}
										</td>

										<td class='text-r'>
											{{getGameVendorTotalSummaryRow().betAmount | amountDisplay}}
										</td>

										<td class='text-r'>
											{{getGameVendorTotalSummaryRow().validAmount | amountDisplay}}
										</td>

										<td class='text-r'>
											{{getGameVendorTotalSummaryRow().payoutAmount | amountDisplay}}
										</td>

										<td class='text-r'
											:class="getGameVendorTotalSummaryRow().winLossAmount < 0 ? 'minus-red' : ''">
											{{getGameVendorTotalSummaryRow().winLossAmount | amountDisplay}}
										</td>

										<td class='text-r'>

											<span :class="getGameVendorTotalSummaryRow().rtp >= 100 ? 'h-yellow' : ''">
												{{getGameVendorTotalSummaryRow().rtp | amountDisplay}}
											</span>

										</td>

									</tr>
								</template>

							</v-data-table>
						</v-col>

						<!-- 玩家語系分布 -->
						<v-col cols="12" sm="10" md="10" lg="10"
							v-if="languageSummary && languageSummary.length > 0">
							<v-data-table id="langSummary" :headers="languageSummaryHeaders"
								:items="languageSummary" hide-default-footer>
								<template v-slot:top>
									<v-toolbar>
										<v-toolbar-title>
											<h4>

												{{ $t('TableHeader.__playerLanguageDistribute') }}

											</h4>
										</v-toolbar-title>
										<v-divider class="mx-4" inset vertical></v-divider>
										<v-spacer></v-spacer>
									</v-toolbar>
								</template>

								<template v-slot:item.totalCount="{ item }">
									{{ item.totalCount | countDisplay }}
								</template>

								<template v-slot:item.rate_100X="{ item }">
									{{ item.rate_100X  | amountDisplay }}
								</template>

							</v-data-table>
						</v-col>

						<!-- 玩家登入裝置分布 -->
						<v-col cols="12" sm="10" md="10" lg="10"
							v-if="loginDeviceSummary && loginDeviceSummary.length > 0 && hasPermissionSuperAdmin">
							<v-data-table id="loginDeviceSummary" :headers="loginDeviceSummaryHeaders"
								:items="loginDeviceSummary" hide-default-footer>
								<template v-slot:top>
									<v-toolbar>
										<v-toolbar-title>
											<h4>
												{{$t("TableHeader.__playerDeviceDistribute")}}
											</h4>
										</v-toolbar-title>
										<v-divider class="mx-4" inset vertical></v-divider>

										<v-icon small>info</v-icon>
										<span class="ml-1 f12">
											{{$t("Message.__someOfPlayerWillMultipleDevice")}}
										</span>

										<v-icon small class="ml-4">info</v-icon>
										<span class="ml-1 f12">
											商戶需啟用iframe才有紀錄
										</span>

										<v-spacer></v-spacer>
									</v-toolbar>
								</template>

								<template v-slot:item.totalCount="{ item }">
									{{ item.totalCount | countDisplay }}
								</template>

								<template v-slot:item.rate_100X="{ item }">
									{{ item.rate_100X  | amountDisplay }}
								</template>

							</v-data-table>
						</v-col>

						<!-- 玩家投注額區間 -->
						<v-col cols="12" sm="10" md="10" lg="10"
							v-if="playerTWDBetAmountDistribute && playerTWDBetAmountDistribute.items &&  playerTWDBetAmountDistribute.items.length > 0 && hasPermissionSuperAdmin">
							<v-data-table id="table_playerTWDBetAmountDistribute" :footer-props="footerProps"
								:headers="playerTWDBetAmountDistributeSummaryHeaders"
								:items="playerTWDBetAmountDistribute.items" hide-default-footer>
								<template v-slot:top>
									<v-toolbar>
										<v-toolbar-title>
											<h4>
												總投注額玩家人數 (TWD台幣)
												<span class="f12">

												</span>
											</h4>
										</v-toolbar-title>
										<v-divider class="mx-4" inset vertical></v-divider>

										<span class="ml-1 f12">
											{{playerTWDBetAmountDistribute.currency}}轉TWD，轉換匯率:
											1:{{playerTWDBetAmountDistribute.exchangeRate}}
										</span>

										<v-icon small class="ml-4" color="yellow">info</v-icon>
										<span class="ml-1 f12 yellow-text">
											捕魚遊戲會以 [有效流水] 計算
										</span>

										<v-spacer></v-spacer>
									</v-toolbar>
								</template>

								<template v-slot:item.minBetAmount="{ item }">
									{{ item.minBetAmount | integerDisplay }}
								</template>

								<template v-slot:item.totalCount="{ item }">

									<span @click="showPlayersPopup(item.playerIDs)"
										:class=" item.totalCount > 0 ? 'link-text' : ''">
										{{ item.totalCount | countDisplay }}
									</span>

								</template>

								<template v-slot:item.totalBetAmount="{ item }">

									<span>
										{{ item.totalBetAmount | amountDisplay }}
									</span>

								</template>

								<template v-slot:item.isAnyFishingGame="{ item }">

									<v-chip v-if="item.isAnyFishingGame" small>
										部分玩家有捕魚數據
									</v-chip>

								</template>

								<template v-slot:item.rate_100X="{ item }">
									{{ item.rate_100X  | amountDisplay }}
								</template>

								<template v-slot:item.betAmountRate_100X="{ item }">
									{{ item.betAmountRate_100X  | amountDisplay }}
								</template>

							</v-data-table>
						</v-col>

					</v-row>
				</v-row>
			</v-col>
		</v-row>

		<CommonPopupBriefList ref="refCommonPopupBriefList">
		</CommonPopupBriefList>

		<v-dialog v-model="dialog" v-if="dialog">
			<LineChartPopup v-if="isMounted" :title="title" :datasets="datasets" :xTitle="xTitle"
				:yTitle="yTitle" @close="dialog = false" />
			<v-overlay :absolute="true" :value="true" v-show="!isMounted">
				<!-- <v-progress-circular class="loading" :size="70" :width="7" indeterminate></v-progress-circular> -->
				<div class="loader"></div>
			</v-overlay>
		</v-dialog>

		<TextareaParseToListIDPopup ref="refTextareaParseToListIDPopup"
			@parentSetResultIDs="setResultMerchantIDs">
		</TextareaParseToListIDPopup>

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
import * as ChartModel from '@/models/interfaces/statistics/chart';
import * as GameModel from '@/models/interfaces/game-setting/switch';
import MerchantSummaryApi from '@/api/statistics/merchant-summary';

import ChartApi from '@/api/statistics/chart';
import DatePicker from 'vue2-datepicker';
import LineChartPopup from '@/components/statistics/LineChartPopup.vue';
import CommonPopupBriefList from '@/components/system-setting/CommonPopupBriefList.vue';
import { ChartDataTypeEnum } from '@/models/status/statistics';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import * as Permission from '@/models/status/permission';

const authModule = namespace('Auth');
import SinglePlayerGameReportPopup from '@/components/statistics/players/SinglePlayerGameReportPopup.vue';

// @ts-ignore
@Component({ mixins: [datetimeMixin, commonDisplayFiltersMixin, commonMethodMixin], components: { TextareaParseToListIDPopup, DatePicker, LineChartPopup, CommonPopupBriefList } })
export default class SearchList extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];

	panel: number[] = [0];
	selectMerchant: number = 0;
	selectMerchantIDs: number[] = [];
	renderComponent: boolean = true;
	isRenderForm: boolean = true;

	searchFilter: Model.IMerchantSummarySearchRequest = {} as Model.IMerchantSummarySearchRequest;
	datetimeRangePicker: DropdownModel.DatetimeRangePicker = new DropdownModel.DatetimeRangePicker();

	searchResult: any = {};
	basicSummary: Model.IBasicSummary[] = [];
	loginSummary: Model.ILoginSummary[] = [];
	gameSummary: Model.IGameSummary[] = [];
	gameVendorSummary: Model.IGameVendorSummary[] = [];
	languageSummary: Model.ICountReportSummary[] = [];
	searchFirstMerchantID: number = 0;
	loginDeviceSummary: Model.ICountReportSummary[] = [];
	playerTWDBetAmountDistribute: Model.IBetAmountDistribute = {} as Model.IBetAmountDistribute;

	merchantsDropdownItems: DropdownModel.IDropdownItems[] = [];
	merchantsWithoutCategoryDropdownItems: DropdownModel.IDropdownItems[] = [];
	searchExtraColumnDropdownItems: DropdownModel.IDropdownItems[] = [];

	dialog: boolean = false;
	isMounted: boolean = false;
	title: string = '';
	datasets: any[] = [];
	xTitle: string = '';
	yTitle: string = '';

	footerProps: GameModel.IDataTablePageObject = {
		'items-per-page-options': [50],
	} as GameModel.IDataTablePageObject;

	get basicSummaryHeaders() {
		return [
			{ text: this.$t('TableHeader.__merchant'), value: 'merchantName', sortable: false, width: '20%' },
			{ text: this.$t('TableHeader.__currency'), value: 'currency', sortable: false, width: '10%' },
			{ text: this.$t('TableHeader.__totalLoginPlayer'), value: 'totalLoginPlayer' },
			{ text: this.$t('TableHeader.__moneyChangeAmount'), value: 'transferAmount', align: 'end', sortable: false },
			{ text: this.$t('TableHeader.__transferInAmount'), value: 'transferInAmount', align: 'end', sortable: false },
			{ text: this.$t('TableHeader.__transferOutAmount'), value: 'transferOutAmount', align: 'end', sortable: false },
		];
	}

	// : any =;
	// loginSummaryHeaders: any = [
	// 	{ text: this.$t('TableHeader.__merchant'), value: 'merchantName' },
	// 	{ text: '总登录人次  (圖)', value: 'totalLoginCount' },
	// 	{ text: '新登录人数', value: 'newLoginPlayer' },
	// 	{ text: '最高在线', value: 'maxLoginPlayer' },
	// 	{ text: '平均在线  (圖)', value: 'avgLoginPlayer' },
	// ];

	get gameSummaryHeaders() {
		return [
			{ text: this.$t('TableHeader.__merchant'), value: 'merchantID', align: 'start', width: '10%' },
			{ text: this.$t('TableHeader.__totalRound'), value: 'totalRound', align: 'end', width: '10%' },
			{ text: this.$t('TableHeader.__totalLoginPlayer'), value: 'totalBetPlayer', align: 'end', width: '15%' },
			{ text: this.$t('TableHeader.__betAmount'), value: 'betAmount', align: 'end' },
			{ text: this.$t('TableHeader.__validAmount'), value: 'validAmount', align: 'end' },
			{ text: this.$t('TableHeader.__payoutAmount'), value: 'payoutAmount', align: 'end' },
			{ text: this.$t('TableHeader.__playerWinLoss'), value: 'moneyChangeAmount', align: 'end' },
			{ text: this.$t('TableHeader.__rtp'), value: 'rtp', align: 'end' },
			{ text: this.$t('TableHeader.__promotionBonus'), value: 'totalPromotionCost', align: 'end' },
		];
	}

	get gameVendorSummaryHeaders() {
		return [
			{ text: this.$t('TableHeader.__gameVendorType'), value: 'gameVendorTypeText', width: '20%' },
			{ text: this.$t('TableHeader.__totalRound'), value: 'totalRound', align: 'end', width: '10%' },
			{ text: this.$t('TableHeader.__totalLoginPlayer'), value: 'betPlayer', align: 'end', width: '15%' },
			{ text: this.$t('TableHeader.__betAmount'), value: 'betAmount', align: 'end' },
			{ text: this.$t('TableHeader.__validAmount'), value: 'validAmount', align: 'end' },
			{ text: this.$t('TableHeader.__payoutAmount'), value: 'payoutAmount', align: 'end' },
			{ text: this.$t('TableHeader.__playerWinLoss'), value: 'winLossAmount', align: 'end' },
			{ text: this.$t('TableHeader.__rtp'), value: 'rtp', align: 'end' },
		];
	}

	get languageSummaryHeaders() {
		return [
			{ text: this.$t('TableHeader.__lang'), value: 'displayText', width: '15%' },
			{ text: this.$t('TableHeader.__totalPeopleCount'), value: 'totalCount', align: 'end', width: '15%' },
			{ text: this.$t('TableHeader.__rate') + '(%)', value: 'rate_100X', align: 'end' },
			{ text: '', value: 'qqq', align: 'end', width: '60%' },
		];
	}

	get loginDeviceSummaryHeaders() {
		return [
			{ text: this.$t('TableHeader.__device'), value: 'displayText', width: '15%' },
			{ text: this.$t('TableHeader.__totalPeopleCount'), value: 'totalCount', align: 'end', width: '15%' },
			{ text: this.$t('TableHeader.__rate') + '(%)', value: 'rate_100X', align: 'end' },
			{ text: '', value: 'qqq', align: 'end', width: '60%' },
		];
	}

	get playerTWDBetAmountDistributeSummaryHeaders() {
		return [
			{ text: this.$t('TableHeader.__betAmount') + '區間' + ' (>=)', value: 'minBetAmount', width: '15%', align: 'end', sortable: false },
			{ text: '', value: 'qqqqqq', width: '3%', sortable: false },
			{ text: this.$t('TableHeader.__totalPeopleCount'), value: 'totalCount', align: 'end', width: '10%', sortable: false },
			{ text: this.$t('TableHeader.__rate') + '(%)', value: 'rate_100X', align: 'end', width: '10%', sortable: false },

			{ text: '玩家總投注額(TWD)', value: 'totalBetAmount', align: 'end', width: '15%', sortable: false },
			{ text: '玩家總投注額占整體比例' + '(%)', value: 'betAmountRate_100X', align: 'end', width: '10%', sortable: false },
			{ text: this.$t('TableHeader.__memo'), value: 'isAnyFishingGame', align: 'end', width: '10%', sortable: false },

			{ text: '', value: 'qqq', align: 'end', width: '20%', sortable: false },
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

	isFieldVisible(searchExtraInfoID: number) {
		if (this.searchFilter.includeExtraSearchInfo == null || this.searchFilter.includeExtraSearchInfo.length < 0) {
			return;
		}

		return this.searchFilter.includeExtraSearchInfo.includes(searchExtraInfoID);
	}

	getGameVendorTotalSummaryRow() {
		if (!this.gameVendorSummary) {
			return null;
		}

		return this.gameVendorSummary[0].totalSummary;
	}

	copyPlayerIDs(item: Model.IBetAmountDistributeItem) {
		const vm: any = this;
		const playerIDs = item.playerIDs.join();

		if (item.playerIDs.length <= 0) {
			return;
		}

		vm.copyTextToClipboard2(playerIDs);
	}

	showPlayersPopup(playerIDs: number[]) {
		if (playerIDs.length <= 0) {
			return;
		}

		const targetRef: any = this.$refs.refCommonPopupBriefList;
		targetRef.getPlayers(playerIDs);
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
		// 根據使用者不同 會允許多選
		if (this.selectMerchantIDs.length > 0) {
			this.searchFilter.merchantIDs = this.selectMerchantIDs;
		} else {
			if (this.selectMerchant <= 0) {
				EventBus.$emit('auto-hidden-error', this.$t('Message.__pleaseSelectMerchant'));
				return;
			}

			this.searchFilter.merchantIDs = [this.selectMerchant];
		}

		this.searchFilter.begin = this.datetimeRangePicker.searchStartTimeRange[0] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[0]) : null;
		this.searchFilter.end = this.datetimeRangePicker.searchStartTimeRange[1] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[1]) : null;

		EventBus.$emit('show_mask', null);

		MerchantSummaryApi.getMerchantSummaryList(this.searchFilter)
			.then((res) => {
				this.basicSummary = [];
				this.loginSummary = [];
				this.gameSummary = [];
				this.gameVendorSummary = [];

				this.searchResult = res.data;
				res.data.merchants.forEach((item) => {
					this.searchFirstMerchantID = item.merchantID;
					this.basicSummary.push(<Model.IBasicSummary>{
						merchantName: item.merchantName,
						merchantID: item.merchantID,
						currency: item.basicSummary.currency,
						totalLoginPlayer: item.basicSummary.totalLoginPlayer,
						transferAmount: item.basicSummary.transferAmount,
						transferInAmount: item.basicSummary.transferInAmount,
						transferOutAmount: item.basicSummary.transferOutAmount,
						walletAmount: item.basicSummary.walletAmount,
					});

					this.loginSummary.push(<Model.ILoginSummary>{
						merchantName: item.merchantName,
						totalLoginCount: item.loginSummary.totalLoginCount,
						newLoginPlayer: item.loginSummary.newLoginPlayer,
						maxLoginPlayer: item.loginSummary.maxLoginPlayer,
						avgLoginPlayer: item.loginSummary.avgLoginPlayer,
					});

					this.gameSummary.push(item.gameSummary);

					// 	<Model.IGameSummary>{
					// 	merchantName: item.merchantName,
					// 	totalRound: item.gameSummary.totalRound,
					// 	totalBetPlayer: item.gameSummary.totalBetPlayer,
					// 	winAmount: item.gameSummary.winAmount,
					// 	loseAmount: item.gameSummary.loseAmount,
					// 	betAmount: item.gameSummary.betAmount,
					// 	payoutAmount: item.gameSummary.payoutAmount,
					// 	validAmount: item.gameSummary.validAmount,
					// 	moneyChangeAmount: item.gameSummary.moneyChangeAmount,
					// 	rtp: item.gameSummary.rtp,
					// 	to
					// });

					this.gameVendorSummary.push(item.gameVendorSummary);
					this.languageSummary = item.languageDistributeReport;
					this.loginDeviceSummary = item.deviceDistributeReport;
					this.playerTWDBetAmountDistribute = item.playerTWDBetAmountDistribute;

					console.log('------------');
					console.log(this.playerTWDBetAmountDistribute);
				});
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	clear() {
		this.selectMerchant = 0;
		this.selectMerchantIDs = [];
	}

	mounted() {
		this.registerReloadComponent();
		this.init();
	}

	init() {
		this.datetimeRangePicker.searchStartTimeRange = [daysAgo(0), daysAgo(1)];

		DropdownApi.getMerchants(true).then((res) => {
			this.merchantsDropdownItems = res;
			if (this.merchantsDropdownItems.length === 1) {
				// 疑似沒效果 不弄了
				const selectValue: any = this.merchantsDropdownItems[0].valueInt;
				this.selectMerchant = selectValue;
			}
		});

		// 管理員才會用這種模式
		DropdownApi.getMerchantsByFilter({} as any, false).then((res) => {
			this.merchantsWithoutCategoryDropdownItems = res;
		});

		DropdownApi.getStatReportSearchColumns('merchant-summary-report').then((res) => {
			this.searchExtraColumnDropdownItems = res;
		});
	}
	showTotalLoginCountChart() {
		this.isMounted = false;
		this.dialog = true;

		this.xTitle = this.$t('TableHeader.__xTitle').toString();
		this.yTitle = this.$t('TableHeader.__merchant').toString();

		ChartApi.getChart(ChartDataTypeEnum.Summary_TotalLogin, <ChartModel.IChartRequest>{
			begin: this.datetimeRangePicker.searchStartTimeRange[0] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[0]) : null,
			end: this.datetimeRangePicker.searchStartTimeRange[1] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[1]) : null,
			merchantIDs: [this.selectMerchant],
		}).then((res) => {
			this.isMounted = true;
			this.title = this.$t('TableHeader.__totalLoginCount').toString(); // res.data.chartName
			this.datasets = [];
			res.data.chartDatas.forEach((item) => {
				this.datasets.push({
					group: item.merchantName,
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
	showAvgLoginPlayerChart() {
		this.isMounted = false;
		this.dialog = true;

		this.xTitle = this.$t('TableHeader.__xTitle').toString();
		this.yTitle = this.$t('TableHeader.__merchant').toString();
		ChartApi.getChart(ChartDataTypeEnum.Summary_AverageOnline, <ChartModel.IChartRequest>{
			begin: this.datetimeRangePicker.searchStartTimeRange[0] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[0]) : null,
			end: this.datetimeRangePicker.searchStartTimeRange[1] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[1]) : null,
			merchantIDs: [this.selectMerchant],
		}).then((res) => {
			this.isMounted = true;
			this.title = this.$t('TableHeader.__avgLoginPlayer').toString(); // res.data.chartName
			this.datasets = [];
			res.data.chartDatas.forEach((item) => {
				this.datasets.push({
					group: item.merchantName,
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
	showTotalRoundChart() {
		this.isMounted = false;
		this.dialog = true;

		this.xTitle = this.$t('TableHeader.__xTitle').toString();
		this.yTitle = this.$t('TableHeader.__merchant').toString();
		ChartApi.getChart(ChartDataTypeEnum.Summary_TotalGame, <ChartModel.IChartRequest>{
			begin: this.datetimeRangePicker.searchStartTimeRange[0] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[0]) : null,
			end: this.datetimeRangePicker.searchStartTimeRange[1] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[1]) : null,
			merchantIDs: [this.selectMerchant],
		}).then((res) => {
			this.isMounted = true;
			this.title = this.$t('TableHeader.__totalRound').toString(); // res.data.chartName
			this.datasets = [];
			res.data.chartDatas.forEach((item) => {
				this.datasets.push({
					group: item.merchantName,
					data: item.data.map((s) => {
						return {
							x: new Date(timeWithTimezone(s.statTime, this.userProfile.timeZone)),
							y: s.dataCount,
						};
					}),
				});
			});
			this.dialog = true;
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
