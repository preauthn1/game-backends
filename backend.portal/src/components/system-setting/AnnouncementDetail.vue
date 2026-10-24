<template>

	<div class="event-theme--dark">

		<v-card v-if="searchResult && searchResult.announcementID > 0"
			style="min-height:95vh;overflow-x: hidden;border: solid 3px #464646;">

			<div class="text-r">

				<div style="padding:10px 30px;height:100px;">
					<v-btn color="grey lighten-1" large @click="closeAllModal"
						class="btn button-font-14 mr-3">
						{{ $t('Button.__cancel') }}
					</v-btn>

					<v-btn color="red lighten-1" large @click="saveSubmit" class="btn button-font-14">
						{{ $t('Button.__save') }}
					</v-btn>
				</div>

			</div>

			<v-row style="padding:5px 30px 5px 30px;    margin-top: -40px !important;"
				v-if="isRenderEditForm">

				<v-col cols="12" sm="4" md="4">

					<div class="row">

						<v-col cols="12" sm="11" md="11">
							<div>

								<v-text-field v-model="searchResult.announcementID" disabled="disabled"
									:label="$t('TableHeader.__ID')">
								</v-text-field>

							</div>
						</v-col>

						<v-col cols="12" sm="11" md="11">
							<div>

								<v-text-field v-model="searchResult.typeText" disabled="disabled"
									:label="$t('TableHeader.__type')">
								</v-text-field>

							</div>
						</v-col>

						<v-col cols="12" sm="11" md="11">
							<div>
								<v-text-field v-model="searchResult.memo"
									:label="$t('TableHeader.__memoOrInternalDisplayName')">
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
								{{ item.settingName}}
							</div>
							<div class="common-info-value" style="color:#888">
								{{ item.settingValue}}
							</div>

						</v-col>

					</div>

				</v-col>

				<v-col cols="12" sm="8" md="8" class="advanceSettingPanel"
					style="padding: 10px 5px 5px 25px;height:80vh;overflow-y: scroll;overflow-x: hidden;;border-bottom:solid 3px #666;">

					<div>

						<div class="common-info-label">
							{{$t("TableHeader.__supportMerchants")}} (模板選擇)

							<span class="link-text" style="float: right;" @click="showMerchantSelectDialog()">
								選擇／新增模板
							</span>
						</div>

						<v-autocomplete clearable v-model="searchResult.merchantSelectTemplateID"
							:disabled="alwaysTrue" :items="templateDropdownItems" dense chips small-chips solo
							outlined :label="'選擇模板'">

						</v-autocomplete>

					</div>

					<div>
						語系設置
					</div>

					<div v-for="(item , index) in searchResult.contents" :key="index">

						<div class="mt-4">

							<v-chip color="green darken-1">
								{{item.languageText}}
							</v-chip>

							<v-chip v-if="item.isDefault" color="brown darken-1" class='ml-2'>
								預設語系
							</v-chip>

							<div style="float:right;display: inline-block;margin-right: 20px;margin-top: 10px;">
								<v-icon class="ml-4 mr-0" color="red darken-2">
									close
								</v-icon>
								<span @click="clearAllTextOfContentBlockItem(item)" class="link-text-grey f12">清除文字
								</span>
							</div>

						</div>

						<div class="editContentBlockWrapper">

							<div v-if="!item.isDisableEditTitle">

								<v-text-field v-model="item.title" :label="$t('TableHeader.__title')">
								</v-text-field>

							</div>

							<div class="edit-description" v-if="!item.isDisableEditDescriptiion">
								<v-textarea v-model="item.description" :label="$t('TableHeader.__description')">
								</v-textarea>
							</div>

							<div>
								<v-text-field v-model="item.imageUrlMiddle" :label="'圖片網址'"
									style="width:85%;display: inline-block;">
								</v-text-field>

								<v-btn color="blue lighten-3" text class="btn search ml-3"
									@click.stop="showImageUpload(item)">
									<v-icon left>file_upload</v-icon>
									上傳
								</v-btn>&#9;

								<!-- 
								<v-icon color="green lighten-2" @click="showImageUpload" class="ml-2" :title="''">
									file_upload
								</v-icon> -->

								<div
									v-if="item.imageUrlMiddle && item.imageUrlMiddle != null && item.imageUrlMiddle.length > 0">

									<a :href="item.imageUrlMiddle" target="_blank" title="open link" class="hover08"
										style="float: right;margin-top: -15px;z-index: 999;position: relative;">
										<img :src="item.imageUrlMiddle" style="height: 50px;" />
									</a>
								</div>
							</div>

							<div>

								<v-autocomplete v-model="item.linkType" :items="linkTypeDropdownItems"
									style="width:400px;" @change="resetValue(item)" chips :label="'連結類型'">
								</v-autocomplete>

							</div>

							<div>

								<v-text-field v-model="item.linkReference" :label="'連結網址'"
									v-if="item.linkType == 99">
								</v-text-field>

								<v-autocomplete v-model="item.linkReference" :label="'遊戲ID'" style="width:400px;"
									:items="gamesIdTextDropdownItems" v-if="item.linkType == 21">
								</v-autocomplete>

								<v-text-field v-model="item.linkReference" :label="'連結網址'" v-if="item.linkType == 0"
									disabled='disabled'>
								</v-text-field>

							</div>

						</div>
					</div>

				</v-col>

			</v-row>

		</v-card>

		<v-dialog v-model="dialogVisible.isShowPopup_1 " max-width="700px">

			<MerchantSelectTemplatePopup ref="refMerchantSelectTemplatePopup"
				@parentCloseDialog="closeAllChildModal()"
				@parentConfirmAndSaveTemplateID="confirmAndSaveTemplateID">
			</MerchantSelectTemplatePopup>

		</v-dialog>

		<ImageUploadDialog ref="refImageUploadDialog"
			@parentSetImageUrlAndClose="setImageUrlAfterUpload" />

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
import AnnouncementApi from '@/api/system-setting/announcement';
import * as AnnModel from '@/models/interfaces/system-setting/announcement';

import DropdownApi from '@/api/commons/dropdown';
import DatePicker from 'vue2-datepicker';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { commonMethodMixin } from '@/utilities/common-methods';
import NotificationWarning from '@/components/notification/NotificationWarning.vue';
import { pageData, time } from '@/utilities/api-handler.lib';
import { datetimeMixin, startEndDate } from '@/utilities/datetime-format';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import { Locale, Currency, Status, GameType, EventType, GameVendorType } from '@/models/status/common';
import ImageUploadDialog from '@/components/system-setting/ImageUploadDialog.vue';
import MerchantSelectTemplatePopup from '@/components/system-setting/MerchantSelectTemplatePopup.vue';
import { State, Action, Getter, namespace } from 'vuex-class';
const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';
const playersModule = namespace('Players');
import { uuid } from 'vue-uuid';

// @ts-ignore
@Component({ components: { NotificationWarning, DatePicker, ImageUploadDialog, MerchantSelectTemplatePopup }, mixins: [commonDisplayFiltersMixin, commonMethodMixin, datetimeMixin] })
export default class EventDetail extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];

	dialogVisible: DropdownModel.DialogVisible = new DropdownModel.DialogVisible();

	isRenderEditForm: boolean = true;
	alwaysTrue: boolean = true;
	renderComponent: boolean = true;
	searchResult: AnnModel.IAnnouncementDetail = {} as AnnModel.IAnnouncementDetail;
	uploadImageTargetContent: AnnModel.IAnnouncementContentBlockItem = {} as AnnModel.IAnnouncementContentBlockItem;

	startTime: number | null = null;
	endTime: number | null = null;

	panel: number[] = [0];

	templateTypeOfAnnounce: number = 6605;

	testArray: number[] = [175001, 175028];
	searchEventID: number = 0;

	organizationsDropdownItems: DropdownModel.IDropdownItems[] = [];
	linkTypeDropdownItems: DropdownModel.IDropdownItems[] = [];
	gamesIdTextDropdownItems: DropdownModel.IDropdownItems[] = []; // string型別的下拉選單
	templateDropdownItems: DropdownModel.IDropdownItems[] = [];

	get hasPermissionCreate() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSettingMerchant_Create) >= 0;
	}
	get hasPermissionUpdate() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSettingMerchant_Update) >= 0;
	}

	search() {
		this.refresh();
	}

	refresh() {
		this.registerReloadComponent();

		this.searchResult = {} as AnnModel.IAnnouncementDetail;

		EventBus.$emit('show_mask', null);

		AnnouncementApi.getDetail(this.searchEventID)
			.then((res) => {
				this.searchResult = res.data;

				console.log(res.data);
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	saveSubmit() {
		EventBus.$emit('show_mask', null);

		this.searchResult.beginTime = startEndDate(this.searchResult.beginTimeTS_1000);
		this.searchResult.endTime = startEndDate(this.searchResult.endTimeTS_1000);

		AnnouncementApi.updateDetail(this.searchResult)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.closeAllModal();
				this.$emit('refreshList');
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	showImageUpload(targetContent: AnnModel.IAnnouncementContentBlockItem) {
		this.uploadImageTargetContent = targetContent;

		const targetRef: any = this.$refs.refImageUploadDialog;
		targetRef.showDialogAndEmptyValue(true);
	}

	setImageUrlAfterUpload(imageUrl: string) {
		this.uploadImageTargetContent.imageUrlMiddle = imageUrl;

		// 刷新頁面
		this.isRenderEditForm = false;
		this.isRenderEditForm = true;
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
			vm.$refs.refMerchantSelectTemplatePopup.initialPopup(this.templateTypeOfAnnounce, this.searchResult.merchantSelectTemplateID);
		});
	}

	confirmAndSaveTemplateID(templateID: number) {
		this.initiaMerchantSelectDropdown();
		this.searchResult.merchantSelectTemplateID = templateID;
		this.closeAllChildModal();
	}

	closeAllChildModal() {
		this.dialogVisible.resetAllValueToFalse();
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

	convertToNumberDropdwon(items: DropdownModel.IDropdownItems[]) {
		if (items.length <= 0) {
			return items;
		}
		return items.map((element: DropdownModel.IDropdownItems) => {
			return { text: element.text, value: Number(element.value) };
		});
	}

	resetValue(item: AnnModel.IAnnouncementContentBlockItem) {
		item.linkReference = '';
	}

	clearAllTextOfContentBlockItem(item: AnnModel.IAnnouncementContentBlockItem) {
		item.title = '';
		item.description = '';
		item.imageUrlTop = '';
		item.imageUrlMiddle = '';
		item.imageUrlBottom = '';
	}

	initiaMerchantSelectDropdown() {
		DropdownApi.getMerchantSelectTemplatesByCategory(this.templateTypeOfAnnounce).then((res) => {
			this.templateDropdownItems = res;
		});
	}

	init() {
		DropdownApi.getGamesStringFormat().then((res) => {
			this.gamesIdTextDropdownItems = res;
		});

		DropdownApi.getLinkTypes().then((res) => {
			this.linkTypeDropdownItems = res;
		});

		this.initiaMerchantSelectDropdown();
	}

	mounted() {}
}
</script>


<style lang="scss" scoped>
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

.editContentBlockWrapper {
	padding: 15px;
	border: dashed 2px #666;
	margin-top: 10px;
	margin-bottom: 20px;
	margin-left: 15px;
}
</style>