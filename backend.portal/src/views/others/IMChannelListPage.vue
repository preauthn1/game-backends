<template>

	<div>
		<v-flex>
			<IMChannelList ref="refIMChannelList"></IMChannelList>
		</v-flex>
	</div>

</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import IMChannelList from '@/components/system-setting/IMChannelList.vue';

@Component({
	components: {
		IMChannelList,
	},
})
export default class IMChannelListPage extends Vue {
	showNotificationError: boolean = false;
	errorMsg: string = '';

	mounted() {}
	confirmedError() {
		this.showNotificationError = false;
	}
}
</script>

<style scoped lang="css">
</style>
