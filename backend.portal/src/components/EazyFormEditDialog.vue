<template>

	<v-dialog v-model="isShowDialog" :max-width="'500px'">

		<v-card>

			<v-card-title class="grey darken-2">
				{{ $t("Button.__edit") }}
				<span class="f14 grey-text ml-2">
					{{myEazyForm.titleExtraInfo}}
				</span>
			</v-card-title>

			<v-card-text>

				<v-row v-for="(item , index) in myEazyForm.formItems" :key="index">

					<v-col>
						<v-text-field :label="$t(item.i18nKey)" v-model="myEazyForm.myModel[item.keyName]">
						</v-text-field>
					</v-col>

				</v-row>

			</v-card-text>

			<v-card-actions>

				<v-spacer></v-spacer>
				<v-btn text @click="closeAllModal()" class="btn">
					<v-icon left>mdi-cancel</v-icon>
					{{$t('Button.__cancel')}}
				</v-btn>
				<v-btn color="yellow darken-1" text @click="saveForm()" class="btn">
					<v-icon left>mdi-check-circle</v-icon>
					{{$t('Button.__submit')}}
				</v-btn>

			</v-card-actions>

		</v-card>

	</v-dialog>
</template>

<script lang="ts">
import Vue from 'vue';
import EventBus from '@/utilities/event-bus';
import { Component, Prop } from 'vue-property-decorator';
import { State, Action, Getter, namespace } from 'vuex-class';
import { Locale } from '@/models/status/common';
import DropdownApi from '@/api/commons/dropdown';
import EazyFormApi from '@/api/commons/eazyFormApi';
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
export default class EazyFormEditDialog extends Vue {
	@Prop(String) readonly defaultLang!: string;

	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;

	isShowDialog: boolean = false;
	isShowTable: boolean = false;
	isShowCloseButton: boolean = false;
	isFixedHeader: boolean = true;

	configMaxWidth: number = 800;

	headers: any = null;
	copyText: string = '';
	displayName: string = '';
	extraMemo: string = '';

	myEazyForm: any = {};

	getFormColumns(myFilter: DropdownModel.ILazyCommonParameterReqeust) {
		EventBus.$emit('show_mask', null);

		EazyFormApi.getFormColumns(myFilter)
			.then((res) => {
				this.myEazyForm = {};
				this.isShowDialog = true;
				this.myEazyForm = res.data;
			})
			.finally(() => {
				setTimeout(() => {
					EventBus.$emit('hide_mask', null);
				}, 300);
			});
	}

	saveForm() {
		EventBus.$emit('show_mask', null);

		EazyFormApi.saveForm(this.myEazyForm)
			.then((res) => {
				EventBus.$emit(`successfully-modified`);
				this.closeAllModal();
				this.$emit('parentCallbackAfterSave');
			})
			.finally(() => {
				EventBus.$emit('hide_mask', null);
			});
	}

	closeAllModal() {
		this.isShowDialog = false;
	}

	init() {}

	mounted() {
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
