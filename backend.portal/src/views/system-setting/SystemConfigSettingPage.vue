<template>
	<div>
		<v-flex>
			<systemConfigSetting />
		</v-flex>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import systemConfigSetting from '@/components/system-setting/SystemConfigSetting.vue';

@Component({ components: { systemConfigSetting } })
export default class SystemConfigSettingPage extends Vue {}
</script>
<style lang="scss">
.app {
	margin-left: 1% !important;
}
</style>
