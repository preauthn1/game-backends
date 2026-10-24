<template>
	<v-app>
		<List />
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import List from '@/components/system-setting/ServersRobotConfig.vue';
@Component({ components: { List } })
export default class ServersRobotConfig extends Vue {}
</script>
<style lang="scss">
.app {
	margin-left: 1% !important;
}
</style>
