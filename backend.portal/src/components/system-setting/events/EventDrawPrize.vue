<template>
	<v-app v-if="renderComponent" class="app">

		<v-expansion-panels v-model="panel" multiple>
			<v-expansion-panel>

				<v-expansion-panel-content>
					<v-flex>
						<v-row class="search-form">

							<v-col cols="12" sm="3" md="3">
								<div class="item">

									<v-text-field v-model="searchFilter.displayNameLikeQuery"
										:label="$t('TableHeader.__keywordSearch')" clearable>
									</v-text-field>

								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">

									<v-autocomplete clearable v-model="searchFilter.type"
										:items="dropdownItemsEventMultiplePrizeTypes" :label="$t('TableHeader.__type')">
									</v-autocomplete>
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

		<v-data-table :headers="headers" id="table" :items="searchList"
			:page="DataTablePagingParameter.pageIndex"
			:items-per-page="DataTablePagingParameter.itemsPerPage"
			:server-items-length="DataTablePagingParameter.totalCount"
			:footer-props="DataTablePagingParameter.footerProps" @update:page="changePage"
			@update:items-per-page="changeItemPerPage">

			<template v-slot:top>
				<v-toolbar>

					<v-spacer></v-spacer>

					<v-btn icon dark @click="showAddDialog" style="margin-right:15px;">
						<v-icon>add</v-icon>
					</v-btn>

				</v-toolbar>
			</template>

			<template v-slot:item.action="{ item }">

				<v-icon small class="mr-2" style="color:#CCC;font-size:20px;"
					@click="showEdit(item.eventDrawPrizeID)">
					edit
				</v-icon>

				<v-icon small class="mr-2" style="color:#CCC;font-size:20px;"
					@click="showCloneConfirm(item)">
					file_copy
				</v-icon>

			</template>

			<template v-slot:item.amountOrQuantity="{ item }">

				<div v-if="item.isAmountRequired">
					{{ item.amountOrQuantity }}
				</div>

				<div v-if="!item.isAmountRequired">
					-
				</div>

			</template>

			<template v-slot:item.updateAmount="{ item }">

				<div class="text-r" v-if="item.updateAmount && item.updateAmount != null">
					{{item.updateAmount | amountDisplay}}
				</div>
				<div v-else class="text-r">
					-
				</div>

			</template>

			<template v-slot:item.displayName="{ item }">

				<div>
					{{item.displayName}}
				</div>

				<div class="sub-title-text">
					{{item.eventDrawPrizeID}}
				</div>

			</template>

			<template v-slot:item.typeText="{ item }">
				<v-chip :title="item.type" small>
					{{item.typeText}}
				</v-chip>
			</template>

			<template v-slot:item.eventName="{ item }">

				<div :title="item.eventID">
					{{item.eventName}}
				</div>

				<div class="sub-title-text">
					{{item.eventID}}
				</div>

			</template>

			<template v-slot:item.statusText="{ item }">

				<span :class="item.status === 9 ? 'minus-red' : ''">
					{{item.statusText}}
				</span>
			</template>

			<template v-slot:item.totalQuantity="{ item }">

				<span v-if="item.totalQuantity && item.totalQuantity != null">{{item.totalQuantity}}</span>
				<span v-else>不限</span>
			</template>

			<template v-slot:item.updateTime="{ item }">
				{{ item.updateTime | timeWithAddTimezone(userProfile.timeZone) }}
			</template>

		</v-data-table>

		<!-- 編輯項目 -->
		<v-dialog v-model="isShowEditDialog" max-width="1200px" persistent scrollable>

			<v-card v-if="editItem && editItem.pageControl">

				<v-card-title class="grey darken-2">
					{{$t("Popup.__update")}}

					<span style="font-size:12px;color:#efc66c;" class="ml-4"
						v-if="editItem.pageControl.extraDescription && editItem.pageControl.extraDescription != ''">

						<v-icon small style="color:#efc66c">info</v-icon>
						{{editItem.pageControl.extraDescription}}
					</span>

					<div class="top-right-item" @click="closeAllModal">

						<v-btn color="grey lighten-1" @click="closeAllModal" class="btn button-font-14 mr-3">

							{{ $t('Button.__cancel') }}
						</v-btn>

						<v-btn color="green lighten-1" @click="saveEditDetail" class="btn button-font-14">
							{{ $t('Button.__save') }}
						</v-btn>

					</div>
				</v-card-title>

				<v-card-text class="editForm">

					<v-flex>

						<v-row style="padding:5px 50px 5px 10px;">

							<v-col cols="12" sm="3" md="3" style="border-right:solid 1px #444;">

								<div class="row">

									<v-col cols="12" sm="11" md="11">
										<div>

											<v-text-field v-model="editItem.eventDrawPrizeID" disabled="disabled"
												:label="$t('TableHeader.__ID')">
											</v-text-field>

										</div>
									</v-col>

									<v-col cols="12" sm="11" md="11">
										<div>

											<v-text-field v-model="editItem.eventID" disabled="disabled"
												:label="$t('TableHeader.__eventID')">
											</v-text-field>

										</div>
									</v-col>
									<v-col cols="12" sm="11" md="11">
										<div>

											<v-text-field v-model="editItem.typeText" disabled="disabled"
												:label="$t('TableHeader.__type')">
											</v-text-field>

										</div>
									</v-col>

									<v-col cols="12" sm="11" md="11">
										<div>
											<v-text-field v-model="editItem.displayName"
												:label="$t('TableHeader.__displayName')">
											</v-text-field>
										</div>
									</v-col>

									<v-col cols="12" sm="11" md="11"
										v-if="editItem.pageControl.isMaxReferenceValueVisible">

										<div>
											<v-text-field v-model="editItem.maxReferenceValue"
												:label="'最大基準值(基準值*倍率=派發彩金)'">
											</v-text-field>
										</div>

									</v-col>

									<v-col cols="12" sm="11" md="11">

										<div>
											<v-text-field v-model="editItem.displayCurrency" :label="'顯示貨幣'">
											</v-text-field>
										</div>

									</v-col>

									<v-col cols="12" sm="11" md="11">

										<v-radio-group row v-model="editItem.status">
											<v-radio :label="$t('Status.Enable')" :value="1"></v-radio>
											<v-radio :label="$t('Status.Disable')" :value="0"></v-radio>
										</v-radio-group>

									</v-col>

								</div>

							</v-col>

							<v-col cols="12" sm="9" md="9">
								<div class="">

									<div class="v-data-table theme--dark">

										<div class="v-data-table__wrapper">

											<table>

												<!-- <thead class="v-data-table-header">
													<tr>
														<th>獎項</th>
														<th>獎金/倍率/數量</th>
														<th>權重</th>
														<th>限制數量</th>
														<th>已派發數量</th>

													</tr>
												</thead> -->

												<tr v-for="(item , index) in editItem.items" :key="index">
													<td>

														<div style="padding-right: 15px;">
															<v-chip>
																獎項 {{(index + 1)}}
															</v-chip>
														</div>

													</td>
													<td>

														<v-text-field v-model="item.amountOrQuantity"
															:label="editItem.pageControl.amountColumnDisplayText">
														</v-text-field>

													</td>
													<td>
														<v-text-field v-if="!editItem.pageControl.isProbabilityWeightsDisable"
															v-model="item.probabilityWeights" :label="'權重'" value="-"
															:disabled="editItem.pageControl.isProbabilityWeightsDisable">
														</v-text-field>

														<v-text-field v-if="editItem.pageControl.isProbabilityWeightsDisable"
															:label="'權重'" value="-"
															:disabled="editItem.pageControl.isProbabilityWeightsDisable">
														</v-text-field>

													</td>
													<td>

														<v-text-field v-model="item.limitQuantity" :label="'限制數量'">
														</v-text-field>

													</td>
													<td>
														<v-text-field v-model="item.usageQuantity" :label="'已派發數量'"
															disabled='disabled'>
														</v-text-field>
													</td>

													<td>

														<v-text-field
															:value="calculateEstimateRate(item.probabilityWeights , item.limitQuantity)"
															:label="'預估機率(%)'" disabled='disabled'>
														</v-text-field>
													</td>

												</tr>

											</table>

										</div>
									</div>
								</div>
							</v-col>
						</v-row>

					</v-flex>

				</v-card-text>
			</v-card>

		</v-dialog>

		<!-- 複製項目 -->
		<v-dialog v-model="isShowCloneConfirm" max-width="600px">

			<v-card v-if="editItem">

				<v-card-title class="grey darken-2">
					確認是否複製此項目
					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>

				<v-card-text>
					<v-flex style="padding:10px;">

						<v-row>

							<v-text-field class="required" v-model="cloneTarget.eventDrawPrizeID"
								disabled="disabled" :label="$t('TableHeader.__ID')">
							</v-text-field>

						</v-row>

						<v-row>

							<div class="w100 mb-4">
								<div class="common-info-label">
									{{$t('TableHeader.__displayName')}}
								</div>
								<div class="common-info-value">
									{{cloneTarget.displayName}}
								</div>
							</div>

						</v-row>

						<v-row>

							<v-text-field class="required" v-model="cloneForm.displayName"
								:label="$t('TableHeader.__newName')" autofocus required>
							</v-text-field>

						</v-row>

					</v-flex>
				</v-card-text>
				<v-card-actions>
					<v-spacer></v-spacer>
					<v-btn text @click="closeAllModal" class="btn">
						<v-icon left>mdi-cancel</v-icon>
						{{ $t('Button.__cancel') }}
					</v-btn>
					<v-btn color="yellow darken-1" text @click="saveCloneConfirm" class="btn">
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
import { datetimeMixin, startEndDate, timeWithTimezone, daysAgo, weeksAgo, monthsAgo, formatMixin } from '@/utilities/datetime-format';
import EventBus from '@/utilities/event-bus';
import * as Model from '@/models/interfaces/system-setting/merchants';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import * as EventModel from '@/models/interfaces/system-setting/events';
import MerchantsApi from '@/api/system-setting/merchants';
import EventsApi from '@/api/system-setting/events';
import DropdownApi from '@/api/commons/dropdown';
import DatePicker from 'vue2-datepicker';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { commonMethodMixin } from '@/utilities/common-methods';
import NotificationWarning from '@/components/notification/NotificationWarning.vue';
import { pageData, time } from '@/utilities/api-handler.lib';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import { State, Action, Getter, namespace } from 'vuex-class';
const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';
import { uuid } from 'vue-uuid';

// @ts-ignore
@Component({ components: { NotificationWarning, DatePicker }, mixins: [commonDisplayFiltersMixin, commonMethodMixin, datetimeMixin] })
export default class EventDrawPrize extends Vue {
	get hasPermissionCreate() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSettingMerchant_Create) >= 0;
	}
	get hasPermissionUpdate() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSettingMerchant_Update) >= 0;
	}
	get hasPermissionDelete() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSettingMerchant_Delete) >= 0;
	}

	get headers() {
		const data = [
			{ text: this.$t('TableHeader.__action'), value: 'action', width: '7%', sortable: false },
			{ text: this.$t('TableHeader.__type'), value: 'typeText', width: '10%', sortable: false },
			{ text: this.$t('TableHeader.__displayName'), value: 'displayName', sortable: false },
			{ text: '已分配' + this.$t('TableHeader.__eventName'), value: 'eventName', sortable: false },
			// { text: this.$t('TableHeader.__status'), value: 'statusText', sortable: false },
			{ text: '獎項種類數', value: 'prizeCount', sortable: false },
			{ text: '總獎項數', value: 'totalQuantity', sortable: false },
			{ text: '已派發獎項數', value: 'totalUsageQuantity', sortable: false },
			{ text: this.$t('TableHeader.__updateTime'), value: 'updateTime', sortable: false },
		];

		return data;
	}
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];

	dialog: boolean = false;
	isShowEditDialog: boolean = false;
	isShowCloneConfirm: boolean = false;
	renderComponent: boolean = true;

	DataTablePagingParameter: DropdownModel.DataTablePagingParameter = new DropdownModel.DataTablePagingParameter();
	datetimeRangePicker: DropdownModel.DatetimeRangePicker = new DropdownModel.DatetimeRangePicker();
	searchList: EventModel.IEventDrawPrizeList[] = [];
	searchFilter: EventModel.IEventDrawPrizeRequest = {} as EventModel.IEventDrawPrizeRequest;
	editItem: EventModel.IEventDrawPrizeDetailInfo = {} as EventModel.IEventDrawPrizeDetailInfo;
	cloneTarget: EventModel.IEventDrawPrizeList = {} as EventModel.IEventDrawPrizeList;
	cloneForm: EventModel.IEventDrawPrizeCloneRequest = {} as EventModel.IEventDrawPrizeCloneRequest;

	rand: number = 0;
	panel: number[] = [0];
	editModeOpenArray: number[] = [];
	merchantsData: Model.IMerchant[] = [];

	dropdownItemsEvents: DropdownModel.IDropdownItems[] = [];
	dropdownItemsEventPrizeTypes: DropdownModel.IDropdownItems[] = [];
	dropdownItemsEventMultiplePrizeTypes: DropdownModel.IDropdownItems[] = [];

	@Action('Players/setEventID') private setEventID!: any;

	get totalWeight() {
		let myTotalSum = 0;

		if (this.editItem.pageControl.isProbabilityWeightsDisable) {
			return 0;
		}

		this.editItem.items.forEach((x) => {
			myTotalSum = myTotalSum + x.probabilityWeights * x.limitQuantity;
		});

		return myTotalSum;
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

	displayColumnNameByType(type: number) {
		if (type === 21) {
			return '倍率';
		}

		return '金額/數量';
	}

	refresh() {
		this.searchFilter.pageIndex = this.DataTablePagingParameter.pageIndex;
		this.searchFilter.pageSize = this.DataTablePagingParameter.itemsPerPage;
		this.searchFilter.isASC = false;

		EventBus.$emit('show_mask', null);

		EventsApi.getEventsDrawPrize(this.searchFilter)
			.then((res) => {
				this.searchList = res.data.items;
				this.DataTablePagingParameter.totalCount = res.data.totalCount;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	calculateEstimateRate(weight: number, quantity: number) {
		const totalWeight = this.totalWeight;

		if (totalWeight <= 0) {
			return 0;
		}

		const rateValue = ((weight * quantity) / totalWeight) * 100;

		if (quantity > 0 && rateValue <= 0.01) {
			return rateValue.toFixed(4);
		}

		return rateValue.toFixed(2);
	}

	registerReloadComponent() {
		EventBus.$on(`reload-${this.$router.currentRoute.path}`, (info: any) => {
			this.renderComponent = false;
			this.$nextTick(() => {
				this.renderComponent = true;
			});
		});
	}

	showEdit(drawPrizeID: number) {
		this.isShowEditDialog = true;
		this.editItem = {} as EventModel.IEventDrawPrizeDetailInfo;

		EventBus.$emit('show_mask', null);

		EventsApi.getEventsDrawPrizeDetail(drawPrizeID)
			.then((res) => {
				this.editItem = res.data;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	showCloneConfirm(item: EventModel.IEventDrawPrizeList) {
		this.isShowCloneConfirm = true;
		this.cloneTarget = item;
		this.cloneForm.displayName = item.displayName;
	}

	saveCloneConfirm() {
		this.cloneForm.eventDrawPrizeID = this.cloneTarget.eventDrawPrizeID;

		EventBus.$emit('show_mask', null);

		EventsApi.cloneEventDrawPrize(this.cloneForm)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.closeAllModal();
				this.refresh();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	saveEditDetail() {
		EventBus.$emit('show_mask', null);

		EventsApi.updateEventDrawPrizeDetail(this.editItem)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.closeAllModal();
				this.refresh();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	showAddDialog() {
		alert('請直接使用既有的獎項作為範本複製一份資料。系統無提供 [新增] 功能');
	}

	closeAllModal() {
		this.isShowEditDialog = false;
		this.isShowCloneConfirm = false;
		this.dialog = false;
	}

	clear() {
		this.searchFilter = {} as EventModel.IEventDrawPrizeRequest;
		this.datetimeRangePicker = {} as DropdownModel.DatetimeRangePicker;
		this.datetimeRangePicker.searchStartTimeRange = [];
	}

	copySuccess() {
		EventBus.$emit(`successfully-copy`);
	}

	setDefaultFilterByQuerystringAndSearch() {}

	activated() {}
	mounted() {
		this.registerReloadComponent();

		DropdownApi.getEvents().then((res) => {
			this.dropdownItemsEvents = res;
		});

		DropdownApi.getEventPrizeTypes().then((res) => {
			this.dropdownItemsEventPrizeTypes = res;
		});

		DropdownApi.getEventMultiplePrizeTypes().then((res) => {
			this.dropdownItemsEventMultiplePrizeTypes = res;
		});

		this.refresh();
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

.v-text-field {
	padding-top: 0px I !important;
}

.editForm .v-text-field__details {
	display: none;
}
</style>