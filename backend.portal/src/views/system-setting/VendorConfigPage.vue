<template>
	<div>
		<v-flex>
			<MainComponent />
		</v-flex>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import MainComponent from '@/components/system-setting/VendorConfig.vue';

@Component({ components: { MainComponent } })
export default class Merchants extends Vue {}
</script>
<style lang="scss">
.app {
	margin-left: 1% !important;
}
</style>
