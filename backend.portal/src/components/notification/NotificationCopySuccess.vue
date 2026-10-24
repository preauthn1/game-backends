<template>
	<notifications group="copy-success" position="bottom right">
		<template slot="body">
			<div class="vue-notification-template vue-notification auto-hidden-copy">
				<div class="notification-title">
					<v-icon left>error</v-icon>
					{{ $t('Message.__copySuccessful') }}
				</div>
			</div>
		</template>
	</notifications>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Prop } from 'vue-property-decorator';

@Component
export default class NotificationAutoHiddenError extends Vue {
	@Prop(Number) readonly max!: number;
	@Prop(String) readonly title!: string;
	@Prop(String) readonly text!: string;
}
</script>

