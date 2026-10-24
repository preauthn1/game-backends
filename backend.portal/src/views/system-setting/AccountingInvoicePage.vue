<template>
	<div>
		<v-flex>
			<AccountingInvoice />
		</v-flex>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import AccountingInvoice from '@/components/system-setting/AccountingInvoice.vue';

@Component({ components: { AccountingInvoice } })
export default class AccountingInvoicePage extends Vue {}
</script>
<style lang="scss">
.app {
	margin-left: 1% !important;
}
</style>
