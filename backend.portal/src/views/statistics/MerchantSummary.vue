<template>
	<v-app v-if="renderComponent">
		<MerchantSummaryReport />
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import MerchantSummaryReport from '@/components/statistics/merchant-summary/MerchantSummaryReport.vue';
@Component({ components: { MerchantSummaryReport } })
export default class MerchantSummary extends Vue {
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
