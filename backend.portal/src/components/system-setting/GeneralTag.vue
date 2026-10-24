<template>
	<v-app v-if="renderComponent" class="app">

		<v-expansion-panels v-model="panel" multiple>
			<v-expansion-panel>

				<v-expansion-panel-content>
					<v-flex>
						<v-row class="search-form">

							<v-col cols="12" sm="3" md="3">
								<div class="item">

									<v-text-field :label="$t('TableHeader.__keywordSearch')"
										v-model="searchFilter.keyword">

									</v-text-field>

								</div>
							</v-col>
							<!-- <v-col cols="12" sm="3" md="3">
								<div class="item">

								</div>
							</v-col>
							<v-col cols="12" sm="3" md="3">
								<div class="item">

								</div>
							</v-col> -->

							<v-col cols="12" sm="3" md="3">
								<!-- <div class="item right"> -->
								<div class="item search-row-padding">
									<v-btn color="yellow darken-1" text class="btn search"
										@click.stop="pageOneSearch()">
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

		<v-data-table :headers="headers" id="table" :items="searchResult"
			:page="DataTablePagingParameter.pageIndex"
			:items-per-page="DataTablePagingParameter.itemsPerPage"
			:server-items-length="DataTablePagingParameter.totalCount"
			:footer-props="DataTablePagingParameter.footerProps" @update:page="changePage"
			@update:items-per-page="changeItemPerPage">
			<template v-slot:top>
				<v-toolbar>

					<v-divider class="mx-4" inset vertical></v-divider>
					<v-spacer></v-spacer>

					<v-card-title>

						<v-btn color="blue lighten-3" text class="btn search mr-4" @click="showAppendTags()"
							small>
							<v-icon left>add</v-icon>
							對目標標記多個標籤
						</v-btn>&#9;

						<v-btn icon dark @click="showCreateDialog">
							<v-icon>add</v-icon>
						</v-btn>

					</v-card-title>
				</v-toolbar>
			</template>

			<!-- <template v-slot:header.gameID="{ header }">
				QQQQQQQQQQQQQ
			</template> -->

			<template v-slot:item.action="{ item }">

				<div class="operate_icons">

					<v-tooltip top>
						<template v-slot:activator="{ on, attrs }">
							<v-icon small class="mr-1" v-bind="attrs" v-on="on"
								@click="showEditDialog(item.tagID)">
								edit
							</v-icon>
						</template>
						<span>編輯基本資訊</span>
					</v-tooltip>

				</div>

			</template>

			<template v-slot:item.typeText="{ item }">

				<v-chip :color="item.type === 10000 ? 'green darken-3': ''" small>
					{{ item.typeText }}
				</v-chip>

			</template>

			<template v-slot:item.assignTargetCount="{ item }">

				<div class="link-text-grey" @click="showUpdateAssignerDialog(item.tagID)">
					{{ item.assignTargetCount }}
				</div>

			</template>

			<template v-slot:item.createTime="{ item }">

				<span class="pointer" @click="globalPopupCopyDateTime(item.createTime)">
					{{ item.createTime | timeWithAddTimezone(userProfile.timeZone) }}
				</span>

			</template>

		</v-data-table>

		<!-- 新增創建Tag -->
		<v-dialog v-model="dialogVisible.isShowCreate" max-width="500px" persistent>

			<v-card>

				<v-card-title class="grey darken-2">

					新增
					<div class="top-right-item">
						<div class="close mr-2" @click="closeAllModal">
							<v-icon>close</v-icon>
						</div>
					</div>

				</v-card-title>

				<v-card-title style="padding-right: 50px;">

					<v-row>

						<v-col cols="12" md="12" sm="12">
							<v-autocomplete v-model="createForm.type" :label="$t('TableHeader.__type')" chips
								:items="dropdownItemsTagTypes">
							</v-autocomplete>
						</v-col>

						<v-col cols="12" md="12" sm="12" class="minus-mt-4">

							<v-text-field v-model="createForm.tagID" :label="$t('TableHeader.__ID') ">
							</v-text-field>

							<div class="minus-mt-2  minus-red f12">
								<div v-if="createForm.type > 0">
									<v-icon small color="red darken-2">info</v-icon> 此類標籤:
									編號需介於{{(createForm.type)}}~{{(createForm.type + 9999)}}
								</div>
							</div>

						</v-col>

						<v-col cols="12" md="12" sm="12" class="minus-mt-4">

							<v-text-field v-model="createForm.name" :label="$t('TableHeader.__displayName')">
							</v-text-field>
						</v-col>

						<v-col cols="12" md="12" sm="12" class="minus-mt-4">

							<v-text-field v-model="createForm.memo" :label="$t('TableHeader.__memo')">
							</v-text-field>
						</v-col>

					</v-row>

				</v-card-title>

				<v-card-actions>
					<v-spacer></v-spacer>

					<v-btn color="green lighten-1" text @click="saveCreate()" class="btn">
						<v-icon left>check</v-icon>
						{{ $t('Button.__save') }}
					</v-btn>
				</v-card-actions>

			</v-card>

		</v-dialog>

		<!-- 編輯assinger視窗 -->
		<v-dialog v-model="dialogVisible.isShowEdit" max-width="800px" persistent scrollable>

			<v-card>

				<v-card-title class="grey darken-2">

					編輯分配
					<div class="top-right-item">
						<div class="close mr-2" @click="closeAllModal">
							<v-icon>close</v-icon>
						</div>
					</div>

				</v-card-title>

				<v-card-text style="min-height: 250px;">

					<v-row>

						<v-col cols="12" md="12" sm="12">

							<v-text-field disabled="disabled" v-model="updateAssignerForm.tagID"
								:label="$t('TableHeader.__ID')">
							</v-text-field>

						</v-col>

						<v-col cols="12" md="12" sm="12" class="minus-mt-4">

							<v-text-field disabled="disabled" v-model="updateAssignerForm.name"
								:label="$t('TableHeader.__displayName')">
							</v-text-field>

						</v-col>

						<v-col cols="12" md="12" sm="12" class="minus-mt-4">

							<v-autocomplete :items="dropdownItemsGames" v-if="updateAssignerForm.type === 10000"
								clearable deletable-chips small-chips multiple
								v-model="updateAssignerForm.assignTargetIDs" :label="$t('TableHeader.__game')">

							</v-autocomplete>

							<v-autocomplete :items="dropdownItemsMerchants" :label="$t('TableHeader.__merchant')"
								clearable deletable-chips small-chips multiple
								v-model="updateAssignerForm.assignTargetIDs"
								v-if="updateAssignerForm.type === 20000">
							</v-autocomplete>

							<v-autocomplete :items="dropdownItemsIMChannels"
								:label="$t('TableHeader.__imChannelName')" clearable deletable-chips small-chips
								multiple v-model="updateAssignerForm.assignTargetIDs"
								v-if="updateAssignerForm.type === 50000">
							</v-autocomplete>

						</v-col>

						<div style="margin-top: -20px;" class="text-r w100p f12">

							<div class="link-text-grey mr-3"
								style="display: inline-block;position: relative;z-index: 99;"
								@click="copyAllSelectedValue(true)">
								複製勾選項目文字(編號+名稱)</div>

							<div class="link-text-grey"
								style="display: inline-block;position: relative;z-index: 99;"
								@click="copyAllSelectedValue(false)">
								複製勾選項目文字(編號)</div>

							<div class="link-text ml-3"
								style="display: inline-block;position: relative;z-index: 99;"
								@click="showParseTextToListIntDialog()">
								貼上多筆IDs (編輯用)
							</div>

						</div>

					</v-row>

				</v-card-text>

				<v-card-actions style="padding-bottom: 30px;">

					<v-spacer></v-spacer>
					<v-btn color="green lighten-1" text @click="updateAssigner()" class="btn ml-4">
						<v-icon left>check</v-icon>
						{{ $t('Button.__save') }} &nbsp;
					</v-btn>
				</v-card-actions>

			</v-card>

		</v-dialog>

		<!-- 針對特定目標 標記多組tag -->
		<v-dialog v-model="dialogVisible.isShowBatchEdit" max-width="600px" scrollable>

			<v-card>

				<v-card-title class="grey darken-2">

					對目標標記多個標籤
					<div class="top-right-item">
						<div class="close mr-2" @click="closeAllModal">
							<v-icon>close</v-icon>
						</div>
					</div>

				</v-card-title>

				<v-card-text style="height: 300px;">

					<v-row>

						<v-col cols="12" md="12" sm="12">
							<v-autocomplete v-model="appendTagFrom.type" :label="$t('TableHeader.__type')" chips
								@change="whenTagTypeChangedOfAppend()" :items="dropdownItemsTagTypes">
							</v-autocomplete>
						</v-col>

						<v-col cols="12" md="12" sm="12" class="minus-mt-2">

							<v-autocomplete :items="dropdownItemsGames" v-if="appendTagFrom.type === 10000" chips
								clearable v-model="appendTagFrom.assignTargetID" :label="$t('TableHeader.__game')">

							</v-autocomplete>

							<v-autocomplete :items="dropdownItemsMerchants" :label="$t('TableHeader.__merchant')"
								chips clearable v-model="appendTagFrom.assignTargetID"
								v-if="appendTagFrom.type === 20000">
							</v-autocomplete>

							<v-autocomplete :items="dropdownItemsIMChannels"
								:label="$t('TableHeader.__imChannelName')" clearable chips
								v-model="appendTagFrom.assignTargetID" v-if="appendTagFrom.type === 50000">
							</v-autocomplete>

						</v-col>

						<v-col cols="12" md="12" sm="12" class="minus-mt-2" v-if="appendTagFrom.type > 0">
							<v-autocomplete v-model="appendTagFrom.tagIDs" :label="$t('TableHeader.__tag')"
								clearable deletable-chips chips multiple :items="dropdownItemsTags">
							</v-autocomplete>
						</v-col>

					</v-row>

				</v-card-text>

				<v-card-actions>
					<v-spacer></v-spacer>

					<v-btn color="green lighten-1" text @click="saveAppendTagsToAssign()" class="btn">
						<v-icon left>check</v-icon>
						{{ $t('Button.__save') }}
					</v-btn>
				</v-card-actions>

			</v-card>

		</v-dialog>

		<TextareaParseToListIDPopup ref="refTextareaParseToListIDPopup"
			@parentSetResultIDs="setResultMerchantIDs">

		</TextareaParseToListIDPopup>

	</v-app>

</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import * as Model from '@/models/interfaces/system-setting/accountingInvoice';
import * as CurrencyModel from '@/models/interfaces/system-setting/currency';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import * as TagModel from '@/models/interfaces/game-management/generalTag';
import * as EventModel from '@/models/interfaces/system-setting/events';
import AccountingInvoiceApi from '@/api/system-setting/accountingInvoice';
import CurrencyApi from '@/api/system-setting/currency';
import DropdownApi from '@/api/commons/dropdown';
import TagApi from '@/api/commons/generalTag';
import DatePicker from 'vue2-datepicker';
import { Locale, Currency, Status, GameVendorType } from '@/models/status/common';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import TextareaParseToListIDPopup from '@/components/system-setting/TextareaParseToListIDPopup.vue';
import { commonMethodMixin } from '@/utilities/common-methods';
import { pageData, time } from '@/utilities/api-handler.lib';
import { datetimeMixin, startEndDate, timeWithTimezone, daysAgo, weeksAgo, monthsAgo, formatMixin } from '@/utilities/datetime-format';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import { State, Action, Getter, namespace } from 'vuex-class';
const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';
import { uuid } from 'vue-uuid';

// @ts-ignore
@Component({ components: { DatePicker, TextareaParseToListIDPopup }, mixins: [commonDisplayFiltersMixin, commonMethodMixin, datetimeMixin] })
export default class AccountingInvoice extends Vue {
	get hasExportInvoicePermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.System_AccountingInvoice_ExportInvoice) >= 0;
	}

	get headers() {
		const data = [
			{ text: this.$t('TableHeader.__action'), value: 'action', sortable: false, align: 'center' },
			{ text: this.$t('TableHeader.__ID'), value: 'tagID', sortable: false, align: 'start' },
			{ text: this.$t('TableHeader.__type'), value: 'typeText', sortable: false, align: 'start' },
			{ text: this.$t('TableHeader.__displayName'), value: 'name', sortable: false, align: 'start' },
			{ text: this.$t('TableHeader.__memo'), value: 'memo', sortable: false, align: 'start' },
			{ text: '已分配筆數', value: 'assignTargetCount', sortable: false, align: 'start' },
			{ text: this.$t('TableHeader.__createTime'), value: 'createTime', sortable: false, align: 'start' },
			{ text: '', value: 'xxxxx', sortable: false, align: 'start', width: '25%' },
		];

		return data;
	}

	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];

	dialogVisible: DropdownModel.DialogVisible = new DropdownModel.DialogVisible();

	renderComponent: boolean = true;

	searchResult: TagModel.IGeneralTagResponse[] = [];
	searchFilter: TagModel.IGeneralTagRequest = {} as TagModel.IGeneralTagRequest;
	createForm: TagModel.IGeneralTagCreateRequest = {} as TagModel.IGeneralTagCreateRequest;
	appendTagFrom: any = {};
	updateAssignerForm: TagModel.IGeneralTagAssignerUpdateRequest = {} as TagModel.IGeneralTagAssignerUpdateRequest;

	DataTablePagingParameter: DropdownModel.DataTablePagingParameter = new DropdownModel.DataTablePagingParameter();
	datetimeRangePicker: DropdownModel.DatetimeRangePicker = new DropdownModel.DatetimeRangePicker();

	panel: number[] = [0];

	dropdownItemsStatus: DropdownModel.IDropdownItems[] = [];
	dropdownItemsGames: DropdownModel.IDropdownItems[] = [];
	dropdownItemsMerchants: DropdownModel.IDropdownItems[] = [];
	dropdownItemsTagTypes: DropdownModel.IDropdownItems[] = [];
	dropdownItemsTags: DropdownModel.IDropdownItems[] = [];
	dropdownItemsIMChannels: DropdownModel.IDropdownItems[] = [];

	@Action('Players/setEventID') private setEventID!: any;

	get TgChannelsDropdownItems() {
		// 編號比較後面的允許撈skype channels
		if (this.updateAssignerForm.tagID >= 50200 && this.updateAssignerForm.tagID <= 50299) {
			return this.dropdownItemsIMChannels;
		}

		if (this.updateAssignerForm.tagID >= 57000 && this.updateAssignerForm.tagID <= 59999) {
			return this.dropdownItemsIMChannels;
		}
		const items = this.dropdownItemsIMChannels.filter((x: any) => x.valueInt <= 19999);
		return items;
	}

	whenTagTypeChangedOfAppend() {
		EventBus.$emit('show_mask', null);
		DropdownApi.getGeneralTags(this.appendTagFrom.type)
			.then((res) => {
				this.dropdownItemsTags = res;
			})
			.finally(() => {
				setTimeout(() => {
					EventBus.$emit('hide_mask', null);
				}, 500);
			});
	}

	changePage(index: number = 1) {
		this.DataTablePagingParameter.pageIndex = index;
		this.refresh();
	}

	changeItemPerPage(val: number = 10) {
		this.DataTablePagingParameter.itemsPerPage = val;
		this.refresh();
	}

	pageOneSearch() {
		this.DataTablePagingParameter.pageIndex = 1;
		this.refresh();
	}

	refresh() {
		const request = this.searchFilter;
		request.pageIndex = this.DataTablePagingParameter.pageIndex;
		request.pageSize = this.DataTablePagingParameter.itemsPerPage;

		TagApi.getPagingList(this.searchFilter).then((res) => {
			this.searchResult = res.data.items;
			this.DataTablePagingParameter.totalCount = res.data.totalCount;
		});
	}

	showParseTextToListIntDialog() {
		const targetRef: any = this.$refs.refTextareaParseToListIDPopup;
		targetRef.showParseTextareaPraseToIDs();
	}

	setResultMerchantIDs(IDs: number[]) {
		this.updateAssignerForm.assignTargetIDs = IDs;

		this.dialogVisible.isShowEdit = false;

		this.$nextTick(() => {
			this.dialogVisible.isShowEdit = true;
		});
	}

	showAppendTags() {
		this.dialogVisible.isShowBatchEdit = true;
		this.appendTagFrom = {};
	}

	saveAppendTagsToAssign() {
		EventBus.$emit('show_mask', null);

		TagApi.appendTagToAssigner(this.appendTagFrom)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.closeAllModal();
				this.refresh();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	showCreateDialog() {
		this.dialogVisible.isShowCreate = true;
	}

	saveCreate() {
		EventBus.$emit('show_mask', null);
		TagApi.createTag(this.createForm)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.closeAllModal();
				this.pageOneSearch();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	showUpdateAssignerDialog(tagID: number) {
		EventBus.$emit('show_mask', null);

		this.updateAssignerForm.assignTargetIDs = [];

		this.reloadIMChannel();

		TagApi.getByID(tagID)
			.then((res) => {
				this.updateAssignerForm.tagID = res.data.tagID;
				this.updateAssignerForm.name = res.data.name;
				this.updateAssignerForm.type = res.data.type;
				this.updateAssignerForm.assignTargetIDs = res.data.assignTargetIDs;
				this.dialogVisible.isShowEdit = true;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	updateAssigner() {
		EventBus.$emit('show_mask', null);
		TagApi.updateAssigner(this.updateAssignerForm)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.closeAllModal();
				this.pageOneSearch();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	copyAllSelectedValue(isCopyNameColumn: boolean = false) {
		if (this.updateAssignerForm.assignTargetIDs == null || this.updateAssignerForm.assignTargetIDs.length <= 0) {
			return;
		}

		let copyText = '';

		if (this.updateAssignerForm.type === 10000) {
			for (const item of this.dropdownItemsGames) {
				if (this.updateAssignerForm.assignTargetIDs.includes(item.valueIntOrZero)) {
					let extraText = '';

					if (isCopyNameColumn) {
						// @ts-ignore
						extraText = '\t' + item.text.replaceAll(item.value + '｜', '');
					}

					copyText += item.value + extraText + '\n';
				}
			}
		} else if (this.updateAssignerForm.type === 20000) {
			for (const item of this.dropdownItemsMerchants) {
				if (this.updateAssignerForm.assignTargetIDs.includes(item.valueIntOrZero)) {
					let extraText = '';

					if (isCopyNameColumn) {
						// @ts-ignore
						extraText = '\t' + item.text.replaceAll(item.value + '｜', '');
					}

					copyText += item.value + extraText + '\n';
				}
			}
		} else if (this.updateAssignerForm.type === 50000) {
			for (const item of this.dropdownItemsIMChannels) {
				if (this.updateAssignerForm.assignTargetIDs.includes(item.valueIntOrZero)) {
					let extraText = '';

					if (isCopyNameColumn) {
						// @ts-ignore
						extraText = '\t' + item.text.replaceAll(item.value + '｜', '');
					}
					copyText += item.value + extraText + '\n';
				}
			}
		}

		const vm: any = this;
		vm.copyTextToClipboard(copyText);
	}

	closeAllModal() {
		this.dialogVisible.resetAllValueToFalse();
	}

	clear() {
		this.searchFilter = {} as TagModel.IGeneralTagRequest;
	}

	showEditDialog(id: number) {
		const myFilter: any = {
			targetID: id,
			categoryID: 1001,
		};

		const myConfig: any = {
			eazyFormCallback: this.refresh,
		};

		EventBus.$emit(`globalShowEazyFormEditPopupDialog`, myFilter, myConfig);
	}

	copySuccess() {
		EventBus.$emit(`successfully-copy`);
	}

	reloadIMChannel() {
		DropdownApi.getIMChannels().then((res) => {
			this.dropdownItemsIMChannels = res;
		});
	}

	mounted() {
		this.refresh();

		DropdownApi.getGames().then((res) => {
			this.dropdownItemsGames = res;
		});

		DropdownApi.getMerchants().then((res) => {
			this.dropdownItemsMerchants = res;
		});

		DropdownApi.getTagTypes().then((res) => {
			this.dropdownItemsTagTypes = res;
		});

		this.reloadIMChannel();

		// 鍵盤事件監聽
		const vm = this;
		document.addEventListener('keydown', (event) => {
			const key = event.key; // const {key} = event; in ES6+
			if (key === 'Escape') {
				vm.closeAllModal();
			}
		});
	}
}
</script>


<style scoped>
.grey-layout,
grey-layout,
.grey-layout .timeDiff {
	color: #666;
}
</style>