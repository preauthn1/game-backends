<template>

	<!-- 遊戲商配置對應編輯 -->
	<v-dialog v-model="isShowDialog" max-width="800px">

		<v-card>
			<v-card-title class="grey darken-2">
				遊戲商配置對應編輯

				<div class="top-right-item" @click="closeAllModal">
					<v-btn icon dark @click="closeAllModal">
						<v-icon>close</v-icon>
					</v-btn>
				</div>
			</v-card-title>
			<v-card-text>
				<v-flex>

					<div class="mt-4" style="padding:5px 5px;">

						<span style="font-size: 18px;font-weight: 700;color:#FFF;">
							{{searchResult.merchantName}}
						</span>

						<span style="color:#888;">
							({{searchResult.merchantID}})
						</span>

						<v-chip color="red darken-4" class="ml-2" small style="vertical-align: top;">
							{{searchResult.merchantTypeText}}
						</v-chip>

						<div class="mb-2">

						</div>

						<v-divider></v-divider>
					</div>

					<v-row v-for="(item , index) in searchResult.mappings" :key="index">

						<v-col cols="12" sm="12" md="12">

							<div style="display: inline-block;    width: 150px;">
								<v-chip>
									{{item.gameVendorName}}
								</v-chip>
							</div>

							<v-autocomplete :items="convertToIntDropdown(item.dropdownItems)" :label="'遊戲商配置'"
								:disabled="!isEnableEdit(item.gameVendor)" v-model="item.vendorConfigID"
								style="width:250px;display: inline-block;">

							</v-autocomplete>

							<div style="display: inline-block;    width: 150px;" class="ml-4">

								<v-btn class="mr-2" :color="item.isAssignAnyGames? 'blue-grey darken-3': ''"
									v-if="!isEnableEdit(item.gameVendor)" style=""
									@click="switchToEnable(item.gameVendor)">
									編輯
								</v-btn>

								<v-btn class="mr-2" color="blue" v-if="isEnableEdit(item.gameVendor)"
									@click="saveUpdate(item)">
									儲存
								</v-btn>

								<v-btn class="mr-2" color="grey" v-if="isEnableEdit(item.gameVendor)"
									@click="clearEnable()">
									取消
								</v-btn>
							</div>

						</v-col>

					</v-row>

				</v-flex>
			</v-card-text>

		</v-card>
	</v-dialog>

</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import * as Model from '@/models/interfaces/system-setting/vendor';
import * as SystemSettingModel from '@/models/interfaces/system-setting/systemConfigSetting';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import MerchantsApi from '@/api/system-setting/merchants';
import DropdownApi from '@/api/commons/dropdown';
import VendorApi from '@/api/system-setting/vendor';

import { commonMethodMixin } from '@/utilities/common-methods';

import { Locale, Currency, Status } from '@/models/status/common';
import { WalletMode } from '@/models/status/merchant';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { ALL_CURRENCY } from '@/utilities/enums_helper';

import NotificationWarning from '@/components/notification/NotificationWarning.vue';
import MerchantCompare from '@/components/system-setting/MerchantCompare.vue';

import { pageData, time } from '@/utilities/api-handler.lib';
import { datetimeMixin } from '@/utilities/datetime-format';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import { State, Action, Getter, namespace } from 'vuex-class';
const authModule = namespace('Auth');
import * as Permission from '@/models/status/permission';
import { uuid } from 'vue-uuid';

@Component({ components: { NotificationWarning, MerchantCompare }, mixins: [commonDisplayFiltersMixin, datetimeMixin, commonMethodMixin] })
export default class MerchantVendorConfigUpdate extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];

	enableVendorIDs: number[] = [];
	isShowDialog: boolean = false;
	searchResult: Model.IMerchantVendorConfigSettings = {} as Model.IMerchantVendorConfigSettings;

	isEnableEdit(vendorID: number) {
		return this.enableVendorIDs.includes(vendorID);
	}

	switchToEnable(vendorID: number) {
		this.enableVendorIDs = [];
		this.enableVendorIDs.push(vendorID);
	}

	clearEnable() {
		this.enableVendorIDs = [];
	}

	convertToIntDropdown(items: any[]) {
		return items.map((x) => {
			return {
				text: x.text,
				value: x.valueInt,
			};
		});
	}

	getSettings(merchantID: number) {
		this.isShowDialog = true;
		this.enableVendorIDs = [];
		this.searchResult = {} as Model.IMerchantVendorConfigSettings;

		EventBus.$emit('show_mask', null);

		VendorApi.getMerchantVendorConfigMappings(merchantID)
			.then((res) => {
				this.searchResult = res.data;
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	saveUpdate(item: Model.IMerchantVendorConfigSettingItem) {
		const body: Model.IMerchantVendorConfigSettingUpdate = {} as Model.IMerchantVendorConfigSettingUpdate;
		body.vendorConfigID = item.vendorConfigID;
		body.gameVendor = item.gameVendor;
		body.merchantID = this.searchResult.merchantID;

		EventBus.$emit('show_mask', null);

		VendorApi.updateMerchantVendorConfigMappings(body)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.clearEnable();
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	closeAllModal() {
		this.isShowDialog = false;
	}

	mounted() {
		const qqq = '';
	}
}
</script>


<style>
</style>