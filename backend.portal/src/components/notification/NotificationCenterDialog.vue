<template>
	<notifications group="success-dailog" position="top center">

		<template slot="body" slot-scope="props">
			<div class="vue-notification-template vue-notification"
				style="background-color: #c4e5be !important; ">
				<div class="notification-title f16">
					<v-icon class="titleIcon" color="red lighten-2" left>mdi-check-circle</v-icon>
					Prompt
				</div>
				<div class="notification-content f14" style="padding: 10px 5px;">{{ text }}</div>

			</div>
		</template>

	</notifications>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Prop } from 'vue-property-decorator';

@Component
export default class NotificationInfo extends Vue {
	@Prop(Number) readonly max!: number;
	@Prop(String) readonly title!: string;
	@Prop(String) readonly text!: string;
}
</script>