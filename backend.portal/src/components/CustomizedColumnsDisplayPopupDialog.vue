<template>
	<v-dialog v-model="isShowDialog" :max-width="configMaxWidth.toString() + 'px'">
		<v-card>
			<v-card-title class="grey darken-2">
				<span :title="displayType">
					{{ displayName }}
				</span>

				<span class="f12 grey-text ml-2">{{ extraInfo.extraMemo }}</span>

				<!-- 額外提示文案1(可複製) -->
				<v-tooltip top color="brown darken-2" v-if="!isEmptyString(extraInfo.icontHint1)">
					<template v-slot:activator="{ on, attrs }">
						<v-icon color="yellow" v-bind="attrs" v-on="on" small class="mr-2" @click="copyTextToClipboard(extraInfo.icontHint1)"> info </v-icon>
					</template>
					<span>
						<div style="width: 600px; white-space: pre-wrap">
							{{ extraInfo.icontHint1 }}
						</div>
					</span>
				</v-tooltip>

				<!-- 額外提示文案2(可複製) -->
				<v-tooltip top color="brown darken-2" v-if="!isEmptyString(extraInfo.icontHint2)">
					<template v-slot:activator="{ on, attrs }">
						<v-icon color="orange" v-bind="attrs" v-on="on" small class="mr-2" @click="copyTextToClipboard(extraInfo.icontHint2)"> info </v-icon>
					</template>
					<span>
						<div style="width: 600px; white-space: pre-wrap">
							{{ extraInfo.icontHint2 }}
						</div>
					</span>
				</v-tooltip>

				<v-spacer></v-spacer>

				<div class="top-right-item" @click="closeAllModal" v-if="isShowCloseButton">
					<v-btn icon dark color="yellow lighten-3">
						<v-icon>close</v-icon>
					</v-btn>
				</div>

				<div style="display: inline-block" :class="isShowCloseButton ? 'mr-4' : ''">
					<v-btn color="orange lighten-3" text :class="'btn search'" v-if="!isEmptyString(extraInfo.extraCopyInfo)" @click.stop="copyTextToClipboard(extraInfo.extraCopyInfo)">
						<v-icon left>cloud_download</v-icon>
						額外資訊複製 </v-btn
					>&#9;

					<div class="top-right-item" @click="close">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>

					<v-btn color="light-blue lighten-3" text :class="'btn search'" @click.stop="exportItems" style="margin-right: 30px">
						<v-icon left>cloud_download</v-icon>
						{{ $t('Button.__downloadFile') }}
					</v-btn>
				</div>
			</v-card-title>

			<v-card-text>
				<div class="customizedTable">
					<v-data-table
						v-if="isShowTable"
						:headers="headers"
						id="table"
						:items="searchItems"
						:items-per-page="pagingItemsPerPage"
						:footer-props="pagingFooterProps"
						:height="isFixedHeader ? '650px' : ''"
						:fixed-header="isFixedHeader"
						hide-default-footer
					>
						<!-- <template v-slot:item.GameTime="{ item }">

							<span @click="copyDuration_5Minutes(item.GameTime)" class='copyTextTo' title='copy'>
								{{item.GameTime}}
							</span>
						</template> -->

						<template v-slot:item.GameTime="{ item }">
							<span @click="copyDuration_5Minutes(item.GameTime)" class="copyTextTo" title="copy">
								{{ item.GameTime }}
							</span>
						</template>

						<template v-slot:item.MerchantProfit="{ item }">
							<span :class="item.MerchantProfit < 0 ? 'minus-red' : ''">
								{{ item.MerchantProfit | amountDisplay }}
							</span>
						</template>

						<template v-slot:item.TotalRound="{ item }">
							{{ item.TotalRound | countDisplay }}
						</template>

						<template v-slot:item.totalRound="{ item }">
							{{ item.totalRound | countDisplay }}
						</template>

						<template v-slot:item.totalCount="{ item }">
							{{ item.totalCount | countDisplay }}
						</template>

						<template v-slot:item.TotalCount="{ item }">
							{{ item.TotalCount | countDisplay }}
						</template>

						<template v-slot:item.TotalAmount="{ item }">
							{{ item.TotalAmount | amountDisplay }}
						</template>

						<template v-slot:item.totalAmount="{ item }">
							{{ item.totalAmount | amountDisplay }}
						</template>

						<template v-slot:item.avgBonusAmount="{ item }">
							{{ item.avgBonusAmount | amountDisplay }}
						</template>

						<template v-slot:item.Amount="{ item }">
							{{ item.Amount | amountDisplay }}
						</template>

						<template v-slot:item.amount="{ item }">
							{{ item.amount | amountDisplay }}
						</template>

						<template v-slot:item.BetAmount="{ item }">
							{{ item.BetAmount | amountDisplay }}
						</template>

						<template v-slot:item.betAmount="{ item }">
							{{ item.betAmount | amountDisplay }}
						</template>

						<template v-slot:item.PayoutAmount="{ item }">
							{{ item.PayoutAmount | amountDisplay }}
						</template>

						<template v-slot:item.payoutAmount="{ item }">
							{{ item.payoutAmount | amountDisplay }}
						</template>

						<template v-slot:item.ReceiveTime="{ item }">
							{{ item.ReceiveTime | timeWithAddTimezone(userProfile.timeZone) }}
						</template>

						<template v-slot:item.receiveTime="{ item }">
							{{ item.receiveTime | timeWithAddTimezone(userProfile.timeZone) }}
						</template>

						<template v-slot:item.merchantWinLoss="{ item }">
							<span :class="isMinusValue(item.merchantWinLoss) ? 'minus-red' : ''">
								{{ item.merchantWinLoss | amountDisplay }}
							</span>
						</template>

						<template v-slot:item.MerchantWinLoss="{ item }">
							<span :class="isMinusValue(item.MerchantWinLoss) ? 'minus-red' : ''">
								{{ item.MerchantWinLoss | amountDisplay }}
							</span>
						</template>

						<template v-slot:item.merchantWinLossAmount="{ item }">
							<span :class="isMinusValue(item.merchantWinLossAmount) ? 'minus-red' : ''">
								{{ item.merchantWinLossAmount | amountDisplay }}
							</span>
						</template>

						<template v-slot:item.WinLossAmount2="{ item }">
							<span :class="isMinusValue(item.WinLossAmount2) ? 'minus-red' : ''">
								{{ item.WinLossAmount2 | amountDisplay }}
							</span>
						</template>

						<template v-slot:item.WinLossAmount="{ item }">
							<span :class="isMinusValue(item.WinLossAmount) ? 'minus-red' : ''">
								{{ item.WinLossAmount | amountDisplay }}
							</span>
						</template>

						<template v-slot:item.winLossAmount="{ item }">
							<span :class="isMinusValue(item.winLossAmount) ? 'minus-red' : ''">
								{{ item.winLossAmount | amountDisplay }}
							</span>
						</template>

						<template v-slot:item.DiffAmount="{ item }">
							<span :class="isMinusValue(item.DiffAmount) ? 'minus-red' : ''">
								{{ item.DiffAmount | amountDisplay }}
							</span>
						</template>

						<template v-slot:item.diffAmount="{ item }">
							<span :class="isMinusValue(item.diffAmount) ? 'minus-red' : ''">
								{{ item.diffAmount | amountDisplay }}
							</span>
						</template>

						<template v-slot:item.diffPercentage="{ item }">
							<span :class="isMinusValue(item.diffPercentage) ? 'minus-red' : ''"> {{ item.diffPercentage | amountDisplay }} % </span>
						</template>

						<template v-slot:item.RTP="{ item }">
							<span :class="item.RTP >= 100 ? 'h-yellow' : ''">
								{{ item.RTP }}
							</span>
						</template>

						<template v-slot:item.rTP="{ item }">
							<span :class="item.rTP >= 100 ? 'h-yellow' : ''">
								{{ item.rTP }}
							</span>
						</template>

						<template v-slot:item.Value="{ item }">
							<span class="pointer" @click="copyTextToClipboard(item.Value)"> {{ item.Value }} </span>
						</template>

						<template v-slot:item.rtp="{ item }">
							<span :class="item.rtp >= 100 ? 'h-yellow' : ''">
								{{ item.rtp }}
							</span>
						</template>

						<template v-slot:item.UpdateAmount="{ item }">
							<span :class="isMinusValue(item.UpdateAmount) ? 'minus-red' : ''">
								{{ item.UpdateAmount }}
							</span>
						</template>

						<template v-slot:item.fileDonwloadUrl="{ item }">
							<div class="link-text" @click="downloadTargetUrl(item.fileDonwloadUrl)">檔案下載</div>
						</template>
					</v-data-table>
				</div>
			</v-card-text>
		</v-card>
	</v-dialog>
</template>

<script lang="ts">
import Vue from 'vue';
import EventBus from '@/utilities/event-bus';
import { Component, Prop } from 'vue-property-decorator';
import { State, Action, Getter, namespace } from 'vuex-class';
import { Locale } from '@/models/status/common';
import DropdownApi from '@/api/commons/dropdown';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import DatePicker from 'vue2-datepicker';
import * as ResponseModel from '@/models/interfaces/commons/response';
import { datetimeMixin, startEndDate, timeWithTimezone, daysAgo, weeksAgo, monthsAgo, formatMixin, addMinutes } from '@/utilities/datetime-format';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { commonMethodMixin } from '@/utilities/common-methods';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import internalTestApi from '@/api/commons/internalTest';
import * as Permission from '@/models/status/permission';
import { read } from 'xlsx';
const authModule = namespace('Auth');

// @ts-ignore
@Component({
	// @ts-ignore
	mixins: [datetimeMixin, commonDisplayFiltersMixin, formatMixin, commonMethodMixin],
	components: { DatePicker },
})
export default class CustomizedColumnsDisplayPopupDialog extends Vue {
	@Prop(String) readonly defaultLang!: string;

	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;

	isShowDialog: boolean = false;
	isShowTable: boolean = false;
	isShowCloseButton: boolean = false;
	isFixedHeader: boolean = true;

	configMaxWidth: number = 800;

	headers: any = null;
	copyText: string = '';

	pagingFooterProps: any = { 'items-per-page-options': [50, 100, 500] };
	pagingItemsPerPage: number = 500;
	displayName: string = '';
	displayType: number = 0;
	extraMemo: string = '';
	extraInfo: any = {};

	datetimeRangePicker: DropdownModel.DatetimeRangePicker = new DropdownModel.DatetimeRangePicker();
	dataTablePagingParameter: DropdownModel.DataTablePagingParameter = new DropdownModel.DataTablePagingParameter();
	searchDynamicQuerySetting: ResponseModel.IDynamicQueryCommandSettingResult = {} as ResponseModel.IDynamicQueryCommandSettingResult;
	queryDynamicQueryCategoryDropdown: ResponseModel.IDynamicQueryCategoryDropdowns = {} as ResponseModel.IDynamicQueryCategoryDropdowns;

	searchItems: any = null; // 格式是動態的
	searchExtraInfo: any = null;
	panel: number[] = [0];

	copyDuration_5Minutes(time: string) {
		const vm: any = this;

		const beforeMinutes5 = addMinutes(time, -5, this.userProfile.timeZone);
		const afterMinutes5 = addMinutes(time, 5, this.userProfile.timeZone);

		vm.copyTextToClipboard(beforeMinutes5 + ' ~ ' + afterMinutes5);
	}

	isMinusValue(amountDisplayText: string) {
		try {
			const amount = amountDisplayText.toString().replace(',', '');
			return parseInt(amount, 10) < 0;
		} catch (error) {
			console.log(error);
		}

		return false;
	}

	searchAndShowDialog(myFilter: ResponseModel.ICustomizedColumnRequest, config: any) {
		if (myFilter.querySqlID == null || myFilter.querySqlID <= 0) {
			alert('參數未填寫: querySqlID');
			return;
		}

		// 針對UI控制的額外config參數
		if (config && config != null) {
			this.configMaxWidth = config.maxWidth ? config.maxWidth : 800;
			this.isShowCloseButton = config.isShowCloseButton ? true : false;

			if (config.isFixedHeader === false) {
				this.isFixedHeader = false;
			}
		}

		EventBus.$emit('show_mask', null);
		internalTestApi
			.getCustomizedColumns(myFilter)
			.then((res) => {
				this.isShowDialog = true;
				this.isShowTable = true;
				this.searchItems = res.data.items;
				this.searchExtraInfo = res.data.extraObject;
				this.copyText = res.data.extraObject.copyExcel;
				this.displayName = res.data.extraDisplayName;
				this.displayType = res.data.extraDisplayType;
				this.extraInfo = res.data.extraInfo;

				const columnTranslateMapping = res.data.extraObject.columnTranslateMapping;

				this.headers = res.data.extraObject.columns.map((item: any) => {
					const alignValue = this.getAlignByColumn(item.columnName);

					let labelText = item.columnName;

					if (columnTranslateMapping.hasOwnProperty(item.columnName)) {
						labelText = columnTranslateMapping[item.columnName];
					}

					return {
						text: labelText,
						value: item.columnName,
						sortable: this.isAmountColumn(item.columnName) || this.isAllowSortColumn(item.columnName),
						align: alignValue,
					};
				});
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	setDisplayInfoAndShowDialog(displayInfoObject: any, myConfigMaxWidth: number = 1200) {
		this.isShowDialog = true;
		this.isShowTable = true;
		this.searchItems = displayInfoObject.items;
		this.searchExtraInfo = displayInfoObject.extraObject;
		this.copyText = displayInfoObject.copyExcel;
		this.displayName = displayInfoObject.extraDisplayName;
		this.extraInfo = displayInfoObject.extraInfo;
		this.configMaxWidth = myConfigMaxWidth;

		console.log(displayInfoObject);

		const columnTranslateMapping = displayInfoObject.extraObject.columnTranslateMapping;

		this.headers = displayInfoObject.extraObject.columns.map((item: any) => {
			const alignValue = this.getAlignByColumn(item.columnName);

			let labelText = item.columnName;

			if (columnTranslateMapping.hasOwnProperty(item.columnName)) {
				labelText = columnTranslateMapping[item.columnName];
			}

			return {
				text: labelText,
				value: item.columnName,
				sortable: this.isAmountColumn(item.columnName) || this.isAllowSortColumn(item.columnName),
				align: alignValue,
			};
		});
	}

	getAlignByColumn(columnName: string) {
		if (this.isAmountColumn(columnName)) {
			return 'end';
		}

		return 'start';
	}

	isAllowSortColumn(columnName: string) {
		if (columnName.toLowerCase() === 'gameid' || columnName.toLowerCase() === 'gamecode') {
			return true;
		}

		return false;
	}

	downloadTargetUrl(url: string) {
		const targetUrl = process.env.VUE_APP_API_RAW + url;

		const params = new URLSearchParams(url.replaceAll('internal/backend-buttons/action?', ''));

		// 將其轉成普通物件
		const parseObj = Object.fromEntries(params.entries());

		const finalBody: any = {
			BackendButtonActionID: parseObj.BackendButtonActionID,
			extraParameter: parseObj,
		};

		EventBus.$emit('show_mask', null);

		internalTestApi.downloadFileByTargetUrlAndBody(targetUrl, finalBody).then(() => {
			EventBus.$emit('hide_mask', null);
			console.log('download file success');
		});
	}

	isAmountColumn(columnName: string) {
		if (
			columnName.toLowerCase().includes('amount') ||
			columnName.toLowerCase().includes('winloss') ||
			columnName.toLowerCase().includes('rate') ||
			columnName.toLowerCase().includes('round') ||
			columnName.toLowerCase().includes('count') ||
			columnName.toLowerCase().includes('rtp') ||
			columnName.toLowerCase().includes('金額') ||
			columnName.toLowerCase().includes('percentage') ||
			columnName.toLowerCase().includes('數') ||
			columnName.toLowerCase().includes('金幣') ||
			columnName.toLowerCase().includes('餘額') ||
			columnName.toLowerCase().includes('profit')
		) {
			return true;
		}

		return false;
	}

	exportItems() {
		const excelData: any[] = [];
		const excelHeaders: string[] = [];

		this.headers.forEach((item: any) => {
			excelHeaders.push(item.text);
		});

		this.searchItems.forEach((eachItem: any) => {
			const eachRow: any = [];

			this.headers.forEach((item: any) => {
				eachRow.push(eachItem[item.value]);
			});

			excelData.push(eachRow);
		});

		let suffixFileName: any = this.displayName;
		suffixFileName = suffixFileName.replaceAll(' ', '').replaceAll('（', '').replaceAll('）', '').replaceAll('(', '').replaceAll(')', '');

		const config = {
			headers: excelHeaders,
			rowData: excelData,
			fileName: 'Export_' + suffixFileName + '.xlsx',
		};

		// @ts-ignore
		this.downloadExcel(config);
	}

	closeAllModal() {
		this.isShowDialog = false;
	}

	close() {
		this.isShowDialog = false;
	}

	init() {
		this.datetimeRangePicker.searchStartTimeRange = [daysAgo(-1), daysAgo(1)];
	}

	mounted() {
		const urlParams = new URLSearchParams(window.location.search);
		const myParam = urlParams.get('querySqlID');
		this.init();
	}
}
</script>
<style lang="scss" scope>
span.white--text.headline {
	font-family: 微軟正黑體, arial !important;
}
.country {
	background-color: transparent !important;
	box-shadow: none !important;
}
.lang {
	white-space: nowrap !important;
}

table th {
	user-select: initial !important;
}

.col-sm-3 .buttonRow,
.col-md-3 .buttonRow {
	margin-top: 15px;
}

.hintRow,
.hintRow .v-icon {
	padding: 3px 0px;
	font-size: 14px;
	color: #aaa;
}

.hintRow > div {
	display: inline-block;
	margin-right: 15px;
}

.hintRow .v-icon {
	color: #aaa;
	font-size: 20px;
}

.link-sm {
	font-size: 16px;
	margin-right: 20px;
	display: inline-block;
}

.no-margin-padding {
	padding: 3px;
	margin: 0px;
}

.customizedTable {
	min-height: 300px;
}

.customizedTable tbody {
	max-width: 90vw !important;
}

@media only screen and (max-width: 480px) {
	.v-dialog {
		position: fixed;
		margin: 0px;
	}
}
</style>
