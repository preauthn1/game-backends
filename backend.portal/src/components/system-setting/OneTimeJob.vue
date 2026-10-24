<template>
	<v-app v-if="renderComponent" class="app">
		<v-expansion-panels v-model="panel" multiple>
			<v-expansion-panel>
				<v-expansion-panel-content>
					<v-flex>
						<v-row class="search-form">
							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-text-field :label="$t('TableHeader.__keyword')" v-model="searchFilter.keyword"> </v-text-field>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete clearable :items="dropdownItemsTypes" v-model="searchFilter.type" :label="$t('TableHeader.__type')"> </v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item search-row-padding">
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
						</v-row>
					</v-flex>
				</v-expansion-panel-content>
			</v-expansion-panel>
		</v-expansion-panels>

		<v-data-table
			:headers="headers"
			id="table"
			:items="searchList"
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
						<h4></h4>
					</v-toolbar-title>
					<v-divider class="mx-4" inset vertical></v-divider>
					<v-spacer></v-spacer>
					<v-card-title>
						<v-btn icon dark @click="showQuickCreateNewEvent" style="margin-left: 15px">
							<v-icon>add</v-icon>
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
						<v-list-item class="dropdownMenu"> </v-list-item>
					</v-list>
				</v-menu>
			</template>

			<template v-slot:item.displayName="{ item }">
				<div class="double-row-td">
					<div :title="item.eventID">
						{{ item.displayName }}
					</div>

					<div style="color: #999; font-size: 12px; line-height: 10px">
						{{ item.eventID }}
					</div>
				</div>
			</template>

			<template v-slot:item.eventTypeText="{ item }">
				<!-- <v-icon small color="grey lighten-2">
					{{iconsByEventType(item.eventType)}}
				</v-icon> -->

				<span :title="item.eventType">
					{{ item.eventTypeText }}
					<!-- <v-chip :class="'eventType_' + item.eventType">
						{{ item.eventTypeText }}
					</v-chip> -->
				</span>
			</template>

			<template v-slot:item.status="{ item }">
				<span :class="item.status == 0 ? 'minus-red' : item.isAlreadyEnd ? 'grey-text' : ''">
					{{ item.statusText }}
				</span>

				<!-- 
				<span v-if="item.status == 1 && item.isAlreadyEnd" class="ml-1 grey-text">
					(時間截止)
				</span>
				 -->
			</template>

			<template v-slot:item.availableMerchantName="{ item }">
				<div style="max-width: 200px; overflow: hidden; white-space: nowrap; text-overflow: ellipsis">
					{{ item.availableMerchantName }}
				</div>
			</template>

			<template v-slot:item.endTime="{ item }">
				<span :class="begintTimeStyle(item)">
					{{ item.endTime | timeWithAddTimezone(userProfile.timeZone) }}
				</span>
			</template>

			<template v-slot:item.extraTags="{ item }">
				<v-chip v-for="(tag, index) in item.extraTags" small class="blue-grey darken-1" :key="index">
					{{ tag }}
				</v-chip>
			</template>

			<template v-slot:item.createTime="{ item }">
				{{ item.createTime | timeWithAddTimezone(userProfile.timeZone) }}
			</template>

			<template v-slot:item.nextTime="{ item }">
				{{ item.nextTime | timeWithAddTimezone(userProfile.timeZone) }}
			</template>
		</v-data-table>

		<!-- 新增 -->
		<v-dialog v-model="isShowCreateEventDialog" max-width="500px" persistent>
			<v-card>
				<v-card-title class="grey darken-2">
					新增
					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark @click="refresh">
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>
				<v-card-text style="padding: 10px 30px">
					<v-flex class="">
						<v-row>
							<v-autocomplete class="" attach chips :label="$t('TableHeader.__type')" @change="whenChangeType()" v-model="createForm.type" :items="dropdownItemsTypes"> </v-autocomplete>
						</v-row>

						<v-row>
							<v-text-field class="" v-model="createForm.memo" :label="$t('TableHeader.__memoOrInternalDisplayName')" autofocus required clearable> </v-text-field>
						</v-row>

						<v-row>
							<div class="lineDiv f12">下次執行時間</div>

							<date-picker v-model="createForm.nextTimeNumber" type="datetime" value-type="timestamp" @change="reloadCreateDialog()" format="YYYY-MM-DD HH:mm"> </date-picker>

							<v-chip class="ml-1" @click="adjustDatepicker(1 * 60)">1小時</v-chip>
							<v-chip class="ml-1" @click="adjustDatepicker(2 * 60)">2小時</v-chip>
							<v-chip class="ml-1" @click="adjustDatepicker(4 * 60)">4小時</v-chip>
						</v-row>

						<v-row class="mt-4">
							<v-textarea v-model="createForm.settingJson" :label="'配置(json格式或字串)'" clearable style="background: #333; padding: 5px"> </v-textarea>
						</v-row>
					</v-flex>
				</v-card-text>
				<v-card-actions>
					<v-spacer></v-spacer>
					<v-btn text @click="closeAllModal" class="btn">
						<v-icon left>mdi-cancel</v-icon>
						{{ $t('Button.__cancel') }}
					</v-btn>
					<v-btn color="yellow darken-1" text @click="saveQuickCreateNewEvent" class="btn">
						<v-icon left>mdi-check-circle</v-icon>
						{{ $t('Button.__submit') }}
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
import * as AnnModel from '@/models/interfaces/system-setting/announcement';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import * as EventModel from '@/models/interfaces/system-setting/events';
import * as Model from '@/models/interfaces/system-setting/oneTimeJob';

import MerchantsApi from '@/api/system-setting/merchants';
import AnnouncementApi from '@/api/system-setting/announcement';
import DropdownApi from '@/api/commons/dropdown';
import OneTimeJobApi from '@/api/system-setting/oneTimeJob';
import DatePicker from 'vue2-datepicker';
import { Locale, Currency, Status, GameVendorType } from '@/models/status/common';

import AnnouncementDetail from '@/components/system-setting/AnnouncementDetail.vue';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { commonMethodMixin } from '@/utilities/common-methods';
import NotificationWarning from '@/components/notification/NotificationWarning.vue';
import ImageUploadDialog from '@/components/system-setting/ImageUploadDialog.vue';
import { pageData, time } from '@/utilities/api-handler.lib';
import { datetimeMixin, startEndDate, timeWithTimezone, daysAgo, toLocalDateStringYYYYMMDD, weeksAgo, monthsAgo, formatMixin } from '@/utilities/datetime-format';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import { State, Action, Getter, namespace } from 'vuex-class';
const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';
import { uuid } from 'vue-uuid';

// @ts-ignore
@Component({ components: { NotificationWarning, DatePicker, AnnouncementDetail, ImageUploadDialog }, mixins: [commonDisplayFiltersMixin, commonMethodMixin, datetimeMixin] })
export default class OneTimeJob extends Vue {
	get hasPermissionCreate() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSettingMerchant_Create) >= 0;
	}
	get hasPermissionUpdate() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSettingMerchant_Update) >= 0;
	}
	get hasPermissionDelete() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSettingMerchant_Delete) >= 0;
	}

	get headers() {
		const data = [
			{ text: this.$t('TableHeader.__action'), value: 'action', sortable: false, align: 'center', width: '5%' },
			{ text: this.$t('TableHeader.__ID'), value: 'jobID', sortable: false, width: '7%' },
			{ text: this.$t('TableHeader.__type'), value: 'typeText', sortable: false, width: '7%' },

			{ text: this.$t('TableHeader.__memoOrInternalDisplayName'), value: 'memo', sortable: false },
			{ text: this.$t('TableHeader.__status'), value: 'status', sortable: false, width: '7%' },

			{ text: '重試次數', value: 'retryTimes', sortable: false },
			{ text: '下次執行時間', value: 'nextTime', sortable: false },
			{ text: this.$t('TableHeader.__createTime'), value: 'createTime', sortable: false },
			{ text: '唯一鍵值', value: 'uniqueKey', sortable: false },
		];

		return data;
	}

	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];

	isShowDeleteWarning: boolean = false;
	isShowDetail: boolean = false;
	isShowCreateEventDialog: boolean = false;
	renderComponent: boolean = true;

	perPage: number = 100;

	DataTablePagingParameter: DropdownModel.DataTablePagingParameter = new DropdownModel.DataTablePagingParameter();

	searchList: Model.IOneTimeJobViewModel[] = [];
	searchFilter: Model.IQueryOneTimeJobRequest = {} as Model.IQueryOneTimeJobRequest;
	createForm: Model.IOneTimeJobCreateRequest = {} as Model.IOneTimeJobCreateRequest;

	panel: number[] = [0];

	dropdownItemsStatus: DropdownModel.IDropdownItems[] = [];
	dropdownItemsMerchants: DropdownModel.IDropdownItems[] = [];
	dropdownItemsTypes: DropdownModel.IDropdownItems[] = [];

	@Action('Players/setEventID') private setEventID!: any;

	changePage(index: number = 1) {
		this.DataTablePagingParameter.pageIndex = index;
		this.refresh();
	}

	changeItemPerPage(val: number = 10) {
		this.DataTablePagingParameter.itemsPerPage = val;
		this.refresh();
	}

	whenChangeType() {
		if (this.createForm.type === 5252) {
			this.createForm.settingJson = '{ \n  "SPName" : "XXXXXXXXX" ,\n  "DBName" : "MasterDB 【DBConnType】"\n  "Memo" : "注意SP必須返回 result_isSuccess／result_msg"\n}';
			this.reloadCreateDialog();
		} else {
			this.createForm.settingJson = '';
		}
	}

	search() {
		this.DataTablePagingParameter.pageIndex = 1;
		this.refresh();
	}

	refresh() {
		this.registerReloadComponent();

		this.searchFilter.pageIndex = this.DataTablePagingParameter.pageIndex;
		this.searchFilter.pageSize = this.DataTablePagingParameter.itemsPerPage;

		this.searchFilter.isASC = false;

		OneTimeJobApi.getList(this.searchFilter).then((res) => {
			this.searchList = res.data.items;
			this.DataTablePagingParameter.totalCount = res.data.totalCount;
		});
	}

	showQuickCreateNewEvent() {
		this.createForm = {} as Model.IOneTimeJobCreateRequest;
		this.createForm.nextTimeNumber = daysAgo(0);
		this.createForm.type = 9001;
		this.isShowCreateEventDialog = true;
	}

	adjustDatepicker(minutes: number) {
		this.createForm.nextTimeNumber = Date.now() + 60 * minutes * 1000;
		this.reloadCreateDialog();
	}

	reloadCreateDialog() {
		this.isShowCreateEventDialog = false;

		setTimeout(() => {
			this.isShowCreateEventDialog = true;
		}, 50);
	}

	saveQuickCreateNewEvent() {
		EventBus.$emit('show_mask', null);
		this.createForm.nextTime = startEndDate(Number(this.createForm.nextTimeNumber));

		OneTimeJobApi.create(this.createForm)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);

				this.closeAllModal();
				this.search();
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

	closeAllModal() {
		this.isShowDetail = false;
		this.isShowCreateEventDialog = false;
		this.isShowDeleteWarning = false;
	}

	clear() {
		this.searchFilter = {} as AnnModel.IQueryAnnouncementApiParameter;
	}

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

		DropdownApi.getOneTimesJobTypes().then((res) => {
			this.dropdownItemsTypes = res;
		});
	}
}
</script>

<style scoped>
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

.hint-warning {
	color: #ef4c4c;
	font-size: 14px;
}

.hint-warning i {
	color: #ef4c4c !important;
	font-size: 16px !important;
}

.eventType_32 {
	/* 紅包 */
	background: #863f3f !important;
}

.eventType_51 {
	/* 甜蜜期 (顏色太多反而炫目) */
	/* background: #9c719e !important; */
}
</style>
