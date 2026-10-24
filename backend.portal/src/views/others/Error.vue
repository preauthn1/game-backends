<template>
	<v-app id="404">
		<v-container fluid fill-height>
			<v-layout align-center justify-center row>
				<div class="mr-3 hidden-sm-and-down">
					<img src="/static/500.svg" alt />
				</div>
				<div class="text-md-center">
					<h1>Oops！</h1>
					<h2 class="my-3 headline">抱歉，您没有访问此页面的权限</h2>
					<div>
						<v-btn color="primary" @click="goHome">返回首页</v-btn>
					</div>
				</div>
			</v-layout>
		</v-container>
	</v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import EventBus from '@/utilities/event-bus';

@Component
export default class Error extends Vue {
	renderComponent: boolean = true;
	goHome() {
		this.$router.push({ path: '/' });
	}
	mounted() {
		this.registerReloadComponent();
	}
	registerReloadComponent() {
		EventBus.$on(`reload-${this.$router.currentRoute.path}`, (info: any) => {
			this.renderComponent = false;
			this.$nextTick(() => {
				this.renderComponent = true;
			});
		});
	}
}
</script>

<style scoped lang="css">
h1 {
	font-size: 150px;
	line-height: 150px;
	font-weight: 700;
	color: #252932;
	text-shadow: rgba(61, 61, 61, 0.3) 1px 1px, rgba(61, 61, 61, 0.2) 2px 2px, rgba(61, 61, 61, 0.3) 3px 3px;
}
</style>
