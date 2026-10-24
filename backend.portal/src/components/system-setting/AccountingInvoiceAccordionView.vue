<template>
	<v-app v-if="renderComponent" class="app">
		<!--彈出視窗: 篩選條件輸入-商戶折扣明細條目 -->
		<v-dialog v-model="dialogVisible.isShowExtra1" max-width="750px">
			<v-card class="dialog-v-card-border">
				<v-card-title class="grey darken-2">
					額外查詢篩選 (各商戶折扣明細條目)
					<div class="top-right-item" @click="dialogVisible.isShowExtra1 = false">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>

				<v-card-text style="min-height: 250px; padding: 10px 35px">
					<div class="text-l w100p">
						<v-checkbox :label="'自動調整二級代理資料分組(建議勾選)'" v-model="isAutoSplitMerchantUnderMYCSuper" style="display: inline-block"> </v-checkbox>
					</div>

					<div class="w100p mt-4">
						<v-autocomplete :label="'選擇查看月份'" :items="dropdownItemsLatestMonth" chips v-model="searchFilter.targetMonth" style="width: 300px">
							<template v-slot:item="data">
								<div>
									<span>{{ data.item.text }}</span>
									<span class="f11 ml-1 grey-text"> ({{ data.item.extraProperty.accountingBeginEnd }}) </span>
								</div>
							</template>
						</v-autocomplete>
					</div>

					<div class="w100p mt-4">
						<v-autocomplete :items="dropdownItemsInvoiceExportCategory" v-model="invoiceExportCategory" style="width: 300px" chips :label="'帳單分類'"> </v-autocomplete>
					</div>

					<div class="w100p mt-4">
						<v-autocomplete :items="dropdownSearchAndDisplayMode" v-model="searchAndDisplayMode" style="width: 300px" chips :label="'查詢方式'"> </v-autocomplete>
					</div>

					<v-autocomplete class="mt-4" v-model="searchTargetRules" :items="dropdownRules" multiple clearable small-chips :label="'僅搜尋特定rules((可不勾選))'"> </v-autocomplete>

					<div class="100p minus-mt-2 z-index-top-layer">
						<span
							v-for="(item, index) in queryAdjustmentRulesAndAssignGroup.aggregativeGroupList"
							class="link-text mr-2"
							@click="selectDropdownByAggregativeGroup(item.aggregativeGroup)"
							:key="index"
						>
							{{ item.name }}
						</span>

						<div v-if="hasSuperAdminPermission" class="yellow-text">
							<v-icon small color="yellow">error</v-icon>
							<span class="f11 ml-1">參考代碼: AccountingAdjustmentRuleAggregativeGroupEnum</span>
						</div>
					</div>
				</v-card-text>

				<v-card-actions style="margin-top: 50px">
					<v-spacer></v-spacer>
					<v-btn text @click="closeAllModal" class="btn">
						<v-icon left>mdi-cancel</v-icon>
						{{ $t('Button.__cancel') }}
					</v-btn>

					<v-btn color="yellow darken-1" text @click="generateVworldInvoiceEachMerchantDebugDetail" class="btn">
						<v-icon left>mdi-check-circle</v-icon>
						開始查詢
					</v-btn>
				</v-card-actions>
			</v-card>
		</v-dialog>

		<!--《查詢結果》商戶明細條目 -->
		<v-dialog v-model="dialogVisible.isShowDetailInfo" max-width="1100px" scrollable>
			<v-card class="dialog-v-card-border">
				<v-card-title class="grey darken-2">
					查詢詳細

					<span class="f12 grey-text ml-2" v-if="queryVworldInvoiceEachMerchantDetailAccordionData.accountingAccordionData">
						{{ queryVworldInvoiceEachMerchantDetailAccordionData.accountingAccordionData.titleHint }}
					</span>

					<!-- <div style="display:inline-block" class="yellow-text f14 ml-4">
						<v-icon small color="yellow">info</v-icon>建議搭配 Ctrl + F 快捷鍵搜尋畫面 (搜代理名稱)
					</div> -->

					<div style="inline-block;width:350px;position: absolute;right: 50px;">
						<v-text-field placeholder="搜尋總代名稱" append-icon="search" v-model="searchParentName" clearable style="margin-top: 0px; padding-top: 0px" class="ml-2"> </v-text-field>
					</div>

					<div class="top-right-item" @click="dialogVisible.isShowDetailInfo = false">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>

				<v-card-text
					style="min-height: 770px; overflow-y: scroll; padding-right: 0px; position: relative"
					v-if="queryVworldInvoiceEachMerchantDetailAccordionData.accountingAccordionData && queryVworldInvoiceEachMerchantDetailAccordionData.accountingAccordionData.parentGroups"
				>
					<div class="mt-0 text-r mr-2" style="min-height: 35px">
						<div class="w100p">
							<v-btn
								color="blue lighten-3"
								text
								class="btn search"
								small
								style="position: absolute; top: 5px; right: 5px"
								@click="copyAllMerchantAmountData(queryVworldInvoiceEachMerchantDetailAccordionData.accountingAccordionData.parentGroups)"
							>
								<v-icon left>file_copy</v-icon>
								複製所有商戶數字至剪貼簿(Excel文字格式) </v-btn
							>&#9;
						</div>
					</div>

					<div class="w100p" style="padding: 2px 15px 15px 15px" v-if="!isEmptyString(queryVworldInvoiceEachMerchantDetailAccordionData.accountingAccordionData.memoHint)">
						<div class="w100p"><v-icon color="yellow" small>info</v-icon><span class="yellow-text">注意事項</span></div>

						<div style="max-width: 700px">
							{{ queryVworldInvoiceEachMerchantDetailAccordionData.accountingAccordionData.memoHint }}
						</div>
					</div>

					<div style="width: 65%" v-if="queryVworldInvoiceEachMerchantDetailAccordionData.accountingAccordionData.aggregateBySubCategorySummaryItems.length > 0">
						<v-expansion-panels accordion multiple class="mt-2">
							<v-expansion-panel>
								<v-expansion-panel-header class="myHoverRow">
									<div style="display: inline-block">
										<v-icon small class="mr-1">stars</v-icon>依子分類加總(貨幣/Curreny/其他)
										<span class="f12 grey-text">*點擊可展開此</span>
									</div>
								</v-expansion-panel-header>

								<v-expansion-panel-content>
									<v-data-table
										class="my-small-height-table mb-4"
										:items="queryVworldInvoiceEachMerchantDetailAccordionData.accountingAccordionData.aggregateBySubCategorySummaryItems"
										:headers="aggregateBySubCategoryHeaders"
										:footer-props="{ 'items-per-page-options': [100] }"
										hide-default-footer
									>
										<template v-slot:item.referAmount1="{ item }">
											{{ item.referAmount1 | amountDisplay }}
										</template>

										<template v-slot:item.referAmount2="{ item }">
											{{ item.referAmount2 | amountDisplay }}
										</template>
									</v-data-table>

									<div>
										<ul style="color: #cfc958">
											<li>若有補帳項目請排除之。這類型的不包含在內、需另列計算</li>
											<li>正負符號可能與另份報表相反。建議無視此問題</li>
										</ul>
									</div>
								</v-expansion-panel-content>
							</v-expansion-panel>
						</v-expansion-panels>
					</div>

					<v-expansion-panels v-model="expandPannels" accordion multiple class="mt-2">
						<v-expansion-panel v-for="(item, index) in filterParentGroupByKeywordSearch" :key="index">
							<v-expansion-panel-header style="padding: 10px" class="myHoverRow">
								<!-- <div @click.stop="doNothing()" class="my-eazy-input-position">
									<input type="text" class="my-eazy-input" placeholder="臨時備註">
								</div> -->

								<div :class="item.isExpired ? 'expired-grey-text' : ''" :title="item.ruleID">
									<v-chip :class="'mr-2 ' + item.cssStyleColor">{{ item.subCateogyName }}</v-chip>

									<div style="max-width: 650px; display: inline-block; vertical-align: middle" :title="'點擊複製名稱'">
										<div class="copyGroupName" @click.stop="copyTextToClipboard2(item.groupName)">
											{{ item.groupName }}
										</div>
										<div class="sub-title-text sub-title-over">
											<v-icon color="red" small v-if="item.isMinusProfit">error</v-icon>
											<span v-if="item.isMinusProfit" class="minus-red">負盈利* </span>

											<span small class="chip-max-width" v-for="(text, index) in item.itemNames" :class="item.isMinusProfit ? 'minus-red' : ''" :key="index">{{ text }} </span>
										</div>
									</div>

									<!-- <v-chip small class="chip-max-width" v-for="(text , index) in item.itemNames"
										:key="index">{{ text }}
									</v-chip> -->

									<div class="displayAmount1">
										<div class="w100p">
											<v-tooltip top color="orange darken-4">
												<template v-slot:activator="{ on, attrs }">
													<span v-bind="attrs" v-on="on">
														<v-icon small color="grey darken-1" class="mr-1">info</v-icon>
													</span>
												</template>
												<span>
													<ul>
														<li>左邊數字：輸贏(原始100%)，Esther表的數字可能已乘上 30%/50%/70%</li>
													</ul>
													<ul>
														<li>右邊數字：折扣盤費(原貨幣)，已乘上5% (或N%)</li>
													</ul>
												</span>
											</v-tooltip>

											<div class="chip-minx-width chip-style-box" :title="'輸贏'">
												{{ item.referAmount1 | amountDisplay }}
											</div>

											<div
												color="grey darken-3"
												:title="'折扣盤費(原幣值)'"
												@click.stop="copyTextToClipboard2(item.referAmount2.toString())"
												class="ml-1 chip-minx-width chip-style-box"
											>
												{{ item.referAmount2 | amountDisplay }}
											</div>
										</div>
										<div class="w100p">
											<div class="my-extra-memo">
												<!-- {{ item.groupName }} -->
											</div>
										</div>
									</div>
								</div>
							</v-expansion-panel-header>

							<v-expansion-panel-content>
								<div class="text-r mt-2">
									<div class="link-text-grey f12" @click="copyChildItemsAsExcelTextFormat(item.items)">複製細項商戶(Excel文字格式)</div>
								</div>

								<v-data-table :items="item.items" :headers="accordionViewHeaders" fixed-header height="450px" :footer-props="{ 'items-per-page-options': [100] }" hide-default-footer>
									<template v-slot:item.description="{ item }">
										<div style="width: 300px; overflow: hidden; text-overflow: ellipsis" :title="item.description">
											{{ item.description }}
										</div>

										<div class="sub-title-text">
											{{ item.dateMemo }}
										</div>
									</template>

									<template v-slot:item.referenceAmount="{ item }">
										{{ item.referenceAmount | amountDisplay }}
									</template>

									<template v-slot:item.finalAmount_decimail2="{ item }">
										{{ item.finalAmount_decimail2 | amountDisplay }}
									</template>
								</v-data-table>
							</v-expansion-panel-content>
						</v-expansion-panel>
					</v-expansion-panels>

					<div v-if="hasSuperAdminPermission" class="mt-4">
						<div>
							<h3>額外信息</h3>
						</div>

						<div
							style="white-space: break-spaces"
							class="pointer"
							v-clipboard:success="copySuccess"
							v-clipboard:copy="queryVworldInvoiceEachMerchantDetailAccordionData.accountingAccordionData.allMerchantAndDiscountEvent"
						>
							<br />{{ queryVworldInvoiceEachMerchantDetailAccordionData.accountingAccordionData.allMerchantAndDiscountEvent }}
						</div>
					</div>
				</v-card-text>
			</v-card>
		</v-dialog>
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

import * as Qs from 'qs';
import { uuid } from 'vue-uuid';

// @ts-ignore
@Component({ components: { DatePicker }, mixins: [commonDisplayFiltersMixin, commonMethodMixin, datetimeMixin] })
export default class AccountingInvoiceAccordionView extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];

	dialogVisible: DropdownModel.DialogVisible = new DropdownModel.DialogVisible();
	isSearchAndShowTable: boolean = false;
	renderComponent: boolean = true;
	isAutoSplitMerchantUnderMYCSuper: boolean = true;
	excelRawTextForCopy: string = '';
	perPage: number = 100;

	queryAdjustmentRulesAndAssignGroup: any = {};
	allCurrencyList: CurrencyModel.ICurrencyAllListResult = {} as CurrencyModel.ICurrencyAllListResult;
	searchAccountingListResult: Model.IAccountingInvoiceList = {} as Model.IAccountingInvoiceList;
	searchFilter: Model.IQueryAccountingInvoiceListParameter = {} as Model.IQueryAccountingInvoiceListParameter;

	searchAndDisplayMode: number = 0;
	invoiceExportCategory: number = -1;

	DataTablePagingParameter: DropdownModel.DataTablePagingParameter = new DropdownModel.DataTablePagingParameter();
	datetimeRangePicker: DropdownModel.DatetimeRangePicker = new DropdownModel.DatetimeRangePicker();
	copyCloneSameRule: DropdownModel.ILazyCommonParameterReqeust = {} as DropdownModel.ILazyCommonParameterReqeust;
	queryVworldInvoiceEachMerchantDetailAccordionData = {};

	searchParentName: string = '';
	expandPannels: number[] = [];
	panel: number[] = [0];
	searchTargetRules: number[] = [];

	dropdownItemsLatestMonth: DropdownModel.IDropdownItems[] = [];
	dropdownItemsStatus: DropdownModel.IDropdownItems[] = [];
	dropdownItemsMerchants: DropdownModel.IDropdownItems[] = [];
	dropdownItemsInvoiceExportCategory: DropdownModel.IDropdownItems[] = [];

	@Action('Players/setEventID') private setEventID!: any;

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

		const keywordLower = this.searchParentName.toLowerCase();
		return myObject.accountingAccordionData.parentGroups.filter((x: any) => x.groupName.toLowerCase().includes(keywordLower));
	}

	get hasExportInvoicePermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.System_AccountingInvoice_ExportInvoice) >= 0;
	}

	get hasSuperAdminPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSetting_SystemConfigSetting) >= 0;
	}

	get accordionViewHeaders() {
		const data = [
			// { text: '貨幣', value: 'currency', sortable: false, align: 'center' },
			{ text: '項目名稱', value: 'description', sortable: false, align: 'start' },
			{ text: '商戶名稱', value: 'merchantName', sortable: false, align: 'start' },
			{ text: '輸贏(W/L)／有效流水', value: 'referenceAmount', sortable: false, align: 'end' },
			{ text: '盤費/折扣金額', value: 'finalAmount_decimail2', sortable: false, align: 'end' },
			{ text: '盤費比例(%)', value: 'discountRate', sortable: false, align: 'end' },
		];

		return data;
	}

	get aggregateBySubCategoryHeaders() {
		const data = [
			// { text: '貨幣', value: 'currency', sortable: false, align: 'center' },
			{ text: '分類名稱', value: 'subCateogry', sortable: false, align: 'start' },
			{ text: '輸贏(W/L)／有效流水', value: 'referAmount1', sortable: false, align: 'end' },
			{ text: '盤費/折扣金額', value: 'referAmount2', sortable: false, align: 'end' },
		];

		return data;
	}

	get dropdownSearchAndDisplayMode() {
		const items = [];
		items.push({ text: '下載Excel檔案', value: 1 });
		items.push({ text: '使用後台畫面瀏覽(手風琴格式)', value: 2 });
		return items;
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

	copyChildItemsAsExcelTextFormat(items: any[]) {
		let myText = '';

		for (const item of items) {
			myText += item.description + '\t' + item.merchantName + '\t' + item.referenceAmount + '\t' + item.finalAmount_decimail2 + '\t' + item.discountRate + '\n';
		}
		const vm: any = this;
		vm.copyTextToClipboard2(myText);
		return;
	}

	copyAllMerchantAmountData(parentGroup: any[]) {
		let myText = '';

		for (const item of parentGroup) {
			myText += item.subCateogyName + '\t' + item.debug_MerchantID + '\t' + item.debug_MerchantName + '\t' + item.referAmount2 + '\n';
		}

		const vm: any = this;
		vm.copyTextToClipboard2(myText);
	}

	generateVworldInvoiceEachMerchantDebugDetail() {
		if (this.searchFilter.targetMonth == null || this.searchFilter.targetMonth.length <= 0) {
			alert('請選擇月份');
			return;
		}

		if (this.searchAndDisplayMode === 0) {
			alert('請選擇查詢模式');
			return;
		}

		if (this.invoiceExportCategory === -1) {
			alert('請選擇帳單類型');
			return;
		}

		EventBus.$emit('show_mask', null);

		const myQuery = {
			isAutoSplitMerchantUnderMYCSuper: this.isAutoSplitMerchantUnderMYCSuper,
			isShowSkipItemForDebug: this.searchFilter.isShowSkipItemForDebug,
			targetMonth: this.searchFilter.targetMonth,
			filterRuleIDs: this.searchTargetRules,
			invoiceExportCategory: this.invoiceExportCategory,
		};

		const queryString = Qs.stringify(myQuery);
		const targetUrl = process.env.VUE_APP_API_RAW + 'accounting-export/export-excel-vwolrd-version-promotion-detail?' + queryString;

		this.expandPannels = [];
		this.searchParentName = '';

		if (this.searchAndDisplayMode === 1) {
			InternalTestApi.downloadFileByTargetUrl(targetUrl)
				.then(() => {})
				.finally(() => {
					console.log('------end------');
					EventBus.$emit('hide_mask', null);
					this.dialogVisible.isShowExtra1 = false;
				});
		} else {
			EventBus.$emit('show_mask', null);
			AccountingInvoiceApi.getVworldInvoiceEachMerchantPromotionDetailWithAccordionFormat(myQuery)
				.then((res) => {
					this.queryVworldInvoiceEachMerchantDetailAccordionData = res.data;
					this.dialogVisible.isShowDetailInfo = true;
				})
				.finally(() => {
					EventBus.$emit('hide_mask', null);
				});
		}
	}

	showExtraFilterRulesDialog() {
		this.searchTargetRules = [];
		this.searchAndDisplayMode = 2;
		this.dialogVisible.isShowExtra1 = true;
	}

	selectDropdownByAggregativeGroup(aggregativeGroup: number) {
		const targetItems: any = this.queryAdjustmentRulesAndAssignGroup.aggregativeGroupList;
		const targetGroup = targetItems.filter((x: any) => x.aggregativeGroup === aggregativeGroup)[0];

		this.searchTargetRules = [];

		targetGroup.dropdownItems.forEach((element: any) => {
			this.searchTargetRules.push(element.valueInt);
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
			this.dropdownItemsInvoiceExportCategory = this.dropdownItemsInvoiceExportCategory.filter((x) => x.valueInt !== 999);
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
