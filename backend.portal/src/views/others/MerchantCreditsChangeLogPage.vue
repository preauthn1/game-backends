<template>
	<div>
		<v-flex>
			<MerchantCreditChangeLogs />
		</v-flex>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import MerchantCreditChangeLogs from '@/components/agent/MerchantCreditChangeLogs.vue';

@Component({ components: { MerchantCreditChangeLogs } })
export default class MerchantCreditsChangeLogPage extends Vue {}
</script>

<style lang="scss">
.app {
	margin-left: 1% !important;
}
</style>
