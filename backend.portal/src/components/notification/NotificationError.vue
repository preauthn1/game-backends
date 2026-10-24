<template>
	<v-overlay :absolute="false" :value="true">
		<notifications group="error" position="top center" :max="max" class="error-notify-dialog">
			<template slot="body" slot-scope="props">
				<div class="vue-notification-template vue-notification error" @click.stop="confirmedError">
					<div class="notification-title">
						<v-icon left small>error</v-icon>
						<span style="font-size: 18px; vertical-align: middle; margin-right: 10px"> Error </span>
					</div>
					<div class="error-msg-title mt-3 mb-3">
						{{ title }}
					</div>
					<div class="notification-content">{{ text }}</div>

					<v-btn class="ma-2" style="margin-right: 8px !important" outlined color="yellow darken-2" @click.stop="copyText">
						<v-icon left>file_copy</v-icon>
						{{ $t('Button.__copyToClipboard') }}
					</v-btn>

					<v-btn class="ma-2" outlined color="secondary" @click.stop="confirmedError">
						<v-icon left>mdi-check-circle</v-icon>
						{{ $t('Button.__close') }}
					</v-btn>
				</div>
			</template>
		</notifications>
	</v-overlay>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Prop } from 'vue-property-decorator';
import { commonMethodMixin } from '@/utilities/common-methods';

@Component({ mixins: [commonMethodMixin] })
export default class NotificationError extends Vue {
	@Prop(Number) readonly max!: number;
	@Prop(String) readonly title!: string;
	@Prop(String) readonly text!: string;
	@Prop(Function) readonly confirmedError!: () => {};

	copyText() {
		const vm: any = this;
		vm.copyTextToClipboard(this.title);
		this.confirmedError();
		return '';
	}
}
</script>

<style lang="scss" scoped>
.error-msg-title {
	font-size: 15px !important;
	font-weight: 600;
	text-align: left;
	padding-left: 3%;
	padding-right: 3%;
	margin-top: 10px;
	margin-bottom: 10px;
}

.v-overlay {
	&.v-overlay--active {
		z-index: 999 !important;
	}
}
button {
	margin-top: 10px;
}
</style>
