<template>
	<v-app>
		<BetRecord />
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import BetRecord from '@/components/members-management/BetRecord.vue';
@Component({ components: { BetRecord } })
export default class Bet extends Vue {}
</script>