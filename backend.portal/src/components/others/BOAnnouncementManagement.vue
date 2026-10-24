<template>
	<v-app v-if="isRenderComponent" class="app">

		<v-expansion-panels v-model="panel" multiple>
			<v-expansion-panel>

				<v-expansion-panel-content>
					<v-flex>
						<v-row class="search-form padding-bottom-in-phone">

							<v-col cols="12" sm="3" md="3">
								<div class="item">

									<v-text-field v-model="searchFilter.keyword" :label="$t('TableHeader.__keyword')"
										clearable>
									</v-text-field>

								</div>
							</v-col>

							<!-- <v-col cols="12" sm="3" md="3">
								 <div class="item">

									<v-autocomplete clearable :items="dropdownItemsEventType"
										v-model="searchFilter.eventType" :label="$t('TableHeader.__type')">
									</v-autocomplete>

								</div> 
							</v-col> -->

							<v-col cols="12" sm="6" md="6">
								<div class="item search-row-padding">
									<v-btn color="yellow darken-1" text class="btn search mt-1" @click.stop="search">
										<v-icon left>search</v-icon>
										{{$t('Button.__search')}}
									</v-btn>&#9;

									<v-btn color="pink lighten-3" text class="btn search mt-1" @click.stop="clear">
										<v-icon left>clear</v-icon>
										{{$t('Button.__reset')}}
									</v-btn>&#9;

									<v-btn color="light-blue lighten-3" text class="btn search mt-1"
										v-if="hasPermissionToCreate" @click.stop="showCreateDialog">
										<v-icon left>add</v-icon>
										{{$t('Button.__create')}}
									</v-btn>&#9;

								</div>
							</v-col>

						</v-row>

					</v-flex>
				</v-expansion-panel-content>
			</v-expansion-panel>
		</v-expansion-panels>

		<v-data-table :headers="headers" id="table" :items="searchList" class="mt-2"
			:page="DataTablePagingParameter.pageIndex"
			:items-per-page="DataTablePagingParameter.itemsPerPage"
			:server-items-length="DataTablePagingParameter.totalCount"
			:footer-props="DataTablePagingParameter.footerProps" @update:page="changePage"
			@update:items-per-page="changeItemPerPage">

			<template v-slot:top>
				<v-toolbar>

					<v-divider class="mx-4" inset vertical></v-divider>
					<v-spacer></v-spacer>

					<v-card-title>
						<v-btn icon dark @click="showCreateDialog" v-if="hasPermissionToCreate">
							<v-icon>add</v-icon>
						</v-btn>
					</v-card-title>
				</v-toolbar>
			</template>

			<template v-slot:item.action="{ item }">

				<div>

					<v-tooltip top>
						<template v-slot:activator="{ on, attrs }" v-if="hasPermissionToCreate">
							<v-icon v-bind="attrs" v-on="on" small @click.stop="showEditDialog(item)" class="mr-2"
								:title="$t('Button.__edit')">
								edit
							</v-icon>
						</template>
						<span>{{$t("Button.__edit")}}</span>
					</v-tooltip>

				</div>

			</template>

			<template v-slot:item.parentMerchantName="{ item }">
				<span :title="item.parentMerchantID">
					{{item.parentMerchantName}}
				</span>
			</template>

			<template v-slot:item.statusText="{ item }">
				<span :class="item.status == 1 ?  '' : 'minus-red'">
					{{ item.statusText }}
				</span>
			</template>

			<template v-slot:item.updateBy="{ item }">
				{{ item.updateBy }} <span v-if="item.createBy != item.updateBy">({{item.createBy}})</span>
			</template>

			<template v-slot:item.createTime="{ item }">
				{{ item.createTime | timeWithAddTimezone(userProfile.timeZone) }}
			</template>

			<template v-slot:item.updateTime="{ item }">
				{{ item.updateTime | timeWithAddTimezone(userProfile.timeZone) }}
			</template>

			<template v-slot:item.beginTime="{ item }">
				{{ item.beginTime | timeWithAddTimezone(userProfile.timeZone) }} -
				{{ item.endTime | timeWithAddTimezone(userProfile.timeZone) }}
			</template>

			<template v-slot:item.endTime="{ item }">
				{{ item.endTime | timeWithAddTimezone(userProfile.timeZone) }}
			</template>

			<template
				v-slot:item.lastLogonTime="{ item }">{{ item.lastLogonTime | timeWithAddTimezone(userProfile.timeZone) }}
			</template>

		</v-data-table>

		<!-- 新增/編輯 -->
		<v-dialog v-model="dialogVisible.isShowCreate" max-width="650px" persistent scrollable>

			<v-card style="border:solid 1px #444;">

				<v-card-title class="grey darken-2">

					<div v-if="isEditMode(createForm)">{{$t("Button.__edit")}}
						<span class="sub-title-text">
							({{createForm.boAnnouncementID }})
						</span>
					</div>
					<div v-else>{{$t("Button.__create")}}
					</div>

					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>

				</v-card-title>

				<v-card-text>

					<div style="padding:10px;">

						<v-row>
							<v-text-field :label="$t('TableHeader.__title')" class="required"
								v-model="createForm.title">
							</v-text-field>
						</v-row>

						<v-row>

							<div class="w100p">
								<tiptap-vuetify v-model="createForm.content" :extensions="extensions"
									class="my-tiptap" />

							</div>

							<!-- <v-textarea :label="$t('TableHeader.__description')" class="required"
								v-model="createForm.content">
							</v-textarea> -->
						</v-row>

						<v-row style="position:relative">
							<v-text-field :label="$t('TableHeader.__imageUrl')" class=""
								v-model="createForm.imageUrl">
							</v-text-field>

							<v-btn color="blue lighten-3" text class="btn search ml-3 mt-3"
								@click.stop="showImageUpload">
								<v-icon left>file_upload</v-icon>
								{{$t('Button.__upload')}}
							</v-btn>&#9;

							<a :href="createForm.imageUrl" target="_blank"
								v-if="createForm && createForm.imageUrl && createForm.imageUrl.length > 0">
								<img :src="createForm.imageUrl" style="width: 40px;height: 40px;margin-top: 10px;">
							</a>

						</v-row>

						<!-- 
						<v-row>
							<v-autocomplete :items="dropdownItemsAnnouncementTypes"
								:disabled='dropdownItemsAnnouncementTypes.length <= 1' :label="'類型'" chips
								v-model="createForm.type">
							</v-autocomplete>
						</v-row> -->

						<v-row>

							<v-autocomplete :items="dropdownItemsAnnouncementHintTypes"
								:label="$t('TableHeader.__type')" chips v-model="createForm.hintType">
							</v-autocomplete>
						</v-row>

						<v-row>

							<v-autocomplete v-if="isEditMode(createForm)" :items="dropdwonStatus"
								:label="$t('TableHeader.__status')" chips v-model="createForm.status">
							</v-autocomplete>
						</v-row>

						<v-row v-if="hasSuperPermission && !isEditMode(createForm)" class="mt-2">
							<div class="w100">
								<v-text-field :label="$t('TableHeader.__createBy')+'(修改發文者名稱)'" class=""
									v-model="createForm.replaceCreateBy">
								</v-text-field>
							</div>
						</v-row>

						<v-row>
							<div>

								<div class="common-info-label" style="color:#CCC;">
									{{$t('TableHeader.__beginTime')}}/ {{$t('TableHeader.__endTime')}}
								</div>
								<div class="common-info-value">

									<date-picker v-model="datetimeRangePicker.searchStartTimeRange" type="datetime"
										value-type="timestamp" range :placeholder="$t('TableHeader.__time')">
									</date-picker>

								</div>
							</div>
						</v-row>

						<v-row class="mt-4">
							<v-autocomplete :items="dropdownItemsAssignType"
								:label="$t('TableHeader.__sendTarget')" chips
								v-model="createForm.assignParticipate.assignType">
							</v-autocomplete>
						</v-row>

						<v-row v-if="createForm.assignParticipate.assignType === 11">
							<v-autocomplete :items="dropdownItemsOrganization"
								:label="$t('TableHeader.__organization')" multiple deletable-chips chips
								v-model="createForm.assignParticipate.participaterIDs">
							</v-autocomplete>
						</v-row>

						<v-row v-if="createForm.assignParticipate.assignType === 19">
							<v-autocomplete :items="dropdownItemsHierarchyMerchants"
								:label="$t('TableHeader.__merchant') + ' (不含下级)'" deletable-chips chips multiple
								v-model="createForm.assignParticipate.participaterIDs">
							</v-autocomplete>
						</v-row>

					</div>

				</v-card-text>

				<v-card-actions>
					<v-spacer></v-spacer>
					<v-btn text @click="closeAllModal" class="btn">
						<v-icon left>mdi-cancel</v-icon>
						{{ $t('Button.__cancel') }}
					</v-btn>
					<v-btn color="yellow darken-1" text @click="saveCreateOrUpdate" class="btn">
						<v-icon left>mdi-check-circle</v-icon>
						{{ $t('Button.__submit') }}
					</v-btn>
				</v-card-actions>

			</v-card>

		</v-dialog>

		<ImageUploadDialog ref="refImageUploadDialog"
			@parentSetImageUrlAndClose="setImageUrlAfterUpload" />

	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import * as Model from '@/models/interfaces/operator-management/BOAnnouncement';
import * as UserModel from '@/models/interfaces/operator-management/users';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import * as EventModel from '@/models/interfaces/system-setting/events';
import * as MerchantCreditModel from '@/models/interfaces/agent/merchantCredit';
import ImageUploadDialog from '@/components/system-setting/ImageUploadDialog.vue';

import EventsApi from '@/api/system-setting/events';
import MerchantCreditApi from '@/api/agent/merchantCredit';
import MerchantApi from '@/api/system-setting/merchants';
import BOAnnouncementApi from '@/api/operator-management/BOAnnouncement';
import DropdownApi from '@/api/commons/dropdown';
import DatePicker from 'vue2-datepicker';

import { Locale, Currency, Status, GameVendorType } from '@/models/status/common';

// @ts-ignore
import { TiptapVuetify, Heading, Bold, Italic, Strike, Underline, Code, BulletList, OrderedList, ListItem, Image, Link, Blockquote, HorizontalRule } from 'tiptap-vuetify';

import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { commonMethodMixin } from '@/utilities/common-methods';
import NotificationWarning from '@/components/notification/NotificationWarning.vue';
import { pageData, time } from '@/utilities/api-handler.lib';
import { datetimeMixin, startEndDate, timeWithTimezone, daysAgo, weeksAgo, monthsAgo, formatMixin } from '@/utilities/datetime-format';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import { State, Action, Getter, namespace } from 'vuex-class';
const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';
import { uuid } from 'vue-uuid';

// @ts-ignore
@Component({ components: { NotificationWarning, DatePicker, ImageUploadDialog, TiptapVuetify }, mixins: [commonDisplayFiltersMixin, commonMethodMixin, datetimeMixin] })
export default class BOAnnouncementManagement extends Vue {
	get hasPermissionToCreate() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.Other_BOAnnouncement_CreateUpdate) >= 0;
	}

	get hasSuperPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSetting_SystemConfigSetting) >= 0;
	}

	get headers() {
		const data = [
			{ text: this.$t('TableHeader.__action'), value: 'action', sortable: false, align: 'center', width: '8%' },
			{ text: this.$t('TableHeader.__ID'), value: 'boAnnouncementID', sortable: false, align: 'start' },
			{ text: this.$t('TableHeader.__title'), value: 'title', sortable: false },
			// { text: this.$t('TableHeader.__type'), value: 'typeText', sortable: false },
			{ text: this.$t('TableHeader.__type'), value: 'hintTypeText', sortable: false },
			{ text: this.$t('TableHeader.__sendTarget'), value: 'assignTypeText', sortable: false },
			{ text: this.$t('TableHeader.__status'), value: 'statusText', sortable: false },
			{ text: this.$t('TableHeader.__beginTime') + '/' + this.$t('TableHeader.__endTime'), value: 'beginTime', sortable: false },
			{ text: this.$t('TableHeader.__updateTime'), value: 'updateTime', sortable: false },
			{ text: this.$t('TableHeader.__updateBy'), value: 'updateBy', sortable: false },
		];

		return data;
	}

	get dropdwonStatus() {
		const data = [
			{ text: this.$t('Status.Enable'), value: 1 },
			{ text: this.$t('Status.Disable'), value: 0 },
		];

		return data;
	}

	extensions: any = [
		Bold,
		Underline,
		Strike,
		Italic,
		ListItem,
		BulletList,
		OrderedList,
		Image,
		HorizontalRule,
		[
			Heading,
			{
				options: {
					levels: [1, 2, 3, 4, 5],
				},
			},
		],
	];

	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];

	dialogVisible: DropdownModel.DialogVisible = new DropdownModel.DialogVisible();

	datetimeRangePicker: DropdownModel.DatetimeRangePicker = new DropdownModel.DatetimeRangePicker();
	isShowPreview: boolean = false;
	isRenderComponent: boolean = true;
	isOrgDropdownInvisible: boolean = false;
	isLoad: boolean = true;

	DataTablePagingParameter: DropdownModel.DataTablePagingParameter = new DropdownModel.DataTablePagingParameter();
	searchList: Model.IBOAnnouncement[] = [];
	searchFilter: Model.IBOAnnouncementFilter = {} as Model.IBOAnnouncementFilter;
	createForm: Model.IBOAnnouncementRequest = {
		assignParticipate: {} as Model.IBOAnnouncementAssignTypeRequest,
	} as Model.IBOAnnouncementRequest;

	panel: number[] = [0];

	dropdownItemsHierarchyMerchants: DropdownModel.IDropdownItems[] = [];
	dropdownItemsMerchants: DropdownModel.IDropdownItems[] = [];
	dropdownItemsOrganization: DropdownModel.IDropdownItems[] = [];
	dropdownItemsRoles: DropdownModel.IDropdownItems[] = [];
	dropdownItemsLayer2Agents: DropdownModel.IDropdownItems[] = [];
	dropdownItemsAnnouncementTypes: DropdownModel.IDropdownItems[] = [];
	dropdownItemsAnnouncementHintTypes: DropdownModel.IDropdownItems[] = [];
	dropdownItemsAssignType: DropdownModel.IDropdownItems[] = [];

	isEditMode(item: Model.IBOAnnouncementRequest) {
		return item.boAnnouncementID > 0;
	}

	reloadDiv() {
		this.isLoad = false;
		this.isLoad = true;
	}

	changePage(index: number = 1) {
		this.DataTablePagingParameter.pageIndex = index;
		this.refresh();
	}

	changeItemPerPage(val: number = 10) {
		this.DataTablePagingParameter.itemsPerPage = val;
		this.refresh();
	}

	search() {
		this.DataTablePagingParameter.pageIndex = 1;
		this.refresh();
	}

	refresh() {
		this.registerReloadComponent();
		this.searchFilter.pageIndex = this.DataTablePagingParameter.pageIndex;
		this.searchFilter.pageSize = this.DataTablePagingParameter.itemsPerPage;

		EventBus.$emit('show_mask', null);

		BOAnnouncementApi.getList(this.searchFilter)
			.then((res) => {
				this.searchList = res.data.items;
				this.DataTablePagingParameter.totalCount = res.data.totalCount;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	showCreateDialog() {
		this.dialogVisible.isShowCreate = true;

		let defaultUserName = this.userProfile.userAccount;

		if (this.hasSuperPermission) {
			defaultUserName = 'system';
		}

		this.createForm = {
			replaceCreateBy: defaultUserName,
			assignParticipate: {} as Model.IBOAnnouncementAssignTypeRequest,
		} as Model.IBOAnnouncementRequest;

		this.datetimeRangePicker.searchStartTimeRange[0] = daysAgo(0);
		this.datetimeRangePicker.searchStartTimeRange[1] = daysAgo(30);

		if (this.dropdownItemsOrganization.length === 1) {
			this.isOrgDropdownInvisible = true;
		}
	}

	showEditDialog(item: Model.IBOAnnouncement) {
		EventBus.$emit('show_mask', null);
		BOAnnouncementApi.getById(item.boAnnouncementID)
			.then((res) => {
				this.createForm.boAnnouncementID = res.data.boAnnouncementID;
				this.createForm.title = res.data.title;
				this.createForm.content = res.data.content;
				this.createForm.imageUrl = res.data.imageUrl;
				this.createForm.beginTime = res.data.beginTime;
				this.createForm.endTime = res.data.endTime;
				this.datetimeRangePicker.searchStartTimeRange[0] = new Date(res.data.beginTime).getTime();
				this.datetimeRangePicker.searchStartTimeRange[1] = new Date(res.data.endTime).getTime();

				this.createForm.type = res.data.type;
				this.createForm.hintType = res.data.hintType;
				this.createForm.status = res.data.status;
				this.createForm.assignParticipate = res.data.assignParticipate;

				this.dialogVisible.isShowCreate = true;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	saveCreateOrUpdate() {
		const vm: any = this;
		EventBus.$emit('show_mask', null);

		this.createForm.beginTime = this.datetimeRangePicker.searchStartTimeRange[0] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[0]) : null;
		this.createForm.endTime = this.datetimeRangePicker.searchStartTimeRange[1] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[1]) : null;

		if (this.createForm.boAnnouncementID > 0) {
			BOAnnouncementApi.update(this.createForm)
				.then((res) => {
					EventBus.$emit(`successfully-modified`);
					this.closeAllModal();
					this.refresh();
				})
				.finally(() => {
					EventBus.$emit('hide_mask', null);
				});
		} else {
			BOAnnouncementApi.create(this.createForm)
				.then((res) => {
					EventBus.$emit(`successfully-modified`);
					this.closeAllModal();
					this.refresh();
				})
				.finally(() => {
					EventBus.$emit('hide_mask', null);
				});
		}
	}

	showImageUpload() {
		const targetRef: any = this.$refs.refImageUploadDialog;
		targetRef.showDialogAndEmptyValue(true);
	}

	setImageUrlAfterUpload(imageUrl: string) {
		this.createForm.imageUrl = imageUrl;

		// 刷新頁面
		this.dialogVisible.isShowCreate = false;
		this.dialogVisible.isShowCreate = true;
	}

	geti18nTextAsString(key: string) {
		return this.$t(key).toString();
	}

	closeAllModal() {
		this.dialogVisible.resetAllValueToFalse();
	}

	clear() {
		this.searchFilter = {} as UserModel.IUserFilter;
	}

	copySuccess() {
		EventBus.$emit(`successfully-copy`);
	}

	registerReloadComponent() {
		EventBus.$on(`reload-${this.$router.currentRoute.path}`, (info: any) => {
			this.isRenderComponent = false;
			this.$nextTick(() => {
				this.isRenderComponent = true;
			});
		});
	}

	initialFormInput() {
		this.createForm.assignParticipate = {} as Model.IBOAnnouncementAssignTypeRequest;
	}

	mounted() {
		this.initialFormInput();
		this.refresh();

		DropdownApi.getMerchants().then((res) => {
			this.dropdownItemsMerchants = res;
		});

		DropdownApi.getOrganizations().then((res) => {
			this.dropdownItemsOrganization = res;
		});

		DropdownApi.getHierarchyMerchants().then((res) => {
			this.dropdownItemsHierarchyMerchants = res;
		});

		DropdownApi.getRoles().then((res) => {
			this.dropdownItemsRoles = res;
		});

		DropdownApi.getBOAnnouncementType().then((res) => {
			this.dropdownItemsAnnouncementTypes = res;
		});

		DropdownApi.getBOAnnouncementHintType().then((res) => {
			this.dropdownItemsAnnouncementHintTypes = res;
		});

		DropdownApi.getAdminSysAssignTypes().then((res) => {
			this.dropdownItemsAssignType = res;
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