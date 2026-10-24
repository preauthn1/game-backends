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
								<!-- <div class="item right"> -->
								<div class="item">
									<v-btn color="yellow darken-1" text class="btn search" @click.stop="search">
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
					</v-flex>
				</v-expansion-panel-content>
				<v-expansion-panel-content>
					<v-flex>
						<v-row>
							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-select clearable :items="transferTypeItems" v-model="searchData.transferType" :label="$t('TableHeader.__transferType')"></v-select>
								</div>
							</v-col>
							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<date-picker
										v-model="createTimeRange"
										type="datetime"
										value-type="timestamp"
										v-bind:clearable="false"
										range
										:placeholder="$t('TableHeader.__createTime2')"
									></date-picker>
								</div>
							</v-col>
							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-text-field v-model="searchData.orderNo" :label="$t('TableHeader.__orderNo')" append-icon="description" clearable></v-text-field>
								</div>
							</v-col>
							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-text-field v-model="searchData.merchantOrderNo" :label="$t('TableHeader.__merchantOrderNo')" append-icon="description" clearable></v-text-field>
								</div>
							</v-col>
						</v-row>
					</v-flex>
				</v-expansion-panel-content>
			</v-expansion-panel>
		</v-expansion-panels>

		<div class="mt-1">
			<v-icon class="" small style="margin-left: 10px">error</v-icon>
			<span>{{ $t('Message.__singleWalletNotSupport') }}</span>

			<span class="f12 grey-text ml-3" style="display: inline-block" v-if="searchResultObj.serverTimeTW"> {{ $t('TableHeader.__lastSearchTime') }}: {{ searchResultObj.serverTimeTW }} </span>
		</div>

		<v-data-table
			:headers="headers"
			id="table"
			:disable-sort="true"
			:items="playersData"
			class="mt-2"
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
			<template v-slot:item.transferType="{ item }">
				<v-chip v-show="walletTransferTypeEnum.Transfer_In === item.transferType" class="ma-2" small color="green darken-3">{{
					$t(`WalletTransferTypeEnum.${walletTransferTypeEnum[item.transferType]}`)
				}}</v-chip>
				<v-chip v-show="walletTransferTypeEnum.Transfer_Out === item.transferType" class="ma-2" small color="red darken-3">{{
					$t(`WalletTransferTypeEnum.${walletTransferTypeEnum[item.transferType]}`)
				}}</v-chip>
			</template>
			<template v-slot:item.createTime="{ item }">
				<span class="pointer" @click="globalPopupCopyDateTime(item.createTime)">
					{{ item.createTime | timeWithAddTimezone(userProfile.timeZone) }}
				</span>
			</template>

			<template v-slot:item.amount="{ item }">
				{{ item.amount | amountDisplay }}
			</template>

			<template v-slot:item.merchantName="{ item }">
				<div>
					{{ item.merchantName }}
				</div>
				<div class="sub-title-text">
					{{ item.merchantID }}
				</div>
			</template>

			<template v-slot:item.playerName="{ item }">
				<div>
					{{ item.playerName }}
				</div>
				<div class="sub-title-text">
					{{ item.playerID }}
				</div>
			</template>

			<template v-slot:item.orderNo="{ item }">
				<div>
					{{ item.orderNo }}
				</div>
				<div class="sub-title-text">
					{{ item.merchantOrderNo }}
				</div>
			</template>
			<template v-slot:item.beforeAmount="{ item }">
				{{ item.beforeAmount | amountDisplay }}
			</template>
			<template v-slot:item.afterAmount="{ item }">
				{{ item.afterAmount | amountDisplay }}
			</template>
		</v-data-table>

		<PopupCopyDatetime ref="refCopyDateTime"></PopupCopyDatetime>
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Prop, Watch } from 'vue-property-decorator';
import { State, Action, Getter, namespace } from 'vuex-class';
import EventBus from '@/utilities/event-bus';
import { WalletTransferTypeEnum } from '@/models/status/players';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { commonMethodMixin } from '@/utilities/common-methods';
import DropdownApi from '@/api/commons/dropdown';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import { datetimeMixin, startEndDate, timeWithTimezone, daysAgo, monthsAgo } from '@/utilities/datetime-format';
import * as Model from '@/models/interfaces/members-management/transfer';
import TransferApi from '@/api/members-management/transfer';
import PopupCopyDatetime from '@/components/PopupCopyDatetime.vue';
import DatePicker from 'vue2-datepicker';
import DeletePlayersPopup from '@/components/members-management/DeletePlayersPopup.vue';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import XLSX from 'xlsx';

const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';

// @ts-ignore
@Component({ mixins: [datetimeMixin, commonDisplayFiltersMixin, commonMethodMixin], components: { DatePicker, DeletePlayersPopup, PopupCopyDatetime } })
export default class TransferRecord extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];
	walletTransferTypeEnum: typeof WalletTransferTypeEnum = WalletTransferTypeEnum;
	panel: number[] = [0];
	searchData: Model.ITransferRecordRequest = {} as Model.ITransferRecordRequest;
	renderComponent: boolean = true;
	createTimeRange: number[] = [];
	merchantsDropdownItems: DropdownModel.IDropdownItems[] = [];

	get hasPermissionPlayerDetail() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.MembersManagementDetail) >= 0;
	}
	get transferTypeItems() {
		return [
			{ text: this.$t('WalletTransferTypeEnum.Transfer_In'), value: WalletTransferTypeEnum.Transfer_In },
			{ text: this.$t('WalletTransferTypeEnum.Transfer_Out'), value: WalletTransferTypeEnum.Transfer_Out },
		];
	}
	get headers() {
		return [
			{ text: this.$t('TableHeader.__createTime2'), value: 'createTime' },
			{ text: this.$t('TableHeader.__merchant'), value: 'merchantName' },
			{ text: this.$t('TableHeader.__playerAccount'), value: 'playerName' },
			{ text: this.$t('TableHeader.__transferType'), value: 'transferType' },
			{ text: this.$t('TableHeader.__orderNo') + '/' + this.$t('TableHeader.__merchantOrderNo'), value: 'orderNo' },
			{ text: this.$t('TableHeader.__convertRate'), value: 'convertRate', align: 'end' },
			{ text: this.$t('TableHeader.__amount'), value: 'amount', align: 'end' },
			{ text: this.$t('TableHeader.__beforeAmount'), value: 'beforeAmount', align: 'end' },
			{ text: this.$t('TableHeader.__afterAmount'), value: 'afterAmount', align: 'end' },
			{ text: this.$t('TableHeader.__memo'), value: 'memo' },
		];
	}
	get orderByColumn() {
		return [
			{ text: this.$t('TableHeader.__playerID'), value: 'playerID' },
			{ text: this.$t('TableHeader.__playerAccount'), value: 'playerName' },
			{ text: this.$t('TableHeader.__merchant'), value: 'merchantID' },
			{ text: this.$t('TableHeader.__createTime2'), value: 'createTime' },
		];
	}

	totalCount: number = 0;
	pageIndex: number = 1;
	footerProps: any = { 'items-per-page-options': [10, 30, 50, 100] };
	itemsPerPage: number = 10;
	playersData: Model.ITransferRecordReponse[] = [];
	searchResultObj: any = {};
	dialogDeletePlayerPopup: boolean = false;
	delItems: number[] = [];
	downloading: boolean = false;

	@Action('Players/setPlayerID') private setPlayerID!: any;
	get createTimeRangeText() {
		return this.createTimeRange[0] && this.createTimeRange[1] ? this.createTimeRange.join(' ~ ') : '请选择日期范围';
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

	geti18nTextAsString(key: string) {
		return this.$t(key).toString();
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

	search() {
		const request: Model.ITransferRecordRequest = {
			playerID: this.searchData.playerID,
			playerName: this.searchData.playerName,
			merchantID: this.searchData.merchantID,
			orderNo: this.searchData.orderNo,
			merchantOrderNo: this.searchData.merchantOrderNo,
			transferType: this.searchData.transferType,
			createTimeBegin: this.createTimeRange[0] ? startEndDate(this.createTimeRange[0]) : null,
			createTimeEnd: this.createTimeRange[1] ? startEndDate(this.createTimeRange[1]) : null,

			pageIndex: this.pageIndex,
			pageSize: this.itemsPerPage,
			orderByColumn: this.searchData.orderByColumn,
			isASC: this.searchData.isASC,
		};

		EventBus.$emit('show_mask', null);
		TransferApi.getTransferRecord(request)
			.then((res) => {
				this.searchResultObj = res.data;
				this.playersData = res.data.items;
				this.totalCount = res.data.totalCount;
				this.delItems = this.delItems.map((s) => s);
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	clear() {
		this.searchData = {} as Model.ITransferRecordRequest;
		this.searchData.orderByColumn = 'createTime';
		this.createTimeRange = [];
		this.delItems = [];
	}

	mounted() {
		this.createTimeRange = [daysAgo(-7), daysAgo(1)];
		this.registerReloadComponent();
		this.init();
	}
	init() {
		DropdownApi.getMerchants(true).then((res) => {
			this.merchantsDropdownItems = res;
			this.searchData.orderByColumn = 'createTime';
		});
	}

	openDetail(playerID: number) {
		this.setPlayerID(playerID);
		this.$router.push({ path: '/members-management/detail' });
	}

	async exportFiles() {
		const excelHeaders: any = [
			this.geti18nTextAsString('TableHeader.__createTime2'),
			this.geti18nTextAsString('TableHeader.__merchant'),
			this.geti18nTextAsString('TableHeader.__playerID'),
			this.geti18nTextAsString('TableHeader.__playerAccount'),
			this.geti18nTextAsString('TableHeader.__transferType'),
			this.geti18nTextAsString('TableHeader.__orderNo'),
			this.geti18nTextAsString('TableHeader.__merchantOrderNo'),
			this.geti18nTextAsString('TableHeader.__convertRate'),
			this.geti18nTextAsString('TableHeader.__amount'),
			this.geti18nTextAsString('TableHeader.__beforeAmount'),
			this.geti18nTextAsString('TableHeader.__afterAmount'),
			this.geti18nTextAsString('TableHeader.__memo'),
		];

		const excelData: any = [];

		const request: Model.ITransferRecordRequest = {
			playerID: this.searchData.playerID,
			playerName: this.searchData.playerName,
			merchantID: this.searchData.merchantID,
			orderNo: this.searchData.orderNo,
			merchantOrderNo: this.searchData.merchantOrderNo,
			transferType: this.searchData.transferType,
			createTimeBegin: this.createTimeRange[0] ? startEndDate(this.createTimeRange[0]) : null,
			createTimeEnd: this.createTimeRange[1] ? startEndDate(this.createTimeRange[1]) : null,
			pageIndex: 1,
			pageSize: 10000,
			orderByColumn: this.searchData.orderByColumn,
			isASC: this.searchData.isASC,
		};

		const result = await TransferApi.getTransferRecord(request);

		const vm = this;
		result.data.items.forEach((value) => {
			const eachRow: any = [];
			eachRow.push(timeWithTimezone(value.createTime, vm.userProfile.timeZone));
			eachRow.push(value.merchantName);
			eachRow.push(value.playerID);
			eachRow.push(value.playerName);
			eachRow.push(value.transferTypeText);
			eachRow.push(value.orderNo);
			eachRow.push(value.merchantOrderNo);
			eachRow.push(value.convertRate.toFixed(4));
			eachRow.push(value.amount.toFixed(2));
			eachRow.push(value.beforeAmount.toFixed(2));
			eachRow.push(value.afterAmount.toFixed(2));
			eachRow.push(value.memo);

			excelData.push(eachRow);
		});

		const config = {
			headers: excelHeaders,
			rowData: excelData,
			fileName: 'Export_TransferRecord.xlsx',
		};

		// @ts-ignore
		this.downloadExcel(config);
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

.checkbox-item {
	margin-top: -15px !important;
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
