<template>
	<v-app v-if="renderComponent" id="refTableDisplay" style="max-height: 95vh; overflow: scroll">
		<v-row>
			<v-col cols="12">
				<v-row justify="center">
					<v-col>
						<v-autocomplete
							deletable-chips
							v-model="selectMerchants"
							clearable
							:items="merchantsDropdownItems"
							dense
							chips
							small-chips
							:label="$t('TableHeader.__merchant')"
							multiple
							solo
							outlined
						>
						</v-autocomplete>
					</v-col>
					<v-col>
						<v-btn color="yellow darken-1 " text class="btn mr-2" @click.stop="search">
							<v-icon left>search</v-icon>
							{{ $t('Button.__search') }}
						</v-btn>

						<v-btn color="pink lighten-3" text class="btn mr-2" @click.stop="clear">
							<v-icon left>clear</v-icon>
							{{ $t('Button.__reset') }}
						</v-btn>

						<v-btn color="green lighten-2" text class="btn mr-2" @click.stop="showCopySettingDialog">
							<v-icon left>file_copy</v-icon>
							複製商戶設置
						</v-btn>

						<v-btn color="blue lighten-2" text class="btn mr-2" @click.stop="showBatchEditMultipleMerchantGameSettingDialog">
							<v-icon left>edit</v-icon>
							批次編輯開關
						</v-btn>
					</v-col>
				</v-row>
				<v-row>
					<v-row>
						<v-col v-for="(table, index) in tables" :key="index" cols="12" sm="6" md="6" lg="4">
							<v-data-table :headers="headers" id="table.merchantID" :items="table.gameSettingItems" :search="table.search" :footer-props="footerProps" multi-sort>
								<template v-slot:top>
									<div style="padding: 5px; text-align: right">
										<v-btn color="pink lighten-3" text class="btn search ml-1" v-if="table.numberOfChildMerchant > 0" @click.stop="showSyncAllChildDialog(table)">
											<v-icon left>refresh</v-icon>
											與下級同步開關 ({{ table.numberOfChildMerchant }}) </v-btn
										>&#9;

										<v-btn color="light-blue lighten-3" text class="btn search ml-1" @click.stop="exportFiles(table.merchantID, table.gameSettingItems)">
											<v-icon left>cloud_download</v-icon>
											{{ $t('Button.__downloadFile') }} </v-btn
										>&#9;
									</div>

									<div>
										<v-toolbar style="display: flex">
											<div class="text-max-overflow" style="flex: 3; line-height: 40px; padding-right: 25px">
												<h4>{{ table.merchantName }}</h4>
											</div>

											<div style="flex: 1" class="padding-top-adjust">
												<v-text-field v-model="table.search" append-icon="search" label="Search" single-line hide-details clearable> </v-text-field>
											</div>
										</v-toolbar>
									</div>
								</template>

								<template v-slot:item.gameID="{ item }">
									<div
										class="link-text-white"
										@click="showGameMerchantDialog(item.gameID)"
										v-if="!item.isGameListDisable"
										style="overflow: hidden; text-overflow: ellipsis; white-space: nowrap; width: 200px; font-size: 13px"
									>
										{{ item.gameID }}｜{{ item.gameName }}
									</div>

									<div class="minus-red" v-if="item.isGameListDisable" style="overflow: hidden; text-overflow: ellipsis; white-space: nowrap; width: 200px; font-size: 13px">
										<div>{{ item.gameID }}｜{{ item.gameName }}</div>
									</div>
								</template>

								<template v-slot:item.vendorConfigName="{ item }">
									<div style="color: #bbb">
										<div v-if="item.isGameListDisable" class="minus-red"><v-icon small color="red lighten-2">error</v-icon> 遊戲已關閉</div>
										<div v-else-if="item.gameID <= 9999">-</div>
										<div v-else :title="item.vendorConfigID" style="font-size: 12px">
											{{ item.vendorConfigName }}
										</div>
									</div>
								</template>

								<template v-slot:item.convertRate="{ item }">
									<div :class="item.convertRate > 1 ? 'minus-red' : ''">
										{{ item.convertRate }}
									</div>
								</template>

								<template v-slot:item.action="{ item }">
									<div v-if="!item.isEditDisabledDueToSingleWallet">
										<v-switch v-model="item.status" color="yellow darken-3" class="mt-3 mt-0" @change="changeStatus(table, item)"> </v-switch>
									</div>

									<div v-else class="f10">
										<v-tooltip right color="blue darken-1">
											<template v-slot:activator="{ on, attrs }">
												<div v-bind="attrs" v-on="on">
													<v-switch v-model="item.status" color="yellow darken-3" class="mt-3 mt-0" disabled="disabled"> </v-switch>
												</div>
											</template>
											<span>
												<div>不支援單錢包</div>
											</span>
										</v-tooltip>
									</div>
								</template>
							</v-data-table>
						</v-col>
					</v-row>

					<div v-if="tables && tables.length > 0" style="position: fixed; bottom: 5%; right: 5%; z-index: 9999; font-size: 36px">
						<v-icon large @click="scrollTo(-500)">mdi-arrow-up-drop-circle</v-icon>
						<v-icon large @click="scrollTo(99999)">mdi-arrow-down-drop-circle</v-icon>
					</div>
				</v-row>
			</v-col>
		</v-row>

		<!-- 複製商戶遊戲開關 -->
		<v-dialog v-model="dialogVisible.isShowCopyDialog" max-width="650px">
			<v-card>
				<v-card-title class="grey darken-2">
					複製商戶遊戲開關
					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark @click="">
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>
				<v-card-text>
					<v-flex style="padding: 10px">
						<v-row>
							<v-autocomplete clearable :items="merchantsDropdownItems" v-model="copyForm.sourceMerchantID" :label="'以此商戶設置做為基準複製'"> </v-autocomplete>
						</v-row>

						<v-row>
							<v-autocomplete clearable :items="merchantsDropdownItems" v-model="copyForm.copyToMerchantID" :label="'複製到此商戶'"> </v-autocomplete>
						</v-row>

						<v-row>
							<v-checkbox v-model="copyForm.isDeleteOriginalGames" :label="'複製時先清空此商戶全部遊戲開關'"> </v-checkbox>
						</v-row>
					</v-flex>
				</v-card-text>
				<v-card-actions>
					<v-spacer></v-spacer>
					<v-btn text @click="closeAllModal" class="btn">
						<v-icon left>mdi-cancel</v-icon>
						{{ $t('Button.__cancel') }}
					</v-btn>
					<v-btn color="yellow darken-1" text @click="saveCopySetting" class="btn">
						<v-icon left>mdi-check-circle</v-icon>
						{{ $t('Button.__submit') }}
					</v-btn>
				</v-card-actions>
			</v-card>
		</v-dialog>

		<!-- 同步所有遊戲開關至下級代理 -->
		<v-dialog v-model="dialogVisible.isShowPopup_1" max-width="400px">
			<v-card>
				<v-card-title class="grey darken-2">
					是否同步所有遊戲開關至下級代理
					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark @click="">
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>
				<v-card-text>
					<v-flex style="padding: 10px">
						<v-row>
							<v-text-field :label="$t('TableHeader.__parent')" disabled="disabled" :value="syncFrom.merchantName"> </v-text-field>
						</v-row>

						<v-row>
							<v-text-field :label="'預計受影響商戶數'" :value="syncFrom.numberOfChildMerchant" disabled="disabled"> </v-text-field>
						</v-row>
					</v-flex>
				</v-card-text>
				<v-card-actions>
					<v-spacer></v-spacer>
					<v-btn text @click="closeAllModal" class="btn">
						<v-icon left>mdi-cancel</v-icon>
						{{ $t('Button.__cancel') }}
					</v-btn>
					<v-btn color="yellow darken-1" text @click="saveSyncSettingToChild" class="btn">
						<v-icon left>mdi-check-circle</v-icon>
						{{ $t('Button.__submit') }}
					</v-btn>
				</v-card-actions>
			</v-card>
		</v-dialog>

		<!-- 批次編輯多個商戶的遊戲開關 -->
		<v-dialog v-model="dialogVisible.isShowBatchEdit" max-width="1000px" scrollable>
			<v-card>
				<v-card-title class="grey darken-2">
					批次編輯多個商戶的遊戲開關
					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>
				<v-card-text>
					<div style="padding: 10px">
						<v-row>
							<v-autocomplete :label="'操作方式'" :items="batchOperateDropdown" chips @change="whenChangeBatchOperateType()" v-model="batchOperateType"> </v-autocomplete>
						</v-row>

						<v-row class="textarea-bg" style="display: block" v-if="batchOperateType === 0">
							<div style="">
								<v-textarea v-model="multipleMerchantEditForm.rawText" :label="'請貼上商戶ID (支援幾種格式)'" class="textarea-bg"> </v-textarea>
							</div>

							<div style="margin-top: -5px; margin-bottom: 5px; text-align: right; color: #888">
								<span v-if="calculateTotalMerchants() > 0" class="orange-text mr-4"> 共: {{ calculateTotalMerchants() }} 筆 </span>

								<v-icon small style="color: #888">error</v-icon> 建議搭配此功能 [商戶管理] > [複製當前列表所有商戶ID]
							</div>
							<div style="clear: both"></div>
						</v-row>

						<v-row class="textarea-bg" style="display: block" v-if="batchOperateType === 1">
							<v-autocomplete :label="'指定上級代理 (所有下級代理將被同步設置)'" :items="agentMerchantDropdownItems" chips v-model="multipleMerchantEditForm.parentMerchantID">
							</v-autocomplete>
						</v-row>

						<v-row>
							<v-autocomplete :items="multipleMerchantEditType" :label="$t('TableHeader.__operationType')" chips v-model="multipleMerchantEditForm.type"> </v-autocomplete>
						</v-row>

						<v-row style="padding-bottom: 250px" v-if="isRenderElement" class="multipleSelectGames">
							<v-autocomplete
								:items="enableGamesDropdownItems"
								small-chips
								multiple
								clearable
								deletable-chips
								v-model="multipleMerchantEditForm.gameIDs"
								:label="$t('TableHeader.__gameName')"
							>
							</v-autocomplete>

							<div style="width: 100%">
								<span @click="showParseTextToListIntDialog()" class="link-text-grey f12 minus-mt-2 z-999">🔧貼上多筆GameIDs</span>

								<span class="link-text mr-2 f-r" @click="selectGamesByVendor(650000)"> EFG </span>
								<span class="link-text mr-2 f-r" @click="selectGamesByVendor(630000)"> Fuma </span>
								<span class="link-text mr-2 f-r" @click="selectGamesByVendor(610000)"> VPlus </span>
								<!-- 
								<span class="link-text mr-2 f-r" @click="selectGamesByVendor(590000)"> RG </span>
								<span class="link-text mr-2 f-r" @click="selectGamesByVendor(570000)"> ClotPlay </span>			 -->
								<!-- <span class="link-text mr-2 f-r" @click="selectGamesByVendor(550000)"> AGin真人遊戲 </span> -->
								<!-- 
								<span class="link-text mr-2 f-r" @click="selectGamesByVendor(490000)"> AT捕魚 </span>
								<span class="link-text mr-2 f-r" @click="selectGamesByVendor(480000)"> ATGames </span> -->

								<!-- <span class="link-text mr-2 f-r" @click="selectGamesByVendor(450000)">
									yesGame
								</span> -->
								<span class="link-text mr-2 f-r" @click="selectGamesByVendor(430000)">Mario</span>
								<!-- <span class="link-text mr-2 f-r" @click="selectGamesByVendor(410000)">GPI</span> -->
								<span class="link-text mr-2 f-r" @click="selectGamesByVendor(390000)">Jili</span>
								<!-- <span class="link-text mr-2 f-r" @click="selectGamesByVendor(330000)">
									勾選AGGLive
								</span> -->
								<span class="link-text mr-2 f-r" @click="selectGamesByVendor(290000)">AGG</span>
								<span class="link-text mr-2 f-r" @click="selectGamesByVendor(250000)">Funky</span>
								<span class="link-text mr-2 f-r" @click="selectGamesByVendor(0)">target</span>
							</div>
						</v-row>
					</div>
				</v-card-text>
				<v-card-actions>
					<v-spacer></v-spacer>
					<v-btn text @click="closeAllModal" class="btn">
						<v-icon left>mdi-cancel</v-icon>
						{{ $t('Button.__cancel') }}
					</v-btn>
					<v-btn color="yellow darken-1" text @click="saveBatchEditMultipleMerchantGameSetting" class="btn">
						<v-icon left>mdi-check-circle</v-icon>
						{{ $t('Button.__submit') }}
					</v-btn>
				</v-card-actions>
			</v-card>
		</v-dialog>

		<NotificationWarning
			v-show="showNotificationWarning"
			:title="`${$t('Message.__warning')}`"
			:max="1"
			:text="`${$t('Message.__isConfirm')}${actionText}?`"
			:confirmed-action="confirmedAction"
			:canceled-action="canceledAction"
		/>

		<GameMerchantInfoDialog ref="gameMerchantInfo"></GameMerchantInfoDialog>

		<TextareaParseToListIDPopup ref="refTextareaParseToListIDPopup" @parentSetResultIDs="setResultGameIDs"> </TextareaParseToListIDPopup>
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import GameMerchantInfoDialog from '@/components/system-setting/GameMerchantInfoDialog.vue';
import TextareaParseToListIDPopup from '@/components/system-setting/TextareaParseToListIDPopup.vue';
import { Component, Watch } from 'vue-property-decorator';
import { namespace } from 'vuex-class';
import EventBus from '@/utilities/event-bus';
import { Status } from '@/models/status/common';
import { ServerStatusEnum } from '@/models/status/system';
import DropdownApi from '@/api/commons/dropdown';
import internalTestApi from '@/api/commons/internalTest';
import GameSettingSwitchApi from '@/api/game-setting/switch';
import * as Model from '@/models/interfaces/game-setting/switch';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import NotificationWarning from '@/components/notification/NotificationWarning.vue';
import NotificationError from '@/components/notification/NotificationError.vue';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import { commonMethodMixin } from '@/utilities/common-methods';

const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';
@Component({ components: { NotificationWarning, NotificationError, GameMerchantInfoDialog, TextareaParseToListIDPopup }, mixins: [commonDisplayFiltersMixin, commonMethodMixin] })
export default class MerchantGameSettingSwitchList extends Vue {
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];
	@authModule.State('defaultLangTimes') defaultLangTimes!: number;

	isRenderElement: boolean = true;
	batchOperateType: number = 0;
	status: typeof Status = Status;
	serverStatusEnum: typeof ServerStatusEnum = ServerStatusEnum;
	renderComponent: boolean = true;
	merchantsDropdownItems: DropdownModel.IDropdownItems[] = [];
	selectMerchants: number[] = [];
	tables: Model.ITable[] = [];
	get hasPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.GameSettingSwitch_Update) >= 0;
	}

	get batchOperateDropdown() {
		const array = [];
		array.push({ text: '自行輸入商戶號', value: 0 });
		array.push({ text: '勾選上級代理、並同時操作所有下級代理', value: 1 });
		return array;
	}

	get headers() {
		const data = [
			{ text: this.$t('TableHeader.__gameID'), value: 'gameID', sortable: true },
			{ text: this.$t('TableHeader.__action'), value: 'action', sortable: false },
			{ text: this.$t('TableHeader.__vendorConfig'), value: 'vendorConfigName', align: 'center' },

			{ text: this.$t('TableHeader.__convertRate'), value: 'convertRate', align: 'center' },
		];
		if (this.hasPermission) {
			// data.push({ text: this.$t('TableHeader.__action'), value: 'action', sortable: false });
		}
		return data;
	}

	get multipleMerchantEditType() {
		const data = [
			{ text: '開通遊戲', value: 1 },
			{ text: '關閉遊戲', value: 9 },
		];

		return data;
	}

	actionText: string = '';

	copyForm: Model.IGameSettingCopyRequest = {} as Model.IGameSettingCopyRequest;

	dialogVisible: DropdownModel.DialogVisible = new DropdownModel.DialogVisible();

	showNotificationInfo: boolean = true;
	showNotificationWarning: boolean = false;

	adminSysFavoritSetting: any = {};
	editedTable: Model.ITable = {} as Model.ITable;
	editedItem: Model.IGameSettingItem = {} as Model.IGameSettingItem;

	syncFrom: Model.ITable = {} as Model.ITable;
	multipleMerchantEditForm: Model.IMultipleMerchantAdjustGameSetting = {} as Model.IMultipleMerchantAdjustGameSetting;

	footerProps: Model.IDataTablePageObject = {
		'items-per-page-options': [1000],
	} as Model.IDataTablePageObject;

	merchantDropdownItems: DropdownModel.IDropdownItems[] = [];
	agentMerchantDropdownItems: DropdownModel.IDropdownItems[] = [];
	gamesDropdownItems: DropdownModel.IDropdownItems[] = [];
	enableGamesDropdownItems: DropdownModel.IDropdownItems[] = [];

	@Watch('defaultLangTimes')
	onDefaultLangChanged() {
		if (this.tables.length > 0) {
			this.search();
		}
		this.search();
	}

	registerReloadComponent() {
		EventBus.$on(`reload-${this.$router.currentRoute.path}`, (info: any) => {
			this.renderComponent = false;
			this.$nextTick(() => {
				this.selectMerchants = [];
				this.tables = [];
				this.init();
				this.renderComponent = true;
			});
		});
	}

	showBatchEditMultipleMerchantGameSettingDialog() {
		this.multipleMerchantEditForm = {} as Model.IMultipleMerchantAdjustGameSetting;
		this.dialogVisible.isShowBatchEdit = true;
		this.multipleMerchantEditForm.gameIDs = [];
	}

	saveBatchEditMultipleMerchantGameSetting() {
		EventBus.$emit('show_mask', null);
		GameSettingSwitchApi.saveMultipleMerchantAdjustGameSetting(this.multipleMerchantEditForm)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				alert(res.data.message);
				this.closeAllModal();

				if (res.data.resultListIDs && res.data.resultListIDs.length > 0) {
					this.selectMerchants = [];
					let loopTimes: number = 0;

					// 儲存更新後，自動搜尋剛剛更新的商戶 (前三筆)
					res.data.resultListIDs.forEach((merchantID) => {
						if (loopTimes >= 3) {
							return;
						}

						this.selectMerchants.push(merchantID);
						loopTimes++;
					});

					this.search();
				}
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	showCopySettingDialog() {
		this.copyForm = {} as Model.IGameSettingCopyRequest;
		this.dialogVisible.isShowCopyDialog = true;
	}

	showSyncAllChildDialog(item: Model.ITable) {
		this.syncFrom = item;
		this.dialogVisible.isShowPopup_1 = true;
	}

	saveSyncSettingToChild() {
		EventBus.$emit('show_mask', null);
		GameSettingSwitchApi.syncSettingToChilds(this.syncFrom.merchantID)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.closeAllModal();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	closeAllModal() {
		this.dialogVisible.resetAllValueToFalse();
	}

	clear() {
		this.selectMerchants = [];
	}

	whenChangeBatchOperateType() {
		const vm: any = this;
		setTimeout(() => {
			this.multipleMerchantEditForm = {} as Model.IMultipleMerchantAdjustGameSetting;
		}, 250);
	}

	search() {
		this.tables = [];

		if (this.selectMerchants == null || this.selectMerchants.length === 0) {
			EventBus.$emit('auto-hidden-error', this.$t('Message.__pleaseSelectMerchant'));
			return;
		}
		let index: number = 0;

		EventBus.$emit('show_mask', null);

		this.selectMerchants.forEach((merchantID: number) => {
			const item = this.merchantsDropdownItems.find((s) => s.value === merchantID);

			GameSettingSwitchApi.getGames(this.selectMerchants[index])
				.then((res) => {
					this.tables.push(<Model.ITable>{
						merchantID: item && item.value,
						merchantName: item && item.text,
						numberOfChildMerchant: res.data.numberOfChildMerchant,
						gameSettingItems: res.data.gameSettingItems,
					});
					this.tables.sort((a, b) => {
						return a.merchantID - b.merchantID;
					});
				})
				.finally(() => {
					setTimeout(() => {
						EventBus.$emit('hide_mask', null);
					}, 500);
				});

			index++;
		});
	}

	saveCopySetting() {
		EventBus.$emit('show_mask', null);

		GameSettingSwitchApi.CopySetting(this.copyForm)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.closeAllModal();

				if (this.selectMerchants.length === 0) {
					this.selectMerchants.push(this.copyForm.sourceMerchantID);
					this.selectMerchants.push(this.copyForm.copyToMerchantID);
					this.search();
				}
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	geti18nTextAsString(key: string) {
		return this.$t(key).toString();
	}

	exportFiles(merchantID: number, merchantGames: Model.IGameSettingItem[]) {
		const excelHeaders: any = [
			this.geti18nTextAsString('TableHeader.__gameID'),
			this.geti18nTextAsString('TableHeader.__displayName') + '(Chinese)',
			this.geti18nTextAsString('TableHeader.__displayName') + '(English)',
			this.geti18nTextAsString('TableHeader.__type'),
			this.geti18nTextAsString('TableHeader.__imageUrl'),
		];

		const excelData: any = [];

		const enableGames = merchantGames
			.filter((x) => {
				return x.status === 1;
			})
			.map((item, index, array) => {
				return item;
			});

		enableGames.forEach((value) => {
			const eachRow: any = [];

			eachRow.push(value.gameID);
			eachRow.push(value.gameNameCN);
			eachRow.push(value.gameNameEN);
			eachRow.push(value.gameTypeText);
			eachRow.push(value.imageUrl);

			excelData.push(eachRow);
		});

		console.log(excelHeaders);
		console.log(merchantGames);

		const config = {
			headers: excelHeaders,
			rowData: excelData,
			fileName: 'Export_MerchantGames_' + merchantID.toString() + '.xlsx',
		};

		// @ts-ignore
		this.downloadExcel(config);
	}

	selectGamesByVendor(vendor: number) {
		const minGameID = vendor;
		let maxGameID = minGameID + 3000;

		// PGS遊戲的話只勾選slots
		if (vendor === 0) {
			maxGameID = 1999;
		}

		this.multipleMerchantEditForm.gameIDs = this.multipleMerchantEditForm.gameIDs.filter((gameID) => !(gameID >= minGameID && gameID < maxGameID));

		if (vendor <= -1) {
			this.multipleMerchantEditForm.gameIDs = [];
			return;
		}

		// 未正式上線/獨家遊戲 不要被勾選
		const ignoreGameIDs: number[] = this.adminSysFavoritSetting.invisibleGameIDs_ForSelectedAll;

		this.enableGamesDropdownItems.forEach((x) => {
			const targetGameID = Number(x.valueInt);
			if (targetGameID >= minGameID && targetGameID < maxGameID && targetGameID !== 430998) {
				if (!ignoreGameIDs.includes(targetGameID)) {
					this.multipleMerchantEditForm.gameIDs.push(targetGameID);
				}
			}
		});

		this.isRenderElement = false;
		this.isRenderElement = true;
		console.log(this.multipleMerchantEditForm.gameIDs);
	}

	changeStatus(table: Model.ITable, item: Model.IGameSettingItem) {
		this.editedTable = table;
		this.editedItem = item;

		if (item.status) {
			item.status = item.status || item.status === Status.Enable ? Status.Enable : Status.Disable;
			this.confirmedAction();
		} else {
			this.actionText = `${this.$t('Message.__is')}${item.status ? this.$t('Message.__open') : this.$t('Message.__close')}[${table.merchantName}]-${this.$t('Message.__game')}${item.gameName}`;
			this.showNotificationWarning = true;
			this.$notify({
				group: 'confirm',
				type: 'warn',
				duration: -1,
				speed: 10,
			});
		}
		item.status = item.status || item.status === Status.Enable ? Status.Enable : Status.Disable;
	}
	confirmedAction() {
		const self = this;
		GameSettingSwitchApi.updateGames(<Model.IGameSettingItem>{
			merchantID: self.editedTable.merchantID,
			gameID: self.editedItem.gameID,
			status: self.editedItem.status === false || self.editedItem.status === Status.Disable ? Status.Disable : Status.Enable,
		})
			.then(() => {
				EventBus.$emit(`successfully-modified`);
			})
			.catch(() => {
				self.canceledAction();
			})
			.finally(() => {
				self.showNotificationWarning = false;
			});
	}
	canceledAction() {
		const self = this;
		const editTable = self.tables.find((s) => s.merchantID === self.editedTable.merchantID);
		if (editTable) {
			const item = editTable.gameSettingItems.find((s) => s.gameID === self.editedItem.gameID);
			if (item) {
				item.status = item.status === false || item.status === Status.Disable ? Status.Enable : Status.Disable;
			}
		}
		self.showNotificationWarning = false;
	}

	calculateTotalMerchants() {
		try {
			const lines = this.multipleMerchantEditForm.rawText
				.split(/\r?\n/) // 根據換行符號分段
				.filter((line) => line.trim() !== '').length;

			return lines;
		} catch (error) {
			return 0;
		}
	}

	showParseTextToListIntDialog() {
		const targetRef: any = this.$refs.refTextareaParseToListIDPopup;
		targetRef.showParseTextareaPraseToIDs();
	}

	setResultGameIDs(IDs: number[]) {
		this.multipleMerchantEditForm.gameIDs = [];
		for (const id of IDs) {
			this.multipleMerchantEditForm.gameIDs.push(id);
		}

		this.dialogVisible.isShowBatchEdit = false;
		this.$nextTick(() => {
			this.dialogVisible.isShowBatchEdit = true;
		});
	}

	scrollTo(value: number) {
		const myDiv: any = document.getElementById('refTableDisplay');
		myDiv.scrollTop = value;
	}

	showGameMerchantDialog(gameID: number) {
		const targetRef: any = this.$refs.gameMerchantInfo;
		targetRef.searchByGameID(gameID);
	}

	init() {
		DropdownApi.getMerchants().then((res) => {
			this.merchantsDropdownItems = res;
		});

		DropdownApi.getHierarchyMerchants().then((res) => {
			this.agentMerchantDropdownItems = res;
		});

		DropdownApi.getGames().then((res) => {
			this.gamesDropdownItems = res;
		});

		DropdownApi.getEnableGames().then((res) => {
			this.enableGamesDropdownItems = res;
		});

		internalTestApi.getAdminSysFavoritSetting().then((res) => {
			this.adminSysFavoritSetting = res.data;
		});
	}

	mounted() {
		this.registerReloadComponent();
		this.init();
	}
}
</script>

<style scoped>
table td {
	padding: 0px 0px 0px 12px !important;

	/* 	
	margin: 1px !important;
	height: 10px !important;
	max-height: 10px !important;
	line-height: 14px !important; */
}

td .v-input__slot {
	margin-bottom: 0px im !important;
}

td .v-messages {
	display: none;
}

.padding-top-adjust .v-text-field {
	padding-top: 0px !important;
}

.v-data-table th,
.v-data-table td {
	padding: 0px 0px 0px 0px !important;
}

.v-data-table tr th:first-child,
.v-data-table tr td:first-child {
	padding: 0px 0px 0px 15px !important;
}

.v-select .v-chip {
	margin: 1px;
}

.multipleSelectGames .v-chip.v-size--default {
	/* font-size: 14px; */
	height: 26px;
}

textarea {
	background: #333 !important;
}
</style>
