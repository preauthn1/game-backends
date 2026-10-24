<template>
	<div>
		<v-flex>
			<RoleList />
		</v-flex>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import RoleList from '@/components/system-setting/RoleList.vue';

@Component({ components: { RoleList } })
export default class Roles extends Vue {}
</script>
<style lang="scss">
.app {
	margin-left: 1% !important;
}
</style>
