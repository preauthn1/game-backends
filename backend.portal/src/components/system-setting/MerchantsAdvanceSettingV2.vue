<template>

	<!-- 進階設置 -->
	<v-card>
		<v-card-title class="grey darken-2">
			{{$t('Button.__advanceSetting')}}

			<v-chip small class="ml-2" color="brown darken-1">
				{{merchantAdvanceSetting.ownerID}}｜{{merchantAdvanceSetting.ownerName}}
			</v-chip>

			<v-chip small color="blue darken-4" class="ml-2"
				v-if="merchantAdvanceSetting.numberOfSubAgents > 0">
				子代理數:{{merchantAdvanceSetting.numberOfSubAgents}}
			</v-chip>

			<div class="top-right-item" @click="closeMerchantAdvanceSetting">
				<v-btn icon dark>
					<v-icon>close</v-icon>
				</v-btn>
			</div>
		</v-card-title>

		<v-card-text class="advancePanelContent advancePanelContent group-settings" style="">

			<div style="padding-bottom: 50px;">
				<v-flex
					v-if="isAdvanceLoadComplete && merchantAdvanceSetting && merchantAdvanceSetting.groups">

					<v-row>

						<v-col cols="12" md="3" sm="3" style="padding: 0px;">

							<div class="leftGroupChoosePanel">

								<div style="padding: 0px 10px " class="mb-2 searchBar">
									<v-text-field v-model="searchGroupKeyword" append-icon="search" class="" clearable
										@click="whenFocusSearchGroup()" :label="'關鍵字查詢'" single-line hide-details>
									</v-text-field>
								</div>

								<div :class="'pointer f14 leftGroupChoose ' + selectGroupStyle(0)"
									@click="changeVisibleTab(0)">
									全部
								</div>

								<div v-for="(group, index) in merchantAdvanceSetting.groupCategoryList" :key="index"
									:class="'pointer f14 leftGroupChoose ' + selectGroupStyle(group.groupID)"
									@click="changeVisibleTab(group.groupID)">
									{{group.groupName}}
								</div>
							</div>

						</v-col>

						<v-col cols="12" md="9" sm="9">
							<div class="rightGroupPanel">

								<div v-for="(group, index) in merchantAdvanceSetting.groups" :key="index">

									<div v-if="currentVisibleGroupID == 0 || currentVisibleGroupID == group.groupID"
										v-show="keywordMatch(group)">

										<div class="mb-4" style="margin-bottom: 75px;">

											<v-row>
												<v-col cols="12" sm="6" md="6">
													<div class="groupName">
														{{ group.groupName }}
													</div>
												</v-col>

												<v-col cols="12" sm="6" md="6" class="saveBtnRow">
													<div v-show="!isEditModeOpen(group)">
														<v-btn medium v-show="group.isAllDisableEdit" disabled>
															{{ $t('Button.__disabledEdit') }}
														</v-btn>
														<v-btn medium v-show="!group.isAllDisableEdit"
															@click="opneEditMode(group)">
															{{ $t('Popup.__update') }}</v-btn>
													</div>

													<div v-show="isEditModeOpen(group)">
														<v-btn medium @click="saveAdvanceSetting(group)" color="primary">
															{{ $t('Button.__submit') }}</v-btn>
														<v-btn medium @click="closeEditMode(group)" color="#666">
															{{ $t('Button.__cancel') }}</v-btn>
													</div>
												</v-col>
											</v-row>

											<div class="groupItem">
												<div v-for="(item, index2) in group.items" :key="index2">
													<v-row v-show="keywordMatch(item)">
														<v-col cols="12" sm="8" md="8" :title="item.settingKey">
															<v-text-field v-if="!item.isDropdownDataType"
																:label="item.settingKeyDisplayName" v-model="item.value"
																:disabled="item.isDisableEdit || !isEditModeOpen(group)"></v-text-field>

															<v-autocomplete v-if="item.isDropdownDataType"
																:disabled="item.isDisableEdit || !isEditModeOpen(group)" attach
																:label="item.settingKeyDisplayName" v-model="item.value"
																:items="item.dataDropdowns">
															</v-autocomplete>
														</v-col>
													</v-row>
												</div>
											</div>

										</div>
									</div>
								</div>

							</div>
						</v-col>
					</v-row>

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
	currentVisibleGroupID: number = 0;
	searchGroupKeyword: string = '';

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
				this.currentVisibleGroupID = 0;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	keywordMatch(item: any) {
		if (this.searchGroupKeyword == null || this.searchGroupKeyword === '') {
			return true;
		}
		const targetText = this.searchGroupKeyword.toLowerCase();
		return item.keywordForSearch.includes(targetText);
	}

	closeMerchantAdvanceSetting() {
		this.$emit('closeModalAndRefreshParentPage');
		this.merchantAdvanceSetting.groups = [];
		this.editModeOpenGroups = [];
	}

	refreshAfterSave() {
		this.closeMerchantAdvanceSetting();
	}

	changeVisibleTab(groupID: number) {
		this.currentVisibleGroupID = groupID;

		if (groupID > 0) {
			this.searchGroupKeyword = '';
		}
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

	selectGroupStyle(myGroupID: number) {
		return this.currentVisibleGroupID === myGroupID ? 'selectedGroup' : '';
	}

	whenFocusSearchGroup() {
		this.currentVisibleGroupID = 0;
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

.leftGroupChoose {
	padding: 8px 8px;
}

.leftGroupChoose:hover {
	background: #555;
}

.rightGroupPanel {
	overflow-y: auto;
	overflow-x: hidden;
	height: 700px;
	border-bottom: double;
	border-bottom-color: #5e5e5e;
	border-bottom-width: 3px;
}

.selectedGroup {
	color: #feee6a;
}

.searchBar .v-text-field__slot label {
	font-size: 12px !important;
}

.searchBar .v-icon {
	font-size: 16px !important;
}

.searchBar .v-text-field {
	padding-top: 0px !important;
}

.leftGroupChoosePanel {
	background: #222;
	padding: 5px 0px 60px 0px;
}
</style>