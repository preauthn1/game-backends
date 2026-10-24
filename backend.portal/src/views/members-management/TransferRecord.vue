<template>
	<v-app>
		<List />
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import List from '@/components/members-management/TransferRecord.vue';
@Component({ components: { List } })
export default class TransferRecord extends Vue {}
</script>