<template>
	<div>
		<v-flex>
			<BatchEditServerMerchants />
		</v-flex>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';
import BatchEditServerMerchants from '@/components/system-setting/servers/BatchEditServerMerchants.vue';
@Component({ components: { BatchEditServerMerchants } })
export default class Servers extends Vue {}
</script>
<style lang="scss">
.app {
	margin-left: 1% !important;
}
</style>
