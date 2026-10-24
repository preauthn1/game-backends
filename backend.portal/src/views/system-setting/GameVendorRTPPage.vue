<template>
	<div>
		<v-flex>
			<GameVendorRTP />
		</v-flex>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import GameVendorRTP from '@/components/system-setting/events/GameVendorRTP.vue';

@Component({ components: { GameVendorRTP } })
export default class GameVendorRTPPage extends Vue {}
</script>
<style lang="scss">
.app {
	margin-left: 1% !important;
}
</style>
