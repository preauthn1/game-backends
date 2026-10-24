<template>
	<v-app v-if="renderComponent">
		<FreespinReportSummaryReport />
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import FreespinReportSummaryReport from '@/components/statistics/merchant-summary/FreespinReportSummaryReport.vue';
@Component({ components: { FreespinReportSummaryReport } })
export default class FreespinReportPage extends Vue {
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
