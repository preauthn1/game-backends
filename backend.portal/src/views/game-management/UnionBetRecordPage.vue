<template>
	<v-app>
		<UnionBetRecord />
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import UnionBetRecord from '@/components/game-management/UnionBetRecords.vue';
@Component({ components: { UnionBetRecord } })
export default class Record extends Vue {}
</script>