<template>
	<notifications group="updated" position="bottom right"></notifications>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Prop } from 'vue-property-decorator';

@Component
export default class NotificationInfo extends Vue {
	@Prop(Number) readonly max!: number;
	@Prop(String) readonly title!: string;
	@Prop(String) readonly text!: string;
}
</script>