<template>

	<v-dialog v-model="isShowDialog" max-width="800px" scrollable>

		<v-card>

			<v-card-title class="grey darken-2">
				{{SelfDefineColumns.title}}

				<div class="top-right-item mr-2">
					<div class="close" @click="closeAllDialog">
						<v-icon>close</v-icon>
					</div>
				</div>

			</v-card-title>

			<v-card-text>

				<div style="padding: 15px;min-height:400px;">
					<div v-for="(item , index) in SelfDefineColumns.items" :key="index" class="mb-4">

						<div class="common-info-label">
							{{item.displayName}}
						</div>

						<div class=""
							style="word-wrap: break-word;word-break: break-all;    white-space: pre-line;">
							<div v-if="item.format === 'code'" class="code-block">
								{{item.value}}
							</div>
							<div v-else>
								{{item.value}}
							</div>
						</div>

					</div>
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
import { Locale } from '@/models/status/common';
import DatePicker from 'vue2-datepicker';
import * as ResponseModel from '@/models/interfaces/commons/response';
import { datetimeMixin, startEndDate, timeWithTimezone, daysAgo, weeksAgo, monthsAgo, formatMixin, addMinutes } from '@/utilities/datetime-format';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { commonMethodMixin } from '@/utilities/common-methods';
import * as PermissionModel from '@/models/interfaces/commons/permission';
const authModule = namespace('Auth');

// @ts-ignore
@Component({
	// @ts-ignore
	mixins: [datetimeMixin, commonDisplayFiltersMixin, formatMixin, commonMethodMixin],
	components: { DatePicker },
})
export default class CustomizedColumnsDisplayPopupDialog extends Vue {
	@Prop(String) readonly defaultLang!: string;

	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;

	isShowDialog: boolean = false;
	configMaxWidth: number = 800;

	headers: any = null;
	copyText: string = '';

	pagingFooterProps: any = { 'items-per-page-options': [10, 30, 50, 100] };
	pagingItemsPerPage: number = 100;
	displayName: string = '';

	panel: number[] = [0];
	SelfDefineColumns: ResponseModel.ISelfDefineColumnDisplay = {} as ResponseModel.ISelfDefineColumnDisplay;

	displayColumnsByObject(myObject: ResponseModel.ISelfDefineColumnDisplay) {
		this.isShowDialog = true;
		this.SelfDefineColumns = myObject;
	}

	closeAllDialog() {
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

.code-block {
	overflow-wrap: break-word;
	word-break: break-all;
	padding: 7px 10px;
	border-radius: 5px;
	background: #727272;
	color: #fff;
}
</style>
