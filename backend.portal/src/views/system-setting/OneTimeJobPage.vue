<template>
	<div>
		<v-flex>
			<OneTimeJob />
		</v-flex>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import OneTimeJob from '@/components/system-setting/OneTimeJob.vue';

@Component({ components: { OneTimeJob } })
export default class OneTimeJobPage extends Vue {}
</script>
<style lang="scss">
.app {
	margin-left: 1% !important;
}
</style>
