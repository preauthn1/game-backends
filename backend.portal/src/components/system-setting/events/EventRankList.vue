<template>

	<v-card>

		<v-card-title class="grey darken-2">

			派發排名獎勵

			<span class="f14 grey-text ml-2">
				{{ rankEventPreview.eventName }} ({{ rankEventPreview.eventID }})
			</span>

			<span class="f12 grey-text ml-4">
				<v-icon color="grey" small>mdi-clock</v-icon>
				{{ rankEventPreview.beginTimeText }} - {{ rankEventPreview.endTimeText }}
			</span>

			<div class="top-right-item" @click="closeAllModal">
				<v-btn icon dark>
					<v-icon>close</v-icon>
				</v-btn>
			</div>
		</v-card-title>

		<v-card-text>

			<div style="padding:20px;">

				<div class="text-c mb-4">
					<v-chip color="red darken-4" v-if="rankEventPreview.isPrizeEmpty">此活動獎品已派發</v-chip>
				</div>

				<div class="text-c">

					<v-btn color="blue lighten-2" text class="btn search mr-2" @click.stop="refreshData()">
						<v-icon left>refresh</v-icon>
						重整刷新數據
					</v-btn>&#9;

					<v-btn v-if="!isAllowManualSendPrize" disabled='disabled'>
						禁止手動派發獎勵 (可能已設置排程自動發送)
					</v-btn>

					<v-btn v-if="isAllowManualSendPrize" color="yellow darken-1" text
						@click="triggerRankEventSendPrize(rankEventPreview.eventID)" class="btn"
						:disabled='rankEventPreview.isPrizeEmpty'>
						派發獎勵 (排名方式:{{currentSelectedOrderBy.text}})
					</v-btn>

				</div>

				<div class="text-c yellow-text"
					v-if="rankEventPreview.hintMemo != null && rankEventPreview.hintMemo != ''">
					<v-icon small color="orange">error</v-icon> {{rankEventPreview.hintMemo}}
				</div>

				<div class="text-c">

					<span style="font-size:10px;color:#888;" class="mr-2">
						排序方式
					</span>

					<div style="width:250px;display: inline-block;">
						<v-autocomplete :items="dropdownItemsEventCounterOrderBy" v-model="orderBy" chips
							@change="orderByValueChanged()">
						</v-autocomplete>
					</div>

				</div>

			</div>

			<v-flex style="padding:10px;">

				<div class="text-r">
					<v-btn color="pink lighten-3" text class="btn search mr-2"
						@click.stop="copyItemsToExcelFormat()">
						<v-icon left>file_copy</v-icon>
						下載名單(excel格式)
					</v-btn>&#9;

				</div>

				<v-data-table id="rankTable" :headers="headersRankPreivew" :items="rankEventPreview.players"
					:page="DataTablePagingParameterSize500.pageIndex"
					:items-per-page="DataTablePagingParameterSize500.itemsPerPage"
					:footer-props="DataTablePagingParameterSize500.footerProps">

					<template v-slot:item.totalRound="{ item }">
						{{item.totalRound | countDisplay}}
					</template>

					<template v-slot:item.totalBetAmount="{ item }">
						{{item.totalBetAmount | amountDisplay}}
					</template>

					<template v-slot:item.totalPayoutAmount="{ item }">
						{{item.totalPayoutAmount | amountDisplay}}
					</template>

					<template v-slot:item.totalWinLoss="{ item }">
						{{item.totalWinLoss | amountDisplay}}
					</template>

				</v-data-table>

			</v-flex>
		</v-card-text>

	</v-card>

</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import * as Model from '@/models/interfaces/system-setting/merchants';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import * as EventModel from '@/models/interfaces/system-setting/events';
import MerchantsApi from '@/api/system-setting/merchants';
import EventsApi from '@/api/system-setting/events';
import DropdownApi from '@/api/commons/dropdown';
import DatePicker from 'vue2-datepicker';
import { Locale, Currency, Status, GameVendorType } from '@/models/status/common';
import EventDetail from '@/components/system-setting/events/EventDetail.vue';

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
@Component({ components: { NotificationWarning, DatePicker, EventDetail }, mixins: [commonDisplayFiltersMixin, commonMethodMixin, datetimeMixin] })
export default class EventList extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];

	orderBy: number = -1;
	currentEventID: number = 0;
	dropdownItemsEventCounterOrderBy: DropdownModel.IDropdownItems[] = [];

	get currentSelectedOrderBy() {
		if (!this.dropdownItemsEventCounterOrderBy || this.dropdownItemsEventCounterOrderBy.length <= 0) {
			return { text: '', extraProperty: {} };
		}

		return this.dropdownItemsEventCounterOrderBy.filter((x) => x.valueInt === this.orderBy)[0];
	}

	get hasPermissionUpdate() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSettingMerchant_Update) >= 0;
	}

	get hasPermissionManualSendPrize() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSetting_Event_ManualSendPrize) >= 0;
	}

	get headersRankPreivew() {
		const data = [
			{ text: '順序', value: 'sequence', width: '8%', sortable: false, align: 'center' },
			{ text: this.$t('TableHeader.__playerID'), value: 'playerID', width: '8%', sortable: false, align: 'center' },
			{ text: this.$t('TableHeader.__merchant'), value: 'merchantID', width: '8%', sortable: false, align: 'center' },
			{ text: this.$t('TableHeader.__playerAccount'), value: 'playerName', sortable: false, width: '8%' },
			{ text: this.$t('TableHeader.__totalRound'), value: 'totalRound', sortable: false, align: 'end' },
			{ text: this.$t('TableHeader.__betAmount'), value: 'totalBetAmount', sortable: false, align: 'end' },
			{ text: this.$t('TableHeader.__payoutAmount'), value: 'totalPayoutAmount', sortable: false, align: 'end' },
			{ text: this.$t('TableHeader.__playerWinLoss'), value: 'totalWinLoss', sortable: false, align: 'end' },
			{ text: '最大派獎倍數', value: 'maxPayoutRate', sortable: false, align: 'end' },
		];

		return data;
	}

	get isAllowManualSendPrize() {
		return !this.rankEventPreview.isJobExist && this.hasPermissionManualSendPrize;
	}

	DataTablePagingParameterSize500: DropdownModel.DataTablePagingParameterSize500 = new DropdownModel.DataTablePagingParameterSize500();
	rankEventPreview: EventModel.IEventRankPreview = {} as EventModel.IEventRankPreview;

	refreshData() {
		const filter = {} as EventModel.IEventRankPreviewRequest;
		filter.eventID = this.currentEventID;
		filter.orderByType = this.orderBy;

		EventBus.$emit('show_mask', null);

		EventsApi.getRankEventPreview(filter)
			.then((res) => {
				this.rankEventPreview = res.data;

				// 預設值才assign
				if (this.orderBy <= -1) {
					this.orderBy = res.data.defaultCompetiteType;
				}
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	copyItemsToExcelFormat() {
		const vm: any = this;

		const excelHeaders: any = [];

		for (const item of this.headersRankPreivew) {
			excelHeaders.push(item.text);
		}

		const excelData: any = [];

		for (const item of this.rankEventPreview.players) {
			const myItem: any = item;

			const eachRow: any = [];
			eachRow.push(myItem.sequence);
			eachRow.push(myItem.playerID);
			eachRow.push(myItem.merchantID);
			eachRow.push(myItem.playerName);
			eachRow.push(myItem.totalRound);
			eachRow.push(myItem.totalBetAmount);
			eachRow.push(myItem.totalPayoutAmount);
			eachRow.push(myItem.totalWinLoss);
			eachRow.push(myItem.maxPayoutRate);
			eachRow.push(myItem.XXXXXXXXXXXXXXXXXXXXXX);

			excelData.push(eachRow);
		}

		const config = {
			headers: excelHeaders,
			rowData: excelData,
			fileName: 'Export_RankEventList.xlsx',
		};

		// @ts-ignore
		this.downloadExcel(config);
	}

	orderByValueChanged() {
		this.refreshData();
	}

	getRankEventPreview(eventID: number) {
		this.currentEventID = eventID;
		this.orderBy = -1;

		this.refreshData();
	}

	triggerRankEventSendPrize(eventID: number) {
		const msg = this.$t('Message.__isConfirmOperate').toString() + '?' + ' (排名方式:' + this.currentSelectedOrderBy.text + ')';
		if (confirm(msg)) {
			EventBus.$emit('show_mask', null);

			EventsApi.rankEventSendPrize(eventID, this.orderBy)
				.then((res) => {
					EventBus.$emit(`successfully-modified`);
					this.refreshData();
				})
				.finally(() => {
					EventBus.$emit('hide_mask', null);
				});
		}
	}

	closeAllModal() {
		this.$emit('closeParentAllModal');
	}

	mounted() {
		DropdownApi.getEventCounterOrderBy().then((res) => {
			this.dropdownItemsEventCounterOrderBy = res;
		});
	}
}
</script>


<style scoped>
.advancePanelContent {
	min-height: 250px;
	padding-bottom: 75px !important;
}
</style>