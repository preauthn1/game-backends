<template>
	<div>
		<v-flex>
			<UserOperateRecord />
		</v-flex>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import UserOperateRecord from '@/components/system-logs/UserOperateRecord.vue';

@Component({ components: { UserOperateRecord } })
export default class UserOperateRecordPage extends Vue {}
</script>
<style lang="scss">
.app {
	margin-left: 1% !important;
}
</style>
