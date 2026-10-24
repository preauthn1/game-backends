<template>
	<v-app v-if="renderComponent">
		<PlayerGameSummary />
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import PlayerGameSummary from '@/components/statistics/players/PlayerGameSummaryReport.vue';
@Component({ components: { PlayerGameSummary } })
export default class PlayerGameSummaryPage extends Vue {
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
