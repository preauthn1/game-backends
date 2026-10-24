<template>
	<!-- 進階設置 -->
	<v-card>
		<v-card-title class="grey darken-2">
			{{ $t('Button.__advanceSetting') }}

			<v-chip small class="ml-2" color="brown darken-1"> {{ merchantAdvanceSetting.ownerID }}｜{{ merchantAdvanceSetting.ownerName }} </v-chip>

			<div class="top-right-item" @click="closeMerchantAdvanceSetting">
				<v-btn icon dark>
					<v-icon>close</v-icon>
				</v-btn>
			</div>
		</v-card-title>

		<v-card-text class="advancePanelContent advancePanelContent group-settings" style="height: 750px; overflow-y: scroll; padding-bottom: 100px">
			<div style="padding-bottom: 150px">
				<v-flex v-if="isAdvanceLoadComplete && merchantAdvanceSetting && merchantAdvanceSetting.groups">
					<div v-for="(group, index) in merchantAdvanceSetting.groups" :key="index" class="eachGroup">
						<div>
							<v-row>
								<v-col cols="12" sm="7" md="7">
									<div class="groupName">
										{{ group.groupName }}
									</div>
								</v-col>

								<v-col cols="12" sm="5" md="5" class="saveBtnRow">
									<div v-show="!isEditModeOpen(group)">
										<v-btn medium v-show="group.isAllDisableEdit" disabled>
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
									<v-col cols="12" sm="10" md="10" :title="item.settingKey">
										<v-text-field
											v-if="!item.isDropdownDataType"
											:label="item.settingKeyDisplayName"
											v-model="item.value"
											:placeholder="item.settingKey === 7001 ? 'e.g. https://example.invalid/seamless' : ''"
											:disabled="item.isDisableEdit || !isEditModeOpen(group)"
										>
										</v-text-field>

										<div v-if="item.settingKey === 7001 && item.value != null && item.value.length > 0" class="minus-mt-1">
											<div>
												sample: <v-chip small>{{ item.value }}/{{ callackVendorCode() }}/players/wallet</v-chip>
											</div>

											<div v-if="item.value.includes('/' + callackVendorCode()) && !item.value.includes('://' + callackVendorCode())" class="mt-4">
												⚠️ <span class="yellow-text">Please confirm which one is your expect callback url:</span>
												<div>
													<v-chip small>{{ item.value }}/{{ callackVendorCode() }}/players/wallet</v-chip>
												</div>
												<div>
													<v-chip small>{{ item.value }}/players/wallet</v-chip>
												</div>
											</div>
										</div>

										<v-autocomplete
											v-if="item.isDropdownDataType"
											:disabled="item.isDisableEdit || !isEditModeOpen(group)"
											attach
											:label="item.settingKeyDisplayName"
											v-model="item.value"
											:items="item.dataDropdowns"
										>
										</v-autocomplete>
									</v-col>
								</v-row>
							</div>
						</div>
					</div>
				</v-flex>
			</div>
		</v-card-text>
	</v-card>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import * as Model from '@/models/interfaces/system-setting/merchants';
import * as SystemSettingModel from '@/models/interfaces/system-setting/systemConfigSetting';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import MerchantsApi from '@/api/system-setting/merchants';
import DropdownApi from '@/api/commons/dropdown';
import { commonMethodMixin } from '@/utilities/common-methods';

import { Locale, Currency, Status } from '@/models/status/common';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { pageData, time } from '@/utilities/api-handler.lib';
import { datetimeMixin } from '@/utilities/datetime-format';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import { State, Action, Getter, namespace } from 'vuex-class';
const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';

@Component({ components: {}, mixins: [commonDisplayFiltersMixin, datetimeMixin, commonMethodMixin] })
export default class MerchantsAdvanceSetting extends Vue {
	locale: typeof Locale = Locale;

	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];

	renderComponent: boolean = true;
	editModeOpenGroups: number[] = [];
	isAdvanceLoadComplete: boolean = false;

	panel: number[] = [0];

	editModeOpenArray: number[] = [];
	merchantAdvanceSetting: SystemSettingModel.ISystemConfigSetting = {} as SystemSettingModel.ISystemConfigSetting;

	registerReloadComponent() {
		EventBus.$on(`reload-${this.$router.currentRoute.path}`, (info: any) => {
			this.renderComponent = false;
			this.$nextTick(() => {
				this.renderComponent = true;
			});
		});
	}

	getMerchantAdvanceSetting(merchantID: number, specificKey: number | null = null) {
		EventBus.$emit('show_mask', null);

		MerchantsApi.getMerchantAdvanceSetting(merchantID, specificKey)
			.then((res) => {
				this.merchantAdvanceSetting = res.data;
				this.isAdvanceLoadComplete = true;
				this.editModeOpenArray = [];

				// 如果只有一個group 預設打開編輯模式
				if (this.merchantAdvanceSetting.groups && this.merchantAdvanceSetting.groups.length === 1) {
					const targetGroup = this.merchantAdvanceSetting.groups[0];
					if (specificKey !== 7001 && !targetGroup.isAllDisableEdit) {
						this.opneEditMode(targetGroup);
					}
				}
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	getMerchantAdvanceSettingByMultipleKey(merchantID: number, specificKeys: number[]) {
		EventBus.$emit('show_mask', null);

		MerchantsApi.getMerchantAdvanceSettingBySettingKeys(merchantID, specificKeys)
			.then((res) => {
				this.merchantAdvanceSetting = res.data;
				this.isAdvanceLoadComplete = true;
				this.editModeOpenArray = [];
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	closeMerchantAdvanceSetting() {
		this.$emit('closeModalAndRefreshParentPage');
		this.merchantAdvanceSetting.groups = [];
		this.editModeOpenGroups = [];
	}

	refreshAfterSave() {
		this.closeMerchantAdvanceSetting();
	}

	saveAdvanceSetting(item: SystemSettingModel.ISystemConfigSettingGroup) {
		const vm = this;

		const postItems: SystemSettingModel.ISystemConfigSettingUpdateRequestItem[] = [];

		item.items.forEach((element) => {
			console.log(element);
			postItems.push(<SystemSettingModel.ISystemConfigSettingUpdateRequestItem>{
				value: element.value,
				settingKey: element.settingKey,
			});
		});

		EventBus.$emit('show_mask', null);

		MerchantsApi.updateMerchantAdvanceSetting(<SystemSettingModel.ISystemConfigSettingRequest>{
			merchantID: this.merchantAdvanceSetting.ownerID,
			items: postItems,
		})
			.then(() => {
				EventBus.$emit(`successfully-modified`);
				vm.closeEditMode(item);
				// vm.refreshAfterSave();

				// 強制關閉特定頁面(讓componenet要reload)
				EventBus.$emit('close_tab', '/system-setting/system-config-setting');
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	callackVendorCode() {
		const vm: any = this;
		const isBWG = vm.isBWGSiteUrl();

		if (isBWG) {
			return 'bwg';
		} else {
			return 'target';
		}
	}

	isEditModeOpen(item: SystemSettingModel.ISystemConfigSettingGroup) {
		let isOpen = false;
		this.editModeOpenGroups.forEach((element) => {
			if (element === item.groupID) {
				isOpen = true;
			}
		});

		return isOpen;
	}

	opneEditMode(item: SystemSettingModel.ISystemConfigSettingGroup) {
		this.editModeOpenGroups.push(item.groupID);
	}

	closeEditMode(item: SystemSettingModel.ISystemConfigSettingGroup) {
		this.editModeOpenGroups = this.editModeOpenGroups.filter((x) => x !== item.groupID);
	}

	copySuccess() {
		EventBus.$emit(`successfully-copy`);
	}

	mounted() {}
}
</script>

<style>
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

.hint-warning-color {
	color: #ef4c4c;
}

.hint-warning {
	color: #ef4c4c;
	font-size: 14px;
	display: inline-block;
}

.hint-warning i {
	color: #ef4c4c !important;
	font-size: 16px !important;
}

.type_0 {
	color: #b5b5b5;
}

.type_20 {
	color: #72cbff;
}

.type_30 {
	color: #ffa67b;
}

.type_40 {
	color: #49d49a;
}

.color-label label {
	color: #fd6262 !important;
	font-size: 12px;
}

.reviewMerchant .col-md-6,
.reviewMerchant .col-md-12 {
	padding: 4px 12px !important;
}
</style>
