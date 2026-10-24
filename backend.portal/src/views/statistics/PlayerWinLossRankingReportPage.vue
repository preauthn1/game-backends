<template>
	<v-app v-if="renderComponent">
		<PlayerWinLossRankingReport />
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import PlayerWinLossRankingReport from '@/components/statistics/players/PlayerWinLossRankingReport.vue';
@Component({ components: { PlayerWinLossRankingReport } })
export default class PlayerWinLossRankingReportPage extends Vue {
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
