<template>
	<div>
		<v-flex>
			<oneToManyCustomizedSetting />
		</v-flex>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import oneToManyCustomizedSetting from '@/components/system-setting/OneToManyCustomizedSetting.vue';

@Component({ components: { oneToManyCustomizedSetting } })
export default class OneToManyCustomizedSettingPage extends Vue {}
</script>
<style lang="scss">
.app {
	margin-left: 1% !important;
}
</style>
