<template>
	<div>
		<v-flex>
			<gameListComp />
		</v-flex>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import gameListComp from '@/components/system-setting/GameList.vue';

@Component({ components: { gameListComp } })
export default class GameList extends Vue {}
</script>
<style lang="scss">
.app {
	margin-left: 1% !important;
}
</style>
