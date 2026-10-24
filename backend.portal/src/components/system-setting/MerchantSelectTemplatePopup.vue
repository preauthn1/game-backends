<template>

	<div>

		<v-card>
			<v-card-title class="grey darken-2">
				透過模板選擇商戶

				<span class="f14 ml-2">
					{{categoryName}}
				</span>

				<div class="top-right-item" @click="closeParentDialog">
					<v-btn icon dark>
						<v-icon>close</v-icon>
					</v-btn>
				</div>

			</v-card-title>

			<v-card-text style="padding: 20px;">

				<div>

					<div class="text-r" style="margin-bottom: -10px;z-index: 9999999;position: relative;">
						<v-btn color="blue lighten-3" text class="btn search" @click.stop="showCreateTemplate"
							v-if="!isLockAll">
							<v-icon left>add</v-icon>
							新建模板
						</v-btn>&#9;

						<v-btn color="red lighten-2" text class="btn search"
							@click.stop="confirmSelectTemplateID" :disabled="selectTemplateID <= 0"
							v-if="!isLockAll">
							<v-icon left>check</v-icon>
							儲存 (選擇當前的模板)
						</v-btn>&#9;

						<v-btn color="grey lighten-2" text class="btn search" @click.stop="clearSelect"
							v-if="!isLockAll">
							<v-icon left>close</v-icon>
							清空選擇並儲存
						</v-btn>&#9;

					</div>

					<v-autocomplete :items="templateDropdownItems" :label="'選擇模板(商戶選擇)'" chips clearable
						:disabled="isLockAll" @change="whenSelectTemplateChanged()" v-model="selectTemplateID">
					</v-autocomplete>

				</div>

				<div style="min-height:350px;">

					<div v-if="isShowCreateOrEditPanel" class="mt-4"
						style="padding: 20px;background: #282828;">

						<div class="mb-4" v-if="isCreateMode">
							<h2 style="color:#ab5454;">
								新建模板(商戶選擇)
							</h2>
						</div>

						<v-text-field v-model="createMerchantSelectTemplateForm.displayName" :label="'模板名稱'"
							:disabled='formDisable'>

						</v-text-field>

						<v-autocomplete :items="valueTypeDropdownItems" :label="'編輯模式'" chips
							@change="WhenValueTypeChanged()" :disabled='!isCreateMode'
							v-model="createMerchantSelectTemplateForm.valueType">

						</v-autocomplete>

						<v-autocomplete :items="merchantDropdownItems" :label="'選擇商戶(多選)'" chips multiple
							clearable deletable-chips
							v-if="createMerchantSelectTemplateForm.valueType === typeOfIncludeMerchants"
							:disabled='formDisable' v-model="createMerchantSelectTemplateForm.includeMerchantIDs">

						</v-autocomplete>

						<div
							v-if="createMerchantSelectTemplateForm.valueType === typeOfIncludeMerchants && (isCreateMode || isSwitchToEditMode) && !isLockAll"
							class="text-r minus-mt-1">
							<a @click="showParseTextToListIntDialog()" class="link-text-grey">
								貼上多筆商戶IDs
							</a>
						</div>

						<v-autocomplete :items="dropdownItemsAgentMerchants" :label="'指定上級代理'" chips
							@change="changeDropdownWhenParentMerchantChanged()"
							v-if="createMerchantSelectTemplateForm.valueType === typeOfExcludeMerchants"
							:disabled='formDisable' v-model="createMerchantSelectTemplateForm.parentMerchantID">
						</v-autocomplete>

						<v-autocomplete :items="dropdownItemsExcludeAgentMerchants" :label="'不包含特定代理(含下級)'"
							chips deletable-chips multiple
							v-if="createMerchantSelectTemplateForm.valueType === typeOfExcludeMerchants"
							:disabled='formDisable || createMerchantSelectTemplateForm.parentMerchantID <= 0'
							v-model="createMerchantSelectTemplateForm.excludeMerchantIDs">

						</v-autocomplete>

						<v-autocomplete :items="merchantDropdownItems" :label="'強制參加商戶(*特殊設置 需透過技術)'"
							v-if="createMerchantSelectTemplateForm.alwaysAddMerchantIDs && createMerchantSelectTemplateForm.alwaysAddMerchantIDs.length > 0"
							disabled='disabled' v-model="createMerchantSelectTemplateForm.alwaysAddMerchantIDs"
							chips multiple>

						</v-autocomplete>

						<div class="text-r" v-if="isCreateMode" style="margin-top: 35px;">

							<v-btn color="grey lighten-3" text class="btn search" @click.stop="cancelCreateFrom">
								<v-icon left>cancel</v-icon>
								取消
							</v-btn>&#9;

							<v-btn color="red lighten-3" text class="btn search"
								@click.stop="saveCreateSelectTemplate">
								<v-icon left>check</v-icon>
								儲存
							</v-btn>&#9;
						</div>

						<div class="text-r" v-if="!isCreateMode" style="margin-top: 35px;">

							<v-btn color="grey lighten-3" text class="btn search"
								v-if="!isSwitchToEditMode && !isLockAll" @click.stop="switchToEditMode">
								<v-icon left>edit</v-icon>
								編輯模式
							</v-btn>&#9;

							<v-btn color="red lighten-2" text class="btn search mr-2"
								@click.stop="deleteMerchantSelect" v-if="isSwitchToEditMode">
								<v-icon left>delete</v-icon>
								刪除此模板
							</v-btn>&#9;

							<v-btn color="green lighten-2" text class="btn search" v-if="isSwitchToEditMode"
								@click.stop="saveUpdateSelectTemplate">
								<v-icon left>check</v-icon>
								儲存
							</v-btn>&#9;
						</div>

						<div class="text-r mt-2" v-if="isSwitchToEditMode">

							<div class="yellow-text f14">
								<v-icon color="yellow lighten-2" small>error</v-icon>
								若編輯模板。可能影響到所有當前使用該模板的對象。編輯前請務必確認
							</div>

						</div>

					</div>
				</div>

			</v-card-text>

		</v-card>

		<TextareaParseToListIDPopup ref="refTextareaParseToListIDPopup"
			@parentSetResultIDs="setResultMerchantIDs">

		</TextareaParseToListIDPopup>

	</div>

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
import TextareaParseToListIDPopup from '@/components/system-setting/TextareaParseToListIDPopup.vue';

import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { ALL_CURRENCY } from '@/utilities/enums_helper';
import NotificationWarning from '@/components/notification/NotificationWarning.vue';
import { pageData, time } from '@/utilities/api-handler.lib';
// import Validation from '@/utilities/validation';
import { datetimeMixin, daysAgo, timeWithTimezone, toLocalDateString, toDateStringYYYYMMDDHHMMSS } from '@/utilities/datetime-format';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import { commonMethodMixin } from '@/utilities/common-methods';
import { State, Action, Getter, namespace } from 'vuex-class';
import axios, { AxiosResponse, AxiosRequestConfig } from 'axios';
const authModule = namespace('Auth');

@Component({ components: { NotificationWarning, TextareaParseToListIDPopup }, mixins: [commonDisplayFiltersMixin, datetimeMixin, commonMethodMixin] })
export default class MerchantSelectTemplatePopup extends Vue {
	compareResult: Model.IMerchantCompareResult = {} as Model.IMerchantCompareResult;

	isShowCreateOrEditPanel: boolean = false;
	isLockEdit: boolean = false;
	isLockAll: boolean = false;
	isCreateMode: boolean = false;
	isSwitchToEditMode: boolean = false;
	selectTemplateID: number = 0;
	categoryID: number = 0;
	templateDropdownItems: DropdownModel.IDropdownItems[] = [];
	merchantDropdownItems: DropdownModel.IDropdownItems[] = [];
	dropdownItemsAgentMerchants: DropdownModel.IDropdownItems[] = [];
	dropdownItemsExcludeAgentMerchants: DropdownModel.IDropdownItems[] = [];

	typeOfIncludeMerchants: number = 3701;
	typeOfExcludeMerchants: number = 9911;

	get formDisable() {
		return !this.isCreateMode && !this.isSwitchToEditMode;
	}

	get categoryName() {
		if (this.categoryID === 6601) {
			return ' (其他)';
		}

		if (this.categoryID === 6602) {
			return ' (Include／Exclude　特定商戶遊戲維護)';
		}

		if (this.categoryID === 6605) {
			return ' (公告/廣告管理)';
		}

		return '';
	}

	get valueTypeDropdownItems() {
		const myItems: DropdownModel.IDropdownItems[] = [];

		myItems.push(<DropdownModel.IDropdownItems>{
			text: '選擇商戶(多選)',
			value: this.typeOfIncludeMerchants,
			valueInt: this.typeOfIncludeMerchants,
		});

		myItems.push(<DropdownModel.IDropdownItems>{
			text: '選擇代理來指定整個樹狀商戶',
			value: this.typeOfExcludeMerchants,
			valueInt: this.typeOfExcludeMerchants,
		});

		return myItems;
	}

	createMerchantSelectTemplateForm: Model.ICreateMerchantSelectTemplate = {} as Model.ICreateMerchantSelectTemplate;

	leftMerchantID: number = 0;
	rightMerchantID: number = 0;
	isGlobalMaskVisible = false;

	changeDropdownWhenParentMerchantChanged() {
		if (this.createMerchantSelectTemplateForm.parentMerchantID <= 0) {
			return;
		}

		const myFilter = {} as DropdownModel.ILazyCommonParameterReqeust;
		myFilter.hierarchyOfTargerMerchant = this.createMerchantSelectTemplateForm.parentMerchantID;

		DropdownApi.getHierarchyMerchantsByParam(myFilter).then((res) => {
			this.dropdownItemsExcludeAgentMerchants = res;
		});
	}

	WhenValueTypeChanged() {
		this.isShowCreateOrEditPanel = false;
		this.isShowCreateOrEditPanel = true;
	}

	initialPopup(categoryID: number, mySelectTemplateID: number) {
		this.categoryID = categoryID;
		this.createMerchantSelectTemplateForm.categoryID = categoryID;
		this.createMerchantSelectTemplateForm.valueType = this.typeOfIncludeMerchants;
		this.initialTemplateDropdown(categoryID);
		this.initialDropdownOthers();
		this.selectTemplateID = mySelectTemplateID;
		this.isShowCreateOrEditPanel = false;

		if (mySelectTemplateID > 0) {
			this.whenSelectTemplateChanged();
		}
	}

	initialPopupLockAll(categoryID: number, mySelectTemplateID: number) {
		this.initialPopup(categoryID, mySelectTemplateID);
		this.isLockAll = true;
	}

	showCreateTemplate() {
		this.isLockEdit = false;
		this.isCreateMode = true;
		this.isSwitchToEditMode = false;
		this.isShowCreateOrEditPanel = true;
		this.createMerchantSelectTemplateForm.displayName = '';
		this.createMerchantSelectTemplateForm.includeMerchantIDs = [];
		this.createMerchantSelectTemplateForm.excludeMerchantIDs = [];
		this.createMerchantSelectTemplateForm.parentMerchantID = 0;
		this.createMerchantSelectTemplateForm.alwaysAddMerchantIDs = [];
	}

	cancelCreateFrom() {
		this.isShowCreateOrEditPanel = false;
	}

	showParseTextToListIntDialog(IDs: number[]) {
		const targetRef: any = this.$refs.refTextareaParseToListIDPopup;
		targetRef.showParseTextareaPraseToIDs();
	}

	setResultMerchantIDs(IDs: number[]) {
		this.isShowCreateOrEditPanel = false;
		this.createMerchantSelectTemplateForm.includeMerchantIDs = IDs;

		setTimeout(() => {
			this.isShowCreateOrEditPanel = true;
		}, 100);
	}

	whenSelectTemplateChanged() {
		if (this.selectTemplateID <= 0) {
			this.isShowCreateOrEditPanel = false;
			return;
		}

		MerchantsApi.getMerchantSelectTemplateByID(this.selectTemplateID)
			.then((res) => {
				this.isLockEdit = true;
				this.isCreateMode = false;
				this.isSwitchToEditMode = false;

				this.createMerchantSelectTemplateForm.templateID = res.data.templateID;
				this.createMerchantSelectTemplateForm.displayName = res.data.displayName;
				this.createMerchantSelectTemplateForm.valueType = res.data.valueType;
				this.createMerchantSelectTemplateForm.includeMerchantIDs = res.data.includeMerchantIDs;
				this.createMerchantSelectTemplateForm.excludeMerchantIDs = res.data.excludeMerchantIDs;
				this.createMerchantSelectTemplateForm.parentMerchantID = res.data.parentMerchantID;
				this.createMerchantSelectTemplateForm.alwaysAddMerchantIDs = res.data.alwaysAddMerchantIDs;

				if (res.data.valueType === this.typeOfExcludeMerchants) {
					this.changeDropdownWhenParentMerchantChanged();
				}
				// 重新render
				this.isShowCreateOrEditPanel = false;
				this.isShowCreateOrEditPanel = true;
			})
			.finally(() => {});
	}

	saveCreateSelectTemplate() {
		EventBus.$emit('show_mask', null);
		MerchantsApi.createMerchantSelectTemplate(this.createMerchantSelectTemplateForm)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.initialTemplateDropdown(this.createMerchantSelectTemplateForm.categoryID);
				this.selectTemplateID = res.data.resultId;

				setTimeout(() => {
					this.whenSelectTemplateChanged();
				}, 250);
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	switchToEditMode() {
		this.isSwitchToEditMode = true;
	}

	deleteMerchantSelect() {
		if (confirm('確認進行此操作? (刪除)')) {
			EventBus.$emit('show_mask', null);
			MerchantsApi.deleteMerchantSelectTemplate(this.createMerchantSelectTemplateForm.templateID)
				.then((res) => {
					EventBus.$emit(`successfully-modified`);
					this.initialTemplateDropdown(this.createMerchantSelectTemplateForm.categoryID);
					this.isSwitchToEditMode = false;
					this.selectTemplateID = 0;

					setTimeout(() => {
						this.whenSelectTemplateChanged();
					}, 250);
				})
				.finally(() => {
					EventBus.$emit('hide_mask', null);
				});
		}
	}

	saveUpdateSelectTemplate() {
		EventBus.$emit('show_mask', null);
		MerchantsApi.updateMerchantSelectTemplate(this.createMerchantSelectTemplateForm)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.initialTemplateDropdown(this.createMerchantSelectTemplateForm.categoryID);
				this.isSwitchToEditMode = false;

				setTimeout(() => {
					this.whenSelectTemplateChanged();
				}, 250);
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	clearSelect() {
		if (confirm('確認進行此操作? (清除選項)')) {
			this.selectTemplateID = 0;
			this.confirmSelectTemplateID();
		}
	}

	confirmSelectTemplateID() {
		this.$emit('parentConfirmAndSaveTemplateID', this.selectTemplateID);
	}

	closeParentDialog() {
		this.$emit('parentCloseDialog', this.selectTemplateID);
	}

	initialTemplateDropdown(categoryID: number) {
		const myFilter = {} as DropdownModel.ILazyCommonParameterReqeust;
		myFilter.categoryID = categoryID;

		DropdownApi.getMerchantSelectTemplates(myFilter).then((res) => {
			this.templateDropdownItems = res;
		});
	}

	initialDropdownOthers() {
		DropdownApi.getMerchants().then((res) => {
			this.merchantDropdownItems = res;
		});
		DropdownApi.getHierarchyMerchants().then((res) => {
			this.dropdownItemsAgentMerchants = res;
		});
	}

	mounted() {}
}
</script>


<style scoped>
.exterInfo {
	color: #888;
	font-size: 12px;
	padding-bottom: 10px;
}
</style>