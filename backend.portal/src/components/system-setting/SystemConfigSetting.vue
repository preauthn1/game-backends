<template>
	<v-app v-if="renderComponent" class="app">
		<v-row>
			<v-col cols="12" sm="7" md="7">
				<v-card v-if="hasPermissionSystemConfigSettingUpdate">
					<v-card-text class="advancePanelContent group-settings">
						<v-flex>
							<div v-for="(group, index) in systemConfigSettings.groups" :key="index" class="eachGroup">
								<div>
									<v-row>
										<v-col cols="12" sm="6" md="6">
											<div class="groupName">
												{{ group.groupName }}
											</div>
										</v-col>

										<v-col cols="12" sm="6" md="6" class="saveBtnRow">
											<div v-show="!isEditModeOpen(group)">
												<v-btn medium v-show="group.isAllDisableEdit">
													{{ $t('Button.__disabledEdit') }}
												</v-btn>
												<v-btn medium v-show="!group.isAllDisableEdit" @click="opneEditMode(group)"> {{ $t('Popup.__update') }}</v-btn>
											</div>

											<div v-show="isEditModeOpen(group)">
												<v-btn medium @click="saveAdvanceSetting(group)" color="primary"> {{ $t('Button.__submit') }}</v-btn>
												<v-btn medium @click="closeEditMode(group)" color="#666"> {{ $t('Button.__cancel') }}</v-btn>
											</div>
										</v-col>
									</v-row>
								</div>

								<div class="groupItem">
									<div v-for="(item, index2) in group.items" :key="index2">
										<v-row>
											<v-col cols="12" sm="8" md="8" :title="item.settingKey">
												<v-text-field
													v-if="!item.isDropdownDataType"
													:label="item.settingKeyDisplayName"
													v-model="item.value"
													:disabled="item.isDisableEdit || !isEditModeOpen(group)"
												></v-text-field>

												<v-select
													v-if="item.isDropdownDataType"
													:disabled="item.isDisableEdit || !isEditModeOpen(group)"
													attach
													:label="item.settingKeyDisplayName"
													v-model="item.value"
													:items="item.dataDropdowns"
												></v-select>
											</v-col>
										</v-row>
									</div>
								</div>
							</div>
						</v-flex>
					</v-card-text>
				</v-card>
			</v-col>

			<v-col cols="12" sm="5" md="5">
				<div>
					<v-btn color="red darken-1" text class="btn search" v-if="hasPermissionSystemConfigSettingUpdate" @click.stop="removeAllCache" style="width: 150px">
						<v-icon left>refresh</v-icon>
						清除Redis快取
					</v-btn>
				</div>

				<div style="margin-top: 10px">
					<v-btn color="blue darken-1" text class="btn search" @click.stop="showCreateTestAccountDiaolog" style="width: 150px">
						<v-icon left>add</v-icon>
						新增測試玩家帳號
					</v-btn>
				</div>

				<div style="margin-top: 10px" v-for="(item, index) in backendActionButtons.items" :key="index">
					<v-btn color="grey lighten-1" text class="btn search" :title="item.backendButtonActionID" @click.stop="triggerBackendButtonActionOrShowDialog(item)">
						<v-icon left>{{ getButtonIcon(item.backendButtonActionID) }}</v-icon>
						{{ item.displayName }}
					</v-btn>

					<v-tooltip top v-if="item.infoDescription && item.infoDescription != null">
						<template v-slot:activator="{ on, attrs }">
							<v-icon color="grey" class="ml-1" dark v-bind="attrs" v-on="on" small> error </v-icon>
						</template>
						<span>{{ item.infoDescription }} </span>
					</v-tooltip>
				</div>
			</v-col>
		</v-row>

		<!-- 新增測試玩家帳號 -->
		<v-dialog v-model="isShowCreateTestAccount" max-width="600px">
			<v-card>
				<v-card-title class="grey darken-2"> 新增測試玩家帳號 </v-card-title>
				<v-spacer></v-spacer>

				<v-row style="padding: 10px 50px">
					<v-col cols="12" sm="12" md="12">
						<v-autocomplete
							class="required"
							attach
							:label="$t('TableHeader.__merchant')"
							clearable
							chips
							v-model="createTestAccountForm.merchantID"
							:items="dropdownMerchantsAllowCreateTestAccount"
						>
						</v-autocomplete>
					</v-col>

					<v-col cols="12" sm="12" md="12">
						<v-text-field ref="depositWalletAmountText" v-model="createTestAccountForm.playerName" :label="$t('TableHeader.__playerAccount')" clearable maxlength="25"> </v-text-field>
					</v-col>

					<v-col cols="12" sm="12" md="12">
						<v-checkbox v-model="createTestAccountForm.isAddToInternalTestAccount" :label="'自動標記以下用戶標籤:'"> </v-checkbox>

						<ul style="margin-top: -15px; margin-left: 20px; color: #7ed781">
							<li>常用測試帳號</li>
							<li>允許維護期間進行遊戲</li>
						</ul>
					</v-col>
				</v-row>

				<v-card-actions>
					<v-spacer></v-spacer>

					<v-btn text @click="closeAllDialog" class="btn">
						<v-icon left>mdi-cancel</v-icon>
						{{ $t('Button.__close') }}
					</v-btn>

					<v-btn text @click="saveCreateTestAccount" class="btn yellow--text">
						<v-icon left>mdi-check-circle</v-icon>
						{{ $t('Button.__submit') }}
					</v-btn>
				</v-card-actions>
			</v-card>
		</v-dialog>

		<!-- 後端客製按鈕行為(欄位填寫) -->
		<v-dialog v-model="isShowInputColumnForBackendButtons" max-width="700px" scrollable>
			<v-card style="padding-bottom: 10px">
				<v-card-title class="grey darken-2">
					{{ currentSelectBackendButton.displayName }}
					<span class="grey-text f12 ml-2"> ({{ currentSelectBackendButton.backendButtonActionID }}) </span>
				</v-card-title>
				<v-spacer></v-spacer>

				<v-card-text>
					<div style="padding: 15px; white-space: pre-line" class="f12" v-if="currentSelectBackendButton.memoHint != null && currentSelectBackendButton.memoHint.length > 0">
						<v-icon small>info</v-icon> {{ currentSelectBackendButton.memoHint }}
					</div>

					<v-row style="padding: 10px 50px" class="mr-0" v-if="isArrayExistTargerFormItem('gameID')">
						<v-col cols="12" sm="11" md="11">
							<v-autocomplete class="required" attach :label="$t('TableHeader.__game')" v-model="backendButtonForm.extraParameter.gameID" :items="dropdowGames"> </v-autocomplete>
						</v-col>
					</v-row>

					<v-row style="padding: 10px 50px" class="mr-0" v-if="isArrayExistTargerFormItem('merchantID')">
						<v-col cols="12" sm="11" md="11">
							<v-autocomplete class="required" attach :label="$t('TableHeader.__merchant')" v-model="backendButtonForm.extraParameter.merchantID" :items="dropdownMerchants">
							</v-autocomplete>
						</v-col>
					</v-row>

					<v-row style="padding: 10px 50px" class="mr-0" v-if="isArrayExistTargerFormItem('perGameTagID')">
						<v-col cols="12" sm="11" md="11">
							<v-autocomplete class="required" attach :label="$t('TableHeader.__tag')" v-model="backendButtonForm.extraParameter.tagID" :items="dropdowPerGameTags"> </v-autocomplete>
						</v-col>
					</v-row>

					<v-row style="padding: 10px 50px" class="mr-0" v-if="isArrayExistTargerFormItem('gameVendorType')">
						<v-col cols="12" sm="11" md="11">
							<v-autocomplete
								class="required"
								attach
								:label="$t('TableHeader.__gameVendorType')"
								v-model="backendButtonForm.extraParameter.gameVendorType"
								:items="dropdowGameVendorTypes"
							>
							</v-autocomplete>
						</v-col>
					</v-row>

					<v-row style="padding: 10px 50px" class="mr-0" v-if="isArrayExistTargerFormItem('playerID')">
						<v-col cols="12" sm="11" md="11">
							<v-text-field class="required" attach :label="$t('TableHeader.__playerID')" v-model="backendButtonForm.extraParameter.playerID"> </v-text-field>
						</v-col>
					</v-row>

					<v-row style="padding: 10px 50px" class="mr-0" v-if="isArrayExistTargerFormItem('dateRange')">
						<v-col cols="12" sm="11" md="11">
							<div class="f12 grey-text">輸入時間區間</div>
							<date-picker v-model="datetimeRangePicker.searchStartTimeRange" type="datetime" value-type="timestamp" range :placeholder="$t('TableHeader.__time')"> </date-picker>
						</v-col>
					</v-row>

					<v-row style="padding: 10px 50px" class="mr-0" v-if="isArrayExistTargerFormItem('date')">
						<v-col cols="12" sm="11" md="11">
							<div class="f12 grey-text">輸入時間</div>

							<date-picker v-model="backendButtonForm.extraParameter.dateTS" type="datetime" @change="whenChangeDate()" value-type="timestamp" format="YYYY-MM-DD HH:mm"> </date-picker>
						</v-col>
					</v-row>

					<v-row style="padding: 10px 50px" class="mr-0" v-if="isArrayExistTargerFormItem('targetTable')">
						<v-col cols="12" sm="11" md="11">
							<v-autocomplete class="required" attach :label="'輸入Table'" v-model="backendButtonForm.extraParameter.targetTable" :items="currentSelectBackendButton.myExtraDropdown">
							</v-autocomplete>
						</v-col>
					</v-row>

					<v-row style="padding: 10px 50px" class="mr-0" v-if="isArrayExistTargerFormItem('text')">
						<v-col cols="12" sm="11" md="11">
							<v-text-field class="required" attach :label="'文字欄位／單號'" clearable v-model="backendButtonForm.extraParameter.text"> </v-text-field>
						</v-col>
					</v-row>

					<v-row style="padding: 10px 50px" class="mr-0" v-if="isArrayExistTargerFormItem('text2')">
						<v-col cols="12" sm="11" md="11">
							<v-text-field class="required" attach :label="'文字欄位／單號 (2)'" clearable v-model="backendButtonForm.extraParameter.text2"> </v-text-field>
						</v-col>
					</v-row>

					<v-row style="padding: 10px 50px" class="mr-0" v-if="isArrayExistTargerFormItem('numberText')">
						<v-col cols="12" sm="11" md="11">
							<v-text-field class="required" attach :label="'請輸入數字'" clearable v-model="backendButtonForm.extraParameter.numberText"> </v-text-field>
						</v-col>
					</v-row>

					<v-row style="padding: 10px 30px 5px 50px" class="mr-0" v-if="isArrayExistTargerFormItem('textArea')">
						<v-col cols="12" sm="12" md="12">
							<div class="my-text-area">
								<v-textarea
									class="required"
									outlined
									rows="8"
									:label="'貼上文字'"
									v-model="backendButtonForm.extraParameter.textArea"
									:placeholder="this.currentSelectBackendButton.memoHint"
								>
								</v-textarea>
							</div>
						</v-col>
					</v-row>

					<v-row style="padding: 10px 30px 5px 50px" class="mr-0" v-if="isArrayExistTargerFormItem('textArea2')">
						<v-col cols="12" sm="12" md="12">
							<div class="my-text-area">
								<v-textarea
									class="required"
									outlined
									rows="8"
									:label="'貼上文字'"
									v-model="backendButtonForm.extraParameter.textArea2"
									:placeholder="this.currentSelectBackendButton.memoHint"
								>
								</v-textarea>
							</div>
						</v-col>
					</v-row>

					<v-row style="padding: 10px 30px 10px 50px" class="mr-0">
						<div v-if="backendButtonResponse.message && backendButtonResponse.message != null && backendButtonResponse.message != ''">
							<div>
								<h3 class="yellow-text">⭐返回結果顯示訊息: (可複製)</h3>
							</div>
							<div
								style="width: 525px; font-size: 14px; color: #bbb; margin-top: 5px; white-space: pre-line"
								class="pointer"
								@click="copyTextToClipboard2(backendButtonResponse.message)"
							>
								{{ backendButtonResponse.message }}
							</div>
						</div>
					</v-row>
				</v-card-text>

				<v-card-actions style="padding: 10px">
					<v-spacer></v-spacer>

					<v-btn text @click="closeAllDialog" class="btn white--text">
						<v-icon left>close</v-icon>
						{{ $t('Button.__cancel') }}
					</v-btn>

					<v-btn text @click="triggerBackendButtonActioAfterInputComplete" class="btn yellow--text">
						<v-icon left>mdi-check-circle</v-icon>
						{{ $t('Button.__submit') }}
					</v-btn>
				</v-card-actions>
			</v-card>
		</v-dialog>

		<NotificationWarning v-show="showNotificationWarning" :title="`${$t('Message.__warning')}`" :max="1" />
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
import DatePicker from 'vue2-datepicker';
const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';
import { daysAgo, toLocalDateString, startEndDate } from '@/utilities/datetime-format';

@Component({ components: { NotificationWarning, DatePicker }, mixins: [commonMethodMixin] })
export default class ServersRobotConfig extends Vue {
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];
	@authModule.State('defaultLangTimes') defaultLangTimes!: number;

	isShowCreateTestAccount: boolean = false;
	isShowInputColumnForBackendButtons: boolean = false;

	renderComponent: boolean = true;
	showNotificationWarning: boolean = false;
	editModeOpenGroups: number[] = [];

	dropdownMerchantsAllowCreateTestAccount: any[] = [];
	dropdownInternalMerchants: any[] = [];
	dropdownMerchants: any[] = [];
	dropdowGameVendorTypes: any[] = [];
	dropdowGames: any[] = [];
	dropdowPerGameTags: any[] = [];

	datetimeRangePicker: DropdownModel.DatetimeRangePicker = new DropdownModel.DatetimeRangePicker();

	createTestAccountForm: MemberModel.IInternalTestAccountCreateRequest = {} as MemberModel.IInternalTestAccountCreateRequest;
	backendActionButtons: ResponseModel.IBackendButtonResponse = {} as ResponseModel.IBackendButtonResponse;
	currentSelectBackendButton: ResponseModel.IBackendButtonItem = {} as ResponseModel.IBackendButtonItem;
	backendButtonForm: ResponseModel.IBackendButtonActionRequest = {} as ResponseModel.IBackendButtonActionRequest;
	backendButtonResponse: ResponseModel.ICRUDResult = {} as ResponseModel.ICRUDResult;

	systemConfigSettings: Model.ISystemConfigSetting = <Model.ISystemConfigSetting>{};

	get dropdowTables() {
		const myItems: any[] = [];
		myItems.push({ text: 'GameList', value: 'GameList' });
		return myItems;
	}

	get hasPermissionSystemConfigSettingUpdate() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSetting_SystemConfigSetting_Update) >= 0;
	}

	whenChangeDate() {
		this.isShowInputColumnForBackendButtons = false;
		this.isShowInputColumnForBackendButtons = true;
	}

	getButtonIcon(buttonID: number) {
		if (buttonID === 1001 || buttonID === 1009 || buttonID === 1300) {
			return 'logout';
		}

		if (buttonID === 1500 || buttonID === 1504 || buttonID === 11111111) {
			return 'delete';
		}

		if (buttonID >= 3166 && buttonID <= 3249) {
			return 'mdi-settings';
		}

		if (buttonID >= 9901 && buttonID <= 9999) {
			return 'stars';
		}

		if (buttonID === 3351) {
			return 'person';
		}

		return 'add';
	}

	triggerBackendButtonActioAfterInputComplete() {
		EventBus.$emit('show_mask', null);

		this.backendButtonForm.extraParameter.begin = null;
		this.backendButtonForm.extraParameter.end = null;

		if (this.isArrayExistTargerFormItem('dateRange')) {
			this.backendButtonForm.extraParameter.begin = startEndDate(this.datetimeRangePicker.searchStartTimeRange[0]);
			this.backendButtonForm.extraParameter.end = startEndDate(this.datetimeRangePicker.searchStartTimeRange[1]);
		}

		if (this.isArrayExistTargerFormItem('date')) {
			this.backendButtonForm.extraParameter.date = startEndDate(this.backendButtonForm.extraParameter.dateTS);
		}

		if (this.backendButtonForm.backendButtonActionID === 3222) {
			EventBus.$emit('show_mask', null);

			const url = '/backoffice/internal/backend-buttons/action';

			InternalTestApi.httpPostDownloadFile(url, this.backendButtonForm)
				.then((res) => {
					EventBus.$emit(`successfully-modified`);
				})
				.finally(() => {
					EventBus.$emit('hide_mask', null);
				});
		} else {
			InternalTestApi.executeBackendButtonAction(this.backendButtonForm)
				.then((res) => {
					this.backendButtonResponse = res.data;
					console.log(this.backendButtonResponse);

					EventBus.$emit(`successfully-modified`);

					// 特殊顯示
					if (this.backendButtonForm.backendButtonActionID === 1530) {
						const myArray: any[] = [];

						if (res.data.resultListObject != null && res.data.resultListObject.length > 0) {
							res.data.resultListObject.forEach((item) => {
								myArray.push({ displayName: item.key, value: item.value });
							});
						}

						EventBus.$emit(`globalShowSelfDefineColumnsDisplayPopupDialog`, { title: '查詢結果(最多20筆)', items: myArray });
					}
				})
				.finally(() => {
					EventBus.$emit('hide_mask', null);
				});
		}
	}

	triggerBackendButtonActionOrShowDialog(item: ResponseModel.IBackendButtonItem) {
		this.backendButtonForm.backendButtonActionID = item.backendButtonActionID;
		this.backendButtonForm.extraParameter = {} as ResponseModel.IBackendButtonActionExtraParameterRequest;
		this.backendButtonResponse = {} as ResponseModel.ICRUDResult;
		this.backendButtonForm.extraParameter.dateTS = daysAgo(0);

		if (!item.requiredInput || item.requiredInput.length <= 0) {
			if (confirm('是否確定執行:' + item.displayName + '?')) {
				EventBus.$emit('show_mask', null);

				console.log('begin');
				console.log(+new Date());

				InternalTestApi.executeBackendButtonAction(this.backendButtonForm)
					.then((res) => {
						EventBus.$emit(`successfully-modified`);

						// 清空還原表單
						this.backendButtonForm.extraParameter = {} as ResponseModel.IBackendButtonActionExtraParameterRequest;
					})
					.finally(() => {
						EventBus.$emit('hide_mask', null);

						console.log('end');
						console.log(+new Date());
					});
			}
		} else {
			this.currentSelectBackendButton = item;
			this.isShowInputColumnForBackendButtons = true;
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

	isEditModeOpen(item: Model.ISystemConfigSettingGroup) {
		let isOpen = false;
		this.editModeOpenGroups.forEach((element) => {
			if (element === item.groupID) {
				isOpen = true;
			}
		});

		return isOpen;
	}

	opneEditMode(item: Model.ISystemConfigSettingGroup) {
		this.getSystemConfigSettings();
		this.editModeOpenGroups.push(item.groupID);
	}

	closeEditMode(item: Model.ISystemConfigSettingGroup) {
		this.editModeOpenGroups = this.editModeOpenGroups.filter((x) => x !== item.groupID);
	}

	showCreateTestAccountDiaolog() {
		this.isShowCreateTestAccount = true;
		this.createTestAccountForm.merchantID = this.dropdownInternalMerchants[0].value;
		this.createTestAccountForm.playerName = '';
		this.createTestAccountForm.isAddToInternalTestAccount = true;
	}

	saveCreateTestAccount() {
		EventBus.$emit('show_mask', null);

		PlayersApi.createInernalTestAccount(this.createTestAccountForm)
			.then(() => {
				EventBus.$emit(`successfully-modified`);
				this.closeAllDialog();
				this.getSystemConfigSettings();

				const vm: any = this;
				vm.forceClosePageMenuByPath('/system-setting/gameList');
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	saveAdvanceSetting(item: Model.ISystemConfigSettingGroup) {
		const vm = this;

		const postItems: Model.ISystemConfigSettingUpdateRequestItem[] = [];

		item.items.forEach((element) => {
			console.log(element);
			postItems.push(<Model.ISystemConfigSettingUpdateRequestItem>{
				value: element.value,
				settingKey: element.settingKey,
			});
		});

		EventBus.$emit('show_mask', null);

		systemConfigSettingApi
			.updateSystemConfigSetting(<Model.ISystemConfigSettingRequest>{
				items: postItems,
			})
			.then(() => {
				EventBus.$emit(`successfully-modified`);
				vm.closeEditMode(item);
				location.reload();
			})
			.finally(() => {
				setTimeout(() => {
					EventBus.$emit('hide_mask', null);
				}, 1000 * 3);
			});
	}

	closeAllDialog() {
		this.isShowCreateTestAccount = false;
		this.isShowInputColumnForBackendButtons = false;
	}

	getSystemConfigSettings() {
		systemConfigSettingApi.getSystemConfigSettings().then((res) => {
			this.systemConfigSettings = res.data;
		});
	}

	removeAllCache() {
		systemConfigSettingApi.removeAllCache().then((res) => {
			alert('成功');
		});
	}

	debugGetPackageInfo() {
		// console.log('----version---');
		// try {
		// 	const pjson = require('./package.json');
		// 	console.log(pjson.version);
		// } catch (e) {
		// 	console.log(e);
		// }
	}

	registerReloadComponentEvent() {
		const eventName = `reload-${this.$router.currentRoute.path}`;

		EventBus.$on(eventName, (info: any) => {
			// 需要稍微延遲下、不然會異常
			setTimeout(() => {
				this.init();
				this.initialDropdown();
			}, 250);
		});
	}

	mounted() {
		this.registerReloadComponentEvent();
		this.init();
		this.initialDropdown();
		this.debugGetPackageInfo();
	}

	init() {
		this.getSystemConfigSettings();

		this.datetimeRangePicker.searchStartTimeRange = [daysAgo(-1), daysAgo(0)];
	}

	initialDropdown() {
		DropdownApi.getInernalTestMerchants().then((res) => {
			this.dropdownInternalMerchants = res.map((s) => {
				return { value: Number(s.value), text: s.text };
			});
		});

		DropdownApi.getMerchants().then((res) => {
			this.dropdownMerchants = res;
		});

		DropdownApi.getGames().then((res) => {
			this.dropdowGames = res;
		});

		DropdownApi.getGameVendors().then((res) => {
			this.dropdowGameVendorTypes = res;
		});

		InternalTestApi.getBackendActionButtons().then((res) => {
			this.backendActionButtons = res.data;
		});

		DropdownApi.getPerGameTags().then((res) => {
			this.dropdowPerGameTags = res;
		});

		DropdownApi.getMerchantsAllowCreateTestAccount().then((res) => {
			this.dropdownMerchantsAllowCreateTestAccount = res;
		});
	}
}
</script>

<style lang="scss" scoped>
.my-text-area textarea {
	height: 300px !important;
}
</style>
