<template>
	<v-app v-if="renderComponent">
		<EazyQueryForPartner />
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import EazyQueryForPartner from '@/components/statistics/common/EazyQueryForPartner.vue';
@Component({ components: { EazyQueryForPartner } })
export default class EazyQueryForPartnerPage extends Vue {
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
