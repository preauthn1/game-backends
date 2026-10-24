<template>
	<v-app v-if="renderComponent" class="app">
		<v-expansion-panels v-model="panel" multiple>
			<v-expansion-panel>
				<v-expansion-panel-content style="padding: 15px 5px">
					<v-flex>
						<v-row>
							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-text-field v-model="searchFilter.betRecordNoPGS" :label="'單號'" clearable> </v-text-field>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete v-model="searchFilter.merchantID" :items="merchantDropdownItems" :label="$t('TableHeader.__merchant')" clearable></v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete v-model="searchFilter.type" :items="typesDropdownItems" :label="$t('TableHeader.__type')" clearable></v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-btn color="yellow darken-1" text class="btn search" @click.stop="pageOneSearch()">
										<v-icon left>search</v-icon>
										{{ $t('Button.__search') }} </v-btn
									>&#9;

									<v-btn color="pink lighten-3" text class="btn search" @click.stop="clear">
										<v-icon left>clear</v-icon>
										{{ $t('Button.__reset') }}
									</v-btn>
								</div>
							</v-col>
						</v-row>
					</v-flex>
				</v-expansion-panel-content>
			</v-expansion-panel>
		</v-expansion-panels>

		<div>
			<div style="padding: 10px">
				<v-icon color="yellow darken-2" class="icon-m-size">error</v-icon>
				<span class="f12 yellow-text"> 本頁面只管理失敗重發。正常發送不會有此紀錄 </span>
			</div>
		</div>

		<v-data-table
			:items="queryRecords"
			:headers="headers"
			id="table"
			:disable-sort="true"
			:page="DataTablePagingParameter.pageIndex"
			:items-per-page="DataTablePagingParameter.itemsPerPage"
			:server-items-length="DataTablePagingParameter.totalCount"
			:footer-props="DataTablePagingParameter.footerProps"
			@update:page="changePage"
			@update:items-per-page="getItemPerPage"
		>
			<template v-slot:item.action="{ item }">
				<div class="operate_icons">
					<v-tooltip top>
						<template v-slot:activator="{ on, attrs }">
							<v-icon color="" dark v-bind="attrs" v-on="on" @click="ResetNotifyQueue(item.id)"> stars </v-icon>
						</template>
						<span>重製次數</span>
					</v-tooltip>

					<v-tooltip top>
						<template v-slot:activator="{ on, attrs }">
							<v-icon
								color=""
								dark
								v-bind="attrs"
								v-on="on"
								:disabled="!item.betRecordNoPGS"
								@click="openSingleWalletPlatformRequestLog(item.betRecordNoPGS)"
							>
								link
							</v-icon>
						</template>
						<span>查看單錢包商戶請求紀錄</span>
					</v-tooltip>
				</div>
			</template>

			<template v-slot:item.betRecordNoPGS="{ item }">
				<div class="">
					<div class="copyTextTo" @click="copyText(item.betRecordNoPGS)">
						{{ item.betRecordNoPGS }}
					</div>
				</div>
			</template>

			<template v-slot:item.statusText="{ item }">
				<span :class="'status_' + item.status">{{ item.statusText }}</span>
			</template>

			<template v-slot:item.merchantName="{ item }">
				<div>
					{{ item.merchantName }}
				</div>
				<div class="sub-title-text">
					{{ item.merchantID }}
				</div>
			</template>

			<template v-slot:item.type="{ item }">
				<div>{{ item.typeText }}</div>
				<div class="sub-title-text">{{ item.type }}</div>
			</template>

			<template v-slot:item.isSuccess="{ item }">
				<div v-if="!item.isSuccess">❌</div>
			</template>

			<template v-slot:item.retryTimes="{ item }">
				<span :class="{ 'red-text': item.retryTimes === 7 }">
					{{ item.retryTimes }}
				</span>
			</template>

			<template v-slot:item.createTime="{ item }">
				<span class="pointer" @click="globalPopupCopyDateTime(item.createTime)">
					{{ item.createTime | timeWithAddTimezone(userProfile.timeZone) }}
				</span>
			</template>

			<template v-slot:item.nextNotifyTime="{ item }">
				<span class="pointer" :class="{ 'next-notify-time-disabled': item.nextNotifyTime === '2999-01-01T00:00:00Z' }" @click="globalPopupCopyDateTime(item.nextNotifyTime)">
					{{ item.nextNotifyTime | timeWithAddTimezone(userProfile.timeZone) }}
				</span>
			</template>
		</v-data-table>

		<PopupCopyDatetime ref="refCopyDateTime"></PopupCopyDatetime>
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import { State, Action, Getter, namespace } from 'vuex-class';
const playersModule = namespace('Players');
import EventBus from '@/utilities/event-bus';
import { datetimeMixin, startEndDate, timeWithTimezone, daysAgo, weeksAgo, monthsAgo, formatMixin } from '@/utilities/datetime-format';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { commonMethodMixin } from '@/utilities/common-methods';
import * as Model from '@/models/interfaces/game-management/per-game';
import GameManagementApi from '@/api/game-management/per-game';
import DropdownApi, { convertLang } from '@/api/commons/dropdown';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import DatePicker from 'vue2-datepicker';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import * as Permission from '@/models/status/permission';
import PopupCopyDatetime from '@/components/PopupCopyDatetime.vue';
import NotifyQueueDetailPopup from '@/components/game-management/NotifyQueueDetailPopup.vue';

import axios, { AxiosResponse, AxiosRequestConfig } from 'axios';

const authModule = namespace('Auth');

// @ts-ignore
@Component({ mixins: [datetimeMixin, commonDisplayFiltersMixin, formatMixin, commonMethodMixin], components: { DatePicker, NotifyQueueDetailPopup, PopupCopyDatetime } })
export default class CallbackNotifyQueueList extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];
	@playersModule.State('transactionNoPGS') transactionNoPGS!: string;

	dialogVisible: DropdownModel.DialogVisible = new DropdownModel.DialogVisible();

	renderComponent: boolean = true;
	panel: number[] = [0];

	DataTablePagingParameter: DropdownModel.DataTablePagingParameter = new DropdownModel.DataTablePagingParameter();
	datetimeRangePicker: DropdownModel.DatetimeRangePicker = new DropdownModel.DatetimeRangePicker();
	searchFilter: Model.IBetRecordNotifyQueueRequestParameter = {} as Model.IBetRecordNotifyQueueRequestParameter;

	queryRecordsTotalSum: any = {};
	queryRecords: Model.IBetRecordNotifyQueuePageQuery[] = [];

	gamesDropdownItems: DropdownModel.IDropdownItems[] = [];
	merchantDropdownItems: DropdownModel.IDropdownItems[] = [];
	typesDropdownItems: DropdownModel.IDropdownItems[] = [];
	gameVendorDropdownItems: DropdownModel.IDropdownItems[] = [];
	tagsDropdownItems: DropdownModel.IDropdownItems[] = [];

	batchWritePerGameForm: any = {};
	batchRefundForm: any = {};

	findRecordConfirm: any = {};

	queryNotifyQueueInfo: Model.IBetRecordNotifyQueue = {} as Model.IBetRecordNotifyQueue;
	refundTarget: Model.IUnionBetRecordResponse = {} as Model.IUnionBetRecordResponse;

	@Action('Players/setTransactionNoPGS') private setTransactionNoPGS!: any;
	@Action('Players/setPlayerID') private setPlayerID!: any;

	get headers() {
		return [
			{ text: this.$t('TableHeader.__action'), value: 'action', width: '6%' },

			{ text: this.$t('TableHeader.__ID'), value: 'id', width: '6%' },
			{ text: this.$t('TableHeader.__type'), value: 'type', width: '6%' },

			{ text: this.$t('TableHeader.__betRecordNoPGS'), value: 'betRecordNoPGS', width: '12%' },
			{ text: this.$t('TableHeader.__merchant'), value: 'merchantName', width: '8%' },

			{ text: this.$t('TableHeader.__player'), value: 'playerID', width: '8%' },
			{ text: this.$t('TableHeader.__times') + '(重試)', value: 'retryTimes', align: 'center', width: '5%' },
			{ text: this.$t('Message.__success'), value: 'isSuccess', align: 'end', width: '6%' },

			{ text: this.$t('TableHeader.__time'), value: 'createTime', width: '8%' },
			{ text: '下次執行時間', value: 'nextNotifyTime', width: '8%' },
			{ text: '其他', value: 'debugInfo', width: '8%' },
		];
	}
	get searchTargetFieldItems() {
		return [
			{ text: 'PGS單號 (TransactionNo)', value: '1' },
			{ text: '三方遊戲商原廠單號', value: '2' },
			{ text: '遊戲局號', value: '3' },
		];
	}

	get hasDevelopeSuperPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.Special_AllowToUseDevelopeFeature) >= 0;
	}

	get hasAdminPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.GamesManagementAdminPermission) >= 0;
	}

	get hasPermissionManualRefund() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.GamesManagement_BetRecord_ManualRefund) >= 0;
	}

	get hasExportPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.GamesManagement_BetRecordSearch_Export) >= 0;
	}

	changePage(index: number = 1) {
		this.DataTablePagingParameter.pageIndex = index;
		if (this.queryRecords.length > 0) {
			this.search();
		}
	}

	getItemPerPage(val: number = 10) {
		this.DataTablePagingParameter.itemsPerPage = val;
		if (this.queryRecords.length > 0) {
			this.search();
		}
	}

	pageOneSearch() {
		this.DataTablePagingParameter.pageIndex = 1;
		this.search();
	}

	search() {
		const request = this.searchFilter;
		request.pageIndex = this.DataTablePagingParameter.pageIndex;
		request.pageSize = this.DataTablePagingParameter.itemsPerPage;
		request.orderByColumn = this.searchFilter.orderByColumn;
		request.isASC = this.searchFilter.isASC;

		const vm: any = this;

		EventBus.$emit('show_mask', null);

		GameManagementApi.getNotifyQueueList(<Model.IBetRecordNotifyQueueRequestParameter>request)
			.then((res) => {
				this.queryRecords = res.data.items;
				this.DataTablePagingParameter.totalCount = res.data.totalCount;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	ResetNotifyQueue(queueID: number) {
		if (confirm('確定要重置此通知佇列嗎？' + ' (ID:' + queueID + ' 重試次數將歸零)')) {
			EventBus.$emit('show_mask', null);

			GameManagementApi.ResetNotifyQueue(queueID)
				.then((res) => {
					EventBus.$emit(`successfully-modified`);
				})
				.finally(() => {
					EventBus.$emit('hide_mask', null);
				});
		}
	}

	openSingleWalletPlatformRequestLog(betRecordNoPGS: string | null) {
		if (!betRecordNoPGS) {
			return;
		}

		EventBus.$emit('close_tab', '/system-logs/SingleWalletPlatformReqeustLog');
		this.$router.push({
			path: '/system-logs/SingleWalletPlatformReqeustLog',
			query: { referenceTransactionNo: betRecordNoPGS },
		});
	}

	closeAllDialog() {
		this.dialogVisible.resetAllValueToFalse();
	}

	mounted() {
		this.init();
		this.registerReloadComponent();
	}

	copySuccess() {
		EventBus.$emit(`successfully-copy`);
	}

	init() {
		DropdownApi.getMerchants(false).then((res) => {
			this.merchantDropdownItems = res;
		});

		DropdownApi.getNotifyQueueType().then((res) => {
			this.typesDropdownItems = res;
		});

		DropdownApi.getSystemTags(3329).then((res) => {
			this.tagsDropdownItems = res;
		});

		this.clear();
		this.search();
	}

	copyText(text: string) {
		const vm: any = this;
		vm.copyTextToClipboard(text);
	}

	clear() {
		this.searchFilter = {} as Model.IBetRecordNotifyQueueRequestParameter;

		const vm: any = this;
		vm.globalClear();

		this.datetimeRangePicker.searchStartTimeRange = [daysAgo(-1), daysAgo(1)];
	}

	registerReloadComponent() {
		EventBus.$on(`reload-${this.$router.currentRoute.path}`, (info: any) => {
			this.renderComponent = false;
			this.$nextTick(() => {
				this.clear();
				this.queryRecords = [];
				this.renderComponent = true;
			});
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
	max-height: 20px !important;
	margin-top: 2px !important;
	margin-bottom: 3px !important;
	&.right {
		text-align: right;
	}
}
.search {
	max-width: 80px;
	margin-top: 15px !important;
}
.mx-datepicker {
	margin-top: 15px !important;
	width: 100% !important;
	width: 100% !important;
}

.container--fluid {
	margin-right: 5px !important;
}

.status_77,
.status_9 {
	color: #f27575;
}

.status_1 {
	color: #78b17c;
}

.next-notify-time-disabled {
	color: #666;
}

.common-info-value {
	min-height: 30px;
	line-height: 25px;
}

.notify-queue-info .col-12 {
	padding: 3px;
}

.notify-queue-info h3 {
	color: #71abb1;
}

.specialTag {
	text-overflow: clip;
	white-space: nowrap;
	overflow: hidden;
	max-width: 80px;
	display: inline-block;
	font-size: 12px;
	background: #4b71c5;
	border-radius: 10px;
	padding: 6px;
}
</style>
