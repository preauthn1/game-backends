<template>
	<v-app v-if="renderComponent" class="app">

		<v-expansion-panels v-model="panel" multiple>
			<v-expansion-panel>

				<v-expansion-panel-content>
					<v-flex>
						<v-row class="search-form">

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-text-field v-model="searchData.playerName"
										:label="$t('TableHeader.__playerAccount')" append-icon="person" clearable>
									</v-text-field>

								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete clearable :items="dropdownGameVendors"
										v-model="searchData.vendorID" :label="$t('TableHeader.__gameVendorType')">
									</v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<date-picker v-model="createTimeRange" type="datetime" value-type="timestamp"
										range :placeholder="$t('TableHeader.__createTime2')"></date-picker>
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

		<div style="font-size:14px;padding:5px;color:#CCC;">
			<v-icon color="grey">
				error
			</v-icon>
			{{$t('Message.__notAllGameVendorProvideDetailInfo')}} (e.g. AGG)
		</div>

		<v-data-table :headers="headers" id="table" :items="ListData"
			:page="DataTablePagingParameter.pageIndex"
			:items-per-page="DataTablePagingParameter.itemsPerPage"
			:server-items-length="DataTablePagingParameter.totalCount"
			:footer-props="DataTablePagingParameter.footerProps" @update:page="changePage"
			@update:items-per-page="changeItemPerPage" multi-sort>

			<template v-slot:item.action="{ item }">
				<v-icon v-if="item.isLaunchRecordExist" small style="color:#CCC;font-size:20px;"
					class="mr-2" @click="showRequestInfo(item)">info
				</v-icon>
			</template>

			<template v-slot:item.gameName="{ item }">

				<div v-if="!hasPermissionGameList">
					{{item.gameName}}
				</div>
				<div v-else>
					<a @click="showGameLaunchDialog(item)">
						<v-icon color="primary" small>mdi-link</v-icon>
						{{ item.gameName }}
					</a>
				</div>

			</template>

			<template v-slot:item.statusText="{ item }">

				<div v-if="item.status == 1">
					{{item.statusText}}
				</div>
				<div v-else>
					<div class="minus-red">
						{{item.statusText}}
					</div>
				</div>

			</template>

			<template v-slot:item.createTime="{ item }">
				{{ item.createTime | timeWithAddTimezone(userProfile.timeZone) }}
			</template>

		</v-data-table>

		<ExternalApiRequestLogDetailPopup ref="logDetailPopup"></ExternalApiRequestLogDetailPopup>

		<GameLaunchInternalTest ref="gameLaunchTest"></GameLaunchInternalTest>

	</v-app>

</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import GameLaunchInternalTest from '@/components/system-setting/GameLaunchInternalTest.vue';
import ExternalApiRequestLogDetailPopup from '@/components/system-logs/ExternalApiRequestLogDetailPopup.vue';

import * as LogModel from '@/models/interfaces/system-logs/externalApiRequestLog';
import * as RecordModel from '@/models/interfaces/members-management/records';
import * as Model from '@/models/interfaces/members-management/players';
import * as GameModel from '@/models/interfaces/system-setting/gameList';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import playersApi from '@/api/members-management/players';
import logRecordApi from '@/api/members-management/records';
import DropdownApi from '@/api/commons/dropdown';
import InternalTestApi from '@/api/commons/internalTest';

import { Locale, Currency, Status, GameType, CRUDAction, GameVendorType } from '@/models/status/common';
import { WalletMode } from '@/models/status/merchant';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { ALL_CURRENCY } from '@/utilities/enums_helper';
import NotificationWarning from '@/components/notification/NotificationWarning.vue';
import { pageData, time } from '@/utilities/api-handler.lib';
// import Validation from '@/utilities/validation';
import { datetimeMixin, startEndDate, daysAgo } from '@/utilities/datetime-format';
import DatePicker from 'vue2-datepicker';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import { State, Action, Getter, namespace } from 'vuex-class';

const authModule = namespace('Auth');

import * as Permission from '@/models/status/permission';

// @ts-ignore
@Component({ components: { NotificationWarning, DatePicker, GameLaunchInternalTest, ExternalApiRequestLogDetailPopup }, mixins: [commonDisplayFiltersMixin, datetimeMixin] })
export default class ThirdPartyPlayerLoginLogs extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];
	renderComponent: boolean = true;
	panel: number[] = [0];

	ListData: Model.ILoginDetailResponse[] = [];
	createTimeRange: number[] = [];

	searchData: Model.ILoginDetailRequest = {} as Model.ILoginDetailRequest;
	DataTablePagingParameter: DropdownModel.DataTablePagingParameter = new DropdownModel.DataTablePagingParameter();

	copyText: string = '';
	dropdownGameVendors: any[] = [];

	get hasPermissionCreate() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSetting_GameList_Create) >= 0;
	}
	get hasPermissionUpdate() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSetting_GameList_Update) >= 0;
	}

	get headers() {
		const data = [
			{ text: this.$t('TableHeader.__ID'), value: 'id', sortable: false, align: 'start' },
			{ text: this.$t('TableHeader.__merchant'), value: 'merchantName', sortable: false },
			{ text: this.$t('TableHeader.__playerID'), value: 'playerID', sortable: false },
			{ text: this.$t('TableHeader.__playerAccount'), value: 'playerName', sortable: false },
			{ text: this.$t('TableHeader.__gameVendorType'), value: 'vendorName', sortable: false },
			{ text: this.$t('TableHeader.__gameID'), value: 'gameID', sortable: false },
			{ text: this.$t('TableHeader.__game'), value: 'gameName', sortable: false },
			{ text: this.$t('TableHeader.__status'), value: 'statusText', sortable: false },
			{ text: this.$t('TableHeader.__createTime2'), value: 'createTime', sortable: false },
		];

		if (true) {
			data.splice(0, 0, { text: this.$t('TableHeader.__action'), value: 'action', sortable: false, align: 'center' });
		}
		return data;
		return data;
	}

	get hasPermissionGameList() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSetting_GameList) >= 0;
	}

	registerReloadComponent() {
		EventBus.$on(`reload-${this.$router.currentRoute.path}`, (info: any) => {
			this.renderComponent = false;
			this.$nextTick(() => {
				this.renderComponent = true;
			});
		});
	}

	search() {
		this.DataTablePagingParameter.pageIndex = 1;
		this.refresh();
	}

	clear() {
		this.searchData = {} as Model.ILoginDetailRequest;
		this.createTimeRange = [];
	}

	refresh() {
		this.registerReloadComponent();

		this.searchData.pageIndex = this.DataTablePagingParameter.pageIndex;
		this.searchData.pageSize = this.DataTablePagingParameter.itemsPerPage;
		this.searchData.isThirdPartyGame = true;
		this.searchData.createTimeBegin = this.createTimeRange[0] ? startEndDate(this.createTimeRange[0]) : null;
		this.searchData.createTimeEnd = this.createTimeRange[1] ? startEndDate(this.createTimeRange[1]) : null;

		playersApi.getPlayerLoginDetail(this.searchData).then((res) => {
			this.ListData = res.data.items;
			this.DataTablePagingParameter.totalCount = res.data.totalCount;
		});
	}

	changePage(index: number = 1) {
		this.DataTablePagingParameter.pageIndex = index;
		this.refresh();
	}

	changeItemPerPage(val: number = 10) {
		this.DataTablePagingParameter.itemsPerPage = val;
		this.refresh();
	}

	showGameLaunchDialog(item: Model.ILoginDetailResponse) {
		const targetRef: any = this.$refs.gameLaunchTest;

		const model: GameModel.IGameLaunchPreview = {} as GameModel.IGameLaunchPreview;
		model.gameID = item.gameID;
		model.gameName = item.gameName;
		model.vendorName = item.vendorName;

		targetRef.showGameLaunchDialog(model);
	}

	showRequestInfo(item: Model.ILoginDetailResponse) {
		EventBus.$emit('show_mask', null);

		logRecordApi
			.getExternalApiRequestLog(item.requestUUID)
			.then((res) => {
				const model: LogModel.IExternalApiRequestLogDetailInfo = {} as LogModel.IExternalApiRequestLogDetailInfo;

				if (res.data == null) {
					alert('not found data');
					return;
				}

				model.requestUrl = res.data.requestUrl;
				model.requestContent = res.data.requestContent;
				model.requestHeader = res.data.requestHeader;
				model.responseText = res.data.responseText;
				model.responseStatusCode = res.data.responseStatusCode;
				model.playerID = item.playerID;
				model.playerName = item.playerName;
				model.createTime = item.createTime;
				model.isShowCustomerMessage = true;

				const targetRef: any = this.$refs.logDetailPopup;
				targetRef.showInfoPopup(model);
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	mounted() {
		this.createTimeRange = [daysAgo(-7), daysAgo(1)];

		DropdownApi.getThirdVendors().then((res) => {
			this.dropdownGameVendors = res.map((s) => {
				return { value: Number(s.value), text: s.text };
			});
		});

		this.refresh();
	}
}
</script>
