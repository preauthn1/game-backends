<template>
	<v-app id="MerchantSummaryFinanceComparePage" v-if="renderComponent" class="app">
		<v-expansion-panels v-model="panel" multiple>
			<v-expansion-panel>
				<v-expansion-panel-content>
					<v-flex>
						<v-row>
							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete
										clearable
										v-model="searchFilter.vendorConfigID"
										v-if="isRenderDropdown"
										@change="whenSelectChanged()"
										class="merchantDropdownItems"
										:items="vendorConfigDropdownItems"
										:label="$t('TableHeader.__vendorConfig')"
									>
										<template v-slot:selection="data">
											<span :title="data.item.value">
												{{ data.item.text }}
											</span>
										</template>
									</v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<date-picker v-model="datetimeRangePicker.searchStartTimeRange" type="datetime" value-type="timestamp" range :placeholder="$t('TableHeader.__time')"> </date-picker>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3" class="quick-time" style="padding-top: 30px">
								<v-chip class="mr-2" :class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'thisWeek' }" @click="globalQuicklyPickTimeRange('thisWeek')">
									<v-icon left>mdi-alarm-check</v-icon>
									{{ $t(`TableHeader.__thisWeek`) }}
								</v-chip>

								<v-chip class="mr-2" :class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'lastWeek' }" @click="globalQuicklyPickTimeRange('lastWeek')">
									<v-icon left>mdi-alarm-check</v-icon>
									{{ $t(`TableHeader.__lastWeek`) }}
								</v-chip>
								<v-chip class="mr-2" :class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'thisMonth' }" @click="globalQuicklyPickTimeRange('thisMonth')">
									<v-icon left>mdi-alarm-check</v-icon>
									{{ $t(`TableHeader.__thisMonth`) }}
								</v-chip>
								<v-chip class="mr-2" :class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'lastMonth' }" @click="globalQuicklyPickTimeRange('lastMonth')">
									<v-icon left>mdi-alarm-check</v-icon>
									{{ $t(`TableHeader.__lastMonth`) }}
								</v-chip>
								<!-- </div> -->
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<!-- <div class="item right"> -->
								<div class="item">
									<v-btn color="yellow darken-1" text class="btn search ml-1" @click.stop="search">
										<v-icon left>search</v-icon>
										{{ $t('Button.__search') }} </v-btn
									>&#9;
									<v-btn color="pink lighten-3" text class="btn search ml-1" @click.stop="clear">
										<v-icon left>clear</v-icon>
										{{ $t('Button.__reset') }}
									</v-btn>
									<v-btn color="green lighten-3" text class="btn ml-1" style="margin-top: 15px" @click.stop="showBatchExportDialog">
										<v-icon left>cloud_download</v-icon>
										匯出
									</v-btn>
								</div>
							</v-col>
						</v-row>
					</v-flex>
				</v-expansion-panel-content>
			</v-expansion-panel>
		</v-expansion-panels>

		<div>
			<div v-for="(item, index) in searchResult.hintWarningMessages" :key="index" class="text-c">
				<div class="mt-2 text-l warningMsg">⚠️⚠️{{ item }}</div>
			</div>
		</div>

		<v-card>
			<div style="padding: 15px">
				<div v-if="(hasPermissionSuperAdmin || hasPermissionInvoiceExport) && searchResult && searchResult.items">
					<div class="ml-3">
						<v-chip color="pink darken-3"> 玩家視角 </v-chip>

						<v-chip :title="'遊戲商統計時區'" class="ml-1"> +{{ searchResult.gameVendorReportTimezone }}時區 </v-chip>

						<v-chip class="ml-1" :title="'商務號／代理識別號'">
							{{ searchResult.assignVendorID }}
						</v-chip>

						<v-chip color="red darken-4" v-for="(tag, index) in searchResult.extraTags" :key="index" small class="mr-1 mb-1" :title="''">
							{{ tag }}
						</v-chip>

						<div v-if="searchResult && searchResult.totalSummary" class="text-c card-totalWinLoss">
							<div style="font-weight: 400; color: #ffea78">
								<span style="font-size: 16px" v-if="searchResult.isHideTotalSum">
									<div>⚠️ 用下方臨時表比對數據</div>

									<!-- <div v-for="item in searchResult.eachCurrencyTotalSum" :key="item.currency" style="text-align: left; color: #fff">
									<v-chip small style="min-width: 55px" class="mr-1">{{ item.currency }}</v-chip> {{ item.winLossAmount | amountDisplay }}
								</div> -->
								</span>
								<span style="font-size: 22px" v-else>{{ searchResult.totalSummaryAfterAdjustment.totalWinLossAmount | amountDisplay }}</span>
							</div>
							<div class="sub-title-text">
								總輸贏 <span class="ml-1" v-if="searchResult.totalRTP >= 100">(*{{ searchResult.totalRTP }} RTP)</span>
							</div>
						</div>

						<div v-if="searchResult && searchResult.totalSummary && searchResult.jackpotMerchantWinLossAdjustAmount > 0" class="text-c card-totalWinLoss">
							<div style="font-size: 22px; font-weight: 400; color: #ffea78">
								{{ (searchResult.totalSummary.totalWinLossAmount - searchResult.jackpotMerchantWinLossAdjustAmount) | amountDisplay }}
							</div>
							<div class="sub-title-text">總輸贏(排除拉JP商戶)</div>
						</div>
					</div>

					<div class="mt-2">
						<span class="link-text-grey f14 ml-4 mr-2" @click="showDataExistMerchants" v-if="getDataExistMerchantsCount > 0">商戶數:{{ getDataExistMerchantsCount }} (有數據) </span>

						<span class="link-text-grey f14 ml-2 mr-2" @click="showMerchants" v-if="getMerchantsCount > 0">商戶數:{{ getMerchantsCount }} </span>

						<span class="link-text-grey f14 ml-2 mr-2" @click="showEachDateEventSummaryReportPopup"> 查看活動獎勵 </span>

						<span class="link-text-grey f14 ml-2 mr-2" @click="copyDebugSQL"> 複製SQL </span>
					</div>
				</div>

				<div v-if="searchResult.eachCurrencyTotalSum && searchResult.eachCurrencyTotalSum.length > 0">
					<div class="mt-4 yellow-text">⚠️服務器含多組貨幣。請直接用此表格比對 (數據誤差不大即可)</div>
					<div class="mt-1 yellow-text">⚠️服務器含多組貨幣。請直接用此表格比對 (數據誤差不大即可)</div>
					<div class="mt-1 yellow-text">⚠️服務器含多組貨幣。請直接用此表格比對 (數據誤差不大即可)</div>

					<div class="v-data-table theme--dark">
						<div class="v-data-table__wrapper">
							<table style="width: 400px">
								<thead class="v-data-table-header">
									<tr>
										<th>貨幣</th>
										<th class="text-r">注單數</th>
										<th class="text-r">總輸贏 (原始幣值)</th>
									</tr>
								</thead>

								<tbody class="">
									<tr v-for="item in searchResult.eachCurrencyTotalSum" :key="item.currency" style="text-align: left; color: #fff">
										<td>{{ item.currency }}</td>
										<td class="text-r tdText2">{{ item.totalRound | countDisplay }}</td>
										<td class="text-r tdText2">
											<span :class="item.winLossAmount > 0 ? 'minus-red' : ''">
												{{ item.winLossAmount | amountDisplay }}
											</span>
										</td>
										<td class="text-r tdText2">
											<v-btn small color="teal darken-3" @click="showTargetCurrencyMerchantReportPopup(item.currency)">商戶數據</v-btn>
										</td>
									</tr>
								</tbody>
							</table>
						</div>
					</div>

					<hr class="mb-4" />
				</div>

				<div class="v-data-table theme--dark">
					<div class="v-data-table__wrapper">
						<table class="">
							<thead class="v-data-table-header">
								<tr>
									<th style="width: 8%">日期</th>
									<th class="text-r">
										總局數

										<v-tooltip top>
											<template v-slot:activator="{ on, attrs }">
												<v-icon color="" dark v-bind="attrs" v-on="on" small> info </v-icon>
											</template>
											<span>以PGS的結算日做為聚合依據 </span>
										</v-tooltip>
									</th>
									<th class="text-r">總投注額</th>
									<th class="text-r">玩家輸贏</th>
									<th class="text-r">校正注單數</th>
									<th class="text-r">商戶數</th>
									<th class="text-l" style="width: 40%">特殊標籤</th>
									<th></th>
								</tr>
							</thead>

							<tbody v-for="(item, index) in searchResult.items">
								<tr :id="getElementID(item.statDateText)" class="pointer" @click="switchExtraDivOpenOrClose(index)">
									<td>
										<span v-if="item.statDateText == '1970-01-01'" class="yellow-text">⚠️手動調整值</span>
										<span v-else>{{ item.statDateText }}</span>
									</td>

									<td class="text-r">
										{{ item.pgsStatDB.totalRound | countDisplay }}
									</td>
									<td class="text-r">
										{{ item.pgsStatDB.totalBetAmount | amountDisplay }}
									</td>
									<td class="text-r">
										<span :class="item.pgsAfterAdjustment.totalWinLossAmount < 0 ? 'minus-red' : ''">
											{{ item.pgsAfterAdjustment.totalWinLossAmount | amountDisplay }}
										</span>
									</td>
									<td class="text-r">
										<span :class="zeroThenGreyClass(item.totalAdjustRecordCount)">
											<span v-if="item.isAnyIgnoreWagers">⚠️</span>

											<span :class="item.isAnyIgnoreWagers ? 'yellow-text' : ''">
												{{ item.totalAdjustRecordCount }}
											</span>
										</span>
									</td>

									<td class="text-r">
										{{ item.merchantCount }}
									</td>

									<td class="text-l">
										<div v-if="item.isFetchDataAllZero">
											<v-btn color="grey darken-1" class="mr-1" small @click="showEditMemoPopup(item)" v-if="hasPermissionSuperAdmin">
												<v-icon small>edit</v-icon>
												{{ item.extraMemoInfo }}
											</v-btn>
											<!-- 沒資料的情況 -->
											<!-- 沒資料的情況 -->
											<v-chip v-if="item.isFetchDataAllZero" color="yellow darken-2" class="mr-1"> 尚未Fetch三方後台數據 </v-chip>

											<v-btn color="cyan darken-1" class="mr-2" small v-if="!isHideExportButton" @click="exportRecordsOfTargetDate(item)">
												<v-icon small class="mr-1">cloud_download</v-icon>
												注單匯出
											</v-btn>

											<v-btn v-if="item.isFetchDataAllZero" color="lime darken-3" class="mr-2" small @click="showTriggerFetchThirdAccounting(item)"> 重撈三方 </v-btn>
										</div>

										<div v-else>
											<div v-if="item.statDateText != '1970-01-01'">
												<!-- 有資料數據的情況 -->
												<v-btn color="cyan darken-1" class="mr-2" small v-if="!isHideExportButton" @click="exportRecordsOfTargetDate(item)">
													<v-icon small class="mr-1">cloud_download</v-icon>
													注單匯出
												</v-btn>

												<v-btn
													color="grey darken-1"
													class="mr-1"
													small
													@click="showEditMemoPopup(item)"
													v-if="hasPermissionSuperAdmin && item.extraMemoInfo != null && item.extraMemoInfo.length > 0"
												>
													<v-icon small>edit</v-icon>
													{{ item.extraMemoInfo }}
												</v-btn>

												<v-btn
													color="grey darken-1"
													class="mr-1"
													small
													@click="showEditMemoPopup(item)"
													v-if="hasPermissionSuperAdmin && (item.extraMemoInfo == null || item.extraMemoInfo.length <= 0)"
												>
													<v-icon small>edit</v-icon>
												</v-btn>

												<v-btn color="teal darken-3" class="mr-1" small v-if="item.pgsStatDB.totalRound > 0" @click="showThirdPartyGameReportPopup(item)">
													遊戲報表(原廠)
												</v-btn>

												<v-btn color="teal darken-3" class="mr-1" small v-if="item.pgsStatDB.totalRound > 0" @click="showGameReportPopup(item)"> 遊戲報表 </v-btn>

												<v-btn color="teal darken-3" class="mr-1" small v-if="item.pgsStatDB.totalRound > 0" @click="showMerchantReportPopup(item)"> 商戶報表 </v-btn>

												<v-btn color="lime darken-3" class="mr-2" small v-if="item.pgsStatDB.totalRound > 0" @click="showTriggerFetchThirdAccounting(item)"> 重撈三方 </v-btn>

												<v-chip v-if="!item.isRoundNotEqualOnly_ByGameVendorSettleDate && !item.isCompareCorrect_AfterAdjustment" color="purple lighten-1" class="mr-1">
													校正後數據有落差
												</v-chip>

												<v-chip v-if="!item.isCompareCorrect_ByGameVendorSettleDate" color="red lighten-1" class="mr-1"> 數據有落差(參考可能原因) </v-chip>

												<v-chip v-if="item.isCompareNotEqual_ButGapSmall_WithAGGSettleDate" color="blue lighten-3" class="mr-1"> 有微小數字落差 </v-chip>

												<v-chip v-if="item.isRoundNotEqualOnly_ByGameVendorSettleDate" color="green darken-1" class="mr-1"> ⭐僅局數誤差 </v-chip>

												<div v-if="item.isToday" style="display: inline-block" class="hint-row"><v-icon small>error</v-icon> 當日數據會不準確</div>
											</div>
										</div>
									</td>
								</tr>

								<tr v-if="isDetailVisible(index)" class="noHover" style="border-bottom: solid 1px #eee">
									<td></td>
									<td colspan="6">
										<div :id="getDetailElementID(item.statDateText, index)" class="v-data-table theme--dark" style="padding-bottom: 30px; border-radius: 0px">
											<div class="v-data-table__wrapper">
												<!-- 
												<h2>
													總局數
												</h2> -->

												<div class="text-c" style="padding: 5px; color: #ffcf49">
													<span style="color: #fff; font-weight: 400" class="mr-1">{{ searchResult.vendorConfigName }} </span>

													<span style="color: #aaa; font-weight: 400" class="mr-1"> ({{ searchResult.assignVendorID }}) </span>
													數據日期：{{ item.statDateText }}
													<v-icon class="ml-2" small @click="copyCompareText(item)" title="'複製文案'"> file_copy </v-icon>
												</div>

												<table style="background: #505050; max-width: 1200px">
													<thead class="v-data-table-header innerTable">
														<tr>
															<th class="text-l">項目</th>
															<th class="text-r">總局數</th>
															<th class="text-r">總投注額</th>
															<th class="text-r">玩家輸贏</th>
														</tr>
													</thead>

													<tbody>
														<tr>
															<td>
																校正扣除

																<v-tooltip top>
																	<template v-slot:activator="{ on, attrs }">
																		<v-icon color="" dark v-bind="attrs" v-on="on" small> info </v-icon>
																	</template>
																	<span>PGS認定結算日為 {{ item.statDateText }}，而遊戲商認定結算日為其他日期 </span>
																</v-tooltip>
															</td>
															<td class="text-r">
																<span
																	:title="item.pgsSettleThisDate_But_ThirdSettleOtherDate.allPerGameDetailIDs"
																	@click="copyTextOrSkip(item.pgsSettleThisDate_But_ThirdSettleOtherDate.allPerGameDetailIDs)"
																	:class="zeroThenGreyClass(item.pgsSettleThisDate_But_ThirdSettleOtherDate.totalRound)"
																>
																	{{ item.pgsSettleThisDate_But_ThirdSettleOtherDate.totalRound | countDisplay }}
																</span>
															</td>
															<td class="text-r">
																<span :class="zeroThenGreyClass(item.pgsSettleThisDate_But_ThirdSettleOtherDate.totalBetAmount)">
																	{{ item.pgsSettleThisDate_But_ThirdSettleOtherDate.totalBetAmount | amountDisplay }}
																</span>
															</td>
															<td class="text-r">
																<span :class="zeroThenGreyClass(item.pgsSettleThisDate_But_ThirdSettleOtherDate.totalWinLossAmount)">
																	{{ item.pgsSettleThisDate_But_ThirdSettleOtherDate.totalWinLossAmount | amountDisplay }}
																</span>
															</td>
														</tr>

														<tr>
															<td>
																校正補回

																<v-tooltip top>
																	<template v-slot:activator="{ on, attrs }">
																		<v-icon color="" dark v-bind="attrs" v-on="on" small> info </v-icon>
																	</template>
																	<span>遊戲商認定結算日為 {{ item.statDateText }}，而PGS認定結算日為其他日期 </span>
																</v-tooltip>
															</td>
															<td class="text-r">
																<span
																	:title="item.thirdSettleThisDate_But_PgsSettleOtherDate.allPerGameDetailIDs"
																	@click="copyTextOrSkip(item.thirdSettleThisDate_But_PgsSettleOtherDate.allPerGameDetailIDs)"
																	:class="zeroThenGreyClass(item.thirdSettleThisDate_But_PgsSettleOtherDate.totalRound)"
																>
																	{{ item.thirdSettleThisDate_But_PgsSettleOtherDate.totalRound | countDisplay }}
																</span>
															</td>
															<td class="text-r">
																<span :class="zeroThenGreyClass(item.thirdSettleThisDate_But_PgsSettleOtherDate.totalBetAmount)">
																	{{ item.thirdSettleThisDate_But_PgsSettleOtherDate.totalBetAmount | amountDisplay }}
																</span>
															</td>
															<td class="text-r">
																<span :class="zeroThenGreyClass(item.thirdSettleThisDate_But_PgsSettleOtherDate.totalWinLossAmount)">
																	{{ item.thirdSettleThisDate_But_PgsSettleOtherDate.totalWinLossAmount | amountDisplay }}
																</span>
															</td>
														</tr>

														<tr v-if="item.accountingIgnoreRecordSummary && item.accountingIgnoreRecordSummary.totalRound > 0">
															<td>
																<span class="yellow-text">標記帳務排除</span>

																<v-tooltip top>
																	<template v-slot:activator="{ on, attrs }">
																		<v-icon color="yellow" dark v-bind="attrs" v-on="on" small> info </v-icon>
																	</template>
																	<span>技術人員標記不算進帳務 </span>
																</v-tooltip>
															</td>
															<td class="text-r">
																<span
																	:title="item.accountingIgnoreRecordSummary.allPerGameDetailIDs"
																	@click="copyTextOrSkip(item.accountingIgnoreRecordSummary.allPerGameDetailIDs)"
																	:class="zeroThenGreyClass(item.accountingIgnoreRecordSummary.totalRound)"
																>
																	{{ item.accountingIgnoreRecordSummary.totalRound | countDisplay }}
																</span>
															</td>
															<td class="text-r">
																<span :class="zeroThenGreyClass(item.accountingIgnoreRecordSummary.totalBetAmount)">
																	{{ item.accountingIgnoreRecordSummary.totalBetAmount | amountDisplay }}
																</span>
															</td>
															<td class="text-r">
																<span :class="zeroThenGreyClass(item.accountingIgnoreRecordSummary.totalWinLossAmount)">
																	{{ item.accountingIgnoreRecordSummary.totalWinLossAmount | amountDisplay }}
																</span>
															</td>
														</tr>

														<tr>
															<td>統計DB</td>
															<td class="text-r">
																{{ item.pgsStatDB.totalRound | countDisplay }}
															</td>
															<td class="text-r">
																{{ item.pgsStatDB.totalBetAmount | amountDisplay }}
															</td>
															<td class="text-r">
																{{ item.pgsStatDB.totalWinLossAmount | amountDisplay }}
															</td>
														</tr>

														<tr>
															<td>統計DB(校正後)</td>
															<td class="text-r">
																{{ item.pgsAfterAdjustment.totalRound | countDisplay }}
															</td>
															<td class="text-r">
																{{ item.pgsAfterAdjustment.totalBetAmount | amountDisplay }}
															</td>
															<td class="text-r">
																{{ item.pgsAfterAdjustment.totalWinLossAmount | amountDisplay }}
															</td>
														</tr>

														<!-- <tr>
															<td>統計數據(以三方結算日當基準)</td>
															<td class="text-r">
																{{ item.pgsAggregateByThirdSettleDate.totalRound | countDisplay }}
															</td>
															<td class="text-r">
																{{ item.pgsAggregateByThirdSettleDate.totalBetAmount | amountDisplay }}
															</td>
															<td class="text-r">
																{{ item.pgsAggregateByThirdSettleDate.totalWinLossAmount | amountDisplay }}
															</td>
														</tr> -->

														<tr>
															<td>三方後台／三方API數字</td>
															<td class="text-r">
																{{ item.fetchThirdPartyStatisticReport.totalRound | countDisplay }}
															</td>
															<td class="text-r">
																{{ item.fetchThirdPartyStatisticReport.totalBetAmount | amountDisplay }}
															</td>
															<td class="text-r">
																{{ item.fetchThirdPartyStatisticReport.totalWinLossAmount | amountDisplay }}
															</td>
														</tr>

														<tr
															v-if="
																!item.isCompareCorrect_AfterAdjustment ||
																!item.isCompareCorrect_ByGameVendorSettleDate ||
																item.isRoundNotEqualOnly_ByGameVendorSettleDate ||
																item.isCompareNotEqual_ButGapSmall_WithAGGSettleDate
															"
														>
															<td>
																<span style="color: #f97e44"> 誤差值 </span>

																<v-tooltip top color="blue darken-1">
																	<template v-slot:activator="{ on, attrs }">
																		<div v-bind="attrs" v-on="on" style="display: inline-block; color: #bbb" class="ml-2">
																			<v-icon small color="grey"> error </v-icon>

																			<span style="text-decoration: underline">可能原因</span>
																		</div>
																	</template>
																	<span>
																		<div>- 數據中包含了【試玩模式】資料</div>

																		<div>- 商戶的後台數字有異動過</div>

																		<div>- 我方有注單遺失perGame資料</div>

																		<div>- 若統計數據(以三方結算日當基準) 最新一筆有輕微誤差。再隔日的排程數據會校正、無須擔心</div>
																	</span>
																</v-tooltip>
															</td>
															<td class="text-r">
																<!-- 																
																<span title="校正數據 vs 結算日聚合數據" :class="zeroThenGreyClass(item.diff_AfterAdjustment_vs_AggregateByThirdSettleDate.totalRound)">
																	{{ item.diff_AfterAdjustment_vs_AggregateByThirdSettleDate.totalRound }}
																</span>
																／ -->
																<span
																	title="結算日聚合數據 vs 三方後台數據"
																	:class="zeroThenGreyClass(item.diff_AggregateByThirdSettleDate_vs_FetchThirdPartyStatisticReport.totalRound)"
																>
																	{{ item.diff_AggregateByThirdSettleDate_vs_FetchThirdPartyStatisticReport.totalRound }}
																</span>
															</td>
															<td class="text-r">
																<!-- <span
																	title="校正數據 vs 結算日聚合數據"
																	:class="zeroThenGreyClass(item.diff_AfterAdjustment_vs_AggregateByThirdSettleDate.totalBetAmount)"
																>
																	{{ item.diff_AfterAdjustment_vs_AggregateByThirdSettleDate.totalBetAmount | amountDisplay }}
																</span>
																／ -->

																<span
																	title="結算日聚合數據 vs 三方後台數據"
																	:class="zeroThenGreyClass(item.diff_AggregateByThirdSettleDate_vs_FetchThirdPartyStatisticReport.totalBetAmount)"
																>
																	{{ item.diff_AggregateByThirdSettleDate_vs_FetchThirdPartyStatisticReport.totalBetAmount | amountDisplay }}
																</span>
															</td>
															<td class="text-r">
																<!-- <span
																	title="校正數據 vs 結算日聚合數據"
																	:class="zeroThenGreyClass(item.diff_AfterAdjustment_vs_AggregateByThirdSettleDate.totalWinLossAmount)"
																>
																	{{ item.diff_AfterAdjustment_vs_AggregateByThirdSettleDate.totalWinLossAmount | amountDisplay }}
																</span>
																／ -->

																<span
																	title="結算日聚合數據 vs 三方後台數據"
																	:class="zeroThenGreyClass(item.diff_AggregateByThirdSettleDate_vs_FetchThirdPartyStatisticReport.totalWinLossAmount)"
																>
																	{{ item.diff_AggregateByThirdSettleDate_vs_FetchThirdPartyStatisticReport.totalWinLossAmount | amountDisplay }}
																</span>
															</td>
														</tr>
													</tbody>
												</table>

												<v-divider></v-divider>
											</div>
										</div>
									</td>
								</tr>
							</tbody>

							<tbody>
								<tr v-if="searchResult && searchResult.totalSummary">
									<td>
										{{ $t('TableHeader.__total') }}
									</td>
									<td class="text-r">
										{{ searchResult.totalSummary.totalRound | countDisplay }}
									</td>

									<td class="text-r" style="position: relative">
										<v-tooltip top color="orange darken-4">
											<template v-slot:activator="{ on, attrs }">
												<div
													v-bind="attrs"
													v-on="on"
													class="link-text-white"
													@click="copyTextToClipboard('' + searchResult.totalSummary.totalBetAmount + '／' + searchResult.totalSummary.totalValidAmount)"
												>
													<v-icon small>info</v-icon>
													{{ searchResult.totalSummary.totalBetAmount | amountDisplay }}
												</div>
											</template>
											<span>
												<div class="f16" style="padding: 10px; font-weight: 700">
													<div>投注金額：{{ searchResult.totalSummary.totalBetAmount | amountDisplay }}</div>
													<div>
														有效流水：{{ searchResult.totalSummary.totalValidAmount | amountDisplay }}
														<span v-if="searchResult.totalSummary.totalValidAmount !== searchResult.totalSummaryAfterAdjustment.totalValidAmount">
															｜{{ searchResult.totalSummaryAfterAdjustment.totalValidAmount | amountDisplay }} (校正後)
														</span>
													</div>
												</div>
											</span>
										</v-tooltip>
									</td>

									<td class="text-r">
										<span :class="searchResult.totalSummaryAfterAdjustment.totalWinLossAmount < 0 ? 'minus-red' : ''">
											{{ searchResult.totalSummaryAfterAdjustment.totalWinLossAmount | amountDisplay }}
										</span>
									</td>

									<td>
										<v-chip color="orange darken-3" :title="'RTP'">
											{{ searchResult.totalRTP }}
										</v-chip>
									</td>

									<td>
										<div style="max-width: 50px">
											<span class="link-text-grey" @click="showEachMerchantTotalSumDialogView()">每個商戶數字</span>

											<v-btn color="blue darken-1" small @click="ExportEachDayThirdParty_vs_PGS" class="btn ml-3">
												<v-icon left>mdi-download</v-icon>
												匯出每日數字 (雙方數值比較)
											</v-btn>
										</div>
									</td>

									<td></td>

									<td></td>
								</tr>
							</tbody>
						</table>
					</div>
				</div>
			</div>
		</v-card>

		<!-- 備註 -->
		<v-dialog v-model="dialogVisible.isShowEdit" :max-width="'500px'" persistent>
			<v-card class="createForm">
				<v-card-title class="grey darken-2">
					編輯日期備註
					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>
				<v-card-text>
					<v-row>
						<v-col cols="12" sm="12" md="12">
							<v-flex style="padding: 0px 10px 0px 10px">
								<v-row>
									<v-text-field class="required" v-model="editDateMemo.dateText" :label="'日期'" disabled="disabled" autofocus required clearable> </v-text-field>
								</v-row>

								<v-row>
									<v-text-field ref="refEditDateMemo" v-model="editDateMemo.memo" :label="$t('TableHeader.__memo')" clearable> </v-text-field>
								</v-row>
							</v-flex>
						</v-col>
					</v-row>
				</v-card-text>
				<v-card-actions>
					<v-spacer></v-spacer>
					<v-btn text @click="closeAllModal" class="btn">
						<v-icon left>mdi-cancel</v-icon>
						{{ $t('Button.__cancel') }}
					</v-btn>
					<v-btn color="yellow darken-1" text @click="saveEditDateMemo" class="btn">
						<v-icon left>mdi-check-circle</v-icon>
						{{ $t('Button.__submit') }}
					</v-btn>
				</v-card-actions>
			</v-card>
		</v-dialog>

		<!-- 為了避免css跑版 需要有table的style -->
		<div style="visibility: hidden; height: 1px; width: 1px">
			<v-data-table> </v-data-table>
		</div>

		<v-dialog v-model="dialogVisible.isShowTreeInfo" max-width="500px" scrollable>
			<v-card>
				<v-card-title class="grey darken-2">
					商戶
					<span class="f12"> (共 {{ getMerchantsCount }}位) </span>
				</v-card-title>

				<v-card-text>
					<div class="mt-4" style="min-height: 300px">
						<div v-for="(item, index) in searchResult.merchants" :key="index" style="line-height: 20px; padding: 6px">{{ item.merchantID }}｜{{ item.merchantName }}</div>
					</div>
				</v-card-text>
			</v-card>
		</v-dialog>

		<v-dialog v-model="dialogVisible.isShowTreeInfo2" max-width="500px" scrollable>
			<v-card>
				<v-card-title class="grey darken-2">
					商戶
					<span class="f12"> (共 {{ getDataExistMerchantsCount }}位) </span>
				</v-card-title>

				<v-card-text>
					<div class="mt-4" style="min-height: 300px">
						<div v-for="(item, index) in searchResult.merchantsDataExist" :key="index" style="line-height: 20px; padding: 6px">{{ item.merchantID }}｜{{ item.merchantName }}</div>
					</div>
				</v-card-text>
			</v-card>
		</v-dialog>

		<!-- 批次匯出 -->
		<v-dialog v-model="dialogVisible.isShowExport" max-width="600px">
			<v-card>
				<v-card-title class="grey darken-3">
					<div>
						<div style="display: inline-block">
							<div class="f18">批次匯出</div>
							<div class="sub-title-text">{{ batchExportPreviewInfo.begin }}-{{ batchExportPreviewInfo.end }}</div>
						</div>

						<div class="mr-4 top-right-item">
							<v-btn color="green darken-3 f12" small class="btn mr-1" @click.stop="confirmExport(0)">
								<v-icon left small>cloud_download</v-icon>
								匯出(每日)
							</v-btn>

							<v-btn color="blue darken-3 f12" small class="btn mr-1" @click.stop="confirmExport(999)">
								<v-icon left small>cloud_download</v-icon>
								匯出(加總)
							</v-btn>

							<v-btn color="purple darken-3 f12" small class="btn" @click.stop="exportAllMemo()">
								<v-icon left small>cloud_download</v-icon>
								列出全部備註
							</v-btn>
						</div>
					</div>
				</v-card-title>

				<v-card-text style="height: 700px; overflow: scroll">
					<div style="margin-top: 15px">
						<!-- <div class="text-c mt-4 f14 yellow-text">

							<span class="ml-3">*匯出會集中成一個檔案</span>

							<div>

							</div>

						</div> -->
					</div>

					<div>
						<div>
							<v-checkbox :label="'勾選Funky'" @change="checkAllByGameVendor($event, 250000)"> </v-checkbox>
						</div>
						<div class="checkbox-layer2">
							<div v-for="(item, index) in dropdownWithFilterGameVendor(250000)" :key="index">
								<v-checkbox v-model="item.isChecked" :label="item.text"></v-checkbox>
							</div>
						</div>

						<div>
							<v-checkbox :label="'勾選AGG'" @change="checkAllByGameVendor($event, 290000)"> </v-checkbox>
						</div>
						<div class="checkbox-layer2">
							<div v-for="(item, index) in dropdownWithFilterGameVendor(290000)" :key="index">
								<v-checkbox v-model="item.isChecked" :label="item.text"></v-checkbox>
							</div>
						</div>

						<!-- 	
						
						<div>
							<v-checkbox :label="'勾選AGGLive'" @change="checkAllByGameVendor($event, 330000)">> </v-checkbox>
						</div>

						<div class="checkbox-layer2">
							<div v-for="(item, index) in dropdownWithFilterGameVendor(330000)" :key="index">
								<v-checkbox v-model="item.isChecked" :label="item.text">{{ item.text }} </v-checkbox>
							</div>
						</div> -->

						<!-- <div>
							<v-checkbox :label="'勾選AGGFish'" @change="checkAllByGameVendor($event, 370000)">> </v-checkbox>
						</div>
						<div class="checkbox-layer2">
							<div v-for="(item, index) in dropdownWithFilterGameVendor(370000)" :key="index">
								<v-checkbox v-model="item.isChecked" :label="item.text">{{ item.text }} </v-checkbox>
							</div>
						</div> -->

						<div>
							<v-checkbox :label="'勾選Jili'" @change="checkAllByGameVendor($event, 390000)">> </v-checkbox>
						</div>
						<div class="checkbox-layer2">
							<div v-for="(item, index) in dropdownWithFilterGameVendor(390000)" :key="index">
								<v-checkbox v-model="item.isChecked" :label="item.text">{{ item.text }} </v-checkbox>
							</div>
						</div>

						<!-- <div>
							<v-checkbox :label="'勾選Mario'" @change="checkAllByGameVendor($event, 430000)">> </v-checkbox>
						</div>
						<div class="checkbox-layer2">
							<div v-for="(item, index) in dropdownWithFilterGameVendor(430000)" :key="index">
								<v-checkbox v-model="item.isChecked" :label="item.text">{{ item.text }} </v-checkbox>
							</div>
						</div> -->

						<!-- <div>
							<v-checkbox :label="'勾選AT'" @change="checkAllByGameVendor($event, 490000)">> </v-checkbox>
						</div>
						<div class="checkbox-layer2">
							<div v-for="(item, index) in dropdownWithFilterGameVendor(490000)" :key="index">
								<v-checkbox v-model="item.isChecked" :label="item.text">{{ item.text }} </v-checkbox>
							</div>
						</div> -->

						<div>
							<v-checkbox :label="'勾選Galaxy'" @change="checkAllByGameVendor($event, 530000)">> </v-checkbox>
						</div>

						<div class="checkbox-layer2">
							<div v-for="(item, index) in dropdownWithFilterGameVendor(530000)" :key="index">
								<v-checkbox v-model="item.isChecked" :label="item.text">{{ item.text }} </v-checkbox>
							</div>
						</div>

						<div>
							<v-checkbox :label="'勾選EFG'" @change="checkAllByGameVendor($event, 650000)">> </v-checkbox>
						</div>

						<div class="checkbox-layer2">
							<div v-for="(item, index) in dropdownWithFilterGameVendor(650000)" :key="index">
								<v-checkbox v-model="item.isChecked" :label="item.text">{{ item.text }} </v-checkbox>
							</div>
						</div>
					</div>
				</v-card-text>
			</v-card>
		</v-dialog>

		<!-- 三方帳務-各日期備註 (table) -->
		<v-dialog v-model="dialogVisible.isShowAdvanceInfo2" max-width="950px">
			<v-card>
				<v-card-title class="grey darken-2">
					三方帳務-各日期備註
					<span class="grey-text f14 ml-2"> {{ exportAllFinanceDateMemo.beginText }} -{{ exportAllFinanceDateMemo.endText }} </span>

					<div class="top-right-item" @click="dialogVisible.isShowAdvanceInfo2 = false">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>

				<v-card-text style="min-height: 550px">
					<v-data-table
						:items="exportAllFinanceDateMemo.items"
						:headers="dateMemoHeaders"
						hide-default-footer
						:items-per-page="perPageOfMemo"
						:footer-props="{ 'items-per-page-options': [10, 50, 100] }"
					>
						<template v-slot:item.memo="{ item }">
							<div style="width: 350px; white-space: break-spaces">
								{{ item.memo }}
							</div>
						</template>
					</v-data-table>
				</v-card-text>
			</v-card>
		</v-dialog>

		<!-- 重新觸發三方帳務獲取 (重撈三方) -->
		<v-dialog v-model="dialogVisible.isShowExtra2" max-width="750px" persistent scrollable>
			<v-card>
				<v-card-title class="grey darken-2">
					重新觸發三方帳務獲取 (Fetch Accounting)
					<span class="grey-text f14 ml-2"> {{ exportAllFinanceDateMemo.beginText }} -{{ exportAllFinanceDateMemo.endText }} </span>

					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>

				<v-card-text style="min-height: 350px" v-if="triggerModel.gameVendor > 0">
					<v-row>
						<v-col cols="12" md="12" sm="12">
							<v-text-field :value="replaceToNoneIOS8601(triggerModel.begin)" :label="'起始時間 (UTC)'" disabled> </v-text-field>
						</v-col>

						<v-col cols="12" md="12" sm="12" class="minus-mt-3">
							<v-text-field :value="replaceToNoneIOS8601(triggerModel.end)" :label="'結束時間 (UTC)'" disabled> </v-text-field>
						</v-col>

						<v-col cols="12" md="12" sm="12" class="minus-mt-3">
							<v-text-field v-model="triggerModel.gameVendor" :label="'遊戲商編號'" disabled> </v-text-field>
						</v-col>

						<v-col cols="12" md="12" sm="12" class="minus-mt-3">
							<v-autocomplete v-model="triggerModel.vendorConfigIDs" clearable :items="vendorConfigDropdownItems" :label="'指定VendorConfig(不填寫則重跑全部)'" multiple small-chips>
							</v-autocomplete>
						</v-col>

						<div class="text-r w95p">
							<span class="link-text" @click="checkAllSameGameVendor()"> 勾選所有同遊戲商 </span>
						</div>
					</v-row>
				</v-card-text>

				<v-card-actions>
					<v-spacer></v-spacer>

					<v-btn color="red darken-4" @click="saveAndTriggerFetchAccounting" class="btn">
						<v-icon left>mdi-check-circle</v-icon>
						{{ $t('Button.__submit') }} (重撈數據)
					</v-btn>
				</v-card-actions>
			</v-card>
		</v-dialog>

		<CustomizedColumnsDisplayPopupDialog ref="refCustomizedColumnsDisplayPopupDialog"> </CustomizedColumnsDisplayPopupDialog>
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
import * as Model from '@/models/interfaces/statistics/merchant-summary';
import PerGameApi from '@/api/game-management/per-game';
import oneToManyCustomizedSettingApi from '@/api/system-setting/oneToManyCustomizedSetting';
import * as OneToManyModel from '@/models/interfaces/system-setting/systemConfigSetting';
import * as ChartModel from '@/models/interfaces/statistics/chart';
import MerchantSummaryApi from '@/api/statistics/merchant-summary';
import ChartApi from '@/api/statistics/chart';
import DatePicker from 'vue2-datepicker';
import LineChartPopup from '@/components/statistics/LineChartPopup.vue';
import { ChartDataTypeEnum } from '@/models/status/statistics';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import * as Permission from '@/models/status/permission';
import CustomizedColumnsDisplayPopupDialog from '@/components/CustomizedColumnsDisplayPopupDialog.vue';
import axios, { AxiosResponse, AxiosRequestConfig } from 'axios';
import eazyFormApi from '@/api/commons/eazyFormApi';

const authModule = namespace('Auth');

// @ts-ignore
@Component({ mixins: [datetimeMixin, commonDisplayFiltersMixin, commonMethodMixin], components: { DatePicker, LineChartPopup, CustomizedColumnsDisplayPopupDialog } })
export default class MerchantSummaryFinanceCompare extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];

	isHideExportButton: boolean = false;
	isRenderDropdown: boolean = true;
	isLockExpand: boolean = false;

	dialogVisible: DropdownModel.DialogVisible = new DropdownModel.DialogVisible();
	panel: number[] = [0];

	batchExportPreviewInfo: any = {};
	batchExportVendorConfigIDS: number[] = [];
	renderComponent: boolean = true;

	searchFilter: Model.IQueryVendorConfigFinanceCompareRequest = {} as Model.IQueryVendorConfigFinanceCompareRequest;
	searchResult: Model.IVendorConfigFinanceCompareResponse = {} as Model.IVendorConfigFinanceCompareResponse;
	editDateMemo: OneToManyModel.IEditDateMemoRequest = {} as OneToManyModel.IEditDateMemoRequest;
	exportAllFinanceDateMemo: Model.IExportFinanceReportDateMemo = {} as Model.IExportFinanceReportDateMemo;

	datetimeRangePicker: DropdownModel.DatetimeRangePicker = new DropdownModel.DatetimeRangePicker();

	vendorConfigDropdownItems: DropdownModel.IDropdownItems[] = [];
	vendorConfigDropdownItemsForCheckboxList: any[] = [];

	triggerModel: any = {};

	perPageOfMemo: number = 50;

	listOfvisible: number[] = [];

	get dateMemoHeaders() {
		const array = [];
		array.push({ text: 'vendorConfigID', value: 'vendorConfigID', align: 'center', width: '8%' });
		array.push({ text: this.$t('TableHeader.__currency'), value: 'currency', width: '8%' });
		array.push({ text: '日期', value: 'date', align: 'center', width: '12%' });
		array.push({ text: '三方服務器名稱', value: 'vendorConfigName', align: 'end' });
		array.push({ text: this.$t('TableHeader.__memo'), value: 'memo', align: 'end' });
		return array;
	}

	get getMerchantsCount() {
		if (this.searchResult && this.searchResult.merchants) {
			return this.searchResult.merchants.length;
		}

		return 0;
	}

	get getDataExistMerchantsCount() {
		if (this.searchResult && this.searchResult.merchantsDataExist) {
			return this.searchResult.merchantsDataExist.length;
		}

		return 0;
	}

	get hasPermissionInvoiceExport() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.System_AccountingInvoice_ExportInvoice) >= 0;
	}

	get hasPermissionSuperAdmin() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSetting_SystemConfigSetting) >= 0;
	}

	switchExtraDivOpenOrClose(index: number) {
		// 稍微延遲下 (針對快照的配合項目)
		setTimeout(() => {
			if (!this.isLockExpand) {
				if (this.listOfvisible.includes(index)) {
					this.listOfvisible = [];
				} else {
					this.listOfvisible = [];
					this.listOfvisible.push(index);
				}
			}
		}, 50);
	}

	isDetailVisible(index: number) {
		return this.listOfvisible.includes(index);
	}

	dropdownWithFilterGameVendor(targetVendor: number) {
		const myFilter = this.vendorConfigDropdownItemsForCheckboxList.filter((x) => x.extraProperty.gameVendor === targetVendor);
		return myFilter;
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

	showBatchExportDialog() {
		this.dialogVisible.isShowExport = true;

		const timeZoneOffset = +this.userProfile.timeZone * 3600 * 1000;

		this.batchExportPreviewInfo = {
			begin: this.datetimeRangePicker.searchStartTimeRange[0] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[0] + timeZoneOffset) : null,
			end: this.datetimeRangePicker.searchStartTimeRange[1] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[1] + timeZoneOffset) : null,
		};

		this.batchExportPreviewInfo.begin = this.batchExportPreviewInfo.begin.toString().substring(0, 10);
		this.batchExportPreviewInfo.end = this.batchExportPreviewInfo.end.toString().substring(0, 10);

		this.batchExportVendorConfigIDS = [];
		this.vendorConfigDropdownItemsForCheckboxList.forEach((item) => {
			item.isChecked = false;
		});
	}

	showEachMerchantTotalSumDialogView() {
		const myDisplayInfo = this.searchResult.eachMerchantTotalSumCustomizedDisplayFormat;

		console.log('-----------');
		console.log(myDisplayInfo);

		const targetRef: any = this.$refs.refCustomizedColumnsDisplayPopupDialog;
		targetRef.setDisplayInfoAndShowDialog(myDisplayInfo);

		return '';
	}

	checkAllByGameVendor(event: any, gameVendor: number) {
		const afterBooleanValue = event;

		console.log(gameVendor);

		this.vendorConfigDropdownItemsForCheckboxList.forEach((item) => {
			if (item.extraProperty.gameVendor === gameVendor) {
				item.isChecked = afterBooleanValue;
			}
		});

		this.dialogVisible.isShowExport = false;
		this.dialogVisible.isShowExport = true;
	}

	async confirmExport(type: number = 0) {
		console.log(this.vendorConfigDropdownItemsForCheckboxList);

		const selectedVendorConfigs = this.vendorConfigDropdownItemsForCheckboxList.filter((x) => x.isChecked);

		if (selectedVendorConfigs.length <= 0) {
			alert('請勾選至少一個');
			return;
		}

		EventBus.$emit('show_mask', null);

		const excelData: any = [];

		let excelHeaders: any = [
			'遊戲商',
			'遊戲商服務器',
			'服務器識別代碼',
			'幣別',
			'日期',
			'校正注單數',
			'投注金額(校正後)',
			'玩家輸贏(校正後)',
			'商戶營利(校正後)',
			'投注金額(三方)',
			'玩家輸贏(三方)',
			'標籤',
			'人工備註',
		];

		if (type === 999) {
			excelHeaders = [
				'遊戲商',
				'遊戲商服務器',
				'服務器識別代碼',
				'幣別',
				'起始日期',
				'結束日期',
				'投注金額(校正後)',
				'玩家輸贏(校正後)',
				'商戶營利(校正後)',
				'玩家輸贏(三方)',
				'商戶營利(三方)',
				'營利相差值',
			];
		}

		for (const item of selectedVendorConfigs) {
			const myFilter = {} as Model.IQueryVendorConfigFinanceCompareRequest;
			myFilter.beginTime = this.datetimeRangePicker.searchStartTimeRange[0] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[0]) : null;
			myFilter.endTime = this.datetimeRangePicker.searchStartTimeRange[1] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[1]) : null;
			myFilter.vendorConfigID = item.valueInt;
			myFilter.isDisableFakeData = true; // true false
			myFilter.isDescendingOrder = true;

			const result = await MerchantSummaryApi.getVendorConfigFinanceCompareStatReport(myFilter);

			console.log('result');
			console.log(item);
			console.log(result);

			if (type === 0) {
				result.data.items.forEach((value) => {
					const eachRow: any = [];
					eachRow.push(item.extraProperty.gameVendorText);
					eachRow.push(item.text);
					eachRow.push(result.data.assignVendorID);
					eachRow.push(result.data.currency);
					eachRow.push(value.statDate.substring(0, 10));
					eachRow.push(value.totalAdjustRecordCount);
					eachRow.push(value.pgsAfterAdjustment.totalBetAmount);
					eachRow.push(value.pgsAfterAdjustment.totalWinLossAmount);
					eachRow.push(value.pgsAfterAdjustment.totalWinLossAmount * -1);
					eachRow.push(value.fetchThirdPartyStatisticReport.totalBetAmount);
					eachRow.push(value.fetchThirdPartyStatisticReport.totalWinLossAmount);
					const isHintGapDiff =
						!value.isFetchDataAllZero && (!value.isCompareCorrect_ByGameVendorSettleDate || !value.isCompareCorrect_AfterAdjustment) && !value.isRoundNotEqualOnly_ByGameVendorSettleDate;
					eachRow.push(isHintGapDiff ? '與三方數據有誤差' : '');
					eachRow.push(value.extraMemoInfo);
					excelData.push(eachRow);
				});
			} else {
				const totalSummary = result.data.totalSummaryAfterAdjustment;
				const totalSummaryThird = result.data.totalSummaryThirdParty;

				if (totalSummary != null) {
					const eachRow: any = [];
					eachRow.push(item.extraProperty.gameVendorText);
					eachRow.push(result.data.vendorConfigName);
					eachRow.push(result.data.assignVendorID);
					eachRow.push(result.data.currency);
					eachRow.push(this.batchExportPreviewInfo.begin);
					eachRow.push(this.batchExportPreviewInfo.end);
					eachRow.push(totalSummary.totalBetAmount);
					eachRow.push(totalSummary.totalWinLossAmount);
					eachRow.push(totalSummary.totalWinLossAmount * -1);

					if (totalSummaryThird != null) {
						eachRow.push(totalSummaryThird.totalWinLossAmount);
						eachRow.push(totalSummaryThird.totalWinLossAmount * -1);
					}

					excelData.push(totalSummaryThird.totalWinLossAmount - totalSummary.totalWinLossAmount);
					excelData.push(eachRow);
				}
			}
		}

		EventBus.$emit('hide_mask', null);

		console.log(this.batchExportPreviewInfo.begin);

		const dateText: string = this.batchExportPreviewInfo.begin.substring(5, 10).replace('-', '') + '_' + this.batchExportPreviewInfo.end.substring(5, 10).replace('-', '');
		const config = {
			headers: excelHeaders,
			rowData: excelData,
			fileName: '' + dateText + '_PGSxThird_FinanceCompare.xlsx',
		};

		// @ts-ignore
		this.downloadExcel(config);
	}

	exportAllMemo() {
		// 複製全備註
		const myFilter = {} as Model.IQueryVendorConfigFinanceCompareRequest;
		myFilter.beginTime = this.datetimeRangePicker.searchStartTimeRange[0] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[0]) : null;
		myFilter.endTime = this.datetimeRangePicker.searchStartTimeRange[1] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[1]) : null;

		EventBus.$emit('show_mask', null);
		const result = MerchantSummaryApi.getFinanceCompareDateMemo(myFilter)
			.then((res) => {
				this.exportAllFinanceDateMemo = res.data;
				this.dialogVisible.isShowAdvanceInfo2 = true;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	exportRecordsOfTargetDate(item: Model.IVendorConfigFinanceCompare) {
		if (item.pgsStatDB.totalRound >= 2000000) {
			alert('該日期注單筆數過多、不支援匯出功能，您只能透過本地端local操作、並直接下載產生檔案(不透過API回應)');
			return;
		}

		const myFilter = {} as DropdownModel.ILazyCommonParameterReqeust;

		// 不想要展開效果
		this.isLockExpand = true;

		myFilter.vendorConfigID = this.searchFilter.vendorConfigID;
		myFilter.beginDateTW = item.statDateText;

		EventBus.$emit('show_mask', null);
		PerGameApi.exportLargeCsvByVendorConfigID(myFilter, this.searchResult.exportFileNamePrefix)
			.then((res) => {})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
				this.delayReleaseLock();
			});
	}

	toAbs(myValue: number) {
		return Math.abs(myValue);
	}

	async ExportEachDayThirdParty_vs_PGS() {
		console.log(this.vendorConfigDropdownItemsForCheckboxList);

		const vm: any = this;
		let myCopyMessage = '';
		myCopyMessage += '月份：' + this.searchResult.items[0].statDateText + '\n';
		myCopyMessage += '服務器：' + this.searchResult.assignVendorID + '\n';
		myCopyMessage += '我方輸贏：' + this.toAbs(this.searchResult.totalSummaryAfterAdjustment.totalWinLossAmount) + '\n';
		myCopyMessage += '貴方輸贏：' + 'XXXXXXXXXXXXXXX' + ' (貴方月結帳單的數字) \n' + '\n';
		myCopyMessage += '您好，貴方輸贏數字與我方有落差 (帳單)' + '\n';
		myCopyMessage += '且我方數字有與貴方的統計數據API做過比對，照理說誤差沒這麼多' + '\n';
		myCopyMessage += '請參考上方Excel (裡面是每一日貴方API的數字 & 我方的數字)' + '\n';
		myCopyMessage += '需請貴司幫忙確認，貴方提供的數字不同的原因為何，謝謝您' + '\n';

		vm.copyTextToClipboard2(myCopyMessage);

		const excelData: any = [];

		const excelHeaders: any = [
			{ text: '服務器代號', width: 23 },
			{ text: '日期', width: 18 },
			{ text: '玩家輸贏(遊戲原廠 by API)', width: 30 },
			{ text: '玩家輸贏(我方)', width: 30 },
			{ text: '差額', width: 18 },
		];

		let loopIndex = 1;

		for (const item of this.searchResult.items) {
			const eachRow: any = [];

			loopIndex = loopIndex + 1;

			eachRow.push(this.searchResult.vendorConfigName);
			eachRow.push(item.statDateText);
			eachRow.push(item.fetchThirdPartyStatisticReport.totalWinLossAmount);
			eachRow.push(item.pgsAfterAdjustment.totalWinLossAmount);
			eachRow.push(item.diff_AggregateByThirdSettleDate_vs_FetchThirdPartyStatisticReport.totalWinLossAmount);

			excelData.push(eachRow);
		}

		const totalRow: any = [];

		totalRow.push('加總');
		totalRow.push('');
		totalRow.push(this.searchResult.totalGameVendorSideWinloss);
		totalRow.push(this.searchResult.totalSummaryAfterAdjustment.totalWinLossAmount);
		totalRow.push(this.searchResult.totalDiff);

		excelData.push(totalRow);

		const config = {
			headersWithConfig: excelHeaders,
			rowData: excelData,
			fileName: 'PGSxThird_EachDayCompare_' + this.searchResult.currency + '_' + this.searchResult.vendorConfigID.toString() + '.xlsx',
		};

		// @ts-ignore
		this.downloadExcel(config);
	}

	whenSelectChanged() {
		if (this.searchFilter.vendorConfigID && this.searchFilter.vendorConfigID != null && this.searchFilter.vendorConfigID > 0) {
			this.search();
		}
	}

	delayReleaseLock() {
		setTimeout(() => {
			this.isLockExpand = false;
		}, 200);
	}

	showTriggerFetchThirdAccounting(item: Model.IVendorConfigFinanceCompare) {
		this.isLockExpand = true;
		this.delayReleaseLock();

		this.triggerModel = {};
		this.triggerModel.begin = item.beginUTC;
		this.triggerModel.end = item.endUTC;
		this.triggerModel.gameVendor = this.searchResult.gameVendorID;
		this.triggerModel.vendorConfigIDs = [this.searchResult.vendorConfigID];
		this.triggerModel.isDeleteOriginalData = true;

		this.dialogVisible.isShowExtra2 = true;
	}

	saveAndTriggerFetchAccounting() {
		EventBus.$emit('show_mask', null);
		MerchantSummaryApi.triggerFetchThirdAccounting(this.triggerModel)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.search();

				setTimeout(() => {
					this.closeAllModal();
				}, 500);
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	search() {
		this.searchFilter.beginTime = this.datetimeRangePicker.searchStartTimeRange[0] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[0]) : null;
		this.searchFilter.endTime = this.datetimeRangePicker.searchStartTimeRange[1] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[1]) : null;
		this.searchFilter.isDisableFakeData = true; // true false

		if (this.userProfile.timeZone !== 8) {
			alert('此頁查詢功能，只允許使用+8時區，請透過右上方切換');
			return;
		}

		EventBus.$emit('show_mask', null);

		MerchantSummaryApi.getVendorConfigFinanceCompareStatReport(this.searchFilter)
			.then((res) => {
				this.isHideExportButton = res.data.isHideExportButton;
				this.searchResult = res.data;
				this.listOfvisible = [];
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	zeroThenGreyClass(numberValue: number) {
		return numberValue === 0 ? 'grey-text-more' : '';
	}

	copyDebugSQL() {
		const vm: any = this;
		vm.copyTextToClipboard2(this.searchResult.debugSQL);
	}

	checkKeyboardEventAndSelectDropdown(keyEvent: any) {
		let offset = 0;

		if (keyEvent.key === 'ArrowLeft') {
			offset = -1;
		} else if (keyEvent.key === 'ArrowRight') {
			offset = 1;
		}
		if (offset !== 0) {
			if (this.searchFilter.vendorConfigID <= 0) {
				if (offset === 1) {
					this.searchFilter.vendorConfigID = Number(this.vendorConfigDropdownItems[0].valueInt);
					this.isRenderDropdown = false;
					this.isRenderDropdown = true;
					this.search();
				}
			} else {
				const dropdownOfArray = this.getNumberArrayOfDropdown();
				const maxValue = dropdownOfArray.length - 1;
				const currentSelectIndex = dropdownOfArray.indexOf(this.searchFilter.vendorConfigID);
				let targetToSelect = currentSelectIndex + offset;

				if (targetToSelect <= 0) {
					targetToSelect = 0;
				}
				if (targetToSelect >= maxValue) {
					targetToSelect = maxValue;
				}

				if (currentSelectIndex !== targetToSelect) {
					this.searchFilter.vendorConfigID = dropdownOfArray[targetToSelect];
					this.isRenderDropdown = false;
					this.isRenderDropdown = true;
					this.search();
				}
			}
		}
	}

	getNumberArrayOfDropdown() {
		const myArray: number[] = [];
		this.vendorConfigDropdownItems.forEach((x) => myArray.push(Number(x.value)));
		return myArray;
	}

	clear() {}

	mounted() {
		const vm = this;
		document.addEventListener('keydown', (event) => {
			// 避免切換到其他tab頁面的時候。也會吃到keydown偵測
			const focusElement = document.getElementById('MerchantSummaryFinanceComparePage');
			if (focusElement && focusElement != null && !vm.dialogVisible.isShowEdit) {
				vm.checkKeyboardEventAndSelectDropdown(event);
			}
		});

		this.registerReloadComponent();
		this.init();
	}

	getElementID(dateText: string) {
		return 'table_date_' + dateText;
	}

	getDetailElementID(dateText: string) {
		return 'table_detail_date_' + dateText;
	}

	showGameReportPopup(item: Model.IVendorConfigFinanceCompare) {
		this.isLockExpand = true;
		this.delayReleaseLock();

		const myFilter: any = {
			querySqlID: 900092,
			vendorConfigID: this.searchFilter.vendorConfigID,
			beginTime: this.replaceToNoneIOS8601(item.beginUTC),
			endTime: this.replaceToNoneIOS8601(item.endUTC),
		};
		const myConfig: any = {
			maxWidth: 1100,
		};

		EventBus.$emit(`globalShowCustomizedColumnsDisplayPopupDialog`, myFilter, myConfig);
		return;
	}

	showEachDateEventSummaryReportPopup(item: Model.IVendorConfigFinanceCompare) {
		this.isLockExpand = true;

		const myFilter: any = {
			querySqlID: 900018,
			vendorConfigID: this.searchFilter.vendorConfigID,
			beginTime: this.searchFilter.beginTime,
			endTime: this.searchFilter.endTime,
		};
		const myConfig: any = {
			maxWidth: 1100,
		};

		EventBus.$emit(`globalShowCustomizedColumnsDisplayPopupDialog`, myFilter, myConfig);
		return;
	}

	replaceToNoneIOS8601(iso8601: string) {
		return iso8601.replace('T', ' ').replace('Z', '');
	}

	showThirdPartyGameReportPopup(item: Model.IVendorConfigFinanceCompare) {
		this.isLockExpand = true;
		this.delayReleaseLock();

		const myFilter: any = {
			querySqlID: 900095,
			vendorConfigID: this.searchFilter.vendorConfigID,
			beginTime: this.replaceToNoneIOS8601(item.beginUTC),
			endTime: this.replaceToNoneIOS8601(item.endUTC),
		};
		const myConfig: any = {
			maxWidth: 1250,
		};

		EventBus.$emit(`globalShowCustomizedColumnsDisplayPopupDialog`, myFilter, myConfig);
		return;
	}

	showMerchantReportPopup(item: Model.IVendorConfigFinanceCompare) {
		this.isLockExpand = true;
		this.delayReleaseLock();

		const myFilter: any = {
			querySqlID: 900091,
			vendorConfigID: this.searchFilter.vendorConfigID,
			beginTime: this.replaceToNoneIOS8601(item.beginUTC),
			endTime: this.replaceToNoneIOS8601(item.endUTC),
		};
		const myConfig: any = {
			maxWidth: 1100,
		};

		EventBus.$emit(`globalShowCustomizedColumnsDisplayPopupDialog`, myFilter, myConfig);
		return;
	}

	showTargetCurrencyMerchantReportPopup(myCurrency: string) {
		this.isLockExpand = true;
		this.delayReleaseLock();

		const myBeginTime = this.datetimeRangePicker.searchStartTimeRange[0] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[0]) : null;
		const myEndTime = this.datetimeRangePicker.searchStartTimeRange[1] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[1]) : null;

		const myFilter: any = {
			querySqlID: 900091,
			currency: myCurrency,
			vendorConfigID: this.searchFilter.vendorConfigID,
			beginTime: myBeginTime,
			endTime: myEndTime,
		};
		const myConfig: any = {
			maxWidth: 1100,
		};

		EventBus.$emit(`globalShowCustomizedColumnsDisplayPopupDialog`, myFilter, myConfig);
		return;
	}

	showEditMemoPopup(item: Model.IVendorConfigFinanceCompare) {
		this.dialogVisible.isShowEdit = true;
		this.isLockExpand = true;
		this.delayReleaseLock();
		this.editDateMemo.dateText = item.statDateText;
		this.editDateMemo.vendorConfigID = this.searchFilter.vendorConfigID;
		this.editDateMemo.memo = item.extraMemoInfo;

		setTimeout(() => {
			const refInput: any = this.$refs.refEditDateMemo;
			refInput.focus();
		}, 250);

		return;
	}

	copyCompareText(item: any) {
		let extraInfo = '';
		const isAGGLive = this.searchResult.gameVendorID === 330000;

		// AGGLive 數字老是有問題
		if (isAGGLive) {
			extraInfo = ' (貴方數字是透過 operator/DailyReport API獲取) ';
		}

		let msg = '';
		msg += '識別代碼' + this.searchResult.assignVendorID + ' (API_ID)' + '\n';
		msg += '日期：' + item.statDateText + '\n';
		msg += '貨幣：' + this.searchResult.currency + '\n';
		msg += '' + extraInfo + '\n';
		msg += '總局數(我方)：' + item.pgsAfterAdjustment.totalRound + '\n';
		msg += '總局數(貴方)：' + item.fetchThirdPartyStatisticReport.totalRound + '\n';
		msg += '總輸贏(我方)：' + item.pgsAfterAdjustment.totalWinLossAmount + '\n';
		msg += '總輸贏(貴方)：' + item.fetchThirdPartyStatisticReport.totalWinLossAmount + '\n';
		msg += '輸贏誤差值：' + Math.abs(item.pgsAfterAdjustment.totalWinLossAmount - item.fetchThirdPartyStatisticReport.totalWinLossAmount).toFixed(2) + '\n';
		msg += '----------------------' + '\n';
		msg += '您好，我方與貴方的統計數據有誤差' + '\n';
		msg += '請參考上方注單清單(.csv檔案)。' + '\n';

		if (isAGGLive) {
			msg += '麻煩協助確認，是否有 重複帳/卡排程需補單/其他因素 等問題。謝謝您' + '\n';
			//
		} else {
			msg += '需麻煩貴司幫忙確認下，誤差是哪邊資料不一致所導致。謝謝您' + '\n';
		}

		const vm: any = this;
		vm.copyTextToClipboard2(msg);
	}

	saveEditDateMemo() {
		EventBus.$emit('show_mask', null);
		oneToManyCustomizedSettingApi
			.updateDateMemo(this.editDateMemo)
			.then((x) => {
				EventBus.$emit(`successfully-modified`);
				this.search();
				this.closeAllModal();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	showMerchants() {
		this.dialogVisible.isShowTreeInfo = true;
	}

	showDataExistMerchants() {
		this.dialogVisible.isShowTreeInfo2 = true;
	}

	closeAllModal() {
		this.dialogVisible.resetAllValueToFalse();
	}
	async sleep(ms = 0) {
		return new Promise((r) => setTimeout(r, ms));
	}

	checkAllSameGameVendor() {
		if (this.triggerModel.vendorConfigIDs == null || this.triggerModel.vendorConfigIDs.length <= 0) {
			return;
		}

		const firstOne = this.triggerModel.vendorConfigIDs[0];
		const matchTarger = this.vendorConfigDropdownItems.filter((x) => x.valueInt === firstOne);
		const targetGameVendor: number = matchTarger[0].extraProperty.gameVendor;

		this.triggerModel.vendorConfigIDs = [];

		this.dropdownWithFilterGameVendor(targetGameVendor).forEach((x) => {
			this.triggerModel.vendorConfigIDs.push(x.valueInt);
		});

		this.dialogVisible.isShowExtra2 = false;
		this.dialogVisible.isShowExtra2 = true;
	}

	copyTextOrSkip(myText: string) {
		if (myText == null || myText.length <= 0) {
			return;
		}

		const vm: any = this;
		vm.copyTextToClipboard2(myText);
	}

	init() {
		this.datetimeRangePicker.searchStartTimeRange = [daysAgo(0), daysAgo(1)];

		setTimeout(() => {
			const vm: any = this;
			vm.globalQuicklyPickTimeRange('thisMonth');
		}, 500);

		DropdownApi.getVendorConfigSupportAccountingFetch(false).then((res) => {
			this.vendorConfigDropdownItems = res;
			this.vendorConfigDropdownItemsForCheckboxList = res;
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

.warningMsg {
	padding: 5px 10px 5px 10px;
	background: #672020;
	color: #eb8686;
	border-radius: 5px;
	max-width: 800px;
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

.noHover:hover,
.noHover tr:hover {
	background: initial !important;
}

.innerTable th {
	font-weight: 100;
	color: #aaa !important;
}

.grey-text-more {
	color: #777;
}

.pointer td {
	border-bottom: solid 1px #333;
}

.v-data-table__wrapper table {
	width: 100%;
}

.v-data-table__wrapper table td {
	padding-bottom: 4px;
	padding-top: 4px;
}

.tdText2 {
	font-size: 17px !important;
}

.checkbox-layer2 {
	padding-left: 25px;
}

.checkbox-layer2 {
	.v-input--checkbox {
		margin-top: -10px;
	}
}

.card-totalWinLoss {
	display: inline-block;
	min-width: 250px;
	background: #393939;
	padding: 0px 5px 5px 5px;
	border-radius: 5px;
	margin-left: 20px;
}
</style>
