<template>
	<div>
		<v-flex>
			<SystemLogApiRequestLogPage />
		</v-flex>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import SystemLogApiRequestLogPage from '@/components/system-logs/SystemLogApiRequestLog.vue';

@Component({ components: { SystemLogApiRequestLogPage } })
export default class SingleWalletPlatformReqeustLogPage extends Vue {}
</script>
<style lang="scss">
.app {
	margin-left: 1% !important;
}
</style>
