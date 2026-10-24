<template>
	<div>
		<v-flex>
			<EventDetail />
		</v-flex>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import EventDetail from '@/components/system-setting/events/EventDetail.vue';

@Component({ components: { EventDetail } })
export default class EventDetailPage extends Vue {}
</script>
<style lang="scss">
.app {
	margin-left: 1% !important;
}
</style>
