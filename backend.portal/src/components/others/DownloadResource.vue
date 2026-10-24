<template>
	<v-app v-if="isRenderComponent" class="">
		<v-row>
			<v-col cols="12" sm="6" md="6">
				<v-card v-if="isLoad">
					<v-card-text style="padding-bottom: 100px">
						<div v-if="hasApiDocumentPermission">
							<h2 class="white-text">
								<v-icon class="mr-1">cloud_download</v-icon>
								API document (对接文档下载)
							</h2>

							<div class="">
								<div class="mt-4">
									<h4 class="">Transfer (转帐钱包)</h4>
								</div>

								<div>
									<span class="f14 link-text" @click="downloadPDF(1, 'en-us')">
										{{ searchCommonInfo.fileName_Transfer_EN }}
									</span>
								</div>
								<div>
									<span class="f14 link-text" @click="downloadPDF(1, 'zh-cn')">
										{{ searchCommonInfo.fileName_Transfer_CN }}
									</span>
								</div>
								<div class="mt-4">
									<h4 class="">Seamless (无缝钱包)</h4>
								</div>

								<div>
									<span class="f14 link-text" @click="downloadPDF(2, 'en-us')">
										{{ searchCommonInfo.fileName_Seamless_EN }}
									</span>
								</div>
								<div>
									<span class="f14 link-text" @click="downloadPDF(2, 'zh-cn')">
										{{ searchCommonInfo.fileName_Seamless_CN }}
									</span>
								</div>
							</div>
						</div>

						<div v-if="hasApiDocumentPermission" style="margin-top: 50px">
							<h2 class="white-text">
								<v-icon class="mr-1">cloud_download</v-icon>
								Game list (最新游戏清单)
							</h2>

							<div class="">
								<div class="mt-0">
									<span class="f14 link-text" @click="downloadGameList()"> GameList (Excel) </span>
								</div>
							</div>
						</div>

						<div v-if="hasApiDocumentPermission" style="margin-top: 50px">
							<h2 class="white-text">
								<v-icon class="mr-1">cloud_download</v-icon>
								BO operate guide (BO操作教学)
							</h2>

							<div class="">
								<div class="mt-0">
									<a class="f14 link-text" target="_blank" :href="BO_Guid_PDF_URL()"> BO_Guide </a>
								</div>
							</div>
						</div>
					</v-card-text>
				</v-card>
			</v-col>

			<v-col cols="12" sm="6" md="6"> </v-col>
		</v-row>
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import * as Model from '@/models/interfaces/operator-management/BOAnnouncement';
import * as UserModel from '@/models/interfaces/operator-management/users';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import * as EventModel from '@/models/interfaces/system-setting/events';
import * as MerchantCreditModel from '@/models/interfaces/agent/merchantCredit';
import ImageUploadDialog from '@/components/system-setting/ImageUploadDialog.vue';
import EventsApi from '@/api/system-setting/events';
import MerchantCreditApi from '@/api/agent/merchantCredit';
import MerchantApi from '@/api/system-setting/merchants';
import BOAnnouncementApi from '@/api/operator-management/BOAnnouncement';
import DropdownApi from '@/api/commons/dropdown';
import DatePicker from 'vue2-datepicker';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { commonMethodMixin } from '@/utilities/common-methods';
import NotificationWarning from '@/components/notification/NotificationWarning.vue';
import { pageData, time } from '@/utilities/api-handler.lib';
import { datetimeMixin, startEndDate, timeWithTimezone, daysAgo, weeksAgo, monthsAgo, formatMixin } from '@/utilities/datetime-format';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import { State, Action, Getter, namespace } from 'vuex-class';
const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';
import InternalTestApi from '@/api/commons/internalTest';
import { uuid } from 'vue-uuid';

// @ts-ignore
@Component({ components: { NotificationWarning, DatePicker, ImageUploadDialog }, mixins: [commonDisplayFiltersMixin, commonMethodMixin, datetimeMixin] })
export default class DownloadResource extends Vue {
	get hasPermissionToCreate() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.Other_BOAnnouncement_CreateUpdate) >= 0;
	}

	get hasSuperPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSetting_SystemConfigSetting) >= 0;
	}

	get hasApiDocumentPermission() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.Other_DownloadResource_ApiDocument) >= 0;
	}

	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];

	dialogVisible: DropdownModel.DialogVisible = new DropdownModel.DialogVisible();

	isShowPreview: boolean = false;
	isRenderComponent: boolean = true;
	isOrgDropdownInvisible: boolean = false;
	isLoad: boolean = false;

	DataTablePagingParameter: DropdownModel.DataTablePagingParameter = new DropdownModel.DataTablePagingParameter();

	searchCommonInfo: any = {};
	myToken: [REDACTED] = '';

	panel: number[] = [0];

	dropdownItemsHierarchyMerchants: DropdownModel.IDropdownItems[] = [];
	dropdownItemsMerchants: DropdownModel.IDropdownItems[] = [];
	dropdownItemsOrganization: DropdownModel.IDropdownItems[] = [];

	downloadPDF(type: number, langCode: string) {
		EventBus.$emit('show_mask', null);

		InternalTestApi.downloadApiDocumentPDF(type, langCode, this.myToken)
			.then((res) => {})
			.finally(() => {
				console.log('final');
				setTimeout(() => {
					EventBus.$emit('hide_mask', null);
				}, 2000);
			});
	}

	BO_Guid_PDF_URL() {
		const vm: any = this;
		const targetUrl = vm.staticResourceUrl + '/cs-download/PGS_BO_Guide.pdf';
		return targetUrl;
	}

	async downloadGameList() {
		EventBus.$emit('show_mask', null);
		await InternalTestApi.downloadGameListFIle().finally(() => {
			setTimeout(() => {
				EventBus.$emit('hide_mask', null);
			}, 500);
		});
	}

	mounted() {
		InternalTestApi.getCommonAdminSysFavoritSettings().then((res) => {
			this.searchCommonInfo = res.data;
			this.myToken = [REDACTED];
			this.isLoad = true;
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

h2.white-text {
	font-size: 18px;
}
</style>
