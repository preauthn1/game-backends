<template>
	<div>
		<v-flex>
			<Users />
		</v-flex>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import Users from '@/components/system-setting/Users.vue';

@Component({ components: { Users } })
export default class Accounts extends Vue {}
</script>
<style lang="scss">
.app {
	margin-left: 1% !important;
}
</style>
