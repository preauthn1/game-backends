<template>
	<div v-show="$router.currentRoute.name !== 'error'">

		<div class="container hide-in-phone">
			<div class="scroll-btn right" @click="right">
				<v-icon>keyboard_arrow_right</v-icon>
			</div>

			<v-spacer id="items">
				<v-chip :class="{ current: item.routerPath === location.routerPath }"
					:id="generateElemenetID(item.routerPath)" v-for="item in breadcrumbs"
					:key="item.routerPath" close @click:close="close(item)" @click="click(item)">
					{{ $t(`Menu.${item.activator}`) }}
				</v-chip>
			</v-spacer>
			<div class="scroll-btn left" @click="left">
				<v-icon>keyboard_arrow_left</v-icon>
			</div>
		</div>

		<v-layout row class="align-center layout px-4  my-navigate-title app--page-header">
			<div class="page-header-left my-page-title" v-show="location.text">
				<!-- <v-chip class="ma-2" label text-color="white"> -->
				<v-icon left>mdi-near-me</v-icon>
				<!-- {{ location.text }} -->
				{{ $t(`Menu.${location.activator}`) }} <span style="color:#ff4646"> {{envName()}}</span>
				<!-- {{ location.routerPath }} -->
				<!-- </v-chip> -->
			</div>
			<v-spacer class="spacer"></v-spacer>
			<!-- <div class="page-header-right">
				<v-btn icon>
					<v-icon class="text--secondary" @click="refresh">refresh</v-icon>
				</v-btn>
			</div>-->
		</v-layout>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Prop, Watch } from 'vue-property-decorator';
import { getMenuText } from '@/router/menu';
import EventBus from '@/utilities/event-bus';
import { State, Action, Getter, namespace } from 'vuex-class';
import * as PermissionModel from '@/models/interfaces/commons/permission';
import Sortable from 'sortablejs';
const authModule = namespace('Auth');
@Component
export default class PageHeader extends Vue {
	location: any = {
		text: '',
		routerPath: '',
	};
	breadcrumbs: any[] = [];
	@authModule.State('apiPaths') apiPaths!: PermissionModel.INestedPermission;
	@authModule.State('allPermissions') allPermissions!: PermissionModel.IAllPermission[];
	@authModule.State('nestedPermissions') nestedPermissions!: PermissionModel.INestedPermission[];
	@authModule.State('defaultLangTimes') defaultLangTimes!: number;
	@Watch('$route.path', { immediate: true, deep: true })
	onRouteChange() {
		this.computeBreadcrumbs();
	}
	@Watch('defaultLangTimes')
	onDefaultLangChanged() {
		// localStorage.setItem('routerHistories', JSON.stringify([]));
		this.init();
	}

	generateElemenetID(routerPath: string) {
		// @ts-ignore
		return 'menu_' + routerPath.replaceAll('/', '_');
	}

	envName() {
		const env = process.env.VUE_APP_HOST;

		if (env && !(env === undefined)) {
			if (env === 'development') {
				return '(Dev環境)';
			} else if (env === 'local') {
				return '(Local環境)';
			} else if (env === 'qa') {
				return '(QA環境)';
			} else {
				return '';
			}
		} else {
			return '';
		}
	}

	computeBreadcrumbs() {
		const self = this;
		const herf = self.$router.currentRoute.path;
		// 暫時先保留
		// if (herf.split('/').length > 3) {
		// 	herf = `/${herf.split('/')[1]}/${herf.split('/')[2]}`;
		// }
		const path = getMenuText(this.nestedPermissions, herf, this.apiPaths, function changePathText(text: any) {
			self.location = text;
			const data = localStorage.getItem('routerHistories') || '[]';
			const routerArray: any[] = JSON.parse(data);
			const index = routerArray.findIndex((s) => s.routerPath === self.location.routerPath);
			// if (index > -1 && index < 10) {
			// 	// routerArray.splice(index, 1);
			// 	// routerArray.splice(0, 0, self.location);
			// } else if (index < 0) {
			// 	routerArray.splice(0, 0, self.location);
			// } else if (index > 9) {
			// 	routerArray.splice(index, 1);
			// 	routerArray.splice(0, 0, self.location);
			// }
			if (index < 0) {
				routerArray.splice(0, 0, self.location);
			}
			self.breadcrumbs = routerArray;
			localStorage.setItem('routerHistories', JSON.stringify(routerArray));
		});
	}

	async closeAllTabs() {
		const data = localStorage.getItem('routerHistories') || '[]';
		const routerArray: any[] = JSON.parse(data);

		if (routerArray.length === 1) {
			return;
		}

		routerArray.forEach((item) => {
			this.close(item);
		});

		setTimeout(() => {
			// 某種bug導致最後都會剩下2筆頁籤 (可能是async的因素) 因此再強制執行一次相同邏輯
			routerArray.forEach((item) => {
				this.close(item);
			});
		}, 250);
	}

	closeSinglePathByPath(targetPath: string) {
		const data = localStorage.getItem('routerHistories') || '[]';
		const routerArray: any[] = JSON.parse(data);

		if (routerArray.length === 1) {
			return;
		}

		let isTriggerAnyClose: boolean = false;

		routerArray.forEach((item) => {
			if (item.routerPath === targetPath) {
				this.close(item);
				isTriggerAnyClose = true;
			}
		});

		// 如果tab已經被關閉。則需要手動觸發此
		if (!isTriggerAnyClose) {
			const eventName = 'reload-' + targetPath;
			EventBus.$emit(eventName);
		}
	}

	close(item: any) {
		const data = localStorage.getItem('routerHistories') || '[]';
		const routerArray: any[] = JSON.parse(data);

		if (routerArray.length === 1) {
			return;
		}

		const index = routerArray.findIndex((s) => s.routerPath === item.routerPath);
		if (index >= 0) {
			routerArray.splice(index, 1);
		}
		if (item.routerPath === this.location.routerPath) {
			if (index === 0) {
				if (this.breadcrumbs[index + 1]) {
					this.$router.push({ path: this.breadcrumbs[index + 1].routerPath });
				} else {
					this.$router.push({ path: '' });
				}
			} else if (index === routerArray.length) {
				if (this.breadcrumbs[index - 1]) {
					this.$router.push({ path: this.breadcrumbs[index - 1].routerPath });
				} else {
					this.$router.push({ path: '' });
				}
			} else {
				this.$router.push({ path: this.breadcrumbs[index + 1].routerPath });
			}
		}

		this.breadcrumbs = routerArray;
		localStorage.setItem('routerHistories', JSON.stringify(routerArray));
		EventBus.$emit(`reload-${item.routerPath}`, null);
	}
	refresh() {
		EventBus.$emit(`reload-${this.$router.currentRoute.path}`, null);
	}
	click(item: any) {
		this.$router.push({ path: item.routerPath });
	}
	created() {
		setTimeout(() => {
			this.init();
		}, 150);
	}

	init() {
		this.computeBreadcrumbs();
		const el = document.getElementById('items');
		if (!el) {
			return;
		}
		const sortable = Sortable.create(el, {
			onUpdate: (evt) => {
				if (evt.oldIndex === undefined || evt.newIndex === undefined) {
					return;
				}
				const temp = this.breadcrumbs[evt.oldIndex];
				this.breadcrumbs.splice(evt.oldIndex, 1);
				this.breadcrumbs.splice(evt.newIndex, 0, temp);
				localStorage.setItem('routerHistories', JSON.stringify(this.breadcrumbs));
			},
		});

		// 註冊全域事件
		EventBus.$on('close_tab', (targetPath: string) => {
			this.closeSinglePathByPath(targetPath);
		});
	}

	left() {
		const el = document.getElementsByClassName('spacer');
		for (const item of el) {
			item.scrollLeft = item.scrollLeft - 50;
		}
	}
	right() {
		const el = document.getElementsByClassName('spacer');
		for (const item of el) {
			item.scrollLeft = item.scrollLeft + 50;
		}
	}
}
</script>
<style scoped lang="scss">
.disabled {
	color: grey;
	pointer-events: none;
	text-decoration: blink;
}
.spacer {
	background-color: grey;
	opacity: 0.8;
}
.spacer span {
	display: inline-block;
	position: relative;
	cursor: pointer;
	height: 26px;
	line-height: 26px;
	/* border: 1px solid #d8dce5; */
	/* color: #495060; */
	/* background-color: #303030; */

	padding: 0 8px;
	font-size: 12px;
	margin-left: 2px;
	margin-right: 3px;
	/* margin-top: 4px; */

	border-radius: 20% 20% 0% 0%;
	-webkit-border-radius: 20% 20% 0% 0%;
	-moz-border-radius: 20% 20% 0% 0%;
}
.spacer span:nth-child(n) {
	background-color: #555555;
	opacity: 0.9;
}
.current {
	background-color: #000 !important;
	opacity: 1 !important;
	color: #ffee75;
}
/*
.spacer span:nth-child(1) {
	background-color: #303030;
	opacity: 1;
} */

/* .spacer span:nth-child(5n + 1) {
	background-color: yellowgreen;
}
.spacer span:nth-child(5n + 2) {
	background-color: rgba(0, 255, 255, 0.212);
}
.spacer span:nth-child(5n-2) {
	background-color: rgba(255, 192, 203, 0.404);
}
.spacer span:nth-child(5n-1) {
	background-color: rgba(255, 0, 0, 0.582);
}
.spacer span:nth-child(5n) {
	background-color: rgb(129, 47, 122);
}
*/

.spacer {
	white-space: nowrap;
	position: absolute;
	overflow: hidden;
	width: 97.5%;
	top: 0px;
	left: 25px;
	right: 0px;
	// z-index: 20;
	&::-webkit-scrollbar {
		height: 0px;
	}
}

.scroll-btn {
	position: absolute;
	top: 0px;
	z-index: 30;
	background-color: #303030 !important;
	height: 28px !important;
	width: 18px !important;
	// border-color: rgb(107, 71, 17) !important;
	&.left {
		left: 0;
	}
	&.right {
		right: 0;
	}
}
/*
.spacer span:nth-child(n + 11) {
	display: none;
}
*/
</style>
