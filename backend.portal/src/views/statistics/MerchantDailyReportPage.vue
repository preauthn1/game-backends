<template>
	<v-app v-if="renderComponent">
		<MerchantDailyReport />
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import MerchantDailyReport from '@/components/statistics/merchant-summary/MerchantDailyReport.vue';
@Component({ components: { MerchantDailyReport } })
export default class MerchantDailyReportPage extends Vue {
	renderComponent: boolean = true;
	mounted() {
		this.registerReloadComponent();
	}
	registerReloadComponent() {
		EventBus.$on(`reload-${this.$router.currentRoute.path}`, (info: any) => {
			this.renderComponent = false;
			this.$nextTick(() => {
				this.renderComponent = true;
			});
		});
	}
}
</script>
