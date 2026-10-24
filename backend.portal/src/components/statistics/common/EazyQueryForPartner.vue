<template>
	<v-app class="app">

		<div v-if="hasPermissionQueryInvoiceEachMerchantDetail">

			<v-btn color="grey lighten-1" text class="btn mr-1"
				@click.stop="showExtraFilterRulesDialog()">
				<v-icon left>book</v-icon>
				帳單-商戶折扣明細條目
			</v-btn>

		</div>

		<AccountingInvoiceAccordionView ref="refAccountingInvoiceAccordionView">
		</AccountingInvoiceAccordionView>

	</v-app>

</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import { State, Action, Getter, namespace } from 'vuex-class';
import EventBus from '@/utilities/event-bus';
import { commonDisplayFiltersMixin } from '@/utilities/display-filters';
import { commonMethodMixin } from '@/utilities/common-methods';
import DropdownApi from '@/api/commons/dropdown';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import * as Model from '@/models/interfaces/statistics/merchant-summary';
import PerGameApi from '@/api/game-management/per-game';
import oneToManyCustomizedSettingApi from '@/api/system-setting/oneToManyCustomizedSetting';
import * as OneToManyModel from '@/models/interfaces/system-setting/systemConfigSetting';
import AccountingInvoiceAccordionView from '@/components/system-setting/AccountingInvoiceAccordionView.vue';
import * as ChartModel from '@/models/interfaces/statistics/chart';
import MerchantSummaryApi from '@/api/statistics/merchant-summary';
import ChartApi from '@/api/statistics/chart';
import DatePicker from 'vue2-datepicker';
import LineChartPopup from '@/components/statistics/LineChartPopup.vue';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import * as Permission from '@/models/status/permission';

const authModule = namespace('Auth');

// @ts-ignore
@Component({ mixins: [commonDisplayFiltersMixin, commonMethodMixin], components: { DatePicker, LineChartPopup, AccountingInvoiceAccordionView } })
export default class EazyQueryForPartner extends Vue {
	@authModule.State('userProfile') userProfile!: PermissionModel.IProfileResponse;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];

	perPageOfMemo: number = 50;
	listOfvisible: number[] = [];

	get hasPermissionInvoiceExport() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.System_AccountingInvoice_ExportInvoice) >= 0;
	}

	get hasPermissionSuperAdmin() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.SystemSetting_SystemConfigSetting) >= 0;
	}

	get hasPermissionQueryInvoiceEachMerchantDetail() {
		return this.allPermissions.findIndex((s) => s.token === Permission.AllPermissionToken.Statistics_EazyQuery_QueryInvoiceEachMerchantDetail) >= 0;
	}

	showExtraFilterRulesDialog() {
		const targetRef: any = this.$refs.refAccountingInvoiceAccordionView;
		targetRef.showExtraFilterRulesDialog();
	}

	mounted() {
		this.init();
	}

	init() {}
}
</script>
<style lang="scss" scoped>
.checkbox-layer2 {
	.v-input--checkbox {
		margin-top: -10px;
	}
}
</style>

