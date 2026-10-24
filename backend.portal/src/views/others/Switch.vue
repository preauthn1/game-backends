<template>
	<div>
		<switchList />
		<v-btn icon large class="float-refresh" @click="refresh">
			<v-icon>refresh</v-icon>
		</v-btn>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import SwitchList from '@/components/game-management/MerchantGameSettingSwitchList.vue';
import EventBus from '@/utilities/event-bus';
@Component({ components: { SwitchList } })
export default class SwitchGame extends Vue {
	refresh() {
		EventBus.$emit(`reload-${this.$router.currentRoute.path}`, null);
	}
}
</script>
