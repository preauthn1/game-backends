<template>
	<v-app v-if="renderComponent" class="app">
		<v-expansion-panels v-model="panel" multiple>
			<v-expansion-panel>
				<v-expansion-panel-header>
					<!-- <template v-slot:actions> <v-icon color="primary">$expand</v-icon> </template> -->
					<template v-slot:actions>
						<v-icon color="primary">filter_list</v-icon>
					</template>
					{{$t(`Filter.__filter`)}}
				</v-expansion-panel-header>
				<v-expansion-panel-content>
					<v-flex>
						<v-row>
							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete
										clearable
										:items="gamesDropdownItems"
										v-model="searchData.gameID"
										:label="$t('TableHeader.__game')"
									></v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<!-- <div class="item right"> -->
								<div class="item">
									<v-btn color="yellow darken-1" text class="btn search" @click.stop="search">
										<v-icon left>search</v-icon>
										{{$t('Button.__search')}}
									</v-btn>&#9;
									<v-btn color="pink lighten-3" text class="btn search" @click.stop="clear">
										<v-icon left>clear</v-icon>
										{{$t('Button.__reset')}}
									</v-btn>&#9;
								</div>
							</v-col>
						</v-row>
					</v-flex>
				</v-expansion-panel-content>
			</v-expansion-panel>
		</v-expansion-panels>

		<v-data-table
			:items="result"
			:headers="headers"
			id="table"
			:disable-sort="true"
			:page="pageIndex"
			:items-per-page="itemsPerPage"
			:server-items-length="totalCount"
			:footer-props="footerProps"
			@update:page="changePage"
			@update:items-per-page="getItemPerPage"
		>
			<template v-slot:top>
				<v-toolbar>
					<!-- <v-divider class="mx-4" inset vertical></v-divider> -->
					<v-spacer></v-spacer>

					<v-dialog v-model="dialog" max-width="500px">
						<template v-slot:activator="{ on }">
							<v-btn icon dark v-on="on" @click="addItem" v-show="hasPermissionCreate">
								<v-icon>add</v-icon>
							</v-btn>
						</template>
						<v-card>
							<v-card-title class="grey darken-2">{{ formTitle }}</v-card-title>
							<v-card-text>
								<v-flex>
									<v-row v-show="editedIndex === -1">
										<v-autocomplete
											:items="gamesDropdownItems"
											v-model="editedItem.gameID"
											:label="$t('TableHeader.__game')"
											class="required"
										></v-autocomplete>
									</v-row>
									<v-row v-show="editedIndex !== -1">
										<v-text-field
											v-model="editedItem.serverID"
											:label="$t('TableHeader.__server')"
											:disabled="editedIndex !== -1"
										></v-text-field>
									</v-row>
									<v-row>
										<v-autocomplete
											:items="serversDropdownItems"
											v-model="editedItem.serverID"
											:label="$t('TableHeader.__server')"
											class="required"
											:disabled="editedIndex !== -1"
										></v-autocomplete>
									</v-row>
									<v-row>
										<v-select
											class="required"
											attach
											chips
											:label="$t('TableHeader.__minLevel')"
											v-model="editedItem.minLevel"
											:items="[1,2,3,4,5,6,7,8,9,10]"
										></v-select>
									</v-row>
									<v-row>
										<v-select
											class="required"
											attach
											chips
											:label="$t('TableHeader.__maxLevel')"
											v-model="editedItem.maxLevel"
											:items="[1,2,3,4,5,6,7,8,9,10]"
										></v-select>
									</v-row>
									<v-row>
										<v-text-field
											v-model="editedItem.beginTime"
											:label="`${$t('TableHeader.__beginTime')} 00:00`"
											class="required"
											clearable
										></v-text-field>
									</v-row>
									<v-row>
										<v-text-field
											v-model="editedItem.endTime"
											:label="`${$t('TableHeader.__endTime')} 23:59`"
											class="required"
											clearable
										></v-text-field>
									</v-row>
									<v-row>
										<v-text-field
											type="number"
											v-model="minRobotCount"
											:label="$t('TableHeader.__minRobotCount')"
											:rules="[ value => (!!value && Number(value) > 0) || $t('Validation.__numberRules')]"
											required
											class="number-text-field required"
											prepend-inner-icon="mdi-minus"
											append-icon="mdi-plus"
											@click:prepend-inner="minRobotCountMinusOrPlus(false)"
											@click:append="minRobotCountMinusOrPlus(true)"
										></v-text-field>
									</v-row>
									<v-row>
										<v-text-field
											type="number"
											v-model="maxRobotCount"
											:label="$t('TableHeader.__maxRobotCount')"
											:rules="[value => (!!value && Number(value) > 0) || $t('Validation.__numberRules')]"
											required
											class="number-text-field required"
											prepend-inner-icon="mdi-minus"
											append-icon="mdi-plus"
											@click:prepend-inner="maxRobotCountMinusOrPlus(false)"
											@click:append="maxRobotCountMinusOrPlus(true)"
										></v-text-field>
									</v-row>
								</v-flex>
							</v-card-text>
							<v-card-actions>
								<v-spacer></v-spacer>
								<v-btn text @click="close" class="btn">
									<v-icon left>mdi-cancel</v-icon>
									{{$t('Button.__cancel')}}
								</v-btn>
								<v-btn color="yellow darken-1" text @click="save" class="btn">
									<v-icon left>mdi-check-circle</v-icon>
									{{$t('Button.__submit')}}
								</v-btn>
							</v-card-actions>
						</v-card>
					</v-dialog>
				</v-toolbar>
			</template>
			<template v-slot:item.action="{ item }">
				<v-icon v-if="hasPermissionUpdate" small class="mr-2" @click="editItem(item)">edit</v-icon>
				<v-icon v-if="hasPermissionDelete" small @click="deleteItem(item)">delete</v-icon>
			</template>
		</v-data-table>
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
import * as Model from '@/models/interfaces/system-setting/servers';
import ServersApi from '@/api/system-setting/servers';
import DropdownApi, { convertLang } from '@/api/commons/dropdown';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
// import Validation from '@/utilities/validation';
import NotificationWarning from '@/components/notification/NotificationWarning.vue';

import * as PermissionModel from '@/models/interfaces/commons/permission';
import { State, Action, Getter, namespace } from 'vuex-class';
const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';

@Component({ components: { NotificationWarning } })
export default class ServersRobotConfig extends Vue {
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];
	@authModule.State('defaultLangTimes') defaultLangTimes!: number;
	dialog: boolean = false;
	editedIndex: number = -1;
	renderComponent: boolean = true;
	panel: number[] = [0];
	totalCount: number = 0;
	pageIndex: number = 1;
	footerProps: any = { 'items-per-page-options': [10, 30, 50, 100] };
	itemsPerPage: number = 10;
	searchData: Model.IServerRobotConfigRequest = {} as Model.IServerRobotConfigRequest;
	gamesDropdownItemsTemp: DropdownModel.IDropdownItems[] = [];
	gamesDropdownItems: DropdownModel.IDropdownItems[] = [];
	serversDropdownItems: DropdownModel.IDropdownItems[] = [];
	result: Model.IServerRobotConfigReponse[] = [];
	get hasPermissionCreate() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSettingServerRobotConfig_Create) >= 0;
	}
	get hasPermissionUpdate() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSettingServerRobotConfig_Update) >= 0;
	}
	get hasPermissionDelete() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSettingServerRobotConfig_Delete) >= 0;
	}

	get headers() {
		const data = [
			{ text: this.$t('TableHeader.__serverID'), value: 'serverID', sortable: true },
			{ text: this.$t('TableHeader.__serverName'), value: 'serverName' },
			{ text: this.$t('TableHeader.__minLevel'), value: 'minLevel' },
			{ text: this.$t('TableHeader.__maxLevel'), value: 'maxLevel' },
			{ text: this.$t('TableHeader.__beginTime'), value: 'beginTime' },
			{ text: this.$t('TableHeader.__endTime'), value: 'endTime' },
			{ text: this.$t('TableHeader.__minRobotCount'), value: 'minRobotCount' },
			{ text: this.$t('TableHeader.__maxRobotCount'), value: 'maxRobotCount' },
		];

		if (this.hasPermissionUpdate || this.hasPermissionDelete) {
			data.splice(0, 0, { text: this.$t('TableHeader.__lock'), value: 'action', sortable: false });
		}

		return data;
	}

	get orderByColumn() {
		return [
			{ text: this.$t('TableHeader.__serverID'), value: 'serverID', sortable: true },
			{ text: this.$t('TableHeader.__serverName'), value: 'serverName' },
			{ text: this.$t('TableHeader.__minLevel'), value: 'minLevel' },
			{ text: this.$t('TableHeader.__maxLevel'), value: 'maxLevel' },
			{ text: this.$t('TableHeader.__beginTime'), value: 'beginTime' },
			{ text: this.$t('TableHeader.__endTime'), value: 'endTime' },
			{ text: this.$t('TableHeader.__minRobotCount'), value: 'minRobotCount' },
			{ text: this.$t('TableHeader.__maxRobotCount'), value: 'maxRobotCount' },
		];
	}
	editedItem: Model.IUpdateServerRobotConfigRequest = {} as Model.IUpdateServerRobotConfigRequest;
	minRobotCount: number = 0;
	maxRobotCount: number = 0;
	deleteIndex: number = -1;
	showNotificationWarning: boolean = false;

	get formTitle() {
		return this.editedIndex === -1 ? this.$t('Popup.__create') : this.$t('Popup.__update');
	}

	@Watch('editedItem.gameID', { immediate: true, deep: true })
	onGameIDChange() {
		DropdownApi.getServers(!this.editedItem.gameID ? null : Number(this.editedItem.gameID)).then((res) => {
			this.serversDropdownItems = res;
		});
	}

	@Watch('defaultLangTimes')
	onDefaultLangChanged() {
		this.gamesDropdownItems = convertLang(this.gamesDropdownItemsTemp);
	}

	minRobotCountMinusOrPlus(isPlus: boolean) {
		if (!this.minRobotCount) {
			this.minRobotCount = 0;
		}
		this.minRobotCount = isPlus ? Number(this.minRobotCount) + 1 : Number(this.minRobotCount) - 1;
	}
	maxRobotCountMinusOrPlus(isPlus: boolean) {
		if (!this.maxRobotCount) {
			this.maxRobotCount = 0;
		}
		this.maxRobotCount = isPlus ? Number(this.maxRobotCount) + 1 : Number(this.maxRobotCount) - 1;
	}
	changePage(index: number = 1) {
		this.pageIndex = index;
		if (this.result.length > 0) {
			this.search();
		}
	}

	getItemPerPage(val: number = 10) {
		this.itemsPerPage = val;
		if (this.result.length > 0) {
			this.search();
		}
	}
	search() {
		const request = this.searchData;
		request.pageIndex = this.pageIndex;
		request.pageSize = this.itemsPerPage;
		request.orderByColumn = this.searchData.orderByColumn;
		request.isASC = this.searchData.isASC;
		ServersApi.getServersRobotConfig(<Model.IServerRobotConfigRequest>request).then((res) => {
			this.result = res.data.items;
			this.totalCount = res.data.totalCount;
		});
	}
	mounted() {
		this.init();
		this.search();
		this.registerReloadComponent();
	}

	init() {
		DropdownApi.getGamesOriginal().then((res) => {
			this.gamesDropdownItemsTemp = res;
			this.gamesDropdownItems = convertLang(this.gamesDropdownItemsTemp);
			this.searchData.orderByColumn = 'serverID';
		});
		DropdownApi.getServers().then((res) => {
			this.serversDropdownItems = res;
		});
	}

	clear() {
		this.searchData = {} as Model.IServerRobotConfigRequest;
		this.searchData.orderByColumn = 'serverID';
	}

	registerReloadComponent() {
		EventBus.$on(`reload-${this.$router.currentRoute.path}`, (info: any) => {
			this.renderComponent = false;
			this.$nextTick(() => {
				this.clear();
				this.result = [];
				this.renderComponent = true;
			});
		});
	}

	addItem() {
		this.editedIndex = -1;
		this.dialog = true;
		this.editedItem = {} as Model.IUpdateServerRobotConfigRequest;
		this.minRobotCount = 0;
		this.maxRobotCount = 0;
	}
	editItem(robot: Model.IServerRobotConfigReponse) {
		this.editedIndex = robot.serverID;
		this.dialog = true;
		ServersApi.getServersRobotConfigDetail(robot.serverID).then((res) => {
			this.editedItem = res.data;
			this.editedItem.serverID = res.data.serverID.toString();
			this.minRobotCount = res.data.minRobotCount;
			this.maxRobotCount = res.data.maxRobotCount;
		});
	}
	deleteItem(robot: Model.IServerRobotConfigReponse) {
		this.deleteIndex = robot.serverID;
		this.showNotificationWarning = true;
		this.$notify({
			group: 'confirm',
			type: 'warn',
			duration: -1,
			speed: 10,
		});
	}
	save() {
		const request = this.editedItem;
		request.serverID = Number(this.editedItem.serverID);
		request.minRobotCount = this.minRobotCount;
		request.maxRobotCount = this.maxRobotCount;

		if (this.editedIndex === -1) {
			ServersApi.addServersRobotConfig(request).then(() => {
				EventBus.$emit(`successfully-modified`);
				this.close();
				this.search();
			});
		} else {
			ServersApi.updateServersRobotConfig(request).then(() => {
				EventBus.$emit(`successfully-modified`);
				this.close();
				this.search();
			});
		}
	}
	close() {
		this.dialog = false;
	}
	confirmedAction() {
		ServersApi.deleteServersRobotConfig(this.deleteIndex).then(() => {
			EventBus.$emit(`successfully-modified`);
			this.close();
			this.search();
		});

		this.showNotificationWarning = false;
	}
	canceledAction() {
		this.showNotificationWarning = false;
	}
}
</script>
<style lang="scss" scoped>
.v-expansion-panel--active > .v-expansion-panel-header {
	min-height: 1% !important;
	max-height: 1% !important;
}
.item {
	max-width: 350px;
	// width: 100%;
	// display: block;
	max-height: 20px !important;
	margin-top: 2px !important;
	margin-bottom: 3px !important;
	&.right {
		text-align: right;
	}
}
.search {
	max-width: 80px;
	margin-top: 15px !important;
}
.mx-datepicker {
	margin-top: 15px !important;
	width: 100% !important;
	width: 100% !important;
}

.container--fluid {
	margin-right: 5px !important;
}
</style>