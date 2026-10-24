<template>
	<v-app v-if="renderComponent" class="app">

		<v-expansion-panels v-model="panel" multiple>
			<v-expansion-panel>

				<v-expansion-panel-content>
					<v-flex>
						<v-row>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete clearable :items="gamesDropdownItems" v-model="searchData.gameID"
										:label="$t('TableHeader.__game')"></v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item letter-size">

									<v-autocomplete clearable :items="currencySelectItems" class="letter-size"
										v-model="searchData.currency" :label="$t('TableHeader.__currency')">

										<template v-slot:item="data">

											<div class="letter-size-equal">
												<span>{{data.item.value}}</span>｜<span>{{data.item.text}}</span>
											</div>

										</template>

									</v-autocomplete>

								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete clearable :items="merchantsDropdownItems"
										v-model="searchData.assignMerchantID"
										:label="$t('TableHeader.__assignMerchantTotalCount')">
									</v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<!-- <div class="item right"> -->
								<div class="item search-row-padding">
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

		<v-data-table :headers="headers" id="table" :items="serversData" :search="searchKeyword"
			multi-sort>
			<template v-slot:top>
				<v-toolbar>
					<v-toolbar-title>
						<h4>服务器</h4>
					</v-toolbar-title>
					<v-divider class="mx-4" inset vertical></v-divider>
					<v-spacer></v-spacer>

					<v-card-title>
						<v-text-field v-model="searchKeyword" append-icon="search"
							:label="`${$t('TableHeader.__serverID')} / ${$t('TableHeader.__serverName')}`"
							single-line hide-details clearable></v-text-field>
					</v-card-title>

					<v-dialog v-model="dialogEditServer" max-width="500px">
						<template v-slot:activator="{ add, edit }">
							<v-btn icon dark @click="refresh">
								<v-icon>refresh</v-icon>
							</v-btn>
							<v-btn icon dark v-on="add" @click="addServer" v-show="hasPermissionCreate">
								<v-icon>add</v-icon>
							</v-btn>
						</template>
						<EditServerPopup v-if="dialogEditServer" :propsData="editedItem"
							@update-server-success="editServerSuccess"
							@close-update-server="dialogEditServer = false" />
					</v-dialog>
					<!-- <v-dialog v-model="dialogEditServer" max-width="500px">
						<template v-slot:activator="{ edit }"></template>
						<EditServerPopup
							v-if="dialogEditServer"
							:propsData="editedItem"
							@update-server-success="editServerSuccess"
							@close-update-server="dialogEditServer=false"
						/>
					</v-dialog>-->
					<v-dialog content-class="dialog-edit-merchants" v-model="dialogEditMerchants"
						max-width="500px">
						<template v-slot:activator="{ editMerchants }"></template>
						<EditServerMerchantsPopup v-if="dialogEditMerchants" :propsData="editedItem"
							@update-server-merchants-success="editServerMerchantsSuccess"
							@close-update-server-merchants="dialogEditMerchants = false" />
					</v-dialog>
				</v-toolbar>
			</template>
			<template v-slot:item.gameName="{ item }">
				<div :title="item.gameID">{{item.gameName}}</div>
			</template>
			<template
				v-slot:item.currency="{ item }">{{$t(`Currency.${currency[item.currency]}`)}}</template>
			<template v-slot:item.assignMerchantTotalCount="{ item }">
				{{ item.assignMerchantTotalCount }}
				<v-icon v-if="hasPermissionDispatch" small class="mr-2"
					@click="openDetail(item.serverID , item.currency)">
					edit</v-icon>
			</template>
			<template
				v-slot:item.lastUpdateTime="{ item }">{{ (item.updateTime || item.createTime) | timeWithAddTimezone(userProfile.timeZone) }}</template>
			<template v-slot:item.action="{ item }">
				<v-icon v-if="hasPermissionUpdate" small class="mr-2" @click="editServer(item)">edit
				</v-icon>
				<v-icon v-if="hasPermissionDelete" small @click="deleteItem(item)">delete</v-icon>
			</template>
			<template v-slot:item.status="{ item }">
				<v-chip v-show="serverStatusEnum.ServerDown === item.status" class="ma-2">
					{{$t(`ServerStatus.${serverStatusEnum[item.status]}`)}}</v-chip>
				<v-chip v-show="serverStatusEnum.Disable === item.status" class="ma-2" color="red">
					{{$t(`ServerStatus.${serverStatusEnum[item.status]}`)}}</v-chip>
				<v-chip v-show="serverStatusEnum.Enable === item.status" class="ma-2"
					color="green darken-3">
					{{$t(`ServerStatus.${serverStatusEnum[item.status]}`)}}</v-chip>
			</template>
		</v-data-table>
		<NotificationWarning v-show="showNotificationWarning" :title="`${$t('Message.__warning')}`"
			:max="1" :text="`${$t('Message.__isConfirmDelete')}#${deleteIndex}?`"
			:confirmed-action="confirmedAction" :canceled-action="canceledAction" />
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import * as Model from '@/models/interfaces/system-setting/servers';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import ServersApi from '@/api/system-setting/servers';
import DropdownApi from '@/api/commons/dropdown';
import { Currency } from '@/models/status/common';
import { ServerStatusEnum } from '@/models/status/system';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import NotificationWarning from '@/components/notification/NotificationWarning.vue';

import EditServerPopup from '@/components/system-setting/servers/EditServerPopup.vue';
import EditServerMerchantsPopup from '@/components/system-setting/servers/EditServerMerchantsPopup.vue';
import { pageData, time } from '@/utilities/api-handler.lib';
import { datetimeMixin } from '@/utilities/datetime-format';

import { ALL_CURRENCY } from '@/utilities/enums_helper';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import { State, Action, Getter, namespace } from 'vuex-class';
const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';
@Component({ components: { EditServerMerchantsPopup, EditServerPopup, NotificationWarning }, mixins: [commonDisplayFiltersMixin, datetimeMixin] })
export default class ServerList extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];
	@authModule.State('defaultLangTimes') defaultLangTimes!: number;
	currency: typeof Currency = Currency;
	serverStatusEnum: typeof ServerStatusEnum = ServerStatusEnum;
	renderComponent: boolean = true;
	dialogEditServer: boolean = false;
	dialogEditMerchants: boolean = false;
	panel: number[] = [0];
	searchKeyword: string = '';
	searchData: Model.IServerListRequest = {} as Model.IServerListRequest;

	gamesDropdownItems: DropdownModel.IDropdownItems[] = [];
	currencyDropdownItems: DropdownModel.IDropdownItems[] = [];
	merchantsDropdownItems: DropdownModel.IDropdownItems[] = [];
	currencySelectItems: DropdownModel.IDropdownItems[] = [];

	serversData: Model.IServer[] = [];
	editedItem: Model.IServer = {} as Model.IServer;
	deleteIndex: number | null = -1;
	showNotificationWarning: boolean = false;

	@Action('Players/setServerID') private setServerID!: any;

	@Watch('defaultLangTimes')
	onDefaultLangChanged() {
		if (this.serversData.length > 0) {
			this.refresh();
		}
	}

	get hasPermissionCreate() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSettingServers_Create) >= 0;
	}
	get hasPermissionUpdate() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSettingServers_Update) >= 0;
	}
	get hasPermissionDelete() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSettingServers_Delete) >= 0;
	}
	get hasPermissionDispatch() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSettingServers_Dispatch) >= 0;
	}

	get headers() {
		const data = [
			{ text: this.$t('TableHeader.__serverID'), value: 'serverID', sortable: true },
			{ text: this.$t('TableHeader.__gameName'), value: 'gameName' },
			{ text: this.$t('TableHeader.__server'), value: 'serverName' },
			{ text: this.$t('TableHeader.__status'), value: 'status' },
			{ text: this.$t('TableHeader.__flagLevel'), value: 'flagLevel' },
			{ text: this.$t('TableHeader.__roomFlagLevel'), value: 'roomFlagLevelText' },
			{ text: this.$t('TableHeader.__currency'), value: 'currencyText' },
			{ text: this.$t('TableHeader.__assignMerchantTotalCount'), value: 'assignMerchantTotalCount' },
			{ text: this.$t('TableHeader.__minAvailableMoney'), value: 'minAvailableMoney' },
			{ text: this.$t('TableHeader.__lastUpdateTime'), value: 'lastUpdateTime' },

			// { text: this.$t('TableHeader.__clientAddr'), value: 'clientAddr' },
			// { text: this.$t('TableHeader.__notifyAddr'), value: 'notifyAddr' },
			// { text: this.$t('TableHeader.__httpAddr'), value: 'httpAddr' },
		];
		if (this.hasPermissionUpdate || this.hasPermissionDelete) {
			data.splice(0, 0, { text: this.$t('TableHeader.__lock'), value: 'action', sortable: false });
		}
		return data;
	}

	openDetail(serverID: number, currency: string) {
		this.setServerID(serverID);
		this.$router.push({ path: '/system-setting/batch-edit-servers-merchant?serverID=' + serverID + '&currency=' + currency });
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

	refresh() {
		this.registerReloadComponent();

		this.searchData.pageSize = 1000;

		ServersApi.getServers(this.searchData).then((res) => {
			this.serversData = res.data.items;
			const mod: number = res.data.totalCount % pageData.pageSize === 0 ? 0 : 1;
			if (res.data.totalCount > pageData.pageSize) {
				for (let i = 2; i <= res.data.totalCount / pageData.pageSize + mod; i++) {
					this.searchData.pageIndex = i;

					setTimeout(() => {
						ServersApi.getServers(this.searchData).then((restData) => {
							this.serversData.push(...restData.data.items);
						});
					}, time);
				}
			}
			console.log(this.serversData);
		});
	}
	search() {
		this.refresh();
	}
	clear() {
		this.searchData = {} as Model.IServerListRequest;
		this.refresh();
	}

	addServer(item: Model.IServer) {
		this.editedItem.serverID = -1;
		this.dialogEditServer = true;
	}
	editServer(item: Model.IServer) {
		this.editedItem = { ...item };
		this.dialogEditServer = true;
	}

	editServerSuccess() {
		EventBus.$emit(`successfully-modified`);
		this.refresh();
		this.dialogEditServer = false;
	}

	editMerchants(item: Model.IServer) {
		this.editedItem = { ...item };
		this.dialogEditMerchants = true;
	}
	editServerMerchantsSuccess() {
		EventBus.$emit(`successfully-modified`);
		this.refresh();
		// this.dialogEditMerchants = false;
	}

	deleteItem(item: Model.IServer) {
		this.deleteIndex = item.serverID;
		this.showNotificationWarning = true;
		this.$notify({
			group: 'confirm',
			type: 'warn',
			duration: -1,
			speed: 10,
		});
	}
	confirmedAction() {
		ServersApi.deleteServer(Number(this.deleteIndex)).then(() => {
			EventBus.$emit(`successfully-modified`);
			this.showNotificationWarning = false;
			this.refresh();
		});
	}
	canceledAction() {
		this.showNotificationWarning = false;
	}

	mounted() {
		this.refresh();

		DropdownApi.getGamesOriginal().then((res) => {
			this.gamesDropdownItems = res;
		});

		DropdownApi.getCurrency().then((res) => {
			this.currencySelectItems = res;
		});

		DropdownApi.getMerchants().then((res) => {
			this.merchantsDropdownItems = res;
		});
	}
}
</script>

<style lang="scss">
.dialog-edit-merchants {
	// background: linear-gradient(249deg, rgba(158, 169, 169, 0.9) 50%, rgba(155, 163, 158, 0.9) 82%);
	// background: -moz-linear-gradient(249deg, rgba(158, 169, 169, 0.9) 50%, rgba(155, 163, 158, 0.9) 82%);
	// background: -webkit-linear-gradient(249deg, rgba(158, 169, 169, 0.9) 50%, rgba(155, 163, 158, 0.9) 82%);
	// background: -o-linear-gradient(249deg, rgba(158, 169, 169, 0.9) 50%, rgba(155, 163, 158, 0.9) 82%);
	// min-width: 80% !important;
	max-width: 120vh !important;
	// min-height: 75% !important;
}
</style>
