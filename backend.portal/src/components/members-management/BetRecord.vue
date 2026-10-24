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
									<v-text-field v-model="searchData.playerID" :label="$t('TableHeader.__playerID')"
										append-icon="perm_identity" clearable></v-text-field>
								</div>
							</v-col>
							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-text-field v-model="searchData.playerName"
										:label="$t('TableHeader.__playerAccount')" append-icon="person" clearable>
									</v-text-field>
								</div>
							</v-col>
							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-text-field v-model="searchData.iPAddress"
										:label="$t('TableHeader.__iPAddress')" append-icon="pin_drop" clearable>
									</v-text-field>
								</div>
							</v-col>
							<!-- <v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-text-field v-model="searchData.tradeID" label="tradeID" append-icon="question_answer" clearable></v-text-field>
								</div>
							</v-col>-->
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
									<!-- <v-btn color="light-blue lighten-3" text class="btn search" @click.stop="deletePlayer">
										<v-icon left>delete</v-icon>封号
									</v-btn>-->
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
									<v-autocomplete clearable :items="gamesDropdownItems" v-model="searchData.gameID"
										:label="$t('TableHeader.__game')"></v-autocomplete>
								</div>
							</v-col>
							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete clearable :items="serversDropdownItems"
										v-model="searchData.serverID" :label="$t('TableHeader.__server')">
									</v-autocomplete>
								</div>
							</v-col>
							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<date-picker v-model="loginTimeRange" type="datetime" value-type="timestamp" range
										:placeholder="$t('TableHeader.__loginTime')"></date-picker>
								</div>
							</v-col>
							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<date-picker v-model="logoutTimeRange" type="datetime" value-type="timestamp"
										range :placeholder="$t('TableHeader.__logoutTime')"></date-picker>
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
									<v-autocomplete :items="orderByColumn" v-model="searchData.orderByColumn"
										:label="$t('Filter.__sort')"></v-autocomplete>
								</div>
							</v-col>
							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-checkbox v-model="searchData.isASC" :label="$t('Filter.__increasing')">
									</v-checkbox>
								</div>
							</v-col>
						</v-row>
					</v-flex>
				</v-expansion-panel-content>
			</v-expansion-panel>
		</v-expansion-panels>
		<v-data-table :items="result" :headers="headers" id="table" :disable-sort="true"
			:page="pageIndex" :items-per-page="itemsPerPage" :server-items-length="totalCount"
			:footer-props="footerProps" @update:page="changePage" @update:items-per-page="getItemPerPage">
			<template v-slot:item.playerID="{ item }">
				<v-tooltip top color="teal darken-1">
					<template v-slot:activator="{ on }">
						<a v-on="on" v-show="hasPermissionPlayerDetail" @click="openDetail(item.playerID)">
							<v-icon color="primary" small left>mdi-link</v-icon>
							{{ item.playerID }}
						</a>
					</template>
					<span>{{$t('Button.__openLink')}}</span>
				</v-tooltip>
				<div v-show="!hasPermissionPlayerDetail">{{ item.playerID }}</div>
			</template>

			<template v-slot:item.gameName="{ item }">
				<v-tooltip top color="teal darken-1">
					<template v-slot:activator="{ on }">
						<a v-on="on" v-show="hasPermissionGameDetail"
							@click="openGameDetail(item.playerID, item.gameID)">
							<v-icon color="primary" small left>mdi-link</v-icon>
							{{ item.gameName }}
						</a>
					</template>
					<span>{{$t('Button.__openLink')}}</span>
				</v-tooltip>
				<div v-show="!hasPermissionGameDetail">{{ item.gameName }}</div>
			</template>
			<template
				v-slot:item.gameTimeSecond="{ item }">{{ item.gameTimeSecond | toHHMMSS }}</template>
			<template v-slot:item.loginAmount="{ item }">{{ item.loginAmount | amountDisplay }}</template>
			<template
				v-slot:item.updateAmount="{ item }">{{ item.updateAmount | amountDisplay }}</template>
			<template
				v-slot:item.walletMode="{ item }">{{$t(`MerchantsStatus.${walletMode[item.walletMode]}`)}}</template>
			<template
				v-slot:item.loginTime="{ item }">{{ item.loginTime | timeWithAddTimezone(userProfile.timeZone) }}</template>
			<template
				v-slot:item.logoutTime="{ item }">{{ item.logoutTime | timeWithAddTimezone(userProfile.timeZone) }}</template>
		</v-data-table>
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import { State, Action, Getter, namespace } from 'vuex-class';
import EventBus from '@/utilities/event-bus';
import { datetimeMixin, startEndDate } from '@/utilities/datetime-format';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import * as Model from '@/models/interfaces/members-management/players';
import PlayersApi from '@/api/members-management/players';
import DropdownApi, { convertLang } from '@/api/commons/dropdown';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import DatePicker from 'vue2-datepicker';
import * as PermissionModel from '@/models/interfaces/commons/permission';
const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';
import { WalletMode } from '@/models/status/merchant';

// @ts-ignore
@Component({ mixins: [datetimeMixin, commonDisplayFiltersMixin], components: { DatePicker } })
export default class BetRecord extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];
	@authModule.State('defaultLangTimes') defaultLangTimes!: number;
	walletMode: typeof WalletMode = WalletMode;
	renderComponent: boolean = true;
	panel: number[] = [0];
	totalCount: number = 0;
	pageIndex: number = 1;
	footerProps: any = { 'items-per-page-options': [10, 30, 50, 100] };
	itemsPerPage: number = 10;

	loginTimeRange: number[] = [];
	logoutTimeRange: number[] = [];
	searchData: Model.IBetRecordRequest = {} as Model.IBetRecordRequest;
	// merchantID: number | null = null;
	// gameID: number | null = null;
	// serverID: number | null = null;
	// merchantsDropdownItems: DropdownModel.IDropdownItems[] = [];
	gamesDropdownItemsTemp: DropdownModel.IDropdownItems[] = [];
	gamesDropdownItems: DropdownModel.IDropdownItems[] = [];
	serversDropdownItems: DropdownModel.IDropdownItems[] = [];
	playerID: number | null = null;
	playerName: string = '';
	result: Model.IBetRecordReponse[] = [];
	get hasPermissionGameDetail() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.GamesManagementRecord) >= 0;
	}

	get hasPermissionPlayerDetail() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.MembersManagementDetail) >= 0;
	}
	get headers() {
		return [
			{ text: this.$t('TableHeader.__merchant'), value: 'merchantName' },
			{ text: this.$t('TableHeader.__merchantID'), value: 'merchantID' },
			{ text: this.$t('TableHeader.__playerID'), value: 'playerID' },
			{ text: this.$t('TableHeader.__playerAccount'), value: 'playerName' },
			{ text: this.$t('TableHeader.__game'), value: 'gameName' },
			{ text: this.$t('TableHeader.__server'), value: 'serverName' },
			{ text: this.$t('TableHeader.__loginAmount'), value: 'loginAmount' },
			{ text: this.$t('TableHeader.__updateAmount'), value: 'updateAmount' },
			{ text: this.$t('TableHeader.__taxAmount'), value: 'taxAmount' },
			{ text: this.$t('TableHeader.__winCount'), value: 'winCount' },
			{ text: this.$t('TableHeader.__loseCount'), value: 'loseCount' },
			{ text: this.$t('TableHeader.__drawCount'), value: 'drawCount' },
			{ text: this.$t('TableHeader.__gameTimeSeconds'), value: 'gameTimeSecond' },
			{ text: this.$t('TableHeader.__iPAddress'), value: 'ipAddress' },
			{ text: this.$t('TableHeader.__walletMode'), value: 'walletMode' },
			{ text: this.$t('TableHeader.__loginTime'), value: 'loginTime' },
			{ text: this.$t('TableHeader.__logoutTime'), value: 'logoutTime' },
		];
	}
	get orderByColumn() {
		return [
			{ text: this.$t('TableHeader.__merchant'), value: 'merchantID' },
			{ text: this.$t('TableHeader.__playerID'), value: 'playerID' },
			{ text: this.$t('TableHeader.__playerAccount'), value: 'playerName' },
			{ text: this.$t('TableHeader.__game'), value: 'gameID' },
			{ text: this.$t('TableHeader.__server'), value: 'serverID' },
			{ text: this.$t('TableHeader.__loginTime'), value: 'loginTime' },
			{ text: this.$t('TableHeader.__logoutTime'), value: 'logoutTime' },
		];
	}

	@Action('Players/setPlayerID') private setPlayerID!: any;
	@Action('Players/setGameID') private setGameID!: any;
	@Action('Players/setGamePlayerID') private setGamePlayerID!: any;
	changePage(index: number = 1) {
		this.pageIndex = index;
		if (this.result.length > 0) {
			this.search();
		}
	}

	@Watch('searchData.gameID', { immediate: true, deep: true })
	onGameIDChange() {
		DropdownApi.getServers(!this.searchData.gameID ? null : Number(this.searchData.gameID)).then((res) => {
			this.serversDropdownItems = res;
		});
	}

	@Watch('defaultLangTimes')
	onDefaultLangChanged() {
		if (this.result.length > 0) {
			this.search();
		}
		this.gamesDropdownItems = convertLang(this.gamesDropdownItemsTemp);
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
		request.loginTimeBegin = this.loginTimeRange[0] ? startEndDate(this.loginTimeRange[0]) : null;
		request.loginTimeEnd = this.loginTimeRange[1] ? startEndDate(this.loginTimeRange[1]) : null;
		request.logoutTimeBegin = this.logoutTimeRange[0] ? startEndDate(this.logoutTimeRange[0]) : null;
		request.logoutTimeEnd = this.logoutTimeRange[1] ? startEndDate(this.logoutTimeRange[1]) : null;
		request.orderByColumn = this.searchData.orderByColumn;
		request.isASC = this.searchData.isASC;
		PlayersApi.getBetecord(<Model.IBetRecordRequest>request).then((res) => {
			this.result = res.data.items;
			this.totalCount = res.data.totalCount;
		});
	}
	mounted() {
		this.init();
		// this.search();
		this.registerReloadComponent();
	}

	openDetail(playerID: number) {
		this.setPlayerID(playerID);
		this.$router.push({ path: '/members-management/detail' });
	}

	openGameDetail(playerID: number, gameID: number) {
		this.setGamePlayerID(playerID);
		this.setGameID(gameID);
		this.$router.push({ path: '/games-management/record' });
	}

	init() {
		// DropdownApi.getMerchants().then(res => {
		// 	this.merchantsDropdownItems = res.data;
		// 	this.searchData.orderByColumn = 'createTime';
		// });
		DropdownApi.getGamesOriginal().then((res) => {
			this.gamesDropdownItemsTemp = res;
			this.gamesDropdownItems = convertLang(this.gamesDropdownItemsTemp);
			this.searchData.orderByColumn = 'loginTime';
		});
		DropdownApi.getServers().then((res) => {
			this.serversDropdownItems = res;
			// this.searchData.orderByColumn = 'createTime';
		});
	}

	clear() {
		this.searchData = {} as Model.IBetRecordRequest;
		this.searchData.orderByColumn = 'loginTime';
		this.loginTimeRange = [];
		this.logoutTimeRange = [];
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
