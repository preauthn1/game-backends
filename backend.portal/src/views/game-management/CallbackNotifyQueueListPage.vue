<template>
	<v-app>
		<CallbackNotifyQueueList />
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import CallbackNotifyQueueList from '@/components/game-management/CallbackNotifyQueueList.vue';
@Component({ components: { CallbackNotifyQueueList } })
export default class CallbackNotifyQueueListPage extends Vue {}
</script>
