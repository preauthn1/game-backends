<template>
	<v-app v-if="renderComponent" class="app">
		<v-expansion-panels v-model="panel" multiple>
			<v-expansion-panel>
				<v-expansion-panel-content>
					<v-flex>
						<v-row class="search-form">
							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-text-field clearable v-model="searchFilter.keyword" :label="$t('TableHeader.__keywordSearch')"> </v-text-field>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="">
									<v-autocomplete clearable :items="rolesDropdownItems" chips v-model="searchFilter.roleID" :label="$t('TableHeader.__role')"> </v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="">
									<v-autocomplete clearable :items="organizationsDropdownItems" chips v-model="searchFilter.organizationID" :label="$t('TableHeader.__organization')">
									</v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<!-- <div class="item right"> -->
								<div class="search-row-padding">
									<v-btn color="yellow darken-1" text class="btn search" @click.stop="clickSearch">
										<v-icon left>search</v-icon>
										{{ $t('Button.__search') }} </v-btn
									>&#9;
									<v-btn color="pink lighten-3" text class="btn search" @click.stop="clear">
										<v-icon left>clear</v-icon>
										{{ $t('Button.__reset') }} </v-btn
									>&#9;
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3" class="minus-mt">
								<div class="">
									<v-autocomplete clearable :items="hierarchyMerchantsDropdownItems" chips v-model="searchFilter.referenceCreditMerchantID" :label="'採用樹狀代理瀏覽'">
									</v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3" class="minus-mt">
								<div class="">
									<v-autocomplete clearable :items="merchantsDropdownItems" chips v-model="searchFilter.assignMerchantID" :label="'已分配商戶(代理樹狀模式不可用此搜尋)'">
									</v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3" class="minus-mt">
								<div class="">
									<v-autocomplete clearable :items="IMChannelDropdownItems" chips v-model="searchFilter.imChannelID" :label="'TG/Skype頻道'"> </v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3" class="minus-mt">
								<div class="">
									<v-autocomplete clearable :items="specialFilterTypeDropdownItems" chips v-model="searchFilter.specialMerchantFilterType" :label="'特殊篩選'"> </v-autocomplete>
								</div>
							</v-col>
						</v-row>
					</v-flex>
				</v-expansion-panel-content>
			</v-expansion-panel>
		</v-expansion-panels>

		<v-data-table
			:items="userData"
			:headers="headers"
			id="table"
			:disable-sort="true"
			:page="DataTablePagingParameter.pageIndex"
			:items-per-page="DataTablePagingParameter.itemsPerPage"
			:server-items-length="DataTablePagingParameter.totalCount"
			:footer-props="DataTablePagingParameter.footerProps"
			@update:page="changePage"
			@update:items-per-page="getItemPerPage"
			multi-sort
		>
			<template v-slot:top>
				<v-toolbar>
					<v-toolbar-title>
						<h4>{{ $t('TableHeader.__account') }}</h4>
					</v-toolbar-title>
					<v-divider class="mx-4" inset vertical></v-divider>
					<v-spacer></v-spacer>
					<v-card-title>
						<v-btn icon dark @click="addItem" v-show="hasPermissionCreate" class="ml-4">
							<v-icon>add</v-icon>
						</v-btn>
					</v-card-title>
				</v-toolbar>
			</template>

			<template v-slot:item.action="{ item }">
				<v-tooltip top>
					<template v-slot:activator="{ on, attrs }">
						<v-icon v-if="hasPermissionUpdate" v-bind="attrs" v-on="on" small class="mr-2" @click="editItem(item)"> edit </v-icon>
					</template>
					<span>{{ $t('Button.__edit') }}</span>
				</v-tooltip>

				<v-tooltip top>
					<template v-slot:activator="{ on, attrs }">
						<v-icon small class="mr-2" @click.stop="showUpdateTargetUserPasswordDialog(item)" v-bind="attrs" v-on="on"> lock </v-icon>
					</template>
					<span>{{ $t('Popup.__updatePassword') }}</span>
				</v-tooltip>

				<v-tooltip top>
					<template v-slot:activator="{ on, attrs }">
						<v-icon small class="mr-2" @click="copyText(item)" v-bind="attrs" v-on="on"> file_copy </v-icon>
					</template>
					<span>複製帳號密碼</span>
				</v-tooltip>

				<v-tooltip top>
					<template v-slot:activator="{ on, attrs }">
						<v-icon v-if="hasPermissionDelete" small class="mr-2" @click="deleteItem(item)" v-bind="attrs" v-on="on"> delete </v-icon>
					</template>
					<span>{{ $t('Button.__delete') }}</span>
				</v-tooltip>
			</template>

			<template v-slot:item.status="{ item }">
				<v-chip small :color="item.status == 0 ? 'red darken-3' : item.status == 88 ? 'purple' : 'green darken-2'">
					{{ item.statusText }}
				</v-chip>
			</template>

			<template v-slot:item.organizationName="{ item }">
				<div :title="item.organizationID">{{ item.organizationName }}</div>

				<div class="sub-title-text">
					{{ item.imChannelName }}
				</div>
			</template>

			<template v-slot:item.userName="{ item }">
				<div>{{ item.userName }}</div>
				<div class="sub-title-text">
					{{ item.memo }}
					<v-chip color="purple darken-2" x-small v-if="item.riskScore > 0">分數：{{ item.riskScore }}</v-chip>
				</div>
			</template>

			<template v-slot:item.referenceCreditMerchantName="{ item }">
				<span :title="item.referenceCreditMerchant">{{ item.referenceCreditMerchantName }}</span>
			</template>

			<template v-slot:item.roleName_CN="{ item }">
				<span class="f14" :title="item.roleID">{{ item.roleName_CN }}</span>
			</template>

			<template v-slot:item.assignMerchantModeText="{ item }">
				<v-chip small>{{ item.assignMerchantModeText }}</v-chip>
			</template>

			<template v-slot:item.createTime="{ item }">
				<div>
					{{ item.createTime | timeWithAddTimezone(userProfile.timeZone) }}
				</div>
				<div class="sub-title-text">
					{{ item.createBy }}
				</div>
			</template>

			<template v-slot:item.isFinishBind2FA="{ item }">
				<span v-if="item.isFinishBind2FA">
					<v-icon color="green">check</v-icon>
				</span>
				<span v-else>
					<v-icon color="red">close</v-icon>
				</span>
			</template>

			<template v-slot:item.lastLogonTime="{ item }">{{ item.lastLogonTime | timeWithAddTimezone(userProfile.timeZone) }} </template>
		</v-data-table>

		<!-- 新增/編輯 -->
		<v-dialog v-model="dialogVisible.isShowEdit" max-width="800px" persistent scrollable>
			<v-card class="dialog-v-card-border">
				<v-card-title class="grey darken-2">
					{{ formTitle }}
					<div class="top-right-item" @click="close">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>
				<v-card-text class="col-padding-small" style="min-height: 500px">
					<v-flex>
						<v-row>
							<v-divider class="mx-4" inset vertical></v-divider>
						</v-row>
						<v-row>
							<v-col cols="12" sm="6" md="6">
								<v-autocomplete
									:disabled="editedIndex !== -1"
									:items="organizationsDropdownItems"
									autofocus
									v-model="editedItem.organizationID"
									:label="$t('TableHeader.__organizationName')"
									chips
									@change="changeOrganization"
								>
								</v-autocomplete>
							</v-col>

							<v-col cols="12" sm="6" md="6"> </v-col>
						</v-row>

						<v-row style="margin-top: -5px !important" v-if="isEditForm || editedItem.organizationID > 0">
							<v-col cols="12" sm="12" md="12" v-if="!isEditForm">
								<div style="margin-bottom: -30px">
									<v-icon small color=" grey lighten-1" class="mr-1">error</v-icon>
									{{ $t('Popup.__warningFullLoginName') }}：
									<span style="color: #ff584c; font-weight: 700"> {{ organizationTag }}{{ editedItem.userName }} </span>
								</div>
							</v-col>

							<v-col cols="12" sm="6" md="6">
								<v-text-field
									:prefix="organizationTag"
									class=""
									v-if="!isEditForm"
									:disabled="editedIndex !== -1"
									v-model="editedItem.userName"
									@blur="whenBlurUserName()"
									@change="renderComponentAgain()"
									:placeholder="$t('Message.__pleaseInput')"
									:label="$t('TableHeader.__userName')"
									required
									clearable
								>
								</v-text-field>

								<v-text-field class="required" v-else :disabled="isEditForm" readonly v-model="editedItem.userName" :label="$t('TableHeader.__userName')" required clearable>
								</v-text-field>
							</v-col>

							<v-col cols="12" sm="6" md="6">
								<v-text-field class="" v-model="editedItem.displayName" autocomplete="off" :label="$t('TableHeader.__displayName2')" required clearable> </v-text-field>
							</v-col>

							<v-col cols="12" sm="6" md="6">
								<v-text-field
									class=""
									v-model="editedItem.password"
									autocomplete="off"
									:label="$t('TableHeader.__password')"
									:type="'text'"
									v-if="!isEditForm"
									clearable
								></v-text-field>
							</v-col>

							<v-col cols="12" sm="6" md="6">
								<v-text-field
									class=""
									v-model="editedItem.confirmPassword"
									autocomplete="off"
									:label="$t('TableHeader.__password2')"
									:type="'text'"
									v-if="!isEditForm"
									clearable
								></v-text-field>
							</v-col>

							<v-col cols="12" sm="6" md="6">
								<v-autocomplete class="" :items="rolesDropdownItems" v-model="editedItem.roleID" small-chips :label="$t('TableHeader.__roleName')"></v-autocomplete>
							</v-col>

							<v-col cols="12" sm="6" md="6">
								<v-autocomplete v-model="editedItem.timeZoneAreaID" menu-props="auto" small-chips :items="timezoneDropdownItems" :label="$t('TableHeader.__timeZoneArea')">
								</v-autocomplete>
							</v-col>

							<v-col cols="12" sm="6" md="6">
								<v-autocomplete
									v-model="editedItem.assignMerchantMode"
									small-chips
									@change="whenChangeAssignType()"
									:items="userAssignMerchantModeDropdownItems"
									:label="'商戶分配模式'"
								>
								</v-autocomplete>
							</v-col>

							<v-col cols="12" sm="6" md="6">
								<v-text-field class="" v-model="editedItem.memo" :label="$t('TableHeader.__memo')" clearable> </v-text-field>
							</v-col>

							<v-col cols="12" sm="12" md="12" v-if="editedItem.assignMerchantMode == null || editedItem.assignMerchantMode === 0">
								<v-autocomplete deletable-chips disabled :label="$t('TableHeader.__merchant')"> </v-autocomplete>
							</v-col>

							<v-col cols="12" sm="12" md="12" v-if="editedItem.assignMerchantMode === 1">
								<div class="text-r">
									<v-autocomplete deletable-chips v-model="editedItem.merchants" clearable :items="merchantsDropdownItems" small-chips :label="$t('TableHeader.__merchant')" multiple>
									</v-autocomplete>

									<div @click="showParseTextToListIntDialog()" class="link-text-grey f12 minus-mt-2 z-999">貼上多筆商戶IDs</div>
								</div>
							</v-col>

							<v-col cols="12" sm="12" md="12" v-if="editedItem.assignMerchantMode === 5">
								<v-autocomplete
									v-model="editedItem.referenceCreditMerchant"
									:items="hierarchyMerchantsDropdownItems"
									@change="whenReferenceCreditMerchantChanged"
									clearable
									small-chips
									:label="'使用代理樹狀模式瀏覽 (單選)'"
								>
								</v-autocomplete>
							</v-col>

							<v-col cols="12" sm="12" md="12" v-if="editedItem.assignMerchantMode === 99">
								<v-autocomplete
									v-model="editedItem.merchants"
									deletable-chips
									:items="hierarchyMerchantsDropdownItems"
									clearable
									small-chips
									multiple
									:label="'代理樹狀模式瀏覽(可多選)'"
								>
								</v-autocomplete>
							</v-col>

							<v-col cols="12" sm="12" md="12">
								<v-radio-group row v-model="editedItem.status">
									<v-radio :label="$t('Status.Enable')" :value="status.Enable"></v-radio>
									<v-radio :label="$t('Status.Disable')" :value="status.Disable"></v-radio>
									<v-radio :label="'永久禁用(高級)'" :value="88"></v-radio>
								</v-radio-group>
							</v-col>

							<v-col cols="12" sm="6" md="6"> </v-col>
						</v-row>
					</v-flex>
				</v-card-text>
				<v-card-actions style="margin-top: 40px">
					<v-spacer></v-spacer>

					<!-- <v-btn text color="pink lighten-3" v-if="editedItem.referenceCreditMerchant > 0"
						@click="resetReferenceCreditMerchant(editedItem.userID)" class="btn">
						<v-icon left>mdi-cancel</v-icon>
						取消代理樹狀瀏覽模式
					</v-btn> -->

					<v-btn text @click="close" class="btn">
						<v-icon left>mdi-cancel</v-icon>
						{{ $t('Button.__cancel') }}
					</v-btn>

					<v-btn color="yellow darken-1" text @click="save" class="btn">
						<v-icon left>mdi-check-circle</v-icon>
						{{ $t('Button.__submit') }}
					</v-btn>
				</v-card-actions>
			</v-card>
		</v-dialog>

		<!-- 修改密碼 -->
		<v-dialog v-model="dialogVisible.isShowEditOther" max-width="600px" persistent>
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
							<v-text-field :label="$t('TableHeader.__password') + ' (New)'" :type="'password'" v-model="updatePasswordForm.newPassword"> </v-text-field>
						</v-row>

						<v-row>
							<v-text-field :label="$t('TableHeader.__password2')" :type="'password'" v-model="updatePasswordForm.confirmPassword"> </v-text-field>
						</v-row>
					</div>
				</v-card-text>

				<v-card-actions>
					<v-spacer></v-spacer>

					<v-btn text @click="forceLogout()" class="btn" color="blue lighten-2">
						<v-icon color="blue lighten-2" left>mdi-exit-to-app</v-icon>
						強制登出帳號
					</v-btn>

					<v-btn text @click="resetPasswordToDefault()" class="btn" color="pink lighten-2">
						<v-icon color="pink lighten-2" left>mdi-lock</v-icon>
						{{ $t('Button.__resetPasswordToDefault') }}
					</v-btn>

					<v-btn text @click="closeAllModal" class="btn">
						<v-icon left>mdi-cancel</v-icon>
						{{ $t('Button.__cancel') }}
					</v-btn>

					<v-btn color="yellow darken-1" text @click="saveUpdateTargetUserPassword" class="btn">
						<v-icon left>mdi-check-circle</v-icon>
						{{ $t('Button.__submit') }}
					</v-btn>
				</v-card-actions>
			</v-card>
		</v-dialog>

		<!-- 密碼重設成功 -->
		<v-dialog v-model="dialogVisible.isShowSuccess" max-width="600px">
			<v-card>
				<v-card-title class="grey darken-2">
					{{ $t('Button.__copyToClipboard') }} ({{ $t('TableHeader.__account') }} / {{ $t('TableHeader.__password') }})

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

		<TextareaParseToListIDPopup ref="refTextareaParseToListIDPopup" @parentSetResultIDs="setResultMerchantIDs"> </TextareaParseToListIDPopup>

		<NotificationWarning
			v-show="showNotificationWarning"
			:title="`${$t('Message.__warning')}`"
			:max="1"
			:text="`${$t('Message.__isConfirmDelete')}#${deleteIndex}?`"
			:confirmed-action="confirmedAction"
			:canceled-action="canceledAction"
		/>
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import * as Model from '@/models/interfaces/operator-management/users';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import UsersApi from '@/api/operator-management/users';
import DropdownApi from '@/api/commons/dropdown';
import { Locale, Status } from '@/models/status/common';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import NotificationWarning from '@/components/notification/NotificationWarning.vue';
import { pageData, time } from '@/utilities/api-handler.lib';
import { datetimeMixin } from '@/utilities/datetime-format';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import TextareaParseToListIDPopup from '@/components/system-setting/TextareaParseToListIDPopup.vue';
import { commonMethodMixin } from '@/utilities/common-methods';
import { State, Action, Getter, namespace } from 'vuex-class';
const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';
@Component({ components: { NotificationWarning, TextareaParseToListIDPopup }, mixins: [commonDisplayFiltersMixin, datetimeMixin, commonMethodMixin] })
export default class Users extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];
	status: typeof Status = Status;
	renderComponent: boolean = true;

	dialogVisible: DropdownModel.DialogVisible = new DropdownModel.DialogVisible();

	DataTablePagingParameter: DropdownModel.DataTablePagingParameter = new DropdownModel.DataTablePagingParameter();
	searchFilter: Model.IUserFilter = {} as Model.IUserFilter;
	searchKeyword: string = '';
	// headers: any = [];
	userData: Model.IUser[] = [];
	editedIndex: number = -1;
	editedItem: Model.IUserRequest = {} as Model.IUserRequest;
	deleteIndex: number = -1;
	showNotificationWarning: boolean = false;
	passwordResetTextForCopy: string = '';

	IMChannelDropdownItems: DropdownModel.IDropdownItems[] = [];
	organizationsDropdownItems: DropdownModel.IDropdownItems[] = [];
	organizationTag: string = '组织前缀';

	selectedOrganization: DropdownModel.IDropdownItems = {} as DropdownModel.IDropdownItems;
	merchantsDropdownItems: DropdownModel.IDropdownItems[] = [];
	hierarchyMerchantsDropdownItems: DropdownModel.IDropdownItems[] = [];
	rolesDropdownItems: DropdownModel.IDropdownItems[] = [];
	timezoneDropdownItems: DropdownModel.IDropdownItems[] = [];
	userAssignMerchantModeDropdownItems: DropdownModel.IDropdownItems[] = [];
	specialFilterTypeDropdownItems: DropdownModel.IDropdownItems[] = [];

	updatePasswordForm: Model.IUpdatePasswordRequest = {} as Model.IUpdatePasswordRequest;

	panel: number[] = [0];
	show1: boolean = false;
	show2: boolean = false;

	get hasPermissionCreate() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.OperatorManagementAccounts_Create) >= 0;
	}
	get hasPermissionUpdate() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.OperatorManagementAccounts_Update) >= 0;
	}
	get hasPermissionDelete() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.OperatorManagementAccounts_Delete) >= 0;
	}
	get headers() {
		const data = [
			{ text: this.$t('TableHeader.__ID'), value: 'userID', width: '5%' },
			{ text: this.$t('TableHeader.__account'), value: 'userName', width: '15%' },
			{ text: this.$t('TableHeader.__organizationName'), value: 'organizationName' },
			{ text: '商戶瀏覽模式', value: 'assignMerchantModeText', sortable: false },
			{ text: '採用樹狀代理瀏覽', value: 'referenceCreditMerchantName' },
			{ text: this.$t('TableHeader.__roleName'), value: 'roleName_CN' },
			{ text: this.$t('TableHeader.__status'), value: 'status' },
			{ text: this.$t('TableHeader.__createTime2'), value: 'createTime' },
			{ text: this.$t('TableHeader.__lastLoginTime'), value: 'lastLogonTime' },
			{ text: '綁定2FA', value: 'isFinishBind2FA' },
		];
		if (this.hasPermissionUpdate || this.hasPermissionDelete) {
			// @ts-ignore
			data.splice(0, 0, { text: this.$t('TableHeader.__lock'), value: 'action', sortable: false });
		}
		return data;
	}
	get formTitle() {
		return this.isEditForm ? this.$t('Popup.__update') : this.$t('Popup.__create');
	}

	get isEditForm() {
		return this.editedIndex === -1 ? false : true;
	}

	changePage(index: number = 1) {
		this.DataTablePagingParameter.pageIndex = index;
		if (this.userData.length > 0) {
			this.search();
		}
	}

	getItemPerPage(val: number = 10) {
		this.DataTablePagingParameter.itemsPerPage = val;
		if (this.userData.length > 0) {
			this.search();
		}
	}

	whenChangeAssignType() {
		this.editedItem.merchants = [];
		this.editedItem.referenceCreditMerchant = null;
	}

	whenReferenceCreditMerchantChanged() {
		this.editedItem.merchants = [];
	}

	clear() {
		this.searchFilter = {} as Model.IUserFilter;
	}

	clickSearch() {
		this.DataTablePagingParameter.pageIndex = 1;
		this.search();
	}

	search() {
		this.refresh();
	}

	refresh() {
		this.registerReloadComponent();

		this.searchFilter.pageIndex = this.DataTablePagingParameter.pageIndex;
		this.searchFilter.pageSize = this.DataTablePagingParameter.itemsPerPage;

		EventBus.$emit('show_mask', null);
		UsersApi.getUsers(this.searchFilter)
			.then((res) => {
				this.userData = res.data.items;
				this.DataTablePagingParameter.totalCount = res.data.totalCount;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	whenBlurUserName() {
		console.log(this.editedItem.displayName.length);

		if (this.editedItem.displayName != null && this.editedItem.displayName.length <= 0) {
			this.editedItem.displayName = this.editedItem.userName;
			this.renderComponentAgain();
		}
	}

	renderComponentAgain() {
		this.dialogVisible.isShowEdit = false;
		this.dialogVisible.isShowEdit = true;
	}

	resetReferenceCreditMerchant(targetUserID: number) {
		if (confirm('*若取消此帳號樹狀瀏覽。請於儲存後自行編輯勾選分配商戶')) {
			EventBus.$emit('show_mask', null);
			UsersApi.resetReferenceCreditMerchant(targetUserID)
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

	registerReloadComponent() {
		EventBus.$on(`reload-${this.$router.currentRoute.path}`, (info: any) => {
			this.renderComponent = false;
			this.$nextTick(() => {
				this.searchKeyword = '';
				this.renderComponent = true;
			});
		});
	}

	addItem() {
		this.editedIndex = -1;
		this.dialogVisible.isShowEdit = true;

		this.editedItem.userName = '';
		this.editedItem.displayName = '';

		const vm: any = this;
		const defaultPassword: [REDACTED] = vm.getDefaultPassword();
		this.editedItem.password = [REDACTED];
		this.editedItem.confirmPassword = [REDACTED];
		this.editedItem.referenceCreditMerchant = null;
		this.editedItem.merchants = [];

		this.editedItem.status = Status.Enable;
		this.editedItem.timeZoneAreaID = this.userProfile.timeZoneAreaID;
	}

	editItem(item: Model.IUser) {
		EventBus.$emit('show_mask', null);
		UsersApi.getUser(item.userID)
			.then((res) => {
				this.editedItem.userID = res.data.userID;
				this.editedItem.userName = res.data.userName;
				this.editedItem.displayName = res.data.displayName;
				this.editedItem.organizationID = res.data.organizationID;
				this.editedItem.referenceCreditMerchant = res.data.referenceCreditMerchant;

				this.editedItem.memo = res.data.memo;
				this.editedItem.roleID = res.data.roleID.toString();
				this.editedItem.timeZoneAreaID = res.data.timeZoneAreaID;
				this.editedItem.merchants = res.data.merchants.map((merchant) => merchant);
				this.editedItem.status = res.data.status;
				this.editedItem.assignMerchantMode = res.data.assignMerchantMode;

				this.editedItem = { ...this.editedItem };

				// 如果先打開create 再打開edit 會導致password輸入default
				this.editedItem.password = '';
				this.editedItem.confirmPassword = '';
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});

		this.editedIndex = this.userData.indexOf(item);
		this.dialogVisible.isShowEdit = true;
	}

	setResultMerchantIDs(IDs: number[]) {
		this.editedItem.merchants = [];
		for (const id of IDs) {
			this.editedItem.merchants.push(id);
		}

		this.dialogVisible.isShowEdit = false;
		this.$nextTick(() => {
			this.dialogVisible.isShowEdit = true;
		});
	}

	showParseTextToListIntDialog() {
		const targetRef: any = this.$refs.refTextareaParseToListIDPopup;
		targetRef.showParseTextareaPraseToIDs();
	}

	close() {
		this.closeAllModal();
		setTimeout(() => {
			this.editedIndex = -1;
		}, 300);
	}

	closeAllModal() {
		this.showNotificationWarning = false;
		this.dialogVisible.resetAllValueToFalse();
	}

	save() {
		const request = <Model.IUserRequest>{ ...this.editedItem };
		if (this.editedItem.organizationID) {
			request.organizationID = Number(this.editedItem.organizationID);
		}
		if (this.editedItem.roleID) {
			request.roleID = Number(this.editedItem.roleID);
		}
		if (this.editedItem.timeZoneAreaID) {
			request.timeZoneAreaID = Number(this.editedItem.timeZoneAreaID);
		}
		if (this.editedItem.merchants) {
			request.merchants = this.editedItem.merchants.map((merchant: string) => {
				return Number(merchant);
			});
		}
		const temp = { ...request };

		EventBus.$emit('show_mask', null);

		if (this.editedIndex > -1) {
			UsersApi.updateUsers(<Model.IUserRequest>this.editedItem)
				.then(() => {
					EventBus.$emit(`successfully-modified`);
					this.close();
					this.clickSearch();
				})
				.finally(() => {
					EventBus.$emit('hide_mask', null);
				});
		} else {
			request.userName = `${this.organizationTag}${this.editedItem.userName}`;

			if (this.organizationTag === '组织前缀') {
				alert('頁面閒置過久，瀏覽器請F5刷新重新操作');
				location.reload();
			}

			UsersApi.createUsers(<Model.IUserRequest>request)
				.then(() => {
					EventBus.$emit(`successfully-modified`);
					this.close();
					this.clickSearch();
				})
				.finally(() => {
					EventBus.$emit('hide_mask', null);
				});
		}
	}

	showUpdateTargetUserPasswordDialog(item: Model.IUser) {
		this.dialogVisible.isShowEditOther = true;

		this.updatePasswordForm.targetID = item.userID;
		this.updatePasswordForm.targetUserName = item.userName;
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
					this.closeAllModal();

					const myModel = {} as Model.IUser;
					myModel.userID = this.updatePasswordForm.targetID;
					myModel.userName = this.updatePasswordForm.targetUserName;
					myModel.isDefaultPassword = [REDACTED];

					this.copyText(myModel);
					this.setResetPasswordText(this.updatePasswordForm.targetUserName, res.data.resultExtraString);

					this.refresh();
				})
				.finally(() => {
					setTimeout(() => {
						EventBus.$emit('hide_mask', null);
					}, 500);
				});
		}
	}

	copySuccess() {
		EventBus.$emit(`successfully-copy`);
	}

	setResetPasswordText(userName: string, newPassword: [REDACTED] {
		const vm: any = this;
		vm.dialogVisible.isShowSuccess = true;

		let msg = '';
		msg += 'BO url： ' + vm.getBackofficeUrl() + '\n';
		msg += userName + '／' + newPassword + '\n';

		this.passwordResetTextForCopy = msg;
	}

	forceLogout() {
		if (confirm('是否強制登出此帳號?')) {
			EventBus.$emit('show_mask', null);
			UsersApi.forceLogoutUser(this.updatePasswordForm.targetID)
				.then((res) => {
					EventBus.$emit(`successfully-modified`);
					this.closeAllModal();
				})
				.finally(() => {
					setTimeout(() => {
						EventBus.$emit('hide_mask', null);
					}, 500);
				});
		}
	}

	deleteItem(item: Model.IUser) {
		this.deleteIndex = item.userID;
		this.showNotificationWarning = true;
		this.$notify({
			group: 'confirm',
			type: 'warn',
			duration: -1,
			speed: 10,
		});
	}

	confirmedAction() {
		UsersApi.deleteUsers(this.deleteIndex).then(() => {
			EventBus.$emit(`successfully-modified`);
			this.close();
			this.refresh();
		});

		this.showNotificationWarning = false;
	}

	copyText(item: Model.IUser) {
		const vm: any = this;
		EventBus.$emit('show_mask', null);
		return UsersApi.requestUserDefaultPassword(item.userID)
			.then((res) => {
				vm.setResetPasswordText(item.userName, res.data.userPassword);
			})
			.finally(() => {
				setTimeout(() => {
					EventBus.$emit('hide_mask', null);
				}, 500);
			});
	}

	canceledAction() {
		this.showNotificationWarning = false;
	}

	changeOrganization() {
		const index = this.organizationsDropdownItems.findIndex((s) => s.value === this.editedItem.organizationID);
		if (index > -1) {
			this.organizationTag = `${this.organizationsDropdownItems[index].extraProperty.organizationCode}-`;
		}
	}

	mounted() {
		DropdownApi.getTimezoneAreas().then((res) => {
			this.timezoneDropdownItems = res;
		});
		DropdownApi.getOrganizations().then((res) => {
			this.organizationsDropdownItems = res;
		});
		DropdownApi.getRoles().then((res) => {
			this.rolesDropdownItems = res;
		});
		DropdownApi.getHierarchyMerchants().then((res) => {
			this.hierarchyMerchantsDropdownItems = res;
		});
		DropdownApi.getMerchants().then((res) => {
			this.merchantsDropdownItems = res;
		});
		DropdownApi.getIMChannels().then((res) => {
			this.IMChannelDropdownItems = res;
		});

		DropdownApi.getUserAssignMerchantMode().then((res) => {
			this.userAssignMerchantModeDropdownItems = res;
		});
		DropdownApi.getUserSpecialFilterType().then((res) => {
			this.specialFilterTypeDropdownItems = res;
		});

		this.refresh();

		// 鍵盤事件監聽
		const vm = this;
		document.addEventListener('keydown', (event) => {
			const key = event.key; // const {key} = event; in ES6+
			if (key === 'Escape') {
				vm.closeAllModal();
			}
		});
	}
}
</script>

<style scoped>
.minus-mt {
	margin-top: -15px;
}
</style>
