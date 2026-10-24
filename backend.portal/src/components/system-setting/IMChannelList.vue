<template>
	<v-app v-if="renderComponent" class="app">
		<v-expansion-panels v-model="panel" multiple>
			<v-expansion-panel>
				<v-expansion-panel-content>
					<v-flex>
						<v-row class="search-form">
							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-text-field v-model="searchFilter.keyword" clearable :label="$t('TableHeader.__keywordSearch')">> </v-text-field>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete v-model="searchFilter.tagID" clearable :items="dropdownItemsGeneralTags" :label="$t('TableHeader.__tag')"> </v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete v-model="searchFilter.assignMerchantID" clearable :items="dropdownItemsMerchants" :label="$t('TableHeader.__assignMerchantTotalCount')">
									</v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item search-row-padding">
									<v-btn color="yellow darken-1" text class="btn search" @click.stop="search">
										<v-icon left>search</v-icon>
										{{ $t('Button.__search') }} </v-btn
									>&#9;
									<v-btn color="pink lighten-3" text class="btn search" @click.stop="clear">
										<v-icon left>clear</v-icon>
										{{ $t('Button.__reset') }} </v-btn
									>&#9;
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete v-model="searchFilter.notAssignTagID" clearable :items="dropdownItemsGeneralTags" :label="'查詢「未標記」特定標籤Tag'"> </v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete v-model="searchFilter.specialFilterType" clearable :items="dropdownItemsIMChannelSpecialFilter" :label="'特殊篩選'"> </v-autocomplete>
								</div>
							</v-col>
						</v-row>
					</v-flex>
				</v-expansion-panel-content>
			</v-expansion-panel>
		</v-expansion-panels>

		<v-data-table :headers="headers" id="table" :items="searchList">
			<template v-slot:top>
				<v-toolbar>
					<v-toolbar-title> </v-toolbar-title>
					<v-divider class="mx-4" inset vertical></v-divider>
					<v-spacer></v-spacer>
					<v-card-title>
						<v-btn icon dark v-if="hasPermissionAdvance" @click="showQuickCreate" style="margin-left: 15px">
							<v-icon>add</v-icon>
						</v-btn>

						<v-btn icon dark v-if="hasPermissionAdvance" @click="copySearchListAsExcelFormat" style="margin-left: 5px">
							<v-icon>file_copy</v-icon>
						</v-btn>
					</v-card-title>
				</v-toolbar>
			</template>

			<template v-slot:item.action="{ item }">
				<v-icon small @click="showEditDialog(item)" class="mr-1"> edit </v-icon>

				<v-icon small @click="showCustomizedSettings(item.imChannelID)" class="mr-1"> settings </v-icon>

				<v-icon small @click="getAnnouncementHistory(item.imChannelID, item.channelName)" :title="'發送紀錄'" class="mr-1"> mdi-message-bulleted </v-icon>
			</template>

			<template v-slot:item.imChannelID="{ item }">
				<div>{{ item.imChannelID }}</div>
				<div class="sub-title-text">{{ item.type }}</div>
			</template>

			<template v-slot:item.merchantCount="{ item }">
				<div :class="item.merchantCount > 0 ? 'link-text' : 'link-text-grey'" @click="showMappingMerchants(item.imChannelID)">
					{{ item.merchantCount }}
				</div>
			</template>

			<template v-slot:item.channelName="{ item }">
				<div>
					{{ item.channelName }}
				</div>
				<div class="sub-title-text">
					{{ item.internalMemo }}
				</div>
			</template>

			<template v-slot:item.type="{ item }">
				<v-chip small>{{ item.type }}</v-chip>
			</template>

			<template v-slot:item.isMappingProductionMerchant="{ item }">
				<v-chip color="green darken-2" small v-if="item.isMappingProductionMerchant">是</v-chip>
				<v-chip color="grey darken-3" small v-else>否</v-chip>
			</template>

			<template v-slot:item.tags="{ item }">
				<v-chip small v-for="(myItem, index) in item.tags" :key="index" class="mr-1">
					{{ myItem }}
				</v-chip>
			</template>

			<template v-slot:item.referIPTemplateID="{ item }">
				<div v-if="item.referIPTemplateID > 0">
					<div>{{ item.referIPTemplateName }}</div>
					<div class="sub-title-text">{{ item.referIPTemplateID }}</div>
				</div>
			</template>

			<template v-slot:item.preferLangText="{ item }">
				<v-chip small v-if="item.preferLangText == '中'" color="red darken-4">{{ item.preferLangText }}</v-chip>
				<v-chip small v-else-if="item.preferLangText == '英'" color="pink darken-4">{{ item.preferLangText }}</v-chip>
				<v-chip small v-else color="teal darken-4">{{ item.preferLangText }}</v-chip>
			</template>

			<template v-slot:item.createTime="{ item }">
				<div>
					{{ item.createTime | timeWithAddTimezone(userProfile.timeZone) }}
				</div>

				<div class="sub-title-text">
					{{ item.createBy }}
				</div>
			</template>

			<template v-slot:item.updateTime="{ item }">
				{{ item.updateTime | timeWithAddTimezone(userProfile.timeZone) }}
			</template>
		</v-data-table>

		<!-- 新增/編輯 -->
		<v-dialog v-model="dialogVisible.isShowCreateOrEdit" max-width="500px" persistent>
			<v-card>
				<v-card-title class="grey darken-2">
					<span v-if="!isEdit()">新增</span>
					<span v-if="isEdit()">編輯</span>
					<span v-if="isEdit()" class="f12 ml-1"> ({{ editFrom.imChannelID }})) </span>

					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>
				<v-card-text style="padding-bottom: 100px">
					<v-flex>
						<v-row>
							<v-col cols="12" sm="12" md="12" style="margin-top: 10px">
								<v-text-field class="" v-model="editFrom.channelName" :label="$t('TableHeader.__displayName')" autofocus required clearable> </v-text-field>
							</v-col>
						</v-row>

						<v-row v-if="!isEdit()">
							<v-col cols="12" sm="12" md="12">
								<v-autocomplete class="" v-model="editFrom.type" :label="$t('TableHeader.__type')" :items="dropdownOfIMChannelType" required clearable> </v-autocomplete>
							</v-col>
						</v-row>

						<v-row v-if="isEdit()">
							<v-col cols="12" sm="12" md="12" style="margin-top: 0px">
								<v-text-field class="" v-model="editFrom.memo" :label="$t('TableHeader.__memo')" required clearable> </v-text-field>
							</v-col>
						</v-row>

						<v-row v-if="isEdit()">
							<v-col cols="12" sm="12" md="12" style="margin-top: 0px">
								<v-autocomplete :items="dropdownItemsIPTemplates" :label="'預設IP防火牆配置'" clearable small-chips v-model="editFrom.referIPAddressTemplateID"> </v-autocomplete>
							</v-col>
						</v-row>

						<v-row v-if="isEdit()">
							<v-col cols="12" sm="12" md="12" style="margin-top: 0px">
								<v-text-field :label="'Chat_ID (僅限技術編輯)'" :disabled="!hasPermission_SpecialAllowToUseDevelopeFeature" v-model="editFrom.targetChatID"> </v-text-field>
							</v-col>
						</v-row>
					</v-flex>
				</v-card-text>
				<v-card-actions>
					<v-spacer></v-spacer>
					<v-btn text @click="closeAllModal" class="btn">
						<v-icon left>mdi-cancel</v-icon>
						{{ $t('Button.__cancel') }}
					</v-btn>
					<v-btn color="yellow darken-1" text @click="saveCreateOrUpdate" class="btn">
						<v-icon left>mdi-check-circle</v-icon>
						{{ $t('Button.__submit') }}
					</v-btn>
				</v-card-actions>
			</v-card>
		</v-dialog>

		<!-- 訊息發送紀錄 -->
		<v-dialog v-model="dialogVisible.isShowExtra2" max-width="1000px" persistent>
			<v-card>
				<v-card-title class="grey darken-2">
					訊息發送紀錄 <span class="f16 grey-text ml-3">{{ targetChannelName }}</span>

					<v-icon small color="yellow" class="ml-4">error</v-icon>
					<span class="f12 yellow-text">超過24小時可能會刪除失敗(TG)</span>

					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>

				<v-card-text style="min-height: 400px; padding: 5px 15px">
					<v-data-table :items="announcementHistory" :headers="announcementHistoryHeaders" hide-default-footer>
						<template v-slot:item.action="{ item }">
							<v-icon small @click="triggerMessageRrmove(item.uuid)"> delete </v-icon>
						</template>

						<template v-slot:item.uuid="{ item }">
							<span class="f12">
								{{ item.uuid }}
							</span>
						</template>

						<template v-slot:item.templateTypeText="{ item }">
							<span :title="item.templateID">
								{{ item.templateTypeText }}
							</span>
						</template>

						<template v-slot:item.createTime="{ item }">
							<span style="font-size: 13px">
								{{ item.createTime | timeWithAddTimezone(userProfile.timeZone) }}
							</span>
						</template>

						<template v-slot:item.debugResponse="{ item }">
							<span class="maxWidthBeforeClick maxWidth150" @click="expandMaxWidth($event)">
								{{ item.debugResponse }}
							</span>
						</template>
					</v-data-table>
				</v-card-text>
			</v-card>
		</v-dialog>

		<!-- customized設置編輯 -->
		<v-dialog v-model="dialogVisible.isShowBatchEdit" max-width="900px" scrollable>
			<v-card>
				<v-card-title class="grey darken-2">
					進階設置

					<span small class="f12 yellow-text ml-4"> {{ batchEditSetting.ownerName }}({{ batchEditSetting.ownerID }}) </span>

					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>

				<v-card-text>
					<div class="batchEditGameSettingDetails" style="min-height: 400px">
						<v-row>
							<v-col v-for="(item, index) in batchEditSetting.items" :key="index" cols="12" md="12" sm="12">
								<div style="display: inline-block; width: 250px; vertical-align: super">
									<v-chip> {{ item.settingKey }}｜{{ item.settingKeyText }} </v-chip>
								</div>

								<div style="display: inline-block; width: 250px" class="ml-4">
									<v-autocomplete
										v-if="item.multipleValues != null && item.multipleValues.length > 0"
										multiple
										small-chips
										:disabled="!isEnableEdit(item.settingKey)"
										v-model="item.multipleValues"
										:items="item.dataDropdowns"
										:label="'設置'"
									>
									</v-autocomplete>

									<v-autocomplete
										v-else-if="item.dataDropdowns != null && item.dataDropdowns.length > 0"
										small-chips
										:disabled="!isEnableEdit(item.settingKey)"
										v-model="item.value"
										:items="item.dataDropdowns"
										:label="'設置'"
									>
									</v-autocomplete>

									<v-text-field v-else v-model="item.value" :label="'設置'" :disabled="!isEnableEdit(item.settingKey)"> </v-text-field>
								</div>

								<div style="display: inline-block; width: 200px" class="ml-4">
									<v-btn class="mr-2" v-if="!isEnableEdit(item.settingKey)" style="" @click="switchToEnable(item.settingKey)"> 編輯 </v-btn>

									<v-btn class="mr-2" color="blue" v-if="isEnableEdit(item.settingKey)" @click="saveUpdateCustomizedSetting(item)"> 儲存 </v-btn>

									<v-btn class="mr-2" color="grey" v-if="isEnableEdit(item.settingKey)" @click="clearEditEnableFlag()"> 取消 </v-btn>
								</div>
							</v-col>
						</v-row>
					</div>
				</v-card-text>
			</v-card>
		</v-dialog>

		<TextareaParseToListIDPopup ref="refTextareaParseToListIDPopup" @parentSetResultIDs="setResultMerchantIDs"> </TextareaParseToListIDPopup>
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import * as MerchantModel from '@/models/interfaces/system-setting/merchants';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import * as ResponseModel from '@/models/interfaces/commons/response';
import * as IMModel from '@/models/interfaces/system-setting/imChannel';
import * as EventModel from '@/models/interfaces/system-setting/events';

import MerchantsApi from '@/api/system-setting/merchants';
import IMChannelApi from '@/api/system-setting/imChannel';
import InternalTest from '@/api/commons/internalTest';
import DropdownApi from '@/api/commons/dropdown';
import DatePicker from 'vue2-datepicker';
import { Locale, Currency, Status, GameVendorType } from '@/models/status/common';

import TextareaParseToListIDPopup from '@/components/system-setting/TextareaParseToListIDPopup.vue';
import AnnouncementDetail from '@/components/system-setting/AnnouncementDetail.vue';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { commonMethodMixin } from '@/utilities/common-methods';
import NotificationWarning from '@/components/notification/NotificationWarning.vue';

import { pageData, time } from '@/utilities/api-handler.lib';
import { datetimeMixin, startEndDate, timeWithTimezone, daysAgo, weeksAgo, monthsAgo, formatMixin } from '@/utilities/datetime-format';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import { State, Action, Getter, namespace } from 'vuex-class';
const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';
import { uuid } from 'vue-uuid';

// @ts-ignore
@Component({ components: { NotificationWarning, DatePicker, AnnouncementDetail, TextareaParseToListIDPopup }, mixins: [commonDisplayFiltersMixin, commonMethodMixin, datetimeMixin] })
export default class IMChannelList extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];

	dialogVisible: DropdownModel.DialogVisible = new DropdownModel.DialogVisible();

	renderComponent: boolean = true;
	isRenderExtraComponent: boolean = true;

	perPage: number = 100;

	DataTablePagingParameterSize500: DropdownModel.DataTablePagingParameterSize500 = new DropdownModel.DataTablePagingParameterSize500();
	DataTablePagingParameter: DropdownModel.DataTablePagingParameter = new DropdownModel.DataTablePagingParameter();
	datetimeRangePicker: DropdownModel.DatetimeRangePicker = new DropdownModel.DatetimeRangePicker();

	searchList: IMModel.IIMChannelModel[] = [];
	searchFilter: IMModel.IMChannelRequestFilter = {} as IMModel.IMChannelRequestFilter;
	editFrom: IMModel.IMChannelUpdate = {} as IMModel.IMChannelUpdate;
	targetChannelName: string = '';

	announcementHistory: any[] = [];
	batchEditSetting: ResponseModel.ICommonCustomizedSettingBatchUpdate = {} as ResponseModel.ICommonCustomizedSettingBatchUpdate;
	enableEditSettingKeys: number[] = [];

	panel: number[] = [0];

	dropdownItemsStatus: DropdownModel.IDropdownItems[] = [];
	dropdownItemsMerchants: DropdownModel.IDropdownItems[] = [];
	dropdownItemsIMChannels: DropdownModel.IDropdownItems[] = [];
	dropdownItemsIPTemplates: DropdownModel.IDropdownItems[] = [];
	dropdownItemsGeneralTags: DropdownModel.IDropdownItems[] = [];
	dropdownItemsIMChannelSpecialFilter: DropdownModel.IDropdownItems[] = [];

	get hasPermissionAdvance() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSetting_Merchant_IPAddressTemplate_AdvanceEdit) >= 0;
	}

	get hasPermissionAppendNewIP() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSetting_Merchant_IPAddressTemplate_AppendNewIP) >= 0;
	}

	get hasInternalStaffPermission() {
		return this.hasSuperPermission || this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.Special_PGS_CustomerService) >= 0;
	}

	get hasSuperPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSetting_SystemConfigSetting) >= 0;
	}

	get hasPermission_SpecialAllowToUseDevelopeFeature() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.Special_AllowToUseDevelopeFeature) >= 0;
	}

	get dropdownOfIMChannelType() {
		return [
			{ value: 10000, text: 'TG (Telegram)' },
			{ value: 20000, text: 'Skpye' },
		];
	}

	get headers() {
		const data = [
			{ text: this.$t('TableHeader.__action'), value: 'action', sortable: false, align: 'center', width: '5%' },
			{ text: this.$t('TableHeader.__ID'), value: 'imChannelID', sortable: false, width: '5%' },
			{ text: this.$t('TableHeader.__displayName'), value: 'channelName', sortable: false, width: '12%', align: 'left' },
			{ text: '語系', value: 'preferLangText', sortable: false, width: '6%' },
			{ text: '有正式商戶', value: 'isMappingProductionMerchant', sortable: false, width: '6%' },
			{ text: '商戶數', value: 'merchantCount', sortable: false, width: '7%' },
			{ text: '預設IP防火牆配置', value: 'referIPTemplateID', sortable: false, width: '9%' },
			{ text: 'chat_ID', value: 'memo2', sortable: false, align: 'start', width: '7%' },
			{ text: this.$t('TableHeader.__createTime'), value: 'createTime', sortable: false, align: 'start' },
			{ text: this.$t('TableHeader.__tag'), value: 'tags', sortable: false, align: 'start' },
			{ text: this.$t('TableHeader.__memo') + '(3)', value: 'memo3', sortable: false, align: 'start', width: '7%' },
		];

		return data;
	}

	get announcementHistoryHeaders() {
		const data = [
			{ text: this.$t('TableHeader.__action'), value: 'action', sortable: false, width: '5%' },
			{ text: this.$t('TableHeader.__createTime'), value: 'createTime', sortable: false, align: 'start' },
			{ text: this.$t('TableHeader.__ID'), value: 'uuid', sortable: false, width: '5%' },
			{ text: '文案分類', value: 'templateTypeText', sortable: false, align: 'left' },
			{ text: this.$t('TableHeader.__memo') + '(3)', value: 'memo3', sortable: false, align: 'left' },
			{ text: '訊息ID', value: 'resultMessageID', sortable: false, align: 'left' },
			{ text: this.$t('TableHeader.__memo'), value: 'debugResponse', sortable: false, align: 'left' },
		];

		return data;
	}

	isEdit() {
		return this.editFrom.imChannelID > 0;
	}

	showMappingMerchants(imChannelID: number) {
		const myFilter: any = {
			querySqlID: 900163,
			IMChannelID: imChannelID,
		};

		const myConfig: any = {
			maxWidth: 800,
		};

		EventBus.$emit(`globalShowCustomizedColumnsDisplayPopupDialog`, myFilter, myConfig);
	}

	showCustomizedSettings(imChannelID: number) {
		EventBus.$emit('show_mask', null);
		IMChannelApi.getIMChannelCustomizedSettings(imChannelID)
			.then((res) => {
				this.batchEditSetting = res.data;
				this.dialogVisible.isShowBatchEdit = true;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	search() {
		this.DataTablePagingParameter.pageIndex = 1;
		this.refresh();
	}

	refresh() {
		this.registerReloadComponent();

		this.searchFilter.pageIndex = this.DataTablePagingParameter.pageIndex;
		this.searchFilter.pageSize = this.DataTablePagingParameter.itemsPerPage;
		EventBus.$emit('show_mask', null);
		IMChannelApi.getList(this.searchFilter)
			.then((res) => {
				this.searchList = res.data.items;
				this.DataTablePagingParameter.totalCount = res.data.totalCount;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
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

	saveUpdateCustomizedSetting(item: ResponseModel.ICommonCustomizedSettingItemEditUpdate) {
		EventBus.$emit('show_mask', null);
		IMChannelApi.updateIMChannelCustomizedSettings(item)
			.then((res) => {
				this.clearEditEnableFlag();
				EventBus.$emit(`successfully-modified`);
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	copySearchListAsExcelFormat() {
		let myText = '';

		for (const item of this.searchList) {
			myText += item.imChannelID + '\t' + item.channelName + '\n';
		}

		const vm: any = this;
		vm.copyTextToClipboard(myText);
	}

	showQuickCreate() {
		this.dialogVisible.isShowCreateOrEdit = true;
		this.editFrom = {} as IMModel.IMChannelUpdate;
	}

	showEditDialog(item: IMModel.IIMChannelModel) {
		this.editFrom.imChannelID = item.imChannelID;
		this.editFrom.channelName = item.channelName;
		this.editFrom.memo = item.internalMemo;
		this.editFrom.targetChatID = item.memo2;
		this.editFrom.referIPAddressTemplateID = item.referIPTemplateID;

		setTimeout(() => {
			this.dialogVisible.isShowCreateOrEdit = true;
		}, 100);
	}

	saveCreateOrUpdate() {
		EventBus.$emit('show_mask', null);

		if (this.isEdit()) {
			IMChannelApi.update(this.editFrom)
				.then((res) => {
					EventBus.$emit(`successfully-modified`);
					this.closeAllModal();
					this.search();
				})
				.finally(() => {
					EventBus.$emit('hide_mask', null);
				});
		} else {
			IMChannelApi.create(this.editFrom)
				.then((res) => {
					EventBus.$emit(`successfully-modified`);
					this.closeAllModal();
					this.search();
				})
				.finally(() => {
					EventBus.$emit('hide_mask', null);
				});
		}
	}

	getAnnouncementHistory(myIMChannelID: number, channelName: string) {
		const param = {
			IMChannelID: myIMChannelID,
		};
		EventBus.$emit('show_mask', null);

		this.targetChannelName = channelName;

		IMChannelApi.getAnnouncementHistory(param)
			.then((res) => {
				this.announcementHistory = res.data;
				this.dialogVisible.isShowExtra2 = true;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	triggerMessageRrmove(myUUID: string) {
		if (confirm('是否確定刪除訊息?')) {
			EventBus.$emit('show_mask', null);
			IMChannelApi.removeSingleMessages(myUUID)
				.then((res) => {
					EventBus.$emit(`successfully-modified`);
				})
				.finally(() => {
					EventBus.$emit('hide_mask', null);
				});
		}
	}

	registerReloadComponent() {
		EventBus.$on(`reload-${this.$router.currentRoute.path}`, (info: any) => {
			this.renderComponent = false;
			this.$nextTick(() => {
				this.renderComponent = true;
			});
		});
	}

	showParseTextToListIntDialog(IDs: number[]) {
		const targetRef: any = this.$refs.refTextareaParseToListIDPopup;
		targetRef.showParseTextareaPraseToIDs();
	}

	closeAllModal() {
		this.dialogVisible.resetAllValueToFalse();
	}

	clear() {
		this.searchFilter = {} as MerchantModel.IIPAddressTemplateQueryRequest;
	}

	copySuccess() {
		EventBus.$emit(`successfully-copy`);
	}

	reloadDropdown() {
		DropdownApi.getMerchants().then((res) => {
			this.dropdownItemsMerchants = res;
		});
	}

	mounted() {
		this.refresh();

		DropdownApi.getNormalStatus().then((res) => {
			this.dropdownItemsStatus = res;
		});

		DropdownApi.getMerchants().then((res) => {
			this.dropdownItemsMerchants = res;
		});

		DropdownApi.getIMChannels().then((res) => {
			this.dropdownItemsIMChannels = res;
		});

		DropdownApi.getIPTemplates().then((res) => {
			this.dropdownItemsIPTemplates = res;
		});

		DropdownApi.getGeneralTags(50000).then((res) => {
			this.dropdownItemsGeneralTags = res;
		});

		DropdownApi.getIMChannelSpecialFilterType().then((res) => {
			this.dropdownItemsIMChannelSpecialFilter = res;
		});

		// 鍵盤事件監聽
		const vm = this;
		document.addEventListener('keydown', (event) => {
			const key = event.key; // const {key} = event; in ES6+
			if (key === 'Escape') {
				vm.closeAllModal();
			}
		});

		this.reloadDropdown();
	}
}
</script>

<style scoped>
textarea {
	background: #333 !important;
	height: 200px;
	color: #fff;
	padding: 5px;
	width: 100%;
	outline: none;
	font-size: 16px;
}
</style>
