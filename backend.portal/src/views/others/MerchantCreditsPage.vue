<template>
	<div>
		<v-flex>
			<MerchantCredits />
		</v-flex>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import MerchantCredits from '@/components/agent/MerchantCredits.vue';

@Component({ components: { MerchantCredits } })
export default class MerchantCreditsPage extends Vue {}
</script>

<style lang="scss">
.app {
	margin-left: 1% !important;
}
</style>
