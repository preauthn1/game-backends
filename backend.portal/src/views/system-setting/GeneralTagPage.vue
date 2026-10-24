<template>
	<div>
		<v-flex>
			<GeneralTag />
		</v-flex>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import GeneralTag from '@/components/system-setting/GeneralTag.vue';

@Component({ components: { GeneralTag } })
export default class GeneralTagPage extends Vue {}
</script>
<style lang="scss">
.app {
	margin-left: 1% !important;
}
</style>
