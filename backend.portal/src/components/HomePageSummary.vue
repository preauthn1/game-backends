<template>
	<v-app class="homePageSummaryPage">

		<v-row>

			<v-col cols="12" md="7" sm="7" style="margin-top:-30px"
				v-if="hasSuperPermission && financeOverviewReport != null && financeOverviewReport.currencyGroup != null && financeOverviewReport.currencyGroup.length > 0">

				<div class="homePageTodayInfo">

					<div class="text-r" style="margin-bottom: -5px;">
						<span class="f12 mr-1" style="color:#888">
							刷新時間:{{searchHomePageTodayInfo.lastRefreshTimeText}}
						</span>
						<v-icon color="blue" class="pointer" @click="reloadHomePageTodayInfo()">refresh</v-icon>
					</div>

					<v-row v-if="searchHomePageTodayInfo">

						<v-col cols="12" md="4" sm="12" class="todayInfoCardWrapper">
							<div class="todayInfoCard">
								<div class="cardTitle" @click="showEachMerchantOnlinePlayers()">在線玩家數</div>
								<span class="myValue pointer" @click="showEachMerchantOnlinePlayers()">
									{{searchHomePageTodayInfo.currentOnlinePlayers | countDisplay }}
								</span>
							</div>
						</v-col>

						<v-col cols="12" md="4" sm="12" class="todayInfoCardWrapper">
							<div class="todayInfoCard">
								<div class="cardTitle">玩家總數 (今日/昨日)
								</div>
								<span class="myValue pointer" @click="showEachMerchantTotalPlayers('today')">
									{{searchHomePageTodayInfo.totalPlayers_Today | countDisplay}}
								</span>
								<span class="myValue seperateLine" style="">
									|
								</span>
								<span class="myValue pointer" @click="showEachMerchantTotalPlayers('yesterday')">
									{{searchHomePageTodayInfo.totalPlayers_Yesterday | countDisplay}}
								</span>
							</div>
						</v-col>

						<v-col cols="12" md="4" sm="12" class="todayInfoCardWrapper">
							<div class="todayInfoCard">
								<div class="cardTitle">注單總數 (今日/昨日) </div>
								<span class="myValue pointer" @click="showEachMerchantTotalRound('today')">
									{{searchHomePageTodayInfo.totalRound_KValue_Today | countDisplay}}K
								</span>
								<span class="myValue seperateLine" style="">
									|
								</span>
								<span class="myValue pointer" @click="showEachMerchantTotalRound('yesterday')">
									{{searchHomePageTodayInfo.totalRound_KValue_Yesterday | countDisplay}}K
								</span>
							</div>
						</v-col>
					</v-row>
				</div>

				<div class="minus-mt-1">

					<span class="link-text-grey f12 mr-2" @click="showEachCurrencyOnlinePlayers()">
						各貨幣在線人數
					</span>

					<span class="link-text-grey f12 mr-2" @click="showEachDayTotalRoundPopup()">
						每日注單數(不分貨幣)
					</span>

				</div>

				<!-- 圖表區塊 -->
				<div style="position:relative" class="hide-in-phone mt-2">

					<div style="width: 110px; position: absolute; top: -15px; right: 5px; z-index: 3;">
						<v-autocomplete :items="chartCurrencyList" v-model="chartDataTargetCurrency"
							@change="whenChangeChartCurrency()">
						</v-autocomplete>
					</div>

					<div style="width: 110px; position: absolute; top: -15px; right: 140px; z-index: 3;">
						<v-autocomplete :items="chartTypeDropdown" v-model="displayChartType"
							@change="whenChangeChartCurrency()">
						</v-autocomplete>
					</div>

					<div style="position: absolute; bottom: 5px; right: 15px; z-index: 3;">
						<v-icon color="grey" @click="showDailyListPopup()">mdi-eye</v-icon>
					</div>

					<LineChart ref="refChartPopup" :chartData="searchChartData"
						:datasets="searchChartData.datasets" :xTitle="'xxxxx'" :yTitle="'yyyyy'"
						@close="closeAllModal" />
				</div>

				<!-- 帳務總覽 -->
				<h2 style="position:relative" class="mt-2">
					帳務總覽
					<span class="f12 grey-text hide-in-phone ">
						{{financeOverviewReport.beginText}}-{{financeOverviewReport.endText}}
					</span>

					<v-tooltip top color="orange darken-4">
						<template v-slot:activator="{ on, attrs }">
							<v-icon small color="grey" v-bind="attrs" v-on="on">info</v-icon>
						</template>
						<span>
							<div>
								每個摺疊分類顯示最多{{searchFilterFinanceOverview.pageSize}}筆商戶 (按照投注額高低排序)
							</div>
						</span>
					</v-tooltip>

					<div style="display: inline-block; position: absolute; right: 0px;">

						<div class=" f14 mr-4 pointer" @click="showOverviewSettingDialog"
							style="display: inline-block;color:#6d98bd ">

							<div class="hide-in-phone">
								<v-icon color="" small style="color:#6d98bd ">
									settings
								</v-icon> 查詢設置
							</div>

						</div>

						<a class="f14 link-text-grey mr-2 hide-in-phone " @click="expandAllPanel()">展開</a>
						<a class="f14 link-text-grey mr-2 hide-in-phone " @click="closeAllPanel()">縮合</a>

						<v-btn small class="btn mr-1" color="blue lighten-1" text
							@click="setDateRangeAndSearch('lastMonth')">
							<v-icon left>search</v-icon>上月</v-btn>
						<v-btn small class="btn mr-1" color="blue lighten-1" text
							@click="setDateRangeAndSearch('thisMonth')">
							<v-icon left>search</v-icon>本月</v-btn>
					</div>

				</h2>

				<v-expansion-panels accordion multiple v-model="panelFlags" class="mt-2">

					<v-expansion-panel v-for="(item , index) in financeOverviewReport.currencyGroup"
						:key="index">

						<v-expansion-panel-header style="padding: 10px;">
							{{item.categoryText}}

							<span class="f12 ml-2 yellow-text" style="max-width:100px"
								v-if="item.isDynamicHideAgent && searchFilterFinanceOverview.isExcludeMYCAgent">
								*不含MYC代理
							</span>

							<span class="f12 ml-2 grey-text">{{item.memo}}</span>

							<div class="avgDailyRound" v-if="item.isCurrencyCategory" :title="'每日平均注單數'">
								<v-chip x-small color="grey darken-3"
									v-if="item.avgDailyRound_KAmount > 0">{{ item.avgDailyRound_KAmount }} K</v-chip>
								<v-chip x-small color="grey darken-3" v-else><span
										class="f20 mr-1">⇩</span>小於1000</v-chip>
							</div>

							<div v-if="item.totalMoM != null" class="totalMoM">
								<span :class="item.totalMoM < 0 ? 'minus-red': ''">{{item.totalMoM}}%</span>
							</div>
						</v-expansion-panel-header>

						<v-expansion-panel-content
							style="background: #222;min-height: 200px; border-bottom: solid 3px #103860;">

							<table class="ez-table" style="margin-top: 25px;font-size:14px;">

								<tr class="smallTextHeader">
									<!-- <th class="text-l">總代</th> -->
									<th class="text-l" style="width:8%">商戶ID</th>
									<th class="text-l" style="width:17%">商戶</th>
									<th class="text-r">總局數</th>
									<th class="text-r">投注額</th>
									<th class="text-r">商戶輸贏</th>
									<th class="text-r">
										MoM

										<v-tooltip top color="orange darken-4">
											<template v-slot:activator="{ on, attrs }">
												<v-icon small color="grey" v-bind="attrs" v-on="on">info</v-icon>
											</template>
											<span>
												<div>
													投注金額成長率(與上月相比 ※{{financeOverviewReport.beginText_LastMonth}} -
													{{financeOverviewReport.endText_LastMonth}})
												</div>
											</span>
										</v-tooltip>

									</th>
								</tr>

								<tr v-for="(eachMerchant , index2) in item.merchants" :key="index2" class="hover">
									<!-- 									
									<td>
										<v-icon color="grey" v-if="eachMerchant.rootMerchantID > 0" small>
											info
										</v-icon>
										<v-icon color="grey" v-else disabled='disabled' small>
											info
										</v-icon>
									</td> -->

									<td class="text-l">
										<span class="">{{eachMerchant.merchantID}}</span>
									</td>
									<td class="text-l">
										{{eachMerchant.merchantName}}
									</td>

									<td class="text-r">{{eachMerchant.totalRound | countDisplay}}</td>
									<td class="text-r">{{eachMerchant.betAmount | amountDisplay}}</td>
									<td class="text-r">

										<span class="minus-red" v-if="eachMerchant.winLossAmount > 0">
											{{eachMerchant.winLossAmount * -1  | amountDisplay}}
										</span>
										<span class="" v-else>
											{{eachMerchant.winLossAmount * -1  | amountDisplay}}
										</span>

									</td>
									<td class="text-r">

										<div :title="''">
											<span class="minus-red" v-if="eachMerchant.lastMonthCompareRate_MOM < 0">
												{{eachMerchant.lastMonthCompareRate_MOM | amountDisplay}}%
											</span>
											<span class="yellow-text"
												v-else-if="eachMerchant.lastMonthCompareRate_MOM > 200">
												{{eachMerchant.lastMonthCompareRate_MOM | amountDisplay}}%
											</span>
											<span class="" v-else>
												+{{eachMerchant.lastMonthCompareRate_MOM | amountDisplay}}%
											</span>
										</div>

									</td>
								</tr>

							</table>

							<div>

							</div>

							<!-- <div style="padding: 0px 25px;">

								<v-row>
									<v-col cols="12" sm="12" md="12">
										<v-chip color="primary">
											請求編號: {{item.id}}
										</v-chip>

									</v-col>
								</v-row>

								<v-row>
									<v-col cols="12" sm="12" md="12">
										<div class="lineDiv info-column-name">
											請求網址
										</div>
										<div class="info-code-panel">
											{{item.requestUrl}}
										</div>
									</v-col>
								</v-row>

								<v-row>
									<v-col cols="12" sm="12" md="12">
										<div class="lineDiv info-column-name">
											我方送出請求參數 (request body)
										</div>
										<div class="info-code-panel">
											{{item.requestContent}}
										</div>
									</v-col>
								</v-row>

								<v-row>
									<v-col cols="12" sm="12" md="12">
										<div class="lineDiv info-column-name">
											我方送出請求表頭 (request header)
										</div>
										<div class="info-code-panel">
											{{item.requestHeader}}
										</div>
									</v-col>
								</v-row>

								<v-row>
									<v-col cols="12" sm="12" md="12">
										<div class="lineDiv info-column-name">
											HttpStatusCode (response)
										</div>
										<div class="orange-text">
											{{item.responseStatusCode}}
										</div>
									</v-col>
								</v-row>

								<v-row>
									<v-col cols="12" sm="12" md="12">
										<div class="lineDiv info-column-name">
											三方API回應 (response)
										</div>
										<div class="info-code-panel">
											{{item.responseText}}
										</div>
									</v-col>
								</v-row>
							</div> -->

						</v-expansion-panel-content>

					</v-expansion-panel>

				</v-expansion-panels>

			</v-col>

			<v-col cols="12" md="5" sm="5">

				<!-- 可見公告list -->
				<div class="messagePanel" style="">

					<div v-for="(item , index) in visibleAnnouncement" @click="showAnnouncementDetail(item)"
						class="pointer" :key="index"
						style="margin-bottom: 20px;max-width: 480px;padding: 0px;background: #262626;">

						<div :class="item.isRead? '': 'unread'">

							<div class="f16" style="border-bottom: solid 1px #444;padding: 5px;font-weight: 400;">

								<v-icon small style="color:#c4c69e">email</v-icon>

								<v-chip v-if="!item.isRead" color="red darken-4" x-small class='ml-1 mr-0'
									style="padding: 3px 6px;">
									New*
								</v-chip>

								<span>
									{{item.title}}
								</span>

								<div style="float:right;color:#AAA;" class="f12 mr-1">
									{{ item.createTime | timeWithAddTimezone(userProfile.timeZone)}}
								</div>

							</div>

							<div class="message-card-content">

								<div v-if="isValidImageUrl(item.imageUrl) && isEmptyString(item.content)"
									style="color:#CCC;">
									<span style="text-decoration: underline;">{{$t('Message.__contentContainsImage')}}
									</span>
									<v-icon style="color:#CCC">image</v-icon>
								</div>

								<div v-else>
									{{item.contentStripHtml}}
								</div>

							</div>

						</div>
						<!-- 
				<div style='display:flex'>

					<div class="f2">

						2022-12-01
					</div>

					<div class="f2">
						{{item.createBy}}
					</div>

					<div class="f2">
						未讀
					</div>

				</div> -->

					</div>
				</div>
			</v-col>

		</v-row>

		<!-- type1. 點擊彈出查看 -->
		<v-dialog v-model="isShowDetail" max-width="550px">

			<v-card class="dialog-v-card-border">
				<v-card-title class="grey darken-2">

					{{$t('Message.__BOAnnouncement')}}

					<div class="top-right-item" @click="closeAllModal()">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>

				</v-card-title>

				<v-card-text>

					<div class="popup-message-content">

						<div>

							<div class="text-c ann-popup-title">
								{{targetPopupAnnouncement.title}}
							</div>

							<div class="text-r">
								<span class="ml-4 f12 sub-title-text">
									{{ targetPopupAnnouncement.createTime | timeWithAddTimezone(userProfile.timeZone)}}
									({{targetPopupAnnouncement.createBy}})
								</span>
							</div>

						</div>

						<div class="mt-2" v-html="toAnchorText(targetPopupAnnouncement.content)">
						</div>

						<div class="mt-2 text-c" v-if="isValidImageUrl(targetPopupAnnouncement.imageUrl)">
							<a :href="targetPopupAnnouncement.imageUrl" target="_blank">
								<img :src="targetPopupAnnouncement.imageUrl" class="ann-image-size">
							</a>
						</div>
					</div>

				</v-card-text>
			</v-card>

		</v-dialog>

		<!-- type 2. 首頁自動彈出的 -->
		<v-dialog v-model="isShowPopupHint" max-width="550px" persistent>

			<v-card v-for="(item , index) in popupAnnouncement" :key="index" class="dialog-v-card-border"
				v-show="isFirstItemOfArray(item)">
				<v-card-title class="grey darken-2">
					{{$t('Message.__BOAnnouncement')}}

					<div class="top-right-item" @click="setPopupMessageRead(item)">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>

				</v-card-title>

				<v-card-text>

					<div class="popup-message-content">

						<div>

							<div class="text-c ann-popup-title">
								{{item.title}}
							</div>

							<div class="text-r">
								<span class="ml-4 f12 sub-title-text">
									{{ item.createTime | timeWithAddTimezone(userProfile.timeZone)}}
									({{item.createBy}})
								</span>
							</div>

						</div>

						<div class="mt-2" v-html="toAnchorText(item.content)">

						</div>

						<div class="mt-2  text-c" v-if="isValidImageUrl(item.imageUrl)">
							<a :href="item.imageUrl" target="_blank">
								<img :src="item.imageUrl" class="ann-image-size">
							</a>
						</div>
					</div>

				</v-card-text>
			</v-card>

		</v-dialog>

		<v-dialog v-model="isShowOverviewSetting" max-width="500px" persistent>

			<v-card>

				<v-card-title class="grey darken-2">
					查詢設置

					<div class="top-right-item" @click="closeAllSearchAgain()">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>

				</v-card-title>

				<v-card-text style="min-height: 400px;" class="mt-2">

					<v-row>

						<v-col cols="12" md="12" sm="12">
							<v-checkbox :label="'隱藏MYC代理 (部分報表分類)'"
								v-model="searchFilterFinanceOverview.isExcludeMYCAgent">
							</v-checkbox>
						</v-col>

						<v-col cols="12" md="12" sm="12">
							<v-text-field :label="'每個分類下顯示最多幾筆'" v-model="searchFilterFinanceOverview.pageSize"
								style="width: 250px;">
							</v-text-field>
						</v-col>

					</v-row>

				</v-card-text>

			</v-card>

		</v-dialog>

	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import HomePageSummaryApi from '@/api/statistics/home-page-summary';
import BOAnnouncementApi from '@/api/operator-management/BOAnnouncement';
import ChartApi from '@/api/statistics/chart';
import * as Model from '@/models/interfaces/statistics/home-page-summary';
import * as AnnModel from '@/models/interfaces/operator-management/BOAnnouncement';
import { datetimeMixin, startEndDate, timeWithTimezone, daysAgo, weeksAgo, monthsAgo, formatMixin } from '@/utilities/datetime-format';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { commonMethodMixin } from '@/utilities/common-methods';
import { namespace } from 'vuex-class';
import * as ChartModel from '@/models/interfaces/statistics/chart';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import LineChart from '@/components/statistics/LineChartPopup.vue';

const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';
@Component({ components: { LineChart }, mixins: [commonDisplayFiltersMixin, datetimeMixin, commonMethodMixin] })
export default class HomePageSummary extends Vue {
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;

	isShowDetail: boolean = false;
	isShowPopupHint: boolean = false;
	isShowOverviewSetting: boolean = false;
	isAlreadyLoadOverview: boolean = false;
	isShowChartDiv: boolean = false;

	displayChartType: number = 6778;
	chartDataTargetCurrency: string = 'MYR';
	panelFlags: number[] = [];
	summary: Model.IReponse[] = [];
	financeOverviewReport: any = {};
	searchFilterFinanceOverview: any = {};
	searchHomePageTodayInfo: Model.IHomePageTodayInfo = {} as Model.IHomePageTodayInfo;
	searchChartData: ChartModel.ICommonChartReponse = {} as ChartModel.ICommonChartReponse;

	visibleAnnouncement: AnnModel.IBOAnnouncement[] = [];
	popupAnnouncement: AnnModel.IBOAnnouncement[] = [];
	targetPopupAnnouncement: AnnModel.IBOAnnouncement = {} as AnnModel.IBOAnnouncement;

	get headers() {
		const headers = [
			{ text: this.$t('TableHeader.__createTime'), value: 'createTime' },
			{ text: this.$t('TableHeader.__createBy'), value: 'createBy' },
			{ text: this.$t('TableHeader.__title'), value: 'title' },
			{ text: this.$t('TableHeader.__imageUrl'), value: 'imageUrl' },
			{ text: this.$t('TableHeader.__description'), value: 'content' },
			{ text: '', value: 'moreAction' },
		];

		return headers;
	}

	get chartCurrencyList() {
		const headers = [
			{ text: 'MYR', value: 'MYR' },
			{ text: 'MYR100', value: 'MYR100' },
			{ text: 'IDR', value: 'IDR' },
			{ text: 'THB', value: 'THB' },
			{ text: 'AUD', value: 'AUD' },
			{ text: 'SGD', value: 'SGD' },
		];
		return headers;
	}

	get chartTypeDropdown() {
		const headers = [
			{ text: '注單數', value: 6778 },
			{ text: '投注金額', value: 6770 },
		];
		return headers;
	}

	get hasSuperPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSetting_SystemConfigSetting) >= 0;
	}

	whenChangeChartCurrency() {
		this.showChart();
	}

	getVisibleAnnouncement() {
		BOAnnouncementApi.getVisibleAnnouncement().then((res) => {
			this.visibleAnnouncement = res.data.items;
			this.popupAnnouncement = res.data.popupAnns;

			if (this.popupAnnouncement != null && this.popupAnnouncement.length > 0) {
				this.isShowPopupHint = true;
			}
		});
	}

	expandAllPanel() {
		this.panelFlags = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15];
	}

	closeAllPanel() {
		this.panelFlags = [];
	}

	setDateRangeAndSearch(monthText: string) {
		if (monthText === 'lastMonth') {
			this.searchFilterFinanceOverview.beginDateTW = startEndDate(monthsAgo(1)[0] + 8 * 3600 * 1000);
		} else {
			this.searchFilterFinanceOverview.beginDateTW = startEndDate(monthsAgo(0)[0] + 8 * 3600 * 1000);
		}
		if (this.hasSuperPermission) {
			this.searchFilterFinanceOverview.isExcludeMYCAgent = true;
			this.searchFilterFinanceOverview.pageSize = 30;
			this.searchFinanceOverviewReport();
		}
	}

	searchFinanceOverviewReport() {
		// 判斷至少讀取過一次總覽API
		this.isAlreadyLoadOverview = true;
		EventBus.$emit('show_mask', null);
		HomePageSummaryApi.getHomePageFinanceOverview(this.searchFilterFinanceOverview)
			.then((res) => {
				this.financeOverviewReport = res.data;
				console.log(res);
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	showChart() {
		const chartType = this.displayChartType;
		const filter: ChartModel.IChartRequest = {} as ChartModel.IChartRequest;

		EventBus.$emit('show_mask', null);
		filter.currency = this.chartDataTargetCurrency;

		ChartApi.getChartV2(chartType, filter)
			.then((res) => {
				this.searchChartData = res.data;

				this.$nextTick(() => {
					const myConfig = {
						isHideCloseButton: true,
						ticksSuffixText: 'M',
						forceColor: '#156aa3',
						CanvasHeight: 75,
					};

					const targetRef: any = this.$refs.refChartPopup;

					if (targetRef === undefined) {
						setTimeout(() => {
							const targetRef2: any = this.$refs.refChartPopup;
							targetRef2.initialCanvasData(myConfig);
						}, 3000);
					} else {
						targetRef.initialCanvasData(myConfig);
					}
				});
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	showDailyListPopup() {
		const myFilter: any = {
			querySqlID: 900471,
			currency: this.chartDataTargetCurrency,
			BeginDateTW: this.searchFilterFinanceOverview.beginDateTW,
		};

		const myConfig: any = {
			maxWidth: 650,
			isShowCloseButton: true,
		};

		EventBus.$emit(`globalShowCustomizedColumnsDisplayPopupDialog`, myFilter, myConfig);
	}

	showEachDayTotalRoundPopup() {
		const myFilter: any = {
			querySqlID: 900464,
		};

		const myConfig: any = {
			maxWidth: 650,
			isShowCloseButton: true,
		};

		EventBus.$emit(`globalShowCustomizedColumnsDisplayPopupDialog`, myFilter, myConfig);
	}

	getHomePageTodayInfo() {
		const myFilter: any = {};
		this.resetValueToZero();

		// 判斷至少讀取過一次總覽API
		this.isAlreadyLoadOverview = true;

		HomePageSummaryApi.getHomePageTodayInfo(myFilter)
			.then((res) => {
				this.searchHomePageTodayInfo = res.data;
			})
			.finally(() => {});
	}

	reloadHomePageTodayInfo() {
		EventBus.$emit('show_mask', null);

		this.getHomePageTodayInfo();

		setTimeout(() => {
			EventBus.$emit('hide_mask', null);
		}, 750);
	}

	showAnnouncementDetail(item: any) {
		this.isShowDetail = true;
		this.targetPopupAnnouncement = item;

		// 設已讀
		BOAnnouncementApi.SetAnnouncementRead(item.boAnnouncementID).then((res) => {
			item.isRead = true;
		});
	}

	showOverviewSettingDialog() {
		this.isShowOverviewSetting = true;
	}

	closeAllSearchAgain() {
		this.closeAllModal();
		this.searchFinanceOverviewReport();
	}

	closeAllModal() {
		this.isShowDetail = false;
		this.isShowOverviewSetting = false;
	}

	isFirstItemOfArray(item: AnnModel.IBOAnnouncement) {
		if (this.popupAnnouncement.length <= 0) {
			return false;
		}

		return this.popupAnnouncement[0].boAnnouncementID === item.boAnnouncementID;
	}

	setPopupMessageRead(item: any) {
		// 設已讀
		BOAnnouncementApi.SetAnnouncementRead(item.boAnnouncementID).then((res) => {
			item.isRead = true;
			this.popupAnnouncement = this.popupAnnouncement.filter((each) => !each.isRead);
			this.visibleAnnouncement.forEach((each) => {
				if (each.boAnnouncementID === item.boAnnouncementID) {
					each.isRead = true;
				}
			});

			if (this.popupAnnouncement.length <= 0) {
				this.isShowPopupHint = false;
			}
		});
	}

	isValidImageUrl(url: string) {
		const vm: any = this;

		if (vm.isEmptyString(url)) {
			return false;
		}

		if (!url.includes('http')) {
			return false;
		}

		return true;
	}

	resetValueToZero() {
		this.searchHomePageTodayInfo = {} as Model.IHomePageTodayInfo;
	}

	showEachMerchantOnlinePlayers() {
		const myFilter: any = {
			querySqlID: 900451,
		};

		const myConfig: any = {
			maxWidth: 700,
			isShowCloseButton: true,
		};

		EventBus.$emit(`globalShowCustomizedColumnsDisplayPopupDialog`, myFilter, myConfig);
	}

	showEachCurrencyOnlinePlayers() {
		const myFilter: any = {
			querySqlID: 900455,
		};

		const myConfig: any = {
			maxWidth: 450,
			isShowCloseButton: true,
		};

		EventBus.$emit(`globalShowCustomizedColumnsDisplayPopupDialog`, myFilter, myConfig);
	}

	showEachMerchantTotalPlayers(myTargetDayText: string = 'today') {
		const myFilter: any = {
			querySqlID: 900452,
			targetDayText: myTargetDayText,
			orderBy: 'playercount',
		};

		const myConfig: any = {
			maxWidth: 800,
			isShowCloseButton: true,
		};

		EventBus.$emit(`globalShowCustomizedColumnsDisplayPopupDialog`, myFilter, myConfig);
	}

	showEachMerchantTotalRound(myTargetDayText: string = 'today') {
		const myFilter: any = {
			querySqlID: 900452,
			targetDayText: myTargetDayText,
			orderBy: 'totalRound',
		};

		const myConfig: any = {
			maxWidth: 800,
			isShowCloseButton: true,
		};

		EventBus.$emit(`globalShowCustomizedColumnsDisplayPopupDialog`, myFilter, myConfig);
	}

	loadHomePageReportFirstTime() {
		setTimeout(() => {
			if (this.hasSuperPermission) {
				this.setDateRangeAndSearch('thisMonth');
				this.getHomePageTodayInfo();
				this.showChart();
			}
		}, 500);

		// 為了避免 hasSuperPermission 太慢載入 N秒後再判斷一次
		setTimeout(() => {
			if (this.hasSuperPermission && !this.isAlreadyLoadOverview) {
				this.setDateRangeAndSearch('thisMonth');
				this.getHomePageTodayInfo();
				this.showChart();
			}
		}, 3000);
	}
	toAnchorText(content: string) {
		const vm: any = this;

		if (vm.isEmptyString(content)) {
			return '';
		}

		const regex = /(https?:\/\/[^\s]+)/g;
		let myResultText: string = content.replace(regex, '<a href="$1" target="_blank">$1</a>');

		// @ts-ignore
		myResultText = myResultText.replaceAll('_', '');
		return myResultText;
	}

	mounted() {
		window.scrollTo({ top: 0, left: 0 });
		this.getVisibleAnnouncement();
		this.resetValueToZero();

		// 報表區塊 (自己人使用才顯示此區塊)
		this.loadHomePageReportFirstTime();
	}
}
</script>


<style scoped>
.v-expansion-panel-header--active {
	min-height: 48px !important;
}

.message-card-content {
	font-size: 12px;
	padding: 5px 15px;
	width: 100%;
	min-height: 90px;
	max-height: 90px;

	color: #aaa;
	position: relative;
	overflow: hidden;
	text-overflow: ellipsis;
}

.f1 {
	flex: 1;
}
.f2 {
	flex: 2;
}
.f3 {
	flex: 3;
}
.f4 {
	flex: 4;
}
.f5 {
	flex: 5;
}

.popup-message-content {
	padding: 10px 10px;
	min-height: 400px;
	white-space: pre-line;
}

.avator {
	background: #2d2d2d;
	border-radius: 50%;
	width: 75px;
	height: 75px;
	margin: 0 auto;
	text-align: center;
	padding-top: 10px;
}

.ann-popup-title {
	font-size: 24px;
	color: #ddd;
	border-bottom: solid 1px #444;
	padding: 5px;
}

.seperateLine {
	color: #4c6178cc;
	margin-left: 5px;
	margin-right: 5px;
	font-weight: 100;
	font-size: 20px;
}

.ann-image-size {
	margin-left: 0 auto;
	margin-right: 0 auto;
	margin-top: 25px;
}

@media only screen and (max-width: 760px) {
	.messagePanel {
		padding: 0px 5px;
		max-width: 800px;
	}

	.ann-image-size {
		width: 300px;
	}

	.v-card__text {
		padding: 5px !important;
	}
}

@media only screen and (min-width: 761px) {
	.messagePanel {
		padding: 0px 20px;
		max-width: 800px;
	}

	.ann-image-size {
		max-width: 100%;
	}
}
</style>

