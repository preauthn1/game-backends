<template>

	<div class="batchEditServer">

		<div>

			<!-- <div class="currencyItem">
				<v-checkbox :value="isAllCurrencyVisible" :label="'全選'" @click="clickCurrencyAll"
					v-model="isAllCurrencyVisible">
				</v-checkbox>
			</div> -->

			<v-radio-group row v-model="selectedCurrency">
				<v-radio v-for="(item, index) in currencySelectItems" :key="index" :label="item.text"
					@click="changeCurrency(item.value)" :value="item.value">
				</v-radio>
			</v-radio-group>

			<br>

			<!-- 

	<v-radio-group row v-model="editedItem.status">
		<v-radio :label="$t('Status.Enable')" :value="status.Enable"></v-radio>
		<v-radio :label="$t('Status.Disable')" :value="status.Disable"></v-radio>
	</v-radio-group> -->

			<!-- <div v-for="(item, index) in currencySelectItems" :key="index" class="currencyItem">
				<v-checkbox :value="item.isChecked" :key="item.text" :label="item.text"
					@click="reloadArrayValue" v-model="item.isChecked">
				</v-checkbox>
			</div> -->

			<!-- <div class="collapse-all">
				<v-btn color="yellow darken-1" text class="btn search" @click.stop="openAll">
					展開全部
				</v-btn>
				<v-btn color="yellow darken-1" text class="btn search" @click.stop="closeAll">
					縮合全部
				</v-btn>

			</div> -->

			<!-- 
			<div v-for="(item, index) in visibleMerchants" :key="index" class="currencyItem">
				<v-checkbox :value="item.uiCollapseFlag" :key="item.merchantID" :label="item.merchantName"
					@click="updateVisibleArray" v-model="item.uiCollapseFlag">
				</v-checkbox>
			</div> -->

		</div>

		<div id="myWrapper" style="overflow: scroll" v-if="isRenderComponentAgain">

			<div v-for="(eachCurrency, index) in allMerchantMapping.currencyGroups" :key="index"
				class="eachCurrency" v-show="selectedCurrency == eachCurrency.currency">

				<div>
					{{$t("TableHeader.__currency")}}:
					<span class="currencyText">
						{{eachCurrency.currency}}
					</span>

					<div style="display:inline-block" @click="showSelectedMerchant(eachCurrency.currency)"
						class="pointer ml-4 link-text">
						*篩選商戶
						<!--<v-icon color="yellow lighen-3">settings</v-icon> -->

					</div>

					<span class="f12 ml-2 grey-text">*商戶過多 請自行勾選要顯示哪些商戶</span>
				</div>

				<!-- 
				<div v-for="(eachGame, index) in eachCurrency.games" :key="index" class="eachGame"
					v-show="eachGame.servers != null">
					<div :id="'gameID_' + eachGame.gameID" class="gameTitle">
						{{ eachGame.gameID }}｜{{ eachGame.gameName }}</div> -->

				<div :id="eachCurrency.currency" style="padding: 10px 25px">
					<div class="v-data-table theme--dark">
						<div class="v-data-table__wrapper">
							<table class="scroll"
								v-show="eachCurrency.servers != null && eachCurrency.servers.length > 0">
								<thead class="v-data-table-header">
									<tr>
										<th class="gameCell">
											{{$t("TableHeader.__gameName")}}
										</th>
										<th class="serverCell">
											{{$t("TableHeader.__serverName")}}
										</th>
										<th v-for="(item, index) in eachCurrency.merchants" :key="index"
											v-show="isMerchantVisible(item.merchantID)" class="merchantCell">
											{{ item.merchantName }}
										</th>
									</tr>
								</thead>

								<tbody>

									<tr :id="'serverID_' + item.serverID"
										v-for="(item, index) in eachCurrency.servers" :key="index">

										<th class="gameCell">
											<v-chip class="justify-center">
												{{item.gameID}}｜{{item.gameName}}
											</v-chip>
										</th>

										<th class="serverCell">
											<div class="text-main">{{ item.serverName }}</div>
											<div class="text-sub">{{ item.serverID }}</div>
										</th>
										<th v-for="(eachFlag, index) in item.merchantEnableFlags" :key="index"
											v-show="isMerchantVisible(eachFlag.merchantID)" :title="eachFlag.merchantName"
											class="merchantCell">
											<v-switch :disabled="eachFlag.merchantCurrency != item.currency"
												v-model="eachFlag.isEnable" color="yellow darken-3"
												@change="revers(eachFlag.merchantID, item.serverID, eachFlag)"></v-switch>
										</th>
									</tr>

								</tbody>
							</table>

						</div>
					</div>
				</div>

			</div>
		</div>

		<v-dialog v-model="isShowSelectedMerchant" max-width="800px" persistent>
			<v-card>

				<v-card-title class="grey darken-2">

					<div style="padding: 8px;">
						<div class="">
							篩選商戶
							<v-icon small class="ml-2" color="yellow">info</v-icon>
							<span class="f12 yellow-text">僅列出有開通pgs遊戲的商戶</span>

						</div>
						<div style="clear:both"></div>

						<div class="top-right-item" @click="closeAndSearchAgain">
							<v-btn icon dark @click="refresh">
								<v-icon>close</v-icon>
							</v-btn>
						</div>
					</div>

				</v-card-title>

				<v-card-text class="mt-4">

					<div class="mt-4" style="min-height:600px;">

						<div class="mb-2">
							幣別：<v-chip small color="blue darken-4">{{currentCurrency}}</v-chip>
						</div>

						<div>

							<v-autocomplete :items="dropdownItemsOfTargetCurrency" :label="'商戶'" clearable chips
								multiple deletable-chips v-model="visibleMerchantIDs">
							</v-autocomplete>

						</div>

					</div>
				</v-card-text>

			</v-card>
		</v-dialog>

	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import ServersApi from '@/api/system-setting/servers';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import * as Model from '@/models/interfaces/system-setting/servers';
import { Component, Watch } from 'vue-property-decorator';
import { State, Action, Getter, namespace } from 'vuex-class';
import EventBus from '@/utilities/event-bus';
import { ALL_CURRENCY } from '@/utilities/enums_helper';
const playersModule = namespace('Players');

@Component
export default class EditServerMerchantsPopup extends Vue {
	allMerchantMapping: Model.IAllServerMerchantMappings = {
		availableCurrency: [],
		currencyGroups: [],
	};

	isShowSelectedMerchant: boolean = false;
	isAllCurrencyVisible: boolean = true;
	isRenderComponentAgain: boolean = true;

	visibleMerchantIDs: number[] = [];
	visibleMerchants: Model.IEditMerchantItem[] = [];
	collapseGames: number[] = [];
	currencySelectItems: any[] = [];
	currencySelectCurrencyList: string[] = [];
	currentCurrency: string = '';

	selectedCurrency: string = '';
	showTargetCurrencySelecteMerchantDialog: string[] = [];

	// 這頁有修正過幾次 所以有些廢棄變數沒移除掉

	@playersModule.State('serverID') serverID!: number;
	@Watch('serverID', { immediate: true, deep: true })
	onGameIDChange() {
		this.reloadData();
	}

	isSelectedCurrencyVisible(currency: string) {
		return this.showTargetCurrencySelecteMerchantDialog.includes(currency);
	}

	get dropdownItemsOfTargetCurrency() {
		if (this.showTargetCurrencySelecteMerchantDialog == null || this.showTargetCurrencySelecteMerchantDialog.length <= 0) {
			return [];
		}

		const targetCurrency = this.showTargetCurrencySelecteMerchantDialog[0];

		const targetGroup = this.allMerchantMapping.currencyGroups.filter((x) => x.currency === targetCurrency);

		if (targetGroup != null && targetGroup.length > 0) {
			return targetGroup[0].merchantsDropdown;
		}

		return [];
	}

	showSelectedMerchant(currency: string) {
		this.isShowSelectedMerchant = true;
		this.currentCurrency = currency;

		this.showTargetCurrencySelecteMerchantDialog = [];
		this.showTargetCurrencySelecteMerchantDialog.push(currency);
	}

	created() {}

	mounted() {
		this.reloadData();
	}

	closeAndSearchAgain() {
		this.isShowSelectedMerchant = false;
		this.reloadData();
	}

	setCurrency() {
		// this.currencySelectItems = ALL_CURRENCY.map((item, index, array) => {
		// 	return {
		// 		isChecked: true,
		// 		value: item.value,
		// 		text: this.$t(item.i18nKey),
		// 	};
		// });

		this.reloadArrayValue();
	}

	clickCurrencyAll() {
		if (this.isAllCurrencyVisible) {
			this.currencySelectCurrencyList = this.currencySelectItems.map((item) => {
				return item.value;
			});
		} else {
			this.currencySelectCurrencyList = [];
		}

		this.currencySelectItems.forEach((item) => {
			item.isChecked = this.isAllCurrencyVisible;
		});
	}

	isTargetCurrencyVisible(currency: string) {
		return this.currencySelectCurrencyList.includes(currency);
	}

	reloadArrayValue() {
		this.currencySelectCurrencyList = this.currencySelectItems
			.filter((item) => item.isChecked)
			.map((item, index, array) => {
				return item.value.toString();
			});
	}

	reloadData() {
		const vm = this;

		const queryStringCurrency = this.$route.query.currency.toString();

		EventBus.$emit('show_mask', null);

		const myFilter = {} as DropdownModel.ILazyCommonParameterReqeust;
		myFilter.merchantIDs = this.visibleMerchantIDs;

		ServersApi.getAllServerMerchantMappings(myFilter)
			.then((res) => {
				this.allMerchantMapping = res.data;
				this.collapseGames = [];

				this.currencySelectItems = res.data.availableCurrency.map((item, index, array) => {
					return {
						isChecked: true,
						value: item,
						text: this.$t(item),
					};
				});

				if (this.selectedCurrency === '') {
					if (queryStringCurrency && queryStringCurrency !== '') {
						this.selectedCurrency = queryStringCurrency;
					} else {
						this.selectedCurrency = res.data.availableCurrency[0];
					}
				}

				this.changeCurrency(this.selectedCurrency, true);

				Vue.nextTick(() => {
					this.scrollToPosition();
				});
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	scrollToPosition() {
		const targetID = 'serverID_' + this.$route.query.serverID;
		const targetTable = this.$route.query.currency.toString();

		const element = document.getElementById(targetID);
		const elementTable = document.getElementById(targetTable);

		if (element && elementTable) {
			const position = element.offsetTop;
			const positionTable = elementTable.offsetTop;
			const target = document.getElementById('myWrapper');

			if (target) {
				// offsetTop得到的位置似乎是以table內部為基準
				target.scrollTo(0, position + positionTable - 100);
			}
		}
	}

	isCollapsePanel(gameID: number) {
		return this.collapseGames.includes(gameID);
	}

	collapseGame(gameItem: Model.IEditGameItem) {
		const isExist = this.collapseGames.includes(gameItem.gameID);

		if (isExist) {
			this.collapseGames = this.collapseGames.filter((item) => item !== gameItem.gameID);
		} else {
			this.collapseGames.push(gameItem.gameID);
		}
	}

	changeCurrency(currency: string, isTriggerBySearchAgain: boolean = false) {
		this.visibleMerchantIDs = [];
		const tempMerchantIDs: number[] = [];

		this.allMerchantMapping.currencyGroups
			.filter((item) => item.currency === currency)
			.forEach((item) => {
				item.merchants.forEach((merchant) => {
					tempMerchantIDs.push(merchant.merchantID);
				});
			});

		// 如果選單的商戶超過10筆 需要使用者介面自行勾選 (前端render效能太吃緊了)
		if (!isTriggerBySearchAgain) {
			if (tempMerchantIDs.length >= 10) {
				this.visibleMerchantIDs = [];
			} else {
				tempMerchantIDs.forEach((merchant) => {
					this.visibleMerchantIDs.push(merchant);
				});
			}
		} else {
			tempMerchantIDs.forEach((merchant) => {
				this.visibleMerchantIDs.push(merchant);
			});
		}
	}

	cleanUpSelect() {
		this.visibleMerchantIDs = [];
	}

	updateVisibleArray() {
		Vue.nextTick(() => {
			this.visibleMerchantIDs = this.visibleMerchants
				.filter((item) => item.uiCollapseFlag)
				.map((item, index, array) => {
					return item.merchantID;
				});
		});
	}

	isMerchantVisible(merchantID: number) {
		if (this.visibleMerchantIDs.length <= 0) {
			return false;
		}

		return this.visibleMerchantIDs.includes(merchantID);
	}

	revers(myMerchantID: number, myServerID: number, flagItem: Model.IEditMappingFlagItem) {
		ServersApi.reverServerMerchantFlag(<Model.IReverFlag>{
			serverID: myServerID,
			merchantID: myMerchantID,
		})
			.then((res) => {
				flagItem.isEnable = res;
				this.$emit('update-server-merchants-success');
			})
			.catch((error) => {
				flagItem.isEnable = !flagItem.isEnable;
			});
	}
}
</script>



<style scoped>
#myWrapper .v-data-table__wrapper table {
	width: auto !important;
}

.v-data-table th {
	/* height: 32px; */
}

.gameTitle {
	color: #4ca3cd;
	font-weight: 700;
	font-size: 18px;
}

.eachGame {
	margin-bottom: 5px;
}

.eachGame .gameTitle {
	cursor: pointer;
}

.eachGame .gameTitle:hover {
	opacity: 0.8;
}

.text-main {
	font-size: 14px;
}

.text-sub {
	color: #bbb;
	font-weight: 100;
}

.eachCurrency {
}

.maxWidth {
	white-space: normal !important;
	max-width: 125px;
	width: 125px;
}

.currencyItem {
	margin-right: 15px;
	display: inline-block;
}

.flagControl label {
	margin-left: 15px;
}

.v-card {
	min-height: 300px;
}

.v-input__slot {
	margin-bottom: 0px !important;
}

.currency-info {
	color: #eb9292;
}
.v-card__title {
	padding: 12px 12px 0px 12px !important;
}

.v-input--selection-controls {
	margin-top: 3px !important;
}

.collapse-all {
	float: right;
	padding-right: 25px;
}

.collapse-all button {
	margin-right: 5px;
}

.batchEditServer table th.merchantCell {
	min-width: 150px !important;
	max-width: 150px !important;
	width: 150px !important;
}

.batchEditServer table th.serverCell {
	min-width: 250px !important;
	max-width: 250px !important;
	width: 250px !important;
}

.batchEditServer .v-data-table th {
	user-select: text;
}

.batchEditServer thead th {
	white-space: pre-line !important;
}

.currencyText {
	color: #ffc646;
}

.batchEditServer table.scroll {
	width: 100%; /* Optional */
	/* border-collapse: collapse; */
	border-spacing: 0;
	/* border: 2px solid black; */
}

.batchEditServer table.scroll tbody,
.batchEditServer table.scroll thead {
	display: block;
}

.batchEditServer thead tr th {
	height: 30px;
	line-height: 30px;
	/*text-align: left;*/
}

.batchEditServer table.scroll tbody {
	height: 570px;
	overflow-y: auto;
	overflow-x: hidden;
}

.batchEditServer tbody td,
.batchEditServer thead th {
	width: 20%; /* Optional */
	/* border-right: 1px solid black; */
}

.batchEditServer tbody td:last-child,
.batchEditServer thead th:last-child {
	border-right: none;
}

.gameCell {
	width: 220px;
}

/* 
.eachGame .v-messages {
	display: none;
} */
</style>