<template>
	<div>
		<v-flex>
			<EventDrawPrize />
		</v-flex>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import EventDrawPrize from '@/components/system-setting/events/EventDrawPrize.vue';

@Component({ components: { EventDrawPrize } })
export default class EventDrawPrizePage extends Vue {}
</script>
<style lang="scss">
.app {
	margin-left: 1% !important;
}
</style>
