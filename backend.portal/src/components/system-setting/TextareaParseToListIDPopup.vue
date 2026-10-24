<template>

	<v-dialog v-model="isShowDialog" max-width="600px">

		<v-card>
			<v-card-title class="grey darken-2">
				輸入IDs (換行符號、或逗號隔開)
				<div class="top-right-item" @click="closeAllModal">
					<v-btn icon dark @click="isShowDialog">
						<v-icon>close</v-icon>
					</v-btn>
				</div>
			</v-card-title>
			<v-card-text>
				<div style="padding:10px;">

					<v-row style="display: block;">

						<div style="" class="textarea-bg">
							<v-textarea v-model="text" :label="'請每行貼上一組ID'" autofocus>
							</v-textarea>
						</div>

						<div style="clear:both">

						</div>
					</v-row>

				</div>
			</v-card-text>
			<v-card-actions>
				<v-spacer></v-spacer>
				<v-btn color="yellow darken-1" text @click="parseTextToIDsByAPI" class="btn">
					<v-icon left>mdi-check-circle</v-icon>
					轉換成多選IDs勾選
				</v-btn>
			</v-card-actions>
		</v-card>

	</v-dialog>

</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Watch } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import * as Model from '@/models/interfaces/system-setting/merchants';
import * as SystemSettingModel from '@/models/interfaces/system-setting/systemConfigSetting';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import InternalTestApi from '@/api/commons/internalTest';
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
const authModule = namespace('Auth');

@Component({ components: { NotificationWarning }, mixins: [commonDisplayFiltersMixin, datetimeMixin, commonMethodMixin] })
export default class TextareaParseToListIDPopup extends Vue {
	isShowDialog: boolean = false;
	text: string = '';
	resultIDs: number[] = [];

	showParseTextareaPraseToIDs() {
		this.isShowDialog = true;
		this.text = '';
		this.resultIDs = [];
	}

	parseTextToIDsByAPI() {
		const request = {} as DropdownModel.ILazyCommonParameterReqeust;
		request.rawText = this.text;

		InternalTestApi.parseTextToListIDs(request).then((res) => {
			this.resultIDs = res.data.ids;

			this.closeAllModal();
			this.$emit('parentSetResultIDs', this.resultIDs);
		});
	}

	closeAllModal() {
		this.isShowDialog = false;
	}

	mounted() {}
}
</script>


<style scoped>
.exterInfo {
	color: #888;
	font-size: 12px;
	padding-bottom: 10px;
}

.textarea-bg textarea {
	background: #252525;
	padding: 5px;
}
</style>