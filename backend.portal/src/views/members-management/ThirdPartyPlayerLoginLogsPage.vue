<template>
	<div>
		<v-flex>
			<Merchant />
		</v-flex>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import Merchant from '@/components/members-management/ThirdPartyPlayerLoginLogs.vue';

@Component({ components: { Merchant } })
export default class Merchants extends Vue {}
</script>
<style lang="scss">
.app {
	margin-left: 1% !important;
}
</style>
