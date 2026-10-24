<template>
	<v-app v-if="renderComponent" class="app">

		<v-expansion-panels v-model="panel" multiple>
			<v-expansion-panel>

				<v-expansion-panel-content>
					<v-flex>
						<v-row class="search-form">

							<v-col cols="12" sm="3" md="3">
								<div class="item">

									<v-text-field v-model="searchFilter.RTPValue" :label="'RTP數值搜尋'" clearable>
									</v-text-field>

								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">

									<v-autocomplete clearable :items="vendorConfigDropdownItems"
										v-model="searchFilter.vendorConfigID" :label="$t('TableHeader.__vendorConfig')">
									</v-autocomplete>

								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete clearable :items="gamesDropdownItems"
										v-model="searchFilter.gameID" :label="$t('TableHeader.__game')">
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

									<v-btn color="blue lighten-3" text class="btn search"
										@click.stop="showSyncAggRtpDialog">
										<v-icon left>refresh</v-icon>
										重新同步AGG資料
									</v-btn>&#9;

								</div>
							</v-col>
						</v-row>
					</v-flex>
				</v-expansion-panel-content>
			</v-expansion-panel>
		</v-expansion-panels>

		<v-data-table :headers="headers" id="table" :items="searchList"
			:page="DataTablePagingParameter.pageIndex"
			:items-per-page="DataTablePagingParameter.itemsPerPage"
			:server-items-length="DataTablePagingParameter.totalCount"
			:footer-props="DataTablePagingParameter.footerProps" @update:page="changePage"
			@update:items-per-page="changeItemPerPage">

			<template v-slot:item.action="{ item }">

				<v-menu offset-y>
					<template v-slot:activator="{ on, attrs }">

						<div class="dropdown-menu" v-bind="attrs" v-on="on">
							<v-icon dark outline>
								expand_more
							</v-icon>
						</div>

					</template>

					<v-list class="dropdonwMenuPanel">
						<v-list-item class="dropdownMenu">

							<v-list-item @click="showEditRTP(item)">

								<v-icon>
									edit
								</v-icon>
								修改RTP
							</v-list-item>

							<v-list-item @click="showEditWinLimit(item)">
								<v-icon>
									edit
								</v-icon>
								修改贏分上限
							</v-list-item>

							<v-list-item @click="fetchGameOptionRawResponse(item)">
								<v-icon>
									refresh
								</v-icon>
								請求API取得當前設定 (Raw)
							</v-list-item>

						</v-list-item>
					</v-list>
				</v-menu>

			</template>

			<template v-slot:item.vendorConfigName="{ item }">
				{{ item.vendorConfigID  }}<span class="grey-text">｜</span>{{ item.vendorConfigName  }}
			</template>

			<template v-slot:item.winLimit="{ item }">

				<a class="link-text" @click="showEditWinLimit(item)">
					{{ item.winLimit | countDisplay  }}
				</a>
			</template>

			<template v-slot:item.rtp="{ item }">

				<a class="link-text" @click="showEditRTP(item)">
					{{ item.rtp | countDisplay  }}
				</a>
			</template>

			<!-- <template v-slot:item.gameName="{ item }">
				{{ item.gameID  }}<span class="grey-text">｜</span>{{ item.gameName  }}
			</template> -->

			<template v-slot:item.createTime="{ item }">
				{{ item.createTime | timeWithAddTimezone(userProfile.timeZone) }}
			</template>

			<template v-slot:item.lastUpdateTime="{ item }">
				{{ item.lastUpdateTime | timeWithAddTimezone(userProfile.timeZone) }}
			</template>

		</v-data-table>

		<!-- 同步AGG RTP設置 -->
		<v-dialog v-model="isShowSyncRTP" max-width="750px" persistent>

			<v-card>

				<v-card-title class="grey">
					同步AGG 遊戲RTP

					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark @click="">
							<v-icon>close</v-icon>
						</v-btn>
					</div>

				</v-card-title>

				<v-card-text style="padding-bottom: 100px;" class="syncAGGformDiv">

					<v-row class="mt-4">

						<v-autocomplete clearable :items="vendorConfigDropdownItems" multiple small-chips
							deletable-chips v-model="syncRTPForm.vendorConfigIDs"
							:label="$t('TableHeader.__vendorConfig') + '(勾選越多、同步速度越久)'">
						</v-autocomplete>

					</v-row>

					<v-row class="mt-4">

						<v-autocomplete clearable :items="gamesDropdownItems" multiple small-chips
							deletable-chips v-model="syncRTPForm.gameIDs"
							:label="$t('TableHeader.__game') + '(*若未勾選將會同步每一款遊戲、速度會較慢)'">
						</v-autocomplete>

					</v-row>

					<v-row>

						<v-icon small color="yellow">
							info
						</v-icon>

						<span class="yellow-text">
							若勾選太多服務器。可能會有逾時的狀況發生
						</span>
					</v-row>

				</v-card-text>

				<v-card-actions>

					<v-spacer></v-spacer>

					<v-btn color="yellow darken-1" text @click="syncAGGRTP" class="btn">
						<v-icon left>mdi-check-circle</v-icon>
						{{ $t('Button.__submit') }}
					</v-btn>

				</v-card-actions>

			</v-card>

		</v-dialog>

		<!-- 修改RTP -->
		<v-dialog v-model="isShowEditRTP" max-width="500px">

			<v-card>
				<v-card-title class="grey darken-2">
					修改RTP
					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark @click="refresh">
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>
				<v-card-text>
					<v-flex>

						<v-row>

							<v-text-field class="" v-model="updateTargetRTP.vendorConfigID" disabled='disabled'
								:label="$t('TableHeader.__vendorConfig') + ' ID'" autofocus>
							</v-text-field>

						</v-row>

						<v-row>

							<v-text-field class="" v-model="updateTargetRTP.vendorConfigName" disabled='disabled'
								:label="$t('TableHeader.__vendorConfig')" autofocus>
							</v-text-field>

						</v-row>

						<v-row>

							<v-text-field class="" v-model="updateTargetRTP.gameName" disabled='disabled'
								:label="$t('TableHeader.__gameName')" autofocus>
							</v-text-field>
						</v-row>

						<v-row>

							<v-text-field class="required" v-model="updateRTP.RTP" :label="'RTP'" autofocus
								required clearable>
							</v-text-field>
						</v-row>

					</v-flex>
				</v-card-text>
				<v-card-actions>
					<v-spacer></v-spacer>
					<v-btn text @click="closeAllModal" class="btn">
						<v-icon left>mdi-cancel</v-icon>
						{{ $t('Button.__cancel') }}
					</v-btn>
					<v-btn color="yellow darken-1" text @click="saveEditRTP" class="btn">
						<v-icon left>mdi-check-circle</v-icon>
						{{ $t('Button.__submit') }}
					</v-btn>
				</v-card-actions>
			</v-card>

		</v-dialog>

		<!-- 修改贏分上限 -->
		<v-dialog v-model="isShowEditWinLimit" max-width="500px">

			<v-card>
				<v-card-title class="grey darken-2">
					修改贏分上限
					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark @click="refresh">
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>
				<v-card-text>
					<v-flex>

						<v-row>

							<v-text-field class="" v-model="updateTargetRTP.vendorConfigID" disabled='disabled'
								:label="$t('TableHeader.__vendorConfig') + ' ID'" autofocus>
							</v-text-field>

						</v-row>

						<v-row>

							<v-text-field class="" v-model="updateTargetRTP.vendorConfigName" disabled='disabled'
								:label="$t('TableHeader.__vendorConfig')" autofocus>
							</v-text-field>

						</v-row>

						<v-row>

							<v-text-field class="" v-model="updateTargetRTP.gameName" disabled='disabled'
								:label="$t('TableHeader.__gameName')" autofocus>
							</v-text-field>
						</v-row>

						<v-row>

							<v-text-field class="required" v-model="updateRTP.winLimit" :label="'贏分上限 (-1代表不限制)'"
								autofocus required clearable>
							</v-text-field>
						</v-row>

					</v-flex>
				</v-card-text>
				<v-card-actions>
					<v-spacer></v-spacer>
					<v-btn text @click="closeAllModal" class="btn">
						<v-icon left>mdi-cancel</v-icon>
						{{ $t('Button.__cancel') }}
					</v-btn>
					<v-btn color="yellow darken-1" text @click="saveEditWinLimit" class="btn">
						<v-icon left>mdi-check-circle</v-icon>
						{{ $t('Button.__submit') }}
					</v-btn>
				</v-card-actions>
			</v-card>

		</v-dialog>

	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import * as Model from '@/models/interfaces/system-setting/merchants';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import * as EventModel from '@/models/interfaces/system-setting/events';
import MerchantsApi from '@/api/system-setting/merchants';
import EventsApi from '@/api/system-setting/events';
import DropdownApi from '@/api/commons/dropdown';
import DatePicker from 'vue2-datepicker';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { commonMethodMixin } from '@/utilities/common-methods';
import NotificationWarning from '@/components/notification/NotificationWarning.vue';
import { pageData, time } from '@/utilities/api-handler.lib';
import { datetimeMixin } from '@/utilities/datetime-format';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import { State, Action, Getter, namespace } from 'vuex-class';
const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';
import { uuid } from 'vue-uuid';

// @ts-ignore
@Component({ components: { NotificationWarning, DatePicker }, mixins: [commonDisplayFiltersMixin, commonMethodMixin, datetimeMixin] })
export default class GameVendorRTP extends Vue {
	get hasPermissionCreate() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSettingMerchant_Create) >= 0;
	}
	get hasPermissionUpdate() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSettingMerchant_Update) >= 0;
	}

	get headers() {
		const data = [
			{ text: this.$t('TableHeader.__action'), value: 'action', width: '5%', align: 'center', sortable: false },
			{ text: '遊戲商配置', value: 'vendorConfigName', width: '8%', sortable: false },
			{ text: this.$t('TableHeader.__gameID'), value: 'gameID', sortable: false },
			{ text: this.$t('TableHeader.__gameName'), value: 'gameName', sortable: false },
			{ text: '贏分上限', value: 'winLimit', sortable: false },
			{ text: '當前RTP', value: 'rtp', sortable: false },
			{ text: '可用RTP', value: 'availableRTP', sortable: false },
			{ text: this.$t('TableHeader.__memo'), value: 'memo', sortable: false },
			{ text: this.$t('TableHeader.__lastUpdateTime'), value: 'lastUpdateTime', sortable: false },
		];

		return data;
	}
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];

	isShowEditRTP: boolean = false;
	isShowEditWinLimit: boolean = false;
	isShowSyncRTP: boolean = false;
	renderComponent: boolean = true;

	DataTablePagingParameter: DropdownModel.DataTablePagingParameter = new DropdownModel.DataTablePagingParameter();
	datetimeRangePicker: DropdownModel.DatetimeRangePicker = new DropdownModel.DatetimeRangePicker();
	searchList: EventModel.IGameVendorRTP[] = [];
	searchFilter: EventModel.IGameVendorRTPRequest = {} as EventModel.IGameVendorRTPRequest;

	updateTargetRTP: EventModel.IGameVendorRTP = {} as EventModel.IGameVendorRTP;
	updateRTP: EventModel.IGameVendorRTPUpdateRequest = {} as EventModel.IGameVendorRTPUpdateRequest;

	panel: number[] = [0];

	syncRTPForm: DropdownModel.ILazyCommonParameterReqeust = {} as DropdownModel.ILazyCommonParameterReqeust;

	editModeOpenArray: number[] = [];
	merchantsData: Model.IMerchant[] = [];

	organizationsDropdownItems: DropdownModel.IDropdownItems[] = [];
	gamesDropdownItems: DropdownModel.IDropdownItems[] = [];
	vendorConfigDropdownItems: DropdownModel.IDropdownItems[] = [];

	@Action('Players/setEventID') private setEventID!: any;

	showInfo(eventID: number) {
		this.setEventID(eventID);
		this.$router.push({ path: '/system-setting/event-detail' });
	}

	changePage(index: number = 1) {
		this.DataTablePagingParameter.pageIndex = index;
		this.refresh();
	}

	showSyncAggRtpDialog() {
		this.isShowSyncRTP = true;

		this.syncRTPForm.vendorConfigIDs = [];

		// 選項太多了 默認改成不勾選
		// this.vendorConfigDropdownItems.forEach((x) => {
		// 	this.syncRTPForm.vendorConfigIDs.push(Number(x.value));
		// });
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
		this.searchFilter.isASC = true;

		EventsApi.getGameVendorRTP(this.searchFilter).then((res) => {
			this.searchList = res.data.items;
			this.DataTablePagingParameter.totalCount = res.data.totalCount;
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

	showEditRTP(item: EventModel.IGameVendorRTP) {
		this.isShowEditRTP = true;

		this.updateTargetRTP = item;
		this.updateRTP.gameID = item.gameID;
		this.updateRTP.vendorConfigID = item.vendorConfigID;
		this.updateRTP.RTP = item.rtp;
	}

	showEditWinLimit(item: EventModel.IGameVendorRTP) {
		this.isShowEditWinLimit = true;

		this.updateTargetRTP = item;
		this.updateRTP.gameID = item.gameID;
		this.updateRTP.vendorConfigID = item.vendorConfigID;
		this.updateRTP.winLimit = item.winLimit;
	}

	saveEditRTP() {
		EventBus.$emit('show_mask', null);

		EventsApi.updateGameVendorRTP(this.updateRTP)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.refresh();
				this.closeAllModal();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	saveEditWinLimit() {
		EventBus.$emit('show_mask', null);

		EventsApi.updateGameWinLimit(this.updateRTP)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.refresh();
				this.closeAllModal();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	fetchGameOptionRawResponse(item: EventModel.IGameVendorRTP) {
		EventBus.$emit('show_mask', null);

		EventsApi.fetchGameOptionRawResponse(item.vendorConfigID, item.gameID)
			.then((res) => {
				alert(res.data);
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	closeAllModal() {
		this.isShowEditRTP = false;
		this.isShowEditWinLimit = false;
		this.isShowSyncRTP = false;
	}

	clear() {
		this.searchFilter = {} as Model.IMerchantFilterRequest;
	}

	syncAGGRTP() {
		const msg = this.$t('Message.__isConfirmOperate') + '? (syncAGGRTP)';

		EventBus.$emit('show_mask', null);
		EventsApi.syncAGGAllRTP(this.syncRTPForm)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.closeAllModal();
				this.refresh();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	mounted() {
		DropdownApi.getGames().then((res) => {
			this.gamesDropdownItems = res;
		});

		DropdownApi.getVendorConfigSupportAggFeature().then((res) => {
			this.vendorConfigDropdownItems = res;
		});

		this.refresh();
	}
}
</script>


<style>
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

.type_0 {
	color: #b5b5b5;
}

.type_20 {
	color: #a9a6e2;
}

.type_30 {
	color: #ffa67b;
}

.type_40 {
	color: #49d49a;
}

.syncAGGformDiv .mdi-close {
	color: #ff6161;
}

.link-text {
	/* text-underline-position: under; */
}
</style>