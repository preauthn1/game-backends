<template>
	<v-app>
		<List />
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import List from '@/components/game-management/ServersHealthSummary.vue';
@Component({ components: { List } })
export default class ServersHealthSummary extends Vue {}
</script>