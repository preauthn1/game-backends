<template>
	<v-app>
		<MoneyRecord />
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import MoneyRecord from '@/components/members-management/MoneyRecord.vue';
@Component({ components: { MoneyRecord } })
export default class Money extends Vue {}
</script>