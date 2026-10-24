<template>

	<v-dialog v-model="isShowRequestLog" max-width="800px">

		<v-card class="notify-queue-info">
			<v-card-title>

				<div class="top-right-item">
					<div class="close" @click="closeAllDialog">
						<v-icon>close</v-icon>
					</div>
				</div>

			</v-card-title>
			<v-card-text class="">

				<h3>
					基本資訊
				</h3>
				<v-row style="padding:15px 35px;">

					<v-col cols="12" sm="6" md="6">
						<div class="common-info-label">{{$t('TableHeader.__ID')}}</div>
						<div class="common-info-value">
							{{queryNotifyQueueInfo.id}}
						</div>
					</v-col>

					<v-col cols="12" sm="6" md="6">
						<div class="common-info-label">{{$t('TableHeader.__transactionType')}}</div>
						<div class="common-info-value">
							{{queryNotifyQueueInfo.typeName}}
						</div>
					</v-col>

					<v-col cols="12" sm="6" md="6">
						<div class="common-info-label">{{$t('TableHeader.__betRecordNo')}}</div>
						<div class="common-info-value">
							{{queryNotifyQueueInfo.betRecordNoPGS}}
						</div>
					</v-col>

					<v-col cols="12" sm="6" md="6">
						<div class="common-info-label">RequestID</div>
						<div class="common-info-value">
							{{queryNotifyQueueInfo.lastRequestUUID}}
						</div>
					</v-col>

					<v-col cols="12" sm="6" md="6">
						<div class="common-info-label">重試次數</div>
						<div class="common-info-value">
							{{queryNotifyQueueInfo.retryTimes}}
						</div>
					</v-col>

					<v-col cols="12" sm="6" md="6">
						<div class="common-info-label">{{$t('TableHeader.__memo')}}</div>
						<div class="common-info-value">
							&nbsp;
						</div>
					</v-col>

					<v-col cols="12" sm="6" md="6">
						<div class="common-info-label">
							{{$t('TableHeader.__status')}}
						</div>
						<div class="common-info-value">

							<v-chip v-if="queryNotifyQueueInfo.isSuccess" color="success">成功</v-chip>
							<v-chip v-if="!queryNotifyQueueInfo.isSuccess" color="red">失敗</v-chip>

						</div>
					</v-col>

				</v-row>

				<div style="margin-top:15px;" v-if="true || !queryNotifyQueueInfo.isSuccess">
					<div class="text-c">
						<v-btn color="deep-purple darken-1"
							@click="triggerQueueNotify(queryNotifyQueueInfo.id)">
							<v-icon class="mr-1">circle_notifications</v-icon>
							重新送出通知
						</v-btn>
					</div>

				</div>

				<h3 style=" margin-top:35px;margin-bottom: 15px;">
					所有請求紀錄
				</h3>
				<v-expansion-panels accordion>

					<v-expansion-panel v-for="(item , index) in queryNotifyQueueInfo.logs" :key="index">

						<v-expansion-panel-header style="padding: 10px;">

							<div style="display: inline-block;width: 30px;max-width: 30px;margin-right: 3px;">
								<v-icon>history</v-icon>
							</div>

							{{item.createTime | timeWithAddTimezone(userProfile.timeZone)}}

						</v-expansion-panel-header>
						<v-expansion-panel-content>

							<v-divider></v-divider>

							<div style="padding: 0px 25px;">

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
							</div>

						</v-expansion-panel-content>

					</v-expansion-panel>

				</v-expansion-panels>
			</v-card-text>
		</v-card>
	</v-dialog>
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

import axios, { AxiosResponse, AxiosRequestConfig } from 'axios';

const authModule = namespace('Auth');

// @ts-ignore
@Component({ mixins: [datetimeMixin, commonDisplayFiltersMixin, formatMixin, commonMethodMixin], components: { DatePicker, PopupCopyDatetime } })
export default class NotifyQueueDetailPopup extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];
	@playersModule.State('transactionNoPGS') transactionNoPGS!: string;

	renderComponent: boolean = true;
	panel: number[] = [0];
	isShowRequestLog: boolean = false;

	DataTablePagingParameter: DropdownModel.DataTablePagingParameter = new DropdownModel.DataTablePagingParameter();
	datetimeRangePicker: DropdownModel.DatetimeRangePicker = new DropdownModel.DatetimeRangePicker();

	betStatusDropdownItems: DropdownModel.IDropdownItems[] = [];
	gameVendorDropdownItems: DropdownModel.IDropdownItems[] = [];

	queryRecords: Model.IUnionBetRecordResponse[] = [];
	queryNotifyQueueInfo: Model.IBetRecordNotifyQueue = {} as Model.IBetRecordNotifyQueue;
	refundTarget: Model.IUnionBetRecordResponse = {} as Model.IUnionBetRecordResponse;

	@Action('Players/setTransactionNoPGS') private setTransactionNoPGS!: any;
	@Action('Players/setPlayerID') private setPlayerID!: any;

	get hasDevelopeSuperPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.Special_AllowToUseDevelopeFeature) >= 0;
	}

	get hasAdminPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.GamesManagementAdminPermission) >= 0;
	}

	get hasPermissionManualRefund() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.GamesManagement_BetRecord_ManualRefund) >= 0;
	}

	geti18nTextAsString(key: string) {
		return this.$t(key).toString();
	}

	toDisplayTime(isoTime: string) {
		return timeWithTimezone(isoTime, this.userProfile.timeZone);
	}

	showNotifyLogs(queueID: number) {
		this.queryNotifyQueueInfo = {} as Model.IBetRecordNotifyQueue;
		this.isShowRequestLog = true;
		this.reloadNotifyQueueData(queueID);
	}

	reloadNotifyQueueData(queueID: number) {
		EventBus.$emit('show_mask', null);
		GameManagementApi.getNotifyQueueInfo(queueID)
			.then((res) => {
				this.queryNotifyQueueInfo = res.data;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	closeAllDialog() {
		this.isShowRequestLog = false;
	}

	triggerQueueNotify(notifyQueueID: number) {
		EventBus.$emit(`show_mask`);
		GameManagementApi.triggerNotifyQueue(notifyQueueID)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				// 刷新資料
				this.reloadNotifyQueueData(notifyQueueID);
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	mounted() {
		this.init();
		const route: any = this.$route;
	}

	copySuccess() {
		EventBus.$emit(`successfully-copy`);
	}

	init() {
		DropdownApi.getBetStatus().then((res) => {
			this.betStatusDropdownItems = res;
		});

		this.clear();
	}

	copyText(text: string) {
		const vm: any = this;
		vm.copyTextToClipboard(text);
	}

	clear() {
		const vm: any = this;
		vm.globalClear();

		this.datetimeRangePicker.searchStartTimeRange = [daysAgo(-1), daysAgo(1)];
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


