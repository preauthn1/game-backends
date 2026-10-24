<template>
	<v-app class="" style="background: #333;">
		<v-content>
			<v-container fluid fill-height>

				<MerchantCompare ref="MerchantCompare"></MerchantCompare>

			</v-container>
		</v-content>
		<NotificationError v-show="showNotificationError"
			:title="`${$t('Message.__error')} : ${errorMsg}`" :max="1"
			:confirmed-error="confirmedError" />
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import MerchantCompare from '@/components/system-setting/MerchantCompare.vue';
import NotificationError from '@/components/notification/NotificationError.vue';

@Component({
	components: {
		NotificationError,
		MerchantCompare,
	},
})
export default class MerchantComparePage extends Vue {
	showNotificationError: boolean = false;
	errorMsg: string = '';

	mounted() {}
	confirmedError() {
		this.showNotificationError = false;
	}
}
</script>

<style scoped lang="css">
.split-bg {
	height: 50%;
	width: 100%;
	position: absolute;
	top: 0;
	left: 0;
	content: '';
	z-index: 0;
}
</style>
