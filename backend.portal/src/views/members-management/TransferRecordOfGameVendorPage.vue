<template>
	<v-app>
		<TransferRecordOfGameVendor />
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import TransferRecordOfGameVendor from '@/components/members-management/TransferRecordOfGameVendor.vue';
@Component({ components: { TransferRecordOfGameVendor } })
export default class TransferRecordOfGameVendorPage extends Vue {}
</script>