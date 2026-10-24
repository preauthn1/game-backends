<template>

	<div class="event-theme--dark">

		<v-card v-if="searchResult && searchResult.eventID > 0"
			style="min-height:87vh;overflow-x: hidden;">

			<div class="text-r">

				<div style="padding:10px 30px;height:100px;">

					<div style="float:left;margin-top:15px;">
						{{searchResult.eventName}} <span class="f12 grey-text">
							({{searchResult.eventID}})</span>
					</div>

					<div v-if="searchResult.isUseOtherEventContent">

						<h3 class="mt-4">
							已設定引用其他活動文案: <span class="yellow-text">{{searchResult.referenceContentEventID}}</span>
						</h3>

						<div class="mt-2">

							<a :href="searchResult.productionUrl" target="_blank" class="mr-4 link-text">
								真實網址
							</a>

							<a :href="searchResult.previewUrl" target="_blank" class="mr-4 link-text">
								預覽網址
							</a>

							<v-btn color="grey lighten-1" @click="closeAllModal" class="btn button-font-14 mr-3">
								{{ $t('Button.__cancel') }}
							</v-btn>

						</div>

					</div>

					<div v-else-if="!searchResult.isNotInitial">

						<a :href="searchResult.productionUrl" target="_blank" class="mr-4 link-text-grey">
							真實網址
						</a>

						<a :href="searchResult.previewUrl" target="_blank" class="mr-4 link-text-grey">
							預覽網址
						</a>

						<v-btn color="grey lighten-1" x-large @click="closeAllModal"
							class="btn button-font-14 mr-3">

							{{ $t('Button.__cancel') }}
						</v-btn>

						<v-btn color="red lighten-1" x-large @click="saveEvent" class="btn button-font-14">

							{{ $t('Button.__save') }}
						</v-btn>
					</div>

					<div v-else-if="searchResult.isNotInitial">

						<v-btn color="grey lighten-1" x-large @click="closeAllModal"
							class="btn button-font-14 mr-3">

							{{ $t('Button.__cancel') }}
						</v-btn>

						<v-btn color="orange lighten-1" x-large @click="initialDefaultContent"
							class="btn button-font-14 mr-3">
							初始化預設版型
						</v-btn>

					</div>

				</div>

			</div>

			<!-- <區塊設置 -->
			<v-row style="padding:5px 30px 5px 30px;    margin-top: -40px !important;"
				v-if="searchResult.eventContentBlocks && searchResult.eventContentBlocks.length > 0">

				<v-col cols="12" sm="12" md="12">

					<v-autocomplete :items="colorTypeDropdownItems" v-model="searchResult.colorStyleType"
						:label="'版型色系'">

					</v-autocomplete>

				</v-col>

				<v-col cols="12" sm="12" md="12" class="advanceSettingPanel"
					style="padding: 20px 5px 5px 10px;height:77vh;overflow-y: scroll;overflow-x: hidden;border-bottom:solid 3px #666;">

					<div class="text-r mb-2">

						<v-tooltip top color="orange darken-4">
							<template v-slot:activator="{ on, attrs }">
								<span v-bind="attrs" v-on="on" class="f14"
									style="color: #CCC;text-decoration: underline;">
									<v-icon color="grey" small>info</v-icon> 欄位說明(排行榜)
								</span>
							</template>
							<span>

								<ul>
									<li>區塊【標題置頂大圖】 = Banner圖</li>
									<li>區塊【注意事項】 = 活動規則</li>
								</ul>

							</span>
						</v-tooltip>

					</div>

					<v-expansion-panels>
						<v-expansion-panel
							v-for="(eachEventContentBlock , index) in searchResult.eventContentBlocks"
							:key="index">

							<v-expansion-panel-header>
								<!-- <v-chip class="mr-3" color="purple darken-2" style="width:150px;">
									
								</v-chip> -->

								<span>

									<v-icon class="mr-1">
										assignment
									</v-icon>
									{{eachEventContentBlock.layoutTypeText}}
								</span>
							</v-expansion-panel-header>
							<v-expansion-panel-content>

								<!-- <div v-for="(eachEventContentBlock , index) in searchResult.eventContentBlocks"
						style="margin-bottom:50px;" :key="index"> -->

								<div style="clear:both"></div>
								<div style="font-size: 20px;">
									<!-- 
							<v-icon class="mr-2" color="grey">
								assignment
							</v-icon> -->

									<!-- <v-chip class="mr-3" color="purple darken-2">
										{{eachEventContentBlock.layoutTypeText}}
									</v-chip> -->

									<v-text-field v-model="eachEventContentBlock.memo" :label="'自訂備註訊息'"
										style="display:inline-block;width:250px;">
									</v-text-field>

									<v-autocomplete v-model="eachEventContentBlock.status" :label="'顯示設定'"
										class="ml-3" :items="visibleTypeDropdownItems"
										style="display:inline-block;width:300px;">
									</v-autocomplete>

								</div>

								<div>

								</div>

								<div>

									<div v-for="(item , index) in eachEventContentBlock.contents" :key="index"
										class="eachLangContent">

										<div class="mt-4">
											<v-chip color="">
												{{item.languageText}}
											</v-chip>
											<v-chip v-if="item.isDefault">
												預設語系
											</v-chip>
										</div>

										<div
											style="padding:15px;border: dashed 1px #707070;margin-top: 10px; margin-bottom: 20px;margin-left: 15px;">

											<div v-show="isShowTitleEditor(eachEventContentBlock.layoutType)">
												<v-text-field v-model="item.title" :label="$t('TableHeader.__title')">
												</v-text-field>
											</div>

											<div v-if="isShowDescriptionEditor(eachEventContentBlock.layoutType)">

												<div>
													{{$t('TableHeader.__description')}}
												</div>
												<tiptap-vuetify v-model="item.description" :extensions="extensions"
													class="my-tiptap" />

												<!-- <v-textarea v-model="item.description"
													:label="$t('TableHeader.__description')"
													:placeholder="$t('TableHeader.__description')">
												</v-textarea> -->
											</div>

											<div v-if="eachEventContentBlock.layoutType == 21">

												<v-text-field v-model="item.imageUrlMiddle" :label="'圖片網址'"
													style="width:85%;display: inline-block;">
												</v-text-field>

												<v-btn color="blue lighten-3" text class="btn search ml-3"
													@click.stop="showImageUpload">
													<v-icon left>file_upload</v-icon>
													上傳
												</v-btn>&#9;

												<div
													v-if="item.imageUrlMiddle && item.imageUrlMiddle != null && item.imageUrlMiddle.length > 0">

													<a :href="item.imageUrlMiddle" target="_blank" title="open link"
														class="hover08 previewImage">
														<img :src="item.imageUrlMiddle" />
													</a>
												</div>

											</div>

											<div v-if="eachEventContentBlock.layoutType != 21">

												<v-text-field v-model="item.imageUrlTop" :label="'圖片網址(上方)'"
													style="width:85%;display: inline-block;">
												</v-text-field>

												<v-btn color="blue lighten-3" text class="btn search ml-3"
													@click.stop="showImageUpload">
													<v-icon left>file_upload</v-icon>
													上傳
												</v-btn>&#9;

												<div
													v-if="item.imageUrlTop && item.imageUrlTop != null && item.imageUrlTop.length > 0">

													<a :href="item.imageUrlTop" target="_blank" title="open link"
														class="hover08 previewImage">
														<img :src="item.imageUrlTop" />
													</a>
												</div>

												<div style="clear:both"></div>
											</div>

											<div v-if="eachEventContentBlock.layoutType != 21">

												<v-text-field v-model="item.imageUrlBottom" :label="'圖片網址(下方)'"
													style="width:85%;display: inline-block;">
												</v-text-field>

												<v-btn color="blue lighten-3" text class="btn search ml-3"
													@click.stop="showImageUpload">
													<v-icon left>file_upload</v-icon>
													上傳
												</v-btn>&#9;

												<div
													v-if="item.imageUrlBottom && item.imageUrlBottom != null && item.imageUrlBottom.length > 0">

													<a :href="item.imageUrlBottom" target="_blank" title="open link"
														class="hover08 previewImage">
														<img :src="item.imageUrlBottom" />
													</a>
												</div>

												<div style="clear:both"></div>
											</div>

											<!-- <div>

										<v-autocomplete v-model="item.linkType" :items="linkTypeDropdownItems"
											style="width:400px;" @change="resetValue(item)" chips :label="'連結類型'">
										</v-autocomplete>

									</div>

									<div>

										<v-text-field v-model="item.linkReference" :label="'連結網址'"
											v-if="item.linkType == 99">
										</v-text-field>

										<v-autocomplete v-model="item.linkReference" :label="'遊戲ID'"
											style="width:400px;" :items="gamesIdTextDropdownItems"
											v-if="item.linkType == 21">
										</v-autocomplete>

										<v-text-field v-model="item.linkReference" :label="'連結網址'"
											v-if="item.linkType == 0" disabled='disabled'>
										</v-text-field>

									</div> -->

										</div>
									</div>

								</div>
								<!-- 
	</div> -->
							</v-expansion-panel-content>

						</v-expansion-panel>
					</v-expansion-panels>

				</v-col>
			</v-row>

		</v-card>

		<ImageUploadDialog ref="refImageUploadDialog" />

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
import DropdownApi from '@/api/commons/dropdown';
import DatePicker from 'vue2-datepicker';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { commonMethodMixin } from '@/utilities/common-methods';
import NotificationWarning from '@/components/notification/NotificationWarning.vue';
import ImageUploadDialog from '@/components/system-setting/ImageUploadDialog.vue';
import { pageData, time } from '@/utilities/api-handler.lib';
import { datetimeMixin, startEndDate } from '@/utilities/datetime-format';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import { Locale, Currency, Status, GameType, EventType, GameVendorType } from '@/models/status/common';

// @ts-ignore
import { TiptapVuetify, Heading, Bold, Italic, Strike, Underline, Code, BulletList, OrderedList, ListItem, Image, Link, Blockquote, HorizontalRule } from 'tiptap-vuetify';
import 'tiptap-vuetify/dist/main.css';

import { State, Action, Getter, namespace } from 'vuex-class';
const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';
const playersModule = namespace('Players');
import { uuid } from 'vue-uuid';

// @ts-ignore
@Component({ components: { NotificationWarning, DatePicker, ImageUploadDialog, TiptapVuetify }, mixins: [commonDisplayFiltersMixin, commonMethodMixin, datetimeMixin] })
export default class EventContentBlock extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];

	renderComponent: boolean = true;
	searchResult: EventModel.IEventContentBlockEditModelResponse = {} as EventModel.IEventContentBlockEditModelResponse;

	extensions: any = [
		Bold,
		Underline,
		Strike,
		Italic,
		ListItem,
		BulletList,
		OrderedList,
		Image,
		HorizontalRule,
		[
			Heading,
			{
				options: {
					levels: [1, 2, 3, 4, 5],
				},
			},
		],
	];

	// starting editor's content
	content: string = '<h1>Yay Headlines!</h1>';

	panel: number[] = [0];

	organizationsDropdownItems: DropdownModel.IDropdownItems[] = [];
	gamesDropdownItems: DropdownModel.IDropdownItems[] = [];
	visibleTypeDropdownItems: DropdownModel.IDropdownItems[] = [];
	colorTypeDropdownItems: DropdownModel.IDropdownItems[] = [];

	get hasPermissionCreate() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSettingMerchant_Create) >= 0;
	}
	get hasPermissionUpdate() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSettingMerchant_Update) >= 0;
	}

	search() {
		this.refresh();
	}

	getEventContentBlock(eventID: number) {
		EventBus.$emit('show_mask', null);

		this.searchResult = {} as EventModel.IEventContentBlockEditModelResponse;
		EventsApi.getEventContentBlock(eventID)
			.then((res) => {
				this.searchResult = res.data;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	refresh() {
		this.registerReloadComponent();

		this.searchResult = {} as EventModel.IEventContentBlockEditModelResponse;
	}

	saveEvent() {
		EventBus.$emit('show_mask', null);
		EventsApi.updateEventContentBlock(this.searchResult)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	isShowTitleEditor(layoutType: number) {
		return layoutType !== 71;
	}

	isShowDescriptionEditor(layoutType: number) {
		return layoutType !== 21;
	}

	registerReloadComponent() {
		EventBus.$on(`reload-${this.$router.currentRoute.path}`, (info: any) => {
			this.renderComponent = false;
			this.$nextTick(() => {
				this.renderComponent = true;
			});
		});
	}

	initialDefaultContent() {
		EventBus.$emit('show_mask', null);
		EventsApi.initialContentBlock(this.searchResult.eventID)
			.then((res) => {
				this.getEventContentBlock(this.searchResult.eventID);
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	closeAllModal() {
		this.$emit('closeParentAllModal');
	}

	showImageUpload() {
		const targetRef: any = this.$refs.refImageUploadDialog;
		targetRef.showDialogAndEmptyValue();
	}

	init() {
		DropdownApi.getGames().then((res) => {
			this.gamesDropdownItems = res;
		});

		DropdownApi.getVisibilityStatus().then((res) => {
			this.visibleTypeDropdownItems = res;
		});

		DropdownApi.getEventContentColorType().then((res) => {
			this.colorTypeDropdownItems = res;
		});
	}

	mounted() {
		this.init();
	}
}
</script>


<style>
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
	margin-top: 10px;
	margin-bottom: 20px;
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

.border-square {
	border-radius: 8px !important;
}

.extraTagTitle {
	color: #ccc;
}

.previewImage {
	font-size: 14px;
	float: right;
	margin-top: -15px;
	z-index: 999;
	position: relative;
}

.previewImage img {
	height: 40px;
}

.eachLangContent {
	padding: 3px 3px 3px 35px;
}
</style>