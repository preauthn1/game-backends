<template>
	<v-app>
		<AGGJPAmount />
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import AGGJPAmount from '@/components/game-management/AGGJPAmount.vue';
@Component({ components: { AGGJPAmount } })
export default class AGGJPAmountPage extends Vue {}
</script>