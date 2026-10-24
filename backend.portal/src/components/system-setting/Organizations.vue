<template>
	<v-app v-if="renderComponent" class="app">
		<v-data-table :headers="headers" id="table" :items="organizationData" :search="search" multi-sort>
			<template v-slot:top>
				<v-toolbar>
					<v-toolbar-title>
						<h4>
							{{ $t('TableHeader.__organization') }}
							<span class="link-text-grey f14 ml-4" @click="showAllOrganizationCustomizedSettings()"> 查看Customized配置 </span>
						</h4>
					</v-toolbar-title>
					<v-divider class="mx-4" inset vertical></v-divider>
					<v-spacer></v-spacer>
					<v-card-title>
						<v-text-field
							v-model="search"
							append-icon="search"
							:label="`${$t('TableHeader.__organizationID')} / ${$t('TableHeader.__organizationName')}`"
							single-line
							hide-details
							clearable
						>
						</v-text-field>

						<v-btn icon dark @click="refresh" class="ml-4">
							<v-icon>refresh</v-icon>
						</v-btn>
						<v-btn icon dark v-on="on" @click="addItem" v-show="hasPermissionCreate">
							<v-icon>add</v-icon>
						</v-btn>
					</v-card-title>
				</v-toolbar>
			</template>
			<template v-slot:item.lang="{ item }">{{ $t(`Lang.${item.lang}`) }}</template>
			<template v-slot:item.status="{ item }">
				<v-chip v-show="status.PermanentDelete === item.status" class="ma-2"> {{ $t(`Status.${status[item.status]}`) }}</v-chip>
				<v-chip v-show="status.Disable === item.status" class="ma-2" color="red"> {{ $t(`Status.${status[item.status]}`) }}</v-chip>
				<v-chip v-show="status.Enable === item.status" class="ma-2" color="green"> {{ $t(`Status.${status[item.status]}`) }}</v-chip>
			</template>

			<template v-slot:item.organizationName="{ item }">
				<div>{{ item.organizationName }}</div>
				<div class="sub-title-text">{{ item.memo }}</div>
			</template>

			<template v-slot:item.organizationType="{ item }">
				{{ $t(`OrganizationType.${organizationType[item.organizationType]}`) }}
			</template>

			<template v-slot:item.usersTotalCount="{ item }">
				<div class="link-text" @click="showUserCountDialog(item.organizationID)">
					{{ item.usersTotalCount }}
				</div>
			</template>

			<template v-slot:item.createTime="{ item }">
				{{ item.createTime | timeWithAddTimezone(userProfile.timeZone) }}
			</template>

			<template v-slot:item.updateTime="{ item }">
				{{ item.updateTime | timeWithAddTimezone(userProfile.timeZone) }}
			</template>

			<template v-slot:item.action="{ item }">
				<v-icon v-if="hasPermissionUpdate" small class="mr-2" title="Customized Settings" @click="showCustomizedSettings(item)">settings</v-icon>
				<v-icon v-if="hasPermissionUpdate" small class="mr-2" @click="editItem(item)">edit</v-icon>
				<v-icon v-if="hasPermissionDelete" small @click="deleteItem(item)">delete</v-icon>
			</template>
		</v-data-table>

		<v-dialog v-model="isShowEditDialog" max-width="500px" persistent>
			<v-card>
				<v-card-title class="grey darken-2">
					{{ formTitle }}
					<div class="top-right-item" @click="close">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>
				<v-card-text>
					<v-flex>
						<v-row>
							<v-text-field
								class="required"
								v-model="editedItem.organizationName"
								:label="$t('TableHeader.__organizationName')"
								autofocus
								:rules="[(value) => !!value || $t('Validation.__requiredRules')]"
								required
								clearable
							></v-text-field>
						</v-row>
						<v-row>
							<v-text-field
								class="required"
								:disabled="editedIndex !== -1"
								v-model="editedItem.organizationCode"
								:label="$t('TableHeader.__organizationCode')"
								:rules="[(value) => !!value || $t('Validation.__requiredRules')]"
								required
								clearable
							></v-text-field>
						</v-row>
						<v-row>
							<v-text-field v-model="editedItem.memo" :label="$t('TableHeader.__memo')" clearable></v-text-field>
						</v-row>
						<v-row>
							<v-select attach chips :label="$t('TableHeader.__lang')" class="required" v-model="editedItem.lang" :items="langSelectItems"></v-select>
						</v-row>
						<v-row>
							<v-radio-group row v-model="editedItem.status">
								<v-radio :label="$t('Status.Enable')" :value="status.Enable"></v-radio>
								<v-radio :label="$t('Status.Disable')" :value="status.Disable"></v-radio>
								<v-radio disabled :label="$t('Status.PermanentDelete')" :value="status.PermanentDelete"></v-radio>
							</v-radio-group>
						</v-row>
						<v-row>
							<v-autocomplete
								clearable
								deletable-chips
								v-model="editedItem.merchants"
								class="merchantDropdownItems"
								:items="merchantsDropdownItems"
								dense
								chips
								small-chips
								:label="$t('TableHeader.__merchant')"
								multiple
								solo
								outlined
							>
							</v-autocomplete>
						</v-row>
						<v-row>
							<v-autocomplete
								clearable
								deletable-chips
								v-model="editedItem.roles"
								:items="rolesDropdownItems"
								dense
								chips
								small-chips
								:label="$t('TableHeader.__role')"
								multiple
								solo
								outlined
							></v-autocomplete>
						</v-row>
					</v-flex>
				</v-card-text>
				<v-card-actions>
					<v-spacer></v-spacer>
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

		<v-dialog v-model="customizedSettingsDialog" max-width="900px" scrollable>
			<v-card>
				<v-card-title class="grey darken-2">
					Customized Settings
					<span class="f12 yellow-text ml-4">{{ customizedSettingsOrganizationName }} ({{ customizedSettingsOrganizationID }})</span>
					<div class="top-right-item" @click="closeCustomizedSettings">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>
				<v-card-text>
					<v-data-table class="customized-settings-table" :headers="customizedSettingHeaders" :items="customizedSettings" hide-default-footer>
						<template v-slot:item.settingValue="{ item }">
							<v-textarea
								v-if="isCustomizedSettingEditing(item.settingKey)"
								v-model="item.settingValue"
								auto-grow
								rows="1"
								hide-details
								style="width: 400px; min-width: 400px; max-width: 400px; background: #333; padding: 0px 3px; line-height: 22px"
							>
							</v-textarea>
							<div v-else style="width: 400px; min-width: 400px; max-width: 400px; white-space: pre-wrap; overflow-wrap: break-word; word-break: normal">{{ item.settingValue }}</div>
						</template>
						<template v-slot:item.action="{ item }">
							<template v-if="isCustomizedSettingEditing(item.settingKey)">
								<v-btn color="blue" text class="btn mr-2" @click="saveCustomizedSetting(item)">
									{{ $t('Button.__save') }}
								</v-btn>
								<v-btn color="grey" text class="btn" @click="cancelCustomizedSettingEdit(item)">
									{{ $t('Button.__cancel') }}
								</v-btn>
							</template>
							<v-btn v-else-if="hasPermissionUpdate" text class="btn" :disabled="customizedSettingEditingKey !== null" @click="editCustomizedSetting(item)">
								{{ $t('Button.__edit') }}
							</v-btn>
						</template>
					</v-data-table>
				</v-card-text>
			</v-card>
		</v-dialog>

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
import * as Model from '@/models/interfaces/system-setting/organizations';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import OrganizationsApi from '@/api/system-setting/organizations';
import DropdownApi from '@/api/commons/dropdown';
import { Locale, Status } from '@/models/status/common';
import { OrganizationTypeEnum } from '@/models/status/organizations';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import NotificationWarning from '@/components/notification/NotificationWarning.vue';
import { pageData, time } from '@/utilities/api-handler.lib';
// import Validation from '@/utilities/validation';
import { datetimeMixin } from '@/utilities/datetime-format';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import { State, Action, Getter, namespace } from 'vuex-class';
const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';
@Component({ components: { NotificationWarning }, mixins: [commonDisplayFiltersMixin, datetimeMixin] })
export default class Organizations extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];
	status: typeof Status = Status;
	organizationType: typeof OrganizationTypeEnum = OrganizationTypeEnum;
	renderComponent: boolean = true;
	isShowEditDialog: boolean = false;
	search: string = '';
	organizationData: Model.IOrganizationsData[] = [];
	editedIndex: number = -1;
	editedItem: Model.IOrganizationsRequest = {} as Model.IOrganizationsRequest;
	deleteIndex: number = -1;
	showNotificationWarning: boolean = false;
	customizedSettingsDialog: boolean = false;
	customizedSettingsOrganizationID: number = 0;
	customizedSettingsOrganizationName: string = '';
	customizedSettings: Model.IOrganizationCustomizedSetting[] = [];
	customizedSettingEditingKey: number | null = null;
	customizedSettingOriginalValue: string = '';

	get langSelectItems() {
		return [
			{ value: Locale.ZHCN, text: this.$t(`Lang.${Locale.ZHCN}`) },
			{ value: Locale.ZHTW, text: this.$t(`Lang.${Locale.ZHTW}`) },
			{ value: Locale.ENUS, text: this.$t(`Lang.${Locale.ENUS}`) },
		];
	}
	merchantsDropdownItems: DropdownModel.IDropdownItems[] = [];
	rolesDropdownItems: DropdownModel.IDropdownItems[] = [];

	get hasPermissionCreate() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSettingOrganizations_Create) >= 0;
	}
	get hasPermissionUpdate() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSettingOrganizations_Update) >= 0;
	}
	get hasPermissionDelete() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSettingOrganizations_Delete) >= 0;
	}
	get headers() {
		const data = [
			{ text: this.$t('TableHeader.__organizationID'), value: 'organizationID', sortable: true, width: '7%' },
			{ text: this.$t('TableHeader.__organizationName'), value: 'organizationName' },
			{ text: this.$t('TableHeader.__organizationCode'), value: 'organizationCode' },
			{ text: this.$t('TableHeader.__organizationType'), value: 'organizationType' },
			{ text: this.$t('TableHeader.__usersTotalCount'), value: 'usersTotalCount', align: 'center' },
			{ text: this.$t('TableHeader.__lang'), value: 'lang' },
			{ text: this.$t('TableHeader.__status'), value: 'status' },
			{ text: this.$t('TableHeader.__updateTime'), value: 'updateTime' },
			{ text: this.$t('TableHeader.__updateBy'), value: 'updateBy', width: '20%' },
		];
		if (this.hasPermissionUpdate || this.hasPermissionDelete) {
			data.splice(0, 0, { text: this.$t('TableHeader.__action'), value: 'action', sortable: false, width: '9%' });
		}
		return data;
	}

	get customizedSettingHeaders() {
		return [
			{ text: 'settingKey', value: 'settingKey', sortable: false },
			{ text: 'value', value: 'settingValue', sortable: false, width: '500px' },
			{ text: this.$t('TableHeader.__action'), value: 'action', sortable: false, width: '25%' },
		];
	}

	get formTitle() {
		return this.editedIndex === -1 ? this.$t('Popup.__create') : this.$t('Popup.__update');
	}

	refresh() {
		this.registerReloadComponent();

		EventBus.$emit('show_mask', null);
		OrganizationsApi.getOrganizations()
			.then((res) => {
				this.organizationData = res.data.items;
				const mod: number = res.data.totalCount % pageData.pageSize === 0 ? 0 : 1;
				if (res.data.totalCount > pageData.pageSize) {
					for (let i = 2; i <= res.data.totalCount / pageData.pageSize + mod; i++) {
						setTimeout(() => {
							OrganizationsApi.getOrganizations(i).then((restData) => {
								this.organizationData.push(...restData.data.items);
							});
						}, time);
					}
				}
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});

		this.editedItem.status = Status.Enable;
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

	showUserCountDialog(orgID: number) {
		const myFilter: any = {
			querySqlID: 900031,
			organizationID: orgID,
		};

		const myConfig: any = {
			maxWidth: 800,
		};

		EventBus.$emit(`globalShowCustomizedColumnsDisplayPopupDialog`, myFilter, myConfig);
	}

	showCustomizedSettings(item: Model.IOrganizationsData) {
		this.customizedSettingsOrganizationID = item.organizationID;
		this.customizedSettingsOrganizationName = item.organizationName;
		this.customizedSettingEditingKey = null;
		EventBus.$emit('show_mask', null);
		OrganizationsApi.getCustomizedSettings(item.organizationID)
			.then((res) => {
				this.customizedSettings = res.data;
				this.customizedSettingsDialog = true;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	isCustomizedSettingEditing(settingKey: number) {
		return this.customizedSettingEditingKey === settingKey;
	}

	editCustomizedSetting(item: Model.IOrganizationCustomizedSetting) {
		this.customizedSettingEditingKey = item.settingKey;
		this.customizedSettingOriginalValue = item.settingValue;
	}

	cancelCustomizedSettingEdit(item: Model.IOrganizationCustomizedSetting) {
		item.settingValue = this.customizedSettingOriginalValue;
		this.customizedSettingEditingKey = null;
	}

	saveCustomizedSetting(item: Model.IOrganizationCustomizedSetting) {
		const request: Model.IOrganizationCustomizedSettingUpdateRequest = {
			organizationID: this.customizedSettingsOrganizationID,
			settingKey: item.settingKey,
			settingValue: item.settingValue,
		};
		EventBus.$emit('show_mask', null);
		OrganizationsApi.updateCustomizedSetting(request)
			.then(() => {
				this.customizedSettingEditingKey = null;
				EventBus.$emit(`successfully-modified`);
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	closeCustomizedSettings() {
		this.customizedSettingsDialog = false;
		this.customizedSettingEditingKey = null;
	}

	addItem() {
		this.editedIndex = -1;
		DropdownApi.getMerchants().then((res) => {
			this.merchantsDropdownItems = res;
		});
		DropdownApi.getRoles().then((res) => {
			this.rolesDropdownItems = res;
		});
		this.editedItem = {} as Model.IOrganizationsRequest;
		this.editedItem.status = Status.Enable;
		this.isShowEditDialog = true;
	}
	editItem(item: Model.IOrganizationsData) {
		this.addItem();

		EventBus.$emit('show_mask', null);

		OrganizationsApi.getOrganization(item.organizationID)
			.then((res) => {
				this.editedItem.organizationID = res.data.organizationID;
				this.editedItem.organizationName = res.data.organizationName;
				this.editedItem.organizationCode = res.data.organizationCode;
				this.editedItem.memo = res.data.memo;
				this.editedItem.lang = res.data.lang;
				this.editedItem.status = res.data.status;
				this.editedItem.roles = res.data.roles.map((role) => role.toString());
				this.editedItem.merchants = res.data.merchants.map((merchant) => merchant.toString());

				this.editedItem = { ...this.editedItem };
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});

		this.editedIndex = this.organizationData.indexOf(item);
		this.isShowEditDialog = true;
	}

	close() {
		this.isShowEditDialog = false;
		setTimeout(() => {
			this.editedItem.status = Status.Enable;
			this.editedIndex = -1;
		}, 300);
	}

	save() {
		const request = <Model.IOrganizationsRequest>{ ...this.editedItem };
		if (this.editedItem.roles) {
			request.roles = this.editedItem.roles.map((role: string) => Number(role));
		}
		if (this.editedItem.merchants) {
			request.merchants = this.editedItem.merchants.map((merchant: string) => Number(merchant));
		}
		request.status = request.status;
		if (this.editedIndex > -1) {
			OrganizationsApi.updateOrganizations(<Model.IOrganizationsRequest>request).then(() => {
				EventBus.$emit(`successfully-modified`);
				this.close();
				this.refresh();
			});
		} else {
			OrganizationsApi.createOrganizations(<Model.IOrganizationsRequest>request).then(() => {
				EventBus.$emit(`successfully-modified`);
				this.close();
				this.refresh();
			});
		}
	}

	deleteItem(item: Model.IOrganizationsData) {
		this.deleteIndex = item.organizationID;
		this.showNotificationWarning = true;
		this.$notify({
			group: 'confirm',
			type: 'warn',
			duration: -1,
			speed: 10,
		});
	}

	confirmedAction() {
		OrganizationsApi.getOrganization(this.deleteIndex).then((res) => {
			const request = {} as Model.IOrganizationsRequest;
			request.organizationID = res.data.organizationID;
			request.organizationName = res.data.organizationName;
			request.memo = res.data.memo;
			request.lang = res.data.lang;
			request.status = Status.PermanentDelete;
			request.roles = res.data.roles;
			request.merchants = res.data.merchants;

			OrganizationsApi.updateOrganizations(<Model.IOrganizationsRequest>request).then(() => {
				EventBus.$emit(`successfully-modified`);
				this.close();
				this.refresh();
			});
		});

		this.showNotificationWarning = false;
	}

	showAllOrganizationCustomizedSettings() {
		const myFilter: any = {
			querySqlID: 900033,
		};

		const myConfig: any = {
			maxWidth: 800,
		};

		EventBus.$emit(`globalShowCustomizedColumnsDisplayPopupDialog`, myFilter, myConfig);
	}

	canceledAction() {
		this.showNotificationWarning = false;
	}
	mounted() {
		this.refresh();
	}
}
</script>

<style scoped>
.customized-settings-table ::v-deep tbody tr:hover:not(.v-data-table__expanded__content) {
	background: transparent !important;
}
</style>
