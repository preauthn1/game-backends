<template>
	<div>
		<v-flex>
			<IPAddressTemplateList />
		</v-flex>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import IPAddressTemplateList from '@/components/system-setting/IPAddressTemplateList.vue';

@Component({ components: { IPAddressTemplateList } })
export default class IPAddressTemplateListPage extends Vue {}
</script>
<style lang="scss">
.app {
	margin-left: 1% !important;
}
</style>
