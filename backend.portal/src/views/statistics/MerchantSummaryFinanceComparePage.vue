<template>
	<v-app v-if="renderComponent">
		<MerchantSummaryFinanceCompare />
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import MerchantSummaryFinanceCompare from '@/components/statistics/merchant-summary/MerchantSummaryFinanceCompare.vue';
@Component({ components: { MerchantSummaryFinanceCompare } })
export default class MerchantSummaryFinanceComparePage extends Vue {
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
