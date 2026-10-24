<template>
	<v-app v-if="isRenderComponent" class="app">
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

							<!-- <v-col cols="12" sm="3" md="3">
								 <div class="item">

									<v-autocomplete clearable :items="dropdownItemsEventType"
										v-model="searchFilter.eventType" :label="$t('TableHeader.__type')">
									</v-autocomplete>

								</div> 
							</v-col> -->

							<v-col cols="12" sm="6" md="6">
								<div class="mt-2 search-row-padding">
									<v-btn color="yellow darken-1" text class="btn search mt-1" @click.stop="search">
										<v-icon left>search</v-icon>
										{{ $t('Button.__search') }} </v-btn
									>&#9;

									<v-btn color="pink lighten-3" text class="btn search mt-1" @click.stop="clear">
										<v-icon left>clear</v-icon>
										{{ $t('Button.__reset') }} </v-btn
									>&#9;

									<v-btn color="light-blue lighten-3" text class="btn search mt-1" v-if="hasPermissionToCreate" @click.stop="showCreateAccountDialog">
										<v-icon left>add</v-icon>
										{{ $t('Button.__create') }} ({{ $t('TableHeader.__BOAccount') }}) </v-btn
									>&#9;

									<v-btn color="green lighten-3" text class="btn search mt-1" v-if="hasPermissionToCopy" @click.stop="showCopyAccountDialog">
										<v-icon left>add</v-icon>
										{{ $t('Button.__copy') }} ({{ $t('TableHeader.__BOAccount') }}) </v-btn
									>&#9;

									<span v-if="hasSuperPermission" class="ml-2">
										<v-icon small>error</v-icon>
										<span style="font-size: 12px"> 管理員請使用 [管理員設置] > [帳號管理] </span>
									</span>
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
			class="mt-2"
			:page="DataTablePagingParameter.pageIndex"
			:items-per-page="DataTablePagingParameter.itemsPerPage"
			:server-items-length="DataTablePagingParameter.totalCount"
			:footer-props="DataTablePagingParameter.footerProps"
			@update:page="changePage"
			@update:items-per-page="changeItemPerPage"
		>
			<template v-slot:item.action="{ item }">
				<div v-if="!item.isInternalDevelopAccount">
					<v-tooltip top v-if="!hasCsLevelPasswordResetPermission">
						<template v-slot:activator="{ on, attrs }">
							<v-icon v-bind="attrs" v-on="on" small class="mr-2" @click.stop="showUpdateTargetUserPasswordDialog(item)"> lock </v-icon>
						</template>
						<span>{{ $t('Popup.__updatePassword') }}</span>
					</v-tooltip>

					<v-tooltip top v-if="hasCsLevelPasswordResetPermission">
						<template v-slot:activator="{ on, attrs }">
							<v-icon v-bind="attrs" v-on="on" small class="mr-2" @click.stop="showUpdateTargetUserPasswordDialogForCS(item)"> lock </v-icon>
						</template>
						<span>{{ $t('Popup.__updatePassword') }} (CS Level)</span>
					</v-tooltip>

					<v-tooltip top>
						<template v-slot:activator="{ on, attrs }">
							<v-icon
								v-bind="attrs"
								v-on="on"
								small
								@click.stop="showUpdateBriefInfoDialog(item)"
								v-show="hasPermissionToEditStatus && !item.isSelfAccount"
								class="mr-2"
								:title="$t('Button.__edit')"
							>
								edit
							</v-icon>
						</template>
						<span>{{ $t('Button.__edit') }}</span>
					</v-tooltip>
				</div>
			</template>

			<template v-slot:item.parentMerchantName="{ item }">
				<span :title="item.parentMerchantID">
					{{ item.parentMerchantName }}
				</span>
			</template>

			<template v-slot:item.referenceCreditMerchantName="{ item }">
				<div class="pointer" @click="copyMerchantIDAndText(item)">
					<span v-if="item.referenceCreditMerchant > 0">{{ item.referenceCreditMerchant }}｜</span>
					<span>{{ item.referenceCreditMerchantName }}</span>
				</div>
			</template>

			<template v-slot:item.statusText="{ item }">
				<span :class="item.status == 1 ? '' : 'minus-red'">
					{{ item.statusText }}
				</span>
			</template>

			<template v-slot:item.createTime="{ item }">
				{{ item.createTime | timeWithAddTimezone(userProfile.timeZone) }}
			</template>
			<template v-slot:item.lastLogonTime="{ item }">{{ item.lastLogonTime | timeWithAddTimezone(userProfile.timeZone) }} </template>
		</v-data-table>

		<!-- 新增後台帳號 -->
		<v-dialog v-model="dialogVisible.isShowCreate" max-width="400px" persistent>
			<v-card style="border: solid 1px #444">
				<v-card-title class="grey darken-2">
					{{ $t('Button.__create') }}

					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>

				<v-card-text>
					<div style="padding: 10px">
						<v-row>
							<v-autocomplete :items="dropdownItemsMerchants" :label="$t('TableHeader.__merchantOrAgent')" chips v-model="createUserForm.referenceCreditMerchant"> </v-autocomplete>
						</v-row>

						<v-row>
							<v-autocomplete
								:items="dropdownItemsOrganization"
								chips
								v-show="!isOrgDropdownInvisible"
								@change="organizationDropdownChanged()"
								:label="$t('TableHeader.__organization')"
								v-model="createUserForm.organizationID"
							>
							</v-autocomplete>
						</v-row>

						<v-row>
							<v-text-field :label="$t('TableHeader.__BOAccount')" v-model="accountFormInput" clearable @keydown="reloadDiv()" @change="reloadDiv()" maxlength="30"> </v-text-field>

							<div style="margin-top: -13px; margin-bottom: 5px" v-if="isLoad">
								<v-icon small color="grey ">error</v-icon> {{ $t('Message.__adminSysLoginHint1') }}
								<span style="color: #ff4e4e; font-weight: 700">{{ accountPrefix }}-{{ accountFormInput }} </span> {{ $t('Message.__adminSysLoginHint2') }}
							</div>
							<div style="clear: both"></div>
						</v-row>

						<v-row>
							<v-text-field :label="$t('TableHeader.__password')" v-model="createUserForm.password" clearable type="password" maxlength="30"> </v-text-field>
						</v-row>

						<v-row>
							<v-text-field :label="$t('TableHeader.__password2')" clearable v-model="createUserForm.confirmPassword" type="password" maxlength="30"> </v-text-field>
						</v-row>

						<v-row>
							<v-autocomplete :items="dropdownItemsRoles" :label="$t('TableHeader.__role')" chips v-model="createUserForm.roleID"> </v-autocomplete>
						</v-row>
					</div>
				</v-card-text>

				<v-card-actions>
					<v-spacer></v-spacer>
					<v-btn text @click="closeAllModal" class="btn">
						<v-icon left>mdi-cancel</v-icon>
						{{ $t('Button.__cancel') }}
					</v-btn>
					<v-btn color="yellow darken-1" text @click="saveCreateAccount" class="btn">
						<v-icon left>mdi-check-circle</v-icon>
						{{ $t('Button.__submit') }}
					</v-btn>
				</v-card-actions>
			</v-card>
		</v-dialog>

		<!-- 複製帳號 -->
		<v-dialog v-model="dialogVisible.isShowCopy" max-width="450px" persistent>
			<v-card style="border: solid 1px #444">
				<v-card-title class="grey darken-2">
					{{ $t('Button.__copy') }} ({{ $t('TableHeader.__BOAccount') }})

					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>

				<v-card-text>
					<div style="padding: 10px">
						<v-row>
							<v-autocomplete :items="dropdownItemsUsers" :label="$t('TableHeader.__account')" chips @change="whenSourceUserIDChanged()" v-model="copyUserForm.sourceUserID">
							</v-autocomplete>
						</v-row>

						<v-row>
							<v-text-field :label="$t('TableHeader.__BOAccount') + '(' + $t('Message.__notIncludePrefix') + ')'" v-model="copyUserForm.userName" clearable maxlength="30">
							</v-text-field>

							<div style="clear: both"></div>

							<div v-if="isRenderExtraInfo" style="width: 100%" class="mt-4">
								<v-icon small color="grey ">error</v-icon> {{ $t('Message.__adminSysLoginHint1') }}
								<span style="color: #ff4e4e; font-weight: 700">{{ copyUserForm.organizationCode }}-{{ copyUserForm.userName }} </span> {{ $t('Message.__adminSysLoginHint2') }}
							</div>

							<div v-if="isRenderExtraInfo" style="width: 100%">
								<v-icon small color="grey ">error</v-icon>
								{{ $t('TableHeader.__defaultPassword') }}：<span style="color: #ff4e4e; font-weight: 700">{{ getSystemDefaultPassword() }}</span>
							</div>
						</v-row>
					</div>
				</v-card-text>

				<v-card-actions>
					<v-spacer></v-spacer>
					<v-btn text @click="closeAllModal" class="btn">
						<v-icon left>mdi-cancel</v-icon>
						{{ $t('Button.__cancel') }}
					</v-btn>
					<v-btn color="yellow darken-1" text @click="saveCopyAccount" class="btn">
						<v-icon left>mdi-check-circle</v-icon>
						{{ $t('Button.__submit') }}
					</v-btn>
				</v-card-actions>
			</v-card>
		</v-dialog>

		<!-- 修改基本資料 -->
		<v-dialog v-model="dialogVisible.isShowEditBriefInfo" max-width="400px">
			<v-card style="border: solid 1px #444">
				<v-card-title class="grey darken-2">
					{{ $t('Popup.__update') }}

					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>

				<v-card-text>
					<div style="padding: 10px">
						<v-row>
							<v-text-field disabled="disabled" :label="$t('TableHeader.__userName')" v-model="updateBriefInfoForm.targetUserName"> </v-text-field>
						</v-row>

						<v-row>
							<v-text-field :label="$t('TableHeader.__memo')" v-model="updateBriefInfoForm.memo"> </v-text-field>
						</v-row>

						<v-row>
							<v-autocomplete :items="dropdwonStatus" :label="$t('TableHeader.__status')" chips v-model="updateBriefInfoForm.status"> </v-autocomplete>

							<div style="font-size: 12px; color: #ff9b9b">
								<v-icon color="red lighten-2" small> error </v-icon>

								{{ $t('Message.__disableBOAccountHint') }}
							</div>
						</v-row>
					</div>
				</v-card-text>

				<v-card-actions>
					<v-spacer></v-spacer>
					<v-btn text @click="closeAllModal" class="btn">
						<v-icon left>mdi-cancel</v-icon>
						{{ $t('Button.__cancel') }}
					</v-btn>
					<v-btn color="yellow darken-1" text @click="saveUpdateBriefInfoDialog" class="btn">
						<v-icon left>mdi-check-circle</v-icon>
						{{ $t('Button.__submit') }}
					</v-btn>
				</v-card-actions>
			</v-card>
		</v-dialog>

		<!-- 修改密碼 -->
		<v-dialog v-model="dialogVisible.isShowEditOther" max-width="500px" persistent>
			<v-card style="border: solid 1px #444">
				<v-card-title class="grey darken-2">
					{{ $t('Popup.__updatePassword') }}

					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>

				<v-card-text>
					<div style="padding: 10px">
						<v-row>
							<v-text-field disabled="disabled" :label="$t('TableHeader.__userName')" v-model="updatePasswordForm.targetUserName"> </v-text-field>
						</v-row>

						<v-row>
							<v-text-field disabled="disabled" :label="$t('TableHeader.__imChannelName')" v-model="updatePasswordForm.imChannelName"> </v-text-field>
						</v-row>

						<v-row>
							<v-text-field :label="$t('TableHeader.__password') + ' (New)'" :type="'password'" v-model="updatePasswordForm.newPassword"> </v-text-field>
						</v-row>

						<v-row>
							<v-text-field :label="$t('TableHeader.__password2')" :type="'password'" v-model="updatePasswordForm.confirmPassword"> </v-text-field>
						</v-row>
					</div>
				</v-card-text>

				<v-card-actions style="display: block" class="w100p text-r">
					<v-spacer></v-spacer>

					<v-btn text @click="resetPasswordToDefault()" class="btn mb-1" color="brown lighten-2">
						<v-icon color="brown lighten-2" left>mdi-lock</v-icon>
						{{ $t('Button.__resetPasswordToDefault') }}
					</v-btn>

					<v-btn text @click="closeAllModal" class="btn mb-1">
						<v-icon left>mdi-cancel</v-icon>
						{{ $t('Button.__cancel') }}
					</v-btn>

					<v-btn color="yellow darken-1" text @click="saveUpdateTargetUserPassword" class="btn mb-1">
						<v-icon left>mdi-check-circle</v-icon>
						{{ $t('Button.__submit') }}
					</v-btn>
				</v-card-actions>
			</v-card>
		</v-dialog>

		<!-- 修改密碼-客服使用 -->
		<v-dialog v-model="dialogVisible.isShowEdit2" max-width="500px" persistent>
			<v-card style="border: solid 1px #444">
				<v-card-title class="grey darken-2">
					{{ $t('Popup.__updatePassword') }}

					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>

				<v-card-text>
					<div style="padding: 10px">
						<v-row>
							<v-text-field disabled="disabled" :label="$t('TableHeader.__userName')" v-model="updatePasswordFormForCS.targetUserName"> </v-text-field>
						</v-row>

						<v-row>
							<v-autocomplete
								:label="$t('TableHeader.__imChannelName') + '（客戶頻道名稱驗證）'"
								clearable
								:items="dropdownItemsIMChannels"
								v-model="updatePasswordFormForCS.validateIMChannel"
							>
							</v-autocomplete>
						</v-row>
					</div>
				</v-card-text>

				<v-card-actions style="display: block" class="w100p text-r">
					<v-spacer></v-spacer>

					<v-btn color="yellow darken-1" text @click="saveToDefaultPasswordForCS" class="btn mb-1">
						<v-icon left>mdi-check-circle</v-icon>
						{{ $t('Button.__resetPasswordToDefault') }}
					</v-btn>
				</v-card-actions>
			</v-card>
		</v-dialog>

		<v-dialog v-model="dialogVisible.isShowSuccess" max-width="600px">
			<v-card>
				<v-card-title class="grey darken-2">
					{{ $t('Message.__successfully') }} ({{ $t('TableHeader.__password') }})

					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>

				<v-card-text style="min-height: 250px">
					<div class="mt-4 mb-2">
						<v-icon color="grey" small>error</v-icon>
						{{ $t('Message.__youCanClickThisCopyText') }}.
					</div>

					<div
						class="pointer"
						v-clipboard:copy="passwordResetTextForCopy"
						:title="'Click to copy'"
						style="white-space: pre-line; max-width: 300px; background: #444; color: #d77529; border-radius: 5px; padding: 5px 10px; font-size: 15px"
						v-clipboard:success="copySuccess"
					>
						{{ passwordResetTextForCopy }}
					</div>
				</v-card-text>
			</v-card>
		</v-dialog>
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import * as Model from '@/models/interfaces/system-setting/merchants';
import * as UserModel from '@/models/interfaces/operator-management/users';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import * as EventModel from '@/models/interfaces/system-setting/events';
import * as MerchantCreditModel from '@/models/interfaces/agent/merchantCredit';

import EventsApi from '@/api/system-setting/events';
import MerchantCreditApi from '@/api/agent/merchantCredit';
import MerchantApi from '@/api/system-setting/merchants';
import UsersApi from '@/api/operator-management/users';
import DropdownApi from '@/api/commons/dropdown';
import DatePicker from 'vue2-datepicker';

import { Locale, Currency, Status, GameVendorType } from '@/models/status/common';

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
@Component({ components: { NotificationWarning, DatePicker }, mixins: [commonDisplayFiltersMixin, commonMethodMixin, datetimeMixin] })
export default class OrganizationAccountManagement extends Vue {
	get hasPermissionToCopy() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.Other_AccountManagement_Copy) >= 0;
	}

	get hasPermissionToCreate() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.Other_AccountManagement_Create) >= 0;
	}

	get hasPermissionToEditStatus() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.Other_AccountManagement_EditStatus) >= 0;
	}

	get hasSuperPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSetting_SystemConfigSetting) >= 0;
	}

	get hasDisableAgentPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.Agent_MerchantCredit_EditStatus) >= 0;
	}

	get hasCsLevelPasswordResetPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.Other_AccountManagement_ResetPWAndValidChannel) >= 0;
	}

	get headers() {
		const data: any[] = [
			{ text: this.$t('TableHeader.__action'), value: 'action', sortable: false, align: 'center', width: '8%' },
			{ text: this.$t('TableHeader.__account'), value: 'userName', sortable: false, align: 'start' },
		];

		if (this.hasSuperPermission) {
			data.push({ text: this.$t('TableHeader.__memo'), value: 'memo', sortable: false, align: 'start' });
		}

		data.push({ text: this.$t('TableHeader.__merchantOrAgent'), value: 'referenceCreditMerchantName', sortable: false });
		data.push({ text: this.$t('TableHeader.__roleName'), value: 'roleName_CN', sortable: false });
		data.push({ text: this.$t('TableHeader.__status'), value: 'statusText', sortable: false });
		data.push({ text: this.$t('TableHeader.__createTime2'), value: 'createTime', sortable: false });
		data.push({ text: this.$t('TableHeader.__lastLoginTime'), value: 'lastLogonTime', sortable: false });
		return data;
	}

	get dropdwonStatus() {
		const data = [
			{ text: this.$t('Status.Enable'), value: 1 },
			{ text: this.$t('Status.Disable'), value: 0 },
		];

		return data;
	}

	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];

	dialogVisible: DropdownModel.DialogVisible = new DropdownModel.DialogVisible();
	isRenderExtraInfo: boolean = false;
	isShowPreview: boolean = false;
	isRenderComponent: boolean = true;
	isOrgDropdownInvisible: boolean = false;
	isLoad: boolean = true;

	accountPrefix: string = '';
	accountFormInput: string = '';
	passwordResetTextForCopy: string = '';

	DataTablePagingParameter: DropdownModel.DataTablePagingParameter = new DropdownModel.DataTablePagingParameter();
	datetimeRangePicker: DropdownModel.DatetimeRangePicker = new DropdownModel.DatetimeRangePicker();
	searchList: UserModel.IUser[] = [];
	searchFilter: UserModel.IUserFilter = {} as UserModel.IUserFilter;

	createUserForm: UserModel.IUserRequest = {} as UserModel.IUserRequest;
	copyUserForm: UserModel.IUserCopyRequest = {} as UserModel.IUserCopyRequest;
	updatePasswordForm: UserModel.IUpdatePasswordRequest = {} as UserModel.IUpdatePasswordRequest;
	updatePasswordFormForCS: UserModel.IUpdatePasswordRequest = {} as UserModel.IUpdatePasswordRequest;
	updateBriefInfoForm: UserModel.IUpdateUserBriefInfoRequest = {} as UserModel.IUpdateUserBriefInfoRequest;

	panel: number[] = [0];

	dropdownItemsMerchants: DropdownModel.IDropdownItems[] = [];
	dropdownItemsOrganization: DropdownModel.IDropdownItems[] = [];
	dropdownItemsRoles: DropdownModel.IDropdownItems[] = [];
	dropdownItemsLayer2Agents: DropdownModel.IDropdownItems[] = [];
	dropdownItemsUsers: DropdownModel.IDropdownItems[] = [];
	dropdownItemsIMChannels: DropdownModel.IDropdownItems[] = [];

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

	whenSourceUserIDChanged() {
		const vm: any = this;
		this.isRenderExtraInfo = false;
		setTimeout(() => {
			if (vm.copyUserForm.sourceUserID > 0) {
				const matchItem = vm.dropdownItemsUsers.filter((item: any) => item.valueInt === vm.copyUserForm.sourceUserID)[0];
				vm.copyUserForm.organizationCode = matchItem.extraProperty.organizationCode;
			}

			this.isRenderExtraInfo = true;
		}, 250);
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

		UsersApi.getUsersWithPageSize(this.searchFilter)
			.then((res) => {
				this.searchList = res.data.items;
				this.DataTablePagingParameter.totalCount = res.data.totalCount;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	showCreateAccountDialog() {
		this.dialogVisible.isShowCreate = true;
		this.createUserForm.userName = '';
		this.createUserForm.password = '';
		this.accountPrefix = '';

		if (this.dropdownItemsOrganization.length === 1) {
			this.isOrgDropdownInvisible = true;
			this.createUserForm.organizationID = this.dropdownItemsOrganization[0].valueInt;
			this.organizationDropdownChanged();
		}
	}

	saveCreateAccount() {
		const vm: any = this;
		EventBus.$emit('show_mask', null);

		this.createUserForm.userName = this.accountPrefix + '-' + this.accountFormInput;
		this.createUserForm.displayName = this.accountFormInput;
		this.createUserForm.status = 1;
		this.createUserForm.timeZoneAreaID = this.userProfile.timeZoneAreaID;
		this.createUserForm.isOperateFromAgentPage = true;

		UsersApi.createUsers(this.createUserForm)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.closeAllModal();
				this.refresh();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	showCopyAccountDialog() {
		this.dialogVisible.isShowCopy = true;
		this.accountPrefix = '';
	}

	saveCopyAccount() {
		const vm: any = this;
		EventBus.$emit('show_mask', null);

		UsersApi.copyUser(this.copyUserForm)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.closeAllModal();
				this.refresh();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	showUpdateTargetUserPasswordDialog(user: UserModel.IUser) {
		this.updatePasswordForm.targetID = user.userID;
		this.updatePasswordForm.targetUserName = user.userName;
		this.updatePasswordForm.imChannelName = user.imChannelName;

		this.updatePasswordForm.newPassword = '';
		this.updatePasswordForm.confirmPassword = '';
		this.dialogVisible.isShowEditOther = true;
	}

	saveUpdateTargetUserPassword() {
		const vm: any = this;
		EventBus.$emit('show_mask', null);

		UsersApi.updateTargetUserPassword(this.updatePasswordForm)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.closeAllModal();
				this.refresh();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	showUpdateTargetUserPasswordDialogForCS(user: UserModel.IUser) {
		// 這個dialog是提供給CS使用的
		this.updatePasswordFormForCS.targetID = user.userID;
		this.updatePasswordFormForCS.targetUserName = user.userName;
		this.updatePasswordFormForCS.newPassword = [REDACTED];
		this.updatePasswordFormForCS.confirmPassword = [REDACTED];
		this.dialogVisible.isShowEdit2 = true;
	}

	setResetPasswordText(userName: string, newPassword: [REDACTED] {
		const vm: any = this;
		let msg = '';
		msg += 'BO url： ' + vm.getBackofficeUrl() + '\n';
		msg += userName + '／' + newPassword + '\n' + '\n';
		msg += 'Reset password done 😊' + '\n';

		this.passwordResetTextForCopy = msg;
	}

	saveToDefaultPasswordForCS() {
		this.updatePasswordFormForCS.isValidateIMChannel = true;
		this.updatePasswordFormForCS.isResetDefaultPasswordPolicy = true;

		EventBus.$emit('show_mask', null);
		UsersApi.updateTargetUserPassword(this.updatePasswordFormForCS)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.dialogVisible.isShowSuccess = true;

				this.setResetPasswordText(this.updatePasswordFormForCS.targetUserName, res.data.resultExtraString);
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	getSystemDefaultPassword() {
		const vm: any = this;
		return vm.getDefaultPassword();
	}

	resetPasswordToDefault() {
		const defaultPassword: [REDACTED] = this.getSystemDefaultPassword();

		if (confirm(this.$t('Message.__confirmResetPasswordToDefault').toString() + '\n⚠️' + 'Since 2026/10, the default password has no longer been abc123.' + '')) {
			this.updatePasswordForm.newPassword = [REDACTED];
			this.updatePasswordForm.confirmPassword = [REDACTED];
			this.updatePasswordForm.isResetDefaultPasswordPolicy = true;

			EventBus.$emit('show_mask', null);
			UsersApi.updateTargetUserPassword(this.updatePasswordForm)
				.then((res) => {
					EventBus.$emit(`successfully-modified`);
					this.dialogVisible.isShowSuccess = true;
					this.refresh();

					this.setResetPasswordText(this.updatePasswordForm.targetUserName, res.data.resultExtraString);
				})
				.finally(() => {
					setTimeout(() => {
						EventBus.$emit('hide_mask', null);
					}, 500);
				});
		}
	}

	showUpdateBriefInfoDialog(user: UserModel.IUser) {
		this.updateBriefInfoForm.userID = user.userID;
		this.updateBriefInfoForm.targetUserName = user.userName;
		this.updateBriefInfoForm.memo = user.memo;
		this.updateBriefInfoForm.status = user.status;

		this.dialogVisible.isShowEditBriefInfo = true;
	}

	saveUpdateBriefInfoDialog() {
		EventBus.$emit('show_mask', null);
		UsersApi.updateUserBriefInfo(this.updateBriefInfoForm)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.closeAllModal();
				this.refresh();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	copyMerchantIDAndText(item: any) {
		const vm: any = this;
		const myText = item.referenceCreditMerchant.toString() + '｜' + item.referenceCreditMerchantName;
		vm.copyTextToClipboard2(myText);
	}

	organizationDropdownChanged() {
		try {
			this.accountPrefix = this.dropdownItemsOrganization.filter((item) => item.valueInt === this.createUserForm.organizationID)[0].extraProperty.organizationCode;
		} catch (e) {}
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

	mounted() {
		this.refresh();

		DropdownApi.getMerchants().then((res) => {
			this.dropdownItemsMerchants = res;
		});

		DropdownApi.getOrganizations().then((res) => {
			this.dropdownItemsOrganization = res;
		});

		DropdownApi.getHierarchyMerchants().then((res) => {
			this.dropdownItemsLayer2Agents = res;
		});

		DropdownApi.getRoles().then((res) => {
			this.dropdownItemsRoles = res;
		});

		DropdownApi.getUsers(2).then((res) => {
			this.dropdownItemsUsers = res;
		});

		DropdownApi.getIMChannels().then((res) => {
			this.dropdownItemsIMChannels = res;
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
