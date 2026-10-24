<template>
	<v-app v-if="renderComponent">
		<AgentHierarchyDailyReport />
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import AgentHierarchyDailyReport from '@/components/statistics/merchant-summary/AgentHierarchyDailyReport.vue';
@Component({ components: { AgentHierarchyDailyReport } })
export default class AgentHierarchyDailyReportPage extends Vue {
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
