<template>
	<v-app v-if="renderComponent" class="app">
		<v-expansion-panels v-model="panel" multiple v-if="reloadSearchFilterFlag">
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
									>
									</v-text-field>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-text-field v-model="searchData.playerName" :label="$t('TableHeader.__playerAccount')" append-icon="person" clearable> </v-text-field>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item merchantDropdownItemsWrapper">
									<v-autocomplete clearable :items="merchantDropdownItems" class="merchantDropdownItems" v-model="searchData.merchantID" :label="$t('TableHeader.__merchant')">
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
									<v-text-field v-model="searchData.TransactionNoOrThirdNo" :label="$t('TableHeader.__betRecordNoPGS')" append-icon="description" clearable> </v-text-field>
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
									<v-autocomplete
										clearable
										:items="gamesDropdownWithKeywordFilter"
										no-filter
										:no-data-text="$t('Message.__pleaseInputKeywordToSearchDropdown')"
										:search-input.sync="globalKeywordSearchGamesDropdown"
										v-model="searchData.gameID"
										:label="$t('TableHeader.__game')"
									>
										<template slot="selection" slot-scope="data">
											{{ data.item.text }}
										</template>
									</v-autocomplete>
								</div>
							</v-col>
							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete clearable :items="serversDropdownItems" v-model="searchData.serverID" :label="$t('TableHeader.__server')"> </v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-text-field
										v-model="searchData.perGameDetailLogID"
										:label="'betID' + (hasSystenAdminPermission ? ' (DetailLogID)' : ' (API)')"
										append-icon="description"
										clearable
									>
									</v-text-field>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-text-field
										v-model="searchData.perGameLogID"
										:label="'(' + $t('Message.__noLongerSupport') + ') ' + $t('TableHeader.__perGameLogID')"
										append-icon="games"
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

							<v-col cols="12" sm="3" md="3" class="quick-time">
								<!-- <div class="item"> -->
								<v-chip class="mr-2" small :class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'today' }" @click="globalQuicklyPickTimeRange('today')">
									<v-icon left small>mdi-alarm-check</v-icon>
									{{ $t(`TableHeader.__today`) }}
								</v-chip>
								<v-chip class="mr-2" small :class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'yesterday' }" @click="globalQuicklyPickTimeRange('yesterday')">
									<v-icon left small>mdi-alarm-check</v-icon>
									{{ $t(`TableHeader.__yesterday`) }}
								</v-chip>
								<v-chip class="mr-2" small :class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'thisWeek' }" @click="globalQuicklyPickTimeRange('thisWeek')">
									<v-icon left small>mdi-alarm-check</v-icon>
									{{ $t(`TableHeader.__thisWeek`) }}
								</v-chip>

								<v-chip class="mr-2" small :class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'lastWeek' }" @click="globalQuicklyPickTimeRange('lastWeek')">
									<v-icon left small>mdi-alarm-check</v-icon>
									{{ $t(`TableHeader.__lastWeek`) }}
								</v-chip>
								<v-chip
									class="mr-2"
									small
									v-if="hasSystenAdminPermission"
									:class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'thisMonth' }"
									@click="globalQuicklyPickTimeRange('thisMonth')"
								>
									<v-icon left small>mdi-alarm-check</v-icon>
									{{ $t(`TableHeader.__thisMonth`) }}
								</v-chip>
								<v-chip
									class="mr-2"
									small
									v-if="hasSystenAdminPermission"
									:class="{ pink: datetimeRangePicker.quicklyPickTimeRangeText === 'lastMonth' }"
									@click="globalQuicklyPickTimeRange('lastMonth')"
								>
									<v-icon left small>mdi-alarm-check</v-icon>
									{{ $t(`TableHeader.__lastMonth`) }}
								</v-chip>
								<!-- </div> -->
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete clearable :items="perGameTagDropdownItems" v-model="searchData.tagID" :label="$t('TableHeader.__tag')"> </v-autocomplete>
								</div>
							</v-col>
							<v-col cols="12" sm="3" md="3">
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
					</v-flex>
				</v-expansion-panel-content>
			</v-expansion-panel>
		</v-expansion-panels>

		<div>
			<div style="float: left; padding: 5px 15px" class="do_not_show">
				<v-switch :label="$t('Button.__showExtraInfo')" v-model="isShowExtraColumnInfo" color="yellow darken-3" class="mt-0"> </v-switch>
			</div>

			<div style="float: right; padding: 5px">
				<v-icon color="grey" dark style="font-size: 20px"> error </v-icon>
				<span style="color: #ccc; font-size: 12px; vertical-align: middle">
					{{ $t('Message.__dataKeepXMonthOnly', { month: 3 }) }}
				</span>

				<v-icon color="grey" dark style="font-size: 20px"> error </v-icon>
				<span style="color: #ccc; font-size: 12px; vertical-align: middle">
					{{ $t('Message.__singleWalletRemarkInfo') }}
				</span>

				<v-icon color="grey" class="ml-3" style="font-size: 20px"> visibility </v-icon>
				<span style="color: #ccc; font-size: 12px; vertical-align: middle">
					{{ $t('TableHeader.__playerViewPoint') }}
				</span>

				<!-- 
				<v-icon color="grey" class="ml-2" style="font-size:20px;">error</v-icon>
				<span style="color:#CCC;font-size:12px;vertical-align: middle;">
					{{ $t('Message.__dataKeep2MonthOnly') }}
				</span> -->
			</div>
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
			<template v-slot:item.perGameDetailLogID="{ item }">
				<div class="text-c">
					<v-menu offset-y>
						<template v-slot:activator="{ on, attrs }">
							<div class="dropdown-menu" v-bind="attrs" v-on="on">
								<v-icon dark outline> expand_more </v-icon>
							</div>
						</template>

						<v-list class="dropdonwMenuPanel">
							<v-list-item class="dropdownMenu">
								<v-list-item v-if="!item.isExternalVendorGame" @click="openPerGameLogDetail(item.perGameLogID)">
									<v-icon> info </v-icon>
									{{ $t('Button.__showDetail') }}
								</v-list-item>

								<v-list-item v-if="item.isExternalVendorGame" @click="showExternalLinkDialog(item)">
									<v-icon> info </v-icon>
									{{ $t('Button.__showDetail') }}
								</v-list-item>

								<v-list-item v-if="item.isSupportFishGameDetailRecord" @click="showFishGameDetailDialog(item)">
									<v-icon> info </v-icon>

									{{ $t('TableHeader.__detailOfFishGame') }}
								</v-list-item>

								<v-list-item v-if="!item.isAccountingIgnoreData && hasAdminPermission" @click="showAddPerGameTag(item)">
									<v-icon> note_add </v-icon>
									{{ $t('Button.__addTag') }}
								</v-list-item>

								<v-list-item v-if="!item.isAccountingIgnoreData && item.isSlotGame && !item.isExternalVendorGame && item.serverID > 0" @click="showSlotHistoryInfo(item.transactionNo)">
									<v-icon> image </v-icon>
									{{ $t('Button.__showResultImage') }}
								</v-list-item>
							</v-list-item>
						</v-list>
					</v-menu>
				</div>
			</template>

			<template v-slot:item.perGameLogID="{ item }">
				<div>
					{{ item.transactionNo }}
				</div>
				<div class="sub-title-text" :title="'perGameDetailLogID/ThirdBetRecordNo'">{{ item.perGameDetailLogID }} ({{ item.thirdPartyBetRecordNo }})</div>
			</template>

			<template v-slot:item.transactionNo="{ item }">
				<div>
					{{ item.transactionNo }}
				</div>
				<div class="sub-title-text">
					{{ item.thirdPartyBetRecordNo }}
				</div>
			</template>

			<template v-slot:item.vendorConfigID="{ item }">
				<div v-if="item.vendorConfigID > 0">
					{{ item.vendorConfigID }}
				</div>
				<div v-else class="">
					{{ item.serverID }}
				</div>
			</template>

			<template v-slot:item.playerName="{ item }">
				<div v-if="!hasPlayerSearchPermission">
					<div class="">
						{{ item.playerName }}
					</div>

					<div class="sub-title-text">
						{{ item.playerID }}
					</div>
				</div>

				<div v-else>
					<v-tooltip top color="teal darken-1">
						<template v-slot:activator="{ on }">
							<a v-on="on" @click="openPlayerDetail(item.playerID)" class="">
								{{ item.playerName }}
							</a>
						</template>
						<span>{{ $t('Button.__openLink') }}</span>
					</v-tooltip>

					<div class="sub-title-text">
						{{ item.playerID }}
					</div>
				</div>

				<!-- <div :title="item.merchantID" class="smallTextGrey">
					{{item.merchantName}}
				</div> -->
			</template>

			<template v-slot:item.merchantName="{ item }">
				<div :title="item.merchantID">
					{{ item.merchantName }}
				</div>

				<div class="sub-title-text">
					{{ item.merchantID }}
				</div>
			</template>

			<template v-slot:item.serverID="{ item }">
				<span v-if="item.serverID == 0">
					<v-chip>{{ item.vendorName }}</v-chip>
				</span>
				<span v-else>{{ item.serverID }}</span>
			</template>

			<template v-slot:item.gameName="{ item }">
				<div :title="item.gameID">
					{{ item.gameName }}
				</div>

				<div class="sub-title-text" :title="item.serverID">
					{{ item.gameID }}
				</div>
			</template>

			<template v-slot:item.updateAmount="{ item }">
				<div v-if="item.updateAmount >= 0">{{ item.updateAmount | amountDisplay }}</div>
				<div v-if="item.updateAmount < 0" class="minus-red">{{ item.updateAmount | amountDisplay }}</div>
			</template>

			<template v-slot:item.validAmount="{ item }">
				<div>
					{{ item.validAmount | amountDisplay }}
				</div>
			</template>

			<template v-slot:item.validAmount="{ item }">
				<div>
					{{ item.validAmount | amountDisplay }}
				</div>
			</template>

			<template v-slot:item.betAmount="{ item }">
				{{ item.betAmount | amountDisplay }}
			</template>

			<template v-slot:item.payoutAmount="{ item }">
				<div :class="item.jackpotAmount > 0 ? 'yellow-text' : ''">
					{{ item.payoutAmount | amountDisplay }}
				</div>
			</template>
			<template v-slot:item.taxAmount="{ item }">{{ item.taxAmount | amountDisplay }}</template>

			<template v-slot:item.currentAmount="{ item }">
				<span v-if="item.isShowFishGameMoneyRecordButton && isShowMoneyRecordLinkButton" class="link-text-white" @click="showFishGameMoneyRecord(item)">
					{{ $t('Menu.MembersManagementMoneyRecord') }}
				</span>
				<span v-else-if="item.isHideAfterAmountInfo">-</span>
				<span v-else-if="!isAfterAmountVisible">-</span>
				<span v-else class="pointer" @click="toMoneyRecordPage(item.playerID)" :title="'查看金幣紀錄'">
					{{ item.currentAmount | amountDisplay }}
				</span>
			</template>

			<template v-slot:item.afterAmount="{ item }">
				<span v-if="item.isShowFishGameMoneyRecordButton && isShowMoneyRecordLinkButton" class="link-text-white" @click="showFishGameMoneyRecord(item)">
					{{ $t('Menu.MembersManagementMoneyRecord') }}
				</span>
				<span v-else-if="item.isHideAfterAmountInfo"> - </span>
				<span v-else-if="!isAfterAmountVisible">-</span>
				<span v-else class="pointer" @click="toMoneyRecordPage(item.playerID)" :title="'查看金幣紀錄'">
					{{ item.afterAmount | amountDisplay }}
				</span>
			</template>

			<template v-slot:item.extraTag="{ item }">
				<span v-if="item.isDelaySettle && hasSystenAdminPermission" class="minus-red">
					<v-icon small color="red lighten-1">error</v-icon>
					{{ $t('Message.__delaySettle') }}
				</span>

				<div style="display: inline-block" v-if="item.isAnyPrizeRecordReference">
					<span class="link-text-red2 pointer" v-if="hasPermissionPrizeRecordPage" @click="toPrizeRecordPage(item.playerID)">
						<v-icon small color="red lighten-1">mdi-gift</v-icon>
						{{ $t('TableHeader.__promotionBonusReceive') }}
					</span>

					<span class="link-text-red2" v-else>
						<v-icon small color="red lighten-1">mdi-gift</v-icon>
						{{ $t('TableHeader.__promotionBonusReceive') }}
					</span>
				</div>

				<div style="display: inline-block" v-if="item.isAnyTransferRecordReference">
					<span class="yellow-text">
						<v-icon small color="red lighten-1">mdi-swap-horizontal</v-icon>
						{{ $t('Menu.MembersManagementTransfer') }}
					</span>
				</div>
			</template>

			<template v-slot:item.gameTime="{ item }">
				<span class="pointer" style="font-size: 13px" @click="globalPopupCopyDateTime(item.gameTime, item.perGameLogID + ' | ' + item.perGameDetailLogID)">
					{{ item.gameTime | timeWithAddTimezone(userProfile.timeZone) }}
				</span>
			</template>

			<template slot="body.append" v-if="totalCount > 0 && searchTotalSummaty">
				<tr>
					<td colspan="2">
						<div class="text-l">{{ $t('TableHeader.__total') }} ({{ $t('TableHeader.__currentPageOnly') }}) :</div>
					</td>
					<td></td>
					<td v-if="isShowExtraColumnInfo"></td>
					<td v-if="isShowExtraColumnInfo"></td>
					<td></td>
					<td></td>
					<td></td>

					<td class="text-r">
						{{ searchTotalSummaty.betAmount | amountDisplay }}
					</td>
					<td class="text-r">
						{{ searchTotalSummaty.payoutAmount | amountDisplay }}
					</td>
					<td class="text-r">
						<div v-if="searchTotalSummaty.updateAmount >= 0">
							{{ searchTotalSummaty.updateAmount | amountDisplay }}
						</div>
						<div v-if="searchTotalSummaty.updateAmount < 0" class="minus-red">
							{{ searchTotalSummaty.updateAmount | amountDisplay }}
						</div>
					</td>

					<td v-if="isShowExtraColumnInfo">
						{{ searchTotalSummaty.validAmount | amountDisplay }}
					</td>
					<!-- 					
					<td class='text-r'>
						{{searchTotalSummaty.taxAmount | amountDisplay}}
					</td> -->
					<td class="text-r" v-if="!hasHideBeforeAfterAmountPermission">-</td>
					<td class="text-r" v-if="!hasHideBeforeAfterAmountPermission">-</td>
					<td class="text-r"></td>
				</tr>
			</template>
		</v-data-table>

		<!-- 三方注單資訊 & 產生外部遊戲快照連結 -->
		<v-dialog v-model="isShowExternalLinkDialog" max-width="900px">
			<PerGameThirdPartyDetailPopup ref="refPerGameThirdPartyDetailPopup" @closeParentAllModal="closeAllModal"> </PerGameThirdPartyDetailPopup>
		</v-dialog>

		<!-- 捕魚詳細資訊 -->
		<v-dialog v-model="isShowFishGameDetailDialog" max-width="1280px">
			<FishGameDetailRecordPopup ref="refFishGameDetailRecordPopup" @closeParentAllModal="closeAllModal"> </FishGameDetailRecordPopup>
		</v-dialog>

		<v-dialog v-model="isShowSlotHistory" max-width="850px">
			<GameHistoryImageView ref="gameHistoryImageView" @closeSlotHistory="closeAllModal"> </GameHistoryImageView>
		</v-dialog>

		<!-- 標記標籤 -->
		<v-dialog v-model="isShowAddTag" max-width="500px">
			<v-card>
				<v-card-title class="grey darken-2">
					{{ $t('Button.__addTag') }}
					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>
				<v-card-text>
					<v-flex style="padding: 10px">
						<v-row>
							<v-text-field class="" v-model="targetAddTagPerGame.perGameDetailLogID" disabled="disabled" :label="$t('TableHeader.__ID')" autofocus> </v-text-field>
						</v-row>

						<v-row>
							<v-text-field class="" v-model="targetAddTagPerGame.perGameLogID" disabled="disabled" :label="$t('TableHeader.__perGameLogID')" autofocus> </v-text-field>
						</v-row>

						<v-row>
							<v-text-field class="" v-model="targetAddTagPerGame.betAmount" disabled="disabled" :label="$t('TableHeader.__betAmount')" autofocus> </v-text-field>
						</v-row>

						<v-row>
							<v-text-field class="" v-model="targetAddTagPerGame.payoutAmount" disabled="disabled" :label="$t('TableHeader.__payoutAmount')" autofocus> </v-text-field>
						</v-row>

						<v-row>
							<v-chip v-if="targetAddTagPerGame.is"> </v-chip>
						</v-row>

						<v-row>
							<v-autocomplete v-model="addPerGameTagForm.tagID" :items="perGameTagAllowManualDropdownItems" :label="$t('TableHeader.__tag')"> </v-autocomplete>
						</v-row>
					</v-flex>
				</v-card-text>

				<v-card-actions>
					<v-spacer></v-spacer>

					<v-btn text @click="closeAllModal" class="btn">
						<v-icon left>mdi-cancel</v-icon>
						{{ $t('Button.__cancel') }}
					</v-btn>

					<v-btn color="yellow darken-1" text @click="saveAddTag" class="btn">
						<v-icon left>mdi-check-circle</v-icon>
						{{ $t('Button.__submit') }}
					</v-btn>
				</v-card-actions>
			</v-card>
		</v-dialog>

		<PopupCopyDatetime ref="refCopyDateTime"></PopupCopyDatetime>

		<FishingMoneyRecordPopupDialogScrollPaging ref="refFishingMoneyRecordPopupDialogScrollPaging"> </FishingMoneyRecordPopupDialogScrollPaging>
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import { State, Action, Getter, namespace } from 'vuex-class';
const playersModule = namespace('Players');
import EventBus from '@/utilities/event-bus';
import { datetimeMixin, startEndDate, timeWithTimezone, daysAgo, weeksAgo, monthsAgo, formatMixin } from '@/utilities/datetime-format';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { commonMethodMixin } from '@/utilities/common-methods';
import * as Model from '@/models/interfaces/game-management/per-game';
import GameManagementApi from '@/api/game-management/per-game';
import DropdownApi, { convertLang } from '@/api/commons/dropdown';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import DatePicker from 'vue2-datepicker';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import * as Permission from '@/models/status/permission';
import GamePayTableImageCreate from '@/components/system-setting/GamePayTableImageCreate.vue';
import GameHistoryImageView from '@/components/game-management/GameHistoryImageView.vue';
import PerGameThirdPartyDetailPopup from '@/components/game-management/PerGameThirdPartyDetailPopup.vue';
import FishGameDetailRecordPopup from '@/components/game-management/FishGameDetailRecordPopup.vue';
import FishingMoneyRecordPopupDialogScrollPaging from '@/components/common-component/FishingMoneyRecordPopupDialogScrollPaging.vue';

import PopupCopyDatetime from '@/components/PopupCopyDatetime.vue';

import axios, { AxiosResponse, AxiosRequestConfig } from 'axios';

const authModule = namespace('Auth');

// @ts-ignore
@Component({
	// @ts-ignore
	mixins: [datetimeMixin, commonDisplayFiltersMixin, formatMixin, commonMethodMixin],
	components: { PerGameThirdPartyDetailPopup, GameHistoryImageView, FishGameDetailRecordPopup, DatePicker, GamePayTableImageCreate, PopupCopyDatetime, FishingMoneyRecordPopupDialogScrollPaging },
})
export default class List extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];

	@authModule.State('defaultLangTimes') defaultLangTimes!: number;
	@playersModule.State('gamePlayerID') gamePlayerID!: number;
	@playersModule.State('gameID') gameID!: number;
	@playersModule.State('serverID') serverID!: number;
	@playersModule.State('perGameLogID') perGameLogID!: number;
	@playersModule.State('transactionNo') transactionNo!: string;

	isShowAddTag: boolean = false;
	isShowExtraColumnInfo: boolean = false;
	isShowSlotHistory: boolean = false;
	isShowExternalLinkDialog: boolean = false;
	isShowFishGameDetailDialog: boolean = false;
	isAfterAmountVisible: boolean = false;

	reloadSearchFilterFlag: boolean = true;
	renderComponent: boolean = true;
	panel: number[] = [0];
	totalCount: number = 0;
	pageIndex: number = 1;
	footerProps: any = { 'items-per-page-options': [10, 30, 50, 100] };
	itemsPerPage: number = 10;

	datetimeRangePicker: DropdownModel.DatetimeRangePicker = new DropdownModel.DatetimeRangePicker();
	searchData: Model.IListRequest = {} as Model.IListRequest;
	result: Model.IListResponse[] = [];
	searchTotalSummaty: Model.IListResponse = {} as Model.IListResponse;
	targetPopupPerGameDetailLog: Model.IListResponse = {} as Model.IListResponse;
	targetPopupPerGameDetailLogRecordInfo: Model.IBetRecordInfo = {} as Model.IBetRecordInfo;
	targetAddTagPerGame: Model.IListResponse = {} as Model.IListResponse;
	addPerGameTagForm: Model.IPerGameAddTagRequest = {} as Model.IPerGameAddTagRequest;

	gamesDropdownItemsTemp: DropdownModel.IDropdownItems[] = [];
	gamesDropdownItems: DropdownModel.IDropdownItems[] = [];
	serversDropdownItems: DropdownModel.IDropdownItems[] = [];
	merchantDropdownItems: DropdownModel.IDropdownItems[] = [];
	perGameTagDropdownItems: DropdownModel.IDropdownItems[] = [];
	perGameTagAllowManualDropdownItems: DropdownModel.IDropdownItems[] = [];

	quicklyPickTimeRangeText: string = '';

	playerID: number | null = null;
	playerName: string = '';
	copyText: string = '';
	currentTime: string = '';
	searchGamesDropdwonKeyword: string = '';

	specialTagNumber: number = 9001021;

	downloading: boolean = false;

	get headers() {
		const array = [];

		array.push({ text: this.$t('TableHeader.__action'), value: 'perGameDetailLogID', width: '3%', align: 'center' });
		array.push({ text: this.$t('TableHeader.__time'), value: 'gameTime', width: '6%' });
		array.push({ text: this.$t('TableHeader.__betRecordNoPGS') + '／' + this.$t('TableHeader.__perGameLogID'), value: 'perGameLogID', width: '10%' });

		if (this.isShowExtraColumnInfo) {
			array.push({ text: this.$t('TableHeader.__betRecordNo') + '(Extra)', value: 'transactionNo' });
			array.push({ text: this.$t('TableHeader.__serverID'), value: 'vendorConfigID' });
		}

		array.push({ text: this.$t('TableHeader.__game'), value: 'gameName' });
		array.push({ text: this.$t('TableHeader.__merchant'), value: 'merchantName' });
		array.push({ text: this.$t('TableHeader.__playerAccount'), value: 'playerName' });
		array.push({ text: this.$t('TableHeader.__betAmount'), value: 'betAmount', align: 'end' });
		array.push({ text: this.$t('TableHeader.__payoutAmount'), value: 'payoutAmount', align: 'end' });
		array.push({ text: this.$t('TableHeader.__winLossAmount'), value: 'updateAmount', align: 'end' });

		if (this.isShowExtraColumnInfo) {
			array.push({ text: this.$t('TableHeader.__validAmount'), value: 'validAmount', align: 'end' });
		}

		if (!this.hasHideBeforeAfterAmountPermission) {
			array.push({ text: this.$t('TableHeader.__beforeAmount'), value: 'currentAmount', align: 'end' });
			array.push({ text: this.$t('TableHeader.__afterAmount'), value: 'afterAmount', align: 'end' });
		}

		array.push({ text: '', value: 'extraTag', align: 'end' });
		return array;
	}

	get gamesDropdownWithKeywordFilter() {
		const vm: any = this;
		return vm.filterDropdownItemsByKeywordSearch(vm.gamesDropdownItems, vm.globalKeywordSearchGamesDropdown, vm.searchData.gameID);
	}

	get orderByColumn() {
		return [
			{ text: this.$t('TableHeader.__time'), value: 'gameTime' },
			{ text: this.$t('TableHeader.__perGameLogID'), value: 'perGameLogID' },
			{ text: this.$t('TableHeader.__serverID'), value: 'serverID' },
			{ text: this.$t('TableHeader.__gameID'), value: 'gameID' },
			{ text: this.$t('TableHeader.__merchantID'), value: 'merchantID' },
			{ text: this.$t('TableHeader.__playerID'), value: 'playerID' },
		];
	}

	get isShowMoneyRecordLinkButton() {
		return this.hasMoneyRecordPermission;
	}

	get hasAdminPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.GamesManagementAdminPermission) >= 0;
	}

	get hasSystenAdminPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSetting_SystemConfigSetting) >= 0;
	}

	get has_AM_Permission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.Special_AM_Permission) >= 0;
	}
	get hasPlayerSearchPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.MembersManagementList_Search) >= 0;
	}

	get hasMoneyRecordPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.MembersManagementMoneyRecord) >= 0;
	}

	get hasHideBeforeAfterAmountPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.Special_HideBeforeAfterAmount) >= 0;
	}

	get hasSystenExperimentFeaturePermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.Special_AllowToUseExperimentFeature) >= 0;
	}

	get hasPermissionPrizeRecordPage() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSetting_EventPrizeRecord) >= 0;
	}

	@Action('Players/setPrizeRecordPlayerID') private setPrizeRecordPlayerID!: any;
	@Action('Players/setWalletPlayerID') private setWalletPlayerID!: any;
	@Action('Players/setPlayerID') private setPlayerID!: any;
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

	@Watch('perGameLogID')
	onPerGameLogIDChange(newVal: number) {
		this.searchData = {} as Model.IListRequest;
		this.searchData.perGameLogID = this.perGameLogID;
		this.search();
	}

	@Watch('transactionNo')
	onTransactionNoChange(newVal: string | null) {
		this.searchData = {} as Model.IListRequest;
		this.searchData.transactionNo = newVal;
		this.search();
	}

	@Watch('gamePlayerID')
	onGamePlayerIDChange(newVal: number) {
		this.searchData = {} as Model.IListRequest;
		this.searchData.gameID = this.gameID.toString();
		this.searchData.playerID = this.gamePlayerID;
		this.setDefaultTimeRange();
		this.search(true);
	}

	@Watch('gameID')
	onGameChange(newVal: number) {
		this.searchData = {} as Model.IListRequest;
		this.searchData.playerID = this.gamePlayerID;
		this.searchData.gameID = newVal.toString();
		this.search();
	}

	@Watch('serverID')
	onServerIDChange(newVal: number) {
		this.searchData = {} as Model.IListRequest;
		this.searchData.serverID = newVal.toString();
		this.search();
	}

	@Watch('searchData.gameID', { immediate: true, deep: true })
	onGameIDChange() {}

	@Watch('defaultLangTimes')
	onDefaultLangChanged() {
		if (this.result.length > 0) {
			this.search();
		}
		this.gamesDropdownItems = convertLang(this.gamesDropdownItemsTemp);
	}

	geti18nTextAsString(key: string) {
		return this.$t(key).toString();
	}

	search(isSearchAgain: boolean = false) {
		const request = { ...this.searchData };
		request.pageIndex = this.pageIndex;
		request.pageSize = this.itemsPerPage;
		request.gameStartTimeBegin = this.datetimeRangePicker.searchStartTimeRange[0] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[0]) : null;
		request.gameStartTimeEnd = this.datetimeRangePicker.searchStartTimeRange[1] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[1]) : null;
		request.isASC = this.searchData.isASC;

		EventBus.$emit('show_mask', null);

		let delayMilliSecond: number = 0;

		const vm: any = this;

		// 刻意調低回應速度 讓使用者操作查詢頻率減低
		if (!this.hasAdminPermission && request.pageSize >= 50 && request.pageIndex >= 2) {
			if (vm.isEmptyString(request.playerName) && (request.playerID == null || request.playerID <= 0)) {
				delayMilliSecond = 1000 * 5;
			}
		}

		if (request.tagID && request.tagID === this.specialTagNumber) {
			request.payoutAmountLargeThan = 1000000;
			request.tagID = request.tagID - 9000000;
		}

		GameManagementApi.getList(<Model.IListRequest>request)
			.then((res) => {
				const distinctIDs = [...new Set(res.data.items.map((p) => p.playerID))];
				this.isAfterAmountVisible = distinctIDs.length === 1 && (request.playerID > 0 || !vm.isEmptyString(request.playerName));

				// 根據狀況，會故意delay幾秒才顯示結果
				setTimeout(() => {
					this.result = res.data.items;
					this.totalCount = res.data.totalCount;

					if (res.data.extraObject) {
						this.searchTotalSummaty = res.data.extraObject.totalSummary;
					}

					// 特定情況下，若查無資料 則會移除部分篩選 並嘗試再查一次(為了避免智障users的操作問題)
					if (this.totalCount <= 0 && isSearchAgain) {
						console.log('trigger search again');
						this.searchData.gameID = '';
						this.search(false);
					}
				}, delayMilliSecond);
			})
			.finally(() => {
				// 根據狀況，會故意delay幾秒才顯示結果
				setTimeout(() => {
					EventBus.$emit('hide_mask', null);
				}, delayMilliSecond);
			});
	}

	async exportFiles() {
		const excelHeaders: any = [
			this.geti18nTextAsString('TableHeader.__time') + '(GMT+' + this.userProfile.timeZone.toString() + ')',
			this.geti18nTextAsString('TableHeader.__perGameLogID'),
			this.geti18nTextAsString('TableHeader.__betRecordNo') + '(SW)',
			this.geti18nTextAsString('TableHeader.__game'),
			this.geti18nTextAsString('TableHeader.__merchant'),
			this.geti18nTextAsString('TableHeader.__playerAccount'),
			this.geti18nTextAsString('TableHeader.__betAmount'),
			this.geti18nTextAsString('TableHeader.__payoutAmount'),
			this.geti18nTextAsString('TableHeader.__updateAmount'),
			this.geti18nTextAsString('TableHeader.__beforeAmount'),
			this.geti18nTextAsString('TableHeader.__afterAmount'),
			this.geti18nTextAsString('TableHeader.__memo'),
		];

		const excelData: any = [];

		const request = this.searchData;
		request.pageIndex = 1;
		request.pageSize = 10000;

		if (!this.hasAdminPermission) {
			request.pageSize = 2000;
		}

		request.gameStartTimeBegin = this.datetimeRangePicker.searchStartTimeRange[0] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[0]) : null;
		request.gameStartTimeEnd = this.datetimeRangePicker.searchStartTimeRange[1] ? startEndDate(this.datetimeRangePicker.searchStartTimeRange[1]) : null;
		request.isASC = this.searchData.isASC;

		EventBus.$emit('show_mask', null);

		const result = await GameManagementApi.getList(<Model.IListRequest>request).finally(() => {
			EventBus.$emit('hide_mask', null);
		});

		result.data.items.forEach((value) => {
			const eachRow: any = [];
			eachRow.push(timeWithTimezone(value.gameTime, this.userProfile.timeZone));
			eachRow.push(value.perGameLogID);
			eachRow.push(value.transactionNo);
			eachRow.push(value.gameName);
			eachRow.push(value.merchantName);
			eachRow.push(value.playerName);
			eachRow.push(value.betAmount);
			eachRow.push(value.payoutAmount);
			eachRow.push(value.updateAmount);

			if (value.currentAmount === -1) {
				eachRow.push('-');
				eachRow.push('-');
			} else {
				eachRow.push(value.currentAmount);
				eachRow.push(value.afterAmount);
			}

			eachRow.push(value.isAnyPrizeRecordReference ? this.geti18nTextAsString('TableHeader.__promotionBonusReceive') : '');
			excelData.push(eachRow);
		});

		const config = {
			headers: excelHeaders,
			rowData: excelData,
			fileName: 'Export_GameLogRecord.xlsx',
		};

		// @ts-ignore
		this.downloadExcel(config);
	}

	showExternalLinkDialog(item: Model.IListResponse) {
		this.isShowExternalLinkDialog = true;

		this.$nextTick(() => {
			const myRef: any = this.$refs.refPerGameThirdPartyDetailPopup;
			myRef.getExternalLinkDialogInfo(item);
		});
	}

	showFishGameDetailDialog(item: Model.IListResponse) {
		this.isShowFishGameDetailDialog = true;
		this.$nextTick(() => {
			const myRef: any = this.$refs.refFishGameDetailRecordPopup;
			myRef.getFishGameRecords(item.transactionNo);
		});
	}

	showFishGameMoneyRecord(item: Model.IListResponse) {
		const myFilter: any = {
			querySqlID: 900121,
			playerID: item.playerID,
			timeZone: this.userProfile.timeZone,
			pageSize: 200,
		};

		const endTime = new Date(item.gameTime).getTime() + 5 * 60 * 1000; // 5分鐘後
		const myConfig: any = {
			maxWidth: 1400,
			UI_beginTS: endTime - 4 * 60 * 60 * 1000, // N小時前
			UI_endTS: endTime,
			UI_isShowFilterFishLogType: true,
			UI_isShowFilterBeginEndTime: true,
		};

		const myRef: any = this.$refs.refFishingMoneyRecordPopupDialogScrollPaging;
		myRef.searchAndShowDialog(myFilter, myConfig);
	}

	showAddPerGameTag(item: Model.IListResponse) {
		this.isShowAddTag = true;
		this.targetAddTagPerGame = item;
	}

	saveAddTag() {
		EventBus.$emit('show_mask', null);
		this.addPerGameTagForm.perGameDetailLogID = this.targetAddTagPerGame.perGameDetailLogID;
		GameManagementApi.addPerGameTag(this.addPerGameTagForm)
			.then((res) => {
				this.closeAllModal();
				this.search();
				EventBus.$emit(`successfully-modified`);
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	closeAllModal() {
		this.isShowSlotHistory = false;
		this.isShowAddTag = false;
		this.isShowExternalLinkDialog = false;
		this.isShowFishGameDetailDialog = false;
	}

	showSlotHistoryInfo(transactionNo: string) {
		this.isShowSlotHistory = true;

		this.$nextTick(() => {
			const myRef: any = this.$refs.gameHistoryImageView;
			myRef.getSlotHistoryInfo(transactionNo);
		});
	}

	toDisplayTime(isoTime: string) {
		return timeWithTimezone(isoTime, this.userProfile.timeZone);
	}

	pageOneSearch() {
		this.pageIndex = 1;
		this.search();

		const vm: any = this;
		vm.globalKeywordSearchGamesDropdown = '';
	}

	openPlayerDetail(playerID: number) {
		this.setPlayerID(playerID);
		this.$router.push({ path: '/members-management/detail' });
	}

	openPerGameLogDetail(perGameLogID: number) {
		if (this.$route.params.perGameLogID === perGameLogID.toString()) {
			return;
		}
		this.$router.push({ path: `/games-management/record/${perGameLogID}` });
	}

	toMoneyRecordPage(playerID: number) {
		this.setWalletPlayerID(0);
		this.$router.push({ path: '/members-management/money-record' });

		setTimeout(() => {
			this.setWalletPlayerID(playerID);
		}, 250);
	}

	toPrizeRecordPage(playerID: number) {
		this.setPrizeRecordPlayerID(0);
		this.$router.push({ path: '/system-setting/event-prize-record' });

		setTimeout(() => {
			this.setPrizeRecordPlayerID(playerID);
		}, 250);
	}

	copySuccess() {
		EventBus.$emit(`successfully-copy`);
	}

	setDefaultTimeRange() {
		console.log('debug setDefaultTimeRange ');
		console.log(this.$route.params);
		console.log(this.$route.query);

		if (this.$route.query && this.$route.query.beginTS) {
			console.log('setDefaultTimeRange');
			const beginTS = Number(this.$route.query.beginTS) * 1000;
			const endTS = beginTS + 86400 * 2 * 1000;
			this.datetimeRangePicker.searchStartTimeRange = [beginTS, endTS];
		} else {
			this.datetimeRangePicker.searchStartTimeRange = [daysAgo(0), daysAgo(1)];
		}
	}

	setDefaultParameterByQuery() {
		if (this.$route.query && this.$route.query.transactionNo) {
			const myQuery: any = this.$route.query.transactionNo;
			this.searchData.transactionNo = myQuery;
		}
	}

	clear() {
		this.searchData = {} as Model.IListRequest;
		this.datetimeRangePicker.searchStartTimeRange = [];
		this.setDefaultTimeRange();
	}

	registerReloadComponent() {
		EventBus.$on(`reload-${this.$router.currentRoute.path}`, (info: any) => {
			console.log('event ReloadComponent');
			this.renderComponent = false;
			this.$nextTick(() => {
				this.clear();
				this.result = [];
				this.renderComponent = true;
			});
		});
	}

	reloadSearchFilterComponent() {
		console.log('reloadSearchFilterComponent');
		console.log(this.searchData.playerID);
		console.log(this.searchData.gameID);

		this.reloadSearchFilterFlag = false;
		this.$nextTick(() => {
			this.reloadSearchFilterFlag = true;
		});
	}

	mounted() {
		this.init();

		const route: any = this.$route;
		this.searchData.playerName = route.query.playerName;

		this.registerReloadComponent();
		if (this.gameID || this.gamePlayerID) {
			this.searchData.gameID = this.gameID.toString();
			this.searchData.playerID = this.gamePlayerID;
			this.pageOneSearch();
		} else if (this.perGameLogID) {
			this.searchData.perGameLogID = this.perGameLogID;
			this.pageOneSearch();
		} else if (this.serverID) {
			this.searchData.serverID = this.serverID.toString();
			this.pageOneSearch();
		} else if (this.$route.params.perGameLogID) {
			this.searchData.perGameLogID = Number(this.$route.params.perGameLogID);
			this.pageOneSearch();
		}
	}

	init() {
		this.setDefaultTimeRange();

		this.setDefaultParameterByQuery();

		DropdownApi.getGames().then((res) => {
			this.gamesDropdownItemsTemp = res;
			this.gamesDropdownItems = res;
		});
		DropdownApi.getServerAndVendorConfigs().then((res) => {
			this.serversDropdownItems = res;
		});

		DropdownApi.getMerchants(true).then((res) => {
			this.merchantDropdownItems = res;
		});

		DropdownApi.getPerGameTagsAllowManualAdd().then((res) => {
			this.perGameTagAllowManualDropdownItems = res;
		});

		DropdownApi.getPerGameTags().then((res) => {
			this.perGameTagDropdownItems = res;

			if (this.hasSystenAdminPermission || this.has_AM_Permission) {
				// 客戶希望有大額的
				const extraItem: DropdownModel.IDropdownItems = {} as DropdownModel.IDropdownItems;
				const text: any = this.$t('TableHeader.__jackpotTag') + ' (1M+)';
				extraItem.text = text;
				extraItem.value = this.specialTagNumber;

				this.perGameTagDropdownItems.splice(1, 0, extraItem);
			}
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

.targetHistoryValue {
	border-bottom: dashed 1px #888;
	font-size: 16px;
	padding: 2px 0px;
}

.emptyImage {
	cursor: pointer;
	display: inline-block;
	border: dashed 2px #777;
	text-align: center;
	position: relative;
	width: 120px;
	height: 120px;
}

.innerText {
	font-size: 20px;
	position: absolute;
	top: 0;
	width: 120px;
	height: 20px;
	right: 0;
	bottom: 0;
	left: 0;
	margin: auto;
}

.innerText.s-size {
	font-size: 14px;
	width: 60px;
}

.linePreview {
	flex: 2;
	text-align: center;
}

.imageWrapper {
	width: 120px;
	margin: 0 auto;
	overflow: hidden;
	text-align: center;
	position: relative;
	height: 120px;
}

.slotsIcon {
	height: 120px;
	min-width: 100px;
	max-width: none;
	margin: 0 -100%;
}

.parentWarpper {
	display: inline-block;
	height: 125px;
}

.operate_icons .v-icon {
	font-size: 20px;
	margin-left: 4px;
	color: #ddd;
}

.yellow-text {
	color: #fdc038;
}

.ThirdPartyDetailRow .targetHistoryLabel {
	margin-top: 0px;
}

.bet-detail-popup.row > .col-12 {
	padding: 4px 12px !important;
}

.smallTextGrey {
	font-size: 12px;
	color: #999;
}
</style>
