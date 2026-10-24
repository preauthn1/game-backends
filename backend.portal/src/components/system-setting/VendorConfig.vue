<template>
	<div v-if="renderComponent" class="">
		<v-expansion-panels v-model="panel" multiple>
			<v-expansion-panel>
				<v-flex style="padding-left: 25px">
					<v-row class="search-form" style="margin-top: -20px">
						<v-col cols="12" sm="3" md="3">
							<div class="item">
								<v-text-field v-model="searchFilter.keyword" :label="$t('TableHeader.__keyword')" clearable> </v-text-field>
							</div>
						</v-col>

						<v-col cols="12" sm="3" md="3">
							<div class="item">
								<v-autocomplete clearable :items="dropdownGameVendors" v-model="searchFilter.vendorID" :label="$t('TableHeader.__gameVendorType')"> </v-autocomplete>
							</div>
						</v-col>

						<v-col cols="12" sm="3" md="3">
							<!-- <div class="item right"> -->
							<div class="mt-3 search-row-padding text-r">
								<v-btn color="yellow darken-1" text class="btn search mr-1" @click.stop="refresh()">
									<v-icon left>search</v-icon>
									{{ $t('Button.__search') }}
								</v-btn>

								<v-btn color="pink lighten-3" text class="btn search mr-1" @click.stop="clear()">
									<v-icon left>clear</v-icon>
									{{ $t('Button.__reset') }}
								</v-btn>

								<v-btn color="blue lighten-3" text class="btn search mr-1" @click.stop="exportExcel()">
									<v-icon left>cloud_download</v-icon>
									複製Excel格式
								</v-btn>
							</div>
						</v-col>
					</v-row>
				</v-flex>
			</v-expansion-panel>
		</v-expansion-panels>

		<v-data-table
			:headers="headers"
			id="table"
			:items="ListData"
			fixed-header
			height="62vh"
			:page="DataTablePagingParameter.pageIndex"
			:items-per-page="DataTablePagingParameter.itemsPerPage"
			:server-items-length="DataTablePagingParameter.totalCount"
			:footer-props="DataTablePagingParameter.footerProps"
			@update:page="changePage"
			@update:items-per-page="changeItemPerPage"
			multi-sort
		>
			<template v-slot:top>
				<v-toolbar height="50px">
					<v-toolbar-title>
						<h4>遊戲商設置 (VendorConfig)</h4>
					</v-toolbar-title>
					<v-divider class="mx-4" inset vertical></v-divider>
					<v-spacer></v-spacer>
					<v-card-title>
						<v-btn icon dark @click="refresh">
							<v-icon>refresh</v-icon>
						</v-btn>

						<v-btn icon dark @click="showCreateVendorConfig">
							<v-icon>add</v-icon>
						</v-btn>

						<v-btn icon dark @click="triggerSuperAdminPower">
							<v-icon>stars</v-icon>
						</v-btn>
					</v-card-title>
				</v-toolbar>
			</template>

			<template v-slot:item.action="{ item }">
				<v-menu offset-y>
					<template v-slot:activator="{ on, attrs }">
						<div class="dropdown-menu" v-bind="attrs" v-on="on">
							<v-icon dark outline> expand_more </v-icon>
						</div>
					</template>

					<v-list class="dropdonwMenuPanel">
						<v-list-item class="dropdownMenu">
							<v-list-item @click="showSettings(item)">
								<v-icon> edit </v-icon>
								查看/編輯設置
							</v-list-item>

							<v-list-item @click="showCopyConfirmPopup(item)">
								<v-icon> file_copy </v-icon>

								複製並新建立一筆
							</v-list-item>

							<v-list-item @click="syncUnassignMerchants(item)">
								<v-icon> autorenew </v-icon>
								自動分配商戶
							</v-list-item>
						</v-list-item>
					</v-list>
				</v-menu>
			</template>

			<template v-slot:item.isDefault="{ item }">
				<span v-if="item.isDefault">
					<v-chip color="green">預設</v-chip>
				</span>
			</template>

			<template v-slot:item.extraInfo="{ item }">
				<v-chip color="red darken-2" v-if="item.isVipLevelSettingNotExist"> VIP卡別未配置 </v-chip>
			</template>

			<template v-slot:item.exchangeRateToTWD="{ item }">
				<span v-if="item.exchangeRateToTWD > 0">
					{{ item.exchangeRateToTWD }}
				</span>
			</template>

			<template v-slot:item.isTestEnvironment="{ item }">
				<span v-if="item.isTestEnvironment">
					<v-chip color="grey darken-2">測試環境</v-chip>
				</span>
			</template>

			<template v-slot:item.isSkipFetchSummary="{ item }">
				<span v-if="item.isSkipFetchSummary">
					<v-chip color="red darken-4">關閉</v-chip>
				</span>
			</template>

			<template v-slot:item.assignVendorID="{ item }">
				{{ item.assignVendorID }}
				<v-chip small class="ml-2" color="brown darken-2" v-if="!isEmptyString(item.assignGameVendorDefineName)" title="與遊戲商溝通使用此名稱">
					{{ item.assignGameVendorDefineName }}
				</v-chip>
			</template>

			<template v-slot:item.createTime="{ item }">
				{{ item.createTime | timeWithAddTimezone(userProfile.timeZone) }}
			</template>

			<template v-slot:item.updateTime="{ item }">
				{{ item.updateTime | timeWithAddTimezone(userProfile.timeZone) }}
			</template>

			<template v-slot:item.numberOfMerchants="{ item }">
				<span @click="showMerchantsPopup(item)" class="link-text">
					{{ item.numberOfMerchants }}
				</span>
			</template>

			<template v-slot:item.name="{ item }">
				<div @click="showUpdateBasicInfoPopup(item)" class="pointer link-text-white" style="display: inline-block">
					{{ item.name }}
				</div>
			</template>

			<template v-slot:item.vendorName="{ item }">
				<v-chip small :class="'vendorID_' + item.vendorID" :title="item.vendorID">
					{{ item.vendorName }}
				</v-chip>
			</template>
		</v-data-table>

		<!-- 新增 -->
		<v-dialog v-model="isShowCreateVendorConfig" max-width="400px">
			<v-card class="group-settings">
				<v-card-title class="grey darken-2">
					<span>{{ $t('Popup.__create') }}</span>
					<div class="top-right-item" @click="close">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>
				<v-card-text>
					<v-flex>
						<v-row>
							<v-text-field class="required" v-model="createVendorCpnfigForm.name" :label="$t('TableHeader.__displayName')" autofocus required clearable> </v-text-field>
						</v-row>

						<v-row>
							<v-select class="required" attach chips :label="$t('TableHeader.__gameVendorType')" v-model="createVendorCpnfigForm.gameVendorID" :items="dropdownGameVendors"> </v-select>
						</v-row>

						<v-row>
							<v-checkbox :label="'是否為測試環境服務器'" v-model="createVendorCpnfigForm.isTestEnvironment"> </v-checkbox>
						</v-row>

						<v-row>
							<div style="height: 20px; padding-bottom: 150px">&nbsp;</div>
						</v-row>
					</v-flex>
				</v-card-text>

				<v-card-actions>
					<v-spacer></v-spacer>

					<v-btn text @click="close" class="btn">
						<v-icon left>mdi-cancel</v-icon>
						{{ $t('Button.__cancel') }}
					</v-btn>

					<v-btn text @click="saveCreateVendorConfig" color="yellow" class="btn">
						<v-icon left>mdi-check</v-icon>
						{{ $t('Button.__submit') }}
					</v-btn>
				</v-card-actions>
			</v-card>
		</v-dialog>

		<!-- 複製 -->
		<v-dialog v-model="isShowCopyConfirmDialog" max-width="550px">
			<v-card class="group-settings">
				<v-card-title class="grey darken-2">
					<span>{{ $t('Button.__copyNew') }}</span>
					<div class="top-right-item" @click="close">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>
				<v-card-text>
					<v-flex>
						<v-row class="mb-2">
							<div class="w100">
								<div class="common-info-label">
									{{ $t('TableHeader.__vendorConfig') }}
								</div>

								<div class="common-info-value">
									{{ SelectedCopySourceVendorConfig.name }}
								</div>
							</div>
						</v-row>

						<v-row class="mb-2">
							<div class="w100">
								<div class="common-info-label">
									{{ $t('TableHeader.__gameVendorType') }}
								</div>

								<div class="common-info-value">
									{{ SelectedCopySourceVendorConfig.vendorName }}
								</div>
							</div>
						</v-row>

						<v-row>
							<v-text-field class="required" v-model="copyForm.name" :label="$t('TableHeader.__displayName')" autofocus required clearable> </v-text-field>
						</v-row>

						<v-row>
							<v-text-field class="" v-model="copyForm.assignVendorID" :label="'识别代码(商戶號)'" required clearable> </v-text-field>
						</v-row>

						<v-row>
							<v-text-field class="" v-model="copyForm.supportCurrency" :label="'币别限制 (可填空值)'" required clearable> </v-text-field>
						</v-row>

						<v-row>
							<div style="height: 20px; padding-bottom: 100px">&nbsp;</div>
						</v-row>
					</v-flex>
				</v-card-text>

				<v-card-actions>
					<v-spacer></v-spacer>

					<v-btn text @click="close" class="btn">
						<v-icon left>mdi-cancel</v-icon>
						{{ $t('Button.__cancel') }}
					</v-btn>

					<v-btn text @click="confirmCopySubmit" color="yellow" class="btn">
						<v-icon left>mdi-check</v-icon>
						{{ $t('Button.__submit') }}
					</v-btn>
				</v-card-actions>
			</v-card>
		</v-dialog>

		<!-- 編輯進階設置 -->
		<v-dialog v-model="isShowEditDialog" max-width="800px" scrollable>
			<v-card class="group-settings">
				<v-card-title class="grey darken-2">
					<span>
						<b>
							{{ $t('Button.__advanceSetting') }}
						</b>
					</span>

					<span class="f12 ml-2">
						{{ TargetPopupVendorConfig.name }}
					</span>

					<span class="f12 grey-text ml-1"> ({{ TargetPopupVendorConfig.vendorConfigID }}) </span>

					<div class="top-right-item" @click="close">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>

				<v-card-text>
					<v-flex>
						<div v-for="(group, index) in Settings.groups" :key="index" class="eachGroup">
							<div>
								<v-row>
									<v-col cols="12" sm="6" md="6">
										<div class="groupName">
											{{ group.groupName }}
										</div>
									</v-col>

									<v-col cols="12" sm="6" md="6" class="saveBtnRow">
										<div v-show="!isEditModeOpen(group)">
											<v-btn medium disabled v-show="group.isAllDisableEdit"> {{ $t('Button.__disabledEdit') }}</v-btn>
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
												v-show="!item.isDropdownDataType"
												:label="item.settingKeyDisplayName + textOrEmptyString(item.memo)"
												v-model="item.value"
												:disabled="item.isDisableEdit || !isEditModeOpen(group)"
											>
											</v-text-field>

											<v-select
												v-show="item.isDropdownDataType"
												:disabled="item.isDisableEdit || !isEditModeOpen(group)"
												attach
												:label="item.settingKeyDisplayName + textOrEmptyString(item.memo)"
												v-model="item.value"
												:items="item.dataDropdowns"
											>
											</v-select>
										</v-col>
									</v-row>
								</div>
							</div>
						</div>
					</v-flex>
				</v-card-text>
			</v-card>
		</v-dialog>

		<!-- 已分配商戶 -->
		<v-dialog v-model="isShowMerchantDialog" max-width="650px">
			<v-card class="group-settings">
				<v-card-title class="grey darken-2">
					<span> 已分配商戶 </span>

					<span class="ml-2" style="color: #aaa; font-size: 14px"> ({{ TargetPopupItem.numberOfMerchants }}) </span>

					<div class="top-right-item">
						<v-btn color="yellow lighten-2" text outlined @click="copyAllMerchantIDs()">
							<v-icon>file_copy</v-icon>
							複製商戶IDs
						</v-btn>

						<v-btn color="blue lighten-2" text outlined @click="syncUnassignMerchants(TargetPopupItem)">
							<v-icon>autorenew</v-icon>
							指派預設Configs
						</v-btn>

						<v-btn icon dark @click="closeAllModal()">
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>

				<div style="padding: 15px; height: 600px; overflow-y: scroll">
					<div v-for="(item, index) in TargetPopupItem.mappingMerchants" :key="index">
						<v-chip small color="green darken-2" style="min-width: 50px; text-align: center" class="mr-1">
							{{ item.currency }}
						</v-chip>

						<span class="f10" style="color: #aaa">
							{{ item.merchantID }}
						</span>

						<div style="padding: 5px; min-width: 250px; display: inline-block" :title="item.merchantID">
							{{ item.merchantName }}
						</div>

						<v-icon @click="deleteMerchantMapping(item.merchantID)" color="red" title="delete"> close </v-icon>
					</div>
				</div>
			</v-card>
		</v-dialog>

		<!-- 編輯基本資訊 -->
		<v-dialog v-model="isShowUpdateBasicInfoDialog" max-width="450px">
			<v-card class="group-settings">
				<v-card-title class="grey darken-2">
					<span> 編輯基本資訊 </span>

					<span class="ml-2" style="color: #aaa; font-size: 14px"> ({{ updateBasicInfo.vendorConfigID }}) </span>

					<div class="top-right-item">
						<v-btn icon dark @click="closeAllModal()">
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>

				<v-card-text class="mt-4" style="min-height: 250px">
					<v-text-field :label="'名稱'" v-model="updateBasicInfo.name"> </v-text-field>

					<v-text-field :label="'支援幣別'" v-model="updateBasicInfo.supportCurrency"> </v-text-field>

					<v-checkbox :label="'是否為測試環境'" v-model="updateBasicInfo.isTestEnvironment"> </v-checkbox>

					<v-checkbox :label="'停用(關閉)Fetch三方帳務'" v-model="updateBasicInfo.isSkipFetchSummary"> </v-checkbox>
				</v-card-text>

				<v-card-actions>
					<v-spacer></v-spacer>
					<v-btn text @click="saveUpdateBasicInfo" color="yellow" class="btn">
						<v-icon left>mdi-check</v-icon>
						{{ $t('Button.__save') }}
					</v-btn>
				</v-card-actions>
			</v-card>
		</v-dialog>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import * as SettingModel from '@/models/interfaces/system-setting/systemConfigSetting';
import * as Model from '@/models/interfaces/system-setting/vendor';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import VendorApi from '@/api/system-setting/vendor';
import DropdownApi from '@/api/commons/dropdown';
import { Locale, Currency, Status, GameType, CRUDAction, GameVendorType } from '@/models/status/common';
import { WalletMode } from '@/models/status/merchant';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { commonMethodMixin } from '@/utilities/common-methods';
import { ALL_CURRENCY } from '@/utilities/enums_helper';
import NotificationWarning from '@/components/notification/NotificationWarning.vue';
import { pageData, time } from '@/utilities/api-handler.lib';
// import Validation from '@/utilities/validation';
import InternalTestApi from '@/api/commons/internalTest';
import { datetimeMixin, startEndDate, daysAgo } from '@/utilities/datetime-format';
import DatePicker from 'vue2-datepicker';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import { State, Action, Getter, namespace } from 'vuex-class';
import * as ResponseModel from '@/models/interfaces/commons/response';

const authModule = namespace('Auth');

import * as Permission from '@/models/status/permission';

// @ts-ignore
@Component({ components: { NotificationWarning, DatePicker }, mixins: [commonDisplayFiltersMixin, datetimeMixin, commonMethodMixin] })
export default class VendorConfig extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];
	renderComponent: boolean = true;
	isShowEditDialog: boolean = false;
	isShowMerchantDialog: boolean = false;
	isShowCreateVendorConfig: boolean = false;
	isShowCopyConfirmDialog: boolean = false;
	isShowUpdateBasicInfoDialog: boolean = false;

	createVendorCpnfigForm: Model.IVendorConfigCreateRequest = {} as Model.IVendorConfigCreateRequest;

	TargetPopupItem: Model.IVendorConfig = {} as Model.IVendorConfig;
	SelectedCopySourceVendorConfig: Model.IVendorConfig = {} as Model.IVendorConfig;
	TargetPopupVendorConfig: Model.IVendorConfig = {} as Model.IVendorConfig;

	panel: number[] = [0];
	copyForm: Model.IVendorConfigCopyRequest = {} as Model.IVendorConfigCopyRequest;

	searchFilter: Model.IVendorConfigPagingRequest = {} as Model.IVendorConfigPagingRequest;
	ListData: Model.IVendorConfig[] = [];
	Settings: SettingModel.ISystemConfigSetting = {} as SettingModel.ISystemConfigSetting;

	updateBasicInfo: Model.IVendorConfigBasicInfoUpdateRequest = {} as Model.IVendorConfigBasicInfoUpdateRequest;

	DataTablePagingParameter: DropdownModel.DataTablePagingParameter = new DropdownModel.DataTablePagingParameter();
	dropdownGameVendors: any[] = [];
	editModeOpenGroups: number[] = [];

	get headers() {
		const data = [
			{ text: this.$t('TableHeader.__ID'), value: 'vendorConfigID', sortable: false, align: 'start' },
			{ text: this.$t('TableHeader.__gameVendorType'), value: 'vendorName', sortable: false },
			{ text: this.$t('TableHeader.__displayName'), value: 'name', sortable: false },
			{ text: '識別代碼/識別編號', value: 'assignVendorID', sortable: false },
			{ text: this.$t('TableHeader.__assignMerchantTotalCount'), value: 'numberOfMerchants', sortable: false, align: 'center' },
			{ text: this.$t('TableHeader.__isDefault'), value: 'isDefault', sortable: false, align: 'center' },
			{ text: this.$t('TableHeader.__isTestEnv'), value: 'isTestEnvironment', sortable: false, align: 'center' },
			{ text: '關閉Fetch帳務', value: 'isSkipFetchSummary', sortable: false, align: 'center' },
			{ text: this.$t('TableHeader.__restrictCurrency'), value: 'supportCurrency', sortable: false, align: 'center' },
			{ text: '兌換台幣', value: 'exchangeRateToTWD', sortable: false, align: 'end' },
			{ text: ' ', value: 'extraInfo', sortable: false },
		];

		if (true) {
			data.splice(0, 0, { text: this.$t('TableHeader.__action'), value: 'action', sortable: false, align: 'center' });
		}
		return data;
	}

	showUpdateBasicInfoPopup(item: Model.IVendorConfig) {
		this.isShowUpdateBasicInfoDialog = true;
		this.updateBasicInfo.vendorConfigID = item.vendorConfigID;
		this.updateBasicInfo.name = item.name;
		this.updateBasicInfo.supportCurrency = item.supportCurrency;
		this.updateBasicInfo.isTestEnvironment = item.isTestEnvironment;
		this.updateBasicInfo.isSkipFetchSummary = item.isSkipFetchSummary;
	}

	saveUpdateBasicInfo() {
		EventBus.$emit('show_mask', null);
		VendorApi.updateVendorConfigBasicInfo(this.updateBasicInfo)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.closeAllModal();
				this.refresh();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	showCopyConfirmPopup(item: Model.IVendorConfig) {
		this.isShowCopyConfirmDialog = true;
		this.SelectedCopySourceVendorConfig = item;
		this.copyForm.assignVendorID = item.assignVendorID;
	}

	confirmCopySubmit(item: Model.IVendorConfig) {
		EventBus.$emit('show_mask', null);

		this.copyForm.copyFromVendorConfigID = this.SelectedCopySourceVendorConfig.vendorConfigID;

		VendorApi.copyAndCreateNewVendorConfig(this.copyForm)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.closeAllModal();
				this.refresh();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	refresh() {
		this.registerReloadComponent();

		this.searchFilter.pageIndex = this.DataTablePagingParameter.pageIndex;
		this.searchFilter.pageSize = this.DataTablePagingParameter.itemsPerPage;
		this.searchFilter.orderByColumn = 'vendorID,vendorConfigID';

		EventBus.$emit('show_mask', null);
		VendorApi.getVendorConfig(this.searchFilter)
			.then((res) => {
				this.ListData = res.data.items;
				this.DataTablePagingParameter.totalCount = res.data.totalCount;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	showMerchantsPopup(item: Model.IVendorConfig) {
		this.isShowMerchantDialog = true;
		this.getSingleDetail(item.vendorConfigID);
	}

	getSingleDetail(vendorConfigID: number) {
		EventBus.$emit('show_mask', null);

		VendorApi.getSingleDetail(vendorConfigID)
			.then((res) => {
				this.TargetPopupItem = res.data;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	exportExcel() {
		EventBus.$emit('show_mask', null);

		let myText = '';

		this.ListData.forEach((item) => {
			let concateLineText = '';

			concateLineText = concateLineText + item.vendorConfigID.toString() + '\t';
			concateLineText = concateLineText + item.vendorID.toString() + '\t';
			concateLineText = concateLineText + item.name + '\t';
			concateLineText = concateLineText + item.assignVendorID + '\t';
			concateLineText = concateLineText + '\n';

			myText = myText + concateLineText;
		});

		const vm: any = this;
		vm.copyTextToClipboard(myText);

		setTimeout(() => {
			EventBus.$emit('hide_mask', null);
		}, 500);
	}

	clear() {
		this.searchFilter = {} as Model.IVendorConfigPagingRequest;
	}

	registerReloadComponent() {
		EventBus.$on(`reload-${this.$router.currentRoute.path}`, (info: any) => {
			this.renderComponent = false;
			this.$nextTick(() => {
				this.renderComponent = true;
			});
		});
	}

	copyAllMerchantIDs() {
		let copyText = '';

		for (const item of this.TargetPopupItem.mappingMerchants) {
			copyText += item.merchantID + '\n';
		}

		const vm: any = this;
		vm.copyTextToClipboard(copyText);
	}

	showSettings(item: Model.IVendorConfig) {
		this.isShowEditDialog = true;
		this.TargetPopupVendorConfig = item;
		this.getSettings(item.vendorConfigID);
	}

	getSettings(vendorConfigID: number) {
		EventBus.$emit('show_mask', null);
		this.Settings = {} as SettingModel.ISystemConfigSetting;
		VendorApi.getVendorConfigSettings(vendorConfigID)
			.then((res) => {
				this.Settings = res.data;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	isEditModeOpen(item: SettingModel.ISystemConfigSettingGroup) {
		let isOpen = false;
		this.editModeOpenGroups.forEach((element) => {
			if (element === item.groupID) {
				isOpen = true;
			}
		});

		return isOpen;
	}

	textOrEmptyString(myText: string) {
		if (myText == null || myText.length <= 0) {
			return '';
		}

		return myText;
	}

	opneEditMode(item: SettingModel.ISystemConfigSettingGroup) {
		this.editModeOpenGroups.push(item.groupID);
	}

	closeEditMode(item: SettingModel.ISystemConfigSettingGroup) {
		this.editModeOpenGroups = this.editModeOpenGroups.filter((x) => x !== item.groupID);
	}

	saveAdvanceSetting(item: SettingModel.ISystemConfigSettingGroup) {
		const vm = this;

		const postItems: SettingModel.ISystemConfigSettingUpdateRequestItem[] = [];

		item.items.forEach((element) => {
			console.log(element);
			postItems.push(<SettingModel.ISystemConfigSettingUpdateRequestItem>{
				value: element.value,
				settingKey: element.settingKey,
			});
		});

		EventBus.$emit('show_mask', null);

		const payload = <SettingModel.ISystemConfigSettingRequest>{
			ownerID: this.Settings.ownerID,
			items: postItems,
		};

		VendorApi.updateVendorConfigSettings(payload)
			.then(() => {
				EventBus.$emit(`successfully-modified`);
				vm.closeEditMode(item);
				vm.refresh();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	syncUnassignMerchants(item: Model.IVendorConfig) {
		const vm = this;
		if (confirm('是否將未配置的商戶，都分配至當前選擇設置底下？ (' + item.name + ')')) {
			VendorApi.syncUnassignMerchants(item.vendorConfigID).then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.refresh();
			});
		}
	}

	triggerSuperAdminPower() {
		if (confirm('是否執行工程師特殊權限?')) {
			EventBus.$emit('show_mask', null);

			InternalTestApi.executeBackendButtonAction(<ResponseModel.IBackendButtonActionRequest>{
				backendButtonActionID: 9901, // 短時間開通可編輯欄位(工程師專用)
			})
				.then((res) => {
					EventBus.$emit(`successfully-modified`);
				})
				.finally(() => {
					EventBus.$emit('hide_mask', null);
				});
		}
	}

	changePage(index: number = 1) {
		this.DataTablePagingParameter.pageIndex = index;
		this.refresh();
	}

	changeItemPerPage(val: number = 10) {
		this.DataTablePagingParameter.itemsPerPage = val;
		this.refresh();
	}

	resetFormValidate() {
		const form: any = this.$refs.loginForm;
		form.resetValidation();
	}

	showCreateVendorConfig() {
		this.isShowCreateVendorConfig = true;

		this.createVendorCpnfigForm = {} as Model.IVendorConfigCreateRequest;
	}

	saveCreateVendorConfig() {
		VendorApi.createEmptyVendorConfig(this.createVendorCpnfigForm)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.refresh();
				this.close();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	deleteMerchantMapping(merchantID: number) {
		const msg: any = this.$t('Message.__isConfirmDelete') + '? ';

		if (confirm(msg)) {
			EventBus.$emit('show_mask', null);

			VendorApi.deleteMerchantMapping(this.TargetPopupItem.vendorConfigID, merchantID)
				.then((res) => {
					EventBus.$emit(`successfully-modified`);
					this.refresh();
					this.close();
				})
				.finally(() => {
					EventBus.$emit('hide_mask', null);
				});
		}
	}

	close() {
		this.closeAllModal();
	}

	closeAllModal() {
		this.isShowCopyConfirmDialog = false;
		this.isShowCreateVendorConfig = false;
		this.isShowMerchantDialog = false;
		this.isShowEditDialog = false;
		this.isShowUpdateBasicInfoDialog = false;
	}

	save() {}

	mounted() {
		const maxPageSize: number = 200;
		this.DataTablePagingParameter.footerProps = { 'items-per-page-options': [10, maxPageSize] };
		this.DataTablePagingParameter.itemsPerPage = maxPageSize;
		DropdownApi.getThirdVendors().then((res) => {
			this.dropdownGameVendors = res;
		});
	}
}
</script>

<style scoped>
.search-form .col-md-3 {
	padding: 0px 10px 0px 10px;
}

.vendorID_290000 {
	background: #2c4088 !important;
}

.vendorID_330000 {
	background: #2e5038 !important;
}

.vendorID_250000 {
	background: #5f3a3a !important;
}
.vendorID_370000 {
	background: #4a7c7b !important;
}
.vendorID_430000 {
	background: #5a436f !important;
}
.vendorID_390000 {
	background: #6f1515 !important;
}

.vendorID_450000 {
	background: #2e5038 !important;
}
.vendorID_490000 {
	background: #2c4088 !important;
}
.vendorID_550000 {
	background: #4a7c7b !important;
}
.vendorID_570000 {
	background: #6f1515 !important;
}
.vendorID_590000 {
	background: #2c4088 !important;
}

.vendorID_610000 {
	background: #2e5038 !important;
}
.vendorID_630000 {
	background: #8b4318 !important;
}
</style>
