<template>
	<v-app v-if="renderComponent" class="app">
		<v-expansion-panels v-model="panel" multiple>
			<v-expansion-panel>
				<v-expansion-panel-content>
					<v-flex>
						<v-row class="search-form">
							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete :items="searchTargetFieldItems" :label="$t('TableHeader.__searchField')" small-chips v-model="searchTargetField"> </v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-text-field v-model="myInputKeyword" :label="$t('TableHeader.__searchText') + ' (' + $t('Message.__mustSelectLeftFilterWhichColumn') + ')'" clearable>
									</v-text-field>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete clearable :items="dropdownItemsEvents" v-model="searchFilter.eventID" :label="$t('TableHeader.__eventID')"> </v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete clearable :items="dropdownItemsGames" v-model="searchFilter.gameID" :label="$t('TableHeader.__game')"> </v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete clearable :items="dropdownItemsEventPrizeTypes" v-model="searchFilter.eventPrizeType" :label="$t('TableHeader.__type')"> </v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete clearable :items="dropdownItemsPrizeRecordStatus" v-model="searchFilter.status" :label="$t('TableHeader.__status')"> </v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete clearable :items="dropdownItemsMerchants" v-model="searchFilter.merchantID" :label="$t('TableHeader.__merchant')"> </v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete :items="TimeColumnDropdown" v-model="searchFilter.timeColumn" :label="$t('TableHeader.__searchType') + '(' + $t('TableHeader.__time') + ')'">
									</v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3" class="" v-if="hasSuperAdminPermission">
								<div class="item">
									<v-autocomplete clearable :items="dropdownItemsVendorConfigs" v-model="searchFilter.vendorConfigID" :label="$t('TableHeader.__vendorConfig')"> </v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3" class="v-col-padding-0">
								<div class="item">
									<date-picker
										v-model="datetimeRangePicker.searchStartTimeRange"
										type="datetime"
										v-bind:clearable="false"
										value-type="timestamp"
										range
										:placeholder="$t('TableHeader.__createTime2')"
									>
									</date-picker>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3" class="v-col-padding-0">
								<div class="mt-4">
									<v-chip class="mr-1" :class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'today' }" @click="globalQuicklyPickTimeRange('today')">
										<v-icon left small>mdi-alarm-check</v-icon>
										{{ $t(`TableHeader.__today`) }}
									</v-chip>
									<v-chip class="mr-1" :class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'yesterday' }" @click="globalQuicklyPickTimeRange('yesterday')">
										<v-icon left small>mdi-alarm-check</v-icon>
										{{ $t(`TableHeader.__yesterday`) }}
									</v-chip>
									<v-chip class="mr-1" :class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'thisWeek' }" @click="globalQuicklyPickTimeRange('thisWeek')">
										<v-icon left small>mdi-alarm-check</v-icon>
										{{ $t(`TableHeader.__thisWeek`) }}
									</v-chip>

									<v-chip class="mr-1" :class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'lastWeek' }" @click="globalQuicklyPickTimeRange('lastWeek')">
										<v-icon left small>mdi-alarm-check</v-icon>
										{{ $t(`TableHeader.__lastWeek`) }}
									</v-chip>
									<v-chip class="mr-1" :class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'thisMonth' }" @click="globalQuicklyPickTimeRange('thisMonth')">
										<v-icon left small>mdi-alarm-check</v-icon>
										{{ $t(`TableHeader.__thisMonth`) }}
									</v-chip>
									<v-chip class="mr-1" :class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'lastMonth' }" @click="globalQuicklyPickTimeRange('lastMonth')">
										<v-icon left small>mdi-alarm-check</v-icon>
										{{ $t(`TableHeader.__lastMonth`) }}
									</v-chip>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3" class="v-col-padding-0">
								<div class="mt-2 search-row-padding">
									<v-btn color="yellow darken-1" text class="btn search mr-1" @click.stop="search">
										<v-icon left>search</v-icon>
										{{ $t('Button.__search') }}
									</v-btn>

									<v-btn color="pink lighten-3" text class="btn search mr-1" @click.stop="clear">
										<v-icon left>clear</v-icon>
										{{ $t('Button.__reset') }}
									</v-btn>

									<v-btn color="light-blue lighten-3" text class="btn search mr-1" @click.stop="showExportConfirm">
										<v-icon left>cloud_download</v-icon>
										{{ $t('Button.__downloadFile') }}
									</v-btn>

									<v-btn color="purple lighten-2" text class="btn search mt-1" v-if="hasSuperAdminPermission" @click.stop="showManualSendEventPrize">
										<v-icon left>mdi-gift</v-icon>
										派獎測試 </v-btn
									>&#9;
								</div>
							</v-col>
						</v-row>
					</v-flex>
				</v-expansion-panel-content>
			</v-expansion-panel>
		</v-expansion-panels>

		<div>
			<div style="float: left; padding: 5px">
				<v-icon color="grey" small dark style="font-size: 18px"> error </v-icon>
				<span style="color: #ccc; font-size: 12px; vertical-align: middle">
					{{ $t('Message.__dataKeepXMonthOnly', { month: 3 }) }}
				</span>

				<v-icon color="grey" class="ml-4" small dark style="font-size: 18px"> error </v-icon>
				<span style="color: #ccc; font-size: 12px; vertical-align: middle"> {{ $t('Message.__accountingBaseOnColumn') }}《{{ $t('TableHeader.__receiveTime') }}》 </span>
			</div>
		</div>

		<v-data-table
			:headers="headers"
			id="table"
			:items="searchList"
			:page="DataTablePagingParameter.pageIndex"
			:items-per-page="DataTablePagingParameter.itemsPerPage"
			:server-items-length="DataTablePagingParameter.totalCount"
			:footer-props="DataTablePagingParameter.footerProps"
			@update:page="changePage"
			@update:items-per-page="changeItemPerPage"
		>
			<template v-slot:item.action="{ item }">
				<!-- <v-icon small class="mr-2" style="color:#CCC;font-size:20px;"
					@click="showInfo(item.eventID)">
					info
				</v-icon> -->
			</template>

			<template v-slot:item.playerName="{ item }">
				<div class="link-text-no-under" @click="openDetail(item.playerID)">
					{{ item.playerName }}
				</div>

				<div class="sub-title-text">
					{{ item.playerID }}
				</div>
			</template>

			<template v-slot:item.amountOrQuantity="{ item }">
				<div v-if="item.isAmountRequired">
					{{ item.amountOrQuantity }}
				</div>

				<div v-if="!item.isAmountRequired">-</div>
			</template>

			<template v-slot:item.updateAmount="{ item }">
				<div class="text-r" v-if="item.updateAmount && item.updateAmount != null">
					{{ item.updateAmount | amountDisplay }}
				</div>
				<div v-else class="text-r">
					<span v-if="item.eventPrizeType == 650"> 0 </span>
					<span v-else>- </span>
				</div>
			</template>

			<template v-slot:item.referenceNo="{ item }">
				<div :title="'pgsTransactionNo & ' + item.id">
					{{ item.pgsTransactionNo }}
				</div>
				<div class="sub-title-text" :title="'ReferNo / thirdOrderNo'">
					{{ item.displayOrderNo2 }}
				</div>
			</template>

			<template v-slot:item.eventPrizeTypeName="{ item }">
				<div :title="item.eventPrizeType">
					{{ item.eventPrizeTypeName }}
				</div>
				<div class="sub-title-text" :title="item.eventID">
					{{ item.eventName }}
				</div>
			</template>

			<template v-slot:item.gameName="{ item }">
				<div v-if="item.gameID > 0">
					<div :title="item.gameID">
						{{ item.gameName }}
					</div>
					<div class="sub-title-text">
						{{ item.gameID }}
					</div>
				</div>
			</template>

			<template v-slot:item.merchantName="{ item }">
				<div :title="item.merchantID">
					{{ item.merchantName }}
				</div>
				<div class="sub-title-text">
					{{ item.merchantID }}
				</div>
			</template>

			<template v-slot:item.statusText="{ item }">
				<span :class="statusColorStyle(item.status)" :title="item.status">
					{{ item.statusText }}
				</span>
			</template>

			<template v-slot:item.createTime="{ item }">
				<span class="f14">{{ item.createTime | timeWithAddTimezone(userProfile.timeZone) }}</span>
			</template>

			<template v-slot:item.receiveTime="{ item }">
				{{ item.receiveTime | timeWithAddTimezone(userProfile.timeZone) }}
			</template>

			<template v-slot:item.notifyQueueID="{ item }">
				<span class="link-text text-c" v-if="item.eventPrizeType === 115 || item.eventPrizeType === 40 || item.eventPrizeType === 650" @click="showBetRecordDetailPopup(item.referenceNo)">
					{{ $t('Button.__showDetail') }} ({{ $t('TableHeader.__wager') }})
				</span>

				<div v-if="hasSuperAdminPermission">
					<span v-if="item.isSingleWallet">
						<span @click="openSingleWalletLogHistoryPage(item.pgsTransactionNo, toDisplayTime(item.receiveTime))" class="link-text-grey">
							{{ item.notifyQueueID && item.notifyQueueID > 0 ? '需確認log' : '成功' }}
						</span>
					</span>
				</div>
			</template>
		</v-data-table>

		<!-- 匯出前確認 -->
		<v-dialog v-model="isShowExportConfirmDialog" max-width="400px">
			<v-card v-if="isShowExportConfirmDialog">
				<v-card-title class="grey darken-2">
					{{ $t('Message.__isConfirmOperate') }}? ({{ $t('Button.__downloadFile') }})

					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>

				<v-card-text style="min-height: 200px; padding: 10px">
					<table class="ez-table">
						<tr>
							<td>{{ $t('TableHeader.__lang') }}</td>
							<td class="yellow-text">
								{{ showMyLanguage() }}
							</td>
						</tr>
						<tr>
							<td>{{ $t('TableHeader.__timeZoneArea') }}</td>
							<td class="yellow-text">UTC+{{ userProfile.timeZone }}</td>
						</tr>

						<tr>
							<td>{{ $t('TableHeader.__extraFilterType') }}({{ $t('TableHeader.__merchant') }})</td>
							<td class="">
								<span :class="searchFilter.merchantID > 0 ? 'yellow-text' : 'red-text'">
									{{ showCurrectSelectedMerchantText() }}
								</span>
								<v-icon v-if="!(searchFilter.merchantID > 0)" small color="red">error</v-icon>
							</td>
						</tr>

						<tr>
							<td>{{ $t('TableHeader.__extraFilterType') }}({{ $t('TableHeader.__status') }})</td>
							<td class="">
								<span :class="searchFilter.status > 0 ? 'yellow-text' : 'red-text'">
									{{ showCurrectSelectedStatusFilterText() }}
								</span>
								<v-icon v-if="!(searchFilter.status > 0)" small color="red">error</v-icon>
							</td>
						</tr>

						<tr>
							<td>{{ $t('TableHeader.__time') }}</td>
							<td class="white-text">
								{{ convertToLocalDateString(datetimeRangePicker.searchStartTimeRange[0]) }}
								~
								{{ convertToLocalDateString(datetimeRangePicker.searchStartTimeRange[1]) }}
							</td>
						</tr>
						<tr>
							<td>{{ $t('TableHeader.__maxExportSize') }}</td>
							<td class="white-text">
								{{ maxExportSize }}
							</td>
						</tr>
					</table>
				</v-card-text>

				<v-card-actions>
					<v-spacer></v-spacer>

					<v-btn color="light-blue lighten-3" text class="btn" @click.stop="exportFiles">
						<v-icon left>cloud_download</v-icon>
						{{ $t('Button.__downloadFile') }} </v-btn
					>&#9;
				</v-card-actions>
			</v-card>
		</v-dialog>

		<v-dialog v-model="isShowExportDataValidateDialog" max-width="500px">
			<v-card>
				<v-card-title class="grey darken-2">
					Export Validate
					<span class="ml-2 grey-text f12">({{ getTargetMerchantNameByDropdown(searchFilter.merchantID) }}) </span>

					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>

				<v-card-text>
					<div class="text-c mt-2">
						<div v-if="exportResultValidate.totalAmount_StatDB == exportResultValidate.totalAmount_Export">
							<v-icon color="green" x-large>check</v-icon>

							<div class="f14">數字匹配</div>
						</div>

						<div v-else>
							<v-icon color="red" x-large>close</v-icon>
							<div class="f14">數字不相符 (Data compare fail)</div>
						</div>

						<div class="f10 grey-text">{{ exportResultValidate.beginText }} - {{ exportResultValidate.endText }}</div>
					</div>

					<div style="padding: 15px; min-height: 300px" class="mt-2">
						<table class="ez-table">
							<tr>
								<td class="text-r yellow-text">{{ $t('TableHeader.__category') }}</td>
								<td class="text-r yellow-text">{{ $t('TableHeader.__totalCount') }}</td>
								<td class="text-r yellow-text">{{ $t('TableHeader.__totalAmount') }}</td>
							</tr>

							<tr>
								<td class="text-r">Excel Export</td>
								<td class="text-r">{{ exportResultValidate.totalCount_Export | countDisplay }}</td>
								<td class="text-r">{{ exportResultValidate.totalAmount_Export | amountDisplay }}</td>
							</tr>

							<tr>
								<td class="text-r">{{ $t('Menu.Statistics') }}</td>
								<td class="text-r">{{ exportResultValidate.totalCount_StatDB | countDisplay }}</td>
								<td class="text-r">{{ exportResultValidate.totalAmount_StatDB | amountDisplay }}</td>
							</tr>
						</table>

						<div class="mt-4" v-if="exportResultValidate.totalAmount_StatDB == exportResultValidate.totalAmount_Export">
							<div class="f14">
								<v-icon color="" small>info</v-icon>
								檔案已開始下載 <span class="f12 orange-text" style="text-">{{ lastExportFileName }}</span>
							</div>

							<div class="f14">
								<v-icon color="" small>info</v-icon>
								系統將比對此下載與統計數據是否一致
							</div>
						</div>

						<div class="mt-4 minus-red" v-if="exportResultValidate.totalAmount_StatDB != exportResultValidate.totalAmount_Export">
							<div>
								<v-icon color="red" small>info</v-icon>
								<span class=""> If you see validate fail. please contact tech team. </span>
							</div>

							<div>
								<v-icon color="red" small>info</v-icon>
								<span class=""> Please screenshot this information. </span>
							</div>
						</div>
					</div>
				</v-card-text>
			</v-card>
		</v-dialog>

		<!-- 手動測試派發獎勵 -->
		<v-dialog v-model="isShowManualSendEventPrize" max-width="500px">
			<v-card>
				<v-card-title class="grey darken-2">
					手動派發獎勵測試 (僅供測試商戶使用)

					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>

				<v-card-text style="min-height: 200px">
					<v-row>
						<v-col cols="12" sm="12" md="12">
							<v-text-field :label="'玩家ID'" v-model="manualSendPrizeForm.playerID"> </v-text-field>
						</v-col>
					</v-row>

					<v-row>
						<v-col cols="12" sm="12" md="12">
							<v-autocomplete :label="'指定活動'" :items="dropdownItemsEvents" v-model="manualSendPrizeForm.eventID"> </v-autocomplete>
						</v-col>
					</v-row>
				</v-card-text>

				<v-card-actions>
					<v-spacer></v-spacer>

					<v-btn color="grey lighten-1" text @click="closeAllModal" class="btn">
						<v-icon left>mdi-close</v-icon>
						{{ $t('Button.__cancel') }}
					</v-btn>

					<v-btn color="yellow darken-1" text @click="submitManualSendEventPrize" class="btn">
						<v-icon left>mdi-check-circle</v-icon>
						{{ $t('Button.__submit') }}
					</v-btn>
				</v-card-actions>
			</v-card>
		</v-dialog>

		<!-- 三方注單資訊 & 產生外部遊戲快照連結 -->
		<v-dialog v-model="isShowExternalLinkDialog" max-width="900px">
			<PerGameThirdPartyDetailPopup ref="refPerGameThirdPartyDetailPopup" @closeParentAllModal="closeAllModal"> </PerGameThirdPartyDetailPopup>
		</v-dialog>

		<NotifyQueueDetailPopup ref="refNotifyQueueDetailPopup"></NotifyQueueDetailPopup>
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import { datetimeMixin, startEndDate, timeWithTimezone, daysAgo, toLocalDateMMdd, toLocalDateString, weeksAgo, monthsAgo, formatMixin } from '@/utilities/datetime-format';
import EventBus from '@/utilities/event-bus';
import * as Model from '@/models/interfaces/system-setting/merchants';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import * as EventModel from '@/models/interfaces/system-setting/events';
import MerchantsApi from '@/api/system-setting/merchants';
import EventsApi from '@/api/system-setting/events';
import DropdownApi from '@/api/commons/dropdown';
import Cookies from 'js-cookie';
import DatePicker from 'vue2-datepicker';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { commonMethodMixin } from '@/utilities/common-methods';
import NotificationWarning from '@/components/notification/NotificationWarning.vue';
import NotifyQueueDetailPopup from '@/components/game-management/NotifyQueueDetailPopup.vue';
import PerGameThirdPartyDetailPopup from '@/components/game-management/PerGameThirdPartyDetailPopup.vue';
import { pageData, time } from '@/utilities/api-handler.lib';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import { State, Action, Getter, namespace } from 'vuex-class';
const authModule = namespace('Auth');
const playersModule = namespace('Players');
import * as Permission from '@/models/status/permission';
import { uuid } from 'vue-uuid';
import { filter } from 'vue/types/umd';

// @ts-ignore
@Component({ components: { NotificationWarning, DatePicker, NotifyQueueDetailPopup, PerGameThirdPartyDetailPopup }, mixins: [commonDisplayFiltersMixin, commonMethodMixin, datetimeMixin] })
export default class EventPrizeRecord extends Vue {
	get hasPermissionCreate() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSettingMerchant_Create) >= 0;
	}
	get hasPermissionUpdate() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSettingMerchant_Update) >= 0;
	}
	get hasPermissionDelete() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSettingMerchant_Delete) >= 0;
	}

	get hasInternalStaffPermission() {
		return this.hasSuperAdminPermission || this.hasCustomerServicePermission;
	}

	get hasSuperAdminPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSetting_SystemConfigSetting) >= 0;
	}
	get hasCustomerServicePermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.Special_PGS_CustomerService) >= 0;
	}

	get hasRestrictMaxSizePermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.Special_ExportMaxSizeRestrict) >= 0;
	}

	get headers() {
		const data = [
			{ text: this.$t('TableHeader.__action'), value: 'notifyQueueID', sortable: false, width: '5%' },
			{ text: this.$t('TableHeader.__createTime'), value: 'createTime', sortable: false },
			{ text: this.$t('TableHeader.__receiveTime'), value: 'receiveTime', sortable: false },
			{ text: this.$t('TableHeader.__merchant'), value: 'merchantName', sortable: false },
			{ text: this.$t('TableHeader.__playerAccount'), value: 'playerName', sortable: false },
			// { text: this.$t('TableHeader.__eventName'), value: 'eventName', sortable: false },
			{ text: this.$t('TableHeader.__type') + '／' + this.$t('TableHeader.__eventName'), value: 'eventPrizeTypeName' },
			{ text: this.$t('TableHeader.__amountOrQuantity'), value: 'amountOrQuantity', align: 'end', sortable: false },
			{ text: this.$t('TableHeader.__updateAmount'), value: 'updateAmount', align: 'end', sortable: false },
			{ text: this.$t('TableHeader.__status'), value: 'statusText', sortable: false },
			{ text: this.$t('TableHeader.__gameName'), value: 'gameName', width: '8%', sortable: false },
			{ text: this.$t('TableHeader.__transactionNo') + '／' + this.$t('TableHeader.__referenceNo'), value: 'referenceNo', sortable: false },
			{ text: this.$t('TableHeader.__memo'), value: 'memo', sortable: false },
		];

		return data;
	}

	get maxExportSize() {
		if (this.hasRestrictMaxSizePermission) {
			return 10000;
		} else {
			return 20000;
		}
	}

	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];
	@playersModule.State('prizeRecordPlayerID') prizeRecordPlayerID!: number;

	dialog: boolean = false;
	isShowCopyHint: boolean = false;
	isShowAdvanceSetting: boolean = false;
	isShowExportConfirmDialog: boolean = false;
	isShowManualSendEventPrize: boolean = false;
	isShowExportDataValidateDialog: boolean = false;
	isShowExternalLinkDialog: boolean = false;

	renderComponent: boolean = true;
	searchTargetField: string = '1';
	myInputKeyword: string = '';

	DataTablePagingParameter: DropdownModel.DataTablePagingParameter = new DropdownModel.DataTablePagingParameter();
	datetimeRangePicker: DropdownModel.DatetimeRangePicker = new DropdownModel.DatetimeRangePicker();
	searchList: EventModel.IPrizeRecordList[] = [];
	searchFilter: EventModel.IPrizeRecordRequest = {} as EventModel.IPrizeRecordRequest;
	manualSendPrizeForm: EventModel.IManualSendEventPrizeRequest = {} as EventModel.IManualSendEventPrizeRequest;
	exportResultValidate: EventModel.IExportPrizeValidateResult = {} as EventModel.IExportPrizeValidateResult;

	rand: number = 0;
	panel: number[] = [0];
	editModeOpenArray: number[] = [];
	merchantsData: Model.IMerchant[] = [];
	lastExportFileName: string = '';

	dropdownItemsEvents: DropdownModel.IDropdownItems[] = [];
	dropdownItemsEventPrizeTypes: DropdownModel.IDropdownItems[] = [];
	dropdownItemsPrizeRecordStatus: DropdownModel.IDropdownItems[] = [];
	dropdownItemsMerchants: DropdownModel.IDropdownItems[] = [];
	dropdownItemsVendorConfigs: DropdownModel.IDropdownItems[] = [];
	dropdownItemsGames: DropdownModel.IDropdownItems[] = [];

	@Action('Players/setEventID') private setEventID!: any;
	@Action('Players/setPlayerID') private setPlayerID!: any;

	@Watch('prizeRecordPlayerID')
	onPrizeRecordPlayerIDChanged(newVal: number) {
		this.searchFilter.playerID = newVal.toString();
		if (this.searchFilter.playerID && this.searchFilter.playerID !== '') {
			this.searchTargetField = '1';
			this.search();
		}
	}

	get searchTargetFieldItems() {
		return [
			{ text: this.$t('TableHeader.__playerAccount'), value: '1' },
			{ text: this.$t('TableHeader.__playerID') + ' (SystemUniqueID)', value: '2' },
			{ text: this.$t('TableHeader.__referenceNo'), value: '3' },
		];
	}

	get TimeColumnDropdown() {
		return [
			{ text: this.$t('TableHeader.__createTime'), value: 0 },
			{ text: this.$t('TableHeader.__receiveTime'), value: 1 },
		];
	}

	openDetail(playerID: number) {
		this.setPlayerID(playerID);
		this.$router.push({ path: '/members-management/detail' });
	}

	statusColorStyle(status: number) {
		if (status === 9) {
			return 'minus-red';
		}

		if (status === 13) {
			return 'greenStatus';
		}

		if (status === 11) {
			return 'yellowStatus';
		}

		return '';
	}

	getTargetMerchantNameByDropdown(merchantID: number) {
		const match = this.dropdownItemsMerchants.filter((x) => x.valueInt === merchantID);

		if (match.length > 0) {
			return match[0].text;
		}

		return '';
	}

	showInfo(eventID: number) {
		this.setEventID(eventID);
		this.$router.push({ path: '/system-setting/event-detail' });
	}

	changePage(index: number = 1) {
		this.DataTablePagingParameter.pageIndex = index;
		this.refresh();
	}

	changeItemPerPage(val: number = 10) {
		this.DataTablePagingParameter.itemsPerPage = val;
		this.refresh();
	}

	search() {
		this.DataTablePagingParameter.pageIndex = 1;
		this.refresh();
	}

	showBetRecordDetailPopup(betRecordNoPGS: string) {
		this.isShowExternalLinkDialog = true;

		this.$nextTick(() => {
			const myRef: any = this.$refs.refPerGameThirdPartyDetailPopup;
			myRef.getExternalLinkDialogInfoByNo(betRecordNoPGS);
		});
	}

	showNotifyLogs(queueID: number) {
		this.$nextTick(() => {
			const myRef: any = this.$refs.refNotifyQueueDetailPopup;
			myRef.showNotifyLogs(queueID);
		});
	}

	convertToLocalDateString(myTime: number) {
		return toLocalDateString(myTime, this.userProfile.timeZone);
	}

	showCurrectSelectedStatusFilterText() {
		if (this.searchFilter.status > 0) {
			return this.dropdownItemsPrizeRecordStatus.filter((z) => z.valueInt === this.searchFilter.status)[0].text;
		} else {
			return 'None';
		}
	}

	showCurrectSelectedMerchantText() {
		if (this.searchFilter.merchantID > 0) {
			return this.dropdownItemsMerchants.filter((z) => z.valueInt === this.searchFilter.merchantID)[0].text;
		} else {
			return 'None';
		}
	}

	showMyLanguage() {
		const myLang = Cookies.get('locale');

		if (myLang === 'zh-tw') {
			return '繁体中文';
		}

		if (myLang === 'zh-cn') {
			return '简体中文';
		}

		if (myLang === 'en-us' || myLang === 'en') {
			return 'English';
		}

		return myLang;
	}

	showExportConfirm() {
		this.isShowExportConfirmDialog = true;
	}

	instanceSearchtFilter(pageIndex: number, pageSize: number) {
		const myFilter = { ...this.searchFilter };
		myFilter.pageIndex = pageIndex;
		myFilter.pageSize = pageSize;
		myFilter.isASC = false;

		myFilter.createTimeBegin = null;
		myFilter.createTimeEnd = null;
		myFilter.receiveTimeBegin = null;
		myFilter.receiveTimeEnd = null;

		myFilter.playerID = null;
		myFilter.playerName = null;
		myFilter.multipleOrderColumnSearch = null;

		if (this.searchTargetField === '1') {
			myFilter.playerName = this.myInputKeyword;
		} else if (this.searchTargetField === '2') {
			myFilter.playerID = this.myInputKeyword;
		} else if (this.searchTargetField === '3') {
			myFilter.multipleOrderColumnSearch = this.myInputKeyword;
		}

		if (this.searchFilter.timeColumn === 0) {
			myFilter.createTimeBegin = this.datetimeRangePicker.searchStartTimeRange[0] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[0]) : null;
			myFilter.createTimeEnd = this.datetimeRangePicker.searchStartTimeRange[1] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[1]) : null;
		} else {
			myFilter.receiveTimeBegin = this.datetimeRangePicker.searchStartTimeRange[0] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[0]) : null;
			myFilter.receiveTimeEnd = this.datetimeRangePicker.searchStartTimeRange[1] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[1]) : null;
		}

		return myFilter;
	}

	refresh() {
		const myFilter = this.instanceSearchtFilter(this.DataTablePagingParameter.pageIndex, this.DataTablePagingParameter.itemsPerPage);

		EventBus.$emit('show_mask', null);

		EventsApi.getPrizeRecords(myFilter)
			.then((res) => {
				this.searchList = res.data.items;
				this.DataTablePagingParameter.totalCount = res.data.totalCount;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	openSingleWalletLogHistoryPage(transactionNo: string, createTime: string) {
		EventBus.$emit('close_tab', '/system-logs/SingleWalletPlatformReqeustLog');
		const query = '?referenceTransactionNo=' + transactionNo + '&begin=' + createTime;
		this.$router.push({ path: '/system-logs/SingleWalletPlatformReqeustLog' + query });
	}

	async exportFiles() {
		const myTimeZone: string = this.userProfile.timeZone.toString();

		const excelHeaders: any = [
			this.geti18nTextAsString('TableHeader.__createTime') + `(UTC+${myTimeZone})`,
			this.geti18nTextAsString('TableHeader.__receiveTime') + `(UTC+${myTimeZone})`,
			this.geti18nTextAsString('TableHeader.__merchant'),
			this.geti18nTextAsString('TableHeader.__playerID'),
			this.geti18nTextAsString('TableHeader.__playerAccount'),
			this.geti18nTextAsString('TableHeader.__eventName'),
			this.geti18nTextAsString('TableHeader.__type'),
			this.geti18nTextAsString('TableHeader.__amountOrQuantity'),
			this.geti18nTextAsString('TableHeader.__updateAmount'),
			this.geti18nTextAsString('TableHeader.__status'),
			this.geti18nTextAsString('TableHeader.__gameID'),
			this.geti18nTextAsString('TableHeader.__gameName'),
			this.geti18nTextAsString('TableHeader.__referenceInfo'),
			this.geti18nTextAsString('TableHeader.__transactionNo'),
			this.geti18nTextAsString('TableHeader.__referenceNo') + '(1)',
		];

		const excelData: any = [];

		const myFilter = this.instanceSearchtFilter(1, this.maxExportSize);

		EventBus.$emit('show_mask', null);

		const result = await EventsApi.getPrizeRecords(myFilter).finally(() => {
			EventBus.$emit('hide_mask', null);
		});

		result.data.items.forEach((value) => {
			const eachRow: any = [];
			eachRow.push(timeWithTimezone(value.createTime, this.userProfile.timeZone));

			if (value.receiveTime != null) {
				eachRow.push(timeWithTimezone(value.receiveTime, this.userProfile.timeZone));
			} else {
				eachRow.push('-');
			}

			eachRow.push(value.merchantName);
			eachRow.push(value.playerID);
			eachRow.push(value.playerName);
			eachRow.push(value.eventName);
			eachRow.push(value.eventPrizeTypeName);
			eachRow.push(value.amountOrQuantity);
			eachRow.push(value.updateAmount);
			eachRow.push(value.statusText);
			eachRow.push(value.gameID);
			eachRow.push(value.gameName);
			eachRow.push(value.memo);
			eachRow.push(value.pgsTransactionNo);
			eachRow.push(value.displayOrderNo2);

			excelData.push(eachRow);
		});

		if (result.data.extraObject && result.data.extraObject.totalSummary && result.data.extraObject.totalSummary != null) {
			const totalSum: any = result.data.extraObject.totalSummary;
			const eachRow: any = [];
			eachRow.push('');
			eachRow.push('');
			eachRow.push('');
			eachRow.push('');
			eachRow.push('');
			eachRow.push('');
			eachRow.push('');
			eachRow.push('');
			eachRow.push(totalSum.updateAmount);
			eachRow.push('');
			eachRow.push('');
			eachRow.push('');
			eachRow.push('');
			eachRow.push('');
			eachRow.push('');
			excelData.push(eachRow);
		}

		// @ts-ignore
		const beginDateText: string = toLocalDateMMdd(this.datetimeRangePicker.searchStartTimeRange[0], this.userProfile.timeZone).replaceAll('-', '');
		// @ts-ignore
		const endDateText: string = toLocalDateMMdd(this.datetimeRangePicker.searchStartTimeRange[1], this.userProfile.timeZone).replaceAll('-', '');
		let fileNameExtraTag: string = '_' + beginDateText + '_' + endDateText;

		if (this.searchFilter.merchantID > 0) {
			fileNameExtraTag = fileNameExtraTag + '_' + this.searchFilter.merchantID.toString();
		}

		this.lastExportFileName = 'Export_PrizeRecord' + fileNameExtraTag + '.xlsx';

		const config = {
			headers: excelHeaders,
			rowData: excelData,
			fileName: this.lastExportFileName,
		};

		// @ts-ignore
		this.downloadExcel(config);

		// 檢查數據正確性
		if (this.hasInternalStaffPermission && myFilter.merchantID > 0) {
			if (result.data.extraObject && result.data.extraObject.totalSummary && result.data.extraObject.totalSummary != null && result.data.extraObject.totalSummary.updateAmount > 0) {
				this.checkExportDataValid(myFilter, result.data.items.length, result.data.extraObject.totalSummary.updateAmount);
			}
		}
	}

	checkExportDataValid(myFilter: EventModel.IPrizeRecordRequest, totalCount: number, totalAmount: number) {
		const payload = {} as EventModel.IExportPrizeValidateRequest;
		payload.merchantID = myFilter.merchantID;

		if (myFilter.receiveTimeEnd != null) {
			payload.begin = myFilter.receiveTimeBegin;
			payload.end = myFilter.receiveTimeEnd;
		} else {
			payload.begin = myFilter.createTimeBegin;
			payload.end = myFilter.createTimeEnd;
		}

		payload.totalCount = totalCount;
		payload.totalAmount = totalAmount;
		payload.timeZone = this.userProfile.timeZone;

		const result = EventsApi.checkExportPrizeValid(payload).then((res) => {
			this.exportResultValidate = res.data;
			this.isShowExportDataValidateDialog = true;
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

	closeAllModal() {
		this.isShowAdvanceSetting = false;
		this.isShowCopyHint = false;
		this.isShowExportConfirmDialog = false;
		this.isShowManualSendEventPrize = false;
		this.isShowExportDataValidateDialog = false;
		this.isShowExternalLinkDialog = false;
		this.dialog = false;
	}

	geti18nTextAsString(key: string) {
		return this.$t(key).toString();
	}

	toDisplayTime(isoTime: string) {
		return timeWithTimezone(isoTime, this.userProfile.timeZone);
	}

	clear() {
		this.searchFilter = {} as EventModel.IPrizeRecordRequest;
		this.searchFilter.timeColumn = 0;

		this.searchTargetField = '1';
		this.myInputKeyword = '';
	}

	copySuccess() {
		EventBus.$emit(`successfully-copy`);
	}

	showManualSendEventPrize() {
		this.isShowManualSendEventPrize = true;
		this.manualSendPrizeForm = {} as EventModel.IManualSendEventPrizeRequest;
	}

	submitManualSendEventPrize() {
		EventBus.$emit('show_mask', null);
		EventsApi.submitManualSendEventPrize(this.manualSendPrizeForm)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.refresh();
				this.closeAllModal();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	setDefaultFilterByQuerystringAndSearch() {
		const routeQuery: any = this.$route.query;

		if (this.prizeRecordPlayerID > 0) {
			this.searchTargetField = '1';
			this.searchFilter.playerID = this.prizeRecordPlayerID.toString();
		}

		if (routeQuery) {
			if (routeQuery.eventID) {
				this.searchFilter.eventID = routeQuery.eventID;
			}
			console.log('eventID:' + this.searchFilter.eventID);
			this.$nextTick(() => {
				setTimeout(() => {
					// 強制重新render
					this.renderComponent = false;
					this.renderComponent = true;
					this.refresh();
				}, 500);
			});
		}
	}

	activated() {
		this.setDefaultFilterByQuerystringAndSearch();
	}
	mounted() {
		this.registerReloadComponent();

		this.clear();

		this.datetimeRangePicker.searchStartTimeRange = [daysAgo(-14), daysAgo(1)];

		DropdownApi.getEvents().then((res) => {
			this.dropdownItemsEvents = res;
		});

		DropdownApi.getGames().then((res) => {
			this.dropdownItemsGames = res;
		});

		DropdownApi.getEventPrizeTypes().then((res) => {
			this.dropdownItemsEventPrizeTypes = res;
		});

		DropdownApi.getMerchants().then((res) => {
			this.dropdownItemsMerchants = res;
		});

		DropdownApi.getPrizeRecordStatus().then((res) => {
			this.dropdownItemsPrizeRecordStatus = res;
		});

		DropdownApi.getVendorConfig().then((res) => {
			this.dropdownItemsVendorConfigs = res;
		});

		this.setDefaultFilterByQuerystringAndSearch();
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

::-webkit-scrollbar {
	-webkit-appearance: none;
	width: 7px;
}
::-webkit-scrollbar-thumb {
	border-radius: 4px;
	background-color: rgba(0, 0, 0, 0.5);
	-webkit-box-shadow: 0 0 1px rgba(255, 255, 255, 0.5);
}

.greenStatus {
	color: #4dc76c;
}

.yellowStatus {
	color: #dbe24a;
}
</style>
