<template>
	<v-overlay :absolute="true" :value="true" style="z-index: 999;">
		<notifications group="confirm" position="top center" :max="max">
			<template slot="body" slot-scope="props">
				<div class="vue-notification-template vue-notification warn" @click="canceledAction"
					style="z-index: 999;">
					<div class="notification-title">
						<v-icon left>notification_important</v-icon>
						{{ title }}
					</div>
					<div class="notification-content">{{ text }}</div>
					<v-btn class="ma-2" outlined color="success" @click.stop="confirmedAction">
						<v-icon left>mdi-check-circle</v-icon>
						{{ $t('Button.__submit') }}
					</v-btn>
					<v-btn class="ma-2" outlined color="success" @click.stop="canceledAction">
						<v-icon left>mdi-cancel</v-icon>
						{{ $t('Button.__cancel') }}
					</v-btn>
				</div>
			</template>
		</notifications>
	</v-overlay>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Prop } from 'vue-property-decorator';

@Component
export default class NotificationWarning extends Vue {
	@Prop(Number) readonly max!: number;
	@Prop(String) readonly title!: string;
	@Prop(String) readonly text!: string;
	@Prop(Function) readonly confirmedAction!: () => {};
	@Prop(Function) readonly canceledAction!: () => {};
}
</script>
<style lang="scss" scoped>
button {
	margin-top: 10px;
}
</style>
