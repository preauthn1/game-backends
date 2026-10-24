<template>
	<v-app v-if="renderComponent" class="app">
		<v-expansion-panels v-model="panel" multiple>
			<v-expansion-panel>
				<v-expansion-panel-content>
					<v-flex>
						<v-row class="search-form">
							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-text-field v-model="searchFilter.keyword" clearable :label="$t('TableHeader.__keywordSearch')">> </v-text-field>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-text-field v-model="searchFilter.IPAddressKeyword" clearable :label="$t('Button.__search') + '(IPAddress)'"> </v-text-field>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete v-model="searchFilter.assignMerchantID" clearable :items="dropdownItemsMerchants" :label="$t('TableHeader.__assignMerchantTotalCount')"
										>>
									</v-autocomplete>
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

							<v-col cols="12" sm="3" md="3" v-if="hasInternalStaffPermission">
								<div class="minus-mt-1">
									<v-autocomplete v-model="searchFilter.imChannelMappingID" clearable :items="dropdownItemsIMChannels" :label="$t('TableHeader.__imChannelName') + ' (TG/Skype)'"
										>>
									</v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3" v-if="hasSuperPermission">
								<div class="minus-mt-1">
									<v-autocomplete v-model="searchFilter.specialFilterType" clearable :items="dropdownItemsSpecialTypes" :label="$t('TableHeader.__specialCategory')"
										>>
									</v-autocomplete>
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
						<div class="f14 yellow-text">
							<v-icon small color="yellow">info</v-icon>
							<span class="ml-">
								{{ $t('Message.__devEnvDoNotNeedSetWhiteList') }}
							</span>

							<v-icon class="ml-3" small color="yellow">info</v-icon>
							<span class="ml-">
								{{ $t('Message.__ifYouCanNotSeeDataPleaseContactUs') }}
							</span>
						</div>
					</v-toolbar-title>
					<v-divider class="mx-4" inset vertical></v-divider>
					<v-spacer></v-spacer>
					<v-card-title>
						<v-btn icon dark v-if="hasPermissionAdvance" @click="showQuickCreate" style="margin-left: 15px">
							<v-icon>add</v-icon>
						</v-btn>
					</v-card-title>
				</v-toolbar>
			</template>

			<template v-slot:item.action="{ item }">
				<v-icon small @click="showEditDialog(item)" class="mr-1" v-if="hasPermissionAdvance"> edit </v-icon>

				<v-icon small @click="showEditSingleIPAddressDialog(item)" class="mr-1" v-if="hasPermissionAppendNewIP"> mdi-credit-card-plus </v-icon>

				<v-icon small @click="confirmDeleteHint(item.id)" class="mr-1" v-if="hasPermissionToDelete"> delete </v-icon>
			</template>

			<template v-slot:item.status="{ item }">
				<span :class="item.status == 0 ? 'minus-red' : item.isAlreadyEnd ? 'grey-text' : ''">
					{{ item.statusText }}
				</span>
			</template>

			<template v-slot:item.ipAddressForDisplay="{ item }">
				<span class="maxWidthBeforeClick maxWidth500" @click="expandMaxWidth($event)">
					{{ item.ipAddressForDisplay }}
				</span>
			</template>

			<template v-slot:item.availableMerchantName="{ item }">
				<div style="max-width: 200px; overflow: hidden; white-space: nowrap; text-overflow: ellipsis">
					{{ item.availableMerchantName }}
				</div>
			</template>

			<template v-slot:item.assignMerchantCount="{ item }">
				<div :class="item.assignMerchantCount > 0 ? 'link-text' : 'link-text-grey'" v-if="hasPermissionAdvance" @click="showEditAssignMerchant(item)">
					{{ item.assignMerchantCount }}
				</div>
			</template>

			<template v-slot:item.imChannels="{ item }">
				<v-chip v-for="(text, index) in item.imChannels" small class="blue-grey darken-1 mr-1" :key="index">
					{{ text }}
				</v-chip>
			</template>

			<template v-slot:item.displayName="{ item }">
				<div>
					{{ item.displayName }}
				</div>
				<div class="sub-title-text" v-if="hasSuperPermission">
					{{ item.memo }}
				</div>
			</template>

			<template v-slot:item.createTime="{ item }">
				{{ item.createTime | timeWithAddTimezone(userProfile.timeZone) }}
			</template>

			<template v-slot:item.updateTime="{ item }">
				<div>
					{{ item.updateTime | timeWithAddTimezone(userProfile.timeZone) }}
				</div>

				<div class="sub-title-text" :title="'CreateTime'">
					{{ item.createTime | timeWithAddTimezone(userProfile.timeZone) }}
				</div>
			</template>
		</v-data-table>

		<!-- 新增/編輯 -->
		<v-dialog v-model="dialogVisible.isShowCreateOrEdit" max-width="600px" persistent>
			<v-card>
				<v-card-title class="grey darken-2">
					<span v-if="!isEdit()">快速新增</span>
					<span v-if="isEdit()">編輯</span>
					<span v-if="isEdit()" class="f12 ml-1"> ({{ editFrom.id }})) </span>

					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>
				<v-card-text style="padding-bottom: 100px">
					<v-flex>
						<v-row>
							<v-col cols="12" sm="12" md="12" style="margin-top: 10px">
								<v-text-field class="" v-model="editFrom.displayName" :label="$t('TableHeader.__displayName')" autofocus required clearable> </v-text-field>
							</v-col>
						</v-row>

						<v-row>
							<v-col cols="12" sm="12" md="12">
								<v-text-field class="" v-model="editFrom.memo" :label="$t('TableHeader.__memo')" required clearable> </v-text-field>
							</v-col>
						</v-row>

						<v-row>
							<v-col cols="12" sm="12" md="12">
								<div class="">
									<div>{{ $t('TableHeader.__iPAddress') }} ({{ $t('Message.__OneIpPerLine') }}) {{ extraMsg() }}</div>
									<textarea class="" v-model="editFrom.ipAddressWithLinebreak" placeholder="範例: 0.0.0.0"> </textarea>
								</div>
							</v-col>
						</v-row>
					</v-flex>
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

		<!-- 單筆IP插入 -->
		<v-dialog v-model="dialogVisible.isShowEditBriefInfo" max-width="600px" persistent>
			<v-card>
				<v-card-title class="grey darken-2">
					<span>
						{{ $t('Button.__appendNewIP') }}
					</span>

					<span class="f12 grey-text ml-2">({{ singleIPAddressFrom.id }}))</span>

					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>
				<v-card-text style="padding-bottom: 100px">
					<v-flex>
						<v-row>
							<v-col cols="12" sm="12" md="12" style="margin-top: 10px">
								<v-text-field class="" v-model="singleIPAddressFrom.displayName" disabled :label="$t('TableHeader.__displayName')"> </v-text-field>
							</v-col>
						</v-row>

						<v-row>
							<v-col cols="12" sm="12" md="12">
								<div class="textarea-bg">
									<v-textarea class="" v-model="singleIPAddressFrom.ipAddress" :label="$t('Message.__pleaseInputAgentWhiteListIP')" autofocus required clearable> </v-textarea>
								</div>

								<div style="width: 100%" class="text-r">
									<v-btn text @click="saveAppendSingleIP()" class="btn" color="yellow">
										<v-icon left>add</v-icon>
										{{ $t('Button.__save') }}
									</v-btn>
								</div>
							</v-col>
						</v-row>

						<v-row>
							<v-col cols="12" sm="12" md="12">
								<div class="">
									<div>{{ $t('TableHeader.__iPAddress') }} (Exist IPAddress)</div>
									<textarea class="" v-model="singleIPAddressFrom.ipAddressWithLinebreak" disabled> </textarea>
								</div>
							</v-col>
						</v-row>
					</v-flex>
				</v-card-text>
			</v-card>
		</v-dialog>

		<!-- 編輯商戶分配 -->
		<v-dialog v-model="dialogVisible.isShowEditOther" max-width="700px" persistent>
			<v-card>
				<v-card-title class="grey darken-2">
					<span>編輯商戶分配</span> <span class="ml-1 f12">({{ editFormOfAssignMerchant.id }})</span>

					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>
				<v-card-text style="padding-bottom: 100px">
					<v-flex>
						<v-row>
							<v-col cols="12" sm="12" md="12" style="margin-top: 10px">
								<v-text-field class="" v-model="editFormOfAssignMerchant.displayName" :label="$t('TableHeader.__displayName')" required disabled> </v-text-field>
							</v-col>
						</v-row>

						<v-row>
							<v-col cols="12" sm="12" md="12">
								<v-text-field class="" v-model="editFormOfAssignMerchant.ipAddressForDisplay" :label="$t('TableHeader.__iPAddress')" required disabled> </v-text-field>
							</v-col>
						</v-row>

						<v-row>
							<v-col cols="12" sm="12" md="12" v-if="isRenderExtraComponent">
								<v-autocomplete
									class=""
									v-model="editFormOfAssignMerchant.merchantIDs"
									chips
									deletable-chips
									multiple
									autofocus
									:items="dropdownItemsMerchants"
									:label="$t('TableHeader.__assignMerchantTotalCount')"
									clearable
								>
								</v-autocomplete>
							</v-col>

							<v-col cols="12" sm="12" md="12">
								<div class="text-r">
									<v-btn color="brown lighten-3" text @click="showParseTextToListIntDialog()" class="btn mr-2">
										<v-icon left>edit</v-icon>
										貼上多筆商戶IDs
									</v-btn>

									<v-btn color="yellow darken-1" text @click="saveEditAssignMerchant()" class="btn">
										<v-icon left>mdi-check-circle</v-icon>
										加入勾選商戶
									</v-btn>
								</div>
							</v-col>
						</v-row>

						<v-row class="mt-4">
							<v-col cols="12" sm="12" md="12">
								<div style="padding: 5px 10px; height: 300px; overflow-y: auto" v-if="editFormOfAssignMerchant.merchantNameMapping">
									<h3 class="mb-4">
										<v-icon>list</v-icon>
										已使用此配置的商戶
									</h3>
									<div v-for="(merchantID, index) in editFormOfAssignMerchant.assignMerchantIDs" class="assignMerchantRow" :key="index">
										<div class="merchantNameMaxWidth">
											{{ getMerchantMappingName(merchantID) }}
										</div>
										<div style="display: inline-block; vertical-align: top">
											<v-icon color="red" @click="deleteAssignMerchant(merchantID)">close</v-icon>
										</div>
									</div>
								</div>
							</v-col>
						</v-row>
					</v-flex>
				</v-card-text>
				<v-card-actions>
					<v-spacer></v-spacer>
				</v-card-actions>
			</v-card>
		</v-dialog>

		<TextareaParseToListIDPopup ref="refTextareaParseToListIDPopup" @parentSetResultIDs="setResultMerchantIDs"> </TextareaParseToListIDPopup>
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import * as MerchantModel from '@/models/interfaces/system-setting/merchants';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import * as EventModel from '@/models/interfaces/system-setting/events';

import MerchantsApi from '@/api/system-setting/merchants';
import DropdownApi from '@/api/commons/dropdown';
import DatePicker from 'vue2-datepicker';
import { Locale, Currency, Status, GameVendorType } from '@/models/status/common';

import TextareaParseToListIDPopup from '@/components/system-setting/TextareaParseToListIDPopup.vue';
import AnnouncementDetail from '@/components/system-setting/AnnouncementDetail.vue';
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
@Component({ components: { NotificationWarning, DatePicker, AnnouncementDetail, TextareaParseToListIDPopup }, mixins: [commonDisplayFiltersMixin, commonMethodMixin, datetimeMixin] })
export default class IPAddressTemplateList extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];

	dialogVisible: DropdownModel.DialogVisible = new DropdownModel.DialogVisible();

	renderComponent: boolean = true;
	isRenderExtraComponent: boolean = true;

	perPage: number = 100;

	DataTablePagingParameterSize500: DropdownModel.DataTablePagingParameterSize500 = new DropdownModel.DataTablePagingParameterSize500();
	DataTablePagingParameter: DropdownModel.DataTablePagingParameter = new DropdownModel.DataTablePagingParameter();
	datetimeRangePicker: DropdownModel.DatetimeRangePicker = new DropdownModel.DatetimeRangePicker();

	searchList: MerchantModel.IIPAddressTemplateResponse[] = [];
	searchFilter: MerchantModel.IIPAddressTemplateQueryRequest = {} as MerchantModel.IIPAddressTemplateQueryRequest;
	editFrom: MerchantModel.IIPAddressTemplateCreateRequest = {} as MerchantModel.IIPAddressTemplateCreateRequest;
	singleIPAddressFrom: MerchantModel.IIPAddressTemplateSingleAppendRequest = {} as MerchantModel.IIPAddressTemplateSingleAppendRequest;
	editFormOfAssignMerchant: MerchantModel.IIPAddressTemplateUpdateAssignMerchantRequest = {} as MerchantModel.IIPAddressTemplateUpdateAssignMerchantRequest;

	panel: number[] = [0];

	dropdownItemsStatus: DropdownModel.IDropdownItems[] = [];
	dropdownItemsMerchants: DropdownModel.IDropdownItems[] = [];
	dropdownItemsAnnouncementTypes: DropdownModel.IDropdownItems[] = [];
	dropdownItemsIMChannels: DropdownModel.IDropdownItems[] = [];
	dropdownItemsSpecialTypes: DropdownModel.IDropdownItems[] = [];

	get hasPermissionAdvance() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSetting_Merchant_IPAddressTemplate_AdvanceEdit) >= 0;
	}

	get hasPermissionAppendNewIP() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSetting_Merchant_IPAddressTemplate_AppendNewIP) >= 0;
	}

	get hasPermissionToDelete() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSetting_Merchant_IPAddressTemplate_Delete) >= 0;
	}

	get hasInternalStaffPermission() {
		return this.hasSuperPermission || this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.Special_PGS_CustomerService) >= 0;
	}

	get hasSuperPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSetting_SystemConfigSetting) >= 0;
	}

	get headers() {
		const data: any[] = [
			{ text: this.$t('TableHeader.__action'), value: 'action', sortable: false, align: 'center', width: '5%' },
			{ text: this.$t('TableHeader.__ID'), value: 'id', sortable: false, width: '5%' },
			{ text: this.$t('TableHeader.__displayName'), value: 'displayName', sortable: false, width: '15%', align: 'left' },
			{ text: this.$t('TableHeader.__IMChannel'), value: 'imChannels', sortable: false, width: '7%' },
		];

		if (this.hasPermissionAdvance) {
			data.push({ text: '預設IP頻道數', value: 'assignDefaultIPTemplateCount', sortable: false, width: '7%', align: 'center' });
			data.push({ text: this.$t('TableHeader.__assignMerchantTotalCount'), value: 'assignMerchantCount', sortable: false, width: '7%', align: 'center' });
		}

		data.push({ text: this.$t('TableHeader.__AssignIPAddressCount'), value: 'ipAddressCount', sortable: false, width: '7%', align: 'center' });
		data.push({ text: this.$t('TableHeader.__iPAddress'), value: 'ipAddressForDisplay', sortable: false, width: '7%' });
		data.push({ text: this.$t('TableHeader.__lastUpdateTime'), value: 'updateTime', sortable: false });
		return data;
	}

	isEdit() {
		return this.editFrom.id > 0;
	}

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

	extraMsg() {
		if (this.hasSuperPermission) {
			return '結尾填寫* 可以like比對';
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
		EventBus.$emit('show_mask', null);
		MerchantsApi.getIPAddressTemplates(this.searchFilter)
			.then((res) => {
				this.searchList = res.data.items;
				this.DataTablePagingParameter.totalCount = res.data.totalCount;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	getMerchantMappingName(merchantID: number) {
		return this.editFormOfAssignMerchant.merchantNameMapping[merchantID.toString()];
	}

	showQuickCreate() {
		this.dialogVisible.isShowCreateOrEdit = true;
		this.editFrom = {} as MerchantModel.IIPAddressTemplateCreateRequest;
	}

	showEditSingleIPAddressDialog(item: MerchantModel.IIPAddressTemplateResponse) {
		this.dialogVisible.isShowEditBriefInfo = true;
		this.singleIPAddressFrom.id = item.id;
		this.singleIPAddressFrom.displayName = item.displayName;
		this.singleIPAddressFrom.ipAddress = '';
		this.singleIPAddressFrom.ipAddressWithLinebreak = item.ipAddressWithLinebreak;
		this.singleIPAddressFrom.ipAddressList = item.ipAddressList;
	}

	saveAppendSingleIP() {
		EventBus.$emit('show_mask', null);
		MerchantsApi.appendSingleIPAddressToSetting(this.singleIPAddressFrom)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.closeAllModal();
				this.search();
			})
			.finally(() => {
				setTimeout(() => {
					EventBus.$emit('hide_mask', null);
				}, 250);
			});
	}

	showEditDialog(item: MerchantModel.IIPAddressTemplateResponse) {
		this.dialogVisible.isShowCreateOrEdit = true;
		this.editFrom.id = item.id;
		this.editFrom.displayName = item.displayName;
		this.editFrom.memo = item.memo;
		this.editFrom.ipAddressWithLinebreak = item.ipAddressWithLinebreak;
		this.editFrom.ipAddressList = item.ipAddressList;
		this.editFrom.assignMerchantIDs = item.assignMerchantIDs;
	}

	saveCreateOrUpdate() {
		EventBus.$emit('show_mask', null);

		if (this.isEdit()) {
			MerchantsApi.updateIPAddressTemplates(this.editFrom)
				.then((res) => {
					EventBus.$emit(`successfully-modified`);
					this.closeAllModal();
					this.search();
				})
				.finally(() => {
					EventBus.$emit('hide_mask', null);
				});
		} else {
			MerchantsApi.createIPAddressTemplates(this.editFrom)
				.then((res) => {
					EventBus.$emit(`successfully-modified`);
					this.closeAllModal();
					this.search();
				})
				.finally(() => {
					EventBus.$emit('hide_mask', null);
				});
		}
	}

	async showEditAssignMerchant(item: MerchantModel.IIPAddressTemplateResponse) {
		await this.reloadSingleById(item.id);

		this.dialogVisible.isShowEditOther = true;
		this.editFormOfAssignMerchant.id = item.id;
		this.editFormOfAssignMerchant.displayName = item.displayName;
		this.editFormOfAssignMerchant.merchantID = 0;
		this.editFormOfAssignMerchant.ipAddressWithLinebreak = item.ipAddressWithLinebreak;
		this.editFormOfAssignMerchant.ipAddressForDisplay = item.ipAddressForDisplay;
		this.editFormOfAssignMerchant.merchantIDs = [];

		this.reloadDropdown();
	}

	saveEditAssignMerchant() {
		this.editFormOfAssignMerchant.isDelete = false;
		this.editFormOfAssignMerchant.merchantID = 0;

		EventBus.$emit('show_mask', null);
		MerchantsApi.updateIPAddressTemplatesAssignMerchant(this.editFormOfAssignMerchant)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.reloadSingleById(this.editFormOfAssignMerchant.id);
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	deleteAssignMerchant(merchantID: number) {
		const formDelete = {} as MerchantModel.IIPAddressTemplateUpdateAssignMerchantRequest;
		formDelete.id = this.editFormOfAssignMerchant.id;
		formDelete.merchantID = merchantID;
		formDelete.isDelete = true;

		EventBus.$emit('show_mask', null);
		MerchantsApi.updateIPAddressTemplatesAssignMerchant(formDelete)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.reloadSingleById(this.editFormOfAssignMerchant.id);
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	async reloadSingleById(id: number) {
		EventBus.$emit('show_mask', null);

		this.editFormOfAssignMerchant.assignMerchantIDs = [];
		MerchantsApi.getIPAddressTemplatesById(id)
			.then((res) => {
				this.editFormOfAssignMerchant.merchantID = null;
				this.editFormOfAssignMerchant.assignMerchantIDs = res.data.assignMerchantIDs;
				this.editFormOfAssignMerchant.merchantNameMapping = res.data.merchantNameMapping;

				// 刷新畫面
				this.dialogVisible.isShowEditOther = false;
				this.$nextTick(() => {
					this.dialogVisible.isShowEditOther = true;
				});
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

	showDeleteHint(item: MerchantModel.IIPAddressTemplateResponse) {
		const vm: any = this;
		this.dialogVisible.isShowDelete = true;
	}

	showParseTextToListIntDialog(IDs: number[]) {
		const targetRef: any = this.$refs.refTextareaParseToListIDPopup;
		targetRef.showParseTextareaPraseToIDs();
	}

	setResultMerchantIDs(IDs: number[]) {
		this.isRenderExtraComponent = false;
		this.editFormOfAssignMerchant.merchantIDs = IDs;

		console.log(IDs);
		console.log(this.editFormOfAssignMerchant.merchantIDs);

		setTimeout(() => {
			this.isRenderExtraComponent = true;
		}, 100);
	}

	confirmDeleteHint(targetID: number) {
		if (confirm('是否刪除此筆資料？《' + targetID.toString() + '》')) {
			EventBus.$emit('show_mask', null);
			MerchantsApi.deleteIPTemplate(targetID)
				.then((res) => {
					EventBus.$emit(`successfully-modified`);
					this.closeAllModal();
					this.search();
				})
				.finally(() => {
					setTimeout(() => {
						EventBus.$emit('hide_mask', null);
					}, 250);
				});
		}
	}

	closeAllModal() {
		this.dialogVisible.resetAllValueToFalse();
	}

	clear() {
		this.searchFilter = {} as MerchantModel.IIPAddressTemplateQueryRequest;
	}

	copySuccess() {
		EventBus.$emit(`successfully-copy`);
	}

	reloadDropdown() {
		DropdownApi.getMerchants().then((res) => {
			this.dropdownItemsMerchants = res;
		});
	}

	mounted() {
		this.refresh();

		DropdownApi.getNormalStatus().then((res) => {
			this.dropdownItemsStatus = res;
		});

		DropdownApi.getAnnouncementTypes().then((res) => {
			this.dropdownItemsAnnouncementTypes = res;
		});

		DropdownApi.getIMChannels().then((res) => {
			this.dropdownItemsIMChannels = res;
		});

		DropdownApi.getIPTemplateSpecialFilterType().then((res) => {
			this.dropdownItemsSpecialTypes = res;
		});

		this.reloadDropdown();
	}
}
</script>

<style scoped>
textarea {
	background: #333 !important;
	height: 200px;
	color: #fff;
	padding: 5px;
	width: 100%;
	outline: none;
	font-size: 16px;
}

textarea:disabled {
	color: #9e9026;
}

.col-md-12 {
	margin-top: -10px;
	margin-bottom: -10px;
}

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

.merchantNameMaxWidth {
	display: inline-block;
	width: 300px;
	overflow: hidden;
	text-overflow: ellipsis;
	white-space: nowrap;
}

.assignMerchantRow:hover {
	background: #333;
}
</style>
