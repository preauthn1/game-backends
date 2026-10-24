<template>

	<v-card style="border:solid 1px #444;">

		<v-card-title class="grey darken-2">

			{{$t("Popup.__update")}} ({{$t("TableHeader.__status")}})

			<div class="top-right-item" @click="closeAllModal">
				<v-btn icon dark>
					<v-icon>close</v-icon>
				</v-btn>
			</div>

		</v-card-title>

		<v-card-text class="padding-row" v-if="isRenderDOM">

			<div style="padding:10px;color:#EEE;font-size:16px;line-height: 30px;">
				<v-row>
					<v-text-field v-model="previewInfo.merchantID" :label="$t('TableHeader.__merchantID')"
						disabled='disabled'>
					</v-text-field>
				</v-row>

				<v-row>
					<v-text-field v-model="previewInfo.merchantName" :label="$t('TableHeader.__merchant')"
						disabled='disabled'>
					</v-text-field>
				</v-row>

				<v-row v-if="previewInfo.numberOfChilds > 0">
					<v-text-field v-model="previewInfo.numberOfChilds" :label="`下級代理數量`" disabled='disabled'>
					</v-text-field>
				</v-row>

				<v-row>

					<v-autocomplete :items="dropdownItemsStatus" v-model="updateStatusForm.status" chips
						@change="reloadDOM" :label="$t('TableHeader.__status')">
					</v-autocomplete>

				</v-row>

				<v-row>

					<v-text-field v-model="updateStatusForm.memo"
						:label="$t('TableHeader.__memo')+ '／' + $t('TableHeader.__reason')">
					</v-text-field>

				</v-row>

				<v-row v-if="previewInfo.numberOfChilds > 0 && updateStatusForm.status == 0">

					<v-autocomplete :items="dropdownItemsYesOrNo" v-model="updateStatusForm.yesOrNo" chips
						:label="$t('Message.__syncStatusToChilds')">
					</v-autocomplete>

				</v-row>

				<v-row>

					<div style="font-size:12px;color:#ff9b9b;line-height:20px;">
						<v-icon color="red lighten-2" small>
							error
						</v-icon>

						{{$t("Message.__agentStatusDisable_Hint1")}}
					</div>

					<div style="font-size:12px;color:#BBB;line-height:20px;">
						<v-icon color="grey " small>
							error
						</v-icon>

						{{$t("Message.__agentStatusDisable_Hint2")}}
					</div>
				</v-row>

			</div>

		</v-card-text>

		<v-card-actions>
			<v-spacer></v-spacer>

			<v-btn color="yellow darken-1" text @click="saveEditStatus" class="btn">
				<v-icon left>mdi-check-circle</v-icon>
				{{ $t('Button.__submit') }}
			</v-btn>
		</v-card-actions>

	</v-card>

</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import * as Model from '@/models/interfaces/system-setting/merchants';
import * as MerchantCreditModel from '@/models/interfaces/agent/merchantCredit';
import * as SystemSettingModel from '@/models/interfaces/system-setting/systemConfigSetting';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import MerchantsApi from '@/api/system-setting/merchants';
import DropdownApi from '@/api/commons/dropdown';
import { commonMethodMixin } from '@/utilities/common-methods';

import { Locale, Currency, Status } from '@/models/status/common';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { pageData, time } from '@/utilities/api-handler.lib';
import { datetimeMixin } from '@/utilities/datetime-format';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import { State, Action, Getter, namespace } from 'vuex-class';
const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';

@Component({ components: {}, mixins: [commonDisplayFiltersMixin, datetimeMixin, commonMethodMixin] })
export default class MerchantStatusEditPopup extends Vue {
	locale: typeof Locale = Locale;

	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];

	renderComponent: boolean = true;
	isRenderDOM: boolean = true;

	updateStatusForm: DropdownModel.ILazyCommonParameterReqeust = {} as DropdownModel.ILazyCommonParameterReqeust;
	previewInfo: Model.IMerchantStatusHierarchyPreview = {} as Model.IMerchantStatusHierarchyPreview;

	panel: number[] = [0];

	dropdownItemsStatus: DropdownModel.IDropdownItems[] = [];
	dropdownItemsYesOrNo: DropdownModel.IDropdownItems[] = [];

	registerReloadComponent() {
		EventBus.$on(`reload-${this.$router.currentRoute.path}`, (info: any) => {
			this.renderComponent = false;
			this.$nextTick(() => {
				this.renderComponent = true;
			});
		});
	}

	reloadDOM() {
		this.isRenderDOM = false;

		this.$nextTick(() => {
			this.isRenderDOM = true;
		});
	}

	getPreviewInfo(merchantID: number) {
		this.updateStatusForm.yesOrNo = -1;
		this.updateStatusForm.memo = '';

		EventBus.$emit('show_mask', null);
		MerchantsApi.getStatusAndHierarchyPreviewInfo(merchantID)
			.then((res) => {
				this.previewInfo = res.data;
				this.updateStatusForm.merchantID = merchantID;
				this.updateStatusForm.status = res.data.status;

				if (res.data.numberOfChilds <= 0) {
					this.updateStatusForm.yesOrNo = 0;
				}

				this.reloadDOM();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	saveEditStatus() {
		EventBus.$emit('show_mask', null);
		MerchantsApi.updateMerchantStatusByAgent(this.updateStatusForm)
			.then((x) => {
				EventBus.$emit(`successfully-modified`);
				this.closeAllModal();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	closeAllModal() {
		this.$emit(`parentCloseAllModal`);
	}

	mounted() {
		DropdownApi.getNormalStatus().then((res) => {
			this.dropdownItemsStatus = res.filter((x) => x.valueInt !== 999);
		});

		DropdownApi.getYesOrNoOrNotSelect().then((res) => {
			this.dropdownItemsYesOrNo = res;
		});
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

.hint-warning-color {
	color: #ef4c4c;
}

.hint-warning {
	color: #ef4c4c;
	font-size: 14px;
	display: inline-block;
}

.hint-warning i {
	color: #ef4c4c !important;
	font-size: 16px !important;
}

.type_0 {
	color: #b5b5b5;
}

.type_20 {
	color: #72cbff;
}

.type_30 {
	color: #ffa67b;
}

.type_40 {
	color: #49d49a;
}

.color-label label {
	color: #fd6262 !important;
	font-size: 12px;
}

.reviewMerchant .col-md-6,
.reviewMerchant .col-md-12 {
	padding: 4px 12px !important;
}
</style>