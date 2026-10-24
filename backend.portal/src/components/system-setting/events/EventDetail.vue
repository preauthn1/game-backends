<template>

	<div>

		<div class="event-theme--dark">

			<v-expansion-panels v-model="panel" multiple style="display:none;">
				<v-expansion-panel>

					<v-expansion-panel-content>
						<v-flex>
							<v-row class="search-form">

								<v-col cols="12" sm="3" md="3">
									<div class="item">

										<v-text-field v-model="searchEventID" :label="$t('TableHeader.__ID')" clearable>
										</v-text-field>

									</div>
								</v-col>

								<v-col cols="12" sm="3" md="3">
									<!-- <div class="item right"> -->
									<div class="item search-row-padding">
										<v-btn color="yellow darken-1" text class="btn search" @click.stop="search">
											<v-icon left>search</v-icon>
											{{$t('Button.__search')}}
										</v-btn>&#9;
										<v-btn color="pink lighten-3" text class="btn search" @click.stop="clear">
											<v-icon left>clear</v-icon>
											{{$t('Button.__reset')}}
										</v-btn>&#9;
									</div>
								</v-col>

							</v-row>
						</v-flex>
					</v-expansion-panel-content>
				</v-expansion-panel>
			</v-expansion-panels>

			<v-card v-if="searchResult && searchResult.eventID > 0" style="min-height:98vh;">

				<div class="text-r saveButtonRowWrapper">
					<span style="font-size: 36px;float:left;padding-left: 20px;padding-top:5px;">
						{{searchResult.displayName}}
						<span class="f14 grey-text">
							({{searchResult.eventID}})
						</span>
					</span>
					<div class="saveButtonRow">

						<div style="display:inline-block" v-if="searchResult.isOnProgressing">
							<v-icon color="yellow daraken-4">error</v-icon>
							<span class="yellow-text mr-4 f14">活動已在進行中*</span>
						</div>

						<v-btn color="grey lighten-1" large @click="closeAllModal"
							class="btn button-font-14 mr-3">

							{{ $t('Button.__cancel') }}
						</v-btn>

						<v-btn color="red lighten-1" large @click="saveEvent" class="btn button-font-14">
							{{ $t('Button.__save') }}
						</v-btn>
					</div>

					<div style="clear:both">

					</div>

				</div>

				<v-row
					style="padding:0px 10px 5px 30px;  margin-top: -10px !important;width: 100%;position: relative;">

					<v-col cols="12" sm="3" md="3">

						<div class="row">

							<!-- <v-col cols="12" sm="11" md="11">
								<div>

									<v-text-field v-model="searchResult.eventID" disabled="disabled"
										:label="$t('TableHeader.__ID')">
									</v-text-field>

								</div>
							</v-col> -->

							<v-col cols="12" sm="11" md="11">
								<div>

									<v-text-field v-model="searchResult.eventTypeText" disabled="disabled"
										:label="$t('TableHeader.__type')">
									</v-text-field>

								</div>
							</v-col>

							<v-col cols="12" sm="11" md="11">
								<div>
									<v-text-field v-model="searchResult.displayName"
										:label="$t('TableHeader.__displayName')">
									</v-text-field>
								</div>
							</v-col>

							<v-col cols="12" sm="11" md="11">
								<div>
									<v-text-field v-model="searchResult.displayNameExternal" :label="'外部显示名称'">
									</v-text-field>
								</div>
							</v-col>

							<v-col cols="12" sm="11" md="11">
								<div>

									<div class="common-info-label">
										{{$t('TableHeader.__beginTime')}}
									</div>
									<div class="common-info-value">

										<date-picker v-model="searchResult.beginTimeTS_1000" type="datetime"
											value-type="timestamp" :placeholder="$t('TableHeader.__time')">
										</date-picker>

									</div>
								</div>
							</v-col>

							<v-col cols="12" sm="11" md="11">
								<div>
									<div class="common-info-label">
										{{$t('TableHeader.__endTime')}}
									</div>
									<div class="common-info-value">

										<date-picker v-model="searchResult.endTimeTS_1000" type="datetime"
											value-type="timestamp" :placeholder="$t('TableHeader.__time')">
										</date-picker>

									</div>
								</div>
							</v-col>

							<v-col cols="12" sm="11" md="11">

								<v-radio-group row v-model="searchResult.status">
									<v-radio :label="$t('Status.Enable')" :value="1"></v-radio>
									<v-radio :label="$t('Status.Disable')" :value="0"></v-radio>
								</v-radio-group>

							</v-col>

							<v-col cols="12" sm="11" md="11" v-for="(item , index) in searchResult.extraInfo"
								:key="index">

								<div class="common-info-label">
									<v-icon small color="orange">mdi-clock</v-icon> {{ item.settingName}}
								</div>
								<div class="common-info-value" style="color:#888">
									{{ item.settingValue}}
								</div>

							</v-col>

						</div>

					</v-col>

					<v-col cols="12" sm="9" md="9" class="advanceSettingPanel">

						<div>

							<div class="common-info-label">
								{{$t("TableHeader.__supportMerchants")}} (選擇模板)

								<span class="link-text" style="float: right;" @click="showMerchantSelectDialog()">
									選擇／新增模板
								</span>

							</div>

							<div class="text-r">

							</div>

							<v-autocomplete v-model="searchResult.merchantSelectTemplateID" :disabled="alwaysTrue"
								:items="templateDropdownItems" dense chips small-chips :label="'選擇模板'" solo
								outlined>

							</v-autocomplete>
						</div>

						<div v-if="!searchResult.uiControl.isDisableEditGameIDs">

							<div class="common-info-label">
								{{$t("TableHeader.__supportGames")}}

								<span class="link-text mr-2" style="float: right;" @click="selectGamesByVendor(-1)">
									清空選擇
								</span>

								<span class="link-text mr-2" style="float: right;"
									@click="selectGamesByVendor(450000)">
									勾選yesGame
								</span>

								<span class="link-text mr-2" style="float: right;"
									@click="selectGamesByVendor(430000)">
									勾選Mario
								</span>

								<span class="link-text mr-2" style="float: right;"
									@click="selectGamesByVendor(410000)">
									勾選GPI
								</span>

								<span class="link-text mr-2" style="float: right;"
									@click="selectGamesByVendor(390000)">
									勾選Jili
								</span>

								<span class="link-text mr-2" style="float: right;"
									@click="selectGamesByVendor(330000)">
									勾選AGGLive
								</span>

								<span class="link-text mr-2" style="float: right;"
									@click="selectGamesByVendor(290000)">
									勾選AGG
								</span>

								<span class="link-text mr-2" style="float: right;"
									@click="selectGamesByVendor(250000)">
									勾選Funky
								</span>

								<span class="link-text mr-2" style="float: right;" @click="selectGamesByVendor(0)">
									勾選PGS
								</span>

							</div>

							<v-autocomplete clearable deletable-chips v-model="searchResult.supportGameIDs"
								:disabled=' this.searchResult.uiControl.isDisableEditGameIDs'
								:items="availableGames" dense chips small-chips
								:label="$t('TableHeader.__gameName')" multiple solo outlined>
							</v-autocomplete>
						</div>

						<div v-else>

							<div class="common-info-label">
								{{$t("TableHeader.__supportGames")}}
							</div>

							<div style="margin-bottom: 40px;">
								<!-- <v-icon small>error</v-icon> -->
								<v-chip>此類活動不需選擇遊戲</v-chip>
							</div>
						</div>

						<div
							v-if="searchResult.customizedSettings &&  searchResult.customizedSettings.length > 0"
							class="ruleAndPrizeCard">
							<!--alwaysRuleBundle -->
							<div class="prizeBundleTitle extraTagTitle">

								活動特殊設置

							</div>

							<!-- 
						<div class="targetAndPrizeLabelRowTitle">
							特有標記
						</div> -->

							<div v-for="(setting , index) in searchResult.customizedSettings"
								:key="'customizedSettings' + index" class="ruleItemRow"
								style="padding: 0px 35px 10px 35px; ">

								<div class="targetLabel">
									<v-chip :title="setting.settingKey">
										{{setting.settingKeyDisplayName}}
									</v-chip>
								</div>

								<div v-if="setting.isDropdownSelect" style="display:inline-block">

									<v-autocomplete :title="setting.settingKey" :label="setting.settingKeyDisplayName"
										:items="setting.dropdwonItems" v-model="setting.settingValue">
									</v-autocomplete>

								</div>

								<div v-else-if="!setting.isValueRequired" style="display:inline-block">

									<v-text-field :label="'特殊標記'" class="prizeItemInputText" disabled='disabled'>
									</v-text-field>

								</div>

								<div v-else-if="setting.isDisableEdit && setting.isValueRequired"
									style="display:inline-block">
									<v-text-field :label="'對應數值/編號'" class="prizeItemInputText" disabled='disabled'
										:value="setting.settingValue + ( setting.extraValueText && setting.extraValueText != null ? ' (' + setting.extraValueText + ')': '')">
									</v-text-field>
								</div>

								<div v-else style="display:inline-block">

									<v-text-field :label="'對應數值/編號'" class="prizeItemInputText"
										v-model="setting.settingValue" :value="setting.settingValue">
									</v-text-field>

								</div>

							</div>

						</div>

						<div
							v-if="searchResult.alwaysRuleBundle &&  searchResult.alwaysRuleBundle.ruleItems.length > 0"
							class="ruleAndPrizeCard">
							<!--alwaysRuleBundle -->
							<div class="prizeBundleTitle extraTagTitle">
								固定條件
							</div>

							<div class="targetAndPrizeLabelRowTitle">
								需符合此條件
							</div>

							<div v-for="(ruleItem , index) in searchResult.alwaysRuleBundle.ruleItems"
								:key="'alwaysRuleBundle' + index" class="ruleItemRow"
								style="padding: 0px 35px 10px 35px; ">

								<div class="targetLabel">
									<v-chip>

										{{ruleItem.ruleTypeText}}
									</v-chip>
								</div>

								<div v-if="ruleItem.isDateTimeRangeValue" style="display:inline-block">

									<date-picker v-model="ruleItem.beginTimeTS_1000" type="datetime"
										value-type="timestamp" :placeholder="$t('TableHeader.__time')">
									</date-picker>

									<date-picker v-model="ruleItem.endTimeTS_1000" type="datetime"
										style="margin-left:10px;" value-type="timestamp"
										:placeholder="$t('TableHeader.__time')">
									</date-picker>

								</div>
								<div v-else style="display:inline-block">

									<v-text-field :label="'需達成數字/金額/數量'" class="prizeItemInputText"
										v-if="ruleItem.isValueRequired" v-model="ruleItem.value">
									</v-text-field>

								</div>

								<v-text-field :label="'需符合此檢查項目/動作'" class="prizeItemInputText"
									v-if="!ruleItem.isValueRequired" disabled='disabled' value="">
								</v-text-field>

							</div>

						</div>

						<div v-for="(prizeBundle , index) in searchResult.prizeBundles"
							:key="'prizeBundle' + index" class="ruleAndPrizeCard" style="">

							<div class="prizeBundleEnableFlag" v-if="prizeBundle.isAllowEditDisabledFlag">
								<v-switch :label="'啟用此任務'" v-model="prizeBundle.isDisabled" :true-value="false"
									color="green" :false-value="true" :value="!prizeBundle.isDisabled">
								</v-switch>
							</div>

							<div class="disalbedMask" v-if="prizeBundle.isDisabled">

							</div>

							<div class="disabled-text" v-if="prizeBundle.isDisabled">

								<v-icon large color="red lighten-2">error</v-icon>
								此任務已禁用

								<span class="f14">(右上方切換開啟)</span>
							</div>

							<div class="prizeBundleTitle" :title="prizeBundle.eventPrizeBundleID">

								<v-icon color="yellow">
									star
								</v-icon>
								活動目標

							</div>

							<v-row>

								<v-col cols="12" sm="6" md="6">

									<div class="targetAndPrizeLabelRowTitle" style="">
										需完成目標
									</div>

									<div v-for="(ruleBundle , index) in prizeBundle.ruleBundles"
										:key="'ruleBundle' +index"
										:class="prizeBundle.isMultipleRuleBundle ? 'multipleGroupMatchOne' : 'multipleGroupMatchOne'">

										<div v-if="prizeBundle.isMultipleRuleBundle"
											class="successOneHintText theme--dark">

											<span>
												完成其中一項即可獲得獎勵
											</span>

											<!-- 
								<v-chip style="float: right;font-size:11px;">
									擇一完成
								</v-chip> -->

										</div>

										<div v-for="(ruleItem , childIndex) in ruleBundle.ruleItems"
											style="line-height: 62px;" :key="'ruleItem' + childIndex" class="ruleItemRow">

											<div class="targetLabel">

												<div class="ruleTyperRow">

													<v-icon color="grey darken-1" style="font-size:24px;">
														mdi-exclamation
													</v-icon>

													<span class="" :title="ruleItem.ruleType" style="margin-left: -5px">
														{{ruleItem.ruleTypeText}}
													</span>
												</div>

											</div>

											<div v-if="ruleItem.ruleType == -1" style="display: inline-block;">

											</div>

											<div v-else-if="ruleItem.ruleType == 201" class="specificGameDiv"
												style="display: inline-block;">

												<v-autocomplete v-model="ruleItem.value" :items="gamesStringDropdownItems"
													chips style="width: 275px;" :label="'指定遊戲'">
												</v-autocomplete>

											</div>

											<div v-else-if="ruleItem.isRulesOfWhen" style="display: inline-block;">
												<v-text-field :label="'當觸發此行為時檢查以下項目'" class="prizeItemInputText"
													v-if="!ruleItem.isShowRuleActionDisplayName && !ruleItem.isValueRequired"
													disabled='disabled' value="">
												</v-text-field>
											</div>

											<div v-else style="display: inline-block;">
												<v-text-field :label="'需達成數字/金額/數量'" class="prizeItemInputText"
													v-if="!ruleItem.isShowRuleActionDisplayName && ruleItem.isValueRequired"
													v-model="ruleItem.value">

												</v-text-field>

												<v-text-field :label="'需符合此檢查項目/動作'" class="prizeItemInputText"
													v-if="!ruleItem.isShowRuleActionDisplayName && !ruleItem.isValueRequired"
													disabled='disabled' value="">
												</v-text-field>

												<div v-if="ruleItem.ruleType == 61" class="yellow-text memo-estimate">
													{{calculateEstimateRate(ruleItem.value)}}
												</div>
											</div>

										</div>

										<div>

										</div>

									</div>

								</v-col>

								<v-col cols="12" sm="6" md="6">

									<div class="targetAndPrizeLabelRowTitle">
										目標完成獎勵
									</div>

									<div v-for="(prizeItem , index)  in prizeBundle.prizesItems"
										:key="'prizeItem' + index" style="padding: 0px 35px; ">

										<div class="prizeItemRow">

											<div class="targetLabel">

												<v-icon color="green lighten-2">
													check_circle
												</v-icon>

												<span class="ml-0">
													{{prizeItem.prizeTypeText}}
												</span>
												<!-- 
									<v-chip color="green" class="border-square">
										{{prizeItem.prizeTypeText}}
									</v-chip> -->
											</div>

											<div v-if="prizeItem.isDropdownSelect" style="display:inline-block">
												<v-select clearable
													:items="convertToNumberDropdwon(prizeItem.dropdwonItems)" chips
													v-model="prizeItem.amountOrQuantity" :label="'參考設置/對象'">
												</v-select>
											</div>

											<div v-if="!prizeItem.isDropdownSelect"
												style="display:inline-block;position: relative;">

												<v-text-field :label="'獎勵數值/金額/數量'" class="prizeItemInputText mr-2"
													v-if="prizeItem.isValueRequired" v-model="prizeItem.amountOrQuantity">
												</v-text-field>

												<v-text-field :label="'進行此動作'" class="prizeItemInputText " :disabled="true"
													value="" v-if="!prizeItem.isValueRequired">

												</v-text-field>

												<div v-if="prizeItem.prizeType == 222" class="yellow-text memo-estimate">
													{{calculateEstimateRate(prizeItem.amountOrQuantity , '降低至原機率的：')}}
												</div>

											</div>

										</div>

									</div>
								</v-col>
							</v-row>

						</div>
					</v-col>
				</v-row>

			</v-card>

		</div>

		<v-dialog v-model="dialogVisible.isShowPopup_1 " max-width="700px">

			<MerchantSelectTemplatePopup ref="refMerchantSelectTemplatePopup"
				@parentCloseDialog="closeAllChildModal()"
				@parentConfirmAndSaveTemplateID="confirmAndSaveTemplateID">
			</MerchantSelectTemplatePopup>

		</v-dialog>

	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import * as Model from '@/models/interfaces/system-setting/merchants';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import * as EventModel from '@/models/interfaces/system-setting/events';
import MerchantsApi from '@/api/system-setting/merchants';
import EventsApi from '@/api/system-setting/events';
import internalTestApi from '@/api/commons/internalTest';
import DropdownApi from '@/api/commons/dropdown';
import DatePicker from 'vue2-datepicker';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { commonMethodMixin } from '@/utilities/common-methods';
import NotificationWarning from '@/components/notification/NotificationWarning.vue';
import { pageData, time } from '@/utilities/api-handler.lib';
import { datetimeMixin, startEndDate } from '@/utilities/datetime-format';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import MerchantSelectTemplatePopup from '@/components/system-setting/MerchantSelectTemplatePopup.vue';

import { Locale, Currency, Status, GameType, EventType, GameVendorType } from '@/models/status/common';
import { State, Action, Getter, namespace } from 'vuex-class';
const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';
const playersModule = namespace('Players');
import { uuid } from 'vue-uuid';

// @ts-ignore
@Component({ components: { NotificationWarning, DatePicker, MerchantSelectTemplatePopup }, mixins: [commonDisplayFiltersMixin, commonMethodMixin, datetimeMixin] })
export default class EventDetail extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];

	alwaysTrue: boolean = true;
	isShowMultipleMerchantSelect: boolean = false;
	dialogVisible: DropdownModel.DialogVisible = new DropdownModel.DialogVisible();

	renderComponent: boolean = true;
	searchResult: EventModel.IEventDetail = {} as EventModel.IEventDetail;
	startTime: number | null = null;
	endTime: number | null = null;
	adminSysFavoritSetting: any = {};

	panel: number[] = [0];

	templateTypeOfEvent: number = 6609;
	testArray: number[] = [175001, 175028];
	searchEventID: number = 0;

	organizationsDropdownItems: DropdownModel.IDropdownItems[] = [];
	gamesDropdownItems: DropdownModel.IDropdownItems[] = [];
	gamesStringDropdownItems: DropdownModel.IDropdownItems[] = [];
	templateDropdownItems: DropdownModel.IDropdownItems[] = [];

	get hasPermissionCreate() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSettingMerchant_Create) >= 0;
	}
	get hasPermissionUpdate() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSettingMerchant_Update) >= 0;
	}

	get availableGames() {
		const restrictGameIDs = this.searchResult.uiControl.restrictGameIDs;

		if (restrictGameIDs === null || restrictGameIDs.length <= 0) {
			return this.gamesDropdownItems;
		}

		return this.gamesDropdownItems
			.filter((item) => restrictGameIDs.includes(Number(item.valueInt)))
			.map((item, index, array) => {
				return item;
			});
	}

	search() {
		this.refresh();
	}

	refresh() {
		this.registerReloadComponent();

		this.searchResult = {} as EventModel.IEventDetail;

		EventBus.$emit('show_mask', null);

		EventsApi.getEventDetail(this.searchEventID)
			.then((res) => {
				this.searchResult = res.data;

				if (this.searchResult.eventType === EventType.RTPEvent) {
					this.RTPEventCustomizedAdjust();
				}
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	saveEvent() {
		EventBus.$emit('show_mask', null);

		this.searchResult.beginTime = startEndDate(this.searchResult.beginTimeTS_1000);
		this.searchResult.endTime = startEndDate(this.searchResult.endTimeTS_1000);

		this.searchResult.alwaysRuleBundle.ruleItems.forEach((element: any) => {
			element.beginTime = element.beginTime == null ? null : startEndDate(element.beginTimeTS_1000);
			element.endTime = element.endTime == null ? null : startEndDate(element.endTimeTS_1000);
		});

		EventsApi.updateEvent(this.searchResult)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.closeAllModal();
				this.$emit('refreshList');
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	RTPEventCustomizedAdjust() {
		setTimeout(() => {
			DropdownApi.getGamesFilterGameVendor(GameVendorType.AGG).then((res) => {
				this.gamesDropdownItems = res;
			});
		}, 500);
	}

	registerReloadComponent() {
		EventBus.$on(`reload-${this.$router.currentRoute.path}`, (info: any) => {
			this.renderComponent = false;
			this.$nextTick(() => {
				this.renderComponent = true;
			});
		});
	}

	showMerchantSelectDialog() {
		this.dialogVisible.isShowPopup_1 = true;

		const vm: any = this;

		this.$nextTick(() => {
			vm.$refs.refMerchantSelectTemplatePopup.initialPopup(this.templateTypeOfEvent, this.searchResult.merchantSelectTemplateID);
		});
	}

	closeAllChildModal() {
		this.dialogVisible.resetAllValueToFalse();
	}

	confirmAndSaveTemplateID(templateID: number) {
		this.initiaMerchantSelectDropdown();
		this.searchResult.merchantSelectTemplateID = templateID;
		this.closeAllChildModal();
	}

	initiaMerchantSelectDropdown() {
		DropdownApi.getMerchantSelectTemplatesByCategory(this.templateTypeOfEvent).then((res) => {
			this.templateDropdownItems = res;
		});
	}

	closeAllModal() {
		this.$emit('closeParentAllModal');
	}

	clear() {}

	copySuccess() {
		EventBus.$emit(`successfully-copy`);
	}

	queryTargetEventID(eventID: number) {
		this.init();
		this.searchEventID = eventID;
		this.search();
	}

	showMultipleMerchantSelect() {
		this.isShowMultipleMerchantSelect = true;
	}

	setParentMerchantIDsByParseResult(merchantIDs: number[]) {
		this.searchResult.availableMerchantIDs = merchantIDs;
		this.closeMultipleMerchantSelect();
	}

	closeMultipleMerchantSelect() {
		this.isShowMultipleMerchantSelect = false;
	}

	convertToNumberDropdwon(items: DropdownModel.IDropdownItems[]) {
		if (items.length <= 0) {
			return items;
		}
		return items.map((element: DropdownModel.IDropdownItems) => {
			return { text: element.text, value: Number(element.value) };
		});
	}

	calculateEstimateRate(myValue: string, prefixText: string = '機率預估：') {
		if (myValue == null || myValue === '') {
			return '';
		}

		try {
			const numberValue = parseFloat(myValue);

			if (numberValue === 0) {
				return '';
			}

			if (numberValue >= 100) {
				return '*必定中獎';
			}

			// 這邊是以%作為基準顯示
			const estimateRate = (1 / (numberValue / 100)).toFixed(0);

			return prefixText + estimateRate + ' 分之1';
		} catch (e) {}

		return '';
	}

	calculateEstimateRate100X(myValue: string, prefixText: string = '機率預估：') {
		if (myValue == null || myValue === '') {
			return '';
		}

		try {
			const numberValue = parseFloat(myValue);

			if (numberValue === 0) {
				return '';
			}

			if (numberValue >= 1) {
				return '*不合法數值';
			}

			const estimateRate = (1 / numberValue).toFixed(0);

			return prefixText + estimateRate + ' 分之1';
		} catch (e) {}

		return '';
	}

	selectGamesByVendor(vendor: number) {
		const minGameID = vendor;
		const maxGameID = minGameID + 2000;

		this.searchResult.supportGameIDs = this.searchResult.supportGameIDs.filter((gameID) => !(gameID >= minGameID && gameID < maxGameID));

		if (vendor <= -1) {
			this.searchResult.supportGameIDs = [];
			return;
		}
		const ignoreGameIDs: number[] = this.adminSysFavoritSetting.invisibleGameIDs_ForSelectedAll;

		this.gamesDropdownItems.forEach((x) => {
			const targetGameID = Number(x.valueInt);
			if (targetGameID >= minGameID && targetGameID < maxGameID && targetGameID !== 430998) {
				if (!ignoreGameIDs.includes(targetGameID)) {
					this.searchResult.supportGameIDs.push(targetGameID);
				}
			}
		});
	}

	isShowIframeTag() {
		if (this.searchResult) {
			return this.searchResult.eventType === 32;
		}
		return false;
	}

	init() {
		DropdownApi.getGames().then((res) => {
			this.gamesDropdownItems = res;
		});

		DropdownApi.getGamesStringFormat().then((res) => {
			this.gamesStringDropdownItems = res;
		});

		internalTestApi.getAdminSysFavoritSetting().then((res) => {
			this.adminSysFavoritSetting = res.data;
		});

		this.initiaMerchantSelectDropdown();
	}

	mounted() {}
}
</script>


<style lang="scss" scoped>
.advanceSettingPanel {
	padding: 10px 5px 5px 25px;
	height: 87vh;
	overflow-y: scroll;
	overflow-x: hidden;
	border-bottom: solid 3px #666;
}

.advanceSettingPanel::-webkit-scrollbar {
	width: 12px; /* width of the entire scrollbar */
}

.advanceSettingPanel::-webkit-scrollbar-track {
	/* background: #ccc;  */
}

.advanceSettingPanel::-webkit-scrollbar-thumb {
	background-color: #222; /* color of the scroll thumb */
	border-radius: 20px; /* roundness of the scroll thumb */
	border: 2px solid #666; /* creates padding around scroll thumb */
}

.myCheckbox .v-input--selection-controls {
	margin-top: 0px;
	padding-top: 4px;
	height: 30px !important;
}

.multipleGroupMatchOne {
	border: dashed 3px #555;
	margin-top: 0px;
	margin-bottom: 20px;
	padding: 0px 10px 10px 10px;
	border-radius: 10px;
	min-height: 90px;
	margin-left: 15px;
	position: relative;
}

.successOneHintText {
	position: absolute;
	color: #888;
	padding: 0px 3px;
	font-size: 12px;
	left: 10px;
	top: -12px;
}

.ruleTypeName {
	/* 	
	border-left: solid 5px #484848;
	background: #2c2c2c; */

	border-left: solid 3px #898989;
	background: #5a5a5a;
	min-width: 0px;

	display: inline-block;
	line-height: 30px;
	padding: 0px 10px 0px 5px;
	font-size: 14px;
}

.ruleTyperRow {
	font-size: 15px;
}

.ruleTyperRow .v-icon {
	font-size: 14px;
}

.border-square {
	border-radius: 8px !important;
}

.extraTagTitle {
	color: #ccc;
}

span.v-chip {
	margin: 2px 4px 0px 4px !important;
}

.prizeBundleEnableFlag {
	position: absolute;
	top: -33px;
	right: 20px;
	z-index: 9999;
	padding: 2px 10px;
	background: #343434;
}

.disalbedMask {
	width: 100%;
	height: 100%;
	top: -5px;
	left: -5px;
	border: dashed 3px #6a3335;
	background: #a93d3d;
	opacity: 0.45;
	position: absolute;
	text-align: center;
	z-index: 400;
}

.disabled-text {
	z-index: 450;
	width: 100%;
	height: 100%;
	position: absolute;
	text-align: center;
	color: #f00;
	font-weight: 700;
	padding-top: 5%;
	font-size: 48px;
}

.saveButtonRowWrapper {
	background: #222;
	z-index: 9999999;
	position: relative;
	height: 60px;
	border-bottom: solid 2px #404040;
}

.saveButtonRow {
	padding: 10px 30px 0px 30px;
	overflow: hidden;
	z-index: 9999999;
	position: relative;
}

// .specificGameDiv .v-chip,
// .specificGameDiv .v-size--default,
// .v-chip.v-size--default {
// 	height: 24px !important;
// }
</style>