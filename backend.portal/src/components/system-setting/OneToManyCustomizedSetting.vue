<template>
	<v-app v-if="renderComponent" class="app">
		<v-expansion-panels v-model="panel" multiple>
			<v-expansion-panel>
				<v-expansion-panel-content>
					<v-flex>
						<v-row class="search-form">
							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-text-field v-model="searchFilter.keyword" :label="$t('TableHeader.__keyword')" clearable> </v-text-field>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete clearable :items="oneToManySettingTypesDropdownItems" v-model="searchFilter.type" :label="$t('TableHeader.__type')"> </v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item"></div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item search-row-padding">
									<v-btn color="yellow darken-1" text class="btn search" @click.stop="searchPageOne">
										<v-icon left>search</v-icon>
										{{ $t('Button.__search') }} </v-btn
									>&#9;
									<v-btn color="pink lighten-3" text class="btn search" @click.stop="clear">
										<v-icon left>clear</v-icon>
										{{ $t('Button.__reset') }} </v-btn
									>&#9;
								</div>
							</v-col>
						</v-row>
					</v-flex>
				</v-expansion-panel-content>
			</v-expansion-panel>
		</v-expansion-panels>

		<v-data-table
			:headers="headers"
			id="table"
			:items="searchResult"
			:page="DataTablePagingParameter.pageIndex"
			:items-per-page="DataTablePagingParameter.itemsPerPage"
			:server-items-length="DataTablePagingParameter.totalCount"
			:footer-props="DataTablePagingParameter.footerProps"
			@update:page="changePage"
			@update:items-per-page="changeItemPerPage"
		>
			<template v-slot:top>
				<v-toolbar>
					<v-toolbar-title>
						<h4>一對多系統配置管理</h4>
					</v-toolbar-title>
					<v-divider class="mx-4" inset vertical></v-divider>
					<v-spacer></v-spacer>
					<v-card-title>
						<v-btn icon dark @click="showCreateDialog" style="margin-left: 15px">
							<v-icon>add</v-icon>
						</v-btn>
					</v-card-title>
				</v-toolbar>
			</template>

			<template v-slot:item.action="{ item }">
				<v-icon small @click="showDetailItems(item.oneToManyCustomizedSettingID)"> edit </v-icon>
			</template>

			<template v-slot:item.typeName="{ item }">
				<div :title="item.type">
					{{ item.typeName }}
				</div>
				<div class="sub-title-text">
					{{ item.type }}
				</div>
			</template>

			<template v-slot:item.createTime="{ item }">
				{{ item.createTime | timeWithAddTimezone(userProfile.timeZone) }}
			</template>
		</v-data-table>

		<!-- 進階設置 -->
		<v-dialog v-model="dialogVisible.isShowAdvanceInfo" max-width="750px" scrollable>
			<v-card>
				<v-card-title class="grey darken-2">
					進階設置 <span class="f12 ml-1">({{ targetOneToManySettingID }})</span>

					<v-btn class="btn search ml-1" text small color="pink lighten-4" @click="copyTextToClipboard2(searchDetailItems.sqlQueryScript)">
						<v-icon small>file_copy</v-icon>
						常用SQL
					</v-btn>

					<v-btn class="btn search ml-1" text small color="orange lighten-2" @click="copyTextToClipboard2(searchDetailItems.sqlInsertScript)">
						<v-icon small>file_copy</v-icon>
						insert語法
					</v-btn>

					<v-btn class="btn search ml-1" text small color="yellow lighten-2" @click="copyAllCurrentItems()">
						<v-icon small>file_copy</v-icon>
						複製當前項目文字
					</v-btn>

					<v-btn class="btn search ml-1" text small color="green lighten-2" @click="showInsertEmptyNewItemDialog()">
						<v-icon small>add</v-icon>
						插入1筆空陣列
					</v-btn>

					<span style="font-size: 14px; margin-left: 10px">
						<!-- ({{currentAdvanceMerchantID}}) -->
					</span>
					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>

				<v-card-text class="advancePanelContent advancePanelContent group-settings">
					<v-flex>
						<div v-for="(group, index) in searchDetailItems.groups" :key="index" class="eachGroup">
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
									<v-row style="">
										<v-col cols="12" sm="8" md="8" :title="item.settingKey">
											<div style="width: 100%; position: relative">
												<div style="position: absolute; top: 5px; right: 0px; z-index: 888">
													<v-icon color="grey darken-2" v-if="isEditModeOpen(group)" small @click="copyUpdateSQL(item)">stars </v-icon>
													<span x-small class="my-chip">{{ item.memo }}</span>
												</div>

												<v-text-field
													v-show="!item.isDropdownDataType"
													:label="item.settingKeyDisplayName"
													v-model="item.value"
													:disabled="item.isDisableEdit || !isEditModeOpen(group)"
												>
												</v-text-field>

												<v-select
													v-show="item.isDropdownDataType"
													:disabled="item.isDisableEdit || !isEditModeOpen(group)"
													attach
													:label="item.settingKeyDisplayName"
													v-model="item.value"
													:items="item.dataDropdowns"
												>
												</v-select>
											</div>
										</v-col>
									</v-row>
								</div>
							</div>
						</div>
					</v-flex>
				</v-card-text>
			</v-card>
		</v-dialog>

		<!-- 新增 -->
		<v-dialog v-model="dialogVisible.isShowCreate" max-width="500px" width="700px" persistent>
			<v-card>
				<v-card-title class="grey darken-2">
					新增
					<span style="font-size: 14px; margin-left: 10px">
						<!-- ({{currentAdvanceMerchantID}}) -->
					</span>
					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>

				<v-card-text>
					<v-row>
						<v-autocomplete
							v-model="createForm.type"
							:items="oneToManySettingTypesDropdownItems"
							@change="setDefaultValueForType()"
							chips
							class="required"
							:label="$t('TableHeader.__type')"
						>
						</v-autocomplete>
					</v-row>

					<v-row>
						<v-text-field v-model="createForm.displayName" :label="$t('TableHeader.__displayName')" clearable required></v-text-field>
					</v-row>

					<v-row>
						<v-text-field v-model="createForm.defaultSubCategory" :label="'預建子項目'" clearable required> </v-text-field>
					</v-row>

					<v-row>
						<v-text-field v-model="createForm.memo" :label="$t('TableHeader.__memo')" clearable required></v-text-field>
					</v-row>

					<v-row>
						<v-autocomplete v-model="createForm.ownerID" :items="vendorConfigAGGLiveDropdownItems" clearable v-if="createForm.type == 7501" :label="'Owner (VendorConfig)'">
						</v-autocomplete>

						<v-text-field v-model="createForm.ownerID" :label="'OwnerID (可不填寫)'" clearable v-else> </v-text-field>
					</v-row>

					<v-row>
						<v-text-field v-model="createForm.ownerIDMemo" :label="'OwnerMemo (可不填寫)'" clearable> </v-text-field>
					</v-row>

					<v-row> </v-row>
				</v-card-text>

				<v-card-actions>
					<v-spacer></v-spacer>
					<v-btn text @click="closeAllModal" class="btn">
						<v-icon left>mdi-cancel</v-icon>
						{{ $t('Button.__cancel') }}
					</v-btn>
					<v-btn color="yellow darken-1" text @click="saveCreate" class="btn">
						<v-icon left>mdi-check-circle</v-icon>
						{{ $t('Button.__submit') }}
					</v-btn>
				</v-card-actions>
			</v-card>
		</v-dialog>

		<!-- 插入空白子item -->
		<v-dialog v-model="dialogVisible.isShowExtra1" max-width="500px" width="700px" persistent>
			<v-card>
				<v-card-title class="grey darken-2">
					新增 (子item)
					<span style="font-size: 14px; margin-left: 10px">
						<!-- ({{currentAdvanceMerchantID}}) -->
					</span>
					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>

				<v-card-text class="mt-4">
					<div>
						<v-text-field v-model="insertEmptyNewItemFrom.oneToManyCustomizedSettingID" disabled :label="'主表masterID'"> </v-text-field>
					</div>
					<div>
						<v-text-field v-model="insertEmptyNewItemFrom.subCategory" :label="'subCategory(可空值)'" placeholder="若填寫空值或預設帶入流水序號"> </v-text-field>
					</div>

					<div>
						<v-text-field v-model="insertEmptyNewItemFrom.settingValue" :label="'settingValue(設置數值)'"> </v-text-field>
					</div>

					<div>
						<v-text-field v-model="insertEmptyNewItemFrom.memo" :label="'memo(可空值)'"> </v-text-field>
					</div>
				</v-card-text>

				<v-card-actions>
					<v-spacer></v-spacer>

					<v-btn color="yellow darken-1" text class="btn search" @click.stop="insertEmptyNewItem">
						<v-icon left>check</v-icon>
						{{ $t('Button.__save') }} </v-btn
					>&#9;
				</v-card-actions>
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
import oneToManyCustomizedSettingApi from '@/api/system-setting/oneToManyCustomizedSetting';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import NotificationWarning from '@/components/notification/NotificationWarning.vue';
import * as Model from '@/models/interfaces/system-setting/systemConfigSetting';

import * as MemberModel from '@/models/interfaces/members-management/players';
import systemConfigSettingApi from '@/api/system-setting/systemConfigSetting';
import { commonMethodMixin } from '@/utilities/common-methods';

import * as ResponseModel from '@/models/interfaces/commons/response';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import { State, Action, Getter, namespace } from 'vuex-class';
const authModule = namespace('Auth');
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { datetimeMixin, startEndDate, timeWithTimezone, daysAgo, weeksAgo, monthsAgo, formatMixin } from '@/utilities/datetime-format';
import * as Permission from '@/models/status/permission';

@Component({ components: { NotificationWarning }, mixins: [commonMethodMixin, commonDisplayFiltersMixin, datetimeMixin] })
export default class ServersRobotConfig extends Vue {
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];
	@authModule.State('defaultLangTimes') defaultLangTimes!: number;

	dialogVisible: DropdownModel.DialogVisible = new DropdownModel.DialogVisible();

	renderComponent: boolean = true;
	editModeOpenGroups: number[] = [];
	targetOneToManySettingID: number = 0;

	DataTablePagingParameter: DropdownModel.DataTablePagingParameter = new DropdownModel.DataTablePagingParameter();
	totalCount: number = 0;
	searchFilter: Model.IQueryOneToManyCustomizedSettingApiRequestParameter = {} as Model.IQueryOneToManyCustomizedSettingApiRequestParameter;
	searchResult: Model.IOneToManyCustomizedSetting[] = [];
	insertEmptyNewItemFrom: any = {};

	createForm: Model.IQueryOneToManyCustomizedSettingCreateRequestParameter = {} as Model.IQueryOneToManyCustomizedSettingCreateRequestParameter;

	searchDetailItems: Model.IOneToManyCustomizedSettingItem = {} as Model.IOneToManyCustomizedSettingItem;
	systemConfigSettings: Model.ISystemConfigSetting = <Model.ISystemConfigSetting>{};

	panel: number[] = [0];

	oneToManySettingTypesDropdownItems: DropdownModel.IDropdownItems[] = [];
	vendorConfigAGGLiveDropdownItems: DropdownModel.IDropdownItems[] = [];

	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;

	get headers() {
		const array = [];

		array.push({ text: this.$t('TableHeader.__action'), value: 'action' });
		array.push({ text: this.$t('TableHeader.__ID'), value: 'oneToManyCustomizedSettingID' });
		array.push({ text: this.$t('TableHeader.__displayName'), value: 'displayName' });

		array.push({ text: this.$t('TableHeader.__memo'), value: 'memo' });
		array.push({ text: this.$t('TableHeader.__type'), value: 'typeName' });
		array.push({ text: 'ownerID', value: 'ownerID' });
		array.push({ text: 'ownerMemo', value: 'ownerIDMemo' });
		array.push({ text: this.$t('TableHeader.__createTime'), value: 'createTime', align: 'end' });

		return array;
	}

	changePage(index: number = 1) {
		this.DataTablePagingParameter.pageIndex = index;
		this.search();
	}

	changeItemPerPage(val: number = 10) {
		this.DataTablePagingParameter.itemsPerPage = val;
		this.search();
	}

	searchPageOne() {
		this.DataTablePagingParameter.pageIndex = 1;
		this.search();
	}

	search() {
		this.searchFilter.pageIndex = this.DataTablePagingParameter.pageIndex;
		this.searchFilter.pageSize = this.DataTablePagingParameter.itemsPerPage;

		EventBus.$emit('show_mask', null);

		oneToManyCustomizedSettingApi
			.getByPagingFilter(this.searchFilter)
			.then((res) => {
				this.searchResult = res.data.items;
				this.DataTablePagingParameter.totalCount = res.data.totalCount;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	showDetailItems(oneToManySettingID: number) {
		this.dialogVisible.isShowAdvanceInfo = true;
		this.searchDetailItems.groups = [];
		this.targetOneToManySettingID = oneToManySettingID;

		EventBus.$emit('show_mask', null);

		oneToManyCustomizedSettingApi
			.getSettingItems(oneToManySettingID)
			.then((res) => {
				this.searchDetailItems = res.data;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	copyUpdateSQL(item: Model.ISystemConfigSettingItem) {
		const vm: any = this;
		vm.copyTextToClipboard2(item.debug1);
	}

	showCreateDialog() {
		this.dialogVisible.isShowCreate = true;
		this.createForm = {} as Model.IQueryOneToManyCustomizedSettingCreateRequestParameter;
		this.createForm.defaultSubCategory = '1,2,3,4,5';
	}

	saveCreate() {
		EventBus.$emit('show_mask', null);
		oneToManyCustomizedSettingApi
			.create(this.createForm)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.closeAllModal();
				this.search();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	setDefaultValueForType() {
		if (this.createForm.type === 7501) {
			this.createForm.defaultSubCategory = 'VIP000,VIP001,VIP002,VIP003,VIP004,VIP005';
			this.createForm.ownerIDMemo = 'VendorConfig';
		}
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
		this.editModeOpenGroups.push(item.groupID);
	}

	closeEditMode(item: Model.ISystemConfigSettingGroup) {
		this.editModeOpenGroups = this.editModeOpenGroups.filter((x) => x !== item.groupID);
	}

	showInsertEmptyNewItemDialog() {
		this.dialogVisible.isShowExtra1 = true;
		this.insertEmptyNewItemFrom = {};
		this.insertEmptyNewItemFrom.oneToManyCustomizedSettingID = this.targetOneToManySettingID;
	}

	insertEmptyNewItem() {
		oneToManyCustomizedSettingApi.insertEmptyItem(this.insertEmptyNewItemFrom).then(() => {
			// 刷新畫面
			this.showDetailItems(this.targetOneToManySettingID);
			this.dialogVisible.isShowExtra1 = false;
			EventBus.$emit(`successfully-modified`);
		});
	}

	copyAllCurrentItems() {
		let copyText: string = '';

		for (const eachGroup of this.searchDetailItems.groups) {
			for (const item of eachGroup.items) {
				copyText += item.settingKeyDisplayName + '\t' + item.value + '\t' + item.memo + '\n';
			}
		}

		const vm: any = this;
		vm.copyTextToClipboard(copyText);
	}

	saveAdvanceSetting(item: Model.ISystemConfigSettingGroup) {
		const vm = this;

		const postItems: Model.ISystemConfigSettingUpdateRequestItem[] = [];

		item.items.forEach((element) => {
			console.log(element);
			postItems.push(<Model.ISystemConfigSettingUpdateRequestItem>{
				value: element.value,
				settingKeyText: element.settingKeyText,
				settingKey: element.settingKey,
			});
		});

		const payload = <Model.ISystemConfigSettingRequest>{
			ownerID: this.searchDetailItems.ownerID,
			items: postItems,
		};

		console.log(payload);
		EventBus.$emit('show_mask', null);

		oneToManyCustomizedSettingApi
			.updateSettings(payload)
			.then(() => {
				EventBus.$emit(`successfully-modified`);
				vm.closeEditMode(item);
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	closeAllModal() {
		this.dialogVisible.resetAllValueToFalse();
	}

	mounted() {
		this.init();
	}

	clear() {
		this.searchFilter = {} as Model.IQueryOneToManyCustomizedSettingApiRequestParameter;
	}

	init() {
		this.search();

		DropdownApi.getOneToManySettingTypes().then((res) => {
			this.oneToManySettingTypesDropdownItems = res;
		});

		DropdownApi.getVendorConfig(330000).then((res) => {
			this.vendorConfigAGGLiveDropdownItems = res;
		});
	}
}
</script>

<style lang="scss" scoped>
.my-text-area textarea {
	height: 300px !important;
}

.my-chip {
	background: #333;
	color: #aaa;
	min-width: 50px;
	text-align: center;
	display: inline-block;
	border-radius: 5px;
	padding: 0px 3px;
	font-size: 11px;
}
</style>
