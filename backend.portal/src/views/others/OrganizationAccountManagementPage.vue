<template>
	<div>
		<v-flex>
			<OrganizationAccountManagement />
		</v-flex>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import OrganizationAccountManagement from '@/components/others/OrganizationAccountManagement.vue';

@Component({ components: { OrganizationAccountManagement } })
export default class OrganizationAccountManagementPage extends Vue {}
</script>

<style lang="scss">
.app {
	margin-left: 1% !important;
}
</style>
