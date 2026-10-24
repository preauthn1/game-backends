<template>
	<div>
		<v-flex>
			<ServerList />
		</v-flex>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import ServerList from '@/components/system-setting/servers/ServerList.vue';
@Component({ components: { ServerList } })
export default class Servers extends Vue {}
</script>
<style lang="scss">
.app {
	margin-left: 1% !important;
}
</style>
