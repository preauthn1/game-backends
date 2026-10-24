<template>
	<v-app v-if="renderComponent" class="app">
		<v-card class="" id="refSlotHistory" ref="refSlotHistory">
			<v-card-title>
				<div class="top-right-item" v-if="isShowCloseButton">
					<div class="close" @click="closeSlotHistory">
						<v-icon>close</v-icon>
					</div>
				</div>
			</v-card-title>
			<v-card-text>
				<div class="text-c">
					<h2>target {{ $t('TableHeader.__gameResult') }}</h2>

					<div style="padding: 3px; color: #e2bd56">
						{{ slotHistory.gameName }}
					</div>
				</div>

				<v-row>
					<v-col cols="12" sm="1" md="1"> </v-col>

					<v-col cols="12" sm="12" md="12">
						<div style="display: flex">
							<div class="linePreview" v-for="(item, index) in slotHistory.lineResult" v-bind:key="index">
								<div v-for="(eachIcon, index2) in item" class="parentWarpper" v-bind:key="index2">
									<div v-if="isEmptyString(eachIcon.imageUrl)" @click="showCreateImageDialog(slotHistory.gameID, eachIcon.code, eachIcon.imageUrl)" class="emptyImage">
										<div class="innerText">
											{{ eachIcon.code }}
										</div>
									</div>

									<div v-if="!isEmptyString(eachIcon.imageUrl)" class="imageWrapper">
										<img :src="eachIcon.imageUrl" class="slotsIcon" :title="eachIcon.code" @click="showCreateImageDialog(slotHistory.gameID, eachIcon.code, eachIcon.imageUrl)" />
									</div>
								</div>
							</div>
						</div>
					</v-col>
				</v-row>

				<div style="clear: both"></div>
				<div style="text-align: right; padding: 2px 15px; color: #dd8932">
					<div v-if="slotHistory.isMultipleSlotsResult">*此資料有多個盤面結果，僅顯示最後一筆盤面</div>
				</div>

				<v-row>
					<v-col cols="12" sm="1" md="1"> </v-col>
					<v-col cols="12" sm="5" md="5">
						<div class="item">
							<div class="targetHistoryLabel">{{ $t('TableHeader.__betRecordNo') }}</div>
							<div class="targetHistoryValue">
								{{ slotHistory.perGameDetailLogID }}
								<span style="color: #9e9e9e; font-size: 10px; margin-left: 5px"> ({{ slotHistory.slotGameHistoryLogID }}) </span>
							</div>
						</div>
					</v-col>

					<v-col cols="12" sm="5" md="5">
						<div class="item">
							<div class="targetHistoryLabel">{{ $t('TableHeader.__perGameLogID') }}</div>
							<div class="targetHistoryValue">
								{{ slotHistory.perGameLogID }}
							</div>
						</div>
					</v-col>

					<v-col cols="12" sm="1" md="1"> </v-col>
				</v-row>

				<v-row>
					<v-col cols="12" sm="1" md="1"> </v-col>

					<v-col cols="12" sm="5" md="5">
						<div class="item">
							<div class="targetHistoryLabel">{{ $t('TableHeader.__betRecordNoPGS') }}</div>
							<div class="targetHistoryValue">
								{{ slotHistory.perGameTransactionNo }}
							</div>
						</div>
					</v-col>

					<v-col cols="12" sm="5" md="5">
						<div class="item">
							<div class="targetHistoryLabel">{{ $t('TableHeader.__playerAccount') }}</div>
							<div class="targetHistoryValue">{{ slotHistory.playerName }} ({{ slotHistory.playerID }})</div>
						</div>
					</v-col>

					<v-col cols="12" sm="1" md="1"> </v-col>
				</v-row>

				<v-row>
					<v-col cols="12" sm="1" md="1"> </v-col>

					<v-col cols="12" sm="5" md="5">
						<div class="item">
							<div class="targetHistoryLabel">{{ $t('TableHeader.__betAmount') }}</div>
							<div class="targetHistoryValue">
								{{ slotHistory.betAmount }}
							</div>
						</div>
					</v-col>

					<v-col cols="12" sm="5" md="5">
						<div class="item">
							<div class="targetHistoryLabel">{{ $t('TableHeader.__updateAmount') }}</div>
							<div class="targetHistoryValue">
								{{ slotHistory.updateAmount }}
							</div>
						</div>
					</v-col>

					<v-col cols="12" sm="1" md="1"> </v-col>
				</v-row>

				<v-row v-if="!slotHistory.isSingleWalletMode">
					<v-col cols="12" sm="1" md="1"> </v-col>

					<v-col cols="12" sm="5" md="5">
						<div class="item">
							<div class="targetHistoryLabel">{{ $t('TableHeader.__currentAmount') }}</div>
							<div class="targetHistoryValue">
								{{ slotHistory.beforeAmount }}
							</div>
						</div>
					</v-col>

					<v-col cols="12" sm="5" md="5">
						<div class="item">
							<div class="targetHistoryLabel">{{ $t('TableHeader.__afterAmount') }}</div>
							<div class="targetHistoryValue">
								{{ slotHistory.afterAmount }}
							</div>
						</div>
					</v-col>

					<v-col cols="12" sm="1" md="1"> </v-col>
				</v-row>

				<v-row>
					<v-col cols="12" sm="1" md="1"> </v-col>

					<v-col cols="12" sm="5" md="5">
						<div class="item">
							<div class="targetHistoryLabel">{{ $t('TableHeader.__taxAmount') }}</div>
							<div class="targetHistoryValue">
								{{ slotHistory.taxAmount }}
							</div>
						</div>
					</v-col>

					<v-col cols="12" sm="5" md="5">
						<div class="item">
							<div class="targetHistoryLabel">{{ $t('TableHeader.__time') }}</div>
							<div class="targetHistoryValue">
								{{ toDisplayTime(slotHistory.gameTime) }}
							</div>
						</div>
					</v-col>

					<v-col cols="12" sm="1" md="1"> </v-col>
				</v-row>

				<div style="clear: both; padding-bottom: 30px"></div>

				<div style="clear: both; padding-bottom: 30px"></div>
			</v-card-text>
		</v-card>

		<v-card>
			<v-card-text>
				<div style="">
					<div class="text-c" v-if="isShowCopyBtn">
						<v-btn @click="captureScreenshot(false)"> 擷取畫面 </v-btn>

						<v-btn @click="captureScreenshot(true)" style="margin-left: 10px"> 擷取畫面(產生連結) </v-btn>
					</div>
				</div>

				<div v-if="hasAdminPermission">
					<div class="text-c" style="margin-top: 70px">
						<v-spacer></v-spacer>

						<h2>圖片管理 (管理員專用)</h2>

						<v-spacer></v-spacer>
					</div>

					<v-row>
						<v-col cols="12" sm="1" md="1"> </v-col>

						<v-col cols="12" sm="9" md="9">
							<v-row>
								<v-col v-for="(item, index) in slotHistory.distinctIcons" style="text-align: center" cols="12" md="6" v-bind:key="index">
									<div v-if="isEmptyString(item.imageUrl)" style="height: 60px; width: 60px; vertical-align: bottom" class="emptyImage">
										<div class="innerText s-size">{{ item.code }}</div>
									</div>

									<img v-if="!isEmptyString(item.imageUrl)" :src="item.imageUrl" style="height: 60px; vertical-align: bottom" />

									<span style="padding: 0px 15px">
										{{ item.code }}
									</span>

									<v-btn @click="showCreateImageDialog(slotHistory.gameID, item.code, item.imageUrl)"> 圖片管理 </v-btn>
								</v-col>
							</v-row>
							<div></div>
						</v-col>
					</v-row>

					<div style="clear: both"></div>
				</div>
			</v-card-text>
		</v-card>

		<GamePayTableImageCreate ref="gamePayTableImageCreate" @updateSuccess="reloadDataAfterUpdateImage"> </GamePayTableImageCreate>
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import { State, Action, Getter, namespace } from 'vuex-class';
const playersModule = namespace('Players');
import EventBus from '@/utilities/event-bus';
import { datetimeMixin, startEndDate, timeWithTimezone, daysAgo, weeksAgo, monthsAgo, formatMixin } from '@/utilities/datetime-format';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { commonMethodMixin } from '@/utilities/common-methods';
import * as Model from '@/models/interfaces/game-management/per-game';
import GameManagementApi from '@/api/game-management/per-game';
import DropdownApi, { convertLang } from '@/api/commons/dropdown';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import DatePicker from 'vue2-datepicker';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import * as Permission from '@/models/status/permission';
import GamePayTableImageCreate from '@/components/system-setting/GamePayTableImageCreate.vue';
import axios, { AxiosResponse, AxiosRequestConfig } from 'axios';
import eazyFormApi from '@/api/commons/eazyFormApi';

const authModule = namespace('Auth');

// @ts-ignore
@Component({ mixins: [datetimeMixin, commonDisplayFiltersMixin, formatMixin, commonMethodMixin], components: { DatePicker, GamePayTableImageCreate } })
export default class List extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];

	@authModule.State('defaultLangTimes') defaultLangTimes!: number;
	@playersModule.State('gamePlayerID') gamePlayerID!: number;
	@playersModule.State('gameID') gameID!: number;
	@playersModule.State('serverID') serverID!: number;
	@playersModule.State('perGameLogID') perGameLogID!: number;

	isShowCloseButton: boolean = true;
	renderComponent: boolean = true;
	panel: number[] = [0];
	totalCount: number = 0;
	pageIndex: number = 1;
	itemsPerPage: number = 10;
	gameStartTimeRange: number[] = [];
	slotHistory: Model.ISlotHistoryDetailLogResponse = {} as Model.ISlotHistoryDetailLogResponse;

	gamesDropdownItemsTemp: DropdownModel.IDropdownItems[] = [];
	gamesDropdownItems: DropdownModel.IDropdownItems[] = [];
	serversDropdownItems: DropdownModel.IDropdownItems[] = [];
	quicklyPickTimeRangeText: string = '';
	currentTransactionNo: string = '';

	isShowSlotHistory: boolean = false;
	isShowExternalLinkDialog: boolean = false;
	playerID: number | null = null;
	playerName: string = '';
	copyText: string = '';
	result: Model.IListResponse[] = [];
	targetPopupPerGameDetailLog: Model.IListResponse = {} as Model.IListResponse;
	currentTime: string = '';
	isShowCopyBtn: boolean = true;

	downloading: boolean = false;

	get hasAdminPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.GamesManagementAdminPermission) >= 0;
	}

	geti18nTextAsString(key: string) {
		return this.$t(key).toString();
	}

	closeExternalLinkDialog() {
		this.isShowExternalLinkDialog = false;
	}

	closeSlotHistory() {
		this.isShowSlotHistory = false;
		try {
			this.$emit('closeSlotHistory');
		} catch {}
	}

	showSlotHistoryInfo(transactionNo: string) {
		this.isShowSlotHistory = true;
		this.currentTransactionNo = transactionNo;
		this.getSlotHistoryInfo(transactionNo);
	}

	getSlotHistoryInfo(transactionNo: string) {
		this.slotHistory = {} as Model.ISlotHistoryDetailLogResponse;
		GameManagementApi.getSlotHistoryByTransactionNo(transactionNo).then((res) => {
			this.slotHistory = res.data;
		});
	}

	showCreateImageDialog(gameID: number, code: string, imageUrl: string) {
		if (!this.hasAdminPermission) {
			return;
		}

		const targetRef: any = this.$refs.gamePayTableImageCreate;
		targetRef.showCreateDefaultDialog(gameID, code, imageUrl);
	}

	reloadDataAfterUpdateImage() {
		this.getSlotHistoryInfo(this.currentTransactionNo);
	}

	toDisplayTime(isoTime: string) {
		return timeWithTimezone(isoTime, this.userProfile.timeZone);
	}

	async captureScreenshot(isGenerateRemoteUrl: boolean) {
		const el = this.$refs.refSlotHistory;

		// add option type to get the image version
		// if not provided the promise will return
		// the canvas.
		const options = {
			logging: true,
			allowTaint: false,
			letterRendering: 1, // allowTaint: false
			useCORS: true,
		};

		const vm: any = this;

		EventBus.$emit('show_mask', null);

		const elementID = document.getElementById('refSlotHistory');

		vm.$html2canvas(elementID, options).then((canvas: any) => {
			const imgData = canvas.toDataURL('image/jpeg');

			if (isGenerateRemoteUrl) {
				this.uploadBase64ImageToRemote(imgData);
			} else {
				EventBus.$emit('hide_mask', null);
				const windows: any = window.open('');
				const image = new Image();
				image.src = imgData;

				windows.document.write(image.outerHTML);
			}
		});

		setTimeout(() => {
			EventBus.$emit('hide_mask', null);
		}, 3500);
	}

	async uploadBase64ImageToRemote(base64Text: string) {
		const hashResult = await eazyFormApi.getHashKeyByServerTime();

		const formData = {
			base64: base64Text,
			signHash: hashResult.data.hash,
		};

		axios
			.post(process.env.VUE_APP_FILE_API + 'upload/images/v2/base64', formData)
			.then((res: any) => {
				if (res.data.file_path) {
					window.open(res.data.file_path);
				}
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	mounted() {
		this.init();

		const route: any = this.$route;
	}

	copySuccess() {
		EventBus.$emit(`successfully-copy`);
	}

	init() {}

	clear() {}

	registerReloadComponent() {
		EventBus.$on(`reload-${this.$router.currentRoute.path}`, (info: any) => {
			this.renderComponent = false;
			this.$nextTick(() => {
				this.clear();
				this.result = [];
				this.renderComponent = true;
			});
		});
	}
}
</script>

<style lang="scss" scoped>
.container--fluid {
	margin-right: 5px !important;
}

.targetHistoryLabel {
	margin-top: 10px;
	color: #888;
}

.targetHistoryValue {
	border-bottom: dashed 1px #888;
	font-size: 16px;
	padding: 2px 0px;
}

.emptyImage {
	cursor: pointer;
	display: inline-block;
	border: dashed 2px #777;
	text-align: center;
	position: relative;
	width: 120px;
	height: 120px;
}

.innerText {
	font-size: 20px;
	position: absolute;
	top: 0;
	width: 120px;
	height: 20px;
	right: 0;
	bottom: 0;
	left: 0;
	margin: auto;
}

.innerText.s-size {
	font-size: 14px;
	width: 60px;
}

.linePreview {
	flex: 2;
	text-align: center;
}

.imageWrapper {
	width: 120px;
	margin: 0 auto;
	overflow: hidden;
	text-align: center;
	position: relative;
	height: 120px;
}

.slotsIcon {
	height: 120px;
	min-width: 100px;
	max-width: none;
	margin: 0 -100%;
}

.parentWarpper {
	display: inline-block;
	height: 125px;
}

.operate_icons .v-icon {
	font-size: 20px;
	margin-left: 4px;
	color: #ddd;
}
</style>
