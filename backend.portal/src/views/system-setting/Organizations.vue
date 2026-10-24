<template>
	<div>
		<v-flex>
			<Organization />
		</v-flex>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import Organization from '@/components/system-setting/Organizations.vue';

@Component({ components: { Organization } })
export default class Organizations extends Vue {}
</script>
<style lang="scss">
.app {
	margin-left: 1% !important;
}
</style>
