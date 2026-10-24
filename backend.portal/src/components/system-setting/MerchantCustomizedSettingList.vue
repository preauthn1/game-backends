<template>
	<v-app v-if="renderComponent" class="app">

		<v-expansion-panels v-model="panel" multiple>
			<v-expansion-panel>

				<v-expansion-panel-content>
					<v-flex>
						<v-row class="search-form">

							<v-col cols="12" sm="6" md="6">
								<div class="item">

									<v-autocomplete clearable :items="filterSettingKeysDropdownItems" multiple
										no-filter :search-input.sync="keywordSearchSettingKey" class="required require"
										v-model="searchfilter.merchantSettingKeys" :label="'篩選配置設定'">

										<template v-slot:selection="data">

											<v-chip>
												{{data.item.extraProperty.shortName}}
											</v-chip>

										</template>

									</v-autocomplete>

								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3" class="mt-2">
								<div class="">
									<v-autocomplete clearable :items="merchantTypesDropdownItems" class="letter-size"
										chips v-model="searchfilter.merchantType" :label="'商戶分類'">
									</v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3" class="mt-4">
								<div class="">
									<v-text-field clearable class="letter-size"
										v-model="searchfilter.settingValueMatch"
										:label="'设置值搜寻 (支援empty／not_empty／*星號)'">
									</v-text-field>
								</div>
							</v-col>

						</v-row>

						<v-row>

							<v-col cols="12" sm="3" md="3" class="minus-mt-2">
								<div class="">
									<v-text-field clearable class="letter-size" v-model="searchfilter.keyword"
										:label="'關鍵字查商戶'">
									</v-text-field>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3" class="minus-mt-4">
								<div class="">
									<v-autocomplete clearable :items="currencyDropdownItems" class="letter-size" chips
										v-model="searchfilter.currency" :label="'幣別'">
									</v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3" class="minus-mt-4">
								<div class="">
									<v-autocomplete clearable :items="hierarchyMerchantsDropdownItems"
										class="letter-size" chips v-model="searchfilter.hierarchyOfTargetMerchant"
										:label="'搜尋代理樹狀結構'">
									</v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3" class="minus-mt-6">
								<!-- <div class="item right"> -->
								<div class="item search-row-padding">
									<v-btn color="yellow darken-1" text class="btn search" @click.stop="refresh">
										<v-icon left>search</v-icon>
										{{$t('Button.__search')}}
									</v-btn>&#9;

									<v-btn color="pink lighten-3" text class="btn search" @click.stop="clear">
										<v-icon left>clear</v-icon>
										{{$t('Button.__reset')}}
									</v-btn>&#9;

									<v-btn color="light-blue lighten-3" text class="btn search"
										@click.stop="exportExcel">
										<v-icon left>cloud_download</v-icon>
										{{ $t('Button.__downloadFile') }}
									</v-btn>&#9;

									<v-btn color="green lighten-3" text class="btn search mt-1"
										@click.stop="showBatchUpdateDialog">
										<v-icon left>edit</v-icon>
										批次編輯
									</v-btn>&#9;

								</div>
							</v-col>

						</v-row>

					</v-flex>
				</v-expansion-panel-content>
			</v-expansion-panel>
		</v-expansion-panels>

		<v-data-table :headers="headers" id="table" :items="searchResult.items" :search="search"
			class="mt-4" multi-sort>

			<template v-slot:top>
				<v-toolbar>
					<v-toolbar-title>
						<h4>{{ $t('TableHeader.__merchant') }}</h4>
					</v-toolbar-title>
					<v-divider class="mx-4" inset vertical></v-divider>
					<v-spacer></v-spacer>
					<v-card-title>

						<!-- <v-text-field v-model="search" append-icon="search" class="mr-4"
							:label="`${$t('TableHeader.__merchantID')} / ${$t('TableHeader.__merchantName')}`"
							single-line hide-details clearable>
						</v-text-field> -->

						<v-tooltip left>
							<template v-slot:activator="{ on, attrs }">
								<v-btn icon dark v-bind="attrs" v-on="on" color="yellow lighten-2"
									@click="copyAllMerchantIDs">
									<v-icon>file_copy</v-icon>
								</v-btn>
							</template>
							<span>複製當前列表所有商戶ID</span>
						</v-tooltip>

					</v-card-title>

				</v-toolbar>
			</template>

			<template v-slot:item.action="{ item }">

			</template>

			<template v-slot:item.currency="{ item }">{{ $t(`Currency.${currency[item.currency]}`) }}
			</template>

			<template
				v-slot:item.chargeCurrency="{ item }">{{ $t(`Currency.${currency[item.chargeCurrency]}`) }}
			</template>

			<template v-slot:item.imChannelName="{ item }">

				<div>
					{{item.imChannelName}}
				</div>

				<div style="font-size:12px;color:#888;margin-top: -5px;">
					{{item.imSoftwareType}}
				</div>

			</template>

			<template v-slot:item.defaultBalance="{ item }">
				<div v-if="item.defaultBalance == null || item.defaultBalance == 0">
					-
				</div>
				<div v-else>
					{{ item.defaultBalance | amountDisplay}}
				</div>

			</template>

			<template v-slot:item.merchantTypeName="{ item }">

				<v-chip :class="'type_' + item.type">
					{{ item.merchantTypeName }}
				</v-chip>

			</template>

			<template v-for=" (eachExtraHeader , index) in extraHeaders"
				v-slot:[`item.${eachExtraHeader.headerPropertyName}`]="{ item }">

				<div :key="index">
					<span class="link-text-grey"
						v-if="item[eachExtraHeader.headerPropertyName] == '' || item[eachExtraHeader.headerPropertyName] == null"
						@click="showSingleSettingKeyUpdateDialog(item.merchantID , eachExtraHeader.headerPropertyName)">
						默認空值
					</span>

					<span class="link-text" v-else :title="eachExtraHeader.headerPropertyName"
						@click="showSingleSettingKeyUpdateDialog(item.merchantID , eachExtraHeader.headerPropertyName)">
						{{ item[eachExtraHeader.headerPropertyName ] }}
					</span>
				</div>

			</template>

			<template v-slot:item.createTime="{ item }">
				{{ item.createTime | timeWithAddTimezone(userProfile.timeZone) }}
			</template>

			<template v-slot:item.updateTime="{ item }">
				{{ item.updateTime | timeWithAddTimezone(userProfile.timeZone) }}
			</template>

		</v-data-table>

		<!-- 進階設置 -->
		<v-dialog v-model="isShowUpdateAdvanceSetting" max-width="700px">

			<MerchantsAdvanceSetting ref="refMerchantsAdvanceSetting"
				@closeModalAndRefreshParentPage="afterUpdateRefreshTable">
			</MerchantsAdvanceSetting>

		</v-dialog>

		<!-- 批次編輯 -->
		<v-dialog v-model="isShowBatchUpdateDialog" max-width="700px" persistent>

			<v-card>
				<v-card-title class="grey darken-2">
					批次編輯多個商戶設置
					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>
				<v-card-text>
					<div style="padding:10px;">

						<v-row style="display: block;">

							<div style="">
								<v-textarea v-model="multipleMerchantEditForm.merchantIDsRawText"
									:label="'請貼上商戶ID (支援幾種格式)'" class='textarea-bg'>

								</v-textarea>
							</div>


							<div class="text-r minus-mt-1">
									<span v-if="calculateTotalMerchants() > 0" class="orange-text mr-4"> 共: {{ calculateTotalMerchants() }} 筆商戶 </span>
							</div>




							<div style="clear:both">

							</div>
						</v-row>

						<v-row>

							<v-autocomplete clearable :items="filterAvailableSettings"
								v-model="multipleMerchantEditForm.settingKey" chips no-filter
								:search-input.sync="keywordSearchSettingKey2" class="required require"
								:label="'設置鍵值'">

								<template v-slot:selection="data">

									<v-chip>
										{{data.item.extraProperty.shortName}}
									</v-chip>

								</template>

							</v-autocomplete>

						</v-row>

						<v-row>

							<v-text-field :label="'設置值'" v-model="multipleMerchantEditForm.settingValue">
							</v-text-field>

							<div></div>
						</v-row>

					</div>

					<div
						v-if="multipleMerchantEditForm.settingKey == 9051 && !isEmptyString(multipleMerchantEditForm.settingValue)"
						class="w100p">

						<div class="yellow-text mb-2">
							<v-icon small color="yellow lighten-3">info</v-icon> 儲存前，請測試以下網址正常可用
						</div>

						<div>
							<a :href="multipleMerchantEditForm.settingValue + '/lobby/?page=speedTest'"
								target="_blank">測速頁 (speedTest)
							</a>
						</div>

						<div>
							<a :href="multipleMerchantEditForm.settingValue + '/game-icon/agg_ht-apollo.png'"
								target="_blank">遊戲圖片 (gameIcon)
							</a>
						</div>
					</div>

					</div>
				</v-card-text>
				<v-card-actions>
					<v-spacer></v-spacer>
					<v-btn text @click="closeAllModal" class="btn">
						<v-icon left>mdi-cancel</v-icon>
						{{ $t('Button.__cancel') }}
					</v-btn>
					<v-btn color="yellow darken-1" text @click="saveBatchEditMultipleMerchant" class="btn">
						<v-icon left>mdi-check-circle</v-icon>
						{{ $t('Button.__submit') }}
					</v-btn>
				</v-card-actions>
			</v-card>

		</v-dialog>

	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import * as Model from '@/models/interfaces/system-setting/merchants';
import * as SystemSettingModel from '@/models/interfaces/system-setting/systemConfigSetting';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import MerchantsApi from '@/api/system-setting/merchants';
import DropdownApi from '@/api/commons/dropdown';
import { commonMethodMixin } from '@/utilities/common-methods';
import TextareaParseToListIDPopup from '@/components/system-setting/TextareaParseToListIDPopup.vue';

import { Locale, Currency, Status } from '@/models/status/common';
import { WalletMode } from '@/models/status/merchant';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { ALL_CURRENCY } from '@/utilities/enums_helper';

import NotificationWarning from '@/components/notification/NotificationWarning.vue';
import MerchantCompare from '@/components/system-setting/MerchantCompare.vue';
import MerchantsAdvanceSetting from '@/components/system-setting/MerchantsAdvanceSetting.vue';

import { pageData, time } from '@/utilities/api-handler.lib';
import { datetimeMixin } from '@/utilities/datetime-format';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import { State, Action, Getter, namespace } from 'vuex-class';
const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';
import { uuid } from 'vue-uuid';

@Component({ components: { NotificationWarning, MerchantCompare, MerchantsAdvanceSetting, TextareaParseToListIDPopup }, mixins: [commonDisplayFiltersMixin, datetimeMixin, commonMethodMixin] })
export default class Merchants extends Vue {
	locale: typeof Locale = Locale;
	currency: typeof Currency = Currency;
	status: typeof Status = Status;
	WalletMode: typeof WalletMode = WalletMode;

	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];

	renderComponent: boolean = true;

	dialog: boolean = false;

	isShowCopyHint: boolean = false;
	isShowUpdateAdvanceSetting: boolean = false;
	isShowBatchUpdateDialog: boolean = false;

	keywordSearchSettingKey: string = '';
	keywordSearchSettingKey2: string = '';

	search: string = '';
	searchfilter: Model.IMerchantAdvanceCustomizedSettingListRequest = {} as Model.IMerchantAdvanceCustomizedSettingListRequest;
	searchResult: Model.IMerchantAdvanceCustomizedSettingListResult = {} as Model.IMerchantAdvanceCustomizedSettingListResult;
	extraHeaders: Model.IExtraHeader[] = [];
	multipleMerchantEditForm: Model.IBatchUpdateMerchantSettingRequest = {} as Model.IBatchUpdateMerchantSettingRequest;

	panel: number[] = [0];

	hierarchyMerchantsDropdownItems: DropdownModel.IDropdownItems[] = [];
	merchantsDropdownItems: DropdownModel.IDropdownItems[] = [];
	merchantTypesDropdownItems: DropdownModel.IDropdownItems[] = [];
	merchantSettingKeysDropdownItems: DropdownModel.IDropdownItems[] = [];
	currencyDropdownItems: DropdownModel.IDropdownItems[] = [];

	get hasPermissionCreate() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSettingMerchant_Create) >= 0;
	}
	get hasPermissionUpdate() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSettingMerchant_Update) >= 0;
	}
	get hasPermissionDelete() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSettingMerchant_Delete) >= 0;
	}
	get hasPermissionRD() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.Special_AllowToUseDevelopeFeature) >= 0;
	}

	get headers() {
		const data = [
			{ text: this.$t('TableHeader.__merchantID'), value: 'merchantID', width: '6%' },
			{ text: this.$t('TableHeader.__merchantName'), value: 'merchantName', width: '15%' },
			{ text: this.$t('TableHeader.__category'), value: 'merchantTypeName', width: '8%' },
			{ text: this.$t('TableHeader.__currency'), value: 'merchantCurrency', width: '8%' },
		];

		if (this.extraHeaders.length > 0) {
			this.extraHeaders.forEach((item) => {
				data.push({
					text: item.headerDisplayName,
					value: item.headerPropertyName,
					width: '10%',
				});
			});
		}

		data.push({
			text: '',
			value: 'xxxx',
			width: '',
		});

		return data;
	}

	get filterAvailableSettings() {
		const vm: any = this;
		if (this.hasPermissionRD) {
			if (vm.isEmptyString(this.keywordSearchSettingKey2)) {
				return this.merchantSettingKeysDropdownItems;
			}
			const myKeyword: string = this.keywordSearchSettingKey2.toLowerCase();
			const filterItems = this.merchantSettingKeysDropdownItems.filter((item) => {
				return item.fullTextForKeywordSearch.includes(myKeyword);
			});

			return filterItems;
		}

		// 非技術人員只能編輯特定選項
		const availableIDs: number[] = [];
		availableIDs.push(9125);

		const filter = this.merchantSettingKeysDropdownItems.filter((res) => res.valueInt != null && availableIDs.includes(res.valueInt));
		return filter;
	}

	get filterSettingKeysDropdownItems() {
		if (this.keywordSearchSettingKey == null || this.keywordSearchSettingKey.length <= 0) {
			return this.merchantSettingKeysDropdownItems;
		}

		const myKeyword: string = this.keywordSearchSettingKey.toLowerCase();
		const filterItems = this.merchantSettingKeysDropdownItems.filter((item) => {
			return item.fullTextForKeywordSearch.includes(myKeyword);
		});

		return filterItems;
	}

	refresh() {
		this.registerReloadComponent();
		this.search = '';

		if (this.searchfilter.merchantIDsText != null && this.searchfilter.merchantIDsText.length > 0) {
			this.searchfilter.merchantIDsText = this.searchfilter.merchantIDsText.trim();
		}

		const vm: any = this;

		if (!vm.searchfilter.merchantSettingKeys || vm.searchfilter.merchantSettingKeys.length <= 0) {
			alert('本功能查詢有必填輸入項目');
		}

		EventBus.$emit('show_mask', null);
		MerchantsApi.getMerchantCustomizedSettingList(this.searchfilter)
			.then((res) => {
				this.searchResult = res.data;
				this.extraHeaders = res.data.extraHeaders;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	showSingleSettingKeyUpdateDialog(merchantID: number, settingKey: number) {
		this.isShowUpdateAdvanceSetting = true;

		this.$nextTick(() => {
			const ref: any = this.$refs.refMerchantsAdvanceSetting;
			ref.getMerchantAdvanceSetting(merchantID, settingKey);
		});
	}

	afterUpdateRefreshTable() {
		this.closeAllModal();
		this.refresh();
	}

	fixUndefinedValue() {
		if (!this.searchfilter.merchantSettingKeys || this.searchfilter.merchantSettingKeys.length <= 0) {
			this.searchfilter.merchantSettingKeys = [];
		}
	}

	showBatchUpdateDialog() {
		this.isShowBatchUpdateDialog = true;
	}

	saveBatchEditMultipleMerchant() {
		EventBus.$emit('show_mask', null);
		MerchantsApi.batchUpdateCustomizedSetting(this.multipleMerchantEditForm)
			.then((res) => {
				if (res.code === 0) {
					alert(res.data.message);
				}
				this.closeAllModal();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	geti18nTextAsString(key: string) {
		return this.$t(key).toString();
	}

	copyAllMerchantIDs() {
		let myText = '';

		this.searchResult.items.forEach((item) => {
			myText = myText + item.merchantID.toString() + '\n';
		});

		const vm: any = this;
		vm.copyTextToClipboard(myText);
	}


	calculateTotalMerchants() {
		try {
			const lines = this.multipleMerchantEditForm.merchantIDsRawText
				.split(/\r?\n/) // 根據換行符號分段
				.filter((line) => line.trim() !== '').length;

			return lines;
		} catch (error) {
			return 0;
		}
	}

	async exportExcel() {
		const excelHeaders: any[] = [this.geti18nTextAsString('TableHeader.__merchantID'), this.geti18nTextAsString('TableHeader.__merchantName'), this.geti18nTextAsString('TableHeader.__category')];

		const excelData: any = [];

		this.fixUndefinedValue();

		const result = await MerchantsApi.getMerchantCustomizedSettingList(this.searchfilter);

		result.data.extraHeaders.forEach((value) => {
			excelHeaders.push(value.headerDisplayName);
		});

		result.data.items.forEach((value) => {
			const eachRow: any = [];

			eachRow.push(value.merchantID);
			eachRow.push(value.merchantName);
			eachRow.push(value.merchantTypeName);

			result.data.extraHeaders.forEach((headerItem) => {
				eachRow.push(value[headerItem.headerPropertyName]);
			});

			excelData.push(eachRow);
		});

		const config = {
			headers: excelHeaders,
			rowData: excelData,
			fileName: 'Export_MerchantAdvanceSettingList.xlsx',
		};

		// @ts-ignore
		this.downloadExcel(config);
	}

	registerReloadComponent() {
		EventBus.$on(`reload-${this.$router.currentRoute.path}`, (info: any) => {
			this.renderComponent = false;
			this.$nextTick(() => {
				this.search = '';
				this.renderComponent = true;
			});
		});
	}

	closeAllModal() {
		this.isShowCopyHint = false;
		this.isShowUpdateAdvanceSetting = false;
		this.isShowBatchUpdateDialog = false;
	}

	clear() {
		this.searchfilter = {} as Model.IMerchantAdvanceCustomizedSettingListRequest;
	}

	mounted() {
		DropdownApi.getMerchantTypes().then((res) => {
			this.merchantTypesDropdownItems = res;
			this.searchfilter.merchantType = 20;
		});

		DropdownApi.getMerchantSettingKeys().then((res) => {
			this.merchantSettingKeysDropdownItems = res;
		});

		DropdownApi.getCurrency().then((res) => {
			this.currencyDropdownItems = res;
		});

		DropdownApi.getHierarchyMerchants().then((res) => {
			this.hierarchyMerchantsDropdownItems = res;
		});
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

.hint-warning-color {
	color: #ef4c4c;
}

.hint-warning {
	color: #ef4c4c;
	font-size: 14px;
	display: inline-block;
}

.hint-warning i {
	color: #ef4c4c !important;
	font-size: 16px !important;
}

.type_0 {
	color: #b5b5b5;
}

.type_20 {
	color: #72cbff;
}

.type_30 {
	color: #ffa67b;
}

.type_40 {
	color: #49d49a;
}

.color-label label {
	color: #fd6262 !important;
	font-size: 12px;
}

.reviewMerchant .col-md-6,
.reviewMerchant .col-md-12 {
	padding: 4px 12px !important;
}
</style>