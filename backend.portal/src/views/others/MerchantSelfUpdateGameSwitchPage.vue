<template>
	<div>
		<v-flex>
			<LobbyVisiableGameSwitch />
		</v-flex>
	</div>
</template>



<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import LobbyVisiableGameSwitch from '@/components/others/LobbyVisiableGameSwitch.vue';

@Component({ components: { LobbyVisiableGameSwitch } })
export default class MerchantSelfUpdateGameSwitchPage extends Vue {}
</script>

<style lang="scss">
.app {
	margin-left: 1% !important;
}
</style>
