<template>
	<v-app v-if="renderComponent" class="app">
		<v-expansion-panels v-model="panel" multiple>
			<v-expansion-panel>
				<v-expansion-panel-content>
					<v-flex>
						<v-row class="search-form">
							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-text-field :label="$t('TableHeader.__userName')" clearable v-model="searchFilter.userName"> </v-text-field>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete :items="dropdownItemsOperateType" clearable v-model="searchFilter.OperateType" :label="$t('TableHeader.__type')"> </v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-text-field :label="$t('TableHeader.__iPAddress')" clearable v-model="searchFilter.createIPAddress"> </v-text-field>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-text-field :label="'TargetID'" clearable v-model="searchFilter.targetID"> </v-text-field>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="">
									<date-picker
										v-model="datetimeRangePicker.searchStartTimeRange"
										type="datetime"
										v-bind:clearable="false"
										value-type="timestamp"
										range
										:placeholder="$t('TableHeader.__createTime2')"
									>
									</date-picker>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="mt-3">
									<v-chip class="mr-1" small :class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'today' }" @click="globalQuicklyPickTimeRange('today')">
										<v-icon left>mdi-alarm-check</v-icon>
										{{ $t(`TableHeader.__today`) }}
									</v-chip>
									<v-chip class="mr-1" small :class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'yesterday' }" @click="globalQuicklyPickTimeRange('yesterday')">
										<v-icon left>mdi-alarm-check</v-icon>
										{{ $t(`TableHeader.__yesterday`) }}
									</v-chip>
									<v-chip class="mr-1" small :class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'thisWeek' }" @click="globalQuicklyPickTimeRange('thisWeek')">
										<v-icon left>mdi-alarm-check</v-icon>
										{{ $t(`TableHeader.__thisWeek`) }}
									</v-chip>

									<v-chip class="mr-1" small :class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'lastWeek' }" @click="globalQuicklyPickTimeRange('lastWeek')">
										<v-icon left>mdi-alarm-check</v-icon>
										{{ $t(`TableHeader.__lastWeek`) }}
									</v-chip>
									<v-chip class="mr-1" small :class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'thisMonth' }" @click="globalQuicklyPickTimeRange('thisMonth')">
										<v-icon left>mdi-alarm-check</v-icon>
										{{ $t(`TableHeader.__thisMonth`) }}
									</v-chip>
									<v-chip class="mr-1" small :class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'lastMonth' }" @click="globalQuicklyPickTimeRange('lastMonth')">
										<v-icon left>mdi-alarm-check</v-icon>
										{{ $t(`TableHeader.__lastMonth`) }}
									</v-chip>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="">
									<v-autocomplete :items="dropdownItemsOrganization" clearable v-model="searchFilter.organizationID" :label="$t('TableHeader.__organization')"> </v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="search-row-padding">
									<v-btn color="yellow darken-1" text class="btn search" @click.stop="search">
										<v-icon left>search</v-icon>
										{{ $t('Button.__search') }} </v-btn
									>&#9;
									<v-btn color="pink lighten-3" text class="btn search" @click.stop="clear">
										<v-icon left>clear</v-icon>
										{{ $t('Button.__reset') }} </v-btn
									>&#9;
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3" class="minus-mt-6" v-if="hasSuperAdminPermission">
								<div class="">
									<v-text-field :label="'deviceID'" clearable v-model="searchFilter.deviceID"> </v-text-field>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3" class="minus-mt-6" v-if="hasSuperAdminPermission">
								<div class="">
									<v-text-field :label="'userID'" clearable v-model="searchFilter.userID"> </v-text-field>
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
			multi-sort
		>
			<template v-slot:item.action="{ item }"> </template>

			<template v-slot:item.targetID="{ item }">
				<span v-if="item.targetID > 0"> {{ item.targetID }}</span>
				<span v-else>{{ item.targetKey }}</span>
			</template>

			<template v-slot:item.operateTypeText="{ item }">
				<div :title="item.operateType">{{ item.operateTypeText }}</div>
				<div class="sub-title-text">{{ item.id }}</div>
			</template>

			<template v-slot:item.debugInfo="{ item }">
				<span class="maxWidthBeforeClick maxWidth150" @click="expandMaxWidth($event)">
					{{ item.debugInfo }}
				</span>
			</template>

			<template v-slot:item.operateUserName="{ item }">
				<div>{{ item.operateUserName }}</div>
				<div class="sub-title-text" v-if="hasSuperAdminPermission">
					{{ item.userID }}
				</div>
			</template>

			<template v-slot:item.createTime="{ item }">
				<span class="copyTextTo" @click="globalPopupCopyDateTime(item.createTime)" v-if="hasSuperAdminPermission">
					{{ item.createTime | timeWithAddTimezone(userProfile.timeZone) }}
				</span>

				<span v-else>
					{{ item.createTime | timeWithAddTimezone(userProfile.timeZone) }}
				</span>
			</template>
		</v-data-table>

		<ExternalApiRequestLogDetailPopup ref="logDetailPopup"> </ExternalApiRequestLogDetailPopup>

		<PopupCopyDatetime ref="refCopyDateTime"></PopupCopyDatetime>
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import ExternalApiRequestLogDetailPopup from '@/components/system-logs/ExternalApiRequestLogDetailPopup.vue';
import PopupCopyDatetime from '@/components/PopupCopyDatetime.vue';
import EventBus from '@/utilities/event-bus';
import * as SettingModel from '@/models/interfaces/system-setting/systemConfigSetting';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import * as Model from '@/models/interfaces/system-logs/externalApiRequestLog';
import * as RecordModel from '@/models/interfaces/members-management/records';
import LogApi from '@/api/system-logs/externalApiRequestLog';
import DropdownApi from '@/api/commons/dropdown';
import { Locale, Currency, Status, GameType, CRUDAction, GameVendorType } from '@/models/status/common';
import { WalletMode } from '@/models/status/merchant';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { ALL_CURRENCY } from '@/utilities/enums_helper';
import NotificationWarning from '@/components/notification/NotificationWarning.vue';
import { pageData, time } from '@/utilities/api-handler.lib';
// import Validation from '@/utilities/validation';
import { datetimeMixin, startEndDate, daysAgo } from '@/utilities/datetime-format';
import DatePicker from 'vue2-datepicker';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import { commonMethodMixin } from '@/utilities/common-methods';
import { State, Action, Getter, namespace } from 'vuex-class';

const authModule = namespace('Auth');

import * as Permission from '@/models/status/permission';

// @ts-ignore
@Component({ components: { NotificationWarning, ExternalApiRequestLogDetailPopup, DatePicker, PopupCopyDatetime }, mixins: [commonDisplayFiltersMixin, commonMethodMixin, datetimeMixin] })
export default class ExternalApiRequestLog extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];
	renderComponent: boolean = true;
	isShowInfoDialog: boolean = false;
	panel: number[] = [0];

	datetimeRangePicker: DropdownModel.DatetimeRangePicker = new DropdownModel.DatetimeRangePicker();

	searchResult: RecordModel.IOperateRecordReponse[] = [];
	searchFilter: RecordModel.IOperateRecordRequest = {} as RecordModel.IOperateRecordRequest;

	DataTablePagingParameter: DropdownModel.DataTablePagingParameter = new DropdownModel.DataTablePagingParameter();
	dropdownItemsOperateType: DropdownModel.IDropdownItems[] = [];
	dropdownItemsOrganization: DropdownModel.IDropdownItems[] = [];

	get hasSuperAdminPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSetting_SystemConfigSetting) >= 0;
	}

	get headers() {
		const data: any[] = [];

		data.push({ text: '', value: 'action', sortable: false, align: 'start', width: '3%' });
		data.push({ text: this.$t('TableHeader.__createTime2'), value: 'createTime', sortable: false, width: '8%' });
		data.push({ text: this.$t('TableHeader.__type'), value: 'operateTypeText', sortable: false, width: '10%' });
		data.push({ text: this.$t('TableHeader.__updateBy'), value: 'operateUserName', sortable: false, width: '8%' });
		data.push({ text: this.$t('TableHeader.__iPAddress'), value: 'createIPAddress', sortable: false, width: '5%' });
		data.push({ text: this.$t('TableHeader.__memo'), value: 'memo', sortable: false, width: '20%' });

		if (this.hasSuperAdminPermission) {
			data.push({ text: 'targetID (更新对象)', value: 'targetID', sortable: false, width: '8%' });
			data.push({ text: this.$t('TableHeader.__memo') + '(2)', value: 'debugInfo', sortable: false });
			data.push({ text: this.$t('TableHeader.__device'), value: 'deviceTypeText', sortable: false, width: '5%' });
			data.push({ text: this.$t('TableHeader.__device'), value: 'deviceID', sortable: false, width: '10%' });
		}
		return data;
	}

	clear() {
		this.searchFilter = {} as RecordModel.IOperateRecordRequest;
	}

	search() {
		this.DataTablePagingParameter.pageIndex = 1;
		this.refresh();
	}

	refresh() {
		this.registerReloadComponent();

		this.searchFilter.pageIndex = this.DataTablePagingParameter.pageIndex;
		this.searchFilter.pageSize = this.DataTablePagingParameter.itemsPerPage;
		this.searchFilter.createTimeBegin = this.datetimeRangePicker.searchStartTimeRange[0] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[0]) : null;
		this.searchFilter.createTimeEnd = this.datetimeRangePicker.searchStartTimeRange[1] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[1]) : null;

		EventBus.$emit('show_mask', null);

		LogApi.getUserOperateRecords(this.searchFilter)
			.then((res) => {
				this.searchResult = res.data.items;
				this.DataTablePagingParameter.totalCount = res.data.totalCount;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
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

	close() {
		this.isShowInfoDialog = false;
	}

	save() {}

	mounted() {
		this.datetimeRangePicker.searchStartTimeRange = [daysAgo(-1), daysAgo(1)];

		DropdownApi.getUserOperateTypes().then((res) => {
			this.dropdownItemsOperateType = res;
		});

		DropdownApi.getOrganizations().then((res) => {
			this.dropdownItemsOrganization = res;
		});
	}
}
</script>
