<template>
	<div>
		<v-flex>
			<CustomerServiceTool />
		</v-flex>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import CustomerServiceTool from '@/components/system-setting/CustomerServiceTool.vue';

@Component({ components: { CustomerServiceTool } })
export default class CustomerServiceToolPage extends Vue {}
</script>
<style lang="scss">
.app {
	margin-left: 1% !important;
}
</style>
