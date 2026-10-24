<template>
	<v-app v-if="renderComponent">
		<BlankTemplateReport />
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import BlankTemplateReport from '@/components/statistics/merchant-summary/BlankTemplateReport.vue';
@Component({ components: { BlankTemplateReport } })
export default class BlankTemplateReportPage extends Vue {
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
