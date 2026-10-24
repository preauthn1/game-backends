<template>
	<v-app v-if="renderComponent" class="app">
		<v-expansion-panels v-model="panel" multiple>
			<v-expansion-panel>
				<v-expansion-panel-content>
					<v-flex>
						<v-row class="search-form">
							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-text-field v-model="searchFilter.keyword" :label="$t('TableHeader.__keywordSearch')" clearable> </v-text-field>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete clearable :items="dropdownItemsStatus" v-model="searchFilter.status" :label="$t('TableHeader.__status')"> </v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete clearable :items="dropdownItemsEventType" v-model="searchFilter.eventType" :label="$t('TableHeader.__type')"> </v-autocomplete>
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
									<v-autocomplete clearable :items="dropdownItemsMerchants" v-model="searchFilter.merchantID" :label="$t('TableHeader.__assignMerchantTotalCount')"> </v-autocomplete>
								</div>
							</v-col>

							<v-col cols="12" sm="3" md="3">
								<div class="item">
									<v-autocomplete clearable :items="dropdownItemsEventSpecialFilterTypes" v-model="searchFilter.extraFilterType" :label="$t('TableHeader.__extraFilterType')">
									</v-autocomplete>
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
			:items="searchList"
			:page="DataTablePagingParameter.pageIndex"
			:items-per-page="DataTablePagingParameter.itemsPerPage"
			:server-items-length="DataTablePagingParameter.totalCount"
			:footer-props="DataTablePagingParameter.footerProps"
			@update:page="changePage"
			@update:items-per-page="changeItemPerPage"
		>
			<template v-slot:top>
				<v-toolbar>
					<v-toolbar-title>
						<h4>{{ $t('Menu.SystemSetting_EventManagement') }}</h4>
					</v-toolbar-title>
					<v-divider class="mx-4" inset vertical></v-divider>
					<v-spacer></v-spacer>
					<v-card-title>
						<v-btn icon dark @click="showQuickCreateNewEvent" style="margin-left: 15px">
							<v-icon>add</v-icon>
						</v-btn>
					</v-card-title>
				</v-toolbar>
			</template>

			<template v-slot:item.action="{ item }">
				<v-menu offset-y>
					<template v-slot:activator="{ on, attrs }">
						<div class="dropdown-menu" v-bind="attrs" v-on="on">
							<v-icon dark outline> expand_more </v-icon>
						</div>
					</template>
					<v-list class="dropdonwMenuPanel">
						<v-list-item class="dropdownMenu">
							<v-list-item @click="showInfo(item.eventID)">
								<v-icon> edit </v-icon>
								{{ $t('Button.__edit') }}
							</v-list-item>

							<!-- <v-list-item @click="showSelectMerchantByParentAgentDialog(item.eventID)">
								<v-icon>
									edit
								</v-icon>
								透過代理樹狀分派商戶
							</v-list-item> -->

							<!-- <v-list-item @click="redirectToPrizeRecord(item)">

								<v-icon>
									assignment
								</v-icon>
								{{$t("Button.__view")}} {{$t("Menu.SystemSetting_EventPrizeRecord")  }}

							</v-list-item> -->

							<v-list-item @click="showAppendCusomizedSetting(item)">
								<v-icon> settings </v-icon>
								編輯特殊配置
							</v-list-item>

							<v-list-item @click="showEditEventContentBlock(item.eventID)">
								<v-icon> settings </v-icon>
								編輯活動說明
							</v-list-item>

							<v-list-item v-if="item.eventType == 32" @click="showCopyEvent(item)">
								<v-icon> file_copy </v-icon>
								複製活動
							</v-list-item>

							<v-list-item @click="showDeleteHint(item)">
								<v-icon> delete </v-icon>
								{{ $t('Button.__delete') }}
							</v-list-item>

							<v-list-item @click="showResetRTPHint(item)" v-if="item.eventType == 51">
								<v-icon> stars </v-icon>
								{{ $t('Button.__reset') }} RTP
							</v-list-item>

							<v-list-item @click="getRankEventPreview(item.eventID)" v-if="item.eventType == 81">
								<v-icon> visibility </v-icon>
								查看排名
							</v-list-item>

							<v-list-item @click="showEventDailyAnalyzeReport(item.eventID)">
								<v-icon> visibility </v-icon>
								獎勵派發日報表
							</v-list-item>
						</v-list-item>
					</v-list>
				</v-menu>
			</template>

			<template v-slot:item.displayName="{ item }">
				<div class="double-row-td">
					<div :title="item.eventID">
						{{ item.displayName }}
					</div>

					<div style="color: #999; font-size: 12px; line-height: 10px">
						{{ item.eventID }}
					</div>
				</div>
			</template>

			<template v-slot:item.eventTypeText="{ item }">
				<div :title="item.eventType">
					{{ item.eventTypeText }}
				</div>
				<div class="sub-title-text">
					{{ item.eventTypeMemo }}
				</div>
			</template>

			<template v-slot:item.status="{ item }">
				<span :class="item.status == 0 ? 'minus-red' : item.isAlreadyEnd ? 'grey-text' : ''">
					{{ item.statusText }}
				</span>
			</template>

			<template v-slot:item.beginTime="{ item }">
				<div class="double-row-td">
					<div>
						<span :class="begintTimeStyle(item)">
							{{ item.beginTime | timeWithAddTimezone(userProfile.timeZone) }}
						</span>
					</div>

					<div>
						<span :class="begintTimeStyle(item)">
							{{ item.endTime | timeWithAddTimezone(userProfile.timeZone) }}
						</span>
					</div>
				</div>
			</template>

			<template v-slot:item.availableMerchantName="{ item }">
				<div style="max-width: 200px; overflow: hidden; white-space: nowrap; text-overflow: ellipsis">
					{{ item.availableMerchantName }}
				</div>
			</template>

			<template v-slot:item.endTime="{ item }">
				<span :class="begintTimeStyle(item)">
					{{ item.endTime | timeWithAddTimezone(userProfile.timeZone) }}
				</span>
			</template>

			<template v-slot:item.merchantSelectTemplateName="{ item }">
				<v-chip
					small
					:title="item.merchantSelectTemplateID"
					@click="showMerchantSelectDialog(item)"
					style="max-width: 125px; overflow: hidden; text-overflow: ellipsis"
					v-if="item.merchantSelectTemplateName != null && item.merchantSelectTemplateName.length > 0"
				>
					{{ item.merchantSelectTemplateName }}
				</v-chip>
			</template>

			<template v-slot:item.matchMultiplePrizeName="{ item }">
				<v-icon left small color="pink darken-2" v-if="item.matchMultiplePrizeId > 0">mdi-gift </v-icon>
				<span color="red darken-4" :title="item.matchMultiplePrizeId" style="margin-left: -5px" v-if="item.matchMultiplePrizeId > 0">
					{{ item.matchMultiplePrizeName }}
				</span>
			</template>

			<template v-slot:item.extraTags="{ item }">
				<v-chip v-for="(tag, index) in item.extraTags" small :class="dynamicColorByTag(tag) + ' mr-1 '" :key="index">
					{{ tag }}
				</v-chip>
			</template>

			<template v-slot:item.createTime="{ item }">
				<div>
					{{ item.createTime | timeWithAddTimezone(userProfile.timeZone) }}
				</div>

				<div class="sub-title-text">
					{{ item.createBy }}
				</div>
			</template>
		</v-data-table>

		<!-- 快速新增 -->
		<v-dialog v-model="isShowCreateEventDialog" max-width="500px">
			<v-card>
				<v-card-title class="grey darken-2">
					快速新增活動
					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark @click="refresh">
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>
				<v-card-text style="padding-bottom: 250px">
					<v-flex>
						<v-row>
							<v-text-field class="required" v-model="createEventFrom.displayName" :label="$t('TableHeader.__displayName')" autofocus required clearable> </v-text-field>
						</v-row>

						<v-row>
							<v-select class="required" attach chips :label="$t('TableHeader.__type')" v-model="createEventFrom.eventTemplateType" :items="dropdownItemsEventTemplateType"> </v-select>
						</v-row>

						<v-row v-if="createEventFrom.extraEventParameter && createEventFrom.eventTemplateType === 51">
							<v-select
								class="required"
								attach
								chips
								:label="$t('TableHeader.__vendorConfig')"
								v-model="createEventFrom.extraEventParameter.vendorConfigID"
								:items="dropdownItemsVendorConfig"
							>
							</v-select>
						</v-row>

						<v-row
							v-if="
								createEventFrom.extraEventParameter &&
								(createEventFrom.eventTemplateType === 61 ||
									createEventFrom.eventTemplateType === 62 ||
									createEventFrom.eventTemplateType === 63 ||
									createEventFrom.eventTemplateType === 64 ||
									createEventFrom.eventTemplateType === 65 ||
									createEventFrom.eventTemplateType === 66 ||
									createEventFrom.eventTemplateType === 67)
							"
						>
							<v-select
								class="required"
								attach
								chips
								:label="$t('TableHeader.__eventID')"
								v-model="createEventFrom.extraEventParameter.eventID"
								:items="dropdownItemsLuckyWheelEventsWihtoutPointCounter"
							>
							</v-select>
						</v-row>

						<v-row
							v-if="
								createEventFrom.extraEventParameter &&
								(createEventFrom.eventTemplateType === 61 ||
									createEventFrom.eventTemplateType === 62 ||
									createEventFrom.eventTemplateType === 63 ||
									createEventFrom.eventTemplateType === 64)
							"
						>
							<v-select
								class="required"
								attach
								chips
								:label="'使用哪個數值來統計'"
								v-if="createEventFrom.eventTemplateType === 61 || createEventFrom.eventTemplateType === 63"
								v-model="createEventFrom.extraEventParameter.valueCompareType"
								:items="valueCompareTypeDropdown_AllInOne"
							>
							</v-select>

							<v-select
								class="required"
								attach
								chips
								:label="'使用哪個數值來統計'"
								v-if="createEventFrom.eventTemplateType === 62 || createEventFrom.eventTemplateType === 64"
								v-model="createEventFrom.extraEventParameter.valueCompareType"
								:items="valueCompareTypeDropdown_SingleGame"
							>
							</v-select>
						</v-row>
					</v-flex>
				</v-card-text>
				<v-card-actions>
					<v-spacer></v-spacer>
					<v-btn text @click="closeAllModal" class="btn">
						<v-icon left>mdi-cancel</v-icon>
						{{ $t('Button.__cancel') }}
					</v-btn>
					<v-btn color="yellow darken-1" text @click="saveQuickCreateNewEvent" class="btn">
						<v-icon left>mdi-check-circle</v-icon>
						{{ $t('Button.__submit') }}
					</v-btn>
				</v-card-actions>
			</v-card>
		</v-dialog>

		<!-- 重製RTP獎勵 -->
		<v-dialog v-model="isShowResetWarning" max-width="500px">
			<v-card>
				<v-card-title class="grey darken-2">
					將所有玩家RTP還原回預設值(自然機率)
					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark @click="refresh">
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>
				<v-card-text>
					<v-flex style="padding: 10px">
						<v-row>
							<v-text-field class="required" v-model="warningEventItem.eventID" disabled="disabled" :label="$t('TableHeader.__ID')" autofocus required> </v-text-field>
						</v-row>

						<v-row>
							<v-text-field class="required" v-model="warningEventItem.displayName" disabled="disabled" :label="$t('TableHeader.__displayName')" autofocus required> </v-text-field>
						</v-row>

						<v-row>
							<v-icon small class="mr-2" style="color: #ca4b4b; font-size: 20px" @click="showResetRTPHint(item)"> info </v-icon>

							<span class="minus-red"> 會將此活動獲得RTP獎勵的玩家全部還原回自然機率 (僅針對此活動的玩家) </span>
						</v-row>
					</v-flex>
				</v-card-text>
				<v-card-actions>
					<v-spacer></v-spacer>
					<v-btn text @click="closeAllModal" class="btn">
						<v-icon left>mdi-cancel</v-icon>
						{{ $t('Button.__cancel') }}
					</v-btn>
					<v-btn color="yellow darken-1" text @click="confirmResetEventRTP" class="btn">
						<v-icon left>mdi-check-circle</v-icon>
						{{ $t('Button.__submit') }}
					</v-btn>
				</v-card-actions>
			</v-card>
		</v-dialog>

		<!-- 刪除活動 -->
		<v-dialog v-model="isShowDeleteWarning" max-width="500px">
			<v-card>
				<v-card-title class="grey darken-2">
					確定刪除?
					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>
				<v-card-text>
					<v-flex style="padding: 10px">
						<v-row>
							<v-text-field class="" v-model="deleteEventItem.eventID" disabled="disabled" :label="$t('TableHeader.__ID')" autofocus> </v-text-field>
						</v-row>

						<v-row>
							<v-text-field class="" v-model="deleteEventItem.displayName" disabled="disabled" :label="$t('TableHeader.__displayName')" autofocus> </v-text-field>
						</v-row>

						<v-row>
							<v-text-field class="" v-model="deleteEventItem.eventTypeText" disabled="disabled" :label="$t('TableHeader.__type')" autofocus> </v-text-field>
						</v-row>

						<v-row>
							<v-text-field class="" v-model="deleteEventItem.statusText" disabled="disabled" :label="$t('TableHeader.__status')" autofocus> </v-text-field>
						</v-row>

						<v-row>
							<v-text-field class="" v-model="deleteEventItem.createBy" disabled="disabled" :label="$t('TableHeader.__createBy')" autofocus> </v-text-field>
						</v-row>
					</v-flex>
				</v-card-text>

				<div style="font-size: 13px; text-align: right; padding: 0px 15px" class="minus-red">
					<v-icon class="minus-red" color="red lighten-4"> info </v-icon>
					為安全起見。刪除活動需先把活動狀態設為關閉
				</div>

				<v-card-actions>
					<v-spacer></v-spacer>

					<v-btn text @click="closeAllModal" class="btn">
						<v-icon left>mdi-cancel</v-icon>
						{{ $t('Button.__cancel') }}
					</v-btn>

					<v-btn color="yellow darken-1" text @click="confirmDeleteHint" class="btn" :disabled="deleteEventItem.status === 0 || deleteEventItem.isAlreadyEnd ? false : true">
						<v-icon left>mdi-check-circle</v-icon>
						{{ $t('Button.__submit') }}
					</v-btn>
				</v-card-actions>
			</v-card>
		</v-dialog>

		<!-- 複製活動 -->
		<v-dialog v-model="isShowCopyEventDialog" max-width="500px">
			<v-card>
				<v-card-title class="grey darken-2">
					複製活動
					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>
				<v-card-text>
					<v-flex style="padding: 10px">
						<v-row>
							<div class="w100 mb-4">
								<div class="common-info-label">
									{{ $t('TableHeader.__eventName') }}
								</div>
								<div class="common-info-value">
									{{ copyEventForm.sourceEventName }}
								</div>
							</div>
						</v-row>

						<v-row>
							<v-text-field class="" v-model="copyEventForm.name" :label="$t('TableHeader.__displayName')" autofocus> </v-text-field>
						</v-row>

						<v-row>
							<v-text-field class="" v-model="copyEventForm.displayNameExternal" :label="'外部显示名称'"> </v-text-field>
						</v-row>

						<v-row>
							<div class="yellow-text"><v-icon small color="yellow">info</v-icon>*僅部分設置會進行複製</div>
						</v-row>
					</v-flex>
				</v-card-text>

				<v-card-actions>
					<v-spacer></v-spacer>

					<v-btn text @click="closeAllModal" class="btn">
						<v-icon left>mdi-cancel</v-icon>
						{{ $t('Button.__cancel') }}
					</v-btn>

					<v-btn color="yellow darken-1" text @click="saveCopyEvent" class="btn" :disabled="deleteEventItem.status === 1">
						<v-icon left>mdi-check-circle</v-icon>
						{{ $t('Button.__submit') }}
					</v-btn>
				</v-card-actions>
			</v-card>
		</v-dialog>

		<!-- 派發排名獎勵 -->
		<v-dialog v-model="isShowRankPreview" max-width="1100px">
			<EventRankList ref="refEventRankList" @closeParentAllModal="closeAllModal"> </EventRankList>
		</v-dialog>

		<v-dialog v-model="isShowEventDetail" fullscreen persistent>
			<EventDetail ref="refEventDetail" @closeParentAllModal="closeAllModal" @refreshList="refresh"> </EventDetail>
		</v-dialog>

		<v-dialog v-model="isShowEditEventContentBlock" persistent max-width="880px">
			<EventContentBlock ref="refEventContentBlock" @closeParentAllModal="closeAllModal"> </EventContentBlock>
		</v-dialog>

		<v-dialog v-model="isShowMerchantSelectView" max-width="700px">
			<MerchantSelectTemplatePopup ref="refMerchantSelectTemplatePopup" @parentCloseDialog="closeAllModal()"> </MerchantSelectTemplatePopup>
		</v-dialog>

		<v-dialog v-model="isShowAppendCustomizedSetting" max-width="500px">
			<v-card>
				<v-card-title class="grey">
					插入特殊配置
					<div class="top-right-item" @click="closeAllModal">
						<v-btn icon dark>
							<v-icon>close</v-icon>
						</v-btn>
					</div>
				</v-card-title>

				<v-card-text>
					<v-row>
						<v-text-field :label="'活動'" disabled :value="appendEventSettingFrom.displayName + '(' + appendEventSettingFrom.eventID + ')'"> </v-text-field>
					</v-row>

					<v-row>
						<v-autocomplete :label="'配置項目'" v-model="appendEventSettingFrom.settingKey" :items="dropdownItemsManualAddEventCustomizedSetting"> </v-autocomplete>
					</v-row>
					<v-row>
						<v-text-field :label="'配置值'" v-model="appendEventSettingFrom.settingValue"> </v-text-field>
					</v-row>
				</v-card-text>

				<v-card-actions>
					<v-spacer></v-spacer>

					<v-btn text @click="closeAllModal" class="btn">
						<v-icon left>mdi-cancel</v-icon>
						{{ $t('Button.__cancel') }}
					</v-btn>

					<v-btn color="yellow darken-1" text @click="saveAppendCustomizedSetting" class="btn">
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
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import * as EventModel from '@/models/interfaces/system-setting/events';
import MerchantsApi from '@/api/system-setting/merchants';
import EventsApi from '@/api/system-setting/events';
import DropdownApi from '@/api/commons/dropdown';
import DatePicker from 'vue2-datepicker';
import { Locale, Currency, Status, GameVendorType } from '@/models/status/common';
import EventDetail from '@/components/system-setting/events/EventDetail.vue';
import EventRankList from '@/components/system-setting/events/EventRankList.vue';
import EventContentBlock from '@/components/system-setting/events/EventContentBlock.vue';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { commonMethodMixin } from '@/utilities/common-methods';
import NotificationWarning from '@/components/notification/NotificationWarning.vue';
import MerchantSelectTemplatePopup from '@/components/system-setting/MerchantSelectTemplatePopup.vue';
import { pageData, time } from '@/utilities/api-handler.lib';
import { datetimeMixin, startEndDate, timeWithTimezone, daysAgo, weeksAgo, monthsAgo, formatMixin } from '@/utilities/datetime-format';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import { State, Action, Getter, namespace } from 'vuex-class';
const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';
import { uuid } from 'vue-uuid';

// @ts-ignore
@Component({
	components: { NotificationWarning, DatePicker, EventDetail, EventRankList, EventContentBlock, MerchantSelectTemplatePopup },
	mixins: [commonDisplayFiltersMixin, commonMethodMixin, datetimeMixin],
})
export default class EventList extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];

	isShowCopyEventDialog: boolean = false;
	isShowRankPreview: boolean = false;
	isShowDeleteWarning: boolean = false;
	isShowResetWarning: boolean = false;
	isShowEventDetail: boolean = false;
	isShowMerchantSelectView: boolean = false;
	isShowCreateEventDialog: boolean = false;
	isShowEditEventContentBlock: boolean = false;
	isShowAppendCustomizedSetting: boolean = false;
	isShowSelectMerchantByParentAgentDialog: boolean = false;
	isRenderCardText: boolean = true;

	renderComponent: boolean = true;

	perPage: number = 100;

	DataTablePagingParameterSize500: DropdownModel.DataTablePagingParameterSize500 = new DropdownModel.DataTablePagingParameterSize500();
	DataTablePagingParameter: DropdownModel.DataTablePagingParameter = new DropdownModel.DataTablePagingParameter();
	datetimeRangePicker: DropdownModel.DatetimeRangePicker = new DropdownModel.DatetimeRangePicker();
	searchList: EventModel.IEventList[] = [];
	searchFilter: EventModel.IEventListRequest = {} as EventModel.IEventListRequest;
	createEventFrom: EventModel.IEventQuickCreateRequest = {} as EventModel.IEventQuickCreateRequest;
	appendEventSettingFrom: EventModel.IEventAppendSettingRequest = {} as EventModel.IEventAppendSettingRequest;

	selectMerchantByParentForm: EventModel.ISelectMerchantByParentAgentRequest = {} as EventModel.ISelectMerchantByParentAgentRequest;

	warningEventItem: EventModel.IEventList = {} as EventModel.IEventList;
	deleteEventItem: EventModel.IEventList = {} as EventModel.IEventList;
	rankEventPreview: EventModel.IEventRankPreview = {} as EventModel.IEventRankPreview;
	copyEventForm: EventModel.IEventCopyRequest = {} as EventModel.IEventCopyRequest;

	templateTypeOfEvent: number = 6609;
	panel: number[] = [0];
	editModeOpenArray: number[] = [];
	merchantsData: Model.IMerchant[] = [];

	dropdownItemsLuckyWheelEventsWihtoutPointCounter: DropdownModel.IDropdownItems[] = [];
	dropdownItemsEventTemplateType: DropdownModel.IDropdownItems[] = [];
	dropdownItemsEventType: DropdownModel.IDropdownItems[] = [];
	dropdownItemsVendorConfig: DropdownModel.IDropdownItems[] = [];
	dropdownItemsStatus: DropdownModel.IDropdownItems[] = [];
	dropdownItemsMerchants: DropdownModel.IDropdownItems[] = [];
	dropdownItemsAgentMerchants: DropdownModel.IDropdownItems[] = [];
	dropdownItemsExcludeAgentMerchants: DropdownModel.IDropdownItems[] = [];
	dropdownItemsManualAddEventCustomizedSetting: DropdownModel.IDropdownItems[] = [];
	dropdownItemsEventSpecialFilterTypes: DropdownModel.IDropdownItems[] = [];

	@Action('Players/setEventID') private setEventID!: any;

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
		const vm: any = this;
		const data = [
			{ text: this.$t('TableHeader.__action'), value: 'action', width: '4%', sortable: false, align: 'center' },
			{ text: this.$t('TableHeader.__type'), value: 'eventTypeText', sortable: false, width: '5%' },
			{ text: this.$t('TableHeader.__displayName'), value: 'displayName', sortable: false, width: '15%' },
			{ text: this.$t('TableHeader.__beginTime') + ' - ' + this.$t('TableHeader.__endTime'), value: 'beginTime', sortable: false, width: '7%' },
			{ text: this.$t('TableHeader.__status'), value: 'status', sortable: false, width: '7%' },
			{ text: this.$t('TableHeader.__supportGames'), value: 'numberOfGames', sortable: false, width: '5%' },
			{ text: this.$t('TableHeader.__supportMerchants'), value: 'merchantSelectTemplateName', sortable: false, width: '15%' },
			{ text: this.$t('TableHeader.__createTime'), value: 'createTime', sortable: false, width: '5%' },
			{ text: this.$t('TableHeader.__tag'), value: 'extraTags', sortable: false },
		];

		return data;
	}

	get headersRankPreivew() {
		const data = [
			{ text: '順序', value: 'sequence', width: '8%', sortable: false, align: 'center' },
			{ text: this.$t('TableHeader.__playerID'), value: 'playerID', width: '8%', sortable: false, align: 'center' },
			{ text: this.$t('TableHeader.__playerAccount'), value: 'playerName', sortable: false, width: '8%' },
			{ text: this.$t('TableHeader.__totalRound'), value: 'totalRound', sortable: false },
			{ text: this.$t('TableHeader.__betAmount'), value: 'totalBetAmount', sortable: false },
			{ text: this.$t('TableHeader.__payoutAmount'), value: 'totalPayoutAmount', sortable: false },
			{ text: this.$t('TableHeader.__playerWinLoss'), value: 'totalWinLoss', sortable: false },
		];

		return data;
	}

	get valueCompareTypeDropdown_AllInOne() {
		const dropdownItems: DropdownModel.IDropdownItems[] = [];
		dropdownItems.push({ text: '總遊玩局數', valueInt: 101, value: 101 } as DropdownModel.IDropdownItems);
		dropdownItems.push({ text: '有效流水(累計投注積分)', valueInt: 111, value: 111 } as DropdownModel.IDropdownItems);
		return dropdownItems;
	}

	get valueCompareTypeDropdown_SingleGame() {
		const dropdownItems: DropdownModel.IDropdownItems[] = [];
		dropdownItems.push({ text: '總遊玩局數', valueInt: 102, value: 102 } as DropdownModel.IDropdownItems);
		dropdownItems.push({ text: '有效流水(累計投注積分)', valueInt: 112, value: 112 } as DropdownModel.IDropdownItems);

		return dropdownItems;
	}

	changePage(index: number = 1) {
		this.DataTablePagingParameter.pageIndex = index;
		this.refresh();
	}

	dynamicColorByTag(tag: string) {
		if (tag.includes('獎項')) {
			return 'red darken-4';
		}
		if (tag.includes('獎項')) {
			return 'red darken-4';
		}

		if (tag.includes('全數派完')) {
			return 'orange darken-3';
		}

		if (tag === '一次性') {
			return 'green darken-3';
		}

		// if (tag === '每日任務') {
		// 	return 'grey darken-1';
		// }

		return 'lime darken-4';
	}

	iconsByEventType(eventType: number) {
		if (eventType === 51) {
			return 'query_builder';
		}

		if (eventType === 32) {
			return 'bookmarks';
		}

		if (eventType === 81) {
			return 'description';
		}

		return 'stars';
	}

	showAppendCusomizedSetting(eventItem: EventModel.IEventList) {
		this.isShowAppendCustomizedSetting = true;
		this.appendEventSettingFrom = {} as EventModel.IEventAppendSettingRequest;

		this.appendEventSettingFrom.eventID = eventItem.eventID;
		this.appendEventSettingFrom.displayName = eventItem.displayName;
	}

	saveAppendCustomizedSetting() {
		EventsApi.appendEventSetting(this.appendEventSettingFrom)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.closeAllModal();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	showMerchantSelectDialog(eventItem: any) {
		this.isShowMerchantSelectView = true;

		const vm: any = this;

		this.$nextTick(() => {
			vm.$refs.refMerchantSelectTemplatePopup.initialPopupLockAll(this.templateTypeOfEvent, eventItem.merchantSelectTemplateID);
		});
	}

	showEventDailyAnalyzeReport(myEventID: number) {
		const myFilter: any = {
			querySqlID: 900015,
			eventID: myEventID,
		};

		const config: any = {
			maxWidth: 1000,
		};

		EventBus.$emit(`globalShowCustomizedColumnsDisplayPopupDialog`, myFilter, config);
	}

	changeItemPerPage(val: number = 10) {
		this.DataTablePagingParameter.itemsPerPage = val;
		this.refresh();
	}

	changeDropdownWhenParentMerchantChanged() {
		if (!this.selectMerchantByParentForm.merchantSetting) {
			return;
		}

		const myFilter = {} as DropdownModel.ILazyCommonParameterReqeust;
		myFilter.parentMerchantID = this.selectMerchantByParentForm.merchantSetting.merchantID;

		DropdownApi.getHierarchyMerchantsByParam(myFilter).then((res) => {
			this.dropdownItemsExcludeAgentMerchants = res;
		});
	}

	begintTimeStyle(item: any) {
		if (item.isExpired) {
			return 'light-opacity';
		}
		if (item.status === 0 && item.isInProgress) {
			return 'minus-red';
		}

		return '';
	}

	search() {
		this.DataTablePagingParameter.pageIndex = 1;
		this.refresh();
	}

	refresh() {
		this.registerReloadComponent();
		this.searchFilter.pageIndex = this.DataTablePagingParameter.pageIndex;
		this.searchFilter.pageSize = this.DataTablePagingParameter.itemsPerPage;

		this.searchFilter.orderByColumn = 'EventID';
		this.searchFilter.isASC = false;

		EventBus.$emit('show_mask', null);

		EventsApi.getEvents(this.searchFilter)
			.then((res) => {
				this.searchList = res.data.items;
				this.DataTablePagingParameter.totalCount = res.data.totalCount;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	showEditEventContentBlock(eventID: number) {
		this.isShowEditEventContentBlock = true;

		this.$nextTick(() => {
			const myRef: any = this.$refs.refEventContentBlock;
			myRef.getEventContentBlock(eventID);
		});
	}

	showQuickCreateNewEvent() {
		this.isShowCreateEventDialog = true;
		this.createEventFrom = {} as EventModel.IEventQuickCreateRequest;
		this.createEventFrom.extraEventParameter = {} as EventModel.IEventQuickCreateExtraParameterRequest;
	}

	saveQuickCreateNewEvent() {
		EventBus.$emit('show_mask', null);

		this.createEventFrom.extraEventParameter.isAccountCreateTimeCheck = true;

		EventsApi.quickCreateEvent(this.createEventFrom)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);

				this.closeAllModal();
				this.search();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	showCopyEvent(item: EventModel.IEventList) {
		this.isShowCopyEventDialog = true;
		this.copyEventForm = {} as EventModel.IEventCopyRequest;
		this.copyEventForm.sourceEventID = item.eventID;
		this.copyEventForm.sourceEventName = item.displayName + ' (' + item.eventID.toString() + ')';
		this.copyEventForm.name = item.displayName;
		this.copyEventForm.displayNameExternal = item.displayNameExternal;
	}

	saveCopyEvent() {
		EventBus.$emit('show_mask', null);
		EventsApi.copyEvent(this.copyEventForm)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.closeAllModal();
				this.search();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	registerReloadComponent() {
		EventBus.$on(`reload-${this.$router.currentRoute.path}`, (info: any) => {
			this.renderComponent = false;
			this.$nextTick(() => {
				this.renderComponent = true;
			});
		});
	}

	showDeleteHint(item: EventModel.IEventList) {
		const vm: any = this;

		this.isShowDeleteWarning = true;
		this.deleteEventItem = item;
	}

	confirmDeleteHint(item: EventModel.IEventList) {
		const vm: any = this;
		EventBus.$emit('show_mask', null);
		EventsApi.deleteEvent(this.deleteEventItem.eventID)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.closeAllModal();
				this.search();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	showResetRTPHint(item: EventModel.IEventList) {
		const vm: any = this;

		this.warningEventItem = item;
		this.isShowResetWarning = true;
	}

	redirectToPrizeRecord(item: EventModel.IEventList) {
		const para = {
			eventID: item.eventID,
		};

		const myRouter: any = this.$router;
		myRouter.push({ path: '/system-setting/event-prize-record', params: para, query: para });
	}

	confirmResetEventRTP() {
		EventBus.$emit('show_mask', null);
		EventsApi.resetEventPlayerRTP(this.warningEventItem.eventID)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				alert(res.data.message);
				this.closeAllModal();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	showInfo(eventID: number) {
		this.showEventDetail(eventID);
	}

	showEventDetail(eventID: number) {
		this.isShowEventDetail = true;

		this.$nextTick(() => {
			const myRef: any = this.$refs.refEventDetail;
			myRef.queryTargetEventID(eventID);
		});
	}

	saveSelectMerchantByParentAgentDialog(eventID: number) {
		EventBus.$emit('show_mask', null);
		EventsApi.updateSelectMerchantByParentAgent(this.selectMerchantByParentForm)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.closeAllModal();
				this.refresh();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	resetToNormalMode() {
		if (confirm('確認進行此操作?(還原)')) {
			this.selectMerchantByParentForm.merchantSetting.merchantID = 0;
			this.selectMerchantByParentForm.merchantSetting.excludeMerchantIDs = [];
			this.selectMerchantByParentForm.merchantSetting.isResetToInitial = true;

			EventBus.$emit('show_mask', null);
			EventsApi.updateSelectMerchantByParentAgent(this.selectMerchantByParentForm)
				.then((res) => {
					EventBus.$emit(`successfully-modified`);
					this.closeAllModal();
					this.refresh();
				})
				.finally(() => {
					EventBus.$emit('hide_mask', null);
				});
		}
	}

	eventTypeColor(eventType: number) {
		if (eventType === 51) {
			return 'blue darken-3';
		}

		if (eventType === 32) {
			return 'green darken-3';
		}
		return '';
	}

	getRankEventPreview(eventID: number) {
		this.isShowRankPreview = true;

		this.$nextTick(() => {
			const myRef: any = this.$refs.refEventRankList;
			myRef.getRankEventPreview(eventID);
		});
	}

	closeAllModal() {
		this.isShowEventDetail = false;
		this.isShowResetWarning = false;
		this.isShowCreateEventDialog = false;
		this.isShowDeleteWarning = false;
		this.isShowCopyEventDialog = false;
		this.isShowRankPreview = false;
		this.isShowEditEventContentBlock = false;
		this.isShowSelectMerchantByParentAgentDialog = false;
		this.isShowMerchantSelectView = false;
		this.isShowAppendCustomizedSetting = false;
	}

	clear() {
		this.searchFilter = {} as Model.IMerchantFilterRequest;
	}

	copySuccess() {
		EventBus.$emit(`successfully-copy`);
	}

	mounted() {
		this.refresh();

		DropdownApi.getEventTemplateTypes().then((res) => {
			this.dropdownItemsEventTemplateType = res;
		});

		DropdownApi.getEventTypes().then((res) => {
			this.dropdownItemsEventType = res;
		});

		DropdownApi.getVendorConfig(GameVendorType.AGG).then((res) => {
			this.dropdownItemsVendorConfig = res;
		});

		DropdownApi.getNormalStatus().then((res) => {
			this.dropdownItemsStatus = res;
		});

		DropdownApi.getMerchants().then((res) => {
			this.dropdownItemsMerchants = res;
		});

		DropdownApi.getHierarchyMerchants().then((res) => {
			this.dropdownItemsAgentMerchants = res;
		});

		const myFilter = {} as DropdownModel.ILazyCommonParameterReqeust;
		myFilter.eventTemplateType = 94;

		DropdownApi.getEventsByFilter(myFilter).then((res) => {
			this.dropdownItemsLuckyWheelEventsWihtoutPointCounter = res;
		});

		DropdownApi.getEventSettingsForManualAdd(myFilter).then((res) => {
			this.dropdownItemsManualAddEventCustomizedSetting = res;
		});

		DropdownApi.getEventSpecialFilterTypes().then((res) => {
			this.dropdownItemsEventSpecialFilterTypes = res;
		});
	}
}
</script>

<style scoped>
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

.eventType_32 {
	/* 紅包 */
	background: #863f3f !important;
}

.eventType_51 {
	/* 甜蜜期 (顏色太多反而炫目) */
	/* background: #9c719e !important; */
}
</style>
