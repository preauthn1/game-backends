<template>
	<v-app v-if="renderComponent">
		<FreespinStatusList />
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import FreespinStatusList from '@/components/system-setting/events/FreespinStatusList.vue';
@Component({ components: { FreespinStatusList } })
export default class FreespinStatusListPage extends Vue {
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
