<template>
	<div>
		<v-flex>
			<LanguageText />
		</v-flex>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import LanguageText from '@/components/system-setting/LanguageText.vue';

@Component({ components: { LanguageText } })
export default class LanguageTextPage extends Vue {}
</script>
<style lang="scss">
.app {
	margin-left: 1% !important;
}
</style>
