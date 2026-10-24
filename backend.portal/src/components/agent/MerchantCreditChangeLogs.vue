<template>
	<v-app v-if="isRenderComponent" class="app">

		<v-expansion-panels v-model="panel" multiple>
			<v-expansion-panel>

				<v-expansion-panel-content>
					<v-flex>
						<v-row class="search-form">

							<v-col cols="12" sm="3" md="3">
								<div class="">

									<v-autocomplete clearable :items="dropdownItemsMerchants"
										v-model="searchFilter.merchantID" :label="$t('TableHeader.__merchant')">
									</v-autocomplete>

								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">

								<v-autocomplete clearable :items="dropdownItemsCreditChangeTypes"
									v-model="searchFilter.type" :label="$t('TableHeader.__type')">
								</v-autocomplete>

							</v-col>

							<v-col cols="12" sm="3" md="3">

								<v-text-field v-model="searchFilter.referenceOrderNo"
									:label="$t('TableHeader.__referenceNo')">
								</v-text-field>

							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item search-row-padding">
									<v-btn color="yellow darken-1" text class="btn search" @click.stop="search">
										<v-icon left>search</v-icon>
										{{$t('Button.__search')}}
									</v-btn>&#9;
									<v-btn color="pink lighten-3" text class="btn search" @click.stop="clear">
										<v-icon left>clear</v-icon>
										{{$t('Button.__reset')}}
									</v-btn>&#9;
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="minus-mt-3">
									<date-picker v-model="datetimeRangePicker.searchStartTimeRange" type="datetime"
										v-bind:clearable="false" value-type="timestamp" range
										:placeholder="$t('TableHeader.__time')">
									</date-picker>
								</div>
							</v-col>

							<v-col cols="12" sm="6" md="6" class="quick-time">
								<!-- <div class="item"> -->
								<v-chip class="mr-2"
									:class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText  === 'today' }"
									@click="globalQuicklyPickTimeRange('today')">
									<v-icon left>mdi-alarm-check</v-icon>
									{{ $t(`TableHeader.__today`) }}
								</v-chip>
								<v-chip class="mr-2"
									:class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText  === 'yesterday' }"
									@click="globalQuicklyPickTimeRange('yesterday')">
									<v-icon left>mdi-alarm-check</v-icon>
									{{ $t(`TableHeader.__yesterday`) }}
								</v-chip>
								<v-chip class="mr-2"
									:class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText  === 'thisWeek' }"
									@click="globalQuicklyPickTimeRange('thisWeek')">
									<v-icon left>mdi-alarm-check</v-icon>
									{{ $t(`TableHeader.__thisWeek`) }}
								</v-chip>

								<v-chip class="mr-2"
									:class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText  === 'lastWeek' }"
									@click="globalQuicklyPickTimeRange('lastWeek')">
									<v-icon left>mdi-alarm-check</v-icon>
									{{ $t(`TableHeader.__lastWeek`) }}
								</v-chip>

							</v-col>

						</v-row>
					</v-flex>
				</v-expansion-panel-content>
			</v-expansion-panel>
		</v-expansion-panels>

		<div class=" mt-2 f14 ">
			<v-icon class="" small>error</v-icon>
			<span>{{ $t('Message.__dataKeepXMonthOnly' , { month : 2}) }}</span>
		</div>

		<v-data-table :headers="headers" id="table" :items="searchList" class="mt-2"
			:page="DataTablePagingParameter.pageIndex"
			:items-per-page="DataTablePagingParameter.itemsPerPage"
			:server-items-length="DataTablePagingParameter.totalCount"
			:footer-props="DataTablePagingParameter.footerProps" @update:page="changePage"
			@update:items-per-page="changeItemPerPage">

			<template v-slot:item.action="{ item }">

				<!-- <v-menu offset-y>

					<template v-slot:activator="{ on, attrs }">

						<div class="dropdown-menu" v-bind="attrs" v-on="on">
							<v-icon dark outline>
								expand_more
							</v-icon>
						</div>

					</template>
					<v-list class="dropdonwMenuPanel">
						<v-list-item class="dropdownMenu">

							<v-list-item @click="showInfo(item.eventID)">

								<v-icon>
									edit
								</v-icon>
								{{$t("Button.__edit")}}
							</v-list-item>

							<v-list-item @click="redirectToPrizeRecord(item)">

								<v-icon>
									assignment
								</v-icon>
								{{$t("Button.__view")}} {{$t("Menu.SystemSetting_EventPrizeRecord")  }}

							</v-list-item>

							<v-list-item @click="showEditEventContentBlock(item.eventID)">

								<v-icon>
									settings
								</v-icon>
								編輯活動說明

							</v-list-item>

							<v-list-item @click="showDeleteHint(item)">
								<v-icon>
									delete
								</v-icon>
								{{$t("Button.__delete")}}
							</v-list-item>

							<v-list-item @click="showResetRTPHint(item)" v-if="item.eventType == 51">
								<v-icon>
									stars
								</v-icon>
								{{$t("Button.__reset")}} RTP
							</v-list-item>

							<v-list-item @click="getRankEventPreview(item.eventID)" v-if="item.eventType == 81">
								<v-icon>
									visibility
								</v-icon>
								查看排名
							</v-list-item>

						</v-list-item>
					</v-list>
				</v-menu> -->

			</template>

			<template v-slot:item.displayName="{ item }">

				<div class="double-row-td">
					<div :title="item.eventID">
						{{item.displayName}}
					</div>

					<div style="color:#999;font-size:12px;line-height: 10px;">
						{{item.eventID}}
					</div>
				</div>
			</template>

			<template v-slot:item.amount="{ item }">

				<span :class="item.amount < 0 ? 'minus-red': ''">
					{{ item.amount | amountDisplay }}
				</span>
			</template>

			<template v-slot:item.beforeAmount="{ item }">
				{{ item.beforeAmount | amountDisplay }}
			</template>

			<template v-slot:item.afterAmount="{ item }">
				{{ item.afterAmount | amountDisplay }}
			</template>

			<template v-slot:item.createTime="{ item }">
				{{ item.createTime | timeWithAddTimezone(userProfile.timeZone) }}
			</template>

		</v-data-table>

	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import * as Model from '@/models/interfaces/system-setting/merchants';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import * as EventModel from '@/models/interfaces/system-setting/events';
import * as MerchantCreditModel from '@/models/interfaces/agent/merchantCredit';

import MerchantsApi from '@/api/system-setting/merchants';
import EventsApi from '@/api/system-setting/events';
import MerchantCreditApi from '@/api/agent/merchantCredit';
import DropdownApi from '@/api/commons/dropdown';
import DatePicker from 'vue2-datepicker';

import { Locale, Currency, Status, GameVendorType } from '@/models/status/common';

import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { commonMethodMixin } from '@/utilities/common-methods';
import NotificationWarning from '@/components/notification/NotificationWarning.vue';
import { pageData, time } from '@/utilities/api-handler.lib';
import { datetimeMixin, startEndDate, timeWithTimezone, daysAgo, weeksAgo, monthsAgo, formatMixin } from '@/utilities/datetime-format';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import { State, Action, Getter, namespace } from 'vuex-class';
const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';
import { uuid } from 'vue-uuid';

// @ts-ignore
@Component({ components: { NotificationWarning, DatePicker }, mixins: [commonDisplayFiltersMixin, commonMethodMixin, datetimeMixin] })
export default class MerchantCredits extends Vue {
	get hasPermissionCreate() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSettingMerchant_Create) >= 0;
	}
	get hasPermissionUpdate() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSettingMerchant_Update) >= 0;
	}
	get hasPermissionDelete() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSettingMerchant_Delete) >= 0;
	}

	get headers() {
		const data = [
			{ text: this.$t('TableHeader.__action'), value: 'action', width: '4%', sortable: false, align: 'center' },
			{ text: this.$t('TableHeader.__createTime'), value: 'createTime', sortable: false },
			{ text: this.$t('TableHeader.__ID'), value: 'id', sortable: false, width: '7%' },
			{ text: this.$t('TableHeader.__referenceNo'), value: 'referenceOrderNo', sortable: false, width: '7%' },
			{ text: this.$t('TableHeader.__merchantID'), value: 'merchantID', sortable: false, width: '7%' },
			{ text: this.$t('TableHeader.__merchantName'), value: 'merchantName', sortable: false, width: '10%' },
			{ text: this.$t('TableHeader.__type'), value: 'typeText', sortable: false, width: '7%' },
			{ text: this.$t('TableHeader.__memo'), value: 'memo', sortable: false },
			{ text: this.$t('TableHeader.__updateAmount'), value: 'amount', sortable: false, align: 'end' },
			{ text: this.$t('TableHeader.__beforeValue'), value: 'beforeAmount', sortable: false, align: 'end' },
			{ text: this.$t('TableHeader.__afterValue'), value: 'afterAmount', sortable: false, align: 'end' },
			{ text: this.$t('TableHeader.__updateBy'), value: 'createBy', sortable: false },
			{ text: this.$t('TableHeader.__iPAddress'), value: 'ipAddress', sortable: false },
		];

		return data;
	}

	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];

	isRenderComponent: boolean = true;

	DataTablePagingParameter: DropdownModel.DataTablePagingParameter = new DropdownModel.DataTablePagingParameter();
	datetimeRangePicker: DropdownModel.DatetimeRangePicker = new DropdownModel.DatetimeRangePicker();
	searchList: MerchantCreditModel.IMerchantCreditChangeLog[] = [];
	searchFilter: MerchantCreditModel.IQueryMerchantCreditChangeLogApiRequestParameter = {} as MerchantCreditModel.IQueryMerchantCreditChangeLogApiRequestParameter;

	panel: number[] = [0];

	dropdownItemsCreditChangeTypes: DropdownModel.IDropdownItems[] = [];
	dropdownItemsMerchants: DropdownModel.IDropdownItems[] = [];

	changePage(index: number = 1) {
		this.DataTablePagingParameter.pageIndex = index;
		this.refresh();
	}

	changeItemPerPage(val: number = 10) {
		this.DataTablePagingParameter.itemsPerPage = val;
		this.refresh();
	}

	search() {
		this.DataTablePagingParameter.pageIndex = 1;
		this.refresh();
	}

	refresh() {
		this.registerReloadComponent();
		this.searchFilter.pageIndex = this.DataTablePagingParameter.pageIndex;
		this.searchFilter.pageSize = this.DataTablePagingParameter.itemsPerPage;

		this.searchFilter.createTimeBegin = this.datetimeRangePicker.searchStartTimeRange[0] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[0]) : null;
		this.searchFilter.createTimeEnd = this.datetimeRangePicker.searchStartTimeRange[1] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[1]) : null;
		this.searchFilter.isASC = false;

		EventBus.$emit('show_mask', null);

		MerchantCreditApi.getChangeLogs(this.searchFilter)
			.then((res) => {
				this.searchList = res.data.items;
				this.DataTablePagingParameter.totalCount = res.data.totalCount;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	closeAllModal() {}

	clear() {
		this.searchFilter = {} as MerchantCreditModel.IQueryMerchantCreditChangeLogApiRequestParameter;
	}

	copySuccess() {
		EventBus.$emit(`successfully-copy`);
	}

	registerReloadComponent() {
		EventBus.$on(`reload-${this.$router.currentRoute.path}`, (info: any) => {
			this.isRenderComponent = false;
			this.$nextTick(() => {
				this.isRenderComponent = true;
			});
		});
	}

	mounted() {
		this.datetimeRangePicker.searchStartTimeRange = [daysAgo(-3), daysAgo(1)];
		this.refresh();

		DropdownApi.getMerchants().then((res) => {
			this.dropdownItemsMerchants = res;
		});

		DropdownApi.getMerchantCreditChangeType().then((res) => {
			this.dropdownItemsCreditChangeTypes = res;
		});
	}
}
</script>



<style scoped>
.advancePanelContent {
	min-height: 250px;
	padding-bottom: 75px !important;
}

.reviewMerchant {
	padding: 1px;
}

.reviewMerchant > div {
	padding: 3px;
}

.reviewMerchant .label-text {
	color: #ccc;
	width: 175px;
	display: inline-block;
}

.hint-warning {
	color: #ef4c4c;
	font-size: 14px;
}

.hint-warning i {
	color: #ef4c4c !important;
	font-size: 16px !important;
}

.eventType_32 {
	/* 紅包 */
	background: #863f3f !important;
}

.eventType_51 {
	/* 甜蜜期 (顏色太多反而炫目) */
	/* background: #9c719e !important; */
}
</style>