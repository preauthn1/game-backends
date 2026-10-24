<template>

	<div>
		<v-flex>
			<BOAnnouncementManagement ref="refBOAnnouncementManagement"></BOAnnouncementManagement>
		</v-flex>
	</div>

</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import BOAnnouncementManagement from '@/components/others/BOAnnouncementManagement.vue';

@Component({
	components: {
		BOAnnouncementManagement,
	},
})
export default class BOAnnouncementPage extends Vue {
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
