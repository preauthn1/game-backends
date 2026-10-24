<template>
	<v-app v-if="renderComponent" class="app">
		<v-expansion-panels v-model="panel" multiple>
			<v-expansion-panel>
				<v-expansion-panel-content>
					<v-flex>
						<v-row class="search-form">
							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete clearable :items="dropdownActiveStatus" chips v-model="searchFilter.status" :label="$t('TableHeader.__status')"> </v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete clearable chips :items="dropdownGameVendors" v-model="searchFilter.gameVendorType" :label="$t('TableHeader.__gameVendorType')"> </v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item merchantDropdownItemsWrapper">
									<v-autocomplete
										clearable
										chips
										:items="dropdownMerchants"
										class="merchantDropdownItems"
										v-model="searchFilter.assignMerchantID"
										:label="$t('TableHeader.__assignMerchantTotalCount') + ' (僅列出該商戶能玩的遊戲)'"
									>
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
								<div class="item search-row-padding">
									<v-btn color="yellow darken-1" text class="btn search" @click.stop="searchTable">
										<v-icon left>search</v-icon>
										{{ $t('Button.__search') }} </v-btn
									>&#9;
									<v-btn color="pink lighten-3" text class="btn search" @click.stop="clear">
										<v-icon left>clear</v-icon>
										{{ $t('Button.__reset') }} </v-btn
									>&#9;
									<v-btn color="light-blue lighten-3" text class="btn search" @click.stop="showExportConfirm">
										<v-icon left>cloud_download</v-icon>
										{{ $t('Button.__downloadFile') }} </v-btn
									>&#9;
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3" style="margin-bottom: 20px">
								<div class="item">
									<v-autocomplete clearable chips :items="dropdownGameTypes" v-model="searchFilter.gameType" :label="$t('TableHeader.__gameType')"> </v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3" style="margin-bottom: 20px">
								<div class="item">
									<v-autocomplete clearable chips :items="dropdownGameListExtraFilterType" v-model="searchFilter.extraFilterType" :label="$t('TableHeader.__extraFilterType')">
									</v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3" style="margin-bottom: 20px">
								<div class="item">
									<v-autocomplete clearable chips :items="dropdownSpecialQuery" v-model="searchFilter.extraQueryInfo" :label="'指定查詢特殊資訊'"> </v-autocomplete>
								</div>
							</v-col>
						</v-row>
					</v-flex>
				</v-expansion-panel-content>
			</v-expansion-panel>
		</v-expansion-panels>

		<v-data-table
			:headers="headers"
			id="table"
			:items="gameListData"
			:search="search"
			@current-items="setCurrentItems"
			:page.sync="pageIndex"
			:items-per-page="itemsPerPage"
			:footer-props="footerProps"
			multi-sort
		>
			<template v-slot:top>
				<v-toolbar>
					<v-toolbar-title>
						<v-tooltip top color="orange darken-4">
							<template v-slot:activator="{ on, attrs }">
								<v-icon color="grey" small>info</v-icon>
								<span
									v-bind="attrs"
									v-on="on"
									class="f12 pointer"
									@click="copyTextToClipboard('DbEntity_LanguageTexts & GetAllGameNameLanguageText')"
									style="color: #ccc; text-decoration: underline"
								>
									SQL新加遊戲後，需清除部分Cache
								</span>
							</template>
							<span>
								<div>請去操作這個按鈕</div>
								<div>管理員設置 > 清除緩存Cache (新增遊戲GameList後)</div>
								<div></div>
							</span>
						</v-tooltip>
					</v-toolbar-title>

					<v-divider class="mx-4" inset vertical></v-divider>

					<v-spacer></v-spacer>

					<v-card-title>
						<v-text-field
							v-model="search"
							append-icon="search"
							:label="`${$t('TableHeader.__gameID')} / ${$t('TableHeader.__gameName')}`"
							single-line
							hide-details
							clearable
						></v-text-field>
					</v-card-title>

					<v-card-title>
						<v-btn icon dark @click="addItem" v-if="hasPermissionCreate">
							<v-icon>add</v-icon>
						</v-btn>
						<v-btn icon dark @click="copyCurrentItems">
							<v-icon>file_copy</v-icon>
						</v-btn>
					</v-card-title>
				</v-toolbar>
			</template>

			<template v-slot:item.action="{ item }">
				<div class="operate_icons">
					<v-tooltip top v-if="hasPermissionUpdate">
						<template v-slot:activator="{ on, attrs }">
							<v-icon small class="mr-1" v-bind="attrs" v-on="on" @click="editItem(item)"> edit </v-icon>
						</template>
						<span>編輯基本資訊</span>
					</v-tooltip>

					<v-tooltip top v-if="!hasCustomerServicePermission">
						<template v-slot:activator="{ on, attrs }">
							<v-icon small class="mr-1" v-bind="attrs" v-on="on" :disabled="item.status != 1" @click="showGameLaunchDialog(item)"> link </v-icon>
						</template>
						<span>產生連結</span>
					</v-tooltip>

					<!-- <v-tooltip top v-if="hasPermissionUpdate">
						<template v-slot:activator="{ on, attrs }">
							<v-icon small class="mr-1" v-bind="attrs" v-on="on" @click="generateInsertSQL(item.gameID)"> stars </v-icon>
						</template>
						<span>複製工程師SQL語法</span>
					</v-tooltip> -->

					<v-tooltip top v-if="hasPermissionUpdate">
						<template v-slot:activator="{ on, attrs }">
							<v-icon small class="mr-1" v-bind="attrs" v-on="on" @click="showBatchEditSettings(item.gameID)"> settings </v-icon>
						</template>
						<span>編輯進階設置</span>
					</v-tooltip>
				</div>
			</template>

			<template v-slot:item.gameImageUrl="{ item }">
				<div style="opacity: 0.9">
					<a :href="item.gameImageUrl" target="_blank" v-if="item.gameID != 0">
						<img :src="item.gameImageUrl" style="width: 45px" />
					</a>

					<a :href="item.gameImageUrl2" target="_blank" v-if="item.gameID != 0" class="ml-1">
						<img :src="item.gameImageUrl2" style="width: 45px" />
					</a>
				</div>
			</template>

			<template v-slot:item.gameID="{ item }">
				<div>
					{{ item.gameID }}
				</div>
			</template>

			<template v-slot:item.supportedOrientationCN="{ item }">
				<div v-if="item.supportedOrientationCN != null && item.supportedOrientationCN.length > 0">
					<v-chip small :color="item.supportedOrientationCN === '橫版' ? 'grey darken-3' : 'green darken-4'">
						{{ item.supportedOrientationCN }}
					</v-chip>
				</div>
			</template>

			<template v-slot:item.gameName="{ item }">
				<div class="pointer" @click="copyGameNameInfo(item)">
					{{ item.gameName }}
				</div>
				<div class="sub-title-text">
					{{ item.memo }}
				</div>
			</template>

			<template v-slot:item.isMultiLanguageDefined="{ item }">
				<v-chip class="ma-2" v-if="item.isSkipInsertZeroBet" small>
					{{ $t('TableHeader.__zeroSkipInsert') }}
				</v-chip>

				<v-chip class="ma-2" color="light-blue accent-4" small v-if="item.gameID > 0 && !item.isGameListEnumDefined"> 列舉未定義 </v-chip>
			</template>

			<template v-slot:item.createTime="{ item }">
				{{ item.createTime | timeWithAddTimezone(userProfile.timeZone) }}
			</template>

			<template v-slot:item.gameTypeText="{ item }">
				<div :class="item.gameID == 0 ? '' : 'gameType_' + item.gameType" :title="item.gameType">
					{{ item.gameTypeText }}
				</div>
				<div class="sub-title-text">
					{{ item.gameVendorTypeText }}
				</div>
			</template>

			<template v-slot:item.isSupportSingleWallet="{ item }">
				<v-icon color="green" v-if="item.isSupportSingleWallet"> check </v-icon>
				<v-icon color="red" v-else> close </v-icon>
			</template>

			<template v-slot:item.isSupportRaiseProcess="{ item }">
				<v-icon color="orange" v-if="item.isSupportRaiseProcess"> check </v-icon>
			</template>

			<template v-slot:item.isForbiddenGameLaunch="{ item }">
				<v-icon color="green" v-if="!item.isForbiddenGameLaunch"> check </v-icon>
				<v-icon color="red" v-else> close </v-icon>
			</template>

			<template v-slot:item.numberOfMerchants="{ item }">
				<span class="link-text" v-if="hasPermissionUpdate" @click="showGameMerchantDialog(item)">
					{{ item.numberOfMerchants }}
				</span>
			</template>

			<template v-slot:item.gameName_CN="{ item }">
				<div>
					{{ item.gameName_CN }}
				</div>
				<div class="sub-title-text">
					{{ item.gameName_EN }}
				</div>
			</template>
			<template v-slot:item.gameVendorTypeText="{ item }">
				<!-- 這欄只是要提供關鍵字搜尋 但是不需要顯示 -->
			</template>

			<template v-slot:item.thirdPartySecondCode="{ item }">
				<!-- 這欄只是要提供關鍵字搜尋 但是不需要顯示 -->
			</template>

			<template v-slot:item.gameName_EN="{ item }">
				<!-- 這欄只是要提供關鍵字搜尋 但是不需要顯示 -->
			</template>

			<template v-slot:item.extraTextForKeywordSearch="{ item }">
				<!-- 這欄只是要提供關鍵字搜尋 但是不需要顯示 -->
			</template>

			<template v-slot:item.memo="{ item }">
				<!-- 這欄只是要提供關鍵字搜尋 但是不需要顯示 -->
			</template>

			<template v-slot:item.gameCode="{ item }">
				<div>
					<div class="">
						{{ item.gameCode }}
					</div>

					<div class="sub-title-text">
						<span style="display: inline-block">
							{{ item.thirdPartyGameCode }}
						</span>
						<span class="ml-2" v-if="!isEmptyString(item.thirdPartySecondCode)"> ({{ item.thirdPartySecondCode }}) </span>
					</div>
				</div>
			</template>

			<template v-slot:item.isSkipInsertZeroBet="{ item }">
				<div class="statusChips">
					<span v-if="!item.isSkipInsertZeroBet">
						{{ $t(`Button.__no`) }}
					</span>

					<span v-if="item.isSkipInsertZeroBet">
						<v-chip>
							{{ $t(`Button.__yes`) }}
						</v-chip>
					</span>
				</div>
			</template>

			<template v-slot:item.status="{ item }">
				<div class="statusChips">
					<span v-if="status.Disable === item.status" class="minus-red">
						{{ $t(`Status.${status[item.status]}`) }}
					</span>
					<span v-if="status.Enable === item.status" class="green-text">
						{{ $t(`Status.${status[item.status]}`) }}
					</span>
				</div>
			</template>
		</v-data-table>

		<!-- 新增/編輯 -->
		<v-form ref="loginForm">
			<v-dialog v-model="isShowEditDialog" max-width="500px" persistent>
				<v-card>
					<v-card-title class="grey darken-2">
						<span v-if="editAction == CRUDActionEnums.Create">{{ $t('Popup.__create') }}</span>
						<span v-if="editAction == CRUDActionEnums.Update">{{ $t('Popup.__update') }}</span>
						<div class="top-right-item" @click="close">
							<v-btn icon dark>
								<v-icon>close</v-icon>
							</v-btn>
						</div>
					</v-card-title>

					<v-card-text>
						<v-flex>
							<v-row>
								<v-text-field
									v-model="editedItem.gameID"
									:label="$t('TableHeader.__gameID')"
									:rules="[(value) => !!value || $t('Validation.__requiredRules')]"
									required
									v-if="hasPermissionUpdate"
									:disabled="editAction == CRUDActionEnums.Update"
								>
								</v-text-field>
							</v-row>

							<v-row>
								<v-text-field
									v-model="editedItem.gameName"
									:label="$t('TableHeader.__displayName')"
									clearable
									:rules="[(value) => !!value || $t('Validation.__requiredRules')]"
									required
								></v-text-field>
							</v-row>

							<v-row>
								<v-text-field v-model="editedItem.memo" :label="$t('TableHeader.__memo')" clearable required> </v-text-field>
							</v-row>

							<v-row>
								<v-text-field
									v-model="editedItem.gameCode"
									:label="$t('TableHeader.__gameCode')"
									:disabled="isThirdPartyGameLockEdit"
									clearable
									:rules="[(value) => !!value || $t('Validation.__requiredRules')]"
									required
									placeholder="建议使用全大写、3-5码缩写英文"
								></v-text-field>
							</v-row>

							<v-row>
								<v-select class="required" attach chips :label="$t('TableHeader.__gameType')" v-model="editedItem.gameType" :items="dropdownGameTypes"></v-select>
							</v-row>

							<v-row>
								<v-select
									class="required"
									attach
									chips
									:disabled="isThirdPartyGameLockEdit"
									:label="$t('TableHeader.__gameVendorType')"
									v-model="editedItem.gameVendorType"
									:items="dropdownGameVendors"
								></v-select>
							</v-row>

							<v-row>
								<v-radio-group row v-model="editedItem.status">
									<v-radio :label="$t('Status.Enable')" :value="status.Enable"></v-radio>
									<v-radio :label="$t('Status.Disable')" :value="status.Disable"></v-radio>
								</v-radio-group>
							</v-row>
						</v-flex>
					</v-card-text>
					<v-card-actions>
						<v-spacer></v-spacer>
						<v-btn text @click="close" class="btn">
							<v-icon left>mdi-cancel</v-icon>
							{{ $t('Button.__cancel') }}
						</v-btn>
						<v-btn color="yellow darken-1" text @click="save" class="btn">
							<v-icon left>mdi-check-circle</v-icon>
							{{ $t('Button.__submit') }}
						</v-btn>
					</v-card-actions>
				</v-card>
			</v-dialog>
		</v-form>

		<!-- 匯出 -->
		<v-dialog v-model="isShowExportConfirm" max-width="600px">
			<v-card>
				<v-card-title class="grey darken-2">
					匯出遊戲清單
					<div class="top-right-item" @click="close">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>

				<v-card-text>
					<v-row class="mt-4" style="padding: 5px 15px">
						<v-col cols="12" md="12" sm="12">
							<v-autocomplete :label="'包含哪些遊戲商(多選)'" v-model="exportGameVendors" multiple deletable-chips chips clearable :items="dropdownGameVendorsForCheckbox">
							</v-autocomplete>
						</v-col>

						<v-col cols="12" md="12" sm="12">
							<div class="">
								<v-autocomplete clearable :items="dropdownActiveStatus" chips v-model="searchFilter.status" :label="$t('TableHeader.__status')"> </v-autocomplete>
							</div>
						</v-col>
					</v-row>
				</v-card-text>

				<v-card-actions>
					<v-spacer></v-spacer>
					<v-btn text @click="close" class="btn">
						<v-icon left>mdi-cancel</v-icon>
						{{ $t('Button.__cancel') }}
					</v-btn>
					<v-btn color="yellow darken-1" text @click="exportFiles" class="btn">
						<v-icon left>mdi-check-circle</v-icon>
						{{ $t('Button.__downloadFile') }}
					</v-btn>
				</v-card-actions>
			</v-card>
		</v-dialog>

		<!-- 遊戲進階設定 -->
		<v-dialog v-model="isShowBatchEditCustomizedSetting" max-width="900px" scrollable>
			<v-card>
				<v-card-title class="grey darken-2">
					進階設置

					<span small class="f12 yellow-text ml-4"> {{ batchEditGameSetting.gameName }}({{ batchEditGameSetting.gameID }}) </span>

					<div class="top-right-item" @click="close">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>

				<v-card-text>
					<div class="batchEditGameSettingDetails">
						<v-row>
							<v-col v-for="(item, index) in batchEditGameSetting.items" :key="index" cols="12" md="12" sm="12">
								<div style="display: inline-block; width: 250px; vertical-align: super">
									<v-chip> {{ item.settingKey }}｜{{ item.settingKeyText }} </v-chip>
								</div>

								<v-text-field v-model="item.value" :label="'設置'" class="ml-4" :disabled="!isEnableEdit(item.settingKey)" style="width: 250px; display: inline-block"> </v-text-field>

								<div style="display: inline-block; width: 200px" class="ml-4">
									<v-btn class="mr-2" v-if="!isEnableEdit(item.settingKey)" style="" @click="switchToEnable(item.settingKey)"> 編輯 </v-btn>

									<v-btn class="mr-2" color="blue" v-if="isEnableEdit(item.settingKey)" @click="saveUpdateCustomizedSetting(item)"> 儲存 </v-btn>

									<v-btn class="mr-2" color="grey" v-if="isEnableEdit(item.settingKey)" @click="clearEditEnableFlag()"> 取消 </v-btn>

									<v-icon color="red darken-4 ml-2" @click="deleteGameListCustomizedSetting(item)"> close </v-icon>
								</div>
							</v-col>
						</v-row>
					</div>
				</v-card-text>
			</v-card>
		</v-dialog>

		<GameMerchantInfoDialog ref="gameMerchantInfo"></GameMerchantInfoDialog>

		<GameLaunchInternalTest ref="gameLaunchTest"></GameLaunchInternalTest>
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import GameLaunchInternalTest from '@/components/system-setting/GameLaunchInternalTest.vue';
import GameMerchantInfoDialog from '@/components/system-setting/GameMerchantInfoDialog.vue';
import EventBus from '@/utilities/event-bus';
import * as Model from '@/models/interfaces/system-setting/gameList';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import GameListApi from '@/api/system-setting/gameList';
import DropdownApi from '@/api/commons/dropdown';
import InternalTestApi from '@/api/commons/internalTest';
import { Locale, Currency, Status, GameType, CRUDAction, GameVendorType } from '@/models/status/common';
import { WalletMode } from '@/models/status/merchant';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { ALL_CURRENCY } from '@/utilities/enums_helper';
import NotificationWarning from '@/components/notification/NotificationWarning.vue';
import { pageData, time } from '@/utilities/api-handler.lib';
import { datetimeMixin, toLocalDateString } from '@/utilities/datetime-format';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import { commonMethodMixin } from '@/utilities/common-methods';
import { State, Action, Getter, namespace } from 'vuex-class';
const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';

@Component({ components: { NotificationWarning, GameLaunchInternalTest, GameMerchantInfoDialog }, mixins: [commonDisplayFiltersMixin, commonMethodMixin, datetimeMixin] })
export default class GameList extends Vue {
	dialogVisible: DropdownModel.DialogVisible = new DropdownModel.DialogVisible();
	locale: typeof Locale = Locale;
	currency: typeof Currency = Currency;
	status: typeof Status = Status;
	gameTypeDic: typeof GameType = GameType;
	WalletMode: typeof WalletMode = WalletMode;
	CRUDActionEnums: typeof CRUDAction = CRUDAction;
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];
	renderComponent: boolean = true;
	isShowEditDialog: boolean = false;
	isShowExportConfirm: boolean = false;
	isShowBatchEditCustomizedSetting: boolean = false;

	currentItems: any[] = [];
	search: string = '';
	gameListData: Model.IGameList[] = [];
	exportGameVendors: number[] = [];
	editedIndex: number = -1;
	editedItem: Model.IGameListRequest = {} as Model.IGameListRequest;
	editAction: CRUDAction = CRUDAction.Create;
	isEdit: boolean = false;
	isThirdPartyGameLockEdit: boolean = false;

	panel: number[] = [0];

	pageIndex: number = 1;
	itemsPerPage: number = 10;
	footerProps: any = { 'items-per-page-options': [10, 20, 100, -1] };

	batchEditGameSetting: Model.IGameBatchEditSettingModel = {} as Model.IGameBatchEditSettingModel;

	searchFilter: Model.IGameListPagingRequest = {} as Model.IGameListPagingRequest;

	enableEditSettingKeys: number[] = [];
	dropdownGameListExtraFilterType: DropdownModel.IDropdownItems[] = [];
	dropdownGameVendorsForCheckbox: DropdownModel.IDropdownItems[] = [];
	dropdownGameVendors: DropdownModel.IDropdownItems[] = [];
	dropdownGameTypes: DropdownModel.IDropdownItems[] = [];
	dropdownMerchants: DropdownModel.IDropdownItems[] = [];
	dropdownGames: DropdownModel.IDropdownItems[] = [];
	dropdownStatus: DropdownModel.IDropdownItems[] = [];

	get hasPermissionCreate() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSetting_GameList_Create) >= 0;
	}
	get hasPermissionUpdate() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSetting_GameList_Update) >= 0;
	}

	get hasCustomerServicePermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.Special_PGS_CustomerService) >= 0;
	}

	get headers() {
		const data = [
			{ text: '', value: 'gameImageUrl', sortable: false, align: 'start', width: '5%' },
			{ text: this.$t('TableHeader.__gameID'), value: 'gameID', sortable: false, align: 'start', width: '5%' },
			{ text: this.$t('TableHeader.__displayName'), value: 'gameName', sortable: false, width: '10%' },

			{ text: '中文／英文', value: 'gameName_CN', sortable: false, width: '8%' },

			{ text: this.$t('TableHeader.__type') + '／' + '游戏商', value: 'gameTypeText', sortable: false },

			{ text: this.$t('TableHeader.__status'), value: 'status', sortable: false, align: 'center' },

			{ text: this.$t('TableHeader.__assignMerchantTotalCount'), value: 'numberOfMerchants', sortable: false, align: 'center' },
			{ text: this.$t('TableHeader.__gameCode'), value: 'gameCode' },
			{ text: '支援單錢包', value: 'isSupportSingleWallet', sortable: false, align: 'center', width: '6%' },
			{ text: '允許啟動遊戲', value: 'isForbiddenGameLaunch', sortable: false, align: 'center', width: '6%' },
			{ text: '加注流程', value: 'isSupportRaiseProcess', sortable: false, align: 'center', width: '6%' },
			{ text: '版型', value: 'supportedOrientationCN', sortable: false, align: 'center', width: '6%' },
			{ text: this.$t('TableHeader.__memo'), value: 'isMultiLanguageDefined', sortable: false, width: '10%' },

			// 為了讓關鍵字搜尋可以查到
			{ text: '', value: 'extraTextForKeywordSearch', sortable: false, width: '1%' },
			{ text: '', value: 'memo', sortable: false, width: '1%' },
		];

		if (true) {
			data.splice(0, 0, { text: this.$t('TableHeader.__action'), value: 'action', align: 'center', sortable: false, width: '8%' });
		}
		return data;
	}

	generateInsertSQL(gameID: number) {
		const vm: any = this;

		EventBus.$emit('show_mask', null);

		InternalTestApi.generateInsertSQLByID(gameID, 'gameList')
			.then((res) => {
				vm.copyTextToClipboard2(res.data.sql);
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	get dropdownActiveStatus() {
		return [
			{ text: this.$t('Status.Enable'), value: 1 },
			{ text: this.$t('Status.Disable'), value: 0 },
		];
	}

	get dropdownSpecialQuery() {
		// 對應 enum AdminSysGameQueryExtraInfoEnum
		return [
			{ text: '顯示PGS icon圖片', value: 101 },
			{ text: '顯示BWG icon圖片', value: 102 },
		];
	}

	showExportConfirm() {
		this.isShowExportConfirm = true;
		this.exportGameVendors = [];
	}

	showGameMerchantDialog(item: Model.IGameList) {
		const targetRef: any = this.$refs.gameMerchantInfo;
		targetRef.searchByGameID(item.gameID);
	}

	refreshInternalAccountDropdown() {
		const targetRef: any = this.$refs.gameLaunchTest;
		targetRef.refreshInternalAccountDropdown();
	}

	showGameLaunchDialog(item: Model.IGameList) {
		const targetRef: any = this.$refs.gameLaunchTest;

		const model: Model.IGameLaunchPreview = {} as Model.IGameLaunchPreview;
		model.gameID = item.gameID;
		model.gameName = item.gameName;
		model.vendorName = item.gameVendorTypeText;

		targetRef.showGameLaunchDialog(model);
	}

	searchTable() {
		this.pageIndex = 1;
		this.refreshInternalAccountDropdown();
		this.refresh();
	}

	refresh() {
		this.registerReloadComponent();

		EventBus.$emit('show_mask', null);
		GameListApi.getGames(this.searchFilter)
			.then((res) => {
				this.gameListData = res.data.items;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	setCurrentItems(items: any[]) {
		this.currentItems = items;
	}

	copyCurrentItems() {
		let copyText = '';

		for (const item of this.currentItems) {
			copyText += item.gameID + '\t' + item.gameName + '\n';
		}

		const vm: any = this;
		vm.copyTextToClipboard(copyText);
	}

	registerReloadComponent() {
		EventBus.$on(`reload-${this.$router.currentRoute.path}`, (info: any) => {
			this.initialDropdown();
			this.renderComponent = false;
			this.$nextTick(() => {
				this.renderComponent = true;
			});
		});
	}

	addItem() {
		this.editAction = CRUDAction.Create;
		this.isShowEditDialog = true;
		this.editedItem = {} as Model.IGameListRequest;

		this.resetFormValidate();

		this.editedItem.gameType = GameType.None;
		this.editedItem.gameVendorType = GameVendorType.None;
		this.editedItem.status = Status.Enable;
	}

	editItem(item: Model.IGameList) {
		this.editAction = CRUDAction.Update;
		this.isShowEditDialog = true;
		this.isThirdPartyGameLockEdit = item.isThirdPartyGame;

		this.resetFormValidate();

		this.editedItem.gameType = item.gameType;
		this.editedItem.gameVendorType = item.gameVendorType;
		this.editedItem.status = item.status;
		this.editedItem.gameName = item.gameName;
		this.editedItem.gameID = item.gameID;
		this.editedItem.gameCode = item.gameCode;
		this.editedItem.memo = item.memo;
	}

	resetFormValidate() {
		const form: any = this.$refs.loginForm;
		form.resetValidation();
	}

	showBatchEditSettings(gameID: number) {
		this.batchEditGameSetting = {} as Model.IGameBatchEditSettingModel;
		this.isShowBatchEditCustomizedSetting = true;

		this.clearEditEnableFlag();

		EventBus.$emit('show_mask', null);
		GameListApi.getBatchEditCustomizedSetting(gameID)
			.then((res) => {
				this.batchEditGameSetting = res.data;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	saveUpdateCustomizedSetting(item: Model.IGameCustomizedSettingEditItemModel) {
		EventBus.$emit('show_mask', null);
		GameListApi.updateGameListCustomizedSetting(item)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.clearEditEnableFlag();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	deleteGameListCustomizedSetting(item: Model.IGameCustomizedSettingEditItemModel) {
		if (confirm('此按鈕僅供技術人員使用。是否確定移除此配置? (' + item.settingKeyText + ') ')) {
			EventBus.$emit('show_mask', null);
			GameListApi.deleteGameListCustomizedSetting(item)
				.then((res) => {
					EventBus.$emit(`successfully-modified`);
					this.clearEditEnableFlag();
					this.close();
				})
				.finally(() => {
					EventBus.$emit('hide_mask', null);
				});
		}
	}

	isEnableEdit(key: number) {
		return this.enableEditSettingKeys.includes(key);
	}

	switchToEnable(key: number) {
		this.enableEditSettingKeys = [];
		this.enableEditSettingKeys.push(key);
	}

	clearEditEnableFlag() {
		this.enableEditSettingKeys = [];
	}

	close() {
		this.isShowEditDialog = false;
		this.isShowExportConfirm = false;
		this.isShowBatchEditCustomizedSetting = false;
		setTimeout(() => {
			this.editedItem.status = Status.Enable;
			this.editedIndex = -1;
		}, 300);
	}

	clear() {
		this.searchFilter = {} as Model.IGameListPagingRequest;
	}

	save() {
		const form: any = this.$refs.loginForm;

		if (form.validate()) {
			const request = <Model.IGameListRequest>{ ...this.editedItem };
			request.status = request.status;

			EventBus.$emit('show_mask', null);
			if (this.editAction === CRUDAction.Create) {
				GameListApi.createGames(<Model.IGameListRequest>request)
					.then(() => {
						EventBus.$emit(`successfully-modified`);
						this.close();
						this.refresh();
					})
					.finally(() => {
						EventBus.$emit('hide_mask', null);
					});
			} else {
				GameListApi.editGames(<Model.IGameListRequest>request)
					.then(() => {
						EventBus.$emit(`successfully-modified`);
						this.close();
						this.refresh();
					})
					.finally(() => {
						EventBus.$emit('hide_mask', null);
					});
			}
		}
	}

	async exportFiles() {
		const excelHeaders: any = [
			{ text: this.geti18nTextAsString('TableHeader.__gameID'), width: 12 },
			{ text: this.geti18nTextAsString('TableHeader.__displayName') + '(簡中)', width: 18 },
			{ text: this.geti18nTextAsString('TableHeader.__displayName') + '(English)', width: 18 },
			{ text: this.geti18nTextAsString('TableHeader.__gameType'), width: 18 },
		];

		const isShowStatusColumn = this.searchFilter.status !== 1;

		if (isShowStatusColumn) {
			excelHeaders.push({ text: this.geti18nTextAsString('TableHeader.__status'), width: 12 });
		}

		excelHeaders.push({ text: this.geti18nTextAsString('TableHeader.__currency'), width: 50 });
		excelHeaders.push({ text: this.geti18nTextAsString('TableHeader.__layoutOrientation'), width: 15 });
		excelHeaders.push({ text: this.geti18nTextAsString('TableHeader.__imageUrl'), width: 30 });

		const excelData: any = [];

		EventBus.$emit('show_mask', null);

		const myFilter = { ...this.searchFilter };
		myFilter.gameVendorType = null;
		myFilter.gameVendorTypes = this.exportGameVendors;
		myFilter.excludeGameID = 0;

		const result = await GameListApi.getGames(myFilter).finally(() => {
			setTimeout(() => {
				EventBus.$emit('hide_mask', null);
			}, 500);
		});

		result.data.items.forEach((value) => {
			const eachRow: any = [];

			eachRow.push(value.gameID);
			eachRow.push(value.gameName_CN);
			eachRow.push(value.gameName_EN);
			eachRow.push(value.gameTypeText);

			if (isShowStatusColumn) {
				eachRow.push(value.status === 1 ? 'v' : 'Disabled');
			}

			eachRow.push(value.supportCurrency);
			eachRow.push(value.supportedOrientationEN);
			eachRow.push(value.gameImageUrl);
			excelData.push(eachRow);
		});

		let dateString: any = toLocalDateString(Date.now(), 8);
		dateString = dateString.replaceAll('-', '');

		const config = {
			headersWithConfig: excelHeaders,
			rowData: excelData,
			fileName: 'Export_GameList_' + dateString + '.xlsx',
		};

		// @ts-ignore
		this.downloadExcel(config);
	}

	geti18nTextAsString(key: string) {
		return this.$t(key).toString();
	}

	copyGameNameInfo(item: Model.IGameList) {
		const vm: any = this;

		let copyText = 'GameID：' + item.gameID + ' (游戏商请忽略此栏位)' + '\n';
		copyText += '遊戲名稱：' + item.gameName_CN + '／' + item.gameName_EN + '\n';
		copyText += '遊戲代碼：' + item.thirdPartyGameCode + '\n';
		copyText += '遊戲商：' + item.gameVendorTypeText + '\n';

		vm.copyTextToClipboard(copyText);
	}

	initialDropdown() {
		DropdownApi.getGameTypes().then((res) => {
			this.dropdownGameTypes = res;
		});

		DropdownApi.getGameVendors().then((res) => {
			res.push({ text: 'Slot88', value: 2001, valueInt: 2001 } as DropdownModel.IDropdownItems);
			this.dropdownGameVendors = res;
			this.dropdownGameVendorsForCheckbox = res;
		});

		DropdownApi.getMerchants(true).then((res) => {
			this.dropdownMerchants = res;
		});

		DropdownApi.getNormalStatus(true).then((res) => {
			this.dropdownStatus = res;
		});

		DropdownApi.getGameListExtraFilterType().then((res) => {
			this.dropdownGameListExtraFilterType = res;
		});
	}

	mounted() {
		this.initialDropdown();

		this.refresh();

		const vm: any = this;
		document.addEventListener('keydown', (event) => {
			const key = event.key; // const {key} = event; in ES6+
			if (key === 'Escape') {
				vm.close();
			}
		});
	}
}
</script>

<style>
#getGameLinkDialog .v-text-field__details {
	display: none;
}

.statusChips .v-chip {
	min-width: 70px;
	text-align: center;
}

.statusChips .v-chip .v-chip__content {
	margin: auto;
}

.batchEditGameSettingDetails .col-12 {
	padding: 3px 12px;
}

.gameType_0 {
	color: #db7cd2;
}
</style>
