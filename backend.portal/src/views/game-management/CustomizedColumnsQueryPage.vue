<template>
	<v-app>
		<PageComponent />
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import PageComponent from '@/components/game-management/CustomizedColumnsQueryListSearch.vue';
@Component({ components: { PageComponent } })
export default class CustomizedColumnsQueryPage extends Vue {}
</script>