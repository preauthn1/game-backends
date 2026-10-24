<template>
	<v-app v-if="renderComponent" class="app">
		<div>
			<div>
				<v-divider></v-divider>
			</div>

			<v-row>
				<v-col cols="12" sm="4" md="4">
					<v-row>
						<v-col cols="12" sm="12" md="12">
							<h3 class="">
								System health check

								<v-btn color="yellow lighten-3" text class="btn search ml-4" small @click.stop="getCustomerServiceUtilityInfoForRefresh()">
									<v-icon left>refresh</v-icon>
									Refresh check </v-btn
								>&#9;
							</h3>

							<div v-if="csUtilityInfo && csUtilityInfo != null && csUtilityInfo.warningMessages && csUtilityInfo.warningMessages.length > 0">
								<div class="mb-2 f12 grey-text">{{ csUtilityInfo.refreshTimeTW }} (last refresh time)</div>

								<div v-for="(item, index) in csUtilityInfo.warningMessages" :key="index">
									<div v-if="item.isErrorHint">
										<v-icon color="red" class="mr-1">error</v-icon>
										<span class="minus-red">{{ item.message }}</span>
									</div>

									<div v-if="!item.isErrorHint">
										<v-icon color="green" class="mr-1">mdi-check</v-icon>
										<span class="">{{ item.message }}</span>
									</div>
								</div>
							</div>
						</v-col>

						<v-col cols="12" sm="12" md="12" style="margin-top: 30px">
							<h3 class="mb-2"><v-icon small color="yellow darken-2">mdi-lightbulb-on</v-icon> 其他功能按鈕 (Special action)</h3>

							<div>
								<div class="mb-1">
									<v-btn color="blue lighten-3" text class="btn search ml-1" small @click.stop="triggerBackendButtonAction('解鎖後台登入(Unlock login)', 2650)">
										<v-icon left>add</v-icon>
										解鎖後台登入(Unlock login) </v-btn
									>&#9;
								</div>

								<div class="mb-1">
									<v-btn color="purple lighten-3" text class="btn search ml-1" small @click.stop="changeGameUrlDomainByCS()">
										<v-icon left>add</v-icon>
										修正域名封鎖(Url blocked) </v-btn
									>&#9;
								</div>
							</div>
						</v-col>

						<v-col cols="12" sm="12" md="12" style="margin-top: 30px">
							<h3 class="mb-2"><v-icon small color="yellow darken-2">mdi-lightbulb-on</v-icon> 测速页网址 (Speed test url)</h3>

							<div style="background: #444; padding: 5px 8px; border-radius: 5px" class="f14">
								<div>Hi Teams, We need player help test the both links, and screenshot</div>
								<div>the information, please ask player to assist with this. (both link need validate)</div>

								<div>&nbsp;</div>
								<div class="link-text-no-pointer">
									{{ csUtilityInfoOfBasicInfo.speedTestUrls_Basic }}
								</div>
								<div class="link-text-no-pointer">
									{{ csUtilityInfoOfBasicInfo.speedTestUrls_Newest }}
								</div>
							</div>
						</v-col>

						<v-col cols="12" sm="12" md="12" style="margin-top: 0px">
							<div v-for="(item, index) in csUtilityInfoOfBasicInfo.groupItems" :key="index" style="margin-top: 50px !important">
								<h3 class="mb-2">
									<!-- <v-icon small>info</v-icon>  -->
									{{ item.groupName }}
								</h3>

								<div>
									<v-chip small v-for="(text, index2) in item.items" :key="index2" class="mr-2">
										{{ text }}
									</v-chip>
								</div>
							</div>
						</v-col>

						<v-col cols="12" sm="12" md="12" style="margin-top: 50px">
							<h3 class="mb-2">常用連結 (link for cs-test)</h3>

							<div>
								<a class="link-text" @click="downloadGameList()" target="_blank"> Download game list (下載遊戲清單) </a>
							</div>

							<div>
								<a class="link-text" href="https://example.invalid?gameType=1000" target="_blank"> demo-site (Slots game) </a>
							</div>
							<div>
								<a class="link-text" href="https://example.invalid/?gameType=2500" target="_blank"> demo-site (Live game) </a>
							</div>
						</v-col>

						<v-col cols="12" sm="12" md="12" style="margin-top: 50px" v-if="hasSuperAdminPermission">
							<h3 class="mb-2">複製用文案</h3>

							<div class="f14 pointer" style="color: #aaa" @click="copyTextToClipboard2(maintainTextOfCS)">
								<div style="white-space: break-spaces">{{ maintainTextOfCS }}</div>
							</div>
							<div></div>
						</v-col>
					</v-row>
				</v-col>

				<v-col cols="12" sm="8" md="8">
					<h3>
						Support Currency (支持貨幣)
						<v-icon id="copyAllCurrency" class="pointer" @click="copyTextToClipboard2((csUtilityInfoOfBasicInfo.supportCurrency || []).map(item => item.currency).join(', '))">file_copy</v-icon>
					</h3>

					<v-data-table
						:items="csUtilityInfoOfBasicInfo.supportCurrency"
						hide-default-footer
						style="width: 500px"
						:footer-props="{ 'items-per-page-options': [100] }"
						:headers="currencyHeaders"
					>
					</v-data-table>
				</v-col>
			</v-row>
		</div>

		<v-dialog v-model="isShowMessageHintAfterSubmit" max-width="500px">
			<v-card>
				<v-card-title class="grey darken-2">
					{{ $t('TableHeader.__prompt') }}
				</v-card-title>

				<v-card-title>
					<div style="min-height: 400px">
						<div class="f16" v-html="resultMessageHtmlFormat"></div>
					</div>
				</v-card-title>
			</v-card>
		</v-dialog>
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import DropdownApi from '@/api/commons/dropdown';
import PlayersApi from '@/api/members-management/players';
import InternalTestApi from '@/api/commons/internalTest';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import NotificationWarning from '@/components/notification/NotificationWarning.vue';

import * as MemberModel from '@/models/interfaces/members-management/players';
import * as Model from '@/models/interfaces/system-setting/systemConfigSetting';
import systemConfigSettingApi from '@/api/system-setting/systemConfigSetting';
import { commonMethodMixin } from '@/utilities/common-methods';

import * as ResponseModel from '@/models/interfaces/commons/response';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import { State, Action, Getter, namespace } from 'vuex-class';
const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';

@Component({ components: { NotificationWarning }, mixins: [commonMethodMixin] })
export default class ServersRobotConfig extends Vue {
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];
	@authModule.State('defaultLangTimes') defaultLangTimes!: number;

	isShowCreateTestAccount: boolean = false;
	isShowInputColumnForBackendButtons: boolean = false;
	isShowMessageHintAfterSubmit: boolean = false;

	renderComponent: boolean = true;
	showNotificationWarning: boolean = false;
	editModeOpenGroups: number[] = [];

	maintainTextOfCS: string = '';
	resultMessageHtmlFormat: string = '';

	dropdownInternalMerchants: any[] = [];
	dropdownMerchants: any[] = [];
	dropdowGameVendorTypes: any[] = [];

	createTestAccountForm: MemberModel.IInternalTestAccountCreateRequest = {} as MemberModel.IInternalTestAccountCreateRequest;
	currentSelectBackendButton: ResponseModel.IBackendButtonItem = {} as ResponseModel.IBackendButtonItem;
	backendButtonForm: ResponseModel.IBackendButtonActionRequest = {} as ResponseModel.IBackendButtonActionRequest;
	backendButtonResponse: ResponseModel.ICRUDResult = {} as ResponseModel.ICRUDResult;
	csUtilityInfo: ResponseModel.ICustomerServiceUtilityInfo = {} as ResponseModel.ICustomerServiceUtilityInfo;
	csUtilityInfoOfBasicInfo: ResponseModel.ICustomerServiceUtilityInfo = {} as ResponseModel.ICustomerServiceUtilityInfo;

	systemConfigSettings: Model.ISystemConfigSetting = <Model.ISystemConfigSetting>{};

	get currencyHeaders() {
		const headers = [];
		headers.push({ text: 'CurrencyText', value: 'currencyText' });
		headers.push({ text: 'Currency', value: 'currency' });
		headers.push({ text: 'Memo', value: 'memo' });

		return headers;
	}

	get hasSuperAdminPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSetting_SystemConfigSetting) >= 0;
	}

	setTextForCopy() {
		this.maintainTextOfCS = '麻煩通知部分客戶 (只有特定幾個) \n';
		this.maintainTextOfCS += '請前往 後台 > 遊戲紀錄 > 客服工具 > Jili Operator Channel 區塊\n';
		this.maintainTextOfCS += '該區塊有列出有哪些客戶頻道需通知  @CS1 @CS2 \n';
	}

	getCustomerServiceUtilityInfoForRefresh() {
		EventBus.$emit('show_mask', null);
		this.csUtilityInfo = {} as ResponseModel.ICustomerServiceUtilityInfo;

		InternalTestApi.getCustomerServiceUtilityInfo()
			.then((res) => {
				this.csUtilityInfo = res.data;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	getCustomerServiceUtilityInfo() {
		EventBus.$emit('show_mask', null);
		this.csUtilityInfo = {} as ResponseModel.ICustomerServiceUtilityInfo;

		InternalTestApi.getCustomerServiceUtilityInfo()
			.then((res) => {
				this.csUtilityInfoOfBasicInfo = res.data;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	changeGameUrlDomainByCS() {
		const msg = '是否操作修正域名封鎖(url blocked)?\n⚠️*此功能僅針對【[Vendor] TG X ION SLOT】，其他頻道請直接通知技術單位另外處理';
		if (confirm(msg)) {
			EventBus.$emit('show_mask', null);
			InternalTestApi.changeGameUrlDomainByCS()
				.then((res) => {
					this.isShowMessageHintAfterSubmit = true;
					this.resultMessageHtmlFormat =
						'<div>' +
						res.data.message +
						'</div>' +
						'<div>請協助驗證連結是否正常(可進入查看內容)</div>' +
						'<a href="' +
						res.data.resultExtraString +
						'" target="_blank">' +
						res.data.resultExtraString +
						'</a> (驗證此連結)' +
						'<div>若確認無異常。即可通知告戶調整完畢</div>';
				})
				.finally(() => {
					EventBus.$emit('hide_mask', null);
				});
		}
	}

	triggerBackendButtonAction(displayName: string, actionID: number) {
		this.backendButtonForm.backendButtonActionID = actionID;
		this.backendButtonForm.extraParameter = {} as ResponseModel.IBackendButtonActionExtraParameterRequest;
		this.backendButtonResponse = {} as ResponseModel.ICRUDResult;

		if (confirm('Are you sure to continue? (' + displayName + ')')) {
			EventBus.$emit('show_mask', null);
			InternalTestApi.executeBackendButtonActionForCS(this.backendButtonForm)
				.then((res) => {
					EventBus.$emit(`successfully-modified`);

					// 清空還原表單
					this.backendButtonForm.extraParameter = {} as ResponseModel.IBackendButtonActionExtraParameterRequest;
				})
				.finally(() => {
					EventBus.$emit('hide_mask', null);
				});
		}
	}

	isArrayExistTargerFormItem(targetColumnName: string) {
		if (!this.currentSelectBackendButton.requiredInput) {
			return false;
		}

		if (this.currentSelectBackendButton.requiredInput.length <= 0) {
			return;
		}

		const listToLower = this.currentSelectBackendButton.requiredInput.map((item) => {
			return item.toLowerCase();
		});

		targetColumnName = targetColumnName.toLowerCase();
		return listToLower.includes(targetColumnName);
	}

	async downloadGameList() {
		EventBus.$emit('show_mask', null);
		await InternalTestApi.downloadGameListFIle().finally(() => {
			setTimeout(() => {
				EventBus.$emit('hide_mask', null);
			}, 500);
		});
	}

	closeAllDialog() {
		this.isShowCreateTestAccount = false;
		this.isShowInputColumnForBackendButtons = false;
	}

	mounted() {
		this.init();
		this.setTextForCopy();

		DropdownApi.getInernalTestMerchants().then((res) => {
			this.dropdownInternalMerchants = res.map((s) => {
				return { value: Number(s.value), text: s.text };
			});
		});
		DropdownApi.getMerchants().then((res) => {
			this.dropdownMerchants = res;
		});

		DropdownApi.getGameVendors().then((res) => {
			this.dropdowGameVendorTypes = res;
		});
	}

	init() {
		this.getCustomerServiceUtilityInfo();
	}
}
</script>

<style lang="scss" scoped></style>
