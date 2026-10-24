<template>
	<v-app v-if="renderComponent" class="app">

		<!-- 這裡是放一些常用的前端html語法 (樣板複製用) -->
		<!-- 這裡是放一些常用的前端html語法 (樣板複製用) -->
		<!-- 這裡是放一些常用的前端html語法 (樣板複製用) -->
		<!-- 這裡是放一些常用的前端html語法 (樣板複製用) -->
		<!-- 這裡是放一些常用的前端html語法 (樣板複製用) -->
		<!-- 這裡是放一些常用的前端html語法 (樣板複製用) -->

		<!-- #001 彈窗v-dialog -->
		<v-dialog v-model="dialogVisible.isShowCreate" max-width="600px" scrollable>

			<v-card>

				<v-card-title class="grey darken-2">

					{{ $t('TableHeader.__prompt') }} 我的標題XXXXXXXXXXXXXXXX
					<div class="top-right-item">
						<div class="close mr-2" @click="closeAllModal">
							<v-icon>close</v-icon>
						</div>
					</div>

				</v-card-title>

				<v-card-title>

					<v-row>

					</v-row>

				</v-card-title>

				<v-card-actions>
					<v-spacer></v-spacer>

					<v-btn color="green lighten-1" text @click="searchNotSettleRecord()" class="btn">
						<v-icon left>check</v-icon>
						{{ $t('Button.__save') }}
					</v-btn>
				</v-card-actions>

			</v-card>

		</v-dialog>

	</v-app>

</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import * as Model from '@/models/interfaces/system-setting/accountingInvoice';
import * as CurrencyModel from '@/models/interfaces/system-setting/currency';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import * as TagModel from '@/models/interfaces/game-management/generalTag';
import * as EventModel from '@/models/interfaces/system-setting/events';
import AccountingInvoiceApi from '@/api/system-setting/accountingInvoice';
import CurrencyApi from '@/api/system-setting/currency';
import DropdownApi from '@/api/commons/dropdown';
import TagApi from '@/api/commons/generalTag';
import DatePicker from 'vue2-datepicker';
import { Locale, Currency, Status, GameVendorType } from '@/models/status/common';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { commonMethodMixin } from '@/utilities/common-methods';
import { pageData, time } from '@/utilities/api-handler.lib';
import { datetimeMixin, startEndDate, timeWithTimezone, daysAgo, weeksAgo, monthsAgo, formatMixin } from '@/utilities/datetime-format';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import { State, Action, Getter, namespace } from 'vuex-class';
const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';
import { uuid } from 'vue-uuid';

// @ts-ignore
@Component({ components: { DatePicker }, mixins: [commonDisplayFiltersMixin, commonMethodMixin, datetimeMixin] })
export default class AccountingInvoice extends Vue {
	get hasExportInvoicePermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.System_AccountingInvoice_ExportInvoice) >= 0;
	}

	get headers() {
		const data = [
			{ text: this.$t('TableHeader.__action'), value: 'action', sortable: false, align: 'center' },
			{ text: this.$t('TableHeader.__ID'), value: 'tagID', sortable: false, align: 'start' },
			{ text: this.$t('TableHeader.__type'), value: 'typeText', sortable: false, align: 'start' },
			{ text: this.$t('TableHeader.__displayName'), value: 'name', sortable: false, align: 'start' },
			{ text: this.$t('TableHeader.__memo'), value: 'memo', sortable: false, align: 'start' },
			{ text: '已分配筆數', value: 'assignTargetCount', sortable: false, align: 'start' },
			{ text: this.$t('TableHeader.__createTime'), value: 'createTime', sortable: false, align: 'start' },
			{ text: '', value: 'xxxxx', sortable: false, align: 'start', width: '25%' },
		];

		return data;
	}

	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];

	dialogVisible: DropdownModel.DialogVisible = new DropdownModel.DialogVisible();

	renderComponent: boolean = true;

	searchResult: TagModel.IGeneralTagResponse[] = [];
	searchFilter: TagModel.IGeneralTagRequest = {} as TagModel.IGeneralTagRequest;
	createForm: TagModel.IGeneralTagCreateRequest = {} as TagModel.IGeneralTagCreateRequest;
	updateAssignerForm: TagModel.IGeneralTagAssignerUpdateRequest = {} as TagModel.IGeneralTagAssignerUpdateRequest;

	DataTablePagingParameter: DropdownModel.DataTablePagingParameter = new DropdownModel.DataTablePagingParameter();
	datetimeRangePicker: DropdownModel.DatetimeRangePicker = new DropdownModel.DatetimeRangePicker();

	panel: number[] = [0];

	dropdownItemsLatestMonth: DropdownModel.IDropdownItems[] = [];
	dropdownItemsStatus: DropdownModel.IDropdownItems[] = [];
	dropdownItemsMerchants: DropdownModel.IDropdownItems[] = [];

	@Action('Players/setEventID') private setEventID!: any;

	changePage(index: number = 1) {
		this.DataTablePagingParameter.pageIndex = index;
		this.refresh();
	}

	changeItemPerPage(val: number = 10) {
		this.DataTablePagingParameter.itemsPerPage = val;
		this.refresh();
	}

	pageOneSearch() {
		this.DataTablePagingParameter.pageIndex = 1;
		this.refresh();
	}

	refresh() {
		const request = this.searchFilter;
		request.pageIndex = this.DataTablePagingParameter.pageIndex;
		request.pageSize = this.DataTablePagingParameter.itemsPerPage;

		TagApi.getPagingList(this.searchFilter).then((res) => {
			this.searchResult = res.data.items;
			this.DataTablePagingParameter.totalCount = res.data.totalCount;
		});
	}

	closeAllModal() {
		this.dialogVisible.resetAllValueToFalse();
	}

	clear() {}

	copySuccess() {
		EventBus.$emit(`successfully-copy`);
	}

	mounted() {
		this.refresh();

		DropdownApi.getNormalStatus().then((res) => {
			this.dropdownItemsStatus = res;
		});

		DropdownApi.getMerchants().then((res) => {
			this.dropdownItemsMerchants = res;
		});

		DropdownApi.getLatestMonth().then((res) => {
			this.dropdownItemsLatestMonth = res;
		});
	}
}
</script>


<style scoped>
</style>