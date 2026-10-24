<template>
	<v-app>
		<LockRecord />
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import LockRecord from '@/components/members-management/LockRecord.vue';
@Component({ components: { LockRecord } })
export default class Lock extends Vue {}
</script>