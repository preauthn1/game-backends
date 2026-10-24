<template>

	<v-dialog v-model="isShowDialog" :max-width="(configMaxWidth.toString() + 'px')">

		<v-card>

			<v-card-title class="grey darken-2">
				{{displayName}}

				<span class="f12 yellow-text ml-4">
					{{extraInfo.extraMemo}}
				</span>

			</v-card-title>

			<v-card-text>

				<div>

					<div class="mb-4">

						<v-row>

							<v-col cols="12" sm="3" md="3" v-if="isShowFilterBeginEndTime">

								<div class="item">
									<date-picker v-model="datetimeRangePicker.searchStartTimeRange" type="datetime"
										value-type="timestamp" range :placeholder="$t('TableHeader.__time')">
									</date-picker>
								</div>

							</v-col>

							<v-col cols="12" sm="3" md="3" v-if="isShowFilterFishLogType">

								<div class="">
									<v-autocomplete :items="dropdownItemsFishLogTypes" v-model="searchFilter.type"
										clearable :label="'類型'">
									</v-autocomplete>
								</div>

							</v-col>

							<v-col cols="12" sm="3" md="3">

								<div class="item">
									<v-btn color="yellow darken-1" text class="btn search"
										@click.stop="searchPageOne">
										<v-icon left>search</v-icon>
										{{ $t('Button.__search') }}
									</v-btn>&#9;

								</div>

							</v-col>

						</v-row>

					</div>

					<div id="myDiv" style="height: 600px;overflow-y: scroll;">

						<v-data-table v-if="isShowTable" :headers="headers" id="table" :items="searchItems"
							:items-per-page="pagingItemsPerPage" :footer-props="pagingFooterProps"
							hide-default-footer>

							<template v-slot:item.MerchantProfit="{ item }">

								<span :class="item.MerchantProfit < 0 ? 'minus-red': ''">
									{{item.MerchantProfit | amountDisplay}}
								</span>

							</template>

							<template v-slot:item.WinLossAmount="{ item }">

								<span :class="isMinusValue(item.WinLossAmount) ? 'minus-red': ''">
									{{item.WinLossAmount}}
								</span>

							</template>

							<template v-slot:item.RTP="{ item }">

								<span :class="item.RTP >= 100 ? 'h-yellow': ''">
									{{item.RTP}}
								</span>

							</template>

							<template v-slot:item.Amount="{ item }">

								<span :class="isMinusValue(item.Amount) ? 'minus-red': ''">
									{{item.Amount}}
								</span>

							</template>

							<template v-slot:item.UpdateAmount="{ item }">

								<span :class="isMinusValue(item.UpdateAmount) ? 'minus-red': ''">
									{{item.UpdateAmount}}
								</span>

							</template>

						</v-data-table>

						<div style="padding-bottom:60px;" class="mt-4">
							<div class="text-c">

								<v-btn color="blue lighten-1" text class="btn search" @click.stop="searchNextPage"
									v-if="hasNextPageButton">
									<v-icon left>refresh</v-icon>
									载入更多...
								</v-btn>&#9;

								<v-btn color="grey lighten-1" text class="btn search" @click.stop="scrollToTop">
									<v-icon left>mdi-arrow-up</v-icon>
									回上層
								</v-btn>&#9;
							</div>
						</div>

					</div>

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
	isShowFilterFishLogType: boolean = false;
	isShowFilterBeginEndTime: boolean = false;

	hasNextPageButton: boolean = false;
	configMaxWidth: number = 0;

	headers: any = null;

	pagingFooterProps: any = { 'items-per-page-options': [10000] };
	pagingItemsPerPage: number = 10000;
	displayName: string = '';

	datetimeRangePicker: DropdownModel.DatetimeRangePicker = new DropdownModel.DatetimeRangePicker();
	dataTablePagingParameter: DropdownModel.DataTablePagingParameter = new DropdownModel.DataTablePagingParameter();
	searchDynamicQuerySetting: ResponseModel.IDynamicQueryCommandSettingResult = {} as ResponseModel.IDynamicQueryCommandSettingResult;
	queryDynamicQueryCategoryDropdown: ResponseModel.IDynamicQueryCategoryDropdowns = {} as ResponseModel.IDynamicQueryCategoryDropdowns;

	searchFilter: ResponseModel.ICustomizedColumnRequest = {} as ResponseModel.ICustomizedColumnRequest;
	searchItems: any = null; // 格式是動態的
	searchExtraInfo: any = null;
	extraInfo: any = {};

	dropdownItemsFishLogTypes: DropdownModel.IDropdownItems[] = [];

	panel: number[] = [0];

	isMinusValue(amountDisplayText: string) {
		try {
			const amount = amountDisplayText.replace(',', '');
			return parseFloat(amount) < 0;
		} catch (error) {}

		return false;
	}

	searchAndShowDialog(myFilter: ResponseModel.ICustomizedColumnRequest, config: any) {
		if (myFilter.querySqlID == null || myFilter.querySqlID <= 0) {
			alert('參數未填寫: querySqlID');
			return;
		}
		this.configMaxWidth = 1200;
		if (config && config != null) {
			this.configMaxWidth = config.maxWidth ? config.maxWidth : 1200;
			this.datetimeRangePicker.searchStartTimeRange = [daysAgo(-1), daysAgo(1)];
			this.datetimeRangePicker.searchStartTimeRange = [config.UI_beginTS, config.UI_endTS];

			if (config.UI_isShowFilterFishLogType && config.UI_isShowFilterFishLogType === true) {
				this.isShowFilterFishLogType = true;
			}

			if (config.UI_isShowFilterBeginEndTime && config.UI_isShowFilterBeginEndTime === true) {
				this.isShowFilterBeginEndTime = true;
			}
		}

		this.searchFilter = myFilter;
		this.searchPageOne();
	}

	scrollToTop() {
		const myDiv: any = document.getElementById('myDiv');
		myDiv.scrollTop = 0;
	}

	searchPageOne() {
		this.searchFilter.pageIndex = 1;

		let myPageSize = 100;

		if (this.searchFilter.pageSize && this.searchFilter.pageSize > 0) {
			myPageSize = this.searchFilter.pageSize;
		}

		this.searchFilter.pageSize = myPageSize;

		this.searchFilter.beginTime = this.datetimeRangePicker.searchStartTimeRange[0] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[0]) : null;
		this.searchFilter.endTime = this.datetimeRangePicker.searchStartTimeRange[1] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[1]) : null;

		EventBus.$emit('show_mask', null);
		internalTestApi
			.getCustomizedColumns(this.searchFilter)
			.then((res) => {
				this.isShowDialog = true;
				this.isShowTable = true;
				this.searchItems = res.data.items;
				this.searchExtraInfo = res.data.extraObject;
				this.displayName = res.data.extraDisplayName;
				this.extraInfo = res.data.extraInfo;

				const columnTranslateMapping = res.data.extraObject.columnTranslateMapping;

				// 因為程式段會處理掉不需要顯示的資料 實際上回應得筆數可能比較少 即使指定200筆 實際回傳可能只有198筆
				let offsetPagesize = this.searchFilter.pageSize - 5;

				if (offsetPagesize <= 5) {
					offsetPagesize = 5;
				}

				if (res.data.items && res.data.items.length >= offsetPagesize) {
					this.hasNextPageButton = true;
				} else {
					this.hasNextPageButton = false;
				}

				if (res.data.extraObject.columns == null) {
					res.data.extraObject.columns = [];
				}

				this.headers = res.data.extraObject.columns.map((item: any) => {
					const alignValue = this.getAlignByColumn(item.columnName);

					let labelText = item.columnName;

					if (columnTranslateMapping.hasOwnProperty(item.columnName)) {
						labelText = columnTranslateMapping[item.columnName];
					}

					return {
						text: labelText,
						value: item.columnName,
						sortable: false,
						align: alignValue,
					};
				});
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	searchNextPage() {
		EventBus.$emit('show_mask', null);

		this.searchFilter.pageIndex = this.searchFilter.pageIndex + 1;

		internalTestApi
			.getCustomizedColumns(this.searchFilter)
			.then((res) => {
				if (res.data.items && res.data.items.length > 0) {
					res.data.items.forEach((myItem) => {
						this.searchItems.push(myItem);
					});
					if (res.data.items.length >= this.searchFilter.pageSize) {
						this.hasNextPageButton = true;
					}
				}

				this.searchExtraInfo = res.data.extraObject;
				this.displayName = res.data.extraDisplayName;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	getAlignByColumn(columnName: string) {
		if (
			columnName.toLowerCase().includes('amount') ||
			columnName.toLowerCase().includes('count') ||
			columnName.toLowerCase().includes('rate') ||
			columnName.toLowerCase().includes('rtp') ||
			columnName.toLowerCase().includes('金額') ||
			columnName.toLowerCase().includes('數') ||
			columnName.toLowerCase().includes('金幣') ||
			columnName.toLowerCase().includes('餘額') ||
			columnName.toLowerCase().includes('profit')
		) {
			return 'end';
		}

		return 'start';
	}

	init() {
		this.datetimeRangePicker.searchStartTimeRange = [daysAgo(-1), daysAgo(1)];
	}

	mounted() {
		const urlParams = new URLSearchParams(window.location.search);
		const myParam = urlParams.get('querySqlID');
		this.init();

		DropdownApi.GetAGGFishLogTypes().then((res) => {
			this.dropdownItemsFishLogTypes = res;
		});
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

.item {
	max-height: 20px !important;
	margin-top: 15px !important;
	margin-bottom: 3px !important;
	&.right {
		text-align: right;
	}
}
</style>
