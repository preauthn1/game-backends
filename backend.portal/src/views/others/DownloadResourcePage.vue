<template>

	<div>
		<v-flex>
			<DownloadResource></DownloadResource>
		</v-flex>
	</div>

</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import DownloadResource from '@/components/others/DownloadResource.vue';

@Component({
	components: {
		DownloadResource,
	},
})
export default class DownloadResourcePage extends Vue {
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
