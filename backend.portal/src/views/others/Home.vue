<template>
	<v-app id="inspire" v-if="renderComponent">
		<HomePageSummary />
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import HomePageSummary from '@/components/HomePageSummary.vue';
@Component({
	components: { HomePageSummary },
})
export default class Home extends Vue {
	renderComponent: boolean = true;

	mounted() {
		// this.$i18n.locale = 'cn';
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

<style scoped>
@media screen and (-ms-high-contrast: active), (-ms-high-contrast: none) {
	.flex {
		min-width: 300px;
	}
}
</style>
