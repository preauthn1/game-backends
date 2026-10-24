<template>
	<v-app v-if="renderComponent" class="app">
		<v-expansion-panels v-model="panel" multiple>
			<v-expansion-panel>

				<v-expansion-panel-content>
					<v-flex>
						<v-row>
							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-text-field v-model="searchFilter.playerID"
										:label="$t('TableHeader.__playerID')+ $t('Message.__playerId_NumberFormat')"
										type="number" append-icon="perm_identity" clearable></v-text-field>
								</div>
							</v-col>
							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-text-field v-model="searchFilter.playerName"
										:label="$t('TableHeader.__playerAccount')" append-icon="person" clearable>
									</v-text-field>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-text-field v-model="searchFilter.transactionNo"
										:label="$t('TableHeader.__orderNo')" append-icon="description" clearable>
									</v-text-field>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-text-field v-model="searchFilter.platformOrderNo"
										:label="$t('TableHeader.__merchantOrderNo')" append-icon="description"
										clearable></v-text-field>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete clearable :items="merchantsDropdownItems"
										class="merchantDropdownItems" v-model="searchFilter.merchantID"
										:label="$t('TableHeader.__merchant')">

										<template v-slot:item="data">
											<div
												:class="data.item.valueInt == null  ? 'merchantCategoryText': 'merchantValue'">
												<span>{{data.item.text}}</span>
											</div>
										</template>

									</v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-select clearable :items="transferTypeDropdownItems" v-model="searchFilter.type"
										:label="$t('TableHeader.__transferType')">
									</v-select>
								</div>
							</v-col>
							<v-col cols="12" sm="3" md="3">
								<div class="item" v-if="hasPermissionSuperAdmin">
									<v-text-field v-model="searchFilter.thirdOrderNo"
										:label="$t('TableHeader.__orderNo') + '(' + $t('TableHeader.__gameVendorType') + ')'"
										append-icon="description" clearable></v-text-field>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<!-- <div class="item right"> -->
								<div class="item">
									<v-btn color="yellow darken-1" text class="btn search"
										@click.stop="pageOneSearch">
										<v-icon left>search</v-icon>
										{{ $t('Button.__search') }}
									</v-btn>&#9;
									<v-btn color="pink lighten-3" text class="btn search" @click.stop="clear">
										<v-icon left>clear</v-icon>
										{{ $t('Button.__reset') }}
									</v-btn>&#9;

									<!-- <v-btn color="light-blue lighten-3" text class="btn search"
										@click.stop="exportFiles">
										<v-icon left>cloud_download</v-icon>
										{{ $t('Button.__downloadFile') }}
									</v-btn>&#9; -->

								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<date-picker v-model="datetimeRangePicker.searchStartTimeRange" type="datetime"
										value-type="timestamp" range :placeholder="$t('TableHeader.__time')">
									</date-picker>
								</div>
							</v-col>

							<v-col cols="12" sm="9" md="9" class="quick-time">
								<div class="mt-4">
									<v-chip class="mr-2"
										:class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'today' }"
										@click="globalQuicklyPickTimeRange('today')">
										<v-icon left>mdi-alarm-check</v-icon>
										{{ $t(`TableHeader.__today`) }}
									</v-chip>
									<v-chip class="mr-2"
										:class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'yesterday' }"
										@click="globalQuicklyPickTimeRange('yesterday')">
										<v-icon left>mdi-alarm-check</v-icon>
										{{ $t(`TableHeader.__yesterday`) }}
									</v-chip>
									<v-chip class="mr-2"
										:class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'thisWeek' }"
										@click="globalQuicklyPickTimeRange('thisWeek')">
										<v-icon left>mdi-alarm-check</v-icon>
										{{ $t(`TableHeader.__thisWeek`) }}
									</v-chip>

									<v-chip class="mr-2"
										:class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'lastWeek' }"
										@click="globalQuicklyPickTimeRange('lastWeek')">
										<v-icon left>mdi-alarm-check</v-icon>
										{{ $t(`TableHeader.__lastWeek`) }}
									</v-chip>
									<v-chip class="mr-2"
										:class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'thisMonth' }"
										@click="globalQuicklyPickTimeRange('thisMonth')">
										<v-icon left>mdi-alarm-check</v-icon>
										{{ $t(`TableHeader.__thisMonth`) }}
									</v-chip>
									<v-chip class="mr-2"
										:class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'lastMonth' }"
										@click="globalQuicklyPickTimeRange('lastMonth')">
										<v-icon left>mdi-alarm-check</v-icon>
										{{ $t(`TableHeader.__lastMonth`) }}
									</v-chip>
								</div>
							</v-col>

						</v-row>
					</v-flex>
				</v-expansion-panel-content>

			</v-expansion-panel>
		</v-expansion-panels>

		<div class="table-hint-icons">

			<div v-if="isCN_lang()">
				<v-icon small>error</v-icon>若要确认 掉单/转帐单是否有效。请使用
				<span class="orange-text">单号确认</span> 进行验证
			</div>

			<div v-else>
				<v-icon small class="mr-1">error</v-icon>To verify tranfer-record is valid , please click
				<span class="orange-text">Order check</span> to validate.
			</div>

		</div>

		<v-data-table :items="searchRecords" :headers="headers" id="table" :disable-sort="true"
			:page="DataTablePagingParameter.pageIndex"
			:items-per-page="DataTablePagingParameter.itemsPerPage"
			:server-items-length="DataTablePagingParameter.totalCount"
			:footer-props="DataTablePagingParameter.footerProps" @update:page="changePage"
			@update:items-per-page="getItemPerPage">

			<template v-slot:item.action="{ item }">

				<div v-if="hasAdvancePermission" style="display:inline-block">

					<v-icon :disabled="!item.isExternalPlatformOrderNo" small :title="'日誌查詢'" class="mr-1"
						@click="openSingleWalletLogHistoryPage(item)">
						mdi-comment-text-outline
					</v-icon>

					<v-icon :disabled="!(item.gameVendorID == 430000)" small :title="'单号确认'" class="mr-1"
						@click="checkTransferIsValidByThirdApi(item)">
						mdi-comment-question-outline
					</v-icon>
				</div>

			</template>

			<template v-slot:item.merchantName="{ item }">

				<div @click="expandMaxWidth($event)"
					:title="item.merchantName + ' (' + item.merchantID + ')'"
					class="maxWidthBeforeClick maxWidth150">{{item.merchantName}}
				</div>
				<div class="sub-title-text">
					{{item.merchantID}}
				</div>
				<v-chip x-small color="purple" class="ml-1" v-if="item.isSingleWallet"
					style="margin-top:-10px;">
					SW
				</v-chip>
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

			<template v-slot:item.orderNo="{ item }">
				<div>
					{{item.orderNo}}
				</div>
				<div class="sub-title-text">
					{{item.extraOrderNo}}
				</div>
			</template>

			<template v-slot:item.playerID="{ item }">
				<div>
					{{item.playerName}}
				</div>
				<div class="sub-title-text">
					{{item.playerID}}
				</div>
			</template>

			<template v-slot:item.transferType="{ item }">
				<v-chip small :title="item.transferType" :color="chipColor(item)">
					{{item.transferTypeText}}
				</v-chip>
			</template>

			<template v-slot:item.merchantOrderNo="{ item }">
				<span class="f13">
					{{item.merchantOrderNo}}
				</span>
			</template>

			<template v-slot:item.memo="{ item }">
				<v-chip color="red darken-2" v-if="item.isWeirdOrder">
					转帐异常
				</v-chip>
				<v-chip color="red darken-2" v-if="item.isPlatformOrderNoMissing">
					平台单号缺失
				</v-chip>
			</template>

			<template v-slot:item.createTime="{ item }">
				<span class="pointer" @click="globalPopupCopyDateTime(item.createTime)">
					{{ item.createTime | timeWithAddTimezone(userProfile.timeZone) }}
				</span>
			</template>

			<template v-slot:item.amount="{ item }">{{ item.amount | amountDisplay }}</template>

			<template
				v-slot:item.beforeAmount="{ item }">{{ item.beforeAmount | amountDisplay }}</template>
			<template v-slot:item.afterAmount="{ item }">{{ item.afterAmount | amountDisplay }}</template>
		</v-data-table>

		<PopupCopyDatetime ref="refCopyDateTime"></PopupCopyDatetime>

		<v-dialog v-model="dialogVisible.isShowDetailInfo" max-width="500px">

			<v-card>
				<v-card-title class="grey">
					单号确认
				</v-card-title>

				<v-card-text>

					<div class="">

						<div>
							<div class="common-info-label">
								{{$t('TableHeader.__orderNo')}}
							</div>
							<div class="common-info-value">
								{{transactionCheckResult.transactionNo}}
							</div>
						</div>

						<div>
							<div class="common-info-label">
								{{$t('TableHeader.__orderNo')}} ({{$t("TableHeader.__FishingGame")}})
							</div>
							<div class="common-info-value">
								{{transactionCheckResult.thirdOrderNo}}
							</div>
						</div>

						<div>
							<div class="common-info-label">
								{{$t('TableHeader.__orderNo')}} ({{$t("TableHeader.__merchantOrderNo")}})
							</div>
							<div class="common-info-value">
								{{transactionCheckResult.platformOrderNo}} &nbsp;
							</div>
						</div>

						<div>
							<div class="common-info-label">
								{{$t('TableHeader.__type')}}
							</div>
							<div class="common-info-value">
								{{transactionCheckResult.typeText}}
							</div>
						</div>

						<div>
							<div class="common-info-label">
								{{$t('TableHeader.__amount')}}
							</div>
							<div class="common-info-value">
								{{transactionCheckResult.amount}}
							</div>
						</div>

						<div v-if="transactionCheckResult.apiCode == 0">

							<div>
								<div class="common-info-label">
									{{$t('TableHeader.__beforeAmount')}}
								</div>
								<div class="common-info-value">

									<div
										v-if="transactionCheckResult.beforeAmount == 0 && transactionCheckResult.afterAmount == 0">
										<v-chip small>
											Unknow
										</v-chip>
									</div>
									<div v-else>
										{{transactionCheckResult.beforeAmount}}
									</div>

								</div>
							</div>

							<div>
								<div class="common-info-label">
									{{$t('TableHeader.__afterAmount')}}
								</div>
								<div class="common-info-value">

									<div
										v-if="transactionCheckResult.beforeAmount == 0 && transactionCheckResult.afterAmount == 0">
										<v-chip small>
											Unknow
										</v-chip>
									</div>
									<div v-else>
										{{transactionCheckResult.afterAmount}}
									</div>

								</div>
							</div>

							<div>
								<div class="common-info-label">
									{{$t('TableHeader.__beforeAmount')}} ({{$t('TableHeader.__FishingGame')}})
								</div>
								<div class="common-info-value">
									{{transactionCheckResult.beforeFishWalletAmount}}
								</div>
							</div>

							<div>
								<div class="common-info-label">
									{{$t('TableHeader.__afterAmount')}} ({{$t('TableHeader.__FishingGame')}})
								</div>
								<div class="common-info-value">
									{{transactionCheckResult.afterFishWalletAmount}}
								</div>
							</div>

						</div>

						<div class="text-c">
							<div style="padding: 25px;">

								<div>

									<v-icon x-large color="green lighten-2" v-if="transactionCheckResult.isValid">
										mdi-check-circle</v-icon>
									<v-icon large color="yellow" v-if="!transactionCheckResult.isValid">error
									</v-icon>
									<div class="" style="color:#faeb55;font-size:16px;">
										{{transactionCheckResult.messageOfFishingRecord}}
									</div>

								</div>

								<div
									v-if="transactionCheckResult.messageOfPlatformResponse != null && transactionCheckResult.messageOfPlatformResponse.length > 0">

									<v-icon large color="yellow">
										error
									</v-icon>
									<div class="" style="color:#faeb55;font-size:16px;">
										{{transactionCheckResult.messageOfPlatformResponse}}
									</div>
								</div>

								<div
									v-if="transactionCheckResult.messageOfValidateBeforeAmount != null && transactionCheckResult.messageOfValidateBeforeAmount.length > 0">

									<v-icon large color="orange darken-2">
										error
									</v-icon>
									<div class="" style="color:#faeb55;font-size:16px;">
										{{transactionCheckResult.messageOfValidateBeforeAmount}}
									</div>
								</div>

								<div class="f12 grey-text">
									API Result: {{transactionCheckResult.apiCode}} ,
									{{transactionCheckResult.apiMessage}}
								</div>
							</div>
						</div>

					</div>

				</v-card-text>

			</v-card>

		</v-dialog>

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

	dialogVisible: DropdownModel.DialogVisible = new DropdownModel.DialogVisible();

	datetimeRangePicker: DropdownModel.DatetimeRangePicker = new DropdownModel.DatetimeRangePicker();
	DataTablePagingParameter: DropdownModel.DataTablePagingParameter = new DropdownModel.DataTablePagingParameter();
	searchFilter: Model.IGameVendorTransferRecordRequest = {} as Model.IGameVendorTransferRecordRequest;
	searchRecords: Model.IGameVendorTransferRecordReponse[] = [];
	renderComponent: boolean = true;
	transactionCheckResult: Model.ITransferValidCheckReponse = {} as Model.ITransferValidCheckReponse;

	merchantsDropdownItems: DropdownModel.IDropdownItems[] = [];
	transferTypeDropdownItems: DropdownModel.IDropdownItems[] = [];

	get hasAdvancePermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.MembersManagement_GameVendorTransfer_Advance) >= 0;
	}

	get hasPermissionPlayerDetail() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.MembersManagementDetail) >= 0;
	}

	get hasPermissionSuperAdmin() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSetting_SystemConfigSetting) >= 0;
	}

	get headers() {
		return [
			{ text: this.$t('TableHeader.__action'), value: 'action', align: 'center' },
			{ text: this.$t('TableHeader.__createTime2'), value: 'createTime' },
			{ text: this.$t('TableHeader.__merchant'), value: 'merchantName' },
			{ text: this.$t('TableHeader.__playerID'), value: 'playerID' },
			{ text: this.$t('TableHeader.__transferType'), value: 'transferType' },
			{ text: this.$t('TableHeader.__amount'), value: 'amount', align: 'end' },
			{ text: this.$t('TableHeader.__orderNo') + '／' + this.$t('TableHeader.__orderNo') + '(' + this.$t('TableHeader.__gameVendorType') + ')', value: 'orderNo' },
			{ text: this.$t('TableHeader.__merchantOrderNo'), value: 'merchantOrderNo' },

			// { text: this.$t('TableHeader.__beforeAmount'), value: 'beforeAmount', align: 'end' },
			// { text: this.$t('TableHeader.__afterAmount'), value: 'afterAmount', align: 'end' },

			{ text: this.$t('TableHeader.__memo'), value: 'memo' },
		];
	}

	@Action('Players/setPlayerID') private setPlayerID!: any;

	changePage(index: number = 1) {
		this.DataTablePagingParameter.pageIndex = index;
		if (this.searchRecords.length > 0) {
			this.search();
		}
	}
	getItemPerPage(val: number = 10) {
		this.DataTablePagingParameter.itemsPerPage = val;
		if (this.searchRecords.length > 0) {
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
				this.searchRecords = [];
				this.renderComponent = true;
			});
		});
	}

	pageOneSearch() {
		this.DataTablePagingParameter.pageIndex = 1;
		this.search();
	}

	search() {
		this.searchFilter.pageIndex = this.DataTablePagingParameter.pageIndex;
		this.searchFilter.pageSize = this.DataTablePagingParameter.itemsPerPage;
		this.searchFilter.createTimeBegin = this.datetimeRangePicker.searchStartTimeRange[0] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[0]) : null;
		this.searchFilter.createTimeEnd = this.datetimeRangePicker.searchStartTimeRange[1] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[1]) : null;

		EventBus.$emit('show_mask', null);
		TransferApi.getGameVendorTransferRecord(this.searchFilter)
			.then((res) => {
				this.searchRecords = res.data.items;
				this.DataTablePagingParameter.totalCount = res.data.totalCount;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	checkTransferIsValidByThirdApi(item: Model.ITransferRecordReponse) {
		EventBus.$emit('show_mask', null);
		TransferApi.checkTransferIsValidByThirdApi(item.orderNo)
			.then((res) => {
				this.dialogVisible.isShowDetailInfo = true;
				this.transactionCheckResult = res.data;

				if (this.transactionCheckResult.apiCode !== 0) {
					this.search(); // 刷新列表
				}
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	chipColor(item: Model.IGameVendorTransferRecordReponse) {
		if (item.transferType === 1) {
			return 'green darken-4';
		}

		if (item.transferType === 2) {
			return 'blue darken-4';
		}

		if (item.transferType === 51) {
			return 'brown darken-2';
		}

		return '';
	}

	clear() {
		this.searchFilter = {} as Model.ITransferRecordRequest;
	}

	isCN_lang() {
		return this.$i18n.locale === 'cn' || this.$i18n.locale === 'tw';
	}

	mounted() {
		this.datetimeRangePicker.searchStartTimeRange = [daysAgo(-3), daysAgo(1)];
		this.registerReloadComponent();
		this.init();
	}
	init() {
		DropdownApi.getMerchants(true).then((res) => {
			this.merchantsDropdownItems = res;
		});

		DropdownApi.getGameVendorTransferTypes().then((res) => {
			this.transferTypeDropdownItems = res;
		});
	}

	openSingleWalletLogHistoryPage(item: Model.IGameVendorTransferRecordReponse) {
		EventBus.$emit('close_tab', '/system-logs/SingleWalletPlatformReqeustLog');
		const query = '?referenceTransactionNo=' + item.orderNo + '&begin=' + item.createTime;
		this.$router.push({ path: '/system-logs/SingleWalletPlatformReqeustLog' + query });
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
			this.geti18nTextAsString('TableHeader.__amount'),
			this.geti18nTextAsString('TableHeader.__beforeAmount'),
			this.geti18nTextAsString('TableHeader.__afterAmount'),
			this.geti18nTextAsString('TableHeader.__memo'),
		];

		const excelData: any = [];

		this.searchFilter.pageIndex = 1;
		this.searchFilter.pageSize = 10000;
		this.searchFilter.createTimeBegin = this.datetimeRangePicker.searchStartTimeRange[0] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[0]) : null;
		this.searchFilter.createTimeEnd = this.datetimeRangePicker.searchStartTimeRange[1] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[1]) : null;

		const result = await TransferApi.getGameVendorTransferRecord(this.searchFilter);

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
	margin-bottom: 15px !important;
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
