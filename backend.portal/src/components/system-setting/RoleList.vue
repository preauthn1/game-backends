<template>
	<v-app v-if="renderComponent" class="app">

		<div>

			<v-btn color="blue lighten-3" text class="btn search" @click.stop="showCompareRolesDialog">
				<v-icon left>visibility</v-icon>
				角色權限比對
			</v-btn>&#9;

		</div>

		<v-data-table :headers="headers" id="table" :items="rolesData" :search="search" multi-sort
			:footer-props="DataTablePagingParameter.footerProps" :items-per-page="defaultPageSize">
			<template v-slot:top>
				<v-toolbar>
					<v-toolbar-title>
						<h4>{{$t('TableHeader.__role')}}</h4>
					</v-toolbar-title>
					<v-divider class="mx-4" inset vertical></v-divider>
					<v-spacer></v-spacer>
					<v-card-title>

						<template>

							<v-text-field v-model="search" append-icon="search" class="mr-4"
								:label="`${$t('TableHeader.__roleID')} / ${$t('TableHeader.__roleName')}`"
								single-line hide-details clearable>
							</v-text-field>

							<v-btn icon dark @click="addRoles" v-show="hasPermissionCreate">
								<v-icon>add</v-icon>
							</v-btn>

							<v-btn icon dark @click="refresh">
								<v-icon>refresh</v-icon>
							</v-btn>

						</template>

					</v-card-title>

				</v-toolbar>
			</template>

			<template v-slot:item.userMappingCount="{ item }">
				<span @click="showMappingUserAccountsByRoleID(item.roleID)" class="link-text-grey">
					{{item.userMappingCount}}
				</span>
			</template>

			<template v-slot:item.createTime="{ item }">
				{{ item.createTime | timeWithAddTimezone(userProfile.timeZone) }}
			</template>
			<template v-slot:item.updateTime="{ item }">
				{{ item.updateTime | timeWithAddTimezone(userProfile.timeZone) }}
			</template>
			<template v-slot:item.roleName="{ item }">

				<div>
					<div>
						{{ item.roleName }}
						<v-icon v-if="hasPermissionRename" small class="mr-2" @click="editRole(item)">
							edit
						</v-icon>
					</div>
					<div class="sub-title-text">
						{{item.memo}}
					</div>
				</div>

			</template>
			<template v-slot:item.action="{ item }">
				<v-icon v-if="hasPermissionUpdate" small class="mr-2" @click="editPermission(item)">edit
				</v-icon>
				<v-icon v-if="hasPermissionDelete" small @click="deleteItem(item)">delete</v-icon>
			</template>
		</v-data-table>

		<!-- 新增編輯 -->
		<v-dialog v-model="dialogEditRoles" max-width="500px">
			<EditRolePopup v-if="dialogEditRoles" :propsData="editedItem"
				@update-role-success="editRoleSuccess" @close-update-role="dialogEditRoles = false" />
		</v-dialog>

		<!-- 編輯role權限 -->
		<v-dialog content-class="dialog-edit-permissions no-scroll-y" v-model="dialogEditPermissions"
			max-width="500px">
			<EditPermissionsPopup v-if="dialogEditPermissions" :propsData="editedItem"
				@update-role-permissions-success="editRolePermissionSuccess"
				@close-update-role-permissions="dialogEditPermissions = false" />
		</v-dialog>

		<!-- 比對角色 -->
		<v-dialog v-model="isShowCompareRoles" max-width="900px" content-class="no-scroll-y">

			<v-card class="createForm" style="min-height: 300px;">
				<v-card-title class="grey darken-2">
					<div>
						比對角色
					</div>
				</v-card-title>

				<v-card-text>

					<v-row>
						<v-col cols="12" sm="8" md="8">
							<v-autocomplete :items="roleDropdownItems" :label="'選擇角色'" multiple chips clearable
								v-model="compareRolesFilter.roleIDs">

							</v-autocomplete>

						</v-col>
						<v-col cols="12" sm="4" md="4">
							<v-btn color="blue lighten-3" text class="btn search mt-4"
								@click.stop="searchCompareResult">
								<v-icon left>search</v-icon>
								查詢
							</v-btn>&#9;

						</v-col>
					</v-row>

					<div class="v-data-table theme--dark"
						v-if="compareRoleResult && compareRoleResult.roles && compareRoleResult.roles.length > 0">

						<div class="v-data-table__wrapper">

							<table class="scrollv2">

								<thead class="v-data-table-header">

									<tr>
										<!-- <th class="permissionCellSmall">主分類</th> -->
										<th class="permissionCell">項目</th>
										<th v-for="(role , index) in compareRoleResult.roles" :key="index"
											class="roleCell">

											<div class="text-c">
												<div style="color:#FFF;">
													{{role.roleName}}
												</div>
												<div class="grey-text f10">
													{{role.roleID}}
												</div>
											</div>

										</th>
									</tr>

								</thead>

								<tbody>

									<tr v-for="(item , index) in compareRoleResult.permissions" :key="index">

										<!-- <td
											:class="'permissionCellSmall ' + (item.isAllValueEqual ? 'grey-text' : 'yellow-text') ">
											{{ getMenui18nTextAsString(item.rootPermissionToken) }}
										</td> -->

										<td :title="item.permissionID"
											:class="'permissionCell ' + (item.isAllValueEqual ? 'grey-text' : 'yellow-text') ">

											<v-icon @click="showEachRoleIsEnablePermission(item.permissionID)" small>
												info
											</v-icon>

											<span v-for="(myItem , myIndex) in item.permissionNevigatePath"
												:key="myIndex">
												<span v-if="myItem !== item.permissionNevigatePath[0]"> > </span>
												{{ getMenui18nTextAsString(myItem) }}
											</span>

										</td>

										<td v-for="(roleFlag , index2) in item.roleEnableFlag" :key="index2"
											class="text-c roleCell">
											{{roleFlag}}
										</td>
									</tr>

								</tbody>
							</table>

						</div>
					</div>

				</v-card-text>

			</v-card>

		</v-dialog>

		<NotificationWarning v-show="showNotificationWarning" :title="`${$t('Message.__warning')}`"
			:max="1" :text="`${$t('Message.__isConfirmDelete')}#${deleteIndex}?`"
			:confirmed-action="confirmedAction" :canceled-action="canceledAction" />
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import * as Model from '@/models/interfaces/system-setting/roles';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import RolesApi from '@/api/system-setting/roles';
import PermissionApi from '@/api/commons/permission';
import DropdownApi from '@/api/commons/dropdown';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import NotificationWarning from '@/components/notification/NotificationWarning.vue';
import EditPermissionsPopup from '@/components/system-setting/RolePermissionsEditPopup.vue';
import CustomizedColumnsDisplayPopupDialog from '@/components/CustomizedColumnsDisplayPopupDialog.vue';
import EditRolePopup from '@/components/system-setting/RoleEditPopup.vue';
import { pageData, time } from '@/utilities/api-handler.lib';
import { datetimeMixin } from '@/utilities/datetime-format';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import { State, Action, Getter, namespace } from 'vuex-class';
const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';
@Component({ components: { EditPermissionsPopup, EditRolePopup, NotificationWarning }, mixins: [commonDisplayFiltersMixin, datetimeMixin] })
export default class RoleList extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];
	renderComponent: boolean = true;
	dialogAddRoles: boolean = false;
	dialogEditRoles: boolean = false;
	dialogEditPermissions: boolean = false;
	isShowCompareRoles: boolean = false;
	search: string = '';

	rolesData: Model.IRole[] = [];
	editedIndex: number = -1;
	editedItem: Model.IUpdateRoleRequest = {} as Model.IUpdateRoleRequest;
	defaultPageSize: number = 30;

	compareRoleResult: PermissionModel.IRolePermissionCompare = {} as PermissionModel.IRolePermissionCompare;
	compareRolesFilter: DropdownModel.ILazyCommonParameterReqeust = {} as DropdownModel.ILazyCommonParameterReqeust;

	DataTablePagingParameter: DropdownModel.DataTablePagingParameter = new DropdownModel.DataTablePagingParameter();
	deleteIndex: number = -1;
	showNotificationWarning: boolean = false;

	roleDropdownItems: DropdownModel.IDropdownItems[] = [];

	get hasPermissionCreate() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSettingRoles_Create) >= 0;
	}
	get hasPermissionUpdate() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSettingRoles_Update) >= 0;
	}
	get hasPermissionDelete() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSettingRoles_Delete) >= 0;
	}
	get hasPermissionRename() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSettingRoles_Rename) >= 0;
	}
	get headers() {
		const data = [
			{ text: this.$t('TableHeader.__roleID'), value: 'roleID', sortable: true },
			{ text: this.$t('TableHeader.__roleName'), value: 'roleName' },
			{ text: '對應帳號數', value: 'userMappingCount' },
			{ text: this.$t('TableHeader.__createBy'), value: 'createBy' },
			{ text: this.$t('TableHeader.__createTime'), value: 'createTime' },
			{ text: this.$t('TableHeader.__updateTime'), value: 'updateTime' },
			{ text: this.$t('TableHeader.__updateBy'), value: 'updateBy' },
			{ text: 'Sort', value: 'sort' },
		];
		if (this.hasPermissionUpdate || this.hasPermissionDelete) {
			data.splice(0, 0, { text: this.$t('TableHeader.__action'), value: 'action', sortable: false });
		}
		return data;
	}

	registerReloadComponent() {
		EventBus.$on(`reload-${this.$router.currentRoute.path}`, (info: any) => {
			this.renderComponent = false;
			this.$nextTick(() => {
				this.search = '';
				this.renderComponent = true;
			});
		});
	}

	refresh() {
		this.registerReloadComponent();
		RolesApi.getRoles().then((res) => {
			this.rolesData = res.data.items;
			const mod: number = res.data.totalCount % pageData.pageSize === 0 ? 0 : 1;
			if (res.data.totalCount > pageData.pageSize) {
				for (let i = 2; i <= res.data.totalCount / pageData.pageSize + mod; i++) {
					setTimeout(() => {
						RolesApi.getRoles(i).then((restData) => {
							this.rolesData.push(...restData.data.items);
						});
					}, time);
				}
			}
		});
	}

	showCompareRolesDialog() {
		this.isShowCompareRoles = true;
		this.compareRoleResult.roles = [];
		this.compareRoleResult.permissions = [];
	}

	// 比對結果
	searchCompareResult() {
		EventBus.$emit('show_mask', null);
		PermissionApi.getComparePermissionsResult(this.compareRolesFilter)
			.then((res) => {
				this.compareRoleResult = res.data;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	getMenui18nTextAsString(key: string) {
		return this.$t('Menu.' + key).toString();
	}

	addRoles() {
		this.editedItem = {} as Model.IUpdateRoleRequest;
		this.editedItem.roleID = -1;
		this.editedItem.roleName = '';
		this.editedItem.sort = 999;

		this.dialogEditRoles = true;
	}

	editRole(item: Model.IRole) {
		this.editedItem = { ...item };
		this.dialogEditRoles = true;
	}
	editRoleSuccess() {
		EventBus.$emit(`successfully-modified`);
		this.refresh();
		this.dialogEditRoles = false;
	}
	editPermission(item: Model.IRole) {
		this.editedItem = { ...item };
		this.dialogEditPermissions = true;
	}
	editRolePermissionSuccess() {
		EventBus.$emit(`successfully-modified`);
		this.refresh();
		this.dialogEditPermissions = false;
	}

	deleteItem(item: Model.IRole) {
		this.deleteIndex = item.roleID;
		this.showNotificationWarning = true;
		this.$notify({
			group: 'confirm',
			type: 'warn',
			duration: -1,
			speed: 10,
		});
	}

	confirmedAction() {
		EventBus.$emit('show_mask', null);

		RolesApi.deleteRole(this.deleteIndex)
			.then(() => {
				EventBus.$emit(`successfully-modified`);
				this.showNotificationWarning = false;
				this.refresh();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});

		this.showNotificationWarning = false;
	}

	showMappingUserAccountsByRoleID(myRoleID: number) {
		const myFilter: any = {
			querySqlID: 900030,
			roleID: myRoleID,
		};

		const myConfig: any = {
			maxWidth: 800,
		};

		EventBus.$emit(`globalShowCustomizedColumnsDisplayPopupDialog`, myFilter, myConfig);
	}

	showEachRoleIsEnablePermission(myPermissionID: number) {
		const myFilter: any = {
			querySqlID: 900185,
			permissionID: myPermissionID,
		};

		const myConfig: any = {
			maxWidth: 600,
			isShowCloseButton: true,
		};

		EventBus.$emit(`globalShowCustomizedColumnsDisplayPopupDialog`, myFilter, myConfig);
	}

	canceledAction() {
		this.showNotificationWarning = false;
	}

	mounted() {
		this.refresh();

		DropdownApi.getRoles().then((res) => {
			this.roleDropdownItems = res;
		});
	}
}
</script>

<style lang="scss">
.dialog-edit-permissions {
	background: linear-gradient(249deg, rgba(158, 169, 169, 0.9) 50%, rgba(155, 163, 158, 0.9) 82%);
	background: -moz-linear-gradient(249deg, rgba(158, 169, 169, 0.9) 50%, rgba(155, 163, 158, 0.9) 82%);
	background: -webkit-linear-gradient(249deg, rgba(158, 169, 169, 0.9) 50%, rgba(155, 163, 158, 0.9) 82%);
	background: -o-linear-gradient(249deg, rgba(158, 169, 169, 0.9) 50%, rgba(155, 163, 158, 0.9) 82%);
	min-width: 80% !important;
	max-width: 80vh !important;
	min-height: 75% !important;
}

.permissionCellSmall {
	min-width: 120px !important;
	max-width: 120px !important;
	width: 120px !important;
}
</style>
