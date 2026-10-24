<template>
	<v-app>
		<GameManagementList />
		<PerGameLogDetailPopup />
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import GameManagementList from '@/components/game-management/RecordList.vue';
import PerGameLogDetailPopup from '@/components/game-management/PerGameLogDetailPopup.vue';
@Component({ components: { GameManagementList, PerGameLogDetailPopup } })
export default class Record extends Vue {}
</script>