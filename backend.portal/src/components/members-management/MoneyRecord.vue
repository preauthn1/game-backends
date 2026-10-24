<template>
	<v-app v-if="renderComponent" class="app">
		<v-expansion-panels v-model="panel" v-if="isRenderFilter" multiple>
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
									<v-select clearable :items="transactionTypeDropdownItems" v-model="searchData.transactionType" :label="$t('TableHeader.__transactionType')"></v-select>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-text-field
										v-model="searchData.referenceOrderNo"
										:label="$t('TableHeader.__referenceNo') + ' (' + $t('Message._mustFilterPlayerID') + ')'"
										append-icon="description"
										clearable
									>
									</v-text-field>
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
									<v-autocomplete clearable :items="merchantsDropdownItems" class="merchantDropdownItems" v-model="searchData.merchantID" :label="$t('TableHeader.__merchant')">
										<template v-slot:item="data">
											<div :class="data.item.valueInt == null ? 'merchantCategoryText' : 'merchantValue'">
												<span>{{ data.item.text }}</span>
											</div>
										</template>
									</v-autocomplete>
								</div>
							</v-col>
							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete clearable :items="gamesDropdownItems" v-model="searchData.gameID" :label="$t('TableHeader.__game')"></v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item" v-if="hasSystenAdminPermission">
									<v-text-field clearable v-model="searchData.amountMatch" :label="$t('TableHeader.__updateAmount') + '(可能要嘗試*100 )'"> </v-text-field>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<!-- <div class="item right"> -->
								<div class="item">
									<v-btn color="yellow darken-1" text class="btn search" @click.stop="pageOneSearch">
										<v-icon left>search</v-icon>
										{{ $t('Button.__search') }} </v-btn
									>&#9;
									<v-btn color="pink lighten-3" text class="btn search" @click.stop="clear">
										<v-icon left>clear</v-icon>
										{{ $t('Button.__reset') }} </v-btn
									>&#9;

									<v-btn color="light-blue lighten-3" text class="btn search" @click.stop="exportFiles">
										<v-icon left>cloud_download</v-icon>
										{{ $t('Button.__downloadFile') }} </v-btn
									>&#9;
								</div>
							</v-col>
						</v-row>

						<v-row class="mt-4">
							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<date-picker
										v-model="datetimeRangePicker.searchStartTimeRange"
										type="datetime"
										v-bind:clearable="false"
										value-type="timestamp"
										range
										:placeholder="$t('TableHeader.__time')"
									>
									</date-picker>
								</div>
							</v-col>

							<v-col cols="12" sm="6" md="6" class="quick-time mt-4">
								<!-- <div class="item"> -->
								<v-chip class="mr-2" :class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'today' }" @click="globalQuicklyPickTimeRange('today')">
									<v-icon left>mdi-alarm-check</v-icon>
									{{ $t(`TableHeader.__today`) }}
								</v-chip>
								<v-chip class="mr-2" :class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'yesterday' }" @click="globalQuicklyPickTimeRange('yesterday')">
									<v-icon left>mdi-alarm-check</v-icon>
									{{ $t(`TableHeader.__yesterday`) }}
								</v-chip>
								<v-chip class="mr-2" :class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'thisWeek' }" @click="globalQuicklyPickTimeRange('thisWeek')">
									<v-icon left>mdi-alarm-check</v-icon>
									{{ $t(`TableHeader.__thisWeek`) }}
								</v-chip>

								<v-chip class="mr-2" :class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'lastWeek' }" @click="globalQuicklyPickTimeRange('lastWeek')">
									<v-icon left>mdi-alarm-check</v-icon>
									{{ $t(`TableHeader.__lastWeek`) }}
								</v-chip>
								<v-chip
									class="mr-2"
									v-if="hasSystenAdminPermission"
									:class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'thisMonth' }"
									@click="globalQuicklyPickTimeRange('thisMonth')"
								>
									<v-icon left>mdi-alarm-check</v-icon>
									{{ $t(`TableHeader.__thisMonth`) }}
								</v-chip>
								<v-chip
									class="mr-2"
									v-if="hasSystenAdminPermission"
									:class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'lastMonth' }"
									@click="globalQuicklyPickTimeRange('lastMonth')"
								>
									<v-icon left>mdi-alarm-check</v-icon>
									{{ $t(`TableHeader.__lastMonth`) }}
								</v-chip>
								<!-- </div> -->
							</v-col>
						</v-row>
					</v-flex>
				</v-expansion-panel-content>
			</v-expansion-panel>
		</v-expansion-panels>

		<div class="table-hint-icons">
			<v-icon class="">error</v-icon>
			<span>{{ $t('Message.__dataKeepXMonthOnly', { month: 2 }) }}</span>

			<v-icon class="" style="margin-left: 10px">error</v-icon>
			<span>{{ $t('Message.__singleWalletNotSupport') }}</span>

			<span class="f12 grey-text ml-3" style="display: inline-block" v-if="searchResultObj.serverTimeTW"> {{ $t('TableHeader.__lastSearchTime') }}: {{ searchResultObj.serverTimeTW }} </span>
		</div>

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
			<template v-slot:item.gameName="{ item }">
				<v-tooltip top color="teal darken-1">
					<template v-slot:activator="{ on }">
						<a v-on="on" v-show="hasPermissionGameDetail && item.gameName" @click="openGameDetail(item.playerID, item.gameID)">
							<v-icon color="primary" small left>mdi-link</v-icon>
							{{ item.gameName }}
						</a>
					</template>
					<span>{{ $t('Button.__openLink') }}</span>
				</v-tooltip>
				<div v-show="!hasPermissionGameDetail">{{ item.gameName ? item.gameName : '' }}</div>
			</template>
			<template v-slot:item.transactionType="{ item }">
				<v-chip class="ma-2" :color="colorByTransactionType(item.transactionType)">
					{{ item.transactionTypeText }}
				</v-chip>
			</template>
			<template v-slot:item.walletMode="{ item }">{{ item.walletMode ? $t(`MerchantsStatus.${walletMode[item.walletMode]}`) : '' }}</template>

			<template v-slot:item.currentAmount="{ item }">
				{{ item.currentAmount | amountDisplay }}
			</template>
			<template v-slot:item.updateAmount="{ item }">
				<span :class="item.updateAmount < 0 ? 'minus-red' : ''">
					{{ item.updateAmount | amountDisplay }}
				</span>
			</template>
			<template v-slot:item.afterAmount="{ item }">
				{{ item.afterAmount | amountDisplay }}
			</template>

			<template v-slot:item.createTime="{ item }">
				<span class="pointer" @click="globalPopupCopyDateTime(item.createTime)">
					{{ item.createTime | timeWithAddTimezone(userProfile.timeZone) }}
				</span>
			</template>
		</v-data-table>

		<PopupCopyDatetime ref="refCopyDateTime"></PopupCopyDatetime>
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import { State, Action, Getter, namespace } from 'vuex-class';
import EventBus from '@/utilities/event-bus';
import { datetimeMixin, startEndDate, timeWithTimezone, daysAgo, monthsAgo } from '@/utilities/datetime-format';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { commonMethodMixin } from '@/utilities/common-methods';
import * as Model from '@/models/interfaces/members-management/players';
import PlayersApi from '@/api/members-management/players';
import DropdownApi, { convertLang } from '@/api/commons/dropdown';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import DatePicker from 'vue2-datepicker';
import * as PermissionModel from '@/models/interfaces/commons/permission';
const authModule = namespace('Auth');
const playersModule = namespace('Players');
import * as Permission from '@/models/status/permission';
import { TransactionTypeEnum } from '@/models/status/players';
import { WalletMode } from '@/models/status/merchant';
import PopupCopyDatetime from '@/components/PopupCopyDatetime.vue';

// @ts-ignore
@Component({ mixins: [datetimeMixin, commonDisplayFiltersMixin, commonMethodMixin], components: { DatePicker, PopupCopyDatetime } })
export default class MoneyRecord extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];
	@authModule.State('defaultLangTimes') defaultLangTimes!: number;
	@playersModule.State('walletPlayerID') walletPlayerID!: number;

	@playersModule.State('moneyRecordReferenceNo') moneyRecordReferenceNo!: string;

	renderComponent: boolean = true;
	isRenderFilter: boolean = true;

	transactionTypeEnum: typeof TransactionTypeEnum = TransactionTypeEnum;
	walletMode: typeof WalletMode = WalletMode;
	panel: number[] = [0];
	totalCount: number = 0;
	pageIndex: number = 1;
	footerProps: any = { 'items-per-page-options': [10, 30, 50, 100] };
	itemsPerPage: number = 10;

	datetimeRangePicker: DropdownModel.DatetimeRangePicker = new DropdownModel.DatetimeRangePicker();
	searchData: Model.IMoneyRecordRequest = {} as Model.IMoneyRecordRequest;
	result: Model.IMoneyRecordReponse[] = [];
	searchResultObj: any = {};

	merchantsDropdownItems: DropdownModel.IDropdownItems[] = [];
	gamesDropdownItemsTemp: DropdownModel.IDropdownItems[] = [];
	gamesDropdownItems: DropdownModel.IDropdownItems[] = [];
	transactionTypeDropdownItems: DropdownModel.IDropdownItems[] = [];

	playerID: number | null = null;
	playerName: string = '';
	downloading: boolean = false;

	get hasSystenAdminPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSetting_SystemConfigSetting) >= 0;
	}

	get hasPermissionGameDetail() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.GamesManagementRecord) >= 0;
	}

	get hasPermissionPlayerDetail() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.MembersManagementDetail) >= 0;
	}

	get headers() {
		return [
			{ text: this.$t('TableHeader.__time'), value: 'createTime' },
			{ text: this.$t('TableHeader.__merchant'), value: 'merchantName' },
			{ text: this.$t('TableHeader.__playerID'), value: 'playerID' },
			{ text: this.$t('TableHeader.__playerAccount'), value: 'playerName' },
			{ text: this.$t('TableHeader.__transactionType'), value: 'transactionType' },
			{ text: this.$t('TableHeader.__game'), value: 'gameName' },
			{ text: this.$t('TableHeader.__memo'), value: 'referenceNo' },
			{ text: this.$t('TableHeader.__updateAmount'), value: 'updateAmount', align: 'end' },
			{ text: this.$t('TableHeader.__beforeAmount'), value: 'currentAmount', align: 'end' },
			{ text: this.$t('TableHeader.__afterAmount'), value: 'afterAmount', align: 'end' },
		];
	}
	get orderByColumn() {
		return [
			{ text: this.$t('TableHeader.__time'), value: 'createTime' },
			{ text: this.$t('TableHeader.__merchant'), value: 'merchantID' },
			{ text: this.$t('TableHeader.__playerID'), value: 'playerID' },
			{ text: this.$t('TableHeader.__playerAccount'), value: 'playerName' },
			{ text: this.$t('TableHeader.__transactionType'), value: 'transactionType' },
			{ text: this.$t('TableHeader.__game'), value: 'gameID' },
		];
	}

	@Action('Players/setPlayerID') private setPlayerID!: any;
	@Action('Players/setGameID') private setGameID!: any;
	@Action('Players/setGamePlayerID') private setGamePlayerID!: any;
	@Action('Players/setServerID') private setServerID!: any;

	@Watch('walletPlayerID')
	onWalletPlayerIDChanged(newVal: number) {
		this.searchData.playerID = newVal;

		console.log(this.searchData.playerID);

		if (this.searchData.playerID && this.searchData.playerID > 0) {
			this.search();
		}
	}

	@Watch('moneyRecordReferenceNo')
	onMoneyRecordReferenceNo(newVal: string) {
		this.searchData.playerID = this.walletPlayerID;
		this.searchData.referenceOrderNo = newVal;
		this.search();
	}

	@Watch('defaultLangTimes')
	onDefaultLangChanged() {
		if (this.result.length > 0) {
			this.search();
		}
		this.gamesDropdownItems = convertLang(this.gamesDropdownItemsTemp);
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

	geti18nTextAsString(key: string) {
		return this.$t(key).toString();
	}

	reRenderFilterBlock() {
		this.isRenderFilter = false;
		this.isRenderFilter = true;
	}

	colorByTransactionType(type: number) {
		if (type === this.transactionTypeEnum.TransferWallet_To_GameWallet) {
			return 'indigo';
		}
		if (type === this.transactionTypeEnum.TransferWallet_To_PlatformWallet) {
			return 'green darken-2';
		}
		if (type === this.transactionTypeEnum.EventPrize_RedEnvelope || type === this.transactionTypeEnum.EventPrize) {
			return 'red darken-4';
		}
		if (type === this.transactionTypeEnum.ManualDepositWalletAmount || type === this.transactionTypeEnum.BetRecord_ChangeSettleInfo) {
			return 'purple';
		}
		return '';
	}

	async exportFiles() {
		const excelHeaders: any = [
			this.geti18nTextAsString('TableHeader.__time') + '(GMT+' + this.userProfile.timeZone.toString() + ')',
			this.geti18nTextAsString('TableHeader.__merchant'),
			this.geti18nTextAsString('TableHeader.__playerID'),
			this.geti18nTextAsString('TableHeader.__playerAccount'),
			this.geti18nTextAsString('TableHeader.__transactionType'),
			this.geti18nTextAsString('TableHeader.__game'),
			this.geti18nTextAsString('TableHeader.__updateAmount'),
			this.geti18nTextAsString('TableHeader.__beforeAmount'),
			this.geti18nTextAsString('TableHeader.__afterAmount'),
		];

		const excelData: any = [];
		const request = this.searchData;
		request.pageIndex = 1;
		request.pageSize = 10000;
		request.createTimeBegin = this.datetimeRangePicker.searchStartTimeRange[0] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[0]) : null;
		request.createTimeEnd = this.datetimeRangePicker.searchStartTimeRange[1] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[1]) : null;
		request.orderByColumn = this.searchData.orderByColumn;
		request.isASC = this.searchData.isASC;

		const result = await PlayersApi.getMoneyecord(<Model.IMoneyRecordRequest>request);

		result.data.items.forEach((value) => {
			const eachRow: any = [];
			eachRow.push(timeWithTimezone(value.createTime, this.userProfile.timeZone));
			eachRow.push(value.merchantName);
			eachRow.push(value.playerID);
			eachRow.push(value.playerName);
			eachRow.push(value.transactionTypeText);
			eachRow.push(value.gameName);
			eachRow.push(value.updateAmount.toFixed(2));
			eachRow.push(value.currentAmount.toFixed(2));
			eachRow.push(value.afterAmount.toFixed(2));

			excelData.push(eachRow);
		});

		const config = {
			headers: excelHeaders,
			rowData: excelData,
			fileName: 'Export_MoneyRecord.xlsx',
		};

		// @ts-ignore
		this.downloadExcel(config);
	}

	pageOneSearch() {
		this.pageIndex = 1;
		this.search();
	}

	search() {
		const request = this.searchData;
		request.pageIndex = this.pageIndex;
		request.pageSize = this.itemsPerPage;
		request.createTimeBegin = this.datetimeRangePicker.searchStartTimeRange[0] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[0]) : null;
		request.createTimeEnd = this.datetimeRangePicker.searchStartTimeRange[1] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[1]) : null;
		request.orderByColumn = this.searchData.orderByColumn;
		request.isASC = this.searchData.isASC;

		EventBus.$emit('show_mask', null);
		PlayersApi.getMoneyecord(<Model.IMoneyRecordRequest>request)
			.then((res) => {
				this.searchResultObj = res.data;
				this.result = res.data.items;
				this.totalCount = res.data.totalCount;
				this.reRenderFilterBlock();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}
	mounted() {
		this.init();
		this.registerReloadComponent();
		if (this.walletPlayerID) {
			this.searchData.playerID = this.walletPlayerID;
			this.search();
		}
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
		this.datetimeRangePicker.searchStartTimeRange = [daysAgo(-2), daysAgo(1)];

		const route: any = this.$route;
		this.searchData.playerName = route.query.playerName;

		DropdownApi.getMerchants(true).then((res) => {
			this.merchantsDropdownItems = res;
			this.searchData.orderByColumn = 'createTime';
		});

		DropdownApi.getGamesOriginal().then((res) => {
			this.gamesDropdownItemsTemp = res;
			this.gamesDropdownItems = convertLang(this.gamesDropdownItemsTemp);
			this.searchData.orderByColumn = 'createTime';
		});

		DropdownApi.getTransactionTypes().then((res) => {
			this.transactionTypeDropdownItems = res;
		});
	}

	clear() {
		this.searchData = {} as Model.IMoneyRecordRequest;
		this.searchData.orderByColumn = 'createTime';
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
