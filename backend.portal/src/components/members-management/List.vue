<template>
	<v-app v-if="renderComponent" class="app">
		<v-expansion-panels v-model="panel" multiple>
			<v-expansion-panel>
				<v-expansion-panel-content>
					<v-flex>
						<v-row>
							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-text-field
										v-model="searchData.playerID"
										:label="$t('TableHeader.__playerID') + $t('Message.__playerId_NumberFormat')"
										type="number"
										append-icon="perm_identity"
										clearable
									></v-text-field>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-text-field v-model="searchData.playerName" :label="$t('TableHeader.__playerAccount')" append-icon="person" clearable> </v-text-field>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-text-field
										v-model="searchData.playerNameFuzzyQuery"
										:label="$t('TableHeader.__playerAccount') + '(' + $t('Message.__fuzzyQuery') + ')'"
										append-icon="person"
										clearable
									>
									</v-text-field>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete clearable :items="merchantsDropdownItems" class="merchantDropdownItems" v-model="searchData.merchantID" :label="$t('TableHeader.__merchant')">
										<template v-slot:item="data">
											<div :class="data.item.valueInt == null ? 'merchantCategoryText' : 'merchantValue'">
												<span>{{ data.item.text }}</span>
											</div>
										</template>
									</v-autocomplete>
								</div>
							</v-col>
						</v-row>
					</v-flex>
				</v-expansion-panel-content>
				<v-expansion-panel-content>
					<v-flex>
						<v-row>
							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete clearable :items="extraFilterDropdownItems" class="" v-model="searchData.extraFilterType" :label="$t('TableHeader.__extraFilterType')">
									</v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete clearable :items="tagsDropdownItems" class="merchantDropdownItems" v-model="searchData.tagID" :label="$t('TableHeader.__tag')"> </v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete clearable :items="statusDropdownItems" class="statusDropdownItems" v-model="searchData.status" :label="$t('TableHeader.__status')">
									</v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-btn color="yellow darken-1" text class="btn search" @click.stop="searchPageOne">
										<v-icon left>search</v-icon>
										{{ $t('Button.__search') }} </v-btn
									>&#9;
									<v-btn color="pink lighten-3" text class="btn search" @click.stop="clear">
										<v-icon left>clear</v-icon>
										{{ $t('Button.__reset') }} </v-btn
									>&#9;
									<v-btn v-if="hasPermission" color="light-blue lighten-3" text class="btn search" @click.stop="deletePlayer">
										<v-icon left>delete</v-icon>
										{{ $t('Button.__lock') }}
									</v-btn>
									<v-btn v-if="hasSuperermission" color="orange lighten-3" text style="max-width: 150px" class="btn ml-1 mt-2" @click.stop="exportData()">
										<v-icon left>cloud_download</v-icon>
										{{ $t('Button.__downloadFile') }} ({{ $t('TableHeader.__currentPageOnly') }})
									</v-btn>
								</div>
							</v-col>
						</v-row>
					</v-flex>
				</v-expansion-panel-content>
				<v-expansion-panel-content>
					<v-flex>
						<v-row>
							<v-col cols="12" sm="3" md="3" v-if="hasSuperermission">
								<div class="item">
									<date-picker v-model="createTimeRange" type="datetime" value-type="timestamp" range :placeholder="$t('TableHeader.__create')"></date-picker>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3" v-if="hasSuperermission">
								<div class="item">
									<date-picker v-model="lastLoginTimeRange" type="datetime" value-type="timestamp" range :placeholder="$t('TableHeader.__lastLogin')"></date-picker>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-text-field v-model="searchData.vendorSessionTokenMatch" v-if="hasSuperermission" :label="'token搜寻'" clearable> </v-text-field>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3"> </v-col>
						</v-row>
					</v-flex>
				</v-expansion-panel-content>
			</v-expansion-panel>
		</v-expansion-panels>
		<v-data-table
			:headers="headers"
			id="table"
			:disable-sort="true"
			:items="playersData"
			:page="pageIndex"
			:items-per-page="itemsPerPage"
			:server-items-length="totalCount"
			:footer-props="footerProps"
			@update:page="changePage"
			@update:items-per-page="getItemPerPage"
		>
			<template v-slot:item.action="{ item }">
				<v-checkbox v-model="delItems" :value="item.playerID" hide-details></v-checkbox>
			</template>
			<template v-slot:item.playerID="{ item }">
				<v-tooltip top color="teal darken-1">
					<template v-slot:activator="{ on }">
						<a v-on="on" v-show="hasPermissionPlayerDetail" @click="openDetail(item.playerID)">
							<v-icon color="primary" small left>mdi-link</v-icon>
							{{ item.playerID }}
						</a>
					</template>
					<span>{{ $t('Button.__openLink') }}</span>
				</v-tooltip>
				<div v-show="!hasPermissionPlayerDetail">{{ item.playerID }}</div>
			</template>
			<template v-slot:item.status="{ item }">
				<v-chip v-show="status.PermanentDelete === item.status" class="ma-2"> {{ $t('Status.PermanentDelete') }}</v-chip>
				<v-chip v-show="status.Disable === item.status" class="ma-2" color="red"> {{ $t('Status.Disable') }}</v-chip>
				<v-chip v-show="status.Enable === item.status" class="ma-2" color="green darken-3"> {{ $t('Status.Enable') }}</v-chip>
			</template>
			<template v-slot:item.walletMode="{ item }">
				{{ $t(`MerchantsStatus.${walletMode[item.walletMode]}`) }}
				<v-chip x-small color="purple" class="ml-1" v-if="item.walletMode == 2">SW</v-chip>
			</template>
			<template v-slot:item.createTime="{ item }">{{ item.createTime | timeWithAddTimezone(userProfile.timeZone) }} </template>
			<template v-slot:item.lastLoginTime="{ item }">{{ item.lastLoginTime | timeWithAddTimezone(userProfile.timeZone) }} </template>
			<template v-slot:item.walletAmount="{ item }">
				<span v-if="item.walletMode == 1">
					{{ item.walletAmount | amountDisplay }}
				</span>
				<span v-else> - </span>
			</template>

			<template v-slot:item.merchantName="{ item }">
				<span :title="item.merchantID">
					{{ item.merchantName }}
				</span>
			</template>
		</v-data-table>
		<v-dialog v-model="dialogDeletePlayerPopup" max-width="500px">
			<DeletePlayersPopup v-if="dialogDeletePlayerPopup" :propsData="delItems" :status="1" @update-success="updateSuccess" @close="dialogDeletePlayerPopup = false" />
		</v-dialog>
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Prop, Watch } from 'vue-property-decorator';
import { State, Action, Getter, namespace } from 'vuex-class';
import EventBus from '@/utilities/event-bus';
import { Status, ExtraFilterTypeDataCategory } from '@/models/status/common';
import { WalletMode } from '@/models/status/merchant';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import DropdownApi from '@/api/commons/dropdown';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import { datetimeMixin, startEndDate } from '@/utilities/datetime-format';
import * as Model from '@/models/interfaces/members-management/players';
import { commonMethodMixin } from '@/utilities/common-methods';
import PlayersApi from '@/api/members-management/players';
import DatePicker from 'vue2-datepicker';
import DeletePlayersPopup from '@/components/members-management/DeletePlayersPopup.vue';
import * as PermissionModel from '@/models/interfaces/commons/permission';
const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';

// @ts-ignore
@Component({ mixins: [datetimeMixin, commonDisplayFiltersMixin, commonMethodMixin], components: { DatePicker, DeletePlayersPopup } })
export default class List extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];

	status: typeof Status = Status;
	walletMode: typeof WalletMode = WalletMode;
	panel: number[] = [0];
	searchData: Model.IPlayersRequest = {} as Model.IPlayersRequest;
	renderComponent: boolean = true;
	createTimeRange: number[] = [];
	lastLoginTimeRange: number[] = [];

	extraFilterDropdownItems: DropdownModel.IDropdownItems[] = [];
	merchantsDropdownItems: DropdownModel.IDropdownItems[] = [];
	tagsDropdownItems: DropdownModel.IDropdownItems[] = [];
	statusDropdownItems: DropdownModel.IDropdownItems[] = [];

	get hasPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.MembersManagementList_Lock) >= 0;
	}

	get hasPermissionPlayerDetail() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.MembersManagementDetail) >= 0;
	}

	get hasSuperermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSetting_SystemConfigSetting) >= 0;
	}

	get headers() {
		const data = [
			{ text: this.$t('TableHeader.__playerID'), value: 'playerID' },
			{ text: this.$t('TableHeader.__playerAccount'), value: 'playerName' },
			{ text: this.$t('TableHeader.__status'), value: 'status' },
			{ text: this.$t('TableHeader.__merchantID'), value: 'merchantID' },
			{ text: this.$t('TableHeader.__merchant'), value: 'merchantName' },
			{ text: this.$t('TableHeader.__currency'), value: 'currency' },
			{ text: this.$t('TableHeader.__walletMode'), value: 'walletMode' },
			{ text: this.$t('TableHeader.__walletAmount'), value: 'walletAmount', align: 'end' },
			{ text: this.$t('TableHeader.__createTime'), value: 'createTime' },
			{ text: this.$t('TableHeader.__lastLoginTime'), value: 'lastLoginTime' },
		];
		if (this.hasPermission) {
			data.splice(0, 0, { text: this.$t('TableHeader.__lock'), value: 'action' });
		}
		return data;
	}

	totalCount: number = 0;
	pageIndex: number = 1;
	footerProps: any = { 'items-per-page-options': [10, 30, 50, 100] };
	itemsPerPage: number = 10;
	playersData: Model.IPlayer[] = [];
	dialogDeletePlayerPopup: boolean = false;
	delItems: number[] = [];

	@Action('Players/setPlayerID') private setPlayerID!: any;

	get createTimeRangeText() {
		return this.createTimeRange[0] && this.createTimeRange[1] ? this.createTimeRange.join(' ~ ') : '请选择日期范围';
	}

	@Watch('allPermissions')
	onAllPermissionsChanged() {
		const headers = this.headers;
	}

	get lastLoginTimeRangeText() {
		return this.lastLoginTimeRange[0] && this.lastLoginTimeRange[1] ? this.lastLoginTimeRange.join(' ~ ') : '请选择日期范围';
	}

	changePage(index: number = 1) {
		this.pageIndex = index;
		if (this.playersData.length > 0) {
			this.search();
		}
	}

	getItemPerPage(val: number = 10) {
		this.itemsPerPage = val;
		if (this.playersData.length > 0) {
			this.search();
		}
	}
	registerReloadComponent() {
		EventBus.$on(`reload-${this.$router.currentRoute.path}`, (info: any) => {
			this.renderComponent = false;
			this.$nextTick(() => {
				this.clear();
				this.playersData = [];
				this.renderComponent = true;
			});
		});
	}

	searchPageOne() {
		this.pageIndex = 1;
		this.search();
	}

	geti18nTextAsString(key: string) {
		return this.$t(key).toString();
	}

	exportData() {
		const vm: any = this;

		const excelHeaders: any = [
			this.geti18nTextAsString('TableHeader.__playerID'),
			this.geti18nTextAsString('TableHeader.__playerAccount'),
			this.geti18nTextAsString('TableHeader.__merchantID'),
			this.geti18nTextAsString('TableHeader.__merchant'),
			this.geti18nTextAsString('TableHeader.__walletMode'),
			this.geti18nTextAsString('TableHeader.__createTime'),
		];

		const excelData: any = [];

		for (const player of this.playersData) {
			const eachRow: any = [];

			eachRow.push(player.playerID);
			eachRow.push(player.playerName);
			eachRow.push(player.merchantID);
			eachRow.push(player.merchantName);
			eachRow.push(player.walletModeText);
			eachRow.push(player.createTime);
			excelData.push(eachRow);
		}

		const config = {
			headers: excelHeaders,
			rowData: excelData,
			fileName: 'Export_Players.xlsx',
		};

		// @ts-ignore
		this.downloadExcel(config);
	}

	search() {
		this.searchData.createTimeBegin = this.createTimeRange[0] ? startEndDate(this.createTimeRange[0]) : null;
		this.searchData.createTimeEnd = this.createTimeRange[1] ? startEndDate(this.createTimeRange[1]) : null;

		this.searchData.lastLoginTimeBegin = this.lastLoginTimeRange[0] ? startEndDate(this.lastLoginTimeRange[0]) : null;
		this.searchData.lastLoginTimeEnd = this.lastLoginTimeRange[1] ? startEndDate(this.lastLoginTimeRange[1]) : null;

		this.searchData.pageIndex = this.pageIndex;
		this.searchData.pageSize = this.itemsPerPage;

		EventBus.$emit('show_mask', null);
		PlayersApi.getPlayers(this.searchData)
			.then((res) => {
				this.playersData = res.data.items;
				this.totalCount = res.data.totalCount;
				this.delItems = this.delItems.map((s) => s);
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	clear() {
		this.searchData = {} as Model.IPlayersRequest;
		this.createTimeRange = [];
		this.lastLoginTimeRange = [];
		this.delItems = [];
	}
	updateSuccess() {
		this.search();
		this.delItems = [];
		EventBus.$emit(`successfully-modified`);
		this.dialogDeletePlayerPopup = false;
	}
	mounted() {
		this.registerReloadComponent();
		this.init();
	}

	init() {
		const headers = this.headers;

		DropdownApi.getMerchants(true).then((res) => {
			this.merchantsDropdownItems = res;
		});

		DropdownApi.getPlayerTags().then((res) => {
			this.tagsDropdownItems = res;
		});

		DropdownApi.getExtraFilterType(ExtraFilterTypeDataCategory.Player).then((res) => {
			this.extraFilterDropdownItems = res;
		});

		DropdownApi.getEnableDisableStatus().then((res) => {
			this.statusDropdownItems = res;
		});
	}

	deletePlayer() {
		if (this.delItems.length === 0) {
			EventBus.$emit('event-error', this.$t('Message.__pleaseSelectTheTitlePlayer'));
			return;
		}
		this.dialogDeletePlayerPopup = true;
	}

	openDetail(playerID: number) {
		this.setPlayerID(playerID);
		this.$router.push({ path: '/members-management/detail' });
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

// .checkbox-item {
// 	margin-top: -15px !important;
// }
.v-input--hide-details {
	margin: 0px auto !important;
}
.search {
	max-width: 80px;
	margin-top: 15px !important;
}
.footer {
	white-space: nowrap;
}

.mx-datepicker {
	margin-top: 15px !important;
	width: 100% !important;
	width: 100% !important;
}
</style>
