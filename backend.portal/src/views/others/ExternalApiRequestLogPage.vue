<template>
	<div>
		<v-flex>
			<PageComponent />
		</v-flex>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import PageComponent from '@/components/system-logs/ExternalApiRequestLog.vue';

@Component({ components: { PageComponent } })
export default class ExternalApiRequestLogPage extends Vue {}
</script>
<style lang="scss">
.app {
	margin-left: 1% !important;
}
</style>
