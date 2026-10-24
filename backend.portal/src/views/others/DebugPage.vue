<template>
	<v-app>
		<PageComponent />
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import PageComponent from '@/components/Debug.vue';
@Component({ components: { PageComponent } })
export default class DebugPage extends Vue {}
</script>