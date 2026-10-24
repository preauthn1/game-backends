<template>
	<div>
		<v-flex>
			<EventList />
		</v-flex>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import EventList from '@/components/system-setting/events/EventList.vue';

@Component({ components: { EventList } })
export default class EventListPage extends Vue {}
</script>
<style lang="scss">
.app {
	margin-left: 1% !important;
}
</style>
