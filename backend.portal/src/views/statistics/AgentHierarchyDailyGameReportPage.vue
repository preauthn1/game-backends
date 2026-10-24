<template>
	<v-app v-if="renderComponent">
		<AgentHierarchyDailyGameReport />
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import AgentHierarchyDailyGameReport from '@/components/statistics/merchant-summary/AgentHierarchyDailyGameReport.vue';

@Component({ components: { AgentHierarchyDailyGameReport } })
export default class AgentHierarchyDailyGameReportPage extends Vue {
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
