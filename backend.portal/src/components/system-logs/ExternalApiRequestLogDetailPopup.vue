<template>
	<v-app v-if="renderComponent" class="app">
		<v-dialog v-model="isShowInfoDialog" max-width="800px" scrollable>
			<v-card>
				<v-card-title class="grey darken-2">
					<span>
						{{ $t('Menu.SystemLogsExternalApiRequestLogList') }}
					</span>
					<div class="top-right-item" @click="close">
						<v-btn text color="pink" v-if="isShowNotifyBtn" class="btn mr-4" @click.stop="sendLog">
							<v-icon left>mdi-volume-high</v-icon>
							(💀💀別按錯*)發送訊息至頻道
						</v-btn>

						<v-btn text color="orange" v-clipboard:copy="copyText" v-clipboard:success="copySuccess" class="btn">
							<v-icon left>file_copy</v-icon>
							{{ $t('Button.__copyToClipboard') }}
						</v-btn>

						<v-btn icon dark @click="close">
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>

				<v-card-text>
					<v-flex>
						<v-row>
							<v-col cols="12" sm="12" md="12">
								<div class="lineDiv info-column-name">請求網址</div>
								<div style="color: #2196f3">
									{{ detailInfo.requestUrl }}
								</div>
							</v-col>
						</v-row>

						<v-row>
							<v-col cols="12" sm="12" md="12">
								<div class="lineDiv info-column-name">請求時間</div>
								<div>
									{{ detailInfo.createTime | timeWithAddTimezone(userProfile.timeZone) }}
								</div>
							</v-col>
						</v-row>

						<v-row>
							<v-col cols="12" sm="12" md="12">
								<div class="lineDiv info-column-name">請求參數 (request body)</div>
								<div class="info-code-panel">
									{{ detailInfo.requestContent }}
								</div>
							</v-col>
						</v-row>

						<v-row v-if="!isEmptyString(detailInfo.requestHeader)">
							<v-col cols="12" sm="12" md="12">
								<div class="lineDiv info-column-name">請求表頭 (request header)</div>
								<div class="info-code-panel">
									{{ detailInfo.requestHeader }}
								</div>
							</v-col>
						</v-row>

						<v-row>
							<v-col cols="12" sm="12" md="12">
								<div class="lineDiv info-column-name">HttpStatusCode (response)</div>
								<div class="orange-text">
									{{ detailInfo.responseStatusCode }} <span class="ml-2">{{ this.timeoutExtraText }}</span>
								</div>
							</v-col>
						</v-row>

						<v-row v-if="detailInfo.responseTime > 3000">
							<v-col cols="12" sm="12" md="12">
								<div class="lineDiv info-column-name">执行时间 (ExecutionTime)</div>
								<div class="orange-text">{{ (detailInfo.responseTime / 1000).toFixed(1) }} s</div>
							</v-col>
						</v-row>

						<v-row>
							<v-col cols="12" sm="12" md="12">
								<div class="lineDiv info-column-name">
									API回應 (response)
									<v-icon style="font-size: 22px" color="yellow lighten-2" @click="copyTextToClipboard2(detailInfo.responseText)">file_copy</v-icon>
								</div>
								<div class="info-code-panel">
									{{ detailInfo.responseText }}
								</div>
							</v-col>
						</v-row>
					</v-flex>
				</v-card-text>
			</v-card>
		</v-dialog>
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import * as SettingModel from '@/models/interfaces/system-setting/systemConfigSetting';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import * as Model from '@/models/interfaces/system-logs/externalApiRequestLog';
import LogApi from '@/api/system-logs/externalApiRequestLog';
import DropdownApi from '@/api/commons/dropdown';
import { Locale, Currency, Status, GameType, CRUDAction, GameVendorType } from '@/models/status/common';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { pageData, time } from '@/utilities/api-handler.lib';
import { datetimeMixin, startEndDate, daysAgo } from '@/utilities/datetime-format';
import DatePicker from 'vue2-datepicker';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import { commonMethodMixin } from '@/utilities/common-methods';
import { State, Action, Getter, namespace } from 'vuex-class';

const authModule = namespace('Auth');

import * as Permission from '@/models/status/permission';

// @ts-ignore
@Component({ components: { DatePicker }, mixins: [commonMethodMixin, commonDisplayFiltersMixin, datetimeMixin] })
export default class ExternalApiRequestDetailPopup extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];
	renderComponent: boolean = true;
	isShowInfoDialog: boolean = false;
	isShowNotifyBtn: boolean = false;

	panel: number[] = [0];

	ListData: Model.IExternalApiRequestLogResponse[] = [];
	detailInfo: Model.IExternalApiRequestLogDetailInfo = {} as Model.IExternalApiRequestLogDetailInfo;
	copyText: string = '';
	timeoutExtraText: string = '';
	notifyMerchantID: number = 0;
	notifyContent: string = '';

	showInfoPopup(item: Model.IExternalApiRequestLogDetailInfo) {
		this.isShowInfoDialog = true;
		this.timeoutExtraText = '';
		this.detailInfo = item;
		this.isShowNotifyBtn = false;
		this.notifyMerchantID = 0;
		this.notifyContent = '';

		let text1 = '';
		let text2 = '';
		let textFinal = '';

		if (item.merchantID > 0) {
			this.isShowNotifyBtn = true;
			this.notifyMerchantID = item.merchantID;
		}

		if (item.isShowCustomerMessage) {
			text1 = '您好，我方玩家' + item.playerName + ' (' + item.playerID + ') 反馈进入游戏失败' + '\n';
			text2 = '技术这边查看API请求资讯如下' + '\n\n';
			textFinal = '这边要麻烦贵司协助 釐清错误原因，谢谢' + '\n';
		}

		const vm: any = this;

		const timeText: any = this.detailInfo.createTime;
		const timeTextUtc8: any = vm.toTimeWithAddTimezone(this.detailInfo.createTime);

		const requestTime = '．发送时间(RequestTime)\n`' + timeTextUtc8 + '  (UTC+8)`\n\n';
		const infoUrl = '．请求网址(RequestUrl)\n`' + this.detailInfo.requestUrl + '`\n\n';
		const infoBody = '．请求参数(RequestBody)\n```\n' + this.detailInfo.requestContent + '\n```\n\n';
		const infoRes = '．API回应(Response)\n```\n' + this.detailInfo.responseText + '\n```\n\n';

		let statusCode = '';

		if (this.detailInfo.responseStatusCode > 0 && this.detailInfo.responseStatusCode !== 200) {
			if (this.detailInfo.responseStatusCode === 14) {
				let totalSecondsText = '';

				if (item.responseTime > 1000 * 10) {
					totalSecondsText = ' > ' + Math.round(item.responseTime / 1000) + 's';
				}
				this.timeoutExtraText = ' (⚠️Request Timeout ' + totalSecondsText + ')';
				statusCode = '．Http回应码(StatusCode)\n``` -1' + this.timeoutExtraText + '\n```\n\n';
			} else if (this.detailInfo.responseStatusCode <= 99) {
				this.timeoutExtraText = ' (*ErrorCode)';
				statusCode = '．Http回应码(ErrorCode)\n``` ' + this.detailInfo.responseStatusCode + this.timeoutExtraText + '\n```\n\n';
			} else {
				statusCode = '．Http回应码(StatusCode)\n``` ' + this.detailInfo.responseStatusCode + '\n```\n\n';
			}
		}

		let executionTimeText = '';

		if (this.detailInfo.responseTime > 3000) {
			executionTimeText = '执行时间(ExecutionTime)\n```' + (this.detailInfo.responseTime / 1000).toFixed(1) + 's ```\n\n';
		}

		let infoHeader = '．请求表头(RequestHeader)\n```\n' + this.detailInfo.requestHeader + '\n```\n\n';

		if (!this.detailInfo.requestHeader || this.detailInfo.requestHeader === '' || this.detailInfo.requestHeader == null) {
			infoHeader = '';
		}

		this.copyText = text1 + text2 + requestTime + infoUrl + infoBody + infoHeader + statusCode + executionTimeText + infoRes + textFinal;
		this.notifyContent = this.copyText;
	}

	sendLog() {
		const request: Model.ISendLogRequest = {
			notifyMerchantID: this.notifyMerchantID,
			notifyContent: this.notifyContent,
		};

		if (confirm('請確認 是否要發送訊息至TG頻道?? ')) {
			EventBus.$emit('show_mask', null);
			LogApi.sendLog(request)
				.then(() => {
					EventBus.$emit(`successfully-modified`);
				})
				.finally(() => {
					EventBus.$emit('hide_mask', null);
				});
		}
	}

	copySuccess() {
		EventBus.$emit(`successfully-copy`);
	}

	close() {
		this.isShowInfoDialog = false;
	}

	mounted() {}
}
</script>
