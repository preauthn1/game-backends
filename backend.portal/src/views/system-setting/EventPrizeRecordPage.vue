<template>
	<div>
		<v-flex>
			<EventPrizeRecord />
		</v-flex>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import EventPrizeRecord from '@/components/system-setting/events/EventPrizeRecord.vue';

@Component({ components: { EventPrizeRecord } })
export default class EventPrizeRecordPage extends Vue {}
</script>
<style lang="scss">
.app {
	margin-left: 1% !important;
}
</style>
