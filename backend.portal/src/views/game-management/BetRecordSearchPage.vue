<template>
	<v-app>
		<BetRecordSearch />
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import BetRecordSearch from '@/components/game-management/BetRecordSearch.vue';
@Component({ components: { BetRecordSearch } })
export default class BetRecordSearchPage extends Vue {}
</script>