<template>
	<div>
		<v-flex>
			<MerchantHierarchyTreeView />
		</v-flex>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import MerchantHierarchyTreeView from '@/components/agent/MerchantHierarchyTreeView.vue';

@Component({ components: { MerchantHierarchyTreeView } })
export default class MerchantHierarchyTreeViewPage extends Vue {}
</script>

<style lang="scss">
.app {
	margin-left: 1% !important;
}
</style>
