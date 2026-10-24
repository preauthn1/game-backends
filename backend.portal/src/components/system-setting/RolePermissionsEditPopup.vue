<template>
	<div>
		<div v-show="isMounted">
			<v-card-title class="grey darken-3 title">
				<div style="display: block; flex-basis: 100%">
					<span> {{ $t('TableHeader.__role') }}{{ $t('TableHeader.__permission') }} </span>

					<span class="f14 ml-2 link-text-white"> {{ propsData.roleName }} ({{ propsData.roleID }}) </span>

					<div style="float: right; display: inline-block" class="right">
						<span style="margin-left: 20px; color: #f39200; font-size: 16px; font-weight: 700">
							<v-icon dark v-bind="attrs" v-on="on" style="font-size: 20px; color: #f39200"> error </v-icon>
							<span style="vertical-align: middle; font-size: 14px">
								{{ hintMessage }}
							</span>
						</span>

						<v-btn color="grey " @click="$emit('close-update-role-permissions')" class="btn ml-2 mr-2">
							<v-icon left>mdi-cancel</v-icon>
							{{ $t('Button.__cancel') }}
						</v-btn>

						<v-btn color="green darken-1" @click="save" class="btn">
							<v-icon left>mdi-check-circle</v-icon>
							{{ $t('Button.__submit') }}
						</v-btn>
					</div>
				</div>
			</v-card-title>
			<v-card-text>
				<div style="height: 700px; overflow-y: scroll; padding-bottom: 60px" v-if="isLoadTable">
					<v-data-iterator :items="nestedPermissions" :items-per-page.sync="itemsPerPage" hide-default-footer :footer-props="{ itemsPerPageOptions }">
						<template v-slot:default="props">
							<v-row>
								<v-col v-for="item in props.items" :key="item.permissionID" cols="12" sm="6" md="4" lg="3">
									<v-card>
										<!-- {{ checkdPermissions[item.permissionID] }} -->
										<v-treeview v-model="checkdPermissions[item.permissionID]" open-all class="tttest" :items="permissionChild[item.permissionID]" selectable>
											<template slot="label" slot-scope="props">
												<span :title="props.item.id + '／' + props.item.token">
													{{ props.item.name }}
												</span>

												<div style="display: inline-block; min-width: 50px" class="hoverShowDetail">
													<v-icon small class="detail-icon ml-2" color="yellow lighten-1" @click="query_Roles_AssignThisPermission(item.permissionID)"> mdi-magnify </v-icon>

													<v-icon small class="detail-icon ml-1" color="grey lighten-1" @click="query_Users_AssignThisPermission(item.permissionID)"> mdi-magnify </v-icon>
												</div>
											</template>
										</v-treeview>
									</v-card>
								</v-col>
							</v-row>
						</template>
					</v-data-iterator>
				</div>
			</v-card-text>
		</div>
		<v-overlay :absolute="true" :value="true" v-show="!isMounted">
			<!-- <v-progress-circular class="loading" :size="70" :width="7" indeterminate></v-progress-circular> -->
			<div class="loader"></div>
		</v-overlay>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Prop, Watch } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import PermissionApi from '@/api/commons/permission';
import RolesApi from '@/api/system-setting/roles';
import * as Model from '@/models/interfaces/system-setting/roles';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import { VTreeview } from 'vuetify/lib';
import { nextTick } from 'vue/types/umd';

@Component
export default class EditPermissionsPopup extends Vue {
	@Prop(Object) readonly propsData!: Model.IRole;
	itemsPerPageOptions: any = [30];
	itemsPerPage: number = 12;
	checkdPermissions: any = [];
	nestedPermissions: PermissionModel.INestedPermission[] = [];
	allPermissions: PermissionModel.IAllPermission[] = [];
	rolePermissions: number[] = [];
	permissionChild: any = {};
	isMounted: boolean = false;
	isLoadTable: boolean = true;
	hintMessage: string = '修改權限後，需請使用者 [重新登入]';

	getChild(item: PermissionModel.INestedPermission) {
		const data: Model.IMenuRootObject[] = [];
		data.push(<Model.IMenuRootObject>{
			id: item.permissionID,
			name: item.displayName,
			children: item.childPermissions.map((element: PermissionModel.INestedPermission) => {
				return <Model.IChild>{
					id: element.permissionID,
					name: element.displayName,
					token: [REDACTED],
				};
			}),
		});
		return data;
	}
	mounted() {
		PermissionApi.getPermissionsNested()
			.then((res) => {
				this.nestedPermissions = res.data.nestedPermissions;
				this.allPermissions = res.data.allPermissions;
				this.nestedPermissions.forEach((item: any) => {
					this.permissionChild[item.permissionID] = [
						<Model.IMenuRootObject>{
							id: item.permissionID,
							// name: item.displayName,
							name: this.$t(`Menu.${item.token}`),
							children: item.childPermissions.map((element: PermissionModel.INestedPermission) => {
								return <Model.IChild>{
									id: element.permissionID,
									permissionID: element.permissionID,
									name: this.$t(`Menu.${element.token}`),
									token: [REDACTED],
									children: element.childPermissions.map((funcElement: PermissionModel.INestedPermission) => {
										return <Model.IChild>{
											id: funcElement.permissionID,
											permissionID: funcElement.permissionID,
											name: this.$t(`Menu.${funcElement.token}`),
											token: [REDACTED],
										};
									}),
								};
							}),
						},
					];
				});
				RolesApi.getRolePermissions(this.propsData.roleID).then((role) => {
					this.rolePermissions = role.data.permissions;
					const ids = role.data.permissions;
					this.nestedPermissions.forEach((parent) => {
						if (parent.childPermissions && parent.childPermissions.length > 0) {
							if (ids.indexOf(parent.permissionID) > -1) {
								ids.splice(ids.indexOf(parent.permissionID), 1);
							}
						}
						parent.childPermissions.forEach((page: any) => {
							if (page.childPermissions && page.childPermissions.length > 0) {
								if (ids.indexOf(page.permissionID) > -1) {
									ids.splice(ids.indexOf(page.permissionID), 1);
								}
							}
						});
					});

					ids.forEach((id: any) => {
						this.nestedPermissions.forEach((parent: any) => {
							if (parent.permissionID === id) {
								if (this.checkdPermissions[parent.permissionID]) {
									this.checkdPermissions[parent.permissionID].push(id);
								} else {
									this.checkdPermissions[parent.permissionID] = [id];
								}
								return;
							} else {
								if (!parent.childPermissions) {
									return;
								}

								parent.childPermissions.forEach((child: any) => {
									if (child.permissionID === id) {
										if (this.checkdPermissions[parent.permissionID]) {
											this.checkdPermissions[parent.permissionID].push(id);
										} else {
											this.checkdPermissions[parent.permissionID] = [id];
										}
										return;
									}
									child.childPermissions.forEach((func: any) => {
										if (func.permissionID === id) {
											if (this.checkdPermissions[parent.permissionID]) {
												this.checkdPermissions[parent.permissionID].push(id);
											} else {
												this.checkdPermissions[parent.permissionID] = [id];
											}
											return;
										}
									});
								});
							}
						});
					});
					this.isMounted = true;
				});
			})
			.finally(() => {
				// 不知道為何checkbox的勾勾沒有亮起來 重新刷新畫面會解決此問題
				EventBus.$emit('show_mask', null);
				setTimeout(() => {
					this.isLoadTable = false;
					this.$nextTick(() => {
						this.isLoadTable = true;
						EventBus.$emit('hide_mask', null);
					});
				}, 1000);
			});
	}

	query_Roles_AssignThisPermission(myPermissionID: number) {
		const myFilter: any = {
			querySqlID: 900022,
			permissionID: myPermissionID,
		};

		const config: any = {
			maxWidth: 800,
			isShowCloseButton: true,
		};

		EventBus.$emit(`globalShowCustomizedColumnsDisplayPopupDialog`, myFilter, config);
	}

	query_Users_AssignThisPermission(myPermissionID: number) {
		const myFilter: any = {
			querySqlID: 900023,
			permissionID: myPermissionID,
		};

		const config: any = {
			maxWidth: 800,
			isShowCloseButton: true,
		};

		EventBus.$emit(`globalShowCustomizedColumnsDisplayPopupDialog`, myFilter, config);
	}

	save() {
		const self = this;
		const allCheckedPermissions: number[] = [];
		self.nestedPermissions.forEach((item: any) => {
			if (self.checkdPermissions[item.permissionID]) {
				if (self.checkdPermissions[item.permissionID].length > 0) {
					allCheckedPermissions.push(item.permissionID);
				}
				self.checkdPermissions[item.permissionID].forEach((permissionID: number) => {
					if (allCheckedPermissions.indexOf(permissionID) < 0) {
						allCheckedPermissions.push(permissionID);
					}
				});
			}
			item.childPermissions.forEach((pageElement: any) => {
				pageElement.childPermissions.forEach((funcElement: any) => {
					if (allCheckedPermissions.indexOf(funcElement.permissionID) > -1) {
						if (allCheckedPermissions.indexOf(funcElement.permissionID) < 0) {
							allCheckedPermissions.push(funcElement.permissionID);
						}
						if (allCheckedPermissions.indexOf(pageElement.permissionID) < 0) {
							allCheckedPermissions.push(pageElement.permissionID);
						}
					}
				});
			});
		});

		EventBus.$emit('show_mask', null);

		RolesApi.updateRolePermissions(<Model.IUpdateRolePermissionsRequest>{
			roleID: this.propsData.roleID,
			permissions: allCheckedPermissions,
		})
			.then(() => {
				this.$emit('update-role-permissions-success');
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}
}
</script>

<style lang="scss" scoped>
.v-input {
	max-width: 35% !important;
}
.btn {
	background-color: rgb(96, 74, 53);
}
.title {
	font-family: 微軟正黑體, arial !important;
	margin: 0 0 2% 0;
}

.detail-icon {
	visibility: hidden;
}

.hoverShowDetail:hover .detail-icon {
	visibility: initial;
}
</style>
