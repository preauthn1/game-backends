<template>
	<div>
		<MemberList />
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import MemberList from '@/components/members-management/List.vue';
@Component({ components: { MemberList } })
export default class List extends Vue {}
</script>
