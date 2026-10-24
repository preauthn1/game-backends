<template>
	<div style="width: 100%; max-width: 1480px; margin: 0 auto">
		<div class="text-c">
			<v-btn @click="captureScreenshot" color="blue darken-2"> 擷取畫面 </v-btn>
		</div>

		<div id="refSlotHistory" ref="refSlotHistory" style="background: #333">
			<div class="text-c" style="color: #aaa; padding: 10px; font-size: 13px">當前時間: {{ getCurrentDate() }}</div>

			<v-row v-if="compareResult && compareResult.leftMerchant">
				<v-col cols="12" sm="6" md="6">
					<div class="v-data-table theme--dark">
						<div class="v-data-table__wrapper">
							<table class="">
								<thead class="v-data-table-header">
									<tr>
										<th></th>
										<th>
											<div class="merchantName">
												{{ compareResult.leftMerchant.name }}
											</div>

											<div class="exterInfo">
												{{ compareResult.leftMerchant.merchantID }}
											</div>
										</th>
										<th v-if="!isMerchantEqual(compareResult.leftMerchant, compareResult.rightMerchant)">
											<div class="merchantName">
												{{ compareResult.rightMerchant.name }}
											</div>

											<div class="exterInfo">
												{{ compareResult.rightMerchant.merchantID }}
											</div>
										</th>
									</tr>
								</thead>

								<tbody>
									<tr v-for="(item, index) in compareResult.compareColumns" :key="index">
										<td>
											{{ item.displayName }}
										</td>

										<td>
											<div :class="'maxWidth ' + highlightStyle(item)" @click="copyText(item, item.leftValue)">
												{{ item.leftValue }}
											</div>
										</td>
										<td v-if="!isMerchantEqual(compareResult.leftMerchant, compareResult.rightMerchant)">
											<div :class="'maxWidth ' + highlightStyle(item)" @click="copyText(item, item.rightValue)">
												{{ item.rightValue }}
											</div>
										</td>
									</tr>
								</tbody>
							</table>
						</div>
					</div>
				</v-col>

				<v-col cols="12" sm="6" md="6">
					<div class="v-data-table theme--dark">
						<div class="v-data-table__wrapper">
							<table class="">
								<thead class="v-data-table-header">
									<tr>
										<th></th>
										<th>
											<div class="merchantName">
												{{ compareResult.leftMerchant.name }}
											</div>

											<div class="exterInfo">
												{{ compareResult.leftMerchant.merchantID }}
											</div>
										</th>
										<th v-if="!isMerchantEqual(compareResult.leftMerchant, compareResult.rightMerchant)">
											<div class="merchantName">
												{{ compareResult.rightMerchant.name }}
											</div>

											<div class="exterInfo">
												{{ compareResult.rightMerchant.merchantID }}
											</div>
										</th>
									</tr>
								</thead>

								<tbody>
									<tr v-for="(item, index) in compareResult.advanceSettings" :key="index">
										<td>
											{{ item.displayName }}
										</td>
										<td>
											<div :class="'maxWidth ' + highlightStyle(item)" @click="copyText(item, item.leftValue)">
												{{ item.leftValue }}
											</div>
										</td>
										<td v-if="!isMerchantEqual(compareResult.leftMerchant, compareResult.rightMerchant)">
											<div :class="'maxWidth ' + highlightStyle(item)" @click="copyText(item, item.rightValue)">
												{{ item.rightValue }}
											</div>
										</td>
									</tr>
								</tbody>
							</table>
						</div>
					</div>
				</v-col>
			</v-row>
		</div>

		<v-overlay id="globalMask" :value="isGlobalMaskVisible">
			<v-progress-circular indeterminate size="64"> </v-progress-circular>
		</v-overlay>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import * as Model from '@/models/interfaces/system-setting/merchants';
import * as SystemSettingModel from '@/models/interfaces/system-setting/systemConfigSetting';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import MerchantsApi from '@/api/system-setting/merchants';
import DropdownApi from '@/api/commons/dropdown';

import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { ALL_CURRENCY } from '@/utilities/enums_helper';
import NotificationWarning from '@/components/notification/NotificationWarning.vue';
import { pageData, time } from '@/utilities/api-handler.lib';
// import Validation from '@/utilities/validation';
import { datetimeMixin, daysAgo, timeWithTimezone, toLocalDateString, toDateStringYYYYMMDDHHMMSS } from '@/utilities/datetime-format';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import { commonMethodMixin } from '@/utilities/common-methods';
import { State, Action, Getter, namespace } from 'vuex-class';
import axios, { AxiosResponse, AxiosRequestConfig } from 'axios';
import eazyFormApi from '@/api/commons/eazyFormApi';
const authModule = namespace('Auth');

@Component({ components: { NotificationWarning }, mixins: [commonDisplayFiltersMixin, datetimeMixin, commonMethodMixin] })
export default class MerchantCompare extends Vue {
	compareResult: Model.IMerchantCompareResult = {} as Model.IMerchantCompareResult;

	leftMerchantID: number = 0;
	rightMerchantID: number = 0;
	isGlobalMaskVisible = false;

	getCurrentDate() {
		const date = +new Date();
		return toDateStringYYYYMMDDHHMMSS(date, 8);
	}

	highlightStyle(item: Model.IMerchantCompareDetailItem) {
		if (item.isAllowCopy) {
			return 'link-text-grey';
		}

		return item.isEqual ? '' : 'highlightColor';
	}

	setQueryString() {
		const urlParams = new URLSearchParams(window.location.search);

		this.leftMerchantID = Number(urlParams.get('leftMerchantID'));
		this.rightMerchantID = Number(urlParams.get('rightMerchantID'));
	}

	setMerchantID(paraLeftMerchantID: number, paraRightMerchantID: number) {
		this.leftMerchantID = paraLeftMerchantID;
		this.rightMerchantID = paraRightMerchantID;
	}

	getCompareInfo() {
		if (this.leftMerchantID === 0 || this.rightMerchantID === 0) {
			return;
		}

		this.compareResult = {} as Model.IMerchantCompareResult;

		MerchantsApi.getMerchantCompareInfo(this.leftMerchantID, this.rightMerchantID).then((res) => {
			this.compareResult = res.data;
		});
	}

	isMerchantEqual(left: Model.IMerchantCompareBasicInfo, right: Model.IMerchantCompareBasicInfo) {
		return left.merchantID === right.merchantID;
	}

	copyText(item: Model.IMerchantCompareDetailItem, text: string) {
		if (!item.isAllowCopy) {
			return;
		}
		const vm: any = this;
		vm.copyTextToClipboard(text);
	}

	async captureScreenshot() {
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

		this.isGlobalMaskVisible = true;

		const elementID = document.getElementById('refSlotHistory');

		vm.$html2canvas(elementID, options).then((canvas: any) => {
			const imgData = canvas.toDataURL('image/jpeg');
			this.uploadBase64ImageToRemote(imgData);
		});

		setTimeout(() => {
			this.isGlobalMaskVisible = false;
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
				this.isGlobalMaskVisible = false;
			});
	}

	mounted() {
		this.setQueryString();

		this.getCompareInfo();
	}
}
</script>

<style scoped>
.exterInfo {
	color: #888;
	font-size: 12px;
	padding-bottom: 10px;
}

.merchantName {
	font-size: 18px;
	font-weight: 400;
	padding-top: 10px;
}
.maxWidth {
	width: 150px;
	overflow: hidden;
	white-space: nowrap;
	text-overflow: ellipsis;
}

.v-data-table td {
	height: 36px !important;
}

.highlightColor {
	color: #ff76c8;
}

.theme--dark.v-data-table {
	background-color: #424242;
	color: #ffffff;
}
.theme--dark.v-data-table .v-data-table__divider {
	border-right: 1px solid rgba(255, 255, 255, 0.12);
}
.theme--dark.v-data-table.v-data-table--fixed-header thead th {
	background: #424242;
	-webkit-box-shadow: inset 0 -1px 0 rgba(255, 255, 255, 0.12);
	box-shadow: inset 0 -1px 0 rgba(255, 255, 255, 0.12);
}
.theme--dark.v-data-table thead tr:last-child th {
	border-bottom: 1px solid rgba(255, 255, 255, 0.12);
}
.theme--dark.v-data-table thead tr th {
	color: rgba(255, 255, 255, 0.7);
}
.theme--dark.v-data-table tbody tr:not(:last-child) td:not(.v-data-table__mobile-row) {
	border-bottom: 1px solid rgba(255, 255, 255, 0.12);
}
.theme--dark.v-data-table tbody tr:not(:last-child) td:last-child {
	border-bottom: 1px solid rgba(255, 255, 255, 0.12);
}
.theme--dark.v-data-table tbody tr.active {
	background: #505050;
}
.theme--dark.v-data-table tbody tr:hover:not(.v-data-table__expanded__content) {
	background: #616161;
}

.v-data-table table {
	width: 100%;
	border-spacing: 0;
}
.v-data-table td,
.v-data-table th {
	padding: 0 16px;
}
.v-data-table th {
	-webkit-user-select: none;
	-moz-user-select: none;
	-ms-user-select: none;
	user-select: none;
	font-size: 12px;
	height: 48px;
}
.v-application--is-ltr .v-data-table th {
	text-align: left;
}
.v-application--is-rtl .v-data-table th {
	text-align: right;
}
.v-data-table td {
	font-size: 14px;
	height: 48px;
}

.v-data-table__wrapper {
	overflow-x: auto;
	overflow-y: hidden;
}

.v-data-table__progress {
	height: auto !important;
}
.v-data-table__progress th {
	height: auto !important;
	border: none !important;
	padding: 0;
}

.v-data-table--dense td {
	height: 24px;
}
.v-data-table--dense th {
	height: 32px;
}

.v-data-table--fixed-height .v-data-table__wrapper {
	overflow-y: auto;
}

.v-data-table--fixed-header .v-data-table__wrapper {
	overflow-y: auto;
}
.v-data-table--fixed-header thead th {
	border-bottom: 0px !important;
	position: -webkit-sticky;
	position: sticky;
	top: 0;
	z-index: 2;
}
.v-data-table--fixed-header thead tr:nth-child(2) th {
	top: 48px;
}
.v-application--is-ltr .v-data-table--fixed-header .v-data-footer {
	margin-right: 17px;
}
.v-application--is-rtl .v-data-table--fixed-header .v-data-footer {
	margin-left: 17px;
}

.v-data-table--fixed.v-data-table--dense thead tr:nth-child(2) th {
	top: 32px;
}
</style>
