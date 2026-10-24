<template>
	<v-app v-if="renderComponent" class="app">

		<v-data-table :headers="headers" id="table" :items="ListData"
			:page="DataTablePagingParameter.pageIndex"
			:items-per-page="DataTablePagingParameter.itemsPerPage"
			:server-items-length="DataTablePagingParameter.totalCount"
			:footer-props="DataTablePagingParameter.footerProps" @update:page="changePage"
			@update:items-per-page="changeItemPerPage" multi-sort>
			<template v-slot:top>
				<v-toolbar>
					<v-toolbar-title>
						<h4>{{ $t('Menu.SystemSetting_VendorMaintenace') }}</h4>

					</v-toolbar-title>
					<v-divider class="mx-4" inset vertical></v-divider>

					<div style="font-size:14px;color:#f9d572;">

						<v-icon small color="yellow ">info</v-icon>

						<span class="mr-2" style="font-size:14px;color:#f9d572;">
							符合其中一項條件即可跳過遊戲維護
						</span>

						<v-tooltip top color="orange darken-4">
							<template v-slot:activator="{ on, attrs }">
								<span v-bind="attrs" v-on="on"
									style="color: #e89247;text-decoration: underline;">查看條件</span>
							</template>
							<span>

								<div>
									1. 玩家被標記Tag 【允許維護期間遊戲】
								</div>

								<div>
									2. 商戶為 [內部測試商戶](進階編輯)
								</div>

								<div>
									3. 商戶被設為維護排除對象
								</div>

							</span>
						</v-tooltip>

						<v-btn color="red lighten-1" text class="btn ml-4" @click.stop="removeAllMaintain()">
							<v-icon left>delete</v-icon>
							關閉所有維護
						</v-btn>&#9;

					</div>

					<v-spacer></v-spacer>
					<v-card-title>
						<v-btn icon dark @click="refresh">
							<v-icon>refresh</v-icon>
						</v-btn>
						<v-btn icon dark @click="addItem">
							<v-icon>add</v-icon>
						</v-btn>
					</v-card-title>
				</v-toolbar>
			</template>

			<template v-slot:item.action="{ item }">

				<div class="operate_icons">

					<v-tooltip top>
						<template v-slot:activator="{ on, attrs }">
							<v-icon color="" dark v-bind="attrs" v-on="on" @click="deleteItem(item)">
								delete
							</v-icon>
						</template>
						<span>刪除</span>
					</v-tooltip>

					<v-tooltip top>
						<template v-slot:activator="{ on, attrs }">
							<v-icon color="" dark v-bind="attrs" v-on="on" @click="copyMaintainMessage(item)">
								file_copy
							</v-icon>
						</template>
						<span>複製維護文案至剪貼簿</span>

					</v-tooltip>

				</div>

			</template>

			<template v-slot:item.vendorName="{ item }">

				<div :title="item.vendorID">

					<v-chip>
						{{ item.vendorName }}
					</v-chip>
				</div>
			</template>

			<template v-slot:item.excludeMerchantTemplateName="{ item }">
				<div class="link-text-grey" @click="showExcludeMerchantSelect(item)">
					<span v-if="item.excludeMerchantTemplateID > 0">
						{{item.excludeMerchantTemplateName}}
					</span>
					<span v-else>無</span>
				</div>
			</template>

			<template v-slot:item.includeMerchantTemplateName="{ item }">
				<div class="link-text-grey" @click="showIncludeMerchantSelect(item)">
					<span v-if="item.includeMerchantTemplateID > 0">
						{{item.includeMerchantTemplateName}}
					</span>
					<span v-else>無</span>
				</div>
			</template>

			<template v-slot:item.startTime="{ item }">

				<div :class="item.isExpired ? 'light-opacity' : ''">
					{{ item.startTime | timeWithAddTimezone(userProfile.timeZone) }}
					-
					{{ item.endTime | timeWithAddTimezone(userProfile.timeZone) }}

					<span class="ml-1" style="color:#e8bc6c;">
						{{item.extraHintMsg}}
					</span>
				</div>
			</template>

			<template v-slot:item.createTime="{ item }">
				{{ item.createTime | timeWithAddTimezone(userProfile.timeZone) }}
			</template>

			<template v-slot:item.updateTime="{ item }">
				{{ item.updateTime | timeWithAddTimezone(userProfile.timeZone) }}
			</template>

		</v-data-table>

		<!-- 創建 -->
		<v-form ref="editForm">

			<v-dialog v-model="isShowEditDialog" max-width="500px" persistent>
				<v-card>
					<v-card-title class="grey darken-2">
						<span>{{$t("Menu.SystemSetting_VendorMaintenaceCreate")}}</span>

						<div class="top-right-item" @click="closeAllModal">
							<v-btn icon dark @click="refresh">
								<v-icon>close</v-icon>
							</v-btn>
						</div>
					</v-card-title>

					<v-card-text style="padding-bottom: 150px;">
						<v-flex>

							<v-row>

								<v-col cols="12" sm="3" md="8" class="dropdown-height-adjust">

									<v-select class="required " attach chips
										:label="$t('TableHeader.__gameVendorType')" v-model="editedItem.vendorID"
										:items="dropdownGameVendors">
									</v-select>
								</v-col>

							</v-row>

							<v-row>

								<div class="lineDiv">
									{{$t('TableHeader.__beginTime')}}
								</div>

								<v-col cols="12" sm="3" md="8">

									<date-picker v-model="startTime" type="datetime" value-type="timestamp">
									</date-picker>

								</v-col>

							</v-row>

							<v-row>

								<div class="lineDiv">
									{{$t('TableHeader.__endTime')}}
								</div>

								<v-col cols="12" sm="3" md="8">
									<date-picker v-model="endTime" type="datetime" value-type="timestamp">
									</date-picker>

								</v-col>

							</v-row>

						</v-flex>
					</v-card-text>
					<v-card-actions>
						<v-spacer></v-spacer>
						<v-btn text @click="closeAllModal" class="btn">
							<v-icon left>mdi-cancel</v-icon>
							{{$t('Button.__cancel')}}
						</v-btn>
						<v-btn color="yellow darken-1" text @click="save" class="btn">
							<v-icon left>mdi-check-circle</v-icon>
							{{$t('Button.__submit')}}
						</v-btn>
					</v-card-actions>
				</v-card>
			</v-dialog>

		</v-form>

		<v-dialog v-model="isShowExcludeMerchantSelect" max-width="700px">

			<MerchantSelectTemplatePopup ref="refMerchantSelectTemplatePopup"
				@parentCloseDialog="closeAllModal()"
				@parentConfirmAndSaveTemplateID="confirmAndSaveTemplateID">
			</MerchantSelectTemplatePopup>

		</v-dialog>

		<v-dialog v-model="isShowIncludeMerchantSelect" max-width="700px">

			<MerchantSelectTemplatePopup ref="refIncludeMerchantSelectTemplatePopup"
				@parentCloseDialog="closeAllModal()"
				@parentConfirmAndSaveTemplateID="confirmAndSaveIncludeTemplateID">
			</MerchantSelectTemplatePopup>

		</v-dialog>

		<NotificationWarning v-show="isShowConfirmWarning" :title="`${$t('Message.__warning')}`"
			:max="1" :text="`${$t('Message.__isConfirmDelete')}?`" :confirmed-action="confirmedAction"
			:canceled-action="canceledAction" />

	</v-app>

</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import * as Model from '@/models/interfaces/system-setting/vendorMaintenance';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import VendorMaintenaceListApi from '@/api/system-setting/vendorMaintenance';
import { commonMethodMixin } from '@/utilities/common-methods';
import DropdownApi from '@/api/commons/dropdown';
import { Locale, Currency, Status, GameType, CRUDAction, GameVendorType } from '@/models/status/common';
import { WalletMode } from '@/models/status/merchant';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { ALL_CURRENCY } from '@/utilities/enums_helper';
import NotificationWarning from '@/components/notification/NotificationWarning.vue';
import { pageData, time } from '@/utilities/api-handler.lib';
// import Validation from '@/utilities/validation';
import { datetimeMixin, startEndDate, daysAgo, timeWithTimezone } from '@/utilities/datetime-format';
import DatePicker from 'vue2-datepicker';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import { State, Action, Getter, namespace } from 'vuex-class';
import MerchantSelectTemplatePopup from '@/components/system-setting/MerchantSelectTemplatePopup.vue';

const authModule = namespace('Auth');

import * as Permission from '@/models/status/permission';

// @ts-ignore
@Component({ components: { NotificationWarning, DatePicker, MerchantSelectTemplatePopup }, mixins: [commonDisplayFiltersMixin, datetimeMixin, commonMethodMixin] })
export default class VendorMaintenance extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];
	renderComponent: boolean = true;
	isShowEditDialog: boolean = false;
	isShowExcludeMerchantSelect: boolean = false;
	isShowIncludeMerchantSelect: boolean = false;

	ListData: Model.IVendorMaintenance[] = [];
	editedItem: Model.ICreateVendorMaintenance = {} as Model.ICreateVendorMaintenance;
	isShowConfirmWarning: boolean = false;

	targetIdOfUpdateTemplateFrom: number = 0;
	selectAllVendors: number = 9999999;
	deleteID: number = 0;
	startTime: number | null = null;
	endTime: number | null = null;
	currentTime: string = '';
	DataTablePagingParameter: DropdownModel.DataTablePagingParameter = new DropdownModel.DataTablePagingParameter();

	dropdownGameVendors: DropdownModel.IDropdownItems[] = [];

	get hasPermissionCreate() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSetting_GameList_Create) >= 0;
	}

	get hasPermissionUpdate() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSetting_GameList_Update) >= 0;
	}

	get headers() {
		const data = [
			{ text: this.$t('TableHeader.__gameVendorType'), value: 'vendorName', sortable: false, align: 'start' },
			{ text: this.$t('TableHeader.__beginTime') + '／' + this.$t('TableHeader.__endTime'), value: 'startTime', sortable: false },
			{ text: this.$t('TableHeader.__totalMinutes'), value: 'totalMinutes', sortable: false },
			{ text: '設定商戶可略過維護', value: 'excludeMerchantTemplateName', sortable: false },
			{ text: '僅特定商戶維護', value: 'includeMerchantTemplateName', sortable: false },
			{ text: this.$t('TableHeader.__createTime2'), value: 'createTime', sortable: false },
			{ text: this.$t('TableHeader.__createBy'), value: 'createBy', sortable: false },
		];

		if (true) {
			data.splice(0, 0, { text: this.$t('TableHeader.__action'), value: 'action', sortable: false, align: 'center' });
		}
		return data;
	}

	refresh() {
		this.registerReloadComponent();
		VendorMaintenaceListApi.getVendorMaintenance(<Model.IVendorMaintenancePagingRequest>{
			pageIndex: this.DataTablePagingParameter.pageIndex,
			pageSize: this.DataTablePagingParameter.itemsPerPage,
		}).then((res) => {
			this.ListData = res.data.items;
			this.DataTablePagingParameter.totalCount = res.data.totalCount;
		});
	}

	registerReloadComponent() {
		EventBus.$on(`reload-${this.$router.currentRoute.path}`, (info: any) => {
			this.renderComponent = false;
			this.$nextTick(() => {
				this.renderComponent = true;
			});
		});
	}

	addItem() {
		this.isShowEditDialog = true;
		this.editedItem = {} as Model.ICreateVendorMaintenance;

		// 如果出現畫面datePicker無法刷新 可參考 reloadPopupWhenValueChanged
		this.startTime = daysAgo(1) + 60 * 60 * 9 * 1000;
		this.endTime = daysAgo(1) + 60 * 60 * 12 * 1000;
		this.editedItem.vendorID = -1;
		this.getTime();

		this.resetFormValidate();
	}

	showExcludeMerchantSelect(item: Model.IVendorMaintenance) {
		this.isShowExcludeMerchantSelect = true;
		this.targetIdOfUpdateTemplateFrom = item.id;

		const vm: any = this;

		this.$nextTick(() => {
			vm.$refs.refMerchantSelectTemplatePopup.initialPopup(6602, item.excludeMerchantTemplateID);
		});
	}

	showIncludeMerchantSelect(item: Model.IVendorMaintenance) {
		if (confirm('若您配置《僅特定商戶維護》，將會導致其他商戶開放遊戲、結束維護狀態。請確認清楚你要進此操作。')) {
			this.isShowIncludeMerchantSelect = true;
			this.targetIdOfUpdateTemplateFrom = item.id;

			const vm: any = this;

			this.$nextTick(() => {
				vm.$refs.refIncludeMerchantSelectTemplatePopup.initialPopup(6602, item.includeMerchantTemplateID);
			});
		}
	}

	confirmAndSaveTemplateID(templateID: number) {
		EventBus.$emit('show_mask', null);
		VendorMaintenaceListApi.updateExcludeTemplateID(this.targetIdOfUpdateTemplateFrom, templateID)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.closeAllModal();
				this.refresh();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	confirmAndSaveIncludeTemplateID(templateID: number) {
		EventBus.$emit('show_mask', null);
		VendorMaintenaceListApi.updateIncludeTemplateID(this.targetIdOfUpdateTemplateFrom, templateID)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.closeAllModal();
				this.refresh();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
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
		const form: any = this.$refs.editForm;
		form.resetValidation();
	}

	closeAllModal() {
		this.isShowEditDialog = false;
		this.isShowExcludeMerchantSelect = false;
		this.isShowIncludeMerchantSelect = false;
	}

	save() {
		const form: any = this.$refs.editForm;

		if (form.validate()) {
			const request = <Model.ICreateVendorMaintenance>{ ...this.editedItem };
			request.startTime = startEndDate(Number(this.startTime));
			request.endTime = startEndDate(Number(this.endTime));

			VendorMaintenaceListApi.createVendorMaintenance(<Model.ICreateVendorMaintenance>request).then(() => {
				EventBus.$emit(`successfully-modified`);
				this.closeAllModal();
				this.refresh();
			});
		}
	}

	copyMaintainMessage(item: Model.IVendorMaintenance) {
		const vm: any = this;
		vm.copyTextToClipboard(item.maintainMessage);
	}

	deleteItem(item: Model.IVendorMaintenance) {
		this.deleteID = item.id;
		this.isShowConfirmWarning = true;

		this.$notify({
			group: 'confirm',
			type: 'warn',
			duration: -1,
			speed: 10,
		});
	}

	confirmedAction() {
		VendorMaintenaceListApi.deleteVendorMaintenance(this.deleteID).then(() => {
			this.isShowConfirmWarning = false;
			this.deleteID = 0;
			this.refresh();
		});
	}

	removeAllMaintain() {
		if (confirm('是否確認關閉所有啟用中的維護設置?')) {
			VendorMaintenaceListApi.deleteAllVendorMaintenance().then(() => {
				EventBus.$emit(`successfully-modified`);
				this.refresh();
			});
		}
	}

	canceledAction() {
		this.isShowConfirmWarning = false;
	}

	getTime() {
		const date = new Date();
		const seperator1 = '-';
		const seperator2 = ':';
		let month: string | number = date.getMonth() + 1;
		let strDate: string | number = date.getDate();
		if (month >= 1 && month <= 9) {
			month = '0' + month;
		}
		if (strDate >= 0 && strDate <= 9) {
			strDate = '0' + strDate;
		}
		const currentdate = date.getFullYear() + seperator1 + month + seperator1 + strDate + ' ' + date.getHours() + seperator2 + date.getMinutes() + seperator2 + date.getSeconds();
		this.currentTime = timeWithTimezone(currentdate, this.userProfile.timeZone);
	}

	mounted() {
		DropdownApi.getGameVendors().then((res) => {
			this.dropdownGameVendors = res;

			this.dropdownGameVendors.push(<DropdownModel.IDropdownItems>{
				text: 'All (全遊戲商維護)',
				value: this.selectAllVendors,
			});
		});

		this.refresh();
	}
}
</script>



<style scoped>
</style>