<template>
	<div>
		<v-flex>
			<MerchantCustomizedSettingList />
		</v-flex>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import MerchantCustomizedSettingList from '@/components/system-setting/MerchantCustomizedSettingList.vue';

@Component({ components: { MerchantCustomizedSettingList } })
export default class MerchantCustomizedSettingListPage extends Vue {}
</script>
<style lang="scss">
.app {
	margin-left: 1% !important;
}
</style>
