<template>
	<v-app v-if="renderComponent">
		<AgentTotalSummaryReport />
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import AgentTotalSummaryReport from '@/components/statistics/merchant-summary/AgentTotalSummaryReport.vue';
@Component({ components: { AgentTotalSummaryReport } })
export default class AgentTotalSummaryReportPage extends Vue {
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
