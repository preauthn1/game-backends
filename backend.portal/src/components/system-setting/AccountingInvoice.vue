<template>
	<v-app v-if="renderComponent" class="app">
		<div>
			<v-btn color="red darken-2" class="btn search" v-if="hasExportInvoicePermission" @click.stop="showInvoiceListDialog">
				<v-icon left>search</v-icon>
				查看帳單
			</v-btn>

			<v-btn color="blue lighten-1" class="btn search ml-2" v-if="hasExportInvoicePermission" @click.stop="refreshRateByAPI()">
				<v-icon left>refresh</v-icon>
				透過三方工具自動更新匯率
			</v-btn>

			<div class="mt-4">
				<v-row>
					<v-col cols="12" md="8" sm="12">
						<h3 style="position: relative">
							<span> 貨幣匯率 (Currency exchange rate) </span>

							<v-select
								label="轉換成哪一個貨幣匯率"
								:items="editCurrenyDropdown"
								small-chips
								@change="whenChangeTargetCurrency()"
								v-model="selectEditTargetCurrency"
								style="width: 300px; display: inline-block; position: absolute; right: 20px; top: -15px"
							>
							</v-select>
						</h3>

						<div class="v-data-table theme--dark mt-2">
							<div class="v-data-table__wrapper">
								<table class="myTable002">
									<thead class="v-data-table-header">
										<tr>
											<th>最後更新時間</th>
											<th>幣別</th>
											<th class="text-r">匯率({{ selectEditTargetCurrency }})</th>
											<th>備註</th>
											<th>操作</th>
										</tr>
									</thead>

									<tbody>
										<tr v-for="(item, index) in allCurrencyList.items" :key="index">
											<th>
												<v-icon small color="grey" class="mr-1" @click="showCurrencyRateUpdateLogs(item.currency)"> history </v-icon>
												<div :class="item.isDiscardManyDays ? 'grey-layout' : ''" style="display: inline-block">
													<span class="f14">{{ item.lastUpdateTime | timeWithAddTimezone(userProfile.timeZone) }}</span>
													<span class="timeDiff ml-1">({{ item.lastUpdateTimeDiffText }}) </span>
												</div>
											</th>
											<th>
												<div :class="item.isDiscardManyDays ? 'grey-layout' : ''" style="position: relative">
													<v-icon v-if="item.isHot" x-small color="yellow darken-2" style="font-size: 12px; position: absolute; left: -12px; top: -2px"> mdi-star </v-icon>

													<span :title="item.isHot ? '主力貨幣' : ''">
														{{ item.currency }}
													</span>
												</div>
											</th>
											<th class="text-r">
												<div>
													<input
														type="text"
														class="myInputText"
														style="height: 40px"
														:disabled="item.isDisableEdit"
														@keydown="whenChangeExchangeRateValue()"
														v-model="item.exchangeRate_EditNewValue"
													/>
												</div>
											</th>

											<th>
												<div style="min-width: 100px">
													<span class="minus-red f16" v-if="isRenderWarningText" style="width: 80px; display: inline-block">
														{{ currencyChangeWarningText(item) }}
													</span>
												</div>
											</th>

											<th style="position: relative">
												<v-btn color="green lighten-3" text class="btn search" v-if="!item.isDisableEdit" @click.stop="saveCurrencyRate(item)">
													<v-icon left>check</v-icon>
													{{ $t('Button.__save') }} </v-btn
												>&#9;

												<v-btn color="green lighten-3" text class="btn search" disabled="disabled" v-else>
													<v-icon left>close</v-icon>
													禁止編輯 </v-btn
												>&#9;

												<v-chip class="ml-2" style="font-family: monospace; font-size: 16px"> {{ item.currency }}｜{{ item.currencyText }} </v-chip>
											</th>
										</tr>
									</tbody>
								</table>
							</div>
						</div>
					</v-col>

					<v-col cols="12" md="4" sm="12">
						<div class="accountingMemo">
							<H1 style="color: #c8ad7c">注意事項</H1>

							<v-divider></v-divider>

							<h3 class="mt-2">特殊匯率轉換</h3>
							<div><span class="myChip">KIDR</span>= IDR * 1,000</div>
							<div><span class="myChip">KVND</span>= VND * 1,000</div>
							<div><span class="myChip">KCOP</span>= COP * 1,000</div>
							<div><span class="myChip">MYR100</span>= MYR / 100</div>

							<h3 class="mt-6">重要商戶對帳</h3>
							<div style="color: #aaa">建議盤費300美元以上的客戶。稍微對一下數字</div>

							<h3 class="mt-6">輔助功能(其他)</h3>
							<div style="color: #aaa">
								<div>若要跟其他三方提供的帳單做帳務比對(Funky、Jili、AGG?)</div>
								<div>可搭配《統計資料》>《三方數據比對報表》</div>
								<div>⚠️ 請記得時區切換至+8</div>
							</div>

							<h3 class="mt-6">SJ (W88捕魚 x Sean)</h3>
							<div style="color: #aaa">
								<div>請先截圖XE匯率、再開始編輯輸入匯率</div>
							</div>

							<h3 class="mt-6">Vworld帳單 & AGG帳單差異處</h3>
							<div style="color: #aaa">
								<div>AGG帳單所有商戶都以GMT+8計算</div>
								<div>WTT、CJ7的項目會移除掉(兩邊MYR加總可能不一致)</div>
								<div>WTT的紅包項目會被移除(*待確認)</div>
								<div>Vworld帳單與AGG帳單，AGG帳單中的紅包只會篩選GameID=290000系列 (不含PGS自製遊戲)</div>
							</div>

							<h3 class="mt-6">特殊名詞說明</h3>
							<div style="color: #aaa">
								<div>0.04% = 0.008 * 0.05 = 0.0004 (視訊千分之八的返水)</div>
							</div>

							<h3 class="mt-6">GameID(遊戲編號說明)</h3>
							<div style="color: #aaa">
								<div>1~9999：舊版PGS遊戲、Avatar獨家遊戲</div>
								<div>290001~290999：老虎機遊戲</div>
								<div>330001~330999：真人視訊遊戲</div>
								<div>530001~530999：Galaxy Live視訊遊戲</div>
							</div>

							<h3 class="mt-6">常用SQL (技術使用)</h3>
							<div style="">
								<v-btn text outlined color="yellow" @click="copySQL_1()">
									<v-icon left>file_copy</v-icon>
									SQL_指派折扣項目至組別
								</v-btn>
							</div>

							<h3 class="mt-6">
								盤費/折扣調整項目

								<v-btn
									color="blue darken-4"
									small
									style="position: absolute; right: 5px"
									@click="copyTextToClipboard(queryAdjustmentRulesAndAssignGroup.copyText_AllParticipantMerchant)"
								>
									<v-icon small>file_copy</v-icon>
									複製參加商戶(未過期)
								</v-btn>
							</h3>
							<div style="color: #aaa" v-if="queryAdjustmentRulesAndAssignGroup != null && queryAdjustmentRulesAndAssignGroup.items">
								<!-- 右下角-折扣項目 (AdjustmentRules) -->
								<!-- 右下角-折扣項目 (AdjustmentRules) -->
								<!-- 右下角-折扣項目 (AdjustmentRules) -->
								<v-expansion-panels accordion multiple class="mt-2">
									<v-expansion-panel v-for="(item, index) in queryAdjustmentRulesAndAssignGroup.items" :key="index">
										<v-expansion-panel-header style="padding: 10px">
											<div :class="item.isExpired ? 'expired-grey-text' : ''" :title="item.ruleID">
												<span v-if="item.isExpired">(*已過期)</span>

												<span v-if="item.isHintEmptyAssign" class="yellow-text">⚠️*assign</span>

												{{ item.nameCN }}
												<span class="myChip2 ml-2 text-r" :class="item.adjustValuePercentage < 0 ? 'minus-red' : ''" style="max-width: 60px; float: right"
													>{{ item.adjustValuePercentage }}%
												</span>
											</div>
										</v-expansion-panel-header>

										<v-expansion-panel-content>
											<div>
												<v-divider></v-divider>

												<div class="mt-2"></div>

												<div class="mb-2">
													<span class="f12 light-yellow-text">ruleID：</span>
													<span class="f14">{{ item.ruleID }}</span>
												</div>

												<div class="mb-2" v-if="!isEmptyString(item.beginTimeTwText) || !isEmptyString(item.endTimeTwText)">
													<span class="f12 light-yellow-text">限定時間：</span>
													<span class="f14">{{ item.beginTimeTwText }} - {{ item.endTimeTwText }}</span>
												</div>

												<div class="mb-2" v-if="!isEmptyString(item.gameIDs)">
													<span class="f12 light-yellow-text">限定遊戲：</span>
													<span class="f14">{{ item.gameIDs }}</span>
												</div>

												<div class="mb-2 pointer" v-if="item.numberOfMerchantID > 3">
													<span class="f12 light-yellow-text">限定商戶：</span>
													<span class="f14">共 {{ item.numberOfMerchantID }} 個</span>
												</div>

												<div class="mb-2 pointer" v-if="!isEmptyString(item.merchantIDs)" @click="copyTextToClipboard(item.merchantIDs)">
													<span class="f12 light-yellow-text">限定商戶：</span>
													<span class="f14">{{ item.merchantsDisplayInfo }}</span>
												</div>

												<div class="mb-2">
													<span class="f12 light-yellow-text">已分配帳務組別:</span>
													<v-chip v-for="(text, index) in item.assignGroups" small class="ml-1" :key="index">
														{{ text }}
													</v-chip>
												</div>

												<div class="text-c mt-4" v-if="hasSuperAdminPermission">
													<v-btn color="green darken-2" small @click="copyAndCreateNewRule(item)">(技術用)複製建立相同項目</v-btn>
												</div>
											</div>
										</v-expansion-panel-content>
									</v-expansion-panel>
								</v-expansion-panels>
							</div>
						</div>
					</v-col>
				</v-row>
			</div>
		</div>

		<v-dialog v-model="dialogVisible.isShowExport" max-width="1400px">
			<v-card :class="dialogVisible.isShowDetailInfo ? 'v-dialog-opacity' : ''">
				<v-card-title class="grey darken-2">
					帳單商戶列表

					<div style="position: absolute; right: 50px">
						<v-checkbox class="" v-model="searchFilter.isShowSkipItemForDebug" :label="'查詢額外項目(技術debug用)'"> </v-checkbox>
					</div>

					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>

				<v-card-text style="min-height: 700px">
					<v-row>
						<v-col cols="12" sm="4" md="4">
							<v-autocomplete :label="'選擇查看月份'" :items="dropdownItemsLatestMonth" chips v-model="searchFilter.targetMonth" style="width: 300px">
								<template v-slot:item="data">
									<div>
										<span>{{ data.item.text }}</span>
										<span class="f11 ml-1 grey-text"> ({{ data.item.extraProperty.accountingBeginEnd }}) </span>
									</div>
								</template>
							</v-autocomplete>
						</v-col>

						<v-col cols="12" sm="8" md="8">
							<div class="mt-4">
								<v-btn color="yellow darken-1" text class="btn mr-1" @click.stop="getInvoiceList()">
									<v-icon left>search</v-icon>
									搜尋
								</v-btn>

								<v-btn color="blue lighten-1" text class="btn mr-1" @click.stop="exportInvoiceForCheckList()">
									<v-icon left>cloud_download</v-icon>
									匯出勾選帳單
								</v-btn>

								<a target="_blank" class="ml-4 link-text-grey" href="https://example.invalid/spreadsheets/d/1ZwF975XDP3233hyZf4KC8ZJRMwiWHjEvyh3rg-sT6EQ/edit#gid=1903559005"
									>帳單總表(google)
								</a>

								<!-- <v-btn color="pink lighten-3" text class="btn mr-1" style="float:right"
									@click.stop="generateTotalExcelRows()">
									<v-icon left>stars</v-icon>
									總表(技術用)
								</v-btn> -->

								<v-btn color="grey lighten-1" text class="btn mr-1" style="float: right" @click.stop="showExtraFilterRulesDialog()">
									<v-icon left>book</v-icon>
									【手風琴】折扣明細條目(活動)
								</v-btn>

								<!-- <v-btn color="purple lighten-2" text class="btn mr-1" style="float: right" @click.stop="generateAggGameVendorInvoiceDetailItems()">
									<v-icon left>stars</v-icon>
									AGG帳單細項
								</v-btn> -->

								<v-btn color="red lighten-1" text class="btn mr-1" style="float: right" @click.stop="generateVworldInvoiceDetailItems()">
									<v-icon left>stars</v-icon>
									匯出特殊帳單
								</v-btn>
							</div>
						</v-col>
					</v-row>

					<div style="clear: both"></div>

					<v-row v-if="excelRawTextForCopy != null && excelRawTextForCopy.length > 0">
						<v-col cols="12" md="12" sm="12">
							<div>
								<div class="text-c">
									<v-btn color="orange lighten-1" text class="btn mr-1" large v-clipboard:copy="excelRawTextForCopy" v-clipboard:success="copySuccess">
										<v-icon left>file_copy</v-icon>
										複製文字
									</v-btn>

									<span class="orange-text ml-2">請複製文字 並貼到Excel中</span>
								</div>

								<div style="padding: 1% 5% 1% 5%">
									<div style="height: 400px; overflow: auto">
										{{ excelRawTextForCopy }}
									</div>
								</div>
							</div>
						</v-col>
					</v-row>

					<div v-if="isSearchAndShowTable">
						<a @click="setAllMerchantsCheckboxValue(true)" class="mr-2 link-text">勾選全部</a>
						<a @click="setAllMerchantsCheckboxValue(false)" class="mr-2 link-text">取消勾選</a>

						<v-data-table :items="searchAccountingListResult.items" hide-default-footer fixed-header height="625px" :footer-props="{ 'items-per-page-options': [100] }" :headers="headers">
							<template v-slot:item.action="{ item }">
								<v-checkbox v-model="item.f2e_isCheckbox" style="margin-top: 0px" hide-details=""> </v-checkbox>
							</template>

							<template v-slot:item.invoiceGroupCategoryName="{ item }">
								<span :class="'myCategoryColor_' + item.invoiceGroupCategoryColorType.toString()" :title="item.accountingGroupID">
									{{ item.invoiceGroupCategoryName }}
								</span>
							</template>

							<template v-slot:item.merchantID="{ item }">
								<v-tooltip top color="blue darken-4">
									<template v-slot:activator="{ on, attrs }">
										<v-icon small color="grey lighten-1" v-bind="attrs" v-on="on">info</v-icon>
									</template>
									<span>
										<div style="min-width: 300px">
											<ul>
												<li>組別ID： {{ item.accountingGroupID }}</li>
												<li>時區： +{{ item.timezone }}</li>
												<li>頻道： {{ item.imChannelName }}</li>
											</ul>
										</div>
									</span>
								</v-tooltip>

								<span>
									{{ item.merchantID }}
								</span>
							</template>

							<template v-slot:item.merchantName="{ item }">
								<div class="maxWidthBeforeClick maxWidth150" @click="expandMaxWidth($event)">
									{{ item.merchantName }}
								</div>
							</template>

							<template v-slot:item.feeRateText="{ item }">
								<div class="f16">
									{{ item.feeRateText }}
								</div>
							</template>

							<template v-slot:item.exchangeRateToUSDT="{ item }">
								<div>
									{{ item.currency }}
								</div>

								<div class="sub-title-text">
									<span v-if="item.isComebineOneFormat"> - </span>
									<span v-else>
										{{ item.exchangeRateToUSDT }}
									</span>
								</div>
							</template>

							<template v-slot:item.totalWinLoss="{ item }">
								<span v-if="item.isComebineOneFormat"> - </span>
								<span v-else :class="item.totalWinLoss < 0 ? 'minus-red' : ''">
									{{ item.totalWinLoss | amountDisplay }}
								</span>
							</template>

							<template v-slot:item.chargeFeeAmount="{ item }">
								<span v-if="item.isComebineOneFormat"> - </span>
								<span v-else :class="item.chargeFeeAmount < 0 ? 'minus-red' : ''">
									{{ item.chargeFeeAmount | amountDisplay }}
								</span>
							</template>

							<template v-slot:item.chargeFeeAmountAftetExchange="{ item }">
								<span :class="item.chargeFeeAmountAftetExchange < 0 ? 'minus-red' : ''">
									{{ item.chargeFeeAmountAftetExchange | amountDisplay }}
								</span>
							</template>
						</v-data-table>
					</div>
				</v-card-text>
			</v-card>
		</v-dialog>

		<!-- 匯出【特殊帳單】額外勾選確認視窗 -->
		<v-dialog v-model="dialogVisible.isShowConfirm" max-width="550px">
			<v-card>
				<v-card-title class="grey darken-2"
					>匯出【特殊帳單】

					<div class="top-right-item" @click="dialogVisible.isShowConfirm = false">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>

				<v-card-text>
					<div class="mt-4 mb-4 f18 yellow-text">⚠️ 產生帳單需較長時間，請選擇帳單類型</div>

					<div>
						<v-autocomplete :label="'選擇查看月份'" :items="dropdownItemsLatestMonth" chips v-model="searchFilter.targetMonth">
							<template v-slot:item="data">
								<div>
									<span>{{ data.item.text }}</span>
									<span class="f11 ml-1 grey-text"> ({{ data.item.extraProperty.accountingBeginEnd }}) </span>
								</div>
							</template>
						</v-autocomplete>
					</div>

					<div>
						<v-autocomplete :items="dropdownItemsInvoiceExportCategory" chips label="帳單分類" v-model="searchFilter.invoiceExportCategory"> </v-autocomplete>
					</div>

					<div style="margin-top: 40px">
						<v-checkbox :label="'每家遊戲商數字分開(不要合併)'" v-model="isSeperateEachGameVendor" style="max-width: 300px"></v-checkbox>
					</div>
					<div style="margin-top: -20px">
						<v-checkbox :label="'查詢額外項目(技術debug用)'" v-model="searchFilter.isShowSkipItemForDebug" style="max-width: 300px"></v-checkbox>
					</div>
				</v-card-text>

				<v-card-actions>
					<v-spacer></v-spacer>

					<v-btn color="yellow darken-1" text @click="doExportVworldInvoice" class="btn">
						<v-icon left>mdi-check-circle</v-icon>
						{{ $t('Button.__submit') }}
					</v-btn>
				</v-card-actions>
			</v-card>
		</v-dialog>

		<!-- 複製rules -->
		<v-dialog v-model="dialogVisible.isShowCopy" max-width="600px">
			<v-card>
				<v-card-title class="grey darken-2">
					複製一個相同配置
					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>

				<v-card-text style="min-height: 250px; padding: 10px 35px">
					<v-row>
						<v-col cols="12" md="12" sm="12">
							<v-text-field v-model="copyCloneSameRule.targetID" disabled :label="'複製原RuleID'"> </v-text-field>
						</v-col>

						<v-col cols="12" md="12" sm="12">
							<v-text-field v-model="copyCloneSameRule.displayName" :label="$t('TableHeader.__displayName')"> </v-text-field>
						</v-col>

						<v-col cols="12" md="12" sm="12">
							<v-text-field v-model="copyCloneSameRule.gameIDsText" :label="'支援哪些遊戲(填*號 或 逗號隔開gameID)'"> </v-text-field>
						</v-col>

						<v-col cols="12" md="12" sm="12">
							<v-text-field v-model="copyCloneSameRule.merchantIDsText" :label="'支援哪些商戶(逗號隔開商戶ID)'"> </v-text-field>
						</v-col>

						<v-col cols="12" md="12" sm="12">
							<v-text-field v-model="copyCloneSameRule.aggregativeGroup" :label="'折扣項目分組編號'"> </v-text-field>
						</v-col>

						<v-col cols="12" md="12" sm="12" class="minus-mt-4">
							<div class="common-info-label">起始時間</div>

							<date-picker
								v-model="copyCloneSameRule.beginTS"
								type="datetime"
								value-type="timestamp"
								v-if="isReloadDatePicker"
								@change="reloadPopupWhenValueChanged()"
								format="YYYY-MM-DD HH:mm:ss"
							>
							</date-picker>
						</v-col>

						<v-col cols="12" md="12" sm="12">
							<div class="common-info-label">結束時間</div>

							<date-picker
								v-model="copyCloneSameRule.endTS"
								type="datetime"
								value-type="timestamp"
								v-if="isReloadDatePicker"
								@change="reloadPopupWhenValueChanged()"
								format="YYYY-MM-DD HH:mm:ss"
							>
							</date-picker>
						</v-col>

						<v-col cols="12" md="12" sm="12">
							<v-checkbox v-model="copyCloneSameRule.isEnableFlag1" :label="'同步帳務組別分配'"> </v-checkbox>
						</v-col>
					</v-row>
				</v-card-text>

				<v-card-actions>
					<v-spacer></v-spacer>
					<v-btn text @click="closeAllModal" class="btn">
						<v-icon left>mdi-cancel</v-icon>
						{{ $t('Button.__cancel') }}
					</v-btn>
					<v-btn color="yellow darken-1" text @click="confirmCopyAndCreateNewRule" class="btn">
						<v-icon left>mdi-check-circle</v-icon>
						{{ $t('Button.__submit') }}
					</v-btn>
				</v-card-actions>
			</v-card>
		</v-dialog>

		<AccountingInvoiceAccordionView ref="refAccountingInvoiceAccordionView"> </AccountingInvoiceAccordionView>
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import * as Model from '@/models/interfaces/system-setting/accountingInvoice';
import * as CurrencyModel from '@/models/interfaces/system-setting/currency';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import * as EventModel from '@/models/interfaces/system-setting/events';
import AccountingInvoiceApi from '@/api/system-setting/accountingInvoice';
import CurrencyApi from '@/api/system-setting/currency';
import DropdownApi from '@/api/commons/dropdown';
import DatePicker from 'vue2-datepicker';
import { Locale, Currency, Status, GameVendorType } from '@/models/status/common';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { commonMethodMixin } from '@/utilities/common-methods';
import { pageData, time } from '@/utilities/api-handler.lib';
import { datetimeMixin, startEndDate, timeWithTimezone, daysAgo, weeksAgo, monthsAgo, formatMixin } from '@/utilities/datetime-format';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import { State, Action, Getter, namespace } from 'vuex-class';
const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';
import InternalTestApi from '@/api/commons/internalTest';
import Cookies from 'js-cookie';
import * as Qs from 'qs';
import { uuid } from 'vue-uuid';
import AccountingInvoiceAccordionView from '@/components/system-setting/AccountingInvoiceAccordionView.vue';

// @ts-ignore
@Component({ components: { DatePicker, AccountingInvoiceAccordionView }, mixins: [commonDisplayFiltersMixin, commonMethodMixin, datetimeMixin] })
export default class AccountingInvoice extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];

	dialogVisible: DropdownModel.DialogVisible = new DropdownModel.DialogVisible();
	isSearchAndShowTable: boolean = false;
	renderComponent: boolean = true;
	isAutoSplitMerchantUnderMYCSuper: boolean = true;
	isReloadDatePicker: boolean = true;
	isRenderWarningText: boolean = true;
	isSeperateEachGameVendor: boolean = false;

	excelRawTextForCopy: string = '';
	perPage: number = 100;

	queryAdjustmentRulesAndAssignGroup: any = {};
	allCurrencyList: CurrencyModel.ICurrencyAllListResult = {} as CurrencyModel.ICurrencyAllListResult;
	searchAccountingListResult: Model.IAccountingInvoiceList = {} as Model.IAccountingInvoiceList;
	searchFilter: Model.IQueryAccountingInvoiceListParameter = {} as Model.IQueryAccountingInvoiceListParameter;

	searchAndDisplayMode: number = 0;

	DataTablePagingParameter: DropdownModel.DataTablePagingParameter = new DropdownModel.DataTablePagingParameter();
	datetimeRangePicker: DropdownModel.DatetimeRangePicker = new DropdownModel.DatetimeRangePicker();
	copyCloneSameRule: DropdownModel.ILazyCommonParameterReqeust = {} as DropdownModel.ILazyCommonParameterReqeust;
	queryVworldInvoiceEachMerchantDetailAccordionData = {};

	selectEditTargetCurrency: string = 'USDT';
	searchParentName: string = '';
	expandPannels: number[] = [];
	panel: number[] = [0];
	searchTargetRules: number[] = [];

	dropdownItemsLatestMonth: DropdownModel.IDropdownItems[] = [];
	dropdownItemsStatus: DropdownModel.IDropdownItems[] = [];
	dropdownItemsMerchants: DropdownModel.IDropdownItems[] = [];
	dropdownItemsInvoiceExportCategory: DropdownModel.IDropdownItems[] = [];

	@Action('Players/setEventID') private setEventID!: any;

	get editCurrenyDropdown() {
		const items: any[] = [];
		items.push({ text: '轉換匯率(USDT)', value: 'USDT' });
		items.push({ text: '轉換匯率(TWD-AGG帳單使用)', value: 'TWD' });
		return items;
	}

	get filterParentGroupByKeywordSearch() {
		const myObject: any = this.queryVworldInvoiceEachMerchantDetailAccordionData;

		if (!myObject) {
			return [];
		}

		if (!myObject.accountingAccordionData) {
			return [];
		}

		if (!myObject.accountingAccordionData.parentGroups) {
			return [];
		}

		const vm: any = this;

		if (vm.isEmptyString(this.searchParentName)) {
			return myObject.accountingAccordionData.parentGroups;
		}

		return myObject.accountingAccordionData.parentGroups.filter((x: any) => x.groupName.includes(this.searchParentName));
	}

	get hasExportInvoicePermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.System_AccountingInvoice_ExportInvoice) >= 0;
	}

	get hasSuperAdminPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSetting_SystemConfigSetting) >= 0;
	}

	get headers() {
		const data = [
			{ text: this.$t('TableHeader.__action'), value: 'action', sortable: false, align: 'center' },
			{ text: '帳單配置', value: 'invoiceGroupCategoryName', sortable: false, align: 'center' },
			{ text: '商戶ID', value: 'merchantID', sortable: false, align: 'center' },
			{ text: '商戶名稱', value: 'merchantName', sortable: false, align: 'start' },
			{ text: '盤費%數', value: 'feeRateText', sortable: false, align: 'center' },
			// { text: '', value: 'currency', sortable: false, align: 'center' },
			{ text: '貨幣／匯率', value: 'exchangeRateToUSDT', sortable: false, align: 'end' },
			{ text: '商戶輸贏(原始貨幣)', value: 'totalWinLoss', sortable: false, align: 'end' },
			{ text: '盤費(原始貨幣)', value: 'chargeFeeAmount', sortable: false, align: 'end' },
			{ text: '盤費(USDT)', value: 'chargeFeeAmountAftetExchange', sortable: false, align: 'end' },
			{ text: '標籤備註', value: 'tagMemo', sortable: false, align: 'start' },
		];

		return data;
	}

	get dropdownRules() {
		if (this.queryAdjustmentRulesAndAssignGroup == null || !this.queryAdjustmentRulesAndAssignGroup) {
			return [];
		}
		if (!this.queryAdjustmentRulesAndAssignGroup.items || this.queryAdjustmentRulesAndAssignGroup.items.length <= 0) {
			return [];
		}

		const myDropdown = this.queryAdjustmentRulesAndAssignGroup.items.map((x: any) => {
			return {
				text: x.nameCN,
				value: x.ruleID,
			};
		});

		return myDropdown;
	}

	refresh() {}

	doNothing() {
		console.log('doNothing');
	}

	enableExpandPannel(index: number) {
		if (this.expandPannels != null && this.expandPannels.length === 1) {
			if (this.expandPannels[0] === index) {
				this.expandPannels = [];
			}
		} else {
			this.expandPannels = [];
			this.expandPannels.push(index);
		}

		console.log(this.expandPannels);
	}

	currencyChangeWarningText(item: CurrencyModel.ICurrencyItem) {
		if (item.exchangeRateToUSDT === 0 || item.exchangeRate_EditNewValue === 0) {
			console.log('return');
			return '';
		}

		let diffValue = 0;

		if (this.isEditUSDT()) {
			diffValue = ((item.exchangeRate_EditNewValue - item.exchangeRateToUSDT) / item.exchangeRateToUSDT) * 100;
		} else {
			diffValue = ((item.exchangeRate_EditNewValue - item.exchangeRateToTWD) / item.exchangeRateToTWD) * 100;
		}
		console.log(diffValue);
		if (Math.abs(diffValue) <= 0.01) {
			return '';
		}

		return (diffValue >= 0 ? '+' : '') + diffValue.toFixed(2) + '%';
	}

	whenChangeTargetCurrency() {
		for (const item of this.allCurrencyList.items) {
			if (this.isEditUSDT()) {
				item.exchangeRate_EditNewValue = item.exchangeRateToUSDT;
			} else {
				item.exchangeRate_EditNewValue = item.exchangeRateToTWD;
			}
		}
	}

	getAllCurrency() {
		CurrencyApi.getAllCurrencyList().then((res) => {
			this.allCurrencyList = res.data;
			this.whenChangeTargetCurrency();
		});
	}

	refreshRateByAPI() {
		if (confirm('透過匯率API工具更新會需要較長時間，是否確定執行?')) {
			EventBus.$emit('show_mask', null);
			CurrencyApi.updateCurrencyRateByThirdAPI()
				.then((res) => {
					this.getAllCurrency();
				})
				.finally(() => {
					EventBus.$emit('hide_mask', null);
				});
		}
	}

	isEditUSDT() {
		return this.selectEditTargetCurrency === 'USDT';
	}

	refreshComponentValueSaveUpdate(item: CurrencyModel.ICurrencyItem) {
		if (this.isEditUSDT()) {
			item.exchangeRateToUSDT = item.exchangeRate_EditNewValue;
		} else {
			item.exchangeRateToTWD = item.exchangeRate_EditNewValue;
		}
		item.lastUpdateTimeDiffText = '0 分鐘前';
	}

	saveCurrencyRate(item: CurrencyModel.ICurrencyItem) {
		let diffValue = 100;

		if (this.isEditUSDT()) {
			if (item.exchangeRateToUSDT > 0) {
				diffValue = ((item.exchangeRate_EditNewValue - item.exchangeRateToUSDT) / item.exchangeRateToUSDT) * 100;
			}
		} else {
			if (item.exchangeRateToTWD > 0) {
				diffValue = ((item.exchangeRate_EditNewValue - item.exchangeRateToTWD) / item.exchangeRateToTWD) * 100;
			}
		}

		const myModel: CurrencyModel.ICurrencyUpdateRequest = {} as CurrencyModel.ICurrencyUpdateRequest;
		myModel.currency = item.currency;
		myModel.exchangeRate = item.exchangeRate_EditNewValue;
		myModel.exchangeCurrency = this.selectEditTargetCurrency;

		if (Math.abs(diffValue) >= 20) {
			if (
				confirm(
					'您針對貨幣【' +
						item.currency +
						'】進行修改。匯率與上次相差' +
						diffValue.toFixed(2) +
						'%。是否繼續儲存?\n您目前是在編輯💰《兌換' +
						this.selectEditTargetCurrency +
						'》💰的匯率　(確定沒操作錯誤？)'
				)
			) {
				EventBus.$emit('show_mask', null);
				CurrencyApi.updateSingleCurrency(myModel)
					.then((res) => {
						this.refreshComponentValueSaveUpdate(item);
						item.lastUpdateTime = res.data.resultTime;
						EventBus.$emit(`successfully-modified`);
					})
					.finally(() => {
						EventBus.$emit('hide_mask', null);
					});
			}
		} else {
			EventBus.$emit('show_mask', null);
			CurrencyApi.updateSingleCurrency(myModel)
				.then((res) => {
					this.refreshComponentValueSaveUpdate(item);
					item.lastUpdateTime = res.data.resultTime;
					EventBus.$emit(`successfully-modified`);
				})
				.finally(() => {
					EventBus.$emit('hide_mask', null);
				});
		}
	}

	copySQL_1() {
		let mySQL = '';
		mySQL += '-- 查看所有rules' + '\n';
		mySQL += 'DECLARE @targetRuleID int = XXXXXXXXXX' + '\n';
		mySQL += 'INSERT INTO [dbo].[AccountingAdjustmentRuleGroupAssign]' + '\n';
		mySQL += 'SELECT AccountingGroupID , @targetRuleID' + '\n';
		mySQL += 'FROM [dbo].[AccountingAdjustmentRuleGroupAssign]' + '\n';
		mySQL += 'WHERE AccountingAdjustmentRuleID = 100014' + '\n' + '\n';
		mySQL += '-- 查看所有rules' + '\n';
		mySQL += 'SELECT * FROM [dbo].[AccountingAdjustmentRule]' + '\n';

		const vm: any = this;
		vm.copyTextToClipboard(mySQL);
	}

	getInvoiceList() {
		this.isSearchAndShowTable = true;

		EventBus.$emit('show_mask', null);

		this.searchAccountingListResult.items = [];
		this.excelRawTextForCopy = '';

		AccountingInvoiceApi.getExportInvoiceList(this.searchFilter)
			.then((res) => {
				this.searchAccountingListResult = res.data;
				this.setAllMerchantsCheckboxValue(false);
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	setAllMerchantsCheckboxValue(targetValue: boolean) {
		if (this.searchAccountingListResult.items == null || this.searchAccountingListResult.items.length <= 0) {
			return;
		}
		this.searchAccountingListResult.items.forEach((x) => (x.f2e_isCheckbox = targetValue));

		this.isSearchAndShowTable = false;

		this.$nextTick(() => {
			this.isSearchAndShowTable = true;
		});
	}

	reloadPopupWhenValueChanged() {
		// 不知道為何只有這一頁會出現無法刷新畫面中的值...
		this.isReloadDatePicker = false;
		this.isReloadDatePicker = true;
	}

	whenChangeExchangeRateValue() {
		this.isRenderWarningText = false;

		setTimeout(() => {
			this.isRenderWarningText = true;
		}, 100);
	}

	copyAndCreateNewRule(item: any) {
		this.copyCloneSameRule.targetID = item.ruleID;
		this.copyCloneSameRule.displayName = item.nameCN + ' (Copy)';
		this.copyCloneSameRule.gameIDsText = item.gameIDs;
		this.copyCloneSameRule.merchantIDsText = item.merchantIDs;
		this.copyCloneSameRule.aggregativeGroup = item.aggregativeGroup;
		this.copyCloneSameRule.isEnableFlag1 = true;
		this.copyCloneSameRule.beginTS = item.beginTimeTS * 1000;
		this.copyCloneSameRule.endTS = item.endTimeTS * 1000;

		setTimeout(() => {
			this.dialogVisible.isShowCopy = true;
		}, 100);
	}

	confirmCopyAndCreateNewRule(ruleID: number) {
		EventBus.$emit('show_mask', null);

		if (this.copyCloneSameRule.beginTS > 0) {
			this.copyCloneSameRule.begin = startEndDate(this.copyCloneSameRule.beginTS);
		}

		if (this.copyCloneSameRule.endTS > 0) {
			this.copyCloneSameRule.end = startEndDate(this.copyCloneSameRule.endTS);
		}

		AccountingInvoiceApi.copyAndCreateNewRule(this.copyCloneSameRule)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	generateVworldInvoiceDetailItems() {
		this.searchFilter.invoiceExportCategory = -1;
		this.dialogVisible.isShowConfirm = true;
	}

	doExportVworldInvoice() {
		if (this.searchFilter.targetMonth == null || this.searchFilter.targetMonth.length <= 0) {
			alert('請選擇月份');
			return;
		}

		if (this.searchFilter.invoiceExportCategory === -1) {
			alert('請選擇帳單分類');
			return;
		}

		if (this.searchFilter.invoiceExportCategory === 999) {
			this.generateAggGameVendorInvoiceDetailItems();
			return;
		} else {
			EventBus.$emit('show_mask', null);

			const searchExtraQS =
				'&isShowSkipItemForDebug=' +
				(this.searchFilter.isShowSkipItemForDebug ? 'true' : 'false') +
				'&isSeperateEachGameVendor=' +
				this.isSeperateEachGameVendor.toString() +
				'&invoiceExportCategory=' +
				this.searchFilter.invoiceExportCategory;

			const targetUrl = process.env.VUE_APP_API_RAW + 'accounting-export/export-excel-raw-text-vwolrd-version?targetMonth=' + this.searchFilter.targetMonth + searchExtraQS;
			InternalTestApi.downloadFileByTargetUrl(targetUrl)
				.then(() => {})
				.finally(() => {
					console.log('------end------');
					EventBus.$emit('hide_mask', null);

					this.dialogVisible.isShowConfirm = false;
				});
		}
	}

	generateAggGameVendorInvoiceDetailItems() {
		if (this.searchFilter.targetMonth == null || this.searchFilter.targetMonth.length <= 0) {
			alert('請選擇月份');
			return;
		}

		EventBus.$emit('show_mask', null);

		const invoiceExportCategory = this.searchFilter.invoiceExportCategory;
		const searchDebug = '&InvoiceExportCategory=' + invoiceExportCategory + '&isShowSkipItemForDebug=' + (this.searchFilter.isShowSkipItemForDebug ? 'true' : 'false');
		const targetUrl = process.env.VUE_APP_API_RAW + 'accounting-export/export-excel-agg-version?targetMonth=' + this.searchFilter.targetMonth + searchDebug;

		InternalTestApi.downloadFileByTargetUrl(targetUrl)
			.then(() => {})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	copyAllMerchantAmountData(parentGroup: any[]) {
		let myText = '';

		for (const item of parentGroup) {
			myText += item.subCateogyName + '\t' + item.debug_MerchantID + '\t' + item.debug_MerchantName + '\t' + item.referAmount2 + '\n';
		}

		const vm: any = this;
		vm.copyTextToClipboard(myText);
	}

	showExtraFilterRulesDialog() {
		const targetRef: any = this.$refs.refAccountingInvoiceAccordionView;
		targetRef.searchFilter.targetMonth = this.searchFilter.targetMonth;
		targetRef.showExtraFilterRulesDialog();
	}

	showCurrencyRateUpdateLogs(myCurrency: string) {
		const myFilter: any = {
			querySqlID: 900028,
			type: 3518,
			targetKey: myCurrency,
		};

		const myConfig: any = {
			maxWidth: 1000,
			isShowCloseButton: true,
		};

		EventBus.$emit(`globalShowCustomizedColumnsDisplayPopupDialog`, myFilter, myConfig);
	}

	async exportInvoiceForCheckList() {
		if (this.searchAccountingListResult.items == null || this.searchAccountingListResult.items.length <= 0) {
			return;
		}
		let isAnyChecked = false;

		for (const item of this.searchAccountingListResult.items) {
			if (item.f2e_isCheckbox) {
				isAnyChecked = true;
			}
		}

		if (!isAnyChecked) {
			alert('請勾選至少一個');
			return;
		}

		EventBus.$emit('show_mask', null);

		const urls: string[] = [];

		this.searchAccountingListResult.items.forEach((item) => {
			if (item.f2e_isCheckbox === false) {
				return;
			}
			const queryString = '?AccountingGroupID=' + item.accountingGroupID + '&TargetMerchantID=' + item.merchantID + '&TargetMonth=' + this.searchFilter.targetMonth;
			const targetUrl = process.env.VUE_APP_API_RAW + 'invoice-generate' + queryString;
			urls.push(targetUrl);
		});

		EventBus.$emit('show_mask', null);
		try {
			this.downloadFilesSequentially(urls)
				.then(() => {
					console.log('所有文件下载完成');
				})
				.catch((error) => {
					console.error('下载文件时出错：', error);
				})
				.finally(() => {
					EventBus.$emit('hide_mask', null);
				});
		} catch (e) {
			console.log(e);
		}
	}

	downloadFilesSequentially(urls: string[]) {
		const myHeaders: any = {
			access_token: [REDACTED]'token'),
		};

		// 创建一个Promise链，依次下载每个文件
		return urls.reduce((chain, url) => {
			return chain.then(() => {
				return fetch(url, {
					method: 'GET',
					headers: myHeaders,
				})
					.then((response) => {
						if (!response.ok) {
							throw new Error(`Failed to download ${url}`);
						}

						return response.blob().then((blob) => {
							// 处理下载的文件，例如保存到本地
							console.log(`Downloaded ${url}`);
							console.log(blob);

							// 為了避免亂碼問題 檔名base64編碼了
							// @ts-ignore
							const fileName = decodeURIComponent(escape(atob(response.headers.get('X-target-File-Name'))));

							console.log(fileName);
							console.log(response.headers.get('Content-Disposition'));

							const a = document.createElement('a');
							a.href = window.URL.createObjectURL(blob);
							a.download = fileName;
							a.style.display = 'none';
							document.body.appendChild(a);
							a.click();
							window.URL.revokeObjectURL(a.href);
							document.body.removeChild(a);
						});
					})
					.then((blobObject) => {
						console.log(blobObject);
					});
			});
		}, Promise.resolve());
	}

	generateTotalExcelRows() {
		const isOK = confirm('是否產生總表? 將花費較長時間');

		if (!isOK) {
			return;
		}

		const vm: any = this;
		const myFilter: any = {};
		myFilter.targetMonth = this.searchFilter.targetMonth;
		myFilter.IgnoreZero = true;

		EventBus.$emit('show_mask', null);
		AccountingInvoiceApi.exportMonthlyAccountingTotalExcelTextForCopy(myFilter)
			.then((res) => {
				console.log(res);
				this.excelRawTextForCopy = res;
				vm.copyTextToClipboard2(res);
				vm.copySuccess();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	showInvoiceListDialog() {
		this.dialogVisible.isShowExport = true;
		this.excelRawTextForCopy = '';
	}

	closeAllModal() {
		this.dialogVisible.resetAllValueToFalse();
	}

	clear() {}

	copySuccess() {
		EventBus.$emit(`successfully-copy`);
	}

	mounted() {
		this.refresh();

		this.getAllCurrency();

		DropdownApi.getNormalStatus().then((res) => {
			this.dropdownItemsStatus = res;
		});

		DropdownApi.getMerchants().then((res) => {
			this.dropdownItemsMerchants = res;
		});

		DropdownApi.getLatestMonth().then((res) => {
			this.dropdownItemsLatestMonth = res;
		});

		DropdownApi.getInvoiceExportCategory().then((res) => {
			this.dropdownItemsInvoiceExportCategory = res;
		});

		AccountingInvoiceApi.getAdjustmentRulesAndAssignGroup()
			.then((res) => {
				this.queryAdjustmentRulesAndAssignGroup = res.data;
			})
			.finally(() => {});
	}
}
</script>

<style scoped>
.myCategoryColor_0 {
	color: #63ae6d;
}

.myCategoryColor_1 {
	color: #b0965d;
}

.timeDiff {
	color: #a2c0bd;
}

.my-chip-grey {
	/* background: #333 !important; */
}

.my-chip-blue {
	background: #2b355b !important;
}

.myInputText {
	color: #fff;
	outline: none !important;
	background: #444;
	border: 1px solid #444;
	border-radius: 3px;
	text-align: right;
	padding-right: 5px;
	font-size: 18px;
}

.myInputText:focus {
	outline: none !important;
	border: 1px solid #dcac47;
	box-shadow: 0 0 10px #545454;
}

.myInputText:disabled {
	opacity: 0.8;
	color: #666;
}

.myTable002 {
	width: 700px !important;
}

.myTable002 th,
.myTable002 td {
	font-size: 15px !important;
	font-weight: 400 !important;
}

.light-yellow-text {
	color: #dbe39c !important;
}

.grey-layout,
grey-layout,
.grey-layout .timeDiff {
	color: #666;
}
</style>
