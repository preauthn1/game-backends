<template>
	<v-overlay :absolute="true" :value="true">
		<notifications group="warningV2" position="top center" :max="max" style="max-width: 450px; width: 100%">
			<template slot="body" slot-scope="props">
				<div class="vue-notification-template vue-notification" @click="canceledAction" style="background-color: #ffca6b !important">
					<div class="notification-title f14">
						<v-icon class="titleIcon" color="red lighten-2" left>notification_important</v-icon>
						{{ title }}
					</div>
					<div class="notification-content f12" style="padding: 10px 5px">{{ text }}</div>
					<v-btn class="ma-2 mr-2 myBtn" outlined color="red" @click.stop="confirmedAction">
						<v-icon left>mdi-check-circle</v-icon>
						{{ $t('Button.__submit') }}
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
export default class NotificationWarningV2 extends Vue {
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

.myBtn {
	color: #c91c1c;
}

.titleIcon {
	color: #c91c1c;
}
</style>
