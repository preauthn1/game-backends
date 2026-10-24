<template>
	<v-app v-if="renderComponent" class="app">

		<v-expansion-panels v-model="panel" multiple>
			<v-expansion-panel>

				<v-expansion-panel-content>
					<v-flex>
						<v-row class="search-form">

							<v-col cols="12" sm="3" md="3">
								<div class="item">

									<v-autocomplete clearable :items="dropdownItemsStatus"
										v-model="searchFilter.status" :label="$t('TableHeader.__status')">
									</v-autocomplete>

								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">

									<v-autocomplete clearable :items="dropdownItemsMerchants"
										v-model="searchFilter.merchantID"
										:label="$t('TableHeader.__assignMerchantTotalCount')">
									</v-autocomplete>

								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item search-row-padding">
									<v-btn color="yellow darken-1" text class="btn search" @click.stop="search">
										<v-icon left>search</v-icon>
										{{$t('Button.__search')}}
									</v-btn>&#9;
									<v-btn color="pink lighten-3" text class="btn search" @click.stop="clear">
										<v-icon left>clear</v-icon>
										{{$t('Button.__reset')}}
									</v-btn>&#9;

									<v-btn color="blue lighten-3" text class="btn search"
										@click.stop="showImageUpload">
										<v-icon left>file_upload</v-icon>
										上傳圖片
									</v-btn>&#9;

								</div>
							</v-col>

							<!-- 

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<date-picker v-model="datetimeRangePicker.searchStartTimeRange" type="datetime"
										value-type="timestamp" range :placeholder="$t('TableHeader.__createTime2')">
									</date-picker>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">

								<v-chip class="mr-1"
									:class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'today' }"
									@click="globalQuicklyPickTimeRange('today')">
									<v-icon left>mdi-alarm-check</v-icon>
									{{ $t(`TableHeader.__today`) }}
								</v-chip>
								<v-chip class="mr-1"
									:class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'yesterday' }"
									@click="globalQuicklyPickTimeRange('yesterday')">
									<v-icon left>mdi-alarm-check</v-icon>
									{{ $t(`TableHeader.__yesterday`) }}
								</v-chip>
								<v-chip class="mr-1"
									:class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'thisWeek' }"
									@click="globalQuicklyPickTimeRange('thisWeek')">
									<v-icon left>mdi-alarm-check</v-icon>
									{{ $t(`TableHeader.__thisWeek`) }}
								</v-chip>

								<v-chip class="mr-1"
									:class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'lastWeek' }"
									@click="globalQuicklyPickTimeRange('lastWeek')">
									<v-icon left>mdi-alarm-check</v-icon>
									{{ $t(`TableHeader.__lastWeek`) }}
								</v-chip>
								<v-chip class="mr-1"
									:class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'thisMonth' }"
									@click="globalQuicklyPickTimeRange('thisMonth')">
									<v-icon left>mdi-alarm-check</v-icon>
									{{ $t(`TableHeader.__thisMonth`) }}
								</v-chip>
								<v-chip class="mr-1"
									:class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'lastMonth' }"
									@click="globalQuicklyPickTimeRange('lastMonth')">
									<v-icon left>mdi-alarm-check</v-icon>
									{{ $t(`TableHeader.__lastMonth`) }}
								</v-chip>

							</v-col> -->

						</v-row>
					</v-flex>
				</v-expansion-panel-content>
			</v-expansion-panel>
		</v-expansion-panels>

		<v-data-table :headers="headers" id="table" :items="searchList"
			:page="DataTablePagingParameter.pageIndex"
			:items-per-page="DataTablePagingParameter.itemsPerPage"
			:server-items-length="DataTablePagingParameter.totalCount"
			:footer-props="DataTablePagingParameter.footerProps" @update:page="changePage"
			@update:items-per-page="changeItemPerPage">

			<template v-slot:top>
				<v-toolbar>
					<v-toolbar-title>
						<h4>

							{{ $t('Menu.SystemSetting_Announcement') }}

						</h4>
					</v-toolbar-title>
					<v-divider class="mx-4" inset vertical></v-divider>
					<v-spacer></v-spacer>
					<v-card-title>

						<v-btn icon dark @click="showQuickCreateNewEvent" style="margin-left:15px;">
							<v-icon>add</v-icon>
						</v-btn>

					</v-card-title>

				</v-toolbar>
			</template>

			<template v-slot:item.action="{ item }">

				<v-menu offset-y>
					<template v-slot:activator="{ on, attrs }">

						<div class="dropdown-menu" v-bind="attrs" v-on="on">
							<v-icon dark outline>
								expand_more
							</v-icon>
						</div>

					</template>
					<v-list class="dropdonwMenuPanel">
						<v-list-item class="dropdownMenu">

							<v-list-item @click="showInfo(item.announcementID)">

								<v-icon>
									edit
								</v-icon>
								{{$t("Button.__edit")}}
							</v-list-item>

							<v-list-item @click="clearAllRead(item.announcementID , item.memo)">
								<v-icon>
									delete
								</v-icon>
								清除所有已讀
							</v-list-item>

							<v-list-item @click="showDeleteHint(item)">
								<v-icon>
									delete
								</v-icon>
								{{$t("Button.__delete")}}
							</v-list-item>

						</v-list-item>
					</v-list>
				</v-menu>
				<!-- 
				<v-tooltip top>
					<template v-slot:activator="{ on, attrs }">
						<v-icon dark v-bind="attrs" v-on="on" small class="mr-2"
							style="color:#CCC;font-size:20px;" @click="showInfo(item.eventID)">
							edit
						</v-icon>
					</template>
					<span>編輯</span>
				</v-tooltip>

				<v-tooltip top>
					<template v-slot:activator="{ on, attrs }">
						<v-icon dark v-bind="attrs" v-on="on" small class="mr-2"
							style="color:#CCC;font-size:20px;" @click="">
							assignment
						</v-icon>
					</template>
					<span>查看領獎紀錄</span>
				</v-tooltip>

				<v-tooltip top>
					<template v-slot:activator="{ on, attrs }">
						<v-icon dark v-bind="attrs" v-on="on" small class="mr-2"
							style="color:#CCC;font-size:20px;" @click="">
							delete
						</v-icon>
					</template>
					<span>刪除</span>
				</v-tooltip>

				<v-tooltip top>
					<template v-slot:activator="{ on, attrs }">
						<v-icon dark v-bind="attrs" v-on="on" small class="mr-2"
							style="color:#CCC;font-size:20px;" :disabled="item.eventType !== 51"
							@click="showResetRTPHint(item)">
							stars
						</v-icon>
					</template>
					<span>特殊操作</span>
				</v-tooltip> -->

			</template>

			<template v-slot:item.displayName="{ item }">

				<div class="double-row-td">
					<div :title="item.eventID">
						{{item.displayName}}
					</div>

					<div style="color:#999;font-size:12px;line-height: 10px;">
						{{item.eventID}}
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

			<template v-slot:item.beginTime="{ item }">

				<div :class="'double-row-td '">
					<div>
						<span :class="begintTimeStyle(item)">
							{{ item.beginTime | timeWithAddTimezone(userProfile.timeZone) }}
						</span>
					</div>

					<div>
						<span :class="begintTimeStyle(item)">
							{{ item.endTime | timeWithAddTimezone(userProfile.timeZone) }}
						</span>

					</div>
				</div>

			</template>

			<template v-slot:item.availableMerchantName="{ item }">

				<div style="max-width: 200px;overflow: hidden;white-space: nowrap;text-overflow: ellipsis;">
					{{ item.availableMerchantName  }}
				</div>

			</template>

			<template v-slot:item.endTime="{ item }">

				<span :class="begintTimeStyle(item)">
					{{ item.endTime | timeWithAddTimezone(userProfile.timeZone) }}
				</span>

			</template>

			<template v-slot:item.extraTags="{ item }">

				<v-chip v-for="(tag , index) in item.extraTags" small class="blue-grey darken-1"
					:key="index">
					{{tag}}
				</v-chip>

				<!--
	<span :class="begintTimeStyle(item)">
					{{ item.endTime | timeWithAddTimezone(userProfile.timeZone) }}
				</span> -->

			</template>

			<template v-slot:item.createTime="{ item }">
				{{ item.createTime | timeWithAddTimezone(userProfile.timeZone) }}
			</template>

			<template v-slot:item.updateTime="{ item }">
				{{ item.updateTime | timeWithAddTimezone(userProfile.timeZone) }}
			</template>

		</v-data-table>

		<!-- 快速新增 -->
		<v-dialog v-model="isShowCreateEventDialog" max-width="500px">

			<v-card>
				<v-card-title class="grey darken-2">
					快速新增
					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark @click="refresh">
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>
				<v-card-text style="padding-bottom: 100px;">
					<v-flex>

						<v-row>

							<v-text-field class="required" v-model="createEventFrom.memo"
								:label="$t('TableHeader.__memoOrInternalDisplayName')" autofocus required clearable>
							</v-text-field>

						</v-row>

						<v-row>
							<v-autocomplete class="required" attach chips :label="$t('TableHeader.__type')"
								v-model="createEventFrom.type" :items="dropdownItemsAnnouncementTypes">
							</v-autocomplete>
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

		<!-- 刪除活動 -->
		<v-dialog v-model="isShowDeleteWarning" max-width="500px">

			<v-card>
				<v-card-title class="grey darken-2">
					確定刪除?
					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>
				<v-card-text>
					<v-flex style="padding:10px;">

						<v-row>

							<v-text-field class="" v-model="deleteItem.announcementID" disabled="disabled"
								:label="$t('TableHeader.__ID')" autofocus>
							</v-text-field>

						</v-row>

						<v-row>

							<v-text-field class="" v-model="deleteItem.memo" disabled="disabled"
								:label="$t('TableHeader.__memoOrInternalDisplayName')" autofocus>
							</v-text-field>

						</v-row>

					</v-flex>
				</v-card-text>

				<v-card-actions>

					<v-spacer></v-spacer>

					<v-btn text @click="closeAllModal" class="btn">
						<v-icon left>mdi-cancel</v-icon>
						{{ $t('Button.__cancel') }}
					</v-btn>

					<v-btn color="yellow darken-1" text @click="confirmDeleteHint" class="btn">
						<v-icon left>mdi-check-circle</v-icon>
						{{ $t('Button.__submit') }}
					</v-btn>

				</v-card-actions>
			</v-card>

		</v-dialog>

		<v-dialog v-model="isShowDetail" persistent max-width="1280px">

			<AnnouncementDetail ref="refAnnouncementDetail" @closeParentAllModal="closeAllModal"
				@refreshList="refresh">
			</AnnouncementDetail>

		</v-dialog>

		<ImageUploadDialog ref="refImageUploadDialog" />

	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import * as AnnModel from '@/models/interfaces/system-setting/announcement';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import * as EventModel from '@/models/interfaces/system-setting/events';

import MerchantsApi from '@/api/system-setting/merchants';
import AnnouncementApi from '@/api/system-setting/announcement';
import DropdownApi from '@/api/commons/dropdown';
import DatePicker from 'vue2-datepicker';
import { Locale, Currency, Status, GameVendorType } from '@/models/status/common';

import AnnouncementDetail from '@/components/system-setting/AnnouncementDetail.vue';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { commonMethodMixin } from '@/utilities/common-methods';
import NotificationWarning from '@/components/notification/NotificationWarning.vue';
import ImageUploadDialog from '@/components/system-setting/ImageUploadDialog.vue';
import { pageData, time } from '@/utilities/api-handler.lib';
import { datetimeMixin, startEndDate, timeWithTimezone, daysAgo, weeksAgo, monthsAgo, formatMixin } from '@/utilities/datetime-format';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import { State, Action, Getter, namespace } from 'vuex-class';
const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';
import { uuid } from 'vue-uuid';

// @ts-ignore
@Component({ components: { NotificationWarning, DatePicker, AnnouncementDetail, ImageUploadDialog }, mixins: [commonDisplayFiltersMixin, commonMethodMixin, datetimeMixin] })
export default class Announcement extends Vue {
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
			{ text: this.$t('TableHeader.__type'), value: 'typeText', sortable: false, width: '7%' },
			// { text: this.$t('TableHeader.__eventID'), value: 'eventID', width: '8%', sortable: false },

			{ text: this.$t('TableHeader.__ID'), value: 'announcementID', sortable: false, width: '7%' },
			{ text: this.$t('TableHeader.__memoOrInternalDisplayName'), value: 'memo', sortable: false },
			{ text: this.$t('TableHeader.__beginTime') + ' - ' + this.$t('TableHeader.__endTime'), value: 'beginTime', sortable: false, width: '10%' },
			{ text: this.$t('TableHeader.__status'), value: 'status', sortable: false, width: '7%' },

			// { text: this.$t('TableHeader.__endTime'), value: 'endTime', sortable: false },
			// { text: this.$t('TableHeader.__supportGames'), value: 'numberOfGames', sortable: false, width: '5%' },
			// { text: this.$t('TableHeader.__supportMerchants'), value: 'numberOfMerchants', sortable: false, width: '5%' },
			// { text: this.$t('TableHeader.__tag'), value: 'extraTags', sortable: false, width: '5%' },
			{ text: this.$t('TableHeader.__updateBy'), value: 'updateBy', sortable: false },
			{ text: this.$t('TableHeader.__createTime'), value: 'createTime', sortable: false },
			{ text: this.$t('TableHeader.__updateTime'), value: 'updateTime', sortable: false },
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

	DataTablePagingParameterSize500: DropdownModel.DataTablePagingParameterSize500 = new DropdownModel.DataTablePagingParameterSize500();
	DataTablePagingParameter: DropdownModel.DataTablePagingParameter = new DropdownModel.DataTablePagingParameter();
	datetimeRangePicker: DropdownModel.DatetimeRangePicker = new DropdownModel.DatetimeRangePicker();

	searchList: AnnModel.IAnnouncementListResponse[] = [];
	searchFilter: AnnModel.IQueryAnnouncementApiParameter = {} as AnnModel.IQueryAnnouncementApiParameter;
	createEventFrom: AnnModel.IAnnouncementCreateRequest = {} as AnnModel.IAnnouncementCreateRequest;
	deleteItem: AnnModel.IAnnouncementListResponse = {} as AnnModel.IAnnouncementListResponse;

	panel: number[] = [0];

	dropdownItemsStatus: DropdownModel.IDropdownItems[] = [];
	dropdownItemsMerchants: DropdownModel.IDropdownItems[] = [];
	dropdownItemsAnnouncementTypes: DropdownModel.IDropdownItems[] = [];

	@Action('Players/setEventID') private setEventID!: any;

	changePage(index: number = 1) {
		this.DataTablePagingParameter.pageIndex = index;
		this.refresh();
	}

	changeItemPerPage(val: number = 10) {
		this.DataTablePagingParameter.itemsPerPage = val;
		this.refresh();
	}

	begintTimeStyle(item: any) {
		if (item.isExpired) {
			return 'light-opacity';
		}
		if (item.status === 0 && item.isInProgress) {
			return 'minus-red';
		}

		return '';
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

		AnnouncementApi.getList(this.searchFilter).then((res) => {
			this.searchList = res.data.items;
			this.DataTablePagingParameter.totalCount = res.data.totalCount;
		});
	}

	showQuickCreateNewEvent() {
		this.isShowCreateEventDialog = true;
		this.createEventFrom = {} as AnnModel.IAnnouncementCreateRequest;
		this.createEventFrom.type = this.dropdownItemsAnnouncementTypes[0].valueInt;
	}

	saveQuickCreateNewEvent() {
		EventBus.$emit('show_mask', null);

		AnnouncementApi.quickCreate(this.createEventFrom)
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

	showImageUpload() {
		const targetRef: any = this.$refs.refImageUploadDialog;
		targetRef.showDialogAndEmptyValue();
	}

	showDeleteHint(item: AnnModel.IAnnouncementListResponse) {
		const vm: any = this;

		this.isShowDeleteWarning = true;
		this.deleteItem = item;
	}

	clearAllRead(announcementID: number, targetName: string) {
		const msg: string = '確認進行此操作 [清除所有已讀] ? (' + targetName + ')';
		if (confirm(msg)) {
			AnnouncementApi.clearAllRead(announcementID)
				.then((res) => {
					EventBus.$emit(`successfully-modified`);
					this.closeAllModal();
				})
				.finally(() => {
					EventBus.$emit('hide_mask', null);
				});
		}
	}

	confirmDeleteHint() {
		const vm: any = this;
		EventBus.$emit('show_mask', null);

		AnnouncementApi.delete(this.deleteItem.announcementID)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.closeAllModal();
				this.search();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	showInfo(id: number) {
		this.showDetail(id);
	}

	showDetail(id: number) {
		this.isShowDetail = true;
		this.$nextTick(() => {
			const myRef: any = this.$refs.refAnnouncementDetail;
			myRef.queryTargetEventID(id);
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

		DropdownApi.getAnnouncementTypes().then((res) => {
			this.dropdownItemsAnnouncementTypes = res;
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