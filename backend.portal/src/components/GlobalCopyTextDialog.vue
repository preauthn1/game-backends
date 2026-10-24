<template>
	<v-dialog v-model="isShowDialog" :max-width="'600px'">
		<v-card>
			<v-card-title class="grey darken-2">
				{{ $t('Button.__copyToClipboard') }}

				<v-spacer></v-spacer>

				<div class="top-right-item" @click="closeAllModal">
					<v-btn icon dark color="yellow lighten-3">
						<v-icon>close</v-icon>
					</v-btn>
				</div>
			</v-card-title>

			<v-card-text style="min-height: 250px">
				<div class="mt-4 mb-2">
					<v-icon color="grey" small>error</v-icon>
					{{ $t('Message.__youCanClickThisCopyText') }}.
				</div>

				<div
					class="pointer"
					v-clipboard:copy="finalCopyText"
					:title="'Click to copy'"
					style="white-space: pre-line; max-width: 500px; background: #444; color: #b4b437; border-radius: 5px; padding: 5px 10px; font-size: 15px"
					v-clipboard:success="copySuccess"
				>
					{{ copyText }}
				</div>
			</v-card-text>
		</v-card>
	</v-dialog>
</template>

<script lang="ts">
import Vue from 'vue';
import EventBus from '@/utilities/event-bus';
import { Component, Prop } from 'vue-property-decorator';
import { State, Action, Getter, namespace } from 'vuex-class';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import DatePicker from 'vue2-datepicker';
import * as ResponseModel from '@/models/interfaces/commons/response';
import { datetimeMixin, startEndDate, timeWithTimezone, daysAgo, weeksAgo, monthsAgo, formatMixin, addMinutes } from '@/utilities/datetime-format';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { commonMethodMixin } from '@/utilities/common-methods';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import internalTestApi from '@/api/commons/internalTest';
import * as Permission from '@/models/status/permission';
const authModule = namespace('Auth');

// @ts-ignore
@Component({
	// @ts-ignore
	mixins: [datetimeMixin, commonDisplayFiltersMixin, formatMixin, commonMethodMixin],
	components: { DatePicker },
})
export default class GlobalCopyTextDialog extends Vue {
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;

	isShowDialog: boolean = false;
	isShowTable: boolean = false;

	configMaxWidth: number = 800;

	searchItems: any = null; // 格式是動態的
	searchExtraInfo: any = null;
	copyText: string = '';

	panel: number[] = [0];

	get finalCopyText() {
		const result = this.copyText
			.split('\n')
			.filter((line) => !line.includes('遊戲問題請轉至'))
			.join('\n');

		return result;
	}

	showDialog(config: any) {
		this.copyText = config.copyText;
		this.isShowDialog = true;
	}

	closeAllModal() {
		this.isShowDialog = false;
	}

	copySuccess() {
		EventBus.$emit(`successfully-copy`);
	}

	init() {}

	mounted() {
		const urlParams = new URLSearchParams(window.location.search);
		const myParam = urlParams.get('querySqlID');
		this.init();
	}
}
</script>
<style lang="scss" scope>
span.white--text.headline {
	font-family: 微軟正黑體, arial !important;
}
.country {
	background-color: transparent !important;
	box-shadow: none !important;
}
.lang {
	white-space: nowrap !important;
}

table th {
	user-select: initial !important;
}

.col-sm-3 .buttonRow,
.col-md-3 .buttonRow {
	margin-top: 15px;
}

.hintRow,
.hintRow .v-icon {
	padding: 3px 0px;
	font-size: 14px;
	color: #aaa;
}

.hintRow > div {
	display: inline-block;
	margin-right: 15px;
}

.hintRow .v-icon {
	color: #aaa;
	font-size: 20px;
}

.link-sm {
	font-size: 16px;
	margin-right: 20px;
	display: inline-block;
}

.no-margin-padding {
	padding: 3px;
	margin: 0px;
}

.customizedTable {
	min-height: 300px;
}

.customizedTable tbody {
	max-width: 90vw !important;
}

@media only screen and (max-width: 480px) {
	.v-dialog {
		position: fixed;
		margin: 0px;
	}
}
</style>
