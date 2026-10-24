<template>
	<v-app v-if="renderComponent" class="app">
		<v-row class="maxH300-in-phone">
			<v-col cols="12" md="3" sm="12">
				<v-autocomplete :items="dropdownItemsTemplates" v-model="selectTemplateFilter.templateID" chips :label="'選擇發送文案範本'"> </v-autocomplete>
			</v-col>

			<v-col cols="12" md="3" sm="12">
				<v-autocomplete :items="dropdownItemsGames" v-model="selectTemplateFilter.gameID" clearable chips :label="'選擇遊戲'"> </v-autocomplete>
			</v-col>

			<v-col cols="12" md="6" sm="12">
				<v-btn color="yellow darken-1" text class="btn search mt-4-PC" @click.stop="whenSelectedTemplate()">
					<v-icon left>search</v-icon>
					選擇指定文案樣板 </v-btn
				>&#9;

				<v-btn color="blue darken-1" text class="btn search mt-4-PC" @click.stop="syncTGChatID()">
					<v-icon left>refresh</v-icon>
					同步TG新頻道系統ID (技術端) </v-btn
				>&#9;
			</v-col>
		</v-row>

		<div style="clear: both"></div>

		<div style="clear: both"></div>

		<v-card class="mt-4" v-if="tgContent != null && tgContent.templateID > 0 && isRenderContent">
			<v-card-text style="padding: 0px 25px">
				<v-row class="mb-3 mt-3">
					<div class="w100p">
						<h2 class="yellow-text"><v-icon color="yellow">mdi-table-edit</v-icon> Step 1. 填寫發送文案 & 預覽內容</h2>
					</div>

					<div style="clear: both"></div>

					<v-divider></v-divider>
				</v-row>

				<v-row>
					<v-col cols=" 12" md="5" sm="5">
						<div>
							<h2>填寫TG發送文案</h2>
							<v-divider></v-divider>

							<div>
								<v-text-field v-model="tgContent.bannerImageUrl" :label="'Banner圖片 (建議正方形圖片 且至少500*500 避免失真)'" style="width: 85%; display: inline-block">
								</v-text-field>

								<v-btn color="blue lighten-3" text class="btn search ml-3" @click.stop="showImageUpload()">
									<v-icon left>file_upload</v-icon>
									上傳 </v-btn
								>&#9;
							</div>

							<!-- <div>

							<v-text-field v-model="tgContent.title" :label="'粗體字標題'">

							</v-text-field>
						</div> -->

							<div class="textarea-bg">
								<v-textarea v-model="tgContent.content" :label="'發送內文'" rows="10" style="font-size: 14px; line-height: 20px"> </v-textarea>
							</div>

							<div v-if="tgContent.buttons != null && tgContent.buttons.length > 0">
								<h3>連結按鈕</h3>
							</div>

							<div v-for="(item, index) in tgContent.buttons" :key="index">
								<div>
									<v-checkbox v-model="item.isDisabled" :true-value="false" :false-value="true" :label="'啟用此按鈕'" style="width: 200px"></v-checkbox>

									<v-text-field v-model="item.text" :label="'按鈕文字'" :disabled="item.isDisabled" class="ml-4"> </v-text-field>

									<v-text-field v-model="item.linkUrl" :label="'按鈕連結'" :disabled="item.isDisabled" class="ml-4"> </v-text-field>
								</div>
							</div>
						</div>
					</v-col>

					<v-col cols="12" md="6" sm="6" class="preview-panel">
						<h2>發送內容預覽 (夜間模式)</h2>
						<v-divider></v-divider>

						<div v-if="tgContent != null && tgContent.templateID > 0" style="max-width: 450px" class="mt-4">
							<div style="background: #333; border-radius: 20px">
								<div style="position: relative; padding: 5px 10px 5px 10px">
									<span style="color: #f4a370" class="f13">pgs_bot</span>
									<span style="float: right; color: #aaa" class="f13 mt-1">Reply</span>
								</div>

								<img :src="tgContent.bannerImageUrl" v-if="!isEmptyString(tgContent.bannerImageUrl)" style="width: 100%" />

								<div class="tgContentPanel f14">
									<div v-html="toAnchorText(tgContent.content)" style="margin-bottom: 10px"></div>
									<div style="color: #aaa; font-size: 14px; position: absolute; right: 6px; bottom: 5px">下午 04:51</div>
								</div>
							</div>

							<div
								v-for="(item, index) in tgContent.buttons.filter((x) => !x.isDisabled)"
								:key="index"
								class="mt-1 tgButtonLink"
								@click="openLink(item.linkUrl)"
								style="
									position: relative;
									width: 100%;
									background: #333;
									border-radius: 10px;
									padding: 8px 10px;
									text-align: center;
									font-size: 14px;
									font-weight: 700;
									cursor: pointer;
								"
							>
								<v-icon style="position: absolute; right: 5px; top: 2px; font-size: 14px">mdi-arrow-top-right</v-icon>

								<div style="text-shadow: 1px 1px #111">
									{{ item.text }}
								</div>
							</div>

							<div class="mt-4 text-c">
								<v-btn color="pink lighten-3" text class="btn search mt-4" style="min-width: 400px" @click.stop="copySkypeMessage(tgContent.contentForSkype)">
									<v-icon left>file_copy</v-icon>
									Skype訊息複製 </v-btn
								>&#9;
							</div>
						</div>
					</v-col>
				</v-row>
			</v-card-text>
		</v-card>

		<v-card class="mt-4" v-if="tgContent != null && tgContent.templateID > 0 && isRenderContent">
			<v-card-text style="padding: 0px 25px">
				<v-row class="mb-3 mt-3">
					<div class="w100p">
						<h2 class="yellow-text"><v-icon color="yellow">mdi-table-edit</v-icon> Step 2. 選送發送頻道</h2>
					</div>

					<div style="clear: both"></div>
					<v-divider></v-divider>
				</v-row>

				<v-row class="">
					<v-col cols="12" md="4" sm="12">
						<div class="">
							<v-autocomplete
								:items="dropdownItemsTags"
								v-model="tgContent.sendTargetsByTagID"
								clearable
								class="w100p"
								:disabled="!(tgContent != null && tgContent.templateID > 0)"
								chips
								:label="'選擇發送哪些頻道(根據Tag標記)'"
							>
							</v-autocomplete>

							<div class="text-r minus-mt-2" v-if="tgContent.sendTargetsByTagID > 0">
								預計發送 <span class="red-text">{{ expectAnnouncementCount }}</span> 個頻道
							</div>
						</div>

						<div class="mt-4">
							<v-autocomplete
								:items="dropdownItemsIMChannels"
								v-model="tgContent.excludeChannelIDs"
								clearable
								multiple
								class="w100p"
								:disabled="!(tgContent != null && tgContent.templateID > 0)"
								small-chips
								:label="'排除特定頻道不發送'"
							>
							</v-autocomplete>
						</div>
					</v-col>

					<v-col cols="12" md="8" sm="12" style="margin-bottom: 250px">
						<v-btn color="green lighten-1" text class="btn search mt-4" @click.stop="sendTgNotify()" :disabled="tgContent.sendTargetsByTagID <= 0">
							<v-icon left>mdi-message-bulleted</v-icon>
							發送TG公告 </v-btn
						>&#9;

						<v-btn
							color="red lighten-1"
							text
							class="btn search mt-4"
							v-if="hasPermissionAllowToUseDevelopeFeature"
							@click.stop="removeLatestMessages()"
							:disabled="tgContent.sendTargetsByTagID <= 0"
						>
							<v-icon left>delete</v-icon>
							技術用-刪除最後一筆訊息(6小時內) </v-btn
						>&#9;

						<div style="display: inline-block" class="f12 orange-text ml-2 mt-4">
							<div><v-icon small color="orange">info</v-icon> 文案中若有連結。請優先以按鈕的形式提供</div>
							<div><v-icon small color="orange">info</v-icon> 原圖尺寸至少500*500 避免圖片失真</div>
							<div>
								<v-icon small color="orange">info</v-icon>
								TG／Skype　兩邊文案會不太一樣 (內文有連結or沒有)
							</div>
						</div>
					</v-col>
				</v-row>
			</v-card-text>
		</v-card>

		<v-dialog v-model="dialogVisible.isShowError" max-width="600px">
			<v-card>
				<v-card-title class="grey darken-2"> <v-icon color="red darken-3">close</v-icon> 提示 </v-card-title>

				<v-card-text class="mt-2">
					<div style="min-height: 300px">
						<div>⚠️發送完成，可能需留意以下注意事項 (或頻道)</div>
						<ul class="mt-2" style="padding-left: 30px">
							<li v-for="(text, index) in listOfErrorChannels" :key="index">
								{{ text }}
							</li>
						</ul>
					</div>
				</v-card-text>
			</v-card>
		</v-dialog>

		<ImageUploadDialog ref="refImageUploadDialog" @parentSetImageUrlAndClose="setImageUrlAfterUpload" />
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import * as Model from '@/models/interfaces/system-setting/accountingInvoice';
import * as CurrencyModel from '@/models/interfaces/system-setting/currency';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import * as EventModel from '@/models/interfaces/system-setting/events';
import AccountingInvoiceApi from '@/api/system-setting/accountingInvoice';
import CurrencyApi from '@/api/system-setting/currency';
import IMChannelApi from '@/api/system-setting/imChannel';
import DropdownApi from '@/api/commons/dropdown';
import InternalTestApi from '@/api/commons/internalTest';
import ImageUploadDialog from '@/components/system-setting/ImageUploadDialog.vue';

import DatePicker from 'vue2-datepicker';
import { Locale, Currency, Status, GameVendorType } from '@/models/status/common';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { commonMethodMixin } from '@/utilities/common-methods';
import { pageData, time } from '@/utilities/api-handler.lib';
import { datetimeMixin, startEndDate, timeWithTimezone, daysAgo, weeksAgo, monthsAgo, formatMixin } from '@/utilities/datetime-format';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import { State, Action, Getter, namespace } from 'vuex-class';
const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';
import { uuid } from 'vue-uuid';

// @ts-ignore
@Component({ components: { DatePicker, ImageUploadDialog }, mixins: [commonDisplayFiltersMixin, commonMethodMixin, datetimeMixin] })
export default class TGAnnouncement extends Vue {
	get hasExportInvoicePermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.System_AccountingInvoice_ExportInvoice) >= 0;
	}

	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];

	dialogVisible: DropdownModel.DialogVisible = new DropdownModel.DialogVisible();
	isSearchAndShowTable: boolean = false;
	renderComponent: boolean = true;
	isRenderContent: boolean = true;

	queryAdjustmentRulesAndAssignGroup: any = {};
	allCurrencyList: CurrencyModel.ICurrencyAllListResult = {} as CurrencyModel.ICurrencyAllListResult;
	searchAccountingListResult: Model.IAccountingInvoiceList = {} as Model.IAccountingInvoiceList;
	searchFilter: Model.IQueryAccountingInvoiceListParameter = {} as Model.IQueryAccountingInvoiceListParameter;

	tgContent: any = {};
	adminSysFavoritSetting: any = {};

	selectTemplateFilter: DropdownModel.ILazyCommonParameterReqeust = {} as DropdownModel.ILazyCommonParameterReqeust;

	DataTablePagingParameter: DropdownModel.DataTablePagingParameter = new DropdownModel.DataTablePagingParameter();
	datetimeRangePicker: DropdownModel.DatetimeRangePicker = new DropdownModel.DatetimeRangePicker();

	panel: number[] = [0];
	listOfErrorChannels: string[] = [];

	dropdownItemsIMChannels: DropdownModel.IDropdownItems[] = [];
	dropdownItemsTemplates: DropdownModel.IDropdownItems[] = [];
	dropdownItemsStatus: DropdownModel.IDropdownItems[] = [];
	dropdownItemsGames: DropdownModel.IDropdownItems[] = [];
	dropdownItemsTags: DropdownModel.IDropdownItems[] = [];

	@Action('Players/setEventID') private setEventID!: any;

	get hasPermissionAllowToUseDevelopeFeature() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.Special_AllowToUseDevelopeFeature) >= 0;
	}

	get expectAnnouncementCount() {
		if (this.tgContent.sendTargetsByTagID > 0) {
			const matchItem = this.dropdownItemsTags.filter((x) => x.valueInt === this.tgContent.sendTargetsByTagID);
			return matchItem[0].extraProperty.mappingCount;
		} else {
			return 0;
		}
	}

	refresh() {}

	closeAllModal() {
		this.dialogVisible.resetAllValueToFalse();
	}

	clear() {}

	openLink(linkUrl: string) {
		window.open(linkUrl, '_blank');
	}

	copySuccess() {
		EventBus.$emit(`successfully-copy`);
	}

	showImageUpload() {
		const targetRef: any = this.$refs.refImageUploadDialog;
		targetRef.showDialogAndEmptyValue(true);
	}

	setImageUrlAfterUpload(imageUrl: string) {
		this.tgContent.bannerImageUrl = imageUrl;
	}

	reloadComponent() {
		this.renderComponent = false;
		this.$nextTick(() => {
			this.renderComponent = true;
		});
	}

	reloadContentComponent() {
		this.isRenderContent = false;
		this.$nextTick(() => {
			this.isRenderContent = true;
		});
	}

	toAnchorText(content: string) {
		const regex = /(https?:\/\/[^\s]+)/g;
		let myResultText: string = content.replace(regex, '<a href="$1" target="_blank">$1</a>');

		// @ts-ignore
		myResultText = myResultText.replaceAll('_', '');
		return myResultText;
	}

	whenSelectedTemplate() {
		this.getContentByTemplateID();
	}

	getContentByTemplateID() {
		this.tgContent = {};
		IMChannelApi.getTGTemplate(this.selectTemplateFilter)
			.then((res) => {
				this.tgContent = res.data;
			})
			.finally(() => {
				this.setDefaultIgnoreChannels();
			});
	}

	setDefaultIgnoreChannels() {
		if (
			this.selectTemplateFilter.templateID === 2001 ||
			this.selectTemplateFilter.templateID === 2002 ||
			this.selectTemplateFilter.templateID === 2009 ||
			this.selectTemplateFilter.templateID === 5001
		) {
			this.tgContent.excludeChannelIDs = [];
			for (const itemID of this.adminSysFavoritSetting.skipSendNewGamePromotion_IMChannelIDs) {
				this.tgContent.excludeChannelIDs.push(itemID);
			}
		}
	}

	syncTGChatID() {
		if (confirm('此操作可能花費一些時間(同步ID)。是否執行?')) {
			EventBus.$emit('show_mask', null);
			IMChannelApi.syncTGChatID()
				.then((res) => {
					this.tgContent = res.data;
				})
				.finally(() => {
					EventBus.$emit('hide_mask', null);
				});
		}
	}

	removeLatestMessages() {
		if (confirm('將刪除最新一筆發送的訊息(N小時內)。是否執行?')) {
			EventBus.$emit('show_mask', null);

			const myBody = {
				tagID: this.tgContent.sendTargetsByTagID,
				size: 1,
			};

			IMChannelApi.removeLatestMessages(myBody)
				.then((res) => {
					this.tgContent = res.data;
				})
				.finally(() => {
					EventBus.$emit('hide_mask', null);
				});
		}
	}

	sendTgNotify() {
		if (confirm('預計將發送給' + this.expectAnnouncementCount + '個頻道、務必確認文案正確、連結填寫正確。發送後將無法再修改訊息。')) {
			EventBus.$emit('show_mask', null);

			// 需要用來偵測timeout異常 (SOC線路)
			console.log('tg send start---');
			console.log(+new Date());

			IMChannelApi.sendTGNotify(this.tgContent)
				.then((res) => {
					EventBus.$emit(`successfully-modified`);

					if (res.data != null && res.data.resultListText != null && res.data.resultListText.length > 0) {
						this.listOfErrorChannels = res.data.resultListText;
						this.dialogVisible.isShowError = true;
					}

					this.reloadContentComponent();
				})
				.finally(() => {
					EventBus.$emit('hide_mask', null);
					console.log('tg send end---');
					console.log(+new Date());
				});
		}
	}

	copySkypeMessage(skypeMessageByApi: string) {
		const vm: any = this;

		let myCopyText: string = '';

		if (vm.isEmptyString(skypeMessageByApi)) {
			myCopyText = myCopyText + this.tgContent.content + '\n';

			// @ts-ignore
			myCopyText = myCopyText.replaceAll('_', '');

			if (!vm.isEmptyString(this.tgContent.basicGameImageUrl)) {
				myCopyText = myCopyText + '\n' + 'Image (游戏图片)' + '\n';
				myCopyText = myCopyText + this.tgContent.basicGameImageUrl + '\n';
			}

			const myButtons = this.tgContent.buttons.filter((x: any) => !x.isDisabled);

			if (myButtons != null && myButtons.length > 0) {
				for (const item of myButtons) {
					myCopyText = myCopyText + '\n' + item.text + '\n';
					myCopyText = myCopyText + item.linkUrl + '\n';
				}
			}
		} else {
			myCopyText = skypeMessageByApi;
		}

		myCopyText = myCopyText + '\n' + '\n' + '請幫忙通知Skype客戶 (Skype Only)' + '\n' + '@CS1 @CS2 ' + '\n' + '\n';

		if (this.selectTemplateFilter.gameID >= 290000 && this.selectTemplateFilter.gameID < 299999) {
			myCopyText = myCopyText + '⚠️以下頻道不需通知' + '\n';
			myCopyText = myCopyText + 'F11 - Crazy Fishing 2 / Bird Hunter Support Group' + '\n';
			myCopyText = myCopyText + 'Pegasus X 998 / GH78' + '\n';

			if (this.selectTemplateFilter.templateID === 2001 || this.selectTemplateFilter.templateID === 2002) {
				myCopyText = myCopyText + 'UFA - Pegasus' + '\n';
				myCopyText = myCopyText + '所有ION頻道也不需要通知' + '\n';
			} else if (this.selectTemplateFilter.templateID === 5001) {
				myCopyText = myCopyText + 'UFA - Pegasus' + '\n';
				myCopyText = myCopyText + '所有ION頻道不需通知' + '\n';
			}
		}

		vm.copyTextToClipboard(myCopyText);
	}

	mounted() {
		this.refresh();

		DropdownApi.getNormalStatus().then((res) => {
			this.dropdownItemsStatus = res;
		});

		DropdownApi.getGames().then((res) => {
			this.dropdownItemsGames = res;
		});

		DropdownApi.getGeneralTags(50000).then((res) => {
			this.dropdownItemsTags = res;
		});

		DropdownApi.getTGAnnouncementTemplates().then((res) => {
			this.dropdownItemsTemplates = res;
		});

		DropdownApi.getIMChannels().then((res) => {
			this.dropdownItemsIMChannels = res;
		});

		InternalTestApi.getAdminSysFavoritSetting().then((res) => {
			this.adminSysFavoritSetting = res.data;
		});
	}
}
</script>

<style scoped>
.myCategoryColor_0 {
	color: #63ae6d;
}

.tgButtonLink:hover {
	opacity: 0.85;
}

.maxHeightOfRow {
	height: 80px !important;
	max-height: 80px !important;
}

h2 {
	margin-bottom: 10px;
}

.preview-panel {
	padding-left: 50px;
}

@media only screen and (max-width: 480px) {
	.preview-panel {
		padding-left: 10px !important;
	}
}
</style>
