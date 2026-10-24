<template>
	<div>
		<v-card class="" v-if="targetPopupPerGameDetailLogRecordInfo.generalInfo">
			<v-card-title class="grey" style="padding: 8px 20px">
				{{ $t('Button.__showDetail') }}

				<div class="top-right-item">
					<div class="close pointer" @click="closeAllModal">
						<v-icon>close</v-icon>
					</div>
				</div>
			</v-card-title>
			<v-card-text style="padding: 5px 35px" class="ThirdPartyDetailRow">
				<v-row class="bet-detail-popup">
					<v-col cols="12" sm="6" md="6">
						<div class="">
							<div class="targetHistoryLabel">{{ $t('TableHeader.__ID') }}(DetailLog)</div>
							<div class="targetHistoryValue">
								{{ targetPopupPerGameDetailLogRecordInfo.perGameLogInfo.perGameDetailLogID }}

								<v-icon right color="grey" class="pointer" v-clipboard:copy="targetPopupPerGameDetailLogRecordInfo.perGameLogInfo.perGameDetailLogID" v-clipboard:success="copySuccess">
									file_copy
								</v-icon>
							</div>
						</div>
					</v-col>
					<v-col cols="12" sm="6" md="6">
						<div class="">
							<div class="targetHistoryLabel">{{ $t('TableHeader.__betRecordNo') }} ({{ $t('TableHeader.__gameVendorType') }})</div>
							<div class="targetHistoryValue">
								{{ targetPopupPerGameDetailLogRecordInfo.generalInfo.betRecordNo }}

								<!-- <v-icon right color="grey" class="pointer"
									v-clipboard:copy="targetPopupPerGameDetailLogRecordInfo.generalInfo.betRecordNo + ' (' + targetPopupPerGameDetailLogRecordInfo.vendorConfigAssignMerchantID  + ')'"
									v-clipboard:success="copySuccess">
									file_copy
								</v-icon> -->

								<v-icon
									right
									color="yellow darken-2"
									class="pointer"
									v-if="hasSystenAdminPermission"
									v-clipboard:copy="
										'注單號：' +
										targetPopupPerGameDetailLogRecordInfo.generalInfo.betRecordNo +
										'\n服務器：' +
										targetPopupPerGameDetailLogRecordInfo.vendorConfigAssignMerchantID +
										'\n玩家：' +
										targetPopupPerGameDetailLogRecordInfo.generalInfo.playerID +
										' (可能对应贵司的玩家帐号或昵称)\n遊戲：' +
										targetPopupPerGameDetailLogRecordInfo.generalInfo.gameName +
										' (' +
										targetPopupPerGameDetailLogRecordInfo.generalInfo.thirdPartyGameCode +
										'／pgsGameID：' +
										targetPopupPerGameDetailLogRecordInfo.generalInfo.gameID +
										')' +
										'\n時間：' +
										toDisplayTime(targetPopupPerGameDetailLogRecordInfo.generalInfo.createTime) +
										'(GMT+8)'
									"
									v-clipboard:success="copySuccess"
								>
									file_copy
								</v-icon>
							</div>
						</div>
					</v-col>
					<v-col cols="12" sm="6" md="6">
						<div class="">
							<div class="targetHistoryLabel">{{ $t('TableHeader.__betRecordNoPGS') }}</div>
							<div class="targetHistoryValue">
								{{ targetPopupPerGameDetailLogRecordInfo.generalInfo.betRecordNoPGS }}

								<v-icon right color="grey" class="pointer" v-clipboard:copy="targetPopupPerGameDetailLogRecordInfo.generalInfo.betRecordNoPGS" v-clipboard:success="copySuccess">
									file_copy
								</v-icon>
							</div>
						</div>
					</v-col>
					<v-col cols="12" sm="6" md="6">
						<div class="">
							<div class="targetHistoryLabel">{{ $t('TableHeader.__gameVendorType') }}</div>
							<div class="targetHistoryValue">
								<span v-if="hasSystenAdminPermission">
									{{ targetPopupPerGameDetailLogRecordInfo.gameVendorName }}
								</span>
								<span v-else>target</span>
							</div>
						</div>
					</v-col>
					<v-col cols="12" sm="6" md="6">
						<div class="">
							<div class="targetHistoryLabel">{{ $t('TableHeader.__game') }}</div>
							<div class="targetHistoryValue">
								{{ targetPopupPerGameDetailLogRecordInfo.generalInfo.gameName }} ( {{ targetPopupPerGameDetailLogRecordInfo.generalInfo.gameID }} ) ({{
									targetPopupPerGameDetailLogRecordInfo.generalInfo.thirdPartyGameCode
								}})
							</div>
						</div>
					</v-col>

					<v-col cols="12" sm="6" md="6">
						<div class="">
							<div class="targetHistoryLabel">{{ $t('TableHeader.__status') }}</div>
							<div class="targetHistoryValue" v-if="targetPopupPerGameDetailLogRecordInfo.generalInfo">Success</div>
						</div>
					</v-col>

					<v-col cols="12" sm="6" md="6">
						<div class="">
							<div class="targetHistoryLabel">{{ $t('TableHeader.__merchant') }}</div>
							<div class="targetHistoryValue">
								{{ targetPopupPerGameDetailLogRecordInfo.generalInfo.merchantName }}
							</div>
						</div>
					</v-col>

					<v-col cols="12" sm="6" md="6">
						<div class="">
							<div class="targetHistoryLabel">{{ $t('TableHeader.__playerAccount') }}</div>
							<div class="targetHistoryValue">
								{{ targetPopupPerGameDetailLogRecordInfo.generalInfo.playerName }}
								<span class="grey-text">({{ targetPopupPerGameDetailLogRecordInfo.generalInfo.playerID }})</span>
							</div>
						</div>
					</v-col>

					<v-col cols="12" sm="6" md="6">
						<div class="">
							<div class="targetHistoryLabel">{{ $t('TableHeader.__betAmount') }}</div>
							<div class="targetHistoryValue">
								{{ targetPopupPerGameDetailLogRecordInfo.generalInfo.betAmount }}
							</div>
						</div>
					</v-col>
					<v-col cols="12" sm="6" md="6">
						<div class="">
							<div class="targetHistoryLabel">{{ $t('TableHeader.__payoutAmount') }}</div>
							<div class="targetHistoryValue">
								{{ targetPopupPerGameDetailLogRecordInfo.generalInfo.payoutAmount }}
							</div>
						</div>
					</v-col>
					<v-col cols="12" sm="6" md="6">
						<div class="">
							<div class="targetHistoryLabel">{{ $t('TableHeader.__validAmount') }}</div>
							<div class="targetHistoryValue">
								{{ targetPopupPerGameDetailLogRecordInfo.generalInfo.validAmount }}
							</div>
						</div>
					</v-col>
					<v-col cols="12" sm="6" md="6">
						<div class="">
							<div class="targetHistoryLabel">{{ $t('TableHeader.__winLossAmount') }}</div>
							<div class="targetHistoryValue">
								{{ targetPopupPerGameDetailLogRecordInfo.generalInfo.winLossAmount }}
							</div>
						</div>
					</v-col>

					<v-col cols="12" sm="6" md="6">
						<div class="">
							<div class="targetHistoryLabel">{{ $t('TableHeader.__taxAmount') }}</div>
							<div class="targetHistoryValue" v-if="targetPopupPerGameDetailLogRecordInfo.generalInfo">
								{{ targetPopupPerGameDetailLogRecordInfo.generalInfo.taxAmount }}
							</div>
						</div>
					</v-col>

					<v-col cols="12" sm="6" md="6">
						<div class="">
							<div class="targetHistoryLabel">{{ $t('TableHeader.__othersMemo') }}／{{ $t('TableHeader.__convertRate') }}</div>
							<div class="targetHistoryValue">
								<span v-if="targetPopupPerGameDetailLogRecordInfo.generalInfo.convertRate == 1">-</span>
								<span v-else>{{ targetPopupPerGameDetailLogRecordInfo.generalInfo.convertRate }}</span>
							</div>
						</div>
					</v-col>

					<v-col cols="12" sm="6" md="6">
						<div class="">
							<div class="targetHistoryLabel">{{ $t('TableHeader.__createTime') }}</div>
							<div class="targetHistoryValue" v-if="targetPopupPerGameDetailLogRecordInfo.generalInfo">
								{{ targetPopupPerGameDetailLogRecordInfo.generalInfo.createTime | timeWithAddTimezone(userProfile.timeZone) }}
							</div>
						</div>
					</v-col>

					<v-col cols="12" sm="6" md="6">
						<div class="">
							<div class="targetHistoryLabel">{{ $t('TableHeader.__settleTime') }}</div>
							<div class="targetHistoryValue" v-if="targetPopupPerGameDetailLogRecordInfo.generalInfo">
								{{ targetPopupPerGameDetailLogRecordInfo.generalInfo.settlementTime | timeWithAddTimezone(userProfile.timeZone) }}
							</div>
						</div>
					</v-col>

					<v-col cols="12" sm="6" md="6">
						<div class="">
							<div class="targetHistoryLabel">
								{{ $t('TableHeader.__extraInfo') }}
								({{ $t('TableHeader.__roundID') }}/{{ $t('TableHeader.__othersMemo') }})
							</div>
							<div class="targetHistoryValue" v-if="targetPopupPerGameDetailLogRecordInfo.generalInfo">
								{{ targetPopupPerGameDetailLogRecordInfo.generalInfo.extraInfo1 }}
							</div>
						</div>
					</v-col>

					<v-col
						cols="12"
						sm="12"
						md="12"
						v-if="
							targetPopupPerGameDetailLogRecordInfo.perGameLogInfo &&
							targetPopupPerGameDetailLogRecordInfo.perGameLogInfo.tags &&
							targetPopupPerGameDetailLogRecordInfo.perGameLogInfo.tags.length > 0
						"
					>
						<div class="targetHistoryValue">
							<div class="targetHistoryLabel">{{ $t('TableHeader.__tag') }}</div>
							<div class="">
								<v-chip v-for="(item, index) in targetPopupPerGameDetailLogRecordInfo.perGameLogInfo.tags" :key="index">
									{{ item }}
								</v-chip>
							</div>
						</div>
					</v-col>

					<!-- <v-col cols="12" sm="12" md="12"
						v-if="targetPopupPerGameDetailLogRecordInfo.isAccountingIgnore">

						<div class="targetHistoryValue">

							<div class="targetHistoryLabel">{{$t('TableHeader.__tag')}}</div>
							<div class="">
								<v-chip
									v-for="(item , index) in targetPopupPerGameDetailLogRecordInfo.perGameLogInfo.tags"
									:key="index">
									{{item}}
								</v-chip>
							</div>

						</div>

					</v-col> -->
				</v-row>

				<v-row>
					<v-col cols="12" sm="12" md="12">
						<div class="text-c" style="margin-top: 10px">
							<div>
								<v-btn text color="orange" @click="copyTextClick()" class="btn mr-1">
									<v-icon left>file_copy</v-icon>
									{{ $t('Button.__copyToClipboard') }}
								</v-btn>

								<v-btn
									v-if="targetPopupPerGameDetailLogRecordInfo.isSupportExternalLink"
									color="blue darken-1"
									text
									class="btn link mr-1"
									@click.stop="getBetRecordExternalLink(false)"
								>
									<v-icon left>link</v-icon>
									{{ $t('Button.__openExternalLink') }}
								</v-btn>

								<v-btn
									v-if="targetPopupPerGameDetailLogRecordInfo.isSupportExperimentFeatureForInternalUsers && hasSystenExperimentFeaturePermission"
									color="red lighten-1"
									text
									class="btn link mr-1"
									@click.stop="getBetRecordExternalLink(false)"
								>
									<v-icon left>link</v-icon>
									(實驗性功能)產生快照連結
								</v-btn>

								<v-btn v-if="!targetPopupPerGameDetailLogRecordInfo.isSupportExternalLink" color="yellow darken-1" text class="btn link" disabled>
									<v-icon left>link</v-icon>
									{{ $t('Message.__notSupport') }}
								</v-btn>
							</div>
						</div>
					</v-col>
				</v-row>
			</v-card-text>
		</v-card>

		<v-dialog v-model="dialogVisible.isShowCopyDialog" max-width="500px">
			<v-card class="dialog-v-card-border">
				<v-card-title class="">
					<div class="top-right-item">
						<div class="close pointer" @click="closeCopyDialog">
							<v-icon>close</v-icon>
						</div>
					</div>
				</v-card-title>

				<v-card-text>
					<div class="mb-2 text-c" v-if="targetPopupPerGameDetailLogRecordInfo.gameVendorID == 250000">
						<div class="hint-red-warning">⚠️ {{ $t('Message.__thisPageHasLimitIPAddress') }}</div>
						<div class="hint-red-warning">⚠️ {{ $t('Message.__thisPageHasLimitIPAddress') }}</div>
					</div>

					<div>
						{{ $t('Message.__pleaseClickTheBelowLink') }}
					</div>

					<div class="mt-2" style="white-space: nowrap; overflow: hidden; text-overflow: ellipsis; max-width: 95vw">
						<a :href="externalLink" target="_blank">{{ externalLink }}</a>
					</div>

					<div class="mt-3 text-c">
						<v-btn color="yellow darken-1" text class="btn link mr-1" @click.stop="copyTextToClipboard2(externalLink)">
							<v-icon left>file_copy</v-icon>
							{{ $t('Button.__copyExternalLink') }}
						</v-btn>

						<div class="mt-2">
							<v-icon small color="red ligthen-2">error</v-icon>

							<span class="minus-red" style="font-size: 12px">
								{{ $t('Message.__fewOfMobileWillCopyFail') }}
							</span>
						</div>
					</div>
				</v-card-text>
			</v-card>
		</v-dialog>

		<v-dialog v-model="dialogVisible.isShowExtra2" max-width="450px">
			<v-card>
				<v-card-title class="grey darken-2">
					Game-Result
					<div class="top-right-item">
						<div class="close pointer" @click="dialogVisible.isShowExtra2 = false">
							<v-icon>close</v-icon>
						</div>
					</div>
				</v-card-title>

				<v-card-text>
					<div class="text-c" style="padding: 25px">
						<div v-html="parsedHtml(this.thirdPartyRawHtml)"></div>
					</div>
				</v-card-text>
			</v-card>
		</v-dialog>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import * as Model from '@/models/interfaces/game-management/per-game';
import GameManagementApi from '@/api/game-management/per-game';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import * as Permission from '@/models/status/permission';
import { datetimeMixin, startEndDate, timeWithTimezone, daysAgo, weeksAgo, monthsAgo, formatMixin } from '@/utilities/datetime-format';
import { commonMethodMixin } from '@/utilities/common-methods';

import EventBus from '@/utilities/event-bus';
import { State, Action, Getter, namespace } from 'vuex-class';
const authModule = namespace('Auth');
@Component({ mixins: [datetimeMixin, commonDisplayFiltersMixin, formatMixin, commonMethodMixin] })
export default class PerGameLogDetail extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];

	dialogVisible: DropdownModel.DialogVisible = new DropdownModel.DialogVisible();
	externalLink: string = '';
	thirdPartyRawHtml: string = '';

	get hasAdminPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.GamesManagementAdminPermission) >= 0;
	}

	get hasSystenAdminPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSetting_SystemConfigSetting) >= 0;
	}

	get hasSystenExperimentFeaturePermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.Special_AllowToUseExperimentFeature) >= 0;
	}

	targetPopupPerGameDetailLogRecordInfo: Model.IBetRecordInfo = {} as Model.IBetRecordInfo;

	getExternalLinkDialogInfo(item: Model.IListResponse) {
		this.targetPopupPerGameDetailLogRecordInfo = {} as Model.IBetRecordInfo;

		EventBus.$emit('show_mask', null);

		GameManagementApi.getBetRecordByTransactionNoForPerGamePage(item.transactionNo)
			.then((res) => {
				this.targetPopupPerGameDetailLogRecordInfo = res.data;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	getExternalLinkDialogInfoByNo(transactionNo: string) {
		this.targetPopupPerGameDetailLogRecordInfo = {} as Model.IBetRecordInfo;

		EventBus.$emit('show_mask', null);

		GameManagementApi.getBetRecordByTransactionNoForPerGamePage(transactionNo)
			.then((res) => {
				this.targetPopupPerGameDetailLogRecordInfo = res.data;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	parsedHtml(myHtml: string) {
		const parser = new DOMParser();
		const elem = parser.parseFromString(myHtml, 'text/html');

		return elem.body.innerText;
	}

	showCopyLink() {
		this.dialogVisible.isShowCopyDialog = true;
	}

	closeCopyDialog() {
		this.dialogVisible.isShowCopyDialog = false;
	}

	copyTextClick() {
		const vm: any = this;
		const perGame = this.targetPopupPerGameDetailLogRecordInfo.perGameLogInfo;
		const item = this.targetPopupPerGameDetailLogRecordInfo.generalInfo;

		// 2026/03 新版方式 提供另一種文案Copy
		if (this.targetPopupPerGameDetailLogRecordInfo.copyInfoForGameVendor && !vm.isEmptyString(this.targetPopupPerGameDetailLogRecordInfo.copyInfoForGameVendor)) {
			// vm.copyTextToClipboard2(this.targetPopupPerGameDetailLogRecordInfo.copyInfoForGameVendor);
			vm.showCopyTextDialog(this.targetPopupPerGameDetailLogRecordInfo.copyInfoForGameVendor);

			return;
		}

		let text = '';

		if (this.hasSystenAdminPermission) {
			text += '游戏商投注单号：' + item.betRecordNo + '\n';
		}

		text += 'transactionNo：' + item.betRecordNoPGS + '\n';
		text += 'betID：' + perGame.perGameDetailLogID + ' (for-old-version)' + '\n';

		if (this.hasSystenAdminPermission) {
			text += '服務器名：' + this.targetPopupPerGameDetailLogRecordInfo.vendorConfigAssignMerchantID + '\n';
		}

		text += '游戏名称：' + item.gameName + ' (' + item.thirdPartyGameCode + ')' + '\n';
		text += '玩家名称：' + item.playerName + ' (' + item.playerID + ')' + '\n';
		text += '投注金额：' + vm.toAmountDisplay(item.betAmount) + '\n';
		text += '派彩金额：' + vm.toAmountDisplay(item.payoutAmount) + '\n';
		text += '输赢结果：' + vm.toAmountDisplay(item.winLossAmount) + '\n';
		text += '投注時間：' + item.settlementTime + ' (UTC+0)' + '\n';

		vm.copyTextToClipboard2(text);
	}

	showCopyTextDialog(myCopyText: string) {
		const myConfig: any = {
			copyText: myCopyText,
		};

		EventBus.$emit(`globalShowCopyTextDialog`, myConfig);
	}

	toDisplayTime(isoTime: string) {
		return timeWithTimezone(isoTime, this.userProfile.timeZone);
	}

	closeAllModal() {
		this.$emit('closeParentAllModal');
	}

	getBetRecordExternalLink(isCopy: boolean = false) {
		EventBus.$emit('show_mask', null);
		const vm: any = this;

		GameManagementApi.getExternalBetRecordDetailUrlByTransactionNo(this.targetPopupPerGameDetailLogRecordInfo.generalInfo.betRecordNoPGS)
			.then((res) => {
				// 請參考 ThirdPartyVendorInfoTypeEnum.cs
				if (res.data.infoType === 1) {
					this.externalLink = res.data.url;
					this.showCopyLink();
				} else {
					if (res.data.infoType === 5) {
						this.dialogVisible.isShowExtra2 = true;
						this.thirdPartyRawHtml = res.data.rawJson;
					} else {
						const win: any = window.open('', 'Title');
						win.document.body.innerHTML = '<h1>游戏供应商提供以下原始内容参考</h1>' + '<div style="white-space: pre;">' + res.data.rawJson + '</div>';
					}
				}
			})
			.catch((res) => {
				// setTimeout(() => {
				// 	alert(res.messages + '\n提示* 若需联系游戏开发团队排除问题。建议搭配 [三方API请求记录管理] 功能 ');
				// }, 500);
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}
	copySuccess() {
		EventBus.$emit(`successfully-copy`);
	}
}
</script>

<style scoped>
.col-md-6 {
	margin-bottom: -5px;
}

.targetHistoryLabel {
	margin-top: 1px;
	color: #888;
}

.targetHistoryValue {
	border-bottom: dashed 1px #888;
	font-size: 16px;
	padding: 2px 0px;
}

.hint-red-warning {
	background: rgb(97 38 38);
	padding: 5px 10px;
	color: #fb8f8f;
	font-size: 18px;
	margin-bottom: 2px;
	/* display: inline-block; */
	border-radius: 5px;
}
</style>
