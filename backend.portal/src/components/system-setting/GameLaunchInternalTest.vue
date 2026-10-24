<template>

	<v-dialog max-width="600px" v-model="isShowGameLaunchDialog">
		<v-card id="getGameLinkDialog" min-height="750px">
			<v-card-title class="grey darken-2">
				<span>
					取得遊戲網址連結
				</span>
				<div class="top-right-item" @click="closeGameLaunchDialog">
					<v-btn icon dark @click="closeGameLaunchDialog">
						<v-icon>close</v-icon>
					</v-btn>
				</div>
			</v-card-title>

			<v-card-text>
				<v-flex>

					<v-row>

						<v-col cols="12" sm="12" md="12" class="gamesDropdown">

							<v-autocomplete v-model="gameLaunchPreview.gameID" menu-props="auto"
								:label="$t('TableHeader.__game')" :items="dropdownGames">

								<template #selection="{ item }">
									<v-chip color="red darken-3">{{item.text}}</v-chip>
								</template>

							</v-autocomplete>

						</v-col>

						<v-col cols="12" sm="12" md="12" class="minus-mt-2">

							<v-autocomplete class="" attach :label="'介面語系'" v-model="gameLaunchForm.lang" chips
								:items="dropdownLangs">

								<template #selection="{ item }">
									<v-chip color="">{{item.text}}</v-chip>
								</template>

							</v-autocomplete>

						</v-col>

						<v-col cols="12" sm="12" md="12" class="minus-mt-2">

							<div class="selectInternalAccounts">
								<v-autocomplete class="required" attach chips clearable deletable-chips
									:label="'選擇使用哪一組玩家帳號'" height="50px" v-model="gameLaunchPreview.selectedPlayerID"
									:items="dropdownInternalAccounts" @change="whenSelectPlayerIDChnage()">
								</v-autocomplete>

							</div>

							<a class="link-text" target="_blank" @click="openPlayerDetail"
								style="float: right;margin-top: 5px;">
								查看玩家資訊
							</a>

						</v-col>

					</v-row>

				</v-flex>
			</v-card-text>
			<v-card-actions class="minus-mt-4">

				<v-checkbox class="ml-2" :label="'不要使用iframe网址模式'" v-model="doNotIframe">

				</v-checkbox>

				<v-spacer></v-spacer>

				<v-btn color="orange darken-1" text @click="copyGameLink" class="btn">
					<v-icon left>file_copy</v-icon>
					{{$t('Button.__copyUrl')}}
				</v-btn>

				<v-btn color="yellow darken-1" text @click="getGameLink(false)" class="btn">
					<v-icon left>link</v-icon>
					{{$t('Button.__newTabRedirect')}}
				</v-btn>

			</v-card-actions>

			<v-card-actions class="minus-mt-2">
				<v-divider></v-divider>
			</v-card-actions>

			<v-card-actions v-if="gameLaunchPreview && internalPlayerFlag.isVisible">

				<v-row>
					<v-col cols="12" sm="6" md="6">
						<div style="text-align:right; width: 100%; margin-top: 5px;">

							<div class="list-item-width" v-if="internalPlayerFlag.isAnySimulateEnable">
								<div class="bigCategory" style="color: #ff3737">
									<div class="f18" style="display:inline-block">⚠️</div> 錯誤／延遲模擬
								</div>
							</div>

							<div class="" v-if="internalPlayerFlag.isAnySimulateEnable">

								<v-chip small color="red darken-1">
									{{internalPlayerFlag.simulateGameVendorName}}
								</v-chip>

							</div>

							<div class="list-item-width" v-if="internalPlayerFlag.isAnySimulateEnable">
								<span>模擬錯誤(單錢包商戶)</span>
								<v-icon color="green lighten-2" v-if="internalPlayerFlag.isSimulateSingleWallet">
									check
								</v-icon>
								<v-icon color="red" v-else>close</v-icon>
							</div>

							<div class="list-item-width" v-if="internalPlayerFlag.isAnySimulateEnable">
								<span>模擬錯誤(三方遊戲)</span>
								<v-icon color="green lighten-2" v-if="internalPlayerFlag.isSimulateErrorRate">
									check
								</v-icon>
								<v-icon color="red" v-else>close</v-icon>
							</div>

							<div class="list-item-width" v-if="internalPlayerFlag.isAnySimulateEnable">
								<span>模擬延遲逾時</span>
								<v-icon color="green lighten-2" v-if="internalPlayerFlag.isSimulateDelayResponse">
									check
								</v-icon>
								<v-icon color="red" v-else>close</v-icon>
							</div>

						</div>
					</v-col>

					<v-col cols="12" sm="6" md="6">
						<div style="text-align:right; width: 100%; margin-top: 5px;">

							<div class="list-item-width">
								<div class="bigCategory">
									一般配置
								</div>
							</div>

							<div class="list-item-width">
								<span>商戶啟用iframe</span>
								<v-icon color="green" v-if="internalPlayerFlag.isIframeEnable">
									check
								</v-icon>
								<v-icon color="red" v-else>close</v-icon>
							</div>

							<div class="list-item-width">
								<span>遊戲使用新分頁開啟</span>
								<v-icon color="green" v-if="internalPlayerFlag.isGameOpenNewTab">
									check
								</v-icon>
								<v-icon color="red" v-else>close</v-icon>
							</div>

							<div class="list-item-width">
								<span>是否為單錢包模式</span>
								<v-icon color="green lighten-2" v-if="internalPlayerFlag.isSingleWallet">
									check
								</v-icon>
								<v-icon color="red" v-else>close</v-icon>
							</div>

							<div class="list-item-width">
								<span>是否為代理樹狀商戶</span>
								<v-icon color="green lighten-2" v-if="internalPlayerFlag.isHierarchyMerchantMode">
									check
								</v-icon>
								<v-icon color="red" v-else>close</v-icon>
							</div>

							<div class="list-item-width">
								<span>三方遊戲強制使用試玩模式</span>
								<v-icon color="green lighten-2" v-if="internalPlayerFlag.isAlwaysTrialMode">
									check
								</v-icon>
								<v-icon color="red" v-else>close</v-icon>
							</div>

							<div class="list-item-width">
								<span>返回按鈕網址設為pgs大廳網址</span>
								<v-icon color="green" v-if="internalPlayerFlag.isLobbyBackUrl">
									check
								</v-icon>
								<v-icon color="red" v-else>close</v-icon>
							</div>

							<div class="list-item-width mt20">
								<div class="bigCategory">
									維護相關配置
								</div>
							</div>

							<div class="list-item-width">
								<span>玩家被標記標籤(可跳過維護)</span>
								<v-icon color="green lighten-2" v-if="internalPlayerFlag.isPlayerAllowSkipMaintain">
									check
								</v-icon>
								<v-icon color="red" v-else>close</v-icon>
							</div>

							<div class="list-item-width">
								<span>是否為內部測試用商戶(可跳過維護)</span>
								<v-icon color="green lighten-2" v-if="internalPlayerFlag.isInternalTestMerchant">
									check
								</v-icon>
								<v-icon color="red" v-else>close</v-icon>
							</div>

							<div class="list-item-width" style="margin-bottom: 20px;">

							</div>

						</div>
					</v-col>

					<v-col cols="12" sm="12" md="12">

						<v-expansion-panels>

							<v-expansion-panel>

								<v-expansion-panel-header>技術端額外參數(技術用)</v-expansion-panel-header>

								<v-expansion-panel-content>

									<div style="min-height:300px">

										<div class="w100p">

											<v-text-field v-model="gameLaunchPreview.backUrl" :label="'返回網址(backUrl)'">
											</v-text-field>

										</div>

									</div>

								</v-expansion-panel-content>

							</v-expansion-panel>
						</v-expansion-panels>

					</v-col>

				</v-row>

			</v-card-actions>

		</v-card>
	</v-dialog>

</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import * as Model from '@/models/interfaces/system-setting/gameList';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import GameListApi from '@/api/system-setting/gameList';
import DropdownApi from '@/api/commons/dropdown';
import * as ResponseModel from '@/models/interfaces/commons/response';
import InternalTestApi from '@/api/commons/internalTest';
import { commonMethodMixin } from '@/utilities/common-methods';
import { Locale, Currency, Status, GameType, CRUDAction, GameVendorType } from '@/models/status/common';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { datetimeMixin } from '@/utilities/datetime-format';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import { State, Action, Getter, namespace } from 'vuex-class';
const authModule = namespace('Auth');
const playersModule = namespace('Players');

import * as Permission from '@/models/status/permission';
@Component({ components: {}, mixins: [commonDisplayFiltersMixin, datetimeMixin, commonMethodMixin] })
export default class GameLaunchInternalTest extends Vue {
	gameLaunchPreview: Model.IGameLaunchPreview = {} as Model.IGameLaunchPreview;
	gameLaunchForm: ResponseModel.IInternalTestRequest = {} as ResponseModel.IInternalTestRequest;

	isShowGameLaunchDialog: boolean = false;

	dropdownGames: DropdownModel.IDropdownItems[] = [];
	dropdownInternalAccounts: DropdownModel.IDropdownItems[] = [];
	dropdownLangs: any[] = [];

	doNotIframe: boolean = false;

	internalPlayerFlag: ResponseModel.IInternalPlayerExtraInfo = {} as ResponseModel.IInternalPlayerExtraInfo;

	@Action('Players/setPlayerID') private statePlayerID!: any;

	whenSelectPlayerIDChnage() {
		this.internalPlayerFlag = {} as ResponseModel.IInternalPlayerExtraInfo;

		const match = this.dropdownInternalAccounts.filter((element) => {
			return element.valueInt === this.gameLaunchPreview.selectedPlayerID;
		});

		// 顯示該玩家的額外信息
		if (match.length > 0) {
			if (match[0].extraProperty != null) {
				this.internalPlayerFlag = match[0].extraProperty;
				this.internalPlayerFlag.isVisible = true;
			}
		}
	}

	showGameLaunchDialog(item: Model.IGameLaunchPreview) {
		this.isShowGameLaunchDialog = true;

		const lastPlayerID = localStorage.getItem('LastPlayerID');
		const lastIframeCheckbox = localStorage.getItem('LastIframeCheckbox');

		if (lastIframeCheckbox && lastIframeCheckbox === 'true') {
			this.doNotIframe = true;
		}

		this.gameLaunchPreview.selectedPlayerID = this.dropdownInternalAccounts[0].value;

		this.whenSelectPlayerIDChnage();

		const isLocalStorageValueUnDefined = lastPlayerID === undefined || lastPlayerID == null;

		if (!isLocalStorageValueUnDefined) {
			this.gameLaunchPreview.selectedPlayerID = 0;
			this.gameLaunchPreview.selectedPlayerID = Number(lastPlayerID);
		}

		this.gameLaunchPreview.gameID = item.gameID;
	}

	refreshInternalAccountDropdown() {
		DropdownApi.getInernalTestAccounts().then((res) => {
			this.dropdownInternalAccounts = res;
		});
	}

	setPlayerListToOneItem(playerID: number, playerName: string) {
		this.setPlayerList(playerID, playerName);

		setTimeout(() => {
			this.setPlayerList(playerID, playerName);
		}, 500);
	}

	setPlayerList(playerID: number, playerName: string) {
		this.dropdownInternalAccounts = [];
		const myItem = {} as DropdownModel.IDropdownItems;
		myItem.value = playerID;
		myItem.valueInt = playerID;
		myItem.text = playerID.toString() + '｜' + playerName;

		this.dropdownInternalAccounts.push(myItem);
		this.gameLaunchPreview.selectedPlayerID = playerID;
		this.whenSelectPlayerIDChnage();
	}

	closeGameLaunchDialog() {
		this.isShowGameLaunchDialog = false;
	}

	copyGameLink() {
		this.getGameLink(true);
	}

	getGameLink(isCopyLinkOnly: boolean = false) {
		const vm: any = this;

		if (this.gameLaunchPreview.selectedPlayerID <= 0) {
			alert('please select players');
			return;
		}
		EventBus.$emit('show_mask', null);

		this.gameLaunchForm.playerID = this.gameLaunchPreview.selectedPlayerID;
		this.gameLaunchForm.gameID = this.gameLaunchPreview.gameID;
		this.gameLaunchForm.doNotIframe = this.doNotIframe;
		this.gameLaunchForm.backUrl = this.gameLaunchPreview.backUrl;

		localStorage.setItem('LastPlayerID', this.gameLaunchPreview.selectedPlayerID.toString());
		localStorage.setItem('LastIframeCheckbox', this.doNotIframe ? 'true' : 'false');

		InternalTestApi.getGameLaunchLink(this.gameLaunchForm)
			.then((res) => {
				if (isCopyLinkOnly) {
					vm.copyTextToClipboard(res.data.url);
				} else {
					window.open(res.data.url, '_blank');
				}
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	openPlayerDetail() {
		this.statePlayerID(this.gameLaunchPreview.selectedPlayerID);
		this.$router.push({ path: '/members-management/detail' });
	}

	mounted() {
		DropdownApi.getInernalTestAccounts().then((res) => {
			this.dropdownInternalAccounts = res;
		});

		DropdownApi.getLanguageCodes().then((res) => {
			this.dropdownLangs = res;
		});

		DropdownApi.getGames().then((res) => {
			this.dropdownGames = res;

			const lobby: DropdownModel.IDropdownItems = {} as DropdownModel.IDropdownItems;
			lobby.text = 'Lobby大廳';
			lobby.valueInt = 0;
			lobby.value = 0;

			this.dropdownGames.splice(0, 0, lobby);
		});

		this.gameLaunchForm.lang = 'en-us';
	}
}
</script>

<style>
#getGameLinkDialog .v-text-field__details {
	display: none;
}

.v-application .v-chip {
	margin-top: 5px;
}

.v-application .v-chip.v-chip--disabled {
	background-color: #666 !important;
	opacity: 0.3 !important;
}

.gamesDropdown .v-chip {
	background: #fff;
}

.selectInternalAccounts .v-autocomplete__content.v-menu__content {
	/* min-height: 250px !important; */
}

.selectInternalAccounts .v-list-item__title {
	font-size: 14px !important;
}

.list-item-width {
}

.bigCategory {
	color: #888;
	font-weight: 700;
	font-size: 12px;
}

.mt20 {
	margin-top: 20px;
}

.list-item-width span {
	min-width: 200px;
	font-size: 14px;
	display: inline-block;
	margin-right: 5px;
	color: #ccc;
}
</style>